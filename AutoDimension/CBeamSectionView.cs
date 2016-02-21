using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using AutoDimension.Entity;
using Tekla.Structures;
using Tekla.Structures.Drawing;
using Tekla.Structures.Model;
using Tekla.Structures.Geometry3d;

using TSD = Tekla.Structures.Drawing;
using TSM = Tekla.Structures.Model;
using System.Windows.Forms;

namespace AutoDimension
{
    /// <summary>
    /// 剖视图的View对象类;
    /// </summary>
    public class CBeamSectionView:CView
    {
        /// <summary>
        /// 剖面的构造函数;
        /// </summary>
        /// <param name="viewBase"></param>
        /// <param name="model"></param>
        public CBeamSectionView(TSD.View viewBase, Model model)
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

           CDimTools dimTools = CDimTools.GetInstance();
           
           //初始化视图的包围盒;           
           dimTools.InitViewBox();

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

               List<Point> pointList = mrPart.GetPointList();
               int nCount = pointList.Count;

               Comparison<Point> sorterX = new Comparison<Point>(CDimTools.ComparePointX);
               pointList.Sort(sorterX);
               Point minXPoint = pointList[0];
               Point maxXPoint = pointList[nCount - 1];

               if (minXPoint.X > viewMaxPoint.X || maxXPoint.X < viewMinPoint.X)
               {
                   continue;
               }

               Comparison<Point> sorterY = new Comparison<Point>(CDimTools.ComparePointY);
               pointList.Sort(sorterY);
               Point minYPoint = pointList[0];
               Point maxYPoint = pointList[nCount - 1];

               if (minYPoint.Y > viewMaxPoint.Y || maxYPoint.Y < viewMinPoint.Y)
               {
                   continue;
               }

               Comparison<Point> sorterZ = new Comparison<Point>(CDimTools.ComparePointZ);
               pointList.Sort(sorterZ);
               Point minZPoint = pointList[0];
               Point maxZPoint = pointList[nCount - 1];
 
               if (minZPoint.Z > viewMaxPoint.Z || maxZPoint.Z < viewMinPoint.Z)
               {
                    continue;
               }

               dimTools.UpdateViewBox(mrPart);

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

           CMrMarkManager.GetInstance().Clear();
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
            //判断主梁在剖视图中的法向,主要是考虑与X方向和Y方向平行的两个方向;
            Vector normal = mMainBeam.mNormal;

            Vector xVector = new Vector(1, 0, 0);
            Vector yVector = new Vector(0, 1, 0);

            if (CDimTools.GetInstance().IsTwoVectorParallel(normal, xVector)
                || CDimTools.GetInstance().IsVectorInXZPlane(normal))
            {
                DrawXParallelPartMark();
            }
            else if (CDimTools.GetInstance().IsTwoVectorParallel(normal, yVector)
                || CDimTools.GetInstance().IsVectorInYZPlane(normal))
            {
                DrawYParallelPartMark();
            }
        }

        /// <summary>
        /// 绘制主梁与X轴平行时的零件标记;
        /// </summary>
        private void DrawXParallelPartMark()
        {
            Vector zVector = new Vector(0, 0, 1);

            DrawingHandler drawingHandler = new DrawingHandler();
            TSD.UI.DrawingObjectSelector DS = drawingHandler.GetDrawingObjectSelector();

            foreach(CMrPart mrPart in mMrPartList)
            {
                DS.SelectObject(mrPart.mPartInDrawing);
                   
                List<CMrBoltArray> mrBoltArrayList = mrPart.GetBoltArrayList();

                foreach (CMrBoltArray mrBoltArray in mrBoltArrayList)
                {
                    Vector boltNormal = mrBoltArray.mNormal;

                    if(!CDimTools.GetInstance().IsTwoVectorParallel(zVector,boltNormal))
                    {
                        continue;
                    }
                    DS.SelectObject(mrBoltArray.mBoltInDrawing);
                }
            }
            CDimTools.GetInstance().DrawMarkByMacro();
        }

