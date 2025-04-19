using System;
using System.Linq;
using System.Drawing;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows.Forms;
using System.Text;
using System.Text.RegularExpressions;
using CharaSimResource;
using WzComparerR2.CharaSim;
using WzComparerR2.Common;
using WzComparerR2.Controls;
using WzComparerR2.WzLib;
using System.Security.Cryptography.X509Certificates;
using System.Runtime.InteropServices;
using System.Threading;

namespace WzComparerR2.CharaSimControl
{
    public class AfrmStat : AlphaForm
    {
        public AfrmStat()
        {
            sec = new int[2];
            for (int i = 0; i < sec.Length; i++)
                sec[i] = 1 << i;

            hyperStatList = new int[] { 80000400, 80000401, 80000402, 80000403, 80000404, 80000405, 80000406, 80000409, 80000410, 80000412, 80000413, 80000414, 80000416, 80000419, 80000420, 80000421, 80000422 };
            hyperStatBitmapList = hyperStatList.Select(id => Resource.ResourceManager.GetObject("UICharacterInfo_img_common_detailStat_HyperStat_Window_statList_" + id) as Bitmap).ToArray();

            initCtrl();
        }

        private BitVector32 partVisible;
        private int[] sec;
        private Point baseOffset;
        private Point newLocation;
        private Character character;
        private List<TooltipHelpRect> helpList;
        private List<TooltipHelpRect> helpDetailList;
        private List<TooltipHelpRect> detailStatList;
        private int hyperStatScrollValue;
        private int[] hyperStatList;
        private Bitmap[] hyperStatBitmapList;
        private Skill[] hyperStatSkillList;

        private ACtrlButton btnClose;
        private ACtrlButton btntoggleDetailOpen;
        private ACtrlButton btntoggleDetailClose;
        private ACtrlButton btnDetailOpen;
        private ACtrlButton btnDetailClose;
        private ACtrlButton btnHyperStatOpen;
        private ACtrlButton btnHyperStatClose;
        private ACtrlButton btnhelp;
        private ACtrlButton btnparty;
        private ACtrlButton btnguild;
        private ACtrlButton btnexchange;
        private ACtrlButton btnmyhome;
        private ACtrlButton btnpopularityUp;
        private ACtrlButton btnpopularityDown;
        private ACtrlButton btndetailTab;
        private ACtrlButton btndetailTab2;
        private ACtrlButton btndetailTab3;
        private ACtrlButton btndetailTab4;
        private ACtrlButton btnpresetPage1;
        private ACtrlButton btnpresetPage2;
        private ACtrlButton btnpresetPage3;
        private ACtrlButton btnStatFont;
        private ACtrlButton btnArc;
        private ACtrlButton btnAut;
        private ACtrlButton btnLinkSkill;
        private ACtrlButton btnVSkill;
        private ACtrlButton btnHexaSkill;
        private ACtrlButton btnCash;
        private ACtrlButton btnCashPreset;
        private ACtrlButton btnHpUp;
        private ACtrlButton btnLVUp1;
        private ACtrlButton btnLVUp2;
        private ACtrlButton btnLVUp3;
        private ACtrlButton btnLVUp4;
        private ACtrlButton btnLVUp5;
        private ACtrlButton btnLVUp6;
        private ACtrlButton btnLVUp7;
        private ACtrlButton btnLVUp8;
        private ACtrlButton btnLVUp9;
        private ACtrlButton btnLVUp10;
        private ACtrlButton btnLVUp11;
        private ACtrlButton btnLVUp12;
        private ACtrlButton btnReduce;
        private bool waitForRefresh;

        public event ObjectMouseEventHandler ObjectMouseMove;
        public event EventHandler ObjectMouseLeave;

        public Character Character
        {
            get { return character; }
            set { character = value; }
        }

        public bool AbilityVisible
        {
            get { return partVisible[sec[0]]; }
            private set { partVisible[sec[0]] = value; }
        }

        public bool HyperStatVisible
        {
            get { return partVisible[sec[1]]; }
            private set { partVisible[sec[1]] = value; }
        }

        public bool DetailVisible = false;
        public bool StatVisible = false;
        public bool EquipVisible = false;
        public bool SkillVisible = false;
        public bool CashVisible = false;
        public int statFont = 1;
        public int presetPage = 1;
        public int ArcAut = 1;
        public int SkillTab = 1;
        public int CashPreset = 1;
        public int hyperLv = 0;
        public int hyperLv2 = 0;
        public int hyperLv3 = 0;
        public int hyperLv4 = 0;
        public int hyperLv5 = 0;
        public int hyperLv6 = 0;
        public int hyperLv7 = 0;
        public int hyperLv8 = 0;
        public int hyperLv9 = 0;
        public int hyperLv10 = 0;
        public int hyperLv11 = 0;
        public int hyperLv12 = 0;
        public int hyperLv13 = 0;
        public int hyperLv14 = 0;
        public int hyperLv15 = 0;
        public int hyperLv16 = 0;
        public int hyperLv17 = 0;

        private Rectangle DetailRect
        {
            get
            {
                return new Rectangle(
                    new Point(baseOffset.X + Resource.UICharacterInfo_img_common_main_backgrnd.Width + 1, baseOffset.Y + 1),
                    Resource.UICharacterInfo_img_remote_detailStat_ability_backgrnd.Size);
            }
        }

        private Rectangle HyperStatRect
        {
            get
            {
                return new Rectangle(
                    new Point(baseOffset.X - Resource.UICharacterInfo_img_remote_detailStat_HyperStat_Window_backgrnd.Width - 1, baseOffset.Y + 709 - Resource.UICharacterInfo_img_remote_detailStat_HyperStat_Window_backgrnd.Height),
                    Resource.UICharacterInfo_img_remote_detailStat_HyperStat_Window_backgrnd.Size);
            }
        }

