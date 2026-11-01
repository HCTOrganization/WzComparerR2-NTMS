
namespace WzComparerR2
{
    partial class FrmAvatarCode
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
            this.labelXDescription = new DevComponents.DotNetBar.LabelX();
            this.textBoxXCode = new DevComponents.DotNetBar.Controls.TextBoxX();
            this.btnLoadAvatar = new DevComponents.DotNetBar.ButtonX();
            this.btnLoadPreset = new DevComponents.DotNetBar.ButtonItem();
            this.btnImportCharacter = new DevComponents.DotNetBar.ButtonItem();
            this.btnResetDefault = new DevComponents.DotNetBar.ButtonItem();
            this.btnOK = new DevComponents.DotNetBar.ButtonX();
            this.btnCancel = new DevComponents.DotNetBar.ButtonX();
            this.SuspendLayout();
            // 
            // labelXDescription
            // 
            this.labelXDescription.BackColor = System.Drawing.Color.Transparent;
            // 
            // 
            // 
            this.labelXDescription.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.labelXDescription.Location = new System.Drawing.Point(12, 9);
            this.labelXDescription.Name = "labelXDescription";
            this.labelXDescription.Size = new System.Drawing.Size(420, 46);
            this.labelXDescription.TabIndex = 0;
            this.labelXDescription.Text = "請輸入用於椅子與騎乘座騎角色外觀範例的角色外觀代碼。\r\n例: 2000,12000,51484,61183,1003968,1052667\r\n請以相同格式用逗號分隔輸入。";
            // 
            // textBoxXCode
            // 
            this.textBoxXCode.BackColor = System.Drawing.Color.White;
            // 
            // 
            // 
            this.textBoxXCode.Border.Class = "TextBoxBorder";
            this.textBoxXCode.Border.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.textBoxXCode.ForeColor = System.Drawing.Color.Black;
            this.textBoxXCode.Location = new System.Drawing.Point(12, 58);
            this.textBoxXCode.Multiline = true;
            this.textBoxXCode.Name = "textBoxXCode";
            this.textBoxXCode.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxXCode.Size = new System.Drawing.Size(420, 60);
            this.textBoxXCode.TabIndex = 1;
            this.textBoxXCode.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBoxXCode_KeyDown);
            // 
            // btnLoadAvatar
            // 
            this.btnLoadAvatar.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            this.btnLoadAvatar.AutoExpandOnClick = true;
            this.btnLoadAvatar.ColorTable = DevComponents.DotNetBar.eButtonColor.OrangeWithBackground;
            this.btnLoadAvatar.Location = new System.Drawing.Point(12, 126);
            this.btnLoadAvatar.Name = "btnLoadAvatar";
            this.btnLoadAvatar.Size = new System.Drawing.Size(50, 25);
            this.btnLoadAvatar.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.btnLoadAvatar.SubItems.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.btnLoadPreset,
            this.btnImportCharacter,
            this.btnResetDefault});
            this.btnLoadAvatar.TabIndex = 2;
            this.btnLoadAvatar.Text = "載入";
            // 
            // btnLoadPreset
            // 
            this.btnLoadPreset.Name = "btnLoadPreset";
            this.btnLoadPreset.Text = "自訂預設";
            this.btnLoadPreset.Click += new System.EventHandler(this.btnLoadPreset_Click);
            // 
            // btnImportCharacter
            // 
            this.btnImportCharacter.Name = "btnImportCharacter";
            this.btnImportCharacter.Text = "實際角色";
            this.btnImportCharacter.Click += new System.EventHandler(this.btnImportCharacter_Click);
            // 
            // btnResetDefault
            // 
            this.btnResetDefault.Name = "btnResetDefault";
            this.btnResetDefault.Text = "回復預設值";
            this.btnResetDefault.Click += new System.EventHandler(this.btnResetDefault_Click);
            // 
            // btnOK
            // 
            this.btnOK.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            this.btnOK.ColorTable = DevComponents.DotNetBar.eButtonColor.OrangeWithBackground;
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Location = new System.Drawing.Point(270, 126);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(78, 25);
            this.btnOK.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.btnOK.TabIndex = 3;
            this.btnOK.Text = "確定";
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            this.btnCancel.ColorTable = DevComponents.DotNetBar.eButtonColor.OrangeWithBackground;
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(354, 126);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(78, 25);
            this.btnCancel.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.btnCancel.TabIndex = 4;
            this.btnCancel.Text = "取消";
            // 
            // FrmAvatarCode
            // 
            this.AcceptButton = this.btnOK;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(444, 163);
            this.ControlBox = false;
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.btnLoadAvatar);
            this.Controls.Add(this.textBoxXCode);
            this.Controls.Add(this.labelXDescription);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmAvatarCode";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "角色外觀代碼設定";
            this.ResumeLayout(false);
        }

        #endregion

        private DevComponents.DotNetBar.LabelX labelXDescription;
        private DevComponents.DotNetBar.Controls.TextBoxX textBoxXCode;
        private DevComponents.DotNetBar.ButtonX btnLoadAvatar;
        private DevComponents.DotNetBar.ButtonItem btnLoadPreset;
        private DevComponents.DotNetBar.ButtonItem btnImportCharacter;
        private DevComponents.DotNetBar.ButtonItem btnResetDefault;
        private DevComponents.DotNetBar.ButtonX btnOK;
        private DevComponents.DotNetBar.ButtonX btnCancel;
    }
}
