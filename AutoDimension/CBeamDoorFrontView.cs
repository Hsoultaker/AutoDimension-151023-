using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoDimension.Entity;

using Tekla.Structures.Model;
using Tekla.Structures;

using TSD = Tekla.Structures.Drawing;
using TSM = Tekla.Structures.Model;
using System.Threading;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Drawing;
using System.Windows.Forms;

namespace AutoDimension
{
    /// <summary>
    /// 门式钢架前视图标注;
    /// </summary>
    public class CBeamDoorFrontView : CView
    {
        /// <summary>
        /// 零件标记时需要去掉重复标注的点;
        /// </summary>
        private Dictionary<String, bool> mDicMarkDimPoints = new Dictionary<String, bool>();

        /// <summary>
        /// 构造函数;
        /// </summary>
        /// <param name="viewBase"></param>
        /// <param name="model"></param>
        public CBeamDoorFrontView(TSD.View viewBase, Model model)
            : base(viewBase)
        {
            mViewBase = viewBase;
            mModel = model;
        }

        /// <summary>
        /// 初始化视图;
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
                    CDimTools.GetInstance().InitMrPart(modelObjectInModel, mViewBase, mrPart);
                    mrPart.UpdatePartBoxPoint();
                    AppendMrPart(mrPart);
                }
                else
                {
                    mrPart = new CMrPart(partInModel, partInDrawing);
                    CDimTools.GetInstance().InitMrPart(modelObjectInModel, mViewBase, mrPart);
                    mrPart.UpdatePartBoxPoint();
                    AppendMrPart(mrPart);
                }

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
            //清空零件标记管理器中的零件标记;
            CMrMarkManager.GetInstance().Clear();
            
