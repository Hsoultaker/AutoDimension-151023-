using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tekla.Structures.Model;
using Tekla.Structures.Geometry3d;

using TSD=Tekla.Structures.Drawing;
using TSM=Tekla.Structures.Model;

namespace AutoDimension.Entity
{
    /// <summary>
    /// 部件对象;
    /// </summary>
    public class CMrPart :CMrEntity
    {
        /// <summary>
        /// tekla中的Part对象;Model空间中;
        /// </summary>
        public TSM.Part mPartInModel;

        /// <summary>
        /// tekla中的Part对象;Drawing空间中;
        /// </summary>
        public TSD.Part mPartInDrawing;

        /// <summary>
        /// 部件在Drawing中所有点的链表;
        /// </summary>
        private List<Point> mPointList;

        /// <summary>
        /// 零件的最大点;
        /// </summary>
        public Point mMaxPoint;

        /// <summary>
        /// 零件的最小点;
        /// </summary>
        public Point mMinPoint;

        /// <summary>
        /// 左上角;
        /// </summary>
        public Point mLeftTopPoint { get; set; }

        /// <summary>
        /// 左下角;
        /// </summary>
        public Point mLeftBottomPoint { get; set; }

        /// <summary>
        /// 右下角;
        /// </summary>
        public Point mRightBottomPoint { get; set; }

        /// <summary>
        /// 右上角;
        /// </summary>
        public Point mRightTopPoint { get; set; }

        /// <summary>
        /// 零件的中心点;
        /// </summary>
        public Point mMidPoint { get; set; }

        /// 零件中心线中点的链表;
        /// </summary>
        public List<Point> mCenterLinePointList = null;

        /// <summary>
        /// 宽度,与建模坐标系有关;
        /// </summary>
        public double mWidth { get; set; }

        /// <summary>
        /// 高度,与建模坐标系有关;
        /// </summary>
        public double mHeight { get; set; }

        /// <summary>
        /// 面的法矢,是梁厚度方向的法矢;
        /// </summary>
        public Vector mNormal { get; set; }

        /// <summary>
        /// 梁的类型;
        /// </summary>
        public MrBeamType mBeamType { get; set; }

        /// <summary>
        /// 标志该零件是否需要增加零件标记;
        /// </summary>
        private bool mbNeedAddMark;

        /// <summary>
        /// Part中关联的MrBoltArray对象;
        /// </summary>
        private List<CMrBoltArray> mBoltArrayList = new List<CMrBoltArray>();

        /// <summary>
        /// 梁在顶视图中的信息;
        /// </summary>
        private CBeamTopViewInfo mBeamTopViewInfo = null;

        /// <summary>
        /// 梁在前视图中的信息;
        /// </summary>
        private CBeamFrontViewInfo mBeamFrontViewInfo = null;

        /// <summary>
        /// 柱在顶视图中的信息;
        /// </summary>
        private CCylinderTopViewInfo mCylinderTopViewInfo = null;

        /// <summary>
        /// 柱在前视图中的信息;
        /// </summary>
        private CCylinderFrontViewInfo mCylinderFrontViewInfo = null;

        /// <summary>
        /// 门式框架结构梁在顶视图中的信息;
        /// </summary>
        private CBeamDoorFrontViewInfo mBeamDoorFrontViewInfo = null;

        /// <summary>
        /// 门式框架结构柱在顶视图中的信息;
        /// </summary>
        private CCylinderDoorFrontViewInfo mCylinderDoorFrontViewInfo = null;

        /// <summary>
        /// 门式框架结构柱在顶视图中的信息;
        /// </summary>
        private CCylinderDoorTopViewInfo mCylinderDoorTopViewInfo = null;

        /// <summary>
        /// 构造函数;
        /// </summary>
        /// <param name="partInModel"></param>
        /// <param name="partInDrawing"></param>
        public CMrPart(TSM.Part partInModel,TSD.Part partInDrawing)
        {
            mbNeedAddMark = false;
            mPartInModel = partInModel;
            mPartInDrawing = partInDrawing;
            mBeamType = MrBeamType.BEAM;
            
            mBeamTopViewInfo = new CBeamTopViewInfo(this);
            mBeamFrontViewInfo = new CBeamFrontViewInfo(this);
            mCylinderTopViewInfo = new CCylinderTopViewInfo(this);
            mCylinderFrontViewInfo = new CCylinderFrontViewInfo(this);

            mBeamDoorFrontViewInfo = new CBeamDoorFrontViewInfo(this);
            mCylinderDoorFrontViewInfo = new CCylinderDoorFrontViewInfo(this);
            mCylinderDoorTopViewInfo = new CCylinderDoorTopViewInfo(this);

            mLeftBottomPoint = new Point();
            mLeftTopPoint = new Point();
            mRightBottomPoint = new Point();
            mRightTopPoint = new Point();
            mMidPoint = new Point();
        }

