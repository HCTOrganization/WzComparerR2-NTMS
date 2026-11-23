using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using WzComparerR2.UnityLib.Classic;
using WzComparerR2.UnityLib.Unity;

namespace WzComparerR2.Comparer
{
    /// <summary>
    /// Unity 資產節點（<see cref="WzComparerR2.WzLib.Wz_Node"/> 形式）專用的節點比對器。
    /// <para>
    /// Unity 的圖片／文字資產沒有傳統 WZ 的大小與校驗碼，因此改以實際內容判斷差異：
    /// <see cref="UnityCanvas"/> 比較 PNG 內容，<see cref="UnityJsonAsset"/> 比較原文，
    /// 陣列值（例如 <c>origin</c> 的兩元素座標）逐項比較。
    /// </para>
    /// </summary>
    public class UnityFileComparer : WzFileComparer
    {
        public override bool CompareData(object dataNew, object dataOld)
        {
            if (dataNew is UnityCanvas canvasNew && dataOld is UnityCanvas canvasOld)
            {
                return CompareCanvas(canvasNew, canvasOld);
            }

            if (dataNew is UnityJsonAsset jsonNew && dataOld is UnityJsonAsset jsonOld)
            {
                return string.Equals(jsonNew.Text, jsonOld.Text, StringComparison.Ordinal);
            }

            if (dataNew is UnityAudioClipInfo clipNew && dataOld is UnityAudioClipInfo clipOld)
            {
                return clipNew.Channels == clipOld.Channels
                    && clipNew.Frequency == clipOld.Frequency
                    && clipNew.Length == clipOld.Length
                    && clipNew.ResourceOffset == clipOld.ResourceOffset
                    && clipNew.ResourceSize == clipOld.ResourceSize;
            }

            if (dataNew is Array arrayNew && dataOld is Array arrayOld)
            {
                return CompareArray(arrayNew, arrayOld);
            }

            return base.CompareData(dataNew, dataOld);
        }

        private bool CompareArray(Array arrayNew, Array arrayOld)
        {
            if (arrayNew.Length != arrayOld.Length || arrayNew.Rank != arrayOld.Rank)
            {
                return false;
            }

            if (arrayNew.Rank == 1)
            {
                for (int i = 0; i < arrayNew.Length; i++)
                {
                    if (!this.CompareData(arrayNew.GetValue(i), arrayOld.GetValue(i)))
                    {
                        return false;
                    }
                }
                return true;
            }

            return false;
        }

        private bool CompareCanvas(UnityCanvas canvasNew, UnityCanvas canvasOld)
        {
            canvasNew.EnsureExternalPngInfo();
            canvasOld.EnsureExternalPngInfo();
            bool emptyNew = canvasNew.DataLength == 0;
            bool emptyOld = canvasOld.DataLength == 0;
            if (emptyNew || emptyOld)
            {
                return emptyNew && emptyOld;
            }

            switch (this.PngComparison)
            {
                case WzPngComparison.SizeOnly:
                    return canvasNew.Width == canvasOld.Width
                        && canvasNew.Height == canvasOld.Height;

                case WzPngComparison.Pixel:
                    return CompareCanvasPixels(canvasNew, canvasOld);

                case WzPngComparison.SizeAndDataLength:
                default:
                    return canvasNew.Width == canvasOld.Width
                        && canvasNew.Height == canvasOld.Height
                        && canvasNew.DataLength == canvasOld.DataLength;
            }
        }

        private bool CompareCanvasPixels(UnityCanvas canvasNew, UnityCanvas canvasOld)
        {
            try
            {
                using (Bitmap bmpNew = canvasNew.ExtractPng())
                using (Bitmap bmpOld = canvasOld.ExtractPng())
                {
                    if (bmpNew == null || bmpOld == null)
                    {
                        return bmpNew == null && bmpOld == null;
                    }
                    if (bmpNew.Width != bmpOld.Width || bmpNew.Height != bmpOld.Height)
                    {
                        return false;
                    }

                    byte[] pixelsNew = GetPixelBytes(bmpNew);
                    byte[] pixelsOld = GetPixelBytes(bmpOld);
                    if (pixelsNew == null || pixelsOld == null || pixelsNew.Length != pixelsOld.Length)
                    {
                        return false;
                    }
                    return BytesEqual(pixelsNew, pixelsOld);
                }
            }
            catch (Exception)
            {
                // 無法解碼時退回比較 PNG 內容，避免整份比較中斷。
                byte[] dataNew = canvasNew.GetPng();
                byte[] dataOld = canvasOld.GetPng();
                return dataNew == null ? dataOld == null : BytesEqual(dataNew, dataOld);
            }
        }

        private static bool BytesEqual(byte[] left, byte[] right)
        {
            if (left.Length != right.Length)
            {
                return false;
            }
            for (int i = 0; i < left.Length; i++)
            {
                if (left[i] != right[i])
                {
                    return false;
                }
            }
            return true;
        }

        private static byte[] GetPixelBytes(Bitmap bitmap)
        {
            BitmapData data = bitmap.LockBits(
                new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadOnly,
                PixelFormat.Format32bppArgb);
            try
            {
                byte[] pixels = new byte[Math.Abs(data.Stride) * data.Height];
                Marshal.Copy(data.Scan0, pixels, 0, pixels.Length);
                return pixels;
            }
            finally
            {
                bitmap.UnlockBits(data);
            }
        }
    }
}
