using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Tekla.Structures;
using Tekla.Structures.Drawing.UI;
using Tekla.Structures.Drawing;
using Tekla.Structures.Geometry3d;
using System.Collections;
using Tekla.Structures.Model;
using Tekla.Structures.Solid;
using Tekla.Structures.Model.Operations;

using TSD = Tekla.Structures.Drawing;
using TSM = Tekla.Structures.Model;

using AutoDimension.Entity;
using System.IO;

namespace AutoDimension
{
    /// <summary>
    /// 标注工具类;
    /// </summary>
    public class CDimTools
    {
        private static CDimTools mDimTools = null;
        public bool IsOut;
        private CDimTools()
        {
        }

        /// <summary>
        /// 获得单例;
        /// </summary>
        /// <returns></returns>
        public static CDimTools GetInstance()
        {
            if (null == mDimTools)
            {
                mDimTools = new CDimTools();
                mDimTools.IsOut = false;
            }
            return mDimTools;
        }

        /// <summary>
        /// 从界面中选择几个点;
        /// </summary>
        /// <param name="numberToPick"></param>
        /// <param name="pointList"></param>
        /// <param name="view"></param>
        /// <returns></returns>
        public bool PickPoints(int numberToPick, ref PointList pointList, ref ViewBase view)
        {
            try
            {
                Picker picker = new DrawingHandler().GetPicker();
                Point point;
                int ii = numberToPick;
                while (--ii != -1)
                {
                    picker.PickPoint("选择视图中的一个点", out point, out view);
                    pointList.Add(point);
                }
            }
            catch
            {
                return false;
            }

            return pointList.Count == numberToPick;
        }

        /// <summary>
        /// 获取选择的View中所有的零件对象;
        /// </summary>
        /// <param name="view"></param>
        /// <returns></returns>
        public List<TSD.Part> GetAllPartInDrawing(ViewBase view)
        {
            List<TSD.Part> partList = new List<TSD.Part>();

            DrawingObjectEnumerator allParts = view.GetAllObjects(typeof(TSD.Part));

            while (allParts.MoveNext())
            {
                if (allParts.Current == null)
                {
                    continue;
                }

                TSD.Part part = allParts.Current as TSD.Part;

                if (part != null)
                {
                    partList.Add(part);
                }
            }
            return partList;
        }

        /// <summary>
        /// 获取选择View中所有的螺钉;
        /// </summary>
        /// <param name="view"></param>
        /// <returns></returns>
        public Dictionary<Identifier,TSD.Bolt> GetAllBoltInDrawing(ViewBase view)
        {
            Dictionary<Identifier, TSD.Bolt> dicIndentifierBolt = new Dictionary<Identifier,TSD.Bolt>();

            DrawingObjectEnumerator allBolts = view.GetAllObjects(typeof(TSD.Bolt));

            while (allBolts.MoveNext())
            {
                if (allBolts.Current == null)
                {
                    continue;
                }

                TSD.Bolt bolt = allBolts.Current as TSD.Bolt;

                if (bolt != null)
                {
                    dicIndentifierBolt.Add(bolt.ModelIdentifier,bolt);
                }
            }
            return dicIndentifierBolt;
        }


        /// <summary>
        /// Drawing中的ModelObject转化为Model中的ModelObject;
        /// </summary>
        /// <param name="modelObjectInDrawing"></param>
        /// <returns></returns>
        public TSM.ModelObject TransformDrawingToModel(TSD.ModelObject modelObjectInDrawing)
        {
            TSM.Model model=CDimManager.GetInstance().GetModel();

			TSM.ModelObject modelObjectInModel = model.SelectModelObject(modelObjectInDrawing.ModelIdentifier);

            if (modelObjectInModel != null)
            {
                return modelObjectInModel;
            }
            return null;
        }

        /// <summary>
        /// 根据Tekla的模型初始化自定义部件;        
        /// </summary>
        /// <param name="modelObject"></param>
        /// <param name="mrPart"></param>
        public void InitMrPart(TSM.ModelObject modelObject,TSD.View PartView,CMrPart mrPart)
        {
            Point beamMinPoint = null;
            Point beamMaxPoint = null;
            TSM.Part beam= modelObject as TSM.Part;

            if (null == beam)
            {
                return;
            }

            if (beam is Beam)
            {
                mrPart.mBeamType = MrBeamType.BEAM;
            }
            else if (beam is ContourPlate)
            {
                mrPart.mBeamType = MrBeamType.CONTOURPLATE;
            }
            else if (beam is PolyBeam)
            {
                mrPart.mBeamType = MrBeamType.POLYBEAM;
            }

            //获取当前视图的变换矩阵;
            Matrix displayConvMatirx = MatrixFactory.ToCoordinateSystem(PartView.DisplayCoordinateSystem);
            Matrix beamConvMatrix = MatrixFactory.ToCoordinateSystem(beam.GetCoordinateSystem());
            Matrix beamInverseMatrix = MatrixFactory.FromCoordinateSystem(beam.GetCoordinateSystem());

            //获取主零件的最大点和最小点;
            beamMaxPoint = beam.GetSolid().MaximumPoint;
            beamMaxPoint = displayConvMatirx.Transform(beamMaxPoint);

            beamMinPoint = beam.GetSolid().MinimumPoint;
            beamMinPoint = displayConvMatirx.Transform(beamMinPoint);

            //计算梁的法向向量,梁在自身坐标系的法向是Z轴方向;
            Vector zVector = new Vector(0, 0, 1);
            Point point1 = new Point(0, 0, 0);
            Point point2 = zVector;
            point1 = beamInverseMatrix.Transform(point1);
            point2 = beamInverseMatrix.Transform(point2);

            point1 = displayConvMatirx.Transform(point1);
            point2 = displayConvMatirx.Transform(point2);

            Vector newNormal = new Vector(point2.X - point1.X, point2.Y - point1.Y, point2.Z - point1.Z);
            newNormal.Normalize();
            mrPart.mNormal = newNormal;

            //如果是主梁，则记录主梁的工作点;
            if (mrPart == CMrMainBeam.GetInstance())
            {
                ArrayList list = mrPart.mPartInModel.GetCenterLine(false);
                
                if (list.Count == 2)
                {
                    point1 = list[0] as Point;
                    point2 = list[1] as Point;
                    point1 = displayConvMatirx.Transform(point1);
                    point2 = displayConvMatirx.Transform(point2);

                    if (CDimTools.GetInstance().CompareTwoDoubleValue(point1.X,point2.X) < 0)
                    {
                        CMrMainBeam.GetInstance().mLeftWorkPoint = point1;
                        CMrMainBeam.GetInstance().mRightWorkPoint = point2;
                    }
                    else if(CDimTools.GetInstance().CompareTwoDoubleValue(point1.X,point2.X) > 0)
                    {
                        CMrMainBeam.GetInstance().mLeftWorkPoint = point2;
                        CMrMainBeam.GetInstance().mRightWorkPoint = point1;
                    }
                    if (CDimTools.GetInstance().CompareTwoDoubleValue(point1.Y, point2.Y) < 0)
                    {
                        CMrMainBeam.GetInstance().mLeftWorkPoint = point2;
                        CMrMainBeam.GetInstance().mRightWorkPoint = point1;
                    }
                    else if (CDimTools.GetInstance().CompareTwoDoubleValue(point1.Y, point2.Y) > 0)
                    {
                        CMrMainBeam.GetInstance().mLeftWorkPoint = point1;
                        CMrMainBeam.GetInstance().mRightWorkPoint = point2;
                    }

                    List<Point> CenterLinePointList = new List<Point>();

                    CenterLinePointList.Add(point1);
                    CenterLinePointList.Add(point2);

                    mrPart.mCenterLinePointList = CenterLinePointList;
                }
            }
            else
            {
                ArrayList list = mrPart.mPartInModel.GetCenterLine(false);

                List<Point> CenterPointList = new List<Point>();

                foreach (Point pt in list)
                {
                    point1 = displayConvMatirx.Transform(pt);
                    CenterPointList.Add(point1);
                }

                mrPart.mCenterLinePointList = CenterPointList;
            }

            double dblWidth = 0.0;
            double dblHeight = 0.0;
            modelObject.GetReportProperty("PROFILE.WIDTH", ref dblWidth);
            modelObject.GetReportProperty("PROFILE.HEIGHT", ref dblHeight);
            mrPart.mWidth = dblWidth;
            mrPart.mHeight = dblHeight;

            //找到梁的外轮廓点;
            List<Point> beamOutSidePoints = new List<Point>();
            TSM.Solid mainBeamSolid = beam.GetSolid();
            FaceEnumerator faceEnum = mainBeamSolid.GetFaceEnumerator();
            while (faceEnum.MoveNext())
            {
                Face face = faceEnum.Current as Face;
                LoopEnumerator loopEnum = face.GetLoopEnumerator();
                while (loopEnum.MoveNext())
                {
                    Loop loop = loopEnum.Current as Loop;
                    VertexEnumerator vertexEnum = loop.GetVertexEnumerator();

                    while (vertexEnum.MoveNext())
                    {
                        Point point = vertexEnum.Current as Point;
                        Point displayPoint = displayConvMatirx.Transform(point);
                        
                        if (!beamOutSidePoints.Contains(displayPoint))
                        {
                            beamOutSidePoints.Add(displayPoint);
                        }
                    }
                }
            } //寻找外轮廓点结束;

            mrPart.SetName(beam.Name);
            mrPart.SetPointList(beamOutSidePoints);
            mrPart.SetMinAndMaxPoint(beamMinPoint, beamMaxPoint);
        }

