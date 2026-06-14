using System.Drawing;

namespace WzComparerR2
{
    partial class FrmWorldArchiveBrowser
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
            this.cmbRegion = new DevComponents.DotNetBar.Controls.ComboBoxEx();
            this.cmbType = new DevComponents.DotNetBar.Controls.ComboBoxEx();
            this.advTreeLife = new DevComponents.AdvTree.AdvTree();
            this.elementStyle1 = new DevComponents.DotNetBar.ElementStyle();
            this.advTreeMap = new DevComponents.AdvTree.AdvTree();
            this.picWorldArchiveImg = new System.Windows.Forms.PictureBox();
            this.richDescription = new DevComponents.DotNetBar.Controls.RichTextBoxEx();
            this.btnExport = new DevComponents.DotNetBar.ButtonX();
            this.btnLocateExtraIllust = new DevComponents.DotNetBar.ButtonX();
            this.btnCopyMapleStoryWikiFormat = new DevComponents.DotNetBar.ButtonX();
            this.btnTranslate = new DevComponents.DotNetBar.ButtonX();
            this.mainGrid = new System.Windows.Forms.TableLayoutPanel();
            this.tlpColumn0 = new System.Windows.Forms.TableLayoutPanel();
            this.tlpColumn1 = new System.Windows.Forms.TableLayoutPanel();
            this.tlpColumn2 = new System.Windows.Forms.TableLayoutPanel();
            this.pnlButtonsRow2 = new System.Windows.Forms.Panel();
            this.pnlButtonsRow3 = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.advTreeLife)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.advTreeMap)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picWorldArchiveImg)).BeginInit();
            this.mainGrid.SuspendLayout();
            this.tlpColumn0.SuspendLayout();
            this.tlpColumn1.SuspendLayout();
            this.tlpColumn2.SuspendLayout();
            this.pnlButtonsRow2.SuspendLayout();
            this.pnlButtonsRow3.SuspendLayout();
            this.SuspendLayout();
            // 
            // mainGrid - 3 columns main layout
            // 
            this.mainGrid.ColumnCount = 3;
            this.mainGrid.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.mainGrid.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.mainGrid.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 330F));
            this.mainGrid.Controls.Add(this.tlpColumn0, 0, 0);
            this.mainGrid.Controls.Add(this.tlpColumn1, 1, 0);
            this.mainGrid.Controls.Add(this.tlpColumn2, 2, 0);
            this.mainGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainGrid.Location = new System.Drawing.Point(0, 0);
            this.mainGrid.Name = "mainGrid";
            this.mainGrid.RowCount = 1;
            this.mainGrid.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.mainGrid.Size = new System.Drawing.Size(810, 770);
            this.mainGrid.TabIndex = 10;
            // 
            // tlpColumn0 - Column 0 (cmbRegion, advTreeMap) - 2 rows
            // 
            this.tlpColumn0.ColumnCount = 1;
            this.tlpColumn0.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpColumn0.Controls.Add(this.cmbRegion, 0, 0);
            this.tlpColumn0.Controls.Add(this.advTreeMap, 0, 1);
            this.tlpColumn0.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpColumn0.Location = new System.Drawing.Point(0, 0);
            this.tlpColumn0.Name = "tlpColumn0";
            this.tlpColumn0.RowCount = 2;
            this.tlpColumn0.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tlpColumn0.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpColumn0.Size = new System.Drawing.Size(225, 770);
            this.tlpColumn0.TabIndex = 0;
            // 
            // tlpColumn1 - Column 1 (cmbType, advTreeLife) - 2 rows
            // 
            this.tlpColumn1.ColumnCount = 1;
            this.tlpColumn1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpColumn1.Controls.Add(this.cmbType, 0, 0);
            this.tlpColumn1.Controls.Add(this.advTreeLife, 0, 1);
            this.tlpColumn1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpColumn1.Location = new System.Drawing.Point(225, 0);
            this.tlpColumn1.Name = "tlpColumn1";
            this.tlpColumn1.RowCount = 2;
            this.tlpColumn1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tlpColumn1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpColumn1.Size = new System.Drawing.Size(225, 770);
            this.tlpColumn1.TabIndex = 1;
            // 
            // tlpColumn2 - Column 2 (image, description, buttons) - 4 rows
            // 
            this.tlpColumn2.ColumnCount = 1;
            this.tlpColumn2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpColumn2.Controls.Add(this.picWorldArchiveImg, 0, 0);
            this.tlpColumn2.Controls.Add(this.richDescription, 0, 1);
            this.tlpColumn2.Controls.Add(this.pnlButtonsRow2, 0, 2);
            this.tlpColumn2.Controls.Add(this.pnlButtonsRow3, 0, 3);
            this.tlpColumn2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpColumn2.Location = new System.Drawing.Point(450, 0);
            this.tlpColumn2.Name = "tlpColumn2";
            this.tlpColumn2.RowCount = 4;
            this.tlpColumn2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 320F));
            this.tlpColumn2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpColumn2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tlpColumn2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tlpColumn2.Size = new System.Drawing.Size(330, 770);
            this.tlpColumn2.TabIndex = 2;
            // 
            // pnlButtonsRow2
            // 
            this.pnlButtonsRow2.Controls.Add(this.btnTranslate);
            this.pnlButtonsRow2.Controls.Add(this.btnCopyMapleStoryWikiFormat);
            this.pnlButtonsRow2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlButtonsRow2.Location = new System.Drawing.Point(0, 0);
            this.pnlButtonsRow2.Name = "pnlButtonsRow2";
            this.pnlButtonsRow2.Size = new System.Drawing.Size(330, 30);
            this.pnlButtonsRow2.TabIndex = 10;
            // 
            // pnlButtonsRow3
            // 
            this.pnlButtonsRow3.Controls.Add(this.btnLocateExtraIllust);
            this.pnlButtonsRow3.Controls.Add(this.btnExport);
            this.pnlButtonsRow3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlButtonsRow3.Location = new System.Drawing.Point(0, 0);
            this.pnlButtonsRow3.Name = "pnlButtonsRow3";
            this.pnlButtonsRow3.Size = new System.Drawing.Size(330, 30);
            this.pnlButtonsRow3.TabIndex = 11;
            // 
            // btnExport
            // 
            this.btnExport.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            this.btnExport.ColorTable = DevComponents.DotNetBar.eButtonColor.OrangeWithBackground;
            this.btnExport.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnExport.Name = "btnExport";
            this.btnExport.Size = new System.Drawing.Size(155, 23);
            this.btnExport.Margin = new System.Windows.Forms.Padding(5, 0, 0, 0);
            this.btnExport.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.btnExport.TabIndex = 9;
            this.btnExport.Text = "匯出";
            this.btnExport.Click += new System.EventHandler(this.btnExport_Click);
            // 
            // btnLocateExtraIllust
            // 
            this.btnLocateExtraIllust.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            this.btnLocateExtraIllust.ColorTable = DevComponents.DotNetBar.eButtonColor.OrangeWithBackground;
            this.btnLocateExtraIllust.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnLocateExtraIllust.Name = "btnLocateExtraIllust";
            this.btnLocateExtraIllust.Size = new System.Drawing.Size(155, 23);
            this.btnLocateExtraIllust.Margin = new System.Windows.Forms.Padding(0, 0, 5, 0);
            this.btnLocateExtraIllust.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.btnLocateExtraIllust.TabIndex = 8;
            this.btnLocateExtraIllust.Text = "查看追加繪圖";
            this.btnLocateExtraIllust.Click += new System.EventHandler(this.btnLocateExtraIllust_Click);
            this.btnLocateExtraIllust.Enabled = false;
            // 
            // btnCopyMapleStoryWikiFormat
            // 
            this.btnCopyMapleStoryWikiFormat.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            this.btnCopyMapleStoryWikiFormat.ColorTable = DevComponents.DotNetBar.eButtonColor.OrangeWithBackground;
            this.btnCopyMapleStoryWikiFormat.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnCopyMapleStoryWikiFormat.Name = "btnCopyMapleStoryWikiFormat";
            this.btnCopyMapleStoryWikiFormat.Size = new System.Drawing.Size(155, 23);
            this.btnCopyMapleStoryWikiFormat.Margin = new System.Windows.Forms.Padding(5, 0, 0, 0);
            this.btnCopyMapleStoryWikiFormat.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.btnCopyMapleStoryWikiFormat.TabIndex = 7;
            this.btnCopyMapleStoryWikiFormat.Text = "複製 MapleWiki 格式塊";
            this.btnCopyMapleStoryWikiFormat.Click += new System.EventHandler(this.btnCopyMapleStoryWikiFormat_Click);
            // 
            // btnTranslate
            // 
            this.btnTranslate.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            this.btnTranslate.ColorTable = DevComponents.DotNetBar.eButtonColor.OrangeWithBackground;
            this.btnTranslate.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnTranslate.Name = "btnTranslate";
            this.btnTranslate.Size = new System.Drawing.Size(155, 23);
            this.btnTranslate.Margin = new System.Windows.Forms.Padding(0, 0, 5, 0);
            this.btnTranslate.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.btnTranslate.TabIndex = 6;
            this.btnTranslate.Text = "翻譯";
            this.btnTranslate.Click += new System.EventHandler(this.btnTranslate_Click);
            // 
            // cmbRegion
            // 
            this.cmbRegion.DisplayMember = "Text";
            this.cmbRegion.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.cmbRegion.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbRegion.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmbRegion.FormattingEnabled = true;
            this.cmbRegion.ItemHeight = 13;
            this.cmbRegion.Name = "cmbRegion";
            this.cmbRegion.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmbRegion.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.cmbRegion.TabIndex = 0;
            this.cmbRegion.WatermarkColor = System.Drawing.Color.Gray;
            this.cmbRegion.WatermarkText = "世界";
            this.cmbRegion.SelectedValueChanged += new System.EventHandler(this.cmbRegion_SelectedValueChanged);
            // 
            // cmbType
            // 
            this.cmbType.DisplayMember = "Text";
            this.cmbType.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.cmbType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbType.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmbType.FormattingEnabled = true;
            this.cmbType.ItemHeight = 13;
            this.cmbType.Name = "cmbType";
            this.cmbType.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmbType.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.cmbType.TabIndex = 2;
            this.cmbType.WatermarkColor = System.Drawing.Color.Gray;
            this.cmbType.WatermarkText = "類別";
            this.cmbType.SelectedValueChanged += new System.EventHandler(this.cmbType_SelectedValueChanged);
            // 
            // advTreeLife
            // 
            this.advTreeLife.AccessibleRole = System.Windows.Forms.AccessibleRole.Outline;
            this.advTreeLife.AllowDrop = true;
            this.advTreeLife.BackColor = System.Drawing.SystemColors.Window;
            // 
            // 
            // 
            this.advTreeLife.BackgroundStyle.Class = "TreeBorderKey";
            this.advTreeLife.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.advTreeLife.DoubleClickTogglesNode = false;
            this.advTreeLife.DragDropEnabled = false;
            this.advTreeLife.DragDropNodeCopyEnabled = false;
            this.advTreeLife.ExpandWidth = 4;
            this.advTreeLife.HideSelection = true;
            this.advTreeLife.Dock = System.Windows.Forms.DockStyle.Fill;
            this.advTreeLife.Name = "advTreeLife";
            this.advTreeLife.NodeStyle = this.elementStyle1;
            this.advTreeLife.PathSeparator = ";";
            this.advTreeLife.TabIndex = 3;
            this.advTreeLife.Text = "advTreeLife";
            this.advTreeLife.AfterNodeSelect += new DevComponents.AdvTree.AdvTreeNodeEventHandler(this.advTreeLife_AfterNodeSelect);
            this.advTreeLife.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.advTreeLife_MouseDoubleClick);
            // 
            // elementStyle1
            // 
            this.elementStyle1.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.elementStyle1.Name = "elementStyle1";
            this.elementStyle1.TextColor = System.Drawing.SystemColors.ControlText;
            // 
            // advTreeMap
            // 
            this.advTreeMap.AccessibleRole = System.Windows.Forms.AccessibleRole.Outline;
            this.advTreeMap.AllowDrop = true;
            this.advTreeMap.BackColor = System.Drawing.SystemColors.Window;
            // 
            // 
            // 
            this.advTreeMap.BackgroundStyle.Class = "TreeBorderKey";
            this.advTreeMap.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.advTreeMap.DoubleClickTogglesNode = false;
            this.advTreeMap.DragDropEnabled = false;
            this.advTreeMap.DragDropNodeCopyEnabled = false;
            this.advTreeMap.ExpandWidth = 4;
            this.advTreeMap.HideSelection = true;
            this.advTreeMap.Dock = System.Windows.Forms.DockStyle.Fill;
            this.advTreeMap.Name = "advTreeMap";
            this.advTreeMap.NodeStyle = this.elementStyle1;
            this.advTreeMap.PathSeparator = ";";
            this.advTreeMap.Styles.Add(this.elementStyle1);
            this.advTreeMap.TabIndex = 1;
            this.advTreeMap.Text = "advTreeMap";
            this.advTreeMap.AfterNodeSelect += new DevComponents.AdvTree.AdvTreeNodeEventHandler(this.advTreeMap_AfterNodeSelect);
            // 
            // picWorldArchiveImg
            // 
            this.picWorldArchiveImg.Dock = System.Windows.Forms.DockStyle.Fill;
            this.picWorldArchiveImg.Name = "picWorldArchiveImg";
            this.picWorldArchiveImg.Size = new System.Drawing.Size(320, 320);
            this.picWorldArchiveImg.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.picWorldArchiveImg.TabIndex = 4;
            this.picWorldArchiveImg.TabStop = false;
            // 
            // richDescription
            // 
            this.richDescription.BackgroundStyle.Class = "RichTextBoxBorder";
            this.richDescription.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.richDescription.Dock = System.Windows.Forms.DockStyle.Fill;
            this.richDescription.Name = "richDescription";
            this.richDescription.Rtf = "{\\rtf1\\ansi\\ansicpg936\\deff0\\deflang1033\\deflangfe1042{\\fonttbl{\\f0\\fnil\\fcharset" +
    "129 \\\'b5\\\'b8\\\'bf\\\'f2;}}\r\n\\viewkind4\\uc1\\pard\\lang1042\\f0\\fs18\\par\r\n}\r\n";
            this.richDescription.TabIndex = 5;
            this.richDescription.ReadOnly = true;
            // 
            // FrmWorldArchiveBrowser
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(810, 770);
            this.Controls.Add(this.mainGrid);
            this.DoubleBuffered = true;
            this.Font = new System.Drawing.Font("SimSun", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(500, 500);
            this.Name = "FrmWorldArchiveBrowser";
            this.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.Text = "世界檔案";
            this.TopMost = true;
            this.tlpColumn0.ResumeLayout(false);
            this.tlpColumn1.ResumeLayout(false);
            this.tlpColumn2.ResumeLayout(false);
            this.pnlButtonsRow2.ResumeLayout(false);
            this.pnlButtonsRow3.ResumeLayout(false);
            this.mainGrid.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.advTreeLife)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.advTreeMap)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picWorldArchiveImg)).EndInit();
            this.ResumeLayout(false);
        }

        #endregion
        private DevComponents.DotNetBar.Controls.ComboBoxEx cmbRegion;
        private DevComponents.DotNetBar.Controls.ComboBoxEx cmbType;
        private DevComponents.AdvTree.AdvTree advTreeLife;
        private DevComponents.AdvTree.AdvTree advTreeMap;
        private DevComponents.DotNetBar.ElementStyle elementStyle1;
        private System.Windows.Forms.PictureBox picWorldArchiveImg;
        private DevComponents.DotNetBar.Controls.RichTextBoxEx richDescription;
        private DevComponents.DotNetBar.ButtonX btnExport;
        private DevComponents.DotNetBar.ButtonX btnLocateExtraIllust;
        private DevComponents.DotNetBar.ButtonX btnCopyMapleStoryWikiFormat;
        private DevComponents.DotNetBar.ButtonX btnTranslate;
        private System.Windows.Forms.TableLayoutPanel mainGrid;
        private System.Windows.Forms.TableLayoutPanel tlpColumn0;
        private System.Windows.Forms.TableLayoutPanel tlpColumn1;
        private System.Windows.Forms.TableLayoutPanel tlpColumn2;
        private System.Windows.Forms.Panel pnlButtonsRow2;
        private System.Windows.Forms.Panel pnlButtonsRow3;
    }
}