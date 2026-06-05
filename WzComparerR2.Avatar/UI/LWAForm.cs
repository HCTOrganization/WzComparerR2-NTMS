using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using ZXing;
using ZXing.QrCode;
namespace WzComparerR2.Avatar.UI
{
    public partial class LWAForm : DevComponents.DotNetBar.Office2007Form
    {
        public LWAForm()
        {
            InitializeComponent();
#if NET6_0_OR_GREATER
            // https://learn.microsoft.com/en-us/dotnet/core/compatibility/fx-core#controldefaultfont-changed-to-segoe-ui-9pt
            this.Font = new Font(new FontFamily("SimSun"), 9f);
#endif
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

            var options = new QrCodeEncodingOptions
            {
                Width = 1200,
                Height = 1200,
                Margin = 1,
                ErrorCorrection = ZXing.QrCode.Internal.ErrorCorrectionLevel.M
            };
            var writer = new BarcodeWriterPixelData
            {
                Format = BarcodeFormat.QR_CODE,
                Options = options
            };

            var pixelData = writer.Write(craftedQrCodeMsg);
            Bitmap qrImage = new Bitmap(pixelData.Width, pixelData.Height, PixelFormat.Format32bppArgb);
            var bmpData = qrImage.LockBits(new Rectangle(0, 0, qrImage.Width, qrImage.Height),
                                          ImageLockMode.WriteOnly, qrImage.PixelFormat);
            try
            {
                System.Runtime.InteropServices.Marshal.Copy(pixelData.Pixels, 0, bmpData.Scan0, pixelData.Pixels.Length);
            }
            finally
            {
                qrImage.UnlockBits(bmpData);
            }
            picQR.Image = qrImage;
            btnSaveQR.Enabled = true;
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
