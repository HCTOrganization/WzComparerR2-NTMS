using System;
using System.Collections.Generic;
using System.Globalization;
using WzComparerR2.UnityLib.Unity;
using WzComparerR2.UnityLib.Wzjs;
using WzComparerR2.WzLib;

namespace WzComparerR2.UnityLib.Classic
{
    /// <summary>
    /// 把 Unity 的 <c>String/&lt;region&gt;/&lt;category&gt;</c> 資產整理成傳統
    /// String.wz / Etc.wz / Quest.wz 的節點形狀，供
    /// <c>WzComparerR2.Common.StringLinker.Load(Wz_Node, Wz_Node, Wz_Node, Wz_Node)</c>
    /// 直接使用。
    /// <para>
    /// 傳統 WZ 的 <c>String.wz/Skill.img</c> 對應這裡的 <c>String/&lt;region&gt;/Skill</c>；
    /// Classic 把 Cash / Consume / Eqp / Etc / Ins / Pet 併成單一
    /// <c>String/&lt;region&gt;/Item</c>（id → name/desc），因此依 id 前綴再拆回
    /// Eqp / Consume / Ins / Etc / Pet / Cash 等 img。
    /// </para>
    /// </summary>
    public sealed class ClassicStringLinkerTables
    {
        // StringLinker 直接以「一層 id」解析的分類；Map / Eqp 需要額外的分組層，
        // GuildCastle / Roguelike / Redmoon 需要真正的 Wz_Image，這裡不提供。
        private static readonly string[] DirectStringCategories =
        {
            "Pet", "Cash", "Ins", "Consume", "Etc", "Familiar", "FamiliarSkill",
            "MonsterBook", "Mob", "Npc", "Skill",
        };

        private ClassicStringLinkerTables(
            Wz_Node stringNode, Wz_Node itemNode, Wz_Node etcNode, Wz_Node questNode)
        {
            this.StringNode = stringNode;
            this.ItemNode = itemNode;
            this.EtcNode = etcNode;
            this.QuestNode = questNode;
        }

        /// <summary>String.wz 對應節點（子節點名為 <c>Skill.img</c> 這類名稱）。</summary>
        public Wz_Node StringNode { get; private set; }

        /// <summary>Item.wz 對應節點；目前沒有需要補的內容，保留 null。</summary>
        public Wz_Node ItemNode { get; private set; }

        /// <summary>Etc.wz 對應節點（SetItemInfo、Achievement、worldArchive…）。</summary>
        public Wz_Node EtcNode { get; private set; }

        /// <summary>Quest.wz 對應節點（QuestData/&lt;id&gt;.img/QuestInfo）。</summary>
        public Wz_Node QuestNode { get; private set; }

        public bool HasValues
        {
            get
            {
                return (this.StringNode != null && this.StringNode.Nodes.Count > 0)
                    || (this.EtcNode != null && this.EtcNode.Nodes.Count > 0)
                    || (this.QuestNode != null && this.QuestNode.Nodes.Count > 0);
            }
        }

        public static ClassicStringLinkerTables Build(ClassicWzSource source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            return new ClassicStringLinkerTables(
                BuildStringNode(source),
                null,
                BuildEtcNode(source),
                BuildQuestNode(source));
        }

