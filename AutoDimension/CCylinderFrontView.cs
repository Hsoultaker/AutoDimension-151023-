using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Tekla.Structures.Drawing;
using Tekla.Structures.Model;
using Tekla.Structures.Geometry3d;
using Tekla.Structures;

using TSD = Tekla.Structures.Drawing;
using TSM = Tekla.Structures.Model;
using AutoDimension.Entity;
using System.Threading;
using System.Windows.Forms;

namespace AutoDimension
{
    public class CCylinderFrontView:CView
    {
        /// <summary>
        /// 零件标记时需要去掉重复标注的点;
        /// </summary>
        private Dictionary<String, bool> mDicMarkDimPoints = new Dictionary<String, bool>();

        /// <summary>
        /// 柱子左侧牛腿或连接板;
        /// </summary>
        private List<CMrPart> mLeftConnectPlateList = new List<CMrPart>();

        /// <summary>
        /// 柱子右侧牛腿或连接板;
        /// </summary>
        private List<CMrPart> mRightConnectPlateList = new List<CMrPart>();

        /// <summary>
        /// 柱子的左翼缘与轴线的交点;
        /// </summary>
        private List<Point> mLeftIntersectionPointList = new List<Point>();

        /// <summary>
        /// 柱子的右翼缘与轴线的交点;
        /// </summary>
        private List<Point> mRightIntersectionPointList = new List<Point>();

        /// <summary>
        /// 所有中间垂直连接板链表;
        /// </summary>
        private List<CMrPart> mMiddleConnectPlateList = new List<CMrPart>();

        /// <summary>
        /// 所有外侧竖直连接板的链表;
        /// </summary>
        private List<CMrPart> mOutsideVerticalConnectPlateList = new List<CMrPart>();

        /// <summary>
        /// 左侧的标注距离;
        /// </summary>
        private double mLeftDimDistance = 0.0;

        /// <summary>
        /// 右侧的标注距离;
        /// </summary>
        private double mRightDimDistance = 0.0;

        /// <summary>
        /// 左侧标注的第一个点;
        /// </summary>
        private Point mLeftDimFirstPoint = new Point();

        /// <summary>
        /// 右侧标注的第一个点;
        /// </summary>
        private Point mRightDimFirstPoint = new Point();

        /// <summary>
        /// 构造函数;
        /// </summary>
        /// <param name="viewBase"></param>
        /// <param name="model"></param>
        public CCylinderFrontView(TSD.View viewBase, Model model) : base(viewBase)
        {
            mViewBase = viewBase;
            mModel = model;
        }

        /// <summary>
        /// 初始化前视图;
        /// </summary>
        public void InitFrontView()
        {
            if (mViewBase == null)
            {
                return;
            }

            CDimTools dimTools = CDimTools.GetInstance();

            //获取所有螺钉的数据字典;
            Dictionary<Identifier, TSD.Bolt> dicIdentifierBolt = dimTools.GetAllBoltInDrawing(mViewBase);

            //获取所有Drawing视图中的Part;
            List<TSD.Part> partList = dimTools.GetAllPartInDrawing(mViewBase);

            foreach (TSD.Part partInDrawing in partList)
            {
                //1.获取部件的信息;
                TSM.ModelObject modelObjectInModel = dimTools.TransformDrawingToModel(partInDrawing);
                TSM.Part partInModel = modelObjectInModel as TSM.Part;
                CMrPart mrPart = null;

                if (CMrMainBeam.GetInstance().mPartInModel.Identifier.GUID == partInModel.Identifier.GUID)
                {
                    mrPart = CMrMainBeam.GetInstance();
                    mMainBeam = CMrMainBeam.GetInstance();
                    mMainBeam.mPartInDrawing = partInDrawing;
                }
                else
                {
                    mrPart = new CMrPart(partInModel, partInDrawing);
                }

                dimTools.InitMrPart(modelObjectInModel, mViewBase, mrPart);
                AppendMrPart(mrPart);

                //2.获取部件中的所有螺钉组;
                List<BoltArray> boltArrayList = dimTools.GetAllBoltArray(partInModel);

                foreach (BoltArray boltArray in boltArrayList)
                {
                    TSD.Bolt boltInDrawing = dicIdentifierBolt[boltArray.Identifier];
                    CMrBoltArray mrBoltArray = new CMrBoltArray(boltArray, boltInDrawing);
                    dimTools.InitMrBoltArray(boltArray, mViewBase, mrBoltArray);
                    mrPart.AppendMrBoltArray(mrBoltArray);
                }
            }
        }

        /// <summary>
        /// 初始化左侧或则右侧牛腿或连接板的映射表;
        /// </summary>
        /// <param name="mrPart"></param>
        private void InitLeftAndRightPlateMap(CMrPart mrPart)
        {
            Vector normal = mrPart.mNormal;

            //H型型钢;
            if (CDimTools.GetInstance().IsTwoVectorParallel(normal, new Vector(0, 0, 1))
                && CDimTools.GetInstance().IsPartTheHProfileSheet(mrPart))
            {
                if (mrPart.GetCylinderFrontViewInfo().mPostionType == MrPositionType.LEFT)
                {
                    mLeftConnectPlateList.Add(mrPart);
                }
                if (mrPart.GetCylinderFrontViewInfo().mPostionType == MrPositionType.RIGHT)
                {
                    mRightConnectPlateList.Add(mrPart);
                }
            }
            //板的法向与Y轴平行或者法向在XY平面内才可以进行对称性判断;
            else if (CDimTools.GetInstance().IsTwoVectorParallel(normal, new Vector(0, 1, 0))||
                     CDimTools.GetInstance().IsVectorInXYPlane(normal))
            {
                if (mrPart.GetCylinderFrontViewInfo().mPostionType == MrPositionType.LEFT)
                {
                    mLeftConnectPlateList.Add(mrPart);
                }
                if (mrPart.GetCylinderFrontViewInfo().mPostionType == MrPositionType.RIGHT)
                {
                    mRightConnectPlateList.Add(mrPart);
                }
            }
        }

