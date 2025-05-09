using DevComponents.DotNetBar;

namespace WzComparerR2.Avatar.UI
{
    partial class LoadAvatarForm
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
            if(disposing && (components != null))
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
            this.SaveAvatarButton = new DevComponents.DotNetBar.ButtonX();
            this.DeleteAvatarButton = new DevComponents.DotNetBar.ButtonX();
            this.LabelDoubleClickHint = new DevComponents.DotNetBar.LabelX();
            this.dataGridViewX1 = new DevComponents.DotNetBar.Controls.DataGridViewX();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewX1)).BeginInit();
            this.SuspendLayout();
            // 
            // SaveAvatarButton
            // 
            this.SaveAvatarButton.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            this.SaveAvatarButton.ColorTable = DevComponents.DotNetBar.eButtonColor.OrangeWithBackground;
            this.SaveAvatarButton.Location = new System.Drawing.Point(12, 7);
            this.SaveAvatarButton.Name = "SaveAvatarButton";
            this.SaveAvatarButton.Size = new System.Drawing.Size(89, 25);
            this.SaveAvatarButton.TabIndex = 0;
            this.SaveAvatarButton.Text = "保存";
            this.SaveAvatarButton.Click += new System.EventHandler(this.SaveAvatarButton_Click);
            // 
            // DeleteAvatarButton
            // 
            this.DeleteAvatarButton.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            this.DeleteAvatarButton.ColorTable = DevComponents.DotNetBar.eButtonColor.OrangeWithBackground;
            this.DeleteAvatarButton.Location = new System.Drawing.Point(113, 7);
            this.DeleteAvatarButton.Name = "DeleteAvatarButton";
            this.DeleteAvatarButton.Size = new System.Drawing.Size(89, 25);
            this.DeleteAvatarButton.TabIndex = 1;
            this.DeleteAvatarButton.Text = "刪除";
            this.DeleteAvatarButton.Click += new System.EventHandler(this.DeleteAvatarButton_Click);
            // 
            // LabelDoubleClickHint
            // 
            this.LabelDoubleClickHint.AutoSize = true;
            this.LabelDoubleClickHint.BackColor = System.Drawing.Color.Transparent;
            // 
            // 
            // 
            this.LabelDoubleClickHint.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.LabelDoubleClickHint.Location = new System.Drawing.Point(214, 12);
            this.LabelDoubleClickHint.Name = "LabelDoubleClickHint";
            this.LabelDoubleClickHint.Size = new System.Drawing.Size(105, 18);
            this.LabelDoubleClickHint.TabIndex = 5;
            this.LabelDoubleClickHint.Text = "您可點兩下需要載入的紙娃娃預設。";
            // 
            // dataGridViewX1
            // 
            this.dataGridViewX1.AllowUserToResizeColumns = false;
            this.dataGridViewX1.AllowUserToResizeRows = false;
            this.dataGridViewX1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewX1.ColumnHeadersVisible = false;
            this.dataGridViewX1.Location = new System.Drawing.Point(12, 37);
            this.dataGridViewX1.Name = "dataGridViewX1";
            this.dataGridViewX1.RowHeadersVisible = false;
            this.dataGridViewX1.RowHeadersWidth = 51;
            this.dataGridViewX1.RowTemplate.Height = 27;
            this.dataGridViewX1.Size = new System.Drawing.Size(739, 560);
            this.dataGridViewX1.TabIndex = 2;
            this.dataGridViewX1.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridViewX1_CellClick);
            this.dataGridViewX1.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridViewX1_CellDoubleClick);
            this.dataGridViewX1.AllowUserToDeleteRows = false;
            // 
            // LoadAvatarForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(766, 609);
            this.Controls.Add(this.dataGridViewX1);
            this.Controls.Add(this.SaveAvatarButton);
            this.Controls.Add(this.DeleteAvatarButton);
            this.Controls.Add(this.LabelDoubleClickHint);
            this.DoubleBuffered = true;
            this.Font = new System.Drawing.Font("SimSun", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximumSize = new System.Drawing.Size(784, 656);
            this.Name = "LoadAvatarForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "自訂預設";
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.TopMost = true;
            this.Load += new System.EventHandler(this.LoadAvatarForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewX1)).EndInit();
            this.ResumeLayout(false);

        }


        #endregion

       
        private DevComponents.DotNetBar.ButtonX SaveAvatarButton;
        private DevComponents.DotNetBar.ButtonX DeleteAvatarButton;
        private DevComponents.DotNetBar.LabelX LabelDoubleClickHint;
        private DevComponents.DotNetBar.Controls.DataGridViewX dataGridViewX1;
    }
}