        private static Wz_Node BuildStringNode(ClassicWzSource source)
        {
            string locale = source.Locale;
            Wz_Node root = new Wz_Node("String");
            Dictionary<string, Wz_Node> categories =
                new Dictionary<string, Wz_Node>(StringComparer.Ordinal);
            Dictionary<int, string> equipTypes = BuildEquipTypeMap(source);

            string prefix = "String/" + locale + "/";
            foreach (KeyValuePair<string, UnityEntryLocation> entry in source.Catalog.Entries)
            {
                if (!entry.Key.StartsWith(prefix, StringComparison.Ordinal))
                {
                    continue;
                }
                string category = entry.Key.Substring(prefix.Length);
                if (category.Length == 0 || category.IndexOf('/') >= 0)
                {
                    continue;
                }
                if (string.Equals(category, "Item", StringComparison.Ordinal))
                {
                    continue; // 單一道具表，稍後依 id 範圍拆表
                }
                if (Array.IndexOf(DirectStringCategories, category) < 0)
                {
                    continue;
                }
                AddCategoryFromEntry(root, categories, source, entry.Key, category);
            }

            // 目前語系缺少的分類以 COMMON 補齊。
            if (!string.Equals(locale, "COMMON", StringComparison.OrdinalIgnoreCase))
            {
                string commonPrefix = "String/COMMON/";
                foreach (KeyValuePair<string, UnityEntryLocation> entry in source.Catalog.Entries)
                {
                    if (!entry.Key.StartsWith(commonPrefix, StringComparison.Ordinal))
                    {
                        continue;
                    }
                    string category = entry.Key.Substring(commonPrefix.Length);
                    if (category.Length == 0 || category.IndexOf('/') >= 0
                        || string.Equals(category, "Item", StringComparison.Ordinal)
                        || Array.IndexOf(DirectStringCategories, category) < 0
                        || categories.ContainsKey(category))
                    {
                        continue;
                    }
                    AddCategoryFromEntry(root, categories, source, entry.Key, category);
                }
            }

            BuildItemCategories(root, categories, source, prefix, equipTypes);
            BuildEqpImage(root, categories, source, prefix, equipTypes);
            BuildMapImage(root, categories, source, prefix);
            return root;
        }

        private static void AddCategoryFromEntry(
            Wz_Node root, Dictionary<string, Wz_Node> categories,
            ClassicWzSource source, string key, string category)
        {
            if (categories.ContainsKey(category))
            {
                return;
            }
            Wz_Node node = new Wz_Node(WithImgSuffix(category));
            if (TryAppendEntryContents(node, source, key))
            {
                root.Nodes.Add(node);
            }
            categories[category] = node;
        }

        /// <summary>
        /// 單一資產解析失敗時不讓整份字串表消失，攔截例外後繼續。
        /// </summary>
        private static bool TryAppendEntryContents(Wz_Node target, ClassicWzSource source, string key)
        {
            try
            {
                ClassicWzBridge.AppendEntryContents(target, source, key);
                return true;
            }
            catch (Exception ex) when (ex is UnityException || ex is ClassicAssetException || ex is WzjsException)
            {
                target.Nodes.Clear();
                return false;
            }
        }

        /// <summary>
        /// <c>String/&lt;region&gt;/Item</c> 是 Cash / Consume / Eqp / Etc / Ins / Pet
        /// 合併的單一 id 表；依 id 前綴還原成各 <c>*.img</c>。
        /// </summary>
        private static void BuildItemCategories(
            Wz_Node root, Dictionary<string, Wz_Node> categories,
            ClassicWzSource source, string prefix, Dictionary<int, string> equipTypes)
        {
            string itemKey = prefix + "Item";
            if (!source.Catalog.Entries.ContainsKey(itemKey))
            {
                return;
            }

            Wz_Node table = new Wz_Node("Item");
            TryAppendEntryContents(table, source, itemKey);

            List<Wz_Node> itemNodes = new List<Wz_Node>(table.Nodes);
            foreach (Wz_Node itemNode in itemNodes)
            {
                int id;
                if (!int.TryParse(itemNode.Text, NumberStyles.None,
                    CultureInfo.InvariantCulture, out id))
                {
                    continue;
                }

                string category = GetItemCategory(id);
                if (category == null)
                {
                    continue;
                }
                Wz_Node imgNode = GetOrCreateCategory(root, categories, category);
                if (string.Equals(category, "Eqp", StringComparison.Ordinal))
                {
                    // Eqp.img 需要「分類 / 子分類 / id」三層。
                    AddEqpEntry(imgNode, itemNode, id, equipTypes);
                }
                else if (string.Equals(category, "Etc", StringComparison.Ordinal))
                {
                    // Etc.img 需要「Etc / id」兩層。
                    GetOrAddChild(imgNode, "Etc").Nodes.Add(itemNode);
                }
                else
                {
                    imgNode.Nodes.Add(itemNode);
                }
            }
        }

        /// <summary>
        /// 依道具 id 前綴判斷它在傳統 String.wz 屬於哪個 img。
        /// 0xxxxxx~1xxxxxx = Eqp、2xxxxxx = Consume、3xxxxxx = Ins、4xxxxxx = Etc、
        /// 500xxxx = Pet、501xxxx 以上 = Cash。
        /// </summary>
        internal static string GetItemCategory(int id)
        {
            if (id < 0)
            {
                return null;
            }
            if (id < 2000000)
            {
                return "Eqp";
            }
            if (id < 3000000)
            {
                return "Consume";
            }
            if (id < 4000000)
            {
                return "Ins";
            }
            if (id < 5000000)
            {
                return "Etc";
            }
            if (id < 5010000)
            {
                return "Pet";
            }
            return "Cash";
        }

