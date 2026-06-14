using DevComponents.AdvTree;
using DevComponents.DotNetBar;
using DevComponents.Editors;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using WzComparerR2.CharaSim;
using WzComparerR2.Common;
using WzComparerR2.PluginBase;
using WzComparerR2.WzLib;

namespace WzComparerR2
{
    public partial class FrmWorldArchiveBrowser : DevComponents.DotNetBar.Office2007Form
    {
        public FrmWorldArchiveBrowser(bool isDarkMode = false)
        {
            InitializeComponent(); 
#if NET6_0_OR_GREATER
            // https://learn.microsoft.com/en-us/dotnet/core/compatibility/fx-core#controldefaultfont-changed-to-segoe-ui-9pt
            this.Font = new Font(new FontFamily("SimSun"), 9f);
#endif
            cmbRegion.Items.AddRange(new[]
            {
                new ComboItem("楓之谷世界"){ Value = 0 },
                new ComboItem("格蘭蒂斯"){ Value = 1 },
                new ComboItem("奧術之河"){ Value = 2 },
            });
            cmbType.Items.AddRange(new[]
            {
                new ComboItem("NPC"){ Value = 0 },
                new ComboItem("怪物"){ Value = 1 },
            });
            this.elementStyle1.TextColor = isDarkMode ? System.Drawing.Color.LightGray : System.Drawing.SystemColors.ControlText;
            this.richDescription.BackColorRichTextBox = isDarkMode ? Color.Black : Color.White;
            this.DarkMode = isDarkMode;
            this.picWorldArchiveImg.MouseDoubleClick += (s, e) =>
            {
                if (e.Button == System.Windows.Forms.MouseButtons.Left)
                {
                    this.picWorldArchiveImg_Navigate();
                }
            };
            this.picWorldArchiveImg.MouseClick += (s, e) =>
            {
                if (e.Button == System.Windows.Forms.MouseButtons.Right)
                {
                    this.picWorldArchiveImg_Save();
                }
            };
        }

        public Wz_Node EtcWaNode { get; set; }
        public Wz_Node UiWaNode { get; set; }
        public Wz_Node UiWaBonusNode { get; set; }
        public Wz_Node MobNode { get; set; }
        public Wz_Node NpcNode { get; set; }
        public StringLinker stringLinker { get; set; }
        public MainForm _mainForm { get; set; }
        private bool DarkMode;
        private Bitmap unscaledBmp;
        private Wz_Node currentExtraArtworkNode;
        public int regionID
        {
            get
            {
                return ((cmbRegion.SelectedItem as ComboItem)?.Value as int?) ?? 0;
            }
            set
            {
                var items = cmbRegion.Items.Cast<ComboItem>();
                var item = items.FirstOrDefault(_item => _item.Value as int? == value)
                    ?? items.Last();
                item.Value = value;
                cmbRegion.SelectedItem = item;
            }
        }

        public int typeID
        {
            get
            {
                return ((cmbType.SelectedItem as ComboItem)?.Value as int?) ?? 0;
            }
            set
            {
                var items = cmbType.Items.Cast<ComboItem>();
                var item = items.FirstOrDefault(_item => _item.Value as int? == value)
                    ?? items.Last();
                item.Value = value;
                cmbType.SelectedItem = item;
            }
        }

        private async void btnExport_Click(object sender, EventArgs e)
        {
            if (advTreeMap.Nodes.Count == 0) return;

            using (FolderBrowserDialog dlg = new FolderBrowserDialog())
            {
                dlg.Description = "請選擇要匯出的資料夾。";
                if (DialogResult.OK != dlg.ShowDialog()) return;

                DialogResult result1 = MessageBoxEx.Show("是否需要使用MapleWiki格式匯出？", "確認", MessageBoxButtons.YesNoCancel);
                if (result1 == DialogResult.Cancel) return;

                SetControlsEnabled(false);
                picWorldArchiveImg.Image = null;

                try
                {
                    await Task.Run(() => ExportArchiveData(dlg.SelectedPath, result1 == DialogResult.Yes));
                    UpdateText("匯出完成。");
                }
                finally
                {
                    SetControlsEnabled(true);
                }
            }
        }