        /// <summary>
        /// 设置梁中点的数据;
        /// </summary>
        /// <param name="pointList"></param>
        public void SetPointList(List<Point> pointList)
        {
            mPointList = pointList;
        }

        /// <summary>
        /// 获取零件中的点的链表;
        /// </summary>
        /// <returns></returns>
        public List<Point> GetPointList()
        {
            return mPointList;
        }

        /// <summary>
        /// 设置标识;
        /// </summary>
        /// <param name="bValue"></param>
        /// <returns></returns>
        public void SetNeedAddMarkFlag(bool bValue)
        {
            mbNeedAddMark = bValue;
        }

        /// <summary>
        /// 是否需要增加标记;
        /// </summary>
        /// <returns></returns>
        public bool IsNeedAddMark()
        {
            return mbNeedAddMark;
        }

        /// <summary>
        /// 添加螺栓组;
        /// </summary>
        /// <param name="mrBolt"></param>
        public void AppendMrBoltArray(CMrBoltArray mrBoltArray)
        {
            mBoltArrayList.Add(mrBoltArray);
        }

        /// <summary>
        /// 获取螺钉数组的链表;
        /// </summary>
        /// <returns></returns>
        public List<CMrBoltArray> GetBoltArrayList()
        {
            return mBoltArrayList;
        }

        /// <summary>
        /// 获取梁顶视图的信息;
        /// </summary>
        /// <returns></returns>
        public CBeamTopViewInfo GetBeamTopViewInfo()
        {
            return mBeamTopViewInfo;
        }

        /// <summary>
        /// 获取梁前视图的信息;
        /// </summary>
        /// <returns></returns>
        public CBeamFrontViewInfo GetBeamFrontViewInfo()
        {
            return mBeamFrontViewInfo;
        }

        /// <summary>
        /// 获取柱顶视图的信息;
        /// </summary>
        /// <returns></returns>
        public CCylinderTopViewInfo GetCylinderTopViewInfo()
        {
            return mCylinderTopViewInfo;
        }

        /// <summary>
        /// 获取柱前视图的信息;
        /// </summary>
        /// <returns></returns>
        public CCylinderFrontViewInfo GetCylinderFrontViewInfo()
        {
            return mCylinderFrontViewInfo;
        }

        /// <summary>
        /// 获取门式框架梁前视图信息;
        /// </summary>
        /// <returns></returns>
        public CBeamDoorFrontViewInfo GetBeamDoorFrontViewInfo()
        {
            return mBeamDoorFrontViewInfo;
        }

        /// <summary>
        /// 获取门式框架柱前视图信息;
        /// </summary>
        /// <returns></returns>
        public CCylinderDoorFrontViewInfo GetCylinderDoorFrontViewInfo()
        {
            return mCylinderDoorFrontViewInfo;
        }

        /// <summary>
        /// 获取门式框架柱顶视图信息;
        /// </summary>
        /// <returns></returns>
        public CCylinderDoorTopViewInfo GetCylinderDoorTopViewInfo()
        {
            return mCylinderDoorTopViewInfo;
        }

        /// <summary>
        /// 设置零件的最大点和最小点;
        /// </summary>
        /// <param name="minPt"></param>
        /// <param name="maxPt"></param>
        public void SetMinAndMaxPoint(Point minPt,Point maxPt)
        {
            mMinPoint = minPt;
            mMaxPoint = maxPt;

            if(mMaxPoint.X < mMinPoint.X)
            {
                Point tempPoint = mMaxPoint;
                mMaxPoint = mMinPoint;
                mMinPoint = tempPoint;
            }

            mMidPoint.X = (mMinPoint.X + mMaxPoint.X) / 2.0;
            mMidPoint.Y = (mMinPoint.Y + mMaxPoint.Y) / 2.0;
            mMidPoint.Z = 0;

            ComputeBoxPoint();
        }

