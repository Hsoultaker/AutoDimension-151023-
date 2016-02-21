using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tekla.Structures.Geometry3d;

namespace AutoDimension.Entity
{
    public class CCylinderDoorTopViewInfo
    {
         /// <summary>
        /// 与该顶视图关联的Part对象;
        /// </summary>
        private CMrPart mMrPart;

        /// <summary>
        /// 门式框架顶视图信息;
        /// </summary>
        /// <param name="mrPart"></param>
        public CCylinderDoorTopViewInfo(CMrPart mrPart)
        {
            this.mMrPart = mrPart;
        }

        /// <summary>
        /// 获得零件右侧的标注,主要是底部和顶部的零部件;
        /// </summary>
        /// <returns></returns>
        public CMrDimSet GetYRightPartDim()
        {
            CMrDimSet mrDimSet = new CMrDimSet();

            CMrPart yNormalBottomPart = CMrCylinderDoorTopManager.GetInstance().mYNormalBottomPart;
            CMrPart yNormalTopPart = CMrCylinderDoorTopManager.GetInstance().mTopPart;

            if (mMrPart == yNormalBottomPart)
            {
                mrDimSet.AddPoint(mMrPart.GetMaxXMinYPoint());
                mrDimSet.AddPoint(mMrPart.GetMaxXMaxYPoint());
            }
            if (mMrPart == yNormalTopPart)
            {
                mrDimSet.AddPoint(mMrPart.GetMaxXMinYPoint());
                mrDimSet.AddPoint(mMrPart.GetMaxXMaxYPoint());
            }

            return mrDimSet;
        }

        /// <summary>
        /// 获取主梁右侧螺钉组的标注;
        /// </summary>
        /// <returns></returns>
        public CMrDimSet GetYRightBoltDim()
        {
            Vector zVector=new Vector(0,0,1);
            Vector normal = mMrPart.mNormal;

            double maxX = mMrPart.GetMaxXPoint().X;

            if (!CDimTools.GetInstance().IsTwoVectorParallel(normal, zVector))
            {
                return null;
            }
            if (maxX < CCommonPara.mDblError || !mMrPart.IsHaveBolt())
            {
                return null;
            }

            CMrDimSet mrDimSet = new CMrDimSet();

            List<CMrBoltArray> mrBoltArrayList = mMrPart.GetBoltArrayList();

            foreach (CMrBoltArray mrBoltArray in mrBoltArrayList)
            {
                //如果螺钉组的法向与Z轴不平行,则返回继续执行;
                if (!CDimTools.GetInstance().IsTwoVectorParallel(zVector, mrBoltArray.mNormal))
                {
                    continue;
                }
                if (mrBoltArray.mBoltArrayShapeType == MrBoltArrayShapeType.ARRAY)
                {
                    List<Point> maxPointList = mrBoltArray.GetMaxXPointList();

                    mrDimSet.AddRange(maxPointList);
                }
                else if (mrBoltArray.mBoltArrayShapeType == MrBoltArrayShapeType.OBLIQUELINE)
                {
                    List<Point> boltPointList = mrBoltArray.GetBoltPointList();

                    mrDimSet.AddRange(boltPointList);
                }
            }
            return mrDimSet;
        }

        /// <summary>
        /// 获得零件左侧的标注,主要是底部和顶部的零部件;
        /// </summary>
        /// <returns></returns>
        public CMrDimSet GetYLeftPartDim()
        {
            CMrDimSet mrDimSet = new CMrDimSet();

            CMrPart yNormalBottomPart = CMrCylinderDoorTopManager.GetInstance().mYNormalBottomPart;
            CMrPart yNormalTopPart = CMrCylinderDoorTopManager.GetInstance().mTopPart;

            if (mMrPart == yNormalBottomPart)
            {
                mrDimSet.AddPoint(mMrPart.GetMinXMinYPoint());
                mrDimSet.AddPoint(mMrPart.GetMinXMaxYPoint());
            }
            if (mMrPart == yNormalTopPart)
            {
                mrDimSet.AddPoint(mMrPart.GetMinXMinYPoint());
                mrDimSet.AddPoint(mMrPart.GetMinXMaxYPoint());
            }

            return mrDimSet;
        }

