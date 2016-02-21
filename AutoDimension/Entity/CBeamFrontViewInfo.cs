using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tekla.Structures.Drawing;
using Tekla.Structures.Geometry3d;

using TSD = Tekla.Structures.Drawing;

namespace AutoDimension.Entity
{
    /// <summary>
    /// 记录零件在前视图中的信息;
    /// </summary>
    public class CBeamFrontViewInfo
    {
        /// <summary>
        /// 与该FrontView视图关联的Part对象;
        /// </summary>
        private CMrPart mMrPart;

        /// <summary>
        /// 模型的位置类型;
        /// </summary>
        public MrPositionType mPostionType { get; set; }

        /// <summary>
        /// 标志该零件是否左侧已经标注;
        /// </summary>
        public bool mIsLeftDim=false;

        /// <summary>
        /// 主梁最小Y值;
        /// </summary>
        private static double mainBeamMinY = 0.0;

        /// <summary>
        /// 主梁最大Y值;
        /// </summary>
        private static double mainBeamMaxY = 0.0;

        /// <summary>
        /// 部件顶视图信息;
        /// </summary>
        /// <param name="mrPart"></param>
        public CBeamFrontViewInfo(CMrPart mrPart)
        {
            this.mMrPart = mrPart;
        }

         /// <summary>
        /// 初始化该零件的信息,主要初始化零件的位置信息;
        /// </summary>
        public void InitMrPartFrontViewInfo()
        {
            if (mainBeamMinY == 0)
            {
                mainBeamMinY = CMrMainBeam.GetInstance().GetMinYPoint().Y;
            }
            if (mainBeamMaxY == 0)
            {
                mainBeamMaxY = CMrMainBeam.GetInstance().GetMaxYPoint().Y;
            }
            if (CDimTools.GetInstance().CompareTwoDoubleValue(mMrPart.GetMinYPoint().Y,mainBeamMaxY) >= 0)
            {
                mPostionType = MrPositionType.UP;
            }
            else if (CDimTools.GetInstance().CompareTwoDoubleValue(mMrPart.GetMaxYPoint().Y, mainBeamMinY) <= 0)
            {
                mPostionType = MrPositionType.DOWM;
            }
            else
            {
                mPostionType = MrPositionType.MIDDLE;
            }
        }

        /// <summary>
        /// 获取零件前视图上方的标注集合;
        /// </summary>
        /// <returns></returns>
        public CMrDimSet GetPartUpDimSet()
        {
            CMrDimSet mrDimSet=new CMrDimSet();
            Vector normal=mMrPart.mNormal;

            Vector xVector = new Vector(1, 0, 0);
            Vector yVector = new Vector(0, 1, 0);
            Vector zVector = new Vector(0, 0, 1);

            //1.如果零件在主梁的下方则不标注;
            if (mPostionType == MrPositionType.DOWM)
            {
                return null;
            }
            //2.判断角钢是否需要标注;
            if (CDimTools.GetInstance().IsPartTheAngleSteel(mMrPart))
            {
                return GetAngleSheetUpDim(mMrPart);
            }
            //3.如果是垫块，则不标注;
            if (IsPartTheBlockPlate(mMrPart))
            {
                return null;
            }
            //4.如果坐标不满足条件则不进行标注;
            double mainBeamLength = CMrMainBeam.GetInstance().GetXSpaceValue();

            if (mMrPart.GetMinXPoint().X < CCommonPara.mDblError || mMrPart.GetMaxXPoint().X > mainBeamLength)
            {
                return null;
            }
            //5.如果向量与Y轴平行则不标注,但是需要标注垂直方向的定位尺寸;
            if (CDimTools.GetInstance().IsTwoVectorParallel(normal, yVector))
            {
                return null;
            }
            //6.如果法向与X轴平行，标注垂直连接板垂直加筋板,会存在外围连接板;
            else if (CDimTools.GetInstance().IsTwoVectorParallel(normal, xVector) || CDimTools.GetInstance().IsVectorInXYPlane(normal))
            {
                return GetVerticalConnectPlateUpDim(mMrPart);
            }
            //7.如果法向与Z轴平行，标注水平连接板;
            else if (CDimTools.GetInstance().IsTwoVectorParallel(normal, zVector) )
            {
                return GetHorizontalConnentPlateUpDim(mMrPart);
            }
            //8.如果向量在XZ平面中;
            else if (CDimTools.GetInstance().IsVectorInXZPlane(normal))
            {
                return null;
            }
            else
            {
                mrDimSet.AddPoint(mMrPart.GetMaxYMinXPoint());
                return mrDimSet;
            }
        }

