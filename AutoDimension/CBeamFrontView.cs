using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

using AutoDimension.Entity;
using Tekla.Structures.Drawing;
using Tekla.Structures.Model;
using Tekla.Structures.Geometry3d;
using Tekla.Structures;

using TSD = Tekla.Structures.Drawing;
using TSM = Tekla.Structures.Model;

namespace AutoDimension
{
    /// <summary>
    /// 前视图对象;
    /// </summary>
    public class CBeamFrontView : CView
    {
        /// <summary>
        /// 螺钉组的需要在Y方向上标注的数据字典,去掉重复标注;
        /// </summary>
        private Dictionary<String, bool> mDicYBoltDimPoints = new Dictionary<String, bool>();

        /// <summary>
        /// 零件标记时需要去掉重复标注的点;
        /// </summary>
        private Dictionary<String, bool> mDicMarkDimPoints = new Dictionary<String, bool>();

        /// <summary>
        /// 向上第一个标注的点;
        /// </summary>
        private Point mUpDimFirstPoint = new Point();

        /// <summary>
        /// 向上第一个标注的距离;
        /// </summary>
        private double mUpDimDistance = 0.0;

        /// <summary>
        /// 向下第一个标注的点;
        /// </summary>
        private Point mDownDimFirstPoint = new Point();

        /// <summary>
        /// 向下第一个标注的距离;
        /// </summary>
        private double mDownDimDistance = 0.0;

        /// <summary>
        /// 上侧连接板是否已经进行了标注,如果标注了,那么总的X向标注需要向上移动标注间隙的距离;
        /// </summary>
        private bool mbHaveUpConnectPlateDim = false;

        /// <summary>
        /// 下侧连接板是否已经进行了标注;
        /// </summary>
        private bool mbHaveDownConnectPlateDim = false;

        /// <summary>
        /// 左侧切割标注点;
        /// </summary>
        private Point mLeftCuttingDimPoint = new Point();

        /// <summary>
        /// 左侧切割标注距离;
        /// </summary>
        private double mLeftCuttingDimDistance = 0.0;

        /// <summary>
        /// 右侧切割标注点;
        /// </summary>
        private Point mRightCuttingDimPoint = new Point();

        /// <summary>
        /// 右侧切割标注距离;
        /// </summary>
        private double mRightCuttingDimDistance = 0.0;

        /// <summary>
        /// 构造函数;
        /// </summary>
        /// <param name="viewBase"></param>
        /// <param name="model"></param>
        public CBeamFrontView(TSD.View viewBase, Model model): base(viewBase)
        { 
            mViewBase = viewBase;
            mModel = model;
        }

        /// <summary>
        /// 初始化前视图,在自动剖面时候调用;
        /// </summary>
        public void InitFrontView()
        {
            Init();
        }

        /// <summary>
        /// 初始化函数;
        /// </summary>
        protected void Init()
        {
            if (mViewBase == null)
            {
                return;
            }
            //初始化视图的包围盒;
            CDimTools.GetInstance().InitViewBox();

            //获取所有螺钉的数据字典;
            Dictionary<Identifier, TSD.Bolt> dicIdentifierBolt = CDimTools.GetInstance().GetAllBoltInDrawing(mViewBase);

            //获取所有Drawing视图中的Part;
            List<TSD.Part> partList = CDimTools.GetInstance().GetAllPartInDrawing(mViewBase);

            //先初始化主部件;
            CDimTools.GetInstance().InitMrPart(CMrMainBeam.GetInstance().mPartInModel, mViewBase,CMrMainBeam.GetInstance());

            foreach (TSD.Part partInDrawing in partList)
            {
                //1.获取部件的信息;
                TSM.ModelObject modelObjectInModel = CDimTools.GetInstance().TransformDrawingToModel(partInDrawing);
                TSM.Part partInModel = modelObjectInModel as TSM.Part;
                CMrPart mrPart = null;

                if (CMrMainBeam.GetInstance().mPartInModel.Identifier.GUID == partInModel.Identifier.GUID)
                {
                    mrPart = CMrMainBeam.GetInstance();
                    mMainBeam = CMrMainBeam.GetInstance();
                    mMainBeam.mPartInDrawing = partInDrawing;
                    AppendMrPart(mrPart);
                }
                else
                {
                    mrPart = new CMrPart(partInModel, partInDrawing);
                    CDimTools.GetInstance().InitMrPart(modelObjectInModel, mViewBase, mrPart);
                    AppendMrPart(mrPart);
                }
                mrPart.GetBeamFrontViewInfo().InitMrPartFrontViewInfo();
                
                //2.获取部件中的所有螺钉组;
                List<BoltArray> boltArrayList = CDimTools.GetInstance().GetAllBoltArray(partInModel);

                foreach (BoltArray boltArray in boltArrayList)
                {
                    TSD.Bolt boltInDrawing = dicIdentifierBolt[boltArray.Identifier];
                    CMrBoltArray mrBoltArray = new CMrBoltArray(boltArray, boltInDrawing);
                    CDimTools.GetInstance().InitMrBoltArray(boltArray, mViewBase, mrBoltArray);
                    mrPart.AppendMrBoltArray(mrBoltArray);
                }
                CDimTools.GetInstance().UpdateViewBox(mrPart);
            }
            //设置主部件的工作点在梁的顶端;
            CMrMainBeam.GetInstance().mLeftWorkPoint.Y = CMrMainBeam.GetInstance().GetMaxYPoint().Y;
            CMrMainBeam.GetInstance().mRightWorkPoint.Y = CMrMainBeam.GetInstance().GetMaxYPoint().Y;
        }

        /// <summary>
        /// 前视图初始化线程函数;
        /// </summary>
        /// <param name="message"></param>
        private void ThreadFunc(object message)
        {
            lock (mLockString)
            {
                Init();
            }
        }