        /// <summary>
        /// 初始化中间连接板的对称性,主要判断中间连接板的Y值与牛腿的Y值是否相等;
        /// </summary>
        private void InitMiddleConnectPlate()
        {
            foreach (CMrPart mrPart in mMiddleConnectPlateList)
            {
                Point maxYPoint = mrPart.GetMaxYPoint();
               
                foreach (CMrPart mrLeftPart in mLeftConnectPlateList)
                {
                    Point leftMaxXMaxYPoint = mrLeftPart.GetMaxXMaxYPoint();
                    Point leftMaxYPoint = mrLeftPart.GetMaxYPoint();

                    if (CDimTools.GetInstance().CompareTwoDoubleValue(maxYPoint.Y, leftMaxXMaxYPoint.Y) == 0
                        || CDimTools.GetInstance().CompareTwoDoubleValue(maxYPoint.Y, leftMaxYPoint.Y) == 0)
                    {
                        mrPart.GetCylinderFrontViewInfo().mSymPart = mrLeftPart;
                        continue;
                    }
                }
                foreach (CMrPart mrRightPart in mRightConnectPlateList)
                {
                    Point rightMinXMaxYPoint = mrRightPart.GetMinXMaxYPoint();
                    Point rightMaxYPoint = mrRightPart.GetMaxYPoint();

                    if (CDimTools.GetInstance().CompareTwoDoubleValue(maxYPoint.Y, rightMinXMaxYPoint.Y) == 0
                        || CDimTools.GetInstance().CompareTwoDoubleValue(maxYPoint.Y, rightMaxYPoint.Y) == 0)
                    {
                        mrPart.GetCylinderFrontViewInfo().mSymPart = mrRightPart;
                        continue;
                    }
                }
            }
        }

        /// <summary>
        /// 初始化;
        /// </summary>
        protected void Init()
        {
            if (mViewBase == null)
            {
                return;
            }
            CDimTools dimTools = CDimTools.GetInstance();
            
            dimTools.InitViewBox();

            //获取所有螺钉的数据字典;
            Dictionary<Identifier, TSD.Bolt> dicIdentifierBolt = dimTools.GetAllBoltInDrawing(mViewBase);

            //获取所有Drawing视图中的Part;
            List<TSD.Part> partList = dimTools.GetAllPartInDrawing(mViewBase);

            //先初始化主部件;
            dimTools.InitMrPart(CMrMainBeam.GetInstance().mPartInModel, mViewBase, CMrMainBeam.GetInstance());

            foreach (TSD.Part partInDrawing in partList)
            {
                //1.获取部件的信息;
                TSM.ModelObject modelObjectInModel = dimTools.TransformDrawingToModel(partInDrawing);
                TSM.Part partInModel = modelObjectInModel as TSM.Part;

                CMrPart mrPart = null;
                
                if (CMrMainBeam.GetInstance().mPartInModel.Identifier.GUID == partInModel.Identifier.GUID)
                {
                    mrPart = CMrMainBeam.GetInstance();
                    mMainBeam = CMrMainBeam.GetInstance();
                    mMainBeam.mPartInDrawing = partInDrawing;
                }
                else
                {
                    mrPart = new CMrPart(partInModel, partInDrawing);
                    dimTools.InitMrPart(modelObjectInModel, mViewBase, mrPart);
                }
                AppendMrPart(mrPart);

                mrPart.GetCylinderFrontViewInfo().InitCylinderFrontViewInfo();
                
                InitLeftAndRightPlateMap(mrPart);

                //第一次遍历时就把所有的竖直连接板保存起来,后面需要使用;
                if (CDimTools.GetInstance().IsTwoVectorParallel(mrPart.mNormal, new Vector(1, 0, 0)))
                {
                    if (mrPart.GetCylinderFrontViewInfo().mPostionType == MrPositionType.LEFT || 
                        mrPart.GetCylinderFrontViewInfo().mPostionType == MrPositionType.RIGHT)
                    {
                        mOutsideVerticalConnectPlateList.Add(mrPart);
                    }
                }
                //第一次遍历时把所有的中间连接板保存起来,包括直的和斜的;
                if ((CDimTools.GetInstance().IsTwoVectorParallel(mrPart.mNormal, new Vector(0, 1, 0)) ||
                    CDimTools.GetInstance().IsVectorInXYPlane(mrPart.mNormal)) &&
                    mrPart.GetCylinderFrontViewInfo().mPostionType == MrPositionType.MIDDLE)
                {
                    mMiddleConnectPlateList.Add(mrPart);
                }
                //2.获取部件中的所有螺钉组;
                List<BoltArray> boltArrayList = dimTools.GetAllBoltArray(partInModel);

                foreach (BoltArray boltArray in boltArrayList)
                {
                    TSD.Bolt boltInDrawing = dicIdentifierBolt[boltArray.Identifier];

                    CMrBoltArray mrBoltArray = new CMrBoltArray(boltArray, boltInDrawing);
                    dimTools.InitMrBoltArray(boltArray, mViewBase, mrBoltArray);
                    mrPart.AppendMrBoltArray(mrBoltArray);
                }
                dimTools.UpdateViewBox(mrPart);
            }

            InitMiddleConnectPlate();

            InitCylinderGridLine();

            //构建剪切板的拓扑结构;
            CMrClipPlateManager.GetInstance().BuildMrClipPlate(mMrPartList, MrViewType.CylinderFrontView);

            //清空标记管理器中的数据;
            CMrMarkManager.GetInstance().Clear();
        }

        /// <summary>
        /// 初始化柱子的轴线;
        /// </summary>
        private void InitCylinderGridLine()
        {
            if (null == mViewBase)
            {
                return;
            }
            Point mainPartMaxYMinX = CMrMainBeam.GetInstance().GetMaxYMinXPoint();
            Point mainPartMinYMinX = CMrMainBeam.GetInstance().GetMinYMinXPoint();
            
            Point mainPartMaxYMaxX = CMrMainBeam.GetInstance().GetMaxYMaxXPoint();
            Point mainPartMinYMaxX = CMrMainBeam.GetInstance().GetMinYMaxXPoint();

            DrawingObjectEnumerator gridLines = mViewBase.GetAllObjects(typeof(TSD.GridLine));

            while (gridLines.MoveNext())
            {
                GridLine gridLine = gridLines.Current as GridLine;

                if (gridLine == null)
                {
                    continue;
                }

                Point startPoint = gridLine.StartLabel.CenterPoint;
                Point endPoint = gridLine.EndLabel.CenterPoint;

                Vector gridlineDirectionNormal = new Vector(endPoint.X - startPoint.X, endPoint.Y - startPoint.Y, endPoint.Z - startPoint.Z);

                if (!CDimTools.GetInstance().IsTwoVectorParallel(gridlineDirectionNormal, new Vector(1, 0, 0)))
                {
                    continue;
                }

                Point leftIntersectionPoint = CDimTools.GetInstance().ComputeTwoLineIntersectPoint(startPoint, endPoint, mainPartMaxYMinX, mainPartMinYMinX);
                mLeftIntersectionPointList.Add(leftIntersectionPoint);

                Point rightIntersectionPoint = CDimTools.GetInstance().ComputeTwoLineIntersectPoint(startPoint, endPoint, mainPartMaxYMaxX, mainPartMinYMaxX);
                mRightIntersectionPointList.Add(rightIntersectionPoint);
            }
        }

