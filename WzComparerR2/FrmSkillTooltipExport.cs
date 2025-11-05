using DevComponents.DotNetBar;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using WzComparerR2.WzLib;

namespace WzComparerR2
{
    public partial class FrmSkillTooltipExport : DevComponents.DotNetBar.Office2007Form
    {
        public FrmSkillTooltipExport()
        {
            InitializeComponent();
#if NET6_0_OR_GREATER
            // https://learn.microsoft.com/en-us/dotnet/core/compatibility/fx-core#controldefaultfont-changed-to-segoe-ui-9pt
            this.Font = new Font(new FontFamily("SimSun"), 9f);
#endif
            this.clbJobName.Items.Add("其它", false);
            foreach (var i in jobNameToCode.Keys)
            {
                this.clbJobName.Items.Add(i, false);
            }
        }

        public string ExportFolderPath { get; private set; }
        public List<int> SelectedJobCodes { get; private set; }
        public Wz_Node skillNode { get; set; }

        private static Dictionary<string, int[]> jobNameToCode = new Dictionary<string, int[]>()
        {
            { "英雄", new int[] { 100, 110, 111, 112, 114 } },
            { "聖騎士", new int[] { 100, 120, 121, 122, 124 } },
            { "黑騎士", new int[] { 100, 130, 131, 132, 134 } },
            { "大魔導士(火・毒)", new int[] { 200, 210, 211, 212, 214 } },
            { "大魔導士(冰・雷)", new int[] { 200, 220, 221, 222, 224 } },
            { "主教", new int[] { 200, 230, 231, 232, 234 } },
            { "箭神", new int[] { 300, 310, 311, 312, 314 } },
            { "神射手", new int[] { 300, 320, 321, 322, 324 } },
            { "開拓者", new int[] { 301, 330, 331, 332, 334 } },
            { "夜使者", new int[] { 400, 410, 411, 412, 414 } },
            { "暗影神偷", new int[] { 400, 420, 421, 422, 424 } },
            { "影武者", new int[] { 400, 430, 431, 432, 433, 434, 436 } },
            { "拳霸", new int[] { 500, 510, 511, 512, 514 } },
            { "槍神", new int[] { 500, 520, 521, 522, 524 } },
            { "重砲指揮官", new int[] { 501, 530, 531, 532, 534 } },
            { "蒼龍俠客", new int[] { 508, 570, 571, 572, 574 } },
            { "聖魂劍士", new int[] { 1000, 1100, 1110, 1111, 1112, 1114 } },
            { "烈焰巫師", new int[] { 1000, 1200, 1210, 1211, 1212, 1214 } },
            { "破風使者", new int[] { 1000, 1300, 1310, 1311, 1312, 1314 } },
            { "暗夜行者", new int[] { 1000, 1400, 1410, 1411, 1412, 1414 } },
            { "閃雷悍將", new int[] { 1000, 1500, 1510, 1511, 1512, 1514 } },
            { "狂狼勇士", new int[] { 2000, 2100, 2110, 2111, 2112, 2114 } },
            { "龍魔導士", new int[] { 2001, 2200, 2210, 2211, 2212, 2213, 2214, 2215, 2216, 2217, 2218, 2219, 2220 } },
            { "精靈遊俠", new int[] { 2002, 2300, 2310, 2311, 2312, 2314 } },
            { "幻影俠盜", new int[] { 2003, 2400, 2410, 2411, 2412, 2414 } },
            { "夜光", new int[] { 2004, 2700, 2710, 2711, 2712, 2714 } },
            { "隠月", new int[] { 2005, 2500, 2510, 2511, 2512, 2514 } },
            { "惡魔殺手", new int[] { 3001, 3100, 3110, 3111, 3112, 3114 } },
            { "惡魔復仇者", new int[] { 3001, 3101, 3120, 3121, 3122, 3124 } },
            { "爆拳槍神", new int[] { 3000, 3700, 3710, 3711, 3712, 3714 } },
            { "煉獄巫師", new int[] { 3000, 3200, 3210, 3211, 3212, 3214 } },
            { "狂豹獵人", new int[] { 3000, 3300, 3310, 3311, 3312, 3314 } },
            { "機甲戰神", new int[] { 3000, 3500, 3510, 3511, 3512, 3514 } },
            { "傑諾", new int[] { 3002, 3600, 3610, 3611, 3612, 3614 } },
            { "劍豪", new int[] { 4001, 4100, 4110, 4111, 4112, 4114 } },
            { "陰陽師", new int[] { 4002, 4200, 4210, 4211, 4212, 4214 } },
            { "米哈逸", new int[] { 5000, 5100, 5110, 5111, 5112, 5114 } },
            { "凱薩", new int[] { 6000, 6100, 6110, 6111, 6112, 6114 } },
            { "凱殷", new int[] { 6003, 6300, 6310, 6311, 6312, 6314 } },
            { "卡蒂娜", new int[] { 6002, 6400, 6410, 6411, 6412, 6414 } },
            { "天使破壞者", new int[] { 6001, 6500, 6510, 6511, 6512, 6514 } },
            // { "能力", new int[] { 7000 } },
            // { "戰地技能", new int[] { 7100 } },
            // { "怪物農場", new int[] { 7200 } },
            // { "公會", new int[] { 9100 } },
            // { "専業技術", new int[] { 9200, 9201, 9202, 9203, 9204 } },
            { "神之子", new int[] { 10000, 10100, 10110, 10111, 10112, 10114 } },
            { "幻獸師", new int[] { 11000, 11200, 11210, 11211, 11212 } },
            { "竈門炭治郎", new int[] { 12000, 12005, 12100 } },
            { "粉豆", new int[] { 13000, 13100 } },
            { "雪吉拉", new int[] { 13001, 13500 } },
            { "凱內西斯", new int[] { 14000, 14200, 14210, 14211, 14212, 14214 } },
            { "阿戴爾", new int[] { 15002, 15100, 15110, 15111, 15112, 15114 } },
            { "伊利恩", new int[] { 15000, 15200, 15210, 15211, 15212, 15214 } },
            { "卡莉", new int[] { 15003, 15400, 15410, 15411, 15412, 15414 } },
            { "亞克", new int[] { 15001, 15500, 15510, 15511, 15512, 15514 } },
            { "蓮", new int[] { 16002, 16100, 16110, 16111, 16112, 16114 } },
            { "菈菈", new int[] { 16001, 16200, 16210, 16211, 16212, 16214 } },
            { "虎影", new int[] { 16000, 16400, 16410, 16411, 16412, 16414 } },
            { "墨玄", new int[] { 17000, 17500, 17510, 17511, 17512, 17514 } },
            { "琳恩", new int[] { 17001, 17200, 17210, 17211, 17212, 17214 } },
            { "葉里萊特", new int[] { 18001, 18100, 18110, 18111, 18112, 18114 } },
            { "施亞阿斯特", new int[] { 18000, 18200, 18210, 18211, 18212, 18214 } },
            { "艾伊爾", new int[] { 18002, 18300, 18310, 18311, 18312, 18314 } },
            { "5轉(其它)", new int[] { 40000, 40001, 40002, 40003, 40004, 40005 } },
            { "6轉(其它)", new int[] { 50000, 50006, 50007 } },
        };