        /// <summary>
        /// 绘制主梁与Y轴平行时的零件标记;
        /// </summary>
        private void DrawYParallelPartMark()
        {
            Vector zVector = new Vector(0, 0, 1);

            DrawingHandler drawingHandler = new DrawingHandler();
            TSD.UI.DrawingObjectSelector DS = drawingHandler.GetDrawingObjectSelector();

            foreach (CMrPart mrPart in mMrPartList)
            {
                Vector normal = mrPart.mNormal;

                if (!CDimTools.GetInstance().IsTwoVectorParallel(normal, zVector))
                {
                    continue;
                }
                DS.SelectObject(mrPart.mPartInDrawing);

                //2.对螺钉组进行标注;
                List<CMrBoltArray> mrBoltArrayList = mrPart.GetBoltArrayList();

                foreach (CMrBoltArray mrBoltArray in mrBoltArrayList)
                {
                    Vector boltNormal = mrBoltArray.mNormal;

                    if (!CDimTools.GetInstance().IsTwoVectorParallel(boltNormal, zVector))
                    {
                        continue;
                    }
                    DS.SelectObject(mrBoltArray.mBoltInDrawing);
                }
            }
            CDimTools.GetInstance().DrawMarkByMacro();
        }

        /// <summary>
        /// 绘制剖面的标注;
        /// </summary>
        private void DrawSectionDim()
        {
            //判断主梁在剖视图中的法向,主要是考虑与X方向和Y方向平行的两个方向;
            Vector normal = mMainBeam.mNormal;

            Vector xVector = new Vector(1, 0, 0);
            Vector yVector = new Vector(0, 1, 0);

            if(CDimTools.GetInstance().IsTwoVectorParallel(normal,xVector))
            {
                DrawXVectorSectionDim();
            }
            else if(CDimTools.GetInstance().IsVectorInXZPlane(normal))
            {
                DrawXZVectorSectionDim();
            }
            else if(CDimTools.GetInstance().IsTwoVectorParallel(normal,yVector))
            {
                DrawYVectorSectionDim();
            }
            else if(CDimTools.GetInstance().IsVectorInYZPlane(normal))
            {
                DrawYZvectorSectionDim();
            }
        }

        private void DrawXZVectorSectionDim()
        {

        }