        private void initCtrl()
        {
            this.btnClose = new ACtrlButton(); //主页关闭按钮
            this.btnClose.Normal = new BitmapOrigin(Resource.UICharacterInfo_img_common_main_buttonclose_normal_0);
            this.btnClose.Pressed = new BitmapOrigin(Resource.UICharacterInfo_img_common_main_buttonclose_pressed_0);
            this.btnClose.MouseOver = new BitmapOrigin(Resource.UICharacterInfo_img_common_main_buttonclose_mouseOver_0);
            this.btnClose.Disabled = new BitmapOrigin(Resource.UICharacterInfo_img_common_main_buttonclose_disabled_0);
            this.btnClose.Location = new Point(449, 12);
            this.btnClose.Size = new Size(11, 11);
            this.btnClose.ButtonStateChanged += new EventHandler(aCtrl_RefreshCall);
            this.btnClose.MouseClick += new MouseEventHandler(btnClose_MouseClick);

            this.btntoggleDetailOpen = new ACtrlButton();  //详情按钮开启
            this.btntoggleDetailOpen.Normal = new BitmapOrigin(Resource.UICharacterInfo_img_common_main_buttontoggleDetail_normal_0);
            this.btntoggleDetailOpen.Pressed = new BitmapOrigin(Resource.UICharacterInfo_img_common_main_buttontoggleDetail_pressed_0);
            this.btntoggleDetailOpen.MouseOver = new BitmapOrigin(Resource.UICharacterInfo_img_common_main_buttontoggleDetail_mouseOver_0);
            this.btntoggleDetailOpen.Disabled = new BitmapOrigin(Resource.UICharacterInfo_img_common_main_buttontoggleDetail_disabled_0);
            this.btntoggleDetailOpen.Location = new Point(10, 201);
            this.btntoggleDetailOpen.Size = new Size(452, 21);
            this.btntoggleDetailOpen.ButtonStateChanged += new EventHandler(aCtrl_RefreshCall);
            this.btntoggleDetailOpen.MouseClick += new MouseEventHandler(btntoggleDetailOpen_MouseClick);

            this.btntoggleDetailClose = new ACtrlButton(); //详情按钮关闭
            this.btntoggleDetailClose.Normal = new BitmapOrigin(Resource.UICharacterInfo_img_common_main_buttontoggleDetail_normal_0);
            this.btntoggleDetailClose.Pressed = new BitmapOrigin(Resource.UICharacterInfo_img_common_main_buttontoggleDetail_pressed_0);
            this.btntoggleDetailClose.MouseOver = new BitmapOrigin(Resource.UICharacterInfo_img_common_main_buttontoggleDetail_mouseOver_0);
            this.btntoggleDetailClose.Disabled = new BitmapOrigin(Resource.UICharacterInfo_img_common_main_buttontoggleDetail_disabled_0);
            this.btntoggleDetailClose.Location = new Point(10, 201);
            this.btntoggleDetailClose.Size = new Size(452, 21);
            this.btntoggleDetailClose.ButtonStateChanged += new EventHandler(aCtrl_RefreshCall);
            this.btntoggleDetailClose.MouseClick += new MouseEventHandler(btntoggleDetailClose_MouseClick);

            this.btnDetailOpen = new ACtrlButton();  //内在能力按钮开启
            this.btnDetailOpen.Normal = new BitmapOrigin(Resource.UICharacterInfo_img_common_detailStat_buttonability_normal_0);
            this.btnDetailOpen.Pressed = new BitmapOrigin(Resource.UICharacterInfo_img_common_detailStat_buttonability_pressed_0);
            this.btnDetailOpen.MouseOver = new BitmapOrigin(Resource.UICharacterInfo_img_common_detailStat_buttonability_mouseOver_0);
            this.btnDetailOpen.Disabled = new BitmapOrigin(Resource.UICharacterInfo_img_common_detailStat_buttonability_disabled_0);
            this.btnDetailOpen.Location = new Point(355, 679);
            this.btnDetailOpen.Size = new Size(106, 24);
            this.btnDetailOpen.ButtonStateChanged += new EventHandler(aCtrl_RefreshCall);
            this.btnDetailOpen.MouseClick += new MouseEventHandler(btnDetailOpen_MouseClick);

            this.btnDetailClose = new ACtrlButton();  //内在能力按钮关闭
            this.btnDetailClose.Normal = new BitmapOrigin(Resource.UICharacterInfo_img_common_detailStat_buttonability_normal_0);
            this.btnDetailClose.Pressed = new BitmapOrigin(Resource.UICharacterInfo_img_common_detailStat_buttonability_pressed_0);
            this.btnDetailClose.MouseOver = new BitmapOrigin(Resource.UICharacterInfo_img_common_detailStat_buttonability_mouseOver_0);
            this.btnDetailClose.Disabled = new BitmapOrigin(Resource.UICharacterInfo_img_common_detailStat_buttonability_disabled_0);
            this.btnDetailClose.Location = new Point(355, 679);
            this.btnDetailClose.Size = new Size(106, 24);
            this.btnDetailClose.ButtonStateChanged += new EventHandler(aCtrl_RefreshCall);
            this.btnDetailClose.MouseClick += new MouseEventHandler(btnDetailClose_MouseClick);

            this.btnHyperStatOpen = new ACtrlButton();  //超级属性按钮开启
            this.btnHyperStatOpen.Normal = new BitmapOrigin(Resource.UICharacterInfo_img_common_detailStat_buttonhyper_normal_0);
            this.btnHyperStatOpen.Pressed = new BitmapOrigin(Resource.UICharacterInfo_img_common_detailStat_buttonhyper_pressed_0);
            this.btnHyperStatOpen.MouseOver = new BitmapOrigin(Resource.UICharacterInfo_img_common_detailStat_buttonhyper_mouseOver_0);
            this.btnHyperStatOpen.Disabled = new BitmapOrigin(Resource.UICharacterInfo_img_common_detailStat_buttonhyper_disabled_0);
            this.btnHyperStatOpen.Location = new Point(12, 679);
            this.btnHyperStatOpen.Size = new Size(106, 24);
            this.btnHyperStatOpen.ButtonStateChanged += new EventHandler(aCtrl_RefreshCall);
            this.btnHyperStatOpen.MouseClick += new MouseEventHandler(btnHyperStatOpen_MouseClick);

            this.btnHyperStatClose = new ACtrlButton();  //超级属性按钮关闭
            this.btnHyperStatClose.Normal = new BitmapOrigin(Resource.UICharacterInfo_img_common_detailStat_buttonhyper_normal_0);
            this.btnHyperStatClose.Pressed = new BitmapOrigin(Resource.UICharacterInfo_img_common_detailStat_buttonhyper_pressed_0);
            this.btnHyperStatClose.MouseOver = new BitmapOrigin(Resource.UICharacterInfo_img_common_detailStat_buttonhyper_mouseOver_0);
            this.btnHyperStatClose.Disabled = new BitmapOrigin(Resource.UICharacterInfo_img_common_detailStat_buttonhyper_disabled_0);
            this.btnHyperStatClose.Location = new Point(12, 679);
            this.btnHyperStatClose.Size = new Size(106, 24);
            this.btnHyperStatClose.ButtonStateChanged += new EventHandler(aCtrl_RefreshCall);
            this.btnHyperStatClose.MouseClick += new MouseEventHandler(btnHyperStatClose_MouseClick);

            this.btnHpUp = new ACtrlButton();  //内在能力按钮关闭(X)
            this.btnHpUp.Normal = new BitmapOrigin(Resource.UICharacterInfo_img_common_main_buttonclose_normal_0);
            this.btnHpUp.Pressed = new BitmapOrigin(Resource.UICharacterInfo_img_common_main_buttonclose_pressed_0);
            this.btnHpUp.MouseOver = new BitmapOrigin(Resource.UICharacterInfo_img_common_main_buttonclose_mouseOver_0);
            this.btnHpUp.Disabled = new BitmapOrigin(Resource.UICharacterInfo_img_common_main_buttonclose_disabled_0);
            this.btnHpUp.Location = new Point(222, Resource.UICharacterInfo_img_common_main_backgrnd.Height + Resource.UICharacterInfo_img_local_detail_backgrnd.Height - Resource.UICharacterInfo_img_remote_detailStat_ability_backgrnd.Height + 10);
            this.btnHpUp.Size = new Size(11, 11);
            this.btnHpUp.ButtonStateChanged += new EventHandler(aCtrl_RefreshCall);
            this.btnHpUp.MouseClick += new MouseEventHandler(btnDetailClose_MouseClick);

            this.btnReduce = new ACtrlButton();  //超级属性按钮关闭(X)
            this.btnReduce.Normal = new BitmapOrigin(Resource.UICharacterInfo_img_common_main_buttonclose_normal_0);
            this.btnReduce.Pressed = new BitmapOrigin(Resource.UICharacterInfo_img_common_main_buttonclose_pressed_0);
            this.btnReduce.MouseOver = new BitmapOrigin(Resource.UICharacterInfo_img_common_main_buttonclose_mouseOver_0);
            this.btnReduce.Disabled = new BitmapOrigin(Resource.UICharacterInfo_img_common_main_buttonclose_disabled_0);
            this.btnReduce.Location = new Point(192, 11);
            this.btnReduce.Size = new Size(11, 11);
            this.btnReduce.ButtonStateChanged += new EventHandler(aCtrl_RefreshCall);
            this.btnReduce.MouseClick += new MouseEventHandler(btnHyperStatClose_MouseClick);

            this.btnhelp = new ACtrlButton();  //帮助按钮
            this.btnhelp.Normal = new BitmapOrigin(Resource.UICharacterInfo_img_common_detailStat_buttonhelp_normal_0);
            this.btnhelp.Pressed = new BitmapOrigin(Resource.UICharacterInfo_img_common_detailStat_buttonhelp_pressed_0);
            this.btnhelp.MouseOver = new BitmapOrigin(Resource.UICharacterInfo_img_common_detailStat_buttonhelp_mouseOver_0);
            this.btnhelp.Disabled = new BitmapOrigin(Resource.UICharacterInfo_img_common_detailStat_buttonhelp_disabled_0);
            this.btnhelp.Location = new Point(430, 276);
            this.btnhelp.Size = new Size(112, 20);
            this.btnhelp.ButtonStateChanged += new EventHandler(aCtrl_RefreshCall);

            this.btnparty = new ACtrlButton();  //组队邀请按钮
            this.btnparty.Normal = new BitmapOrigin(Resource.UICharacterInfo_img_remote_main_buttonparty_normal_0);
            this.btnparty.Pressed = new BitmapOrigin(Resource.UICharacterInfo_img_remote_main_buttonparty_pressed_0);
            this.btnparty.MouseOver = new BitmapOrigin(Resource.UICharacterInfo_img_remote_main_buttonparty_mouseOver_0);
            this.btnparty.Disabled = new BitmapOrigin(Resource.UICharacterInfo_img_remote_main_buttonparty_disabled_0);
            this.btnparty.Location = new Point(341, 102);
            this.btnparty.Size = new Size(112, 20);
            this.btnparty.ButtonStateChanged += new EventHandler(aCtrl_RefreshCall);

            this.btnguild = new ACtrlButton();  //家族按钮
            this.btnguild.Normal = new BitmapOrigin(Resource.UICharacterInfo_img_common_main_buttonguild_normal_0);
            this.btnguild.Pressed = new BitmapOrigin(Resource.UICharacterInfo_img_common_main_buttonguild_pressed_0);
            this.btnguild.MouseOver = new BitmapOrigin(Resource.UICharacterInfo_img_common_main_buttonguild_mouseOver_0);
            this.btnguild.Disabled = new BitmapOrigin(Resource.UICharacterInfo_img_common_main_buttonguild_disabled_0);
            this.btnguild.Location = new Point(341, 124);
            this.btnguild.Size = new Size(112, 20);
            this.btnguild.ButtonStateChanged += new EventHandler(aCtrl_RefreshCall);

            this.btnexchange = new ACtrlButton();  //交换申请按钮
            this.btnexchange.Normal = new BitmapOrigin(Resource.UICharacterInfo_img_remote_main_buttonexchange_normal_0);
            this.btnexchange.Pressed = new BitmapOrigin(Resource.UICharacterInfo_img_remote_main_buttonexchange_pressed_0);
            this.btnexchange.MouseOver = new BitmapOrigin(Resource.UICharacterInfo_img_remote_main_buttonexchange_mouseOver_0);
            this.btnexchange.Disabled = new BitmapOrigin(Resource.UICharacterInfo_img_remote_main_buttonexchange_disabled_0);
            this.btnexchange.Location = new Point(19, 102);
            this.btnexchange.Size = new Size(112, 20);
            this.btnexchange.ButtonStateChanged += new EventHandler(aCtrl_RefreshCall);

            this.btnpopularityUp = new ACtrlButton();  //人气度增加按钮
            this.btnpopularityUp.Normal = new BitmapOrigin(Resource.UICharacterInfo_img_remote_main_buttonpopularityUp_normal_0);
            this.btnpopularityUp.Pressed = new BitmapOrigin(Resource.UICharacterInfo_img_remote_main_buttonpopularityUp_pressed_0);
            this.btnpopularityUp.MouseOver = new BitmapOrigin(Resource.UICharacterInfo_img_remote_main_buttonpopularityUp_mouseOver_0);
            this.btnpopularityUp.Disabled = new BitmapOrigin(Resource.UICharacterInfo_img_remote_main_buttonpopularityUp_disabled_0);
            this.btnpopularityUp.Location = new Point(63, 172);
            this.btnpopularityUp.Size = new Size(12, 12);
            this.btnpopularityUp.ButtonStateChanged += new EventHandler(aCtrl_RefreshCall);

            this.btnpopularityDown = new ACtrlButton();  //人气度降低按钮
            this.btnpopularityDown.Normal = new BitmapOrigin(Resource.UICharacterInfo_img_remote_main_buttonpopularityDown_normal_0);
            this.btnpopularityDown.Pressed = new BitmapOrigin(Resource.UICharacterInfo_img_remote_main_buttonpopularityDown_pressed_0);
            this.btnpopularityDown.MouseOver = new BitmapOrigin(Resource.UICharacterInfo_img_remote_main_buttonpopularityDown_mouseOver_0);
            this.btnpopularityDown.Disabled = new BitmapOrigin(Resource.UICharacterInfo_img_remote_main_buttonpopularityDown_disabled_0);
            this.btnpopularityDown.Location = new Point(77, 172);
            this.btnpopularityDown.Size = new Size(12, 12);
            this.btnpopularityDown.ButtonStateChanged += new EventHandler(aCtrl_RefreshCall);

            this.btnmyhome = new ACtrlButton();  //我的小屋按钮
            this.btnmyhome.Normal = new BitmapOrigin(Resource.UICharacterInfo_img_remote_main_buttonmyhome_normal_0);
            this.btnmyhome.Pressed = new BitmapOrigin(Resource.UICharacterInfo_img_remote_main_buttonmyhome_pressed_0);
            this.btnmyhome.MouseOver = new BitmapOrigin(Resource.UICharacterInfo_img_remote_main_buttonmyhome_mouseOver_0);
            this.btnmyhome.Disabled = new BitmapOrigin(Resource.UICharacterInfo_img_remote_main_buttonmyhome_disabled_0);
            this.btnmyhome.Location = new Point(341, 80);
            this.btnmyhome.Size = new Size(112, 20);
            this.btnmyhome.ButtonStateChanged += new EventHandler(aCtrl_RefreshCall);

            this.btndetailTab = new ACtrlButton();
            this.btndetailTab.Location = new Point(11, 242);
            this.btndetailTab.Size = new Size(111, 20);
            this.btndetailTab.ButtonStateChanged += new EventHandler(aCtrl_RefreshCall);
            this.btndetailTab.MouseClick += new MouseEventHandler(btndetailTab_MouseClick);

            this.btndetailTab2 = new ACtrlButton();
            this.btndetailTab2.Location = new Point(124, 242);
            this.btndetailTab2.Size = new Size(111, 20);
            this.btndetailTab2.ButtonStateChanged += new EventHandler(aCtrl_RefreshCall);
            this.btndetailTab2.MouseClick += new MouseEventHandler(btndetailTab2_MouseClick);

            this.btndetailTab3 = new ACtrlButton();
            this.btndetailTab3.Location = new Point(237, 242);
            this.btndetailTab3.Size = new Size(111, 20);
            this.btndetailTab3.ButtonStateChanged += new EventHandler(aCtrl_RefreshCall);
            this.btndetailTab3.MouseClick += new MouseEventHandler(btndetailTab3_MouseClick);

            this.btndetailTab4 = new ACtrlButton();
            this.btndetailTab4.Location = new Point(350, 242);
            this.btndetailTab4.Size = new Size(111, 20);
            this.btndetailTab4.ButtonStateChanged += new EventHandler(aCtrl_RefreshCall);
            this.btndetailTab4.MouseClick += new MouseEventHandler(btndetailTab4_MouseClick);

            this.btnpresetPage1 = new ACtrlButton();
            this.btnpresetPage1.Normal = new BitmapOrigin(Resource.UICharacterInfo_img_remote_detailStat_HyperStat_Window_buttonpresetPage1_normal_0);
            this.btnpresetPage1.MouseOver = new BitmapOrigin(Resource.UICharacterInfo_img_remote_detailStat_HyperStat_Window_buttonpresetPage1_mouseOver_0);
            this.btnpresetPage1.Pressed = new BitmapOrigin(Resource.UICharacterInfo_img_remote_detailStat_HyperStat_Window_buttonpresetPage1_pressed_0);
            this.btnpresetPage1.Location = new Point(136, 421);
            this.btnpresetPage1.Size = new Size(12, 12);
            this.btnpresetPage1.ButtonStateChanged += new EventHandler(aCtrl_RefreshCall);
            this.btnpresetPage1.MouseClick += new MouseEventHandler(btnpresetPage1_MouseClick);

            this.btnpresetPage2 = new ACtrlButton();
            this.btnpresetPage2.Normal = new BitmapOrigin(Resource.UICharacterInfo_img_remote_detailStat_HyperStat_Window_buttonpresetPage2_normal_0);
            this.btnpresetPage2.MouseOver = new BitmapOrigin(Resource.UICharacterInfo_img_remote_detailStat_HyperStat_Window_buttonpresetPage2_mouseOver_0);
            this.btnpresetPage2.Pressed = new BitmapOrigin(Resource.UICharacterInfo_img_remote_detailStat_HyperStat_Window_buttonpresetPage2_pressed_0);
            this.btnpresetPage2.Location = new Point(156, 421);
            this.btnpresetPage2.Size = new Size(12, 12);
            this.btnpresetPage2.ButtonStateChanged += new EventHandler(aCtrl_RefreshCall);
            this.btnpresetPage2.MouseClick += new MouseEventHandler(btnpresetPage2_MouseClick);

            this.btnpresetPage3 = new ACtrlButton();
            this.btnpresetPage3.Normal = new BitmapOrigin(Resource.UICharacterInfo_img_remote_detailStat_HyperStat_Window_buttonpresetPage3_normal_0);
            this.btnpresetPage3.MouseOver = new BitmapOrigin(Resource.UICharacterInfo_img_remote_detailStat_HyperStat_Window_buttonpresetPage3_mouseOver_0);
            this.btnpresetPage3.Pressed = new BitmapOrigin(Resource.UICharacterInfo_img_remote_detailStat_HyperStat_Window_buttonpresetPage3_pressed_0);
            this.btnpresetPage3.Location = new Point(176, 421);
            this.btnpresetPage3.Size = new Size(12, 12);
            this.btnpresetPage3.ButtonStateChanged += new EventHandler(aCtrl_RefreshCall);
            this.btnpresetPage3.MouseClick += new MouseEventHandler(btnpresetPage3_MouseClick);

            this.btnStatFont = new ACtrlButton();
            this.btnStatFont.Location = new Point(220, 658);
            this.btnStatFont.Size = new Size(33, 8);
            this.btnStatFont.ButtonStateChanged += new EventHandler(aCtrl_RefreshCall);
            this.btnStatFont.MouseClick += new MouseEventHandler(btnStatFont_MouseClick);

            this.btnArc = new ACtrlButton();
            this.btnArc.Location = new Point(261, 280);
            this.btnArc.Size = new Size(92, 20);
            this.btnArc.ButtonStateChanged += new EventHandler(aCtrl_RefreshCall);
            this.btnArc.MouseClick += new MouseEventHandler(btnArc_MouseClick);

            this.btnAut = new ACtrlButton();
            this.btnAut.Location = new Point(353, 280);
            this.btnAut.Size = new Size(92, 20);
            this.btnAut.ButtonStateChanged += new EventHandler(aCtrl_RefreshCall);
            this.btnAut.MouseClick += new MouseEventHandler(btnAut_MouseClick);

            this.btnLinkSkill = new ACtrlButton();
            this.btnLinkSkill.Location = new Point(27, 285);
            this.btnLinkSkill.Size = new Size(136, 31);
            this.btnLinkSkill.ButtonStateChanged += new EventHandler(aCtrl_RefreshCall);
            this.btnLinkSkill.MouseClick += new MouseEventHandler(btnLinkSkill_MouseClick);

            this.btnVSkill = new ACtrlButton();
            this.btnVSkill.Location = new Point(168, 285);
            this.btnVSkill.Size = new Size(136, 31);
            this.btnVSkill.ButtonStateChanged += new EventHandler(aCtrl_RefreshCall);
            this.btnVSkill.MouseClick += new MouseEventHandler(btnVSkill_MouseClick);

            this.btnHexaSkill = new ACtrlButton();
            this.btnHexaSkill.Location = new Point(309, 285);
            this.btnHexaSkill.Size = new Size(136, 31);
            this.btnHexaSkill.ButtonStateChanged += new EventHandler(aCtrl_RefreshCall);
            this.btnHexaSkill.MouseClick += new MouseEventHandler(btnHexaSkill_MouseClick);

            this.btnCash = new ACtrlButton();
            this.btnCash.Location = new Point(27, 280);
            this.btnCash.Size = new Size(111, 20);
            this.btnCash.ButtonStateChanged += new EventHandler(aCtrl_RefreshCall);
            this.btnCash.MouseClick += new MouseEventHandler(btnCash_MouseClick);

            this.btnCashPreset = new ACtrlButton();
            this.btnCashPreset.Location = new Point(138, 280);
            this.btnCashPreset.Size = new Size(111, 20);
            this.btnCashPreset.ButtonStateChanged += new EventHandler(aCtrl_RefreshCall);
            this.btnCashPreset.MouseClick += new MouseEventHandler(btnCashPreset_MouseClick);
        }

