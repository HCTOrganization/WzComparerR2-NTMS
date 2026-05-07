using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WzComparerR2.WzLib;

namespace WzComparerR2.CharaSim
{
    public class Achievement
    {
        public Achievement()
        {
            this.ID = -1;
            this.PriorIDs = new List<int>();
            this.Missions = new List<string>();
            this.Rewards = new List<AchievementReward>();
        }

        private string _mainCategory { get; set; }
        private string _subCategory { get; set; }
        public int ID { get; set; }
        public int Score { get; set; }
        public string MainCategory
        {
            get { return GetMainCategoryStr(); }
        }
        public string SubCategory
        {
            get { return GetSubCategoryStr(); }
        }
        public string Difficulty { get; set; }
        public string UiForm { get; set; }
        public string Block { get; set; }
        public string PriorCondition { get; set; }
        public string Start { get; set; }
        public string End { get; set; }
        public List<int> PriorIDs { get; set; }
        public List<string> Missions { get; set; }
        public List<AchievementReward> Rewards { get; set; }

        public bool ShowMissions
        {
            get
            {
                return (this.UiForm == "mission" || this.UiForm == "all")
                    && this.Missions.Count > 0;
            }
        }
        public bool HasRewards
        {
            get { return this.Rewards.Count > 0; }
        }
        public bool Hide
        {
            get { return this.Block == "hide"; }
        }

        public static Achievement CreateFromNode(
            Wz_Node node,
            GlobalFindNodeFunction findNode,
            GlobalFindNodeFunction2 findNode2,
            Wz_File wzf = null
        )
        {
            if (node == null)
                return null;

            Match m = Regex.Match(node.Text, @"^(\d+)\.img$");
            if (!(m.Success && Int32.TryParse(m.Result("$1"), out int achievementID)))
            {
                return null;
            }

            Achievement achievement = new Achievement();
            achievement.ID = achievementID;
            Wz_Node infoNode = node.FindNodeByPath("info").ResolveUol();
            if (infoNode != null)
            {
                foreach (var propNode in infoNode.Nodes)
                {
                    switch (propNode.Text)
                    {
                        case "score":
                            achievement.Score = propNode.GetValueEx<int>(0);
                            break;
                        case "mainCategory":
                            achievement._mainCategory = propNode.GetValueEx<string>(null);
                            break;
                        case "subCategory":
                            achievement._subCategory = propNode.GetValueEx<string>(null);
                            break;
                        case "difficulty":
                            achievement.Difficulty = propNode.GetValueEx<string>("normal");
                            break;
                        case "prior":
                            var prior = propNode.FindNodeByPath("achievement_id");
                            if (prior != null)
                                achievement.PriorIDs.Add(prior.GetValueEx<int>(-1));
                            else
                            {
                                var valueNode = propNode.FindNodeByPath("values");
                                foreach (
                                    var value in valueNode?.Nodes
                                        ?? new Wz_Node.WzNodeCollection(null)
                                )
                                {
                                    prior = value.FindNodeByPath("achievement_id");
                                    var priorID = prior.GetValueEx<int>(-1);
                                    if (priorID > -1)
                                    {
                                        achievement.PriorIDs.Add(prior.GetValueEx<int>(priorID));
                                    }
                                }
                            }
                            achievement.PriorCondition = propNode
                                .FindNodeByPath("condition")
                                .GetValueEx<string>(null);
                            break;
                        case "uiType":
                            achievement.UiForm = propNode
                                .FindNodeByPath("uiForm")
                                .GetValueEx<string>("basic");
                            break;
                        case "block":
                            achievement.Block = propNode.GetValueEx<string>("none");
                            break;
                        case "period":
                            achievement.Start = propNode
                                .FindNodeByPath("start")
                                .GetValueEx<string>(null);
                            achievement.End = propNode
                                .FindNodeByPath("end")
                                .GetValueEx<string>(null);
                            break;
                    }
                }
            }

            Wz_Node missionNode = node.FindNodeByPath("mission").ResolveUol();
            if (missionNode != null)
            {
                foreach (var mission in missionNode.Nodes)
                {
                    var missionName = mission.FindNodeByPath("name").GetValueEx<string>(null);
                    if (!string.IsNullOrEmpty(missionName))
                    {
                        achievement.Missions.Add(missionName);
                    }
                }
            }

            Wz_Node rewardNode = node.FindNodeByPath("reward").ResolveUol();
            if (rewardNode != null)
            {
                foreach (var reward in rewardNode.Nodes)
                {
                    var id = reward.FindNodeByPath("id").GetValueEx<int>(-1);
                    var desc = reward.FindNodeByPath("desc").GetValueEx<string>(null);
                    if (id >= 0 && !string.IsNullOrEmpty(desc))
                    {
                        achievement.Rewards.Add(new AchievementReward() { ID = id, Desc = desc });
                    }
                }
            }

            return achievement;
        }

        private string GetMainCategoryStr()
        {
            // Etc/Achievement/AchievementInfo.img/Category
            switch (this._mainCategory)
            {
                case "general":
                    return "一般";
                case "growth":
                    return "養成";
                case "job":
                    return "職業";
                case "item":
                    return "道具";
                case "adventure":
                    return "冒險";
                case "battle":
                    return "戰鬥";
                case "social":
                    return "社群";
                case "event":
                    return "活動";
                case "memory":
                    return "回憶";

                default:
                    return this._mainCategory;
            }
        }

        private string GetSubCategoryStr()
        {
            // Etc/Achievement/AchievementInfo.img/Category
            switch (this._mainCategory)
            {
                case "general":
                case "event":
                case "memory":
                    return null;
            }

            switch (this._subCategory)
            {
                case "level":
                    return "等級";
                case "stat":
                    return "能力值";
                case "personality":
                    return "性向";
                case "makingSkill":
                    return "專業技術";
                case "union":
                    return "聯盟戰地";

                case "story":
                    return "劇情";
                case "jobChange":
                    return "轉職";
                case "skill":
                    return "技能";
                case "vMatrix":
                    return "V矩陣";
                case "linkSkill":
                    return "傳授技能";

                case "collection":
                    return "收集";
                case "enchantment":
                    return "強化";
                case "equip":
                    return "裝備";

                case "exploration":
                    return "探險";
                case "quest":
                    return "任務";
                case "cooperation":
                    return "合作";
                case "special":
                    return "特殊";

                case "field":
                    return "地圖";
                case "boss":
                    return "BOSS";
                case "loot":
                    return "戰利品";

                case "party":
                    return "組隊";
                case "guild":
                    return "公會";
                case "trade":
                    return "交易";
                case "etc":
                    return "其他";

                case "progress":
                    return "進行中活動";
                case "complete":
                    return "過去活動";

                default:
                    return this._subCategory;
            }
        }

        public struct AchievementReward
        {
            public int ID;
            public string Desc;
        }
    }
}
