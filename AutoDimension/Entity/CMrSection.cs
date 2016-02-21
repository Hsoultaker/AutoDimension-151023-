using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tekla.Structures.Geometry3d;

namespace AutoDimension.Entity
{
    /// <summary>
    /// MrRender自定义的剖面对象;
    /// </summary>
    public class CMrSection
    {
        /// <summary>
        /// 剖面标记的索引;
        /// </summary>
        public string mSectionMark = null;

        /// <summary>
        /// 剖面的类型;
        /// </summary>
        public MrSectionType mSectionType;

        /// <summary>
        /// 剖面的方式;
        /// </summary>
        public MrSectionMode mSectionMode = MrSectionMode.None;

        /// <summary>
        /// 当前剖面中主要针对那些零部件进行剖视;
        /// </summary>
        public List<CMrPart> mSectionPartList = new List<CMrPart>();

        /// <summary>
        /// 剖面剖视时起始的X值;
        /// </summary>
        public double mSectionMidX = 0.0;

        /// <summary>
        /// 剖面剖视时起始的Y值;
        /// </summary>
        public double mSectionMidY = 0.0;

        /// <summary>
        /// 剖面剖视时最小的X值;
        /// </summary>
        public double mSectionMinX = 0.0;

        /// <summary>
        /// 剖面剖视时最大的X值;
        /// </summary>
        public double mSectionMaxX = 0.0;

        /// <summary>
        /// 剖面的默认的最大Y值,该值可能会被修改;
        /// </summary>
        public double mSectionMaxY = 0.0;

        /// <summary>
        /// 剖面的默认的最小Y值,该值可能会被修改;
        /// </summary>
        public double mSectionMinY = 0.0;

        /// <summary>
        /// 构造函数;
        /// </summary>
        /// <param name="sectionType"></param>
        public CMrSection(MrSectionType sectionType)
        {
            this.mSectionType = sectionType;
        }

        /// <summary>
        /// 增加零部件到该剖面中;
        /// </summary>
        /// <param name="mrPart"></param>
        public void AppendSectionPart(CMrPart mrPart)
        {
            if (mSectionPartList.Contains(mrPart))
            {
                return;
            }
            mSectionPartList.Add(mrPart);
        }

        /// <summary>
        /// 获得剖面中零部件的链表;
        /// </summary>
        /// <returns></returns>
        public List<CMrPart> GetMrPartList()
        {
            return mSectionPartList;
        }