        private void SetControlsEnabled(bool enabled)
        {
            cmbRegion.Enabled = enabled;
            cmbType.Enabled = enabled;
            advTreeMap.Enabled = enabled;
            advTreeLife.Enabled = enabled;
            btnTranslate.Enabled = enabled;
            btnCopyMapleStoryWikiFormat.Enabled = enabled;
            btnLocateExtraIllust.Enabled = enabled;
            btnExport.Enabled = enabled;
        }

        private void ExportArchiveData(string basePath, bool useWikiFormat)
        {
            ExportWorldDescription(basePath, useWikiFormat);
            ExportWorldIllustration(basePath);

            int mapIndex = 0;
            foreach (Node mapNode in advTreeMap.Nodes)
            {
                mapIndex++;
                UpdateText($"匯出中: {mapNode.Text}\r\n{mapIndex} / {advTreeMap.Nodes.Count}");
                ExportMapData(basePath, mapNode, useWikiFormat);
            }
        }

        private void ExportWorldDescription(string basePath, bool useWikiFormat)
        {
            var worldDescNode = EtcWaNode.FindNodeByPath($"collectionInfo\\{this.regionID}\\worldDesc", true);
            if (worldDescNode == null) return;

            string text = NormalizeLineEndings(worldDescNode.GetValue<string>());
            string content = useWikiFormat ? FormatAsWikiBlock("MapQuote", text) : text;
            File.WriteAllText(Path.Combine(basePath, "worldDesc.txt"), content);
        }

        private void ExportWorldIllustration(string basePath)
        {
            var worldIllustNode = UiWaNode.FindNodeByPath($"regionSelect\\main\\world\\{this.regionID}", true);
            if (worldIllustNode == null) return;

            BitmapOrigin bo = BitmapOrigin.CreateFromNode(worldIllustNode, PluginManager.FindWz);
            bo.Bitmap.Save(Path.Combine(basePath, "worldIllust.png"));
        }

        private void ExportMapData(string basePath, Node mapNode, bool useWikiFormat)
        {
            string mapDirectory = Path.Combine(basePath, mapNode.Text);
            Directory.CreateDirectory(mapDirectory);

            Wz_Node targetNode = mapNode.Tag as Wz_Node;
            if (!Int32.TryParse(targetNode.Text, out int mapID)) return;

            ExportMapDescription(mapDirectory, mapID, useWikiFormat);
            ExportMapIllustration(mapDirectory, mapID);
            ExportLifeData(mapDirectory, targetNode, mapID, useWikiFormat);
        }

        private void ExportMapDescription(string mapDirectory, int mapID, bool useWikiFormat)
        {
            var descNode = EtcWaNode.FindNodeByPath($"collectionInfo\\{this.regionID}\\{mapID}\\regionDesc", true);
            if (descNode == null) return;

            string text = NormalizeLineEndings(descNode.GetValue<string>());
            string content = useWikiFormat ? FormatAsWikiBlock("MapQuote", text) : text;
            File.WriteAllText(Path.Combine(mapDirectory, "mapDesc.txt"), content);
        }

        private void ExportMapIllustration(string mapDirectory, int mapID)
        {
            var illustNode = UiWaNode.FindNodeByPath($"detail\\main\\regionillust\\{this.regionID}\\{mapID}", true);
            if (illustNode == null) return;

            BitmapOrigin bo = BitmapOrigin.CreateFromNode(illustNode, PluginManager.FindWz);
            bo.Bitmap.Save(Path.Combine(mapDirectory, "mapIllust.png"));
        }

        private void ExportLifeData(string mapDirectory, Wz_Node targetNode, int mapID, bool useWikiFormat)
        {
            foreach (string lifeType in new[] { "npc", "mob" })
            {
                var lifeNodes = targetNode.FindNodeByPath(lifeType);
                if (lifeNodes == null) continue;

                string lifeDirectory = Path.Combine(mapDirectory, lifeType);
                Directory.CreateDirectory(lifeDirectory);

                foreach (var node in lifeNodes.Nodes)
                {
                    ExportLifeInstance(lifeDirectory, node, lifeType, useWikiFormat);
                }
            }
        }

