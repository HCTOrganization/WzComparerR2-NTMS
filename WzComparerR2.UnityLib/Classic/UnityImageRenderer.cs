using System;
using System.Collections.Generic;
using WzComparerR2.UnityLib.Images;
using WzComparerR2.UnityLib.Unity;
using WzComparerR2.UnityLib.Wzjs;

namespace WzComparerR2.UnityLib.Classic
{
    /// <summary>
    /// 圖片查詢與渲染：WZJS 的 Canvas / UOL 節點 → PNG。
    /// <para>
    /// spritesheet 由 WZSS（舊版）或 SpriteSheet <c>.asset</c>（新版）索引 + atlas 頁
    /// Texture2D 組成；spriteset 則由 sprite 清單 MonoBehaviour + Sprite 物件組成。
    /// 只有目標 sprite 所在的 atlas 頁會被解碼。
    /// </para>
    /// </summary>
    public sealed class UnityImageRenderer
    {
        private const int ImageCacheCapacity = 64;
        private const long ImageCacheBytes = 64L * 1024 * 1024;

        private readonly ClassicCatalog catalog;
        private readonly IReadOnlyList<UnityFsBundle> bundles;
        private readonly Func<string, WzjsNode> treeProvider;
        private readonly object syncRoot = new object();
        private readonly Dictionary<string, byte[]> imageCache = new Dictionary<string, byte[]>(StringComparer.Ordinal);
        private readonly List<string> imageCacheOrder = new List<string>();
        private long imageCacheBytes;

        public UnityImageRenderer(
            ClassicCatalog catalog, IReadOnlyList<UnityFsBundle> bundles,
            Func<string, WzjsNode> treeProvider, string locale)
        {
            if (catalog == null)
            {
                throw new ArgumentNullException(nameof(catalog));
            }
            if (bundles == null)
            {
                throw new ArgumentNullException(nameof(bundles));
            }
            if (treeProvider == null)
            {
                throw new ArgumentNullException(nameof(treeProvider));
            }
            this.catalog = catalog;
            this.bundles = bundles;
            this.treeProvider = treeProvider;
            this.Locale = locale;
        }

        public string Locale { get; set; }

        /// <summary>清空渲染快取（切換語系後應呼叫）。</summary>
        public void ClearCache()
        {
            lock (this.syncRoot)
            {
                this.imageCache.Clear();
                this.imageCacheOrder.Clear();
                this.imageCacheBytes = 0;
            }
        }

        /// <summary>讀取 <see cref="UnityEntryLocation"/> 指向的物件 bytes。</summary>
        public byte[] ReadEntry(UnityEntryLocation location)
        {
            this.ValidateLocation(location);
            UnityFsBundle bundle = this.bundles[location.BundleIndex];
            return bundle.ReadNodeRange(location.NodeIndex, location.AbsoluteOffset, (int)location.ByteSize);
        }

        /// <summary>取 canvas 或 uol 節點的圖片（PNG bytes）。查無圖時回 null。</summary>
        public byte[] GetImage(string path)
        {
            byte[] cached = this.TryGetCachedImage(path);
            if (cached != null)
            {
                return cached;
            }
            byte[] image = this.GetImageUncached(path);
            if (image != null)
            {
                return this.CacheImage(path, image);
            }
            return null;
        }

        /// <summary>
        /// 精確 frame 無法由 WZSS 節點鍵解出時，提供完整 atlas 頁作為預覽。
        /// 僅接受已驗證的 Canvas 或 UOL 內部節點；不讓 img 根或一般 SubTree 誤顯示 atlas。
        /// </summary>
        public byte[] GetCanvasAtlasFallback(string path)
        {
            string imageKey;
            string inner;
            if (!this.catalog.TrySplitImagePrefix(path, out imageKey, out inner) || inner.Length == 0)
            {
                return null;
            }
            WzjsNode node = this.treeProvider(imageKey).Find(inner);
            if (node == null || !node.IsImageCapable)
            {
                return null;
            }

            List<string> candidates = new List<string>();
            candidates.Add(imageKey + "/" + inner);
            int slash = imageKey.LastIndexOf('/');
            if (slash >= 0)
            {
                candidates.Add(imageKey.Substring(0, slash) + "/" + inner);
            }
            candidates.Add(imageKey);

            foreach (string candidate in candidates)
            {
                UnitySheetEntry entry = this.GetSheet(candidate);
                if (entry == null || entry.AtlasPages.Count == 0)
                {
                    continue;
                }
                return this.RenderAtlasPage(entry.AtlasPages[0].Value);
            }
            return null;
        }

