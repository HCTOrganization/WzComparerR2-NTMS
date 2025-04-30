using System.Windows.Forms;
using WzComparerR2.Properties;

namespace WzComparerR2
{
    partial class FrmSetupWizard
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.wizard = new DevComponents.DotNetBar.Wizard();
            this.wizardPage1 = new DevComponents.DotNetBar.WizardPage();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.previewCommonWizardPage = new DevComponents.DotNetBar.WizardPage();
            this.chkEnable22AniStyle = new DevComponents.DotNetBar.Controls.CheckBoxX();
            this.chkEnableAutoPreview = new DevComponents.DotNetBar.Controls.CheckBoxX();
            this.chkShowID = new DevComponents.DotNetBar.Controls.CheckBoxX();
            this.chkShowSoldPrice = new DevComponents.DotNetBar.Controls.CheckBoxX();
            this.chkShowPurchasePrice = new DevComponents.DotNetBar.Controls.CheckBoxX();
            this.chkShowSkillDelay = new DevComponents.DotNetBar.Controls.CheckBoxX();
            this.chkShowSkillRange = new DevComponents.DotNetBar.Controls.CheckBoxX();
            this.chkShowMedalTag = new DevComponents.DotNetBar.Controls.CheckBoxX();
            this.chkShowNickTag = new DevComponents.DotNetBar.Controls.CheckBoxX();
            this.picPreview = new System.Windows.Forms.PictureBox();
            this.translatorWizardPage = new DevComponents.DotNetBar.WizardPage();
            this.chkEnableTranslate = new DevComponents.DotNetBar.Controls.CheckBoxX();
            this.chkOpenAIExtraOption = new DevComponents.DotNetBar.Controls.CheckBoxX();
            this.cmbDesiredLanguage = new DevComponents.DotNetBar.Controls.ComboBoxEx();
            this.cmbMozhiBackend = new DevComponents.DotNetBar.Controls.ComboBoxEx();
            this.cmbLanguageModel = new DevComponents.DotNetBar.Controls.ComboBoxEx();
            this.cmbDetectCurrency = new DevComponents.DotNetBar.Controls.ComboBoxEx();
            this.cmbDesiredCurrency = new DevComponents.DotNetBar.Controls.ComboBoxEx();
            this.cmbPreferredLayout = new DevComponents.DotNetBar.Controls.ComboBoxEx();
            this.cmbPreferredTranslateEngine = new DevComponents.DotNetBar.Controls.ComboBoxEx();
            this.translateLabelX3 = new DevComponents.DotNetBar.LabelX();
            this.translateLabelX4 = new DevComponents.DotNetBar.LabelX();
            this.translateLabelX5 = new DevComponents.DotNetBar.LabelX();
            this.translateLabelX6 = new DevComponents.DotNetBar.LabelX();
            this.translateLabelX7 = new DevComponents.DotNetBar.LabelX();
            this.translateLabelX8 = new DevComponents.DotNetBar.LabelX();
            this.translateLabelX9 = new DevComponents.DotNetBar.LabelX();
            this.translateLabelX10 = new DevComponents.DotNetBar.LabelX();
            this.translateLabelX11 = new DevComponents.DotNetBar.LabelX();
            this.translateLabelX12 = new DevComponents.DotNetBar.LabelX();
            this.translateLabelX13 = new DevComponents.DotNetBar.LabelX();
            this.translateLabelX14 = new DevComponents.DotNetBar.LabelX();
            this.translateLabelX15 = new DevComponents.DotNetBar.LabelX();
            this.translateLabelX16 = new DevComponents.DotNetBar.LabelX();
            this.txtAPIkey = new DevComponents.DotNetBar.Controls.TextBoxX();
            this.txtNxAPIkey = new DevComponents.DotNetBar.Controls.TextBoxX();
            this.txtOpenAIBackend = new DevComponents.DotNetBar.Controls.TextBoxX();
            this.txtOpenAISystemMessage = new DevComponents.DotNetBar.Controls.TextBoxX();
            this.txtSecretkey = new DevComponents.DotNetBar.Controls.TextBoxX();
            this.txtLMTemperature = new DevComponents.Editors.DoubleInput();
            this.txtMaximumToken = new DevComponents.Editors.IntegerInput();
            this.buttonX3 = new DevComponents.DotNetBar.ButtonX();
            this.buttonX2 = new DevComponents.DotNetBar.ButtonX();
            this.buttonX1 = new DevComponents.DotNetBar.ButtonX();
            this.buttonXCheck = new DevComponents.DotNetBar.ButtonX();
            this.buttonXCheck2 = new DevComponents.DotNetBar.ButtonX();
            this.buttonXCheck3 = new DevComponents.DotNetBar.ButtonX();

