using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tekla.Structures.Model;
using Tekla.Structures.Drawing;
using Tekla.Structures.Geometry3d;

using TSD = Tekla.Structures.Drawing;
using TSM = Tekla.Structures.Model;
using System.Windows.Forms;

namespace AutoDimension.Entity
{
    /// <summary>
    /// 记录零件在顶视图中的信息;
    /// </summary>
    public class CBeamTopViewInfo
    {
        /// <summary>
        /// 与该TopView视图关联的Part对象;
        /// </summary>
        private CMrPart mMrPart;

        /// <summary>
        /// 模型的位置类型;
        /// </summary>
        public MrPositionType mPostionType { get; set; }

        /// <summary>
        /// 与该对象上下位置对称的零件;
        /// </summary>
        public CMrPart mSymPart { get; set; }

        /// <summary>
        /// 判断梁的斜率类型;
        /// </summary>
        public MrSlopeType mSlopeType { get; set; }

        /// <summary>
        /// 部件顶视图信息;
        /// </summary>
        /// <param name="mrPart"></param>
        public CBeamTopViewInfo(CMrPart mrPart)
        {
            this.mMrPart = mrPart;
            mSymPart = null;
        }

        /// <summary>
        /// 初始化该零件的信息,主要初始化零件的位置信息和斜率;
        /// </summary>
        public void InitMrPartTopViewInfo()
        {
            if (mMrPart.mLeftBottomPoint.Y >= CCommonPara.mDblError)
            {
                mPostionType = MrPositionType.UP;
            }
            else if (mMrPart.mLeftTopPoint.Y <= CCommonPara.mDblError)
            {
                mPostionType = MrPositionType.DOWM;
            }
            else
            {
                mPostionType = MrPositionType.MIDDLE;
            }
            mSlopeType=CDimTools.GetInstance().JudgeLineSlope(mMrPart.mLeftBottomPoint,mMrPart.mLeftTopPoint);
        }

        /// <summary>
        /// 获取零件顶视图上方的标注集合;
        /// </summary>
        /// <returns></returns>
        public CMrDimSet GetPartUpDimSet()
        {
            Vector xVector = new Vector(1, 0, 0);
            Vector yVector = new Vector(0, 1, 0);
            Vector zVector = new Vector(0, 0, 1);
            Vector normal = mMrPart.mNormal;

            CMrDimSet mrDimSet = new CMrDimSet();

            //0.如果零件在X轴下方则不标注;
            if (mPostionType == MrPositionType.DOWM)
            {
                return null;
            }

            //1.顶视图标注配置类;
            CBeamTopViewSetting beamTopViewSetting = CBeamDimSetting.GetInstance().mTopViewSetting;

            //2.判断角钢是否需要标注;
            if (CDimTools.GetInstance().IsPartTheAngleSteel(mMrPart))
            {
                return GetAngleSheetUpDim(mMrPart);
            }
            //3.如果向量与Y轴平行则不标注,该尺寸在前视图标注;
            if (CDimTools.GetInstance().IsTwoVectorParallel(normal, yVector))
            {
                return null;
            }
            //4.如果坐标不满足条件则不进行标注;
            double mainBeamLength = CMrMainBeam.GetInstance().GetXSpaceValue();

            if (mMrPart.mLeftBottomPoint.X < CCommonPara.mDblError || mMrPart.mRightBottomPoint.X > mainBeamLength)
            {
                return null;
            }
            //5.过滤掉垫块;
            if (IsPartTheHeelBlock(mMrPart))
            {
                return null;
            }
            //6.如果向量与X轴平行，标注垂直连接板;
            if (CDimTools.GetInstance().IsTwoVectorParallel(normal, xVector)||CDimTools.GetInstance().IsVectorInXYPlane(normal))
            {
                return GetVerticalConnectPlateUpDim(mMrPart);
            }
            //7.如果法向与Z轴平行;
            else if (CDimTools.GetInstance().IsTwoVectorParallel(normal, zVector))
            {
                return GetHorizontalConnentPlateUpDim(mMrPart);
            }
            //8.如果向量在XZ平面,则不标注,如果竖直的牛腿;
            else if (CDimTools.GetInstance().IsVectorInXZPlane(normal))
            {
                return null;
            }
            else
            {
                mrDimSet.AddPoint(mMrPart.GetMaxYMinXPoint());
            }
            return mrDimSet;
        }

        /// <summary>
        /// 获得角钢的上标注;
        /// </summary>
        /// <returns></returns>
        private CMrDimSet GetAngleSheetUpDim(CMrPart mrPart)
        {
            CMrDimSet mrDimSet = new CMrDimSet();

            //1.顶视图标注配置类;
            CBeamTopViewSetting beamTopViewSetting = CBeamDimSetting.GetInstance().mTopViewSetting;            
            bool bMarkValue = beamTopViewSetting.FindMarkValueByName(CBeamTopViewSetting.mstrAngleSheet);
            mMrPart.SetNeedAddMarkFlag(bMarkValue);

            bool bDimValue = beamTopViewSetting.FindDimValueByName(CBeamTopViewSetting.mstrAngleSheet);

            if (!bDimValue)
            {
                return null;
            }

            double minX = mMrPart.GetMinXPoint().X;
            double maxYminX = mMrPart.GetMaxYMinXPoint().X;
            if (CDimTools.GetInstance().CompareTwoDoubleValue(minX, maxYminX) == 0)
            {
                mrDimSet.AddPoint(mMrPart.GetMaxYMinXPoint());
                return mrDimSet;
            }
            else
            {
                mrDimSet.AddPoint(mMrPart.GetMaxYMaxXPoint());
                return mrDimSet;
            }
        }

        /// <summary>
        /// 获得垂直连接板及加筋板的上标注;
        /// </summary>
        /// <param name="mrPart"></param>
        /// <returns></returns>
        private CMrDimSet GetVerticalConnectPlateUpDim(CMrPart mrPart)
        {
            CMrDimSet mrDimSet = new CMrDimSet();

            //1.顶视图标注配置类;
            CBeamTopViewSetting beamTopViewSetting = CBeamDimSetting.GetInstance().mTopViewSetting;

            bool bMarkValue = beamTopViewSetting.FindMarkValueByName(CBeamTopViewSetting.mstrVerticalConnectPlate);
            mMrPart.SetNeedAddMarkFlag(bMarkValue);

            bool bValue = beamTopViewSetting.FindDimValueByName(CBeamTopViewSetting.mstrVerticalConnectPlate);

            if (!bValue)
            {
                return null;
            }
            //如果是外围连接板则排除,并且外围连接板不增加零件标记;
            if (IsOutConnectPlate(mMrPart))
            {
                mMrPart.SetNeedAddMarkFlag(false);
                return null;
            }

            Vector normal = mMrPart.mNormal;

            if (CDimTools.GetInstance().IsTwoVectorParallel(normal, new Vector(1, 0, 0)))
            {
                //如果存在对称的板,默认优先标注下侧的加筋板;
                if (mSymPart != null)
                {
                    if (mMrPart.IsHaveBolt() && !mSymPart.IsHaveBolt())
                    {
                        mrDimSet.AddPoint(mMrPart.GetMaxYMinXPoint());
                        return mrDimSet;
                    }
                    return null;
                }
                else
                {
                    mrDimSet.AddPoint(mMrPart.GetMaxYMinXPoint());
                    return mrDimSet;
                }
            }
            //.如果向量在XY平面内，则为倾斜的连接及加筋板;
            else if(CDimTools.GetInstance().IsVectorInXYPlane(normal))
            {
                if (mSlopeType == MrSlopeType.MORETHAN_ZERO||mSlopeType==MrSlopeType.LESSTHAN_ZERO)
                {
                    mrDimSet.AddPoint(mMrPart.GetMinYPoint());
                    return mrDimSet;
                }
            }
            return null;
        }

