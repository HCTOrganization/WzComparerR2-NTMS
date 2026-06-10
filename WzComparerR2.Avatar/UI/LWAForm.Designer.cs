using DevComponents.DotNetBar;
using System.Windows.Forms;

namespace WzComparerR2.Avatar.UI
{
    partial class LWAForm
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
            this.tableLayout = new System.Windows.Forms.TableLayoutPanel();
            this.picPreview = new System.Windows.Forms.PictureBox();
            this.picQR = new System.Windows.Forms.PictureBox();
            this.panelIGN = new System.Windows.Forms.TableLayoutPanel();
            this.labelIGN = new DevComponents.DotNetBar.LabelX();
            this.txtIGN = new DevComponents.DotNetBar.Controls.TextBoxX();
            this.btnGenerate = new DevComponents.DotNetBar.ButtonX();
            this.btnSaveQR = new DevComponents.DotNetBar.ButtonX();
            this.tableLayout.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picPreview)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picQR)).BeginInit();
            this.panelIGN.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayout
            // 
            this.tableLayout.ColumnCount = 2;
            this.tableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayout.RowCount = 3;
            this.tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 264F));
            this.tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this.tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this.tableLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayout.Padding = new System.Windows.Forms.Padding(4);
            this.tableLayout.Name = "tableLayout";
            this.tableLayout.TabIndex = 0;
            // Row 0, Col 0 – picPreview
            this.tableLayout.Controls.Add(this.picPreview, 0, 0);
            // Row 0, Col 1 – picQR
            this.tableLayout.Controls.Add(this.picQR, 1, 0);
            // Row 1, Col 0 – panelIGN (labelIGN + txtIGN)
            this.tableLayout.Controls.Add(this.panelIGN, 0, 1);
            // Row 1, Col 1 – empty (intentionally unoccupied)
            // Row 2, Col 0 – btnGenerate
            this.tableLayout.Controls.Add(this.btnGenerate, 0, 2);
            // Row 2, Col 1 – btnSaveQR
            this.tableLayout.Controls.Add(this.btnSaveQR, 1, 2);
            // 
            // picPreview
            // 
            this.picPreview.Dock = System.Windows.Forms.DockStyle.Fill;
            this.picPreview.Name = "picPreview";
            this.picPreview.Size = new System.Drawing.Size(256, 256);
            this.picPreview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.picPreview.TabIndex = 0;
            this.picPreview.TabStop = false;
            // 
            // picQR
            // 
            this.picQR.Dock = System.Windows.Forms.DockStyle.Fill;
            this.picQR.Name = "picQR";
            this.picQR.Size = new System.Drawing.Size(256, 256);
            this.picQR.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picQR.TabIndex = 1;
            this.picQR.TabStop = false;
            // 
            // panelIGN  (inner TableLayoutPanel: label auto-width, txtIGN fills remainder)
            // 
            this.panelIGN.AutoSize = true;
            this.panelIGN.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.panelIGN.ColumnCount = 2;
            this.panelIGN.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            this.panelIGN.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.panelIGN.RowCount = 1;
            this.panelIGN.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this.panelIGN.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelIGN.Name = "panelIGN";
            this.panelIGN.TabIndex = 2;
            this.panelIGN.Controls.Add(this.labelIGN, 0, 0);
            this.panelIGN.Controls.Add(this.txtIGN, 1, 0);
            // 
            // labelIGN
            // 
            this.labelIGN.AutoSize = true;
            this.labelIGN.BackColor = System.Drawing.Color.Transparent;
            this.labelIGN.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.labelIGN.Margin = new System.Windows.Forms.Padding(0, 4, 4, 0);
            this.labelIGN.Name = "labelIGN";
            this.labelIGN.Size = new System.Drawing.Size(51, 16);
            this.labelIGN.TabIndex = 3;
            this.labelIGN.Text = "角色名";
            // 
            // txtIGN
            // 
            this.txtIGN.Border.Class = "TextBoxBorder";
            this.txtIGN.Border.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.txtIGN.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtIGN.Name = "txtIGN";
            this.txtIGN.Size = new System.Drawing.Size(180, 21);
            this.txtIGN.TabIndex = 4;
            this.txtIGN.WatermarkText = "WzComparerR2";
            // 
            // btnGenerate
            // 
            this.btnGenerate.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            this.btnGenerate.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            this.btnGenerate.ColorTable = DevComponents.DotNetBar.eButtonColor.OrangeWithBackground;
            this.btnGenerate.Name = "btnGenerate";
            this.btnGenerate.Size = new System.Drawing.Size(55, 23);
            this.btnGenerate.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.btnGenerate.TabIndex = 5;
            this.btnGenerate.Text = "生成";
            this.btnGenerate.Click += new System.EventHandler(this.btnGenerate_Click);
            // 
            // btnSaveQR
            // 
            this.btnSaveQR.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            this.btnSaveQR.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            this.btnSaveQR.ColorTable = DevComponents.DotNetBar.eButtonColor.OrangeWithBackground;
            this.btnSaveQR.Enabled = false;
            this.btnSaveQR.Name = "btnSaveQR";
            this.btnSaveQR.Size = new System.Drawing.Size(55, 23);
            this.btnSaveQR.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.btnSaveQR.TabIndex = 6;
            this.btnSaveQR.Text = "保存";
            this.btnSaveQR.Click += new System.EventHandler(this.btnSaveQR_Click);
            // 
            // LWAForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(525, 330);
            this.Controls.Add(this.tableLayout);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "LWAForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "樂天世界冒險QR生成器";
            this.TopMost = true;
            this.panelIGN.ResumeLayout(false);
            this.panelIGN.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picPreview)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picQR)).EndInit();
            this.tableLayout.ResumeLayout(false);
            this.tableLayout.PerformLayout();
            this.Load += new System.EventHandler(this.LWAForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion
        private System.Windows.Forms.TableLayoutPanel tableLayout;
        private System.Windows.Forms.TableLayoutPanel panelIGN;
        private DevComponents.DotNetBar.ButtonX btnSaveQR;
        private DevComponents.DotNetBar.ButtonX btnGenerate;
        private DevComponents.DotNetBar.Controls.TextBoxX txtIGN;
        private DevComponents.DotNetBar.LabelX labelIGN;
        private System.Windows.Forms.PictureBox picQR;
        private System.Windows.Forms.PictureBox picPreview;
    }
}
