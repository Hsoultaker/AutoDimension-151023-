namespace AutoDimension.UI
{
    partial class QueryInfoFrom
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.modelObjectTextBox = new System.Windows.Forms.TextBox();
            this.Quit = new System.Windows.Forms.Button();
            this.PickObjectInDrawing = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.modelObjectTextBox);
            this.groupBox1.Location = new System.Drawing.Point(7, 8);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(414, 376);
            this.groupBox1.TabIndex = 5;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Model object info";
            // 
            // modelObjectTextBox
            // 
            this.modelObjectTextBox.Location = new System.Drawing.Point(6, 18);
            this.modelObjectTextBox.Multiline = true;
            this.modelObjectTextBox.Name = "modelObjectTextBox";
            this.modelObjectTextBox.Size = new System.Drawing.Size(402, 353);
            this.modelObjectTextBox.TabIndex = 5;
            // 
            // Quit
            // 
            this.Quit.Location = new System.Drawing.Point(436, 74);
            this.Quit.Name = "Quit";
            this.Quit.Size = new System.Drawing.Size(132, 30);
            this.Quit.TabIndex = 7;
            this.Quit.Text = "Close";
            this.Quit.UseVisualStyleBackColor = true;
            // 
            // PickObjectInDrawing
            // 
            this.PickObjectInDrawing.Location = new System.Drawing.Point(436, 20);
            this.PickObjectInDrawing.Name = "PickObjectInDrawing";
            this.PickObjectInDrawing.Size = new System.Drawing.Size(132, 30);
            this.PickObjectInDrawing.TabIndex = 6;
            this.PickObjectInDrawing.Text = "Pick object in drawing";
            this.PickObjectInDrawing.UseVisualStyleBackColor = true;
            this.PickObjectInDrawing.Click += new System.EventHandler(this.PickObjectInDrawing_Click);
            // 
            // QueryInfoFrom
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(574, 392);
            this.Controls.Add(this.Quit);
            this.Controls.Add(this.PickObjectInDrawing);
            this.Controls.Add(this.groupBox1);
            this.Name = "QueryInfoFrom";
            this.Text = "QueryInfoFrom";
            this.TopMost = true;
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox modelObjectTextBox;
        private System.Windows.Forms.Button Quit;
        private System.Windows.Forms.Button PickObjectInDrawing;
    }
}