        /// <summary>
        /// 如果向量是在YZ平面上;
        /// </summary>
        private void DrawYZvectorSectionDim()
        {
            List<Point> upPointList = new List<Point>();
            List<Point> downPointList = new List<Point>();
            List<Point> leftPointList = new List<Point>();
            List<Point> rightPointList = new List<Point>();

            double mainBeamMinX = mMainBeam.mLeftTopPoint.X;
            double mainBeamMaxX = mMainBeam.mRightTopPoint.X;

            Vector xVector = new Vector(1, 0, 0);
            Vector yVector = new Vector(0, 1, 0);
            Vector zVector = new Vector(0, 0, 1);

            foreach (CMrPart mrPart in mMrPartList)
            {
                if (!mrPart.IsHaveBolt())
                {
                    continue;
                }

                List<Point> pointList = mrPart.GetPointList();

                int nCount = pointList.Count;

                Comparison<Point> sorterX = new Comparison<Point>(CDimTools.ComparePointX);
                pointList.Sort(sorterX);
                Point partMinXPoint = pointList[0];
                Point partMaxXPoint = pointList[nCount - 1];

                Comparison<Point> sorterY = new Comparison<Point>(CDimTools.ComparePointY);
                pointList.Sort(sorterY);
                Point partMinYPoint = pointList[0];
                Point partMaxYPoint = pointList[nCount - 1];

                Vector normal = mrPart.mNormal;

                if (CDimTools.GetInstance().IsTwoVectorParallel(xVector, normal))
                {
                    if (partMinYPoint.Y > CCommonPara.mDblError)
                    {
                        if (IsValidPoint(mrPart.mLeftTopPoint))
                        {
                            upPointList.Add(mrPart.mLeftTopPoint);
                        }
                    }
                    else if (partMinYPoint.Y < CCommonPara.mDblError)
                    {
                        if (IsValidPoint(mrPart.mLeftBottomPoint))
                        {
                            downPointList.Add(mrPart.mLeftBottomPoint);
                        }
                    }
                }
                else if (CDimTools.GetInstance().IsTwoVectorParallel(yVector, normal))
                {
                    if (CDimTools.GetInstance().CompareTwoDoubleValue(partMaxXPoint.X, mainBeamMinX) <= 0)
                    {
                        if (IsValidPoint(mrPart.mLeftTopPoint))
                        {
                            leftPointList.Add(mrPart.mLeftTopPoint);
                        }
                    }
                    else if (CDimTools.GetInstance().CompareTwoDoubleValue(partMinXPoint.X, mainBeamMaxX) >= 0)
                    {
                        if (IsValidPoint(mrPart.mRightTopPoint))
                        {
                            rightPointList.Add(mrPart.mRightTopPoint);
                        }
                    }
                }
                else if (CDimTools.GetInstance().IsTwoVectorParallel(zVector, normal))
                {
                    if (partMinYPoint.Y > CCommonPara.mDblError)
                    {
                        List<CMrBoltArray> mrBoltArrayList = mrPart.GetBoltArrayList();

                        foreach (CMrBoltArray mrBoltArray in mrBoltArrayList)
                        {
                            Vector boltNormal = mrBoltArray.mNormal;

                            if (!CDimTools.GetInstance().IsTwoVectorParallel(boltNormal, zVector))
                            {
                                continue;
                            }

                            List<Point> boltMinXPointList = mrBoltArray.GetMinXPointList();
                            List<Point> boltMaxYPointList = mrBoltArray.GetMaxYPointList();

                            leftPointList.AddRange(boltMinXPointList);
                            upPointList.AddRange(boltMaxYPointList);
                        }
                    }
                    else if (partMaxYPoint.Y < CCommonPara.mDblError)
                    {
                        List<CMrBoltArray> mrBoltArrayList = mrPart.GetBoltArrayList();

                        foreach (CMrBoltArray mrBoltArray in mrBoltArrayList)
                        {
                            Vector boltNormal = mrBoltArray.mNormal;

                            if (!CDimTools.GetInstance().IsTwoVectorParallel(boltNormal, zVector))
                            {
                                continue;
                            }

                            List<Point> boltMinYPointList = mrBoltArray.GetMinYPointList();
                            List<Point> boltMinXPointList = mrBoltArray.GetMinXPointList();

                            leftPointList.AddRange(boltMinXPointList);
                            downPointList.AddRange(boltMinYPointList);
                        }
                    }
                }
            }

            Point viewMinPoint = mViewBase.RestrictionBox.MinPoint;
            Point viewMaxPoint = mViewBase.RestrictionBox.MaxPoint;

            double viewMaxY = viewMaxPoint.Y;
            double viewMinY = viewMinPoint.Y;
            double viewMaxX = viewMaxPoint.X;
            double viewMinX = viewMinPoint.X;

            AdjustMainBeamPoint();

            if (upPointList.Count > 0)
            {
                upPointList.Add(mMainBeam.mLeftTopPoint);
                upPointList.Add(mMainBeam.mRightTopPoint);

                Point dimMinYPoint = CDimTools.GetInstance().GetMinYPoint(upPointList);

                Comparison<Point> sorterX = new Comparison<Point>(CDimTools.ComparePointX);
                upPointList.Sort(sorterX);

                Vector upVector = new Vector(0, 1, 0);
                double distance = Math.Abs(viewMaxY - upPointList[0].Y) + CCommonPara.mDefaultDimDistance;

                CMrDimSet mrDimSet = new CMrDimSet();

                mrDimSet.AddRange(upPointList);
                mrDimSet.mDimVector = upVector;
                mrDimSet.mDimDistance = distance;

                CreateDimByMrDimSet(mrDimSet);
            }
            if (downPointList.Count > 0)
            {
                downPointList.Add(mMainBeam.mLeftBottomPoint);
                downPointList.Add(mMainBeam.mRightBottomPoint);

                Point dimMaxYPoint = CDimTools.GetInstance().GetMaxYPoint(downPointList);

                Comparison<Point> sorterX = new Comparison<Point>(CDimTools.ComparePointX);
                downPointList.Sort(sorterX);

                Vector downVector = new Vector(0, -1, 0);
                double distance = Math.Abs(viewMinY - downPointList[0].Y) + CCommonPara.mDefaultDimDistance;

                CMrDimSet mrDimSet = new CMrDimSet();

                mrDimSet.AddRange(downPointList);
                mrDimSet.mDimVector = downVector;
                mrDimSet.mDimDistance = distance;

                CreateDimByMrDimSet(mrDimSet);
            }
        }


