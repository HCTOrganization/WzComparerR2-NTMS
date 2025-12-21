using DevComponents.DotNetBar;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace WzComparerR2
{
    public partial class FrmSoundExport : DevComponents.DotNetBar.Office2007Form
    {
        public FrmSoundExport(bool isDarkMode)
        {
            InitializeComponent();
            if (isDarkMode)
            {
                this.clbSoundImgName.BackColor = System.Drawing.Color.FromArgb(-13816528);
                this.clbSoundImgName.ForeColor = System.Drawing.Color.LightGray;
            }
#if NET6_0_OR_GREATER
            // https://learn.microsoft.com/en-us/dotnet/core/compatibility/fx-core#controldefaultfont-changed-to-segoe-ui-9pt
            this.Font = new Font(new FontFamily("SimSun"), 9f);
#endif
        }

        public string ExportFolderPath { get; private set; }
        public List<string> SelectedSoundCodes { get; private set; }

        public void AddSoundEntry(string soundImgEntry)
        {
            this.clbSoundImgName.Items.Add(soundImgEntry, soundImgEntry.StartsWith("Bgm"));
        }

        private void btnSelectAll_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < this.clbSoundImgName.Items.Count; i++)
            {
                this.clbSoundImgName.SetItemChecked(i, true);
            }
        }

        private void btnReverseSelect_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < this.clbSoundImgName.Items.Count; i++)
            {
                this.clbSoundImgName.SetItemChecked(i, !this.clbSoundImgName.GetItemChecked(i));
            }
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            if (this.clbSoundImgName.CheckedItems.Count == 0)
            {
                MessageBoxEx.Show("請至少選擇1個音訊歸檔。", "未選擇音訊", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            FolderBrowserDialog dlg = new FolderBrowserDialog();
            dlg.Description = "請選擇要匯出的資料夾。";

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                SelectedSoundCodes = new List<string>();
                foreach (var i in this.clbSoundImgName.CheckedItems)
                {
                    SelectedSoundCodes.Add(i.ToString());
                }
                ExportFolderPath = dlg.SelectedPath;
                this.DialogResult = DialogResult.OK;
            }
        }
    }
}