        /// <summary>
        /// 获取零件的最大最小点;
        /// </summary>
        /// <param name="modelObject"></param>
        /// <param name="PartView"></param>
        /// <param name="partMinPoint"></param>
        /// <param name="partMaxPoint"></param>
        public void GetPartMinAndMaxPoint(TSM.ModelObject modelObject,TSD.View PartView,ref Point partMinPoint,ref Point partMaxPoint)
        {
            TSM.Part beam = modelObject as TSM.Part;

            if (null == beam)
            {
                return;
            }

            //获取当前视图的变换矩阵;
            Matrix displayConvMatirx = MatrixFactory.ToCoordinateSystem(PartView.DisplayCoordinateSystem);

            //获取主零件的最大点和最小点;
            partMaxPoint = beam.GetSolid().MaximumPoint;
            partMaxPoint = displayConvMatirx.Transform(partMaxPoint);
            partMinPoint = beam.GetSolid().MinimumPoint;
            partMinPoint = displayConvMatirx.Transform(partMinPoint);
        }

        /// <summary>
        /// 判断零件是否是在视图的包围盒中;
        /// </summary>
        /// <param name="partMinPoint"></param>
        /// <param name="partMaxPoint"></param>
        /// <param name="viewMinPoint"></param>
        /// <param name="viewMaxPoint"></param>
        /// <returns></returns>
        public bool IsPartInViewBox(Point partMinPoint,Point partMaxPoint,Point viewMinPoint,Point viewMaxPoint)
        {
            try
            {
                //YB 
                return CServers.GetServers().IsPartInViewBox(TKPointToPt(partMinPoint),
                                                             TKPointToPt(partMaxPoint),
                                                             TKPointToPt(viewMinPoint),
                                                             TKPointToPt(viewMaxPoint));
            }
            catch{
                mDimTools.IsOut = true;
                return false;
                }
           
        }
        private ServiceReference.Point TKPointToPt(Point point)
        {
            ServiceReference.Point pt = new ServiceReference.Point();
            pt.X = point.X;
            pt.Y = point.Y;
            pt.Z = point.Z;
            return pt;
        }

        /// <summary>
        /// 初始化自定义的螺钉组;
        /// </summary>
        /// <param name="boltArray"></param>
        /// <param name="boltView"></param>
        /// <param name="mrBoltArray"></param>
        public void InitMrBoltArray(TSM.BoltArray boltArray, TSD.View boltView, CMrBoltArray mrBoltArray)
        {
            Point firstPoint = boltArray.FirstPosition;
            Point secondPoint = boltArray.SecondPosition;

            ArrayList boltPoisitons = boltArray.BoltPositions;

            //获取当前视图的变换矩阵;
            Matrix convMatirx = MatrixFactory.ToCoordinateSystem(boltView.DisplayCoordinateSystem);

            //获取螺钉组向当前平面转换的矩阵;
            Matrix boltConvMatrix = MatrixFactory.FromCoordinateSystem(boltArray.GetCoordinateSystem());

            //转换螺钉组的起始点和终止点;
            Point firstPointInDrawing = convMatirx.Transform(firstPoint);
            Point secondPointInDrawing = convMatirx.Transform(secondPoint);

            firstPointInDrawing.Z = 0;
            secondPointInDrawing.Z = 0;

            mrBoltArray.mFirstPoint = firstPointInDrawing;
            mrBoltArray.mSecondPoint = secondPointInDrawing;

            Point pointInDrawing = null;

            foreach (Point point in boltPoisitons)
            {
                pointInDrawing = convMatirx.Transform(point);
             
                CMrBolt mrBolt = new CMrBolt();
                mrBolt.mPosition = pointInDrawing;
                mrBoltArray.AppendBolts(mrBolt);
            }

            //计算螺钉组的法向量;螺钉组在自身坐标系下的法向是Z方向;
            Vector zVector = new Vector(0, 0, 1);
            Point point1 = new Point(0, 0, 0);
            Point point2 = zVector;

            point1 = boltConvMatrix.Transform(point1);
            point2 = boltConvMatrix.Transform(point2);

            point1 = convMatirx.Transform(point1);
            point2 = convMatirx.Transform(point2);

            Vector newNormal = new Vector(point2.X - point1.X, point2.Y - point1.Y, point2.Z - point1.Z);

            newNormal.Normalize();
            mrBoltArray.mNormal = newNormal;

            mrBoltArray.InitBoltArrayShape();
        }