        /// <summary>
        /// 顶视图初始化以及标注线程函数;
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
        /// 创建标注;
        /// </summary>
        public void CreateDim()
        {
            //启动初始化函数;
            Thread thread = new Thread(new ParameterizedThreadStart(ThreadFunc));
            thread.Start();

            //首先清空标注和标记;
            CDimTools.GetInstance().ClearAllDim(mViewBase);
            CDimTools.GetInstance().ClearAllPartMark(mViewBase);

            lock (mLockString)
            {
#if DEBUG
                DrawSupportPlateBoltDimY();
                DrawAllPartLeftFenSizeDimY();
                DrawMainPartBoltDimY();
                DrawAllPartRightFenSizeDimY();
                DrawMainPartLengthDimY();
                DrawWorkPointToMainPartDimY();
                DrawWorkPointToWorkPointDimY();
                DrawSupportPlateBoltAndConnectPlateDimX();
                DrawAngleSheetBoltDimX();
                DrawMainPartBoltDimX();
                DrawMainPartMiddlePlateDimX();
                DrawMainPartMiddleAngleSheetDimX();
                DrawPartMark();
                DrawMainPartBoltMark();
#else
                try
                {
                    DrawSupportPlateBoltDimY();
                    DrawAllPartLeftFenSizeDimY();
                    DrawMainPartBoltDimY();
                    DrawAllPartRightFenSizeDimY();
                    DrawMainPartLengthDimY();
                    DrawWorkPointToMainPartDimY();
                    DrawWorkPointToWorkPointDimY();
                    DrawSupportPlateBoltAndConnectPlateDimX();
                    DrawAngleSheetBoltDimX();
                    DrawMainPartBoltDimX();
                    DrawMainPartMiddlePlateDimX();
                    DrawMainPartMiddleAngleSheetDimX();
                    DrawPartMark();
                    DrawMainPartBoltMark();
                 }
                 catch (Exception e)
                 {
                     string strText = "提示：程序发生异常\n" + "异常信息：" + e.Message;
                     MessageBox.Show(strText);
                     return;
                 }
#endif
            }
        }

        /// <summary>
        /// 绘制零部件在主梁左侧Y方向上的分尺寸标注;
        /// </summary>
        private void DrawAllPartLeftFenSizeDimY()
        {
            //移除主梁;
            mMrPartList.Remove(mMainBeam);
            CCylinderFrontViewSetting cylinderFrontViewSetting = CCylinderDimSetting.GetInstance().mFrontViewSetting;

            bool bNeedHorizontalConnectPlateDim = false;
            bool bNeedOtherPartDim = false;

            List<Point> horizontalConnectPlateDimPointList = new List<Point>();
            List<Point> otherPartDimPointList = new List<Point>();

            //1.遍历获得所有零件的向上标注的点;
            foreach (CMrPart mrPart in mMrPartList)
            {
                Vector normal = mrPart.mNormal;
                CMrDimSet partDimSet = mrPart.GetCylinderFrontViewInfo().GetPartLeftDimSet();

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
            Point minYPoint = mMainBeam.GetMinYMinXPoint();
            Point maxYPoint = mMainBeam.GetMaxYMinXPoint();

            //1.如果其他剩下的零件需要标注;
            if (bNeedOtherPartDim)
            {
                PointList pointList = new PointList();
                foreach (Point pt in otherPartDimPointList)
                {
                    pointList.Add(pt);
                }
                //如果需要标注轴线;
                if (cylinderFrontViewSetting.FindDimValueByName(CCylinderFrontViewSetting.mstrAixLine))
                {
                    foreach (Point pt in mLeftIntersectionPointList)
                    {
                        pointList.Add(pt);
                    }
                }
                pointList.Add(minYPoint);
                pointList.Add(maxYPoint);

                Vector leftDimVector = new Vector(-1, 0, 0);
                mLeftDimDistance = GetLeftDimDistance(pointList[0]);
                CDimTools.GetInstance().DrawDimensionSet(mViewBase, pointList, leftDimVector, mLeftDimDistance, CCommonPara.mSizeDimPath);
            }
            //2.如果水平连接板及支撑需要标注;
            if (bNeedHorizontalConnectPlateDim)
            {
                PointList pointList = new PointList();

                foreach (Point pt in horizontalConnectPlateDimPointList)
                {
                    pointList.Add(pt);
                }
                //如果需要标注轴线;
                if (cylinderFrontViewSetting.FindDimValueByName(CCylinderFrontViewSetting.mstrAixLine))
                {
                    foreach (Point pt in mLeftIntersectionPointList)
                    {
                        pointList.Add(pt);
                    }
                }
                pointList.Add(minYPoint);
                pointList.Add(maxYPoint);

                Vector leftDimVector = new Vector(-1, 0, 0);
                mLeftDimDistance = GetLeftDimDistance(pointList[0]);
                CDimTools.GetInstance().DrawDimensionSet(mViewBase, pointList, leftDimVector, mLeftDimDistance, CCommonPara.mSizeDimPath);
            }
            mMrPartList.Add(mMainBeam);
        }

        /// <summary>
        /// 绘制零部件在主梁右侧Y方向上的分尺寸标注;
        /// </summary>
        private void DrawAllPartRightFenSizeDimY()
        {
            //移除主梁;
            mMrPartList.Remove(mMainBeam);
            CCylinderFrontViewSetting cylinderFrontViewSetting = CCylinderDimSetting.GetInstance().mFrontViewSetting;

            bool bNeedHorizontalConnectPlateDim = false;
            bool bNeedOtherPartDim = false;

            List<Point> horizontalConnectPlateDimPointList = new List<Point>();
            List<Point> otherPartDimPointList = new List<Point>();

            //1.遍历获得所有零件的向上标注的点;
            foreach (CMrPart mrPart in mMrPartList)
            {
                Vector normal = mrPart.mNormal;
                CMrDimSet partDimSet = mrPart.GetCylinderFrontViewInfo().GetPartRightDimSet();

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
            Point minYPoint = mMainBeam.GetMinYMaxXPoint();
            Point maxYPoint = mMainBeam.GetMaxYMaxXPoint();

            //1.如果其他剩下的零件需要标注;
            if (bNeedOtherPartDim)
            {
                PointList pointList = new PointList();
                foreach (Point pt in otherPartDimPointList)
                {
                    pointList.Add(pt);
                }
                //如果需要标注轴线;
                if (cylinderFrontViewSetting.FindDimValueByName(CCylinderFrontViewSetting.mstrAixLine))
                {
                    foreach (Point pt in mRightIntersectionPointList)
                    {
                        pointList.Add(pt);
                    }
                }
                pointList.Add(minYPoint);
                pointList.Add(maxYPoint);

                Vector rightDimVector = new Vector(1, 0, 0);
                mRightDimDistance = GetRightDimDistance(pointList[0]);
                CDimTools.GetInstance().DrawDimensionSet(mViewBase, pointList, rightDimVector, mRightDimDistance, CCommonPara.mSizeDimPath);
            }
            //2.如果水平连接板及支撑需要标注;
            if (bNeedHorizontalConnectPlateDim)
            {
                PointList pointList = new PointList();

                foreach (Point pt in horizontalConnectPlateDimPointList)
                {
                    pointList.Add(pt);
                }
                //如果需要标注轴线;
                if (cylinderFrontViewSetting.FindDimValueByName(CCylinderFrontViewSetting.mstrAixLine))
                {
                    foreach (Point pt in mRightIntersectionPointList)
                    {
                        pointList.Add(pt);
                    }
                }
                pointList.Add(minYPoint);
                pointList.Add(maxYPoint);

                Vector rightDimVector = new Vector(1, 0, 0);
                mRightDimDistance = GetRightDimDistance(pointList[0]);
                CDimTools.GetInstance().DrawDimensionSet(mViewBase, pointList, rightDimVector, mRightDimDistance, CCommonPara.mSizeDimPath);
            }
            mMrPartList.Add(mMainBeam);
        }

        /// <summary>
        /// 绘制主部件在Y向的螺钉标注;
        /// </summary>
        private void DrawMainPartBoltDimY()
        {
            PointList pointList = new PointList();
            CCylinderFrontViewSetting cylinderFrontViewSetting = CCylinderDimSetting.GetInstance().mFrontViewSetting;

            //1.判断主梁的螺栓是否需要标注;
            bool bNeedMainBeamBoltDim = cylinderFrontViewSetting.FindDimValueByName(CCylinderFrontViewSetting.mstrBolt);

            if (bNeedMainBeamBoltDim)
            {
                CMrDimSet mrDimSet = GetMainPartBoltRightDimY();

                foreach (Point pt in mrDimSet.GetDimPointList())
                {
                    pointList.Add(pt);
                }
                if (pointList.Count == 0)
                {
                    return;
                }
                Point minYPoint = mMainBeam.GetMinYMaxXPoint();
                Point maxYPoint = mMainBeam.GetMaxYMaxXPoint();

                pointList.Add(minYPoint);
                pointList.Add(maxYPoint);

                Vector rightDimVector = new Vector(1, 0, 0);
                mRightDimDistance = GetRightDimDistance(pointList[0]);
                CDimTools.GetInstance().DrawDimensionSet(mViewBase, pointList, rightDimVector, mRightDimDistance, CCommonPara.mSizeDimPath);
            }
        }

        /// <summary>
        /// 获得主部件右侧需要在Y方向上标注的螺钉组;
        /// </summary>
        private CMrDimSet GetMainPartBoltRightDimY()
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
                List<Point> pointList = mrBoltArray.GetMaxXPointList();
                mrDimSet.AddRange(pointList);
            }
            return mrDimSet;
        }

