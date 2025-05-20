using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevComponents.DotNetBar;
using DevComponents.Editors;
using DevComponents.DotNetBar.Controls;
using WzComparerR2.CharaSim;
using WzComparerR2.Common;
using WzComparerR2.WzLib;
using WzComparerR2.PluginBase;
using WzComparerR2.Config;
using WzComparerR2.Controls;
using WzComparerR2.AvatarCommon;
using WzComparerR2.Encoders;
using System.IO;
using DevComponents.AdvTree;

#if NET6_0_OR_GREATER
using WzComparerR2.OpenAPI;
#endif

namespace WzComparerR2.Avatar.UI
{
    internal partial class AvatarForm : DevComponents.DotNetBar.OfficeForm
    {
        public AvatarForm()
        {
            InitializeComponent();
            this.avatar = new AvatarCanvas();
            this.animator = new Animator();
            // virtual comboboxes for item effects, not shown
            this.cmbEffectFrames = new DevComponents.DotNetBar.Controls.ComboBoxEx[AvatarCanvas.LayerSlotLength];
            this.cmbActionEffects = new DevComponents.DotNetBar.Controls.ComboBoxEx[AvatarCanvas.LayerSlotLength];
            for (int i = 0; i < AvatarCanvas.LayerSlotLength; i++)
            {
                var t1 = new DevComponents.DotNetBar.Controls.ComboBoxEx();
                t1.SelectedIndexChanged += new System.EventHandler(this.cmbEffectFrames_SelectedIndexChanged);
                cmbEffectFrames[i] = t1;

                var t2 = new DevComponents.DotNetBar.Controls.ComboBoxEx();
                t2.SelectedIndexChanged += new System.EventHandler(this.cmbActionEffect_SelectedIndexChanged);
                cmbActionEffects[i] = t2;
            }
            this.panelDockContainer2.Controls.Remove(this.chkHairShade); // disable chkHairShade
            btnReset_Click(btnReset, EventArgs.Empty);
            FillWeaponIdx();
            FillEarSelection();
            Instance = this;

#if NET6_0_OR_GREATER
            // https://learn.microsoft.com/en-us/dotnet/core/compatibility/fx-core#controldefaultfont-changed-to-segoe-ui-9pt
            this.Font = new Font(new FontFamily("SimSun"), 9f);
#endif
        }

        public SuperTabControlPanel GetTabPanel()
        {
            this.TopLevel = false;
            this.Dock = DockStyle.Fill;
            this.FormBorderStyle = FormBorderStyle.None;
            this.DoubleBuffered = true;
            var pnl = new SuperTabControlPanel();
            pnl.Controls.Add(this);
            pnl.Padding = new System.Windows.Forms.Padding(1);
            this.Visible = true;
            return pnl;
        }

        public Entry PluginEntry { get; set; }
        public static AvatarForm Instance;

        AvatarCanvas avatar;
        bool inited;
        string partsTag;
        bool suspendUpdate;
        bool needUpdate;
        Animator animator;
        string specifiedSavePath = "";
        // virtual comboboxes for item effects, not shown
        private DevComponents.DotNetBar.Controls.ComboBoxEx[] cmbActionEffects;
        private DevComponents.DotNetBar.Controls.ComboBoxEx[] cmbEffectFrames;
        private bool updatingActionEffect = false;
#if NET6_0_OR_GREATER
        private NexonOpenAPI API;
        private string characterName = "";
        private int previousRegion = 7;
#endif

        private string chairName;

        /// <summary>
        /// wz1节点选中事件。
        /// </summary>
        public void OnSelectedNode1Changed(object sender, WzNodeEventArgs e)
        {
            if (PluginEntry.Context.SelectedTab != PluginEntry.Tab || e.Node == null
                || this.btnLock.Checked)
            {
                return;
            }

            Wz_File file = e.Node.GetNodeWzFile();
            if (file == null)
            {
                return;
            }

            switch (file.Type)
            {
                case Wz_Type.Character: //读取装备
                    Wz_Image wzImg = e.Node.GetValue<Wz_Image>();
                    if (wzImg != null && wzImg.TryExtract())
                    {
                        this.SuspendUpdateDisplay();
                        LoadPart(wzImg.Node);
                        this.ResumeUpdateDisplay();
                    }
                    break;
            }
        }

        /// <summary>
        /// wz2节点选中事件。
        /// </summary>
        public void OnSelectedNode2Changed(object sender, WzNodeEventArgs e)
        {
            if (PluginEntry.Context.SelectedTab != PluginEntry.Tab || e.Node == null
                || this.btnLock.Checked)
            {
                return;
            }

            Wz_File file = e.Node.GetNodeWzFile();
            if (file == null)
            {
                return;
            }

            switch (file.Type)
            {
                case Wz_Type.Skill:
                    Wz_Node skillNode = e.Node;
                    if (Int32.TryParse(skillNode.Text, out int skillID))
                    {
                        int tamingMobID = skillNode.Nodes["vehicleID"].GetValueEx<int>(0);
                        if (tamingMobID == 0)
                        {
                            tamingMobID = PluginBase.PluginManager.FindWz(string.Format(@"Skill\RidingSkillInfo.img\{0:D7}\vehicleID", skillID)).GetValueEx<int>(0);
                        }
                        if (tamingMobID != 0)
                        {
                            var tamingMobNode = PluginBase.PluginManager.FindWz(string.Format(@"Character\TamingMob\{0:D8}.img", tamingMobID));
                            if (tamingMobNode != null)
                            {
                                this.SuspendUpdateDisplay();
                                RemoveChairPart();
                                LoadTamingPart(tamingMobNode, BitmapOrigin.CreateFromNode(skillNode.Nodes["icon"], PluginBase.PluginManager.FindWz), skillID, true);
                                this.ResumeUpdateDisplay();
                            }
                        }
                    }
                    break;

                case Wz_Type.Item: // should sync with LoadCode()
                    Wz_Node itemNode = e.Node;
                    if (Int32.TryParse(itemNode.Text, out int itemID))
                    {
                        bool removeTamingPart = true;
                        Wz_Vector brm = null;

                        int tamingMobID = itemNode.FindNodeByPath("info\\tamingMob").GetValueEx<int>(0);
                        if (tamingMobID == 0)
                        {
                            tamingMobID = itemNode.FindNodeByPath("info\\customChair\\self\\tamingMob").GetValueEx<int>(0);
                        }
                        if (tamingMobID != 0)
                        {
                            brm = itemNode.FindNodeByPath("info\\group\\sit\\0\\bodyRelMove").GetValueEx<Wz_Vector>(null);
                            var tamingMobNode = PluginBase.PluginManager.FindWz(string.Format(@"Character\TamingMob\{0:D8}.img", tamingMobID));
                            if (tamingMobNode != null)
                            {
                                removeTamingPart = false;

                                this.SuspendUpdateDisplay();
                                RemoveChairPart();
                                LoadTamingPart(tamingMobNode, BitmapOrigin.CreateFromNode(tamingMobNode.FindNodeByPath("info\\icon"), PluginBase.PluginManager.FindWz), tamingMobID, false, brm);
                                this.ResumeUpdateDisplay();
                            }
                        }
                        
                        brm = itemNode.FindNodeByPath("info\\bodyRelMove").GetValueEx<Wz_Vector>(null);
                        bool isSitActionExists = itemNode.FindNodeByPath("info\\sitAction").GetValueEx<string>(null) != null;
                        if (itemID / 10000 == 301 || itemID / 1000 == 5204 || brm != null || isSitActionExists) // 의자 아이템, 아이템 코드나 bodyRelMove과 sitAction 속성 유무로 결정
                        {
                            bool fb = false;
                            if (brm == null)
                            {
                                fb = false;
                            }
                            else if (isSitActionExists)
                            {
                                fb = true;
                            }

                            this.SuspendUpdateDisplay();
                            if (removeTamingPart) RemoveTamingPart();
                            LoadChairPart(itemNode, BitmapOrigin.CreateFromNode(itemNode.FindNodeByPath("info\\icon"), PluginBase.PluginManager.FindWz), itemID, brm, fb);
                            this.ResumeUpdateDisplay();
                        }

                        if (itemID / 10000 == 501) // effect items
                        {
                            this.SuspendUpdateDisplay();
                            LoadEffectPart(itemNode);
                            this.ResumeUpdateDisplay();
                        }
                    }
                    break;
            }
        }

        public void OnWzClosing(object sender, WzStructureEventArgs e)
        {
            bool hasChanged = false;
            for (int i = 0; i < avatar.Parts.Length; i++)
            {
                var part = avatar.Parts[i];
                if (part != null)
                {
                    var wzFile = part.Node.GetNodeWzFile();
                    if (wzFile != null && e.WzStructure.wz_files.Contains(wzFile))//将要关闭文件 移除
                    {
                        avatar.Parts[i] = null;
                        hasChanged = true;
                    }
                }
            }

            if (hasChanged)
            {
                this.FillAvatarParts();
                UpdateDisplay();
            }
        }

        /// <summary>
        /// 初始化纸娃娃资源。
        /// </summary>
        private bool AvatarInit()
        {
            this.inited = this.avatar.LoadZ()
                && this.avatar.LoadActions()
                && this.avatar.LoadEmotions();

            if (this.inited)
            {
                this.FillBodyAction();
                this.FillEmotion();
            }
            return this.inited;
        }

        /// <summary>
        /// 加载装备部件。
        /// </summary>
        /// <param name="imgNode"></param>
        private void LoadPart(Wz_Node imgNode)
        {
            if (!this.inited && !this.AvatarInit() && imgNode == null)
            {
                return;
            }

            AvatarPart part = this.avatar.AddPart(imgNode);
            if (part != null)
            {
                if (part == this.avatar.Taming)
                {
                    RemoveChairPart();
                }
                OnNewPartAdded(part);
                FillAvatarParts();
                UpdateDisplay();
            }
        }

        /// <summary>
        /// TamingMob 파트를 삭제합니다.
        /// </summary>
        private void RemoveTamingPart()
        {
            this.avatar.RemoveTamingPart();
            this.cmbTamingFrame.Items.Clear();
            this.cmbActionTaming.Items.Clear();
        }

        /// <summary>
        /// TamingMob 파트를 로드합니다.
        /// </summary>
        /// <param name="brm">BodyRelMove 정보입니다.</param>
        private void LoadTamingPart(Wz_Node imgNode, BitmapOrigin forceIcon, int forceID, bool isSkill, Wz_Vector brm = null)
        {
            if (!this.inited && !this.AvatarInit() && imgNode == null)
            {
                return;
            }

            AvatarPart part = this.avatar.AddTamingPart(imgNode, forceIcon, forceID, isSkill, brm);
            if (part != null)
            {
                OnNewPartAdded(part);
                FillAvatarParts();
                UpdateDisplay();
            }
        }

        /// <summary>
        /// 의자 아이템 파트를 삭제합니다.
        /// </summary>
        private void RemoveChairPart()
        {
            this.avatar.RemoveChairPart();
            this.cmbGroupChair.Items.Clear();
            this.cmbGroupChair.Enabled = false;
        }

        /// <summary>
        /// 의자 아이템 파트를 로드합니다.
        /// </summary>
        /// <param name="brm">BodyRelMove 정보입니다.</param>
        private void LoadChairPart(Wz_Node imgNode, BitmapOrigin forceIcon, int forceID, Wz_Vector brm, bool forceAct)
        {
            if (!this.inited && !this.AvatarInit() && imgNode == null)
            {
                return;
            }

            AvatarPart part = this.avatar.AddChairPart(imgNode, forceIcon, forceID, brm, forceAct);
            if (part != null)
            {
                if (part.GroupCount > 0)
                {
                    this.cmbGroupChair.Enabled = true;
                    FillComboItems(this.cmbGroupChair, 1, part.GroupCount);
                }
                else this.cmbGroupChair.Enabled = false;

                OnNewPartAdded(part);
                FillAvatarParts();
                UpdateDisplay();
            }
        }

        /// <summary>
        /// 이펙트 아이템 파트를 로드합니다.
        /// </summary>
        private void LoadEffectPart(Wz_Node imgNode)
        {
            if (!this.inited && !this.AvatarInit() && imgNode == null)
            {
                return;
            }

            AvatarPart part = this.avatar.AddEffectPart(imgNode);
            if (part != null)
            {
                OnNewPartAdded(part);
                FillAvatarParts();
                UpdateDisplay();
            }
        }

        private void OnNewPartAdded(AvatarPart part)
        {
            if (part == null)
            {
                return;
            }

            if (part == avatar.Body) //同步head
            {
                int headID = 10000 + part.ID.Value % 10000;
                if (avatar.Head == null || avatar.Head.ID != headID)
                {
                    var headImgNode = PluginBase.PluginManager.FindWz(string.Format("Character\\{0:D8}.img", headID));
                    if (headImgNode != null)
                    {
                        this.avatar.AddPart(headImgNode);
                    }
                }
            }
            else if (part == avatar.Head) //同步body
            {
                int bodyID = part.ID.Value % 10000;
                if (avatar.Body == null || avatar.Body.ID != bodyID)
                {
                    var bodyImgNode = PluginBase.PluginManager.FindWz(string.Format("Character\\{0:D8}.img", bodyID));
                    if (bodyImgNode != null)
                    {
                        this.avatar.AddPart(bodyImgNode);
                    }
                }
            }
            else if (part == avatar.Face) //同步表情
            {
                this.avatar.LoadEmotions();
                FillEmotion();
            }
            else if (part == avatar.Taming) //同步座驾动作
            {
                this.avatar.LoadTamingActions();
                FillTamingAction();
                SetTamingDefaultBodyAction();
                SetTamingDefault();
            }
            else if (part == avatar.Chair)
            {
                SetChairDefault();
            }
            else if (part == avatar.Weapon) //同步武器类型
            {
                FillWeaponTypes();
            }
            else if (part == avatar.Pants || part == avatar.Coat) //隐藏套装
            {
                if (avatar.Longcoat != null)
                {
                    avatar.Longcoat.Visible = false;
                }
            }
            else if (part == avatar.Longcoat) //还是。。隐藏套装
            {
                if (avatar.Pants != null && avatar.Pants.Visible
                    || avatar.Coat != null && avatar.Coat.Visible)
                {
                    avatar.Longcoat.Visible = false;
                }
            }
            else if (part == avatar.Cap) // sets CapType
            {
                avatar.CapType = part.VSlot;
            }

            if (part.EffectNode != null || part == avatar.Chair || part == avatar.Effect) // load Effects
            {
                this.updatingActionEffect = true;
                this.avatar.LoadAllEffects();
                FillEffectAction();
                this.updatingActionEffect = false;

                if (this.chkBodyPlay.Checked)
                {
                    SyncBodyEffect();
                }
                if (this.chkTamingPlay.Checked)
                {
                    SyncTamingEffect();
                }
            }
        }

