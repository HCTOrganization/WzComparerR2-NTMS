using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using WzComparerR2.WzLib;
using WzComparerR2.PluginBase;
using System.Linq;

namespace WzComparerR2.CharaSim
{
    public static class CharaSimLoader
    {
        static CharaSimLoader()
        {
            LoadedSetItems = new Dictionary<int, SetItem>();
            LoadedAstraSubWeapons = new Dictionary<int, AstraSubWeaponInfo>();
            LoadedExclusiveEquips = new Dictionary<int, ExclusiveEquip>();
            LoadedCommoditiesBySN = new Dictionary<int, Commodity>();
            LoadedCommoditiesByItemId = new Dictionary<int, Commodity>();
            LoadedCommodityPricesByItemId = new List<Dictionary<int, List<CommodityPriceInfo>>>();
            for (int i = 0; i < 2; i++) // 2 slots
                LoadedCommodityPricesByItemId.Add(new Dictionary<int, List<CommodityPriceInfo>>());
            LoadedMintableNFTItems = new List<int>();
            LoadedMintableSBTItems = new List<int>();
            LoadedMintableFTItems = new List<int>();
            LoadedGmsTucBlacklistItems = new List<int>();
            LoadedPetEquipInfo = new Dictionary<int, List<int>>();
        }

        public static Dictionary<int, SetItem> LoadedSetItems { get; private set; }
        public static Dictionary<int, AstraSubWeaponInfo> LoadedAstraSubWeapons { get; private set; }

        /// <summary>
        /// 以 <c>*.img</c> 名稱尋找節點。Unity 資料集的樹沒有 <c>.img</c> 副檔名，
        /// 找不到時再以不含副檔名的名稱尋找（img 會回傳展開後的內容）。
        /// </summary>
        private static Wz_Node FindImgNode(Wz_Node root, string imgName, string innerPath = null)
        {
            if (root == null || string.IsNullOrEmpty(imgName))
            {
                return null;
            }
            string name = imgName.EndsWith(".img", StringComparison.OrdinalIgnoreCase)
                ? imgName.Substring(0, imgName.Length - 4)
                : imgName;
            foreach (string candidate in new[]
            {
                innerPath == null ? imgName : imgName + "\\" + innerPath,
                innerPath == null ? name : name + "\\" + innerPath,
            })
            {
                Wz_Node node = root.FindNodeByPath(candidate, true);
                if (node != null)
                {
                    return node;
                }
            }
            return null;
        }

        public static Dictionary<int, ExclusiveEquip> LoadedExclusiveEquips { get; private set; }
        public static Dictionary<int, Commodity> LoadedCommoditiesBySN { get; private set; }
        public static Dictionary<int, Commodity> LoadedCommoditiesByItemId { get; private set; }
        public static List<Dictionary<int, List<CommodityPriceInfo>>> LoadedCommodityPricesByItemId { get; private set; }
        public static List<int> LoadedMintableNFTItems { get; private set; }
        public static List<int> LoadedMintableSBTItems { get; private set; }
        public static List<int> LoadedMintableFTItems { get; private set; }
        public static Dictionary<int, List<int>> LoadedPetEquipInfo { get; private set; }
        public static List<int> LoadedGmsTucBlacklistItems { get; private set; }

        public static void LoadSetItemsIfEmpty(Wz_File sourceWzFile = null)
        {
            if (LoadedSetItems.Count == 0)
            {
                LoadSetItems(sourceWzFile);
            }
        }

        public static void LoadSetItems(Wz_File sourceWzFile)
        {
            //搜索setItemInfo.img
            Wz_Node etcWz = PluginManager.FindWz(Wz_Type.Etc, sourceWzFile, true);
            if (etcWz == null)
                return;
            Wz_Node setItemNode = FindImgNode(etcWz, "SetItemInfo.img");
            if (setItemNode == null)
                return;

            //搜索ItemOption.img
            Wz_Node itemWz = PluginManager.FindWz(Wz_Type.Item, sourceWzFile, true);
            if (itemWz == null)
                return;
            Wz_Node optionNode = FindImgNode(itemWz, "ItemOption.img");
            if (optionNode == null)
                return;

            LoadedSetItems.Clear();
            foreach (Wz_Node node in setItemNode.Nodes)
            {
                int setItemIndex;
                if (Int32.TryParse(node.Text, out setItemIndex))
                {
                    SetItem setItem = SetItem.CreateFromNode(node, optionNode);
                    if (setItem != null)
                    {
                        ResolveSetItemName(setItem);
                        LoadedSetItems[setItemIndex] = setItem;
                    }
                }
            }
        }