        private IEnumerable<AControl> aControls
        {
            get
            {
                yield return btnClose;
                yield return btntoggleDetailOpen;
                yield return btntoggleDetailClose;
                yield return btnDetailOpen;
                yield return btnDetailClose;
                yield return btnHyperStatOpen;
                yield return btnHyperStatClose;
                yield return btnhelp;
                yield return btnparty;
                yield return btnguild;
                yield return btnexchange;
                yield return btnpopularityUp;
                yield return btnpopularityDown;
                yield return btnmyhome;
                yield return btndetailTab;
                yield return btndetailTab2;
                yield return btndetailTab3;
                yield return btndetailTab4;
                yield return btnStatFont;
                yield return btnArc;
                yield return btnAut;
                yield return btnLinkSkill;
                yield return btnVSkill;
                yield return btnHexaSkill;
                yield return btnCash;
                yield return btnCashPreset;
            }
        }

        private IEnumerable<AControl> aDetailControls
        {
            get
            {
                yield return btnHpUp;
            }
        }

        private IEnumerable<AControl> aHyperStatControls
        {
            get
            {
                yield return btnReduce;
                yield return btnpresetPage1;
                yield return btnpresetPage2;
                yield return btnpresetPage3;
            }
        }

        public override void Refresh()
        {
            this.preRender();
            this.SetBitmap(this.Bitmap);
            this.CaptionRectangle = new Rectangle(this.baseOffset, new Size(Resource.UICharacterInfo_img_common_main_backgrnd.Width, 24));
            this.Location = newLocation;
            base.Refresh();
        }