            this.animationOutputPage = new DevComponents.DotNetBar.WizardPage();
            this.finishedWizardPage = new DevComponents.DotNetBar.WizardPage();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.wizard.SuspendLayout();
            this.wizardPage1.SuspendLayout();
            this.previewCommonWizardPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picPreview)).BeginInit();
            this.translatorWizardPage.SuspendLayout();
            this.finishedWizardPage.SuspendLayout();
            this.SuspendLayout();
            // 
            // wizard
            // 
            this.wizard.BackButtonText = "< 下一步(&B)";
            this.wizard.BackColor = System.Drawing.SystemColors.Control;
            this.wizard.CancelButtonText = "取消(&C)";
            this.wizard.CancelButtonWidth = 80;
            this.wizard.Cursor = System.Windows.Forms.Cursors.Default;
            this.wizard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.wizard.FinishButtonTabIndex = 3;
            this.wizard.FinishButtonText = "完成(&F)";
            this.wizard.FooterHeight = 42;
            // 
            // 
            // 
            this.wizard.FooterStyle.BackColor = System.Drawing.SystemColors.Control;
            this.wizard.FooterStyle.BackColorGradientAngle = 90;
            this.wizard.FooterStyle.BorderBottomWidth = 1;
            this.wizard.FooterStyle.BorderColor = System.Drawing.SystemColors.Control;
            this.wizard.FooterStyle.BorderLeftWidth = 1;
            this.wizard.FooterStyle.BorderRightWidth = 1;
            this.wizard.FooterStyle.BorderTop = DevComponents.DotNetBar.eStyleBorderType.Etched;
            this.wizard.FooterStyle.BorderTopColor = System.Drawing.SystemColors.Control;
            this.wizard.FooterStyle.BorderTopWidth = 1;
            this.wizard.FooterStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.wizard.FooterStyle.TextAlignment = DevComponents.DotNetBar.eStyleTextAlignment.Center;
            this.wizard.FooterStyle.TextColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelText;
            this.wizard.ForeColor = System.Drawing.SystemColors.ControlText;
            this.wizard.HeaderCaptionFont = new System.Drawing.Font("SimSun", 12F, System.Drawing.FontStyle.Bold);
            this.wizard.HeaderDescriptionFont = new System.Drawing.Font("SimSun", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.wizard.HeaderDescriptionIndent = 16;
            this.wizard.HeaderImageSize = new System.Drawing.Size(0, 0);
            // 
            // 
            // 
            this.wizard.HeaderStyle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.wizard.HeaderStyle.BackColorGradientAngle = 90;
            this.wizard.HeaderStyle.BorderBottom = DevComponents.DotNetBar.eStyleBorderType.Etched;
            this.wizard.HeaderStyle.BorderBottomWidth = 1;
            this.wizard.HeaderStyle.BorderColor = System.Drawing.SystemColors.Control;
            this.wizard.HeaderStyle.BorderLeftWidth = 1;
            this.wizard.HeaderStyle.BorderRightWidth = 1;
            this.wizard.HeaderStyle.BorderTopWidth = 1;
            this.wizard.HeaderStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.wizard.HeaderStyle.TextAlignment = DevComponents.DotNetBar.eStyleTextAlignment.Center;
            this.wizard.HeaderStyle.TextColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelText;
            this.wizard.HelpButtonVisible = false;
            this.wizard.Location = new System.Drawing.Point(0, 0);
            this.wizard.Name = "wizard";
            this.wizard.NextButtonText = "下一步(&N) >";
            this.wizard.Size = new System.Drawing.Size(879, 538);
            this.wizard.TabIndex = 0;
            this.wizard.WizardPages.AddRange(new DevComponents.DotNetBar.WizardPage[] {
            this.wizardPage1,
            this.previewCommonWizardPage,
            this.translatorWizardPage,
            // this.animationOutputPage,
            this.finishedWizardPage});
            this.wizard.FinishButtonClick += new System.ComponentModel.CancelEventHandler(this.wizard_FinishButtonClick);
            this.wizard.WizardPageChanging += new DevComponents.DotNetBar.WizardCancelPageChangeEventHandler(this.wizard_WizardPageChanging);
            // 
            // wizardPage1
            // 
            this.wizardPage1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.wizardPage1.BackColor = System.Drawing.Color.White;
            this.wizardPage1.Controls.Add(this.label1);
            this.wizardPage1.Controls.Add(this.label2);
            this.wizardPage1.Controls.Add(this.label3);
            this.wizardPage1.InteriorPage = false;
            this.wizardPage1.Location = new System.Drawing.Point(0, 0);
            this.wizardPage1.Name = "wizardPage1";
            this.wizardPage1.Size = new System.Drawing.Size(879, 496);
            // 
            // 
            // 
            this.wizardPage1.Style.BackColor = System.Drawing.Color.White;
            this.wizardPage1.Style.BackgroundImagePosition = DevComponents.DotNetBar.eStyleBackgroundImage.TopLeft;
            this.wizardPage1.Style.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.wizardPage1.StyleMouseDown.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.wizardPage1.StyleMouseOver.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.wizardPage1.TabIndex = 7;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.Font = new System.Drawing.Font("SimSun", 16F);
            this.label1.Location = new System.Drawing.Point(210, 17);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(655, 61);
            this.label1.TabIndex = 0;
            this.label1.Text = "歡迎使用WzComparerR2-NTMS";
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.BackColor = System.Drawing.Color.Transparent;
            this.label2.Location = new System.Drawing.Point(210, 92);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(654, 374);
            this.label2.TabIndex = 1;
            this.label2.Text = "此精靈將引導您配置WzComparerR2以便提高使用便利性。";
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label3.BackColor = System.Drawing.Color.Transparent;
            this.label3.Location = new System.Drawing.Point(210, 474);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(600, 12);
            this.label3.TabIndex = 2;
            this.label3.Text = "按下一步繼續。";
            // 
            // previewCommonWizardPage
            // 
            this.previewCommonWizardPage.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.previewCommonWizardPage.AntiAlias = false;
            this.previewCommonWizardPage.Controls.Add(this.chkEnable22AniStyle);
            this.previewCommonWizardPage.Controls.Add(this.chkEnableAutoPreview);
            this.previewCommonWizardPage.Controls.Add(this.chkShowID);
            this.previewCommonWizardPage.Controls.Add(this.chkShowSoldPrice);
            this.previewCommonWizardPage.Controls.Add(this.chkShowPurchasePrice);
            this.previewCommonWizardPage.Controls.Add(this.chkShowSkillDelay);
            this.previewCommonWizardPage.Controls.Add(this.chkShowSkillRange);
            this.previewCommonWizardPage.Controls.Add(this.chkShowMedalTag);
            this.previewCommonWizardPage.Controls.Add(this.chkShowNickTag);
            this.previewCommonWizardPage.Controls.Add(this.picPreview);
            this.previewCommonWizardPage.Location = new System.Drawing.Point(7, 72);
            this.previewCommonWizardPage.Name = "previewCommonWizardPage";
            this.previewCommonWizardPage.PageDescription = "這些選項會影響工具提示的佈局。";
            this.previewCommonWizardPage.PageTitle = "預覽選項";
            this.previewCommonWizardPage.Size = new System.Drawing.Size(865, 412);
            // 
            // 
            // 
            this.previewCommonWizardPage.Style.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.previewCommonWizardPage.StyleMouseDown.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.previewCommonWizardPage.StyleMouseOver.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.previewCommonWizardPage.TabIndex = 12;
            // 
            // chkEnable22AniStyle
            // 
            this.chkEnable22AniStyle.AutoSize = true;
            this.chkEnable22AniStyle.BackColor = System.Drawing.Color.Transparent;
            // 
            // 
            // 
            this.chkEnable22AniStyle.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.chkEnable22AniStyle.Location = new System.Drawing.Point(13, 30);
            this.chkEnable22AniStyle.Name = "chkEnable22AniStyle";
            this.chkEnable22AniStyle.Size = new System.Drawing.Size(192, 16);
            this.chkEnable22AniStyle.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.chkEnable22AniStyle.TabIndex = 5;
            this.chkEnable22AniStyle.Text = "啟用22週年紀念主題";
            this.chkEnable22AniStyle.MouseHover += new System.EventHandler(this.chkSet1_MouseHover);
            this.chkEnable22AniStyle.CheckedChanged += new System.EventHandler(this.chkSet1_CheckedChanged);
            // 
            // chkEnableAutoPreview
            // 
            this.chkEnableAutoPreview.AutoSize = true;
            this.chkEnableAutoPreview.BackColor = System.Drawing.Color.Transparent;
            // 
            // 
            // 
            this.chkEnableAutoPreview.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.chkEnableAutoPreview.Location = new System.Drawing.Point(13, 12);
            this.chkEnableAutoPreview.Name = "chkEnableAutoPreview";
            this.chkEnableAutoPreview.Size = new System.Drawing.Size(324, 16);
            this.chkEnableAutoPreview.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.chkEnableAutoPreview.TabIndex = 4;
            this.chkEnableAutoPreview.Text = "選擇 IMG 時自動預覽 (建議)";
            // 
            // chkShowID
            // 
            this.chkShowID.AutoSize = true;
            this.chkShowID.BackColor = System.Drawing.Color.Transparent;
            // 
            // 
            // 
            this.chkShowID.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.chkShowID.Location = new System.Drawing.Point(13, 48);
            this.chkShowID.Name = "chkShowID";
            this.chkShowID.Size = new System.Drawing.Size(158, 16);
            this.chkShowID.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.chkShowID.TabIndex = 6;
            this.chkShowID.Text = "在左上角顯示ID（全局）";
            this.chkShowID.MouseHover += new System.EventHandler(this.chkSet1_MouseHover);
            this.chkShowID.CheckedChanged += new System.EventHandler(this.chkSet1_CheckedChanged);
            // 
            // chkShowSoldPrice
            // 
            this.chkShowSoldPrice.AutoSize = true;
            this.chkShowSoldPrice.BackColor = System.Drawing.Color.Transparent;
            // 
            // 
            // 
            this.chkShowSoldPrice.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.chkShowSoldPrice.Location = new System.Drawing.Point(13, 66);
            this.chkShowSoldPrice.Name = "chkShowSoldPrice";
            this.chkShowSoldPrice.Size = new System.Drawing.Size(158, 16);
            this.chkShowSoldPrice.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.chkShowSoldPrice.TabIndex = 7;
            this.chkShowSoldPrice.Text = "顯示楓幣售價（全局）";
            this.chkShowSoldPrice.MouseHover += new System.EventHandler(this.chkSet1_MouseHover);
            this.chkShowSoldPrice.CheckedChanged += new System.EventHandler(this.chkSet1_CheckedChanged);
            // 
            // chkShowPurchasePrice
            // 
            this.chkShowPurchasePrice.AutoSize = true;
            this.chkShowPurchasePrice.BackColor = System.Drawing.Color.Transparent;
            // 
            // 
            // 
            this.chkShowPurchasePrice.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.chkShowPurchasePrice.Location = new System.Drawing.Point(13, 84);
            this.chkShowPurchasePrice.Name = "chkShowPurchasePrice";
            this.chkShowPurchasePrice.Size = new System.Drawing.Size(177, 16);
            this.chkShowPurchasePrice.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.chkShowPurchasePrice.TabIndex = 8;
            this.chkShowPurchasePrice.Text = "顯示點數購買價格（全局）";
            this.chkShowPurchasePrice.MouseHover += new System.EventHandler(this.chkSet2_MouseHover);
            this.chkShowPurchasePrice.CheckedChanged += new System.EventHandler(this.chkSet2_CheckedChanged);
            // 
            // chkShowSkillDelay
            // 
            this.chkShowSkillDelay.AutoSize = true;
            this.chkShowSkillDelay.BackColor = System.Drawing.Color.Transparent;
            // 
            // 
            // 
            this.chkShowSkillDelay.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.chkShowSkillDelay.Location = new System.Drawing.Point(13, 120);
            this.chkShowSkillDelay.Name = "chkShowSkillDelay";
            this.chkShowSkillDelay.Size = new System.Drawing.Size(138, 16);
            this.chkShowSkillDelay.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.chkShowSkillDelay.TabIndex = 9;
            this.chkShowSkillDelay.Text = "顯示技能延遲";
            this.chkShowSkillDelay.MouseHover += new System.EventHandler(this.chkSet3_MouseHover);
            this.chkShowSkillDelay.CheckedChanged += new System.EventHandler(this.chkSet3_CheckedChanged);
            // 
            // chkShowSkillRange
            // 
            this.chkShowSkillRange.AutoSize = true;
            this.chkShowSkillRange.BackColor = System.Drawing.Color.Transparent;
            // 
            // 
            // 
            this.chkShowSkillRange.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.chkShowSkillRange.Location = new System.Drawing.Point(13, 138);
            this.chkShowSkillRange.Name = "chkShowSkillRange";
            this.chkShowSkillRange.Size = new System.Drawing.Size(147, 16);
            this.chkShowSkillRange.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.chkShowSkillRange.TabIndex = 10;
            this.chkShowSkillRange.Text = "顯示技能範圍座標";
            this.chkShowSkillRange.MouseHover += new System.EventHandler(this.chkSet3_MouseHover);
            this.chkShowSkillRange.CheckedChanged += new System.EventHandler(this.chkSet3_CheckedChanged);
            // 
            // chkShowMedalTag
            // 
            this.chkShowMedalTag.AutoSize = true;
            this.chkShowMedalTag.BackColor = System.Drawing.Color.Transparent;
            // 
            // 
            // 
            this.chkShowMedalTag.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.chkShowMedalTag.Location = new System.Drawing.Point(13, 174);
            this.chkShowMedalTag.Name = "chkShowMedalTag";
            this.chkShowMedalTag.Size = new System.Drawing.Size(248, 16);
            this.chkShowMedalTag.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.chkShowMedalTag.TabIndex = 11;
            this.chkShowMedalTag.Text = "顯示勳章預覽（僅限舊主題）";
            this.chkShowMedalTag.MouseHover += new System.EventHandler(this.chkSet4_MouseHover);
            this.chkShowMedalTag.CheckedChanged += new System.EventHandler(this.chkSet4_CheckedChanged);
            // 
            // chkShowNickTag
            // 
            this.chkShowNickTag.AutoSize = true;
            this.chkShowNickTag.BackColor = System.Drawing.Color.Transparent;
            // 
            // 
            // 
            this.chkShowNickTag.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.chkShowNickTag.Location = new System.Drawing.Point(13, 192);
            this.chkShowNickTag.Name = "chkShowNickTag";
            this.chkShowNickTag.Size = new System.Drawing.Size(156, 16);
            this.chkShowNickTag.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.chkShowNickTag.TabIndex = 12;
            this.chkShowNickTag.Text = "顯示稱號預覽";
            this.chkShowNickTag.MouseHover += new System.EventHandler(this.chkSet5_MouseHover);
            this.chkShowNickTag.CheckedChanged += new System.EventHandler(this.chkSet5_CheckedChanged);
            // 
            // picPreview
            // 
            this.picPreview.Location = new System.Drawing.Point(480, 12);
            this.picPreview.Name = "picPreview";
            this.picPreview.Size = new System.Drawing.Size(385, 385);
            this.picPreview.TabIndex = 1;
            this.picPreview.TabStop = false;
            this.picPreview.SizeMode = PictureBoxSizeMode.Zoom;
            // 
            // translatorWizardPage
            // 
            this.translatorWizardPage.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.translatorWizardPage.AntiAlias = false;
            this.translatorWizardPage.Controls.Add(this.chkEnableTranslate);
            this.translatorWizardPage.Controls.Add(this.translateLabelX5);
            this.translatorWizardPage.Controls.Add(this.buttonX3);
            this.translatorWizardPage.Controls.Add(this.buttonXCheck2);
            this.translatorWizardPage.Controls.Add(this.cmbMozhiBackend);
            this.translatorWizardPage.Controls.Add(this.cmbLanguageModel);
            this.translatorWizardPage.Controls.Add(this.translateLabelX6);
            this.translatorWizardPage.Controls.Add(this.cmbDesiredLanguage);
            this.translatorWizardPage.Controls.Add(this.translateLabelX7);
            this.translatorWizardPage.Controls.Add(this.translateLabelX8);
            this.translatorWizardPage.Controls.Add(this.cmbPreferredTranslateEngine);
            this.translatorWizardPage.Controls.Add(this.cmbPreferredLayout);
            this.translatorWizardPage.Controls.Add(this.translateLabelX9);
            this.translatorWizardPage.Controls.Add(this.cmbDetectCurrency);
            this.translatorWizardPage.Controls.Add(this.translateLabelX10);
            this.translatorWizardPage.Controls.Add(this.translateLabelX11);
            this.translatorWizardPage.Controls.Add(this.translateLabelX12);
            this.translatorWizardPage.Controls.Add(this.translateLabelX13);
            this.translatorWizardPage.Controls.Add(this.translateLabelX14);
            this.translatorWizardPage.Controls.Add(this.translateLabelX15);
            this.translatorWizardPage.Controls.Add(this.translateLabelX16);
            this.translatorWizardPage.Controls.Add(this.txtOpenAIBackend);
            this.translatorWizardPage.Controls.Add(this.txtLMTemperature);
            this.translatorWizardPage.Controls.Add(this.txtMaximumToken);
            this.translatorWizardPage.Controls.Add(this.txtAPIkey);
            this.translatorWizardPage.Controls.Add(this.chkOpenAIExtraOption);
            this.translatorWizardPage.Controls.Add(this.txtOpenAISystemMessage);
            this.translatorWizardPage.Controls.Add(this.cmbDesiredCurrency);
            this.translatorWizardPage.Location = new System.Drawing.Point(7, 72);
            this.translatorWizardPage.Name = "translatorWizardPage";
            this.translatorWizardPage.PageDescription = "翻譯功能可以幫助您理解外語的新內容。";
            this.translatorWizardPage.PageTitle = "翻譯功能";
            this.translatorWizardPage.Size = new System.Drawing.Size(865, 412);
            // 
            // 
            // 
            this.translatorWizardPage.Style.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.translatorWizardPage.StyleMouseDown.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.translatorWizardPage.StyleMouseOver.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.translatorWizardPage.TabIndex = 18;
            // 
            // chkEnableTranslate
            // 
            this.chkEnableTranslate.AutoSize = true;
            this.chkEnableTranslate.BackColor = System.Drawing.Color.Transparent;
            // 
            // 
            // 
            this.chkEnableTranslate.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.chkEnableTranslate.Location = new System.Drawing.Point(13, 9);
            this.chkEnableTranslate.Name = "chkEnableTranslate";
            this.chkEnableTranslate.Size = new System.Drawing.Size(121, 16);
            this.chkEnableTranslate.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.chkEnableTranslate.TabIndex = 4;
            this.chkEnableTranslate.Text = "啟用翻譯";
            this.chkEnableTranslate.CheckedChanged += new System.EventHandler(this.chkEnableTranslate_CheckedChanged);
            // 
            // translateLabelX5
            // 
            this.translateLabelX5.AutoSize = true;
            this.translateLabelX5.BackColor = System.Drawing.Color.Transparent;
            // 
            // 
            // 
            this.translateLabelX5.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.translateLabelX5.Location = new System.Drawing.Point(14, 33);
            this.translateLabelX5.Name = "translateLabelX5";
            this.translateLabelX5.TabIndex = 9;
            this.translateLabelX5.Text = "Mozhi伺服器";
            // 
            // translateLabelX7
            // 
            this.translateLabelX7.AutoSize = true;
            this.translateLabelX7.BackColor = System.Drawing.Color.Transparent;
            // 
            // 
            // 
            this.translateLabelX7.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.translateLabelX7.Location = new System.Drawing.Point(14, 60);
            this.translateLabelX7.Name = "translateLabelX7";
            this.translateLabelX7.TabIndex = 9;
            this.translateLabelX7.Text = "翻譯引擎";
            // 
            // translateLabelX8
            // 
            this.translateLabelX8.AutoSize = true;
            this.translateLabelX8.BackColor = System.Drawing.Color.Transparent;
            // 
            // 
            // 
            this.translateLabelX8.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.translateLabelX8.Location = new System.Drawing.Point(14, 87);
            this.translateLabelX8.Name = "translateLabelX8";
            this.translateLabelX8.TabIndex = 9;
            this.translateLabelX8.Text = "譯文顯示";
            // 
            // translateLabelX6
            // 
            this.translateLabelX6.AutoSize = true;
            this.translateLabelX6.BackColor = System.Drawing.Color.Transparent;
            // 
            // 
            // 
            this.translateLabelX6.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.translateLabelX6.Location = new System.Drawing.Point(14, 114);
            this.translateLabelX6.Name = "translateLabelX6";
            this.translateLabelX6.Size = new System.Drawing.Size(131, 16);
            this.translateLabelX6.TabIndex = 7;
            this.translateLabelX6.Text = "目標語言";
            // 
            // translateLabelX9
            // 
            this.translateLabelX9.AutoSize = true;
            this.translateLabelX9.BackColor = System.Drawing.Color.Transparent;
            // 
            // 
            // 
            this.translateLabelX9.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.translateLabelX9.Location = new System.Drawing.Point(14, 141);
            this.translateLabelX9.Name = "translateLabelX9";
            this.translateLabelX9.TabIndex = 9;
            this.translateLabelX9.Text = "點數單位";
            // 
            // translateLabelX10
            // 
            this.translateLabelX10.AutoSize = true;
            this.translateLabelX10.BackColor = System.Drawing.Color.Transparent;
            // 
            // 
            // 
            this.translateLabelX10.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.translateLabelX10.Location = new System.Drawing.Point(14, 168);
            this.translateLabelX10.Name = "translateLabelX10";
            this.translateLabelX10.TabIndex = 9;
            this.translateLabelX10.Text = "希望單位";
            // 
            // translateLabelX11
            // 
            this.translateLabelX11.AutoSize = true;
            this.translateLabelX11.BackColor = System.Drawing.Color.Transparent;
            // 
            // 
            // 
            this.translateLabelX11.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.translateLabelX11.Location = new System.Drawing.Point(14, 33);
            this.translateLabelX11.Name = "translateLabelX11";
            this.translateLabelX11.TabIndex = 9;
            this.translateLabelX11.Text = "語言模型";
            this.translateLabelX11.Visible = false;
            // 
            // translateLabelX12
            // 
            this.translateLabelX12.AutoSize = true;
            this.translateLabelX12.BackColor = System.Drawing.Color.Transparent;
            // 
            // 
            // 
            this.translateLabelX12.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.translateLabelX12.Location = new System.Drawing.Point(400, 15);
            this.translateLabelX12.Name = "translateLabelX12";
            this.translateLabelX12.TabIndex = 9;
            this.translateLabelX12.Text = "後端地址";
            this.translateLabelX12.Visible = false;
            // 
            // translateLabelX13
            // 
            this.translateLabelX13.AutoSize = true;
            this.translateLabelX13.BackColor = System.Drawing.Color.Transparent;
            // 
            // 
            // 
            this.translateLabelX13.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.translateLabelX13.Location = new System.Drawing.Point(400, 123);
            this.translateLabelX13.Name = "translateLabelX13";
            this.translateLabelX13.TabIndex = 9;
            this.translateLabelX13.Text = "語言模型溫度值";
            this.translateLabelX13.Visible = false;
            // 
            // translateLabelX14
            // 
            this.translateLabelX14.AutoSize = true;
            this.translateLabelX14.BackColor = System.Drawing.Color.Transparent;
            // 
            // 
            // 
            this.translateLabelX14.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.translateLabelX14.Location = new System.Drawing.Point(400, 150);
            this.translateLabelX14.Name = "translateLabelX14";
            this.translateLabelX14.TabIndex = 9;
            this.translateLabelX14.Text = "最大Token上限";
            this.translateLabelX14.Visible = false;
            // 
            // translateLabelX15
            // 
            this.translateLabelX15.AutoSize = true;
            this.translateLabelX15.BackColor = System.Drawing.Color.Transparent;
            // 
            // 
            // 
            this.translateLabelX15.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.translateLabelX15.Location = new System.Drawing.Point(400, 42);
            this.translateLabelX15.Name = "translateLabelX15";
            this.translateLabelX15.TabIndex = 9;
            this.translateLabelX15.Text = "AI系統訊息";
            this.translateLabelX15.Visible = false;
            // 
            // translateLabelX16
            // 
            this.translateLabelX16.AutoSize = true;
            this.translateLabelX16.BackColor = System.Drawing.Color.Transparent;
            // 
            // 
            // 
            this.translateLabelX16.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.translateLabelX16.Location = new System.Drawing.Point(400, 177);
            this.translateLabelX16.Name = "translateLabelX16";
            this.translateLabelX16.TabIndex = 10;
            this.translateLabelX16.Text = "API金鑰 （不含「Bearer」）";
            this.translateLabelX16.Visible = false;
            // 
            // cmbDesiredLanguage
            // 
            this.cmbDesiredLanguage.DisplayMember = "Text";
            this.cmbDesiredLanguage.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.cmbDesiredLanguage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbDesiredLanguage.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmbDesiredLanguage.FormattingEnabled = true;
            this.cmbDesiredLanguage.ItemHeight = 13;
            this.cmbDesiredLanguage.Width = 248;
            this.cmbDesiredLanguage.Location = new System.Drawing.Point(110, 112);
            this.cmbDesiredLanguage.Name = "cmbDesiredLanguage";
            this.cmbDesiredLanguage.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.cmbDesiredLanguage.TabIndex = 8;
            // 
            // cmbMozhiBackend
            // 
            this.cmbMozhiBackend.DisplayMember = "Text";
            this.cmbMozhiBackend.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.cmbMozhiBackend.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbMozhiBackend.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmbMozhiBackend.FormattingEnabled = true;
            this.cmbMozhiBackend.ItemHeight = 13;
            this.cmbMozhiBackend.Width = 248;
            this.cmbMozhiBackend.Location = new System.Drawing.Point(110, 31);
            this.cmbMozhiBackend.Name = "cmbMozhiBackend";
            this.cmbMozhiBackend.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.cmbMozhiBackend.TabIndex = 12;
            // 
            // cmbLanguageModel
            // 
            this.cmbLanguageModel.DisplayMember = "Text";
            this.cmbLanguageModel.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.cmbLanguageModel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbLanguageModel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmbLanguageModel.FormattingEnabled = true;
            this.cmbLanguageModel.ItemHeight = 13;
            this.cmbLanguageModel.Width = 248;
            this.cmbLanguageModel.Location = new System.Drawing.Point(110, 31);
            this.cmbLanguageModel.Name = "cmbLanguageModel";
            this.cmbLanguageModel.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.cmbLanguageModel.TabIndex = 12;
            this.cmbLanguageModel.Visible = false;
            // 
            // cmbPreferredTranslateEngine
            // 
            this.cmbPreferredTranslateEngine.DisplayMember = "Text";
            this.cmbPreferredTranslateEngine.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.cmbPreferredTranslateEngine.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbPreferredTranslateEngine.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmbPreferredTranslateEngine.FormattingEnabled = true;
            this.cmbPreferredTranslateEngine.ItemHeight = 13;
            this.cmbPreferredTranslateEngine.Width = 248;
            this.cmbPreferredTranslateEngine.Location = new System.Drawing.Point(110, 58);
            this.cmbPreferredTranslateEngine.Name = "cmbPreferredTranslateEngine";
            this.cmbPreferredTranslateEngine.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.cmbPreferredTranslateEngine.TabIndex = 13;
            this.cmbPreferredTranslateEngine.SelectedIndexChanged += cmbPreferredTranslateEngine_SelectedIndexChanged;
            // 
            // cmbPreferredLayout
            // 
            this.cmbPreferredLayout.DisplayMember = "Text";
            this.cmbPreferredLayout.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.cmbPreferredLayout.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbPreferredLayout.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmbPreferredLayout.FormattingEnabled = true;
            this.cmbPreferredLayout.ItemHeight = 13;
            this.cmbPreferredLayout.Width = 248;
            this.cmbPreferredLayout.Location = new System.Drawing.Point(110, 85);
            this.cmbPreferredLayout.Name = "cmbPreferredLayout";
            this.cmbPreferredLayout.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.cmbPreferredLayout.TabIndex = 15;
            // 
            // cmbDetectCurrency
            // 
            this.cmbDetectCurrency.DisplayMember = "Text";
            this.cmbDetectCurrency.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.cmbDetectCurrency.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbDetectCurrency.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmbDetectCurrency.FormattingEnabled = true;
            this.cmbDetectCurrency.ItemHeight = 13;
            this.cmbDetectCurrency.Width = 248;
            this.cmbDetectCurrency.Location = new System.Drawing.Point(110, 139);
            this.cmbDetectCurrency.Name = "cmbDetectCurrency";
            this.cmbDetectCurrency.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.cmbDetectCurrency.TabIndex = 25;
            // 
            // cmbDesiredCurrency
            // 
            this.cmbDesiredCurrency.DisplayMember = "Text";
            this.cmbDesiredCurrency.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.cmbDesiredCurrency.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbDesiredCurrency.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmbDesiredCurrency.FormattingEnabled = true;
            this.cmbDesiredCurrency.ItemHeight = 13;
            this.cmbDesiredCurrency.Width = 248;
            this.cmbDesiredCurrency.Location = new System.Drawing.Point(110, 166);
            this.cmbDesiredCurrency.Name = "cmbDesiredCurrency";
            this.cmbDesiredCurrency.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.cmbDesiredCurrency.TabIndex = 26;
            // 
            // txtOpenAIBackend
            // 
            // 
            // 
            // 
            this.txtOpenAIBackend.Border.Class = "TextBoxBorder";
            this.txtOpenAIBackend.Border.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.txtOpenAIBackend.Location = new System.Drawing.Point(540, 13);
            this.txtOpenAIBackend.Name = "txtOpenAIBackend";
            this.txtOpenAIBackend.Size = new System.Drawing.Size(208, 23);
            this.txtOpenAIBackend.TabIndex = 10;
            this.txtOpenAIBackend.WatermarkText = "https://api.openai.com/v1";
            this.txtOpenAIBackend.Visible = false;
            // 
            // txtOpenAISystemMessage
            // 
            // 
            // 
            // 
            this.txtOpenAISystemMessage.Border.Class = "TextBoxBorder";
            this.txtOpenAISystemMessage.Border.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.txtOpenAISystemMessage.Location = new System.Drawing.Point(540, 40);
            this.txtOpenAISystemMessage.Multiline = true;
            this.txtOpenAISystemMessage.Name = "txtOpenAISystemMessage";
            this.txtOpenAISystemMessage.Size = new System.Drawing.Size(208, 50);
            this.txtOpenAISystemMessage.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtOpenAISystemMessage.TabIndex = 11;
            this.txtOpenAISystemMessage.WatermarkText = "You are an automated translator for a community game engine, and I only need translated result in output.";
            this.txtOpenAISystemMessage.Visible = false;
            // 
            // chkOpenAIExtraOption
            // 
            this.chkOpenAIExtraOption.AutoSize = true;
            this.chkOpenAIExtraOption.BackColor = System.Drawing.Color.Transparent;
            // 
            // 
            // 
            this.chkOpenAIExtraOption.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.chkOpenAIExtraOption.Location = new System.Drawing.Point(400, 94);
            this.chkOpenAIExtraOption.Name = "chkOpenAIExtraOption";
            this.chkOpenAIExtraOption.Size = new System.Drawing.Size(212, 16);
            this.chkOpenAIExtraOption.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.chkOpenAIExtraOption.TabIndex = 12;
            this.chkOpenAIExtraOption.Text = "使用以下額外設定";
            this.chkOpenAIExtraOption.CheckedChanged += new System.EventHandler(this.chkOpenAIExtraOption_CheckedChanged);
            this.chkOpenAIExtraOption.Visible = false;
            //
            // txtLMTemperature
            //
            //
            //
            this.txtLMTemperature.BackgroundStyle.Class = "DateTimeInputBackground";
            this.txtLMTemperature.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.txtLMTemperature.ButtonFreeText.Shortcut = DevComponents.DotNetBar.eShortcut.F2;
            this.txtLMTemperature.ForeColor = System.Drawing.SystemColors.ControlText;
            this.txtLMTemperature.Location = new System.Drawing.Point(540, 121);
            this.txtLMTemperature.MinValue = 0;
            this.txtLMTemperature.MaxValue = 2;
            this.txtLMTemperature.Increment = 0.1;
            this.txtLMTemperature.DisplayFormat = "0.0";
            this.txtLMTemperature.Name = "txtLMTemperature";
            this.txtLMTemperature.ShowUpDown = true;
            this.txtLMTemperature.Size = new System.Drawing.Size(70, 21);
            this.txtLMTemperature.TabIndex = 13;
            this.txtLMTemperature.Visible = false;
            //
            // txtMaximumToken
            //
            //
            //
            this.txtMaximumToken.BackgroundStyle.Class = "DateTimeInputBackground";
            this.txtMaximumToken.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.txtMaximumToken.ButtonFreeText.Shortcut = DevComponents.DotNetBar.eShortcut.F2;
            this.txtMaximumToken.ForeColor = System.Drawing.SystemColors.ControlText;
            this.txtMaximumToken.Location = new System.Drawing.Point(540, 148);
            this.txtMaximumToken.MinValue = -1;
            this.txtMaximumToken.Name = "txtMaximumToken";
            this.txtMaximumToken.ShowUpDown = true;
            this.txtMaximumToken.Size = new System.Drawing.Size(70, 21);
            this.txtMaximumToken.TabIndex = 14;
            this.txtMaximumToken.Visible = false;
            // 
            // txtAPIkey
            // 
            // 
            // 
            // 
            this.txtAPIkey.Border.Class = "TextBoxBorder";
            this.txtAPIkey.Border.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.txtAPIkey.Location = new System.Drawing.Point(540, 175);
            this.txtAPIkey.Name = "txtAPIkey";
            this.txtAPIkey.Size = new System.Drawing.Size(208, 23);
            this.txtAPIkey.TabIndex = 10;
            // 
            // buttonXCheck2
            // 
            this.buttonXCheck2.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            this.buttonXCheck2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonXCheck2.ColorTable = DevComponents.DotNetBar.eButtonColor.OrangeWithBackground;
            this.buttonXCheck2.Location = new System.Drawing.Point(303, 4);
            this.buttonXCheck2.Name = "buttonXCheck2";
            this.buttonXCheck2.Size = new System.Drawing.Size(55, 19);
            this.buttonXCheck2.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.buttonXCheck2.TabIndex = 4;
            this.buttonXCheck2.Text = "檢查";
            this.buttonXCheck2.Click += new System.EventHandler(this.buttonXCheck2_Click);
            // 
            // animationOutputPage
            // 
            this.animationOutputPage.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.animationOutputPage.AntiAlias = false;
            this.animationOutputPage.Location = new System.Drawing.Point(7, 72);
            this.animationOutputPage.Name = "animationOutputPage";
            this.animationOutputPage.PageDescription = "允許您配置動畫的匯出方式。";
            this.animationOutputPage.PageTitle = "動畫匯出";
            this.animationOutputPage.Size = new System.Drawing.Size(865, 412);
            // 
            // 
            // 
            this.animationOutputPage.Style.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.animationOutputPage.StyleMouseDown.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.animationOutputPage.StyleMouseOver.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.animationOutputPage.TabIndex = 19;
            // 
            // finishedWizardPage
            // 
            this.finishedWizardPage.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.finishedWizardPage.AntiAlias = false;
            this.finishedWizardPage.BackColor = System.Drawing.Color.White;
            this.finishedWizardPage.Controls.Add(this.label4);
            this.finishedWizardPage.Controls.Add(this.label5);
            this.finishedWizardPage.Controls.Add(this.label6);
            this.finishedWizardPage.InteriorPage = false;
            this.finishedWizardPage.Location = new System.Drawing.Point(0, 0);
            this.finishedWizardPage.Name = "finishedWizardPage";
            this.finishedWizardPage.Size = new System.Drawing.Size(879, 496);
            // 
            // 
            // 
            this.finishedWizardPage.Style.BackColor = System.Drawing.Color.White;
            this.finishedWizardPage.Style.BackgroundImagePosition = DevComponents.DotNetBar.eStyleBackgroundImage.TopLeft;
            this.finishedWizardPage.Style.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.finishedWizardPage.StyleMouseDown.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.finishedWizardPage.StyleMouseOver.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.finishedWizardPage.TabIndex = 11;
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label4.BackColor = System.Drawing.Color.Transparent;
            this.label4.Font = new System.Drawing.Font("SimSun", 16F);
            this.label4.Location = new System.Drawing.Point(210, 17);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(655, 61);
            this.label4.TabIndex = 0;
            this.label4.Text = "設定完成";
            // 
            // label5
            // 
            this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label5.BackColor = System.Drawing.Color.Transparent;
            this.label5.Location = new System.Drawing.Point(210, 92);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(654, 374);
            this.label5.TabIndex = 1;
            this.label5.Text = "首次運行精靈已完成。";
            // 
            // label6
            // 
            this.label6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label6.BackColor = System.Drawing.Color.Transparent;
            this.label6.Location = new System.Drawing.Point(210, 474);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(600, 12);
            this.label6.TabIndex = 2;
            this.label6.Text = "若要儲存設置，請按一下完成。";
            // 
            // FrmSetupWizard
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(879, 538);
            this.Controls.Add(this.wizard);
            this.DoubleBuffered = true;
            this.EnableGlass = false;
            this.Font = new System.Drawing.Font("SimSun", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmSetupWizard";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "首次運行精靈";
            this.TopMost = true;
            this.wizard.ResumeLayout(false);
            this.wizardPage1.ResumeLayout(false);
            this.previewCommonWizardPage.ResumeLayout(false);
            this.previewCommonWizardPage.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picPreview)).EndInit();
            this.translatorWizardPage.ResumeLayout(false);
            this.translatorWizardPage.PerformLayout();
            this.finishedWizardPage.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private DevComponents.DotNetBar.Wizard wizard;
        private DevComponents.DotNetBar.WizardPage wizardPage1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private DevComponents.DotNetBar.WizardPage finishedWizardPage;
        private DevComponents.DotNetBar.WizardPage previewCommonWizardPage;
        private DevComponents.DotNetBar.WizardPage translatorWizardPage;
        private DevComponents.DotNetBar.WizardPage animationOutputPage;

        private DevComponents.DotNetBar.Controls.CheckBoxX chkEnable22AniStyle;
        private DevComponents.DotNetBar.Controls.CheckBoxX chkEnableAutoPreview;
        private DevComponents.DotNetBar.Controls.CheckBoxX chkShowID;
        private DevComponents.DotNetBar.Controls.CheckBoxX chkShowSoldPrice;
        private DevComponents.DotNetBar.Controls.CheckBoxX chkShowPurchasePrice;
        private DevComponents.DotNetBar.Controls.CheckBoxX chkShowSkillDelay;
        private DevComponents.DotNetBar.Controls.CheckBoxX chkShowSkillRange;
        private DevComponents.DotNetBar.Controls.CheckBoxX chkShowMedalTag;
        private DevComponents.DotNetBar.Controls.CheckBoxX chkShowNickTag;

        private DevComponents.DotNetBar.Controls.CheckBoxX chkEnableTranslate;
        private DevComponents.DotNetBar.Controls.CheckBoxX chkOpenAIExtraOption;
        private DevComponents.DotNetBar.Controls.ComboBoxEx cmbDesiredLanguage;
        private DevComponents.DotNetBar.Controls.ComboBoxEx cmbMozhiBackend;
        private DevComponents.DotNetBar.Controls.ComboBoxEx cmbLanguageModel;
        private DevComponents.DotNetBar.Controls.ComboBoxEx cmbDetectCurrency;
        private DevComponents.DotNetBar.Controls.ComboBoxEx cmbDesiredCurrency;
        private DevComponents.DotNetBar.Controls.ComboBoxEx cmbPreferredLayout;
        private DevComponents.DotNetBar.Controls.ComboBoxEx cmbPreferredTranslateEngine;
        private DevComponents.DotNetBar.LabelX translateLabelX3;
        private DevComponents.DotNetBar.LabelX translateLabelX4;
        private DevComponents.DotNetBar.LabelX translateLabelX5;
        private DevComponents.DotNetBar.LabelX translateLabelX6;
        private DevComponents.DotNetBar.LabelX translateLabelX7;
        private DevComponents.DotNetBar.LabelX translateLabelX8;
        private DevComponents.DotNetBar.LabelX translateLabelX9;
        private DevComponents.DotNetBar.LabelX translateLabelX10;
        private DevComponents.DotNetBar.LabelX translateLabelX11;
        private DevComponents.DotNetBar.LabelX translateLabelX12;
        private DevComponents.DotNetBar.LabelX translateLabelX13;
        private DevComponents.DotNetBar.LabelX translateLabelX14;
        private DevComponents.DotNetBar.LabelX translateLabelX15;
        private DevComponents.DotNetBar.LabelX translateLabelX16;
        private DevComponents.DotNetBar.Controls.TextBoxX txtAPIkey;
        private DevComponents.DotNetBar.Controls.TextBoxX txtNxAPIkey;
        private DevComponents.DotNetBar.Controls.TextBoxX txtOpenAIBackend;
        private DevComponents.DotNetBar.Controls.TextBoxX txtOpenAISystemMessage;
        private DevComponents.DotNetBar.Controls.TextBoxX txtSecretkey;
        private DevComponents.Editors.DoubleInput txtLMTemperature;
        private DevComponents.Editors.IntegerInput txtMaximumToken;
        private DevComponents.DotNetBar.ButtonX buttonX3;
        private DevComponents.DotNetBar.ButtonX buttonX2;
        private DevComponents.DotNetBar.ButtonX buttonX1;
        private DevComponents.DotNetBar.ButtonX buttonXCheck;
        private DevComponents.DotNetBar.ButtonX buttonXCheck2;
        private DevComponents.DotNetBar.ButtonX buttonXCheck3;
        private System.Windows.Forms.PictureBox picPreview;
    }
}