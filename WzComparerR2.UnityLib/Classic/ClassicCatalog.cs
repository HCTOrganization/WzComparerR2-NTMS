using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
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
            this.spritesetEntryKeys = new HashSet<string>(StringComparer.Ordinal);
            this.SoundEntries = new SortedDictionary<string, UnityEntryLocation>(StringComparer.Ordinal);
            this.SoundKeysByImage = new SortedDictionary<string, List<string>>(StringComparer.Ordinal);
        }

        public string RootPath { get; private set; }

        public List<ClassicBundleMeta> Bundles { get; private set; }

        /// <summary>邏輯路徑（如 <c>Character/00002000</c>）→ 物件位置。</summary>
        public SortedDictionary<string, UnityEntryLocation> Entries { get; private set; }

        public UnityImageIndex Image { get; private set; }

        /// <summary>
        /// 音效路徑（如 <c>Sound/Bgm00/FloralLife</c>）→ AudioClip 位置。
        /// 這些條目不會放進 <see cref="Entries"/>：WZ 的音效是 img 底下的節點，
        /// 由 <see cref="SoundKeysByImage"/> 組成 img 內容。
        /// </summary>
        public SortedDictionary<string, UnityEntryLocation> SoundEntries { get; private set; }

        /// <summary>音效 img（如 <c>Sound/Bgm00</c>）→ 其下的音效路徑。</summary>
        public SortedDictionary<string, List<string>> SoundKeysByImage { get; private set; }

        /// <summary>由 <see cref="RebuildSpritesetEntries"/> 補上的索引鍵（換語系時可整批撤銷）。</summary>
        private readonly HashSet<string> spritesetEntryKeys;

        /// <summary>掃描目錄下所有 .bundle，用每個 AssetBundle 物件的 m_Container 建索引。</summary>
        /// <param name="rootDirectory">
        /// 含有 bundle 的目錄。無論指定 bundle 目錄本身、StreamingAssets/aa 或客戶端根目錄，
        /// 都會自動解析出實際位置。
        /// </param>
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

            string bundleDirectory = ResolveBundleDirectory(rootDirectory);

            List<string> files = new List<string>();
            foreach (string path in Directory.GetFiles(bundleDirectory))
            {
                if (string.Equals(Path.GetExtension(path), ".bundle", StringComparison.OrdinalIgnoreCase))
                {
                    files.Add(path);
                }
            }
            files.Sort(StringComparer.Ordinal);

            ClassicCatalog catalog = new ClassicCatalog(bundleDirectory);
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
            catalog.GroupSoundEntries();
            return catalog;
        }

        /// <summary>
        /// 找出實際存放 .bundle 的目錄。
        /// Addressables 1.x 的本機配置是 <c>&lt;aa&gt;/StandaloneWindows64</c>，
        /// 2.x 的遠端 catalog 配置是 <c>&lt;aa&gt;/w</c>，
        /// 因此無論選擇客戶端根目錄、<see cref="System.IO.Path"/> 的 <c>StreamingAssets/aa</c>
        /// 或其下任一目錄都能開啟。
        /// </summary>
        public static string ResolveBundleDirectory(string rootDirectory)
        {
            if (rootDirectory == null)
            {
                throw new ArgumentNullException(nameof(rootDirectory));
            }
            if (HasBundles(rootDirectory))
            {
                return rootDirectory;
            }

            List<string> candidates = new List<string>();
            // 無論選擇客戶端根目錄、StreamingAssets 或 aa 都能開啟。
            List<string> aaDirectories = new List<string>();
            aaDirectories.Add(rootDirectory);
            aaDirectories.Add(Path.Combine(rootDirectory, "StreamingAssets", "aa"));
            aaDirectories.Add(Path.Combine(rootDirectory, "aa"));
            aaDirectories.Add(Path.Combine(rootDirectory, "StreamingAssets"));

            foreach (string aaDirectory in aaDirectories)
            {
                // Addressables 的兩種配置（遠端 catalog w／本機 StandaloneWindows64）。
                candidates.Add(Path.Combine(aaDirectory, "w"));
                string buildTarget = ReadBuildTarget(Path.Combine(aaDirectory, "settings.json"));
                if (buildTarget != null)
                {
                    candidates.Add(Path.Combine(aaDirectory, buildTarget));
                }
                candidates.Add(Path.Combine(aaDirectory, "StandaloneWindows64"));

                // 依序檢查 aa 直下的子目錄。
                candidates.AddRange(SafeGetDirectories(aaDirectory));
                candidates.Add(aaDirectory);
            }

            // 也檢查客戶端直下的子目錄。
            candidates.AddRange(SafeGetDirectories(rootDirectory));

            foreach (string candidate in candidates)
            {
                if (Directory.Exists(candidate) && HasBundles(candidate))
                {
                    return candidate;
                }
            }

            throw new ClassicAssetException(
                "找不到 AssetBundle (.bundle): " + rootDirectory + "\r\n"
                + "請選擇客戶端的 StreamingAssets/aa 資料夾，或含有 bundle 的子資料夾。");
        }

        private static bool HasBundles(string directory)
        {
            return Directory.Exists(directory)
                && Directory.GetFiles(directory, "*.bundle").Length > 0;
        }

        private static IEnumerable<string> SafeGetDirectories(string directory)
        {
            try
            {
                return Directory.Exists(directory)
                    ? Directory.GetDirectories(directory)
                    : Enumerable.Empty<string>();
            }
            catch (IOException)
            {
                return Enumerable.Empty<string>();
            }
        }

        /// <summary>從 aa/settings.json 讀取建置目標名稱（例如 <c>StandaloneWindows64</c>）。</summary>
        private static string ReadBuildTarget(string settingsPath)
        {
            try
            {
                if (!File.Exists(settingsPath))
                {
                    return null;
                }
                string text = File.ReadAllText(settingsPath);
                Match match = Regex.Match(text, "\"m_BuildTarget\"\\s*:\\s*\"([^\"]+)\"", RegexOptions.CultureInvariant);
                return match.Success ? match.Groups[1].Value : null;
            }
            catch (IOException)
            {
                return null;
            }
        }

        /// <summary>
        /// 把音效條目整理成 WZ 的 img 形狀：<c>Sound/&lt;img&gt;/&lt;子目錄…&gt;/&lt;名稱&gt;</c>
        /// 收進 <see cref="SoundEntries"/>，並在 <see cref="Entries"/> 只留下 img 本身
        /// （<c>Sound/&lt;img&gt;</c>），讓樹上出現「Sound &gt; Bgm00 &gt; FloralLife」而不是把每個音效都攤在根目錄。
        /// </summary>
        /// </summary>
        private void GroupSoundEntries()
        {
            List<KeyValuePair<string, UnityEntryLocation>> sounds = new List<KeyValuePair<string, UnityEntryLocation>>();
            foreach (KeyValuePair<string, UnityEntryLocation> entry in this.Entries)
            {
                if (entry.Value.ClassId == UnityClassIds.AudioClip)
                {
                    sounds.Add(entry);
                }
            }

            foreach (KeyValuePair<string, UnityEntryLocation> entry in sounds)
            {
                int firstSlash = entry.Key.IndexOf('/');
                int secondSlash = firstSlash < 0 ? -1 : entry.Key.IndexOf('/', firstSlash + 1);
                if (secondSlash <= firstSlash + 1 || secondSlash == entry.Key.Length - 1)
                {
                    continue; // 形狀不符（例如 Sound/<name>）：維持原樣
                }

                string imageKey = entry.Key.Substring(0, secondSlash);
                List<string> keys;
                if (!this.SoundKeysByImage.TryGetValue(imageKey, out keys))
                {
                    keys = new List<string>();
                    this.SoundKeysByImage[imageKey] = keys;
                }
                keys.Add(entry.Key);
                this.SoundEntries[entry.Key] = entry.Value;
                this.Entries.Remove(entry.Key);
            }

            foreach (KeyValuePair<string, List<string>> group in this.SoundKeysByImage)
            {
                group.Value.Sort(StringComparer.Ordinal);
                if (!this.Entries.ContainsKey(group.Key))
                {
                    this.Entries[group.Key] = this.SoundEntries[group.Value[0]];
                }
            }
        }

        /// <summary>該索引鍵是否為音效 img（如 <c>Sound/Bgm00</c>）。</summary>
        public bool IsSoundImage(string key)
        {
            return key != null && this.SoundKeysByImage.ContainsKey(key);
        }

        /// <summary>音效 img 底下的音效路徑（如 <c>Sound/Bgm00/FloralLife</c>），已排序。</summary>
        public IReadOnlyList<string> GetSoundKeys(string imageKey)
        {
            List<string> keys;
            if (imageKey != null && this.SoundKeysByImage.TryGetValue(imageKey, out keys))
            {
                return keys;
            }
            return new string[0];
        }

        /// <summary>
        /// 重建 spriteset 索引：只收錄 <paramref name="locale"/> 與 <c>Common</c> 提供的路徑。
        /// <para>
        /// spriteset（<c>SpriteSet/&lt;region&gt;/UI/…</c>）不是 WZJS/JSON 資產，原本不會出現在索引裡，
        /// 導致 <c>UI/UIWindow</c>、<c>UI/Login</c> 這類只以 spriteset 提供的目錄在樹上整個消失。
        /// 與傳統 WZ 一致，這裡只索引目前地域實際存在的節點：其他地域獨有的資產
        /// （例如只在 TW/KR 提供的 <c>UI/Login/GameLogo</c>）不會出現在樹上。
        /// </para>
        /// <para>Value 由 <see cref="UnityCanvas"/> 於渲染時依語系解析。</para>
        /// </summary>
        public void RebuildSpritesetEntries(string locale)
        {
            foreach (string key in this.spritesetEntryKeys)
            {
                this.Entries.Remove(key);
            }
            this.spritesetEntryKeys.Clear();

            if (!string.IsNullOrEmpty(locale))
            {
                this.AddSpritesetEntries(locale);
            }
            this.AddSpritesetEntries("Common");
        }

        private void AddSpritesetEntries(string region)
        {
            foreach (KeyValuePair<string, Dictionary<string, UnitySetSpriteEntry>> asset in this.Image.SetAssets)
            {
                int slash = asset.Key.IndexOf('/');
                if (slash <= 0 || slash == asset.Key.Length - 1)
                {
                    continue;
                }
                if (!string.Equals(asset.Key.Substring(0, slash), region, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                string logicalPath = asset.Key.Substring(slash + 1);
                foreach (KeyValuePair<string, UnitySetSpriteEntry> sprite in asset.Value)
                {
                    string path = string.IsNullOrEmpty(sprite.Key)
                        ? logicalPath
                        : logicalPath + "/" + sprite.Key;
                    if (this.Entries.ContainsKey(path) && !this.spritesetEntryKeys.Contains(path))
                    {
                        continue; // WZJS / JSON 資產優先
                    }
                    this.Entries[path] = sprite.Value.SpriteLocation;
                    this.spritesetEntryKeys.Add(path);
                }
            }
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
        /// 依道具 id 推出 <c>Item</c> 目錄下可能的路徑，依優先順序排列；路徑最後一段是
        /// img，之後的段落是 img 內的節點（與參考實作的 item_path_candidates 一致）。
        /// </summary>
        public List<string> GetItemPathCandidates(int itemId)
        {
            List<string> candidates = new List<string>();
            if (itemId < 0)
            {
                return candidates;
            }
            string id = itemId.ToString("D8", CultureInfo.InvariantCulture);
            string group = (itemId / 10000).ToString("D4", CultureInfo.InvariantCulture);
            switch (itemId / 1000000)
            {
                case 2:
                    candidates.Add("Item/Consume/" + group + "/" + id);
                    break;
                case 3:
                    candidates.Add("Item/Install/" + group + "/" + id);
                    break;
                case 4:
                    candidates.Add("Item/Etc/" + group + "/" + id);
                    break;
                case 5:
                    // 寵物是一個 id 一個 img；其餘 5 開頭道具落在 Cash 分區 img。
                    string pet = "Item/Pet/" + itemId.ToString(CultureInfo.InvariantCulture);
                    if (this.Entries.ContainsKey(pet))
                    {
                        candidates.Add(pet);
                    }
                    candidates.Add("Item/Cash/" + group + "/" + id);
                    break;
                case 9:
                    // 套裝組、欄位擴充等：Special img 內的節點是 7 碼 id。
                    candidates.Add("Item/Special/0"
                        + (itemId / 10000).ToString(CultureInfo.InvariantCulture)
                        + "/" + itemId.ToString("D7", CultureInfo.InvariantCulture));
                    break;
            }
            return candidates;
        }

        /// <summary>
        /// 依裝備 id 推出 <c>Character</c> 目錄下可能的路徑，依優先順序排列。
        /// </summary>
        public List<string> GetEquipPathCandidates(int itemId)
        {
            List<string> candidates = new List<string>();
            if (itemId / 1000000 != 1)
            {
                return candidates;
            }
            string directory = GetEquipDirectory(itemId / 10000);
            if (directory != null)
            {
                candidates.Add("Character/" + directory + "/"
                    + itemId.ToString("D8", CultureInfo.InvariantCulture));
            }
            return candidates;
        }

        /// <summary>
        /// 依怪物 id 推出 <c>Mob</c> 目錄下可能的路徑，依優先順序排列
        /// （對應傳統 WZ 的 <c>Mob/&lt;id&gt;.img</c> 與特殊分類）。
        /// </summary>
        public List<string> GetMobPathCandidates(int mobId)
        {
            List<string> candidates = new List<string>();
            if (mobId <= 0)
            {
                return candidates;
            }
            string id = mobId.ToString("D7", CultureInfo.InvariantCulture);
            candidates.Add("Mob/" + id);
            foreach (string group in new[]
            {
                "AbyssExpeditionMob", "MExplorerMob", "QuestCountGroup", "RoguelikeMob",
            })
            {
                candidates.Add("Mob/" + group + "/" + id);
            }
            candidates.Add("Mob/RoguelikeMob/Redmoon/" + id);
            return candidates;
        }

        /// <summary>依 NPC id 推出 <c>Npc</c> 目錄下的路徑。</summary>
        public List<string> GetNpcPathCandidates(int npcId)
        {
            List<string> candidates = new List<string>();
            if (npcId <= 0)
            {
                return candidates;
            }
            candidates.Add("Npc/" + npcId.ToString("D7", CultureInfo.InvariantCulture));
            return candidates;
        }

        /// <summary>
        /// 依技能 id 推出 <c>Skill</c> 目錄下的路徑：
        /// img 是職業（<c>Skill/&lt;id/10000&gt;</c>），技能本身是 img 內的
        /// <c>skill/&lt;id&gt;</c>。
        /// </summary>
        public List<string> GetSkillPathCandidates(int skillId)
        {
            List<string> candidates = new List<string>();
            if (skillId <= 0)
            {
                return candidates;
            }
            candidates.Add("Skill/" + (skillId / 10000).ToString(CultureInfo.InvariantCulture)
                + "/skill/" + skillId.ToString(CultureInfo.InvariantCulture));
            return candidates;
        }

        /// <summary>
        /// 依地圖 id 推出 <c>Map</c> 目錄下的路徑：
        /// <c>Map/Map/Map&lt;首位數字&gt;/&lt;9 碼 id&gt;</c>（與 WZ 的 <c>Map/Map/MapN/&lt;id&gt;.img</c> 對應）。
        /// </summary>
        public List<string> GetMapPathCandidates(int mapId)
        {
            List<string> candidates = new List<string>();
            if (mapId <= 0)
            {
                return candidates;
            }
            string id = mapId.ToString("D9", CultureInfo.InvariantCulture);
            candidates.Add("Map/Map/Map" + id[0] + "/" + id);
            return candidates;
        }

        /// <summary>
        /// 依任務 id 推出 <c>Quest</c> 目錄下的路徑：
        /// <c>Quest/QuestInfo/&lt;id&gt;</c>（對應 WZ 的 <c>Quest/QuestInfo.img/&lt;id&gt;</c>）。
        /// </summary>
        public List<string> GetQuestPathCandidates(int questId)
        {
            List<string> candidates = new List<string>();
            if (questId <= 0)
            {
                return candidates;
            }
            candidates.Add("Quest/QuestInfo/" + questId.ToString(CultureInfo.InvariantCulture));
            return candidates;
        }

        /// <summary>
        /// 在指定目錄（含下一層子目錄）內找出「節點名數值等於 <paramref name="id"/>」的 img 索引鍵，
        /// 忽略補零差異；找不到回 null。用於 WZ 分類目錄與資產樹目錄不一致的情況。
        /// </summary>
        public string FindImageKeyByNumericName(string directory, int id)
        {
            if (string.IsNullOrEmpty(directory) || id < 0)
            {
                return null;
            }
            string prefix = directory + "/";
            foreach (string key in this.Entries.Keys)
            {
                if (!key.StartsWith(prefix, StringComparison.Ordinal))
                {
                    continue;
                }
                string rest = key.Substring(prefix.Length);
                int slash = rest.LastIndexOf('/');
                if (slash >= 0 && rest.IndexOf('/') != slash)
                {
                    continue; // 超過兩層不視為分類 img
                }
                string leaf = slash < 0 ? rest : rest.Substring(slash + 1);
                long value;
                if (TryParseNumericName(leaf, out value) && value == id)
                {
                    return key;
                }
            }
            return null;
        }

        private static string GetEquipDirectory(int group)
        {
            switch (group)
            {
                case 100:
                    return "Cap";
                case 101:
                case 102:
                case 103:
                case 112:
                case 113:
                case 114:
                case 115:
                    return "Accessory";
                case 104:
                    return "Coat";
                case 105:
                    return "Longcoat";
                case 106:
                    return "Pants";
                case 107:
                    return "Shoes";
                case 108:
                    return "Glove";
                case 109:
                    return "Shield";
                case 110:
                    return "Cape";
                case 111:
                    return "Ring";
                case 180:
                    return "PetEquip";
                default:
                    return group >= 121 && group <= 179 ? "Weapon" : null;
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