        /// <summary>
        /// 计算零件包围盒的四个点;
        /// </summary>
        private void ComputeBoxPoint()
        {
            int nCount=mPointList.Count;

            if (nCount == 0)
            {
                return;
            }

            //首先计算出最大X值和最小X值的点以及最小Y值与最大Y值点;
            Comparison<Point> sorterX = new Comparison<Point>(CDimTools.ComparePointX);
            mPointList.Sort(sorterX);
            Point minXPoint = mPointList[0];
            Point maxXPoint = mPointList[nCount - 1];

            Comparison<Point> sorterY = new Comparison<Point>(CDimTools.ComparePointY);
            mPointList.Sort(sorterY);
            Point minYPoint = mPointList[0];
            Point maxYPoint = mPointList[nCount - 1];

            double minX = minXPoint.X;
            double maxX = maxXPoint.X;
            double minY = minYPoint.Y;
            double maxY = maxYPoint.Y;

            mLeftBottomPoint.X = minX;
            mLeftBottomPoint.Y=int.MaxValue;

            mLeftTopPoint.X = minX;
            mLeftTopPoint.Y=int.MinValue;
                
            mRightBottomPoint.X = maxX;
            mRightBottomPoint.Y=int.MaxValue;
                
            mRightTopPoint.X = maxX;
            mRightTopPoint.Y=int.MinValue;

            foreach (Point point in mPointList)
            {
                if (Math.Abs(point.X - mLeftBottomPoint.X) < CCommonPara.mDblError && point.Y - mLeftBottomPoint.Y < 0)
                {
                    mLeftBottomPoint = point;
                }
                if (Math.Abs(point.X - mLeftTopPoint.X) < CCommonPara.mDblError && point.Y - mLeftTopPoint.Y > 0)
                {
                    mLeftTopPoint = point;
                }
                if (Math.Abs(point.X - mRightBottomPoint.X) < CCommonPara.mDblError && point.Y - mRightBottomPoint.Y < 0)
                {
                    mRightBottomPoint = point;
                }
                if (Math.Abs(point.X - mRightTopPoint.X) < CCommonPara.mDblError && point.Y - mRightTopPoint.Y > 0)
                {
                    mRightTopPoint = point;
                }
            }
            //如果是直梁则直接置为最大的X点和最小的X点;
            if (Math.Abs(mLeftBottomPoint.Y - mRightBottomPoint.Y) < CCommonPara.mDblError)
            {
                return;
            }
            if (Math.Abs(mLeftTopPoint.Y - mRightTopPoint.Y) < CCommonPara.mDblError)
            {
                return;
            }
            Vector zVector = new Vector(0, 0, 1);

            if(CDimTools.GetInstance().IsTwoVectorParallel(mNormal,zVector))
            {
                return;
            }

            //下面主要是判断斜梁的情况,且斜梁的斜率与Z轴不平行;
            if (maxXPoint.Y - minXPoint.Y > CCommonPara.mDblError)
            {
                mLeftBottomPoint = minXPoint;
                mLeftTopPoint = maxYPoint;
                mRightBottomPoint = minYPoint;
                mRightTopPoint = maxXPoint;
            }
            else if (maxXPoint.Y - minXPoint.Y < CCommonPara.mNegativeDblError)
            {
                mLeftBottomPoint = minYPoint;
                mLeftTopPoint = minXPoint;
                mRightBottomPoint = maxXPoint;
                mRightTopPoint = maxYPoint;
            }
        }