        /// <summary>
        /// 获得Part中所有的螺钉组;
        /// </summary>
        /// <param name="?"></param>
        /// <returns></returns>
        public List<BoltArray> GetAllBoltArray(TSM.Part partInModel)
        {
            List<BoltArray> boltArrayList=new List<BoltArray>();

            ModelObjectEnumerator modelEnum = partInModel.GetBolts();

            if (null == modelEnum)
            {
                return boltArrayList;
            }

            while (modelEnum.MoveNext())
            {
                TSM.BoltArray boltArray = modelEnum.Current as BoltArray;

                if (boltArray != null)
                {
                    boltArrayList.Add(boltArray);
                }
            }
            return boltArrayList;
        }

        /// <summary>
        /// 判断两条直线的斜率;
        /// </summary>
        /// <param name="pt1"></param>
        /// <param name="pt2"></param>
        /// <returns></returns>
        public MrSlopeType JudgeLineSlope(Point pt1,Point pt2)
        {
#if DEBUG
            if (Math.Abs(pt1.X - pt2.X) < CCommonPara.mDblError)
            {
                return MrSlopeType.INFINITY;
            }
            if (Math.Abs(pt1.Y - pt2.Y) < CCommonPara.mDblError)
            {
                return MrSlopeType.EQUAL_ZERO;
            }

            double k = 0.0;
            k = (pt2.Y - pt1.Y) / (pt2.X - pt1.X);

            if (k > CCommonPara.mDblError)
            {
                return MrSlopeType.MORETHAN_ZERO;
            }
            else
            {
                return MrSlopeType.LESSTHAN_ZERO;
            }
#else
            try
            {
                 return (MrSlopeType)CServers.GetServers().JudgeLineSlope(TKPointToPt(pt1), TKPointToPt(pt2));
             }
             catch
             {
                 mDimTools.IsOut = true;
                 return MrSlopeType.EQUAL_ZERO;
             }
#endif
        }

        /// <summary>
        /// 比较点的X值的大小函数;
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static int ComparePointX(Point x, Point y)
        {
            if (x.X > y.X)
            {
                return 1;
            }
            else if (x.X < y.X)
            {
                return -1;
            }
            return 0;  
        }

        /// <summary>
        /// 比较点的Y值的大小函数;
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static int ComparePointY(Point x, Point y)
        {
            if (x.Y > y.Y)
            {
                return 1;
            }
            else if (x.Y < y.Y)
            {
                return -1;
            }
            return 0;
        }

        /// <summary>
        /// 比较点的Z值的大小函数;
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static int ComparePointZ(Point x, Point y)
        {
            if (x.Z > y.Z)
            {
                return 1;
            }
            else if (x.Z < y.Z)
            {
                return -1;
            }
            return 0;
        }

        /// <summary>
        /// 根据零件最小的X值来进行排序;
        /// </summary>
        /// <param name="mrPart1"></param>
        /// <param name="mrPart2"></param>
        /// <returns></returns>
        public static int CompareMrPartByMinX(CMrPart mrPart1, CMrPart mrPart2)
        {
            double minX1 = mrPart1.GetMinXPoint().X;
            double minX2 = mrPart2.GetMinXPoint().X;

            if (minX1 > minX2)
            {
                return 1;
            }
            else if (minX1 < minX2)
            {
                return -1;
            }

            return 0;
        }

        /// <summary>
        /// 根据零件最小的Y值来进行排序;
        /// </summary>
        /// <param name="mrPart1"></param>
        /// <param name="mrPart2"></param>
        /// <returns></returns>
        public static int CompareMrPartByMinY(CMrPart mrPart1, CMrPart mrPart2)
        {
            double minY1 = mrPart1.GetMinYPoint().Y;
            double minY2 = mrPart2.GetMinYPoint().Y;

            if (minY1 > minY2)
            {
                return 1;
            }
            else if (minY1 < minY2)
            {
                return -1;
            }

            return 0;
        }

        /// <summary>
        /// 根据零件最大的Y值来进行排序;
        /// </summary>
        /// <param name="mrPart1"></param>
        /// <param name="mrPart2"></param>
        /// <returns></returns>
        public static int CompareMrPartByMaxY(CMrPart mrPart1, CMrPart mrPart2)
        {
            double maxY1 = mrPart1.GetMaxYPoint().Y;
            double maxY2 = mrPart2.GetMaxYPoint().Y;

            if (maxY1 > maxY2)
            {
                return 1;
            }
            else if (maxY1 < maxY2)
            {
                return -1;
            }

            return 0;
        }

        /// <summary>
        /// 根据螺钉的最小X值来进行排序;
        /// </summary>
        /// <param name="mrBoltArray1"></param>
        /// <param name="mrBoltArray2"></param>
        /// <returns></returns>
        public static int CompareMrBoltArrayByMinX(CMrBoltArray mrBoltArray1, CMrBoltArray mrBoltArray2)
        {
            double minX1 = mrBoltArray1.GetMinXPoint().X;
            double minX2 = mrBoltArray2.GetMinXPoint().X;

            if (minX1 > minX2)
            {
                return 1;
            }
            else if (minX1 < minX2)
            {
                return -1;
            }

            return 0;
        }

        /// <summary>
        /// 根据螺钉的最大Y值来进行排序;
        /// </summary>
        /// <param name="mrBoltArray1"></param>
        /// <param name="mrBoltArray2"></param>
        /// <returns></returns>
        public static int CompareMrBoltArrayByMaxY(CMrBoltArray mrBoltArray1, CMrBoltArray mrBoltArray2)
        {
            double maxY1 = mrBoltArray1.GetMaxYPoint().Y;
            double maxY2 = mrBoltArray2.GetMaxYPoint().Y;

            if (maxY1 > maxY2)
            {
                return 1;
            }
            else if (maxY1 < maxY2)
            {
                return -1;
            }
            return 0;
        }

        /// <summary>
        /// 根据零部件最小X值来进行排序;
        /// </summary>
        /// <param name="mrPartList"></param>
        /// <returns></returns>
        public bool SortMrPartByMinX(List<CMrPart> mrPartList)
        {
            if (mrPartList == null || mrPartList.Count == 0)
            {
                return false;
            }

            Comparison<CMrPart> sorterMrPartMinX = new Comparison<CMrPart>(CDimTools.CompareMrPartByMinX);

            mrPartList.Sort(sorterMrPartMinX);

            return true;
        }

