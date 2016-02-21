using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using AutoDimension.Entity;
using AutoDimension.UI;
using System.IO;
using System.Xml;

namespace AutoDimension
{
    public partial class NewMainForm : Form
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
        /// 当前选择的视图名称;
        /// </summary>
        private string mCurrentViewType = "顶视图";

        /// <summary>
        /// 当前选择的视图类型是梁还是柱;
        /// </summary>
        private string mCurrentViewName = "梁视图";

        /// <summary>
        /// 构造函数;
        /// </summary>
        public NewMainForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 加载窗口;
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NewMainForm_Load(object sender, EventArgs e)
        {
            InitForm();
            InitListViewDrawing();
            InitListViewDrawingDoor();
            InitDimSettingFile();

            dataGridViewBeam.AllowUserToAddRows = false;
            treeViewBeam.ExpandAll();
            TreeNode selTreeNode = treeViewBeam.Nodes[0].FirstNode;
            treeViewBeam.SelectedNode = selTreeNode;
            UpdateDataGridView();

            comboBox_TopViewDimSetting.SelectedIndex = 0;
            comboBox_FrontViewDimSetting.SelectedIndex = 0;            

            progressBar_Current.Minimum = 0;
            progressBar_Current.Maximum = 100;
            progressBar_Current.Step = 1;
            progressBar_Whole.Minimum = 0;
            progressBar_Whole.Maximum = 100;
            progressBar_Whole.Step = 1;

            textBox_BoltCombineDistance.Text = CCommonPara.mDefaultTwoBoltArrayGap.ToString();
            textBox_PartCombineDistance.Text = CCommonPara.mDefaultTwoDimLineGap.ToString();
            textBox_DefaultDimDistance.Text = CCommonPara.mDefaultDimDistanceWithScale.ToString();
            textBox_TwoDimLineGap.Text = CCommonPara.mDefaultTwoDimLineGapWithScale.ToString();
            textBox_DefaultDimDistanceThreshold.Text = CCommonPara.mDefaultDimDistanceThreshold.ToString();

            textBox_SectionDepth.Text = CCommonPara.mDblSectionUpDepth.ToString();
            comboBox_AutoSectionSetting.SelectedIndex = 0;
            comboBox_SectionMark.SelectedIndex = 0;
            comboBox_HoritionalSection.SelectedIndex = 0;
            comboBox_VerticalSection.SelectedIndex = 0;

        }