        private void ExportLifeInstance(string lifeDirectory, Wz_Node node, string lifeType, bool useWikiFormat)
        {
            var idNode = node.FindNodeByPath("id");
            if (idNode == null) return;

            foreach (var id in idNode.Nodes)
            {
                int lifeID = id.GetValue<int>();
                StringResult sr = GetLifeStringResult(lifeID, lifeType);
                var lifeDescNode = node.FindNodeByPath("desc");

                if (lifeDescNode == null) continue;

                string text = NormalizeLineEndings(lifeDescNode.GetValue<string>());

                if (useWikiFormat)
                {
                    var quotes = QuoteParser.Parse(text);
                    string content = FormatAsWikiBlockWithQuotes(quotes);
                    File.WriteAllText(Path.Combine(lifeDirectory, $"{RemoveInvalidFileNameChars(sr.Name)}.txt"), content);
                }
                else
                {
                    File.WriteAllText(Path.Combine(lifeDirectory, $"{lifeID}_{RemoveInvalidFileNameChars(sr.Name)}.txt"), text);
                }
            }
        }

        private StringResult GetLifeStringResult(int lifeID, string lifeType)
        {
            StringResult sr = null;

            if (this.stringLinker != null)
            {
                switch (lifeType)
                {
                    case "npc":
                        this.stringLinker.StringNpc.TryGetValue(lifeID, out sr);
                        break;
                    case "mob":
                        this.stringLinker.StringMob.TryGetValue(lifeID, out sr);
                        break;
                }
            }

            if (sr != null) return sr;

            sr = new StringResult();
            sr.Name = lifeType == "npc" ? "未知的NPC" : lifeType == "mob" ? "未知的怪物" : "(null)";
            return sr;
        }

        private string NormalizeLineEndings(string input)
        {
            return input.Replace("\\r", "\r").Replace("\\n", "\n");
        }

        private string FormatAsWikiBlock(string key, string value)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("{{World Archive Description");
            sb.AppendLine($"|{key}={value.Replace("\r\n", "<br />").Replace("\n", "<br />")}");
            sb.AppendLine("}}");
            return sb.ToString();
        }

        private string FormatAsWikiBlockWithQuotes(Dictionary<string, List<string>> quotes)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("{{World Archive Description");

            int quoteIndex = 1;
            foreach (var quoteGroup in quotes)
            {
                foreach (var quote in quoteGroup.Value)
                {
                    sb.AppendLine($"|Quote{quoteIndex}={quote}");
                    sb.AppendLine($"|QuoteCitation{quoteIndex}={quoteGroup.Key}");
                    quoteIndex++;
                }
            }