        /// <summary>
        /// 绘制主梁与X方向平行的剖面;
        /// </summary>
        private void DrawXVectorSectionDim()
        {
            List<Point> upPointList = new List<Point>();
            List<Point> downPointList = new List<Point>();
            List<Point> leftPointList = new List<Point>();
            List<Point> rightPointList = new List<Point>();

            double mainBeamMaxY = mMainBeam.GetMaxYPoint().Y;
            double mainBeamMinY = mMainBeam.GetMinYPoint().Y;

            Vector xVector = new Vector(1, 0, 0);
            Vector yVector = new Vector(0, 1, 0);
            Vector zVector = new Vector(0, 0, 1);

            //记录标注中是否存在螺钉,只有法向与Z轴平行的螺钉才进行标注;
            bool bHaveBoltDim = false;

            foreach(CMrPart mrPart in mMrPartList)
            {
                if (!mrPart.IsHaveBolt())
                {
                    continue;
                }

                Point partMinXPoint = mrPart.GetMinXPoint();
                Point partMaxXPoint = mrPart.GetMaxXPoint();
                Point partMinYPoint = mrPart.GetMinYPoint();
                Point partMaxYPoint = mrPart.GetMaxYPoint();
                
                Vector normal = mrPart.mNormal;

                //1.先标注法向与Z轴平行的螺钉组;
                List<CMrBoltArray> mrBoltArrayList = mrPart.GetBoltArrayList();

                foreach (CMrBoltArray mrBoltArray in mrBoltArrayList)
                {
                    Vector boltNormal = mrBoltArray.mNormal;

                    if (!CDimTools.GetInstance().IsTwoVectorParallel(boltNormal, zVector))
                    {
                        continue;
                    }

                    List<Point> boltMinXPointList = mrBoltArray.GetMinXPointList();
                    List<Point> boltMaxXPointList = mrBoltArray.GetMaxXPointList();
                    List<Point> boltMaxYPointList = mrBoltArray.GetMaxYPointList();

                    if (boltMaxXPointList[0].X < CCommonPara.mDblError)
                    {
                        leftPointList.AddRange(boltMinXPointList);
                        upPointList.AddRange(boltMaxYPointList);
                    }
                    else
                    {
                        rightPointList.AddRange(boltMaxXPointList);
                        upPointList.AddRange(boltMaxYPointList);
                    }
                    bHaveBoltDim = true;
                }

                //2.标注法向与X轴平行的零部件;
                if(CDimTools.GetInstance().IsTwoVectorParallel(xVector,normal))
                {
                    if (CDimTools.GetInstance().CompareTwoDoubleValue(partMinYPoint.Y,mainBeamMaxY) >= 0)
                    {
                        upPointList.Add(mrPart.GetMaxYMinXPoint());
                    }
                    else if (CDimTools.GetInstance().CompareTwoDoubleValue(partMaxYPoint.Y, mainBeamMinY) <= 0)
                    {
                        downPointList.Add(mrPart.GetMinYMinXPoint());
                    }
                }
                //3.标注法向与Y轴平行的零部件;
                else if(CDimTools.GetInstance().IsTwoVectorParallel(yVector,normal))
                {
                    if (partMaxYPoint.X < CCommonPara.mDblError)
                    {
                        leftPointList.Add(mrPart.GetMaxYMinXPoint());
                    }
                    else if(partMinXPoint.X > CCommonPara.mDblError)
                    {
                        rightPointList.Add(mrPart.GetMaxYMaxXPoint());
                    }
                }
                else if(CDimTools.GetInstance().IsTwoVectorParallel(zVector,normal))
                {
                    //4.法向与Z轴平行的零部件只标注螺钉，故不标注;
                }
            }

            Point viewMinPoint = mViewBase.RestrictionBox.MinPoint;
            Point viewMaxPoint = mViewBase.RestrictionBox.MaxPoint;

            double viewMaxY = viewMaxPoint.Y;
            double viewMinY = viewMinPoint.Y;
            double viewMaxX = viewMaxPoint.X;
            double viewMinX = viewMinPoint.X;

            AdjustMainBeamPoint();

            //1.如果主梁上面需要进行标注;
            if (upPointList.Count > 0)
            {
                if (upPointList.Count > 1)
                {
                    Point middlePoint = new Point(0, mMainBeam.GetMaxYPoint().Y, 0);
                    upPointList.Add(middlePoint);
                }
                else if (upPointList.Count == 1)
                {
                    if (bHaveBoltDim)
                    {
                        Point middlePoint = new Point(0, mMainBeam.mLeftTopPoint.Y, 0);
                        if (upPointList[0].X < CCommonPara.mDblError)
                        {
                            upPointList.Add(mMainBeam.mLeftTopPoint);
                            upPointList.Add(middlePoint);
                        }
                        else if (upPointList[0].X > CCommonPara.mDblError)
                        {
                            upPointList.Add(mMainBeam.mRightTopPoint);
                            upPointList.Add(middlePoint);
                        }
                    }
                    else
                    {
                        upPointList.Add(mMainBeam.mLeftTopPoint);
                        upPointList.Add(mMainBeam.mRightTopPoint);
                    }
                }
                Comparison<Point> sorterX = new Comparison<Point>(CDimTools.ComparePointX);
                upPointList.Sort(sorterX);
                Vector upVector = new Vector(0, 1, 0);
                double distance = Math.Abs(CCommonPara.mViewMaxY - upPointList[0].Y) + CCommonPara.mDefaultDimDistance;
                CMrDimSet mrDimSet = new CMrDimSet();
                mrDimSet.AddRange(upPointList);
                mrDimSet.mDimVector = upVector;
                mrDimSet.mDimDistance = distance;
                CreateDimByMrDimSet(mrDimSet);
            }

            //2.如果主梁的下面需要进行标注;
            if (downPointList.Count > 0)
            {
                if (downPointList.Count > 1)
                {
                    Point middlePoint = new Point(0, mMainBeam.mLeftBottomPoint.Y, 0);
                    downPointList.Add(middlePoint);
                }
                else if (downPointList.Count == 1)
                {
                    Point middlePoint = new Point(0, mMainBeam.mLeftBottomPoint.Y, 0);
                    if (downPointList[0].X > CCommonPara.mDblError)
                    {
                        downPointList.Add(mMainBeam.mRightBottomPoint);
                        downPointList.Add(middlePoint);
                    }
                    else if(downPointList[0].X < CCommonPara.mDblError)
                    {
                        downPointList.Add(mMainBeam.mLeftBottomPoint);
                        downPointList.Add(middlePoint);
                    }
                    else
                    {
                        downPointList.Add(mMainBeam.mLeftBottomPoint);
                        downPointList.Add(mMainBeam.mRightBottomPoint);
                    }
                }
                Comparison<Point> sorterX = new Comparison<Point>(CDimTools.ComparePointX);
                downPointList.Sort(sorterX);
                Vector downVector = new Vector(0, -1, 0);
                double distance = Math.Abs(CCommonPara.mViewMinY - downPointList[0].Y) + CCommonPara.mDefaultDimDistance;
                CMrDimSet mrDimSet = new CMrDimSet();
                mrDimSet.AddRange(downPointList);
                mrDimSet.mDimVector = downVector;
                mrDimSet.mDimDistance = distance;
                CreateDimByMrDimSet(mrDimSet);
            }

            //3.如果主梁右侧需要进行标注;
            if(rightPointList.Count > 0)
            {
                rightPointList.Add(mMainBeam.mRightTopPoint);
                Comparison<Point> sorterY = new Comparison<Point>(CDimTools.ComparePointY);
                rightPointList.Sort(sorterY);
                Vector rightVector = new Vector(1, 0, 0);
                double distance = Math.Abs(CCommonPara.mViewMaxX - rightPointList[0].X) + CCommonPara.mDefaultDimDistance;
                CMrDimSet mrDimSet = new CMrDimSet();
                mrDimSet.AddRange(rightPointList);
                mrDimSet.mDimVector = rightVector;
                mrDimSet.mDimDistance = distance;
                CreateDimByMrDimSet(mrDimSet);
            }
            //4.如果梁的左侧需要进行标注;
            if (leftPointList.Count > 0)
            {
                leftPointList.Add(mMainBeam.mLeftTopPoint);
                Comparison<Point> sorterY = new Comparison<Point>(CDimTools.ComparePointY);
                leftPointList.Sort(sorterY);
                Vector leftVector = new Vector(-1, 0, 0);
                double distance = Math.Abs(CCommonPara.mViewMinX - leftPointList[0].X) + CCommonPara.mDefaultDimDistance;
                CMrDimSet mrDimSet = new CMrDimSet();
                mrDimSet.AddRange(leftPointList);
                mrDimSet.mDimVector = leftVector;
                mrDimSet.mDimDistance = distance;
                CreateDimByMrDimSet(mrDimSet);
            }
        }

