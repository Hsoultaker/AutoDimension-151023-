using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tekla.Structures.Geometry3d;

namespace AutoDimension.Entity
{
    /// <summary>
    /// 门式框架结构柱在前视图中标注的管理器;
    /// </summary>
    public class CMrCylinderDoorFrontManager
    {
        /// <summary>
        /// 单一实例;
        /// </summary>
        private static CMrCylinderDoorFrontManager mInstance = null;

        /// <summary>
        /// 门式框架中在前视图中进行整体标注的零部件;
        /// </summary>
        public List<CMrPart> mRightDimPartList = new List<CMrPart>();

        /// <summary>
        /// 门式框架中在在主梁内部并且法向与Y轴平行的零部件;
        /// </summary>
        public List<CMrPart> mYNormalMiddlePartList = new List<CMrPart>();

        /// <summary>
        /// 门式框架中在前视图中法向与X轴平行的需要进行单独标注的零部件;
        /// </summary>
        public List<CMrPart> mXNormalAloneDimPartList = new List<CMrPart>();

        /// <summary>
        /// 构建在主梁外围法向与Y轴平行的零件与檩托板的映射表;
        /// </summary>
        public Dictionary<CMrPart, CMrApronPlate> mMapYNormalPartToMrApronPlate = new Dictionary<CMrPart, CMrApronPlate>();

        /// <summary>
        /// 构建在主梁外围法向与Z轴平行的零部件与檩托板的映射表;
        /// </summary>
        public Dictionary<CMrPart, CMrApronPlate> mMapZNormalPartToMrApronPlate = new Dictionary<CMrPart, CMrApronPlate>();

        /// <summary>
        /// 构建在主梁内部法向与Y轴平行的零部件与檩托板的映射表;
        /// </summary>
        public Dictionary<CMrPart, CMrApronPlate> mMapYNormalPartToMrApronPlate2 = new Dictionary<CMrPart, CMrApronPlate>();

        /// <summary>
        /// 前视图中在主梁下面法向与Y轴相同的零部件;
        /// </summary>
        public CMrPart mYNormalBottomPart = null;

        /// <summary>
        /// 前视图中在主梁下面法向与Z轴相同的零部件;
        /// </summary>
        public CMrPart mZNormalBottomPart = null;

        /// <summary>
        /// 前视图中主梁左侧上方竖直的挡板,该零件有可能不存在;
        /// </summary>
        public CMrPart mLeftTopPart = null;

        /// <summary>
        /// 前视图中主梁左侧上方中间竖直的挡板,该零件可能不存在;
        /// </summary>
        public CMrPart mLeftTopMiddlePart = null;

        /// <summary>
        /// 前视图中主梁左侧竖直的挡板;
        /// </summary>
        public CMrPart mLeftPart = null;

        /// <summary>
        /// 前视图中主梁右侧上方竖直的挡板,该零件有可能不存在;
        /// </summary>
        public CMrPart mRightTopPart = null;

        /// <summary>
        /// 前视图中主梁右侧上方中间竖直的挡板,该零件可能不存在;
        /// </summary>
        public CMrPart mRightTopMiddlePart = null;

        /// <summary>
        /// 前视图中主梁右侧竖直的挡板;
        /// </summary>
        public CMrPart mRightPart = null;

        /// <summary>
        /// 主梁顶部的零部件;
        /// </summary>
        public CMrPart mTopPart = null;

        /// <summary>
        /// 主梁最小的X值;
        /// </summary>
        private double mMainBeamMinX = 0.0;

        /// <summary>
        /// 主梁最大的X值;
        /// </summary>
        private double mMainBeamMaxX = 0.0;

        /// <summary>
        /// 主梁最小的Y值;
        /// </summary>
        private double mMainBeamMinY = 0.0;

        /// <summary>
        /// 主梁最大的Y值;
        /// </summary>
        private double mMainBeamMaxY = 0.0;

        /// <summary>
        /// 获取单例;
        /// </summary>
        /// <returns></returns>
        public static CMrCylinderDoorFrontManager GetInstance()
        {
            if (null == mInstance)
            {
                mInstance = new CMrCylinderDoorFrontManager();
            }

            return mInstance;
        }

