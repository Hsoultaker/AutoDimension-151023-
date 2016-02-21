using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tekla.Structures.Model;
using Tekla.Structures.Geometry3d;
using Tekla.Structures;
using AutoDimension.Entity;

using TSD = Tekla.Structures.Drawing;
using TSM = Tekla.Structures.Model;
using System.Threading;
using Tekla.Structures.Drawing;
using System.Windows.Forms;

namespace AutoDimension
{
    /// <summary>
    /// 梁的剖视图;
    /// </summary>
    public class CBeamDoorSectionView:CView
    {
         /// <summary>
        /// 构造函数;
        /// </summary>
        /// <param name="viewBase"></param>
        /// <param name="model"></param>
        public CBeamDoorSectionView(TSD.View viewBase, Model model)
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
        public void DrawPartMark()
        {
            Vector normal = mMainBeam.mNormal;

            //如果主梁的法向与X轴平行;
            if (CDimTools.GetInstance().IsTwoVectorParallel(normal, new Vector(1, 0, 0)))
            {
                DrawXVectorMark();
            }
            //如果主梁的法向与Y轴平行;
            else if (CDimTools.GetInstance().IsTwoVectorParallel(normal, new Vector(0, 1, 0)))
            {
                DrawYVectorMark();
            }
        }

        /// <summary>
        /// 绘制剖视图的标注;
        /// </summary>
        public void DrawSectionDim()
        {
            Vector normal=mMainBeam.mNormal;

            //如果主梁的法向与X轴平行;
            if (CDimTools.GetInstance().IsTwoVectorParallel(normal, new Vector(1, 0, 0)))
            {
                DrawXVectorSectionDim();
            }
            //如果主梁的法向与Y轴平行;
            else if (CDimTools.GetInstance().IsTwoVectorParallel(normal, new Vector(0, 1, 0)))
            {
                DrawYVectorSectionDim();
            }
        }

        /// <summary>
        /// 绘制主梁法向朝X方向的剖面标注;
        /// </summary>
        private void DrawXVectorSectionDim()
        {
            DrawXVectorTopDim();
            DrawXVectorRightDim();
            DrawXVectorDownDim();
        }

