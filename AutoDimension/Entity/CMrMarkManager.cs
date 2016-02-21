using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoDimension.Entity;
using Tekla.Structures.Drawing;
using Tekla.Structures.Geometry3d;

namespace AutoDimension
{
    public class CMrMarkManager
    {
        /// <summary>
        /// 单一实例;
        /// </summary>
        private static CMrMarkManager mInstance = null;

        /// <summary>
        /// 零件标记的链表;
        /// </summary>
        private List<CMrMark> mMrMarkList = new List<CMrMark>();

        /// <summary>
        /// 获取单例;
        /// </summary>
        /// <returns></returns>
        public static CMrMarkManager GetInstance()
        {
            if (null == mInstance)
            {
                mInstance = new CMrMarkManager();
            }

            return mInstance;
        }

        /// <summary>
        /// 添加零件标记;
        /// </summary>
        /// <param name="mrMark"></param>
        public void AppendMrMark(CMrMark mrMark)
        {
            if(null==mrMark)
            {
                return;
            }

            mMrMarkList.Add(mrMark);
        }

        /// <summary>
        /// 清空Mark对象;
        /// </summary>
        public void Clear()
        {
            mMrMarkList.Clear();
        }

        /// <summary>
        /// 调整零件标记;
        /// </summary>
        /// <param name="mrMark"></param>
        public void AdjustMrMark(CMrMark mrMark)
        {
            foreach(CMrMark mrMark1 in mMrMarkList)
            {
                if(!IsTwoMarkOverlapp(mrMark,mrMark1))
                {
                    continue;
                }
                AdjustOverlappMark(mrMark,mrMark1);
            }
        }

        /// <summary>
        /// 判断两个Mark是否重叠;
        /// </summary>
        /// <param name="mrMark1"></param>
        /// <param name="mrMark2"></param>
        /// <returns></returns>
        private bool IsTwoMarkOverlapp(CMrMark mrMark1,CMrMark mrMark2)
        {
            RectangleBoundingBox boundingBox1 = mrMark1.mTextBoundingBox;
            RectangleBoundingBox boundingBox2 = mrMark2.mTextBoundingBox;

            Point centerPoint1 = boundingBox1.GetCenterPoint();
            Point centerPoint2 = boundingBox2.GetCenterPoint();

            double x = Math.Abs(centerPoint1.X - centerPoint2.X);
            double y = Math.Abs(centerPoint1.Y - centerPoint2.Y);

            if(x < CCommonPara.mDblError && y < CCommonPara.mDblError)
            {
                return false;
            }

            if (y > (boundingBox1.Height + boundingBox2.Height) / 2.0)
            {
                return false;
            }

            if (x > (boundingBox1.Width + boundingBox2.Width) / 2.0)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 调整重叠的Mark;
        /// </summary>
        /// <param name="mrMark1">需要调整的Mark</param>
        /// <param name="mrMark2">已经存在的Mark</param>
        private void AdjustOverlappMark(CMrMark mrMark1,CMrMark mrMark2)
        {
            RectangleBoundingBox boundingBox1 = mrMark1.mTextBoundingBox;
            RectangleBoundingBox boundingBox2 = mrMark2.mTextBoundingBox;

            Point centerPoint1 = boundingBox1.GetCenterPoint();
            Point centerPoint2 = boundingBox2.GetCenterPoint();

            double x = centerPoint1.X - centerPoint2.X;
            double y = centerPoint1.Y - centerPoint2.Y;

            if (Math.Abs(x) < (boundingBox1.Width + boundingBox2.Width) / 2.0)
            {
                double overlappWidth = (boundingBox1.Width + boundingBox2.Width) / 2.0 - Math.Abs(x) + 10;

                if(x < CCommonPara.mDblError)
                {
                    mrMark1.mTextPoint.X = mrMark1.mTextPoint.X - overlappWidth;
                   
                    mrMark1.mTextBoundingBox.LowerLeft.X = mrMark1.mTextBoundingBox.LowerLeft.X - overlappWidth;
                    mrMark1.mTextBoundingBox.LowerRight.X = mrMark1.mTextBoundingBox.LowerRight.X - overlappWidth;
                    mrMark1.mTextBoundingBox.UpperLeft.X = mrMark1.mTextBoundingBox.UpperLeft.X - overlappWidth;
                    mrMark1.mTextBoundingBox.UpperRight.X = mrMark1.mTextBoundingBox.UpperRight.X - overlappWidth;
                }
                else if(x > CCommonPara.mDblError)
                {
                    mrMark1.mTextPoint.X = mrMark1.mTextPoint.X + overlappWidth;

                    mrMark1.mTextBoundingBox.LowerLeft.X = mrMark1.mTextBoundingBox.LowerLeft.X + overlappWidth;
                    mrMark1.mTextBoundingBox.LowerRight.X = mrMark1.mTextBoundingBox.LowerRight.X + overlappWidth;
                    mrMark1.mTextBoundingBox.UpperLeft.X = mrMark1.mTextBoundingBox.UpperLeft.X + overlappWidth;
                    mrMark1.mTextBoundingBox.UpperRight.X = mrMark1.mTextBoundingBox.UpperRight.X + overlappWidth;
                }
            }
            if (Math.Abs(y) < (boundingBox1.Height + boundingBox2.Height) / 2.0)
            {
                double overlappHeight = (boundingBox1.Height + boundingBox2.Height) / 2.0 - Math.Abs(y) + 10;

                if(y < CCommonPara.mDblError)
                {                    
                    mrMark1.mTextPoint.Y = mrMark1.mTextPoint.Y - overlappHeight;

                    mrMark1.mTextBoundingBox.LowerLeft.Y = mrMark1.mTextBoundingBox.LowerLeft.Y - overlappHeight;
                    mrMark1.mTextBoundingBox.LowerRight.Y = mrMark1.mTextBoundingBox.LowerRight.Y - overlappHeight;
                    mrMark1.mTextBoundingBox.UpperLeft.Y = mrMark1.mTextBoundingBox.UpperLeft.Y - overlappHeight;
                    mrMark1.mTextBoundingBox.UpperRight.Y = mrMark1.mTextBoundingBox.UpperRight.Y - overlappHeight;
                }
                else if(y > CCommonPara.mDblError)
                {
                    mrMark1.mTextPoint.Y = mrMark1.mTextPoint.Y + overlappHeight;

                    mrMark1.mTextBoundingBox.LowerLeft.Y = mrMark1.mTextBoundingBox.LowerLeft.Y + overlappHeight;
                    mrMark1.mTextBoundingBox.LowerRight.Y = mrMark1.mTextBoundingBox.LowerRight.Y + overlappHeight;
                    mrMark1.mTextBoundingBox.UpperLeft.Y = mrMark1.mTextBoundingBox.UpperLeft.Y + overlappHeight;
                    mrMark1.mTextBoundingBox.UpperRight.Y = mrMark1.mTextBoundingBox.UpperRight.Y + overlappHeight;
                }
            }
        }
    }
}