        /// <summary>
        /// 添加右侧进行标注的零部件到标注链表中;
        /// </summary>
        /// <param name="mrPart"></param>
        public void AppendRightDimPart(CMrPart mrPart)
        {
            mRightDimPartList.Add(mrPart);
        }

        /// <summary>
        /// 添加右侧单独进行标注的零部件到标注链表中;
        /// </summary>
        /// <param name="mrPart"></param>
        public void AppendYNormalMiddleDimPart(CMrPart mrPart)
        {
            mYNormalMiddlePartList.Add(mrPart);
        }

        /// <summary>
        /// 添加法向与X轴平行在梁外侧，剪切板内部的零部件;
        /// </summary>
        /// <param name="mrPart"></param>
        public void AppendXNormalAloneDimPart(CMrPart mrPart)
        {
            mXNormalAloneDimPartList.Add(mrPart);
        }

        /// <summary>
        /// 构建门式框架结构前视图中的拓扑结构;
        /// </summary>
        /// <param name="mrPartList"></param>
        public void BuildCylinderDoorFrontTopo(List<CMrPart> mrPartList)
        {
            ClearData();

            mMainBeamMinX = CMrMainBeam.GetInstance().GetMinXPoint().X;
            mMainBeamMaxX = CMrMainBeam.GetInstance().GetMaxXPoint().X;
            mMainBeamMinY = CMrMainBeam.GetInstance().GetMinYPoint().Y;
            mMainBeamMaxY = CMrMainBeam.GetInstance().GetMaxYPoint().Y;

            //判断零部件;
            foreach (CMrPart mrPart in mrPartList)
            {
                JudgeZNormalBottomPart(mrPart);
                JudgeYNormalBottomPart(mrPart);
                JudgeLeftAndLeftTopPart(mrPart);
                JudgeRightAndRightTopPart(mrPart);
            }

            //判断左侧中间以及右侧中间的零部件;
            foreach (CMrPart mrPart in mrPartList)
            {
                if (mLeftTopPart != null)
                {
                    JudgeLeftMiddlePart(mrPart);
                }
                if(mRightTopPart != null)
                {
                    JudgeRightMiddlePart(mrPart);
                }
                JudgeTopPart(mrPart);
            }

            //构建类型1的檩托板映射表;
            BuildMrApronPlateType1(mrPartList);

            //构建类型2的檩托板映射表;
            BuildMrApronPlateType2(mrPartList);
        }

        /// <summary>
        /// 判断在主梁下方法向与Z轴平行的零部件;
        /// </summary>
        /// <param name="mrPart"></param>
        private void JudgeZNormalBottomPart(CMrPart mrPart)
        {
            if (mZNormalBottomPart != null)
            {
                return;
            }

            Vector normal = mrPart.mNormal;

            if (!CDimTools.GetInstance().IsTwoVectorParallel(normal, new Vector(0, 0, 1)))
            {
                return;
            }

            double partMaxY = mrPart.GetMaxYPoint().Y;

            if (partMaxY < mMainBeamMinY)
            {
                mZNormalBottomPart = mrPart;
            }
        }

        /// <summary>
        /// 判断主梁下方法向与Y轴平行的零部件,该零件的最大Y值与主梁的最小Y值相等;
        /// </summary>
        /// <param name="mrPart"></param>
        private void JudgeYNormalBottomPart(CMrPart mrPart)
        {
            if (mYNormalBottomPart != null)
            {
                return;
            }

            Vector normal = mrPart.mNormal;

            if (!CDimTools.GetInstance().IsTwoVectorParallel(normal, new Vector(0, 1, 0)))
            {
                return;
            }

            double partMaxY = mrPart.GetMaxYPoint().Y;

            if (Math.Abs(partMaxY -mMainBeamMinY) < 0.1)
            {
                mYNormalBottomPart = mrPart;
            }
        }

