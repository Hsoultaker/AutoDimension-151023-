using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using Tekla.Structures.Drawing;
using Tekla.Structures.Model;
using Tekla.Structures.Geometry3d;
using Tekla.Structures;

using TSD = Tekla.Structures.Drawing;
using TSM = Tekla.Structures.Model;
using AutoDimension.Entity;
using System.Windows.Forms;

namespace AutoDimension
{
    public class CCylinderSectionView:CView
    {
        /// <summary>
        /// 螺钉组的X坐标数组与螺钉组自身的数据字典,判断螺钉组的上下对称性;
        /// </summary>
        private Dictionary<String, CMrBoltArray> mXDicAttributeBoltArray = new Dictionary<String, CMrBoltArray>();

        /// <summary>
        /// 螺钉组的Y坐标数组与螺钉组自身的数据字典,判断螺钉组的左右对称性;
        /// </summary>
        private Dictionary<String, CMrBoltArray> mYDicAttributeBoltArray = new Dictionary<string, CMrBoltArray>();

        /// <summary>
        /// 数据字典记录已经标记的螺钉组的点;
        /// </summary>
        private Dictionary<Point, bool> mDicBoltPoint = new Dictionary<Point, bool>();

        /// <summary>
        /// 构造函数;
        /// </summary>
        /// <param name="viewBase"></param>
        /// <param name="model"></param>
        public CCylinderSectionView(TSD.View viewBase, Model model)
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

