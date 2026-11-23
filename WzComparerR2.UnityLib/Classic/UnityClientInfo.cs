using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using WzComparerR2.UnityLib.Unity;

namespace WzComparerR2.UnityLib.Classic
{
    /// <summary>
    /// Unity 客戶端的基本資訊：名稱（PlayerSettings.productName）、版本
    /// （PlayerSettings.bundleVersion）與引擎版本（AssetBundle 的 UnityFS header）。
    /// <para>
    /// 資料來源依序為：<c>&lt;Client&gt;_Data/globalgamemanagers</c> 的 PlayerSettings、
    /// <c>&lt;Client&gt;_Data/app.info</c>、以及 <c>StreamingAssets/aa/settings.json</c>
    /// 內 Addressables 的 <c>catalog_&lt;version&gt;.hash</c>。任一來源缺少時對應欄位為 null。
    /// </para>
    /// </summary>
    public sealed class UnityClientInfo
    {
        private static readonly Regex CatalogVersionPattern = new Regex(
            @"catalog_([0-9][0-9A-Za-z._-]*)\.hash", RegexOptions.CultureInvariant);

        private static readonly Regex VersionTextPattern = new Regex(
            @"^[0-9]+(\.[0-9]+)+$", RegexOptions.CultureInvariant);

        private UnityClientInfo()
        {
        }

        /// <summary>開發商名稱（PlayerSettings.companyName）。</summary>
        public string CompanyName { get; private set; }

        /// <summary>客戶端名稱（PlayerSettings.productName）。</summary>
        public string ProductName { get; private set; }

        /// <summary>客戶端版本（PlayerSettings.bundleVersion）。</summary>
        public string Version { get; private set; }

        /// <summary>
        /// 引擎版本（AssetBundle 的 unityRevision，例如 <c>6000.3.11f1</c>）。
        /// UnityFS header 的第一個字串只是格式佔位（<c>5.x.x</c>），真正版本在第二個字串。
        /// </summary>
        public string EngineVersion { get; private set; }

        /// <summary>解析到的 <c>&lt;Client&gt;_Data</c> 目錄；找不到時為 null。</summary>
        public string DataFolder { get; private set; }

        /// <summary>「名稱 版本」的顯示字串；兩者都沒有時為 null。</summary>
        public string DisplayName
        {
            get
            {
                if (string.IsNullOrEmpty(this.ProductName))
                {
                    return string.IsNullOrEmpty(this.Version) ? null : this.Version;
                }
                return string.IsNullOrEmpty(this.Version)
                    ? this.ProductName
                    : this.ProductName + " " + this.Version;
            }
        }

        /// <summary>
        /// 從 bundle 目錄（<c>StreamingAssets/aa/StandaloneWindows64</c>）解析客戶端資訊。
        /// 找不到的項目留 null，不會擲出例外。
        /// </summary>
        /// <param name="bundleDirectory">bundle 所在目錄（用來往上找 <c>*_Data</c> 與 <c>aa/settings.json</c>）。</param>
        /// <param name="engineVersion">引擎版本字串（<see cref="UnityFsBundle.UnityRevision"/>，可為 null）。</param>
        public static UnityClientInfo Resolve(string bundleDirectory, string engineVersion)
        {
            UnityClientInfo info = new UnityClientInfo();
            info.EngineVersion = string.IsNullOrEmpty(engineVersion) ? null : engineVersion;

            try
            {
                string dataFolder = FindDataFolder(bundleDirectory);
                info.DataFolder = dataFolder;
                if (dataFolder != null)
                {
                    info.ApplyPlayerSettings(Path.Combine(dataFolder, "globalgamemanagers"));
                    if (string.IsNullOrEmpty(info.ProductName))
                    {
                        info.ApplyAppInfo(Path.Combine(dataFolder, "app.info"));
                    }
                }

                if (string.IsNullOrEmpty(info.Version))
                {
                    // Addressables 的 catalog 檔名帶有 Application.version，是未經猜測的字面值。
                    info.Version = ReadAddressablesVersion(bundleDirectory);
                }
                if (string.IsNullOrEmpty(info.Version) && dataFolder != null)
                {
                    info.ApplyPlayerSettingsVersion(Path.Combine(dataFolder, "globalgamemanagers"));
                }
            }
            catch (Exception)
            {
                // 資訊僅供顯示，任何解析失敗都不影響資料集載入。
            }

            return info;
        }