        private byte[] GetImageUncached(string path)
        {
            // spriteset 素材（TW/UI/...、Common/UI/...）：不在 catalog.Entries，
            // 用 SetAssets 的鍵做「地域前綴 + 前綴比對」；語系優先，其次 Common。
            foreach (SetAssetLookup lookup in this.GetSetAssetLookups(path))
            {
                Dictionary<string, UnitySetSpriteEntry> asset;
                if (!this.catalog.Image.SetAssets.TryGetValue(lookup.Key, out asset))
                {
                    continue;
                }
                UnitySetSpriteEntry spriteEntry = PickSetSprite(asset, lookup.Inner);
                if (spriteEntry != null)
                {
                    return this.RenderSetSprite(spriteEntry);
                }
            }

            string imageKey;
            string inner;
            if (!this.catalog.TrySplitImagePrefix(path, out imageKey, out inner))
            {
                return null;
            }

            // 圖片只屬於 Canvas 與 UOL（UOL 在 WZSS 節點鍵池裡也有自己的槽）。
            // 在查索引前先以 WZJS 樹明確限縮。
            if (inner.Length > 0)
            {
                WzjsNode node = this.treeProvider(imageKey).Find(inner);
                if (node == null || !node.IsImageCapable)
                {
                    return null;
                }
            }

            List<string> candidates = new List<string>();
            if (inner.Length > 0)
            {
                candidates.Add(imageKey + "/" + inner);
                int slash = imageKey.LastIndexOf('/');
                if (slash >= 0)
                {
                    candidates.Add(imageKey.Substring(0, slash) + "/" + inner);
                }
            }
            candidates.Add(imageKey);

            foreach (string candidate in candidates)
            {
                Dictionary<string, UnitySheetEntry> variants;
                if (!this.catalog.Image.SheetsByPath.TryGetValue(candidate, out variants))
                {
                    continue;
                }
                UnitySheetEntry entry = UnityImageIndex.SelectLocalized(variants, this.Locale);
                if (entry == null)
                {
                    continue;
                }
                // 地域變體（如 Map/Back/blueCave2 的 CN）只收錄該地域改動過的 sprite；
                // 其餘節點要回退到 COMMON 的索引。
                UnitySheetEntry fallback = null;
                UnitySheetEntry common;
                if (variants.TryGetValue("COMMON", out common) && !ReferenceEquals(common, entry))
                {
                    fallback = common;
                }
                bool perImage = string.Equals(candidate, imageKey, StringComparison.Ordinal);
                return this.RenderSheet(entry, fallback, candidate, imageKey, inner, perImage);
            }

            Dictionary<string, UnitySetSpriteEntry> setAsset;
            if (this.catalog.Image.SetAssets.TryGetValue(imageKey, out setAsset))
            {
                UnitySetSpriteEntry spriteEntry = PickSetSprite(setAsset, inner);
                if (spriteEntry != null)
                {
                    return this.RenderSetSprite(spriteEntry);
                }
            }
            return null;
        }

        /// <summary>spriteset 的一組查詢：(asset 索引鍵, sprite 名稱)。</summary>
        private struct SetAssetLookup
        {
            public SetAssetLookup(string key, string inner)
            {
                this.Key = key;
                this.Inner = inner;
            }

            public string Key { get; private set; }
            public string Inner { get; private set; }
        }

