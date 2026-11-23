using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using WzComparerR2.UnityLib.Classic;
using WzComparerR2.WzLib;

namespace WzComparerR2.Comparer
{
    public partial class EasyComparer
    {
        /// <summary>
        /// Unity 資產資料集之間的比較。兩邊都會先整理成「類 WZ」目錄樹
        /// （<see cref="ClassicWzBridge.CreateCatalogNode(ClassicWzSource, bool, bool)"/>），
        /// 再依 <see cref="Wz_Type"/> 分頁輸出，結果格式與 WZ 比較一致。
        /// </summary>
        /// <param name="labelNew">新版本的顯示名稱（例如 <c>冒険島懐旧服 1.1.2</c>）。</param>
        /// <param name="labelOld">舊版本的顯示名稱。</param>
        public void EasyCompareUnity(
            string labelNew, ClassicWzSource sourceNew,
            string labelOld, ClassicWzSource sourceOld,
            string outputDir, StreamWriter index = null)
        {
            if (sourceNew == null)
            {
                throw new ArgumentNullException(nameof(sourceNew));
            }
            if (sourceOld == null)
            {
                throw new ArgumentNullException(nameof(sourceOld));
            }

            this.Comparer = new UnityFileComparer
            {
                PngComparison = this.Comparer.PngComparison,
                ResolvePngLink = false,
                IgnoreWzFile = true,
            };

            ReportVersionToDiscord(string.Format("{0} -> {1}", labelOld, labelNew));
            StateInfo = "比較中...";
            StateDetail = "新しいバージョンの構成を読み込んでいます...";

            CreateStyleSheet(outputDir);

            UnityCompareSide sideNew = new UnityCompareSide(
                sourceNew, ClassicWzBridge.CreateCatalogNode(sourceNew, false, false));
            StateDetail = "古いバージョンの構成を読み込んでいます...";
            UnityCompareSide sideOld = new UnityCompareSide(
                sourceOld, ClassicWzBridge.CreateCatalogNode(sourceOld, false, false));

            Dictionary<Wz_Type, Dictionary<string, Wz_Node>> indexNew = BuildUnityTypeIndex(sideNew.Root);
            Dictionary<Wz_Type, Dictionary<string, Wz_Node>> indexOld = BuildUnityTypeIndex(sideOld.Root);

            List<Wz_Type> typeList = indexNew.Keys.Union(indexOld.Keys)
                .OrderBy(type => type.ToString(), StringComparer.Ordinal)
                .ToList();

            foreach (Wz_Type type in typeList)
            {
                bool selected;
                if (this.selectedNodes != null
                    && this.selectedNodes.TryGetValue(type.ToString(), out selected)
                    && !selected)
                {
                    continue;
                }

                // 型別ごとの走査は時間がかかるため、こまめに進捗を出す。
                this.unityCompareCount = 0;
                this.unityCompareNotify = 0;

                Dictionary<string, Wz_Node> dirsNew, dirsOld;
                indexNew.TryGetValue(type, out dirsNew);
                indexOld.TryGetValue(type, out dirsOld);

                List<UnityCompareEntry> entries = new List<UnityCompareEntry>();
                foreach (UnityDirectoryPair pair in MatchUnityDirectories(dirsNew, dirsOld))
                {
                    if (pair.NodeNew != null && pair.NodeOld != null)
                    {
                        CompareUnityDirectory(sideNew, pair.NodeNew, sideOld, pair.NodeOld, pair.Name, entries);
                    }
                    else if (pair.NodeNew != null)
                    {
                        AddUnityEntryTree(sideNew, pair.NodeNew, pair.Name, DifferenceType.Append, entries);
                    }
                    else
                    {
                        AddUnityEntryTree(sideOld, pair.NodeOld, pair.Name, DifferenceType.Remove, entries);
                    }
                }

                OutputUnityFile(labelNew, labelOld, type, entries, outputDir, index);
            }

            StateInfo = "比較完了";
            StateDetail = "";
            GC.Collect();
        }

