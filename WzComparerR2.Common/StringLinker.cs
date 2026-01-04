using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using WzComparerR2.WzLib;

namespace WzComparerR2.Common
{
    public class StringLinker
    {
        public StringLinker()
        {
            stringEqp = new Dictionary<int, StringResult>();
            stringItem = new Dictionary<int, StringResult>();
            stringMap = new Dictionary<int, StringResult>();
            stringMob = new Dictionary<int, StringResult>();
            stringNpc = new Dictionary<int, StringResult>();
            stringFamiliarSkill = new Dictionary<int, StringResult>();
            stringGuildCastleGuildResearch = new Dictionary<int, StringResult>();
            stringGuildCastlePersonalResearch = new Dictionary<int, StringResult>();
            stringGuildCastleResearchTooltip = new Dictionary<string, StringResult>();
            stringRoguelikeSkill = new Dictionary<int, StringResult>();
            stringSkill = new Dictionary<int, StringResult>();
            stringSkill2 = new Dictionary<string, StringResult>();
            stringSetItem = new Dictionary<int, StringResult>();
            stringQuest = new Dictionary<int, StringResult>();
            stringAchievement = new Dictionary<int, StringResult>();
            stringWorldArchiveMob = new Dictionary<int, StringResult>();
            stringWorldArchiveNpc = new Dictionary<int, StringResult>();
            stringMonsterBook = new Dictionary<int, StringResult>();
        }

        public bool Update(Wz_Node stringNode, Wz_Node itemNode, Wz_Node etcNode, Wz_Node questNode)
        {
            if (stringNode == null || itemNode == null || etcNode == null)
                return false;

            return Load(stringNode, itemNode, etcNode, questNode, update: true);
        }

        public bool Load(Wz_File stringWz, Wz_File itemWz, Wz_File etcWz)
        {
            return Load(stringWz, itemWz, etcWz, null);
        }

        public bool Load(Wz_File stringWz, Wz_File itemWz, Wz_File etcWz, Wz_File questWz)
        {
            if (stringWz == null || stringWz.Node == null ||
                itemWz == null || itemWz.Node == null ||
                etcWz == null || etcWz.Node == null)
                return false;
            this.Clear();

            return Load(stringWz.Node, itemWz.Node, etcWz.Node, questWz.Node);
        }