            mDicBoltPoint.Clear();
            mYDicAttributeBoltArray.Clear();
            mXDicAttributeBoltArray.Clear();

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
                    InitMrBoltArray(mrBoltArray);
                }
            }

            CMrMarkManager.GetInstance().Clear();
        }

        /// <summary>
        /// 初始化螺钉组,主要构造螺钉组的对称性;
        /// </summary>
        /// <param name="mrBoltArray"></param>
        protected void InitMrBoltArray(CMrBoltArray mrBoltArray)
        {
            Vector zVector = new Vector(0, 0, 1);

            if (!CDimTools.GetInstance().IsTwoVectorParallel(zVector, mrBoltArray.mNormal))
            {
                return;
            }

            //构造螺钉组的对称性;
            List<Point> pointList = mrBoltArray.GetBoltPointList();

            String strAttributeX = null;
            String strAttributeY = null;

            foreach (Point point in pointList)
            {
                strAttributeX = strAttributeX + "_" + point.X.ToString();
                strAttributeY = strAttributeY + "_" + point.Y.ToString();
            }

            if(mrBoltArray.mBoltArrayShapeType==MrBoltArrayShapeType.ARRAY)
            {
                if (mXDicAttributeBoltArray.ContainsKey(strAttributeX))
                {
                    CMrBoltArray symMrBoltArray = mXDicAttributeBoltArray[strAttributeX];
                    symMrBoltArray.GetMrBoltArrayInfo().mXSymBoltArray = mrBoltArray;
                    mrBoltArray.GetMrBoltArrayInfo().mXSymBoltArray = symMrBoltArray;
                }
                else
                {
                    mXDicAttributeBoltArray.Add(strAttributeX, mrBoltArray);
                }
            }
            else if(mrBoltArray.mBoltArrayShapeType==MrBoltArrayShapeType.ARRAY)
            {
                if (mYDicAttributeBoltArray.ContainsKey(strAttributeY))
                {
                    CMrBoltArray symMrBoltArray = mYDicAttributeBoltArray[strAttributeY];
                    symMrBoltArray.GetMrBoltArrayInfo().mYSymBoltArray = mrBoltArray;
                    mrBoltArray.GetMrBoltArrayInfo().mYSymBoltArray = symMrBoltArray;
                }
                else
                {
                    mYDicAttributeBoltArray.Add(strAttributeY, mrBoltArray);
                }
            }             
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
                    DrawPartMark();
                    DrawSectionDim();
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
            Vector xVector = new Vector(1, 0, 0);
            Vector zVector = new Vector(0, 0, 1);

            DrawingHandler drawingHandler = new DrawingHandler();
            TSD.UI.DrawingObjectSelector DS = drawingHandler.GetDrawingObjectSelector();

            foreach (CMrPart mrPart in mMrPartList)
            {
                if (!IsInvalidPart(mrPart))
                {
                    continue;
                }

                Point minXPoint = mrPart.GetMinXPoint();
                Point maxXPoint = mrPart.GetMaxXPoint();
                Point minYPoint = mrPart.GetMinYPoint();
                Point maxYPoint = mrPart.GetMaxYPoint();

                //1.如果零件在坐标系的左侧;
                if (maxXPoint.X < CCommonPara.mDblError || minXPoint.X > CCommonPara.mDblError)
                {
                    DS.SelectObject(mrPart.mPartInDrawing);

                    //标注螺钉组的零件标记;
                    List<CMrBoltArray> mrBoltArrayList = mrPart.GetBoltArrayList();

                    foreach (CMrBoltArray mrBoltArray in mMrBoltArrayList)
                    {
                        Vector boltNormal = mrBoltArray.mNormal;

                        if (!CDimTools.GetInstance().IsTwoVectorParallel(boltNormal, zVector))
                        {
                            continue;
                        }
                        DS.SelectObject(mrBoltArray.mBoltInDrawing);
                    }
                }
                //3.如果零件在坐标系的中间;
                else
                {
                   DS.SelectObject(mrPart.mPartInDrawing);

                    //标注螺钉组的零件标记;
                    List<CMrBoltArray> mrBoltArrayList = mrPart.GetBoltArrayList();

                    foreach (CMrBoltArray mrBoltArray in mrBoltArrayList)
                    {
                        Vector boltNormal = mrBoltArray.mNormal;

                        if (CDimTools.GetInstance().IsTwoVectorParallel(boltNormal, xVector))
                        {
                            continue;
                        }

                        DS.SelectObject(mrBoltArray.mBoltInDrawing);
                    }
                }
            }

            CDimTools.GetInstance().DrawMarkByMacro();
        }

        /// <summary>
        /// 绘制剖面标注;
        /// </summary>
        private void DrawSectionDim()
        {
            //判断主梁在剖视图中的法向,主要是考虑与X方向和Y方向平行的两个方向;
            Vector normal = mMainBeam.mNormal;

            Vector xVector = new Vector(1, 0, 0);
            Vector yVector = new Vector(0, 1, 0);

            if (CDimTools.GetInstance().IsTwoVectorParallel(normal, yVector))
            {
                DrawYVectorSectionDim();
            }
        }

        /// <summary>
        /// 绘制主零件的法向与X平行时的标注;
        /// </summary>
        private void DrawYVectorSectionDim()
        {
            DrawTopPartDim();
            DrawRightPartDim();
            DrawLeftPartDim();
            DrawBottomPartDim();
        }

        /// <summary>
        /// 标注上侧的零部件;
        /// </summary>
        private void DrawTopPartDim()
        {
            List<Point> pointList = new List<Point>();

            Vector xVector = new Vector(1, 0, 0);
            Vector yVector = new Vector(0, 1, 0);
            Vector zVector = new Vector(0, 0, 1);

            double mainBeamMinX = mMainBeam.GetMinXPoint().X;
            double mainBeamMaxX = mMainBeam.GetMaxXPoint().X;

            bool bFlagLeft = false;
            bool bFlagRight = false;

            foreach (CMrPart mrPart in mMrPartList)
            {
                if (!IsInvalidPart(mrPart))
                {
                    continue;
                }

                Point minYPoint = mrPart.GetMinYPoint();
                Point maxYPoint = mrPart.GetMaxYPoint();

                //0.如果零件在下侧则直接返回;
                if (maxYPoint.Y < CCommonPara.mDblError)
                {
                    continue;
                }

                //1.如果存在板的法向与x轴平行;
                if (CDimTools.GetInstance().IsTwoVectorParallel(mrPart.mNormal, xVector))
                {
                    Point maxYminXPoint = mrPart.GetMaxYMinXPoint();
                    Point maxYmaxXPoint = mrPart.GetMaxYMaxXPoint();

                    if (Math.Abs(maxYminXPoint.X-mainBeamMinX) < CCommonPara.mDblError)
                    {
                        bFlagLeft = true;
                        pointList.Add(maxYminXPoint);
                    }
                    else if (Math.Abs(maxYmaxXPoint.X - mainBeamMaxX) < CCommonPara.mDblError)
                    {
                        bFlagRight = true;
                        pointList.Add(maxYmaxXPoint);
                    }
                    else
                    {
                        pointList.Add(maxYminXPoint);
                    }
                }
                //2.如果板的法向与Z轴或Y轴平行;
                if (CDimTools.GetInstance().IsTwoVectorParallel(mrPart.mNormal, zVector)
                    || CDimTools.GetInstance().IsTwoVectorParallel(mrPart.mNormal, yVector))
                {
                    if (!mrPart.IsHaveBolt())
                    {
                        continue;
                    }

                    List<CMrBoltArray> mrBoltArrayList = mrPart.GetBoltArrayList();

                    foreach (CMrBoltArray mrBoltArray in mrBoltArrayList)
                    {
                        Vector boltNormal = mrBoltArray.mNormal;

                        if (!CDimTools.GetInstance().IsTwoVectorParallel(boltNormal, zVector))
                        {
                            continue;
                        }

                        List<Point> boltPointList = mrBoltArray.GetMaxYPointList();

                        if(boltPointList==null||boltPointList.Count==0)
                        {
                            continue;
                        }

                        pointList.AddRange(boltPointList);

                        mrBoltArray.GetMrBoltArrayInfo().mbIsUpDim = true;
                    }
                }
            }

            if(pointList.Count==0)
            {
                return;
            }

            //3.增加柱两边的端点;
            double mainBeamMaxY = mMainBeam.GetMaxYPoint().Y;

            if(bFlagLeft==false)
            {
                pointList.Add(new Point(mainBeamMinX, mainBeamMaxY, 0));
            }
            if(bFlagRight==false)
            {
                pointList.Add(new Point(mainBeamMaxX, mainBeamMaxY, 0));
            }

            CMrDimSet mrDimSet = new CMrDimSet();
            mrDimSet.AddRange(pointList);

            pointList = mrDimSet.GetDimPointList();

            Point viewMaxPoint = mViewBase.RestrictionBox.MaxPoint;
            double viewMaxY = viewMaxPoint.Y;

            Vector dimVector = new Vector(0, 1, 0);
            double distance = Math.Abs(viewMaxY - pointList[0].Y) + CCommonPara.mDefaultDimDistance;

            mrDimSet.mDimVector = dimVector;
            mrDimSet.mDimDistance = distance;

            CreateDimByMrDimSet(mrDimSet);
        }

        /// <summary>
        /// 标注右侧的零部件;
        /// </summary>
        private void DrawRightPartDim()
        {
            List<Point> pointList = new List<Point>();

            Vector xVector = new Vector(1, 0, 0);
            Vector yVector = new Vector(0, 1, 0);
            Vector zVector = new Vector(0, 0, 1);

            bool bFlag = false;

            foreach (CMrPart mrPart in mMrPartList)
            {
                if (!IsInvalidPart(mrPart))
                {
                    continue;
                }

                Point minXPoint = mrPart.GetMinXPoint();
                Point maxXPoint = mrPart.GetMaxXPoint();

                //0.如果零件在左侧则直接返回;
                if (maxXPoint.X < CCommonPara.mDblError)
                {
                    continue;
                }

                //1.如果存在板的法向与y轴平行;
                if (CDimTools.GetInstance().IsTwoVectorParallel(mrPart.mNormal, yVector))
                {
                    //有些零件的法向朝Y，但是上面的螺钉是朝Z轴的，需要排除掉;
                    List<CMrBoltArray> mrBoltArrayList = mrPart.GetBoltArrayList();

                    if(mrBoltArrayList != null && mrBoltArrayList.Count > 0)
                    {
                        Vector boltNormal = mrBoltArrayList[0].mNormal;

                        if(CDimTools.GetInstance().IsTwoVectorParallel(boltNormal,zVector))
                        {
                            
                        }
                    }
                    else
                    {
                        //如果有水平的板的刚好水平放置在柱的中间,则取水平板的中间点即可;
                        Point maxXminYPoint = mrPart.GetMaxXMinYPoint();
                        Point maxXmaxYPoint = mrPart.GetMaxXMaxYPoint();

                        if (Math.Abs(maxXminYPoint.Y) < 10 && Math.Abs(maxXmaxYPoint.Y) < 10 &&
                            Math.Abs(maxXminYPoint.Y + maxXmaxYPoint.Y) < CCommonPara.mDblError)
                        {
                            Point newPoint = new Point(0, 0, 0);
                            newPoint.Y = 0;
                            newPoint.X = maxXminYPoint.X;
                            pointList.Add(newPoint);
                            bFlag = true;
                        }
                        else
                        {
                            pointList.Add(maxXminYPoint);
                        }
                    }
                }
                //2.如果板的法向与Z轴平行;
                if (CDimTools.GetInstance().IsTwoVectorParallel(mrPart.mNormal, zVector)
                    || CDimTools.GetInstance().IsTwoVectorParallel(mrPart.mNormal, yVector))
                {
                    if (!mrPart.IsHaveBolt())
                    {
                        continue;
                    }

                    List<CMrBoltArray> mrBoltArrayList = mrPart.GetBoltArrayList();
                    foreach (CMrBoltArray mrBoltArray in mrBoltArrayList)
                    {
                        Vector boltNormal = mrBoltArray.mNormal;

                        if (!CDimTools.GetInstance().IsTwoVectorParallel(boltNormal, zVector))
                        {
                            continue;
                        }

                        List<Point> boltPointList = mrBoltArray.GetMaxXPointList();
                        
                        if (boltPointList == null || boltPointList.Count == 0 )
                        {
                            continue;
                        }

                        pointList.AddRange(boltPointList);

                        //标志该螺钉在右侧已经标注了;
                        mrBoltArray.GetMrBoltArrayInfo().mbIsRightDim = true;
                    }
                }
            }

            if(pointList.Count==0)
            {
                return;
            }

            //3.如果没有设置水平中间板的点,则自动设置改点;
            if (false == bFlag && pointList.Count > 0)
            {
                double mainBeamMaxX = mMainBeam.GetMaxXPoint().X;
                pointList.Add(new Point(mainBeamMaxX, 0, 0));
            }

            CMrDimSet mrDimSet = new CMrDimSet();
            mrDimSet.AddRange(pointList);

            pointList = mrDimSet.GetDimPointList();

            Point viewMaxPoint = mViewBase.RestrictionBox.MaxPoint;
            double viewMaxX = viewMaxPoint.X;

            Vector dimVector = new Vector(1, 0, 0);
            double distance = Math.Abs(viewMaxX - pointList[0].X) + CCommonPara.mDefaultDimDistance;

            mrDimSet.mDimVector = dimVector;
            mrDimSet.mDimDistance = distance;

            CreateDimByMrDimSet(mrDimSet);
        }

        /// <summary>
        /// 标注左侧的零部件;
        /// </summary>
        private void DrawLeftPartDim()
        {
            List<Point> pointList = new List<Point>();

            Vector xVector = new Vector(1, 0, 0);
            Vector yVector = new Vector(0, 1, 0);
            Vector zVector = new Vector(0, 0, 1);

            bool bFlag = false;

            foreach (CMrPart mrPart in mMrPartList)
            {
                if (!IsInvalidPart(mrPart))
                {
                    continue;
                }

                Point minXPoint = mrPart.GetMinXPoint();
                Point maxXPoint = mrPart.GetMaxXPoint();

                //0.如果零件在右侧则直接返回;
                if (minXPoint.X > CCommonPara.mDblError)
                {
                    continue;
                }

                //1.如果存在板的法向与y轴平行;
                if (CDimTools.GetInstance().IsTwoVectorParallel(mrPart.mNormal, yVector))
                {
                    //有些零件的法向朝Y，但是上面的螺钉是朝Z轴的，需要排除掉;
                    List<CMrBoltArray> mrBoltArrayList = mrPart.GetBoltArrayList();

                    if (mrBoltArrayList != null && mrBoltArrayList.Count > 0)
                    {
                        Vector boltNormal = mrBoltArrayList[0].mNormal;

                        if (CDimTools.GetInstance().IsTwoVectorParallel(boltNormal, zVector))
                        {
                           
                        }
                    }
                    else
                    {
                        //如果有水平的板的刚好水平放置在柱的中间,则取水平板的中间点即可;
                        Point minXminYPoint = mrPart.GetMinXMinYPoint();
                        Point minXmaxYPoint = mrPart.GetMinXMaxYPoint();

                        if (Math.Abs(minXmaxYPoint.Y) < 10 && Math.Abs(minXminYPoint.Y) < 10 &&
                            Math.Abs(minXmaxYPoint.Y + minXminYPoint.Y) < CCommonPara.mDblError)
                        {
                            Point newPoint = new Point(0, 0, 0);
                            newPoint.Y = 0;
                            newPoint.X = minXminYPoint.X;
                            pointList.Add(newPoint);
                            bFlag = true;
                        }
                        else
                        {
                            pointList.Add(minXminYPoint);
                        }
                    }
                }
                //2.如果板的法向与Z轴平行;
                if (CDimTools.GetInstance().IsTwoVectorParallel(mrPart.mNormal, zVector)
                    || CDimTools.GetInstance().IsTwoVectorParallel(mrPart.mNormal, yVector))
                {
                    if (!mrPart.IsHaveBolt())
                    {
                        continue;
                    }

                    List<CMrBoltArray> mrBoltArrayList = mrPart.GetBoltArrayList();
                    foreach (CMrBoltArray mrBoltArray in mrBoltArrayList)
                    {
                        //如果该螺钉组的对称螺钉已经标注则不进行标注;
                        CMrBoltArray mYSymBoltArray = mrBoltArray.GetMrBoltArrayInfo().mYSymBoltArray;

                        if(mYSymBoltArray!=null && mYSymBoltArray.GetMrBoltArrayInfo().mbIsRightDim==true)
                        {
                            continue;
                        }

                        Vector boltNormal = mrBoltArray.mNormal;

                        if (!CDimTools.GetInstance().IsTwoVectorParallel(boltNormal, zVector))
                        {
                            continue;
                        }

                        List<Point> boltPointList = mrBoltArray.GetMinXPointList();

                        if (boltPointList == null || boltPointList.Count == 0)
                        {
                            continue;
                        }
                        pointList.AddRange(boltPointList);
                    }
                }
            }

            if (pointList.Count == 0)
            {
                return;
            }

            //3.如果没有设置水平中间板的点,则自动设置改点;
            if (false == bFlag && pointList.Count > 0)
            {
                double mainBeamMinX = mMainBeam.GetMinXPoint().X;
                pointList.Add(new Point(mainBeamMinX, 0, 0));
            }

            CMrDimSet mrDimSet = new CMrDimSet();

            mrDimSet.AddRange(pointList);

            pointList = mrDimSet.GetDimPointList();

            Point viewMinPoint = mViewBase.RestrictionBox.MinPoint;
            double viewMinX = viewMinPoint.X;

            Vector dimVector = new Vector(-1, 0, 0);
            double distance = Math.Abs(viewMinX - pointList[0].X) + CCommonPara.mDefaultDimDistance;

            mrDimSet.mDimVector = dimVector;
            mrDimSet.mDimDistance = distance;

            CreateDimByMrDimSet(mrDimSet);
        }

        /// <summary>
        /// 标注下方的零部件;
        /// </summary>
        private void DrawBottomPartDim()
        {
            List<Point> pointList = new List<Point>();

            Vector xVector = new Vector(1, 0, 0);
            Vector yVector = new Vector(0, 1, 0);
            Vector zVector = new Vector(0, 0, 1);

            bool bFlagLeft = false;
            bool bFlagRight = false;

            double mainBeamMinX = mMainBeam.GetMinXPoint().X;
            double mainBeamMaxX = mMainBeam.GetMaxXPoint().X;

            foreach (CMrPart mrPart in mMrPartList)
            {
                if (!IsInvalidPart(mrPart))
                {
                    continue;
                }

                Point minYPoint = mrPart.GetMinYPoint();
                Point maxYPoint = mrPart.GetMaxYPoint();

                //0.如果零件在上侧则直接返回;
                if (minYPoint.Y > CCommonPara.mDblError)
                {
                    continue;
                }

                //1.如果存在板的法向与x轴平行;
                if (CDimTools.GetInstance().IsTwoVectorParallel(mrPart.mNormal, xVector))
                {
                    Point minYminXPoint = mrPart.GetMinYMinXPoint();
                    Point minYmaxXPoint = mrPart.GetMinYMaxXPoint();

                    if (Math.Abs(minYminXPoint.X - mainBeamMinX) < CCommonPara.mDblError)
                    {
                        bFlagLeft = true;
                        pointList.Add(minYminXPoint);
                    }
                    else if (Math.Abs(minYmaxXPoint.X - mainBeamMaxX) < CCommonPara.mDblError)
                    {
                        bFlagRight = true;
                        pointList.Add(minYmaxXPoint);
                    }
                    else
                    {
                        pointList.Add(minYminXPoint);
                    }
                }
                //2.如果板的法向与Z轴平行;
                if (CDimTools.GetInstance().IsTwoVectorParallel(mrPart.mNormal, zVector)
                    || CDimTools.GetInstance().IsTwoVectorParallel(mrPart.mNormal, yVector))
                {
                    if (!mrPart.IsHaveBolt())
                    {
                        continue;
                    }

                    List<CMrBoltArray> mrBoltArrayList = mrPart.GetBoltArrayList();
                    foreach (CMrBoltArray mrBoltArray in mrBoltArrayList)
                    {
                        //如果该螺钉组的对称螺钉组已经标注，则不进行标注;
                        CMrBoltArray mXSymBoltArray = mrBoltArray.GetMrBoltArrayInfo().mXSymBoltArray;

                        if (mXSymBoltArray!=null && mXSymBoltArray.GetMrBoltArrayInfo().mbIsUpDim == true)
                        {
                            continue;
                        }

                        Vector boltNormal = mrBoltArray.mNormal;

                        if (!CDimTools.GetInstance().IsTwoVectorParallel(boltNormal, zVector))
                        {
                            continue;
                        }

                        List<Point> boltPointList = mrBoltArray.GetMinYPointList();

                        if (boltPointList == null || boltPointList.Count == 0 )
                        {
                            continue;
                        }

                        pointList.AddRange(boltPointList);
                    }
                }
            }

            if (pointList.Count == 0)
            {
                return;
            }

            //3.增加柱两边的端点;
            double mainBeamMinY = mMainBeam.GetMinYPoint().Y;

            if(bFlagLeft==false)
            {
                pointList.Add(new Point(mainBeamMinX, mainBeamMinY, 0));
            }
            if(bFlagRight==false)
            {
                pointList.Add(new Point(mainBeamMaxX, mainBeamMinY, 0));
            }

            CMrDimSet mrDimSet = new CMrDimSet();
            mrDimSet.AddRange(pointList);

            pointList = mrDimSet.GetDimPointList();

            Point viewMinPoint = mViewBase.RestrictionBox.MinPoint;
            double viewMinY = viewMinPoint.Y;

            Vector dimVector = new Vector(0, -1, 0);
            double distance = Math.Abs(viewMinY - pointList[0].Y) + CCommonPara.mDefaultDimDistance;

            mrDimSet.mDimVector = dimVector;
            mrDimSet.mDimDistance = distance;

            CreateDimByMrDimSet(mrDimSet);
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

        /// <summary>
        /// 判断零部件是否无效,主要是判断零部件的z坐标与柱子的z坐标的关系;
        /// </summary>
        /// <param name="mrPart"></param>
        /// <returns></returns>
        private bool IsInvalidPart(CMrPart mrPart)
        {
            double mainBeamMinZ = mMainBeam.GetMinZPoint().Z;
            double mainBeamMaxZ = mMainBeam.GetMaxZPoint().Z;

            double mrPartMinZ = mrPart.GetMinZPoint().Z;
            double mrPartMaxZ = mrPart.GetMaxZPoint().Z;

            if(CDimTools.GetInstance().CompareTwoDoubleValue(mrPartMaxZ,mainBeamMinZ)<0)
            {
                return false;
            }
            if(CDimTools.GetInstance().CompareTwoDoubleValue(mrPartMinZ,mainBeamMaxZ)>0)
            {
                return false;
            }
            return true;
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

            if (CDimTools.GetInstance().CompareTwoDoubleValue(dblX, viewMinX) < 0)
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
        /// 判断该螺钉组的是否需要添加零件标记;
        /// </summary>
        /// <param name="boltPoint"></param>
        /// <returns></returns>
        private bool IsBoltNeedDrawMark(Point boltPoint)
        {
            if(mDicBoltPoint.ContainsKey(boltPoint))
            {
                return false;
            }
            else
            {
                mDicBoltPoint.Add(boltPoint,true);

                return true;
            }
        }
    }
}