        /// <summary>
        /// 绘制主梁与Y方向平行的剖面;
        /// </summary>
        private void DrawYVectorSectionDim()
        {
            List<Point> upPointList = new List<Point>();
            List<Point> downPointList = new List<Point>();
            List<Point> leftPointList = new List<Point>();
            List<Point> rightPointList = new List<Point>();

            double mainBeamMinX = mMainBeam.mLeftTopPoint.X;
            double mainBeamMaxX = mMainBeam.mRightTopPoint.X;

            Vector xVector = new Vector(1, 0, 0);
            Vector yVector = new Vector(0, 1, 0);
            Vector zVector = new Vector(0, 0, 1);

            foreach (CMrPart mrPart in mMrPartList)
            {
                if (!mrPart.IsHaveBolt())
                {
                    continue;
                }

                List<Point> pointList = mrPart.GetPointList();

                int nCount = pointList.Count;

                Comparison<Point> sorterX = new Comparison<Point>(CDimTools.ComparePointX);
                pointList.Sort(sorterX);
                Point partMinXPoint = pointList[0];
                Point partMaxXPoint = pointList[nCount - 1];

                Comparison<Point> sorterY = new Comparison<Point>(CDimTools.ComparePointY);
                pointList.Sort(sorterY);
                Point partMinYPoint = pointList[0];
                Point partMaxYPoint = pointList[nCount - 1];

                Vector normal = mrPart.mNormal;

                if (CDimTools.GetInstance().IsTwoVectorParallel(xVector, normal))
                {
                    if (partMinYPoint.Y > CCommonPara.mDblError)
                    {
                        if (IsValidPoint(mrPart.mLeftTopPoint))
                        {
                            upPointList.Add(mrPart.mLeftTopPoint);
                        }
                    }
                    else if (partMinYPoint.Y < CCommonPara.mDblError )
                    {
                        if (IsValidPoint(mrPart.mLeftBottomPoint))
                        {
                            downPointList.Add(mrPart.mLeftBottomPoint);
                        }
                    }
                }
                else if (CDimTools.GetInstance().IsTwoVectorParallel(yVector, normal))
                {
                    if(CDimTools.GetInstance().CompareTwoDoubleValue(partMaxXPoint.X,mainBeamMinX) <= 0)
                    {
                        if (IsValidPoint(mrPart.mLeftTopPoint))
                        {
                            leftPointList.Add(mrPart.mLeftTopPoint);
                        }
                    }
                    else if (CDimTools.GetInstance().CompareTwoDoubleValue(partMinXPoint.X, mainBeamMaxX) >= 0)
                    {
                        if (IsValidPoint(mrPart.mRightTopPoint))
                        {
                            rightPointList.Add(mrPart.mRightTopPoint);
                        }
                    }
                }
                else if (CDimTools.GetInstance().IsTwoVectorParallel(zVector, normal))
                {
                    if (partMinYPoint.Y > CCommonPara.mDblError)
                    {
                        List<CMrBoltArray> mrBoltArrayList = mrPart.GetBoltArrayList();

                        foreach (CMrBoltArray mrBoltArray in mrBoltArrayList)
                        {
                            Vector boltNormal = mrBoltArray.mNormal;

                            if (!CDimTools.GetInstance().IsTwoVectorParallel(boltNormal, zVector))
                            {
                                continue;
                            }

                            List<Point> boltMinXPointList = mrBoltArray.GetMinXPointList();
                            List<Point> boltMaxYPointList = mrBoltArray.GetMaxYPointList();

                            leftPointList.AddRange(boltMinXPointList);
                            upPointList.AddRange(boltMaxYPointList);
                        }
                    }
                    else if (partMaxYPoint.Y < CCommonPara.mDblError)
                    {
                        List<CMrBoltArray> mrBoltArrayList = mrPart.GetBoltArrayList();

                        foreach (CMrBoltArray mrBoltArray in mrBoltArrayList)
                        {
                            Vector boltNormal = mrBoltArray.mNormal;

                            if (!CDimTools.GetInstance().IsTwoVectorParallel(boltNormal, zVector))
                            {
                                continue;
                            }

                            List<Point> boltMinYPointList = mrBoltArray.GetMinYPointList();
                            List<Point> boltMinXPointList = mrBoltArray.GetMinXPointList();

                            leftPointList.AddRange(boltMinXPointList);
                            downPointList.AddRange(boltMinYPointList);
                        }
                    }
                }
            }

            Point viewMinPoint = mViewBase.RestrictionBox.MinPoint;
            Point viewMaxPoint = mViewBase.RestrictionBox.MaxPoint;

            double viewMaxY = viewMaxPoint.Y;
            double viewMinY = viewMinPoint.Y;
            double viewMaxX = viewMaxPoint.X;
            double viewMinX = viewMinPoint.X;

            AdjustMainBeamPoint();

            if (upPointList.Count > 0)
            {
                upPointList.Add(mMainBeam.mLeftTopPoint);
                upPointList.Add(mMainBeam.mRightTopPoint);
                
                Point dimMinYPoint = CDimTools.GetInstance().GetMinYPoint(upPointList);

                Comparison<Point> sorterX = new Comparison<Point>(CDimTools.ComparePointX);
                upPointList.Sort(sorterX);

                Vector upVector = new Vector(0, 1, 0);
                double distance = Math.Abs(viewMaxY - upPointList[0].Y) + CCommonPara.mDefaultDimDistance;

                CMrDimSet mrDimSet = new CMrDimSet();

                mrDimSet.AddRange(upPointList);
                mrDimSet.mDimVector = upVector;
                mrDimSet.mDimDistance = distance;

                CreateDimByMrDimSet(mrDimSet);
            }
            if (downPointList.Count > 0)
            {
                downPointList.Add(mMainBeam.mLeftBottomPoint);
                downPointList.Add(mMainBeam.mRightBottomPoint);

                Point dimMaxYPoint = CDimTools.GetInstance().GetMaxYPoint(downPointList);

                Comparison<Point> sorterX = new Comparison<Point>(CDimTools.ComparePointX);
                downPointList.Sort(sorterX);

                Vector downVector = new Vector(0, -1, 0);
                double distance = Math.Abs(viewMinY - downPointList[0].Y) + CCommonPara.mDefaultDimDistance;

                CMrDimSet mrDimSet = new CMrDimSet();

                mrDimSet.AddRange(downPointList);
                mrDimSet.mDimVector = downVector;
                mrDimSet.mDimDistance = distance;

                CreateDimByMrDimSet(mrDimSet);
            }
            if (rightPointList.Count > 0)
            {
                if(rightPointList.Count > 1)
                {
                    Point middlePoint = new Point(mMainBeam.mRightTopPoint.X, 0, 0);
                    rightPointList.Add(middlePoint);
                }
                else if(rightPointList.Count==1)
                {
                    rightPointList.Add(mMainBeam.mRightBottomPoint);
                    rightPointList.Add(mMainBeam.mRightTopPoint);
                }

                Point dimMinXPoint = CDimTools.GetInstance().GetMinXPoint(rightPointList);

                Comparison<Point> sorterY = new Comparison<Point>(CDimTools.ComparePointY);
                rightPointList.Sort(sorterY);

                Vector rightVector = new Vector(1, 0, 0);
                double distance = Math.Abs(viewMaxX - rightPointList[0].X) + CCommonPara.mDefaultDimDistance;

                CMrDimSet mrDimSet = new CMrDimSet();

                mrDimSet.AddRange(rightPointList);
                mrDimSet.mDimVector = rightVector;
                mrDimSet.mDimDistance = distance;

                CreateDimByMrDimSet(mrDimSet);
            }
            if (leftPointList.Count > 0)
            {
                if (leftPointList.Count > 1)
                {
                    Point middlePoint = new Point(mMainBeam.mLeftTopPoint.X, 0, 0);
                    leftPointList.Add(middlePoint);
                }
                else if (leftPointList.Count == 1)
                {
                    leftPointList.Add(mMainBeam.mLeftBottomPoint);
                    leftPointList.Add(mMainBeam.mLeftTopPoint);
                }

                Point dimMaxXPoint = CDimTools.GetInstance().GetMaxXPoint(leftPointList);

                Comparison<Point> sorterY = new Comparison<Point>(CDimTools.ComparePointY);
                leftPointList.Sort(sorterY);

                Vector leftVector = new Vector(-1, 0, 0);
                double distance = Math.Abs(viewMinX - leftPointList[0].X) + CCommonPara.mDefaultDimDistance;

                CMrDimSet mrDimSet = new CMrDimSet();

                mrDimSet.AddRange(leftPointList);
                mrDimSet.mDimVector = leftVector;
                mrDimSet.mDimDistance = distance;

                CreateDimByMrDimSet(mrDimSet);
            }
        }

