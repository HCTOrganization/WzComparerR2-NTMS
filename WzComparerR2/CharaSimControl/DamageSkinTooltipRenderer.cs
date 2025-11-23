using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using WzComparerR2.AvatarCommon;
using WzComparerR2.CharaSim;
using WzComparerR2.Common;
using WzComparerR2.PluginBase;
using WzComparerR2.WzLib;

namespace WzComparerR2.CharaSimControl
{
    public class DamageSkinTooltipRenderer : TooltipRender
    {
        public DamageSkinTooltipRenderer()
        {
        }

        private DamageSkin damageSkin;

        public DamageSkin DamageSkin
        {
            get { return damageSkin; }
            set { damageSkin = value; }
        }


        public override object TargetItem
        {
            get { return this.damageSkin; }
            set { this.damageSkin = value as DamageSkin; }
        }

        public string CJKUnitSampleNumber { get; set; }
        public string ThousandsUnitSampleNumber { get; set; }
        public bool UseMiniSize { get; set; } = false;
        public bool AlwaysUseMseaFormat { get; set; } = false;
        public long DamageSkinNumber { get; set; } = 1234567890;
        public Bitmap customSampleNonCritical { get; private set; }
        public Bitmap customSampleCritical { get; private set; }

        public override Bitmap Render()
        {
            this.ShowObjectID = true;

            if (this.damageSkin == null)
            {
                return null;
            }

            customSampleNonCritical = GetCustomSample(DamageSkinNumber, UseMiniSize, false);
            customSampleCritical = GetCustomSample(DamageSkinNumber, UseMiniSize, true);
            Bitmap extraBitmap = null;
            // Bitmap extraBitmap = GetExtraEffect(Math.Max(customSampleCritical.Width, customSampleNonCritical.Width));

            int previewWidth = Math.Max(customSampleNonCritical.Width, customSampleCritical.Width);
            int previewHeight = customSampleNonCritical.Height + customSampleCritical.Height;

            if (extraBitmap != null)
            {
                previewWidth = Math.Max(previewWidth, extraBitmap.Width);
                previewHeight += extraBitmap.Height;
            }

            int picH = 10;

            Bitmap tooltip = new Bitmap(previewWidth + 30, previewHeight + 30);
            Graphics g = Graphics.FromImage(tooltip);

            GearGraphics.DrawNewTooltipBack(g, 0, 0, tooltip.Width, tooltip.Height);

            if (this.ShowObjectID)
            {
                GearGraphics.DrawGearDetailNumber(g, 3, 3, $"{this.damageSkin.DamageSkinID.ToString()}", true);
            }

            g.DrawImage(customSampleNonCritical, 10, 10, new Rectangle(0, 0, customSampleNonCritical.Width, customSampleNonCritical.Height), GraphicsUnit.Pixel);
            g.DrawImage(customSampleCritical, 10, 15 + customSampleNonCritical.Height, new Rectangle(0, 0, customSampleCritical.Width, customSampleCritical.Height), GraphicsUnit.Pixel);
            
            if (extraBitmap != null)
            {
                g.DrawImage(extraBitmap, 10, 15 + customSampleNonCritical.Height + customSampleCritical.Height, new Rectangle(0, 0, extraBitmap.Width, extraBitmap.Height), GraphicsUnit.Pixel);
            }

            customSampleNonCritical.Dispose();
            customSampleCritical.Dispose();
            g.Dispose();
            return tooltip;
        }