        /// <summary>
        /// 创建前视图标注;
        /// </summary>
        public void CreateDim()
        {
            //启动初始化函数;
            Thread thread = new Thread(new ParameterizedThreadStart(ThreadFunc));
            thread.Start();

            //首先清空标注和标记;
            CDimTools.GetInstance().ClearAllDim(mViewBase);
            CDimTools.GetInstance().ClearAllPartMark(mViewBase);

            lock(mLockString)
            {
#if DEBUG
                //采用总尺寸标注;
                if (CBeamDimSetting.GetInstance().mFrontViewSetting.mDimOverallSize == true)
                {
                    DrawCuttingDim();
                    DrawPartBoltDim();
                    DrawMainPartBoltDimY();
                    DrawMainPartMiddlePlateDimY();
                    DrawMainPartMiddleAngleSheetDimY();
                    DrawAllPartUpDimX();
                    DrawAllPartDownDimX();
                    DrawMainPartLengthDimX();
                    DrawWorkPointToMainPartDimX();
                    DrawWorkPointToWorkPointDimX();
                    DrawPartMark();
                    DrawMainPartBoltMark();
                }
                else if (CBeamDimSetting.GetInstance().mFrontViewSetting.mDimSize == true)
                {
                    DrawCuttingDim();
                    DrawPartBoltDim();
                    DrawMainPartBoltDimY();
                    DrawMainPartMiddlePlateDimY();
                    DrawMainPartMiddleAngleSheetDimY();
                    DrawAllPartUpFenSizeDimX();
                    DrawAllPartDownFenSizeDimX();
                    DrawMainPartBoltDimX();
                    DrawMainPartLengthDimX();
                    DrawWorkPointToMainPartDimX();
                    DrawWorkPointToWorkPointDimX();
                    DrawPartMark();
                    DrawMainPartBoltMark();
                }
#else
                try
                {
                    //采用总尺寸标注;
                    if (CBeamDimSetting.GetInstance().mFrontViewSetting.mDimOverallSize == true)
                    {
                        DrawCuttingDim();
                        DrawPartBoltDim();
                        DrawMainPartBoltDimY();
                        DrawMainPartMiddlePlateDimY();
                        DrawMainPartMiddleAngleSheetDimY();
                        DrawAllPartUpDimX();
                        DrawAllPartDownDimX();
                        DrawMainPartLengthDimX();
                        DrawWorkPointToMainPartDimX();
                        DrawWorkPointToWorkPointDimX();
                        DrawPartMark();
                        DrawMainPartBoltMark();
                    }
                    else if (CBeamDimSetting.GetInstance().mFrontViewSetting.mDimSize == true)
                    {
                        DrawCuttingDim();
                        DrawPartBoltDim();
                        DrawMainPartBoltDimY();
                        DrawMainPartMiddlePlateDimY();
                        DrawMainPartMiddleAngleSheetDimY();
                        DrawAllPartUpFenSizeDimX();
                        DrawAllPartDownFenSizeDimX();
                        DrawMainPartBoltDimX();
                        DrawMainPartLengthDimX();
                        DrawWorkPointToMainPartDimX();
                        DrawWorkPointToWorkPointDimX();
                        DrawPartMark();
                        DrawMainPartBoltMark();
                    }
                }
                catch(Exception e)
                {
                    string strText = "提示：程序发生异常\n" + "异常信息：" + e.Message;
                    MessageBox.Show(strText);
                    return;
                }
#endif
            }
        }

        /// <summary>
        /// 绘制所有零件的X向上标注;
        /// </summary>
        public void DrawAllPartUpDimX()
        {
            //移除主梁;
            mMrPartList.Remove(mMainBeam);
            
            bool bNeedUpDim = false;

            List<Point> upDimPointList = new List<Point>();

            foreach (CMrPart mrPart in mMrPartList)
            {
                CMrDimSet partDimSet = mrPart.GetBeamFrontViewInfo().GetPartUpDimSet();

                if (partDimSet != null && partDimSet.Count > 0)
                {
                    bNeedUpDim = true;
                    upDimPointList.AddRange(partDimSet.GetDimPointList());
                }
            }

            //1.判断主梁的螺栓是否需要标注;
            CBeamFrontViewSetting beamFrontViewSetting = CBeamDimSetting.GetInstance().mFrontViewSetting;

            bool bNeedMainBeamBoltDim = beamFrontViewSetting.FindDimValueByName(CBeamFrontViewSetting.mstrBolt);

            if (bNeedMainBeamBoltDim)
            {
                CMrDimSet mrDimSet = GetMainPartBoltUpDimX();
                upDimPointList.AddRange(mrDimSet.GetDimPointList());
            }
            if (bNeedUpDim == false && bNeedMainBeamBoltDim == false)
            {
                return;
            }

            //2.默认把主梁的左右最小最大值点加入到链表中;
            Point minXPoint = mMainBeam.GetMinXMaxYPoint();
            Point maxXPoint = mMainBeam.GetMaxXMaxYPoint();
            upDimPointList.Add(minXPoint);
            upDimPointList.Add(maxXPoint);

            PointList pointList = new PointList();

            foreach (Point point in upDimPointList)
            {
                pointList.Add(point);
            }

            mUpDimDistance = GetUpDimDistance(upDimPointList[0]);
            Vector upDimVector = new Vector(0, 1, 0);
            CDimTools.GetInstance().DrawDimensionSet(mViewBase, pointList, upDimVector, mUpDimDistance, CCommonPara.mSizeDimPath);
            mMrPartList.Add(mMainBeam);
        }

        /// <summary>
        /// 获得主梁上方需要标注的螺钉组;
        /// </summary>
        private CMrDimSet GetMainPartBoltUpDimX()
        {
            CMrDimSet mrDimSet = new CMrDimSet();

            List<CMrBoltArray> mrBoltArrayList = mMainBeam.GetBoltArrayList();

            foreach (CMrBoltArray mrBoltArray in mrBoltArrayList)
            {
                Vector normal = mrBoltArray.mNormal;

                if (!CDimTools.GetInstance().IsTwoVectorParallel(normal, new Vector(0, 0, 1)))
                {
                    continue;
                }
                List<Point> pointList = mrBoltArray.GetMaxYPointList();
                mrDimSet.AddRange(pointList);
            }

            return mrDimSet;
        }

        /// <summary>
        /// 绘制所有零件的上方分尺寸,主要是支持牛腿及连接板一道尺寸和剩下的一道尺寸;
        /// </summary>
        public void DrawAllPartUpFenSizeDimX()
        {
            //移除主梁;
            mMrPartList.Remove(mMainBeam);

            bool bNeedHorizontalConnectPlateDim = false;
            bool bNeedOtherPartDim = false;
            List<Point> horizontalConnectPlateDimPointList = new List<Point>();
            List<Point> otherPartDimPointList = new List<Point>();

            //1.遍历获得所有零件的向上标注的点;
            foreach (CMrPart mrPart in mMrPartList)
            {
                Vector normal = mrPart.mNormal;
                CMrDimSet partDimSet = mrPart.GetBeamFrontViewInfo().GetPartUpDimSet();

                if (partDimSet != null && partDimSet.Count > 0)
                {
                    //如果是水平连接板及支撑板、H型钢等;
                    if (CDimTools.GetInstance().IsPartTheSupportPlate(mrPart))
                    {
                        bNeedHorizontalConnectPlateDim = true;
                        horizontalConnectPlateDimPointList.AddRange(partDimSet.GetDimPointList());
                    }
                    else
                    {
                        bNeedOtherPartDim = true;
                        otherPartDimPointList.AddRange(partDimSet.GetDimPointList());
                    }
                }
            }

            Point minXPoint = mMainBeam.GetMinXMaxYPoint();
            Point maxXPoint = mMainBeam.GetMaxXMaxYPoint();

            //1.如果其他剩下的零件需要标注;
            if (bNeedOtherPartDim)
            {
                PointList pointList = new PointList();

                foreach (Point pt in otherPartDimPointList)
                {
                    pointList.Add(pt);
                }

                pointList.Add(minXPoint);
                pointList.Add(maxXPoint);

                Vector upDimVector = new Vector(0, 1, 0);
                mUpDimDistance = GetUpDimDistance(pointList[0]);
                CDimTools.GetInstance().DrawDimensionSet(mViewBase, pointList, upDimVector, mUpDimDistance, CCommonPara.mSizeDimPath);
            }
            //2.如果水平连接板及支撑需要标注;
            if (bNeedHorizontalConnectPlateDim)
            {
                PointList pointList = new PointList();

                foreach (Point pt in horizontalConnectPlateDimPointList)
                {
                    pointList.Add(pt);
                }

                pointList.Add(minXPoint);
                pointList.Add(maxXPoint);

                Vector upDimVector = new Vector(0, 1, 0);
                mUpDimDistance = GetUpDimDistance(pointList[0]);
                CDimTools.GetInstance().DrawDimensionSet(mViewBase, pointList, upDimVector, mUpDimDistance, CCommonPara.mSizeDimPath);
            }
            mMrPartList.Add(mMainBeam);
        }

