using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tekla.Structures.Geometry3d;

namespace AutoDimension.Entity
{
    /// <summary>
    /// 门式框架结构顶视图管理器;
    /// </summary>
    public class CMrCylinderDoorTopManager
    {
        /// <summary>
        /// 单一实例;
        /// </summary>
        private static CMrCylinderDoorTopManager mInstance = null;

        /// <summary>
        /// 顶视图中在主梁下面法向与Y轴相同的零部件;
        /// </summary>
        public CMrPart mYNormalBottomPart = null;

        /// <summary>
        /// 顶视图中在主梁上面法向与Y轴相同的零部件;
        /// </summary>
        public CMrPart mTopPart = null;

        /// <summary>
        /// 构建在主梁外围法向与Y轴平行的零件与檩托板的映射表;
        /// </summary>
        public Dictionary<CMrPart, CMrApronPlate> mMapYNormalPartToMrApronPlate = new Dictionary<CMrPart, CMrApronPlate>();

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
        public static CMrCylinderDoorTopManager GetInstance()
        {
            if (null == mInstance)
            {
                mInstance = new CMrCylinderDoorTopManager();
            }

            return mInstance;
        }

        /// <summary>
        /// 构建门式框架结构顶视图中的拓扑结构;
        /// </summary>
        /// <param name="mrPartList"></param>
        public void BuildCylinderDoorTopTopo(List<CMrPart> mrPartList)
        {
            ClearData();

            mMainBeamMinX = CMrMainBeam.GetInstance().GetMinXPoint().X;
            mMainBeamMaxX = CMrMainBeam.GetInstance().GetMaxXPoint().X;
            mMainBeamMinY = CMrMainBeam.GetInstance().GetMinYPoint().Y;
            mMainBeamMaxY = CMrMainBeam.GetInstance().GetMaxYPoint().Y;

            //判断零部件;
            foreach (CMrPart mrPart in mrPartList)
            {
                JudgeYNormalBottomPart(mrPart);
                JudgeTopPart(mrPart);
            }

            //构建檩托板的映射表;
            BuildMrApronPlate(mrPartList);
        }

        /// <summary>
        /// 清除数据;
        /// </summary>
        private void ClearData()
        {
            mMapYNormalPartToMrApronPlate.Clear();
         
            mYNormalBottomPart = null;
            mTopPart = null;

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

            if (Math.Abs(partMaxY - mMainBeamMinY) < 0.1)
            {
                mYNormalBottomPart = mrPart;
            }
        }

        /// <summary>
        /// 判断顶部的零部件,顶部的零部件法向与Y轴平行;
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
                if (Math.Abs(maxY - CCommonPara.mViewMaxY) < CCommonPara.mDblError)
                {
                    mTopPart = mrPart;
                }
            }
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
        /// 构建檩托板;
        /// </summary>
        public void BuildMrApronPlate(List<CMrPart> mrPartList)
        {
            Vector yVector = new Vector(0, 1, 0);

            foreach (CMrPart mrPart in mrPartList)
            {
                Vector normal = mrPart.mNormal;

                if (!CDimTools.GetInstance().IsTwoVectorParallel(normal, yVector))
                {
                    continue;
                }

                CMrApronPlate mrApronPlate = CreateMrApronPlate(mrPart, mrPartList);

                if (mrApronPlate == null)
                {
                    continue;
                }
                CMrPart zNormalPart = mrApronPlate.mZNormalPart;

                if (!mMapYNormalPartToMrApronPlate.ContainsKey(mrPart))
                {
                    mMapYNormalPartToMrApronPlate.Add(mrPart, mrApronPlate);
                }
            }
        }

        /// <summary>
        /// 根据给定的零部件找到上板或者下板;
        /// </summary>
        /// <param name="mrPart"></param>
        /// <param name="mrPartList"></param>
        /// <returns></returns>
        public CMrApronPlate CreateMrApronPlate(CMrPart myNormalPart, List<CMrPart> mrPartList)
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
                double zNormalMinX = mrPart.GetMinXPoint().X;
                double zNormalMaxX = mrPart.GetMaxXPoint().X;

                if (CDimTools.GetInstance().CompareTwoDoubleValue(zNormalMaxY, minY) == 0 &&
                    Math.Abs(zNormalMinX - minX) < 5 && Math.Abs(zNormalMaxX - maxX) < 5)
                {
                    CMrApronPlate mrApronPlate = new CMrApronPlate(myNormalPart, mrPart,MrApronPlateType.Type1);

                    mrApronPlate.mIsUp = false;

                    return mrApronPlate;
                }
                else if (CDimTools.GetInstance().CompareTwoDoubleValue(zNormalMinY, maxY) == 0 &&
                         Math.Abs(zNormalMinX-minX) < 5 && Math.Abs(zNormalMaxX-maxX) < 5)
                {
                    CMrApronPlate mrApronPlate = new CMrApronPlate(myNormalPart, mrPart, MrApronPlateType.Type1);

                    mrApronPlate.mIsUp = true;

                    return mrApronPlate;
                }
            }
            return null;
        }
    }
}