        public Bitmap GetCustomSample(long inputNumber, bool useMiniSize, bool isCritical)
        {
            string numberStr = "";
            switch (damageSkin.CustomType)
            {
                case "hangulUnit": // CJK
                    numberStr = ItemStringHelper.ToCJKNumberExpr(inputNumber);
                    break;
                case "glUnit": // GMS
                    numberStr = ItemStringHelper.ToThousandsNumberExpr(inputNumber, this.AlwaysUseMseaFormat);
                    break;
                case "glUnit2": // MSEA
                    numberStr = ItemStringHelper.ToThousandsNumberExpr(inputNumber, true);
                    break;
                default:
                    numberStr = inputNumber.ToString();
                    break;
            }

            Bitmap criticalSign = null;
            if (this.damageSkin.BigCriticalUnit.TryGetValue("effect3", out BitmapOrigin criticalSignOrigin))
            {
                criticalSign = criticalSignOrigin.Bitmap;
            }

            int totalWidth = 0;
            int maxHeight = 0;
            int digitSpacing = isCritical ?
                            (useMiniSize ? this.damageSkin.MiniCriticalDigitSpacing :
                            this.damageSkin.BigCriticalDigitSpacing) :
                            (useMiniSize ? this.damageSkin.MiniDigitSpacing :
                            this.damageSkin.BigDigitSpacing);
            int unitSpacing = isCritical ?
                            (useMiniSize ? this.damageSkin.MiniCriticalUnitSpacing :
                            this.damageSkin.BigCriticalUnitSpacing) :
                            (useMiniSize ? this.damageSkin.MiniUnitSpacing :
                            this.damageSkin.BigUnitSpacing);

            if (isCritical && criticalSign != null)
            {
                totalWidth += criticalSign.Width + unitSpacing;
                maxHeight = Math.Max(maxHeight, criticalSign.Height);
            }

            // Calculate total width and max height
            foreach (char c in numberStr)
            {
                string character = c.ToString();
                switch (character)
                {
                    case "0":
                    case "1":
                    case "2":
                    case "3":
                    case "4":
                    case "5":
                    case "6":
                    case "7":
                    case "8":
                    case "9":
                        totalWidth += isCritical ?
                            (useMiniSize ? this.damageSkin.MiniCriticalDigit[character].Bitmap.Width : 
                            this.damageSkin.BigCriticalDigit[character].Bitmap.Width) : 
                            (useMiniSize ? this.damageSkin.MiniDigit[character].Bitmap.Width :
                            this.damageSkin.BigDigit[character].Bitmap.Width);
                        totalWidth += digitSpacing;
                        maxHeight = Math.Max(maxHeight, isCritical ?
                            (useMiniSize ? this.damageSkin.MiniCriticalDigit[character].Bitmap.Height :
                            this.damageSkin.BigCriticalDigit[character].Bitmap.Height) :
                            (useMiniSize ? this.damageSkin.MiniDigit[character].Bitmap.Height :
                            this.damageSkin.BigDigit[character].Bitmap.Height));
                        break;

                    case "万":
                    case "萬":
                    case "만":
                    case ".":
                        totalWidth += isCritical ?
                            (useMiniSize ? this.damageSkin.MiniCriticalUnit.ElementAt(0).Value.Bitmap.Width :
                            this.damageSkin.BigCriticalUnit.ElementAt(0).Value.Bitmap.Width) :
                            (useMiniSize ? this.damageSkin.MiniUnit.ElementAt(0).Value.Bitmap.Width :
                            this.damageSkin.BigUnit.ElementAt(0).Value.Bitmap.Width);
                        totalWidth += unitSpacing;
                        maxHeight = Math.Max(maxHeight, isCritical ?
                            (useMiniSize ? this.damageSkin.MiniCriticalUnit.ElementAt(0).Value.Bitmap.Height :
                            this.damageSkin.BigCriticalUnit.ElementAt(0).Value.Bitmap.Height) :
                            (useMiniSize ? this.damageSkin.MiniUnit.ElementAt(0).Value.Bitmap.Height :
                            this.damageSkin.BigUnit.ElementAt(0).Value.Bitmap.Height));
                        break;

                    case "億":
                    case "亿":
                    case "억":
                    case "K":
                        totalWidth += isCritical ?
                            (useMiniSize ? this.damageSkin.MiniCriticalUnit.ElementAt(1).Value.Bitmap.Width :
                            this.damageSkin.BigCriticalUnit.ElementAt(1).Value.Bitmap.Width) :
                            (useMiniSize ? this.damageSkin.MiniUnit.ElementAt(1).Value.Bitmap.Width :
                            this.damageSkin.BigUnit.ElementAt(1).Value.Bitmap.Width);
                        totalWidth += unitSpacing;
                        maxHeight = Math.Max(maxHeight, isCritical ?
                            (useMiniSize ? this.damageSkin.MiniCriticalUnit.ElementAt(1).Value.Bitmap.Height :
                            this.damageSkin.BigCriticalUnit.ElementAt(1).Value.Bitmap.Height) :
                            (useMiniSize ? this.damageSkin.MiniUnit.ElementAt(1).Value.Bitmap.Height :
                            this.damageSkin.BigUnit.ElementAt(1).Value.Bitmap.Height));
                        break;

                    case "兆":
                    case "조":
                    case "M":
                        if (this.damageSkin.BigUnit.Count >= 3)
                        {
                            totalWidth += isCritical ?
                                (useMiniSize ? this.damageSkin.MiniCriticalUnit.ElementAt(2).Value.Bitmap.Width :
                                this.damageSkin.BigCriticalUnit.ElementAt(2).Value.Bitmap.Width) :
                                (useMiniSize ? this.damageSkin.MiniUnit.ElementAt(2).Value.Bitmap.Width :
                                this.damageSkin.BigUnit.ElementAt(2).Value.Bitmap.Width);
                            totalWidth += unitSpacing;
                            maxHeight = Math.Max(maxHeight, isCritical ?
                                (useMiniSize ? this.damageSkin.MiniCriticalUnit.ElementAt(2).Value.Bitmap.Height :
                                this.damageSkin.BigCriticalUnit.ElementAt(2).Value.Bitmap.Height) :
                                (useMiniSize ? this.damageSkin.MiniUnit.ElementAt(2).Value.Bitmap.Height :
                                this.damageSkin.BigUnit.ElementAt(2).Value.Bitmap.Height));
                        }
                        break;

                    case "京":
                    case "교":
                    case "B":
                        if (this.damageSkin.BigUnit.Count >= 4)
                        {
                            totalWidth += isCritical ?
                                (useMiniSize ? this.damageSkin.MiniCriticalUnit.ElementAt(3).Value.Bitmap.Width :
                                this.damageSkin.BigCriticalUnit.ElementAt(3).Value.Bitmap.Width) :
                                (useMiniSize ? this.damageSkin.MiniUnit.ElementAt(3).Value.Bitmap.Width :
                                this.damageSkin.BigUnit.ElementAt(3).Value.Bitmap.Width);
                            totalWidth += unitSpacing;
                            maxHeight = Math.Max(maxHeight, isCritical ?
                                (useMiniSize ? this.damageSkin.MiniCriticalUnit.ElementAt(3).Value.Bitmap.Height :
                                this.damageSkin.BigCriticalUnit.ElementAt(3).Value.Bitmap.Height) :
                                (useMiniSize ? this.damageSkin.MiniUnit.ElementAt(3).Value.Bitmap.Height :
                                this.damageSkin.BigUnit.ElementAt(3).Value.Bitmap.Height));
                        }
                        break;

                    case "T":
                        if (this.damageSkin.BigUnit.Count >= 5)
                        {
                            totalWidth += isCritical ?
                                (useMiniSize ? this.damageSkin.MiniCriticalUnit.ElementAt(4).Value.Bitmap.Width :
                                this.damageSkin.BigCriticalUnit.ElementAt(4).Value.Bitmap.Width) :
                                (useMiniSize ? this.damageSkin.MiniUnit.ElementAt(4).Value.Bitmap.Width :
                                this.damageSkin.BigUnit.ElementAt(4).Value.Bitmap.Width);
                            totalWidth += unitSpacing;
                            maxHeight = Math.Max(maxHeight, isCritical ?
                                (useMiniSize ? this.damageSkin.MiniCriticalUnit.ElementAt(4).Value.Bitmap.Height :
                                this.damageSkin.BigCriticalUnit.ElementAt(4).Value.Bitmap.Height) :
                                (useMiniSize ? this.damageSkin.MiniUnit.ElementAt(4).Value.Bitmap.Height :
                                this.damageSkin.BigUnit.ElementAt(4).Value.Bitmap.Height));
                        }
                        break;

                    case "Q":
                        if (this.damageSkin.BigUnit.Count >= 6)
                        {
                            totalWidth += isCritical ?
                                (useMiniSize ? this.damageSkin.MiniCriticalUnit.ElementAt(5).Value.Bitmap.Width :
                                this.damageSkin.BigCriticalUnit.ElementAt(5).Value.Bitmap.Width) :
                                (useMiniSize ? this.damageSkin.MiniUnit.ElementAt(5).Value.Bitmap.Width :
                                this.damageSkin.BigUnit.ElementAt(5).Value.Bitmap.Width);
                            totalWidth += unitSpacing;
                            maxHeight = Math.Max(maxHeight, isCritical ?
                                (useMiniSize ? this.damageSkin.MiniCriticalUnit.ElementAt(5).Value.Bitmap.Height :
                                this.damageSkin.BigCriticalUnit.ElementAt(5).Value.Bitmap.Height) :
                                (useMiniSize ? this.damageSkin.MiniUnit.ElementAt(5).Value.Bitmap.Height :
                                this.damageSkin.BigUnit.ElementAt(5).Value.Bitmap.Height));
                        }
                        break;
                }
            }

            Bitmap finalBitmap = new Bitmap(totalWidth, maxHeight);

            using (Graphics g = Graphics.FromImage(finalBitmap))
            {
                g.Clear(Color.Transparent);
                int offsetX = 0;
                if (isCritical && criticalSign != null)
                {
                    g.DrawImage(criticalSign, offsetX, 0);
                    offsetX += criticalSign.Width;
                }
                foreach (char c in numberStr)
                {
                    string character = c.ToString();
                    Bitmap charBitmap = null;
                    switch (character)
                    {
                        case "0":
                        case "1":
                        case "2":
                        case "3":
                        case "4":
                        case "5":
                        case "6":
                        case "7":
                        case "8":
                        case "9":
                            charBitmap = isCritical ?
                                (useMiniSize ? this.damageSkin.MiniCriticalDigit[character].Bitmap :
                                this.damageSkin.BigCriticalDigit[character].Bitmap) :
                                (useMiniSize ? this.damageSkin.MiniDigit[character].Bitmap :
                                this.damageSkin.BigDigit[character].Bitmap);
                            g.DrawImage(charBitmap, offsetX, maxHeight - charBitmap.Height);
                            offsetX += charBitmap.Width + digitSpacing;
                            break;
                        case "万":
                        case "萬":
                        case "만":
                        case ".":
                            charBitmap = isCritical ?
                                (useMiniSize ? this.damageSkin.MiniCriticalUnit.ElementAt(0).Value.Bitmap :
                                this.damageSkin.BigCriticalUnit.ElementAt(0).Value.Bitmap) :
                                (useMiniSize ? this.damageSkin.MiniUnit.ElementAt(0).Value.Bitmap :
                                this.damageSkin.BigUnit.ElementAt(0).Value.Bitmap);
                            g.DrawImage(charBitmap, offsetX, maxHeight - charBitmap.Height);
                            offsetX += charBitmap.Width + unitSpacing;
                            break;
                        case "億":
                        case "亿":
                        case "억":
                        case "K":
                            charBitmap = isCritical ?
                                (useMiniSize ? this.damageSkin.MiniCriticalUnit.ElementAt(1).Value.Bitmap :
                                this.damageSkin.BigCriticalUnit.ElementAt(1).Value.Bitmap) :
                                (useMiniSize ? this.damageSkin.MiniUnit.ElementAt(1).Value.Bitmap :
                                this.damageSkin.BigUnit.ElementAt(1).Value.Bitmap);
                            g.DrawImage(charBitmap, offsetX, maxHeight - charBitmap.Height);
                            offsetX += charBitmap.Width + unitSpacing;
                            break;
                        case "兆":
                        case "조":
                        case "M":
                            if (this.damageSkin.BigUnit.Count >= 3)
                            {
                                charBitmap = isCritical ?
                                    (useMiniSize ? this.damageSkin.MiniCriticalUnit.ElementAt(2).Value.Bitmap :
                                    this.damageSkin.BigCriticalUnit.ElementAt(2).Value.Bitmap) :
                                    (useMiniSize ? this.damageSkin.MiniUnit.ElementAt(2).Value.Bitmap :
                                    this.damageSkin.BigUnit.ElementAt(2).Value.Bitmap);
                                g.DrawImage(charBitmap, offsetX, maxHeight - charBitmap.Height);
                                offsetX += charBitmap.Width + unitSpacing;
                            }
                            break;

                        case "京":
                        case "교":
                        case "B":
                            if (this.damageSkin.BigUnit.Count >= 4)
                            {
                                charBitmap = isCritical ?
                                    (useMiniSize ? this.damageSkin.MiniCriticalUnit.ElementAt(3).Value.Bitmap :
                                    this.damageSkin.BigCriticalUnit.ElementAt(3).Value.Bitmap) :
                                    (useMiniSize ? this.damageSkin.MiniUnit.ElementAt(3).Value.Bitmap :
                                    this.damageSkin.BigUnit.ElementAt(3).Value.Bitmap);
                                g.DrawImage(charBitmap, offsetX, maxHeight - charBitmap.Height);
                                offsetX += charBitmap.Width + unitSpacing;
                            }
                            break;

                        case "T":
                            if (this.damageSkin.BigUnit.Count >= 5)
                            {
                                charBitmap = isCritical ?
                                    (useMiniSize ? this.damageSkin.MiniCriticalUnit.ElementAt(4).Value.Bitmap :
                                    this.damageSkin.BigCriticalUnit.ElementAt(4).Value.Bitmap) :
                                    (useMiniSize ? this.damageSkin.MiniUnit.ElementAt(4).Value.Bitmap :
                                    this.damageSkin.BigUnit.ElementAt(4).Value.Bitmap);
                                g.DrawImage(charBitmap, offsetX, maxHeight - charBitmap.Height);
                                offsetX += charBitmap.Width + unitSpacing;
                            }
                            break;

                        case "Q":
                            if (this.damageSkin.BigUnit.Count >= 6)
                            {
                                charBitmap = isCritical ?
                                    (useMiniSize ? this.damageSkin.MiniCriticalUnit.ElementAt(5).Value.Bitmap :
                                    this.damageSkin.BigCriticalUnit.ElementAt(5).Value.Bitmap) :
                                    (useMiniSize ? this.damageSkin.MiniUnit.ElementAt(5).Value.Bitmap :
                                    this.damageSkin.BigUnit.ElementAt(5).Value.Bitmap);
                                g.DrawImage(charBitmap, offsetX, maxHeight - charBitmap.Height);
                                offsetX += charBitmap.Width + unitSpacing;
                            }
                            break;
                    }
                }
            }
            return finalBitmap;
        }

