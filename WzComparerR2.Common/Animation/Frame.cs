using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WzComparerR2.WzLib;
using WzComparerR2.Common;
using WzComparerR2.Rendering;

namespace WzComparerR2.Animation
{
    public class Frame
    {
        public Frame()
        {
            this.A0 = 255;
            this.A1 = 255;
        }

        public Frame(Texture2D texture) : this(texture, null)
        {
        }

        public Frame(Texture2D atlasPage, Rectangle? atlasRect) : this()
        {
            this.Texture = atlasPage;
            this.AtlasRect = atlasRect;
        }

        public Frame(Texture2D texture, Point origin, int z, int delay, bool blend) : this(texture)
        {
            this.Origin = origin;
            this.Z = z;
            this.Delay = delay;
            this.Blend = blend;
        }

        public Texture2D Texture { get; set; }
        public Rectangle? AtlasRect { get; set; }
        public Wz_Png Png { get; set; }
        public int Page { get; set; }
        public Point Origin { get; set; }
        public int Z { get; set; }
        public int Delay { get; set; }
        public int A0 { get; set; }
        public int A1 { get; set; }
        public bool Blend { get; set; }
        public Point LT { get; set; }
        public Point RB { get; set; }

        public Rectangle Rectangle
        {
            get
            {
                if (AtlasRect != null)
                {
                    return new Rectangle(-Origin.X, -Origin.Y, AtlasRect.Value.Width, AtlasRect.Value.Height);
                }
                else if (Texture != null)
                {
                    return new Rectangle(-Origin.X, -Origin.Y, Texture.Width, Texture.Height);
                }
                else
                {
                    return Rectangle.Empty;
                }   
            }
        }

        public static Frame CreateFromNode(Wz_Node frameNode, GraphicsDevice graphicsDevice, GlobalFindNodeFunction findNode, bool loadTexture = true)
        {
            if (frameNode == null || frameNode.Value == null)
            {
                return null;
            }

            while (frameNode.Value is Wz_Uol)
            {
                Wz_Uol uol = frameNode.Value as Wz_Uol;
                Wz_Node uolNode = uol.HandleUol(frameNode);
                if (uolNode != null)
                {
                    frameNode = uolNode;
                }
                else
                {
                    break;
                }
            }
            if (frameNode.Value is Wz_Png)
            {
                var linkNode = frameNode.GetLinkedSourceNode(findNode);
                Wz_Png png = linkNode?.GetValue<Wz_Png>() ?? (Wz_Png)frameNode.Value;

                var frame = new Frame(loadTexture ? png.ToTexture(graphicsDevice) : null)
                {
                    Png = png,
                };
                return FillFrameFromNode(frame, frameNode);
            }
            else if (frameNode.Value is IWzPngSource pngSource)
            {
                // Unity 資產的 Canvas 節點：圖片以 PNG 編碼提供，沒有 Wz_Png 可掛載。
                var frame = new Frame(loadTexture ? pngSource.ToTexture(graphicsDevice) : null);
                return FillFrameFromNode(frame, frameNode);
            }
            return null;
        }

        private static Frame FillFrameFromNode(Frame frame, Wz_Node frameNode)
        {
            foreach (Wz_Node propNode in frameNode.Nodes)
            {
                switch (propNode.Text)
                {
                    case "origin":
                        frame.Origin = ToPoint(propNode.Value);
                        break;
                    case "lt":
                        frame.LT = ToPoint(propNode.Value);
                        break;
                    case "rb":
                        frame.RB = ToPoint(propNode.Value);
                        break;
                    case "delay":
                        frame.Delay = propNode.GetValue<int>();
                        break;
                    case "z":
                        frame.Z = propNode.GetValue<int>();
                        break;
                    case "a0":
                        frame.A0 = propNode.GetValue<int>();
                        break;
                    case "a1":
                        frame.A1 = propNode.GetValue<int>();
                        break;
                }
            }

            if (frame.Delay == 0)
            {
                frame.Delay = 120;//给予默认delay
            }
            return frame;
        }

        /// <summary>座標節點可能是 <see cref="Wz_Vector"/>，也可能是 int/float 陣列。</summary>
        private static Point ToPoint(object value)
        {
            if (value is Wz_Vector vector)
            {
                return new Point(vector.X, vector.Y);
            }
            if (value is int[] ints && ints.Length >= 2)
            {
                return new Point(ints[0], ints[1]);
            }
            if (value is float[] floats && floats.Length >= 2)
            {
                return new Point((int)Math.Round(floats[0]), (int)Math.Round(floats[1]));
            }
            return Point.Zero;
        }
    }
}