        /// <summary>
        /// 列出 spriteset 的候選索引鍵：先試路徑本身（已含 <c>Common/…</c>、<c>TW/…</c> 前綴），
        /// 再依「目前語系 → Common」補上前綴。地域前綴不跨語系回退，避免顯示錯誤地域的圖。
        /// </summary>
        private List<SetAssetLookup> GetSetAssetLookups(string path)
        {
            List<SetAssetLookup> lookups = new List<SetAssetLookup>();
            HashSet<string> seen = new HashSet<string>(StringComparer.Ordinal);
            CollectSetAssetLookups(lookups, seen, path);

            string localePrefix = FindSetPrefix(this.Locale);
            if (localePrefix != null)
            {
                CollectSetAssetLookups(lookups, seen, localePrefix + "/" + path);
            }
            string commonPrefix = FindSetPrefix("Common");
            if (commonPrefix != null)
            {
                CollectSetAssetLookups(lookups, seen, commonPrefix + "/" + path);
            }
            return lookups;
        }

        private string FindSetPrefix(string locale)
        {
            if (string.IsNullOrEmpty(locale))
            {
                return null;
            }
            foreach (string prefix in this.catalog.Image.SetPrefixes)
            {
                if (string.Equals(prefix, locale, StringComparison.OrdinalIgnoreCase))
                {
                    return prefix;
                }
            }
            return null;
        }

        private void CollectSetAssetLookups(
            List<SetAssetLookup> lookups, HashSet<string> seen, string candidatePath)
        {
            string key;
            string inner;
            if (!TrySplitSetAssetPrefix(this.catalog, candidatePath, out key, out inner))
            {
                return;
            }
            if (seen.Add(key + "\n" + inner))
            {
                lookups.Add(new SetAssetLookup(key, inner));
            }
        }

        private UnitySheetEntry GetSheet(string key)
        {
            Dictionary<string, UnitySheetEntry> variants;
            if (!this.catalog.Image.SheetsByPath.TryGetValue(key, out variants))
            {
                return null;
            }
            return UnityImageIndex.SelectLocalized(variants, this.Locale);
        }

        /// <summary>spritesheet → PNG（frame 切圖；映射未知時只有 img 根可輸出整頁）。</summary>
        private byte[] RenderSheet(
            UnitySheetEntry entry, UnitySheetEntry fallback, string sheetKey,
            string imageKey, string inner, bool perImage)
        {
            byte[] image = this.RenderSheetEntry(entry, sheetKey, imageKey, inner, perImage);
            if (image == null && fallback != null && inner.Length > 0)
            {
                // 地域變體只收錄該地域改動過的 sprite：缺少的節點改從 COMMON 變體取圖。
                image = this.RenderSheetEntry(fallback, sheetKey, imageKey, inner, perImage);
            }
            if (image != null)
            {
                return image;
            }
            if (inner.Length == 0)
            {
                throw new UnityException("spritesheet 無 atlas 頁");
            }
            throw new UnityException(string.Format(
                "找不到 Canvas {0}/{1} 可驗證的 spritesheet frame", imageKey, inner));
        }

