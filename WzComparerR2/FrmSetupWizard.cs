using DevComponents.DotNetBar;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Resources;
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
            this.Font = new Font(new FontFamily("MS PGothic"), 9f);
#endif
            InitializeComponent();
            this.wizard.HeaderImage = null;
            this.picPreview.Image = null;
        }

        private ResourceManager resMan = Properties.Resources.ResourceManager;

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
            if (isFirstRun) this.label2.Text += "\r\n\r\n初回実行ウィザードを実行するのは今回が初めてです。「キャンセル」をクリックすると、次回からはウィザードが表示されなくなります。このウィザードには、後で「ヘルプ」タブからアクセスできます。";

            //Set Default Values
        }

        private void wizard_WizardPageChanging(object sender, WizardCancelPageChangeEventArgs e)
        {
            if (e.OldPage == translatorWizardPage && e.PageChangeSource == eWizardPageChangeSource.NextButton)
            {
                // if (NameTextBoxX.Text == String.Empty || entryIDIntegerInput.Value == 0 || displayIDIntegerInput.Value == 0)
                if (false)
                {
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

        public void Load(WcR2Config mainconfig, CharaSimConfig characonfig)
        {
            // this.PreferredLayout = mainconfig.PreferredLayout;
            // if (this.PreferredLayout == 0) this.EnableTranslate = false;
            /*            this.NxOpenAPIKey = mainconfig.NxOpenAPIKey;
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
                        this.PreferredStringCopyMethod = characonfig.PreferredStringCopyMethod;
                        this.CopyParsedSkillString = characonfig.CopyParsedSkillString;*/
            this.Enable22AniStyle = characonfig.Enable22AniStyle;
            this.EnableAutoPreview = characonfig.AutoQuickView;
            this.ShowObjectID = characonfig.Gear.ShowID || characonfig.Item.ShowID || characonfig.Skill.ShowID || characonfig.Recipe.ShowID;
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
                // mainconfig.PreferredLayout = this.PreferredLayout;
            }
            else
            {
                mainconfig.PreferredLayout = 0;
            }
                /*            mainconfig.NxOpenAPIKey = this.NxOpenAPIKey;
                            mainconfig.NxSecretKey = this.NxSecretKey;
                            mainconfig.MozhiBackend = this.MozhiBackend;
                            if (this.LanguageModel != "none") config.LanguageModel = this.LanguageModel;
                            mainconfig.OpenAIBackend = this.OpenAIBackend;
                            mainconfig.OpenAIExtraOption = this.OpenAIExtraOption;
                            mainconfig.OpenAISystemMessage = this.OpenAISystemMessage;
                            mainconfig.LMTemperature = this.LMTemperature;
                            mainconfig.MaximumToken = this.MaximumToken;
                            mainconfig.PreferredTranslateEngine = this.PreferredTranslateEngine;
                            mainconfig.DesiredLanguage = this.DesiredLanguage;
                            mainconfig.DetectCurrency = this.DetectCurrency;
                            mainconfig.DesiredCurrency = this.DesiredCurrency;
                            characonfig.PreferredStringCopyMethod = this.PreferredStringCopyMethod;
                            characonfig.CopyParsedSkillString = this.CopyParsedSkillString;*/
                characonfig.Enable22AniStyle = this.Enable22AniStyle;
            characonfig.AutoQuickView = this.EnableAutoPreview;
            characonfig.Gear.ShowID = this.ShowObjectID;
            characonfig.Item.ShowID = this.ShowObjectID;
            characonfig.Skill.ShowID = this.ShowObjectID;
            characonfig.Recipe.ShowID = this.ShowObjectID;
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