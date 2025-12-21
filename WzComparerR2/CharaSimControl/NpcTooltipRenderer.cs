using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using WzComparerR2.CharaSim;
using WzComparerR2.Common;
using WzComparerR2.AvatarCommon;
using WzComparerR2.WzLib;
using static WzComparerR2.CharaSimControl.RenderHelper;

namespace WzComparerR2.CharaSimControl
{
    public class NpcTooltipRenderer : TooltipRender
    {
        public NpcTooltipRenderer()
        {

        }


        public override object TargetItem
        {
            get { return this.NpcInfo; }
            set { this.NpcInfo = value as Npc; }
        }

        public Npc NpcInfo { get; set; }
        private AvatarCanvasManager avatar { get; set; }
        public override Bitmap Render()
        {
            if (NpcInfo == null)
            {
                return null;
            }
            Bitmap bmp = new Bitmap(1, 1, PixelFormat.Format32bppArgb);
            Graphics g = Graphics.FromImage(bmp);

            //预绘制
            List<TextBlock> titleBlocks = new List<TextBlock>();
            List<TextBlock> propBlocks = new List<TextBlock>();
            int picY = 0;

            if (NpcInfo.ID > -1)
            {
                string mobName = GetNpcName(NpcInfo.ID);
                var block = PrepareText(g, mobName ?? "(null)", GearGraphics.ItemNameFont2, Brushes.White, 0, 0);
                titleBlocks.Add(block);
                block = PrepareText(g, "ID:" + NpcInfo.ID, GearGraphics.ItemDetailFont, Brushes.White, block.Size.Width + 6, 0);
                titleBlocks.Add(block);
            }

            propBlocks.Add(PrepareText(g, "位置:", GearGraphics.ItemDetailFont, GearGraphics.GearNameBrushG, 0, 0));
            if (NpcInfo?.ID != null)
            {
                var locNode = PluginBase.PluginManager.FindWz("Etc\\NpcLocation.img\\" + NpcInfo.ID.ToString());
                if (locNode != null)
                {
                    foreach (var locMapNode in locNode.Nodes)
                    {
                        int mapID;
                        string mapName = null;
                        if (int.TryParse(locMapNode.Text, out mapID))
                        {
                            mapName = GetMapName(mapID);
                        }
                        string npcLoc = string.Format("{0}({1})", mapName ?? "null", locMapNode.Text);//{0}  ({1}) in the quotations for both location and mapID

                        propBlocks.Add(PrepareText(g, npcLoc, GearGraphics.ItemDetailFont, Brushes.White, 0, picY += 16));
                    }
                }
            }

            if (propBlocks.Count == 1) //获取地区失败
            {
                propBlocks.Add(PrepareText(g, "不明", GearGraphics.ItemDetailFont, Brushes.White, 0, picY += 16));
            }

            //计算大小
            Rectangle titleRect = Measure(titleBlocks);
            Rectangle imgRect = Rectangle.Empty;
            Rectangle textRect = Measure(propBlocks);
            Bitmap npcImg = NpcInfo.Default.Bitmap;
            if (NpcInfo.IsComponentNPC)
            {
                if (this.avatar == null)
                {
                    this.avatar = new AvatarCanvasManager();
                }

                var skin = NpcInfo.Component.Nodes["skin"].GetValueEx<int>(0);
                this.avatar.AddBodyFromSkin3(skin);

                foreach (var node in NpcInfo.Component.Nodes)
                {
                    var gearID = node.GetValueEx<int>(0);
                    this.avatar.AddGear(gearID);
                }

                var img = this.avatar.GetBitmapOrigin();
                if (img.Bitmap != null)
                {
                    if (NpcInfo.Default.Bitmap != null)
                    {
                        NpcInfo.Default.Bitmap.Dispose();
                    }
                    NpcInfo.Default = img;
                    npcImg = img.Bitmap;
                    NpcInfo.AvatarBitmap = npcImg;
                }

                this.avatar.ClearCanvas();
            }
            if (npcImg != null)
            {
                if (npcImg.Width > 250 || npcImg.Height > 300) //进行缩放
                {
                    double scale = Math.Min((double)250 / npcImg.Width, (double)300 / npcImg.Height);
                    imgRect = new Rectangle(0, 0, (int)(npcImg.Width * scale), (int)(npcImg.Height * scale));
                }
                else
                {
                    imgRect = new Rectangle(0, 0, npcImg.Width, npcImg.Height);
                }
            }

            Bitmap illustration2Tooltip = drawIllustration2SetTooltip(NpcInfo.Illustration2Bitmaps, 8, 4);

            //布局 
            //水平排列
            int width = 0;
            if (!imgRect.IsEmpty)
            {
                textRect.X = imgRect.Width + 4;
            }
            width = Math.Max(titleRect.Width, Math.Max(imgRect.Right, textRect.Right));
            titleRect.X = (width - titleRect.Width) / 2;

            //垂直居中
            int height = Math.Max(imgRect.Height, textRect.Height);
            imgRect.Y = (height - imgRect.Height) / 2;
            textRect.Y = (height - textRect.Height) / 2;
            if (!titleRect.IsEmpty)
            {
                height += titleRect.Height + 4;
                imgRect.Y += titleRect.Bottom + 4;
                textRect.Y += titleRect.Bottom + 4;
            }

            //绘制
            bmp = new Bitmap(width + 20, height + 20);
            titleRect.Offset(10, 10);
            imgRect.Offset(10, 10);
            textRect.Offset(10, 10);
            g = Graphics.FromImage(bmp);
            //绘制背景
            GearGraphics.DrawNewTooltipBack(g, 0, 0, bmp.Width, bmp.Height);
            //绘制标题
            foreach (var item in titleBlocks)
            {
                DrawText(g, item, titleRect.Location);
            }
            //绘制图像
            if (npcImg != null && !imgRect.IsEmpty)
            {
                g.DrawImage(npcImg, imgRect);
            }
            //绘制文本
            foreach (var item in propBlocks)
            {
                DrawText(g, item, textRect.Location);
            }
            g.Dispose();
            if (illustration2Tooltip != null)
            {
                Point illustration2Origin = new Point(bmp.Width, 0);
                int totalWidth = bmp.Width + illustration2Tooltip.Width;
                int totalHeight = Math.Max(bmp.Height, illustration2Tooltip.Height);
                Bitmap newTooltip = new Bitmap(totalWidth, totalHeight, PixelFormat.Format32bppArgb);
                Graphics g2 = Graphics.FromImage(newTooltip);
                g2.DrawImage(bmp, 0, 0);
                g2.DrawImage(illustration2Tooltip, illustration2Origin);
                g2.Dispose();
                return newTooltip;
            }
            else
            {
                return bmp;
            }
        }