        /// <summary>
        /// 判断主梁左侧和左上侧的零部件,该零部件的法向与X轴平行;
        /// </summary>
        /// <param name="mrPart"></param>
        private void JudgeLeftAndLeftTopPart(CMrPart mrPart)
        {
            Vector normal = mrPart.mNormal;

            if(mrPart.GetMinXPoint().X > 0)
            {
                return;
            }

            double partMinX = mrPart.GetMinXPoint().X;
            double partMaxX = mrPart.GetMaxXPoint().X;
            double partMinY = mrPart.GetMinYPoint().Y;
            double partMaxY = mrPart.GetMaxYPoint().Y;

            double partHeight = Math.Abs(partMaxY - partMinY);
            double mainBeamHeight = Math.Abs(mMainBeamMaxY - mMainBeamMinY);

            //左边的零部件长度至少大于主梁的一半;
            if (partHeight > mainBeamHeight / 2.0 && Math.Abs(partMinY - mMainBeamMinY) < 0.5)
            {
                if (mLeftPart == null)
                {
                    mLeftPart = mrPart;
                }
                else if (mrPart.GetMinXPoint().X < mLeftPart.GetMinXPoint().X)
                {
                    mLeftPart = mrPart;
                }
                return;
            }
            if (CDimTools.GetInstance().CompareTwoDoubleValue(partMaxY,mMainBeamMaxY) >= 0)
            {
                if (!CDimTools.GetInstance().IsTwoVectorParallel(normal, new Vector(1, 0, 0)))
                {
                    return;
                }
                if (mLeftTopPart == null)
                {
                    mLeftTopPart = mrPart;
                }
                else if (partMaxY > mLeftTopPart.GetMaxYPoint().Y)
                {
                    mLeftTopPart = mrPart;
                }
            }
        }
        /// <summary>
        /// 判断右侧和右上侧的零部件;
        /// </summary>
        /// <param name="mrPart"></param>
        private void JudgeRightAndRightTopPart(CMrPart mrPart)
        {
            Vector normal = mrPart.mNormal;

            if (mrPart.GetMaxXPoint().X < 0)
            {
                return;
            }

            double partMaxX = mrPart.GetMaxXPoint().X;
            double partMinX = mrPart.GetMinXPoint().X;
            double partMinY = mrPart.GetMinYPoint().Y;
            double partMaxY = mrPart.GetMaxYPoint().Y;

            double partHeight = Math.Abs(partMaxY - partMinY);

            double mainBeamHeight = Math.Abs(mMainBeamMaxY - mMainBeamMinY);

            if (partHeight > mainBeamHeight / 2.0 && Math.Abs(partMinY - mMainBeamMinY) < 0.5)
            {
                if (mRightPart == null)
                {
                    mRightPart = mrPart;
                }
                else if (mrPart.GetMaxXPoint().X > mRightPart.GetMaxXPoint().X)
                {
                    mRightPart = mrPart;
                }

                return;
            }
            if (CDimTools.GetInstance().CompareTwoDoubleValue(partMaxY,mMainBeamMaxY) >= 0)
            {
                if (!CDimTools.GetInstance().IsTwoVectorParallel(normal, new Vector(1, 0, 0)))
                {
                    return;
                }
                if (mRightTopPart == null)
                {
                    mRightTopPart = mrPart;
                }
                else if (partMaxY > mRightTopPart.GetMaxYPoint().Y)
                {
                    mRightTopPart = mrPart;
                }
            }
        }

        /// <summary>
        /// 判断顶部的零部件,顶部的零部件法向可能与Y轴平行,也可能在XY平面内;
        /// </summary>
        /// <param name="?"></param>
        private void JudgeTopPart(CMrPart mrPart)
        {
            Vector normal = mrPart.mNormal;
            double maxY = mrPart.GetMaxYPoint().Y;

            if (maxY < mMainBeamMaxY)
            {
                return;
            }

            //1.如果零部件的法向与Y轴的法向相同;
            if (CDimTools.GetInstance().IsTwoVectorParallel(normal, new Vector(0, 1, 0)))
            {
                if(mTopPart==null)
                {
                    mTopPart = mrPart;
                }
                else if (maxY > mTopPart.GetMaxYPoint().Y)
                {
                    mTopPart = mrPart;
                }
            }
            //2.如果法向在XY平面内;
            else if(CDimTools.GetInstance().IsVectorInXYPlane(normal))
            {
                if (mTopPart == null)
                {
                    mTopPart = mrPart;
                }
                else if (mLeftTopPart != null && mRightTopPart != null)
                {
                    double leftTopMaxY = mLeftTopPart.GetMaxYPoint().Y;
                    double rightTopMaxY = mRightTopPart.GetMaxYPoint().Y;

                    if (maxY > leftTopMaxY && maxY > rightTopMaxY)
                    {
                        return;
                    }
                    else if (maxY > mTopPart.GetMaxYPoint().Y)
                    {
                        mTopPart = mrPart;
                    }
                }
                else if (maxY > mTopPart.GetMaxYPoint().Y)
                {
                    mTopPart = mrPart;
                }
            }
        }

