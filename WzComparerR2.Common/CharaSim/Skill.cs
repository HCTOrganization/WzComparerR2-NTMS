using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using WzComparerR2.WzLib;

namespace WzComparerR2.CharaSim
{
    public class Skill
    {
        public Skill()
        {
            this.level = 0;
            this.levelCommon = new List<Dictionary<string, string>>();
            this.common = new Dictionary<string, string>();
            this.PVPcommon = new Dictionary<string, string>();
            this.RelationSkill = null;
            this.ReqSkill = new Dictionary<int, int>();
            this.Action = new List<string>();
            this.IsRoguelikeSkill = false;
            this.IsRedmoon = false;
            this.IsGuildCastleResearch = false;
            this.GuildCastleResearchType = 0;
            this.GuildCastleResearchRequirements = new Dictionary<int, int>();
            this.GuildCastleResearchReqCondition = "";
            this.VariableProps = new List<string>();
            this.PerJobAttackInfo = new Dictionary<int, Dictionary<string, string>>();
            this.perJobIndex = 0;
        }

        private int level;
        private int perJobIndex;
        internal List<Dictionary<string, string>> levelCommon;
        internal Dictionary<string, string> common;

        /// <summary>common 內的攻擊範圍（所有等級共用）。</summary>
        private Point commonLT;
        private Point commonRB;

        /// <summary>各等級的攻擊範圍（<c>level/&lt;n&gt;/lt</c>、<c>rb</c>）。</summary>
        private Dictionary<int, Point> levelLT;
        private Dictionary<int, Point> levelRB;

        public Dictionary<string, string> Common
        {
            get
            {
                if (PreBBSkill && this.level > 0 && this.level <= levelCommon.Count)
                    return levelCommon[this.level - 1];
                else
                    return common;
            }
        }

        public Dictionary<string, string> GetCommon(int level)
        {
            Dictionary<string, string> result = new Dictionary<string, string>(this.common);
            if (level > 0 && level <= levelCommon.Count)
            {
                foreach (var kv in levelCommon[level - 1])
                {
                    result[kv.Key] = kv.Value;
                }
            }
            return result;
        }

        public Dictionary<string, string> PVPcommon { get; private set; }
        public int SkillID { get; set; }
        public BitmapOrigin Icon { get; set; }
        public BitmapOrigin IconMouseOver { get; set; }
        public BitmapOrigin IconDisabled { get; set; }

        public HyperSkillType Hyper { get; set; }
        public bool HyperStat { get; set; }

        public int Level
        {
            get { return level; }
            set
            {
                bool canBreakLevel = this.CombatOrders || this.VSkill
                    || this.SkillID / 100000 == 4000; //fix for evan
                int maxLevel = canBreakLevel ? 100 : this.MaxLevel;
                level = Math.Max(0, Math.Min(value, maxLevel));
            }
        }

        public int PerJobIndex
        {
            get { return perJobIndex; }
            set
            {
                perJobIndex = Math.Max(0, Math.Min(value, this.PerJobAttackInfo.Count - 1));
            }
        }

        public int ReqLevel { get; set; }
        public int ReqAmount { get; set; }
        public bool PreBBSkill { get; set; }
        public bool Invisible { get; set; }
        public bool CombatOrders { get; set; }
        public bool NotRemoved { get; set; }
        public bool VSkill { get; set; }
        public int VSkillValue { get; set; }
        public bool NotIncBuffDuration { get; set; }
        public bool NotCooltimeReset { get; set; }
        public bool NotCooltimeReduce { get; set; }
        public bool Origin { get; set; }
        public bool Ascent { get; set; }
        public bool TimeLimited { get; set; }
        public Tuple<int, int> RelationSkill { get; set; }
        public bool IsPetAutoBuff { get; set; }
        public bool IsSequenceOn { get; set; }
        public bool IsRoguelikeSkill { get; private set; }
        public bool IsRedmoon { get; private set; }
        public bool IsGuildCastleResearch { get; private set; }
        public int GuildCastleResearchType { get; private set; }
        public Dictionary<int, int> GuildCastleResearchRequirements { get; private set; }
        public string GuildCastleResearchReqCondition { get; set; }
        public List<string> VariableProps { get; private set; }
        public bool DisableNextLevelInfo { get; set; }
        public int MasterLevel { get; set; }
        public Dictionary<int, int> ReqSkill { get; private set; }
        public List<string> Action { get; private set; }
        public int AddAttackToolTipDescSkill { get; set; }
        public int AssistSkillLink { get; set; }
        public int VehicleID { get; set; }
        public Dictionary<int, Dictionary<string, string>> PerJobAttackInfo { get; private set; }