        public static void LoadAstraSubWeaponsIfEmpty(Wz_File sourceWzFile = null)
        {
            if (LoadedAstraSubWeapons.Count == 0)
            {
                LoadAstraSubWeapons(sourceWzFile);
            }
        }

        public static void LoadAstraSubWeapons(Wz_File sourceWzFile)
        {
            //搜索setItemInfo.img
            Wz_Node etcWz = PluginManager.FindWz(Wz_Type.Etc, sourceWzFile);
            if (etcWz == null)
                return;
            Wz_Node astraNode = FindImgNode(etcWz, "SubWeaponTransferData.img", "Job");
            if (astraNode == null)
                return;

            LoadedAstraSubWeapons.Clear();
            List<int> idSet = new List<int>();
            Action<int> insert = (int jobID) =>
            {
                idSet.Sort();
                for (int i = 0; i < idSet.Count; i++)
                {
                    LoadedAstraSubWeapons[idSet[i]] = new AstraSubWeaponInfo(idSet[i], i, jobID);
                }
                idSet.Clear();
            };

            foreach (var job in astraNode.Nodes)
            {
                if (!int.TryParse(job.Text, out var jobID))
                    continue;

                var targetNode = job.FindNodeByPath("target");
                foreach (var target in targetNode?.Nodes ?? new Wz_Node.WzNodeCollection(null))
                {
                    if (int.TryParse(target.Text, out var id))
                    {
                        idSet.Add(id);
                    }
                    else
                    {
                        insert(jobID);
                        foreach (var inner_target in target?.Nodes ?? new Wz_Node.WzNodeCollection(null))
                        {
                            if (int.TryParse(inner_target.Text, out var inner_id))
                            {
                                idSet.Add(inner_id);
                            }
                        }
                        insert(jobID);
                    }
                }
                insert(jobID);
            }
        }

        public static void LoadMsnMintableItemListIfEmpty(Wz_File sourceWzFile = null)
        {
            if (LoadedMintableNFTItems.Count == 0 || LoadedMintableSBTItems.Count == 0 || LoadedMintableFTItems.Count == 0)
            {
                LoadMsnMintableItemList(sourceWzFile);
            }
        }

        public static void LoadMsnMintableItemList(Wz_File sourceWzFile)
        {
            Wz_Node itemWz = PluginManager.FindWz(Wz_Type.Item, sourceWzFile);
            if (itemWz == null)
                return;
            Wz_Node mintableListNode = FindImgNode(itemWz, "MintableList.img");
            if (mintableListNode == null)
                return;

            LoadedMintableNFTItems.Clear();
            LoadedMintableSBTItems.Clear();
            LoadedMintableFTItems.Clear();

            Wz_Node nftNode = mintableListNode.FindNodeByPath("NFT");
            if (nftNode != null)
            {
                foreach (var i in nftNode.Nodes)
                {
                    switch (i.Text)
                    {
                        case "SBT":
                            foreach (var j in i.Nodes)
                            {
                                if (int.TryParse(j.Text, out int sbtId))
                                    LoadedMintableSBTItems.Add(sbtId);
                            }
                            break;
                        default:
                            if (int.TryParse(i.Text, out int id))
                                LoadedMintableNFTItems.Add(id);
                            break;
                    }
                }
            }

            Wz_Node ftNode = mintableListNode.FindNodeByPath("FT");
            if (ftNode != null)
            {
                foreach (var i in ftNode.Nodes)
                {
                    if (int.TryParse(i.Text, out int id))
                        LoadedMintableFTItems.Add(id);
                }
            }
        }

        public static void LoadGmsTucBlacklistItemsIfEmpty(Wz_File sourceWzFile = null)
        {
            if (LoadedGmsTucBlacklistItems.Count == 0)
            {
                LoadGmsTucBlacklistItems(sourceWzFile);
            }
        }

        public static void LoadGmsTucBlacklistItems(Wz_File sourceWzFile)
        {
            Wz_Node etcWz = PluginManager.FindWz(Wz_Type.Etc, sourceWzFile);
            if (etcWz == null)
                return;
            Wz_Node gmsTucBlacklistNode = FindImgNode(etcWz, "TUC_Blacklist_GL.img", "ItemList");
            if (gmsTucBlacklistNode == null)
                return;
            foreach (var i in gmsTucBlacklistNode.Nodes)
            {
                if (Int32.TryParse(i.Value.ToString(), out int gearID))
                {
                    LoadedGmsTucBlacklistItems.Add(gearID);
                }
            }
        }

