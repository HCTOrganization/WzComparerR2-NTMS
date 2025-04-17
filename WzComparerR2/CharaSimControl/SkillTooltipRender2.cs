using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using Resource = CharaSimResource.Resource;
using WzComparerR2.Common;
using WzComparerR2.CharaSim;
using WzComparerR2.WzLib;
using System.Text.RegularExpressions;
using System.Linq;

namespace WzComparerR2.CharaSimControl
{
    public class SkillTooltipRender2 : TooltipRender
    {
        public SkillTooltipRender2()
        {
        }

        public Skill Skill { get; set; }

        public override object TargetItem
        {
            get { return this.Skill; }
            set { this.Skill = value as Skill; }
        }

        public bool ShowProperties { get; set; } = true;
        public bool ShowDelay { get; set; }
        public bool ShowReqSkill { get; set; } = true;
        public bool DisplayCooltimeMSAsSec { get; set; } = true;
        public bool DisplayPermyriadAsPercent { get; set; } = true;
        public bool IgnoreEvalError { get; set; } = false;
        public bool ShowRangeCoordinates { get; set; } = true;
        public bool IsWideMode { get; set; } = true;
        public bool Enable22AniStyle { get; set; }
        public Dictionary<string, List<string>> DiffSkillTags { get; set; } = new Dictionary<string, List<string>>();
        public Wz_Node wzNode { get; set; } = null;

        public TooltipRender LinkRidingGearRender { get; set; }
        public string ParsedHdesc { get; set; }

        public override Bitmap Render()
        {
            return Render(false);
        }
        public Bitmap Render(bool doHighlight)
        {
            if (this.Skill == null)
            {
                return null;
            }

            CanvasRegion region = this.IsWideMode ? (this.Enable22AniStyle ? CanvasRegion._22AniWide : CanvasRegion.Wide) : (this.Enable22AniStyle ? CanvasRegion._22AniOriginal : CanvasRegion.Original);

            int picHeight;
            List<int> splitterH;
            Bitmap originBmp = RenderSkill(region, out picHeight, out splitterH, doHighlight);
            Bitmap ridingGearBmp = null;

            int vehicleID = Skill.VehicleID;
            if (vehicleID == 0)
            {
                vehicleID = PluginBase.PluginManager.FindWz(string.Format(@"Skill\RidingSkillInfo.img\{0:D7}\vehicleID", Skill.SkillID)).GetValueEx<int>(0);
            }
            if (vehicleID != 0)
            {
                Wz_Node imgNode = PluginBase.PluginManager.FindWz(string.Format(@"Character\TamingMob\{0:D8}.img", vehicleID));
                if (imgNode != null)
                {
                    Gear gear = Gear.CreateFromNode(imgNode, path => PluginBase.PluginManager.FindWz(path));
                    if (gear != null)
                    {
                        ridingGearBmp = RenderLinkRidingGear(gear);
                    }
                }
            }

            Size totalSize = new Size(originBmp.Width, picHeight);
            Point ridingGearOrigin = Point.Empty;

            if (ridingGearBmp != null)
            {
                totalSize.Width += ridingGearBmp.Width;
                totalSize.Height = Math.Max(picHeight, ridingGearBmp.Height);
                ridingGearOrigin.X = originBmp.Width;
            }

            Bitmap tooltip = new Bitmap(totalSize.Width, totalSize.Height);
            Graphics g = Graphics.FromImage(tooltip);

            //绘制背景区域
            GearGraphics.DrawNewTooltipBack(g, 0, 0, originBmp.Width, picHeight);
            if (splitterH != null && splitterH.Count > 0)
            {
                g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
                foreach (var y in splitterH)
                {
                    DrawV6SkillDotline(g, region.SplitterX1, region.SplitterX2, y);
                }
                g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;
            }

            //复制图像
            g.DrawImage(originBmp, 0, 0, new Rectangle(0, 0, originBmp.Width, picHeight), GraphicsUnit.Pixel);

            //左上角
            if (!Enable22AniStyle) g.DrawImage(Resource.UIToolTip_img_Skill_Frame_cover, 3, 3);

            if (this.ShowObjectID)
            {
                GearGraphics.DrawGearDetailNumber(g, 3, 3, Skill.SkillID.ToString("d7"), true);
            }

            if (ridingGearBmp != null)
            {
                g.DrawImage(ridingGearBmp, ridingGearOrigin.X, ridingGearOrigin.Y,
                    new Rectangle(Point.Empty, ridingGearBmp.Size), GraphicsUnit.Pixel);
            }

            if (originBmp != null)
                originBmp.Dispose();
            if (ridingGearBmp != null)
                ridingGearBmp.Dispose();

            g.Dispose();
            return tooltip;
        }

