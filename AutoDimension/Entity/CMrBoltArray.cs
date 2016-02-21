using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tekla.Structures.Model;
using Tekla.Structures.Drawing;
using Tekla.Structures.Geometry3d;

using TSM=Tekla.Structures.Model;
using TSD = Tekla.Structures.Drawing;

namespace AutoDimension.Entity
{
    /// <summary>
    /// 螺钉组类;
    /// </summary>
    public class CMrBoltArray:CMrEntity
    {
        /// <summary>
        /// 该螺钉组中的螺钉对象;
        /// </summary>
        private List<CMrBolt> mMrBoltList=new List<CMrBolt>();

        /// <summary>
        /// 与该螺钉组对象的Tekla中的螺钉组;
        /// </summary>
        public TSM.BoltArray mBoltArrayInModel;

        /// <summary>
        /// 在Drawing视图中属于该螺钉组的对象;
        /// </summary>
        public TSD.Bolt mBoltInDrawing;

        /// <summary>
        /// 螺钉组的形状类型;
        /// </summary>
        public MrBoltArrayShapeType mBoltArrayShapeType;

        /// <summary>
        /// 螺栓组的起始点;
        /// </summary>
        public Point mFirstPoint;

        /// <summary>
        /// 螺栓组的终止点;
        /// </summary>
        public Point mSecondPoint;

        /// <summary>
        /// 螺栓组的法向量;
        /// </summary>
        public Vector mNormal;

        /// <summary>
        /// 螺栓组的方向向量;
        /// </summary>
        public Vector mDirectionNormal
        {
            get
            {
                return new Vector(mSecondPoint.X - mFirstPoint.X, mSecondPoint.Y - mFirstPoint.Y, mSecondPoint.Z - mFirstPoint.Z);
            }
        }

        /// <summary>
        /// 螺钉组关联的信息;
        /// </summary>
        private CMrBoltArrayInfo mMrBoltArrayInfo;

        /// <summary>
        /// 螺钉组的构造函数;
        /// </summary>
        /// <param name="boltArray"></param>
        public CMrBoltArray(TSM.BoltArray boltArray,TSD.Bolt boltInDrawing)
        {
            mName = "BoltArray";
            mBoltArrayInModel = boltArray;
            mBoltInDrawing = boltInDrawing;

            mBoltArrayShapeType = MrBoltArrayShapeType.ARRAY;
            mMrBoltArrayInfo = new CMrBoltArrayInfo(this);
        }

        /// <summary>
        /// 螺钉组的链表中添加螺钉;
        /// </summary>
        /// <param name="mrBolt"></param>
        public void AppendBolts(CMrBolt mrBolt)
        {
            mMrBoltList.Add(mrBolt);
        }

        /// <summary>
        /// 获取螺钉组中的所有螺钉;
        /// </summary>
        /// <returns></returns>
        public List<CMrBolt> GetMrBoltList()
        {
            return mMrBoltList;
        }

        /// <summary>
        /// 计算BoltArray的形状;
        /// </summary>
        public void InitBoltArrayShape()
        {
            if (mMrBoltList.Count() == 0)
            {
                return;
            }

            MrSlopeType mrSlopeType = CDimTools.GetInstance().JudgeLineSlope(mFirstPoint,mSecondPoint);

            if (mrSlopeType == MrSlopeType.EQUAL_ZERO || mrSlopeType == MrSlopeType.INFINITY||mMrBoltList.Count==0)
            {
                mBoltArrayShapeType = MrBoltArrayShapeType.ARRAY;
            }
            else if(mrSlopeType==MrSlopeType.MORETHAN_ZERO || mrSlopeType==MrSlopeType.LESSTHAN_ZERO)
            {
                mBoltArrayShapeType = MrBoltArrayShapeType.OBLIQUELINE;
            }
        }

        /// <summary>
        /// 获取螺钉组的属性信息;
        /// </summary>
        /// <returns></returns>
        public CMrBoltArrayInfo GetMrBoltArrayInfo()
        {
            return mMrBoltArrayInfo;
        }

        /// <summary>
        /// 获取螺钉组中所有螺钉位置点的链表;
        /// </summary>
        /// <returns></returns>
        public List<Point> GetBoltPointList()
        {
            List<Point> pointList = new List<Point>();

            foreach (CMrBolt mrBolt in mMrBoltList)
            {
                pointList.Add(mrBolt.mPosition);
            }

            return pointList;
        }

        /// <summary>
        /// 获得螺钉中Y值最小的点;
        /// </summary>
        /// <returns></returns>
        public Point GetMinYPoint()
        {
            List<Point> pointList = new List<Point>();

            foreach (CMrBolt mrBolt in mMrBoltList)
            {
                pointList.Add(mrBolt.mPosition);
            }
            return CDimTools.GetInstance().GetMinYPoint(pointList);
        }

