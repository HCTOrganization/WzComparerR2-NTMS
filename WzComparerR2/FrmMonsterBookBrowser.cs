using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevComponents.DotNetBar;
using DevComponents.Editors;
using WzComparerR2.CharaSim;
using WzComparerR2.Common;
using WzComparerR2.PluginBase;
using WzComparerR2.UnityLib.Classic;
using WzComparerR2.WzLib;

namespace WzComparerR2
{
    public partial class FrmMonsterBookBrowser : DevComponents.DotNetBar.Office2007Form
    {
        public FrmMonsterBookBrowser()
        {
            InitializeComponent();
#if NET6_0_OR_GREATER
            // https://learn.microsoft.com/en-us/dotnet/core/compatibility/fx-core#controldefaultfont-changed-to-segoe-ui-9pt
            this.Font = new Font(new FontFamily("SimSun"), 9f);
#endif
            // 將多行編輯控制項的繪製矩形置中，讓搜尋框文字垂直置中顯示。
            this.txtSearch.HandleCreated += (s, e) => this.CenterSearchText();
            this.txtSearch.SizeChanged += (s, e) => this.CenterSearchText();
            this.txtSearch.FontChanged += (s, e) => this.CenterSearchText();
            // 說明欄為編輯中的控制項，無法完全透明。讓它跟隨父面板的背景色，呈現透明效果。
            this.pnlMobRow1.BackColorChanged += (s, e) => this.ApplyMobDescriptionBackColor();
            this.ApplyMobDescriptionBackColor();
            // 載入中的動畫會依右側區域的矩形手動配置。
            this.tlpRightArea.SizeChanged += (s, e) => this.UpdateLoadingOverlayBounds();
            this.picLoading.BackColor = this.tlpRightArea.BackColor;
        }

        private void ApplyMobDescriptionBackColor()
        {
            this.richMobDescription.BackColorRichTextBox = this.pnlMobRow1.BackColor;
            this.richMobDescription.BackColor = this.pnlMobRow1.BackColor;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct NativeRect
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        private const int EM_SETRECT = 0xB3;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, ref NativeRect lParam);

        private void CenterSearchText()
        {
            if (!this.txtSearch.IsHandleCreated)
            {
                return;
            }

            int textHeight = this.txtSearch.Font.Height;
            int top = Math.Max(0, (this.txtSearch.ClientSize.Height - textHeight) / 2);
            NativeRect rect = new NativeRect
            {
                Left = 0,
                Top = top,
                Right = this.txtSearch.ClientSize.Width,
                Bottom = top + textHeight,
            };
            SendMessage(this.txtSearch.Handle, EM_SETRECT, IntPtr.Zero, ref rect);
            this.txtSearch.Invalidate();
        }

        private readonly List<MobListItem> mobItems = new List<MobListItem>();
        private readonly List<MapListItem> mapItems = new List<MapListItem>();
        private readonly List<ItemListItem> itemListItems = new List<ItemListItem>();
        private readonly Dictionary<int, string> mobSearchTextCache = new Dictionary<int, string>();
        private static Dictionary<int, List<MobMapSpawnEntry>> mobToMapIndex;
        private static bool mobToMapIndexBuilding;
        private static readonly object mobToMapIndexLock = new object();
        private static readonly Dictionary<int, string> mapMarkCache = new Dictionary<int, string>();
        private int? mapListMobID;
        private static readonly Font MobStatsFont = new Font("Noto Sans TC", 10f);
        private static readonly Font MobElemAttrFont = new Font("SimSun", 10f);
        private readonly Label lblMobElemAttr = new Label
        {
            AutoSize = true,
            Name = "lblMobElemAttr",
            TabIndex = 0,
        };
        private readonly List<Bitmap> mobAnimationFrames = new List<Bitmap>();
        private readonly List<int> mobAnimationDelays = new List<int>();
        private Timer mobFilterTimer;
        private Timer mobAnimationTimer;
        private Bitmap mobAnimationPlaceholder;
        private int mobAnimationFrameIndex;
        private bool isUpdatingAnimationTypes;
        private bool isLoadingMobList;
        private readonly List<Bitmap> loadingFrames = new List<Bitmap>();
        private readonly List<int> loadingDelays = new List<int>();
        private readonly Stopwatch loadingFrameWatch = new Stopwatch();
        private bool loadingFramesLoaded;
        private int loadingFrameIndex;
        private long loadingNextFrameMs;

        public StringLinker stringLinker { get; set; }

        public MainForm _mainForm { get; set; }

        /// <summary>目前選取的怪物。第 1 欄的詳細顯示會參照此項目。</summary>
        private MobListItem SelectedMob { get; set; }
        /// <summary>怪物選取變更時發生（用於更新第 1 欄的內容）。</summary>
        public event EventHandler MobSelected;

        /// <summary>
        /// MonsterBook 資產（例如 Unity 客戶端的 <c>/MonsterBook</c>）的內容節點。
        /// 子節點名稱為怪物 ID。
        /// </summary>
        public Wz_Node MonsterBookNode { get; set; }

        /// <summary>
        /// 說明文字的來源字串表（Unity 的 <c>String/&lt;region&gt;/MonsterBook</c>）。
        /// 子節點名稱為怪物 ID，節點值為說明文字。WZ 時為 <c>null</c>。
        /// </summary>
        public Wz_Node MonsterBookStringNode { get; set; }

        public void LoadMobList()
        {
            if (this.isLoadingMobList)
            {
                return;
            }

            this.isLoadingMobList = true;
            try
            {
                this.SetLoadingVisible(true);
                this.BuildMobList();
            }
            finally
            {
                this.SetLoadingVisible(false);
                this.isLoadingMobList = false;
            }
        }

        /// <summary>表單顯示後延後執行清單建構，先繪製載入畫面。</summary>
        public void BeginLoadMobList()
        {
            if (this.IsHandleCreated && !this.IsDisposed)
            {
                this.BeginInvoke((Action)this.LoadMobList);
            }
        }

        private void SetLoadingVisible(bool visible)
        {
            // 因為重疊在同一個儲存格，載入期間會隱藏清單面板本身
            // （使用 BringToFront 會讓 TableLayoutPanel 把標籤重新配置到儲存格外）。
            this.lblLoading.Visible = visible;
            this.itemPanelMobList.Visible = !visible;
            this.txtSearch.Visible = !visible;

            // 載入期間會呈現白色的控制項（說明欄、怪物圖片、各清單）也一併隱藏。
            this.richMobDescription.Visible = !visible;
            this.picMobAnimation.Visible = !visible;
            this.itemPanelMapList.Visible = !visible;
            this.itemPanelItemList.Visible = !visible;

            if (visible)
            {
                this.StartLoadingAnimation();
                // 在建構讓 UI 執行緒停住之前，先繪製「載入中...」。
                this.lblLoading.Update();
            }
            else
            {
                this.StopLoadingAnimation();
            }
        }

        /// <summary>
        /// 開始載入中的動畫（將 <c>Npc/0002006/smile1</c> 以 200% 整數倍放大）。
        /// </summary>
        private void StartLoadingAnimation()
        {
            this.EnsureLoadingFrames();
            this.loadingFrameIndex = 0;
            if (this.loadingFrames.Count == 0)
            {
                // 無法提供動畫的客戶端仍只顯示「載入中...」。
                this.picLoading.Image = null;
                this.picLoading.Visible = false;
                this.loadingNextFrameMs = 0;
                this.loadingFrameWatch.Reset();
                return;
            }

            this.picLoading.Image = this.loadingFrames[0];
            this.loadingFrameWatch.Restart();
            this.loadingNextFrameMs = this.loadingDelays[0];

            this.UpdateLoadingOverlayBounds();
            this.picLoading.Visible = true;
            this.picLoading.BringToFront();
            this.picLoading.Refresh();
        }