        public bool Load(Wz_Node stringNode, Wz_Node itemNode, Wz_Node etcNode, Wz_Node questNode, bool update = false)
        {
            int id;
            foreach (Wz_Node node in stringNode.Nodes ?? new Wz_Node.WzNodeCollection(null))
            {
                Wz_Image image = node.Value as Wz_Image;
                if (image == null)
                    continue;
                switch (node.Text)
                {
                    case "Pet.img":
                    case "Cash.img":
                    case "Ins.img":
                    case "Consume.img":
                        if (!image.TryExtract()) break;
                        foreach (Wz_Node tree in image.Node.Nodes)
                        {
                            Wz_Node test_tree = TryLocateUolNode(tree);
                            if (Int32.TryParse(tree.Text, out id) && tree.ResolveUol() is Wz_Node linkNode)
                            {
                                StringResult strResult = null;
                                if (update)
                                {
                                    try { strResult = stringItem[id]; }
                                    catch { }
                                }
                                if (strResult == null) strResult = new StringResult();

                                strResult.Name = GetDefaultString(linkNode, "name") ?? strResult.Name ?? string.Empty;
                                strResult.Desc = GetDefaultString(linkNode, "desc") ?? strResult.Desc;
                                strResult.AutoDesc = GetDefaultString(linkNode, "autodesc") ?? strResult.AutoDesc;
                                if (tree.FullPath == test_tree.FullPath)
                                {
                                    if (tree.FullPath == test_tree.FullPath)
                                    {
                                        strResult.FullPath = tree.FullPath;
                                    }
                                    else
                                    {
                                        strResult.FullPath = tree.FullPath + " -> " + test_tree.FullPath;
                                    }
                                }
                                else
                                {
                                    strResult.FullPath = tree.FullPath + " -> " + test_tree.FullPath;
                                }

                                AddAllValue(strResult, linkNode);
                                stringItem[id] = strResult;
                            }
                        }
                        break;
                    case "Etc.img":
                        if (!image.TryExtract()) break;
                        foreach (Wz_Node tree0 in image.Node.Nodes)
                        {
                            foreach (Wz_Node tree in tree0.Nodes)
                            {
                                Wz_Node test_tree = TryLocateUolNode(tree);
                                if (Int32.TryParse(tree.Text, out id) && tree.ResolveUol() is Wz_Node linkNode)
                                {
                                    StringResult strResult = null;
                                    if (update)
                                    {
                                        try { strResult = stringItem[id]; }
                                        catch { }
                                    }
                                    if (strResult == null) strResult = new StringResult();

                                    strResult.Name = GetDefaultString(linkNode, "name") ?? strResult.Name ?? string.Empty;
                                    strResult.Desc = GetDefaultString(linkNode, "desc") ?? strResult.Desc;
                                    if (tree.FullPath == test_tree.FullPath)
                                    {
                                        if (tree.FullPath == test_tree.FullPath)
                                        {
                                            strResult.FullPath = tree.FullPath;
                                        }
                                        else
                                        {
                                            strResult.FullPath = tree.FullPath + " -> " + test_tree.FullPath;
                                        }
                                    }
                                    else
                                    {
                                        strResult.FullPath = tree.FullPath + " -> " + test_tree.FullPath;
                                    }

                                    AddAllValue(strResult, linkNode);
                                    stringItem[id] = strResult;
                                }
                            }
                        }
                        break;
                    case "Familiar.img":
                    case "FamiliarSkill.img":
                        if (!image.TryExtract()) break;
                        foreach (Wz_Node tree0 in image.Node.Nodes)
                        {
                            if (tree0.Text == "skill")
                            {
                                foreach (Wz_Node tree1 in tree0.Nodes)
                                {
                                    Wz_Node test_tree = TryLocateUolNode(tree1);
                                    if (Int32.TryParse(tree1.Text, out id) && tree1.ResolveUol() is Wz_Node linkNode)
                                    {
                                        StringResult strResult = null;
                                        if (update)
                                        {
                                            try { strResult = stringFamiliarSkill[id]; }
                                            catch { }
                                        }
                                        if (strResult == null) strResult = new StringResult();

                                        strResult.Name = GetDefaultString(linkNode, "name") ?? strResult.Name ?? string.Empty;
                                        strResult.Desc = GetDefaultString(linkNode, "desc") ?? strResult.Desc;
                                        if (tree1.FullPath == test_tree.FullPath)
                                        {
                                            if (tree1.FullPath == test_tree.FullPath)
                                            {
                                                strResult.FullPath = tree1.FullPath;
                                            }
                                            else
                                            {
                                                strResult.FullPath = tree1.FullPath + " -> " + test_tree.FullPath;
                                            }
                                        }
                                        else
                                        {
                                            strResult.FullPath = tree1.FullPath + " -> " + test_tree.FullPath;
                                        }

                                        AddAllValue(strResult, linkNode);
                                        stringFamiliarSkill[id] = strResult;
                                    }
                                }
                            }
                        }
                        break;
                    case "GuildCastle.img":
                        if (!image.TryExtract()) break;
                        Wz_Node researchNode = image.Node.FindNodeByPath("Research");
                        foreach (Wz_Node tree in researchNode.Nodes)
                        {
                            switch (tree.Text)
                            {
                                case "Guild":
                                    foreach (Wz_Node guildNode in tree.Nodes)
                                    {
                                        Wz_Node test_tree = TryLocateUolNode(guildNode);
                                        if (Int32.TryParse(guildNode.Text, out id) && guildNode.ResolveUol() is Wz_Node linkNode)
                                        {
                                            StringResultSkill strResult = new StringResultSkill();

                                            strResult.Name = GetDefaultString(linkNode, "name") ?? strResult.Name ?? string.Empty;
                                            strResult.Desc = GetDefaultString(linkNode, "desc") ?? strResult.Desc;
                                            var h = GetDefaultString(linkNode, "h");
                                            if (update && h != null)
                                            {
                                                strResult.SkillH.Clear();
                                            }
                                            strResult.SkillH.Add(h);

                                            if (guildNode.FullPath == test_tree.FullPath)
                                            {
                                                if (guildNode.FullPath == test_tree.FullPath)
                                                {
                                                    strResult.FullPath = guildNode.FullPath;
                                                }
                                                else
                                                {
                                                    strResult.FullPath = guildNode.FullPath + " -> " + test_tree.FullPath;
                                                }
                                            }
                                            else
                                            {
                                                strResult.FullPath = guildNode.FullPath + " -> " + test_tree.FullPath;
                                            }
                                            AddAllValue(strResult, linkNode);
                                            stringGuildCastleGuildResearch[id] = strResult;
                                        }
                                    }
                                    break;
                                case "Personal":
                                    foreach (Wz_Node personalNode in tree.Nodes)
                                    {
                                        Wz_Node test_tree = TryLocateUolNode(personalNode);
                                        if (Int32.TryParse(personalNode.Text, out id) && personalNode.ResolveUol() is Wz_Node linkNode)
                                        {
                                            StringResultSkill strResult = new StringResultSkill();

                                            strResult.Name = GetDefaultString(linkNode, "name") ?? strResult.Name ?? string.Empty;
                                            strResult.Desc = GetDefaultString(linkNode, "desc") ?? strResult.Desc;
                                            var h = GetDefaultString(linkNode, "h");
                                            if (update && h != null)
                                            {
                                                strResult.SkillH.Clear();
                                            }
                                            strResult.SkillH.Add(h);

                                            if (personalNode.FullPath == test_tree.FullPath)
                                            {
                                                if (personalNode.FullPath == test_tree.FullPath)
                                                {
                                                    strResult.FullPath = personalNode.FullPath;
                                                }
                                                else
                                                {
                                                    strResult.FullPath = personalNode.FullPath + " -> " + test_tree.FullPath;
                                                }
                                            }
                                            else
                                            {
                                                strResult.FullPath = personalNode.FullPath + " -> " + test_tree.FullPath;
                                            }
                                            AddAllValue(strResult, linkNode);
                                            stringGuildCastlePersonalResearch[id] = strResult;
                                        }
                                    }
                                    break;
                                case "Tooltip":
                                    foreach (Wz_Node tooltipNode in tree.Nodes)
                                    {
                                        StringResult sr = new StringResult();
                                        int counter = 0;
                                        sr.Desc = Regex.Replace(tooltipNode.Value.ToString(), @"\{\}", m =>
                                        {
                                            counter++;
                                            return counter == 2 ? "#level" : m.Value;
                                        }).Replace("{}", $"#{tooltipNode.Text}");
                                        stringGuildCastleResearchTooltip[tooltipNode.Text] = sr;
                                    }
                                    break;
                            }
                        }
                        break;
                    case "Mob.img":
                        if (!image.TryExtract()) break;
                        foreach (Wz_Node tree in image.Node.Nodes)
                        {
                            Wz_Node test_tree = TryLocateUolNode(tree);
                            if (Int32.TryParse(tree.Text, out id) && tree.ResolveUol() is Wz_Node linkNode)
                            {
                                StringResult strResult = null;
                                if (update)
                                {
                                    try { strResult = stringMob[id]; }
                                    catch { }
                                }
                                if (strResult == null) strResult = new StringResult();

                                strResult.Name = GetDefaultString(linkNode, "name") ?? strResult.Name ?? string.Empty;
                                if (tree.FullPath == test_tree.FullPath)
                                {
                                    strResult.FullPath = tree.FullPath;
                                }
                                else
                                {
                                    strResult.FullPath = tree.FullPath + " -> " + test_tree.FullPath;
                                }

                                AddAllValue(strResult, linkNode);
                                stringMob[id] = strResult;
                            }
                        }
                        break;
                    case "MonsterBook.img":
                        if (!image.TryExtract()) break;
                        foreach (Wz_Node tree in image.Node.Nodes)
                        {
                            if (Int32.TryParse(tree.Text, out id))
                            {
                                if (stringMob.ContainsKey(id))
                                {
                                    Wz_Node messageNode = tree.FindNodeByPath("episode");
                                    if (messageNode != null)
                                    {
                                        StringResult mbSr = new StringResult();
                                        mbSr.Name = stringMob[id].Name;
                                        mbSr.Desc = messageNode.Value.ToString();
                                        stringMonsterBook.Add(id, mbSr);
                                    }
                                }
                            }
                        }
                        break;
                    case "Npc.img":
                        if (!image.TryExtract()) break;
                        foreach (Wz_Node tree in image.Node.Nodes)
                        {
                            Wz_Node test_tree = TryLocateUolNode(tree);
                            if (Int32.TryParse(tree.Text, out id) && tree.ResolveUol() is Wz_Node linkNode)
                            {
                                StringResult strResult = null;
                                if (update)
                                {
                                    try { strResult = stringNpc[id]; }
                                    catch { }
                                }
                                if (strResult == null) strResult = new StringResult();

                                strResult.Name = GetDefaultString(linkNode, "name") ?? strResult.Name ?? string.Empty;
                                strResult.Desc = GetDefaultString(linkNode, "func") ?? strResult.Desc;
                                if (tree.FullPath == test_tree.FullPath)
                                {
                                    strResult.FullPath = tree.FullPath;
                                }
                                else
                                {
                                    strResult.FullPath = tree.FullPath + " -> " + test_tree.FullPath;
                                }

                                AddAllValue(strResult, linkNode);
                                stringNpc[id] = strResult;
                            }
                        }
                        break;
                    case "Map.img":
                        if (!image.TryExtract()) break;
                        foreach (Wz_Node tree0 in image.Node.Nodes)
                        {
                            foreach (Wz_Node tree in tree0.Nodes)
                            {
                                Wz_Node test_tree = TryLocateUolNode(tree);
                                if (Int32.TryParse(tree.Text, out id) && tree.ResolveUol() is Wz_Node linkNode)
                                {
                                    StringResult strResult = null;
                                    if (update)
                                    {
                                        try { strResult = stringMap[id]; }
                                        catch { }
                                    }
                                    if (strResult == null) strResult = new StringResult();

                                    var streetName = GetDefaultString(linkNode, "streetName");
                                    var mapName = GetDefaultString(linkNode, "mapName");
                                    strResult.Name = string.Format("{0} : {1}",
                                        streetName,
                                        mapName) ?? strResult.Name;
                                    strResult.StreetName = streetName ?? strResult.StreetName ?? string.Empty;
                                    strResult.MapName = mapName ?? strResult.MapName ?? string.Empty;
                                    strResult.Desc = GetDefaultString(linkNode, "mapDesc") ?? strResult.Desc;
                                    if (tree.FullPath == test_tree.FullPath)
                                    {
                                        strResult.FullPath = tree.FullPath;
                                    }
                                    else
                                    {
                                        strResult.FullPath = tree.FullPath + " -> " + test_tree.FullPath;
                                    }

                                    AddAllValue(strResult, linkNode);
                                    stringMap[id] = strResult;
                                }
                            }
                        }
                        break;
                    case "Roguelike.img":
                    case "Redmoon.img":
                        if (!image.TryExtract()) break;
                        foreach (Wz_Node tree in image.Node.Nodes)
                        {
                            switch (tree.Text)
                            {
                                case "artifact":
                                    foreach (Wz_Node artifactNode in tree.Nodes)
                                    {
                                        Wz_Node test_tree = TryLocateUolNode(artifactNode);
                                        if (Int32.TryParse(artifactNode.Text, out id) && artifactNode.ResolveUol() is Wz_Node linkNode)
                                        {
                                            StringResult strResult = null;
                                            if (update)
                                            {
                                                try { strResult = stringItem[id]; }
                                                catch { }
                                            }
                                            if (strResult == null) strResult = new StringResult();

                                            strResult.Name = GetDefaultString(linkNode, "name") ?? strResult.Name ?? string.Empty;
                                            strResult.Desc = GetDefaultString(linkNode, "desc") ?? strResult.Desc;
                                            if (artifactNode.FullPath == test_tree.FullPath)
                                            {
                                                if (artifactNode.FullPath == test_tree.FullPath)
                                                {
                                                    strResult.FullPath = artifactNode.FullPath;
                                                }
                                                else
                                                {
                                                    strResult.FullPath = artifactNode.FullPath + " -> " + test_tree.FullPath;
                                                }
                                            }
                                            else
                                            {
                                                strResult.FullPath = artifactNode.FullPath + " -> " + test_tree.FullPath;
                                            }

                                            AddAllValue(strResult, linkNode);
                                            stringItem[id] = strResult;
                                        }
                                    }
                                    break;
                                case "skill":
                                    foreach (Wz_Node skillNode in tree.Nodes)
                                    {
                                        Wz_Node test_tree = TryLocateUolNode(skillNode);
                                        if (Int32.TryParse(skillNode.Text, out id) && skillNode.ResolveUol() is Wz_Node linkNode)
                                        {
                                            StringResultSkill strResult = new StringResultSkill();

                                            strResult.Name = GetDefaultString(linkNode, "name") ?? strResult.Name ?? string.Empty;
                                            strResult.Desc = GetDefaultString(linkNode, "desc") ?? strResult.Desc;
                                            var h = GetDefaultString(linkNode, "h");
                                            if (update && h != null)
                                            {
                                                strResult.SkillH.Clear();
                                            }
                                            strResult.SkillH.Add(h);

                                            if (skillNode.FullPath == test_tree.FullPath)
                                            {
                                                if (skillNode.FullPath == test_tree.FullPath)
                                                {
                                                    strResult.FullPath = skillNode.FullPath;
                                                }
                                                else
                                                {
                                                    strResult.FullPath = skillNode.FullPath + " -> " + test_tree.FullPath;
                                                }
                                            }
                                            else
                                            {
                                                strResult.FullPath = skillNode.FullPath + " -> " + test_tree.FullPath;
                                            }
                                            AddAllValue(strResult, linkNode);
                                            stringRoguelikeSkill[id] = strResult;
                                        }
                                    }
                                    break;
                            }
                        }
                        break;
                    case "Skill.img":
                        if (!image.TryExtract()) break;
                        foreach (Wz_Node tree in image.Node.Nodes)
                        {
                            Wz_Node test_tree = TryLocateUolNode(tree);
                            if (tree.ResolveUol() is not Wz_Node linkNode)
                            {
                                continue;
                            }
                            StringResultSkill strResult = new StringResultSkill();

                            strResult.Name = GetDefaultString(linkNode, "name") ?? strResult.Name ?? string.Empty;//?? GetDefaultString(tree, "bookName");
                            strResult.Desc = GetDefaultString(linkNode, "desc") ?? strResult.Desc;
                            strResult.Pdesc = GetDefaultString(linkNode, "pdesc") ?? strResult.Pdesc;

                            var h = GetDefaultString(linkNode, "h");
                            if (update && h != null)
                            {
                                strResult.SkillH.Clear();
                            }
                            strResult.SkillH.Add(h);

                            h = GetDefaultString(linkNode, "ph");
                            if (update && h != null)
                            {
                                strResult.SkillpH.Clear();
                                strResult.SkillpH.Add(h);
                            }
                            else if (!update) strResult.SkillpH.Add(h);

                            h = GetDefaultString(linkNode, "hch");
                            if (update && h != null)
                            {
                                strResult.SkillhcH.Clear();
                                strResult.SkillhcH.Add(h);
                            }
                            else if (!update) strResult.SkillhcH.Add(h);

                            if (strResult.SkillH.Count > 0 && strResult.SkillH.Last() == null)
                            {
                                strResult.SkillH.RemoveAt(strResult.SkillH.Count - 1);
                                bool cleared = false;

                                for (int i = 1; ; i++)
                                {
                                    string hi = GetDefaultString(linkNode, "h" + i);
                                    if (string.IsNullOrEmpty(hi))
                                        break;
                                    else if (update && !cleared)
                                    {
                                        strResult.SkillH.Clear();
                                        cleared = true;
                                    }
                                    strResult.SkillH.Add(hi);
                                }
                            }
                            // KMST1196, add h_ prefix strings
                            foreach (Wz_Node child in linkNode.Nodes)
                            {
                                if (child.Text.StartsWith("h_") && int.TryParse(child.Text.Substring(2), out int level) && level > 0 && child.Value != null)
                                {
                                    strResult.SkillExtraH.Add(new KeyValuePair<int, string>(level, child.GetValue<string>()));
                                }
                            }
                            if (strResult.SkillExtraH.Count > 1)
                            {
                                strResult.SkillExtraH.Sort((left, right) => left.Key.CompareTo(right.Key));
                            }
                            strResult.SkillH.TrimExcess();
                            strResult.SkillpH.TrimExcess();
                            if (tree.FullPath == test_tree.FullPath)
                            {
                                strResult.FullPath = tree.FullPath;
                            }
                            else
                            {
                                strResult.FullPath = tree.FullPath + " -> " + test_tree.FullPath;
                            }

                            AddAllValue(strResult, linkNode);
                            if (tree.Text.Length >= 7 && Int32.TryParse(tree.Text, out id))
                            {
                                stringSkill[id] = strResult;
                            }
                            stringSkill2[tree.Text] = strResult;
                        }
                        break;
                    case "Eqp.img":
                        if (!image.TryExtract()) break;
                        foreach (Wz_Node tree0 in image.Node.Nodes)
                        {
                            foreach (Wz_Node tree1 in tree0.Nodes)
                            {
                                foreach (Wz_Node tree in tree1.Nodes)
                                {
                                    Wz_Node test_tree = TryLocateUolNode(tree);
                                    if (Int32.TryParse(tree.Text, out id) && tree.ResolveUol() is Wz_Node linkNode)
                                    {
                                        StringResult strResult = null;
                                        if (update)
                                        {
                                            try { strResult = stringEqp[id]; }
                                            catch { }
                                        }
                                        if (strResult == null) strResult = new StringResult();

                                        strResult.Name = GetDefaultString(linkNode, "name") ?? strResult.Name ?? string.Empty;
                                        strResult.Desc = GetDefaultString(linkNode, "desc") ?? strResult.Desc;
                                        if (tree.FullPath == test_tree.FullPath)
                                        {
                                            strResult.FullPath = tree.FullPath;
                                        }
                                        else
                                        {
                                            strResult.FullPath = tree.FullPath + " -> " + test_tree.FullPath;
                                        }

                                        AddAllValue(strResult, linkNode);
                                        stringEqp[id] = strResult;
                                    }
                                }
                            }
                        }
                        break;
                }
            }

            foreach (Wz_Node node in itemNode.FindNodeByPath("Special")?.Nodes ?? new Wz_Node.WzNodeCollection(null))
            {
                Wz_Image image = node.Value as Wz_Image;
                if (image == null)
                    continue;
                switch (node.Text)
                {
                    case "0910.img":
                        if (!image.TryExtract()) break;
                        foreach (Wz_Node tree in image.Node.Nodes)
                        {
                            Wz_Node test_tree = TryLocateUolNode(tree);
                            if (Int32.TryParse(tree.Text, out id) && tree.ResolveUol() is Wz_Node linkNode)
                            {
                                StringResult strResult = null;
                                if (update)
                                {
                                    try { strResult = stringItem[id]; }
                                    catch { }
                                }
                                if (strResult == null) strResult = new StringResult();

                                strResult.Name = GetDefaultString(linkNode, "name") ?? strResult.Name ?? string.Empty;
                                strResult.Desc = GetDefaultString(linkNode, "desc") ?? strResult.Desc;
                                if (tree.FullPath == test_tree.FullPath)
                                {
                                    strResult.FullPath = tree.FullPath;
                                }
                                else
                                {
                                    strResult.FullPath = tree.FullPath + " -> " + test_tree.FullPath;
                                }

                                AddAllValue(strResult, linkNode);
                                stringItem[id] = strResult;
                            }
                        }
                        break;
                }
            }

            foreach (Wz_Node node in etcNode.Nodes ?? new Wz_Node.WzNodeCollection(null))
            {
                Wz_Image image = node.Value as Wz_Image;
                if (image == null)
                    continue;
                switch (node.Text)
                {
                    case "SetItemInfo.img":
                        if (!image.TryExtract()) break;
                        foreach (Wz_Node tree in image.Node.Nodes)
                        {

                            Wz_Node test_tree = TryLocateUolNode(tree);
                            if (Int32.TryParse(tree.Text, out id) && tree.ResolveUol() is Wz_Node linkNode)
                            {
                                StringResult strResult = null;
                                if (update)
                                {
                                    try { strResult = stringSetItem[id]; }
                                    catch { }
                                }
                                if (strResult == null) strResult = new StringResult();

                                strResult.Name = GetDefaultString(linkNode, "setItemName") ?? strResult.Name ?? string.Empty;
                                if (tree.FullPath == test_tree.FullPath)
                                {
                                    strResult.FullPath = tree.FullPath;
                                }
                                else
                                {
                                    strResult.FullPath = tree.FullPath + " -> " + test_tree.FullPath;
                                }

                                AddAllValue(strResult, linkNode);
                                stringSetItem[id] = strResult;
                            }
                        }
                        break;
                }
            }

            var achievementNode = etcNode.FindNodeByPath("Achievement\\AchievementData");
            foreach (Wz_Node node in achievementNode?.Nodes ?? new Wz_Node.WzNodeCollection(null))
            {
                Wz_Image image = node.Value as Wz_Image;
                if (image == null || !image.TryExtract())
                    continue;
                Wz_Node tree = image.Node;
                Wz_Node infoNode = tree.FindNodeByPath("info");
                if (Int32.TryParse(tree.Text.Replace(".img", ""), out id) && infoNode.ResolveUol() is Wz_Node linkNode && linkNode != null)
                {
                    StringResult strResult = null;
                    if (update)
                    {
                        try { strResult = stringAchievement[id]; }
                        catch { }
                    }
                    if (strResult == null) strResult = new StringResult();

                    strResult.Name = GetDefaultString(linkNode, "name") ?? strResult.Name ?? string.Empty;
                    strResult.Desc = GetDefaultString(linkNode, "desc") ?? strResult.Desc;
                    strResult.FullPath = "AchievementData\\" + tree.FullPath;

                    //AddAllValue(strResult, linkNode);
                    stringAchievement[id] = strResult;
                }
            }

            var worldArchiveNode = etcNode.FindNodeByPath("worldArchive.img");
            if (worldArchiveNode != null)
            {
                Wz_Image worldArchiveImg = worldArchiveNode.Value as Wz_Image;
                if (worldArchiveImg != null && worldArchiveImg.TryExtract())
                {
                    Wz_Node targetNode = worldArchiveImg.Node;
                    Wz_Node infoNode = targetNode.FindNodeByPath("collectionInfo");
                    foreach (Wz_Node node in infoNode?.Nodes ?? new Wz_Node.WzNodeCollection(null))
                    {
                        foreach (Wz_Node subNode in node.Nodes)
                        {
                            switch (subNode.Text)
                            {
                                case "worldName":
                                case "worldDesc": break;
                                default:
                                    if (int.TryParse(subNode.Text, out _))
                                    {
                                        foreach (Wz_Node subNode2 in subNode.Nodes)
                                        {
                                            switch (subNode2.Text)
                                            {
                                                case "mob":
                                                    Dictionary<int, string> mobDict = new Dictionary<int, string>();
                                                    foreach (Wz_Node mobNode in subNode2.Nodes)
                                                    {
                                                        foreach (Wz_Node mobSubNode in mobNode.Nodes)
                                                        {
                                                            switch (mobSubNode.Text)
                                                            {
                                                                case "id":
                                                                    foreach (Wz_Node idNode in mobSubNode.Nodes)
                                                                    {
                                                                        mobDict[idNode.GetValueEx<int>(0)] = "(null)";
                                                                    }
                                                                    break;
                                                                case "desc":
                                                                    foreach (int mobId in mobDict.Keys)
                                                                    {
                                                                        mobDict[mobId] = mobSubNode.Value.ToString();
                                                                    }
                                                                    foreach (var kvp in mobDict)
                                                                    {
                                                                        StringResult strResult = new StringResult();
                                                                        strResult.Name = stringMob[kvp.Key].Name;
                                                                        strResult.Desc = kvp.Value;

                                                                        stringWorldArchiveMob[kvp.Key] = strResult;
                                                                    }
                                                                    mobDict.Clear();
                                                                    break;
                                                            }
                                                        }
                                                    }
                                                    break;
                                                case "npc":
                                                    Dictionary<int, string> npcDict = new Dictionary<int, string>();
                                                    foreach (Wz_Node npcNode in subNode2.Nodes)
                                                    {
                                                        foreach (Wz_Node npcSubNode in npcNode.Nodes)
                                                        {
                                                            switch (npcSubNode.Text)
                                                            {
                                                                case "id":
                                                                    foreach (Wz_Node idNode in npcSubNode.Nodes)
                                                                    {
                                                                        npcDict[idNode.GetValueEx<int>(0)] = "(null)";
                                                                    }
                                                                    break;
                                                                case "desc":
                                                                    foreach (int npcId in npcDict.Keys)
                                                                    {
                                                                        npcDict[npcId] = npcSubNode.Value.ToString();
                                                                    }
                                                                    foreach (var kvp in npcDict)
                                                                    {
                                                                        StringResult strResult = new StringResult();
                                                                        strResult.Name = stringNpc[kvp.Key].Name;
                                                                        strResult.Desc = kvp.Value;

                                                                        stringWorldArchiveNpc[kvp.Key] = strResult;
                                                                        npcDict.Clear();
                                                                    }
                                                                    break;
                                                            }
                                                        }
                                                    }
                                                    break;
                                            }
                                        }
                                    }
                                    break;
                            }
                        }
                    }
                }

            }

            Wz_Node qDataNode = questNode?.FindNodeByPath("QuestData");
            Wz_Node qInfoNode = null;
            bool newQuestDir = true;
            if (qDataNode == null)
            {
                qDataNode = questNode?.FindNodeByPath("QuestInfo.img");
                if (qDataNode != null)
                {
                    Wz_Image image = qDataNode.Value as Wz_Image;
                    if (image != null && image.TryExtract())
                    {
                        qDataNode = image.Node;
                        newQuestDir = false;
                    }
                }
            }
            foreach (Wz_Node node in qDataNode?.Nodes ?? new Wz_Node.WzNodeCollection(null))
            {
                Wz_Node tree = node;
                if (node.Value is Wz_Image image)
                {
                    if (image == null)
                        continue;

                    if (!image.TryExtract()) continue;
                    tree = image.Node;
                }
                qInfoNode = newQuestDir ? tree.FindNodeByPath("QuestInfo") : tree;
                if (Int32.TryParse(tree.Text.Replace(".img", ""), out id) && qInfoNode.ResolveUol() is Wz_Node linkNode && linkNode != null)
                {
                    StringResult strResult = null;
                    if (update)
                    {
                        try { strResult = stringQuest[id]; }
                        catch { }
                    }
                    if (strResult == null) strResult = new StringResult();

                    strResult.Name = GetDefaultString(linkNode, "name") ?? strResult.Name ?? string.Empty;
                    strResult.Desc = GetDefaultString(linkNode, "0") ?? strResult.Desc;
                    strResult.FullPath = (newQuestDir ? "QuestData\\" : "") + tree.FullPath;

                    AddAllValue(strResult, linkNode);
                    stringQuest[id] = strResult;
                }
            }

            return this.HasValues;
        }

