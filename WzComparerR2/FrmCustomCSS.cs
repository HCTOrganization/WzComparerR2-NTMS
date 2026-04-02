using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using WzComparerR2.Config;

namespace WzComparerR2
{
    public partial class FrmCustomCSS : DevComponents.DotNetBar.Office2007Form
    {
        public FrmCustomCSS(bool isDarkMode)
        {
            InitializeComponent();
            this.DarkMode = isDarkMode;
#if NET6_0_OR_GREATER
            // https://learn.microsoft.com/en-us/dotnet/core/compatibility/fx-core#controldefaultfont-changed-to-segoe-ui-9pt
            this.Font = new Font(new FontFamily("SimSun"), 9f);
#endif
        }

        private bool DarkMode;

        public Color backgroundColor
        {
            get { return colorPickerBackgroundColor.SelectedColor; }
            set { colorPickerBackgroundColor.SelectedColor = value; }
        }

        public Color normalTextColor
        {
            get { return colorPickerNormalTextColor.SelectedColor; }
            set { colorPickerNormalTextColor.SelectedColor = value; }
        }
        public Color changedBackgroundColor
        {
            get { return colorPickerChangedBackgroundColor.SelectedColor; }
            set { colorPickerChangedBackgroundColor.SelectedColor = value; }
        }
        public Color addedBackgroundColor
        {
            get { return colorPickerAddedBackgroundColor.SelectedColor; }
            set { colorPickerAddedBackgroundColor.SelectedColor = value; }
        }

        public Color removedBackgroundColor
        {
            get { return colorPickerRemovedBackgroundColor.SelectedColor; }
            set { colorPickerRemovedBackgroundColor.SelectedColor = value; }
        }

        public Color changedTextColor
        {
            get { return colorPickerChangedTextColor.SelectedColor; }
            set { colorPickerChangedTextColor.SelectedColor = value; }
        }

        public Color addedTextColor
        {
            get { return colorPickerAddedTextColor.SelectedColor; }
            set { colorPickerAddedTextColor.SelectedColor = value; }
        }

        public Color removedTextColor
        {
            get { return colorPickerRemovedTextColor.SelectedColor; }
            set { colorPickerRemovedTextColor.SelectedColor = value; }
        }

        public Color hyperlinkColor
        {
            get { return colorPickerHyperlinkColor.SelectedColor; }
            set { colorPickerHyperlinkColor.SelectedColor = value; }
        }

        private void btnDefault_Click(object sender, EventArgs e)
        {
            if (DarkMode)
            {
                this.backgroundColor = Color.FromArgb(Int32.Parse("ff000000", NumberStyles.HexNumber));
                this.normalTextColor = Color.FromArgb(Int32.Parse("ffffffff", NumberStyles.HexNumber));
                this.changedBackgroundColor = Color.FromArgb(Int32.Parse("ff003049", NumberStyles.HexNumber));
                this.addedBackgroundColor = Color.FromArgb(Int32.Parse("ff000000", NumberStyles.HexNumber));
                this.removedBackgroundColor = Color.FromArgb(Int32.Parse("ff462306", NumberStyles.HexNumber));
                this.changedTextColor = Color.FromArgb(Int32.Parse("ffffffff", NumberStyles.HexNumber));
                this.addedTextColor = Color.FromArgb(Int32.Parse("ffffffff", NumberStyles.HexNumber));
                this.removedTextColor = Color.FromArgb(Int32.Parse("ffffffff", NumberStyles.HexNumber));
                this.hyperlinkColor = Color.FromArgb(Int32.Parse("ffffffff", NumberStyles.HexNumber));
            }
            else
            {
                this.backgroundColor = Color.FromArgb(Int32.Parse("ffbbbbbb", NumberStyles.HexNumber));
                this.normalTextColor = Color.FromArgb(Int32.Parse("ff000000", NumberStyles.HexNumber));
                this.changedBackgroundColor = Color.FromArgb(Int32.Parse("ffcccc00", NumberStyles.HexNumber));
                this.addedBackgroundColor = Color.FromArgb(Int32.Parse("ff9ce34e", NumberStyles.HexNumber));
                this.removedBackgroundColor = Color.FromArgb(Int32.Parse("ffff6666", NumberStyles.HexNumber));
                this.changedTextColor = Color.FromArgb(Int32.Parse("ff000000", NumberStyles.HexNumber));
                this.addedTextColor = Color.FromArgb(Int32.Parse("ff000000", NumberStyles.HexNumber));
                this.removedTextColor = Color.FromArgb(Int32.Parse("ff000000", NumberStyles.HexNumber));
                this.hyperlinkColor = Color.FromArgb(Int32.Parse("ff0000ff", NumberStyles.HexNumber));
            }
            this.DarkMode = !this.DarkMode;
        }

