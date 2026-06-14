using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Resource = CharaSimResource.Resource;
using WzComparerR2.AvatarCommon;
using WzComparerR2.CharaSim;
using WzComparerR2.Common;
using WzComparerR2.PluginBase;
using WzComparerR2.WzLib;

namespace WzComparerR2.CharaSimControl
{
    public class DamageSkinTooltipRender : TooltipRender
    {
        public DamageSkinTooltipRender()
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

        public bool UseMiniSize { get; set; }
        public bool AlwaysUseMseaFormat { get; set; }
        public bool DisplayUnitOnSingleLine { get; set; }
        public bool UseInGameSpacing { get; set; }
        public long DamageSkinNumber { get; set; }

        public override Bitmap Render()
        {
            if (this.damageSkin == null)
            {
                return null;
            }

            Bitmap customSampleNonCritical = GetCustomSample(DamageSkinNumber, UseMiniSize, false);
            Bitmap customSampleCritical = GetCustomSample(DamageSkinNumber, UseMiniSize, true);
            Bitmap extraBitmap = GetExtraEffect();
            Bitmap unitBitmap = null;

            int previewWidth = Math.Max(customSampleNonCritical.Width, customSampleCritical.Width);
            int previewHeight = customSampleNonCritical.Height + customSampleCritical.Height;

            if (extraBitmap != null)
            {
                previewWidth = Math.Max(previewWidth, extraBitmap.Width);
                previewHeight += extraBitmap.Height;
                if (DisplayUnitOnSingleLine)
                {
                    unitBitmap = GetUnit();
                    if (unitBitmap != null)
                    {
                        previewWidth = Math.Max(previewWidth, unitBitmap.Width);
                        previewHeight += unitBitmap.Height;
                    }
                }
            }

            int picH = 10;

            Bitmap tooltip = new Bitmap(previewWidth + 30, previewHeight + 30);
            Graphics g = Graphics.FromImage(tooltip);

            GearGraphics.DrawNewTooltipBack(g, 0, 0, tooltip.Width, tooltip.Height);

            if (this.ShowObjectID)
            {
                GearGraphics.DrawGearDetailNumber(g, 3, 3, $"{this.damageSkin.DamageSkinID.ToString()}", true);
            }

            g.DrawImage(customSampleNonCritical, 10, picH, new Rectangle(0, 0, customSampleNonCritical.Width, customSampleNonCritical.Height), GraphicsUnit.Pixel);

            picH += customSampleNonCritical.Height + 5;

            g.DrawImage(customSampleCritical, 10, picH, new Rectangle(0, 0, customSampleCritical.Width, customSampleCritical.Height), GraphicsUnit.Pixel);

            picH += customSampleCritical.Height + 5;

            if (unitBitmap != null)
            {
                g.DrawImage(unitBitmap, 10, picH, new Rectangle(0, 0, unitBitmap.Width, unitBitmap.Height), GraphicsUnit.Pixel);
                picH += unitBitmap.Height + 5;
            }

            if (extraBitmap != null)
            {
                g.DrawImage(extraBitmap, 10, picH, new Rectangle(0, 0, extraBitmap.Width, extraBitmap.Height), GraphicsUnit.Pixel);
            }

            customSampleNonCritical.Dispose();
            customSampleCritical.Dispose();
            g.Dispose();
            return tooltip;
        }

        public Bitmap GetCustomSample(long inputNumber, bool useMiniSize, bool isCritical)
        {
            string numberStr = FormatNumberString(inputNumber);
            var damageStyleParams = GetDamageStyleParameters(isCritical);
            BitmapOrigin criticalSign = GetCriticalSign();

            var drawOrder = new List<DamageSkinDrawInfo>();
            var critInfo = InitializeCriticalInfo(criticalSign, ref damageStyleParams);

            BuildDrawOrder(numberStr, drawOrder, damageStyleParams, isCritical);

            return RenderDamageBitmap(drawOrder, critInfo, damageStyleParams);
        }

        /// <summary>
        /// Formats the input number string based on the damage skin's custom type.
        /// </summary>
        private string FormatNumberString(long inputNumber)
        {
            if (DisplayUnitOnSingleLine)
                return inputNumber.ToString();

            switch (damageSkin.CustomType)
            {
                case "hangul": // CJK Detailed
                    return ItemStringHelper.ToCJKNumberExpr(inputNumber, detailedExpr: true);
                case "hangulUnit": // CJK
                    return ItemStringHelper.ToCJKNumberExpr(inputNumber);
                case "glUnit": // GMS
                    return ItemStringHelper.ToThousandsNumberExpr(inputNumber, isMsea: this.AlwaysUseMseaFormat);
                case "glUnit2": // MSEA
                    return ItemStringHelper.ToThousandsNumberExpr(inputNumber, isMsea: true);
                default:
                    return this.DamageSkin.MiniUnit.Count > 0
                        ? ItemStringHelper.ToCJKNumberExpr(inputNumber)
                        : inputNumber.ToString();
            }
        }

