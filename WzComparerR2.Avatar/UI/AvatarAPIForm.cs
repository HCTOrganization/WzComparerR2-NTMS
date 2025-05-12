using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using DevComponents.DotNetBar;
using WzComparerR2.CharaSim;

namespace WzComparerR2.Avatar.UI
{
    public partial class AvatarAPIForm : DevComponents.DotNetBar.OfficeForm
    {
        public AvatarAPIForm()
        {
            InitializeComponent();
#if NET6_0_OR_GREATER
            // https://learn.microsoft.com/en-us/dotnet/core/compatibility/fx-core#controldefaultfont-changed-to-segoe-ui-9pt
            this.Font = new Font(new FontFamily("SimSun"), 9f);
#endif
        }

        public string CharaName
        {
            get { return textBoxX1.Text; }
            set { textBoxX1.Text = value; }
        }

        public bool Type1
        {
            get { return checkBoxX1.Checked; }
        }

        private void textBoxX1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private void textBoxX1_TextChanged(object sender, EventArgs e)
        {
            if (Translator.IsKoreanStringPresent(textBoxX1.Text))
            {
                textBoxX1.Font = new Font("Dotum", 9f);
            }
            else
            {
                textBoxX1.Font = new Font("SimSun", 9f);
            }
        }
    }
}