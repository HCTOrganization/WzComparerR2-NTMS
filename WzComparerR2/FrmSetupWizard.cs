using DevComponents.DotNetBar;
using DevComponents.Editors;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Resources;
using System.Text;
using System.Windows.Forms;
using WzComparerR2.Config;

namespace WzComparerR2
{
    public partial class FrmSetupWizard : DevComponents.DotNetBar.Office2007Form
    {
        public FrmSetupWizard()
        {
#if NET6_0_OR_GREATER
            // https://learn.microsoft.com/en-us/dotnet/core/compatibility/fx-core#controldefaultfont-changed-to-segoe-ui-9pt
            this.Font = new Font(new FontFamily("SimSun"), 9f);
#endif
            InitializeComponent();
            this.wizard.HeaderImage = null;
            this.picPreview.Image = null;

            this.translateLabelX5.Enabled = chkEnableTranslate.Checked;
            this.buttonX3.Enabled = chkEnableTranslate.Checked;
            this.buttonXCheck2.Enabled = chkEnableTranslate.Checked;
            this.cmbMozhiBackend.Enabled = chkEnableTranslate.Checked;
            this.cmbLanguageModel.Enabled = chkEnableTranslate.Checked;
            this.translateLabelX6.Enabled = chkEnableTranslate.Checked;
            this.cmbDesiredLanguage.Enabled = chkEnableTranslate.Checked;
            this.translateLabelX7.Enabled = chkEnableTranslate.Checked;
            this.translateLabelX8.Enabled = chkEnableTranslate.Checked;
            this.cmbPreferredTranslateEngine.Enabled = chkEnableTranslate.Checked;
            this.cmbPreferredLayout.Enabled = chkEnableTranslate.Checked;
            this.translateLabelX9.Enabled = chkEnableTranslate.Checked;
            this.cmbDetectCurrency.Enabled = chkEnableTranslate.Checked;
            this.translateLabelX10.Enabled = chkEnableTranslate.Checked;
            this.translateLabelX11.Enabled = chkEnableTranslate.Checked;
            this.cmbDesiredCurrency.Enabled = chkEnableTranslate.Checked;
            this.cmbMozhiBackend.Enabled = false;

            cmbDesiredLanguage.Items.AddRange(new[]
            {
                new ComboItem("繁體中文 (TMS)"){ Value = "zh-TW" },
                new ComboItem("韓文 (KMS)"){ Value = "ko" },
                new ComboItem("簡體中文 (CMS)"){ Value = "zh-CN" },
                new ComboItem("日文 (JMS)"){ Value = "ja" },
                new ComboItem("英文 (GMS/MSEA)"){ Value = "en" },
                new ComboItem("粵文 (HKMS)"){ Value = "yue" },
            });

            cmbMozhiBackend.Items.AddRange(new[]
            {
                new ComboItem("mozhi.aryak.me"){ Value = "https://mozhi.aryak.me" },
                new ComboItem("translate.bus-hit.me"){ Value = "https://translate.bus-hit.me" },
                new ComboItem("nyc1.mz.ggtyler.dev"){ Value = "https://nyc1.mz.ggtyler.dev" },
                new ComboItem("translate.projectsegfau.lt"){ Value = "https://translate.projectsegfau.lt" },
                new ComboItem("translate.nerdvpn.de"){ Value = "https://translate.nerdvpn.de" },
                new ComboItem("mozhi.ducks.party"){ Value = "https://mozhi.ducks.party" },
                new ComboItem("mozhi.frontendfriendly.xyz"){ Value = "https://mozhi.frontendfriendly.xyz" },
                new ComboItem("mozhi.pussthecat.org"){ Value = "https://mozhi.pussthecat.org" },
                new ComboItem("mo.zorby.top"){ Value = "https://mo.zorby.top" },
                new ComboItem("mozhi.adminforge.de"){ Value = "https://mozhi.adminforge.de" },
                new ComboItem("translate.privacyredirect.com"){ Value = "https://translate.privacyredirect.com" },
                new ComboItem("mozhi.canine.tools"){ Value = "https://mozhi.canine.tools" },
                new ComboItem("mozhi.gitro.xyz"){ Value = "https://mozhi.gitro.xyz" },
                new ComboItem("api.hikaricalyx.com"){ Value = "https://api.hikaricalyx.com/mozhi" },
            });

            cmbPreferredTranslateEngine.Items.AddRange(new[]
            {
                new ComboItem("Google (非Mozhi)"){ Value = 0 },
                new ComboItem("Google"){ Value = 1 },
                new ComboItem("DeepL"){ Value = 2 },
                new ComboItem("DuckDuckGo / Bing"){ Value = 3 },
                new ComboItem("MyMemory"){ Value = 4 },
                new ComboItem("Yandex"){ Value = 5 },
                new ComboItem("Naver Papago (非Mozhi)"){ Value = 6 },
                new ComboItem("OpenAI相容"){ Value = 9 },
            });

            cmbPreferredLayout.Items.AddRange(new[]
            {
                new ComboItem("先譯文后原文"){ Value = 1 },
                new ComboItem("先原文后譯文"){ Value = 2 },
                new ComboItem("僅顯示譯文"){ Value = 3 },
                new ComboItem("不翻譯"){ Value = 0 },
            });

            cmbDetectCurrency.Items.AddRange(new[]
            {
                new ComboItem("自動檢測"){ Value = "auto" },
                new ComboItem("美金 (USD)"){ Value = "usd" },
                new ComboItem("台幣 (NTD)"){ Value = "twd" },
                new ComboItem("韓幣 (KRW)"){ Value = "krw" },
                new ComboItem("新幣 (SGD)"){ Value = "sgd" },
                new ComboItem("日圓 (JPY)"){ Value = "jpy" },
                new ComboItem("人民幣 (CNY)"){ Value = "cny" },
            });

            cmbDesiredCurrency.Items.AddRange(new[]
            {
                new ComboItem("不變換"){ Value = "none" },
                new ComboItem("馬來林吉特 (MYR)"){ Value = "myr" },
                new ComboItem("美金 (USD)"){ Value = "usd" },
                new ComboItem("台幣 (NTD)"){ Value = "twd" },
                new ComboItem("港幣 (HKD)"){ Value = "hkd" },
                new ComboItem("韓幣 (KRW)"){ Value = "krw" },
                new ComboItem("加幣 (CAD)"){ Value = "cad" },
                new ComboItem("新幣 (SGD)"){ Value = "sgd" },
                new ComboItem("日圓 (JPY)"){ Value = "jpy" },
                new ComboItem("人民幣 (CNY)"){ Value = "cny" },
                new ComboItem("澳門幣 (MOP)"){ Value = "mop" },
                new ComboItem("澳洲幣 (AUD)"){ Value = "aud" },
                new ComboItem("歐元 (EUR)"){ Value = "eur" },
            });

            cmbLanguageModel.Items.AddRange(new[]
            {
                new ComboItem("保持現有"){ Value = "none" },
            });
        }