        public static void LoadPetEquipInfoIfEmpty(Wz_File sourceWzFile = null)
        {
            if (LoadedPetEquipInfo.Count == 0)
            {
                LoadPetEquipInfo(sourceWzFile);
            }
        }

        public static void LoadPetEquipInfo(Wz_File sourceWzFile)
        {
            Wz_Node characterWz = PluginManager.FindWz(Wz_Type.Character, sourceWzFile);
            if (characterWz == null)
                return;
            Wz_Node petEquipNode = characterWz.FindNodeByPath("PetEquip", true);
            if (petEquipNode == null)
                return;

            LoadedPetEquipInfo.Clear();
            foreach (var i in petEquipNode.Nodes)
            {
                Wz_Image image = i.GetValue<Wz_Image>();
                if (image == null || !image.TryExtract())
                {
                    continue;
                }
                else
                {
                    if (Int32.TryParse(i.Text.Replace(".img", ""), out int petEquipId))
                    {
                        List<int> applicablePets = new List<int>();
                        foreach (var j in image.Node.Nodes)
                        {
                            if (Int32.TryParse(j.Text, out int petID))
                            {
                                applicablePets.Add(petID);
                            }
                        }
                        LoadedPetEquipInfo[petEquipId] = applicablePets;
                    }
                }
            }
        }

        public static SetItem LoadSetItem(int setID, Wz_File sourceWzFile)
        {
            //搜索setItemInfo.img
            Wz_Node etcWz = PluginManager.FindWz(Wz_Type.Etc, sourceWzFile);
            if (etcWz == null)
                return null;
            Wz_Node setItemNode = FindImgNode(etcWz, "SetItemInfo.img");
            if (setItemNode == null)
                return null;

            //搜索ItemOption.img
            Wz_Node itemWz = PluginManager.FindWz(Wz_Type.Item, sourceWzFile);
            if (itemWz == null)
                return null;
            Wz_Node optionNode = FindImgNode(itemWz, "ItemOption.img");
            if (optionNode == null)
                return null;

            foreach (Wz_Node node in setItemNode.Nodes)
            {
                int setItemIndex;
                if (Int32.TryParse(node.Text, out setItemIndex) && setItemIndex == setID)
                {
                    SetItem setItem = SetItem.CreateFromNode(node, optionNode);
                    if (setItem != null)
                    {
                        ResolveSetItemName(setItem);
                        return setItem;
                    }
                    else
                        return null;
                }
            }
            return null;
        }

        /// <summary>
        /// 套裝名稱為腳本字串索引鍵（例如 <c>SID_WONDERPET_...</c>）時，從字串表解析。
        /// Classic（Unity）客戶端以索引鍵形式儲存套裝名稱。
        /// </summary>
        private static void ResolveSetItemName(SetItem setItem)
        {
            if (setItem == null)
            {
                return;
            }
            string resolved = ResolveScriptString(setItem.SetItemName);
            if (!string.IsNullOrEmpty(resolved))
            {
                setItem.SetItemName = resolved;
            }
        }

        public static void LoadExclusiveEquipsIfEmpty(Wz_File sourceWzFile = null)
        {
            if (LoadedExclusiveEquips.Count == 0)
            {
                LoadExclusiveEquips(sourceWzFile);
            }
        }

        public static void LoadExclusiveEquips(Wz_File sourceWzFile)
        {
            Wz_Node exclusiveNode = PluginManager.FindWz("Etc/ExclusiveEquip.img", sourceWzFile);
            if (exclusiveNode == null)
                return;

            LoadedExclusiveEquips.Clear();
            foreach (Wz_Node node in exclusiveNode.Nodes)
            {
                int exclusiveEquipIndex;
                if (Int32.TryParse(node.Text, out exclusiveEquipIndex))
                {
                    ExclusiveEquip exclusiveEquip = ExclusiveEquip.CreateFromNode(node);
                    if (exclusiveEquip != null)
                        LoadedExclusiveEquips[exclusiveEquipIndex] = exclusiveEquip;
                }
            }
        }

        public static void LoadCommoditiesIfEmpty(Wz_File sourceWzFile = null, int slotIdx = 0)
        {
            if (LoadedCommoditiesBySN.Count == 0 && LoadedCommoditiesByItemId.Count == 0)
            {
                LoadCommodities(sourceWzFile, slotIdx);
            }
        }