        private int unityCompareCount;
        private int unityCompareNotify;
        private int unityNodeTick;
        private string unityEntryPath;
        private Wz_Node unityContentNew;
        private Wz_Node unityContentOld;

        /// <summary>走査の進捗を一定間隔で表示する（UI スレッドへの通知回数を抑える）。</summary>
        private void ReportUnityProgress(string path)
        {
            this.unityCompareCount++;
            if (this.unityCompareCount - this.unityCompareNotify < 16)
            {
                return;
            }
            this.unityCompareNotify = this.unityCompareCount;
            this.StateInfo = string.Format("比較中... {0} ({1})", path, this.unityCompareCount);
        }

        /// <summary>
        /// 比較中のノードパスを表示する（WZ 比較と同じ粒度）。ノード数が多いので間引く。
        /// </summary>
        private void ReportUnityNodeProgress(Wz_Node nodeNew, Wz_Node nodeOld)
        {
            int now = Environment.TickCount;
            if (now - this.unityNodeTick < 100)
            {
                return;
            }
            this.unityNodeTick = now;

            string path = this.GetUnityComparedPath(nodeNew, this.unityContentNew);
            if (path == null)
            {
                path = this.GetUnityComparedPath(nodeOld, this.unityContentOld);
            }
            if (path == null)
            {
                return;
            }

            this.StateInfo = string.Format("比較中... {0} ({1})", path, this.unityCompareCount + 1);
        }

        private string GetUnityComparedPath(Wz_Node node, Wz_Node contentRoot)
        {
            if (node == null || contentRoot == null || this.unityEntryPath == null)
            {
                return null;
            }
            string relative = GetUnityRelativePath(node, contentRoot);
            return string.IsNullOrEmpty(relative)
                ? this.unityEntryPath
                : CombineUnityPath(this.unityEntryPath, relative);
        }

        #region 節點走訪與差異判定

        /// <summary>依目錄名配對兩邊型別下的頂層目錄。</summary>
        private static IEnumerable<UnityDirectoryPair> MatchUnityDirectories(
            Dictionary<string, Wz_Node> dirsNew, Dictionary<string, Wz_Node> dirsOld)
        {
            if (dirsNew != null)
            {
                foreach (KeyValuePair<string, Wz_Node> item in dirsNew.OrderBy(kv => kv.Key, StringComparer.Ordinal))
                {
                    Wz_Node nodeOld = null;
                    if (dirsOld != null)
                    {
                        dirsOld.TryGetValue(item.Key, out nodeOld);
                    }
                    yield return new UnityDirectoryPair(item.Key, item.Value, nodeOld);
                }
            }

            if (dirsOld != null)
            {
                foreach (KeyValuePair<string, Wz_Node> item in dirsOld.OrderBy(kv => kv.Key, StringComparer.Ordinal))
                {
                    if (dirsNew != null && dirsNew.ContainsKey(item.Key))
                    {
                        continue;
                    }
                    yield return new UnityDirectoryPair(item.Key, null, item.Value);
                }
            }
        }

        /// <summary>把 <paramref name="root"/> 的頂層目錄依 <see cref="Wz_Type"/> 分類。</summary>
        private static Dictionary<Wz_Type, Dictionary<string, Wz_Node>> BuildUnityTypeIndex(Wz_Node root)
        {
            Dictionary<Wz_Type, Dictionary<string, Wz_Node>> index =
                new Dictionary<Wz_Type, Dictionary<string, Wz_Node>>();
            foreach (Wz_Node child in root.Nodes)
            {
                Wz_Type type = ClassicWzTypeMap.GetWzType(child.Text);
                if (type == Wz_Type.Unknown)
                {
                    continue;
                }

                Dictionary<string, Wz_Node> dirs;
                if (!index.TryGetValue(type, out dirs))
                {
                    dirs = new Dictionary<string, Wz_Node>(StringComparer.Ordinal);
                    index[type] = dirs;
                }
                dirs[child.Text] = child;
            }
            return index;
        }

