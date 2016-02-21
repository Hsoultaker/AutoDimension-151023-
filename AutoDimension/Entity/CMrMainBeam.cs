using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TSM = Tekla.Structures.Model;
using TSD = Tekla.Structures.Drawing;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model;

namespace AutoDimension.Entity
{
    /// <summary>
    /// 主梁对象;
    /// </summary>
    public class CMrMainBeam:CMrPart
    {
        /// <summary>
        /// 主梁的实例对象;
        /// </summary>
        private static CMrMainBeam mInstance = null;

        /// <summary>
        /// 主梁的翼缘宽度;
        /// </summary>
        public double mFlangeThickness = 0.0;

        /// <summary>
        /// 厚度;
        /// </summary>
        public double mWebThickness = 0.0;

        /// <summary>
        /// 设置实例;
        /// </summary>
        /// <param name="instance"></param>
        public static void SetInstance(CMrMainBeam instance)
        {
            mInstance = instance;
        }

        /// <summary>
        /// 获得实例;
        /// </summary>
        /// <returns></returns>
        public static CMrMainBeam GetInstance()
        {
            return mInstance;
        }

        /// <summary>
        /// 左侧工作点;
        /// </summary>
        public Point mLeftWorkPoint;

        /// <summary>
        /// 右侧工作点;
        /// </summary>
        public Point mRightWorkPoint;

        /// <summary>
        /// 主梁的构造函数;
        /// </summary>
        /// <param name="partInModel"></param>
        /// <param name="partInDrawing"></param>
        public CMrMainBeam(TSM.Part partInModel, TSD.Part partInDrawing):base(partInModel,partInDrawing)
        {
            this.mPartInModel = partInModel;
            this.mPartInDrawing = partInDrawing;
        }

        /// <summary>
        /// 判断主梁上左上方是否有小的切口，切口的高度是翼缘的高度乘以tan45;
        /// </summary>
        public bool JudgeLeftTopIncision()
        {
            Point minXMaxYPoint = GetMinXMaxYPoint();
            Point maxYminXPoint = GetMaxYMinXPoint();
 
            if(CDimTools.GetInstance().CompareTwoDoubleValue(minXMaxYPoint.X,maxYminXPoint.X)==0)
            {
                return false;
            }
            else
            {
                double yValue = Math.Abs(minXMaxYPoint.Y - maxYminXPoint.Y);

                if (yValue <= mFlangeThickness * Math.Tan(45 * Math.PI / 180))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 判断主梁左下方是否有小切口;
        /// </summary>
        /// <returns></returns>
        public bool JudgeLeftBottomIncision()
        {
            Point minXminYPoint = GetMinXMinYPoint();
            Point minYminXPoint = GetMinYMinXPoint();

            if (CDimTools.GetInstance().CompareTwoDoubleValue(minXminYPoint.X, minYminXPoint.X) == 0)
            {
                return false;
            }
            else
            {
                double yValue = Math.Abs(minXminYPoint.Y - minYminXPoint.Y);

                if (yValue <= mFlangeThickness * Math.Tan(45 * Math.PI / 180))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 判断主梁右上方是否有小切口;
        /// </summary>
        /// <returns></returns>
        public bool JudgeRightTopIncision()
        {
            Point maxXmaxYPoint = GetMaxXMaxYPoint();
            Point maxYmaxXPoint = GetMaxYMaxXPoint();

            if (CDimTools.GetInstance().CompareTwoDoubleValue(maxXmaxYPoint.X, maxYmaxXPoint.X) == 0)
            {
                return false;
            }
            else
            {
                double yValue = Math.Abs(maxXmaxYPoint.Y - maxYmaxXPoint.Y);

                if (yValue <= mFlangeThickness * Math.Tan(45 * Math.PI / 180))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 判断主梁右下方是否有小切口;
        /// </summary>
        /// <returns></returns>
        public bool JudgeRightBottomIncision()
        {
            Point maxXminYPoint = GetMaxXMinYPoint();
            Point minYmaxXPoint = GetMinYMaxXPoint();

            if (CDimTools.GetInstance().CompareTwoDoubleValue(maxXminYPoint.X, minYmaxXPoint.X) == 0)
            {
                return false;
            }
            else
            {
                double yValue = Math.Abs(maxXminYPoint.Y - minYmaxXPoint.Y);

                if (yValue <= mFlangeThickness * Math.Tan(45 * Math.PI / 180))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