        /// <summary>
        /// 判断左上侧中间的零部件;
        /// </summary>
        /// <param name="mrPart"></param>
        private void JudgeLeftMiddlePart(CMrPart mrPart)
        {
            if(mLeftTopMiddlePart!=null)
            {
                return;
            }

            double partMaxX = mrPart.GetMaxXPoint().X;
            double partMaxY = mrPart.GetMaxYPoint().Y;
            double leftPartMaxY = mLeftPart.GetMaxYPoint().Y;
            double leftTopPartMaxY = mLeftTopPart.GetMaxYPoint().Y;
            double leftTopPartMaxX = mLeftTopPart.GetMaxXPoint().X;

            if (partMaxY > leftPartMaxY && partMaxY < leftTopPartMaxY &&
               CDimTools.GetInstance().CompareTwoDoubleValue(partMaxX, leftTopPartMaxX) == 0)
            {
                mLeftTopMiddlePart = mrPart;
            }
        }

        /// <summary>
        /// 判断右上侧中间的零部件;
        /// </summary>
        /// <param name="mrPart"></param>
        private void JudgeRightMiddlePart(CMrPart mrPart)
        {
            if (mRightTopMiddlePart != null)
            {
                return;
            }

            double partMinX = mrPart.GetMinXPoint().X;
            double partMaxY = mrPart.GetMaxYPoint().Y;
            double rightPartMaxY = mRightPart.GetMaxYPoint().Y;
            double rightTopPartMaxY = mRightTopPart.GetMaxYPoint().Y;
            double rightTopPartMinX = mRightTopPart.GetMinXPoint().X;

            if (partMaxY > rightPartMaxY && partMaxY < rightTopPartMaxY &&
               CDimTools.GetInstance().CompareTwoDoubleValue(partMinX, rightTopPartMinX) == 0)
            {
                mRightTopMiddlePart = mrPart;
            }
        }

        /// <summary>
        /// 清除数据;
        /// </summary>
        private void ClearData()
        {
            mRightDimPartList.Clear();
            mXNormalAloneDimPartList.Clear();
            mYNormalMiddlePartList.Clear();

            mMapYNormalPartToMrApronPlate.Clear();
            mMapZNormalPartToMrApronPlate.Clear();
           
            mYNormalBottomPart = null;
            mZNormalBottomPart = null;
            mLeftPart = null;
            mLeftTopMiddlePart = null;
            mLeftTopPart = null;
            mRightPart = null;
            mRightTopMiddlePart = null;
            mRightTopPart = null;
            mTopPart = null;

        }

        /// <summary>
        /// 得到与给定点最相近的零部件;
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public CMrPart GetMostNearPart(Point point)
        {
            CMrPart mNearPart = null;

            double dblNearDistance = double.MaxValue;

            foreach (CMrPart mrPart in mRightDimPartList)
            {
                Point pt1 = mrPart.GetMinXMaxYPoint();
                Point pt2 = mrPart.GetMaxXMaxYPoint();

                double distance = CDimTools.GetInstance().ComputePointToLineDistance(point, pt1, pt2);

                if (distance < dblNearDistance)
                {
                    mNearPart = mrPart;

                    dblNearDistance = distance;
                }
            }

            return mNearPart;
        }

        /// <summary>
        /// 根据给定的零件查找檩托板;
        /// </summary>
        /// <param name="mrPart"></param>
        /// <returns></returns>
        public CMrApronPlate FindMrApronPlateByYNormalPart(CMrPart mrPart)
        {
            if (mMapYNormalPartToMrApronPlate.ContainsKey(mrPart))
            {
                return mMapYNormalPartToMrApronPlate[mrPart];
            }
            return null;
        }

