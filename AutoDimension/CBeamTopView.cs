using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoDimension.Entity;
using Tekla.Structures.Drawing;
using Tekla.Structures.Model;
using Tekla.Structures.Geometry3d;
using Tekla.Structures;
using Tekla.Structures.Model.Operations;
using TSD = Tekla.Structures.Drawing;
using TSM = Tekla.Structures.Model;
using System.Windows.Forms;
using System.Collections;
using System.Threading;
using System.IO;

namespace AutoDimension
{
    /// <summary>
    /// 顶视图对象;
    /// </summary>
    public class CBeamTopView : CView
    {
        /// <summary>
        /// 部件的属性与部件自身的数据字典,主要是来判断上下的对称性;
        /// </summary>
        private Dictionary<String, CMrPart> mDicAttributePart=new Dictionary<String,CMrPart>();

        /// <summary>
        /// 螺钉组的属性与螺钉组自身的数据字典,判断螺钉组的对称性;
        /// </summary>
        private Dictionary<String, CMrBoltArray> mDicAttributeBoltArray = new Dictionary<String, CMrBoltArray>();

        /// <summary>
        /// 螺钉组的需要在Y方向上标注的数据字典,去掉重复标注;
        /// </summary>
        private Dictionary<String, bool> mDicYBoltDimPoints = new Dictionary<String, bool>();

        /// <summary>
        /// 零件标记时需要去掉重复标注的点;
        /// </summary>
        private Dictionary<String, bool> mDicMarkDimPoints = new Dictionary<String, bool>();

        /// <summary>
        /// 向上标注的第一个点;
        /// </summary>
        private Point mUpDimFirstPoint = new Point();

        /// <summary>
        /// 第一个向上标注的距离;
        /// </summary>
        private double mUpDimDistance = 0.0;

        /// <summary>
        /// 向下标注的第一个点;
        /// </summary>
        private Point mDownDimFirstPoint = new Point();

        /// <summary>
        /// 第一个向下标注的距离;
        /// </summary>
        private double mDownDimDistance = 0.0;

        /// <summary>
        /// 构造函数;
        /// </summary>
        /// <param name="viewBase"></param>
        /// <param name="model"></param>
        public CBeamTopView(TSD.View viewBase, Model model): base(viewBase)
        {
            mViewBase = viewBase;
            mModel = model;
        }

        /// <summary>
        /// 初始化零部件,构造出上下部件的对称性,只考虑法向与Y轴平行的板的对称性;
        /// </summary>
        /// <param name="mrPart"></param>
        private void InitMrPart(CMrPart mrPart)
        {
            Vector normal = mrPart.mNormal;

            if(!CDimTools.GetInstance().IsTwoVectorParallel(normal,new Vector(1,0,0)))
            {
                return;
            }
            
            Point ptLeftBottom = mrPart.mLeftBottomPoint;
            Point ptRightBottom = mrPart.mRightBottomPoint;
            Point ptLeftTop = mrPart.mLeftTopPoint;
            Point ptRightTop = mrPart.mRightTopPoint;
            
            normal.X = Math.Abs(normal.X);
            normal.Y = Math.Abs(normal.Y);
            normal.Z = Math.Abs(normal.Z);

            String strAttribute1 = ((int)ptLeftBottom.X).ToString() + "_" + ((int)ptRightBottom.X).ToString() + "_" + normal.ToString();

            if (mDicAttributePart.ContainsKey(strAttribute1))
            {
                CMrPart symPart = mDicAttributePart[strAttribute1];
                mrPart.GetBeamTopViewInfo().mSymPart = symPart;
                symPart.GetBeamTopViewInfo().mSymPart = mrPart;
            }
            else
            {
                mDicAttributePart.Add(strAttribute1, mrPart);
            }
        }

