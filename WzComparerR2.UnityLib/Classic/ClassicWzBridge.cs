using System;
using System.Collections.Generic;
using System.IO;
using WzComparerR2.UnityLib.Unity;
using WzComparerR2.UnityLib.Wzjs;
using WzComparerR2.WzLib;

namespace WzComparerR2.UnityLib.Classic
{
    /// <summary>
    /// 非 WZ 檔來源（Unity 資產）的 img 基底：繼承 <see cref="Wz_Image"/> 讓只認得
    /// <c>Wz_Image</c> 的既有程式（例如 MapRender）可以直接取用。
    /// <para>
    /// <see cref="Wz_Image.TryExtract(out Exception)"/> 會即時把資產內容轉成 WZ 節點
    /// （<see cref="Wz_Image.Node"/>），節點顯示名稱沿用 <c>&lt;名稱&gt;.img</c> 的傳統慣例。
    /// </para>
    /// </summary>
    public abstract class UnityAssetImage : Wz_Image
    {
        private readonly object extractLock = new object();
        private Wz_Node rootedNode;
        private readonly object rootedLock = new object();
        private bool extracted;

        protected UnityAssetImage(ClassicWzSource source, string imageKey)
            : base(imageKey, GetTrailingName(imageKey) + ".img")
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (imageKey == null)
            {
                throw new ArgumentNullException(nameof(imageKey));
            }
            this.Source = source;
        }

        /// <summary>img 邏輯路徑，如 <c>Character/00002000</c>、<c>Sound/Bgm00</c>。</summary>
        public string ImageKey
        {
            get { return this.Name; }
        }

        /// <summary>所屬資料集。</summary>
        public ClassicWzSource Source { get; private set; }

        /// <summary>把資產內容填入 <see cref="Wz_Image.Node"/>（只會被呼叫一次）。</summary>
        protected abstract void PopulateContent();

        /// <summary>內容是否已展開（供比較工具判斷是否需要自行釋放）。</summary>
        public bool IsExtracted
        {
            get { return this.extracted; }
        }

        /// <summary>把資產內容展開成 WZ 節點（可重複呼叫）。</summary>
        public override bool TryExtract(out Exception e)
        {
            try
            {
                this.EnsureExtracted();
                // 與 WZ 的 img 一樣，讓內容節點能往上追溯到目錄與 img 節點。
                // 否則 FullPathToFile 會變成 "WorldMap.img\BaseImg\0" 這種不含目錄名的
                // 路徑，PluginManager.FindWz 的資源搜尋就無法解析。
                this.GetRootedNode();
                e = null;
                return true;
            }
            catch (Exception ex)
            {
                e = ex;
                return false;
            }
        }

        private void EnsureExtracted()
        {
            if (this.extracted)
            {
                return;
            }
            lock (this.extractLock)
            {
                if (this.extracted)
                {
                    return;
                }
                this.PopulateContent();
                this.extracted = true;
            }
        }

        /// <summary>以 WZ 節點樹形式取得內容（首次呼叫時轉換並快取）。</summary>
        public Wz_Node AsWzNode()
        {
            this.EnsureExtracted();
            return this.Node;
        }

        /// <summary>
        /// 釋放已展開的內容（比較工具會在每次比對後呼叫）；下次 <see cref="TryExtract(out Exception)"/>
        /// 會重新由資產轉換一次。資產本身仍保有資料，因此可重複展開。
        /// </summary>
        public override void Unextract()
        {
            lock (this.extractLock)
            {
                if (!this.extracted)
                {
                    return;
                }
                this.extracted = false;
                this.Node.Nodes.Clear();
            }
        }

