using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Linq;
using System.Reflection;
using System.IO;
using System.Xml;
using Timer = System.Timers.Timer;
using System.Threading;
using System.Threading.Tasks;
using DevComponents.DotNetBar;
using DevComponents.DotNetBar.Controls;
using DevComponents.AdvTree;
using DevComponents.AdvTree.Display;
using WzComparerR2.WzLib;
using WzComparerR2.UnityLib.Classic;
using WzComparerR2.UnityLib.Unity;
using WzComparerR2.UnityLib.Wzjs;
using WzComparerR2.Common;
using WzComparerR2.CharaSimControl;
using WzComparerR2.PluginBase;
using WzComparerR2.CharaSim;
using WzComparerR2.Comparer;
using WzComparerR2.Controls;
using WzComparerR2.Rendering;
using WzComparerR2.Config;
using WzComparerR2.Animation;
using WzComparerR2.Encoders;
using static Microsoft.Xna.Framework.MathHelper;
using Microsoft.Win32;
using SharpDX;
using System.Drawing.Imaging;
using Newtonsoft.Json.Linq;

namespace WzComparerR2
{
    public partial class MainForm : Office2007RibbonForm, PluginContextProvider
    {
        public MainForm()
        {
            InitializeComponent();
            this.FormClosing += new FormClosingEventHandler(MainForm_FormClosing);
            this.Shown += new EventHandler(MainForm_Shown);
#if NET6_0_OR_GREATER
            // https://learn.microsoft.com/en-us/dotnet/core/compatibility/fx-core#controldefaultfont-changed-to-segoe-ui-9pt
            this.Font = new Font(new FontFamily("SimSun"), 9f);
#endif
            Form.CheckForIllegalCrossThreadCalls = false;
            this.MinimumSize = new Size(600, 450);
            advTree1.AfterNodeSelect += new AdvTreeNodeEventHandler(advTree1_AfterNodeSelect_2);
            advTree2.AfterNodeSelect += new AdvTreeNodeEventHandler(advTree2_AfterNodeSelect_2);
            //new ImageDragHandler(this.pictureBox1).AttachEvents();
            RegisterPluginEvents();
            createStyleItems();
            initFields();
            loadUIState();
        }

        public List<Wz_Structure> openedWz;
        private Dictionary<Node, UnityAssetDocument> openedUnitySources;

        /// <summary>開啟中的 Unity 資產資料集；保留根 Wz_Node 以免 AdvTree 的 WeakReference 失效。</summary>
        private sealed class UnityAssetDocument
        {
            public UnityAssetDocument(ClassicWzSource source, Wz_Node rootNode)
            {
                this.Source = source;
                this.RootNode = rootNode;
            }

            public ClassicWzSource Source { get; }
            public Wz_Node RootNode { get; }

            /// <summary>advTree1 中對應 <see cref="RootNode"/> 的節點。</summary>
            public Node TreeNode { get; set; }

            /// <summary>String/Etc/Quest 資產整理成的類 WZ 節點，供 StringLinker 使用。</summary>
            public ClassicStringLinkerTables StringTables { get; set; }
        }
        StringLinker stringLinker;
        HistoryList<Node> historyNodeList;
        bool historySelecting;

        //soundPlayer
        BassSoundPlayer soundPlayer;
        Timer soundTimer;
        bool timerChangeValue;

        //charaSim
        AfrmTooltip tooltipQuickView;
        CharaSimControlGroup charaSimCtrl;
        AdvTree lastSelectedTree;
        DefaultLevel skillDefaultLevel = DefaultLevel.Level0;
        int skillInterval = 32;

        /// <summary>前一次開啟 Unity 資料集時選擇的地域コード。多地域資料集再次開啟時作為預設值。</summary>
        string lastUnityLocale;

        //compare
        Thread compareThread;

        private void initFields()
        {
            openedWz = new List<Wz_Structure>();
            openedUnitySources = new Dictionary<Node, UnityAssetDocument>();
            stringLinker = new StringLinker();
            historyNodeList = new HistoryList<Node>();

            /*
            tooltipQuickView = new AfrmTooltip();
            tooltipQuickView = new AfrmTooltip();
            tooltipQuickView.Visible = false;
            tooltipQuickView.StringLinker = this.stringLinker;
            tooltipQuickView.KeyDown += new KeyEventHandler(afrm_KeyDown);
            tooltipQuickView.ShowID = true;
            tooltipQuickView.ShowMenu = true;
            */

            charaSimCtrl = new CharaSimControlGroup();
            charaSimCtrl.StringLinker = this.stringLinker;
            charaSimCtrl.Character = new Character();
            charaSimCtrl.Character.Name = "光卡的拉羊羊";
            charaSimCtrl.UIItem.Visible = false;
            charaSimCtrl.UIItem.VisibleChanged += new EventHandler(afrm_VisibleChanged);
            charaSimCtrl.UIStat.Visible = false;
            charaSimCtrl.UIStat.VisibleChanged += new EventHandler(afrm_VisibleChanged);
            charaSimCtrl.UIEquip.Visible = false;
            charaSimCtrl.UIEquip.VisibleChanged += new EventHandler(afrm_VisibleChanged);

            tooltipQuickView = charaSimCtrl.TooltipQuickView;
            tooltipQuickView.Visible = false;
            tooltipQuickView.StringLinker = this.stringLinker;
            tooltipQuickView.KeyDown += new KeyEventHandler(afrm_KeyDown);
            tooltipQuickView.ShowID = true;
            tooltipQuickView.ShowMenu = true;
            tooltipQuickView.OnStatusUpdate = (status) =>
            {
                labelItemStatus.Text = status;
            };

            string[] images = new string[] { "dir", "mp3", "num", "png", "str", "uol", "vector", "img", "rawdata", "convex", "video" };
            foreach (string img in images)
            {
                imageList1.Images.Add(img, (Image)Properties.Resources.ResourceManager.GetObject(img));
            }

            soundPlayer = new BassSoundPlayer();
            if (!soundPlayer.Init())
            {
                ManagedBass.Errors error = soundPlayer.GetLastError();
                MessageBoxEx.Show("Failed to initialize Bass\r\n\r\nError: " + (int)error + "(" + error + ")", LocalizedString_JP.COMMON_ERROR);
            }
            soundTimer = new Timer(120d);
            soundTimer.Elapsed += new System.Timers.ElapsedEventHandler(soundTimer_Elapsed);
            soundTimer.Enabled = true;

            PluginBase.PluginManager.WzFileFinding += new FindWzEventHandler(CharaSimLoader_WzFileFinding);
            CharaSimLoader.ActionDelayResolver = this.ResolveUnityActionDelay;
            QuestTooltipRenderer.ExternalNodeResolver = this.FindUnityNodeByPath;
            Wz_NodeExtension.ExternalTypeResolver = this.ResolveUnityWzType;

            foreach (WzPngComparison comp in Enum.GetValues(typeof(WzPngComparison)))
            {
                cmbComparePng.Items.Add(comp);
            }
            cmbComparePng.SelectedItem = WzPngComparison.SizeAndDataLength;

            foreach (var i in Enum.GetValues(typeof(Wz_Type)))
            {
                if (i is Wz_Type wzType && wzType != Wz_Type.Unknown)
                {
                    this.clbRootNode.Items.Add(wzType.ToString(), true);
                }
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            saveUIState();
        }

        private void saveUIState()
        {
            UIStateConfig.Default.WindowState = (int)this.WindowState;
            UIStateConfig.Default.WindowWidth = this.Size.Width;
            UIStateConfig.Default.WindowHeight = this.Size.Height;
            UIStateConfig.Default.RibbonExpanded = this.ribbonControl1.Expanded;
            UIStateConfig.Default.SelectedRibbonTabIndex = this.ribbonControl1.SelectedRibbonTabItem.Name.Last() - '0';
            UIStateConfig.Default.SplitterPosition1 = this.expandableSplitter1.SplitPosition;
            UIStateConfig.Default.SplitterPosition2 = this.expandableSplitter2.SplitPosition;
            UIStateConfig.Default.ColumnWidth3 = this.columnHeader3.Width.Absolute;
            UIStateConfig.Default.ColumnWidth4 = this.columnHeader4.Width.Absolute;
            UIStateConfig.Default.ColumnWidth5 = this.columnHeader5.Width.Absolute;
            UIStateConfig.Default.ColumnWidth6 = this.columnHeader6.Width;
            UIStateConfig.Default.ColumnWidth7 = this.columnHeader7.Width;
            UIStateConfig.Default.ColumnWidth8 = this.columnHeader8.Width;
            UIStateConfig.Default.ColumnWidth9 = this.columnHeader9.Width;
            UIStateConfig.Default.BarLayout = this.dotNetBarManager1.LayoutDefinition;
            ConfigManager.Save();
        }

        private void loadUIState()
        {
            try
            {
                this.WindowState = (FormWindowState)UIStateConfig.Default.WindowState.Value;
                this.Size = new Size(UIStateConfig.Default.WindowWidth, UIStateConfig.Default.WindowHeight);
                if (this.ribbonControl1.Expanded = UIStateConfig.Default.RibbonExpanded)
                {
                    switch (UIStateConfig.Default.SelectedRibbonTabIndex)
                    {
                        case 1: this.ribbonControl1.SelectedRibbonTabItem = this.ribbonTabItem1; break;
                        case 2: this.ribbonControl1.SelectedRibbonTabItem = this.ribbonTabItem2; break;
                        case 3: this.ribbonControl1.SelectedRibbonTabItem = this.ribbonTabItem3; break;
                        default: this.ribbonControl1.SelectFirstVisibleRibbonTab(); break;
                    }
                }
                this.expandableSplitter1.SplitPosition = UIStateConfig.Default.SplitterPosition1;
                this.expandableSplitter2.SplitPosition = UIStateConfig.Default.SplitterPosition2;
                this.columnHeader3.Width.Absolute = UIStateConfig.Default.ColumnWidth3;
                this.columnHeader4.Width.Absolute = UIStateConfig.Default.ColumnWidth4;
                this.columnHeader5.Width.Absolute = UIStateConfig.Default.ColumnWidth5;
                this.columnHeader6.Width = UIStateConfig.Default.ColumnWidth6;
                this.columnHeader7.Width = UIStateConfig.Default.ColumnWidth7;
                this.columnHeader8.Width = UIStateConfig.Default.ColumnWidth8;
                this.columnHeader9.Width = UIStateConfig.Default.ColumnWidth9;
                this.dotNetBarManager1.LayoutDefinition = UIStateConfig.Default.BarLayout;
            }
            catch (Exception ex)
            {
                this.WindowState = FormWindowState.Normal;
                this.Size = new Size(1200, 800); // = new Size(766, 520);
                this.ribbonControl1.Expanded = false; // = false;
                this.expandableSplitter1.SplitPosition = 468; // = 233;
                this.expandableSplitter2.SplitPosition = 230; // = 255;
                this.columnHeader3.Width.Absolute = 150;
                this.columnHeader4.Width.Absolute = 150;
                this.columnHeader5.Width.Absolute = 150;
                this.columnHeader6.Width = 80;
                this.columnHeader7.Width = 200; // = 100
                this.columnHeader8.Width = 600; // = 350
                this.columnHeader9.Width = 250; // = 150
                this.dotNetBarManager1.LayoutDefinition = "<dotnetbarlayout version=\"6\" zorder=\"7,8,1,0\"><docksite size=\"0\" dockingside=\"Top\" originaldocksitesize=\"0\" /><docksite size=\"182\" dockingside=\"Bottom\" originaldocksitesize=\"0\"><dockcontainer orientation=\"1\" w=\"0\" h=\"0\"><barcontainer w=\"1184\" h=\"179\"><bar name=\"bar1\" dockline=\"0\" layout=\"2\" dockoffset=\"0\" state=\"2\" dockside=\"4\" visible=\"true\"><items><item name=\"dockContainerItem1\" origBar=\"\" origPos=\"-1\" pos=\"0\" /></items></bar></barcontainer></dockcontainer></docksite><docksite size=\"0\" dockingside=\"Left\" originaldocksitesize=\"0\" /><docksite size=\"0\" dockingside=\"Right\" originaldocksitesize=\"0\" /><bars /></dotnetbarlayout>";
            }
        }

        /// <summary>
        /// 插件加载时执行的方法，用于初始化配置檔案。
        /// </summary>
        internal async void PluginOnLoad()
        {
            ConfigManager.RegisterAllSection(this.GetType().Assembly);
            var conf = ImageHandlerConfig.Default;
            //刷新最近打开檔案列表
            refreshRecentDocItems();
            //读取CharaSim配置
            UpdateCharaSimSettings();
            //wz加载配置
            UpdateWzLoadingSettings();
            //Translator Configuration Load
            UpdateTranslateSettings();
            //Discord Config
            UpdateDiscordConfig();

            //杂项配置
            labelItemAutoSaveFolder.Text = ImageHandlerConfig.Default.AutoSavePictureFolder;
            buttonItemAutoSave.Checked = ImageHandlerConfig.Default.AutoSaveEnabled;
            comboBoxItemLanguage.SelectedIndex = Clamp(CharaSimConfig.Default.SelectedFontIndex, 0, comboBoxItemLanguage.Items.Count);
            buttonItemIgnoreArticles.Checked = WcR2Config.Default.IgnoreArticles;

            //更新界面颜色
            styleManager1.ManagerStyle = WcR2Config.Default.MainStyle;
            UpdateButtonItemStyles();
            styleManager1.ManagerColorTint = WcR2Config.Default.MainStyleColor;
        }

        void UpdateCharaSimSettings()
        {
            var Setting = CharaSimConfig.Default;
            this.buttonItemAutoQuickView.Checked = Setting.AutoQuickView;
            tooltipQuickView.PreferredStringCopyMethod = Setting.PreferredStringCopyMethod;
            tooltipQuickView.CopyParsedSkillString = Setting.CopyParsedSkillString;
            tooltipQuickView.Enable22AniStyle = Setting.Enable22AniStyle;
            tooltipQuickView.SkillRender.ShowProperties = Setting.Skill.ShowProperties;
            tooltipQuickView.SkillRender.ShowObjectID = Setting.Skill.ShowID;
            tooltipQuickView.SkillRender.ShowDelay = Setting.Skill.ShowDelay;
            tooltipQuickView.SkillRender.DisplayCooltimeMSAsSec = Setting.Skill.DisplayCooltimeMSAsSec;
            tooltipQuickView.SkillRender.DisplayPermyriadAsPercent = Setting.Skill.DisplayPermyriadAsPercent;
            tooltipQuickView.SkillRender.IgnoreEvalError = Setting.Skill.IgnoreEvalError;
            tooltipQuickView.SkillRender.ShowRangeCoordinates = Setting.Skill.ShowRangeCoordinates;
            tooltipQuickView.SkillRender.Enable22AniStyle = Setting.Enable22AniStyle;
            this.skillDefaultLevel = Setting.Skill.DefaultLevel;
            this.skillInterval = Setting.Skill.IntervalLevel;
            tooltipQuickView.GearRender.ShowObjectID = Setting.Gear.ShowID;
            tooltipQuickView.GearRender.ShowSpeed = Setting.Gear.ShowWeaponSpeed;
            tooltipQuickView.GearRender.ShowLevelOrSealed = Setting.Gear.ShowLevelOrSealed;
            tooltipQuickView.GearRender.ShowMedalTag = Setting.Gear.ShowMedalTag;
            tooltipQuickView.GearRender.ShowSoldPrice = Setting.Gear.ShowSoldPrice;
            tooltipQuickView.GearRender.ShowCashPurchasePrice = Setting.Gear.ShowCashPurchasePrice;
            tooltipQuickView.GearRender.ShowCombatPower = Setting.Gear.ShowCombatPower;
            tooltipQuickView.GearRender.AutoTitleWrap = Setting.Gear.AutoTitleWrap;
            tooltipQuickView.GearRender.CosmeticHairColor = Setting.Item.CosmeticHairColor;
            tooltipQuickView.GearRender.CosmeticFaceColor = Setting.Item.CosmeticFaceColor;
            tooltipQuickView.GearRender.ShowApplicablePet = Setting.Misc.LocatePetEquip;
            tooltipQuickView.GearRender3.ShowObjectID = Setting.Gear.ShowID;
            tooltipQuickView.GearRender3.ShowSpeed = Setting.Gear.ShowWeaponSpeed;
            tooltipQuickView.GearRender3.ShowLevelOrSealed = Setting.Gear.ShowLevelOrSealed;
            tooltipQuickView.GearRender3.ShowSoldPrice = Setting.Gear.ShowSoldPrice;
            tooltipQuickView.GearRender3.ShowCashPurchasePrice = Setting.Gear.ShowCashPurchasePrice;
            tooltipQuickView.GearRender3.AutoTitleWrap = Setting.Gear.AutoTitleWrap;
            tooltipQuickView.GearRender3.CosmeticHairColor = Setting.Item.CosmeticHairColor;
            tooltipQuickView.GearRender3.CosmeticFaceColor = Setting.Item.CosmeticFaceColor;
            tooltipQuickView.GearRender3.ShowApplicablePet = Setting.Misc.LocatePetEquip;
            tooltipQuickView.ItemRender.ShowObjectID = Setting.Item.ShowID;
            tooltipQuickView.ItemRender.LinkRecipeInfo = Setting.Item.LinkRecipeInfo;
            tooltipQuickView.ItemRender.LinkRecipeItem = Setting.Item.LinkRecipeItem;
            tooltipQuickView.ItemRender.ShowLevelOrSealed = Setting.Gear.ShowLevelOrSealed;
            tooltipQuickView.ItemRender.ShowNickTag = Setting.Item.ShowNickTag;
            tooltipQuickView.ItemRender.ShowSoldPrice = Setting.Item.ShowSoldPrice;
            tooltipQuickView.ItemRender.ShowCashPurchasePrice = Setting.Item.ShowCashPurchasePrice;
            tooltipQuickView.ItemRender.ShowLinkedTamingMob = Setting.Item.ShowLinkedTamingMob;
            tooltipQuickView.ItemRender.CosmeticHairColor = Setting.Item.CosmeticHairColor;
            tooltipQuickView.ItemRender.CosmeticFaceColor = Setting.Item.CosmeticFaceColor;
            tooltipQuickView.ItemRender.Enable22AniStyle = Setting.Enable22AniStyle;
            tooltipQuickView.ItemRender.ShowDamageSkin = Setting.DamageSkin.ShowDamageSkin;
            tooltipQuickView.ItemRender.ShowDamageSkinID = Setting.DamageSkin.ShowDamageSkinID;
            tooltipQuickView.ItemRender.UseMiniSizeDamageSkin = Setting.DamageSkin.UseMiniSize;
            tooltipQuickView.ItemRender.AlwaysUseMseaFormatDamageSkin = Setting.DamageSkin.AlwaysUseMseaFormat;
            tooltipQuickView.ItemRender.DisplayUnitOnSingleLine = Setting.DamageSkin.DisplayUnitOnSingleLine;
            tooltipQuickView.ItemRender.UseInGameSpacing = Setting.DamageSkin.UseInGameSpacing;
            tooltipQuickView.ItemRender.DamageSkinNumber = Setting.DamageSkin.DamageSkinNumber;
            tooltipQuickView.ItemRender.AllowFamiliarOutOfBounds = Setting.Familiar.AllowOutOfBounds;
            tooltipQuickView.ItemRender.UseCTFamiliarRender = Setting.Familiar.UseCTFamiliarUI;
            tooltipQuickView.ItemRender.ShowApplicablePetEquip = Setting.Misc.LocatePetEquip;
            tooltipQuickView.ItemRender3.ShowObjectID = Setting.Item.ShowID;
            tooltipQuickView.ItemRender3.LinkRecipeInfo = Setting.Item.LinkRecipeInfo;
            tooltipQuickView.ItemRender3.LinkRecipeItem = Setting.Item.LinkRecipeItem;
            tooltipQuickView.ItemRender3.ShowLevelOrSealed = Setting.Gear.ShowLevelOrSealed;
            tooltipQuickView.ItemRender3.ShowNickTag = Setting.Item.ShowNickTag;
            tooltipQuickView.ItemRender3.ShowSoldPrice = Setting.Item.ShowSoldPrice;
            tooltipQuickView.ItemRender3.ShowCashPurchasePrice = Setting.Item.ShowCashPurchasePrice;
            tooltipQuickView.ItemRender3.ShowLinkedTamingMob = Setting.Item.ShowLinkedTamingMob;
            tooltipQuickView.ItemRender3.ShowApplicablePetEquip = Setting.Misc.LocatePetEquip;
            tooltipQuickView.ItemRender3.CosmeticHairColor = Setting.Item.CosmeticHairColor;
            tooltipQuickView.ItemRender3.CosmeticFaceColor = Setting.Item.CosmeticFaceColor;
            tooltipQuickView.ItemRender3.ShowDamageSkin = Setting.DamageSkin.ShowDamageSkin;
            tooltipQuickView.ItemRender3.ShowDamageSkinID = Setting.DamageSkin.ShowDamageSkinID;
            tooltipQuickView.ItemRender3.UseMiniSizeDamageSkin = Setting.DamageSkin.UseMiniSize;
            tooltipQuickView.ItemRender3.AlwaysUseMseaFormatDamageSkin = Setting.DamageSkin.AlwaysUseMseaFormat;
            tooltipQuickView.ItemRender3.DisplayUnitOnSingleLine = Setting.DamageSkin.DisplayUnitOnSingleLine;
            tooltipQuickView.ItemRender3.UseInGameSpacing = Setting.DamageSkin.UseInGameSpacing;
            tooltipQuickView.ItemRender3.DamageSkinNumber = Setting.DamageSkin.DamageSkinNumber;
            tooltipQuickView.ItemRender3.AllowFamiliarOutOfBounds = Setting.Familiar.AllowOutOfBounds;
            tooltipQuickView.ItemRender3.UseCTFamiliarRender = Setting.Familiar.UseCTFamiliarUI;
            tooltipQuickView.UseCTFamiliarUI = Setting.Familiar.UseCTFamiliarUI;
            tooltipQuickView.FamiliarRender.AllowOutOfBounds = Setting.Familiar.AllowOutOfBounds;
            tooltipQuickView.FamiliarRender2.AllowOutOfBounds = Setting.Familiar.AllowOutOfBounds;
            tooltipQuickView.EnableAssembleTooltip = Setting.Item.UseAssembleUI;
            tooltipQuickView.MapRender.ShowMiniMap = Setting.Map.ShowMiniMap;
            tooltipQuickView.MapRender.ShowObjectID = Setting.Map.ShowMapObjectID;
            tooltipQuickView.MapRender.ShowMobNpcObjectID = Setting.Map.ShowMobNpcObjectID;
            tooltipQuickView.MapRender.ShowBgmName = Setting.Map.ShowBgmName;
            tooltipQuickView.MapRender.Enable22AniStyle = Setting.Enable22AniStyle;
            tooltipQuickView.MapRender.ShowMiniMapMob = Setting.Map.ShowMiniMapMob;
            tooltipQuickView.MapRender.ShowMiniMapNpc = Setting.Map.ShowMiniMapNpc;
            tooltipQuickView.MapRender.ShowMiniMapPortal = Setting.Map.ShowMiniMapPortal;
            tooltipQuickView.MobRender.MaxWidth = Screen.PrimaryScreen.Bounds.Width;
            tooltipQuickView.MobRender.ShowAllSubMobAtOnce = Setting.Mob.ShowAllSubMobAtOnce;
            tooltipQuickView.MobRender.EnableWorldArchive = Setting.Misc.EnableWorldArchive;
            tooltipQuickView.MobRender.EnableMonsterBook = Setting.Mob.EnableMonsterBook;
            tooltipQuickView.NpcRender.ShowAllIllustAtOnce = Setting.Npc.ShowAllIllustAtOnce;
            tooltipQuickView.NpcRender.ShowNpcQuotes = Setting.Npc.ShowNpcQuotes;
            tooltipQuickView.NpcRender.EnableWorldArchive = Setting.Misc.EnableWorldArchive;
            tooltipQuickView.QuestRender.ShowObjectID = Setting.Quest.ShowID;
            tooltipQuickView.QuestRender.DefaultState = Setting.Quest.DefaultState;
            tooltipQuickView.QuestRender.ShowAllStates = Setting.Quest.ShowAllStates;

            tooltipQuickView.RecipeRender.ShowObjectID = Setting.Recipe.ShowID;
            tooltipQuickView.RecipeRender.Enable22AniStyle = Setting.Enable22AniStyle;
            GearGraphics.is22aniStyle = Setting.Enable22AniStyle;
            charaSimCtrl.Enable22AniStyle = Setting.Enable22AniStyle;
        }

        void UpdateWzLoadingSettings()
        {
            var config = WcR2Config.Default;
            Encoding enc;
            try
            {
                enc = Encoding.GetEncoding(config.WzEncoding);
            }
            catch
            {
                enc = null;
            }
            Wz_Structure.DefaultEncoding = enc;
            Wz_Structure.DefaultAutoDetectExtFiles = config.AutoDetectExtFiles;
            Wz_Structure.DefaultImgCheckDisabled = config.ImgCheckDisabled;
            switch (config.PreferredClientRegion)
            {
                default:
                case 0:
                    this.buttonItemJMS.Checked = true;
                    break;
                case 1:
                    this.buttonItemKMS.Checked = true;
                    break;
                case 2:
                    this.buttonItemKMST.Checked = true;
                    break;
                case 3:
                    this.buttonItemMSN.Checked = true;
                    break;
                case 4:
                    this.buttonItemGMS.Checked = true;
                    break;
                case 5:
                    this.buttonItemGMSC.Checked = true;
                    break;
                case 6:
                    this.buttonItemGMSC.Checked = true;
                    break;
                case 7:
                    this.buttonItemGMSPTS.Checked = true;
                    break;
                case 8:
                    this.buttonItemCMS.Checked = true;
                    break;
            }
        }

        void UpdateTranslateSettings()
        {
            var config = WcR2Config.Default;
            Translator.DefaultDesiredLanguage = config.DesiredLanguage;
            Translator.DefaultMozhiBackend = config.MozhiBackend;
            Translator.DefaultLanguageModel = config.LanguageModel;
            Translator.OAITranslateBaseURL = config.OpenAIBackend;
            Translator.DefaultOpenAISystemMessage = config.OpenAISystemMessage;
            Translator.DefaultPreferredTranslateEngine = config.PreferredTranslateEngine;
            Translator.DefaultTranslateAPIKey = config.NxSecretKey;
            Translator.DefaultPreferredLayout = config.PreferredLayout;
            Translator.IsTranslateEnabled = (config.PreferredLayout > 0);
            Translator.DefaultDetectCurrency = config.DetectCurrency;
            Translator.DefaultDesiredCurrency = config.DesiredCurrency;
            Translator.DefaultLMTemperature = config.LMTemperature;
            Translator.DefaultMaximumToken = config.MaximumToken;
            Translator.IsExtraParamEnabled = config.OpenAIExtraOption;
            Translator.ExchangeTable = null;
            Translator.InitializeCache();
        }

        void UpdateDiscordConfig()
        {
            var config = WcR2Config.Default;
            DiscordService.BotToken = config.DiscordBotToken;
            DiscordService.ChannelIDList = config.DiscordChannelID;
        }

        async Task AutomaticCheckUpdate()
        {
            var config = WcR2Config.Default;
            if (config.EnableAutoUpdate)
            {
                var updater = new Updater();
                try
                {
                    await updater.QueryUpdateAsync();
                    if (updater.UpdateAvailable)
                    {
                        ToastNotification.Show(this, $"發現更新: {updater.LatestVersionString}", 5000, eToastPosition.TopCenter);
                        var frmUpdater = new FrmUpdater(updater, styleManager1.ManagerStyle == eStyle.VisualStudio2012Dark);
                        frmUpdater.LoadConfig(config);
                        frmUpdater.ShowDialog(this);
                    }
                }
                catch
                {
                    // ignore error
                }
            }
        }

        void CharaSimLoader_WzFileFinding(object sender, FindWzEventArgs e)
        {
            string[] fullPath = null;
            if (!string.IsNullOrEmpty(e.FullPath)) //用fullpath作为输入参数
            {
                fullPath = e.FullPath.Split('/', '\\');
                e.WzType = Enum.TryParse<Wz_Type>(fullPath[0], true, out var wzType) ? wzType : Wz_Type.Unknown;
            }

            List<Wz_Node> preSearch = new List<Wz_Node>();
            if (e.WzType != Wz_Type.Unknown) //用wztype作为输入参数
            {
                IEnumerable<Wz_Structure> preSearchWz = e.WzFile?.WzStructure != null ?
                    Enumerable.Repeat(e.WzFile.WzStructure, 1) :
                    this.openedWz;
                foreach (var wzs in preSearchWz)
                {
                    Wz_File baseWz = null;
                    bool find = false;
                    foreach (Wz_File wz_f in wzs.wz_files)
                    {
                        if (wz_f.Type == e.WzType)
                        {
                            if (wz_f.Node.Nodes.Count <= 0)
                            {
                                continue;
                            }
                            preSearch.Add(wz_f.Node);
                            find = true;
                            //e.WzFile = wz_f;
                        }
                        if (wz_f.Type == Wz_Type.Base)
                        {
                            baseWz = wz_f;
                        }
                    }

                    // detect data.wz
                    if (baseWz != null && !find)
                    {
                        string key = e.WzType.ToString();
                        foreach (Wz_Node node in baseWz.Node.Nodes)
                        {
                            if (node.Text == key && node.Nodes.Count > 0)
                            {
                                preSearch.Add(node);
                            }
                        }
                    }
                }
            }

            if (fullPath == null || fullPath.Length <= 1)
            {
                if (e.WzType != Wz_Type.Unknown && preSearch.Count > 0) //返回wzFile
                {
                    e.WzNode = preSearch[0];
                    e.WzFile = preSearch[0].Value as Wz_File;
                }
                return;
            }

            if (preSearch.Count <= 0)
            {
                return;
            }

            foreach (var wzFileNode in preSearch)
            {
                var searchNode = wzFileNode;
                for (int i = 1; i < fullPath.Length && searchNode != null; i++)
                {
                    searchNode = searchNode.Nodes[fullPath[i]];
                    var img = searchNode.GetValueEx<Wz_Image>(null);
                    if (img != null)
                    {
                        searchNode = img.TryExtract() ? img.Node : null;
                    }
                }

                if (searchNode != null)
                {
                    e.WzNode = searchNode;
                    e.WzFile = wzFileNode.Value as Wz_File;
                    return;
                }
            }
            //寻找失败
            e.WzNode = null;
        }

        #region 界面主题配置
        private void createStyleItems()
        {
            //添加菜单
            foreach (eStyle style in Enum.GetValues(typeof(eStyle)).OfType<eStyle>().Distinct())
            {
                var buttonItemStyle = new ButtonItem() { Tag = style, Text = style.ToString(), Checked = (styleManager1.ManagerStyle == style) };
                buttonItemStyle.Click += new EventHandler(buttonItemStyle_Click);
                this.buttonItemStyle.SubItems.Add(buttonItemStyle);
            }

            var styleColorPicker = new ColorPickerDropDown() { Text = "StyleColorTint", BeginGroup = true, SelectedColor = styleManager1.ManagerColorTint };
            styleColorPicker.SelectedColorChanged += new EventHandler(styleColorPicker_SelectedColorChanged);
            buttonItemStyle.SubItems.Add(styleColorPicker);
        }

        private void buttonItemStyle_Click(object sender, EventArgs e)
        {
            var style = (eStyle)((sender as ButtonItem).Tag);
            styleManager1.ManagerStyle = style;
            UpdateButtonItemStyles();
            ConfigManager.Reload();
            WcR2Config.Default.MainStyle = style;
            ConfigManager.Save();
        }

        private void UpdateButtonItemStyles()
        {
            foreach (BaseItem item in buttonItemStyle.SubItems)
            {
                ButtonItem buttonItem = item as ButtonItem;
                if (buttonItem != null)
                {
                    buttonItem.Checked = (buttonItem.Tag as eStyle?) == styleManager1.ManagerStyle;
                }
            }

            if (styleManager1.ManagerStyle == eStyle.VisualStudio2012Dark)
            {
                this.elementStyle1.TextColor = System.Drawing.Color.LightGray;
                this.elementStyle2.TextColor = System.Drawing.Color.LightGray;
                this.elementStyle3.TextColor = System.Drawing.Color.LightGray;
                this.listViewExWzDetail.BackColor = System.Drawing.Color.FromArgb(-13816528);
                this.listViewExWzDetail.ForeColor = System.Drawing.Color.LightGray;
                this.listViewExWzDetail.GridLines = false;
                this.listViewExString.BackColor = System.Drawing.Color.FromArgb(-13816528);
                this.listViewExString.ForeColor = System.Drawing.Color.LightGray;
                this.listViewExString.GridLines = false;
                this.pictureBoxEx1.BackColor = System.Drawing.Color.FromArgb(-13816528);
                this.pictureBoxEx1.PictureBoxInfoText = Microsoft.Xna.Framework.Color.LightGray;
                //this.chkEnableDarkMode.Checked = true;
                this.clbRootNode.BackColor = System.Drawing.Color.FromArgb(-13816528);
                this.clbRootNode.ForeColor = System.Drawing.Color.LightGray;
            }
            else
            {
                this.elementStyle1.TextColor = System.Drawing.SystemColors.ControlText;
                this.elementStyle2.TextColor = System.Drawing.SystemColors.ControlText;
                this.elementStyle3.TextColor = System.Drawing.SystemColors.ControlText;
                this.listViewExWzDetail.BackColor = System.Drawing.Color.White;
                this.listViewExWzDetail.ForeColor = System.Drawing.Color.Black;
                this.listViewExWzDetail.GridLines = true;
                this.listViewExString.BackColor = System.Drawing.Color.White;
                this.listViewExString.ForeColor = System.Drawing.Color.Black;
                this.listViewExString.GridLines = true;
                this.pictureBoxEx1.BackColor = System.Drawing.Color.White;
                this.pictureBoxEx1.PictureBoxInfoText = Microsoft.Xna.Framework.Color.Black;
                //this.chkEnableDarkMode.Checked = false;
                this.clbRootNode.BackColor = System.Drawing.Color.White;
                this.clbRootNode.ForeColor = System.Drawing.Color.Black;
            }

            // 讓外掛（例如 MapleStoryDB2）能跟著切換配色。
            this.mainStyleChanged?.Invoke(this, EventArgs.Empty);
        }

        private void styleColorPicker_SelectedColorChanged(object sender, EventArgs e)
        {
            var color = (sender as ColorPickerDropDown).SelectedColor;
            styleManager1.ManagerColorTint = color;
            ConfigManager.Reload();
            WcR2Config.Default.MainStyleColor = color;
            ConfigManager.Save();
        }
        #endregion

        #region 读取wz相关方法
        private Node createNode(Wz_Node wzNode)
        {
            if (wzNode == null)
                return null;

            Node parentNode = new Node(wzNode.Text) { Tag = new WeakReference(wzNode) };
            foreach (Wz_Node subNode in wzNode.Nodes)
            {
                Node subTreeNode = createNode(subNode);
                if (subTreeNode != null)
                    parentNode.Nodes.Add(subTreeNode);
            }
            return parentNode;
        }

        private void sortWzNode(Wz_Node wzNode)
        {
            this.sortWzNode(wzNode, WcR2Config.Default.SortWzByImgID);
        }

        private void sortWzNode(Wz_Node wzNode, bool sortByImgID)
        {
            if (wzNode.Nodes.Count > 1)
            {
                if (sortByImgID)
                {
                    wzNode.Nodes.SortByImgID();
                }
                else
                {
                    wzNode.Nodes.Sort();
                }
            }
            foreach (Wz_Node subNode in wzNode.Nodes)
            {
                sortWzNode(subNode, sortByImgID);
            }
        }
        #endregion

        #region wz提取右侧
        private void cmbItemAniNames_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.cmbItemAniNames.SelectedIndex > -1 && this.pictureBoxEx1.Items.Count > 0)
            {
                if (this.pictureBoxEx1.Items[0] is ISpineAnimator aniItem)
                {
                    string aniName = this.cmbItemAniNames.SelectedItem as string;
                    aniItem.SelectedAnimationName = aniName;
                    this.cmbItemAniNames.Tooltip = aniName;
                }
                else if (this.pictureBoxEx1.Items[0] is FrameAnimator frameAni && this.cmbItemAniNames.SelectedItem is int selectedpage)
                {
                    if (frameAni.Data.Frames.Count == 1)
                    {
                        var png = frameAni.Data.Frames[0].Png;
                        if (png != null && png.ActualPages > 1 && 0 <= selectedpage && selectedpage < png.ActualPages)
                        {
                            this.pictureBoxEx1.ShowImage(png, selectedpage);
                        }
                    }
                }
                this.pictureBoxEx1.UpdateLength(0);
            }
        }