        protected override bool captionHitTest(Point point)
        {
            Rectangle rect = this.btnClose.Rectangle;
            rect.Offset(this.baseOffset);
            if (rect.Contains(point))
                return false;
            return base.captionHitTest(point);
        }

        private void preRender()
        {
            if (Bitmap != null)
                Bitmap.Dispose();

            setControlState();

            Point baseOffsetnew = calcRenderBaseOffset();
            Size size = Resource.UICharacterInfo_img_common_main_backgrnd.Size;
            if (this.DetailVisible)
                size = new Size(size.Width, size.Height + Resource.UICharacterInfo_img_local_detail_backgrnd.Height);
            size.Width += baseOffsetnew.X + 2;
            size.Height += 1;
            if (this.AbilityVisible)
                size = new Size(size.Width + Resource.UICharacterInfo_img_remote_detailStat_ability_backgrnd.Width, size.Height);

            this.newLocation = new Point(this.Location.X + this.baseOffset.X - baseOffsetnew.X,
                this.Location.Y + this.baseOffset.Y - baseOffsetnew.Y);
            this.baseOffset = baseOffsetnew;

            //绘制背景
            Bitmap stat = new Bitmap(size.Width, size.Height);
            Graphics g = Graphics.FromImage(stat);
            renderBase(g);
            if (this.DetailVisible)
                renderTab(g);
            if (this.StatVisible)
                renderDetail(g);
            if (this.EquipVisible)
                renderEquip(g);
            if (this.SkillVisible)
                renderSkill(g);
            if (this.CashVisible)
                renderCash(g);
            if (this.AbilityVisible)
                renderAbility(g);
            if (this.HyperStatVisible)
                renderHyperStat(g);

            //绘制按钮
            g.TranslateTransform(baseOffset.X, baseOffset.Y);
            foreach (AControl ctrl in this.aControls)
            {
                ctrl.Draw(g);
            }
            g.ResetTransform();

            g.TranslateTransform(this.DetailRect.X, this.DetailRect.Y);
            foreach (AControl ctrl in this.aDetailControls)
            {
                ctrl.Draw(g);
            }
            g.ResetTransform();

            g.TranslateTransform(this.HyperStatRect.X, this.HyperStatRect.Y);
            foreach (AControl ctrl in this.aHyperStatControls)
            {
                ctrl.Draw(g);
            }
            g.ResetTransform();

            g.Dispose();
            this.Bitmap = stat;

            if (detailStatList == null)
            {
                this.detailStatList = new List<TooltipHelpRect>();
                foreach (Wz_Node helpNode in PluginBase.PluginManager.FindWz("UI/UICharacterInfo.img/common/detailStat/Stat")?.Nodes ?? Enumerable.Empty<Wz_Node>())
                {
                    Wz_Vector lt = helpNode.Nodes["clickRangeLT"]?.Value as Wz_Vector ?? new Wz_Vector(0, 0);
                    Wz_Vector rb = helpNode.Nodes["clickRangeRB"]?.Value as Wz_Vector ?? new Wz_Vector(0, 0);
                    detailStatList.Add(new TooltipHelpRect(new Rectangle(lt.X + 12, lt.Y + 269, rb.X - lt.X, rb.Y - lt.Y), new TooltipHelp(helpNode.Nodes["Title"].GetValueEx<string>(null), helpNode.Nodes["Desc"].GetValueEx<string>(null))));
                }
                if (detailStatList.Count == 0)
                {
                    detailStatList = null;
                }
            }

            if (hyperStatSkillList == null)
            {
                try
                {
                    hyperStatSkillList = hyperStatList.Select(id => id.ToString().PadLeft(7, '0')).Select(id => Skill.CreateFromNode(PluginBase.PluginManager.FindWz("Skill/" + (Regex.IsMatch(id, @"80\d{6}") ? id.Substring(0, 6) : id.Substring(0, id.Length - 4)) + ".img/skill/" + id), PluginBase.PluginManager.FindWz)).ToArray();
                }
                catch (Exception ex)
                {
                    hyperStatSkillList = null;
                }
            }
        }

        private Point calcRenderBaseOffset()
        {
            if (this.HyperStatVisible)
                return new Point(Resource.UICharacterInfo_img_remote_detailStat_HyperStat_Window_backgrnd.Width, 0);
            else
                return new Point(0, 0);
        }

        private void setControlState()
        {
            if (this.StatVisible && this.DetailVisible)
            {
                this.btntoggleDetailOpen.Visible = false;
                this.btntoggleDetailClose.Visible = true;
                this.btnDetailOpen.Visible = true;
                this.btnDetailClose.Visible = false;
                this.btnhelp.Visible = true;
                this.EquipVisible = false;
                this.SkillVisible = false;
                this.CashVisible = false;
            }
            else if (this.EquipVisible && this.DetailVisible)
            {
                this.btntoggleDetailOpen.Visible = false;
                this.btntoggleDetailClose.Visible = true;
                this.btnDetailOpen.Visible = false;
                this.btnDetailClose.Visible = false;
                this.btnhelp.Visible = false;
                this.StatVisible = false;
                this.SkillVisible = false;
                this.CashVisible = false;
            }
            else if (this.SkillVisible && this.DetailVisible)
            {
                this.btntoggleDetailOpen.Visible = false;
                this.btntoggleDetailClose.Visible = true;
                this.btnDetailOpen.Visible = false;
                this.btnDetailClose.Visible = false;
                this.btnhelp.Visible = false;
                this.StatVisible = false;
                this.EquipVisible = false;
                this.CashVisible = false;
            }
            else if (this.CashVisible && this.DetailVisible)
            {
                this.btntoggleDetailOpen.Visible = false;
                this.btntoggleDetailClose.Visible = true;
                this.btnDetailOpen.Visible = false;
                this.btnDetailClose.Visible = false;
                this.btnhelp.Visible = false;
                this.StatVisible = false;
                this.EquipVisible = false;
                this.SkillVisible = false;
            }
            else
            {
                this.btntoggleDetailOpen.Visible = true;
                this.btntoggleDetailClose.Visible = false;
                this.btnDetailOpen.Visible = false;
                this.btnDetailClose.Visible = false;
                this.btnhelp.Visible = false;
                this.StatVisible = false;
                this.EquipVisible = false;
                this.SkillVisible = false;
                this.CashVisible = false;
            }
            if (this.AbilityVisible && this.StatVisible)
            {
                this.btnDetailOpen.Visible = false;
                this.btnDetailClose.Visible = true;
                this.btnHpUp.Visible = true;
            }
            else if (this.AbilityVisible && !this.StatVisible)
            {
                this.btnDetailOpen.Visible = false;
                this.btnDetailClose.Visible = false;
                this.btnHpUp.Visible = true;
            }
            else if (!this.AbilityVisible && this.StatVisible)
            {
                this.btnDetailOpen.Visible = true;
                this.btnDetailClose.Visible = false;
                this.btnHpUp.Visible = false;
            }
            else
            {
                this.btnDetailOpen.Visible = false;
                this.btnDetailClose.Visible = false;
                this.btnHpUp.Visible = false;
            }

            if (this.HyperStatVisible && this.StatVisible)
            {
                this.btnHyperStatOpen.Visible = false;
                this.btnHyperStatClose.Visible = true;
                this.btnReduce.Visible = true;
            }
            else if (this.HyperStatVisible && !this.StatVisible)
            {
                this.btnHyperStatOpen.Visible = false;
                this.btnHyperStatClose.Visible = false;
                this.btnReduce.Visible = true;
            }
            else if (!this.HyperStatVisible && this.StatVisible)
            {
                this.btnHyperStatOpen.Visible = true;
                this.btnHyperStatClose.Visible = false;
                this.btnReduce.Visible = false;
            }
            else
            {
                this.btnHyperStatOpen.Visible = false;
                this.btnHyperStatClose.Visible = false;
                this.btnReduce.Visible = false;
            }

            if (this.character != null)
            {
                CharacterStatus charStat = this.character.Status;
                //setButtonEnabled(this.btnHPUp, charStat.Ap > 0 && charStat.MaxHP.BaseVal < charStat.MaxHP.TotalMax);
                //setButtonEnabled(this.btnMPUp, charStat.Ap > 0 && charStat.MaxMP.BaseVal < charStat.MaxMP.TotalMax);
                //setButtonEnabled(this.btnStrUp, charStat.Ap > 0/* && charStat.Strength.BaseVal <= 999*/);
                //setButtonEnabled(this.btnDexUp, charStat.Ap > 0/* && charStat.Dexterity.BaseVal <= 999*/);
                //setButtonEnabled(this.btnIntUp, charStat.Ap > 0/* && charStat.Intelligence.BaseVal <= 999*/);
                //setButtonEnabled(this.btnLukUp, charStat.Ap > 0/* && charStat.Luck.BaseVal <= 999*/);
                //setButtonEnabled(this.btnAuto, charStat.Ap > 0);
            }
            else
            {
                foreach (AControl ctrl in this.aControls)
                {
                    setButtonEnabled(ctrl as ACtrlButton, true);
                }
            }
        }

