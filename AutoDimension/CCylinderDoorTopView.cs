using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tekla.Structures.Model;
using Tekla.Structures.Drawing;
using Tekla.Structures;

using TSD = Tekla.Structures.Drawing;
using TSM = Tekla.Structures.Model;
using AutoDimension.Entity;
using System.Threading;
using System.Windows.Forms;
using Tekla.Structures.Geometry3d;

namespace AutoDimension
{
    /// <summary>
    /// 门式框架结构中的顶视图对象;
    /// </summary>
    public class CCylinderDoorTopView:CView
    {
        /// <summary>
        /// Y值最大的零部件;
        /// </summary>
        private CMrPart mMaxYPart = null;

        /// <summary>
        /// Y值最小的零部件;
        /// </summary>
        private CMrPart mMinYPart = null;


        /// <summary>
        /// 门式框架结构顶视图对象;
        /// </summary>
        /// <param name="viewBase"></param>
        /// <param name="model"></param>
        public CCylinderDoorTopView(TSD.View viewBase, Model model)
            : base(viewBase)
        {
            mViewBase = viewBase;
            mModel = model;
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

                UpdateMinAndMaxYPart(mrPart);
            }

            //移除主柱子;
            mMrPartList.Remove(mMainBeam);

            //清空零件标记管理器中的零件标记;
            CMrMarkManager.GetInstance().Clear();

            //构建门式框架结构柱标注顶视图的拓扑;
            CMrCylinderDoorTopManager.GetInstance().BuildCylinderDoorTopTopo(mMrPartList);
        }


