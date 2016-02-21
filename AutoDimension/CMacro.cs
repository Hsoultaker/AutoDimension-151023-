// using System;
// using System.Collections.Generic;
// using System.Collections;
// using System.ComponentModel;
// using System.Data;
// using System.Drawing;
// using System.Linq;
// using System.Text;
// using System.Windows.Forms;
// using Tekla.Structures;
// using Tekla.Structures.Filtering;
// using Tekla.Structures.Filtering.Categories;
// using Tekla.Structures.Model;
// using TSM = Tekla.Structures.Model;
// using TSG = Tekla.Structures.Geometry3d;
// using System.IO;
// using Tekla.Technology.Akit.UserScript;
// using Tekla.Technology.Scripting;
// using System.Diagnostics;
// 
// namespace Tekla.Technology.Akit.UserScript
// {
//     partial class CADSUPPORT_COPY_WITH_BASE_POINT_FORM : System.Windows.Forms.Form
//     {
//         private System.Windows.Forms.Button button1;
//         private System.Windows.Forms.Button button2;
//         private System.Windows.Forms.LinkLabel linkLabel1;
//         private System.ComponentModel.IContainer components = null;
//         protected override void Dispose(bool disposing)
//         {
//             if (disposing && (components != null))
//             {
//                 components.Dispose();
//             }
//             base.Dispose(disposing);
//         }
//         private void InitializeComponent()
//         {
//             this.button1 = new System.Windows.Forms.Button();
//             this.button2 = new System.Windows.Forms.Button();
//             this.linkLabel1 = new System.Windows.Forms.LinkLabel();
//             this.SuspendLayout();
//             // 
//             // button1
//             // 
//             this.button1.Location = new System.Drawing.Point(10, 10);
//             this.button1.Name = "button1";
//             this.button1.Size = new System.Drawing.Size(100, 40);
//             this.button1.TabIndex = 0;
//             this.button1.Text = "COPY WITH BASE POINT";
//             this.button1.UseVisualStyleBackColor = true;
//             this.button1.Click += new System.EventHandler(this.button1_Click);
//             // 
//             // button2
//             // 
//             this.button2.Location = new System.Drawing.Point(120, 10);
//             this.button2.Name = "button2";
//             this.button2.Size = new System.Drawing.Size(100, 40);
//             this.button2.TabIndex = 1;
//             this.button2.Text = "PASTE";
//             this.button2.UseVisualStyleBackColor = true;
//             this.button2.Click += new System.EventHandler(this.button2_Click);
//             this.button2.Enabled = false;
//             // 
//             // linkLabel1
//             // 
//             this.linkLabel1.AutoSize = true;
//             this.linkLabel1.Location = new System.Drawing.Point(120, 60);
//             this.linkLabel1.Name = "linkLabel1";
//             this.linkLabel1.Size = new System.Drawing.Size(100, 20);
//             this.linkLabel1.TabIndex = 5;
//             this.linkLabel1.TabStop = true;
//             this.linkLabel1.Text = "http://cadsupport.ru";
//             this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
//             // 
//             // Form1
//             // 
//             this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
//             this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
//             this.ClientSize = new System.Drawing.Size(230, 80);
//             this.Controls.Add(this.linkLabel1);
//             this.Controls.Add(this.button2);
//             this.Controls.Add(this.button1);
//             this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
//             this.Name = "Form1";
//             this.Text = "CADSUPPORT_COPY_WITH_BASE_POINT";
//             this.TopMost = true;
//             this.ResumeLayout(false);
//             this.PerformLayout();
//         }
//         Model M = new Model();
//         String sysPath = "";
//         private static int _TempFileIndex = -1;
//         private static readonly Random Random = new Random();
//         private const int MaxTempFiles = 32;
//         private const string FileNameFormat = "macro_{0:00}.cs";
//         Tekla.Structures.Model.ModelObjectEnumerator Copy_enum = null;
//         TSG.Point copy_point = null;
//         public CADSUPPORT_COPY_WITH_BASE_POINT_FORM()
//         {
//             InitializeComponent();
//             sysPath = M.GetInfo().ModelPath;
//             sysPath = Path.Combine(sysPath, "attributes");
//             sysPath = Path.Combine(sysPath, "AF.SObjGrp");
//         }
//         private string GetMacroFileName()
//         {
//             lock (Random)
//             {
//                 if (_TempFileIndex < 0)
//                 {
//                     _TempFileIndex = Random.Next(0, MaxTempFiles);
//                 }
//                 else
//                 {
//                     _TempFileIndex = (_TempFileIndex + 1) % MaxTempFiles;
//                 }
// 
//                 return string.Format(FileNameFormat, _TempFileIndex);
//             }
//         }
//         private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
//         {
//             System.Diagnostics.Process.Start("http://cadsupport.ru");
//         }
//         private void button1_Click(object sender, EventArgs e)//copy
//         {
//             Copy_enum = new TSM.UI.ModelObjectSelector().GetSelectedObjects();
//             Tekla.Structures.Model.UI.Picker Picker = new Tekla.Structures.Model.UI.Picker();
//             System.Collections.ArrayList pointss;
//             try
//             {
//                 pointss = Picker.PickPoints(Tekla.Structures.Model.UI.Picker.PickPointEnum.PICK_ONE_POINT);
//                 copy_point = pointss[0] as Tekla.Structures.Geometry3d.Point;
//                 this.button2.Enabled = true;
//             }
//             catch { };
//         }
//         private void button2_Click(object sender, EventArgs e)//paste
//         {
//             while (true)
//             {
//                 Tekla.Structures.Model.UI.Picker Picker = new Tekla.Structures.Model.UI.Picker();
//                 System.Collections.ArrayList pointss;
//                 TSG.Point paste_point = new TSG.Point();
//                 try
//                 {
//                     pointss = Picker.PickPoints(Tekla.Structures.Model.UI.Picker.PickPointEnum.PICK_ONE_POINT);
//                     paste_point = pointss[0] as Tekla.Structures.Geometry3d.Point;
//                 }
//                 catch { return; };
// 
// 
//                 string Name = GetMacroFileName();
//                 string MacrosPath = string.Empty;
//                 Tekla.Structures.TeklaStructuresSettings.GetAdvancedOption("XS_MACRO_DIRECTORY", ref MacrosPath);
//                 if (MacrosPath.IndexOf(';') > 0) { MacrosPath = MacrosPath.Remove(MacrosPath.IndexOf(';')); }
// 
//                 string name = (sender as Button).Text;
// 
//                 TSM.UI.ModelObjectSelector MS = new TSM.UI.ModelObjectSelector();
// 
//                 System.Collections.ArrayList copy_list = new System.Collections.ArrayList();
//                 Copy_enum.Reset();
// 
//                 while (Copy_enum.MoveNext())
//                 {
//                     copy_list.Add(Copy_enum.Current);
//                 }
//                 MS.Select(copy_list);
// 
// 
//                 double dx = paste_point.X - copy_point.X;
//                 double dy = paste_point.Y - copy_point.Y;
//                 double dz = paste_point.Z - copy_point.Z;
// 
//                 string script2 = @"namespace Tekla.Technology.Akit.UserScript
// {
//     public class Script
//     {
//         public static void Run(Tekla.Technology.Akit.IScript akit)
//         {
//             akit.CommandStart(""ail_copy_translate"", """", ""main_frame"");
//             akit.ValueChange(""Copy"", ""dx"", """ + dx.ToString("F5") + @""");
//             akit.ValueChange(""Copy"", ""dy"", """ + dy.ToString("F5") + @""");
//             akit.ValueChange(""Copy"", ""dz"", """ + dz.ToString("F5") + @""");
//             akit.PushButton(""dia_copy_apply"", ""Copy"");
//             akit.PushButton(""dia_copy_ok"", ""Copy"");
//         }
//     }
// }";
//                 File.WriteAllText(Path.Combine(MacrosPath, Name), script2);
//                 Tekla.Structures.Model.Operations.Operation.RunMacro("..\\" + Name);
//             }
//         }
//     }
// 
//     public class Script
//         {
//             public static void Run(Tekla.Technology.Akit.IScript akit)
//             {
//                 Application.EnableVisualStyles();
//                 Application.Run(new CADSUPPORT_COPY_WITH_BASE_POINT_FORM());
//             }
//         }
//     }
// 
