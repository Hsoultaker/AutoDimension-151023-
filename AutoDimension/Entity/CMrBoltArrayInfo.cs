using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Tekla.Structures.Drawing;
using Tekla.Structures.Geometry3d;


namespace AutoDimension.Entity
{
    /// <summary>
    /// 螺钉组的信息类,主要是判断螺钉组的对称性;
    /// </summary>
    public class CMrBoltArrayInfo
    {
        /// <summary>
        /// 关联的螺钉组;
        /// </summary>
        private CMrBoltArray mrBoltArray;

        /// <summary>
        /// 与该螺钉组上下对称的螺钉组;
        /// </summary>
        public CMrBoltArray mXSymBoltArray=null;

        /// <summary>
        /// 与该螺钉组左右对称的螺钉组;
        /// </summary>
        public CMrBoltArray mYSymBoltArray = null;

        /// <summary>
        /// 上侧标注;
        /// </summary>
        public bool mbIsUpDim = false;

        /// <summary>
        /// 下侧标注;
        /// </summary>
        public bool mbIsDownDim = false;

        /// <summary>
        /// 左侧标注;
        /// </summary>
        public bool mbIsLeftDim = false;

        /// <summary>
        /// 右侧标注;
        /// </summary>
        public bool mbIsRightDim = false;

        /// <summary>
        /// 构造函数;
        /// </summary>
        /// <param name="mrBoltArray"></param>
        public CMrBoltArrayInfo(CMrBoltArray mrBoltArray)
        {
            this.mrBoltArray = mrBoltArray;
        }        
    }
}
