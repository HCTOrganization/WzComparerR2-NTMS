using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using WzComparerR2.WzLib;

namespace WzComparerR2.CharaSim
{
    public class Mob : IDisposable
    {
        public Mob()
        {
            this.ID = -1;
            this.ElemAttr = new MobElemAttr(null);
            this.Revive = new List<int>();
            this.QuestCountGroupMobID = new List<int>();
            this.mobGroupIndex = 0;
            //this.Animates = new LifeAnimateCollection();

            this.FirstAttack = false;
            this.BodyAttack = false;
            this.DamagedByMob = false;
            this.IgnoreMoveImpact = false;
            this.IgnoreMovable = false;
            this.IsQuestCountGroupMob = false;
            this.LvOptimum = false;
            this.Filters = 0;
        }

        public int ID { get; set; }
        public int Level { get; set; }
        public string DefaultHP { get; set; }
        public string DefaultMP { get; set; }
        public string FinalMaxHP { get; set; }
        public string FinalMaxMP { get; set; }
        public long MaxHP { get; set; }
        public long MaxMP { get; set; }
        public int HPRecovery { get; set; }
        public int MPRecovery { get; set; }
        public int? Speed { get; set; }
        public int? FlySpeed { get; set; }
        public int PADamage { get; set; }
        public int MADamage { get; set; }
        public int PDRate { get; set; }
        public int MDRate { get; set; }
        public int PDDamage { get; set; }
        public int MDDamage { get; set; }
        public int Acc { get; set; }
        public int Eva { get; set; }
        public long Pushed { get; set; }
        public int Exp { get; set; }
        public int CharismaEXP { get; set; }
        public int SenseEXP { get; set; }
        public int InsightEXP { get; set; }
        public int WillEXP { get; set; }
        public int CraftEXP { get; set; }
        public int CharmEXP { get; set; }
        public bool Boss { get; set; }
        public bool Undead { get; set; }
        public int Category { get; set; }
        public bool FirstAttack { get; set; }
        public bool BodyAttack { get; set; }
        public int FixedBodyAttackDamageR { get; set; }
        public int RemoveAfter { get; set; }
        public bool DamagedByMob { get; set; }
        public bool ChangeableMob { get; set; }
        public bool AllyMob { get; set; }
        public bool Invincible { get; set; }
        public bool NotAttack { get; set; }
        public int FixedDamage { get; set; }
        public bool IgnoreDamage { get; set; }
        public bool IgnoreMoveImpact { get; set; }
        public bool IgnoreMovable { get; set; }
        public bool NoDebuff { get; set; }
        public bool OnlyNormalAttack { get; set; }
        public bool OnlyHittedByCommonAttack { get; set; }
        public bool PartyBonusMob { get; set; }

        public bool IsQuestCountGroupMob { get; set; }
        public bool LvOptimum { get; set; }
        public int WP { get; set; }
        public MobElemAttr ElemAttr { get; set; }
        public long AttackPower { get; set; }
        public List<int> QuestCountGroupMobID { get; set; }
        private int mobGroupIndex;
        public int MobGroupIndex
        {
            get { return mobGroupIndex; }
            set
            {
                if (this.QuestCountGroupMobID.Count == 0)
                {
                    mobGroupIndex = 0;
                }
                else
                {
                    mobGroupIndex = Math.Max(0, Math.Min(value, this.QuestCountGroupMobID.Count - 1));
                }
            }
        }
        public int Filters { get; set; }

        public int? Link { get; set; }
        public bool Skeleton { get; set; }
        public bool JsonLoad { get; set; }

        public List<int> Revive { get; private set; }

        public BitmapOrigin Default { get; set; }
        //public LifeAnimateCollection Animates { get; private set; }