        /// <summary>
        /// 绘制所有零件下方的分尺寸,主要是支持牛腿及连接板一道尺寸和剩下的一道尺寸;
        /// </summary>
        public void DrawAllPartDownFenSizeDimX()
        {
            //移除主梁;
            mMrPartList.Remove(mMainBeam);

            bool bNeedHorizontalConnectPlateDim = false;
            bool bNeedOtherPartDim = false;
            List<Point> horizontalConnectPlateDimPointList = new List<Point>();
            List<Point> otherPartDimPointList = new List<Point>();

            //1.遍历获得所有零件的向上标注的点;
            foreach (CMrPart mrPart in mMrPartList)
            {
                Vector normal = mrPart.mNormal;
                CMrDimSet partDimSet = mrPart.GetBeamFrontViewInfo().GetPartDownDimSet();

                if (partDimSet != null && partDimSet.Count > 0)
                {
                    //如果是水平连接板及支撑板、H型钢等;
                    if (CDimTools.GetInstance().IsPartTheSupportPlate(mrPart))
                    {
                        bNeedHorizontalConnectPlateDim = true;
                        horizontalConnectPlateDimPointList.AddRange(partDimSet.GetDimPointList());
                    }
                    else
                    {
                        bNeedOtherPartDim = true;
                        otherPartDimPointList.AddRange(partDimSet.GetDimPointList());
                    }
                }
            }

            Point minXPoint = mMainBeam.GetMinXMinYPoint();
            Point maxXPoint = mMainBeam.GetMaxXMinYPoint();

            //1.如果其他剩下的零件需要标注;
            if (bNeedOtherPartDim)
            {
                PointList pointList = new PointList();

                foreach (Point pt in otherPartDimPointList)
                {
                    pointList.Add(pt);
                }

                pointList.Add(minXPoint);
                pointList.Add(maxXPoint);

                Vector upDimVector = new Vector(0, -1, 0);
                mDownDimDistance = GetDownDimDistance(pointList[0]);
                CDimTools.GetInstance().DrawDimensionSet(mViewBase, pointList, upDimVector, mDownDimDistance, CCommonPara.mSizeDimPath);
            }
            //2.如果水平连接板及支撑需要标注;
            if (bNeedHorizontalConnectPlateDim)
            {
                PointList pointList = new PointList();

                foreach (Point pt in horizontalConnectPlateDimPointList)
                {
                    pointList.Add(pt);
                }

                pointList.Add(minXPoint);
                pointList.Add(maxXPoint);

                Vector upDimVector = new Vector(0, -1, 0);
                mDownDimDistance = GetDownDimDistance(pointList[0]);
                CDimTools.GetInstance().DrawDimensionSet(mViewBase, pointList, upDimVector, mDownDimDistance, CCommonPara.mSizeDimPath);
            }
            mMrPartList.Add(mMainBeam);
        }

        /// <summary>
        /// 绘制主部件螺栓，采用单独的一道尺寸;
        /// </summary>
        public void DrawMainPartBoltDimX()
        {
            PointList pointList = new PointList();

            CBeamTopViewSetting beamTopViewSetting = CBeamDimSetting.GetInstance().mTopViewSetting;

            //2.判断主梁的螺栓是否需要标注;
            bool bNeedMainBeamBoltDim = beamTopViewSetting.FindDimValueByName(CBeamTopViewSetting.mstrBolt);

            if (bNeedMainBeamBoltDim)
            {
                CMrDimSet mrDimSet = GetMainPartBoltUpDimX();

                foreach (Point pt in mrDimSet.GetDimPointList())
                {
                    pointList.Add(pt);
                }

                if (pointList.Count == 0)
                {
                    return;
                }

                Point minXPoint = mMainBeam.GetMinXMaxYPoint();
                Point maxXPoint = mMainBeam.GetMaxXMaxYPoint();

                pointList.Add(minXPoint);
                pointList.Add(maxXPoint);

                Vector upDimVector = new Vector(0, 1, 0);
                mUpDimDistance = GetUpDimDistance(pointList[0]);
                CDimTools.GetInstance().DrawDimensionSet(mViewBase, pointList, upDimVector, mUpDimDistance, CCommonPara.mSizeDimPath);
            }
        }

        /// <summary>
        /// 绘制所有零件X向的下标注;
        /// </summary>
        public void DrawAllPartDownDimX()
        {
            List<Point> downDimPointList = new List<Point>();

            bool bNeedDownDim = false;

            foreach (CMrPart mrPart in mMrPartList)
            {
                CMrDimSet partDimSet = mrPart.GetBeamFrontViewInfo().GetPartDownDimSet();

                if (partDimSet != null && partDimSet.Count > 0)
                {
                    bNeedDownDim = true;
                    downDimPointList.AddRange(partDimSet.GetDimPointList());
                }
            }

            if (bNeedDownDim == false)
            {
                return;
            }

            Point minXPoint = mMainBeam.GetMinXMinYPoint();
            Point maxXPoint = mMainBeam.GetMaxXMinYPoint();
            downDimPointList.Add(minXPoint);
            downDimPointList.Add(maxXPoint);

            PointList pointList = new PointList();

            foreach (Point point in downDimPointList)
            {
                pointList.Add(point);
            }

            mDownDimDistance = GetDownDimDistance(downDimPointList[0]);
            Vector upDimVector = new Vector(0, -1, 0);
            CDimTools.GetInstance().DrawDimensionSet(mViewBase, pointList, upDimVector, mDownDimDistance, CCommonPara.mSizeDimPath);
        }

        /// <summary>
        /// 绘制主部件长度标注;
        /// </summary>
        public void DrawMainPartLengthDimX()
        {
            CBeamFrontViewSetting beamFrontViewSetting = CBeamDimSetting.GetInstance().mFrontViewSetting;

            //1.判断是否需要标注主梁的长度,如果需要采用单一的一道总尺寸;
            bool bNeedDimMainPartLength = beamFrontViewSetting.FindDimValueByName(CBeamFrontViewSetting.mstrMainPartLength);

            bool bMarkValue = beamFrontViewSetting.FindMarkValueByName(CBeamFrontViewSetting.mstrMainPartLength);
            mMainBeam.SetNeedAddMarkFlag(bMarkValue);

            if (!bNeedDimMainPartLength)
            {
                return;
            }

            Point minXPoint = mMainBeam.GetMinXMaxYPoint();
            Point maxXPoint = mMainBeam.GetMaxXMaxYPoint();

            //2.如果需要标注工作点到工作点,并且主部件与工作点的距离左边或者右边为0，或者都为0时则不标注主部件长度;
            bool bNeedWorkPtToWorkPt = beamFrontViewSetting.FindDimValueByName(CBeamFrontViewSetting.mstrWorkPointToWorkPoint);

            bMarkValue = beamFrontViewSetting.FindMarkValueByName(CBeamFrontViewSetting.mstrWorkPointToWorkPoint);
            mMainBeam.SetNeedAddMarkFlag(bMarkValue);

            if(bNeedWorkPtToWorkPt)
            {
                double xDistanceLeft = Math.Abs(minXPoint.X - CMrMainBeam.GetInstance().mLeftWorkPoint.X);
                double xDistanceRight = Math.Abs(maxXPoint.X - CMrMainBeam.GetInstance().mRightWorkPoint.X);

                if (xDistanceLeft < CCommonPara.mDblError || xDistanceRight < CCommonPara.mDblError)
                {
                    return;
                }
            }

            PointList pointList = new PointList();
            pointList.Add(minXPoint);
            pointList.Add(maxXPoint);
            Vector upDimVector = new Vector(0, 1, 0);

            mUpDimDistance = GetUpDimDistance(minXPoint);
            CDimTools.GetInstance().DrawDimensionSet(mViewBase, pointList, upDimVector, mUpDimDistance, CCommonPara.mMainSizeDimPath);
        }

