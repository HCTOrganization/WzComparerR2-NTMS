using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text.RegularExpressions;
using WzComparerR2.CharaSim;
using WzComparerR2.Common;
using WzComparerR2.WzLib;
using WzComparerR2.AvatarCommon;
using static WzComparerR2.CharaSimControl.RenderHelper;

namespace WzComparerR2.CharaSimControl
{
    public class MobTooltipRenderer : TooltipRender
    {

        public MobTooltipRenderer()
        {
        }

        public override object TargetItem
        {
            get { return this.MobInfo; }
            set { this.MobInfo = value as Mob; }
        }

        public Mob MobInfo { get; set; }
        private AvatarCanvasManager avatar { get; set; }
        public override Bitmap Render()
        {
            if (MobInfo == null)
            {
                return null;
            }

            Bitmap bmp = new Bitmap(1, 1, PixelFormat.Format32bppArgb);
            Graphics g = Graphics.FromImage(bmp);
            bool isTranslateRequired = Translator.IsTranslateEnabled;

            //预绘制
            List<TextBlock> titleBlocks = new List<TextBlock>();

            if (MobInfo.ID > -1)
            {
                string mobName = GetMobName(MobInfo.ID);
                var block = PrepareText(g, mobName ?? "(null)", GearGraphics.ItemNameFont2, Brushes.White, 0, 0);
                titleBlocks.Add(block);
                block = PrepareText(g, "ID:" + MobInfo.ID, GearGraphics.ItemDetailFont, Brushes.White, block.Size.Width + 6, 0);
                titleBlocks.Add(block);
            }

            List<TextBlock> propBlocks = new List<TextBlock>();
            int picY = 0;

            StringBuilder sbExt = new StringBuilder();
            if (MobInfo.Boss && MobInfo.PartyBonusMob)
            {
                sbExt.Append("MiniBoss ");
            }
            if (MobInfo.Boss && !MobInfo.PartyBonusMob)
            {
                sbExt.Append("Boss ");
            }
            if (MobInfo.Undead)
            {
                sbExt.Append("不死系 ");
            }
            if (MobInfo.FirstAttack)
            {
                sbExt.Append("主動攻擊 ");
            }
            if (!MobInfo.BodyAttack)
            {
                sbExt.Append("無觸触傷害 ");
            }
            if (MobInfo.DamagedByMob)
            {
                sbExt.Append("弱小怪物 ");
            }
            if (MobInfo.ChangeableMob)
            {
                sbExt.Append("只受怪物傷害 ");
            }
            if (MobInfo.AllyMob)
            {
                sbExt.Append("同盟怪物 ");
            }
            if (MobInfo.Invincible)
            {
                sbExt.Append("無敵 ");
            }
            if (MobInfo.NotAttack)
            {
                sbExt.Append("無法攻擊 ");
            }
            if (MobInfo.FixedDamage > 0)
            {
                sbExt.Append("固定傷害 " + ToCJKNumberExpr(MobInfo.FixedDamage) + " ");
            }
            if (MobInfo.FixedBodyAttackDamageR > 0)
            {
                sbExt.Append("固定接触傷害: " + MobInfo.FixedBodyAttackDamageR + "% ");
            }
            if (MobInfo.IgnoreDamage)
            {
                sbExt.Append("無視傷害 ");
            }
            if (MobInfo.IgnoreMoveImpact)
            {
                sbExt.Append("無視眩暈 ");
            }
            if (MobInfo.IgnoreMovable)
            {
                sbExt.Append("無視束縛 ");
            }
            if (MobInfo.NoDebuff)
            {
                sbExt.Append("無視減益 ");
            }
            if (MobInfo.OnlyNormalAttack)
            {
                sbExt.Append("只能承受一般攻擊 ");
            }
            if (MobInfo.OnlyHittedByCommonAttack)
            {
                sbExt.Append("只能承受一般攻擊 ");
            }

            if (sbExt.Length > 1)
            {
                sbExt.Remove(sbExt.Length - 1, 1);
                propBlocks.Add(PrepareText(g, sbExt.ToString(), GearGraphics.ItemDetailFont, Brushes.GreenYellow, 0, picY));
                picY += 16;
            }

            if (MobInfo.RemoveAfter > 0)
            {
                propBlocks.Add(PrepareText(g, "[" + MobInfo.RemoveAfter + "秒後自動消失]", GearGraphics.ItemDetailFont, Brushes.GreenYellow, 0, picY));
                picY += 16;
            }

            propBlocks.Add(PrepareText(g, "種類: " + GetMobCategoryName(MobInfo.Category), GearGraphics.ItemDetailFont, Brushes.White, 0, picY));
            long fmaxhp, fmaxmp;
            string hpNum = long.TryParse(MobInfo.FinalMaxHP, out fmaxhp) ? ToCJKNumberExpr(fmaxhp) : ToCJKNumberExpr(MobInfo.MaxHP);
            string mpNum = long.TryParse(MobInfo.FinalMaxMP, out fmaxmp) ? ToCJKNumberExpr(fmaxmp) : ToCJKNumberExpr(MobInfo.MaxMP);
            if (MobInfo.ChangeableMob)
            {
                propBlocks.Add(PrepareText(g, "等級: 不定", GearGraphics.ItemDetailFont, Brushes.White, 0, picY += 16));
                propBlocks.Add(PrepareText(g, "HP: 不定", GearGraphics.ItemDetailFont, Brushes.White, 0, picY += 16));
                propBlocks.Add(PrepareText(g, "MP: 不定", GearGraphics.ItemDetailFont, Brushes.White, 0, picY += 16));
            }
            else
            {
                propBlocks.Add(PrepareText(g, "等級: " + MobInfo.Level, GearGraphics.ItemDetailFont, Brushes.White, 0, picY += 16));
                propBlocks.Add(PrepareText(g, "HP: " + hpNum, GearGraphics.ItemDetailFont, Brushes.White, 0, picY += 16));
                propBlocks.Add(PrepareText(g, "MP: " + mpNum, GearGraphics.ItemDetailFont, Brushes.White, 0, picY += 16));
            }
            if (MobInfo.HPRecovery > 0)
            {
                propBlocks.Add(PrepareText(g, "HP回復: " + ToCJKNumberExpr(MobInfo.HPRecovery), GearGraphics.ItemDetailFont, Brushes.White, 0, picY += 16));
            }
            if (MobInfo.MPRecovery > 0)
            {
                propBlocks.Add(PrepareText(g, "MP回復: " + ToCJKNumberExpr(MobInfo.MPRecovery), GearGraphics.ItemDetailFont, Brushes.White, 0, picY += 16));
            }
            propBlocks.Add(PrepareText(g, "物理傷害: " + ToCJKNumberExpr(MobInfo.PADamage), GearGraphics.ItemDetailFont, Brushes.White, 0, picY += 16));
            propBlocks.Add(PrepareText(g, "魔法傷害: " + ToCJKNumberExpr(MobInfo.MADamage), GearGraphics.ItemDetailFont, Brushes.White, 0, picY += 16));
            propBlocks.Add(PrepareText(g, "物理防御率: " + MobInfo.PDRate + "%", GearGraphics.ItemDetailFont, Brushes.White, 0, picY += 16));
            propBlocks.Add(PrepareText(g, "魔法防御率: " + MobInfo.MDRate + "%", GearGraphics.ItemDetailFont, Brushes.White, 0, picY += 16));
            propBlocks.Add(PrepareText(g, "擊退率: " + ToCJKNumberExpr(MobInfo.Pushed), GearGraphics.ItemDetailFont, Brushes.White, 0, picY += 16));
            propBlocks.Add(PrepareText(g, "經驗值: " + ToCJKNumberExpr(MobInfo.Exp), GearGraphics.ItemDetailFont, Brushes.White, 0, picY += 16));
            if (MobInfo.CharismaEXP > 0)
            {
                propBlocks.Add(PrepareText(g, "領導力: +" + MobInfo.CharismaEXP, GearGraphics.ItemDetailFont, Brushes.White, 0, picY += 16));
            }
            if (MobInfo.SenseEXP > 0)
            {
                propBlocks.Add(PrepareText(g, "感性: +" + MobInfo.SenseEXP, GearGraphics.ItemDetailFont, Brushes.White, 0, picY += 16));
            }
            if (MobInfo.InsightEXP > 0)
            {
                propBlocks.Add(PrepareText(g, "洞察力: +" + MobInfo.InsightEXP, GearGraphics.ItemDetailFont, Brushes.White, 0, picY += 16));
            }
            if (MobInfo.WillEXP > 0)
            {
                propBlocks.Add(PrepareText(g, "意志: +" + MobInfo.WillEXP, GearGraphics.ItemDetailFont, Brushes.White, 0, picY += 16));
            }
            if (MobInfo.CraftEXP > 0)
            {
                propBlocks.Add(PrepareText(g, "手藝: +" + MobInfo.CraftEXP, GearGraphics.ItemDetailFont, Brushes.White, 0, picY += 16));
            }
            if (MobInfo.CharmEXP > 0)
            {
                propBlocks.Add(PrepareText(g, "魅力: +" + MobInfo.CharmEXP, GearGraphics.ItemDetailFont, Brushes.White, 0, picY += 16));
            }
            if (MobInfo.WP > 0)
            {
                propBlocks.Add(PrepareText(g, "WP: " + MobInfo.WP, GearGraphics.ItemDetailFont, Brushes.White, 0, picY += 16));
            }
            //propBlocks.Add(PrepareText(g, GetElemAttrString(MobInfo.ElemAttr), GearGraphics.ItemDetailFont, Brushes.White, 0, picY += 16));
            if (GetElemAttrString(MobInfo.ElemAttr) != "")
            {
                propBlocks.Add(PrepareText(g, GetElemAttrString(MobInfo.ElemAttr), GearGraphics.ItemDetailFont, Brushes.White, 0, picY += 16));
            }
            if (MobInfo.AttackPower > 0)
            {
                propBlocks.Add(PrepareText(g, "入場最低戰鬥力 (單人): " + ToCJKNumberExpr(MobInfo.AttackPower), GearGraphics.ItemDetailFont, Brushes.White, 0, picY += 16));
            }
            if (MobInfo?.ID != null)
            {
                var locNode = PluginBase.PluginManager.FindWz("Etc\\MobLocation.img\\" + MobInfo.ID.ToString());
                if (locNode != null)
                {
                    propBlocks.Add(PrepareText(g, "位置: ", GearGraphics.ItemDetailFont, GearGraphics.LocationBrush, 0, picY += 30));
                    foreach (var locMapNode in locNode.Nodes)
                    {
                        int mapID = locMapNode.GetValueEx<int>(-1);
                        string mapName = null;
                        if (mapID >= 0)
                        {
                            mapName = GetMapName(mapID);
                        }
                        string mobLoc = string.Format(" - {0} ({1})", mapName ?? "null", mapID);

                        propBlocks.Add(PrepareText(g, mobLoc, Translator.IsKoreanStringPresent(mobLoc) ? GearGraphics.KMSItemDetailFont : GearGraphics.ItemDetailFont, GearGraphics.LocationBrush, 0, picY += 16));
                    }
                }
            }

            picY += 28;

            if (MobInfo.Revive.Count > 0)
            {
                Dictionary<int, int> reviveCounts = new Dictionary<int, int>();
                foreach (var reviveID in MobInfo.Revive)
                {
                    int count = 0;
                    reviveCounts.TryGetValue(reviveID, out count);
                    reviveCounts[reviveID] = count + 1;
                }

                StringBuilder sb = new StringBuilder();
                //sb.Append("Summons after death: ");
                sb.Append("死亡后召喚: ");
                int rowCount = 0;
                foreach (var kv in reviveCounts)
                {
                    if (rowCount++ > 0)
                    {
                        sb.AppendLine().Append("       ");
                    }
                    string mobName = GetMobName(kv.Key);
                    sb.AppendFormat("{0} ({1:D7})", mobName, kv.Key);
                    if (kv.Value > 1)
                    {
                        sb.Append(" * " + kv.Value);
                    }
                }

                propBlocks.Add(PrepareText(g, sb.ToString(), Translator.IsKoreanStringPresent(sb.ToString()) ? GearGraphics.KMSItemDetailFont : GearGraphics.ItemDetailFont, Brushes.GreenYellow, 0, picY));
            }
            g.Dispose();
            bmp.Dispose();

            //计算大小
            Rectangle titleRect = Measure(titleBlocks);
            Rectangle imgRect = Rectangle.Empty;
            Rectangle textRect = Measure(propBlocks);
            Bitmap mobImg = MobInfo.Default.Bitmap;
            if (MobInfo.IsAvatarLook)
            {
                if (this.avatar == null)
                {
                    this.avatar = new AvatarCanvasManager();
                }

                var skin = MobInfo.AvatarLook.Nodes["skin"].GetValueEx<int>(0);
                this.avatar.AddBodyFromSkin3(skin);

                foreach (var node in MobInfo.AvatarLook.Nodes)
                {
                    var gearID = node.GetValueEx<int>(0);
                    this.avatar.AddGear(gearID);
                }

                var img = this.avatar.GetBitmapOrigin();
                if (img.Bitmap != null)
                {
                    MobInfo.Default = img;
                    mobImg = img.Bitmap;
                }

                this.avatar.ClearCanvas();
            }
            if (mobImg != null)
            {
                if (mobImg.Width > 250 || mobImg.Height > 300) //进行缩放
                {
                    double scale = Math.Min((double)250 / mobImg.Width, (double)300 / mobImg.Height);
                    imgRect = new Rectangle(0, 0, (int)(mobImg.Width * scale), (int)(mobImg.Height * scale));
                }
                else
                {
                    imgRect = new Rectangle(0, 0, mobImg.Width, mobImg.Height);
                }
            }


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
            if (mobImg != null && !imgRect.IsEmpty)
            {
                g.DrawImage(mobImg, imgRect);
            }
            //绘制文本
            foreach (var item in propBlocks)
            {
                DrawText(g, item, textRect.Location);
            }
            g.Dispose();
            return bmp;
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

        private string GetElemAttrString(MobElemAttr elemAttr)
        {
            StringBuilder sb1 = new StringBuilder(),
                sb2 = new StringBuilder();

            sb1.Append("冰雷火毒聖暗物");
            sb2.Append(GetElemAttrResistString(elemAttr.I));
            sb2.Append(GetElemAttrResistString(elemAttr.L));
            sb2.Append(GetElemAttrResistString(elemAttr.F));
            sb2.Append(GetElemAttrResistString(elemAttr.S));
            sb2.Append(GetElemAttrResistString(elemAttr.H));
            sb2.Append(GetElemAttrResistString(elemAttr.D));
            sb2.Append(GetElemAttrResistString(elemAttr.P));
            sb1.AppendLine().Append(sb2.ToString());
            return sb1.ToString();
        }

        private string GetElemAttrResistString(ElemResistance resist)
        {
            string e = null;
            switch (resist)
            {
                case ElemResistance.Immune: e = "×"; break;
                case ElemResistance.Resist: e = "△"; break;
                case ElemResistance.Normal: e = "○"; break;
                case ElemResistance.Weak: e = "◎"; break;
            }
            return e ?? "  ";
        }

        public static string GetMobCategoryName(int category)
        {
            switch (category)
            {
                case 1: return "動物型";
                case 2: return "植物型";
                case 3: return "魚類型";
                case 4: return "爬蟲類型";
                case 5: return "精靈型";
                case 6: return "惡魔型";
                case 7: return "不死型";
                case 8: return "無機物型";
                default: return "無形态";
            }
        }

        private string AddCommaSeparators(string number)
        {
            return Regex.Replace(number, @"^(\d+?)(\d{3})+$", m =>
            {
                var sb = new StringBuilder();
                sb.Append(m.Result("$1"));
                foreach (Capture cap in m.Groups[2].Captures)
                {
                    sb.Append(",");
                    sb.Append(cap.ToString());
                }
                return sb.ToString();
            });
        }

        private static string ToCJKNumberExpr(long value)
        {
            var sb = new StringBuilder(32);
            string secondPart = value.ToString("N0");
            bool firstPart = true;
            if (value >= 1_0000_0000_0000_0000)
            {
                long part = value / 1_0000_0000_0000_0000;
                sb.AppendFormat("{0}京", part); // Korean: 교, Chinese+Japanese: 京
                value -= part * 1_0000_0000_0000_0000;
                firstPart = false;
            }
            if (value >= 1_0000_0000_0000)
            {
                long part = value / 1_0000_0000_0000;
                sb.Append(firstPart ? null : " ");
                sb.AppendFormat("{0}兆", part); // Korean: 조, Chinese+Japanese: 兆
                value -= part * 1_0000_0000_0000;
                firstPart = false;
            }
            if (value >= 1_0000_0000)
            {
                long part = value / 1_0000_0000;
                sb.Append(firstPart ? null : " ");
                sb.AppendFormat("{0}億", part); // Korean: 억, TradChinese+Japanese: 億, SimpChinese: 亿
                value -= part * 1_0000_0000;
                firstPart = false;
            }
            if (value >= 1_0000)
            {
                long part = value / 1_0000;
                sb.Append(firstPart ? null : " ");
                sb.AppendFormat("{0}萬", part); // Korean: 만, TradChinese: 萬, SimpChinese+Japanese: 万
                value -= part * 1_0000;
                firstPart = false;
            }
            if (value > 0)
            {
                sb.Append(firstPart ? null : " ");
                sb.AppendFormat("{0}", value);
            }

            return sb.Length > 0 ? sb.ToString() + " (" + secondPart + ")" : "0";
        }
        
        private string GetMapName(int mapID)
        {
            StringResult sr;
            if (this.StringLinker == null || !this.StringLinker.StringMap.TryGetValue(mapID, out sr))
            {
                return null;
            }
            return sr.Name;
        }
    }
}