        public static void LoadCommodities(Wz_File sourceWzFile, int slotIdx = 0)
        {
            Wz_Node commodityNode = PluginManager.FindWz("Etc/Commodity.img", sourceWzFile);
            if (commodityNode == null)
                return;

            LoadedCommoditiesBySN.Clear();
            LoadedCommoditiesByItemId.Clear();
            LoadedCommodityPricesByItemId[slotIdx].Clear();
            foreach (Wz_Node node in commodityNode.Nodes)
            {
                int commodityIndex;
                if (Int32.TryParse(node.Text, out commodityIndex))
                {
                    Commodity commodity = Commodity.CreateFromNode(node);
                    if (commodity != null)
                    {
                        LoadedCommoditiesBySN[commodity.SN] = commodity;
                        if (commodity.ItemId / 10000 == 910)
                            LoadedCommoditiesByItemId[commodity.ItemId] = commodity;

                        // if (commodity.OnSale > 0)
                        {
                            if (!LoadedCommodityPricesByItemId[slotIdx].ContainsKey(commodity.ItemId))
                            {
                                LoadedCommodityPricesByItemId[slotIdx][commodity.ItemId] = new List<CommodityPriceInfo>();
                            }
                            if (commodity.Price > 1) LoadedCommodityPricesByItemId[slotIdx][commodity.ItemId].Add(commodity.PriceInfo);
                        }
                    }
                }
            }

            foreach (var kv in LoadedCommodityPricesByItemId[slotIdx])
            {
                kv.Value.Sort();
            }
        }

        public static void ClearAll()
        {
            LoadedSetItems.Clear();
            LoadedExclusiveEquips.Clear();
            LoadedCommoditiesBySN.Clear();
            LoadedCommoditiesByItemId.Clear();
            foreach (var dict in LoadedCommodityPricesByItemId)
            {
                dict.Clear();
            }
            LoadedMintableNFTItems.Clear();
            LoadedMintableSBTItems.Clear();
            LoadedMintableFTItems.Clear();
            LoadedPetEquipInfo.Clear();
            LoadedGmsTucBlacklistItems.Clear();
        }

        /// <summary>
        /// WZ 以外的動作延遲來源（例如 Unity 資產）；找不到資料時回傳 null。
        /// </summary>
        public static Func<string, int?> ActionDelayResolver { get; set; }

        /// <summary>
        /// 將客戶端腳本字串索引鍵（例如 <c>SID_EXCLUSIVE_EQUIP_ADMIN_INFO</c>）
        /// 從 <c>String/&lt;region&gt;/StringTable</c> 解析。非索引鍵格式或無法解析時回傳 null。
        /// </summary>
        public static string ResolveScriptString(string key)
        {
            if (!IsScriptStringKey(key))
            {
                return null;
            }
            Wz_Node node = PluginManager.FindWz("String/StringTable/" + key);
            string text = node == null ? null : node.Value as string;
            return string.IsNullOrEmpty(text) ? null : text;
        }

        /// <summary>是否為不含空白與符號的 <c>AAA_BBB</c> 格式索引鍵。</summary>
        private static bool IsScriptStringKey(string key)
        {
            if (string.IsNullOrEmpty(key) || key.Length > 128)
            {
                return false;
            }
            bool hasUnderscore = false;
            bool hasLetter = false;
            foreach (char c in key)
            {
                if (c == '_')
                {
                    hasUnderscore = true;
                }
                else if (c >= '0' && c <= '9')
                {
                }
                else if ((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z'))
                {
                    hasLetter = true;
                }
                else
                {
                    return false;
                }
            }
            return hasUnderscore && hasLetter;
        }

        public static int GetActionDelay(string actionName, Wz_Node wzNode = null)
        {
            if (string.IsNullOrEmpty(actionName))
            {
                return 0;
            }
            Wz_Node actionNode = wzNode == null ? PluginManager.FindWz("Character/00002000.img/" + actionName) :
                PluginManager.FindWz("Character/00002000.img/" + actionName, wzNode.GetNodeWzFile());
            if (actionNode == null)
            {
                Func<string, int?> resolver = ActionDelayResolver;
                if (resolver != null)
                {
                    int? resolved = resolver(actionName);
                    if (resolved.HasValue)
                    {
                        return resolved.Value;
                    }
                }
                return 0;
            }

            int delay = 0;
            foreach (Wz_Node frameNode in actionNode.Nodes)
            {
                Wz_Node delayNode = frameNode.Nodes["delay"];
                if (delayNode != null)
                {
                    delay += Math.Abs(delayNode.GetValue<int>());
                }
            }

            return delay;
        }
    }
}