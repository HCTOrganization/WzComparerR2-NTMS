using System;
using System.Text;
using WzComparerR2.WzLib;

namespace WzComparerR2.UnityLib.Classic
{
    /// <summary>
    /// 一個 TextAsset（class 49）JSON 資產的 WZ-like 表示；節點樹於第一次使用時解析。
    /// </summary>
    public sealed class UnityJsonAsset
    {
        private readonly ClassicWzSource source;
        private readonly object syncRoot = new object();
        private Wz_Node node;
        private Wz_Node rootedNode;
        private string text;

        public UnityJsonAsset(ClassicWzSource source, string imageKey)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (imageKey == null)
            {
                throw new ArgumentNullException(nameof(imageKey));
            }
            this.source = source;
            this.ImageKey = imageKey;
        }

        /// <summary>資產的索引鍵，如 <c>String/CN/Skill</c>。</summary>
        public string ImageKey { get; private set; }

        /// <summary>節點名稱（索引鍵最後一段），如 <c>Skill</c>。</summary>
        public string Name
        {
            get
            {
                int slash = this.ImageKey.LastIndexOf('/');
                return slash < 0 ? this.ImageKey : this.ImageKey.Substring(slash + 1);
            }
        }

        /// <summary>JSON 原文（UTF-8）。</summary>
        public string Text
        {
            get
            {
                if (this.text == null)
                {
                    this.text = Encoding.UTF8.GetString(this.source.ReadTextAsset(this.ImageKey).Script);
                }
                return this.text;
            }
        }

        /// <summary>以 WZ 節點樹形式取得內容（首次呼叫時解析並快取）。</summary>
        public Wz_Node AsWzNode()
        {
            if (this.node == null)
            {
                lock (this.syncRoot)
                {
                    if (this.node == null)
                    {
                        this.node = UnityJson.Parse(
                            this.source.ReadTextAsset(this.ImageKey).Script, this.Name);
                    }
                }
            }
            return this.node;
        }

        /// <summary>
        /// 內容根節點。必要時補上與目錄同名的上層節點鏈
        /// （例如 <c>String\CN\Skill.img\…</c>），讓 <see cref="Wz_Node.FullPathToFile"/> 等路徑判斷
        /// 能沿用 WZ 的邏輯。<see cref="UnityAssetImage.GetRootedNode"/> 的作用相同。
        /// </summary>
        public Wz_Node GetRootedNode()
        {
            Wz_Node content = this.AsWzNode();
            if (content.ParentNode != null)
            {
                return content;
            }
            lock (this.syncRoot)
            {
                if (this.rootedNode == null)
                {
                    this.rootedNode = CreateRootedTree(content, this.ImageKey);
                }
            }
            return content;
        }

        private Wz_Node CreateRootedTree(Wz_Node content, string imageKey)
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

            string imageName = segments[segments.Length - 1];
            Wz_Node imageNode = new Wz_Node(
                imageName.EndsWith(".img", StringComparison.OrdinalIgnoreCase) ? imageName : imageName + ".img");
            imageNode.Value = this;
            parent.Nodes.Add(imageNode);
            imageNode.Nodes.Add(content);
            return root;
        }

        public override string ToString()
        {
            return "json " + this.ImageKey;
        }
    }
}
