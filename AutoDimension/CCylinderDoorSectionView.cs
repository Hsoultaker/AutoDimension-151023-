using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tekla.Structures.Model;
using Tekla.Structures.Geometry3d;
using Tekla.Structures;
using AutoDimension.Entity;
using System.Threading;

using TSD = Tekla.Structures.Drawing;
using TSM = Tekla.Structures.Model;
using System.Windows.Forms;
using Tekla.Structures.Drawing;


namespace AutoDimension
{
    /// <summary>
    /// 门式框架结构的剖视图;
    /// </summary>
    public class CCylinderDoorSectionView : CView
    {
        /// <summary>
        /// 主梁法向与Y轴平行时，左侧的零部件;
        /// </summary>
        private CMrPart mLeftPart;

        /// <summary>
        /// 主梁法向与Y轴平行时，右侧的零部件;
        /// </summary>
        private CMrPart mRightPart;

        /// <summary>
        /// 向上标注的螺钉映射表;
        /// </summary>
        private Dictionary<CMrBoltArray, bool> mDicBoltArrayToUpDim = new Dictionary<CMrBoltArray, bool>();

        /// <summary>
        /// 右侧标注的螺钉映射表;
        /// </summary>
        private Dictionary<CMrBoltArray, bool> mDicBoltArrayToRightDim = new Dictionary<CMrBoltArray, bool>();

        /// <summary>
        /// 构造函数;
        /// </summary>
        /// <param name="viewBase"></param>
        /// <param name="model"></param>
        public CCylinderDoorSectionView(TSD.View viewBase, Model model)
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

            CDimTools dimTools = CDimTools.GetInstance();

            Point viewMinPoint = mViewBase.RestrictionBox.MinPoint;
            Point viewMaxPoint = mViewBase.RestrictionBox.MaxPoint;

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

                Point minXPoint = mrPart.GetMinXPoint();
                Point maxXPoint = mrPart.GetMaxXPoint();

                if (minXPoint.X > viewMaxPoint.X || maxXPoint.X < viewMinPoint.X)
                {
                    continue;
                }

                Point minYPoint = mrPart.GetMinYPoint();
                Point maxYPoint = mrPart.GetMaxYPoint();

                if (minYPoint.Y > viewMaxPoint.Y || maxYPoint.Y < viewMinPoint.Y)
                {
                    continue;
                }

                Point minZPoint = mrPart.GetMinZPoint();
                Point maxZPoint = mrPart.GetMaxZPoint();

                if (minZPoint.Z > viewMaxPoint.Z || maxZPoint.Z < viewMinPoint.Z)
                {
                    continue;
                }

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
                CDimTools.GetInstance().UpdateViewBox(mrPart);
            }

            CMrMarkManager.GetInstance().Clear();
        }

        /// <summary>
        /// 初始化以及标注线程函数;
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
                //把主梁从链表中移除;
                mMrPartList.Remove(mMainBeam);
                DrawSectionDim();
                DrawPartMark();