        /// <summary>
        /// 根据给定的零件查找檩托板;
        /// </summary>
        /// <param name="mrPart"></param>
        /// <returns></returns>
        public CMrApronPlate FindMrApronPlateByZNormalPart(CMrPart mrPart)
        {
            if (mMapZNormalPartToMrApronPlate.ContainsKey(mrPart))
            {
                return mMapZNormalPartToMrApronPlate[mrPart];
            }
            return null;
        }

        /// <summary>
        /// 根据给定的零件查找檩托板;
        /// </summary>
        /// <param name="mrPart"></param>
        /// <returns></returns>
        public CMrApronPlate FindMrApronPlate2ByYNormalPart(CMrPart mrPart)
        {
            if (mMapYNormalPartToMrApronPlate2.ContainsKey(mrPart))
            {
                return mMapYNormalPartToMrApronPlate2[mrPart];
            }

            return null;
        }

        /// <summary>
        /// 构建类型1的檩托板;
        /// </summary>
        public void BuildMrApronPlateType1(List<CMrPart> mrPartList)
        {
            CDimTools.GetInstance().SortMrPartByMinY(mrPartList);

            Vector yVector = new Vector(0, 1, 0);

            foreach (CMrPart mrPart in mrPartList)
            {
                Vector normal = mrPart.mNormal;

                if (!CDimTools.GetInstance().IsTwoVectorParallel(normal, yVector))
                {
                    continue;
                }

                CMrApronPlate mrApronPlate = CreateMrApronPlateType1(mrPart,mrPartList);

                if(mrApronPlate==null)
                {
                    continue;
                }

                CMrPart zNormalPart=mrApronPlate.mZNormalPart;

                if(!mMapYNormalPartToMrApronPlate.ContainsKey(mrPart))
                {
                    mMapYNormalPartToMrApronPlate.Add(mrPart, mrApronPlate);
                }
                if(!mMapZNormalPartToMrApronPlate.ContainsKey(zNormalPart))
                {
                    mMapZNormalPartToMrApronPlate.Add(zNormalPart,mrApronPlate);
                }
            }
        }

        /// <summary>
        /// 根据给定的零部件找到上板或者下板;
        /// </summary>
        /// <param name="mrPart"></param>
        /// <param name="mrPartList"></param>
        /// <returns></returns>
        public CMrApronPlate CreateMrApronPlateType1(CMrPart myNormalPart, List<CMrPart> mrPartList)
        {
            Vector zVector = new Vector(0, 0, 1);

            double minY = myNormalPart.GetMinYPoint().Y;
            double maxY = myNormalPart.GetMaxYPoint().Y; 
            double minX = myNormalPart.GetMinXPoint().X;
            double maxX = myNormalPart.GetMaxXPoint().X;
   
            foreach (CMrPart mrPart in mrPartList)
            {
                Vector normal = mrPart.mNormal;
   
                if (!CDimTools.GetInstance().IsTwoVectorParallel(normal, zVector))
                {
                    continue;
                }

                double zNormalMaxY = mrPart.GetMaxYPoint().Y;
                double zNormalMinY = mrPart.GetMinYPoint().Y;
                double zNormalMaxX = mrPart.GetMaxXPoint().X;
                double zNormalMinX = mrPart.GetMinXPoint().X;

                if (CDimTools.GetInstance().CompareTwoDoubleValue(zNormalMaxY, minY) == 0 &&
                    CDimTools.GetInstance().CompareTwoDoubleValue(zNormalMaxX, maxX) == 0 &&
                    CDimTools.GetInstance().CompareTwoDoubleValue(zNormalMinX, minX) == 0 )
                {
                    CMrApronPlate mrApronPlate = new CMrApronPlate(myNormalPart, mrPart, MrApronPlateType.Type1);
                    mrApronPlate.mIsUp = false;
                    return mrApronPlate;
                }
                else if (CDimTools.GetInstance().CompareTwoDoubleValue(zNormalMinY, maxY) == 0 &&
                    CDimTools.GetInstance().CompareTwoDoubleValue(zNormalMaxX, maxX) == 0 &&
                    CDimTools.GetInstance().CompareTwoDoubleValue(zNormalMinX, minX) == 0)
                {
                    CMrApronPlate mrApronPlate = new CMrApronPlate(myNormalPart, mrPart, MrApronPlateType.Type1);
                    mrApronPlate.mIsUp = true;
                    return mrApronPlate;
                }
            }
            return null;
        }

