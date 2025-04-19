using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WzComparerR2
{
    public partial class FrmWaiting : Form
    {
        public FrmWaiting()
        {
            InitializeComponent();
            this.FormClosing += new FormClosingEventHandler(FrmWaiting_Closing);
        }

        public void UpdateMessage(string message)
        {
            LabelWaiting.Text = message;
        }

        private void FrmWaiting_Closing(Object sender, CancelEventArgs e)
        {
            this.Hide();
            e.Cancel = true;
        }
    }
}
