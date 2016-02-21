namespace AutoDimension.UI
{
    partial class DimSettingForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DimSettingForm));
            this.dataGridView_DrawingMark = new System.Windows.Forms.DataGridView();
            this.图纸标记 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.标注类型 = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.button_OK = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView_DrawingMark)).BeginInit();
            this.SuspendLayout();
            // 
            // dataGridView_DrawingMark
            // 
            this.dataGridView_DrawingMark.AllowUserToAddRows = false;
            this.dataGridView_DrawingMark.AllowUserToDeleteRows = false;
            this.dataGridView_DrawingMark.BackgroundColor = System.Drawing.SystemColors.Control;
            this.dataGridView_DrawingMark.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView_DrawingMark.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.图纸标记,
            this.标注类型});
            this.dataGridView_DrawingMark.Location = new System.Drawing.Point(-1, -1);
            this.dataGridView_DrawingMark.Name = "dataGridView_DrawingMark";
            this.dataGridView_DrawingMark.RowHeadersWidth = 70;
            this.dataGridView_DrawingMark.RowTemplate.Height = 23;
            this.dataGridView_DrawingMark.Size = new System.Drawing.Size(274, 233);
            this.dataGridView_DrawingMark.TabIndex = 0;
            // 
            // 图纸标记
            // 
            this.图纸标记.HeaderText = "图纸标记";
            this.图纸标记.Name = "图纸标记";
            this.图纸标记.ReadOnly = true;
            // 
            // 标注类型
            // 
            this.标注类型.HeaderText = "标注类型";
            this.标注类型.Items.AddRange(new object[] {
            "梁标注",
            "柱标注"});
            this.标注类型.Name = "标注类型";
            // 
            // button_OK
            // 
            this.button_OK.Location = new System.Drawing.Point(195, 242);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(75, 23);
            this.button_OK.TabIndex = 1;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // DimSettingForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(275, 271);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.dataGridView_DrawingMark);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "DimSettingForm";
            this.Text = "标注设置";
            this.Load += new System.EventHandler(this.DimSettingForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView_DrawingMark)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dataGridView_DrawingMark;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.DataGridViewTextBoxColumn 图纸标记;
        private System.Windows.Forms.DataGridViewComboBoxColumn 标注类型;

    }
}