        private Bitmap RenderSkill(CanvasRegion region, out int picH, out List<int> splitterH, bool doHighlight = false)
        {
            Bitmap bitmap = new Bitmap(region.Width, DefaultPicHeight);
            Graphics g = Graphics.FromImage(bitmap);
            StringFormat format = (StringFormat)StringFormat.GenericDefault.Clone();
            var v6SkillSummaryFontColorTable = new Dictionary<string, Color>()
            {
                { "c", GearGraphics.SkillSummaryOrangeTextColor },
                { "$g", GearGraphics.gearCyanColor },
            };

            picH = 0;
            splitterH = new List<int>();
            string skillIDstr = Skill.SkillID.ToString().PadLeft(7, '0');

            //获取文字
            StringResult sr;
            if (StringLinker == null || !StringLinker.StringSkill.TryGetValue(Skill.SkillID, out sr))
            {
                sr = new StringResultSkill();
                sr.Name = "(null)";
            }

            bool isTranslateRequired = Translator.IsTranslateEnabled;
            bool isNewLineRequired = false;
            string translatedSkillName = "";
            if (isTranslateRequired)
            {
                translatedSkillName = Translator.TranslateString(sr.Name, true);
                SizeF titleSize;
                if (Translator.IsKoreanStringPresent(translatedSkillName + sr.Name))
                {
                    titleSize = TextRenderer.MeasureText(g, translatedSkillName + " (" + sr.Name + ")", GearGraphics.KMSItemNameFont, new Size(int.MaxValue, int.MaxValue), TextFormatFlags.NoPrefix);
                }
                else
                {
                    titleSize = TextRenderer.MeasureText(g, translatedSkillName + " (" + sr.Name + ")", GearGraphics.ItemNameFont2, new Size(int.MaxValue, int.MaxValue), TextFormatFlags.NoPrefix);
                }
                if (titleSize.Width > (int)(0.96 * region.Width))
                {
                    isNewLineRequired = true;
                }
            }

            // for 6th job skills
            if (Skill.Origin)
            {
                g.DrawImage(Resource.UIWindow2_img_Skill_skillTypeIcon_origin, 16, 11);
            }

            //绘制技能名称
            format.Alignment = StringAlignment.Center;
            if (isTranslateRequired)
            {
                string mergedSkillName;
                if (isNewLineRequired)
                {
                    mergedSkillName = Translator.MergeString(sr.Name, translatedSkillName, 1, false, true);
                }
                else
                {
                    mergedSkillName = Translator.MergeString(sr.Name, translatedSkillName, 0, false, true);
                }
                if (Translator.IsKoreanStringPresent(translatedSkillName + sr.Name))
                {
                    TextRenderer.DrawText(g, mergedSkillName, GearGraphics.KMSItemNameFont, new Point(bitmap.Width, 10), Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.NoPrefix);
                }
                else
                {
                    TextRenderer.DrawText(g, mergedSkillName, GearGraphics.ItemNameFont2, new Point(bitmap.Width, 10), Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.NoPrefix);
                }
                if (translatedSkillName.Contains(Environment.NewLine))
                {
                    picH += 30;
                }
            }
            else
            {
                if (Translator.IsKoreanStringPresent(sr.Name))
                {
                    TextRenderer.DrawText(g, sr.Name, GearGraphics.KMSItemNameFont, new Point(bitmap.Width, 10), Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.NoPrefix);
                }
                else
                {
                    TextRenderer.DrawText(g, sr.Name, GearGraphics.ItemNameFont2, new Point(bitmap.Width, 10), Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.NoPrefix);
                }
            }


            //绘制图标
            if (Skill.Icon.Bitmap != null)
            {
                picH = 33;
                g.DrawImage(Resource.UIToolTip_img_Skill_Frame_iconBackgrnd, 13, picH - 2);
                g.DrawImage(GearGraphics.EnlargeBitmap(Skill.Icon.Bitmap),
                15 + (1 - Skill.Icon.Origin.X) * 2,
                picH + (33 - Skill.Icon.Bitmap.Height) * 2);
            }



            //绘制desc
            picH = 35;
            if (Skill.HyperStat)
                GearGraphics.DrawString(g, "[等級上限：" + Skill.MaxLevel + "]", GearGraphics.ItemDetailFont, region.LevelDescLeft, region.TextRight, ref picH, 16);
            else if (!Skill.PreBBSkill)
                GearGraphics.DrawString(g, "[精通等級：" + Skill.MaxLevel + "]", GearGraphics.ItemDetailFont, region.SkillDescLeft, region.TextRight, ref picH, 16);

            if (sr.Desc != null)
            {
                string hdesc = SummaryParser.GetSkillSummary(sr.Desc, Skill.Level, Skill.Common, SummaryParams.Default);
                if (isTranslateRequired)
                {
                    string mergedDescString = Translator.MergeString(hdesc, Translator.TranslateString(hdesc), 2);
                    if (Translator.IsKoreanStringPresent(mergedDescString))
                    {
                        GearGraphics.DrawString(g, mergedDescString, GearGraphics.KMSItemDetailFont, v6SkillSummaryFontColorTable, Skill.Icon.Bitmap == null ? region.LevelDescLeft : region.SkillDescLeft, region.TextRight, ref picH, 16);
                    }
                    else
                    {
                        GearGraphics.DrawString(g, mergedDescString, GearGraphics.ItemDetailFont, v6SkillSummaryFontColorTable, Skill.Icon.Bitmap == null ? region.LevelDescLeft : region.SkillDescLeft, region.TextRight, ref picH, 16);
                    }
                }
                else
                {
                    if (Translator.IsKoreanStringPresent(hdesc))
                    {
                        GearGraphics.DrawString(g, hdesc, GearGraphics.KMSItemDetailFont, v6SkillSummaryFontColorTable, Skill.Icon.Bitmap == null ? region.LevelDescLeft : region.SkillDescLeft, region.TextRight, ref picH, 16);
                    }
                    else
                    {
                        GearGraphics.DrawString(g, hdesc, GearGraphics.ItemDetailFont, v6SkillSummaryFontColorTable, Skill.Icon.Bitmap == null ? region.LevelDescLeft : region.SkillDescLeft, region.TextRight, ref picH, 16);
                    }
                }
                //string hStr = SummaryParser.GetSkillSummary(skill, skill.Level, sr, SummaryParams.Default);

            }
            if (Skill.TimeLimited)
            {
                DateTime time = DateTime.Now.AddDays(7d);
                string expireStr = time.ToString("有效期 : yyyy年 M月 d日 HH時 mm分");
                // GearGraphics.DrawString(g, "#c" + expireStr + "#", GearGraphics.ItemDetailFont, Skill.Icon.Bitmap == null ? region.LevelDescLeft : region.SkillDescLeft, region.TextRight, ref picH, 16);
                GearGraphics.DrawString(g, "#c" + expireStr + "#", GearGraphics.ItemDetailFont2, v6SkillSummaryFontColorTable, Skill.Icon.Bitmap == null ? region.LevelDescLeft : region.SkillDescLeft, region.TextRight, ref picH, 16);
            }
            if (Skill.RelationSkill != null)
            {
                StringResult sr2 = null;
                if (StringLinker == null || !StringLinker.StringSkill.TryGetValue(Skill.RelationSkill.Item1, out sr2))
                {
                    sr2 = new StringResultSkill();
                    sr2.Name = "(null)";
                }
                DateTime time = DateTime.Now.AddMinutes(Skill.RelationSkill.Item2);
                string expireStr = time.ToString("有效期 : yyyy年 M月 d日 H時 m分");
                // GearGraphics.DrawString(g, "#c" + sr2.Name + "の " + expireStr + "#", GearGraphics.ItemDetailFont, Skill.Icon.Bitmap == null ? region.LevelDescLeft : region.SkillDescLeft, region.TextRight, ref picH, 16);
                GearGraphics.DrawString(g, "#c" + sr2.Name + "の " + expireStr + "#", GearGraphics.ItemDetailFont2, v6SkillSummaryFontColorTable, Skill.Icon.Bitmap == null ? region.LevelDescLeft : region.SkillDescLeft, region.TextRight, ref picH, 16);
            }
            if (Skill.IsSequenceOn)
            {
                if (doHighlight && DiffSkillTags.ContainsKey(skillIDstr) && DiffSkillTags[skillIDstr].Contains("isSequenceOn"))
                {
                    GearGraphics.DrawString(g, "#$g可登入技能順序#", GearGraphics.ItemDetailFont2, v6SkillSummaryFontColorTable, Skill.Icon.Bitmap == null ? region.LevelDescLeft : region.SkillDescLeft, region.TextRight, ref picH, 16);
                }
                else
                {
                    GearGraphics.DrawString(g, "#c可登入技能順序#", GearGraphics.ItemDetailFont2, v6SkillSummaryFontColorTable, Skill.Icon.Bitmap == null ? region.LevelDescLeft : region.SkillDescLeft, region.TextRight, ref picH, 16);
                }
            }
            if (Skill.IsPetAutoBuff)
            {
                if (doHighlight && DiffSkillTags.ContainsKey(skillIDstr) && DiffSkillTags[skillIDstr].Contains("isPetAutoBuff"))
                {
                    GearGraphics.DrawString(g, "#$g可登入寵物Buff自動技能#", GearGraphics.ItemDetailFont2, v6SkillSummaryFontColorTable, Skill.Icon.Bitmap == null ? region.LevelDescLeft : region.SkillDescLeft, region.TextRight, ref picH, 16);
                }
                else
                {
                    GearGraphics.DrawString(g, "#c可登入寵物Buff自動技能#", GearGraphics.ItemDetailFont2, v6SkillSummaryFontColorTable, Skill.Icon.Bitmap == null ? region.LevelDescLeft : region.SkillDescLeft, region.TextRight, ref picH, 16);
                }
            }
            /*if (Skill.ReqLevel > 0)
            {
                GearGraphics.DrawString(g, "#c[要求等级：" + Skill.ReqLevel.ToString() + "]#", GearGraphics.ItemDetailFont, region.SkillDescLeft, region.TextRight, ref picH, 16);
            }
            if (Skill.ReqAmount > 0)
            {
                GearGraphics.DrawString(g, "#c" + ItemStringHelper.GetSkillReqAmount(Skill.SkillID, Skill.ReqAmount) + "#", GearGraphics.ItemDetailFont, region.SkillDescLeft, region.TextRight, ref picH, 16);
            }*/
            picH += 13;

            //delay rendering v6 splitter
            picH = Math.Max(picH, 114);
            splitterH.Add(picH);
            picH += 15;

            var skillSummaryOptions = new SkillSummaryOptions
            {
                ConvertCooltimeMS = this.DisplayCooltimeMSAsSec,
                ConvertPerM = this.DisplayPermyriadAsPercent,
                IgnoreEvalError = this.IgnoreEvalError,
                EndColorOnNewLine = true,
            };

            if (Skill.Level > 0)
            {

                // 스킬 변경점에 초록색 칠하기
                if (doHighlight)
                {

                    if (Skill.SkillID / 100000 == 4000)
                    {
                        if (Skill.VSkillValue == 2) Skill.Level = 60;
                        if (Skill.VSkillValue == 1) Skill.Level = 30;
                    }
                }
                string hStr = SummaryParser.GetSkillSummary(Skill, Skill.Level, sr, SummaryParams.Default, skillSummaryOptions, doHighlight, skillIDstr, this.DiffSkillTags);

                GearGraphics.DrawString(g, "[現在等級" + Skill.Level + "]", GearGraphics.ItemDetailFont, region.LevelDescLeft, region.TextRight, ref picH, 16);
                if (Skill.SkillID / 10000 / 1000 == 10 && Skill.Level == 1 && Skill.ReqLevel > 0)
                {
                    GearGraphics.DrawPlainText(g, "[必要等級: " + Skill.ReqLevel.ToString() + "等級以上]", GearGraphics.ItemDetailFont, GearGraphics.skillYellowColor, region.LevelDescLeft, region.TextRight, ref picH, 16);
                }
                if (hStr != null)
                {
                    ParsedHdesc = hStr;
                    if (isTranslateRequired)
                    {
                        string mergedhStr = Translator.MergeString(hStr, Translator.TranslateString(hStr), 2);
                        if (Translator.IsKoreanStringPresent(mergedhStr))
                        {
                            GearGraphics.DrawString(g, mergedhStr, GearGraphics.KMSItemDetailFont, v6SkillSummaryFontColorTable, region.LevelDescLeft, region.TextRight, ref picH, 16);
                        }
                        else
                        {
                            GearGraphics.DrawString(g, mergedhStr, GearGraphics.ItemDetailFont, v6SkillSummaryFontColorTable, region.LevelDescLeft, region.TextRight, ref picH, 16);
                        }
                    }
                    else
                    {

                        if (Translator.IsKoreanStringPresent(hStr))
                        {
                            GearGraphics.DrawString(g, hStr, GearGraphics.KMSItemDetailFont, v6SkillSummaryFontColorTable, region.LevelDescLeft, region.TextRight, ref picH, 16);
                        }
                        else
                        {
                            GearGraphics.DrawString(g, hStr, GearGraphics.ItemDetailFont, v6SkillSummaryFontColorTable, region.LevelDescLeft, region.TextRight, ref picH, 16);
                        }
                    }
                }
            }

            if (Skill.Level < Skill.MaxLevel && !Skill.DisableNextLevelInfo)
            {
                string hStr = SummaryParser.GetSkillSummary(Skill, Skill.Level + 1, sr, SummaryParams.Default, new SkillSummaryOptions
                {
                    ConvertCooltimeMS = this.DisplayCooltimeMSAsSec,
                    ConvertPerM = this.DisplayPermyriadAsPercent,
                    IgnoreEvalError = this.IgnoreEvalError,
                });
                GearGraphics.DrawString(g, "[下次等級" + (Skill.Level + 1) + "]", GearGraphics.ItemDetailFont, region.LevelDescLeft, region.TextRight, ref picH, 16);
                if (Skill.SkillID / 10000 / 1000 == 10 && (Skill.Level + 1) == 1 && Skill.ReqLevel > 0)
                {
                    GearGraphics.DrawPlainText(g, "[所需等級: " + Skill.ReqLevel.ToString() + "等級以上]", GearGraphics.ItemDetailFont, GearGraphics.skillYellowColor, region.LevelDescLeft, region.TextRight, ref picH, 16);
                }
                if (hStr != null)
                {
                    if (isTranslateRequired)
                    {
                        string mergedhStr = Translator.MergeString(hStr, Translator.TranslateString(hStr), 2);
                        if (Translator.IsKoreanStringPresent(mergedhStr))
                        {
                            GearGraphics.DrawString(g, mergedhStr, GearGraphics.KMSItemDetailFont, v6SkillSummaryFontColorTable, region.LevelDescLeft, region.TextRight, ref picH, 16);
                        }
                        else
                        {
                            GearGraphics.DrawString(g, mergedhStr, GearGraphics.ItemDetailFont, v6SkillSummaryFontColorTable, region.LevelDescLeft, region.TextRight, ref picH, 16);
                        }
                    }
                    else
                    {

                        if (Translator.IsKoreanStringPresent(hStr))
                        {
                            GearGraphics.DrawString(g, hStr, GearGraphics.KMSItemDetailFont, v6SkillSummaryFontColorTable, region.LevelDescLeft, region.TextRight, ref picH, 16);
                        }
                        else
                        {
                            GearGraphics.DrawString(g, hStr, GearGraphics.ItemDetailFont, v6SkillSummaryFontColorTable, region.LevelDescLeft, region.TextRight, ref picH, 16);
                        }
                    }
                }
            }
            picH += 3;

            if (Skill.AddAttackToolTipDescSkill != 0)
            {
                //delay rendering v6 splitter
                splitterH.Add(picH);
                picH += 15;
                GearGraphics.DrawPlainText(g, "[組合技能]", GearGraphics.ItemDetailFont, Color.FromArgb(119, 204, 255), region.LevelDescLeft, region.TextRight, ref picH, 16);
                picH += 4;
                BitmapOrigin icon = new BitmapOrigin();
                Wz_Node skillNode = PluginBase.PluginManager.FindWz(string.Format(@"Skill\{0}.img\skill\{1}", Skill.AddAttackToolTipDescSkill / 10000, Skill.AddAttackToolTipDescSkill));
                if (skillNode != null)
                {
                    Skill skill = Skill.CreateFromNode(skillNode, PluginBase.PluginManager.FindWz);
                    icon = skill.Icon;
                }
                if (icon.Bitmap != null)
                {
                    g.DrawImage(icon.Bitmap, 13 - icon.Origin.X, picH + 32 - icon.Origin.Y);
                }
                string skillName;
                if (this.StringLinker != null && this.StringLinker.StringSkill.TryGetValue(Skill.AddAttackToolTipDescSkill, out sr))
                {
                    skillName = sr.Name;
                }
                else
                {
                    skillName = Skill.AddAttackToolTipDescSkill.ToString();
                }
                picH += 10;
                GearGraphics.DrawString(g, skillName, GearGraphics.ItemDetailFont, region.LinkedSkillNameLeft, region.TextRight, ref picH, 16);
                picH += 6;
                picH += 13;
            }

            if (Skill.AssistSkillLink != 0)
            {
                //delay rendering v6 splitter
                splitterH.Add(picH);
                picH += 15;
                GearGraphics.DrawPlainText(g, "[輔助技能]", GearGraphics.ItemDetailFont, GearGraphics.SkillSummaryOrangeTextColor, region.LevelDescLeft, region.TextRight, ref picH, 16);
                picH += 4;
                BitmapOrigin icon = new BitmapOrigin();
                Wz_Node skillNode = PluginBase.PluginManager.FindWz(string.Format(@"Skill\{0}.img\skill\{1}", Skill.AssistSkillLink / 10000, Skill.AssistSkillLink));
                if (skillNode != null)
                {
                    Skill skill = Skill.CreateFromNode(skillNode, PluginBase.PluginManager.FindWz);
                    icon = skill.Icon;
                }
                if (icon.Bitmap != null)
                {
                    g.DrawImage(icon.Bitmap, 13 - icon.Origin.X, picH + 32 - icon.Origin.Y);
                }
                string skillName;
                if (this.StringLinker != null && this.StringLinker.StringSkill.TryGetValue(Skill.AssistSkillLink, out sr))
                {
                    skillName = sr.Name;
                }
                else
                {
                    skillName = Skill.AssistSkillLink.ToString();
                }
                picH += 10;
                GearGraphics.DrawString(g, skillName, GearGraphics.ItemDetailFont, region.LinkedSkillNameLeft, region.TextRight, ref picH, 16);
                picH += 6;
                picH += 13;
            }

            List<string> skillDescEx = new List<string>();
            if (ShowProperties)
            {
                List<string> attr = new List<string>();
                if (Skill.ReqLevel > 0)
                 {
                    attr.Add("所需等級: " + Skill.ReqLevel);
                }
                if (Skill.Invisible)
                {
                    attr.Add("隐藏技能");
                }
                if (Skill.Hyper != HyperSkillType.None)
                {
                    attr.Add("超級技能: " + Skill.Hyper);
                }
                if (Skill.CombatOrders)
                {
                    // attr.Add("コンバットオーダー適用");
                    if (doHighlight && DiffSkillTags.ContainsKey(skillIDstr) && DiffSkillTags[skillIDstr].Contains("combatOrders"))
                    {
                        attr.Add("#g戰鬥命令加成#");
                    }
                    else
                    {
                        attr.Add("戰鬥命令加成");
                    }
                }
                if (Skill.NotRemoved)
                {
                    attr.Add("無法被移除");
                }
                if (Skill.MasterLevel > 0 && Skill.MasterLevel < Skill.MaxLevel)
                {
                    attr.Add("初始掌握: Lv." + Skill.MasterLevel);
                }

                if (attr.Count > 0)
                {
                    skillDescEx.Add("#c" + string.Join(", ", attr.ToArray()) + "#");
                }
            }

            if (ShowDelay && Skill.Action.Count > 0)
            {
                foreach (string action in Skill.Action)
                {
                    skillDescEx.Add("#c[技能延遲] " + action + ": " + CharaSimLoader.GetActionDelay(action, this.wzNode) + " ms#");
                }
            }

            if (ShowReqSkill && Skill.ReqSkill.Count > 0)
            {
                foreach (var kv in Skill.ReqSkill)
                {
                    string skillName;
                    if (this.StringLinker != null && this.StringLinker.StringSkill.TryGetValue(kv.Key, out sr))
                    {
                        skillName = sr.Name;
                    }
                    else
                    {
                        skillName = kv.Key.ToString();
                    }
                    skillDescEx.Add("#c[必要技能] " + skillName + ": " + kv.Value + " 以上#");
                }
            }

            if (Skill.LT.X != 0 && ShowRangeCoordinates)
            {

                skillDescEx.Add("#c[範圍座標] LT(左上): (" + Skill.LT.X + "," + Skill.LT.Y + ")" + " / " +
                                            "RB(右下): (" + Skill.RB.X + "," + Skill.RB.Y + ")");
                int LT = Math.Abs(Skill.LT.X) + Skill.RB.X;
                int RB = Math.Abs(Skill.LT.Y) + Skill.RB.Y;
                skillDescEx.Add("#c[範圍] " + LT + " x " + RB);

            }

            if (skillDescEx.Count > 0)
            {
                //delay rendering v6 splitter
                splitterH.Add(picH);
                picH += 9;
                foreach (var descEx in skillDescEx)
                {
                    if (Translator.IsKoreanStringPresent(descEx))
                    {
                        GearGraphics.DrawString(g, descEx, GearGraphics.KMSItemDetailFont, region.LevelDescLeft, region.TextRight, ref picH, 16);
                    }
                    else
                    {
                        GearGraphics.DrawString(g, descEx, GearGraphics.ItemDetailFont, region.LevelDescLeft, region.TextRight, ref picH, 16);
                    }
                }
                picH += 3;
            }

            picH += 6;

            format.Dispose();
            g.Dispose();
            return bitmap;
        }
        private void DrawV6SkillDotline(Graphics g, int x1, int x2, int y)
        {
            // here's a trick that we won't draw left and right part because it looks the same as background border.
            var picCenter = Enable22AniStyle ? Resource.UIToolTipNew_img_Skill_Frame_dotline_c : Resource.UIToolTip_img_Skill_Frame_dotline_c;
            using (var brush = new TextureBrush(picCenter))
            {
                brush.TranslateTransform(x1, y);
                g.FillRectangle(brush, new Rectangle(x1, y, x2 - x1, picCenter.Height));
            }
        }