        public void Clear()
        {
            stringEqp.Clear();
            stringItem.Clear();
            stringMob.Clear();
            stringMap.Clear();
            stringNpc.Clear();
            stringFamiliarSkill.Clear();
            stringGuildCastleGuildResearch.Clear();
            stringGuildCastlePersonalResearch.Clear();
            stringRoguelikeSkill.Clear();
            stringSkill.Clear();
            stringSkill2.Clear();
            stringSetItem.Clear();
            stringQuest.Clear();
            stringAchievement.Clear();
            stringWorldArchiveMob.Clear();
            stringWorldArchiveNpc.Clear();
            stringMonsterBook.Clear();
        }

        public bool HasValues
        {
            get
            {
                return (
                    stringEqp.Count + 
                    stringItem.Count + 
                    stringMap.Count +
                    stringMob.Count + 
                    stringNpc.Count + 
                    stringFamiliarSkill.Count + 
                    stringGuildCastleGuildResearch.Count +
                    stringGuildCastlePersonalResearch.Count +
                    stringSkill.Count + 
                    stringRoguelikeSkill.Count + 
                    stringSetItem.Count + 
                    stringQuest.Count + 
                    stringAchievement.Count > 0);
            }
        }

        private Dictionary<int, StringResult> stringEqp;
        private Dictionary<int, StringResult> stringItem;
        private Dictionary<int, StringResult> stringMap;
        private Dictionary<int, StringResult> stringMob;
        private Dictionary<int, StringResult> stringNpc;
        private Dictionary<int, StringResult> stringFamiliarSkill;
        private Dictionary<int, StringResult> stringGuildCastleGuildResearch;
        private Dictionary<int, StringResult> stringGuildCastlePersonalResearch;
        private Dictionary<string, StringResult> stringGuildCastleResearchTooltip;
        private Dictionary<int, StringResult> stringRoguelikeSkill;
        private Dictionary<int, StringResult> stringSkill;
        private Dictionary<string, StringResult> stringSkill2;
        private Dictionary<int, StringResult> stringSetItem;
        private Dictionary<int, StringResult> stringQuest;
        private Dictionary<int, StringResult> stringAchievement;
        private Dictionary<int, StringResult> stringWorldArchiveMob;
        private Dictionary<int, StringResult> stringWorldArchiveNpc;
        private Dictionary<int, StringResult> stringMonsterBook;

