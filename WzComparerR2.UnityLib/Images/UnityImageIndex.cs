using System;
using System.Collections.Generic;
using WzComparerR2.UnityLib.Classic;
using WzComparerR2.UnityLib.Unity;

namespace WzComparerR2.UnityLib.Images
{
    /// <summary>spritesheet 的一組索引與 atlas 頁。</summary>
    public sealed class UnitySheetEntry
    {
        public UnitySheetEntry()
        {
            this.AtlasPages = new List<KeyValuePair<int, UnityEntryLocation>>();
        }

        /// <summary>container 條目順序不保證 index 早於 atlas，建構途中允許尚未看到索引。</summary>
        public UnityEntryLocation IndexLocation { get; set; }

        /// <summary>新版圖集用 .asset 保存路徑與裁切資料；舊版仍使用 WZSS。</summary>
        public bool IndexIsAsset { get; set; }

        /// <summary>(page 序號, Texture2D 物件位置)，依頁號排序。</summary>
        public List<KeyValuePair<int, UnityEntryLocation>> AtlasPages { get; private set; }
    }

    /// <summary>spriteset 的一個 sprite：Sprite 物件位置 + 其 Texture2D 物件位置。</summary>
    public sealed class UnitySetSpriteEntry
    {
        public UnitySetSpriteEntry(UnityEntryLocation spriteLocation, UnityEntryLocation textureLocation)
        {
            this.SpriteLocation = spriteLocation;
            this.TextureLocation = textureLocation;
        }

        public UnityEntryLocation SpriteLocation { get; private set; }
        public UnityEntryLocation TextureLocation { get; private set; }
    }

    /// <summary>
    /// 圖片索引：邏輯路徑 → 圖片來源。
    /// <list type="bullet">
    /// <item>spritesheet_*：container 裡的 <c>.wzspritesheet</c>（舊 WZSS）或
    /// <c>.asset</c>（新版索引物件）+ <c>_N.png</c>（atlas 頁 Texture2D）。</item>
    /// <item>spriteset_*：container 裡的 <c>.asset</c>（sprite 清單 MonoBehaviour）+
    /// Sprite(213) 物件 + Texture2D。</item>
    /// </list>
    /// </summary>
    public sealed class UnityImageIndex
    {
        private const string SheetPrefix = "assets/wzassets/spritesheet/";
        private const string SetPrefix = "assets/wzassets/spriteset/";

        public UnityImageIndex()
        {
            this.SheetsByPath = new Dictionary<string, Dictionary<string, UnitySheetEntry>>(StringComparer.Ordinal);
            this.SetAssets = new Dictionary<string, Dictionary<string, UnitySetSpriteEntry>>(StringComparer.Ordinal);
        }

        /// <summary>邏輯路徑 → 語系 → 該語系的一組索引與圖頁。</summary>
        public Dictionary<string, Dictionary<string, UnitySheetEntry>> SheetsByPath { get; private set; }

        /// <summary>asset 路徑（去前綴去副檔名）→ sprite 清單（名稱 → sprite）。</summary>
        public Dictionary<string, Dictionary<string, UnitySetSpriteEntry>> SetAssets { get; private set; }

        /// <summary>
        /// 使用 Catalog 已解析的 SerializedFile 與 AssetBundle container 收錄圖片來源。
        /// </summary>
        internal void CollectFromContainer(
            UnityFsBundle bundle, int bundleIndex, string bundleStem, int nodeIndex,
            UnitySerializedFile serializedFile, IReadOnlyList<UnityContainerEntry> container,
            Dictionary<long, UnityObjectInfo> objectsByPathId)
        {
            bool isSheet = bundleStem.StartsWith("spritesheet_", StringComparison.Ordinal);
            bool isSet = bundleStem.StartsWith("spriteset_", StringComparison.Ordinal);
            if (!isSheet && !isSet)
            {
                return;
            }

            Func<long, UnityEntryLocation> locationOf = pathId =>
            {
                UnityObjectInfo obj;
                if (!objectsByPathId.TryGetValue(pathId, out obj))
                {
                    return null;
                }
                long absolute = serializedFile.DataOffset + obj.ByteStart;
                if (absolute < serializedFile.DataOffset)
                {
                    return null;
                }
                return new UnityEntryLocation(
                    bundleIndex, nodeIndex, absolute, obj.ByteSize, obj.ClassId);
            };

            if (isSheet)
            {
                this.CollectSheet(container, locationOf);
            }
            else
            {
                this.CollectSet(bundle, nodeIndex, serializedFile, container, locationOf);
            }
        }