        /// <summary>
        /// 标注工作点到主零件的距离之间尺寸;
        /// </summary>
        public void DrawWorkPointToMainPartDimX()
        {
            CBeamFrontViewSetting beamFrontViewSetting = CBeamDimSetting.GetInstance().mFrontViewSetting;

            bool bMarkValue = beamFrontViewSetting.FindMarkValueByName(CBeamFrontViewSetting.mstrWorkPointToMainPart);
            mMainBeam.SetNeedAddMarkFlag(bMarkValue);

            //1.判断是否需要标注主梁与工作点之间的距离,采用一道尺寸来进行标注;
            bool bNeedDimWpToMainPart = beamFrontViewSetting.FindDimValueByName(CBeamFrontViewSetting.mstrWorkPointToMainPart);

            if (!bNeedDimWpToMainPart)
            {
                return;
            }

            Point mainBeamMinX = CMrMainBeam.GetInstance().GetMinXMaxYPoint();
            Point mainBeamMaxX = CMrMainBeam.GetInstance().GetMaxXMaxYPoint();
            Point leftWorkPoint = CMrMainBeam.GetInstance().mLeftWorkPoint;
            Point rightWorkPoint = CMrMainBeam.GetInstance().mRightWorkPoint;

            //2.如果工作点到工作点需要进行标注，而主部件左右侧与工作点的左右侧的距离都为0则不需要标注;
            bool bNeedWorkPtToWorkPt = beamFrontViewSetting.FindDimValueByName(CBeamFrontViewSetting.mstrWorkPointToWorkPoint);

            bMarkValue = beamFrontViewSetting.FindMarkValueByName(CBeamFrontViewSetting.mstrWorkPointToWorkPoint);
            mMainBeam.SetNeedAddMarkFlag(bMarkValue);

            if (bNeedWorkPtToWorkPt)
            {
                double xDistanceLeft = Math.Abs(mainBeamMinX.X - leftWorkPoint.X);
                double xDistanceRight = Math.Abs(mainBeamMaxX.X - rightWorkPoint.X);

                if (xDistanceLeft < CCommonPara.mDblError && xDistanceRight < CCommonPara.mDblError)
                {
                    return;
                }
            }

            PointList pointList = new PointList();
            pointList.Add(leftWorkPoint);
            pointList.Add(rightWorkPoint);
            pointList.Add(mainBeamMinX);
            pointList.Add(mainBeamMaxX);

            Vector upDimVector = new Vector(0, 1, 0);

            mUpDimDistance = GetUpDimDistance(leftWorkPoint);
            CDimTools.GetInstance().DrawDimensionSet(mViewBase, pointList, upDimVector, mUpDimDistance, CCommonPara.mMainSizeDimPath);            
        }

        /// <summary>
        /// 标注工作点到工作点之间的尺寸;
        /// </summary>
        public void DrawWorkPointToWorkPointDimX()
        {
            CBeamFrontViewSetting beamFrontViewSetting = CBeamDimSetting.GetInstance().mFrontViewSetting;

            PointList pointList = new PointList();

            //1.判断工作点与工作点之间是否需要标注，需要标注的话是一道单尺寸;
            bool bNeedDimWpToWp = beamFrontViewSetting.FindDimValueByName(CBeamFrontViewSetting.mstrWorkPointToWorkPoint);

            bool bMarkValue = beamFrontViewSetting.FindMarkValueByName(CBeamFrontViewSetting.mstrWorkPointToWorkPoint);
            mMainBeam.SetNeedAddMarkFlag(bMarkValue);

            if (bNeedDimWpToWp)
            {
                Point leftWorkPoint = CMrMainBeam.GetInstance().mLeftWorkPoint;
                Point rightWorkPoint = CMrMainBeam.GetInstance().mRightWorkPoint;

                pointList.Add(leftWorkPoint);
                pointList.Add(rightWorkPoint);

                Vector upDimVector = new Vector(0, 1, 0);
                mUpDimDistance = GetUpDimDistance(leftWorkPoint);
                CDimTools.GetInstance().DrawDimensionSet(mViewBase, pointList, upDimVector, mUpDimDistance, CCommonPara.mMainSizeDimPath);
            }
        }