        /// <summary>
        /// 先调整一下主梁的坐标,主要是因为如果是斜剖，主梁的X或者Y可能会很大;
        /// </summary>
        private void AdjustMainBeamPoint()
        {
            Point viewMinPoint = mViewBase.RestrictionBox.MinPoint;
            Point viewMaxPoint = mViewBase.RestrictionBox.MaxPoint;

            double viewMaxY = viewMaxPoint.Y;
            double viewMinY = viewMinPoint.Y;
            double viewMaxX = viewMaxPoint.X;
            double viewMinX = viewMinPoint.X;

            //调整主梁的Y坐标;
            if (CDimTools.GetInstance().CompareTwoDoubleValue(mMainBeam.mLeftTopPoint.Y, viewMaxY) > 0)
            {
                mMainBeam.mLeftTopPoint.Y = 0;
            }
            if (CDimTools.GetInstance().CompareTwoDoubleValue(mMainBeam.mLeftBottomPoint.Y, viewMinY) < 0)
            {
                mMainBeam.mLeftBottomPoint.Y = 0;
            }
            if (CDimTools.GetInstance().CompareTwoDoubleValue(mMainBeam.mRightTopPoint.Y, viewMaxY) > 0)
            {
                mMainBeam.mRightTopPoint.Y = 0;
            }
            if (CDimTools.GetInstance().CompareTwoDoubleValue(mMainBeam.mRightBottomPoint.Y, viewMinY) < 0)
            {
                mMainBeam.mRightBottomPoint.Y = 0;
            }

            //调整主梁的X坐标;
            if (CDimTools.GetInstance().CompareTwoDoubleValue(mMainBeam.mLeftTopPoint.X, viewMinX) < 0)
            {
                mMainBeam.mLeftTopPoint.X= 0;
            }
            if (CDimTools.GetInstance().CompareTwoDoubleValue(mMainBeam.mLeftBottomPoint.X, viewMinX) < 0)
            {
                mMainBeam.mLeftBottomPoint.X = 0;
            }
            if (CDimTools.GetInstance().CompareTwoDoubleValue(mMainBeam.mRightTopPoint.X, viewMaxX) > 0)
            {
                mMainBeam.mRightTopPoint.X = 0;
            }
            if (CDimTools.GetInstance().CompareTwoDoubleValue(mMainBeam.mRightBottomPoint.X, viewMaxX) > 0)
            {
                mMainBeam.mRightBottomPoint.X = 0;
            }
        }