        /// <summary>
        /// 获取主梁左侧螺钉组的标注;
        /// </summary>
        /// <returns></returns>
        public CMrDimSet GetYLeftBoltDim()
        {
            Vector zVector = new Vector(0, 0, 1);
            Vector normal = mMrPart.mNormal;

            double maxX = mMrPart.GetMaxXPoint().X;

            if (!CDimTools.GetInstance().IsTwoVectorParallel(normal, zVector))
            {
                return null;
            }
            if (maxX > CCommonPara.mDblError || !mMrPart.IsHaveBolt())
            {
                return null;
            }

            CMrDimSet mrDimSet = new CMrDimSet();

            List<CMrBoltArray> mrBoltArrayList = mMrPart.GetBoltArrayList();

            foreach (CMrBoltArray mrBoltArray in mrBoltArrayList)
            {
                //如果螺钉组的法向与Z轴不平行,则返回继续执行;
                if (!CDimTools.GetInstance().IsTwoVectorParallel(zVector, mrBoltArray.mNormal))
                {
                    continue;
                }
                if (mrBoltArray.mBoltArrayShapeType == MrBoltArrayShapeType.ARRAY)
                {
                    List<Point> minXPointList = mrBoltArray.GetMinXPointList();

                    mrDimSet.AddRange(minXPointList);
                }
                else if (mrBoltArray.mBoltArrayShapeType == MrBoltArrayShapeType.OBLIQUELINE)
                {
                    List<Point> boltPointList = mrBoltArray.GetBoltPointList();

                    mrDimSet.AddRange(boltPointList);
                }
            }
            return mrDimSet;
        }

        /// <summary>
        /// 获取零件上螺钉的标注集,只有左侧或则右侧的零部件螺钉需要标注;
        /// </summary>
        /// <returns></returns>
        public List<CMrDimSet> GetBoltDimSetList()
        {
            List<CMrDimSet> mrDimSetList = new List<CMrDimSet>();

            Vector normal = mMrPart.mNormal;

            Vector zVector=new Vector(0,0,1);

            double minX = mMrPart.GetMinXPoint().X;
            double maxX = mMrPart.GetMaxYPoint().X;

            if (minX < 0 && maxX > 0)
            {
                return mrDimSetList;
            }
            if (!CDimTools.GetInstance().IsTwoVectorParallel(normal, zVector))
            {
                return mrDimSetList;
            }

            List<CMrBoltArray> mrBoltArrayList = mMrPart.GetBoltArrayList();

            foreach (CMrBoltArray mrBoltArray in mrBoltArrayList)
            {
                if (mrBoltArray.mBoltArrayShapeType == MrBoltArrayShapeType.ARRAY)
                {
                    List<Point> maxYPointList = mrBoltArray.GetMaxYPointList();

                    CMrDimSet mrDimSet = new CMrDimSet();

                    mrDimSet.AddRange(maxYPointList);
                    mrDimSet.AddPoint(new Point(0, maxYPointList[0].Y, 0));
                    mrDimSet.mDimVector = new Vector(0, 1, 0);
                    mrDimSet.mDimDistance = Math.Abs(maxYPointList[0].Y - mMrPart.GetMaxYPoint().Y) + 60;

                    if (mrDimSet.mDimDistance > 500)
                    {
                        mrDimSet.mDimDistance = CCommonPara.mDefaultDimDistance;
                    }

                    mrDimSetList.Add(mrDimSet);
                }
                else if (mrBoltArray.mBoltArrayShapeType == MrBoltArrayShapeType.OBLIQUELINE)
                {
                    List<Point> boltPointList = mrBoltArray.GetBoltPointList();

                    CMrDimSet mrDimSet = new CMrDimSet();

                    Point firstPt = mrBoltArray.mFirstPoint;
                    Point secondPt = mrBoltArray.mSecondPoint;

                    Vector dimVector = null;

                    MrSlopeType mrSlopeType = CDimTools.GetInstance().JudgeLineSlope(firstPt, secondPt);

                    if (mrSlopeType == MrSlopeType.MORETHAN_ZERO)
                    {
                        dimVector = new Vector(0, 1, 0);
                        mrDimSet.mDimDistance = Math.Abs(boltPointList[0].Y - mMrPart.GetMaxYPoint().Y) + 60;
                    }
                    else if (mrSlopeType == MrSlopeType.LESSTHAN_ZERO)
                    {
                        dimVector = new Vector(0, -1, 0);
                        mrDimSet.mDimDistance = Math.Abs(boltPointList[0].Y - mMrPart.GetMinYPoint().Y) + 60;
                    }

                    if (mrDimSet.mDimDistance > 500)
                    {
                        mrDimSet.mDimDistance = CCommonPara.mDefaultDimDistance;
                    }

                    mrDimSet.AddRange(boltPointList);
                    mrDimSet.mDimVector = dimVector;
                    mrDimSet.AddPoint(new Point(0,boltPointList[0].Y,0));
            
                    mrDimSetList.Add(mrDimSet);
                }
            }
            return mrDimSetList;
        }