        /// <summary>
        /// 绘制主梁左侧长度的标注;
        /// </summary>
        private void DrawMainPartLengthDimY()
        {
            CCylinderFrontViewSetting cylinderFrontViewSetting = CCylinderDimSetting.GetInstance().mFrontViewSetting;

            //1.判断是否需要标注主梁的长度,如果需要采用单一的一道总尺寸;
            bool bNeedDimMainPartLength = cylinderFrontViewSetting.FindDimValueByName(CCylinderFrontViewSetting.mstrMainPartLength);
            bool bMarkValue = cylinderFrontViewSetting.FindMarkValueByName(CCylinderFrontViewSetting.mstrMainPartLength);
            mMainBeam.SetNeedAddMarkFlag(bMarkValue);

            if (!bNeedDimMainPartLength)
            {
                return;
            }

            Point minYPoint = mMainBeam.GetMinYMinXPoint();
            Point maxYPoint = mMainBeam.GetMaxYMinXPoint();

            bMarkValue = cylinderFrontViewSetting.FindMarkValueByName(CCylinderFrontViewSetting.mstrWorkPointToWorkPoint);
            mMainBeam.SetNeedAddMarkFlag(bMarkValue);

            //2.如果需要标注工作点到工作点,并且主部件与工作点的距离左边或者右边为0，或者都为0时则不标注主部件长度;
            bool bNeedWorkPtToWorkPt = cylinderFrontViewSetting.FindDimValueByName(CCylinderFrontViewSetting.mstrWorkPointToWorkPoint);

            if (bNeedWorkPtToWorkPt)
            {
                double xDistanceLeft = Math.Abs(maxYPoint.Y - CMrMainBeam.GetInstance().mLeftWorkPoint.Y);
                double xDistanceRight = Math.Abs(minYPoint.Y - CMrMainBeam.GetInstance().mRightWorkPoint.Y);

                if (xDistanceLeft < CCommonPara.mDblError || xDistanceRight < CCommonPara.mDblError)
                {
                    return;
                }
            }

            PointList pointList = new PointList();

            pointList.Add(minYPoint);
            pointList.Add(maxYPoint);

            Vector leftDimVector = new Vector(-1, 0, 0);
            mLeftDimDistance = GetLeftDimDistance(minYPoint);
            CDimTools.GetInstance().DrawDimensionSet(mViewBase, pointList, leftDimVector, mLeftDimDistance, CCommonPara.mMainSizeDimPath);
        }