        /// <summary>
        /// 判断零件中的点是否合法;
        /// </summary>
        /// <param name="point"></param>
        private bool IsValidPoint(Point point)
        {
            Point viewMinPoint = mViewBase.RestrictionBox.MinPoint;
            Point viewMaxPoint = mViewBase.RestrictionBox.MaxPoint;

            double viewMaxY = viewMaxPoint.Y;
            double viewMinY = viewMinPoint.Y;
            double viewMaxX = viewMaxPoint.X;
            double viewMinX = viewMinPoint.X;

            double dblX = point.X;
            double dblY = point.Y;

            if(CDimTools.GetInstance().CompareTwoDoubleValue(dblX,viewMinX) < 0)
            {
                return false;
            }
            if (CDimTools.GetInstance().CompareTwoDoubleValue(dblX, viewMaxX) > 0)
            {
                return false;
            }
            if (CDimTools.GetInstance().CompareTwoDoubleValue(dblY, viewMaxY) > 0)
            {
                return false;
            }
            if (CDimTools.GetInstance().CompareTwoDoubleValue(dblY, viewMinY) < 0)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 根据CMrDimSet来创建Tekla中的标注;
        /// </summary>
        /// <param name="mrDimSet"></param>
        private void CreateDimByMrDimSet(CMrDimSet mrDimSet)
        {
            PointList pointList = new PointList();

            foreach (Point point in mrDimSet.GetDimPointList())
            {
                pointList.Add(point);
            }

            CDimTools.GetInstance().DrawDimensionSet(mViewBase, pointList, mrDimSet.mDimVector, mrDimSet.mDimDistance, CCommonPara.mSizeDimPath);
        }
    }
}