        /// <summary>
        /// 更新视图中Y值最大和最小的零部件;
        /// </summary>
        /// <param name="mrPart"></param>
        private void UpdateMinAndMaxYPart(CMrPart mrPart)
        {
            double minY = mrPart.GetMinYPoint().Y;
            double maxY = mrPart.GetMaxYPoint().Y;

            if (mMinYPart == null)
            {
                mMinYPart = mrPart;

                return;
            }
            if (mMaxYPart == null)
            {
                mMaxYPart = mrPart;

                return;
            }
            if (minY < mMinYPart.GetMinYPoint().Y)
            {
                mMinYPart = mrPart;
            }
            if (maxY > mMaxYPart.GetMaxYPoint().Y)
            {
                mMaxYPart = mrPart;
            }
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
                DrawZNormalMiddleBoltDim();
                DrawYMainRightDim();
                DrawYMainLeftDim();
                DrawBoltDim();
                DrawPartMark();
                DrawMrApronPlateMark();
                DrawZNormalMiddleBoltMark();
#else
                try
                {
                    DrawZNormalMiddleBoltDim();
                    DrawYMainRightDim();
                    DrawYMainLeftDim();
                    DrawBoltDim();
                    DrawPartMark();
                    DrawMrApronPlateMark();
                    DrawZNormalMiddleBoltMark();
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
        public void DrawPartMark()
        {
            DrawingHandler drawingHandler = new DrawingHandler();
            TSD.UI.DrawingObjectSelector DS = drawingHandler.GetDrawingObjectSelector();

            foreach (CMrPart mrPart in mMrPartList)
            {
                CMrMark mrMark = mrPart.GetCylinderDoorTopViewInfo().GetPartMark();

                if (null == mrMark)
                {
                    continue;
                }

                DS.SelectObject(mrPart.mPartInDrawing);
            }
            CDimTools.GetInstance().DrawMarkByMacro();
        }

        /// <summary>
        /// 绘制法向与Z轴平行的中间零部件上螺钉的标记;
        /// </summary>
        public void DrawZNormalMiddleBoltMark()
        {
            DrawingHandler drawingHandler = new DrawingHandler();
            TSD.UI.DrawingObjectSelector DS = drawingHandler.GetDrawingObjectSelector();

            foreach (CMrPart mrPart in mMrPartList)
            {
                Vector normal = mrPart.mNormal;

                double minX = mrPart.GetMinXPoint().X;
                double maxX = mrPart.GetMaxXPoint().X;

                if (!CDimTools.GetInstance().IsTwoVectorParallel(normal, new Vector(0, 0, 1)) || !mrPart.IsHaveBolt())
                {
                    continue;
                }
                if (CDimTools.GetInstance().CompareTwoDoubleValue(minX, 0) >= 0 ||
                    CDimTools.GetInstance().CompareTwoDoubleValue(maxX, 0) <= 0)
                {
                    continue;
                }
                List<CMrBoltArray> mrBoltArrayList = mrPart.GetBoltArrayList();

                foreach (CMrBoltArray mrBoltArray in mrBoltArrayList)
                {
                    if (!CDimTools.GetInstance().IsTwoVectorParallel(mrBoltArray.mNormal, new Vector(0, 0, 1)))
                    {
                        continue;
                    }
                    DS.SelectObject(mrBoltArray.mBoltInDrawing);
                }
            }
            CDimTools.GetInstance().DrawMarkByMacro();
        }

        /// <summary>
        /// 绘制法向与Z轴平行的中间零部件上的螺钉标记; 
        /// </summary>
        public void DrawZNormalMiddleBoltDim()
        {
            foreach (CMrPart mrPart in mMrPartList)
            {
                List<CMrDimSet> mrDimSetList = mrPart.GetCylinderDoorTopViewInfo().GetZNormalMiddleDimSet();

                if (null == mrDimSetList || mrDimSetList.Count == 0)
                {
                    continue;
                }
                foreach (CMrDimSet mrDimSet in mrDimSetList)
                {
                    int nCount=mrDimSet.GetDimPointList().Count;

                    if(nCount==0||nCount==1)
                    {
                        continue;
                    }

                    PointList pointList=new PointList();

                    foreach (Point point in mrDimSet.GetDimPointList())
                    {
                        pointList.Add(point);
                    }
                    CDimTools.GetInstance().DrawDimensionSet(mViewBase, pointList, mrDimSet.mDimVector, mrDimSet.mDimDistance, CCommonPara.mSizeDimPath);
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

            Dictionary<CMrPart, CMrApronPlate> mapPartToMrApronPlate = CMrCylinderDoorTopManager.GetInstance().mMapYNormalPartToMrApronPlate;

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
        /// 绘制主梁右侧的标注;
        /// </summary>
        public void DrawYMainRightDim()
        {
            List<Point> rightDimPointList = new List<Point>();

            foreach (CMrPart mrPart in mMrPartList)
            {
                CMrDimSet partDimSet = mrPart.GetCylinderDoorTopViewInfo().GetYRightPartDim();
                CMrDimSet boltDimSet = mrPart.GetCylinderDoorTopViewInfo().GetYRightBoltDim();

                if (partDimSet != null && partDimSet.Count > 0)
                {
                    rightDimPointList.AddRange(partDimSet.GetDimPointList());
                }
                if (boltDimSet != null && boltDimSet.Count > 0)
                {
                    rightDimPointList.AddRange(boltDimSet.GetDimPointList());
                }
            }

            //把最大Y值和最小Y值的点加进来;
            if (mMaxYPart != null)
            {
                rightDimPointList.Add(mMaxYPart.GetMaxYMaxXPoint());
            }
            if (mMinYPart != null)
            {
                rightDimPointList.Add(mMinYPart.GetMinYMaxXPoint());
            }

            //把檩托板的点加进来;
            List<CMrDimSet> mrDimSetList = GetMrApronPlateDimSet(2);

            foreach (CMrDimSet mrDimSet in mrDimSetList)
            {
                if (mrDimSet != null)
                {
                    rightDimPointList.AddRange(mrDimSet.GetDimPointList());
                }
            }
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
        }

        /// <summary>
        /// 获取左侧檩托板的标注集;
        /// </summary>
        /// <param name="nFlag">1:左侧;2:右侧</param>
        /// <returns></returns>
        private List<CMrDimSet> GetMrApronPlateDimSet(int nFlag)
        {
            List<CMrDimSet> mrDimSetList=new List<CMrDimSet>();

            Dictionary<CMrPart,CMrApronPlate> mMapPartToApronPlate = CMrCylinderDoorTopManager.GetInstance().mMapYNormalPartToMrApronPlate;

            //如果是右侧;
            if (nFlag == 2)
            {
                foreach (CMrApronPlate mrApronPlate in mMapPartToApronPlate.Values)
                {
                    CMrPart yNormalPart = mrApronPlate.mYNormalPart;
                    double minX = yNormalPart.GetMinXPoint().X;
                    if (minX < 0)
                    {
                        continue;
                    }
                    CMrDimSet mrDimSet = new CMrDimSet();
                    bool bIsUp = mrApronPlate.mIsUp;

                    if (bIsUp)
                    {
                        mrDimSet.AddPoint(yNormalPart.GetMaxXMinYPoint());
                    }
                    else
                    {
                        mrDimSet.AddPoint(yNormalPart.GetMaxXMaxYPoint());
                    }
                    mrDimSetList.Add(mrDimSet);
                }
            }
            //如果是左侧;
            else if (nFlag == 1)
            {
                foreach (CMrApronPlate mrApronPlate in mMapPartToApronPlate.Values)
                {
                    CMrPart yNormalPart = mrApronPlate.mYNormalPart;
                    double maxX = yNormalPart.GetMaxXPoint().X;
                    if (maxX > 0)
                    {
                        continue;
                    }
                    CMrDimSet mrDimSet = new CMrDimSet();
                    bool bIsUp = mrApronPlate.mIsUp;

                    if (bIsUp)
                    {
                        mrDimSet.AddPoint(yNormalPart.GetMinXMinYPoint());
                    }
                    else
                    {
                        mrDimSet.AddPoint(yNormalPart.GetMinXMaxYPoint());
                    }
                    mrDimSetList.Add(mrDimSet);
                }
            }
         
            return mrDimSetList;
        }

        /// <summary>
        /// 绘制主梁左侧的标注;
        /// </summary>
        public void DrawYMainLeftDim()
        {
            List<Point> leftDimPointList = new List<Point>();

            foreach (CMrPart mrPart in mMrPartList)
            {
                CMrDimSet partDimSet = mrPart.GetCylinderDoorTopViewInfo().GetYLeftPartDim();
                CMrDimSet boltDimSet = mrPart.GetCylinderDoorTopViewInfo().GetYLeftBoltDim();

                if (partDimSet != null && partDimSet.Count > 0)
                {
                    leftDimPointList.AddRange(partDimSet.GetDimPointList());
                }
                if (boltDimSet != null && boltDimSet.Count > 0)
                {
                    leftDimPointList.AddRange(boltDimSet.GetDimPointList());
                }
            }

            //把最大Y值和最小Y值的点加进来;
            if (mMaxYPart != null)
            {
                leftDimPointList.Add(mMaxYPart.GetMaxYMinXPoint());
            }
            if (mMinYPart != null)
            {
                leftDimPointList.Add(mMinYPart.GetMinYMinXPoint());
            }

            //把檩托板的点加进来;
            List<CMrDimSet> mrDimSetList = GetMrApronPlateDimSet(1);

            foreach (CMrDimSet mrDimSet in mrDimSetList)
            {
                if (mrDimSet != null)
                {
                    leftDimPointList.AddRange(mrDimSet.GetDimPointList());
                }
            }
            Comparison<Point> sorterY = new Comparison<Point>(CDimTools.ComparePointY);
            leftDimPointList.Sort(sorterY);

            PointList pointList = new PointList();
            foreach (Point point in leftDimPointList)
            {
                pointList.Add(point);
            }

            Point minYPoint = leftDimPointList[0];
            double dimDistance = Math.Abs(CCommonPara.mViewMinX - minYPoint.X) + 2 * CCommonPara.mDefaultDimDistance;
            Vector rightDimVector = new Vector(-1, 0, 0);
            CDimTools.GetInstance().DrawDimensionSet(mViewBase, pointList, rightDimVector, dimDistance, CCommonPara.mSizeDimPath);
        }

        /// <summary>
        /// 绘制螺钉组的标记;
        /// </summary>
        public void DrawBoltDim()
        {
            foreach (CMrPart mrPart in mMrPartList)
            {
                if (mrPart == mMainBeam)
                {
                    continue;
                }

                List<CMrDimSet> mrDimSetList = mrPart.GetCylinderDoorTopViewInfo().GetBoltDimSetList();

                foreach (CMrDimSet mrDimSet in mrDimSetList)
                {
                    if (mrDimSet == null || mrDimSet.Count == 0)
                    {
                        continue;
                    }

                    PointList pointList = new PointList();

                    foreach (Point point in mrDimSet.GetDimPointList())
                    {
                        pointList.Add(point);
                    }

                    CDimTools.GetInstance().DrawDimensionSet(mViewBase, pointList,mrDimSet.mDimVector, mrDimSet.mDimDistance, CCommonPara.mSizeDimPath);
                }
            }
        }
    }
}