            sb.AppendLine("}}");
            return sb.ToString();
        }

        private void btnLocateExtraIllust_Click(object sender, EventArgs e)
        {
            _mainForm.RedirectToNode(currentExtraArtworkNode);
        }

        private async void btnTranslate_Click(object sender, EventArgs e)
        {
            if (this.richDescription.TextLength > 0)
            {
                this.btnTranslate.Enabled = false;
                string originalText = this.richDescription.Text;
                UpdateText("翻譯中...");
                await Task.Run(() =>
                {
                    string translatedText = Translator.TranslateString(originalText);
                    UpdateText(translatedText);
                });
            }
        }

        private void btnCopyMapleStoryWikiFormat_Click(object sender, EventArgs e)
        {
            if (this.regionID >= 3000000)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("{{World Archive Description");
                sb.AppendLine($"|MapQuote={this.richDescription.Text.Replace("\r\n", "<br />").Replace("\n", "<br />")}");
                sb.AppendLine("}}");
                Clipboard.SetText(sb.ToString());
            }
            else if (this.advTreeLife.SelectedNode != null)
            {
                var kvp = (KeyValuePair<int, Wz_Node>)this.advTreeLife.SelectedNode.Tag;
                Wz_Node descNode = kvp.Value.FindNodeByPath("desc");
                if (descNode != null)
                {
                    string text = descNode.GetValue<string>().Replace("\\r", "\r").Replace("\\n", "\n");
                    var quotes = QuoteParser.Parse(text);
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("{{World Archive Description");
                    int quoteIndex = 1;
                    foreach (var i in quotes)
                    {
                        foreach (var j in i.Value)
                        {
                            sb.AppendLine($"|Quote{quoteIndex}={j}");
                            sb.AppendLine($"|Citation{quoteIndex}={i.Key}");
                            quoteIndex++;
                        }
                    }
                    sb.AppendLine("}}");
                    Clipboard.SetText(sb.ToString());
                }
            }
            else if (this.advTreeMap.SelectedNode != null)
            {
                Wz_Node descNode = (this.advTreeMap.SelectedNode.Tag as Wz_Node)?.FindNodeByPath("regionDesc");
                if (descNode != null)
                {
                    string text = descNode.GetValue<string>().Replace("\\r", "\r").Replace("\\n", "\n");
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("{{World Archive Description");
                    sb.AppendLine($"|MapQuote={text.Replace("\r\n", "<br />").Replace("\n", "<br />")}");
                    sb.AppendLine("}}");
                    Clipboard.SetText(sb.ToString());
                }
            }
            else
            {
                Wz_Node descNode = EtcWaNode.FindNodeByPath($"collectionInfo\\{this.regionID}\\worldDesc", true);
                if (descNode != null)
                {
                    string text = descNode.GetValue<string>().Replace("\\r", "\r").Replace("\\n", "\n");
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("{{World Archive Description");
                    sb.AppendLine($"|MapQuote={text.Replace("\r\n", "<br />").Replace("\n", "<br />")}");
                    sb.AppendLine("}}");
                    Clipboard.SetText(sb.ToString());
                }
            }
        }

        private void cmbRegion_SelectedValueChanged(object sender, EventArgs e)
        {
            this.btnTranslate.Enabled = true;
            this.advTreeMap.Nodes.Clear();
            this.advTreeLife.Nodes.Clear();
            this.richDescription.Clear();
            if (this.regionID >= 3000000)
            {
                this.cmbType.Enabled = false;
                this.btnExport.Enabled = false;
                var appendixNodes = UiWaBonusNode.FindNodeByPath($"info\\{this.regionID - 3000000}", true);
                if (appendixNodes != null)
                {
                    foreach (var appendixNode in appendixNodes.Nodes)
                    {
                        Wz_Node regionNameNode = appendixNode.FindNodeByPath("name");
                        if (regionNameNode != null)
                        {
                            var node = new Node(regionNameNode.Value.ToString());
                            node.Tag = appendixNode;
                            this.advTreeMap.Nodes.Add(node);
                        }
                    }
                }
            }
            else
            {
                this.cmbType.Enabled = true;
                this.btnExport.Enabled = true;
                var mapNodes = EtcWaNode.FindNodeByPath($"collectionInfo\\{this.regionID}", true);
                if (mapNodes != null)
                {
                    foreach (var mapNode in mapNodes.Nodes)
                    {
                        Wz_Node regionNameNode = mapNode.FindNodeByPath("regionName");
                        if (regionNameNode != null)
                        {
                            var node = new Node(regionNameNode.Value.ToString());
                            node.Tag = mapNode;
                            this.advTreeMap.Nodes.Add(node);
                        }
                    }
                    Wz_Node worldDescNode = mapNodes.FindNodeByPath("worldDesc");
                    if (worldDescNode != null)
                    {
                        UpdateText(worldDescNode.GetValue<string>().Replace("\\r", "\r").Replace("\\n", "\n"));
                    }
                }
                var worldIllustNode = UiWaNode.FindNodeByPath($"regionSelect\\main\\world\\{this.regionID}", true);
                if (worldIllustNode != null)
                {
                    BitmapOrigin bo = BitmapOrigin.CreateFromNode(worldIllustNode, PluginManager.FindWz);
                    this.picWorldArchiveImg.Image = bo.Bitmap;
                    this.unscaledBmp = null;
                }
                else
                {
                    this.picWorldArchiveImg.Image = null;
                }
            }
        }

        private void cmbType_SelectedValueChanged(object sender, EventArgs e)
        {
            this.btnTranslate.Enabled = true;
            UpdateAdvTreeLife();
        }

        private void advTreeMap_AfterNodeSelect(object sender, EventArgs e)
        {
            this.btnTranslate.Enabled = true;
            if (this.regionID >= 3000000)
            {
                UpdateAppendixEntry();
            }
            else
            {
                UpdateMapImageInfo();
                UpdateAdvTreeLife();
            }
        }

        private void advTreeLife_AfterNodeSelect(object sender, EventArgs e)
        {
            this.btnTranslate.Enabled = true;
            if (this.regionID >= 3000000)
            {
                UpdateAppendixInfo();
            }
            else
            {
                UpdateLifeImageInfo();
            }
        }

        private void advTreeLife_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.btnTranslate.Enabled = true;
            picWorldArchiveImg_Navigate();
        }

        private void picWorldArchiveImg_Navigate()
        {
            if (this.advTreeLife.SelectedNode != null)
            {
                int LifeID = this.advTreeLife.SelectedNode.Tag is KeyValuePair<int, Wz_Node> kvp ? kvp.Key : -1;
                Wz_Node lifeNode = null;
                switch (this.typeID)
                {
                    case 0:
                        lifeNode = PluginManager.FindWz(Wz_Type.Npc)?.FindNodeByPath($"{LifeID:D7}.img");
                        break;
                    case 1:
                        lifeNode = PluginManager.FindWz(Wz_Type.Mob)?.FindNodeByPath($"{LifeID:D7}.img");
                        break;
                }
                if ((lifeNode ?? (this.advTreeLife.SelectedNode.Tag as Wz_Node)) != null)
                {
                    _mainForm.RedirectToNode(lifeNode ?? (this.advTreeLife.SelectedNode.Tag as Wz_Node));
                }
                else
                {
                    ToastNotification.Show(this, $"沒有找到對應的WZ節點。", null, 2000, eToastGlowColor.Red, eToastPosition.TopCenter);
                }
            }
        }

        private void picWorldArchiveImg_Save()
        {
            if (this.picWorldArchiveImg.Image != null)
            {
                string fileName = "";
                var TypeID = this.typeID;
                if (this.advTreeLife.SelectedNode == null)
                {
                    fileName += this.advTreeMap.SelectedNode == null ? $"Map_{this.cmbRegion.SelectedItem.ToString()}" : $"Map_{this.advTreeMap.SelectedNode.Text}";
                }
                else
                {
                    switch (TypeID)
                    {
                        case 0: fileName += $"Npc_{advTreeLife.SelectedNode.Text}"; break;
                        case 1: fileName += $"Mob_{advTreeLife.SelectedNode.Text}"; break;
                    }
                }
                using (SaveFileDialog dlg = new SaveFileDialog())
                {
                    dlg.Filter = "PNG (*.png)|*.png|*.*|*.*";
                    dlg.FileName = fileName;

                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        if (this.unscaledBmp != null)
                        {
                            this.unscaledBmp.Save(dlg.FileName, System.Drawing.Imaging.ImageFormat.Png);
                        }
                        else
                        {
                            this.picWorldArchiveImg.Image.Save(dlg.FileName, System.Drawing.Imaging.ImageFormat.Png);
                        }
                    }
                }
            }
        }

        private void UpdateMapImageInfo()
        {
            this.advTreeLife.Nodes.Clear();
            int mapID = -1;
            if (this.advTreeMap.SelectedNode == null)
            {
                return;
            }
            else
            {
                Int32.TryParse((this.advTreeMap.SelectedNode.Tag as Wz_Node).Text, out mapID);
            }
            Wz_Node illustNode = UiWaNode.FindNodeByPath($"detail\\main\\regionillust\\{this.regionID}\\{mapID}", true);
            if (illustNode != null)
            {
                BitmapOrigin bo = BitmapOrigin.CreateFromNode(illustNode, PluginManager.FindWz);
                this.picWorldArchiveImg.Image = bo.Bitmap;
                this.unscaledBmp = null;
            }
            else
            {
                this.picWorldArchiveImg = null;
            }
            Wz_Node descNode = EtcWaNode.FindNodeByPath($"collectionInfo\\{this.regionID}\\{mapID}\\regionDesc", true);
            if (descNode != null)
            {
                UpdateText(descNode.GetValue<string>().Replace("\\r", "\r").Replace("\\n", "\n"));
            }
            else
            {
                this.richDescription.Clear();
            }
        }

        private void UpdateLifeImageInfo()
        {
            if (this.advTreeLife.SelectedNode == null)
            {
                return;
            }
            var TypeID = this.typeID;
            double scale = 1.00;
            KeyValuePair<int, Wz_Node> kvp = this.advTreeLife.SelectedNode.Tag as KeyValuePair<int, Wz_Node>? ?? default;
            if (kvp.Value == null) return;
            Wz_Node scaleNode = kvp.Value.FindNodeByPath("scale");
            if (scaleNode != null)
            {
                scale = scaleNode.GetValueEx<double>(100) / 100;
            }
            Wz_Node descNode = kvp.Value.FindNodeByPath("desc");
            int LifeID = kvp.Key;
            TryLocateExtraIllust(LifeID);
            if (descNode != null)
            {
                UpdateText(descNode.GetValue<string>().Replace("\\r", "\r").Replace("\\n", "\n"));
            }
            else
            {
                this.richDescription.Clear();
            }
            Bitmap bmp = null;
            Bitmap altBmp = null;
            Wz_Node lifeNode;
            Wz_Node altImageNode;
            switch (TypeID)
            {
                case 0:
                    lifeNode = PluginManager.FindWz(Wz_Type.Npc)?.FindNodeByPath($"{LifeID:D7}.img", true);
                    if (lifeNode != null)
                    {
                        Npc npc = Npc.CreateFromNode(lifeNode, PluginManager.FindWz);
                        bmp = npc.Default.Bitmap;
                    }
                    altImageNode = UiWaNode.FindNodeByPath($"image\\npc\\{LifeID:D7}", true);
                    if (altImageNode != null)
                    {
                        BitmapOrigin bo = BitmapOrigin.CreateFromNode(altImageNode, PluginManager.FindWz);
                        altBmp = bo.Bitmap;
                    }
                    break;
                case 1:
                    lifeNode = PluginManager.FindWz(Wz_Type.Mob)?.FindNodeByPath($"{LifeID:D7}.img", true);
                    if (lifeNode != null)
                    {
                        Mob mob = Mob.CreateFromNode(lifeNode, PluginManager.FindWz);
                        bmp = mob.Default.Bitmap;
                    }
                    altImageNode = UiWaNode.FindNodeByPath($"image\\mob\\{LifeID:D7}", true);
                    if (altImageNode != null)
                    {
                        BitmapOrigin bo = BitmapOrigin.CreateFromNode(altImageNode, PluginManager.FindWz);
                        altBmp = bo.Bitmap;
                    }
                    break;
            }
            this.unscaledBmp = altBmp ?? bmp;
            this.picWorldArchiveImg.Image = ResizeImage(this.unscaledBmp, scale);
        }

        private void UpdateAppendixEntry()
        {
            if (this.advTreeMap.SelectedNode == null)
            {
                return;
            }
            this.advTreeLife.Nodes.Clear();
            Wz_Node entryNode = this.advTreeMap.SelectedNode.Tag as Wz_Node;
            StringBuilder contentSb = new StringBuilder();
            foreach (var i in entryNode.Nodes)
            {
                switch (i.Text)
                {
                    case "name": break;
                    case "disable":
                        if (i.GetValue<int>() == 1)
                        {
                            this.advTreeLife.Nodes.Clear();
                            return;
                        }
                        break;
                    default:
                        string title = "";
                        string content = "";
                        foreach (var j in i.Nodes)
                        {
                            switch (j.Text)
                            {
                                case "str":
                                    content = j.GetValue<string>().Replace("\\r", "\r").Replace("\\n", "\n");
                                    break;
                                case "title":
                                    title = j.GetValue<string>();
                                    break;
                                case "url":
                                    if (Int32.TryParse(i.Text, out int id))
                                    {
                                        content = $"[画像_{id}]";
                                        Node lifeNode = new Node($"画像{id}");
                                        lifeNode.Tag = PluginManager.FindWz(j.GetValue<string>());
                                        this.advTreeLife.Nodes.Add(lifeNode);
                                    }
                                    break;
                            }
                        }
                        if (!string.IsNullOrEmpty(title)) contentSb.AppendLine(title);
                        if (!string.IsNullOrEmpty(content)) contentSb.AppendLine(content);
                        break;
                }
            }
            UpdateText(contentSb.ToString());
            if (this.advTreeLife.Nodes.Count > 0)
            {
                Wz_Node imageNode = this.advTreeLife.Nodes[0].Tag as Wz_Node;
                if (imageNode != null)
                {
                    BitmapOrigin bo = BitmapOrigin.CreateFromNode(imageNode, PluginManager.FindWz);
                    if (bo.Bitmap != null)
                    {
                        this.unscaledBmp = bo.Bitmap;
                        this.picWorldArchiveImg.Image = bo.Bitmap;
                    }
                }
            }
        }

        private void UpdateAppendixInfo()
        {
            if (this.advTreeLife.SelectedNode == null)
            {
                return;
            }
            Wz_Node imageNode = this.advTreeLife.SelectedNode.Tag as Wz_Node;
            if (imageNode != null)
            {
                BitmapOrigin bo = BitmapOrigin.CreateFromNode(imageNode, PluginManager.FindWz);
                if (bo.Bitmap != null)
                {
                    this.unscaledBmp = bo.Bitmap;
                    this.picWorldArchiveImg.Image = bo.Bitmap;
                }
            }
        }

        private void UpdateAdvTreeLife()
        {
            var TypeID = this.typeID;
            this.advTreeLife.Nodes.Clear();
            if (this.advTreeMap.SelectedNode == null)
            {
                return;
            }
            var lifeNode = this.advTreeMap.SelectedNode.Tag as Wz_Node;
            if (lifeNode != null)
            {
                string lifeType = TypeID == 0 ? "npc" : TypeID == 1 ? "mob" : null;
                if (lifeType == null) return;

                Wz_Node lifeNodes = lifeNode.FindNodeByPath(lifeType);
                if (lifeNodes != null)
                {
                    foreach (var node in lifeNodes.Nodes)
                    {
                        Wz_Node idNode = node.FindNodeByPath("id");
                        if (idNode != null)
                        {
                            foreach (var id in idNode.Nodes)
                            {
                                var lifeID = id.GetValue<int>();
                                StringResult sr = GetLifeStringResult(lifeID, lifeType);
                                var newNode = new Node($"{sr.Name} ({lifeID})");
                                newNode.Tag = new KeyValuePair<int, Wz_Node>(lifeID, node);
                                this.advTreeLife.Nodes.Add(newNode);
                            }
                        }
                    }
                }
            }
        }

        private void TryLocateExtraIllust(int npcID)
        {
            Wz_Node npcExtraArtworkNode = UiWaNode.FindNodeByPath($"illust\\npc\\{npcID}", true);
            this.currentExtraArtworkNode = npcExtraArtworkNode;
            this.btnLocateExtraIllust.Enabled = (npcExtraArtworkNode != null);
        }

        private Bitmap ResizeImage(Bitmap bmp, double scale)
        {
            if (bmp == null) return null;
            if (scale == 0)
            {
                scale = Math.Min((double)this.picWorldArchiveImg.Width / bmp.Width, (double)this.picWorldArchiveImg.Height / bmp.Height);
            }

            int w = (int)(bmp.Width * scale);
            int h = (int)(bmp.Height * scale);

            Bitmap result = new Bitmap(w, h);

            using (Graphics g = Graphics.FromImage(result))
            {
                g.InterpolationMode = scale >= 1.00 ? System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor : System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;

                g.DrawImage(bmp, new Rectangle(0, 0, w, h));
            }

            return result;
        }

        private void UpdateText(string text)
        {
            this.richDescription.Clear();
            this.richDescription.AppendText(text);
            this.richDescription.Select(0, text.Length);
            this.richDescription.SelectionColor = DarkMode ? Color.LightGray : System.Drawing.SystemColors.ControlText;
            this.richDescription.SelectionFont = new Font("Noto Sans TC", 14f);
            this.richDescription.Rtf = Regex.Replace(
                this.richDescription.Rtf,
                "#s#(.*?)#s#",
                "{\\strike $1\\strike0}",
                RegexOptions.Singleline
                );
            this.richDescription.Rtf = Regex.Replace(
                this.richDescription.Rtf,
                "#e(.*?)#n",
                "{\\b $1\\b0}",
                RegexOptions.Singleline
                );
            this.richDescription.Select(0, 0);
        }

        public void LoadCmbRegion()
        {
            Wz_Node collectionInfoNode = this.EtcWaNode.FindNodeByPath("collectionInfo", true);
            if (collectionInfoNode != null)
            {
                foreach (var i in collectionInfoNode.Nodes)
                {
                    int targetRegionID = -1;
                    if (Int32.TryParse(i.Text, out targetRegionID))
                    {
                        Wz_Node worldNameNode = i.FindNodeByPath("worldName");
                        string worldName = worldNameNode != null ? worldNameNode.GetValue<string>() : $"World{i.Text}";
                        var existingItem = this.cmbRegion.Items
                            .OfType<ComboItem>()
                            .FirstOrDefault(i => Equals(i.Value, targetRegionID));
                        if (existingItem != null)
                        {
                            if (worldName != existingItem.Text) existingItem.Text = $"{worldName} ({existingItem.Text})";
                        }
                        else
                        {
                            this.cmbRegion.Items.Add(new ComboItem(worldName) { Value = targetRegionID });
                        }
                    }
                }
            }
            Wz_Node appendixInfoNode = this.UiWaBonusNode.FindNodeByPath($"info", true);
            if (appendixInfoNode != null)
            {
                foreach (var i in appendixInfoNode.Nodes)
                {
                    Wz_Node nameNode = i.FindNodeByPath("name");
                    if (nameNode != null)
                    {
                        if (Int32.TryParse(i.Text, out int id))
                        cmbRegion.Items.Add(new ComboItem(nameNode.GetValue<string>()) { Value = 3000000 + id });
                    }
                }
            }
        }

        private string RemoveInvalidFileNameChars(string fileName)
        {
            if (String.IsNullOrEmpty(fileName)) return "未知";
            string invalidChars = new string(System.IO.Path.GetInvalidFileNameChars());
            string regexPattern = $"[{Regex.Escape(invalidChars)}]";
            return Regex.Replace(fileName, regexPattern, "_");
        }
    }

    public static class QuoteParser
    {
        private static readonly Regex CitationRegex = new Regex(@"^\s*-\s*(.+)$");

        public static Dictionary<string, List<string>> Parse(string input)
        {
            var result = new Dictionary<string, List<string>>();
            var currentQuoteLines = new List<string>();

            var lines = input.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

            foreach (var rawLine in lines)
            {
                var trimmed = rawLine.Trim();
                var match = CitationRegex.Match(trimmed);

                if (match.Success)
                {
                    var citation = match.Groups[1].Value.Trim();
                    var quote = string.Join("<br />", currentQuoteLines.Where(s => !string.IsNullOrEmpty(s)).ToList()).TrimEnd();
                    quote = Regex.Replace(
                            quote,
                            "#s#(.*?)#s#",
                            "<s>$1</s>",
                            RegexOptions.Singleline
                    );
                    quote = Regex.Replace(
                            quote,
                            "#e(.*?)#n",
                            "<strong>$1</strong>",
                            RegexOptions.Singleline
                    );

                    if (!string.IsNullOrWhiteSpace(quote))
                    {
                        if (!result.TryGetValue(citation, out var list))
                        {
                            list = new List<string>();
                            result[citation] = list;
                        }

                        list.Add(quote);
                    }
                    currentQuoteLines.Clear();
                }
                else
                {
                    currentQuoteLines.Add(rawLine);
                }
            }

            return result;
        }
    }

}