        /// <summary>
        /// 获得角钢的上标注，角钢上标注默认只标注在主梁翼缘上方的角钢;
        /// </summary>
        /// <returns></returns>
        private CMrDimSet GetAngleSheetUpDim(CMrPart mrPart)
        {
            CMrDimSet mrDimSet = new CMrDimSet();

            //上标注默认只标注主部件翼缘上方的角钢;
            if (mPostionType == MrPositionType.DOWM || mPostionType == MrPositionType.MIDDLE)
            {
                return null;
            }

            //前视图标注配置类;
            CBeamFrontViewSetting beamFrontViewSetting = CBeamDimSetting.GetInstance().mFrontViewSetting;

            bool bMarkValue = beamFrontViewSetting.FindMarkValueByName(CBeamFrontViewSetting.mstrAngleSheet);
            mMrPart.SetNeedAddMarkFlag(bMarkValue);

            bool bValue = beamFrontViewSetting.FindDimValueByName(CBeamFrontViewSetting.mstrAngleSheet);

            if (!bValue)
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
        /// 获得垂直连接板及加筋板以及外围连接板的上标注;
        /// </summary>
        /// <param name="mrPart"></param>
        /// <returns></returns>
        private CMrDimSet GetVerticalConnectPlateUpDim(CMrPart mrPart)
        {
            Vector normal = mrPart.mNormal;
            CMrDimSet mrDimSet = new CMrDimSet();

            //前视图标注配置类;
            CBeamFrontViewSetting beamFrontViewSetting = CBeamDimSetting.GetInstance().mFrontViewSetting;

            //1.如果是主部件上侧的外围连接板,可能会存在法向与X轴平行的H型钢;
            if (mPostionType==MrPositionType.UP)
            {
                //如果是H型钢,则属于支撑及牛腿类型;
                if (CDimTools.GetInstance().IsPartTheHProfileSheet(mrPart))
                {
                    bool bValue = beamFrontViewSetting.FindDimValueByName(CBeamFrontViewSetting.mstrSupportPlate);

                    bool bMarkValue = beamFrontViewSetting.FindMarkValueByName(CBeamFrontViewSetting.mstrSupportPlate);
                    mMrPart.SetNeedAddMarkFlag(bMarkValue);

                    if (!bValue)
                    {
                        return null;
                    }
                    if (CDimTools.GetInstance().IsTwoVectorParallel(normal, new Vector(1, 0, 0)))
                    {
                        Point maxYmaxXPt = mMrPart.GetMaxYMaxXPoint();
                        Point maxYminXPt = mMrPart.GetMaxYMinXPoint();
                        Point midPoint = new Point((maxYminXPt.X + maxYmaxXPt.X) / 2, maxYmaxXPt.Y, 0);
                        mrDimSet.AddPoint(midPoint);
                        return mrDimSet;
                    }
                }
                else 
                {
                    bool bValue = beamFrontViewSetting.FindDimValueByName(CBeamFrontViewSetting.mstrOutConnectPlate);

                    bool bMarkValue = beamFrontViewSetting.FindMarkValueByName(CBeamFrontViewSetting.mstrOutConnectPlate);
                    mMrPart.SetNeedAddMarkFlag(bMarkValue);

                    if (!bValue)
                    {
                        return null;
                    }
                    //如果向量与X轴平行，则为外围直的连接板;
                    if (CDimTools.GetInstance().IsTwoVectorParallel(normal, new Vector(1, 0, 0)))
                    {
                       mrDimSet.AddPoint(mMrPart.GetMaxYMinXPoint());
                       return mrDimSet;
                    }
                    //如果向量在XY平面内，则为倾斜的外围连接板，先不考虑标注，以后待扩展（因为与三块板拼牛腿冲突）;
                    else if (CDimTools.GetInstance().IsVectorInXYPlane(normal))
                    {
                        return null;
                    }
                    return null;
                }
            }
            //2.否则是中间的垂直加筋板及连接板,或者是倾斜的连接板;
            else
            {
                bool bMarkValue = beamFrontViewSetting.FindMarkValueByName(CBeamFrontViewSetting.mstrVerticalConnectPlate);
                mMrPart.SetNeedAddMarkFlag(bMarkValue);

                bool bValue = beamFrontViewSetting.FindDimValueByName(CBeamFrontViewSetting.mstrVerticalConnectPlate);

                if (!bValue)
                {
                    return null;
                }
                //如果向量与X轴平行，则为梁中间垂直的加筋板及连接板;
                if (CDimTools.GetInstance().IsTwoVectorParallel(normal, new Vector(1, 0, 0)))
                {
                    mrDimSet.AddPoint(mMrPart.GetMaxYMinXPoint());
                    return mrDimSet;
                }
                //如果向量在XY平面内，则为倾斜的加筋板或连接板;
                else if (CDimTools.GetInstance().IsVectorInXYPlane(normal))
                {
                    mrDimSet.AddPoint(mMrPart.GetMaxYPoint());
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
            CBeamFrontViewSetting beamFrontViewSetting = CBeamDimSetting.GetInstance().mFrontViewSetting;

            CMrDimSet mrDimSet = new CMrDimSet();

            //1.如果是水平无孔连接板;
            if (!mMrPart.IsHaveBolt())
            {
                //如果是板类零件;
                if (CDimTools.GetInstance().IsPartThePlate(mrPart))
                {
                    bool bMarkValue = beamFrontViewSetting.FindMarkValueByName(CBeamFrontViewSetting.mstrHorizontalConnentPlate);
                    mMrPart.SetNeedAddMarkFlag(bMarkValue);

                    bool bValue = beamFrontViewSetting.FindDimValueByName(CBeamFrontViewSetting.mstrHorizontalConnentPlate);

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
                    bool bMarkValue = beamFrontViewSetting.FindMarkValueByName(CBeamFrontViewSetting.mstrSupportPlate);
                    mMrPart.SetNeedAddMarkFlag(bMarkValue);

                    bool bValue = beamFrontViewSetting.FindDimValueByName(CBeamFrontViewSetting.mstrSupportPlate);

                    if (!bValue)
                    {
                        return null;
                    }
                    mrDimSet.AddPoint(mMrPart.GetMaxYMinXPoint());
                    return mrDimSet;
                }
            }
            //2.如果是水平有孔连接板及支撑牛腿等;
            else if (mMrPart.IsHaveBolt())
            {
                bool bMarkValue = beamFrontViewSetting.FindMarkValueByName(CBeamFrontViewSetting.mstrSupportPlate);
                mMrPart.SetNeedAddMarkFlag(bMarkValue);

                bool bValue = beamFrontViewSetting.FindDimValueByName(CBeamFrontViewSetting.mstrSupportPlate);

                if (!bValue)
                {
                    return null;
                }
                return GetBoltUpDimSet();
            }
            return null;
        }

        /// <summary>
        /// 获取前视图下方的标注集合;
        /// </summary>
        /// <returns></returns>
        public CMrDimSet GetPartDownDimSet()
        {
            CMrDimSet mrDimSet = new CMrDimSet();
            Vector normal = mMrPart.mNormal;

            Vector xVector = new Vector(1, 0, 0);
            Vector yVector = new Vector(0, 1, 0);
            Vector zVector = new Vector(0, 0, 1);

            //1.如果零件在主梁的上方则不标注;
            if (mPostionType == MrPositionType.UP)
            {
                return null;
            }
            //2.判断角钢是否需要标注;
            if (CDimTools.GetInstance().IsPartTheAngleSteel(mMrPart))
            {
                return GetAngleSheetDownDim(mMrPart);
            }
            //3.如果是垫块，则不标注;
            if (IsPartTheBlockPlate(mMrPart))
            {
                return null;
            }
            //4.如果坐标不满足条件则不进行标注;
            double mainBeamLength = CMrMainBeam.GetInstance().GetXSpaceValue();

            if (mMrPart.GetMinXPoint().X < CCommonPara.mDblError || mMrPart.GetMaxXPoint().X > mainBeamLength)
            {
                return null;
            }
            //5.如果向量与Y轴平行则不标注,但是需要标注垂直方向的定位尺寸;
            if (CDimTools.GetInstance().IsTwoVectorParallel(normal, yVector))
            {
                return null;
            }
            //6.如果法向与X轴平行，标注垂直连接板垂直加筋板,会存在外围连接板;
            else if (CDimTools.GetInstance().IsTwoVectorParallel(normal, xVector) || CDimTools.GetInstance().IsVectorInXYPlane(normal))
            {
                return GetVerticalConnectPlateDownDim(mMrPart);
            }
            //7.如果法向与Z轴平行，标注水平连接板;
            else if (CDimTools.GetInstance().IsTwoVectorParallel(normal, zVector))
            {
                return GetHorizontalConnentPlateDownDim(mMrPart);
            }
            //8.如果向量在XZ平面中;
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
        /// 获得角钢的下标注，角钢下标注默认标注在主梁翼缘下方的角钢和主梁中间的角钢;
        /// </summary>
        /// <returns></returns>
        private CMrDimSet GetAngleSheetDownDim(CMrPart mrPart)
        {
            CMrDimSet mrDimSet = new CMrDimSet();

            if (mPostionType == MrPositionType.UP)
            {
                return null;
            }

            //前视图标注配置类;
            CBeamFrontViewSetting beamFrontViewSetting = CBeamDimSetting.GetInstance().mFrontViewSetting;

            bool bMarkValue = beamFrontViewSetting.FindMarkValueByName(CBeamFrontViewSetting.mstrAngleSheet);
            mMrPart.SetNeedAddMarkFlag(bMarkValue);

            bool bValue = beamFrontViewSetting.FindDimValueByName(CBeamFrontViewSetting.mstrAngleSheet);

            if (!bValue)
            {
                return null;
            }

            //1.如果角钢在主梁翼缘的下方;
            if (mPostionType == MrPositionType.DOWM)
            {
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
            //2.如果角钢在主梁中间,需要判断角钢是否有螺钉法向与Z轴平行，如果有则标注螺钉;
            else if (mPostionType == MrPositionType.MIDDLE)
            {
                bool bBoltDim = false;
                List<CMrBoltArray> mrBoltArrayList = mMrPart.GetBoltArrayList();

                foreach (CMrBoltArray mrBoltArray in mrBoltArrayList)
                {
                    Vector boltNormal = mrBoltArray.mNormal;

                    if (CDimTools.GetInstance().IsTwoVectorParallel(boltNormal, new Vector(0, 0, 1)))
                    {
                        bBoltDim = true;
                        mrDimSet.AddRange(mrBoltArray.GetBoltPointList());
                    }
                }

                //如果没有螺钉,需要判断一下角钢的厚度在左边还是在右边;
                if (bBoltDim == false)
                {
                    Point maxZminXPt = mMrPart.GetMaxZMinXPoint();
                    Point minXPt = mMrPart.GetMinXPoint();
                    Point maxZmaxXPt = mMrPart.GetMaxZMaxXPoint();
                    Point maxXPt = mMrPart.GetMaxXPoint();

                    if (CDimTools.GetInstance().CompareTwoDoubleValue(minXPt.X, maxZminXPt.X) == 0)
                    {
                        mrDimSet.AddPoint(mMrPart.GetMinYMinXPoint());
                    }
                    else if (CDimTools.GetInstance().CompareTwoDoubleValue(maxXPt.X, maxZmaxXPt.X) == 0)
                    {
                        mrDimSet.AddPoint(mMrPart.GetMinYMaxXPoint());
                    }
                    else
                    {
                        mrDimSet.AddPoint(mMrPart.GetMinYMinXPoint());
                    }
                }
                return mrDimSet;
            }
            return null;
        }

        /// <summary>
        /// 获得垂直连接板及加筋板以及外围连接板的下标注,由于中间的垂直连接板在上方已经标注，故下标注只进行下外围连接板的标注;
        /// </summary>
        /// <param name="mrPart"></param>
        /// <returns></returns>
        private CMrDimSet GetVerticalConnectPlateDownDim(CMrPart mrPart)
        {
            Vector normal = mrPart.mNormal;
            CMrDimSet mrDimSet = new CMrDimSet();

            //如果是上面或中间的垂直连接板或外围连接板 直接返回; 如果是主部件下侧的外围连接板,可能会存在法向与X轴平行的H型钢;
            if (mPostionType == MrPositionType.UP || mPostionType == MrPositionType.MIDDLE)
            {
                return null;
            }

            //前视图标注配置类;
            CBeamFrontViewSetting beamFrontViewSetting = CBeamDimSetting.GetInstance().mFrontViewSetting;

            //如果是H型钢,则属于支撑及牛腿类型;
            if (CDimTools.GetInstance().IsPartTheHProfileSheet(mrPart))
            {
                bool bMarkValue = beamFrontViewSetting.FindMarkValueByName(CBeamFrontViewSetting.mstrSupportPlate);
                mMrPart.SetNeedAddMarkFlag(bMarkValue);

                bool bValue = beamFrontViewSetting.FindDimValueByName(CBeamFrontViewSetting.mstrSupportPlate);

                if (!bValue)
                {
                    return null;
                }
                if (CDimTools.GetInstance().IsTwoVectorParallel(normal, new Vector(1, 0, 0)))
                {
                    Point minYmaxXPt = mMrPart.GetMinYMaxXPoint();
                    Point minYminXPt = mMrPart.GetMinYMinXPoint();
                    Point midPoint = new Point((minYminXPt.X + minYmaxXPt.X) / 2, minYmaxXPt.Y, 0);
                    mrDimSet.AddPoint(midPoint);
                    return mrDimSet;
                }
            }
            else
            {
                bool bMarkValue = beamFrontViewSetting.FindMarkValueByName(CBeamFrontViewSetting.mstrOutConnectPlate);
                mMrPart.SetNeedAddMarkFlag(bMarkValue);

                bool bValue = beamFrontViewSetting.FindDimValueByName(CBeamFrontViewSetting.mstrOutConnectPlate);

                if (!bValue)
                {
                    return null;
                }
                //如果向量与X轴平行，则为外围直的连接板;
                if (CDimTools.GetInstance().IsTwoVectorParallel(normal, new Vector(1, 0, 0)))
                {
                    mrDimSet.AddPoint(mMrPart.GetMinYMinXPoint());
                    return mrDimSet;
                }
                //如果向量在XY平面内，则为倾斜的外围连接板，先不考虑标注，以后待扩展（因为与三块板拼牛腿冲突）;
                else if (CDimTools.GetInstance().IsVectorInXYPlane(normal))
                {
                    return null;
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
            CBeamFrontViewSetting beamFrontViewSetting = CBeamDimSetting.GetInstance().mFrontViewSetting;

            CMrDimSet mrDimSet = new CMrDimSet();

            //1.如果是水平无孔连接板;
            if (!mMrPart.IsHaveBolt())
            {
                //如果是板类零件;
                if (CDimTools.GetInstance().IsPartThePlate(mrPart))
                {
                    bool bMarkValue = beamFrontViewSetting.FindMarkValueByName(CBeamFrontViewSetting.mstrHorizontalConnentPlate);
                    mMrPart.SetNeedAddMarkFlag(bMarkValue);

                    bool bValue = beamFrontViewSetting.FindDimValueByName(CBeamFrontViewSetting.mstrHorizontalConnentPlate);

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
                    bool bMarkValue = beamFrontViewSetting.FindMarkValueByName(CBeamFrontViewSetting.mstrSupportPlate);
                    mMrPart.SetNeedAddMarkFlag(bMarkValue);

                    bool bValue = beamFrontViewSetting.FindDimValueByName(CBeamFrontViewSetting.mstrSupportPlate);

                    if (!bValue)
                    {
                        return null;
                    }
                    mrDimSet.AddPoint(mMrPart.GetMinYMinXPoint());
                    return mrDimSet;
                }
            }
            //2.如果是水平有孔连接板及支撑牛腿等;
            else if (mMrPart.IsHaveBolt())
            {
                bool bMarkValue = beamFrontViewSetting.FindMarkValueByName(CBeamFrontViewSetting.mstrSupportPlate);
                mMrPart.SetNeedAddMarkFlag(bMarkValue);

                bool bValue = beamFrontViewSetting.FindDimValueByName(CBeamFrontViewSetting.mstrSupportPlate);

                if (!bValue)
                {
                    return null;
                }
                return GetBoltDownDimSet();
            }
            return null;
        }

        /// <summary>
        /// 获取在水平方向上方进行标注的螺钉点的链表;
        /// </summary>
        /// <returns></returns>
        public CMrDimSet GetBoltUpDimSet()
        {
            if (!mMrPart.IsHaveBolt())
            {
                return null;
            }
            if (mMrPart.GetBeamFrontViewInfo().mPostionType == MrPositionType.DOWM)
            {
                return null;
            }
            CMrDimSet mrDimSet = new CMrDimSet();
            Vector zVector = new Vector(0, 0, 1);

            double mainBeamWidth = CMrMainBeam.GetInstance().GetXSpaceValue();

            List<CMrBoltArray> newMrBoltArrayList = new List<CMrBoltArray>();

            //找到所有法向与Z轴平行的螺钉组;
            foreach (CMrBoltArray boltArray in mMrPart.GetBoltArrayList())
            {
                if (CDimTools.GetInstance().IsTwoVectorParallel(zVector, boltArray.mNormal))
                {
                    newMrBoltArrayList.Add(boltArray);
                }
            }

            int nCount = newMrBoltArrayList.Count;

            if (nCount == 0)
            {
                return null;
            }

            //获得X值最小Y值最大的螺钉组;
            CMrBoltArray mrBoltArray = CDimTools.GetInstance().GetMinXMaxYMrBoltArray(newMrBoltArrayList);
            
            List<Point> boltPointList = mrBoltArray.GetBoltPointList();

            //得到所有最大Y值中，X值最小的螺钉孔;
            if (mrBoltArray.mBoltArrayShapeType == MrBoltArrayShapeType.ARRAY)
            {
                List<Point> maxYPointList = mrBoltArray.GetMaxYPointList();

                Point firstPt = maxYPointList[0];

                foreach (Point point in maxYPointList)
                {
                    if (point.X < firstPt.X && point.X > 0 && point.X < mainBeamWidth)
                    {
                        firstPt = point;
                    }
                }
                mrDimSet.AddPoint(firstPt);
            }
            else if (mrBoltArray.mBoltArrayShapeType == MrBoltArrayShapeType.OBLIQUELINE)
            {
                if (boltPointList.Count < 4)
                {
                    mrDimSet.AddPoint(mrBoltArray.GetMaxYPoint());
                }
                else
                {
                    Point maxYPoint = mrBoltArray.GetMaxYPoint();
                    Point firstPoint = mrBoltArray.mFirstPoint;
                    Point secondPoint = mrBoltArray.mSecondPoint;
                    Vector directionVector = new Vector(secondPoint.X - firstPoint.X, secondPoint.Y - firstPoint.Y, secondPoint.Z - firstPoint.Z);

                    List<Point> boltDimPointList = new List<Point>();

                    foreach (Point boltPoint in boltPointList)
                    {
                        Vector newVector = new Vector(boltPoint.X - maxYPoint.X, boltPoint.Y - maxYPoint.Y, boltPoint.Z - maxYPoint.Z);
                            
                        if (CDimTools.GetInstance().IsTwoVectorVertical(directionVector, newVector))
                        {
                            boltDimPointList.Add(boltPoint);
                        }
                    }
                    CDimTools.GetInstance().GetMinXPoint(boltDimPointList);
                    mrDimSet.AddPoint(boltDimPointList[0]);
                }
            }
            return mrDimSet;
        }

        /// <summary>
        /// 获取在水平方向下方进行标注的螺钉点的链表;
        /// </summary>
        /// <returns></returns>
        public CMrDimSet GetBoltDownDimSet()
        {
            if (!mMrPart.IsHaveBolt())
            {
                return null;
            }
            
            if (mMrPart.GetBeamFrontViewInfo().mPostionType == MrPositionType.UP)
            {
                return null;
            }

            CMrDimSet mrDimSet = new CMrDimSet();
            Vector zVector = new Vector(0, 0, 1);

            double mainBeamWidth = CMrMainBeam.GetInstance().GetXSpaceValue();

            List<CMrBoltArray> newMrBoltArrayList = new List<CMrBoltArray>();

            //找到所有法向与Z轴平行的螺钉组;
            foreach (CMrBoltArray boltArray in mMrPart.GetBoltArrayList())
            {
                if (CDimTools.GetInstance().IsTwoVectorParallel(zVector, boltArray.mNormal))
                {
                    newMrBoltArrayList.Add(boltArray);
                }
            }

            int nCount = newMrBoltArrayList.Count;

            if (nCount == 0)
            {
                return null;
            }

            //获得X值最小Y值最小的螺钉组;
            CMrBoltArray mrBoltArray = CDimTools.GetInstance().GetMinXMinYMrBoltArray(newMrBoltArrayList);

            List<Point> boltPointList = mrBoltArray.GetBoltPointList();

            //得到所有最小Y值中，X值最小的螺钉孔;
            if (mrBoltArray.mBoltArrayShapeType == MrBoltArrayShapeType.ARRAY)
            {
                List<Point> minYPointList = mrBoltArray.GetMinYPointList();

                Point firstPt = minYPointList[0];

                foreach (Point point in minYPointList)
                {
                    if (point.X < firstPt.X && point.X > 0 && point.X < mainBeamWidth)
                    {
                        firstPt = point;
                    }
                }
                mrDimSet.AddPoint(firstPt);
            }
            else if (mrBoltArray.mBoltArrayShapeType == MrBoltArrayShapeType.OBLIQUELINE)
            {
                if (boltPointList.Count < 4)
                {
                    mrDimSet.AddPoint(mrBoltArray.GetMinYPoint());
                }
                else
                {
                    Point minYPoint = mrBoltArray.GetMinYPoint();
                    Point firstPoint = mrBoltArray.mFirstPoint;
                    Point secondPoint = mrBoltArray.mSecondPoint;
                    Vector directionVector = new Vector(secondPoint.X - firstPoint.X, secondPoint.Y - firstPoint.Y, secondPoint.Z - firstPoint.Z);

                    List<Point> boltDimPointList = new List<Point>();

                    foreach (Point boltPoint in boltPointList)
                    {
                        Vector newVector = new Vector(boltPoint.X - minYPoint.X, boltPoint.Y - minYPoint.Y, boltPoint.Z - minYPoint.Z);

                        if (CDimTools.GetInstance().IsTwoVectorVertical(directionVector, newVector))
                        {
                            boltDimPointList.Add(boltPoint);
                        }
                    }

                    CDimTools.GetInstance().GetMinXPoint(boltDimPointList);
                    mrDimSet.AddPoint(boltDimPointList[0]);
                }
            }
            return mrDimSet;
        }

        /// <summary>
        /// 获取零件自身的螺钉组的标注,主要针对水平连接板中的有孔螺钉和竖直的H型钢;
        /// </summary>
        /// <returns></returns>
        public List<CMrDimSet> GetPartBoltDimSetList()
        {
            Vector normal = mMrPart.mNormal;

            List<CMrDimSet> mrDimSetList = new List<CMrDimSet>();

            if (!mMrPart.IsHaveBolt())
            {
                return null;
            }

            //1.如果是垂直的H型钢,螺钉的法向也有可能与Z轴平行,H型钢的螺钉一般都是ARRAY类型;
            if (CDimTools.GetInstance().IsTwoVectorParallel(normal, new Vector(1, 0, 0))
                && CDimTools.GetInstance().IsPartTheHProfileSheet(mMrPart))
            {
                CBeamFrontViewSetting beamFrontViewSetting = CBeamDimSetting.GetInstance().mFrontViewSetting;

                bool bValue = beamFrontViewSetting.FindDimValueByName(CBeamFrontViewSetting.mstrSupportPlate);

                if (!bValue)
                {
                    return null;
                }

                //获得所有法向与Z轴平行的螺钉组;
                List<CMrBoltArray> newMrBoltArrayList = new List<CMrBoltArray>();

                foreach (CMrBoltArray mrBoltArray in mMrPart.GetBoltArrayList())
                {
                    if (CDimTools.GetInstance().IsTwoVectorParallel(mrBoltArray.mNormal, new Vector(0, 0, 1)))
                    {
                        newMrBoltArrayList.Add(mrBoltArray);
                    }
                }
                //A.如果法向与Z轴平行的螺钉组个数为0;
                if (newMrBoltArrayList.Count == 0)
                {
                    return null;
                }
                //B.如果法向与Z轴平行的螺钉组个数为1;
                if (newMrBoltArrayList.Count == 1)
                {
                    CMrBoltArray mrBoltArray = newMrBoltArrayList[0];

                    return GetOneBoltArrayDimSetListInner(mrBoltArray);
                }
                else
                {
                    return GetMoreThanOneBoltArrayDimSetListInner(newMrBoltArrayList);
                }
            }
            //2.如果是水平连接板;
            else if (CDimTools.GetInstance().IsTwoVectorParallel(normal, new Vector(0, 0, 1)))
            {
                CBeamFrontViewSetting beamFrontViewSetting = CBeamDimSetting.GetInstance().mFrontViewSetting;

                bool bValue = beamFrontViewSetting.FindDimValueByName(CBeamFrontViewSetting.mstrSupportPlate);

                if (!bValue)
                {
                    return null;
                }

                //获得所有法向与Z轴平行的螺钉组;
                List<CMrBoltArray> newMrBoltArrayList = new List<CMrBoltArray>();

                foreach (CMrBoltArray mrBoltArray in mMrPart.GetBoltArrayList())
                {
                    if (CDimTools.GetInstance().IsTwoVectorParallel(mrBoltArray.mNormal, new Vector(0, 0, 1)))
                    {
                        newMrBoltArrayList.Add(mrBoltArray);
                    }
                }

                //如果该水平连接板在翼缘上方或者在翼缘下方;
                if (mPostionType == MrPositionType.UP || mPostionType == MrPositionType.DOWM)
                {
                    //A.如果法向与Z轴平行的螺钉组个数为0;
                    if (newMrBoltArrayList.Count == 0)
                    {
                        return null;
                    }
                    //B.如果法向与Z轴平行的螺钉组个数为1;
                    if (newMrBoltArrayList.Count == 1)
                    {
                        CMrBoltArray mrBoltArray = newMrBoltArrayList[0];
                        return GetOneBoltArrayDimSetListInner(mrBoltArray);
                    }
                    else
                    {
                        return GetMoreThanOneBoltArrayDimSetListInner(newMrBoltArrayList);
                    }
                }
                //如果该水平连接板在中间;
                else if (mPostionType == MrPositionType.MIDDLE)
                {
                    List<CMrBoltArray> upMrBoltArrayList = new List<CMrBoltArray>();
                    List<CMrBoltArray> downMrBoltArrayList = new List<CMrBoltArray>();

                    foreach (CMrBoltArray mrBoltArray in newMrBoltArrayList)
                    {
                        if (mrBoltArray.GetMinYPoint().Y > 0)
                        {
                            upMrBoltArrayList.Add(mrBoltArray);
                        }
                        if (mrBoltArray.GetMaxYPoint().Y < 0)
                        {
                            downMrBoltArrayList.Add(mrBoltArray);
                        }
                    }
                    
                    if (upMrBoltArrayList.Count == 1)
                    {
                        return GetOneBoltArrayDimSetListInner(upMrBoltArrayList[0]);
                    }
                    else if(upMrBoltArrayList.Count > 1)
                    {
                        return GetMoreThanOneBoltArrayDimSetListInner(upMrBoltArrayList);
                    }

                    if (downMrBoltArrayList.Count == 1)
                    {
                        return GetOneBoltArrayDimSetListInner(downMrBoltArrayList[0]);
                    }
                    else if(downMrBoltArrayList.Count > 1)
                    {
                        return GetMoreThanOneBoltArrayDimSetListInner(downMrBoltArrayList);
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// 获得一个螺钉组时螺钉标注集;
        /// </summary>
        /// <returns></returns>
        private List<CMrDimSet> GetOneBoltArrayDimSetListInner(CMrBoltArray mrBoltArray)
        {
            List<CMrDimSet> mrDimSetList=new List<CMrDimSet>();
             
            if (mrBoltArray.mBoltArrayShapeType == MrBoltArrayShapeType.ARRAY)
            {                   
                //如果该螺钉在主梁上方;
                if (mrBoltArray.GetMaxYPoint().Y > mainBeamMaxY)
                {
                    List<Point> maxYPointList = mrBoltArray.GetMaxYPointList();
                    List<Point> minXPointList = mrBoltArray.GetMinXPointList();

                    //水平方向的标注;
                    CMrDimSet mrDimSet = new CMrDimSet();
                    mrDimSet.AddRange(maxYPointList);
                    mrDimSet.mDimVector = new Vector(0, 1, 0);
                    mrDimSet.mDimDistance = mMrPart.GetUpDefaultDimDistance(maxYPointList[0]);
                    mrDimSetList.Add(mrDimSet);

                    //垂直方向的标注;
                    mrDimSet = new CMrDimSet();
                    mrDimSet.AddRange(minXPointList);
                    mrDimSet.AddPoint(new Point(minXPointList[0].X, mainBeamMaxY, 0));

                    mrDimSet.mDimVector = new Vector(-1, 0, 0);
                    mrDimSet.mDimDistance = mMrPart.GetLeftDefaultDimDistance(minXPointList[0]);
                    mrDimSetList.Add(mrDimSet);

                    return mrDimSetList;
                }
                //如果该螺钉在主梁下方;
                else if (mrBoltArray.GetMinYPoint().Y < mainBeamMinY)
                {
                    List<Point> minYPointList = mrBoltArray.GetMinYPointList();
                    List<Point> minXPointList = mrBoltArray.GetMinXPointList();

                    //水平方向的标注;
                    CMrDimSet mrDimSet = new CMrDimSet();
                    mrDimSet.AddRange(minYPointList);
                    mrDimSet.mDimVector = new Vector(0, -1, 0);
                    mrDimSet.mDimDistance = mMrPart.GetDownDefaultDimDistance(minXPointList[0]);
                    mrDimSetList.Add(mrDimSet);

                    //垂直方向的标注;
                    mrDimSet = new CMrDimSet();
                    mrDimSet.AddRange(minXPointList);
                    mrDimSet.AddPoint(new Point(minXPointList[0].X, mainBeamMinY, 0));
                    mrDimSet.mDimVector = new Vector(-1, 0, 0);
                    mrDimSet.mDimDistance = mMrPart.GetLeftDefaultDimDistance(minXPointList[0]);
                    mrDimSetList.Add(mrDimSet);

                    return mrDimSetList;
                }
            }
            else if (mrBoltArray.mBoltArrayShapeType == MrBoltArrayShapeType.OBLIQUELINE)
            {
                //如果在主梁上方;
                if (mrBoltArray.GetMaxYPoint().Y > mainBeamMaxY)
                {
                    //判断螺钉组中所有螺钉是否在一条直线上;
                    if (CDimTools.GetInstance().IsObliqueBoltOnLine(mrBoltArray))
                    {
                        //水平方向的标注;
                        CMrDimSet mrDimSet = new CMrDimSet();
                        mrDimSet.AddPoint(mrBoltArray.GetMaxYPoint());
                        mrDimSet.AddPoint(mrBoltArray.GetMinYPoint());
                        mrDimSet.mDimVector = new Vector(0, 1, 0);
                        mrDimSet.mDimDistance = mMrPart.GetUpDefaultDimDistance(mrBoltArray.GetMaxYPoint());
                        mrDimSetList.Add(mrDimSet);

                        //垂直方向的标注;
                        mrDimSet = new CMrDimSet();
                        mrDimSet.AddPoint(mrBoltArray.GetMaxYPoint());
                        mrDimSet.AddPoint(mrBoltArray.GetMinYPoint());
                        mrDimSet.AddPoint(new Point(mrBoltArray.GetMinYPoint().X, mainBeamMaxY, 0));
                        Vector dimVector = GetBoltObliqueLineDimVector(mrBoltArray);

                        if (dimVector.X > 0)
                        {
                            mrDimSet.mDimDistance = mMrPart.GetRightDefaultDimDistance(mrDimSet.GetDimPointList()[0]);
                        }
                        else if (dimVector.X < 0)
                        {
                            mrDimSet.mDimDistance = mMrPart.GetLeftDefaultDimDistance(mrDimSet.GetDimPointList()[0]);
                        }
                        mrDimSet.mDimVector = dimVector;
                        mrDimSetList.Add(mrDimSet);

                        return mrDimSetList;
                    }
                    //否则所有螺钉组不在一条直线上;
                    else
                    {
                        CMrDimSet xMrDimSet = new CMrDimSet();
                        CMrDimSet yMrDimSet = new CMrDimSet();

                        Point firstPt = mrBoltArray.mFirstPoint;
                        Point secondPt = mrBoltArray.mSecondPoint;
                        Point maxYPoint = mrBoltArray.GetMaxYPoint();
                        Vector directionVector = new Vector(secondPt.X - firstPt.X, secondPt.Y - firstPt.Y, secondPt.Z - firstPt.Z);
                        
                        foreach (Point boltPoint in mrBoltArray.GetBoltPointList())
                        {
                            Vector newVector = new Vector(boltPoint.X - maxYPoint.X, boltPoint.Y - maxYPoint.Y, boltPoint.Z - maxYPoint.Z);

                            if (CDimTools.GetInstance().IsTwoVectorVertical(directionVector, newVector))
                            {
                                xMrDimSet.AddPoint(boltPoint);
                                yMrDimSet.AddPoint(boltPoint);
                            }
                        }
                        if (xMrDimSet.Count <= 1)
                        {
                            return null;
                        }

                        //水平方向的标注;
                        xMrDimSet.mDimVector = new Vector(0, 1, 0);
                        xMrDimSet.mDimDistance = mMrPart.GetUpDefaultDimDistance(xMrDimSet.GetDimPointList()[0]);
                        mrDimSetList.Add(xMrDimSet);

                        //垂直方向的标注;
                        Vector dimVector = GetBoltObliqueLineDimVector(mrBoltArray);
                        if (dimVector.X > 0)
                        {
                            yMrDimSet.mDimDistance = mMrPart.GetRightDefaultDimDistance(yMrDimSet.GetDimPointList()[0]);
                        }
                        else if (dimVector.X < 0)
                        {
                            yMrDimSet.mDimDistance = mMrPart.GetLeftDefaultDimDistance(yMrDimSet.GetDimPointList()[0]);
                        }
                        yMrDimSet.AddPoint(new Point(yMrDimSet.GetDimPointList()[0].X, mainBeamMaxY, 0));
                        yMrDimSet.mDimVector = dimVector;
                        mrDimSetList.Add(yMrDimSet);

                        return mrDimSetList;
                    }
                }
                //如果在主梁的下方;
                else if (mrBoltArray.GetMinYPoint().Y < mainBeamMinY)
                {
                    //判断螺钉组中所有螺钉是否在一条直线上;
                    if (CDimTools.GetInstance().IsObliqueBoltOnLine(mrBoltArray))
                    {
                        //水平方向的标注;
                        CMrDimSet mrDimSet = new CMrDimSet();
                        mrDimSet.AddPoint(mrBoltArray.GetMinYPoint());
                        mrDimSet.AddPoint(mrBoltArray.GetMaxYPoint());
                        mrDimSet.mDimVector = new Vector(0, -1, 0);
                        mrDimSet.mDimDistance = mMrPart.GetDownDefaultDimDistance(mrDimSet.GetDimPointList()[0]);
                        mrDimSetList.Add(mrDimSet);

                        //垂直方向的标注;
                        mrDimSet = new CMrDimSet();
                        mrDimSet.AddPoint(mrBoltArray.GetMinYPoint());
                        mrDimSet.AddPoint(mrBoltArray.GetMaxYPoint());
                        mrDimSet.AddPoint(new Point(mrBoltArray.GetMinYPoint().X, mainBeamMinY, 0));
                        Vector dimVector = GetBoltObliqueLineDimVector(mrBoltArray);

                        if (dimVector.X > 0)
                        {
                            mrDimSet.mDimDistance = mMrPart.GetRightDefaultDimDistance(mrDimSet.GetDimPointList()[0]);
                        }
                        else if (dimVector.X < 0)
                        {
                            mrDimSet.mDimDistance = mMrPart.GetLeftDefaultDimDistance(mrDimSet.GetDimPointList()[0]);
                        }
                        mrDimSet.mDimVector = dimVector;
                        mrDimSetList.Add(mrDimSet);

                        return mrDimSetList;
                    }
                    //否则所有螺钉组不在一条直线上;
                    else
                    {
                        CMrDimSet xMrDimSet = new CMrDimSet();
                        CMrDimSet yMrDimSet = new CMrDimSet();

                        Point firstPt = mrBoltArray.mFirstPoint;
                        Point secondPt = mrBoltArray.mSecondPoint;
                        Point minYPoint = mrBoltArray.GetMinYPoint();
                        Vector directionVector = new Vector(secondPt.X - firstPt.X, secondPt.Y - firstPt.Y, secondPt.Z - firstPt.Z);

                        foreach (Point boltPoint in mrBoltArray.GetBoltPointList())
                        {
                            Vector newVector = new Vector(boltPoint.X - minYPoint.X, boltPoint.Y - minYPoint.Y, boltPoint.Z - minYPoint.Z);

                            if (CDimTools.GetInstance().IsTwoVectorVertical(directionVector, newVector))
                            {
                                xMrDimSet.AddPoint(boltPoint);
                                yMrDimSet.AddPoint(boltPoint);
                            }
                        }
                        if (xMrDimSet.Count <= 1)
                        {
                            return null;
                        }

                        //水平方向的标注;
                        xMrDimSet.mDimVector = new Vector(0, -1, 0);
                        xMrDimSet.mDimDistance = mMrPart.GetDownDefaultDimDistance(xMrDimSet.GetDimPointList()[0]);
                        mrDimSetList.Add(xMrDimSet);

                        //垂直方向的标注;
                        Vector dimVector = GetBoltObliqueLineDimVector(mrBoltArray);
                        if (dimVector.X > 0)
                        {
                            yMrDimSet.mDimDistance = mMrPart.GetRightDefaultDimDistance(yMrDimSet.GetDimPointList()[0]);
                        }
                        else if (dimVector.X < 0)
                        {
                            yMrDimSet.mDimDistance = mMrPart.GetLeftDefaultDimDistance(yMrDimSet.GetDimPointList()[0]);
                        }
                        yMrDimSet.AddPoint(new Point(yMrDimSet.GetDimPointList()[0].X, mainBeamMinY, 0));
                        yMrDimSet.mDimVector = dimVector;
                        mrDimSetList.Add(yMrDimSet);

                        return mrDimSetList;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// 获得超过一个螺钉组的螺钉标注集;
        /// </summary>
        /// <param name="mrBoltArray"></param>
        /// <returns></returns>
        private List<CMrDimSet> GetMoreThanOneBoltArrayDimSetListInner(List<CMrBoltArray> mrBoltArrayList)
        {
            List<CMrDimSet> mrDimSetList = new List<CMrDimSet>();

            CDimTools.GetInstance().SortMrBoltArrayByMinX(mrBoltArrayList);

            List<CMrBoltArrayGroup> mrBoltArrayGroupList = new List<CMrBoltArrayGroup>();

            Dictionary<Vector, CMrBoltArrayGroup> mapNormalToMrBoltArrayGroup = new Dictionary<Vector, CMrBoltArrayGroup>();

            foreach (CMrBoltArray mrBoltArray in mrBoltArrayList)
            {
                Point firstPt = mrBoltArray.mFirstPoint;
                Point secondPt = mrBoltArray.mSecondPoint;

                Vector normal = new Vector(secondPt.X - firstPt.X, secondPt.Y - firstPt.Y, secondPt.Z - firstPt.Z);
                normal.Normalize();

                if (!mapNormalToMrBoltArrayGroup.ContainsKey(normal))
                {
                    CMrBoltArrayGroup mrBoltArrayGroup=new CMrBoltArrayGroup();
                    mrBoltArrayGroup.AppendMrBoltArray(mrBoltArray);
                    mapNormalToMrBoltArrayGroup.Add(normal, mrBoltArrayGroup);
                    mrBoltArrayGroupList.Add(mrBoltArrayGroup);
                }
                else
                {
                    CMrBoltArrayGroup mrBoltArrayGroup = mapNormalToMrBoltArrayGroup[normal];
                    mrBoltArrayGroup.AppendMrBoltArray(mrBoltArray);
                }
            }
            if (mrBoltArrayGroupList.Count == 0)
            {
                return null;
            }

            CMrBoltArrayGroup firstMrBoltArrayGroup = mrBoltArrayGroupList[0];


            //如果零件在主梁上方;
            if (firstMrBoltArrayGroup.GetMinYPoint().Y > 0)
            {
                CMrDimSet xMrDimSet = new CMrDimSet();

                //根据螺钉组合来进行螺钉组的标注;
                foreach (CMrBoltArrayGroup mrBoltArrayGroup in mrBoltArrayGroupList)
                {
                    List<CMrBoltArray> newMrBoltArrayList = mrBoltArrayGroup.mrBoltArrayList;

                    CMrBoltArray firstBoltArray = newMrBoltArrayList[0];

                    //1.如果螺钉是数组排列;
                    if (firstBoltArray.mBoltArrayShapeType == MrBoltArrayShapeType.ARRAY)
                    {
                        CMrDimSet yMrDimSet = new CMrDimSet();

                        foreach (CMrBoltArray mrBoltArray in newMrBoltArrayList)
                        {
                            List<Point> minXPointList = mrBoltArray.GetMinXPointList();
                            List<Point> maxYPointList = mrBoltArray.GetMaxYPointList();

                            yMrDimSet.AddRange(minXPointList);
                            xMrDimSet.AddRange(maxYPointList);
                        }

                        yMrDimSet.AddPoint(new Point(firstBoltArray.GetMinYPoint().X, mainBeamMaxY, 0));

                        yMrDimSet.mDimVector = new Vector(-1, 0, 0);
                        yMrDimSet.mDimDistance = mMrPart.GetLeftDefaultDimDistance(yMrDimSet.GetDimPointList()[0]);
                            
                        mrDimSetList.Add(yMrDimSet);
                    }
                    else if (firstBoltArray.mBoltArrayShapeType == MrBoltArrayShapeType.OBLIQUELINE)
                    {
                        CMrDimSet yMrDimSet = new CMrDimSet();

                        foreach (CMrBoltArray mrBoltArray in newMrBoltArrayList)
                        {
                            //判断螺钉组中所有螺钉是否在一条直线上;
                            if (CDimTools.GetInstance().IsObliqueBoltOnLine(mrBoltArray))
                            {
                                //水平方向的标注;
                                xMrDimSet.AddPoint(mrBoltArray.GetMaxYPoint());
                                xMrDimSet.AddPoint(mrBoltArray.GetMinYPoint());
                                
                                //垂直方向的标注;
                                yMrDimSet.AddPoint(mrBoltArray.GetMaxYPoint());
                                yMrDimSet.AddPoint(mrBoltArray.GetMinYPoint());
                            }
                            //否则所有螺钉组不在一条直线上;
                            else
                            {
                                Point firstPt = mrBoltArray.mFirstPoint;
                                Point secondPt = mrBoltArray.mSecondPoint;
                                Point maxYPoint = mrBoltArray.GetMaxYPoint();
                                Vector directionVector = new Vector(secondPt.X - firstPt.X, secondPt.Y - firstPt.Y, secondPt.Z - firstPt.Z);

                                foreach (Point boltPoint in mrBoltArray.GetBoltPointList())
                                {
                                    Vector newVector = new Vector(boltPoint.X - maxYPoint.X, boltPoint.Y - maxYPoint.Y, boltPoint.Z - maxYPoint.Z);

                                    if (CDimTools.GetInstance().IsTwoVectorVertical(directionVector, newVector))
                                    {
                                        xMrDimSet.AddPoint(boltPoint);
                                        yMrDimSet.AddPoint(boltPoint);
                                    }
                                }
                            }
                        }
                        
                        yMrDimSet.AddPoint(new Point(firstBoltArray.GetMinYPoint().X, mainBeamMaxY, 0));

                        Vector dimVector = GetBoltObliqueLineDimVector(firstBoltArray);

                        if (dimVector.X > 0)
                        {
                            yMrDimSet.mDimDistance = mMrPart.GetRightDefaultDimDistance(yMrDimSet.GetDimPointList()[0]);
                        }
                        else if (dimVector.X < 0)
                        {
                            yMrDimSet.mDimDistance = mMrPart.GetLeftDefaultDimDistance(yMrDimSet.GetDimPointList()[0]);
                        }
                        yMrDimSet.mDimVector = dimVector;
                        mrDimSetList.Add(yMrDimSet);
                    }
                }
                xMrDimSet.mDimVector = new Vector(0, 1, 0);
                xMrDimSet.mDimDistance = mMrPart.GetUpDefaultDimDistance(xMrDimSet.GetDimPointList()[0]);
                mrDimSetList.Add(xMrDimSet);

                return mrDimSetList;
            }
            //如果零件在主梁下方;
            else if (firstMrBoltArrayGroup.GetMaxYPoint().Y < 0)
            {
                CMrDimSet xMrDimSet = new CMrDimSet();

                //根据螺钉组合来进行螺钉组的标注;
                foreach (CMrBoltArrayGroup mrBoltArrayGroup in mrBoltArrayGroupList)
                {
                    List<CMrBoltArray> newMrBoltArrayList = mrBoltArrayGroup.mrBoltArrayList;

                    CMrBoltArray firstBoltArray = newMrBoltArrayList[0];

                    //1.如果螺钉是数组排列;
                    if (firstBoltArray.mBoltArrayShapeType == MrBoltArrayShapeType.ARRAY)
                    {
                        CMrDimSet yMrDimSet = new CMrDimSet();

                        foreach (CMrBoltArray mrBoltArray in newMrBoltArrayList)
                        {
                            List<Point> minXPointList = mrBoltArray.GetMinXPointList();
                            List<Point> minYPointList = mrBoltArray.GetMinYPointList();

                            yMrDimSet.AddRange(minXPointList);
                            xMrDimSet.AddRange(minYPointList);
                        }

                        yMrDimSet.AddPoint(new Point(firstBoltArray.GetMinYPoint().X, mainBeamMaxY, 0));
                        yMrDimSet.mDimVector = new Vector(-1, 0, 0);
                        yMrDimSet.mDimDistance = mMrPart.GetLeftDefaultDimDistance(yMrDimSet.GetDimPointList()[0]);

                        mrDimSetList.Add(yMrDimSet);
                    }
                    else if (firstBoltArray.mBoltArrayShapeType == MrBoltArrayShapeType.OBLIQUELINE)
                    {
                        CMrDimSet yMrDimSet = new CMrDimSet();

                        foreach (CMrBoltArray mrBoltArray in newMrBoltArrayList)
                        {
                            //判断螺钉组中所有螺钉是否在一条直线上;
                            if (CDimTools.GetInstance().IsObliqueBoltOnLine(mrBoltArray))
                            {
                                //水平方向的标注;
                                xMrDimSet.AddPoint(mrBoltArray.GetMaxYPoint());
                                xMrDimSet.AddPoint(mrBoltArray.GetMinYPoint());
                              
                                //垂直方向的标注;
                                yMrDimSet.AddPoint(mrBoltArray.GetMaxYPoint());
                                yMrDimSet.AddPoint(mrBoltArray.GetMinYPoint());
                            }
                            //否则所有螺钉组不在一条直线上;
                            else
                            {
                                Point firstPt = mrBoltArray.mFirstPoint;
                                Point secondPt = mrBoltArray.mSecondPoint;
                                Point minYPoint = mrBoltArray.GetMinYPoint();
                                Vector directionVector = new Vector(secondPt.X - firstPt.X, secondPt.Y - firstPt.Y, secondPt.Z - firstPt.Z);

                                foreach (Point boltPoint in mrBoltArray.GetBoltPointList())
                                {
                                    Vector newVector = new Vector(boltPoint.X - minYPoint.X, boltPoint.Y - minYPoint.Y, boltPoint.Z - minYPoint.Z);

                                    if (CDimTools.GetInstance().IsTwoVectorVertical(directionVector, newVector))
                                    {
                                        xMrDimSet.AddPoint(boltPoint);
                                        yMrDimSet.AddPoint(boltPoint);
                                    }
                                }
                            }
                        }
                        yMrDimSet.AddPoint(new Point(firstBoltArray.GetMinYPoint().X, mainBeamMinY, 0));

                        Vector dimVector = GetBoltObliqueLineDimVector(firstBoltArray);

                        if (dimVector.X > 0)
                        {
                            yMrDimSet.mDimDistance = mMrPart.GetRightDefaultDimDistance(yMrDimSet.GetDimPointList()[0]);
                        }
                        else if (dimVector.X < 0)
                        {
                            yMrDimSet.mDimDistance = mMrPart.GetLeftDefaultDimDistance(yMrDimSet.GetDimPointList()[0]);
                        }
                        yMrDimSet.mDimVector = dimVector;
                        mrDimSetList.Add(yMrDimSet);
                    }
                }
                xMrDimSet.mDimVector = new Vector(0, -1, 0);
                xMrDimSet.mDimDistance = mMrPart.GetDownDefaultDimDistance(xMrDimSet.GetDimPointList()[0]);
                mrDimSetList.Add(xMrDimSet);

                return mrDimSetList;
            }
            return null;
        }

        /// <summary>
        /// 获取零件自身的标注;
        /// </summary>
        /// <returns></returns>
        public List<CMrDimSet> GetPartDimSet()
        {
            List<CMrDimSet> mrDimSetList = new List<CMrDimSet>();

            Vector normal = mMrPart.mNormal;
            Vector yVector=new Vector(0,1,0);

            if(!IsPartInMainBeamMiddle(mMrPart))
            {
                return null;
            }
            if(IsPartInMainBeamFlange(mMrPart))
            {
                return null;
            }
            if(!CDimTools.GetInstance().IsTwoVectorParallel(normal,yVector))
            {
                return null;
            }

            double mainBeamHeight = CMrMainBeam.GetInstance().GetYSpaceValue();
            double mainBeamWidth = CMrMainBeam.GetInstance().GetXSpaceValue();

            Point leftTopPoint = mMrPart.mLeftTopPoint;
            Point rightTopPoint = mMrPart.mRightTopPoint;

            //如果是左右的两块夹板则不标注;
            if(leftTopPoint.X < 0 || rightTopPoint.X > mainBeamWidth)
            {
                return null;
            }

            CMrDimSet mrDimSetY = new CMrDimSet();

            mrDimSetY.AddPoint(mMrPart.mRightTopPoint);
            Point basePoint = new Point(mMrPart.mRightTopPoint.X, mainBeamHeight / 2.0);
            mrDimSetY.AddPoint(basePoint);
            mrDimSetY.mDimVector = new Vector(1, 0, 0);
            mrDimSetY.mDimDistance = CCommonPara.mDefaultDimDistance;
            mrDimSetList.Add(mrDimSetY);

            return mrDimSetList;
        }

        /// <summary>
        /// 判断零部件是否在主梁之间;
        /// </summary>
        /// <returns></returns>
        private bool IsPartInMainBeamMiddle(CMrPart mrPart)
        {
            Point leftTopPoint = mrPart.mLeftTopPoint;
            Point leftBottomPoint = mrPart.mLeftBottomPoint;

            double mainBeamHeight = CMrMainBeam.GetInstance().GetYSpaceValue();

            if(leftTopPoint.Y < mainBeamHeight / 2.0 &&
                leftBottomPoint.Y > -mainBeamHeight / 2.0)
            {
                return true;
            }

            return false;
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
            //1.如果零部件的法向与x轴平行则不标记;
            Vector normal = mMrPart.mNormal;
            Vector xVector = new Vector(1, 0, 0);
            Vector yVector = new Vector(0, 1, 0);

            if (CDimTools.GetInstance().IsTwoVectorParallel(normal, xVector)||CDimTools.GetInstance().IsTwoVectorParallel(normal,yVector))
            {
                return null;
            }
            if (CDimTools.GetInstance().IsVectorInXZPlane(normal))
            {
                return null;
            }

            if(IsPartInMainBeamFlange(mMrPart))
            {
                return null;
            }

            CMrMark mrMark = new CMrMark();

            Point maxYPoint = CDimTools.GetInstance().GetMaxYPoint(mMrPart.GetPointList());
            Point minYPoint = CDimTools.GetInstance().GetMinYPoint(mMrPart.GetPointList());
            double mainBeamHeight = CMrMainBeam.GetInstance().GetYSpaceValue();
            double mainBeamWidth = CMrMainBeam.GetInstance().GetXSpaceValue();

            //零件在主梁中间上方;
            if(CDimTools.GetInstance().CompareTwoDoubleValue(maxYPoint.Y,mainBeamHeight/2) <= 0 && minYPoint.Y > CCommonPara.mDblError)
            {
                if(CDimTools.GetInstance().IsTwoVectorParallel(normal,yVector))
                {
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
                }

                mrMark.mModelObject = mMrPart.mPartInDrawing;
                mrMark.mInsertPoint = mMrPart.mMidPoint;
                double increaseXDistance = CCommonPara.mPartMarkLength / Math.Tan(CCommonPara.mPartMarkAngle);
                mrMark.mTextPoint.X = mrMark.mInsertPoint.X + increaseXDistance;
                mrMark.mTextPoint.Y = mrMark.mInsertPoint.Y - CCommonPara.mPartMarkLength;

                return mrMark;
            }
            //零件在主梁的中间下方;
            else if (CDimTools.GetInstance().CompareTwoDoubleValue(minYPoint.Y, -mainBeamHeight / 2) >= 0  && maxYPoint.Y < CCommonPara.mDblError)
            {
                if (CDimTools.GetInstance().IsTwoVectorParallel(normal, yVector))
                {
                    double increaseXDistance = 0.0;

                    //如果是中间左侧的双夹板;
                    if (mMrPart.mLeftTopPoint.X < CCommonPara.mDblError && mMrPart.mRightTopPoint.X > CCommonPara.mDblError)
                    {
                        mrMark.mModelObject = mMrPart.mPartInDrawing;
                        mrMark.mInsertPoint = mMrPart.mRightTopPoint;
                        increaseXDistance = CCommonPara.mPartMarkLength / Math.Tan(CCommonPara.mPartMarkAngle);
                        mrMark.mTextPoint.X = mrMark.mInsertPoint.X + increaseXDistance;
                        mrMark.mTextPoint.Y = mrMark.mInsertPoint.Y - CCommonPara.mPartMarkLength;

                        return mrMark;
                    }
                    //如果是中间右侧的双夹板;
                    if (mMrPart.mLeftTopPoint.X < mainBeamWidth && mMrPart.mRightTopPoint.X > mainBeamWidth)
                    {
                        mrMark.mModelObject = mMrPart.mPartInDrawing;
                        mrMark.mInsertPoint = mMrPart.mRightTopPoint;
                        increaseXDistance = CCommonPara.mPartMarkLength / Math.Tan(CCommonPara.mPartMarkAngle);
                        mrMark.mTextPoint.X = mrMark.mInsertPoint.X + increaseXDistance;
                        mrMark.mTextPoint.Y = mrMark.mInsertPoint.Y - CCommonPara.mPartMarkLength;

                        return mrMark;
                    }

                    mrMark.mModelObject = mMrPart.mPartInDrawing;
                    mrMark.mInsertPoint = mMrPart.mMidPoint;
                    increaseXDistance = CCommonPara.mPartMarkLength / Math.Tan(CCommonPara.mPartMarkAngle);
                    mrMark.mTextPoint.X = mrMark.mInsertPoint.X + increaseXDistance;
                    mrMark.mTextPoint.Y = mrMark.mInsertPoint.Y + CCommonPara.mPartMarkLength;

                    return mrMark;
                }
            }
            //零件在梁的上方;
            else if (CDimTools.GetInstance().CompareTwoDoubleValue(minYPoint.Y, mainBeamHeight / 2) >= 0)
            {
                if (CDimTools.GetInstance().IsTwoVectorParallel(normal, yVector))
                {
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
                }

                mrMark.mModelObject = mMrPart.mPartInDrawing;
                mrMark.mInsertPoint = mMrPart.mMidPoint;
                double increaseXDistance = CCommonPara.mPartMarkLength * Math.Tan(CCommonPara.mPartMarkAngle);
                mrMark.mTextPoint.X = mrMark.mInsertPoint.X + increaseXDistance;
                mrMark.mTextPoint.Y = mrMark.mInsertPoint.Y + CCommonPara.mPartMarkLength;

                return mrMark;
            }
            //零件在梁的下方;
            else if (CDimTools.GetInstance().CompareTwoDoubleValue(maxYPoint.Y, -mainBeamHeight / 2) <= 0)
            {
                double increaseXDistance = 0.0;

                if (CDimTools.GetInstance().IsTwoVectorParallel(normal, yVector))
                {
                    //如果是中间左侧的双夹板;
                    if (mMrPart.mLeftTopPoint.X < CCommonPara.mDblError && mMrPart.mRightTopPoint.X > CCommonPara.mDblError)
                    {
                        mrMark.mModelObject = mMrPart.mPartInDrawing;
                        mrMark.mInsertPoint = mMrPart.mLeftBottomPoint;
                        increaseXDistance = CCommonPara.mPartMarkLength / Math.Tan(CCommonPara.mPartMarkAngle);
                        mrMark.mTextPoint.X = mrMark.mInsertPoint.X - increaseXDistance;
                        mrMark.mTextPoint.Y = mrMark.mInsertPoint.Y - CCommonPara.mPartMarkLength;

                        return mrMark;
                    }
                    //如果是中间右侧的双夹板;
                    if (mMrPart.mLeftTopPoint.X < mainBeamWidth && mMrPart.mRightTopPoint.X > mainBeamWidth)
                    {
                        mrMark.mModelObject = mMrPart.mPartInDrawing;
                        mrMark.mInsertPoint = mMrPart.mLeftBottomPoint;
                        increaseXDistance = CCommonPara.mPartMarkLength / Math.Tan(CCommonPara.mPartMarkAngle);
                        mrMark.mTextPoint.X = mrMark.mInsertPoint.X - increaseXDistance;
                        mrMark.mTextPoint.Y = mrMark.mInsertPoint.Y - CCommonPara.mPartMarkLength;

                        return mrMark;
                    }
                }

                mrMark.mModelObject = mMrPart.mPartInDrawing;
                mrMark.mInsertPoint = mMrPart.mMidPoint;
                increaseXDistance = CCommonPara.mPartMarkLength / Math.Tan(CCommonPara.mPartMarkAngle);
                mrMark.mTextPoint.X = mrMark.mInsertPoint.X + increaseXDistance;
                mrMark.mTextPoint.Y = mrMark.mInsertPoint.Y - CCommonPara.mPartMarkLength;

                return mrMark;
            }
            else
            {

                if (mMrPart.mLeftTopPoint.X < CCommonPara.mDblError && mMrPart.mRightTopPoint.X > CCommonPara.mDblError)
                {
                    return null;
                }
                else if (mMrPart.mLeftTopPoint.X < mainBeamWidth && mMrPart.mRightTopPoint.X > mainBeamWidth)
                {
                    return null;
                }
                else
                {
                    mrMark.mModelObject = mMrPart.mPartInDrawing;
                    mrMark.mInsertPoint = mMrPart.mRightTopPoint;
                    double increaseXDistance = CCommonPara.mPartMarkLength / Math.Tan(CCommonPara.mPartMarkAngle);
                    mrMark.mTextPoint.X = mrMark.mInsertPoint.X + increaseXDistance;
                    mrMark.mTextPoint.Y = mrMark.mInsertPoint.Y + CCommonPara.mPartMarkLength;

                    return mrMark;
                }
            }
            
            return null;
        }

        /// <summary>
        /// 判断零件是否在主梁翼缘的两侧,并且不高度不超过主梁翼缘的高度;
        /// </summary>
        /// <param name="mrPart"></param>
        /// <returns></returns>
        private bool IsPartInMainBeamFlange(CMrPart mrPart)
        {
            Vector normal=mrPart.mNormal;
            
            if(!CDimTools.GetInstance().IsTwoVectorParallel(normal,new Vector(0,1,0)))
            {
                return false;
            }

            double dblMinY=mrPart.GetMinYPoint().Y;
            double dblMaxY=mrPart.GetMaxYPoint().Y;

            double dblHalfHeight = CMrMainBeam.GetInstance().GetYSpaceValue() / 2.0;
            double dblBeamFlange = CMrMainBeam.GetInstance().mFlangeThickness;

            if (Math.Abs(dblMinY - (dblHalfHeight - dblBeamFlange)) <= 5 && Math.Abs(dblMaxY - dblHalfHeight) <= 5)
            {
                return true;
            }
            if (Math.Abs(dblMinY + dblHalfHeight) <= 5 && Math.Abs(dblMaxY + (dblHalfHeight - dblBeamFlange)) <= 5)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 判断零件是否是挡板;挡板是竖直的在梁的两头;
        /// 0：不是，1：左边挡板。2：右边挡板。
        /// </summary>
        /// <param name="mrPart"></param>
        /// <returns></returns>
        private int IsPartTheBackPlate(CMrPart mrPart)
        {
            Vector normal = mrPart.mNormal;

            if (!CDimTools.GetInstance().IsTwoVectorParallel(normal, new Vector(1, 0, 0)))
            {
                return 0;
            }
            Point minXPoint = mrPart.GetMinXPoint();
            Point maxXPoint = mrPart.GetMaxXPoint();

            Point mainBeamMinXPoint = CMrMainBeam.GetInstance().GetMinXPoint();
            Point mainBeamMaxXPoint = CMrMainBeam.GetInstance().GetMaxXPoint();

            if (CDimTools.GetInstance().CompareTwoDoubleValue(maxXPoint.X, mainBeamMinXPoint.X) == 0)
            {
                return 1;
            }
            else if (CDimTools.GetInstance().CompareTwoDoubleValue(minXPoint.X, mainBeamMaxXPoint.X) == 0)
            {
                return 2;
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// 判断零件是否是竖直的耳板;
        /// </summary>
        /// <param name="mrPart"></param>
        /// <returns></returns>
        private bool IsPartTheEarPlate(CMrPart mrPart)
        {
            Vector normal = mrPart.mNormal;

            if (!CDimTools.GetInstance().IsTwoVectorParallel(normal, new Vector(0, 0, 1)))
            {
                return false;
            }

            Point minXPoint = mrPart.GetMinXPoint();
            Point maxXPoint = mrPart.GetMaxXPoint();

            Point mainBeamMinXPoint = CMrMainBeam.GetInstance().GetMinXPoint();
            Point mainBeamMaxXPoint = CMrMainBeam.GetInstance().GetMaxXPoint();

            if (CDimTools.GetInstance().CompareTwoDoubleValue(minXPoint.X, mainBeamMinXPoint.X) == 0)
            {
                return true;
            }
            else if (CDimTools.GetInstance().CompareTwoDoubleValue(maxXPoint.X, mainBeamMaxXPoint.X) == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 判断零件是否是垫块;
        /// </summary>
        /// <param name="mrPart"></param>
        /// <returns></returns>
        private bool IsPartTheBlockPlate(CMrPart mrPart)
        {
            Vector normal = mMrPart.mNormal;
            Vector yVector = new Vector(0, 1, 0);

            if (!mMrPart.IsHaveBolt() 
                && mMrPart.GetYSpaceValue() <= 8 
                && mMrPart.GetXSpaceValue() <= 30
                && mMrPart.GetZSpaceValue() >= CMrMainBeam.GetInstance().GetZSpaceValue()
                && CDimTools.GetInstance().IsTwoVectorParallel(normal, yVector))
            {
                return true;
            }

            return false;
        }
    }
}
