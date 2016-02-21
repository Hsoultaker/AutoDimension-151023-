using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using AutoDimension.Entity;

namespace AutoDimension.UI
{
    public partial class DimSettingForm : Form
    {
        public DimSettingForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 加载;
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DimSettingForm_Load(object sender, EventArgs e)
        {  
            CDimManager.GetInstance().BuildDrawingMarkNumberDic();

            Dictionary<string, string> mDicMarkNumberToType = CDimManager.GetInstance().mDicMarkNumberToType;
            Dictionary<string, string>.Enumerator enumerator = mDicMarkNumberToType.GetEnumerator();

            while (enumerator.MoveNext())
            {
                KeyValuePair<string, string> keyPair  = enumerator.Current;

                string strKey = keyPair.Key;
                string strValue = keyPair.Value;

                dataGridView_DrawingMark.Rows.Add(new object[] {strKey,strValue});
            }
        }

        /// <summary>
        /// 确认;
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_OK_Click(object sender, EventArgs e)
        {
            Dictionary<string, string> mDicMarkNumberToType = CDimManager.GetInstance().mDicMarkNumberToType;

            int nCount = dataGridView_DrawingMark.Rows.Count;

            for (int i = 0; i < nCount; i++)
            {
                DataGridViewRow row = dataGridView_DrawingMark.Rows[i];

                string strKey = row.Cells[0].Value.ToString();
                string strValue = row.Cells[1].Value.ToString();

                if (mDicMarkNumberToType.ContainsKey(strKey))
                {
                    mDicMarkNumberToType[strKey] = strValue;
                }
            }
            this.Close();

            CDimManager.GetInstance().InitMrAssemblyDrawingList();
        }
    }
}
