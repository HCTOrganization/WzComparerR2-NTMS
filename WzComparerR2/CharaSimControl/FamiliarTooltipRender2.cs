using System;
using System.Collections.Generic;
using System.Drawing;
using WzComparerR2.CharaSim;
using WzComparerR2.Common;
using WzComparerR2.PluginBase;
using static WzComparerR2.CharaSimControl.RenderHelper;
using Resource = CharaSimResource.Resource;

namespace WzComparerR2.CharaSimControl
{
    public class FamiliarTooltipRender2 : TooltipRender
    {
        // This class is for GMS/JMS version Familiar UI only.
        // For CMS/TMS version, check FamiliarTooltipRenderer.
        public FamiliarTooltipRender2()
        {
        }

        private Familiar familiar;

        public Familiar Familiar
        {
            get { return familiar; }
            set { familiar = value; }
        }

        public override object TargetItem
        {
            get { return this.familiar; }
            set { this.familiar = value as Familiar; }
        }

        public int? ItemID { get; set; }
        public int FamiliarTier { get; set; }
        public bool AllowOutOfBounds { get; set; }
        public bool UseAssembleUI { get; set; }

        public override Bitmap Render()
        {
            if (this.familiar == null)
            {
                return null;
            }

            Bitmap baseFamiliar = this.UseAssembleUI ? GeneratePostAssembleFamiliarCard() : GeneratePreAssembleFamiliarCard();

            Bitmap tooltip = new Bitmap(baseFamiliar.Width + 26, baseFamiliar.Height + 23);

            using (Graphics g = Graphics.FromImage(tooltip))
            {
                GearGraphics.DrawNewTooltipBack(g, 0, 0, tooltip.Width, tooltip.Height);
                g.DrawImage(baseFamiliar, 10, 10, new Rectangle(0, 0, baseFamiliar.Width, baseFamiliar.Height), GraphicsUnit.Pixel);
                if (this.ShowObjectID)
                {
                    GearGraphics.DrawGearDetailNumber(g, 3, 3, this.ItemID != null ? $"{((int)this.ItemID).ToString("d8")}" : $"{this.familiar.FamiliarID.ToString()}", true);
                }
            }

            return tooltip;
        }
        private string GetMobName(int mobID)
        {
            bool isTranslateRequired = Translator.IsTranslateEnabled;
            StringResult sr;
            if (this.StringLinker == null || !this.StringLinker.StringMob.TryGetValue(mobID, out sr))
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

        private Bitmap Resize(Bitmap sourceBmp, Point alignOrigin, Point mobOrigin, out int xOffset, out int yOffset, out int tDelta, out int bDelta, out int lDelta, out int rDelta)
        {
            tDelta = 0;
            bDelta = 0;
            lDelta = 0;
            rDelta = 0;
            xOffset = 0;
            yOffset = 0;

            if (sourceBmp == null)
            {
                return null;
            }

            xOffset = alignOrigin.X - mobOrigin.X;
            yOffset = alignOrigin.Y - mobOrigin.Y;

            if (this.AllowOutOfBounds)
            {
                lDelta = mobOrigin.X > alignOrigin.X ? mobOrigin.X - alignOrigin.X : 0;
                rDelta = sourceBmp.Width - mobOrigin.X > 154 ? sourceBmp.Width - mobOrigin.X - 154 : 0;
                tDelta = mobOrigin.Y > alignOrigin.Y ? mobOrigin.Y - alignOrigin.Y : 0;
                bDelta = sourceBmp.Height - mobOrigin.Y > 228 ? sourceBmp.Height - mobOrigin.Y - 228 : 0;
                return sourceBmp;
            }

            int rectangWidth = 0;
            int rectangHeight = 0;

            int maxWidth = this.UseAssembleUI ? 337 : 337;
            int maxHeight = this.UseAssembleUI ? 197 : 197;

            float ratioX = (float)maxWidth / sourceBmp.Width;
            float ratioY = (float)maxHeight / sourceBmp.Height;
            float ratio = Math.Min(ratioX, ratioY);

            if (ratio < 1.0f)
            {
                rectangWidth = (int)(sourceBmp.Width * ratio);
                rectangHeight = (int)(sourceBmp.Height * ratio);

                if (rectangWidth == maxWidth)
                {
                    xOffset = this.UseAssembleUI ? 22 : 22;
                    yOffset = alignOrigin.Y - (int)(mobOrigin.Y * ratio);
                }
                else
                {
                    xOffset = alignOrigin.X - (int)(mobOrigin.X * ratio);
                    yOffset = this.UseAssembleUI ? 49 : 49;
                }

                Bitmap resizedImage = new Bitmap(rectangWidth, rectangHeight);

                using (Graphics g = Graphics.FromImage(resizedImage))
                {
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    g.DrawImage(sourceBmp, 0, 0, rectangWidth, rectangHeight);
                }
                return resizedImage;
            }
            else
            {
                return sourceBmp;
            }
        }

        private Bitmap GeneratePreAssembleFamiliarCard()
        {
            // Get Mob image and name
            Mob mob = Mob.CreateFromNode(PluginManager.FindWz($@"Mob\{familiar.MobID.ToString().PadLeft(7, '0')}.img", this.SourceWzFile), PluginManager.FindWz);

            Point alignOrigin = new Point(190, 222);
            Point mobOrigin = new Point(0, 0);
            int mobXoffset = 0;
            int mobYoffset = 0;
            int tDelta = 0;
            int bDelta = 0;
            int lDelta = 0;
            int rDelta = 0;
            if (familiar.FamiliarCover.Bitmap != null)
            {
                mobOrigin = familiar.FamiliarCover.Origin;
            }
            else
            {
                mobOrigin = mob.Default.Origin;
            }

            Bitmap mobImg = Resize(familiar.FamiliarCover.Bitmap ?? mob.Default.Bitmap, alignOrigin, mobOrigin, out mobXoffset, out mobYoffset, out tDelta, out bDelta, out lDelta, out rDelta);

            Bitmap tooltip = new Bitmap(Resource.Familiar__InfoWnd_gradeside_backgrnd.Width + lDelta + rDelta, Resource.Familiar__InfoWnd_gradeside_backgrnd.Height + tDelta + bDelta);

            using (Graphics g = Graphics.FromImage(tooltip))
            {
                g.DrawImage(Resource.Familiar__InfoWnd_backgrnd, 2 + lDelta, 2 + tDelta, new Rectangle(0, 0, Resource.Familiar__InfoWnd_backgrnd.Width, Resource.Familiar__InfoWnd_backgrnd.Height), GraphicsUnit.Pixel);
                g.DrawImage(Resource.Familiar__InfoWnd_gradeside_backgrnd, lDelta, tDelta, new Rectangle(0, 0, Resource.Familiar__InfoWnd_gradeside_backgrnd.Width, Resource.Familiar__InfoWnd_gradeside_backgrnd.Height), GraphicsUnit.Pixel);
                switch (this.FamiliarTier)
                {
                    default:
                    case 0:
                        g.DrawImage(Resource.Familiar__InfoWnd_gradeside_graderank__0, lDelta, tDelta, new Rectangle(0, 0, Resource.Familiar__InfoWnd_gradeside_graderank__0.Width, Resource.Familiar__InfoWnd_gradeside_graderank__0.Height), GraphicsUnit.Pixel);
                        break;
                    case 1:
                        g.DrawImage(Resource.Familiar__InfoWnd_gradeside_graderank__1, lDelta, tDelta, new Rectangle(0, 0, Resource.Familiar__InfoWnd_gradeside_graderank__1.Width, Resource.Familiar__InfoWnd_gradeside_graderank__1.Height), GraphicsUnit.Pixel);
                        break;
                    case 2:
                    case 5:
                        g.DrawImage(Resource.Familiar__InfoWnd_gradeside_graderank__2, lDelta, tDelta, new Rectangle(0, 0, Resource.Familiar__InfoWnd_gradeside_graderank__2.Width, Resource.Familiar__InfoWnd_gradeside_graderank__2.Height), GraphicsUnit.Pixel);
                        break;
                    case 3:
                    case 6:
                        g.DrawImage(Resource.Familiar__InfoWnd_gradeside_graderank__3, lDelta, tDelta, new Rectangle(0, 0, Resource.Familiar__InfoWnd_gradeside_graderank__3.Width, Resource.Familiar__InfoWnd_gradeside_graderank__3.Height), GraphicsUnit.Pixel);
                        break;
                    case 4:
                    case 7:
                        g.DrawImage(Resource.Familiar__InfoWnd_gradeside_graderank__4, lDelta, tDelta, new Rectangle(0, 0, Resource.Familiar__InfoWnd_gradeside_graderank__4.Width, Resource.Familiar__InfoWnd_gradeside_graderank__4.Height), GraphicsUnit.Pixel);
                        break;
                }
                g.DrawImage(Resource.Familiar__InfoWnd_backgrnd2, 19 + lDelta, 46 + tDelta, new Rectangle(0, 0, Resource.Familiar__InfoWnd_backgrnd2.Width, Resource.Familiar__InfoWnd_backgrnd2.Height), GraphicsUnit.Pixel);

                // Mob Placement here
                g.DrawImage(mobImg, mobXoffset + lDelta, mobYoffset + tDelta, new Rectangle(0, 0, mobImg.Width, mobImg.Height), GraphicsUnit.Pixel);

                // Name tag placement here
                switch (this.FamiliarTier)
                {
                    default:
                    case 0:
                        g.DrawImage(Resource.Familiar__InfoWnd_gradename__0, 84 + lDelta, 16 + tDelta, new Rectangle(0, 0, Resource.Familiar__InfoWnd_gradename__0.Width, Resource.Familiar__InfoWnd_gradename__0.Height), GraphicsUnit.Pixel);
                        break;
                    case 1:
                        g.DrawImage(Resource.Familiar__InfoWnd_gradename__1, 84 + lDelta, 16 + tDelta, new Rectangle(0, 0, Resource.Familiar__InfoWnd_gradename__1.Width, Resource.Familiar__InfoWnd_gradename__1.Height), GraphicsUnit.Pixel);
                        break;
                    case 2:
                    case 5:
                        g.DrawImage(Resource.Familiar__InfoWnd_gradename__2, 84 + lDelta, 16 + tDelta, new Rectangle(0, 0, Resource.Familiar__InfoWnd_gradename__2.Width, Resource.Familiar__InfoWnd_gradename__2.Height), GraphicsUnit.Pixel);
                        break;
                    case 3:
                    case 6:
                        g.DrawImage(Resource.Familiar__InfoWnd_gradename__3, 84 + lDelta, 16 + tDelta, new Rectangle(0, 0, Resource.Familiar__InfoWnd_gradename__3.Width, Resource.Familiar__InfoWnd_gradename__3.Height), GraphicsUnit.Pixel);
                        break;
                    case 4:
                    case 7:
                        g.DrawImage(Resource.Familiar__InfoWnd_gradename__4, 84 + lDelta, 16 + tDelta, new Rectangle(0, 0, Resource.Familiar__InfoWnd_gradename__4.Width, Resource.Familiar__InfoWnd_gradename__4.Height), GraphicsUnit.Pixel);
                        break;
                }

                g.DrawImage(Resource.Familiar__InfoWnd_level_gauge_exp_backgrnd, 27 + lDelta, 31 + tDelta, new Rectangle(0, 0, Resource.Familiar__InfoWnd_level_gauge_exp_backgrnd.Width, Resource.Familiar__InfoWnd_level_gauge_exp_backgrnd.Height), GraphicsUnit.Pixel);
                g.DrawImage(Resource.Familiar__InfoWnd_level_lv_base, 19 + lDelta, 10 + tDelta, new Rectangle(0, 0, Resource.Familiar__InfoWnd_level_lv_base.Width, Resource.Familiar__InfoWnd_level_lv_base.Height), GraphicsUnit.Pixel);
                g.DrawImage(Resource.Familiar__InfoWnd_level_Lv_, 38 + lDelta, 18 + tDelta, new Rectangle(0, 0, Resource.Familiar__InfoWnd_level_Lv_.Width, Resource.Familiar__InfoWnd_level_Lv_.Height), GraphicsUnit.Pixel);
                g.DrawImage(Resource.Familiar__InfoWnd_Attribute_base, 324 + lDelta, 61 + tDelta, new Rectangle(0, 0, Resource.Familiar__InfoWnd_Attribute_base.Width, Resource.Familiar__InfoWnd_Attribute_base.Height), GraphicsUnit.Pixel);
                g.DrawImage(Resource.Familiar__InfoWnd_Attribute_base, 324 + lDelta, 92 + tDelta, new Rectangle(0, 0, Resource.Familiar__InfoWnd_Attribute_base.Width, Resource.Familiar__InfoWnd_Attribute_base.Height), GraphicsUnit.Pixel);

                g.DrawImage(Resource.Familiar__InfoWnd_gradegauge_info_base, 30 + lDelta, 253 + tDelta, new Rectangle(0, 0, Resource.Familiar__InfoWnd_backgrnd2.Width, Resource.Familiar__InfoWnd_backgrnd2.Height), GraphicsUnit.Pixel);
                switch (this.FamiliarTier)
                {
                    default:
                    case 0:
                        g.DrawImage(Resource.Familiar__InfoWnd_gradejewel_info_common_1, 23 + lDelta, 246 + tDelta, new Rectangle(0, 0, Resource.Familiar__InfoWnd_gradejewel_info_common_1.Width, Resource.Familiar__InfoWnd_gradejewel_info_common_1.Height), GraphicsUnit.Pixel);
                        break;
                    case 1:
                        g.DrawImage(Resource.Familiar__InfoWnd_gradejewel_info_rare_1, 23 + lDelta, 246 + tDelta, new Rectangle(0, 0, Resource.Familiar__InfoWnd_gradejewel_info_rare_1.Width, Resource.Familiar__InfoWnd_gradejewel_info_rare_1.Height), GraphicsUnit.Pixel);
                        break;
                    case 2:
                    case 5:
                        g.DrawImage(Resource.Familiar__InfoWnd_gradejewel_info_epic_1, 23 + lDelta, 246 + tDelta, new Rectangle(0, 0, Resource.Familiar__InfoWnd_gradejewel_info_epic_1.Width, Resource.Familiar__InfoWnd_gradejewel_info_epic_1.Height), GraphicsUnit.Pixel);
                        break;
                    case 3:
                    case 6:
                        g.DrawImage(Resource.Familiar__InfoWnd_gradejewel_info_unique_1, 23 + lDelta, 246 + tDelta, new Rectangle(0, 0, Resource.Familiar__InfoWnd_gradejewel_info_unique_1.Width, Resource.Familiar__InfoWnd_gradejewel_info_unique_1.Height), GraphicsUnit.Pixel);
                        break;
                    case 4:
                    case 7:
                        g.DrawImage(Resource.Familiar__InfoWnd_gradejewel_info_regendary_1, 23 + lDelta, 246 + tDelta, new Rectangle(0, 0, Resource.Familiar__InfoWnd_gradejewel_info_regendary_1.Width, Resource.Familiar__InfoWnd_gradejewel_info_regendary_1.Height), GraphicsUnit.Pixel);
                        break;
                }
                g.DrawImage(Resource.Familiar__InfoWnd_bt_Addtional_normal_0, 20 + lDelta, 269 + tDelta, new Rectangle(0, 0, Resource.Familiar__InfoWnd_bt_Addtional_normal_0.Width, Resource.Familiar__InfoWnd_bt_Addtional_normal_0.Height), GraphicsUnit.Pixel);
                g.DrawImage(Resource.Familiar__InfoWnd_att, 44 + lDelta, 342 + tDelta, new Rectangle(0, 0, Resource.Familiar__InfoWnd_att.Width, Resource.Familiar__InfoWnd_att.Height), GraphicsUnit.Pixel);
                g.DrawImage(Resource.Familiar__InfoWnd_def, 214 + lDelta, 342 + tDelta, new Rectangle(0, 0, Resource.Familiar__InfoWnd_def.Width, Resource.Familiar__InfoWnd_def.Height), GraphicsUnit.Pixel);

                // Draw Name
                List<TextBlock> titleBlocks = new List<TextBlock>();
                string mobName = GetMobName(mob.ID);
                var block = PrepareText(g, mobName ?? "(null)", GearGraphics.FamiliarNameFont, Brushes.White, 0, 0);
                titleBlocks.Add(block);

                foreach (var item in titleBlocks)
                {
                    DrawText(g, item, new Point(110 + lDelta, 24 + tDelta));
                }

                // Draw 1 at Level, ATT, DEF
                g.DrawString("1", GearGraphics.FamiliarLevelFont, Brushes.White, new Point(58 + lDelta, 19 + tDelta));
                g.DrawString("1", GearGraphics.FamiliarLevelFont, Brushes.White, new Point(91 + lDelta, 341 + tDelta));
                g.DrawString("1", GearGraphics.FamiliarLevelFont, Brushes.White, new Point(261 + lDelta, 341 + tDelta));

                // Draw Attribute
                Bitmap fAttribute = PreAssembleAttribute[familiar.FamiliarAttribute];
                Point attrPoint = PreAssembleAttributeOffsets[familiar.FamiliarAttribute];
                g.DrawImage(fAttribute, attrPoint.X + lDelta, attrPoint.Y + tDelta, new Rectangle(0, 0, fAttribute.Width, fAttribute.Height), GraphicsUnit.Pixel);

                // Draw Category
                Bitmap fCategory = PreAssembleCategory[familiar.FamiliarCategory];
                Point catPoint = PreAssembleCategoryOffsets[familiar.FamiliarCategory];
                g.DrawImage(fCategory, catPoint.X + lDelta, catPoint.Y + tDelta, new Rectangle(0, 0, fCategory.Width, fCategory.Height), GraphicsUnit.Pixel);
            }
            return tooltip;
        }

        private Dictionary<string, Bitmap> PreAssembleAttribute = new Dictionary<string, Bitmap>()
        {
            { "D", Resource.Familiar__InfoWnd_Attribute_AttrVariation_Dark },
            { "F", Resource.Familiar__InfoWnd_Attribute_AttrVariation_Fire },
            { "H", Resource.Familiar__InfoWnd_Attribute_AttrVariation_Holy },
            { "I", Resource.Familiar__InfoWnd_Attribute_AttrVariation_Ice },
            { "L", Resource.Familiar__InfoWnd_Attribute_AttrVariation_Lighting },
            { "N", Resource.Familiar__InfoWnd_Attribute_AttrVariation_Fire },
            { "P", Resource.Familiar__InfoWnd_Attribute_AttrVariation_Poison }
        };

        private Dictionary<string, Point> PreAssembleAttributeOffsets = new Dictionary<string, Point>()
        {
            { "D", new Point(334, 65) },
            { "F", new Point(337, 65) },
            { "H", new Point(332, 62) },
            { "I", new Point(334, 65) },
            { "L", new Point(335, 64) },
            { "N", new Point(337, 65) },
            { "P", new Point(336, 64) }
        };

        private List<Bitmap> PreAssembleCategory = new List<Bitmap>()
        {
            Resource.Familiar__InfoWnd_kind_KindVariation_Human,
            Resource.Familiar__InfoWnd_kind_KindVariation_Beast,
            Resource.Familiar__InfoWnd_kind_KindVariation_Plant,
            Resource.Familiar__InfoWnd_kind_KindVariation_Fish,
            Resource.Familiar__InfoWnd_kind_KindVariation_Reptile,
            Resource.Familiar__InfoWnd_kind_KindVariation_Nymph,
            Resource.Familiar__InfoWnd_kind_KindVariation_Devil,
            Resource.Familiar__InfoWnd_kind_KindVariation_Undead,
            Resource.Familiar__InfoWnd_kind_KindVariation_Machine
        };

        private List<Point> PreAssembleCategoryOffsets = new List<Point>()
        {
            new Point(336, 95), // Human
            new Point(335, 97), // Beast
            new Point(336, 98), // Plant
            new Point(336, 95), // Fish
            new Point(335, 97), // Reptile
            new Point(335, 97), // Nymph
            new Point(335, 96), // Devil
            new Point(335, 97), // Undead
            new Point(334, 96) // Machine
        };

        private Bitmap GeneratePostAssembleFamiliarCard()
        {
            // Get Mob image and name
            Mob mob = Mob.CreateFromNode(PluginManager.FindWz($@"Mob\{familiar.MobID.ToString().PadLeft(7, '0')}.img", this.SourceWzFile), PluginManager.FindWz);

            Point alignOrigin = new Point(201, 221);
            Point mobOrigin = new Point(0, 0);
            int mobXoffset = 0;
            int mobYoffset = 0;
            int tDelta = 0;
            int bDelta = 0;
            int lDelta = 0;
            int rDelta = 0;
            if (familiar.FamiliarCover.Bitmap != null)
            {
                mobOrigin = familiar.FamiliarCover.Origin;
            }
            else
            {
                mobOrigin = mob.Default.Origin;
            }

            Bitmap mobImg = Resize(familiar.FamiliarCover.Bitmap ?? mob.Default.Bitmap, alignOrigin, mobOrigin, out mobXoffset, out mobYoffset, out tDelta, out bDelta, out lDelta, out rDelta);

            Bitmap tooltip = new Bitmap(Resource.Familiar2025__InfoWnd_back.Width + lDelta + rDelta, Resource.Familiar2025__InfoWnd_back.Height + tDelta + bDelta);

            using (Graphics g = Graphics.FromImage(tooltip))
            {
                g.DrawImage(Resource.Familiar2025__InfoWnd_back, lDelta, tDelta, new Rectangle(0, 0, Resource.Familiar2025__InfoWnd_back.Width, Resource.Familiar2025__InfoWnd_back.Height), GraphicsUnit.Pixel);
                g.DrawImage(Resource.Familiar2025__InfoWnd_gradeSide_backgrnd, 2 + lDelta, 2 + tDelta, new Rectangle(0, 0, Resource.Familiar2025__InfoWnd_gradeSide_backgrnd.Width, Resource.Familiar2025__InfoWnd_gradeSide_backgrnd.Height), GraphicsUnit.Pixel);
                switch (this.FamiliarTier)
                {
                    default:
                    case 0:
                        g.DrawImage(Resource.Familiar2025__InfoWnd_gradeSide_graderank__0, 2 + lDelta, 2 + tDelta, new Rectangle(0, 0, Resource.Familiar2025__InfoWnd_gradeSide_graderank__0.Width, Resource.Familiar2025__InfoWnd_gradeSide_graderank__0.Height), GraphicsUnit.Pixel);
                        break;
                    case 1:
                        g.DrawImage(Resource.Familiar2025__InfoWnd_gradeSide_graderank__1, 2 + lDelta, 2 + tDelta, new Rectangle(0, 0, Resource.Familiar2025__InfoWnd_gradeSide_graderank__1.Width, Resource.Familiar2025__InfoWnd_gradeSide_graderank__1.Height), GraphicsUnit.Pixel);
                        break;
                    case 2:
                    case 5:
                        g.DrawImage(Resource.Familiar2025__InfoWnd_gradeSide_graderank__2, 2 + lDelta, 2 + tDelta, new Rectangle(0, 0, Resource.Familiar2025__InfoWnd_gradeSide_graderank__2.Width, Resource.Familiar2025__InfoWnd_gradeSide_graderank__2.Height), GraphicsUnit.Pixel);
                        break;
                    case 3:
                    case 6:
                        g.DrawImage(Resource.Familiar2025__InfoWnd_gradeSide_graderank__3, 2 + lDelta, 2 + tDelta, new Rectangle(0, 0, Resource.Familiar2025__InfoWnd_gradeSide_graderank__3.Width, Resource.Familiar2025__InfoWnd_gradeSide_graderank__3.Height), GraphicsUnit.Pixel);
                        break;
                    case 4:
                    case 7:
                        g.DrawImage(Resource.Familiar2025__InfoWnd_gradeSide_graderank__4, 2 + lDelta, 2 + tDelta, new Rectangle(0, 0, Resource.Familiar2025__InfoWnd_gradeSide_graderank__4.Width, Resource.Familiar2025__InfoWnd_gradeSide_graderank__4.Height), GraphicsUnit.Pixel);
                        break;
                }
                g.DrawImage(Resource.Familiar2025__InfoWnd_back_familiar, 12 + lDelta, 47 + tDelta, new Rectangle(0, 0, Resource.Familiar2025__InfoWnd_back_familiar.Width, Resource.Familiar2025__InfoWnd_back_familiar.Height), GraphicsUnit.Pixel);

                // Mob Placement here
                g.DrawImage(mobImg, mobXoffset + lDelta, mobYoffset + tDelta, new Rectangle(0, 0, mobImg.Width, mobImg.Height), GraphicsUnit.Pixel);

                // Name tag placement here
                switch (this.FamiliarTier)
                {
                    default:
                    case 0:
                        g.DrawImage(Resource.Familiar2025__InfoWnd_gradeName__0, 14 + lDelta, 14 + tDelta, new Rectangle(0, 0, Resource.Familiar2025__InfoWnd_gradeName__0.Width, Resource.Familiar2025__InfoWnd_gradeName__0.Height), GraphicsUnit.Pixel);
                        g.DrawImage(Resource.Familiar2025__InfoWnd_gradeJewel_info_common, 93 + lDelta, 249 + tDelta, new Rectangle(0, 0, Resource.Familiar2025__InfoWnd_gradeJewel_info_common.Width, Resource.Familiar2025__InfoWnd_gradeJewel_info_common.Height), GraphicsUnit.Pixel);
                        break;
                    case 1:
                        g.DrawImage(Resource.Familiar2025__InfoWnd_gradeName__1, 14 + lDelta, 14 + tDelta, new Rectangle(0, 0, Resource.Familiar2025__InfoWnd_gradeName__1.Width, Resource.Familiar2025__InfoWnd_gradeName__1.Height), GraphicsUnit.Pixel);
                        g.DrawImage(Resource.Familiar2025__InfoWnd_gradeJewel_info_rare, 93 + lDelta, 249 + tDelta, new Rectangle(0, 0, Resource.Familiar2025__InfoWnd_gradeJewel_info_rare.Width, Resource.Familiar2025__InfoWnd_gradeJewel_info_rare.Height), GraphicsUnit.Pixel);
                        break;
                    case 2:
                    case 5:
                        g.DrawImage(Resource.Familiar2025__InfoWnd_gradeName__2, 14 + lDelta, 14 + tDelta, new Rectangle(0, 0, Resource.Familiar2025__InfoWnd_gradeName__2.Width, Resource.Familiar2025__InfoWnd_gradeName__2.Height), GraphicsUnit.Pixel);
                        g.DrawImage(Resource.Familiar2025__InfoWnd_gradeJewel_info_epic, 93 + lDelta, 249 + tDelta, new Rectangle(0, 0, Resource.Familiar2025__InfoWnd_gradeJewel_info_epic.Width, Resource.Familiar2025__InfoWnd_gradeJewel_info_epic.Height), GraphicsUnit.Pixel);
                        break;
                    case 3:
                    case 6:
                        g.DrawImage(Resource.Familiar2025__InfoWnd_gradeName__3, 14 + lDelta, 14 + tDelta, new Rectangle(0, 0, Resource.Familiar2025__InfoWnd_gradeName__3.Width, Resource.Familiar2025__InfoWnd_gradeName__3.Height), GraphicsUnit.Pixel);
                        g.DrawImage(Resource.Familiar2025__InfoWnd_gradeJewel_info_unique, 93 + lDelta, 249 + tDelta, new Rectangle(0, 0, Resource.Familiar2025__InfoWnd_gradeJewel_info_unique.Width, Resource.Familiar2025__InfoWnd_gradeJewel_info_unique.Height), GraphicsUnit.Pixel);
                        break;
                    case 4:
                    case 7:
                        g.DrawImage(Resource.Familiar2025__InfoWnd_gradeName__4, 14 + lDelta, 14 + tDelta, new Rectangle(0, 0, Resource.Familiar2025__InfoWnd_gradeName__4.Width, Resource.Familiar2025__InfoWnd_gradeName__4.Height), GraphicsUnit.Pixel);
                        g.DrawImage(Resource.Familiar2025__InfoWnd_gradeJewel_info_legendary, 93 + lDelta, 249 + tDelta, new Rectangle(0, 0, Resource.Familiar2025__InfoWnd_gradeJewel_info_legendary.Width, Resource.Familiar2025__InfoWnd_gradeJewel_info_legendary.Height), GraphicsUnit.Pixel);
                        break;
                }
                g.DrawImage(Resource.Familiar2025__InfoWnd_back_spec, 22 + lDelta, 223 + tDelta, new Rectangle(0, 0, Resource.Familiar2025__InfoWnd_back_spec.Width, Resource.Familiar2025__InfoWnd_back_spec.Height), GraphicsUnit.Pixel);
                g.DrawImage(Resource.Familiar2025__InfoWnd_attribute_base, 315 + lDelta, 61 + tDelta, new Rectangle(0, 0, Resource.Familiar2025__InfoWnd_attribute_base.Width, Resource.Familiar2025__InfoWnd_attribute_base.Height), GraphicsUnit.Pixel);
                g.DrawImage(Resource.Familiar2025__InfoWnd_attribute_base, 315 + lDelta, 92 + tDelta, new Rectangle(0, 0, Resource.Familiar2025__InfoWnd_attribute_base.Width, Resource.Familiar2025__InfoWnd_attribute_base.Height), GraphicsUnit.Pixel);

                g.DrawImage(Resource.Familiar2025__InfoWnd_button_Potential_normal_0, 13 + lDelta, 274 + tDelta, new Rectangle(0, 0, Resource.Familiar2025__InfoWnd_button_Potential_normal_0.Width, Resource.Familiar2025__InfoWnd_button_Potential_normal_0.Height), GraphicsUnit.Pixel);

                // Draw Name
                List<TextBlock> titleBlocks = new List<TextBlock>();
                string mobName = GetMobName(mob.ID);
                var block = PrepareText(g, mobName ?? "(null)", GearGraphics.FamiliarNameFont, Brushes.White, 0, 0);
                titleBlocks.Add(block);

                foreach (var item in titleBlocks)
                {
                    DrawText(g, item, new Point(27 + lDelta, 23 + tDelta));
                }

                // Draw 1 at Level, ATT, DEF
                g.DrawString("1", GearGraphics.FamiliarLevelFont, Brushes.White, new Point(43 + lDelta, 227 + tDelta));
                g.DrawString("1", GearGraphics.FamiliarLevelFont, Brushes.White, new Point(285 + lDelta, 226 + tDelta));
                g.DrawString("1", GearGraphics.FamiliarLevelFont, Brushes.White, new Point(332 + lDelta, 226 + tDelta));

                // Draw Attribute
                Bitmap fAttribute = PostAssembleAttribute[familiar.FamiliarAttribute];
                Point attrPoint = PostAssembleAttributeOffsets[familiar.FamiliarAttribute];
                g.DrawImage(fAttribute, attrPoint.X + lDelta, attrPoint.Y + tDelta, new Rectangle(0, 0, fAttribute.Width, fAttribute.Height), GraphicsUnit.Pixel);

                // Draw Category
                Bitmap fCategory = PostAssembleCategory[familiar.FamiliarCategory];
                Point catPoint = PostAssembleCategoryOffsets[familiar.FamiliarCategory];
                g.DrawImage(fCategory, catPoint.X + lDelta, catPoint.Y + tDelta, new Rectangle(0, 0, fCategory.Width, fCategory.Height), GraphicsUnit.Pixel);
            }
            return tooltip;
        }

        private Dictionary<string, Bitmap> PostAssembleAttribute = new Dictionary<string, Bitmap>()
        {
            { "D", Resource.Familiar2025__InfoWnd_attribute_AttrVariation_Dark },
            { "F", Resource.Familiar2025__InfoWnd_attribute_AttrVariation_Fire },
            { "H", Resource.Familiar2025__InfoWnd_attribute_AttrVariation_Holy },
            { "I", Resource.Familiar2025__InfoWnd_attribute_AttrVariation_Ice },
            { "L", Resource.Familiar2025__InfoWnd_attribute_AttrVariation_Lighting },
            { "N", Resource.Familiar2025__InfoWnd_attribute_AttrVariation_Fire },
            { "P", Resource.Familiar2025__InfoWnd_attribute_AttrVariation_Poison }
        };

        private Dictionary<string, Point> PostAssembleAttributeOffsets = new Dictionary<string, Point>()
        {
            { "D", new Point(325, 65) },
            { "F", new Point(328, 65) },
            { "H", new Point(323, 62) },
            { "I", new Point(325, 65) },
            { "L", new Point(327, 66) },
            { "N", new Point(328, 65) },
            { "P", new Point(327, 64) }
        };

        private List<Bitmap> PostAssembleCategory = new List<Bitmap>()
        {
            Resource.Familiar2025__InfoWnd_kind_KindVariation_Human,
            Resource.Familiar2025__InfoWnd_kind_KindVariation_Beast,
            Resource.Familiar2025__InfoWnd_kind_KindVariation_Plant,
            Resource.Familiar2025__InfoWnd_kind_KindVariation_Fish,
            Resource.Familiar2025__InfoWnd_kind_KindVariation_Reptile,
            Resource.Familiar2025__InfoWnd_kind_KindVariation_Nymph,
            Resource.Familiar2025__InfoWnd_kind_KindVariation_Devil,
            Resource.Familiar2025__InfoWnd_kind_KindVariation_Undead,
            Resource.Familiar2025__InfoWnd_kind_KindVariation_Machine
        };

        private List<Point> PostAssembleCategoryOffsets = new List<Point>()
        {
            new Point(327, 95), // Human
            new Point(326, 97), // Beast
            new Point(327, 98), // Plant
            new Point(327, 95), // Fish
            new Point(326, 97), // Reptile
            new Point(326, 97), // Nymph
            new Point(326, 96), // Devil
            new Point(326, 97), // Undead
            new Point(325, 96) // Machine
        };
    }
}