        /// <summary>以單一圖集變體輸出 PNG；該變體沒有對應 frame 時回 null。</summary>
        private byte[] RenderSheetEntry(
            UnitySheetEntry entry, string sheetKey, string imageKey, string inner, bool perImage)
        {
            if (entry.IndexLocation == null)
            {
                throw new UnityException("spritesheet 缺少圖片索引");
            }
            byte[] payload = this.ReadEntry(entry.IndexLocation);
            UnityMonoBehaviourInfo mono = UnityObjectParsers.ParseMonoBehaviour(payload, true);
            if (mono.HeaderLength > payload.Length)
            {
                throw new UnityException("SpriteSheet MonoBehaviour header 越界");
            }

            WzssSprite? sprite;
            if (entry.IndexIsAsset)
            {
                SpriteSheetAsset asset = SpriteSheetAsset.Parse(Slice(payload, mono.HeaderLength));
                if (!string.Equals(asset.Path, sheetKey, StringComparison.Ordinal))
                {
                    throw new UnityException(string.Format(
                        "SpriteSheet asset 路徑不符: {0} != {1}", asset.Path, sheetKey));
                }
                WzssSprite found;
                if (inner.Length > 0 && asset.TryGetSpriteForNode(inner, out found))
                {
                    sprite = found;
                }
                else if (!perImage && inner.Length > 0 && asset.TryGetFirstSprite(out found))
                {
                    sprite = found;
                }
                else
                {
                    sprite = null;
                }
            }
            else
            {
                WzssIndex wzss = WzssIndex.Parse(Slice(payload, mono.HeaderLength));
                // 舊版 WZSS 用節點鍵定位，少數 UOL 需要反推。
                int frame = -1;
                bool hasFrame = false;
                if (inner.Length > 0)
                {
                    hasFrame = wzss.TryGetFrameIndexForNode(inner, out frame);
                }
                if (!hasFrame)
                {
                    if (perImage && inner.Length > 0)
                    {
                        int inferred;
                        if (TryInferFrameFromUol(this.treeProvider(imageKey), inner, wzss.UolLinks, out inferred))
                        {
                            frame = inferred;
                            hasFrame = true;
                        }
                    }
                    else if (wzss.Rects.Count > 0)
                    {
                        frame = 0;
                        hasFrame = true;
                    }
                }
                WzssSprite wzssSprite;
                sprite = hasFrame && wzss.TryGetSprite(frame, out wzssSprite) ? wzssSprite : (WzssSprite?)null;
            }

            // 只解碼目標 sprite 所在的 atlas 頁。
            int? wantedPage = sprite.HasValue ? sprite.Value.Page : (int?)null;
            List<AtlasPage> pages = new List<AtlasPage>();
            foreach (KeyValuePair<int, UnityEntryLocation> page in entry.AtlasPages)
            {
                bool needed = wantedPage.HasValue
                    ? page.Key == wantedPage.Value
                    // 沒有 frame 時只有 img 根會用到第一頁。
                    : inner.Length == 0 && pages.Count == 0;
                if (!needed)
                {
                    continue;
                }
                byte[] data = this.ReadEntry(page.Value);
                UnityTexture2DInfo texture = UnityObjectParsers.ParseTexture2D(data, true);
                UnityFsBundle bundle = this.bundles[page.Value.BundleIndex];
                pages.Add(new AtlasPage(
                    page.Key, texture.Width, texture.Height,
                    UnityTexture.FetchPixels(bundle, texture, data)));
            }

            if (sprite.HasValue)
            {
                return WzssIndex.CutSprite(pages, sprite.Value).ToPng();
            }
            if (inner.Length == 0)
            {
                if (pages.Count == 0)
                {
                    throw new UnityException("spritesheet 無 atlas 頁");
                }
                AtlasPage page = pages[0];
                UnityTexture.FlipVertical(page.Rgba, page.Width, page.Height);
                return UnityTexture.ToPng(page.Width, page.Height, page.Rgba);
            }
            return null;
        }

        private byte[] RenderAtlasPage(UnityEntryLocation location)
        {
            byte[] data = this.ReadEntry(location);
            UnityTexture2DInfo texture = UnityObjectParsers.ParseTexture2D(data, true);
            UnityFsBundle bundle = this.bundles[location.BundleIndex];
            byte[] rgba = UnityTexture.FetchPixels(bundle, texture, data);
            UnityTexture.FlipVertical(rgba, texture.Width, texture.Height);
            return UnityTexture.ToPng(texture.Width, texture.Height, rgba);
        }

        /// <summary>spriteset sprite → PNG（m_Rect 切圖）。</summary>
        private byte[] RenderSetSprite(UnitySetSpriteEntry entry)
        {
            byte[] spriteData = this.ReadEntry(entry.SpriteLocation);
            UnitySpriteInfo sprite = UnityObjectParsers.ParseSprite(spriteData, true);
            UnityEntryLocation location = entry.TextureLocation;
            byte[] data = this.ReadEntry(location);
            UnityTexture2DInfo texture = UnityObjectParsers.ParseTexture2D(data, true);
            UnityFsBundle bundle = this.bundles[location.BundleIndex];

            int x;
            int y;
            int width;
            int height;
            SpriteRectToPixels(sprite.Rect, out x, out y, out width, out height);
            byte[] rgba = UnityTexture.FetchPixels(bundle, texture, data);
            byte[] cropped = UnityTexture.CropRgba(texture.Width, texture.Height, rgba, x, y, width, height);
            UnityTexture.FlipVertical(cropped, width, height);
            return UnityTexture.ToPng(width, height, cropped);
        }