        private void setButtonEnabled(ACtrlButton button, bool enabled)
        {
            if (button == null)
                return;
            if (enabled)
            {
                if (button.State == ButtonState.Disabled)
                {
                    button.State = ButtonState.Normal;
                }
            }
            else
            {
                if (button.State != ButtonState.Disabled)
                {
                    button.State = ButtonState.Disabled;
                }
            }
        }

        private static string ToCJKNumberExpr(long value)
        {
            var sb = new StringBuilder(32);
            bool firstPart = true;
            if (value >= 1_0000_0000_0000_0000)
            {
                long part = value / 1_0000_0000_0000_0000;
                sb.AppendFormat("{0}京", part); // Korean: 교, Chinese+Japanese: 京
                value -= part * 1_0000_0000_0000_0000;
                firstPart = false;
            }
            if (value >= 1_0000_0000_0000)
            {
                long part = value / 1_0000_0000_0000;
                sb.Append(firstPart ? null : " ");
                sb.AppendFormat("{0}兆", part); // Korean: 조, Chinese+Japanese: 兆
                value -= part * 1_0000_0000_0000;
                firstPart = false;
            }
            if (value >= 1_0000_0000)
            {
                long part = value / 1_0000_0000;
                sb.Append(firstPart ? null : " ");
                sb.AppendFormat("{0}億", part); // Korean: 억, TradChinese+Japanese: 億, SimpChinese: 億
                value -= part * 1_0000_0000;
                firstPart = false;
            }
            if (value >= 1_0000)
            {
                long part = value / 1_0000;
                sb.Append(firstPart ? null : " ");
                sb.AppendFormat("{0}万", part); // Korean: 만, TradChinese: 萬, SimpChinese+Japanese: 万
                value -= part * 1_0000;
                firstPart = false;
            }
            if (value > 0)
            {
                sb.Append(firstPart ? null : " ");
                sb.AppendFormat("{0}", value);
            }

            return sb.Length > 0 ? sb.ToString() : "0";
        }

        private void renderBase(Graphics g) //绘制角色信息界面
        {
            g.TranslateTransform(baseOffset.X, baseOffset.Y);
            g.DrawImage(Resource.UICharacterInfo_img_common_main_backgrnd, 1, 0);
            //g.DrawImage(Resource.UICharacterInfo_img_customBackground_5_image_0, 141, 32);
            g.DrawImage(Resource.UICharacterInfo_img_common_main_layername, 183, 32);
            //g.DrawImage(Resource.UICharacterInfo_img_common_main_canvasmasterDisciple, 437, 39);
            if (this.character != null)
            {
                CharacterStatus charStat = this.character.Status;
                g.DrawString(this.character.Name, GearGraphics.ItemDetailFont, GearGraphics.WhiteBrush, (472f - g.MeasureString(this.character.Name, GearGraphics.ItemDetailFont).Width) / 2, 174f);
                g.DrawString(ItemStringHelper.GetJobName(charStat.Job), GearGraphics.ItemDetailFont, GearGraphics.WhiteBrush, (150f - g.MeasureString(ItemStringHelper.GetJobName(charStat.Job), GearGraphics.ItemDetailFont).Width) / 2, 44f);
                g.DrawString(string.IsNullOrEmpty(this.character.Guild) ? "  King丶Back" : this.character.Guild.PadLeft(12), GearGraphics.ItemDetailFont, GearGraphics.GrayBrush, 373f, 150f);
                g.DrawString("-", GearGraphics.ItemDetailFont, GearGraphics.GrayBrush, 444f - g.MeasureString("-", GearGraphics.ItemDetailFont).Width, 173f);
                g.DrawString(charStat.Pop.ToString().PadLeft(5), GearGraphics.ItemDetailFont, GearGraphics.GrayBrush, 92f, 173f);
                g.DrawString(charStat.Level.ToString().PadLeft(3), GearGraphics.LevelBoldFont, GearGraphics.WhiteBrush, 234f, 35f);
                g.DrawString(charStat.UnionLevel.ToString().PadLeft(5), GearGraphics.ItemDetailFont, GearGraphics.GrayBrush, 92f, 129f);
                g.DrawString(charStat.DojoFloor.ToString().PadLeft(3) + "层", GearGraphics.ItemDetailFont, GearGraphics.GrayBrush, 92f, 151f);
            }
            g.ResetTransform();
        }

        private void renderTab(Graphics g)
        {
            g.TranslateTransform(baseOffset.X, baseOffset.Y);
            g.DrawImage(Resource.UICharacterInfo_img_local_detail_backgrnd, 1, 231);  //属性背景
            // 四选一标签
            g.DrawImage(Resource.UICharacterInfo_img_remote_detail_tabdetailTab_normal_0, 12, 242);
            g.DrawImage(Resource.UICharacterInfo_img_remote_detail_tabdetailTab_normal_1, 125, 242);
            g.DrawImage(Resource.UICharacterInfo_img_remote_detail_tabdetailTab_normal_2, 238, 242);
            g.DrawImage(Resource.UICharacterInfo_img_remote_detail_tabdetailTab_normal_3, 351, 242);
            g.ResetTransform();
        }