        private Bitmap drawIllustration2SetTooltip(List<Bitmap> bitmaps, int margin, int perLineCount)
        {
            if (bitmaps == null || bitmaps.Count == 0)
            {
                return null;
            }

            int requiredLines = (int)Math.Ceiling(bitmaps.Count / (double)perLineCount);

            int width = 0;
            int height = 0;

            int currentLineWidth = 0;
            int currentLineHeight = 0;
            int lineCount = 0;
            foreach (var bmp in bitmaps)
            {
                if (bmp != null)
                {
                    currentLineWidth += bmp.Width + margin;
                    currentLineHeight = Math.Max(currentLineHeight, bmp.Height);
                }
                if (bitmaps.IndexOf(bmp) % perLineCount == perLineCount - 1)
                {
                    width = Math.Max(width, currentLineWidth);
                    height += currentLineHeight + margin;
                    currentLineWidth = 0;
                    currentLineHeight = 0;
                    lineCount++;
                }
            }
            if (lineCount < requiredLines)
            {
                width = Math.Max(width, currentLineWidth);
                height += currentLineHeight + margin;
                currentLineWidth = 0;
            }
            Bitmap result = new Bitmap(width + 30, height + 30, PixelFormat.Format32bppArgb);
            Graphics g = Graphics.FromImage(result);
            
            GearGraphics.DrawNewTooltipBack(g, 0, 0, result.Width, result.Height);

            int x = 15;
            int y = 15;
            int maxLineHeight = 0;
            foreach (var bmp in bitmaps)
            {
                if (bmp != null)
                {
                    maxLineHeight = Math.Max(maxLineHeight, bmp.Height);
                    g.DrawImage(bmp, x, y + maxLineHeight - bmp.Height);
                    x += bmp.Width + margin;
                }
                if (bitmaps.IndexOf(bmp) % perLineCount == perLineCount - 1)
                {
                    x = 15;
                    y += maxLineHeight + margin;
                    maxLineHeight = 0;
                }
            }

            // Draw Illust Info
            var labelFont = new Font("SimSun", 12f, GraphicsUnit.Pixel);
            int picH = 2;
            GearGraphics.DrawPlainText(g, $"立繪: {bitmaps.Count}幅", labelFont, Color.FromArgb(255, 255, 255), 2, 80, ref picH, 10);

            g.Dispose();
            return result;
        }

        private string GetNpcName(int npcID)
        {
            bool isTranslateRequired = Translator.IsTranslateEnabled;
            StringResult sr;
            if (this.StringLinker == null || !this.StringLinker.StringNpc.TryGetValue(npcID, out sr))
            {
                return null;
            }
            if (isTranslateRequired)
            {
                return Translator.MergeString(sr.Name, Translator.TranslateString(sr.Name, true), 0, false, true);
            }
            else
            {
                return sr.Name;
            }
        }

        private string GetMapName(int mapID)
        {
            bool isTranslateRequired = Translator.IsTranslateEnabled;
            StringResult sr;
            if (this.StringLinker == null || !this.StringLinker.StringMap.TryGetValue(mapID, out sr))
            {
                return null;
            }
            if (isTranslateRequired)
            {
                return Translator.MergeString(sr.Name, Translator.TranslateString(sr.Name, true), 0, false, true);
            }
            else
            {
                return sr.Name;
            }
        }
    }
}