        /// <summary>
        /// 將載入中的動畫覆蓋在整個右側區域（為了避開 TableLayoutPanel 的儲存格配置而手動配置）。
        /// </summary>
        private void UpdateLoadingOverlayBounds()
        {
            if (this.picLoading == null || this.tlpRightArea == null
                || this.picLoading.IsDisposed || this.tlpRightArea.IsDisposed)
            {
                return;
            }

            Rectangle screenBounds = this.tlpRightArea.RectangleToScreen(this.tlpRightArea.ClientRectangle);
            this.picLoading.Bounds = this.RectangleToClient(screenBounds);
        }

        private void StopLoadingAnimation()
        {
            this.picLoading.Visible = false;
            this.picLoading.Image = null;
            this.loadingFrameIndex = 0;
            this.loadingNextFrameMs = 0;
            this.loadingFrameWatch.Reset();
        }

        /// <summary>
        /// 清單建構讓 UI 執行緒停住期間仍推進動畫影格。
        /// （因為不執行訊息迴圈，不用擔心重入。）
        /// </summary>
        private void PumpLoadingAnimation()
        {
            if (!this.picLoading.Visible || this.loadingFrames.Count <= 1)
            {
                return;
            }

            long now = this.loadingFrameWatch.ElapsedMilliseconds;
            if (now < this.loadingNextFrameMs)
            {
                return;
            }

            this.loadingFrameIndex = (this.loadingFrameIndex + 1) % this.loadingFrames.Count;
            this.loadingNextFrameMs = now + this.loadingDelays[this.loadingFrameIndex];
            this.picLoading.Image = this.loadingFrames[this.loadingFrameIndex];
            this.picLoading.Refresh();
        }

        /// <summary>
        /// <c>Npc/0002006/smile1</c> 的影格以 200% 整數倍繪製並快取（僅第一次）。
        /// </summary>
        private void EnsureLoadingFrames()
        {
            if (this.loadingFramesLoaded)
            {
                return;
            }
            this.loadingFramesLoaded = true;

            // Unity 資料集以不含副檔名的路徑解析。
            Wz_Node animationNode = PluginManager.FindWz(@"Npc\0002006.img\smile1")
                ?? PluginManager.FindWz(@"Npc\0002006\smile1");
            if (animationNode == null)
            {
                return;
            }

            List<BitmapOrigin> sources = new List<BitmapOrigin>();
            List<int> delays = new List<int>();
            Rectangle bounds = Rectangle.Empty;
            for (int i = 0; ; i++)
            {
                Wz_Node frameNode = animationNode.FindNodeByPath(i.ToString());
                if (frameNode == null)
                {
                    break;
                }

                BitmapOrigin origin = BitmapOrigin.CreateFromNode(frameNode, PluginManager.FindWz);
                if (origin.Bitmap == null)
                {
                    continue;
                }

                Rectangle frameBounds = new Rectangle(origin.OpOrigin, origin.Bitmap.Size);
                bounds = sources.Count == 0 ? frameBounds : Rectangle.Union(bounds, frameBounds);
                sources.Add(origin);
                delays.Add(Math.Max(1, frameNode.FindNodeByPath("delay").GetValueEx(120)));
            }

            if (sources.Count == 0)
            {
                return;
            }

            try
            {
                int canvasSize = Math.Max(1, (int)Math.Ceiling(Math.Max(bounds.Width, bounds.Height) * LoadingAnimationScale));
                foreach (BitmapOrigin source in sources)
                {
                    this.loadingFrames.Add(RenderMobAnimationFrame(
                        source.Bitmap, new Rectangle(source.OpOrigin, source.Bitmap.Size), bounds,
                        LoadingAnimationScale, InterpolationMode.NearestNeighbor, canvasSize));
                }
                this.loadingDelays.AddRange(delays);
            }
            finally
            {
                foreach (BitmapOrigin source in sources)
                {
                    source.Bitmap.Dispose();
                }
            }
        }

        private void DisposeLoadingFrames()
        {
            foreach (Bitmap frame in this.loadingFrames)
            {
                frame.Dispose();
            }
            this.loadingFrames.Clear();
            this.loadingDelays.Clear();
        }

        private void BuildMobList()
        {
            itemPanelMobList.BeginUpdate();
            itemPanelMobList.Items.Clear();
            this.mobItems.Clear();
            this.mobSearchTextCache.Clear();
            this.SelectedMob = null;
            this.ClearMobIntro();

            if (this.MonsterBookNode != null)
            {
                foreach (Wz_Node mobNode in this.MonsterBookNode.Nodes)
                {
                    this.PumpLoadingAnimation();

                    int mobID;
                    if (!int.TryParse(mobNode.Text, out mobID))
                    {
                        continue;
                    }

                    StringResult mobSr = null;
                    this.stringLinker?.StringMob.TryGetValue(mobID, out mobSr);
                    string mobName = string.IsNullOrEmpty(mobSr?.Name) ? "未知的怪物" : mobSr.Name;
                    MobListItem item = new MobListItem(mobID, mobName);
                    item.SetIcon(LoadMobIcon(mobID));
                    this.mobItems.Add(item);
                    itemPanelMobList.Items.Add(item);
                }
            }

            itemPanelMobList.EndUpdate();
            this.ApplyMobFilter();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            if (this.mobFilterTimer != null)
            {
                this.mobFilterTimer.Stop();
                this.mobFilterTimer.Dispose();
                this.mobFilterTimer = null;
            }
            this.StopMobAnimation();
            this.StopLoadingAnimation();
            this.DisposeLoadingFrames();
            this.ClearMobIntro();
            base.OnFormClosed(e);
        }

        private void txtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            if (txtSearch.Text.IndexOf('\r') >= 0 || txtSearch.Text.IndexOf('\n') >= 0)
            {
                // 因為是多行編輯控制項，避免貼上時插入換行。
                int selectionStart = txtSearch.SelectionStart;
                txtSearch.Text = txtSearch.Text.Replace("\r", string.Empty).Replace("\n", string.Empty);
                txtSearch.SelectionStart = Math.Min(selectionStart, txtSearch.Text.Length);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                // 變成空白時不等待，立即回到原本的清單。
                this.mobFilterTimer?.Stop();
                this.ApplyMobFilter();
            }
            else
            {
                this.RestartMobFilterTimer();
            }
        }

        private void RestartMobFilterTimer()
        {
            if (this.mobFilterTimer == null)
            {
                this.mobFilterTimer = new Timer();
                this.mobFilterTimer.Interval = 1000;
                this.mobFilterTimer.Tick += this.MobFilterTimer_Tick;
            }
            this.mobFilterTimer.Stop();
            this.mobFilterTimer.Start();
        }

        private void MobFilterTimer_Tick(object sender, EventArgs e)
        {
            this.mobFilterTimer.Stop();
            this.ApplyMobFilter();
        }

        private void itemPanelMobList_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
            {
                return;
            }

