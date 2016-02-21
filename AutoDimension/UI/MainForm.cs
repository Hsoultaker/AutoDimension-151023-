using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using Tekla.Structures.Model;

using AutoDimension.UI;
using System.IO;
using AutoDimension.Entity;
using Tekla.Structures.Drawing;
using System.Threading;

namespace AutoDimension
{
    public partial class MainForm : Form
    {
        /// <summary>
        /// 标志是否是梁标注;
        /// </summary>
        private bool mbBeamDim = true;

        /// <summary>
        /// 标志是否是柱标注;
        /// </summary>
        private bool mbCylindeDim = false;

        /// <summary>
        /// 标志是否是门式结构的梁标注;
        /// </summary>
        private bool mbBeamDoorDim = true;

        /// <summary>
        /// 标志是否是门式结构的柱标注;
        /// </summary>
        private bool mbCylinderDoorDim = false;

        /// <summary>
        /// 是否是框架结构的标注;
        /// </summary>
        private bool mbFrameDim = true;

        /// <summary>
        /// 是否是门式框架结构的标注;
        /// </summary>
        private bool mbFrameDoorDim = false;

        /// <summary>
        /// 主框架构造函数;
        /// </summary>
        public MainForm()
        {
            InitializeComponent();

#if DEBUG
            button_Test.Visible = true;
            button_AutoSection.Visible = true;
#else
            button_Test.Visible = false;
            button_AutoSection.Visible = false;
#endif
            checkBoxBeam.Checked = true;
            checkBoxTopShow.Checked = true;
            checkBoxTopShow.BringToFront();
            InitListViewDrawing();
            InitListViewDrawingDoor();
        }

        /// <summary>
        /// 初始化框架结构的图纸列表;
        /// </summary>
        private void InitListViewDrawing()
        {
            ColumnHeader cMark = new ColumnHeader();
            cMark.Text = "标记";
            cMark.Width = 100;

            ColumnHeader cName = new ColumnHeader();
            cName.Text = "名称";
            cName.Width = 100;

            ColumnHeader cTitle1 = new ColumnHeader();
            cTitle1.Text = "标题1";
            cTitle1.Width = 100;

            ColumnHeader cTitle2 = new ColumnHeader();
            cTitle2.Text = "标题2";
            cTitle2.Width = 100;

            ColumnHeader cTitle3 = new ColumnHeader();
            cTitle3.Text = "标题3";
            cTitle3.Width = 100;

            listView_Drawing.Columns.AddRange(new ColumnHeader[] { cMark, cName,cTitle1, cTitle2, cTitle3 });
            listView_Drawing.View = System.Windows.Forms.View.Details;
        }

        /// <summary>
        /// 初始化门式框架结构的图纸列表;
        /// </summary>
        private void InitListViewDrawingDoor()
        {
            ColumnHeader cMark = new ColumnHeader();
            cMark.Text = "标记";
            cMark.Width = 100;

            ColumnHeader cName = new ColumnHeader();
            cName.Text = "名称";
            cName.Width = 100;

            ColumnHeader cTitle1 = new ColumnHeader();
            cTitle1.Text = "标题1";
            cTitle1.Width = 100;

            ColumnHeader cTitle2 = new ColumnHeader();
            cTitle2.Text = "标题2";
            cTitle2.Width = 100;

            ColumnHeader cTitle3 = new ColumnHeader();
            cTitle3.Text = "标题3";
            cTitle3.Width = 100;

            listView_DrawingDoor.Columns.AddRange(new ColumnHeader[] { cMark, cName, cTitle1, cTitle2, cTitle3 });
            listView_DrawingDoor.View = System.Windows.Forms.View.Details;
        }

        /// <summary>
        /// 判断tekla是否已经打开;
        /// </summary>
        /// <returns></returns>
        private bool JudgeTeklaIsOpen()
        {
            CDimManager.GetInstance().Init();

            if(!CDimManager.GetInstance().GetModel().GetConnectionStatus())
			{
                MessageBox.Show("Tekla Structures must be opened!");

                return false;
            }
            return true;
        }

        /// <summary>
        /// 切换Tab页的响应函数;
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            int nSelIndex=tabControl1.SelectedIndex;

            if (nSelIndex == 0)
            {
                mbFrameDim = true;
                mbFrameDoorDim = false;
            }
            if (nSelIndex == 1)
            {
                mbFrameDim = false;
                mbFrameDoorDim = true;
            }
        }