        private ResourceManager resMan = Properties.Resources.ResourceManager;

        private bool isCheckPassed = false;

        public bool isFirstRun { get; set; }

        public bool Enable22AniStyle
        {
            get { return chkEnable22AniStyle.Checked; }
            set { chkEnable22AniStyle.Checked = value; }
        }

        public bool EnableAutoPreview
        {
            get { return chkEnableAutoPreview.Checked; }
            set { chkEnableAutoPreview.Checked = value; }
        }

        public bool ShowObjectID
        {
            get { return chkShowID.Checked; }
            set { chkShowID.Checked = value; }
        }

        public bool ShowSoldPrice
        {
            get { return chkShowSoldPrice.Checked; }
            set { chkShowSoldPrice.Checked = value; }
        }

        public bool ShowPurchasePrice
        {
            get { return chkShowPurchasePrice.Checked; }
            set { chkShowPurchasePrice.Checked = value; }
        }

        public bool ShowMedalTag
        {
            get { return chkShowMedalTag.Checked; }
            set { chkShowMedalTag.Checked = value; }
        }

        public bool ShowNickTag
        {
            get { return chkShowNickTag.Checked; }
            set { chkShowNickTag.Checked = value; }
        }

        public bool ShowSkillDelay
        {
            get { return chkShowSkillDelay.Checked; }
            set { chkShowSkillDelay.Checked = value; }
        }

