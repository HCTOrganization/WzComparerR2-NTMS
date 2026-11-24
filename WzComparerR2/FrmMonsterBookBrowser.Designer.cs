using System.Drawing;

namespace WzComparerR2
{
    partial class FrmMonsterBookBrowser
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.mainGrid = new System.Windows.Forms.TableLayoutPanel();
            this.pnlLeftColumn = new System.Windows.Forms.Panel();
            this.tlpLeftColumn = new System.Windows.Forms.TableLayoutPanel();
            this.txtSearch = new DevComponents.DotNetBar.Controls.TextBoxX();
            this.itemPanelMobList = new DevComponents.DotNetBar.ItemPanel();
            this.lblLoading = new System.Windows.Forms.Label();
            this.tlpRightArea = new System.Windows.Forms.TableLayoutPanel();
            this.picLoading = new System.Windows.Forms.PictureBox();
            this.pnlTopArea = new System.Windows.Forms.Panel();
            this.tlpMobIntro = new System.Windows.Forms.TableLayoutPanel();
            this.tlpMobImageColumn = new System.Windows.Forms.TableLayoutPanel();
            this.picMobAnimation = new System.Windows.Forms.PictureBox();
            this.cmbAnimationType = new DevComponents.DotNetBar.Controls.ComboBoxEx();
            this.tlpMobStats = new System.Windows.Forms.TableLayoutPanel();
            this.pnlMobStatsScroll = new System.Windows.Forms.Panel();
            this.tlpMobDetails = new System.Windows.Forms.TableLayoutPanel();
            this.tlpMobHeader = new System.Windows.Forms.TableLayoutPanel();
            this.lblMobName = new System.Windows.Forms.Label();
            this.lblMobID = new System.Windows.Forms.Label();
            this.pnlMobRow1 = new System.Windows.Forms.Panel();
            this.richMobDescription = new DevComponents.DotNetBar.Controls.RichTextBoxEx();
            this.tlpBottomArea = new System.Windows.Forms.TableLayoutPanel();
            this.pnlBottomLeft = new System.Windows.Forms.Panel();
            this.itemPanelMapList = new DevComponents.DotNetBar.ItemPanel();
            this.pnlBottomRight = new System.Windows.Forms.Panel();
            this.itemPanelItemList = new DevComponents.DotNetBar.ItemPanel();
            this.mainGrid.SuspendLayout();
            this.pnlLeftColumn.SuspendLayout();
            this.tlpLeftColumn.SuspendLayout();
            this.tlpRightArea.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picLoading)).BeginInit();
            this.pnlTopArea.SuspendLayout();
            this.tlpMobIntro.SuspendLayout();
            this.tlpMobImageColumn.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picMobAnimation)).BeginInit();
            this.tlpMobDetails.SuspendLayout();
            this.tlpMobHeader.SuspendLayout();
            this.tlpBottomArea.SuspendLayout();
            this.SuspendLayout();
            // 
            // mainGrid - 2 columns: fixed 300px left column + remaining right column
            // 
            this.mainGrid.ColumnCount = 2;
            this.mainGrid.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 300F));
            this.mainGrid.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.mainGrid.Controls.Add(this.pnlLeftColumn, 0, 0);
            this.mainGrid.Controls.Add(this.tlpRightArea, 1, 0);
            this.mainGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainGrid.Location = new System.Drawing.Point(0, 0);
            this.mainGrid.Margin = new System.Windows.Forms.Padding(0);
            this.mainGrid.Name = "mainGrid";
            this.mainGrid.RowCount = 1;
            this.mainGrid.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.mainGrid.Size = new System.Drawing.Size(1280, 720);
            this.mainGrid.TabIndex = 0;
            // 
            // pnlLeftColumn - column 0 preview
            // 
            this.pnlLeftColumn.Controls.Add(this.tlpLeftColumn);
            this.pnlLeftColumn.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlLeftColumn.Location = new System.Drawing.Point(8, 8);
            this.pnlLeftColumn.Margin = new System.Windows.Forms.Padding(8, 8, 4, 8);
            this.pnlLeftColumn.Name = "pnlLeftColumn";
            this.pnlLeftColumn.Size = new System.Drawing.Size(288, 704);
            this.pnlLeftColumn.TabIndex = 0;
            // 
            // tlpLeftColumn - search box (fixed 32px) + item panel, 8px apart
            // 
            this.tlpLeftColumn.ColumnCount = 1;
            this.tlpLeftColumn.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpLeftColumn.Controls.Add(this.txtSearch, 0, 0);
            this.tlpLeftColumn.Controls.Add(this.itemPanelMobList, 0, 1);
            this.tlpLeftColumn.Controls.Add(this.lblLoading, 0, 1);
            this.tlpLeftColumn.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpLeftColumn.Location = new System.Drawing.Point(0, 0);
            this.tlpLeftColumn.Margin = new System.Windows.Forms.Padding(0);
            this.tlpLeftColumn.Name = "tlpLeftColumn";
            this.tlpLeftColumn.RowCount = 2;
            this.tlpLeftColumn.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tlpLeftColumn.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpLeftColumn.Size = new System.Drawing.Size(288, 704);
            this.tlpLeftColumn.TabIndex = 0;
            // 
            // txtSearch
            // 
            this.txtSearch.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtSearch.Font = new System.Drawing.Font("Yu Gothic UI", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtSearch.Location = new System.Drawing.Point(0, 0);
            this.txtSearch.Margin = new System.Windows.Forms.Padding(0, 0, 0, 8);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.txtSearch.Size = new System.Drawing.Size(288, 32);
            this.txtSearch.TabIndex = 0;
            this.txtSearch.WordWrap = false;
            this.txtSearch.TextChanged += new System.EventHandler(this.txtSearch_TextChanged);
            this.txtSearch.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtSearch_KeyDown);
            // 
            // itemPanelMobList
            // 
            this.itemPanelMobList.AutoScroll = true;
            this.itemPanelMobList.BackgroundStyle.Class = "ItemPanel";
            this.itemPanelMobList.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.itemPanelMobList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.itemPanelMobList.FitButtonsToContainerWidth = true;
            this.itemPanelMobList.LayoutOrientation = DevComponents.DotNetBar.eOrientation.Vertical;
            this.itemPanelMobList.Location = new System.Drawing.Point(0, 40);
            this.itemPanelMobList.Margin = new System.Windows.Forms.Padding(0);
            this.itemPanelMobList.MultiLine = false;
            this.itemPanelMobList.Name = "itemPanelMobList";
            this.itemPanelMobList.Size = new System.Drawing.Size(288, 664);
            this.itemPanelMobList.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.itemPanelMobList.TabIndex = 1;
            this.itemPanelMobList.Text = "itemPanelMobList";
            this.itemPanelMobList.MouseClick += new System.Windows.Forms.MouseEventHandler(this.itemPanelMobList_MouseClick);
            this.itemPanelMobList.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.itemPanelMobList_MouseDoubleClick);
            // 
            // lblLoading
            // 
            this.lblLoading.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblLoading.Font = new System.Drawing.Font("Noto Sans TC", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblLoading.Location = new System.Drawing.Point(0, 40);
            this.lblLoading.Name = "lblLoading";
            this.lblLoading.Size = new System.Drawing.Size(288, 664);
            this.lblLoading.TabIndex = 2;
            this.lblLoading.Text = "載入中...";
            this.lblLoading.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblLoading.Visible = false;
            // 
            // tlpRightArea - column 1, 2 rows: 60% top + 40% bottom
            // 
            this.tlpRightArea.ColumnCount = 1;
            this.tlpRightArea.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpRightArea.Controls.Add(this.pnlTopArea, 0, 0);
            this.tlpRightArea.Controls.Add(this.tlpBottomArea, 0, 1);
            this.tlpRightArea.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpRightArea.Location = new System.Drawing.Point(304, 8);
            this.tlpRightArea.Margin = new System.Windows.Forms.Padding(4, 8, 8, 8);
            this.tlpRightArea.Name = "tlpRightArea";
            this.tlpRightArea.RowCount = 2;
            this.tlpRightArea.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 75F));
            this.tlpRightArea.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tlpRightArea.Size = new System.Drawing.Size(968, 704);
            this.tlpRightArea.TabIndex = 1;
            // 
            // picLoading - 載入中的動畫（覆蓋在右側整個區域並置中顯示）
            // 
            this.picLoading.BackColor = System.Drawing.SystemColors.Control;
            this.picLoading.Location = new System.Drawing.Point(304, 8);
            this.picLoading.Margin = new System.Windows.Forms.Padding(0);
            this.picLoading.Name = "picLoading";
            this.picLoading.Size = new System.Drawing.Size(968, 704);
            this.picLoading.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.picLoading.TabIndex = 3;
            this.picLoading.TabStop = false;
            this.picLoading.Visible = false;
            // 
            // pnlTopArea - row 0 preview
            // 
            this.pnlTopArea.Controls.Add(this.tlpMobIntro);
            this.pnlTopArea.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlTopArea.Location = new System.Drawing.Point(0, 0);
            this.pnlTopArea.Margin = new System.Windows.Forms.Padding(0, 0, 0, 4);
            this.pnlTopArea.Name = "pnlTopArea";
            this.pnlTopArea.Size = new System.Drawing.Size(968, 418);
            this.pnlTopArea.TabIndex = 2;
            // 
            // tlpMobIntro - column 0: 256px animation column, column 1: mob details
            // 
            this.tlpMobIntro.ColumnCount = 2;
            this.tlpMobIntro.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 256F));
            this.tlpMobIntro.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMobIntro.Controls.Add(this.tlpMobImageColumn, 0, 0);
            this.tlpMobIntro.Controls.Add(this.tlpMobDetails, 1, 0);
            this.tlpMobIntro.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpMobIntro.Location = new System.Drawing.Point(0, 0);
            this.tlpMobIntro.Margin = new System.Windows.Forms.Padding(0);
            this.tlpMobIntro.Name = "tlpMobIntro";
            this.tlpMobIntro.RowCount = 1;
            this.tlpMobIntro.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMobIntro.Size = new System.Drawing.Size(968, 418);
            this.tlpMobIntro.TabIndex = 0;
            // 
            // tlpMobImageColumn - 256x256 animation image + animation type combo + mob stats
            // 
            this.tlpMobImageColumn.ColumnCount = 1;
            this.tlpMobImageColumn.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMobImageColumn.Controls.Add(this.picMobAnimation, 0, 0);
            this.tlpMobImageColumn.Controls.Add(this.cmbAnimationType, 0, 1);
            this.tlpMobImageColumn.Controls.Add(this.pnlMobStatsScroll, 0, 2);
            this.tlpMobImageColumn.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpMobImageColumn.Location = new System.Drawing.Point(0, 0);
            this.tlpMobImageColumn.Margin = new System.Windows.Forms.Padding(0);
            this.tlpMobImageColumn.Name = "tlpMobImageColumn";
            this.tlpMobImageColumn.RowCount = 3;
            this.tlpMobImageColumn.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 256F));
            this.tlpMobImageColumn.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMobImageColumn.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMobImageColumn.Size = new System.Drawing.Size(256, 418);
            this.tlpMobImageColumn.TabIndex = 0;
            // 
            // picMobAnimation
            // 
            this.picMobAnimation.Location = new System.Drawing.Point(0, 0);
            this.picMobAnimation.Margin = new System.Windows.Forms.Padding(0);
            this.picMobAnimation.Name = "picMobAnimation";
            this.picMobAnimation.Size = new System.Drawing.Size(256, 256);
            this.picMobAnimation.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.picMobAnimation.TabIndex = 0;
            this.picMobAnimation.TabStop = false;
            // 
            // cmbAnimationType
            // 
            this.cmbAnimationType.DisplayMember = "Text";
            this.cmbAnimationType.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmbAnimationType.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.cmbAnimationType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbAnimationType.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmbAnimationType.FormattingEnabled = true;
            this.cmbAnimationType.ItemHeight = 13;
            this.cmbAnimationType.Location = new System.Drawing.Point(0, 264);
            this.cmbAnimationType.Margin = new System.Windows.Forms.Padding(0, 8, 0, 0);
            this.cmbAnimationType.Name = "cmbAnimationType";
            this.cmbAnimationType.Size = new System.Drawing.Size(256, 21);
            this.cmbAnimationType.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.cmbAnimationType.TabIndex = 1;
            this.cmbAnimationType.WatermarkColor = System.Drawing.Color.Gray;
            this.cmbAnimationType.SelectedIndexChanged += new System.EventHandler(this.cmbAnimationType_SelectedIndexChanged);
            this.cmbAnimationType.Visible = false;
            // 
            // pnlMobStatsScroll - scroll host so the table never shows a horizontal scrollbar
            // 
            this.pnlMobStatsScroll.AutoScroll = true;
            this.pnlMobStatsScroll.Controls.Add(this.tlpMobStats);
            this.pnlMobStatsScroll.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlMobStatsScroll.Location = new System.Drawing.Point(0, 293);
            this.pnlMobStatsScroll.Margin = new System.Windows.Forms.Padding(0, 8, 0, 0);
            this.pnlMobStatsScroll.Name = "pnlMobStatsScroll";
            this.pnlMobStatsScroll.Size = new System.Drawing.Size(256, 125);
            this.pnlMobStatsScroll.TabIndex = 2;
            // 
            // tlpMobStats - stat name (left) + value (right aligned), rows built at runtime
            // 
            this.tlpMobStats.AutoSize = true;
            this.tlpMobStats.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpMobStats.ColumnCount = 2;
            this.tlpMobStats.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMobStats.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMobStats.Dock = System.Windows.Forms.DockStyle.Top;
            this.tlpMobStats.Location = new System.Drawing.Point(0, 0);
            this.tlpMobStats.Margin = new System.Windows.Forms.Padding(0);
            this.tlpMobStats.Name = "tlpMobStats";
            this.tlpMobStats.RowCount = 0;
            this.tlpMobStats.Size = new System.Drawing.Size(256, 0);
            this.tlpMobStats.TabIndex = 0;
            // 
            // tlpMobDetails - header (36px) + description filling the rest
            // 
            this.tlpMobDetails.ColumnCount = 1;
            this.tlpMobDetails.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMobDetails.Controls.Add(this.tlpMobHeader, 0, 0);
            this.tlpMobDetails.Controls.Add(this.pnlMobRow1, 0, 1);
            this.tlpMobDetails.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpMobDetails.Location = new System.Drawing.Point(264, 0);
            this.tlpMobDetails.Margin = new System.Windows.Forms.Padding(8, 0, 0, 0);
            this.tlpMobDetails.Name = "tlpMobDetails";
            this.tlpMobDetails.RowCount = 2;
            this.tlpMobDetails.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            this.tlpMobDetails.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMobDetails.Size = new System.Drawing.Size(704, 418);
            this.tlpMobDetails.TabIndex = 1;
            // 
            // tlpMobHeader - mob name + ID
            // 
            this.tlpMobHeader.ColumnCount = 3;
            this.tlpMobHeader.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMobHeader.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMobHeader.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMobHeader.Controls.Add(this.lblMobName, 0, 0);
            this.tlpMobHeader.Controls.Add(this.lblMobID, 1, 0);
            this.tlpMobHeader.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpMobHeader.Location = new System.Drawing.Point(0, 0);
            this.tlpMobHeader.Margin = new System.Windows.Forms.Padding(0);
            this.tlpMobHeader.Name = "tlpMobHeader";
            this.tlpMobHeader.RowCount = 1;
            this.tlpMobHeader.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMobHeader.Size = new System.Drawing.Size(704, 36);
            this.tlpMobHeader.TabIndex = 0;
            // 
            // lblMobName
            // 
            this.lblMobName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblMobName.AutoSize = true;
            this.lblMobName.Font = new System.Drawing.Font("Noto Sans TC", 20F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMobName.Location = new System.Drawing.Point(0, 0);
            this.lblMobName.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);
            this.lblMobName.Name = "lblMobName";
            this.lblMobName.Size = new System.Drawing.Size(0, 33);
            this.lblMobName.TabIndex = 0;
            // 
            // lblMobID
            // 
            this.lblMobID.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblMobID.AutoSize = true;
            this.lblMobID.Font = new System.Drawing.Font("SimSun", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMobID.Location = new System.Drawing.Point(8, 23);
            this.lblMobID.Margin = new System.Windows.Forms.Padding(0);
            this.lblMobID.Name = "lblMobID";
            this.lblMobID.Size = new System.Drawing.Size(0, 12);
            this.lblMobID.TabIndex = 1;
            // 
            // pnlMobRow1
            // 
            this.pnlMobRow1.Controls.Add(this.richMobDescription);
            this.pnlMobRow1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlMobRow1.Location = new System.Drawing.Point(0, 36);
            this.pnlMobRow1.Margin = new System.Windows.Forms.Padding(0);
            this.pnlMobRow1.Name = "pnlMobRow1";
            this.pnlMobRow1.Size = new System.Drawing.Size(704, 382);
            this.pnlMobRow1.TabIndex = 1;
            // 
            // richMobDescription
            // 
            this.richMobDescription.BackColorRichTextBox = System.Drawing.SystemColors.Control;
            this.richMobDescription.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.richMobDescription.Dock = System.Windows.Forms.DockStyle.Fill;
            this.richMobDescription.Font = new System.Drawing.Font("Noto Sans TC", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.richMobDescription.Location = new System.Drawing.Point(0, 0);
            this.richMobDescription.Margin = new System.Windows.Forms.Padding(0);
            this.richMobDescription.Name = "richMobDescription";
            this.richMobDescription.ReadOnly = true;
            this.richMobDescription.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.richMobDescription.Size = new System.Drawing.Size(704, 382);
            this.richMobDescription.TabIndex = 0;
            this.richMobDescription.WordWrap = true;
            // 
            // tlpBottomArea - row 1, 2 columns: 50% + 50%
            // 
            this.tlpBottomArea.ColumnCount = 2;
            this.tlpBottomArea.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpBottomArea.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpBottomArea.Controls.Add(this.pnlBottomLeft, 0, 0);
            this.tlpBottomArea.Controls.Add(this.pnlBottomRight, 1, 0);
            this.tlpBottomArea.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpBottomArea.Location = new System.Drawing.Point(0, 422);
            this.tlpBottomArea.Margin = new System.Windows.Forms.Padding(0, 4, 0, 0);
            this.tlpBottomArea.Name = "tlpBottomArea";
            this.tlpBottomArea.RowCount = 1;
            this.tlpBottomArea.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpBottomArea.Size = new System.Drawing.Size(968, 282);
            this.tlpBottomArea.TabIndex = 3;
            // 
            // pnlBottomLeft - row 1, column 0: maps the selected mob appears in
            // 
            this.pnlBottomLeft.Controls.Add(this.itemPanelMapList);
            this.pnlBottomLeft.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlBottomLeft.Location = new System.Drawing.Point(0, 0);
            this.pnlBottomLeft.Margin = new System.Windows.Forms.Padding(0, 0, 4, 0);
            this.pnlBottomLeft.Name = "pnlBottomLeft";
            this.pnlBottomLeft.Size = new System.Drawing.Size(480, 282);
            this.pnlBottomLeft.TabIndex = 4;
            // 
            // itemPanelMapList
            // 
            this.itemPanelMapList.AutoScroll = true;
            this.itemPanelMapList.BackgroundStyle.Class = "ItemPanel";
            this.itemPanelMapList.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.itemPanelMapList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.itemPanelMapList.FitButtonsToContainerWidth = true;
            this.itemPanelMapList.LayoutOrientation = DevComponents.DotNetBar.eOrientation.Vertical;
            this.itemPanelMapList.Location = new System.Drawing.Point(0, 0);
            this.itemPanelMapList.Margin = new System.Windows.Forms.Padding(0);
            this.itemPanelMapList.MultiLine = false;
            this.itemPanelMapList.Name = "itemPanelMapList";
            this.itemPanelMapList.Size = new System.Drawing.Size(480, 282);
            this.itemPanelMapList.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.itemPanelMapList.TabIndex = 0;
            this.itemPanelMapList.Text = "itemPanelMapList";
            this.itemPanelMapList.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.itemPanelMapList_MouseDoubleClick);
            // 
            // pnlBottomRight - row 1, column 1: items obtainable from the selected mob
            // 
            this.pnlBottomRight.Controls.Add(this.itemPanelItemList);
            this.pnlBottomRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlBottomRight.Location = new System.Drawing.Point(484, 0);
            this.pnlBottomRight.Margin = new System.Windows.Forms.Padding(4, 0, 0, 0);
            this.pnlBottomRight.Name = "pnlBottomRight";
            this.pnlBottomRight.Size = new System.Drawing.Size(484, 282);
            this.pnlBottomRight.TabIndex = 5;
            // 
            // itemPanelItemList
            // 
            this.itemPanelItemList.AutoScroll = true;
            this.itemPanelItemList.BackgroundStyle.Class = "ItemPanel";
            this.itemPanelItemList.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.itemPanelItemList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.itemPanelItemList.FitButtonsToContainerWidth = true;
            this.itemPanelItemList.LayoutOrientation = DevComponents.DotNetBar.eOrientation.Vertical;
            this.itemPanelItemList.Location = new System.Drawing.Point(0, 0);
            this.itemPanelItemList.Margin = new System.Windows.Forms.Padding(0);
            this.itemPanelItemList.MultiLine = false;
            this.itemPanelItemList.Name = "itemPanelItemList";
            this.itemPanelItemList.Size = new System.Drawing.Size(484, 282);
            this.itemPanelItemList.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.itemPanelItemList.TabIndex = 0;
            this.itemPanelItemList.Text = "itemPanelItemList";
            this.itemPanelItemList.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.itemPanelItemList_MouseDoubleClick);
            // 
            // FrmMonsterBookBrowser
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1280, 720);
            this.Controls.Add(this.mainGrid);
            this.Controls.Add(this.picLoading);
            this.DoubleBuffered = true;
            this.Font = new System.Drawing.Font("MS PGothic", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(800, 720);
            this.Name = "FrmMonsterBookBrowser";
            this.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "怪物手冊";
            this.tlpMobHeader.ResumeLayout(false);
            this.tlpMobHeader.PerformLayout();
            this.tlpMobDetails.ResumeLayout(false);
            this.tlpMobStats.ResumeLayout(false);
            this.tlpMobStats.PerformLayout();
            this.pnlMobStatsScroll.ResumeLayout(false);
            this.pnlMobStatsScroll.PerformLayout();
            this.tlpMobImageColumn.ResumeLayout(false);
            this.tlpMobImageColumn.PerformLayout();
            this.tlpMobIntro.ResumeLayout(false);
            this.pnlTopArea.ResumeLayout(false);
            this.tlpBottomArea.ResumeLayout(false);
            this.tlpRightArea.ResumeLayout(false);
            this.tlpLeftColumn.ResumeLayout(false);
            this.pnlLeftColumn.ResumeLayout(false);
            this.mainGrid.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.picMobAnimation)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picLoading)).EndInit();
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel mainGrid;
        private System.Windows.Forms.Panel pnlLeftColumn;
        private System.Windows.Forms.TableLayoutPanel tlpLeftColumn;
        private DevComponents.DotNetBar.Controls.TextBoxX txtSearch;
        private DevComponents.DotNetBar.ItemPanel itemPanelMobList;
        private System.Windows.Forms.Label lblLoading;
        private System.Windows.Forms.TableLayoutPanel tlpRightArea;
        private System.Windows.Forms.PictureBox picLoading;
        private System.Windows.Forms.Panel pnlTopArea;
        private System.Windows.Forms.TableLayoutPanel tlpMobIntro;
        private System.Windows.Forms.TableLayoutPanel tlpMobImageColumn;
        private System.Windows.Forms.PictureBox picMobAnimation;
        private DevComponents.DotNetBar.Controls.ComboBoxEx cmbAnimationType;
        private System.Windows.Forms.TableLayoutPanel tlpMobStats;
        private System.Windows.Forms.Panel pnlMobStatsScroll;
        private System.Windows.Forms.TableLayoutPanel tlpMobDetails;
        private System.Windows.Forms.TableLayoutPanel tlpMobHeader;
        private System.Windows.Forms.Label lblMobName;
        private System.Windows.Forms.Label lblMobID;
        private System.Windows.Forms.Panel pnlMobRow1;
        private DevComponents.DotNetBar.Controls.RichTextBoxEx richMobDescription;
        private System.Windows.Forms.TableLayoutPanel tlpBottomArea;
        private System.Windows.Forms.Panel pnlBottomLeft;
        private DevComponents.DotNetBar.ItemPanel itemPanelMapList;
        private System.Windows.Forms.Panel pnlBottomRight;
        private DevComponents.DotNetBar.ItemPanel itemPanelItemList;
    }
}