        /// <summary>
        /// <c>String/&lt;region&gt;/Eqp</c>（若存在）也是單一 id 表；
        /// StringLinker 的 Eqp.img 需要「分類 / 子分類 / id」三層。
        /// </summary>
        private static void BuildEqpImage(
            Wz_Node root, Dictionary<string, Wz_Node> categories,
            ClassicWzSource source, string prefix, Dictionary<int, string> equipTypes)
        {
            string eqpKey = prefix + "Eqp";
            if (!source.Catalog.Entries.ContainsKey(eqpKey))
            {
                return;
            }

            Wz_Node table = new Wz_Node("Eqp");
            TryAppendEntryContents(table, source, eqpKey);
            Wz_Node eqpImg = GetOrCreateCategory(root, categories, "Eqp");
            foreach (Wz_Node node in new List<Wz_Node>(table.Nodes))
            {
                int id;
                if (int.TryParse(node.Text, NumberStyles.None, CultureInfo.InvariantCulture, out id))
                {
                    AddEqpEntry(eqpImg, node, id, equipTypes);
                }
            }
        }

        /// <summary>
        /// <c>String/&lt;region&gt;/Map</c> 是單一 id 表；
        /// StringLinker 的 Map.img 需要「分組 / id」兩層。
        /// </summary>
        private static void BuildMapImage(
            Wz_Node root, Dictionary<string, Wz_Node> categories,
            ClassicWzSource source, string prefix)
        {
            string mapKey = prefix + "Map";
            if (!source.Catalog.Entries.ContainsKey(mapKey))
            {
                return;
            }

            Wz_Node table = new Wz_Node("Map");
            TryAppendEntryContents(table, source, mapKey);
            Wz_Node mapImg = GetOrCreateCategory(root, categories, "Map");
            foreach (Wz_Node node in new List<Wz_Node>(table.Nodes))
            {
                int id;
                if (!int.TryParse(node.Text, NumberStyles.None, CultureInfo.InvariantCulture, out id))
                {
                    continue;
                }
                string groupName = (id / 10000000).ToString(CultureInfo.InvariantCulture);
                GetOrAddChild(mapImg, groupName).Nodes.Add(node);
            }
        }

        private static void AddEqpEntry(
            Wz_Node eqpImg, Wz_Node entry, int id, Dictionary<int, string> equipTypes)
        {
            // StringLinker 需要「分類 / 子分類 / id」三層，因此也保留原本的形狀。
            Wz_Node group = GetOrAddChild(
                eqpImg, (id / 10000).ToString(CultureInfo.InvariantCulture));
            Wz_Node slot = GetOrAddChild(group, "0");
            slot.Nodes.Add(entry);

            // 同時提供與 WZ 相同的「Eqp.img/Eqp/<分類名>/<id>」形狀（供 DB2 等使用）。
            string typeName;
            if (equipTypes == null || !equipTypes.TryGetValue(id, out typeName))
            {
                return;
            }
            Wz_Node eqpRoot = GetOrAddChild(eqpImg, "Eqp");
            GetOrAddChild(eqpRoot, typeName).Nodes.Add(CloneNode(entry));
            if (string.Equals(typeName, "TamingMob", StringComparison.Ordinal))
            {
                // WZ 端的分類名是 "Taming"。
                GetOrAddChild(eqpRoot, "Taming").Nodes.Add(CloneNode(entry));
            }
        }

        private static Wz_Node CloneNode(Wz_Node node)
        {
            Wz_Node copy = new Wz_Node(node.Text);
            copy.Value = node.Value;
            foreach (Wz_Node child in node.Nodes)
            {
                copy.Nodes.Add(CloneNode(child));
            }
            return copy;
        }