        private static HashSet<int> AllClassesCode()
        {
            HashSet<int> hsClassCode = new HashSet<int>() { };
            foreach (var i in jobNameToCode.Values)
            {
                foreach (var j in i)
                {
                    hsClassCode.Add(j);
                }
            }
            return hsClassCode;
        }

        private void btnSelectAll_Click(object sender, EventArgs e)
        {
            int checkedCount = this.clbJobName.CheckedItems.Contains("其它") ? this.clbJobName.CheckedItems.Count - 1 : this.clbJobName.CheckedItems.Count;
            bool checkedStatus = (checkedCount < this.clbJobName.Items.Count - 1);
            for (int i = 1; i < this.clbJobName.Items.Count; i++)
            {
                this.clbJobName.SetItemChecked(i, checkedStatus);
            }
        }

        private void btnReverseSelect_Click(object sender, EventArgs e)
        {
            for (int i = 1; i < this.clbJobName.Items.Count; i++)
            {
                this.clbJobName.SetItemChecked(i, !this.clbJobName.GetItemChecked(i));
            }
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            if (this.clbJobName.CheckedItems.Count == 0)
            {
                MessageBoxEx.Show("請至少選定1個職業匯出。", "未選定職業", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            FolderBrowserDialog dlg = new FolderBrowserDialog();
            dlg.Description = "請選擇目標資料夾。";

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                bool allSelected = this.clbJobName.CheckedItems.Count == this.clbJobName.Items.Count;

                List<int> skillImg = new List<int>() { };
                foreach (Wz_Node node in skillNode.Nodes)
                {
                    Wz_Image currentImg = node.GetValue<Wz_Image>();
                    if (currentImg != null && Int32.TryParse(currentImg.Name.Replace(".img", ""), out int jobCode))
                    {
                        skillImg.Add(jobCode);
                    }
                }
                List<int> selectedJob = skillImg.Intersect(this.clbJobName.CheckedItems.Cast<string>().SelectMany(name => jobNameToCode.ContainsKey(name) ? jobNameToCode[name] : new int[] { }).ToList()).ToList();
                if (this.clbJobName.CheckedItems.Contains("其它"))
                {
                    selectedJob.AddRange(skillImg.Except(AllClassesCode()));
                }
                ExportFolderPath = dlg.SelectedPath;
                SelectedJobCodes = allSelected ? skillImg : selectedJob;
                this.DialogResult = DialogResult.OK;
            }
        }
    }
}
