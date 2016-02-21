using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using UserManagement;

namespace UserManagement
{
    public partial class MainForm : Form
    {
        bool IsAdd = true;      
               
        public MainForm()
        {
            InitializeComponent();
           
        }
        
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                ulong Serial = CDogTools.GetInstance().GetSerial();
                if (Serial == 0)
                {
                    MessageBox.Show("请确认是否已插入加密狗？", "提示", MessageBoxButtons.OK, MessageBoxIcon.Question);
                }
                else if (HasExist(Serial))
                {
                    //MessageBox.Show("该加密狗已经初始化过，可修改用户信息！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else if (CDogTools.GetInstance().initLock())
                {
                    CDogTools.GetInstance().WriteLock(101, 32291);
                    CDogTools.GetInstance().WriteLock(102, 5389);
                    CDogTools.GetInstance().WriteLock(103, 23740);
                    CDogTools.GetInstance().WriteLock(104, 7575);

                    //101 6 PROFILE.FLANGE_THICKNESS111 6  PROFILE.WEB_THICKNESS
                    CDogTools.GetInstance().WriteLock(201, "PROF");
                    CDogTools.GetInstance().WriteLock(202, "ILE.");
                    CDogTools.GetInstance().WriteLock(203, "FLAN");
                    CDogTools.GetInstance().WriteLock(204, "GE_T");
                    CDogTools.GetInstance().WriteLock(205, "HICK");
                    CDogTools.GetInstance().WriteLock(206, "NESS");

                    CDogTools.GetInstance().WriteLock(211, "PROF");
                    CDogTools.GetInstance().WriteLock(212, "ILE.");
                    CDogTools.GetInstance().WriteLock(213, "WEB_");
                    CDogTools.GetInstance().WriteLock(214, "THIC");
                    CDogTools.GetInstance().WriteLock(215, "KNES");
                    CDogTools.GetInstance().WriteLock(216, "S");


                    if (CDogTools.GetInstance().GetLongData(104) == 7575)
                    {

                        if (CDogTools.GetInstance().Validate())
                        {

                            MessageBox.Show("加密狗初始化完成！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show("加密狗初始化失败！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                }
                else
                {
                    MessageBox.Show("加密狗初始化失败！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch
            {
                MessageBox.Show("加密狗初始化失败！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private bool HasExist(ulong Serial)
        {
            groupBox1.Enabled = true;
            TXT_SerialNo.Text = Serial.ToString();
            button2.Text = "添加用户";
            TXT_Address.Text = "";
            TXT_Company.Text ="";
            TXT_Email.Text = "";
            TXT_QQ.Text = "";
            TXT_Telephone.Text = "";
            TXT_UserName.Text = "";
            TXYRemark.Text = "";
           
            DataClassesDataContext dc = new DataClassesDataContext();
       
            var userInfolst = from g in dc.UserInfo
                                     where g.SerialNo_ == (int)Serial
                                     select g;
            foreach (UserInfo user in userInfolst)
            {
                TXT_Address.Text = user.Address;
                TXT_Company.Text = user.Company;
                TXT_Email.Text = user.Email;
                TXT_QQ.Text = user.QQ;
                TXT_Telephone.Text = user.Telephone;
                TXT_UserName.Text = user.UserName;
                TXYRemark.Text = user.Remark;
                button2.Text = "修改信息";
                IsAdd = false;             
                SetPurview(user.Purview);
                return true;
            }
            //dc.SubmitChanges();
            return false;
        }



        private void button2_Click_1(object sender, EventArgs e)
        {
            try
            {
                DataClassesDataContext dc = new DataClassesDataContext();

                if (IsAdd)
                {
                    UserInfo userInfo = new UserInfo();
                    userInfo.SerialNo_ = int.Parse(TXT_SerialNo.Text);
                    userInfo.Address = TXT_Address.Text;
                    userInfo.Company = TXT_Company.Text;
                    userInfo.Email = TXT_Email.Text;
                    userInfo.QQ = TXT_QQ.Text;
                    userInfo.Telephone = TXT_Telephone.Text;
                    userInfo.UserName = TXT_UserName.Text;
                    userInfo.Remark = TXYRemark.Text;
                    userInfo.Purview = GetPurview();
                    dc.UserInfo.InsertOnSubmit(userInfo);
                    dc.SubmitChanges();
                    MessageBox.Show("用户信息添加完成！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    var userInfolst = from g in dc.UserInfo
                                      where g.SerialNo_ == int.Parse(TXT_SerialNo.Text)
                                      select g;
                    foreach (UserInfo userInfo in userInfolst)
                    {
                        userInfo.SerialNo_ = int.Parse(TXT_SerialNo.Text);
                        userInfo.Address = TXT_Address.Text;
                        userInfo.Company = TXT_Company.Text;
                        userInfo.Email = TXT_Email.Text;
                        userInfo.QQ = TXT_QQ.Text;
                        userInfo.Telephone = TXT_Telephone.Text;
                        userInfo.UserName = TXT_UserName.Text;
                        userInfo.Remark = TXYRemark.Text;
                        userInfo.Purview = GetPurview();
                        dc.SubmitChanges();
                        MessageBox.Show("用户信息修改完成！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;

                    }

                }
            }
            catch {
                if(IsAdd)
                     MessageBox.Show("用户信息添加失败！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                else
                    MessageBox.Show("用户信息修改失败！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            
            
        }
        private void SetPurview(int Purview)
        {
            for (int i = 0; i < CLB_Purview.Items.Count; i++)
            {
                int K = (int)Math.Pow(2, i + 1);
                if ((Purview & K) == K)                   
                    CLB_Purview.SetItemChecked(i, true);               
            }
        }
        private int GetPurview()
        {
            int Purview = 0;
            for (int i = 0; i < CLB_Purview.Items.Count; i++)
            {
                if (CLB_Purview.GetItemChecked(i))
                {
                    Purview += (int)Math.Pow(2, i+1);
                }
            }

            return Purview;
        }
    }
}