        public bool ShowSkillRange
        {
            get { return chkShowSkillRange.Checked; }
            set { chkShowSkillRange.Checked = value; }
        }

        public bool EnableTranslate
        {
            get { return chkEnableTranslate.Checked; }
            set { chkEnableTranslate.Checked = value; }
        }

        public int PreferredLayout
        {
            get
            {
                return ((cmbPreferredLayout.SelectedItem as ComboItem)?.Value as int?) ?? 0;
            }
            set
            {
                var items = cmbPreferredLayout.Items.Cast<ComboItem>();
                var item = items.FirstOrDefault(_item => _item.Value as int? == value)
                    ?? items.Last();
                item.Value = value;
                cmbPreferredLayout.SelectedItem = item;
            }
        }

        public string MozhiBackend
        {
            get
            {
                return ((cmbMozhiBackend.SelectedItem as ComboItem)?.Value as string) ?? "https://mozhi.aryak.me";
            }
            set
            {
                var items = cmbMozhiBackend.Items.Cast<ComboItem>();
                var item = items.FirstOrDefault(_item => _item.Value as string == value)
                    ?? items.Last();
                item.Value = value;
                cmbMozhiBackend.SelectedItem = item;
            }
        }

        public string LanguageModel
        {
            get
            {
                return ((cmbLanguageModel.SelectedItem as ComboItem)?.Value as string) ?? "";
            }
            set
            {
                var items = cmbLanguageModel.Items.Cast<ComboItem>();
                var item = items.FirstOrDefault(_item => _item.Value as string == value)
                    ?? items.Last();
                item.Value = value;
                cmbLanguageModel.SelectedItem = item;
            }
        }

        public bool OpenAIExtraOption
        {
            get { return chkOpenAIExtraOption.Checked; }
            set { chkOpenAIExtraOption.Checked = value; }
        }

        public double LMTemperature
        {
            get
            {
                return Double.Parse(txtLMTemperature.Text);
            }
            set
            {
                txtLMTemperature.Text = value.ToString();
            }
        }
        public int MaximumToken
        {
            get
            {
                return int.Parse(txtMaximumToken.Text);
            }
            set
            {
                txtMaximumToken.Text = value.ToString();
            }
        }

        public string DetectCurrency
        {
            get
            {
                return ((cmbDetectCurrency.SelectedItem as ComboItem)?.Value as string) ?? "auto";
            }
            set
            {
                var items = cmbDetectCurrency.Items.Cast<ComboItem>();
                var item = items.FirstOrDefault(_item => _item.Value as string == value)
                    ?? items.Last();
                item.Value = value;
                cmbDetectCurrency.SelectedItem = item;
            }
        }

        public string DesiredCurrency
        {
            get
            {
                return ((cmbDesiredCurrency.SelectedItem as ComboItem)?.Value as string) ?? "jpy";
            }
            set
            {
                var items = cmbDesiredCurrency.Items.Cast<ComboItem>();
                var item = items.FirstOrDefault(_item => _item.Value as string == value)
                    ?? items.Last();
                item.Value = value;
                cmbDesiredCurrency.SelectedItem = item;
            }
        }

        public int PreferredTranslateEngine
        {
            get
            {
                return ((cmbPreferredTranslateEngine.SelectedItem as ComboItem)?.Value as int?) ?? 0;
            }
            set
            {
                var items = cmbPreferredTranslateEngine.Items.Cast<ComboItem>();
                var item = items.FirstOrDefault(_item => _item.Value as int? == value)
                    ?? items.Last();
                item.Value = value;
                cmbPreferredTranslateEngine.SelectedItem = item;
            }
        }

