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
using static WzComparerR2.CharaSimControl.RenderHelper;

namespace WzComparerR2.CharaSimControl
{
    public class FamiliarTooltipRenderer : TooltipRender
    {
        // This class is for CMS/TMS version Familiar UI only.
        // For GMS/JMS version, check FamiliarTooltipRenderer2.
        public FamiliarTooltipRenderer()
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

        public override Bitmap Render()
        {
            if (this.familiar == null)
            {
                return null;
            }

            Bitmap tooltip = Resource.UIFamiliar_img_familiarCard_backgrnd;
            Graphics g = Graphics.FromImage(tooltip);

            // Get Mob image and name
            Mob mob = Mob.CreateFromNode(PluginManager.FindWz($@"Mob\{familiar.MobID.ToString().PadLeft(7, '0')}.img", this.SourceWzFile), PluginManager.FindWz);

            Bitmap mobImg = Crop(mob.Default.Bitmap, mob.Default.Rectangle);

            // Draw Familiar Card basic background
            g.DrawImage(Resource.UIFamiliar_img_familiarCard_base, 45, 37, new Rectangle(0, 0, Resource.UIFamiliar_img_familiarCard_base.Width, Resource.UIFamiliar_img_familiarCard_base.Height), GraphicsUnit.Pixel);
            g.DrawImage(Resource.UIFamiliar_img_familiarCard_name, 31, 222, new Rectangle(0, 0, Resource.UIFamiliar_img_familiarCard_name.Width, Resource.UIFamiliar_img_familiarCard_name.Height), GraphicsUnit.Pixel);

            // Draw Mob
            int mobXoffset = 160 - (mobImg.Width / 2);
            int mobYoffset = 200 - mobImg.Height;
            g.DrawImage(mobImg, mobXoffset, mobYoffset, new Rectangle(0, 0, mobImg.Width, mobImg.Height), GraphicsUnit.Pixel);

            g.DrawImage(Resource.UIFamiliar_img_jewel_backgrnd, 25, 21, new Rectangle(0, 0, Resource.UIFamiliar_img_jewel_backgrnd.Width, Resource.UIFamiliar_img_jewel_backgrnd.Height), GraphicsUnit.Pixel);
            g.DrawImage(Resource.UIFamiliar_img_jewel_normal_5, 30, 27, new Rectangle(0, 0, Resource.UIFamiliar_img_jewel_normal_5.Width, Resource.UIFamiliar_img_jewel_normal_5.Height), GraphicsUnit.Pixel);

            // Pre-Drawing
            List<TextBlock> titleBlocks = new List<TextBlock>();
            string mobName = GetMobName(mob.ID);
            var block = PrepareText(g, mobName ?? "(null)", GearGraphics.LevelBoldFont, Brushes.White, 0, 0);
            titleBlocks.Add(block);

            Rectangle titleRect = Measure(titleBlocks);

            int titleXoffset = Resource.UIFamiliar_img_familiarCard_name.Width >= Resource.UIFamiliar_img_familiarCard_name.Width ? (Resource.UIFamiliar_img_familiarCard_name.Width - titleRect.Width) / 2 : 0;
            int titleYoffset = (Resource.UIFamiliar_img_familiarCard_name.Height - titleRect.Height) / 2;

            foreach (var item in titleBlocks)
            {
                DrawText(g, item, new Point(31 + titleXoffset, 222 + titleYoffset));
            }
            

            // Layout
            int width = 0;

            if (this.ShowObjectID)
            {
                GearGraphics.DrawGearDetailNumber(g, 24, 24, this.ItemID != null ? $"{((int)this.ItemID).ToString("d8")}" : $"{this.familiar.FamiliarID.ToString()}", true);
            }

            g.Dispose();
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
                return Translator.MergeString(sr.Name, Translator.TranslateString(sr.Name, true), 1, false, true);
            }
            else
            {
                return sr.Name;
            }
        }

        private Bitmap Crop(Bitmap sourceBmp, Rectangle sourceHitbox)
        {

            if (sourceBmp == null)
                return null;

            int rectangXoffset = 0;
            int rectangYoffset = 0;
            int rectangWidth = 0;
            int rectangHeight = 0;

            if (sourceBmp.Width > Resource.UIFamiliar_img_familiarCard_base.Width - 18)
            {
                // rectangXoffset = (sourceBmp.Width - Resource.UIFamiliar_img_familiarCard_base.Width) / 2 + 9;
                rectangWidth = Resource.UIFamiliar_img_familiarCard_base.Width - 16;
            }
            else
            {
                rectangWidth = sourceBmp.Width;
            }

            if (sourceBmp.Height > Resource.UIFamiliar_img_familiarCard_base.Height - 36)
            {
                rectangYoffset = sourceBmp.Height - Resource.UIFamiliar_img_familiarCard_base.Height + 36;
                rectangHeight = Resource.UIFamiliar_img_familiarCard_base.Height - 36;
            }
            else
            {
                rectangHeight = sourceBmp.Height;
            }

            return sourceBmp.Clone(new Rectangle(rectangXoffset, rectangYoffset, rectangWidth, rectangHeight), sourceBmp.PixelFormat);
        }
    }
}