        private void renderDetail(Graphics g)
        {
            g.TranslateTransform(baseOffset.X, baseOffset.Y);
            g.DrawImage(Resource.UICharacterInfo_img_remote_detail_tabdetailTab_selected_0, 11, 242);  //属性标签
            g.DrawImage(Resource.UICharacterInfo_img_common_detailStat_canvasattackBack, 12, 269);  //战斗力背景
            g.DrawImage(Resource.UICharacterInfo_img_remote_detailStat_canvasmainStatBack, 12, 307);  //主属性背景
            g.DrawImage(Resource.UICharacterInfo_img_common_detailStat_canvasutilityBack, 12, 583);  //三选一属性背景
            g.DrawImage(Resource.UICharacterInfo_img_common_detailStat_canvasmainStatFont, 24, 321);  //主属性文本
            g.DrawImage(Resource.UICharacterInfo_img_common_detailStat_canvasattackFont, 24, 400);   //属性文本

            if (this.character != null)
            {
                CharacterStatus charStat = this.character.Status;
                int brushSign;

                double max, min;
                this.character.CalcAttack(out max, out min, out brushSign);
                g.DrawString(charStat.MaxHP.GetSum().ToString("N0").PadLeft(7), GearGraphics.ItemDetailFont, GearGraphics.WhiteBrush, 187f, 320f);
                switch (charStat.Job)
                {
                    case 3101:
                    case 3120:
                    case 3121:
                    case 3122:
                    case 3124:
                    case 14000:
                    case 14200:
                    case 14210:
                    case 14211:
                    case 14212:
                    case 14213:
                    case 14214:
                        break;
                    case 3100:
                    case 3110:
                    case 3111:
                    case 3112:
                    case 3114:
                        g.DrawImage(Resource.UICharacterInfo_img_common_detailStat_Stat_1_titleImageDF, 244, 321);
                        g.DrawString(charStat.SpecialValue.GetSum().ToString("N0").PadLeft(7), GearGraphics.ItemDetailFont, GearGraphics.WhiteBrush, 406f, 320f);
                        break;
                    case 4200:
                    case 4210:
                    case 4211:
                    case 4212:
                    case 4216:
                        g.DrawImage(Resource.UICharacterInfo_img_common_detailStat_Stat_1_titleImageSE, 244, 321);
                        g.DrawString(charStat.SpecialValue.GetSum().ToString("N0").PadLeft(7), GearGraphics.ItemDetailFont, GearGraphics.WhiteBrush, 406f, 320f);
                        break;
                    case 10000:
                    case 10100:
                    case 10110:
                    case 10111:
                    case 10112:
                    case 10114:
                        g.DrawImage(Resource.UICharacterInfo_img_common_detailStat_Stat_1_titleImageTF, 244, 321);
                        g.DrawString(charStat.SpecialValue.GetSum().ToString("N0").PadLeft(7), GearGraphics.ItemDetailFont, GearGraphics.WhiteBrush, 406f, 320f);
                        break;
                    default:
                        g.DrawImage(Resource.UICharacterInfo_img_common_detailStat_Stat_1_titleImage, 244, 321);
                        g.DrawString(charStat.MaxMP.GetSum().ToString("N0").PadLeft(7), GearGraphics.ItemDetailFont, GearGraphics.WhiteBrush, 406f, 320f);
                        break;
                }
                string CombatPower = ToCJKNumberExpr(charStat.combatPower.GetSum());
                int xPosition = 200;
                foreach (char c in CombatPower)
                {
                    if (c == ' ')
                    {
                        xPosition += 4; continue;
                    }
                    char replacementChar = c;
                    if (c == '万') replacementChar = 'x';
                    else if (c == '億') replacementChar = 'y';
                    else if (c == '兆') replacementChar = 'z';
                    string imageName = "UICharacterInfo_img_common_detailStat_attackPowerFont_" + replacementChar;
                    DrawImage(g, imageName, xPosition, 279);
                    xPosition += GetImageWidth(imageName);
                }
                g.DrawString(charStat.Strength.GetSum().ToString("N0").PadLeft(7), GearGraphics.ItemDetailFont, GearGraphics.WhiteBrush, 187f, 342f);
                g.DrawString(charStat.Dexterity.GetSum().ToString("N0").PadLeft(7), GearGraphics.ItemDetailFont, GearGraphics.WhiteBrush, 406f, 342f);
                g.DrawString(charStat.Intelligence.GetSum().ToString("N0").PadLeft(7), GearGraphics.ItemDetailFont, GearGraphics.WhiteBrush, 187f, 364f);
                g.DrawString(charStat.Luck.GetSum().ToString("N0").PadLeft(7), GearGraphics.ItemDetailFont, GearGraphics.WhiteBrush, 406f, 364f);

                float y = 401f;
                StringFormat format = new StringFormat();
                format.Alignment = StringAlignment.Far;
                g.DrawString(ToCJKNumberExpr(charStat.attackRange.GetSum()), GearGraphics.ItemDetailFont, GearGraphics.WhiteBrush, 234f, y, format);
                g.DrawString(charStat.DamageRate.GetSum().ToString("N2") + "%", GearGraphics.ItemDetailFont, charStat.DamageRate.BuffAdd > 0 ? Brushes.Red : GearGraphics.WhiteBrush, 451f, y, format);
                g.DrawString(charStat.FinalDamageRate.GetSum().ToString("N2") + "%", GearGraphics.ItemDetailFont, charStat.FinalDamageRate.BuffAdd > 0 ? Brushes.Red : GearGraphics.WhiteBrush, 234f, (y += 22f), format);
                g.DrawString(charStat.BossDamageRate.GetSum().ToString("N2") + "%", GearGraphics.ItemDetailFont, charStat.BossDamageRate.BuffAdd > 0 ? Brushes.Red : GearGraphics.WhiteBrush, 451f, y, format);
                g.DrawString(charStat.IgnoreMobDefenceRate.GetSum().ToString("N2") + "%", GearGraphics.ItemDetailFont, charStat.IgnoreMobDefenceRate.BuffAdd > 0 ? Brushes.Red : GearGraphics.WhiteBrush, 234f, (y += 22f), format);
                g.DrawString(charStat.NormalMonsterDamR.GetSum() + ".00%", GearGraphics.ItemDetailFont, charStat.NormalMonsterDamR.BuffAdd > 0 ? Brushes.Red : GearGraphics.WhiteBrush, 451f, y, format);
                g.DrawString(charStat.PADamage.GetSum().ToString("N0"), GearGraphics.ItemDetailFont, charStat.PADamage.BuffAdd > 0 ? Brushes.Red : GearGraphics.WhiteBrush, 234f, (y += 22f), format);
                g.DrawString(charStat.CriticalRate.GetSum() + "%", GearGraphics.ItemDetailFont, charStat.CriticalRate.BuffAdd > 0 ? Brushes.Red : GearGraphics.WhiteBrush, 451f, y, format);
                g.DrawString(charStat.MADamage.GetSum().ToString("N0"), GearGraphics.ItemDetailFont, charStat.MADamage.BuffAdd > 0 ? Brushes.Red : GearGraphics.WhiteBrush, 234f, (y += 22f), format);
                g.DrawString(charStat.CriticalDamage.GetSum().ToString("N2") + "%", GearGraphics.ItemDetailFont, charStat.CriticalDamage.BuffAdd > 0 ? Brushes.Red : GearGraphics.WhiteBrush, 451f, y, format);
                g.DrawString(charStat.CooltimeReduceSecond.GetSum() + "秒/" + charStat.CooltimeReduceR.GetSum() + "%", GearGraphics.ItemDetailFont, GearGraphics.WhiteBrush, 234f, (y += 22f), format);
                g.DrawString(charStat.BuffDurationIncR.GetSum() + "%", GearGraphics.ItemDetailFont, charStat.BuffDurationIncR.BuffAdd > 0 ? Brushes.Red : GearGraphics.WhiteBrush, 451f, y, format);
                g.DrawString(charStat.CooltimeIgnoreR.GetSum().ToString("N2") + "%", GearGraphics.ItemDetailFont, charStat.CooltimeIgnoreR.BuffAdd > 0 ? Brushes.Red : GearGraphics.WhiteBrush, 234f, (y += 22f), format);
                g.DrawString(charStat.StatusResistance.GetSum().ToString("N2") + "%", GearGraphics.ItemDetailFont, charStat.StatusResistance.BuffAdd > 0 ? Brushes.Red : GearGraphics.WhiteBrush, 451f, y, format);
                g.DrawString(charStat.AbnormalDmgR.ToString("N2") + "%", GearGraphics.ItemDetailFont, GearGraphics.WhiteBrush, 234f, (y += 22f), format);
                g.DrawString(charStat.TamingMobDurationIncR.GetSum() + "%", GearGraphics.ItemDetailFont, charStat.TamingMobDurationIncR.BuffAdd > 0 ? Brushes.Red : GearGraphics.WhiteBrush, 451f, y, format);

                y = 594f;
                switch (statFont)
                {
                    case 1:
                        g.DrawImage(Resource.UICharacterInfo_img_common_detailStat_canvasutilityFont, 24, 594);  //三选一属性第1页
                        g.DrawString(charStat.MesoGainR.GetSum() + "%", GearGraphics.ItemDetailFont, charStat.MesoGainR.BuffAdd > 0 ? Brushes.Red : GearGraphics.WhiteBrush, 235f, y, format);
                        g.DrawString(charStat.StarForce.GetSum().ToString("N0"), GearGraphics.ItemDetailFont, charStat.StarForce.BuffAdd > 0 ? Brushes.Red : GearGraphics.WhiteBrush, 453f, y, format);
                        g.DrawString(charStat.DropGainR.GetSum() + "%", GearGraphics.ItemDetailFont, charStat.DropGainR.BuffAdd > 0 ? Brushes.Red : GearGraphics.WhiteBrush, 235f, (y += 22f), format);
                        g.DrawString(charStat.ArcaneForce.GetSum().ToString("N0"), GearGraphics.ItemDetailFont, charStat.ArcaneForce.BuffAdd > 0 ? Brushes.Red : GearGraphics.WhiteBrush, 453f, y, format);
                        g.DrawString(charStat.ExpGainR.ToString("N2") + "%", GearGraphics.ItemDetailFont, GearGraphics.WhiteBrush, 235f, (y += 22f), format);
                        g.DrawString(charStat.AuthenticForce.GetSum().ToString("N0"), GearGraphics.ItemDetailFont, charStat.AuthenticForce.BuffAdd > 0 ? Brushes.Red : GearGraphics.WhiteBrush, 453f, y, format);
                        break;
                    case 2:
                        g.DrawImage(Resource.UICharacterInfo_img_common_detailStat_canvasdefenseFont, 24, 594);  //三选一属性第2页
                        g.DrawString(charStat.Defense.GetSum().ToString("N0"), GearGraphics.ItemDetailFont, charStat.MesoGainR.BuffAdd > 0 ? Brushes.Red : GearGraphics.WhiteBrush, 235f, y, format);
                        g.DrawString(charStat.StatusResistance.GetSum().ToString("N0"), GearGraphics.ItemDetailFont, charStat.StarForce.BuffAdd > 0 ? Brushes.Red : GearGraphics.WhiteBrush, 453f, y, format);
                        g.DrawString(charStat.MoveSpeed.GetSum() + "%", GearGraphics.ItemDetailFont, charStat.DropGainR.BuffAdd > 0 ? Brushes.Red : GearGraphics.WhiteBrush, 235f, (y += 22f), format);
                        g.DrawString(charStat.Jump.GetSum().ToString("N0") + "%", GearGraphics.ItemDetailFont, charStat.ArcaneForce.BuffAdd > 0 ? Brushes.Red : GearGraphics.WhiteBrush, 453f, y, format);
                        g.DrawString(charStat.Stance.GetSum().ToString("N0") + "%", GearGraphics.ItemDetailFont, GearGraphics.WhiteBrush, 235f, (y += 22f), format);
                        g.DrawString("" + charStat.attackSpeed.GetSum().ToString("N0") + "段階", GearGraphics.ItemDetailFont, charStat.AuthenticForce.BuffAdd > 0 ? Brushes.Red : GearGraphics.WhiteBrush, 455f, y, format);
                        break;
                    default: break;
                }
            }
            g.ResetTransform();
        }

        private void DrawImage(Graphics g, string imageName, int x, int y)
        {
            System.Drawing.Bitmap image = Resource.ResourceManager.GetObject(imageName) as System.Drawing.Bitmap;
            g.DrawImage(image, new Rectangle(x, y, image.Width, image.Height));
        }

        private int GetImageWidth(string imageName)
        {
            System.Drawing.Bitmap image = Resource.ResourceManager.GetObject(imageName) as System.Drawing.Bitmap;
            return image != null ? image.Width : 0;
        }

        private void renderEquip(Graphics g) //绘制装备界面
        {
            g.TranslateTransform(baseOffset.X, baseOffset.Y);
            g.DrawImage(Resource.UICharacterInfo_img_remote_detail_tabdetailTab_selected_1, 125, 242);
            g.DrawImage(Resource.UICharacterInfo_img_remote_detailEquip_canvasequip, 13, 269);
            g.DrawImage(Resource.UICharacterInfo_img_remote_detailEquip_tabsymbolTab_normal_0, 262, 280);
            g.DrawImage(Resource.UICharacterInfo_img_remote_detailEquip_tabsymbolTab_normal_1, 354, 280);
            switch (this.ArcAut)
            {
                case 1:
                    g.DrawImage(Resource.UICharacterInfo_img_remote_detailEquip_tabsymbolTab_selected_0, 262, 280);
                    g.DrawImage(Resource.UICharacterInfo_img_remote_detailEquip_ArcEquip_backgrnd, 262, 305);
                    g.DrawImage(Resource.UICharacterInfo_img_remote_detailEquip_ArcEquip_slotVector1_canvasslot, 280, 384);
                    g.DrawImage(Resource.UICharacterInfo_img_remote_detailEquip_ArcEquip_slotVector1_canvasslot, 330, 384);
                    g.DrawImage(Resource.UICharacterInfo_img_remote_detailEquip_ArcEquip_slotVector1_canvasslot, 380, 384);
                    g.DrawImage(Resource.UICharacterInfo_img_remote_detailEquip_ArcEquip_slotVector1_canvasslot, 280, 476);
                    g.DrawImage(Resource.UICharacterInfo_img_remote_detailEquip_ArcEquip_slotVector1_canvasslot, 330, 476);
                    g.DrawImage(Resource.UICharacterInfo_img_remote_detailEquip_ArcEquip_slotVector1_canvasslot, 380, 476);
                    break;
                case 2:
                    g.DrawImage(Resource.UICharacterInfo_img_remote_detailEquip_tabsymbolTab_selected_1, 354, 280);
                    g.DrawImage(Resource.UICharacterInfo_img_remote_detailEquip_AutEquip_backgrnd, 262, 305);
                    g.DrawImage(Resource.UICharacterInfo_img_remote_detailEquip_AutEquip_slotVector1_canvasslot, 282, 384);
                    g.DrawImage(Resource.UICharacterInfo_img_remote_detailEquip_AutEquip_slotVector1_canvasslot, 332, 384);
                    g.DrawImage(Resource.UICharacterInfo_img_remote_detailEquip_AutEquip_slotVector1_canvasslot, 382, 384);
                    g.DrawImage(Resource.UICharacterInfo_img_remote_detailEquip_AutEquip_slotVector1_canvasslot, 282, 476);
                    g.DrawImage(Resource.UICharacterInfo_img_remote_detailEquip_AutEquip_slotVector1_canvasslot, 332, 476);
                    g.DrawImage(Resource.UICharacterInfo_img_remote_detailEquip_AutEquip_slotVector1_canvasslot, 382, 476);
                    break;
                default: break;
            }
            g.ResetTransform();
        }