        private void CompareUnityDirectory(
            UnityCompareSide sideNew, Wz_Node dirNew,
            UnityCompareSide sideOld, Wz_Node dirOld,
            string prefix, List<UnityCompareEntry> entries)
        {
            Dictionary<string, Wz_Node> oldChildren = new Dictionary<string, Wz_Node>(StringComparer.Ordinal);
            foreach (Wz_Node child in dirOld.Nodes)
            {
                oldChildren[child.Text] = child;
            }

            HashSet<string> newNames = new HashSet<string>(StringComparer.Ordinal);
            foreach (Wz_Node childNew in SortUnityNodes(dirNew.Nodes))
            {
                newNames.Add(childNew.Text);
                string path = CombineUnityPath(prefix, childNew.Text);

                Wz_Node childOld;
                if (!oldChildren.TryGetValue(childNew.Text, out childOld))
                {
                    AddUnityEntryTree(sideNew, childNew, path, DifferenceType.Append, entries);
                    continue;
                }
                CompareUnityNode(sideNew, childNew, sideOld, childOld, path, entries);
            }

            foreach (Wz_Node childOld in SortUnityNodes(dirOld.Nodes))
            {
                if (newNames.Contains(childOld.Text))
                {
                    continue;
                }
                AddUnityEntryTree(sideOld, childOld, CombineUnityPath(prefix, childOld.Text), DifferenceType.Remove, entries);
            }
        }

        private void CompareUnityNode(
            UnityCompareSide sideNew, Wz_Node nodeNew,
            UnityCompareSide sideOld, Wz_Node nodeOld,
            string path, List<UnityCompareEntry> entries)
        {
            bool isDirNew = nodeNew.Value == null && nodeNew.Nodes.Count > 0;
            bool isDirOld = nodeOld.Value == null && nodeOld.Nodes.Count > 0;

            if (isDirNew && isDirOld)
            {
                CompareUnityDirectory(sideNew, nodeNew, sideOld, nodeOld, path, entries);
                return;
            }
            if (isDirNew)
            {
                AddUnityEntryTree(sideNew, nodeNew, path, DifferenceType.Append, entries);
                return;
            }
            if (isDirOld)
            {
                AddUnityEntryTree(sideOld, nodeOld, path, DifferenceType.Remove, entries);
                return;
            }

            string displayPath = GetUnityDisplayPath(path, nodeNew.Text, nodeNew);
            UnityEntryChange change;
            this.unityEntryPath = displayPath;
            try
            {
                change = CompareUnityEntry(sideNew, nodeNew, sideOld, nodeOld);
            }
            catch (Exception ex)
            {
                // 解析できない資産があっても比較全体は続行する。
                this.FailToExportNodes[displayPath] = ex.Message;
                change = UnityEntryChange.Value;
            }
            finally
            {
                this.unityEntryPath = null;
                this.unityContentNew = null;
                this.unityContentOld = null;
            }

            ReportUnityProgress(displayPath);
            if (change == UnityEntryChange.None)
            {
                return;
            }

            entries.Add(new UnityCompareEntry
            {
                DifferenceType = DifferenceType.Changed,
                SideNew = sideNew,
                SideOld = sideOld,
                NodeNew = nodeNew,
                NodeOld = nodeOld,
                DisplayPath = displayPath,
                ValueOnly = change == UnityEntryChange.Value,
            });
        }

        /// <summary>把整棵子樹的葉節點加入差異清單（用於只有單邊存在的資產）。</summary>
        private void AddUnityEntryTree(
            UnityCompareSide side, Wz_Node node, string path,
            DifferenceType type, List<UnityCompareEntry> entries)
        {
            if (node.Value == null && node.Nodes.Count > 0)
            {
                foreach (Wz_Node child in SortUnityNodes(node.Nodes))
                {
                    AddUnityEntryTree(side, child, CombineUnityPath(path, child.Text), type, entries);
                }
                return;
            }

            ReportUnityProgress(path);
            entries.Add(new UnityCompareEntry
            {
                DifferenceType = type,
                SideNew = type == DifferenceType.Remove ? null : side,
                SideOld = type == DifferenceType.Remove ? side : null,
                NodeNew = type == DifferenceType.Remove ? null : node,
                NodeOld = type == DifferenceType.Remove ? node : null,
                DisplayPath = GetUnityDisplayPath(path, node.Text, node),
            });
        }