        /// <summary>
        /// 根据螺钉数组的最小X值来进行排序;
        /// </summary>
        /// <param name="mrBoltArrayList"></param>
        /// <returns></returns>
        public bool SortMrBoltArrayByMinX(List<CMrBoltArray> mrBoltArrayList)
        {
            if (mrBoltArrayList == null || mrBoltArrayList.Count == 0)
            {
                return false;
            }

            Comparison<CMrBoltArray> sorterMrBoltArrayMinX = new Comparison<CMrBoltArray>(CDimTools.CompareMrBoltArrayByMinX);

            mrBoltArrayList.Sort(sorterMrBoltArrayMinX);

            return true;
        }

        /// <summary>
        /// 获取X值最小Y值最大的螺钉组;
        /// </summary>
        /// <returns></returns>
        public CMrBoltArray GetMinXMaxYMrBoltArray(List<CMrBoltArray> mrBoltArrayList)
        {
            if (mrBoltArrayList == null || mrBoltArrayList.Count == 0)
            {
                return null;
            }

            Comparison<CMrBoltArray> sorterMrBoltArrayMinX = new Comparison<CMrBoltArray>(CDimTools.CompareMrBoltArrayByMinX);

            mrBoltArrayList.Sort(sorterMrBoltArrayMinX);

            double minX = mrBoltArrayList[0].GetMinXPoint().X;

            CMrBoltArray mrBoltArray = null;

            double maxY=int.MinValue;

            foreach (CMrBoltArray boltArray in mrBoltArrayList)
            {
                if (minX == boltArray.GetMinXPoint().X)
                {
                    double boltMaxY = boltArray.GetMaxYPoint().Y;

                    if (boltMaxY > maxY)
                    {
                        mrBoltArray = boltArray;

                        maxY = boltMaxY;
                    }
                }
            }
            return mrBoltArray;
        }

        /// <summary>
        /// 获取X值最大Y值最大的螺钉组;
        /// </summary>
        /// <param name="mrBoltArrayList"></param>
        /// <returns></returns>
        public CMrBoltArray GetMaxXMaxYMrBoltArray(List<CMrBoltArray> mrBoltArrayList)
        {
            if (mrBoltArrayList == null || mrBoltArrayList.Count == 0)
            {
                return null;
            }

            Comparison<CMrBoltArray> sorterMrBoltArrayMinX = new Comparison<CMrBoltArray>(CDimTools.CompareMrBoltArrayByMinX);

            mrBoltArrayList.Sort(sorterMrBoltArrayMinX);

            double maxX = mrBoltArrayList[mrBoltArrayList.Count - 1].GetMaxXPoint().X;

            CMrBoltArray mrBoltArray = null;

            double maxY = int.MinValue;

            foreach (CMrBoltArray boltArray in mrBoltArrayList)
            {
                if (maxX == boltArray.GetMaxXPoint().X)
                {
                    double boltMaxY = boltArray.GetMaxYPoint().Y;

                    if (boltMaxY > maxY)
                    {
                        mrBoltArray = boltArray;

                        maxY = boltMaxY;
                    }
                }
            }
            return mrBoltArray;
        }

        /// <summary>
        /// 获取X值最小Y值最小的螺钉组;
        /// </summary>
        /// <returns></returns>
        public CMrBoltArray GetMinXMinYMrBoltArray(List<CMrBoltArray> mrBoltArrayList)
        {
            if (mrBoltArrayList == null || mrBoltArrayList.Count == 0)
            {
                return null;
            }

            Comparison<CMrBoltArray> sorterMrBoltArrayMinX = new Comparison<CMrBoltArray>(CDimTools.CompareMrBoltArrayByMinX);

            mrBoltArrayList.Sort(sorterMrBoltArrayMinX);

            double minX = mrBoltArrayList[0].GetMinXPoint().X;

            CMrBoltArray mrBoltArray = null;

            double minY = int.MaxValue;

            foreach (CMrBoltArray boltArray in mrBoltArrayList)
            {
                if (minX == boltArray.GetMinXPoint().X)
                {
                    double boltMinY = boltArray.GetMinYPoint().Y;

                    if (boltMinY < minY)
                    {
                        mrBoltArray = boltArray;

                        minY = boltMinY;
                    }
                }
            }
            return mrBoltArray;
        }


        /// <summary>
        /// 根据螺钉的最大Y值来进行排序;
        /// </summary>
        /// <param name="mrBoltArrayList"></param>
        /// <returns></returns>
        public bool SortMrBoltArrayByMaxY(List<CMrBoltArray> mrBoltArrayList)
        {
            if (mrBoltArrayList == null || mrBoltArrayList.Count == 0)
            {
                return false;
            }

            Comparison<CMrBoltArray> sorterMrBoltArrayMaxY = new Comparison<CMrBoltArray>(CDimTools.CompareMrBoltArrayByMaxY);

            mrBoltArrayList.Sort(sorterMrBoltArrayMaxY);

            return true;
        }

        /// <summary>
        /// 根据零部件最小X值来进行排序;
        /// </summary>
        /// <param name="mrPartList"></param>
        /// <returns></returns>
        public bool SortMrPartByMinY(List<CMrPart> mrPartList)
        {
            if (mrPartList == null || mrPartList.Count == 0)
            {
                return false;
            }

            Comparison<CMrPart> sorterMrPartMinY = new Comparison<CMrPart>(CDimTools.CompareMrPartByMinY);

            mrPartList.Sort(sorterMrPartMinY);

            return true;
        }

        /// <summary>
        /// 根据零部件最小X值来进行排序;
        /// </summary>
        /// <param name="mrPartList"></param>
        /// <returns></returns>
        public bool SortMrPartByMaxY(List<CMrPart> mrPartList)
        {
            if (mrPartList == null || mrPartList.Count == 0)
            {
                return false;
            }

            Comparison<CMrPart> sorterMrPartMaxY = new Comparison<CMrPart>(CDimTools.CompareMrPartByMaxY);

            mrPartList.Sort(sorterMrPartMaxY);

            return true;
        }

