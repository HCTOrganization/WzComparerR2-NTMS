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
            this.wizard.BackButtonText = "< 戻る(&B)";
            this.wizard.BackColor = System.Drawing.SystemColors.Control;
            this.wizard.CancelButtonText = "キャンセル(&C)";
            this.wizard.CancelButtonWidth = 80;
            this.wizard.Cursor = System.Windows.Forms.Cursors.Default;
            this.wizard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.wizard.FinishButtonTabIndex = 3;
            this.wizard.FinishButtonText = "完了(&F)";
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
            this.wizard.HeaderCaptionFont = new System.Drawing.Font("MS PGothic", 12F, System.Drawing.FontStyle.Bold);
            this.wizard.HeaderDescriptionFont = new System.Drawing.Font("MS PGothic", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
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
            this.wizard.NextButtonText = "次へ(&N) >";
            this.wizard.Size = new System.Drawing.Size(879, 538);
            this.wizard.TabIndex = 0;
            this.wizard.WizardPages.AddRange(new DevComponents.DotNetBar.WizardPage[] {
            this.wizardPage1,
            this.previewCommonWizardPage,
            // this.translatorWizardPage,
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
            this.label1.Font = new System.Drawing.Font("MS PGothic", 16F);
            this.label1.Location = new System.Drawing.Point(210, 17);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(655, 61);
            this.label1.TabIndex = 0;
            this.label1.Text = "WzComparerR2-JMSへようこそ";
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
            this.label2.Text = "このウィザードは、WzComparerR2をより便利に使用できるように設定する方法を説明します。";
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label3.BackColor = System.Drawing.Color.Transparent;
            this.label3.Location = new System.Drawing.Point(210, 474);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(600, 12);
            this.label3.TabIndex = 2;
            this.label3.Text = "続行するには、「次へ(&N)」をクリックします。";
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
            this.previewCommonWizardPage.PageDescription = "これらのオプションはツールチップのレイアウトに影響します。";
            this.previewCommonWizardPage.PageTitle = "プレビューオプション";
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
            this.chkEnable22AniStyle.Text = "22周年記念テーマを有効にする";
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
            this.chkEnableAutoPreview.Text = "IMGが選択されると自動的にプレビューを表示する（推奨）";
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
            this.chkShowID.Text = "左上隅にIDを表示 (全体)";
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
            this.chkShowSoldPrice.Text = "メル販売價額表示 (全体)";
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
            this.chkShowPurchasePrice.Text = "ポイント購入價額表示 (全体)";
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
            this.chkShowSkillDelay.Text = "スキルディレイを表示";
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
            this.chkShowSkillRange.Text = "スキル範囲座標を表示";
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
            this.chkShowMedalTag.Text = "勲章プレビューを表示 (以前のテーマのみ)";
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
            this.chkShowNickTag.Text = "タイトルプレビューを表示";
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
            this.translatorWizardPage.Location = new System.Drawing.Point(7, 72);
            this.translatorWizardPage.Name = "translatorWizardPage";
            this.translatorWizardPage.PageDescription = "翻訳機能を使用すると、外国語で新しいコンテンツを理解できるようになります。";
            this.translatorWizardPage.PageTitle = "翻訳機能";
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
            this.chkEnableTranslate.Location = new System.Drawing.Point(13, 12);
            this.chkEnableTranslate.Name = "chkEnableTranslate";
            this.chkEnableTranslate.Size = new System.Drawing.Size(121, 16);
            this.chkEnableTranslate.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.chkEnableTranslate.TabIndex = 4;
            this.chkEnableTranslate.Text = "翻訳を有効にする";
            // 
            // animationOutputPage
            // 
            this.animationOutputPage.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.animationOutputPage.AntiAlias = false;
            this.animationOutputPage.Location = new System.Drawing.Point(7, 72);
            this.animationOutputPage.Name = "animationOutputPage";
            this.animationOutputPage.PageDescription = "アニメーションをエクスポートする方法を設定できます。";
            this.animationOutputPage.PageTitle = "アニメーション出力";
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
            this.label4.Font = new System.Drawing.Font("MS PGothic", 16F);
            this.label4.Location = new System.Drawing.Point(210, 17);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(655, 61);
            this.label4.TabIndex = 0;
            this.label4.Text = "設定完了";
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
            this.label5.Text = "初回実行ウィザードが終了しました。";
            // 
            // label6
            // 
            this.label6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label6.BackColor = System.Drawing.Color.Transparent;
            this.label6.Location = new System.Drawing.Point(210, 474);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(600, 12);
            this.label6.TabIndex = 2;
            this.label6.Text = "設定を保存するには、「完了(&F)」をクリックします。";
            // 
            // FrmSetupWizard
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(879, 538);
            this.Controls.Add(this.wizard);
            this.DoubleBuffered = true;
            this.EnableGlass = false;
            this.Font = new System.Drawing.Font("MS PGothic", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmSetupWizard";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "初回実行ウィザード";
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
        private DevComponents.DotNetBar.LabelX lblPreferredLayout;
        private DevComponents.DotNetBar.LabelX lblPreferredDesiredLangauge;
        private DevComponents.DotNetBar.LabelX lblPreferredTranslateEngine;
        private DevComponents.DotNetBar.LabelX lblMozhiBackend;
        private DevComponents.DotNetBar.LabelX lblLanguageModel;
        private DevComponents.DotNetBar.Controls.ComboBoxEx cmbPreferredLayout;
        private DevComponents.DotNetBar.Controls.ComboBoxEx cmbPreferredDesiredLanguage;
        private DevComponents.DotNetBar.Controls.ComboBoxEx cmbPreferredTranslateEngine;
        private DevComponents.DotNetBar.Controls.ComboBoxEx cmbMozhiBackend;
        private DevComponents.DotNetBar.Controls.ComboBoxEx cmbLanguageModel;
        private System.Windows.Forms.PictureBox picPreview;
    }
}