        /// <summary>
        /// 判断一个零件是否需要在顶部进行标注;
        /// </summary>
        /// <param name="mrPart"></param>
        /// <returns></returns>
        private bool IsNeedTopDim(CMrPart mrPart)
        {
            Vector normal = mrPart.mNormal;
            Vector zVector = new Vector(0, 0, 1);

            if (!CDimTools.GetInstance().IsTwoVectorParallel(normal, zVector) || !mrPart.IsHaveBolt())
            {
                return false;
            }

            double minY = mrPart.GetMinYPoint().Y;
            double maxY = mrPart.GetMaxYPoint().Y;

            //如果该板是与主梁交叉;
            if (minY < 0 && maxY > 0)
            {
                return true;
            }
            //如果在主梁上方;
            if (minY > 0)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 判断是否需要向下标注;
        /// </summary>
        /// <returns></returns>
        private bool IsNeedDownDim(CMrPart mrPart)
        {
            Vector normal = mrPart.mNormal;
            Vector zVector = new Vector(0, 0, 1);

            if (!CDimTools.GetInstance().IsTwoVectorParallel(normal, zVector) || !mrPart.IsHaveBolt())
            {
                return false;
            }

            double minY = mrPart.GetMinYPoint().Y;
            double maxY = mrPart.GetMaxYPoint().Y;
            double minX = mrPart.GetMinXPoint().X;
            double maxX = mrPart.GetMaxXPoint().X;

//             if (minY < 0 && maxY > 0 && minX < 0 && maxX > 0)
//             {
//                 return true;
//             }
            if (maxY < 0)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 绘制上侧的标注;
        /// </summary>
        private void DrawXVectorTopDim()
        {
            CMrDimSet mrDimSet = new CMrDimSet();

            Vector xVector = new Vector(1, 0, 0);
            Vector yVector = new Vector(0, 1, 0);
            Vector zVector = new Vector(0, 0, 1);

            foreach (CMrPart mrPart in mMrPartList)
            {
                Vector normal = mrPart.mNormal;

                if (!IsNeedTopDim(mrPart))
                {
                    continue;
                }

                double minX = mrPart.GetMinXPoint().X;
                double maxX = mrPart.GetMaxXPoint().X;
                double maxY = mrPart.GetMaxYPoint().Y;

                List<CMrBoltArray> mrBoltArrayList = mrPart.GetBoltArrayList();

                foreach (CMrBoltArray mrBoltArray in mrBoltArrayList)
                {
                    List<Point> maxYPointList=mrBoltArray.GetMaxYPointList();

                    if (minX > 0|| maxX < 0)
                    {
                        mrDimSet.AddRange(maxYPointList);
                        mrDimSet.AddPoint(new Point(0, maxY, 0));
                    }
                    else
                    {
                        mrDimSet.AddRange(maxYPointList);
                        mrDimSet.AddPoint(new Point(0, maxY, 0));
                        mrDimSet.AddPoint(mrPart.GetMinXMaxYPoint());
                        mrDimSet.AddPoint(mrPart.GetMaxXMaxYPoint());
                    }
                }
            }

            if (mrDimSet.Count == 0)
            {
                return;
            }

            mrDimSet.mDimVector = yVector;
            mrDimSet.mDimDistance = 1.5 * CCommonPara.mDefaultDimDistance;

            CreateDimByMrDimSet(mrDimSet);
        }

        /// <summary>
        /// 绘制右侧的标注;
        /// </summary>
        private void DrawXVectorRightDim()
        {
            Vector xVector = new Vector(1, 0, 0);
            Vector yVector = new Vector(0, 1, 0);
            Vector zVector = new Vector(0, 0, 1);

            foreach (CMrPart mrPart in mMrPartList)
            {
                Vector normal = mrPart.mNormal;

                CMrDimSet mrDimSet = new CMrDimSet();

                double maxX = mrPart.GetMaxXPoint().X;

                if (maxX < 0)
                {
                    continue;
                }

                //如果法向与Z轴平行;
                if (CDimTools.GetInstance().IsTwoVectorParallel(normal, zVector)&&mrPart.IsHaveBolt())
                {
                    List<CMrBoltArray> mrBoltArrayList = mrPart.GetBoltArrayList();

                    foreach (CMrBoltArray mrBoltArray in mrBoltArrayList)
                    {
                        List<Point> maxXPointList = mrBoltArray.GetMaxXPointList();
                        mrDimSet.AddRange(maxXPointList);
                        mrDimSet.AddPoint(mrPart.GetMaxYMaxXPoint());
                        mrDimSet.AddPoint(mrPart.GetMinYMaxXPoint());
                    }
                }

                if (mrDimSet.Count == 0)
                {
                    continue;
                }

                mrDimSet.mDimDistance = 1.5 * CCommonPara.mDefaultDimDistance;
                mrDimSet.mDimVector = xVector;

                CreateDimByMrDimSet(mrDimSet);
            }
        }

        /// <summary>
        /// 绘制下侧的标注;
        /// </summary>
        private void DrawXVectorDownDim()
        {
            CMrDimSet mrDimSet = new CMrDimSet();

            Vector xVector = new Vector(1, 0, 0);
            Vector yVector = new Vector(0, 1, 0);
            Vector zVector = new Vector(0, 0, 1);

            foreach (CMrPart mrPart in mMrPartList)
            {
                Vector normal = mrPart.mNormal;

                if (!IsNeedDownDim(mrPart))
                {
                    continue;
                }

                double minY = mrPart.GetMinYPoint().Y;
                double minX = mrPart.GetMinXPoint().X;
                double maxX = mrPart.GetMaxXPoint().X;
                
                List<CMrBoltArray> mrBoltArrayList = mrPart.GetBoltArrayList();

                foreach (CMrBoltArray mrBoltArray in mrBoltArrayList)
                {
                    List<Point> minYPointList = mrBoltArray.GetMinYPointList();

                    if (minX > 0 || maxX < 0)
                    {
                        mrDimSet.AddRange(minYPointList);
                        mrDimSet.AddPoint(new Point(0, minY, 0));
                    }
                }
            }
            if (mrDimSet.Count == 0)
            {
                return;
            }

            mrDimSet.mDimVector = new Vector(0, 1, 0);
            mrDimSet.mDimDistance = 1.2 * CCommonPara.mDefaultDimDistance;

            CreateDimByMrDimSet(mrDimSet);
        }

        /// <summary>
        /// 绘制主梁法向朝Y方向的剖面标注;
        /// </summary>
        private void DrawYVectorSectionDim()
        {
            Vector zVector = new Vector(0, 0, 1);
            Vector yVector = new Vector(0, 1, 0);
            Vector xVector = new Vector(1, 0, 0);

            foreach (CMrPart mrPart in mMrPartList)
            {
                Vector normal =mrPart.mNormal;

                if (CDimTools.GetInstance().IsTwoVectorParallel(normal, new Vector(0, 0, 1))&&mrPart.IsHaveBolt())
                {
                    double minY = mrPart.GetMinYPoint().Y;
                    double maxY = mrPart.GetMaxYPoint().Y;

                    //如果零件在X轴上方或者下方;
                    if (minY > 0 || maxY < 0)
                    {
                        List<CMrBoltArray> mrBoltArrayList = mrPart.GetBoltArrayList();
                        
                        CMrBoltArray mrBoltArray = mrBoltArrayList[0];
                        List<Point> maxYBoltPointList = mrBoltArray.GetMaxYPointList();
                        List<Point> maxXBoltPointList = mrBoltArray.GetMaxXPointList();

                        Vector dimVector = null;

                        if (minY > 0)
                        {
                            dimVector = new Vector(0, 1, 0);
                        }
                        if (maxY < 0)
                        {
                            dimVector = new Vector(0, -1, 0);
                        }

                        //标注零部件的顶侧标注;
                        CMrDimSet mrDimSet = new CMrDimSet();

                        mrDimSet.AddPoint(mrPart.GetMinXMaxYPoint());
                        mrDimSet.AddPoint(mrPart.GetMaxXMaxYPoint());
                        mrDimSet.AddRange(maxYBoltPointList);
                        mrDimSet.mDimDistance = 1.2 * CCommonPara.mDefaultDimDistance;
                        mrDimSet.mDimVector = dimVector;

                        CreateDimByMrDimSet(mrDimSet);

                        //标注零部件的右侧标注;
                        mrDimSet = new CMrDimSet();
                        mrDimSet.AddRange(maxXBoltPointList);
                        mrDimSet.AddPoint(new Point(maxXBoltPointList[0].X, 0, 0));
                        mrDimSet.mDimDistance = 1.5 * CCommonPara.mDefaultDimDistance;
                        mrDimSet.mDimVector = xVector;

                        CreateDimByMrDimSet(mrDimSet);
                    }
                    else if (minY < 0 && maxY > 0)
                    {
                        List<CMrBoltArray> mrBoltArrayList = mrPart.GetBoltArrayList();

                        CMrBoltArray mrBoltArray = mrBoltArrayList[0];
                        List<Point> maxYBoltPointList = mrBoltArray.GetMaxYPointList();
                        List<Point> maxXBoltPointList = mrBoltArray.GetMaxXPointList();

                        //标注零部件的顶侧标注;
                        CMrDimSet mrDimSet = new CMrDimSet();

                        mrDimSet.AddRange(maxYBoltPointList);
                        mrDimSet.AddPoint(mrPart.GetMinXMaxYPoint());
                        mrDimSet.AddPoint(mrPart.GetMaxXMaxYPoint());
                        mrDimSet.mDimDistance = 1.2 * CCommonPara.mDefaultDimDistance;
                        mrDimSet.mDimVector = yVector;

                        CreateDimByMrDimSet(mrDimSet);

                        //标注零部件的右侧标注;
                        mrDimSet = new CMrDimSet();
                        mrDimSet.AddPoint(mrPart.GetMaxXMaxYPoint());
                        mrDimSet.AddPoint(mrPart.GetMaxXMinYPoint());
                        mrDimSet.AddRange(maxXBoltPointList);
                        mrDimSet.AddPoint(new Point(maxXBoltPointList[0].X, 0, 0));
                        mrDimSet.mDimDistance = 1.2 * CCommonPara.mDefaultDimDistance;
                        mrDimSet.mDimVector = xVector;

                        CreateDimByMrDimSet(mrDimSet);
                    }
                }
            }
        }

        /// <summary>
        /// 绘制主梁法向与X轴平行时的零件标记;
        /// </summary>
        private void DrawXVectorMark()
        {
            Vector zVector = new Vector(0, 0, 1);

            DrawingHandler drawingHandler = new DrawingHandler();
            TSD.UI.DrawingObjectSelector DS = drawingHandler.GetDrawingObjectSelector();

            foreach (CMrPart mrPart in mMrPartList)
            {
                Vector normal = mrPart.mNormal;

                if (!(CDimTools.GetInstance().IsTwoVectorParallel(normal, zVector) && mrPart.IsHaveBolt()))
                {
                    continue;
                }

                DS.SelectObject(mrPart.mPartInDrawing);

                //绘制螺钉标记;
                List<CMrBoltArray> mrBoltArrayList = mrPart.GetBoltArrayList();

                foreach (CMrBoltArray mrBoltArray in mrBoltArrayList)
                {
                    if (CDimTools.GetInstance().IsTwoVectorParallel(mrBoltArray.mNormal, zVector))
                    {
                        DS.SelectObject(mrBoltArray.mBoltInDrawing);
                    }
                }
            }
            CDimTools.GetInstance().DrawMarkByMacro();
        }

        /// <summary>
        /// 绘制主梁法向与Y轴平行时的零件标记;
        /// </summary>
        private void DrawYVectorMark()
        {
            DrawingHandler drawingHandler = new DrawingHandler();
            TSD.UI.DrawingObjectSelector DS = drawingHandler.GetDrawingObjectSelector();

            Vector xVector = new Vector(1, 0, 0);
            Vector yVector = new Vector(0, 1, 0);
            Vector zVector = new Vector(0, 0, 1);

            //1.绘制带螺钉的板的零件标记;
            foreach (CMrPart mrPart in mMrPartList)
            {
                double minY = mrPart.GetMinYPoint().Y;
                double maxY = mrPart.GetMaxYPoint().Y;

                Vector normal = mrPart.mNormal;

                //如果法向与Z轴平行并且有螺钉;
                if (CDimTools.GetInstance().IsTwoVectorParallel(normal, zVector) && mrPart.IsHaveBolt())
                {
                    DS.SelectObject(mrPart.mPartInDrawing);

                    //2.绘制螺钉标记;
                    List<CMrBoltArray> mrBoltArrayList = mrPart.GetBoltArrayList();

                    foreach (CMrBoltArray mrBoltArray in mrBoltArrayList)
                    {
                        if (CDimTools.GetInstance().IsTwoVectorParallel(mrBoltArray.mNormal, zVector))
                        {
                            DS.SelectObject(mrBoltArray.mBoltInDrawing);
                        }
                    }
                }
                //如果法向与Z轴平行没有螺钉（暂且判定为凸起的螺帽）;
                else if (CDimTools.GetInstance().IsTwoVectorParallel(normal, zVector) && !mrPart.IsHaveBolt())
                {
                    if (minY > 0 || maxY < 0)
                    {
                        DS.SelectObject(mrPart.mPartInDrawing);
                    }
                }
                //如果法向与Y轴平行;
                else if (CDimTools.GetInstance().IsTwoVectorParallel(normal, yVector))
                {
                    if (minY > 0 || maxY < 0)
                    {
                        DS.SelectObject(mrPart.mPartInDrawing);
                    }
                }
            }

            CDimTools.GetInstance().DrawMarkByMacro();
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
