using QRCoder;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace WzComparerR2.Avatar.UI
{
    public partial class LWAForm : DevComponents.DotNetBar.Office2007Form
    {
        public LWAForm()
        {
            InitializeComponent();
        }

        public string avatarCode { get; set; }
        public string ign
        {
            get { return txtIGN.Text; }
            set { txtIGN.Text = value; }
        }

        private void btnGenerate_Click(object sender, EventArgs e)
        {
            string targetIGN = Uri.EscapeDataString(ign);
            if (string.IsNullOrEmpty(targetIGN)) targetIGN = Uri.EscapeDataString(txtIGN.WatermarkText);
            string craftedQrCodeMsg = $"01/{avatarCode}|{targetIGN}|0|1|1|1";

            using (var qrGenerator = new QRCodeGenerator())
            using (var qrData = qrGenerator.CreateQrCode(craftedQrCodeMsg, QRCodeGenerator.ECCLevel.Q))
            using (var qrCode = new QRCode(qrData))
            {
                Bitmap qrImage = qrCode.GetGraphic(20);
                picQR.Image = qrImage;
                btnSaveQR.Enabled = true;
            }
        }

        private void btnSaveQR_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog dlg = new SaveFileDialog())
            {
                dlg.Title = "QRコードを保存";
                dlg.Filter = "PNG (*.png)|*.png";
                dlg.CheckPathExists = true;
                dlg.DefaultExt = "txt";

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    picQR.Image.Save(dlg.FileName, ImageFormat.Png);
                }
            }
        }

        private void LWAForm_Load(object sender, EventArgs e)
        {
            string imageUrl = $"https://open.api.nexon.com/static/maplestory/character/look/{avatarCode}";
            Task.Run(() => ShowImageAsync(imageUrl));
        }

        private async Task ShowImageAsync(string url)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    byte[] imageBytes = await client.GetByteArrayAsync(url);

                    using (MemoryStream ms = new MemoryStream(imageBytes))
                    {
                        Image img = Image.FromStream(ms);
                        picPreview.Image = img; // safe, still on UI thread
                    }
                }
            }
            catch
            {
            }
        }
    }
}