        private void ValidateLocation(UnityEntryLocation location)
        {
            if (location == null)
            {
                throw new ArgumentNullException(nameof(location));
            }
            if (location.BundleIndex < 0 || location.BundleIndex >= this.bundles.Count)
            {
                throw new UnityException(string.Format(
                    "bundle index {0} 越界（共 {1}）", location.BundleIndex, this.bundles.Count));
            }
            UnityFsBundle bundle = this.bundles[location.BundleIndex];
            if (location.NodeIndex < 0 || location.NodeIndex >= bundle.Nodes.Count)
            {
                throw new UnityException(string.Format(
                    "node index {0} 越界（共 {1}）", location.NodeIndex, bundle.Nodes.Count));
            }
            UnityFsBundle.BundleNode node = bundle.Nodes[location.NodeIndex];
            long end = location.AbsoluteOffset + location.ByteSize;
            if (location.AbsoluteOffset < 0 || end > node.Size)
            {
                throw new UnityException(string.Format(
                    "物件範圍 {0}..{1} 超出 node {2} bytes",
                    location.AbsoluteOffset, end, node.Size));
            }
        }

        private byte[] TryGetCachedImage(string path)
        {
            lock (this.syncRoot)
            {
                byte[] image;
                if (!this.imageCache.TryGetValue(path, out image))
                {
                    return null;
                }
                this.imageCacheOrder.Remove(path);
                this.imageCacheOrder.Add(path);
                return image;
            }
        }

        private byte[] CacheImage(string path, byte[] image)
        {
            if (image.Length > ImageCacheBytes)
            {
                return image;
            }
            lock (this.syncRoot)
            {
                while (this.imageCacheOrder.Count > 0
                    && (this.imageCacheOrder.Count >= ImageCacheCapacity
                        || this.imageCacheBytes + image.Length > ImageCacheBytes))
                {
                    string evict = this.imageCacheOrder[0];
                    this.imageCacheOrder.RemoveAt(0);
                    byte[] removed;
                    if (this.imageCache.TryGetValue(evict, out removed))
                    {
                        this.imageCache.Remove(evict);
                        this.imageCacheBytes -= removed.Length;
                    }
                }
                byte[] previous;
                if (this.imageCache.TryGetValue(path, out previous))
                {
                    this.imageCacheBytes -= previous.Length;
                    this.imageCacheOrder.Remove(path);
                }
                this.imageCache[path] = image;
                this.imageCacheBytes += image.Length;
                this.imageCacheOrder.Add(path);
                return image;
            }
        }

        private static byte[] Slice(byte[] data, int offset)
        {
            if (offset < 0 || offset > data.Length)
            {
                throw new UnityException("payload offset 越界");
            }
            byte[] result = new byte[data.Length - offset];
            Buffer.BlockCopy(data, offset, result, 0, result.Length);
            return result;
        }

