using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevComponents.Editors;
using WzComparerR2.CharaSim;

#if NET6_0_OR_GREATER
using WzComparerR2.OpenAPI;
#endif

namespace WzComparerR2
{
    /// <summary>
    /// Dialog that fetches an avatar code from a real character.
    /// </summary>
    public partial class FrmAvatarImport : DevComponents.DotNetBar.Office2007Form
    {
        public FrmAvatarImport()
            : this(false)
        {
        }

        public FrmAvatarImport(bool isDarkMode)
        {
#if NET6_0_OR_GREATER
            // https://learn.microsoft.com/en-us/dotnet/core/compatibility/fx-core#controldefaultfont-changed-to-segoe-ui-9pt
            this.Font = new Font(new FontFamily("SimSun"), 9f);
#endif
            InitializeComponent();
            this.DarkMode = isDarkMode;
            if (isDarkMode)
            {
                this.textBoxX1.BackColor = DarkBackgroundColor;
                this.textBoxX1.ForeColor = Color.LightGray;
            }

            cmbRegion.Items.AddRange(new[]
            {
                new ComboItem("KMS"){ Value = 1 },
                new ComboItem("JMS"){ Value = 2 },
                new ComboItem("GMS北美"){ Value = 4 },
                new ComboItem("GMS歐洲"){ Value = 5 },
                new ComboItem("MSEA"){ Value = 6 },
                new ComboItem("TMS"){ Value = 7 },
                new ComboItem("MSN"){ Value = 8 },
            });
            cmbRegion.SelectedIndex = 1;
        }

        private bool DarkMode;
        private static readonly Color DarkBackgroundColor = Color.FromArgb(-13816528);

        /// <summary>
        /// Avatar code that was fetched.
        /// </summary>
        public string ResultCode { get; private set; }

        public string CharaName
        {
            get { return textBoxX1.Text; }
            set { textBoxX1.Text = value; }
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
                cmbRegion.SelectedItem = item;
            }
        }

        private bool IsFullWidth(char c)
        {
            return (c >= 0xFF01 && c <= 0xFF5E) || // Full-width ASCII
            (c >= 0x3000 && c <= 0x30FF) || // CJK symbols & kana
            (c >= 0x4E00 && c <= 0x9FFF) || // Common CJK ideographs
            (c >= 0xF900 && c <= 0xFAFF);   // CJK Compatibility
        }

        private bool IsEmoji(char c)
        {
            return char.GetUnicodeCategory(c) == System.Globalization.UnicodeCategory.OtherSymbol;
        }

        private void textBoxX1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                this.btnOK.PerformClick();
            }
        }

        private void textBoxX1_TextChanged(object sender, EventArgs e)
        {
            if (selectedRegion < 9)
            {
                string text = textBoxX1.Text;
                int fullWidthCount = 0;
                int halfWidthCount = 0;
                string filteredText = "";

                foreach (char c in text)
                {
                    if (IsEmoji(c))
                        continue;
                    else if (IsFullWidth(c))
                        fullWidthCount++;
                    else
                        halfWidthCount++;

                    filteredText += c;
                }

                if (fullWidthCount * 2 + halfWidthCount > 12)
                {
                    filteredText = filteredText.Substring(0, filteredText.Length - 1);
                }

                if (textBoxX1.Text != filteredText)
                {
                    int cursorPos = textBoxX1.SelectionStart;
                    textBoxX1.Text = filteredText;
                    textBoxX1.SelectionStart = Math.Min(cursorPos, textBoxX1.Text.Length);
                }
            }

            if (Translator.IsKoreanStringPresent(textBoxX1.Text))
            {
                textBoxX1.Font = new Font("Dotum", 9f);
            }
            else
            {
                textBoxX1.Font = new Font("SimSun", 9f);
            }
            btnOK.Enabled = textBoxX1.Text.Length > 0 && cmbRegion.SelectedItem != null;
        }

        private void cmbRegion_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnOK.Enabled = textBoxX1.Text.Length > 0 && cmbRegion.SelectedItem != null;
            textBoxX1.MaxLength = 12;
            ComboItem selectedItem = (ComboItem)cmbRegion.SelectedItem;
            if (selectedItem == null)
            {
                return;
            }
        }

#if NET6_0_OR_GREATER
        private async void btnOK_Click(object sender, EventArgs e)
#else
        private void btnOK_Click(object sender, EventArgs e)
#endif
        {
            this.ResultCode = null;
            this.btnOK.Enabled = false;

#if NET6_0_OR_GREATER
            string characterName = this.CharaName;
            int region = this.selectedRegion;
            labelX1.Text = "正在取得角色外觀，請稍候...";

            string code = null;
            string error = null;
            try
            {
                code = await FetchAvatarCodeAsync(characterName, region);
            }
            catch (Exception ex)
            {
                error = ex.Message;
            }

            if (string.IsNullOrEmpty(code))
            {
                labelX1.Text = error == null
                    ? "找不到角色。"
                    : "找不到角色。 (" + error + ")";
                this.btnOK.Enabled = true;
                return;
            }

            this.ResultCode = code;
            this.DialogResult = DialogResult.OK;
            this.Close();
#else
            labelX1.Text = "此功能需要使用 .NET 6.0 或 .NET 8.0 版本。";
            this.btnOK.Enabled = true;
#endif
        }

