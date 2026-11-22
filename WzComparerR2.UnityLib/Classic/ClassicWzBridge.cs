using System;
using System.Collections.Generic;
using System.IO;
using WzComparerR2.UnityLib.Unity;
using WzComparerR2.UnityLib.Wzjs;
using WzComparerR2.WzLib;

namespace WzComparerR2.UnityLib.Classic
{
    /// <summary>
    /// 以傳統 WZ 的 <see cref="Wz_Node"/> 形式表示一個 WZJS img。
    /// <para>
    /// 舊版 WZ 的 Canvas 值型別（<c>Wz_Png</c>）需要綁定 <c>Wz_Image</c> 與檔案資料流，
    /// 無法從記憶體憑空建立；因此這裡的 Canvas 節點以 <see cref="UnityCanvas"/>
    /// 作為 <see cref="Wz_Node.Value"/>，需要圖片時再呼叫其
    /// <see cref="UnityCanvas.GetPng"/> 解碼。其餘型別沿用 WzLib 既有表示：
    /// 整數 → <see cref="int"/>、長整數 → <see cref="long"/>、浮點 → <see cref="float"/>/
    /// <see cref="double"/>、向量 → <see cref="Wz_Vector"/>、UOL → <see cref="Wz_Uol"/>。
    /// </para>
    /// </summary>
    public sealed class UnityWzImage
    {
        private readonly ClassicWzSource source;
        private Wz_Node node;

        public UnityWzImage(ClassicWzSource source, string name)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }
            this.source = source;
            this.Name = name;
        }

        /// <summary>img 邏輯路徑，如 <c>Character/00002000</c>。</summary>
        public string Name { get; private set; }

        /// <summary>所屬資料集。</summary>
        public ClassicWzSource Source
        {
            get { return this.source; }
        }

        public WzjsNode Root
        {
            get { return this.source.GetTree(this.Name); }
        }

        /// <summary>取 img 內某節點的 PNG bytes；查無圖時回 null。</summary>
        public byte[] GetPng(string innerPath)
        {
            string path = string.IsNullOrEmpty(innerPath) ? this.Name : this.Name + "/" + innerPath;
            return this.source.GetImage(path);
        }

        /// <summary>以 WZ 風格路徑查詢同一資料集內的節點（可含 <c>.img</c> 後綴）。</summary>
        public Wz_Node FindNode(string wzPath)
        {
            return this.source.FindNode(wzPath);
        }

        /// <summary>以 WZ 節點樹形式取得內容（首次呼叫時轉換並快取）。</summary>
        public Wz_Node AsWzNode()
        {
            if (this.node == null)
            {
                this.node = ClassicWzBridge.CreateImageNode(this);
            }
            return this.node;
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
            string rootName = Path.GetFileName(
                source.Catalog.RootPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
            if (string.IsNullOrEmpty(rootName))
            {
                rootName = "MapleStory Classic";
            }

            Wz_Node root = new Wz_Node(rootName);
            Dictionary<string, Wz_Node> nodes = new Dictionary<string, Wz_Node>(StringComparer.Ordinal);
            nodes[string.Empty] = root;

            foreach (KeyValuePair<string, UnityEntryLocation> entry in source.Catalog.Entries)
            {
                string[] segments = entry.Key.Split('/');
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
                        try
                        {
                            current.Value = source.GetAudioClip(entry.Key);
                        }
                        catch (Exception ex) when (ex is UnityException || ex is ClassicAssetException)
                        {
                            current.Value = null;
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
                }
            }
            return root;
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

        private static Wz_Node ConvertNode(WzjsNode node, string imageKey, ClassicWzSource imageSource)
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
            return ConvertValue(node.Value);
        }
    }
}