        private void renderSkill(Graphics g)  //绘制技能界面
        {
            g.TranslateTransform(baseOffset.X, baseOffset.Y);
            g.DrawImage(Resource.UICharacterInfo_img_remote_detail_tabdetailTab_selected_2, 238, 242);
            g.DrawImage(Resource.UICharacterInfo_img_remote_detailSkill_canvasskill, 13, 269);
            g.DrawImage(Resource.UICharacterInfo_img_remote_detailSkill_tabtypeTab_normal_0, 28, 285);
            g.DrawImage(Resource.UICharacterInfo_img_remote_detailSkill_tabtypeTab_normal_1, 169, 285);
            g.DrawImage(Resource.UICharacterInfo_img_remote_detailSkill_tabtypeTab_normal_2, 310, 285);
            int skillNum = 12;
            switch (this.SkillTab)
            {
                case 1:
                    g.DrawImage(Resource.UICharacterInfo_img_remote_detailSkill_tabtypeTab_selected_0, 28, 285);
                    renderSkillBack(g, skillNum);
                    break;
                case 2:
                    g.DrawImage(Resource.UICharacterInfo_img_remote_detailSkill_tabtypeTab_selected_1, 169, 285);
                    renderSkillBack(g, skillNum);
                    break;
                case 3:
                    g.DrawImage(Resource.UICharacterInfo_img_remote_detailSkill_tabtypeTab_selected_2, 310, 285);
                    renderSkillBack(g, skillNum);
                    break;
                default: break;
            }
            g.ResetTransform();
        }

        private void renderSkillBack(Graphics g, int skillNum)
        {
            for (int i = 0; i <= (skillNum - 1); i++)
            {
                g.DrawImage(Resource.UICharacterInfo_img_remote_detailSkill_canvasskillBlank, 26 + (i % 2) * 208, 323 + (i / 2) * 61);
            }
        }

        private void renderCash(Graphics g)  //绘制现金界面
        {
            g.TranslateTransform(baseOffset.X, baseOffset.Y);
            g.DrawImage(Resource.UICharacterInfo_img_remote_detail_tabdetailTab_selected_3, 351, 242);
            g.DrawImage(Resource.UICharacterInfo_img_remote_detailCash_canvascash, 13, 269);
            g.DrawImage(Resource.UICharacterInfo_img_remote_detailCash_tabcashTab_normal_0, 28, 280);
            g.DrawImage(Resource.UICharacterInfo_img_remote_detailCash_tabcashTab_normal_1, 139, 280);
            switch (this.CashPreset)
            {
                case 1: g.DrawImage(Resource.UICharacterInfo_img_remote_detailCash_tabcashTab_selected_0, 28, 280); break;
                case 2: g.DrawImage(Resource.UICharacterInfo_img_remote_detailCash_tabcashTab_selected_1, 139, 280); break;
                default: break;
            }
            g.ResetTransform();
        }

        private void renderAbility(Graphics g)  //绘制内在能力
        {
            Rectangle rect = this.DetailRect;
            g.TranslateTransform(rect.X, rect.Y);
            int AbilityYOffset = Resource.UICharacterInfo_img_common_main_backgrnd.Height + Resource.UICharacterInfo_img_local_detail_backgrnd.Height - Resource.UICharacterInfo_img_remote_detailStat_ability_backgrnd.Height;
            g.DrawImage(Resource.UICharacterInfo_img_remote_detailStat_ability_backgrnd, 1, AbilityYOffset);

            g.DrawImage(Resource.UICharacterInfo_img_remote_detailStat_ability_abilityTitle_legendary_0, 11, (AbilityYOffset + 30));
            g.DrawImage(Resource.UICharacterInfo_img_remote_detailStat_ability_metierLine_activated_0_legendary_0, 12, (AbilityYOffset + 62));
            g.DrawImage(Resource.UICharacterInfo_img_remote_detailStat_ability_metierLine_activated_0_unique_0, 12, (AbilityYOffset + 82));
            g.DrawImage(Resource.UICharacterInfo_img_remote_detailStat_ability_metierLine_activated_0_unique_0, 12, (AbilityYOffset + 102));

            if (this.character != null)
            {
                CharacterStatus charStat = this.character.Status;
            }

            g.ResetTransform();
        }

        private void renderHyperStat(Graphics g) //绘制超级属性
        {
            Rectangle rect = this.HyperStatRect;
            g.TranslateTransform(rect.X, rect.Y);
            g.DrawImage(Resource.UICharacterInfo_img_remote_detailStat_HyperStat_Window_backgrnd, 1, 0); //超级属性背景
            g.DrawImage(Resource.UICharacterInfo_img_remote_detailStat_HyperStat_Window_buttonpresetPage1_checked_0, 136, 421);
            switch (this.presetPage)
            {
                case 1: g.DrawImage(Resource.UICharacterInfo_img_remote_detailStat_HyperStat_Window_presetSelected0_0, 135, 420); break;
                case 2: g.DrawImage(Resource.UICharacterInfo_img_remote_detailStat_HyperStat_Window_presetSelected1_0, 155, 420); break;
                case 3: g.DrawImage(Resource.UICharacterInfo_img_remote_detailStat_HyperStat_Window_presetSelected2_0, 175, 420); break;
                default: break;
            }
            float ydistance = 22f;
            var series = new[] { hyperLv, hyperLv2, hyperLv3, hyperLv4, hyperLv5, hyperLv6, hyperLv7, hyperLv8, hyperLv9, hyperLv10, hyperLv11, hyperLv12, hyperLv13, hyperLv14, hyperLv15, hyperLv16, hyperLv17 };
            int count = 0;
            foreach (var param in series)
            {
                g.DrawString(param.ToString(), GearGraphics.LevelBoldFont, GearGraphics.WhiteBrush, 183f, 42f + ydistance * count);
                count++;
            }
            g.ResetTransform();
        }

        private Brush getDetailBrush(int sign)
        {
            switch (sign)
            {
                case 1: return Brushes.Red;
                case -1: return Brushes.Blue;
                case 0:
                default: return GearGraphics.GrayBrush;
            }

        }

        public TooltipHelp GetPairByPoint(Point point)
        {
            Point p = point;
            if (AbilityVisible && DetailRect.Contains(p))
            {
                p = Point.Subtract(point, new Size(DetailRect.X, DetailRect.Y));
                return helpDetailList?.FirstOrDefault(t => t.Rect.Contains(p))?.Help;
            }
            p = Point.Subtract(point, new Size(baseOffset.X, baseOffset.Y));
            return helpList?.FirstOrDefault(t => t.Rect.Contains(p))?.Help;
        }

        public int GetSlotIndexByPoint(Point point)
        {
            Point p = point;
            p.Offset(-11, -41);
            if (p.X < 0 || p.Y < 0)
                return -1;
            int idx = p.Y / 18;
            if (new Rectangle(new Point(0, idx * 18), new Size(71, 16)).Contains(p))
                return idx;
            else
                return -1;
        }

        public int GetHyperStatIndexByPoint(Point point)
        {
            int slotIdx = GetSlotIndexByPoint(point);
            if (slotIdx != -1)
            {
                slotIdx += this.hyperStatScrollValue;
            }
            return slotIdx;
        }

        public Skill GetHyperStatByPoint(Point point)
        {
            if (HyperStatVisible && HyperStatRect.Contains(point) && hyperStatSkillList != null)
            {
                int hyperStatIdx = GetHyperStatIndexByPoint(Point.Subtract(point, new Size(HyperStatRect.X, HyperStatRect.Y)));
                if (hyperStatIdx > -1 && hyperStatIdx < this.hyperStatSkillList.Length)
                    return this.hyperStatSkillList[hyperStatIdx];
                else
                    return null;
            }
            return null;
        }

        private void btnClose_MouseClick(object sender, MouseEventArgs e)
        {
            this.Visible = false;
        }

        private void aCtrl_RefreshCall(object sender, EventArgs e)
        {
            this.waitForRefresh = true;
        }

        private void btntoggleDetailOpen_MouseClick(object sender, MouseEventArgs e)
        {
            this.DetailVisible = true;
            this.StatVisible = true;
            this.EquipVisible = false;
            this.SkillVisible = false;
            this.CashVisible = false;
            this.AbilityVisible = false;
            this.HyperStatVisible = false;
            this.waitForRefresh = true;
        }