        /// <summary>優先使用目前語系，再使用共用圖集。只有一份的素材可供各語系共用。</summary>
        public static UnitySheetEntry SelectLocalized(
            Dictionary<string, UnitySheetEntry> variants, string locale)
        {
            if (variants == null || variants.Count == 0)
            {
                return null;
            }
            if (variants.Count == 1)
            {
                foreach (UnitySheetEntry only in variants.Values)
                {
                    return only;
                }
            }
            UnitySheetEntry found;
            if (locale != null && variants.TryGetValue(locale.ToUpperInvariant(), out found))
            {
                return found;
            }
            if (variants.TryGetValue("COMMON", out found))
            {
                return found;
            }
            return null;
        }

        private void CollectSheet(
            IReadOnlyList<UnityContainerEntry> container, Func<long, UnityEntryLocation> locationOf)
        {
            foreach (UnityContainerEntry entry in container)
            {
                string locale;
                string rest;
                if (!TrySplitSheetPath(entry.Path, out locale, out rest))
                {
                    continue;
                }

                string key;
                bool isAsset;
                int page;
                if (TryStripSuffix(rest, ".wzspritesheet", out key))
                {
                    isAsset = false;
                }
                else if (TryStripSuffix(rest, ".asset", out key))
                {
                    isAsset = true;
                }
                else if (TrySplitAtlasPage(rest, out key, out page))
                {
                    UnityEntryLocation pageLocation = locationOf(entry.Asset.PathId);
                    if (pageLocation == null)
                    {
                        continue;
                    }
                    UnitySheetEntry pageEntry = this.GetOrCreateSheet(key, locale);
                    bool exists = false;
                    foreach (KeyValuePair<int, UnityEntryLocation> item in pageEntry.AtlasPages)
                    {
                        if (item.Key == page)
                        {
                            exists = true;
                            break;
                        }
                    }
                    if (!exists)
                    {
                        pageEntry.AtlasPages.Add(new KeyValuePair<int, UnityEntryLocation>(page, pageLocation));
                        pageEntry.AtlasPages.Sort((a, b) => a.Key.CompareTo(b.Key));
                    }
                    continue;
                }
                else
                {
                    continue;
                }

                UnityEntryLocation indexLocation = locationOf(entry.Asset.PathId);
                if (indexLocation == null)
                {
                    continue;
                }
                UnitySheetEntry sheet = this.GetOrCreateSheet(key, locale);
                sheet.IndexLocation = indexLocation;
                sheet.IndexIsAsset = isAsset;
            }
        }

