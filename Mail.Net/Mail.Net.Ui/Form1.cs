using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Mail.Net.Ui
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            Mail.Net.Mailing m = new Mailing(txtUsr.Text, txtPwd.Text, txtSmtpHost.Text, Convert.ToInt32(txtSmtpPort.Text), txtFromAddress.Text, chkSsl.Checked);
            m.SendMail(txtRecipients.Text, txtBCC.Text, txtSubject.Text, txtBody.Text,
                () =>
                {
                    MessageBox.Show("Send complete!");
                },
                (ex) =>
                {
                    MessageBox.Show(ex.ToString());
                });
        }
    }
}