        /// <summary>
        /// 判断部件是否有螺钉;
        /// </summary>
        /// <returns></returns>
        public bool IsHaveBolt()
        {
            if (mBoltArrayList.Count > 0)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 获得主梁在X方向上的跨度;
        /// </summary>
        /// <returns></returns>
        public double GetXSpaceValue()
        {
            return Math.Abs(GetMaxXPoint().X - GetMinXPoint().X);
        }

        /// <summary>
        /// 获得主梁在Y方向上的跨度;
        /// </summary>
        /// <returns></returns>
        public double GetYSpaceValue()
        {
            return Math.Abs(GetMaxYPoint().Y - GetMinYPoint().Y);
        }

        /// <summary>
        /// 获取主梁在Z方向上的跨度;
        /// </summary>
        /// <returns></returns>
        public double GetZSpaceValue()
        {
            return Math.Abs(GetMaxZPoint().Z - GetMinZPoint().Z);
        }

        /// <summary>
        /// 获取Y值最大的点;
        /// </summary>
        /// <returns></returns>
        public Point GetMaxYPoint()
        {
            return CDimTools.GetInstance().GetMaxYPoint(mPointList);
        }

        /// <summary>
        /// 获取Y值最小的点;
        /// </summary>
        /// <returns></returns>
        public Point GetMinYPoint()
        {
            return CDimTools.GetInstance().GetMinYPoint(mPointList);
        }

        /// <summary>
        /// 获取X值最大的点;
        /// </summary>
        /// <returns></returns>
        public Point GetMaxXPoint()
        {
            return CDimTools.GetInstance().GetMaxXPoint(mPointList);
        }

        /// <summary>
        /// 获取X值最小的点;
        /// </summary>
        /// <returns></returns>
        public Point GetMinXPoint()
        {
            return CDimTools.GetInstance().GetMinXPoint(mPointList);
        }

        /// <summary>
        /// 获取Z值最大的点;
        /// </summary>
        /// <returns></returns>
        public Point GetMaxZPoint()
        {
            return CDimTools.GetInstance().GetMaxZPoint(mPointList);
        }

        /// <summary>
        /// 获取Z值最小的点;
        /// </summary>
        /// <returns></returns>
        public Point GetMinZPoint()
        {
            return CDimTools.GetInstance().GetMinZPoint(mPointList);
        }

        /// <summary>
        /// 获取最小X值点的链表;
        /// </summary>
        /// <returns></returns>
        public List<Point> GetMinXPointList()
        {
            Point minXPoint = CDimTools.GetInstance().GetMinXPoint(mPointList);

            List<Point> minXPointList = new List<Point>();

            foreach (Point point in mPointList)
            {
                if (CDimTools.GetInstance().CompareTwoDoubleValue(point.X, minXPoint.X) == 0)
                {
                    minXPointList.Add(point);
                }
            }

            return minXPointList;
        }

        /// <summary>
        /// 获取最大X值点的链表;
        /// </summary>
        /// <returns></returns>
        public List<Point> GetMaxXPointList()
        {
            Point maxXPoint = CDimTools.GetInstance().GetMaxXPoint(mPointList);

            List<Point> maxXPointList = new List<Point>();

            foreach (Point point in mPointList)
            {
                if (CDimTools.GetInstance().CompareTwoDoubleValue(point.X, maxXPoint.X) == 0)
                {
                    maxXPointList.Add(point);
                }
            }
            return maxXPointList;
        }

        /// <summary>
        /// 获取最小Y值点的链表;
        /// </summary>
        /// <returns></returns>
        public List<Point> GetMinYPointList()
        {
            Point minYPoint = CDimTools.GetInstance().GetMinYPoint(mPointList);

            List<Point> minYPointList = new List<Point>();

            foreach (Point point in mPointList)
            {
                if (CDimTools.GetInstance().CompareTwoDoubleValue(point.Y, minYPoint.Y) == 0)
                {
                    minYPointList.Add(point);
                }
            }
            return minYPointList;
        }

        /// <summary>
        /// 获取最大Y值点的链表;
        /// </summary>
        /// <returns></returns>
        public List<Point> GetMaxYPointList()
        {
            Point maxYPoint = CDimTools.GetInstance().GetMaxYPoint(mPointList);

            List<Point> maxYPointList = new List<Point>();

            foreach (Point point in mPointList)
            {
                if (CDimTools.GetInstance().CompareTwoDoubleValue(point.Y, maxYPoint.Y) == 0)
                {
                    maxYPointList.Add(point);
                }
            }

            return maxYPointList;
        }


        /// <summary>
        /// 获得零件Y值最大并且X最也最大点;
        /// </summary>
        /// <returns></returns>
        public Point GetMaxYMaxXPoint()
        {
            Point maxYPoint=CDimTools.GetInstance().GetMaxYPoint(mPointList);

            List<Point> maxYPointList = new List<Point>();

            foreach(Point point in mPointList)
            {
                if(CDimTools.GetInstance().CompareTwoDoubleValue(point.Y,maxYPoint.Y)==0)
                {
                    maxYPointList.Add(point);
                }
            }

            Point maxYAndMaxXPoint = CDimTools.GetInstance().GetMaxXPoint(maxYPointList);

            return maxYAndMaxXPoint;
        }


        /// <summary>
        /// 获得零件Y值最小并且X最也最大点;
        /// </summary>
        /// <returns></returns>
        public Point GetMinYMaxXPoint()
        {
            Point minYPoint = CDimTools.GetInstance().GetMinYPoint(mPointList);

            List<Point> minYPointList = new List<Point>();

            foreach (Point point in mPointList)
            {
                if (CDimTools.GetInstance().CompareTwoDoubleValue(point.Y, minYPoint.Y) == 0)
                {
                    minYPointList.Add(point);
                }
            }

            Point minYMaxXPoint = CDimTools.GetInstance().GetMaxXPoint(minYPointList);

            return minYMaxXPoint;
        }

        /// <summary>
        /// 获得零件Y值最大并且X值最小的点;
        /// </summary>
        /// <returns></returns>
        public Point GetMaxYMinXPoint()
        {
            Point maxYPoint = CDimTools.GetInstance().GetMaxYPoint(mPointList);

            List<Point> maxYPointList = new List<Point>();

            foreach (Point point in mPointList)
            {
                if (CDimTools.GetInstance().CompareTwoDoubleValue(point.Y, maxYPoint.Y) == 0)
                {
                    maxYPointList.Add(point);
                }
            }

            Point maxYAndMinXPoint = CDimTools.GetInstance().GetMinXPoint(maxYPointList);

            return maxYAndMinXPoint;
        }

        /// <summary>
        /// 获得零件X值最大并且Y值最小的点;
        /// </summary>
        /// <returns></returns>
        public Point GetMaxXMinYPoint()
        {
            Point maxXPoint = CDimTools.GetInstance().GetMaxXPoint(mPointList);

            List<Point> maxXPointList = new List<Point>();

            foreach (Point point in mPointList)
            {
                if (CDimTools.GetInstance().CompareTwoDoubleValue(point.X, maxXPoint.X) == 0)
                {
                    maxXPointList.Add(point);
                }
            }

            Point maxXAndMinYPoint = CDimTools.GetInstance().GetMinYPoint(maxXPointList);

            return maxXAndMinYPoint;
        }

        /// <summary>
        /// 获得零件X值最大并且Y值最大的点;
        /// </summary>
        /// <returns></returns>
        public Point GetMaxXMaxYPoint()
        {
            Point maxXPoint = CDimTools.GetInstance().GetMaxXPoint(mPointList);

            List<Point> maxXPointList = new List<Point>();

            foreach (Point point in mPointList)
            {
                if (CDimTools.GetInstance().CompareTwoDoubleValue(point.X, maxXPoint.X) == 0)
                {
                    maxXPointList.Add(point);
                }
            }

            Point maxXAndMaxYPoint = CDimTools.GetInstance().GetMaxYPoint(maxXPointList);

            return maxXAndMaxYPoint;
        }

        /// <summary>
        /// 获得零件X值最小并且Y值最小的点;
        /// </summary>
        /// <returns></returns>
        public Point GetMinXMinYPoint()
        {
            Point minXPoint = CDimTools.GetInstance().GetMinXPoint(mPointList);

            List<Point> minXPointList = new List<Point>();

            foreach (Point point in mPointList)
            {
                if (CDimTools.GetInstance().CompareTwoDoubleValue(point.X, minXPoint.X) == 0)
                {
                    minXPointList.Add(point);
                }
            }

            Point minXAndMinYPoint = CDimTools.GetInstance().GetMinYPoint(minXPointList);

            return minXAndMinYPoint;
        }

        /// <summary>
        /// 获得零件X值最小并且Y值最大的点;
        /// </summary>
        /// <returns></returns>
        public Point GetMinXMaxYPoint()
        {
            Point minXPoint = CDimTools.GetInstance().GetMinXPoint(mPointList);

            List<Point> minXPointList = new List<Point>();

            foreach (Point point in mPointList)
            {
                if (CDimTools.GetInstance().CompareTwoDoubleValue(point.X, minXPoint.X) == 0)
                {
                    minXPointList.Add(point);
                }
            }

            Point minXAndMaxYPoint = CDimTools.GetInstance().GetMaxYPoint(minXPointList);

            return minXAndMaxYPoint;

        }

        /// <summary>
        /// 获得零件Y值最小并且X值最小的点;
        /// </summary>
        /// <returns></returns>
        public Point GetMinYMinXPoint()
        {
            Point minYPoint = CDimTools.GetInstance().GetMinYPoint(mPointList);

            List<Point> minYPointList = new List<Point>();

            foreach (Point point in mPointList)
            {
                if (CDimTools.GetInstance().CompareTwoDoubleValue(point.Y, minYPoint.Y) == 0)
                {
                    minYPointList.Add(point);
                }
            }

            Point minYAndMinXPoint = CDimTools.GetInstance().GetMinXPoint(minYPointList);

            return minYAndMinXPoint;
        }

        /// <summary>
        /// 获得Z坐标最大X值最小的点;
        /// </summary>
        /// <returns></returns>
        public Point GetMaxZMinXPoint()
        {
            Point maxZPoint = CDimTools.GetInstance().GetMaxZPoint(mPointList);

            List<Point> maxZPointList = new List<Point>();

            foreach (Point point in mPointList)
            {
                if (CDimTools.GetInstance().CompareTwoDoubleValue(point.Z, maxZPoint.Z) == 0)
                {
                    maxZPointList.Add(point);
                }
            }

            Point maxZAndMinXPoint = CDimTools.GetInstance().GetMinXPoint(maxZPointList);

            return maxZAndMinXPoint;
        }

        /// <summary>
        /// 获得Z坐标最大X值最大的点;
        /// </summary>
        /// <returns></returns>
        public Point GetMaxZMaxXPoint()
        {
            Point maxZPoint = CDimTools.GetInstance().GetMaxZPoint(mPointList);

            List<Point> maxZPointList = new List<Point>();

            foreach (Point point in mPointList)
            {
                if (CDimTools.GetInstance().CompareTwoDoubleValue(point.Z, maxZPoint.Z) == 0)
                {
                    maxZPointList.Add(point);
                }
            }

            Point maxZAndMaxXPoint = CDimTools.GetInstance().GetMaxXPoint(maxZPointList);

            return maxZAndMaxXPoint;
        }

        /// <summary>
        /// 获得Z坐标最小X值最小的点;
        /// </summary>
        /// <returns></returns>
        public Point GetMinZMinXPoint()
        {
            Point minZPoint = CDimTools.GetInstance().GetMinZPoint(mPointList);

            List<Point> minZPointList = new List<Point>();

            foreach (Point point in mPointList)
            {
                if (CDimTools.GetInstance().CompareTwoDoubleValue(point.Z, minZPoint.Z) == 0)
                {
                    minZPointList.Add(point);
                }
            }
            Point minZAndMinXPoint = CDimTools.GetInstance().GetMinXPoint(minZPointList);

            return minZAndMinXPoint;
        }

        /// <summary>
        /// 获得Z坐标最小X值最大的点;
        /// </summary>
        /// <returns></returns>
        public Point GetMinZMaxXPoint()
        {
            Point minZPoint = CDimTools.GetInstance().GetMinZPoint(mPointList);

            List<Point> minZPointList = new List<Point>();

            foreach (Point point in mPointList)
            {
                if (CDimTools.GetInstance().CompareTwoDoubleValue(point.Z, minZPoint.Z) == 0)
                {
                    minZPointList.Add(point);
                }
            }

            Point minZAndMaxXPoint = CDimTools.GetInstance().GetMaxXPoint(minZPointList);

            return minZAndMaxXPoint;
        }

        /// <summary>
        /// 获得X坐标最小Z值最大的点;
        /// </summary>
        /// <returns></returns>
        public Point GetMinXMaxZPoint()
        {
            Point minXPoint = CDimTools.GetInstance().GetMinXPoint(mPointList);

            List<Point> minXPointList = new List<Point>();

            foreach (Point point in mPointList)
            {
                if (CDimTools.GetInstance().CompareTwoDoubleValue(point.X, minXPoint.X) == 0)
                {
                    minXPointList.Add(point);
                }
            }

            Point minXAndMaxZPoint = CDimTools.GetInstance().GetMaxZPoint(minXPointList);

            return minXAndMaxZPoint;
        }

        /// <summary>
        /// 获得X坐标最小Z值最小的点;
        /// </summary>
        /// <returns></returns>
        public Point GetMinXMinZPoint()
        {
            Point minXPoint = CDimTools.GetInstance().GetMinXPoint(mPointList);

            List<Point> minXPointList = new List<Point>();

            foreach (Point point in mPointList)
            {
                if (CDimTools.GetInstance().CompareTwoDoubleValue(point.X, minXPoint.X) == 0)
                {
                    minXPointList.Add(point);
                }
            }

            Point minXAndMinZPoint = CDimTools.GetInstance().GetMinZPoint(minXPointList);

            return minXAndMinZPoint;
        }

        /// <summary>
        /// 获得X坐标最大Z值最小的点;
        /// </summary>
        /// <returns></returns>
        public Point GetMaxXMinZPoint()
        {
            Point maxXPoint = CDimTools.GetInstance().GetMaxXPoint(mPointList);

            List<Point> maxXPointList = new List<Point>();

            foreach (Point point in mPointList)
            {
                if (CDimTools.GetInstance().CompareTwoDoubleValue(point.X, maxXPoint.X) == 0)
                {
                    maxXPointList.Add(point);
                }
            }

            Point maxXAndMinZPoint = CDimTools.GetInstance().GetMinZPoint(maxXPointList);

            return maxXAndMinZPoint;
        }

        /// <summary>
        /// 获得X坐标最大Z值最大的点;
        /// </summary>
        /// <returns></returns>
        public Point GetMaxXMaxZPoint()
        {
            Point maxXPoint = CDimTools.GetInstance().GetMaxXPoint(mPointList);

            List<Point> maxXPointList = new List<Point>();

            foreach (Point point in mPointList)
            {
                if (CDimTools.GetInstance().CompareTwoDoubleValue(point.X, maxXPoint.X) == 0)
                {
                    maxXPointList.Add(point);
                }
            }

            Point maxXAndMaxZPoint = CDimTools.GetInstance().GetMaxZPoint(maxXPointList);

            return maxXAndMaxZPoint;
        }

        /// <summary>
        /// 获得Z坐标最大Y坐标最小的点;
        /// </summary>
        /// <returns></returns>
        public Point GetMaxZMinYPoint()
        {
            Point maxZPoint = CDimTools.GetInstance().GetMaxZPoint(mPointList);

            List<Point> maxZPointList = new List<Point>();

            foreach (Point point in mPointList)
            {
                if (CDimTools.GetInstance().CompareTwoDoubleValue(point.Z, maxZPoint.Z) == 0)
                {
                    maxZPointList.Add(point);
                }
            }

            Point maxZAndMinYPoint = CDimTools.GetInstance().GetMinYPoint(maxZPointList);

            return maxZAndMinYPoint;
        }

        /// <summary>
        /// 获得Z坐标最大Y坐标最大的点;
        /// </summary>
        /// <returns></returns>
        public Point GetMaxZMaxYPoint()
        {
            Point maxZPoint = CDimTools.GetInstance().GetMaxZPoint(mPointList);

            List<Point> maxZPointList = new List<Point>();

            foreach (Point point in mPointList)
            {
                if (CDimTools.GetInstance().CompareTwoDoubleValue(point.Z, maxZPoint.Z) == 0)
                {
                    maxZPointList.Add(point);
                }
            }

            Point maxZAndMaxYPoint = CDimTools.GetInstance().GetMaxYPoint(maxZPointList);

            return maxZAndMaxYPoint;
        }

        /// <summary>
        /// 获得Y坐标最大Z坐标最大的点;
        /// </summary>
        /// <returns></returns>
        public Point GetMaxYMaxZPoint()
        {
            Point maxYPoint = CDimTools.GetInstance().GetMaxYPoint(mPointList);

            List<Point> maxYPointList = new List<Point>();

            foreach (Point point in mPointList)
            {
                if (CDimTools.GetInstance().CompareTwoDoubleValue(point.Y, maxYPoint.Y) == 0)
                {
                    maxYPointList.Add(point);
                }
            }

            Point maxYAndMaxZPoint = CDimTools.GetInstance().GetMaxZPoint(maxYPointList);

            return maxYAndMaxZPoint;
        }

        /// <summary>
        /// 获得Y坐标最小Z坐标最大的点;
        /// </summary>
        /// <returns></returns>
        public Point GetMinYMaxZPoint()
        {
            Point minYPoint = CDimTools.GetInstance().GetMinYPoint(mPointList);

            List<Point> minYPointList = new List<Point>();

            foreach (Point point in mPointList)
            {
                if (CDimTools.GetInstance().CompareTwoDoubleValue(point.Y, minYPoint.Y) == 0)
                {
                    minYPointList.Add(point);
                }
            }

            Point minYAndMaxZPoint = CDimTools.GetInstance().GetMaxZPoint(minYPointList);

            return minYAndMaxZPoint;
        }

        /// <summary>
        /// 获得Y坐标最小Z坐标最小的点;
        /// </summary>
        /// <returns></returns>
        public Point GetMinYMinZPoint()
        {
            Point minYPoint = CDimTools.GetInstance().GetMinYPoint(mPointList);

            List<Point> minYPointList = new List<Point>();

            foreach (Point point in mPointList)
            {
                if (CDimTools.GetInstance().CompareTwoDoubleValue(point.Y, minYPoint.Y) == 0)
                {
                    minYPointList.Add(point);
                }
            }

            Point minYAndMinZPoint = CDimTools.GetInstance().GetMinZPoint(minYPointList);

            return minYAndMinZPoint;
        }

        /// <summary>
        /// 获得Y坐标最大Z坐标最小的点;
        /// </summary>
        /// <returns></returns>
        public Point GetMaxYMinZPoint()
        {
            Point maxYPoint = CDimTools.GetInstance().GetMaxYPoint(mPointList);

            List<Point> maxYPointList = new List<Point>();

            foreach (Point point in mPointList)
            {
                if (CDimTools.GetInstance().CompareTwoDoubleValue(point.Y, maxYPoint.Y) == 0)
                {
                    maxYPointList.Add(point);
                }
            }

            Point maxYAndMinZPoint = CDimTools.GetInstance().GetMinZPoint(maxYPointList);

            return maxYAndMinZPoint;
        }

        /// <summary>
        /// 获得Z坐标最小Y坐标最大的点;
        /// </summary>
        /// <returns></returns>
        public Point GetMinZMaxYPoint()
        {
            Point minZPoint = CDimTools.GetInstance().GetMinZPoint(mPointList);

            List<Point> minZPointList = new List<Point>();

            foreach (Point point in mPointList)
            {
                if (CDimTools.GetInstance().CompareTwoDoubleValue(point.Z, minZPoint.Z) == 0)
                {
                    minZPointList.Add(point);
                }
            }

            Point minZAndMaxYPoint = CDimTools.GetInstance().GetMaxYPoint(minZPointList);

            return minZAndMaxYPoint;
        }

        /// <summary>
        /// 获得Z坐标最小Y坐标最小的点;
        /// </summary>
        /// <returns></returns>
        public Point GetMinZMinYPoint()
        {
            Point minZPoint = CDimTools.GetInstance().GetMinZPoint(mPointList);

            List<Point> minZPointList = new List<Point>();

            foreach (Point point in mPointList)
            {
                if (CDimTools.GetInstance().CompareTwoDoubleValue(point.Z, minZPoint.Z) == 0)
                {
                    minZPointList.Add(point);
                }
            }

            Point minZAndMinYPoint = CDimTools.GetInstance().GetMinYPoint(minZPointList);

            return minZAndMinYPoint;
        }

        /// <summary>
        /// 设置零件中点的链表中的点的Z坐标为零;
        /// </summary>
        public void SetPointListZValueZero()
        {
            List<Point> pointList = new List<Point>();

            foreach(Point point in mPointList)
            {
                point.Z = 0;

                if(!pointList.Contains(point))
                {
                    pointList.Add(point);
                }
            }

            mPointList = pointList;
        }

        /// <summary>
        /// 获得该零件左侧默认的标注距离;
        /// </summary>
        /// <param name="mrPart"></param>
        /// <returns></returns>
        public double GetLeftDefaultDimDistance(Point firstDimPoint)
        {
            return Math.Abs(GetMinXPoint().X - firstDimPoint.X) + CCommonPara.mDefaultDimDistance;
        }

        /// <summary>
        /// 获得该零件右侧默认的标注距离;
        /// </summary>
        /// <param name="mrPart"></param>
        /// <returns></returns>
        public double GetRightDefaultDimDistance(Point firstDimPoint)
        {
            return Math.Abs(GetMaxXPoint().X - firstDimPoint.X) + CCommonPara.mDefaultDimDistance;
        }

        /// <summary>
        /// 获得该零件上侧默认的标注距离;
        /// </summary>
        /// <param name="mrPart"></param>
        /// <returns></returns>
        public double GetUpDefaultDimDistance(Point firstDimPoint)
        {
            return Math.Abs(GetMaxYPoint().Y - firstDimPoint.Y) + CCommonPara.mDefaultDimDistance;
        }

        /// <summary>
        /// 获得该零件下侧默认的标注距离;
        /// </summary>
        /// <param name="mrPart"></param>
        /// <returns></returns>
        public double GetDownDefaultDimDistance(Point firstDimPoint)
        {
            return Math.Abs(GetMinYPoint().Y - firstDimPoint.Y) + CCommonPara.mDefaultDimDistance;
        }

        /// <summary>
        /// 更新零件包围盒的四个角点;
        /// </summary>
        public virtual void UpdatePartBoxPoint()
        {
            Point minX = GetMinXPoint();
            Point maxX = GetMaxXPoint();
            Point minY = GetMinYPoint();
            Point maxY = GetMaxYPoint();

            MrSlopeType mrYSlopeType = CDimTools.GetInstance().JudgeLineSlope(maxY, minY);
            MrSlopeType mrXSlopeType = CDimTools.GetInstance().JudgeLineSlope(maxX, minX);

            //1.如果斜率大于零;
            if (mrYSlopeType == MrSlopeType.MORETHAN_ZERO)
            {
                mLeftBottomPoint = minX;
                mLeftTopPoint = maxY;
                mRightTopPoint = maxX;
                mRightBottomPoint = minY;
            }
            //2.如果斜率小于零;
            if (mrXSlopeType == MrSlopeType.LESSTHAN_ZERO)
            {
                mLeftBottomPoint = minY;
                mLeftTopPoint = minX;
                mRightTopPoint = maxY;
                mRightBottomPoint = maxX;
            }
        }
    }
}