            MobListItem item = this.itemPanelMobList.HitTest(e.X, e.Y) as MobListItem;
            if (item != null)
            {
                this.SelectMobItem(item);
            }
        }

        private void itemPanelMobList_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
            {
                return;
            }

            MobListItem item = this.itemPanelMobList.HitTest(e.X, e.Y) as MobListItem;
            if (item == null)
            {
                return;
            }

            if (this.SelectedMob != item)
            {
                this.SelectMobItem(item);
            }
            this.NavigateToMobNode(item);
        }

        private void SelectMobItem(MobListItem item)
        {
            this.SelectedMob = item;
            this.itemPanelMobList.SelectedItem = item;

            this.lblMobName.Text = item.MobName;
            this.lblMobID.Text = item.MobID.ToString();
            this.richMobDescription.Text = GetMobDescription(item.MobID);
            this.richMobDescription.SelectionStart = 0;
            this.richMobDescription.SelectionLength = 0;
            this.LoadMobStats(item.MobID);
            this.LoadAnimationTypes(item);
            this.LoadMapList(item);
            this.LoadItemList(item);

            this.MobSelected?.Invoke(this, EventArgs.Empty);
        }

        private void ClearMobIntro()
        {
            this.lblMobName.Text = string.Empty;
            this.lblMobID.Text = string.Empty;
            this.richMobDescription.Text = string.Empty;
            this.ClearMobStats();
            this.cmbAnimationType.Visible = false;
            this.mapListMobID = null;
            this.ClearMapList();
            this.ClearItemList();
            this.StopMobAnimation();

            this.isUpdatingAnimationTypes = true;
            try
            {
                this.cmbAnimationType.Items.Clear();
                this.cmbAnimationType.SelectedIndex = -1;
            }
            finally
            {
                this.isUpdatingAnimationTypes = false;
            }
            this.SetMobAnimationPlaceholder();
        }

        private void LoadAnimationTypes(MobListItem item)
        {
            this.isUpdatingAnimationTypes = true;
            try
            {
                this.cmbAnimationType.Items.Clear();

                int moveIndex = -1, standIndex = -1;
                foreach (string animationName in GetMobAnimationNames(item.MobID))
                {
                    int index = this.cmbAnimationType.Items.Add(new ComboItem(animationName) { Value = animationName });
                    if (moveIndex < 0 && string.Equals(animationName, "move", StringComparison.OrdinalIgnoreCase))
                    {
                        moveIndex = index;
                    }
                    if (standIndex < 0 && string.Equals(animationName, "stand", StringComparison.OrdinalIgnoreCase))
                    {
                        standIndex = index;
                    }
                }

                // 以 move 為預設，資料集沒有時依 stand → 第一個的順序遞補。
                int selectedIndex = moveIndex >= 0 ? moveIndex
                    : standIndex >= 0 ? standIndex
                    : this.cmbAnimationType.Items.Count > 0 ? 0 : -1;
                this.cmbAnimationType.SelectedIndex = selectedIndex;
                this.cmbAnimationType.Visible = this.cmbAnimationType.Items.Count > 0;
            }
            finally
            {
                this.isUpdatingAnimationTypes = false;
            }

            this.ShowMobAnimation(item, this.SelectedAnimationName);
        }

        private string SelectedAnimationName
        {
            get
            {
                return (this.cmbAnimationType.SelectedItem as ComboItem)?.Value as string
                    ?? this.cmbAnimationType.SelectedItem as string;
            }
        }

        private void cmbAnimationType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.isUpdatingAnimationTypes)
            {
                return;
            }
            this.ShowMobAnimation(this.SelectedMob, this.SelectedAnimationName);
        }

        /// <summary>
        /// 在左欄下方顯示怪物的基本能力與屬性抗性。
        /// </summary>
        private void LoadMobStats(int mobID)
        {
            Wz_Node mobNode = PluginManager.FindWz(string.Format(@"Mob\{0:D7}.img", mobID));
            Mob mob = mobNode == null ? null : Mob.CreateFromNode(mobNode, PluginManager.FindWz);
            if (mob == null)
            {
                this.ClearMobStats();
                return;
            }

            string[][] rows =
            {
                new[] { "HP", ItemStringHelper.ToCJKNumberExpr(mob.MaxHP) },
                new[] { "MP", ItemStringHelper.ToCJKNumberExpr(mob.MaxMP) },
                new[] { "物理傷害", ItemStringHelper.ToCJKNumberExpr(mob.PADamage) },
                new[] { "魔法傷害", ItemStringHelper.ToCJKNumberExpr(mob.MADamage) },
                new[] { "物理防御率", mob.PDRate.ToString() + "%" },
                new[] { "魔法防御率", mob.MDRate.ToString() + "%" },
                new[] { "擊退率", ItemStringHelper.ToCJKNumberExpr(mob.Pushed) },
                new[] { "EXP", ItemStringHelper.ToCJKNumberExpr(mob.Exp) },
            };
            this.BuildMobStats(rows, GetMobElemAttrText(mob.ElemAttr));
        }

        private void ClearMobStats()
        {
            this.tlpMobStats.SuspendLayout();
            this.tlpMobStats.Controls.Clear();
            this.tlpMobStats.RowStyles.Clear();
            this.tlpMobStats.RowCount = 0;
            this.tlpMobStats.ResumeLayout(true);
        }

        private void BuildMobStats(string[][] rows, string elemAttrText)
        {
            this.tlpMobStats.SuspendLayout();
            this.tlpMobStats.Controls.Clear();
            this.tlpMobStats.RowStyles.Clear();
            this.tlpMobStats.RowCount = 0;

            foreach (string[] row in rows)
            {
                int rowIndex = this.tlpMobStats.RowCount;
                this.tlpMobStats.RowCount++;
                this.tlpMobStats.RowStyles.Add(new RowStyle(SizeType.AutoSize));

                Label nameLabel = new Label
                {
                    AutoSize = true,
                    Font = MobStatsFont,
                    Margin = new System.Windows.Forms.Padding(0, 0, 8, 0),
                    Text = row[0],
                };
                Label valueLabel = new Label
                {
                    AutoSize = false,
                    Dock = DockStyle.Fill,
                    Font = MobStatsFont,
                    Margin = new System.Windows.Forms.Padding(0),
                    TextAlign = ContentAlignment.MiddleRight,
                    Text = row[1],
                };
                this.tlpMobStats.Controls.Add(nameLabel, 0, rowIndex);
                this.tlpMobStats.Controls.Add(valueLabel, 1, rowIndex);
            }

            if (!string.IsNullOrEmpty(elemAttrText))
            {
                int rowIndex = this.tlpMobStats.RowCount;
                this.tlpMobStats.RowCount++;
                this.tlpMobStats.RowStyles.Add(new RowStyle(SizeType.AutoSize));

                this.lblMobElemAttr.Font = MobElemAttrFont;
                this.lblMobElemAttr.Margin = new System.Windows.Forms.Padding(0, 8, 0, 0);
                this.lblMobElemAttr.Text = elemAttrText;
                this.tlpMobStats.Controls.Add(this.lblMobElemAttr, 0, rowIndex);
                this.tlpMobStats.SetColumnSpan(this.lblMobElemAttr, 2);
            }

            this.tlpMobStats.ResumeLayout(true);
        }

        /// <summary>
        /// 以「冰雷火毒聖暗物」與抗性符號兩行表示屬性攻擊的抗性（與怪物提示框相同表記）。
        /// </summary>
        private static string GetMobElemAttrText(MobElemAttr elemAttr)
        {
            if (elemAttr == null)
            {
                return string.Empty;
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("冰雷火毒聖暗物");
            sb.Append(GetElemResistText(elemAttr.I));
            sb.Append(GetElemResistText(elemAttr.L));
            sb.Append(GetElemResistText(elemAttr.F));
            sb.Append(GetElemResistText(elemAttr.S));
            sb.Append(GetElemResistText(elemAttr.H));
            sb.Append(GetElemResistText(elemAttr.D));
            sb.Append(GetElemResistText(elemAttr.P));
            return sb.ToString();
        }

        private static string GetElemResistText(ElemResistance resist)
        {
            switch (resist)
            {
                case ElemResistance.Immune: return "×";
                case ElemResistance.Resist: return "△";
                case ElemResistance.Normal: return "○";
                case ElemResistance.Weak: return "◎";
                default: return "  ";
            }
        }

        /// <summary>
        /// 取得 <c>String/&lt;region&gt;/MonsterBook/&lt;id&gt;</c> 的說明文字。
        /// Unity 資料集使用字串表側的節點值，WZ 則以 <c>episode</c> 子節點為說明文字
        /// （Unity 的 MonsterBook 資產側 <c>episode</c> 只有話數編號，因此不參照）。
        /// </summary>
        private string GetMobDescription(int mobID)
        {
            string text = this.MonsterBookStringNode?.FindNodeByPath(mobID.ToString())?.Value?.ToString();

            if (string.IsNullOrEmpty(text))
            {
                Wz_Node descriptionNode = PluginManager.FindWz(string.Format(@"String\MonsterBook.img\{0}", mobID));
                text = descriptionNode?.FindNodeByPath("episode")?.Value?.ToString()
                    ?? descriptionNode?.Value?.ToString();
            }

            if (string.IsNullOrEmpty(text))
            {
                // 單獨開啟 MonsterBook.img 時的後備處理（忽略只有數值的 episode）。
                Wz_Node entryNode = this.MonsterBookNode?.FindNodeByPath(mobID.ToString());
                string entryText = entryNode?.FindNodeByPath("episode")?.Value?.ToString() ?? entryNode?.Value?.ToString();
                int ignored;
                if (!string.IsNullOrEmpty(entryText) && !int.TryParse(entryText, out ignored))
                {
                    text = entryText;
                }
            }

            if (string.IsNullOrEmpty(text))
            {
                StringResult mobSr;
                if (this.stringLinker != null
                    && this.stringLinker.StringMonsterBook.TryGetValue(mobID, out mobSr))
                {
                    text = mobSr.Desc;
                }
            }

            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }
            return text.Replace("\\r\\n", "\r\n").Replace("\\r", "\r").Replace("\\n", "\n");
        }

        private static IEnumerable<string> GetMobAnimationNames(int mobID)
        {
            Wz_Node mobNode = PluginManager.FindWz(string.Format(@"Mob\{0:D7}.img", mobID));
            if (mobNode == null)
            {
                yield break;
            }

            foreach (Wz_Node child in mobNode.Nodes)
            {
                if (string.Equals(child.Text, "info", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                if (IsMobAnimationNode(child))
                {
                    yield return child.Text;
                }
            }
        }

        private static bool IsMobAnimationNode(Wz_Node node)
        {
            Wz_Node firstFrame = node.FindNodeByPath("0");
            return firstFrame != null
                && (firstFrame.Value is Wz_Png || firstFrame.Value is IWzPngSource || firstFrame.Value is Wz_Uol);
        }

        private void ShowMobAnimation(MobListItem item, string animationName)
        {
            this.StopMobAnimation();

            if (item == null || string.IsNullOrEmpty(animationName))
            {
                this.SetMobAnimationPlaceholder();
                return;
            }

            Wz_Node mobNode = PluginManager.FindWz(string.Format(@"Mob\{0:D7}.img", item.MobID));
            Wz_Node animationNode = mobNode?.FindNodeByPath(animationName);
            if (animationNode == null || !this.BuildMobAnimation(animationNode))
            {
                this.SetMobAnimationPlaceholder();
                return;
            }

            this.mobAnimationFrameIndex = 0;
            this.mobAnimationTimer = new Timer();
            this.mobAnimationTimer.Tick += this.MobAnimationTimer_Tick;
            this.picMobAnimation.Image = this.mobAnimationFrames[0];
            this.mobAnimationTimer.Interval = this.mobAnimationDelays[0];
            if (this.mobAnimationFrames.Count > 1)
            {
                this.mobAnimationTimer.Start();
            }
        }

        private bool BuildMobAnimation(Wz_Node animationNode)
        {
            List<BitmapOrigin> sources = new List<BitmapOrigin>();
            List<int> delays = new List<int>();
            Rectangle bounds = Rectangle.Empty;

            for (int i = 0; ; i++)
            {
                Wz_Node frameNode = animationNode.FindNodeByPath(i.ToString());
                if (frameNode == null)
                {
                    break;
                }

                BitmapOrigin origin = BitmapOrigin.CreateFromNode(frameNode, PluginManager.FindWz);
                if (origin.Bitmap == null)
                {
                    continue;
                }

                Rectangle frameBounds = new Rectangle(origin.OpOrigin, origin.Bitmap.Size);
                bounds = sources.Count == 0 ? frameBounds : Rectangle.Union(bounds, frameBounds);
                sources.Add(origin);
                delays.Add(Math.Max(1, frameNode.FindNodeByPath("delay").GetValueEx(120)));
            }

            if (sources.Count == 0)
            {
                return false;
            }

            if (animationNode.FindNodeByPath("zigzag").GetValueEx(0) != 0)
            {
                // zigzag：0 1 2 3 → 0 1 2 3 2 1，在尾端追加往返的部分。
                for (int i = sources.Count - 2; i > 0; i--)
                {
                    sources.Add(sources[i]);
                    delays.Add(delays[i]);
                }
            }

            float scale = GetMobScaleFactor(Math.Max(bounds.Width, bounds.Height), MobAnimationSize, true, out InterpolationMode interpolationMode);
            try
            {
                foreach (BitmapOrigin source in sources)
                {
                    this.mobAnimationFrames.Add(RenderMobAnimationFrame(
                        source.Bitmap, new Rectangle(source.OpOrigin, source.Bitmap.Size), bounds,
                        scale, interpolationMode, MobAnimationSize));
                    this.mobAnimationDelays.Add(0);
                }
            }
            finally
            {
                foreach (BitmapOrigin source in sources)
                {
                    source.Bitmap.Dispose();
                }
            }

            for (int i = 0; i < delays.Count; i++)
            {
                this.mobAnimationDelays[i] = delays[i];
            }
            return true;
        }

        private void MobAnimationTimer_Tick(object sender, EventArgs e)
        {
            if (this.mobAnimationFrames.Count == 0)
            {
                return;
            }

            this.mobAnimationFrameIndex = (this.mobAnimationFrameIndex + 1) % this.mobAnimationFrames.Count;
            this.picMobAnimation.Image = this.mobAnimationFrames[this.mobAnimationFrameIndex];
            this.mobAnimationTimer.Interval = this.mobAnimationDelays[this.mobAnimationFrameIndex];
        }

        private void StopMobAnimation()
        {
            if (this.mobAnimationTimer != null)
            {
                this.mobAnimationTimer.Stop();
                this.mobAnimationTimer.Tick -= this.MobAnimationTimer_Tick;
                this.mobAnimationTimer.Dispose();
                this.mobAnimationTimer = null;
            }

            this.picMobAnimation.Image = null;
            foreach (Bitmap frame in this.mobAnimationFrames)
            {
                frame.Dispose();
            }
            this.mobAnimationFrames.Clear();
            this.mobAnimationDelays.Clear();
            this.mobAnimationFrameIndex = 0;

            this.mobAnimationPlaceholder?.Dispose();
            this.mobAnimationPlaceholder = null;
        }

        private void SetMobAnimationPlaceholder()
        {
            this.mobAnimationPlaceholder?.Dispose();
            this.mobAnimationPlaceholder = new Bitmap(MobAnimationSize, MobAnimationSize, PixelFormat.Format32bppArgb);
            this.picMobAnimation.Image = this.mobAnimationPlaceholder;
        }

        private void itemPanelMapList_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
            {
                return;
            }

            MapListItem item = this.itemPanelMapList.HitTest(e.X, e.Y) as MapListItem;
            if (item != null)
            {
                this.NavigateToMapNode(item);
            }
        }

        private void NavigateToMapNode(MapListItem item)
        {
            if (this._mainForm == null)
            {
                return;
            }

            // Map.wz 的結構：Map/Map<首位>/<9 位數 ID>.img
            Wz_Node mapNode = PluginManager.FindWz(string.Format(@"Map\Map\Map{0}\{1:D9}.img", item.MapID / 100000000, item.MapID));
            if (mapNode != null)
            {
                this._mainForm.RedirectToNode(mapNode);
            }
            else
            {
                ToastNotification.Show(this, "找不到對應的地圖節點。", null, 2000, eToastGlowColor.Red, eToastPosition.TopCenter);
            }
        }

        private void LoadMapList(MobListItem item)
        {
            this.mapListMobID = item?.MobID;
            this.ClearMapList();
            if (!this.mapListMobID.HasValue)
            {
                return;
            }

            // String/MonsterBook.img 的 <id>/map 中有出現地圖清單（WZ / Unity 共通）。
            List<int> nodeMaps = this.GetMonsterBookMapIDs(this.mapListMobID.Value);
            if (nodeMaps != null)
            {
                if (nodeMaps.Count == 0)
                {
                    this.itemPanelMapList.Items.Add(new LabelItem { Text = "無出現地圖" });
                    return;
                }

                this.itemPanelMapList.BeginUpdate();
                foreach (int mapID in nodeMaps.OrderBy(id => id))
                {
                    MapListItem mapItem = new MapListItem(mapID, this.GetMapDisplayName(mapID));
                    mapItem.SetIcon(LoadMapIcon(TryGetMapMark(mapID)));
                    this.mapItems.Add(mapItem);
                    this.itemPanelMapList.Items.Add(mapItem);
                }
                this.itemPanelMapList.EndUpdate();
                return;
            }

            // 舊資料等沒有 map 節點時，會掃描所有地圖來求得。
            this.PopulateMapList();
        }

        /// <summary>
        /// 傳回列舉於 <c>MonsterBook.img/&lt;id&gt;/map</c> 的地圖 ID。
        /// 若連 <c>map</c> 節點本身都沒有，則傳回 <c>null</c>（出現地圖不明）。
        /// </summary>
        private List<int> GetMonsterBookMapIDs(int mobID)
        {
            Wz_Node mapNode = this.MonsterBookNode?.FindNodeByPath(mobID.ToString())?.FindNodeByPath("map");
            if (mapNode == null)
            {
                return null;
            }

            List<int> result = new List<int>();
            foreach (Wz_Node child in mapNode.Nodes)
            {
                int mapID;
                string raw = child.Value?.ToString() ?? child.Text;
                if (int.TryParse(raw, out mapID) && mapID > 0 && !result.Contains(mapID))
                {
                    result.Add(mapID);
                }
            }
            return result;
        }

        /// <summary>取得地圖 img 的 <c>info/mapMark</c>（MapHelper 的標記名稱）。</summary>
        private static string TryGetMapMark(int mapID)
        {
            lock (mapMarkCache)
            {
                string cached;
                if (mapMarkCache.TryGetValue(mapID, out cached))
                {
                    return cached;
                }
            }

            string mapMark = null;
            Wz_Node mapNode = PluginManager.FindWz(string.Format(@"Map\Map\Map{0}\{1:D9}.img", mapID / 100000000, mapID));
            Wz_Image mapImage = mapNode?.GetValue<Wz_Image>();
            if (mapImage != null)
            {
                UnityAssetImage unityImage = mapImage as UnityAssetImage;
                bool wasExtracted = unityImage != null
                    ? unityImage.IsExtracted
                    : mapImage.Node != null && mapImage.Node.Nodes.Count > 0;
                if (mapImage.TryExtract())
                {
                    try
                    {
                        mapMark = mapImage.Node?.FindNodeByPath("info")?.FindNodeByPath("mapMark")?.Value?.ToString();
                    }
                    finally
                    {
                        // 釋放掃描時展開的 img，避免保留所有地圖的記憶體。
                        if (!wasExtracted)
                        {
                            mapImage.Unextract();
                        }
                    }
                }
            }

            lock (mapMarkCache)
            {
                mapMarkCache[mapID] = mapMark;
            }
            return mapMark;
        }

        private void ClearMapList()
        {
            this.itemPanelMapList.BeginUpdate();
            this.itemPanelMapList.Items.Clear();
            foreach (MapListItem mapItem in this.mapItems)
            {
                mapItem.SetIcon(null);
            }
            this.mapItems.Clear();
            this.itemPanelMapList.EndUpdate();
        }

        private void PopulateMapList()
        {
            if (this.IsDisposed)
            {
                return;
            }

            this.ClearMapList();
            if (!this.mapListMobID.HasValue)
            {
                return;
            }

            int mobID = this.mapListMobID.Value;
            Dictionary<int, List<MobMapSpawnEntry>> index;
            lock (mobToMapIndexLock)
            {
                index = mobToMapIndex;
            }

            if (index == null)
            {
                this.EnsureMobToMapIndex();
                this.itemPanelMapList.Items.Add(new LabelItem { Text = "正在搜尋地圖..." });
                return;
            }

            List<MobMapSpawnEntry> spawns;
            if (!index.TryGetValue(mobID, out spawns) || spawns.Count == 0)
            {
                this.itemPanelMapList.Items.Add(new LabelItem { Text = "無出現地圖" });
                return;
            }

            this.itemPanelMapList.BeginUpdate();
            foreach (MobMapSpawnEntry spawn in spawns.OrderBy(s => s.MapID))
            {
                string mapName = this.GetMapDisplayName(spawn.MapID);
                MapListItem item = new MapListItem(spawn.MapID, mapName);
                item.SetIcon(LoadMapIcon(spawn.MapMark));
                this.mapItems.Add(item);
                this.itemPanelMapList.Items.Add(item);
            }
            this.itemPanelMapList.EndUpdate();
        }

        private string GetMapDisplayName(int mapID)
        {
            string mapName;
            return this.TryGetMapName(mapID, out mapName)
                ? mapName
                : string.Format("未知地圖 ({0})", mapID);
        }

        private bool TryGetMapName(int mapID, out string mapName)
        {
            mapName = null;
            StringResult mapSr;
            if (this.stringLinker != null
                && this.stringLinker.StringMap.TryGetValue(mapID, out mapSr)
                && !string.IsNullOrEmpty(mapSr.Name))
            {
                mapName = mapSr.Name;
                return true;
            }
            return false;
        }

        private static Bitmap LoadMapIcon(string mapMark)
        {
            Bitmap placeholder = new Bitmap(MobIconSize, MobIconSize, PixelFormat.Format32bppArgb);
            if (string.IsNullOrEmpty(mapMark))
            {
                return placeholder;
            }
            placeholder.Dispose();

            Wz_Node markNode = PluginManager.FindWz(string.Format(@"Map\MapHelper.img\mark\{0}", mapMark));
            Bitmap source = BitmapOrigin.CreateFromNode(markNode, PluginManager.FindWz).Bitmap;
            try
            {
                return CreateScaledMobBitmap(source, MobIconSize);
            }
            finally
            {
                source?.Dispose();
            }
        }

        /// <summary>
        /// 掃描所有地圖的 life 節點，建立「怪物 ID → 出現地圖」索引。
        /// 建構一次需數秒，因此在背景只執行一次並快取。
        /// </summary>
        private void EnsureMobToMapIndex()
        {
            lock (mobToMapIndexLock)
            {
                if (mobToMapIndex != null || mobToMapIndexBuilding)
                {
                    return;
                }
                mobToMapIndexBuilding = true;
            }

            Task.Run(() =>
            {
                Dictionary<int, List<MobMapSpawnEntry>> index = null;
                try
                {
                    index = BuildMobToMapIndex();
                }
                catch
                {
                    // 失敗時不建立索引，以便下次選取時重試。
                    index = null;
                }

                lock (mobToMapIndexLock)
                {
                    if (index != null)
                    {
                        mobToMapIndex = index;
                    }
                    mobToMapIndexBuilding = false;
                }

                try
                {
                    if (this.IsHandleCreated && !this.IsDisposed)
                    {
                        this.BeginInvoke((Action)this.OnMobToMapIndexReady);
                    }
                }
                catch (InvalidOperationException)
                {
                    // 表單已關閉時忽略。
                }
            });
        }

        /// <summary>地圖索引完成後更新清單，並讓包含地圖名稱的搜尋也生效。</summary>
        private void OnMobToMapIndexReady()
        {
            if (this.IsDisposed)
            {
                return;
            }

            this.mobSearchTextCache.Clear();
            this.PopulateMapList();
            this.ApplyMobFilter();
        }

        private static Dictionary<int, List<MobMapSpawnEntry>> BuildMobToMapIndex()
        {
            Dictionary<int, List<MobMapSpawnEntry>> index = new Dictionary<int, List<MobMapSpawnEntry>>();
            Wz_Node mapWzRoot = PluginManager.FindWz(Wz_Type.Map);
            // Unity 資料集是 Map/Map/&lt;群組&gt;/，WZ 則是 Map/&lt;群組&gt;/，階層不同。
            Wz_Node mapsNode = mapWzRoot?.FindNodeByPath("Map") ?? mapWzRoot;
            if (mapsNode == null)
            {
                return index;
            }

            foreach (Wz_Node groupNode in mapsNode.Nodes)
            {
                foreach (Wz_Node mapNode in groupNode.Nodes)
                {
                    int mapID;
                    string nodeName = mapNode.Text ?? string.Empty;
                    if (nodeName.EndsWith(".img", StringComparison.OrdinalIgnoreCase))
                    {
                        nodeName = nodeName.Substring(0, nodeName.Length - 4);
                    }
                    if (!int.TryParse(nodeName, out mapID))
                    {
                        continue;
                    }

                    Wz_Image mapImage = mapNode.GetValue<Wz_Image>();
                    if (mapImage == null)
                    {
                        continue;
                    }

                    UnityAssetImage unityImage = mapImage as UnityAssetImage;
                    bool wasExtracted = unityImage?.IsExtracted ?? false;
                    if (!mapImage.TryExtract())
                    {
                        continue;
                    }

                    try
                    {
                        Wz_Node mapRoot = mapImage.Node;
                        Wz_Node lifeNode = mapRoot?.FindNodeByPath("life");
                        if (lifeNode == null)
                        {
                            continue;
                        }

                        string mapMark = mapRoot.FindNodeByPath("info")?.FindNodeByPath("mapMark")?.Value?.ToString();
                        foreach (Wz_Node life in lifeNode.Nodes)
                        {
                            if (life.FindNodeByPath("type")?.Value?.ToString() != "m")
                            {
                                continue;
                            }

                            int mobID;
                            if (!int.TryParse(life.FindNodeByPath("id")?.Value?.ToString(), out mobID))
                            {
                                continue;
                            }

                            List<MobMapSpawnEntry> list;
                            if (!index.TryGetValue(mobID, out list))
                            {
                                index[mobID] = list = new List<MobMapSpawnEntry>();
                            }
                            if (!list.Exists(s => s.MapID == mapID))
                            {
                                list.Add(new MobMapSpawnEntry(mapID, mapMark));
                            }
                        }
                    }
                    finally
                    {
                        // 釋放掃描時展開的 img，避免保留所有地圖的記憶體。
                        if (!wasExtracted)
                        {
                            mapImage.Unextract();
                        }
                    }
                }
            }

            return index;
        }

        private struct MobMapSpawnEntry
        {
            public MobMapSpawnEntry(int mapID, string mapMark)
            {
                this.MapID = mapID;
                this.MapMark = mapMark;
            }

            public int MapID;

            public string MapMark;
        }

        private void LoadItemList(MobListItem mob)
        {
            this.ClearItemList();
            if (mob == null || this.MonsterBookNode == null)
            {
                return;
            }

            Wz_Node rewardNode = this.MonsterBookNode.FindNodeByPath(mob.MobID.ToString())?.FindNodeByPath("reward");
            if (rewardNode == null || rewardNode.Nodes.Count == 0)
            {
                return;
            }

            this.itemPanelItemList.BeginUpdate();
            foreach (Wz_Node reward in rewardNode.Nodes)
            {
                int itemID;
                if (!int.TryParse(reward.Value?.ToString(), out itemID) || itemID <= 0)
                {
                    continue;
                }

                ItemListItem item = new ItemListItem(itemID, this.GetItemDisplayName(itemID));
                item.SetIcon(LoadItemIcon(itemID));
                this.itemListItems.Add(item);
                this.itemPanelItemList.Items.Add(item);
            }
            this.itemPanelItemList.EndUpdate();
        }

        private void ClearItemList()
        {
            this.itemPanelItemList.BeginUpdate();
            this.itemPanelItemList.Items.Clear();
            foreach (ItemListItem item in this.itemListItems)
            {
                item.SetIcon(null);
            }
            this.itemListItems.Clear();
            this.itemPanelItemList.EndUpdate();
        }

        private string GetItemDisplayName(int itemID)
        {
            string itemName;
            return this.TryGetItemName(itemID, out itemName)
                ? itemName
                : string.Format("未知道具 ({0})", itemID);
        }

        private bool TryGetItemName(int itemID, out string itemName)
        {
            itemName = null;
            if (this.stringLinker == null)
            {
                return false;
            }

            StringResult itemSr = null;
            if (itemID < 2000000)
            {
                this.stringLinker.StringEqp.TryGetValue(itemID, out itemSr);
            }
            else
            {
                this.stringLinker.StringItem.TryGetValue(itemID, out itemSr);
            }
            if (string.IsNullOrEmpty(itemSr?.Name))
            {
                return false;
            }
            itemName = itemSr.Name;
            return true;
        }

        private void itemPanelItemList_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
            {
                return;
            }

            ItemListItem item = this.itemPanelItemList.HitTest(e.X, e.Y) as ItemListItem;
            if (item == null || this._mainForm == null)
            {
                return;
            }

            Wz_Node itemNode = FindItemImageNode(item.ItemID);
            if (itemNode != null)
            {
                this._mainForm.RedirectToNode(itemNode);
            }
            else
            {
                ToastNotification.Show(this, "找不到對應的道具節點。", null, 2000, eToastGlowColor.Red, eToastPosition.TopCenter);
            }
        }

        /// <summary>
        /// 從道具 ID 解析 img 節點。
        /// 裝備為 <c>Character/&lt;分類&gt;/&lt;8 位數 ID&gt;.img</c>，其他則為
        /// <c>Item/&lt;Type&gt;/&lt;4 位數&gt;.img/&lt;8 位數 ID&gt;</c>。
        /// </summary>
        private static Wz_Node FindItemImageNode(int itemID)
        {
            Wz_Node node;
            if (itemID < 2000000)
            {
                // Unity 資料集可直接解析 <c>Character/&lt;8 位數 ID&gt;</c>。
                node = PluginManager.FindWz(string.Format(@"Character\{0:D8}", itemID))
                    ?? FindGearImageNodeInCategories(itemID);
            }
            else
            {
                string itemType = Item.GetItemType(itemID).ToString();
                string path = (itemID / 10000 == 500)
                    ? string.Format(@"Item\{0}\{1:D7}.img", itemType, itemID)
                    : string.Format(@"Item\{0}\{1:D4}.img\{2:D8}", itemType, itemID / 10000, itemID);
                node = PluginManager.FindWz(path) ?? PluginManager.FindWz(string.Format(@"Item\{0}", itemID));
            }

            // 樹狀結構上的 img 節點本身沒有內容，因此展開後傳回內容節點。
            Wz_Image image = node?.GetValue<Wz_Image>();
            if (image != null)
            {
                return image.TryExtract() ? image.Node : null;
            }
            return node;
        }

        /// <summary>
        /// 從分類目錄尋找 WZ 的 <c>Character/&lt;分類&gt;/&lt;8 位數 ID&gt;.img</c>。
        /// </summary>
        private static Wz_Node FindGearImageNodeInCategories(int itemID)
        {
            string idName = itemID.ToString("D8");
            Wz_Node characterNode = PluginManager.FindWz(Wz_Type.Character);
            foreach (Wz_Node category in characterNode?.Nodes ?? Enumerable.Empty<Wz_Node>())
            {
                if (category.Text.IndexOf("canvas", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    continue;
                }
                if (category.Text == idName || category.Text == idName + ".img")
                {
                    return category;
                }
                Wz_Node found = category.Nodes[idName + ".img"] ?? category.Nodes[idName];
                if (found != null)
                {
                    return found;
                }
            }
            return null;
        }

        private static Bitmap LoadItemIcon(int itemID)
        {
            Wz_Node imageNode = FindItemImageNode(itemID);
            Wz_Node infoNode = imageNode?.FindNodeByPath("info");
            Wz_Node iconNode = infoNode?.FindNodeByPath("iconRaw") ?? infoNode?.FindNodeByPath("icon");
            Bitmap source = BitmapOrigin.CreateFromNode(iconNode, PluginManager.FindWz).Bitmap;
            try
            {
                // 道具圖示不放大，未超過目標大小時以原尺寸顯示。
                return CreateScaledMobBitmap(source, MobIconSize, false);
            }
            finally
            {
                source?.Dispose();
            }
        }

        private void NavigateToMobNode(MobListItem item)
        {
            if (this._mainForm == null)
            {
                return;
            }

            Wz_Node mobNode = PluginManager.FindWz(string.Format(@"Mob\{0:D7}.img", item.MobID));
            if (mobNode != null)
            {
                this._mainForm.RedirectToNode(mobNode);
            }
            else
            {
                ToastNotification.Show(this, "找不到對應的怪物節點。", null, 2000, eToastGlowColor.Red, eToastPosition.TopCenter);
            }
        }

        private void ApplyMobFilter()
        {
            string keyword = Translator.NormalizeForSearch(txtSearch.Text.Trim());

            itemPanelMobList.BeginUpdate();
            foreach (MobListItem item in this.mobItems)
            {
                item.Visible = string.IsNullOrEmpty(keyword) || IsMobMatch(item, keyword);
            }
            itemPanelMobList.EndUpdate();
        }

        private bool IsMobMatch(MobListItem item, string keyword)
        {
            string name = Translator.NormalizeForSearch(item.MobName);
            if (name.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }

            string paddedID = item.MobID.ToString("D7");
            if (paddedID.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0
                || item.MobID.ToString().IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }

            // 讓掉落道具名稱、出現地圖名稱也能搜尋。
            return this.GetMobSearchText(item.MobID).IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        /// <summary>
        /// 將怪物相關的搜尋用文字（掉落道具名稱 + 出現地圖名稱）正規化後傳回。
        /// 結果會快取，並在清單重建時或地圖索引完成時捨棄。
        /// </summary>
        private string GetMobSearchText(int mobID)
        {
            string cached;
            if (this.mobSearchTextCache.TryGetValue(mobID, out cached))
            {
                return cached;
            }

            StringBuilder sb = new StringBuilder();

            Wz_Node rewardNode = this.MonsterBookNode?.FindNodeByPath(mobID.ToString())?.FindNodeByPath("reward");
            if (rewardNode != null)
            {
                foreach (Wz_Node reward in rewardNode.Nodes)
                {
                    int itemID;
                    string itemName;
                    if (int.TryParse(reward.Value?.ToString(), out itemID) && this.TryGetItemName(itemID, out itemName))
                    {
                        sb.Append(itemName).Append('\n');
                    }
                }
            }

            Dictionary<int, List<MobMapSpawnEntry>> index;
            lock (mobToMapIndexLock)
            {
                index = mobToMapIndex;
            }
            List<int> nodeMaps = this.GetMonsterBookMapIDs(mobID);
            if (nodeMaps != null)
            {
                foreach (int mapID in nodeMaps)
                {
                    string mapName;
                    if (this.TryGetMapName(mapID, out mapName))
                    {
                        sb.Append(mapName).Append('\n');
                    }
                }
            }
            else if (index != null && index.TryGetValue(mobID, out List<MobMapSpawnEntry> spawns))
            {
                foreach (MobMapSpawnEntry spawn in spawns)
                {
                    string mapName;
                    if (this.TryGetMapName(spawn.MapID, out mapName))
                    {
                        sb.Append(mapName).Append('\n');
                    }
                }
            }

            string text = Translator.NormalizeForSearch(sb.ToString());
            this.mobSearchTextCache[mobID] = text;
            return text;
        }

        private const int MobIconSize = 48;
        private const int MobAnimationSize = 256;
        private const float LoadingAnimationScale = 2f;

        private static Bitmap LoadMobIcon(int mobID)
        {
            Wz_Node iconNode = PluginManager.FindWz(string.Format(@"Mob\{0:D7}.img\stand\0", mobID));
            Bitmap source = BitmapOrigin.CreateFromNode(iconNode, PluginManager.FindWz).Bitmap;
            try
            {
                return CreateScaledMobBitmap(source, MobIconSize);
            }
            finally
            {
                source?.Dispose();
            }
        }

        private static Bitmap CreateScaledMobBitmap(Bitmap source, int targetSize, bool allowIntegerUpscale = true)
        {
            Bitmap result = new Bitmap(targetSize, targetSize, PixelFormat.Format32bppArgb);
            if (source == null || source.Width <= 0 || source.Height <= 0)
            {
                return result;
            }

            Rectangle sourceBounds = new Rectangle(0, 0, source.Width, source.Height);
            float scale = GetMobScaleFactor(Math.Max(source.Width, source.Height), targetSize, allowIntegerUpscale, out InterpolationMode interpolationMode);
            using (Graphics g = Graphics.FromImage(result))
            {
                g.InterpolationMode = interpolationMode;
                g.PixelOffsetMode = interpolationMode == InterpolationMode.NearestNeighbor
                    ? PixelOffsetMode.Half
                    : PixelOffsetMode.HighQuality;

                RectangleF dest = new RectangleF(
                    (targetSize - sourceBounds.Width * scale) / 2f,
                    (targetSize - sourceBounds.Height * scale) / 2f,
                    sourceBounds.Width * scale,
                    sourceBounds.Height * scale);
                if (interpolationMode == InterpolationMode.NearestNeighbor)
                {
                    g.DrawImage(source, new Rectangle(
                        (int)Math.Round(dest.X), (int)Math.Round(dest.Y),
                        Math.Max(1, (int)Math.Round(dest.Width)), Math.Max(1, (int)Math.Round(dest.Height))));
                }
                else
                {
                    g.DrawImage(source, dest);
                }
            }

            return result;
        }

        /// <summary>
        /// 圖示／動畫共用的縮放比例。
        /// 未達一半大小時以整數倍放大、未超過目標大小時以原尺寸顯示、超過時縮小至目標大小。
        /// <paramref name="allowIntegerUpscale"/> 為 false 時不放大（供道具圖示使用）。
        /// </summary>
        private static float GetMobScaleFactor(int maxDimension, int targetSize, bool allowIntegerUpscale, out InterpolationMode interpolationMode)
        {
            interpolationMode = InterpolationMode.NearestNeighbor;
            if (maxDimension <= 0)
            {
                return 1f;
            }
            if (maxDimension < targetSize / 2)
            {
                return allowIntegerUpscale ? Math.Max(1, targetSize / maxDimension) : 1f;
            }
            if (maxDimension > targetSize)
            {
                interpolationMode = InterpolationMode.HighQualityBicubic;
                return (float)targetSize / maxDimension;
            }
            return 1f;
        }

        private static Bitmap RenderMobAnimationFrame(
            Bitmap source, Rectangle sourceBounds, Rectangle bounds,
            float scale, InterpolationMode interpolationMode, int targetSize)
        {
            Bitmap result = new Bitmap(targetSize, targetSize, PixelFormat.Format32bppArgb);
            if (source == null)
            {
                return result;
            }

            float offsetX = (targetSize - bounds.Width * scale) / 2f;
            float offsetY = (targetSize - bounds.Height * scale) / 2f;
            RectangleF dest = new RectangleF(
                offsetX + (sourceBounds.X - bounds.X) * scale,
                offsetY + (sourceBounds.Y - bounds.Y) * scale,
                sourceBounds.Width * scale,
                sourceBounds.Height * scale);

            using (Graphics g = Graphics.FromImage(result))
            {
                g.InterpolationMode = interpolationMode;
                g.PixelOffsetMode = interpolationMode == InterpolationMode.NearestNeighbor
                    ? PixelOffsetMode.Half
                    : PixelOffsetMode.HighQuality;
                if (interpolationMode == InterpolationMode.NearestNeighbor)
                {
                    g.DrawImage(source, new Rectangle(
                        (int)Math.Round(dest.X), (int)Math.Round(dest.Y),
                        Math.Max(1, (int)Math.Round(dest.Width)), Math.Max(1, (int)Math.Round(dest.Height))));
                }
                else
                {
                    g.DrawImage(source, dest);
                }
            }

            return result;
        }

        /// <summary>左欄怪物清單的一列（圖示 + 名稱 + ID）。</summary>
        private sealed class MobListItem : ButtonItem
        {
            public MobListItem(int mobID, string mobName)
            {
                this.MobID = mobID;
                this.MobName = mobName ?? string.Empty;

                this.ButtonStyle = eButtonStyle.ImageAndText;
                this.ImageFixedSize = new Size(MobIconSize, MobIconSize);
                this.AutoDisposeImages = true;
                this.EnableMarkup = true;
                this.Text = string.Format("{0}<br/><font color=\"#808080\">{1}</font>",
                    EscapeMarkup(this.MobName), mobID);
                this.Tooltip = string.Format("{0} ({1})", this.MobName, mobID);
                this.Tag = mobID;
            }

            public int MobID { get; private set; }

            public string MobName { get; private set; }

            public void SetIcon(Bitmap icon)
            {
                if (this.Image != null)
                {
                    this.Image.Dispose();
                    this.Image = null;
                }
                this.Image = icon;
            }
        }

        /// <summary>下段地圖清單的一列（標記圖示 + 地圖名稱 + ID）。</summary>
        private sealed class MapListItem : ButtonItem
        {
            public MapListItem(int mapID, string mapName)
            {
                this.MapID = mapID;
                this.MapName = mapName ?? string.Empty;

                this.ButtonStyle = eButtonStyle.ImageAndText;
                this.ImageFixedSize = new Size(MobIconSize, MobIconSize);
                this.AutoDisposeImages = true;
                this.EnableMarkup = true;
                this.Text = string.Format("{0}<br/><font color=\"#808080\">{1}</font>",
                    EscapeMarkup(this.MapName), mapID);
                this.Tooltip = string.Format("{0} ({1})", this.MapName, mapID);
                this.Tag = mapID;
            }

            public int MapID { get; private set; }

            public string MapName { get; private set; }

            public void SetIcon(Bitmap icon)
            {
                if (this.Image != null)
                {
                    this.Image.Dispose();
                    this.Image = null;
                }
                this.Image = icon;
            }
        }

        /// <summary>下段道具清單的一列（iconRaw + 道具名稱 + ID）。</summary>
        private sealed class ItemListItem : ButtonItem
        {
            public ItemListItem(int itemID, string itemName)
            {
                this.ItemID = itemID;
                this.ItemName = itemName ?? string.Empty;

                this.ButtonStyle = eButtonStyle.ImageAndText;
                this.ImageFixedSize = new Size(MobIconSize, MobIconSize);
                this.AutoDisposeImages = true;
                this.EnableMarkup = true;
                this.Text = string.Format("{0}<br/><font color=\"#808080\">{1}</font>",
                    EscapeMarkup(this.ItemName), itemID);
                this.Tooltip = string.Format("{0} ({1})", this.ItemName, itemID);
                this.Tag = itemID;
            }

            public int ItemID { get; private set; }

            public string ItemName { get; private set; }

            public void SetIcon(Bitmap icon)
            {
                if (this.Image != null)
                {
                    this.Image.Dispose();
                    this.Image = null;
                }
                this.Image = icon;
            }
        }

        private static string EscapeMarkup(string text)
        {
            return SecurityElement.Escape(text) ?? string.Empty;
        }
    }
}
