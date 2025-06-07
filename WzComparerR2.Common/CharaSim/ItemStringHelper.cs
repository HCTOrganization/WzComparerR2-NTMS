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

        public static string[] GetGearPropString3(GearPropType propType, long value)
        {
            return GetGearPropString3(propType, value, 0);
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
                case GearPropType.knockback: return "直接打擊時，以 " + value + "%的機率強弓";
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
                case GearPropType.superiorEqp: return value == 0 ? null : "道具強化成功時, 可以獲得更高效果. ";
                case GearPropType.nActivatedSocket: return value == 0 ? null : "#c可以鑲嵌星岩#";
                case GearPropType.jokerToSetItem: return value == 0 ? null : " #c當前裝備3個以上的所有套装道具中包含的幸運物品！#";
                case GearPropType.abilityTimeLimited: return value == 0 ? null : "期間限定能力值";
                case GearPropType.blockGoldHammer: return value == 0 ? null : "無法使用黄金鐵鎚";
                case GearPropType.cantRepair: return value == 0 ? null : "無法修理";
                case GearPropType.colorvar: return value == 0 ? null : "#c此裝備可以通過染色顏料進行染色。#";
                case GearPropType.noLookChange: return value == 0 ? null : "無法使用神秘鐵砧";

                case GearPropType.incAllStat_incMHP25: return "全部屬性：" + sign + value + ", 最大HP : " + sign + (value * 25);// check once Lv 250 set comes out in GMS
                case GearPropType.incAllStat_incMHP50_incMMP50: return "全部屬性：" + sign + value + ", 最大HP / 最大MP : " + sign + (value * 50);
                case GearPropType.incMHP_incMMP: return "最大HP / 最大MP : " + sign + value;
                case GearPropType.incMHPr_incMMPr: return "最大HP / 最大MP : " + sign + value + "%";
                case GearPropType.incPAD_incMAD:
                case GearPropType.incAD: return "攻擊力 / 魔法攻擊力 : " + sign + value;
                case GearPropType.incPDD_incMDD: return "防御力 : " + sign + value;
                //case GearPropType.incACC_incEVA: return "ACC/AVO :" + sign + value;

                case GearPropType.incARC: return "ARC : " + sign + value;
                case GearPropType.incAUT: return "AUT : " + sign + value;

                case GearPropType.Etuc: return "可進行卓越強化。 (最大\n\r: " + value + "次)";
                case GearPropType.CuttableCount: return "可使用剪刀次數：" + value + "次";
                default: return null;
            }
        }

        public static string[] GetGearPropString3(GearPropType propType, long value, int signFlag)
        {
            string[] res = new string[2];
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
                case GearPropType.incSTR:
                    res[0] = "STR";
                    res[1] = sign + value;
                    return res;
                case GearPropType.incSTRr:
                    res[0] = "STR";
                    res[1] = sign + value + "%";
                    return res;
                case GearPropType.incDEX:
                    res[0] = "DEX";
                    res[1] = sign + value;
                    return res;
                case GearPropType.incDEXr:
                    res[0] = "DEX";
                    res[1] = sign + value + "%";
                    return res;
                case GearPropType.incINT:
                    res[0] = "INT";
                    res[1] = sign + value;
                    return res;
                case GearPropType.incINTr:
                    res[0] = "INT";
                    res[1] = sign + value + "%";
                    return res;
                case GearPropType.incLUK:
                    res[0] = "LUK";
                    res[1] = sign + value;
                    return res;
                case GearPropType.incLUKr:
                    res[0] = "LUK";
                    res[1] = sign + value + "%";
                    return res;
                case GearPropType.incAllStat:
                    res[0] = "全部屬性";
                    res[1] = sign + value;
                    return res;
                case GearPropType.statR:
                    res[0] = "全部屬性";
                    res[1] = sign + value + "%";
                    return res;
                case GearPropType.incMHP:
                    res[0] = "最大HP";
                    res[1] = sign + value;
                    return res;
                case GearPropType.incMHPr:
                    res[0] = "最大HP";
                    res[1] = sign + value + "%";
                    return res;
                case GearPropType.incMMP:
                    res[0] = "最大MP";
                    res[1] = sign + value;
                    return res;
                case GearPropType.incMMPr:
                    res[0] = "最大MP";
                    res[1] = sign + value + "%";
                    return res;
                case GearPropType.incMDF:
                    res[0] = "最大DF";
                    res[1] = sign + value;
                    return res;
                case GearPropType.incPAD:
                    res[0] = "攻擊力";
                    res[1] = sign + value;
                    return res;
                case GearPropType.incPADr:
                    res[0] = "攻擊力";
                    res[1] = sign + value + "%";
                    return res;
                case GearPropType.incMAD:
                    res[0] = "魔法攻擊力";
                    res[1] = sign + value;
                    return res;
                case GearPropType.incMADr:
                    res[0] = "魔法攻擊力";
                    res[1] = sign + value + "%";
                    return res;
                case GearPropType.incPDD:
                    res[0] = "防御力";
                    res[1] = sign + value;
                    return res;
                case GearPropType.incPDDr:
                    res[0] = "防御力";
                    res[1] = sign + value + "%";
                    return res;
                case GearPropType.incSpeed:
                    res[0] = "移動速度";
                    res[1] = sign + value;
                    return res;
                case GearPropType.incJump:
                    res[0] = "跳躍力";
                    res[1] = sign + value;
                    return res;
                case GearPropType.incCraft:
                    res[0] = "手藝";
                    res[1] = sign + value;
                    return res;
                case GearPropType.damR:
                case GearPropType.incDAMr:
                    res[0] = "總傷害";
                    res[1] = sign + value + "%";
                    return res;
                case GearPropType.incCr:
                    res[0] = "爆擊率";
                    res[1] = sign + value + "%";
                    return res;
                case GearPropType.incCDr:
                    res[0] = "爆擊傷害";
                    res[1] = sign + value + "%";
                    return res;
                case GearPropType.knockback:
                    res[0] = "直接打擊時，以 " + value + "%的機率強弓";
                    return res;
                case GearPropType.incPQEXPr:
                    res[0] = "组隊任務經驗值增加";
                    res[1] = value + "%";
                    return res;
                case GearPropType.incBDR:
                case GearPropType.bdR:
                    res[0] = "BOSS傷害";
                    res[1] = "+" + value + "%";
                    return res;
                case GearPropType.incIMDR:
                case GearPropType.imdR:
                    res[0] = "無視怪物防禦率";
                    res[1] = "+" + value + "%";
                    return res;
                case GearPropType.limitBreak:
                    res[0] = "傷害上限";
                    res[1] = ToCJKNumberExpr(value);
                    return res;
                    /*
                case GearPropType.attackSpeed:
                    if (2 <= value && value <= 9)
                    {
                        res[0] = "攻擊速度";
                        res[1] = $"{10 - value}段階";
                    }
                    return res;
                    */
                case GearPropType.nbdR:
                    res[0] = "攻擊一般怪物時傷害";
                    res[1] = "+" + value + "%";
                    return res;
                case GearPropType.incARC:
                    res[0] = "ARC";
                    res[1] = sign + value;
                    return res;
                case GearPropType.incAUT:
                    res[0] = "AUT";
                    res[1] = sign + value;
                    return res;
                case GearPropType.incCHUC:
                    res[0] = "星力";
                    res[1] = sign + value;
                    return res;

                case GearPropType.tradeBlock:
                    res[0] = value == 0 ? null : "#$r無法交換#";
                    return res;
                case GearPropType.accountSharable:
                    res[0] = value == 0 ? null : "#$r只能在同帳號內移動#";
                    return res;
                case GearPropType.sharableOnce:
                    res[0] = value == 0 ? null : "#$r僅可在同帳號間移動一次\n(移動後無法更換)#";
                    return res;
                case GearPropType.only:
                    res[0] = value == 0 ? null : "#$r專屬道具#";
                    return res;
                case GearPropType.onlyEquip:
                    res[0] = value == 0 ? null : "#$r只能單獨使用#";
                    return res;
                case GearPropType.equipTradeBlock:
                    res[0] = value == 0 ? null : "#$r裝備時不可交換#";
                    return res;
                case GearPropType.notExtend:
                    res[0] = value == 0 ? null : " (無法延長)";
                    return res;
                case GearPropType.accountSharableAfterExchange:
                    res[0] = value == 0 ? null : "#$r可交換1次\n(僅限在相同世界內的我的角色之間轉移)#";
                    return res;
                case GearPropType.timeLimited:
                    res[0] = value == 0 ? null : "期限制道具";
                    return res;
                case GearPropType.abilityTimeLimited:
                    res[0] = value == 0 ? null : "期間限定能力値";
                    return res;
                case GearPropType.noLookChange:
                    res[0] = value == 0 ? null : "#$r無法使用神秘鐵砧#";
                    return res;
                case GearPropType.mintable:
                    res[0] = value == 0 ? null : "#$r可鑄造#";
                    return res;
                case GearPropType.tradeAvailable:
                    switch (value)
                    {
                        case 1:
                            res[0] = "#$g若使用 #c宿命剪刀，該道具可進行一次交易#";
                            return res;
                        case 2:
                            res[0] = "#$g若使用 #c白金神奇剪刀，該道具可進行一次交易#";
                            return res;
                        default: return res;
                    }
                case GearPropType.accountShareTag:
                    switch (value)
                    {
                        case 1:
                            res[0] = "#c若使用分享名牌技術，可以相同帳號內的角色進行移動一次。#";
                            return res;
                        default: return res;
                    }
                //case GearPropType.noPotential: return value == 0 ? null : "잠재능력 설정 불가";
                //case GearPropType.fixedPotential: return value == 0 ? null : "잠재능력 재설정 불가";
                case GearPropType.superiorEqp:
                    res[0] = value == 0 ? null : "道具強化成功時, 可以獲得更高效果. ";
                    return res;
                //case GearPropType.jokerToSetItem: return value == 0 ? null : "#c3개 이상 착용하고 있는 모든 세트 아이템에 포함되는 럭키 아이템! (단, 2개 이상의 럭키 아이템 착용 시 1개만 효과 적용.)#";
                //case GearPropType.cantRepair: return value == 0 ? null : "수리 불가";

                case GearPropType.incAllStat_incMHP25:
                    res[0] = "全部屬性  " + sign + value + ", 最大HP  " + sign + (value * 25);
                    return res;
                case GearPropType.incAllStat_incMHP50_incMMP50:
                    res[0] = "全部屬性  " + sign + value + ", 最大HP / 最大MP  " + sign + (value * 50);
                    return res;
                case GearPropType.incMHP_incMMP:
                    res[0] = "最大HP / 最大MP  " + sign + value;
                    return res;
                case GearPropType.incMHPr_incMMPr:
                    res[0] = "最大HP / 最大MP  " + sign + value + "%";
                    return res;
                case GearPropType.incPAD_incMAD:
                case GearPropType.incAD:
                    res[0] = "攻擊力 / 魔法攻擊力  " + sign + " " + value;
                    return res;
                case GearPropType.incPDD_incMDD:
                    res[0] = "防御力  " + sign + value;
                    return res;

                case GearPropType.Etuc:
                    res[0] = $"#$d卓越強化 : 無# (最大{value}次)";
                    return res;
                case GearPropType.CuttableCount:
                    res[0] = $" #$r(可使用剪刀次数: {value} / {value})#";
                    return res;

                case GearPropType.incEXPr:
                default: return res;
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

        public static string[] GetGearPropDiffString3(GearPropType propType, int value, int standardValue)
        {
            string[] res = new string[3];

            var propStr = GetGearPropString3(propType, value, 0);
            res[0] = propStr[0];
            res[1] = propStr[1];

            if (value > standardValue)
            {
                string suffix = null;
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
                        suffix = $"({standardValue} #$e+{value - standardValue}#)"; break;
                    case GearPropType.bdR:
                    case GearPropType.incBDR:
                    case GearPropType.imdR:
                    case GearPropType.incIMDR:
                    case GearPropType.damR:
                    case GearPropType.incDAMr:
                    case GearPropType.statR:
                        suffix = $"({standardValue}% #$e+{value - standardValue}%#)"; break;
                }
                res[2] = suffix;
            }
            return res;
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
                case GearType.dragonMask: return "龍使者帽子";
                case GearType.dragonPendant: return "龍使者墜飾";
                case GearType.dragonWings: return "龍使者翅膀";
                case GearType.dragonTail: return "龍使者尾巴";
                case GearType.glove: return "手套";
                case GearType.longcoat: return "套服";
                case GearType.machineEngine: return "機甲戰神引擎";
                case GearType.machineArms: return "機甲戰神手臂";
                case GearType.machineLegs: return "機甲戰神腿部";
                case GearType.machineBody: return "機甲戰神身軀";
                case GearType.machineTransistors: return "機甲戰神晶體管";
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
                case GearType.novaMarrow: return "龍之精華";
                case GearType.soulBangle: return "靈魂戒指";
                case GearType.mailin: return "萬能彈藥";
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
                case GearType.rosario: return "念珠";
                case GearType.chain: return "鐵鍊";
                case GearType.book1:
                case GearType.book2:
                case GearType.book3: return "魔導書";
                case GearType.bowMasterFeather: return "箭羽";
                case GearType.crossBowThimble: return "弓箭指套";
                case GearType.shadowerSheath: return "短劍用劍套";
                case GearType.nightLordPoutch: return "符咒";
                case GearType.viperWristband: return "手環";
                case GearType.captainSight: return "瞄準器";
                case GearType.cannonGunPowder:
                case GearType.cannonGunPowder2: return "火藥桶";
                case GearType.aranPendulum: return "重錘";
                case GearType.evanPaper: return "文件";
                case GearType.battlemageBall: return "魔法珠子";
                case GearType.wildHunterArrowHead: return "火箭頭";
                case GearType.cygnusGem: return "寶石";
                case GearType.controller: return "操縱桿";
                case GearType.foxPearl: return "狐狸寶珠";
                case GearType.chess: return "西洋棋";
                case GearType.powerSource: return "能源";

                case GearType.energySword: return "能量劍";
                case GearType.desperado: return "魔劍";
                case GearType.memorialStaff: return "記憶長杖";
                case GearType.magicStick: return "幻獸棍棒";
                case GearType.leaf:
                case GearType.leaf2: return "葉子";
                case GearType.boxingClaw: return "拳爪";
                case GearType.kodachi:
                case GearType.kodachi2: return "小太刀";
                case GearType.espLimiter: return "ESP限制器";

                case GearType.GauntletBuster: return "重拳槍";
                case GearType.ExplosivePill: return "裝填";

                case GearType.chain2: return "鎖鏈";
                case GearType.magicGauntlet: return "魔法護腕";
                case GearType.transmitter: return "發信器";
                case GearType.magicWing: return "魔法翅膀";
                case GearType.pathOfAbyss: return "深淵通行";

                case GearType.ancientBow: return "古代之弓";
                case GearType.relic: return "遺跡";

                case GearType.handFan: return "仙扇";
                case GearType.fanTassel: return "扇墜";

                case GearType.tuner: return "調節器";
                case GearType.bracelet: return "手鐲";

                case GearType.breathShooter: return "龍息射手";
                case GearType.weaponBelt: return "武器腰封";

                case GearType.ornament: return "掛飾";

                case GearType.chakram: return "環刃";
                case GearType.hexSeeker: return "能量手環";

                case GearType.boxingCannon: return "武拳";
                case GearType.boxingSky: return "拳環";

                case GearType.arcaneSymbol: return "秘法符文";
                case GearType.authenticSymbol: return "真實符文";
                case GearType.grandAuthenticSymbol: return "豪華真實符文";

                case GearType.jewel: return "寳玉";

                case GearType.celestialLight: return "天之星光權杖";
                case GearType.compass: return "羅盤";

                case GearType.gram: return "克拉";
                case GearType.keir: return "克伊勒";

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
                case GearType.heroMedal: return "可裝備英雄職業";
                case GearType.rosario: return "可裝備聖騎士職業";
                case GearType.chain: return "可裝備黑骑士職業";
                case GearType.book1: return "可裝備火毒系列魔法師";
                case GearType.book2: return "可裝備冰電系列魔法師";
                case GearType.book3: return "可裝備主教系列魔法師";
                case GearType.bowMasterFeather: return "可裝備箭神職業";
                case GearType.crossBowThimble: return "可裝備神射手職業";
                case GearType.relic: return "可裝備開拓者職業群";
                case GearType.shadowerSheath: return "可裝備暗影神偷職業";
                case GearType.nightLordPoutch: return "可裝備夜使者職業";
                case GearType.katara: return "可裝備影武者職業";
                case GearType.viperWristband: return "可裝備拳霸職業";
                case GearType.captainSight: return "可裝備槍神職業";
                case GearType.cannonGunPowder:
                case GearType.cannonGunPowder2: return "可裝備重砲指揮官職業";
                case GearType.box:
                case GearType.boxingClaw: return "可裝備蒼龍俠客";

                //1xxx
                case GearType.cygnusGem: return "可裝備蒼龍俠客";

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
                case GearType.desperado: return "可裝備惡魔復仇者";
                case GearType.battlemageBall: return "可裝備煉獄巫師職業";
                case GearType.wildHunterArrowHead: return "可裝備狂豹獵人";
                case GearType.machineEngine:
                case GearType.machineArms:
                case GearType.machineLegs:
                case GearType.machineBody:
                case GearType.machineTransistors:
                case GearType.mailin: return "可裝備機甲戰神";
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
                case GearType.soulShield: return "可裝備米哈逸";

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

                //18xxx
                case GearType.gram:
                case GearType.keir: return GetExtraJobReqString(181);

                case GearType.celestialLight:
                case GearType.compass: return GetExtraJobReqString(182);
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
                case 21: return "可裝備狂狼勇士";
                case 22: return "可裝備魔龍導士";
                case 23: return "可裝備精靈遊俠";
                case 24: return "可裝備幻影俠盜";
                case 25: return "可裝備隐月";
                case 27: return "可裝備夜光";
                case 31: return "可裝備惡魔職業";
                case 36: return "可裝備傑諾";
                case 37: return "可裝備爆拳槍神";
                case 41: return "可裝備劍豪";
                case 42: return "可裝備陰陽師";
                case 51: return "可裝備米哈逸";
                case 61: return "可裝備凱薩";
                case 64: return "可裝備卡蒂娜";
                case 65: return "可裝備天使破壞者";
                case 99: return "可裝備花狐";
                case 101: return "可裝備神之子";
                case 112: return "可裝備幻獸師";
                case 142: return "可裝備凱內西斯";
                case 151: return "可裝備阿戴爾";
                case 152: return "可裝備伊利恩";
                case 155: return "可裝備亞克";
                case 162: return "可裝備菈菈";
                case 164: return "可裝備虎影";
                case 172: return "可裝備琳恩";
                case 175: return "可裝備墨玄";
                case 181: return "可裝備葉里";
                case 182: return "可裝備施亞";
                default: return null;
            }
        }

        public static string GetReqSpecJobMultipleString(int specJob)
        {
            switch (specJob)
            {
                case 1: return "英雄、聖騎士、";
                case 2: return "大魔導士、主教、";
                case 4: return "暗影神偷、";
                case 11: return "聖魂騎士";
                case 12: return "烈焰巫師\r\n";
                case 22: return "龍魔導士、";
                case 32: return "煉獄巫師、";
                case 172: return "琳恩";

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
                    case 1: extraJobNames.AddRange(new[] { "英雄", "聖騎士" }); break;
                    case 2: extraJobNames.AddRange(new[] { "大魔導士", "主教" }); break;
                    case 4: extraJobNames.Add("暗影神偷"); break;
                    case 11: extraJobNames.Add("聖魂騎士"); break;
                    case 12: extraJobNames.Add("烈焰巫師"); break;
                    case 22: extraJobNames.Add("\r\n龍魔導士"); break;
                    case 32: extraJobNames.Add("煉獄巫師"); break;
                    case 172: extraJobNames.Add("琳恩"); break;
                    default: extraJobNames.Add(specJob.ToString()); break;
                }
            }
            if (extraJobNames.Count == 0)
            {
                return null;
            }
            return "可裝備" + string.Join("、", extraJobNames);
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
                    return value == 0 ? null : "魔法時間不會結束的奇幻寵物。";
                case ItemPropType.multiPet:
                    // return value == 0 ? null : "マルチペット(他のペットと最大3個重複使用可能)";
                    return value == 0 ? "普通寵物（不能與其他相同普通寵物一起使用）" : "多隻寵物（最多可使用3隻相同寵物）";
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
                case 100: return "劍士";
                case 110: return "狂戰士";
                case 111: return "十字軍";
                case 112: return "英雄";
                case 114: return "英雄(6轉)";
                case 120: return "見習騎士";
                case 121: return "騎士";
                case 122: return "聖騎士";
                case 124: return "聖騎士(6轉)";
                case 130: return "槍騎兵";
                case 131: return "嗜血狂騎";
                case 132: return "黑騎士";
                case 134: return "黑騎士(6轉)";
                case 200: return "法師";
                case 210: return "巫師（火，毒）";
                case 211: return "魔導士（火，毒）";
                case 212: return "大魔導士（火，毒）";
                case 214: return "大魔導士（火，毒）(6轉)";
                case 220: return "巫師（冰，雷）";
                case 221: return "魔導士（冰，雷）";
                case 222: return "大魔導士（冰，雷）";
                case 224: return "大魔導士（冰，雷）(6轉)";
                case 230: return "僧侶";
                case 231: return "祭司";
                case 232: return "主教";
                case 234: return "主教(6轉)";
                case 300: return "弓箭手";
                case 301: return "弓箭手";
                case 310: return "獵人";
                case 311: return "遊俠";
                case 312: return "箭神";
                case 314: return "箭神";
                case 320: return "弩弓手";
                case 321: return "狙擊手";
                case 322: return "神射手";
                case 324: return "神射手(6轉)";
                case 330: return "古代弓箭手";
                case 331: return "追擊者";
                case 332: return "開拓者";
                case 334: return "開拓者(6轉)";
                case 400: return "盜賊";
                case 410: return "刺客";
                case 411: return "暗殺者";
                case 412: return "夜使者";
                case 414: return "夜使者(6轉)";
                case 420: return "侠客";
                case 421: return "神偷";
                case 422: return "暗影神偷";
                case 424: return "暗影神偷(6轉)";
                case 430: return "下忍";
                case 431: return "中忍";
                case 432: return "上忍";
                case 433: return "隱忍";
                case 434: return "影武者";
                case 436: return "影武者(6轉)";
                case 500: return "海盜";
                case 501: return "海盗(炮手)";
                case 508: return "蒼龍俠客(1轉)";
                case 510: return "打手";
                case 511: return "格鬥家";
                case 512: return "拳霸";
                case 514: return "拳霸(6轉)";
                case 520: return "槍手";
                case 521: return "神槍手";
                case 522: return "槍神";
                case 524: return "槍神(6轉)";
                case 530: return "重砲兵";
                case 531: return "重砲兵隊長";
                case 532: return "重砲指揮官";
                case 534: return "重砲指揮官(6轉)";
                case 570: return "蒼龍俠客(2轉)";
                case 571: return "蒼龍俠客(3轉)";
                case 572: return "蒼龍俠客(4轉)";

                case 800: 
                case 900: return "管理員";

                case 1000: return "初心者";
                case 1100: return "聖魂劍士(1轉)";
                case 1110: return "聖魂劍士(2轉)";
                case 1111: return "聖魂劍士(3轉)";
                case 1112: return "聖魂劍士(4轉)";
                case 1114: return "聖魂劍士((6轉)";
                case 1200: return "烈焰巫師(1轉)";
                case 1210: return "烈焰巫師(2轉)";
                case 1211: return "烈焰巫師(3轉)";
                case 1212: return "烈焰巫師(4轉)";
                case 1214: return "烈焰巫師(6轉)";
                case 1300: return "破風使者(1轉)";
                case 1310: return "破風使者(2轉)";
                case 1311: return "破風使者(3轉)";
                case 1312: return "破風使者(4轉)";
                case 1314: return "破風使者(6轉)";
                case 1400: return "暗夜行者(1轉)";
                case 1410: return "暗夜行者(2轉)";
                case 1411: return "暗夜行者(3轉)";
                case 1412: return "暗夜行者(4轉)";
                case 1414: return "暗夜行者(6轉)";
                case 1500: return "閃雷悍將(1轉)";
                case 1510: return "閃雷悍將(2轉)";
                case 1511: return "閃雷悍將(3轉)";
                case 1512: return "閃雷悍將(4轉)";
                case 1514: return "閃雷悍將(6轉)";


                case 2000: return "傳說";
                case 2001: return "小不點";
                case 2002: return "精靈遊俠";
                case 2003: return "幻影俠盜";
                case 2004: return "夜光";
                case 2005: return "隱月";
                case 2100: return "狂狼勇士(1轉)";
                case 2110: return "狂狼勇士(2轉)";
                case 2111: return "狂狼勇士(3轉)";
                case 2112: return "狂狼勇士(4轉)";
                case 2114: return "狂狼勇士(6轉)";
                case 2200:
                case 2210: return "龍魔導士(1轉)";
                case 2211:
                case 2212:
                case 2213: return "龍魔導士(2轉)";
                case 2214:
                case 2215:
                case 2216: return "龍魔導士(3轉)";
                case 2217:
                case 2218: return "龍魔導士(4轉)";
                case 2220: return "龍魔導士(6轉)";
                case 2300: return "精靈遊俠(1轉)";
                case 2310: return "精靈遊俠(2轉)";
                case 2311: return "精靈遊俠(3轉)";
                case 2312: return "精靈遊俠(4轉)";
                case 2314: return "精靈遊俠(6轉)";
                case 2400: return "幻影俠盜(1轉)";
                case 2410: return "幻影俠盜(2轉)";
                case 2411: return "幻影俠盜(3轉)";
                case 2412: return "幻影俠盜(4轉)";
                case 2414: return "幻影俠盜(6轉)";
                case 2500: return "隠月(1轉)";
                case 2510: return "隠月(2轉)";
                case 2511: return "隠月(3轉)";
                case 2512: return "隠月(4轉)";
                case 2514: return "隠月(6轉)";
                case 2700: return "夜光(1轉)";
                case 2710: return "夜光(2轉)";
                case 2711: return "夜光(3轉)";
                case 2712: return "夜光(4轉)";
                case 2714: return "夜光(6轉)";


                case 3000: return "市民";
                case 3001: return "惡魔";
                case 3100: return "惡魔殺手(1轉)";
                case 3110: return "惡魔殺手(2轉)";
                case 3111: return "惡魔殺手(3轉)";
                case 3112: return "惡魔殺手(4轉)";
                case 3114: return "惡魔殺手(6轉)";
                case 3101: return "惡魔復仇者(1轉)";
                case 3120: return "惡魔復仇者(2轉)";
                case 3121: return "惡魔復仇者(3轉)";
                case 3122: return "惡魔復仇者(4轉)";
                case 3124: return "惡魔復仇者(6轉)";
                case 3200: return "煉獄巫師(1轉)";
                case 3210: return "煉獄巫師(2轉)";
                case 3211: return "煉獄巫師(3轉)";
                case 3212: return "煉獄巫師(4轉)";
                case 3214: return "煉獄巫師(6轉)";
                case 3300: return "狂豹獵人(1轉)";
                case 3310: return "狂豹獵人(2轉)";
                case 3311: return "狂豹獵人(3轉)";
                case 3312: return "狂豹獵人(4轉)";
                case 3314: return "狂豹獵人(6轉)";
                case 3500: return "機甲戰神(1轉)";
                case 3510: return "機甲戰神(2轉)";
                case 3511: return "機甲戰神(3轉)";
                case 3512: return "機甲戰神(4轉)";
                case 3514: return "機甲戰神(6轉)";
                case 3002: return "傑諾";
                case 3600: return "傑諾(1轉)";
                case 3610: return "傑諾(2轉)";
                case 3611: return "傑諾(3轉)";
                case 3612: return "傑諾(4轉)";
                case 3614: return "傑諾(6轉)";
                case 3700: return "爆拳槍神(1轉)";
                case 3710: return "爆拳槍神(2轉)";
                case 3711: return "爆拳槍神(3轉)";
                case 3712: return "爆拳槍神(4轉)";
                case 3714: return "爆拳槍神(6轉)";

                case 4001: return "劍豪";
                case 4002: return "陰陽師";
                case 4100: return "劍豪(1轉)";
                case 4110: return "劍豪(2轉)";
                case 4111: return "劍豪(3轉)";
                case 4112: return "劍豪(4轉)";
                case 4114: return "劍豪(6轉)";
                case 4200: return "陰陽師(1轉)";
                case 4210: return "陰陽師(2轉)";
                case 4211: return "陰陽師(3轉)";
                case 4212: return "陰陽師(4轉)";
                case 4214: return "陰陽師(6轉)";


                case 5000: return "無名少年";
                case 5100: return "米哈逸(1轉)";
                case 5110: return "米哈逸(2轉)";
                case 5111: return "米哈逸(3轉)";
                case 5112: return "米哈逸(4轉)";
                case 5114: return "米哈逸(6轉)";


                case 6000: return "凱薩";
                case 6001: return "天使破壞者";
                case 6002: return "卡蒂娜";
                case 6003: return "凱殷";
                case 6100: return "凱薩(1轉)";
                case 6110: return "凱薩(2轉)";
                case 6111: return "凱薩(3轉)";
                case 6112: return "凱薩(4轉)";
                case 6114: return "凱薩(6轉)";
                case 6300: return "凱殷(1轉)";
                case 6310: return "凱殷(2轉)";
                case 6311: return "凱殷(3轉)";
                case 6312: return "凱殷(4轉)";
                case 6314: return "凱殷(6轉)";
                case 6400: return "卡蒂娜(1轉)";
                case 6410: return "卡蒂娜(2轉)";
                case 6411: return "卡蒂娜(3轉)";
                case 6412: return "卡蒂娜(4轉)";
                case 6414: return "卡蒂娜(6轉)";
                case 6500: return "天使破壞者(1轉)";
                case 6510: return "天使破壞者(2轉)";
                case 6511: return "天使破壞者(3轉)";
                case 6512: return "天使破壞者(4轉)";
                case 6514: return "天使破壞者(6轉)";

                case 7000: return "能力";
                case 7100: return "戰地技能";
                case 7200: return "怪物農場";


                case 9100: return "公會";
                case 9200:
                case 9201:
                case 9202:
                case 9203:
                case 9204: return "専業技術";

                case 10000: return "神之子";
                case 10100: return "神之子(1轉)";
                case 10110: return "神之子(2轉)";
                case 10111: return "神之子(3轉)";
                case 10112: return "神之子(4轉)";
                case 10114: return "神之子(6轉)";

                case 11000: return "幻獸師";
                case 11200: return "幻獸師(熊)";
                case 11210: return "幻獸師(豹)";
                case 11211: return "幻獸師(鷹)";
                case 11212: return "幻獸師(貓)";

                case 12000:
                case 12005:
                case 12100: return "竈門炭治郎";

                case 13000: return "粉豆";
                case 13001: return "雪吉拉";
                case 13100: return "粉豆";
                case 13500: return "雪吉拉";

                case 14000: return "凱內西斯";
                case 14200: return "凱內西斯(1轉)";
                case 14210: return "凱內西斯(2轉)";
                case 14211: return "凱內西斯(3轉)";
                case 14212: return "凱內西斯(4轉)";
                case 14213: return "凱內西斯(5轉)";
                case 14214: return "凱內西斯(6轉)";

                case 15000: return "伊利恩";
                case 15001: return "亞克";
                case 15002: return "阿戴爾";
                case 15003: return "卡莉";
                case 15100: return "阿戴爾(1轉)";
                case 15110: return "阿戴爾(2轉)";
                case 15111: return "阿戴爾(3轉)";
                case 15112: return "阿戴爾(4轉)";
                case 15114: return "阿戴爾(6轉)";
                case 15200: return "伊利恩(1轉)";
                case 15210: return "伊利恩(2轉)";
                case 15211: return "伊利恩(3轉)";
                case 15212: return "伊利恩(4轉)";
                case 15214: return "伊利恩(6轉)";
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

                case 16000: return "阿尼瑪盜賊";
                case 16001: return "菈菈";
                case 16002: return "Len";
                case 16100: return "Len(1次)";
                case 16110: return "Len(2次)";
                case 16111: return "Len(3次)";
                case 16112: return "Len(4次)";
                case 16114: return "Len(6次)";
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

                case 18000: return "施亞阿斯特";
                case 18001: return "葉里萊特";
                case 18100: return "葉里萊特(1轉)";
                case 18110: return "葉里萊特(2轉)";
                case 18111: return "葉里萊特(3轉)";
                case 18112: return "葉里萊特(4轉)";
                case 18114: return "葉里萊特(6轉)";
                case 18200: return "施亞阿斯特(1轉)";
                case 18210: return "施亞阿斯特(2轉)";
                case 18211: return "施亞阿斯特(3轉)";
                case 18212: return "施亞阿斯特(4轉)";
                case 18214: return "施亞阿斯特(6轉)";

                case 40000: return "5轉";
                case 40001: return "5轉(劍士)";
                case 40002: return "5轉(法師)";
                case 40003: return "5轉(弓箭手)";
                case 40004: return "5轉(盜賊)";
                case 40005: return "5轉(海盜)";


                case 50000: return "6轉";
                case 50006: return "6轉(強化核心)";
                case 50007: return "6轉(HEXA屬性)";
            }
            return null;
        }

        public static string ToCJKNumberExpr(long value)
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

        public static string GetMobSkillName(int id)
        {
            switch (id)
            {
                case 100: return "物理攻擊力增加";
                case 101: return "魔法攻擊力增加";
                case 102: return "物理防御力增加";
                case 103: return "魔法防御力增加";
                case 105: return "HP吸收";

                case 110: return "周辺物理攻擊力增加";
                case 111: return "周辺魔法攻擊力增加";
                case 112: return "周辺物理防御力增加";
                case 113: return "周辺魔法防御力增加";
                case 114: return "HP回復";
                case 115: return "移動速度增加";

                case 120: return "封印";
                case 121: return "打偏";
                case 122: return "虛弱";
                case 123: return "束縛";
                case 124: return "詛咒";
                case 125: return "中毒";
                case 126: return "減速";
                case 127: return "解除增益";
                case 128: return "誘惑";
                case 129: return "追放";

                case 131: return "範圍持續傷害";
                case 132: return "混乱";
                case 133: return "不死族";
                case 134: return "藥水封印";
                case 135: return "永不停止";
                case 136: return "暗闇";
                case 137: return "冷凍";
                case 138: return "潛能無效";

                case 140: return "物理攻擊無視";
                case 141: return "魔法攻擊無視";
                case 142: return "硬化技能";
                case 143: return "物理攻擊反射";
                case 144: return "魔法攻擊反射";
                case 145: return "攻擊反射";
                case 146: return "無敵";

                case 150: return "物理攻擊力增加";
                case 151: return "魔法攻擊力增加";
                case 152: return "物理防御力增加";
                case 153: return "魔法防御力增加";
                case 154: return "命中率增加";
                case 155: return "回避率增加";
                case 156: return "移動速度增加";

                case 170: return "傳送";
                case 171: return "爆發";
                case 172: return "變異";
                case 173: return "空降";
                case 174: return "石化";

                case 200: return "召喚";
                case 201: return "召喚";

                default: return null;
            }
        }
    }
}
