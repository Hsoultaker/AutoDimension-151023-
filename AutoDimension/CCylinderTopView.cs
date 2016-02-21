using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using AutoDimension.Entity;

using Tekla.Structures.Drawing;
using Tekla.Structures.Model;
using Tekla.Structures.Geometry3d;
using Tekla.Structures;

using TSD = Tekla.Structures.Drawing;
using TSM = Tekla.Structures.Model;

using System.Windows.Forms;
using System.Collections;
using System.Threading;

namespace AutoDimension
{
    public class CCylinderTopView:CView
    {
        /// <summary>
        /// 柱的左上角最大点,不一定是主梁;
        /// </summary>
        public Point mLeftTopPoint;

        /// <summary>
        /// 柱的右上角最大点，不一定是主梁;
        /// </summary>
        public Point mRightTopPoint;

        /// <summary>
        /// 零件标记时需要去掉重复标注的点;
        /// </summary>
        private Dictionary<String, bool> mDicMarkDimPoints = new Dictionary<String, bool>();

        /// <summary>
        /// 部件的属性与部件自身的数据字典,主要是来判断上下的对称性;
        /// </summary>
        private Dictionary<String, CMrPart> mDicAttributePart = new Dictionary<String, CMrPart>();

        /// <summary>
        /// 柱子的轴线与中心线的交点;
        /// </summary>
        private List<Point> mIntersectionPointList = new List<Point>();

        /// <summary>
        /// 所有垂直连接板的链表;
        /// </summary>
        private List<CMrPart> mVerticalConnectPlateList = new List<CMrPart>();

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
        public CCylinderTopView(TSD.View viewBase, Model model):base(viewBase)
        {
            mViewBase = viewBase;
            mModel = model;
            mLeftTopPoint = new Point();
            mRightTopPoint = new Point();
        }