        private string GetDefaultString(Wz_Node node, string searchNodeText)
        {
            node = node.FindNodeByPath(searchNodeText);
            return node == null ? null : Convert.ToString(node.Value);
        }

        private void AddAllValue(StringResult sr, Wz_Node node)
        {
            foreach (Wz_Node child in node.Nodes)
            {
                if (child.Value != null)
                {
                    sr[child.Text] = child.GetValue<string>();
                }
            }
        }

        private Wz_Node TryLocateUolNode(Wz_Node node)
        {
            if (node.Value is Wz_Uol)
            {
                Wz_Uol uol = node.Value as Wz_Uol;
                Wz_Node uolNode = uol.HandleUol(node);
                if (uolNode != null)
                {
                    return uolNode;
                }
                else
                {
                    return node;
                }
            }
            else
            {
                return node;
            }
        }


        public Dictionary<int, StringResult> StringEqp
        {
            get { return stringEqp; }
        }

        public Dictionary<int, StringResult> StringItem
        {
            get { return stringItem; }
        }

        public Dictionary<int, StringResult> StringMap
        {
            get { return stringMap; }
        }

        public Dictionary<int, StringResult> StringMob
        {
            get { return stringMob; }
        }

        public Dictionary<int, StringResult> StringNpc
        {
            get { return stringNpc; }
        }