        /// <summary>
        /// 內容根節點，並在必要時補上與目錄同名的上層節點鏈
        /// （例如 <c>Character\Weapon\01442000\...</c>），讓
        /// <see cref="Wz_Node.FullPathToFile"/> 之類的路徑判斷可沿用 WZ 邏輯。
        /// </summary>
        public Wz_Node GetRootedNode()
        {
            Wz_Node content = this.AsWzNode();
            if (content.ParentNode != null)
            {
                return content;
            }
            lock (rootedLock)
            {
                if (this.rootedNode == null)
                {
                    this.rootedNode = CreateRootedTree(this.Node, this.Name);
                    if (this.rootedNode != null)
                    {
                        this.OwnerNode = this.Node.ParentNode;
                    }
                }
            }
            return content;
        }

        /// <summary>
        /// 為內容節點建立與傳統 WZ 相同的上層鏈：目錄節點 + 一個同名 <c>&lt;img&gt;.img</c> 節點
        /// （內容節點掛在它底下），讓 <see cref="Wz_Node.FullPathToFile"/> 能組出
        /// <c>Sound\Bgm00.img\FloralLife</c> 這種路徑。
        /// </summary>
        private static Wz_Node CreateRootedTree(Wz_Node content, string imageKey)
        {
            string[] segments = imageKey.Split('/');
            if (segments.Length < 2)
            {
                return null;
            }

            Wz_Node root = new Wz_Node(segments[0]);
            Wz_Node parent = root;
            for (int i = 1; i < segments.Length - 1; i++)
            {
                Wz_Node folder = new Wz_Node(segments[i]);
                parent.Nodes.Add(folder);
                parent = folder;
            }

            Wz_Node imageNode = new Wz_Node(content.Text);
            imageNode.Value = content.GetValue<Wz_Image>();
            parent.Nodes.Add(imageNode);
            imageNode.Nodes.Add(content);
            return root;
        }