        /// <summary>
        /// 构建垂直连接板左右侧的对称性;
        /// </summary>
        /// <param name="mrPart"></param>
        private void InitMrPart(CMrPart mrPart)
        {
            Vector normal = mrPart.mNormal;

            //1.板的法向与Y轴平行或者法向在XY平面内才可以进行对称性判断;
            if (!(CDimTools.GetInstance().IsTwoVectorParallel(normal, new Vector(0, 1, 0)) ||CDimTools.GetInstance().IsVectorInXYPlane(normal)))
            {
                return;
            }
            //2.需要判断板的位置;
            if(mrPart.GetCylinderTopViewInfo().mPostionType==MrPositionType.LEFT)
            {
                Point maxXMaxY = mrPart.GetMaxXMaxYPoint();
                Point maxXMinY = mrPart.GetMaxXMinYPoint();

                String strAttribute1 = ((int)maxXMaxY.Y).ToString();

                if (mDicAttributePart.ContainsKey(strAttribute1))
                {
                    CMrPart symPart = mDicAttributePart[strAttribute1];
                    mrPart.GetCylinderTopViewInfo().mSymPart = symPart;
                    symPart.GetCylinderTopViewInfo().mSymPart = mrPart;
                }
                else
                {
                    mDicAttributePart.Add(strAttribute1, mrPart);
                }
            }
            if(mrPart.GetCylinderTopViewInfo().mPostionType==MrPositionType.RIGHT)
            {
                Point minXMaxY = mrPart.GetMinXMaxYPoint();
                Point minXMinY = mrPart.GetMinXMinYPoint();

                String strAttribute1 = ((int)minXMaxY.Y).ToString();

                if (mDicAttributePart.ContainsKey(strAttribute1))
                {
                    CMrPart symPart = mDicAttributePart[strAttribute1];
                    mrPart.GetCylinderTopViewInfo().mSymPart = symPart;
                    symPart.GetCylinderTopViewInfo().mSymPart = mrPart;
                }
                else
                {
                    mDicAttributePart.Add(strAttribute1, mrPart);
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

            //初始化视图包围盒;
            dimTools.InitViewBox();

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

                mrPart.GetCylinderTopViewInfo().InitCylinderTopViewInfo();
                InitMrPart(mrPart);

                //第一次遍历时就把所有的垂直加筋板保存起来,后面需要使用;
                if (CDimTools.GetInstance().IsTwoVectorParallel(mrPart.mNormal, new Vector(1, 0, 0)))
                {
                    mVerticalConnectPlateList.Add(mrPart);
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

            InitCylinderGridLine();

            //构建剪切板的拓扑结构;
            CMrClipPlateManager.GetInstance().BuildMrClipPlate(mMrPartList,MrViewType.CylinderTopView);

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

            DrawingObjectEnumerator gridLines = mViewBase.GetAllObjects(typeof(TSD.GridLine));

            DrawingHandler drawingHandler = new DrawingHandler();
            TSD.UI.DrawingObjectSelector DS = drawingHandler.GetDrawingObjectSelector();

            while (gridLines.MoveNext())
            {
                GridLine gridLine = gridLines.Current as GridLine;

                if (gridLine == null)
                {
                    continue;
                }

                Point startPoint=gridLine.StartLabel.CenterPoint;
                Point endPoint = gridLine.EndLabel.CenterPoint;

                Vector gridlineDirectionNormal = new Vector(endPoint.X - startPoint.X, endPoint.Y - startPoint.Y, endPoint.Z - startPoint.Z);

                if (!CDimTools.GetInstance().IsTwoVectorParallel(gridlineDirectionNormal, new Vector(1, 0, 0)))
                {
                    continue;
                }

                Point intersectionPoint=CDimTools.GetInstance().ComputeTwoLineIntersectPoint(startPoint, endPoint, new Point(0, 0, 0), new Point(0, 1000, 0));
                mIntersectionPointList.Add(intersectionPoint);
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
        /// 开始进行标注;
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
                    DrawPartMark();
                    DrawMainPartBoltMark();
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
        /// 绘制左侧所有零部件在Y向的标注，主要是区分支撑板和剩下的零部件标注;
        /// </summary>
        private void DrawAllPartLeftFenSizeDimY()
        {
            //移除主梁;
            mMrPartList.Remove(mMainBeam);

            CCylinderTopViewSetting cylinderTopViewSetting=CCylinderDimSetting.GetInstance().mTopViewSetting;

            bool bNeedHorizontalConnectPlateDim = false;
            bool bNeedOtherPartDim = false;

            List<Point> horizontalConnectPlateDimPointList = new List<Point>();
            List<Point> otherPartDimPointList = new List<Point>();

            //1.遍历获得所有零件的向上标注的点;
            foreach (CMrPart mrPart in mMrPartList)
            {
                Vector normal = mrPart.mNormal;
                
                CMrDimSet partDimSet = mrPart.GetCylinderTopViewInfo().GetPartLeftDimSet();

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
                if (cylinderTopViewSetting.FindDimValueByName(CCylinderTopViewSetting.mstrAixLine))
                {
                    foreach (Point pt in mIntersectionPointList)
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
                if (cylinderTopViewSetting.FindDimValueByName(CCylinderTopViewSetting.mstrAixLine))
                {
                    foreach (Point pt in mIntersectionPointList)
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
        /// 绘制右侧所有零部件在Y向的标注，主要是区分支撑板和剩下的零部件标注;
        /// </summary>
        private void DrawAllPartRightFenSizeDimY()
        {
            //移除主梁;
            mMrPartList.Remove(mMainBeam);

            CCylinderTopViewSetting cylinderTopViewSetting = CCylinderDimSetting.GetInstance().mTopViewSetting;

            bool bNeedHorizontalConnectPlateDim = false;
            bool bNeedOtherPartDim = false;

            List<Point> horizontalConnectPlateDimPointList = new List<Point>();
            List<Point> otherPartDimPointList = new List<Point>();

            //1.遍历获得所有零件的向上标注的点;
            foreach (CMrPart mrPart in mMrPartList)
            {
                Vector normal = mrPart.mNormal;

                CMrDimSet partDimSet = mrPart.GetCylinderTopViewInfo().GetPartRightDimSet();

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
                if (cylinderTopViewSetting.FindDimValueByName(CCylinderTopViewSetting.mstrAixLine))
                {
                    foreach (Point pt in mIntersectionPointList)
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
                if (cylinderTopViewSetting.FindDimValueByName(CCylinderTopViewSetting.mstrAixLine))
                {
                    foreach (Point pt in mIntersectionPointList)
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
        /// 绘制主梁左侧长度的标注;
        /// </summary>
        private void DrawMainPartLengthDimY()
        {
            CCylinderTopViewSetting cylinderTopViewSetting = CCylinderDimSetting.GetInstance().mTopViewSetting;

            //1.判断是否需要标注主梁的长度,如果需要采用单一的一道总尺寸;
            bool bNeedDimMainPartLength = cylinderTopViewSetting.FindDimValueByName(CCylinderTopViewSetting.mstrMainPartLength);
            bool bMarkValue = cylinderTopViewSetting.FindMarkValueByName(CCylinderTopViewSetting.mstrMainPartLength);
            mMainBeam.SetNeedAddMarkFlag(bMarkValue);

            if (!bNeedDimMainPartLength)
            {
                return;
            }

            Point minYPoint = mMainBeam.GetMinYMinXPoint();
            Point maxYPoint = mMainBeam.GetMaxYMinXPoint();

            bMarkValue = cylinderTopViewSetting.FindMarkValueByName(CCylinderTopViewSetting.mstrWorkPointToWorkPoint);
            mMainBeam.SetNeedAddMarkFlag(bMarkValue);

            //2.如果需要标注工作点到工作点,并且主部件与工作点的距离左边或者右边为0，或者都为0时则不标注主部件长度;
            bool bNeedWorkPtToWorkPt = cylinderTopViewSetting.FindDimValueByName(CCylinderTopViewSetting.mstrWorkPointToWorkPoint);

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
            CCylinderTopViewSetting cylinderTopViewSetting = CCylinderDimSetting.GetInstance().mTopViewSetting;

            bool bMarkValue = cylinderTopViewSetting.FindMarkValueByName(CCylinderTopViewSetting.mstrWorkPointToMainPart);
            mMainBeam.SetNeedAddMarkFlag(bMarkValue);

            //1.判断是否需要标注主梁与工作点之间的距离,采用一道尺寸来进行标注;
            bool bNeedDimWpToMainPart = cylinderTopViewSetting.FindDimValueByName(CCylinderTopViewSetting.mstrWorkPointToMainPart);

            if (!bNeedDimWpToMainPart)
            {
                return;
            }

            Point mainBeamMinY = CMrMainBeam.GetInstance().GetMinYMinXPoint();
            Point mainBeamMaxY = CMrMainBeam.GetInstance().GetMaxYMinXPoint();
            Point leftWorkPoint = CMrMainBeam.GetInstance().mLeftWorkPoint;
            Point rightWorkPoint = CMrMainBeam.GetInstance().mRightWorkPoint;

            bMarkValue = cylinderTopViewSetting.FindMarkValueByName(CCylinderTopViewSetting.mstrWorkPointToWorkPoint);
            mMainBeam.SetNeedAddMarkFlag(bMarkValue);

            //2.如果工作点到工作点需要进行标注，而主部件左右侧与工作点的左右侧的距离都为0则不需要标注;
            bool bNeedWorkPtToWorkPt = cylinderTopViewSetting.FindDimValueByName(CCylinderTopViewSetting.mstrWorkPointToWorkPoint);

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
            CCylinderTopViewSetting cylinderTopViewSetting = CCylinderDimSetting.GetInstance().mTopViewSetting;

            PointList pointList = new PointList();

            bool bMarkValue = cylinderTopViewSetting.FindMarkValueByName(CCylinderTopViewSetting.mstrWorkPointToWorkPoint);
            mMainBeam.SetNeedAddMarkFlag(bMarkValue);

            //1.判断工作点与工作点之间是否需要标注，需要标注的话是一道单尺寸;
            bool bNeedDimWpToWp = cylinderTopViewSetting.FindDimValueByName(CCylinderTopViewSetting.mstrWorkPointToWorkPoint);

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
        /// 绘制支撑板及牛腿上的螺钉在Y方向的标注;
        /// </summary>
        private void DrawSupportPlateBoltDimY()
        {
            CCylinderTopViewSetting cylinderTopViewSetting = CCylinderDimSetting.GetInstance().mTopViewSetting;

            if (!cylinderTopViewSetting.FindDimValueByName(CCylinderTopViewSetting.mstrSupportPlate))
            {
                return;
            }

            //移除主梁;
            mMrPartList.Remove(mMainBeam);

            foreach (CMrPart mrPart in mMrPartList)
            {
                CMrDimSet mrDimSet = mrPart.GetCylinderTopViewInfo().GetSupportPlateBoltYDimSet();

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
            CCylinderTopViewSetting cylinderTopViewSetting = CCylinderDimSetting.GetInstance().mTopViewSetting;

            mMrPartList.Remove(mMainBeam);

            foreach (CMrPart mrPart in mMrPartList)
            {
                CMrDimSet mrBoltDimSet = mrPart.GetCylinderTopViewInfo().GetSupportPlateBoltXDimSet();
                
                CMrDimSet mrConnectPlateDimSet = GetSupportPlateMiddleConnectPlateDimSet(mrPart);

                PointList pointList = new PointList();

                //1.支撑板或连接板是否需要标注;
                if (cylinderTopViewSetting.FindDimValueByName(CCylinderTopViewSetting.mstrSupportPlate))
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
                if (cylinderTopViewSetting.FindDimValueByName(CCylinderTopViewSetting.mstrConnentPlateOnSupport))
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
            CCylinderTopViewSetting cylinderTopViewSetting = CCylinderDimSetting.GetInstance().mTopViewSetting;

            mMrPartList.Remove(mMainBeam);

            foreach (CMrPart mrPart in mMrPartList)
            {
                CMrDimSet mrBoltDimSet = mrPart.GetCylinderTopViewInfo().GetAngleSheetBoltXDimSet();

                PointList pointList = new PointList();

                //1.支撑板或连接板是否需要标注;
                if (cylinderTopViewSetting.FindDimValueByName(CCylinderTopViewSetting.mstrAngleSheet))
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

            CCylinderTopViewSetting cylinderTopViewSetting = CCylinderDimSetting.GetInstance().mTopViewSetting;

            List<CMrPart> mrConnectPlates = GetSupportPlateMiddleConnectPlates(mrSupportPlate);

            if (mrConnectPlates == null || mrConnectPlates.Count == 0)
            {
                return null;
            }

            CMrDimSet mrDimSet = new CMrDimSet();

            foreach (CMrPart mrPart in mrConnectPlates)
            {
                mrDimSet.AddPoint(mrPart.GetMinYMinXPoint());
                bool bValue = cylinderTopViewSetting.FindMarkValueByName(CCylinderTopViewSetting.mstrConnentPlateOnSupport);
                mrPart.SetNeedAddMarkFlag(bValue);
            }

            if (mrDimSet.Count > 0)
            {
                mrDimSet.AddPoint(new Point(0, mrDimSet.GetDimPointList()[0].Y, 0));
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

            if (maxXPoint.X < 0)
            {
                foreach (CMrPart mrPart in mVerticalConnectPlateList)
                {
                    if (IsOutsidePlate(mrPart)||mrPart.GetMaxXPoint().X > 0)
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
            else if (minXPoint.X > 0)
            {
                foreach (CMrPart mrPart in mVerticalConnectPlateList)
                {
                    if (IsOutsidePlate(mrPart) || mrPart.GetMinXPoint().X < 0)
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
        /// 绘制主部件在Y向的螺钉标注;
        /// </summary>
        private void DrawMainPartBoltDimY()
        {
            PointList pointList = new PointList();
            CCylinderTopViewSetting cylinderTopViewSetting = CCylinderDimSetting.GetInstance().mTopViewSetting;

            //1.判断主梁的螺栓是否需要标注;
            bool bNeedMainBeamBoltDim = cylinderTopViewSetting.FindDimValueByName(CCylinderTopViewSetting.mstrBolt);

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
        /// 绘制主部件上螺钉X方向的标注;
        /// </summary>
        private void DrawMainPartBoltDimX()
        {
            CBeamFrontViewSetting beamFrontViewSetting = CBeamDimSetting.GetInstance().mFrontViewSetting;
            bool bNeedMainBeamBoltDim = beamFrontViewSetting.FindDimValueByName(CBeamFrontViewSetting.mstrBolt);
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

            //.主梁上剩下未标注的螺钉组;
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
            //根据组合好的螺钉组集合来进行标注;
            foreach (CMrBoltArrayGroup mrBoltArrayGroup in mrBoltArrayGroupList)
            {
                CMrDimSet yMrDimSet = new CMrDimSet();

                foreach (CMrBoltArray mrBoltArray in mrBoltArrayGroup.mrBoltArrayList)
                {
                    List<Point> minYPointList = mrBoltArray.GetMinYPointList();
                    yMrDimSet.AddRange(minYPointList);
                }
                yMrDimSet.AddPoint(new Point(0,yMrDimSet.GetDimPointList()[0].Y, 0));

                //需要判断螺钉是否在柱子的底端或上端;
                double maxY = mrBoltArrayGroup.GetMaxYPoint().Y;
                double minY = mrBoltArrayGroup.GetMinYPoint().Y;
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