        private void SuspendUpdateDisplay()
        {
            this.suspendUpdate = true;
            this.needUpdate = false;
        }

        private void ResumeUpdateDisplay()
        {
            if (this.suspendUpdate)
            {
                this.suspendUpdate = false;
                if (this.needUpdate)
                {
                    this.UpdateDisplay();
                }
            }
        }

        /// <summary>
        /// 更新画布。
        /// </summary>
        private void UpdateDisplay()
        {
            if (suspendUpdate)
            {
                this.needUpdate = true;
                return;
            }

            string newPartsTag = GetAllPartsTag();
            if (this.partsTag != newPartsTag)
            {
                this.partsTag = newPartsTag;
                this.avatarContainer1.ClearAllCache();
            }

            ComboItem selectedItem;
            //同步角色动作
            selectedItem = this.cmbActionBody.SelectedItem as ComboItem;
            this.avatar.ActionName = selectedItem != null ? selectedItem.Text : null;
            //同步表情
            selectedItem = this.cmbEmotion.SelectedItem as ComboItem;
            this.avatar.EmotionName = selectedItem != null ? selectedItem.Text : null;
            //同步骑宠动作
            selectedItem = this.cmbActionTaming.SelectedItem as ComboItem;
            this.avatar.TamingActionName = selectedItem != null ? selectedItem.Text : null;

            //获取动作帧
            this.GetSelectedBodyFrame(out int bodyFrame, out _);
            this.GetSelectedEmotionFrame(out int emoFrame, out _);
            this.GetSelectedTamingFrame(out int tamingFrame, out _);
            this.GetSelectedEffectFrames(out int[] effectFrames, out _);

            //获取武器状态
            selectedItem = this.cmbWeaponType.SelectedItem as ComboItem;
            this.avatar.WeaponType = selectedItem != null ? Convert.ToInt32(selectedItem.Text) : 0;

            selectedItem = this.cmbWeaponIdx.SelectedItem as ComboItem;
            this.avatar.WeaponIndex = selectedItem != null ? Convert.ToInt32(selectedItem.Text) : 0;

            //获取耳朵状态
            selectedItem = this.cmbEar.SelectedItem as ComboItem;
            this.avatar.EarType = selectedItem != null ? Convert.ToInt32(selectedItem.Text) : 0;

            if (bodyFrame < 0 && emoFrame < 0 && tamingFrame < 0)
            {
                return;
            }

            string actionTag = string.Format("{0}:{1},{2}:{3},{4}:{5},{6},{7},{8},{9},{10},{11},{12}:{13}:{14}",
                this.avatar.ActionName,
                bodyFrame,
                this.avatar.EmotionName,
                emoFrame,
                this.avatar.TamingActionName,
                tamingFrame,
                this.avatar.HairCover ? 1 : 0,
                this.avatar.ShowHairShade ? 1 : 0,
                this.avatar.EarType,
                this.avatar.WeaponType,
                this.avatar.WeaponIndex,
                this.avatar.GroupChair,
                this.avatar.ActionName,
                string.Join("_", effectFrames),
                string.Join("_", this.avatar.EffectVisibles));

            if (!avatarContainer1.HasCache(actionTag))
            {
                try
                {
                    var actionFrames = avatar.GetActionFrames(avatar.ActionName);
                    var bone = avatar.CreateFrame(bodyFrame, emoFrame, tamingFrame, effectFrames);
                    var layers = avatar.CreateFrameLayers(bone);
                    avatarContainer1.AddCache(actionTag, layers);
                }
                catch
                {
                }
            }

            avatarContainer1.SetKey(actionTag);
        }

        public string GetAllPartsTag()
        {
            string[] partsID = new string[avatar.Parts.Length];
            for (int i = 0; i < avatar.Parts.Length; i++)
            {
                var part = avatar.Parts[i];
                if (part != null && part.Visible)
                {
                    partsID[i] = (part.IsSkill ? "s" : "") + part.ID.ToString();
                    if (part.IsMixing)
                    {
                        partsID[i] += "+" + part.MixColor + "*" + part.MixOpacity;
                    }
                }
            }
            return string.Join(",", partsID);
        }

        void AddPart(string imgPath)
        {
            Wz_Node imgNode = PluginManager.FindWz(imgPath);
            if (imgNode != null)
            {
                this.avatar.AddPart(imgNode);
            }
        }

        private void SelectBodyAction(string actionName)
        {
            if (!this.chkBodyPlay.Checked && this.chkTamingPlay.Checked && this.avatar.Chair != null)
            {
                this.chkBodyPlay.Checked = true;
            }
            for (int i = 0; i < cmbActionBody.Items.Count; i++)
            {
                ComboItem item = cmbActionBody.Items[i] as ComboItem;
                if (item != null && item.Text == actionName)
                {
                    cmbActionBody.SelectedIndex = i;
                    return;
                }
            }
        }

        /// <summary>
        /// Body 액션을 선택된 프레임으로 고정합니다.
        /// </summary>
        /// <param name="actionName">고정할 Body Action의 이름</param>
        /// <param name="idx">고정할 프레임 번호</param>
        private void FixBodyAction(string actionName, int Idx)
        {
            this.SelectBodyAction(actionName);

            if (this.chkBodyPlay.Checked)
            {
                this.chkBodyPlay.Checked = false;
            }

            if (Idx >= 0)
            {
                this.cmbBodyFrame.SelectedIndex = Idx;
            }
        }

        /// <summary>
        /// cmbActionEffects의 각 콤보 박스에 대하여, actionName과 같은 이름을 가진 아이템을 선택합니다.
        /// <br/>actionName과 같은 이름을 가진 아이템이 없다면, defalut, effect, effect2와 같은 이름을 가진 아이템을 선택합니다.
        /// </summary>
        private void SelectEffectAction(string actionName)
        {
            for (int j = 0; j < cmbActionEffects.Count(); j++)
            {
                var defaultIdx = -1;
                for (int i = 0; i < cmbActionEffects[j].Items.Count; i++)
                {
                    ComboItem item = cmbActionEffects[j].Items[i] as ComboItem;
                    if (item != null && item.Text == actionName)
                    {
                        cmbActionEffects[j].SelectedIndex = i;
                        defaultIdx = -1;
                        break;
                    }
                    if (item != null && (item.Text == "default" || item.Text == "effect" || item.Text == "effect2"))
                    {
                        defaultIdx = i;
                        continue;
                    }
                }
                if (defaultIdx > -1)
                {
                    cmbActionEffects[j].SelectedIndex = defaultIdx;
                }
            }
        }

        private void SelectEmotion(string emotionName)
        {
            for (int i = 0; i < cmbEmotion.Items.Count; i++)
            {
                ComboItem item = cmbEmotion.Items[i] as ComboItem;
                if (item != null && item.Text == emotionName)
                {
                    cmbEmotion.SelectedIndex = i;
                    return;
                }
            }
        }

        private void SelectEmotionByIndex(int emotionIdx)
        {
            cmbEmotion.SelectedIndex = emotionIdx + 1;
        }

        #region 同步界面
        private void FillBodyAction()
        {
            var oldSelection = cmbActionBody.SelectedItem as ComboItem;
            int? newSelection = null;
            cmbActionBody.BeginUpdate();
            cmbActionBody.Items.Clear();
            foreach (var action in this.avatar.Actions)
            {
                ComboItem cmbItem = new ComboItem(action.Name);
                switch (action.Level)
                {
                    case 0:
                        cmbItem.FontStyle = FontStyle.Bold;
                        cmbItem.ForeColor = Color.Indigo;
                        break;

                    case 1:
                        cmbItem.ForeColor = Color.Indigo;
                        break;
                }
                cmbItem.Tag = action;
                cmbActionBody.Items.Add(cmbItem);

                if (newSelection == null && oldSelection != null)
                {
                    if (cmbItem.Text == oldSelection.Text)
                    {
                        newSelection = cmbActionBody.Items.Count - 1;
                    }
                }
            }

            if (cmbActionBody.Items.Count > 0)
            {
                cmbActionBody.SelectedIndex = newSelection ?? 0;
            }

            cmbActionBody.EndUpdate();
        }

        private void FillEmotion()
        {
            FillComboItems(cmbEmotion, avatar.Emotions);
        }

        private void FillTamingAction()
        {
            FillComboItems(cmbActionTaming, avatar.TamingActions);
        }

        /// <summary>
        /// 이펙트 동작 이름(AvatarCanvas의 EffectActions의 값)으로 cmbActionEffects를 채웁니다.
        /// </summary>
        private void FillEffectAction()
        {
            for (int i = 0; i < cmbActionEffects.Length; i++)
            {
                FillComboItems(cmbActionEffects[i], avatar.EffectActions[i]);
            }

            var selectedItem = this.cmbActionBody.SelectedItem as ComboItem;
            SelectEffectAction(selectedItem.Text);

            this.SuspendUpdateDisplay();
            FillEffectFrame();
            this.ResumeUpdateDisplay();
            UpdateDisplay();
        }

        private void FillWeaponTypes()
        {
            List<int> weaponTypes = avatar.GetCashWeaponTypes();
            FillComboItems(cmbWeaponType, weaponTypes.ConvertAll(i => i.ToString()));
        }

        private void SetTamingDefaultBodyAction()
        {
            string actionName;
            var tamingAction = (this.cmbActionTaming.SelectedItem as ComboItem)?.Text;
            switch (tamingAction)
            {
                case "ladder":
                case "rope":
                    actionName = tamingAction;
                    break;
                default:
                    actionName = "sit";
                    break;
            }
            SelectBodyAction(actionName);
        }

        private void SetTamingDefault()
        {
            if (this.avatar.Taming != null)
            {
                var tamingAction = (this.cmbActionTaming.SelectedItem as ComboItem)?.Text;
                if (tamingAction != null)
                {
                    string forceAction = this.avatar.Taming.Node.FindNodeByPath($@"characterAction\{tamingAction}").GetValueEx<string>(null);
                    if (forceAction != null)
                    {
                        this.SelectBodyAction(forceAction);
                    }

                    string forceEmotion = this.avatar.Taming.Node.FindNodeByPath($@"characterEmotion\{tamingAction}").GetValueEx<string>(null);
                    if (forceEmotion != null)
                    {
                        this.SelectEmotion(forceEmotion);
                    }
                }
            }
        }

        /// <summary>
        /// 의자 아이템의 기본 캐릭터 동작을 지정합니다.<br/>기본값 sit 또는 sitAction으로 설정된 값
        /// </summary>
        private void SetChairDefault()
        {
            if (this.avatar.Taming == null && this.avatar.Chair != null)
            {
                string forceAction = this.avatar.Chair.Node.FindNodeByPath("info\\sitAction").GetValueEx<string>("sit");
                int fixFrameIdx = this.avatar.Chair.Node.FindNodeByPath("info\\fixFrameIdx").GetValueEx<int>(-1);
                if (fixFrameIdx >= 0)
                {
                    this.FixBodyAction(forceAction, fixFrameIdx);
                }
                else
                {
                    this.SelectBodyAction(forceAction);
                }

                int forceEmotion = this.avatar.Chair.Node.FindNodeByPath("info\\sitEmotion").GetValueEx<int>(-1);
                this.SelectEmotionByIndex(forceEmotion);
            }
        }

        /// <summary>
        /// 更新当前显示部件列表。
        /// </summary>
        private void FillAvatarParts()
        {
            itemPanel1.BeginUpdate();
            itemPanel1.Items.Clear();
            foreach (var part in avatar.Parts)
            {
                if (part != null)
                {
                    var btn = new AvatarPartButtonItem(part.ID.Value, part.IsMixing ? part.MixColor : (int?)null, part.IsMixing ? part.MixOpacity : (int?)null);
                    this.SetButtonText(part, btn);
                    btn.SetIcon(part.Icon.Bitmap);
                    btn.Tag = part;
                    btn.Checked = part.Visible;
                    btn.btnItemShow.Click += BtnItemShow_Click;
                    btn.btnItemDel.Click += BtnItemDel_Click;
                    btn.chkShowEffect.Click += ChkShowEffect_Click;
                    btn.CheckedChanged += Btn_CheckedChanged;
                    btn.rdoMixColor0.CheckedChanged += RadioMixColor0_CheckedChanged;
                    btn.rdoMixColor1.CheckedChanged += RadioMixColor1_CheckedChanged;
                    btn.rdoMixColor2.CheckedChanged += RadioMixColor2_CheckedChanged;
                    btn.rdoMixColor3.CheckedChanged += RadioMixColor3_CheckedChanged;
                    btn.rdoMixColor4.CheckedChanged += RadioMixColor4_CheckedChanged;
                    btn.rdoMixColor5.CheckedChanged += RadioMixColor5_CheckedChanged;
                    btn.rdoMixColor6.CheckedChanged += RadioMixColor6_CheckedChanged;
                    btn.rdoMixColor7.CheckedChanged += RadioMixColor7_CheckedChanged;
                    btn.sliderMixRatio.ValueChanged += SliderMixRatio_ValueChanged;
                    itemPanel1.Items.Add(btn);
                }
            }
            itemPanel1.EndUpdate();
        }

