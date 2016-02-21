namespace UserManagement
{
    partial class MainForm
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.TXT_SerialNo = new System.Windows.Forms.TextBox();
            this.TXT_UserName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.TXT_Telephone = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.TXT_QQ = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.TXT_Email = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.TXT_Company = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.TXT_Address = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label9 = new System.Windows.Forms.Label();
            this.CLB_Purview = new System.Windows.Forms.CheckedListBox();
            this.TXYRemark = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(406, 17);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(87, 30);
            this.button1.TabIndex = 0;
            this.button1.Text = "初始化";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(406, 65);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(87, 30);
            this.button2.TabIndex = 20;
            this.button2.Text = "添加用户";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click_1);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(23, 23);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 12);
            this.label1.TabIndex = 21;
            this.label1.Text = "序列号：";
            // 
            // TXT_SerialNo
            // 
            this.TXT_SerialNo.Location = new System.Drawing.Point(85, 20);
            this.TXT_SerialNo.Name = "TXT_SerialNo";
            this.TXT_SerialNo.Size = new System.Drawing.Size(100, 21);
            this.TXT_SerialNo.TabIndex = 22;
            // 
            // TXT_UserName
            // 
            this.TXT_UserName.Location = new System.Drawing.Point(260, 20);
            this.TXT_UserName.Name = "TXT_UserName";
            this.TXT_UserName.Size = new System.Drawing.Size(109, 21);
            this.TXT_UserName.TabIndex = 24;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(190, 23);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(65, 12);
            this.label2.TabIndex = 23;
            this.label2.Text = "用户名称：";
            // 
            // TXT_Telephone
            // 
            this.TXT_Telephone.Location = new System.Drawing.Point(85, 59);
            this.TXT_Telephone.Name = "TXT_Telephone";
            this.TXT_Telephone.Size = new System.Drawing.Size(100, 21);
            this.TXT_Telephone.TabIndex = 26;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(11, 62);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(65, 12);
            this.label3.TabIndex = 25;
            this.label3.Text = "联系电话：";
            // 
            // TXT_QQ
            // 
            this.TXT_QQ.Location = new System.Drawing.Point(260, 59);
            this.TXT_QQ.Name = "TXT_QQ";
            this.TXT_QQ.Size = new System.Drawing.Size(109, 21);
            this.TXT_QQ.TabIndex = 28;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(226, 62);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(29, 12);
            this.label4.TabIndex = 27;
            this.label4.Text = "QQ：";
            // 
            // TXT_Email
            // 
            this.TXT_Email.Location = new System.Drawing.Point(85, 91);
            this.TXT_Email.Name = "TXT_Email";
            this.TXT_Email.Size = new System.Drawing.Size(284, 21);
            this.TXT_Email.TabIndex = 30;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(29, 94);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(47, 12);
            this.label5.TabIndex = 29;
            this.label5.Text = "Email：";
            // 
            // TXT_Company
            // 
            this.TXT_Company.Location = new System.Drawing.Point(85, 131);
            this.TXT_Company.Name = "TXT_Company";
            this.TXT_Company.Size = new System.Drawing.Size(284, 21);
            this.TXT_Company.TabIndex = 32;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(11, 134);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(65, 12);
            this.label6.TabIndex = 31;
            this.label6.Text = "公司名称：";
            // 
            // TXT_Address
            // 
            this.TXT_Address.Location = new System.Drawing.Point(85, 171);
            this.TXT_Address.Name = "TXT_Address";
            this.TXT_Address.Size = new System.Drawing.Size(284, 21);
            this.TXT_Address.TabIndex = 34;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(35, 174);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(41, 12);
            this.label7.TabIndex = 33;
            this.label7.Text = "地址：";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label9);
            this.groupBox1.Controls.Add(this.CLB_Purview);
            this.groupBox1.Controls.Add(this.TXYRemark);
            this.groupBox1.Controls.Add(this.label8);
            this.groupBox1.Controls.Add(this.TXT_SerialNo);
            this.groupBox1.Controls.Add(this.TXT_Address);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.TXT_Company);
            this.groupBox1.Controls.Add(this.TXT_UserName);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.TXT_Email);
            this.groupBox1.Controls.Add(this.TXT_Telephone);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.TXT_QQ);
            this.groupBox1.Enabled = false;
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(388, 471);
            this.groupBox1.TabIndex = 35;
            this.groupBox1.TabStop = false;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(23, 247);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(41, 12);
            this.label9.TabIndex = 39;
            this.label9.Text = "权限：";
            // 
            // CLB_Purview
            // 
            this.CLB_Purview.FormattingEnabled = true;
            this.CLB_Purview.Items.AddRange(new object[] {
            "顶视图",
            "前视图",
            "剖视图",
            "获取列表",
            "标注设置",
            "列表标注"});
            this.CLB_Purview.Location = new System.Drawing.Point(85, 247);
            this.CLB_Purview.Name = "CLB_Purview";
            this.CLB_Purview.Size = new System.Drawing.Size(284, 212);
            this.CLB_Purview.TabIndex = 38;
            // 
            // TXYRemark
            // 
            this.TXYRemark.Location = new System.Drawing.Point(85, 204);
            this.TXYRemark.Name = "TXYRemark";
            this.TXYRemark.Size = new System.Drawing.Size(284, 21);
            this.TXYRemark.TabIndex = 37;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(35, 207);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(41, 12);
            this.label8.TabIndex = 36;
            this.label8.Text = "备注：";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(501, 495);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "用户信息维护";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox TXT_SerialNo;
        private System.Windows.Forms.TextBox TXT_UserName;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox TXT_Telephone;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox TXT_QQ;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox TXT_Email;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox TXT_Company;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox TXT_Address;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox TXYRemark;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.CheckedListBox CLB_Purview;
    }
}

