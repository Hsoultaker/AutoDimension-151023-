using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tekla.Structures.Geometry3d;


namespace AutoDimension.Entity
{
    public class CBeamDoorFrontViewInfo
    {
        /// <summary>
        /// 与该FrontView视图关联的Part对象;
        /// </summary>
        private CMrPart mMrPart;

         /// <summary>
        /// 门式框架顶视图信息;
        /// </summary>
        /// <param name="mrPart"></param>
        public CBeamDoorFrontViewInfo(CMrPart mrPart)
        {
            this.mMrPart = mrPart;
        }

        /// <summary>
        /// 判断是否需要在上面进行标注;
        /// </summary>
        /// <returns></returns>
        protected bool IsNeedXUpDimNormal()
        {
            Vector normal = mMrPart.mNormal;

            if (CDimTools.GetInstance().IsTwoVectorParallel(normal, new Vector(0, 0, 1)))
            {
                return false;
            }

            double partHeight = Math.Abs(mMrPart.GetMaxYPoint().Y - mMrPart.GetMinYPoint().Y);

            double mainBeamHeight = Math.Abs(CMrMainBeam.GetInstance().GetMaxYPoint().Y - CMrMainBeam.GetInstance().GetMinYPoint().Y);

            CMrPart mTopBeam = CMrBeamDoorManager.GetInstance().mTopBeam;

            Vector topBeamNormal = mTopBeam.mNormal;

            Point leftTopPoint = CMrMainBeam.GetInstance().mLeftTopPoint;
            Point rightTopPoint = CMrMainBeam.GetInstance().mRightTopPoint;
            Point rightBottomPoint = CMrMainBeam.GetInstance().mRightBottomPoint;

            Point partMinY = mMrPart.GetMinYPoint();
            Point partMaxY = mMrPart.GetMaxYPoint();

            //(1).如果是零件是左右两侧的板;
            if (mMrPart == CMrBeamDoorManager.GetInstance().mLeftBeam || mMrPart==CMrBeamDoorManager.GetInstance().mRightBeam)
            {       
                return false;
            }
            //(2).如果零件在主梁的上方;
            if (CDimTools.GetInstance().IsThePointOnLine(partMinY, leftTopPoint, rightTopPoint) > 0)
            {
                if (CDimTools.GetInstance().IsTwoVectorVertical(normal, topBeamNormal))
                {
                    CMrBeamDoorManager.GetInstance().AppendUpDimPart(mMrPart);

                    return true;
                }
            }
            //(3).如果零件与主梁对角线相交,并且与地面垂直;  
            if (CDimTools.GetInstance().IsThePointOnLine(partMaxY, leftTopPoint, rightBottomPoint) > 0 &&
                CDimTools.GetInstance().IsThePointOnLine(partMinY, leftTopPoint, rightBottomPoint) < 0 &&
                partHeight > mainBeamHeight / 3.0)
            {
                if (CDimTools.GetInstance().IsTwoVectorParallel(normal, new Vector(1, 0, 0)))
                {
                    CMrBeamDoorManager.GetInstance().AppendUpDimPart(mMrPart);
                    return true;
                }
            }
            return false;
        }