        /// <summary>
        /// 构建类型2的檩托板;
        /// </summary>
        /// <param name="mrPartList"></param>
        public void BuildMrApronPlateType2(List<CMrPart> mrPartList)
        {
            CDimTools.GetInstance().SortMrPartByMinY(mrPartList);

            double mainBeamMinX = CMrMainBeam.GetInstance().GetMinXPoint().X;
            double mainBeamMaxX = CMrMainBeam.GetInstance().GetMaxXPoint().X;

            Vector yVector = new Vector(0, 1, 0);

            foreach (CMrPart mrPart in mrPartList)
            {
                Vector normal = mrPart.mNormal;

                if (!CDimTools.GetInstance().IsTwoVectorParallel(normal, yVector))
                {
                    continue;
                }

                double partMinX = mrPart.GetMinXPoint().X;
                double partMaxX = mrPart.GetMaxXPoint().X;

                if(!(CDimTools.GetInstance().CompareTwoDoubleValue(partMinX, mainBeamMinX) > 0 &&
                   CDimTools.GetInstance().CompareTwoDoubleValue(partMaxX, mainBeamMaxX) < 0))
                {
                    continue;
                }

                CMrApronPlate mrApronPlate = CreateMrApronPlateType2(mrPart, mrPartList);

                if (mrApronPlate == null)
                {
                    continue;
                }

                if (!mMapYNormalPartToMrApronPlate2.ContainsKey(mrPart))
                {
                    mMapYNormalPartToMrApronPlate2.Add(mrPart, mrApronPlate);
                }
            }
        }

        /// <summary>
        /// 创建类型2的檩托板;
        /// </summary>
        /// <param name="myNormalPart"></param>
        /// <param name="mrPartList"></param>
        /// <returns></returns>
        public CMrApronPlate CreateMrApronPlateType2(CMrPart myNormalPart, List<CMrPart> mrPartList)
        {
            Vector xVector = new Vector(1, 0, 0);

            double minY = myNormalPart.GetMinYPoint().Y;
            double maxY = myNormalPart.GetMaxYPoint().Y;
            double minX = myNormalPart.GetMinXPoint().X;
            double maxX = myNormalPart.GetMaxXPoint().X;

            foreach (CMrPart mrPart in mrPartList)
            {
                Vector normal = mrPart.mNormal;

                if (!CDimTools.GetInstance().IsTwoVectorParallel(normal, xVector))
                {
                    continue;
                }

                double xNormalMaxY = mrPart.GetMaxYPoint().Y;
                double xNormalMinY = mrPart.GetMinYPoint().Y;
                double xNormalMaxX = mrPart.GetMaxXPoint().X;
                double xNormalMinX = mrPart.GetMinXPoint().X;

                if (CDimTools.GetInstance().CompareTwoDoubleValue(xNormalMaxY, minY) == 0 ||
                    CDimTools.GetInstance().CompareTwoDoubleValue(xNormalMinY, maxY) == 0)
                {
                    CMrApronPlate mrApronPlate = new CMrApronPlate(mrPart, myNormalPart, MrApronPlateType.Type2);
                    return mrApronPlate;
                }
            }
            return null;
        }

        /// <summary>
        /// 得到类型1的第一块檩托板;
        /// </summary>
        /// <returns></returns>
        public CMrApronPlate GetFirstMrApronPlateType1()
        {
            if (mMapYNormalPartToMrApronPlate.Count == 0)
            {
                return null;
            }

            foreach (CMrPart mrPart in mMapYNormalPartToMrApronPlate.Keys)
            {
                return mMapYNormalPartToMrApronPlate[mrPart];
            }

            return null;
        }

        /// <summary>
        /// 得到类型2的第一块檩托板;
        /// </summary>
        /// <returns></returns>
        public CMrApronPlate GetFirstMrApronPlateType2()
        {
            if (mMapYNormalPartToMrApronPlate2.Count == 0)
            {
                return null;
            }

            foreach (CMrPart mrPart in mMapYNormalPartToMrApronPlate2.Keys)
            {
                return mMapYNormalPartToMrApronPlate2[mrPart];
            }

            return null;
        }
    }
}
