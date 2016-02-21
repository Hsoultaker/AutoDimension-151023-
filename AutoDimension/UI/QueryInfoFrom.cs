using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using AutoDimension.Entity;

using Tekla.Structures.Drawing;
using Tekla.Structures.Drawing.UI;
using Tekla.Structures;

using TSD = Tekla.Structures.Drawing;
using TSM = Tekla.Structures.Model;

namespace AutoDimension.UI
{
    public partial class QueryInfoFrom : Form
    {
        public QueryInfoFrom()
        {
            InitializeComponent();
        }

        private void PickObjectInDrawing_Click(object sender, EventArgs e)
        {
            Picker picker = new DrawingHandler().GetPicker();
            
            DrawingObject pickedObject = null;
            TSD.ViewBase viewBase = null;

            try
            {
               picker.PickObject("选择视图中的一个对象", out pickedObject, out viewBase);

               ShowDrawingObjectInfo(pickedObject, viewBase);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }

        /// <summary>
        /// 显示拾取对象的信息;
        /// </summary>
        /// <param name="pickedObject"></param>
        private void ShowDrawingObjectInfo(DrawingObject pickedObject,TSD.ViewBase viewBase)
        {
            if(null==pickedObject)
            {
                return;
            }
            if(pickedObject is TSD.ModelObject)
            {
                TSD.ModelObject modelObjectInDrawing = pickedObject as TSD.ModelObject;
                TSM.ModelObject modelObject = CDimTools.GetInstance().TransformDrawingToModel(modelObjectInDrawing);

                if(modelObject is TSM.Part)
                {
                    TSM.Part partInModel = modelObject as TSM.Part;
                    TSD.Part partInDrawing = modelObjectInDrawing as TSD.Part;

                    CMrPart mrPart = new CMrPart(partInModel, partInDrawing);

                    CDimTools.GetInstance().InitMrPart(partInModel,viewBase as TSD.View,mrPart);

                    UpdateUIInfo(mrPart);
                }
            }
        }

        /// <summary>
        /// 零件信息显示到界面上;
        /// </summary>
        /// <param name="mrPart"></param>
        private void UpdateUIInfo(CMrPart mrPart)
        {
            modelObjectTextBox.Text="";

            modelObjectTextBox.Text = mrPart.mBeamType.ToString() + Environment.NewLine +
                "Normal" + mrPart.mNormal.ToString() + Environment.NewLine +
                "LeftTop" + mrPart.mLeftTopPoint.ToString() + Environment.NewLine +
                "LeftBottom" + mrPart.mLeftBottomPoint.ToString() + Environment.NewLine +
                "RightTop" + mrPart.mRightTopPoint.ToString() + Environment.NewLine +
                "RightBottom"+mrPart.mRightBottomPoint.ToString();

        }
    }
}