        /// <summary>
        /// 验证当前该模块是否可用;
        /// </summary>
        /// <param name="mrModuleType"></param>
        /// <returns></returns>
        private bool ValidateModule(MrModuleType mrModuleType)
        {
            //需要验证加密狗;
            if (CCommonPara.mBEnableLock)
            {
                bool IsOut;
                if (!CDogTools.GetInstance().Validate(mrModuleType, out IsOut))
                {
                    if (IsOut)
                    {
                        return false;
                    }
                }
                return true;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// 顶视图标注;
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnTopView_Click(object sender, EventArgs e)
        {
            if (!ValidateModule(MrModuleType.DimTOP))
            {
                Close();
            }
            if(!JudgeTeklaIsOpen())
            {
                return;
            }

            if(mbBeamDim)
            {
                CDimManager.GetInstance().CreateBeamTopViewDim();
            }
            if(mbCylindeDim)
            {
                CDimManager.GetInstance().CreateCylinderTopViewDim();
            }
        }

        /// <summary>
        /// 前视图标注;
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnFrontView_Click(object sender, EventArgs e)
        {
            if (!ValidateModule(MrModuleType.DimBefore))
            {
                Close();
            }

            if (!JudgeTeklaIsOpen())
            {
                return;
            }

            if (mbBeamDim)
            {
                CDimManager.GetInstance().CreateBeamFrontViewDim();
            }
            if (mbCylindeDim)
            {
                CDimManager.GetInstance().CreateCylinderFrontViewDim();
            }
        }

        /// <summary>
        /// 剖视图的标注;
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSectionView_Click(object sender, EventArgs e)
        {
            if (!ValidateModule(MrModuleType.DimSection))
            {
                Close();
            }

            if (!JudgeTeklaIsOpen())
            {
                return;
            }

            if(mbBeamDim)
            {
                CDimManager.GetInstance().CreateBeamSectionView();
            }
            if (mbCylindeDim)
            {
                CDimManager.GetInstance().CreateCylinderSectionView();
            }
        }

        /// <summary>
        /// 一键快速标注;
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnOneKeyDim_Click(object sender, EventArgs e)
        {
            if (!ValidateModule(MrModuleType.DimTOP))
            {
                Close();
            }
            if (!JudgeTeklaIsOpen())
            {
                return;
            }
            string strDimType = null;

            if (mbBeamDim == true)
            {
                strDimType = "梁标注";
            }
            if (mbCylindeDim)
            {
                strDimType = "柱标注";
            }

            CDimManager.GetInstance().DrawDrawingOneKey(strDimType);
        }
           
        private void checkBoxBeam_CheckedChanged(object sender, EventArgs e)
        {
            mbBeamDim = checkBoxBeam.Checked;

            if (mbBeamDim)
            {
                checkBoxCylinder.Checked = false;
            }
        }

        private void checkBoxCylinder_CheckedChanged(object sender, EventArgs e)
        {
            mbCylindeDim = checkBoxCylinder.Checked;

            if (mbCylindeDim)
            {
                checkBoxBeam.Checked = false;
            }
        }

        /// <summary>
        /// 是否显示最上端;
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void checkBoxTopShow_CheckedChanged(object sender, EventArgs e)
        {
            if(checkBoxTopShow.Checked)
            {
                this.TopMost = true;
            }
            else
            {
                this.TopMost = false;
            }
        }

        /// <summary>
        /// 加载主控件;
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainForm_Load(object sender, EventArgs e)
        {
            try
            {
                textBox_MainSize.Text = "dim";
                textBox_GeneralSize.Text = "standard";
                textBox_Angle.Text = "angle";
                textBox_MainPart.Text = "mark";
                textBox_GeneralPart.Text = "standard";
                textBox_Bolt.Text = "bolt";
                textBox_Section.Text = "standard";
                textBox_SectionMark.Text = "standard";

                textBox_DownDepth.Text = CCommonPara.mDblSectionDownDepth.ToString();
                textBox_UpDepth.Text = CCommonPara.mDblSectionUpDepth.ToString();
                checkBox_SameSectionMark.Checked = CCommonPara.mbShowSameSectionMark;

                UpdateAttribute();

            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        
        /// <summary>
        /// 更新属性;
        /// </summary>
        private void UpdateAttribute()
        {
            CDimManager.GetInstance().Init();

            if (!CDimManager.GetInstance().GetModel().GetConnectionStatus())
            {
                return;
            }

            string strModelPath = CDimManager.GetInstance().GetModel().GetInfo().ModelPath;

            if (strModelPath == null)
            {
                return;
            }

            string strDirectory = System.AppDomain.CurrentDomain.BaseDirectory;
            string strAttribute = Path.Combine(strDirectory, "attributes");

            CCommonPara.mMainSizeDimPath = textBox_MainSize.Text + ".dim";
            CCommonPara.mSizeDimPath = textBox_GeneralSize.Text + ".dim";
            CCommonPara.mAngleDimPath = textBox_Angle.Text + ".dim";
            CCommonPara.mMainPartNotePath = textBox_MainPart.Text + ".note";
            CCommonPara.mPartNotePath = textBox_GeneralPart.Text + ".note";
            CCommonPara.mBoltNotePath = textBox_Bolt.Text + ".note";
            CCommonPara.mSectionAttPath = textBox_Section.Text + ".vi";
            CCommonPara.mSectionMarkNotePath = textBox_SectionMark.Text + ".cs";
            
            string strMainSize = Path.Combine(strAttribute, CCommonPara.mMainSizeDimPath);
            string strGeneralSize = Path.Combine(strAttribute, CCommonPara.mSizeDimPath);
            string strAngleSize = Path.Combine(strAttribute, CCommonPara.mAngleDimPath);
            string strMainPart = Path.Combine(strAttribute, CCommonPara.mMainPartNotePath);
            string strGeneralPart = Path.Combine(strAttribute, CCommonPara.mPartNotePath);
            string strBoltNote = Path.Combine(strAttribute, CCommonPara.mBoltNotePath);
            string strSectionAtt = Path.Combine(strAttribute, CCommonPara.mSectionAttPath);
            string strSectionMarkNote = Path.Combine(strAttribute, CCommonPara.mSectionMarkNotePath);

            string strModelAttPath=Path.Combine(strModelPath,"attributes");

            File.Copy(strMainSize,Path.Combine(strModelAttPath,CCommonPara.mMainSizeDimPath),true);
            File.Copy(strGeneralSize, Path.Combine(strModelAttPath, CCommonPara.mSizeDimPath), true);
            File.Copy(strAngleSize, Path.Combine(strModelAttPath, CCommonPara.mAngleDimPath), true);
            File.Copy(strMainPart, Path.Combine(strModelAttPath, CCommonPara.mMainPartNotePath), true);
            File.Copy(strGeneralPart, Path.Combine(strModelAttPath, CCommonPara.mPartNotePath), true);
            File.Copy(strBoltNote, Path.Combine(strModelAttPath, CCommonPara.mBoltNotePath), true);
            File.Copy(strSectionAtt, Path.Combine(strModelAttPath, CCommonPara.mSectionAttPath), true);
            File.Copy(strSectionMarkNote, Path.Combine(strModelAttPath, CCommonPara.mSectionMarkNotePath), true);
        }

        /// <summary>
        /// 获取标注对象的列表;
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_GetList_Click(object sender, EventArgs e)
        {
            if (!ValidateModule(MrModuleType.GetList))
            {
                Close();
            }

            if (!JudgeTeklaIsOpen())
            {
                return;
            }

            listView_Drawing.Items.Clear();

            CDimManager.GetInstance().GetSelDrawingList();

            List<CMrAssemblyDrawing> mrAssemblyDrawingList = CDimManager.GetInstance().mrAssemblyDrawingList;

            if (mrAssemblyDrawingList == null||mrAssemblyDrawingList.Count==0)
            {
                return;
            }

            foreach(CMrAssemblyDrawing drawing in mrAssemblyDrawingList)
            {
                string strName=drawing.mName;
                string strMark=drawing.mMark;
                string strTitle1 = drawing.mTitle1;
                string strTitle2 = drawing.mTitle2;
                string strTitle3 = drawing.mTitle3;

                ListViewItem listViewItem = new ListViewItem(new string[] { strMark, strName, strTitle1, strTitle2, strTitle3 });
                listViewItem.Tag = drawing;
                listView_Drawing.Items.Add(listViewItem);
            }
        }

        /// <summary>
        /// 标注设置响应函数;
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_DimSetting_Click(object sender, EventArgs e)
        {
            if (!ValidateModule(MrModuleType.SetDim))
            {
                Close();
            }
            if (!JudgeTeklaIsOpen())
            {
                return;
            }

            List<CMrAssemblyDrawing> drawingList = CDimManager.GetInstance().mrAssemblyDrawingList;

            if (drawingList.Count == 0)
            {
                MessageBox.Show("请先获取图纸列表");
                return;
            }

            DimSettingForm form = new DimSettingForm();

            form.ShowDialog(this);
        }

        /// <summary>
        /// 多张图纸选择标注;
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSelectList_Click(object sender, EventArgs e)
        {
            if (!ValidateModule(MrModuleType.DimList))
            {
                Close();
            }

            if (!JudgeTeklaIsOpen())
            {
                return;
            }

            CDimManager.GetInstance().DrawSelectListDrawing();
        }

        /// <summary>
        /// 高亮正在标注的图纸;
        /// </summary>
        /// <param name="drawing"></param>
        public void HighlightSelAssemblyDrawing(CMrAssemblyDrawing drawing)
        {
            foreach (ListViewItem lv in listView_Drawing.Items)
            {
                if (lv.Tag == drawing)
                {
                    lv.Selected = true;
                }
            }
        }

        /// <suuammary>
        /// 双击打开图纸;
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listView_Drawing_DoubleClick(object sender, EventArgs e)
        {
            ListView.SelectedListViewItemCollection lvItems = listView_Drawing.SelectedItems;
            
            if(lvItems.Count==0)
            {
                return;
            }

            ListViewItem lvItem = lvItems[0];

            object tag = lvItem.Tag;

            if(tag==null)
            {
                return;
            }

            CMrAssemblyDrawing mrDrawing = tag as CMrAssemblyDrawing;
            DrawingHandler drawingHandler = new DrawingHandler();
            AssemblyDrawing assemblyDrawing = mrDrawing.mAssemblyDring;

            drawingHandler.SetActiveDrawing(assemblyDrawing, true);
        }

        /// <summary>
        /// 删除列表项;
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ListView.SelectedListViewItemCollection lvItems =null;

            if(mbFrameDim)
            {
                lvItems = listView_Drawing.SelectedItems;
            }
            else if (mbFrameDoorDim)
            {
                lvItems = listView_DrawingDoor.SelectedItems;
            }
            if (lvItems.Count == 0)
            {
                return;
            }

            for(int i=0;i<lvItems.Count;i++)
            {
                ListViewItem lvItem = lvItems[0];

                object tag = lvItem.Tag;

                if (tag == null)
                {
                    continue;
                }

                CMrAssemblyDrawing mrDrawing = tag as CMrAssemblyDrawing;
                CDimManager.GetInstance().mrAssemblyDrawingList.Remove(mrDrawing);

                if (mbFrameDim)
                {
                    listView_Drawing.Items.Remove(lvItem);
                }
                else if (mbFrameDoorDim)
                {
                    listView_DrawingDoor.Items.Remove(lvItem);
                }
            }
        }

        /// <summary>
        /// 清空列表项;
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (mbFrameDim)
            {
                listView_Drawing.Items.Clear();
            }
            else if (mbFrameDoorDim)
            {
                listView_DrawingDoor.Items.Clear();
            }

            CDimManager.GetInstance().mrAssemblyDrawingList.Clear();
            CDimManager.GetInstance().mDicMarkNumberToType.Clear();
        }

        /// <summary>
        /// 测试按钮;
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_Test_Click(object sender, EventArgs e)
        {
            if (!JudgeTeklaIsOpen())
            {
                return;
            }

            QueryInfoFrom queryInfoForm = new QueryInfoFrom();
            queryInfoForm.Show();
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (!CCommonPara.mBEnableLock)
            {
                return;
            }
            try
            {
                CServers.GetServers().CloseService();
            }
            catch { }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
          //  return;
            timer1.Interval = 1000 * 60 * 1;
            try
            {               
                // timer1.Enabled = false;
                if (timer1.Interval < 1000)
                {
                    timer1.Interval = 1000 * 60 * 1;
                    int re = CDogTools.GetInstance().GetUserAuthority();
                    if (re == -1)
                    {
                        CServers.GetServers().CloseService();
                        MessageBox.Show("您的加密狗已经在其他程序中使用，不能重复登录！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        this.Close();
                    }
                    else if (re <= 0)
                    {
                        CServers.GetServers().CloseService();
                        MessageBox.Show("未检测到加密狗！请确认是否已经插好！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        this.Close();
                    }
                }
                else
                {
                     if(!CDimTools.GetInstance().IsOut) 
                     {
                        CServers.GetServers().GetUserName();
                     }
                     else
                     {
                         MessageBox.Show("服务器异常，请确认网络或者加密狗", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                         this.Close();
                     }
                }
            }
            catch
            {
                MessageBox.Show("服务器异常，请确认网络或者加密狗", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.Close();
            }
        }

        /// <summary>
        /// 门式钢架的顶视图标注;
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonTopViewDoor_Click(object sender, EventArgs e)
        {
            if (!ValidateModule(MrModuleType.DimTOP))
            {
                Close();
            }
            if (!JudgeTeklaIsOpen())
            {
                return;
            }
            //门式钢架没有顶视图标注;
            if (mbBeamDoorDim)
            {
                return;
            }
            //门式框架结构中的柱标注;
            if (mbCylinderDoorDim)
            {
                CDimManager.GetInstance().CreateCylinderDoorTopView();
            }
        }

        /// <summary>
        /// 门式钢架的前视图标注;
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonFrontViewDoor_Click(object sender, EventArgs e)
        {
            if (!ValidateModule(MrModuleType.DimBefore))
            {
                Close();
            }
            if (!JudgeTeklaIsOpen())
            {
                return;
            }

            //门式框架结构中的梁标注;
            if (mbBeamDoorDim)
            {
                CDimManager.GetInstance().CreateBeamDoorFrontView();
            }
            //门式框架结构中的柱标注;
            if (mbCylinderDoorDim)
            {
                CDimManager.GetInstance().CreateCylinderDoorFrontView();
            }
        }

        /// <summary>
        /// 门式框架的剖视图标注;
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonSectionViewDoor_Click(object sender, EventArgs e)
        {
            if (!ValidateModule(MrModuleType.DimSection))
            {
                Close();
            }
            if (!JudgeTeklaIsOpen())
            {
                return;
            }
            if (mbBeamDoorDim)
            {
                CDimManager.GetInstance().CreateBeamDoorSectionView();
            }
            if (mbCylinderDoorDim)
            {
                CDimManager.GetInstance().CreateCylinderDoorSectionView();
            }
        }

        /// <summary>
        /// 门式框架的梁标注;
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void checkBoxBeamDoor_CheckedChanged(object sender, EventArgs e)
        {
            mbBeamDoorDim = checkBoxBeamDoor.Checked;

            if (mbBeamDoorDim)
            {
                checkBoxCylinderDoor.Checked = false;
            }
        }

        /// <summary>
        /// 门式框架的柱标注;
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void checkBoxCylinderDoor_CheckedChanged(object sender, EventArgs e)
        {
            mbCylinderDoorDim = checkBoxCylinderDoor.Checked;

            if (mbCylinderDoorDim)
            {
                checkBoxBeamDoor.Checked = false;
            }
        }

        /// <summary>
        /// 门式框架结构一键标注;
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonOneKeyDimDoor_Click(object sender, EventArgs e)
        {
            if (!ValidateModule(MrModuleType.DimTOP))
            {
                Close();
            }
            if (!JudgeTeklaIsOpen())
            {
                return;
            }

            string strDimType = null;

            if (mbBeamDoorDim == true)
            {
                strDimType = "梁标注";
            }
            if (mbCylinderDoorDim)
            {
                strDimType = "柱标注";
            }

            CDimManager.GetInstance().DrawDrawingDoorOneKey(strDimType);
        }

        /// <summary>
        /// 获取门式框架结构选择的列表;
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_GetListDoor_Click(object sender, EventArgs e)
        {
            if (!ValidateModule(MrModuleType.DimList))
            {
                Close();
            }
            if (!JudgeTeklaIsOpen())
            {
                return;
            }

            listView_DrawingDoor.Items.Clear();

            CDimManager.GetInstance().GetSelDrawingList();

            List<CMrAssemblyDrawing> mrAssemblyDrawingList = CDimManager.GetInstance().mrAssemblyDrawingList;

            if (mrAssemblyDrawingList == null || mrAssemblyDrawingList.Count == 0)
            {
                return;
            }

            foreach (CMrAssemblyDrawing drawing in mrAssemblyDrawingList)
            {
                string strName = drawing.mName;
                string strMark = drawing.mMark;
                string strTitle1 = drawing.mTitle1;
                string strTitle2 = drawing.mTitle2;
                string strTitle3 = drawing.mTitle3;

                ListViewItem listViewItem = new ListViewItem(new string[] { strMark, strName, strTitle1, strTitle2, strTitle3 });
                listViewItem.Tag = drawing;
                listView_DrawingDoor.Items.Add(listViewItem);
            }
        }

        /// <summary>
        /// 门式框架结构的标注设置;
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_DimSettingDoor_Click(object sender, EventArgs e)
        {
            if (!ValidateModule(MrModuleType.SetDim))
            {
                Close();
            }
            if (!JudgeTeklaIsOpen())
            {
                return;
            }

            List<CMrAssemblyDrawing> drawingList = CDimManager.GetInstance().mrAssemblyDrawingList;

            if (drawingList.Count == 0)
            {
                MessageBox.Show("请先获取图纸列表");
                return;
            }

            DimSettingForm form = new DimSettingForm();

            form.ShowDialog(this);
        }

        /// <summary>
        /// 门式框架结构列表标注;
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_DimListDoor_Click(object sender, EventArgs e)
        {
            if (!JudgeTeklaIsOpen())
            {
                return;
            }

            CDimManager.GetInstance().DrawSelectListDrawingDoor();
        }

        /// <summary>
        /// 双击打开图纸;
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listView_DrawingDoor_DoubleClick(object sender, EventArgs e)
        {
            ListView.SelectedListViewItemCollection lvItems = listView_DrawingDoor.SelectedItems;

            if (lvItems.Count == 0)
            {
                return;
            }

            ListViewItem lvItem = lvItems[0];

            object tag = lvItem.Tag;

            if (tag == null)
            {
                return;
            }

            CMrAssemblyDrawing mrDrawing = tag as CMrAssemblyDrawing;
            DrawingHandler drawingHandler = new DrawingHandler();
            AssemblyDrawing assemblyDrawing = mrDrawing.mAssemblyDring;

            drawingHandler.SetActiveDrawing(assemblyDrawing, true);
        }

        /// <summary>
        /// 应用属性;
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_ApplyAttribute_Click(object sender, EventArgs e)
        {
            UpdateAttribute();
        }

        /// <summary>
        /// 自动剖面测试;
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_AutoSection_Click(object sender, EventArgs e)
        {
            if (!ValidateModule(MrModuleType.DimTOP))
            {
                Close();
            }
            if (!JudgeTeklaIsOpen())
            {
                return;
            }
            string strDimType = null;

            if (mbBeamDim == true)
            {
                strDimType = "梁标注";
            }
            if (mbCylindeDim)
            {
                strDimType = "柱标注";
            }

            CDimManager.GetInstance().CreateAutoSectionTest(strDimType,0);
        }

        private void button_AutoSectionDoor_Click(object sender, EventArgs e)
        {
            if (!ValidateModule(MrModuleType.DimTOP))
            {
                Close();
            }
            if (!JudgeTeklaIsOpen())
            {
                return;
            }
            string strDimType = null;

            if (mbBeamDoorDim == true)
            {
                strDimType = "梁标注";
            }
            if (mbCylinderDoorDim)
            {
                strDimType = "柱标注";
            }

            CDimManager.GetInstance().CreateAutoSectionTest(strDimType,1);
        }

        /// <summary>
        /// 相同剖面的显示标记的check状态;
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void checkBox_SameSectionMark_CheckedChanged(object sender, EventArgs e)
        {
            CCommonPara.mbShowSameSectionMark = checkBox_SameSectionMark.Checked;
        }

        /// <summary>
        /// 向下剖面的深度;
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBox_DownDepth_TextChanged(object sender, EventArgs e)
        {
            try
            {
                CCommonPara.mDblSectionDownDepth = Convert.ToDouble(textBox_DownDepth.Text);
            }
            catch (System.Exception ex)
            {
                CCommonPara.mDblSectionDownDepth = 10;
            }
        }

        /// <summary>
        /// 向上剖面的深度;
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBox_UpDepth_TextChanged(object sender, EventArgs e)
        {
            try
            {
                CCommonPara.mDblSectionUpDepth = Convert.ToDouble(textBox_UpDepth.Text);
            }
            catch (System.Exception ex)
            {
                CCommonPara.mDblSectionUpDepth = 10;
            }
        }
    }
}
