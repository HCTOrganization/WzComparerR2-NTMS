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
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
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
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToResizeColumns = false;
            this.dataGridView1.AllowUserToResizeRows = false;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.ColumnHeadersVisible = false;
            this.dataGridView1.Location = new System.Drawing.Point(12, 37);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowHeadersVisible = false;
            this.dataGridView1.RowHeadersWidth = 51;
            this.dataGridView1.RowTemplate.Height = 27;
            this.dataGridView1.Size = new System.Drawing.Size(739, 560);
            this.dataGridView1.TabIndex = 1;
            this.dataGridView1.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_CellClick);
            // 
            // LoadAvatarForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(766, 609);
            this.Controls.Add(this.dataGridView1);
            this.Controls.Add(this.SaveAvatarButton);
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
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.ResumeLayout(false);

        }


        #endregion

       
        private DevComponents.DotNetBar.ButtonX SaveAvatarButton;
        private System.Windows.Forms.DataGridView dataGridView1;
    }
}