        /// <summary>
        /// 判断两个向量是否平行;
        /// </summary>
        /// <returns></returns>
        public bool IsTwoVectorParallel(Vector vector1,Vector vector2)
        {
            vector1.Normalize();
            vector2.Normalize();
         
            if((Math.Abs(vector1.X- vector2.X) < CCommonPara.mDblError)
                &&(Math.Abs(vector1.Y-vector2.Y)<CCommonPara.mDblError)
                &&(Math.Abs(vector1.Z - vector2.Z) < CCommonPara.mDblError))
            {
                return true;
            }
            if ((Math.Abs(vector1.X + vector2.X) < CCommonPara.mDblError)
               && (Math.Abs(vector1.Y + vector2.Y) < CCommonPara.mDblError)
               && (Math.Abs(vector1.Z + vector2.Z) < CCommonPara.mDblError))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 判断两个向量是否垂直;
        /// </summary>
        /// <param name="vector1"></param>
        /// <param name="vector2"></param>
        /// <returns></returns>
        public bool IsTwoVectorVertical(Vector vector1, Vector vector2)
        {
            double dblValue = vector1.X * vector2.X + vector1.Y * vector2.Y + vector1.Z * vector2.Z;

            if (Math.Abs(dblValue) < 0.1)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 判断向量是否在xz平面内,而且不是Z轴和X轴;
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        public bool IsVectorInXZPlane(Vector vector)
        {
            vector.Normalize();

            if(Math.Abs(vector.X) > CCommonPara.mDblError
              && Math.Abs(vector.Y) < CCommonPara.mDblError
              && Math.Abs(vector.Z) > CCommonPara.mDblError )
            {
                return true;
            }

            return false;
        }
        
        /// <summary>
        /// 判断向量是否在XY平面内;
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        public bool IsVectorInXYPlane(Vector vector)
        {
            vector.Normalize();

            if (Math.Abs(vector.X) > CCommonPara.mDblError
                && Math.Abs(vector.Y) > CCommonPara.mDblError
                && Math.Abs(vector.Z) < CCommonPara.mDblError)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 判断向量是否在YZ平面内;
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        public bool IsVectorInYZPlane(Vector vector)
        {
            vector.Normalize();

            if (Math.Abs(vector.X) < CCommonPara.mDblError
                && Math.Abs(vector.Y) > CCommonPara.mDblError
                && Math.Abs(vector.Z) > CCommonPara.mDblError)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 获得Y值最小的点;
        /// </summary>
        /// <param name="pointList"></param>
        /// <returns></returns>
        public Point GetMinYPoint(List<Point> pointList)
        {
            if (pointList == null || pointList.Count == 0)
            {
                return null;
            }

            Comparison<Point> sorterY = new Comparison<Point>(CDimTools.ComparePointY);
            pointList.Sort(sorterY);

            return pointList[0];
        }

        /// <summary>
        /// 获得Y值最大的点;
        /// </summary>
        /// <param name="pointList"></param>
        /// <returns></returns>
        public Point GetMaxYPoint(List<Point> pointList)
        {
            if (pointList == null || pointList.Count == 0)
            {
                return null;
            }

            Comparison<Point> sorterY = new Comparison<Point>(CDimTools.ComparePointY);

            pointList.Sort(sorterY);

            int nCount = pointList.Count;

            return pointList[nCount-1];
        }

        /// <summary>
        /// 获得X值最小的点;
        /// </summary>
        /// <param name="pointList"></param>
        /// <returns></returns>
        public Point GetMinXPoint(List<Point> pointList)
        {
/*#if DEBUG*/
            if (pointList == null || pointList.Count == 0)
            {
                return null;
            }
            Comparison<Point> sorterX = new Comparison<Point>(CDimTools.ComparePointX);
            pointList.Sort(sorterX);
            return pointList[0];
/*#else*/
//             try
//             {
//                 ServiceReference.Point[] TkPointList = new ServiceReference.Point[pointList.Count];
// 
//                 int nIndex=0;
//                 foreach (Point pt in pointList)
//                 {
//                     TkPointList[nIndex] = TKPointToPt(pt);
//                     nIndex++;
//                 }
// 
//                 ServiceReference.Point sPt = CServers.GetServers().GetMinXPoint(TkPointList);
//                 Point minXPt = new Point(sPt.X,sPt.Y,sPt.Z);
//                 return minXPt;
//              }
//              catch
//              {
//                  mDimTools.IsOut = true;
//                  return null;
//              }
// #endif
        }

        /// <summary>
        /// 获得X值最大的点;
        /// </summary>
        /// <param name="pointList"></param>
        /// <returns></returns>
        public Point GetMaxXPoint(List<Point> pointList)
        {
            if (pointList == null || pointList.Count == 0)
            {
                return null;
            }

            Comparison<Point> sorterX = new Comparison<Point>(CDimTools.ComparePointX);

            pointList.Sort(sorterX);

            int nCount = pointList.Count;

            return pointList[nCount - 1];
        }

        /// <summary>
        /// 获取z值最小的点;
        /// </summary>
        /// <param name="pointList"></param>
        /// <returns></returns>
        public Point GetMinZPoint(List<Point> pointList)
        {
            if (pointList == null || pointList.Count == 0)
            {
                return null;
            }

            Comparison<Point> sorterZ = new Comparison<Point>(CDimTools.ComparePointZ);

            pointList.Sort(sorterZ);

            int nCount = pointList.Count;

            return pointList[0];
        }

        /// <summary>
        /// 获取z值最大的点;
        /// </summary>
        /// <param name="pointList"></param>
        /// <returns></returns>
        public Point GetMaxZPoint(List<Point> pointList)
        {
            if (pointList == null || pointList.Count == 0)
            {
                return null;
            }

            Comparison<Point> sorterZ = new Comparison<Point>(CDimTools.ComparePointZ);

            pointList.Sort(sorterZ);

            int nCount = pointList.Count;

            return pointList[nCount - 1];
        }

        /// <summary>
        /// 比较两个点是否相等;
        /// </summary>
        /// <param name="point1"></param>
        /// <param name="point2"></param>
        /// <returns></returns>
        public bool IsTwoPointEqual(Point point1,Point point2)
        {
            try
            {
                //YB
                return CServers.GetServers().IsTwoPointEqual(TKPointToPt(point1), TKPointToPt(point2));
            }
            catch
            {
                mDimTools.IsOut = true;
                return false;
            }

        }

        /// <summary>
        /// 清空视图中的所有的标注;
        /// </summary>
        /// <param name="viewBase"></param>
        public void ClearAllDim(TSD.View viewBase)
        {
            if (null == viewBase)
            {
                return;
            }

            DrawingHandler drawingHandler = new DrawingHandler();
            TSD.UI.DrawingObjectSelector DS = drawingHandler.GetDrawingObjectSelector();

            DrawingObjectEnumerator allDims = viewBase.GetAllObjects(typeof(TSD.DimensionBase));

            while (allDims.MoveNext())
            {
                DimensionBase dimInDrawing = allDims.Current as DimensionBase;

                if (dimInDrawing != null)
                {
                    DS.SelectObject(dimInDrawing);
                }
            }

            string strMacrosPath = string.Empty;
            Tekla.Structures.TeklaStructuresSettings.GetAdvancedOption("XS_MACRO_DIRECTORY", ref strMacrosPath);
            if (strMacrosPath.IndexOf(';') > 0)
            {
                strMacrosPath = strMacrosPath.Remove(strMacrosPath.IndexOf(';'));
            }
//#if DEBUG
            string strScript = @"namespace Tekla.Technology.Akit.UserScript
             {
                 public class Script
                 {
                     public static void Run(Tekla.Technology.Akit.IScript akit)
                     {
                         akit.Callback(""acmd_delete_selected_dr"", """", ""main_frame"");
                     }
                 }
             }";
// #else
//             string strScript = CServers.GetServers().GetDimDeleteScript();
// #endif
            File.WriteAllText(Path.Combine(strMacrosPath, "DeleteDimension.cs"), strScript);
            Tekla.Structures.Model.Operations.Operation.RunMacro("..\\" + "DeleteDimension.cs");
        }

        /// <summary>
        /// 清空所有的零件标记;
        /// </summary>
        /// <param name="viewBase"></param>
        public void ClearAllPartMark(TSD.View viewBase)
        {
            if(null==viewBase)
            {
                return;
            }

            DrawingObjectEnumerator allMarks = viewBase.GetAllObjects(typeof(TSD.MarkBase));
            
            DrawingHandler drawingHandler = new DrawingHandler();
            TSD.UI.DrawingObjectSelector DS = drawingHandler.GetDrawingObjectSelector();

            while (allMarks.MoveNext())
            {
                MarkBase markInDrawing = allMarks.Current as MarkBase;

                if (markInDrawing != null)
                {
                    DS.SelectObject(markInDrawing);
                }
            }

            string strMacrosPath = string.Empty;
            Tekla.Structures.TeklaStructuresSettings.GetAdvancedOption("XS_MACRO_DIRECTORY", ref strMacrosPath);
            if (strMacrosPath.IndexOf(';') > 0)
            {
                strMacrosPath = strMacrosPath.Remove(strMacrosPath.IndexOf(';'));
            }

            string strScript = @"namespace Tekla.Technology.Akit.UserScript
{
    public class Script
    {
        public static void Run(Tekla.Technology.Akit.IScript akit)
        {
            akit.Callback(""acmd_delete_selected_dr"", """", ""main_frame"");
        }
    }
}";
            File.WriteAllText(Path.Combine(strMacrosPath, "DeletePartMark.cs"), strScript);
            Tekla.Structures.Model.Operations.Operation.RunMacro("..\\" + "DeletePartMark.cs");

        }

        /// <summary>
        /// 绘制零件标记,该函数会自动调整零件标记防止重叠;
        /// </summary>
        /// <param name="mrMark"></param>
        /// <param name="strAttributeFile"></param>
        public void DrawMrMark(CMrMark mrMark, string strAttributeFile)
        {
            RectangleBoundingBox boundingBox = GetMarkBoundingBox(mrMark);
            mrMark.mTextBoundingBox = boundingBox;

            CMrMarkManager.GetInstance().AdjustMrMark(mrMark);
            CMrMarkManager.GetInstance().AppendMrMark(mrMark);

            DrawingHandler drawingHandler = new DrawingHandler();
            Drawing drawing = drawingHandler.GetActiveDrawing();

            Mark mark = new Mark(mrMark.mModelObject);
            mark.Placing = new LeaderLinePlacing(mrMark.mInsertPoint);
            mark.InsertionPoint = mrMark.mTextPoint;

            if (strAttributeFile != null)
            {
                mark.Attributes = new Mark.MarkAttributes(mrMark.mModelObject, strAttributeFile);
            }

            mrMark.mModelObject.Select();
            mrMark.mModelObject.Modify();
            mark.Insert();
            drawing.CommitChanges();
        }

        /// <summary>
        /// 获取零件标记包围盒的宽度;
        /// </summary>
        /// <param name="partInDrawing"></param>
        /// <param name="strFileAttribute"></param>
        /// <returns></returns>
        public double GetMarkBoundingBoxWidth(TSD.ModelObject partInDrawing,string strFileAttribute=null)
        {
            Mark mark = new Mark(partInDrawing);
            
            mark.Insert();
            RectangleBoundingBox boundingBox = mark.GetAxisAlignedBoundingBox();
            mark.Delete();

            return boundingBox.Width;
        }

        /// <summary>
        /// 获取零件标记文字的包围盒;
        /// </summary>
        /// <param name="modelObject"></param>
        /// <param name="markPoint"></param>
        /// <param name="textPoint"></param>
        /// <returns></returns>
        public RectangleBoundingBox GetMarkBoundingBox(CMrMark mrMark)
        {
            Mark mark = new Mark(mrMark.mModelObject);
            mark.Placing = new LeaderLinePlacing(mrMark.mInsertPoint);
            mark.InsertionPoint = mrMark.mTextPoint;

            mark.Insert();
            RectangleBoundingBox boundingBox=mark.GetAxisAlignedBoundingBox();
            mark.Delete();

            return boundingBox;
        }

        /// <summary>
        /// 绘制标注尺寸;
        /// </summary>
        /// <param name="viewBase"></param>
        /// <param name="pointList"></param>
        /// <param name="dimVector"></param>
        /// <param name="dimDistance"></param>
        /// <param name="strAttributeFile"></param>
        public void DrawDimensionSet(TSD.View viewBase,PointList pointList,Vector dimVector,double dimDistance,string strAttributeFile)
        {
            StraightDimensionSet.StraightDimensionSetAttributes attributes=null;

            if (strAttributeFile != null)
            {
                attributes = new StraightDimensionSet.StraightDimensionSetAttributes(null,strAttributeFile);
                bool bFlag=attributes.LoadAttributes(strAttributeFile);
            }
            if (attributes != null)
            {
                StraightDimensionSet XDimensions = new StraightDimensionSetHandler().CreateDimensionSet(viewBase, pointList, dimVector, dimDistance, attributes);
            }
            else
            {
                StraightDimensionSet XDimensions = new StraightDimensionSetHandler().CreateDimensionSet(viewBase, pointList, dimVector, dimDistance);
            }
        }

        /// <summary>
        /// 比较两个Double值的大小;
        /// </summary>
        /// <param name="dblValue1"></param>
        /// <param name="dblValue2"></param>
        /// <returns></returns>
        public int CompareTwoDoubleValue(double dblValue1,double dblValue2)
        {
            if(Math.Abs(dblValue1 - dblValue2) < 1)
            {
                return 0;
            }
            else if(dblValue1 > dblValue2)
            {
                return 1;
            }
            else
            {
                return -1;
            }
        }

        /// <summary>
        /// 判断指定的零部件是否在另一个零部件的包围盒中;
        /// </summary>
        /// <param name="mrPart"></param>
        /// <param name="mrOtherPart"></param>
        /// <returns></returns>
        public bool IsPartInOtherPartBox(CMrPart mrPart,CMrPart mrOtherPart)
        {
            if (null == mrPart || null == mrOtherPart)
            {
                return false;
            }

            Point maxOtherPartYPoint = mrOtherPart.GetMaxYPoint();
            Point minOtherPartYPoint = mrOtherPart.GetMinYPoint();

            Point minOtherPartXPoint = mrOtherPart.GetMinXPoint();
            Point maxOtherPartXPoint = mrOtherPart.GetMaxXPoint();

            Point maxYPoint = mrPart.GetMaxYPoint();
            Point minYPoint = mrPart.GetMinYPoint();
            Point maxXPoint = mrPart.GetMaxXPoint();
            Point minXPoint = mrPart.GetMinXPoint();

            if (CompareTwoDoubleValue(maxYPoint.Y, maxOtherPartYPoint.Y) <= 0 &&
                CompareTwoDoubleValue(minYPoint.Y, minOtherPartYPoint.Y) >= 0 &&
                CompareTwoDoubleValue(maxXPoint.X, maxOtherPartXPoint.X) <= 0 &&
                CompareTwoDoubleValue(minXPoint.X, minOtherPartXPoint.X) >= 0)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 判断点与直线的位置关系。0:点在直线上;1:点在直线上方;2:点在直线下方;
        /// </summary>
        /// <param name="point"></param>
        /// <param name="line1Point"></param>
        /// <param name="line2Point"></param>
        /// <returns></returns>
        public int IsThePointOnLine(Point point, Point line1Point, Point line2Point)
        {
            double k = (line1Point.Y - line2Point.Y) / (line1Point.X - line2Point.X);

            double b = line1Point.Y - k * line1Point.X;

            double dblValue = point.Y - (k * point.X + b);

            if (dblValue > CCommonPara.mDblError)
            {
                return 1;
            }
            else if (dblValue < CCommonPara.mDblError && dblValue > -CCommonPara.mDblError)
            {
                return 0;
            }
            else
            {
                return -1;
            }
        }

        /// <summary>
        /// 得到点与直线之间的距离，即垂线的长度;
        /// </summary>
        /// <param name="pt">给定的点</param>
        /// <param name="pt1">直线上的点1</param>
        /// <param name="pt2">直线上的点2</param>
        /// <returns></returns>
        public double ComputePointToLineDistance(Point pt, Point pt1, Point pt2)
        {
            Point intersectPt = ComputeFootPointToLine(pt, pt1, pt2);

            double distance = Math.Sqrt((pt.Y - intersectPt.Y) * (pt.Y - intersectPt.Y) + (pt.X - intersectPt.X) * (pt.X - intersectPt.X));

            return distance;
        }

        /// <summary>
        /// 计算点与直线的垂足点;
        /// </summary>
        /// <param name="pt"></param>
        /// <param name="pt1"></param>
        /// <param name="pt2"></param>
        /// <returns></returns>
        public Point ComputeFootPointToLine(Point pt, Point pt1, Point pt2)
        {
            Vector lineDerectNormal = new Vector(pt2.X - pt1.X, pt2.Y - pt1.Y, 0);

            Vector lineNormal = new Vector(-lineDerectNormal.Y, lineDerectNormal.X, 0);

            Point newPt = new Point(pt.X + 2 * lineNormal.X, pt.Y + 2 * lineNormal.Y, 0);

            Point intersectPt = ComputeTwoLineIntersectPoint(pt, newPt, pt1, pt2);

            return intersectPt;
        }

        /// <summary>
        /// 计算两点之间的距离;
        /// </summary>
        /// <param name="pt1"></param>
        /// <param name="pt2"></param>
        /// <returns></returns>
        public double ComputeTwoPointDistance(Point pt1, Point pt2)
        {
            double distance = Math.Sqrt((pt2.X - pt1.X) * (pt2.X - pt1.X) + (pt2.Y - pt1.Y) * (pt2.Y - pt1.Y) + (pt2.Z - pt1.Z) * (pt2.Z - pt1.Z));

            return distance;
        }

        /// <summary>
        /// 计算两直线的交点;
        /// </summary>
        /// <param name="line1Pt1"></param>
        /// <param name="line1Pt2"></param>
        /// <param name="line2Pt1"></param>
        /// <param name="line2Pt2"></param>
        /// <returns></returns>
        public Point ComputeTwoLineIntersectPoint(Point pt1, Point pt2, Point pt3, Point pt4)
        {
            //交点;
            Point intersectPt = new Point();

            //如果直线1的斜率不等于无穷;
            if (pt2.X - pt1.X != 0)
            {
                double k1 = (pt2.Y - pt1.Y) / (pt2.X - pt1.X);
                double b1 = pt2.Y - k1 * pt2.X;

                double k2 = (pt4.Y - pt3.Y) / (pt4.X - pt3.X);
                double b2 = pt4.Y - k2 * pt4.X;

                double x = 0.0;
                double y = 0.0;

                //如果线段2的斜率不等于无穷;
                if (pt4.X - pt3.X != 0)
                {
                    x = -(b2 - b1) / (k2 - k1);
                    y = k1 * x + b1;
                }
                else
                {
                    x = pt3.X;
                    y = k1 * x + b1;
                }

                intersectPt = new Point(x, y, 0);
            }
            //如果直线1的斜率是无穷;
            else
            {
                double x = 0.0;
                double y = 0.0;

                double k2 = (pt4.Y - pt3.Y) / (pt4.X - pt3.X);
                double b2 = pt4.Y - k2 * pt4.X;

                //如果线段2的斜率不等于无穷;
                if (pt4.X - pt3.X != 0)
                {
                    x = pt1.X;
                    y = k2 * x + b2;

                    intersectPt = new Point(x, y, 0);
                }
            }

            return intersectPt;
        }

        /// <summary>
        /// 获取当前选择的所有标注对象列表;
        /// </summary>
        /// <returns></returns>
        public List<CMrAssemblyDrawing> GetSelMrAssemblyDrawingList()
        {
            List<CMrAssemblyDrawing> mrAssemblyDrawingList = new List<CMrAssemblyDrawing>();

            DrawingHandler drawingHandler = new DrawingHandler();

            DrawingEnumerator selected = drawingHandler.GetDrawingSelector().GetSelected();

            while (selected.MoveNext())
            {
                AssemblyDrawing assemblyDrawing = selected.Current as AssemblyDrawing;
                if (assemblyDrawing != null)
                {
                    CMrAssemblyDrawing mrAssemblyDrawing = new CMrAssemblyDrawing(assemblyDrawing);

                    mrAssemblyDrawing.mName=assemblyDrawing.Name;
                    mrAssemblyDrawing.mMark = assemblyDrawing.Mark;
                    mrAssemblyDrawing.mTitle1 = assemblyDrawing.Title1;
                    mrAssemblyDrawing.mTitle2 = assemblyDrawing.Title2;
                    mrAssemblyDrawing.mTitle3 = assemblyDrawing.Title3;

                    mrAssemblyDrawingList.Add(mrAssemblyDrawing);
                }
            }
            return mrAssemblyDrawingList;
        }
 
        /// <summary>
        /// 初始化所有零件的包围盒;
        /// </summary>
        public void InitViewBox()
        {
            CCommonPara.mViewMinX = double.MaxValue;
            CCommonPara.mViewMaxX = double.MinValue;
            CCommonPara.mViewMinY = double.MaxValue;
            CCommonPara.mViewMaxY = double.MinValue;
        }

        /// <summary>
        /// 根据零件的最大最小值来更新所有零件的包围盒;
        /// </summary>
        /// <param name="mrPart"></param>
        public void UpdateViewBox(CMrPart mrPart) 
        {
            Point minXPoint = mrPart.GetMinXPoint();
            Point maxXPoint = mrPart.GetMaxXPoint();
            Point minYPoint = mrPart.GetMinYPoint();
            Point maxYPoint = mrPart.GetMaxYPoint();

            if (minXPoint.X < CCommonPara.mViewMinX)
            {
                CCommonPara.mViewMinX = minXPoint.X;
            }
            if (maxXPoint.X > CCommonPara.mViewMaxX)
            {
                CCommonPara.mViewMaxX = maxXPoint.X;
            }
            if (minYPoint.Y < CCommonPara.mViewMinY)
            {
                CCommonPara.mViewMinY = minYPoint.Y;
            }
            if (maxYPoint.Y > CCommonPara.mViewMaxY)
            {
                CCommonPara.mViewMaxY = maxYPoint.Y;
            }
        }

        /// <summary>
        /// 判断射线1与线段是否相交;
        /// </summary>
        /// <param name="line1"></param>
        /// <param name="line2"></param>
        /// <returns></returns>
        public bool IsLineIntersectWithLineSegment(Point pt1,Point pt2,Point pt3,Point pt4,ref Point intersectPt) 
        {
            try
            {
                ServiceReference.Point intersectpt = new ServiceReference.Point();
                bool re = CServers.GetServers().IsLineIntersectWithLineSegment(out intersectpt,
                                                                            TKPointToPt(pt1),
                                                                            TKPointToPt(pt2), TKPointToPt(pt3),
                                                                            TKPointToPt(pt4));
                intersectPt.X = intersectpt.X;
                intersectPt.Y = intersectpt.Y;
                intersectPt.Z = intersectpt.Z;
                return re;
            }
            catch
            {
                mDimTools.IsOut = true;
                return false;
            }

        }

        /// <summary>
        /// 判断直线是否在零件包围盒的外侧,并且与其相交;
        /// </summary>
        /// <param name="line1">射线</param>
        /// <param name="mrPart">零部件</param>
        /// <param name="nFlag">1：射线向左侧发散。2：射线向右侧发散</param>
        /// <returns></returns>
        public bool IsLineIntersectWithPart(Point pt1,Point pt2,CMrPart mrPart,int nFlag)
        {
            int nIntersectCount = 0;

            List<Point> pointList = mrPart.GetPointList();

            Point ptStart = pointList[0];
            Point ptEnd = null;

            Point intersectPt = new Point();

            for(int i=1;i<pointList.Count;i++)
            {
                ptEnd = pointList[i];

                bool bRes = IsLineIntersectWithLineSegment(pt1,pt2,ptStart,ptEnd,ref intersectPt);

                if(bRes)
                {
                    if(nFlag==1)
                    {
                        if(intersectPt.X < pt2.X)
                        {
                            nIntersectCount++;
                        }
                    }
                    if(nFlag==2)
                    {
                        if (intersectPt.X > pt2.X)
                        {
                            nIntersectCount++;
                        }
                    }
                }

                ptStart = ptEnd;
            }

            if(nIntersectCount==2)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 判断零件是否是角钢;
        /// </summary>
        /// <param name="mrPart"></param>
        /// <returns></returns>
        public bool IsPartTheAngleSteel(CMrPart mrPart)
        {
            String strProfile = mrPart.mPartInModel.Profile.ProfileString;

            strProfile.ToUpper();

            if (strProfile[0].Equals('L'))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 判断零件是否是板类;
        /// </summary>
        /// <param name="mrPart"></param>
        /// <returns></returns>
        public bool IsPartThePlate(CMrPart mrPart)
        {
            String strProfile = mrPart.mPartInModel.Profile.ProfileString;

            strProfile.ToUpper();

            if (strProfile[0].Equals('B'))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 判断零部件是否是有孔的水平支撑板;
        /// </summary>
        /// <param name="mrPart"></param>
        /// <returns></returns>
        public bool IsPartTheSupportPlate(CMrPart mrPart)
        {
            if(IsPartTheAngleSteel(mrPart))
            {
                return false;
            }

            Vector normal = mrPart.mNormal;

            if (CDimTools.GetInstance().IsTwoVectorParallel(normal, new Vector(0, 0, 1)) && mrPart.IsHaveBolt())
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 判断零件是否是型钢内牛腿支撑等;
        /// </summary>
        /// <param name="mrPart"></param>
        /// <returns></returns>
        public bool IsPartTheHProfileSheet(CMrPart mrPart)
        {
            String strProfile = mrPart.mPartInModel.Profile.ProfileString;

            strProfile.ToUpper();

            if (strProfile[0].Equals('H'))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 判断倾斜的螺钉组中所有的螺钉是否在一条直线上;
        /// </summary>
        /// <param name="mrBoltArray"></param>
        /// <returns></returns>
        public bool IsObliqueBoltOnLine(CMrBoltArray mrBoltArray)
        {
            if(mrBoltArray.mBoltArrayShapeType != MrBoltArrayShapeType.OBLIQUELINE)
            {
                return false;
            }

            Point boltPt = mrBoltArray.GetBoltPointList()[0];

            Point firstPt = mrBoltArray.mFirstPoint;
            Point secondPt = mrBoltArray.mSecondPoint;

            double k = (secondPt.Y - firstPt.Y) / (secondPt.X - firstPt.X);
            double b = boltPt.Y - k * boltPt.X;

            foreach(Point pt in mrBoltArray.GetBoltPointList())
            {
                if (Math.Abs((k * pt.X + b) - pt.Y) > 0.5)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 根据宏来创建零件标记;
        /// </summary>
        public void DrawMarkByMacro()
        {
            string strMacrosPath = string.Empty;
            Tekla.Structures.TeklaStructuresSettings.GetAdvancedOption("XS_MACRO_DIRECTORY", ref strMacrosPath);
            if (strMacrosPath.IndexOf(';') > 0)
            {
                strMacrosPath = strMacrosPath.Remove(strMacrosPath.IndexOf(';'));
            }

            string strScript = @"namespace Tekla.Technology.Akit.UserScript
{
    public class Script
    {
        public static void Run(Tekla.Technology.Akit.IScript akit)
        {
            akit.Callback(""acmd_create_marks_selected"", """", ""main_frame"");
        }
    }
}";
            File.WriteAllText(Path.Combine(strMacrosPath, "AddPartMark.cs"), strScript);
            Tekla.Structures.Model.Operations.Operation.RunMacro("..\\" + "AddPartMark.cs");

            DrawingHandler drawingHandler = new DrawingHandler();
            TSD.UI.DrawingObjectSelector DS = drawingHandler.GetDrawingObjectSelector();
            DS.UnselectAllObjects();
        }
    }
}
