using DevComponents.DotNetBar;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using WzComparerR2.PluginBase;
using WzComparerR2.WzLib;
namespace WzComparerR2.Avatar.UI
{
    public partial class LoadAvatarForm : DevComponents.DotNetBar.Office2007Form
    {
        public LoadAvatarForm()
        {
            InitializeComponent();
            Instance = this;
#if NET6_0_OR_GREATER
            // https://learn.microsoft.com/en-us/dotnet/core/compatibility/fx-core#controldefaultfont-changed-to-segoe-ui-9pt
            this.Font = new Font(new FontFamily("SimSun"), 9f);
#endif
        }
        public static List<string> _files = new List<string>();
        public static List<Image> ImageList = new List<Image>();
        public static int _imageSize = 85;
        public static LoadAvatarForm Instance;
        string Code = "";
        private string avatarPath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Images");
        private void LoadAvatarForm_Load(object sender, EventArgs e)
        {
            this.FormClosing += (s, e1) =>
           {
               this.Hide();
               e1.Cancel = true;
           };

        }

        public static void LoadImages()
        {
            if (_files == null)
            {
                return;
            }
            LoadAvatarForm.Instance.dataGridViewX1.Rows.Clear();
            LoadAvatarForm.Instance.dataGridViewX1.Columns.Clear();

            int numColumnsForWidth = (LoadAvatarForm.Instance.dataGridViewX1.Width - 10) / (_imageSize + 20);
            int numRows = 0;
            int numImages = _files.Count;

            numRows = numImages / numColumnsForWidth;

            if (numImages % numColumnsForWidth > 0)
            {
                numRows += 1;
            }
            if (numImages < numColumnsForWidth)
            {
                numColumnsForWidth = numImages;
            }
            int numGeneratedCells = numRows * numColumnsForWidth;
            // Dynamically create the columns
            for (int index = 0; index < numColumnsForWidth; index++)
            {
                DataGridViewImageColumn dataGridViewColumn = new DataGridViewImageColumn();
                LoadAvatarForm.Instance.dataGridViewX1.Columns.Add(dataGridViewColumn);
                LoadAvatarForm.Instance.dataGridViewX1.Columns[index].Width = _imageSize + 20;
            }

            for (int index = 0; index < numRows; index++)
            {
                LoadAvatarForm.Instance.dataGridViewX1.Rows.Add();
                LoadAvatarForm.Instance.dataGridViewX1.Rows[index].Height = _imageSize + 20;
            }
            int columnIndex = 0;
            int rowIndex = 0;

            for (int index = 0; index < _files.Count; index++)
            {
                Image image = Image.FromFile(_files[index]);
                LoadAvatarForm.ImageList.Add(image);
                LoadAvatarForm.Instance.dataGridViewX1.Rows[rowIndex].Cells[columnIndex].Value = image;
                LoadAvatarForm.Instance.dataGridViewX1.Rows[rowIndex].Cells[columnIndex].ToolTipText = Path.GetFileName(_files[index]);

                if (columnIndex == numColumnsForWidth - 1)
                {
                    rowIndex++;
                    columnIndex = 0;
                }
                else
                {
                    columnIndex++;
                }

            }
        }

        private void SaveAvatarButton_Click(object sender, EventArgs e)
        {
            string pendingCode = AvatarForm.Instance.GetAllPartsTag();
            if (!pendingCode.Any(char.IsDigit)) return;
            AvatarForm.Instance.SavePreset(pendingCode);
            LoadAvatarForm._files.Clear();
            string[] files = Directory.GetFiles(avatarPath);
            LoadAvatarForm._files.AddRange(files);
            LoadAvatarForm.LoadImages();
        }

        private void DeleteAvatarButton_Click(object sender, EventArgs e)
        {
            if (Code.Length == 0) return;
            if (!File.Exists(Path.Combine(avatarPath, Code))) return;
            if (MessageBoxEx.Show(this, "要刪除這個紙娃娃預設嗎？", "確認", MessageBoxButtons.YesNo) == DialogResult.No) return;
            for (int i = 0; i < LoadAvatarForm.ImageList.Count; i++) ImageList[i].Dispose();
            File.Delete(Path.Combine(avatarPath, Code));
            LoadAvatarForm._files.Clear();
            string[] files = Directory.GetFiles(avatarPath);
            LoadAvatarForm._files.AddRange(files);
            LoadAvatarForm.LoadImages();
        }

        private void dataGridViewX1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            Code = dataGridViewX1.Rows[e.RowIndex].Cells[e.ColumnIndex].ToolTipText;
        }

        private void dataGridViewX1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            Code = dataGridViewX1.Rows[e.RowIndex].Cells[e.ColumnIndex].ToolTipText;
            if (PluginManager.FindWz(Wz_Type.Base) == null)
            {
                MessageBoxEx.Show(this, "Base.wz 未載入。", "注意");
                return;
            }
            if (Code.Length < 10)
                return;
            string Code2 = Code.Replace(".png", "");
            AvatarForm.Instance.LoadCode(Code2, 0);
        }
    }
}
