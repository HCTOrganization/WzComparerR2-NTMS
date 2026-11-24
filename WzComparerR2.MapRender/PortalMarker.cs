using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WzComparerR2.Animation;

namespace WzComparerR2.MapRender
{
    /// <summary>
    /// 為找不到傳送點圖片的資料集產生替代標記（藍色光柱）。
    /// 只有在設定 <c>showPortalMarker</c> 啟用時才會繪製。
    /// </summary>
    internal static class PortalMarker
    {
        private const int MarkerWidth = 40;
        private const int MarkerHeight = 56;

        private static Frame cachedFrame;

        /// <summary>回傳替代標記的影格（僅首次生成，貼圖會快取）。</summary>
        public static Frame GetFrame(GraphicsDevice graphicsDevice)
        {
            if (graphicsDevice == null)
            {
                return null;
            }

            Frame frame = cachedFrame;
            if (frame != null && frame.Texture != null
                && ReferenceEquals(frame.Texture.GraphicsDevice, graphicsDevice))
            {
                return frame;
            }

            var texture = new Texture2D(graphicsDevice, MarkerWidth, MarkerHeight);
            texture.SetData(CreatePixels());
            frame = new Frame(texture)
            {
                // 與實際傳送點圖片一樣，從腳下座標略微上浮。
                Origin = new Point(MarkerWidth / 2, (int)(MarkerHeight * 0.85f)),
            };
            cachedFrame = frame;
            return frame;
        }

        private static Color[] CreatePixels()
        {
            const float centerX = MarkerWidth / 2f;
            const float centerY = MarkerHeight / 2f;
            const float radiusX = MarkerWidth / 2f - 1f;
            const float radiusY = MarkerHeight / 2f - 1f;

            Color[] pixels = new Color[MarkerWidth * MarkerHeight];
            for (int y = 0; y < MarkerHeight; y++)
            {
                // 越下方越亮，讓它看起來像遊戲內的傳送點光柱。
                float verticalFade = 0.55f + 0.45f * ((y + 0.5f) / MarkerHeight);

                for (int x = 0; x < MarkerWidth; x++)
                {
                    float nx = (x + 0.5f - centerX) / radiusX;
                    float ny = (y + 0.5f - centerY) / radiusY;
                    float distance = (float)Math.Sqrt(nx * nx + ny * ny);
                    if (distance > 1.05f)
                    {
                        pixels[y * MarkerWidth + x] = Color.Transparent;
                        continue;
                    }

                    // 外圍光環 + 內側的淡光。
                    float ring = Clamp01(1f - Math.Abs(distance - 0.78f) / 0.26f);
                    float glow = Clamp01(1f - distance);
                    float alpha = (ring * 0.8f + glow * glow * 0.45f) * verticalFade;
                    if (alpha <= 0.01f)
                    {
                        pixels[y * MarkerWidth + x] = Color.Transparent;
                        continue;
                    }

                    float white = Clamp01(1f - distance * 1.3f);
                    byte r = (byte)(110 + white * 130);
                    byte g = (byte)(185 + white * 60);
                    byte b = 255;
                    float a = Clamp01(alpha);
                    pixels[y * MarkerWidth + x] = new Color(r, g, b, (byte)(a * 255));
                }
            }
            return pixels;
        }

        private static float Clamp01(float value)
        {
            return value < 0f ? 0f : (value > 1f ? 1f : value);
        }
    }
}
