using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using WzComparerR2.UnityLib.Images;
using WzComparerR2.UnityLib.Unity;

namespace WzComparerR2.UnityLib.Classic
{
    /// <summary>catalog 內一個 bundle 的識別資料。</summary>
    public sealed class ClassicBundleMeta
    {
        public ClassicBundleMeta(string name, long size, long lastWriteTicks)
        {
            this.Name = name;
            this.Size = size;
            this.LastWriteTicks = lastWriteTicks;
        }

        /// <summary>bundle 檔名（去副檔名）。</summary>
        public string Name { get; private set; }

        public long Size { get; private set; }

        public long LastWriteTicks { get; private set; }
    }

    /// <summary>索引目錄的下一層。</summary>
    public sealed class ClassicIndexChild
    {
        internal ClassicIndexChild(string name, string path, bool isImage, int imageCount)
        {
            this.Name = name;
            this.Path = path;
            this.IsImage = isImage;
            this.ImageCount = imageCount;
        }

        public string Name { get; private set; }
        public string Path { get; private set; }
        public bool IsImage { get; private set; }
        public int ImageCount { get; private set; }
    }

    /// <summary>
    /// 資產索引：bundle → 邏輯路徑 → 物件位置。
    /// <para>
    /// 路徑協定：container 路徑
    /// <c>Assets/WzAssets/Json/Character/00002000.wzjson</c> → 索引鍵
    /// <c>Character/00002000</c>（去前綴、去副檔名）。樹上前幾段是「目錄」，
    /// 命中索引鍵的那個節點是「img」，img 之後的段落由 WZJS 樹導航
    /// （如 <c>Character/00020116/angry/0/face</c>）。
    /// </para>
    /// <para>
    /// 索引在 <see cref="Build"/> 時重建（與參考實作同樣掃描每個 bundle 的
    /// AssetBundle container）；磁碟快取尚未實作。
    /// </para>
    /// </summary>
    public sealed class ClassicCatalog
    {
        public const int CurrentVersion = 7;

        private const string JsonPrefix = "assets/wzassets/json/";
        private const string SoundPrefix = "assets/wzdata/sound/";
        private static readonly string[] SoundSuffixes = new[] { ".mp3", ".wav" };

        private ClassicCatalog(string rootPath)
        {
            this.RootPath = rootPath;
            this.Bundles = new List<ClassicBundleMeta>();
            this.Entries = new SortedDictionary<string, UnityEntryLocation>(StringComparer.Ordinal);
            this.Image = new UnityImageIndex();
        }

        public string RootPath { get; private set; }

        public List<ClassicBundleMeta> Bundles { get; private set; }

        /// <summary>邏輯路徑（如 <c>Character/00002000</c>）→ 物件位置。</summary>
        public SortedDictionary<string, UnityEntryLocation> Entries { get; private set; }

        public UnityImageIndex Image { get; private set; }

        /// <summary>掃描目錄下所有 .bundle，用每個 AssetBundle 物件的 m_Container 建索引。</summary>
        public static ClassicCatalog Build(string rootDirectory)
        {
            if (rootDirectory == null)
            {
                throw new ArgumentNullException(nameof(rootDirectory));
            }
            if (!Directory.Exists(rootDirectory))
            {
                throw new DirectoryNotFoundException("資料集目錄不存在: " + rootDirectory);
            }

            List<string> files = new List<string>();
            foreach (string path in Directory.GetFiles(rootDirectory))
            {
                if (string.Equals(Path.GetExtension(path), ".bundle", StringComparison.OrdinalIgnoreCase))
                {
                    files.Add(path);
                }
            }
            files.Sort(StringComparer.Ordinal);

            ClassicCatalog catalog = new ClassicCatalog(rootDirectory);
            foreach (string path in files)
            {
                FileInfo info = new FileInfo(path);
                catalog.Bundles.Add(new ClassicBundleMeta(
                    Path.GetFileNameWithoutExtension(path), info.Length, info.LastWriteTimeUtc.Ticks));
            }

            for (int bundleIndex = 0; bundleIndex < files.Count; bundleIndex++)
            {
                using (UnityFsBundle bundle = UnityFsBundle.Open(files[bundleIndex]))
                {
                    catalog.IndexBundle(bundle, bundleIndex, catalog.Bundles[bundleIndex].Name);
                }
            }
            return catalog;
        }