        /// <summary>
        /// Gets the critical sign bitmap if available.
        /// </summary>
        private BitmapOrigin GetCriticalSign()
        {
            return this.damageSkin.BigCriticalDigit.TryGetValue("effect3", out var sign) ? sign : new BitmapOrigin();
        }

        /// <summary>
        /// Encapsulates all damage style parameters for Big/Mini and Critical variants.
        /// </summary>
        private class DamageStyleParams
        {
            public Dictionary<string, BitmapOrigin> NumsBig { get; set; }
            public Dictionary<string, BitmapOrigin> NumsMini { get; set; }
            public Dictionary<string, BitmapOrigin> UnitsBig { get; set; }
            public Dictionary<string, BitmapOrigin> UnitsMini { get; set; }
            public int DigitSpacingBig { get; set; }
            public int DigitSpacingMini { get; set; }
            public int UnitSpacingBig { get; set; }
            public int UnitSpacingMini { get; set; }
            public int UnitDotSpaceBig { get; set; }
            public int UnitDotSpaceMini { get; set; }
            public bool DigitBottomFixBig { get; set; }
            public bool DigitBottomFixMini { get; set; }
            public bool UnitBottomFixBig { get; set; }
            public bool UnitBottomFixMini { get; set; }
        }

        /// <summary>
        /// Gets damage style parameters based on critical state.
        /// </summary>
        private DamageStyleParams GetDamageStyleParameters(bool isCritical)
        {
            const int EXTRA_SPACING = 30;

            var numsBig = isCritical ? this.damageSkin.BigCriticalDigit : this.damageSkin.BigDigit;
            var numsMini = isCritical ? this.damageSkin.MiniCriticalDigit : this.damageSkin.MiniDigit;
            var unitsBig = isCritical ? this.damageSkin.BigCriticalUnit : this.damageSkin.BigUnit;
            var unitsMini = isCritical ? this.damageSkin.MiniCriticalUnit : this.damageSkin.MiniUnit;

            var digitSpacingBig = isCritical ? this.damageSkin.BigCriticalDigitSpacing : this.damageSkin.BigDigitSpacing;
            var digitSpacingMini = isCritical ? this.damageSkin.MiniCriticalDigitSpacing : this.damageSkin.MiniDigitSpacing;
            var unitSpacingBig = isCritical ? this.damageSkin.BigCriticalUnitSpacing : this.damageSkin.BigUnitSpacing;
            var unitSpacingMini = isCritical ? this.damageSkin.MiniCriticalUnitSpacing : this.damageSkin.MiniUnitSpacing;
            var unitDotSpaceBig = isCritical ? this.DamageSkin.BigCriticalUnitDotSpace : this.DamageSkin.BigUnitDotSpace;
            var unitDotSpaceMini = isCritical ? this.DamageSkin.MiniCriticalUnitDotSpace : this.DamageSkin.MiniUnitDotSpace;

            if (!UseInGameSpacing)
            {
                digitSpacingBig += EXTRA_SPACING;
                digitSpacingMini += EXTRA_SPACING;
                unitSpacingBig += EXTRA_SPACING;
                unitSpacingMini += EXTRA_SPACING;
                unitDotSpaceBig += EXTRA_SPACING;
                unitDotSpaceMini += EXTRA_SPACING;
            }

            return new DamageStyleParams
            {
                NumsBig = numsBig,
                NumsMini = numsMini,
                UnitsBig = unitsBig,
                UnitsMini = unitsMini,
                DigitSpacingBig = digitSpacingBig,
                DigitSpacingMini = digitSpacingMini,
                UnitSpacingBig = unitSpacingBig,
                UnitSpacingMini = unitSpacingMini,
                UnitDotSpaceBig = unitDotSpaceBig,
                UnitDotSpaceMini = unitDotSpaceMini,
                DigitBottomFixBig = isCritical ? this.DamageSkin.BigCriticalDigitBottomFix : this.DamageSkin.BigDigitBottomFix,
                DigitBottomFixMini = isCritical ? this.DamageSkin.MiniCriticalDigitBottomFix : this.DamageSkin.MiniDigitBottomFix,
                UnitBottomFixBig = isCritical ? this.DamageSkin.BigCriticalUnitBottomFix : this.DamageSkin.BigUnitBottomFix,
                UnitBottomFixMini = isCritical ? this.DamageSkin.MiniCriticalUnitBottomFix : this.DamageSkin.MiniUnitBottomFix,
            };
        }

