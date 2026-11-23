using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace WzComparerR2
{
    public partial class FrmSelectUnityLocale : DevComponents.DotNetBar.Office2007Form
    {
        public FrmSelectUnityLocale(IReadOnlyList<string> locales, string defaultLocale, string datasetPath, bool isDarkMode = false)
        {
            InitializeComponent();
            if (isDarkMode)
            {
                this.listBoxLocale.BackColor = Color.FromArgb(-13816528);
                this.listBoxLocale.ForeColor = Color.LightGray;
            }
#if NET6_0_OR_GREATER
            // https://learn.microsoft.com/en-us/dotnet/core/compatibility/fx-core#controldefaultfont-changed-to-segoe-ui-9pt
            this.Font = new Font(new FontFamily("SimSun"), 9f);
#endif
            this.labelX1.Text = "請選擇要載入的區域代碼。\r\n"
                + "（共通資料會自動合併）\r\n"
                + "目標資料夾: " + GetFolderName(datasetPath);

            foreach (var locale in locales)
            {
                this.listBoxLocale.Items.Add(new LocaleItem(locale));
            }
            this.SelectLocale(defaultLocale);
        }

        /// <summary>使用者選擇的區域代碼（TW/CN/JP/KR…）。</summary>
        public string SelectedLocale
        {
            get
            {
                var item = this.listBoxLocale.SelectedItem as LocaleItem;
                return item == null ? null : item.Code;
            }
        }

        private void listBoxLocale_DoubleClick(object sender, EventArgs e)
        {
            if (this.listBoxLocale.SelectedIndex >= 0)
            {
                this.DialogResult = DialogResult.OK;
            }
        }

        private static string GetFolderName(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return string.Empty;
            }
            var name = System.IO.Path.GetFileName(path.TrimEnd('\\', '/'));
            return string.IsNullOrEmpty(name) ? path : name;
        }

        private void SelectLocale(string code)
        {
            for (int i = 0; i < this.listBoxLocale.Items.Count; i++)
            {
                var item = this.listBoxLocale.Items[i] as LocaleItem;
                if (item != null && string.Equals(item.Code, code, StringComparison.OrdinalIgnoreCase))
                {
                    this.listBoxLocale.SelectedIndex = i;
                    return;
                }
            }
            if (this.listBoxLocale.Items.Count > 0)
            {
                this.listBoxLocale.SelectedIndex = 0;
            }
        }

        private sealed class LocaleItem
        {
            public LocaleItem(string code)
            {
                this.Code = code;
            }

            public string Code { get; private set; }

            public override string ToString()
            {
                return this.Code + "  " + GetLocaleDescription(this.Code);
            }

            private static string GetLocaleDescription(string code)
            {
                switch ((code ?? string.Empty).ToUpperInvariant())
                {
                    case "TW": return "（繁體中文）";
                    case "CN": return "（簡體中文）";
                    case "JP": return "（日文）";
                    case "KR": return "（韓文）";
                    case "COMMON": return "（共通）";
                    default: return string.Empty;
                }
            }
        }
    }
}