#else
                try
                {
                    //把主梁从链表中移除;
                    mMrPartList.Remove(mMainBeam);
                    DrawSectionDim();
                    DrawPartMark();
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
        /// 绘制零件标记;
        /// </summary>
        private void DrawPartMark()
        {
            Vector normal = mMainBeam.mNormal;

            if(CDimTools.GetInstance().IsTwoVectorParallel(normal,new Vector(0,1,0)))
            {
                DrawYVectorPartMark();
            }
            else if (CDimTools.GetInstance().IsTwoVectorParallel(normal, new Vector(1, 0, 0)))
            {
                DrawXVectorPartMark();
            }
        }

        /// <summary>
        /// 绘制主梁法向朝X方向的零件标记;
        /// </summary>
        private void DrawXVectorPartMark()
        {
            DrawingHandler drawingHandler = new DrawingHandler();
            TSD.UI.DrawingObjectSelector DS = drawingHandler.GetDrawingObjectSelector();

            foreach (CMrPart mrPart in mMrPartList)
            {
                Vector vector = mrPart.mNormal;

                if (!CDimTools.GetInstance().IsTwoVectorParallel(vector, new Vector(0, 0, 1)))
                {
                    continue;
                }
                if (!mrPart.IsHaveBolt())
                {
                    continue;
                }

                DS.SelectObject(mrPart.mPartInDrawing);
                
                //2.绘制螺钉组标记;
                List<CMrBoltArray> mrBoltArrayList = mrPart.GetBoltArrayList();

                foreach (CMrBoltArray mrBoltArray in mrBoltArrayList)
                {
                    if (CDimTools.GetInstance().IsTwoVectorParallel(mrBoltArray.mNormal, new Vector(0, 0, 1)))
                    {
                        DS.SelectObject(mrBoltArray.mBoltInDrawing);
                    }
                }
            }

            CDimTools.GetInstance().DrawMarkByMacro();
        }

        /// <summary>
        /// 绘制主梁法向朝Y方向的零件标记;
        /// </summary>
        private void DrawYVectorPartMark()
        {
            BuildLeftAndRightPart();
            
            DrawingHandler drawingHandler = new DrawingHandler();
            TSD.UI.DrawingObjectSelector DS = drawingHandler.GetDrawingObjectSelector();

            foreach (CMrPart mrPart in mMrPartList)
            {
                Vector normal = mrPart.mNormal;
                
                //如果零件的法向量与Z轴平行;
                if (CDimTools.GetInstance().IsTwoVectorParallel(normal, new Vector(0, 0, 1)))
                {
                    DS.SelectObject(mrPart.mPartInDrawing);

                    List<CMrBoltArray> mrBoltArrayList = mrPart.GetBoltArrayList();

                    foreach (CMrBoltArray mrBoltArray in mrBoltArrayList)
                    {
                        if (CDimTools.GetInstance().IsTwoVectorParallel(mrBoltArray.mNormal, new Vector(0, 0, 1)))
                        {
                            DS.SelectObject(mrBoltArray.mBoltInDrawing);
                        }
                    }
                }
                else if (CDimTools.GetInstance().IsTwoVectorParallel(normal, new Vector(1, 0, 0)))
                {
                    DS.SelectObject(mrPart.mPartInDrawing);
                }
            }

            CDimTools.GetInstance().DrawMarkByMacro();
        }

        /// <summary>
        /// 绘制剖面的标注;
        /// </summary>
        private void DrawSectionDim()
        {
            Vector normal = mMainBeam.mNormal;

            //如果主梁的法向与Y轴平行;
            if (CDimTools.GetInstance().IsTwoVectorParallel(normal, new Vector(0, 1, 0)))
            {
                DrawYVectorSectionDim();
            }
            //如果主梁的法向与X轴平行;
            else if (CDimTools.GetInstance().IsTwoVectorParallel(normal, new Vector(1, 0, 0)))
            {
                DrawXVectorSectionDim();
            }
        }

        /// <summary>
        /// 绘制主梁法向朝X方向的剖面标注;
        /// </summary>
        private void DrawXVectorSectionDim()
        {
            List<Point> upPointList = new List<Point>();
            List<Point> rightPointList = new List<Point>();

            foreach (CMrPart mrPart in mMrPartList)
            {
                Vector normal = mrPart.mNormal;

                if (!CDimTools.GetInstance().IsTwoVectorParallel(normal, new Vector(0, 0, 1)) || !mrPart.IsHaveBolt())
                {
                    continue;
                }

                List<CMrBoltArray> mrBoltArrayList = mrPart.GetBoltArrayList();

                foreach (CMrBoltArray mrBoltArray in mrBoltArrayList)
                {
                    if (!CDimTools.GetInstance().IsTwoVectorParallel(normal, new Vector(0, 0, 1)))
                    {
                        continue;
                    }

                    List<Point> maxYPointList = mrBoltArray.GetMaxYPointList();
                    upPointList.AddRange(maxYPointList);

                    List<Point> maxXPointList = mrBoltArray.GetMaxXPointList();
                    rightPointList.AddRange(maxXPointList);
                }

                upPointList.Add(mrPart.GetMaxYMinXPoint());
                upPointList.Add(mrPart.GetMaxYMaxXPoint());

                rightPointList.Add(mrPart.GetMaxYMaxXPoint());
                rightPointList.Add(mrPart.GetMinYMaxXPoint());
            }

            //顶部标注;
            PointList upDimPointList = new PointList();
            Vector dimVector = new Vector(0, 1, 0);
            double dimDistance = Math.Abs(CCommonPara.mViewMaxY - upPointList[0].Y) + 0.6 * CCommonPara.mDefaultDimDistance;

            foreach(Point pt in upPointList)
            {
                upDimPointList.Add(pt);
            }
            CDimTools.GetInstance().DrawDimensionSet(mViewBase, upDimPointList, dimVector, dimDistance, CCommonPara.mSizeDimPath);

            //右侧标注;
            PointList rightDimPointList = new PointList();
            dimVector = new Vector(1, 0, 0);
            dimDistance = Math.Abs(CCommonPara.mViewMaxX - upPointList[0].X) + 0.6 * CCommonPara.mDefaultDimDistance;

            foreach (Point pt in rightPointList)
            {
                rightDimPointList.Add(pt);
            }

            CDimTools.GetInstance().DrawDimensionSet(mViewBase, rightDimPointList, dimVector, dimDistance, CCommonPara.mSizeDimPath);
        }

        /// <summary>
        /// 绘制主梁法向朝Y方向的剖面标注;
        /// </summary>
        private void DrawYVectorSectionDim()
        {
            mLeftPart = null;
            mRightPart = null;
            
            mDicBoltArrayToRightDim.Clear();
            mDicBoltArrayToUpDim.Clear();

            //构建左右零件拓扑;
            BuildLeftAndRightPart();

            //移除左右两边零件;
            mMrPartList.Remove(mLeftPart);
            mMrPartList.Remove(mRightPart);

            //如果是底板的剖面标注;
            if (IsHaveBottomPlatePart())
            {
                DrawYVectorUpDim();
                DrawYVectorRightDim();
            }
            else
            {
                DrawYVectorUpDim();
                DrawYVectorRightDim();
                DrawYVectorLeftDim();
                DrawYVectorDownDim();
            }

            mMrPartList.Add(mLeftPart);
            mMrPartList.Add(mRightPart);
        }

        /// <summary>
        /// 绘制上侧的零部件标注;
        /// </summary>
        private void DrawYVectorUpDim()
        {
            List<Point> pointList = new List<Point>();

            bool bNeedAddLeftPart = true;
            bool bNeedAddRightPart = true;

            foreach (CMrPart mrPart in mMrPartList)
            {
                double minY = mrPart.GetMinYPoint().Y;
                double maxY = mrPart.GetMaxYPoint().Y;

                if (minY < 0 && maxY < 0)
                {
                    continue;
                }

                Vector normal = mrPart.mNormal;

                //1.先判断螺钉的标注;
                List<CMrBoltArray> mrBoltArrayList = mrPart.GetBoltArrayList();

                foreach (CMrBoltArray mrBoltArray in mrBoltArrayList)
                {
                    if (!CDimTools.GetInstance().IsTwoVectorParallel(mrBoltArray.mNormal, new Vector(0, 0, 01)))
                    {
                        continue;
                    }
                    List<Point> maxYPointList = mrBoltArray.GetMaxYPointList();
                    pointList.AddRange(maxYPointList);

                    //把该螺钉组添加到映射表中;
                    mDicBoltArrayToUpDim.Add(mrBoltArray, true);
                }
                //2.如果法向与Y轴平行;
                if (CDimTools.GetInstance().IsTwoVectorParallel(normal, new Vector(1, 0, 0)))
                {
                    if (mrPart.GetMaxXPoint().X < mLeftPart.GetMaxXPoint().X || mrPart.GetMinXPoint().X > mRightPart.GetMinXPoint().X)
                    {
                        continue;
                    }

                    if (!IsLeftPartNeedDim(mrPart))
                    {
                        bNeedAddLeftPart = false;
                        pointList.Add(mrPart.GetMaxYMaxXPoint());
                    }
                    else if (!IsRightPartNeedDim(mrPart))
                    {
                        bNeedAddRightPart = false;
                        pointList.Add(mrPart.GetMaxYMinXPoint());
                    }
                    else
                    {
                        pointList.Add(mrPart.GetMaxYMinXPoint());
                    }
                }
                if (IsMidPlate(mrPart))
                {
                    pointList.Add(mrPart.GetMaxYMinXPoint());
                    pointList.Add(mrPart.GetMaxYMaxXPoint());
                }
            }
            if (pointList.Count == 0)
            {
                return;
            }

            if (mLeftPart != null && bNeedAddLeftPart)
            {
                pointList.Add(mLeftPart.GetMaxYMinXPoint());
            }
            if (mRightPart != null && bNeedAddRightPart)
            {
                pointList.Add(mRightPart.GetMaxYMaxXPoint());
            }

            Comparison<Point> sorterX = new Comparison<Point>(CDimTools.ComparePointX);
            pointList.Sort(sorterX);

            PointList dimPointList = new PointList();

            foreach (Point pt in pointList)
            {
                dimPointList.Add(pt);
            }

            Vector dimVector = new Vector(0, 1, 0);
            double dimDistance = Math.Abs(CCommonPara.mViewMaxY - pointList[0].Y) + 0.6 * CCommonPara.mDefaultDimDistance;
            CDimTools.GetInstance().DrawDimensionSet(mViewBase, dimPointList, dimVector, dimDistance, CCommonPara.mSizeDimPath);
        }

        /// <summary>
        /// 绘制右侧零部件的标注;
        /// </summary>
        private void DrawYVectorRightDim()
        {
            List<Point> pointList = new List<Point>();

            foreach (CMrPart mrPart in mMrPartList)
            {
                if (mrPart.GetMaxXPoint().X < mLeftPart.GetMaxXPoint().X)
                {
                    continue;
                }
                if (!mrPart.IsHaveBolt())
                {
                    continue;
                }
                
                bool bFlag = false;

                List<CMrBoltArray> mrBoltArrayList = mrPart.GetBoltArrayList();

                foreach (CMrBoltArray mrBoltArray in mrBoltArrayList)
                {
                    if (!CDimTools.GetInstance().IsTwoVectorParallel(mrBoltArray.mNormal, new Vector(0, 0, 1)))
                    {
                        continue;
                    }

                    List<Point> maxXPointList = mrBoltArray.GetMaxXPointList();
                    pointList.AddRange(maxXPointList);

                    //把该螺钉组添加到映射表中;
                    mDicBoltArrayToRightDim.Add(mrBoltArray, true);

                    bFlag = true;
                }
                if (bFlag)
                {
                    pointList.Add(new Point(mrPart.GetMaxXPoint().X, 0, 0));
                }
                if (IsMidPlate(mrPart))
                {
                    pointList.Add(mrPart.GetMaxYMaxXPoint());
                    pointList.Add(mrPart.GetMinYMaxXPoint());
                }
            }

            if (pointList.Count == 0 || pointList.Count == 1)
            {
                return;
            }

            PointList dimPointList = new PointList();

            foreach (Point pt in pointList)
            {
                dimPointList.Add(pt);
            }

            Vector dimVector = new Vector(1, 0, 0);
            double dimDistance = Math.Abs(CCommonPara.mViewMaxX - pointList[0].X) + 0.6 * CCommonPara.mDefaultDimDistance;

            CDimTools.GetInstance().DrawDimensionSet(mViewBase, dimPointList, dimVector, dimDistance, CCommonPara.mSizeDimPath);
        }

        /// <summary>
        /// 绘制左侧零部件的标注;
        /// </summary>
        private void DrawYVectorLeftDim()
        {
            List<Point> pointList = new List<Point>();

            foreach (CMrPart mrPart in mMrPartList)
            {
                if (mrPart.GetMinXPoint().X > mRightPart.GetMinXPoint().X)
                {
                    continue;
                }
                if (!mrPart.IsHaveBolt())
                {
                    continue;
                }

                bool bFlag = false;

                List<CMrBoltArray> mrBoltArrayList = mrPart.GetBoltArrayList();

                foreach (CMrBoltArray mrBoltArray in mrBoltArrayList)
                {
                    if (!CDimTools.GetInstance().IsTwoVectorParallel(mrBoltArray.mNormal, new Vector(0, 0, 1)))
                    {
                        continue;
                    }
                    if (mDicBoltArrayToRightDim.ContainsKey(mrBoltArray))
                    {
                        continue;
                    }

                    List<Point> minXPointList = mrBoltArray.GetMinXPointList();

                    pointList.AddRange(minXPointList);

                    bFlag = true;
                }
                if (bFlag)
                {
                    pointList.Add(new Point(mrPart.GetMinXPoint().X, 0, 0));

                    if (IsMidPlate(mrPart))
                    {
                        pointList.Add(mrPart.GetMaxYMinXPoint());
                        pointList.Add(mrPart.GetMinYMinXPoint());
                    }
                }
            }

            if (pointList.Count == 0 || pointList.Count == 1)
            {
                return;
            }

            PointList dimPointList = new PointList();

            foreach (Point pt in pointList)
            {
                dimPointList.Add(pt);
            }

            Vector dimVector = new Vector(-1, 0, 0);
            double dimDistance = Math.Abs(CCommonPara.mViewMinX - pointList[0].X) + 0.6 * CCommonPara.mDefaultDimDistance;

            CDimTools.GetInstance().DrawDimensionSet(mViewBase, dimPointList, dimVector, dimDistance, CCommonPara.mSizeDimPath);
        }

        /// <summary>
        /// 绘制下侧的零部件标注;
        /// </summary>
        private void DrawYVectorDownDim()
        {
            List<Point> pointList = new List<Point>();

            bool bNeedAddLeftPart = true;
            bool bNeedAddRightPart = true;

            foreach (CMrPart mrPart in mMrPartList)
            {
                double maxY = mrPart.GetMaxYPoint().Y;
                double minY = mrPart.GetMinYPoint().Y;

                if (minY > 0 && maxY > 0)
                {
                    continue;
                }

                Vector normal = mrPart.mNormal;

                //1.如果法向与Z轴平行并且有螺钉;
                List<CMrBoltArray> mrBoltArrayList = mrPart.GetBoltArrayList();

                foreach (CMrBoltArray mrBoltArray in mrBoltArrayList)
                {
                    if (!CDimTools.GetInstance().IsTwoVectorParallel(mrBoltArray.mNormal, new Vector(0, 0, 1)))
                    {
                        continue;
                    }
                    if (mDicBoltArrayToUpDim.ContainsKey(mrBoltArray))
                    {
                        continue;
                    }

                    List<Point> minYPointList = mrBoltArray.GetMinYPointList();
                    pointList.AddRange(minYPointList);
                }
                
                //2.如果法向与Y轴平行;
                if (CDimTools.GetInstance().IsTwoVectorParallel(normal, new Vector(1, 0, 0)))
                {
                    if (mrPart.GetMaxXPoint().X < mLeftPart.GetMaxXPoint().X || mrPart.GetMinXPoint().X > mRightPart.GetMinXPoint().X)
                    {
                        continue;
                    }

                    if (!IsLeftPartNeedDim(mrPart))
                    {
                        bNeedAddLeftPart = false;
                        pointList.Add(mrPart.GetMinYMaxXPoint());
                    }
                    else if (!IsRightPartNeedDim(mrPart))
                    {
                        bNeedAddRightPart = false;
                        pointList.Add(mrPart.GetMinYMinXPoint());
                    }
                    else
                    {
                        pointList.Add(mrPart.GetMinYMinXPoint());
                    }
                }
                if (IsMidPlate(mrPart))
                {
                    pointList.Add(mrPart.GetMinYMinXPoint());
                    pointList.Add(mrPart.GetMinYMaxXPoint());
                }
            }
            if (pointList.Count == 0)
            {
                return;
            }

            if (mLeftPart != null && bNeedAddLeftPart)
            {
                pointList.Add(mLeftPart.GetMinYMinXPoint());
            }
            if (mRightPart != null && bNeedAddRightPart)
            {
                pointList.Add(mRightPart.GetMinYMaxXPoint());
            }

            Comparison<Point> sorterX = new Comparison<Point>(CDimTools.ComparePointX);
            pointList.Sort(sorterX);

            PointList dimPointList = new PointList();

            foreach (Point pt in pointList)
            {
                dimPointList.Add(pt);
            }

            Vector dimVector = new Vector(0, -1, 0);
            double dimDistance = Math.Abs(CCommonPara.mViewMinY - pointList[0].Y) + 0.6 * CCommonPara.mDefaultDimDistance;
            CDimTools.GetInstance().DrawDimensionSet(mViewBase, dimPointList, dimVector, dimDistance, CCommonPara.mSizeDimPath);
        }

        /// <summary>
        /// 判断是否含有底部的平板零件;
        /// </summary>
        /// <returns></returns>
        private bool IsHaveBottomPlatePart()
        {
            foreach (CMrPart mrPart in mMrPartList)
            {
                if (IsMidPlate(mrPart))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 构建左右侧的零件;
        /// </summary>
        private void BuildLeftAndRightPart()
        {
            foreach (CMrPart mrPart in mMrPartList)
            {
                Vector normal = mrPart.mNormal;

                if (!CDimTools.GetInstance().IsTwoVectorParallel(normal, new Vector(1, 0, 0))
                    && !CDimTools.GetInstance().IsVectorInXZPlane(normal))
                {
                    continue;
                }

                JudgeLeftPart(mrPart);
                JudgeRightPart(mrPart);
            }
        }

        /// <summary>
        /// 判断左侧的零部件;
        /// </summary>
        /// <param name="mrPart"></param>
        private void JudgeLeftPart(CMrPart mrPart)
        {
            double minX = mrPart.GetMinXPoint().X;
            double maxX = mrPart.GetMaxXPoint().X;
            double minY = mrPart.GetMinYPoint().Y;
            double maxY = mrPart.GetMaxYPoint().Y;

            if (minX > 0 || minY > 0 || maxY < 0)
            {
                return;
            }

            if (mLeftPart == null)
            {
                mLeftPart = mrPart;
                return;
            }

            if (Math.Abs(maxX - minX) > 50)
            {
                return;
            }
        
            double leftPartMinX = mLeftPart.GetMinXPoint().X;

            if (leftPartMinX < minX)
            {
                mLeftPart = mrPart;
            }
        }

        /// <summary>
        /// 判断右侧的零部件;
        /// </summary>
        /// <param name="mrPart"></param>
        private void JudgeRightPart(CMrPart mrPart)
        {
            double minX = mrPart.GetMinXPoint().X;
            double maxX = mrPart.GetMinXPoint().X;
            double minY = mrPart.GetMinYPoint().Y;
            double maxY = mrPart.GetMaxYPoint().Y;

            if (maxX < 0 || minY > 0 || maxY < 0)
            {
                return;
            }
            if (mRightPart == null)
            {
                mRightPart = mrPart;
                return;
            }
            if (Math.Abs(maxX - minX) > 50)
            {
                return;
            }

            double rightPartMaxX = mRightPart.GetMaxXPoint().X;

            if (rightPartMaxX > maxX)
            {
                mRightPart = mrPart;
            }
        }

        /// <summary>
        /// 判断左侧的零部件是否需要标注;
        /// </summary>
        /// <param name="mrPart"></param>
        /// <returns></returns>
        private bool IsLeftPartNeedDim(CMrPart mrPart)
        {
            double minX = mrPart.GetMinXPoint().X;
            double maxX = mrPart.GetMaxXPoint().X;

            double leftPartMaxX = mLeftPart.GetMaxXPoint().X;
            double leftPartMinX = mLeftPart.GetMinXPoint().X;
           
            if (CDimTools.GetInstance().CompareTwoDoubleValue(maxX, leftPartMaxX) == 0 ||
                CDimTools.GetInstance().CompareTwoDoubleValue(minX, leftPartMaxX) == 0 ||
                CDimTools.GetInstance().CompareTwoDoubleValue(minX,leftPartMinX) ==0)
            {
                return false;
            }
            if(CDimTools.GetInstance().CompareTwoDoubleValue(minX,leftPartMinX) >= 0 &&
                CDimTools.GetInstance().CompareTwoDoubleValue(maxX, leftPartMaxX) <= 0)
            {
                return false;
            }
            if (!CDimTools.GetInstance().IsTwoVectorParallel(mLeftPart.mNormal, new Vector(1, 0, 0)))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 判断右侧的零部件是否需要标注;
        /// </summary>
        /// <param name="mrPart"></param>
        /// <returns></returns>
        private bool IsRightPartNeedDim(CMrPart mrPart)
        {
            double minX = mrPart.GetMinXPoint().X;
            double maxX = mrPart.GetMaxXPoint().X;

            double rightPartMaxX = mRightPart.GetMaxXPoint().X;
            double rightPartMinX = mRightPart.GetMinXPoint().X;

            if (CDimTools.GetInstance().CompareTwoDoubleValue(maxX, rightPartMaxX) == 0 ||
                CDimTools.GetInstance().CompareTwoDoubleValue(minX, rightPartMinX) == 0 ||
                CDimTools.GetInstance().CompareTwoDoubleValue(maxX, rightPartMinX) == 0)
            {
                return false;
            }
            if (CDimTools.GetInstance().CompareTwoDoubleValue(minX, rightPartMinX) >= 0 &&
                CDimTools.GetInstance().CompareTwoDoubleValue(maxX, rightPartMaxX) <= 0)
            {
                return false;
            }
            if (!CDimTools.GetInstance().IsTwoVectorParallel(mLeftPart.mNormal, new Vector(1, 0, 0)))
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 判断是否是中间的板状零件;
        /// </summary>
        /// <param name="mrPart"></param>
        /// <returns></returns>
        private bool IsMidPlate(CMrPart mrPart)
        {
            Vector normal = mrPart.mNormal;

            if (!CDimTools.GetInstance().IsTwoVectorParallel(normal, new Vector(0, 0, 1)))
            {
                return false;
            }
            if (!mrPart.IsHaveBolt())
            {
                return false;
            }

            double minX = mrPart.GetMinXPoint().X;
            double maxX = mrPart.GetMaxXPoint().X;
            double minY = mrPart.GetMinYPoint().Y;
            double maxY = mrPart.GetMaxYPoint().Y;

            if (minX < 0 && maxX > 0 && minY < 0 && maxY > 0)
            {
                return true;
            }

            return false;
        }
    }
}