        public string DesiredLanguage
        {
            get
            {
                return ((cmbDesiredLanguage.SelectedItem as ComboItem)?.Value as string) ?? "ja";
            }
            set
            {
                var items = cmbDesiredLanguage.Items.Cast<ComboItem>();
                var item = items.FirstOrDefault(_item => _item.Value as string == value)
                    ?? items.Last();
                item.Value = value;
                cmbDesiredLanguage.SelectedItem = item;
            }
        }

        public string OpenAIBackend
        {
            get { return txtOpenAIBackend.Text; }
            set { txtOpenAIBackend.Text = value; }
        }

        public string OpenAISystemMessage
        {
            get { return txtOpenAISystemMessage.Text; }
            set { txtOpenAISystemMessage.Text = value; }
        }

        public string NxSecretKey { get; set; }

        public string OpenAPIkey
        {
            get { return txtAPIkey.Text; }
            set { txtAPIkey.Text = value; }
        }

        private string ItemIDPriceSuffix(bool isShowID, bool isShowPrice)
        {
            return (isShowID ? "ShowID" : "HideID") + "_" + (isShowPrice ? "ShowPrice" : "HidePrice");
        }

        private string SkillIDDelayRangeSuffix(bool isShowID, bool isShowDelay, bool isShowRange)
        {
            return (isShowID ? "ShowID" : "HideID") + "_" + (isShowDelay ? "ShowDelay" : "HideDelay") + "_" + (isShowRange ? "ShowRange" : "HideRange");
        }

        private string GearIDMedalSuffix(bool isShowID, bool isShowMedal)
        {
            return (isShowID ? "ShowID" : "HideID") + "_" + (isShowMedal ? "ShowMedal" : "HideMedal");
        }

        private string ItemIDTitleSuffix(bool isShowID, bool isShowTitle)
        {
            return (isShowID ? "ShowID" : "HideID") + "_" + (isShowTitle ? "ShowTitle" : "HideTitle");
        }

        private void FrmSetupWizard_Load(object sender, EventArgs e)
        {
            if (isFirstRun) this.label2.Text += "\r\n\r\n這是您第一次運行首次運行精靈。如果按一下“取消”，則精靈將不再顯示。您可以稍後從「幫助」標籤存取該精靈。";

            //Set Default Values
        }

        private void cmbPreferredTranslateEngine_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboItem selectedItem = (ComboItem)cmbPreferredTranslateEngine.SelectedItem;
            switch ((int)selectedItem.Value)
            {
                case 0:
                case 6:
                    translateLabelX5.Visible = true;
                    translateLabelX5.Enabled = false;
                    translateLabelX11.Visible = false;
                    translateLabelX12.Visible = false;
                    translateLabelX13.Visible = false;
                    translateLabelX14.Visible = false;
                    translateLabelX15.Visible = false;
                    translateLabelX16.Visible = false;
                    txtOpenAIBackend.Visible = false;
                    txtOpenAISystemMessage.Visible = false;
                    txtAPIkey.Visible = false;
                    txtLMTemperature.Visible = false;
                    txtMaximumToken.Visible = false;
                    cmbMozhiBackend.Visible = true;
                    cmbMozhiBackend.Enabled = false;
                    cmbLanguageModel.Visible = false;
                    chkOpenAIExtraOption.Visible = false;
                    buttonXCheck2.Visible = false;
                    break;
                case 8:
                case 9:
                    translateLabelX5.Visible = false;
                    translateLabelX11.Visible = true;
                    translateLabelX12.Visible = true;
                    translateLabelX13.Visible = chkOpenAIExtraOption.Checked;
                    translateLabelX14.Visible = chkOpenAIExtraOption.Checked;
                    translateLabelX15.Visible = true;
                    translateLabelX16.Visible = true;
                    txtOpenAIBackend.Visible = true;
                    txtOpenAISystemMessage.Visible = true;
                    txtAPIkey.Visible = true;
                    txtLMTemperature.Visible = chkOpenAIExtraOption.Checked;
                    txtMaximumToken.Visible = chkOpenAIExtraOption.Checked;
                    cmbMozhiBackend.Visible = false;
                    cmbLanguageModel.Visible = true;
                    chkOpenAIExtraOption.Visible = true;
                    buttonXCheck2.Visible = true;
                    break;
                default:
                    translateLabelX5.Visible = true;
                    translateLabelX5.Enabled = true;
                    translateLabelX11.Visible = false;
                    translateLabelX12.Visible = false;
                    translateLabelX13.Visible = false;
                    translateLabelX14.Visible = false;
                    translateLabelX15.Visible = false;
                    translateLabelX16.Visible = false;
                    txtOpenAIBackend.Visible = false;
                    txtOpenAISystemMessage.Visible = false;
                    txtAPIkey.Visible = false;
                    txtLMTemperature.Visible = false;
                    txtMaximumToken.Visible = false;
                    cmbMozhiBackend.Visible = true;
                    cmbMozhiBackend.Enabled = true;
                    cmbLanguageModel.Visible = false;
                    chkOpenAIExtraOption.Visible = false;
                    buttonXCheck2.Visible = true;
                    break;
            }
        }