        private void BtnItemShow_Click(object sender, EventArgs e)
        {
            var btn = (sender as BaseItem).Parent as AvatarPartButtonItem;
            if (btn != null)
            {
                btn.Checked = !btn.Checked;
            }
        }

        private void BtnItemDel_Click(object sender, EventArgs e)
        {
            var btn = (sender as BaseItem).Parent as AvatarPartButtonItem;
            if (btn != null)
            {
                var part = btn.Tag as AvatarPart;
                if (part != null)
                {
                    int index = Array.IndexOf(this.avatar.Parts, part);
                    if (index > -1)
                    {
                        this.avatar.Parts[index] = null;
                        this.FillAvatarParts();
                        this.UpdateDisplay();
                    }
                }
            }
        }

        private void ChkShowEffect_Click(object sender, EventArgs e)
        {
            var btn = (sender as BaseItem).Parent as AvatarPartButtonItem;
            if (btn != null)
            {
                var part = btn.Tag as AvatarPart;
                part.EffectVisible = btn.chkShowEffect.Checked;
                if (part != null)
                {
                    int index = Array.IndexOf(this.avatar.Parts, part);
                    if (index > -1)
                    {
                        this.avatar.EffectVisibles[index] = btn.chkShowEffect.Checked;
                        this.UpdateDisplay();
                    }
                }
            }
        }

        private void Btn_CheckedChanged(object sender, EventArgs e)
        {
            var btn = sender as AvatarPartButtonItem;
            if (btn != null)
            {
                var part = btn.Tag as AvatarPart;
                if (part != null)
                {
                    part.Visible = btn.Checked;
                    this.UpdateDisplay();
                }
            }
        }

        private void RadioMixColor0_CheckedChanged(object sender, EventArgs e)
        {
            if ((sender as CheckBoxItem).Checked)
            {
                RadioMixColor_CheckedChanged(sender, 0);
            }
        }

        private void RadioMixColor1_CheckedChanged(object sender, EventArgs e)
        {
            if ((sender as CheckBoxItem).Checked)
            {
                RadioMixColor_CheckedChanged(sender, 1);
            }
        }

        private void RadioMixColor2_CheckedChanged(object sender, EventArgs e)
        {
            if ((sender as CheckBoxItem).Checked)
            {
                RadioMixColor_CheckedChanged(sender, 2);
            }
        }

        private void RadioMixColor3_CheckedChanged(object sender, EventArgs e)
        {
            if ((sender as CheckBoxItem).Checked)
            {
                RadioMixColor_CheckedChanged(sender, 3);
            }
        }

        private void RadioMixColor4_CheckedChanged(object sender, EventArgs e)
        {
            if ((sender as CheckBoxItem).Checked)
            {
                RadioMixColor_CheckedChanged(sender, 4);
            }
        }

        private void RadioMixColor5_CheckedChanged(object sender, EventArgs e)
        {
            if ((sender as CheckBoxItem).Checked)
            {
                RadioMixColor_CheckedChanged(sender, 5);
            }
        }

        private void RadioMixColor6_CheckedChanged(object sender, EventArgs e)
        {
            if ((sender as CheckBoxItem).Checked)
            {
                RadioMixColor_CheckedChanged(sender, 6);
            }
        }

        private void RadioMixColor7_CheckedChanged(object sender, EventArgs e)
        {
            if ((sender as CheckBoxItem).Checked)
            {
                RadioMixColor_CheckedChanged(sender, 7);
            }
        }

        private void RadioMixColor_CheckedChanged(object sender, int mixColor)
        {
            var btn = (sender as BaseItem).Parent as AvatarPartButtonItem;
            if (btn != null)
            {
                var part = btn.Tag as AvatarPart;
                if (part != null)
                {
                    part.MixColor = mixColor;
                    this.UpdateDisplay();
                    this.SetButtonText(part, btn);
                }
            }
        }

        private void SliderMixRatio_ValueChanged(object sender, EventArgs e)
        {
            var slider = sender as SliderItem;
            var btn = (sender as BaseItem).Parent as AvatarPartButtonItem;
            if (btn != null)
            {
                var part = btn.Tag as AvatarPart;
                if (part != null)
                {
                    part.MixOpacity = slider.Value;
                    this.UpdateDisplay();
                    this.SetButtonText(part, btn);
                }
            }
        }

        private void SetButtonText(AvatarPart part, AvatarPartButtonItem btn)
        {
            var stringLinker = this.PluginEntry.Context.DefaultStringLinker;
            StringResult sr;
            string text;
            if (part.ID != null && (stringLinker.StringEqp.TryGetValue(part.ID.Value, out sr) || stringLinker.StringSkill.TryGetValue(part.ID.Value, out sr) || stringLinker.StringItem.TryGetValue(part.ID.Value, out sr)))
            {
                text = string.Format("{0}\r\n{1}{2}", sr.Name, part.IsSkill ? "s" : "", part.ID);
                if (part.IsMixing)
                {
                    text = string.Format("{0}\r\n{1} {2} : {3} {4}\r\n{5}+{6}*{7}",
                        Regex.Replace(sr.Name, "^([^ ]+色 )?", "混染 "),
                        GetColorName(part.ID.Value),
                        100 - part.MixOpacity,
                        GetMixColorName(part.MixColor, part.ID.Value),
                        part.MixOpacity,
                        part.ID,
                        part.MixColor,
                        part.MixOpacity);
                }
            }
            else
            {
                text = string.Format("{0}\r\n{1}", "(null)", part.ID == null ? "-" : part.ID.ToString());
            }
            if (part.ID.ToString().StartsWith("3"))
            {
                chairName = RemoveInvalidFileNameChars(string.Format("{0}_{1}", part.ID.ToString(), text.Substring(0, text.IndexOf("\r\n"))));
            }
            if (!part.HasImage && part.EffectNode == null)
            {
                text += " (未顯示)";
            }
            btn.Text = text;
            btn.NeedRecalcSize = true;
            btn.Refresh();
        }

        private string GetColorName(int ID)
        {
            GearType type = Gear.GetGearType(ID);
            if (Gear.IsFace(type))
            {
                return GetMixColorName(ID / 100 % 10, ID);
            }
            if (Gear.IsHair(type))
            {
                return GetMixColorName(ID % 10, ID);
            }
            return null;
        }

        private string GetMixColorName(int mixColor, int baseID)
        {
            GearType type = Gear.GetGearType(baseID);
            if (Gear.IsFace(type))
            {
                return AvatarPartButtonItem.LensColors[mixColor];
            }
            if (Gear.IsHair(type))
            {
                return AvatarPartButtonItem.HairColors[mixColor];
            }
            return null;
        }

        private void FillBodyActionFrame()
        {
            ComboItem actionItem = cmbActionBody.SelectedItem as ComboItem;
            if (actionItem != null)
            {
                var frames = avatar.GetActionFrames(actionItem.Text);
                FillComboItems(cmbBodyFrame, frames);
            }
            else
            {
                cmbBodyFrame.Items.Clear();
            }
        }

        private void FillEmotionFrame()
        {
            ComboItem emotionItem = cmbEmotion.SelectedItem as ComboItem;
            if (emotionItem != null)
            {
                var frames = avatar.GetFaceFrames(emotionItem.Text);
                FillComboItems(cmbEmotionFrame, frames);
            }
            else
            {
                cmbEmotionFrame.Items.Clear();
            }
        }

        private void FillTamingActionFrame()
        {
            ComboItem actionItem = cmbActionTaming.SelectedItem as ComboItem;
            if (actionItem != null)
            {
                var frames = avatar.GetTamingFrames(actionItem.Text);
                FillComboItems(cmbTamingFrame, frames);
            }
            else
            {
                cmbTamingFrame.Items.Clear();
            }
        }

        /// <summary>
        /// cmbEffectFrames에 이펙트의 프레임을 ActionFrame[]으로 불러옵니다.
        /// </summary>
        private void FillEffectFrame()
        {
            for (int i = 0; i < this.cmbEffectFrames.Length; i++)
            {
                ComboItem actionItem = cmbActionEffects[i].SelectedItem as ComboItem;
                if (actionItem != null)
                {
                    ActionFrame[] frames = avatar.GetEffectFrames(actionItem.Text, i);
                    FillComboItems(cmbEffectFrames[i], frames);
                }
                else
                {
                    cmbEffectFrames[i].Items.Clear();
                }
            }
        }

        private void FillWeaponIdx()
        {
            FillComboItems(cmbWeaponIdx, 0, 4);
        }

        private void FillEarSelection()
        {
            FillComboItems(cmbEar, 0, 4);
        }

        private void FillComboItems(ComboBoxEx comboBox, int start, int count)
        {
            List<ComboItem> items = new List<ComboItem>(count);
            for (int i = 0; i < count; i++)
            {
                ComboItem item = new ComboItem();
                item.Text = (start + i).ToString();
                items.Add(item);
            }
            FillComboItems(comboBox, items);
        }

        private void FillComboItems(ComboBoxEx comboBox, IEnumerable<string> items)
        {
            List<ComboItem> _items = new List<ComboItem>();
            foreach (var itemText in items)
            {
                ComboItem item = new ComboItem();
                item.Text = itemText;
                _items.Add(item);
            }
            FillComboItems(comboBox, _items);
        }

        private void FillComboItems(ComboBoxEx comboBox, IEnumerable<ActionFrame> frames)
        {
            List<ComboItem> items = new List<ComboItem>();
            int i = 0;
            foreach (var f in frames)
            {
                ComboItem item = new ComboItem();
                item.Text = (i++).ToString();
                item.Tag = f;
                items.Add(item);
            }
            FillComboItems(comboBox, items);
        }

        private void FillComboItems(ComboBoxEx comboBox, IEnumerable<ComboItem> items)
        {
            //保持原有选项
            var oldSelection = comboBox.SelectedItem as ComboItem;
            int? newSelection = null;
            comboBox.BeginUpdate();
            comboBox.Items.Clear();

            foreach (var item in items)
            {
                comboBox.Items.Add(item);

                if (newSelection == null && oldSelection != null)
                {
                    if (item.Text == oldSelection.Text)
                    {
                        newSelection = comboBox.Items.Count - 1;
                    }
                }
            }

            //恢复原有选项
            if (comboBox.Items.Count > 0)
            {
                comboBox.SelectedIndex = newSelection ?? 0;
            }

            comboBox.EndUpdate();
        }

        private bool GetSelectedActionFrame(ComboBoxEx comboBox, out int frameIndex, out ActionFrame actionFrame)
        {
            var selectedItem = comboBox.SelectedItem as ComboItem;
            if (selectedItem != null
                && int.TryParse(selectedItem.Text, out frameIndex)
                && selectedItem?.Tag is ActionFrame _actionFrame)
            {
                actionFrame = _actionFrame;
                return true;
            }
            else
            {
                frameIndex = -1;
                actionFrame = null;
                return false;
            }
        }

        private bool GetSelectedBodyFrame(out int frameIndex, out ActionFrame actionFrame)
        {
            return this.GetSelectedActionFrame(this.cmbBodyFrame, out frameIndex, out actionFrame);
        }

        private bool GetSelectedEmotionFrame(out int frameIndex, out ActionFrame actionFrame)
        {
            return this.GetSelectedActionFrame(this.cmbEmotionFrame, out frameIndex, out actionFrame);
        }

        private bool GetSelectedTamingFrame(out int frameIndex, out ActionFrame actionFrame)
        {
            return this.GetSelectedActionFrame(this.cmbTamingFrame, out frameIndex, out actionFrame);
        }

        private bool GetSelectedEffectFrames(out int[] frameIndex, out ActionFrame[] actionFrame)
        {
            var frameIndexs = new List<int>();
            var actionFrames = new List<ActionFrame>();
            foreach (var cmb in this.cmbEffectFrames)
            {
                this.GetSelectedActionFrame(cmb, out int fi, out ActionFrame af);
                frameIndexs.Add(fi);
                actionFrames.Add(af);
            }
            frameIndex = frameIndexs.ToArray();
            actionFrame = actionFrames.ToArray();
            return frameIndex.Count(x => x >= 0) > 0 && actionFrame.Count(x => x != null) > 0;
        }
        #endregion

        private void cmbActionBody_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.SuspendUpdateDisplay();
            FillBodyActionFrame();

            this.updatingActionEffect = true;
            var selectedItem = this.cmbActionBody.SelectedItem as ComboItem;
            SelectEffectAction(selectedItem.Text); // effect action is bounded to body action
            FillEffectFrame();
            this.updatingActionEffect = false;

            this.ResumeUpdateDisplay();
            UpdateDisplay();
        }

