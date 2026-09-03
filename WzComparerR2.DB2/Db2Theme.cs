using System.Drawing;
using System.Windows.Forms;
using DevComponents.DotNetBar;

namespace WzComparerR2.DB2
{
    /// <summary>
    /// 深色樣式（VisualStudio2012Dark）的配色套用工具。
    ///
    /// 表單本身與 DotNetBar 控制項（ButtonX / ComboBoxEx / LabelX / TextBoxX /
    /// PanelEx / SuperTabControl）會自動跟著 StyleManager 換色，
    /// 但 DataGridView、ListBox 這類原生 WinForms 控制項不會，必須在這裡手動指定。
    /// 色碼沿用主程式 MainForm.UpdateButtonItemStyles 的設定。
    /// </summary>
    internal static class Db2Theme
    {
        /// <summary>VisualStudio2012Dark 的內容區底色 (#2D2D30)。</summary>
        public static readonly Color DarkBackColor = Color.FromArgb(-13816528);
        public static readonly Color DarkForeColor = Color.LightGray;
        public static readonly Color DarkGridColor = Color.FromArgb(63, 63, 70);
        public static readonly Color DarkSelectionBackColor = Color.FromArgb(38, 79, 120);
        public static readonly Color DarkHeaderBackColor = Color.FromArgb(37, 37, 38);

        /// <summary>主程式目前是否為深色樣式。</summary>
        public static bool IsDarkMode
        {
            get { return Db2Host.Context?.IsDarkMode ?? false; }
        }

        public static void Apply(Form form)
        {
            if (form == null)
            {
                return;
            }
            // Office2007Form 會自行跟著 StyleManager 上色，這裡只處理非 DotNetBar 的子控制項。
            ApplyToChildren(form);
        }

        private static void ApplyToChildren(Control parent)
        {
            foreach (Control control in parent.Controls)
            {
                switch (control)
                {
                    case DataGridView grid:
                        Apply(grid);
                        break;

                    case ListBox listBox:
                        Apply(listBox);
                        break;

                    case LabelX labelX:
                        // LabelX 的 ForeColor 一旦被設定就不再跟著樣式走，所以兩種樣式都要明確指定。
                        labelX.ForeColor = IsDarkMode ? DarkForeColor : SystemColors.ControlText;
                        break;
                }

                if (control.HasChildren)
                {
                    ApplyToChildren(control);
                }
            }
        }

        public static void Apply(DataGridView grid)
        {
            if (grid == null)
            {
                return;
            }

            bool dark = IsDarkMode;
            grid.EnableHeadersVisualStyles = false;
            grid.BackgroundColor = dark ? DarkBackColor : SystemColors.AppWorkspace;
            grid.GridColor = dark ? DarkGridColor : SystemColors.ControlDark;

            grid.DefaultCellStyle.BackColor = dark ? DarkBackColor : Color.White;
            grid.DefaultCellStyle.ForeColor = dark ? DarkForeColor : Color.Black;
            grid.DefaultCellStyle.SelectionBackColor = dark ? DarkSelectionBackColor : Color.LightCyan;
            grid.DefaultCellStyle.SelectionForeColor = dark ? Color.White : Color.Black;

            grid.ColumnHeadersDefaultCellStyle.BackColor = dark ? DarkHeaderBackColor : SystemColors.Control;
            grid.ColumnHeadersDefaultCellStyle.ForeColor = dark ? DarkForeColor : SystemColors.ControlText;
            grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = grid.ColumnHeadersDefaultCellStyle.BackColor;
            grid.ColumnHeadersDefaultCellStyle.SelectionForeColor = grid.ColumnHeadersDefaultCellStyle.ForeColor;

            grid.RowHeadersDefaultCellStyle.BackColor = grid.ColumnHeadersDefaultCellStyle.BackColor;
            grid.RowHeadersDefaultCellStyle.ForeColor = grid.ColumnHeadersDefaultCellStyle.ForeColor;
        }

        public static void Apply(ListBox listBox)
        {
            if (listBox == null)
            {
                return;
            }

            bool dark = IsDarkMode;
            listBox.BackColor = dark ? DarkBackColor : Color.White;
            listBox.ForeColor = dark ? DarkForeColor : Color.Black;
        }
    }
}