            //构建门式框架主梁的拓扑结构;
            CMrBeamDoorManager.GetInstance().BuildBeamDoorTopo(mMrPartList);
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
                if (CMrBeamDoorManager.GetInstance().mType == MrBeamDoorType.TypeNormal)
                {
                    DrawXUpDimNormal();
                    DrawXDownDimNormal();
                    DrawPartDimNormal();
                    DrawMainBeamBoltDimNormal();
                    DrawPartMarkNormal();
                }
                else 
                {
                    DrawXUpDimMiddle();
                    DrawXDownDimMiddle();
                    DrawPartDimMiddle();
                    DrawMainBeamBoltDimMiddle();
                    DrawPartMarkMiddle();
                }
#else
                try
                {
                    if (CMrBeamDoorManager.GetInstance().mType == MrBeamDoorType.TypeNormal)
                    {
                        DrawXUpDimNormal();
                        DrawXDownDimNormal();
                        DrawPartDimNormal();
                        DrawMainBeamBoltDimNormal();
                        DrawPartMarkNormal();
                    }
                    else 
                    {
                        DrawXUpDimMiddle();
                        DrawXDownDimMiddle();
                        DrawPartDimMiddle();
                        DrawMainBeamBoltDimMiddle();
                        DrawPartMarkMiddle();
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
        /// 对于正常的左右倾斜梁的上标注;
        /// </summary>
        public void DrawXUpDimNormal()
        {
            bool bNeedUpDim = false;

            List<Point> upDimPointList = new List<Point>();

            foreach (CMrPart mrPart in mMrPartList)
            {
                CMrDimSet partDimSet = mrPart.GetBeamDoorFrontViewInfo().GetXUpDimSetNormal();

                if (partDimSet != null && partDimSet.Count > 0)
                {
                    bNeedUpDim = true;
                    upDimPointList.AddRange(partDimSet.GetDimPointList());
                }
            }

            if (bNeedUpDim == false)
            {
                return;
            }

            CMrPart topBeam = CMrBeamDoorManager.GetInstance().mTopBeam;

            upDimPointList.Add(topBeam.mLeftTopPoint);
            upDimPointList.Add(topBeam.mRightTopPoint);

            Comparison<Point> sorterX = new Comparison<Point>(CDimTools.ComparePointX);
            upDimPointList.Sort(sorterX);

            PointList pointList = new PointList();

            foreach (Point point in upDimPointList)
            {
                pointList.Add(point);
            }

            Point MinXPoint = upDimPointList[0];
            double dimDistance = Math.Abs(CCommonPara.mViewMaxY - MinXPoint.Y) + 2 * CCommonPara.mDefaultDimDistance;

            Vector upDimVector = CMrBeamDoorManager.GetInstance().GetTopBeamUpDimVector();
            CDimTools.GetInstance().DrawDimensionSet(mViewBase, pointList, upDimVector, dimDistance, CCommonPara.mSizeDimPath);

            //再标注一个总长度;
            pointList.Clear();

            Point firstPoint = topBeam.mLeftTopPoint;
            Point secondPoint = topBeam.mRightTopPoint;

            pointList.Add(firstPoint);
            pointList.Add(secondPoint);

            dimDistance = Math.Abs(CCommonPara.mViewMaxY - firstPoint.Y) + 4 * CCommonPara.mDefaultDimDistance;

            CDimTools.GetInstance().DrawDimensionSet(mViewBase, pointList, upDimVector, dimDistance, CCommonPara.mSizeDimPath);
        }

        /// <summary>
        /// 对于正常的左右倾斜梁的下标注;
        /// </summary>
        public void DrawXDownDimNormal()
        {
            List<Point> downDimPointList = new List<Point>();

            foreach (CMrPart mrPart in mMrPartList)
            {
                CMrDimSet partDimSet = mrPart.GetBeamDoorFrontViewInfo().GetXDownDimSetNormal();

                if (partDimSet != null && partDimSet.Count > 0)
                {
                    downDimPointList.AddRange(partDimSet.GetDimPointList());
                }
            }

            CMrPart topBeam = CMrBeamDoorManager.GetInstance().mTopBeam;
            CMrPart bottomBeam = CMrBeamDoorManager.GetInstance().mBottonBeam;
            CMrPart leftBeam = CMrBeamDoorManager.GetInstance().mLeftBeam;
            CMrPart rightBeam = CMrBeamDoorManager.GetInstance().mRightBeam;

            downDimPointList.Add(topBeam.mLeftTopPoint);
            downDimPointList.Add(bottomBeam.mLeftBottomPoint);
            downDimPointList.Add(topBeam.mRightTopPoint);
            downDimPointList.Add(bottomBeam.mRightBottomPoint);

            Comparison<Point> sorterX = new Comparison<Point>(CDimTools.ComparePointX);
            downDimPointList.Sort(sorterX);

            PointList pointList = new PointList();

            foreach (Point point in downDimPointList)
            {
                pointList.Add(point);
            }

            double dimDistance = Math.Abs(CCommonPara.mViewMinY - downDimPointList[0].Y) + 1.5 * CCommonPara.mDefaultDimDistance;
            Vector downDimVector = new Vector(0, -1, 0);
            CDimTools.GetInstance().DrawDimensionSet(mViewBase, pointList, downDimVector, dimDistance, CCommonPara.mSizeDimPath);
        }

        /// <summary>
        /// 对主梁内部的一些零件进行标注;
        /// </summary>
        public void DrawPartDimNormal()
        {
            DrawLeftAndRightBeamDimNormal();
            DrawMainBeamMiddlePartDimNormal();
        }

        /// <summary>
        /// 绘制左右板的标注;
        /// </summary>
        private void DrawLeftAndRightBeamDimNormal()
        {
            CMrPart leftBeam = CMrBeamDoorManager.GetInstance().mLeftBeam;
            CMrPart rightBeam = CMrBeamDoorManager.GetInstance().mRightBeam;
            CMrPart topBeam = CMrBeamDoorManager.GetInstance().mTopBeam;
            CMrPart bottomBeam = CMrBeamDoorManager.GetInstance().mBottonBeam;

            if (IsLeftBeamNeedDim())
            {
                //1.标注左侧的挡板零件;
                PointList pointList = new PointList();
                pointList.Add(leftBeam.mLeftBottomPoint);
                pointList.Add(leftBeam.mLeftTopPoint);
                pointList.Add(bottomBeam.mLeftBottomPoint);
                pointList.Add(topBeam.mLeftTopPoint);

                double dimDistance = CCommonPara.mDefaultDimDistance;
                Vector dimVector = CMrBeamDoorManager.GetInstance().GetLeftBeamLeftDimVector();
                CDimTools.GetInstance().DrawDimensionSet(mViewBase, pointList, dimVector, dimDistance, CCommonPara.mSizeDimPath);
            }
            if (IsRightBeamNeedDim())
            {
                //2.标注右侧的挡板零件;
                PointList pointList = new PointList();
                pointList.Add(rightBeam.mRightBottomPoint);
                pointList.Add(rightBeam.mRightTopPoint);
                pointList.Add(bottomBeam.mRightBottomPoint);
                pointList.Add(topBeam.mRightTopPoint);

                double dimDistance = CCommonPara.mDefaultDimDistance;
                Vector dimVector = CMrBeamDoorManager.GetInstance().GetRightBeamRightDimVector();
                CDimTools.GetInstance().DrawDimensionSet(mViewBase, pointList, dimVector, dimDistance, CCommonPara.mSizeDimPath);
            }
        }

        /// <summary>
        /// 判断左侧挡板零件是否需要进行标注;
        /// </summary>
        /// <returns></returns>
        private bool IsLeftBeamNeedDim()
        {
            CMrPart leftBeam = CMrBeamDoorManager.GetInstance().mLeftBeam;
            CMrPart topBeam = CMrBeamDoorManager.GetInstance().mTopBeam;
            CMrPart bottomBeam = CMrBeamDoorManager.GetInstance().mBottonBeam;

            double distance1=Math.Abs(leftBeam.mRightTopPoint.X-topBeam.mLeftTopPoint.X);
            double distance2=Math.Abs(leftBeam.mRightBottomPoint.X-bottomBeam.mLeftBottomPoint.X);

            if (distance1 > 10||distance2 > 10)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 判断右侧挡板零件是否需要标注;
        /// </summary>
        /// <returns></returns>
        private bool IsRightBeamNeedDim()
        {
            CMrPart rightBeam = CMrBeamDoorManager.GetInstance().mRightBeam;
            CMrPart topBeam = CMrBeamDoorManager.GetInstance().mTopBeam;
            CMrPart bottomBeam = CMrBeamDoorManager.GetInstance().mBottonBeam;

            double distance1 = Math.Abs(rightBeam.mLeftTopPoint.X - topBeam.mRightTopPoint.X);
            double distance2 = Math.Abs(rightBeam.mLeftBottomPoint.X - bottomBeam.mRightBottomPoint.X);

            if (distance1 > 10 || distance2 > 10)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 绘制主梁中间的零部件标注;
        /// 主梁中间的零部件只有法向与主梁顶部部件平行的才进行标注;
        /// </summary>
        private void DrawMainBeamMiddlePartDimNormal()
        {
            CMrPart topBeam=CMrBeamDoorManager.GetInstance().mTopBeam;
            
            Point leftTopPoint = topBeam.mLeftTopPoint;
            Point rightTopPoint = topBeam.mRightTopPoint;

            CMrBeamDoorManager.GetInstance().BuildMostNearPartToPartList(mMrPartList);

            Dictionary<CMrPart, List<CMrPart>> dicPartToPartList = CMrBeamDoorManager.GetInstance().mDicPartToPartList;

            List<CMrDimSet> mrDimSetList = new List<CMrDimSet>();

            foreach (CMrPart mostNearPart in dicPartToPartList.Keys)
            {
                List<CMrPart> mrPartList = dicPartToPartList[mostNearPart];

                if (mrPartList.Count == 0)
                {
                    continue;
                }

                //(1).与顶板平行方向标注螺钉;
                CMrDimSet mrDimSet = new CMrDimSet();

                MrSlopeType slopeType = CDimTools.GetInstance().JudgeLineSlope(leftTopPoint, rightTopPoint);

                if (slopeType == MrSlopeType.MORETHAN_ZERO)
                {
                    mrDimSet.AddPoint(mostNearPart.mRightTopPoint);
                }
                else
                {
                    mrDimSet.AddPoint(mostNearPart.mLeftTopPoint);
                }

                foreach (CMrPart mrPart in mrPartList)
                {
                    CMrBoltArray mrBoltArray = mrPart.GetBoltArrayList()[0];
                    Point boltPoint = mrBoltArray.GetMaxXPoint();
                    mrDimSet.AddPoint(boltPoint);
                }

                mrDimSet.mDimVector = CMrBeamDoorManager.GetInstance().GetTopBeamUpDimVector();
                mrDimSet.mDimDistance = CCommonPara.mDefaultDimDistance;
                mrDimSetList.Add(mrDimSet);

                //(2).与顶板竖直方向标注;
                mrDimSet = new CMrDimSet();
                mrDimSet.AddPoint(mrPartList[0].mRightBottomPoint);

                Point fontPt = CDimTools.GetInstance().ComputeFootPointToLine(mrPartList[0].mRightBottomPoint, topBeam.mLeftTopPoint, topBeam.mRightTopPoint);
                mrDimSet.AddPoint(fontPt);
                mrDimSet.mDimDistance = CCommonPara.mDefaultDimDistance;

                mrDimSet.mDimVector = new Vector(rightTopPoint.X - leftTopPoint.X, rightTopPoint.Y - leftTopPoint.Y, 0);
                mrDimSetList.Add(mrDimSet);
            }
            foreach (CMrDimSet mrDimSet in mrDimSetList)
            {
                List<Point> dimPointList = mrDimSet.GetDimPointList();

                PointList pointList = new PointList();

                foreach (Point point in dimPointList)
                {
                    pointList.Add(point);
                }

                Vector dimVector = mrDimSet.mDimVector;
                double length = mrDimSet.mDimDistance;

                CDimTools.GetInstance().DrawDimensionSet(mViewBase, pointList, dimVector, length, CCommonPara.mSizeDimPath);
            }
        }

        /// <summary>
        /// 绘制主梁上螺钉的标注;
        /// </summary>
        private void DrawMainBeamBoltDimNormal()
        {
            CMrPart topBeam = CMrBeamDoorManager.GetInstance().mTopBeam;
            
            Point leftTopPoint = topBeam.mLeftTopPoint;
            Point rightTopPoint = topBeam.mRightTopPoint;

            List<CMrBoltArray> mrBoltArrayList = mMainBeam.GetBoltArrayList();

            foreach (CMrBoltArray mrBoltArray in mrBoltArrayList)
            {
                Point boltPoint = mrBoltArray.GetMaxXPoint();

                //寻找与该零件最近的零部件;
                CMrPart mostNearPart = CMrBeamDoorManager.GetInstance().GetMostNearPart(boltPoint);

                if (mostNearPart == null)
                {
                    continue;
                }

                //(1).与顶板平行方向标注螺钉;
                PointList pointList = new PointList();

                MrSlopeType slopeType = CDimTools.GetInstance().JudgeLineSlope(leftTopPoint, rightTopPoint);

                if (slopeType == MrSlopeType.MORETHAN_ZERO)
                {
                    pointList.Add(mostNearPart.mRightTopPoint);
                }
                else
                {
                    pointList.Add(mostNearPart.mLeftTopPoint);
                }

                pointList.Add(boltPoint);

                Vector dimVector = CMrBeamDoorManager.GetInstance().GetTopBeamUpDimVector();
                double dblDistance = CCommonPara.mDefaultDimDistance;
                CDimTools.GetInstance().DrawDimensionSet(mViewBase, pointList, dimVector, dblDistance, CCommonPara.mSizeDimPath);

                //(2).与顶板竖直方向标注板的位置;
                pointList.Clear();
                pointList.Add(boltPoint);
                Point fontPt = CDimTools.GetInstance().ComputeFootPointToLine(boltPoint, topBeam.mLeftTopPoint, topBeam.mRightTopPoint);
                pointList.Add(fontPt);
                dimVector = new Vector(rightTopPoint.X - leftTopPoint.X, rightTopPoint.Y - leftTopPoint.Y, 0);
                CDimTools.GetInstance().DrawDimensionSet(mViewBase, pointList, dimVector, dblDistance, CCommonPara.mSizeDimPath);
            }
        }

        /// <summary>
        /// 绘制上翼板左右倾斜时的零件标记;
        /// </summary>
        private void DrawPartMarkNormal()
        {
            mMrPartList.Remove(mMainBeam);
            
            DrawingHandler drawingHandler = new DrawingHandler();
            TSD.UI.DrawingObjectSelector DS = drawingHandler.GetDrawingObjectSelector();

            foreach (CMrPart mrPart in mMrPartList)
            {
                CMrMark mrMark = mrPart.GetBeamDoorFrontViewInfo().GetPartMarkNormal();

                if (null == mrMark)
                {
                    continue;
                }
                DS.SelectObject(mrPart.mPartInDrawing);
            }

            mMrPartList.Add(mMainBeam);

            DS.SelectObject(mMainBeam.mPartInDrawing);

            //绘制主梁上的螺钉标记;
            DrawMainBeamBoltMark(1);

            CDimTools.GetInstance().DrawMarkByMacro();
        }

        /// <summary>
        /// 绘制主梁上的螺钉标记;
        /// </summary>
        /// <param name="nFlag">nFlag=1：上翼板是倾斜。nFlag=2:上翼板向两侧倾斜</param>
        private void DrawMainBeamBoltMark(int nFlag)
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
        }

        /// <summary>
        /// 绘制第三种中间梁的上标注;
        /// </summary>
        public void DrawXUpDimMiddle()
        {
            MrBeamDoorType beamDoorType = CMrBeamDoorManager.GetInstance().mType;

            if (beamDoorType == MrBeamDoorType.TypeMiddle1 || beamDoorType == MrBeamDoorType.TypeMiddle2)
            {
                DrawTypeMiddle1And2XUpDim();
            }
            if (beamDoorType == MrBeamDoorType.TypeMiddle3)
            {
                DrawTypeMiddle3XUpDim();
            }
        }

        /// <summary>
        /// 绘制中间主梁的类型为1和2时向上的标注;
        /// </summary>
        private void DrawTypeMiddle1And2XUpDim()
        {
            //1.绘制左侧上方的标注;
            bool bNeedUpDim = false;

            List<Point> upDimPointList = new List<Point>();

            foreach (CMrPart mrPart in mMrPartList)
            {
                CMrDimSet partDimSet = mrPart.GetBeamDoorFrontViewInfo().GetLeftXUpDimSetMiddle();

                if (partDimSet != null && partDimSet.Count > 0)
                {
                    bNeedUpDim = true;
                    upDimPointList.AddRange(partDimSet.GetDimPointList());
                }
            }
            if (bNeedUpDim == false)
            {
                return;
            }

            CMrPart topBeam = CMrBeamDoorManager.GetInstance().mTopBeam;

            //把中间Y值最大的点加入到标注链表中;
            upDimPointList.Add(CMrBeamDoorManager.GetInstance().mMidMaxPoint);
            upDimPointList.Add(topBeam.mLeftTopPoint);

            Comparison<Point> sorterX = new Comparison<Point>(CDimTools.ComparePointX);
            upDimPointList.Sort(sorterX);

            PointList pointList = new PointList();
            foreach (Point point in upDimPointList)
            {
                pointList.Add(point);
            }

            Point minXPoint1 = upDimPointList[0];

            double dimDistance = Math.Abs(CCommonPara.mViewMaxY - minXPoint1.Y) + 2 * CCommonPara.mDefaultDimDistance;
            Vector upDimVector = CMrBeamDoorManager.GetInstance().mLeftTopVector;
            CDimTools.GetInstance().DrawDimensionSet(mViewBase, pointList, upDimVector, dimDistance, CCommonPara.mSizeDimPath);

            //2.再标注左侧的一个总长度;
            pointList.Clear();

            Point firstPoint = topBeam.mLeftTopPoint;
            Point secondPoint = CMrBeamDoorManager.GetInstance().mMidMaxPoint;

            pointList.Add(firstPoint);
            pointList.Add(secondPoint);

            dimDistance = Math.Abs(CCommonPara.mViewMaxY - firstPoint.Y) + 4 * CCommonPara.mDefaultDimDistance;

            CDimTools.GetInstance().DrawDimensionSet(mViewBase, pointList, upDimVector, dimDistance, CCommonPara.mSizeDimPath);

            //3.绘制右侧上方的标注;
            bNeedUpDim = false;
            upDimPointList.Clear();

            foreach (CMrPart mrPart in mMrPartList)
            {
                CMrDimSet partDimSet = mrPart.GetBeamDoorFrontViewInfo().GetRightXUpDimSetMiddle();

                if (partDimSet != null && partDimSet.Count > 0)
                {
                    bNeedUpDim = true;
                    upDimPointList.AddRange(partDimSet.GetDimPointList());
                }
            }
            if (bNeedUpDim == false)
            {
                return;
            }

            //把中间Y值最大的点加入到标注链表中;
            upDimPointList.Add(CMrBeamDoorManager.GetInstance().mMidMaxPoint);
            upDimPointList.Add(topBeam.mRightTopPoint);
            upDimPointList.Sort(sorterX);

            int nCount = upDimPointList.Count;
            upDimPointList.Reverse(0, nCount);

            pointList.Clear();

            foreach (Point point in upDimPointList)
            {
                pointList.Add(point);
            }

            Point minXPoint2 = upDimPointList[0];
            dimDistance = Math.Abs(CCommonPara.mViewMaxY - minXPoint2.Y) + 2 * CCommonPara.mDefaultDimDistance;
            upDimVector = CMrBeamDoorManager.GetInstance().mRightTopVector;

            CDimTools.GetInstance().DrawDimensionSet(mViewBase, pointList, upDimVector, dimDistance, CCommonPara.mSizeDimPath);

            //4.再标注右侧的一个总长度;
            pointList.Clear();

            firstPoint = topBeam.mRightTopPoint;
            secondPoint = CMrBeamDoorManager.GetInstance().mMidMaxPoint;

            pointList.Add(firstPoint);
            pointList.Add(secondPoint);

            dimDistance = Math.Abs(CCommonPara.mViewMaxY - firstPoint.Y) + 4 * CCommonPara.mDefaultDimDistance;
            CDimTools.GetInstance().DrawDimensionSet(mViewBase, pointList, upDimVector, dimDistance, CCommonPara.mSizeDimPath);
        }

        /// <summary>
        /// 绘制中间主梁类型为3时向上的标注;
        /// </summary>
        private void DrawTypeMiddle3XUpDim()
        {
            //1.绘制左侧上方的标注;
            bool bNeedUpDim = false;

            List<Point> upDimPointList = new List<Point>();

            foreach (CMrPart mrPart in mMrPartList)
            {
                CMrDimSet partDimSet = mrPart.GetBeamDoorFrontViewInfo().GetTypeMiddle3XUpDimSetMiddle();

                if (partDimSet != null && partDimSet.Count > 0)
                {
                    bNeedUpDim = true;
                    upDimPointList.AddRange(partDimSet.GetDimPointList());
                }
            }
            if (bNeedUpDim == false)
            {
                return;
            }

            CMrPart topBeam = CMrBeamDoorManager.GetInstance().mTopBeam;

            Point leftTopPt = topBeam.mLeftTopPoint;
            Point rightTopPt = topBeam.mRightTopPoint;

            upDimPointList.Add(leftTopPt);
            upDimPointList.Add(rightTopPt);

            Comparison<Point> sorterX = new Comparison<Point>(CDimTools.ComparePointX);
            upDimPointList.Sort(sorterX);

            PointList pointList = new PointList();
            foreach (Point point in upDimPointList)
            {
                pointList.Add(point);
            }

            double dimDistance = Math.Abs(CCommonPara.mViewMaxY - upDimPointList[0].Y) + 2 * CCommonPara.mDefaultDimDistance;

            //计算标注向量;
            Vector directVector = new Vector(rightTopPt.X - leftTopPt.X, rightTopPt.Y - leftTopPt.Y, 0);
            directVector.Normalize();
            Vector upDimVector = new Vector(directVector.Y, -directVector.X, 0);

            MrSlopeType mrSlopeType = CDimTools.GetInstance().JudgeLineSlope(directVector, new Point(0, 0, 0));

            if (mrSlopeType == MrSlopeType.LESSTHAN_ZERO)
            {
                upDimVector = new Vector(Math.Abs(upDimVector.X), Math.Abs(upDimVector.Y), 0);
            }
            else if (mrSlopeType == MrSlopeType.MORETHAN_ZERO)
            {
                upDimVector = new Vector(-Math.Abs(upDimVector.X), Math.Abs(upDimVector.Y), 0);
            }

            CDimTools.GetInstance().DrawDimensionSet(mViewBase, pointList, upDimVector, dimDistance, CCommonPara.mSizeDimPath);

            //2.再标注一个总长度;
            pointList.Clear();
            pointList.Add(leftTopPt);
            pointList.Add(rightTopPt);

            dimDistance = Math.Abs(CCommonPara.mViewMaxY - leftTopPt.Y) + 4 * CCommonPara.mDefaultDimDistance;

            CDimTools.GetInstance().DrawDimensionSet(mViewBase, pointList, upDimVector, dimDistance, CCommonPara.mSizeDimPath);
        }

        /// <summary>
        /// 绘制第三种中间梁的下标注;
        /// </summary>
        public void DrawXDownDimMiddle()
        {
            bool bNeedDownDim = false;

            List<Point> downDimPointList = new List<Point>();

            foreach (CMrPart mrPart in mMrPartList)
            {
                CMrDimSet partDimSet = mrPart.GetBeamDoorFrontViewInfo().GetXDownDimSetMiddle();

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

            CMrPart topBeam=CMrBeamDoorManager.GetInstance().mTopBeam;
            CMrPart leftBottomBeam=CMrBeamDoorManager.GetInstance().mLeftBottomBeam;
            CMrPart rightBottomBeam=CMrBeamDoorManager.GetInstance().mRightBottomBeam;

            downDimPointList.Add(leftBottomBeam.mLeftBottomPoint);
            downDimPointList.Add(topBeam.mLeftTopPoint);
            downDimPointList.Add(rightBottomBeam.mRightBottomPoint);
            downDimPointList.Add(topBeam.mRightTopPoint);

            Comparison<Point> sorterX = new Comparison<Point>(CDimTools.ComparePointX);
            downDimPointList.Sort(sorterX);

            PointList pointList = new PointList();

            foreach (Point point in downDimPointList)
            {
                pointList.Add(point);
            }

            Point MinXPoint = downDimPointList[0];

            double dimDistance = Math.Abs(CCommonPara.mViewMinY - MinXPoint.Y) + 1.2 * CCommonPara.mDefaultDimDistance;

            Vector downDimVector = new Vector(0, -1, 0);

            CDimTools.GetInstance().DrawDimensionSet(mViewBase, pointList, downDimVector, dimDistance, CCommonPara.mSizeDimPath);
        }

        /// <summary>
        /// 绘制第三种中间梁的中间零件的标注;
        /// </summary>
        public void DrawPartDimMiddle()
        {
            DrawLeftAndRightBeamDimMiddle();
            DrawMainBeamMiddlePartDimMiddle();
            DrawYNormalPartDimMiddle();
        }

        /// <summary>
        /// 绘制上翼板向两边弯曲时的左右两块端板的标注;
        /// </summary>
        private void DrawLeftAndRightBeamDimMiddle()
        {
            CMrPart leftBeam = CMrBeamDoorManager.GetInstance().mLeftBeam;
            CMrPart rightBeam = CMrBeamDoorManager.GetInstance().mRightBeam;
            CMrPart topBeam = CMrBeamDoorManager.GetInstance().mTopBeam;
            CMrPart leftBottomBeam = CMrBeamDoorManager.GetInstance().mLeftBottomBeam;
            CMrPart rightBottomBeam = CMrBeamDoorManager.GetInstance().mRightBottomBeam;

            //1.标注左侧的挡板零件;
            PointList pointList = new PointList();
            pointList.Add(leftBeam.mLeftBottomPoint);
            pointList.Add(leftBeam.mLeftTopPoint);
            pointList.Add(leftBottomBeam.mLeftBottomPoint);
            pointList.Add(topBeam.mLeftTopPoint);

            double dimDistance = CCommonPara.mDefaultDimDistance;
            Vector dimVector = CMrBeamDoorManager.GetInstance().GetLeftBeamLeftDimVector();

            CDimTools.GetInstance().DrawDimensionSet(mViewBase, pointList, dimVector, dimDistance, CCommonPara.mSizeDimPath);

            //2.标注右侧的挡板零件;
            pointList.Clear();
            pointList.Add(rightBeam.mRightBottomPoint);
            pointList.Add(rightBeam.mRightTopPoint);
            pointList.Add(rightBottomBeam.mRightBottomPoint);
            pointList.Add(topBeam.mRightTopPoint);

            dimDistance = CCommonPara.mDefaultDimDistance;
            dimVector = CMrBeamDoorManager.GetInstance().GetRightBeamRightDimVector();

            CDimTools.GetInstance().DrawDimensionSet(mViewBase, pointList, dimVector, dimDistance, CCommonPara.mSizeDimPath);
        }

        /// <summary>
        /// 绘制主梁中间的零部件标注;
        /// 主梁中间的零部件只有法向与主梁顶部部件平行的才进行标注;
        /// </summary>
        private void DrawMainBeamMiddlePartDimMiddle()
        {
            CMrPart topBeam = CMrBeamDoorManager.GetInstance().mTopBeam;

            Point midMaxPoint = CMrBeamDoorManager.GetInstance().mMidMaxPoint;
            Point leftTopPoint = topBeam.mLeftTopPoint;
            Point rightTopPoint = topBeam.mRightTopPoint;

            CMrBeamDoorManager.GetInstance().BuildMostNearPartToPartList(mMrPartList);

            Dictionary<CMrPart, List<CMrPart>> dicPartToPartList = CMrBeamDoorManager.GetInstance().mDicPartToPartList;

            List<CMrDimSet> mrDimSetList = new List<CMrDimSet>();

            foreach (CMrPart mostNearPart in dicPartToPartList.Keys)
            {
                List<CMrPart> mrPartList = dicPartToPartList[mostNearPart];

                if (mrPartList.Count == 0)
                {
                    continue;
                }

                //(1).与顶板平行方向标注螺钉;
                CMrDimSet mrDimSet = new CMrDimSet();

                if (mostNearPart.mLeftTopPoint.X < midMaxPoint.X)
                {
                    mrDimSet.AddPoint(mostNearPart.mRightTopPoint);
                }
                else
                {
                    mrDimSet.AddPoint(mostNearPart.mLeftTopPoint);
                }

                //如果在上翼板的左侧;
                if (mostNearPart.mLeftTopPoint.X < midMaxPoint.X)
                {
                    foreach (CMrPart mrPart in mrPartList)
                    {
                        CMrBoltArray mrBoltArray = mrPart.GetBoltArrayList()[0];
                        Point boltPoint = mrBoltArray.GetMaxXPoint();
                        mrDimSet.AddPoint(boltPoint);
                    }
                    mrDimSet.mDimVector = CMrBeamDoorManager.GetInstance().mLeftTopVector;
                    mrDimSet.mDimDistance = CCommonPara.mDefaultDimDistance;
                    mrDimSetList.Add(mrDimSet);

                    //(2).与顶板竖直方向标注;
                    mrDimSet = new CMrDimSet();
                    mrDimSet.AddPoint(mrPartList[0].mLeftBottomPoint);

                    Point fontPt = CDimTools.GetInstance().ComputeFootPointToLine(mrPartList[0].mLeftBottomPoint, leftTopPoint, midMaxPoint);
                    mrDimSet.AddPoint(fontPt);
                    mrDimSet.mDimDistance = CCommonPara.mDefaultDimDistance;

                    mrDimSet.mDimVector = new Vector(leftTopPoint.X - midMaxPoint.X, leftTopPoint.Y - midMaxPoint.Y, 0);
                    mrDimSetList.Add(mrDimSet);
                }
                //如果在上翼板的右侧;
                else if (mostNearPart.mLeftTopPoint.X > midMaxPoint.X)
                {
                    foreach (CMrPart mrPart in mrPartList)
                    {
                        CMrBoltArray mrBoltArray = mrPart.GetBoltArrayList()[0];
                        Point boltPoint = mrBoltArray.GetMaxXPoint();
                        mrDimSet.AddPoint(boltPoint);
                    }

                    mrDimSet.mDimVector = CMrBeamDoorManager.GetInstance().mRightTopVector;
                    mrDimSet.mDimDistance = CCommonPara.mDefaultDimDistance;
                    mrDimSetList.Add(mrDimSet);

                    //(2).与顶板竖直方向标注;
                    mrDimSet = new CMrDimSet();
                    mrDimSet.AddPoint(mrPartList[0].mRightBottomPoint);

                    Point fontPt = CDimTools.GetInstance().ComputeFootPointToLine(mrPartList[0].mRightBottomPoint, rightTopPoint, midMaxPoint);
                    mrDimSet.AddPoint(fontPt);
                    mrDimSet.mDimDistance = CCommonPara.mDefaultDimDistance;

                    mrDimSet.mDimVector = new Vector(rightTopPoint.X - midMaxPoint.X, rightTopPoint.Y - midMaxPoint.Y, 0);
                    mrDimSetList.Add(mrDimSet);
                }
            }
            foreach (CMrDimSet mrDimSet in mrDimSetList)
            {
                List<Point> dimPointList = mrDimSet.GetDimPointList();

                PointList pointList = new PointList();

                foreach (Point point in dimPointList)
                {
                    pointList.Add(point);
                }

                Vector dimVector = mrDimSet.mDimVector;
                double length = mrDimSet.mDimDistance;

                CDimTools.GetInstance().DrawDimensionSet(mViewBase, pointList, dimVector, length, CCommonPara.mSizeDimPath);
            }
        }

        /// <summary>
        /// 绘制在中间法向与Y轴方向平行的零件标注;
        /// </summary>
        public void DrawYNormalPartDimMiddle()
        {
            CMrPart midYNormalPart = null;
            Point midPoint = CMrBeamDoorManager.GetInstance().mMidMaxPoint;

            CMrPart topBeam = CMrBeamDoorManager.GetInstance().mTopBeam;
            CMrPart leftBottomBeam = CMrBeamDoorManager.GetInstance().mLeftBottomBeam;
            CMrPart rightBottomBeam = CMrBeamDoorManager.GetInstance().mRightBottomBeam;

            foreach (CMrPart mrPart in mMrPartList)
            {
                if (mrPart == topBeam || mrPart == leftBottomBeam || mrPart == rightBottomBeam)
                {
                    continue;
                }
              
                Vector normal = mrPart.mNormal;
                
                if (!CDimTools.GetInstance().IsTwoVectorParallel(normal, new Vector(0, 1, 0)))
                {
                    continue;
                }

                Point minXPoint = mrPart.GetMinXPoint();
                Point maxXPoint = mrPart.GetMaxXPoint();

                if (minXPoint.X < midPoint.X && maxXPoint.X > midPoint.X)
                {
                    if (midYNormalPart == null)
                    {
                        midYNormalPart = mrPart;
                    }
                    else if (mrPart.GetMaxYPoint().Y > midYNormalPart.GetMaxYPoint().Y)
                    {
                        midYNormalPart = mrPart;
                    }
                }
            }
            if (midYNormalPart == null)
            {
                return;
            }

            PointList pointList = new PointList();

            pointList.Add(midYNormalPart.GetMaxXMinYPoint());
            pointList.Add(rightBottomBeam.GetMinXMaxYPoint());

            Vector dimVector = new Vector(1, 0, 0);
            double length = 2 * CCommonPara.mDefaultDimDistance;

            CDimTools.GetInstance().DrawDimensionSet(mViewBase, pointList, dimVector, length, CCommonPara.mSizeDimPath);
        }


        /// <summary>
        /// 绘制第三种主梁上的螺钉的标注;
        /// </summary>
        public void DrawMainBeamBoltDimMiddle()
        {
            CMrPart topBeam = CMrBeamDoorManager.GetInstance().mTopBeam;

            Point leftTopPoint = topBeam.mLeftTopPoint;
            Point rightTopPoint = topBeam.mRightTopPoint;
            Point midTopPoint = CMrBeamDoorManager.GetInstance().mMidMaxPoint;

            List<CMrBoltArray> mrBoltArrayList = mMainBeam.GetBoltArrayList();

            foreach (CMrBoltArray mrBoltArray in mrBoltArrayList)
            {
                Point boltPoint = mrBoltArray.GetMaxXPoint();

                //寻找与该零件最近的零部件;
                CMrPart mostNearPart = CMrBeamDoorManager.GetInstance().GetMostNearPart(boltPoint);

                //(1).与顶板平行方向标注螺钉;
                PointList pointList = new PointList();

                if (mostNearPart.mLeftTopPoint.X < midTopPoint.X)
                {
                    pointList.Add(mostNearPart.mRightTopPoint);
                }
                else
                {
                    pointList.Add(mostNearPart.mLeftTopPoint);
                }

                pointList.Add(boltPoint);
               
                Vector dimVector =new Vector();
                
                if (boltPoint.X > midTopPoint.X)
                {
                    dimVector = CMrBeamDoorManager.GetInstance().mRightTopVector;
                }
                if (boltPoint.X < midTopPoint.X)
                {
                    dimVector = CMrBeamDoorManager.GetInstance().mLeftTopVector;
                }

                double dblDistance = CCommonPara.mDefaultDimDistance;

                CDimTools.GetInstance().DrawDimensionSet(mViewBase, pointList, dimVector, dblDistance, CCommonPara.mSizeDimPath);

                //(2).与顶板竖直方向标注板的位置;
                pointList.Clear();
                pointList.Add(boltPoint);

                if (boltPoint.X > midTopPoint.X)
                {
                    Point fontPt = CDimTools.GetInstance().ComputeFootPointToLine(boltPoint, rightTopPoint,midTopPoint);
              
                    dimVector = new Vector(rightTopPoint.X - midTopPoint.X, rightTopPoint.Y - midTopPoint.Y, 0);

                    pointList.Add(fontPt);
                }
                if (boltPoint.X < midTopPoint.X)
                {
                    Point fontPt = CDimTools.GetInstance().ComputeFootPointToLine(boltPoint, leftTopPoint, midTopPoint);

                    dimVector = new Vector(leftTopPoint.X - midTopPoint.X, leftTopPoint.Y - midTopPoint.Y, 0);

                    pointList.Add(fontPt);
                }
                CDimTools.GetInstance().DrawDimensionSet(mViewBase, pointList, dimVector, dblDistance, CCommonPara.mSizeDimPath);
            }
        }

        /// <summary>
        /// 绘制上翼板是两边弯曲时的零件标记;
        /// </summary>
        private void DrawPartMarkMiddle()
        {
            DrawingHandler drawingHandler = new DrawingHandler();
            TSD.UI.DrawingObjectSelector DS = drawingHandler.GetDrawingObjectSelector();

            mMrPartList.Remove(mMainBeam);

            foreach (CMrPart mrPart in mMrPartList)
            {
                CMrMark mrMark = mrPart.GetBeamDoorFrontViewInfo().GetPartMarkMiddle();

                if (null == mrMark)
                {
                    continue;
                }
                DS.SelectObject(mrPart.mPartInDrawing);
            }

            mMrPartList.Add(mMainBeam);

            DS.SelectObject(mMainBeam.mPartInDrawing);

            //绘制主梁上的螺钉标记;
            DrawMainBeamBoltMark(2);

            CDimTools.GetInstance().DrawMarkByMacro();
        }

        /// <summary>
        /// 判断该Mark是否需要标注;
        /// </summary>
        /// <returns></returns>
        private bool IsNeedMarkDim(CMrMark mrMark)
        {
            Point insertPoint = mrMark.mInsertPoint;

            string strAttribute = ((int)insertPoint.X).ToString() + "_" + ((int)insertPoint.Y).ToString();

            if (mDicMarkDimPoints.ContainsKey(strAttribute))
            {
                return false;
            }
            else
            {
                mDicMarkDimPoints.Add(strAttribute, true);

                return true;
            }
        }
    }
}
