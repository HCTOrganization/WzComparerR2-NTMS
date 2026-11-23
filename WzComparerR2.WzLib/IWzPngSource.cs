using System;

namespace WzComparerR2.WzLib
{
    /// <summary>
    /// 以 PNG 編碼資料提供圖片的節點值。
    /// <para>
    /// 傳統 WZ 的圖片是 <see cref="Wz_Png"/>（像素存放在 Wz 檔案資料流中）；
    /// 其他來源（例如 Unity 資產的 Canvas 節點）只要實作這個介面，
    /// 就能沿用既有的圖片解碼與 tooltip 流程。
    /// </para>
    /// </summary>
    public interface IWzPngSource
    {
        /// <summary>取得 PNG 編碼的位元組；沒有圖片時回傳 null。</summary>
        byte[] GetPngData();
    }
}