        private void InitForm()
        {
            comboBoxDimType.SelectedIndex = 0;
            mbBeamDim = true;
            mbCylindeDim = false;
            comboBoxDimTypeDoor.SelectedIndex = 0;
            mbBeamDoorDim = true;
            mbCylinderDoorDim = false;
            checkBoxTopShow.Checked = false;
            checkBoxTopShow.BringToFront();

            //加载属性;
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

                ApplyAttribute();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// 加载标注配置文件;
        /// </summary>
        private void InitDimSettingFile()
        {
            string strDirectory = System.AppDomain.CurrentDomain.BaseDirectory;
            string strFolderFullName = Path.Combine(strDirectory, "DimSetting");

            if (File.Exists(strFolderFullName))
            {
                return;
            }

            DirectoryInfo TheFolder = new DirectoryInfo(strFolderFullName);
            FileInfo[] infos=TheFolder.GetFiles();

            foreach (FileInfo info in infos)
            {
                string strName = info.Name;
                strName = Path.GetFileNameWithoutExtension(strName);
                comboBox_DimSetting.Items.Add(strName);
            }
            comboBox_DimSetting.SelectedIndex = 0;
        }

        /// <summary>
        /// 应用属性;
        /// </summary>
        private void ApplyAttribute()
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

            string strModelAttPath = Path.Combine(strModelPath, "attributes");

            File.Copy(strMainSize, Path.Combine(strModelAttPath, CCommonPara.mMainSizeDimPath), true);
            File.Copy(strGeneralSize, Path.Combine(strModelAttPath, CCommonPara.mSizeDimPath), true);
            File.Copy(strAngleSize, Path.Combine(strModelAttPath, CCommonPara.mAngleDimPath), true);
            File.Copy(strMainPart, Path.Combine(strModelAttPath, CCommonPara.mMainPartNotePath), true);
            File.Copy(strGeneralPart, Path.Combine(strModelAttPath, CCommonPara.mPartNotePath), true);
            File.Copy(strBoltNote, Path.Combine(strModelAttPath, CCommonPara.mBoltNotePath), true);
            File.Copy(strSectionAtt, Path.Combine(strModelAttPath, CCommonPara.mSectionAttPath), true);
            File.Copy(strSectionMarkNote, Path.Combine(strModelAttPath, CCommonPara.mSectionMarkNotePath), true);
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

            listView_Drawing.Columns.AddRange(new ColumnHeader[] { cMark, cName, cTitle1, cTitle2, cTitle3 });
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

            if (!CDimManager.GetInstance().GetModel().GetConnectionStatus())
            {
                MessageBox.Show("Tekla Structures must be opened!");
                return false;
            }
            return true;
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
        /// 标注类型切换响应;
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboBoxDimType_SelectedValueChanged(object sender, EventArgs e)
        {
            int nIndex = comboBoxDimType.SelectedIndex;

            if (nIndex == 0)
            {
                mbBeamDim = true;
                mbCylindeDim = false;
            }
            if (nIndex == 1)
            {
                mbBeamDim = false;
                mbCylindeDim = true;
            }
        }

        /// <summary>
        /// 顶视图标注;
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonTopView_Click(object sender, EventArgs e)
        {
            if (!ValidateModule(MrModuleType.DimTOP))
            {
                Close();
            }
            if (!JudgeTeklaIsOpen())
            {
                return;
            }
            if (mbBeamDim)
            {
                CDimManager.GetInstance().CreateBeamTopViewDim();
            }
            if (mbCylindeDim)
            {
                CDimManager.GetInstance().CreateCylinderTopViewDim();
            }
        }

        /// <summary>
        /// 前视图标注;
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonFrontView_Click(object sender, EventArgs e)
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
        /// 剖视图标注;
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonSectionView_Click(object sender, EventArgs e)
        {
            if (!ValidateModule(MrModuleType.DimSection))
            {
                Close();
            }
            if (!JudgeTeklaIsOpen())
            {
                return;
            }
            if (mbBeamDim)
            {
                CDimManager.GetInstance().CreateBeamSectionView();
            }
            if (mbCylindeDim)
            {
                CDimManager.GetInstance().CreateCylinderSectionView();
            }
        }

        /// <summary>
        /// 一键标注;
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonOneKeyDim_Click(object sender, EventArgs e)
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

        /// <summary>
        /// 获取选择的标注列表;
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonGetList_Click(object sender, EventArgs e)
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
                listView_Drawing.Items.Add(listViewItem);
            }
        }

        /// <summary>
        /// 多张图纸标注设置;
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonListDimSetting_Click(object sender, EventArgs e)
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
        /// 开始批量标注;
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonBeginDim_Click(object sender, EventArgs e)
        {
            if (!ValidateModule(MrModuleType.DimList))
            {
                Close();
            }
            if (!JudgeTeklaIsOpen())
            {
                return;
            }
            CDimManager.GetInstance().mMainForm = this;
            CDimManager.GetInstance().DrawSelectListDrawing();
        }

        private delegate void SetProgressMaxDelegate(int nMax);
        private delegate void SetProgressPosDelegate(int nValue);
        private delegate void SetLableText(string strText);

        /// <summary>
        /// 设置当前进度条的范围;
        /// </summary>
        /// <param name="nMax"></param>
        public void SetCurrentProgressMax(int nMax)
        {
            if (progressBar_Current.InvokeRequired)//等待异步;
            {
                SetProgressMaxDelegate fc = new SetProgressMaxDelegate(SetCurrentProgressMax);
                this.Invoke(fc, new object[] { nMax }); //通过代理调用刷新方法;
            }
            else
            {
                progressBar_Current.Maximum = nMax;
                progressBar_Current.Minimum = 0;
            }
        }

        /// <summary>
        /// 设置总进度条的范围;
        /// </summary>
        /// <param name="nMax"></param>
        public void SetWholeProgressMax(int nMax)
        {
            if (progressBar_Whole.InvokeRequired)//等待异步;
            {
                SetProgressMaxDelegate fc = new SetProgressMaxDelegate(SetWholeProgressMax);
                this.Invoke(fc, new object[] { nMax }); //通过代理调用刷新方法;
            }
            else
            {
                progressBar_Whole.Maximum = nMax;
                progressBar_Whole.Minimum = 0;
            }
        }

        /// <summary>
        /// 设置当前进度条的位置;
        /// </summary>
        /// <param name="nValue"></param>
        public void SetCurrentProgressPos(int nValue)
        {
            if (nValue < 0)
            {
                return;
            }
            if (progressBar_Current.InvokeRequired)//等待异步;
            {
                SetProgressPosDelegate fc = new SetProgressPosDelegate(SetCurrentProgressPos);
                this.Invoke(fc, new object[] { nValue }); //通过代理调用刷新方法;
            }
            else
            {   
                if (nValue <= progressBar_Current.Maximum)
                {
                    progressBar_Current.Value = nValue;
                }
            }
        }

        /// <summary>
        /// 设置总进度条的位置;
        /// </summary>
        /// <param name="nValue"></param>
        public void SetWholeProgressPos(int nValue)
        {
            if (nValue < 0)
            {
                return;
            }
            if (progressBar_Whole.InvokeRequired)//等待异步;
            {
                SetProgressPosDelegate fc = new SetProgressPosDelegate(SetWholeProgressPos);
                this.Invoke(fc, new object[] { nValue }); //通过代理调用刷新方法;
            }
            else
            {
                if (nValue <= progressBar_Whole.Maximum)
                {
                    progressBar_Whole.Value = nValue;
                }
            }
        }

        /// <summary>
        /// 设置当前进度条的提示文字;
        /// </summary>
        /// <param name="strText"></param>
        public void SetCurrentLabelText(string strText)
        {
            if (progressBar_Current.InvokeRequired)//等待异步;
            {
                SetLableText fc = new SetLableText(SetCurrentLabelText);
                this.Invoke(fc, new object[] { strText }); //通过代理调用刷新方法;
            }
            else
            {
                label_CurPos.Text = strText;
            }
        }

        /// <summary>
        /// 设置整体进度条的提示文字;
        /// </summary>
        /// <param name="strText"></param>
        public void SetWholeLabelText(string strText)
        {
            if (progressBar_Whole.InvokeRequired)//等待异步;
            {
                SetLableText fc = new SetLableText(SetWholeLabelText);
                this.Invoke(fc, new object[] { strText }); //通过代理调用刷新方法;
            }
            else
            {
                label_WholePos.Text = strText;
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
        /// 应用属性设置中的属性;
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_ApplyAttribute_Click(object sender, EventArgs e)
        {
            ApplyAttribute();
        }

        private void buttonTest_Click(object sender, EventArgs e)
        {
            if (!JudgeTeklaIsOpen())
            {
                return;
            }
            QueryInfoFrom queryInfoForm = new QueryInfoFrom();
            queryInfoForm.Show();
        }

        /// <summary>
        /// 更新表格控件中的显示;
        /// </summary>
        private void UpdateDataGridView()
        {
            dataGridViewBeam.Rows.Clear();

            if (mCurrentViewName == "梁视图")
            {
                CBeamDimSetting beamDimSetting = CBeamDimSetting.GetInstance();

                //1.顶视图设置;
                if (mCurrentViewType.Equals("顶视图"))
                {
                    Dictionary<string, CDimAndMarkFlag> dimObjectDictionary = beamDimSetting.mTopViewSetting.mDimObjectDictionary;
                    Dictionary<string, CDimAndMarkFlag>.Enumerator enumerator = dimObjectDictionary.GetEnumerator();

                    while (enumerator.MoveNext())
                    {
                        KeyValuePair<string, CDimAndMarkFlag> keyPair = enumerator.Current;
                        string strKey = keyPair.Key;
                        bool bValue1 = keyPair.Value.mbDim;
                        bool bValue2 = keyPair.Value.mbMark;
                        dataGridViewBeam.Rows.Add(new object[] { strKey, bValue1, bValue2 });
                    }
                }
                //2.前视图标注设置;
                if (mCurrentViewType.Equals("前视图"))
                {
                    Dictionary<string, CDimAndMarkFlag> dimObjectDictionary = beamDimSetting.mFrontViewSetting.mDimObjectDictionary;
                    Dictionary<string, CDimAndMarkFlag>.Enumerator enumerator = dimObjectDictionary.GetEnumerator();

                    while (enumerator.MoveNext())
                    {
                        KeyValuePair<string, CDimAndMarkFlag> keyPair = enumerator.Current;
                        string strKey = keyPair.Key;
                        bool bValue1 = keyPair.Value.mbDim;
                        bool bValue2 = keyPair.Value.mbMark;
                        dataGridViewBeam.Rows.Add(new object[] { strKey, bValue1, bValue2 });
                    }
                }
                //3.剖视图标注设置;
                if (mCurrentViewType.Equals("剖视图"))
                {
                    Dictionary<string, CDimAndMarkFlag> dimObjectDictionary = beamDimSetting.mSectionViewSetting.mDimObjectDictionary;
                    Dictionary<string, CDimAndMarkFlag>.Enumerator enumerator = dimObjectDictionary.GetEnumerator();

                    while (enumerator.MoveNext())
                    {
                        KeyValuePair<string, CDimAndMarkFlag> keyPair = enumerator.Current;
                        string strKey = keyPair.Key;
                        bool bValue1 = keyPair.Value.mbDim;
                        bool bValue2 = keyPair.Value.mbMark;
                        dataGridViewBeam.Rows.Add(new object[] { strKey, bValue1, bValue2 });
                    }
                }
            }
             else if (mCurrentViewName == "柱视图")
             {
                 CCylinderDimSetting cylinderDimSetting = CCylinderDimSetting.GetInstance();

                 //1.顶视图设置;
                 if (mCurrentViewType.Equals("顶视图"))
                 {
                     Dictionary<string, CDimAndMarkFlag> dimObjectDictionary = cylinderDimSetting.mTopViewSetting.mDimObjectDictionary;
                     Dictionary<string, CDimAndMarkFlag>.Enumerator enumerator = dimObjectDictionary.GetEnumerator();

                     while (enumerator.MoveNext())
                     {
                         KeyValuePair<string, CDimAndMarkFlag> keyPair = enumerator.Current;
                         string strKey = keyPair.Key;
                         bool bValue1 = keyPair.Value.mbDim;
                         bool bValue2 = keyPair.Value.mbMark;
                         dataGridViewBeam.Rows.Add(new object[] { strKey, bValue1, bValue2 });
                     }
                 }
                 //2.前视图设置;
                 if (mCurrentViewType.Equals("前视图"))
                 {
                     Dictionary<string, CDimAndMarkFlag> dimObjectDictionary = cylinderDimSetting.mFrontViewSetting.mDimObjectDictionary;
                     Dictionary<string, CDimAndMarkFlag>.Enumerator enumerator = dimObjectDictionary.GetEnumerator();

                     while (enumerator.MoveNext())
                     {
                         KeyValuePair<string, CDimAndMarkFlag> keyPair = enumerator.Current;
                         string strKey = keyPair.Key;
                         bool bValue1 = keyPair.Value.mbDim;
                         bool bValue2 = keyPair.Value.mbMark;
                         dataGridViewBeam.Rows.Add(new object[] { strKey, bValue1, bValue2 });
                     }
                 }
             }
        }

        /// <summary>
        /// 点击树节点;
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void treeView_BeamView_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            string strNodeText = e.Node.Text;

            if (e.Node.Parent == null)
            {
                mCurrentViewName = strNodeText;

                return;
            }
            
            string strParentNodeText = e.Node.Parent.Text;

            mCurrentViewName = strParentNodeText;
            mCurrentViewType = strNodeText;

            UpdateDataGridView();
        }

        /// <summary>
        /// 点击表格单元格;
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridViewBeam_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            int nColumnIndex = e.ColumnIndex;
            int nRowIndex = e.RowIndex;

            if (nRowIndex < 0)
            {
                return;
            }
            if (mCurrentViewName.Equals("梁视图"))
            {
                if (nColumnIndex == 1)
                {
                    DataGridViewCell nameCell = dataGridViewBeam[nColumnIndex - 1, nRowIndex];

                    if (nameCell.Value == null)
                    {
                        return;
                    }

                    string strName = nameCell.Value.ToString();

                    DataGridViewCell valueCell = dataGridViewBeam[nColumnIndex, nRowIndex];

                    if (mCurrentViewType == "顶视图")
                    {
                        CBeamTopViewSetting viewSetting = CBeamDimSetting.GetInstance().mTopViewSetting;

                        bool bValue = viewSetting.FindDimValueByName(strName);
                        valueCell.Value = !bValue;
                        viewSetting.SetDimValueByName(strName, !bValue);
                    }
                    if (mCurrentViewType == "前视图")
                    {
                        CBeamFrontViewSetting viewSetting = CBeamDimSetting.GetInstance().mFrontViewSetting;

                        bool bValue = viewSetting.FindDimValueByName(strName);
                        valueCell.Value = !bValue;
                        viewSetting.SetDimValueByName(strName, !bValue);
                    }
                    if (mCurrentViewType == "剖视图")
                    {
                        CBeamSectionViewSetting viewSetting = CBeamDimSetting.GetInstance().mSectionViewSetting;

                        bool bValue = viewSetting.FindDimValueByName(strName);
                        valueCell.Value = !bValue;
                        viewSetting.SetDimValueByName(strName, !bValue);
                    }
                }
                else if (nColumnIndex == 2)
                {
                    DataGridViewCell nameCell = dataGridViewBeam[nColumnIndex - 2, nRowIndex];

                    if (nameCell.Value == null)
                    {
                        return;
                    }

                    string strName = nameCell.Value.ToString();

                    DataGridViewCell valueCell = dataGridViewBeam[nColumnIndex, nRowIndex];

                    if (mCurrentViewType == "顶视图")
                    {
                        CBeamTopViewSetting viewSetting = CBeamDimSetting.GetInstance().mTopViewSetting;

                        bool bValue = viewSetting.FindMarkValueByName(strName);
                        valueCell.Value = !bValue;
                        viewSetting.SetMarkValueByName(strName, !bValue);
                    }
                    if (mCurrentViewType == "前视图")
                    {
                        CBeamFrontViewSetting viewSetting = CBeamDimSetting.GetInstance().mFrontViewSetting;

                        bool bValue = viewSetting.FindMarkValueByName(strName);
                        valueCell.Value = !bValue;
                        viewSetting.SetMarkValueByName(strName, !bValue);
                    }
                    if (mCurrentViewType == "剖视图")
                    {
                        CBeamSectionViewSetting viewSetting = CBeamDimSetting.GetInstance().mSectionViewSetting;

                        bool bValue = viewSetting.FindMarkValueByName(strName);
                        valueCell.Value = !bValue;
                        viewSetting.SetMarkValueByName(strName, !bValue);
                    }
                }
            }
            else if (mCurrentViewName.Equals("柱视图"))
            {
                if (nColumnIndex == 1)
                {
                    DataGridViewCell nameCell = dataGridViewBeam[nColumnIndex - 1, nRowIndex];

                    if (nameCell.Value == null)
                    {
                        return;
                    }

                    string strName = nameCell.Value.ToString();

                    DataGridViewCell valueCell = dataGridViewBeam[nColumnIndex, nRowIndex];

                    if (mCurrentViewType == "顶视图")
                    {
                        CCylinderTopViewSetting viewSetting = CCylinderDimSetting.GetInstance().mTopViewSetting;

                        bool bValue = viewSetting.FindDimValueByName(strName);
                        valueCell.Value = !bValue;
                        viewSetting.SetDimValueByName(strName, !bValue);
                    }
                    if (mCurrentViewType == "前视图")
                    {
                        CCylinderFrontViewSetting viewSetting = CCylinderDimSetting.GetInstance().mFrontViewSetting;

                        bool bValue = viewSetting.FindDimValueByName(strName);
                        valueCell.Value = !bValue;
                        viewSetting.SetDimValueByName(strName, !bValue);
                    }
                    if (mCurrentViewType == "剖视图")
                    {
                        CCylinderSectionViewSetting viewSetting = CCylinderDimSetting.GetInstance().mSectionViewSetting;

                        bool bValue = viewSetting.FindDimValueByName(strName);
                        valueCell.Value = !bValue;
                        viewSetting.SetDimValueByName(strName, !bValue);
                    }
                }
                else if (nColumnIndex == 2)
                {
                    DataGridViewCell nameCell = dataGridViewBeam[nColumnIndex - 2, nRowIndex];

                    if (nameCell.Value == null)
                    {
                        return;
                    }

                    string strName = nameCell.Value.ToString();

                    DataGridViewCell valueCell = dataGridViewBeam[nColumnIndex, nRowIndex];

                    if (mCurrentViewType == "顶视图")
                    {
                        CCylinderTopViewSetting viewSetting = CCylinderDimSetting.GetInstance().mTopViewSetting;

                        bool bValue = viewSetting.FindMarkValueByName(strName);
                        valueCell.Value = !bValue;
                        viewSetting.SetMarkValueByName(strName, !bValue);
                    }
                    if (mCurrentViewType == "前视图")
                    {
                        CCylinderFrontViewSetting viewSetting = CCylinderDimSetting.GetInstance().mFrontViewSetting;

                        bool bValue = viewSetting.FindMarkValueByName(strName);
                        valueCell.Value = !bValue;
                        viewSetting.SetMarkValueByName(strName, !bValue);
                    }
                    if (mCurrentViewType == "剖视图")
                    {
                        CCylinderSectionViewSetting viewSetting = CCylinderDimSetting.GetInstance().mSectionViewSetting;

                        bool bValue = viewSetting.FindMarkValueByName(strName);
                        valueCell.Value = !bValue;
                        viewSetting.SetMarkValueByName(strName, !bValue);
                    }
                }
            }
        }

        private void buttonTest_Click_1(object sender, EventArgs e)
        {
            if (!JudgeTeklaIsOpen())
            {
                return;
            }
            QueryInfoFrom queryInfoForm = new QueryInfoFrom();
            queryInfoForm.Show();
        }

        /// <summary>
        /// 删除菜单响应;
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ListView.SelectedListViewItemCollection lvItems = null;
            
            lvItems = listView_Drawing.SelectedItems;
           
            if (lvItems.Count == 0)
            {
                return;
            }

            for (int i = 0; i < lvItems.Count; i++)
            {
                ListViewItem lvItem = lvItems[0];

                object tag = lvItem.Tag;

                if (tag == null)
                {
                    continue;
                }

                CMrAssemblyDrawing mrDrawing = tag as CMrAssemblyDrawing;
                CDimManager.GetInstance().mrAssemblyDrawingList.Remove(mrDrawing);
                listView_Drawing.Items.Remove(lvItem);
            }
        }

        /// <summary>
        /// 清空菜单响应;
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            listView_Drawing.Items.Clear();
            CDimManager.GetInstance().mrAssemblyDrawingList.Clear();
            CDimManager.GetInstance().mDicMarkNumberToType.Clear();
        }

        /// <summary>
        /// 读取标注配置;
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_ReadDimSetting_Click(object sender, EventArgs e)
        {
            string strDimSettingFile = comboBox_DimSetting.Text;

            if (strDimSettingFile.Equals("") || strDimSettingFile == null)
            {
                MessageBox.Show("文件名不存在！");
                return;
            }

            string strDirectory = System.AppDomain.CurrentDomain.BaseDirectory;
            string strXmlFileName = Path.Combine(strDirectory, "DimSetting");
            strDimSettingFile = strDimSettingFile + ".xml";
            strXmlFileName = Path.Combine(strXmlFileName, strDimSettingFile);

            CBeamTopViewSetting beamTopViewSetting = CBeamDimSetting.GetInstance().mTopViewSetting;
            CBeamFrontViewSetting beamFrontViewSetting = CBeamDimSetting.GetInstance().mFrontViewSetting;
            CBeamSectionViewSetting beamSectionViewSetting = CBeamDimSetting.GetInstance().mSectionViewSetting;

            CCylinderTopViewSetting cylinderTopViewSetting = CCylinderDimSetting.GetInstance().mTopViewSetting;
            CCylinderFrontViewSetting cylinderFrontViewSetting = CCylinderDimSetting.GetInstance().mFrontViewSetting;
            CCylinderSectionViewSetting cylinderSectionViewSetting = CCylinderDimSetting.GetInstance().mSectionViewSetting;

            if (!File.Exists(strXmlFileName))
            {
                MessageBox.Show("文件不存在，请重新选择！");
                return;
            }

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(strXmlFileName);

            XmlNode xmlNode = xmlDoc.SelectSingleNode("rootNode");

            foreach (XmlNode node in xmlNode.ChildNodes)
            {
                XmlElement element = node as XmlElement;
                string strName = element.LocalName;

                if (strName.Equals("BeamTopDimSetting"))
                {
                    beamTopViewSetting.ReadSettingFromXml(xmlDoc, node);
                }
                if (strName.Equals("BeamFrontDimSetting"))
                {
                    beamFrontViewSetting.ReadSettingFromXml(xmlDoc, node);
                }
                if (strName.Equals("CylinderTopDimSetting"))
                {
                    cylinderTopViewSetting.ReadSettingFromXml(xmlDoc, node);
                }
                if (strName.Equals("CylinderFrontDimSetting"))
                {
                    cylinderFrontViewSetting.ReadSettingFromXml(xmlDoc, node);
                }
                UpdateDataGridView();
            }
        }

        /// <summary>
        /// 保存标注配置;
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_SaveDimSetting_Click(object sender, EventArgs e)
        {
            string strDimSettingFile = comboBox_DimSetting.Text;

            if (strDimSettingFile.Equals("") || strDimSettingFile == null)
            {
                MessageBox.Show("请输入保存的配置文件名！");
                return;
            }

            string strDirectory = System.AppDomain.CurrentDomain.BaseDirectory;
            string strFolderFullName = Path.Combine(strDirectory, "DimSetting");
            strDimSettingFile = strDimSettingFile + ".xml";
            string strXmlFileName = Path.Combine(strFolderFullName, strDimSettingFile);

            CBeamTopViewSetting beamTopViewSetting = CBeamDimSetting.GetInstance().mTopViewSetting;
            CBeamFrontViewSetting beamFrontViewSetting = CBeamDimSetting.GetInstance().mFrontViewSetting;
            CBeamSectionViewSetting beamSectionViewSetting = CBeamDimSetting.GetInstance().mSectionViewSetting;

            CCylinderTopViewSetting cylinderTopViewSetting = CCylinderDimSetting.GetInstance().mTopViewSetting;
            CCylinderFrontViewSetting cylinderFrontViewSetting = CCylinderDimSetting.GetInstance().mFrontViewSetting;
            CCylinderSectionViewSetting cylinderSectionViewSetting = CCylinderDimSetting.GetInstance().mSectionViewSetting;

            XmlDocument xmlDoc = new XmlDocument();

            //创建类型声明节点;  
            XmlNode xmlRoot = xmlDoc.CreateXmlDeclaration("1.0", "utf-8", "");
            xmlDoc.AppendChild(xmlRoot);

            XmlNode rootNode = xmlDoc.CreateElement("element", "rootNode", "");
            xmlDoc.AppendChild(rootNode);

            XmlNode xmlNode = xmlDoc.CreateNode("element", "BeamTopDimSetting", "");
            beamTopViewSetting.SaveSettingToXml(xmlDoc, xmlNode);
            rootNode.AppendChild(xmlNode);

            xmlNode = xmlDoc.CreateNode("element", "BeamFrontDimSetting", "");
            beamFrontViewSetting.SaveSettingToXml(xmlDoc, xmlNode);
            rootNode.AppendChild(xmlNode);

            xmlNode = xmlDoc.CreateNode("element", "CylinderTopDimSetting", "");
            cylinderTopViewSetting.SaveSettingToXml(xmlDoc, xmlNode);
            rootNode.AppendChild(xmlNode);

            xmlNode = xmlDoc.CreateNode("element", "CylinderFrontDimSetting", "");
            cylinderFrontViewSetting.SaveSettingToXml(xmlDoc, xmlNode);
            rootNode.AppendChild(xmlNode);

            xmlDoc.Save(strXmlFileName);

            comboBox_DimSetting.Items.Clear();
            DirectoryInfo TheFolder = new DirectoryInfo(strFolderFullName);

            FileInfo[] infos = TheFolder.GetFiles();

            foreach (FileInfo info in infos)
            {
                string strName = info.Name;
                strName = Path.GetFileNameWithoutExtension(strName);
                comboBox_DimSetting.Items.Add(strName);
            }
            comboBox_DimSetting.Text = Path.GetFileNameWithoutExtension(strDimSettingFile);
        }

        private void NewMainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (!CCommonPara.mBEnableLock)
            {
                return;
            }
            try
            {
                CServers.GetServers().CloseService();
            }
            catch(Exception ex)
            {
                //MessageBox.Show(ex.Message);
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
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
                    if (!CDimTools.GetInstance().IsOut)
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
        /// 门刚标注选择;
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboBoxDimTypeDoor_SelectedValueChanged(object sender, EventArgs e)
        {
            int nIndex = comboBoxDimTypeDoor.SelectedIndex;

            if (nIndex == 0)
            {
                mbBeamDoorDim = true;
                mbCylinderDoorDim = false;
            }
            if (nIndex == 1)
            {
                mbBeamDoorDim = false;
                mbCylinderDoorDim = true;
            }
        }

        /// <summary>
        /// 门刚顶视图标注;
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
        /// 门刚前视图标注;
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
        /// 门刚剖视图标注;
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
        /// 门刚一键标注;
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
        /// 获取门刚图纸标注的列表;
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
        /// 门刚列表标注设置;
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonListDimSettingDoor_Click(object sender, EventArgs e)
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
        /// 开始门刚列表标注;
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonBeginDimDoor_Click(object sender, EventArgs e)
        {
            if (!ValidateModule(MrModuleType.DimList))
            {
                Close();
            }
            if (!JudgeTeklaIsOpen())
            {
                return;
            }
            CDimManager.GetInstance().mMainForm = this;
            CDimManager.GetInstance().DrawSelectListDrawingDoor();
        }

        /// <summary>
        /// 标注时的默认距离，该距离需要乘以比例;
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBox_DefaultDimDistance_TextChanged(object sender, EventArgs e)
        {
            string strValue=textBox_DefaultDimDistance.Text;
            try
            {
                double dblValue = Convert.ToDouble(strValue);
                CCommonPara.mDefaultDimDistanceWithScale = dblValue;
            }
            catch
            {
                CCommonPara.mDefaultDimDistanceWithScale = 15;
            }
        }

        /// <summary>
        /// 尺寸线之间的间距,实际的间距需要乘以视图比例;
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBox_TwoDimLineGap_TextChanged(object sender, EventArgs e)
        {
            string strValue = textBox_TwoDimLineGap.Text;

            try
            {
                double dblValue = Convert.ToDouble(strValue);
                CCommonPara.mDefaultTwoDimLineGapWithScale = dblValue;
            }
            catch
            {
                CCommonPara.mDefaultTwoDimLineGapWithScale = 15;
            }
        }

        /// <summary>
        /// 螺钉尺寸组合距离;
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBox_BoltCombineDistance_TextChanged(object sender, EventArgs e)
        {
            string strValue = textBox_BoltCombineDistance.Text;
            
            try
            {
                double dblValue = Convert.ToDouble(strValue);
                CCommonPara.mDefaultTwoBoltArrayGap = dblValue;
            }
            catch
            {
                CCommonPara.mDefaultTwoBoltArrayGap = 150;
            }
        }

        /// <summary>
        /// 零件尺寸组合距离;
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBox_PartCombineDistance_TextChanged(object sender, EventArgs e)
        {
            string strValue = textBox_PartCombineDistance.Text;
            try
            {
                double dblValue = Convert.ToDouble(strValue);
                CCommonPara.mDefalutTwoPartGap = dblValue;
            }
            catch
            {
                CCommonPara.mDefalutTwoPartGap = 150;
            }
        }

        /// <summary>
        /// 默认的标注阈值;
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBox_DefaultDimDistanceThreshold_TextChanged(object sender, EventArgs e)
        {
            string strValue = textBox_DefaultDimDistanceThreshold.Text;

            try
            {
                double dblValue = Convert.ToDouble(strValue);
                CCommonPara.mDefaultDimDistanceThreshold = dblValue;
            }
            catch
            {
                CCommonPara.mDefaultDimDistanceThreshold = 200;
            }
        }

        /// <summary>
        /// 顶视图尺寸设置选择;
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboBox_TopViewDimSetting_SelectionChangeCommitted(object sender, EventArgs e)
        {
            string strText=comboBox_TopViewDimSetting.Text;

            if (strText.Equals("总尺寸"))
            {
                CBeamDimSetting.GetInstance().mTopViewSetting.mDimOverallSize = true;
                CBeamDimSetting.GetInstance().mTopViewSetting.mDimSize = false;
            }
            else if (strText.Equals("分尺寸"))
            {
                CBeamDimSetting.GetInstance().mTopViewSetting.mDimOverallSize = false;
                CBeamDimSetting.GetInstance().mTopViewSetting.mDimSize = true;
            }
        }

        /// <summary>
        /// 前视图尺寸设置选择;
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboBox_FrontViewDimSetting_SelectionChangeCommitted(object sender, EventArgs e)
        {
            string strText = comboBox_FrontViewDimSetting.Text;

            if (strText.Equals("总尺寸"))
            {
                CBeamDimSetting.GetInstance().mFrontViewSetting.mDimOverallSize = true;
                CBeamDimSetting.GetInstance().mFrontViewSetting.mDimSize = false;
            }
            else if (strText.Equals("分尺寸"))
            {
                CBeamDimSetting.GetInstance().mFrontViewSetting.mDimOverallSize = false;
                CBeamDimSetting.GetInstance().mFrontViewSetting.mDimSize = true;
            }
        }

        /// <summary>
        /// 修改剖面深度;
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBox_SectionDepth_TextChanged(object sender, EventArgs e)
        {
            string strText = comboBox_TopViewDimSetting.Text;

            try
            {
                double dblValue = Convert.ToDouble(strText);
                CCommonPara.mDblSectionUpDepth = dblValue;
                CCommonPara.mDblSectionDownDepth = dblValue;
            }
            catch
            {
                CCommonPara.mDblSectionUpDepth = 100;
                CCommonPara.mDblSectionDownDepth = 100;
            }
        }

        /// <summary>
        /// 自动剖面设置;
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboBox_AutoSectionSetting_SelectionChangeCommitted(object sender, EventArgs e)
        {
            string strText = comboBox_AutoSectionSetting.Text;

            if (strText.Equals("无"))
            {
                CCommonPara.mAutoSectionType = MrAutoSectionType.MrNone;
            }
            else if (strText.Equals("一键标注"))
            {
                CCommonPara.mAutoSectionType = MrAutoSectionType.MrOneKeyDim;
            }
            else if (strText.Equals("列表标注"))
            {
                CCommonPara.mAutoSectionType = MrAutoSectionType.MrListDim;
            }
            else if (strText.Equals("两者都"))
            {
                CCommonPara.mAutoSectionType = MrAutoSectionType.MrTwoTypeDim;
            }
        }

        /// <summary>
        /// 剖面符号选择;
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboBox_SectionMark_SelectionChangeCommitted(object sender, EventArgs e)
        {
            string strText = comboBox_SectionMark.Text;

            if (strText.Equals("仅一次"))
            {
                CCommonPara.mbShowSameSectionMark = false;
            }
            else if (strText.Equals("全部都"))
            {
                CCommonPara.mbShowSameSectionMark = true;
            }
        }

        /// <summary>
        /// 水平方向剖面设置;
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboBox_HoritionalSection_SelectionChangeCommitted(object sender, EventArgs e)
        {
            string strText = comboBox_HoritionalSection.Text;

            if (strText.Equals("向右"))
            {
                CCommonPara.mHorizontalSection = MrSectionOrientation.MrSectionRight;
            }
            else if (strText.Equals("向左"))
            {
                CCommonPara.mHorizontalSection = MrSectionOrientation.MrSectionLeft;
            }
        }

        /// <summary>
        /// 竖直方向剖面设置;
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboBox_VerticalSection_SelectionChangeCommitted(object sender, EventArgs e)
        {
            string strText = comboBox_VerticalSection.Text;

            if (strText.Equals("向上"))
            {
                CCommonPara.mVerticalSection = MrSectionOrientation.MrSectionUp;
            }
            else if (strText.Equals("向下"))
            {
                CCommonPara.mVerticalSection = MrSectionOrientation.MrSectionDown;
            }
        }
    }
}
