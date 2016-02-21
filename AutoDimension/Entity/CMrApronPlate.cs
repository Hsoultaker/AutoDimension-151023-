using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AutoDimension.Entity
{
    /// <summary>
    /// 檩托板对象,主要用在门式框架结构柱标注里面; 
    /// </summary>
    public class CMrApronPlate
    {
        /// <summary>
        /// 檩托板类型;
        /// </summary>
        public MrApronPlateType mType;

        /// <summary>
        /// 法向与Y轴平行的零部件;
        /// </summary>
        public CMrPart mYNormalPart=null;

        /// <summary>
        /// 法向与Z轴平行的零部件;
        /// </summary>
        public CMrPart mZNormalPart=null;

        /// <summary>
        /// 法向与X轴平行的零部件;
        /// </summary>
        public CMrPart mXNormalPart = null;

        /// <summary>
        /// 判断法向与Z轴平行的零部件是否在法向与Y轴平行的零部件上方;
        /// </summary>
        public bool mIsUp=false;

        /// <summary>
        /// 构造函数;
        /// </summary>
        /// <param name="yNormalPart"></param>
        /// <param name="zNormalPart"></param>
        public CMrApronPlate(CMrPart yNormalPart,CMrPart zNormalPart,MrApronPlateType type)
        {
            mType = type;

            if (mType == MrApronPlateType.Type1)
            {
                mYNormalPart = yNormalPart;
                mZNormalPart = zNormalPart;
            }
            if (mType == MrApronPlateType.Type2)
            {
                mXNormalPart = yNormalPart;
                mYNormalPart = zNormalPart;
            }
        }
    }
}