        private Bitmap GetExtraEffect(int maxWidth)
        {
            BitmapOrigin[] bitmapOrigins = new BitmapOrigin[5];
            if (!this.damageSkin.MiniDigit.TryGetValue("Miss", out bitmapOrigins[0]) &&
                !this.damageSkin.MiniDigit.TryGetValue("guard", out bitmapOrigins[1]) &&
                !this.damageSkin.MiniDigit.TryGetValue("resist", out bitmapOrigins[2]) &&
                !this.damageSkin.MiniDigit.TryGetValue("shoot", out bitmapOrigins[3]) &&
                !this.damageSkin.MiniDigit.TryGetValue("counter", out bitmapOrigins[4]))
            {
                return null;
            }

            Bitmap[] originalBitmaps = new Bitmap[5]
            {
                bitmapOrigins[0].Bitmap,
                bitmapOrigins[1].Bitmap,
                bitmapOrigins[2].Bitmap,
                bitmapOrigins[3].Bitmap,
                bitmapOrigins[4].Bitmap
            };
            

            int row = 0;
            int[] rowWidths = new int[5] { 0, 0, 0, 0, 0 };
            int[] rowHeights = new int[5] { 0, 0, 0, 0, 0 };
            int[] objectIndices = new int[5] { -1, -1, -1, -1, -1 };

            foreach (var bo in originalBitmaps)
            {
                objectIndices[row]++;
                int currentIndex = originalBitmaps.ToList<Bitmap>().IndexOf(bo);
                rowWidths[row] += bo.Width;
                if (rowWidths[row] > maxWidth)
                {
                    if (currentIndex > 0)
                    {
                        rowWidths[row] -= bo.Width;
                        row += 1;
                        rowWidths[row] += bo.Width;
                        rowHeights[row] = Math.Max(rowHeights[row], bo.Height);
                        continue;
                    }
                }
                rowHeights[row] = Math.Max(rowHeights[row], bo.Height);
            }

            Bitmap bitmap = new Bitmap(rowWidths.Sum(), rowHeights.Sum());

            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.Transparent);
                int offsetX = 0;
                int bitmapIndex = 0;
                for (int i = 0; i < row; i++)
                {
                    for (int j = 0; j < objectIndices[i]; j++)
                    {
                        g.DrawImage(originalBitmaps[bitmapIndex], offsetX, rowHeights.Take(row).Sum() + rowHeights[i] - originalBitmaps[bitmapIndex].Height);
                        offsetX += originalBitmaps[bitmapIndex].Width;
                    }
                    offsetX = 0;
                }
            }

            return bitmap;
        }
    }
}
