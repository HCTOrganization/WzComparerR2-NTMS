using DevComponents.Editors;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
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
            cmbRegion.Items.AddRange(new[]
            {
                new ComboItem("KMS"){ Value = 1 },
                new ComboItem("JMS"){ Value = 2 },
                //new ComboItem("CMS"){ Value = 3 },
                new ComboItem("GMS北美"){ Value = 4 },
                new ComboItem("GMS歐洲"){ Value = 5 },
                //new ComboItem("MSEA"){ Value = 6 },
                //new ComboItem("TMS"){ Value = 7 },
                //new ComboItem("MSN"){ Value = 8 },
            });
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

        public int selectedRegion
        {
            get
            {
                return ((cmbRegion.SelectedItem as ComboItem)?.Value as int?) ?? 0;
            }
            set
            {
                var items = cmbRegion.Items.Cast<ComboItem>();
                var item = items.FirstOrDefault(_item => _item.Value as int? == value)
                    ?? items.Last();
                item.Value = value;
                cmbRegion.SelectedItem = item;
            }
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
            ComboItem selectedItem = (ComboItem)cmbRegion.SelectedItem;
            if (Translator.IsKoreanStringPresent(textBoxX1.Text))
            {
                textBoxX1.Font = new Font("Dotum", 9f);
            }
            else
            {
                textBoxX1.Font = new Font("SimSun", 9f);
            }
            buttonX1.Enabled = (textBoxX1.Text.Length > 0 && selectedItem != null);
        }

        private void cmbRegion_SelectedIndexChanged(object sender, EventArgs e)
        {
            buttonX1.Enabled = textBoxX1.Text.Length > 0;
            ComboItem selectedItem = (ComboItem)cmbRegion.SelectedItem;
            switch ((int)selectedItem.Value)
            {
                case 1:
                case 3:
                case 6:
                    labelX1.Enabled = true;
                    checkBoxX1.Enabled = true;
                    checkBoxX2.Enabled = true;
                    break;
                case 2:
                case 4:
                case 5:
                case 7:
                case 8:
                    labelX1.Enabled = false;
                    checkBoxX1.Enabled = false;
                    checkBoxX2.Enabled = false;
                    break;


            }
        }
    }
}