        /// <summary>
        /// Initializes critical sign info if available.
        /// </summary>
        private DamageSkinDrawInfo InitializeCriticalInfo(BitmapOrigin criticalSign, ref DamageStyleParams damageStyleParams)
        {
            if (criticalSign.Bitmap != null)
            {
                return new DamageSkinDrawInfo(criticalSign, -criticalSign.Origin.X, -criticalSign.Origin.Y, false);
            }
            return null;
        }

        /// <summary>
        /// Maps unit characters to their corresponding unit keys.
        /// </summary>
        private static readonly Dictionary<char, string> UnitCharacterMap = new()
        {
            { '十', "0" }, { '십', "0" }, { '.', "0" },
            { '百', "1" }, { '백', "1" }, { 'K', "1" },
            { '千', "2" }, { '천', "2" }, { 'M', "2" },
            { '万', "3" }, { '萬', "3" }, { '만', "3" }, { 'B', "3" },
            { '億', "4" }, { '亿', "4" }, { '억', "4" }, { 'T', "4" },
            { '兆', "5" }, { '조', "5" }, { 'Q', "5" },
            { '京', "6" }, { '경', "6" }
        };

        /// <summary>
        /// Resolves unit bitmap and spacing for a given character.
        /// </summary>
        private (BitmapOrigin bitmap, int spacing, bool bottomFix) ResolveUnitCharacter(
            char character, bool isFirstUnit, DamageStyleParams damageStyleParams)
        {
            const int FALLBACK_SPACING = 10;
            const int FALLBACK_SIZE = 20;

            if (!UnitCharacterMap.TryGetValue(character, out var unitKey))
                return (new BitmapOrigin(), FALLBACK_SPACING, false);

            var unitBig = isFirstUnit ? damageStyleParams.UnitsBig : damageStyleParams.UnitsBig;
            var unitMini = !isFirstUnit ? damageStyleParams.UnitsMini : damageStyleParams.UnitsMini;
            var spacingBig = character == '.' ? damageStyleParams.UnitDotSpaceBig : damageStyleParams.UnitSpacingBig;
            var spacingMini = character == '.' ? damageStyleParams.UnitDotSpaceMini : damageStyleParams.UnitSpacingMini;

            if (isFirstUnit && unitBig.ContainsKey(unitKey))
            {
                return (unitBig[unitKey], spacingBig, damageStyleParams.UnitBottomFixBig);
            }
            if (!isFirstUnit && unitMini.ContainsKey(unitKey))
            {
                return (unitMini[unitKey], spacingMini, damageStyleParams.UnitBottomFixMini);
            }

            // Fallback for unavailable units
            return character switch
            {
                'T' => (new BitmapOrigin(Resource.Unit_T, FALLBACK_SIZE, FALLBACK_SIZE), FALLBACK_SPACING, true),
                '兆' or '조' => (new BitmapOrigin(Resource.Unit_E12, FALLBACK_SIZE, FALLBACK_SIZE), FALLBACK_SPACING, true),
                'Q' => (new BitmapOrigin(Resource.Unit_Q, FALLBACK_SIZE, FALLBACK_SIZE), FALLBACK_SPACING, true),
                '京' or '경' => (new BitmapOrigin(Resource.Unit_E16, FALLBACK_SIZE, FALLBACK_SIZE), FALLBACK_SPACING, true),
                _ => (new BitmapOrigin(), FALLBACK_SPACING, false)
            };
        }

