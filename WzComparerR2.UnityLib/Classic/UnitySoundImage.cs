using System;
using System.Collections.Generic;
using WzComparerR2.WzLib;

namespace WzComparerR2.UnityLib.Classic
{
    /// <summary>
    /// Unity 資料集的音效 img（<c>Sound/&lt;img&gt;</c>）以 WZ 的
    /// <c>Sound/&lt;img&gt;.img</c> 形式呈現：內容是巢狀目錄 + <see cref="Wz_Sound"/> 葉節點，
    /// 與傳統 WZ 的配置一致。
    /// <para>
    /// 音效資料（FSB5）在第一次取用時才轉封裝成 Ogg Vorbis，
    /// 因此 <see cref="Wz_Sound.ExtractSound"/>、播放與匯出都能直接使用。
    /// </para>
    /// </summary>
    public sealed class UnitySoundImage : UnityAssetImage
    {
        public UnitySoundImage(ClassicWzSource source, string imageKey)
            : base(source, imageKey)
        {
        }

        /// <summary>以 WZ 風格路徑查詢同一資料集內的節點（可含 <c>.img</c> 後綴）。</summary>
        public Wz_Node FindNode(string wzPath)
        {
            return this.Source.FindNode(wzPath);
        }

        protected override void PopulateContent()
        {
            IReadOnlyList<string> soundKeys = this.Source.Catalog.GetSoundKeys(this.Name);
            string prefixLength = this.Name + "/";
            foreach (string soundKey in soundKeys)
            {
                if (!soundKey.StartsWith(prefixLength, StringComparison.Ordinal))
                {
                    continue;
                }

                string[] segments = soundKey.Substring(prefixLength.Length).Split('/');
                Wz_Node parent = this.Node;
                for (int i = 0; i < segments.Length - 1; i++)
                {
                    parent = GetOrAddFolder(parent, segments[i]);
                }

                string name = segments[segments.Length - 1];
                if (parent.Nodes[name] == null)
                {
                    Wz_Node soundNode = new Wz_Node(name);
                    soundNode.Value = this.Source.CreateSound(soundKey);
                    parent.Nodes.Add(soundNode);
                }
            }
        }

        private static Wz_Node GetOrAddFolder(Wz_Node parent, string name)
        {
            Wz_Node folder = parent.Nodes[name];
            if (folder == null)
            {
                folder = new Wz_Node(name);
                parent.Nodes.Add(folder);
            }
            return folder;
        }
    }
}