        private void chkOpenAIExtraOption_CheckedChanged(object sender, EventArgs e)
        {
            txtLMTemperature.Visible = chkOpenAIExtraOption.Checked && chkOpenAIExtraOption.Enabled;
            txtMaximumToken.Visible = chkOpenAIExtraOption.Checked && chkOpenAIExtraOption.Enabled;
            translateLabelX13.Visible = chkOpenAIExtraOption.Checked && chkOpenAIExtraOption.Enabled;
            translateLabelX14.Visible = chkOpenAIExtraOption.Checked && chkOpenAIExtraOption.Enabled;
            txtLMTemperature.Enabled = chkOpenAIExtraOption.Checked && chkOpenAIExtraOption.Enabled;
            txtMaximumToken.Enabled = chkOpenAIExtraOption.Checked && chkOpenAIExtraOption.Enabled;
            translateLabelX13.Enabled = chkOpenAIExtraOption.Checked && chkOpenAIExtraOption.Enabled;
            translateLabelX14.Enabled = chkOpenAIExtraOption.Checked && chkOpenAIExtraOption.Enabled;
        }

        private void wizard_WizardPageChanging(object sender, WizardCancelPageChangeEventArgs e)
        {
            if (e.OldPage == translatorWizardPage && e.PageChangeSource == eWizardPageChangeSource.NextButton)
            {
                ComboItem selectedEngine = (ComboItem)cmbPreferredTranslateEngine.SelectedItem;
                ComboItem selectedLM = (ComboItem)cmbLanguageModel.SelectedItem;
                if (chkEnableTranslate.Checked && !this.isCheckPassed)
                {
                    switch ((int)selectedEngine.Value)
                    {
                        case 7:
                            MessageBoxEx.Show("在繼續之前，請檢查您的 Papago API 設定。");
                            break;
                        case 9:
                            MessageBoxEx.Show("在繼續之前，請檢查您的 OpenAI API 設定。");
                            break;
                    }
                    e.Cancel = true;
                }
            }
        }