        /// <summary>
        /// 获得水平连接板的上标注;
        /// </summary>
        /// <param name="mrPart"></param>
        /// <returns></returns>
        private CMrDimSet GetHorizontalConnentPlateUpDim(CMrPart mrPart)
        {
            //1.顶视图标注配置类;
            CBeamTopViewSetting beamTopViewSetting = CBeamDimSetting.GetInstance().mTopViewSetting;

            CMrDimSet mrDimSet = new CMrDimSet();

            //如果梁上面有盖板，这种盖板没有螺钉并且贴在梁的上面,则不定位;
            if (!mMrPart.IsHaveBolt() && IsPartPosteOnOrUnderMainBeam(mMrPart))
            {
                return null;
            }
            //如果没有螺钉则无孔连接板，需要判断是否是无孔支撑板或者是无孔连接板，根据配置来决定是否标注;
            if (!mMrPart.IsHaveBolt())
            {
                //如果是板类零件;
                if (CDimTools.GetInstance().IsPartThePlate(mrPart))
                {
                    bool bValue = beamTopViewSetting.FindDimValueByName(CBeamTopViewSetting.mstrHorizontalConnentPlate);
                    
                    bool bMarkValue = beamTopViewSetting.FindMarkValueByName(CBeamTopViewSetting.mstrHorizontalConnentPlate);
                    mMrPart.SetNeedAddMarkFlag(bMarkValue);

                    if (!bValue)
                    {
                        return null;
                    }
                    mrDimSet.AddPoint(mMrPart.GetMaxYMinXPoint());
                    return mrDimSet;
                }
                //如果是型钢类零部件;
                else 
                {
                    bool bValue = beamTopViewSetting.FindDimValueByName(CBeamTopViewSetting.mstrSupportPlate);

                    bool bMarkValue = beamTopViewSetting.FindMarkValueByName(CBeamTopViewSetting.mstrSupportPlate);
                    mMrPart.SetNeedAddMarkFlag(bMarkValue);

                    if (!bValue)
                    {
                        return null;
                    }

                    mrDimSet.AddPoint(mMrPart.GetMaxYMinXPoint());
                    return mrDimSet;
                }
            }
            //否则是有孔的支撑板;
            else
            {
                bool bValue = beamTopViewSetting.FindDimValueByName(CBeamTopViewSetting.mstrSupportPlate);
                
                bool bMarkValue = beamTopViewSetting.FindMarkValueByName(CBeamTopViewSetting.mstrSupportPlate);
                mMrPart.SetNeedAddMarkFlag(bMarkValue);

                if (!bValue)
                {
                    return null;
                }
                return GetBoltUpDimSet();
            }
        }