        public Wz_Node AvatarLook { get; set; }
        public bool IsAvatarLook
        {
            get
            {
                return this.AvatarLook != null;
            }
        }
        public static Mob CreateFromNode(Wz_Node node, GlobalFindNodeFunction findNode)
        {
            int mobID;
            Match m = Regex.Match(node.Text, @"^(\d{7})\.img$");
            if (!(m.Success && Int32.TryParse(m.Result("$1"), out mobID)))
            {
                return null;
            }

            Mob mobInfo = new Mob();
            mobInfo.ID = mobID;
            Wz_Node infoNode = node.FindNodeByPath("info").ResolveUol() ?? node.FindNodeByPath("Info\\constants").ResolveUol();
            //加载基础属性
            if (infoNode != null)
            {
                if (infoNode.FullPathToFile.Contains("QuestCountGroup"))
                {
                    mobInfo.IsQuestCountGroupMob = true;
                    foreach (var propNode in infoNode.Nodes)
                    {
                        switch (propNode.Text)
                        {
                            case "changeableMob": mobInfo.ChangeableMob = propNode.GetValueEx<int>(0) != 0; break;
                            case "changeableMobs":
                                foreach (var subNode in propNode.Nodes)
                                {
                                    if (subNode.Text.StartsWith("changeableMob"))
                                    {
                                        mobInfo.ChangeableMob = subNode.GetValueEx<int>(0) != 0;
                                    }
                                }
                                break;
                            case "lvOptimum": mobInfo.LvOptimum = propNode.GetValueEx<int>(0) != 0; break;
                            case "filters": mobInfo.Filters = propNode.GetValueEx<int>(0); break;
                            default: mobInfo.QuestCountGroupMobID.Add(propNode.GetValueEx<int>(0)); break;
                        }
                    }
                }
                else
                {
                    foreach (var propNode in infoNode.Nodes)
                    {
                        switch (propNode.Text)
                        {
                            case "level": mobInfo.Level = propNode.GetValueEx<int>(0); break;
                            case "defaultHP": mobInfo.DefaultHP = propNode.GetValueEx<string>(null); break;
                            case "defaultMP": mobInfo.DefaultMP = propNode.GetValueEx<string>(null); break;
                            case "finalmaxHP": mobInfo.FinalMaxHP = propNode.GetValueEx<string>(null); break;
                            case "finalmaxMP": mobInfo.FinalMaxMP = propNode.GetValueEx<string>(null); break;
                            case "maxHP": mobInfo.MaxHP = propNode.GetValueEx<long>(0); break;
                            case "maxMP": mobInfo.MaxMP = propNode.GetValueEx<long>(0); break;
                            case "hpRecovery": mobInfo.HPRecovery = propNode.GetValueEx<int>(0); break;
                            case "mpRecovery": mobInfo.MPRecovery = propNode.GetValueEx<int>(0); break;
                            case "speed": mobInfo.Speed = propNode.GetValueEx<int>(0); break;
                            case "flySpeed": mobInfo.FlySpeed = propNode.GetValueEx<int>(0); break;

                            case "PADamage": mobInfo.PADamage = propNode.GetValueEx<int>(0); break;
                            case "MADamage": mobInfo.MADamage = propNode.GetValueEx<int>(0); break;
                            case "PDRate": mobInfo.PDRate = propNode.GetValueEx<int>(0); break;
                            case "MDRate": mobInfo.MDRate = propNode.GetValueEx<int>(0); break;
                            case "PDDamage": mobInfo.PDDamage = propNode.GetValueEx<int>(0); break;
                            case "MDDamage": mobInfo.MDDamage = propNode.GetValueEx<int>(0); break;
                            //case "acc": mobInfo.Acc = propNode.GetValueEx<int>(0); break; //no longer used
                            //case "eva": mobInfo.Eva = propNode.GetValueEx<int>(0); break; //no longer used
                            case "pushed": mobInfo.Pushed = propNode.GetValueEx<long>(0); break;
                            case "exp": mobInfo.Exp = propNode.GetValueEx<int>(0); break;
                            case "charismaEXP": mobInfo.CharismaEXP = propNode.GetValueEx<int>(0); break;
                            case "senseEXP": mobInfo.SenseEXP = propNode.GetValueEx<int>(0); break;
                            case "insightEXP": mobInfo.InsightEXP = propNode.GetValueEx<int>(0); break;
                            case "willEXP": mobInfo.WillEXP = propNode.GetValueEx<int>(0); break;
                            case "craftEXP": mobInfo.CraftEXP = propNode.GetValueEx<int>(0); break;
                            case "charmEXP": mobInfo.CharmEXP = propNode.GetValueEx<int>(0); break;
                            case "wp": mobInfo.WP = propNode.GetValueEx<int>(0); break;

                            case "boss": mobInfo.Boss = propNode.GetValueEx<int>(0) != 0; break;
                            case "partyBonusMob": mobInfo.PartyBonusMob = propNode.GetValueEx<int>(0) != 0; break;
                            case "undead": mobInfo.Undead = propNode.GetValueEx<int>(0) != 0; break;
                            case "firstAttack": mobInfo.FirstAttack = propNode.GetValueEx<int>(0) != 0; break;
                            case "bodyAttack": mobInfo.BodyAttack = propNode.GetValueEx<int>(0) != 0; break;
                            case "fixedBodyAttackDamageR": mobInfo.FixedBodyAttackDamageR = propNode.GetValueEx<int>(0); break;
                            case "category": mobInfo.Category = propNode.GetValueEx<int>(0); break;
                            case "removeAfter": mobInfo.RemoveAfter = propNode.GetValueEx<int>(0); break;
                            case "damagedByMob": mobInfo.DamagedByMob = propNode.GetValueEx<int>(0) != 0; break;
                            case "changeableMob": mobInfo.ChangeableMob = propNode.GetValueEx<int>(0) != 0; break;
                            case "allyMob": mobInfo.AllyMob = propNode.GetValueEx<int>(0) != 0; break;
                            case "invincible": mobInfo.Invincible = propNode.GetValueEx<int>(0) != 0; break;
                            case "notAttack": mobInfo.NotAttack = propNode.GetValueEx<int>(0) != 0; break;
                            case "fixedDamage": mobInfo.FixedDamage = propNode.GetValueEx<int>(0); break;
                            case "ignoreDamage": mobInfo.IgnoreDamage = propNode.GetValueEx<int>(0) != 0; break;
                            case "ignoreMoveImpact": mobInfo.IgnoreMoveImpact = propNode.GetValueEx<int>(0) != 0; break;
                            case "ignoreMovable": mobInfo.IgnoreMovable = propNode.GetValueEx<int>(0) != 0; break;
                            case "noDebuff": mobInfo.NoDebuff = propNode.GetValueEx<int>(0) != 0; break;
                            case "onlyNormalAttack": mobInfo.OnlyNormalAttack = propNode.GetValueEx<int>(0) != 0; break;
                            case "onlyHittedByCommonAttack": mobInfo.OnlyHittedByCommonAttack = propNode.GetValueEx<int>(0) != 0; break;
                            case "elemAttr": mobInfo.ElemAttr = new MobElemAttr(propNode.GetValueEx<string>(null)); break;

                            case "lvOptimum": mobInfo.LvOptimum = propNode.GetValueEx<int>(0) != 0; break;
                            case "changeableMobs":
                                foreach (var subNode in propNode.Nodes)
                                {
                                    if (subNode.Text.StartsWith("changeableMob"))
                                    {
                                        mobInfo.ChangeableMob = propNode.GetValueEx<int>(0) != 0;
                                    }
                                }
                                break;

                            case "link": mobInfo.Link = propNode.GetValueEx<int>(0); break;
                            case "skeleton": mobInfo.Skeleton = propNode.GetValueEx<int>(0) != 0; break;
                            case "jsonLoad": mobInfo.JsonLoad = propNode.GetValueEx<int>(0) != 0; break;
                            case "avatarLook": mobInfo.AvatarLook = propNode; break;
                            //case "skill": LoadSkill(mobInfo, propNode); break;
                            //case "attack": LoadAttack(mobInfo, propNode); break;
                            //case "buff": LoadBuff(mobInfo, propNode); break;
                            case "revive":
                                for (int i = 0; ; i++)
                                {
                                    var reviveNode = propNode.FindNodeByPath(i.ToString());
                                    if (reviveNode == null)
                                    {
                                        break;
                                    }
                                    mobInfo.Revive.Add(reviveNode.GetValue<int>());
                                }
                                break;
                            case "attackPower": mobInfo.AttackPower = propNode.GetValueEx<long>(0) * 10000; break;
                            case "attackPower1": mobInfo.AttackPower = propNode.GetValueEx<long>(0); break;
                        }
                    }
                }
            }

            //读取怪物默认动作
            {
                Wz_Node linkNode = null;
                if (mobInfo.Link != null && findNode != null)
                {
                    linkNode = findNode(string.Format("Mob\\{0:d7}.img", mobInfo.Link));
                }
                if (linkNode == null)
                {
                    linkNode = node;
                }

                var imageFrame = new BitmapOrigin();

                foreach (var action in new[] { @"stand\0", @"move\0", @"fly\0", @"info\thumbnail", @"info\default\0" })
                {
                    var actNode = linkNode.FindNodeByPath(action);
                    imageFrame = BitmapOrigin.CreateFromNode(actNode, findNode);
                    if (imageFrame.Bitmap != null && !(imageFrame.Bitmap.Width == 1 && imageFrame.Bitmap.Height == 1))
                    {
                        break;
                    }
                }

                var overseasAnimSetNode = linkNode.FindNodeByPath("AnimSet") ?? linkNode.FindNodeByPath("DefaultAnims");

                if (overseasAnimSetNode != null && imageFrame.Bitmap == null)
                {
                    foreach (var action in new[] { "stand", "move", "die", "hit", "jump", "appear" })
                    {
                        var actNode = overseasAnimSetNode.FindNodeByPath($"{action}\\LayerSlots\\Slot0\\Segment0\\0") ?? overseasAnimSetNode.FindNodeByPath($"{action}\\LayerSlots\\Slot0\\Segment0\\AnimReference\\0");
                        imageFrame = BitmapOrigin.CreateFromNode(actNode, findNode);
                        if (imageFrame.Bitmap != null && !(imageFrame.Bitmap.Width == 1 && imageFrame.Bitmap.Height == 1))
                        {
                            break;
                        }
                    }
                }

                mobInfo.Default = imageFrame;
            }

            return mobInfo;
        }

        public void Dispose()
        {
            if (this.Default.Bitmap != null)
                this.Default.Bitmap.Dispose();
        }
    }
}