        private void cmbEmotion_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.SuspendUpdateDisplay();
            FillEmotionFrame();
            this.ResumeUpdateDisplay();
            UpdateDisplay();
        }

        private void cmbActionTaming_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.SuspendUpdateDisplay();
            FillTamingActionFrame();
            SetTamingDefaultBodyAction();
            SetTamingDefault();
            this.ResumeUpdateDisplay();
            UpdateDisplay();
        }

        private void cmbActionEffect_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.updatingActionEffect) return;
            this.SuspendUpdateDisplay();
            FillEffectFrame();
            this.ResumeUpdateDisplay();
            UpdateDisplay();
        }

        private void cmbBodyFrame_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateDisplay();
        }

        private void cmbEmotionFrame_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateDisplay();
        }

        private void cmbTamingFrame_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateDisplay();
        }

        private void cmbEffectFrames_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateDisplay();
        }

        private void cmbWeaponType_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateDisplay();
        }

        private void cmbWeaponIdx_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateDisplay();
        }

        private void cmbEar_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateDisplay();
        }

        private void cmbGroupChair_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.SuspendUpdateDisplay();
            var part = this.avatar.GroupChairChanged((Convert.ToInt32((sender as ComboBoxEx).Text) - 1).ToString());
            if (part != null)
            {
                OnNewPartAdded(part);
                FillAvatarParts();
            }
            FillEffectFrame();
            this.ResumeUpdateDisplay();
            UpdateDisplay();
        }

        private void chkBodyPlay_CheckedChanged(object sender, EventArgs e)
        {
            setBodyDelay();
        }

        public void setBodyDelay()
        {
            if (chkBodyPlay.Checked)
            {
                if (!this.timer1.Enabled)
                {
                    AnimateStart();
                }

                if (this.GetSelectedBodyFrame(out _, out var actionFrame) && actionFrame.AbsoluteDelay > 0)
                {
                    this.animator.BodyDelay = actionFrame.AbsoluteDelay;
                }
                setEffectDelay(false);
            }
            else
            {
                this.animator.BodyDelay = -1;
                setEffectDelay(true);
                TimerEnabledCheck();
            }
        }

        public void setTamingDelay()
        {
            if (chkTamingPlay.Checked)
            {
                if (!this.timer1.Enabled)
                {
                    AnimateStart();
                }

                if (this.GetSelectedTamingFrame(out _, out var actionFrame) && actionFrame.AbsoluteDelay > 0)
                {
                    this.animator.TamingDelay = actionFrame.AbsoluteDelay;
                }
                setChairDelay(false);
            }
            else
            {
                this.animator.TamingDelay = -1;
                setChairDelay(true);
                TimerEnabledCheck();
            }
        }

        public void setEffectDelay(bool init = false)
        {
            if (init)
            {
                for (int i = 0; i < this.animator.EffectDelay.Length; i++)
                {
                    if (i == AvatarCanvas.IndexChairLayer1 || i == AvatarCanvas.IndexChairLayer2
                        || i == AvatarCanvas.IndexChairEffectLayer1 || i == AvatarCanvas.IndexChairEffectLayer2)
                        continue;

                    this.animator.EffectDelay[i] = -1;
                }
            }
            else
            {
                bool effects = this.GetSelectedEffectFrames(out _, out var effectFrame);
                if (effects)
                {
                    for (int i = 0; i < effectFrame.Length; i++)
                    {
                        if (i == AvatarCanvas.IndexChairLayer1 || i == AvatarCanvas.IndexChairLayer2
                            || i == AvatarCanvas.IndexChairEffectLayer1 || i == AvatarCanvas.IndexChairEffectLayer2)
                            continue;

                        if (effectFrame[i]?.AbsoluteDelay > 0)
                        {
                            this.animator.EffectDelay[i] = effectFrame[i].AbsoluteDelay;
                        }
                    }
                }
            }
        }

        public void setChairDelay(bool init = false)
        {
            var index = new[] { AvatarCanvas.IndexChairLayer1, AvatarCanvas.IndexChairLayer2, AvatarCanvas.IndexChairEffectLayer1, AvatarCanvas.IndexChairEffectLayer2 };

            if (init)
            {
                foreach (int i in index)
                {
                    this.animator.EffectDelay[i] = -1;
                }
            }
            else
            {
                bool effects = this.GetSelectedEffectFrames(out _, out var effectFrame);
                if (effects)
                {
                    foreach (int i in index)
                    {
                        if (effectFrame[i]?.AbsoluteDelay > 0)
                        {
                            this.animator.EffectDelay[i] = effectFrame[i].AbsoluteDelay;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Body 프레임과 의자를 제외한 ItemEffect 프레임을 모두 0으로 맞춥니다.
        /// </summary>
        private void SyncBodyEffect()
        {
            if (this.cmbBodyFrame.Items.Count > 0)
            {
                this.cmbBodyFrame.SelectedIndex = 0;
            }

            for (int i = 0; i < cmbEffectFrames.Length; i++)
            {
                if (i == AvatarCanvas.IndexChairLayer1 || i == AvatarCanvas.IndexChairLayer2
                    || i == AvatarCanvas.IndexChairEffectLayer1 || i == AvatarCanvas.IndexChairEffectLayer2)
                    continue;

                if (this.cmbEffectFrames[i].Items.Count > 0)
                {
                    this.cmbEffectFrames[i].SelectedIndex = 0;
                }
            }

            setBodyDelay();
        }

        /// <summary>
        /// TamingMob 프레임과 의자의 프레임을 모두 0으로 맞춥니다.
        /// </summary>
        private void SyncTamingEffect()
        {
            var index = new[] { AvatarCanvas.IndexChairLayer1, AvatarCanvas.IndexChairLayer2, AvatarCanvas.IndexChairEffectLayer1, AvatarCanvas.IndexChairEffectLayer2 };

            if (this.cmbTamingFrame.Items.Count > 0)
            {
                this.cmbTamingFrame.SelectedIndex = 0;
            }

            foreach (int i in index)
            {
                if (this.cmbEffectFrames[i].Items.Count > 0)
                {
                    this.cmbEffectFrames[i].SelectedIndex = 0;
                }
            }

            setTamingDelay();
        }

        private void chkEmotionPlay_CheckedChanged(object sender, EventArgs e)
        {
            if (chkEmotionPlay.Checked)
            {
                if (!this.timer1.Enabled)
                {
                    AnimateStart();
                }

                if (this.GetSelectedEmotionFrame(out _, out var actionFrame) && actionFrame.AbsoluteDelay > 0)
                {
                    this.animator.EmotionDelay = actionFrame.AbsoluteDelay;
                }
            }
            else
            {
                this.animator.EmotionDelay = -1;
                TimerEnabledCheck();
            }
        }

        private void chkTamingPlay_CheckedChanged(object sender, EventArgs e)
        {
            setTamingDelay();
        }

        private void chkHairCover_CheckedChanged(object sender, EventArgs e)
        {
            avatar.HairCover = chkHairCover.Checked;
            UpdateDisplay();
        }

        private void chkHairShade_CheckedChanged(object sender, EventArgs e)
        {
            avatar.ShowHairShade = chkHairShade.Checked;
            UpdateDisplay();
        }

        private void chkApplyBRM_CheckedChanged(object sender, EventArgs e)
        {
            avatar.ApplyBRM = chkApplyBRM.Checked;
            this.avatarContainer1.ClearAllCache();
            UpdateDisplay();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            this.animator.Elapse(timer1.Interval);
            this.AnimateUpdate();
            int interval = this.animator.NextFrameDelay;

            if (interval <= 0)
            {
                this.timer1.Stop();
            }
            else
            {
                this.timer1.Interval = interval;
            }
        }

        private void AnimateUpdate()
        {
            this.SuspendUpdateDisplay();
            this.animator.SuspendUpdate();

            var beforeTamingFrameIdx = cmbTamingFrame.SelectedIndex;

            if (this.animator.BodyDelay == 0 && FindNextFrame(cmbBodyFrame) && this.GetSelectedBodyFrame(out _, out var bodyFrame))
            {
                this.animator.BodyDelay = bodyFrame.AbsoluteDelay;
            }

            if (this.animator.EmotionDelay == 0 && FindNextFrame(cmbEmotionFrame) && this.GetSelectedEmotionFrame(out _, out var emoFrame))
            {
                this.animator.EmotionDelay = emoFrame.AbsoluteDelay;
            }

            if (this.animator.TamingDelay == 0 && FindNextFrame(cmbTamingFrame) && this.GetSelectedTamingFrame(out _, out var tamingFrame))
            {
                this.animator.TamingDelay = tamingFrame.AbsoluteDelay;
            }

            for (int i = 0; i < this.cmbEffectFrames.Length; i++)
            {
                if (this.animator.EffectDelay[i] == 0 && FindNextFrame(cmbEffectFrames[i]) && this.GetSelectedActionFrame(cmbEffectFrames[i], out _, out var effectFrame))
                {
                    this.animator.EffectDelay[i] = effectFrame.AbsoluteDelay;
                }
            }

            this.animator.TrigUpdate();
            this.ResumeUpdateDisplay();
        }

        private void AnimateStart()
        {
            TimerEnabledCheck();
            if (timer1.Enabled)
            {
                AnimateUpdate();
            }
        }

        private void TimerEnabledCheck()
        {
            if (chkBodyPlay.Checked || chkEmotionPlay.Checked || chkTamingPlay.Checked)
            {
                if (!this.timer1.Enabled)
                {
                    this.timer1.Interval = 1;
                    this.timer1.Start();
                }
            }
            else
            {
                AnimateStop();
            }
        }

        private void AnimateStop()
        {
            chkBodyPlay.Checked = false;
            chkEmotionPlay.Checked = false;
            chkTamingPlay.Checked = false;
            this.timer1.Stop();
        }

        private bool FindNextFrame(ComboBoxEx cmbFrames)
        {
            ComboItem item = cmbFrames.SelectedItem as ComboItem;
            if (item == null)
            {
                if (cmbFrames.Items.Count > 0)
                {
                    cmbFrames.SelectedIndex = 0;
                    return true;
                }
                else
                {
                    return false;
                }
            }

            int selectedIndex = cmbFrames.SelectedIndex;
            int i = selectedIndex;
            do
            {
                i = (++i) % cmbFrames.Items.Count;
                item = cmbFrames.Items[i] as ComboItem;
                if (item != null && item.Tag is ActionFrame actionFrame && actionFrame.AbsoluteDelay > 0)
                {
                    cmbFrames.SelectedIndex = i;
                    return true;
                }
            }
            while (i != selectedIndex);

            return false;
        }

        private void btnCode_Click(object sender, EventArgs e)
        {
            var dlg = new AvatarCodeForm();
            string code = GetAllPartsTag();
            dlg.CodeText = code;
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                if (dlg.CodeText != code && !string.IsNullOrEmpty(dlg.CodeText))
                {
                    LoadCode(dlg.CodeText, dlg.LoadType);
                }
            }
        }

        private void btnMale_Click(object sender, EventArgs e)
        {
            if (this.avatar.Parts.All(part => part == null)
                || MessageBoxEx.Show("要呼出男角色的紙娃娃預設嗎？", "確認") == DialogResult.OK)
            {
                LoadCode("2000,12000,20000,30000,1040036,1060026", 0);
            }
        }

        private void btnFemale_Click(object sender, EventArgs e)
        {
            if (this.avatar.Parts.All(part => part == null)
                || MessageBoxEx.Show("要呼出女角色的紙娃娃預設嗎？", "確認") == DialogResult.OK)
            {
                LoadCode("2000,12000,21000,31000,1041046,1061039", 0);
            }
        }


        private void btnLara_Click(object sender, EventArgs e)
        {
            switch (MessageBoxEx.Show("要呼出菈菈的紙娃娃預設嗎？\r\n\r\nYes - 女\r\nNo - 男", "確認", MessageBoxButtons.YesNoCancel))
            {
                case DialogResult.Yes:
                    LoadCode("2003,12003,51484,61183,1051667,1073551,1372243", 0);
                    return;

                case DialogResult.No:
                    LoadCode("2003,12003,50469,60193,1050593,1073551,1372243", 0);
                    return;

                case DialogResult.Cancel:
                default:
                    return;
            }
        }
        private void btnPopuko_Click(object sender, EventArgs e)
        {
            switch (MessageBoxEx.Show("要呼出pop子的紙娃娃預設嗎？", "確認", MessageBoxButtons.YesNo))
            {
                case DialogResult.Yes:
                    LoadCode("2000,12000,21000,1005031,1053233,1073245", 0);
                    return;

                case DialogResult.No:
                default:
                    return;
            }
        }
        private void btnPipimi_Click(object sender, EventArgs e)
        {
            switch (MessageBoxEx.Show("要呼出pipi美的紙娃娃預設嗎？", "確認", MessageBoxButtons.YesNo))
            {
                case DialogResult.Yes:
                    LoadCode("2000,12000,21000,1005028,1053233,1073245", 0);
                    return;

                case DialogResult.No:
                default:
                    return;
            }
        }
        private void btnMegumin_Click(object sender, EventArgs e)
        {
            switch (MessageBoxEx.Show("要呼出慧慧的紙娃娃預設嗎？", "確認", MessageBoxButtons.YesNo))
            {
                case DialogResult.Yes:
                    LoadCode("2000,12000,56037,1006342,1054041,1073789,1703397,1012701", 0);
                    return;

                case DialogResult.No:
                default:
                    return;
            }
        }
        private void btnAqua_Click(object sender, EventArgs e)
        {
            switch (MessageBoxEx.Show("要呼出阿克婭的紙娃娃預設嗎？", "確認", MessageBoxButtons.YesNo))
            {
                case DialogResult.Yes:
                    LoadCode("2000,12000,56036,32750,1054040,1073788,1012804", 0);
                    return;

                case DialogResult.No:
                default:
                    return;
            }
        }
        private void btnDarkness_Click(object sender, EventArgs e)
        {
            switch (MessageBoxEx.Show("要呼出達克尼斯的紙娃娃預設嗎？", "確認", MessageBoxButtons.YesNo))
            {
                case DialogResult.Yes:
                    LoadCode("2000,12000,56038,42240,1054042,1073790", 0);
                    return;

                case DialogResult.No:
                default:
                    return;
            }
        }
        private void btnTanjiroKamado_Click(object sender, EventArgs e)
        {
            switch (MessageBoxEx.Show("要呼出竈門炭治郎的紙娃娃預設嗎？", "確認", MessageBoxButtons.YesNo))
            {
                case DialogResult.Yes:
                    LoadCode("2000,12000,55074,63820,1054199,1073962,1703528,1022344", 0);
                    return;

                case DialogResult.No:
                default:
                    return;
            }
        }
        private void btnNezukoKamado_Click(object sender, EventArgs e)
        {
            switch (MessageBoxEx.Show("要呼出竈門禰豆子的紙娃娃預設嗎？", "確認", MessageBoxButtons.YesNo))
            {
                case DialogResult.Yes:
                    LoadCode("2000,12000,56095,67490,1054200,1073963,1012832", 0);
                    return;

                case DialogResult.No:
                default:
                    return;
            }
        }
        private void btnZenitsuAgatsuma_Click(object sender, EventArgs e)
        {
            switch (MessageBoxEx.Show("要呼出我妻善逸的紙娃娃預設嗎？", "確認", MessageBoxButtons.YesNo))
            {
                case DialogResult.Yes:
                    LoadCode("2000,12000,55075,63830,1054201,1073964,1703529", 0);
                    return;

                case DialogResult.No:
                default:
                    return;
            }
        }
        private void btnInosukeHashibira_Click(object sender, EventArgs e)
        {
            switch (MessageBoxEx.Show("要呼出嘴平伊之助的紙娃娃預設嗎？", "確認", MessageBoxButtons.YesNo))
            {
                case DialogResult.Yes:
                    LoadCode("2000,12000,20000,30000,1006567,1050783,1073965,1103743,1703530", 0);
                    return;

                case DialogResult.No:
                default:
                    return;
            }
        }
        private void btnLaraTheSheep_Click(object sender, EventArgs e)
        {
            switch (MessageBoxEx.Show("要呼出作者的菈菈紙娃娃預設嗎？", "確認", MessageBoxButtons.YesNo))
            {
                case DialogResult.Yes:
                    LoadCode("2000,12000,51484,61183,1003968,1052667,1072867,1082552,1703113", 0);
                    return;

                case DialogResult.No:
                default:
                    return;
            }
        }

        private void btnCustomPreset_Click(object sender, EventArgs e)
        {
            string avatarPath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Images");
            if (LoadAvatarForm.Instance == null)
            {
                new LoadAvatarForm().Show();
            }
            else
            {
                LoadAvatarForm.Instance.Show();
            }
            LoadAvatarForm._files.Clear();
            if (!File.Exists(avatarPath))
            {
                System.IO.Directory.CreateDirectory(avatarPath);
            }
            string[] files = Directory.GetFiles(avatarPath);
            LoadAvatarForm._files.AddRange(files);
            LoadAvatarForm.LoadImages();
        }

        public void SavePreset(string pendingCode)
        {
            if (string.IsNullOrEmpty(pendingCode)) return;
            string avatarPresetPath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Images", pendingCode.Replace("*", "×") + ".png");
            this.GetSelectedBodyFrame(out int bodyFrame, out _);
            this.GetSelectedEmotionFrame(out int emoFrame, out _);
            this.GetSelectedTamingFrame(out int tamingFrame, out _);
            this.GetSelectedEffectFrames(out int[] effectFrames, out _);
            var bone = this.avatar.CreateFrame(bodyFrame, emoFrame, tamingFrame, effectFrames);
            var frame = this.avatar.DrawFrame(bone);
            frame.Bitmap.Save(avatarPresetPath, System.Drawing.Imaging.ImageFormat.Png);
        }

        private void btnAngelicBuster_Click(object sender, EventArgs e)
        {
            switch (MessageBoxEx.Show("要呼出天使破壞者的紙娃娃預設嗎？\r\n\r\nYes - 新\r\nNo - 舊", "確認", MessageBoxButtons.YesNoCancel))
            {
                case DialogResult.Yes:
                    LoadCode("2000,12000,56660,68181,1051812,1222000", 0);
                    return;

                case DialogResult.No:
                    LoadCode("2000,12000,21173,37141,1051291,1222000", 0);
                    return;

                case DialogResult.Cancel:
                default:
                    return;
            }
        }
        private void btnZero_Click(object sender, EventArgs e)
        {
            switch (MessageBoxEx.Show("要呼出神之子的紙娃娃預設嗎？\r\n\r\nYes - 阿爾法\r\nNo - 蓓塔", "確認", MessageBoxButtons.YesNoCancel))
            {
                case DialogResult.Yes:
                    LoadCode("2000,12000,20294,36633,1052606,1072814,1082521,1102552,1572001", 0);
                    return;

                case DialogResult.No:
                    LoadCode("2000,12000,21290,37623,1052607,1072814,1082521,1102552,1562001", 0);
                    return;

                case DialogResult.Cancel:
                default:
                    return;
            }
        }

        private void btnPathfinder_Click(object sender, EventArgs e)
        {
            switch (MessageBoxEx.Show("要呼出開拓者的紙娃娃預設嗎？\r\n\r\nYes - 女\r\nNo - 男", "確認", MessageBoxButtons.YesNoCancel))
            {
                case DialogResult.Yes:
                    LoadCode("2000,12000,28141,47525,1005313,1053402,1073334,1592000", 0);
                    return;

                case DialogResult.No:
                    LoadCode("2000,12000,27135,46025,1005313,1053402,1073334,1592000", 0);
                    return;

                case DialogResult.Cancel:
                default:
                    return;
            }
        }

        private void btnLynn_Click(object sender, EventArgs e)
        {
            switch (MessageBoxEx.Show("要呼出琳恩的紙娃娃預設嗎？\r\n\r\nYes - 女\r\nNo - 男", "確認", MessageBoxButtons.YesNoCancel))
            {
                case DialogResult.Yes:
                    LoadCode("2033,12033,56262,68197,1001163,1051821,1073839,1252000", 0);
                    return;

                case DialogResult.No:
                    LoadCode("2033,12033,55241,65147,1000137,1050742,1073839,1252000", 0);
                    return;

                case DialogResult.Cancel:
                default:
                    return;
            }
        }

        private void btnHayato_Click(object sender, EventArgs e)
        {
            switch (MessageBoxEx.Show("要呼出劍豪的紙娃娃預設嗎？\r\n\r\nYes - 男\r\nNo - 女", "確認", MessageBoxButtons.YesNoCancel))
            {
                case DialogResult.Yes:
                    LoadCode("2000,12000,20273,36207,1003557,1052466,1072671,1082437,1542011", 0);
                    return;

                case DialogResult.No:
                    LoadCode("2000,12000,24364,38891,1003557,1052466,1072671,1082437,1542011", 0);
                    return;

                case DialogResult.Cancel:
                default:
                    return;
            }
        }
        private void btnBeastTamer_Click(object sender, EventArgs e)
        {
            switch (MessageBoxEx.Show("要呼出幻獸師的紙娃娃預設嗎？\r\n\r\nYes - 女\r\nNo - 男", "確認", MessageBoxButtons.YesNoCancel))
            {
                case DialogResult.Yes:
                    LoadCode("2000,12000,24209,38257,1004062,1051378,1072902,1102661,1259000", 0);
                    return;

                case DialogResult.No:
                    LoadCode("2000,12000,23213,35177,1004062,1050307,1072902,1102661,1259000", 0);
                    return;

                case DialogResult.Cancel:
                default:
                    return;
            }
        }
        private void btnKanna_Click(object sender, EventArgs e)
        {
            switch (MessageBoxEx.Show("要呼出陰陽師的紙娃娃預設嗎？\r\n\r\nYes - 女\r\nNo - 男", "確認", MessageBoxButtons.YesNoCancel))
            {
                case DialogResult.Yes:
                    LoadCode("2000,12000,21676,37086,1003573,1052482,1072674,1082453,1552011", 0);
                    return;

                case DialogResult.No:
                    LoadCode("2000,12000,23665,35825,1000064,1052482,1072674,1082453,1552011", 0);
                    return;

                case DialogResult.Cancel:
                default:
                    return;
            }
        }

        private void btnOldBokugen_Click(object sender, EventArgs e)
        {
            switch (MessageBoxEx.Show("要呼出墨玄(舊版)的紙娃娃預設嗎？\r\n\r\nYes - 男\r\nNo - 女", "確認", MessageBoxButtons.YesNoCancel))
            {
                case DialogResult.Yes:
                    LoadCode("2000,12000,50646,60050,1050575,1073510,1403000", 0);
                    return;

                case DialogResult.No:
                    LoadCode("2000,12000,51659,61040,1051647,1073897,1403000", 0);
                    return;

                case DialogResult.Cancel:
                default:
                    return;
            }
        }

        private void btnNewBokugen_Click(object sender, EventArgs e)
        {
            switch (MessageBoxEx.Show("要呼出墨玄(新版)的紙娃娃預設嗎？\r\n\r\nYes - 男\r\nNo - 女", "確認", MessageBoxButtons.YesNoCancel))
            {
                case DialogResult.Yes:
                    LoadCode("2041,12041,55661,63740,1050758,1070188,1403000", 0);
                    return;

                case DialogResult.No:
                    LoadCode("2041,12041,56683,67230,1051837,1071203,1403000", 0);
                    return;

                case DialogResult.Cancel:
                default:
                    return;
            }
        }

        private void btnSaveAsGif_Click(object sender, EventArgs e)
        {
            if (this.avatar.Body == null || this.avatar.Head == null)
            {
                MessageBoxEx.Show("未呼出角色。");
                return;
            }

            SaveGif(sender, e, chkBodyPlay.Checked, chkEmotionPlay.Checked, chkTamingPlay.Checked);
        }

        private async void btnAPI_Click(object sender, EventArgs e)
        {
#if NET6_0_OR_GREATER
            if (PluginManager.FindWz(Wz_Type.Base) == null)
            {
                ToastNotification.Show(this, $"錯誤：未開啟 Base.wz 檔案。", null, 2000, eToastGlowColor.Red, eToastPosition.TopCenter);
                return;
            }

            var dlg = new AvatarAPIForm();
            dlg.CharaName = characterName;
            dlg.selectedRegion = previousRegion;

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                characterName = dlg.CharaName;
                previousRegion = dlg.selectedRegion;
                string avatarCode;
                switch (dlg.selectedRegion)
                {
                    default:
                        ToastNotification.Show(this, $"請選擇角色歸屬地區。", null, 3000, eToastGlowColor.Red, eToastPosition.TopCenter);
                        return;
                    case 1: // KMS
                        this.API = new NexonOpenAPI("-");
                        try
                        {
                            ToastNotification.Show(this, $"正在取得紙娃娃情報，請稍等...", null, 3000, eToastGlowColor.Green, eToastPosition.TopCenter);
                            avatarCode = await this.API.GetAvatarCode(dlg.CharaName, "KMS");
                            if (string.IsNullOrEmpty(avatarCode))
                            {
                                ToastNotification.Show(this, $"找不到角色。", null, 3000, eToastGlowColor.Red, eToastPosition.TopCenter);
                            }
                            else
                            {
                                await Type3(avatarCode);
                            }
                        }
                        catch (Exception ex)
                        {
                            ToastNotification.Show(this, $"警告: {ex.Message}", null, 3000, eToastGlowColor.Orange, eToastPosition.TopCenter);
                        }
                        break;

                        // Following are unused
                        var key = ((string)WcR2Config.Default.NxOpenAPIKey).Trim();
                        if (string.IsNullOrEmpty(key))
                        {
                            ToastNotification.Show(this, $"請在設定中輸入您的 API 金鑰。", null, 3000, eToastGlowColor.Red, eToastPosition.TopCenter);
                            return;
                        }

                        try
                        {
                            if (this.API == null || !this.API.CheckSameAPIKey(key))
                            {
                                this.API = new NexonOpenAPI(key);
                            }

                            ToastNotification.Show(this, $"正在取得紙娃娃情報，請稍等...", null, 3000, eToastGlowColor.Green, eToastPosition.TopCenter);
                            var name = dlg.CharaName;
                            var ocid = await this.API.GetCharacterOCID(name);

                            if (dlg.Type1)
                            {
                                await Type1(ocid);
                            }
                            else
                            {
                                await Type2(ocid);
                            }
                        }
                        catch (Exception ex)
                        {
                            ToastNotification.Show(this, $"錯誤: {ex.Message}", null, 3000, eToastGlowColor.Red, eToastPosition.TopCenter);
                        }
                        break;
                    case 2: // JMS
                        this.API = new NexonOpenAPI("-");
                        try
                        {
                            ToastNotification.Show(this, $"正在取得紙娃娃情報，請稍等...", null, 3000, eToastGlowColor.Green, eToastPosition.TopCenter);
                            avatarCode = await this.API.GetAvatarCode(dlg.CharaName, "JMS");
                            if (string.IsNullOrEmpty(avatarCode))
                            {
                                ToastNotification.Show(this, $"找不到角色。", null, 3000, eToastGlowColor.Red, eToastPosition.TopCenter);
                            }
                            else
                            {
                                await Type3(avatarCode);
                            }
                        }
                        catch (Exception ex)
                        {
                            ToastNotification.Show(this, $"警告: {ex.Message}", null, 3000, eToastGlowColor.Orange, eToastPosition.TopCenter);
                        }
                        break;
                    case 4: // GMS-NA
                        this.API = new NexonOpenAPI("-");
                        try
                        {
                            ToastNotification.Show(this, $"正在取得紙娃娃情報，請稍等...", null, 3000, eToastGlowColor.Green, eToastPosition.TopCenter);
                            avatarCode = await this.API.GetAvatarCode(dlg.CharaName, "GMS-NA");
                            if (string.IsNullOrEmpty(avatarCode))
                            {
                                ToastNotification.Show(this, $"找不到角色。", null, 3000, eToastGlowColor.Red, eToastPosition.TopCenter);
                            }
                            else
                            {
                                await Type3(avatarCode);
                            }
                        }
                        catch (Exception ex)
                        {
                            ToastNotification.Show(this, $"警告: {ex.Message}", null, 3000, eToastGlowColor.Orange, eToastPosition.TopCenter);
                        }
                        break;
                    case 5: // GMS-EU
                        this.API = new NexonOpenAPI("-");
                        try
                        {
                            ToastNotification.Show(this, $"正在取得紙娃娃情報，請稍等...", null, 3000, eToastGlowColor.Green, eToastPosition.TopCenter);
                            avatarCode = await this.API.GetAvatarCode(dlg.CharaName, "GMS-EU");
                            if (string.IsNullOrEmpty(avatarCode))
                            {
                                ToastNotification.Show(this, $"找不到角色。", null, 3000, eToastGlowColor.Red, eToastPosition.TopCenter);
                            }
                            else
                            {
                                await Type3(avatarCode);
                            }
                        }
                        catch (Exception ex)
                        {
                            ToastNotification.Show(this, $"警告: {ex.Message}", null, 3000, eToastGlowColor.Orange, eToastPosition.TopCenter);
                        }
                        break;
                    case 6: // MSEA
                        this.API = new NexonOpenAPI("-");
                        try
                        {
                            ToastNotification.Show(this, $"正在取得紙娃娃情報，請稍等...", null, 3000, eToastGlowColor.Green, eToastPosition.TopCenter);
                            avatarCode = await this.API.GetAvatarCode(dlg.CharaName, "MSEA");
                            if (string.IsNullOrEmpty(avatarCode))
                            {
                                ToastNotification.Show(this, $"找不到角色。", null, 3000, eToastGlowColor.Red, eToastPosition.TopCenter);
                            }
                            else
                            {
                                await Type3(avatarCode);
                            }
                        }
                        catch (Exception ex)
                        {
                            ToastNotification.Show(this, $"警告: {ex.Message}", null, 3000, eToastGlowColor.Orange, eToastPosition.TopCenter);
                        }
                        break;
                    case 7: // TMS
                        this.API = new NexonOpenAPI("-");
                        try
                        {
                            ToastNotification.Show(this, $"正在取得紙娃娃情報，請稍等...", null, 3000, eToastGlowColor.Green, eToastPosition.TopCenter);
                            avatarCode = await this.API.GetAvatarCode(dlg.CharaName, "TMS");
                            if (string.IsNullOrEmpty(avatarCode))
                            {
                                ToastNotification.Show(this, $"找不到角色。", null, 3000, eToastGlowColor.Red, eToastPosition.TopCenter);
                            }
                            else
                            {
                                await Type4(avatarCode);
                            }
                        }
                        catch (Exception ex)
                        {
                            ToastNotification.Show(this, $"警告: {ex.Message}", null, 3000, eToastGlowColor.Orange, eToastPosition.TopCenter);
                        }
                        break;
                    case 8: // MSN
                        this.API = new NexonOpenAPI("-");
                        try
                        {
                            avatarCode = await this.API.GetAvatarCode(dlg.CharaName, "MSN");
                            if (string.IsNullOrEmpty(avatarCode))
                            {
                                ToastNotification.Show(this, $"找不到角色。", null, 3000, eToastGlowColor.Red, eToastPosition.TopCenter);
                            }
                            else
                            {
                                string[] decodedInfo = Encoding.UTF8.GetString(Convert.FromBase64String(avatarCode)).Split("a");
                                List<string> msnCode = new List<string> {};
                                msnCode.Add((Int32.Parse(decodedInfo[0]) + 2000).ToString());
                                msnCode.Add((Int32.Parse(decodedInfo[1]) + 12000).ToString());
                                foreach (string itemCode in decodedInfo.Skip(2))
                                {
                                    switch (itemCode.Length)
                                    {
                                        default:
                                            break;
                                        case 5:
                                        case 7:
                                            msnCode.Add(itemCode);
                                            break;
                                        case 8:
                                            msnCode.Add(itemCode.Substring(0, 5));
                                            break;

                                    }
                                }
                                LoadCode(string.Join(",", msnCode), 0);
                            }
                        }
                        catch (Exception ex)
                        {
                            ToastNotification.Show(this, $"警告: {ex.Message}", null, 3000, eToastGlowColor.Orange, eToastPosition.TopCenter);
                        }
                        break;
                }
            }

            async Task Type1(string ocid) // 외형 기준
            {
                UnpackedAvatarData res = await this.API.GetAvatarResult(ocid);

                var mixFace = int.Parse(res.MixFaceRatio) != 0 ? $"+{res.MixFaceColor}*{res.MixFaceRatio}" : "";
                var mixHair = int.Parse(res.MixHairRatio) != 0 ? $"+{res.MixHairColor}*{res.MixHairRatio}" : "";

                for (int i = 0; i < this.cmbEar.Items.Count; i++)
                {
                    if ((this.cmbEar.Items[i] as ComboItem).Text == res.EarType.ToString())
                    {
                        this.cmbEar.SelectedIndex = i;
                        break;
                    }
                }

                var code = $"20{res.Skin}, {res.Face + mixFace}, {res.Hair + mixHair}, {res.Cap}, {res.FaceAcc}, {res.EyeAcc}, {res.EarAcc}, {res.Coat}, {res.Pants}, {res.Shoes}, {res.Gloves}, {res.Cape}, {res.Shield}, {res.Weapon}, {res.CashWeapon}";
                LoadCode(code, 0);

                if (res.UnknownVer)
                {
                    throw new Exception($"未知代碼版本。 (版本: {res.Version})");
                }
            }

            async Task Type2(string ocid) // 장비창 기준
            {
                LoadedAvatarData res = await this.API.GetAvatarResult2(ocid);

                var skinID = FindIDFromString(res.SkinInfo["SkinName"], gender: 2);
                var faceID = FindIDFromString(res.FaceInfo["FaceName"], gender: res.Gender);
                var hairID = FindIDFromString(res.HairInfo["HairName"], gender: res.Gender);

                if (string.IsNullOrEmpty(skinID))
                {
                    throw new Exception($"連接並更新您的角色。");
                }

                if (!string.IsNullOrEmpty(faceID) && faceID.Length == 5)
                    faceID = faceID.Remove(2, 1).Insert(2, Array.IndexOf(AvatarCanvas.FaceColor, res.FaceInfo["BaseColor"]).ToString());
                if (!string.IsNullOrEmpty(hairID) && hairID.Length == 5)
                    hairID = hairID.Remove(4, 1).Insert(4, Array.IndexOf(AvatarCanvas.HairColor, res.HairInfo["BaseColor"]).ToString());

                var mixFace = !string.IsNullOrEmpty(res.FaceInfo["MixColor"]) ? $"+{Array.IndexOf(AvatarCanvas.FaceColor, res.FaceInfo["MixColor"])}*{res.FaceInfo["MixRate"]}" : "";
                var mixHair = !string.IsNullOrEmpty(res.HairInfo["MixColor"]) ? $"+{Array.IndexOf(AvatarCanvas.HairColor, res.HairInfo["MixColor"])}*{res.HairInfo["MixRate"]}" : "";

                LoadCode($"{skinID},{faceID + mixFace},{hairID + mixHair}", 0);
                foreach (var list in new[] { res.ItemList, res.CashBaseItemList, res.CashPresetItemList })
                {
                    if (list.Count > 0)
                        LoadCode(string.Join(",", list), 1);
                }
            }

            async Task Type3(string avatarCode) // raw avatarCode
            {
                UnpackedAvatarData res = await this.API.ParseAvatarCode(avatarCode);

                var mixFace = int.Parse(res.MixFaceRatio) != 0 ? $"+{res.MixFaceColor}*{res.MixFaceRatio}" : "";
                var mixHair = int.Parse(res.MixHairRatio) != 0 ? $"+{res.MixHairColor}*{res.MixHairRatio}" : "";

                for (int i = 0; i < this.cmbEar.Items.Count; i++)
                {
                    if ((this.cmbEar.Items[i] as ComboItem).Text == res.EarType.ToString())
                    {
                        this.cmbEar.SelectedIndex = i;
                        break;
                    }
                }

                var code = $"20{res.Skin}, {res.Face + mixFace}, {res.Hair + mixHair}, {res.Cap}, {res.FaceAcc}, {res.EyeAcc}, {res.EarAcc}, {res.Coat}, {res.Pants}, {res.Shoes}, {res.Gloves}, {res.Cape}, {res.Shield}, {res.Weapon}, {res.CashWeapon}";
                LoadCode(code, 0);

                if (res.UnknownVer)
                {
                    throw new Exception($"未知のコードバージョンです. (バージョン: {res.Version})");
                }
            }

            async Task Type4(string avatarCode) // TMS cipherText
            {
                UnpackedAvatarData res = await this.API.ParseCharacterLookCipherText(avatarCode);

                var mixFace = int.Parse(res.MixFaceRatio) != 0 ? $"+{res.MixFaceColor}*{res.MixFaceRatio}" : "";
                var mixHair = int.Parse(res.MixHairRatio) != 0 ? $"+{res.MixHairColor}*{res.MixHairRatio}" : "";

                for (int i = 0; i < this.cmbEar.Items.Count; i++)
                {
                    if ((this.cmbEar.Items[i] as ComboItem).Text == res.EarType.ToString())
                    {
                        this.cmbEar.SelectedIndex = i;
                        break;
                    }
                }

                var code = $"20{res.Skin}, {res.Face}, {res.Face + mixFace}, {res.Hair}, {res.Hair + mixHair}, {res.Cap}, {res.FaceAcc}, {res.EyeAcc}, {res.EarAcc}, {res.Coat}, {res.Pants}, {res.Shoes}, {res.Gloves}, {res.Cape}, {res.Shield}, {res.Weapon}, {res.CashWeapon}";
                LoadCode(code, 0);

                if (res.UnknownVer)
                {
                    throw new Exception($"未知のコードバージョンです. (バージョン: {res.Version})");
                }
            }
#else
            ToastNotification.Show(this, $"若要使用此功能，您必須使用.NET 6.0 或 .NET 8.0 版本。", null, 2000, eToastGlowColor.Red, eToastPosition.TopCenter);
            return;
#endif
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            this.avatarContainer1.Origin = new Point(this.avatarContainer1.Width / 2, this.avatarContainer1.Height / 2 + 40);
            this.avatarContainer1.Invalidate();
        }

        private void SaveGif(object sender, EventArgs e, bool isBodyPlayingChecked = true, bool isEmotionPlayingChecked = true, bool isTamingPlayingChecked = true, string outputFileName = null)
        { 
            bool bodyPlaying = isBodyPlayingChecked && cmbBodyFrame.Items.Count > 1;
            bool emoPlaying = isEmotionPlayingChecked && cmbEmotionFrame.Items.Count > 1;
            bool tamingPlaying = isTamingPlayingChecked && cmbTamingFrame.Items.Count > 1;
            bool effectPlaying = (bodyPlaying || (isTamingPlayingChecked && avatar.Chair != null)) ? (GetSelectedEffectFrames(out int[] frameIndex, out ActionFrame[] actionFrame) ? true : false) : false;
            string defaultFileName;

            int aniCount = new[] { bodyPlaying, emoPlaying, tamingPlaying }.Count(b => b);
            int effectCount = effectPlaying ? 1 : 0;
            aniCount += effectCount; // add effect parts

            if (aniCount == 0)
            {
                this.GetSelectedBodyFrame(out int bodyFrame, out _);
                this.GetSelectedEmotionFrame(out int emoFrame, out _);
                this.GetSelectedTamingFrame(out int tamingFrame, out _);
                this.GetSelectedEffectFrames(out int[] effectFrames, out _);

                defaultFileName = string.Format("avatar{0}{1}{2}{3}{4}.png",
                        string.IsNullOrEmpty(avatar.ActionName) ? "" : ("_" + avatar.ActionName + "(" + bodyFrame + ")"),
                        string.IsNullOrEmpty(avatar.EmotionName) ? "" : ("_" + avatar.EmotionName + "(" + emoFrame + ")"),
                        string.IsNullOrEmpty(avatar.TamingActionName) ? "" : ("_" + avatar.TamingActionName + "(" + tamingFrame + ")"),
                        (!string.IsNullOrEmpty(avatar.ActionName) && avatar.ActionName == "sit") ? ("_" + chairName) : "",
                        btnEnableAutosave.Checked ? ("_" + DateTime.Now.ToString("yyyyMMdd_HHmmss")) : "");

                // no animation is playing, save as png
                if (!btnEnableAutosave.Checked)
                {
                    var dlg = new SaveFileDialog()
                    {
                        Title = "紙娃娃幀保存",
                        Filter = "PNG (*.png)|*.png|*.*|*.*",
                        FileName = defaultFileName
                    };
                    if (dlg.ShowDialog() != DialogResult.OK)
                    {
                        return;
                    }
                    outputFileName = dlg.FileName;
                }
                else
                {
                    outputFileName = Path.Combine(specifiedSavePath, defaultFileName.Replace('\\', '.'));
                }

                var bone = this.avatar.CreateFrame(bodyFrame, emoFrame, tamingFrame, effectFrames);
                var frame = this.avatar.DrawFrame(bone);
                frame.Bitmap.Save(outputFileName, System.Drawing.Imaging.ImageFormat.Png);
            }
            else
            {
                var config = ImageHandlerConfig.Default;
                using var encoder = AnimateEncoderFactory.CreateEncoder(config);
                var cap = encoder.Compatibility;
                string extensionFilter = string.Join(";", cap.SupportedExtensions.Select(ext => $"*{ext}"));

                defaultFileName = string.Format("avatar{0}{1}{2}{3}{4}{5}",
                        string.IsNullOrEmpty(avatar.ActionName) ? "" : ("_" + avatar.ActionName),
                        string.IsNullOrEmpty(avatar.EmotionName) ? "" : ("_" + avatar.EmotionName),
                        string.IsNullOrEmpty(avatar.TamingActionName) ? "" : ("_" + avatar.TamingActionName),
                        (!string.IsNullOrEmpty(avatar.ActionName) && avatar.ActionName == "sit") ? ("_" + chairName) : "",
                        btnEnableAutosave.Checked ? ("_" + DateTime.Now.ToString("yyyyMMdd_HHmmss")) : "",
                        cap.DefaultExtension);

                if (!btnEnableAutosave.Checked)
                {
                    var dlg = new SaveFileDialog()
                    {
                        Title = "保存紙娃娃",
                        Filter = string.Format("{0} (*{1})|*{1}|所有格式 (*.*)|*.*", encoder.Name, extensionFilter),
                        FileName = defaultFileName
                    };
                    if (dlg.ShowDialog() != DialogResult.OK)
                    {
                        return;
                    }
                    outputFileName = dlg.FileName;
                }
                else
                {
                    outputFileName = System.IO.Path.Combine(specifiedSavePath, defaultFileName.Replace('\\', '.'));
                }

                var actPlaying = new[] { bodyPlaying, emoPlaying, tamingPlaying };
                var actFrames = new[] { cmbBodyFrame, cmbEmotionFrame, cmbTamingFrame }
                    .Select((cmb, i) =>
                    {
                        if (actPlaying[i])
                        {
                            return cmb.Items.OfType<ComboItem>().Select(cmbItem => new
                            {
                                index = int.Parse(cmbItem.Text),
                                actionFrame = cmbItem.Tag as ActionFrame,
                            }).ToArray();
                        }
                        else if (this.GetSelectedActionFrame(cmb, out var index, out var actionFrame))
                        {
                            return new[] { new { index, actionFrame } };
                        }
                        else
                        {
                            return null;
                        }
                    }).ToArray();
                var effectActFrames = cmbEffectFrames // get ActionFrame array from effect combobox
                    .Select((cmb, i) =>
                    {
                        if (effectPlaying && avatar.IsPartEffectVisible(i)) // effect playing is bounded to body playing or taming playing with Chair part
                        {
                            return cmb.Items.OfType<ComboItem>().Select(cmbItem => new
                            {
                                index = int.Parse(cmbItem.Text),
                                actionFrame = cmbItem.Tag as ActionFrame,
                            }).ToArray();
                        }
                        else if (this.GetSelectedActionFrame(cmb, out var index, out var actionFrame))
                        {
                            return new[] { new { index, actionFrame } };
                        }
                        else
                        {
                            return null;
                        }
                    }).ToArray();

                var gifLayer = new GifLayer();

                if (aniCount == 1 && !cap.IsFixedFrameRate && !effectPlaying)
                {
                    int aniActIndex = Array.FindIndex(actPlaying, b => b);
                    for (int fIdx = 0, fCnt = actFrames[aniActIndex].Length; fIdx < fCnt; fIdx++)
                    {
                        int[] actionIndices = new int[] { -1, -1, -1 };
                        int delay = 0;
                        for (int i = 0; i < actFrames.Length; i++)
                        {
                            var act = actFrames[i];
                            if (i == aniActIndex)
                            {
                                actionIndices[i] = act[fIdx].index;
                                delay = act[fIdx].actionFrame.AbsoluteDelay;
                            }
                            else if (act != null)
                            {
                                actionIndices[i] = act[0].index;
                            }
                        }
                        var bone = this.avatar.CreateFrame(actionIndices[0], actionIndices[1], actionIndices[2], null);
                        var frameData = this.avatar.DrawFrame(bone);
                        gifLayer.AddFrame(new GifFrame(frameData.Bitmap, frameData.Origin, delay));
                    }
                }
                else
                {
                    // more than 2 animating action parts, for simplicity, we use fixed frame delay.
                    actFrames = actFrames.Concat(effectActFrames).ToArray();
                    int aniLength = actFrames.Max(layer => layer == null ? 0 : layer.Sum(f => f.actionFrame.AbsoluteDelay));
                    int aniDelay = config.MinDelay;

                    // pipeline functions
                    IEnumerable<int> RenderDelay()
                    {
                        int t = 0;
                        while (t < aniLength)
                        {
                            int frameDelay = Math.Min(aniLength - t, aniDelay);
                            t += frameDelay;
                            yield return frameDelay;
                        }
                    }

                    IEnumerable<Tuple<int[], int>> GetFrameActionIndices(IEnumerable<int> delayEnumerator)
                    {
                        int[] time = new int[actFrames.Length];
                        int[] actionState = new int[actFrames.Length];
                        for (int i = 0; i < actionState.Length; i++)
                        {
                            actionState[i] = actFrames[i] != null ? (actFrames[i].Length < 1 ? -1 : 0) : -1;
                        }

                        foreach (int delay in delayEnumerator)
                        {
                            // return state
                            int[] actIndices = new int[actionState.Length];
                            for (int i = 0; i < actionState.Length; i++)
                            {
                                actIndices[i] = actionState[i] > -1 ? actFrames[i][actionState[i]].index : -1;
                            }
                            yield return Tuple.Create(actIndices, delay);

                            // update state
                            for (int i = 0; i < actionState.Length; i++)
                            {
                                if (i >= 3 ? effectPlaying : actPlaying[i])
                                {
                                    var act = actFrames[i];
                                    time[i] += delay;
                                    int frameIndex = actionState[i];
                                    if (act == null || act.Length < 1)
                                    {
                                        continue;
                                    }
                                    while (time[i] >= act[frameIndex].actionFrame.AbsoluteDelay)
                                    {
                                        time[i] -= act[frameIndex].actionFrame.AbsoluteDelay;
                                        frameIndex = (frameIndex + 1) % act.Length;
                                    }
                                    actionState[i] = frameIndex;
                                }
                            }
                        }
                    }

                    IEnumerable<Tuple<int[], int>> MergeFrames(IEnumerable<Tuple<int[], int>> frames)
                    {
                        int[] prevFrame = null;
                        int prevDelay = 0;

                        foreach (var frame in frames)
                        {
                            int[] currentFrame = frame.Item1;
                            int currentDelay = frame.Item2;

                            if (prevFrame == null)
                            {
                                prevFrame = currentFrame;
                                prevDelay = currentDelay;
                            }
                            else if (prevFrame.SequenceEqual(currentFrame))
                            {
                                prevDelay += currentDelay;
                            }
                            else
                            {
                                yield return Tuple.Create(prevFrame, prevDelay);
                                prevFrame = currentFrame;
                                prevDelay = currentDelay;
                            }
                        }

                        if (prevFrame != null)
                        {
                            yield return Tuple.Create(prevFrame, prevDelay);
                        }
                    }

                    GifFrame ApplyFrame(int[] actionIndices, int delay)
                    {
                        var bone = this.avatar.CreateFrame(actionIndices[0], actionIndices[1], actionIndices[2], actionIndices.Skip(3).ToArray());
                        var frameData = this.avatar.DrawFrame(bone);
                        return new GifFrame(frameData.Bitmap, frameData.Origin, delay);
                    }

                    // build pipeline
                    var step1 = RenderDelay();
                    var step2 = GetFrameActionIndices(step1);
                    var step3 = cap.IsFixedFrameRate ? step2 : MergeFrames(step2);
                    var step4 = step3.Select(tp => ApplyFrame(tp.Item1, tp.Item2));

                    // run pipeline
                    foreach (var gifFrame in step4)
                    {
                        gifLayer.AddFrame(gifFrame);
                    }
                }

                if (gifLayer.Frames.Count <= 0)
                {
                    MessageBoxEx.Show(this, "動畫資料計算失敗。", "錯誤");
                    return;
                }

                Rectangle clientRect = gifLayer.Frames
                    .Select(f => new Rectangle(-f.Origin.X, -f.Origin.Y, f.Bitmap.Width, f.Bitmap.Height))
                    .Aggregate((rect1, rect2) =>
                    {
                        int left = Math.Min(rect1.X, rect2.X);
                        int top = Math.Min(rect1.Y, rect2.Y);
                        int right = Math.Max(rect1.Right, rect2.Right);
                        int bottom = Math.Max(rect1.Bottom, rect2.Bottom);
                        return new Rectangle(left, top, right - left, bottom - top);
                    });

                Brush CreateBackgroundBrush()
                {
                    switch (config.BackgroundType.Value)
                    {
                        default:
                        case ImageBackgroundType.Transparent:
                            return null;
                        case ImageBackgroundType.Color:
                            return new SolidBrush(config.BackgroundColor.Value);
                        case ImageBackgroundType.Mosaic:
                            int blockSize = Math.Max(1, config.MosaicInfo.BlockSize);
                            var texture = new Bitmap(blockSize * 2, blockSize * 2);
                            using (var g = Graphics.FromImage(texture))
                            using (var brush0 = new SolidBrush(config.MosaicInfo.Color0))
                            using (var brush1 = new SolidBrush(config.MosaicInfo.Color1))
                            {
                                g.FillRectangle(brush0, 0, 0, blockSize, blockSize);
                                g.FillRectangle(brush0, blockSize, blockSize, blockSize, blockSize);
                                g.FillRectangle(brush1, 0, blockSize, blockSize, blockSize);
                                g.FillRectangle(brush1, blockSize, 0, blockSize, blockSize);
                            }
                            return new TextureBrush(texture);
                    }
                }

                using var bgBrush = CreateBackgroundBrush();
                encoder.Init(outputFileName, clientRect.Width, clientRect.Height);
                foreach (IGifFrame gifFrame in gifLayer.Frames)
                {
                    using (var bmp = new Bitmap(clientRect.Width, clientRect.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
                    {
                        using (var g = Graphics.FromImage(bmp))
                        {
                            // draw background
                            if (bgBrush != null)
                            {
                                g.FillRectangle(bgBrush, 0, 0, bmp.Width, bmp.Height);
                            }
                            gifFrame.Draw(g, clientRect);
                        }
                        encoder.AppendFrame(bmp, Math.Max(cap.MinFrameDelay, gifFrame.Delay));
                    }
                }
            }
        }

        public void LoadCode(string code, int loadType)
        {
            chairName = "";
            //解析
            var matches = Regex.Matches(code, @"s?(\d+)(\+([0-7])\*(\d{1,2}))?([,\s]|$)");
            if (matches.Count <= 0)
            {
                ToastNotification.Show(this, $"錯誤: 沒有與道具 ID 相符的道具。", null, 3000, eToastGlowColor.Red, eToastPosition.TopCenter);
                return;
            }

            if (PluginManager.FindWz(Wz_Type.Base) == null)
            {
                ToastNotification.Show(this, $"錯誤: 未開啟 Base.wz。", null, 2000, eToastGlowColor.Red, eToastPosition.TopCenter);
                return;
            }

            var characWz = PluginManager.FindWz(Wz_Type.Character);
            var skillWz = PluginManager.FindWz(Wz_Type.Skill);
            var itemWz = PluginManager.FindWz(Wz_Type.Item);

            //试图初始化
            if (!this.inited && !this.AvatarInit())
            {
                ToastNotification.Show(this, $"錯誤: 無法初始化紙娃娃模組。", null, 2000, eToastGlowColor.Red, eToastPosition.TopCenter);
                return;
            }
            var sl = this.PluginEntry.Context.DefaultStringLinker;
            if (!sl.HasValues) //生成默认stringLinker
            {
                sl.Load(PluginManager.FindWz(Wz_Type.String).GetValueEx<Wz_File>(null), PluginManager.FindWz(Wz_Type.Item).GetValueEx<Wz_File>(null), PluginManager.FindWz(Wz_Type.Etc).GetValueEx<Wz_File>(null));
            }

            if (loadType == 0) //先清空。。
            {
                Array.Clear(this.avatar.Parts, 0, this.avatar.Parts.Length);
            }

            List<int> failList = new List<int>();

            foreach (Match m in matches)
            {
                int gearID;
                if (Int32.TryParse(m.Result("$1"), out gearID))
                {
                    Wz_Node imgNode = FindNodeByGearID(characWz, gearID);
                    if (imgNode != null)
                    {
                        var part = this.avatar.AddPart(imgNode);
                        if (m.Groups.Count >= 4 && Int32.TryParse(m.Result("$3"), out int mixColor) && Int32.TryParse(m.Result("$4"), out int mixOpacity))
                        {
                            part.MixColor = mixColor;
                            part.MixOpacity = mixOpacity;
                        }
                        OnNewPartAdded(part);
                        continue;
                    }
                    if (m.ToString().StartsWith("s"))
                    {
                        imgNode = FindNodeBySkillID(skillWz, gearID);
                        if (imgNode != null)
                        {
                            int tamingMobID = imgNode.Nodes["vehicleID"].GetValueEx<int>(0);
                            if (tamingMobID == 0)
                            {
                                tamingMobID = PluginBase.PluginManager.FindWz(string.Format(@"Skill\RidingSkillInfo.img\{0:D7}\vehicleID", gearID)).GetValueEx<int>(0);
                            }
                            if (tamingMobID != 0)
                            {
                                var tamingMobNode = PluginBase.PluginManager.FindWz(string.Format(@"Character\TamingMob\{0:D8}.img", tamingMobID));
                                if (tamingMobNode != null)
                                {
                                    var part = this.avatar.AddTamingPart(tamingMobNode, BitmapOrigin.CreateFromNode(imgNode.Nodes["icon"], PluginBase.PluginManager.FindWz), gearID, true);
                                    OnNewPartAdded(part);
                                }
                            }
                            continue;
                        }
                    }
                    imgNode = FindNodeByItemID(itemWz, gearID);
                    if (imgNode != null) // should sync with OnSelectedNode2Changed()
                    {
                        bool removeTamingPart = true;
                        Wz_Vector brm = null;

                        int tamingMobID = imgNode.FindNodeByPath("info\\tamingMob").GetValueEx<int>(0);
                        if (tamingMobID == 0)
                        {
                            tamingMobID = imgNode.FindNodeByPath("info\\customChair\\self\\tamingMob").GetValueEx<int>(0);
                        }
                        if (tamingMobID != 0)
                        {
                            brm = imgNode.FindNodeByPath("info\\group\\sit\\0\\bodyRelMove").GetValueEx<Wz_Vector>(null);
                            var tamingMobNode = PluginBase.PluginManager.FindWz(string.Format(@"Character\TamingMob\{0:D8}.img", tamingMobID));
                            if (tamingMobNode != null)
                            {
                                removeTamingPart = false;

                                this.avatar.RemoveChairPart();
                                var part = this.avatar.AddTamingPart(tamingMobNode, BitmapOrigin.CreateFromNode(tamingMobNode.FindNodeByPath("info\\icon"), PluginBase.PluginManager.FindWz), tamingMobID, false, brm);
                                OnNewPartAdded(part);
                            }
                        }

                        brm = imgNode.FindNodeByPath("info\\bodyRelMove").GetValueEx<Wz_Vector>(null);
                        bool isSitActionExists = imgNode.FindNodeByPath("info\\sitAction").GetValueEx<string>(null) != null;
                        if (gearID / 10000 == 301 || gearID / 1000 == 5204 || brm != null || isSitActionExists) // 의자 아이템, 아이템 코드나 bodyRelMove과 sitAction 속성 유무로 결정
                        {
                            bool fb = false;
                            if (brm == null)
                            {
                                fb = false;
                            }
                            else if (isSitActionExists)
                            {
                                fb = true;
                            }

                            if (removeTamingPart) RemoveTamingPart();
                            var part = this.avatar.AddChairPart(imgNode, BitmapOrigin.CreateFromNode(imgNode.FindNodeByPath("info\\icon"), PluginBase.PluginManager.FindWz), gearID, brm, fb);
                            OnNewPartAdded(part);
                        }

                        if (gearID / 10000 == 501) // effect items
                        {
                            var part = this.avatar.AddEffectPart(imgNode);
                            OnNewPartAdded(part);
                        }
                        continue;
                    }
                    // else
                    {
                        failList.Add(gearID);
                    }
                }
            }

            if (this.avatar.Longcoat != null)
            {
                if (this.avatar.Pants != null)
                {
                    this.avatar.Pants.Visible = false;
                }
                if (this.avatar.Coat != null)
                {
                    this.avatar.Coat.Visible = false;
                }
                this.avatar.Longcoat.Visible = true;
            }

            //刷新
            //Use stand1 pose by request
            this.SelectBodyAction("stand1" ?? "default");
            this.FillAvatarParts();
            this.UpdateDisplay();

            //其他提示
            if (failList.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("未找到以下道具ID : ");
                foreach (var gearID in failList)
                {
                    sb.Append("  ").AppendLine(gearID.ToString("D8"));
                }
                ToastNotification.Show(this, sb.ToString(), null, 4000, eToastGlowColor.Red, eToastPosition.TopCenter);
            }

        }

        private Wz_Node FindNodeByGearID(Wz_Node characWz, int id)
        {
            string imgName = id.ToString("D8") + ".img";
            Wz_Node imgNode = null;

            foreach (var node1 in characWz.Nodes)
            {
                if (node1.Text.Contains("_Canvas"))
                {
                    continue;
                }

                if (node1.Text == imgName)
                {
                    imgNode = node1;
                    break;
                }
                else if (node1.Nodes.Count > 0)
                {
                    foreach (var node2 in node1.Nodes)
                    {
                        if (node2.Text == imgName)
                        {
                            imgNode = node2;
                            break;
                        }
                    }
                    if (imgNode != null)
                    {
                        break;
                    }
                }
            }

            if (imgNode != null)
            {
                Wz_Image img = imgNode.GetValue<Wz_Image>();
                if (img != null && img.TryExtract())
                {
                    return img.Node;
                }
            }

            return null;
        }

        private Wz_Node FindNodeBySkillID(Wz_Node skillWz, int id)
        {
            string idName = id.ToString();

            foreach (var node1 in skillWz.Nodes)
            {
                if (idName.StartsWith(node1.Text.Replace(".img", "")))
                {
                    Wz_Image img = node1.GetValue<Wz_Image>();
                    if (img != null && img.TryExtract())
                    {
                        if (img.Node.Nodes["skill"].Nodes.Count > 0)
                        {
                            foreach (var skillNode in img.Node.Nodes["skill"].Nodes)
                            {
                                if (skillNode.Text == idName)
                                {
                                    return skillNode;
                                }
                            }
                        }
                    }
                }
            }

            return null;
        }


        private Wz_Node FindNodeByItemID(Wz_Node itemWz, int id)
        {
            string idName = id.ToString("D8");
            Wz_Node imgNode = null;

            foreach (var node1 in itemWz.Nodes)
            {
                if (node1.Nodes.Count > 0)
                {
                    foreach (var node2 in node1.Nodes)
                    {
                        if (idName.StartsWith(node2.Text.Replace(".img", "")))
                        {
                            imgNode = node2;
                            break;
                        }
                    }
                    if (imgNode != null)
                    {
                        break;
                    }
                }
            }

            if (imgNode != null)
            {
                Wz_Image img = imgNode.GetValue<Wz_Image>();
                if (img != null && img.TryExtract())
                {
                    if (img.Node.Nodes.Count > 0)
                    {
                        foreach (var itemNode in img.Node.Nodes)
                        {
                            if (itemNode.Text == idName)
                            {
                                return itemNode;
                            }
                        }
                    }
                }
            }

            return null;
        }

        private string FindIDFromString(string name, int gender = 2)
        {
            if (string.IsNullOrEmpty(name))
            {
                return "";
            }

            var sl = this.PluginEntry.Context.DefaultStringLinker;
            if (!sl.HasValues) //生成默认stringLinker
            {
                sl.Load(PluginManager.FindWz(Wz_Type.String).GetValueEx<Wz_File>(null), PluginManager.FindWz(Wz_Type.Item).GetValueEx<Wz_File>(null), PluginManager.FindWz(Wz_Type.Etc).GetValueEx<Wz_File>(null));
            }

            foreach (var kv in sl.StringEqp)
            {
                if (kv.Value.Name == name)
                {
                    if (gender == 2 || ((gender + 1) & Gear.GetCosmeticGender(kv.Key)) > 0)
                    {
                        return kv.Key.ToString();
                    }
                }
            }
            return "";
        }

        private class Animator
        {
            public Animator()
            {
                this.delays = new int[3] { -1, -1, -1 };
                this.effectDelays = Enumerable.Repeat(-1, AvatarCanvas.LayerSlotLength).ToArray();
                this.suspend = false;
            }

            private int[] delays;
            private int[] effectDelays;
            private bool suspend;

            public int NextFrameDelay { get; private set; }

            public int BodyDelay
            {
                get { return this.delays[0]; }
                set
                {
                    this.delays[0] = value;
                    Update();
                }
            }

            public int EmotionDelay
            {
                get { return this.delays[1]; }
                set
                {
                    this.delays[1] = value;
                    Update();
                }
            }

            public int TamingDelay
            {
                get { return this.delays[2]; }
                set
                {
                    this.delays[2] = value;
                    Update();
                }
            }

            public int[] EffectDelay
            {
                get { return this.effectDelays; }
                set
                {
                    this.effectDelays = value;
                    Update();
                }
            }

            public void Elapse(int millisecond)
            {
                for (int i = 0; i < delays.Length; i++)
                {
                    if (delays[i] >= 0)
                    {
                        delays[i] = delays[i] > millisecond ? (delays[i] - millisecond) : 0;
                    }
                }
                for (int i = 0; i < effectDelays.Length; i++)
                {
                    if (effectDelays[i] >= 0)
                    {
                        effectDelays[i] = effectDelays[i] > millisecond ? (effectDelays[i] - millisecond) : 0;
                    }
                }
            }

            private void Update()
            {
                if (this.suspend) return;

                int nextFrame = 0;
                foreach (int delay in this.delays)
                {
                    if (delay > 0)
                    {
                        nextFrame = nextFrame <= 0 ? delay : Math.Min(nextFrame, delay);
                    }
                }
                foreach (int delay in this.effectDelays)
                {
                    if (delay > 0)
                    {
                        nextFrame = nextFrame <= 0 ? delay : Math.Min(nextFrame, delay);
                    }
                }
                this.NextFrameDelay = nextFrame;
            }

            public void SuspendUpdate()
            {
                this.suspend = true;
            }

            public void TrigUpdate()
            {
                this.suspend = false;
                Update();
            }
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            ExportAvatar(sender, e);
        }

        private void btnEnableAutosave_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(specifiedSavePath)) btnSpecifySavePath_Click(sender, e);
            if (!String.IsNullOrEmpty(specifiedSavePath))
            {
                btnSpecifySavePath.Enabled = btnEnableAutosave.Checked;
            }
            else
            {
                btnEnableAutosave.Checked = false;
            }
        }

        private void btnSpecifySavePath_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dlg = new FolderBrowserDialog())
            {
                dlg.Description = "選擇您想要自動儲存紙娃娃的位置。";
                if (DialogResult.OK == dlg.ShowDialog())
                {
                    specifiedSavePath = dlg.SelectedPath;
                }
            }
        }

        private void ExportAvatar(object sender, EventArgs e)
        {
            ComboItem selectedItem;
            //同步角色动作
            selectedItem = this.cmbActionBody.SelectedItem as ComboItem;
            this.avatar.ActionName = selectedItem != null ? selectedItem.Text : null;
            //同步表情
            selectedItem = this.cmbEmotion.SelectedItem as ComboItem;
            this.avatar.EmotionName = selectedItem != null ? selectedItem.Text : null;
            //同步骑宠动作
            this.avatar.TamingActionName = null;

            //获取动作帧
            this.GetSelectedBodyFrame(out int bodyFrame, out _);
            this.GetSelectedEmotionFrame(out int emoFrame, out _);
            this.GetSelectedTamingFrame(out int tamingFrame, out _);

            //获取武器状态
            selectedItem = this.cmbWeaponType.SelectedItem as ComboItem;
            this.avatar.WeaponType = selectedItem != null ? Convert.ToInt32(selectedItem.Text) : 0;

            selectedItem = this.cmbWeaponIdx.SelectedItem as ComboItem;
            this.avatar.WeaponIndex = selectedItem != null ? Convert.ToInt32(selectedItem.Text) : 0;

            if (this.avatar.ActionName == null)
            {
                MessageBoxEx.Show("未找到紙娃娃。");
                return;
            }

            var config = ImageHandlerConfig.Default;
            using var encoder = AnimateEncoderFactory.CreateEncoder(config);
            var cap = encoder.Compatibility;

            string extensionFilter = string.Join(";", cap.SupportedExtensions.Select(ext => $"*{ext}"));

            FolderBrowserDialog dlg = new FolderBrowserDialog();
            dlg.Description = "選擇要匯出的資料夾。";

            async Task ExportGif(string actionName)
            {
                var actionFrames = avatar.GetActionFrames(actionName);
                var faceFrames = avatar.GetFaceFrames(avatar.EmotionName);
                var tamingFrames = avatar.GetTamingFrames(avatar.TamingActionName);
                var effectActions = new ActionFrame[AvatarCanvas.LayerSlotLength];
                if (emoFrame <= -1 || emoFrame >= faceFrames.Length)
                {
                    return;
                }

                Gif gif = new Gif();

                foreach (var frame in string.IsNullOrEmpty(avatar.TamingActionName) ? actionFrames : tamingFrames)
                {
                    if (frame.Delay != 0)
                    {
                        var bone = string.IsNullOrEmpty(avatar.TamingActionName) ? avatar.CreateFrame(frame, faceFrames[emoFrame], null, null) : avatar.CreateFrame(actionFrames[0], faceFrames[emoFrame], frame, null);
                        var bmp = avatar.DrawFrame(bone);

                        Point pos = bmp.OpOrigin;
                        pos.Offset(frame.Flip ? new Point(-frame.Move.X, frame.Move.Y) : frame.Move);
                        GifFrame f = new GifFrame(bmp.Bitmap, new Point(-pos.X, -pos.Y), Math.Abs(frame.Delay));
                        gif.Frames.Add(f);
                    }
                }

                string fileName = System.IO.Path.Combine(dlg.SelectedPath, actionName.Replace('\\', '.') + cap.DefaultExtension);

                var tasks = new List<Task>();

                tasks.Add(Task.Run(() =>
                {
                    GifEncoder enc = AnimateEncoderFactory.CreateEncoder(config);
                    gif.SaveGif(enc, fileName, Color.Transparent);
                }));

                await Task.WhenAll(tasks);
            }

            async Task ExportJob(IProgressDialogContext context, CancellationToken cancellationToken)
            {
                IEnumerable<AvatarCommon.Action> actionEnumerator = avatar.Actions;
                var step1 = actionEnumerator.TakeWhile(_ => !cancellationToken.IsCancellationRequested);

                var step2 = step1.Select(item => ExportGif(item.Name));

                // run pipeline
                try
                {
                    this.Enabled = false;
                    context.ProgressMin = 0;
                    context.ProgressMax = avatar.Actions.Count;
                    foreach (var task in step2)
                    {
                        await task;
                        context.Progress++;
                    }
                }
                catch (Exception ex)
                {
                    context.Message = $"錯誤: {ex.Message}";
                    throw;
                }
                finally
                {
                    this.Enabled = true;
                }
            }

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                ProgressDialog.Show(this.FindForm(), "正在匯出...", "正在匯出" + avatar.Actions.Count + "個動作...", true, false, ExportJob);
            }
        }
        private static string RemoveInvalidFileNameChars(string fileName)
        {
            if (String.IsNullOrEmpty(fileName)) return "未知";
            string invalidChars = new string(System.IO.Path.GetInvalidFileNameChars());
            string regexPattern = $"[{Regex.Escape(invalidChars)}]";
            return Regex.Replace(fileName, regexPattern, "_");
        }
    }
}
