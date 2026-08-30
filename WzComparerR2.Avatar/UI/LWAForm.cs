using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WzComparerR2.CharaSim;
using ZXing;
using ZXing.QrCode;
#if NET8_0_OR_GREATER
using TlsClientWrapperSharp.Handlers;
using TlsClientWrapperSharp.Helpers;
using TlsClientWrapperSharp.Models;
#endif
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

        public string csvCode { get; set; }
        private string avatarCode;
        public string ign
        {
            get { return txtIGN.Text; }
            set { txtIGN.Text = value; }
        }

        private void btnGenerate_Click(object sender, EventArgs e)
        {
            string targetIGN = Uri.EscapeDataString(ign);
            if (string.IsNullOrEmpty(targetIGN)) targetIGN = Uri.EscapeDataString(txtIGN.WatermarkText);
            string craftedQrCodeMsg = $"01/{avatarCode}|{targetIGN}|0|0|1|1|{Uri.EscapeDataString("묵현")}|2";

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
                dlg.Title = "保存QR碼";
                dlg.Filter = "PNG (*.png)|*.png";
                dlg.CheckPathExists = true;
                dlg.DefaultExt = "png";

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    picQR.Image.Save(dlg.FileName, ImageFormat.Png);
                }
            }
        }

        private void LWAForm_Load(object sender, EventArgs e)
        {
            // string imageUrl = $"https://open.api.nexon.com/static/maplestory/character/look/{avatarCode}";
            Task.Run(() => ShowImageMakAsync(csvCode));
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

        private static string BuildAvatarImageRequestBody(string csvCodeValue)
        {
            var partMap = new Dictionary<GearType, string>
            {
                { GearType.head, "SK" },
                { GearType.face, "FA" },
                { GearType.face2, "FA" },
                { GearType.face3, "FA" },
                { GearType.hair, "HR" },
                { GearType.hair2, "HR" },
                { GearType.hair3, "HR" },
                { GearType.hair4, "HR" },
                { GearType.faceAccessory, "FC" },
                { GearType.eyeAccessory, "EY" },
                { GearType.earrings, "EA" },
                { GearType.shoulderPad, "SO" },
                { GearType.cape, "CP" },
                { GearType.coat, "CT" },
                { GearType.longcoat, "CT" },
                { GearType.pants, "PA" },
                { GearType.glove, "GL" },
                { GearType.weapon, "WP" },
                { GearType.subWeapon, "WP" },
                { GearType.cashWeapon, "WP" },
                { GearType.shovel, "WP" },
                { GearType.pickaxe, "WP" },
            };

            var equipItems = new JArray
            {
                new JObject
                {
                    ["part"] = "GE",
                    ["itemId"] = 1
                }
            };

            string[] csvValues = (csvCodeValue ?? string.Empty).Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string rawValue in csvValues)
            {
                string trimmed = rawValue.Trim();
                if (string.IsNullOrEmpty(trimmed) || !int.TryParse(trimmed, out int itemId))
                {
                    continue;
                }

                if (itemId == 1 && Gear.GetGearType(itemId) == GearType.body)
                {
                    continue;
                }

                // Generic weapon slots are represented as GearType.weapon/subWeapon (-1/-2) in the avatar model.
                // The canonical prefix table lives in NexonOpenAPI.Utils.WeaponsKMS for avatar-code serialization,
                // but the LWA API still expects the slot to be emitted as part "WP" instead of being discarded.
                GearType gearType = Gear.GetGearType(itemId);

                // Weapons are represented by many concrete gear types (e.g. 1222000 -> soulShooter / 122xx family)
                // and not only by the synthetic GearType.weapon/subWeapon sentinels.
                if (Gear.IsWeapon(gearType) || Gear.IsSubWeapon(gearType) || Gear.IsCashWeapon(gearType))
                {
                    equipItems.Add(new JObject
                    {
                        ["part"] = "WP",
                        ["itemId"] = itemId,
                        ["isVisible"] = "true"
                    });
                    continue;
                }

                if (!partMap.TryGetValue(gearType, out string partName))
                {
                    continue;
                }

                equipItems.Add(new JObject
                {
                    ["part"] = partName,
                    ["itemId"] = itemId,
                    ["isVisible"] = "true"
                });
            }

            JObject body = new JObject
            {
                ["EquipArray"] = new JArray(
                    new JObject
                    {
                        ["equipItemList"] = equipItems
                    })
            };

            return JsonConvert.SerializeObject(body);
        }

        private static void AddCompatibleHeaders(HttpRequestMessage request, string referer, string userAgent, string accept = null)
        {
            request.Headers.TryAddWithoutValidation("origin", "https://mapleisland.nexon.com");
            request.Headers.TryAddWithoutValidation("referer", referer);
            if (!string.IsNullOrEmpty(userAgent))
            {
                request.Headers.TryAddWithoutValidation("User-Agent", userAgent);
            }
            if (!string.IsNullOrEmpty(accept))
            {
                request.Headers.TryAddWithoutValidation("accept", accept);
            }
        }

        private static string ExtractFirstCodeFromResponse(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            JObject root = JObject.Parse(json);
            JToken codesToken = root["data"]?["codes"];
            if (codesToken == null || !codesToken.HasValues)
            {
                return null;
            }

            return codesToken[0]?.Value<string>();
        }

        private static async Task<HttpClient> CreateAvatarHttpClientAsync()
        {
#if NET8_0_OR_GREATER
            await TlsLibraryLoader.EnsureLibraryExistsAsync();
            var tlsHandler = new TlsClientHandler
            {
                TlsClientIdentifier = ClientIdentifier.Chrome133
            };
            return new HttpClient(tlsHandler);
#else
            return new HttpClient();
#endif
        }

        private async Task ShowImageMakAsync(string code)
        {
            try
            {
                btnGenerate.Text = "生成中";
                string bodyJson = BuildAvatarImageRequestBody(string.IsNullOrEmpty(code) ? csvCode : code);
                using (HttpClient client = await CreateAvatarHttpClientAsync())
                {
                    using (var request = new HttpRequestMessage(HttpMethod.Post, "https://mapleisland.nexon.com/character/api/avatar-imagecode"))
                    {
                        AddCompatibleHeaders(
                            request,
                            "https://mapleisland.nexon.com/",
                            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/136.0.0.0 Safari/537.36");

                        request.Content = new StringContent(bodyJson, Encoding.UTF8);
                        request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                        using (var response = await client.SendAsync(request))
                        {
                            response.EnsureSuccessStatusCode();
                            string responseText = await response.Content.ReadAsStringAsync();
                            avatarCode = ExtractFirstCodeFromResponse(responseText);
                            if (string.IsNullOrEmpty(avatarCode))
                            {
                                return;
                            }

                            string renderUrl = $"https://mak.nexon.com/render/maplestory/character/code/{avatarCode}?width=256&height=256";
                            using (var imageRequest = new HttpRequestMessage(HttpMethod.Get, renderUrl))
                            {
                                AddCompatibleHeaders(
                                    imageRequest,
                                    "https://mapleisland.nexon.com/",
                                    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/136.0.0.0 Safari/537.36",
                                    "image/avif,image/webp,image/apng,image/svg+xml,image/*,*/*;q=0.8");

                                using (var imageResponse = await client.SendAsync(imageRequest))
                                {
                                    imageResponse.EnsureSuccessStatusCode();
                                    byte[] imageBytes = await imageResponse.Content.ReadAsByteArrayAsync();

                                    using (MemoryStream ms = new MemoryStream(imageBytes))
                                    {
                                        Image img = Image.FromStream(ms);
                                        picPreview.Image = img;
                                        btnGenerate.Text = "生成";
                                        btnGenerate.Enabled = true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                btnGenerate.Text = "生成失敗";
            }
        }
    }
}
