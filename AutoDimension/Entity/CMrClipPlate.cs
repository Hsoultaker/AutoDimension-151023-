using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Tekla.Structures.Geometry3d;

namespace AutoDimension.Entity
{
    /// <summary>
    /// 自定义的剪切板对象,主要是在柱标注中;
    /// 剪切板是由三块板组成，主要有中间板和上下两块板组成;上下两块板的方向向量竖直，
    /// 与中间板的方向向量垂直;
    /// </summary>
    public class CMrClipPlate
    {
        /// <summary>
        /// 中间板对象;
        /// </summary>
        public CMrPart mMrMidPart;

        /// <summary>
        /// 上面的一块板;
        /// </summary>
        public CMrPart mMrTopPart;

        /// <summary>
        /// 下面的一块板;
        /// </summary>
        public CMrPart mMrBottomPart;

        /// <summary>
        /// 标志剪切板是否是有效的;
        /// </summary>
        public bool mbValid = true;

        /// <summary>
        /// 剪切板的构造函数;
        /// </summary>
        /// <param name="mTopPart"></param>
        /// <param name="mMidPart"></param>
        /// <param name="mBottomPart"></param>
        public CMrClipPlate(CMrPart mTopPart,CMrPart mMidPart,CMrPart mBottomPart)
        {
            mMrTopPart = mTopPart;
            mMrMidPart = mMidPart;
            mMrBottomPart = mBottomPart;

            //如果上下板之间有一个为空则无效;
            if(mTopPart==null || mBottomPart==null)
            {
                mbValid = false;
            }
        }

        /// <summary>
        /// 是否包含该对象;
        /// </summary>
        /// <param name="mrPart"></param>
        /// <returns></returns>
        public MrClipPlatePosType IsHaveThePart(CMrPart mrPart)
        {
            if(mMrTopPart == mrPart)
            {
                return MrClipPlatePosType.TOP;
            }
            if(mMrMidPart == mrPart)
            {
                return MrClipPlatePosType.MIDDLE;
            }
            if (mMrBottomPart == mrPart)
            {
                return MrClipPlatePosType.BOTTOM;
            }

            return MrClipPlatePosType.NONE;
        }

        /// <summary>
        /// 获取Y值最大的点;
        /// </summary>
        /// <returns></returns>
        public Point GetMaxYPoint()
        {
            return mMrTopPart.GetMaxYPoint();
        }

        /// <summary>
        /// 获取Y值最小的点;
        /// </summary>
        /// <returns></returns>
        public Point GetMinYPoint()
        {
            return mMrBottomPart.GetMinYPoint();
        }

        /// <summary>
        /// 获取X值最大的点;
        /// </summary>
        /// <returns></returns>
        public Point GetMaxXPoint()
        {
            Point maxXPoint = mMrTopPart.GetMaxXPoint();

            Point newMaxXPoint=mMrMidPart.GetMaxXPoint();

            if(CDimTools.GetInstance().CompareTwoDoubleValue(maxXPoint.X,newMaxXPoint.X) < 0)
            {
                maxXPoint = newMaxXPoint;
            }

            newMaxXPoint = mMrBottomPart.GetMaxXPoint();
            if (CDimTools.GetInstance().CompareTwoDoubleValue(maxXPoint.X, newMaxXPoint.X) < 0)
            {
                maxXPoint = newMaxXPoint;
            }

            return maxXPoint;
        }

        /// <summary>
        /// 获取X值最小的点;
        /// </summary>
        /// <returns></returns>
        public Point GetMinXPoint()
        {
            Point minXPoint = mMrTopPart.GetMinXPoint();

            Point newMinXPoint = mMrMidPart.GetMinXPoint();

            if (CDimTools.GetInstance().CompareTwoDoubleValue(minXPoint.X, newMinXPoint.X) > 0)
            {
                minXPoint = newMinXPoint;
            }

            newMinXPoint = mMrBottomPart.GetMinXPoint();
            if (CDimTools.GetInstance().CompareTwoDoubleValue(minXPoint.X, newMinXPoint.X) > 0)
            {
                minXPoint = newMinXPoint;
            }

            return minXPoint;
        }
    }
}