        /// <summary>
        /// 标注切割的情况;
        /// </summary>
        public void DrawCuttingDim()
        {
            CBeamFrontViewSetting beamFrontViewSetting = CBeamDimSetting.GetInstance().mFrontViewSetting;

            //判断是否需要标注切割部分，切割需要采用单一尺寸来标注;
            bool bNeedDimCutting = beamFrontViewSetting.FindDimValueByName(CBeamFrontViewSetting.mstrCutting);

            if (!bNeedDimCutting)
            {
                return;
            }
            //1.标注左上方的切割;
            //首先判断左上角是否存在小切口，如果存在小切口则不为切割;
            if (!CMrMainBeam.GetInstance().JudgeLeftTopIncision())
            {
                Point minXmaxYPoint = mMainBeam.GetMinXMaxYPoint();
                Point maxYminXPoint = mMainBeam.GetMaxYMinXPoint();

                if (CDimTools.GetInstance().CompareTwoDoubleValue(minXmaxYPoint.X, maxYminXPoint.X) != 0)
                {
                    //X方向的标注;
                    PointList pointList = new PointList();
                    pointList.Add(minXmaxYPoint);
                    pointList.Add(maxYminXPoint);
                    Vector dimVector = new Vector(0, 1, 0);
                    double upDimDistance = CCommonPara.mDefaultDimDistance;
                    CDimTools.GetInstance().DrawDimensionSet(mViewBase, pointList, dimVector, upDimDistance, CCommonPara.mSizeDimPath);

                    //Y方向的标注;
                    dimVector = new Vector(-1, 0, 0);
                    double leftDimDistance = Math.Abs(mMainBeam.GetMinXPoint().X - pointList[0].X) + CCommonPara.mDefaultDimDistance;
                    CDimTools.GetInstance().DrawDimensionSet(mViewBase, pointList, dimVector, leftDimDistance, CCommonPara.mSizeDimPath);
                    mLeftCuttingDimPoint = pointList[0];
                    mLeftCuttingDimDistance = leftDimDistance;
                }
            }
            //2.判断左下方的切割;
            if (!CMrMainBeam.GetInstance().JudgeLeftBottomIncision())
            {
                Point minXminYPoint = mMainBeam.GetMinXMinYPoint();
                Point minYminXPoint = mMainBeam.GetMinYMinXPoint();

                if (CDimTools.GetInstance().CompareTwoDoubleValue(minXminYPoint.X, minYminXPoint.X) != 0)
                {
                    //X方向的标注;
                    PointList pointList = new PointList();
                    pointList.Add(minXminYPoint);
                    pointList.Add(minYminXPoint);
                    Vector dimVector = new Vector(0, -1, 0);
                    double downDimDistance = CCommonPara.mDefaultDimDistance;
                    CDimTools.GetInstance().DrawDimensionSet(mViewBase, pointList, dimVector, downDimDistance, CCommonPara.mSizeDimPath);

                    //Y方向的标注;
                    dimVector = new Vector(-1, 0, 0);
                    double leftDimDistance = Math.Abs(mMainBeam.GetMinXPoint().X - pointList[0].X) + CCommonPara.mDefaultDimDistance;
                    CDimTools.GetInstance().DrawDimensionSet(mViewBase, pointList, dimVector, leftDimDistance, CCommonPara.mSizeDimPath);
                    mLeftCuttingDimPoint = pointList[0];
                    mLeftCuttingDimDistance = leftDimDistance;
                }
            }
            //3.判断右上方的切割;
            if (!CMrMainBeam.GetInstance().JudgeRightTopIncision())
            {
                Point maxXmaxYPoint = mMainBeam.GetMaxXMaxYPoint();
                Point maxYmaxXPoint = mMainBeam.GetMaxYMaxXPoint();

                if (CDimTools.GetInstance().CompareTwoDoubleValue(maxXmaxYPoint.X, maxYmaxXPoint.X) != 0)
                {
                    //X方向的标注;
                    PointList pointList = new PointList();
                    pointList.Add(maxXmaxYPoint);
                    pointList.Add(maxYmaxXPoint);
                    Vector dimVector = new Vector(0, 1, 0);
                    double upDimDistance = CCommonPara.mDefaultDimDistance;
                    CDimTools.GetInstance().DrawDimensionSet(mViewBase, pointList, dimVector, upDimDistance, CCommonPara.mSizeDimPath);

                    //Y方向的标注;
                    dimVector = new Vector(1, 0, 0);
                    double rightDimDistance = Math.Abs(mMainBeam.GetMaxXPoint().X - pointList[0].X) + CCommonPara.mDefaultDimDistance;
                    CDimTools.GetInstance().DrawDimensionSet(mViewBase, pointList, dimVector, rightDimDistance, CCommonPara.mSizeDimPath);
                    mRightCuttingDimPoint = pointList[0];
                    mRightCuttingDimDistance = rightDimDistance;
                }
            }
            //4.判断右下方的切割;
            if (!CMrMainBeam.GetInstance().JudgeRightBottomIncision())
            {
                Point maxXminYPoint = mMainBeam.GetMaxXMinYPoint();
                Point minYmaxXPoint = mMainBeam.GetMinYMaxXPoint();

                if (CDimTools.GetInstance().CompareTwoDoubleValue(maxXminYPoint.X, minYmaxXPoint.X) != 0)
                {
                    //X方向的标注;
                    PointList pointList = new PointList();
                    pointList.Add(maxXminYPoint);
                    pointList.Add(minYmaxXPoint);
                    Vector dimVector = new Vector(0, -1, 0);
                    double downDimDistance = CCommonPara.mDefaultDimDistance;
                    CDimTools.GetInstance().DrawDimensionSet(mViewBase, pointList, dimVector, downDimDistance, CCommonPara.mSizeDimPath);

                    //Y方向的标注;
                    dimVector = new Vector(1, 0, 0);
                    double rightDimDistance = Math.Abs(mMainBeam.GetMaxXPoint().X - pointList[0].X) + CCommonPara.mDefaultDimDistance;
                    CDimTools.GetInstance().DrawDimensionSet(mViewBase, pointList, dimVector, rightDimDistance, CCommonPara.mSizeDimPath);
                    mRightCuttingDimPoint = pointList[0];
                    mRightCuttingDimDistance = rightDimDistance;
                }
            }
        }

        /// <summary>
        /// 绘制零件中Bolt的标注;
        /// </summary>
        public void DrawPartBoltDim()
        {
            //移除主梁;
            mMrPartList.Remove(mMainBeam);

            foreach (CMrPart mrPart in mMrPartList)
            {
                List<CMrDimSet> boltDimSetList = mrPart.GetBeamFrontViewInfo().GetPartBoltDimSetList();

                if (boltDimSetList == null || boltDimSetList.Count == 0)
                {
                    continue;
                }

                if (mrPart.GetBeamFrontViewInfo().mPostionType == MrPositionType.UP)
                {
                    mbHaveUpConnectPlateDim = true;
                }
                else if (mrPart.GetBeamFrontViewInfo().mPostionType == MrPositionType.DOWM)
                {
                    mbHaveDownConnectPlateDim = true;
                }

                foreach (CMrDimSet mrDimSet in boltDimSetList)
                {
                    if (mrDimSet == null || mrDimSet.Count <= 1)
                    {
                        continue;
                    }
                    List<Point> dimPointList = mrDimSet.GetDimPointList();
                    
                    PointList pointList = new PointList();

                    foreach (Point point in dimPointList)
                    {
                        pointList.Add(point);
                    }
                    double dimDistance = mrDimSet.mDimDistance;
                    Vector dimVector = mrDimSet.mDimVector;
                    CDimTools.GetInstance().DrawDimensionSet(mViewBase, pointList, dimVector, dimDistance, CCommonPara.mSizeDimPath);
                }
            }
            mMrPartList.Add(mMainBeam);
        }

        /// <summary>
        /// 绘制主梁螺钉在Y向的标注;
        /// </summary>
        public void DrawMainPartBoltDimY()
        {
            CBeamFrontViewSetting beamFrontViewSetting = CBeamDimSetting.GetInstance().mFrontViewSetting;

            bool bNeedMainBeamBoltDim = beamFrontViewSetting.FindDimValueByName(CBeamFrontViewSetting.mstrBolt);

            if (!bNeedMainBeamBoltDim)
            {
                return;
            }

            List<CMrDimSet> boltDimSetList = GetMainPartBoltDimSetY();

            if (boltDimSetList == null || boltDimSetList.Count == 0)
            {
                return;
            }

            foreach (CMrDimSet mrDimSet in boltDimSetList)
            {
                if (mrDimSet == null || mrDimSet.Count <= 1)
                {
                    continue;
                }
                List<Point> dimPointList = mrDimSet.GetDimPointList();
                PointList pointList = new PointList();
                foreach (Point point in dimPointList)
                {
                    pointList.Add(point);
                }

                double dimDistance = mrDimSet.mDimDistance;
                Vector dimVector = mrDimSet.mDimVector;
                CDimTools.GetInstance().DrawDimensionSet(mViewBase, pointList, dimVector, dimDistance, CCommonPara.mSizeDimPath);
            }
        }