        /// <summary>
        /// 获得螺钉中Y值最大的点;
        /// </summary>
        /// <returns></returns>
        public Point GetMaxYPoint()
        {
            List<Point> pointList = new List<Point>();

            foreach (CMrBolt mrBolt in mMrBoltList)
            {
                pointList.Add(mrBolt.mPosition);
            }

            return CDimTools.GetInstance().GetMaxYPoint(pointList);
        }

        /// <summary>
        /// 获取X值最小的点;
        /// </summary>
        /// <returns></returns>
        public Point GetMinXPoint()
        {
            List<Point> pointList = new List<Point>();

            foreach (CMrBolt mrBolt in mMrBoltList)
            {
                pointList.Add(mrBolt.mPosition);
            }

            Point minXPoint = CDimTools.GetInstance().GetMinXPoint(pointList);

            return minXPoint;
        }

        /// <summary>
        /// 获取最小的X值与最小的Y值;
        /// </summary>
        /// <returns></returns>
        public Point GetMinXMinYPoint()
        {
            List<Point> pointList = new List<Point>();

            foreach (CMrBolt mrBolt in mMrBoltList)
            {
                pointList.Add(mrBolt.mPosition);
            }

            Point minXPoint = CDimTools.GetInstance().GetMinXPoint(pointList);

            List<Point> minXPointList = new List<Point>();

            foreach (Point point in pointList)
            {
                if (CDimTools.GetInstance().CompareTwoDoubleValue(point.X, minXPoint.X) == 0)
                {
                    minXPointList.Add(point);
                }
            }

            Point minXAndMinYPoint = CDimTools.GetInstance().GetMinYPoint(minXPointList);

            return minXAndMinYPoint;
        }

        /// <summary>
        /// 获取最小的X值与最大的Y值;
        /// </summary>
        /// <returns></returns>
        public Point GetMinXMaxYPoint()
        {
            List<Point> pointList = new List<Point>();

            foreach (CMrBolt mrBolt in mMrBoltList)
            {
                pointList.Add(mrBolt.mPosition);
            }

            Point minXPoint = CDimTools.GetInstance().GetMinXPoint(pointList);

            List<Point> minXPointList = new List<Point>();

            foreach (Point point in pointList)
            {
                if (CDimTools.GetInstance().CompareTwoDoubleValue(point.X, minXPoint.X) == 0)
                {
                    minXPointList.Add(point);
                }
            }

            Point minXAndMaxYPoint = CDimTools.GetInstance().GetMaxYPoint(minXPointList);

            return minXAndMaxYPoint;
        }

        /// <summary>
        /// 获取最大Y和最大X的点;
        /// </summary>
        /// <returns></returns>
        public Point GetMaxYMaxXPoint()
        {
            List<Point> pointList = new List<Point>();

            foreach (CMrBolt mrBolt in mMrBoltList)
            {
                pointList.Add(mrBolt.mPosition);
            }

            Point maxYPoint = CDimTools.GetInstance().GetMaxYPoint(pointList);

            List<Point> maxYPointList = new List<Point>();

            foreach (Point point in pointList)
            {
                if (CDimTools.GetInstance().CompareTwoDoubleValue(point.Y, maxYPoint.Y) == 0)
                {
                    maxYPointList.Add(point);
                }
            }

            Point maxYAndMaxXPoint = CDimTools.GetInstance().GetMaxXPoint(maxYPointList);

            return maxYAndMaxXPoint;
        }

        /// <summary>
        /// 获得最大Y和最小X的点;
        /// </summary>
        /// <returns></returns>
        public Point GetMaxYMinXPoint()
        {
            List<Point> pointList = new List<Point>();

            foreach (CMrBolt mrBolt in mMrBoltList)
            {
                pointList.Add(mrBolt.mPosition);
            }

            Point maxYPoint = CDimTools.GetInstance().GetMaxYPoint(pointList);

            List<Point> maxYPointList = new List<Point>();

            foreach (Point point in pointList)
            {
                if (CDimTools.GetInstance().CompareTwoDoubleValue(point.Y, maxYPoint.Y) == 0)
                {
                    maxYPointList.Add(point);
                }
            }

            Point maxYAndMinXPoint = CDimTools.GetInstance().GetMinXPoint(maxYPointList);

            return maxYAndMinXPoint;
        }

        /// <summary>
        /// 获取最大Y和最大X的点;
        /// </summary>
        /// <returns></returns>
        public Point GetMinYMaxXPoint()
        {
            List<Point> pointList = new List<Point>();

            foreach (CMrBolt mrBolt in mMrBoltList)
            {
                pointList.Add(mrBolt.mPosition);
            }

            Point minYPoint = CDimTools.GetInstance().GetMinYPoint(pointList);

            List<Point> maxYPointList = new List<Point>();

            foreach (Point point in pointList)
            {
                if (CDimTools.GetInstance().CompareTwoDoubleValue(point.Y, minYPoint.Y) == 0)
                {
                    maxYPointList.Add(point);
                }
            }

            Point minYAndMaxXPoint = CDimTools.GetInstance().GetMaxXPoint(maxYPointList);

            return minYAndMaxXPoint;
        }
        /// <summary>
        /// 获取X值最大的点;
        /// </summary>
        /// <returns></returns>
        public Point GetMaxXPoint()
        {
            List<Point> pointList = new List<Point>();

            foreach (CMrBolt mrBolt in mMrBoltList)
            {
                pointList.Add(mrBolt.mPosition);
            }

            Point maxXPoint = CDimTools.GetInstance().GetMaxXPoint(pointList);

            return maxXPoint;
        }