         /// <summary>
        /// 判断是否需要在下面进行标注;
        /// </summary>
        /// <returns></returns>
        protected bool IsNeedXDownDimNormal()
        {
            Vector normal = mMrPart.mNormal;

            if (CDimTools.GetInstance().IsTwoVectorParallel(normal, new Vector(0, 0, 1)))
            {
                return false;
            }

            double partHeight = Math.Abs(mMrPart.GetMaxYPoint().Y - mMrPart.GetMinYPoint().Y);
            double mainBeamHeight = Math.Abs(CMrMainBeam.GetInstance().GetMaxYPoint().Y - CMrMainBeam.GetInstance().GetMinYPoint().Y);

            CMrPart mBottomBeam = CMrBeamDoorManager.GetInstance().mBottonBeam;

            Point leftBottomPoint = CMrMainBeam.GetInstance().mLeftTopPoint;
            Point rightBottomPoint = CMrMainBeam.GetInstance().mRightBottomPoint;
            Point partMaxY = mMrPart.GetMaxYPoint();
            Point partMinY = mMrPart.GetMinYPoint();

            //(1).如果是零件是左右两侧的板;
            if (mMrPart == CMrBeamDoorManager.GetInstance().mLeftBeam || mMrPart == CMrBeamDoorManager.GetInstance().mRightBeam)
            {
                return false;
            }
            //(2).如果零件在主梁的下方,并且与地面垂直;
            if (CDimTools.GetInstance().IsThePointOnLine(partMaxY, leftBottomPoint, rightBottomPoint) < 0 &&
                CDimTools.GetInstance().IsTwoVectorParallel(normal, new Vector(1, 0, 0)))
            {
                return true;
            }
            //(3).如果零件在主梁中间，并且与地面垂直;
            if (CDimTools.GetInstance().IsThePointOnLine(partMaxY, leftBottomPoint, rightBottomPoint) > 0 &&
                CDimTools.GetInstance().IsTwoVectorParallel(normal, new Vector(1, 0, 0)) && partHeight > mainBeamHeight / 2.0)
            {
                return true;
            }
            //(4).如果零件在主梁下方，并且与地面水平;
            if (mMrPart.GetMinYPoint().Y < mBottomBeam.GetMinYPoint().Y &&
                CDimTools.GetInstance().IsTwoVectorParallel(normal, new Vector(0, 1, 0)))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 获取顶视图上方的标注集合;
        /// </summary>
        /// <returns></returns>
        public CMrDimSet GetXUpDimSetNormal()
        {
            if (!IsNeedXUpDimNormal())
            {
                return null;
            }

            CMrDimSet mrDimSet = new CMrDimSet();

            CMrPart topBeam = CMrBeamDoorManager.GetInstance().mTopBeam;

            Vector normal = mMrPart.mNormal;
            Vector topBeamNormal = topBeam.mNormal;

            Point leftTopPoint = topBeam.mLeftTopPoint;
            Point rightTopPoint = topBeam.mRightTopPoint;

            MrSlopeType slopeType = CDimTools.GetInstance().JudgeLineSlope(leftTopPoint, rightTopPoint);

            if (slopeType == MrSlopeType.MORETHAN_ZERO)
            {
                if (CDimTools.GetInstance().IsTwoVectorParallel(normal, new Vector(1, 0, 0)))
                {
                    mrDimSet.AddPoint(mMrPart.GetMaxXMaxYPoint());
                }
                else
                {
                     mrDimSet.AddPoint(mMrPart.mRightTopPoint);
                }
            }
            else if (slopeType == MrSlopeType.LESSTHAN_ZERO)
            {
                if (CDimTools.GetInstance().IsTwoVectorParallel(normal, new Vector(1, 0, 0)))
                {
                    mrDimSet.AddPoint(mMrPart.GetMinXMaxYPoint());
                }
                else
                {
                    mrDimSet.AddPoint(mMrPart.mLeftTopPoint);
                }
            }
            else
            {
                mrDimSet.AddPoint(mMrPart.mLeftTopPoint);
            }
            
            return mrDimSet;
        }

        /// <summary>
        /// 获取顶视图下方的标注集合;
        /// </summary>
        /// <returns></returns>
        public CMrDimSet GetXDownDimSetNormal()
        {
            if (!IsNeedXDownDimNormal())
            {
                return null;
            }

            Vector normal = mMrPart.mNormal;

            CMrDimSet mrDimSet = new CMrDimSet();

            if (CDimTools.GetInstance().IsTwoVectorParallel(normal, new Vector(1, 0, 0)))
            {
                mrDimSet.AddPoint(mMrPart.mLeftBottomPoint);
            }
            else if (CDimTools.GetInstance().IsTwoVectorParallel(normal, new Vector(0, 1, 0)))
            {
                mrDimSet.AddPoint(mMrPart.mLeftBottomPoint);
                mrDimSet.AddPoint(mMrPart.mRightBottomPoint);
            }

            return mrDimSet;
        }

        /// <summary>
        /// 判断该板是否需要进行标注;
        /// </summary>
        /// <returns></returns>
        protected bool IsNeedXUpDimMiddle()
        {
            Vector normal = mMrPart.mNormal;

            if (CDimTools.GetInstance().IsTwoVectorParallel(normal, new Vector(0, 0, 1)))
            {
                return false;
            }

            CMrPart mTopBeam = CMrBeamDoorManager.GetInstance().mTopBeam;

            Vector leftTopVector = CMrBeamDoorManager.GetInstance().mLeftTopVector;
            Vector rightTopVector = CMrBeamDoorManager.GetInstance().mRightTopVector;
           
            Point leftBottom = mTopBeam.mLeftBottomPoint;
            Point rightBottomPoint = mTopBeam.mRightBottomPoint;
            Point midTopPoint = CMrBeamDoorManager.GetInstance().mMidMaxPoint;

            Point partMinY = mMrPart.GetMinYPoint();
           
            //(1).如果是零件是左右两侧的板;
            if (mMrPart == CMrBeamDoorManager.GetInstance().mLeftBeam ||
                mMrPart == CMrBeamDoorManager.GetInstance().mRightBeam)
            {
                return false;
            }

            //(2).如果零件在主梁的左上方;
            if (CDimTools.GetInstance().IsThePointOnLine(partMinY, leftBottom, midTopPoint) > 0)
            {
                if (CDimTools.GetInstance().IsTwoVectorVertical(normal, leftTopVector))
                {
                    CMrBeamDoorManager.GetInstance().AppendUpDimPart(mMrPart);

                    return true;
                }
            }
            //(3).如果零件在主梁的右上方;
            if (CDimTools.GetInstance().IsThePointOnLine(partMinY, rightBottomPoint, midTopPoint) > 0)
            {
                if (CDimTools.GetInstance().IsTwoVectorVertical(normal, rightTopVector))
                {
                    CMrBeamDoorManager.GetInstance().AppendUpDimPart(mMrPart);

                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 获得左侧向上的标注集;
        /// </summary>
        /// <returns></returns>
        public CMrDimSet GetLeftXUpDimSetMiddle()
        {
            if (!IsNeedXUpDimMiddle())
            {
                return null;
            }

            Point minXPoint = mMrPart.GetMinXPoint();

            Point midTopPoint=CMrBeamDoorManager.GetInstance().mMidMaxPoint;

            if (minXPoint.X > midTopPoint.X)
            {
                return null;
            }

            Vector normal = mMrPart.mNormal;

            CMrDimSet mrDimSet = new CMrDimSet();

            if (CDimTools.GetInstance().IsTwoVectorParallel(normal, new Vector(1, 0, 0)))
            {
                mrDimSet.AddPoint(mMrPart.GetMinXMaxYPoint());
            }
            else
            {
                MrBeamDoorType beamDoorType = CMrBeamDoorManager.GetInstance().mType;

                if (beamDoorType == MrBeamDoorType.TypeMiddle1)
                {
                    mrDimSet.AddPoint(mMrPart.mRightTopPoint);
                }
                else if (beamDoorType == MrBeamDoorType.TypeMiddle2)
                {
                    mrDimSet.AddPoint(mMrPart.mLeftTopPoint);
                }
            }
            return mrDimSet;
        }

        /// <summary>
        /// 得到右侧向上的零部件;
        /// </summary>
        /// <returns></returns>
        public CMrDimSet GetRightXUpDimSetMiddle()
        {
            if (!IsNeedXUpDimMiddle())
            {
                return null;
            }

            Point maxXPoint = mMrPart.GetMaxXPoint();

            Point midTopPoint = CMrBeamDoorManager.GetInstance().mMidMaxPoint;

            if (maxXPoint.X < midTopPoint.X)
            {
                return null;
            }

            Vector normal = mMrPart.mNormal;

            CMrDimSet mrDimSet = new CMrDimSet();

            if (CDimTools.GetInstance().IsTwoVectorParallel(normal, new Vector(1, 0, 0)))
            {
                mrDimSet.AddPoint(mMrPart.GetMaxXMaxYPoint());
            }
            else
            {
                MrBeamDoorType beamDoorType = CMrBeamDoorManager.GetInstance().mType;

                if (beamDoorType == MrBeamDoorType.TypeMiddle1)
                {
                    mrDimSet.AddPoint(mMrPart.mLeftTopPoint);
                }
                else if (beamDoorType == MrBeamDoorType.TypeMiddle2)
                {
                    mrDimSet.AddPoint(mMrPart.mRightTopPoint);
                }
            }
            return mrDimSet;
        }

        /// <summary>
        /// 获取主梁为类型3时的标注集合;
        /// </summary>
        /// <returns></returns>
        public CMrDimSet GetTypeMiddle3XUpDimSetMiddle()
        {
            if (!IsNeedXUpDimMiddle())
            {
                return null;
            }

            Point maxXPoint = mMrPart.GetMaxXPoint();
            
            Vector normal = mMrPart.mNormal;

            CMrPart topBeam = CMrBeamDoorManager.GetInstance().mTopBeam;

            Vector topBeamNormal = topBeam.mNormal;

            CMrDimSet mrDimSet = new CMrDimSet();

            if (CDimTools.GetInstance().IsTwoVectorParallel(normal, new Vector(1, 0, 0)))
            {
                mrDimSet.AddPoint(mMrPart.GetMaxXMaxYPoint());
            }
            else
            {
                MrBeamDoorType beamDoorType = CMrBeamDoorManager.GetInstance().mType;

                MrSlopeType mrSlopeType = CDimTools.GetInstance().JudgeLineSlope(new Point(0, 0, 0), topBeamNormal);

                if (mrSlopeType == MrSlopeType.MORETHAN_ZERO)
                {
                    mrDimSet.AddPoint(mMrPart.mLeftTopPoint);
                }
                else if (mrSlopeType == MrSlopeType.LESSTHAN_ZERO)
                {
                    mrDimSet.AddPoint(mMrPart.mRightTopPoint);
                }
            }
            return mrDimSet;
        }


        /// <summary>
        /// 判断当上翼板向两边弯时该板是否需要进行标注;
        /// </summary>
        /// <returns></returns>
        protected bool IsNeedXDownDimMiddle()
        {
            Vector normal = mMrPart.mNormal;

            if (CDimTools.GetInstance().IsTwoVectorParallel(normal, new Vector(0, 0, 1)))
            {
                return false;
            }

            Point leftBottomPoint = CMrMainBeam.GetInstance().mLeftTopPoint;
            Point rightBottomPoint = CMrMainBeam.GetInstance().mRightBottomPoint;
            Point partMaxY = mMrPart.GetMaxYPoint();

            //(1).如果是零件是左右两侧的板;
            if (mMrPart == CMrBeamDoorManager.GetInstance().mLeftBeam ||
                mMrPart == CMrBeamDoorManager.GetInstance().mRightBeam)
            {
                return false;
            }
            //(2).如果是下面的左右侧的板;
            if( mMrPart == CMrBeamDoorManager.GetInstance().mLeftBottomBeam||
                mMrPart == CMrBeamDoorManager.GetInstance().mRightBottomBeam)
            {
                return true;
            }
            //(2).如果零件与地面垂直;
            if (CDimTools.GetInstance().IsTwoVectorParallel(normal, new Vector(1, 0, 0)))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 获取上翼板向两边弯时顶视图下方的标注集合;
        /// </summary>
        /// <returns></returns>
        public CMrDimSet GetXDownDimSetMiddle()
        {
            if (!IsNeedXDownDimMiddle())
            {
                return null;
            }

            CMrDimSet mrDimSet = new CMrDimSet();

            CMrPart leftBottomBeam = CMrBeamDoorManager.GetInstance().mLeftBottomBeam;
            CMrPart rightBottomBeam = CMrBeamDoorManager.GetInstance().mRightBottomBeam;

            Point midMaxPoint=CMrBeamDoorManager.GetInstance().mMidMaxPoint;

            if(mMrPart==leftBottomBeam)
            {
                mrDimSet.AddPoint(mMrPart.mRightBottomPoint);
            }
            else if (mMrPart == rightBottomBeam)
            {
                mrDimSet.AddPoint(mMrPart.mLeftBottomPoint);
            }
            else 
            {
                if (mMrPart.mLeftBottomPoint.X < midMaxPoint.X)
                {
                    mrDimSet.AddPoint(mMrPart.mLeftBottomPoint);
                }
                else if (mMrPart.mLeftBottomPoint.X > midMaxPoint.X)
                {
                    mrDimSet.AddPoint(mMrPart.mRightBottomPoint);
                }
            }
            return mrDimSet;
        }

        /// <summary>
        /// 获取上翼板左斜或者右斜时的零件标记;
        /// </summary>
        /// <returns></returns>
        public CMrMark GetPartMarkNormal()
        {
            CMrPart topBeam = CMrBeamDoorManager.GetInstance().mTopBeam;
            CMrPart bottomBeam = CMrBeamDoorManager.GetInstance().mBottonBeam;
            CMrPart leftBeam = CMrBeamDoorManager.GetInstance().mLeftBeam;
            CMrPart rightBeam = CMrBeamDoorManager.GetInstance().mRightBeam;
            
            Point minY = mMrPart.GetMinYPoint();
            Point maxY = mMrPart.GetMaxYPoint();
            Vector normal = mMrPart.mNormal;

            CMrMark mrMark = new CMrMark();
            double dblAngle = CCommonPara.mPartMarkAngle;
            mrMark.mModelObject = mMrPart.mPartInDrawing;

            if (mMrPart == leftBeam)
            {
                mrMark.mInsertPoint = mMrPart.mMidPoint;
                double increaseXDistance = 1.8 * CCommonPara.mPartMarkLength / Math.Tan(dblAngle);
                mrMark.mTextPoint.X = mrMark.mInsertPoint.X - increaseXDistance;
                mrMark.mTextPoint.Y = mrMark.mInsertPoint.Y + 1.8 * CCommonPara.mPartMarkLength;
                return mrMark;
            }
            if (mMrPart == rightBeam)
            {
                mrMark.mInsertPoint = mMrPart.mMidPoint;
                double increaseXDistance = 1.8 * CCommonPara.mPartMarkLength / Math.Tan(dblAngle);
                mrMark.mTextPoint.X = mrMark.mInsertPoint.X + increaseXDistance;
                mrMark.mTextPoint.Y = mrMark.mInsertPoint.Y + 1.8 * CCommonPara.mPartMarkLength;
                return mrMark;
            }
            if (mMrPart == topBeam || mMrPart == bottomBeam)
            {
                mrMark.mInsertPoint = mMrPart.mMidPoint;
                double increaseXDistance = CCommonPara.mPartMarkLength / Math.Tan(dblAngle);
                mrMark.mTextPoint.X = mrMark.mInsertPoint.X + increaseXDistance;
                mrMark.mTextPoint.Y = mrMark.mInsertPoint.Y - CCommonPara.mPartMarkLength;
                return mrMark;
            }
            //如果向量与Z轴平行;
            if (CDimTools.GetInstance().IsTwoVectorParallel(normal, new Vector(0, 0, 1)))
            {
                //如果是在上翼板的上方;
                if (CDimTools.GetInstance().IsThePointOnLine(maxY, topBeam.mLeftTopPoint, topBeam.mRightTopPoint) > 0)
                {
                    //1.如果是靠右侧挡板的那块板;
                    if (Math.Abs(mMrPart.GetMaxYPoint().X - rightBeam.mLeftTopPoint.X) < 5)
                    {
                        double x = mMrPart.GetMaxXPoint().X - 20;
                        double y = mMrPart.GetMinYPoint().Y + 20;
                        Point insertPoint = new Point(x, y, 0);
                        mrMark.mInsertPoint = insertPoint;
                        double increaseXDistance = 1.2 * CCommonPara.mPartMarkLength / Math.Tan(dblAngle);
                        mrMark.mTextPoint.X = mrMark.mInsertPoint.X + increaseXDistance;
                        mrMark.mTextPoint.Y = mrMark.mInsertPoint.Y + 1.2 * CCommonPara.mPartMarkLength;
                        return mrMark;
                    }
                    //2.如果是靠左侧挡板的那块板;
                    if (Math.Abs(mMrPart.GetMaxYPoint().X - leftBeam.mRightTopPoint.X) < 5)
                    {
                        double x = mMrPart.GetMaxXPoint().X - 20;
                        double y = mMrPart.GetMinYPoint().Y + 20;
                        Point insertPoint = new Point(x, y, 0);
                        mrMark.mInsertPoint = insertPoint;
                        double increaseXDistance = 1.2 * CCommonPara.mPartMarkLength / Math.Tan(dblAngle);
                        mrMark.mTextPoint.X = mrMark.mInsertPoint.X - increaseXDistance;
                        mrMark.mTextPoint.Y = mrMark.mInsertPoint.Y + 1.2 * CCommonPara.mPartMarkLength;
                        return mrMark;
                    }
                    else
                    {
                        MrSlopeType mrSlopeType = CDimTools.GetInstance().JudgeLineSlope(new Point(0,0,0),topBeam.mNormal);

                        if (mrSlopeType == MrSlopeType.MORETHAN_ZERO)
                        {
                            double x = mMrPart.GetMinXPoint().X + 20;
                            double y = mMrPart.GetMinYPoint().Y + 20;
                            Point insertPoint = new Point(x, y, 0);
                            mrMark.mInsertPoint = insertPoint;
                            double increaseXDistance = CCommonPara.mPartMarkLength / Math.Tan(dblAngle);
                            mrMark.mTextPoint.X = mrMark.mInsertPoint.X + 1.1 * increaseXDistance;
                            mrMark.mTextPoint.Y = mrMark.mInsertPoint.Y + 0.8 * CCommonPara.mPartMarkLength;
                            return mrMark;
                        }
                        else if (mrSlopeType == MrSlopeType.LESSTHAN_ZERO)
                        {
                            double x = mMrPart.GetMinXPoint().X + 20;
                            double y = mMrPart.GetMinYPoint().Y + 20;
                            Point insertPoint = new Point(x, y, 0);
                            mrMark.mInsertPoint = insertPoint;
                            double increaseXDistance = CCommonPara.mPartMarkLength / Math.Tan(dblAngle);
                            mrMark.mTextPoint.X = mrMark.mInsertPoint.X - 1.1 * increaseXDistance;
                            mrMark.mTextPoint.Y = mrMark.mInsertPoint.Y + 0.8 * CCommonPara.mPartMarkLength;
                            return mrMark;
                        }
                    }
                }
                //如果在下翼板下方;
                if (CDimTools.GetInstance().IsThePointOnLine(minY, bottomBeam.mLeftBottomPoint, bottomBeam.mRightBottomPoint) < 0)
                {
                    //1.如果是靠右侧挡板的那块板;
                    if (Math.Abs(mMrPart.GetMinYPoint().X - rightBeam.mLeftBottomPoint.X) < 5)
                    {
                        double x = mMrPart.GetMaxXPoint().X - 20;
                        double y = mMrPart.GetMaxYPoint().Y - 20;
                        Point insertPoint = new Point(x, y, 0);
                        mrMark.mInsertPoint = insertPoint;
                        double increaseXDistance = 1.2 * CCommonPara.mPartMarkLength / Math.Tan(dblAngle);
                        mrMark.mTextPoint.X = mrMark.mInsertPoint.X + increaseXDistance;
                        mrMark.mTextPoint.Y = mrMark.mInsertPoint.Y - 1.2 * CCommonPara.mPartMarkLength;
                        return mrMark;
                    }
                    //2.如果是靠左侧挡板的那块板;
                    else if (Math.Abs(mMrPart.GetMinYPoint().X - leftBeam.mRightBottomPoint.X) < 5)
                    {
                        double x = mMrPart.GetMaxXPoint().X - 20;
                        double y = mMrPart.GetMaxYPoint().Y - 20;
                        Point insertPoint = new Point(x, y, 0);
                        mrMark.mInsertPoint = insertPoint;
                        double increaseXDistance = 1.2 * CCommonPara.mPartMarkLength / Math.Tan(dblAngle);
                        mrMark.mTextPoint.X = mrMark.mInsertPoint.X - increaseXDistance;
                        mrMark.mTextPoint.Y = mrMark.mInsertPoint.Y - 1.2 * CCommonPara.mPartMarkLength;
                        return mrMark;
                    }
                    else
                    {
                        double x = mMrPart.GetMinXPoint().X + 20;
                        double y = mMrPart.GetMaxYPoint().Y - 20;
                        Point insertPoint = new Point(x, y, 0);
                        mrMark.mInsertPoint = insertPoint;
                        double increaseXDistance = CCommonPara.mPartMarkLength / Math.Tan(dblAngle);
                        mrMark.mTextPoint.X = mrMark.mInsertPoint.X + 1.1 * increaseXDistance;
                        mrMark.mTextPoint.Y = mrMark.mInsertPoint.Y - 0.8 * CCommonPara.mPartMarkLength;
                        return mrMark;
                    }
                }
            }
            else if (CDimTools.GetInstance().IsTwoVectorParallel(normal, new Vector(1, 0, 0)))
            {
                mrMark.mInsertPoint = mMrPart.mRightBottomPoint;
                double increaseXDistance = CCommonPara.mPartMarkLength / Math.Tan(dblAngle);
                mrMark.mTextPoint.X = mrMark.mInsertPoint.X + increaseXDistance;
                mrMark.mTextPoint.Y = mrMark.mInsertPoint.Y - CCommonPara.mPartMarkLength;
                return mrMark;
            }
            //如果法向量与上翼板的法向垂直;
            else if(CDimTools.GetInstance().IsTwoVectorVertical(normal,topBeam.mNormal))
            {
                //如果在上翼板的上方;
                if (CDimTools.GetInstance().IsThePointOnLine(maxY, topBeam.mLeftTopPoint, topBeam.mRightTopPoint) > 0)
                {
                     MrSlopeType mrSlopeType = CDimTools.GetInstance().JudgeLineSlope(new Point(0,0,0),topBeam.mNormal);

                     if (mrSlopeType == MrSlopeType.MORETHAN_ZERO)
                     {
                         mrMark.mInsertPoint = mMrPart.mRightTopPoint;
                         double increaseXDistance = 1.2 * CCommonPara.mPartMarkLength / Math.Tan(dblAngle);
                         mrMark.mTextPoint.X = mrMark.mInsertPoint.X + increaseXDistance;
                         mrMark.mTextPoint.Y = mrMark.mInsertPoint.Y + 1.2 * CCommonPara.mPartMarkLength;
                         return mrMark;
                     }
                     else if (mrSlopeType == MrSlopeType.LESSTHAN_ZERO)
                     {
                         mrMark.mInsertPoint = mMrPart.mRightTopPoint;
                         double increaseXDistance = 1.2 * CCommonPara.mPartMarkLength / Math.Tan(dblAngle);
                         mrMark.mTextPoint.X = mrMark.mInsertPoint.X - increaseXDistance;
                         mrMark.mTextPoint.Y = mrMark.mInsertPoint.Y + 1.2 * CCommonPara.mPartMarkLength;
                         return mrMark;
                     }
                }
                else if (CDimTools.GetInstance().IsThePointOnLine(maxY, bottomBeam.mLeftTopPoint, bottomBeam.mRightTopPoint) > 0)
                {
                    mrMark.mInsertPoint = mMrPart.mRightBottomPoint;
                    double increaseXDistance = CCommonPara.mPartMarkLength / Math.Tan(dblAngle);
                    mrMark.mTextPoint.X = mrMark.mInsertPoint.X + increaseXDistance;
                    mrMark.mTextPoint.Y = mrMark.mInsertPoint.Y - 1.2 * CCommonPara.mPartMarkLength;
                    return mrMark;
                }
            }
            else if (CDimTools.GetInstance().IsTwoVectorParallel(normal, topBeam.mNormal) && mMrPart.IsHaveBolt())
            {
                mrMark.mInsertPoint = mMrPart.mRightBottomPoint;
                double increaseXDistance = CCommonPara.mPartMarkLength / Math.Tan(dblAngle);
                mrMark.mTextPoint.X = mrMark.mInsertPoint.X + increaseXDistance;
                mrMark.mTextPoint.Y = mrMark.mInsertPoint.Y - CCommonPara.mPartMarkLength;
                return mrMark;
            }
            return null;
        }

        /// <summary>
        /// 获取上翼板是向两边弯曲时的零件标记;
        /// </summary>
        /// <returns></returns>
        public CMrMark GetPartMarkMiddle()
        {
            CMrPart topBeam = CMrBeamDoorManager.GetInstance().mTopBeam;
            CMrPart leftBeam = CMrBeamDoorManager.GetInstance().mLeftBeam;
            CMrPart rightBeam = CMrBeamDoorManager.GetInstance().mRightBeam;
            CMrPart leftBottomBeam = CMrBeamDoorManager.GetInstance().mLeftBottomBeam;
            CMrPart rightBottomBeam = CMrBeamDoorManager.GetInstance().mRightBottomBeam;

            Vector leftTopVector = CMrBeamDoorManager.GetInstance().mLeftTopVector;
            Vector rightTopVector = CMrBeamDoorManager.GetInstance().mRightTopVector;
            Point  midMaxPoint = CMrBeamDoorManager.GetInstance().mMidMaxPoint;
            Vector leftDirectionVector = new Vector(midMaxPoint.X - topBeam.mLeftTopPoint.X, midMaxPoint.Y - topBeam.mLeftTopPoint.Y, 0);
            Vector rightDirectionVector = new Vector(midMaxPoint.X - topBeam.mRightTopPoint.X, midMaxPoint.Y - topBeam.mRightTopPoint.Y, 0);
            
            Point minY = mMrPart.GetMinYPoint();
            Point maxY = mMrPart.GetMaxYPoint();
            Vector normal = mMrPart.mNormal;

            CMrMark mrMark = new CMrMark();
            double dblAngle = CCommonPara.mPartMarkAngle;
            mrMark.mModelObject = mMrPart.mPartInDrawing;

            if (mMrPart == leftBeam)
            {
                mrMark.mInsertPoint = mMrPart.mMidPoint;
                double increaseXDistance = 1.8 * CCommonPara.mPartMarkLength / Math.Tan(dblAngle);
                mrMark.mTextPoint.X = mrMark.mInsertPoint.X - increaseXDistance;
                mrMark.mTextPoint.Y = mrMark.mInsertPoint.Y + 1.8 * CCommonPara.mPartMarkLength;
                return mrMark;
            }
            else if (mMrPart == rightBeam)
            {
                mrMark.mInsertPoint = mMrPart.mMidPoint;
                double increaseXDistance = 1.8 * CCommonPara.mPartMarkLength / Math.Tan(dblAngle);
                mrMark.mTextPoint.X = mrMark.mInsertPoint.X + increaseXDistance;
                mrMark.mTextPoint.Y = mrMark.mInsertPoint.Y + 1.8 * CCommonPara.mPartMarkLength;
                return mrMark;
            }
            else if (mMrPart == leftBottomBeam || mMrPart == rightBottomBeam)
            {
                mrMark.mInsertPoint = mMrPart.mMidPoint;
                double increaseXDistance = 1.8 * CCommonPara.mPartMarkLength / Math.Tan(dblAngle);
                mrMark.mTextPoint.X = mrMark.mInsertPoint.X + increaseXDistance;
                mrMark.mTextPoint.Y = mrMark.mInsertPoint.Y - 1.8 * CCommonPara.mPartMarkLength;
                return mrMark;
            }
            else if (mMrPart == topBeam)
            {
                mrMark.mInsertPoint = mMrPart.mMidPoint;
                double increaseXDistance = 0.5 * CCommonPara.mPartMarkLength / Math.Tan(dblAngle);
                mrMark.mTextPoint.X = mrMark.mInsertPoint.X + increaseXDistance;
                mrMark.mTextPoint.Y = mrMark.mInsertPoint.Y + 0.5 * CCommonPara.mPartMarkLength;
                return mrMark;
            }
            //如果向量与Z轴平行;
            if (CDimTools.GetInstance().IsTwoVectorParallel(normal, new Vector(0, 0, 1)))
            {
                //如果是在上翼板的左上方;
                if (CDimTools.GetInstance().IsThePointOnLine(maxY, topBeam.mLeftTopPoint, midMaxPoint) > 0)
                {
                    //如果是靠左侧挡板的那块板;
                    if (CDimTools.GetInstance().CompareTwoDoubleValue(mMrPart.GetMaxYPoint().X, leftBeam.mRightTopPoint.X) == 0)
                    {
                        double x = mMrPart.GetMinXPoint().X + 20;
                        double y = mMrPart.GetMinYPoint().Y + 20;
                        Point insertPoint = new Point(x, y, 0);
                        mrMark.mInsertPoint = insertPoint;
                        double increaseXDistance = 1.2 * CCommonPara.mPartMarkLength / Math.Tan(dblAngle);
                        mrMark.mTextPoint.X = mrMark.mInsertPoint.X + increaseXDistance;
                        mrMark.mTextPoint.Y = mrMark.mInsertPoint.Y + 1.2 * CCommonPara.mPartMarkLength;
                        return mrMark;
                    }
                    else
                    {
                        if (mMrPart.mLeftTopPoint.X < midMaxPoint.X)
                        {
                            double x = mMrPart.GetMaxXPoint().X - 20;
                            double y = mMrPart.GetMinYPoint().Y + 20;
                            Point insertPoint = new Point(x, y, 0);
                            mrMark.mInsertPoint = insertPoint;
                            double increaseXDistance = CCommonPara.mPartMarkLength / Math.Tan(dblAngle);
                            mrMark.mTextPoint.X = mrMark.mInsertPoint.X - increaseXDistance;
                            mrMark.mTextPoint.Y = mrMark.mInsertPoint.Y + 0.8 * CCommonPara.mPartMarkLength;
                            return mrMark;
                        }
                    }
                }
                //如果在上翼板的右侧上方;
                if (CDimTools.GetInstance().IsThePointOnLine(maxY, midMaxPoint, topBeam.mRightTopPoint) > 0)
                {
                    //如果是靠右侧挡板的那块板;
                    if (CDimTools.GetInstance().CompareTwoDoubleValue(mMrPart.GetMaxYPoint().X, leftBeam.mRightTopPoint.X) == 0)
                    {
                        double x = mMrPart.GetMaxXPoint().X - 20;
                        double y = mMrPart.GetMinYPoint().Y + 20;
                        Point insertPoint = new Point(x, y, 0);
                        mrMark.mInsertPoint = insertPoint;
                        double increaseXDistance = 1.2 * CCommonPara.mPartMarkLength / Math.Tan(dblAngle);
                        mrMark.mTextPoint.X = mrMark.mInsertPoint.X + increaseXDistance;
                        mrMark.mTextPoint.Y = mrMark.mInsertPoint.Y + 1.2 * CCommonPara.mPartMarkLength;
                        return mrMark;
                    }
                    else
                    {
                        if (mMrPart.mLeftTopPoint.X > midMaxPoint.X)
                        {
                            double x = mMrPart.GetMinXPoint().X + 20;
                            double y = mMrPart.GetMinYPoint().Y + 20;
                            Point insertPoint = new Point(x, y, 0);
                            mrMark.mInsertPoint = insertPoint;
                            double increaseXDistance = CCommonPara.mPartMarkLength / Math.Tan(dblAngle);
                            mrMark.mTextPoint.X = mrMark.mInsertPoint.X + increaseXDistance;
                            mrMark.mTextPoint.Y = mrMark.mInsertPoint.Y + 0.8 * CCommonPara.mPartMarkLength;
                            return mrMark;
                        }
                    }
                }
                //如果是在下翼板的下方;
                if (CDimTools.GetInstance().IsThePointOnLine(minY, leftBeam.mLeftTopPoint, leftBeam.mRightTopPoint) < 0)
                {
                    if (CDimTools.GetInstance().CompareTwoDoubleValue(mMrPart.GetMinYPoint().X, leftBeam.mRightBottomPoint.X) == 0)
                    {
                        double x = mMrPart.GetMinXPoint().X + 20;
                        double y = mMrPart.GetMaxYPoint().Y - 20;
                        Point insertPoint = new Point(x, y, 0);
                        mrMark.mInsertPoint = insertPoint;
                        double increaseXDistance = 1.4 * CCommonPara.mPartMarkLength / Math.Tan(dblAngle);
                        mrMark.mTextPoint.X = mrMark.mInsertPoint.X + increaseXDistance;
                        mrMark.mTextPoint.Y = mrMark.mInsertPoint.Y - 1.4 * CCommonPara.mPartMarkLength;
                        return mrMark;
                    }
                    if (CDimTools.GetInstance().CompareTwoDoubleValue(mMrPart.GetMinYPoint().X, rightBeam.mLeftBottomPoint.X) == 0)
                    {
                        double x = mMrPart.GetMaxXPoint().X - 20;
                        double y = mMrPart.GetMaxYPoint().Y - 20;
                        Point insertPoint = new Point(x, y, 0);
                        mrMark.mInsertPoint = insertPoint;
                        double increaseXDistance = 1.4 * CCommonPara.mPartMarkLength / Math.Tan(dblAngle);
                        mrMark.mTextPoint.X = mrMark.mInsertPoint.X + increaseXDistance;
                        mrMark.mTextPoint.Y = mrMark.mInsertPoint.Y - 1.4 * CCommonPara.mPartMarkLength;
                        return mrMark;
                    }
                }
            }
            //如果法向与X轴平行;
            else if (CDimTools.GetInstance().IsTwoVectorParallel(normal, new Vector(1, 0, 0)))
            {
                if (mMrPart.mLeftBottomPoint.X < midMaxPoint.X)
                {
                    mrMark.mInsertPoint = mMrPart.mRightBottomPoint;
                    double increaseXDistance = CCommonPara.mPartMarkLength / Math.Tan(dblAngle);
                    mrMark.mTextPoint.X = mrMark.mInsertPoint.X - increaseXDistance;
                    mrMark.mTextPoint.Y = mrMark.mInsertPoint.Y - CCommonPara.mPartMarkLength;
                    return mrMark;
                }
                else if (mMrPart.mLeftBottomPoint.X > midMaxPoint.X)
                {
                    mrMark.mInsertPoint = mMrPart.mRightBottomPoint;
                    double increaseXDistance = CCommonPara.mPartMarkLength / Math.Tan(dblAngle);
                    mrMark.mTextPoint.X = mrMark.mInsertPoint.X + increaseXDistance;
                    mrMark.mTextPoint.Y = mrMark.mInsertPoint.Y - CCommonPara.mPartMarkLength;
                    return mrMark;
                }
            }
            //如果法向与Y轴平行;
            else if (CDimTools.GetInstance().IsTwoVectorParallel(normal, new Vector(0, 1, 0)))
            {
                mrMark.mInsertPoint = mMrPart.mMidPoint;
                double increaseXDistance = CCommonPara.mPartMarkLength / Math.Tan(dblAngle);
                mrMark.mTextPoint.X = mrMark.mInsertPoint.X + increaseXDistance;
                mrMark.mTextPoint.Y = mrMark.mInsertPoint.Y - CCommonPara.mPartMarkLength;
                return mrMark;
            }
            //如果法向量与上翼板左侧的法向垂直;
            else if (CDimTools.GetInstance().IsTwoVectorParallel(normal, leftDirectionVector)
                   || CDimTools.GetInstance().IsTwoVectorParallel(normal, rightDirectionVector))
            {
                //如果在上翼板左侧上方;
                if (CDimTools.GetInstance().IsThePointOnLine(maxY, topBeam.mLeftTopPoint,midMaxPoint) > 0)
                {
                    if (mMrPart.mLeftTopPoint.X < midMaxPoint.X)
                    {
                        mrMark.mInsertPoint = mMrPart.mRightTopPoint;
                        double increaseXDistance = 1.2 * CCommonPara.mPartMarkLength / Math.Tan(dblAngle);
                        mrMark.mTextPoint.X = mrMark.mInsertPoint.X - increaseXDistance;
                        mrMark.mTextPoint.Y = mrMark.mInsertPoint.Y + 1.2 * CCommonPara.mPartMarkLength;
                        return mrMark;
                    }
                }
                //如果在下翼板左侧上方;
                else if (CDimTools.GetInstance().IsThePointOnLine(maxY, leftBottomBeam.mLeftTopPoint, leftBottomBeam.mRightTopPoint) > 0)
                {
                    if (mMrPart.mLeftTopPoint.X < midMaxPoint.X)
                    {
                        mrMark.mInsertPoint = mMrPart.mRightBottomPoint;
                        double increaseXDistance = CCommonPara.mPartMarkLength / Math.Tan(dblAngle);
                        mrMark.mTextPoint.X = mrMark.mInsertPoint.X - increaseXDistance;
                        mrMark.mTextPoint.Y = mrMark.mInsertPoint.Y - 1.2 * CCommonPara.mPartMarkLength;
                        return mrMark;
                    }
                }
                //如果在上翼板右侧上方;
                if (CDimTools.GetInstance().IsThePointOnLine(maxY, topBeam.mRightTopPoint, midMaxPoint) > 0)
                {
                    if (mMrPart.mLeftTopPoint.X > midMaxPoint.X)
                    {
                        mrMark.mInsertPoint = mMrPart.mRightTopPoint;
                        double increaseXDistance = 1.2 * CCommonPara.mPartMarkLength / Math.Tan(dblAngle);
                        mrMark.mTextPoint.X = mrMark.mInsertPoint.X + increaseXDistance;
                        mrMark.mTextPoint.Y = mrMark.mInsertPoint.Y + 1.2 * CCommonPara.mPartMarkLength;
                        return mrMark;
                    }
                }
                //如果在下翼板右侧上方;
                else if (CDimTools.GetInstance().IsThePointOnLine(maxY, rightBottomBeam.mLeftTopPoint, rightBottomBeam.mRightTopPoint) > 0)
                {
                    if (mMrPart.mLeftTopPoint.X > midMaxPoint.X)
                    {
                        mrMark.mInsertPoint = mMrPart.mRightBottomPoint;
                        double increaseXDistance = CCommonPara.mPartMarkLength / Math.Tan(dblAngle);
                        mrMark.mTextPoint.X = mrMark.mInsertPoint.X + increaseXDistance;
                        mrMark.mTextPoint.Y = mrMark.mInsertPoint.Y - 1.2 * CCommonPara.mPartMarkLength;
                        return mrMark;
                    }
                }
            }
            //如果法向与左右向上的法向平行并且有螺钉;
            else if ((CDimTools.GetInstance().IsTwoVectorVertical(normal, leftDirectionVector) ||
                     CDimTools.GetInstance().IsTwoVectorVertical(normal, rightDirectionVector)) && mMrPart.IsHaveBolt())
            {
                mrMark.mInsertPoint = mMrPart.mRightBottomPoint;
                double increaseXDistance = CCommonPara.mPartMarkLength / Math.Tan(dblAngle);
                mrMark.mTextPoint.X = mrMark.mInsertPoint.X + increaseXDistance;
                mrMark.mTextPoint.Y = mrMark.mInsertPoint.Y - CCommonPara.mPartMarkLength;
                return mrMark;
            }
            return null;
        }
    }
}
