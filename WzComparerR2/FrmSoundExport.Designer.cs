namespace WzComparerR2
{
    partial class FrmSoundExport
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
            this.lblSelectSoundIntro = new DevComponents.DotNetBar.LabelX();
            this.clbSoundImgName = new System.Windows.Forms.CheckedListBox();
            this.btnSelectAll = new DevComponents.DotNetBar.ButtonX();
            this.btnReverseSelect = new DevComponents.DotNetBar.ButtonX();
            this.btnExport = new DevComponents.DotNetBar.ButtonX();
            this.SuspendLayout();
            // 
            // lblSelectSoundIntro
            // 
            this.lblSelectSoundIntro.AutoSize = true;
            // 
            // 
            // 
            this.lblSelectSoundIntro.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.lblSelectSoundIntro.Location = new System.Drawing.Point(10, 7);
            this.lblSelectSoundIntro.Name = "lblSelectSoundIntro";
            this.lblSelectSoundIntro.Size = new System.Drawing.Size(222, 16);
            this.lblSelectSoundIntro.TabIndex = 0;
            this.lblSelectSoundIntro.Text = "請選擇要匯出的音訊檔案。";
            // 
            // clbSoundImgName
            // 
            this.clbSoundImgName.FormattingEnabled = true;
            this.clbSoundImgName.Location = new System.Drawing.Point(10, 30);
            this.clbSoundImgName.Name = "clbSoundImgName";
            this.clbSoundImgName.Font = new System.Drawing.Font("SimSun", 12F);
            this.clbSoundImgName.Size = new System.Drawing.Size(430, 522);
            this.clbSoundImgName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.clbSoundImgName.TabIndex = 1;
            // 
            // 
            // btnSelectAll
            // 
            this.btnSelectAll.Location = new System.Drawing.Point(10, 550);
            this.btnSelectAll.Name = "btnSelectAll";
            this.btnSelectAll.Size = new System.Drawing.Size(100, 35);
            this.btnSelectAll.TabIndex = 3;
            this.btnSelectAll.Text = "全選";
            this.btnSelectAll.ColorTable = DevComponents.DotNetBar.eButtonColor.OrangeWithBackground;
            this.btnSelectAll.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.btnSelectAll.Click += new System.EventHandler(this.btnSelectAll_Click);
            // 
            // btnReverseSelect
            // 
            this.btnReverseSelect.Location = new System.Drawing.Point(174, 550);
            this.btnReverseSelect.Name = "btnReverseSelect";
            this.btnReverseSelect.Size = new System.Drawing.Size(100, 35);
            this.btnReverseSelect.TabIndex = 4;
            this.btnReverseSelect.Text = "反選";
            this.btnReverseSelect.ColorTable = DevComponents.DotNetBar.eButtonColor.OrangeWithBackground;
            this.btnReverseSelect.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.btnReverseSelect.Click += new System.EventHandler(this.btnReverseSelect_Click);
            // 
            // btnExport
            // 
            this.btnExport.Location = new System.Drawing.Point(338, 550);
            this.btnExport.Name = "btnExport";
            this.btnExport.Size = new System.Drawing.Size(100, 35);
            this.btnExport.TabIndex = 5;
            this.btnExport.Text = "匯出";
            this.btnExport.ColorTable = DevComponents.DotNetBar.eButtonColor.OrangeWithBackground;
            this.btnExport.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.btnExport.Click += new System.EventHandler(this.btnExport_Click);
            // 
            // FrmSoundExport
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(450, 600);
            this.Controls.Add(this.btnExport);
            this.Controls.Add(this.btnReverseSelect);
            this.Controls.Add(this.btnSelectAll);
            this.Controls.Add(this.clbSoundImgName);
            this.Controls.Add(this.lblSelectSoundIntro);
            this.DoubleBuffered = true;
            this.Font = new System.Drawing.Font("SimSun", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmSoundExport";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "匯出音訊";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        
        private DevComponents.DotNetBar.LabelX lblSelectSoundIntro;
        private System.Windows.Forms.CheckedListBox clbSoundImgName;
        private DevComponents.DotNetBar.ButtonX btnSelectAll;
        private DevComponents.DotNetBar.ButtonX btnReverseSelect;
        private DevComponents.DotNetBar.ButtonX btnExport;
    }
}