        /// <summary>
        /// 获取x值和y值都最大的点;
        /// </summary>
        /// <returns></returns>
        public Point GetMaxXMaxYPoint()
        {
            List<Point> pointList = new List<Point>();

            foreach (CMrBolt mrBolt in mMrBoltList)
            {
                pointList.Add(mrBolt.mPosition);
            }

            Point maxXPoint = CDimTools.GetInstance().GetMaxXPoint(pointList);

            List<Point> maxXPointList = new List<Point>();

            foreach (Point point in pointList)
            {
                if (CDimTools.GetInstance().CompareTwoDoubleValue(point.X, maxXPoint.X) == 0)
                {
                    maxXPointList.Add(point);
                }
            }

            Point maxXAndMaxYPoint = CDimTools.GetInstance().GetMaxYPoint(maxXPointList);

            return maxXAndMaxYPoint;
        }

        /// <summary>
        /// 获取x最大值y最小值;
        /// </summary>
        /// <returns></returns>
        public Point GetMaxXMinYPoint()
        {
            List<Point> pointList = new List<Point>();

            foreach (CMrBolt mrBolt in mMrBoltList)
            {
                pointList.Add(mrBolt.mPosition);
            }

            Point maxXPoint = CDimTools.GetInstance().GetMaxXPoint(pointList);

            List<Point> maxXPointList = new List<Point>();

            foreach (Point point in pointList)
            {
                if (CDimTools.GetInstance().CompareTwoDoubleValue(point.X, maxXPoint.X) == 0)
                {
                    maxXPointList.Add(point);
                }
            }

            Point maxXAndMinYPoint = CDimTools.GetInstance().GetMinYPoint(maxXPointList);

            return maxXAndMinYPoint;
        }

        /// <summary>
        /// 获得螺钉中X值最小的点链表;
        /// </summary>
        /// <returns></returns>
        public List<Point> GetMinXPointList()
        {
            List<Point> pointList = new List<Point>();

            foreach (CMrBolt mrBolt in mMrBoltList)
            {
                pointList.Add(mrBolt.mPosition);
            }

            List<Point> minXPointList = new List<Point>();

            Point minXPoint=CDimTools.GetInstance().GetMinXPoint(pointList);

            foreach (Point point in pointList)
            {
                if (Math.Abs(point.X - minXPoint.X) < CCommonPara.mDblError)
                {
                    minXPointList.Add(point);
                }
            }
            return minXPointList;
        }

        /// <summary>
        /// 获取x值最大的点链表;
        /// </summary>
        /// <returns></returns>
        public List<Point> GetMaxXPointList()
        {
            List<Point> pointList = new List<Point>();

            foreach (CMrBolt mrBolt in mMrBoltList)
            {
                pointList.Add(mrBolt.mPosition);
            }

            List<Point> maxXPointList = new List<Point>();

            Point maxXPoint = CDimTools.GetInstance().GetMaxXPoint(pointList);

            foreach (Point point in pointList)
            {
                if (Math.Abs(point.X - maxXPoint.X) < CCommonPara.mDblError)
                {
                    maxXPointList.Add(point);
                }
            }
            return maxXPointList;
        }

        /// <summary>
        /// 获得螺钉中Y值最小的点链表;
        /// </summary>
        /// <returns></returns>
        public List<Point> GetMinYPointList()
        {
            List<Point> pointList = new List<Point>();

            foreach (CMrBolt mrBolt in mMrBoltList)
            {
                pointList.Add(mrBolt.mPosition);
            }

            List<Point> minYPointList = new List<Point>();

            Point minYPoint = CDimTools.GetInstance().GetMinYPoint(pointList);

            foreach (Point point in pointList)
            {
                if (Math.Abs(point.Y - minYPoint.Y) < CCommonPara.mDblError)
                {
                    minYPointList.Add(point);
                }
            }

            return minYPointList;
        }

        /// <summary>
        /// 获取螺钉组中Y值最大的点链表;
        /// </summary>
        /// <returns></returns>
        public List<Point> GetMaxYPointList()
        {
            List<Point> pointList = new List<Point>();

            foreach (CMrBolt mrBolt in mMrBoltList)
            {
                pointList.Add(mrBolt.mPosition);
            }

            List<Point> maxYPointList = new List<Point>();

            Point maxYPoint = CDimTools.GetInstance().GetMaxYPoint(pointList);

            foreach (Point point in pointList)
            {
                if (Math.Abs(point.Y - maxYPoint.Y) < CCommonPara.mDblError)
                {
                    maxYPointList.Add(point);
                }
            }
            return maxYPointList;
        }
    }
}
