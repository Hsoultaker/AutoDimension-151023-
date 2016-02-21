using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace AutoDimension.UI
{
    public partial class FrmConfigure : Form
    {
        public FrmConfigure()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            MessageBox.Show("当前已经是最新版本，无需升级！", "升级提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void FrmConfigure_Load(object sender, EventArgs e)
        {
            comboBox1.SelectedIndex = 0;
            comboBox2.SelectedIndex = 0;
        }
    }
}