        /// <summary>開啟一個 bundle 檔讀取引擎版本後解析客戶端資訊。</summary>
        public static UnityClientInfo ResolveFromBundle(string bundlePath)
        {
            string engineVersion = null;
            string directory = null;
            try
            {
                if (!string.IsNullOrEmpty(bundlePath) && File.Exists(bundlePath))
                {
                    directory = Path.GetDirectoryName(bundlePath);
                    using (UnityFsBundle bundle = UnityFsBundle.Open(bundlePath))
                    {
                        engineVersion = bundle.UnityRevision;
                    }
                }
            }
            catch (Exception)
            {
            }
            return Resolve(directory, engineVersion);
        }

        /// <summary>從 <c>*_Data</c> 目錄往上找 bundle 目錄所屬的客戶端資料夾。</summary>
        private static string FindDataFolder(string bundleDirectory)
        {
            if (string.IsNullOrEmpty(bundleDirectory))
            {
                return null;
            }
            string current = bundleDirectory;
            for (int i = 0; i < 5 && !string.IsNullOrEmpty(current); i++)
            {
                if (File.Exists(Path.Combine(current, "globalgamemanagers"))
                    || File.Exists(Path.Combine(current, "app.info")))
                {
                    return current;
                }
                DirectoryInfo parent = Directory.GetParent(current);
                current = parent?.FullName;
                if (current != null && current.EndsWith("_Data", StringComparison.OrdinalIgnoreCase))
                {
                    if (File.Exists(Path.Combine(current, "globalgamemanagers"))
                        || File.Exists(Path.Combine(current, "app.info")))
                    {
                        return current;
                    }
                }
            }
            return null;
        }

        /// <summary>讀 PlayerSettings（class 129）取 companyName / productName / bundleVersion。</summary>
        private void ApplyPlayerSettings(string path)
        {
            if (!File.Exists(path))
            {
                return;
            }

            byte[] data = File.ReadAllBytes(path);
            UnitySerializedFile file = UnitySerializedFile.Parse(data);
            List<string> strings = null;
            foreach (UnityObjectInfo obj in file.Objects)
            {
                if (obj.ClassId != UnityClassIds.PlayerSettings)
                {
                    continue;
                }
                long start = file.DataOffset + obj.ByteStart;
                if (start < 0 || start + obj.ByteSize > data.Length)
                {
                    continue;
                }
                strings = ReadAlignedStrings(data, (int)start, (int)obj.ByteSize);
                break;
            }
            if (strings == null || strings.Count == 0)
            {
                return;
            }

            // PlayerSettings 序列化的前兩個字串固定是 companyName、productName。
            if (strings.Count >= 2)
            {
                this.CompanyName = strings[0];
                this.ProductName = strings[1];
            }
            else
            {
                this.ProductName = strings[0];
            }
        }