        /// <summary>
        /// Builds the draw order by parsing the number string and resolving bitmap resources.
        /// </summary>
        private void BuildDrawOrder(string numberStr, List<DamageSkinDrawInfo> drawOrder,
            DamageStyleParams damageStyleParams, bool isCritical)
        {
            const int MAX_UNIT_WIDTH = 50;
            const int MAX_DIGIT_WIDTH = 35;
            const int WAVE_HEIGHT = 0;

            int totalWidth = 0;
            int maxHeight = 0;
            bool firstNum = true;
            bool firstUnit = true;
            int count = 0;

            foreach (char c in numberStr)
            {
                BitmapOrigin tmpBmp = new BitmapOrigin();
                int spacing = 0;
                bool bottomFix = false;
                bool isUnit = true;

                if (char.IsDigit(c))
                {
                    var nums = firstNum ? damageStyleParams.NumsBig : damageStyleParams.NumsMini;
                    var spacing_val = firstNum ? damageStyleParams.DigitSpacingBig : damageStyleParams.DigitSpacingMini;
                    var bottomFix_val = firstNum ? damageStyleParams.DigitBottomFixBig : damageStyleParams.DigitBottomFixMini;

                    if (nums.TryGetValue(c.ToString(), out var digit))
                    {
                        tmpBmp = digit;
                        spacing = spacing_val;
                        bottomFix = bottomFix_val;
                        isUnit = false;
                        firstNum = false;
                    }
                }
                else
                {
                    var (unitBmp, unitSpacing, unitBottomFix) = ResolveUnitCharacter(c, firstUnit, damageStyleParams);
                    if (unitBmp.Bitmap != null)
                    {
                        tmpBmp = unitBmp;
                        spacing = unitSpacing;
                        bottomFix = unitBottomFix;
                        firstUnit = false;
                    }
                }

                if (tmpBmp.Bitmap != null)
                {
                    var drawInfo = new DamageSkinDrawInfo(tmpBmp, totalWidth - tmpBmp.Origin.X,
                        0 - tmpBmp.Origin.Y - (count++ % 2 != 0 ? WAVE_HEIGHT : 0), isUnit, bottomFix);

                    if (drawInfo.BottomFix)
                        drawInfo.Y = -tmpBmp.Bitmap.Height;

                    int widthLimit = isUnit ? MAX_UNIT_WIDTH : MAX_DIGIT_WIDTH;
                    totalWidth += Math.Min(drawInfo.Width, widthLimit) + spacing;
                    maxHeight = Math.Max(maxHeight, drawInfo.Y + drawInfo.Height);

                    drawOrder.Add(drawInfo);
                }
            }
        }

        /// <summary>
        /// Renders the final damage bitmap from the draw order.
        /// </summary>
        private Bitmap RenderDamageBitmap(List<DamageSkinDrawInfo> drawOrder, DamageSkinDrawInfo critInfo, DamageStyleParams damageStyleParams)
        {
            if (drawOrder.Count == 0)
                return new Bitmap(1, 1);

            var firstDraw = drawOrder[0];
            int offsetX = -firstDraw.X;
            int offsetY = -drawOrder.Min(info => info.Y);

            var digitInfos = drawOrder.Where(info => !info.IsUnit).ToList();
            int digitBaseY1 = digitInfos.Count > 0 ? digitInfos.Min(info => info.Y) : 0;
            int digitBaseY2 = digitInfos.Count > 0 ? digitInfos.Max(info => info.Y + info.Height) : 0;

            if (critInfo != null)
            {
                critInfo.X = firstDraw.X + firstDraw.OriginX + critInfo.X;
                critInfo.Y = firstDraw.Y + firstDraw.OriginY + critInfo.Y;
                offsetX = Math.Max(offsetX, -critInfo.X);
                offsetY = Math.Max(offsetY, -critInfo.Y);
            }

            int totalWidth = drawOrder.Last().X + drawOrder.Last().Width + offsetX;
            int maxHeight = drawOrder.Max(info => info.Y + info.Height) + offsetY;

            var finalBitmap = new Bitmap(totalWidth, maxHeight);

            using (Graphics g = Graphics.FromImage(finalBitmap))
            {
                g.Clear(Color.Transparent);

                if (critInfo != null)
                {
                    g.DrawImage(critInfo.Bmp, offsetX + critInfo.X, offsetY + critInfo.Y);
                }

                foreach (var drawInfo in drawOrder)
                {
                    int drawY = offsetY + drawInfo.Y;
                    if (drawInfo.IsUnit && digitInfos.Count > 0)
                    {
                        drawY = offsetY + Math.Min(Math.Max(drawInfo.Y, digitBaseY1), digitBaseY2 - drawInfo.Height);
                    }
                    g.DrawImage(drawInfo.Bmp, offsetX + drawInfo.X, drawY);
                }
            }

            return finalBitmap;
        }