        /// <summary>頂層（或某目錄節點下）的下一層。</summary>
        public List<ClassicIndexChild> ListIndexChildren(string prefix)
        {
            SortedDictionary<string, ClassicIndexChild> output =
                new SortedDictionary<string, ClassicIndexChild>(StringComparer.Ordinal);
            string pfx = string.IsNullOrEmpty(prefix) ? string.Empty : prefix + "/";
            foreach (string key in this.Entries.Keys)
            {
                if (!key.StartsWith(pfx, StringComparison.Ordinal))
                {
                    continue;
                }
                string rest = key.Substring(pfx.Length);
                int slash = rest.IndexOf('/');
                string segment = slash < 0 ? rest : rest.Substring(0, slash);
                string childPath = pfx + segment;

                ClassicIndexChild child;
                if (!output.TryGetValue(childPath, out child))
                {
                    child = new ClassicIndexChild(segment, childPath, false, 0);
                    output[childPath] = child;
                }
                bool isImage = slash < 0;
                output[childPath] = new ClassicIndexChild(
                    segment, childPath, child.IsImage || isImage, child.ImageCount + 1);
            }
            List<ClassicIndexChild> result = new List<ClassicIndexChild>(output.Values);
            result.Sort((a, b) => CompareNodeNames(a.Name, b.Name));
            return result;
        }

        /// <summary>把 path 拆成 (img_key, img 內部路徑)；找不到任何 img 前綴回 false。</summary>
        public bool TrySplitImagePrefix(string path, out string imageKey, out string inner)
        {
            imageKey = null;
            inner = null;
            if (path == null)
            {
                return false;
            }
            string current = path;
            while (true)
            {
                if (this.Entries.ContainsKey(current))
                {
                    imageKey = current;
                    string rest = path.Substring(current.Length);
                    if (rest.StartsWith("/", StringComparison.Ordinal))
                    {
                        rest = rest.Substring(1);
                    }
                    inner = rest;
                    return true;
                }
                int slash = current.LastIndexOf('/');
                if (slash < 0)
                {
                    return false;
                }
                current = current.Substring(0, slash);
            }
        }

        /// <summary>
        /// 節點名比較：純數字名依數值排最前，其餘不分大小寫依字母。
        /// </summary>
        public static int CompareNodeNames(string left, string right)
        {
            long leftNumber;
            long rightNumber;
            bool leftIsNumber = TryParseNumericName(left, out leftNumber);
            bool rightIsNumber = TryParseNumericName(right, out rightNumber);
            if (leftIsNumber && rightIsNumber)
            {
                int compare = leftNumber.CompareTo(rightNumber);
                return compare != 0 ? compare : string.CompareOrdinal(left, right);
            }
            if (leftIsNumber)
            {
                return -1;
            }
            if (rightIsNumber)
            {
                return 1;
            }
            int insensitive = string.Compare(left, right, StringComparison.OrdinalIgnoreCase);
            return insensitive != 0 ? insensitive : string.CompareOrdinal(left, right);
        }