        public Dictionary<int, StringResult> StringFamiliarSkill
        {
            get { return stringFamiliarSkill; }
        }

        public Dictionary<int, StringResult> StringGuildCastleGuildResearch
        {
            get { return stringGuildCastleGuildResearch; }
        }

        public Dictionary<int, StringResult> StringGuildCastlePersonalResearch
        {
            get { return stringGuildCastlePersonalResearch; }
        }

        public Dictionary<string, StringResult> StringGuildCastleResearchTooltip
        {
            get { return stringGuildCastleResearchTooltip; }
        }

        public Dictionary<int, StringResult> StringRoguelikeSkill
        {
            get { return stringRoguelikeSkill; }
        }

        public Dictionary<int, StringResult> StringSkill
        {
            get { return stringSkill; }
        }

        public Dictionary<string, StringResult> StringSkill2
        {
            get { return stringSkill2; }
        }

        public Dictionary<int, StringResult> StringSetItem
        {
            get { return stringSetItem; }
        }

        public Dictionary<int, StringResult> StringQuest
        {
            get { return stringQuest; }
        }

        public Dictionary<int, StringResult> StringAchievement
        {
            get { return stringAchievement; }
        }

        public Dictionary<int, StringResult> StringWorldArchiveMob
        {
            get { return stringWorldArchiveMob;  }
        }

        public Dictionary<int, StringResult> StringWorldArchiveNpc
        {
            get { return stringWorldArchiveNpc; }
        }

        public Dictionary<int, StringResult> StringMonsterBook
        {
            get { return stringMonsterBook; }
        }
    }
}