        /// <summary>
        /// 获得主梁螺钉在Y向的标注集;
        /// </summary>
        /// <returns></returns>
        private List<CMrDimSet> GetMainPartBoltDimSetY()
        {
            double halfBeamHeight = mMainBeam.GetYSpaceValue() / 2.0;
            double halfBeamWidth = mMainBeam.GetXSpaceValue() / 2.0;

            List<CMrDimSet> mrDimSetList = new List<CMrDimSet>();

            List<CMrBoltArray> mrLeftBoltArrayList = new List<CMrBoltArray>();
            List<CMrBoltArray> mrRightBoltArrayList = new List<CMrBoltArray>();

            GetMainPartBorderBoltArrayList(ref mrLeftBoltArrayList, ref mrRightBoltArrayList);

            //主梁左侧的标注集;
            if (mrLeftBoltArrayList.Count > 0)
            {
                //1.左侧的螺钉的标注集;
                CMrDimSet mrLeftDimSet = new CMrDimSet();

                foreach (CMrBoltArray mrBoltArray in mrLeftBoltArrayList)
                {
                    List<Point> minXPointList = mrBoltArray.GetMinXPointList();
                    mrLeftDimSet.AddRange(minXPointList);
                }

                Point maxYminXPoint = mMainBeam.GetMaxYMinXPoint();
                Point minYminXPoint = mMainBeam.GetMinYMinXPoint();

                mrLeftDimSet.AddPoint(maxYminXPoint);
                mrLeftDimSet.AddPoint(minYminXPoint);

                List<Point> pointList = mrLeftDimSet.GetDimPointList();
                CDimTools.GetInstance().GetMinXPoint(pointList);

                double leftDimDistance = Math.Abs(CCommonPara.mViewMinX - pointList[0].X) + CCommonPara.mDefaultDimDistance;

                if (mLeftCuttingDimDistance > 0)
                {
                    leftDimDistance = leftDimDistance + CCommonPara.mDefaultTwoDimLineGap + mLeftCuttingDimPoint.X - pointList[0].X;
                }

                mrLeftDimSet.mDimDistance = leftDimDistance;
                mrLeftDimSet.mDimVector = new Vector(-1, 0, 0);
                mrDimSetList.Add(mrLeftDimSet);
            }
            //主梁右侧的标注集;
            if (mrRightBoltArrayList.Count > 0)
            {
                //1.右侧的标注集;
                CMrDimSet mrRightDimSet = new CMrDimSet();

                foreach (CMrBoltArray mrBoltArray in mrRightBoltArrayList)
                {
                    List<Point> maxXPointList = mrBoltArray.GetMaxXPointList();
                    mrRightDimSet.AddRange(maxXPointList);
                }
                
                Point maxYmaxXPoint = mMainBeam.GetMaxYMaxXPoint();
                Point minYmaxXPoint = mMainBeam.GetMinYMaxXPoint();

                mrRightDimSet.AddPoint(maxYmaxXPoint);
                mrRightDimSet.AddPoint(minYmaxXPoint);

                List<Point> pointList = mrRightDimSet.GetDimPointList();
                CDimTools.GetInstance().GetMinXPoint(pointList);

                double rightDimDistance = Math.Abs(CCommonPara.mViewMaxX - pointList[0].X) + CCommonPara.mDefaultDimDistance;

                if (mRightCuttingDimDistance > 0.0)
                {
                    rightDimDistance = rightDimDistance + CCommonPara.mDefaultTwoDimLineGap + mRightCuttingDimPoint.X - pointList[0].X;
                }
                mrRightDimSet.mDimDistance = rightDimDistance;
                mrRightDimSet.mDimVector = new Vector(1, 0, 0);
                mrDimSetList.Add(mrRightDimSet);
            }
            //3.主梁上剩下未标注的螺钉组;
            List<CMrBoltArray> lastMrBoltArrayList = new List<CMrBoltArray>();

            //构建具有相同属性螺钉组的集合;
            List<CMrBoltArrayGroup> mrBoltArrayGroupList = new List<CMrBoltArrayGroup>();

            //构建主梁中间剩下的螺钉组的链表;
            foreach (CMrBoltArray mrBoltArray in mMainBeam.GetBoltArrayList())
            {
                if (mrLeftBoltArrayList.Contains(mrBoltArray) || mrRightBoltArrayList.Contains(mrBoltArray))
                {
                    continue;
                }
                if (mrBoltArray.mBoltArrayShapeType != MrBoltArrayShapeType.ARRAY)
                {
                    continue;
                }
                if (!CDimTools.GetInstance().IsTwoVectorParallel(mrBoltArray.mNormal, new Vector(0, 0, 1)))
                {
                    continue;
                }
                lastMrBoltArrayList.Add(mrBoltArray);
            }

            //构建剩下螺钉组的组合;
            int nCount = lastMrBoltArrayList.Count;

            //螺钉的唯一标识符与螺钉组集合的映射表;
            Dictionary<Identifier, CMrBoltArrayGroup> mapIdentifierToBoltArrayGroup = new Dictionary<Identifier, CMrBoltArrayGroup>();

            for (int i = 0; i < nCount; i++)
            {
                CMrBoltArray firstBoltArray = lastMrBoltArrayList[i];

                //如果该螺钉已经加到螺钉组的集合中则返回继续查找;
                if (mapIdentifierToBoltArrayGroup.ContainsKey(firstBoltArray.mBoltArrayInModel.Identifier))
                {
                    continue;
                }

                CMrBoltArrayGroup mrBoltArrayGroup = new CMrBoltArrayGroup();
                mrBoltArrayGroup.AppendMrBoltArray(firstBoltArray);
                mrBoltArrayGroupList.Add(mrBoltArrayGroup);
                mapIdentifierToBoltArrayGroup.Add(firstBoltArray.mBoltArrayInModel.Identifier, mrBoltArrayGroup);

                for (int j = i + 1; j < nCount; j++)
                {
                    CMrBoltArray secondBoltArray = lastMrBoltArrayList[j];

                    if (IsMrBoltArrayCanAddToMrBoltArrayGroup(secondBoltArray, mrBoltArrayGroup))
                    {
                        mrBoltArrayGroup.AppendMrBoltArray(secondBoltArray);
                        mapIdentifierToBoltArrayGroup.Add(secondBoltArray.mBoltArrayInModel.Identifier, mrBoltArrayGroup);
                    }
                }
            }
            CBeamFrontViewSetting beamFrontViewSetting = CBeamDimSetting.GetInstance().mFrontViewSetting;

            bool bNeedBoltClosed = beamFrontViewSetting.FindDimValueByName(CBeamFrontViewSetting.mstrBoltClosed);

            //根据组合好的螺钉组集合来进行标注;
            foreach (CMrBoltArrayGroup mrBoltArrayGroup in mrBoltArrayGroupList)
            {
                CMrDimSet yMrDimSet = new CMrDimSet();

                foreach (CMrBoltArray mrBoltArray in mrBoltArrayGroup.mrBoltArrayList)
                {
                    List<Point> minXPointList = mrBoltArray.GetMinXPointList();
                    yMrDimSet.AddRange(minXPointList);
                }
                
                yMrDimSet.AddPoint(new Point(yMrDimSet.GetDimPointList()[0].X, mMainBeam.GetMaxYPoint().Y, 0));

                if (bNeedBoltClosed)
                {
                    yMrDimSet.AddPoint(new Point(yMrDimSet.GetDimPointList()[0].X, mMainBeam.GetMinYPoint().Y, 0));
                }
                yMrDimSet.mDimDistance = CCommonPara.mDefaultDimDistance;
                yMrDimSet.mDimVector = new Vector(-1, 0, 0);
                mrDimSetList.Add(yMrDimSet);
            }
            return mrDimSetList;
        }

        /// <summary>
        /// 获得主梁上的左边和右边螺钉组的链表;
        /// </summary>
        /// <returns></returns>
        private void GetMainPartBorderBoltArrayList(ref List<CMrBoltArray> leftBoltArrayList,
                                                    ref List<CMrBoltArray> rightBoltArrayList)
        {
            double mainBeamMinX = mMainBeam.GetMinXPoint().X;
            double mainBeamMaxX = mMainBeam.GetMaxXPoint().X;

            List<CMrBoltArray> mrBoltArrayList = mMainBeam.GetBoltArrayList();

            foreach (CMrBoltArray mrBoltArray in mrBoltArrayList)
            {
                if (mrBoltArray.mBoltArrayShapeType != MrBoltArrayShapeType.ARRAY)
                {
                    continue;
                }
                if (!CDimTools.GetInstance().IsTwoVectorParallel(mrBoltArray.mNormal, new Vector(0, 0, 1)))
                {
                    continue;
                }
                Point minXPoint = mrBoltArray.GetMinXPoint();
                Point maxXPoint = mrBoltArray.GetMaxXPoint();

                if (Math.Abs(minXPoint.X - mainBeamMinX) < CCommonPara.mDefaultDimDistanceThreshold)
                {
                    leftBoltArrayList.Add(mrBoltArray);
                }
                if (Math.Abs(maxXPoint.X - mainBeamMaxX) < CCommonPara.mDefaultDimDistanceThreshold)
                {
                    rightBoltArrayList.Add(mrBoltArray);
                }
            }
        }

