using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tekla.Structures.Geometry3d;

namespace AutoDimension.Entity
{
    /// <summary>
    /// 记录柱在前视图中的信息;
    /// </summary>
    public class CCylinderFrontViewInfo
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
        /// 垂直连接板或加筋板的对称零部件,主要是指主部件中间的连接板是否存在外围连接板与他对称;
        /// </summary>
        public CMrPart mSymPart { get; set; }

        /// <summary>
        /// 部件前视图信息;
        /// </summary>
        /// <param name="mrPart"></param>
        public CCylinderFrontViewInfo(CMrPart mrPart)
        {
            this.mMrPart = mrPart;
            mSymPart = null;
            this.mPostionType = MrPositionType.NONE;
        }

        /// <summary>
        /// 初始化该零件的信息,主要初始化零件的位置信息和斜率;
        /// </summary>
        public void InitCylinderFrontViewInfo()
        {
            Point maxXPoint = mMrPart.GetMaxXPoint();
            Point minXPoint = mMrPart.GetMinXPoint();

            Point mainBeamMaxXPoint = CMrMainBeam.GetInstance().GetMaxXPoint();
            Point mainBeamMinXPoint = CMrMainBeam.GetInstance().GetMinXPoint();

            if (CDimTools.GetInstance().CompareTwoDoubleValue(minXPoint.X, mainBeamMaxXPoint.X) >= 0)
            {
                mPostionType = MrPositionType.RIGHT;
            }
            else if (CDimTools.GetInstance().CompareTwoDoubleValue(maxXPoint.X, mainBeamMinXPoint.X) <= 0)
            {
                mPostionType = MrPositionType.LEFT;
            }
            else 
            {
                mPostionType = MrPositionType.MIDDLE;
            }
        }

        /// <summary>
        /// 获取前视图零件左侧的主标注集合;
        /// </summary>
        /// <returns></returns>
        public CMrDimSet GetPartLeftDimSet()
        {
            CMrDimSet mrDimSet = new CMrDimSet();

            Vector xVector = new Vector(1, 0, 0);
            Vector yVector = new Vector(0, 1, 0);
            Vector zVector = new Vector(0, 0, 1);

            double x1 = mMrPart.GetMinXPoint().X;
            double x2 = CMrMainBeam.GetInstance().GetMaxXPoint().X;

            //1.如果零件在主零件的右侧，则不标注;
            if (mPostionType==MrPositionType.RIGHT)
            {
                return null;
            }
            //2.判断角钢是否需要标注;
            if (CDimTools.GetInstance().IsPartTheAngleSteel(mMrPart))
            {
                return GetAngleSheetLeftDim(mMrPart);
            }
            //3.如果是外围连接板则不标注;
            if (IsOutsidePlate(mMrPart))
            {
                return null;
            }
            //4.过滤掉垫块;
            if (IsHeelBlock(mMrPart))
            {
                return null;
            }
            //5.如果向量与X轴平行则不标注,但是需要判断零件标记，主要是针对牛腿上的支撑板的法向可能与X轴平行;
            Vector normal = mMrPart.mNormal;

            if (CDimTools.GetInstance().IsTwoVectorParallel(normal, xVector))
            {
                if (mPostionType==MrPositionType.LEFT)
                {
                    CCylinderFrontViewSetting cylinderTopViewSetting = CCylinderDimSetting.GetInstance().mFrontViewSetting;
                    bool bMarkValue = cylinderTopViewSetting.FindMarkValueByName(CCylinderFrontViewSetting.mstrSupportPlate);
                    mMrPart.SetNeedAddMarkFlag(bMarkValue);
                }
                return null;
            }
            //6.如果向量与Y轴平行，标注垂直连接板及加筋板，垂直连接板属于在主部件内部的零部件,也会存在外围连接板;
            if (CDimTools.GetInstance().IsTwoVectorParallel(normal, yVector))
            {
                return GetVerticalConnectPlateLeftDim(mMrPart);
            }
            //7.如果向量与Z轴平行，标准水平连接板或者是支撑及牛腿;
            if (CDimTools.GetInstance().IsTwoVectorParallel(normal, zVector))
            {
                return GetHorizontalConnentPlateLeftDim(mMrPart);
            }
            //8.如果向量在XY平面内，如斜板，牛腿板，先不标注，但是需要标注零件标记;
            if (CDimTools.GetInstance().IsVectorInXYPlane(normal))
            {
                return GetXYPlanePlateLeftDim(mMrPart);
            }
            return null;
        }