        private void btntoggleDetailClose_MouseClick(object sender, MouseEventArgs e)
        {
            this.DetailVisible = false;
            this.StatVisible = false;
            this.EquipVisible = false;
            this.SkillVisible = false;
            this.CashVisible = false;
            this.AbilityVisible = false;
            this.HyperStatVisible = false;
            this.waitForRefresh = true;
        }

        private void btnDetailOpen_MouseClick(object sender, MouseEventArgs e)
        {
            this.AbilityVisible = true;
            this.waitForRefresh = true;
        }

        private void btnDetailClose_MouseClick(object sender, MouseEventArgs e)
        {
            this.AbilityVisible = false;
            this.waitForRefresh = true;
        }

        private void btnHyperStatOpen_MouseClick(object sender, MouseEventArgs e)
        {
            this.HyperStatVisible = true;
            this.waitForRefresh = true;
        }

        private void btnHyperStatClose_MouseClick(object sender, MouseEventArgs e)
        {
            this.HyperStatVisible = false;
            this.waitForRefresh = true;
        }

        private void btndetailTab_MouseClick(object sender, MouseEventArgs e)
        {
            this.StatVisible = true;
            this.EquipVisible = false;
            this.SkillVisible = false;
            this.CashVisible = false;
        }

        private void btndetailTab2_MouseClick(object sender, MouseEventArgs e)
        {
            this.StatVisible = false;
            this.EquipVisible = true;
            this.SkillVisible = false;
            this.CashVisible = false;
        }

        private void btndetailTab3_MouseClick(object sender, MouseEventArgs e)
        {
            this.StatVisible = false;
            this.EquipVisible = false;
            this.SkillVisible = true;
            this.CashVisible = false;
        }

        private void btndetailTab4_MouseClick(object sender, MouseEventArgs e)
        {
            this.StatVisible = false;
            this.EquipVisible = false;
            this.SkillVisible = false;
            this.CashVisible = true;
        }

        private void btnpresetPage1_MouseClick(object sender, MouseEventArgs e)
        {
            this.presetPage = 1;
        }

        private void btnpresetPage2_MouseClick(object sender, MouseEventArgs e)
        {
            this.presetPage = 2;
        }

        private void btnpresetPage3_MouseClick(object sender, MouseEventArgs e)
        {
            this.presetPage = 3;
        }

        private void btnStatFont_MouseClick(object sender, MouseEventArgs e)
        {
            if (this.StatVisible && this.DetailVisible)
            {
                switch (statFont)
                {
                    case 1: statFont = 2; break;
                    case 2: statFont = 1; break;
                    default: break;
                }
            }
        }

        private void btnArc_MouseClick(object sender, MouseEventArgs e)
        {
            if (this.EquipVisible)
                this.ArcAut = 1;
        }

        private void btnAut_MouseClick(object sender, MouseEventArgs e)
        {
            if (this.EquipVisible)
                this.ArcAut = 2;
        }

        private void btnLinkSkill_MouseClick(object sender, MouseEventArgs e)
        {
            if (this.SkillVisible)
                this.SkillTab = 1;
        }

        private void btnVSkill_MouseClick(object sender, MouseEventArgs e)
        {
            if (this.SkillVisible)
                this.SkillTab = 2;
        }

        private void btnHexaSkill_MouseClick(object sender, MouseEventArgs e)
        {
            if (this.SkillVisible)
                this.SkillTab = 3;
        }

        private void btnCash_MouseClick(object sender, MouseEventArgs e)
        {
            if (this.CashVisible)
                this.CashPreset = 1;
        }

        private void btnCashPreset_MouseClick(object sender, MouseEventArgs e)
        {
            if (this.CashVisible)
                this.CashPreset = 2;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            MouseEventArgs childArgs = new MouseEventArgs(e.Button, e.Clicks, e.X - baseOffset.X, e.Y - baseOffset.Y, e.Delta);

            foreach (AControl ctrl in this.aControls)
            {
                ctrl.OnMouseMove(childArgs);
            }

            MouseEventArgs detailChildArgs = new MouseEventArgs(e.Button, e.Clicks, e.X - DetailRect.X, e.Y - DetailRect.Y, e.Delta);

            foreach (AControl ctrl in this.aDetailControls)
            {
                ctrl.OnMouseMove(detailChildArgs);
            }

            MouseEventArgs hyperStatChildArgs = new MouseEventArgs(e.Button, e.Clicks, e.X - HyperStatRect.X, e.Y - HyperStatRect.Y, e.Delta);

            foreach (AControl ctrl in this.aHyperStatControls)
            {
                ctrl.OnMouseMove(hyperStatChildArgs);
            }

            if (this.waitForRefresh)
            {
                this.Refresh();
                waitForRefresh = false;
            }

            base.OnMouseMove(e);

            object obj = GetPairByPoint(e.Location);
            if (obj == null)
                obj = GetHyperStatByPoint(e.Location);
            if (obj != null)
                this.OnObjectMouseMove(new ObjectMouseEventArgs(e, obj));
            else
                this.OnObjectMouseLeave(EventArgs.Empty);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            MouseEventArgs childArgs = new MouseEventArgs(e.Button, e.Clicks, e.X - baseOffset.X, e.Y - baseOffset.Y, e.Delta);

            foreach (AControl ctrl in this.aControls)
            {
                ctrl.OnMouseDown(childArgs);
            }

            MouseEventArgs detailChildArgs = new MouseEventArgs(e.Button, e.Clicks, e.X - DetailRect.X, e.Y - DetailRect.Y, e.Delta);

            foreach (AControl ctrl in this.aDetailControls)
            {
                ctrl.OnMouseDown(detailChildArgs);
            }

            MouseEventArgs hyperStatChildArgs = new MouseEventArgs(e.Button, e.Clicks, e.X - HyperStatRect.X, e.Y - HyperStatRect.Y, e.Delta);

            foreach (AControl ctrl in this.aHyperStatControls)
            {
                ctrl.OnMouseDown(hyperStatChildArgs);
            }

            if (this.waitForRefresh)
            {
                this.Refresh();
                waitForRefresh = false;
            }

            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            MouseEventArgs childArgs = new MouseEventArgs(e.Button, e.Clicks, e.X - baseOffset.X, e.Y - baseOffset.Y, e.Delta);

            foreach (AControl ctrl in this.aControls)
            {
                ctrl.OnMouseUp(childArgs);
            }

            MouseEventArgs detailChildArgs = new MouseEventArgs(e.Button, e.Clicks, e.X - DetailRect.X, e.Y - DetailRect.Y, e.Delta);

            foreach (AControl ctrl in this.aDetailControls)
            {
                ctrl.OnMouseUp(detailChildArgs);
            }

            MouseEventArgs hyperStatChildArgs = new MouseEventArgs(e.Button, e.Clicks, e.X - HyperStatRect.X, e.Y - HyperStatRect.Y, e.Delta);

            foreach (AControl ctrl in this.aHyperStatControls)
            {
                ctrl.OnMouseUp(hyperStatChildArgs);
            }

            if (this.waitForRefresh)
            {
                this.Refresh();
                waitForRefresh = false;
            }

            base.OnMouseUp(e);
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            MouseEventArgs childArgs = new MouseEventArgs(e.Button, e.Clicks, e.X - baseOffset.X, e.Y - baseOffset.Y, e.Delta);

            foreach (AControl ctrl in this.aControls)
            {
                ctrl.OnMouseClick(childArgs);
            }

            MouseEventArgs detailChildArgs = new MouseEventArgs(e.Button, e.Clicks, e.X - DetailRect.X, e.Y - DetailRect.Y, e.Delta);

            foreach (AControl ctrl in this.aDetailControls)
            {
                ctrl.OnMouseClick(detailChildArgs);
            }

            MouseEventArgs hyperStatChildArgs = new MouseEventArgs(e.Button, e.Clicks, e.X - HyperStatRect.X, e.Y - HyperStatRect.Y, e.Delta);

            foreach (AControl ctrl in this.aHyperStatControls)
            {
                ctrl.OnMouseClick(hyperStatChildArgs);
            }

            if (this.waitForRefresh)
            {
                this.Refresh();
                waitForRefresh = false;
            }

            base.OnMouseClick(e);
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            MouseEventArgs childArgs = new MouseEventArgs(e.Button, e.Clicks, e.X - baseOffset.X, e.Y - baseOffset.Y, e.Delta);

            foreach (AControl ctrl in this.aControls)
            {
                ctrl.OnMouseWheel(childArgs);
            }

            MouseEventArgs detailChildArgs = new MouseEventArgs(e.Button, e.Clicks, e.X - DetailRect.X, e.Y - DetailRect.Y, e.Delta);

            foreach (AControl ctrl in this.aDetailControls)
            {
                ctrl.OnMouseWheel(detailChildArgs);
            }

            MouseEventArgs hyperStatChildArgs = new MouseEventArgs(e.Button, e.Clicks, e.X - HyperStatRect.X, e.Y - HyperStatRect.Y, e.Delta);

            foreach (AControl ctrl in this.aHyperStatControls)
            {
                ctrl.OnMouseWheel(hyperStatChildArgs);
            }

            if (this.waitForRefresh)
            {
                this.Refresh();
                waitForRefresh = false;
            }

            base.OnMouseWheel(e);
        }

        protected virtual void OnObjectMouseMove(ObjectMouseEventArgs e)
        {
            if (this.ObjectMouseMove != null)
                this.ObjectMouseMove(this, e);
        }

        protected virtual void OnObjectMouseLeave(EventArgs e)
        {
            if (this.ObjectMouseLeave != null)
                this.ObjectMouseLeave(this, e);
        }

        public class TooltipHelpRect
        {
            public TooltipHelpRect(Rectangle rect, TooltipHelp pair)
            {
                this.rect = rect;
                this.help = pair;
            }

            private Rectangle rect;
            private TooltipHelp help;

            public Rectangle Rect
            {
                get { return rect; }
            }

            public TooltipHelp Help
            {
                get { return help; }
            }
        }
    }
}