        /// <summary>判斷單一資產的內容是否有差異。</summary>
        private UnityEntryChange CompareUnityEntry(
            UnityCompareSide sideNew, Wz_Node nodeNew,
            UnityCompareSide sideOld, Wz_Node nodeOld)
        {
            Wz_Image imageNew, imageOld;
            bool ownedNew, ownedOld;
            Wz_Node contentNew = BuildUnityContent(sideNew, nodeNew, out imageNew, out ownedNew);
            Wz_Node contentOld = BuildUnityContent(sideOld, nodeOld, out imageOld, out ownedOld);
            try
            {
                bool hasContentNew = HasUnityContent(contentNew, imageNew, nodeNew);
                bool hasContentOld = HasUnityContent(contentOld, imageOld, nodeOld);
                if (!hasContentNew && !hasContentOld)
                {
                    // sprite canvas 之類沒有子節點的資產：直接比對值。
                    return this.Comparer.CompareData(nodeNew.Value, nodeOld.Value)
                        ? UnityEntryChange.None
                        : UnityEntryChange.Value;
                }
                if (hasContentNew != hasContentOld || contentNew == null || contentOld == null)
                {
                    return UnityEntryChange.Value;
                }

                return this.CompareUnityContent(contentNew, contentOld)
                    ? UnityEntryChange.Content
                    : UnityEntryChange.None;
            }
            finally
            {
                if (ownedNew && imageNew != null)
                {
                    imageNew.Unextract();
                }
                if (ownedOld && imageOld != null)
                {
                    imageOld.Unextract();
                }
            }
        }

        private static bool HasUnityContent(Wz_Node content, Wz_Image image, Wz_Node entry)
        {
            if (entry.Value is Wz_Image)
            {
                // img 展開失敗（content 為 null）時視同無內容，交由值比較處理。
                return content != null;
            }
            return content != null && content.Nodes.Count > 0;
        }

        /// <summary>
        /// 內容比較：最初の差異が見つかった時点で打ち切る。
        /// 比較中のノードパスを表示するため、比較フックを一時的に購読する。
        /// </summary>
        private bool CompareUnityContent(Wz_Node contentNew, Wz_Node contentOld)
        {
            this.unityContentNew = contentNew;
            this.unityContentOld = contentOld;
            this.Comparer.ComparingNode += this.ReportUnityNodeProgress;
            try
            {
                foreach (CompareDifference diff in this.Comparer.Compare(contentNew, contentOld))
                {
                    return true;
                }
                return false;
            }
            finally
            {
                this.Comparer.ComparingNode -= this.ReportUnityNodeProgress;
            }
        }

        /// <summary>
        /// 取得資產內容的節點容器：img（WZJS／音效）直接展開，文字資產解析成節點，
        /// 其餘資產回傳空容器。
        /// </summary>
        private static Wz_Node BuildUnityContent(
            UnityCompareSide side, Wz_Node entry, out Wz_Image image, out bool ownedByUs)
        {
            image = entry.Value as Wz_Image;
            ownedByUs = false;
            if (image != null)
            {
                // 既に展開済み（ツリー表示などで使用中）の場合は、こちらの都合で破棄しない。
                UnityAssetImage unityImage = image as UnityAssetImage;
                ownedByUs = unityImage == null || !unityImage.IsExtracted;
                return image.TryExtract() ? image.Node : null;
            }

            Wz_Node holder = new Wz_Node(entry.Text);
            if (entry.Value != null)
            {
                string imageKey = side.GetImageKey(entry);
                if (imageKey != null)
                {
                    try
                    {
                        ClassicWzBridge.AppendEntryContents(holder, side.Source, imageKey);
                    }
                    catch (Exception)
                    {
                        // 無法解析的資產視為無內容，差異改由值比較判斷。
                        holder.Nodes.Clear();
                    }
                }
            }
            return holder;
        }