        /// <summary>
        /// 判断该剖面中是否已经包含了该零件;
        /// </summary>
        /// <param name="mrPart"></param>
        public bool IsHaveThePart(CMrPart mrPart)
        {
            if (mSectionPartList.Contains(mrPart))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 获取剖面中的所有零件的最大和最小Y值;
        /// </summary>
        /// <param name="minY"></param>
        /// <param name="maxY"></param>
        public void GetSectionMinYAndMaxY(ref double minY,ref double  maxY)
        {
            minY = double.MaxValue;
            maxY = double.MinValue;

            foreach (CMrPart mrPart in mSectionPartList)
            {
                double partMinY = mrPart.GetMinYPoint().Y;
                double partMaxY = mrPart.GetMaxYPoint().Y;

                if (partMaxY > maxY)
                {
                    maxY = partMaxY;
                }
                if (partMinY < minY)
                {
                    minY = partMinY;
                }
            }
        }

        /// <summary>
        /// 获得最大的Y值点和最小的Y值点;
        /// </summary>
        /// <param name="minYPt"></param>
        /// <param name="maxYPt"></param>
        public void GetSectionMinYAndMaxYPoint(ref Point minYPt, ref Point maxYPt)
        {
            double minY = double.MaxValue;
            double maxY = double.MinValue;

            foreach (CMrPart mrPart in mSectionPartList)
            {
                Point maxYPoint = mrPart.GetMaxYPoint();
                Point minYPoint = mrPart.GetMinYPoint();

                double partMaxY = maxYPoint.Y;
                double partMinY = minYPoint.Y;

                if (partMaxY > maxY)
                {
                    maxYPt = maxYPoint;
                    maxY = partMaxY;
                }
                if (partMinY < minY)
                {
                    minYPt = minYPoint;
                    minY = partMinY;
                }
            }
        }

        /// <summary>
        /// 获得剖面中所有零部件的最大与最小的X值;
        /// </summary>
        /// <param name="minX"></param>
        /// <param name="maxX"></param>
        public void GetSectionMinXAndMaxX(ref double minX, ref double maxX)
        {
            minX = double.MaxValue;
            maxX = double.MinValue;

            foreach (CMrPart mrPart in mSectionPartList)
            {
                double partMinX = mrPart.GetMinXPoint().X;
                double partMaxX = mrPart.GetMaxXPoint().X;

                if (partMaxX > maxX)
                {
                    maxX = partMaxX;
                }
                if (partMinX < minX)
                {
                    minX = partMinX;
                }
            }
        }

        /// <summary>
        /// 获得剖面中所有零部件的最大与最小的X值点;
        /// </summary>
        /// <param name="minX"></param>
        /// <param name="maxX"></param>
        public void GetSectionMinXAndMaxXPoint(ref Point minXPt, ref Point maxXPt)
        {
            double minX = double.MaxValue;
            double maxX = double.MinValue;

            foreach (CMrPart mrPart in mSectionPartList)
            {
                Point minXPoint = mrPart.GetMinXPoint();
                Point maxXPoint = mrPart.GetMaxXPoint();

                double partMinX = minXPoint.X;
                double partMaxX = maxXPoint.X;

                if (partMaxX > maxX)
                {
                    maxXPt = maxXPoint;
                    maxX = partMaxX;
                }
                if (partMinX < minX)
                {
                    minXPt = minXPoint;
                    minX = partMinX;
                }
            }
        }

        /// <summary>
        /// 判断两个剖面是否相同;
        /// </summary>
        /// <param name="mrSection"></param>
        /// <returns></returns>
        public bool IsSameSection(CMrSection mrSection)
        {
            if (mSectionType == MrSectionType.MrSectionBeam)
            {
                List<CMrPart> mrPartList = mrSection.GetMrPartList();

                if (mrPartList.Count != mSectionPartList.Count || mrSection.mSectionType != mSectionType)
                {
                    return false;
                }
                foreach (CMrPart mrPart in mrPartList)
                {
                    bool bRes = IsThePartSameWithSectionParts(mrPart);

                    if (!bRes)
                    {
                        return false;
                    }
                }
            }
            else if (mSectionType == MrSectionType.MrSectionCylinder)
            {
                List<CMrPart> mrPartList = mrSection.GetMrPartList();

                if (mrPartList.Count != mSectionPartList.Count || mrSection.mSectionType != mSectionType)
                {
                    return false;
                }
                foreach (CMrPart mrPart in mrPartList)
                {
                    bool bRes = IsThePartSameWithSectionParts(mrPart);

                    if (!bRes)
                    {
                        return false;
                    }
                }
            }
            else if(mSectionType==MrSectionType.MrSectionBeamDoor)
            {
                List<CMrPart> mrPartList = mrSection.GetMrPartList();

                if (mrPartList.Count != mSectionPartList.Count || mrSection.mSectionType != mSectionType)
                {
                    return false;
                }
                foreach (CMrPart mrPart in mrPartList)
                {
                    bool bRes = IsThePartSameWithSectionParts(mrPart);

                    if (!bRes)
                    {
                        return false;
                    }
                }
            }
            else if (mSectionType == MrSectionType.MrSectionCylinderDoor)
            {
                List<CMrPart> mrPartList = mrSection.GetMrPartList();

                if (mrPartList.Count != mSectionPartList.Count || mrSection.mSectionType != mSectionType)
                {
                    return false;
                }
                foreach (CMrPart mrPart in mrPartList)
                {
                    bool bRes = IsThePartSameWithSectionParts(mrPart);

                    if (!bRes)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// 判断两个剖面不同是否因为牛腿下方存在水平的板;
        /// </summary>
        /// <param name="mrSection"></param>
        /// <returns></returns>
        private bool IsHaveThePlatePart(CMrSection mrSection)
        {
            List<CMrPart> tempPartList = new List<CMrPart>();

            int nCount1 = mrSection.mSectionPartList.Count;
            int nCount2 = mSectionPartList.Count;

            if (nCount1 == nCount2 + 1)
            {
                foreach (CMrPart mrPart in mrSection.GetMrPartList())
                {
                    if (!CDimTools.GetInstance().IsTwoVectorParallel(mrPart.mNormal, new Vector(0, 0, 1)))
                    {
                        continue;
                    }
                    foreach (CMrPart mrSectionPart in mSectionPartList)
                    {
                        if (!IsTwoCylinderPartSame(mrPart, mrSectionPart))
                        {
                            tempPartList.Add(mrPart);
                        }
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// 是否该零件与剖面中的零部件相同;
        /// </summary>
        /// <param name="mrPart"></param>
        /// <returns></returns>
        private bool IsThePartSameWithSectionParts(CMrPart mrPart)
        {
            foreach(CMrPart mSectionPart in mSectionPartList)
            {
                if (mSectionType == MrSectionType.MrSectionBeam)
                {
                    bool bRes = IsTwoBeamPartSame(mSectionPart, mrPart);

                    if (bRes)
                    {
                        return true;
                    }
                }
                else if (mSectionType == MrSectionType.MrSectionCylinder)
                {
                    bool bRes = IsTwoCylinderPartSame(mSectionPart, mrPart);

                    if (bRes)
                    {
                        return true;
                    }
                }
                else if (mSectionType == MrSectionType.MrSectionBeamDoor)
                {
                    bool bRes = IsTwoCylinderPartSame(mSectionPart, mrPart);

                    if (bRes)
                    {
                        return true;
                    }
                }
                else if (mSectionType == MrSectionType.MrSectionCylinderDoor)
                {
                    bool bRes = IsTwoCylinderPartSame(mSectionPart, mrPart);

                    if (bRes)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// 判断框架梁剖面中的两个零件是否相同;
        /// </summary>
        /// <param name="mrPart"></param>
        /// <returns></returns>
        private bool IsTwoBeamPartSame(CMrPart mrSrcPart,CMrPart mrDesPart)
        {
            Vector srcVector = mrSrcPart.mNormal;
            Vector desVector = mrDesPart.mNormal;

            string strSrcPartMark = mrSrcPart.mPartInModel.GetPartMark();
            string strDesPartMark = mrDesPart.mPartInModel.GetPartMark();

            if (!CDimTools.GetInstance().IsTwoVectorParallel(desVector, srcVector))
            {
                return false;
            }

            if (!strSrcPartMark.Equals(strDesPartMark))
            {
                return false;
            }

            if (mrSrcPart.IsHaveBolt() != mrDesPart.IsHaveBolt())
            {
                return false;
            }

            List<CMrBoltArray> mrSrcBoltArray = mrSrcPart.GetBoltArrayList();
            List<CMrBoltArray> mrDesBoltArray = mrDesPart.GetBoltArrayList();

            if (mrSrcBoltArray.Count != mrDesBoltArray.Count)
            {
                return false;
            }

            List<Point> mrSrcBoltPointList = new List<Point>();
            List<Point> mrDesBoltPointList = new List<Point>();

            foreach(CMrBoltArray mrBoltArray in mrSrcBoltArray)
            {
                mrSrcBoltPointList.AddRange(mrBoltArray.GetBoltPointList());
            }

            foreach (CMrBoltArray mrBoltArray in mrDesBoltArray)
            {
                mrDesBoltPointList.AddRange(mrBoltArray.GetBoltPointList());
            }

            if (mrSrcBoltPointList.Count != mrDesBoltPointList.Count)
            {
                return false;
            }

            int nCount = mrSrcBoltPointList.Count;

            if (CDimTools.GetInstance().IsTwoVectorParallel(srcVector, new Vector(1, 0, 0))||
                CDimTools.GetInstance().IsTwoVectorParallel(srcVector, new Vector(0, 1, 0)))
            {
                for (int i = 0; i < nCount; i++)
                {
                    CDimTools.GetInstance().GetMaxYPoint(mrDesBoltPointList);
                    CDimTools.GetInstance().GetMaxYPoint(mrSrcBoltPointList);

                    if (Math.Abs(mrDesBoltPointList[i].Y - mrSrcBoltPointList[i].Y) > 3)
                    {
                        return false;
                    }

                    CDimTools.GetInstance().GetMaxZPoint(mrDesBoltPointList);
                    CDimTools.GetInstance().GetMaxZPoint(mrSrcBoltPointList);

                    if (Math.Abs(mrDesBoltPointList[i].Z - mrSrcBoltPointList[i].Z) > 3)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// 判断框架柱剖面中的两个零件是否相同;
        /// </summary>
        /// <param name="mrSrcPart"></param>
        /// <param name="mrDesPart"></param>
        /// <returns></returns>
        private bool IsTwoCylinderPartSame(CMrPart mrSrcPart, CMrPart mrDesPart)
        {
            Vector srcVector = mrSrcPart.mNormal;
            Vector desVector = mrDesPart.mNormal;

            string strSrcPartMark = mrSrcPart.mPartInModel.GetPartMark();
            string strDesPartMark = mrDesPart.mPartInModel.GetPartMark();

            if (!CDimTools.GetInstance().IsTwoVectorParallel(desVector, srcVector))
            {
                return false;
            }

            if (!strSrcPartMark.Equals(strDesPartMark))
            {
                return false;
            }

            if (mrSrcPart.IsHaveBolt() != mrDesPart.IsHaveBolt())
            {
                return false;
            }

            List<CMrBoltArray> mrSrcBoltArray = mrSrcPart.GetBoltArrayList();
            List<CMrBoltArray> mrDesBoltArray = mrDesPart.GetBoltArrayList();

            if (mrSrcBoltArray.Count != mrDesBoltArray.Count)
            {
                return false;
            }

            List<Point> mrSrcBoltPointList = new List<Point>();
            List<Point> mrDesBoltPointList = new List<Point>();

            foreach (CMrBoltArray mrBoltArray in mrSrcBoltArray)
            {
                mrSrcBoltPointList.AddRange(mrBoltArray.GetBoltPointList());
            }

            foreach (CMrBoltArray mrBoltArray in mrDesBoltArray)
            {
                mrDesBoltPointList.AddRange(mrBoltArray.GetBoltPointList());
            }

            if (mrSrcBoltPointList.Count != mrDesBoltPointList.Count)
            {
                return false;
            }
            return true;
        }
    }
}