        /// <summary>把 container 路徑正規化成索引鍵；不屬於 json / 聲音資產的回 null。</summary>
        public static string NormalizeContainerPath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }
            string lower = path.ToLowerInvariant();
            if (lower.StartsWith(JsonPrefix, StringComparison.Ordinal))
            {
                string stripped = path.Substring(JsonPrefix.Length);
                if (lower.EndsWith(".wzjson", StringComparison.Ordinal))
                {
                    return stripped.Substring(0, stripped.Length - ".wzjson".Length);
                }
                if (lower.EndsWith(".json", StringComparison.Ordinal))
                {
                    return stripped.Substring(0, stripped.Length - ".json".Length);
                }
                return null;
            }
            if (lower.StartsWith(SoundPrefix, StringComparison.Ordinal))
            {
                foreach (string suffix in SoundSuffixes)
                {
                    if (lower.EndsWith(suffix, StringComparison.Ordinal))
                    {
                        string stripped = path.Substring(SoundPrefix.Length);
                        return "Sound/" + stripped.Substring(0, stripped.Length - suffix.Length);
                    }
                }
            }
            return null;
        }

        private static bool TryParseNumericName(string name, out long value)
        {
            value = 0;
            if (string.IsNullOrEmpty(name))
            {
                return false;
            }
            for (int i = 0; i < name.Length; i++)
            {
                if (name[i] < '0' || name[i] > '9')
                {
                    return false;
                }
            }
            return long.TryParse(name, NumberStyles.None, CultureInfo.InvariantCulture, out value);
        }

        private void IndexBundle(UnityFsBundle bundle, int bundleIndex, string bundleStem)
        {
            IReadOnlyList<UnityFsBundle.BundleNode> nodes = bundle.Nodes;
            for (int nodeIndex = 0; nodeIndex < nodes.Count; nodeIndex++)
            {
                if (!nodes[nodeIndex].IsSerializedFile)
                {
                    continue;
                }

                byte[] head;
                int metadataLength;
                try
                {
                    head = bundle.ReadNodeRange(nodeIndex, 0, UnitySerializedFile.HeaderSize);
                    metadataLength = UnitySerializedFile.GetMetadataLength(head);
                }
                catch (Exception ex) when (ex is UnityException || ex is ArgumentException || ex is OverflowException)
                {
                    continue;
                }

                UnitySerializedFile serializedFile;
                try
                {
                    head = bundle.ReadNodeRange(nodeIndex, 0, metadataLength);
                    serializedFile = UnitySerializedFile.Parse(head);
                }
                catch (Exception ex) when (ex is UnityException || ex is ArgumentException || ex is OverflowException)
                {
                    continue;
                }

                Dictionary<long, UnityObjectInfo> objectsByPathId =
                    new Dictionary<long, UnityObjectInfo>(serializedFile.Objects.Count);
                foreach (UnityObjectInfo obj in serializedFile.Objects)
                {
                    objectsByPathId[obj.PathId] = obj;
                }

                foreach (UnityObjectInfo assetBundleObject in serializedFile.ObjectsByClass(UnityClassIds.AssetBundle))
                {
                    long objectOffset = serializedFile.DataOffset + assetBundleObject.ByteStart;
                    if (objectOffset < serializedFile.DataOffset || assetBundleObject.ByteSize > int.MaxValue)
                    {
                        throw new UnityException(string.Format(
                            "AssetBundle 物件 offset 異常: bundle={0}, node={1}", bundleStem, nodeIndex));
                    }

                    UnityAssetBundleInfo assetBundle;
                    try
                    {
                        byte[] data = bundle.ReadNodeRange(
                            nodeIndex, objectOffset, (int)assetBundleObject.ByteSize);
                        assetBundle = UnityObjectParsers.ParseAssetBundle(data, serializedFile.IsLittleEndian);
                    }
                    catch (UnityException ex)
                    {
                        throw new UnityException(string.Format(
                            "AssetBundle 物件解析失敗: bundle={0}, node={1}: {2}",
                            bundleStem, nodeIndex, ex.Message), ex);
                    }

                    this.Image.CollectFromContainer(
                        bundle, bundleIndex, bundleStem, nodeIndex,
                        serializedFile, assetBundle.Container, objectsByPathId);

                    foreach (UnityContainerEntry containerEntry in assetBundle.Container)
                    {
                        string key = NormalizeContainerPath(containerEntry.Path);
                        if (key == null)
                        {
                            continue;
                        }
                        if (containerEntry.Asset.FileId != 0)
                        {
                            continue; // 外部參照不在此 bundle，略過
                        }
                        UnityObjectInfo obj;
                        if (!objectsByPathId.TryGetValue(containerEntry.Asset.PathId, out obj))
                        {
                            continue;
                        }
                        long absolute = serializedFile.DataOffset + obj.ByteStart;
                        if (absolute < serializedFile.DataOffset)
                        {
                            continue;
                        }
                        this.Entries[key] = new UnityEntryLocation(
                            bundleIndex, nodeIndex, absolute, obj.ByteSize, obj.ClassId);
                    }
                }
            }
        }
    }
}
