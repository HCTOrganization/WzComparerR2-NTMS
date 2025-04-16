using System.Drawing;
using System.Windows.Forms;

namespace WzComparerR2
{
    partial class FrmWaiting
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
            this.LabelWaiting = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // LabelWaiting
            // 
            this.LabelWaiting.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LabelWaiting.Font = new System.Drawing.Font("SimSun", 11F);
            this.LabelWaiting.Location = new System.Drawing.Point(0, 0);
            this.LabelWaiting.Name = "LabelWaiting";
            this.LabelWaiting.Size = new System.Drawing.Size(200, 100);
            this.LabelWaiting.TabIndex = 0;
            this.LabelWaiting.Text = "Placeholder";
            this.LabelWaiting.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // FrmWaiting
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(200, 100);
            this.Controls.Add(this.LabelWaiting);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmWaiting";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "WzComparerR2-NTMS";
            this.TopMost = true;
            this.ResumeLayout(false);

        }

        #endregion

        private Label LabelWaiting;
    }
}