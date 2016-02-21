using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using AutoDimension.Entity;

namespace AutoDimension
{
    public partial class FrmLoad : Form
    {
        public FrmLoad()
        {
            InitializeComponent();

#if DEBUG
            CCommonPara.mBEnableLock = false;
#else
            CCommonPara.mBEnableLock = true;
#endif
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if(!CCommonPara.mBEnableLock)
            {
                this.DialogResult = DialogResult.Yes;
                this.Close();
                return;
            }
            try
            {
                timer1.Enabled = false;
                int re = CDogTools.GetInstance().GetUserAuthority();
                if (re == -1)
                {
                    MessageBox.Show("您的加密狗已经在其他程序中使用，不能重复登录！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    CServers.GetServers().CloseService();
                    this.DialogResult = DialogResult.Cancel;
                    this.Close();
                    return;
                  
                }
                else if (re <= 0)
                {
                    MessageBox.Show("未检测到加密狗！请确认是否已经插好！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    CServers.GetServers().CloseService();
                    this.DialogResult = DialogResult.Cancel;
                    this.Close();
                    return;
                }
            }
            catch
            {
                MessageBox.Show("服务器异常，请确认网络或者加密狗", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.DialogResult = DialogResult.Cancel;
                this.Close();
                return;
            }

            this.DialogResult = DialogResult.Yes;
            this.Close();
        }
    }
}
