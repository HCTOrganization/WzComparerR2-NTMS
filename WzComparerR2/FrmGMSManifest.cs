using DevComponents.Editors;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using WzComparerR2.CharaSim;

namespace WzComparerR2
{
    public partial class FrmGMSManifest : DevComponents.DotNetBar.Office2007Form
    {
        public FrmGMSManifest()
        {
            InitializeComponent();
#if NET6_0_OR_GREATER
            // https://learn.microsoft.com/en-us/dotnet/core/compatibility/fx-core#controldefaultfont-changed-to-segoe-ui-9pt
            this.Font = new Font(new FontFamily("SimSun"), 9f);
#endif
        }

        public string ManifestUrl
        {
            get { return textBoxX1.Text; }
            set { textBoxX1.Text = value; }
        }
    }
}