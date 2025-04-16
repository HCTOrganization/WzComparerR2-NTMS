using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WzComparerR2.CharaSim
{
    public static class ItemStringHelper
    {
        /// <summary>
        /// 获取怪物category属性对应的类型说明。
        /// </summary>
        /// <param Name="category">怪物的category属性的值。</param>
        /// <returns></returns>
        public static string GetMobCategoryName(int category)
        {
            switch (category)
            {
                case 0: return "無形态";
                case 1: return "動物型";
                case 2: return "植物型";
                case 3: return "魚類型";
                case 4: return "爬蟲類型";
                case 5: return "精靈型";
                case 6: return "惡魔型";
                case 7: return "不死型";
                case 8: return "無機物型";
                default: return null;
            }
        }

        public static string GetGearPropString(GearPropType propType, long value)
        {
            return GetGearPropString(propType, value, 0);
        }

        /// <summary>
        /// 获取GearPropType所对应的文字说明。
        /// </summary>
        /// <param Name="propType">表示装备属性枚举GearPropType。</param>
        /// <param Name="Value">表示propType属性所对应的值。</param>
        /// <returns></returns>
        public static string GetGearPropString(GearPropType propType, long value, int signFlag)
        {

            string sign;
            switch (signFlag)
            {
                default:
                case 0: //默认处理符号
                    sign = value > 0 ? "+" : null;
                    break;

                case 1: //固定加号
                    sign = "+";
                    break;

                case 2: //无特别符号
                    sign = "";
                    break;
            }
            switch (propType)
            {
                case GearPropType.incSTR: return "力量 : " + sign + value;
                case GearPropType.incSTRr: return "力量 : " + sign + value + "%";
                case GearPropType.incDEX: return "敏捷 : " + sign + value;
                case GearPropType.incDEXr: return "敏捷 : " + sign + value + "%";
                case GearPropType.incINT: return "智力 : " + sign + value;
                case GearPropType.incINTr: return "智力 : " + sign + value + "%";
                case GearPropType.incLUK: return "幸運 : " + sign + value;
                case GearPropType.incLUKr: return "幸運 : " + sign + value + "%";
                case GearPropType.incAllStat: return "全部屬性: " + sign + value;
                case GearPropType.statR: return "全部屬性: " + sign + value + "%";
                case GearPropType.incMHP: return "MaxHP： " + sign + value;
                case GearPropType.incMHPr: return "MaxHP： " + sign + value + "%";
                case GearPropType.incMMP: return "MaxMP： " + sign + value;
                case GearPropType.incMMPr: return "MaxMP： " + sign + value + "%";
                case GearPropType.incMDF: return "MaxDF : " + sign + value;
                case GearPropType.incPAD: return "攻擊力 : " + sign + value;
                case GearPropType.incPADr: return "攻擊力 : " + sign + value + "%";
                case GearPropType.incMAD: return "魔法攻擊力 : " + sign + value;
                case GearPropType.incMADr: return "魔法攻擊力 : " + sign + value + "%";
                case GearPropType.incPDD: return "防禦力 : " + sign + value;
                case GearPropType.incPDDr: return "防禦力 : " + sign + value + "%";
                //case GearPropType.incMDD: return "MAGIC DEF. : " + sign + value;
                //case GearPropType.incMDDr: return "MAGIC DEF. : " + sign + value + "%";
                //case GearPropType.incACC: return "ACCURACY : " + sign + value;
                //case GearPropType.incACCr: return "ACCURACY : " + sign + value + "%";
                //case GearPropType.incEVA: return "AVOIDABILITY : " + sign + value;
                //case GearPropType.incEVAr: return "AVOIDABILITY : " + sign + value + "%";
                case GearPropType.incSpeed: return "移動速度 : " + sign + value;
                case GearPropType.incJump: return "跳躍力 : " + sign + value;
                case GearPropType.incCraft: return "手藝 : " + sign + value;
                case GearPropType.damR:
                case GearPropType.incDAMr: return "總傷害 : " + sign + value + "%";
                case GearPropType.incCr: return "爆擊率 : " + sign + value + "%";
                case GearPropType.incCDr: return "爆擊傷害 : " + sign + value + "%";
                case GearPropType.knockback: return "直接打擊時的機率強弓:" + value;
                //case GearPropType.incPVPDamage: return "Battle Mode ATT " + sign + " " + value;
                case GearPropType.incPQEXPr: return "组隊任務經驗值增加" + value + "%";
                case GearPropType.incEXPr: return "經驗值增加" + value + "%";
                case GearPropType.incBDR:
                case GearPropType.bdR: return "BOSS攻擊時傷害 +" + value + "%";
                case GearPropType.incIMDR:
                case GearPropType.imdR: return "無視怪物防禦率+" + value + "%";
                case GearPropType.limitBreak: return "傷害上限突破至 " + ToCJKNumberExpr(value);
                case GearPropType.reduceReq: return "裝備等级降低：- " + value;
                case GearPropType.nbdR: return "攻擊一般怪物時傷害+" + value + "%";

                case GearPropType.only: return value == 0 ? null : "專屬道具";
                case GearPropType.tradeBlock: return value == 0 ? null : "無法交換";
                case GearPropType.equipTradeBlock: return value == 0 ? null : "裝備時不可交換";
                case GearPropType.accountSharable: return value == 0 ? null : "只能在同帳號內移動";
                case GearPropType.sharableOnce: return value == 0 ? null : "僅可在同帳號間移動一次\n(移動後無法更換)";
                case GearPropType.onlyEquip: return value == 0 ? null : "只能單獨使用";
                case GearPropType.notExtend: return value == 0 ? null : "無法延長有效時間。";
                case GearPropType.accountSharableAfterExchange: return value == 0 ? null : "可交換1次\n(僅限在相同世界內的我的角色之間轉移)";
                case GearPropType.mintable: return value == 0 ? null : "可鑄造";
                case GearPropType.tradeAvailable:
                    switch (value)
                    {
                        case 1: return "若使用 #c宿命剪刀，該道具可進行一次交易！#";
                        case 2: return "若使用 #c白金神奇剪刀，該道具可進行一次交易！#";
                        default: return null;
                    }
                case GearPropType.accountShareTag:
                    switch (value)
                    {
                        case 1: return " #c若使用分享名牌技術，可以相同帳號內的角色進行移動一次。#";
                        default: return null;
                    }
                //case GearPropType.noPotential: return value == 0 ? null : "This item cannot gain Potential.";
                case GearPropType.noPotential: return value == 0 ? null : "無法設置潛能。";
                case GearPropType.fixedPotential: return value == 0 ? null : "無法重設潛能";
                case GearPropType.superiorEqp: return value == 0 ? null : "道具強化成功時, 可以獲得更高效果。";
                case GearPropType.nActivatedSocket: return value == 0 ? null : "#c可以鑲嵌星岩#";
                case GearPropType.jokerToSetItem: return value == 0 ? null : " c當前裝備3個以上的所有套装道具中包含的幸運物品！#";
                case GearPropType.abilityTimeLimited: return value == 0 ? null : "期間限定能力值";
                case GearPropType.blockGoldHammer: return value == 0 ? null : "無法使用黄金鐵鎚";
                case GearPropType.cantRepair: return value == 0 ? null : "修理不可";
                case GearPropType.colorvar: return value == 0 ? null : "#c此裝備可以通過染色顏料進行染色。#";
                case GearPropType.noLookChange: return value == 0 ? null : "神秘のカナトコ使用不可";

                case GearPropType.incAllStat_incMHP25: return "Allｽﾃｰﾀｽ：" + sign + value + ", 最大HP : " + sign + (value * 25);// check once Lv 250 set comes out in GMS
                case GearPropType.incAllStat_incMHP50_incMMP50: return "Allｽﾃｰﾀｽ：" + sign + value + ", 最大HP / 最大MP : " + sign + (value * 50);
                case GearPropType.incMHP_incMMP: return "最大HP / 最大MP : " + sign + value;
                case GearPropType.incMHPr_incMMPr: return "最大HP / 最大MP : " + sign + value + "%";
                case GearPropType.incPAD_incMAD:
                case GearPropType.incAD: return "攻撃力 / 魔力 : " + sign + value;
                case GearPropType.incPDD_incMDD: return "防御力 : " + sign + value;
                //case GearPropType.incACC_incEVA: return "ACC/AVO :" + sign + value;

                case GearPropType.incARC: return "ARC : " + sign + value;
                case GearPropType.incAUT: return "AUT : " + sign + value;

                case GearPropType.Etuc: return "エクセプショナル強化かできます。 (最大\n\r: " + value + "回)";
                case GearPropType.CuttableCount: return "はさみ使用可能回数：" + value + "回";
                default: return null;
            }
        }


        public static string GetGearPropDiffString(GearPropType propType, int value, int standardValue)
        {
            var propStr = GetGearPropString(propType, value);
            if (value > standardValue)
            {
                string subfix = null;
                string openAPISubfix = "";
                switch (propType)
                {
                    case GearPropType.incSTR:
                    case GearPropType.incDEX:
                    case GearPropType.incINT:
                    case GearPropType.incLUK:
                    case GearPropType.incMHP:
                    case GearPropType.incMMP:
                    case GearPropType.incMDF:
                    case GearPropType.incARC:
                    case GearPropType.incAUT:
                    case GearPropType.incPAD:
                    case GearPropType.incMAD:
                    case GearPropType.incPDD:
                    case GearPropType.incMDD:
                    case GearPropType.incSpeed:
                    case GearPropType.incJump:
                        subfix = $"({standardValue} #$e+{value - standardValue}#)"; break;
                    case GearPropType.bdR:
                    case GearPropType.incBDR:
                    case GearPropType.imdR:
                    case GearPropType.incIMDR:
                    case GearPropType.damR:
                    case GearPropType.incDAMr:
                    case GearPropType.statR:
                        subfix = $"({standardValue}% #$y+{value - standardValue}%#)"; break;

                    case GearPropType.addSTR:
                    case GearPropType.addDEX:
                    case GearPropType.addINT:
                    case GearPropType.addLUK:
                    case GearPropType.addMHP:
                    case GearPropType.addMMP:
                    case GearPropType.addPAD:
                    case GearPropType.addMAD:
                    case GearPropType.addDEF:
                    case GearPropType.addSpeed:
                    case GearPropType.addJump:
                    case GearPropType.addLvlDec:
                        openAPISubfix += $"#$g+{value - standardValue}#"; break;


                    case GearPropType.addBDR:
                    case GearPropType.addDamR:
                    case GearPropType.addAllStatR:
                        openAPISubfix += $"#$g+{value - standardValue}%#"; break;

                    case GearPropType.scrollSTR:
                    case GearPropType.scrollDEX:
                    case GearPropType.scrollINT:
                    case GearPropType.scrollLUK:
                    case GearPropType.scrollMHP:
                    case GearPropType.scrollMMP:
                    case GearPropType.scrollPAD:
                    case GearPropType.scrollMAD:
                    case GearPropType.scrollDEF:
                    case GearPropType.scrollSpeed:
                    case GearPropType.scrollJump:
                        openAPISubfix += $" #$e+{value - standardValue}#"; break;

                    case GearPropType.starSTR:
                    case GearPropType.starDEX:
                    case GearPropType.starINT:
                    case GearPropType.starLUK:
                    case GearPropType.starMHP:
                    case GearPropType.starMMP:
                    case GearPropType.starPAD:
                    case GearPropType.starMAD:
                    case GearPropType.starDEF:
                    case GearPropType.starSpeed:
                    case GearPropType.starJump:
                        openAPISubfix += $" #c+{value - standardValue}#"; break;

                }
                if (openAPISubfix.Length > 0 )
                {
                    openAPISubfix = $"({standardValue}" + openAPISubfix + ")";
                }
                propStr = "#$y" + propStr + "# " + subfix + openAPISubfix;
            }
            return propStr;
        }

        /// <summary>
        /// 获取gearGrade所对应的字符串。
        /// </summary>
        /// <param Name="rank">表示装备的潜能等级GearGrade。</param>
        /// <returns></returns>
        public static string GetGearGradeString(GearGrade rank)
        {
            switch (rank)
            {
                //case GearGrade.C: return "C级(一般物品)";
                case GearGrade.B: return "(特殊道具)";
                case GearGrade.A: return "(稀有道具)";
                case GearGrade.S: return "(罕見道具)";
                case GearGrade.SS: return "(傳說道具)";
                case GearGrade.Special: return "(特殊道具)";
                default: return null;
            }
        }

        /// <summary>
        /// 获取gearType所对应的字符串。
        /// </summary>
        /// <param Name="Type">表示装备类型GearType。</param>
        /// <returns></returns>
        public static string GetGearTypeString(GearType type)
        {
            switch (type)
            {
                case GearType.body: return "紙娃娃(身體)";
                case GearType.head:
                case GearType.head_n: return "紙娃娃(頭部)";
                case GearType.face:
                case GearType.face2:
                case GearType.face_n: return "紙娃娃(臉型)";
                case GearType.hair:
                case GearType.hair2:
                case GearType.hair3:
                case GearType.hair_n: return "紙娃娃(髮型)";
                case GearType.faceAccessory: return "臉飾";
                case GearType.eyeAccessory: return "眼飾";
                case GearType.earrings: return "耳環";
                case GearType.pendant: return "墜飾";
                case GearType.belt: return "腰帶";
                case GearType.medal: return "勳章";
                case GearType.shoulderPad: return "肩膀裝飾";
                case GearType.cap: return "帽子";
                case GearType.cape: return "披風";
                case GearType.coat: return "上衣";
                case GearType.dragonMask: return "龍魔頭盔子";
                case GearType.dragonPendant: return "龍魔項鍊";
                case GearType.dragonWings: return "龍魔翅膀";
                case GearType.dragonTail: return "龍魔尾巴";
                case GearType.glove: return "手套";
                case GearType.longcoat: return "套服";
                case GearType.machineEngine: return "戰神引擎";
                case GearType.machineArms: return "戰神手臂";
                case GearType.machineLegs: return "戰神腿部";
                case GearType.machineBody: return "戰神身軀";
                case GearType.machineTransistors: return "戰神電晶體";
                case GearType.pants: return "褲/裙";
                case GearType.ring: return "戒指";
                case GearType.shield: return "盾牌";
                case GearType.shoes: return "鞋子";
                case GearType.shiningRod: return "閃亮克魯";
                case GearType.soulShooter: return "靈魂射手";
                case GearType.ohSword: return "單手劍";
                case GearType.ohAxe: return "單手斧";
                case GearType.ohBlunt: return "單手棍";
                case GearType.dagger: return "短劍";
                case GearType.katara: return "雙刀";
                case GearType.magicArrow: return "魔法箭";
                case GearType.card: return "卡牌";
                case GearType.box: return "寶盒";
                case GearType.orb: return "夜光彈";
                case GearType.novaMarrow: return "龍之精水";
                case GearType.soulBangle: return "靈魂之環";
                case GearType.mailin: return "連發槍";
                case GearType.cane: return "手杖";
                case GearType.wand: return "短杖";
                case GearType.staff: return "長杖";
                case GearType.thSword: return "雙手劍";
                case GearType.thAxe: return "雙手斧";
                case GearType.thBlunt: return "雙手棍";
                case GearType.spear: return "槍";
                case GearType.polearm: return "矛";
                case GearType.bow: return "弓";
                case GearType.crossbow: return "弩";
                case GearType.throwingGlove: return "拳套";
                case GearType.knuckle: return "指虎";
                case GearType.gun: return "火槍";
                case GearType.android: return "機器人";
                case GearType.machineHeart: return "機器人心臟";
                case GearType.pickaxe: return "採礦";
                case GearType.shovel: return "採藥";
                case GearType.pocket: return "口袋道具";
                case GearType.dualBow: return "雙弩槍";
                case GearType.handCannon: return "加農砲";
                case GearType.badge: return "胸章";
                case GearType.emblem: return "徽章";
                case GearType.soulShield: return "靈魂盾牌";
                case GearType.demonShield: return "力量之盾";
                case GearType.totem: return "圖騰";
                case GearType.petEquip: return "寵物裝備";
                case GearType.taming:
                case GearType.taming2:
                case GearType.taming3:
                case GearType.tamingChair: return "騎寵";
                case GearType.saddle: return "馬鞍";
                case GearType.katana: return "太刀";
                case GearType.fan: return "扇子";
                case GearType.swordZB: return "琉";
                case GearType.swordZL: return "璃";
                case GearType.weapon: return "武器";
                case GearType.subWeapon: return "輔助武器";
                case GearType.heroMedal: return "獎牌";
                case GearType.rosario: return "羅札里歐";
                case GearType.chain: return "鐵鍊";
                case GearType.book1:
                case GearType.book2:
                case GearType.book3: return "魔導書";
                case GearType.bowMasterFeather: return "箭失";
                case GearType.crossBowThimble: return "弓箭指套";
                case GearType.shadowerSheath: return "短劍用劍套";
                case GearType.nightLordPoutch: return "符咒";
                case GearType.viperWristband: return "手環";
                case GearType.captainSight: return "照準器";
                case GearType.cannonGunPowder:
                case GearType.cannonGunPowder2: return "火藥桶";
                case GearType.aranPendulum: return "壓力軸";
                case GearType.evanPaper: return "文件";
                case GearType.battlemageBall: return "魔法珠子";
                case GearType.wildHunterArrowHead: return "箭矢";
                case GearType.cygnusGem: return "寶石";
                case GearType.controller: return "控制";
                case GearType.foxPearl: return "狐狸寶珠";
                case GearType.chess: return "西洋棋";
                case GearType.powerSource: return "能源";

                case GearType.energySword: return "能量劍";
                case GearType.desperado: return "魔劍";
                case GearType.memorialStaff: return "記憶長杖";
                case GearType.magicStick: return "幻獸棍棒";
                case GearType.leaf:
                case GearType.leaf2: return "葉片";
                case GearType.boxingClaw: return "拳爪";
                case GearType.kodachi:
                case GearType.kodachi2: return "小太刀";
                case GearType.espLimiter: return "ESP限制器";

                case GearType.GauntletBuster: return "重拳槍";
                case GearType.ExplosivePill: return "裝填";

                case GearType.chain2: return "鎖鏈";
                case GearType.magicGauntlet: return "魔法護腕";
                case GearType.transmitter: return "武器傳送裝置";
                case GearType.magicWing: return "魔力翅膀";
                case GearType.pathOfAbyss: return "深淵通行";

                case GearType.ancientBow: return "古代之弓";
                case GearType.relic: return "遺物";

                case GearType.handFan: return "仙扇";
                case GearType.fanTassel: return "扇墜";

                case GearType.tuner: return "調節器";
                case GearType.bracelet: return "手鐲";

                case GearType.breathShooter: return "龍息臂箭";
                case GearType.weaponBelt: return "武器腰帶";

                case GearType.ornament: return "飾品";

                case GearType.chakram: return "環刃";
                case GearType.hexSeeker: return "追魂器";

                case GearType.boxingCannon: return "武拳";
                case GearType.boxingSky: return "拳環";

                case GearType.jewel: return "寳玉";
                default: return null;
            }
        }

        /// <summary>
        /// 获取武器攻击速度所对应的字符串。
        /// </summary>
        /// <param Name="attackSpeed">表示武器的攻击速度，通常为2~9的数字。</param>
        /// <returns></returns>
        public static string GetAttackSpeedString(int attackSpeed)
        {
            switch (attackSpeed)
            {
                case 2:
                case 3: return "比較快";
                case 4:
                case 5: return "快";
                case 6: return "普通";
                case 7:
                case 8: return "慢";
                case 9: return "比較慢";
                default:
                    return attackSpeed.ToString();
            }
        }

        /// <summary>
        /// 获取套装装备类型的字符串。
        /// </summary>
        /// <param Name="Type">表示套装装备类型的GearType。</param>
        /// <returns></returns>
        public static string GetSetItemGearTypeString(GearType type)
        {
            return GetGearTypeString(type);
        }

        /// <summary>
        /// 获取装备额外职业要求说明的字符串。
        /// </summary>
        /// <param Name="Type">表示装备类型的GearType。</param>
        /// <returns></returns>
        public static string GetExtraJobReqString(GearType type)
        {
            switch (type)
            {
                //0xxx
                case GearType.heroMedal: return "英雄職業群可套用";
                case GearType.rosario: return "聖騎士職業群可套用";
                case GearType.chain: return "黑骑士職業群可套用";
                case GearType.book1: return "火毒系列魔法師可套用";
                case GearType.book2: return "冰雷系列魔法師可套用";
                case GearType.book3: return "主教系列魔法師可套用";
                case GearType.bowMasterFeather: return "箭神職業群可套用";
                case GearType.crossBowThimble: return "神射手職業群可套用";
                case GearType.relic: return "開拓者職業可穿載";
                case GearType.shadowerSheath: return "暗影神偷職業群可套用";
                case GearType.nightLordPoutch: return "夜使者職業群可套用";
                case GearType.katara: return "影武者可以裝備";
                case GearType.viperWristband: return "拳霸職業群可套用";
                case GearType.captainSight: return "槍神職業群可套用";
                case GearType.cannonGunPowder:
                case GearType.cannonGunPowder2: return "重砲指揮官職業群可套用";
                case GearType.box:
                case GearType.boxingClaw: return "蒼龍俠客着用可能";

                //1xxx
                case GearType.cygnusGem: return "蒼龍俠客可以裝備";

                //2xxx
                case GearType.aranPendulum: return GetExtraJobReqString(21);
                case GearType.dragonMask:
                case GearType.dragonPendant:
                case GearType.dragonWings:
                case GearType.dragonTail:
                case GearType.evanPaper: return GetExtraJobReqString(22);
                case GearType.magicArrow: return GetExtraJobReqString(23);
                case GearType.card: return GetExtraJobReqString(24);
                case GearType.foxPearl: return GetExtraJobReqString(25);
                case GearType.orb:
                case GearType.shiningRod: return GetExtraJobReqString(27);

                //3xxx
                case GearType.demonShield: return GetExtraJobReqString(31);
                case GearType.desperado: return "可以裝備在惡魔職業群上";
                case GearType.battlemageBall: return "煉獄巫師職業群可套用";
                case GearType.wildHunterArrowHead: return "狂豹獵人職業群可套用";
                case GearType.machineEngine:
                case GearType.machineArms:
                case GearType.machineLegs:
                case GearType.machineBody:
                case GearType.machineTransistors:
                case GearType.mailin: return "機甲戰神可套用";
                case GearType.controller:
                case GearType.powerSource:
                case GearType.energySword: return GetExtraJobReqString(36);
                case GearType.GauntletBuster:
                case GearType.ExplosivePill: return GetExtraJobReqString(37);

                //4xxx
                case GearType.katana:
                case GearType.kodachi:
                case GearType.kodachi2: return GetExtraJobReqString(41);
                case GearType.fan: return GetExtraJobReqString(42);

                //5xxx
                case GearType.soulShield: return "可套用米哈逸";

                //6xxx
                case GearType.novaMarrow: return GetExtraJobReqString(61);
                case GearType.weaponBelt:
                case GearType.breathShooter: return GetExtraJobReqString(63);
                case GearType.chain2:
                case GearType.transmitter: return GetExtraJobReqString(64);
                case GearType.soulBangle:
                case GearType.soulShooter: return GetExtraJobReqString(65);

                //10xxx
                case GearType.swordZB:
                case GearType.swordZL: return GetExtraJobReqString(101);

                case GearType.magicStick: return GetExtraJobReqString(112);
                case GearType.leaf:
                case GearType.leaf2:
                case GearType.memorialStaff: return GetExtraJobReqString(172);

                case GearType.espLimiter:
                case GearType.chess: return GetExtraJobReqString(142);

                case GearType.magicGauntlet:
                case GearType.magicWing: return GetExtraJobReqString(152);

                case GearType.pathOfAbyss: return GetExtraJobReqString(155);
                case GearType.handFan:
                case GearType.fanTassel: return GetExtraJobReqString(164);

                case GearType.tuner:
                case GearType.bracelet: return GetExtraJobReqString(151);

                case GearType.boxingCannon:
                case GearType.boxingSky: return GetExtraJobReqString(175);

                case GearType.ornament: return GetExtraJobReqString(162);
                default: return null;
            }
        }

        /// <summary>
        /// 获取装备额外职业要求说明的字符串。
        /// </summary>
        /// <param Name="specJob">表示装备属性的reqSpecJob的值。</param>
        /// <returns></returns>
        public static string GetExtraJobReqString(int specJob)
        {
            switch (specJob)
            {
                case 21: return "狂狼勇士職業群可套用";
                case 22: return "魔龍導士職業群可套用";
                case 23: return "可裝備精靈遊俠";
                case 24: return "可裝備幻影俠盜";
                case 25: return "隐月可套用";
                case 27: return "夜光可套用";
                case 31: return "可以裝備在惡魔職業群上";
                case 36: return "可以套用傑諾";
                case 37: return "可裝備爆拳槍神";
                case 41: return "劍豪可套用";
                case 42: return "可裝備陰陽師";
                case 51: return "可套用米哈逸";
                case 61: return "凱薩可套用";
                case 64: return "可裝備卡蒂娜";
                case 65: return "天使破壞者可套用";
                case 99: return "花狐可套用";
                case 101: return "可以裝備神之子";
                case 112: return "可裝備幻獸師";
                case 142: return "可裝備凱內西斯";
                case 151: return "可裝備阿戴爾";
                case 152: return "可裝備伊利恩";
                case 155: return "亞克可以套用";
                case 162: return "可裝備菈菈";
                case 164: return "可裝備虎影職業群";
                case 172: return "琳恩着用可能";
                case 175: return "墨玄着用可能";

                default: return null;
            }
        }

        public static string GetReqSpecJobMultipleString(int specJob)
        {
            switch (specJob)
            {
                case 1: return "ﾋｰﾛｰ､ﾊﾟﾗﾃﾞｨﾝ､";
                case 2: return "ｱｰｸﾒｲｼﾞ(氷･雷)､ｱｰｸﾒｲｼﾞ(火･毒)､ﾋﾞｼｮｯﾌﾟ\r\n";
                case 4: return "シャドー､";
                case 11: return "ｿｳﾙﾏｽﾀｰ､";
                case 12: return "ﾌﾚｲﾑｳｨｻﾞｰﾄﾞ､";
                case 22: return "ｴｳﾞｧﾝ､";
                case 32: return "ﾊﾞﾄﾙﾒｲｼﾞ､";
                case 172: return "ﾘﾝ､";

                default: return null;
            }
        }

        public static string GetExtraJobReqString(IEnumerable<int> specJobs)
        {
            List<string> extraJobNames = new List<string>();
            foreach (int specJob in specJobs)
            {
                switch (specJob)
                {
                    case 1: extraJobNames.AddRange(new[] { "ﾋｰﾛｰ", "ﾊﾟﾗﾃﾞｨﾝ" }); break;
                    case 2: extraJobNames.AddRange(new[] { "ｱｰｸﾒｲｼﾞ(氷･雷)", "ｱｰｸﾒｲｼﾞ(火･毒)", "ﾋﾞｼｮｯﾌﾟ" }); break;
                    case 4: extraJobNames.Add("シャドー"); break;
                    case 11: extraJobNames.Add("ｿｳﾙﾏｽﾀｰ"); break;
                    case 12: extraJobNames.Add("ﾌﾚｲﾑｳｨｻﾞｰﾄﾞ"); break;
                    case 22: extraJobNames.Add("ｴｳﾞｧﾝ"); break;
                    case 32: extraJobNames.Add("ﾊﾞﾄﾙﾒｲｼﾞ"); break;
                    case 172: extraJobNames.Add("ﾘﾝ"); break;
                    default: extraJobNames.Add(specJob.ToString()); break;
                }
            }
            if (extraJobNames.Count == 0)
            {
                return null;
            }
            return string.Join("､", extraJobNames) + "着用可能";
        }

        public static string GetItemPropString(ItemPropType propType, long value)
        {
            switch (propType)
            {
                case ItemPropType.tradeBlock:
                    return GetGearPropString(GearPropType.tradeBlock, value);
                case ItemPropType.useTradeBlock:
                    return value == 0 ? null : "使用後不可交換";
                case ItemPropType.tradeAvailable:
                    return GetGearPropString(GearPropType.tradeAvailable, value);
                case ItemPropType.only:
                    return GetGearPropString(GearPropType.only, value);
                case ItemPropType.accountSharable:
                    return GetGearPropString(GearPropType.accountSharable, value);
                case ItemPropType.sharableOnce:
                    return GetGearPropString(GearPropType.sharableOnce, value);
                case ItemPropType.accountSharableAfterExchange:
                    return GetGearPropString(GearPropType.accountSharableAfterExchange, value);
                case ItemPropType.exchangeableOnce:
                    return value == 0 ? null : "可交換1次 (交換後不可交換)";
                case ItemPropType.quest:
                    return value == 0 ? null : "任務道具";
                case ItemPropType.pquest:
                    return value == 0 ? null : "組隊任務道具";
                case ItemPropType.permanent:
                    return value == 0 ? null : "魔法の時間が終わらないミラクルペットです。";
                case ItemPropType.multiPet:
                    // return value == 0 ? null : "マルチペット(他のペットと最大3個重複使用可能)";
                    return value == 0 ? "一般ペット(他の一般ペットと重複使用不可)" : "マルチペット(他のペットと最大3個重複使用可能)";
                case ItemPropType.mintable:
                    return GetGearPropString(GearPropType.mintable, value);
                default:
                    return null;
            }
        }

        public static string GetItemCoreSpecString(ItemCoreSpecType coreSpecType, int value, string desc)
        {
            bool hasCoda = false;
            if (desc?.Length > 0)
            {
                char lastCharacter = desc.Last();
                hasCoda = lastCharacter >= '가' && lastCharacter <= '힣' && (lastCharacter - '가') % 28 != 0;
            }
            switch (coreSpecType)
            {
                case ItemCoreSpecType.Ctrl_mobLv:
                    return value == 0 ? null : "Monster Level " + "+" + value;
                case ItemCoreSpecType.Ctrl_mobHPRate:
                    return value == 0 ? null : "Monster HP " + "+" + value + "%";
                case ItemCoreSpecType.Ctrl_mobRate:
                    return value == 0 ? null : "Monster Population " + "+" + value + "%";
                case ItemCoreSpecType.Ctrl_mobRateSpecial:
                    return value == 0 ? null : "Monster Population " + "+" + value + "%";
                case ItemCoreSpecType.Ctrl_change_Mob:
                    return desc == null ? null : "Change monster skins for " + desc;
                case ItemCoreSpecType.Ctrl_change_BGM:
                    return desc == null ? null : "Change music for " + desc;
                case ItemCoreSpecType.Ctrl_change_BackGrnd:
                    return desc == null ? null : "Change background image for " + desc;
                case ItemCoreSpecType.Ctrl_partyExp:
                    return value == 0 ? null : "Party EXP " + "+" + value + "%";
                case ItemCoreSpecType.Ctrl_partyExpSpecial:
                    return value == 0 ? null : "Party EXP " + "+" + value + "%";
                case ItemCoreSpecType.Ctrl_addMob:
                    return value == 0 || desc == null ? null : desc + ", Link " + value + " added to area";
                case ItemCoreSpecType.Ctrl_dropRate:
                    return value == 0 ? null : "Drop Rate " + "+" + value + "%";
                case ItemCoreSpecType.Ctrl_dropRateSpecial:
                    return value == 0 ? null : "Drop Rate " + "+" + value + "%";
                case ItemCoreSpecType.Ctrl_dropRate_Herb:
                    return value == 0 ? null : "Herb Drop Rate " + "+" + value + "%";
                case ItemCoreSpecType.Ctrl_dropRate_Mineral:
                    return value == 0 ? null : "Mineral Drop Rate " + "+" + value + "%";
                case ItemCoreSpecType.Ctrl_dropRareEquip:
                    return value == 0 ? null : "Rare Equipment Drop";
                case ItemCoreSpecType.Ctrl_reward:
                case ItemCoreSpecType.Ctrl_addMission:
                    return desc;
                default:
                    return null;
            }
        }

        public static string GetSkillReqAmount(int skillID, int reqAmount)
        {
            switch (skillID / 10000)
            {
                case 11200: return "[必要なポポSP: " + reqAmount + "]";
                case 11210: return "[必要なライSP: " + reqAmount + "]";
                case 11211: return "[必要なエカSP: " + reqAmount + "]";
                case 11212: return "[必要なアルSP: " + reqAmount + "]";
                default: return "[必要な??SP: " + reqAmount + "]";
            }
        }

        public static string GetJobName(int jobCode)
        {
            switch (jobCode)
            {
                case 0: return "初心者";
                case 100: return "ファイター";
                case 110: return "ソードマン";
                case 111: return "ナイト";
                case 112: return "ヒーロー";
                case 114: return "ヒーロー(6轉)";
                case 120: return "ページ";
                case 121: return "クルセイダー";
                case 122: return "パラディン";
                case 124: return "パラディン(6轉)";
                case 130: return "スピアマン";
                case 131: return "バーサーカー";
                case 132: return "ダークナイト";
                case 134: return "ダークナイト(6轉)";
                case 200: return "マジシャン";
                case 210: return "ウィザード(火・毒)";
                case 211: return "メイジ(火・毒)";
                case 212: return "亞克メイジ(火・毒)";
                case 214: return "亞克メイジ(火・毒)(6轉)";
                case 220: return "ウィザード(氷・雷)";
                case 221: return "メイジ(氷・雷)";
                case 222: return "亞克メイジ(氷・雷)";
                case 224: return "亞克メイジ(氷・雷)(6轉)";
                case 230: return "クレリック";
                case 231: return "プリースト";
                case 232: return "ビショップ";
                case 234: return "ビショップ(6轉)";
                case 300: return "弓使い";
                case 301: return "弓使い";
                case 310: return "ハンター";
                case 311: return "レンジャー";
                case 312: return "ボウマスター";
                case 314: return "ボウマスター(6轉)";
                case 320: return "クロスボウマン";
                case 321: return "スナイパー";
                case 322: return "クロスボウマスター";
                case 324: return "クロスボウマスター(6轉)";
                case 330: return "エンシェントアーチャー";
                case 331: return "チェイサー";
                case 332: return "パスファインダー";
                case 333: return "パスファインダー(5轉)";
                case 334: return "パスファインダー(6轉)";
                case 400: return "ローグ";
                case 410: return "アサシン";
                case 411: return "ハーミット";
                case 412: return "ナイトロード";
                case 414: return "ナイトロード(6轉)";
                case 420: return "シーフ";
                case 421: return "マスターシーフ";
                case 422: return "シャドー";
                case 424: return "シャドー(6轉)";
                case 430: return "セミデュアル";
                case 431: return "デュアル";
                case 432: return "デュアルマスター";
                case 433: return "スラッシャー";
                case 434: return "デュアルブレイド";
                case 436: return "デュアルブレイド(6轉)";
                case 500: return "海賊";
                case 501: return "海賊(キャノン)";
                case 508: return "蒼龍俠客(1轉)";
                case 510: return "インファイター";
                case 511: return "バッカニア";
                case 512: return "バイパー";
                case 514: return "バイパー(6轉)";
                case 520: return "ガンス琳恩ガー";
                case 521: return "ヴァイキング";
                case 522: return "キャプテン";
                case 524: return "キャプテン(6轉)";
                case 530: return "キャノンシューター";
                case 531: return "キャノンブラスター";
                case 532: return "キャノンマスター";
                case 534: return "キャノンマスター(6轉)";
                case 570: return "蒼龍俠客(2轉)";
                case 571: return "蒼龍俠客(3轉)";
                case 572: return "蒼龍俠客(4轉)";

                case 800: 
                case 900: return "運用者";

                case 1000: return "ノーブレス";
                case 1100: return "ソウルマスター(1轉)";
                case 1110: return "ソウルマスター(2轉)";
                case 1111: return "ソウルマスター(3轉)";
                case 1112: return "ソウルマスター(4轉)";
                case 1114: return "ソウルマスター((6轉)";
                case 1200: return "フレイムウィザード(1轉)";
                case 1210: return "フレイムウィザード(2轉)";
                case 1211: return "フレイムウィザード(3轉)";
                case 1212: return "フレイムウィザード(4轉)";
                case 1214: return "フレイムウィザード(6轉)";
                case 1300: return "ウインドシューター(1轉)";
                case 1310: return "ウインドシューター(2轉)";
                case 1311: return "ウインドシューター(3轉)";
                case 1312: return "ウィンドシューター(4轉)";
                case 1314: return "ウィンドシューター(6轉)";
                case 1400: return "ナイトウォーカー(1轉)";
                case 1410: return "ナイトウォーカー(2轉)";
                case 1411: return "ナイトウォーカー(3轉)";
                case 1412: return "ナイトウォーカー(4轉)";
                case 1414: return "ナイトウォーカー(6轉)";
                case 1500: return "ストライカー(1轉)";
                case 1510: return "ストライカー(2轉)";
                case 1511: return "ストライカー(3轉)";
                case 1512: return "ストライカー(4轉)";
                case 1514: return "ストライカー(6轉)";


                case 2000: return "アラン";
                case 2001: return "エヴァン";
                case 2002: return "メルセデス";
                case 2003: return "ファントム";
                case 2004: return "ルミナス";
                case 2005: return "隠月";
                case 2100: return "アラン(1轉)";
                case 2110: return "アラン(2轉)";
                case 2111: return "アラン(3轉)";
                case 2112: return "アラン(4轉)";
                case 2114: return "アラン(6轉)";
                case 2200:
                case 2210: return "エヴァン(1轉)";
                case 2211:
                case 2212:
                case 2213: return "エヴァン(2轉)";
                case 2214:
                case 2215:
                case 2216: return "エヴァン(3轉)";
                case 2217:
                case 2218: return "エヴァン(4轉)";
                case 2220: return "エヴァン(6轉)";
                case 2300: return "メルセデス(1轉)";
                case 2310: return "メルセデス(2轉)";
                case 2311: return "メルセデス(3轉)";
                case 2312: return "メルセデス(4轉)";
                case 2314: return "メルセデス(6轉)";
                case 2400: return "ファントム(1轉)";
                case 2410: return "ファントム(2轉)";
                case 2411: return "ファントム(3轉)";
                case 2412: return "ファントム(4轉)";
                case 2414: return "ファントム(6轉)";
                case 2500: return "隠月(1轉)";
                case 2510: return "隠月(2轉)";
                case 2511: return "隠月(3轉)";
                case 2512: return "隠月(4轉)";
                case 2514: return "隠月(6轉)";
                case 2700: return "ルミナス(1轉)";
                case 2710: return "ルミナス(2轉)";
                case 2711: return "ルミナス(3轉)";
                case 2712: return "ルミナス(4轉)";
                case 2714: return "ルミナス(6轉)";


                case 3000: return "市民";
                case 3001: return "デーモン";
                case 3100: return "デーモンスレイヤー(1轉)";
                case 3110: return "デーモンスレイヤー(2轉)";
                case 3111: return "デーモンスレイヤー(3轉)";
                case 3112: return "デーモンスレイヤー(4轉)";
                case 3114: return "デーモンスレイヤー(6轉)";
                case 3101: return "デーモンアヴェンジャー(1轉)";
                case 3120: return "デーモンアヴェンジャー(2轉)";
                case 3121: return "デーモンアヴェンジャー(3轉)";
                case 3122: return "デーモンアヴェンジャー(4轉)";
                case 3124: return "デーモンアヴェンジャー(6轉)";
                case 3200: return "バトルメイジ(1轉)";
                case 3210: return "バトルメイジ(2轉)";
                case 3211: return "バトルメイジ(3轉)";
                case 3212: return "バトルメイジ(4轉)";
                case 3214: return "バトルメイジ(6轉)";
                case 3300: return "ワイルドハンター(1轉)";
                case 3310: return "ワイルドハンター(2轉)";
                case 3311: return "ワイルドハンター(3轉)";
                case 3312: return "ワイルドハンター(4轉)";
                case 3314: return "ワイルドハンター(6轉)";
                case 3500: return "メカニック(1轉)";
                case 3510: return "メカニック(2轉)";
                case 3511: return "メカニック(3轉)";
                case 3512: return "メカニック(4轉)";
                case 3514: return "メカニック(6轉)";
                case 3002: return "ゼノン";
                case 3600: return "ゼノン(1轉)";
                case 3610: return "ゼノン(2轉)";
                case 3611: return "ゼノン(3轉)";
                case 3612: return "ゼノン(4轉)";
                case 3614: return "ゼノン(6轉)";
                case 3700: return "ブラスター(1轉)";
                case 3710: return "ブラスター(2轉)";
                case 3711: return "ブラスター(3轉)";
                case 3712: return "ブラスター(4轉)";
                case 3714: return "ブラスター(6轉)";

                case 4001: return "ハヤト";
                case 4002: return "カンナ";
                case 4100: return "ハヤト(1轉)";
                case 4110: return "ハヤト(2轉)";
                case 4111: return "ハヤト(3轉)";
                case 4112: return "ハヤト(4轉)";
                case 4114: return "ハヤト(6轉)";
                case 4200: return "カンナ(1轉)";
                case 4210: return "カンナ(2轉)";
                case 4211: return "カンナ(3轉)";
                case 4212: return "カンナ(4轉)";
                case 4216: return "カンナ(6轉)";


                case 5000: return "ミハエル";
                case 5100: return "ミハエル(1轉)";
                case 5110: return "ミハエル(2轉)";
                case 5111: return "ミハエル(3轉)";
                case 5112: return "ミハエル(4轉)";
                case 5114: return "ミハエル(6轉)";


                case 6000: return "カイザー";
                case 6001: return "エンジェリックバスター";
                case 6002: return "カデナ";
                case 6003: return "カイン";
                case 6100: return "カイザー(1轉)";
                case 6110: return "カイザー(2轉)";
                case 6111: return "カイザー(3轉)";
                case 6112: return "カイザー(4轉)";
                case 6114: return "カイザー(6轉)";
                case 6300: return "カイン(1轉)";
                case 6310: return "カイン(2轉)";
                case 6311: return "カイン(3轉)";
                case 6312: return "カイン(4轉)";
                case 6314: return "カイン(6轉)";
                case 6400: return "カデナ(1轉)";
                case 6410: return "カデナ(2轉)";
                case 6411: return "カデナ(3轉)";
                case 6412: return "カデナ(4轉)";
                case 6414: return "カデナ(6轉)";
                case 6500: return "エンジェリックバスター(1轉)";
                case 6510: return "エンジェリックバスター(2轉)";
                case 6511: return "エンジェリックバスター(3轉)";
                case 6512: return "エンジェリックバスター(4轉)";
                case 6514: return "エンジェリックバスター(6轉)";

                case 7000: return "アビリティ";
                case 7100: return "ユニオン";
                case 7200: return "モンスターライフ";


                case 9100: return "ギルド";
                case 9200:
                case 9201:
                case 9202:
                case 9203:
                case 9204: return "専業技術";

                case 10000: return "ゼロ";
                case 10100: return "ゼロ(1轉)";
                case 10110: return "ゼロ(2轉)";
                case 10111: return "ゼロ(3轉)";
                case 10112: return "ゼロ(4轉)";
                case 10114: return "ゼロ(6轉)";

                case 11000: return "ビーストテイマー";
                case 11200: return "ビーストテイマー(ポポ)";
                case 11210: return "ビーストテイマー(ライ)";
                case 11211: return "ビーストテイマー(エカ)";
                case 11212: return "ビーストテイマー(アル)";

                case 12000:
                case 12005:
                case 12100: return "竈門炭治郎";

                case 13000: return "ピンクビーン";
                case 13001: return "イェティ";
                case 13100: return "ピンクビーン";
                case 13500: return "イェティ";

                case 14000: return "キネシス";
                case 14200: return "キネシス(1轉)";
                case 14210: return "キネシス(2轉)";
                case 14211: return "キネシス(3轉)";
                case 14212: return "キネシス(4轉)";
                case 14213: return "キネシス(5轉)";
                case 14214: return "キネシス(6轉)";

                case 15000: return "イリウム";
                case 15001: return "亞克";
                case 15002: return "アデル";
                case 15003: return "卡莉";
                case 15100: return "アデル(1轉)";
                case 15110: return "アデル(2轉)";
                case 15111: return "アデル(3轉)";
                case 15112: return "アデル(4轉)";
                case 15114: return "アデル(6轉)";
                case 15200: return "イリウム(1轉)";
                case 15210: return "イリウム(2轉)";
                case 15211: return "イリウム(3轉)";
                case 15212: return "イリウム(4轉)";
                case 15214: return "イリウム(6轉)";
                case 15400: return "卡莉(1轉)";
                case 15410: return "卡莉(2轉)";
                case 15411: return "卡莉(3轉)";
                case 15412: return "卡莉(4轉)";
                case 15414: return "卡莉(6轉)";
                case 15500: return "亞克(1轉)";
                case 15510: return "亞克(2轉)";
                case 15511: return "亞克(3轉)";
                case 15512: return "亞克(4轉)";
                case 15514: return "亞克(6轉)";

                case 16000: return "アニマ盗賊";
                case 16001: return "菈菈";
                case 16200: return "菈菈(1轉)";
                case 16210: return "菈菈(2轉)";
                case 16211: return "菈菈(3轉)";
                case 16212: return "菈菈(4轉)";
                case 16214: return "菈菈(6轉)";
                case 16400: return "虎影(1轉)";
                case 16410: return "虎影(2轉)";
                case 16411: return "虎影(3轉)";
                case 16412: return "虎影(4轉)";
                case 16414: return "虎影(6轉)";

                case 17000: return "墨玄";
                case 17001: return "琳恩";
                case 17200: return "琳恩(1轉)";
                case 17210: return "琳恩(2轉)";
                case 17211: return "琳恩(3轉)";
                case 17212: return "琳恩(4轉)";
                case 17214: return "琳恩(6轉)";
                case 17500: return "墨玄(1轉)";
                case 17510: return "墨玄(2轉)";
                case 17511: return "墨玄(3轉)";
                case 17512: return "墨玄(4轉)";
                case 17514: return "墨玄(6轉)";

                case 40000: return "5轉";
                case 40001: return "5轉(戦士)";
                case 40002: return "5轉(魔法使い)";
                case 40003: return "5轉(弓使い)";
                case 40004: return "5轉(盗賊)";
                case 40005: return "5轉(海賊)";


                case 50000: return "6次";
                case 50006: return "6次(強化コア)";
                case 50007: return "6次(ヘキサスタット)";
            }
            return null;
        }

        private static string ToCJKNumberExpr(long value)
        {
            var sb = new StringBuilder(16);
            bool firstPart = true;
            if (value < 0)
            {
                sb.Append("-");
                value = -value; // just ignore the exception -2147483648
            }
            if (value >= 1_0000_0000)
            {
                long part = value / 1_0000_0000;
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

            return sb.Length > 0 ? sb.ToString() : "0";
        }
    }
}