        /// <summary>
        /// 初始化螺钉组;
        /// </summary>
        /// <param name="mrBoltArray"></param>
        protected void InitMrBoltArray(CMrBoltArray mrBoltArray)
        {
            Vector zVector = new Vector(0, 0, 1);

            if (!CDimTools.GetInstance().IsTwoVectorParallel(zVector, mrBoltArray.mNormal))
            {
                return;
            }

            //构造螺钉组的对称性;螺钉组的堆成都是上下对称;
            List<Point> pointList=mrBoltArray.GetBoltPointList();
          
            String strAttributeX = null;
            
            foreach (Point point in pointList)
            {
                strAttributeX = strAttributeX + "_" + ((int)point.X).ToString() + "_"+((int)point.Z).ToString();
            }
            if (mDicAttributeBoltArray.ContainsKey(strAttributeX))
            {
                CMrBoltArray symMrBoltArray = mDicAttributeBoltArray[strAttributeX];

                symMrBoltArray.GetMrBoltArrayInfo().mXSymBoltArray = mrBoltArray;
                mrBoltArray.GetMrBoltArrayInfo().mXSymBoltArray = symMrBoltArray;
            }
            else
            {
                mDicAttributeBoltArray.Add(strAttributeX, mrBoltArray);
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
                
                if(CMrMainBeam.GetInstance().mPartInModel.Identifier.GUID==partInModel.Identifier.GUID)
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
                InitMrPart(mrPart);

                mrPart.GetBeamTopViewInfo().InitMrPartTopViewInfo();
               
                //2.获取部件中的所有螺钉组;
                List<BoltArray> boltArrayList = dimTools.GetAllBoltArray(partInModel);
                
                foreach (BoltArray boltArray in boltArrayList)
                {
                    TSD.Bolt boltInDrawing = dicIdentifierBolt[boltArray.Identifier];

                    CMrBoltArray mrBoltArray = new CMrBoltArray(boltArray, boltInDrawing);
                    dimTools.InitMrBoltArray(boltArray, mViewBase, mrBoltArray);
                    InitMrBoltArray(mrBoltArray);
                    mrPart.AppendMrBoltArray(mrBoltArray);
                }
                dimTools.UpdateViewBox(mrPart);
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
#if     DEBUG
                //采用总尺寸标注;
                if (CBeamDimSetting.GetInstance().mTopViewSetting.mDimOverallSize == true)
                {
                    DrawCuttingDim();
                    DrawObliquePartDim();
                    DrawPartBoltDimY();
                    DrawMainPartBoltDimY();
                    DrawAllPartUpDimX();
                    DrawAllPartDownDimX();
                    DrawMainPartLengthDimX();
                    DrawWorkPointToMainPartDimX();
                    DrawWorkPointToWorkPointDimX();
                    DrawPartMark();
                    DrawMainPartBoltMark();
                }
                //采用分尺寸标注;
                else if (CBeamDimSetting.GetInstance().mTopViewSetting.mDimSize == true)
                {
                    DrawCuttingDim();
                    DrawObliquePartDim();
                    DrawPartBoltDimY();
                    DrawMainPartBoltDimY();
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
                    if (CBeamDimSetting.GetInstance().mTopViewSetting.mDimOverallSize == true)
                    {
                        DrawCuttingDim();
                        DrawObliquePartDim();
                        DrawPartBoltDimY();
                        DrawMainPartBoltDimY();
                        DrawAllPartUpDimX();
                        DrawAllPartDownDimX();
                        DrawMainPartLengthDimX();
                        DrawWorkPointToMainPartDimX();
                        DrawWorkPointToWorkPointDimX();
                        DrawPartMark();
                        DrawMainPartBoltMark();
                    }
                    //采用分尺寸标注;
                    else if (CBeamDimSetting.GetInstance().mTopViewSetting.mDimSize == true)
                    {
                        DrawCuttingDim();
                        DrawObliquePartDim();
                        DrawPartBoltDimY();
                        DrawMainPartBoltDimY();
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
        /// 绘制所有零件的上方标注;
        /// </summary>
        public void DrawAllPartUpDimX()
        {
            //移除主梁;
            mMrPartList.Remove(mMainBeam);

            List<Point> upDimPointList = new List<Point>();

            bool bNeedUpDim = false;

            //1.遍历获得所有零件的向上标注的点;
            foreach (CMrPart mrPart in mMrPartList)
            {
                CMrDimSet partDimSet = mrPart.GetBeamTopViewInfo().GetPartUpDimSet();

                if (partDimSet != null && partDimSet.Count > 0)
                {
                    bNeedUpDim = true;
                    upDimPointList.AddRange(partDimSet.GetDimPointList());
                }
            }

            CBeamTopViewSetting beamTopViewSetting = CBeamDimSetting.GetInstance().mTopViewSetting;
            
            //2.判断主梁的螺栓是否需要标注;
            bool bNeedMainBeamBoltDim = beamTopViewSetting.FindDimValueByName(CBeamTopViewSetting.mstrBolt);
            
            if (bNeedMainBeamBoltDim)
            {
                CMrDimSet mrDimSet= GetMainPartBoltUpDimX();
                upDimPointList.AddRange(mrDimSet.GetDimPointList());
            }
            if (bNeedUpDim == false && bNeedMainBeamBoltDim == false)
            {
                return;
            }

            //3.默认把主梁的左右最小最大值点加入到链表中;
            Point minXPoint = mMainBeam.GetMinXPoint();
            Point maxXPoint = mMainBeam.GetMaxXPoint();
            upDimPointList.Add(minXPoint);
            upDimPointList.Add(maxXPoint);

            PointList pointList = new PointList();
            foreach (Point point in upDimPointList)
            {
                pointList.Add(point);
            }

            Vector upDimVector = new Vector(0, 1, 0);
            mUpDimDistance = GetUpDimDistance(upDimPointList[0]);
            CDimTools.GetInstance().DrawDimensionSet(mViewBase, pointList, upDimVector, mUpDimDistance, CCommonPara.mSizeDimPath);
            mMrPartList.Add(mMainBeam);
        }

        /// <summary>
        /// 绘制所有零件的下方标注;
        /// </summary>
        public void DrawAllPartDownDimX()
        {
            mMrPartList.Remove(mMainBeam);
            bool bNeedDownDim = false;
            
            List<Point> downDimPointList = new List<Point>();

            foreach (CMrPart mrPart in mMrPartList)
            {
                CMrDimSet partDimSet = mrPart.GetBeamTopViewInfo().GetPartDownDimSet();

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

            //2.默认把主梁的左右最小最大点添加到链表中;
            Point minXPoint = mMainBeam.GetMinXPoint();
            Point maxXPoint = mMainBeam.GetMaxXPoint();
            downDimPointList.Add(minXPoint);
            downDimPointList.Add(maxXPoint);

            PointList pointList = new PointList();
            
            foreach (Point point in downDimPointList)
            {
                pointList.Add(point);
            }

            double dimDistance = GetDownDimDistance(pointList[0]);
            Vector upDimVector = new Vector(0, -1, 0);
            CDimTools.GetInstance().DrawDimensionSet(mViewBase, pointList, upDimVector, dimDistance, CCommonPara.mSizeDimPath);
            mMrPartList.Add(mMainBeam);
        }

        /// <summary>
        /// 绘制所有零件的上方分尺寸;
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
                CMrDimSet partDimSet = mrPart.GetBeamTopViewInfo().GetPartUpDimSet();

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

            Point minXPoint = mMainBeam.GetMinXPoint();
            Point maxXPoint = mMainBeam.GetMaxXPoint();

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
        /// 绘制所有零件向下的分尺寸;
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
                CMrDimSet partDimSet = mrPart.GetBeamTopViewInfo().GetPartDownDimSet();

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

                Point minXPoint = mMainBeam.GetMinXPoint();
                Point maxXPoint = mMainBeam.GetMaxXPoint();

                pointList.Add(minXPoint);
                pointList.Add(maxXPoint);

                Vector upDimVector = new Vector(0, 1, 0);
                mUpDimDistance = GetUpDimDistance(pointList[0]);
                CDimTools.GetInstance().DrawDimensionSet(mViewBase, pointList, upDimVector, mUpDimDistance, CCommonPara.mSizeDimPath);
            }
        }

        /// <summary>
        /// 标注主零件的长度尺寸;
        /// </summary>
        public void DrawMainPartLengthDimX()
        {
            CBeamTopViewSetting beamTopViewSetting = CBeamDimSetting.GetInstance().mTopViewSetting;

            //1.判断是否需要标注主梁的长度,如果需要采用单一的一道总尺寸;
            bool bNeedDimMainPartLength = beamTopViewSetting.FindDimValueByName(CBeamTopViewSetting.mstrMainPartLength);

            bool bMarkValue = beamTopViewSetting.FindMarkValueByName(CBeamTopViewSetting.mstrMainPartLength);
            mMainBeam.SetNeedAddMarkFlag(bMarkValue);

            if (!bNeedDimMainPartLength)
            {
                return;
            }

            Point minXPoint = mMainBeam.GetMinXPoint();
            Point maxXPoint = mMainBeam.GetMaxXPoint();

            bMarkValue = beamTopViewSetting.FindMarkValueByName(CBeamTopViewSetting.mstrWorkPointToWorkPoint);
            mMainBeam.SetNeedAddMarkFlag(bMarkValue);

            //2.如果需要标注工作点到工作点,并且主部件与工作点的距离左边或者右边为0，或者都为0时则不标注主部件长度;
            bool bNeedWorkPtToWorkPt = beamTopViewSetting.FindDimValueByName(CBeamTopViewSetting.mstrWorkPointToWorkPoint);

            if (bNeedWorkPtToWorkPt)
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
            CBeamTopViewSetting beamTopViewSetting = CBeamDimSetting.GetInstance().mTopViewSetting;

            bool bMarkValue = beamTopViewSetting.FindMarkValueByName(CBeamTopViewSetting.mstrWorkPointToMainPart);
            mMainBeam.SetNeedAddMarkFlag(bMarkValue);

            //1.判断是否需要标注主梁与工作点之间的距离,采用一道尺寸来进行标注;
            bool bNeedDimWpToMainPart = beamTopViewSetting.FindDimValueByName(CBeamTopViewSetting.mstrWorkPointToMainPart);

            if (!bNeedDimWpToMainPart)
            {
                return;
            }

            Point mainBeamMinX = CMrMainBeam.GetInstance().GetMinXPoint();
            Point mainBeamMaxX = CMrMainBeam.GetInstance().GetMaxXPoint();
            Point leftWorkPoint = CMrMainBeam.GetInstance().mLeftWorkPoint;
            Point rightWorkPoint = CMrMainBeam.GetInstance().mRightWorkPoint;

            bMarkValue = beamTopViewSetting.FindMarkValueByName(CBeamTopViewSetting.mstrWorkPointToWorkPoint);
            mMainBeam.SetNeedAddMarkFlag(bMarkValue);

            //2.如果工作点到工作点需要进行标注，而主部件左右侧与工作点的左右侧的距离都为0则不需要标注;
            bool bNeedWorkPtToWorkPt = beamTopViewSetting.FindDimValueByName(CBeamTopViewSetting.mstrWorkPointToWorkPoint);

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
            CBeamTopViewSetting beamTopViewSetting = CBeamDimSetting.GetInstance().mTopViewSetting;

            PointList pointList = new PointList();

            bool bMarkValue = beamTopViewSetting.FindMarkValueByName(CBeamTopViewSetting.mstrWorkPointToWorkPoint);
            mMainBeam.SetNeedAddMarkFlag(bMarkValue);

            //1.判断工作点与工作点之间是否需要标注，需要标注的话是一道单尺寸;
            bool bNeedDimWpToWp = beamTopViewSetting.FindDimValueByName(CBeamTopViewSetting.mstrWorkPointToWorkPoint);

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
        /// 标注切割的尺寸;
        /// </summary>
        public void DrawCuttingDim()
        {
            CBeamTopViewSetting beamTopViewSetting = CBeamDimSetting.GetInstance().mTopViewSetting;
            PointList pointList = new PointList();

            //判断是否需要标注切割部分，切割需要采用单一尺寸来标注;
            bool bNeedDimCutting = beamTopViewSetting.FindDimValueByName(CBeamTopViewSetting.mstrCutting);

            if (bNeedDimCutting)
            {
                Point minYMinXPoint = mMainBeam.GetMinYMinXPoint();
                Point maxYMinXPoint = mMainBeam.GetMaxYMinXPoint();

                Vector upDimVector = new Vector(0, 1, 0);
                Vector downDimVector = new Vector(0, -1, 0);

                //A.判断主梁左侧的切割;
                if (maxYMinXPoint.X > minYMinXPoint.X)
                {
                    pointList.Clear();
                    pointList.Add(maxYMinXPoint);
                    pointList.Add(minYMinXPoint);
                    double dimDistance = CCommonPara.mDefaultDimDistance;
                    CDimTools.GetInstance().DrawDimensionSet(mViewBase, pointList, upDimVector, dimDistance, CCommonPara.mMainSizeDimPath);
                }
                if (maxYMinXPoint.X < minYMinXPoint.X)
                {
                    pointList.Clear();
                    pointList.Add(maxYMinXPoint);
                    pointList.Add(minYMinXPoint);
                    double dimDistance = CCommonPara.mDefaultDimDistance;
                    CDimTools.GetInstance().DrawDimensionSet(mViewBase, pointList, downDimVector, dimDistance, CCommonPara.mMainSizeDimPath);
                }

                //B.判断主梁右侧的切割;
                Point minYMaxXPoint = mMainBeam.GetMinYMaxXPoint();
                Point maxYMaxXPoint = mMainBeam.GetMaxYMaxXPoint();

                if (minYMaxXPoint.X > maxYMaxXPoint.X)
                {
                    pointList.Clear();
                    pointList.Add(maxYMaxXPoint);
                    pointList.Add(minYMaxXPoint);
                    double dimDistance = CCommonPara.mDefaultDimDistance;
                    CDimTools.GetInstance().DrawDimensionSet(mViewBase, pointList, upDimVector, dimDistance, CCommonPara.mMainSizeDimPath);
                }
                else if (minYMaxXPoint.X < maxYMaxXPoint.X)
                {
                    pointList.Clear();
                    pointList.Add(minYMaxXPoint);
                    pointList.Add(maxYMaxXPoint);
                    double dimDistance = CCommonPara.mDefaultDimDistance;
                    CDimTools.GetInstance().DrawDimensionSet(mViewBase, pointList, downDimVector, dimDistance, CCommonPara.mMainSizeDimPath);
                }
            }
        }

        /// <summary>
        /// 绘制斜的加筋板或斜的连接板自身的标注;
        /// </summary>
        public void DrawObliquePartDim()
        {
            mMrPartList.Remove(mMainBeam); 

            foreach (CMrPart mrPart in mMrPartList)
            {
                List<CMrDimSet> partDimSetList = mrPart.GetBeamTopViewInfo().GetObliquePartDimSet();

                if (partDimSetList == null || partDimSetList.Count == 0)
                {
                    continue;
                }
                foreach (CMrDimSet mrDimSet in partDimSetList)
                {
                    List<Point> dimPointList = mrDimSet.GetDimPointList();

                    if (dimPointList.Count == 1)
                    {
                        continue;
                    }
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
            mMrPartList.Add(mMainBeam);
        }

        /// <summary>
        /// 绘制零件的Bolt在Y向上的标注;
        /// </summary>
        public void DrawPartBoltDimY()
        {
            //移除主梁;
            mMrPartList.Remove(mMainBeam);

            foreach (CMrPart mrPart in mMrPartList)
            {
                List<CMrDimSet> boltDimSetList = mrPart.GetBeamTopViewInfo().GetPartBoltYDimSetList();

                if (boltDimSetList == null || boltDimSetList.Count == 0)
                {
                    continue;
                }
                foreach (CMrDimSet mrDimSet in boltDimSetList)
                {
                    if (mrDimSet==null||mrDimSet.Count <= 1)
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

                    if (dimVector.X > 0)
                    {
                        dimDistance = mrPart.GetRightDefaultDimDistance(pointList[0]);
                    }
                    else
                    {
                        dimDistance = mrPart.GetLeftDefaultDimDistance(pointList[0]);
                    }

                    CDimTools.GetInstance().DrawDimensionSet(mViewBase, pointList, dimVector, dimDistance, CCommonPara.mSizeDimPath);
                }
            }
            mMrPartList.Add(mMainBeam);
        }

        /// <summary>
        /// 绘制主梁的Bolt在Y向上的标注;
        /// </summary>
        private void DrawMainPartBoltDimY()
        {
            CBeamTopViewSetting beamTopViewSetting = CBeamDimSetting.GetInstance().mTopViewSetting;

            bool bNeedMainBeamBoltDim = beamTopViewSetting.FindDimValueByName(CBeamTopViewSetting.mstrBolt);

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
                if (!IsNeedMrDimSetDim(mrDimSet))
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
        /// 获得向上标注的距离;
        /// </summary>
        /// <returns></returns>
        private double GetUpDimDistance(Point firstDimPoint)
        {
            if (mUpDimDistance == 0)
            {
                mUpDimDistance = Math.Abs(CCommonPara.mViewMaxY - firstDimPoint.Y) + CCommonPara.mDefaultDimDistance;
            }
            else
            {
                mUpDimDistance = mUpDimDistance + CCommonPara.mDefaultTwoDimLineGap + mUpDimFirstPoint.Y - firstDimPoint.Y;
            }
            
            mUpDimFirstPoint = firstDimPoint;

            return mUpDimDistance;
        }

        /// <summary>
        /// 获得向下标注的距离;
        /// </summary>
        /// <returns></returns>
        private double GetDownDimDistance(Point firstDimPoint)
        {
            if (mDownDimDistance == 0)
            {
                mDownDimDistance = Math.Abs(CCommonPara.mViewMinY - firstDimPoint.Y) + CCommonPara.mDefaultDimDistance;
            }
            else
            {
                mDownDimDistance = mDownDimDistance + CCommonPara.mDefaultTwoDimLineGap + mDownDimFirstPoint.Y - firstDimPoint.Y;
            }

            mDownDimFirstPoint = firstDimPoint;

            return mDownDimDistance;
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
        /// 获得主梁螺钉在Y向的标注集;
        /// </summary>
        /// <returns></returns>
        private List<CMrDimSet> GetMainPartBoltDimSetY()
        {
            List<CMrDimSet> mrDimSetList = new List<CMrDimSet>();
            List<CMrBoltArray> mrLeftBoltArrayList = new List<CMrBoltArray>();
            List<CMrBoltArray> mrRightBoltArrayList = new List<CMrBoltArray>();

            //0.首先需要获得在主梁两侧并在一定阈值内的螺钉，该螺钉需要标注到主梁两侧;
            GetMainPartBorderBoltArrayList(ref mrLeftBoltArrayList, ref mrRightBoltArrayList);

            //1.左侧的标注集;
            if (mrLeftBoltArrayList.Count > 0)
            {
                CMrDimSet mrLeftDimSet = new CMrDimSet();
                foreach (CMrBoltArray mrBoltArray in mrLeftBoltArrayList)
                {
                    List<Point> minXPointList = mrBoltArray.GetMinXPointList();
                    mrLeftDimSet.AddRange(minXPointList);
                }
                if (mrLeftDimSet.Count > 0)
                {
                    mrLeftDimSet.AddPoint(new Point(mMainBeam.GetMinXPoint().X, 0, 0));
                }
                mrLeftDimSet.mDimVector = new Vector(-1, 0, 0);
                double dblDistance = Math.Abs(CCommonPara.mViewMinX - mrLeftDimSet.GetDimPointList()[0].X);
                mrLeftDimSet.mDimDistance = dblDistance + CCommonPara.mDefaultDimDistance;
                mrDimSetList.Add(mrLeftDimSet);
            }
            //2.右侧的标注集;
            if (mrRightBoltArrayList.Count > 0)
            {
                CMrDimSet mrRightDimSet = new CMrDimSet();

                foreach (CMrBoltArray mrBoltArray in mrRightBoltArrayList)
                {
                    List<Point> maxXPointList = mrBoltArray.GetMaxXPointList();
                    mrRightDimSet.AddRange(maxXPointList);
                }
                if (mrRightDimSet.Count > 0)
                {
                    mrRightDimSet.AddPoint(new Point(mMainBeam.GetMaxXPoint().X, 0, 0));
                }
                mrRightDimSet.mDimVector = new Vector(1, 0, 0);
                double dblDistance = Math.Abs(CCommonPara.mViewMaxX - mrRightDimSet.GetDimPointList()[0].X);
                mrRightDimSet.mDimDistance = dblDistance + CCommonPara.mDefaultDimDistance;
                mrDimSetList.Add(mrRightDimSet);
            }
            //3.主梁上剩下的螺钉组标注，标注在主梁中间;
            foreach (CMrBoltArray mrBoltArray in mMainBeam.GetBoltArrayList())
            {
                Vector normal = mrBoltArray.mNormal;

                if (!CDimTools.GetInstance().IsTwoVectorParallel(normal, new Vector(0, 0, 1)))
                {
                    continue;
                }
                if (mrLeftBoltArrayList.Contains(mrBoltArray)||mrRightBoltArrayList.Contains(mrBoltArray))
                {
                    continue;
                }
                CMrDimSet mrDimSet = new CMrDimSet();
                mrDimSet.AddRange(mrBoltArray.GetMaxXPointList());
                mrDimSet.AddPoint(new Point(mrBoltArray.GetMaxXPoint().X, 0, 0));
                mrDimSet.mDimVector = new Vector(1, 0, 0);
                mrDimSet.mDimDistance = CCommonPara.mDefaultDimDistance;
                mrDimSetList.Add(mrDimSet);
            }
            return mrDimSetList;
        }

        /// <summary>
        /// 获得主梁上的左边和右边螺钉组的链表;
        /// </summary>
        /// <returns></returns>
        private void GetMainPartBorderBoltArrayList(ref List<CMrBoltArray> leftBoltArrayList, ref List<CMrBoltArray> rightBoltArrayList)
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
        /// 开始绘制零件标记;
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
        /// 绘制主梁上螺钉的标记;
        /// </summary>
        public void DrawMainPartBoltMark()
        {
            CBeamTopViewSetting beamTopViewSetting = CBeamDimSetting.GetInstance().mTopViewSetting;

            bool bNeedMark = beamTopViewSetting.FindMarkValueByName(CBeamTopViewSetting.mstrBolt);

            if (!bNeedMark)
            {
                return;
            }

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

        /// <summary>
        /// 判断该MrDimSet是否需要标注;
        /// </summary>
        /// <param name="mrDimSet"></param>
        /// <returns></returns>
        private bool IsNeedMrDimSetDim(CMrDimSet mrDimSet)
        {
            String strPointList = null;
            
            List<Point> dimPointList = mrDimSet.GetDimPointList();

            Comparison<Point> sorterX = new Comparison<Point>(CDimTools.ComparePointX);

            dimPointList.Sort(sorterX);

            foreach (Point point in dimPointList)
            {
                strPointList = strPointList + "_" + ((int)point.X).ToString();
            }

            if (mDicYBoltDimPoints.ContainsKey(strPointList))
            {
                return false;
            }
            else
            {
                mDicYBoltDimPoints.Add(strPointList, true);

                return true;
            }
        }
    }
}