        /// <summary>
        /// 绘制工作点到主梁的距离;
        /// </summary>
        private void DrawWorkPointToMainPartDimY()
        {
            CCylinderFrontViewSetting cylinderFrontViewSetting = CCylinderDimSetting.GetInstance().mFrontViewSetting;

            bool bMarkValue = cylinderFrontViewSetting.FindMarkValueByName(CCylinderFrontViewSetting.mstrWorkPointToMainPart);
            mMainBeam.SetNeedAddMarkFlag(bMarkValue);

            //1.判断是否需要标注主梁与工作点之间的距离,采用一道尺寸来进行标注;
            bool bNeedDimWpToMainPart = cylinderFrontViewSetting.FindDimValueByName(CCylinderFrontViewSetting.mstrWorkPointToMainPart);

            if (!bNeedDimWpToMainPart)
            {
                return;
            }

            Point mainBeamMinY = CMrMainBeam.GetInstance().GetMinYMinXPoint();
            Point mainBeamMaxY = CMrMainBeam.GetInstance().GetMaxYMinXPoint();
            Point leftWorkPoint = CMrMainBeam.GetInstance().mLeftWorkPoint;
            Point rightWorkPoint = CMrMainBeam.GetInstance().mRightWorkPoint;

            bMarkValue = cylinderFrontViewSetting.FindMarkValueByName(CCylinderFrontViewSetting.mstrWorkPointToWorkPoint);
            mMainBeam.SetNeedAddMarkFlag(bMarkValue);

            //2.如果工作点到工作点需要进行标注，而主部件左右侧与工作点的左右侧的距离都为0则不需要标注;
            bool bNeedWorkPtToWorkPt = cylinderFrontViewSetting.FindDimValueByName(CCylinderFrontViewSetting.mstrWorkPointToWorkPoint);

            if (bNeedWorkPtToWorkPt)
            {
                double xDistanceLeft = Math.Abs(mainBeamMaxY.Y - leftWorkPoint.Y);
                double xDistanceRight = Math.Abs(mainBeamMaxY.Y - rightWorkPoint.Y);

                if (xDistanceLeft < CCommonPara.mDblError && xDistanceRight < CCommonPara.mDblError)
                {
                    return;
                }
            }

            PointList pointList = new PointList();

            pointList.Add(leftWorkPoint);
            pointList.Add(rightWorkPoint);
            pointList.Add(mainBeamMinY);
            pointList.Add(mainBeamMaxY);

            Vector leftDimVector = new Vector(-1, 0, 0);
            mLeftDimDistance = GetLeftDimDistance(leftWorkPoint);
            CDimTools.GetInstance().DrawDimensionSet(mViewBase, pointList, leftDimVector, mLeftDimDistance, CCommonPara.mMainSizeDimPath);
        }

        /// <summary>
        /// 绘制主梁左侧工作点与工作点之间的标注;
        /// </summary>
        private void DrawWorkPointToWorkPointDimY()
        {
            CCylinderFrontViewSetting cylinderFrontViewSetting = CCylinderDimSetting.GetInstance().mFrontViewSetting;
            PointList pointList = new PointList();

            bool bMarkValue = cylinderFrontViewSetting.FindMarkValueByName(CCylinderFrontViewSetting.mstrWorkPointToWorkPoint);
            mMainBeam.SetNeedAddMarkFlag(bMarkValue);

            //1.判断工作点与工作点之间是否需要标注，需要标注的话是一道单尺寸;
            bool bNeedDimWpToWp = cylinderFrontViewSetting.FindDimValueByName(CCylinderFrontViewSetting.mstrWorkPointToWorkPoint);

            if (bNeedDimWpToWp)
            {
                Point leftWorkPoint = CMrMainBeam.GetInstance().mLeftWorkPoint;
                Point rightWorkPoint = CMrMainBeam.GetInstance().mRightWorkPoint;

                pointList.Add(leftWorkPoint);
                pointList.Add(rightWorkPoint);

                Vector leftDimVector = new Vector(-1, 0, 0);
                mLeftDimDistance = GetLeftDimDistance(leftWorkPoint);
                CDimTools.GetInstance().DrawDimensionSet(mViewBase, pointList, leftDimVector, mLeftDimDistance, CCommonPara.mMainSizeDimPath);
            }
        }