        private Bitmap RenderLinkRidingGear(Gear gear)
        {
            TooltipRender renderer = this.LinkRidingGearRender;
            if (renderer == null)
            {
                GearTooltipRender2 defaultRenderer = new GearTooltipRender2();
                defaultRenderer.StringLinker = this.StringLinker;
                defaultRenderer.ShowObjectID = false;
                renderer = defaultRenderer;
            }

            renderer.TargetItem = gear;
            return renderer.Render();
        }

        private class CanvasRegion
        {
            public int Width { get; private set; }
            public int TitleCenterX { get; private set; }
            public int SplitterX1 { get; private set; }
            public int SplitterX2 { get; private set; }
            public int SkillDescLeft { get; private set; }
            public int LinkedSkillNameLeft { get; private set; }
            public int LevelDescLeft { get; private set; }
            public int TextRight { get; private set; }

            public static CanvasRegion Original { get; } = new CanvasRegion()
            {
                Width = 290,
                TitleCenterX = 144,
                SplitterX1 = 4,
                SplitterX2 = 284,
                SkillDescLeft = 90,
                LinkedSkillNameLeft = 46,
                LevelDescLeft = 8,
                TextRight = 272,
            };

            public static CanvasRegion Wide { get; } = new CanvasRegion()
            {
                Width = 430,
                TitleCenterX = 215,
                SplitterX1 = 4,
                SplitterX2 = 424,
                SkillDescLeft = 92,
                LinkedSkillNameLeft = 49,
                LevelDescLeft = 13,
                TextRight = 411,
            };

            public static CanvasRegion _22AniOriginal { get; } = new CanvasRegion()
            {
                Width = 290,
                TitleCenterX = 144,
                SplitterX1 = 12,
                SplitterX2 = 276,
                SkillDescLeft = 90,
                LinkedSkillNameLeft = 46,
                LevelDescLeft = 8,
                TextRight = 272,
            };

            public static CanvasRegion _22AniWide { get; } = new CanvasRegion()
            {
                Width = 430,
                TitleCenterX = 215,
                SplitterX1 = 12,
                SplitterX2 = 416,
                SkillDescLeft = 92,
                LinkedSkillNameLeft = 49,
                LevelDescLeft = 13,
                TextRight = 411,
            };
        }
    }
}
