using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using WzComparerR2.CharaSim;

namespace WzComparerR2
{
    /// <summary>
    /// Read-only dialog used to pick an avatar preset.
    /// </summary>
    public partial class FrmAvatarPreset : DevComponents.DotNetBar.Office2007Form
    {
        public FrmAvatarPreset()
            : this(false)
        {
        }

        public FrmAvatarPreset(bool isDarkMode)
        {
#if NET6_0_OR_GREATER
            // https://learn.microsoft.com/en-us/dotnet/core/compatibility/fx-core#controldefaultfont-changed-to-segoe-ui-9pt
            this.Font = new Font(new FontFamily("SimSun"), 9f);
#endif
            InitializeComponent();
            this.DarkMode = isDarkMode;
            if (isDarkMode)
            {
                this.dataGridViewX1.BackgroundColor = DarkBackgroundColor;
                this.dataGridViewX1.GridColor = Color.DimGray;
            }
        }

        private bool DarkMode;
        private static readonly Color DarkBackgroundColor = Color.FromArgb(-13816528);
        private const int ImageSize = 85;

        /// <summary>
        /// Folder that stores the presets saved by the avatar plugin (same location as the plugin).
        /// </summary>
        private static string AvatarPath
        {
            get { return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images"); }
        }

        private static string PresetJsonPath
        {
            get { return Path.Combine(AvatarPath, "config.json"); }
        }

        private readonly List<Image> previewImages = new List<Image>();
        private string selectedCode;

        /// <summary>
        /// Selected avatar code, or an empty string when nothing is selected.
        /// </summary>
        public string SelectedCode
        {
            get { return selectedCode; }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            this.LoadPresets();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            // the grid still references the images, so drop the cells before disposing them
            this.dataGridViewX1.Rows.Clear();
            this.dataGridViewX1.Columns.Clear();
            foreach (var image in this.previewImages)
            {
                image.Dispose();
            }
            this.previewImages.Clear();
            base.OnFormClosed(e);
        }

        private void LoadPresets()
        {
            this.dataGridViewX1.Rows.Clear();
            this.dataGridViewX1.Columns.Clear();
            this.selectedCode = null;
            this.btnOK.Enabled = false;

            Dictionary<string, string> presetDict;
            if (!File.Exists(PresetJsonPath))
            {
                this.labelStatus.Text = "找不到預設。請在角色外觀製作畫面中儲存。";
                return;
            }

            try
            {
                presetDict = Translator.loadDict(PresetJsonPath) ?? new Dictionary<string, string>();
            }
            catch (Exception ex)
            {
                this.labelStatus.Text = "預設載入失敗: " + ex.Message;
                return;
            }

            var files = presetDict.Keys
                .Select(key => Path.Combine(AvatarPath, key + ".png"))
                .Where(File.Exists)
                .ToList();

            if (files.Count <= 0)
            {
                this.labelStatus.Text = "找不到預設。請在角色外觀製作畫面中儲存。";
                return;
            }

            int columnCount = Math.Max(1, (this.dataGridViewX1.Width - 10) / (ImageSize + 20));
            int rowCount = (files.Count + columnCount - 1) / columnCount;
            if (files.Count < columnCount)
            {
                columnCount = files.Count;
            }

            for (int i = 0; i < columnCount; i++)
            {
                var column = new DataGridViewImageColumn();
                this.dataGridViewX1.Columns.Add(column);
                this.dataGridViewX1.Columns[i].Width = ImageSize + 20;
            }

            for (int i = 0; i < rowCount; i++)
            {
                this.dataGridViewX1.Rows.Add();
                this.dataGridViewX1.Rows[i].Height = ImageSize + 20;
            }

            int rowIndex = 0;
            int columnIndex = 0;
            foreach (string file in files)
            {
                Image image;
                try
                {
                    image = Image.FromFile(file);
                }
                catch
                {
                    continue;
                }

                string key = Path.GetFileNameWithoutExtension(file);
                string code;
                if (!presetDict.TryGetValue(key, out code) || string.IsNullOrEmpty(code))
                {
                    image.Dispose();
                    continue;
                }

                this.previewImages.Add(image);
                var cell = this.dataGridViewX1.Rows[rowIndex].Cells[columnIndex];
                cell.Value = image;
                cell.Tag = code;
                cell.ToolTipText = code;
                cell.Style.SelectionBackColor = this.DarkMode ? Color.SteelBlue : SystemColors.Highlight;

                if (columnIndex == columnCount - 1)
                {
                    rowIndex++;
                    columnIndex = 0;
                }
                else
                {
                    columnIndex++;
                }
            }

            this.labelStatus.Text = string.Format("可雙擊選擇 {0} 個預設。", files.Count);
        }

        private void dataGridViewX1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
            {
                return;
            }

            var cell = this.dataGridViewX1.Rows[e.RowIndex].Cells[e.ColumnIndex];
            this.selectedCode = cell.Tag as string;
            this.btnOK.Enabled = !string.IsNullOrEmpty(this.selectedCode);
        }

        private void dataGridViewX1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            this.dataGridViewX1_CellClick(sender, e);
            if (!string.IsNullOrEmpty(this.selectedCode))
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(this.selectedCode))
            {
                this.DialogResult = DialogResult.None;
                return;
            }
            this.DialogResult = DialogResult.OK;
        }
    }
}