        /// <summary>
        /// 將 <c>Etc/&lt;region&gt;/*</c> 對映成 Etc.wz 的節點形狀。葉節點會加上 <c>.img</c>
        /// （如 <c>Etc/SetItemInfo.img</c>、<c>Etc/CashPackage.img</c>、<c>Etc/Achievement/AchievementData/&lt;id&gt;.img</c>）。
        /// </summary>
        private static Wz_Node BuildEtcNode(ClassicWzSource source)
        {
            string prefix = "Etc/" + source.Locale + "/";
            Wz_Node root = new Wz_Node("Etc");
            foreach (KeyValuePair<string, UnityEntryLocation> entry in source.Catalog.Entries)
            {
                if (!entry.Key.StartsWith(prefix, StringComparison.Ordinal))
                {
                    continue;
                }
                string rest = entry.Key.Substring(prefix.Length);
                if (rest.Length == 0)
                {
                    continue;
                }

                string[] segments = rest.Split('/');
                Wz_Node current = root;
                for (int i = 0; i < segments.Length; i++)
                {
                    bool isLeaf = i == segments.Length - 1;
                    string name = isLeaf ? WithImgSuffix(segments[i]) : segments[i];
                    Wz_Node next = GetOrAddChild(current, name);
                    if (isLeaf)
                    {
                        TryAppendEntryContents(next, source, entry.Key);
                    }
                    current = next;
                }
            }
            return root;
        }

        /// <summary>
        /// 從 <c>Character/&lt;type&gt;/&lt;id&gt;</c>（例如 <c>Character/Cap/01002000</c>）
        /// 建立裝備 id → 分類名的對應，用於組合 WZ 的 <c>String.wz/Eqp.img/Eqp/Cap/…</c>。
        /// </summary>
        private static Dictionary<int, string> BuildEquipTypeMap(ClassicWzSource source)
        {
            Dictionary<int, string> types = new Dictionary<int, string>();
            foreach (string key in source.Catalog.Entries.Keys)
            {
                if (!key.StartsWith("Character/", StringComparison.Ordinal))
                {
                    continue;
                }
                string[] parts = key.Split('/');
                if (parts.Length != 3)
                {
                    continue;
                }
                int id;
                if (!int.TryParse(parts[2], NumberStyles.None, CultureInfo.InvariantCulture, out id))
                {
                    continue;
                }
                types[id] = parts[1];
            }
            return types;
        }

        /// <summary>
        /// 由 <c>String/&lt;region&gt;/QuestData</c> 組出 Quest.wz 的
        /// <c>QuestData/&lt;id&gt;.img/QuestInfo</c> 形狀。
        /// </summary>
        private static Wz_Node BuildQuestNode(ClassicWzSource source)
        {
            string key = "String/" + source.Locale + "/QuestData";
            if (!source.Catalog.Entries.ContainsKey(key))
            {
                key = "String/COMMON/QuestData";
                if (!source.Catalog.Entries.ContainsKey(key))
                {
                    return null;
                }
            }

            Wz_Node table = new Wz_Node("QuestData");
            TryAppendEntryContents(table, source, key);

            Wz_Node root = new Wz_Node("Quest");
            Wz_Node dataNode = root.Nodes.Add("QuestData");
            List<Wz_Node> quests = new List<Wz_Node>(table.Nodes);
            foreach (Wz_Node quest in quests)
            {
                Wz_Node questImg = dataNode.Nodes.Add(WithImgSuffix(quest.Text));
                Wz_Node infoNode = questImg.Nodes.Add("QuestInfo");
                List<Wz_Node> children = new List<Wz_Node>(quest.Nodes);
                foreach (Wz_Node child in children)
                {
                    infoNode.Nodes.Add(child);
                }
            }
            return root;
        }

        private static Wz_Node GetOrCreateCategory(
            Wz_Node root, Dictionary<string, Wz_Node> categories, string category)
        {
            Wz_Node node;
            if (categories.TryGetValue(category, out node))
            {
                return node;
            }
            node = GetOrAddChild(root, WithImgSuffix(category));
            categories[category] = node;
            return node;
        }

        private static Wz_Node GetOrAddChild(Wz_Node parent, string name)
        {
            Wz_Node existing = parent.Nodes[name];
            if (existing != null)
            {
                return existing;
            }
            return parent.Nodes.Add(name);
        }

        private static string WithImgSuffix(string name)
        {
            return name.EndsWith(".img", StringComparison.OrdinalIgnoreCase) ? name : name + ".img";
        }
    }
}
