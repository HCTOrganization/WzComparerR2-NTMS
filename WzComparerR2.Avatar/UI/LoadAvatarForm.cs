using DevComponents.DotNetBar;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Windows.Forms;
using WzComparerR2.CharaSim;
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
            this.isConfigMigrated = File.Exists(presetJsonPath);
            if (!isConfigMigrated)
            {
                MigrateConfiguration();
                LoadImages();
            }
        }
        public static List<string> _files = new List<string>();
        public static List<Image> ImageList = new List<Image>();
        public static Dictionary<string, string> presetDict = new Dictionary<string, string>();
        public static int _imageSize = 85;
        public static LoadAvatarForm Instance;
        string Code = "";
        string CurrentFileName = "";
        private static string avatarPath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Images");
        private static string presetJsonPath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Images", "config.json");
        private bool isConfigMigrated;
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
                if (!File.Exists(_files[index]))
                {
                    string originalCode = AvatarForm.Instance.GetAllPartsTag();
                    string md5 = Path.GetFileName(_files[index]).Replace(".png", "");
                    string pendingCode = presetDict[md5];
                    AvatarForm.Instance.LoadCode(pendingCode, 0);
                    AvatarForm.Instance.SavePreset(pendingCode, md5);
                    AvatarForm.Instance.LoadCode(originalCode, 0);
                }

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

        private static void MigrateConfiguration()
        {
            string[] files = Directory.GetFiles(avatarPath);
            foreach (string file in files)
            {
                if (!file.Contains(","))
                {
                    File.Delete(file);
                    continue;
                }
                string avatarCode = Path.GetFileName(file).Replace(".png", "").Replace("×", "*");
                string newFileName = GenerateMD5(avatarCode);
                string newFilePath = Path.Combine(avatarPath, newFileName + ".png");
                if (File.Exists(newFilePath)) File.Delete(newFilePath);
                File.Move(file, newFilePath);
                presetDict.Add(newFileName, avatarCode);
            }
            Translator.saveDict(presetJsonPath, presetDict);
        }

        private static string GenerateMD5(string input)
        {
            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
            }
        }

        private void SaveAvatarButton_Click(object sender, EventArgs e)
        {
            string pendingCode = AvatarForm.Instance.GetAllPartsTag();
            string md5 = GenerateMD5(pendingCode);
            if (!pendingCode.Any(char.IsDigit)) return;
            AvatarForm.Instance.SavePreset(pendingCode, md5);
            presetDict.Add(md5, pendingCode);
            Translator.saveDict(presetJsonPath, presetDict);
            Translator.saveDict(presetJsonPath, presetDict);
            LoadAvatarForm._files.Clear();
            LoadAvatarForm._files = LoadAvatarForm.presetDict.Keys.Select(key => Path.Combine(avatarPath, key + ".png")).ToList();
            LoadAvatarForm.LoadImages();
        }

        private void DeleteAvatarButton_Click(object sender, EventArgs e)
        {
            if (!File.Exists(Path.Combine(avatarPath, CurrentFileName))) return;
            if (MessageBoxEx.Show(this, "要刪除這個紙娃娃預設嗎？", "確認", MessageBoxButtons.YesNo) == DialogResult.No) return;
            for (int i = 0; i < LoadAvatarForm.ImageList.Count; i++) ImageList[i].Dispose();
            File.Delete(Path.Combine(avatarPath, CurrentFileName));
            presetDict.Remove(CurrentFileName.Replace(".png", ""));
            Translator.saveDict(presetJsonPath, presetDict);
            LoadAvatarForm._files.Clear();
            LoadAvatarForm._files = LoadAvatarForm.presetDict.Keys.Select(key => Path.Combine(avatarPath, key + ".png")).ToList();
            LoadAvatarForm.LoadImages();
        }

        private void dataGridViewX1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            CurrentFileName = dataGridViewX1.Rows[e.RowIndex].Cells[e.ColumnIndex].ToolTipText;
            Code = presetDict[CurrentFileName.Replace(".png", "")];
        }

        private void dataGridViewX1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                CurrentFileName = dataGridViewX1.Rows[e.RowIndex].Cells[e.ColumnIndex].ToolTipText;
                Code = presetDict[CurrentFileName.Replace(".png", "")];
            }
            catch
            {
            }
            if (PluginManager.FindWz(Wz_Type.Base) == null)
            {
                MessageBoxEx.Show(this, "Base.wz 未載入。", "注意");
                return;
            }
            if (Code.Length < 10)
                return;
            AvatarForm.Instance.LoadCode(Code, 0);
        }
    }
}
