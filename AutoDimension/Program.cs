using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Threading;
using AutoDimension.UI;

namespace AutoDimension
{
    static class Program
    {
        //public 
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            FrmLoad frmLoad = new FrmLoad();
            if (frmLoad.ShowDialog() == DialogResult.Yes)
            {
                UI.FrmConfigure frmcf = new UI.FrmConfigure();
                if (frmcf.ShowDialog() == DialogResult.Yes)
                {
                    Application.Run(new NewMainForm());
                }
                else
                {
                    try
                    {
                        CServers.GetServers().CloseService();
                    }
                    catch { }
                }
            }
           
           
          /*
            int re = CDogTools.GetInstance().GetUserAuthority();
            if (re >0)
            {
                string a = CDogTools.GetInstance().GetStringData(101);
                long b = CDogTools.GetInstance().GetLongData(101);

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new MainForm());
            }
            else if( re==-1)
            {
                MessageBox.Show("您的加密狗已经在其他程序中使用，不能重复登录！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                MessageBox.Show("未检测到加密狗！请确认是否已经插好！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }*/
        }
        static private void StareServer()
        {
            CServers.GetServers();
        }
    }
}
