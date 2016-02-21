using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tekla.Structures.Model;
using Tekla.Structures.Drawing;
using Tekla.Structures;

using TSM = Tekla.Structures.Model;
using TSD = Tekla.Structures.Drawing;
using AutoDimension.Entity;
using System.Threading;
using Tekla.Structures.Geometry3d;
using System.Windows.Forms;

namespace AutoDimension
{
    /// <summary>
    /// 门式框架的前视图对象;
    /// </summary>
    public class CCylinderDoorFrontView:CView
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
        public CCylinderDoorFrontView(TSD.View viewBase, Model model)
            : base(viewBase)
        {
            mViewBase = viewBase;
            mModel = model;
        }

        /// <summary>
        /// 初始化前视图;
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
                    AppendMrPart(mrPart);
                }
                else
                {
                    mrPart = new CMrPart(partInModel, partInDrawing);
                    CDimTools.GetInstance().InitMrPart(modelObjectInModel, mViewBase, mrPart);
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

            //移除主柱子;
            mMrPartList.Remove(mMainBeam);

            //清空零件标记管理器中的零件标记;
            CMrMarkManager.GetInstance().Clear();

            //构建柱标注的拓扑结构;
            CMrCylinderDoorFrontManager.GetInstance().BuildCylinderDoorFrontTopo(mMrPartList);
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
                DrawYMainRightDim();
                DrawYMainLeftDim();
                DrawXNormalAlonePartDim();
                DrawMainBeamBoltDim();
                DrawPartMark();
                DrawMrApronPlateMark();
                DrawMainBeamBoltMark();
#else
                try
                 {
                     DrawYMainRightDim();
                     DrawYMainLeftDim();
                     DrawXNormalAlonePartDim();
                     DrawMainBeamBoltDim();
                     DrawPartMark();
                     DrawMrApronPlateMark();
                     DrawMainBeamBoltMark(); 
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
        /// 绘制零件标记;
        /// </summary>
        private void DrawPartMark()
        {
            DrawingHandler drawingHandler = new DrawingHandler();
            TSD.UI.DrawingObjectSelector DS = drawingHandler.GetDrawingObjectSelector();

            foreach (CMrPart mrPart in mMrPartList)
            {
                CMrMark mrMark = mrPart.GetCylinderDoorFrontViewInfo().GetPartMark();

                if (null == mrMark)
                {
                    continue;
                }
                DS.SelectObject(mrPart.mPartInDrawing);
            }

            CDimTools.GetInstance().DrawMarkByMacro();
        }

        /// <summary>
        /// 绘制主梁右侧Y方向上的标注;
        /// </summary>
        private void DrawYMainRightDim()
        {
            List<Point> rightDimPointList = new List<Point>();

            foreach (CMrPart mrPart in mMrPartList)
            {
                CMrDimSet partDimSet = mrPart.GetCylinderDoorFrontViewInfo().GetYPartMainRightDimSet();

                if (partDimSet != null && partDimSet.Count > 0)
                {
                    rightDimPointList.AddRange(partDimSet.GetDimPointList());
                }
            }

            //1.得到主梁中间需要整体进行标注的标注集;
            CMrDimSet mrDimSet = GetYNormalMiddlePartDimSet();
            rightDimPointList.AddRange(mrDimSet.GetDimPointList());
          
            //2.得到顶部需要进行标注的零部件;
            CMrDimSet mrTopPartDimSet = GetTopPartDimSet(2);
            rightDimPointList.AddRange(mrTopPartDimSet.GetDimPointList());

            //3.得到主梁上螺钉组的标注集;
            CMrDimSet mrBoltDimSet = GetMainBeamYRightBoltDimSet();
            rightDimPointList.AddRange(mrBoltDimSet.GetDimPointList());

            Comparison<Point> sorterY = new Comparison<Point>(CDimTools.ComparePointY);
            rightDimPointList.Sort(sorterY);

            PointList pointList = new PointList();
            foreach (Point point in rightDimPointList)
            {
                pointList.Add(point);
            }

            Point minYPoint = rightDimPointList[0];
            double dimDistance = Math.Abs(CCommonPara.mViewMaxX - minYPoint.X) + 2 * CCommonPara.mDefaultDimDistance;
            Vector rightDimVector = new Vector(1, 0, 0);
            CDimTools.GetInstance().DrawDimensionSet(mViewBase, pointList, rightDimVector, dimDistance, CCommonPara.mSizeDimPath);

            //2.标注一个主尺寸;
            Point maxYPoint = rightDimPointList[rightDimPointList.Count - 1];
            pointList.Clear();
            pointList.Add(minYPoint);
            pointList.Add(maxYPoint);

            dimDistance = dimDistance + 2 * CCommonPara.mDefaultDimDistance;
            CDimTools.GetInstance().DrawDimensionSet(mViewBase, pointList, rightDimVector, dimDistance, CCommonPara.mMainSizeDimPath);
        }

        /// <summary>
        /// 获取主梁螺钉右侧的标注集;
        /// </summary>
        /// <returns></returns>
        private CMrDimSet GetMainBeamYRightBoltDimSet()
        {
            CMrDimSet mrDimSet = new CMrDimSet();

            foreach (CMrBoltArray mrBoltArray in mMainBeam.GetBoltArrayList())
            {
                if (!CDimTools.GetInstance().IsTwoVectorParallel(mrBoltArray.mNormal, new Vector(0, 0, 1)))
                {
                    continue;
                }

                List<Point> pointList = mrBoltArray.GetMaxXPointList();
                mrDimSet.AddRange(pointList);
            }

            return mrDimSet;
        }

        /// <summary>
        /// 绘制主梁左侧Y方向上的标注;
        /// </summary>
        private void DrawYMainLeftDim()
        {
            List<Point> leftDimPointList = new List<Point>();
            foreach (CMrPart mrPart in mMrPartList)
            {
                CMrDimSet partDimSet = mrPart.GetCylinderDoorFrontViewInfo().GetYPartMainLeftDimSet();

                if (partDimSet != null && partDimSet.Count > 0)
                {
                    leftDimPointList.AddRange(partDimSet.GetDimPointList());
                }
            }

            //1.得到顶部需要进行标注的零部件;
            CMrDimSet mrTopPartDimSet = GetTopPartDimSet(1);
            leftDimPointList.AddRange(mrTopPartDimSet.GetDimPointList());

            Comparison<Point> sorterY = new Comparison<Point>(CDimTools.ComparePointY);
            leftDimPointList.Sort(sorterY);

            PointList pointList = new PointList();
            foreach (Point point in leftDimPointList)
            {
                pointList.Add(point);
            }

            Point MinXPoint = leftDimPointList[0];
            double dimDistance = Math.Abs(CCommonPara.mViewMinX - MinXPoint.X) + 2 * CCommonPara.mDefaultDimDistance;
            Vector rightDimVector = new Vector(-1, 0, 0);
            CDimTools.GetInstance().DrawDimensionSet(mViewBase, pointList, rightDimVector, dimDistance, CCommonPara.mSizeDimPath);
        }

        /// <summary>
        /// 获取法向与Y轴平行的单独进行标注的零部件的标注集合;
        /// </summary>
        /// <returns></returns>
        private CMrDimSet GetYNormalMiddlePartDimSet()
        {
            CMrDimSet mrDimSet = new CMrDimSet();

            List<CMrPart> yNormalAlonePartList=CMrCylinderDoorFrontManager.GetInstance().mYNormalMiddlePartList;

            foreach (CMrPart mrPart in yNormalAlonePartList)
            {
                if (!IsYNormalMiddlePartNeedDim(mrPart))
                {
                    continue;
                }

                mrDimSet.AddPoint(mrPart.GetMaxXMinYPoint());
            }

            return mrDimSet;
        }

        /// <summary>
        /// 获取顶部零件的标注集;
        /// </summary>
        /// <param name="nFlag">1:左侧，2：右侧</param>
        /// <returns></returns>
        private CMrDimSet GetTopPartDimSet(int nFlag)
        {
            CMrDimSet mrDimSet = new CMrDimSet();

            CMrPart mrTopPart = CMrCylinderDoorFrontManager.GetInstance().mTopPart;
            CMrPart mrLeftTopPart = CMrCylinderDoorFrontManager.GetInstance().mLeftTopPart;
            CMrPart mrRightTopPart = CMrCylinderDoorFrontManager.GetInstance().mRightTopPart;

            //标注顶板右侧的情况;
            if (nFlag == 2)
            {
                //如果顶部零件为空,则把主梁最上面的点加入进去;
                if (mrTopPart == null)
                {
                    mrDimSet.AddPoint(mMainBeam.GetMaxYMaxXPoint());
                    return mrDimSet;
                }
             
                Vector normal = mrTopPart.mNormal;

                if (CDimTools.GetInstance().IsTwoVectorParallel(normal, new Vector(0, 1, 0)))
                {
                    mrDimSet.AddPoint(mrTopPart.GetMaxXMaxYPoint());
                    mrDimSet.AddPoint(mrTopPart.GetMaxXMinYPoint());
                }
                else
                {
                    if (mrLeftTopPart != null && mrRightTopPart != null)
                    {
                        if (mrLeftTopPart.GetMaxYPoint().Y > mrRightTopPart.GetMaxYPoint().Y)
                        {
                            mrDimSet.AddPoint(mrLeftTopPart.GetMaxXMaxYPoint());
                        }
                        else
                        {
                            mrDimSet.AddPoint(mrRightTopPart.GetMaxXMaxYPoint());
                        }
                    }
                    else if (mrLeftTopPart != null)
                    {
                        mrDimSet.AddPoint(mrLeftTopPart.GetMaxXMaxYPoint());
                    }
                    else if (mrRightTopPart != null)
                    {
                        mrDimSet.AddPoint(mrRightTopPart.GetMinXMaxYPoint());
                    }
                }
            }
            //标注顶板左侧的情况;
            else if (nFlag == 1)
            {
                if (mrTopPart == null)
                {
                    mrDimSet.AddPoint(mMainBeam.GetMaxYMinXPoint());
                    return mrDimSet;
                }

                Vector normal = mrTopPart.mNormal;
                
                if (CDimTools.GetInstance().IsTwoVectorParallel(normal, new Vector(0, 1, 0)))
                {
                    mrDimSet.AddPoint(mrTopPart.GetMinXMaxYPoint());
                    mrDimSet.AddPoint(mrTopPart.GetMinXMinYPoint());
                }
                else
                {
                    if (mrLeftTopPart != null && mrRightTopPart != null)
                    {
                        if (mrLeftTopPart.GetMaxYPoint().Y > mrRightTopPart.GetMaxYPoint().Y)
                        {
                            mrDimSet.AddPoint(mrLeftTopPart.GetMaxXMaxYPoint());
                        }
                        else
                        {
                            mrDimSet.AddPoint(mrRightTopPart.GetMinXMaxYPoint());
                        }
                    }
                    else if (mrLeftTopPart != null)
                    {
                        mrDimSet.AddPoint(mrLeftTopPart.GetMaxXMaxYPoint());
                    }
                    else if (mrRightTopPart != null)
                    {
                        mrDimSet.AddPoint(mrRightTopPart.GetMinXMaxYPoint());
                    }
                }
            }

            return mrDimSet;
        }

        /// <summary>
        /// 判断法向与Y轴平行的主梁中间进行标注的零部件是否需要进行标注;
        /// </summary>
        /// <returns></returns>
        private bool IsYNormalMiddlePartNeedDim(CMrPart mrPart)
        {
            List<CMrPart> rightPartList = CMrCylinderDoorFrontManager.GetInstance().mRightDimPartList;

            int minY = (int)mrPart.GetMaxXMinYPoint().Y;
            int maxY = (int)mrPart.GetMaxXMaxYPoint().Y;

            string strY = minY.ToString() + "," + maxY.ToString();

            foreach(CMrPart mrHaveDimPart in rightPartList)
            {
                minY = (int)mrHaveDimPart.GetMaxXMinYPoint().Y;
                maxY = (int)mrHaveDimPart.GetMaxXMaxYPoint().Y;
                
                string strNewY = minY.ToString() + "," + maxY.ToString();

                if(strY.Equals(strNewY))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 绘制主梁外侧法向与X轴平行的零件标注;
        /// </summary>
        private void DrawXNormalAlonePartDim()
        {
            List<CMrPart> xNormalAlonePartList = CMrCylinderDoorFrontManager.GetInstance().mXNormalAloneDimPartList;

            CMrPart leftPart = CMrCylinderDoorFrontManager.GetInstance().mLeftPart;
            CMrPart leftTopPart = CMrCylinderDoorFrontManager.GetInstance().mLeftTopPart;
            CMrPart leftTopMiddlePart = CMrCylinderDoorFrontManager.GetInstance().mLeftTopMiddlePart;

            CMrPart rightPart = CMrCylinderDoorFrontManager.GetInstance().mRightPart;
            CMrPart rightTopPart = CMrCylinderDoorFrontManager.GetInstance().mRightTopPart;
            CMrPart rightTopMiddlePart = CMrCylinderDoorFrontManager.GetInstance().mRightTopMiddlePart;

            double leftPartMinX = leftPart.GetMinXPoint().X;
            double rightPartMaxX = rightPart.GetMaxXPoint().X;

            foreach(CMrPart mrPart in xNormalAlonePartList)
            {
                double partMinX = mrPart.GetMinXPoint().X;
                double partMaxX = mrPart.GetMaxXPoint().X;

                if (partMinX < leftPartMinX)
                {
                    PointList pointList = new PointList();
                    Point minXmaxYPoint = mrPart.GetMinXMaxYPoint();
                    pointList.Add(minXmaxYPoint);
                   
                    double dimDistance = CCommonPara.mDefaultDimDistance;
                    Vector dimVector = new Vector(0, 1, 0);

                    if (leftTopPart != null && leftTopPart.GetMinXMinYPoint().Y < minXmaxYPoint.Y)
                    {
                        pointList.Add(new Point(leftTopPart.GetMinXPoint().X, minXmaxYPoint.Y, 0));
                    }
                    else if (leftTopMiddlePart != null && leftTopMiddlePart.GetMinXMinYPoint().Y < minXmaxYPoint.Y)
                    {
                        pointList.Add(new Point(leftTopMiddlePart.GetMinXPoint().X, minXmaxYPoint.Y, 0));
                    }
                    else
                    {
                        pointList.Add(new Point(leftPartMinX, minXmaxYPoint.Y, 0));
                    }
                    CDimTools.GetInstance().DrawDimensionSet(mViewBase, pointList, dimVector, dimDistance, CCommonPara.mSizeDimPath);
                }
                else if(partMaxX > rightPartMaxX)
                {
                    PointList pointList = new PointList();
                    Point maxXmaxYPoint = mrPart.GetMaxXMaxYPoint();
                    pointList.Add(maxXmaxYPoint);
                    double dimDistance = CCommonPara.mDefaultDimDistance;
                    Vector dimVector = new Vector(0, 1, 0);

                    if(rightTopPart != null && rightTopPart.GetMaxXMinYPoint().Y < maxXmaxYPoint.Y)
                    {
                        pointList.Add(new Point(rightTopPart.GetMaxXPoint().X, maxXmaxYPoint.Y, 0));
                    }
                    else if (rightTopMiddlePart != null && rightTopMiddlePart.GetMaxXMinYPoint().Y < maxXmaxYPoint.Y)
                    {
                        pointList.Add(new Point(rightTopMiddlePart.GetMaxXPoint().X, maxXmaxYPoint.Y, 0));
                    }
                    else
                    {
                        pointList.Add(new Point(rightPartMaxX, maxXmaxYPoint.Y, 0));
                    }
                 
                    CDimTools.GetInstance().DrawDimensionSet(mViewBase, pointList, dimVector, dimDistance, CCommonPara.mSizeDimPath);
                }
            }
        }

        /// <summary>
        /// 绘制主梁上螺钉的标注，需要判断螺钉与主梁顶部的距离，如果距离超过一定的阈值只进行单独标注;
        /// </summary>
        private void DrawMainBeamBoltDim()
        {
            CMrPart mrLeftPart = CMrCylinderDoorFrontManager.GetInstance().mLeftPart;
            CMrPart mrRightPart = CMrCylinderDoorFrontManager.GetInstance().mRightPart;

            List<CMrBoltArray> mrBoltArrayList = mMainBeam.GetBoltArrayList();

            foreach(CMrBoltArray mrBoltArray in mrBoltArrayList)
            {
                Vector normal = mrBoltArray.mNormal;

                if(!CDimTools.GetInstance().IsTwoVectorParallel(normal,new Vector(0,0,1)))
                {
                    continue;
                }

                double distance = Math.Abs(mrBoltArray.GetMaxYPoint().Y - mMainBeam.GetMaxYPoint().Y);

                if (distance < 200)
                {
                    //1.标注螺钉组的上侧标注;
                    PointList pointList = new PointList();

                    foreach (Point point in mrBoltArray.GetMaxYPointList())
                    {
                        pointList.Add(point);
                    }
                    pointList.Add(mrRightPart.GetMaxXMaxYPoint());
                    pointList.Add(mrLeftPart.GetMinXMaxYPoint());

                    double dimDistance = Math.Abs(mMainBeam.GetMaxYPoint().Y - pointList[0].Y) + CCommonPara.mDefaultDimDistance;
                    Vector dimVector = new Vector(0, 1, 0);
                    CDimTools.GetInstance().DrawDimensionSet(mViewBase, pointList, dimVector, dimDistance, CCommonPara.mSizeDimPath);
                }
                else
                {
                    //1.标注螺钉组的上侧标注;
                    PointList pointList = new PointList();

                    foreach (Point point in mrBoltArray.GetMaxYPointList())
                    {
                        pointList.Add(point);
                    }

                    Point boltMaxYPt=mrBoltArray.GetMaxYPoint();

                    Vector leftPartNormal = mrLeftPart.mNormal;
                    Vector rightPartNormal = mrRightPart.mNormal;

                    if (CDimTools.GetInstance().IsTwoVectorParallel(leftPartNormal, new Vector(1, 0, 0)))
                    {
                        pointList.Add(new Point(mrLeftPart.GetMinXPoint().X, boltMaxYPt.Y, 0));
                    }
                    if (CDimTools.GetInstance().IsTwoVectorParallel(rightPartNormal, new Vector(1, 0, 0)))
                    {
                        pointList.Add(new Point(mrRightPart.GetMaxXPoint().X, boltMaxYPt.Y, 0));
                    }
                    if (pointList.Count >= 2)
                    {
                        double dimDistance = CCommonPara.mDefaultDimDistance;
                        Vector dimVector = new Vector(0, 1, 0);
                        CDimTools.GetInstance().DrawDimensionSet(mViewBase, pointList, dimVector, dimDistance, CCommonPara.mSizeDimPath);
                    }
                }           
            }
        }

        /// <summary>
        /// 绘制檩托板的零件标记;
        /// </summary>
        private void DrawMrApronPlateMark()
        {
            DrawingHandler drawingHandler = new DrawingHandler();
            TSD.UI.DrawingObjectSelector DS = drawingHandler.GetDrawingObjectSelector();

            Dictionary<CMrPart, CMrApronPlate> mapPartToMrApronPlate = CMrCylinderDoorFrontManager.GetInstance().mMapYNormalPartToMrApronPlate;

            foreach (CMrApronPlate mrApronPlate in mapPartToMrApronPlate.Values)
            {
                CMrPart yNormalPart = mrApronPlate.mYNormalPart;
                CMrPart zNormalPart = mrApronPlate.mZNormalPart;

                DS.SelectObject(yNormalPart.mPartInDrawing);
                DS.SelectObject(zNormalPart.mPartInDrawing);
            }
            CDimTools.GetInstance().DrawMarkByMacro();
        }

        /// <summary>
        /// 绘制主梁上面的螺钉标记;
        /// </summary>
        private void DrawMainBeamBoltMark()
        {
            DrawingHandler drawingHandler = new DrawingHandler();
            TSD.UI.DrawingObjectSelector DS = drawingHandler.GetDrawingObjectSelector();

            List<CMrBoltArray> mrBoltArrayList = mMainBeam.GetBoltArrayList();

            foreach (CMrBoltArray mrBoltArray in mrBoltArrayList)
            {
                if (!CDimTools.GetInstance().IsTwoVectorParallel(mrBoltArray.mNormal, new Vector(0, 0, 1)))
                {
                    continue;
                }
                DS.SelectObject(mrBoltArray.mBoltInDrawing);
            }

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
