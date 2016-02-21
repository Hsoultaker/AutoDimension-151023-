namespace AutoDimension
{
    partial class NewMainForm
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
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.TreeNode treeNode9 = new System.Windows.Forms.TreeNode("顶视图");
            System.Windows.Forms.TreeNode treeNode10 = new System.Windows.Forms.TreeNode("前视图");
            System.Windows.Forms.TreeNode treeNode11 = new System.Windows.Forms.TreeNode("剖视图");
            System.Windows.Forms.TreeNode treeNode12 = new System.Windows.Forms.TreeNode("梁视图", new System.Windows.Forms.TreeNode[] {
            treeNode9,
            treeNode10,
            treeNode11});
            System.Windows.Forms.TreeNode treeNode13 = new System.Windows.Forms.TreeNode("顶视图");
            System.Windows.Forms.TreeNode treeNode14 = new System.Windows.Forms.TreeNode("前视图");
            System.Windows.Forms.TreeNode treeNode15 = new System.Windows.Forms.TreeNode("剖视图");
            System.Windows.Forms.TreeNode treeNode16 = new System.Windows.Forms.TreeNode("柱视图", new System.Windows.Forms.TreeNode[] {
            treeNode13,
            treeNode14,
            treeNode15});
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tabControl2 = new System.Windows.Forms.TabControl();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.treeViewBeam = new System.Windows.Forms.TreeView();
            this.comboBox_DimSetting = new System.Windows.Forms.ComboBox();
            this.button_SaveDimSetting = new System.Windows.Forms.Button();
            this.button_ReadDimSetting = new System.Windows.Forms.Button();
            this.dataGridViewBeam = new System.Windows.Forms.DataGridView();
            this.Column1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column2 = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.Column3 = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.tabPage9 = new System.Windows.Forms.TabPage();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.textBox_PartCombineDistance = new System.Windows.Forms.TextBox();
            this.label13 = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.textBox_DefaultDimDistanceThreshold = new System.Windows.Forms.TextBox();
            this.textBox_BoltCombineDistance = new System.Windows.Forms.TextBox();
            this.label14 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.textBox_DefaultDimDistance = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.textBox_TwoDimLineGap = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.groupBox6 = new System.Windows.Forms.GroupBox();
            this.comboBox_FrontViewDimSetting = new System.Windows.Forms.ComboBox();
            this.comboBox_TopViewDimSetting = new System.Windows.Forms.ComboBox();
            this.label17 = new System.Windows.Forms.Label();
            this.label16 = new System.Windows.Forms.Label();
            this.tabPage5 = new System.Windows.Forms.TabPage();
            this.button_ApplyAttribute = new System.Windows.Forms.Button();
            this.剖面属性 = new System.Windows.Forms.GroupBox();
            this.textBox_SectionMark = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.textBox_Section = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.textBox_Bolt = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.textBox_GeneralPart = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.textBox_MainPart = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.textBox_Angle = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.textBox_GeneralSize = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox_MainSize = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.tabPage8 = new System.Windows.Forms.TabPage();
            this.comboBox_VerticalSection = new System.Windows.Forms.ComboBox();
            this.label22 = new System.Windows.Forms.Label();
            this.comboBox_SectionMark = new System.Windows.Forms.ComboBox();
            this.label21 = new System.Windows.Forms.Label();
            this.comboBox_HoritionalSection = new System.Windows.Forms.ComboBox();
            this.label20 = new System.Windows.Forms.Label();
            this.comboBox_AutoSectionSetting = new System.Windows.Forms.ComboBox();
            this.label19 = new System.Windows.Forms.Label();
            this.textBox_SectionDepth = new System.Windows.Forms.TextBox();
            this.label18 = new System.Windows.Forms.Label();
            this.tabPage7 = new System.Windows.Forms.TabPage();
            this.label_WholePos = new System.Windows.Forms.Label();
            this.label_CurPos = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.comboBoxDimType = new System.Windows.Forms.ComboBox();
            this.buttonListDimSetting = new System.Windows.Forms.Button();
            this.progressBar_Whole = new System.Windows.Forms.ProgressBar();
            this.progressBar_Current = new System.Windows.Forms.ProgressBar();
            this.listView_Drawing = new System.Windows.Forms.ListView();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.DeleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ClearToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.buttonBeginDim = new System.Windows.Forms.Button();
            this.buttonGetList = new System.Windows.Forms.Button();
            this.buttonOneKeyDim = new System.Windows.Forms.Button();
            this.buttonSectionView = new System.Windows.Forms.Button();
            this.buttonFrontView = new System.Windows.Forms.Button();
            this.buttonTopView = new System.Windows.Forms.Button();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.tabControl3 = new System.Windows.Forms.TabControl();
            this.tabPage6 = new System.Windows.Forms.TabPage();
            this.label10 = new System.Windows.Forms.Label();
            this.comboBoxDimTypeDoor = new System.Windows.Forms.ComboBox();
            this.buttonListDimSettingDoor = new System.Windows.Forms.Button();
            this.listView_DrawingDoor = new System.Windows.Forms.ListView();
            this.buttonBeginDimDoor = new System.Windows.Forms.Button();
            this.button_GetListDoor = new System.Windows.Forms.Button();
            this.buttonOneKeyDimDoor = new System.Windows.Forms.Button();
            this.buttonSectionViewDoor = new System.Windows.Forms.Button();
            this.buttonFrontViewDoor = new System.Windows.Forms.Button();
            this.buttonTopViewDoor = new System.Windows.Forms.Button();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.checkBoxTopShow = new System.Windows.Forms.CheckBox();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabControl2.SuspendLayout();
            this.tabPage4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewBeam)).BeginInit();
            this.tabPage9.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox6.SuspendLayout();
            this.tabPage5.SuspendLayout();
            this.剖面属性.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.tabPage8.SuspendLayout();
            this.tabPage7.SuspendLayout();
            this.contextMenuStrip1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tabControl3.SuspendLayout();
            this.tabPage6.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.tabControl1.Location = new System.Drawing.Point(0, 4);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(724, 421);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.tabControl2);
            this.tabPage1.Location = new System.Drawing.Point(4, 24);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(716, 393);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "框架结构";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // tabControl2
            // 
            this.tabControl2.Controls.Add(this.tabPage4);
            this.tabControl2.Controls.Add(this.tabPage9);
            this.tabControl2.Controls.Add(this.tabPage5);
            this.tabControl2.Controls.Add(this.tabPage8);
            this.tabControl2.Controls.Add(this.tabPage7);
            this.tabControl2.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.tabControl2.Location = new System.Drawing.Point(0, 1);
            this.tabControl2.Name = "tabControl2";
            this.tabControl2.SelectedIndex = 0;
            this.tabControl2.Size = new System.Drawing.Size(713, 392);
            this.tabControl2.TabIndex = 0;
            // 
            // tabPage4
            // 
            this.tabPage4.Controls.Add(this.splitContainer1);
            this.tabPage4.Location = new System.Drawing.Point(4, 22);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage4.Size = new System.Drawing.Size(705, 366);
            this.tabPage4.TabIndex = 0;
            this.tabPage4.Text = "标注设置";
            this.tabPage4.UseVisualStyleBackColor = true;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Location = new System.Drawing.Point(3, 3);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.treeViewBeam);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.comboBox_DimSetting);
            this.splitContainer1.Panel2.Controls.Add(this.button_SaveDimSetting);
            this.splitContainer1.Panel2.Controls.Add(this.button_ReadDimSetting);
            this.splitContainer1.Panel2.Controls.Add(this.dataGridViewBeam);
            this.splitContainer1.Size = new System.Drawing.Size(701, 360);
            this.splitContainer1.SplitterDistance = 160;
            this.splitContainer1.TabIndex = 4;
            // 
            // treeViewBeam
            // 
            this.treeViewBeam.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeViewBeam.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.treeViewBeam.FullRowSelect = true;
            this.treeViewBeam.HideSelection = false;
            this.treeViewBeam.ItemHeight = 18;
            this.treeViewBeam.Location = new System.Drawing.Point(0, 0);
            this.treeViewBeam.Name = "treeViewBeam";
            treeNode9.Name = "节点1";
            treeNode9.Text = "顶视图";
            treeNode10.Name = "节点2";
            treeNode10.Text = "前视图";
            treeNode11.Name = "节点3";
            treeNode11.Text = "剖视图";
            treeNode12.Name = "节点0";
            treeNode12.Text = "梁视图";
            treeNode13.Name = "顶视图";
            treeNode13.Text = "顶视图";
            treeNode14.Name = "节点2";
            treeNode14.Text = "前视图";
            treeNode15.Name = "节点3";
            treeNode15.Text = "剖视图";
            treeNode16.Name = "柱视图";
            treeNode16.Text = "柱视图";
            this.treeViewBeam.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode12,
            treeNode16});
            this.treeViewBeam.Size = new System.Drawing.Size(160, 360);
            this.treeViewBeam.TabIndex = 0;
            this.treeViewBeam.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.treeView_BeamView_NodeMouseClick);
            // 
            // comboBox_DimSetting
            // 
            this.comboBox_DimSetting.DropDownHeight = 300;
            this.comboBox_DimSetting.FormattingEnabled = true;
            this.comboBox_DimSetting.IntegralHeight = false;
            this.comboBox_DimSetting.ItemHeight = 12;
            this.comboBox_DimSetting.Location = new System.Drawing.Point(391, 5);
            this.comboBox_DimSetting.MaxDropDownItems = 10;
            this.comboBox_DimSetting.Name = "comboBox_DimSetting";
            this.comboBox_DimSetting.Size = new System.Drawing.Size(61, 20);
            this.comboBox_DimSetting.TabIndex = 3;
            // 
            // button_SaveDimSetting
            // 
            this.button_SaveDimSetting.Location = new System.Drawing.Point(457, 3);
            this.button_SaveDimSetting.Name = "button_SaveDimSetting";
            this.button_SaveDimSetting.Size = new System.Drawing.Size(74, 23);
            this.button_SaveDimSetting.TabIndex = 2;
            this.button_SaveDimSetting.Text = "保存/应用";
            this.button_SaveDimSetting.UseVisualStyleBackColor = true;
            this.button_SaveDimSetting.Click += new System.EventHandler(this.button_SaveDimSetting_Click);
            // 
            // button_ReadDimSetting
            // 
            this.button_ReadDimSetting.Location = new System.Drawing.Point(333, 3);
            this.button_ReadDimSetting.Name = "button_ReadDimSetting";
            this.button_ReadDimSetting.Size = new System.Drawing.Size(54, 23);
            this.button_ReadDimSetting.TabIndex = 1;
            this.button_ReadDimSetting.Text = "读取";
            this.button_ReadDimSetting.UseVisualStyleBackColor = true;
            this.button_ReadDimSetting.Click += new System.EventHandler(this.button_ReadDimSetting_Click);
            // 
            // dataGridViewBeam
            // 
            this.dataGridViewBeam.BackgroundColor = System.Drawing.Color.White;
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle4.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridViewCellStyle4.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridViewBeam.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle4;
            this.dataGridViewBeam.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewBeam.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Column1,
            this.Column2,
            this.Column3});
            dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle5.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle5.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridViewCellStyle5.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle5.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle5.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle5.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridViewBeam.DefaultCellStyle = dataGridViewCellStyle5;
            this.dataGridViewBeam.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridViewBeam.Location = new System.Drawing.Point(0, 0);
            this.dataGridViewBeam.Name = "dataGridViewBeam";
            dataGridViewCellStyle6.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle6.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle6.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridViewCellStyle6.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle6.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle6.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle6.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridViewBeam.RowHeadersDefaultCellStyle = dataGridViewCellStyle6;
            this.dataGridViewBeam.RowHeadersWidth = 20;
            this.dataGridViewBeam.RowTemplate.Height = 23;
            this.dataGridViewBeam.Size = new System.Drawing.Size(537, 360);
            this.dataGridViewBeam.TabIndex = 0;
            this.dataGridViewBeam.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridViewBeam_CellClick);
            // 
            // Column1
            // 
            this.Column1.HeaderText = "内容";
            this.Column1.Name = "Column1";
            this.Column1.ReadOnly = true;
            this.Column1.Width = 150;
            // 
            // Column2
            // 
            this.Column2.HeaderText = "是否标注";
            this.Column2.Name = "Column2";
            this.Column2.Width = 80;
            // 
            // Column3
            // 
            this.Column3.HeaderText = "零件标记";
            this.Column3.Name = "Column3";
            this.Column3.Width = 80;
            // 
            // tabPage9
            // 
            this.tabPage9.Controls.Add(this.groupBox2);
            this.tabPage9.Controls.Add(this.groupBox1);
            this.tabPage9.Controls.Add(this.groupBox6);
            this.tabPage9.Location = new System.Drawing.Point(4, 22);
            this.tabPage9.Name = "tabPage9";
            this.tabPage9.Size = new System.Drawing.Size(705, 366);
            this.tabPage9.TabIndex = 6;
            this.tabPage9.Text = "尺寸设置";
            this.tabPage9.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.textBox_PartCombineDistance);
            this.groupBox2.Controls.Add(this.label13);
            this.groupBox2.Controls.Add(this.label15);
            this.groupBox2.Controls.Add(this.textBox_DefaultDimDistanceThreshold);
            this.groupBox2.Controls.Add(this.textBox_BoltCombineDistance);
            this.groupBox2.Controls.Add(this.label14);
            this.groupBox2.Location = new System.Drawing.Point(4, 108);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(235, 131);
            this.groupBox2.TabIndex = 11;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "通用设置-尺寸组合";
            // 
            // textBox_PartCombineDistance
            // 
            this.textBox_PartCombineDistance.Location = new System.Drawing.Point(122, 62);
            this.textBox_PartCombineDistance.Name = "textBox_PartCombineDistance";
            this.textBox_PartCombineDistance.Size = new System.Drawing.Size(100, 21);
            this.textBox_PartCombineDistance.TabIndex = 10;
            this.textBox_PartCombineDistance.TextChanged += new System.EventHandler(this.textBox_PartCombineDistance_TextChanged);
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(7, 99);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(113, 12);
            this.label13.TabIndex = 6;
            this.label13.Text = "螺钉外侧标注距离：";
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(8, 66);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(113, 12);
            this.label15.TabIndex = 9;
            this.label15.Text = "零件尺寸组合距离：";
            // 
            // textBox_DefaultDimDistanceThreshold
            // 
            this.textBox_DefaultDimDistanceThreshold.Location = new System.Drawing.Point(122, 95);
            this.textBox_DefaultDimDistanceThreshold.Name = "textBox_DefaultDimDistanceThreshold";
            this.textBox_DefaultDimDistanceThreshold.Size = new System.Drawing.Size(100, 21);
            this.textBox_DefaultDimDistanceThreshold.TabIndex = 7;
            this.textBox_DefaultDimDistanceThreshold.TextChanged += new System.EventHandler(this.textBox_DefaultDimDistanceThreshold_TextChanged);
            // 
            // textBox_BoltCombineDistance
            // 
            this.textBox_BoltCombineDistance.Location = new System.Drawing.Point(122, 28);
            this.textBox_BoltCombineDistance.Name = "textBox_BoltCombineDistance";
            this.textBox_BoltCombineDistance.Size = new System.Drawing.Size(100, 21);
            this.textBox_BoltCombineDistance.TabIndex = 7;
            this.textBox_BoltCombineDistance.TextChanged += new System.EventHandler(this.textBox_BoltCombineDistance_TextChanged);
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(8, 31);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(113, 12);
            this.label14.TabIndex = 6;
            this.label14.Text = "螺栓尺寸组合距离：";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.textBox_DefaultDimDistance);
            this.groupBox1.Controls.Add(this.label11);
            this.groupBox1.Controls.Add(this.textBox_TwoDimLineGap);
            this.groupBox1.Controls.Add(this.label12);
            this.groupBox1.Location = new System.Drawing.Point(252, 10);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(209, 92);
            this.groupBox1.TabIndex = 8;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "通用设置-尺寸间距";
            // 
            // textBox_DefaultDimDistance
            // 
            this.textBox_DefaultDimDistance.Location = new System.Drawing.Point(93, 20);
            this.textBox_DefaultDimDistance.Name = "textBox_DefaultDimDistance";
            this.textBox_DefaultDimDistance.Size = new System.Drawing.Size(100, 21);
            this.textBox_DefaultDimDistance.TabIndex = 3;
            this.textBox_DefaultDimDistance.TextChanged += new System.EventHandler(this.textBox_DefaultDimDistance_TextChanged);
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(11, 24);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(77, 12);
            this.label11.TabIndex = 2;
            this.label11.Text = "尺寸线边距：";
            // 
            // textBox_TwoDimLineGap
            // 
            this.textBox_TwoDimLineGap.Location = new System.Drawing.Point(94, 55);
            this.textBox_TwoDimLineGap.Name = "textBox_TwoDimLineGap";
            this.textBox_TwoDimLineGap.Size = new System.Drawing.Size(100, 21);
            this.textBox_TwoDimLineGap.TabIndex = 5;
            this.textBox_TwoDimLineGap.TextChanged += new System.EventHandler(this.textBox_TwoDimLineGap_TextChanged);
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(13, 58);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(77, 12);
            this.label12.TabIndex = 4;
            this.label12.Text = "尺寸线间距：";
            // 
            // groupBox6
            // 
            this.groupBox6.Controls.Add(this.comboBox_FrontViewDimSetting);
            this.groupBox6.Controls.Add(this.comboBox_TopViewDimSetting);
            this.groupBox6.Controls.Add(this.label17);
            this.groupBox6.Controls.Add(this.label16);
            this.groupBox6.Location = new System.Drawing.Point(4, 9);
            this.groupBox6.Name = "groupBox6";
            this.groupBox6.Size = new System.Drawing.Size(235, 93);
            this.groupBox6.TabIndex = 0;
            this.groupBox6.TabStop = false;
            this.groupBox6.Text = "梁尺寸设置";
            // 
            // comboBox_FrontViewDimSetting
            // 
            this.comboBox_FrontViewDimSetting.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_FrontViewDimSetting.FormattingEnabled = true;
            this.comboBox_FrontViewDimSetting.Items.AddRange(new object[] {
            "总尺寸",
            "分尺寸"});
            this.comboBox_FrontViewDimSetting.Location = new System.Drawing.Point(71, 53);
            this.comboBox_FrontViewDimSetting.Name = "comboBox_FrontViewDimSetting";
            this.comboBox_FrontViewDimSetting.Size = new System.Drawing.Size(103, 20);
            this.comboBox_FrontViewDimSetting.TabIndex = 3;
            this.comboBox_FrontViewDimSetting.SelectionChangeCommitted += new System.EventHandler(this.comboBox_FrontViewDimSetting_SelectionChangeCommitted);
            // 
            // comboBox_TopViewDimSetting
            // 
            this.comboBox_TopViewDimSetting.BackColor = System.Drawing.SystemColors.Control;
            this.comboBox_TopViewDimSetting.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_TopViewDimSetting.FormattingEnabled = true;
            this.comboBox_TopViewDimSetting.Items.AddRange(new object[] {
            "总尺寸",
            "分尺寸"});
            this.comboBox_TopViewDimSetting.Location = new System.Drawing.Point(71, 20);
            this.comboBox_TopViewDimSetting.Name = "comboBox_TopViewDimSetting";
            this.comboBox_TopViewDimSetting.Size = new System.Drawing.Size(103, 20);
            this.comboBox_TopViewDimSetting.TabIndex = 2;
            this.comboBox_TopViewDimSetting.SelectionChangeCommitted += new System.EventHandler(this.comboBox_TopViewDimSetting_SelectionChangeCommitted);
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(12, 56);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(53, 12);
            this.label17.TabIndex = 1;
            this.label17.Text = "前视图：";
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(12, 24);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(53, 12);
            this.label16.TabIndex = 0;
            this.label16.Text = "顶视图：";
            // 
            // tabPage5
            // 
            this.tabPage5.Controls.Add(this.button_ApplyAttribute);
            this.tabPage5.Controls.Add(this.剖面属性);
            this.tabPage5.Controls.Add(this.groupBox5);
            this.tabPage5.Controls.Add(this.groupBox4);
            this.tabPage5.Location = new System.Drawing.Point(4, 22);
            this.tabPage5.Name = "tabPage5";
            this.tabPage5.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage5.Size = new System.Drawing.Size(705, 366);
            this.tabPage5.TabIndex = 1;
            this.tabPage5.Text = "属性设置";
            this.tabPage5.UseVisualStyleBackColor = true;
            // 
            // button_ApplyAttribute
            // 
            this.button_ApplyAttribute.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button_ApplyAttribute.Location = new System.Drawing.Point(566, 208);
            this.button_ApplyAttribute.Name = "button_ApplyAttribute";
            this.button_ApplyAttribute.Size = new System.Drawing.Size(76, 26);
            this.button_ApplyAttribute.TabIndex = 8;
            this.button_ApplyAttribute.Text = "应用";
            this.button_ApplyAttribute.UseVisualStyleBackColor = true;
            this.button_ApplyAttribute.Click += new System.EventHandler(this.button_ApplyAttribute_Click);
            // 
            // 剖面属性
            // 
            this.剖面属性.Controls.Add(this.textBox_SectionMark);
            this.剖面属性.Controls.Add(this.label7);
            this.剖面属性.Controls.Add(this.textBox_Section);
            this.剖面属性.Controls.Add(this.label9);
            this.剖面属性.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.剖面属性.Location = new System.Drawing.Point(435, 6);
            this.剖面属性.Name = "剖面属性";
            this.剖面属性.Size = new System.Drawing.Size(207, 125);
            this.剖面属性.TabIndex = 7;
            this.剖面属性.TabStop = false;
            this.剖面属性.Text = "剖面属性";
            // 
            // textBox_SectionMark
            // 
            this.textBox_SectionMark.Location = new System.Drawing.Point(76, 65);
            this.textBox_SectionMark.Name = "textBox_SectionMark";
            this.textBox_SectionMark.Size = new System.Drawing.Size(108, 21);
            this.textBox_SectionMark.TabIndex = 12;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(7, 68);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(65, 12);
            this.label7.TabIndex = 11;
            this.label7.Text = "剖视标记：";
            // 
            // textBox_Section
            // 
            this.textBox_Section.Location = new System.Drawing.Point(76, 28);
            this.textBox_Section.Name = "textBox_Section";
            this.textBox_Section.Size = new System.Drawing.Size(108, 21);
            this.textBox_Section.TabIndex = 10;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(7, 32);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(53, 12);
            this.label9.TabIndex = 1;
            this.label9.Text = "剖视图：";
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.textBox_Bolt);
            this.groupBox5.Controls.Add(this.label5);
            this.groupBox5.Controls.Add(this.textBox_GeneralPart);
            this.groupBox5.Controls.Add(this.label4);
            this.groupBox5.Controls.Add(this.textBox_MainPart);
            this.groupBox5.Controls.Add(this.label6);
            this.groupBox5.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.groupBox5.Location = new System.Drawing.Point(221, 6);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(198, 125);
            this.groupBox5.TabIndex = 5;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "标记属性";
            // 
            // textBox_Bolt
            // 
            this.textBox_Bolt.Location = new System.Drawing.Point(71, 92);
            this.textBox_Bolt.Name = "textBox_Bolt";
            this.textBox_Bolt.Size = new System.Drawing.Size(108, 21);
            this.textBox_Bolt.TabIndex = 5;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(7, 96);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(41, 12);
            this.label5.TabIndex = 4;
            this.label5.Text = "螺栓：";
            // 
            // textBox_GeneralPart
            // 
            this.textBox_GeneralPart.Location = new System.Drawing.Point(71, 61);
            this.textBox_GeneralPart.Name = "textBox_GeneralPart";
            this.textBox_GeneralPart.Size = new System.Drawing.Size(108, 21);
            this.textBox_GeneralPart.TabIndex = 3;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(7, 65);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(53, 12);
            this.label4.TabIndex = 2;
            this.label4.Text = "次部件：";
            // 
            // textBox_MainPart
            // 
            this.textBox_MainPart.Location = new System.Drawing.Point(71, 28);
            this.textBox_MainPart.Name = "textBox_MainPart";
            this.textBox_MainPart.Size = new System.Drawing.Size(108, 21);
            this.textBox_MainPart.TabIndex = 1;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(7, 32);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(53, 12);
            this.label6.TabIndex = 0;
            this.label6.Text = "主部件：";
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.textBox_Angle);
            this.groupBox4.Controls.Add(this.label8);
            this.groupBox4.Controls.Add(this.textBox_GeneralSize);
            this.groupBox4.Controls.Add(this.label2);
            this.groupBox4.Controls.Add(this.textBox_MainSize);
            this.groupBox4.Controls.Add(this.label3);
            this.groupBox4.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.groupBox4.Location = new System.Drawing.Point(6, 6);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(199, 125);
            this.groupBox4.TabIndex = 1;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "尺寸属性";
            // 
            // textBox_Angle
            // 
            this.textBox_Angle.Location = new System.Drawing.Point(71, 88);
            this.textBox_Angle.Name = "textBox_Angle";
            this.textBox_Angle.Size = new System.Drawing.Size(108, 21);
            this.textBox_Angle.TabIndex = 9;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(8, 92);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(41, 12);
            this.label8.TabIndex = 8;
            this.label8.Text = "角度：";
            // 
            // textBox_GeneralSize
            // 
            this.textBox_GeneralSize.Location = new System.Drawing.Point(71, 56);
            this.textBox_GeneralSize.Name = "textBox_GeneralSize";
            this.textBox_GeneralSize.Size = new System.Drawing.Size(108, 21);
            this.textBox_GeneralSize.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(7, 60);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 12);
            this.label2.TabIndex = 2;
            this.label2.Text = "分尺寸：";
            // 
            // textBox_MainSize
            // 
            this.textBox_MainSize.Location = new System.Drawing.Point(71, 25);
            this.textBox_MainSize.Name = "textBox_MainSize";
            this.textBox_MainSize.Size = new System.Drawing.Size(108, 21);
            this.textBox_MainSize.TabIndex = 1;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(7, 28);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 12);
            this.label3.TabIndex = 0;
            this.label3.Text = "总尺寸：";
            // 
            // tabPage8
            // 
            this.tabPage8.Controls.Add(this.comboBox_VerticalSection);
            this.tabPage8.Controls.Add(this.label22);
            this.tabPage8.Controls.Add(this.comboBox_SectionMark);
            this.tabPage8.Controls.Add(this.label21);
            this.tabPage8.Controls.Add(this.comboBox_HoritionalSection);
            this.tabPage8.Controls.Add(this.label20);
            this.tabPage8.Controls.Add(this.comboBox_AutoSectionSetting);
            this.tabPage8.Controls.Add(this.label19);
            this.tabPage8.Controls.Add(this.textBox_SectionDepth);
            this.tabPage8.Controls.Add(this.label18);
            this.tabPage8.Location = new System.Drawing.Point(4, 22);
            this.tabPage8.Name = "tabPage8";
            this.tabPage8.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage8.Size = new System.Drawing.Size(705, 366);
            this.tabPage8.TabIndex = 4;
            this.tabPage8.Text = "剖面设置";
            this.tabPage8.UseVisualStyleBackColor = true;
            // 
            // comboBox_VerticalSection
            // 
            this.comboBox_VerticalSection.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_VerticalSection.FormattingEnabled = true;
            this.comboBox_VerticalSection.Items.AddRange(new object[] {
            "向下",
            "向上"});
            this.comboBox_VerticalSection.Location = new System.Drawing.Point(107, 157);
            this.comboBox_VerticalSection.Name = "comboBox_VerticalSection";
            this.comboBox_VerticalSection.Size = new System.Drawing.Size(100, 20);
            this.comboBox_VerticalSection.TabIndex = 10;
            this.comboBox_VerticalSection.SelectionChangeCommitted += new System.EventHandler(this.comboBox_VerticalSection_SelectionChangeCommitted);
            // 
            // label22
            // 
            this.label22.AutoSize = true;
            this.label22.Location = new System.Drawing.Point(12, 160);
            this.label22.Name = "label22";
            this.label22.Size = new System.Drawing.Size(89, 12);
            this.label22.TabIndex = 9;
            this.label22.Text = "剖面竖直方向：";
            // 
            // comboBox_SectionMark
            // 
            this.comboBox_SectionMark.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_SectionMark.FormattingEnabled = true;
            this.comboBox_SectionMark.Items.AddRange(new object[] {
            "仅一次",
            "全部都"});
            this.comboBox_SectionMark.Location = new System.Drawing.Point(107, 90);
            this.comboBox_SectionMark.Name = "comboBox_SectionMark";
            this.comboBox_SectionMark.Size = new System.Drawing.Size(100, 20);
            this.comboBox_SectionMark.TabIndex = 8;
            this.comboBox_SectionMark.SelectionChangeCommitted += new System.EventHandler(this.comboBox_SectionMark_SelectionChangeCommitted);
            // 
            // label21
            // 
            this.label21.AutoSize = true;
            this.label21.Location = new System.Drawing.Point(22, 93);
            this.label21.Name = "label21";
            this.label21.Size = new System.Drawing.Size(65, 12);
            this.label21.TabIndex = 7;
            this.label21.Text = "剖面符号：";
            // 
            // comboBox_HoritionalSection
            // 
            this.comboBox_HoritionalSection.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_HoritionalSection.FormattingEnabled = true;
            this.comboBox_HoritionalSection.Items.AddRange(new object[] {
            "向右",
            "向左"});
            this.comboBox_HoritionalSection.Location = new System.Drawing.Point(107, 123);
            this.comboBox_HoritionalSection.Name = "comboBox_HoritionalSection";
            this.comboBox_HoritionalSection.Size = new System.Drawing.Size(100, 20);
            this.comboBox_HoritionalSection.TabIndex = 6;
            this.comboBox_HoritionalSection.SelectionChangeCommitted += new System.EventHandler(this.comboBox_HoritionalSection_SelectionChangeCommitted);
            // 
            // label20
            // 
            this.label20.AutoSize = true;
            this.label20.Location = new System.Drawing.Point(12, 127);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(89, 12);
            this.label20.TabIndex = 5;
            this.label20.Text = "剖面水平方向：";
            // 
            // comboBox_AutoSectionSetting
            // 
            this.comboBox_AutoSectionSetting.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_AutoSectionSetting.FormattingEnabled = true;
            this.comboBox_AutoSectionSetting.Items.AddRange(new object[] {
            "无",
            "一键标注",
            "列表标注",
            "两者都"});
            this.comboBox_AutoSectionSetting.Location = new System.Drawing.Point(107, 54);
            this.comboBox_AutoSectionSetting.Name = "comboBox_AutoSectionSetting";
            this.comboBox_AutoSectionSetting.Size = new System.Drawing.Size(100, 20);
            this.comboBox_AutoSectionSetting.TabIndex = 4;
            this.comboBox_AutoSectionSetting.SelectionChangeCommitted += new System.EventHandler(this.comboBox_AutoSectionSetting_SelectionChangeCommitted);
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Location = new System.Drawing.Point(21, 57);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(65, 12);
            this.label19.TabIndex = 3;
            this.label19.Text = "自动剖面：";
            // 
            // textBox_SectionDepth
            // 
            this.textBox_SectionDepth.Location = new System.Drawing.Point(107, 20);
            this.textBox_SectionDepth.Name = "textBox_SectionDepth";
            this.textBox_SectionDepth.Size = new System.Drawing.Size(100, 21);
            this.textBox_SectionDepth.TabIndex = 1;
            this.textBox_SectionDepth.TextChanged += new System.EventHandler(this.textBox_SectionDepth_TextChanged);
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Location = new System.Drawing.Point(20, 23);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(65, 12);
            this.label18.TabIndex = 0;
            this.label18.Text = "剖面深度：";
            // 
            // tabPage7
            // 
            this.tabPage7.Controls.Add(this.label_WholePos);
            this.tabPage7.Controls.Add(this.label_CurPos);
            this.tabPage7.Controls.Add(this.label1);
            this.tabPage7.Controls.Add(this.comboBoxDimType);
            this.tabPage7.Controls.Add(this.buttonListDimSetting);
            this.tabPage7.Controls.Add(this.progressBar_Whole);
            this.tabPage7.Controls.Add(this.progressBar_Current);
            this.tabPage7.Controls.Add(this.listView_Drawing);
            this.tabPage7.Controls.Add(this.buttonBeginDim);
            this.tabPage7.Controls.Add(this.buttonGetList);
            this.tabPage7.Controls.Add(this.buttonOneKeyDim);
            this.tabPage7.Controls.Add(this.buttonSectionView);
            this.tabPage7.Controls.Add(this.buttonFrontView);
            this.tabPage7.Controls.Add(this.buttonTopView);
            this.tabPage7.Location = new System.Drawing.Point(4, 22);
            this.tabPage7.Name = "tabPage7";
            this.tabPage7.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage7.Size = new System.Drawing.Size(705, 366);
            this.tabPage7.TabIndex = 3;
            this.tabPage7.Text = "图纸标注";
            this.tabPage7.UseVisualStyleBackColor = true;
            // 
            // label_WholePos
            // 
            this.label_WholePos.AutoSize = true;
            this.label_WholePos.Location = new System.Drawing.Point(604, 344);
            this.label_WholePos.Name = "label_WholePos";
            this.label_WholePos.Size = new System.Drawing.Size(53, 12);
            this.label_WholePos.TabIndex = 20;
            this.label_WholePos.Text = "总进度：";
            // 
            // label_CurPos
            // 
            this.label_CurPos.AutoSize = true;
            this.label_CurPos.Location = new System.Drawing.Point(597, 323);
            this.label_CurPos.Name = "label_CurPos";
            this.label_CurPos.Size = new System.Drawing.Size(65, 12);
            this.label_CurPos.TabIndex = 19;
            this.label_CurPos.Text = "当前进度：";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 14);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 12);
            this.label1.TabIndex = 18;
            this.label1.Text = "类型：";
            // 
            // comboBoxDimType
            // 
            this.comboBoxDimType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxDimType.FormattingEnabled = true;
            this.comboBoxDimType.Items.AddRange(new object[] {
            "梁标注",
            "柱标注"});
            this.comboBoxDimType.Location = new System.Drawing.Point(50, 10);
            this.comboBoxDimType.Name = "comboBoxDimType";
            this.comboBoxDimType.Size = new System.Drawing.Size(80, 20);
            this.comboBoxDimType.TabIndex = 17;
            this.comboBoxDimType.SelectedValueChanged += new System.EventHandler(this.comboBoxDimType_SelectedValueChanged);
            // 
            // buttonListDimSetting
            // 
            this.buttonListDimSetting.Location = new System.Drawing.Point(537, 9);
            this.buttonListDimSetting.Name = "buttonListDimSetting";
            this.buttonListDimSetting.Size = new System.Drawing.Size(74, 23);
            this.buttonListDimSetting.TabIndex = 15;
            this.buttonListDimSetting.Text = "列表设置";
            this.buttonListDimSetting.UseVisualStyleBackColor = true;
            this.buttonListDimSetting.Click += new System.EventHandler(this.buttonListDimSetting_Click);
            // 
            // progressBar_Whole
            // 
            this.progressBar_Whole.Location = new System.Drawing.Point(6, 344);
            this.progressBar_Whole.Name = "progressBar_Whole";
            this.progressBar_Whole.Size = new System.Drawing.Size(576, 13);
            this.progressBar_Whole.TabIndex = 12;
            // 
            // progressBar_Current
            // 
            this.progressBar_Current.Location = new System.Drawing.Point(6, 323);
            this.progressBar_Current.Name = "progressBar_Current";
            this.progressBar_Current.Size = new System.Drawing.Size(576, 13);
            this.progressBar_Current.TabIndex = 11;
            // 
            // listView_Drawing
            // 
            this.listView_Drawing.ContextMenuStrip = this.contextMenuStrip1;
            this.listView_Drawing.FullRowSelect = true;
            this.listView_Drawing.Location = new System.Drawing.Point(7, 39);
            this.listView_Drawing.Name = "listView_Drawing";
            this.listView_Drawing.Size = new System.Drawing.Size(681, 278);
            this.listView_Drawing.TabIndex = 10;
            this.listView_Drawing.UseCompatibleStateImageBehavior = false;
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.DeleteToolStripMenuItem,
            this.ClearToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(101, 48);
            // 
            // DeleteToolStripMenuItem
            // 
            this.DeleteToolStripMenuItem.Name = "DeleteToolStripMenuItem";
            this.DeleteToolStripMenuItem.Size = new System.Drawing.Size(100, 22);
            this.DeleteToolStripMenuItem.Text = "删除";
            this.DeleteToolStripMenuItem.Click += new System.EventHandler(this.DeleteToolStripMenuItem_Click);
            // 
            // ClearToolStripMenuItem
            // 
            this.ClearToolStripMenuItem.Name = "ClearToolStripMenuItem";
            this.ClearToolStripMenuItem.Size = new System.Drawing.Size(100, 22);
            this.ClearToolStripMenuItem.Text = "清空";
            this.ClearToolStripMenuItem.Click += new System.EventHandler(this.ClearToolStripMenuItem_Click);
            // 
            // buttonBeginDim
            // 
            this.buttonBeginDim.Location = new System.Drawing.Point(617, 9);
            this.buttonBeginDim.Name = "buttonBeginDim";
            this.buttonBeginDim.Size = new System.Drawing.Size(73, 23);
            this.buttonBeginDim.TabIndex = 9;
            this.buttonBeginDim.Text = "开始标注";
            this.buttonBeginDim.UseVisualStyleBackColor = true;
            this.buttonBeginDim.Click += new System.EventHandler(this.buttonBeginDim_Click);
            // 
            // buttonGetList
            // 
            this.buttonGetList.Location = new System.Drawing.Point(456, 9);
            this.buttonGetList.Name = "buttonGetList";
            this.buttonGetList.Size = new System.Drawing.Size(75, 23);
            this.buttonGetList.TabIndex = 8;
            this.buttonGetList.Text = "获取列表";
            this.buttonGetList.UseVisualStyleBackColor = true;
            this.buttonGetList.Click += new System.EventHandler(this.buttonGetList_Click);
            // 
            // buttonOneKeyDim
            // 
            this.buttonOneKeyDim.Location = new System.Drawing.Point(375, 9);
            this.buttonOneKeyDim.Name = "buttonOneKeyDim";
            this.buttonOneKeyDim.Size = new System.Drawing.Size(75, 23);
            this.buttonOneKeyDim.TabIndex = 7;
            this.buttonOneKeyDim.Text = "一键标注";
            this.buttonOneKeyDim.UseVisualStyleBackColor = true;
            this.buttonOneKeyDim.Click += new System.EventHandler(this.buttonOneKeyDim_Click);
            // 
            // buttonSectionView
            // 
            this.buttonSectionView.Location = new System.Drawing.Point(293, 9);
            this.buttonSectionView.Name = "buttonSectionView";
            this.buttonSectionView.Size = new System.Drawing.Size(76, 23);
            this.buttonSectionView.TabIndex = 6;
            this.buttonSectionView.Text = "剖视图";
            this.buttonSectionView.UseVisualStyleBackColor = true;
            this.buttonSectionView.Click += new System.EventHandler(this.buttonSectionView_Click);
            // 
            // buttonFrontView
            // 
            this.buttonFrontView.Location = new System.Drawing.Point(212, 9);
            this.buttonFrontView.Name = "buttonFrontView";
            this.buttonFrontView.Size = new System.Drawing.Size(75, 23);
            this.buttonFrontView.TabIndex = 5;
            this.buttonFrontView.Text = "前视图";
            this.buttonFrontView.UseVisualStyleBackColor = true;
            this.buttonFrontView.Click += new System.EventHandler(this.buttonFrontView_Click);
            // 
            // buttonTopView
            // 
            this.buttonTopView.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.buttonTopView.Location = new System.Drawing.Point(138, 9);
            this.buttonTopView.Name = "buttonTopView";
            this.buttonTopView.Size = new System.Drawing.Size(68, 23);
            this.buttonTopView.TabIndex = 4;
            this.buttonTopView.Text = "顶视图";
            this.buttonTopView.UseVisualStyleBackColor = true;
            this.buttonTopView.Click += new System.EventHandler(this.buttonTopView_Click);
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.tabControl3);
            this.tabPage2.Location = new System.Drawing.Point(4, 24);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(716, 393);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "门刚结构";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // tabControl3
            // 
            this.tabControl3.Controls.Add(this.tabPage6);
            this.tabControl3.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.tabControl3.Location = new System.Drawing.Point(0, -1);
            this.tabControl3.Name = "tabControl3";
            this.tabControl3.SelectedIndex = 0;
            this.tabControl3.Size = new System.Drawing.Size(720, 393);
            this.tabControl3.TabIndex = 0;
            // 
            // tabPage6
            // 
            this.tabPage6.Controls.Add(this.label10);
            this.tabPage6.Controls.Add(this.comboBoxDimTypeDoor);
            this.tabPage6.Controls.Add(this.buttonListDimSettingDoor);
            this.tabPage6.Controls.Add(this.listView_DrawingDoor);
            this.tabPage6.Controls.Add(this.buttonBeginDimDoor);
            this.tabPage6.Controls.Add(this.button_GetListDoor);
            this.tabPage6.Controls.Add(this.buttonOneKeyDimDoor);
            this.tabPage6.Controls.Add(this.buttonSectionViewDoor);
            this.tabPage6.Controls.Add(this.buttonFrontViewDoor);
            this.tabPage6.Controls.Add(this.buttonTopViewDoor);
            this.tabPage6.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.tabPage6.Location = new System.Drawing.Point(4, 22);
            this.tabPage6.Name = "tabPage6";
            this.tabPage6.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage6.Size = new System.Drawing.Size(712, 367);
            this.tabPage6.TabIndex = 0;
            this.tabPage6.Text = "图纸标注";
            this.tabPage6.UseVisualStyleBackColor = true;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(9, 13);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(41, 12);
            this.label10.TabIndex = 28;
            this.label10.Text = "类型：";
            // 
            // comboBoxDimTypeDoor
            // 
            this.comboBoxDimTypeDoor.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxDimTypeDoor.FormattingEnabled = true;
            this.comboBoxDimTypeDoor.Items.AddRange(new object[] {
            "梁标注",
            "柱标注"});
            this.comboBoxDimTypeDoor.Location = new System.Drawing.Point(53, 9);
            this.comboBoxDimTypeDoor.Name = "comboBoxDimTypeDoor";
            this.comboBoxDimTypeDoor.Size = new System.Drawing.Size(80, 20);
            this.comboBoxDimTypeDoor.TabIndex = 27;
            this.comboBoxDimTypeDoor.SelectedValueChanged += new System.EventHandler(this.comboBoxDimTypeDoor_SelectedValueChanged);
            // 
            // buttonListDimSettingDoor
            // 
            this.buttonListDimSettingDoor.Location = new System.Drawing.Point(540, 8);
            this.buttonListDimSettingDoor.Name = "buttonListDimSettingDoor";
            this.buttonListDimSettingDoor.Size = new System.Drawing.Size(74, 23);
            this.buttonListDimSettingDoor.TabIndex = 26;
            this.buttonListDimSettingDoor.Text = "列表设置";
            this.buttonListDimSettingDoor.UseVisualStyleBackColor = true;
            this.buttonListDimSettingDoor.Click += new System.EventHandler(this.buttonListDimSettingDoor_Click);
            // 
            // listView_DrawingDoor
            // 
            this.listView_DrawingDoor.ContextMenuStrip = this.contextMenuStrip1;
            this.listView_DrawingDoor.FullRowSelect = true;
            this.listView_DrawingDoor.Location = new System.Drawing.Point(10, 38);
            this.listView_DrawingDoor.Name = "listView_DrawingDoor";
            this.listView_DrawingDoor.Size = new System.Drawing.Size(681, 321);
            this.listView_DrawingDoor.TabIndex = 25;
            this.listView_DrawingDoor.UseCompatibleStateImageBehavior = false;
            // 
            // buttonBeginDimDoor
            // 
            this.buttonBeginDimDoor.Location = new System.Drawing.Point(620, 8);
            this.buttonBeginDimDoor.Name = "buttonBeginDimDoor";
            this.buttonBeginDimDoor.Size = new System.Drawing.Size(73, 23);
            this.buttonBeginDimDoor.TabIndex = 24;
            this.buttonBeginDimDoor.Text = "开始标注";
            this.buttonBeginDimDoor.UseVisualStyleBackColor = true;
            this.buttonBeginDimDoor.Click += new System.EventHandler(this.buttonBeginDimDoor_Click);
            // 
            // button_GetListDoor
            // 
            this.button_GetListDoor.Location = new System.Drawing.Point(459, 8);
            this.button_GetListDoor.Name = "button_GetListDoor";
            this.button_GetListDoor.Size = new System.Drawing.Size(75, 23);
            this.button_GetListDoor.TabIndex = 23;
            this.button_GetListDoor.Text = "获取列表";
            this.button_GetListDoor.UseVisualStyleBackColor = true;
            this.button_GetListDoor.Click += new System.EventHandler(this.button_GetListDoor_Click);
            // 
            // buttonOneKeyDimDoor
            // 
            this.buttonOneKeyDimDoor.Location = new System.Drawing.Point(378, 8);
            this.buttonOneKeyDimDoor.Name = "buttonOneKeyDimDoor";
            this.buttonOneKeyDimDoor.Size = new System.Drawing.Size(75, 23);
            this.buttonOneKeyDimDoor.TabIndex = 22;
            this.buttonOneKeyDimDoor.Text = "一键标注";
            this.buttonOneKeyDimDoor.UseVisualStyleBackColor = true;
            this.buttonOneKeyDimDoor.Click += new System.EventHandler(this.buttonOneKeyDimDoor_Click);
            // 
            // buttonSectionViewDoor
            // 
            this.buttonSectionViewDoor.Location = new System.Drawing.Point(296, 8);
            this.buttonSectionViewDoor.Name = "buttonSectionViewDoor";
            this.buttonSectionViewDoor.Size = new System.Drawing.Size(76, 23);
            this.buttonSectionViewDoor.TabIndex = 21;
            this.buttonSectionViewDoor.Text = "剖视图";
            this.buttonSectionViewDoor.UseVisualStyleBackColor = true;
            this.buttonSectionViewDoor.Click += new System.EventHandler(this.buttonSectionViewDoor_Click);
            // 
            // buttonFrontViewDoor
            // 
            this.buttonFrontViewDoor.Location = new System.Drawing.Point(215, 8);
            this.buttonFrontViewDoor.Name = "buttonFrontViewDoor";
            this.buttonFrontViewDoor.Size = new System.Drawing.Size(75, 23);
            this.buttonFrontViewDoor.TabIndex = 20;
            this.buttonFrontViewDoor.Text = "前视图";
            this.buttonFrontViewDoor.UseVisualStyleBackColor = true;
            this.buttonFrontViewDoor.Click += new System.EventHandler(this.buttonFrontViewDoor_Click);
            // 
            // buttonTopViewDoor
            // 
            this.buttonTopViewDoor.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.buttonTopViewDoor.Location = new System.Drawing.Point(141, 8);
            this.buttonTopViewDoor.Name = "buttonTopViewDoor";
            this.buttonTopViewDoor.Size = new System.Drawing.Size(68, 23);
            this.buttonTopViewDoor.TabIndex = 19;
            this.buttonTopViewDoor.Text = "顶视图";
            this.buttonTopViewDoor.UseVisualStyleBackColor = true;
            this.buttonTopViewDoor.Click += new System.EventHandler(this.buttonTopViewDoor_Click);
            // 
            // tabPage3
            // 
            this.tabPage3.Location = new System.Drawing.Point(4, 24);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage3.Size = new System.Drawing.Size(716, 393);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "关于";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // statusStrip1
            // 
            this.statusStrip1.Location = new System.Drawing.Point(0, 428);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(726, 22);
            this.statusStrip1.TabIndex = 1;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // checkBoxTopShow
            // 
            this.checkBoxTopShow.AutoSize = true;
            this.checkBoxTopShow.Location = new System.Drawing.Point(630, 5);
            this.checkBoxTopShow.Name = "checkBoxTopShow";
            this.checkBoxTopShow.Size = new System.Drawing.Size(72, 16);
            this.checkBoxTopShow.TabIndex = 2;
            this.checkBoxTopShow.Text = "顶端显示";
            this.checkBoxTopShow.UseVisualStyleBackColor = true;
            this.checkBoxTopShow.CheckedChanged += new System.EventHandler(this.checkBoxTopShow_CheckedChanged);
            // 
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // NewMainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(726, 450);
            this.Controls.Add(this.checkBoxTopShow);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.tabControl1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "NewMainForm";
            this.Text = "Tekla智能标注系统V2.0-扬州石头哥";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.NewMainForm_FormClosed);
            this.Load += new System.EventHandler(this.NewMainForm_Load);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabControl2.ResumeLayout(false);
            this.tabPage4.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewBeam)).EndInit();
            this.tabPage9.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox6.ResumeLayout(false);
            this.groupBox6.PerformLayout();
            this.tabPage5.ResumeLayout(false);
            this.剖面属性.ResumeLayout(false);
            this.剖面属性.PerformLayout();
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.tabPage8.ResumeLayout(false);
            this.tabPage8.PerformLayout();
            this.tabPage7.ResumeLayout(false);
            this.tabPage7.PerformLayout();
            this.contextMenuStrip1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.tabControl3.ResumeLayout(false);
            this.tabPage6.ResumeLayout(false);
            this.tabPage6.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.CheckBox checkBoxTopShow;
        private System.Windows.Forms.TabControl tabControl2;
        private System.Windows.Forms.TabPage tabPage4;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TreeView treeViewBeam;
        private System.Windows.Forms.DataGridView dataGridViewBeam;
        private System.Windows.Forms.TabPage tabPage9;
        private System.Windows.Forms.TabPage tabPage5;
        private System.Windows.Forms.Button button_ApplyAttribute;
        private System.Windows.Forms.GroupBox 剖面属性;
        private System.Windows.Forms.TextBox textBox_SectionMark;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox textBox_Section;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.TextBox textBox_Bolt;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox textBox_GeneralPart;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textBox_MainPart;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.TextBox textBox_Angle;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox textBox_GeneralSize;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox_MainSize;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TabPage tabPage8;
        private System.Windows.Forms.TabPage tabPage7;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox comboBoxDimType;
        private System.Windows.Forms.Button buttonListDimSetting;
        private System.Windows.Forms.ProgressBar progressBar_Whole;
        private System.Windows.Forms.ProgressBar progressBar_Current;
        private System.Windows.Forms.ListView listView_Drawing;
        private System.Windows.Forms.Button buttonBeginDim;
        private System.Windows.Forms.Button buttonGetList;
        private System.Windows.Forms.Button buttonOneKeyDim;
        private System.Windows.Forms.Button buttonSectionView;
        private System.Windows.Forms.Button buttonFrontView;
        private System.Windows.Forms.Button buttonTopView;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column1;
        private System.Windows.Forms.DataGridViewCheckBoxColumn Column2;
        private System.Windows.Forms.DataGridViewCheckBoxColumn Column3;
        private System.Windows.Forms.Label label_WholePos;
        private System.Windows.Forms.Label label_CurPos;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem DeleteToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ClearToolStripMenuItem;
        private System.Windows.Forms.Button button_SaveDimSetting;
        private System.Windows.Forms.Button button_ReadDimSetting;
        private System.Windows.Forms.ComboBox comboBox_DimSetting;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.TabControl tabControl3;
        private System.Windows.Forms.TabPage tabPage6;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.ComboBox comboBoxDimTypeDoor;
        private System.Windows.Forms.Button buttonListDimSettingDoor;
        private System.Windows.Forms.ListView listView_DrawingDoor;
        private System.Windows.Forms.Button buttonBeginDimDoor;
        private System.Windows.Forms.Button button_GetListDoor;
        private System.Windows.Forms.Button buttonOneKeyDimDoor;
        private System.Windows.Forms.Button buttonSectionViewDoor;
        private System.Windows.Forms.Button buttonFrontViewDoor;
        private System.Windows.Forms.Button buttonTopViewDoor;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TextBox textBox_PartCombineDistance;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.TextBox textBox_DefaultDimDistanceThreshold;
        private System.Windows.Forms.TextBox textBox_BoltCombineDistance;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox textBox_DefaultDimDistance;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.TextBox textBox_TwoDimLineGap;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.GroupBox groupBox6;
        private System.Windows.Forms.ComboBox comboBox_FrontViewDimSetting;
        private System.Windows.Forms.ComboBox comboBox_TopViewDimSetting;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.ComboBox comboBox_AutoSectionSetting;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.TextBox textBox_SectionDepth;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.ComboBox comboBox_SectionMark;
        private System.Windows.Forms.Label label21;
        private System.Windows.Forms.ComboBox comboBox_HoritionalSection;
        private System.Windows.Forms.Label label20;
        private System.Windows.Forms.ComboBox comboBox_VerticalSection;
        private System.Windows.Forms.Label label22;
    }
}