        public Bitmap GetUnit()
        {
            Bitmap unitBitmap = null;

            int width = 0;
            int height = 0;

            if (damageSkin.BigUnit.Count > 0)
            {
                if (UseMiniSize)
                {
                    foreach (var unit in damageSkin.MiniUnit.Values)
                    {
                        width += unit.Bitmap.Width;
                        height = Math.Max(height, unit.Bitmap.Height);
                        width += this.damageSkin.MiniUnitSpacing;
                    }
                    foreach (var unit in damageSkin.MiniCriticalUnit.Values)
                    {
                        width += unit.Bitmap.Width;
                        height = Math.Max(height, unit.Bitmap.Height);
                        width += this.damageSkin.MiniCriticalUnitSpacing;
                    }
                    unitBitmap = new Bitmap(width, height);
                    using (Graphics g = Graphics.FromImage(unitBitmap))
                    {
                        g.Clear(Color.Transparent);
                        int offsetX = 0;
                        foreach (var unit in damageSkin.MiniUnit.Values)
                        {
                            g.DrawImage(unit.Bitmap, offsetX, height - unit.Bitmap.Height);
                            offsetX += unit.Bitmap.Width;
                            offsetX += this.damageSkin.MiniUnitSpacing;
                        }
                        foreach (var unit in damageSkin.MiniCriticalUnit.Values)
                        {
                            g.DrawImage(unit.Bitmap, offsetX, height - unit.Bitmap.Height);
                            offsetX += unit.Bitmap.Width;
                            offsetX += this.damageSkin.MiniCriticalUnitSpacing;
                        }
                    }
                }
                else
                {
                    foreach (var unit in damageSkin.BigUnit.Values)
                    {
                        width += unit.Bitmap.Width;
                        height = Math.Max(height, unit.Bitmap.Height);
                        width += this.damageSkin.BigUnitSpacing;
                    }
                    foreach (var unit in damageSkin.BigCriticalUnit.Values)
                    {
                        width += unit.Bitmap.Width;
                        height = Math.Max(height, unit.Bitmap.Height);
                        width += this.damageSkin.BigCriticalUnitSpacing;
                    }
                    unitBitmap = new Bitmap(width, height);
                    using (Graphics g = Graphics.FromImage(unitBitmap))
                    {
                        g.Clear(Color.Transparent);
                        int offsetX = 0;
                        foreach (var unit in damageSkin.BigUnit.Values)
                        {
                            g.DrawImage(unit.Bitmap, offsetX, height - unit.Bitmap.Height);
                            offsetX += unit.Bitmap.Width;
                            offsetX += this.damageSkin.BigUnitSpacing;
                        }
                        foreach (var unit in damageSkin.BigCriticalUnit.Values)
                        {
                            g.DrawImage(unit.Bitmap, offsetX, height - unit.Bitmap.Height);
                            offsetX += unit.Bitmap.Width;
                            offsetX += this.damageSkin.BigCriticalUnitSpacing;
                        }
                    }
                }
            }
            return unitBitmap;
        }

        public Bitmap GetExtraEffect()
        {

            Bitmap[] originalBitmaps = new Bitmap[5]
            {
                this.damageSkin.MiniDigit.ContainsKey("Miss") ? this.damageSkin.MiniDigit?["Miss"].Bitmap : null,
                this.damageSkin.MiniDigit.ContainsKey("guard") ? this.damageSkin.MiniDigit?["guard"].Bitmap : null,
                this.damageSkin.MiniDigit.ContainsKey("resist") ? this.damageSkin.MiniDigit?["resist"].Bitmap : null,
                this.damageSkin.MiniDigit.ContainsKey("shot") ? this.damageSkin.MiniDigit?["shot"].Bitmap : null,
                this.damageSkin.MiniDigit.ContainsKey("counter") ? this.damageSkin.MiniDigit?["counter"].Bitmap : null
            };

            int width = 0;
            int height = 0;

            foreach (var bo in originalBitmaps)
            {
                if (bo == null) continue;
                width += bo.Width;
                height = Math.Max(height, bo.Height);
            }

            Bitmap bitmap = new Bitmap(width, height);

            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.Transparent);
                int offsetX = 0;
                for (int j = 0; j < originalBitmaps.Count(); j++)
                {
                    if (originalBitmaps[j] == null) continue;
                    g.DrawImage(originalBitmaps[j], offsetX, height - originalBitmaps[j].Height);
                    offsetX += originalBitmaps[j].Width;
                }
            }

            return bitmap;
        }

        private class DamageSkinDrawInfo
        {
            public DamageSkinDrawInfo(BitmapOrigin bo, int x, int y, bool isUnit, bool bottomFix = false)
            {
                BO = bo;
                X = x;
                Y = y;
                IsUnit = isUnit;
                BottomFix = bottomFix;
            }

            public BitmapOrigin BO;
            public int X;
            public int Y;
            public bool BottomFix;
            public Bitmap Bmp { get { return this.BO.Bitmap; } }
            public int Width { get { return this.Bmp?.Width ?? 0; } }
            public int Height { get { return this.Bmp?.Height ?? 0; } }
            public int OriginX { get { return this.BO.Origin.X; } }
            public int OriginY { get { return this.BO.Origin.Y; } }
            public bool IsUnit;
        }
    }
}