        private static IEnumerable<Wz_Node> SortUnityNodes(IEnumerable<Wz_Node> nodes)
        {
            return nodes.OrderBy(node => node.Text, new UnityNodeNameComparer());
        }

        private sealed class UnityNodeNameComparer : IComparer<string>
        {
            public int Compare(string x, string y)
            {
                return ClassicCatalog.CompareNodeNames(x, y);
            }
        }

        /// <summary>把目錄路徑與節點名組成 WZ 風格路徑，img 會補上 <c>.img</c>。</summary>
        private static string GetUnityDisplayPath(string directoryPath, string nodeName, Wz_Node node)
        {
            string path = CombineUnityPath(directoryPath, nodeName);
            return node != null && node.Value is Wz_Image ? path + ".img" : path;
        }

        private static string CombineUnityPath(string prefix, string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return prefix;
            }
            return string.IsNullOrEmpty(prefix) ? name : prefix + "\\" + name;
        }

        #endregion

        #region HTML 輸出

        private void OutputUnityFile(
            string labelNew, string labelOld, Wz_Type type,
            List<UnityCompareEntry> entries, string outputDir, StreamWriter index)
        {
            string htmlFilePath = Path.Combine(outputDir, type.ToString() + ".html");
            for (int i = 1; File.Exists(htmlFilePath); i++)
            {
                htmlFilePath = Path.Combine(outputDir, string.Format("{0}_{1}.html", type, i));
            }
            string srcDirPath = Path.Combine(outputDir, Path.GetFileNameWithoutExtension(htmlFilePath) + "_files");
            if (this.OutputPng && !Directory.Exists(srcDirPath))
            {
                Directory.CreateDirectory(srcDirPath);
            }

            string fileName = Path.GetFileName(htmlFilePath);
            string fileNameNoExt = Path.GetFileNameWithoutExtension(htmlFilePath);
            int changed = 0, added = 0, removed = 0;
            foreach (UnityCompareEntry entry in entries)
            {
                switch (entry.DifferenceType)
                {
                    case DifferenceType.Changed: changed++; break;
                    case DifferenceType.Append: added++; break;
                    case DifferenceType.Remove: removed++; break;
                }
            }

            StreamWriter sw = null;
            StateInfo = type + "を作成しています...";
            StateDetail = "出力ファイルを作成しています...";
            try
            {
                sw = new StreamWriter(new FileStream(htmlFilePath, FileMode.Create, FileAccess.Write), Encoding.UTF8);
                sw.WriteLine("<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">");
                sw.WriteLine("<html>");
                sw.WriteLine("<head>");
                sw.WriteLine("<meta http-equiv=\"content-type\" content=\"text/html;charset=utf-8\">");
                sw.WriteLine("<title>{0} {2} → {1}</title>", type, EncodeUnityHtml(labelOld), EncodeUnityHtml(labelNew));
                sw.WriteLine("<link type=\"text/css\" rel=\"stylesheet\" href=\"style.css\" />");
                sw.WriteLine("</head>");
                sw.WriteLine("<body>");
                sw.WriteLine("<p class=\"wzf\">");
                sw.WriteLine("<table>");
                sw.WriteLine("<tr><th>&nbsp;</th><th>クライアント</th><th>バージョン</th></tr>");
                sw.WriteLine("<tr><td>新しいバージョン</td><td colspan=\"2\">{0}</td></tr>", EncodeUnityHtml(labelNew));
                sw.WriteLine("<tr><td>古いバージョン</td><td colspan=\"2\">{0}</td></tr>", EncodeUnityHtml(labelOld));
                sw.WriteLine("<tr><td>現在の時刻</td><td colspan='2'>{0:yyyy年 M月 d日 HH:mm:ss.fff}</td></tr>", DateTime.Now);
                sw.WriteLine("<tr><td>オプション</td><td colspan='2'>{0}</td></tr>", string.Join("<br/>", new[] {
                    this.OutputPng ? "- PNGファイルを出力" : null,
                    this.OutputAddedImg ? "- 追加ファイル" : null,
                    this.OutputRemovedImg ? "- 削除済みファイル" : null,
                    "- Compare " + this.Comparer.PngComparison,
                }.Where(option => option != null)));
                sw.WriteLine("</table>");
                sw.WriteLine("</p>");

                // 目次
                string[] menus = new string[3];
                int[] menuCount = new int[3];
                foreach (UnityCompareEntry entry in entries)
                {
                    int idx = GetUnityDiffIndex(entry.DifferenceType);
                    if (idx < 0)
                    {
                        continue;
                    }
                    menus[idx] += string.Format(
                        "<tr><td><a name=\"m_{0}_{1}\" href=\"#a_{0}_{1}\">{2}</a></td></tr>\r\n",
                        idx, menuCount[idx], EncodeUnityHtml(entry.DisplayPath));
                    menuCount[idx]++;
                }

                string[] diffStr = { "変更", "追加", "削除" };
                for (int i = 0; i < menus.Length; i++)
                {
                    sw.WriteLine("<table class=\"lst{0}\">", i);
                    sw.WriteLine("<tr><th><a name=\"m_{0}\">{1}:{2}</a></th></tr>", i, diffStr[i], menuCount[i]);
                    if (menus[i] != null)
                    {
                        sw.Write(menus[i]);
                    }
                    sw.WriteLine("</table>");
                    menus[i] = null;
                }

                // 詳細
                int[] count = new int[3];
                foreach (UnityCompareEntry entry in entries)
                {
                    int idx = GetUnityDiffIndex(entry.DifferenceType);
                    if (idx < 0)
                    {
                        continue;
                    }

                    OutputUnityEntry(sw, entry, idx, count[idx], count[idx] + 1, menuCount[idx], diffStr[idx],
                        labelNew, labelOld, this.OutputPng ? srcDirPath : outputDir);
                    count[idx]++;
                }

                sw.WriteLine("</body>");
                sw.WriteLine("</html>");
            }
            finally
            {
                if (sw != null)
                {
                    sw.Flush();
                    sw.Close();
                }
            }

            if (index != null)
            {
                index.WriteLine("<tr><td><a href=\"{0}.html\">{0}</a></td><td>{1}</td><td>{2}</td><td><a href=\"{0}.html#m_0\">{3}</a></td><td><a href=\"{0}.html#m_1\">{4}</a></td><td><a href=\"{0}.html#m_2\">{5}</a></td></tr>",
                    fileNameNoExt,
                    EncodeUnityHtml(labelNew),
                    EncodeUnityHtml(labelOld),
                    changed,
                    added,
                    removed
                    );
                index.Flush();
            }

            StateDetail = fileName + " を作成しました。";
        }

        private void OutputUnityEntry(
            StreamWriter sw, UnityCompareEntry entry, int idx, int anchorIndex,
            int entryIndex, int entryTotal, string diffLabel,
            string labelNew, string labelOld, string outputDir)
        {
            string anchorName = string.Format("a_{0}_{1}", idx, anchorIndex);
            string menuAnchorName = string.Format("m_{0}_{1}", idx, anchorIndex);

            StateDetail = string.Format("{0} を比較中...", entry.DisplayPath);
            List<UnityCompareRow> rows = BuildUnityRows(entry, outputDir);
            StateDetail = string.Format("合計{0}件の変更", rows.Count);

            int changed = 0, added = 0, removed = 0;
            foreach (UnityCompareRow row in rows)
            {
                switch (row.DifferenceType)
                {
                    case DifferenceType.Changed: changed++; break;
                    case DifferenceType.Append: added++; break;
                    case DifferenceType.Remove: removed++; break;
                }
            }

            StringBuilder sb = new StringBuilder();
            foreach (UnityCompareRow row in rows)
            {
                int rowIndex = GetUnityDiffIndex(row.DifferenceType);
                StateInfo = string.Format("{0}/{1} {2}: {3}", entryIndex, entryTotal, diffLabel,
                    row.Path ?? entry.DisplayPath);
                sb.AppendFormat("<tr class=\"r{0}\">", rowIndex);
                sb.AppendFormat("<td>{0}</td>", row.Path == null ? " " : EncodeUnityHtml(row.Path));
                sb.AppendFormat("<td>{0}</td>", OutputNodeValue(row.Path, row.NodeOld, 1, outputDir) ?? " ");
                sb.AppendFormat("<td>{0}</td>", OutputNodeValue(row.Path, row.NodeNew, 0, outputDir) ?? " ");
                sb.AppendLine("</tr>");
            }

            sw.WriteLine("<table class=\"img\">");
            sw.WriteLine("<tr><th colspan=\"3\"><a name=\"{1}\">{0}</a>: 変更: {2}; 追加: {3}; 削除: {4}</th></tr>",
                EncodeUnityHtml(entry.DisplayPath), anchorName, changed, added, removed);
            sw.WriteLine("<tr><th>パス</th><th>旧バージョン ({0})</th><th>新バージョン ({1})</th></tr>",
                EncodeUnityHtml(labelOld), EncodeUnityHtml(labelNew));
            sw.Write(sb.ToString());
            sw.WriteLine("<tr><td colspan=\"3\"><a href=\"#{1}\">{0}</a></td></tr>", "トップに戻る", menuAnchorName);
            sw.WriteLine("</table>");
        }

        /// <summary>產生單一資產在詳細表格中的資料列。</summary>
        private List<UnityCompareRow> BuildUnityRows(UnityCompareEntry entry, string outputDir)
        {
            List<UnityCompareRow> rows = new List<UnityCompareRow>();
            if (entry.DifferenceType == DifferenceType.Changed && entry.ValueOnly)
            {
                rows.Add(new UnityCompareRow
                {
                    DifferenceType = DifferenceType.Changed,
                    Path = entry.DisplayPath,
                    NodeNew = entry.NodeNew,
                    NodeOld = entry.NodeOld,
                });
                return rows;
            }

            Wz_Image imageNew = null, imageOld = null;
            bool ownedNew = false, ownedOld = false;
            Wz_Node contentNew = null, contentOld = null;
            try
            {
                if (entry.SideNew != null && entry.NodeNew != null)
                {
                    contentNew = BuildUnityContent(entry.SideNew, entry.NodeNew, out imageNew, out ownedNew);
                }
                if (entry.SideOld != null && entry.NodeOld != null)
                {
                    contentOld = BuildUnityContent(entry.SideOld, entry.NodeOld, out imageOld, out ownedOld);
                }

                if (entry.DifferenceType == DifferenceType.Changed)
                {
                    if (contentNew != null && contentOld != null)
                    {
                        foreach (CompareDifference diff in this.Comparer.Compare(contentNew, contentOld))
                        {
                            string pathNew = diff.NodeNew == null ? null
                                : CombineUnityPath(entry.DisplayPath, GetUnityRelativePath(diff.NodeNew, contentNew));
                            string pathOld = diff.NodeOld == null ? null
                                : CombineUnityPath(entry.DisplayPath, GetUnityRelativePath(diff.NodeOld, contentOld));
                            rows.Add(new UnityCompareRow
                            {
                                DifferenceType = diff.DifferenceType,
                                Path = pathNew ?? pathOld ?? entry.DisplayPath,
                                NodeNew = diff.NodeNew,
                                NodeOld = diff.NodeOld,
                            });
                        }
                    }
                }
                else if (entry.DifferenceType == DifferenceType.Append && contentNew != null)
                {
                    AddUnityRowsOfTree(contentNew, entry.DisplayPath, DifferenceType.Append, rows);
                }
                else if (entry.DifferenceType == DifferenceType.Remove && contentOld != null)
                {
                    AddUnityRowsOfTree(contentOld, entry.DisplayPath, DifferenceType.Remove, rows);
                }
            }
            finally
            {
                if (ownedNew && imageNew != null)
                {
                    imageNew.Unextract();
                }
                if (ownedOld && imageOld != null)
                {
                    imageOld.Unextract();
                }
            }

            if (rows.Count == 0)
            {
                rows.Add(new UnityCompareRow
                {
                    DifferenceType = entry.DifferenceType,
                    Path = entry.DisplayPath,
                    NodeNew = entry.NodeNew,
                    NodeOld = entry.NodeOld,
                });
            }
            return rows;
        }

        private static void AddUnityRowsOfTree(
            Wz_Node node, string path, DifferenceType type, List<UnityCompareRow> rows)
        {
            foreach (Wz_Node child in SortUnityNodes(node.Nodes))
            {
                string childPath = CombineUnityPath(path, child.Text);
                rows.Add(new UnityCompareRow
                {
                    DifferenceType = type,
                    Path = childPath,
                    NodeNew = type == DifferenceType.Append ? child : null,
                    NodeOld = type == DifferenceType.Remove ? child : null,
                });
                if (child.Nodes.Count > 0)
                {
                    AddUnityRowsOfTree(child, childPath, type, rows);
                }
            }
        }

        private static string GetUnityRelativePath(Wz_Node node, Wz_Node root)
        {
            if (node == root)
            {
                return null;
            }

            List<string> segments = new List<string>();
            for (Wz_Node current = node; current != null && current != root; current = current.ParentNode)
            {
                segments.Add(current.Text);
            }
            segments.Reverse();
            return string.Join("\\", segments.ToArray());
        }

        private static int GetUnityDiffIndex(DifferenceType type)
        {
            switch (type)
            {
                case DifferenceType.Changed:
                    return 0;
                case DifferenceType.Append:
                    return 1;
                case DifferenceType.Remove:
                    return 2;
                default:
                    return -1;
            }
        }

        private static string EncodeUnityHtml(string text)
        {
            return string.IsNullOrEmpty(text) ? " " : WebUtility.HtmlEncode(text);
        }

        #endregion

        #region 內部型別

        private enum UnityEntryChange
        {
            None = 0,
            Content,
            Value,
        }

        private sealed class UnityCompareSide
        {
            public UnityCompareSide(ClassicWzSource source, Wz_Node root)
            {
                this.Source = source;
                this.Root = root;
            }

            public ClassicWzSource Source { get; private set; }
            public Wz_Node Root { get; private set; }

            /// <summary>由節點在類 WZ 目錄樹中的位置還原 catalog 的資產索引鍵。</summary>
            public string GetImageKey(Wz_Node node)
            {
                List<string> segments = new List<string>();
                for (Wz_Node current = node; current != null && current != this.Root; current = current.ParentNode)
                {
                    segments.Add(current.Text);
                }
                if (segments.Count == 0)
                {
                    return null;
                }
                segments.Reverse();
                return string.Join("/", segments.ToArray());
            }
        }

        private sealed class UnityDirectoryPair
        {
            public UnityDirectoryPair(string name, Wz_Node nodeNew, Wz_Node nodeOld)
            {
                this.Name = name;
                this.NodeNew = nodeNew;
                this.NodeOld = nodeOld;
            }

            public string Name { get; private set; }
            public Wz_Node NodeNew { get; private set; }
            public Wz_Node NodeOld { get; private set; }
        }

        private sealed class UnityCompareEntry
        {
            public DifferenceType DifferenceType { get; set; }
            public UnityCompareSide SideNew { get; set; }
            public UnityCompareSide SideOld { get; set; }
            public Wz_Node NodeNew { get; set; }
            public Wz_Node NodeOld { get; set; }
            public string DisplayPath { get; set; }
            public bool ValueOnly { get; set; }
        }

        private sealed class UnityCompareRow
        {
            public DifferenceType DifferenceType { get; set; }
            public string Path { get; set; }
            public Wz_Node NodeNew { get; set; }
            public Wz_Node NodeOld { get; set; }
        }

        #endregion
    }
}
