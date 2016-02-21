using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tekla.Structures.Geometry3d;

namespace AutoDimension.Entity
{
    /// <summary>
    /// 门式框架结构中柱子的前视图信息;
    /// </summary>
    public class CCylinderDoorFrontViewInfo
    {
        /// <summary>
        /// 与该FrontView视图关联的Part对象;
        /// </summary>
        private CMrPart mMrPart;

        /// <summary>
        /// 门式框架前视图信息;
        /// </summary>
        /// <param name="mrPart"></param>
        public CCylinderDoorFrontViewInfo(CMrPart mrPart)
        {
            this.mMrPart = mrPart;
        }

        /// <summary>
        /// 判断与Y轴平行零部件是否刚好在主梁的中间;
        /// </summary>
        /// <param name="mrPart"></param>
        /// <returns></returns>
        private bool IsYNormalPartInMainBeamMiddle(CMrPart mrPart)
        {
            double mainBeamMinX = CMrMainBeam.GetInstance().GetMinXPoint().X;
            double mainBeamMaxX = CMrMainBeam.GetInstance().GetMaxXPoint().X;
            double mainBeamXLength = mainBeamMaxX - mainBeamMinX;

            double partMinX = mMrPart.GetMinXPoint().X;
            double partMaxX = mMrPart.GetMaxXPoint().X;
            double partXLength = partMaxX - partMinX;

            Vector normal = mrPart.mNormal;

            if (!CDimTools.GetInstance().IsTwoVectorParallel(normal, new Vector(0, 1, 0)))
            {
                return false;
            }
            if (partMinX > mainBeamMaxX)
            {
                return false;
            }
            if (partMaxX < mainBeamMinX)
            {
                return false;
            }
            if (Math.Abs(mainBeamXLength - partXLength) > 2 * mainBeamMaxX / 3)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 获取主梁右侧的标注集合;
        /// </summary>
        /// <returns></returns>
        public CMrDimSet GetYPartMainRightDimSet()
        {
            CMrDimSet mrDimSet = new CMrDimSet();

            Vector xVector = new Vector(1, 0, 0);
            Vector yVector = new Vector(0, 1, 0);
            Vector zVector = new Vector(0, 0, 1);

            double mainBeamMinX = CMrMainBeam.GetInstance().GetMinXPoint().X;
            double mainBeamMaxX = CMrMainBeam.GetInstance().GetMaxXPoint().X;

            double partMinX = mMrPart.GetMinXPoint().X;
            double partMaxX = mMrPart.GetMaxXPoint().X;

            if (partMaxX < CCommonPara.mDblError)
            {
                return null;
            }

            CMrPart mrRightPart = CMrCylinderDoorFrontManager.GetInstance().mRightPart;
            CMrPart mrRightTopPart = CMrCylinderDoorFrontManager.GetInstance().mRightTopPart;
            CMrPart mrRightTopMiddlePart = CMrCylinderDoorFrontManager.GetInstance().mRightTopMiddlePart;

            CMrPart mrTopPart = CMrCylinderDoorFrontManager.GetInstance().mTopPart;
            CMrPart mrZNormalBottomPart = CMrCylinderDoorFrontManager.GetInstance().mZNormalBottomPart;
            CMrPart mrYNormalBottomPart = CMrCylinderDoorFrontManager.GetInstance().mYNormalBottomPart;

            Vector normal = mMrPart.mNormal;

            //1.如果是左下侧法向与z轴平行的零部件;
            if (mMrPart == mrZNormalBottomPart)
            {
                mrDimSet.AddPoint(mMrPart.GetMaxXMinYPoint());
                return mrDimSet;
            }
            //2.如果是右上侧的零部件;
            if (mMrPart == mrRightTopPart)
            {
                if (CDimTools.GetInstance().CompareTwoDoubleValue(partMaxX, mainBeamMaxX) < 0)
                {
                    return null;
                }
                else
                {
                    mrDimSet.AddPoint(mMrPart.GetMaxXMinYPoint());
                    return mrDimSet;
                }
            }
            //3.如果是右上侧中间的零部件;
            if (mMrPart == mrRightTopMiddlePart)
            {
                mrDimSet.AddPoint(mMrPart.GetMaxXMaxYPoint());
                return mrDimSet;
            }
            //4.如果是下侧法向与Y轴平行的零部件;
            if (mMrPart == mrYNormalBottomPart)
            {
                mrDimSet.AddPoint(mrYNormalBottomPart.GetMaxXMaxYPoint());
                mrDimSet.AddPoint(mrYNormalBottomPart.GetMaxXMinYPoint());

                return mrDimSet;
            }
            //5.如果是上侧的零部件;
            if (mMrPart == mrTopPart)
            {
                //A.如果上侧零件的法向与Y轴平行;
                if (CDimTools.GetInstance().IsTwoVectorParallel(normal, yVector))
                {
                    mrDimSet.AddPoint(mrTopPart.GetMaxXMaxYPoint());
                    mrDimSet.AddPoint(mrTopPart.GetMaxXMinYPoint());
                    return mrDimSet;
                }
                //B.如果法向不与Y轴平行;
                else if (CDimTools.GetInstance().IsVectorInXYPlane(normal))
                {
                    Point minYPoint = mMrPart.GetMinYPoint();
                    Point maxYPoint = mMrPart.GetMaxYPoint();

                    MrSlopeType mrSlopeType = CDimTools.GetInstance().JudgeLineSlope(minYPoint, maxYPoint);

                    if (mrSlopeType == MrSlopeType.LESSTHAN_ZERO)
                    {
                        mrDimSet.AddPoint(mMrPart.GetMaxYPoint());
                        mrDimSet.AddPoint(mMrPart.GetMaxXPoint());
                        return mrDimSet;
                    }
                }
            }
            //6.如果向量与X轴平行,并且该零部件贴在右侧板上或右上侧板上的;
            if (CDimTools.GetInstance().IsTwoVectorParallel(normal, xVector))
            {
                //A.如果右上侧的零件不为空，并且该零件的左侧就是右上侧的板;
                if (mrRightTopPart != null && mrRightTopPart.GetMaxXMinYPoint().Y < mMrPart.GetMaxYPoint().Y)
                {
                    double rightTopMaxX = mrRightTopPart.GetMaxXPoint().X;

                    if (Math.Abs(partMinX - rightTopMaxX) < CCommonPara.mDblError)
                    {
                        mrDimSet.AddPoint(mMrPart.GetMaxXMinYPoint());

                        CMrCylinderDoorFrontManager.GetInstance().AppendRightDimPart(mMrPart);

                        return mrDimSet;
                    }
                    else if (partMinX > rightTopMaxX)
                    {
                        CMrCylinderDoorFrontManager.GetInstance().AppendXNormalAloneDimPart(mMrPart);
                    }
                }
                //B.如果右上侧中间的零件不为空，并且该零件的左侧就是右上侧中间的板;
                if (mrRightTopMiddlePart != null && mrRightTopMiddlePart.GetMaxXMinYPoint().Y < mMrPart.GetMaxYPoint().Y)
                {
                    double rightTopMiddleMaxX = mrRightTopMiddlePart.GetMaxXPoint().X;

                    if (Math.Abs(partMinX - rightTopMiddleMaxX) < CCommonPara.mDblError)
                    {
                        mrDimSet.AddPoint(mMrPart.GetMaxXMinYPoint());

                        CMrCylinderDoorFrontManager.GetInstance().AppendRightDimPart(mMrPart);

                        return mrDimSet;
                    }
                    else if (partMinX > rightTopMiddleMaxX)
                    {
                        CMrCylinderDoorFrontManager.GetInstance().AppendXNormalAloneDimPart(mMrPart);
                    }
                }
                //C.如果零件的左侧不是右上侧板;
                else
                {
                    double rightMaxX = mrRightPart.GetMaxXPoint().X;

                    if (Math.Abs(partMinX - rightMaxX) < CCommonPara.mDblError)
                    {
                        mrDimSet.AddPoint(mMrPart.GetMaxXMinYPoint());

                        CMrCylinderDoorFrontManager.GetInstance().AppendRightDimPart(mMrPart);

                        return mrDimSet;
                    }
                    else if (partMinX > rightMaxX)
                    {
                        CMrCylinderDoorFrontManager.GetInstance().AppendXNormalAloneDimPart(mMrPart);
                    }
                }
            }
            //7.如果向量与Y轴平行;
            else if (CDimTools.GetInstance().IsTwoVectorParallel(normal, yVector))
            {
                double rightMaxX = mrRightPart.GetMaxXPoint().X;

                if (CDimTools.GetInstance().CompareTwoDoubleValue(partMaxX, rightMaxX) > 0
                    && CDimTools.GetInstance().CompareTwoDoubleValue(partMinX, rightMaxX) <= 0)
                {
                    CMrApronPlate mrApronPlate = CMrCylinderDoorFrontManager.GetInstance().FindMrApronPlateByYNormalPart(mMrPart);

                    if (mrApronPlate != null)
                    {
                        if (mrApronPlate.mIsUp == true)
                        {
                            mrDimSet.AddPoint(mMrPart.GetMaxXMinYPoint());
                        }
                        else
                        {
                            mrDimSet.AddPoint(mMrPart.GetMaxXMaxYPoint());
                        }
                    }
                    else
                    {
                        mrDimSet.AddPoint(mMrPart.GetMaxXMaxYPoint());
                    }

                    CMrCylinderDoorFrontManager.GetInstance().AppendRightDimPart(mMrPart);
                }
                else if (IsYNormalPartInMainBeamMiddle(mMrPart))
                {
                    CMrCylinderDoorFrontManager.GetInstance().AppendYNormalMiddleDimPart(mMrPart);

                    return null;
                }
            }
            return mrDimSet;
        }

        /// <summary>
        /// 获取主梁左侧的标注集合;
        /// </summary>
        /// <returns></returns>
        public CMrDimSet GetYPartMainLeftDimSet()
        {
            CMrDimSet mrDimSet = new CMrDimSet();

            Vector xVector = new Vector(1, 0, 0);
            Vector yVector = new Vector(0, 1, 0);
            Vector zVector = new Vector(0, 0, 1);

            double mainBeamMinX = CMrMainBeam.GetInstance().GetMinXPoint().X;
            double mainBeamMaxX = CMrMainBeam.GetInstance().GetMaxXPoint().X;

            double partMinX = mMrPart.GetMinXPoint().X;
            double partMaxX = mMrPart.GetMaxXPoint().X;

            CMrPart mrLeftPart = CMrCylinderDoorFrontManager.GetInstance().mLeftPart;
            CMrPart mrLeftTopPart = CMrCylinderDoorFrontManager.GetInstance().mLeftTopPart;
            CMrPart mrLeftTopMiddlePart = CMrCylinderDoorFrontManager.GetInstance().mLeftTopMiddlePart;
            CMrPart mrZNormalBottomPart = CMrCylinderDoorFrontManager.GetInstance().mZNormalBottomPart;
            CMrPart mrYNormalBottomPart = CMrCylinderDoorFrontManager.GetInstance().mYNormalBottomPart;
            CMrPart mrTopPart = CMrCylinderDoorFrontManager.GetInstance().mTopPart;
            CMrPart mrRightPart = CMrCylinderDoorFrontManager.GetInstance().mRightPart;

            Vector normal = mMrPart.mNormal;

            //1.如果是左下侧法向与z轴平行的零部件;
            if (mMrPart == mrZNormalBottomPart)
            {
                mrDimSet.AddPoint(mMrPart.GetMinXMinYPoint());
                return mrDimSet;
            }
            //2.如果是左侧上方的零部件;
            if (mMrPart == mrLeftTopPart)
            {
                if (CDimTools.GetInstance().CompareTwoDoubleValue(partMinX, mainBeamMinX) > 0)
                {
                    return null;
                }
                else
                {
                    mrDimSet.AddPoint(mMrPart.GetMinXMinYPoint());
                    return mrDimSet;
                }
            }
            //3.如果是左上侧上方中间的零部件;
            if (mMrPart == mrLeftTopMiddlePart)
            {
                mrDimSet.AddPoint(mMrPart.GetMinXMaxYPoint());
                return mrDimSet;
            }
            //4.如果是下侧法向与Y轴平行的零部件;
            if (mMrPart == mrYNormalBottomPart)
            {
                mrDimSet.AddPoint(mrYNormalBottomPart.GetMinXMaxYPoint());
                mrDimSet.AddPoint(mrYNormalBottomPart.GetMinXMinYPoint());
                return mrDimSet;
            }
            //5.如果是上侧的零部件;
            if (mMrPart == mrTopPart)
            {
                //如果上侧零件的法向与Y轴平行;
                if (CDimTools.GetInstance().IsTwoVectorParallel(normal, yVector))
                {
                    mrDimSet.AddPoint(mrTopPart.GetMinXMaxYPoint());
                    mrDimSet.AddPoint(mrTopPart.GetMinXMinYPoint());
                    return mrDimSet;
                }
                //如果法向不与Y轴平行;
                else if (CDimTools.GetInstance().IsVectorInXYPlane(normal))
                {
                    Point maxYPoint = mMrPart.GetMaxYPoint();
                    Point minYPoint = mMrPart.GetMinYPoint();

                    MrSlopeType mrSlopeType = CDimTools.GetInstance().JudgeLineSlope(minYPoint, maxYPoint);

                    if (mrSlopeType == MrSlopeType.MORETHAN_ZERO)
                    {
                        mrDimSet.AddPoint(mMrPart.GetMaxYPoint());
                        mrDimSet.AddPoint(mMrPart.GetMinXPoint());
                        return mrDimSet;
                    }
                }
            }

            double leftMinX = mrLeftPart.GetMinXPoint().X;

            //6.如果向量与X轴平行,并且该零部件贴在左侧板上;
            if (CDimTools.GetInstance().IsTwoVectorParallel(normal, xVector))
            {
                //A:如果左上侧零部件不为空;
                if (mrLeftTopPart != null && mrLeftTopPart.GetMinXMinYPoint().Y < mMrPart.GetMaxYPoint().Y)
                {
                    double leftTopMinX = mrLeftTopPart.GetMinXPoint().X;

                    if (Math.Abs(partMaxX - leftTopMinX) < CCommonPara.mDblError)
                    {
                        mrDimSet.AddPoint(mMrPart.GetMinXMinYPoint());

                        return mrDimSet;
                    }
                    else if (partMaxX < leftTopMinX)
                    {
                        CMrCylinderDoorFrontManager.GetInstance().AppendXNormalAloneDimPart(mMrPart);
                    }
                }
                //B:如果左上侧中间零件不为空;
                if (mrLeftTopMiddlePart != null && mrLeftTopMiddlePart.GetMinXMinYPoint().Y < mMrPart.GetMaxYPoint().Y)
                {
                    double leftTopMiddleMinX = mrLeftTopMiddlePart.GetMinXPoint().X;

                    if (Math.Abs(partMaxX - leftTopMiddleMinX) < CCommonPara.mDblError)
                    {
                        mrDimSet.AddPoint(mMrPart.GetMinXMinYPoint());

                        return mrDimSet;
                    }
                    else if (partMaxX < leftTopMiddleMinX)
                    {
                        CMrCylinderDoorFrontManager.GetInstance().AppendXNormalAloneDimPart(mMrPart);
                    }
                }
                else
                {
                    if (Math.Abs(partMaxX - leftMinX) < CCommonPara.mDblError)
                    {
                        mrDimSet.AddPoint(mMrPart.GetMinXMinYPoint());

                        return mrDimSet;
                    }
                    else if (partMaxX < leftMinX)
                    {
                        CMrCylinderDoorFrontManager.GetInstance().AppendXNormalAloneDimPart(mMrPart);
                    }
                }
            }
            //7.如果向量与Y轴平行;
            else if (CDimTools.GetInstance().IsTwoVectorParallel(normal, yVector))
            {
                if (CDimTools.GetInstance().CompareTwoDoubleValue(partMinX, leftMinX) < 0
                    && CDimTools.GetInstance().CompareTwoDoubleValue(partMaxX, leftMinX) >= 0)
                {
                    CMrApronPlate mrApronPlate = CMrCylinderDoorFrontManager.GetInstance().FindMrApronPlateByYNormalPart(mMrPart);

                    if (mrApronPlate != null)
                    {
                        if (mrApronPlate.mIsUp == true)
                        {
                            mrDimSet.AddPoint(mMrPart.GetMaxXMinYPoint());
                        }
                        else
                        {
                            mrDimSet.AddPoint(mMrPart.GetMinXMaxYPoint());
                        }
                    }
                    else
                    {
                        mrDimSet.AddPoint(mMrPart.GetMinXMaxYPoint());
                    }
                }
            }
            return mrDimSet;
        }

        /// <summary>
        /// 获取门式框架结构柱子中的零件标记;
        /// </summary>
        /// <returns></returns>
        public CMrMark GetPartMark()
        {
            CMrPart mrRightPart = CMrCylinderDoorFrontManager.GetInstance().mRightPart;
            CMrPart mrRightTopPart = CMrCylinderDoorFrontManager.GetInstance().mRightTopPart;
            CMrPart mrRightTopMiddlePart = CMrCylinderDoorFrontManager.GetInstance().mRightTopMiddlePart;

            CMrPart mrLeftPart = CMrCylinderDoorFrontManager.GetInstance().mLeftPart;
            CMrPart mrLeftTopPart = CMrCylinderDoorFrontManager.GetInstance().mLeftTopPart;
            CMrPart mrLeftTopMiddlePart = CMrCylinderDoorFrontManager.GetInstance().mLeftTopMiddlePart;

            CMrPart mrTopPart = CMrCylinderDoorFrontManager.GetInstance().mTopPart;
            CMrPart mrZNormalBottomPart = CMrCylinderDoorFrontManager.GetInstance().mZNormalBottomPart;
            CMrPart mrYNormalBottomPart = CMrCylinderDoorFrontManager.GetInstance().mYNormalBottomPart;

            Vector xVector = new Vector(1, 0, 0);
            Vector yVector = new Vector(0, 1, 0);
            Vector zVector = new Vector(0, 0, 1);

            Vector normal = mMrPart.mNormal;

            CMrMark mrMark = new CMrMark();
            double dblAngle = CCommonPara.mPartMarkAngle;
            mrMark.mModelObject = mMrPart.mPartInDrawing;


            //0.如果是主梁;
            if (mMrPart == CMrMainBeam.GetInstance())
            {
                mrMark.mInsertPoint = mMrPart.mMidPoint;
                double increaseXDistance = 2 * CCommonPara.mPartMarkLength / Math.Tan(dblAngle);
                mrMark.mTextPoint.X = mrMark.mInsertPoint.X - increaseXDistance;
                mrMark.mTextPoint.Y = mrMark.mInsertPoint.Y + 2 * CCommonPara.mPartMarkLength;
                return mrMark;
            }
            //1.如果是左侧零部件或者是右侧零部件;
            if (mMrPart == mrLeftPart || mMrPart == mrRightPart)
            {
                return null;
            }
            //2.如果是主梁底部零部件;
            if (mMrPart == mrZNormalBottomPart)
            {
                mrMark.mInsertPoint = mMrPart.GetMinYMinXPoint();
                double increaseXDistance = CCommonPara.mPartMarkLength / Math.Tan(dblAngle);
                mrMark.mTextPoint.X = mrMark.mInsertPoint.X - increaseXDistance;
                mrMark.mTextPoint.Y = mrMark.mInsertPoint.Y - CCommonPara.mPartMarkLength;
                return mrMark;
            }
            //3.如果是主梁顶部的零部件;
            else if (mMrPart == mrTopPart)
            {
                mrMark.mInsertPoint = mMrPart.GetMaxXMaxYPoint();
                double increaseXDistance = CCommonPara.mPartMarkLength / Math.Tan(dblAngle);
                mrMark.mTextPoint.X = mrMark.mInsertPoint.X + increaseXDistance;
                mrMark.mTextPoint.Y = mrMark.mInsertPoint.Y + CCommonPara.mPartMarkLength;
                return mrMark;
            }
            //4.如果是主梁左上侧零部件;
            else if (mMrPart == mrLeftTopPart)
            {
                mrMark.mInsertPoint = mMrPart.mMidPoint;
                double increaseXDistance = CCommonPara.mPartMarkLength / Math.Tan(dblAngle);
                mrMark.mTextPoint.X = mrMark.mInsertPoint.X - increaseXDistance;
                mrMark.mTextPoint.Y = mrMark.mInsertPoint.Y + CCommonPara.mPartMarkLength;
                return mrMark;
            }
            //5.如果是主梁右上侧零部件;
            else if (mMrPart == mrRightTopPart)
            {
                mrMark.mInsertPoint = mMrPart.mMidPoint;
                double increaseXDistance = CCommonPara.mPartMarkLength / Math.Tan(dblAngle);
                mrMark.mTextPoint.X = mrMark.mInsertPoint.X + increaseXDistance;
                mrMark.mTextPoint.Y = mrMark.mInsertPoint.Y + CCommonPara.mPartMarkLength;
                return mrMark;
            }
            //6.如果是主梁左上侧中间零部件;
            else if(mMrPart==mrLeftTopMiddlePart)
            {
                mrMark.mInsertPoint = mMrPart.GetMinXMinYPoint();
                double increaseXDistance = CCommonPara.mPartMarkLength / Math.Tan(dblAngle);
                mrMark.mTextPoint.X = mrMark.mInsertPoint.X - increaseXDistance;
                mrMark.mTextPoint.Y = mrMark.mInsertPoint.Y - CCommonPara.mPartMarkLength;
                return mrMark;
            }
            //7.如果是主梁右上侧中间零部件;
            else if (mMrPart == mrRightTopMiddlePart)
            {
                mrMark.mInsertPoint = mMrPart.GetMaxXMinYPoint();
                double increaseXDistance = CCommonPara.mPartMarkLength / Math.Tan(dblAngle);
                mrMark.mTextPoint.X = mrMark.mInsertPoint.X + increaseXDistance;
                mrMark.mTextPoint.Y = mrMark.mInsertPoint.Y - CCommonPara.mPartMarkLength;
                return mrMark;
            }
            //8.如果零件的向量与X轴平行;
            if (CDimTools.GetInstance().IsTwoVectorParallel(normal, xVector))
            {
                mrMark = GerXNormalPartMark(mMrPart);

                return mrMark;
            }
            //9.如果零件的向量与Y轴平行;
            else if (CDimTools.GetInstance().IsTwoVectorParallel(normal, yVector))
            {
                mrMark = GetYNormalPartMark(mMrPart);

                return mrMark;
            }
            //10.如果零件的向量与Z轴平行;
            else if (CDimTools.GetInstance().IsTwoVectorParallel(normal, zVector))
            {
                mrMark = GetZNormalPartMark(mMrPart);

                return mrMark;
            }
            //11.如果零件的法向在XY面内;
            else if(CDimTools.GetInstance().IsVectorInXYPlane(normal))
            {
                mrMark = GetXYPlaneNormalPartMark(mMrPart);

                return mrMark;
            }
            return null;
        }

        /// <summary>
        /// 获取零件法向与X轴平行的零件标记;
        /// </summary>
        /// <param name="mrPart"></param>
        /// <returns></returns>
        private CMrMark GerXNormalPartMark(CMrPart mrPart)
        {
            CMrMark mrMark = new CMrMark();
            double dblAngle = CCommonPara.mPartMarkAngle;
            mrMark.mModelObject = mMrPart.mPartInDrawing;

            CMrPart mrYNormalBottomPart = CMrCylinderDoorFrontManager.GetInstance().mYNormalBottomPart;
            CMrPart mrRightPart = CMrCylinderDoorFrontManager.GetInstance().mRightPart;
            CMrPart mrRightTopPart = CMrCylinderDoorFrontManager.GetInstance().mRightTopPart;
            CMrPart mrRightTopMiddlePart = CMrCylinderDoorFrontManager.GetInstance().mRightTopMiddlePart;

            CMrPart mrLeftPart = CMrCylinderDoorFrontManager.GetInstance().mLeftPart;
            CMrPart mrLeftTopPart = CMrCylinderDoorFrontManager.GetInstance().mLeftTopPart;
            CMrPart mrLeftTopMiddlePart = CMrCylinderDoorFrontManager.GetInstance().mLeftTopMiddlePart;

            double minX = mMrPart.GetMinXPoint().X;
            double maxX = mMrPart.GetMaxXPoint().X;
            double minY = mMrPart.GetMinYPoint().Y;
            double yBottomPartMaxY = mrYNormalBottomPart.GetMaxYPoint().Y;

            //1.如果零件的底部和主梁下面的零件顶部重合;
            if (CDimTools.GetInstance().CompareTwoDoubleValue(minY, yBottomPartMaxY) == 0)
            {
                if (minX < 0)
                {
                    mrMark.mInsertPoint = mMrPart.GetMinXMinYPoint();
                    double increaseXDistance = CCommonPara.mPartMarkLength / Math.Tan(dblAngle);
                    mrMark.mTextPoint.X = mrMark.mInsertPoint.X - increaseXDistance;
                    mrMark.mTextPoint.Y = mrMark.mInsertPoint.Y - CCommonPara.mPartMarkLength;
                    return mrMark;
                }
                else
                {
                    mrMark.mInsertPoint = mMrPart.GetMaxXMinYPoint();
                    double increaseXDistance = CCommonPara.mPartMarkLength / Math.Tan(dblAngle);
                    mrMark.mTextPoint.X = mrMark.mInsertPoint.X + increaseXDistance;
                    mrMark.mTextPoint.Y = mrMark.mInsertPoint.Y - CCommonPara.mPartMarkLength;
                    return mrMark;
                }
            }
            else if(CDimTools.GetInstance().CompareTwoDoubleValue(maxX,mrLeftPart.GetMinXPoint().X) <= 0)
            {
                mrMark.mInsertPoint = mMrPart.GetMinXMinYPoint();
                double increaseXDistance = CCommonPara.mPartMarkLength / Math.Tan(dblAngle);
                mrMark.mTextPoint.X = mrMark.mInsertPoint.X - increaseXDistance;
                mrMark.mTextPoint.Y = mrMark.mInsertPoint.Y - CCommonPara.mPartMarkLength;
                return mrMark;
            }
            else if(CDimTools.GetInstance().CompareTwoDoubleValue(minX,mrRightPart.GetMaxXPoint().X) >= 0)
            {
                mrMark.mInsertPoint = mMrPart.GetMaxXMinYPoint();
                double increaseXDistance = CCommonPara.mPartMarkLength / Math.Tan(dblAngle);
                mrMark.mTextPoint.X = mrMark.mInsertPoint.X + increaseXDistance;
                mrMark.mTextPoint.Y = mrMark.mInsertPoint.Y - CCommonPara.mPartMarkLength;
                return mrMark;
            }
            else if (mrLeftTopPart != null && minY > mrLeftTopPart.GetMinYPoint().Y &&
                CDimTools.GetInstance().CompareTwoDoubleValue(maxX,mrLeftTopPart.GetMaxXPoint().X) <= 0)
            {
                mrMark.mInsertPoint = mMrPart.GetMinXMinYPoint();
                double increaseXDistance = CCommonPara.mPartMarkLength / Math.Tan(dblAngle);
                mrMark.mTextPoint.X = mrMark.mInsertPoint.X - increaseXDistance;
                mrMark.mTextPoint.Y = mrMark.mInsertPoint.Y - CCommonPara.mPartMarkLength;
                return mrMark;
            }
            else if (mrRightTopPart != null && minY > mrRightTopPart.GetMinYPoint().Y &&
                CDimTools.GetInstance().CompareTwoDoubleValue(minX, mrRightTopPart.GetMaxXPoint().X) >= 0)
            {
                mrMark.mInsertPoint = mMrPart.GetMaxXMinYPoint();
                double increaseXDistance = CCommonPara.mPartMarkLength / Math.Tan(dblAngle);
                mrMark.mTextPoint.X = mrMark.mInsertPoint.X + increaseXDistance;
                mrMark.mTextPoint.Y = mrMark.mInsertPoint.Y - CCommonPara.mPartMarkLength;
                return mrMark;
            }
            else if (mrLeftTopMiddlePart != null && minY > mrLeftTopMiddlePart.GetMinYPoint().Y &&
                CDimTools.GetInstance().CompareTwoDoubleValue(maxX, mrLeftTopMiddlePart.GetMaxXPoint().X) <= 0)
            {
                mrMark.mInsertPoint = mMrPart.GetMinXMinYPoint();
                double increaseXDistance = CCommonPara.mPartMarkLength / Math.Tan(dblAngle);
                mrMark.mTextPoint.X = mrMark.mInsertPoint.X - increaseXDistance;
                mrMark.mTextPoint.Y = mrMark.mInsertPoint.Y - CCommonPara.mPartMarkLength;
                return mrMark;
            }
            else if (mrRightTopMiddlePart != null && minY > mrRightTopMiddlePart.GetMinYPoint().Y &&
                CDimTools.GetInstance().CompareTwoDoubleValue(minX, mrRightTopMiddlePart.GetMaxXPoint().X) >= 0)
            {
                mrMark.mInsertPoint = mMrPart.GetMaxXMinYPoint();
                double increaseXDistance = CCommonPara.mPartMarkLength / Math.Tan(dblAngle);
                mrMark.mTextPoint.X = mrMark.mInsertPoint.X + increaseXDistance;
                mrMark.mTextPoint.Y = mrMark.mInsertPoint.Y - CCommonPara.mPartMarkLength;
                return mrMark;
            }
            return null;
        }

        /// <summary>
        /// 获取零件法向与Y轴平行的零件标记;
        /// </summary>
        /// <param name="mrPart"></param>
        /// <returns></returns>
        private CMrMark GetYNormalPartMark(CMrPart mrPart)
        {
            CMrMark mrMark = new CMrMark();
            double dblAngle = CCommonPara.mPartMarkAngle;
            mrMark.mModelObject = mMrPart.mPartInDrawing;

            CMrPart mrRightPart = CMrCylinderDoorFrontManager.GetInstance().mRightPart;
            CMrPart mrRightTopPart = CMrCylinderDoorFrontManager.GetInstance().mRightTopPart;
            CMrPart mrRightTopMiddlePart = CMrCylinderDoorFrontManager.GetInstance().mRightTopMiddlePart;

            CMrPart mrLeftPart = CMrCylinderDoorFrontManager.GetInstance().mLeftPart;
            CMrPart mrLeftTopPart = CMrCylinderDoorFrontManager.GetInstance().mLeftTopPart;
            CMrPart mrLeftTopMiddlePart = CMrCylinderDoorFrontManager.GetInstance().mLeftTopMiddlePart;

            double minX = mMrPart.GetMinXPoint().X;
            double maxX = mMrPart.GetMaxXPoint().X;
            double minY = mMrPart.GetMinYPoint().Y;

            //如果该零件存在于檩托板则不进行标记;
            CMrApronPlate mrApronPlate = CMrCylinderDoorFrontManager.GetInstance().FindMrApronPlateByYNormalPart(mMrPart);

            if(mrApronPlate!=null)
            {
                return null;
            }

            if (CDimTools.GetInstance().CompareTwoDoubleValue(maxX, mrLeftPart.GetMinXPoint().X) <= 0)
            {
                mrMark.mInsertPoint = mMrPart.GetMinXMinYPoint();
                double increaseXDistance = CCommonPara.mPartMarkLength / Math.Tan(dblAngle);
                mrMark.mTextPoint.X = mrMark.mInsertPoint.X - increaseXDistance;
                mrMark.mTextPoint.Y = mrMark.mInsertPoint.Y + CCommonPara.mPartMarkLength;
                return mrMark;
            }
            else if (CDimTools.GetInstance().CompareTwoDoubleValue(minX, mrRightPart.GetMaxXPoint().X) >= 0)
            {
                mrMark.mInsertPoint = mMrPart.GetMaxXMinYPoint();
                double increaseXDistance = CCommonPara.mPartMarkLength / Math.Tan(dblAngle);
                mrMark.mTextPoint.X = mrMark.mInsertPoint.X + increaseXDistance;
                mrMark.mTextPoint.Y = mrMark.mInsertPoint.Y + CCommonPara.mPartMarkLength;
                return mrMark;
            }
            else if (mrLeftTopPart != null && minY > mrLeftTopPart.GetMinYPoint().Y &&
                CDimTools.GetInstance().CompareTwoDoubleValue(maxX, mrLeftTopPart.GetMaxXPoint().X) <= 0)
            {
                mrMark.mInsertPoint = mMrPart.GetMinXMinYPoint();
                double increaseXDistance = CCommonPara.mPartMarkLength / Math.Tan(dblAngle);
                mrMark.mTextPoint.X = mrMark.mInsertPoint.X - increaseXDistance;
                mrMark.mTextPoint.Y = mrMark.mInsertPoint.Y + CCommonPara.mPartMarkLength;
                return mrMark;
            }
            else if (mrRightTopPart != null && minY > mrRightTopPart.GetMinYPoint().Y &&
                CDimTools.GetInstance().CompareTwoDoubleValue(minX, mrRightTopPart.GetMaxXPoint().X) >= 0)
            {
                mrMark.mInsertPoint = mMrPart.GetMaxXMinYPoint();
                double increaseXDistance = CCommonPara.mPartMarkLength / Math.Tan(dblAngle);
                mrMark.mTextPoint.X = mrMark.mInsertPoint.X + increaseXDistance;
                mrMark.mTextPoint.Y = mrMark.mInsertPoint.Y + CCommonPara.mPartMarkLength;
                return mrMark;
            }
            else if (mrLeftTopMiddlePart != null && minY > mrLeftTopMiddlePart.GetMinYPoint().Y &&
                CDimTools.GetInstance().CompareTwoDoubleValue(maxX, mrLeftTopMiddlePart.GetMaxXPoint().X) <= 0)
            {
                mrMark.mInsertPoint = mMrPart.GetMinXMinYPoint();
                double increaseXDistance = CCommonPara.mPartMarkLength / Math.Tan(dblAngle);
                mrMark.mTextPoint.X = mrMark.mInsertPoint.X - increaseXDistance;
                mrMark.mTextPoint.Y = mrMark.mInsertPoint.Y + CCommonPara.mPartMarkLength;
                return mrMark;
            }
            else if (mrRightTopMiddlePart != null && minY > mrRightTopMiddlePart.GetMinYPoint().Y &&
                CDimTools.GetInstance().CompareTwoDoubleValue(minX, mrRightTopMiddlePart.GetMaxXPoint().X) >= 0)
            {
                mrMark.mInsertPoint = mMrPart.GetMaxXMinYPoint();
                double increaseXDistance = CCommonPara.mPartMarkLength / Math.Tan(dblAngle);
                mrMark.mTextPoint.X = mrMark.mInsertPoint.X + increaseXDistance;
                mrMark.mTextPoint.Y = mrMark.mInsertPoint.Y + CCommonPara.mPartMarkLength;
                return mrMark;
            }
            else if (IsYNormalPartInMainBeamMiddle(mMrPart))
            {
                mrMark.mInsertPoint = mMrPart.GetMinXMinYPoint();
                double increaseXDistance = CCommonPara.mPartMarkLength / Math.Tan(dblAngle);
                mrMark.mTextPoint.X = mrMark.mInsertPoint.X - increaseXDistance;
                mrMark.mTextPoint.Y = mrMark.mInsertPoint.Y - CCommonPara.mPartMarkLength;
                return mrMark;
            }

            return null;
        }

        /// <summary>
        /// 获取零件法向与Z后平行的零件标记;
        /// </summary>
        /// <param name="mrPart"></param>
        /// <returns></returns>
        private CMrMark GetZNormalPartMark(CMrPart mrPart)
        {
            CMrMark mrMark = new CMrMark();
            double dblAngle = CCommonPara.mPartMarkAngle;
            mrMark.mModelObject = mMrPart.mPartInDrawing;

            CMrPart mrRightPart = CMrCylinderDoorFrontManager.GetInstance().mRightPart;
            CMrPart mrRightTopPart = CMrCylinderDoorFrontManager.GetInstance().mRightTopPart;
            CMrPart mrRightTopMiddlePart = CMrCylinderDoorFrontManager.GetInstance().mRightTopMiddlePart;

            CMrPart mrLeftPart = CMrCylinderDoorFrontManager.GetInstance().mLeftPart;
            CMrPart mrLeftTopPart = CMrCylinderDoorFrontManager.GetInstance().mLeftTopPart;
            CMrPart mrLeftTopMiddlePart = CMrCylinderDoorFrontManager.GetInstance().mLeftTopMiddlePart;

            CMrPart mrYNormalBottomPart = CMrCylinderDoorFrontManager.GetInstance().mYNormalBottomPart;
            CMrPart mrTopPart=CMrCylinderDoorFrontManager.GetInstance().mTopPart;

            double minX = mMrPart.GetMinXPoint().X;
            double minY = mMrPart.GetMinYPoint().Y;
            double maxX = mMrPart.GetMaxXPoint().X;
            double maxY = mMrPart.GetMaxYPoint().Y;

            double yBottomPartMaxY = mrYNormalBottomPart.GetMaxYPoint().Y;

            //如果该零件存在于檩托板则不进行标记;
            CMrApronPlate mrApronPlate = CMrCylinderDoorFrontManager.GetInstance().FindMrApronPlateByZNormalPart(mMrPart);

            if (mrApronPlate != null)
            {
                return null;
            }
            //1:如果零件的底部和主梁下面的零件顶部重合;
            if (CDimTools.GetInstance().CompareTwoDoubleValue(minY, yBottomPartMaxY) == 0)
            {
                if (minX > 0)
                {
                    mrMark.mInsertPoint = mMrPart.GetMaxXMaxYPoint();
                    double increaseXDistance = CCommonPara.mPartMarkLength / Math.Tan(dblAngle);
                    mrMark.mTextPoint.X = mrMark.mInsertPoint.X + increaseXDistance;
                    mrMark.mTextPoint.Y = mrMark.mInsertPoint.Y + CCommonPara.mPartMarkLength;
                    return mrMark;
                }
                else
                {
                    mrMark.mInsertPoint = mMrPart.GetMinXMaxYPoint();
                    double increaseXDistance = CCommonPara.mPartMarkLength / Math.Tan(dblAngle);
                    mrMark.mTextPoint.X = mrMark.mInsertPoint.X - increaseXDistance;
                    mrMark.mTextPoint.Y = mrMark.mInsertPoint.Y + CCommonPara.mPartMarkLength;
                    return mrMark;
                }
            }
            else if (CDimTools.GetInstance().CompareTwoDoubleValue(maxX, mrLeftPart.GetMinXPoint().X) <= 0)
            {
                Point insertPt = mMrPart.GetMinXMaxYPoint();
                mrMark.mInsertPoint = new Point(insertPt.X, insertPt.Y - 20, 0);
                double increaseXDistance = CCommonPara.mPartMarkLength / Math.Tan(dblAngle);
                mrMark.mTextPoint.X = mrMark.mInsertPoint.X - increaseXDistance;
                mrMark.mTextPoint.Y = mrMark.mInsertPoint.Y - CCommonPara.mPartMarkLength;
                return mrMark;
            }
            else if (CDimTools.GetInstance().CompareTwoDoubleValue(minX, mrRightPart.GetMaxXPoint().X) >= 0)
            {
                Point insertPt = mMrPart.GetMaxXMaxYPoint();
                mrMark.mInsertPoint = new Point(insertPt.X, insertPt.Y - 20, 0);
                double increaseXDistance = CCommonPara.mPartMarkLength / Math.Tan(dblAngle);
                mrMark.mTextPoint.X = mrMark.mInsertPoint.X + increaseXDistance;
                mrMark.mTextPoint.Y = mrMark.mInsertPoint.Y - CCommonPara.mPartMarkLength;
                return mrMark;
            }
            else if (mrLeftTopPart != null && minY > mrLeftTopPart.GetMinYPoint().Y &&
                CDimTools.GetInstance().CompareTwoDoubleValue(maxX, mrLeftTopPart.GetMaxXPoint().X) <= 0)
            {
                Point insertPt = mMrPart.GetMinXMaxYPoint();
                mrMark.mInsertPoint = new Point(insertPt.X, insertPt.Y - 20, 0);
                double increaseXDistance = CCommonPara.mPartMarkLength / Math.Tan(dblAngle);
                mrMark.mTextPoint.X = mrMark.mInsertPoint.X - increaseXDistance;
                mrMark.mTextPoint.Y = mrMark.mInsertPoint.Y - CCommonPara.mPartMarkLength;
                return mrMark;
            }
            else if (mrRightTopPart != null && minY > mrRightTopPart.GetMinYPoint().Y &&
                CDimTools.GetInstance().CompareTwoDoubleValue(minX, mrRightTopPart.GetMaxXPoint().X) >= 0)
            {
                Point insertPt = mMrPart.GetMaxXMaxYPoint();
                mrMark.mInsertPoint = new Point(insertPt.X, insertPt.Y - 20, 0);
                double increaseXDistance = CCommonPara.mPartMarkLength / Math.Tan(dblAngle);
                mrMark.mTextPoint.X = mrMark.mInsertPoint.X + increaseXDistance;
                mrMark.mTextPoint.Y = mrMark.mInsertPoint.Y - CCommonPara.mPartMarkLength;
                return mrMark;
            }
            else if (mrLeftTopMiddlePart != null && minY > mrLeftTopMiddlePart.GetMinYPoint().Y &&
                CDimTools.GetInstance().CompareTwoDoubleValue(maxX, mrLeftTopMiddlePart.GetMaxXPoint().X) <= 0)
            {
                Point insertPt = mMrPart.GetMinXMaxYPoint();
                mrMark.mInsertPoint = new Point(insertPt.X, insertPt.Y - 20, 0);
                double increaseXDistance = CCommonPara.mPartMarkLength / Math.Tan(dblAngle);
                mrMark.mTextPoint.X = mrMark.mInsertPoint.X - increaseXDistance;
                mrMark.mTextPoint.Y = mrMark.mInsertPoint.Y - CCommonPara.mPartMarkLength;
                return mrMark;
            }
            else if (mrRightTopMiddlePart != null && minY > mrRightTopMiddlePart.GetMinYPoint().Y &&
                CDimTools.GetInstance().CompareTwoDoubleValue(minX, mrRightTopMiddlePart.GetMaxXPoint().X) >= 0)
            {
                Point insertPt = mMrPart.GetMaxXMaxYPoint();
                mrMark.mInsertPoint = new Point(insertPt.X, insertPt.Y - 20, 0);
                double increaseXDistance = CCommonPara.mPartMarkLength / Math.Tan(dblAngle);
                mrMark.mTextPoint.X = mrMark.mInsertPoint.X + increaseXDistance;
                mrMark.mTextPoint.Y = mrMark.mInsertPoint.Y - CCommonPara.mPartMarkLength;
                return mrMark;
            }
            //如果是顶端法向与z轴平行的板;
            if(maxY>mrTopPart.GetMaxYPoint().Y)
            {
                if (minX > 0)
                {
                    mrMark.mInsertPoint = mMrPart.mMidPoint;
                    double increaseXDistance = CCommonPara.mPartMarkLength / Math.Tan(dblAngle);
                    mrMark.mTextPoint.X = mrMark.mInsertPoint.X + increaseXDistance;
                    mrMark.mTextPoint.Y = mrMark.mInsertPoint.Y + CCommonPara.mPartMarkLength;
                    return mrMark;
                }
                else
                {
                    mrMark.mInsertPoint = mMrPart.mMidPoint;
                    double increaseXDistance = CCommonPara.mPartMarkLength / Math.Tan(dblAngle);
                    mrMark.mTextPoint.X = mrMark.mInsertPoint.X - increaseXDistance;
                    mrMark.mTextPoint.Y = mrMark.mInsertPoint.Y + CCommonPara.mPartMarkLength;
                    return mrMark;
                }
            }

            return null;
        }
    
        /// <summary>
        /// 获取在零件法向在XY平面内的零件标记;
        /// </summary>
        /// <param name="mrPart"></param>
        /// <returns></returns>
        private CMrMark GetXYPlaneNormalPartMark(CMrPart mrPart)
        {
            double maxY = mMrPart.GetMaxYPoint().Y;
            double minX = mMrPart.GetMinXPoint().X;
            double maxX = mMrPart.GetMaxXPoint().X;

            CMrPart mrTopPart = CMrCylinderDoorFrontManager.GetInstance().mTopPart;

            CMrMark mrMark = new CMrMark();
            double dblAngle = CCommonPara.mPartMarkAngle;
            mrMark.mModelObject = mMrPart.mPartInDrawing;

            if(mrTopPart!=null)
            {
                if (maxY > mrTopPart.GetMaxYPoint().Y)
                {
                    mrMark.mInsertPoint = mMrPart.GetMaxYPoint();
                    double increaseXDistance = CCommonPara.mPartMarkLength / Math.Tan(dblAngle);
                    mrMark.mTextPoint.X = mrMark.mInsertPoint.X + increaseXDistance;
                    mrMark.mTextPoint.Y = mrMark.mInsertPoint.Y + CCommonPara.mPartMarkLength;
                    return mrMark;
                }
            }
            if (minX > 0)
            {
                mrMark.mInsertPoint = mMrPart.mMidPoint;
                double increaseXDistance = CCommonPara.mPartMarkLength / Math.Tan(dblAngle);
                mrMark.mTextPoint.X = mrMark.mInsertPoint.X + increaseXDistance;
                mrMark.mTextPoint.Y = mrMark.mInsertPoint.Y - CCommonPara.mPartMarkLength;
                return mrMark;
            }
            else if (minX < 0)
            {
                mrMark.mInsertPoint = mMrPart.mMidPoint;
                double increaseXDistance = CCommonPara.mPartMarkLength / Math.Tan(dblAngle);
                mrMark.mTextPoint.X = mrMark.mInsertPoint.X - increaseXDistance;
                mrMark.mTextPoint.Y = mrMark.mInsertPoint.Y - CCommonPara.mPartMarkLength;
                return mrMark;
            }
            return null;
        }
    }
}