        private void wizard_FinishButtonClick(object sender, CancelEventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void chkSet1_MouseHover(object sender, EventArgs e)
        {
            this.picPreview.Image = (Image)resMan.GetObject((Enable22AniStyle ? "NewGear_" : "OldGear_") + ItemIDPriceSuffix(ShowObjectID, ShowSoldPrice));

        }

        private void chkSet1_CheckedChanged(object sender, EventArgs e)
        {
            if (this.picPreview.Image != null)
            {
                chkSet1_MouseHover(sender, e);
            }
        }

        private void chkSet2_MouseHover(object sender, EventArgs e)
        {
            this.picPreview.Image = (Image)resMan.GetObject((Enable22AniStyle ? "NewItem_" : "OldItem_") + ItemIDPriceSuffix(ShowObjectID, ShowPurchasePrice));
        }

        private void chkSet2_CheckedChanged(object sender, EventArgs e)
        {
            if (this.picPreview.Image != null)
            {
                chkSet2_MouseHover(sender, e);
            }
        }

        private void chkSet3_MouseHover(object sender, EventArgs e)
        {
            this.picPreview.Image = (Image)resMan.GetObject((Enable22AniStyle ? "NewSkill_" : "OldSkill_") + SkillIDDelayRangeSuffix(ShowObjectID, ShowSkillDelay, ShowSkillRange));
        }

        private void chkSet3_CheckedChanged(object sender, EventArgs e)
        {
            if (this.picPreview.Image != null)
            {
                chkSet3_MouseHover(sender, e);
            }
        }

        private void chkSet4_MouseHover(object sender, EventArgs e)
        {
            this.picPreview.Image = (Image)resMan.GetObject("OldGear_" + GearIDMedalSuffix(ShowObjectID, ShowMedalTag));
        }

        private void chkSet4_CheckedChanged(object sender, EventArgs e)
        {
            if (this.picPreview.Image != null)
            {
                chkSet4_MouseHover(sender, e);
            }
        }

        private void chkSet5_MouseHover(object sender, EventArgs e)
        {
            this.picPreview.Image = (Image)resMan.GetObject((Enable22AniStyle ? "NewItem_" : "OldItem_") + ItemIDTitleSuffix(ShowObjectID, ShowNickTag));
        }

        private void chkSet5_CheckedChanged(object sender, EventArgs e)
        {
            if (this.picPreview.Image != null)
            {
                chkSet5_MouseHover(sender, e);
            }
        }

        private void chkEnableTranslate_CheckedChanged(object sender, EventArgs e)
        {
            this.translateLabelX5.Enabled = chkEnableTranslate.Checked;
            this.buttonX3.Enabled = chkEnableTranslate.Checked;
            this.buttonXCheck2.Enabled = chkEnableTranslate.Checked;
            this.cmbMozhiBackend.Enabled = chkEnableTranslate.Checked;
            this.cmbLanguageModel.Enabled = chkEnableTranslate.Checked;
            this.translateLabelX6.Enabled = chkEnableTranslate.Checked;
            this.cmbDesiredLanguage.Enabled = chkEnableTranslate.Checked;
            this.translateLabelX7.Enabled = chkEnableTranslate.Checked;
            this.translateLabelX8.Enabled = chkEnableTranslate.Checked;
            this.cmbPreferredTranslateEngine.Enabled = chkEnableTranslate.Checked;
            this.cmbPreferredLayout.Enabled = chkEnableTranslate.Checked;
            this.translateLabelX9.Enabled = chkEnableTranslate.Checked;
            this.cmbDetectCurrency.Enabled = chkEnableTranslate.Checked;
            this.translateLabelX10.Enabled = chkEnableTranslate.Checked;
            this.translateLabelX11.Enabled = chkEnableTranslate.Checked;
            this.cmbDesiredCurrency.Enabled = chkEnableTranslate.Checked;
            this.translateLabelX12.Visible = chkEnableTranslate.Checked;
            this.translateLabelX13.Visible = chkEnableTranslate.Checked && chkOpenAIExtraOption.Checked;
            this.translateLabelX14.Visible = chkEnableTranslate.Checked && chkOpenAIExtraOption.Checked;
            this.translateLabelX15.Visible = chkEnableTranslate.Checked;
            this.translateLabelX16.Visible = chkEnableTranslate.Checked;
            this.txtOpenAIBackend.Visible = chkEnableTranslate.Checked;
            this.txtOpenAISystemMessage.Visible = chkEnableTranslate.Checked;
            this.txtLMTemperature.Visible = chkEnableTranslate.Checked && chkOpenAIExtraOption.Checked;
            this.txtMaximumToken.Visible = chkEnableTranslate.Checked && chkOpenAIExtraOption.Checked;
            this.cmbMozhiBackend.Enabled = chkEnableTranslate.Checked;
            this.chkOpenAIExtraOption.Visible = chkEnableTranslate.Checked;
            this.buttonXCheck2.Visible = chkEnableTranslate.Checked;
            this.txtAPIkey.Visible = chkEnableTranslate.Checked;
        }

        private void buttonXCheck2_Click(object sender, EventArgs e)
        {
            ComboItem selectedItem = (ComboItem)cmbPreferredTranslateEngine.SelectedItem;
            string respText = "";
            HttpWebRequest req;
            string backendAddress;
            if (string.IsNullOrEmpty(OpenAIBackend))
            {
                backendAddress = txtOpenAIBackend.WatermarkText;
            }
            else
            {
                backendAddress = OpenAIBackend;
            }
            switch ((int)selectedItem.Value)
            {
                case 8:
                case 9:
                    req = WebRequest.Create(backendAddress + "/models") as HttpWebRequest;
                    req.Timeout = 15000;
                    if (!string.IsNullOrEmpty(this.OpenAPIkey))
                    {
                        req.Headers.Add("Authorization", "Bearer " + this.OpenAPIkey);
                    }
                    try
                    {
                        string respJson = new StreamReader(req.GetResponse().GetResponseStream(), Encoding.UTF8).ReadToEnd();
                        JObject jsonResp = JObject.Parse(respJson);
                        JArray dataArray = (JArray)jsonResp["data"];
                        StringBuilder sb = new StringBuilder();
                        cmbLanguageModel.Items.Clear();
                        foreach (JObject dataItem in dataArray)
                        {
                            sb.AppendLine(dataItem["id"].ToString());
                            cmbLanguageModel.Items.Add(new ComboItem(dataItem["id"].ToString()) { Value = dataItem["id"].ToString() });
                        }
                        cmbLanguageModel.SelectedIndex = 0;
                        respText = sb.ToString();
                        this.isCheckPassed = true;
                    }
                    catch
                    {
                        respText = "該 API 設定無效。";
                    }
                    break;
            }
            MessageBoxEx.Show(respText);
        }

        private void parseAPIkey()
        {
            JObject testJObject;
            Dictionary<string, string> Headers = new Dictionary<string, string>();
            try
            {
                testJObject = JObject.Parse(NxSecretKey);
                foreach (var property in testJObject.Properties()) Headers.Add(property.Name, property.Value.ToString());
                if (Headers.ContainsKey("Authorization")) this.OpenAPIkey = Headers["Authorization"].Replace("Bearer ", "");
            }
            catch
            {
            }
        }

        private void saveAPIkey()
        {
            ComboItem selectedItem = (ComboItem)cmbPreferredTranslateEngine.SelectedItem;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("{");
            switch ((int)selectedItem.Value)
            {
                case 6:
                    break;
                case 8:
                case 9:
                    sb.AppendLine("    \"Authorization\": \"" + this.OpenAPIkey + "\"");
                    break;
                default:
                    return;
            }
            sb.AppendLine("}");
            this.NxSecretKey = sb.ToString();
        }

        public void Load(WcR2Config mainconfig, CharaSimConfig characonfig)
        {
            this.PreferredLayout = mainconfig.PreferredLayout;
            this.EnableTranslate = !(this.PreferredLayout == 0);
            this.NxSecretKey = mainconfig.NxSecretKey;
            this.MozhiBackend = mainconfig.MozhiBackend;
            this.LanguageModel = mainconfig.LanguageModel;
            this.OpenAIBackend = mainconfig.OpenAIBackend;
            this.OpenAIExtraOption = mainconfig.OpenAIExtraOption;
            this.OpenAISystemMessage = mainconfig.OpenAISystemMessage;
            this.LMTemperature = mainconfig.LMTemperature;
            this.MaximumToken = mainconfig.MaximumToken;
            this.PreferredTranslateEngine = mainconfig.PreferredTranslateEngine;
            this.DesiredLanguage = mainconfig.DesiredLanguage;
            this.DetectCurrency = mainconfig.DetectCurrency;
            this.DesiredCurrency = mainconfig.DesiredCurrency;
            this.NxSecretKey = mainconfig.NxSecretKey;
            parseAPIkey();
            this.Enable22AniStyle = characonfig.Enable22AniStyle;
            this.EnableAutoPreview = characonfig.AutoQuickView;
            this.ShowObjectID = characonfig.Gear.ShowID || characonfig.Item.ShowID || characonfig.Skill.ShowID || characonfig.Recipe.ShowID || characonfig.Map.ShowMapObjectID || characonfig.Map.ShowMobNpcObjectID;
            this.ShowSoldPrice = characonfig.Gear.ShowSoldPrice || characonfig.Item.ShowSoldPrice;
            this.ShowPurchasePrice = characonfig.Gear.ShowCashPurchasePrice || characonfig.Item.ShowCashPurchasePrice;
            this.ShowMedalTag = characonfig.Gear.ShowMedalTag;
            this.ShowNickTag = characonfig.Item.ShowNickTag;
            this.ShowSkillDelay = characonfig.Skill.ShowDelay;
            this.ShowSkillRange = characonfig.Skill.ShowRangeCoordinates;
        }

        public void Save(WcR2Config mainconfig, CharaSimConfig characonfig)
        {
            if (EnableTranslate)
            {
                mainconfig.PreferredLayout = this.PreferredLayout;
                mainconfig.MozhiBackend = this.MozhiBackend;
                if (this.LanguageModel != "none") mainconfig.LanguageModel = this.LanguageModel;
                mainconfig.OpenAIBackend = this.OpenAIBackend;
                mainconfig.OpenAIExtraOption = this.OpenAIExtraOption;
                mainconfig.OpenAISystemMessage = this.OpenAISystemMessage;
                mainconfig.LMTemperature = this.LMTemperature;
                mainconfig.MaximumToken = this.MaximumToken;
                mainconfig.PreferredTranslateEngine = this.PreferredTranslateEngine;
                mainconfig.DesiredLanguage = this.DesiredLanguage;
                mainconfig.DetectCurrency = this.DetectCurrency;
                mainconfig.DesiredCurrency = this.DesiredCurrency;
                saveAPIkey();
                mainconfig.NxSecretKey = this.NxSecretKey;
            }
            else
            {
                mainconfig.PreferredLayout = 0;
            }
            characonfig.Enable22AniStyle = this.Enable22AniStyle;
            characonfig.AutoQuickView = this.EnableAutoPreview;
            characonfig.Gear.ShowID = this.ShowObjectID;
            characonfig.Item.ShowID = this.ShowObjectID;
            characonfig.Skill.ShowID = this.ShowObjectID;
            characonfig.Recipe.ShowID = this.ShowObjectID;
            characonfig.Map.ShowMapObjectID = this.ShowObjectID;
            characonfig.Map.ShowMobNpcObjectID = this.ShowObjectID;
            characonfig.Gear.ShowSoldPrice = this.ShowSoldPrice;
            characonfig.Item.ShowSoldPrice = this.ShowSoldPrice;
            characonfig.Gear.ShowCashPurchasePrice = this.ShowPurchasePrice;
            characonfig.Item.ShowCashPurchasePrice = this.ShowPurchasePrice;
            characonfig.Gear.ShowMedalTag = this.ShowMedalTag;
            characonfig.Item.ShowNickTag = this.ShowNickTag;
            characonfig.Skill.ShowDelay = this.ShowSkillDelay;
            characonfig.Skill.ShowRangeCoordinates = this.ShowSkillRange;
        }
    }
}