        /// <summary>
        /// 获得角钢的标注,左边标注处理左侧角钢和中间的角钢;
        /// </summary>
        /// <param name="?"></param>
        /// <returns></returns>
        private CMrDimSet GetAngleSheetLeftDim(CMrPart mMrPart)
        {
            CMrDimSet mrDimSet = new CMrDimSet();

            //前视图标注配置类;
            CCylinderFrontViewSetting cylinderFrontViewSetting = CCylinderDimSetting.GetInstance().mFrontViewSetting;

            bool bMarkValue = cylinderFrontViewSetting.FindMarkValueByName(CCylinderFrontViewSetting.mstrAngleSheet);
            mMrPart.SetNeedAddMarkFlag(bMarkValue);

            bool bValue = cylinderFrontViewSetting.FindDimValueByName(CCylinderFrontViewSetting.mstrAngleSheet);
            if (!bValue)
            {
                return null;
            }

            bool bNeedDim = true;

            if (mMrPart.IsHaveBolt())
            {
                List<CMrBoltArray> mrBoltArrayList = mMrPart.GetBoltArrayList();

                foreach (CMrBoltArray mrBoltArray in mrBoltArrayList)
                {
                    if (CDimTools.GetInstance().IsTwoVectorParallel(mrBoltArray.mNormal, new Vector(0, 0, 1)))
                    {
                        mrDimSet.AddRange(mrBoltArray.GetMinXPointList());
                        bNeedDim = false;
                    }
                }
            }
            if (!bNeedDim)
            {
                return mrDimSet;
            }
            //如果角钢在主梁的左侧;
            if (mPostionType==MrPositionType.LEFT)
            {
                double minX = mMrPart.GetMinXPoint().X;
                double maxYminX = mMrPart.GetMaxYMinXPoint().X;
                double minYminX = mMrPart.GetMinYMinXPoint().X;

                if (CDimTools.GetInstance().CompareTwoDoubleValue(minX, maxYminX) == 0)
                {
                    mrDimSet.AddPoint(mMrPart.GetMaxYMinXPoint());
                    return mrDimSet;
                }
                else if (CDimTools.GetInstance().CompareTwoDoubleValue(minX, minYminX) == 0)
                {
                    mrDimSet.AddPoint(mMrPart.GetMinYMinXPoint());
                    return mrDimSet;
                }
                else
                {
                    mrDimSet.AddPoint(mMrPart.GetMaxYMinXPoint());
                    return mrDimSet;
                }
            }
            else if (mPostionType==MrPositionType.MIDDLE)
            {
                double maxY = mMrPart.GetMaxYPoint().Y;
                double minY = mMrPart.GetMinYPoint().Y;
                double maxZmaxY = mMrPart.GetMaxZMaxYPoint().Y;
                double minZmaxY = mMrPart.GetMinZMaxYPoint().Y;
                double maxZminY = mMrPart.GetMaxZMinYPoint().Y;
                double minZminY = mMrPart.GetMinZMinYPoint().Y;

                double maxZ = mMrPart.GetMaxZPoint().Z;
                double minZ = mMrPart.GetMinZPoint().Z;

                if (minZ > 0)
                {
                    if (CDimTools.GetInstance().CompareTwoDoubleValue(maxY, maxZmaxY) == 0)
                    {
                        mrDimSet.AddPoint(mMrPart.GetMaxYMinXPoint());
                        return mrDimSet;
                    }
                    else if (CDimTools.GetInstance().CompareTwoDoubleValue(minY, maxZminY) == 0)
                    {
                        mrDimSet.AddPoint(mMrPart.GetMinYMinXPoint());
                        return mrDimSet;
                    }
                    else
                    {
                        mrDimSet.AddPoint(mMrPart.GetMaxYMinXPoint());
                        return mrDimSet;
                    }
                }
                else if (maxZ < 0)
                {
                    if (CDimTools.GetInstance().CompareTwoDoubleValue(maxY, minZmaxY) == 0)
                    {
                        mrDimSet.AddPoint(mMrPart.GetMaxYMinXPoint());
                        return mrDimSet;
                    }
                    else if (CDimTools.GetInstance().CompareTwoDoubleValue(minY, minZminY) == 0)
                    {
                        mrDimSet.AddPoint(mMrPart.GetMinYMinXPoint());
                        return mrDimSet;
                    }
                    else
                    {
                        mrDimSet.AddPoint(mMrPart.GetMaxYMinXPoint());
                        return mrDimSet;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// 获得垂直连接板及加筋板左侧的标注集;
        /// </summary>
        /// <param name="?"></param>
        /// <returns></returns>
        private CMrDimSet GetVerticalConnectPlateLeftDim(CMrPart mMrPart)
        {
            CMrDimSet mrDimSet = new CMrDimSet();

            //1.前视图标注配置类;
            CCylinderFrontViewSetting cylinderFrontViewSetting = CCylinderDimSetting.GetInstance().mFrontViewSetting;

            //2.如果该连接板在主梁的左侧则为外围连接板;
            if (mPostionType == MrPositionType.LEFT)
            {
                //如果是斜支撑的外围连接板;
                if (mMrPart.mBeamType == MrBeamType.POLYBEAM)
                {
                    return null;
                }

                bool bMarkValue = cylinderFrontViewSetting.FindMarkValueByName(CCylinderFrontViewSetting.mstrOutConnectPlate);
                mMrPart.SetNeedAddMarkFlag(bMarkValue);

                bool bValue = cylinderFrontViewSetting.FindDimValueByName(CCylinderFrontViewSetting.mstrOutConnectPlate);

                if (!bValue)
                {
                    return null;
                }
                //首先判断垂直连接板是否属于剪切板中的一块板,如果是则进行标注;
                MrClipPlatePosType mrClipPlatePosType = MrClipPlatePosType.NONE;

                //需要先判断零件是否在剪切板中;
                if (CMrClipPlateManager.GetInstance().IsInClipPlate(mMrPart, out mrClipPlatePosType))
                {
                    //只标注在剪切板中上方的垂直连接板;
                    if (mrClipPlatePosType == MrClipPlatePosType.TOP)
                    {
                        mrDimSet.AddPoint(mMrPart.GetMaxYMinXPoint());
                        return mrDimSet;
                    }
                    if (mrClipPlatePosType == MrClipPlatePosType.BOTTOM)
                    {
                        return null;
                    }
                }
                else //如果不是剪切板中的一块板;
                {
                    mrDimSet.AddPoint(mMrPart.GetMinYMinXPoint());
                    return mrDimSet;
                }
            }
            //3.如果该连接板在主梁的中间则为中间的垂直连接板及加筋板;
            else if(mPostionType==MrPositionType.MIDDLE)
            {
                bool bMarkValue = cylinderFrontViewSetting.FindMarkValueByName(CCylinderFrontViewSetting.mstrVerticalConnectPlate);
                mMrPart.SetNeedAddMarkFlag(bMarkValue);
                bool bValue = cylinderFrontViewSetting.FindDimValueByName(CCylinderFrontViewSetting.mstrVerticalConnectPlate);

                if (!bValue)
                {
                    return null;
                }
                //如果是H型钢则返回;
                if (CDimTools.GetInstance().IsPartTheHProfileSheet(mMrPart))
                {
                    return null;
                }
                //如果不存在外围的连接板与该板在同一高度，则需要进行标注;
                if (mSymPart == null)
                {
                    mrDimSet.AddPoint(mMrPart.GetMinYMinXPoint());
                    return mrDimSet;
                }
            }
            return null;
        }

        /// <summary>
        /// 获得水平连接板的左侧标注集;
        /// </summary>
        /// <param name="?"></param>
        /// <returns></returns>
        private CMrDimSet GetHorizontalConnentPlateLeftDim(CMrPart mrPart)
        {
            //1.前视图标注配置类;
            CCylinderFrontViewSetting cylinderFrontViewSetting = CCylinderDimSetting.GetInstance().mFrontViewSetting;

            if (mPostionType==MrPositionType.MIDDLE)
            {
                return null;
            }

            CMrDimSet mrDimSet = new CMrDimSet();

            //2.如果没有螺钉则无孔连接板，需要判断是否是无孔支撑板或者是无孔连接板，根据配置来决定是否标注;
            if (!mMrPart.IsHaveBolt())
            {
                //如果是板类零件;
                if (CDimTools.GetInstance().IsPartThePlate(mrPart))
                {
                    bool bValue = cylinderFrontViewSetting.FindDimValueByName(CCylinderFrontViewSetting.mstrHorizontalConnentPlate);
                    bool bMarkValue = cylinderFrontViewSetting.FindMarkValueByName(CCylinderFrontViewSetting.mstrHorizontalConnentPlate);
                    mMrPart.SetNeedAddMarkFlag(bMarkValue);

                    if (!bValue)
                    {
                        return null;
                    }
                    //需要先判断连接板是否在剪切板中,如果在则默认标注剪切板的最上面的板;
                    MrClipPlatePosType mrClipPlatePosType = MrClipPlatePosType.NONE;

                    if (CMrClipPlateManager.GetInstance().IsInClipPlate(mMrPart, out mrClipPlatePosType))
                    {
                        CMrClipPlate mrClipPlate = CMrClipPlateManager.GetInstance().GetMrClipPlate(mMrPart);
                        mrDimSet.AddPoint(mrClipPlate.mMrTopPart.GetMaxYMinXPoint());
                        return mrDimSet;
                    }
                    else
                    {
                        mrDimSet.AddPoint(mMrPart.GetMinYMinXPoint());
                        return mrDimSet;
                    }
                }
                //如果是型钢类零部件;
                else
                {
                    bool bValue = cylinderFrontViewSetting.FindDimValueByName(CCylinderFrontViewSetting.mstrSupportPlate);
                    bool bMarkValue = cylinderFrontViewSetting.FindMarkValueByName(CCylinderFrontViewSetting.mstrSupportPlate);
                    mMrPart.SetNeedAddMarkFlag(bMarkValue);

                    if (!bValue)
                    {
                        return null;
                    }

                    //需要先判断连接板是否在剪切板中,如果在则默认标注剪切板的最上面的板;
                    MrClipPlatePosType mrClipPlatePosType = MrClipPlatePosType.NONE;

                    if (CMrClipPlateManager.GetInstance().IsInClipPlate(mMrPart, out mrClipPlatePosType))
                    {
                        CMrClipPlate mrClipPlate = CMrClipPlateManager.GetInstance().GetMrClipPlate(mMrPart);
                        mrDimSet.AddPoint(mrClipPlate.mMrTopPart.GetMaxYMinXPoint());
                        return mrDimSet;
                    }
                    else
                    {
                        mrDimSet.AddPoint(mMrPart.GetMinYMinXPoint());
                        return mrDimSet;
                    }
                }
            }
            //否则是有孔的支撑板;
            else
            {
                bool bValue = cylinderFrontViewSetting.FindDimValueByName(CCylinderFrontViewSetting.mstrSupportPlate);
                bool bMarkValue = cylinderFrontViewSetting.FindMarkValueByName(CCylinderFrontViewSetting.mstrSupportPlate);
                mMrPart.SetNeedAddMarkFlag(bMarkValue);

                if (!bValue)
                {
                    return null;
                }

                //需要先判断连接板是否在剪切板中,如果在则默认标注剪切板的最上面的板;
                MrClipPlatePosType mrClipPlatePosType = MrClipPlatePosType.NONE;

                if (CMrClipPlateManager.GetInstance().IsInClipPlate(mMrPart, out mrClipPlatePosType))
                {
                    CMrClipPlate mrClipPlate = CMrClipPlateManager.GetInstance().GetMrClipPlate(mMrPart);
                    mrDimSet.AddPoint(mrClipPlate.mMrTopPart.GetMaxYMinXPoint());
                    return mrDimSet;
                }
                else
                {
                    return GetSupportPlateBoltLeftDimSet();
                }
            }
        }

        /// <summary>
        /// 获得支撑及牛腿以及水平连接板左侧的螺钉组的标注;
        /// </summary>
        /// <returns></returns>
        private CMrDimSet GetSupportPlateBoltLeftDimSet()
        {
            if (!mMrPart.IsHaveBolt())
            {
                return null;
            }

            CMrDimSet mrDimSet = new CMrDimSet();
            Vector zVector = new Vector(0, 0, 1);

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
                mrDimSet.AddPoint(mrBoltArray.GetMaxYMinXPoint());
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
        /// 获得XY平面内的板的标注,有待后续处理，此处只处理零件标记的显示,当着连接板来处理;
        /// </summary>
        /// <param name="mrPart"></param>
        /// <returns></returns>
        private CMrDimSet GetXYPlanePlateLeftDim(CMrPart mrPart)
        {
            //1.如果是主部件外围的斜牛腿及支撑板，则不标注;
            if(mPostionType == MrPositionType.LEFT)
            {
                CCylinderFrontViewSetting cylinderFrontViewSetting = CCylinderDimSetting.GetInstance().mFrontViewSetting;
                bool bMarkValue = cylinderFrontViewSetting.FindMarkValueByName(CCylinderFrontViewSetting.mstrSupportPlate);
                mMrPart.SetNeedAddMarkFlag(bMarkValue);

                return null;
            }
            //2.如果是中间的斜板,则需要进行标注;
            if(mPostionType == MrPositionType.MIDDLE)
            {
                CMrDimSet mrDimSet = new CMrDimSet();

                CCylinderFrontViewSetting cylinderFrontViewSetting = CCylinderDimSetting.GetInstance().mFrontViewSetting;
                bool bMarkValue = cylinderFrontViewSetting.FindMarkValueByName(CCylinderFrontViewSetting.mstrVerticalConnectPlate);
                mMrPart.SetNeedAddMarkFlag(bMarkValue);

                bool bValue = cylinderFrontViewSetting.FindDimValueByName(CCylinderFrontViewSetting.mstrVerticalConnectPlate);

                if (!bValue)
                {
                    return null;
                }

                mrDimSet.AddPoint(mMrPart.GetMinXPoint());
                return mrDimSet;
            }
            return null;
        }

        /// <summary>
        /// 获取前视图零件右侧的主标注集合;
        /// </summary>
        /// <returns></returns>
        public CMrDimSet GetPartRightDimSet()
        {
            CMrDimSet mrDimSet = new CMrDimSet();

            Vector xVector = new Vector(1, 0, 0);
            Vector yVector = new Vector(0, 1, 0);
            Vector zVector = new Vector(0, 0, 1);

            //1.如果零件在主零件的左侧或则中间，则不标注;
            if (mPostionType == MrPositionType.LEFT || mPostionType == MrPositionType.MIDDLE)
            {
                return null;
            }
            //2.判断角钢是否需要标注;
            if (CDimTools.GetInstance().IsPartTheAngleSteel(mMrPart))
            {
                return GetAngleSheetRightDim(mMrPart);
            }
            //3.如果是外围连接板则不标注;
            if (IsOutsidePlate(mMrPart))
            {
                return null;
            }
            //4.过滤掉垫块;
            if (IsHeelBlock(mMrPart))
            {
                return null;
            }
            //5.如果向量与X轴平行则不标注,但是需要判断零件标记，主要是针对牛腿上的支撑板的法向可能与X轴平行;
            Vector normal = mMrPart.mNormal;

            if (CDimTools.GetInstance().IsTwoVectorParallel(normal, xVector))
            {
                if (mPostionType == MrPositionType.RIGHT)
                {
                    CCylinderFrontViewSetting cylinderTopViewSetting = CCylinderDimSetting.GetInstance().mFrontViewSetting;
                    bool bMarkValue = cylinderTopViewSetting.FindMarkValueByName(CCylinderFrontViewSetting.mstrSupportPlate);
                    mMrPart.SetNeedAddMarkFlag(bMarkValue);
                }
                return null;
            }
            //6.如果向量与Y轴平行，标注垂直连接板及加筋板，垂直连接板属于在主部件内部的零部件,也会存在外围连接板;
            if (CDimTools.GetInstance().IsTwoVectorParallel(normal, yVector))
            {
                return GetVerticalConnectPlateRightDim(mMrPart);
            }
            //7.如果向量与Z轴平行，标准水平连接板或者是支撑及牛腿;
            if (CDimTools.GetInstance().IsTwoVectorParallel(normal, zVector))
            {
                return GetHorizontalConnentPlateRightDim(mMrPart);
            }
            //8.如果向量在XY平面内，如斜板，牛腿板，先不标注，但是需要标注零件标记;
            if (CDimTools.GetInstance().IsVectorInXYPlane(normal))
            {
                return GetXYPlanePlateRightDim(mMrPart);
            }
            return null;
        }

        /// <summary>
        /// 获得右边角钢的标注;
        /// </summary>
        /// <param name="?"></param>
        /// <returns></returns>
        private CMrDimSet GetAngleSheetRightDim(CMrPart mMrPart)
        {
            CMrDimSet mrDimSet = new CMrDimSet();

            //前视图标注配置类;
            CCylinderFrontViewSetting cylinderFrontViewSetting = CCylinderDimSetting.GetInstance().mFrontViewSetting;

            bool bMarkValue = cylinderFrontViewSetting.FindMarkValueByName(CCylinderFrontViewSetting.mstrAngleSheet);
            mMrPart.SetNeedAddMarkFlag(bMarkValue);

            bool bValue = cylinderFrontViewSetting.FindDimValueByName(CCylinderFrontViewSetting.mstrAngleSheet);
            if (!bValue)
            {
                return null;
            }

            bool bNeedDim = true;

            if (mMrPart.IsHaveBolt())
            {
                List<CMrBoltArray> mrBoltArrayList = mMrPart.GetBoltArrayList();

                foreach (CMrBoltArray mrBoltArray in mrBoltArrayList)
                {
                    if (CDimTools.GetInstance().IsTwoVectorParallel(mrBoltArray.mNormal, new Vector(0, 0, 1)))
                    {
                        mrDimSet.AddRange(mrBoltArray.GetMinXPointList());
                        bNeedDim = false;
                    }
                }
            }
            if (!bNeedDim)
            {
                return mrDimSet;
            }
            //标注主梁右侧的角钢;
            if (mPostionType == MrPositionType.RIGHT)
            {
                double maxX = mMrPart.GetMaxXPoint().X;
                double maxYmaxX = mMrPart.GetMaxYMaxXPoint().X;
                double minYmaxX = mMrPart.GetMinYMaxXPoint().X;

                if (CDimTools.GetInstance().CompareTwoDoubleValue(maxX, maxYmaxX) == 0)
                {
                    mrDimSet.AddPoint(mMrPart.GetMaxYMaxXPoint());
                    return mrDimSet;
                }
                else if (CDimTools.GetInstance().CompareTwoDoubleValue(maxX, minYmaxX) == 0)
                {
                    mrDimSet.AddPoint(mMrPart.GetMinYMaxXPoint());
                    return mrDimSet;
                }
                else
                {
                    mrDimSet.AddPoint(mMrPart.GetMaxYMaxXPoint());
                    return mrDimSet;
                }
            }
            return null;
        }

        /// <summary>
        /// 获得垂直连接板及加筋板右侧的标注集;
        /// </summary>
        /// <param name="?"></param>
        /// <returns></returns>
        private CMrDimSet GetVerticalConnectPlateRightDim(CMrPart mMrPart)
        {
            CMrDimSet mrDimSet = new CMrDimSet();

            //1.前视图标注配置类;
            CCylinderFrontViewSetting cylinderFrontViewSetting = CCylinderDimSetting.GetInstance().mFrontViewSetting;

            //2.如果该连接板在主梁的右侧则为外围连接板;
            if (mPostionType == MrPositionType.RIGHT)
            {
                //如果是斜支撑的外围连接板;
                if (mMrPart.mBeamType == MrBeamType.POLYBEAM)
                {
                    return null;
                }

                bool bMarkValue = cylinderFrontViewSetting.FindMarkValueByName(CCylinderFrontViewSetting.mstrOutConnectPlate);
                mMrPart.SetNeedAddMarkFlag(bMarkValue);

                bool bValue = cylinderFrontViewSetting.FindDimValueByName(CCylinderFrontViewSetting.mstrOutConnectPlate);

                if (!bValue)
                {
                    return null;
                }

                //首先判断垂直连接板是否属于剪切板中的一块板,如果是则进行标注;
                MrClipPlatePosType mrClipPlatePosType = MrClipPlatePosType.NONE;

                //需要先判断零件是否在剪切板中;
                if (CMrClipPlateManager.GetInstance().IsInClipPlate(mMrPart, out mrClipPlatePosType))
                {
                    //只标注在剪切板中上方的垂直连接板;
                    if (mrClipPlatePosType == MrClipPlatePosType.TOP)
                    {
                        mrDimSet.AddPoint(mMrPart.GetMaxYMaxXPoint());
                        return mrDimSet;
                    }
                    if (mrClipPlatePosType == MrClipPlatePosType.BOTTOM)
                    {
                        return null;
                    }
                }
                else //如果不是剪切板中的一块板;
                {
                    mrDimSet.AddPoint(mMrPart.GetMinYMaxXPoint());
                    return mrDimSet;
                }
            }
            return null;
        }

        /// <summary>
        /// 获得水平连接板的右侧标注集;
        /// </summary>
        /// <param name="?"></param>
        /// <returns></returns>
        private CMrDimSet GetHorizontalConnentPlateRightDim(CMrPart mrPart)
        {
            //1.前视图标注配置类;
            CCylinderFrontViewSetting cylinderFrontViewSetting = CCylinderDimSetting.GetInstance().mFrontViewSetting;

            if (mPostionType == MrPositionType.MIDDLE)
            {
                return null;
            }

            CMrDimSet mrDimSet = new CMrDimSet();

            //2.如果没有螺钉则无孔连接板，需要判断是否是无孔支撑板或者是无孔连接板，根据配置来决定是否标注;
            if (!mMrPart.IsHaveBolt())
            {
                //如果是板类零件;
                if (CDimTools.GetInstance().IsPartThePlate(mrPart))
                {
                    bool bValue = cylinderFrontViewSetting.FindDimValueByName(CCylinderFrontViewSetting.mstrHorizontalConnentPlate);
                    bool bMarkValue = cylinderFrontViewSetting.FindMarkValueByName(CCylinderFrontViewSetting.mstrHorizontalConnentPlate);
                    mMrPart.SetNeedAddMarkFlag(bMarkValue);

                    if (!bValue)
                    {
                        return null;
                    }
                    //需要先判断连接板是否在剪切板中,如果在则默认标注剪切板的最上面的板;
                    MrClipPlatePosType mrClipPlatePosType = MrClipPlatePosType.NONE;

                    if (CMrClipPlateManager.GetInstance().IsInClipPlate(mMrPart, out mrClipPlatePosType))
                    {
                        CMrClipPlate mrClipPlate = CMrClipPlateManager.GetInstance().GetMrClipPlate(mMrPart);
                        mrDimSet.AddPoint(mrClipPlate.mMrTopPart.GetMaxYMaxXPoint());
                        return mrDimSet;
                    }
                    else
                    {
                        mrDimSet.AddPoint(mMrPart.GetMinYMaxXPoint());
                        return mrDimSet;
                    }
                }
                //如果是型钢类零部件;
                else
                {
                    bool bValue = cylinderFrontViewSetting.FindDimValueByName(CCylinderFrontViewSetting.mstrSupportPlate);
                    bool bMarkValue = cylinderFrontViewSetting.FindMarkValueByName(CCylinderFrontViewSetting.mstrSupportPlate);
                    mMrPart.SetNeedAddMarkFlag(bMarkValue);

                    if (!bValue)
                    {
                        return null;
                    }

                    //需要先判断连接板是否在剪切板中,如果在则默认标注剪切板的最上面的板;
                    MrClipPlatePosType mrClipPlatePosType = MrClipPlatePosType.NONE;

                    if (CMrClipPlateManager.GetInstance().IsInClipPlate(mMrPart, out mrClipPlatePosType))
                    {
                        CMrClipPlate mrClipPlate = CMrClipPlateManager.GetInstance().GetMrClipPlate(mMrPart);
                        mrDimSet.AddPoint(mrClipPlate.mMrTopPart.GetMaxYMaxXPoint());
                        return mrDimSet;
                    }
                    else
                    {
                        mrDimSet.AddPoint(mMrPart.GetMinYMaxXPoint());
                        return mrDimSet;
                    }
                }
            }
            //否则是有孔的支撑板;
            else
            {
                bool bValue = cylinderFrontViewSetting.FindDimValueByName(CCylinderFrontViewSetting.mstrSupportPlate);
                bool bMarkValue = cylinderFrontViewSetting.FindMarkValueByName(CCylinderFrontViewSetting.mstrSupportPlate);
                mMrPart.SetNeedAddMarkFlag(bMarkValue);

                if (!bValue)
                {
                    return null;
                }

                //需要先判断连接板是否在剪切板中,如果在则默认标注剪切板的最上面的板;
                MrClipPlatePosType mrClipPlatePosType = MrClipPlatePosType.NONE;

                if (CMrClipPlateManager.GetInstance().IsInClipPlate(mMrPart, out mrClipPlatePosType))
                {
                    CMrClipPlate mrClipPlate = CMrClipPlateManager.GetInstance().GetMrClipPlate(mMrPart);
                    mrDimSet.AddPoint(mrClipPlate.mMrTopPart.GetMaxYMaxXPoint());
                    return mrDimSet;
                }
                else
                {
                    return GetSupportPlateBoltRightDimSet();
                }
            }
        }

        /// <summary>
        /// 获得支撑及牛腿以及水平连接板左侧的螺钉组的标注;
        /// </summary>
        /// <returns></returns>
        private CMrDimSet GetSupportPlateBoltRightDimSet()
        {
            if (!mMrPart.IsHaveBolt())
            {
                return null;
            }

            CMrDimSet mrDimSet = new CMrDimSet();
            Vector zVector = new Vector(0, 0, 1);

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
                mrDimSet.AddPoint(mrBoltArray.GetMaxYMaxXPoint());
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
                    CDimTools.GetInstance().GetMaxXPoint(boltDimPointList);
                    mrDimSet.AddPoint(boltDimPointList[0]);
                }
            }
            return mrDimSet;
        }

        /// <summary>
        /// 获得XY平面内的板的标注,有待后续处理，此处只处理零件标记的显示,当着连接板来处理;
        /// </summary>
        /// <param name="mrPart"></param>
        /// <returns></returns>
        private CMrDimSet GetXYPlanePlateRightDim(CMrPart mrPart)
        {
            //1.如果是主部件右侧外围的斜牛腿及支撑板，则不标注;
            CCylinderFrontViewSetting cylinderFrontViewSetting = CCylinderDimSetting.GetInstance().mFrontViewSetting;
            bool bMarkValue = cylinderFrontViewSetting.FindMarkValueByName(CCylinderFrontViewSetting.mstrSupportPlate);
            mMrPart.SetNeedAddMarkFlag(bMarkValue);
            return null;
        }

        /// <summary>
        /// 获得支撑板的螺钉在Y方向上的标注;
        /// </summary>
        /// <returns></returns>
        public CMrDimSet GetSupportPlateBoltYDimSet()
        {
            CMrDimSet mrDimSet = new CMrDimSet();

            Vector normal = mMrPart.mNormal;

            if (!mMrPart.IsHaveBolt() || !CDimTools.GetInstance().IsTwoVectorParallel(normal, new Vector(0, 0, 1)) || IsOutsidePlate(mMrPart))
            {
                return null;
            }
            if (CDimTools.GetInstance().IsPartTheAngleSteel(mMrPart))
            {
                return null;
            }

            //如果是柱子上下端的双夹板;
            if (mMrPart.GetMinYPoint().Y < 0 && mMrPart.GetMaxYPoint().Y > 0)
            {
                return null;
            }
            if (mMrPart.GetMinYPoint().Y < CMrMainBeam.GetInstance().GetMaxYPoint().Y &&
                mMrPart.GetMaxYPoint().Y > CMrMainBeam.GetInstance().GetMaxYPoint().Y)
            {
                return null;
            }
            List<CMrBoltArray> mrBoltArrayList = mMrPart.GetBoltArrayList();

            //如果零件在主梁的左侧;
            if (mPostionType == MrPositionType.LEFT)
            {
                foreach (CMrBoltArray mrBoltArray in mrBoltArrayList)
                {
                    if (!CDimTools.GetInstance().IsTwoVectorParallel(mrBoltArray.mNormal, new Vector(0, 0, 1)))
                    {
                        continue;
                    }
                    if (mrBoltArray.mBoltArrayShapeType == MrBoltArrayShapeType.ARRAY)
                    {
                        mrDimSet.AddRange(mrBoltArray.GetMinXPointList());
                    }
                    else if (mrBoltArray.mBoltArrayShapeType == MrBoltArrayShapeType.OBLIQUELINE)
                    {
                        Point firstPoint = mrBoltArray.mFirstPoint;
                        Point secondPoint = mrBoltArray.mSecondPoint;

                        List<Point> boltPointList = mrBoltArray.GetBoltPointList();

                        if (boltPointList.Count < 4)
                        {
                            mrDimSet.AddPoint(mrBoltArray.GetMinYPoint());
                            mrDimSet.AddPoint(mrBoltArray.GetMaxYPoint());
                        }
                        else
                        {
                            Point boltPoint = null;

                            MrSlopeType mrSlopeType = CDimTools.GetInstance().JudgeLineSlope(firstPoint, secondPoint);

                            if (mrSlopeType == MrSlopeType.MORETHAN_ZERO)
                            {
                                boltPoint = mrBoltArray.GetMinYPoint();
                            }
                            else if (mrSlopeType == MrSlopeType.LESSTHAN_ZERO)
                            {
                                boltPoint = mrBoltArray.GetMaxYPoint();
                            }
                            Vector directionVector = new Vector(secondPoint.X - firstPoint.X, secondPoint.Y - firstPoint.Y, secondPoint.Z - firstPoint.Z);

                            List<Point> pointList = new List<Point>();

                            foreach (Point pt in boltPointList)
                            {
                                Vector newVector = new Vector(pt.X - boltPoint.X, pt.Y - boltPoint.Y, pt.Z - boltPoint.Z);

                                if (CDimTools.GetInstance().IsTwoVectorVertical(directionVector, newVector))
                                {
                                    pointList.Add(pt);
                                }
                            }
                            int nCount = pointList.Count;
                            
                            if (nCount > 2)
                            {
                                mrDimSet.AddPoint(pointList[0]);
                                mrDimSet.AddPoint(pointList[nCount - 1]);
                            }
                            else
                            {
                                mrDimSet.AddRange(pointList);
                            }
                        }
                    }
                }
                if (mrDimSet.Count == 0)
                {
                    return null;
                }

                mrDimSet.mDimDistance = Math.Abs(mMrPart.GetMinXPoint().X - mrDimSet.GetDimPointList()[0].X) + CCommonPara.mDefaultDimDistance;
                mrDimSet.mDimVector = new Vector(-1, 0, 0);

                //如果该零件是剪切板中的中间板;
                if (CMrClipPlateManager.GetInstance().IsInClipPlate(mMrPart))
                {
                    CMrClipPlate mrClipPlate = CMrClipPlateManager.GetInstance().GetMrClipPlate(mMrPart);
                    mrDimSet.AddPoint(mrClipPlate.mMrTopPart.GetMaxYMinXPoint());
                    mrDimSet.AddPoint(mrClipPlate.mMrBottomPart.GetMinYMinXPoint());
                }
                //如果该零件是H型钢，且该型钢还是水平的;
                else if (CDimTools.GetInstance().IsPartTheHProfileSheet(mMrPart))
                {
                    MrSlopeType mCenterLineSlopeType = CDimTools.GetInstance().JudgeLineSlope(mMrPart.mCenterLinePointList[0], mMrPart.mCenterLinePointList[1]);

                    if (mCenterLineSlopeType == MrSlopeType.EQUAL_ZERO)
                    {
                        mrDimSet.AddPoint(mMrPart.GetMaxYMinXPoint());
                        mrDimSet.AddPoint(mMrPart.GetMinYMinXPoint());
                    }
                }
                return mrDimSet;
            }
            //如果零件在主梁的右侧;
            else if (mPostionType == MrPositionType.RIGHT)
            {
                foreach (CMrBoltArray mrBoltArray in mrBoltArrayList)
                {
                    if (!CDimTools.GetInstance().IsTwoVectorParallel(mrBoltArray.mNormal, new Vector(0, 0, 1)))
                    {
                        continue;
                    }
                    if (mrBoltArray.mBoltArrayShapeType == MrBoltArrayShapeType.ARRAY)
                    {
                        mrDimSet.AddRange(mrBoltArray.GetMaxXPointList());
                    }
                    else if (mrBoltArray.mBoltArrayShapeType == MrBoltArrayShapeType.OBLIQUELINE)
                    {
                        Point firstPoint = mrBoltArray.mFirstPoint;
                        Point secondPoint = mrBoltArray.mSecondPoint;

                        List<Point> boltPointList = mrBoltArray.GetBoltPointList();

                        if (boltPointList.Count < 4)
                        {
                            mrDimSet.AddPoint(mrBoltArray.GetMinYPoint());
                            mrDimSet.AddPoint(mrBoltArray.GetMaxYPoint());
                        }
                        else
                        {
                            Point boltPoint = null;
                            MrSlopeType mrSlopeType = CDimTools.GetInstance().JudgeLineSlope(firstPoint, secondPoint);

                            if (mrSlopeType == MrSlopeType.MORETHAN_ZERO)
                            {
                                boltPoint = mrBoltArray.GetMaxYPoint();
                            }
                            else if (mrSlopeType == MrSlopeType.LESSTHAN_ZERO)
                            {
                                boltPoint = mrBoltArray.GetMinYPoint();
                            }
                            Vector directionVector = new Vector(secondPoint.X - firstPoint.X, secondPoint.Y - firstPoint.Y, secondPoint.Z - firstPoint.Z);

                            List<Point> pointList = new List<Point>();

                            foreach (Point pt in boltPointList)
                            {
                                Vector newVector = new Vector(pt.X - boltPoint.X, pt.Y - boltPoint.Y, pt.Z - boltPoint.Z);

                                if (CDimTools.GetInstance().IsTwoVectorVertical(directionVector, newVector))
                                {
                                    pointList.Add(pt);
                                }
                            }

                            int nCount = pointList.Count;

                            if (nCount > 2)
                            {
                                mrDimSet.AddPoint(pointList[0]);
                                mrDimSet.AddPoint(pointList[nCount - 1]);
                            }
                            else
                            {
                                mrDimSet.AddRange(pointList);
                            }
                        }
                    }
                }

                if (mrDimSet.Count == 0)
                {
                    return null;
                }

                mrDimSet.mDimDistance = Math.Abs(mMrPart.GetMaxXPoint().X - mrDimSet.GetDimPointList()[0].X) + CCommonPara.mDefaultDimDistance;
                mrDimSet.mDimVector = new Vector(1, 0, 0);

                //如果该零件是剪切板中的中间板;
                if (CMrClipPlateManager.GetInstance().IsInClipPlate(mMrPart))
                {
                    CMrClipPlate mrClipPlate = CMrClipPlateManager.GetInstance().GetMrClipPlate(mMrPart);
                    mrDimSet.AddPoint(mrClipPlate.mMrTopPart.GetMaxYMaxXPoint());
                    mrDimSet.AddPoint(mrClipPlate.mMrBottomPart.GetMinYMaxXPoint());
                }
                //如果该零件是H型钢,且该型钢还是水平的;
                else if (CDimTools.GetInstance().IsPartTheHProfileSheet(mMrPart))
                {
                    MrSlopeType mCenterLineSlopeType = CDimTools.GetInstance().JudgeLineSlope(mMrPart.mCenterLinePointList[0], mMrPart.mCenterLinePointList[1]);

                    if (mCenterLineSlopeType == MrSlopeType.EQUAL_ZERO)
                    {
                        mrDimSet.AddPoint(mMrPart.GetMaxYMaxXPoint());
                        mrDimSet.AddPoint(mMrPart.GetMinYMaxXPoint());
                    }
                }
                return mrDimSet;
            }
            return null;
        }

        /// <summary>
        /// 获得支撑板及牛腿上的螺钉在X方向上的标注;
        /// </summary>
        /// <returns></returns>
        public CMrDimSet GetSupportPlateBoltXDimSet()
        {
            CMrDimSet mrDimSet = new CMrDimSet();

            Vector normal = mMrPart.mNormal;

            if (!mMrPart.IsHaveBolt() || !CDimTools.GetInstance().IsTwoVectorParallel(normal, new Vector(0, 0, 1)) || IsOutsidePlate(mMrPart))
            {
                return null;
            }
            if (CDimTools.GetInstance().IsPartTheAngleSteel(mMrPart))
            {
                return null;
            }

            //如果是柱子上下端的双夹板;
            if (mMrPart.GetMinYPoint().Y < 0 && mMrPart.GetMaxYPoint().Y > 0)
            {
                return null;
            }
            if (mMrPart.GetMinYPoint().Y < CMrMainBeam.GetInstance().GetMaxYPoint().Y &&
                mMrPart.GetMaxYPoint().Y > CMrMainBeam.GetInstance().GetMaxYPoint().Y)
            {
                return null;
            }

            List<CMrBoltArray> mrBoltArrayList = mMrPart.GetBoltArrayList();

            //如果零件在主梁的左侧;
            if (mPostionType == MrPositionType.LEFT)
            {
                bool bUpDim = false;

                foreach (CMrBoltArray mrBoltArray in mrBoltArrayList)
                {
                    if (!CDimTools.GetInstance().IsTwoVectorParallel(mrBoltArray.mNormal, new Vector(0, 0, 1)))
                    {
                        continue;
                    }
                    if (mrBoltArray.mBoltArrayShapeType == MrBoltArrayShapeType.ARRAY)
                    {
                        mrDimSet.AddRange(mrBoltArray.GetMinYPointList());
                    }
                    else if (mrBoltArray.mBoltArrayShapeType == MrBoltArrayShapeType.OBLIQUELINE)
                    {
                        Point firstPoint = mrBoltArray.mFirstPoint;
                        Point secondPoint = mrBoltArray.mSecondPoint;

                        Point boltPoint = null;

                        MrSlopeType mrSlopeType = CDimTools.GetInstance().JudgeLineSlope(firstPoint, secondPoint);

                        if (mrSlopeType == MrSlopeType.MORETHAN_ZERO)
                        {
                            bUpDim = false;
                            boltPoint = mrBoltArray.GetMinYPoint();
                        }
                        else if (mrSlopeType == MrSlopeType.LESSTHAN_ZERO)
                        {
                            bUpDim = true;
                            boltPoint = mrBoltArray.GetMaxYPoint();
                        }

                        List<Point> boltPointList = mrBoltArray.GetBoltPointList();

                        if (boltPointList.Count < 4)
                        {
                            mrDimSet.AddPoint(mrBoltArray.GetMinYPoint());
                            mrDimSet.AddPoint(mrBoltArray.GetMaxYPoint());
                        }
                        else
                        {
                            List<Point> pointList = new List<Point>();

                            Vector directionVector = new Vector(secondPoint.X - firstPoint.X, secondPoint.Y - firstPoint.Y, secondPoint.Z - firstPoint.Z);
                            foreach (Point pt in boltPointList)
                            {
                                Vector newVector = new Vector(pt.X - boltPoint.X, pt.Y - boltPoint.Y, pt.Z - boltPoint.Z);
                                if (CDimTools.GetInstance().IsTwoVectorVertical(directionVector, newVector))
                                {
                                    pointList.Add(pt);
                                }
                            }
                            int nCount = pointList.Count;

                            if (nCount > 2)
                            {
                                mrDimSet.AddPoint(pointList[0]);
                                mrDimSet.AddPoint(pointList[nCount - 1]);
                            }
                            else
                            {
                                mrDimSet.AddRange(pointList);
                            }
                        }
                    }
                }
                if (mrDimSet.Count == 0)
                {
                    return null;
                }
                if (bUpDim)
                {
                    mrDimSet.mDimDistance = Math.Abs(mMrPart.GetMaxYPoint().Y - mrDimSet.GetDimPointList()[0].Y) + CCommonPara.mDefaultDimDistance;
                    mrDimSet.mDimVector = new Vector(0, 1, 0);
                }
                else
                {
                    mrDimSet.mDimDistance = Math.Abs(mMrPart.GetMinYPoint().Y - mrDimSet.GetDimPointList()[0].Y) + CCommonPara.mDefaultDimDistance;
                    mrDimSet.mDimVector = new Vector(0, -1, 0);
                }

                mrDimSet.AddPoint(new Point(CMrMainBeam.GetInstance().GetMinXPoint().X, mrDimSet.GetDimPointList()[0].Y, 0));

                return mrDimSet;
            }
            //如果零件在主梁的右侧;
            else if (mPostionType == MrPositionType.RIGHT)
            {
                bool bUpDim = false;

                foreach (CMrBoltArray mrBoltArray in mrBoltArrayList)
                {
                    if (!CDimTools.GetInstance().IsTwoVectorParallel(mrBoltArray.mNormal, new Vector(0, 0, 1)))
                    {
                        continue;
                    }
                    if (mrBoltArray.mBoltArrayShapeType == MrBoltArrayShapeType.ARRAY)
                    {
                        mrDimSet.AddRange(mrBoltArray.GetMinYPointList());
                    }
                    else if (mrBoltArray.mBoltArrayShapeType == MrBoltArrayShapeType.OBLIQUELINE)
                    {
                        Point firstPoint = mrBoltArray.mFirstPoint;
                        Point secondPoint = mrBoltArray.mSecondPoint;

                        Point boltPoint = null;

                        MrSlopeType mrSlopeType = CDimTools.GetInstance().JudgeLineSlope(firstPoint, secondPoint);

                        if (mrSlopeType == MrSlopeType.MORETHAN_ZERO)
                        {
                            bUpDim = true;
                            boltPoint = mrBoltArray.GetMaxYPoint();
                        }
                        else if (mrSlopeType == MrSlopeType.LESSTHAN_ZERO)
                        {
                            bUpDim = false;
                            boltPoint = mrBoltArray.GetMinYPoint();
                        }
                        List<Point> boltPointList = mrBoltArray.GetBoltPointList();

                        if (boltPointList.Count < 4)
                        {
                            mrDimSet.AddPoint(mrBoltArray.GetMinYPoint());
                            mrDimSet.AddPoint(mrBoltArray.GetMaxYPoint());
                        }
                        else
                        {
                            List<Point> pointList = new List<Point>();

                            Vector directionVector = new Vector(secondPoint.X - firstPoint.X, secondPoint.Y - firstPoint.Y, secondPoint.Z - firstPoint.Z);

                            foreach (Point pt in boltPointList)
                            {
                                Vector newVector = new Vector(pt.X - boltPoint.X, pt.Y - boltPoint.Y, pt.Z - boltPoint.Z);
                                if (CDimTools.GetInstance().IsTwoVectorVertical(directionVector, newVector))
                                {
                                    pointList.Add(pt);
                                }
                            }

                            int nCount = pointList.Count;

                            if (nCount > 2)
                            {
                                mrDimSet.AddPoint(pointList[0]);
                                mrDimSet.AddPoint(pointList[nCount - 1]);
                            }
                            else
                            {
                                mrDimSet.AddRange(pointList);
                            }
                        }
                    }
                }
                if (mrDimSet.Count == 0)
                {
                    return null;
                }
                if (bUpDim)
                {
                    mrDimSet.mDimDistance = Math.Abs(mMrPart.GetMaxYPoint().Y - mrDimSet.GetDimPointList()[0].Y) + CCommonPara.mDefaultDimDistance;
                    mrDimSet.mDimVector = new Vector(0, 1, 0);
                }
                else
                {
                    mrDimSet.mDimDistance = Math.Abs(mMrPart.GetMinYPoint().Y - mrDimSet.GetDimPointList()[0].Y) + CCommonPara.mDefaultDimDistance;
                    mrDimSet.mDimVector = new Vector(0, -1, 0);
                }
                mrDimSet.AddPoint(new Point(CMrMainBeam.GetInstance().GetMaxXPoint().X, mrDimSet.GetDimPointList()[0].Y, 0));

                return mrDimSet;
            }
            return null;
        }

        /// <summary>
        /// 获得角钢上的螺钉在X方向上的标注;
        /// </summary>
        /// <returns></returns>
        public CMrDimSet GetAngleSheetBoltXDimSet()
        {
            CMrDimSet mrDimSet = new CMrDimSet();

            Vector normal = mMrPart.mNormal;

            if (!mMrPart.IsHaveBolt() || IsOutsidePlate(mMrPart) || !CDimTools.GetInstance().IsPartTheAngleSteel(mMrPart))
            {
                return null;
            }

            List<CMrBoltArray> mrBoltArrayList = mMrPart.GetBoltArrayList();

            //如果零件在主梁的左侧;
            if (mPostionType == MrPositionType.LEFT)
            {
                foreach (CMrBoltArray mrBoltArray in mrBoltArrayList)
                {
                    if (!CDimTools.GetInstance().IsTwoVectorParallel(mrBoltArray.mNormal, new Vector(0, 0, 1)))
                    {
                        continue;
                    }
                    if (mrBoltArray.mBoltArrayShapeType == MrBoltArrayShapeType.ARRAY)
                    {
                        mrDimSet.AddRange(mrBoltArray.GetMinYPointList());
                    }
                }
                  
                if (mrDimSet.Count == 0)
                {
                    return null;
                }
                mrDimSet.mDimDistance = Math.Abs(mMrPart.GetMinYPoint().Y - mrDimSet.GetDimPointList()[0].Y) + CCommonPara.mDefaultDimDistance;
                mrDimSet.mDimVector = new Vector(0, -1, 0);
                mrDimSet.AddPoint(new Point(CMrMainBeam.GetInstance().GetMinXPoint().X, mrDimSet.GetDimPointList()[0].Y, 0));

                return mrDimSet;
            }
            //如果零件在主梁的右侧;
            else if (mPostionType == MrPositionType.RIGHT)
            {
                foreach (CMrBoltArray mrBoltArray in mrBoltArrayList)
                {
                    if (!CDimTools.GetInstance().IsTwoVectorParallel(mrBoltArray.mNormal, new Vector(0, 0, 1)))
                    {
                        continue;
                    }
                    if (mrBoltArray.mBoltArrayShapeType == MrBoltArrayShapeType.ARRAY)
                    {
                        mrDimSet.AddRange(mrBoltArray.GetMinYPointList());
                    }
                }
                if (mrDimSet.Count == 0)
                {
                    return null;
                }
                
                mrDimSet.mDimDistance = Math.Abs(mMrPart.GetMinYPoint().Y - mrDimSet.GetDimPointList()[0].Y) + CCommonPara.mDefaultDimDistance;
                mrDimSet.mDimVector = new Vector(0, -1, 0);
                mrDimSet.AddPoint(new Point(CMrMainBeam.GetInstance().GetMaxXPoint().X, mrDimSet.GetDimPointList()[0].Y, 0));
                return mrDimSet;
            }
            return null;
        }

        /// <summary>
        /// 判断零件是否是外围板;
        /// </summary>
        /// <param name="mrPart"></param>
        /// <returns></returns>
        private bool IsOutsidePlate(CMrPart mrPart)
        {
            Vector normal = mrPart.mNormal;
            double minZ = mrPart.GetMinZPoint().Z;
            double maxZ = mrPart.GetMaxZPoint().Z;

            double mainBeamMinZ = CMrMainBeam.GetInstance().GetMinZPoint().Z;
            double mainBeamMaxZ = CMrMainBeam.GetInstance().GetMaxZPoint().Z;

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
        /// 获取零件标记;
        /// </summary>
        public CMrMark GetPartMark()
        {
            Vector normal = mMrPart.mNormal;
            Vector xVector = new Vector(1, 0, 0);
            Vector yVector = new Vector(0, 1, 0);
            Vector zVector = new Vector(0, 0, 1);
            CMrMark mrMark = new CMrMark();

            //1.如果零部件的法向与X轴平行则不标记;
            if (CDimTools.GetInstance().IsTwoVectorParallel(normal, xVector))
            {
                return null;
            }

            //2.如果与z轴或Y轴平行,并且该板的z坐标大于主梁的最大值的z坐标,则不进行标注;
            if (CDimTools.GetInstance().IsTwoVectorParallel(normal, zVector))
            {
                double mainBeamMaxZ = Math.Abs(CMrMainBeam.GetInstance().mMaxPoint.Z);
                double mainBeamMinZ = -Math.Abs(CMrMainBeam.GetInstance().mMinPoint.Z);

                double partZ = mMrPart.mLeftBottomPoint.Z;

                if (CDimTools.GetInstance().CompareTwoDoubleValue(partZ, mainBeamMinZ) < 0 ||
                    CDimTools.GetInstance().CompareTwoDoubleValue(partZ, mainBeamMaxZ) > 0)
                {
                    return null;
                }
            }

            if (mPostionType == MrPositionType.LEFT)
            {
                MrClipPlatePosType mrClipPlatePosType = MrClipPlatePosType.NONE;

                //如果零件在剪切板中;
                if (CMrClipPlateManager.GetInstance().IsInClipPlate(mMrPart, out mrClipPlatePosType))
                {
                    if (mrClipPlatePosType == MrClipPlatePosType.TOP)
                    {
                        Point minXmaxYPoint = mMrPart.GetMinXMaxYPoint();
                        mrMark.mModelObject = mMrPart.mPartInDrawing;
                        mrMark.mInsertPoint = minXmaxYPoint;

                        double increaseXDistance = 0.8 * CCommonPara.mPartMarkLength / Math.Tan(CCommonPara.mPartMarkAngle);
                        mrMark.mTextPoint.X = mrMark.mInsertPoint.X - increaseXDistance;
                        mrMark.mTextPoint.Y = mrMark.mInsertPoint.Y + 0.8 * CCommonPara.mPartMarkLength;

                        return mrMark;
                    }
                    if (mrClipPlatePosType == MrClipPlatePosType.MIDDLE)
                    {
                        Point minXPoint = mMrPart.GetMinXPoint();
                        mrMark.mModelObject = mMrPart.mPartInDrawing;
                        mrMark.mInsertPoint = new Point(minXPoint.X + 10, mMrPart.mMidPoint.Y, 0);

                        double increaseXDistance = 2 * CCommonPara.mPartMarkLength / Math.Tan(CCommonPara.mPartMarkAngle);
                        mrMark.mTextPoint.X = mrMark.mInsertPoint.X - increaseXDistance;
                        mrMark.mTextPoint.Y = mrMark.mInsertPoint.Y + 2 * CCommonPara.mPartMarkLength;

                        return mrMark;
                    }
                    if (mrClipPlatePosType == MrClipPlatePosType.BOTTOM)
                    {
                        Point minXminYPoint = mMrPart.GetMinXMinYPoint();
                        mrMark.mModelObject = mMrPart.mPartInDrawing;
                        mrMark.mInsertPoint = minXminYPoint;

                        double increaseXDistance = 0.8 * CCommonPara.mPartMarkLength / Math.Tan(CCommonPara.mPartMarkAngle);
                        mrMark.mTextPoint.X = mrMark.mInsertPoint.X - increaseXDistance;
                        mrMark.mTextPoint.Y = mrMark.mInsertPoint.Y - 0.8 * CCommonPara.mPartMarkLength;

                        return mrMark;
                    }
                }
                //如果斜率与Z轴平行;
                else if (CDimTools.GetInstance().IsTwoVectorParallel(normal, zVector))
                {
                    //如果是多边形板;
                    if (mMrPart.mBeamType == MrBeamType.CONTOURPLATE || mMrPart.mBeamType == MrBeamType.POLYBEAM)
                    {
                        List<CMrBoltArray> mrBoltArrayList = mMrPart.GetBoltArrayList();

                        if (mrBoltArrayList != null && mrBoltArrayList.Count > 0)
                        {
                            CMrBoltArray mrBoltArray = mrBoltArrayList[0];
                            Point minXPoint = mrBoltArray.GetMinXMinYPoint();

                            mrMark.mModelObject = mMrPart.mPartInDrawing;
                            mrMark.mInsertPoint = new Point(minXPoint.X - 2, minXPoint.Y, 0);

                            MrSlopeType mrSlopeType = CDimTools.GetInstance().JudgeLineSlope(mrBoltArray.mFirstPoint, mrBoltArray.mSecondPoint);

                            if (mrSlopeType == MrSlopeType.MORETHAN_ZERO)
                            {
                                double increaseXDistance = 2 * CCommonPara.mPartMarkLength / Math.Tan(CCommonPara.mPartMarkAngle);
                                mrMark.mTextPoint.X = mrMark.mInsertPoint.X - increaseXDistance;
                                mrMark.mTextPoint.Y = mrMark.mInsertPoint.Y - 2 * CCommonPara.mPartMarkLength;
                            }
                            else if (mrSlopeType == MrSlopeType.LESSTHAN_ZERO)
                            {
                                double increaseXDistance = 2 * CCommonPara.mPartMarkLength / Math.Tan(CCommonPara.mPartMarkAngle);
                                mrMark.mTextPoint.X = mrMark.mInsertPoint.X - increaseXDistance;
                                mrMark.mTextPoint.Y = mrMark.mInsertPoint.Y + 2 * CCommonPara.mPartMarkLength;
                            }
                            else
                            {
                                double increaseXDistance = 0.8 * CCommonPara.mPartMarkLength / Math.Tan(CCommonPara.mPartMarkAngle);
                                mrMark.mTextPoint.X = mrMark.mInsertPoint.X - increaseXDistance;
                                mrMark.mTextPoint.Y = mrMark.mInsertPoint.Y - 0.8 * CCommonPara.mPartMarkLength;
                            }
                            return mrMark;
                        }
                        //如果多边形板中没有螺钉;
                        else
                        {
                            mrMark.mModelObject = mMrPart.mPartInDrawing;
                            mrMark.mInsertPoint = mMrPart.GetMinXMinYPoint();

                            double increaseXDistance = 0.8 * CCommonPara.mPartMarkLength / Math.Tan(CCommonPara.mPartMarkAngle);
                            mrMark.mTextPoint.X = mrMark.mInsertPoint.X - increaseXDistance;
                            mrMark.mTextPoint.Y = mrMark.mInsertPoint.Y - 0.8 * CCommonPara.mPartMarkLength;

                            return mrMark;
                        }
                    }
                    //如果不是多边形板，是规则的板;
                    else
                    {
                        mrMark.mModelObject = mMrPart.mPartInDrawing;
                        mrMark.mInsertPoint = mMrPart.GetMinXMinYPoint();

                        double increaseXDistance = 0.8 * CCommonPara.mPartMarkLength / Math.Tan(CCommonPara.mPartMarkAngle);
                        mrMark.mTextPoint.X = mrMark.mInsertPoint.X - increaseXDistance;
                        mrMark.mTextPoint.Y = mrMark.mInsertPoint.Y - 0.8 * CCommonPara.mPartMarkLength;

                        return mrMark;
                    }
                }
                //如果斜率在XY平面内;
                else if (CDimTools.GetInstance().IsVectorInXYPlane(normal))
                {
                    if ((normal.X > CCommonPara.mDblError && normal.Y > CCommonPara.mDblError)
                        || (normal.X < CCommonPara.mDblError && normal.Y < CCommonPara.mDblError))
                    {
                        mrMark.mModelObject = mMrPart.mPartInDrawing;
                        mrMark.mInsertPoint = mMrPart.GetMinXMinYPoint();

                        double increaseXDistance = 2 * CCommonPara.mPartMarkLength / Math.Tan(CCommonPara.mPartMarkAngle);
                        mrMark.mTextPoint.X = mrMark.mInsertPoint.X - increaseXDistance;
                        mrMark.mTextPoint.Y = mrMark.mInsertPoint.Y + 2 * CCommonPara.mPartMarkLength;

                        return mrMark;
                    }
                    else if ((normal.X < CCommonPara.mDblError && normal.Y > CCommonPara.mDblError)
                          || (normal.X > CCommonPara.mDblError && normal.Y < CCommonPara.mDblError))
                    {
                        mrMark.mModelObject = mMrPart.mPartInDrawing;
                        mrMark.mInsertPoint = mMrPart.GetMinXMinYPoint();

                        double increaseXDistance = 2 * CCommonPara.mPartMarkLength / Math.Tan(CCommonPara.mPartMarkAngle);
                        mrMark.mTextPoint.X = mrMark.mInsertPoint.X - increaseXDistance;
                        mrMark.mTextPoint.Y = mrMark.mInsertPoint.Y - 2 * CCommonPara.mPartMarkLength;

                        return mrMark;
                    }
                }
            }
            if (mPostionType == MrPositionType.RIGHT)
            {
                MrClipPlatePosType mrClipPlatePosType = MrClipPlatePosType.NONE;

                //如果零件在剪切板中;
                if (CMrClipPlateManager.GetInstance().IsInClipPlate(mMrPart, out mrClipPlatePosType))
                {
                    if (mrClipPlatePosType == MrClipPlatePosType.TOP)
                    {
                        Point maxXmaxYPoint = mMrPart.GetMaxXMaxYPoint();
                        mrMark.mModelObject = mMrPart.mPartInDrawing;
                        mrMark.mInsertPoint = maxXmaxYPoint;

                        double increaseXDistance = 0.8 * CCommonPara.mPartMarkLength / Math.Tan(CCommonPara.mPartMarkAngle);
                        mrMark.mTextPoint.X = mrMark.mInsertPoint.X + increaseXDistance;
                        mrMark.mTextPoint.Y = mrMark.mInsertPoint.Y + 0.8 * CCommonPara.mPartMarkLength;

                        return mrMark;
                    }
                    if (mrClipPlatePosType == MrClipPlatePosType.MIDDLE)
                    {
                        Point maxXPoint = mMrPart.GetMaxXPoint();
                        mrMark.mModelObject = mMrPart.mPartInDrawing;
                        mrMark.mInsertPoint = new Point(maxXPoint.X - 10, mMrPart.mMidPoint.Y, 0);

                        double increaseXDistance = 2 * CCommonPara.mPartMarkLength / Math.Tan(CCommonPara.mPartMarkAngle);
                        mrMark.mTextPoint.X = mrMark.mInsertPoint.X + increaseXDistance;
                        mrMark.mTextPoint.Y = mrMark.mInsertPoint.Y + 2 * CCommonPara.mPartMarkLength;

                        return mrMark;
                    }
                    if (mrClipPlatePosType == MrClipPlatePosType.BOTTOM)
                    {
                        Point maxXminYPoint = mMrPart.GetMaxXMinYPoint();
                        mrMark.mModelObject = mMrPart.mPartInDrawing;
                        mrMark.mInsertPoint = maxXminYPoint;

                        double increaseXDistance = 0.8 * CCommonPara.mPartMarkLength / Math.Tan(CCommonPara.mPartMarkAngle);
                        mrMark.mTextPoint.X = mrMark.mInsertPoint.X + increaseXDistance;
                        mrMark.mTextPoint.Y = mrMark.mInsertPoint.Y - 0.8 * CCommonPara.mPartMarkLength;

                        return mrMark;
                    }
                }
                //如果斜率与Z轴平行;
                else if (CDimTools.GetInstance().IsTwoVectorParallel(normal, zVector))
                {
                    //如果是多边形板;
                    if (mMrPart.mBeamType == MrBeamType.CONTOURPLATE || mMrPart.mBeamType == MrBeamType.POLYBEAM)
                    {
                        List<CMrBoltArray> mrBoltArrayList = mMrPart.GetBoltArrayList();

                        if (mrBoltArrayList != null && mrBoltArrayList.Count > 0)
                        {
                            CMrBoltArray mrBoltArray = mrBoltArrayList[0];
                            Point maxXPoint = mrBoltArray.GetMaxXMinYPoint();

                            mrMark.mModelObject = mMrPart.mPartInDrawing;
                            mrMark.mInsertPoint = new Point(maxXPoint.X + 2, maxXPoint.Y, 0);

                            MrSlopeType mrSlopeType = CDimTools.GetInstance().JudgeLineSlope(mrBoltArray.mFirstPoint, mrBoltArray.mSecondPoint);

                            if (mrSlopeType == MrSlopeType.MORETHAN_ZERO)
                            {
                                double increaseXDistance = 2 * CCommonPara.mPartMarkLength / Math.Tan(CCommonPara.mPartMarkAngle);
                                mrMark.mTextPoint.X = mrMark.mInsertPoint.X + increaseXDistance;
                                mrMark.mTextPoint.Y = mrMark.mInsertPoint.Y + 2 * CCommonPara.mPartMarkLength;
                            }
                            else if (mrSlopeType == MrSlopeType.LESSTHAN_ZERO)
                            {
                                double increaseXDistance = 2 * CCommonPara.mPartMarkLength / Math.Tan(CCommonPara.mPartMarkAngle);
                                mrMark.mTextPoint.X = mrMark.mInsertPoint.X + increaseXDistance;
                                mrMark.mTextPoint.Y = mrMark.mInsertPoint.Y - 2 * CCommonPara.mPartMarkLength;
                            }
                            else
                            {
                                double increaseXDistance = 0.8 * CCommonPara.mPartMarkLength / Math.Tan(CCommonPara.mPartMarkAngle);
                                mrMark.mTextPoint.X = mrMark.mInsertPoint.X + increaseXDistance;
                                mrMark.mTextPoint.Y = mrMark.mInsertPoint.Y - 0.8 * CCommonPara.mPartMarkLength;
                            }
                            return mrMark;
                        }
                        //如果多边形板中没有螺钉;
                        else
                        {
                            mrMark.mModelObject = mMrPart.mPartInDrawing;
                            mrMark.mInsertPoint = mMrPart.GetMaxXMinYPoint();

                            double increaseXDistance = 0.8 * CCommonPara.mPartMarkLength / Math.Tan(CCommonPara.mPartMarkAngle);
                            mrMark.mTextPoint.X = mrMark.mInsertPoint.X + increaseXDistance;
                            mrMark.mTextPoint.Y = mrMark.mInsertPoint.Y - 0.8 * CCommonPara.mPartMarkLength;

                            return mrMark;
                        }
                    }
                    //如果不是多边形板，是规则的板;
                    else
                    {
                        mrMark.mModelObject = mMrPart.mPartInDrawing;
                        mrMark.mInsertPoint = mMrPart.GetMaxXMinYPoint();

                        double increaseXDistance = 0.8 * CCommonPara.mPartMarkLength / Math.Tan(CCommonPara.mPartMarkAngle);
                        mrMark.mTextPoint.X = mrMark.mInsertPoint.X + increaseXDistance;
                        mrMark.mTextPoint.Y = mrMark.mInsertPoint.Y - 0.8 * CCommonPara.mPartMarkLength;

                        return mrMark;
                    }
                }
                //如果斜率在XY平面内;
                else if (CDimTools.GetInstance().IsVectorInXYPlane(normal))
                {
                    if ((normal.X > CCommonPara.mDblError && normal.Y > CCommonPara.mDblError)
                        || (normal.X < CCommonPara.mDblError && normal.Y < CCommonPara.mDblError))
                    {
                        mrMark.mModelObject = mMrPart.mPartInDrawing;
                        mrMark.mInsertPoint = mMrPart.GetMaxXMinYPoint();

                        double increaseXDistance = 2 * CCommonPara.mPartMarkLength / Math.Tan(CCommonPara.mPartMarkAngle);
                        mrMark.mTextPoint.X = mrMark.mInsertPoint.X + increaseXDistance;
                        mrMark.mTextPoint.Y = mrMark.mInsertPoint.Y - 2 * CCommonPara.mPartMarkLength;

                        return mrMark;
                    }
                    else if ((normal.X < CCommonPara.mDblError && normal.Y > CCommonPara.mDblError)
                          || (normal.X > CCommonPara.mDblError && normal.Y < CCommonPara.mDblError))
                    {
                        mrMark.mModelObject = mMrPart.mPartInDrawing;
                        mrMark.mInsertPoint = mMrPart.GetMaxXMinYPoint();

                        double increaseXDistance = 2 * CCommonPara.mPartMarkLength / Math.Tan(CCommonPara.mPartMarkAngle);
                        mrMark.mTextPoint.X = mrMark.mInsertPoint.X + increaseXDistance;
                        mrMark.mTextPoint.Y = mrMark.mInsertPoint.Y + 2 * CCommonPara.mPartMarkLength;

                        return mrMark;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// 判断该零件是否是垫块;
        /// </summary>
        /// <param name="mrPart"></param>
        /// <returns></returns>
        private bool IsHeelBlock(CMrPart mrPart)
        {
            Vector normal = mrPart.mNormal;

            if (mrPart.IsHaveBolt())
            {
                return false;
            }
            if (!CDimTools.GetInstance().IsTwoVectorParallel(normal, new Vector(0, 0, 1)))
            {
                return false;
            }
            if (CDimTools.GetInstance().CompareTwoDoubleValue(mMrPart.GetZSpaceValue(), 8) <= 0 &&
               CDimTools.GetInstance().CompareTwoDoubleValue(mMrPart.GetYSpaceValue(), 30) <= 0 &&
               CDimTools.GetInstance().CompareTwoDoubleValue(mMrPart.GetXSpaceValue(), CMrMainBeam.GetInstance().GetXSpaceValue()) >= 0)
            {
                return true;
            }

            return false;
        }
    }
}
