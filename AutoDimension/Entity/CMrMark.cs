using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Tekla.Structures.Drawing;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model;

using TSM = Tekla.Structures.Model;
using TSD = Tekla.Structures.Drawing;

namespace AutoDimension.Entity
{
    /// <summary>
    /// 标记对象;
    /// </summary>
    public class CMrMark
    {
        /// <summary>
        /// 模型对象;
        /// </summary>
        public TSD.ModelObject mModelObject=null;

        /// <summary>
        /// 零件标记的插入点;
        /// </summary>
        public Point mInsertPoint = null;

        /// <summary>
        /// 零件标记的中文字的点;
        /// </summary>
        public Point mTextPoint=null;

        /// <summary>
        /// 标记文字的包围盒;
        /// </summary>
        public RectangleBoundingBox mTextBoundingBox=null;

        /// <summary>
        /// 零件标记的构造函数;
        /// </summary>
        public CMrMark()
        {
            mModelObject = null;
            mInsertPoint = new Point(0, 0, 0);
            mTextPoint = new Point(0, 0, 0);
        }
    }
}