        /// <summary>
        /// 判断该螺钉是否可以加到该螺钉组中,主要是判断螺钉与该螺钉组的方向向量是否相同，并且距离小于阈值距离;
        /// </summary>
        /// <param name="mrBoltArray"></param>
        /// <param name="mrBoltArrayGroup"></param>
        /// <returns></returns>
        private bool IsMrBoltArrayCanAddToMrBoltArrayGroup(CMrBoltArray mrBoltArray, CMrBoltArrayGroup mrBoltArrayGroup)
        {
            if (mrBoltArrayGroup.mrBoltArrayList.Count == 0)
            {
                return true;
            }

            Vector boltDirectionNormal = mrBoltArray.mDirectionNormal;
            Vector groupDirectionNormal = mrBoltArrayGroup.mrBoltArrayList[0].mDirectionNormal;

            if (!CDimTools.GetInstance().IsTwoVectorParallel(boltDirectionNormal, groupDirectionNormal))
            {
                return false;
            }

            Point boltPoint = mrBoltArray.GetMinXMaxYPoint();

            foreach (CMrBoltArray groupBoltArray in mrBoltArrayGroup.mrBoltArrayList)
            {
                Point groupBoltPoint = groupBoltArray.GetMinXMaxYPoint();

                if (Math.Abs(boltPoint.X - groupBoltPoint.X) < CCommonPara.mDefaultTwoBoltArrayGap)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 绘制主部件中间腹板在Y向标注;
        /// </summary>
        public void DrawMainPartMiddlePlateDimY()
        {
            CBeamFrontViewSetting beamFrontViewSetting = CBeamDimSetting.GetInstance().mFrontViewSetting;

            bool bNeedMainBeamMiddlePlate = beamFrontViewSetting.FindDimValueByName(CBeamFrontViewSetting.mstrMainBeamMiddlePart);

            if (!bNeedMainBeamMiddlePlate)
            {
                return;
            }

            List<CMrDimSet> middlePartDimSetList = GetMainPartMiddlePartDimY();

            if (middlePartDimSetList == null || middlePartDimSetList.Count == 0)
            {
                return;
            }

            foreach (CMrDimSet mrDimSet in middlePartDimSetList)
            {
                if (mrDimSet == null || mrDimSet.Count <= 1)
                {
                    continue;
                }
                List<Point> dimPointList = mrDimSet.GetDimPointList();
                PointList pointList = new PointList();
                foreach (Point point in dimPointList)
                {
                    pointList.Add(point);
                }

                double dimDistance = mrDimSet.mDimDistance;
                Vector dimVector = mrDimSet.mDimVector;
                CDimTools.GetInstance().DrawDimensionSet(mViewBase, pointList, dimVector, dimDistance, CCommonPara.mSizeDimPath);
            }
        }

        /// <summary>
        /// 获得主部件中间零部件在Y向上的标注;
        /// </summary>
        /// <returns></returns>
        private List<CMrDimSet> GetMainPartMiddlePartDimY()
        {
            List<CMrDimSet> mrDimSetList = new List<CMrDimSet>();
            List<CMrPart> middlePlateList = new List<CMrPart>();

            double mainBeamMinY = mMainBeam.GetMinYPoint().Y + CMrMainBeam.GetInstance().mFlangeThickness;
            double mainBeamMaxY = mMainBeam.GetMaxYPoint().Y - CMrMainBeam.GetInstance().mFlangeThickness;

            foreach (CMrPart mrPart in mMrPartList)
            {
                if (!CDimTools.GetInstance().IsTwoVectorParallel(mrPart.mNormal, new Vector(0, 1, 0)))
                {
                    continue;
                }

                double mrPartMaxY = mrPart.GetMaxYPoint().Y;
                double mrPartMinY = mrPart.GetMinYPoint().Y;

                if (CDimTools.GetInstance().CompareTwoDoubleValue(mrPartMaxY, mainBeamMaxY) < 0 &&
                    CDimTools.GetInstance().CompareTwoDoubleValue(mrPartMinY, mainBeamMinY) > 0)
                {
                    middlePlateList.Add(mrPart);

                }
            }

            //构建中间腹板上零件组的组合;
            int nCount = middlePlateList.Count;

            //零件的唯一标识符与零件组集合的映射表;
            Dictionary<Identifier, CMrPartGroup> mapIdentifierToPartGroup = new Dictionary<Identifier, CMrPartGroup>();

            //构建具有相同属性零件组的集合;
            List<CMrPartGroup> mrPartGroupList = new List<CMrPartGroup>();

            //把中间腹板上距离很近的零部件组合起来;
            for (int i = 0; i < nCount; i++)
            {
                CMrPart firstPart = middlePlateList[i];

                //如果该螺钉已经加到螺钉组的集合中则返回继续查找;
                if (mapIdentifierToPartGroup.ContainsKey(firstPart.mPartInModel.Identifier))
                {
                    continue;
                }

                CMrPartGroup mrPartGroup = new CMrPartGroup();
                mrPartGroup.AppendMrPart(firstPart);

                mrPartGroupList.Add(mrPartGroup);
                mapIdentifierToPartGroup.Add(firstPart.mPartInModel.Identifier, mrPartGroup);

                for (int j = i + 1; j < nCount; j++)
                {
                    CMrPart secondPart = middlePlateList[j];

                    if (IsMrPartCanAddToMrPartGroup(secondPart, mrPartGroup))
                    {
                        mrPartGroup.AppendMrPart(secondPart);
                        mapIdentifierToPartGroup.Add(secondPart.mPartInModel.Identifier, mrPartGroup);
                    }
                }
            }

            //对中间腹板上的零部件进行标注;
            foreach (CMrPartGroup mrPartGroup in mrPartGroupList)
            {
                CMrDimSet yMrDimSet = new CMrDimSet();

                List<CMrPart> mrPartList = mrPartGroup.mrPartList;

                foreach (CMrPart mrPart in mrPartList)
                {
                    yMrDimSet.AddPoint(mrPart.GetMaxXMaxYPoint());
                }

                yMrDimSet.AddPoint(new Point(yMrDimSet.GetDimPointList()[0].X, mMainBeam.GetMaxYPoint().Y, 0));
                yMrDimSet.mDimVector = new Vector(1, 0, 0);
                yMrDimSet.mDimDistance = CCommonPara.mDefaultDimDistance;
                mrDimSetList.Add(yMrDimSet);
            }
            return mrDimSetList;
        }

        /// <summary>
        /// 判断该零部件是否可以加入到零件组中;
        /// </summary>
        /// <param name="mrPart"></param>
        /// <param name="mrPartGroup"></param>
        /// <returns></returns>
        private bool IsMrPartCanAddToMrPartGroup(CMrPart mrPart, CMrPartGroup mrPartGroup)
        {
            if (mrPartGroup.mrPartList.Count == 0)
            {
                return true;
            }

            foreach (CMrPart groupPart in mrPartGroup.mrPartList)
            {
                if (Math.Abs(mrPart.mMidPoint.X - groupPart.mMidPoint.X) < CCommonPara.mDefalutTwoPartGap)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 获得向上标注的距离;
        /// </summary>
        /// <returns></returns>
        private double GetUpDimDistance(Point firstDimPoint)
        {
            if (mUpDimDistance == 0)
            {
                if (mbHaveUpConnectPlateDim)
                {
                    mUpDimDistance = Math.Abs(CCommonPara.mViewMaxY - firstDimPoint.Y) + CCommonPara.mDefaultDimDistance + CCommonPara.mDefaultTwoDimLineGap;
                }
                else
                {
                    mUpDimDistance = Math.Abs(CCommonPara.mViewMaxY - firstDimPoint.Y) + CCommonPara.mDefaultDimDistance;
                }
            }
            else
            {
                mUpDimDistance = mUpDimDistance + CCommonPara.mDefaultTwoDimLineGap + mUpDimFirstPoint.Y - firstDimPoint.Y;
            }
            mUpDimFirstPoint = firstDimPoint;

            return mUpDimDistance;
        }

        /// <summary>
        /// 获得向下的标注;
        /// </summary>
        /// <param name="firstDimPoint"></param>
        /// <returns></returns>
        private double GetDownDimDistance(Point firstDimPoint)
        {
            if (mDownDimDistance == 0)
            {
                if (mbHaveDownConnectPlateDim)
                {
                    mDownDimDistance = Math.Abs(CCommonPara.mViewMinY - firstDimPoint.Y) + CCommonPara.mDefaultDimDistance + CCommonPara.mDefaultTwoDimLineGap;
                }
                else
                {
                    mDownDimDistance = Math.Abs(CCommonPara.mViewMinY - firstDimPoint.Y) + CCommonPara.mDefaultDimDistance;
                }
            }
            else
            {
                mDownDimDistance = mDownDimDistance + CCommonPara.mDefaultTwoDimLineGap + mDownDimFirstPoint.Y - firstDimPoint.Y;
            }
            mDownDimFirstPoint = firstDimPoint;

            return mDownDimDistance;
        }

        /// <summary>
        /// 绘制主梁中间角钢的标注;
        /// </summary>
        private void DrawMainPartMiddleAngleSheetDimY()
        {
            CBeamFrontViewSetting beamFrontViewSetting = CBeamDimSetting.GetInstance().mFrontViewSetting;

            bool bNeedMainBeamMiddleAngleSheet = beamFrontViewSetting.FindDimValueByName(CBeamFrontViewSetting.mstrAngleSheet);

            if (!bNeedMainBeamMiddleAngleSheet)
            {
                return;
            }

            List<CMrPart> mrAngleSheetList = new List<CMrPart>();

            foreach (CMrPart mrPart in mMrPartList)
            {
                if (!CDimTools.GetInstance().IsPartTheAngleSteel(mrPart))
                {
                    continue;
                }
                if (!CDimTools.GetInstance().IsTwoVectorParallel(mrPart.mNormal, new Vector(0, 0, 1)))
                {
                    continue;
                }
                if(mrPart.GetBeamFrontViewInfo().mPostionType==MrPositionType.UP ||
                   mrPart.GetBeamFrontViewInfo().mPostionType==MrPositionType.DOWM)
                {
                    continue;
                }
                mrAngleSheetList.Add(mrPart);
            }

            foreach (CMrPart mrPart in mrAngleSheetList)
            {
                CMrDimSet yMrDimSet = new CMrDimSet();

                //如果存在螺钉则把螺钉加到标注集中;
                foreach (CMrBoltArray mrBoltArray in mrPart.GetBoltArrayList())
                {
                    if (CDimTools.GetInstance().IsTwoVectorParallel(mrBoltArray.mNormal, new Vector(0, 0, 1)))
                    {
                        List<Point> maxXPointList = mrBoltArray.GetMaxXPointList();
                        yMrDimSet.AddRange(maxXPointList);
                    }
                }

                //判断出角钢的厚度方向;
                Point maxYPoint = mrPart.GetMaxYPoint();
                Point minYPoint = mrPart.GetMinYPoint();
                Point maxZmaxYPoint = mrPart.GetMaxZMaxYPoint();
                Point maxZminYPoint = mrPart.GetMaxZMinYPoint();

                if (CDimTools.GetInstance().CompareTwoDoubleValue(maxYPoint.Y, maxZmaxYPoint.Y) == 0)
                {
                    yMrDimSet.AddPoint(mrPart.GetMaxYMaxXPoint());
                }
                else if (CDimTools.GetInstance().CompareTwoDoubleValue(minYPoint.Y, maxZminYPoint.Y) == 0)
                {
                    yMrDimSet.AddPoint(mrPart.GetMinYMaxXPoint());
                }
                else
                {
                    yMrDimSet.AddPoint(mrPart.GetMaxYMaxXPoint());
                }

                yMrDimSet.AddPoint(new Point(mrPart.GetMaxYMaxXPoint().X,CMrMainBeam.GetInstance().GetMaxYPoint().Y,0));

                yMrDimSet.mDimDistance = CCommonPara.mDefaultDimDistance;
                yMrDimSet.mDimVector = new Vector(1, 0, 0);

                PointList pointList=new PointList();

                foreach (Point pt in yMrDimSet.GetDimPointList())
                {
                    pointList.Add(pt);
                }

                CDimTools.GetInstance().DrawDimensionSet(mViewBase,pointList,yMrDimSet.mDimVector,yMrDimSet.mDimDistance,CCommonPara.mSizeDimPath);
            }
        }

        /// <summary>
        /// 绘制零件标记;
        /// </summary>
        protected void DrawPartMark()
        {
            mViewBase.Select();

            DrawingHandler drawingHandler = new DrawingHandler();
            TSD.UI.DrawingObjectSelector DS = drawingHandler.GetDrawingObjectSelector();

            foreach (CMrPart mrPart in mMrPartList)
            {
                if (mrPart.IsNeedAddMark())
                {
                    DS.SelectObject(mrPart.mPartInDrawing);
                }
            }

            CDimTools.GetInstance().DrawMarkByMacro();
        }

        /// <summary>
        /// 绘制主梁中螺钉组的标记;
        /// </summary>
        private void DrawMainPartBoltMark()
        {
            List<CMrBoltArray> mrBoltArrayList = mMainBeam.GetBoltArrayList();
            
            Vector zVector = new Vector(0, 0, 1);

            DrawingHandler drawingHandler = new DrawingHandler();
            TSD.UI.DrawingObjectSelector DS = drawingHandler.GetDrawingObjectSelector();

            foreach (CMrBoltArray mrBoltArray in mrBoltArrayList)
            {
                //只有与Z轴平行的螺钉组才进行编号;
                if (!CDimTools.GetInstance().IsTwoVectorParallel(zVector, mrBoltArray.mNormal))
                {
                    continue;
                }

                DS.SelectObject(mrBoltArray.mBoltInDrawing);
            }

            CDimTools.GetInstance().DrawMarkByMacro();
        }

        /// <summary>
        /// 获得主梁的右侧挡板;
        /// </summary>
        /// <returns></returns>
        private CMrPart GetRightBackPlate()
        {
            foreach (CMrPart mrPart in mMrPartList)
            {
                int nRes = IsPartTheBackPlate(mrPart);

                if (nRes == 0)
                {
                    continue;
                }
                if (nRes == 2)
                {
                    return mrPart;
                }
            }

            return null;
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
    }
}
