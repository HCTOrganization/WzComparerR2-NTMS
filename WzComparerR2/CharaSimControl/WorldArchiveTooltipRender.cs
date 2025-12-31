using System;
using System.Collections.Generic;
using System.Drawing;
using WzComparerR2.CharaSim;
using static WzComparerR2.CharaSimControl.RenderHelper;
using Resource = CharaSimResource.Resource;

namespace WzComparerR2.CharaSimControl
{
    public class WorldArchiveTooltipRender : TooltipRender
    {
        public WorldArchiveTooltipRender()
        {
        }

        public string WorldArchiveMessage { get; set; }
        public string MonsterBookMessage { get; set; }
        public string NpcQuoteMessage { get; set; }

        public override Bitmap Render()
        {
            if (string.IsNullOrEmpty(WorldArchiveMessage) && string.IsNullOrEmpty(MonsterBookMessage) && string.IsNullOrEmpty(NpcQuoteMessage))
                return null;

            bool isTranslateEnabled = Translator.IsTranslateEnabled;
            if (isTranslateEnabled)
            {
                WorldArchiveMessage = Translator.MergeString(WorldArchiveMessage, Translator.TranslateString(WorldArchiveMessage), 2);
                MonsterBookMessage = Translator.MergeString(MonsterBookMessage, Translator.TranslateString(MonsterBookMessage), 2);
                NpcQuoteMessage = Translator.MergeString(NpcQuoteMessage, Translator.TranslateString(NpcQuoteMessage), 1);
            }

            if (string.IsNullOrEmpty(WorldArchiveMessage) && !string.IsNullOrEmpty(NpcQuoteMessage))
            {
                WorldArchiveMessage = "(無資料)";
            }

            int height = 30;
            Bitmap bmp1 = new Bitmap(1, 1);
            using (Graphics g = Graphics.FromImage(bmp1))
            {
                if (!string.IsNullOrEmpty(WorldArchiveMessage))
                {
                    foreach (var i in SplitLine(WorldArchiveMessage))
                    {
                        GearGraphics.DrawPlainText(g, i, Translator.IsKoreanStringPresent(i) ? GearGraphics.KMSItemDetailFont : GearGraphics.ItemDetailFont, Color.White, 13, 272, ref height, 16);
                    }
                }
                if (!string.IsNullOrEmpty(WorldArchiveMessage) && !string.IsNullOrEmpty(MonsterBookMessage))
                    height += 15;
                if (!string.IsNullOrEmpty(MonsterBookMessage))
                {
                    GearGraphics.DrawPlainText(g, "[警告] 以下資訊已過時，與目前版本不符。", GearGraphics.ItemDetailFont, Color.White, 13, 272, ref height, 16);
                    height += 4;
                    foreach (var i in SplitLine(MonsterBookMessage))
                    {
                        GearGraphics.DrawPlainText(g, i, Translator.IsKoreanStringPresent(i) ? GearGraphics.KMSItemDetailFont : GearGraphics.ItemDetailFont, Color.White, 13, 272, ref height, 16);
                    }
                }
                if (!string.IsNullOrEmpty(WorldArchiveMessage) && !string.IsNullOrEmpty(NpcQuoteMessage))
                    height += 15;
                if (!string.IsNullOrEmpty(NpcQuoteMessage))
                {
                    GearGraphics.DrawPlainText(g, "該NPC的對話内容", GearGraphics.ItemDetailFont, Color.White, 13, 272, ref height, 16);
                    height += 4;
                    foreach (var i in SplitLine(NpcQuoteMessage))
                    {
                        switch (i.Trim())
                        {
                            case "": break;
                            default:
                                GearGraphics.DrawPlainText(g, "·" + i, Translator.IsKoreanStringPresent(i) ? GearGraphics.KMSItemDetailFont : GearGraphics.ItemDetailFont, Color.White, 13, 272, ref height, 16);
                                break;
                        }
                    }
                }
            }
            height += 13;

            Bitmap bmp2 = new Bitmap(300, height);
            using (Graphics g = Graphics.FromImage(bmp2))
            {
                GearGraphics.DrawNewTooltipBack(g, 0, 0, bmp2.Width, bmp2.Height);
                int picH = 8;
                GearGraphics.DrawPlainText(g, "世界檔案", GearGraphics.ItemDetailFont, Color.FromArgb(255, 255, 255), 8, 130, ref picH, 13);
                picH = 30;
                if (!string.IsNullOrEmpty(WorldArchiveMessage))
                {
                    foreach (var i in SplitLine(WorldArchiveMessage))
                    {
                        GearGraphics.DrawPlainText(g, i, Translator.IsKoreanStringPresent(i) ? GearGraphics.KMSItemDetailFont : GearGraphics.ItemDetailFont, Color.White, 13, 272, ref picH, 16);
                    }
                }
                if (!string.IsNullOrEmpty(WorldArchiveMessage) && !string.IsNullOrEmpty(MonsterBookMessage))
                {
                    g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
                    picH += 3;
                    DrawV6SkillDotline(g, 12, 288, picH);
                    picH += 12;
                    g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;
                }
                if (!string.IsNullOrEmpty(MonsterBookMessage))
                {
                    GearGraphics.DrawPlainText(g, "[警告] 以下資訊已過時，與目前版本不符。", GearGraphics.ItemDetailFont, Color.Orange, 13, 272, ref picH, 16);
                    picH += 4;
                    foreach (var i in SplitLine(MonsterBookMessage))
                    {
                        GearGraphics.DrawPlainText(g, i, Translator.IsKoreanStringPresent(i) ? GearGraphics.KMSItemDetailFont : GearGraphics.ItemDetailFont, Color.White, 13, 272, ref picH, 16);
                    }
                }
                if (!string.IsNullOrEmpty(WorldArchiveMessage) && !string.IsNullOrEmpty(NpcQuoteMessage))
                {
                    g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
                    picH += 3;
                    DrawV6SkillDotline(g, 12, 288, picH);
                    picH += 12;
                    g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;
                }
                if (!string.IsNullOrEmpty(NpcQuoteMessage))
                {
                    GearGraphics.DrawPlainText(g, "該NPC的對話内容", GearGraphics.ItemDetailFont, Color.FromArgb(204, 255, 0), 13, 272, ref picH, 16);
                    picH += 4;
                    foreach (var i in SplitLine(NpcQuoteMessage))
                    {
                        switch (i.Trim())
                        {
                            case "": break;
                            default:
                                GearGraphics.DrawPlainText(g, "·" + i, Translator.IsKoreanStringPresent(i) ? GearGraphics.KMSItemDetailFont : GearGraphics.ItemDetailFont, Color.White, 13, 272, ref picH, 16);
                                break;
                        }
                    }
                }
            }
            return bmp2;
        }

        private string[] SplitLine(string orgText)
        {
            return orgText.Split(new string[] { "\r\n", "\\r\\n", "\\r", "\\n", "\r", "\n" }, StringSplitOptions.None);
        }

        private void DrawV6SkillDotline(Graphics g, int x1, int x2, int y)
        {
            // here's a trick that we won't draw left and right part because it looks the same as background border.
            var picCenter = GearGraphics.is22aniStyle ? Resource.UIToolTipNew_img_Skill_Frame_dotline_c : Resource.UIToolTip_img_Skill_Frame_dotline_c;
            using (var brush = new TextureBrush(picCenter))
            {
                brush.TranslateTransform(x1, y);
                g.FillRectangle(brush, new Rectangle(x1, y, x2 - x1, picCenter.Height));
            }
        }
    }
}