        /// <summary>
        /// 攻擊範圍左上角。等級專屬的 <c>level/&lt;n&gt;/lt</c> 優先，
        /// 否則使用 <c>common/lt</c>。
        /// </summary>
        public Point LT
        {
            get { return this.GetLevelPoint(this.levelLT, this.level) ?? this.commonLT; }
            set { this.commonLT = value; }
        }

        /// <summary>
        /// 攻擊範圍右下角。等級專屬的 <c>level/&lt;n&gt;/rb</c> 優先，
        /// 否則使用 <c>common/rb</c>。
        /// </summary>
        public Point RB
        {
            get { return this.GetLevelPoint(this.levelRB, this.level) ?? this.commonRB; }
            set { this.commonRB = value; }
        }

        /// <summary>取得指定等級的攻擊範圍（等級專屬優先，否則 common）。</summary>
        public void GetAttackRange(int level, out Point lt, out Point rb)
        {
            lt = this.GetLevelPoint(this.levelLT, level) ?? this.commonLT;
            rb = this.GetLevelPoint(this.levelRB, level) ?? this.commonRB;
        }

        /// <summary>
        /// 指定等級的攻擊範圍面積佔最高等級的百分比（保留 1 位小數）；
        /// 缺少範圍資料時回傳 null。
        /// </summary>
        public double? GetRangeAreaPercent(int level)
        {
            double area = this.GetRangeArea(level);
            if (area <= 0)
            {
                return null;
            }

            int maxLevel = this.MaxLevel > 0 ? this.MaxLevel : level;
            double maxArea = this.GetRangeArea(maxLevel);
            if (maxArea <= 0)
            {
                return null;
            }

            return Math.Round(area / maxArea * 100d, 1, MidpointRounding.AwayFromZero);
        }

        private double GetRangeArea(int level)
        {
            Point lt, rb;
            this.GetAttackRange(level, out lt, out rb);
            return Math.Abs((double)rb.X - lt.X) * Math.Abs((double)rb.Y - lt.Y);
        }

        private Point? GetLevelPoint(Dictionary<int, Point> points, int level)
        {
            if (points == null || points.Count == 0)
            {
                return null;
            }

            Point point;
            if (level > 0 && points.TryGetValue(level, out point))
            {
                return point;
            }

            // 沒有剛好對應的等級（例如 Combat Orders 超過上限、或 Level 0）時，
            // 取不超過目前等級的最高等級，再退回最低等級。
            int best = 0;
            int lowest = int.MaxValue;
            foreach (int key in points.Keys)
            {
                if (key <= level && key > best)
                {
                    best = key;
                }
                if (key < lowest)
                {
                    lowest = key;
                }
            }
            if (best != 0)
            {
                return points[best];
            }
            return lowest == int.MaxValue ? (Point?)null : points[lowest];
        }

