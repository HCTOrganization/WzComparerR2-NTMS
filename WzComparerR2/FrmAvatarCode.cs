using System;
using System.Drawing;
using System.Windows.Forms;
using WzComparerR2.Comparer;

namespace WzComparerR2
{
    public partial class FrmAvatarCode : DevComponents.DotNetBar.Office2007Form
    {
        public FrmAvatarCode()
            : this(false)
        {
        }

        public FrmAvatarCode(bool isDarkMode)
        {
#if NET6_0_OR_GREATER
            // https://learn.microsoft.com/en-us/dotnet/core/compatibility/fx-core#controldefaultfont-changed-to-segoe-ui-9pt
            this.Font = new Font(new FontFamily("SimSun"), 9f);
#endif
            InitializeComponent();
            this.DarkMode = isDarkMode;
            if (isDarkMode)
            {
                ApplyDarkMode();
            }
        }

        private bool DarkMode;

        private void ApplyDarkMode()
        {
            if (!this.DarkMode)
            {
                return;
            }

            this.textBoxXCode.BackColor = DarkBackgroundColor;
            this.textBoxXCode.ForeColor = Color.LightGray;
        }

        private static readonly Color DarkBackgroundColor = Color.FromArgb(-13816528);

        /// <summary>
        /// Item code used for the chair and riding mount avatar examples.
        /// </summary>
        public string CodeText
        {
            get { return textBoxXCode.Text.Trim(); }
            set { textBoxXCode.Text = value; }
        }

        private void btnLoadPreset_Click(object sender, EventArgs e)
        {
            using (var frm = new FrmAvatarPreset(this.DarkMode))
            {
                if (frm.ShowDialog(this) == DialogResult.OK && !string.IsNullOrEmpty(frm.SelectedCode))
                {
                    this.CodeText = frm.SelectedCode;
                }
            }
        }

        private void btnImportCharacter_Click(object sender, EventArgs e)
        {
            using (var frm = new FrmAvatarImport(this.DarkMode))
            {
                if (frm.ShowDialog(this) == DialogResult.OK && !string.IsNullOrEmpty(frm.ResultCode))
                {
                    this.CodeText = frm.ResultCode;
                }
            }
        }

        private void btnResetDefault_Click(object sender, EventArgs e)
        {
            this.CodeText = EasyComparer.DefaultAvatarCode;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(this.CodeText))
            {
                DevComponents.DotNetBar.MessageBoxEx.Show(this, "請輸入道具代碼。", "錯誤");
                this.DialogResult = DialogResult.None;
                return;
            }
            this.DialogResult = DialogResult.OK;
        }

        private void textBoxXCode_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.A)
            {
                textBoxXCode.SelectAll();
            }
        }
    }
}