        private void CollectSet(
            UnityFsBundle bundle, int nodeIndex, UnitySerializedFile serializedFile,
            IReadOnlyList<UnityContainerEntry> container, Func<long, UnityEntryLocation> locationOf)
        {
            // .asset path → (名稱 → sprite pid)
            Dictionary<string, Dictionary<string, long>> assetMap =
                new Dictionary<string, Dictionary<string, long>>(StringComparer.Ordinal);
            foreach (UnityContainerEntry entry in container)
            {
                string lower = entry.Path.ToLowerInvariant();
                if (!lower.StartsWith(SetPrefix, StringComparison.Ordinal) || !lower.EndsWith(".asset", StringComparison.Ordinal))
                {
                    continue;
                }
                string key = entry.Path.Substring(SetPrefix.Length, entry.Path.Length - SetPrefix.Length - ".asset".Length);
                UnityEntryLocation assetLocation = locationOf(entry.Asset.PathId);
                if (assetLocation == null)
                {
                    continue;
                }
                byte[] data = bundle.ReadNodeRange(
                    nodeIndex, assetLocation.AbsoluteOffset, (int)assetLocation.ByteSize);
                UnityMonoBehaviourInfo mono;
                try
                {
                    mono = UnityObjectParsers.ParseMonoBehaviour(data, serializedFile.IsLittleEndian);
                }
                catch (UnityException)
                {
                    continue;
                }
                if (mono.HeaderLength > data.Length)
                {
                    continue;
                }
                byte[] payload = new byte[data.Length - mono.HeaderLength];
                Buffer.BlockCopy(data, mono.HeaderLength, payload, 0, payload.Length);

                Dictionary<string, long> sprites;
                if (!assetMap.TryGetValue(key, out sprites))
                {
                    sprites = new Dictionary<string, long>(StringComparer.Ordinal);
                    assetMap[key] = sprites;
                }
                foreach (KeyValuePair<string, long> item in ParseSetAssetPayload(payload))
                {
                    sprites[item.Key] = item.Value;
                }
            }

            // sprite pid → UnitySetSpriteEntry（解析 Sprite 拿 texture pid）
            foreach (KeyValuePair<string, Dictionary<string, long>> asset in assetMap)
            {
                Dictionary<string, UnitySetSpriteEntry> output =
                    new Dictionary<string, UnitySetSpriteEntry>(StringComparer.Ordinal);
                foreach (KeyValuePair<string, long> item in asset.Value)
                {
                    UnityEntryLocation spriteLocation = locationOf(item.Value);
                    if (spriteLocation == null || spriteLocation.ClassId != UnityClassIds.Sprite)
                    {
                        continue;
                    }
                    byte[] data;
                    try
                    {
                        data = bundle.ReadNodeRange(
                            spriteLocation.NodeIndex, spriteLocation.AbsoluteOffset, (int)spriteLocation.ByteSize);
                    }
                    catch (UnityException)
                    {
                        continue;
                    }
                    UnitySpriteInfo sprite;
                    try
                    {
                        sprite = UnityObjectParsers.ParseSprite(data, serializedFile.IsLittleEndian);
                    }
                    catch (UnityException)
                    {
                        continue;
                    }
                    if (sprite.Texture.FileId != 0)
                    {
                        continue;
                    }
                    UnityEntryLocation textureLocation = locationOf(sprite.Texture.PathId);
                    if (textureLocation == null)
                    {
                        continue;
                    }
                    output[item.Key] = new UnitySetSpriteEntry(spriteLocation, textureLocation);
                }
                if (output.Count > 0)
                {
                    this.SetAssets[asset.Key] = output;
                }
            }
        }

        private UnitySheetEntry GetOrCreateSheet(string key, string locale)
        {
            Dictionary<string, UnitySheetEntry> variants;
            if (!this.SheetsByPath.TryGetValue(key, out variants))
            {
                variants = new Dictionary<string, UnitySheetEntry>(StringComparer.Ordinal);
                this.SheetsByPath[key] = variants;
            }
            string code = locale.ToUpperInvariant();
            UnitySheetEntry entry;
            if (!variants.TryGetValue(code, out entry))
            {
                entry = new UnitySheetEntry();
                variants[code] = entry;
            }
            return entry;
        }

        private static bool TrySplitSheetPath(string path, out string locale, out string rest)
        {
            locale = null;
            rest = null;
            if (path == null || path.Length <= SheetPrefix.Length)
            {
                return false;
            }
            if (string.Compare(path, 0, SheetPrefix, 0, SheetPrefix.Length, StringComparison.OrdinalIgnoreCase) != 0)
            {
                return false;
            }
            string tail = path.Substring(SheetPrefix.Length);
            int slash = tail.IndexOf('/');
            if (slash <= 0 || slash == tail.Length - 1)
            {
                return false;
            }
            locale = tail.Substring(0, slash);
            rest = tail.Substring(slash + 1);
            return true;
        }

