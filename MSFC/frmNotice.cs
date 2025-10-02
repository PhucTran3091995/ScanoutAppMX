using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MSFC
{
    public partial class frmNotice : Form
    {
        public string noticeMsg { get; set; }
        public frmNotice(string noticeMsg)
        {
            // test git
            InitializeComponent();
            this.MaximizeBox = false;
            this.noticeMsg = noticeMsg;
            lblEBR.Text = $"{noticeMsg}";
        }

        private void btnConfirm_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