        /// <summary>
        /// 判断零件是否是外围连接板;
        /// </summary>
        /// <param name="mrPart"></param>
        /// <returns></returns>
        private bool IsOutConnectPlate(CMrPart mrPart)
        {
            Vector normal = mrPart.mNormal;

            if (!CDimTools.GetInstance().IsTwoVectorParallel(normal, new Vector(1, 0, 0)))
            {
                return false;
            }

            double minZ = mrPart.GetMinZPoint().Z;
            double maxZ = mrPart.GetMaxZPoint().Z;

            double mainBeamMinZ = CMrMainBeam.GetInstance().GetMinZPoint().Z;
            double mainBeamMaxZ = CMrMainBeam.GetInstance().GetMaxZPoint().Z;

            if(CDimTools.GetInstance().CompareTwoDoubleValue(minZ,mainBeamMaxZ)>=0)
            {
                return true;
            }
            if (CDimTools.GetInstance().CompareTwoDoubleValue(maxZ, mainBeamMinZ) <= 0)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 获取下方标注点的链表;
        /// </summary>
        /// <returns></returns>
        public CMrDimSet GetPartDownDimSet()
        {
            Vector normal = mMrPart.mNormal;
            Vector xVector = new Vector(1, 0, 0);
            Vector yVector = new Vector(0, 1, 0);
            Vector zVector = new Vector(0, 0, 1);

            CMrDimSet mrDimSet = new CMrDimSet();

            //0.顶视图标注配置类;
            CBeamTopViewSetting beamTopViewSetting = CBeamDimSetting.GetInstance().mTopViewSetting;

            //1.如果部件在X轴上方则不进行标注;
            if (mPostionType == MrPositionType.UP)
            {
                return null;
            }
            //2.判断角钢是否需要标注;
            if (CDimTools.GetInstance().IsPartTheAngleSteel(mMrPart))
            {
                return GetAngleSheetDownDim(mMrPart);
            }
            //3.如果坐标不满足要求则不进行标注;
            double mainBeamLength = CMrMainBeam.GetInstance().GetXSpaceValue();

            if (mMrPart.mLeftBottomPoint.X < CCommonPara.mDblError || mMrPart.mRightBottomPoint.X > mainBeamLength)
            {
                return null;
            }
            //4.过滤掉垫块;
            if (IsPartTheHeelBlock(mMrPart))
            {
                return null;
            }
            //5.如果法向与Y轴平行的不需要进行标注;
            if (CDimTools.GetInstance().IsTwoVectorParallel(normal, yVector))
            {
                return null;
            }
            //6.如果向量与X轴平行或者向量在XY平面内，标注垂直连接板或者垂直加紧板或倾斜的加筋板;
            else if (CDimTools.GetInstance().IsTwoVectorParallel(normal, xVector)|| CDimTools.GetInstance().IsVectorInXYPlane(normal))
            {
                return GetVerticalConnectPlateDownDim(mMrPart);
            }
            //7.如果向量与Z轴平行，标注水平连接板;
            else if (CDimTools.GetInstance().IsTwoVectorParallel(normal, zVector))
            {
                return GetHorizontalConnentPlateDownDim(mMrPart);
            }
            //8.如果向量在XZ平面,,则不标注;
            else if (CDimTools.GetInstance().IsVectorInXZPlane(normal))
            {
                return null;
            }
            else
            {
                mrDimSet.AddPoint(mMrPart.GetMinYMinXPoint());
                return mrDimSet;
            }
        }

        /// <summary>
        /// 获得角钢的下标注;
        /// </summary>
        /// <returns></returns>
        private CMrDimSet GetAngleSheetDownDim(CMrPart mrPart)
        {
            CMrDimSet mrDimSet = new CMrDimSet();

            //顶视图标注配置类;
            CBeamTopViewSetting beamTopViewSetting = CBeamDimSetting.GetInstance().mTopViewSetting;

            bool bMarkValue = beamTopViewSetting.FindMarkValueByName(CBeamTopViewSetting.mstrAngleSheet);
            mMrPart.SetNeedAddMarkFlag(bMarkValue);

            bool bValue = beamTopViewSetting.FindDimValueByName(CBeamTopViewSetting.mstrAngleSheet);

            if (!bValue)
            {
                return null;
            }

            double minX = mMrPart.GetMinXPoint().X;
            double minYminX = mMrPart.GetMinYMinXPoint().X;

            if (CDimTools.GetInstance().CompareTwoDoubleValue(minX, minYminX) == 0)
            {
                mrDimSet.AddPoint(mMrPart.GetMinYMinXPoint());
                return mrDimSet;
            }
            else
            {
                mrDimSet.AddPoint(mMrPart.GetMinYMaxXPoint());
                return mrDimSet;
            }
        }

        /// <summary>
        /// 获得垂直连接板及加筋板的下标注;
        /// </summary>
        /// <param name="mrPart"></param>
        /// <returns></returns>
        private CMrDimSet GetVerticalConnectPlateDownDim(CMrPart mrPart)
        {
            Vector normal = mrPart.mNormal;

            CMrDimSet mrDimSet = new CMrDimSet();

            //1.顶视图标注配置类;
            CBeamTopViewSetting beamTopViewSetting = CBeamDimSetting.GetInstance().mTopViewSetting;

            bool bMarkValue = beamTopViewSetting.FindMarkValueByName(CBeamTopViewSetting.mstrVerticalConnectPlate);
            mMrPart.SetNeedAddMarkFlag(bMarkValue);

            bool bValue = beamTopViewSetting.FindDimValueByName(CBeamTopViewSetting.mstrVerticalConnectPlate);

            if (!bValue)
            {
                return null;
            }
            //如果是外围连接板则排除;
            if (IsOutConnectPlate(mMrPart))
            {
                mMrPart.SetNeedAddMarkFlag(false);
                return null;
            }
            if (CDimTools.GetInstance().IsTwoVectorParallel(normal, new Vector(1, 0, 0)))
            {
                //如果存在对称的板;
                if (mSymPart != null)
                {
                    if (!mMrPart.IsHaveBolt() && mSymPart.IsHaveBolt())
                    {
                        return null;
                    }
                    else
                    {
                        mrDimSet.AddPoint(mMrPart.GetMinYMinXPoint());
                        return mrDimSet;
                    }
                }
                else
                {
                    mrDimSet.AddPoint(mMrPart.GetMinYMinXPoint());
                    return mrDimSet;
                }
            }
            //如果向量在XY平面内，则为倾斜的连接加筋板;
            else if (CDimTools.GetInstance().IsVectorInXYPlane(normal))
            {
                //如果梁是倾斜的;
                if (mSlopeType == MrSlopeType.MORETHAN_ZERO || mSlopeType == MrSlopeType.LESSTHAN_ZERO)
                {
                    mrDimSet.AddPoint(mMrPart.GetMaxYPoint());
                    return mrDimSet;
                }
            }
            return null;
        }

        /// <summary>
        /// 获得水平连接板的下标注;
        /// </summary>
        /// <param name="mrPart"></param>
        /// <returns></returns>
        private CMrDimSet GetHorizontalConnentPlateDownDim(CMrPart mrPart)
        {
            //1.顶视图标注配置类;
            CBeamTopViewSetting beamTopViewSetting = CBeamDimSetting.GetInstance().mTopViewSetting;

            CMrDimSet mrDimSet = new CMrDimSet();

            //如果梁上面有盖板，这种盖板没有螺钉并且贴在梁的上面,则不定位;
            if (!mMrPart.IsHaveBolt() && IsPartPosteOnOrUnderMainBeam(mMrPart))
            {
                return null;
            }
            //如果没有螺钉则无孔连接板，需要判断是否是无孔支撑板还是无孔连接板，根据配置来决定是否标注;
            if (!mMrPart.IsHaveBolt())
            {
                //如果是板类零件;
                if (CDimTools.GetInstance().IsPartThePlate(mrPart))
                {
                    bool bMarkValue = beamTopViewSetting.FindMarkValueByName(CBeamTopViewSetting.mstrHorizontalConnentPlate);
                    mMrPart.SetNeedAddMarkFlag(bMarkValue);

                    bool bValue = beamTopViewSetting.FindDimValueByName(CBeamTopViewSetting.mstrHorizontalConnentPlate);

                    if (!bValue)
                    {
                        return null;
                    }
                    mrDimSet.AddPoint(mMrPart.GetMinYMinXPoint());
                    return mrDimSet;
                }
                //如果是型钢类零部件;
                else
                {
                    bool bMarkValue = beamTopViewSetting.FindMarkValueByName(CBeamTopViewSetting.mstrSupportPlate);
                    mMrPart.SetNeedAddMarkFlag(bMarkValue);

                    bool bValue = beamTopViewSetting.FindDimValueByName(CBeamTopViewSetting.mstrSupportPlate);

                    if (!bValue)
                    {
                        return null;
                    }
                    mrDimSet.AddPoint(mMrPart.GetMinYMinXPoint());
                    return mrDimSet;
                }
            }
            //否则是有孔的支撑板;
            else
            {
                bool bMarkValue = beamTopViewSetting.FindMarkValueByName(CBeamTopViewSetting.mstrSupportPlate);
                mMrPart.SetNeedAddMarkFlag(bMarkValue);

                bool bValue = beamTopViewSetting.FindDimValueByName(CBeamTopViewSetting.mstrSupportPlate);

                if (!bValue)
                {
                    return null;
                }
                return GetBoltDownDimSet();
            }
        }

        /// <summary>
        /// 获取在水平方向下方进行标注的螺钉点的链表;
        /// </summary>
        /// <returns></returns>
        private CMrDimSet GetBoltDownDimSet()
        {
            if (!mMrPart.IsHaveBolt())
            {
                return null;
            }
            if (mMrPart.GetBeamTopViewInfo().mPostionType == MrPositionType.UP)
            {
                return null;
            }

            CMrDimSet mrDimSet = new CMrDimSet();
            Vector zVector = new Vector(0, 0, 1);
            double mainBeamWidth = CMrMainBeam.GetInstance().GetXSpaceValue();

            List<CMrBoltArray> mrBoltArrayList = mMrPart.GetBoltArrayList();
            int nCount = mrBoltArrayList.Count;

            List<CMrBoltArray> newMrBoltArrayList = new List<CMrBoltArray>();

            //默认只标注最左和最右的两个螺钉组;
            if (nCount > 2)
            {
                newMrBoltArrayList.Add(mrBoltArrayList[0]);
                newMrBoltArrayList.Add(mrBoltArrayList[nCount - 1]);
            }
            else
            {
                newMrBoltArrayList.AddRange(mrBoltArrayList);
            }

            foreach (CMrBoltArray mrBoltArray in newMrBoltArrayList)
            {
                if (mrBoltArray.GetMrBoltArrayInfo().mbIsUpDim)
                {
                    continue;
                }
                //如果螺钉组的法向与Z轴不平行,则返回;
                if (!CDimTools.GetInstance().IsTwoVectorParallel(zVector, mrBoltArray.mNormal))
                {
                    continue;
                }
                if (mrBoltArray.mBoltArrayShapeType == MrBoltArrayShapeType.ARRAY)
                {
                    List<Point> pointList = mrBoltArray.GetMinYPointList();

                    foreach (Point point in pointList)
                    {
                        if (point.X > 0 && point.X < mainBeamWidth)
                        {
                            mrDimSet.AddPoint(point);
                        }
                    }
                }
                else if (mrBoltArray.mBoltArrayShapeType == MrBoltArrayShapeType.OBLIQUELINE)
                {
                    List<Point> boltPointList = mrBoltArray.GetBoltPointList();

                    if (boltPointList.Count < 4)
                    {
                        mrDimSet.AddPoint(mrBoltArray.GetMaxYPoint());
                        mrDimSet.AddPoint(mrBoltArray.GetMinYPoint());
                    }
                    else
                    {
                        Point minYPoint = mrBoltArray.GetMinYPoint();
                        Point firstPoint = mrBoltArray.mFirstPoint;
                        Point secondPoint = mrBoltArray.mSecondPoint;

                        Vector directionVector = new Vector(secondPoint.X - firstPoint.X, secondPoint.Y - firstPoint.Y, secondPoint.Z - firstPoint.Z);

                        foreach (Point boltPoint in boltPointList)
                        {
                            Vector newVector = new Vector(boltPoint.X - minYPoint.X, boltPoint.Y - minYPoint.Y, boltPoint.Z - minYPoint.Z);

                            if (CDimTools.GetInstance().IsTwoVectorVertical(directionVector, newVector))
                            {
                                mrDimSet.AddPoint(boltPoint);
                            }
                        }
                    }
                }
            }
            return mrDimSet;
        }

        /// <summary>
        /// 获取在水平方向上方进行标注的螺钉点的链表;
        /// </summary>
        /// <returns></returns>
        private CMrDimSet GetBoltUpDimSet()
        {
            if (!mMrPart.IsHaveBolt())
            {
                return null;
            }

            if (mMrPart.GetBeamTopViewInfo().mPostionType == MrPositionType.DOWM)
            {
                return null;
            }

            CMrDimSet mrDimSet = new CMrDimSet();
            Vector zVector = new Vector(0, 0, 1);
            double mainBeamWidth = CMrMainBeam.GetInstance().GetXSpaceValue();
            
            List<CMrBoltArray> mrBoltArrayList = mMrPart.GetBoltArrayList();
            int nCount = mrBoltArrayList.Count;

            List<CMrBoltArray> newMrBoltArrayList = new List<CMrBoltArray>();

            //默认只标注最左和最右的两个螺钉组;
            if (nCount > 2)
            {
                newMrBoltArrayList.Add(mrBoltArrayList[0]);
                newMrBoltArrayList.Add(mrBoltArrayList[nCount - 1]);
            }
            else
            {
                newMrBoltArrayList.AddRange(mrBoltArrayList);
            }

            foreach (CMrBoltArray mrBoltArray in newMrBoltArrayList)
            {
                //如果螺钉组的法向与Z轴不平行,则返回继续执行;
                if (!CDimTools.GetInstance().IsTwoVectorParallel(zVector, mrBoltArray.mNormal))
                {
                    continue;
                }
                //如果该螺钉组下方有对称的螺钉组,则上面不标注;
                if (mrBoltArray.GetMrBoltArrayInfo().mXSymBoltArray != null)
                {
                    continue;
                }
                if (mrBoltArray.mBoltArrayShapeType == MrBoltArrayShapeType.ARRAY)
                {
                    List<Point> pointList = mrBoltArray.GetMaxYPointList();
                    foreach (Point point in pointList)
                    {
                        if (point.X > 0 && point.X < mainBeamWidth)
                        {
                            mrDimSet.AddPoint(point);
                        }
                    }
                }
                else if (mrBoltArray.mBoltArrayShapeType == MrBoltArrayShapeType.OBLIQUELINE)
                {
                    List<Point> boltPointList = mrBoltArray.GetBoltPointList();

                    if(boltPointList.Count < 4 )
                    {
                        mrDimSet.AddPoint(mrBoltArray.GetMinYPoint());
                        mrDimSet.AddPoint(mrBoltArray.GetMaxYPoint());
                    }
                    else
                    {
                        Point maxYPoint = mrBoltArray.GetMaxYPoint();
                        Point firstPoint = mrBoltArray.mFirstPoint;
                        Point secondPoint = mrBoltArray.mSecondPoint;

                        Vector directionVector = new Vector(secondPoint.X - firstPoint.X, secondPoint.Y - firstPoint.Y, secondPoint.Z - firstPoint.Z);
                        foreach (Point boltPoint in boltPointList)
                        {
                            Vector newVector = new Vector(boltPoint.X - maxYPoint.X, boltPoint.Y - maxYPoint.Y, boltPoint.Z - maxYPoint.Z);
                            if (CDimTools.GetInstance().IsTwoVectorVertical(directionVector, newVector))
                            {
                                mrDimSet.AddPoint(boltPoint);
                            }
                        }
                    }
                }
                if (mrDimSet.Count > 0)
                {
                    //表明该螺钉组已经标注了;
                    mrBoltArray.GetMrBoltArrayInfo().mbIsUpDim = true;
                }
            }
            return mrDimSet;
        }

        /// <summary>
        /// 获得倾斜的加筋板或连接板的标注;
        /// </summary>
        /// <returns></returns>
        public List<CMrDimSet> GetObliquePartDimSet()
        {
            CBeamTopViewSetting beamTopViewSetting = CBeamDimSetting.GetInstance().mTopViewSetting;

            bool bValue = beamTopViewSetting.FindDimValueByName(CBeamTopViewSetting.mstrVerticalConnectPlate);

            if (!bValue)
            {
                return null;
            }

            double mainBeamWidth = CMrMainBeam.GetInstance().GetXSpaceValue();
            double middleBeamWidth = CMrMainBeam.GetInstance().GetXSpaceValue() / 2.0;
            List<CMrDimSet> mrDimSetList = new List<CMrDimSet>();

            if (mPostionType == MrPositionType.UP)
            {
                if (mSlopeType == MrSlopeType.MORETHAN_ZERO)
                {
                    CMrDimSet mrDimSetY = new CMrDimSet();

                    mrDimSetY.AddPoint(mMrPart.mRightTopPoint);
                    Point basePoint = new Point(mMrPart.mRightTopPoint.X, mMrPart.mRightBottomPoint.Y);
                    mrDimSetY.AddPoint(basePoint);
                    mrDimSetY.mDimVector = new Vector(1, 0, 0);
                    mrDimSetY.mDimDistance = CCommonPara.mDefaultDimDistance;
                    mrDimSetList.Add(mrDimSetY);

                    CMrDimSet mrDimSetX = new CMrDimSet();
                    mrDimSetX.AddPoint(mMrPart.mRightTopPoint);
                    basePoint = new Point(mMrPart.mRightBottomPoint.X, mMrPart.mRightTopPoint.Y);
                    mrDimSetX.AddPoint(basePoint);
                    mrDimSetX.mDimVector = new Vector(0, 1, 0);
                    mrDimSetX.mDimDistance = CCommonPara.mDefaultDimDistance;
                    mrDimSetList.Add(mrDimSetX);
                }
                else if (mSlopeType == MrSlopeType.LESSTHAN_ZERO)
                {
                    CMrDimSet mrDimSetY = new CMrDimSet();

                    mrDimSetY.AddPoint(mMrPart.mLeftTopPoint);
                    Point basePoint = new Point(mMrPart.mLeftTopPoint.X, mMrPart.mLeftBottomPoint.Y);
                    mrDimSetY.AddPoint(basePoint);
                    mrDimSetY.mDimVector = new Vector(-1, 0, 0);
                    mrDimSetY.mDimDistance = CCommonPara.mDefaultDimDistance;
                    mrDimSetList.Add(mrDimSetY);

                    CMrDimSet mrDimSetX = new CMrDimSet();
                    mrDimSetX.AddPoint(mMrPart.mLeftTopPoint);
                    basePoint = new Point(mMrPart.mLeftBottomPoint.X, mMrPart.mLeftTopPoint.Y);
                    mrDimSetX.AddPoint(basePoint);
                    mrDimSetX.mDimVector = new Vector(0, 1, 0);
                    mrDimSetX.mDimDistance = CCommonPara.mDefaultDimDistance;
                    mrDimSetList.Add(mrDimSetX);
                }
            }
            else if (mPostionType == MrPositionType.DOWM)
            {
                if (mSlopeType == MrSlopeType.MORETHAN_ZERO)
                {
                    CMrDimSet mrDimSetY = new CMrDimSet();

                    mrDimSetY.AddPoint(mMrPart.mLeftBottomPoint);
                    Point basePoint = new Point(mMrPart.mLeftBottomPoint.X, mMrPart.mLeftTopPoint.Y);
                    mrDimSetY.AddPoint(basePoint);
                    mrDimSetY.mDimVector = new Vector(-1, 0, 0);
                    mrDimSetY.mDimDistance = CCommonPara.mDefaultDimDistance;
                    mrDimSetList.Add(mrDimSetY);

                    CMrDimSet mrDimSetX = new CMrDimSet();
                    mrDimSetX.AddPoint(mMrPart.mLeftBottomPoint);
                    basePoint = new Point(mMrPart.mLeftTopPoint.X, mMrPart.mLeftBottomPoint.Y);
                    mrDimSetX.AddPoint(basePoint);
                    mrDimSetX.mDimVector = new Vector(0, -1, 0);
                    mrDimSetX.mDimDistance = CCommonPara.mDefaultDimDistance;
                    mrDimSetList.Add(mrDimSetX);
                }
                else if (mSlopeType == MrSlopeType.LESSTHAN_ZERO)
                {
                    CMrDimSet mrDimSetY = new CMrDimSet();

                    mrDimSetY.AddPoint(mMrPart.mRightBottomPoint);
                    Point basePoint = new Point(mMrPart.mRightBottomPoint.X, mMrPart.mRightTopPoint.Y);
                    mrDimSetY.AddPoint(basePoint);
                    mrDimSetY.mDimVector = new Vector(1, 0, 0);
                    mrDimSetY.mDimDistance = CCommonPara.mDefaultDimDistance;
                    mrDimSetList.Add(mrDimSetY);

                    CMrDimSet mrDimSetX = new CMrDimSet();
                    mrDimSetX.AddPoint(mMrPart.mRightBottomPoint);
                    basePoint = new Point(mMrPart.mRightTopPoint.X, mMrPart.mRightBottomPoint.Y);
                    mrDimSetX.AddPoint(basePoint);
                    mrDimSetX.mDimVector = new Vector(0, -1, 0);
                    mrDimSetX.mDimDistance = CCommonPara.mDefaultDimDistance;
                    mrDimSetList.Add(mrDimSetX);
                }
            }
            return mrDimSetList;
        }

        /// <summary>
        /// 获得有孔的水平连接板及支撑板的标注;
        /// </summary>
        /// <returns></returns>
        public List<CMrDimSet> GetPartBoltYDimSetList()
        {
            if (!mMrPart.IsHaveBolt())
            {
                return null;
            }
            CBeamTopViewSetting beamTopViewSetting = CBeamDimSetting.GetInstance().mTopViewSetting;

            bool bValue = beamTopViewSetting.FindDimValueByName(CBeamTopViewSetting.mstrSupportPlate);

            if (!bValue)
            {
                return null;
            }
            Vector zVector = new Vector(0, 0, 1);
           
            List<CMrDimSet> mrDimSetList = new List<CMrDimSet>();
            List<CMrBoltArray> mrBoltArrayList = mMrPart.GetBoltArrayList();

            int nCount = mrBoltArrayList.Count;
            List<CMrBoltArray> newMrBoltArrayList = new List<CMrBoltArray>();

            //默认只标注最左和最右的两个螺钉组;
            if (nCount > 2)
            {
                newMrBoltArrayList.Add(mrBoltArrayList[0]);
                newMrBoltArrayList.Add(mrBoltArrayList[nCount - 1]);
            }
            else
            {
                newMrBoltArrayList.AddRange(mrBoltArrayList);
            }
            foreach (CMrBoltArray mrBoltArray in newMrBoltArrayList)
            {
                //如果螺钉组的法向与Z轴不平行,则返回继续执行;
                if (!CDimTools.GetInstance().IsTwoVectorParallel(zVector, mrBoltArray.mNormal))
                {
                    continue;
                }
                if (mrBoltArray.mBoltArrayShapeType == MrBoltArrayShapeType.ARRAY)
                {
                    mrDimSetList.Add(GetBoltArrayDimSet(mrBoltArray));
                }
                else if (mrBoltArray.mBoltArrayShapeType == MrBoltArrayShapeType.OBLIQUELINE)
                {
                    mrDimSetList.Add(GetBoltObliqueLineDimSet(mrBoltArray));
                }
            }
            return mrDimSetList;
        }

        /// <summary>
        /// 获取Array类型的螺钉组在Y方向上的标注;
        /// </summary>
        /// <returns></returns>
        private CMrDimSet GetBoltArrayDimSet(CMrBoltArray mrBoltArray)
        {
            Point minXPoint = mMrPart.GetMinXPoint();
            Point maxXPoint = mMrPart.GetMaxXPoint();

            double mainBeamWidth = CMrMainBeam.GetInstance().GetXSpaceValue();

            if(minXPoint.X < CCommonPara.mDblError && maxXPoint.X > CCommonPara.mDblError)
            {
                return null;
            }
            if(maxXPoint.X > mainBeamWidth)
            {
                return null;
            }

            CMrDimSet mrDimSet = new CMrDimSet();
            double middleBeamWidth = mainBeamWidth / 2.0;

            Point leftTopPoint = mMrPart.mLeftTopPoint;
            Point rightTopPoint = mMrPart.mRightTopPoint;

            CMrBoltArray symBoltArray = mrBoltArray.GetMrBoltArrayInfo().mXSymBoltArray;

            //如果该螺钉组有对称的螺钉组;
            if (symBoltArray != null)
            {
                if (minXPoint.X <= middleBeamWidth)
                {
                    List<Point> minXPointList = mrBoltArray.GetMinXPointList();
                    List<Point> symMinXPointList = symBoltArray.GetMinXPointList();

                    mrDimSet.mDimVector = new Vector(-1, 0, 0);

                    mrDimSet.AddRange(minXPointList);
                    mrDimSet.AddRange(symMinXPointList);
                    
                    if (mrDimSet.Count > 1)
                    {
                        Point basePoint = new Point(minXPoint.X, 0, 0);
                        mrDimSet.AddPoint(basePoint);
                    }
                    mrDimSet.mDimDistance = mMrPart.GetLeftDefaultDimDistance(mrDimSet.GetDimPointList()[0]);
                }
                else if (minXPoint.X > middleBeamWidth)
                {
                    List<Point> maxXPointList = mrBoltArray.GetMaxXPointList();
                    List<Point> symMaxXPointList = symBoltArray.GetMaxXPointList();
                    
                    mrDimSet.mDimVector = new Vector(1, 0, 0);
                    mrDimSet.AddRange(maxXPointList);
                    mrDimSet.AddRange(symMaxXPointList);
                    
                    if (mrDimSet.Count > 1)
                    {
                        Point basePoint = new Point(maxXPoint.X, 0, 0);
                        mrDimSet.AddPoint(basePoint);
                    }
                    mrDimSet.mDimDistance = mMrPart.GetRightDefaultDimDistance(mrDimSet.GetDimPointList()[0]); 
                }
            }
            else
            {
                if(minXPoint.X <= middleBeamWidth)
                {
                    List<Point> minXPointList = mrBoltArray.GetMinXPointList();

                    mrDimSet.mDimVector = new Vector(-1, 0, 0);
                    mrDimSet.AddRange(minXPointList);
                    mrDimSet.AddPoint(new Point(minXPoint.X, 0, 0));
                    mrDimSet.mDimDistance = mMrPart.GetLeftDefaultDimDistance(mrDimSet.GetDimPointList()[0]); 
                }
                else if (minXPoint.X > middleBeamWidth)
                {
                    List<Point> maxXPointList = mrBoltArray.GetMaxXPointList();
                    
                    mrDimSet.mDimVector = new Vector(1, 0, 0);
                    mrDimSet.AddRange(maxXPointList);
                    mrDimSet.AddPoint(new Point(maxXPoint.X, 0, 0));
                    mrDimSet.mDimDistance = mMrPart.GetRightDefaultDimDistance(mrDimSet.GetDimPointList()[0]); 
                }
            }
            return mrDimSet;
        }

        /// <summary>
        /// 获取倾斜的螺钉组在Y方向上的标注集;
        /// </summary>
        /// <param name="mrBoltArray"></param>
        /// <returns></returns>
        private CMrDimSet GetBoltObliqueLineDimSet(CMrBoltArray mrBoltArray)
        {
            CMrDimSet mrDimSet = new CMrDimSet();

            double mainBeamWidth = CMrMainBeam.GetInstance().GetXSpaceValue();
            double middleBeamWidth = mainBeamWidth / 2.0;
           
            Point minYPoint = mrBoltArray.GetMinYPoint();
            Point maxYPoint = mrBoltArray.GetMaxYPoint();

            if (minYPoint.Y < 0)
            {
                List<Point> boltPointList = mrBoltArray.GetBoltPointList();
                Point firstPoint = mrBoltArray.mFirstPoint;
                Point secondPoint = mrBoltArray.mSecondPoint;

                if (boltPointList.Count < 4)
                {
                    mrDimSet.AddPoint(mrBoltArray.GetMinYPoint());
                    mrDimSet.AddPoint(mrBoltArray.GetMaxYPoint());
                }
                else
                {
                    Vector directionVector = new Vector(secondPoint.X - firstPoint.X, secondPoint.Y - firstPoint.Y, secondPoint.Z - firstPoint.Z);
                    foreach (Point boltPoint in boltPointList)
                    {
                        Vector newVector = new Vector(boltPoint.X - minYPoint.X, boltPoint.Y - minYPoint.Y, boltPoint.Z - minYPoint.Z);
                        
                        if (CDimTools.GetInstance().IsTwoVectorVertical(directionVector, newVector))
                        {
                            mrDimSet.AddPoint(boltPoint);
                        }
                    }
                }
                if (mrDimSet.Count > 1)
                {
                    Point basePoint = new Point(minYPoint.X, 0, 0);
                    mrDimSet.AddPoint(basePoint);
                }
                if (mrDimSet.Count >= 3)
                {
                    mrDimSet.mDimVector = GetBoltObliqueLineDimVector(mrBoltArray);
                }
            }
            else if (maxYPoint.Y > 0)
            {
                List<Point> boltPointList = mrBoltArray.GetBoltPointList();

                Point firstPoint = mrBoltArray.mFirstPoint;
                Point secondPoint = mrBoltArray.mSecondPoint;

                if (boltPointList.Count < 4)
                {
                    mrDimSet.AddPoint(mrBoltArray.GetMinYPoint());
                    mrDimSet.AddPoint(mrBoltArray.GetMaxYPoint());
                }
                else
                {
                    Vector directionVector = new Vector(secondPoint.X - firstPoint.X, secondPoint.Y - firstPoint.Y, secondPoint.Z - firstPoint.Z);
                    foreach (Point boltPoint in boltPointList)
                    {
                        Vector newVector = new Vector(boltPoint.X - maxYPoint.X, boltPoint.Y - maxYPoint.Y, boltPoint.Z - maxYPoint.Z);
                        if (CDimTools.GetInstance().IsTwoVectorVertical(directionVector, newVector))
                        {
                            mrDimSet.AddPoint(boltPoint);
                        }
                    }
                }
                if (mrDimSet.Count > 1)
                {
                    Point basePoint = new Point(maxYPoint.X, 0, 0);
                    mrDimSet.AddPoint(basePoint);
                }
                if (mrDimSet.Count >= 3)
                {
                    mrDimSet.mDimVector = GetBoltObliqueLineDimVector(mrBoltArray);
                }
            }
            if (mrDimSet.Count <= 1)
            {
                return null;
            }
            if (mrDimSet.mDimVector.X > 0)
            {
                mrDimSet.mDimDistance = mMrPart.GetRightDefaultDimDistance(mrDimSet.GetDimPointList()[0]);
            }
            else
            {
                mrDimSet.mDimDistance = mMrPart.GetLeftDefaultDimDistance(mrDimSet.GetDimPointList()[0]);
            }

            return mrDimSet;
        }

        /// <summary>
        /// 获取螺钉组的标注向量;
        /// </summary>
        /// <param name="firstPoint"></param>
        /// <param name="secondPoint"></param>
        /// <returns></returns>
        private Vector GetBoltObliqueLineDimVector(CMrBoltArray mrBoltArray)
        {
            Point maxYPoint = mrBoltArray.GetMaxYPoint();
            Vector dimVector = null;

            Point minXPoint = mMrPart.GetMinXPoint();
            Point maxXPoint = mMrPart.GetMaxXPoint();

            double dblDistance1 = Math.Abs(maxYPoint.X - minXPoint.X);
            double dblDistance2 = Math.Abs(maxYPoint.X - maxXPoint.X);

            if (dblDistance1 < dblDistance2)
            {
                dimVector = new Vector(-1, 0, 0);
            }
            else
            {
                dimVector = new Vector(1, 0, 0);
            }
            return dimVector;
        }

        /// <summary>
        /// 获取零件标记;
        /// </summary>
        public CMrMark GetPartMark()
        {            
            //1.如果零部件的法向与Y轴平行则不标记;
            Vector normal = mMrPart.mNormal;
            Vector yVector = new Vector(0, 1, 0);
            Vector zVector = new Vector(0, 0, 1);

            double mainBeamWidth = CMrMainBeam.GetInstance().GetXSpaceValue();

            if(CDimTools.GetInstance().IsVectorInXZPlane(normal))
            {
                return null;
            }

            //如果板没有螺钉，并且贴在主梁上方或下方则不标记;
            if (!mMrPart.IsHaveBolt() && IsPartPosteOnOrUnderMainBeam(mMrPart))
            {
                return null;
            }

            CMrMark mrMark = new CMrMark();

            if (mPostionType == MrPositionType.UP )
            {
                double dblAngle = 0.0;

                //判断双夹板中法向与Y轴平行的零件;
                if (CDimTools.GetInstance().IsTwoVectorParallel(normal, yVector))
                {
                    //如果是中间左侧的双夹板;
                    if (mMrPart.mLeftTopPoint.X < CCommonPara.mDblError && mMrPart.mRightTopPoint.X > CCommonPara.mDblError)
                    {
                        dblAngle = CCommonPara.mPartMarkAngle;
                        mrMark.mModelObject = mMrPart.mPartInDrawing;

                        mrMark.mInsertPoint = mMrPart.mLeftTopPoint;

                        double increaseXDistance = CCommonPara.mPartMarkLength / Math.Tan(dblAngle);
                        mrMark.mTextPoint.X = mrMark.mInsertPoint.X - increaseXDistance;
                        mrMark.mTextPoint.Y = mrMark.mInsertPoint.Y + 1.2 * CCommonPara.mPartMarkLength;

                        return mrMark;
                    }
                    //如果是中间右侧的双夹板;
                    else if (mMrPart.mLeftTopPoint.X < mainBeamWidth && mMrPart.mRightTopPoint.X > mainBeamWidth)
                    {
                        dblAngle = CCommonPara.mPartMarkAngle;
                        mrMark.mModelObject = mMrPart.mPartInDrawing;

                        mrMark.mInsertPoint = mMrPart.mRightTopPoint;
                        double increaseXDistance = CCommonPara.mPartMarkLength / Math.Tan(dblAngle);
                        mrMark.mTextPoint.X = mrMark.mInsertPoint.X + increaseXDistance;
                        mrMark.mTextPoint.Y = mrMark.mInsertPoint.Y + 1.2 * CCommonPara.mPartMarkLength;

                        return mrMark;
                    }
                    else
                    {
                        return null;
                    }
                }
                if(mSlopeType==MrSlopeType.MORETHAN_ZERO)
                {
                    dblAngle = Math.Atan((mMrPart.mLeftTopPoint.Y - mMrPart.mLeftBottomPoint.Y) / (mMrPart.mLeftTopPoint.X - mMrPart.mLeftBottomPoint.X));

                    mrMark.mModelObject = mMrPart.mPartInDrawing;
                    mrMark.mInsertPoint = mMrPart.mRightTopPoint;

                    double increaseXDistance = CCommonPara.mPartMarkLength / Math.Tan(dblAngle - 0.5);

                    mrMark.mTextPoint.X = mrMark.mInsertPoint.X + increaseXDistance;
                    mrMark.mTextPoint.Y = mrMark.mInsertPoint.Y + CCommonPara.mPartMarkLength;

                    return mrMark;
                }
                else if(mSlopeType==MrSlopeType.LESSTHAN_ZERO)
                {
                    dblAngle = Math.PI-Math.Atan((mMrPart.mLeftTopPoint.Y - mMrPart.mLeftBottomPoint.Y) / (-mMrPart.mLeftTopPoint.X + mMrPart.mLeftBottomPoint.X));

                    mrMark.mModelObject = mMrPart.mPartInDrawing;
                    mrMark.mInsertPoint = mMrPart.mLeftTopPoint;

                    double increaseXDistance = CCommonPara.mPartMarkLength / Math.Tan(dblAngle + 0.5);

                    mrMark.mTextPoint.X = mrMark.mInsertPoint.X + increaseXDistance;
                    mrMark.mTextPoint.Y = mrMark.mInsertPoint.Y + CCommonPara.mPartMarkLength;

                    return mrMark;
                }
                else 
                {
                    if (mMrPart.mLeftTopPoint.X < 0 && mMrPart.mRightTopPoint.X > 0)
                    {
                        return null;
                    }
                    if (mMrPart.mLeftTopPoint.X < mainBeamWidth+0.05 && mMrPart.mRightTopPoint.X > mainBeamWidth+0.05)
                    {
                        return null;
                    }

                    //如果是多边形板,把标记点放在螺钉旁边;
                    if((mMrPart.mBeamType==MrBeamType.CONTOURPLATE||mMrPart.mBeamType==MrBeamType.POLYBEAM)
                        && (mMrPart.IsHaveBolt() && CDimTools.GetInstance().IsTwoVectorParallel(zVector, normal)))
                    {
                        Point maxYPoint=new Point();
                        maxYPoint.Y = int.MinValue;

                        foreach(CMrBoltArray mrBoltArray in mMrPart.GetBoltArrayList())
                        {
                            Point maxPoint = mrBoltArray.GetMaxXPoint();

                            if( maxPoint.Y > maxYPoint.Y)
                            {
                                maxYPoint = maxPoint;
                            }
                        }

                        dblAngle = CCommonPara.mPartMarkAngle;
                        mrMark.mModelObject = mMrPart.mPartInDrawing;

                        mrMark.mInsertPoint = maxYPoint;
                        mrMark.mInsertPoint.X = maxYPoint.X ;
                        mrMark.mInsertPoint.Y = maxYPoint.Y + 300 / CCommonPara.mViewScale;
                        double increaseXDistance = CCommonPara.mPartMarkLength / Math.Tan(dblAngle);
                        mrMark.mTextPoint.X = mrMark.mInsertPoint.X + increaseXDistance;
                        mrMark.mTextPoint.Y = mrMark.mInsertPoint.Y + CCommonPara.mPartMarkLength;

                        return mrMark;
                    }
                    else
                    {
                        dblAngle = CCommonPara.mPartMarkAngle;
                        mrMark.mModelObject = mMrPart.mPartInDrawing;

                        mrMark.mInsertPoint = mMrPart.mRightTopPoint;
                        mrMark.mInsertPoint.X = mMrPart.mRightTopPoint.X - 5;
                        mrMark.mInsertPoint.Y = mMrPart.mRightTopPoint.Y - 5;
                        double increaseXDistance = CCommonPara.mPartMarkLength / Math.Tan(dblAngle);
                        mrMark.mTextPoint.X = mrMark.mInsertPoint.X + increaseXDistance;
                        mrMark.mTextPoint.Y = mrMark.mInsertPoint.Y + CCommonPara.mPartMarkLength;

                        return mrMark;
                    }
                }
            }
            else if (mPostionType == MrPositionType.DOWM)
            {
                double dblAngle = 0.0;

                //判断双夹板中法向与Y轴平行的零件;
                if (CDimTools.GetInstance().IsTwoVectorParallel(normal, yVector))
                {
                    //如果是中间左侧的双夹板;
                    if (mMrPart.mLeftTopPoint.X < CCommonPara.mDblError && mMrPart.mRightTopPoint.X > CCommonPara.mDblError)
                    {
                        dblAngle = CCommonPara.mPartMarkAngle;
                        mrMark.mModelObject = mMrPart.mPartInDrawing;

                        mrMark.mInsertPoint = mMrPart.mLeftBottomPoint;
                        double increaseXDistance = CCommonPara.mPartMarkLength / Math.Tan(dblAngle);
                        mrMark.mTextPoint.X = mrMark.mInsertPoint.X - increaseXDistance;
                        mrMark.mTextPoint.Y = mrMark.mInsertPoint.Y - 1.2 * CCommonPara.mPartMarkLength;

                        return mrMark;
                    }
                    //如果是中间右侧的双夹板;
                    if (mMrPart.mLeftTopPoint.X < mainBeamWidth && mMrPart.mRightTopPoint.X > mainBeamWidth)
                    {
                        dblAngle = CCommonPara.mPartMarkAngle;
                        mrMark.mModelObject = mMrPart.mPartInDrawing;

                        mrMark.mInsertPoint = mMrPart.mRightBottomPoint;
                        double increaseXDistance = CCommonPara.mPartMarkLength / Math.Tan(dblAngle);
                        mrMark.mTextPoint.X = mrMark.mInsertPoint.X + increaseXDistance;
                        mrMark.mTextPoint.Y = mrMark.mInsertPoint.Y - 1.2 * CCommonPara.mPartMarkLength;

                        return mrMark;
                    }
                    else
                    {
                        return null;
                    }
                }
                if (mSlopeType == MrSlopeType.MORETHAN_ZERO)
                {
                    dblAngle = Math.PI - Math.Atan((mMrPart.mLeftTopPoint.Y - mMrPart.mLeftBottomPoint.Y) / (mMrPart.mLeftTopPoint.X - mMrPart.mLeftBottomPoint.X));

                    mrMark.mModelObject = mMrPart.mPartInDrawing;
                    mrMark.mInsertPoint = mMrPart.mLeftBottomPoint;

                    double increaseXDistance = CCommonPara.mPartMarkLength / Math.Tan(dblAngle + 0.5);

                    mrMark.mTextPoint.X = mrMark.mInsertPoint.X + increaseXDistance;
                    mrMark.mTextPoint.Y = mrMark.mInsertPoint.Y - CCommonPara.mPartMarkLength;

                    return mrMark;
                }
                else if (mSlopeType == MrSlopeType.LESSTHAN_ZERO)
                {
                    dblAngle = Math.Atan((mMrPart.mRightTopPoint.Y - mMrPart.mRightBottomPoint.Y) / (mMrPart.mRightBottomPoint.X - mMrPart.mRightTopPoint.X));

                    mrMark.mModelObject = mMrPart.mPartInDrawing;
                    mrMark.mInsertPoint = mMrPart.mRightBottomPoint;

                    double increaseXDistance = CCommonPara.mPartMarkLength / Math.Tan(dblAngle - 0.5);

                    mrMark.mTextPoint.X = mrMark.mInsertPoint.X + increaseXDistance;
                    mrMark.mTextPoint.Y = mrMark.mInsertPoint.Y - CCommonPara.mPartMarkLength;

                    return mrMark;
                }
                else 
                {
                    if (mMrPart.mLeftTopPoint.X < 0 && mMrPart.mRightTopPoint.X > 0)
                    {
                        return null;
                    }
                    if (mMrPart.mLeftTopPoint.X < mainBeamWidth && mMrPart.mRightTopPoint.X > mainBeamWidth)
                    {
                        return null;
                    }

                    //如果是多边形板;
                    if ((mMrPart.mBeamType == MrBeamType.CONTOURPLATE || mMrPart.mBeamType == MrBeamType.POLYBEAM)
                        && (mMrPart.IsHaveBolt() && CDimTools.GetInstance().IsTwoVectorParallel(zVector, normal)))
                    {
                        Point maxYPoint = new Point();
                        maxYPoint.Y = int.MaxValue;

                        foreach (CMrBoltArray mrBoltArray in mMrPart.GetBoltArrayList())
                        {
                            Point maxPoint = mrBoltArray.GetMaxXPoint();

                            if (maxPoint.Y < maxYPoint.Y)
                            {
                                maxYPoint = maxPoint;
                            }
                        }

                        dblAngle = CCommonPara.mPartMarkAngle;
                        mrMark.mModelObject = mMrPart.mPartInDrawing;

                        mrMark.mInsertPoint = maxYPoint;
                        mrMark.mInsertPoint.X = maxYPoint.X;
                        mrMark.mInsertPoint.Y = maxYPoint.Y - 300 / CCommonPara.mViewScale;
                        double increaseXDistance = CCommonPara.mPartMarkLength / Math.Tan(dblAngle);
                        mrMark.mTextPoint.X = mrMark.mInsertPoint.X + increaseXDistance;
                        mrMark.mTextPoint.Y = mrMark.mInsertPoint.Y - CCommonPara.mPartMarkLength;

                        return mrMark;
                    }
                    else
                    {
                        dblAngle = CCommonPara.mPartMarkAngle;
                        mrMark.mModelObject = mMrPart.mPartInDrawing;

                        mrMark.mInsertPoint = mMrPart.mRightBottomPoint;
                        mrMark.mInsertPoint.X = mMrPart.mRightBottomPoint.X - 5;
                        mrMark.mInsertPoint.Y = mMrPart.mRightBottomPoint.Y + 5;
                        double increaseXDistance = CCommonPara.mPartMarkLength / Math.Tan(dblAngle);
                        mrMark.mTextPoint.X = mrMark.mInsertPoint.X + increaseXDistance;
                        mrMark.mTextPoint.Y = mrMark.mInsertPoint.Y - CCommonPara.mPartMarkLength;

                        return mrMark;
                    }
                }
            }
            else if(mPostionType == MrPositionType.MIDDLE)
            {
                if (CDimTools.GetInstance().IsTwoVectorParallel(normal, yVector))
                {
                    return null;
                }

                //如果是中间左侧的双夹板;
                if (mMrPart.mLeftTopPoint.X < CCommonPara.mDblError && mMrPart.mRightTopPoint.X > CCommonPara.mDblError)
                {
                    return null;
                }

                //如果是中间右侧的双夹板;
                if (mMrPart.mLeftTopPoint.X < mainBeamWidth && mMrPart.mRightTopPoint.X > mainBeamWidth)
                {
                    return null;
                }

                mrMark.mModelObject = mMrPart.mPartInDrawing;
                mrMark.mInsertPoint = mMrPart.mLeftTopPoint;

                double increaseXDistance = CCommonPara.mPartMarkLength / Math.Tan(CCommonPara.mPartMarkAngle);
                mrMark.mTextPoint.X = mrMark.mInsertPoint.X + increaseXDistance;
                mrMark.mTextPoint.Y = mrMark.mInsertPoint.Y + CCommonPara.mPartMarkLength;

                return mrMark;
            }
            return null;
        }

        /// <summary>
        /// 判断该零件是否是垫块;
        /// </summary>
        /// <param name="mrPart"></param>
        /// <returns></returns>
        private bool IsPartTheHeelBlock(CMrPart mrPart)
        {
            Vector normal = mrPart.mNormal;
            Vector zVector = new Vector(0, 0, 1);

            if (!mMrPart.IsHaveBolt()
                && mMrPart.GetZSpaceValue() <= 8 
                && mMrPart.GetXSpaceValue() <= 30
                && mMrPart.GetYSpaceValue() >= CMrMainBeam.GetInstance().GetYSpaceValue()
                && CDimTools.GetInstance().IsTwoVectorParallel(normal, zVector))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 判断板是否贴在主梁的上下方;
        /// </summary>
        /// <param name="mrPart"></param>
        /// <returns></returns>
        private bool IsPartPosteOnOrUnderMainBeam(CMrPart mrPart)
        {
            Vector normal = mrPart.mNormal;

            if(!CDimTools.GetInstance().IsTwoVectorParallel(normal,new Vector(0,0,1)))
            {
                return false;
            }

            double mainBeamMinZ = CMrMainBeam.GetInstance().GetMinZPoint().Z;
            double mainBeamMaxZ = CMrMainBeam.GetInstance().GetMaxZPoint().Z;

            double maxZ = mrPart.GetMaxZPoint().Z;
            double minZ = mrPart.GetMinZPoint().Z;

            if (CDimTools.GetInstance().CompareTwoDoubleValue(minZ, mainBeamMaxZ) >= 0)
            {
                return true;
            }
            if (CDimTools.GetInstance().CompareTwoDoubleValue(maxZ, mainBeamMinZ) <= 0)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 判断零件是否在主梁翼缘的两侧,并且不高度不超过主梁翼缘的高度;
        /// </summary>
        /// <param name="mrPart"></param>
        /// <returns></returns>
        private bool IsPartInMainBeamFlange(CMrPart mrPart)
        {
            Vector normal = mrPart.mNormal;

            if (!CDimTools.GetInstance().IsTwoVectorParallel(normal, new Vector(0, 0, 1)))
            {
                return false;
            }

            double dblMinZ = mrPart.GetMinZPoint().Z;
            double dblMaxZ = mrPart.GetMaxZPoint().Z;

            double dblHalfZ = CMrMainBeam.GetInstance().GetZSpaceValue() / 2.0;
            double dblBeamFlange = CMrMainBeam.GetInstance().mFlangeThickness;

            if (CDimTools.GetInstance().CompareTwoDoubleValue(dblMinZ, dblHalfZ - dblBeamFlange) >= 0
                && CDimTools.GetInstance().CompareTwoDoubleValue(dblMaxZ, dblHalfZ) <= 0)
            {
                return true;
            }
            if (CDimTools.GetInstance().CompareTwoDoubleValue(dblMinZ, -dblHalfZ) >= 0
                && CDimTools.GetInstance().CompareTwoDoubleValue(dblMaxZ, -dblHalfZ + dblBeamFlange) <= 0)
            {
                return true;
            }
            return false;
        }
    }
}