        private void colorPickers_SelectedColorChanged(object sender, EventArgs e)
        {
            this.richTextBoxEx1.ReadOnly = false;
            this.richTextBoxEx1.Clear();
            this.richTextBoxEx1.BackColorRichTextBox = backgroundColor;
            this.richTextBoxEx1.AppendText("                      ");
            var contents = new[]
            {
                new {Text = "超連結" + Environment.NewLine, Fore = hyperlinkColor, Back = backgroundColor, Underline = true},
                new {Text = "                常規內容的説明文字               " + Environment.NewLine, Fore = normalTextColor, Back = backgroundColor, Underline = false},
                new {Text = "               已變更內容的説明文字               " + Environment.NewLine, Fore = changedTextColor, Back = changedBackgroundColor, Underline = false},
                new {Text = "               已新增內容的説明文字               " + Environment.NewLine, Fore = addedTextColor, Back = addedBackgroundColor, Underline = false},
                new {Text = "               已刪除內容的説明文字               ", Fore = removedTextColor, Back = removedBackgroundColor, Underline = false},
            };
            foreach (var line in contents)
            {
                int start = richTextBoxEx1.TextLength;
                richTextBoxEx1.AppendText(line.Text);
                richTextBoxEx1.Select(start, line.Text.Length);
                richTextBoxEx1.SelectionColor = line.Fore;
                richTextBoxEx1.SelectionBackColor = line.Back;
                if (line.Underline)
                {
                    richTextBoxEx1.SelectionFont = new Font(richTextBoxEx1.Font, FontStyle.Underline);
                }
            }
            this.richTextBoxEx1.ReadOnly = true;
        }

        public void LoadConfig(CustomCSSConfig config)
        {
            this.backgroundColor = config.BackgroundColor;
            this.normalTextColor = config.NormalTextColor;
            this.changedBackgroundColor = config.ChangedBackgroundColor;
            this.addedBackgroundColor = config.AddedBackgroundColor;
            this.removedBackgroundColor = config.RemovedBackgroundColor;
            this.changedTextColor = config.ChangedTextColor;
            this.addedTextColor = config.AddedTextColor;
            this.removedTextColor = config.RemovedTextColor;
            this.changedBackgroundColor = config.ChangedBackgroundColor;
            this.hyperlinkColor = config.HyperlinkColor;
        }

        public void SaveConfig(CustomCSSConfig config)
        {
            config.BackgroundColor = this.backgroundColor;
            config.NormalTextColor = this.normalTextColor;
            config.ChangedBackgroundColor = this.changedBackgroundColor;
            config.AddedBackgroundColor = this.addedBackgroundColor;
            config.RemovedBackgroundColor = this.removedBackgroundColor;
            config.ChangedTextColor = this.changedTextColor;
            config.AddedTextColor = this.addedTextColor;
            config.RemovedTextColor = this.removedTextColor;
            config.ChangedBackgroundColor = this.changedBackgroundColor;
            config.HyperlinkColor = this.hyperlinkColor;
        }
    }
}