        private void cmbItemSkins_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.cmbItemSkins.SelectedIndex > -1 && this.pictureBoxEx1.Items.Count > 0)
            {
                if (this.pictureBoxEx1.Items[0] is ISpineAnimator aniItem)
                {
                    string skinName = this.cmbItemSkins.SelectedItem as string;
                    aniItem.SelectedSkin = skinName;
                    this.cmbItemSkins.Tooltip = skinName;
                }
                this.pictureBoxEx1.UpdateLength(0);
            }
        }

        private void buttonItemSaveImage_Click(object sender, EventArgs e)
        {
            this.OnSaveImage(false);
        }

        private void buttonItemSaveWithOptions_Click(object sender, EventArgs e)
        {
            this.OnSaveImage(true);
        }

        private Node handleUol(Node currentNode, string uolString)
        {
            if (currentNode == null || currentNode.Parent == null || string.IsNullOrEmpty(uolString))
                return null;
            string[] dirs = uolString.Split('/');
            currentNode = currentNode.Parent;

            for (int i = 0; i < dirs.Length; i++)
            {
                string dir = dirs[i];
                if (dir == "..")
                {
                    currentNode = currentNode.Parent;
                }
                else
                {
                    bool find = false;
                    foreach (Node child in currentNode.Nodes)
                    {
                        if (child.Text == dir)
                        {
                            currentNode = child;
                            find = true;
                            break;
                        }
                    }
                    if (!find)
                        currentNode = null;
                }
                if (currentNode == null)
                    return null;
            }
            return currentNode;
        }

        private void labelItemAutoSaveFolder_Click(object sender, EventArgs e)
        {
            string dir = ImageHandlerConfig.Default.AutoSavePictureFolder;
            if (!string.IsNullOrEmpty(dir))
            {
                System.Diagnostics.Process.Start("explorer.exe", dir);
            }
        }

        private void buttonItemGif_Click(object sender, EventArgs e)
        {
            if (advTree3.SelectedNode == null)
                return;

            Wz_Node node = advTree3.SelectedNode.AsWzNode();
            string aniName = GetSelectedNodeImageName();
            if (this.pictureBoxEx1.IsPaused)
            {
                ResumePictureBox();
            }

            //添加到动画控件
            var spineDetectResult = SpineLoader.Detect(node);
            if (spineDetectResult.Success)
            {
                var spineData = this.pictureBoxEx1.LoadSpineAnimation(spineDetectResult);

                if (spineData != null)
                {
                    this.pictureBoxEx1.ShowAnimation(spineData);
                    var aniItem = this.pictureBoxEx1.Items[0] as ISpineAnimator;

                    this.cmbItemAniNames.Items.Clear();
                    this.cmbItemAniNames.Items.Add("");
                    this.cmbItemAniNames.Items.AddRange(aniItem.Animations.ToArray());
                    this.cmbItemAniNames.SelectedIndex = 0;

                    this.cmbItemSkins.Visible = true;
                    this.cmbItemSkins.Items.Clear();
                    this.cmbItemSkins.Items.AddRange(aniItem.Skins.ToArray());
                    this.cmbItemSkins.SelectedIndex = aniItem.Skins.IndexOf(aniItem.SelectedSkin);
                }
            }
            else if (node.Value is Wz_Video)
            {
                var origin = node.FindNodeByPath("origin").GetValueEx<Wz_Vector>(null);
                var videoFrameData = this.pictureBoxEx1.LoadVideo(node.Value as Wz_Video, origin);

                if (videoFrameData != null)
                {
                    this.pictureBoxEx1.ShowAnimation(videoFrameData);
                    this.cmbItemAniNames.Items.Clear();
                    this.cmbItemSkins.Visible = false;
                }
            }
            else
            {
                var options = (sender == this.buttonItemExtractGifEx) ? FrameAnimationCreatingOptions.ScanAllChildrenFrames: default;
                var frameData = this.pictureBoxEx1.LoadFrameAnimation(node, options);

                if (frameData != null)
                {
                    this.pictureBoxEx1.ShowAnimation(frameData);
                    this.cmbItemAniNames.Items.Clear();
                    this.cmbItemSkins.Visible = false;
                }
                else
                {
                    var multiData = this.pictureBoxEx1.LoadMultiFrameAnimation(node);

                    if (multiData != null)
                    {
                        this.pictureBoxEx1.ShowAnimation(multiData);
                        var aniItem = this.pictureBoxEx1.Items[0] as Animation.MultiFrameAnimator;

                        this.cmbItemAniNames.Items.Clear();
                        this.cmbItemAniNames.Items.AddRange(aniItem.Animations.ToArray());
                        this.cmbItemAniNames.SelectedIndex = 0;
                    }
                }
            }
            this.pictureBoxEx1.PictureName = aniName;
        }

        private void buttonItemGif2_Click(object sender, EventArgs e)
        {
            // code from buttonItemGif_Click()
            // Todo: reimplement overall overlay feature
            // keep each animation item instead of merge them into one
            if (advTree3.SelectedNode == null)
                return;

            Wz_Node node = advTree3.SelectedNode.AsWzNode();
            string aniName = "堆叠_" + GetSelectedNodeImageName();
            if (this.pictureBoxEx1.IsPaused)
            {
                ResumePictureBox();
            }

            if (node.Value is Wz_Png || node.Value is IWzPngSource)
            {
                var pngFrameData = this.pictureBoxEx1.LoadPngFrameAnimation(node);

                if (pngFrameData != null)
                {
                    this.pictureBoxEx1.ShowOverlayAnimation(pngFrameData, isPngFrameAni: true);
                    this.cmbItemAniNames.Items.Clear();
                    this.cmbItemSkins.Visible = false;
                    this.pictureBoxEx1.PictureName = aniName;
                }

                return;
            }
            else if (node.Value is Wz_Video)
            {
                var origin = node.FindNodeByPath("origin").GetValueEx<Wz_Vector>(null);
                var videoFrameData = this.pictureBoxEx1.LoadVideo(node.Value as Wz_Video, origin);

                if (videoFrameData != null)
                {
                    this.pictureBoxEx1.ShowOverlayAnimation(videoFrameData);
                    this.cmbItemAniNames.Items.Clear();
                    this.cmbItemSkins.Visible = false;
                    this.pictureBoxEx1.PictureName = aniName;
                }

                return;
            }

            var spineDetectResult = SpineLoader.Detect(node);
            if (spineDetectResult.Success)
            {
                var spineData = this.pictureBoxEx1.LoadSpineAnimation(spineDetectResult);

                if (spineData != null)
                {
                    var aniItem = spineData.CreateAnimator() as AnimationItem;

                    var frmOverlayAniOptions = new FrmOverlaySpineOptions(aniItem);
                    var name = "";
                    var skin = "";
                    var delay = 0;

                    if (frmOverlayAniOptions.ShowDialog() == DialogResult.OK)
                    {
                        frmOverlayAniOptions.GetValues(out name, out skin, out delay);
                        this.pictureBoxEx1.ShowSpineOverlayAnimation(aniItem, delay);
                        this.cmbItemAniNames.Items.Clear();
                        this.cmbItemSkins.Visible = false;
                        this.pictureBoxEx1.PictureName = $"{aniName}_{name}";
                    }
                    else
                    {
                        this.pictureBoxEx1.DisposeAnimationItem(aniItem);
                    }

                    /*
                    var frameData = this.pictureBoxEx1.ConvertSpineToFrameAnimation(aniItem, delay);

                    if (frameData != null)
                    {
                        this.pictureBoxEx1.ShowOverlayAnimation(frameData);
                        this.cmbItemAniNames.Items.Clear();
                        this.cmbItemSkins.Visible = false;
                        this.pictureBoxEx1.PictureName = name;
                    }
                    */
                }
                return;
            }
            else
            {
                var options = (sender == this.buttonOverlayExtractGifEx) ? FrameAnimationCreatingOptions.ScanAllChildrenFrames : default;
                var frameData = this.pictureBoxEx1.LoadFrameAnimation(node, options);

                if (frameData != null)
                {
                    this.pictureBoxEx1.ShowOverlayAnimation(frameData);
                    this.cmbItemAniNames.Items.Clear();
                    this.cmbItemSkins.Visible = false;
                    this.pictureBoxEx1.PictureName = aniName;
                }
                else
                {
                    var multiData = this.pictureBoxEx1.LoadMultiFrameAnimation(node);

                    if (multiData != null)
                    {
                        var aniItem = new MultiFrameAnimator(multiData);

                        var frmOverlayAniOptions = new FrmOverlaySpineOptions(aniItem);
                        var name = "";
                        var skin = "";
                        var delay = 0;

                        if (frmOverlayAniOptions.ShowDialog() == DialogResult.OK)
                        {
                            frmOverlayAniOptions.GetValues(out name, out skin, out delay);

                            foreach (var kv_frames in aniItem.Data.Frames)
                            {
                                var selectedFrameData = new FrameAnimationData(kv_frames.Value);
                                if (kv_frames.Key == name)
                                {
                                    this.pictureBoxEx1.ShowOverlayAnimation(new FrameAnimationData(aniItem.Data.Frames[name]), multiFrameInfo: name);
                                    this.cmbItemAniNames.Items.Clear();
                                    this.cmbItemSkins.Visible = false;
                                    this.pictureBoxEx1.PictureName = $"{aniName}_{name}";
                                }
                                else
                                {
                                    this.pictureBoxEx1.DisposeAnimationItem(new FrameAnimator(selectedFrameData));
                                }
                            }
                        }
                        else
                        {
                            this.pictureBoxEx1.DisposeAnimationItem(aniItem);
                        }

                        /*
                        foreach (var kv_frames in multiData.Frames)
                        {
                            var selectedFrameData = new FrameAnimationData(kv_frames.Value);

                            this.pictureBoxEx1.ShowOverlayAnimation(selectedFrameData, multiFrameInfo: kv_frames.Key);
                        }
                        this.cmbItemAniNames.Items.Clear();
                        this.cmbItemSkins.Visible = false;
                        this.pictureBoxEx1.PictureName = aniName;
                        */
                    }

                    return;
                }
            }
            //this.pictureBoxEx1.PictureName = aniName;
        }

        private void OverlayMultiFrameWithKey(object sender, EventArgs e)
        {
            if (advTree3.SelectedNode == null)
                return;

            Wz_Node node = advTree3.SelectedNode.AsWzNode();
            string aniName = "堆叠_" + GetSelectedNodeImageName();

            if ((sender as ButtonItem).Name != aniName)
            {
                MessageBoxEx.Show("載入的多重幀列表與目前選定的節點不匹配。", "錯誤");
                return;
            }

            var multiData = this.pictureBoxEx1.LoadMultiFrameAnimation(node);
            var key = (sender as ButtonItem).Text;

            if (multiData != null && multiData.Frames.ContainsKey(key))
            {
                var selectedFrameData = new FrameAnimationData(multiData.Frames[key]);
                this.pictureBoxEx1.ShowOverlayAnimation(selectedFrameData, multiFrameInfo: key);
                this.cmbItemAniNames.Items.Clear();
                this.cmbItemSkins.Visible = false;
                this.pictureBoxEx1.PictureName = aniName;
            }

            return;
        }

        private string GetSelectedNodeImageName()
        {
            Wz_Node node = advTree3.SelectedNode.AsWzNode();

            string aniName;
            switch (ImageHandlerConfig.Default.ImageNameMethod.Value)
            {
                default:
                case ImageNameMethod.Default:
                    advTree3.PathSeparator = ".";
                    aniName = advTree3.SelectedNode.FullPath;
                    break;

                case ImageNameMethod.PathToImage:
                    aniName = node.FullPath.Replace('\\', '.');
                    break;

                case ImageNameMethod.PathToWz:
                    aniName = node.FullPathToFile.Replace('\\', '.');
                    break;
            }

            return aniName;
        }

        private void buttonItemGifSetting_Click(object sender, EventArgs e)
        {
            FrmGifSetting frm = new FrmGifSetting();
            frm.Load(ImageHandlerConfig.Default);
            frm.FFmpegBinPathHint = FFmpegEncoder.DefaultExecutionFileName;
            frm.FFmpegArgumentHint = FFmpegEncoder.DefaultArgumentFormat;
            frm.FFmpegDefaultExtensionHint = FFmpegEncoder.DefaultOutputFileExtension;
            if (frm.ShowDialog() == DialogResult.OK)
            {
                ConfigManager.Reload();
                frm.Save(ImageHandlerConfig.Default);
                ConfigManager.Save();
            }
        }

        private void buttonDisableOverlayAni_Click(object sender, EventArgs e)
        {
            if (this.pictureBoxEx1.ShowOverlayAni)
            {
                this.pictureBoxEx1.ShowOverlayAni = false;
                this.pictureBoxEx1.DisposeItemList();
            }
        }

        private void buttonHitboxOverlay_Click(object sender, EventArgs e)
        {
            if (this.pictureBoxEx1.ShowOverlayAni)
            {
                Wz_Node node = advTree3.SelectedNode?.AsWzNode() ?? null;
                var frameData = this.pictureBoxEx1.LoadFrameAnimation(node, loadTexture: false);
                this.pictureBoxEx1.AddHitboxOverlay(frameData);
            }
        }

        private void buttonLoadMultiFrameAniList_Click(object sender, EventArgs e)
        {
            if (advTree3.SelectedNode == null)
                return;

            Wz_Node node = advTree3.SelectedNode.AsWzNode();
            string aniNameKey = "堆叠_" + GetSelectedNodeImageName();

            if ((sender as ButtonItem).Name == aniNameKey)
            {
                return;
            }

            this.buttonLoadMultiFrameAniList.SubItems.Clear();
            (sender as ButtonItem).Name = aniNameKey;

            var list = MultiFrameAnimationData.CreateListFromNode(node, PluginBase.PluginManager.FindWz);
            if (list.Count > 0)
            {
                this.buttonLoadMultiFrameAniList.SubItems.AddRange(list.Select(item =>
                {
                    var buttonItem = new DevComponents.DotNetBar.ButtonItem();
                    buttonItem.Name = aniNameKey;
                    buttonItem.Text = item;
                    buttonItem.Click += new System.EventHandler(this.OverlayMultiFrameWithKey);
                    return buttonItem as BaseItem;
                }).ToArray());
            }
        }

        private void buttonItemAutoSave_Click(object sender, EventArgs e)
        {
            ConfigManager.Reload();
            ImageHandlerConfig.Default.AutoSaveEnabled = buttonItemAutoSave.Checked;
            ConfigManager.Save();
        }

        private void buttonItemAutoSaveFolder_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dlg = new FolderBrowserDialog())
            {
                dlg.Description = "選擇將自動儲存圖片的目標資料夾。";
                dlg.SelectedPath = ImageHandlerConfig.Default.AutoSavePictureFolder;
                if (DialogResult.OK == dlg.ShowDialog())
                {
                    labelItemAutoSaveFolder.Text = dlg.SelectedPath;
                    ConfigManager.Reload();
                    ImageHandlerConfig.Default.AutoSavePictureFolder = dlg.SelectedPath;
                    ConfigManager.Save();
                }
            }
        }

        private void buttonCaptureAni_Click(object sender, EventArgs e)
        {
            if (this.pictureBoxEx1.Items.Count <= 0) return;

            FrmCaptureAniOptions FrmAniCaptureOptions = new FrmCaptureAniOptions(this.pictureBoxEx1.MaxLength);
            CaptureAniOptions options = new CaptureAniOptions();

            if (FrmAniCaptureOptions.ShowDialog() == DialogResult.OK)
            {
                options = FrmAniCaptureOptions.GetValues();
            }
            else
            {
                return;
            }

            var clonedAniItem = this.pictureBoxEx1.Items.Select(aniItem => (AnimationItem)aniItem.Clone());
            var aniItemTime = this.pictureBoxEx1.ItemTimes;
            FrameAnimationData frameData = this.pictureBoxEx1.CaptureAnimation(clonedAniItem, aniItemTime, options.CaptureTime);

            if (frameData != null && frameData.Frames.Count == 1)
            {
                this.OnSavePngFile(frameData.Frames[0], captureTime: options.CaptureTime.ToString());
                this.pictureBoxEx1.DisposeAnimationItem(new FrameAnimator(frameData));
            }
            else
            {
                labelItemStatus.Text = "圖片保存失敗";
            }
        }

        private void buttonItemPBPlay_Click(object sender, EventArgs e)
        {
            if (this.pictureBoxEx1.Items.Count <= 0)
            {
                return;
            }
            if (this.pictureBoxEx1.IsPlaying)
            {
                PausePictureBox();
            }
            else
            {
                ResumePictureBox();
            }
        }

        private void PausePictureBox()
        {
            this.pictureBoxEx1.DoPause();
            this.buttonItemPBPlay.Image = global::WzComparerR2.Properties.Resources.Play;
            this.buttonItemPBPlay.Tooltip = "播放";
            this.buttonItemPBGA1.Enabled = true;
            this.buttonItemPBGA2.Enabled = true;
            this.buttonItemPBGB1.Enabled = true;
            this.buttonItemPBGB2.Enabled = true;
        }

        private void ResumePictureBox()
        {
            this.pictureBoxEx1.DoResume();
            this.buttonItemPBPlay.Image = global::WzComparerR2.Properties.Resources.Pause;
            this.buttonItemPBPlay.Tooltip = "暫停";
            this.buttonItemPBGA1.Enabled = false;
            this.buttonItemPBGA2.Enabled = false;
            this.buttonItemPBGB1.Enabled = false;
            this.buttonItemPBGB2.Enabled = false;
        }

        private void buttonItemPBGA1_Click(object sender, EventArgs e)
        {
            if (this.pictureBoxEx1.Items.Count <= 0)
            {
                return;
            }
            this.pictureBoxEx1.DoTimeUpdate(30);
        }

        private void buttonItemPBGA2_Click(object sender, EventArgs e)
        {
            if (this.pictureBoxEx1.Items.Count <= 0)
            {
                return;
            }
            this.pictureBoxEx1.DoTimeUpdate(360);
        }

        private void buttonItemPBGB1_Click(object sender, EventArgs e)
        {
            if (this.pictureBoxEx1.Items.Count <= 0)
            {
                return;
            }
            this.pictureBoxEx1.DoTimeUpdate(-30);
        }

        private void buttonItemPBGB2_Click(object sender, EventArgs e)
        {
            if (this.pictureBoxEx1.Items.Count <= 0)
            {
                return;
            }
            this.pictureBoxEx1.DoTimeUpdate(-360);
        }

        private void OnSaveImage(bool options)
        {
            if (this.pictureBoxEx1.Items.Count <= 0)
            {
                return;
            }

            var aniItem = this.pictureBoxEx1.Items;
            var aniItemTime = this.pictureBoxEx1.ItemTimes;
            var frameData = (aniItem?.FirstOrDefault(item => item is FrameAnimator) as FrameAnimator)?.Data;
            if (aniItem.Count == 1 && frameData != null && frameData.Frames.Count == 1 
                && frameData.Frames[0].A0 == 255 && frameData.Frames[0].A1 == 255 && (frameData.Frames[0].Delay == 0 || pictureBoxEx1.ShowOverlayAni))
            {
                // save still picture as png
                this.OnSavePngFile(frameData.Frames[0]);
            }
            else if (aniItem.Count == 1 && aniItem[0] is AnimationItem singleAniItem
                && singleAniItem is ISpineAnimator && singleAniItem.Length <= 0)
            {
                this.OnSaveAnimationPngFile(singleAniItem, options);
            }
            else
            {
                // save as gif/apng
                this.OnSaveGifFile(aniItem, aniItemTime, options);
            }
        }

        private void OnSavePngFile(Frame frame, string captureTime = "")
        {
            if (frame.Png != null)
            {
                var config = ImageHandlerConfig.Default;
                int page = frame.Page;
                string pngFileName = pictureBoxEx1.PictureName + (frame.Png.ActualPages > 1 ? $".{page}" : null) + ".png";

                if (config.AutoSaveEnabled)
                {
                    pngFileName = Path.Combine(config.AutoSavePictureFolder, string.Join("_", pngFileName.Replace("/", "").Replace("\\", ".").Split(Path.GetInvalidFileNameChars(), StringSplitOptions.None)));
                }
                else
                {
                    var dlg = new SaveFileDialog();
                    dlg.Filter = "PNG (*.png)|*.png|全部格式 (*.*)|*.*";
                    dlg.FileName = pngFileName;
                    if (dlg.ShowDialog() != DialogResult.OK)
                    {
                        return;
                    }

                    pngFileName = dlg.FileName;
                }

                using (var bmp = frame.Png.ExtractPng(page))
                {
                    bmp.Save(pngFileName, System.Drawing.Imaging.ImageFormat.Png);
                }
                labelItemStatus.Text = "已保存為: " + pngFileName;
            }
            else if ((pictureBoxEx1.ShowOverlayAni || !string.IsNullOrEmpty(captureTime)) && frame.Texture != null) // 애니메이션 중첩
            {
                var config = ImageHandlerConfig.Default;
                string pngFileName = string.IsNullOrEmpty(captureTime) ? pictureBoxEx1.PictureName + ".png" : $"{pictureBoxEx1.PictureName}_{captureTime}.png";

                if (config.AutoSaveEnabled)
                {
                    pngFileName = Path.Combine(config.AutoSavePictureFolder, string.Join("_", pngFileName.Replace("/", "").Replace("\\", ".").Split(Path.GetInvalidFileNameChars(), StringSplitOptions.None)));
                }
                else
                {
                    var dlg = new SaveFileDialog();
                    dlg.Filter = "PNG (*.png)|*.png|全部格式 (*.*)|*.*";
                    dlg.FileName = pngFileName;
                    if (dlg.ShowDialog() != DialogResult.OK)
                    {
                        return;
                    }

                    pngFileName = dlg.FileName;
                }

                byte[] frameData = new byte[frame.Texture.Width * frame.Texture.Height * 4];
                frame.Texture.GetData(frameData);
                var targetSize = new System.Drawing.Point(frame.Texture.Width, frame.Texture.Height);
                unsafe
                {
                    fixed (byte* pFrameBuffer = frameData)
                    {
                        using (var bmp = new System.Drawing.Bitmap(targetSize.X, targetSize.Y, targetSize.X * 4, System.Drawing.Imaging.PixelFormat.Format32bppArgb, new IntPtr(pFrameBuffer)))
                        {
                            bmp.Save(pngFileName, System.Drawing.Imaging.ImageFormat.Png);
                        }
                    }
                }
                labelItemStatus.Text = "已保存為: " + pngFileName;
            }
            else
            {
                labelItemStatus.Text = "圖片保存失敗。";
            }
        }

        private void OnSaveAnimationPngFile(AnimationItem aniItem, bool options)
        {
            var config = ImageHandlerConfig.Default;
            string aniName = this.cmbItemAniNames.SelectedItem as string;
            string pngFileName = pictureBoxEx1.PictureName
                + (string.IsNullOrEmpty(aniName) ? "" : ("." + aniName))
                + ".png";

            if (config.AutoSaveEnabled)
            {
                pngFileName = Path.Combine(config.AutoSavePictureFolder, string.Join("_", pngFileName.Replace("/", "").Replace("\\", ".").Split(Path.GetInvalidFileNameChars(), StringSplitOptions.None)));
            }
            else
            {
                var dlg = new SaveFileDialog();
                dlg.Filter = "PNG (*.png)|*.png|全部格式 (*.*)|*.*";
                dlg.FileName = pngFileName;
                if (dlg.ShowDialog() != DialogResult.OK)
                {
                    return;
                }
                pngFileName = dlg.FileName;
            }

            var clonedAniItem = (AnimationItem)aniItem.Clone();
            if (this.pictureBoxEx1.SaveAsPng(clonedAniItem, pngFileName, config, options))
            {
                labelItemStatus.Text = "已保存為: " + pngFileName;
            }
        }

        private void OnSaveGifFile(IEnumerable<AnimationItem> aniItem, IEnumerable<Tuple<int, int>> aniItemTime, bool options)
        {
            var config = ImageHandlerConfig.Default;
            using var encoder = AnimateEncoderFactory.CreateEncoder(config);
            var cap = encoder.Compatibility;

            string aniName = this.cmbItemAniNames.SelectedItem as string;
            string aniFileName = pictureBoxEx1.PictureName
                    + (string.IsNullOrEmpty(aniName) ? "" : ("." + aniName))
                    + cap.DefaultExtension;

            if (config.AutoSaveEnabled)
            {
                var fullFileName = Path.Combine(config.AutoSavePictureFolder, string.Join("_", aniFileName.Replace("/", "").Replace("\\", ".").Split(Path.GetInvalidFileNameChars(), StringSplitOptions.None)));
                int i = 1;
                while (File.Exists(fullFileName))
                {
                    fullFileName = Path.Combine(config.AutoSavePictureFolder, string.Format("{0}({1}){2}",
                        Path.GetFileNameWithoutExtension(aniFileName), i, Path.GetExtension(aniFileName)));
                    i++;
                }
                aniFileName = fullFileName;
            }
            else
            {
                var dlg = new SaveFileDialog();
                string extensionFilter = string.Join(";", cap.SupportedExtensions.Select(ext => $"*{ext}"));
                dlg.Filter = string.Format("{0} (*{1})|*{1}|全部格式 (*.*)|*.*", encoder.Name, extensionFilter);
                dlg.FileName = aniFileName;

                if (dlg.ShowDialog() != DialogResult.OK)
                {
                    return;
                }
                aniFileName = dlg.FileName;
            }

            var clonedAniItem = aniItem.Select(aniItem => (AnimationItem)aniItem.Clone());
            if (this.pictureBoxEx1.SaveAsGif(clonedAniItem, aniItemTime, aniFileName, config, encoder, options))
            {
                labelItemStatus.Text = "已保存為: " + aniFileName;
            }
        }
        #endregion

        #region File菜单的事件
        private void btnItemOpenWz_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Title = LocalizedString_JP.MAINFORM_OPENWZDLG_TITLE;
                dlg.Filter = LocalizedString_JP.MAINFORM_OPENWZDLG_FILTER;
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    Task.Run(() => openWz(dlg.FileName));
                }
            }
        }

        private async void openWz(string wzFilePath)
        {
            foreach (Wz_Structure wzs in openedWz)
            {
                foreach (Wz_File wz_f in wzs.wz_files)
                {
                    if (string.Compare(wz_f.Header.FileName, wzFilePath, true) == 0)
                    {
                        MessageBoxEx.Show(this, LocalizedString_JP.MAINFORM_OPENWZDLG_ALREADYOPENED, LocalizedString_JP.COMMON_ERROR);
                        return;
                    }
                }
            }

            Wz_Structure wz = new Wz_Structure();
            QueryPerformance.Start();
            labelItemStatus.Text = $"載入中: {wzFilePath}";
            advTree1.BeginUpdate();
            try
            {
                btnItemOpenWz.Enabled = false;
                btnItemOpenImg.Enabled = false;
                btnItemOpenUnityFolder.Enabled = false;
                buttonItemClose.Enabled = false;
                buttonItemCloseAll.Enabled = false;
                buttonItemSearchWz.Enabled = false;
                buttonItemSearchString.Enabled = false;
                galleryContainerRecent.Enabled = false;
                string[] msFileExtensions = { ".ms", ".mn" };
                if (msFileExtensions.Any(ext => string.Equals(Path.GetExtension(wzFilePath), ext, StringComparison.OrdinalIgnoreCase)))
                {
                    wz.LoadMsFile(wzFilePath);
                }
                else if (wz.IsKMST1125WzFormat(wzFilePath))
                {
                    wz.LoadKMST1125DataWz(wzFilePath);
                    if (string.Equals(Path.GetFileName(wzFilePath), "Base.wz", StringComparison.OrdinalIgnoreCase))
                    {
                        string packsDir = Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(wzFilePath)), "Packs");
                        if (Directory.Exists(packsDir))
                        {
                            foreach (var ext in msFileExtensions)
                            {
                                foreach (var msFile in Directory.GetFiles(packsDir, $"*{ext}"))
                                {
                                    wz.LoadMsFile(msFile);
                                }
                            }
                        }
                    }
                }
                else
                {
                    wz.Load(wzFilePath, true);
                }

                if (WcR2Config.Default.SortWzOnOpened)
                {
                    sortWzNode(wz.WzNode);
                }
                Node node = createNode(wz.WzNode);
                node.Expand();
                advTree1.Nodes.Add(node);
                this.openedWz.Add(wz);
                OnWzOpened(new WzStructureEventArgs(wz)); //触发事件
                if (!this.stringLinker.HasValues)
                {
                    this.LoadStringLinker();
                }
                QueryPerformance.End();
                labelItemStatus.Text = (this.stringLinker.HasValues ? "檔案已載入。時間經過：" : "檔案已載入，但字串表未能載入。時間經過：") + (Math.Round(QueryPerformance.GetLastInterval(), 4) * 1000) + "毫秒，" + wz.img_number + " IMG";

                ConfigManager.Reload();
                WcR2Config.Default.RecentDocuments.Remove(wzFilePath);
                WcR2Config.Default.RecentDocuments.Insert(0, wzFilePath);
                ConfigManager.Save();
                refreshRecentDocItems();
            }
            catch (FileNotFoundException)
            {
                MessageBoxEx.Show(this, LocalizedString_JP.MAINFORM_COMMON_FILENOTFOUND, LocalizedString_JP.COMMON_ERROR);
                WcR2Config.Default.RecentDocuments.Remove(wzFilePath);
            }
            catch (Exception ex)
            {
                MessageBoxEx.Show(this, ex.ToString(), LocalizedString_JP.COMMON_ERROR);
                wz.Clear();
            }
            finally
            {
                btnItemOpenWz.Enabled = true;
                btnItemOpenImg.Enabled = true;
                btnItemOpenUnityFolder.Enabled = true;
                buttonItemClose.Enabled = true;
                buttonItemCloseAll.Enabled = true;
                buttonItemSearchWz.Enabled = true;
                buttonItemSearchString.Enabled = true;
                galleryContainerRecent.Enabled = true;
                advTree1.EndUpdate();
            }
        }

        private void btnItemOpenImg_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Title = "選擇新楓之谷 IMG 檔案";
                dlg.Filter = "*.img;Data.wz (Minor修正檔)|*.img;Data.wz|*.wz|*.wz";
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    Task.Run(() => openImg(dlg.FileName));
                }
            }
        }

        private async void openImg(string imgFileName)
        {
            foreach (Wz_Structure wzs in openedWz)
            {
                foreach (Wz_File wz_f in wzs.wz_files)
                {
                    if (StringComparer.OrdinalIgnoreCase.Equals(wz_f.Header.FileName, imgFileName))
                    {
                        MessageBoxEx.Show("此 WZ 檔案已打開。", LocalizedString_JP.COMMON_ERROR);
                        return;
                    }
                }
            }

            Wz_Structure wz = new Wz_Structure();
            var sw = Stopwatch.StartNew();
            labelItemStatus.Text = $"載入中: {imgFileName}";
            advTree1.BeginUpdate();
            try
            {
                btnItemOpenWz.Enabled = false;
                btnItemOpenImg.Enabled = false;
                btnItemOpenUnityFolder.Enabled = false;
                buttonItemClose.Enabled = false;
                buttonItemCloseAll.Enabled = false;
                buttonItemSearchWz.Enabled = false;
                buttonItemSearchString.Enabled = false;
                galleryContainerRecent.Enabled = false;
                wz.LoadImg(imgFileName);

                Node node = createNode(wz.WzNode);
                node.Expand();
                advTree1.Nodes.Add(node);
                this.openedWz.Add(wz);
                OnWzOpened(new WzStructureEventArgs(wz)); //触发事件
                sw.Stop();
                labelItemStatus.Text = $"已打開 IMG 檔案。已用時間：{sw.ElapsedMilliseconds} 毫秒";
                refreshRecentDocItems();
            }
            catch (FileNotFoundException)
            {
                MessageBoxEx.Show(LocalizedString_JP.MAINFORM_COMMON_FILENOTFOUND, LocalizedString_JP.COMMON_ERROR);
            }
            catch (Exception ex)
            {
                MessageBoxEx.Show(ex.ToString(), LocalizedString_JP.COMMON_ERROR);
                wz.Clear();
            }
            finally
            {
                btnItemOpenWz.Enabled = true;
                btnItemOpenImg.Enabled = true;
                btnItemOpenUnityFolder.Enabled = true;
                buttonItemClose.Enabled = true;
                buttonItemCloseAll.Enabled = true;
                buttonItemSearchWz.Enabled = true;
                buttonItemSearchString.Enabled = true;
                galleryContainerRecent.Enabled = true;
                advTree1.EndUpdate();
            }
        }

        private void btnItemOpenUnityFolder_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dlg = new FolderBrowserDialog())
            {
                dlg.Description = "請選擇 MapleStory Classic World 的 Unity 資源資料夾（包含 *.bundle 的資料夾）。";
                dlg.ShowNewFolderButton = false;
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    Task.Run(() => openUnityFolder(dlg.SelectedPath));
                }
            }
        }

        private void openUnityFolder(string assetFolderPath)
        {
            if (this.openedUnitySources.Values.Any(document =>
                string.Equals(document.Source.Catalog.RootPath, assetFolderPath, StringComparison.OrdinalIgnoreCase)))
            {
                MessageBoxEx.Show(this, "此 Unity 資源資料夾已經開啟。", LocalizedString_JP.COMMON_ERROR);
                return;
            }

            labelItemStatus.Text = $"載入中: {assetFolderPath}";
            advTree1.BeginUpdate();
            ClassicWzSource unitySource = null;
            try
            {
                btnItemOpenWz.Enabled = false;
                btnItemOpenImg.Enabled = false;
                btnItemOpenUnityFolder.Enabled = false;
                buttonItemClose.Enabled = false;
                buttonItemCloseAll.Enabled = false;
                buttonItemSearchWz.Enabled = false;
                buttonItemSearchString.Enabled = false;
                galleryContainerRecent.Enabled = false;

                var sw = Stopwatch.StartNew();
                unitySource = ClassicWzSource.Open(assetFolderPath);
                int entryCount = unitySource.Catalog.Entries.Count;
                int bundleCount = unitySource.Catalog.Bundles.Count;

                // 多地域資料集：讓使用者選擇偏好的地域コード（字串表與圖集的語系）。
                // openUnityFolder 執行於背景執行緒，對話方塊必須回到 UI 執行緒顯示。
                if (unitySource.Locales.Count > 1)
                {
                    string selectedLocale = null;
                    bool cancelled = false;
                    this.Invoke(new Action(() =>
                    {
                        using (FrmSelectUnityLocale dialog = new FrmSelectUnityLocale(
                            unitySource.Locales,
                            this.lastUnityLocale ?? unitySource.Locale,
                            assetFolderPath,
                            styleManager1.ManagerStyle == eStyle.VisualStudio2012Dark))
                        {
                            if (dialog.ShowDialog(this) == DialogResult.OK)
                            {
                                selectedLocale = dialog.SelectedLocale;
                            }
                            else
                            {
                                cancelled = true;
                            }
                        }
                    }));
                    if (cancelled || selectedLocale == null)
                    {
                        unitySource.Dispose();
                        unitySource = null;
                        labelItemStatus.Text = "読み込みをキャンセルしました。";
                        return;
                    }
                    this.lastUnityLocale = selectedLocale;
                    unitySource.Locale = selectedLocale;
                }
                string loadedLocale = unitySource.Locale;

                // String/<region>/<category> 整理成 String.wz 的節點形狀；失敗不阻擋資料集開啟。
                ClassicStringLinkerTables stringTables = null;
                string stringTableError = null;
                try
                {
                    stringTables = ClassicStringLinkerTables.Build(unitySource);
                }
                catch (Exception ex)
                {
                    stringTableError = ex.Message;
                }

                // img 內容延後到選取時才解碼（UnityWzImage.AsWzNode），避免一次載入整個資料集。
                Wz_Node rootNode = ClassicWzBridge.CreateCatalogNode(unitySource, false, false);
                Node treeNode = createNode(rootNode);
                treeNode.Expand();
                advTree1.Nodes.Add(treeNode);
                this.openedUnitySources.Add(treeNode, new UnityAssetDocument(unitySource, rootNode)
                {
                    TreeNode = treeNode,
                    StringTables = stringTables,
                });
                unitySource = null; // 所有權轉移到 openedUnitySources

                if (!this.stringLinker.HasValues)
                {
                    this.LoadStringLinker();
                }

                sw.Stop();
                labelItemStatus.Text = $"已開啟 Unity 資源資料夾。時間經過：{sw.ElapsedMilliseconds}毫秒，"
                    + $"{entryCount} 個項目，{bundleCount} 個 bundle，區域: {loadedLocale}"
                    + (stringTableError == null ? null : "，字串表載入失敗: " + stringTableError);
            }
            catch (Exception ex)
            {
                MessageBoxEx.Show(this, ex.ToString(), LocalizedString_JP.COMMON_ERROR);
            }
            finally
            {
                if (unitySource != null)
                {
                    unitySource.Dispose();
                }
                btnItemOpenWz.Enabled = true;
                btnItemOpenImg.Enabled = true;
                btnItemOpenUnityFolder.Enabled = true;
                buttonItemClose.Enabled = true;
                buttonItemCloseAll.Enabled = true;
                buttonItemSearchWz.Enabled = true;
                buttonItemSearchString.Enabled = true;
                galleryContainerRecent.Enabled = true;
                advTree1.EndUpdate();
            }
        }

        private void buttonItemClose_Click(object sender, EventArgs e)
        {
            if (advTree1.SelectedNode == null)
            {
                MessageBoxEx.Show(LocalizedString_JP.MAINFORM_CLOSEWZDLG_FILENOTSELECTED, LocalizedString_JP.COMMON_ERROR);
                return;
            }
            Node baseWzNode = advTree1.SelectedNode;
            while (baseWzNode.Parent != null)
                baseWzNode = baseWzNode.Parent;
            if (this.openedUnitySources.TryGetValue(baseWzNode, out UnityAssetDocument unityDocument))
            {
                advTree1.Nodes.Remove(baseWzNode);
                advTree2.ClearAndDisposeAllNodes();
                advTree3.ClearAndDisposeAllNodes();
                listViewExWzDetail.Items.Clear();
                this.openedUnitySources.Remove(baseWzNode);
                unityDocument.Source.Dispose();
                labelItemStatus.Text = "已關閉";
                return;
            }
            if (baseWzNode.Text.ToLower() == "list.wz")
            {
                advTree1.Nodes.Remove(baseWzNode);
                labelItemStatus.Text = "List.wz已棄用。";
                return;
            }

            Wz_File wz_f = advTree1.SelectedNode.AsWzNode()?.GetNodeWzFile();
            if (wz_f == null)
            {
                MessageBoxEx.Show(LocalizedString_JP.MAINFORM_CLOSEWZDLG_FILENOTSELECTED, LocalizedString_JP.COMMON_ERROR);
                return;
            }
            Wz_Structure wz = wz_f.WzStructure;

            advTree1.Nodes.Remove(baseWzNode);

            listViewExWzDetail.Items.Clear();

            Wz_Image image = null;
            if (advTree2.Nodes.Count > 0
                && (image = advTree2.Nodes[0].AsWzNode()?.GetValue<Wz_Image>()) != null
                && image.WzFile.WzStructure == wz)
            {
                advTree2.Nodes.Clear();
            }

            if (advTree3.Nodes.Count > 0
                && (image = advTree3.Nodes[0].AsWzNode()?.GetNodeWzImage()) != null
                && image.WzFile.WzStructure == wz)
            {
                advTree3.Nodes.Clear();
            }

            OnWzClosing(new WzStructureEventArgs(wz));
            wz.Clear();
            CharaSimLoader.ClearAll();
            if (this.openedWz.Remove(wz))
                labelItemStatus.Text = "關閉";
            else
                labelItemStatus.Text = "無法關閉 WZ 檔案：未知錯誤";
        }

        private void buttonItemCloseAll_Click(object sender, EventArgs e)
        {
            advTree1.ClearAndDisposeAllNodes();
            advTree1.ClearLayoutCellInfo();
            advTree2.ClearAndDisposeAllNodes();
            advTree2.ClearLayoutCellInfo();
            advTree3.ClearAndDisposeAllNodes();
            advTree3.ClearLayoutCellInfo();
            foreach (Wz_Structure wz in openedWz)
            {
                OnWzClosing(new WzStructureEventArgs(wz));
                wz.Clear();
            }
            openedWz.Clear();
            foreach (UnityAssetDocument unityDocument in this.openedUnitySources.Values)
            {
                unityDocument.Source.Dispose();
            }
            this.openedUnitySources.Clear();
            CharaSimLoader.ClearAll();
            stringLinker.Clear();
            labelItemStatus.Text = "所有檔案已關閉";
            GC.Collect();
        }

        private void refreshRecentDocItems()
        {
            List<BaseItem> items = new List<BaseItem>();
            foreach (BaseItem item in galleryContainerRecent.SubItems)
            {
                if (item is ButtonItem)
                {
                    items.Add(item);
                }
            }
            galleryContainerRecent.SubItems.RemoveRange(items.ToArray());
            items.Clear();

            foreach (var doc in WcR2Config.Default.RecentDocuments)
            {
                ButtonItem item = new ButtonItem() { Text = "&" + (items.Count + 1) + ". " + Path.GetFileName(doc), Tooltip = doc, Tag = doc };
                item.Click += new EventHandler(buttonItemRecentDocument_Click);
                items.Add(item);
            }
            galleryContainerRecent.SubItems.AddRange(items.ToArray());
        }

        void buttonItemRecentDocument_Click(object sender, EventArgs e)
        {
            ButtonItem btnItem = sender as ButtonItem;
            string path;
            if (btnItem == null || (path = btnItem.Tag as string) == null)
                return;
            Task.Run(() => openWz(path));
        }
        #endregion

        #region wzView和提取的事件和方法
        private void advTree1_DragEnter(object sender, DragEventArgs e)
        {
            string[] types = e.Data.GetFormats();
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                foreach (string file in files)
                {
                    if (Path.GetExtension(file) != ".wz")
                    {
                        e.Effect = DragDropEffects.None;
                        return;
                    }
                }
                e.Effect = DragDropEffects.Move;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void advTree1_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                foreach (string file in files)
                {
                    Task.Run(() => openWz(file));
                }
            }
        }

        private void advTree1_AfterNodeSelect(object sender, AdvTreeNodeEventArgs e)
        {
            Wz_Node selectedNode = e.Node.AsWzNode();

            if (selectedNode == null)
            {
                return;
            }

            if (selectedNode.FullPathToFile.Contains("Language"))
            {
                this.advTree1.ContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.toolStripMenuItem5,
                this.tsmi1UpdateStringLinker});
            }
            else if (this.advTree1.ContextMenuStrip.Items.Contains(this.tsmi1UpdateStringLinker))
            {
                for (int i = 0; i < 2; i++)
                {
                    this.advTree1.ContextMenuStrip.Items.RemoveAt(this.advTree1.ContextMenuStrip.Items.Count - 1);
                }
            }

            if (selectedNode.FullPathToFile.Contains("Sound"))
            {
                this.advTree1.ContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.toolStripMenuItem5,
                this.tsmi1ExportSound});
            }
            else if (this.advTree1.ContextMenuStrip.Items.Contains(this.tsmi1ExportSound))
            {
                for (int i = 0; i < 2; i++)
                {
                    this.advTree1.ContextMenuStrip.Items.RemoveAt(this.advTree1.ContextMenuStrip.Items.Count - 1);
                }
            }

            listViewExWzDetail.BeginUpdate();
            listViewExWzDetail.Items.Clear();

            if (selectedNode.Value == null)
            {
                listViewExWzDetail.Items.Add(new ListViewItem(new string[] { "目錄名", Path.GetFileName(e.Node.Text) }));
                autoResizeColumns(listViewExWzDetail);
            }
            else if (selectedNode.Value is Wz_File wzFile)
            {
                listViewExWzDetail.Items.Add(new ListViewItem(new string[] { "檔案名", wzFile.Header.FileName }));
                listViewExWzDetail.Items.Add(new ListViewItem(new string[] { "檔案大小", wzFile.Header.FileSize + " 位元組" }));
                listViewExWzDetail.Items.Add(new ListViewItem(new string[] { "版權", wzFile.Header.Copyright }));
                listViewExWzDetail.Items.Add(new ListViewItem(new string[] { "版本", wzFile.GetMergedVersion().ToString() }));
                listViewExWzDetail.Items.Add(new ListViewItem(new string[] { "WZ類型", wzFile.IsSubDir ? "Subdirectory" : wzFile.Type.ToString() }));

                foreach (Wz_File subFile in wzFile.MergedWzFiles)
                {
                    listViewExWzDetail.Items.Add(" ");
                    listViewExWzDetail.Items.Add(new ListViewItem(new string[] { "檔案名", subFile.Header.FileName }));
                    listViewExWzDetail.Items.Add(new ListViewItem(new string[] { "檔案大小", subFile.Header.FileSize + " 位元組" }));
                    listViewExWzDetail.Items.Add(new ListViewItem(new string[] { "版權", subFile.Header.Copyright }));
                    listViewExWzDetail.Items.Add(new ListViewItem(new string[] { "版本", subFile.Header.WzVersion.ToString() }));
                }

                autoResizeColumns(listViewExWzDetail);
            }
            else if (selectedNode.Value is Wz_Image wzImage)
            {
                listViewExWzDetail.Items.Add(new ListViewItem(new string[] { "IMG名", wzImage.Name }));
                listViewExWzDetail.Items.Add(new ListViewItem(new string[] { "IMG大小", wzImage.Size + " 位元組" }));
                listViewExWzDetail.Items.Add(new ListViewItem(new string[] { "IMG偏移量", wzImage.Offset + " 位元組" }));
                listViewExWzDetail.Items.Add(new ListViewItem(new string[] { "路徑", wzImage.Node.FullPathToFile }));
                listViewExWzDetail.Items.Add(new ListViewItem(new string[] { "校驗和", wzImage.Checksum.ToString() }));
                autoResizeColumns(listViewExWzDetail);

                advTree2.ClearAndDisposeAllNodes();
                //advTree2.Nodes.Clear();

                QueryPerformance.Start();
                try
                {
                    Exception ex;
                    if (wzImage.TryExtract(out ex))
                    {
                        advTree2.Nodes.Add(createNode(wzImage.Node));
                        advTree2.Nodes[0].Expand();
                        QueryPerformance.End();
                        double ms = (Math.Round(QueryPerformance.GetLastInterval(), 4) * 1000);

                        labelItemStatus.Text = "已匯入。已用時間：" + ms + "毫秒";
                    }
                    else
                    {

                        labelItemStatus.Text = "匯入失敗：" + ex.Message;
                    }
                }
                catch (Exception ex)
                {
                    labelItemStatus.Text = "匯入失敗：" + ex.Message;
                }
            }
            else if (selectedNode.Value is UnityWzImage unityImage)
            {
                listViewExWzDetail.Items.Add(new ListViewItem(new string[] { "IMG名", unityImage.Name }));
                listViewExWzDetail.Items.Add(new ListViewItem(new string[] {
                    "WZタイプ", ClassicWzTypeMap.GetWzType(unityImage.Name).ToString() }));
                autoResizeColumns(listViewExWzDetail);

                advTree2.ClearAndDisposeAllNodes();

                QueryPerformance.Start();
                try
                {
                    // WZJS 樹在第一次選取時才解碼，UnityWzImage 會保留結果。
                    Node imageNode = createNode(unityImage.AsWzNode());
                    advTree2.Nodes.Add(imageNode);
                    imageNode.Expand();
                    QueryPerformance.End();
                    double ms = (Math.Round(QueryPerformance.GetLastInterval(), 4) * 1000);

                    labelItemStatus.Text = "已匯入。已用時間：" + ms + "毫秒";
                }
                catch (Exception ex)
                {
                    labelItemStatus.Text = "匯入失敗：" + ex.Message;
                }
            }
            else if (selectedNode.Value is UnityJsonAsset jsonAsset)
            {
                listViewExWzDetail.Items.Add(new ListViewItem(new string[] { "JSON名", jsonAsset.Name }));
                listViewExWzDetail.Items.Add(new ListViewItem(new string[] {
                    "WZタイプ", ClassicWzTypeMap.GetWzType(jsonAsset.ImageKey).ToString() }));
                autoResizeColumns(listViewExWzDetail);

                advTree2.ClearAndDisposeAllNodes();

                QueryPerformance.Start();
                try
                {
                    // JSON 於第一次選取時才解析，UnityJsonAsset 會保留結果。
                    Node jsonNode = createNode(jsonAsset.AsWzNode());
                    advTree2.Nodes.Add(jsonNode);
                    jsonNode.Expand();
                    QueryPerformance.End();
                    double ms = (Math.Round(QueryPerformance.GetLastInterval(), 4) * 1000);

                    labelItemStatus.Text = "已匯入。已用時間：" + ms + "毫秒";
                }
                catch (Exception ex)
                {
                    labelItemStatus.Text = "匯入失敗：" + ex.Message;
                }
            }
            listViewExWzDetail.EndUpdate();
        }

        private void autoResizeColumns(ListViewEx listView)
        {
            listView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            foreach (System.Windows.Forms.ColumnHeader column in listView.Columns)
            {
                column.Width += (int)(listView.Font.Size * 2);
            }
        }

        private void advTree2_NodeDoubleClick(object sender, TreeNodeMouseEventArgs e)
        {
            if (e.Node == null || e.Button != MouseButtons.Left)
                return;
            historyNodeList.Clear();
            advTree3.Nodes.Clear();

            var selectedNode = e.Node.AsWzNode();
            if (selectedNode != null)
            {
                advTree3.BeginUpdate();
                try
                {
                    var node = createNodeDetail(e.Node);
                    node.ExpandAll();
                    advTree3.Nodes.Add(node);
                    advTree3.SelectedNode = node;
                }
                finally
                {
                    advTree3.EndUpdate();
                }
            }
        }

        private Node createNodeDetail(Node parentNode)
        {
            Node newNode = new Node(parentNode.Text);
            newNode.Tag = parentNode.Tag;
            Wz_Node wzNode = newNode.AsWzNode();
            if (wzNode != null)
            {
                newNode.Cells.Add(new Cell(wzNode.Value == null ? "<" + parentNode.Nodes.Count + ">" : getValueString(wzNode.Value)));
                newNode.Cells.Add(new Cell(wzNode.Value == null ? null : wzNode.Value.GetType().Name));
                newNode.ImageKey = wzNode.Value == null ? "dir" : (getValueImageKey(wzNode.Value) ?? "num");
            }
            foreach (Node subNode in parentNode.Nodes)
            {
                newNode.Nodes.Add(createNodeDetail(subNode));
            }
            return newNode;
        }

        private string getValueString(object value)
        {
            switch (value)
            {
                case Wz_Png png:
                    return $"png {png.Width}*{png.Height} ({(int)png.Format}{(png.Scale > 0 ? $", {png.Scale}" : null)})";

                case byte[] pngData:
                    return $"png {pngData.Length} 位元組";

                case UnityCanvas unityCanvas:
                    return "canvas " + unityCanvas.Path;

                case UnityJsonAsset jsonAsset:
                    return "json " + jsonAsset.ImageKey;

                case UnityWzImage unityImage:
                    return $"unity img {unityImage.Name}";

                case Wz_Vector vector:
                    return $"({vector.X}, {vector.Y})";

                case Wz_Uol uol:
                    return uol.Uol;

                case Wz_Sound sound:
                    return $"sound {sound.Ms}ms";

                case Wz_Image img:
                    return $"<{img.Node.Nodes.Count}>";

                case Wz_RawData rawData:
                    return $"rawdata {rawData.Length}";

                case Wz_Convex convex:
                    return $"convex [{convex.Points.Length}]";

                case Wz_Video video:
                    return $"video {video.Length}";

                default:
                    string cellVal = Convert.ToString(value);
                    if (cellVal != null && cellVal.Length > 50)
                    {
                        cellVal = cellVal.Substring(0, 50);
                    }
                    return cellVal;
            }
        }

        private string getValueImageKey(object value)
        {
            return value switch
            {
                string => "str",
                short or int or long or float or double=> "num",
                Wz_Png => "png",
                byte[] => "png",
                UnityCanvas => "png",
                Wz_Vector => "vector",
                Wz_Uol => "uol",
                Wz_Sound sound => sound.SoundType == Wz_SoundType.Binary ? "rawdata" : "mp3",
                Wz_Image => "img",
                Wz_RawData => "rawdata",
                Wz_Convex => "convex",
                Wz_Video => "video",
                _ => null
            };
        }

        private void advTree3_AfterNodeSelect(object sender, AdvTreeNodeEventArgs e)
        {
            if (e.Node == null)
                return;

            if (!historySelecting && (historyNodeList.Count == 0 || e.Node != historyNodeList.Current))
            {
                historyNodeList.Add(e.Node);
            }
            else
            {
                historySelecting = false;
            }

            Wz_Node selectedNode = e.Node.AsWzNode();
            if (selectedNode == null)
                return;

            switch (selectedNode.Value)
            {
                case Wz_Png png:
                    pictureBoxEx1.PictureName = GetSelectedNodeImageName();
                    pictureBoxEx1.ShowImage(png);
                    this.cmbItemAniNames.Items.Clear();
                    if (this.pictureBoxEx1.IsPaused)
                    {
                        ResumePictureBox();
                    }
                    if (png.ActualPages > 1)
                    {
                        for (int i = 0; i < png.ActualPages; i++)
                            this.cmbItemAniNames.Items.Add(i);
                    }

                    advTree3.PathSeparator = ".";
                    textBoxX1.Text = "數據長:  " + png.DataLength + " 位元組\r\n" +
                        "偏移量:  " + png.Offset + "\r\n" +
                        "大小:  " + png.Width + "*" + png.Height + "\r\n" +
                        "PNG形式:  " + png.Format + "(" + (int)png.Format + ")\r\n" +
                        "尺度: " + png.Scale + "(x" + png.ActualScale + ")\r\n" +
                        "頁: " + png.Pages + "(" + png.ActualPages + ")\r\n" +
                        "不明1: " + png.Unknown1;

                    var sourceNode = selectedNode.GetLinkedSourceNode(PluginManager.FindWz);
                    if (sourceNode != selectedNode)
                    {
                        png = sourceNode.GetValueEx<Wz_Png>(null);
                        if (png != null)
                        {
                            string linkStr = Convert.ToString((selectedNode.Nodes["source"] ?? selectedNode.Nodes["_inlink"] ?? selectedNode.Nodes["_outlink"])?.Value);
                            if (linkStr != null && linkStr.Contains("\n") && !linkStr.Contains("\r\n"))
                            {
                                linkStr = linkStr.Replace("\n", "\r\n");
                            }
                            textBoxX1.AppendText("\r\n\r\n" + Convert.ToString(linkStr));

                            pictureBoxEx1.PictureName = GetSelectedNodeImageName();
                            pictureBoxEx1.ShowImage(png);
                            this.cmbItemAniNames.Items.Clear();
                            advTree3.PathSeparator = ".";
                            textBoxX1.AppendText("\r\n\r\n數據長:  " + png.DataLength + " 位元組\r\n" +
                                "偏移量:  " + png.Offset + "\r\n" +
                                "大小:  " + png.Width + "*" + png.Height + "\r\n" +
                                "PNG形式:  " + png.Format + "(" + (int)png.Format + ")\r\n" +
                                "尺度: " + png.Scale + "(x" + png.ActualScale + ")\r\n" +
                                "頁: " + png.Pages + "(" + png.ActualPages + ")\r\n" +
                                "不明1: " + png.Unknown1);
                        }
                    }
                    break;

                case UnityCanvas unityCanvas:
                    try
                    {
                        byte[] canvasPng = unityCanvas.GetPng();
                        if (canvasPng != null)
                        {
                            pictureBoxEx1.PictureName = GetSelectedNodeImageName();
                            pictureBoxEx1.ShowImage(canvasPng);
                            this.cmbItemAniNames.Items.Clear();
                            if (this.pictureBoxEx1.IsPaused)
                            {
                                ResumePictureBox();
                            }
                            advTree3.PathSeparator = ".";
                            textBoxX1.Text = "數據長:  " + canvasPng.Length + " 位元組\r\n" +
                                "路徑:  " + unityCanvas.Path;
                        }
                        else
                        {
                            textBoxX1.Text = "找不到圖片。";
                        }
                    }
                    catch (Exception ex)
                    {
                        textBoxX1.Text = "無法載入圖片：" + ex.Message;
                    }
                    break;

                case byte[] pngData:
                    try
                    {
                        pictureBoxEx1.PictureName = GetSelectedNodeImageName();
                        pictureBoxEx1.ShowImage(pngData);
                        this.cmbItemAniNames.Items.Clear();
                        if (this.pictureBoxEx1.IsPaused)
                        {
                            ResumePictureBox();
                        }
                        advTree3.PathSeparator = ".";
                        textBoxX1.Text = "數據長:  " + pngData.Length + " 位元組";
                    }
                    catch (Exception)
                    {
                        textBoxX1.Text = "無法載入圖片。";
                    }
                    break;

                case Wz_Vector vector:
                    textBoxX1.Text = "x: " + vector.X + " px\r\n" +
                        "y: " + vector.Y + " px";
                    break;

                case Wz_Convex convex:
                    var sb = new StringBuilder();
                    for (int i = 0; i < convex.Points.Length; i++)
                    {
                        if (i > 0) sb.AppendLine();
                        sb.AppendFormat("({0}, {1})", convex.Points[i].X, convex.Points[i].Y);
                    }
                    textBoxX1.Text = sb.ToString();
                    break;

                case Wz_Uol uol:
                    textBoxX1.Text = "uolPath: " + uol.Uol;
                    break;

                case Wz_Sound sound:
                    preLoadSound(sound, selectedNode.Text);
                    textBoxX1.Text = "數據長:  " + sound.DataLength + " 位元組\r\n" +
                        "偏移量:  " + sound.Offset + "\r\n" +
                        "時長: " + sound.Ms + " ms\r\n" +
                        "頻道: " + sound.Channels + "\r\n" +
                        "頻率: " + sound.Frequency + " Hz\r\n" +
                        "類型: " + sound.SoundType.ToString();
                    break;

                case Wz_Image:
                    //do nothing;
                    break;

                case Wz_RawData rawData:
                    textBoxX1.Text = "數據長:  " + rawData.Length + " 位元組\r\n" +
                        "偏移量:  " + rawData.Offset;
                    break;

                case Wz_Video video:
                    textBoxX1.Text = "數據長:  " + video.Length + " 位元組\r\n" +
                        "偏移量:  " + video.Offset;
                    if (this.pictureBoxEx1.ShowOverlayAni) break; // 애니메이션 중첩 중일때는 자동 video 미리보기 없음
                    var origin = selectedNode.FindNodeByPath("origin").GetValueEx<Wz_Vector>(null);
                    var videoFrameData = this.pictureBoxEx1.LoadVideo(video, origin);
                    pictureBoxEx1.PictureName = GetSelectedNodeImageName();
                    this.pictureBoxEx1.ShowAnimation(videoFrameData);
                    this.cmbItemAniNames.Items.Clear();
                    if (this.pictureBoxEx1.IsPaused)
                    {
                        ResumePictureBox();
                    }
                    break;


                default:
                    string valueStr = Convert.ToString(selectedNode.Value);
                    if (valueStr != null && valueStr.Contains("\n") && !valueStr.Contains("\r\n"))
                    {
                        valueStr = valueStr.Replace("\n", "\r\n");
                    }
                    textBoxX1.Text = Convert.ToString(valueStr);

                    switch (selectedNode.Text)
                    {
                        case "source":
                        case "_inlink":
                        case "_outlink":
                            {
                                var parentNode = selectedNode.ParentNode;
                                if (parentNode != null && parentNode.Value is Wz_Png)
                                {
                                    var linkNode = parentNode.GetLinkedSourceNode(PluginManager.FindWz);
                                    var png = linkNode.GetValueEx<Wz_Png>(null);

                                    if (png != null)
                                    {
                                        pictureBoxEx1.PictureName = GetSelectedNodeImageName();
                                        pictureBoxEx1.ShowImage(png);
                                        this.cmbItemAniNames.Items.Clear();
                                        advTree3.PathSeparator = ".";
                                        textBoxX1.AppendText("\r\n\r\n數據長:  " + png.DataLength + " 位元組\r\n" +
                                        "偏移量:  " + png.Offset + "\r\n" +
                                        "大小:  " + png.Width + "*" + png.Height + "\r\n" +
                                            "PNG形式:  " + png.Format + "(" + (int)png.Format + ")\r\n" +
                                            "尺度: " + png.Scale + "(x" + png.ActualScale + ")\r\n" +
                                            "頁: " + png.Pages + "(" + png.ActualPages + ")");
                                    }
                                }
                            }
                            break;
                    }
                    break;
            }
        }

        private void pictureBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            /*
            if (pictureBox1.Image != null && e.Button == MouseButtons.Left)
            {
                string tempFile = Path.Combine(Path.GetTempPath(), Convert.ToString(pictureBox1.Tag));
                switch (Path.GetExtension(tempFile))
                {
                    case ".png":
                        pictureBox1.Image.Save(tempFile, System.Drawing.Imaging.ImageFormat.Png);
                        System.Diagnostics.Process.Start(tempFile);
                        break;
                    case ".gif":
                        pictureBox1.Image.Save(tempFile, System.Drawing.Imaging.ImageFormat.Gif);
                        System.Diagnostics.Process.Start(tempFile);
                        break;
                    default:
                        MessageBoxEx.Show("不识别的檔案名：" + tempFile, "喵~");
                        break;
                }
            }*/
        }

        private void listViewExString_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                this.listViewExStringFind();
            }
        }

        private void listViewExString_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                this.listViewExStringFind();
            }
            else if (e.KeyCode == Keys.C && e.Control)
            {
                this.listViewExStringCopy();
            }
        }

        private void listViewExStringFind()
        {
            if (listViewExString.SelectedItems.Count == 0 || advTree1.Nodes.Count == 0)
            {
                return;
            }
            string id = listViewExString.SelectedItems[0].Text;
            string nodePath = listViewExString.SelectedItems[0].SubItems[3].Text;
            List<string[]> objPathList = detectObjPathByStringPath(id, nodePath);

            //分离wz路径和img路径
            foreach (string[] fullPath in objPathList)
            {
                //寻找所有可能的wzfile
                List<Wz_Node> allWzFile = new List<Wz_Node>();
                Wz_Type wzType = ParseType(fullPath[0]);
                foreach (var wzs in this.openedWz)
                {
                    foreach (var wzf in wzs.wz_files)
                    {
                        if (wzf.Type == wzType && wzf.OwnerWzFile == null)
                        {
                            allWzFile.Add(wzf.Node);
                        }
                    }
                }

                //开始搜索
                foreach (var wzFileNode in allWzFile)
                {
                    Wz_Node node = SearchNode(wzFileNode, fullPath, 1);
                    if (node != null)
                    {
                        OnSelectedWzNode(node); //遇到第一个 选中 返回
                        return;
                    }
                }
            }

            //失败
            if (this.TryFindUnityStringNode(id, nodePath))
            {
                return;
            }

            string path;
            if (objPathList.Count == 1)
            {
                path = string.Join("\\", objPathList[0]);
            }
            else
            {
                path = "(" + objPathList.Count + ") 節點";
            }
            labelItemStatus.Text = "未找到該IMG節點: " + path;
        }

        private Wz_Node SearchNode(Wz_Node parent, string[] path, int startIndex)
        {
            if (startIndex >= path.Length)
            {
                return null;
            }
            if (parent.Value is Wz_Image)
            {
                Wz_Image img = parent.GetValue<Wz_Image>();
                if (!img.TryExtract())
                {
                    return null;
                }
                parent = img.Node;
            }
            string nodeName = path[startIndex];
            if (!string.IsNullOrEmpty(nodeName))
            {
                Wz_Node child = parent.FindNodeByPath(false, true, nodeName);
                if (child != null)
                {
                    return (startIndex == path.Length - 1) ? child : SearchNode(child, path, startIndex + 1);
                }
            }
            else //遍历全部
            {
                foreach (Wz_Node child in parent.Nodes)
                {
                    if (child.Nodes.Count == 0) //只过滤檔案夹 未来有需求再改
                    {
                        continue;
                    }
                    Wz_Node find = SearchNode(child, path, startIndex + 1);
                    if (find != null)
                    {
                        return (startIndex == path.Length - 1) ? null : find;
                    }

                }
            }

            return null;
        }

        /// <summary>字串結果所屬的分類；Unity 資產的節點名稱沒有 <c>.img</c> 後綴。</summary>
        private enum UnityStringCategory
        {
            None,
            Item,
            Equip,
            Mob,
            Npc,
            Skill,
            Map,
            Quest,
        }

        /// <summary>
        /// 字串搜尋結果在 Unity 資料集內找對應節點：先選取 img（advTree1），
        /// 再選取 img 內的節點（advTree2）。
        /// </summary>
        private bool TryFindUnityStringNode(string id, string stringNodePath)
        {
            if (this.openedUnitySources == null || this.openedUnitySources.Count == 0)
            {
                return false;
            }

            int entryId;
            if (!int.TryParse(id, out entryId))
            {
                return false;
            }

            UnityStringCategory category = GetUnityStringCategory(stringNodePath);
            string rootDirectory = GetUnityStringRootDirectory(category);
            if (rootDirectory == null)
            {
                return false;
            }

            foreach (UnityAssetDocument document in this.openedUnitySources.Values)
            {
                ClassicCatalog catalog = document.Source.Catalog;
                foreach (string candidate in GetUnityStringPathCandidates(catalog, category, entryId))
                {
                    if (this.SelectUnityNode(document, candidate))
                    {
                        return true;
                    }
                }

                // 保底：WZ 的分類目錄／補零方式可能與資產樹不同，改以 id 數值掃描分類目錄。
                string imageKey = catalog.FindImageKeyByNumericName(rootDirectory, entryId);
                if (imageKey != null && this.SelectUnityNode(document, imageKey))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 依字串節點路徑（如 <c>Eqp.img\100\0\01002000</c>）判斷分類；
        /// Unity 的字串表節點名稱帶 <c>.img</c>，資產樹則不帶，兩種寫法都接受。
        /// </summary>
        private static UnityStringCategory GetUnityStringCategory(string stringNodePath)
        {
            if (string.IsNullOrEmpty(stringNodePath))
            {
                return UnityStringCategory.None;
            }
            int arrow = stringNodePath.IndexOf(" -> ", StringComparison.Ordinal);
            if (arrow >= 0)
            {
                stringNodePath = stringNodePath.Substring(0, arrow);
            }
            foreach (string segment in stringNodePath.Split('\\'))
            {
                string name = segment;
                if (name.EndsWith(".img", StringComparison.OrdinalIgnoreCase))
                {
                    name = name.Substring(0, name.Length - 4);
                }
                switch (name.ToLowerInvariant())
                {
                    case "eqp":
                        return UnityStringCategory.Equip;
                    case "item":
                    case "consume":
                    case "install":
                    case "ins":
                    case "etc":
                    case "pet":
                    case "cash":
                        return UnityStringCategory.Item;
                    case "mob":
                        return UnityStringCategory.Mob;
                    case "npc":
                        return UnityStringCategory.Npc;
                    case "skill":
                        return UnityStringCategory.Skill;
                    case "map":
                        return UnityStringCategory.Map;
                    case "quest":
                    case "questdata":
                    case "questinfo":
                        return UnityStringCategory.Quest;
                }
            }
            return UnityStringCategory.None;
        }

        private static string GetUnityStringRootDirectory(UnityStringCategory category)
        {
            switch (category)
            {
                case UnityStringCategory.Item:
                    return "Item";
                case UnityStringCategory.Equip:
                    return "Character";
                case UnityStringCategory.Mob:
                    return "Mob";
                case UnityStringCategory.Npc:
                    return "Npc";
                case UnityStringCategory.Skill:
                    return "Skill";
                case UnityStringCategory.Map:
                    return "Map";
                case UnityStringCategory.Quest:
                    return "Quest";
                default:
                    return null;
            }
        }

        private static List<string> GetUnityStringPathCandidates(
            ClassicCatalog catalog, UnityStringCategory category, int id)
        {
            switch (category)
            {
                case UnityStringCategory.Item:
                    return catalog.GetItemPathCandidates(id);
                case UnityStringCategory.Equip:
                    return catalog.GetEquipPathCandidates(id);
                case UnityStringCategory.Mob:
                    return catalog.GetMobPathCandidates(id);
                case UnityStringCategory.Npc:
                    return catalog.GetNpcPathCandidates(id);
                case UnityStringCategory.Skill:
                    return catalog.GetSkillPathCandidates(id);
                case UnityStringCategory.Map:
                    return catalog.GetMapPathCandidates(id);
                case UnityStringCategory.Quest:
                    return catalog.GetQuestPathCandidates(id);
                default:
                    return new List<string>();
            }
        }

        /// <summary>選取 Unity 資料集內的路徑：先在 advTree1 選 img，再於 advTree2 選其中的節點。</summary>
        private bool SelectUnityNode(UnityAssetDocument document, string nodePath)
        {
            string imageKey, innerPath;
            if (!document.Source.Catalog.TrySplitImagePrefix(nodePath, out imageKey, out innerPath))
            {
                return false;
            }

            Node imageTreeNode = findUnityTreeNode(document.TreeNode, imageKey.Split('/'));
            if (imageTreeNode == null)
            {
                return false;
            }

            advTree1.SelectedNode = imageTreeNode;
            imageTreeNode.EnsureVisible();
            if (advTree2.Nodes.Count == 0)
            {
                return false;
            }

            Node targetNode = string.IsNullOrEmpty(innerPath)
                ? advTree2.Nodes[0]
                : findUnityTreeNode(advTree2.Nodes[0], innerPath.Split('/'));
            if (targetNode == null)
            {
                return false;
            }

            targetNode.EnsureVisible();
            advTree2.SelectedNode = targetNode;
            return true;
        }

        private static Node findUnityTreeNode(Node parent, string[] segments)
        {
            Node node = parent;
            foreach (string segment in segments)
            {
                if (segment.Length == 0)
                {
                    continue;
                }
                Node next = null;
                long segmentNumber;
                bool segmentIsNumber = long.TryParse(segment, out segmentNumber);
                foreach (Node child in node.Nodes)
                {
                    if (string.Equals(child.Text, segment, StringComparison.OrdinalIgnoreCase))
                    {
                        next = child;
                        break;
                    }
                    // id 節點的補零位數可能不同（02000000 / 2000000）。
                    long childNumber;
                    if (segmentIsNumber && long.TryParse(child.Text, out childNumber)
                        && childNumber == segmentNumber)
                    {
                        next = child;
                    }
                }
                if (next == null)
                {
                    return null;
                }
                node = next;
            }
            return node;
        }

        private bool OnSelectedWzNode(Wz_Node wzNode)
        {
            Wz_File wzFile = wzNode.GetNodeWzFile();
            string[] path = wzNode.FullPathToFile.Split('\\');
            if (wzFile == null)
            {
                return false;
            }

            Node treeNode = findWzFileTreeNode(wzFile);
            if (treeNode == null)
            {
                return false;
            }

            for (int i = 1; i < path.Length; i++)
            {
                Node find = null;
                foreach (Node child in treeNode.Nodes)
                {
                    if (child.Text == path[i])
                    {
                        find = child;
                        break;
                    }
                }
                if (find == null)
                {
                    return false;
                }

                if (find.AsWzNode()?.Value is Wz_Image)
                {
                    advTree1.SelectedNode = find;
                    if (advTree2.Nodes.Count > 0)
                    {
                        treeNode = advTree2.Nodes[0];
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    treeNode = find;
                }
            }

            advTree2.SelectedNode = treeNode;
            return true;
        }

        private void listViewExStringCopy()
        {
            if (listViewExString.SelectedItems.Count == 0 || advTree1.Nodes.Count == 0)
            {
                return;
            }

            StringBuilder sb = new StringBuilder();
            foreach (ListViewItem.ListViewSubItem item in listViewExString.SelectedItems[0].SubItems)
            {
                sb.Append(item.Text).Append(" ");
            }
            sb.Remove(sb.Length - 1, 1);
            Clipboard.SetText(sb.ToString(), TextDataFormat.UnicodeText);
            labelItemStatus.Text = "已複製到剪貼簿。";
        }

        private List<string[]> detectObjPathByStringPath(string id, string stringNodePath)
        {
            List<string[]> pathList = new List<string[]>();

            List<string> wzPath = new List<string>();
            List<string> imagePath = new List<string>();

            Action addPath = () =>
            {
                List<string> fullPath = new List<string>(wzPath.Count + imagePath.Count);
                fullPath.AddRange(wzPath);
                fullPath.AddRange(imagePath);
                pathList.Add(fullPath.ToArray());
            };

            string[] pathArray = stringNodePath.Split('\\');
            switch (pathArray[0])
            {
                case "Cash.img":
                case "Consume.img":
                case "Etc.img":
                case "Pet.img":
                    wzPath.Add("Item");
                    wzPath.Add(pathArray[0].Substring(0, pathArray[0].IndexOf(".img")));
                    if (pathArray[0] == "Pet.img")
                    {
                        wzPath.Add(id.TrimStart('0') + ".img");
                    }
                    else
                    {
                        id = id.PadLeft(8, '0');
                        wzPath.Add(id.Substring(0, 4) + ".img");
                        imagePath.Add(id);
                    }
                    addPath();
                    break;

                case "Ins.img": //KMST1066
                    wzPath.Add("Item");
                    wzPath.Add("Install");
                    wzPath.Add("");
                    id = id.PadLeft(8, '0');
                    imagePath.Add(id);
                    for (int len = 4; len <= 6; len++)
                    {
                        wzPath[2] = id.Substring(0, len) + ".img";
                        addPath();
                    }
                    break;

                case "Eqp.img":
                    wzPath.Add("Character");
                    if (pathArray[2] == "Taming")
                    {
                        wzPath.Add("TamingMob");
                    }
                    else if (pathArray[2] != "Skin")
                    {
                        wzPath.Add(pathArray[2]);
                    }
                    wzPath.Add(id.PadLeft(8, '0') + ".img");
                    addPath();
                    //往往这个不靠谱。。 加一个任意门备用
                    wzPath[1] = "";
                    addPath();
                    break;

                case "GuildCastle.img":
                    wzPath.AddRange(new string[] { "Etc", "GuildCastle.img", "ResearchList", pathArray[2], id } );
                    addPath();
                    break;

                case "Map.img":
                    id = id.PadLeft(9, '0');
                    wzPath.AddRange(new string[] { "Map", "Map", "Map" + id[0], id + ".img" });
                    addPath();
                    break;

                case "Mob.img":
                    wzPath.Add("Mob");
                    wzPath.Add(id.PadLeft(7, '0') + ".img");
                    addPath();
                    //Add special mobs
                    foreach (var i in new string[] { "AbyssExpeditionMob", "MExplorerMob", "QuestCountGroup", "RoguelikeMob" })
                    {
                        wzPath.Clear();
                        wzPath.AddRange(new string[] { "Mob", i, id.PadLeft(7, '0') + ".img" });
                        addPath();
                    }
                    //Redmoon
                    wzPath.Clear();
                    wzPath.AddRange(new string[] { "Mob", "RoguelikeMob", "Redmoon", id.PadLeft(7, '0') + ".img" });
                    addPath();
                    break;

                case "Npc.img":
                    wzPath.Add("Npc");
                    wzPath.Add(id.PadLeft(7, '0') + ".img");
                    addPath();
                    break;

                case "Roguelike.img":
                case "Redmoon.img":
                    switch (pathArray[1])
                    {
                        case "artifact":
                            wzPath.Add("Item");
                            wzPath.Add("Consume");
                            id = id.PadLeft(8, '0');
                            wzPath.Add(id.Substring(0, 4) + ".img");
                            imagePath.Add(id);
                            addPath();
                            break;
                        case "skill":
                            foreach (var i in new string[] { "Skill", "Buff" })
                            {
                                wzPath.Clear();
                                wzPath.AddRange(new string[] { "Skill", "Roguelike", i, id + ".img" });
                                addPath();
                                wzPath.Clear();
                                wzPath.AddRange(new string[] { "Skill", "Roguelike", i, "Redmoon", id + ".img" });
                                addPath();
                            }
                            break;
                    }
                    break;

                case "Skill.img":
                    id = id.PadLeft(7, '0');
                    wzPath.Add("Skill");
                    //old skill
                    wzPath.Add(id.Substring(0, id.Length - 4) + ".img");
                    imagePath.Add("skill");
                    imagePath.Add(id);
                    addPath();
                    if (Regex.IsMatch(id, @"80\d{6}")) //kmst new skill
                    {
                        wzPath[1] = id.Substring(0, 6) + ".img";
                        addPath();
                    }
                    break;

                case "0910.img":
                    wzPath.Add("Item");
                    wzPath.Add("Special");
                    wzPath.Add("0910.img");
                    imagePath.Add(id);
                    addPath();
                    break;

                case "SetItemInfo.img":
                    wzPath.Add("Etc");
                    wzPath.Add("SetItemInfo.img");
                    imagePath.Add(id);
                    addPath();
                    break;

                case "QuestData":
                    wzPath.Add("Quest");
                    wzPath.Add("QuestData");
                    wzPath.Add($"{id}.img");
                    addPath();
                    break;

                case "QuestInfo.img":
                    wzPath.Add("Quest");
                    wzPath.Add("QuestInfo.img");
                    wzPath.Add($"{id}");
                    addPath();
                    break;

                case "AchievementData":
                    wzPath.Add("Etc");
                    wzPath.Add("Achievement");
                    wzPath.Add("AchievementData");
                    wzPath.Add($"{id}.img");
                    addPath();
                    break;

                default:
                    break;
            }

            return pathList;
        }

        /// <summary>
        /// 通过给定的wz名称，在advTree1中寻找第一个对应的wz_file节点。
        /// </summary>
        /// <param Name="wzName">要寻找的wz名称，不包含".wz"后缀。</param>
        /// <returns></returns>
        private Node findWzFileTreeNode(string wzName)
        {
            Wz_Type type = ParseType(wzName);
            if (type == Wz_Type.Unknown)
            {
                return null;
            }

            foreach (var wzs in this.openedWz)
            {
                foreach (var wzf in wzs.wz_files)
                {
                    if (wzf.Type == type)
                    {
                        Node node = findWzFileTreeNode(wzf);
                        if (node != null)
                        {
                            return node;
                        }
                    }
                }
            }

            return null;
        }

        private Wz_Type ParseType(string wzName)
        {
            Wz_Type type;
            try
            {
                type = (Wz_Type)Enum.Parse(typeof(Wz_Type), wzName, true);
            }
            catch
            {
                type = Wz_Type.Unknown;
            }

            return type;
        }

        private Node findWzFileTreeNode(Wz_File wzFile)
        {
            foreach (Node baseNode in advTree1.Nodes)
            {
                Wz_File wz_f = baseNode.AsWzNode()?.Value as Wz_File;
                if (wz_f != null)
                {
                    if (wz_f == wzFile)
                    {
                        return baseNode;
                    }
                    else if (wz_f.Type == Wz_Type.Base)
                    {
                        foreach (Node wzNode in baseNode.Nodes)
                        {
                            if ((wz_f = wzNode.AsWzNode()?.Value as Wz_File) != null && wz_f == wzFile)
                            {
                                return wzNode;
                            }
                        }
                    }
                }
            }
            return null;
        }

        private Node findChildTreeNode(Node parent, string[] path)
        {
            if (parent == null || path == null)
                return null;
            for (int i = 0; i < path.Length; i++)
            {
                bool find = false;
                foreach (Node subNode in parent.Nodes)
                {
                    if (subNode.Text == path[i])
                    {
                        parent = subNode;
                        find = true;
                        break;
                    }
                }
                if (!find)
                {
                    return null;
                }
            }
            return parent;
        }
        #endregion

        #region contextMenuStrip1
        private void tsmi1Sort_Click(object sender, EventArgs e)
        {
            if (openedWz.Count > 0)
            {
                var sw = Stopwatch.StartNew();
                advTree1.BeginUpdate();
                try
                {
                    advTree1.ClearAndDisposeAllNodes();
                    foreach (Wz_Structure wz in openedWz)
                    {
                        sortWzNode(wz.WzNode);
                        Node node = createNode(wz.WzNode);
                        node.Expand();
                        advTree1.Nodes.Add(node);
                    }
                }
                finally
                {
                    advTree1.EndUpdate();
                    sw.Stop();
                }
                GC.Collect();
                labelItemStatus.Text = $"排序完成，用時{sw.ElapsedMilliseconds}毫秒。";
            }
            else
            {
                labelItemStatus.Text = "排序失敗：WZ 檔案未開啟。";
            }
        }

        private async void tsmi1Export_Click(object sender, EventArgs e)
        {
            Wz_Node node = advTree1.SelectedNode?.AsWzNode();
            Wz_Image img = node.GetValue<Wz_Image>();
            if (img != null)
            {
                SaveFileDialog dlg = new SaveFileDialog();
                dlg.DefaultExt = ".img";
                dlg.FileName = img.Name;
                dlg.Filter = "IMG (*.img)|*.img";
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    FileStream fs = null;
                    try
                    {
                        fs = new FileStream(dlg.FileName, FileMode.Create, FileAccess.Write);
                        var s = img.OpenRead();
                        s.Position = 0;
                        s.CopyTo(fs);
                        fs.Close();
                        labelItemStatus.Text = "已匯出: " + img.Name;
                    }
                    catch (Exception ex)
                    {
                        fs?.Close();
                        ToastNotification.Show(this, $"錯誤: {ex.Message}", null, 3000, eToastGlowColor.Red, eToastPosition.TopCenter);
                    }
                }
            }
            else if (node != null)
            {
                FolderBrowserDialog dlg = new FolderBrowserDialog();
                dlg.Description = "請選擇要匯出的資料夾。";
                if (dlg.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }
                string exportedFolder = dlg.SelectedPath;
                if (!Directory.Exists(Path.Combine(exportedFolder, node.FullPathToFile)))
                {
                    Directory.CreateDirectory(Path.Combine(exportedFolder, node.FullPathToFile));
                }
                labelItemStatus.Text = "匯出中";
                progressBarItem1.Value = 0;
                progressBarItem1.Visible = true;
                await Task.Run(() =>
                {
                    foreach (Wz_Node wz_Node in node.Nodes)
                    {
                        Wz_Image wzImage = wz_Node.GetValue<Wz_Image>();
                        if (wzImage != null)
                        {
                            try
                            {
                                string fileName = Path.Combine(Path.Combine(exportedFolder, node.FullPathToFile), wzImage.Name);
                                dumpImgToFile(wzImage, fileName);
                            }
                            catch (Exception ex)
                            {
                                ToastNotification.Show(this, $"錯誤: {ex.Message}", null, 3000, eToastGlowColor.Red, eToastPosition.TopCenter);
                                break;
                            }
                        }
                        else
                        {
                            recurseImgDirectory(wz_Node, exportedFolder);
                        }
                    }
                });
                progressBarItem1.Visible = false;
                labelItemStatus.Text = "已匯出: " + exportedFolder;
            }
            else
            {
                MessageBoxEx.Show("選擇您想要匯出的 IMG。");
                return;
            }
        }

        private void dumpImgToFile(Wz_Image img, string path)
        {
            if (img == null || string.IsNullOrEmpty(path))
            {
                return;
            }
            try
            {
                this.Invoke((System.Windows.Forms.MethodInvoker)(() =>
                {
                    labelItemStatus.Text = "匯出中: " + img.Name;
                }));
                using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write))
                {
                    var s = img.OpenRead();
                    s.Position = 0;
                    s.CopyTo(fs);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void recurseImgDirectory(Wz_Node node, string directory)
        {
            if (!Directory.Exists(Path.Combine(directory, node.FullPathToFile)))
            {
                Directory.CreateDirectory(Path.Combine(directory, node.FullPathToFile));
            }
            foreach (Wz_Node subNode in node.Nodes)
            {
                Wz_Image subImg = subNode.GetValue<Wz_Image>();
                if (subImg != null)
                {
                    string fileName = Path.Combine(Path.Combine(directory, node.FullPathToFile), subImg.Name);
                    try
                    {
                        dumpImgToFile(subImg, fileName);
                    }
                    catch (Exception ex)
                    {
                        throw ex;

                    }
                }
                else
                {
                    recurseImgDirectory(subNode, directory);
                }
            }
        }

        private async void tsmi1DumpAsXml_Click(object sender, EventArgs e)
        {
            Wz_Node node = advTree1.SelectedNode?.AsWzNode();
            Wz_Image img = node.GetValue<Wz_Image>();
            if (img != null)
            {
                SaveFileDialog dlg = new SaveFileDialog();
                dlg.DefaultExt = ".xml";
                dlg.Filter = "XML (*.xml)|*.xml";
                dlg.FileName = img.Node.FullPathToFile.Replace('\\', '.') + ".xml";
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    FileStream fs = null;
                    try
                    {
                        fs = new FileStream(dlg.FileName, FileMode.Create, FileAccess.Write);
                        var xsetting = new XmlWriterSettings()
                        {
                            CloseOutput = false,
                            Indent = true,
                            Encoding = Encoding.UTF8,
                            CheckCharacters = true,
                            NewLineChars = Environment.NewLine,
                            NewLineOnAttributes = false,
                        };
                        var writer = XmlWriter.Create(fs, xsetting);
                        writer.WriteStartDocument(true);
                        img.Node.DumpAsXml(writer);
                        writer.WriteEndDocument();
                        writer.Close();

                        labelItemStatus.Text = "已匯出: " + img.Name + "為 XML 檔案";
                    }
                    catch (Exception ex)
                    {
                        ToastNotification.Show(this, $"錯誤: {ex.Message}", null, 3000, eToastGlowColor.Red, eToastPosition.TopCenter);
                    }
                    finally
                    {
                        if (fs != null)
                        {
                            fs.Close();
                        }
                    }
                }
            }
            // else if (node != null)
            // {
            //     FolderBrowserDialog dlg = new FolderBrowserDialog();
            //     dlg.Description = "請選擇要匯出的資料夾。";
            //     if (dlg.ShowDialog(this) != DialogResult.OK)
            //     {
            //         return;
            //     }
            //     string exportedFolder = dlg.SelectedPath;
            //     if (!Directory.Exists(Path.Combine(exportedFolder, node.FullPathToFile)))
            //     {
            //         Directory.CreateDirectory(Path.Combine(exportedFolder, node.FullPathToFile));
            //     }
            //     labelItemStatus.Text = "匯出中";
            //     progressBarItem1.Value = 0;
            //     progressBarItem1.Visible = true;

            //     await Task.Run(() =>
            //     {
            //         foreach (Wz_Node wz_Node in node.Nodes)
            //         {
            //             Wz_Image wzImage = wz_Node.GetValue<Wz_Image>();
            //             if (wzImage != null)
            //             {
            //                 try
            //                 {
            //                     string fileName = Path.Combine(Path.Combine(exportedFolder, node.FullPathToFile), wzImage.Name + ".xml");
            //                     dumpXmlToFile(wz_Node, fileName);
            //                 }
            //                 catch (Exception ex)
            //                 {
            //                     ToastNotification.Show(this, $"錯誤: {ex.Message}", null, 3000, eToastGlowColor.Red, eToastPosition.TopCenter);
            //                     break;
            //                 }
            //             }
            //             else
            //             {
            //                 recurseXmlDirectory(wz_Node, exportedFolder);
            //             }
            //         }
            //     });
            //     progressBarItem1.Visible = false;
            //     labelItemStatus.Text = "已匯出: " + exportedFolder;
            // }
            else
            {
                MessageBoxEx.Show("暫未實現批量匯出 XML 檔案。");
                return;
            }
        }

        private void dumpXmlToFile(Wz_Node node, string path)
        {
            if (node == null || string.IsNullOrEmpty(path))
            {
                return;
            }
            try
            {
                this.Invoke((System.Windows.Forms.MethodInvoker)(() =>
                {
                    labelItemStatus.Text = "匯出中: " + node.GetValue<Wz_Image>().Name;
                }));
                using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write))
                {
                    var xsetting = new XmlWriterSettings()
                    {
                        CloseOutput = false,
                        Indent = true,
                        Encoding = Encoding.UTF8,
                        CheckCharacters = true,
                        NewLineChars = Environment.NewLine,
                        NewLineOnAttributes = false,
                    };
                    var writer = XmlWriter.Create(fs, xsetting);
                    writer.WriteStartDocument(true);
                    node.DumpAsXml(writer);
                    writer.WriteEndDocument();
                    writer.Close();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void recurseXmlDirectory(Wz_Node node, string directory)
        {
            if (!Directory.Exists(Path.Combine(directory, node.FullPathToFile)))
            {
                Directory.CreateDirectory(Path.Combine(directory, node.FullPathToFile));
            }
            foreach (Wz_Node subNode in node.Nodes)
            {
                Wz_Image subImg = subNode.GetValue<Wz_Image>();
                if (subImg != null)
                {
                    string fileName = Path.Combine(Path.Combine(directory, node.FullPathToFile), subImg.Name + ".xml");
                    try
                    {
                        dumpXmlToFile(subNode, fileName);
                    }
                    catch (Exception ex)
                    {
                        throw ex;

                    }
                }
                else
                {
                    recurseXmlDirectory(subNode, directory);
                }
            }
        }

        private async void tsmi1UpdateStringLinker_Click(object sender, EventArgs e)
        {
            Wz_Node selectedNode = advTree1.SelectedNode?.AsWzNode();
            Wz_Node stringNode = selectedNode?.FindNodeByPath("String") ?? selectedNode?.FindNodeByPath("String.img", true);
            Wz_Node itemNode = selectedNode?.FindNodeByPath("Item") ?? selectedNode?.FindNodeByPath("Item.img", true);
            Wz_Node etcNode = selectedNode?.FindNodeByPath("Etc") ?? selectedNode?.FindNodeByPath("Etc.img", true);
            Wz_Node questNode = selectedNode?.FindNodeByPath("Quest") ?? selectedNode?.FindNodeByPath("Quest.img", true);

            bool r = false;

            btnItemOpenWz.Enabled = false;
            btnItemOpenImg.Enabled = false;
            buttonItemClose.Enabled = false;
            buttonItemCloseAll.Enabled = false;
            buttonItemSearchWz.Enabled = false;
            buttonItemSearchString.Enabled = false;
            galleryContainerRecent.Enabled = false;

            await Task.Run(() =>
            {
                QueryPerformance.Start();
                labelItemStatus.Text = "StringLinker更新中...";
                UnityAssetDocument unityDoc = this.FindUnityDocument(advTree1.SelectedNode);
                if (unityDoc != null && unityDoc.StringTables != null)
                {
                    // Unity 資料集的字串表是整體重建，選取的子樹不另外 Update。
                    r = this.LoadStringLinker();
                }
                else
                {
                    r = this.LoadStringLinker() && stringLinker.Update(stringNode, itemNode, etcNode, questNode); //reset(needed?) and update
                }
                QueryPerformance.End();
            });
            if (r)
            {
                double ms = (Math.Round(QueryPerformance.GetLastInterval(), 4) * 1000);
                labelItemStatus.Text = "StringLinker更新完成。經過時間：" + ms + "毫秒";
            }
            else
            {
                MessageBoxEx.Show("StringLinker更新失敗。", "錯誤");
            }


            btnItemOpenWz.Enabled = true;
            btnItemOpenImg.Enabled = true;
            buttonItemClose.Enabled = true;
            buttonItemCloseAll.Enabled = true;
            buttonItemSearchWz.Enabled = true;
            buttonItemSearchString.Enabled = true;
            galleryContainerRecent.Enabled = true;
        }

        private async void tsmi1ExportSound_Click(object sender, EventArgs e)
        {
            Wz_Node soundNode = advTree1.SelectedNode?.AsWzNode();
            if (soundNode != null && soundNode.Value is Wz_Image)
            {
                FolderBrowserDialog dlg = new FolderBrowserDialog();
                dlg.Description = "請選擇要匯出的資料夾。";
                if (dlg.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }
                string exportedFolder = Path.Combine(dlg.SelectedPath, soundNode.FullPathToFile);
                if (!Directory.Exists(exportedFolder))
                {
                    Directory.CreateDirectory(exportedFolder);
                }
                labelItemStatus.Text = "匯出中";
                progressBarItem1.Value = 0;
                progressBarItem1.Visible = true;
                await Task.Run(() =>
                {
                    BatchExportSound(soundNode.GetValue<Wz_Image>().Node, exportedFolder);
                });
                progressBarItem1.Visible = false;
                labelItemStatus.Text = "匯出完成至: " + exportedFolder;
            }
            else
            {
                using (FrmSoundExport frm = new FrmSoundExport(styleManager1.ManagerStyle == eStyle.VisualStudio2012Dark))
                {
                    foreach (var img in soundNode.Nodes)
                    {
                        frm.AddSoundEntry(img.Text);
                    }
                    if (frm.ShowDialog() == DialogResult.OK)
                    {
                        string exportedFolder = Path.Combine(frm.ExportFolderPath, soundNode.FullPathToFile);
                        if (!Directory.Exists(exportedFolder))
                        {
                            Directory.CreateDirectory(exportedFolder);
                        }
                        labelItemStatus.Text = "匯出中";
                        progressBarItem1.Value = 0;
                        progressBarItem1.Visible = true;
                        await Task.Run(() =>
                        {
                            foreach (var img in soundNode.Nodes)
                            {
                                if (!frm.SelectedSoundCodes.Contains(img.Text))
                                {
                                    continue;
                                }
                                Wz_Image image = img.GetValue<Wz_Image>();
                                if (image.TryExtract())
                                {
                                    if (!Directory.Exists(Path.Combine(exportedFolder, img.Text)))
                                    {
                                        Directory.CreateDirectory(Path.Combine(exportedFolder, img.Text));
                                    }
                                    BatchExportSound(image.Node, Path.Combine(exportedFolder, img.Text));
                                }
                            }
                        });
                        progressBarItem1.Visible = false;
                        labelItemStatus.Text = "匯出完成至: " + exportedFolder;
                    }
                }
            }
        }

        private void BatchExportSound(Wz_Node node, string path)
        {
            foreach (Wz_Node child in node.Nodes)
            {
                if (child.Value is Wz_Sound sound)
                {
                    string soundName = child.Text;
                    this.Invoke((System.Windows.Forms.MethodInvoker)(() =>
                    {
                        labelItemStatus.Text = $"匯出中: {soundName}";
                    }));
                    byte[] soundData = sound.ExtractSound();
                    if (soundData == null || soundData.Length <= 0)
                    {
                        continue;
                    }
                    switch (sound.SoundType)
                    {
                        case Wz_SoundType.Mp3: soundName += ".mp3"; break;
                        case Wz_SoundType.Pcm: soundName += ".wav"; break;
                    }

                    FileStream fs = null;
                    try
                    {
                        fs = new FileStream(Path.Combine(path, RemoveInvalidFileNameChars(soundName)), FileMode.Create, FileAccess.Write);
                        fs.Write(soundData, 0, soundData.Length);
                    }
                    catch
                    {
                    }
                    finally
                    {
                        fs?.Close();
                    }
                }
                else if (child.Value == null && child.Nodes.Count > 0)
                {
                    string subDirPath = Path.Combine(path, RemoveInvalidFileNameChars(child.Text));
                    if (!Directory.Exists(subDirPath))
                    {
                        Directory.CreateDirectory(subDirPath);
                    }
                    BatchExportSound(child, subDirPath);
                }
            }
        }

        private async void tsmi1SaveImgList_Click(object sender, EventArgs e)
        {
            Wz_Node node = advTree1.SelectedNode?.AsWzNode();
            Wz_Image img = node.GetValue<Wz_Image>();
            if (img != null)
            {
                MessageBoxEx.Show("選擇要匯出的目錄。");
                return;
            }
            else if (node != null)
            {
                SaveFileDialog dlg = new SaveFileDialog();
                dlg.DefaultExt = ".txt";
                dlg.FileName = RemoveInvalidFileNameChars(node.FullPathToFile) + ".txt";
                dlg.Filter = "TXT (*.txt)|*.txt";
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    StreamWriter sw = new StreamWriter(dlg.FileName);
                    try
                    {
                        foreach (Wz_Node wz_Node in node.Nodes)
                        {
                            Wz_Image wzImage = wz_Node.GetValue<Wz_Image>();
                            if (wzImage != null)
                            {
                                try
                                {
                                    sw.WriteLine(wzImage.Name);
                                }
                                catch (Exception ex)
                                {
                                    ToastNotification.Show(this, $"錯誤: {ex.Message}", null, 3000, eToastGlowColor.Red, eToastPosition.TopCenter);
                                    break;
                                }
                            }
                        }
                        sw?.Close();
                        labelItemStatus.Text = "已匯出: " + dlg.FileName;
                    }
                    catch (Exception ex)
                    {
                        sw?.Close();
                        ToastNotification.Show(this, $"錯誤: {ex.Message}", null, 3000, eToastGlowColor.Red, eToastPosition.TopCenter);
                    }
                }
            }
            else
            {
                MessageBoxEx.Show("選擇要匯出的目錄。");
                return;
            }
        }
        #endregion

        #region Tools菜单事件和方法
        private void buttonItemSearchWz_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBoxItemSearchWz.Text))
                return;
            if (comboBoxItem1.SelectedIndex == -1)
            {
                comboBoxItem1.SelectedIndex = 0;
            }

            switch (comboBoxItem1.SelectedIndex)
            {
                case 0:
                    searchAdvTree(advTree1, 0, textBoxItemSearchWz.Text, checkBoxItemExact1.Checked, checkBoxItemRegex1.Checked);
                    break;
                case 1:
                    searchAdvTree(advTree2, 0, textBoxItemSearchWz.Text, checkBoxItemExact1.Checked, checkBoxItemRegex1.Checked);
                    break;
                case 2:
                    searchAdvTree(advTree3, 1, textBoxItemSearchWz.Text, checkBoxItemExact1.Checked, checkBoxItemRegex1.Checked);
                    break;
                case 3:
                    searchAdvTreeEx(advTree3, 0, 1, textBoxItemSearchWz.Text);
                    break;
                case 4: //full path
                    searchAdvTreeFullPath(textBoxItemSearchWz.Text);
                    break;
            }
        }

        private void searchAdvTree(AdvTree advTree, int cellIndex, string searchText, bool exact, bool regex)
        {
            if (string.IsNullOrEmpty(searchText))
                return;

            try
            {
                Node searchNode = searchAdvTree(advTree, cellIndex, searchText, exact, regex, true);
                advTree.SelectedNode = searchNode;
                if (searchNode == null)
                    MessageBoxEx.Show("未找到結果。", LocalizedString_JP.COMMON_ERROR);
            }
            catch (Exception ex)
            {
                MessageBoxEx.Show(this, ex.Message, LocalizedString_JP.COMMON_ERROR, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void searchAdvTreeEx(AdvTree advTree, int cellIndex1, int cellIndex2, string searchText)
        {
            if (string.IsNullOrEmpty(searchText))
                return;

            try
            {
                if (advTree.Nodes.Count == 0)
                    return;

                Node searchNode = null;

                // split input by ','
                var searchText1 = "^" + searchText.Split(',')[0] + "$";
                var searchText2 = "^" + searchText.Split(',')[1] + "$";

                if (string.IsNullOrEmpty(searchText2))
                    return;

                Regex r1 = new Regex(searchText1, RegexOptions.IgnoreCase);
                Regex r2 = new Regex(searchText2, RegexOptions.IgnoreCase);

                foreach (var node in findNextNode(advTree))
                {
                    if (node != null && node.Cells.Count > Math.Max(cellIndex1, cellIndex2) && r1.IsMatch(node.Cells[cellIndex1].Text) && r2.IsMatch(node.Cells[cellIndex2].Text))
                    {
                        searchNode = node;
                        break;
                    }
                }

                advTree.SelectedNode = searchNode;
                if (searchNode == null)
                    MessageBoxEx.Show("未找到搜尋結果。", "錯誤");
            }
            catch (Exception ex)
            {
                MessageBoxEx.Show(this, ex.Message, "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private Node searchAdvTree(AdvTree advTree, int cellIndex, string searchText, bool exact, bool isRegex, bool ignoreCase)
        {
            if (advTree.Nodes.Count == 0)
                return null;

            if (isRegex)
            {
                Regex r = new Regex(searchText, ignoreCase ? RegexOptions.IgnoreCase : RegexOptions.None);
                foreach (var node in findNextNode(advTree))
                {
                    if (node != null && node.Cells.Count > cellIndex && r.IsMatch(node.Cells[cellIndex].Text))
                    {
                        return node;
                    }
                }
            }
            else
            {
                string[] pattern = searchText.Split('\\');
                foreach (var node in findNextNode(advTree))
                {
                    if (checkSearchNodeText(node, cellIndex, pattern, exact, ignoreCase))
                    {
                        return node;
                    }
                }
            }

            return null;
        }

        private void searchAdvTreeFullPath(string fullPath)
        {
            string[] pathSegments = fullPath.Split('/');

            bool isNodePathMatches(string pathSegment, string nodeName, StringComparison stringComparison)
            {
                if (string.Equals(pathSegment, nodeName, stringComparison))
                {
                    return true;
                }
                int pathExtIndex = pathSegment.LastIndexOf('.');
                int nodeExtIndex = nodeName.LastIndexOf(".");
                if (pathExtIndex != -1 || nodeExtIndex != -1)
                {
                    ReadOnlySpan<char> pathWithoutExt = pathExtIndex == -1 ? pathSegment.AsSpan() : pathSegment.AsSpan(0, pathExtIndex);
                    ReadOnlySpan<char> nodeWithoutExt = nodeExtIndex == -1 ? nodeName.AsSpan() : nodeName.AsSpan(0, nodeExtIndex);
                    return pathWithoutExt.Equals(nodeWithoutExt, stringComparison);
                }
                return false;
            }

            IEnumerable<(Node result, int resultPathLevel)> searchNode(Node treeNode, int pathLevel)
            {
                string pathSegment = pathSegments[pathLevel];
                if (isNodePathMatches(pathSegment, treeNode.Text, StringComparison.OrdinalIgnoreCase))
                {
                    if (treeNode.Nodes.Count == 0 || pathLevel == pathSegments.Length - 1)
                    {
                        yield return (treeNode, pathLevel);
                    }
                    foreach (Node childNode in treeNode.Nodes)
                    {
                        foreach (var resultTuple in searchNode(childNode, pathLevel + 1))
                        {
                            yield return resultTuple;
                        }
                    }
                }
            }

            foreach (var node in findNextNode(this.advTree1))
            {
                foreach ((Node result, int resultPathLevel) in searchNode(node, 0))
                {
                    if (resultPathLevel == pathSegments.Length - 1)
                    {
                        this.advTree1.SelectedNode = node;
                        return;
                    }

                    var wzNode = result.AsWzNode();
                    if (wzNode != null && wzNode.Value is Wz_Image wzImg && wzImg.TryExtract(out _))
                    {
                        // find remaining path in wzImg
                        wzNode = wzImg.Node;
                        for (int i = resultPathLevel + 1; i < pathSegments.Length; i++)
                        {
                            string pathSegment = pathSegments[i];
                            wzNode = wzNode.Nodes.FirstOrDefault(child => isNodePathMatches(pathSegment, child.Text, StringComparison.OrdinalIgnoreCase));
                            if (wzNode == null)
                            {
                                break;
                            }
                        }
                        if (wzNode != null && this.OnSelectedWzNode(wzNode))
                        {
                            return;
                        }
                    }
                }
            }

            this.advTree1.SelectedNode = null;
            MessageBoxEx.Show(this, "已搜尋到結尾。");
        }

        private IEnumerable<Node> findNextNode(AdvTree advTree)
        {
            var node = advTree.SelectedNode;
            if (node == null)
            {
                node = advTree.Nodes[0];
                yield return node;
            }

            var levelStack = new Stack<int>();
            int index = node.Index + 1;

            while (true)
            {
                if (node.Nodes.Count > 0)
                {
                    levelStack.Push(index);
                    index = 0;
                    yield return node = node.Nodes[index++];
                    continue;
                }

                NodeCollection owner;

                while (index >= (owner = (node.Parent?.Nodes ?? advTree.Nodes)).Count)
                {
                    node = node.Parent;
                    if (node == null)
                    {
                        yield break;
                    }
                    if (levelStack.Count > 0)
                    {
                        index = levelStack.Pop();
                    }
                    else
                    {
                        index = node.Index + 1;
                    }
                }

                yield return node = owner[index++];
            }
        }

        private bool checkSearchNodeText(Node node, int cellIndex, string[] searchTextArray, bool exact, bool ignoreCase)
        {
            if (node == null || searchTextArray == null || searchTextArray.Length == 0)
                return false;
            for (int i = searchTextArray.Length - 1; i >= 0; i--)
            {
                if (node == null || node.Cells.Count <= cellIndex)
                    return false;
                if (exact)
                {
                    if (string.Compare(node.Cells[cellIndex].Text, searchTextArray[i], ignoreCase) != 0)
                        return false;
                }
                else
                {
                    if (ignoreCase ? node.Cells[cellIndex].Text.IndexOf(searchTextArray[i], StringComparison.CurrentCultureIgnoreCase) < 0 :
                        !node.Cells[cellIndex].Text.Contains(searchTextArray[i]))
                        return false;
                }

                node = node.Parent;
            }
            return true;
        }

        private void textBoxItemSearchWz_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (!buttonItemSearchWz.Enabled) return;
                buttonItemSearchWz_Click(buttonItemSearchWz, EventArgs.Empty);
            }
        }

        private void buttonItemSearchString_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBoxItemSearchString.Text))
                return;
            QueryPerformance.Start();
            if (!this.stringLinker.HasValues)
            {
                if (!this.LoadStringLinker())
                {
                    MessageBoxEx.Show("選擇 Base.wz。", LocalizedString_JP.COMMON_ERROR);
                    return;
                }
                QueryPerformance.End();
                double ms = (Math.Round(QueryPerformance.GetLastInterval(), 4) * 1000);
                labelItemStatus.Text = "字串表連結已被重置。時間經過：" + ms + "毫秒";
            }
            if (comboBoxItem2.SelectedIndex < 0)
                comboBoxItem2.SelectedIndex = 0;

            List<Dictionary<int, StringResult>> dicts = new List<Dictionary<int, StringResult>>();
            switch (comboBoxItem2.SelectedIndex)
            {
                case 0:
                    dicts.Add(stringLinker.StringEqp);
                    dicts.Add(stringLinker.StringGuildCastleGuildResearch);
                    dicts.Add(stringLinker.StringGuildCastlePersonalResearch);
                    dicts.Add(stringLinker.StringItem);
                    dicts.Add(stringLinker.StringMap);
                    dicts.Add(stringLinker.StringMob);
                    dicts.Add(stringLinker.StringNpc);
                    dicts.Add(stringLinker.StringQuest);
                    dicts.Add(stringLinker.StringRoguelikeSkill);
                    dicts.Add(stringLinker.StringSkill);
                    dicts.Add(stringLinker.StringSetItem);
                    dicts.Add(stringLinker.StringAchievement);
                    break;
                case 1:
                    dicts.Add(stringLinker.StringEqp);
                    break;
                case 2:
                    dicts.Add(stringLinker.StringItem);
                    break;
                case 3:
                    dicts.Add(stringLinker.StringMap);
                    break;
                case 4:
                    dicts.Add(stringLinker.StringMob);
                    break;
                case 5:
                    dicts.Add(stringLinker.StringNpc);
                    break;
                case 6:
                    dicts.Add(stringLinker.StringQuest);
                    break;
                case 7:
                    dicts.Add(stringLinker.StringGuildCastleGuildResearch);
                    dicts.Add(stringLinker.StringGuildCastlePersonalResearch);
                    dicts.Add(stringLinker.StringRoguelikeSkill);
                    dicts.Add(stringLinker.StringSkill);
                    break;
                case 8:
                    dicts.Add(stringLinker.StringSetItem);
                    break;
                case 9:
                    dicts.Add(stringLinker.StringAchievement);
                    break;
            }

            listViewExString.BeginUpdate();
            try
            {
                listViewExString.Items.Clear();
                IEnumerable<KeyValuePair<int, StringResult>> results = searchStringLinker(dicts, textBoxItemSearchString.Text, checkBoxItemExact2.Checked, checkBoxItemRegex2.Checked, buttonItemIgnoreArticles.Checked);
                foreach (KeyValuePair<int, StringResult> kv in results)
                {
                    string[] item = new string[] { kv.Key.ToString(), kv.Value.Name, kv.Value.Desc, kv.Value.FullPath };
                    listViewExString.Items.Add(new ListViewItem(item));
                }
            }
            catch (Exception ex)
            {
                MessageBoxEx.Show(ex.Message, LocalizedString_JP.COMMON_ERROR, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                listViewExString.EndUpdate();
            }            
        }

        private bool TryLoadStringWz()
        {
            foreach (Wz_Structure wz in openedWz)
            {
                foreach (Wz_File file in wz.wz_files)
                {
                    if (file.Type == Wz_Type.String && this.stringLinker.Load(file, null, null, null))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private Wz_File findStringWz()
        {
            foreach (Wz_Structure wz in openedWz)
            {
                foreach (Wz_File file in wz.wz_files)
                {
                    if (file.Type == Wz_Type.String && file.Node.Nodes.Count > 0)
                    {
                        return file;
                    }
                }
            }
            return null;
        }

        private Wz_File findItemWz()
        {
            foreach (Wz_Structure wz in openedWz)
            {
                foreach (Wz_File file in wz.wz_files)
                {
                    if (file.Type == Wz_Type.Item && file.Node.Nodes.Count > 0)
                    {
                        return file;
                    }
                }
            }
            return null;
        }

        private Wz_File findEtcWz()
        {
            foreach (Wz_Structure wz in openedWz)
            {
                foreach (Wz_File file in wz.wz_files)
                {
                    if (file.Type == Wz_Type.Etc && file.Node.Nodes.Count > 0)
                    {
                        return file;
                    }
                }
            }
            return null;
        }

        private Wz_File findQuestWz()
        {
            foreach (Wz_Structure wz in openedWz)
            {
                foreach (Wz_File file in wz.wz_files)
                {
                    if (file.Type == Wz_Type.Quest && file.Node.Nodes.Count > 0)
                    {
                        return file;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// 依目前開啟的資料集載入字串表：優先使用 WZ，其次使用 Unity 資料集
        /// （String/&lt;region&gt;/&lt;category&gt;）。
        /// </summary>
        private bool LoadStringLinker()
        {
            Wz_File stringWz = findStringWz();
            Wz_File itemWz = findItemWz();
            Wz_File etcWz = findEtcWz();
            Wz_File questWz = findQuestWz();
            if (stringWz != null || itemWz != null || etcWz != null || questWz != null)
            {
                return this.stringLinker.Load(stringWz, itemWz, etcWz, questWz);
            }

            UnityAssetDocument unityDoc = this.openedUnitySources.Values
                .FirstOrDefault(doc => doc.StringTables != null && doc.StringTables.HasValues);
            if (unityDoc != null)
            {
                this.stringLinker.Clear();
                return this.stringLinker.Load(
                    unityDoc.StringTables.StringNode,
                    unityDoc.StringTables.ItemNode,
                    unityDoc.StringTables.EtcNode,
                    unityDoc.StringTables.QuestNode);
            }

            this.stringLinker.Clear();
            return false;
        }

        /// <summary>找出 AdvTree 節點所屬的 Unity 資料集（往根節點找）。</summary>
        private UnityAssetDocument FindUnityDocument(Node node)
        {
            while (node != null)
            {
                if (node.Parent == null)
                {
                    UnityAssetDocument document;
                    return this.openedUnitySources.TryGetValue(node, out document) ? document : null;
                }
                node = node.Parent;
            }
            return null;
        }

        /// <summary>判斷是否至少包含一個 CJK 統合漢字(簡體字/繁體字/漢字)。</summary>
        private static bool HasCjkIdeograph(string text)
        {
            if (string.IsNullOrEmpty(text))
                return false;
            foreach (char c in text)
            {
                if ((c >= '\u3400' && c <= '\u4DBF') || (c >= '\u4E00' && c <= '\u9FFF'))
                {
                    return true;
                }
            }
            return false;
        }

        private IEnumerable<KeyValuePair<int, StringResult>> searchStringLinker(IEnumerable<Dictionary<int, StringResult>> dicts, string key, bool exact, bool isRegex, bool ignoreArticles)
        {
            // 除了原始鍵之外，也準備將假名、全形/半形、簡體字/繁體字正規化後的鍵。
            string fullWidthKey = Translator.FullWidthKatakana(key);
            string cnKey = Translator.NormalizeForSearch(key);
            bool cnKeyHasHan = HasCjkIdeograph(key);

            string[] match = (key).Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            string[] match2 = (fullWidthKey).Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            string[] matchCn = (cnKey).Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            string[] articles = { "a", "an", "the" };
            if (ignoreArticles)
            {
                match = match.Where(word => !articles.Contains(word.ToLower())).ToArray();
                match2 = match2.Where(word => !articles.Contains(word.ToLower())).ToArray();
                matchCn = matchCn.Where(word => !articles.Contains(word.ToLower())).ToArray();
            }
            Regex re = null;
            Regex fullWidthRe = null;
            Regex cnRe = null;
            if (isRegex)
            {
                if (ignoreArticles)
                {
                    string pattern = string.Join(@"\s+(?:a|an|the)?\s*", match.Select(Regex.Escape));
                    string fullWidthPattern = string.Join(@"\s+(?:a|an|the)?\s*", match2.Select(Regex.Escape));
                    pattern = $@"\b{pattern}\b";
                    fullWidthPattern = $@"\b{fullWidthPattern}\b";
                    re = new Regex(pattern, RegexOptions.IgnoreCase);
                    fullWidthRe = new Regex(fullWidthPattern, RegexOptions.IgnoreCase);
                    if (cnKeyHasHan && matchCn.Length > 0)
                    {
                        string cnPattern = string.Join(@"\s+(?:a|an|the)?\s*", matchCn.Select(Regex.Escape));
                        cnPattern = $@"\b{cnPattern}\b";
                        cnRe = new Regex(cnPattern, RegexOptions.IgnoreCase);
                    }
                }
                else
                {
                    re = new Regex(key, RegexOptions.IgnoreCase);
                    fullWidthRe = new Regex(fullWidthKey, RegexOptions.IgnoreCase);
                    if (cnKeyHasHan)
                    {
                        cnRe = new Regex(cnKey, RegexOptions.IgnoreCase);
                    }
                }
            }

            foreach (Dictionary<int, StringResult> dict in dicts)
            {
                foreach (KeyValuePair<int, StringResult> kv in dict)
                {
                    if (exact)
                    {
                        // 傳統的完全一致＋假名正規化後的一致。
                        if (kv.Key.ToString() == key || kv.Value.Name == key || Translator.FullWidthKatakana(kv.Value.Name) == fullWidthKey)
                        {
                            yield return kv;
                        }
                        // 簡體字／繁體字的相互一致(僅在查詢含漢字時)。
                        else if (cnKeyHasHan && !string.IsNullOrEmpty(kv.Value.Name) && Translator.NormalizeForSearch(kv.Value.Name) == cnKey)
                        {
                            yield return kv;
                        }
                    }
                    else if (isRegex)
                    {
                        if (re.IsMatch(kv.Key.ToString())
                            || (!string.IsNullOrEmpty(kv.Value.Name) && re.IsMatch(kv.Value.Name))
                            || (!string.IsNullOrEmpty(kv.Value.Name) && fullWidthRe.IsMatch(Translator.FullWidthKatakana(kv.Value.Name))))
                        {
                            yield return kv;
                        }
                        // 簡體字／繁體字的相互一致。
                        else if (cnRe != null && !string.IsNullOrEmpty(kv.Value.Name) && cnRe.IsMatch(Translator.NormalizeForSearch(kv.Value.Name)))
                        {
                            yield return kv;
                        }
                    }
                    else
                    {
                        string id = kv.Key.ToString();
                        bool r = true;
                        foreach (string str in (match.Concat(match2).ToArray()))
                        {
                            if (!(id.Contains(str) || (!string.IsNullOrEmpty(kv.Value.Name) && kv.Value.Name.Contains(str)) || (!string.IsNullOrEmpty(kv.Value.Name) && Translator.FullWidthKatakana(kv.Value.Name).Contains(str))))
                            {
                                r = false;
                                break;
                            }
                        }

                        // 簡體字／繁體字的相互一致(AND)。僅在查詢含漢字時判定。
                        if (!r && cnKeyHasHan && !string.IsNullOrEmpty(kv.Value.Name) && matchCn.Length > 0)
                        {
                            r = true;
                            string nameCn = Translator.NormalizeForSearch(kv.Value.Name);
                            foreach (string str in matchCn)
                            {
                                if (!nameCn.Contains(str))
                                {
                                    r = false;
                                    break;
                                }
                            }
                        }

                        if (r)
                        {
                            yield return kv;
                        }
                    }
                }
            }
        }

        private void textBoxItemSearchString_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (!buttonItemSearchString.Enabled) return;
                buttonItemSearchString_Click(buttonItemSearchString, EventArgs.Empty);
            }
        }

        private void buttonItemSelectStringWz_Click(object sender, EventArgs e)
        {
            buttonItemSearchString.Enabled = false;
            Task.Run(() => selectStringWz());
        }

        private async void selectStringWz()
        {
            labelItemStatus.Text = "正在重新載入字串表連結...";
            Wz_File stringWzFile = advTree1.SelectedNode?.AsWzNode()?.FindNodeByPath("String").GetNodeWzFile();
            Wz_File itemWzFile = advTree1.SelectedNode?.AsWzNode()?.FindNodeByPath("Item").GetNodeWzFile();
            Wz_File etcWzFile = advTree1.SelectedNode?.AsWzNode()?.FindNodeByPath("Etc").GetNodeWzFile();
            Wz_File questWzFile = advTree1.SelectedNode?.AsWzNode()?.FindNodeByPath("Quest").GetNodeWzFile();
            if (stringWzFile == null || itemWzFile == null || etcWzFile == null)
            {
                UnityAssetDocument unityDoc = this.FindUnityDocument(advTree1.SelectedNode);
                if (unityDoc != null && unityDoc.StringTables != null)
                {
                    if (this.LoadStringLinker())
                    {
                        labelItemStatus.Text = "字串表連結已被重置。";
                    }
                    else
                    {
                        MessageBoxEx.Show(this, "無法重置字串表連結。", LocalizedString_JP.COMMON_ERROR);
                    }
                    buttonItemSearchString.Enabled = true;
                    return;
                }
                MessageBoxEx.Show(this, "請選擇 Base.wz。", LocalizedString_JP.COMMON_ERROR);
                buttonItemSearchString.Enabled = true;
                return;
            }
            QueryPerformance.Start();
            bool r = stringLinker.Load(stringWzFile, itemWzFile, etcWzFile, questWzFile);
            QueryPerformance.End();
            if (r)
            {
                double ms = (Math.Round(QueryPerformance.GetLastInterval(), 4) * 1000);
                labelItemStatus.Text = "字串表連結已被重置。時間經過：" + ms + "毫秒";
            }
            else
            {
                MessageBoxEx.Show(this, "字串表連結已被重置。", LocalizedString_JP.COMMON_ERROR);
            }
            buttonItemSearchString.Enabled = true;
        }

        private void buttonItemClearStringWz_Click(object sender, EventArgs e)
        {
            stringLinker.Clear();
            labelItemStatus.Text = "字串表連結已被重置。";
        }

        private void buttonItemIgnoreArticles_Click(object sender, EventArgs e)
        {
            ConfigManager.Reload();
            WcR2Config.Default.IgnoreArticles = buttonItemIgnoreArticles.Checked;
            ConfigManager.Save();
        }

        private void buttonItemPatcher_Click(object sender, EventArgs e)
        {
            foreach (Form form in Application.OpenForms)
            {
                if (form is FrmPatcher && !form.IsDisposed)
                {
                    form.Show();
                    form.BringToFront();
                    return;
                }
            }

            FrmPatcher patcher = new FrmPatcher(this);
            var config = WcR2Config.Default;
            var defaultEnc = config?.WzEncoding?.Value ?? 0;
            if (defaultEnc != 0)
            {
                patcher.PatcherNoticeEncoding = Encoding.GetEncoding(defaultEnc);
            }
            patcher.Owner = this;
            patcher.Show();
        }

        private void buttonInstallGame_Click(object sender, EventArgs e)
        {
            int preferredRegion = WcR2Config.Default.PreferredClientRegion;
            string ngmProtocol = "ngm";
            string gameCode = "16785939@bb01";
            switch (preferredRegion)
            {
                default:
                    break;
                case 1:
                    gameCode = "589825";
                    break;
                case 2:
                    gameCode = "589826";
                    break;
                case 3:
                    ngmProtocol = "msul";
                    gameCode = "106690@d811";
                    break;
                case 4:
                    gameCode = "10100";
                    break;
                case 5:
                    gameCode = "59822";
                    break;
                case 6:
                    gameCode = "40600";
                    break;
            }
            switch (preferredRegion)
            {
                case 0:
                case 1:
                case 2:
                case 3:
                    if (!IsUriSchemeRegistered(ngmProtocol))
                    {
                        ngmInstallPrompt(ngmProtocol);
                        return;
                    }
                    else
                    {
                        try
                        {
#if NET6_0_OR_GREATER
                    Process.Start(new ProcessStartInfo
                    {
                        UseShellExecute = true,
                        FileName = ngmProtocol + "://launch/ -mode:install -game:'" + gameCode + "'",
                    });
#else
                            Process.Start(ngmProtocol + "://launch/ -mode:install -game:'" + gameCode + "'");
#endif
                        }
                        catch
                        {
                            ngmInstallPrompt(ngmProtocol);
                        }
                    }
                    return;
                case 4:
                case 5:
                case 6:
                    foreach (Form form in Application.OpenForms)
                    {
                        if (form is FrmGMSDownloader && !form.IsDisposed)
                        {
                            form.Show();
                            form.BringToFront();
                            return;
                        }
                    }
                    FrmGMSDownloader frm = new FrmGMSDownloader(styleManager1.ManagerStyle == eStyle.VisualStudio2012Dark);
                    frm.Owner = this;
                    if (int.TryParse(gameCode, out int x))
                    {
                        frm.gameCode = x;
                    }
                    frm.Show();
                    return;
                default:
                    MessageBoxEx.Show(this, "Not implemented", "注意");
                    break;
            }
        }

        private void buttonGameStart_Click(object sender, EventArgs e)
        {
            int preferredRegion = WcR2Config.Default.PreferredClientRegion;
            string ngmProtocol = "ngm";
            string gameCode = "16785939@bb01";
            switch (preferredRegion)
            {
                default:
                    break;
                case 1:
                    gameCode = "589825";
                    break;
                case 2:
                    gameCode = "589826";
                    break;
                case 3:
                    ngmProtocol = "msul";
                    gameCode = "106690@d811";
                    break;
                case 4:
                    ngmProtocol = "nxl";
                    gameCode = "10100";
                    break;
                case 5:
                    ngmProtocol = "nxl";
                    gameCode = "59822";
                    break;
                case 6:
                    ngmProtocol = "nxl";
                    gameCode = "40600";
                    break;
            }

            if (!IsUriSchemeRegistered(ngmProtocol))
            {
                ngmInstallPrompt(ngmProtocol);
                return;
            }
            else
            {
                try
                {
                    switch (preferredRegion)
                    {
                        case 0:
                        case 1:
                        case 2:
                        case 3:
#if NET6_0_OR_GREATER
                    Process.Start(new ProcessStartInfo
                    {
                        UseShellExecute = true,
                        FileName = ngmProtocol + "://launch/ -mode:launch -game:'" + gameCode + "'",
                    });
#else
                            Process.Start(ngmProtocol + "://launch/ -mode:launch -game:'" + gameCode + "'");
#endif
                            break;
                        case 4:
                        case 5:

#if NET6_0_OR_GREATER
                    Process.Start(new ProcessStartInfo
                    {
                        UseShellExecute = true,
                        FileName = ngmProtocol + "://games/" + gameCode + "?partnerkey=3267",
                    });
#else
                            Process.Start(ngmProtocol + "://games/" + gameCode + "?partnerkey=3267");
#endif
                            break;
                        default:
                            MessageBoxEx.Show(this, "Not implemented", "注意");
                            break;
                    }
                }
                catch
                {
                    ngmInstallPrompt(ngmProtocol);
                }
            }
            return;
        }

        private void buttonItemJMS_Click(object sender, EventArgs e)
        {
            ConfigManager.Reload();
            WcR2Config.Default.PreferredClientRegion = 0;
            this.buttonItemJMS.Checked = true;
            this.buttonItemKMS.Checked = false;
            this.buttonItemKMST.Checked = false;
            this.buttonItemMSN.Checked = false;
            this.buttonItemGMS.Checked = false;
            this.buttonItemGMSC.Checked = false;
            this.buttonItemGMSPTS.Checked = false;
            this.buttonItemCMS.Checked = false;
            ConfigManager.Save();
        }

        private void buttonItemKMS_Click(object sender, EventArgs e)
        {
            ConfigManager.Reload();
            WcR2Config.Default.PreferredClientRegion = 1;
            this.buttonItemJMS.Checked = false;
            this.buttonItemKMS.Checked = true;
            this.buttonItemKMST.Checked = false;
            this.buttonItemMSN.Checked = false;
            this.buttonItemGMS.Checked = false;
            this.buttonItemGMSC.Checked = false;
            this.buttonItemGMSPTS.Checked = false;
            this.buttonItemCMS.Checked = false;
            ConfigManager.Save();
        }

        private void buttonItemKMST_Click(object sender, EventArgs e)
        {
            ConfigManager.Reload();
            WcR2Config.Default.PreferredClientRegion = 2;
            this.buttonItemJMS.Checked = false;
            this.buttonItemKMS.Checked = false;
            this.buttonItemKMST.Checked = true;
            this.buttonItemMSN.Checked = false;
            this.buttonItemGMS.Checked = false;
            this.buttonItemGMSC.Checked = false;
            this.buttonItemGMSPTS.Checked = false;
            this.buttonItemCMS.Checked = false;
            ConfigManager.Save();
        }

        private void buttonItemMSN_Click(object sender, EventArgs e)
        {
            ConfigManager.Reload();
            WcR2Config.Default.PreferredClientRegion = 3;
            this.buttonItemJMS.Checked = false;
            this.buttonItemKMS.Checked = false;
            this.buttonItemKMST.Checked = false;
            this.buttonItemMSN.Checked = true;
            this.buttonItemGMS.Checked = false;
            this.buttonItemGMSC.Checked = false;
            this.buttonItemGMSPTS.Checked = false;
            this.buttonItemCMS.Checked = false;
            ConfigManager.Save();
        }

        private void buttonItemGMS_Click(object sender, EventArgs e)
        {
            ConfigManager.Reload();
            WcR2Config.Default.PreferredClientRegion = 4;
            this.buttonItemJMS.Checked = false;
            this.buttonItemKMS.Checked = false;
            this.buttonItemKMST.Checked = false;
            this.buttonItemMSN.Checked = false;
            this.buttonItemGMS.Checked = true;
            this.buttonItemGMSC.Checked = false;
            this.buttonItemGMSPTS.Checked = false;
            this.buttonItemCMS.Checked = false;
            ConfigManager.Save();
        }

        private void buttonItemGMSC_Click(object sender, EventArgs e)
        {
            ConfigManager.Reload();
            WcR2Config.Default.PreferredClientRegion = 5;
            this.buttonItemJMS.Checked = false;
            this.buttonItemKMS.Checked = false;
            this.buttonItemKMST.Checked = false;
            this.buttonItemMSN.Checked = false;
            this.buttonItemGMS.Checked = false;
            this.buttonItemGMSC.Checked = true;
            this.buttonItemGMSPTS.Checked = false;
            this.buttonItemCMS.Checked = false;
            ConfigManager.Save();
        }

        private void buttonItemGMSPTS_Click(object sender, EventArgs e)
        {
            ConfigManager.Reload();
            WcR2Config.Default.PreferredClientRegion = 6;
            this.buttonItemJMS.Checked = false;
            this.buttonItemKMS.Checked = false;
            this.buttonItemKMST.Checked = false;
            this.buttonItemMSN.Checked = false;
            this.buttonItemGMS.Checked = false;
            this.buttonItemGMSC.Checked = false;
            this.buttonItemGMSPTS.Checked = true;
            this.buttonItemCMS.Checked = false;
            ConfigManager.Save();
        }

        private void buttonItemCMS_Click(object sender, EventArgs e)
        {
            ConfigManager.Reload();
            WcR2Config.Default.PreferredClientRegion = 7;
            this.buttonItemJMS.Checked = false;
            this.buttonItemKMS.Checked = false;
            this.buttonItemKMST.Checked = false;
            this.buttonItemMSN.Checked = false;
            this.buttonItemGMS.Checked = false;
            this.buttonItemGMSC.Checked = false;
            this.buttonItemGMSPTS.Checked = false;
            this.buttonItemCMS.Checked = true;
            ConfigManager.Save();
        }

        private void ngmInstallPrompt(string protocol)
        {
            if (string.IsNullOrEmpty(protocol))
            {
                protocol = "ngm";
            }
            string message = "";
            string url = "";
            switch (protocol)
            {
                default:
                case "ngm":
                    message = "下載或啟動遊戲需要 Nexon Game Manager，但它似乎尚未安裝。 \r\n\r\n是否要下載並安裝？";
                    url = "https://platform.nexon.com/NGM/Bin/Install_NGM.exe";
                    break;
                case "msul":
                    message = "下載或啟動遊戲需要 Nexpace Game Manager，但它似乎尚未安裝。 \r\n\r\n是否要下載並安裝？";
                    url = "https://static.msu.io/ngm/Bin/Install_NGM.exe";
                    break;
                case "gamania":
                    message = "下載或啟動遊戲需要 Gamania Games Manager，但它似乎尚未安裝。 \r\n\r\n是否要下載並安裝？";
                    url = "https://tw.beanfun.com/ggm/index.html";
                    break;
                case "nxl":
                    message = "下載或啟動遊戲需要 Nexon Launcher，但它似乎尚未安裝。 \r\n\r\n是否要下載並安裝？";
                    url = "https://download.nxfs.nexon.com/download-launcher?file=NexonLauncherSetup.exe";
                    break;
            }
            DialogResult ngmresult = MessageBoxEx.Show(message, "確認", MessageBoxButtons.YesNo);
            if (ngmresult == DialogResult.Yes)
            {
#if NET6_0_OR_GREATER
                Process.Start(new ProcessStartInfo
                {
                    UseShellExecute = true,
                    FileName = url,
                });
#else
                Process.Start(url);
                #endif
            }
        }
        #endregion

        #region soundPlayer相关事件
        private void preLoadSound(Wz_Sound sound, string soundName)
        {
            byte[] data = sound.ExtractSound();
            if (data == null || data.Length <= 0)
            {
                return;
            }
            soundPlayer.PreLoad(data);
            labelItemSoundTitle.Text = soundName;

            switch (sound.SoundType)
            {
                case Wz_SoundType.Mp3: soundName += ".mp3"; break;
                case Wz_SoundType.Pcm: soundName += ".wav"; break;
            }
            soundPlayer.PlayingSoundName = soundName;
            labelItemSoundTitle.Tooltip = soundName;
        }

        private void sliderItemSoundTime_ValueChanged(object sender, EventArgs e)
        {
            if (!timerChangeValue)
                soundPlayer.SoundPosition = sliderItemSoundTime.Value;
        }

        private void sliderItemSoundVol_ValueChanged(object sender, EventArgs e)
        {
            soundPlayer.Volume = sliderItemSoundVol.Value;
        }

        private void buttonItemLoadSound_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                List<string> supportExt = new List<string>();
                supportExt.Add("音訊檔案 (*.mp3;*.ogg;*.wav)|*.mp3;*.ogg;*.wav");
                foreach (string ext in this.soundPlayer.GetPluginSupportedExt())
                {
                    supportExt.Add(ext);
                }
                supportExt.Add("全部格式 (*.*)|*.*");

                dlg.Title = "選擇音訊檔案";
                dlg.Filter = string.Join("|", supportExt.ToArray());
                dlg.Multiselect = false;

                if (DialogResult.OK == dlg.ShowDialog())
                {
                    loadCostumSoundFile(dlg.FileName);
                }
            }
        }

        private void buttonItemSoundPlay_Click(object sender, EventArgs e)
        {
            if (soundPlayer.State == PlayState.Playing)
            {
                soundPlayer.Pause();
                buttonItemSoundPlay.Image = WzComparerR2.Properties.Resources.Play;
                //buttonItemSoundPlay.Text = " Play";
            }
            else if (soundPlayer.State == PlayState.Paused)
            {
                soundPlayer.Resume();
                //buttonItemSoundPlay.Text = "Pause";
                buttonItemSoundPlay.Image = WzComparerR2.Properties.Resources.Pause;
            }
            else
            {
                soundPlayer.Play();
                //buttonItemSoundPlay.Text = "Pause";
                buttonItemSoundPlay.Image = WzComparerR2.Properties.Resources.Pause;
            }
        }

        private void buttonItemSoundStop_Click(object sender, EventArgs e)
        {
            soundPlayer.Stop();
            //buttonItemSoundPlay.Text = " Play";
            buttonItemSoundPlay.Image = WzComparerR2.Properties.Resources.Play;
        }

        private void buttonItemSoundSave_Click(object sender, EventArgs e)
        {
            byte[] data = soundPlayer.Data;
            if (data == null)
                return;

            using (SaveFileDialog dlg = new SaveFileDialog())
            {
                dlg.AddExtension = true;
                dlg.Title = "另存為";
                dlg.Filter = "MP3 (*.mp3)|*.mp3|WAV File (*.wav)|*.wav|OGG File (*.ogg)|*.ogg|全部格式 (*.*)|*.*";
                dlg.AddExtension = false;
                dlg.FileName = soundPlayer.PlayingSoundName;
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    FileStream fs = null;
                    try
                    {
                        fs = new FileStream(dlg.FileName, FileMode.Create);
                        fs.Write(data, 0, data.Length);

                        MessageBoxEx.Show("檔案已保存。");
                    }
                    catch (Exception ex)
                    {
                        MessageBoxEx.Show("檔案保存失敗。\r\n\r\n" + ex.ToString(), LocalizedString_JP.COMMON_ERROR);
                    }
                    finally
                    {
                        if (fs != null)
                        {
                            fs.Close();
                        }
                    }
                }
            }
        }

        private void checkBoxItemSoundLoop_CheckedChanged(object sender, CheckBoxChangeEventArgs e)
        {
            soundPlayer.Loop = checkBoxItemSoundLoop.Checked;
        }

        private void soundTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            TimeSpan currentTime = TimeSpan.FromSeconds(soundPlayer.SoundPosition);
            TimeSpan totalTime = TimeSpan.FromSeconds(soundPlayer.SoundLength);
            labelItemSoundTime.Text = string.Format("{0:d2}:{1:d2}:{2:d2}.{3:d3} / {4:d2}:{5:d2}:{6:d2}.{7:d3}",
                currentTime.Hours, currentTime.Minutes, currentTime.Seconds, currentTime.Milliseconds,
                totalTime.Hours, totalTime.Minutes, totalTime.Seconds, totalTime.Milliseconds);
            timerChangeValue = true;
            sliderItemSoundTime.Maximum = (int)totalTime.TotalSeconds;
            sliderItemSoundTime.Value = (int)currentTime.TotalSeconds;
            timerChangeValue = false;
        }

        private void ribbonBar3_DragEnter(object sender, DragEventArgs e)
        {
            string[] types = e.Data.GetFormats();
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Move;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void ribbonBar3_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                loadCostumSoundFile(files[0]);
            }
        }

        private void loadCostumSoundFile(string fileName)
        {
            CustomSoundFile soundFile = new CustomSoundFile(fileName, 0, (int)(new FileInfo(fileName).Length));
            soundPlayer.PreLoad(soundFile);
            soundPlayer.PlayingSoundName = Path.GetFileName(fileName);
            labelItemSoundTitle.Text = "(外部檔案) " + soundPlayer.PlayingSoundName;
            labelItemSoundTitle.Tooltip = fileName;
        }
        #endregion

        #region contextMenuStrip2
        private void tsmi2SaveAs_Click(object sender, EventArgs e)
        {
            object item = advTree3.SelectedNode?.AsWzNode()?.Value;

            if (item == null)
                return;

            if (item is string str)
            {
                SaveFileDialog dlg = new SaveFileDialog();
                dlg.FileName = advTree3.SelectedNode.Text;
                if (!dlg.FileName.Contains("."))
                {
                    dlg.FileName += ".txt";
                }
                dlg.Filter = "全部格式 (*.*)|*.*";
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        File.WriteAllText(dlg.FileName, str);
                        this.labelItemStatus.Text = "Saved";
                    }
                    catch (Exception ex)
                    {
                        MessageBoxEx.Show("Failed to save\r\n" + ex.ToString(), LocalizedString_JP.COMMON_ERROR);
                    }
                }
            }
            else if (item is IMapleStoryBlob blob)
            {
                SaveFileDialog dlg = new SaveFileDialog();
                dlg.FileName = advTree3.SelectedNode.Text;
                if (!dlg.FileName.Contains(".") && blob is Wz_Sound wzSound)
                {
                    switch (wzSound.SoundType)
                    {
                        case Wz_SoundType.Mp3: dlg.FileName += ".mp3"; break;
                        case Wz_SoundType.Pcm: dlg.FileName += ".pcm"; break;
                    }
                }
                dlg.Filter = "全部格式 (*.*)|*.*";
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        byte[] data = new byte[blob.Length];
                        blob.CopyTo(data, 0);
                        using (var f = File.Create(dlg.FileName))
                        {
                            f.Write(data, 0, data.Length);
                            f.Flush();
                        }
                        this.labelItemStatus.Text = "檔案已保存";
                    }
                    catch (Exception ex)
                    {
                        MessageBoxEx.Show("保存失敗\r\n" + ex.ToString(), LocalizedString_JP.COMMON_ERROR);
                    }
                }
            }
            else if (item is Wz_RawData rawData)
            {
                SaveFileDialog dlg = new SaveFileDialog();
                dlg.FileName = advTree3.SelectedNode.Text;
                dlg.Filter = "全部格式 (*.*)|*.*";
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        using (var f = File.Create(dlg.FileName))
                        {
                            rawData.WzFile.FileStream.Seek(rawData.Offset, SeekOrigin.Begin);
                            byte[] buffer = new byte[4096];
                            int bytes = rawData.Length;
                            while (bytes > 0)
                            {
                                int count = rawData.WzFile.FileStream.Read(buffer, 0, Math.Min(buffer.Length, bytes));
                                if (count > 0)
                                {
                                    f.Write(buffer, 0, count);
                                    bytes -= count;
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }
                        this.labelItemStatus.Text = "檔案已保存";
                    }
                    catch (Exception ex)
                    {
                        MessageBoxEx.Show("保存失敗\r\n" + ex.ToString(), LocalizedString_JP.COMMON_ERROR);
                    }
                }
            }
            else if (item is Wz_Png png)
            {
                SaveFileDialog dlg = new SaveFileDialog();
                dlg.Title = "保存原始Canvas數據";
                dlg.FileName = advTree3.SelectedNode.Text + ".bin";
                dlg.Filter = "*.*|*.*";
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        using (var dataReader = png.UnsafeOpenRead())
                        using (var outputFile = dlg.OpenFile())
                        {
                            dataReader.CopyTo(outputFile);
                        }
                        this.labelItemStatus.Text = "檔案已保存";
                    }
                    catch (Exception ex)
                    {
                        MessageBoxEx.Show("保存失敗\r\n" + ex.ToString(), LocalizedString_JP.COMMON_ERROR);
                    }
                }
            }
        }

        private void tsmi2HandleUol_Click(object sender, EventArgs e)
        {
            Wz_Uol uol = advTree3.SelectedNode?.AsWzNode()?.Value as Wz_Uol;
            if (uol == null)
            {
                labelItemStatus.Text = "未選擇UOL節點。";
                return;
            }

            Node uolNode = handleUol(advTree3.SelectedNode, uol.Uol);
            if (uolNode == null)
            {
                labelItemStatus.Text = "找不到目標UOL節點。";
                return;
            }
            else
            {
                advTree3.SelectedNode = uolNode;
            }
        }

        private void tsmi2ExpandAll_Click(object sender, EventArgs e)
        {
            if (advTree3.SelectedNode == null)
                return;
            advTree3.BeginUpdate();
            advTree3.SelectedNode.ExpandAll();
            advTree3.SelectedNode.Expand();
            advTree3.EndUpdate();
        }

        private void tsmi2CollapseAll_Click(object sender, EventArgs e)
        {
            if (advTree3.SelectedNode == null)
                return;
            advTree3.BeginUpdate();
            advTree3.SelectedNode.Collapse();
            advTree3.SelectedNode.CollapseAll();
            advTree3.EndUpdate();
        }

        private void tsmi2ExpandLevel_Click(object sender, EventArgs e)
        {
            if (advTree3.SelectedNode == null)
                return;

            advTree3.BeginUpdate();
            foreach (Node node in getEqualLevelNode(advTree3.SelectedNode))
            {
                node.Expand();
            }
            advTree3.EndUpdate();
        }

        private void tsmi2CollapseLevel_Click(object sender, EventArgs e)
        {
            if (advTree3.SelectedNode == null)
                return;

            advTree3.BeginUpdate();
            foreach (Node node in getEqualLevelNode(advTree3.SelectedNode))
            {
                node.Collapse();
            }
            advTree3.EndUpdate();
        }

        private IEnumerable<Node> getEqualLevelNode(Node currentNode)
        {
            if (currentNode == null)
                yield break;
            int level = currentNode.Level;
            Node parent = currentNode;
            while (parent != null && parent.Parent != null)
            {
                parent = parent.Parent;
            }
            Queue<Node> nodeList = new Queue<Node>();
            nodeList.Enqueue(parent);
            for (int i = 0; i < level; i++)
            {
                int count = nodeList.Count;
                for (int j = 0; j < count; j++)
                {
                    Node node = nodeList.Dequeue();
                    foreach (Node child in node.Nodes)
                        nodeList.Enqueue(child);
                }
            }

            while (nodeList.Count > 0)
            {
                yield return nodeList.Dequeue();
            }
        }

        private void tsmi2ExpandType_Click(object sender, EventArgs e)
        {
            if (advTree3.SelectedNode == null)
                return;

            advTree3.BeginUpdate();
            foreach (Node node in getEqualTypeNode(advTree3.SelectedNode))
            {
                node.Expand();
            }
            advTree3.EndUpdate();
        }

        private void tsmi2CollapseType_Click(object sender, EventArgs e)
        {
            if (advTree3.SelectedNode == null)
                return;

            advTree3.BeginUpdate();
            foreach (Node node in getEqualTypeNode(advTree3.SelectedNode))
            {
                node.Collapse();
            }
            advTree3.EndUpdate();
        }

        private IEnumerable<Node> getEqualTypeNode(Node currentNode)
        {
            if (currentNode == null)
                yield break;
            Type type = currentNode.AsWzNode()?.Value?.GetType();
            Node parent = currentNode;
            while (parent != null && parent.Parent != null)
            {
                parent = parent.Parent;
            }
            Queue<Node> nodeList = new Queue<Node>();
            nodeList.Enqueue(parent);
            while (nodeList.Count > 0)
            {
                int count = nodeList.Count;
                for (int i = 0; i < count; i++)
                {
                    Node node = nodeList.Dequeue();
                    if (node.AsWzNode()?.Value?.GetType() == type)
                    {
                        yield return node;
                    }
                    foreach (Node child in node.Nodes)
                        nodeList.Enqueue(child);
                }
            }
        }

        private void tsmi2Prev_Click(object sender, EventArgs e)
        {
            if (historyNodeList.PrevCount > 0)
            {
                historySelecting = true;
                advTree3.SelectedNode = historyNodeList.MovePrev();
            }
        }

        private void tsmi2Next_Click(object sender, EventArgs e)
        {
            if (historyNodeList.NextCount > 0)
            {
                historySelecting = true;
                advTree3.SelectedNode = historyNodeList.MoveNext();
            }
        }

        private void tsmi2CopyFullPath_Click(object sender, EventArgs e)
        {
            var selectedWzNode = advTree3.SelectedNode.AsWzNode();
            if (selectedWzNode != null)
            {
                string fullPath = selectedWzNode.FullPathToFile.Replace('\\', '/');
                Clipboard.SetText(fullPath);
                ToastNotification.Show(this, "已經複製目前所選節點完整路徑。", 1000, eToastPosition.TopCenter);
            }
        }

        private void contextMenuStrip2_Opening(object sender, CancelEventArgs e)
        {
            var node = advTree3.SelectedNode.AsWzNode();
            tsmi2SaveAs.Visible = false;
            tsmi2HandleUol.Visible = false;
            if (node != null)
            {
                if (node.Value is Wz_Sound || node.Value is Wz_Png || node.Value is string || node.Value is Wz_RawData || node.Value is Wz_Video)
                {
                    tsmi2SaveAs.Visible = true;
                    tsmi2SaveAs.Enabled = true;
                }
                else if (node.Value is Wz_Uol)
                {
                    tsmi2HandleUol.Visible = true;
                }
                else
                {
                    tsmi2SaveAs.Visible = true;
                    tsmi2SaveAs.Enabled = false;
                }
            }
        }
        #endregion

        #region charaSim相关
        private void buttonItemQuickView_Click(object sender, EventArgs e)
        {
            quickView();
        }

        private void advTree1_AfterNodeSelect_2(object sender, AdvTreeNodeEventArgs e)
        {
            lastSelectedTree = advTree1;
            if (buttonItemAutoQuickView.Checked)
            {
                quickView(advTree1.SelectedNode);
            }
        }

        private void advTree2_AfterNodeSelect_2(object sender, AdvTreeNodeEventArgs e)
        {
            lastSelectedTree = advTree2;
            if (buttonItemAutoQuickView.Checked)
            {
                quickView(advTree2.SelectedNode);
            }
        }

        private void quickView()
        {
            if (lastSelectedTree != null)
            {
                quickView(lastSelectedTree.SelectedNode);
            }
        }

        private void quickView(Node node)
        {
            Wz_Node selectedNode = node.AsWzNode();
            if (selectedNode == null)
            {
                return;
            }

            Wz_Image image;

            UnityWzImage unityImage = FindUnityWzImage(selectedNode);
            if (unityImage != null)
            {
                this.quickViewUnity(selectedNode, unityImage);
                return;
            }

            Wz_File wzf = selectedNode.GetNodeWzFile();
            if (wzf == null)
            {
                labelItemStatus.Text = "找不到該節點所屬的WZ檔案。";
                return;
            }

            if (!this.stringLinker.HasValues)
            {
                this.LoadStringLinker();
            }

            object obj = null;
            string fileName = null;

            StringResult sr = new StringResult();
            string altAutoDesc = null;
            var wzfType = wzf.Type; // temp workaround
            // Clear previous record
            tooltipQuickView.NodeName = string.Empty;
            tooltipQuickView.NodeID = 0;
            tooltipQuickView.Desc = string.Empty;
            tooltipQuickView.Pdesc = string.Empty;
            tooltipQuickView.AutoDesc = string.Empty;
            tooltipQuickView.Hdesc = string.Empty;
            tooltipQuickView.DescLeftAlign = string.Empty;
            // temp workaround start
            if (wzfType == Wz_Type.Unknown)
            {
                string[] path = selectedNode.FullPathToFile.Split('\\');
                wzfType = ParseWzTypeManually(path[0]);
            }
            // temp workaround end
            switch (wzfType)
            {
                case Wz_Type.Character:
                    if (!selectedNode.FullPathToFile.Contains(".img")) return;
                    string[] characterNodePath = selectedNode.FullPathToFile.Split('\\');
                    string characterImgStr = characterNodePath.LastOrDefault(part => part.EndsWith(".img")).Replace(".img", String.Empty);
                    if (!Int64.TryParse(characterImgStr, out _)) return; // Ignore Non-numeral img to prevent Auto Preview crash
                    if ((image = selectedNode.GetValue<Wz_Image>()) == null || !image.TryExtract())
                        return;
                    CharaSimLoader.LoadSetItemsIfEmpty();
                    CharaSimLoader.LoadAstraSubWeaponsIfEmpty();
                    CharaSimLoader.LoadExclusiveEquipsIfEmpty();
                    CharaSimLoader.LoadExclusiveEquipsIfEmpty();
                    CharaSimLoader.LoadCommoditiesIfEmpty();
                    CharaSimLoader.LoadMsnMintableItemListIfEmpty();
                    CharaSimLoader.LoadGmsTucBlacklistItemsIfEmpty();
                    if (CharaSimConfig.Default.Misc.LocatePetEquip) CharaSimLoader.LoadPetEquipInfoIfEmpty();
                    if (characterNodePath.Contains("Familiar"))
                    {
                        var familiar = Familiar.CreateFromNode(image.Node, PluginManager.FindWz);
                        obj = familiar;
                        if (stringLinker == null || !stringLinker.StringMob.TryGetValue(familiar.MobID, out sr))
                        {
                            sr = new StringResult();
                            sr.Name = "未知的萌獸";
                        }
                        if (familiar != null)
                        {
                            fileName = "familiar_" + familiar.FamiliarID + "_" + RemoveInvalidFileNameChars(sr.Name) + ".png";
                            tooltipQuickView.NodeID = familiar.FamiliarID;
                        }
                    }
                    else
                    {
                        var gear = Gear.CreateFromNode(image.Node, PluginManager.FindWz);
                        obj = gear;
                        if (stringLinker == null || !stringLinker.StringEqp.TryGetValue(gear.ItemID, out sr))
                        {
                            sr = new StringResult();
                            sr.Name = "未知的装備";
                        }
                        if (gear != null)
                        {
                            fileName = "eqp_" + gear.ItemID + "_" + RemoveInvalidFileNameChars(sr.Name) + ".png";
                            tooltipQuickView.NodeID = gear.ItemID;
                        }
                    }
                    break;
                case Wz_Type.Item:
                    CharaSimLoader.LoadCommoditiesIfEmpty();
                    CharaSimLoader.LoadMsnMintableItemListIfEmpty();
                    if (CharaSimConfig.Default.Misc.LocatePetEquip) CharaSimLoader.LoadPetEquipInfoIfEmpty();
                    Wz_Node itemNode = selectedNode;
                    if (Regex.IsMatch(itemNode.FullPathToFile, @"^Item\\(Cash|Consume|Etc|Install|Cash)\\\d{4,6}.img\\\d+$") || Regex.IsMatch(itemNode.FullPathToFile, @"^Item\\Special\\0910.img\\\d+$"))
                    {
                        var item = Item.CreateFromNode(itemNode, PluginManager.FindWz);
                        obj = item;
                        if (stringLinker == null || !stringLinker.StringItem.TryGetValue(item.ItemID, out sr))
                        {
                            sr = new StringResult();
                            sr.Name = "未知的道具";
                        }
                        if (item != null)
                        {
                            fileName = "item_" + item.ItemID + "_" + RemoveInvalidFileNameChars(sr.Name) + ".png";
                            tooltipQuickView.NodeID = item.ItemID;
                        }
                    }
                    else if (Regex.IsMatch(itemNode.FullPathToFile, @"^Item\\Pet\\\d{7}.img"))
                    {
                        if (CharaSimLoader.LoadedSetItems.Count == 0) //宠物 预读套装
                        {
                            CharaSimLoader.LoadSetItemsIfEmpty();
                        }
                        if ((image = selectedNode.GetValue<Wz_Image>()) == null || !image.TryExtract())
                            return;
                        var item = Item.CreateFromNode(image.Node, PluginManager.FindWz);
                        obj = item;
                        if (stringLinker == null || !stringLinker.StringItem.TryGetValue(item.ItemID, out sr))
                        {
                            sr = new StringResult();
                            sr.Name = "未知的寵物";
                        }
                        if (item != null)
                        {
                            fileName = "pet_" + item.ItemID + "_" + RemoveInvalidFileNameChars(sr.Name) + ".png";
                            tooltipQuickView.NodeID = item.ItemID;
                        }
                    }

                    break;
                case Wz_Type.Skill:
                    Wz_Node skillNode = selectedNode;
                    //模式路径分析
                    if (Regex.IsMatch(skillNode.FullPathToFile, @"^Skill\d*\\Recipe_\d+.img\\\d+$"))
                    {
                        Recipe recipe = Recipe.CreateFromNode(skillNode);
                        obj = recipe;
                        if (stringLinker == null || !stringLinker.StringSkill.TryGetValue(recipe.RecipeID, out sr))
                        {
                            sr = new StringResultSkill();
                            sr.Name = "未知的配方";
                        }
                        if (recipe != null)
                        {
                            fileName = "recipe_" + recipe.RecipeID + "_" + RemoveInvalidFileNameChars(sr.Name) + ".png";
                            tooltipQuickView.NodeID = recipe.RecipeID;
                        }
                    }
                    else if (Regex.IsMatch(skillNode.FullPathToFile, @"^Skill\d*\\\d+.img\\skill\\\d+$"))
                    {
                        Skill skill = Skill.CreateFromNode(skillNode, PluginManager.FindWz, PluginManager.FindWz);
                        if (stringLinker == null || !stringLinker.StringSkill.TryGetValue(skill.SkillID, out sr))
                        {
                            sr = new StringResultSkill();
                            sr.Name = "未知的技能";
                        }
                        if (skill != null)
                        {
                            switch (this.skillDefaultLevel)
                            {
                                case DefaultLevel.Level0: skill.Level = 0; break;
                                case DefaultLevel.Level1: skill.Level = 1; break;
                                case DefaultLevel.LevelMax: skill.Level = skill.MaxLevel; break;
                                case DefaultLevel.LevelMaxWithCO: skill.Level = skill.MaxLevel + 2; break;
                            }
                            obj = skill;
                            fileName = "skill_" + skill.SkillID + "_" + RemoveInvalidFileNameChars(sr.Name) + ".png";
                            tooltipQuickView.NodeID = skill.SkillID;
                        }
                    }
                    else if (Regex.IsMatch(skillNode.FullPathToFile, @"^Skill\\Roguelike\\.+\\(\d+)\.img$"))
                    {
                        if ((image = skillNode.GetValue<Wz_Image>()) == null || !image.TryExtract())
                            return;
                        Skill skill = Skill.CreateFromNode(image.Node, PluginManager.FindWz, PluginManager.FindWz);
                        if (stringLinker == null || !stringLinker.StringRoguelikeSkill.TryGetValue(skill.SkillID, out sr))
                        {
                            sr = new StringResultSkill();
                            sr.Name = "未知的技能";
                        }
                        if (skill != null)
                        {
                            switch (this.skillDefaultLevel)
                            {
                                case DefaultLevel.Level0: skill.Level = 0; break;
                                case DefaultLevel.Level1: skill.Level = 1; break;
                                case DefaultLevel.LevelMax: skill.Level = skill.MaxLevel; break;
                                case DefaultLevel.LevelMaxWithCO: skill.Level = skill.MaxLevel + 2; break;
                            }
                            obj = skill;
                            fileName = "skill_" + skill.SkillID + "_" + RemoveInvalidFileNameChars(sr.Name) + ".png";
                            tooltipQuickView.NodeID = skill.SkillID;
                        }
                    }
                    break;

                case Wz_Type.Map:
                    if ((image = selectedNode.GetValue<Wz_Image>()) == null || !image.TryExtract())
                        return;
                    if (!image.Node.FullPathToFile.StartsWith("Map\\Map\\Map")) return; // Ignore other assets to prevent Auto Preview crash
                    var map = Map.CreateFromNode(image.Node, PluginManager.FindWz);
                    obj = map;
                    if (stringLinker == null || !stringLinker.StringMap.TryGetValue(map.MapID, out sr))
                    {
                        sr = new StringResult();
                        sr.Name = "未知的地圖";
                    }
                    if (map != null)
                    {
                        fileName = "map_" + map.MapID + "_" + RemoveInvalidFileNameChars(sr.Name.Replace(" : ", ":")) + ".png";
                        tooltipQuickView.NodeID = map.MapID;
                    }
                    break;

                case Wz_Type.Mob:
                    if (!selectedNode.FullPathToFile.Contains(".img")) return;
                    string[] mobNodePath = selectedNode.FullPathToFile.Split('\\');
                    string mobImgStr = mobNodePath.LastOrDefault(part => part.EndsWith(".img")).Replace(".img", String.Empty);
                    if (!Int64.TryParse(mobImgStr, out _)) return; // Ignore Non-numeral img to prevent Auto Preview crash
                    if ((image = selectedNode.GetValue<Wz_Image>()) == null || !image.TryExtract())
                        return;
                    var mob = Mob.CreateFromNode(image.Node, PluginManager.FindWz);
                    obj = mob;
                    if (stringLinker == null || !stringLinker.StringMob.TryGetValue(mob.ID, out sr))
                    {
                        sr = new StringResult();
                        sr.Name = "未知的怪物";
                    }
                    if (mob != null)
                    {
                        fileName = "mob_" + mob.ID + "_" + RemoveInvalidFileNameChars(sr.Name) + ".png";
                        tooltipQuickView.NodeID = mob.ID;
                    }
                    break;

                case Wz_Type.Npc:
                    if ((image = selectedNode.GetValue<Wz_Image>()) == null || !image.TryExtract())
                        return;
                    var npc = Npc.CreateFromNode(image.Node, PluginManager.FindWz, getSpineDefaultFunc: this.pictureBoxEx1.GetSpineDefault);
                    obj = npc;
                    if (stringLinker == null || !stringLinker.StringNpc.TryGetValue(npc.ID, out sr))
                    {
                        sr = new StringResult();
                        sr.Name = "未知的NPC";
                    }
                    if (npc != null)
                    {
                        fileName = "npc_" + npc.ID + "_" + RemoveInvalidFileNameChars(sr.Name) + ".png";
                        tooltipQuickView.NodeID = npc.ID;
                    }
                    break;

                case Wz_Type.Quest:
                    Quest quest = null;
                    if (!((image = selectedNode.GetValue<Wz_Image>()) == null || !image.TryExtract()))
                        quest = Quest.CreateFromNode(image.Node, PluginManager.FindWz, PluginManager.FindWz);
                    else if (quest == null)
                    {
                        Wz_Node questInfoNode = selectedNode;
                        var m = Regex.Match(questInfoNode.FullPathToFile, @"^Quest\\QuestInfo.img\\(\d+)$");
                        int questID = 0;
                        if (m.Success && Int32.TryParse(m.Result("$1"), out questID))
                        {
                            quest = Quest.CreateFromNode(questInfoNode, PluginManager.FindWz, PluginManager.FindWz, fromInfoNode: questID);
                        }
                    }
                    obj = quest;
                    if (quest != null)
                    {
                        tooltipQuickView.NodeName = quest.Name;
                        tooltipQuickView.Desc = string.Join("\r\n", quest.Desc);
                        tooltipQuickView.QuestLvmin = quest.Lvmin;
                        if (quest.Desc.Count() == 3)
                        {
                            tooltipQuickView.QuestAvailable = quest.Desc[0];
                            tooltipQuickView.QuestProgress = quest.Desc[1];
                            tooltipQuickView.QuestComplete = quest.Desc[2];
                        }
                        else
                        {
                            tooltipQuickView.QuestAvailable = "";
                            tooltipQuickView.QuestProgress = "";
                            tooltipQuickView.QuestComplete = "";
                        }
                        if (quest.Category.Count() == 2)
                        {
                            tooltipQuickView.QuestCategory = "" + quest.Category[0] + "-" + quest.Category[1];
                        }
                        else
                        {
                            tooltipQuickView.QuestCategory = "0-0";
                        }
                        tooltipQuickView.Pdesc = quest.DemandBase;
                        tooltipQuickView.Hdesc = quest.DemandSummary;
                        tooltipQuickView.AutoDesc = quest.PlaceSummary;
                        tooltipQuickView.DescLeftAlign = quest.Summary;
                        tooltipQuickView.NodeID = quest.ID;
                        fileName = "quest_" + quest.ID + "_" + RemoveInvalidFileNameChars(quest.Name) + ".png";
                        quest.State = tooltipQuickView.QuestRender.DefaultState;
                    }
                    break;

                case Wz_Type.Etc:
                    CharaSimLoader.LoadSetItemsIfEmpty();
                    Wz_Node setItemNode = selectedNode;
                    if (Regex.IsMatch(setItemNode.FullPathToFile, @"^Etc\\SetItemInfo.img\\-?\d+$"))
                    {
                        SetItem setItem;
                        if (!CharaSimLoader.LoadedSetItems.TryGetValue(Convert.ToInt32(selectedNode.Text), out setItem))
                            return;
                        obj = setItem;
                        if (stringLinker == null || !stringLinker.StringSetItem.TryGetValue(setItem.SetItemID, out sr))
                        {
                            sr = new StringResult();
                            sr.Name = "未知的套組";
                        }
                        if (setItem != null)
                        {
                            fileName = "set_" + setItem.SetItemID + "_" + RemoveInvalidFileNameChars(sr.Name) + ".png";
                            tooltipQuickView.NodeID = setItem.SetItemID;
                        }
                    }
                    else if (Regex.IsMatch(selectedNode.FullPathToFile, @"^Etc\\Achievement\\AchievementData\\(\d+).img$"))
                    {
                        if ((image = selectedNode.GetValue<Wz_Image>()) == null || !image.TryExtract())
                            return;
                        Achievement achievement = Achievement.CreateFromNode(image.Node, PluginManager.FindWz, PluginManager.FindWz);
                        if (stringLinker == null || !stringLinker.StringAchievement.TryGetValue(achievement.ID, out sr))
                        {
                            sr = new StringResult();
                            sr.Name = "未知的成就";
                        }
                        obj = achievement;
                        if (achievement != null)
                        {
                            fileName = "achievement_" + achievement.ID + "_" + sr.Name + ".png";
                            altAutoDesc = string.Join("\r\n", achievement.Missions);
                            tooltipQuickView.NodeID = achievement.ID;
                        }
                    }
                    else if (Regex.IsMatch(selectedNode.FullPathToFile, @"^Etc\\GuildCastle.img\\ResearchList\\(Guild|Personal)\\(\d+)$"))
                    {
                        Skill skill = Skill.CreateFromNode(selectedNode, PluginManager.FindWz, PluginManager.FindWz);
                        switch (skill.GuildCastleResearchType)
                        {
                            case 0:
                                if (stringLinker == null || !stringLinker.StringGuildCastleGuildResearch.TryGetValue(skill.SkillID, out sr))
                                {
                                    sr = new StringResultSkill();
                                    sr.Name = "未知的研究";
                                }
                                break;
                            case 1:
                                if (stringLinker == null || !stringLinker.StringGuildCastlePersonalResearch.TryGetValue(skill.SkillID, out sr))
                                {
                                    sr = new StringResultSkill();
                                    sr.Name = "未知的研究";
                                }
                                break;
                        }
                        if (skill != null)
                        {
                            switch (this.skillDefaultLevel)
                            {
                                case DefaultLevel.Level0: skill.Level = 0; break;
                                case DefaultLevel.Level1: skill.Level = 1; break;
                                case DefaultLevel.LevelMax: skill.Level = skill.MaxLevel; break;
                                case DefaultLevel.LevelMaxWithCO: skill.Level = skill.MaxLevel + 2; break;
                            }
                            obj = skill;
                            fileName = "guildresearch_" + skill.SkillID + "_" + RemoveInvalidFileNameChars(sr.Name) + ".png";
                            tooltipQuickView.NodeID = skill.SkillID;
                        }
                    }
                    break;
            }
            tooltipQuickView.TargetItem = obj;
            tooltipQuickView.ImageFileName = fileName;
            tooltipQuickView.Refresh();
            tooltipQuickView.HideOnHover = false;
            if (tooltipQuickView.TargetItem != null) tooltipQuickView.Show();
            if (Translator.IsTranslateEnabled) tooltipQuickView.QuickRefresh();

            StringResult waSr = new StringResult();
            StringResult mbSr = new StringResult();
            StringBuilder npcQuoteSb = new StringBuilder();
            if (tooltipQuickView.TargetItem != null)
            {
                switch (tooltipQuickView.TargetItem)
                {
                    case Mob item:
                        if (CharaSimConfig.Default.Misc.EnableWorldArchive)
                        {
                            if (stringLinker == null || !stringLinker.StringWorldArchiveMob.TryGetValue(item.ID, out waSr))
                            {
                                waSr = new StringResult();
                            }
                        }
                        if (CharaSimConfig.Default.Mob.EnableMonsterBook)
                        {
                            if (stringLinker == null || !stringLinker.StringMonsterBook.TryGetValue(item.ID, out mbSr))
                            {
                                mbSr = new StringResult();
                            }
                        }
                        //item.Dispose();
                        break;
                    case Npc item:
                        if (CharaSimConfig.Default.Misc.EnableWorldArchive)
                        {
                            if (stringLinker == null || !stringLinker.StringWorldArchiveNpc.TryGetValue(item.ID, out waSr))
                            {
                                waSr = new StringResult();
                            }
                            if (CharaSimConfig.Default.Npc.ShowNpcQuotes)
                            {
                                NpcQuote quote = NpcQuote.CreateFromNode(PluginManager.FindWz($@"String\Npc.img\{item.ID}"), PluginManager.FindWz, stringLinker);
                                if (quote != null)
                                {
                                    foreach (var kvp in quote.NQuote)
                                        npcQuoteSb.AppendLine($"n{kvp.Key}: {kvp.Value}");
                                    foreach (var kvp in quote.FQuote)
                                        npcQuoteSb.AppendLine($"f{kvp.Key}: {kvp.Value}");
                                    foreach (var kvp in quote.WQuote)
                                        npcQuoteSb.AppendLine($"w{kvp.Key}: {kvp.Value}");
                                    foreach (var kvp in quote.DQuote)
                                        npcQuoteSb.AppendLine($"d{kvp.Key}: {kvp.Value}");
                                    foreach (var kvp in quote.SpecialQuote)
                                        npcQuoteSb.AppendLine($"s{kvp.Key}: {kvp.Value}");
                                }
                            }
                        }
                        //item.Dispose();
                        break;
                    case Quest item:
                        //item.Dispose();
                        break;
                }
                if (wzf.Type is not Wz_Type.Quest)
                {
                    tooltipQuickView.NodeName = sr.Name;
                    tooltipQuickView.Desc = sr.Desc ?? mbSr.Desc;
                    tooltipQuickView.Pdesc = sr.Pdesc ?? waSr.Desc;
                    tooltipQuickView.AutoDesc = altAutoDesc ?? sr.AutoDesc ?? npcQuoteSb.ToString();
                    tooltipQuickView.Hdesc = sr["h"];
                    tooltipQuickView.DescLeftAlign = sr["desc_leftalign"];
                }
            }
        }

        /// <summary>
        /// 往上找出節點所屬的 Unity WZJS img；非 Unity 節點回傳 null。
        /// </summary>
        private static UnityWzImage FindUnityWzImage(Wz_Node node)        {
            while (node != null)
            {
                if (node.Value is UnityWzImage unityImage)
                {
                    return unityImage;
                }
                node = node.ParentNode;
            }
            return null;
        }

        /// <summary>
        /// Unity 資產的快速預覽，支援道具（Item）、裝備（Character）、怪物（Mob）
        /// 與 NPC（Npc）分類。
        /// <para>
        /// Classic 的 Item 資產像傳統 WZ 一樣分組：群組 img 是
        /// <c>Item/Consume/0200</c>，道具本身則是 img 內的子節點
        /// <c>0200/02000000</c>。因此要往上找到「父節點是群組 img」的節點，
        /// 而不是直接用 img 根。
        /// </para>
        /// <para>
        /// 裝備 / 怪物 / NPC 都是一個 id 一個 img（如 <c>Character/Cap/01002000</c>、
        /// <c>Mob/0100100</c>、<c>Npc/1010100</c>），選到該 img 即顯示 tooltip。
        /// </para>
        /// <para>其他分類待補；無法顯示時直接返回，保留目前的 tooltip。</para>
        /// </summary>
        private void quickViewUnity(Wz_Node selectedNode, UnityWzImage unityImage)
        {
            UnityStringCategory category = GetUnityImageCategory(unityImage.Name);
            if (category == UnityStringCategory.None)
            {
                // 尚未支援的分類，保留目前的 tooltip。
                return;
            }

            if (category == UnityStringCategory.Skill)
            {
                // 技能是 img 內的 skill/<id> 節點（如 Skill/100/skill/1001002）。
                this.quickViewUnitySkill(selectedNode);
                return;
            }

            if (category == UnityStringCategory.Quest)
            {
                // 任務是 Quest/QuestInfo img 內的 <id> 節點。
                this.quickViewUnityQuest(selectedNode, unityImage);
                return;
            }

            if (category != UnityStringCategory.Item)
            {
                // img 本身就是該對象；選到 img 內的子節點時保留目前的 tooltip。
                if (!(selectedNode.Value is UnityWzImage))
                {
                    return;
                }

                // advTree1 的節點是目錄索引（不含子樹），tooltip 需要展開後的 WZJS 內容。
                Wz_Node imageNode;
                try
                {
                    imageNode = unityImage.AsWzNode();
                }
                catch (Exception ex)
                {
                    labelItemStatus.Text = "ツールチップの生成に失敗しました：" + ex.Message;
                    return;
                }

                switch (category)
                {
                    case UnityStringCategory.Equip:
                        this.quickViewUnityGear(imageNode);
                        break;
                    case UnityStringCategory.Mob:
                        this.quickViewUnityMob(imageNode);
                        break;
                    case UnityStringCategory.Npc:
                        this.quickViewUnityNpc(imageNode);
                        break;
                    case UnityStringCategory.Map:
                        this.quickViewUnityMap(imageNode, unityImage);
                        break;
                }
                return;
            }

            // 道具有兩種形狀：群組 img（Item/Consume/0200，道具是 img 內的子節點）與
            // 一 id 一 img（Item/Pet/5002584，img 本身就是道具）。
            Wz_Node itemNode;
            if (selectedNode.Value is UnityWzImage && IsUnityPerItemImageKey(unityImage.Name))
            {
                try
                {
                    itemNode = unityImage.AsWzNode();
                }
                catch (Exception ex)
                {
                    labelItemStatus.Text = "ツールチップの生成に失敗しました：" + ex.Message;
                    return;
                }
            }
            else
            {
                // 往上找道具節點：父節點是群組 img 的那個節點。
                itemNode = selectedNode;
                while (itemNode.ParentNode != null && !(itemNode.ParentNode.Value is UnityWzImage))
                {
                    itemNode = itemNode.ParentNode;
                }
                int itemId;
                if (itemNode.ParentNode == null
                    || !(itemNode.ParentNode.Value is UnityWzImage)
                    || !int.TryParse(itemNode.Text, out itemId))
                {
                    // 選到群組 img 或目錄本身，沒有具體道具可顯示。
                    return;
                }
            }

            if (!this.stringLinker.HasValues)
            {
                this.LoadStringLinker();
            }

            Item item = null;
            StringResult sr = null;
            try
            {
                item = Item.CreateFromNode(itemNode, PluginManager.FindWz);
                if (item != null && (stringLinker == null
                    || !(stringLinker.StringItem.TryGetValue(item.ItemID, out sr)
                        || stringLinker.StringEqp.TryGetValue(item.ItemID, out sr))))
                {
                    sr = new StringResult();
                    sr.Name = "未知のアイテム";
                }
            }
            catch (Exception ex)
            {
                labelItemStatus.Text = "ツールチップの生成に失敗しました：" + ex.Message;
            }

            if (item == null)
            {
                return;
            }

            this.resetUnityTooltip();
            tooltipQuickView.NodeID = item.ItemID;
            this.applyUnityStringToTooltip(sr, null, null);
            this.showUnityTooltip(
                item, "item_" + item.ItemID + "_" + RemoveInvalidFileNameChars(sr.Name) + ".png");
        }

        /// <summary>
        /// 判斷 Item 的 img 是否「一個 id 一個 img」（<c>Item/Pet/5002584</c>）。
        /// 群組 img（<c>Item/Consume/0200</c>）的末段是 4 碼群組編號，不是道具 id。
        /// </summary>
        private static bool IsUnityPerItemImageKey(string imageName)
        {
            if (string.IsNullOrEmpty(imageName))
            {
                return false;
            }
            string[] keyParts = imageName.Split('/');
            if (keyParts.Length != 3 || !string.Equals(keyParts[0], "Item", StringComparison.Ordinal))
            {
                return false;
            }
            string id = keyParts[2];
            int ignore;
            return (id.Length == 7 || id.Length == 8) && int.TryParse(id, out ignore);
        }

        /// <summary>
        /// 判斷 img 索引鍵屬於哪個分類（供 tooltip 使用）：
        /// <c>Item/&lt;分類&gt;/&lt;群組&gt;</c>、<c>Character/&lt;分類&gt;/&lt;8 碼 id&gt;</c>、
        /// <c>Mob/…/&lt;7 碼 id&gt;</c>、<c>Npc/&lt;7 碼 id&gt;</c>。
        /// </summary>
        private static UnityStringCategory GetUnityImageCategory(string imageName)
        {
            if (string.IsNullOrEmpty(imageName))
            {
                return UnityStringCategory.None;
            }
            string[] keyParts = imageName.Split('/');
            if (keyParts.Length < 2)
            {
                return UnityStringCategory.None;
            }

            int id;
            switch (keyParts[0])
            {
                case "Item":
                    // 群組 img（Item/Consume/0200），道具是 img 內的子節點。
                    return keyParts.Length >= 3 ? UnityStringCategory.Item : UnityStringCategory.None;
                case "Character":
                    // 一個 id 一個 img；Familiar 另有專用 tooltip。
                    return keyParts.Length == 3
                        && !string.Equals(keyParts[1], "Familiar", StringComparison.Ordinal)
                        && int.TryParse(keyParts[2], out id)
                        ? UnityStringCategory.Equip : UnityStringCategory.None;
                case "Mob":
                    return int.TryParse(keyParts[keyParts.Length - 1], out id)
                        ? UnityStringCategory.Mob : UnityStringCategory.None;
                case "Npc":
                    return int.TryParse(keyParts[keyParts.Length - 1], out id)
                        ? UnityStringCategory.Npc : UnityStringCategory.None;
                case "Skill":
                    // 技能 img 是 Skill/<職業>，技能本身是 img 內的 skill/<id> 節點。
                    return UnityStringCategory.Skill;
                case "Map":
                    // 地圖 img 是 Map/Map/Map<首位數字>/<9 碼 id>。
                    return keyParts.Length >= 4
                        && keyParts[keyParts.Length - 1].Length == 9
                        && int.TryParse(keyParts[keyParts.Length - 1], out id)
                        ? UnityStringCategory.Map : UnityStringCategory.None;
                case "Quest":
                    // 任務 img 是 Quest/QuestInfo，任務本身是 img 內的 <id> 節點。
                    return keyParts.Length == 2
                        && string.Equals(keyParts[1], "QuestInfo", StringComparison.OrdinalIgnoreCase)
                        ? UnityStringCategory.Quest : UnityStringCategory.None;
                default:
                    return UnityStringCategory.None;
            }
        }

        /// <summary>以字串表內容補上 tooltip 的標題與說明欄位。</summary>
        private void applyUnityStringToTooltip(StringResult sr, StringResult monsterBook, StringResult worldArchive)
        {
            tooltipQuickView.NodeName = sr.Name;
            tooltipQuickView.Desc = sr.Desc ?? monsterBook?.Desc;
            tooltipQuickView.Pdesc = sr.Pdesc ?? worldArchive?.Desc;
            tooltipQuickView.AutoDesc = sr.AutoDesc;
            tooltipQuickView.Hdesc = sr["h"];
            tooltipQuickView.DescLeftAlign = sr["desc_leftalign"];
        }

        /// <summary>清除上一個 Unity tooltip 的內容。</summary>
        private void resetUnityTooltip()
        {
            tooltipQuickView.TargetItem = null;
            tooltipQuickView.NodeName = string.Empty;
            tooltipQuickView.NodeID = 0;
            tooltipQuickView.Desc = string.Empty;
            tooltipQuickView.Pdesc = string.Empty;
            tooltipQuickView.AutoDesc = string.Empty;
            tooltipQuickView.Hdesc = string.Empty;
            tooltipQuickView.DescLeftAlign = string.Empty;
        }

        private void showUnityTooltip(object obj, string fileName)
        {
            tooltipQuickView.TargetItem = obj;
            tooltipQuickView.ImageFileName = fileName;
            tooltipQuickView.Refresh();
            tooltipQuickView.HideOnHover = false;
            tooltipQuickView.Show();
            if (Translator.IsTranslateEnabled) tooltipQuickView.QuickRefresh();
        }

        /// <summary>
        /// Unity 裝備（<c>Character/&lt;分類&gt;/&lt;8 碼 id&gt;</c>）的快速預覽。
        /// </summary>
        private void quickViewUnityGear(Wz_Node imageNode)
        {
            if (!this.stringLinker.HasValues)
            {
                this.LoadStringLinker();
            }

            CharaSimLoader.LoadSetItemsIfEmpty();
            CharaSimLoader.LoadAstraSubWeaponsIfEmpty();
            CharaSimLoader.LoadExclusiveEquipsIfEmpty();
            CharaSimLoader.LoadCommoditiesIfEmpty();
            CharaSimLoader.LoadMsnMintableItemListIfEmpty();
            CharaSimLoader.LoadGmsTucBlacklistItemsIfEmpty();
            if (CharaSimConfig.Default.Misc.LocatePetEquip) CharaSimLoader.LoadPetEquipInfoIfEmpty();

            Gear gear = null;
            StringResult sr = null;
            try
            {
                gear = Gear.CreateFromNode(imageNode, PluginManager.FindWz);
                if (gear != null && stringLinker != null)
                {
                    stringLinker.StringEqp.TryGetValue(gear.ItemID, out sr);
                }
            }
            catch (Exception ex)
            {
                labelItemStatus.Text = "ツールチップの生成に失敗しました：" + ex.Message;
            }

            if (gear == null)
            {
                return;
            }

            this.resetUnityTooltip();
            tooltipQuickView.NodeID = gear.ItemID;
            this.showUnityTooltip(
                gear,
                "eqp_" + gear.ItemID + "_" + RemoveInvalidFileNameChars(sr?.Name ?? "item") + ".png");
        }

        /// <summary>Unity 怪物（<c>Mob/&lt;7 碼 id&gt;</c>）的快速預覽。</summary>
        private void quickViewUnityMob(Wz_Node imageNode)
        {
            if (!this.stringLinker.HasValues)
            {
                this.LoadStringLinker();
            }

            Mob mob = null;
            StringResult sr = null;
            StringResult mbSr = null;
            StringResult waSr = null;
            try
            {
                mob = Mob.CreateFromNode(imageNode, PluginManager.FindWz);
                if (mob != null)
                {
                    if (stringLinker == null || !stringLinker.StringMob.TryGetValue(mob.ID, out sr))
                    {
                        sr = new StringResult();
                        sr.Name = "未知のモンスター";
                    }

                    if (stringLinker != null && CharaSimConfig.Default.Mob.EnableMonsterBook)
                    {
                        stringLinker.StringMonsterBook.TryGetValue(mob.ID, out mbSr);
                    }

                    if (stringLinker != null && CharaSimConfig.Default.Misc.EnableWorldArchive)
                    {
                        stringLinker.StringWorldArchiveMob.TryGetValue(mob.ID, out waSr);
                    }
                }
            }
            catch (Exception ex)
            {
                labelItemStatus.Text = "ツールチップの生成に失敗しました：" + ex.Message;
            }

            if (mob == null)
            {
                return;
            }

            this.resetUnityTooltip();
            tooltipQuickView.NodeID = mob.ID;
            this.applyUnityStringToTooltip(sr, mbSr, waSr);
            this.showUnityTooltip(
                mob, "mob_" + mob.ID + "_" + RemoveInvalidFileNameChars(sr.Name) + ".png");
        }

        /// <summary>Unity NPC（<c>Npc/&lt;7 碼 id&gt;</c>）的快速預覽。</summary>
        private void quickViewUnityNpc(Wz_Node imageNode)
        {
            if (!this.stringLinker.HasValues)
            {
                this.LoadStringLinker();
            }

            Npc npc = null;
            StringResult sr = null;
            StringResult waSr = null;
            try
            {
                npc = Npc.CreateFromNode(
                    imageNode, PluginManager.FindWz,
                    getSpineDefaultFunc: this.pictureBoxEx1.GetSpineDefault);
                if (npc != null)
                {
                    if (stringLinker == null || !stringLinker.StringNpc.TryGetValue(npc.ID, out sr))
                    {
                        sr = new StringResult();
                        sr.Name = "未知のNPC";
                    }

                    if (stringLinker != null && CharaSimConfig.Default.Misc.EnableWorldArchive)
                    {
                        stringLinker.StringWorldArchiveNpc.TryGetValue(npc.ID, out waSr);
                    }
                }
            }
            catch (Exception ex)
            {
                labelItemStatus.Text = "ツールチップの生成に失敗しました：" + ex.Message;
            }

            if (npc == null)
            {
                return;
            }

            this.resetUnityTooltip();
            tooltipQuickView.NodeID = npc.ID;
            this.applyUnityStringToTooltip(sr, null, waSr);
            this.showUnityTooltip(
                npc, "npc_" + npc.ID + "_" + RemoveInvalidFileNameChars(sr.Name) + ".png");
        }

        /// <summary>
        /// Unity 資料集節點對應的 <see cref="Wz_Type"/>（目錄 → 型別）；非 Unity 節點回 null。
        /// </summary>
        private Wz_Type? ResolveUnityWzType(Wz_Node node)
        {
            if (node == null)
            {
                return null;
            }

            // img 展開樹內的節點：以所屬 img 的索引鍵（Character/Cap/01002000）判斷。
            for (Wz_Node cursor = node; cursor != null; cursor = cursor.ParentNode)
            {
                if (cursor.Value is UnityWzImage image)
                {
                    return ClassicWzTypeMap.TryGetWzType(image.Name);
                }
                if (cursor.Value is UnityJsonAsset json)
                {
                    return ClassicWzTypeMap.TryGetWzType(json.ImageKey);
                }
            }

            // 目錄索引樹：以資料集根節點下一層的目錄名稱判斷。
            if (this.openedUnitySources == null || this.openedUnitySources.Count == 0)
            {
                return null;
            }
            Wz_Node root = node;
            while (root.ParentNode != null)
            {
                root = root.ParentNode;
            }
            foreach (UnityAssetDocument document in this.openedUnitySources.Values)
            {
                if (!ReferenceEquals(document.RootNode, root))
                {
                    continue;
                }
                Wz_Node category = node;
                while (category.ParentNode != null && !ReferenceEquals(category.ParentNode, root))
                {
                    category = category.ParentNode;
                }
                return ClassicWzTypeMap.TryGetWzType(category.Text);
            }
            return null;
        }

        /// <summary>
        /// 以 WZ 風格路徑在已開啟的 Unity 資料集內找節點（WZ 查不到時的備援）。
        /// </summary>
        private Wz_Node FindUnityNodeByPath(string wzPath)        {
            if (this.openedUnitySources == null || this.openedUnitySources.Count == 0
                || string.IsNullOrEmpty(wzPath))
            {
                return null;
            }
            foreach (UnityAssetDocument document in this.openedUnitySources.Values)
            {
                try
                {
                    Wz_Node node = document.Source.FindNode(wzPath);
                    if (node != null)
                    {
                        return node;
                    }
                }
                catch (Exception ex) when (ex is UnityException || ex is ClassicAssetException)
                {
                    // 單一資產解碼失敗不影響其他資料集。
                }
            }
            return null;
        }

        /// <summary>Unity 角色動作 img 的索引鍵（各動作的影格延遲來源）。</summary>
        private const string UnityCharacterImageKey = "Character/00002000";

        /// <summary>
        /// Unity 資產的動作延遲：<c>Character/00002000/&lt;action&gt;/&lt;frame&gt;/delay</c>
        /// 的絕對值加總；沒有對應資料時回傳 null（讓 tooltip 顯示 0）。
        /// </summary>
        private int? ResolveUnityActionDelay(string actionName)
        {
            if (this.openedUnitySources == null || string.IsNullOrEmpty(actionName))
            {
                return null;
            }

            foreach (UnityAssetDocument document in this.openedUnitySources.Values)
            {
                int? delay;
                try
                {
                    delay = document.Source.GetActionDelay(UnityCharacterImageKey, actionName);
                }
                catch (Exception ex) when (ex is UnityException || ex is ClassicAssetException)
                {
                    continue;
                }
                if (delay.HasValue)
                {
                    return delay;
                }
            }
            return null;
        }

        /// <summary>
        /// Unity 地圖（<c>Map/Map/Map&lt;N&gt;/&lt;9 碼 id&gt;</c>）的快速預覽（含 miniMap 縮圖）。
        /// </summary>
        private void quickViewUnityMap(Wz_Node imageNode, UnityWzImage unityImage)
        {
            if (!this.stringLinker.HasValues)
            {
                this.LoadStringLinker();
            }

            Map map = null;
            StringResult sr = null;
            try
            {
                // Map.CreateFromNode 以 WZ 風格路徑查 link / NPC / 怪物等關聯節點，
                // 這裡先查 Unity 資料集，找不到才退回 WZ。
                GlobalFindNodeFunction finder = path =>
                    unityImage.FindNode(path) ?? PluginManager.FindWz(path);
                map = Map.CreateFromNode(imageNode, finder);
                if (map != null && (stringLinker == null
                    || !stringLinker.StringMap.TryGetValue(map.MapID, out sr)))
                {
                    sr = new StringResult();
                    sr.Name = "未知のマップ";
                }
            }
            catch (Exception ex)
            {
                labelItemStatus.Text = "ツールチップの生成に失敗しました：" + ex.Message;
            }

            if (map == null)
            {
                return;
            }

            this.resetUnityTooltip();
            tooltipQuickView.NodeID = map.MapID;
            this.applyUnityStringToTooltip(sr, null, null);
            this.showUnityTooltip(
                map, "map_" + map.MapID + "_"
                    + RemoveInvalidFileNameChars((sr.Name ?? "unknown").Replace(" : ", ":")) + ".png");
        }

        /// <summary>
        /// Unity 任務（<c>Quest/QuestInfo/&lt;id&gt;</c>）的快速預覽。
        /// <para>
        /// Classic 把任務資料拆成 <c>Quest/QuestInfo</c>、<c>Quest/Check</c>、
        /// <c>Quest/Act</c>、<c>Quest/Say</c> 四個 img，文字放在
        /// <c>String/&lt;locale&gt;/QuestData</c>。這裡把它們組成傳統 WZ 的
        /// <c>Quest/QuestInfo.img/&lt;id&gt;</c> 形狀後交給 <see cref="Quest.CreateFromNode"/>。
        /// </para>
        /// </summary>
        private void quickViewUnityQuest(Wz_Node questIdNode, UnityWzImage unityImage)
        {
            int questId;
            if (questIdNode.ParentNode == null
                || !(questIdNode.ParentNode.Value is UnityWzImage)
                || !int.TryParse(questIdNode.Text, out questId))
            {
                // 選到 QuestInfo img 本身時沒有任務可顯示。
                return;
            }

            if (!this.stringLinker.HasValues)
            {
                this.LoadStringLinker();
            }

            Quest quest = null;
            try
            {
                Wz_Node questImg = this.BuildUnityQuestImage(questIdNode, questId, unityImage);
                GlobalFindNodeFunction finder = path =>
                    unityImage.FindNode(path) ?? PluginManager.FindWz(path);
                GlobalFindNodeFunction2 finder2 = (path, wzf) => finder(path);
                quest = Quest.CreateFromNode(questImg, finder, finder2);
            }
            catch (Exception ex)
            {
                labelItemStatus.Text = "ツールチップの生成に失敗しました：" + ex.Message;
            }

            if (quest == null)
            {
                return;
            }

            this.resetUnityTooltip();
            tooltipQuickView.NodeName = quest.Name;
            tooltipQuickView.Desc = string.Join("\r\n", quest.Desc);
            tooltipQuickView.QuestLvmin = quest.Lvmin;
            if (quest.Desc.Count() == 3)
            {
                tooltipQuickView.QuestAvailable = quest.Desc[0];
                tooltipQuickView.QuestProgress = quest.Desc[1];
                tooltipQuickView.QuestComplete = quest.Desc[2];
            }
            else
            {
                tooltipQuickView.QuestAvailable = "";
                tooltipQuickView.QuestProgress = "";
                tooltipQuickView.QuestComplete = "";
            }
            tooltipQuickView.QuestCategory = quest.Category.Count() == 2
                ? "" + quest.Category[0] + "-" + quest.Category[1]
                : "0-0";
            tooltipQuickView.Pdesc = quest.DemandBase;
            tooltipQuickView.Hdesc = quest.DemandSummary;
            tooltipQuickView.AutoDesc = quest.PlaceSummary;
            tooltipQuickView.DescLeftAlign = quest.Summary;
            tooltipQuickView.NodeID = quest.ID;
            quest.State = tooltipQuickView.QuestRender.DefaultState;
            this.showUnityTooltip(
                quest, "quest_" + quest.ID + "_" + RemoveInvalidFileNameChars(quest.Name) + ".png");
        }

        /// <summary>
        /// 把 Classic 拆開的任務資產組成 WZ 形狀的 img 節點：
        /// <c>&lt;id&gt;/QuestInfo</c>（QuestInfo img）與 <c>&lt;id&gt;/Check</c>（Check img）。
        /// </summary>
        private Wz_Node BuildUnityQuestImage(Wz_Node questInfoNode, int questId, UnityWzImage unityImage)
        {
            Wz_Node img = new Wz_Node(questId.ToString());

            // QuestInfo/訊息：先帶入資料集內的欄位，再以字串表覆寫（字串表才是顯示文字）。
            Wz_Node info = img.Nodes.Add("QuestInfo");
            CopyWzNodeChildren(questInfoNode, info);

            // 資料集內的 name 可能只是佔位節點（值為空），先移除避免顯示成字面上的 "name"。
            RemoveChildByName(info, "name");

            Wz_Node check = unityImage.FindNode("Quest/Check/" + questId);
            if (check != null)
            {
                Wz_Node checkImg = img.Nodes.Add("Check");
                CopyWzNodeChildren(check, checkImg);
            }

            string name = this.ApplyQuestStrings(info, questId, unityImage);
            SetOrAddChildValue(info, "name", string.IsNullOrEmpty(name) ? "未知のクエスト" : name);
            return img;
        }

        /// <summary>
        /// 以 <c>String/&lt;locale&gt;/QuestData/&lt;id&gt;</c> 補上任務的顯示文字
        /// （name → name、Info/&lt;n&gt; → 0/1/2、demandSummary/rewardSummary）；回傳任務名稱。
        /// </summary>
        private string ApplyQuestStrings(Wz_Node info, int questId, UnityWzImage unityImage)
        {
            UnityAssetDocument document = this.openedUnitySources == null ? null
                : this.openedUnitySources.Values.FirstOrDefault(
                    doc => ReferenceEquals(doc.Source, unityImage.Source));
            Wz_Node questNode = document?.StringTables?.QuestNode;
            if (questNode == null)
            {
                return null;
            }

            string id = questId.ToString();
            Wz_Node questInfo = null;
            Wz_Node dataNode = FindChildByName(questNode, "QuestData");
            foreach (Wz_Node node in dataNode?.Nodes ?? Enumerable.Empty<Wz_Node>())
            {
                if (string.Equals(node.Text.Replace(".img", ""), id, StringComparison.Ordinal))
                {
                    questInfo = FindChildByName(node, "QuestInfo") ?? node;
                    break;
                }
            }
            if (questInfo == null)
            {
                return null;
            }

            // String/<region>/QuestData/<id>/name
            string name = FindChildByName(questInfo, "name")?.GetValueEx<string>(null);

            Wz_Node stringInfo = FindChildByName(questInfo, "Info");
            foreach (Wz_Node node in stringInfo?.Nodes ?? Enumerable.Empty<Wz_Node>())
            {
                if (node.Value == null)
                {
                    continue;
                }
                string value = node.GetValueEx<string>(null);
                int stage;
                if (int.TryParse(node.Text, out stage))
                {
                    // 階段說明 → WZ 的 QuestInfo/0、1、2…
                    SetOrAddChildValue(info, stage.ToString(), value);
                    continue;
                }
                switch (node.Text.ToLowerInvariant())
                {
                    case "demandsummary":
                        SetOrAddChildValue(info, "demandSummary", value);
                        break;
                    case "rewardsummary":
                        SetOrAddChildValue(info, "rewardSummary", value);
                        break;
                }
            }
            return name;
        }

        /// <summary>以名稱尋找子節點（先精確比對，再不分大小寫）。</summary>
        private static Wz_Node FindChildByName(Wz_Node parent, string name)
        {
            if (parent == null || name == null)
            {
                return null;
            }
            Wz_Node child = parent.Nodes[name];
            if (child != null)
            {
                return child;
            }
            foreach (Wz_Node candidate in parent.Nodes)
            {
                if (string.Equals(candidate.Text, name, StringComparison.OrdinalIgnoreCase))
                {
                    return candidate;
                }
            }
            return null;
        }

        /// <summary>移除指定名稱的子節點（不分大小寫）。</summary>
        private static void RemoveChildByName(Wz_Node parent, string name)
        {
            List<Wz_Node> keep = new List<Wz_Node>();
            bool removed = false;
            foreach (Wz_Node child in parent.Nodes)
            {
                if (string.Equals(child.Text, name, StringComparison.OrdinalIgnoreCase))
                {
                    removed = true;
                }
                else
                {
                    keep.Add(child);
                }
            }
            if (!removed)
            {
                return;
            }
            parent.Nodes.Clear();
            foreach (Wz_Node child in keep)
            {
                parent.Nodes.Add(child);
            }
        }

        /// <summary>把來源節點的子樹複製到目標節點底下（不共用父子關係）。</summary>
        private static void CopyWzNodeChildren(Wz_Node source, Wz_Node target)
        {
            foreach (Wz_Node child in source.Nodes)
            {
                target.Nodes.Add(CloneWzNode(child));
            }
        }

        private static Wz_Node CloneWzNode(Wz_Node source)
        {
            Wz_Node clone = new Wz_Node(source.Text);
            clone.Value = source.Value;
            CopyWzNodeChildren(source, clone);
            return clone;
        }

        /// <summary>設定子節點字串值；節點不存在時新增，值為 null 時不變更。</summary>
        private static void SetOrAddChildValue(Wz_Node parent, string name, string value)
        {
            if (value == null)
            {
                return;
            }
            Wz_Node child = parent.Nodes[name];
            if (child == null)
            {
                child = parent.Nodes.Add(name);
            }
            child.Value = value;
        }

        /// <summary>判斷節點是否為 Unity 技能節點（img 內的 <c>skill/&lt;id&gt;</c>）。</summary>
        private static bool IsUnitySkillNode(Wz_Node node)        {
            return node != null
                && node.ParentNode != null
                && string.Equals(node.ParentNode.Text, "skill", StringComparison.OrdinalIgnoreCase)
                && node.ParentNode.ParentNode != null
                && node.ParentNode.ParentNode.Value is UnityWzImage;
        }

        /// <summary>
        /// Unity 技能（<c>Skill/&lt;職業&gt;</c> img 內的 <c>skill/&lt;id&gt;</c> 節點）的快速預覽。
        /// </summary>
        private void quickViewUnitySkill(Wz_Node skillNode)
        {
            // 對應 WZ 的 Skill.wz\&lt;職業&gt;.img\skill\&lt;id&gt;，只有選到技能節點才顯示。
            if (!IsUnitySkillNode(skillNode))
            {
                return;
            }

            if (!this.stringLinker.HasValues)
            {
                this.LoadStringLinker();
            }

            Skill skill = null;
            StringResult sr = null;
            try
            {
                skill = Skill.CreateFromNode(skillNode, PluginManager.FindWz, PluginManager.FindWz);
                if (skill != null)
                {
                    switch (this.skillDefaultLevel)
                    {
                        case DefaultLevel.Level0: skill.Level = 0; break;
                        case DefaultLevel.Level1: skill.Level = 1; break;
                        case DefaultLevel.LevelMax: skill.Level = skill.MaxLevel; break;
                        case DefaultLevel.LevelMaxWithCO: skill.Level = skill.MaxLevel + 2; break;
                    }

                    if (stringLinker == null || !stringLinker.StringSkill.TryGetValue(skill.SkillID, out sr))
                    {
                        sr = new StringResultSkill();
                        sr.Name = "未知のスキル";
                    }
                }
            }
            catch (Exception ex)
            {
                labelItemStatus.Text = "ツールチップの生成に失敗しました：" + ex.Message;
            }

            if (skill == null)
            {
                return;
            }

            this.resetUnityTooltip();
            tooltipQuickView.NodeID = skill.SkillID;
            this.applyUnityStringToTooltip(sr, null, null);
            this.showUnityTooltip(
                skill, "skill_" + skill.SkillID + "_" + RemoveInvalidFileNameChars(sr.Name) + ".png");
        }

        private void comboBoxItemLanguage_SelectedIndexChanged(object sender, EventArgs e)
        {
            DevComponents.Editors.ComboItem item = comboBoxItemLanguage.SelectedItem as DevComponents.Editors.ComboItem;

            if (item != null)
            {
                GearGraphics.SetFontFamily(item.Text);
                ConfigManager.Reload();
                CharaSimConfig.Default.SelectedFontIndex = comboBoxItemLanguage.SelectedIndex;
                ConfigManager.Save();
            }
        }

        private void buttonItemClearSetItems_Click(object sender, EventArgs e)
        {
            int count = CharaSimLoader.LoadedSetItems.Count;
            CharaSimLoader.LoadedSetItems.Clear();
            CharaSimLoader.LoadedAstraSubWeapons.Clear();
            labelItemStatus.Text = "已整合" + count + "個套裝道具。";
        }

        private void buttonItemClearExclusiveEquips_Click(object sender, EventArgs e)
        {
            int count = CharaSimLoader.LoadedExclusiveEquips.Count;
            CharaSimLoader.LoadedExclusiveEquips.Clear();
            labelItemStatus.Text = "已整合" + count + "個不重複道具。";
        }

        private void buttonItemClearCommodities_Click(object sender, EventArgs e)
        {
            int count = CharaSimLoader.LoadedCommoditiesBySN.Count;
            CharaSimLoader.LoadedCommoditiesBySN.Clear();
            CharaSimLoader.LoadedCommoditiesByItemId.Clear();
            foreach (var dict in CharaSimLoader.LoadedCommodityPricesByItemId)
            {
                dict.Clear();
            }
            labelItemStatus.Text = "已整合" + count + "個現金道具。";
        }

        private void buttonItemCharItem_CheckedChanged(object sender, EventArgs e)
        {
            if (buttonItemCharItem.Checked)
                this.charaSimCtrl.UIItem.Refresh();
            this.charaSimCtrl.UIItem.Visible = buttonItemCharItem.Checked;
        }

        private void btnWorldArchiveBrowser_Click(object sender, EventArgs e)
        {
            if (PluginManager.FindWz(Wz_Type.Base) == null)
            {
                ToastNotification.Show(this, $"錯誤: 未開啟 Base.wz。", null, 2000, eToastGlowColor.Red, eToastPosition.TopCenter);
                return;
            }
            if (openedWz.Count > 1)
            {
                ToastNotification.Show(this, $"錯誤：使用此功能前僅可開啟一個 Base.wz。", null, 4000, eToastGlowColor.Red, eToastPosition.TopCenter);
                return;
            }
            Wz_Node etcWaNode = PluginManager.FindWz(Wz_Type.Etc)?.FindNodeByPath("worldArchive.img");
            Wz_Node uiWaNode = PluginManager.FindWz(Wz_Type.UI)?.FindNodeByPath("UIworldArchive.img");
            Wz_Node uiWaBonusNode = PluginManager.FindWz(Wz_Type.UI)?.FindNodeByPath("UIWorldArchiveBonusBook.img");
            if (etcWaNode == null || uiWaNode == null || uiWaBonusNode == null)
            {
                ToastNotification.Show(this, $"錯誤：該客戶端未實裝世界檔案。", null, 2000, eToastGlowColor.Red, eToastPosition.TopCenter);
                return;
            }
            foreach (Form form in Application.OpenForms)
            {
                if (form is FrmWorldArchiveBrowser && !form.IsDisposed)
                {
                    form.Show();
                    form.BringToFront();
                    return;
                }
            }
            FrmWorldArchiveBrowser frmWorldArchiveBrowser = new FrmWorldArchiveBrowser(styleManager1.ManagerStyle == eStyle.VisualStudio2012Dark);
            frmWorldArchiveBrowser.EtcWaNode = etcWaNode;
            frmWorldArchiveBrowser.UiWaNode = uiWaNode;
            frmWorldArchiveBrowser.UiWaBonusNode = uiWaBonusNode;
            frmWorldArchiveBrowser.MobNode = PluginManager.FindWz(Wz_Type.Mob);
            frmWorldArchiveBrowser.NpcNode = PluginManager.FindWz(Wz_Type.Npc);
            frmWorldArchiveBrowser.stringLinker = this.stringLinker;
            frmWorldArchiveBrowser.regionID = 0;
            frmWorldArchiveBrowser.typeID = 0;
            frmWorldArchiveBrowser._mainForm = this;
            frmWorldArchiveBrowser.LoadCmbRegion();
            frmWorldArchiveBrowser.Show();
        }

        public void RedirectToNode(Wz_Node node)
        {
            if (OnSelectedWzNode(node))
            {
                tooltipQuickView.BringToFront();
            }
        }

        private void buttonItemAddItem_Click(object sender, EventArgs e)
        {
            bool success;

            success = this.charaSimCtrl.UIItem.AddItem(this.tooltipQuickView.TargetItem as ItemBase);
            if (!success)
            {
                labelItemStatus.Text = "選定的項目不存在或不再可用。";
            }
        }

        private void afrm_KeyDown(object sender, KeyEventArgs e)
        {
            AfrmTooltip frm = sender as AfrmTooltip;
            if (frm == null)
                return;

            bool doMove = true;
            Skill skill = frm.TargetItem as Skill;
            if (skill != null)
            {
                switch (e.KeyCode)
                {
                    case Keys.Oemplus:
                    case Keys.Add:
                        skill.Level += 1;
                        frm.Refresh();
                        return;

                    case Keys.OemMinus:
                    case Keys.Subtract:
                        skill.Level -= 1;
                        frm.Refresh();
                        return;

                    case Keys.OemOpenBrackets:
                        skill.Level -= this.skillInterval;
                        frm.Refresh();
                        return;
                    case Keys.OemCloseBrackets:
                        skill.Level += this.skillInterval;
                        frm.Refresh();
                        return;

                    case Keys.PageDown:
                        skill.PerJobIndex += 1;
                        frm.Refresh();
                        return;

                    case Keys.PageUp:
                        skill.PerJobIndex -= 1;
                        frm.Refresh();
                        return;
                }
            }

            Quest quest = frm.TargetItem as Quest;
            if (quest != null && !frm.QuestRender.ShowAllStates)
            {
                switch (e.KeyCode)
                {
                    case Keys.Right:
                        if (!e.Control)
                        {
                            quest.State += 1;
                            doMove = false;
                            frm.Refresh();
                            return;
                        }
                        break;
                    case Keys.Oemplus:
                    case Keys.Add:
                        quest.State += 1;
                        frm.Refresh();
                        return;

                    case Keys.Left:
                        if (!e.Control)
                        {
                            quest.State -= 1;
                            doMove = false;
                            frm.Refresh();
                            return;
                        }
                        break;
                    case Keys.OemMinus:
                    case Keys.Subtract:
                        quest.State -= 1;
                        frm.Refresh();
                        return;
                }
            }


            Npc npc = frm.TargetItem as Npc;
            if (npc != null)
            {
                switch (e.KeyCode)
                {
                    case Keys.Oemplus:
                    case Keys.Add:
                        npc.IllustIndex += 1;
                        frm.Refresh();
                        return;

                    case Keys.OemMinus:
                    case Keys.Subtract:
                        npc.IllustIndex -= 1;
                        frm.Refresh();
                        return;
                }
            }

            Mob mob = frm.TargetItem as Mob;
            if (mob != null)
            {
                switch (e.KeyCode)
                {
                    case Keys.Oemplus:
                    case Keys.Add:
                        mob.MobGroupIndex += 1;
                        frm.Refresh();
                        return;

                    case Keys.OemMinus:
                    case Keys.Subtract:
                        mob.MobGroupIndex -= 1;
                        frm.Refresh();
                        return;
                }
            }

            switch (e.KeyCode)
            {
                case Keys.Escape:
                    frm.Hide();
                    return;
                case Keys.Up:
                    if (doMove) frm.Top -= 1;
                    return;
                case Keys.Down:
                    if (doMove) frm.Top += 1;
                    return;
                case Keys.Left:
                    if (doMove) frm.Left -= 1;
                    return;
                case Keys.Right:
                    if (doMove) frm.Left += 1;
                    return;
            }
        }

        private void afrm_VisibleChanged(object sender, EventArgs e)
        {
            if (sender is AfrmItem)
            {
                buttonItemCharItem.Checked = ((AfrmItem)sender).Visible;
            }
            else if (sender is AfrmStat)
            {
                buttonItemCharaStat.Checked = ((AfrmStat)sender).Visible;
            }
        }

        private void buttonItemCharaStat_CheckedChanged(object sender, EventArgs e)
        {
            if (buttonItemCharaStat.Checked)
            {
                this.charaSimCtrl.UIStat.Refresh();
            }
            this.charaSimCtrl.UIStat.Visible = buttonItemCharaStat.Checked;
        }


        private void buttonItemQuickViewSetting_Click(object sender, EventArgs e)
        {
            using (FrmQuickViewSetting frm = new FrmQuickViewSetting())
            {
                frm.Load(CharaSimConfig.Default);

                if (frm.ShowDialog() == DialogResult.OK)
                {
                    ConfigManager.Reload();
                    frm.Save(CharaSimConfig.Default);
                    ConfigManager.Save();
                    UpdateCharaSimSettings();
                }
            }
        }
        #endregion

        #region 实现插件接口
        Office2007RibbonForm PluginContextProvider.MainForm
        {
            get { return this; }
        }

        DotNetBarManager PluginContextProvider.DotNetBarManager
        {
            get { return this.dotNetBarManager1; }
        }

        IList<Wz_Structure> PluginContextProvider.LoadedWz
        {
            get { return new System.Collections.ObjectModel.ReadOnlyCollection<Wz_Structure>(this.openedWz); }
        }

        Wz_Node PluginContextProvider.SelectedNode1
        {
            get { return advTree1.SelectedNode.AsWzNode(); }
        }

        Wz_Node PluginContextProvider.SelectedNode2
        {
            get { return advTree2.SelectedNode.AsWzNode(); }
        }

        Wz_Node PluginContextProvider.SelectedNode3
        {
            get { return advTree3.SelectedNode.AsWzNode(); }
        }

        bool PluginContextProvider.SelectNode(Wz_Node node)
        {
            if (node == null)
            {
                return false;
            }
            if (this.InvokeRequired)
            {
                return (bool)this.Invoke(new Func<Wz_Node, bool>(((PluginContextProvider)this).SelectNode), node);
            }
            return this.OnSelectedWzNode(node);
        }

        private EventHandler<WzNodeEventArgs> selectedNode1Changed;
        private EventHandler<WzNodeEventArgs> selectedNode2Changed;
        private EventHandler<WzNodeEventArgs> selectedNode3Changed;
        private EventHandler<WzStructureEventArgs> wzOpened;
        private EventHandler<WzStructureEventArgs> wzClosing;

        event EventHandler<WzNodeEventArgs> PluginContextProvider.SelectedNode1Changed
        {
            add { selectedNode1Changed += value; }
            remove { selectedNode1Changed -= value; }
        }

        event EventHandler<WzNodeEventArgs> PluginContextProvider.SelectedNode2Changed
        {
            add { selectedNode2Changed += value; }
            remove { selectedNode2Changed -= value; }
        }

        event EventHandler<WzNodeEventArgs> PluginContextProvider.SelectedNode3Changed
        {
            add { selectedNode3Changed += value; }
            remove { selectedNode3Changed -= value; }
        }

        event EventHandler<WzStructureEventArgs> PluginContextProvider.WzOpened
        {
            add { wzOpened += value; }
            remove { wzOpened -= value; }
        }

        event EventHandler<WzStructureEventArgs> PluginContextProvider.WzClosing
        {
            add { wzClosing += value; }
            remove { wzClosing -= value; }
        }

        StringLinker PluginContextProvider.DefaultStringLinker
        {
            get { return this.stringLinker; }
        }

        AlphaForm PluginContextProvider.DefaultTooltipWindow
        {
            get { return this.tooltipQuickView; }
        }

        eStyle PluginContextProvider.MainStyle
        {
            get { return this.styleManager1.ManagerStyle; }
        }

        private EventHandler mainStyleChanged;

        event EventHandler PluginContextProvider.MainStyleChanged
        {
            add { mainStyleChanged += value; }
            remove { mainStyleChanged -= value; }
        }

        private void RegisterPluginEvents()
        {
            advTree1.AfterNodeSelect += advTree1_AfterNodeSelect_Plugin;
            advTree2.AfterNodeSelect += advTree2_AfterNodeSelect_Plugin;
            advTree3.AfterNodeSelect += advTree3_AfterNodeSelect_Plugin;
        }

        private void advTree1_AfterNodeSelect_Plugin(object sender, AdvTreeNodeEventArgs e)
        {
            if (selectedNode1Changed != null)
            {
                var wzNode = ((PluginContextProvider)(this)).SelectedNode1;
                var args = new WzNodeEventArgs(wzNode);
                selectedNode1Changed(this, args);
            }
        }

        private void advTree2_AfterNodeSelect_Plugin(object sender, AdvTreeNodeEventArgs e)
        {
            if (selectedNode2Changed != null)
            {
                var wzNode = ((PluginContextProvider)(this)).SelectedNode2;
                var args = new WzNodeEventArgs(wzNode);
                selectedNode2Changed(this, args);
            }
        }

        private void advTree3_AfterNodeSelect_Plugin(object sender, AdvTreeNodeEventArgs e)
        {
            if (selectedNode3Changed != null)
            {
                var wzNode = ((PluginContextProvider)(this)).SelectedNode3;
                var args = new WzNodeEventArgs(wzNode);
                selectedNode3Changed(this, args);
            }
        }

        protected virtual void OnWzOpened(WzStructureEventArgs e)
        {
            if (wzOpened != null)
            {
                wzOpened(this, e);
            }
        }

        protected virtual void OnWzClosing(WzStructureEventArgs e)
        {
            if (wzClosing != null)
            {
                wzClosing(this, e);
            }
        }
        #endregion

        private void btnEasyCompare_Click(object sender, EventArgs e)
        {
            if (compareThread != null)
            {
                // compareThread.Suspend();
                if (DialogResult.Yes == MessageBoxEx.Show("比較正在進行中。你想中斷嗎？", "注意", MessageBoxButtons.YesNoCancel))
                {
                    // compareThread.Resume();
                    compareThread.Interrupt();
                    compareThread = null;
                    GC.Collect();
                }
                else
                {
                    // compareThread.Resume();
                }
                return;
            }

            if (openedWz.Count < 2)
            {
                MessageBoxEx.Show("若要開始比較，請開啟兩個 WZ 檔案。", LocalizedString_JP.COMMON_ERROR);
                return;
            }

            FolderBrowserDialog dlg = new FolderBrowserDialog();
            dlg.Description = "選擇目標資料夾。";

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                Dictionary<string, bool> selectedNodes = new Dictionary<string, bool>();
                for (int i = 0; i < clbRootNode.Items.Count; i++)
                {
                    string item = clbRootNode.Items[i].ToString();
                    bool isChecked = clbRootNode.GetItemChecked(i);
                    selectedNodes[item] = isChecked;
                }
                clbRootNode.Visible = false;
                btnSelectDeselectAllNode.Visible = false;
                btnReverseNodeSelection.Visible = false;
                compareThread = new Thread(() =>
                {
                    System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();
                    EasyComparer comparer = new EasyComparer();
                    comparer.selectedNodes = selectedNodes;
                    comparer.Comparer.PngComparison = (WzPngComparison)cmbComparePng.SelectedItem;
                    comparer.Comparer.ResolvePngLink = chkResolvePngLink.Checked;
                    comparer.OutputPng = chkOutputPng.Checked;
                    comparer.OutputAddedImg = chkOutputAddedImg.Checked;
                    comparer.OutputRemovedImg = chkOutputRemovedImg.Checked;
                    comparer.OutputSkillTooltip = chkOutputSkillTooltip.Checked && selectedNodes["Skill"] && selectedNodes["String"];
                    comparer.OutputItemTooltip = chkOutputItemTooltip.Checked && selectedNodes["Item"] && selectedNodes["String"];
                    comparer.OutputGearTooltip = chkOutputEqpTooltip.Checked && selectedNodes["Character"] && selectedNodes["String"];
                    comparer.OutputMapTooltip = chkOutputMapTooltip.Checked && selectedNodes["Map"] && selectedNodes["String"];
                    comparer.OutputMobTooltip = chkOutputMobTooltip.Checked && selectedNodes["Mob"] && selectedNodes["String"];
                    comparer.OutputNpcTooltip = chkOutputNpcTooltip.Checked && selectedNodes["Npc"] && selectedNodes["String"];
                    comparer.OutputCashTooltip = chkOutputCashTooltip.Checked && selectedNodes["Item"] && selectedNodes["String"];
                    comparer.OutputQuestTooltip = chkOutputQuestTooltip.Checked && selectedNodes["Quest"];
                    comparer.OutputAchvTooltip = chkOutputAchvTooltip.Checked && selectedNodes["Etc"];
                    comparer.ShowObjectID = chkShowObjectID.Checked;
                    comparer.ShowChangeType = chkShowChangeType.Checked;
                    comparer.ShowPrice = chkShowPrice.Checked;
                    comparer.ShowLinkedTamingMob = chkShowLinkedTamingMob.Checked;
                    comparer.SkipKMSContent = chkSkipKMSContent.Checked;
                    comparer.SkipGodChangseopDuplicatedNodes = chkSkipGodChangseopDuplicatedNodes.Checked;
                    comparer.PostChangesToDiscord = chkPostChangesToDiscord.Checked;
                    comparer.Enable22AniStyle = GearGraphics.is22aniStyle;
                    comparer.EnableAssembleTooltip = CharaSimConfig.Default.Item.UseAssembleUI;
                    comparer.ShowDamageSkin = CharaSimConfig.Default.DamageSkin.ShowDamageSkin;
                    comparer.UseMiniSizeDamageSkin = CharaSimConfig.Default.DamageSkin.UseMiniSize;
                    comparer.AlwaysUseMseaFormatDamageSkin = CharaSimConfig.Default.DamageSkin.AlwaysUseMseaFormat;
                    comparer.DisplayDamageSkinUnitOnSingleLine = CharaSimConfig.Default.DamageSkin.DisplayUnitOnSingleLine;
                    comparer.UseInGameSpacing = CharaSimConfig.Default.DamageSkin.UseInGameSpacing;
                    comparer.DamageSkinNumber = CharaSimConfig.Default.DamageSkin.DamageSkinNumber;
                    comparer.AllowFamiliarOutOfBounds = CharaSimConfig.Default.Familiar.AllowOutOfBounds;
                    comparer.UseCTFamiliarUI = CharaSimConfig.Default.Familiar.UseCTFamiliarUI;
                    comparer.EnableWorldArchive = CharaSimConfig.Default.Misc.EnableWorldArchive;
                    comparer.ShowNpcQuotes = CharaSimConfig.Default.Npc.ShowNpcQuotes;
                    comparer.EnableMonsterBook = CharaSimConfig.Default.Mob.EnableMonsterBook;
                    comparer.LocatePetEquip = CharaSimConfig.Default.Misc.LocatePetEquip;
                    comparer.OutputChairAndRidingMount = chkOutputChairAndRidingMount.Checked;
                    comparer.AvatarCode = GetEasyCompareAvatarCode();
                    comparer.StateInfoChanged += new EventHandler(comparer_StateInfoChanged);
                    comparer.StateDetailChanged += new EventHandler(comparer_StateDetailChanged);
                    comparer.ColorTable = new List<System.Drawing.Color>()
                    {
                        CustomCSSConfig.Default.BackgroundColor,
                        CustomCSSConfig.Default.NormalTextColor,
                        CustomCSSConfig.Default.ChangedBackgroundColor,
                        CustomCSSConfig.Default.AddedBackgroundColor,
                        CustomCSSConfig.Default.RemovedBackgroundColor,
                        CustomCSSConfig.Default.ChangedTextColor,
                        CustomCSSConfig.Default.AddedTextColor,
                        CustomCSSConfig.Default.RemovedTextColor,
                        CustomCSSConfig.Default.HyperlinkColor
                    };
                    try
                    {
                        Wz_File fileNew = openedWz[0].wz_files[0];
                        Wz_File fileOld = openedWz[1].wz_files[0];

                        while (true)
                        {
                            string txt = string.Format("WZ 檔案:\r\n\r\n 較新版本：{0} (V{1})\r\n 較舊版本：{2} (V{3})\r\n\r\n點選「Yes」開始比較。點選「No」交換版本。",
                                fileNew.Header.FileName,
                                fileNew.GetMergedVersion(),
                                fileOld.Header.FileName,
                                fileOld.GetMergedVersion()
                                );
                            switch (MessageBoxEx.Show(txt, "WZ比較", MessageBoxButtons.YesNoCancel))
                            {
                                case DialogResult.Yes:
                                    btnEasyCompare.Enabled = false;
                                    btnPreset.Enabled = false;
                                    clbRootNode.Enabled = false;
                                    btnSelectDeselectAllNode.Enabled = false;
                                    btnReverseNodeSelection.Enabled = false;
                                    btnCustomCSS.Enabled = false;
                                    cmbComparePng.Enabled = false;
                                    chkOutputPng.Enabled = false;
                                    chkResolvePngLink.Enabled = false;
                                    chkOutputAddedImg.Enabled = false;
                                    chkOutputRemovedImg.Enabled = false;
                                    chkOutputSkillTooltip.Enabled = false;
                                    chkOutputItemTooltip.Enabled = false;
                                    chkOutputEqpTooltip.Enabled = false;
                                    chkOutputMapTooltip.Enabled = false;
                                    chkOutputMobTooltip.Enabled = false;
                                    chkOutputNpcTooltip.Enabled = false;
                                    chkOutputQuestTooltip.Enabled = false;
                                    chkOutputAchvTooltip.Enabled = false;
                                    // chkOutputCashTooltip.Enabled = false;
                                    chkShowObjectID.Enabled = false;
                                    chkShowChangeType.Enabled = false;
                                    chkShowPrice.Enabled = false;
                                    chkShowLinkedTamingMob.Enabled = false;
                                    chkSkipKMSContent.Enabled = false;
                                    chkSkipGodChangseopDuplicatedNodes.Enabled = false;
                                    chkPostChangesToDiscord.Enabled = false;
                                    chkOutputChairAndRidingMount.Enabled = false;
                                    btnAvatarCode.Enabled = false;
                                    if (chkSkipKMSContent.Checked)
                                    {
                                        switch (MessageBoxEx.Show(this, "是否要下載 KMS 內容資料庫？ \r\n\r\n如果選擇“No”，則只跳過 KMS 技能。", "WZ比較", MessageBoxButtons.YesNo))
                                        {
                                            case DialogResult.Yes:
                                                comparer.DownloadKMSContentDB = true;
                                                break;
                                            case DialogResult.No:
                                                comparer.DownloadKMSContentDB = false;
                                                break;
                                            default:
                                                return;
                                        }
                                    }
                                    comparer.EasyCompareWzFiles(fileNew, fileOld, dlg.SelectedPath);
                                    return;

                                case DialogResult.No:
                                    Wz_File tmp = fileNew;
                                    fileNew = fileOld;
                                    fileOld = tmp;
                                    break;

                                case DialogResult.Cancel:
                                default:
                                    return;
                            }
                        }

                    }
                    catch (ThreadAbortException)
                    {
                        MessageBoxEx.Show(this, "比較已中斷。", LocalizedString_JP.COMMON_ERROR);
                    }
                    catch (Exception ex)
                    {
                        MessageBoxEx.Show(this, "比較已中斷。" + ex.ToString(), LocalizedString_JP.COMMON_ERROR);
                    }
                    finally
                    {
                        sw.Stop();
                        compareThread = null;
                        labelXComp1.Text = "比較已完成。時間經過：" + sw.Elapsed.ToString();
                        labelXComp2.Text = "";
                        btnEasyCompare.Enabled = true;
                        btnPreset.Enabled = true;
                        clbRootNode.Enabled = true;
                        btnSelectDeselectAllNode.Enabled = true;
                        btnReverseNodeSelection.Enabled = true;
                        btnCustomCSS.Enabled = true;
                        cmbComparePng.Enabled = true;
                        chkOutputPng.Enabled = true;
                        chkResolvePngLink.Enabled = true;
                        chkOutputAddedImg.Enabled = true;
                        chkOutputRemovedImg.Enabled = true;
                        chkOutputSkillTooltip.Enabled = true;
                        chkOutputItemTooltip.Enabled = true;
                        chkOutputEqpTooltip.Enabled = true;
                        chkOutputMapTooltip.Enabled = true;
                        chkOutputMobTooltip.Enabled = true;
                        chkOutputNpcTooltip.Enabled = true;
                        chkOutputQuestTooltip.Enabled = true;
                        chkOutputAchvTooltip.Enabled = true;
                        // chkOutputCashTooltip.Enabled = true;
                        chkShowObjectID.Enabled = true;
                        chkShowChangeType.Enabled = true;
                        chkShowPrice.Enabled = true;
                        chkShowLinkedTamingMob.Enabled = true;
                        chkSkipKMSContent.Enabled = true;
                        chkSkipGodChangseopDuplicatedNodes.Enabled = true;
                        chkPostChangesToDiscord.Enabled = true;
                        chkOutputChairAndRidingMount.Enabled = true;
                        btnAvatarCode.Enabled = chkOutputChairAndRidingMount.Checked;
                        if (comparer.FailToExportNodes.Count > 0 || comparer.FailToExportTooltips.Count > 0)
                        {
                            string failData = Newtonsoft.Json.JsonConvert.SerializeObject(comparer.FailToExportNodes, Newtonsoft.Json.Formatting.Indented) + "\r\n" + Newtonsoft.Json.JsonConvert.SerializeObject(comparer.FailToExportTooltips, Newtonsoft.Json.Formatting.Indented);
                            File.WriteAllText(Path.Combine(dlg.SelectedPath, "fail_to_export_nodes.log"), failData, Encoding.UTF8);
                            MessageBoxEx.Show(this, "比較已完成，但有些節點無法解析。 \r\n按一下「確定」以查看哪些節點無法匯出。", "WZ比較", MessageBoxButtons.OK, MessageBoxIcon.Warning);
#if NET6_0_OR_GREATER
                            Process.Start(new ProcessStartInfo
                            {
                                UseShellExecute = true,
                                FileName = Path.Combine(dlg.SelectedPath, "fail_to_export_nodes.log"),
                            });
#else
            Process.Start(Path.Combine(dlg.SelectedPath, "fail_to_export_nodes.log"));
#endif
                        }
                    }
                });
                compareThread.Priority = ThreadPriority.Highest;
                compareThread.Start();
            }
        }

        void comparer_StateDetailChanged(object sender, EventArgs e)
        {
            EasyComparer comp = sender as EasyComparer;
            if (comp != null)
            {
                labelXComp1.Text = comp.StateInfo;
            }
        }

        void comparer_StateInfoChanged(object sender, EventArgs e)
        {
            EasyComparer comp = sender as EasyComparer;
            if (comp != null)
            {
                labelXComp2.Text = comp.StateDetail;
            }
        }

        void RunSetupWizard(bool isFirstRun=false)
        {
            var frm = new FrmSetupWizard(styleManager1.ManagerStyle == eStyle.VisualStudio2012Dark);
            frm.Load(WcR2Config.Default, CharaSimConfig.Default);
            frm.isFirstRun = isFirstRun;
            if (frm.ShowDialog() == DialogResult.OK)
            {
                frm.Save(WcR2Config.Default, CharaSimConfig.Default);
                ConfigManager.Save();
                UpdateWzLoadingSettings();
                UpdateTranslateSettings();
                UpdateCharaSimSettings();
            }
        }

        private void buttonItemAbout_Click(object sender, EventArgs e)
        {
            new FrmAbout(styleManager1.ManagerStyle == eStyle.VisualStudio2012Dark).ShowDialog();
        }

        private void btnRootNode_Click(object sender, EventArgs e)
        {
            clbRootNode.Visible = !clbRootNode.Visible;
            btnSelectDeselectAllNode.Visible = !btnSelectDeselectAllNode.Visible;
            btnReverseNodeSelection.Visible = !btnReverseNodeSelection.Visible;
        }

        private void btnMusicChannel_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < clbRootNode.Items.Count; i++)
            {
                bool nodeOption = new string[] { "Effect", "Map", "Sound", "String", "UI" }.Contains(clbRootNode.Items[i].ToString());
                clbRootNode.SetItemChecked(i, nodeOption);
            }
            chkOutputPng.Checked = true;
            chkResolvePngLink.Checked = true;
            chkOutputAddedImg.Checked = true;
            chkOutputRemovedImg.Checked = true;
            chkOutputSkillTooltip.Checked = false;
            chkOutputEqpTooltip.Checked = false;
            chkOutputItemTooltip.Checked = false;
            chkOutputMapTooltip.Checked = true;
            chkOutputMobTooltip.Checked = false;
            chkOutputNpcTooltip.Checked = false;
            chkOutputQuestTooltip.Checked = false;
            chkOutputAchvTooltip.Checked = false;
            chkOutputChairAndRidingMount.Checked = false;
            chkShowObjectID.Checked = true;
            chkShowChangeType.Checked = true;
            chkShowPrice.Checked = true;
            chkShowLinkedTamingMob.Checked = false;
            chkSkipKMSContent.Checked = false;
        }

        private Wz_Type ParseWzTypeManually(string baseDir)
        {
            switch (baseDir)
            {
                case "Character":
                    return Wz_Type.Character;
                case "DataMap":
                    return Wz_Type.DataMap;
                case "Effect":
                    return Wz_Type.Effect;
                case "Etc":
                    return Wz_Type.Etc;
                case "Item":
                    return Wz_Type.Item;
                case "Language":
                    return Wz_Type.Language;
                case "Map":
                    return Wz_Type.Map;
                case "Mob":
                    return Wz_Type.Mob;
                case "Morph":
                    return Wz_Type.Morph;
                case "Npc":
                    return Wz_Type.Npc;
                case "Quest":
                    return Wz_Type.Quest;
                case "Reactor":
                    return Wz_Type.Reactor;
                case "Skill":
                    return Wz_Type.Skill;
                case "Sound":
                    return Wz_Type.Sound;
                case "String":
                    return Wz_Type.String;
                case "TamingMob":
                    return Wz_Type.TamingMob;
                case "UI":
                    return Wz_Type.UI;
                default:
                    return Wz_Type.Unknown;
            }
        }

        private void btnSkillChangeInfo_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < clbRootNode.Items.Count; i++)
            {
                bool nodeOption = new string[] { "Skill", "String" }.Contains(clbRootNode.Items[i].ToString());
                clbRootNode.SetItemChecked(i, nodeOption);
            }
            chkOutputPng.Checked = true;
            chkResolvePngLink.Checked = true;
            chkOutputAddedImg.Checked = true;
            chkOutputRemovedImg.Checked = true;
            chkOutputSkillTooltip.Checked = true;
            chkOutputEqpTooltip.Checked = false;
            chkOutputItemTooltip.Checked = false;
            chkOutputMapTooltip.Checked = false;
            chkOutputMobTooltip.Checked = false;
            chkOutputNpcTooltip.Checked = false;
            chkOutputQuestTooltip.Checked = false;
            chkOutputAchvTooltip.Checked = false;
            chkOutputChairAndRidingMount.Checked = false;
            chkShowObjectID.Checked = true;
            chkShowChangeType.Checked = true;
            chkShowPrice.Checked = true;
            chkShowLinkedTamingMob.Checked = false;
            chkSkipKMSContent.Checked = false;
        }

        private void btnNewItemNews_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < clbRootNode.Items.Count; i++)
            {
                bool nodeOption = new string[] { "Character", "Effect", "Item", "Map", "String", "UI" }.Contains(clbRootNode.Items[i].ToString());
                clbRootNode.SetItemChecked(i, nodeOption);
            }
            chkOutputPng.Checked = true;
            chkResolvePngLink.Checked = true;
            chkOutputAddedImg.Checked = true;
            chkOutputRemovedImg.Checked = true;
            chkOutputSkillTooltip.Checked = false;
            chkOutputEqpTooltip.Checked = true;
            chkOutputItemTooltip.Checked = true;
            chkOutputMapTooltip.Checked = true;
            chkOutputMobTooltip.Checked = false;
            chkOutputNpcTooltip.Checked = false;
            chkOutputQuestTooltip.Checked = false;
            chkOutputAchvTooltip.Checked = false;
            chkOutputChairAndRidingMount.Checked = true;
            chkShowObjectID.Checked = true;
            chkShowChangeType.Checked = true;
            chkShowPrice.Checked = true;
            chkShowLinkedTamingMob.Checked = false;
            chkSkipKMSContent.Checked = false;
        }

        private void btnMapleWiki_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < clbRootNode.Items.Count; i++)
            {
                clbRootNode.SetItemChecked(i, true);
            }
            chkOutputPng.Checked = true;
            chkResolvePngLink.Checked = true;
            chkOutputAddedImg.Checked = true;
            chkOutputRemovedImg.Checked = true;
            chkOutputSkillTooltip.Checked = true;
            chkOutputEqpTooltip.Checked = true;
            chkOutputItemTooltip.Checked = true;
            chkOutputMapTooltip.Checked = true;
            chkOutputMobTooltip.Checked = true;
            chkOutputNpcTooltip.Checked = true;
            chkOutputQuestTooltip.Checked = true;
            chkOutputAchvTooltip.Checked = true;
            chkOutputChairAndRidingMount.Checked = true;
            chkShowObjectID.Checked = true;
            chkShowChangeType.Checked = true;
            chkShowPrice.Checked = true;
            chkShowLinkedTamingMob.Checked = true;
            chkSkipKMSContent.Checked = false;
        }

        private void btnSelectDeselectAllNode_Click(object sender, EventArgs e)
        {
            bool selectAll = clbRootNode.CheckedItems.Count < clbRootNode.Items.Count;
            for (int i = 0; i < clbRootNode.Items.Count; i++)
            {
                clbRootNode.SetItemChecked(i, selectAll);
            }
        }

        private void btnReverseNodeSelection_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < clbRootNode.Items.Count; i++)
            {
                clbRootNode.SetItemChecked(i, !clbRootNode.GetItemChecked(i));
            }
        }

        private async void btnExportSkill_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dlg = new FolderBrowserDialog();
            dlg.Description = "選擇要匯出到的資料夾。";
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                btnExportSkill.Enabled = false;
                labelX2.Text = "匯出中";
                System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();

                await Task.Run(() =>
                {
                    sw.Start();
                    if (!this.stringLinker.HasValues)
                        this.LoadStringLinker();

                    DBConnection conn = new DBConnection(this.stringLinker);
                    DataSet ds = conn.GenerateSkillTable();
                    foreach (DataTable dt in ds.Tables)
                    {
                        FileStream fs = new FileStream(Path.Combine(dlg.SelectedPath, dt.TableName + ".csv"), FileMode.Create);
                        StreamWriter sw = new StreamWriter(fs, Encoding.UTF8);
                        conn.OutputCsv(sw, dt);
                        sw.Close();
                        fs.Dispose();
                    }
                    stringLinker.StringMob.TryGetValue(100000, out var sr);
                    if (sr != null)
                    {
                        string langcode = "qps-ploc";
                        switch (sr.Name)
                        {
                            case "Snail":
                                langcode = "en";
                                break;
                            case "蜗牛":
                                langcode = "zh-CN";
                                break;
                            case "달팽이":
                                langcode = "ko";
                                break;
                            case "嫩寶":
                                langcode = "zh-TW";
                                break;
                            case "デンデン":
                                langcode = "ja";
                                break;
                        }
                        if (Directory.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TranslationCache")))
                        {
                            Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TranslationCache"));
                        }
                        if (File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TranslationCache", String.Format("ms_skill_{0}.csv", langcode))))
                        {
                            File.Delete(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TranslationCache", String.Format("ms_skill_{0}.csv", langcode)));
                        }
                        File.Copy(Path.Combine(dlg.SelectedPath, "ms_skill.csv"), Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TranslationCache", String.Format("ms_skill_{0}.csv", langcode)));
                    }
                });
                sw.Stop();
                btnExportSkill.Enabled = true;
                labelX2.Text = "匯出完成。時間經過：" + sw.Elapsed.ToString();
                labelItemStatus.Text = "匯出完成: " + dlg.SelectedPath;
            }
        }

        private async void btnPetEquipExport_Click(object sender, EventArgs e)
        {
            if (PluginManager.FindWz(Wz_Type.Base) == null)
            {
                ToastNotification.Show(this, $"錯誤: 未開啟 Base.wz。", null, 2000, eToastGlowColor.Red, eToastPosition.TopCenter);
                return;
            }
            if (openedWz.Count > 1)
            {
                ToastNotification.Show(this, $"錯誤：使用此功能前僅可開啟一個 Base.wz。", null, 4000, eToastGlowColor.Red, eToastPosition.TopCenter);
                return;
            }
            FolderBrowserDialog dlg = new FolderBrowserDialog();
            dlg.Description = "選擇要匯出到的資料夾。";
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                btnPetEquipExport.Enabled = false;
                labelX2.Text = "匯出中";
                System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                await Task.Run(() =>
                {
                    sw.Start();
                    if (!this.stringLinker.HasValues)
                        this.LoadStringLinker();

                    CharaSimLoader.LoadPetEquipInfoIfEmpty();

                    SortedDictionary<int, List<int>> petToEquip = new SortedDictionary<int, List<int>>();

                    foreach (var i in CharaSimLoader.LoadedPetEquipInfo.Keys)
                    {
                        List<int> applicablePets = CharaSimLoader.LoadedPetEquipInfo[i];
                        foreach (var j in applicablePets)
                        {
                            if (!petToEquip.ContainsKey(j)) petToEquip[j] = new List<int>();
                            petToEquip[j].Add(i);
                        }
                    }

                    JObject root = new JObject();
                    foreach (var pet in petToEquip)
                    {
                        int petId = pet.Key;
                        StringResult sr;
                        if (this.stringLinker == null || !this.stringLinker.StringItem.TryGetValue(petId, out sr))
                        {
                            sr = new StringResult();
                            sr.Name = "(null)";
                        }

                        var petEquips = new Dictionary<int, string>();

                        foreach (var i in pet.Value)
                        {
                            StringResult sr2;
                            if (this.stringLinker == null || !this.stringLinker.StringEqp.TryGetValue(i, out sr2))
                            {
                                sr2 = new StringResult();
                                sr2.Name = "(null)";
                            }
                            petEquips.Add(i, sr2.Name);
                        }

                        var equipsObj = new JObject(); 
                        foreach (var eq in petEquips)
                            equipsObj[eq.Key.ToString()] = eq.Value;

                        var petObj = new JObject
                        {
                            ["name"] = sr.Name,
                            ["petEquips"] = equipsObj
                        };
                        root[petId.ToString()] = petObj;
                    }
                    File.WriteAllText(Path.Combine(dlg.SelectedPath, "petToEquip.json"), root.ToString());
                });
                sw.Stop();
                btnPetEquipExport.Enabled = true;
                labelX2.Text = "匯出完成。時間經過：" + sw.Elapsed.ToString();
                labelItemStatus.Text = "匯出完成: " + dlg.SelectedPath;
            }
        }

        private async void btnSkillTooltipExport_Click(object sender, EventArgs e)
        {
            if (PluginManager.FindWz(Wz_Type.Base) == null)
            {
                ToastNotification.Show(this, $"錯誤: 未開啟 Base.wz。", null, 2000, eToastGlowColor.Red, eToastPosition.TopCenter);
                return;
            }
            if (openedWz.Count > 1)
            {
                ToastNotification.Show(this, $"錯誤：使用此功能前僅可開啟一個 Base.wz。", null, 4000, eToastGlowColor.Red, eToastPosition.TopCenter);
                return;
            }
            using (FrmSkillTooltipExport frm = new FrmSkillTooltipExport(styleManager1.ManagerStyle == eStyle.VisualStudio2012Dark))
            {
                frm.skillNode = PluginManager.FindWz(Wz_Type.Skill);
                if (frm.ShowDialog() == DialogResult.OK)
                {
                    var Setting = CharaSimConfig.Default;
                    List<int> selectedJob = frm.SelectedJobCodes;
                    bool doRoguelikePharaoh = selectedJob.Contains(99990000);
                    bool doRedmoon = selectedJob.Contains(99990001);
                    bool doGuildCastleGuildResearch = selectedJob.Contains(99990100);
                    bool doGuildCastlePersonalResearch = selectedJob.Contains(99990101);
                    string exportedFolder = frm.ExportFolderPath;
                    labelX2.Text = "匯出中";
                    System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                    try
                    {
                        sw.Start();
                        btnSkillTooltipExport.Enabled = false;
                        await Task.Run(() =>
                        {
                            if (!this.stringLinker.HasValues)
                                this.LoadStringLinker();

                            // Initialize VCore Dictionary
                            Dictionary<int, List<int>> FifthJobSkillToJobID = new Dictionary<int, List<int>>();
                            Wz_Node vCoreData = PluginManager.FindWz("Etc\\VCore.img\\CoreData") ?? PluginManager.FindWz("Etc\\VCore.img\\vSkill\\CoreData");
                            if (vCoreData != null)
                            {
                                foreach (Wz_Node data in vCoreData.Nodes)
                                {
                                    Wz_Node connectSkill = data.FindNodeByPath("connectSkill").ResolveUol();
                                    Wz_Node jobIDValue = data.FindNodeByPath("job").ResolveUol();
                                    List<int> applicableJobID = new List<int>();
                                    foreach (Wz_Node jobID in jobIDValue.Nodes)
                                    {
                                        applicableJobID.Add(jobID.GetValueEx<int>(0));
                                    }
                                    if (connectSkill == null)
                                    {
                                        int skillIDValue = data.FindNodeByPath("spCoreOption\\effect\\skill_id").ResolveUol().GetValueEx<int>(0);
                                        if (!FifthJobSkillToJobID.ContainsKey(skillIDValue)) FifthJobSkillToJobID.Add(skillIDValue, [0]);
                                    }
                                    else
                                    {
                                        foreach (Wz_Node skillID in connectSkill.Nodes)
                                        {
                                            int skillIDValue = skillID.GetValueEx<int>(0);
                                            if (skillIDValue > 0 && !FifthJobSkillToJobID.ContainsKey(skillIDValue))
                                            {
                                                FifthJobSkillToJobID.Add(skillIDValue, applicableJobID);
                                            }
                                        }
                                    }
                                }
                            }
                            SkillTooltipRender2 tooltip = new SkillTooltipRender2();
                            tooltip.StringLinker = this.stringLinker;
                            tooltip.ShowObjectID = Setting.Skill.ShowID;
                            tooltip.ShowDelay = Setting.Skill.ShowDelay;
                            tooltip.IgnoreEvalError = Setting.Skill.IgnoreEvalError;
                            tooltip.Enable22AniStyle = Setting.Enable22AniStyle;
                            foreach (var i in selectedJob)
                            {
                                var jobImg = PluginManager.FindWz($"Skill\\{i:D3}.img\\skill");
                                if (jobImg == null)
                                {
                                    continue;
                                }
                                foreach (var j in jobImg.Nodes)
                                {
                                    StringResult sr;
                                    string skillName;
                                    if (tooltip.StringLinker == null || !tooltip.StringLinker.StringSkill.TryGetValue(int.Parse(j.Text), out sr))
                                    {
                                        sr = new StringResultSkill();
                                        sr.Name = "未知的技能";
                                    }
                                    skillName = sr.Name;
                                    labelX2.Text = string.Format("匯出中：{0} - {1}", j.Text, skillName);
                                    Skill skill = Skill.CreateFromNode(j, PluginManager.FindWz, PluginManager.FindWz);
                                    bool isPerJobSkill = false;
                                    if (skill != null)
                                    {
                                        skill.Level = skill.MaxLevel;
                                        tooltip.Skill = skill;
                                        isPerJobSkill = skill.PerJobAttackInfo.Count > 0;
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                    if (isPerJobSkill)
                                    {
                                        for (int jobIndex = 0; jobIndex < skill.PerJobAttackInfo.Count; jobIndex++)
                                        {
                                            skill.PerJobIndex = jobIndex;
                                            Bitmap resultImage = tooltip.Render();
                                            int jobID = skill.PerJobAttackInfo.Keys.ToList()[jobIndex];
                                            string categoryPath = ItemStringHelper.GetJobName((i / 10000) == 5 ? jobID + 2 : jobID) ?? ItemStringHelper.GetJobName((i / 10000) == 5 ? jobID + 3 : jobID) ?? ItemStringHelper.GetJobName(jobID) ?? "其它";
                                            if (!Directory.Exists(Path.Combine(exportedFolder, categoryPath)))
                                            {
                                                Directory.CreateDirectory(Path.Combine(exportedFolder, categoryPath));
                                            }
                                            string imageName = Path.Combine(exportedFolder, categoryPath, "skill_" + j.Text + "_" + RemoveInvalidFileNameChars(skillName) + ".png");
                                            if (File.Exists(imageName)) File.Delete(imageName);
                                            resultImage.Save(imageName, System.Drawing.Imaging.ImageFormat.Png);
                                            resultImage.Dispose();
                                        }
                                    }
                                    else
                                    {
                                        Bitmap resultImage = tooltip.Render();
                                        string categoryPath = "";
                                        if (FifthJobSkillToJobID.ContainsKey(int.Parse(j.Text)))
                                        {
                                            categoryPath = ItemStringHelper.GetFifthJobName(int.Parse(j.Text), FifthJobSkillToJobID[int.Parse(j.Text)]);
                                        }
                                        else
                                        {
                                            categoryPath = ItemStringHelper.GetJobName(i) ?? "其它";
                                        }
                                        if (!Directory.Exists(Path.Combine(exportedFolder, categoryPath)))
                                        {
                                            Directory.CreateDirectory(Path.Combine(exportedFolder, categoryPath));
                                        }
                                        string imageName = Path.Combine(exportedFolder, categoryPath, "skill_" + j.Text + "_" + RemoveInvalidFileNameChars(skillName) + ".png");
                                        if (File.Exists(imageName)) File.Delete(imageName);
                                        resultImage.Save(imageName, System.Drawing.Imaging.ImageFormat.Png);
                                        resultImage.Dispose();
                                    }
                                }
                                if (FifthJobSkillToJobID.Count > 0)
                                {
                                    foreach (var kvp in FifthJobSkillToJobID)
                                    {
                                        if (kvp.Value.Contains(i))
                                        {
                                            var skillNode = PluginManager.FindWz($"Skill\\{kvp.Key / 10000}.img\\skill\\{kvp.Key}");
                                            if (skillNode == null)
                                            {
                                                continue;
                                            }
                                            StringResult sr;
                                            string skillName;
                                            if (tooltip.StringLinker == null || !tooltip.StringLinker.StringSkill.TryGetValue(int.Parse(skillNode.Text), out sr))
                                            {
                                                sr = new StringResultSkill();
                                                sr.Name = "未知的技能";
                                            }
                                            skillName = sr.Name;
                                            labelX2.Text = string.Format("匯出中：{0} - {1}", skillNode.Text, skillName);
                                            Skill skill = Skill.CreateFromNode(skillNode, PluginManager.FindWz, PluginManager.FindWz);
                                            if (skill != null)
                                            {
                                                skill.Level = skill.MaxLevel;
                                                tooltip.Skill = skill;
                                            }
                                            else
                                            {
                                                continue;
                                            }
                                            Bitmap resultImage = tooltip.Render();
                                            string categoryPath = ItemStringHelper.GetFifthJobName(kvp.Key, kvp.Value) ?? "其它";
                                            if (!Directory.Exists(Path.Combine(exportedFolder, categoryPath)))
                                            {
                                                Directory.CreateDirectory(Path.Combine(exportedFolder, categoryPath));
                                            }
                                            string imageName = Path.Combine(exportedFolder, categoryPath, "skill_" + skillNode.Text + "_" + RemoveInvalidFileNameChars(skillName) + ".png");
                                            if (File.Exists(imageName)) File.Delete(imageName);
                                            resultImage.Save(imageName, System.Drawing.Imaging.ImageFormat.Png);
                                            resultImage.Dispose();
                                        }
                                    }
                                }
                            }
                            if (doRoguelikePharaoh)
                            {
                                var jobImg = PluginManager.FindWz($"Skill\\Roguelike\\Skill");
                                if (jobImg != null)
                                {
                                    foreach (var i in jobImg.Nodes)
                                    {
                                        StringResult sr;
                                        string skillName;
                                        Wz_Image image;
                                        if ((image = i.GetValue<Wz_Image>()) == null || !image.TryExtract())
                                            return;
                                        if (tooltip.StringLinker == null || !tooltip.StringLinker.StringRoguelikeSkill.TryGetValue(int.Parse(i.Text.Replace(".img", "")), out sr))
                                        {
                                            sr = new StringResultSkill();
                                            sr.Name = "未知的技能";
                                        }
                                        skillName = sr.Name;
                                        labelX2.Text = string.Format("匯出中: {0} - {1}", i.Text.Replace(".img", ""), skillName);
                                        Skill skill = Skill.CreateFromNode(image.Node, PluginManager.FindWz, PluginManager.FindWz);
                                        if (skill != null)
                                        {
                                            skill.Level = skill.MaxLevel;
                                            tooltip.Skill = skill;
                                        }
                                        else
                                        {
                                            continue;
                                        }
                                        Bitmap resultImage = tooltip.Render();
                                        string categoryPath = "Roguelike技能(法老王的寶物)";
                                        if (!Directory.Exists(Path.Combine(exportedFolder, categoryPath)))
                                        {
                                            Directory.CreateDirectory(Path.Combine(exportedFolder, categoryPath));
                                        }
                                        string imageName = Path.Combine(exportedFolder, categoryPath, "skill_" + i.Text.Replace(".img", "") + "_" + RemoveInvalidFileNameChars(skillName) + ".png");
                                        if (File.Exists(imageName)) File.Delete(imageName);
                                        resultImage.Save(imageName, System.Drawing.Imaging.ImageFormat.Png);
                                        resultImage.Dispose();
                                    }
                                }
                            }
                            if (doRedmoon)
                            {
                                var jobImg = PluginManager.FindWz($"Skill\\Roguelike\\Skill\\Redmoon");
                                if (jobImg != null)
                                {
                                    foreach (var i in jobImg.Nodes)
                                    {
                                        StringResult sr;
                                        string skillName;
                                        Wz_Image image;
                                        if ((image = i.GetValue<Wz_Image>()) == null || !image.TryExtract())
                                            continue;
                                        if (tooltip.StringLinker == null || !tooltip.StringLinker.StringRoguelikeSkill.TryGetValue(int.Parse(i.Text.Replace(".img", "")), out sr))
                                        {
                                            sr = new StringResultSkill();
                                            sr.Name = "未知的技能";
                                        }
                                        skillName = sr.Name;
                                        labelX2.Text = string.Format("匯出中: {0} - {1}", i.Text.Replace(".img", ""), skillName);
                                        Skill skill = Skill.CreateFromNode(image.Node, PluginManager.FindWz, PluginManager.FindWz);
                                        if (skill != null)
                                        {
                                            skill.Level = skill.MaxLevel;
                                            tooltip.Skill = skill;
                                        }
                                        else
                                        {
                                            continue;
                                        }
                                        Bitmap resultImage = tooltip.Render();
                                        string categoryPath = "Roguelike技能(紅月之森)";
                                        if (!Directory.Exists(Path.Combine(exportedFolder, categoryPath)))
                                        {
                                            Directory.CreateDirectory(Path.Combine(exportedFolder, categoryPath));
                                        }
                                        string imageName = Path.Combine(exportedFolder, categoryPath, "skill_" + i.Text.Replace(".img", "") + "_" + RemoveInvalidFileNameChars(skillName) + ".png");
                                        if (File.Exists(imageName)) File.Delete(imageName);
                                        resultImage.Save(imageName, System.Drawing.Imaging.ImageFormat.Png);
                                        resultImage.Dispose();
                                    }
                                }
                            }
                            if (doGuildCastleGuildResearch)
                            {
                                var jobImg = PluginManager.FindWz($"Etc\\GuildCastle.img\\ResearchList\\Guild");
                                if (jobImg != null)
                                {
                                    foreach (var i in jobImg.Nodes)
                                    {
                                        StringResult sr;
                                        string skillName;
                                        if (tooltip.StringLinker == null || !tooltip.StringLinker.StringGuildCastleGuildResearch.TryGetValue(int.Parse(i.Text.Replace(".img", "")), out sr))
                                        {
                                            sr = new StringResultSkill();
                                            sr.Name = "未知的研究";
                                        }
                                        skillName = sr.Name;
                                        labelX2.Text = string.Format("匯出中：{0} - {1}", i.Text.Replace(".img", ""), skillName);
                                        Skill skill = Skill.CreateFromNode(i, PluginManager.FindWz, PluginManager.FindWz);
                                        if (skill != null)
                                        {
                                            skill.Level = skill.MaxLevel;
                                            tooltip.Skill = skill;
                                        }
                                        else
                                        {
                                            continue;
                                        }
                                        Bitmap resultImage = tooltip.Render();
                                        string categoryPath = "公會城研究(共通研究)";
                                        if (!Directory.Exists(Path.Combine(exportedFolder, categoryPath)))
                                        {
                                            Directory.CreateDirectory(Path.Combine(exportedFolder, categoryPath));
                                        }
                                        string imageName = Path.Combine(exportedFolder, categoryPath, "研究_" + i.Text.Replace(".img", "") + "_" + RemoveInvalidFileNameChars(skillName) + ".png");
                                        if (File.Exists(imageName)) File.Delete(imageName);
                                        resultImage.Save(imageName, System.Drawing.Imaging.ImageFormat.Png);
                                        resultImage.Dispose();
                                    }
                                }
                            }
                            if (doGuildCastlePersonalResearch)
                            {
                                var jobImg = PluginManager.FindWz($"Etc\\GuildCastle.img\\ResearchList\\Personal");
                                if (jobImg != null)
                                {
                                    foreach (var i in jobImg.Nodes)
                                    {
                                        StringResult sr;
                                        string skillName;
                                        if (tooltip.StringLinker == null || !tooltip.StringLinker.StringGuildCastlePersonalResearch.TryGetValue(int.Parse(i.Text.Replace(".img", "")), out sr))
                                        {
                                            sr = new StringResultSkill();
                                            sr.Name = "未知的研究";
                                        }
                                        skillName = sr.Name;
                                        labelX2.Text = string.Format("匯出中：{0} - {1}", i.Text.Replace(".img", ""), skillName);
                                        Skill skill = Skill.CreateFromNode(i, PluginManager.FindWz, PluginManager.FindWz);
                                        if (skill != null)
                                        {
                                            skill.Level = skill.MaxLevel;
                                            tooltip.Skill = skill;
                                        }
                                        else
                                        {
                                            continue;
                                        }
                                        Bitmap resultImage = tooltip.Render();
                                        string categoryPath = "公會城研究(個人研究)";
                                        if (!Directory.Exists(Path.Combine(exportedFolder, categoryPath)))
                                        {
                                            Directory.CreateDirectory(Path.Combine(exportedFolder, categoryPath));
                                        }
                                        string imageName = Path.Combine(exportedFolder, categoryPath, "研究_" + i.Text.Replace(".img", "") + "_" + RemoveInvalidFileNameChars(skillName) + ".png");
                                        if (File.Exists(imageName)) File.Delete(imageName);
                                        resultImage.Save(imageName, System.Drawing.Imaging.ImageFormat.Png);
                                        resultImage.Dispose();
                                    }
                                }
                            }
                        });
                    }
                    catch (Exception ex)
                    {
                        MessageBoxEx.Show(ex.ToString(), LocalizedString_JP.COMMON_ERROR);
                    }
                    finally
                    {
                        sw.Stop();
                        btnSkillTooltipExport.Enabled = true;
                        labelX2.Text = "匯出完成。時間經過：" + sw.Elapsed.ToString();
                    }
                    labelItemStatus.Text = "匯出完成: " + exportedFolder;
                }
            }
        }

        private void btnCustomCSS_Click(object sender, EventArgs e)
        {
            ConfigManager.Reload();
            var Setting = CustomCSSConfig.Default;
            using (FrmCustomCSS frm = new FrmCustomCSS(styleManager1.ManagerStyle == eStyle.VisualStudio2012Dark))
            {
                frm.LoadConfig(Setting);
                if (frm.ShowDialog() == DialogResult.OK)
                {
                    frm.SaveConfig(Setting);
                    ConfigManager.Save();
                }
            }
        }

        /// <summary>
        /// Gets the avatar code used for the chair / riding mount output. Falls back to the default when unset.
        /// </summary>
        private string GetEasyCompareAvatarCode()
        {
            string code = WcR2Config.Default.EasyCompareAvatarCode;
            return string.IsNullOrEmpty(code) ? EasyComparer.DefaultAvatarCode : code;
        }

        private void btnAvatarCode_Click(object sender, EventArgs e)
        {
            using (FrmAvatarCode frm = new FrmAvatarCode(styleManager1.ManagerStyle == eStyle.VisualStudio2012Dark))
            {
                frm.CodeText = GetEasyCompareAvatarCode();
                if (frm.ShowDialog() == DialogResult.OK)
                {
                    ConfigManager.Reload();
                    WcR2Config.Default.EasyCompareAvatarCode = frm.CodeText;
                    ConfigManager.Save();
                }
            }
        }

        private void chkOutputChairAndRidingMount_CheckedChanged(object sender, EventArgs e)
        {
            btnAvatarCode.Enabled = chkOutputChairAndRidingMount.Checked;
        }

        private void btnExportSkillOption_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dlg = new FolderBrowserDialog();
            dlg.Description = "選擇要匯出到的資料夾。";
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                if (!this.stringLinker.HasValues)
                    this.LoadStringLinker();

                DBConnection conn = new DBConnection(this.stringLinker);
                conn.ExportSkillOption(dlg.SelectedPath);
                MessageBoxEx.Show("匯出完成。");
            }
        }

        private void buttonItemAutoQuickView_Click(object sender, EventArgs e)
        {
            ConfigManager.Reload();
            CharaSimConfig.Default.AutoQuickView = buttonItemAutoQuickView.Checked;
            ConfigManager.Save();
        }

        private void panelExLeft_SizeChanged(object sender, EventArgs e)
        {
            if (this.WindowState != FormWindowState.Minimized)
            {
                if (panelExLeft.Tag is int)
                {
                    int oldHeight = (int)panelExLeft.Tag;
                    advTree1.Height = (int)(1.0 * advTree1.Height / oldHeight * panelExLeft.Height);
                }
                panelExLeft.Tag = panelExLeft.Height;
            }
        }

        private void buttonItem1_Click(object sender, EventArgs e)
        {
#if NET6_0_OR_GREATER
            Process.Start(new ProcessStartInfo
            {
                UseShellExecute = true,
                FileName = "https://github.com/HikariCalyx/WzComparerR2-JMS/issues",
            });
#else
            Process.Start("https://github.com/HikariCalyx/WzComparerR2-JMS/issues");
#endif
        }

        private void btnSetupWizard_Click(object sender, EventArgs e)
        {
            RunSetupWizard();
        }

        private void btnDiscordServer_Click(object sender, EventArgs e)
        {
#if NET6_0_OR_GREATER
            Process.Start(new ProcessStartInfo
            {
                UseShellExecute = true,
                FileName = "https://api.hikaricalyx.com/WcR2-JMS/v1/DiscordInvitation",
            });
#else
            Process.Start("https://api.hikaricalyx.com/WcR2-JMS/v1/DiscordInvitation");
#endif
        }

        private void labelItemStatus_TextChanged(object sender, EventArgs e)
        {
            ribbonBar2.RecalcLayout();
        }

        private void btnNodeBack_Click(object sender, EventArgs e)
        {

        }

        private void btnNodeForward_Click(object sender, EventArgs e)
        {

        }

        private void buttonItemUpdate_Click(object sender, EventArgs e)
        {
            var frm = new FrmUpdater(new Updater(), styleManager1.ManagerStyle == eStyle.VisualStudio2012Dark);
            frm.LoadConfig(WcR2Config.Default);
            frm.ShowDialog();
        }

        private void btnItemOptions_Click(object sender, System.EventArgs e)
        {
            var frm = new FrmOptions();
            frm.Load(WcR2Config.Default);
            if (frm.ShowDialog() == DialogResult.OK)
            {
                ConfigManager.Reload();
                frm.Save(WcR2Config.Default);
                ConfigManager.Save();
                UpdateWzLoadingSettings();
                UpdateTranslateSettings();
                UpdateDiscordConfig();
            }
        }
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            foreach (Form form in Application.OpenForms)
            {
                if (form is FrmPatcher && !form.IsDisposed)
                {
                    form.Show();
                    form.BringToFront();
                    MessageBoxEx.Show(this, "退出前請先關閉更新程式。", "注意", MessageBoxButtons.OK);
                    e.Cancel = true;
                    return;
                }
            }
            if (compareThread != null)
            {
                if (DialogResult.Yes == MessageBoxEx.Show(this, "比較正在進行中。你想中斷嗎？", "注意", MessageBoxButtons.YesNo))
                {
                    compareThread.Interrupt();
                    compareThread = null;
                    GC.Collect();
                    e.Cancel = true;
                    return;
                }
                else
                {
                    e.Cancel = true;
                    return;
                }
            }
            DialogResult result = MessageBoxEx.Show(this, "是否要退出？", "確認", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                e.Cancel = false;  //点击OK
            }
            else
            {
                e.Cancel = true;
            }
        }

        private async void MainForm_Shown(object sender, EventArgs e)
        {
            // Run Setup Wizard
            if (!WcR2Config.Default.IsSetupWizardCompleted)
            {
                RunSetupWizard(true);
                ConfigManager.Reload();
                WcR2Config.Default.IsSetupWizardCompleted = true;
                ConfigManager.Save();
            }
            await this.AutomaticCheckUpdate();
        }

        private void buttomItem13_FormClosing(object sender, EventArgs e)
        {
            this.Close();
        }

        private static bool IsUriSchemeRegistered(string scheme)
        {
            string registryKey = $@"HKEY_CLASSES_ROOT\{scheme}";
            object keyValue = Registry.GetValue(registryKey, "", null);
            return keyValue != null;
        }

        private static string RemoveInvalidFileNameChars(string fileName)
        {
            if (String.IsNullOrEmpty(fileName)) return "未知";
            string invalidChars = new string(System.IO.Path.GetInvalidFileNameChars());
            string regexPattern = $"[{Regex.Escape(invalidChars)}]";
            return Regex.Replace(fileName, regexPattern, "_");
        }
        
        private void colorPickerPicBoxBgColor_SelectedColorChanged(object sender, EventArgs e)
        {
            this.pictureBoxEx1.BackColor = ((ColorPickerDropDown)sender).SelectedColor;
        }
    }

    #region 内部用扩展方法
    internal static partial class Ext
    {
        public static Wz_Node AsWzNode(this Node node)
        {
            return (node?.Tag as WeakReference)?.Target as Wz_Node;
        }

        public static void ClearLayoutCellInfo(this AdvTree advTree)
        {
            var bindingPrivateField = BindingFlags.NonPublic | BindingFlags.Instance;
            {
                var field1 = advTree.GetType().GetField("Ֆ", bindingPrivateField);
                var obj1 = field1.GetValue(advTree);
                if (obj1 != null)
                {
                    var field2 = obj1.GetType().BaseType.GetField("ӹ", bindingPrivateField);
                    var obj2 = field2.GetValue(obj1);
                    if (obj2 != null)
                    {
                        var field3 = obj2.GetType().GetField("ܦ", bindingPrivateField);
                        var obj3 = field3.GetValue(obj2);
                        if (obj3 != null)
                        {
                            field3.SetValue(obj2, null);
                        }
                    }
                }
            }
            {
                var display = advTree.NodeDisplay as NodeTreeDisplay;
                if (display != null)
                {
                    var field4 = display.GetType().GetField("☼", bindingPrivateField);
                    var obj4 = field4.GetValue(display) as NodeCellRendererEventArgs;
                    if (obj4 != null)
                    {
                        obj4.Node = null;
                        obj4.Cell = null;
                    }
                }
            }
        }
    }
    #endregion
}