        private void ApplyPlayerSettingsVersion(string path)
        {
            if (!File.Exists(path))
            {
                return;
            }
            try
            {
                byte[] data = File.ReadAllBytes(path);
                UnitySerializedFile file = UnitySerializedFile.Parse(data);
                foreach (UnityObjectInfo obj in file.Objects)
                {
                    if (obj.ClassId != UnityClassIds.PlayerSettings)
                    {
                        continue;
                    }
                    long start = file.DataOffset + obj.ByteStart;
                    if (start < 0 || start + obj.ByteSize > data.Length)
                    {
                        continue;
                    }
                    ApplyVersionFromStrings(
                        ReadAlignedStrings(data, (int)start, (int)obj.ByteSize), 0);
                    break;
                }
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// bundleVersion 是 PlayerSettings 內一組版本字串中的最後一個（前面是
        /// iPhone/Android 等平台各自的版本欄位），因此取「最完整」的版本字串。
        /// </summary>
        private void ApplyVersionFromStrings(List<string> strings, int skip)
        {
            if (!string.IsNullOrEmpty(this.Version))
            {
                return;
            }
            string best = null;
            for (int i = skip; i < strings.Count; i++)
            {
                string text = strings[i];
                if (string.IsNullOrEmpty(text) || !VersionTextPattern.IsMatch(text))
                {
                    continue;
                }
                if (best == null || CountSegments(text) >= CountSegments(best))
                {
                    best = text;
                }
            }
            this.Version = best;
        }

        private static int CountSegments(string version)
        {
            int count = 1;
            foreach (char c in version)
            {
                if (c == '.')
                {
                    count++;
                }
            }
            return count;
        }

        /// <summary>app.info：第一行公司名稱、第二行產品名稱（UTF-8）。</summary>
        private void ApplyAppInfo(string path)
        {
            if (!File.Exists(path))
            {
                return;
            }
            string[] lines = File.ReadAllLines(path, Encoding.UTF8);
            if (lines.Length >= 1 && this.CompanyName == null)
            {
                this.CompanyName = lines[0].Trim();
            }
            if (lines.Length >= 2 && this.ProductName == null)
            {
                this.ProductName = lines[1].Trim();
            }
        }

        /// <summary>
        /// Addressables 的遠端 catalog 檔名是 <c>catalog_&lt;Application.version&gt;.hash</c>，
        /// 因此可由 <c>aa/settings.json</c> 取得客戶端版本。
        /// </summary>
        private static string ReadAddressablesVersion(string bundleDirectory)
        {
            if (string.IsNullOrEmpty(bundleDirectory))
            {
                return null;
            }
            string settings = Path.Combine(bundleDirectory, "..", "settings.json");
            if (!File.Exists(settings))
            {
                settings = Path.Combine(bundleDirectory, "settings.json");
            }
            if (!File.Exists(settings))
            {
                return null;
            }

            string text = File.ReadAllText(settings, Encoding.UTF8);
            Match match = CatalogVersionPattern.Match(text);
            return match.Success ? match.Groups[1].Value : null;
        }

        /// <summary>讀出物件內所有「u32 長度 + 內容 + 4 bytes 對齊」的可列印字串。</summary>
        private static List<string> ReadAlignedStrings(byte[] data, int offset, int size)
        {
            List<string> strings = new List<string>();
            int end = offset + size;
            for (int pos = offset + 8; pos + 5 <= end; pos++)
            {
                string text;
                int next;
                if (TryReadString(data, pos, end, out text, out next) && text.Length > 0)
                {
                    strings.Add(text);
                    pos = next - 1;
                }
            }
            return strings;
        }

        /// <summary>嘗試在 <paramref name="pos"/> 讀一個長度前綴字串；成功時 <paramref name="next"/> 為對齊後的位置。</summary>
        private static bool TryReadString(byte[] data, int pos, int end, out string text, out int next)
        {
            text = null;
            next = pos;
            int length = BitConverter.ToInt32(data, pos);
            if (length <= 0 || length > 512 || pos + 4 + length > end)
            {
                return false;
            }

            for (int i = 0; i < length; i++)
            {
                byte b = data[pos + 4 + i];
                if (b < 0x20 && b != '\r' && b != '\n' && b != '\t')
                {
                    return false;
                }
            }

            try
            {
                text = Utf8Strict.GetString(data, pos + 4, length);
            }
            catch (ArgumentException)
            {
                return false;
            }

            next = (pos + 4 + length + 3) & ~3;
            return true;
        }

        private static readonly UTF8Encoding Utf8Strict = new UTF8Encoding(false, true);
    }
}