#if NET6_0_OR_GREATER
        /// <summary>
        /// Fetches the avatar code for the given character name. Returns null when the character is not found.
        /// </summary>
        private static async Task<string> FetchAvatarCodeAsync(string characterName, int region)
        {
            var api = new NexonOpenAPI("-", "KMS");
            string avatarCode;

            switch (region)
            {
                case 9: // custom 3: raw avatar code
                    return BuildAvatarCode(await api.ParseAvatarCode(characterName));

                case 10: // custom 4: TMS cipher text
                    return BuildAvatarCode(await api.ParseCharacterLookCipherText(characterName));

                case 1: avatarCode = await api.GetAvatarCode(characterName, "ChangseopMS"); break;
                case 2: avatarCode = await api.GetAvatarCode(characterName, "ZipanguMS"); break;
                case 4: avatarCode = await api.GetAvatarCode(characterName, "InkwellMS_NA"); break;
                case 5: avatarCode = await api.GetAvatarCode(characterName, "InkwellMS_EU"); break;
                case 6: avatarCode = await api.GetAvatarCode(characterName, "TerryMS_Peninsula"); break;
                case 7: avatarCode = await api.GetAvatarCode(characterName, "TerryMS_Island"); break;
                case 8: avatarCode = await api.GetAvatarCode(characterName, "TunerMS"); break;
                default: return null;
            }

            if (string.IsNullOrEmpty(avatarCode))
            {
                return null;
            }

            // MSN uses its own format, so decode it into a list of item codes
            if (region == 8)
            {
                string[] decodedInfo = Encoding.UTF8.GetString(Convert.FromBase64String(avatarCode)).Split('a');
                var msnCode = new List<int>
                {
                    int.Parse(decodedInfo[1]) + 2000,
                    int.Parse(decodedInfo[1]) + 12000,
                };
                foreach (string itemCode in decodedInfo.Skip(2))
                {
                    switch (itemCode.Length)
                    {
                        case 5:
                        case 7:
                            msnCode.Add(int.Parse(itemCode));
                            break;
                        case 8:
                            msnCode.Add(int.Parse(itemCode.Substring(0, 5)));
                            break;
                    }
                }
                msnCode.Sort();
                return string.Join(",", msnCode);
            }

            return BuildAvatarCode(await api.ParseAvatarCode(avatarCode));
        }

        /// <summary>
        /// Converts UnpackedAvatarData into an avatar code string.
        /// </summary>
        private static string BuildAvatarCode(UnpackedAvatarData res)
        {
            if (res == null)
            {
                return null;
            }

            var mixFace = int.Parse(res.MixFaceRatio) != 0 ? $"+{res.MixFaceColor}*{res.MixFaceRatio}" : "";
            var mixHair = int.Parse(res.MixHairRatio) != 0 ? $"+{res.MixHairColor}*{res.MixHairRatio}" : "";

            var code = $"20{res.Skin}, 120{res.Skin + GetPrismCode(res.SkinPrismInfo)}, {res.Face + mixFace}, {res.Hair + mixHair}," +
                $"{res.Cap + GetPrismCode(res.CapPrismInfo)}," +
                $"{res.FaceAcc}," +
                $"{res.EyeAcc}," +
                $"{res.EarAcc}," +
                $"{res.Coat + GetPrismCode(res.CoatPrismInfo)}," +
                $"{res.Pants + GetPrismCode(res.PantsPrismInfo)}," +
                $"{res.Shoes + GetPrismCode(res.ShoesPrismInfo)}," +
                $"{res.Gloves + GetPrismCode(res.GlovesPrismInfo)}," +
                $"{res.Cape + GetPrismCode(res.CapePrismInfo)}," +
                $"{res.Shield}," +
                $"{res.Weapon}," +
                $"{res.CashWeapon + GetPrismCode(res.WeaponPrismInfo)}";

            var customOrigins = GetCustomOriginCode(res);
            if (!string.IsNullOrEmpty(customOrigins))
            {
                code += "," + customOrigins;
            }

            return code;
        }

        private static string GetPrismCode(PrismInfo prism, bool alwaysReturnCode = false)
        {
            if (prism != null && prism.Valid)
            {
                if (prism.Brightness > 0 && prism.Saturation > 0 && prism.Hue >= 0)
                {
                    return $"+{prism.ColorType}h{prism.Hue}s{prism.Saturation}v{prism.Brightness}{(prism.ConvertPureBlack ? "PB" : "")}";
                }
                return "+0h0s100v100";
            }
            return "";
        }

        private static string GetPrismCode(PrismInfoCollection prisms)
        {
            if (prisms == null)
            {
                return "";
            }
            return GetPrismCode(prisms.Prism1, true) + GetPrismCode(prisms.Prism2);
        }

        private static string GetCustomOriginCode(UnpackedAvatarData data)
        {
            if (data.CustomOrigin == null)
            {
                return "";
            }

            var ret = new List<string>();
            for (int i = 0; i < data.CustomOrigin.Length; i++)
            {
                CustomOriginInfo cur = data.CustomOrigin[i];
                if (cur.Valid)
                {
                    ret.Add($"co{i}x{cur.Origin.X}y{cur.Origin.Y}");
                }
            }
            return string.Join(",", ret);
        }
#endif
    }
}