        /// <summary>
        /// 获取零件标记;
        /// </summary>
        /// <returns></returns>
        public CMrMark GetPartMark()
        {
            CMrMark mrMark = new CMrMark();

            Vector normal = mMrPart.mNormal;

            double minX = mMrPart.GetMinXPoint().X;
            double maxX = mMrPart.GetMaxXPoint().X;

            if (minX < 0 && maxX > 0)
            {
                return null;
            }
            if (!CDimTools.GetInstance().IsTwoVectorParallel(normal, new Vector(0, 0, 1))||!mMrPart.IsHaveBolt())
            {
                return null;
            }

            double dblAngle = CCommonPara.mPartMarkAngle;
            mrMark.mModelObject = mMrPart.mPartInDrawing;

            if (minX > 0)
            {
                mrMark.mInsertPoint = mMrPart.GetMaxXMaxYPoint();
                double increaseXDistance = CCommonPara.mPartMarkLength / Math.Tan(dblAngle);
                mrMark.mTextPoint.X = mrMark.mInsertPoint.X + increaseXDistance;
                mrMark.mTextPoint.Y = mrMark.mInsertPoint.Y + 1.2 * CCommonPara.mPartMarkLength;
                return mrMark;
            }
            if (maxX < 0)
            {
                mrMark.mInsertPoint = mMrPart.GetMinXMaxYPoint();
                double increaseXDistance = CCommonPara.mPartMarkLength / Math.Tan(dblAngle);
                mrMark.mTextPoint.X = mrMark.mInsertPoint.X - increaseXDistance;
                mrMark.mTextPoint.Y = mrMark.mInsertPoint.Y + 1.2 * CCommonPara.mPartMarkLength;
                return mrMark;
            }
            return null;
        }

        /// <summary>
        /// 获得法向与Z轴平行的中间的零件中螺钉标记;
        /// </summary>
        /// <returns></returns>
        public List<CMrMark> GetZNormalMiddleBoltMark()
        {
            List<CMrMark> mrMarkList = new List<CMrMark>();

            Vector normal = mMrPart.mNormal;

            double minX = mMrPart.GetMinXPoint().X;
            double maxX = mMrPart.GetMaxXPoint().X;

            if (!CDimTools.GetInstance().IsTwoVectorParallel(normal, new Vector(0, 0, 1)) || !mMrPart.IsHaveBolt())
            {
                return mrMarkList;
            }
            
            if(CDimTools.GetInstance().CompareTwoDoubleValue(minX, 0) >= 0 ||
                CDimTools.GetInstance().CompareTwoDoubleValue(maxX, 0) <= 0)
            {
                return mrMarkList;
            }

            List<CMrBoltArray> mrBoltArrayList = mMrPart.GetBoltArrayList();

            foreach (CMrBoltArray mrBoltArray in mrBoltArrayList)
            {
                if (!CDimTools.GetInstance().IsTwoVectorParallel(mrBoltArray.mNormal, new Vector(0, 0, 1)))
                {
                    continue;
                }
                CMrMark mrMark = new CMrMark();

                double dblAngle = CCommonPara.mBoltMarkAngle;
                mrMark.mModelObject = mrBoltArray.mBoltInDrawing;

                mrMark.mInsertPoint = mrBoltArray.GetMaxXMaxYPoint();
                double increaseXDistance = CCommonPara.mBoltMarkLength / Math.Tan(dblAngle);
                mrMark.mTextPoint.X = mrMark.mInsertPoint.X + increaseXDistance;
                mrMark.mTextPoint.Y = mrMark.mInsertPoint.Y + 1.2 * CCommonPara.mBoltMarkLength;
                
                mrMarkList.Add(mrMark);
            }

            return mrMarkList;
        }

        /// <summary>
        /// 获得法向与Z轴平行的中间的零件中螺钉标注;
        /// </summary>
        /// <returns></returns>
        public List<CMrDimSet> GetZNormalMiddleDimSet()
        {
            List<CMrDimSet> mrDimSetList = new List<CMrDimSet>();

            Vector normal = mMrPart.mNormal;

            double minX = mMrPart.GetMinXPoint().X;
            double maxX = mMrPart.GetMaxXPoint().X;

            if (!CDimTools.GetInstance().IsTwoVectorParallel(normal, new Vector(0, 0, 1)) || !mMrPart.IsHaveBolt())
            {
                return mrDimSetList;
            }
            if (CDimTools.GetInstance().CompareTwoDoubleValue(minX, 0) >= 0 
                || CDimTools.GetInstance().CompareTwoDoubleValue(maxX, 0) <= 0)
            {
                return mrDimSetList;
            }

            List<CMrBoltArray> mrBoltArrayList = mMrPart.GetBoltArrayList();

            foreach (CMrBoltArray mrBoltArray in mrBoltArrayList)
            {
                if (!CDimTools.GetInstance().IsTwoVectorParallel(mrBoltArray.mNormal, new Vector(0, 0, 1)))
                {
                    continue;
                }

                List<Point> pointList = mrBoltArray.GetMaxYPointList();

                CMrDimSet mrDimSet = new CMrDimSet();

                mrDimSet.AddRange(pointList);
                mrDimSet.mDimVector = new Vector(0, 1, 0);
                mrDimSet.mDimDistance = CCommonPara.mDefaultDimDistance;
                mrDimSetList.Add(mrDimSet);
            }

            return mrDimSetList;
        }
    }
}
