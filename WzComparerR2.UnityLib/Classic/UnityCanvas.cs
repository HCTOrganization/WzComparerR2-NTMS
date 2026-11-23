using System;
using WzComparerR2.WzLib;

namespace WzComparerR2.UnityLib.Classic
{
    /// <summary>
    /// 一個 WZ-like 的 Canvas 節點值：代表「某個 Unity 資產路徑上的圖片」，
    /// PNG 於第一次使用時才解碼（等同傳統 WZ 的 <c>Wz_Png</c>）。
    /// <para>
    /// 繼承 <see cref="Wz_Png"/> 讓只認得 WZ 型別的既有程式（MapRender、預覽工具等）
    /// 也能直接使用：圖片由 <see cref="Wz_Png.PngDataProvider"/> 提供，
    /// <see cref="Wz_Png.ExtractPng(int)"/> 會解碼這份 PNG 而不讀 WZ 資料流。
    /// </para>
    /// </summary>
    public sealed class UnityCanvas : Wz_Png, IWzPngSource
    {
        private readonly ClassicWzSource source;
        private readonly object syncRoot = new object();
        private byte[] png;
        private long pngGeneration = -1;

        public UnityCanvas(ClassicWzSource source, string path)
            : base(0, 0, 0, Wz_TextureFormat.ARGB8888, 0, 0, 0, 0, null)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }
            this.source = source;
            this.Path = path;
            this.PngDataProvider = this.GetPng;
        }

        /// <summary>邏輯路徑，如 <c>Character/00002000/stand1/0/body</c>。</summary>
        public string Path { get; private set; }

        /// <summary>取得（並快取）PNG bytes；查無對應圖片時回 null。</summary>
        public byte[] GetPng()
        {
            long generation = this.source.LocaleGeneration;
            byte[] cached = this.png;
            if (cached != null && this.pngGeneration == generation)
            {
                return cached;
            }
            lock (this.syncRoot)
            {
                if (this.png == null || this.pngGeneration != generation)
                {
                    // 語系切換後同一個 Canvas 可能對應到不同地域的圖，必須重新解碼。
                    this.png = this.source.GetImage(this.Path);
                    this.pngGeneration = generation;
                }
                return this.png;
            }
        }

        byte[] IWzPngSource.GetPngData()
        {
            return this.GetPng();
        }

        public override string ToString()
        {
            return "canvas " + this.Path;
        }
    }
}