        /// <summary>
        /// 绘制支撑板及牛腿上的螺钉在Y方向的标注;
        /// </summary>
        private void DrawSupportPlateBoltDimY()
        {
            CCylinderFrontViewSetting cylinderFrontViewSetting = CCylinderDimSetting.GetInstance().mFrontViewSetting;

            if (!cylinderFrontViewSetting.FindDimValueByName(CCylinderFrontViewSetting.mstrSupportPlate))
            {
                return;
            }
            //移除主梁;
            mMrPartList.Remove(mMainBeam);

            foreach (CMrPart mrPart in mMrPartList)
            {
                CMrDimSet mrDimSet = mrPart.GetCylinderFrontViewInfo().GetSupportPlateBoltYDimSet();

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
            mMrPartList.Add(mMainBeam);
        }

        /// <summary>
        /// 绘制支撑板上及牛腿上的螺钉以及上面的连接板在X方向的标注;
        /// </summary>
        private void DrawSupportPlateBoltAndConnectPlateDimX()
        {
            CCylinderFrontViewSetting cylinderFrontViewSetting = CCylinderDimSetting.GetInstance().mFrontViewSetting;

            mMrPartList.Remove(mMainBeam);

            foreach (CMrPart mrPart in mMrPartList)
            {
                CMrDimSet mrBoltDimSet = mrPart.GetCylinderFrontViewInfo().GetSupportPlateBoltXDimSet();

                CMrDimSet mrConnectPlateDimSet = GetSupportPlateMiddleConnectPlateDimSet(mrPart);

                PointList pointList = new PointList();

                //1.支撑板或连接板是否需要标注;
                if (cylinderFrontViewSetting.FindDimValueByName(CCylinderFrontViewSetting.mstrSupportPlate))
                {
                    if (mrBoltDimSet != null)
                    {
                        foreach (Point point in mrBoltDimSet.GetDimPointList())
                        {
                            pointList.Add(point);
                        }
                    }
                }
                //2.支撑板与连接板中间的加筋板是否需要标注;
                if (cylinderFrontViewSetting.FindDimValueByName(CCylinderFrontViewSetting.mstrConnentPlateOnSupport))
                {
                    if (mrConnectPlateDimSet != null)
                    {
                        foreach (Point point in mrConnectPlateDimSet.GetDimPointList())
                        {
                            pointList.Add(point);
                        }
                    }
                }
                if (pointList.Count > 0)
                {
                    if (mrBoltDimSet != null && mrBoltDimSet.Count > 0)
                    {
                        double dimDistance = mrBoltDimSet.mDimDistance;
                        Vector dimVector = mrBoltDimSet.mDimVector;
                        CDimTools.GetInstance().DrawDimensionSet(mViewBase, pointList, dimVector, dimDistance, CCommonPara.mSizeDimPath);
                    }
                    else
                    {
                        double dimDistance = Math.Abs(mrPart.GetMinYPoint().Y - pointList[0].Y) + CCommonPara.mDefaultDimDistance;
                        Vector dimVector = new Vector(0, -1, 0);
                        CDimTools.GetInstance().DrawDimensionSet(mViewBase, pointList, dimVector, dimDistance, CCommonPara.mSizeDimPath);
                    }
                }
            }
            mMrPartList.Add(mMainBeam);
        }

        /// <summary>
        /// 绘制主部件外侧角钢的螺钉标注;
        /// </summary>
        private void DrawAngleSheetBoltDimX()
        {
            CCylinderFrontViewSetting cylinderFrontViewSetting = CCylinderDimSetting.GetInstance().mFrontViewSetting;

            mMrPartList.Remove(mMainBeam);

            foreach (CMrPart mrPart in mMrPartList)
            {
                CMrDimSet mrBoltDimSet = mrPart.GetCylinderFrontViewInfo().GetAngleSheetBoltXDimSet();

                PointList pointList = new PointList();

                //1.支撑板或连接板是否需要标注;
                if (cylinderFrontViewSetting.FindDimValueByName(CCylinderFrontViewSetting.mstrAngleSheet))
                {
                    if (mrBoltDimSet != null)
                    {
                        foreach (Point point in mrBoltDimSet.GetDimPointList())
                        {
                            pointList.Add(point);
                        }
                    }
                }
                if (pointList.Count > 0)
                {
                    if (mrBoltDimSet != null && mrBoltDimSet.Count > 0)
                    {
                        double dimDistance = mrBoltDimSet.mDimDistance;
                        Vector dimVector = mrBoltDimSet.mDimVector;
                        CDimTools.GetInstance().DrawDimensionSet(mViewBase, pointList, dimVector, dimDistance, CCommonPara.mSizeDimPath);
                    }
                    else
                    {
                        double dimDistance = Math.Abs(mrPart.GetMinYPoint().Y - pointList[0].Y) + CCommonPara.mDefaultDimDistance;
                        Vector dimVector = new Vector(0, -1, 0);
                        CDimTools.GetInstance().DrawDimensionSet(mViewBase, pointList, dimVector, dimDistance, CCommonPara.mSizeDimPath);
                    }
                }
            }

            mMrPartList.Add(mMainBeam);
        }

        /// <summary>
        /// 获得支撑板中间的连接板的标注;
        /// </summary>
        /// <returns></returns>
        public CMrDimSet GetSupportPlateMiddleConnectPlateDimSet(CMrPart mrSupportPlate)
        {
            if (!CDimTools.GetInstance().IsTwoVectorParallel(mrSupportPlate.mNormal, new Vector(0, 0, 1))
                || CDimTools.GetInstance().IsPartTheAngleSteel(mrSupportPlate) || IsOutsidePlate(mrSupportPlate))
            {
                return null;
            }

            CCylinderFrontViewSetting cylinderFrontViewSetting = CCylinderDimSetting.GetInstance().mFrontViewSetting;

            List<CMrPart> mrConnectPlates = GetSupportPlateMiddleConnectPlates(mrSupportPlate);

            if (mrConnectPlates == null || mrConnectPlates.Count == 0)
            {
                return null;
            }

            CMrDimSet mrDimSet = new CMrDimSet();

            foreach (CMrPart mrPart in mrConnectPlates)
            {
                mrDimSet.AddPoint(mrPart.GetMinYMinXPoint());
                bool bValue = cylinderFrontViewSetting.FindMarkValueByName(CCylinderFrontViewSetting.mstrConnentPlateOnSupport);
                mrPart.SetNeedAddMarkFlag(bValue);
            }
            if (mrDimSet.Count > 0)
            {
                if(mrSupportPlate.GetCylinderFrontViewInfo().mPostionType == MrPositionType.RIGHT)
                {
                    mrDimSet.AddPoint(new Point(mMainBeam.GetMaxXPoint().X, mrDimSet.GetDimPointList()[0].Y, 0));
                }
                if (mrSupportPlate.GetCylinderFrontViewInfo().mPostionType == MrPositionType.LEFT)
                {
                    mrDimSet.AddPoint(new Point(mMainBeam.GetMinXPoint().X, mrDimSet.GetDimPointList()[0].Y, 0));
                }
            }
            return mrDimSet;
        }

        /// <summary>
        /// 获得支撑及牛腿中间的垂直加筋板;
        /// </summary>
        /// <param name="mrSupportPlate"></param>
        /// <returns></returns>
        private List<CMrPart> GetSupportPlateMiddleConnectPlates(CMrPart mrSupportPlate)
        {
            Point maxXPoint = mrSupportPlate.GetMaxXPoint();
            Point minXPoint = mrSupportPlate.GetMinXPoint();

            List<CMrPart> mrConnectPlates = new List<CMrPart>();

            if (mrSupportPlate.GetCylinderFrontViewInfo().mPostionType == MrPositionType.LEFT)
            {
                foreach (CMrPart mrPart in mOutsideVerticalConnectPlateList)
                {
                    if (IsOutsidePlate(mrPart) || mrPart.GetCylinderFrontViewInfo().mPostionType == MrPositionType.RIGHT)
                    {
                        continue;
                    }
                    if (CDimTools.GetInstance().IsPartInOtherPartBox(mrPart, mrSupportPlate))
                    {
                        mrConnectPlates.Add(mrPart);
                    }
                }
                return mrConnectPlates;
            }
            else if (mrSupportPlate.GetCylinderFrontViewInfo().mPostionType == MrPositionType.RIGHT)
            {
                foreach (CMrPart mrPart in mOutsideVerticalConnectPlateList)
                {
                    if (IsOutsidePlate(mrPart) || mrPart.GetCylinderFrontViewInfo().mPostionType == MrPositionType.LEFT)
                    {
                        continue;
                    }
                    if (CDimTools.GetInstance().IsPartInOtherPartBox(mrPart, mrSupportPlate))
                    {
                        mrConnectPlates.Add(mrPart);
                    }
                }
                return mrConnectPlates;
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
        /// 绘制主部件在X向的螺钉组;
        /// </summary>
        private void DrawMainPartBoltDimX()
        {
            CCylinderFrontViewSetting cylinderFrontViewSetting = CCylinderDimSetting.GetInstance().mFrontViewSetting;

            bool bNeedMainBeamBoltDim = cylinderFrontViewSetting.FindDimValueByName(CCylinderFrontViewSetting.mstrBolt);

            if (!bNeedMainBeamBoltDim)
            {
                return;
            }
            List<CMrDimSet> boltDimSetList = GetMainPartBoltDimSetX();

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
        /// 获得主梁螺钉在X向的标注集;
        /// </summary>
        /// <returns></returns>
        private List<CMrDimSet> GetMainPartBoltDimSetX()
        {
            List<CMrDimSet> mrDimSetList = new List<CMrDimSet>();

            //主梁上未标注的螺钉组;
            List<CMrBoltArray> lastMrBoltArrayList = new List<CMrBoltArray>();

            //构建具有相同属性螺钉组的集合;
            List<CMrBoltArrayGroup> mrBoltArrayGroupList = new List<CMrBoltArrayGroup>();

            //构建主梁中间剩下的螺钉组的链表;
            foreach (CMrBoltArray mrBoltArray in mMainBeam.GetBoltArrayList())
            {
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
            CCylinderFrontViewSetting cylinderFrontViewSetting = CCylinderDimSetting.GetInstance().mFrontViewSetting;

            bool bNeedBoltClosed = cylinderFrontViewSetting.FindDimValueByName(CBeamFrontViewSetting.mstrBoltClosed);

            //根据组合好的螺钉组集合来进行标注;
            foreach (CMrBoltArrayGroup mrBoltArrayGroup in mrBoltArrayGroupList)
            {
                CMrDimSet yMrDimSet = new CMrDimSet();

                foreach (CMrBoltArray mrBoltArray in mrBoltArrayGroup.mrBoltArrayList)
                {
                    List<Point> minYPointList = mrBoltArray.GetMinYPointList();
                   
                    yMrDimSet.AddRange(minYPointList);
                }

                if (yMrDimSet.GetDimPointList().Count == 0)
                {
                    continue;
                }

                yMrDimSet.AddPoint(new Point(mMainBeam.GetMinXPoint().X, yMrDimSet.GetDimPointList()[0].Y, 0));

                if (bNeedBoltClosed)
                {
                    yMrDimSet.AddPoint(new Point(mMainBeam.GetMaxXPoint().X, yMrDimSet.GetDimPointList()[0].Y, 0));
                }

                //需要判断螺钉是否在柱子的底端或上端;
                double maxY = mrBoltArrayGroup.GetMaxYPoint().Y;
                double minY =mrBoltArrayGroup.GetMinYPoint().Y;
                double mainBeamMaxY = mMainBeam.GetMaxYPoint().Y;
                double mainBeamMinY = mMainBeam.GetMinYPoint().Y;

                if (Math.Abs(maxY - mainBeamMaxY) < CCommonPara.mDefaultDimDistanceThreshold)
                {
                    yMrDimSet.mDimDistance = Math.Abs(yMrDimSet.GetDimPointList()[0].Y - CCommonPara.mViewMaxY) + CCommonPara.mDefaultDimDistance;
                    yMrDimSet.mDimVector = new Vector(0, 1, 0);
                    mrDimSetList.Add(yMrDimSet);
                }
                else if (Math.Abs(minY - mainBeamMinY) < CCommonPara.mDefaultDimDistanceThreshold)
                {
                    yMrDimSet.mDimDistance = Math.Abs(yMrDimSet.GetDimPointList()[0].Y - CCommonPara.mViewMinY) + CCommonPara.mDefaultDimDistance;
                    yMrDimSet.mDimVector = new Vector(0, -1, 0);
                    mrDimSetList.Add(yMrDimSet);
                }
                else
                {
                    yMrDimSet.mDimDistance = CCommonPara.mDefaultDimDistance;
                    yMrDimSet.mDimVector = new Vector(0, -1, 0);
                    mrDimSetList.Add(yMrDimSet);
                }
            }
            return mrDimSetList;
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

                if (Math.Abs(boltPoint.Y - groupBoltPoint.Y) < CCommonPara.mDefaultTwoBoltArrayGap)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 绘制主部件中间腹板在X向标注;
        /// </summary>
        private void DrawMainPartMiddlePlateDimX()
        {
            CCylinderFrontViewSetting cylinderFrontViewSetting = CCylinderDimSetting.GetInstance().mFrontViewSetting;
            bool bNeedMainBeamMiddlePlate = cylinderFrontViewSetting.FindDimValueByName(CCylinderFrontViewSetting.mstrMainBeamMiddlePart);
           
            if (!bNeedMainBeamMiddlePlate)
            {
                return;
            }

            List<CMrDimSet> middlePartDimSetList = GetMainPartMiddlePartDimX();

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
        /// 绘制主梁中间角钢的标注;
        /// </summary>
        private void DrawMainPartMiddleAngleSheetDimX()
        {
            CCylinderFrontViewSetting beamFrontViewSetting = CCylinderDimSetting.GetInstance().mFrontViewSetting;

            bool bNeedMainBeamMiddleAngleSheet = beamFrontViewSetting.FindDimValueByName(CCylinderFrontViewSetting.mstrAngleSheet);

            if (!bNeedMainBeamMiddleAngleSheet)
            {
                return;
            }
            List<CMrPart> mrAngleSheetList = new List<CMrPart>();

            foreach (CMrPart mrPart in mMrPartList)
            {
                if (!CDimTools.GetInstance().IsPartTheAngleSteel(mrPart)||
                    mrPart.GetCylinderFrontViewInfo().mPostionType != MrPositionType.MIDDLE)
                {
                    continue;
                }
                mrAngleSheetList.Add(mrPart);
            }

            //构建中间腹板上角钢组的组合;
            int nCount = mrAngleSheetList.Count;

            //零件的唯一标识符与零件组集合的映射表;
            Dictionary<Identifier, CMrPartGroup> mapIdentifierToPartGroup = new Dictionary<Identifier, CMrPartGroup>();

            //构建具有相同属性零件组的集合;
            List<CMrPartGroup> mrPartGroupList = new List<CMrPartGroup>();

            //把中间腹板上距离很近的角钢组合起来;
            for (int i = 0; i < nCount; i++)
            {
                CMrPart firstPart = mrAngleSheetList[i];

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
                    CMrPart secondPart = mrAngleSheetList[j];

                    if (IsMrPartCanAddToMrPartGroup(secondPart, mrPartGroup))
                    {
                        mrPartGroup.AppendMrPart(secondPart);
                        mapIdentifierToPartGroup.Add(secondPart.mPartInModel.Identifier, mrPartGroup);
                    }
                }
            }
            //对中间腹板上的角钢进行标注;
            foreach (CMrPartGroup mrPartGroup in mrPartGroupList)
            {
                CMrDimSet XMrDimSet = new CMrDimSet();

                List<CMrPart> mrPartList = mrPartGroup.mrPartList;
                
                foreach (CMrPart mrPart in mrPartList)
                {
                    bool bNeedDim = true;

                    foreach (CMrBoltArray mrBoltArray in mrPart.GetBoltArrayList())
                    {
                        if (CDimTools.GetInstance().IsTwoVectorParallel(mrBoltArray.mNormal, new Vector(0, 0, 1)))
                        {
                            bNeedDim = false;
                        }
                    }
                    if (!bNeedDim)
                    {
                        continue;
                    }

                    //判断出角钢的厚度方向;
                    Point maxZPoint = mrPart.GetMaxZPoint();
                    Point minZPoint = mrPart.GetMinZPoint();
                    Point maxXmaxZPoint = mrPart.GetMaxXMaxZPoint();
                    Point maxXminZPoint = mrPart.GetMaxXMinZPoint();
                    Point minXmaxZPoint = mrPart.GetMinXMaxZPoint();
                    Point minXminZPoint = mrPart.GetMinXMinZPoint();

                    if (maxZPoint.Z > 0)
                    {
                        if (CDimTools.GetInstance().CompareTwoDoubleValue(maxZPoint.Z, minXmaxZPoint.Z) == 0)
                        {
                            XMrDimSet.AddPoint(mrPart.GetMaxYMinXPoint());
                        }
                        else if (CDimTools.GetInstance().CompareTwoDoubleValue(maxZPoint.Z, maxXmaxZPoint.Z) == 0)
                        {
                            XMrDimSet.AddPoint(mrPart.GetMaxYMaxXPoint());
                        }
                        else
                        {
                            XMrDimSet.AddPoint(mrPart.GetMaxYMinXPoint());
                        }
                    }
                    else if (minZPoint.Z < 0)
                    {
                        if (CDimTools.GetInstance().CompareTwoDoubleValue(minZPoint.Z, minXminZPoint.Z) == 0)
                        {
                            XMrDimSet.AddPoint(mrPart.GetMaxYMinXPoint());
                        }
                        else if (CDimTools.GetInstance().CompareTwoDoubleValue(minZPoint.Z, maxXminZPoint.Z) == 0)
                        {
                            XMrDimSet.AddPoint(mrPart.GetMaxYMaxXPoint());
                        }
                        else
                        {
                            XMrDimSet.AddPoint(mrPart.GetMaxYMinXPoint());
                        }
                    }
                }
                if (XMrDimSet.Count > 0)
                {
                    XMrDimSet.AddPoint(new Point(CMrMainBeam.GetInstance().GetMinXPoint().X, XMrDimSet.GetDimPointList()[0].Y, 0));
                    XMrDimSet.mDimDistance = CCommonPara.mDefaultDimDistance;
                    XMrDimSet.mDimVector = new Vector(0, 1, 0);

                    PointList pointList = new PointList();

                    foreach (Point pt in XMrDimSet.GetDimPointList())
                    {
                        pointList.Add(pt);
                    }
                    CDimTools.GetInstance().DrawDimensionSet(mViewBase, pointList, XMrDimSet.mDimVector, XMrDimSet.mDimDistance, CCommonPara.mSizeDimPath);
                }
            }
        }

        /// <summary>
        /// 获得主部件中间零部件在X向上的标注,即腹板上的竖直部件;
        /// </summary>
        /// <returns></returns>
        private List<CMrDimSet> GetMainPartMiddlePartDimX()
        {
            List<CMrDimSet> mrDimSetList = new List<CMrDimSet>();
            List<CMrPart> middlePlateList = new List<CMrPart>();
            double flageThickness = CMrMainBeam.GetInstance().mFlangeThickness;

            foreach (CMrPart mrPart in mMrPartList)
            {
                if (!CDimTools.GetInstance().IsTwoVectorParallel(mrPart.mNormal, new Vector(1, 0, 0)) ||
                    CDimTools.GetInstance().IsPartTheAngleSteel(mrPart) ||
                    mrPart.GetCylinderFrontViewInfo().mPostionType != MrPositionType.MIDDLE || IsOutsidePlate(mrPart)||
                    CDimTools.GetInstance().IsPartTheHProfileSheet(mrPart))
                {
                    continue;
                }
                if (Math.Abs(mMainBeam.GetMinXPoint().X - mrPart.GetMinXPoint().X) <= flageThickness||
                    Math.Abs(mMainBeam.GetMaxXPoint().X - mrPart.GetMaxXPoint().X) <= flageThickness)
                {
                    continue;
                }

                middlePlateList.Add(mrPart);

                CCylinderFrontViewSetting cylinderFrontViewSetting = CCylinderDimSetting.GetInstance().mFrontViewSetting;
                bool bMarkFlag = cylinderFrontViewSetting.FindMarkValueByName(CCylinderFrontViewSetting.mstrMainBeamMiddlePart);
                mrPart.SetNeedAddMarkFlag(bMarkFlag);
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
                    yMrDimSet.AddPoint(mrPart.GetMinXMaxYPoint());
                }
                yMrDimSet.AddPoint(new Point(mMainBeam.GetMinXPoint().X, yMrDimSet.GetDimPointList()[0].Y, 0));
                yMrDimSet.mDimVector = new Vector(0, 1, 0);
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
                if (Math.Abs(mrPart.mMidPoint.Y - groupPart.mMidPoint.Y) < CCommonPara.mDefalutTwoPartGap)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 获得向左标注的距离;
        /// </summary>
        /// <returns></returns>
        private double GetLeftDimDistance(Point firstDimPoint)
        {
            if (mLeftDimDistance == 0)
            {
                mLeftDimDistance = Math.Abs(CCommonPara.mViewMinX) - Math.Abs(firstDimPoint.X) + CCommonPara.mDefaultDimDistance + CCommonPara.mDefaultTwoDimLineGap;
            }
            else
            {
                mLeftDimDistance = mLeftDimDistance + CCommonPara.mDefaultTwoDimLineGap + Math.Abs(mLeftDimFirstPoint.X) - Math.Abs(firstDimPoint.X);
            }

            mLeftDimFirstPoint = firstDimPoint;

            return mLeftDimDistance;
        }

        /// <summary>
        /// 获得向下的标注;
        /// </summary>
        /// <param name="firstDimPoint"></param>
        /// <returns></returns>
        private double GetRightDimDistance(Point firstDimPoint)
        {
            if (mRightDimDistance == 0)
            {
                mRightDimDistance = CCommonPara.mViewMaxX - firstDimPoint.X + CCommonPara.mDefaultDimDistance + CCommonPara.mDefaultTwoDimLineGap;
            }
            else
            {
                mRightDimDistance = mRightDimDistance + CCommonPara.mDefaultTwoDimLineGap + mRightDimFirstPoint.X - firstDimPoint.X;
            }

            mRightDimFirstPoint = firstDimPoint;

            return mRightDimDistance;
        }

        /// <summary>
        /// 绘制零件标记;
        /// </summary>
        public void DrawPartMark()
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
    }
}