        /// <summary>
        /// 記錄某等級的攻擊範圍（<c>level/&lt;n&gt;/lt</c>、<c>rb</c>）。
        /// 值可能是 vector（type 14）或 2 個數值的組合（type 8）。
        /// </summary>
        private bool SetLevelPoint(int level, string name, object value)
        {
            if (level <= 0 || !TryGetPoint(value, out Point point))
            {
                return false;
            }
            switch (name.ToLowerInvariant())
            {
                case "lt":
                    if (this.levelLT == null) this.levelLT = new Dictionary<int, Point>();
                    this.levelLT[level] = point;
                    return true;
                case "rb":
                    if (this.levelRB == null) this.levelRB = new Dictionary<int, Point>();
                    this.levelRB[level] = point;
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>把節點值解讀成座標點（vector 或 2 個數值的組合）。</summary>
        private static bool TryGetPoint(object value, out Point point)
        {
            switch (value)
            {
                case Wz_Vector vector:
                    point = new Point(vector.X, vector.Y);
                    return true;
                case int[] intPair when intPair.Length == 2:
                    point = new Point(intPair[0], intPair[1]);
                    return true;
                case float[] floatPair when floatPair.Length == 2:
                    point = new Point(
                        (int)Math.Round(floatPair[0]), (int)Math.Round(floatPair[1]));
                    return true;
                default:
                    point = Point.Empty;
                    return false;
            }
        }

        public int MaxLevel
        {
            get
            {
                string v;
                if (this.PreBBSkill)
                    return levelCommon.Count;
                else if (common.TryGetValue("maxLevel", out v))
                    return Convert.ToInt32(v);
                return 0;
            }
        }

        public static Skill CreateFromNode(Wz_Node node, GlobalFindNodeFunction findNode, GlobalFindNodeFunction2 findNode2, Wz_File wzf = null)
        {
            if (node == null) return null;
            // 節點型別由 WZ 檔或外部型別解析器（Unity 資料集目錄）判斷。
            if (node.GetNodeWzType() is Wz_Type wzType
                && wzType != Wz_Type.Skill && wzType != Wz_Type.Etc) return null;

            Skill skill = new Skill();
            int skillID;
            if (!Int32.TryParse(node?.Text, out skillID))
            {
                if (node.FullPathToFile.StartsWith("Skill\\Roguelike"))
                {
                    Match m = Regex.Match(node.FullPathToFile, @"^Skill\\Roguelike\\.+\\(\d+)\.img$");
                    if (!(m.Success && Int32.TryParse(m.Result("$1"), out skillID)))
                    {
                        return null;
                    }
                    else
                    {
                        skill.IsRoguelikeSkill = true;
                        if (node.FullPathToFile.Contains("Redmoon")) skill.IsRedmoon = true;
                    }
                }
            }
            else if (node.FullPathToFile.StartsWith("Etc\\GuildCastle.img"))
            {
                Match m = Regex.Match(node.FullPathToFile, @"^Etc\\GuildCastle.img\\ResearchList\\(Guild|Personal)\\(\d+)$");
                if (!m.Success)
                {
                    return null;
                }
                else
                {
                    skill.IsGuildCastleResearch = true;
                    switch (m.Result("$1"))
                    {
                        case "Guild": skill.GuildCastleResearchType = 0; break;
                        case "Personal": skill.GuildCastleResearchType = 1; break;
                    }
                }
            }
            skill.SkillID = skillID;

            foreach (Wz_Node childNode in node.Nodes)
            {
                switch (childNode.Text)
                {
                    case "icon":
                        skill.Icon = BitmapOrigin.CreateFromNode(childNode, findNode, wzf);
                        break;
                    case "iconMouseOver":
                        skill.IconMouseOver = BitmapOrigin.CreateFromNode(childNode, findNode, wzf);
                        break;
                    case "iconDisabled":
                        skill.IconDisabled = BitmapOrigin.CreateFromNode(childNode, findNode, wzf);
                        break;
                    case "common":
                        if (skill.IsRoguelikeSkill)
                        {
                            skill.common.Add("maxLevel", "1");
                            foreach (Wz_Node commonNode in childNode.Nodes)
                            {
                                switch (commonNode.Text)
                                {
                                    case "icon":
                                        skill.Icon = BitmapOrigin.CreateFromNode(commonNode, findNode, wzf);
                                        break;
                                    case "iconMouseOver":
                                        skill.IconMouseOver = BitmapOrigin.CreateFromNode(commonNode, findNode, wzf);
                                        break;
                                    case "iconDisabled":
                                        skill.IconDisabled = BitmapOrigin.CreateFromNode(commonNode, findNode, wzf);
                                        break;
                                    case "Combo":
                                    case "Tags":
                                        break;
                                    default:
                                        if (commonNode.Value != null && !(commonNode.Value is Wz_Vector))
                                        {
                                            skill.common[commonNode.Text] = commonNode.Value.ToString();
                                        }
                                        else if (commonNode.Value != null && commonNode.Value is Wz_Vector)
                                        {
                                            Wz_Vector cNode = commonNode.Value as Wz_Vector;
                                            if (commonNode.Text == "lt")
                                            {
                                                skill.LT = new Point(cNode.X, cNode.Y);
                                            }
                                            else if (commonNode.Text == "rb")
                                            {
                                                skill.RB = new Point(cNode.X, cNode.Y);
                                            }
                                        }
                                        break;
                                }
                            }
                        }
                        else
                        {
                            foreach (Wz_Node commonNode in childNode.Nodes)
                            {
                                if (commonNode.Value != null && !(commonNode.Value is Wz_Vector))
                                {
                                    skill.common[commonNode.Text] = commonNode.Value.ToString();
                                }
                                else if (commonNode.Value != null && commonNode.Value is Wz_Vector)
                                {
                                    Wz_Vector cNode = commonNode.Value as Wz_Vector;
                                    if (commonNode.Text == "lt")
                                    {
                                        skill.LT = new Point(cNode.X, cNode.Y);
                                    }
                                    else if (commonNode.Text == "rb")
                                    {
                                        skill.RB = new Point(cNode.X, cNode.Y);
                                    }
                                }
                                else if (commonNode.Text == "attackInfo")
                                {
                                    if (commonNode.Nodes.Count > 0)
                                    {
                                        foreach (Wz_Node jobNode in commonNode.Nodes)
                                        {
                                            int jobID;
                                            if (Int32.TryParse(jobNode.Text, out jobID))
                                            {
                                                Dictionary<string, string> attackInfo = new Dictionary<string, string>();
                                                foreach (Wz_Node infoNode in jobNode.Nodes)
                                                {
                                                    attackInfo[infoNode.Text] = infoNode.Value.ToString();
                                                }
                                                skill.PerJobAttackInfo[jobID] = attackInfo;
                                            }
                                        }
                                    }
                                }
                                else if (commonNode.Nodes.Count > 0)
                                {
                                    skill.VariableProps.Add(commonNode.Text);
                                    foreach (var levelNode in commonNode.Nodes)
                                    {
                                        if (levelNode.Value != null && !(levelNode.Value is Wz_Vector))
                                        {
                                            skill.common[$"{commonNode.Text}_{levelNode.Text}"] = levelNode.Value.ToString();
                                        }
                                    }
                                }
                            }
                        }
                        break;
                    case "constants":
                        foreach (Wz_Node commonNode in childNode.Nodes)
                        {
                            if (commonNode.Value != null && !(commonNode.Value is Wz_Vector))
                            {
                                skill.common[commonNode.Text] = commonNode.Value.ToString();
                            }
                            else if (commonNode.Value != null && commonNode.Value is Wz_Vector)
                            {
                                Wz_Vector cNode = commonNode.Value as Wz_Vector;
                                if (commonNode.Text == "lt")
                                {
                                    skill.LT = new Point(cNode.X, cNode.Y);
                                }
                                else if (commonNode.Text == "rb")
                                {
                                    skill.RB = new Point(cNode.X, cNode.Y);
                                }
                            }
                        }
                        break;
                    case "PVPcommon":
                        foreach (Wz_Node commonNode in childNode.Nodes)
                        {
                            if (commonNode.Value != null && !(commonNode.Value is Wz_Vector))
                            {
                                skill.PVPcommon[commonNode.Text] = commonNode.Value.ToString();
                            }
                        }
                        break;
                    case "level":
                        for (int i = 1; ; i++)
                        {
                            Wz_Node levelNode = childNode.FindNodeByPath(i.ToString());
                            if (levelNode == null)
                                break;
                            Dictionary<string, string> levelInfo = new Dictionary<string, string>();

                            foreach (Wz_Node commonNode in levelNode.Nodes)
                            {
                                if (commonNode.Value == null)
                                {
                                    continue;
                                }
                                // Classic 客戶端把攻擊範圍放在各等級底下（level/<n>/lt、rb）。
                                if (skill.SetLevelPoint(i, commonNode.Text, commonNode.Value))
                                {
                                    continue;
                                }
                                if (commonNode.Value is Wz_Vector)
                                {
                                    continue; // 其他 vector 屬性維持原有行為（不進 levelCommon）
                                }
                                levelInfo[commonNode.Text] = commonNode.Value.ToString();
                            }

                            skill.levelCommon.Add(levelInfo);
                        }
                        break;
                    case "maxLevel":
                        skill.common["maxLevel"] = childNode.Value.ToString();
                        break;
                    case "hyper":
                        skill.Hyper = (HyperSkillType)childNode.GetValue<int>();
                        break;
                    case "hyperStat":
                        skill.HyperStat = childNode.GetValue<int>() != 0;
                        break;
                    case "hide":
                    case "invisible":
                        skill.Invisible = childNode.GetValue<int>() != 0;
                        break;
                    case "combatOrders":
                        skill.CombatOrders = childNode.GetValue<int>() != 0;
                        break;
                    case "notRemoved":
                        skill.NotRemoved = childNode.GetValue<int>() != 0;
                        break;
                    case "vSkill":
                        skill.VSkill = childNode.GetValue<int>() != 0;
                        skill.VSkillValue = childNode.GetValue<int>();
                        break;
                    case "notIncBuffDuration":
                        skill.NotIncBuffDuration = childNode.GetValue<int>() != 0;
                        break;
                    case "notCooltimeReset":
                        skill.NotCooltimeReset = childNode.GetValue<int>() != 0;
                        break;
                    case "notCooltimeReduce":
                        skill.NotCooltimeReduce = childNode.GetValue<int>() != 0;
                        break;
                    case "origin":
                        skill.Origin = childNode.GetValue<int>() != 0;
                        break;
                    case "ascent":
                        skill.Ascent = childNode.GetValue<int>() != 0;
                        break;
                    case "timeLimited":
                        skill.TimeLimited = childNode.GetValue<int>() != 0;
                        break;
                    case "relationSkill":
                        skill.RelationSkill = Tuple.Create(childNode.Nodes["skillID"].GetValueEx<int>(0), childNode.Nodes["periodMin"].GetValueEx<int>(0));
                        break;
                    case "isPetAutoBuff":
                        skill.IsPetAutoBuff = childNode.GetValue<int>() != 0;
                        break;
                    case "isSequenceOn":
                        skill.IsSequenceOn = childNode.GetValue<int>() != 0;
                        break;
                    case "disableNextLevelInfo":
                        skill.DisableNextLevelInfo = childNode.GetValue<int>() != 0;
                        break;
                    case "masterLevel":
                        skill.MasterLevel = childNode.GetValue<int>();
                        break;
                    case "reqLev":
                        skill.ReqLevel = childNode.GetValue<int>();
                        break;
                    case "req":
                        foreach (Wz_Node reqNode in childNode.Nodes)
                        {
                            if (reqNode.Text == "level")
                            {
                                skill.ReqLevel = reqNode.GetValue<int>();
                            }
                            else if (reqNode.Text == "reqAmount")
                            {
                                skill.ReqAmount = reqNode.GetValue<int>();
                            }
                            else
                            {
                                int reqSkill;
                                if (Int32.TryParse(reqNode.Text, out reqSkill))
                                {
                                    skill.ReqSkill[reqSkill] = reqNode.GetValue<int>();
                                }
                            }
                        }
                        break;
                    case "requirement":
                        if (skill.IsGuildCastleResearch)
                        {
                            Wz_Node conditionNode = childNode.FindNodeByPath("condition");
                            if (conditionNode != null)
                                skill.GuildCastleResearchReqCondition = conditionNode.Value.ToString();
                            foreach (Wz_Node reqNode in childNode.Nodes)
                            {
                                Wz_Node researchIDNode = reqNode.FindNodeByPath("researchID");
                                Wz_Node researchLvNode = reqNode.FindNodeByPath("researchLv");
                                if (researchIDNode == null || researchLvNode == null)
                                    continue;
                                skill.GuildCastleResearchRequirements[researchIDNode.GetValue<int>()] = researchLvNode.GetValue<int>();
                            }
                        }
                        break;
                    case "action":
                        for (int i = 0; ; i++)
                        {
                            Wz_Node idxNode = childNode.FindNodeByPath(i.ToString());
                            if (idxNode == null)
                                break;
                            skill.Action.Add(idxNode.GetValue<string>());
                        }
                        break;
                    case "addAttack":
                        Wz_Node toolTipDescNode = childNode.FindNodeByPath("toolTipDesc");
                        if (toolTipDescNode != null && toolTipDescNode.GetValue<int>() != 0)
                        {
                            skill.AddAttackToolTipDescSkill = childNode.FindNodeByPath("toolTipDescSkill").GetValue<int>();
                        }
                        break;
                    case "assistSkillLink":
                        skill.AssistSkillLink = childNode.FindNodeByPath("skill").GetValue<int>();
                        break;
                    case "vehicleID":
                        skill.VehicleID = childNode.GetValue<int>();
                        break;
                }
            }

            if ((skill.common.ContainsKey("forceCon") || (skill.levelCommon.Count > 0 && skill.levelCommon[0].ContainsKey("forceCon"))) && skill.Hyper == HyperSkillType.None)
            {
                Wz_Node forceNode = null;
                if (skill.SkillID / 10000 == 3001 || skill.SkillID / 10000 == 3100 || skill.SkillID / 10000 == 3110 || skill.SkillID / 10000 == 3111 || skill.SkillID / 10000 == 3112)
                {
                    forceNode = findNode2.Invoke(string.Format("UI\\UIWindow2.img\\Skill\\main\\Force\\{0}", (Int32.Parse(skill.common["forceCon"]) - 1) / 30), wzf);
                }
                else if (skill.SkillID / 10000 / 1000 == 10)
                {
                    forceNode = findNode2.Invoke(string.Format("UI\\UIWindow2.img\\SkillZero\\main\\Alpha\\{0}", skill.SkillID / 1000 % 10), wzf);
                }
                if (forceNode != null)
                {
                    BitmapOrigin force = BitmapOrigin.CreateFromNode(forceNode, findNode, wzf);
                    using (Graphics graphics = Graphics.FromImage(skill.Icon.Bitmap))
                    {
                        graphics.DrawImage(force.Bitmap, new Point(0, 0));
                    }
                    using (Graphics graphics = Graphics.FromImage(skill.IconMouseOver.Bitmap))
                    {
                        graphics.DrawImage(force.Bitmap, new Point(0, 0));
                    }
                    using (Graphics graphics = Graphics.FromImage(skill.IconDisabled.Bitmap))
                    {
                        graphics.DrawImage(force.Bitmap, new Point(0, 0));
                    }
                }
            }

            //判定技能声明版本
            skill.PreBBSkill = skill.levelCommon.Count > 0;

            return skill;
        }
    }
}