        private static bool TryStripSuffix(string value, string suffix, out string result)
        {
            if (value.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
            {
                result = value.Substring(0, value.Length - suffix.Length);
                return true;
            }
            result = null;
            return false;
        }

        /// <summary>"login_2.png" → ("login", 2)；無 _N 後綴 → false。</summary>
        private static bool TrySplitAtlasPage(string value, out string key, out int page)
        {
            key = null;
            page = 0;
            if (!TryStripSuffix(value, ".png", out string baseName))
            {
                return false;
            }
            int underscore = baseName.LastIndexOf('_');
            if (underscore <= 0 || underscore == baseName.Length - 1)
            {
                return false;
            }
            if (!int.TryParse(baseName.Substring(underscore + 1), System.Globalization.NumberStyles.Integer,
                System.Globalization.CultureInfo.InvariantCulture, out page) || page < 0)
            {
                return false;
            }
            key = baseName.Substring(0, underscore);
            return true;
        }

        /// <summary>
        /// spriteset .asset payload：(count, (string, pathID)×n) 或 (count, PPtr×n)；
        /// 另有無 count 前綴、直接連排 PPtr 的佈局。回傳 (名稱, sprite pathID)；
        /// 未命名條目名稱給 ""。
        /// </summary>
        internal static List<KeyValuePair<string, long>> ParseSetAssetPayload(byte[] payload)
        {
            List<KeyValuePair<string, long>> output = new List<KeyValuePair<string, long>>();
            uint count = U32At(payload, 0);
            if (count > 100000)
            {
                return output;
            }
            // count 為 0 有兩種可能：真的是空集合，或根本沒有 count 前綴、
            // 開頭就是 PPtr（fileID = 0）。後者直接連排解析。
            if (count == 0)
            {
                return ParseBarePPtrs(payload);
            }

            uint declaredLength = U32At(payload, 4);
            bool named = declaredLength > 0 && declaredLength < 512;
            if (named)
            {
                long end = 8L + declaredLength;
                if (end > payload.Length)
                {
                    named = false;
                }
                else
                {
                    for (int i = 8; i < end; i++)
                    {
                        byte c = payload[i];
                        if (!((c >= 0x21 && c <= 0x7E) || c == 0x20))
                        {
                            named = false;
                            break;
                        }
                    }
                }
            }

            int position = 4;
            for (uint i = 0; i < count; i++)
            {
                if (named)
                {
                    if (position > payload.Length - 4)
                    {
                        break;
                    }
                    int length = (int)U32At(payload, position);
                    position += 4;
                    if (length < 0 || position > payload.Length - length)
                    {
                        break;
                    }
                    string name = System.Text.Encoding.UTF8.GetString(payload, position, length);
                    position += length;
                    position = (position + 3) & ~3;
                    if (position > payload.Length - 8)
                    {
                        break;
                    }
                    long pathId = I64At(payload, position);
                    position += 8;
                    output.Add(new KeyValuePair<string, long>(name, pathId));
                }
                else
                {
                    // PPtr: fileID i32 + pathID i64
                    if (position > payload.Length - 12)
                    {
                        break;
                    }
                    int fileId = I32At(payload, position);
                    if (fileId != 0)
                    {
                        break;
                    }
                    long pathId = I64At(payload, position + 4);
                    position += 12;
                    output.Add(new KeyValuePair<string, long>(string.Empty, pathId));
                }
            }
            return output;
        }

        /// <summary>
        /// 無 count 前綴的 PPtr 連排：(fileID i32, pathID i64)×n，
        /// fileID 非 0 或 pathID 為 0 時結束。
        /// </summary>
        private static List<KeyValuePair<string, long>> ParseBarePPtrs(byte[] payload)
        {
            List<KeyValuePair<string, long>> output = new List<KeyValuePair<string, long>>();
            int position = 0;
            while (position <= payload.Length - 12)
            {
                int fileId = I32At(payload, position);
                long pathId = I64At(payload, position + 4);
                if (fileId != 0 || pathId == 0)
                {
                    break;
                }
                output.Add(new KeyValuePair<string, long>(string.Empty, pathId));
                position += 12;
            }
            return output;
        }

        private static uint U32At(byte[] data, int position)
        {
            if (position < 0 || position > data.Length - 4)
            {
                return 0;
            }
            return (uint)(data[position]
                | (data[position + 1] << 8)
                | (data[position + 2] << 16)
                | (data[position + 3] << 24));
        }

        private static int I32At(byte[] data, int position)
        {
            return unchecked((int)U32At(data, position));
        }

        private static long I64At(byte[] data, int position)
        {
            if (position < 0 || position > data.Length - 8)
            {
                return 0;
            }
            return data[position]
                | ((long)data[position + 1] << 8)
                | ((long)data[position + 2] << 16)
                | ((long)data[position + 3] << 24)
                | ((long)data[position + 4] << 32)
                | ((long)data[position + 5] << 40)
                | ((long)data[position + 6] << 48)
                | ((long)data[position + 7] << 56);
        }
    }
}