        private static bool TrySplitSetAssetPrefix(
            ClassicCatalog catalog, string path, out string key, out string inner)
        {
            key = null;
            inner = null;
            if (path == null)
            {
                return false;
            }
            string current = path;
            while (true)
            {
                if (catalog.Image.SetAssets.ContainsKey(current))
                {
                    key = current;
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

        private static UnitySetSpriteEntry PickSetSprite(
            Dictionary<string, UnitySetSpriteEntry> asset, string inner)
        {
            if (string.IsNullOrEmpty(inner))
            {
                foreach (UnitySetSpriteEntry entry in asset.Values)
                {
                    return entry;
                }
                return null;
            }
            UnitySetSpriteEntry found;
            if (asset.TryGetValue(inner, out found))
            {
                return found;
            }
            string suffix = "/" + inner;
            foreach (KeyValuePair<string, UnitySetSpriteEntry> item in asset)
            {
                if (item.Key.EndsWith(suffix, StringComparison.Ordinal))
                {
                    return item.Value;
                }
            }
            int slash = inner.LastIndexOf('/');
            string last = slash < 0 ? inner : inner.Substring(slash + 1);
            if (asset.TryGetValue(last, out found))
            {
                return found;
            }
            foreach (KeyValuePair<string, UnitySetSpriteEntry> item in asset)
            {
                int itemSlash = item.Key.LastIndexOf('/');
                string itemLast = itemSlash < 0 ? item.Key : item.Key.Substring(itemSlash + 1);
                if (string.Equals(itemLast, last, StringComparison.Ordinal))
                {
                    return item.Value;
                }
            }
            return null;
        }

        /// <summary>
        /// WZSS 的直接 Canvas 鍵是遊戲自訂 hash，rect 也以該 hash 排序，不能用
        /// WZJSON DFS 順序硬配。若同一條原始 UOL 在 WZSS 中一致指向單一 frame，
        /// 則可從 WZJSON 解出其目標 Canvas，得到可驗證的精確映射。
        /// </summary>
        private static bool TryInferFrameFromUol(
            WzjsNode root, string target, IReadOnlyList<WzssUolLink> links, out int frame)
        {
            frame = -1;
            Dictionary<string, SortedSet<int?>> framesByUol =
                new Dictionary<string, SortedSet<int?>>(StringComparer.Ordinal);
            foreach (WzssUolLink link in links)
            {
                SortedSet<int?> frames;
                if (!framesByUol.TryGetValue(link.Uol, out frames))
                {
                    frames = new SortedSet<int?>();
                    framesByUol[link.Uol] = frames;
                }
                frames.Add(link.FrameIndex);
            }

            SortedSet<int> candidates = new SortedSet<int>();
            // WZJSON 的根本身是資產；inner 從根的 children 開始，不含根名稱。
            foreach (WzjsNode child in root.Children)
            {
                WalkUol(child, string.Empty, target, framesByUol, candidates);
            }
            if (candidates.Count != 1)
            {
                return false;
            }
            foreach (int candidate in candidates)
            {
                frame = candidate;
            }
            return true;
        }

        private static void WalkUol(
            WzjsNode node, string path, string target,
            Dictionary<string, SortedSet<int?>> framesByUol, SortedSet<int> candidates)
        {
            string here = path.Length == 0 ? node.Name : path + "/" + node.Name;
            if (node.Value.Kind == WzjsValueKind.Uol)
            {
                string uol = node.Value.Text;
                if (string.Equals(ResolveUolTarget(here, uol), target, StringComparison.Ordinal))
                {
                    SortedSet<int?> frames;
                    if (framesByUol.TryGetValue(uol, out frames) && frames.Count == 1)
                    {
                        foreach (int? frame in frames)
                        {
                            if (frame.HasValue)
                            {
                                candidates.Add(frame.Value);
                            }
                        }
                    }
                }
            }
            foreach (WzjsNode child in node.Children)
            {
                WalkUol(child, here, target, framesByUol, candidates);
            }
        }

        /// <summary>UOL 是相對於該 UOL property 的父節點。</summary>
        internal static string ResolveUolTarget(string source, string uol)
        {
            List<string> segments = new List<string>();
            foreach (string part in source.Split('/'))
            {
                if (part.Length > 0)
                {
                    segments.Add(part);
                }
            }
            if (segments.Count == 0)
            {
                return null;
            }
            segments.RemoveAt(segments.Count - 1);
            foreach (string part in uol.Split('/'))
            {
                if (part.Length == 0 || part == ".")
                {
                    continue;
                }
                if (part == "..")
                {
                    if (segments.Count == 0)
                    {
                        return null;
                    }
                    segments.RemoveAt(segments.Count - 1);
                }
                else
                {
                    segments.Add(part);
                }
            }
            return string.Join("/", segments);
        }

        internal static void SpriteRectToPixels(UnityRectF rect, out int x, out int y, out int width, out int height)
        {
            x = ExactPixel(rect.X, "x", true);
            y = ExactPixel(rect.Y, "y", true);
            width = ExactPixel(rect.Width, "width", false);
            height = ExactPixel(rect.Height, "height", false);
        }

        private static int ExactPixel(float value, string label, bool allowZero)
        {
            if (float.IsNaN(value) || float.IsInfinity(value) || value < 0 || (!allowZero && value == 0))
            {
                throw new UnityException(string.Format("Sprite rect {0} 異常: {1}", label, value));
            }
            float rounded = (float)Math.Round(value);
            if (Math.Abs(value - rounded) > 0.001 || rounded > int.MaxValue)
            {
                throw new UnityException(string.Format(
                    "Sprite rect {0} 不是有效整數像素: {1}", label, value));
            }
            return (int)rounded;
        }
    }
}