        private static string GetTrailingName(string name)
        {
            int slash = name.LastIndexOf('/');
            return slash < 0 ? name : name.Substring(slash + 1);
        }
    }

    /// <summary>
    /// 以傳統 WZ 的 <see cref="Wz_Node"/> 形式表示一個 WZJS img。
    /// <para>
    /// Canvas 值為 <see cref="UnityCanvas"/>，它同時是 <see cref="Wz_Png"/>，
    /// 需要圖片時才解碼。
    /// </para>
    /// </summary>
    public class UnityWzImage : UnityAssetImage
    {
        public UnityWzImage(ClassicWzSource source, string name)
            : base(source, name)
        {
        }

        public virtual WzjsNode Root
        {
            get { return this.Source.GetTree(this.Name); }
        }

        /// <summary>取 img 內某節點的 PNG bytes；查無圖時回 null。</summary>
        public byte[] GetPng(string innerPath)
        {
            string path = string.IsNullOrEmpty(innerPath) ? this.Name : this.Name + "/" + innerPath;
            return this.Source.GetImage(path);
        }

        /// <summary>以 WZ 風格路徑查詢同一資料集內的節點（可含 <c>.img</c> 後綴）。</summary>
        public Wz_Node FindNode(string wzPath)
        {
            return this.Source.FindNode(wzPath);
        }

        protected override void PopulateContent()
        {
            WzjsNode tree = this.Source.GetTree(this.Name);
            foreach (WzjsNode child in tree.Children)
            {
                this.Node.Nodes.Add(ClassicWzBridge.ConvertNode(
                    child, this.Name, this.Source));
            }
        }
    }

    /// <summary>
    /// 在 WZ 中以單一 img（例如 <c>Map.wz/MapHelper.img</c>）提供的目錄，
    /// 這裡從 Unity 資料集中個別 sprite 的集合組成一個 img。
    /// </summary>
    public sealed class UnityDirectoryImage : UnityWzImage
    {
        public UnityDirectoryImage(ClassicWzSource source, string directoryPath)
            : base(source, directoryPath)
        {
        }

        /// <summary>沒有 WZJS 實體，因此不能使用 <see cref="UnityWzImage.Root"/>。</summary>
        public override WzjsNode Root
        {
            get { return null; }
        }

        protected override void PopulateContent()
        {
            ClassicWzBridge.BuildDirectoryNodes(this.Source, this.Node, this.Name);
        }
    }

    /// <summary>
    /// Unity 資產樹 → WzComparerR2 <see cref="Wz_Node"/> 的橋接。
    /// </summary>
    public static class ClassicWzBridge
    {
        /// <summary>把一個 WZJS 節點（含子樹）轉成 WzLib 節點。</summary>
        public static Wz_Node ConvertNode(WzjsNode node)
        {
            if (node == null)
            {
                throw new ArgumentNullException(nameof(node));
            }
            Wz_Node root = new Wz_Node(node.Name);
            root.Value = ConvertValue(node.Value);
            foreach (WzjsNode child in node.Children)
            {
                root.Nodes.Add(ConvertNode(child));
            }
            return root;
        }

        /// <summary>建立單一 img 的 WZ 節點（含已解碼子樹與 Canvas PNG）。</summary>
        public static Wz_Node CreateImageNode(UnityWzImage image)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }
            return CreateImageNode(image, image.Name);
        }

        /// <summary>
        /// 以 catalog 建出整棵「類 WZ」目錄樹：中繼節點是目錄，末端節點是 img
        /// （Value 為 <see cref="UnityWzImage"/>）、JSON（<see cref="UnityJsonAsset"/>）
        /// 或 AudioClip 資訊。
        /// </summary>
        public static Wz_Node CreateCatalogNode(ClassicWzSource source, bool includeTrees, bool includeImages)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            string rootName = source.ClientInfo?.DisplayName;
            if (string.IsNullOrEmpty(rootName))
            {
                rootName = Path.GetFileName(
                    source.Catalog.RootPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
            }
            if (string.IsNullOrEmpty(rootName))
            {
                rootName = "MapleStory Classic";
            }

            Wz_Node root = new Wz_Node(rootName);
            BuildEntryNodes(source, root, string.Empty, includeTrees, includeImages);
            CollapseWzImagedDirectories(root, source);
            return root;
        }

        /// <summary>
        /// 在 WZ 中以單一 img 提供的目錄（例如 <c>Map.wz/MapHelper.img</c>）。
        /// Unity 資料集中是以個別 sprite 的目錄索引，因此將其整合為一個 img。
        /// </summary>
        internal static readonly string[] WzImagedDirectories = { "Map/MapHelper" };

        /// <summary><paramref name="key"/> 是否為要整合成 img 的目錄。</summary>
        public static bool IsWzImagedDirectory(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return false;
            }
            foreach (string directory in WzImagedDirectories)
            {
                if (string.Equals(key, directory, StringComparison.Ordinal))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 將 <paramref name="directoryPath"/> 下的 catalog 條目建立在 <paramref name="root"/> 之下，
        /// 成為 WZ 形式的節點（即 <see cref="UnityDirectoryImage"/> 的內容）。
        /// </summary>
        internal static void BuildDirectoryNodes(ClassicWzSource source, Wz_Node root, string directoryPath)
        {
            BuildEntryNodes(source, root, directoryPath + "/", false, false);
        }

        private static void BuildEntryNodes(
            ClassicWzSource source, Wz_Node root, string prefix, bool includeTrees, bool includeImages)
        {
            Dictionary<string, Wz_Node> nodes = new Dictionary<string, Wz_Node>(StringComparer.Ordinal);
            nodes[string.Empty] = root;

            foreach (KeyValuePair<string, UnityEntryLocation> entry in source.Catalog.Entries)
            {
                if (!entry.Key.StartsWith(prefix, StringComparison.Ordinal))
                {
                    continue;
                }
                string[] segments = entry.Key.Substring(prefix.Length).Split('/');
                string currentPath = string.Empty;
                Wz_Node current = root;
                for (int i = 0; i < segments.Length; i++)
                {
                    currentPath = currentPath.Length == 0 ? segments[i] : currentPath + "/" + segments[i];
                    Wz_Node child;
                    if (!nodes.TryGetValue(currentPath, out child))
                    {
                        child = current.Nodes.Add(segments[i]);
                        nodes[currentPath] = child;
                    }
                    current = child;
                }

                switch (entry.Value.ClassId)
                {
                    case UnityClassIds.TextAsset:
                        current.Value = new UnityJsonAsset(source, entry.Key);
                        if (includeTrees)
                        {
                            try
                            {
                                UnityJson.Populate(current, source.ReadTextAsset(entry.Key).Script);
                            }
                            catch (Exception ex) when (ex is UnityException || ex is ClassicAssetException)
                            {
                                current.Nodes.Clear();
                            }
                        }
                        break;

                    case UnityClassIds.AudioClip:
                        if (source.Catalog.IsSoundImage(entry.Key))
                        {
                            // WZ 的音效是 img（Sound/Bgm00.img）底下的節點，這裡以同樣形狀呈現。
                            current.Value = source.GetSoundImage(entry.Key);
                        }
                        else
                        {
                            try
                            {
                                current.Value = source.GetAudioClip(entry.Key);
                            }
                            catch (Exception ex) when (ex is UnityException || ex is ClassicAssetException)
                            {
                                current.Value = null;
                            }
                        }
                        break;

                    case UnityClassIds.MonoBehaviour:
                        current.Value = new UnityWzImage(source, entry.Key);
                        if (includeTrees)
                        {
                            try
                            {
                                WzjsNode tree = source.GetTree(entry.Key);
                                foreach (WzjsNode child in tree.Children)
                                {
                                    current.Nodes.Add(ConvertNode(
                                        child, entry.Key, includeImages ? source : null));
                                }
                            }
                            catch (Exception ex) when (ex is UnityException || ex is WzjsException || ex is ClassicAssetException)
                            {
                                current.Nodes.Clear();
                            }
                        }
                        break;

                    case UnityClassIds.Sprite:
                        // spriteset（SpriteSet/<region>/UI/…）的單一 sprite：解碼時才依語系取圖。
                        current.Value = new UnityCanvas(source, entry.Key);
                        break;
                }
            }
        }

        /// <summary>
        /// 把 <see cref="WzImagedDirectories"/> 的目錄整合成單一 img 節點。
        /// 資料集中若存在相同索引鍵的實體（WZJS），則不做任何處理。
        /// </summary>
        private static void CollapseWzImagedDirectories(Wz_Node root, ClassicWzSource source)
        {
            foreach (string directoryPath in WzImagedDirectories)
            {
                Wz_Node node = root;
                foreach (string segment in directoryPath.Split('/'))
                {
                    node = node?.Nodes[segment];
                    if (node == null)
                    {
                        break;
                    }
                }
                if (node == null || node.Value != null || node.Nodes.Count == 0)
                {
                    continue;
                }
                node.Nodes.Clear();
                node.Text = node.Text + ".img";
                node.Value = source.GetDirectoryImage(directoryPath);
            }
        }

        /// <summary>
        /// 把某個 catalog 資產的內容（WZJS 子樹或 JSON 屬性）附加到
        /// <paramref name="target"/> 底下。
        /// </summary>
        public static void AppendEntryContents(Wz_Node target, ClassicWzSource source, string imageKey)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            UnityEntryLocation location;
            if (!source.Catalog.Entries.TryGetValue(imageKey, out location))
            {
                return;
            }
            switch (location.ClassId)
            {
                case UnityClassIds.TextAsset:
                    UnityJson.Populate(target, source.ReadTextAsset(imageKey).Script);
                    break;

                case UnityClassIds.MonoBehaviour:
                    foreach (WzjsNode child in source.GetTree(imageKey).Children)
                    {
                        target.Nodes.Add(ConvertNode(child, imageKey, source));
                    }
                    break;

                case UnityClassIds.AudioClip:
                    if (source.Catalog.IsSoundImage(imageKey))
                    {
                        UnitySoundImage soundImage = source.GetSoundImage(imageKey);
                        foreach (Wz_Node child in soundImage.AsWzNode().Nodes)
                        {
                            target.Nodes.Add(child);
                        }
                    }
                    break;
            }
        }

        /// <summary>
        /// 動作影格延遲總和（各影格 <c>delay</c> 的絕對值相加）；
        /// null 或沒有 delay 時回 0。
        /// </summary>
        public static int SumActionDelay(Wz_Node actionNode)
        {
            if (actionNode == null)
            {
                return 0;
            }
            int delay = 0;
            foreach (Wz_Node frameNode in actionNode.Nodes)
            {
                Wz_Node delayNode = frameNode.FindNodeByPath("delay");
                if (delayNode != null && delayNode.Value != null)
                {
                    delay += Math.Abs(delayNode.GetValueEx<int>(0));
                }
            }
            return delay;
        }

        /// <summary>把 WZJS 節點值轉成 WzLib 慣用的值型別。</summary>
        public static object ConvertValue(WzjsValue value)        {
            switch (value.Kind)
            {
                case WzjsValueKind.U8:
                case WzjsValueKind.U16:
                case WzjsValueKind.Int:
                    return (int)value.Integer;
                case WzjsValueKind.Long:
                    return value.Integer;
                case WzjsValueKind.Double:
                    return value.Real;
                case WzjsValueKind.Float2:
                    return new[] { value.X, value.Y };
                case WzjsValueKind.Int2:
                    return new[] { value.Int2A, value.Int2B };
                case WzjsValueKind.Vector:
                    return new Wz_Vector(
                        (int)Math.Round(value.X), (int)Math.Round(value.Y));
                case WzjsValueKind.String:
                    return value.Text;
                case WzjsValueKind.Uol:
                    return new Wz_Uol(value.Text);
                default:
                    return null;
            }
        }

        private static Wz_Node CreateImageNode(UnityWzImage image, string imageKey)
        {
            WzjsNode tree = image.Root;
            Wz_Node root = new Wz_Node(tree.Name);
            root.Value = image;
            foreach (WzjsNode child in tree.Children)
            {
                root.Nodes.Add(ConvertNode(child, imageKey, image.Source));
            }
            return root;
        }

        internal static Wz_Node ConvertNode(WzjsNode node, string imageKey, ClassicWzSource imageSource)
        {
            return ConvertNode(node, imageKey, string.Empty, imageSource);
        }

        /// <summary>
        /// 轉換單一節點（含相對路徑追蹤），讓 Canvas 能查到對應 PNG。
        /// </summary>
        private static Wz_Node ConvertNode(WzjsNode node, string imageKey, string parentPath, ClassicWzSource imageSource)
        {
            string path = parentPath.Length == 0 ? node.Name : parentPath + "/" + node.Name;
            Wz_Node result = new Wz_Node(node.Name);
            result.Value = ConvertNodeValue(node, imageKey, path, imageSource);
            foreach (WzjsNode child in node.Children)
            {
                result.Nodes.Add(ConvertNode(child, imageKey, path, imageSource));
            }
            return result;
        }

        private static object ConvertNodeValue(
            WzjsNode node, string imageKey, string innerPath, ClassicWzSource imageSource)
        {
            if (node.IsCanvas)
            {
                if (imageSource == null)
                {
                    return null;
                }
                string fullPath = innerPath.Length == 0 ? imageKey : imageKey + "/" + innerPath;
                return new UnityCanvas(imageSource, fullPath);
            }

            // Classic 中放在 spriteset 的圖片（例如 Map/MapHelper/worldMap/mapImage）
            // 在 WZJS 中會以沒有值也沒有子節點的葉節點出現。
            // 若 catalog 的 sprite 索引有相同路徑，就視為 Canvas，使其形狀與 WZ 相同。
            if (node.Value.Kind == WzjsValueKind.SubTree && node.Children.Count == 0
                && imageSource != null && innerPath.Length > 0)
            {
                string spritePath = imageKey + "/" + innerPath;
                UnityEntryLocation location;
                if (imageSource.Catalog.Entries.TryGetValue(spritePath, out location)
                    && location.ClassId == UnityClassIds.Sprite)
                {
                    return new UnityCanvas(imageSource, spritePath);
                }
            }

            return ConvertValue(node.Value);
        }
    }
}
