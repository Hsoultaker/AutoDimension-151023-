using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tekla.Structures.Drawing;
using Tekla.Structures.Geometry3d;

using TSD = Tekla.Structures.Drawing;

namespace AutoDimension.Entity
{
    /// <summary>
    /// 门式框架梁的样式;
    /// </summary>
    public enum MrBeamDoorType
    {
        TypeNormal = 1,      //类型1,门式框架里面两边的梁;
        TypeMiddle1 = 2,     //类型2,门式框架里面中间的梁,上翼板是往两边向下倾斜的;
        TypeMiddle2 = 3,     //类型3,门式框架里面中间的梁,上翼板是往两边向上倾斜的;
        TypeMiddle3 = 4,     //类型4,门式框架里面中间的梁,上翼板是直的一整块板;   
    };

    /// <summary>
    /// 门式框架结构梁的管理器;
    /// </summary>
    public class CMrBeamDoorManager
    {
        /// <summary>
        /// 该映射表主要记录与顶部板法向相同的零部件需要与周边零部件标注的关系;
        /// </summary>
        public Dictionary<CMrPart, List<CMrPart>> mDicPartToPartList=new Dictionary<CMrPart, List<CMrPart>>();

        /// <summary>
        /// 主部件对象;
        /// </summary>
        public CMrMainBeam mMainBeam = null;

        /// <summary>
        /// 主部件上方的板部件,上翼板;
        /// </summary>
        public CMrPart mTopBeam = null;

        /// <summary>
        /// 主部件下方的板部件,下翼板;
        /// </summary>
        public CMrPart mBottonBeam = null;

        /// <summary>
        /// 主部件最左侧的板部件,一般该部件会与mTopBeam垂直;
        /// </summary>
        public CMrPart mLeftBeam = null;

        /// <summary>
        /// 主部件最右侧的板部件,一般该部件会与mTopBeam垂直;
        /// </summary>
        public CMrPart mRightBeam = null;

        /// <summary>
        /// 对于上翼板是向两边弯的情况左下边的翼板;
        /// </summary>
        public CMrPart mLeftBottomBeam = null;

        /// <summary>
        /// 对于上翼板是向两边弯的情况右下边的翼板;
        /// </summary>
        public CMrPart mRightBottomBeam = null;

        /// <summary>
        /// 对于上翼板向两边弯的情况左上边的法向;
        /// </summary>
        public Vector mLeftTopVector = null;

        /// <summary>
        /// 对于上翼板向两边弯的情况右上边的法向;
        /// </summary>
        public Vector mRightTopVector = null;

        /// <summary>
        /// 上翼板中间最大点;
        /// </summary>
        public Point mMidMaxPoint = new Point();

        /// <summary>
        /// 当前门式框架梁的类型;
        /// </summary>
        public MrBeamDoorType mType = MrBeamDoorType.TypeNormal;

        /// <summary>
        /// 需要进行标注的零部件链表;
        /// </summary>
        public List<CMrPart> mDimPartList = new List<CMrPart>();

        /// <summary>
        /// 单一实例;
        /// </summary>
        private static CMrBeamDoorManager mInstance = null;

        /// <summary>
        /// 获取单例;
        /// </summary>
        /// <returns></returns>
        public static CMrBeamDoorManager GetInstance()
        {
            if (null == mInstance)
            {
                mInstance = new CMrBeamDoorManager();
            }

            return mInstance;
        }

        /// <summary>
        /// 构建门式框架梁的拓扑结构;
        /// </summary>
        public void BuildBeamDoorTopo(List<CMrPart> mrPartList)
        {
            ClearData();

            //先赋值主梁;
            mMainBeam = CMrMainBeam.GetInstance();

            foreach (CMrPart mrPart in mrPartList)
            {
                if (mrPart == mMainBeam)
                {
                    continue;
                }

                JudgeLeftBeam(mrPart);
                JudgeRightBeam(mrPart);
                JudgeTopBeam(mrPart);
                JudgeBottomBeam(mrPart);
                JudgeLeftAndRightBottomBeam(mrPart);
            }
            if (mBottonBeam == mTopBeam && mLeftBottomBeam != null && mRightBottomBeam != null)
            {//这个是一块板的形式;
                mBottonBeam = null;

                UpdatePartBoxPoint(mLeftBottomBeam);
                UpdatePartBoxPoint(mRightBottomBeam);

                ComputeTypeMiddleParas();
            }
            else
            {//这个是三块板的形式，下翼缘part不为空;
                mType = MrBeamDoorType.TypeNormal;

                UpdatePartBoxPoint(mTopBeam);
                UpdatePartBoxPoint(mBottonBeam);
                UpdatePartBoxPoint(mMainBeam);
            }
        }

        /// <summary>
        /// 初始化;
        /// </summary>
        private void ClearData()
        {
            mLeftBeam = null;
            mRightBeam = null;
            mTopBeam = null;
            mBottonBeam = null;
            mDimPartList.Clear();
        }

        /// <summary>
        /// 判断是否是最左边的板部件;
        /// </summary>
        /// <param name="mrPart"></param>
        /// <returns></returns>
        public void JudgeLeftBeam(CMrPart mrPart)
        {
            if (mLeftBeam == null)
            {
                mLeftBeam = mrPart;

                return;
            }
            if (CDimTools.GetInstance().IsTwoVectorParallel(mrPart.mNormal, new Vector(0, 1, 0)))
            {
                return;
            }

            Point minXPoint = mrPart.GetMinXPoint();
            
            Point leftMinXPoint = mLeftBeam.GetMinXPoint();

            if (CDimTools.GetInstance().CompareTwoDoubleValue(minXPoint.X, leftMinXPoint.X) < 0)
            {
                mLeftBeam = mrPart;
            }
        }

        /// <summary>
        /// 判断是否是最右侧的板部件;
        /// </summary>
        /// <param name="mrPart"></param>
        public void JudgeRightBeam(CMrPart mrPart)
        {
            if (mRightBeam == null)
            {
                mRightBeam = mrPart;

                return;
            }
            if (CDimTools.GetInstance().IsTwoVectorParallel(mrPart.mNormal, new Vector(0, 1, 0)))
            {
                return;
            }

            Point maxXPoint = mrPart.GetMaxXPoint();
            Point rightMaxXPoint = mRightBeam.GetMaxXPoint();

            if (CDimTools.GetInstance().CompareTwoDoubleValue(maxXPoint.X, rightMaxXPoint.X) > 0)
            {
                mRightBeam = mrPart;
            }
        }

        /// <summary>
        /// 判断门式框架顶部的梁;
        /// </summary>
        /// <param name="mrPart"></param>
        public void JudgeTopBeam(CMrPart mrPart)
        {
            double dblWidth = mrPart.GetXSpaceValue();

            double mainBeamWidth = mMainBeam.GetXSpaceValue();

            //顶部梁的长度应该大于主梁的五分之四;
            if (dblWidth < 4 * mainBeamWidth / 5.0)
            {
                return;
            }

            if (mTopBeam == null)
            {
                mTopBeam = mrPart;

                return;
            }

            Point maxYPoint = mrPart.GetMaxYPoint();
            Point topMaxYPoint = mTopBeam.GetMaxYPoint();

            if (CDimTools.GetInstance().CompareTwoDoubleValue(maxYPoint.Y, topMaxYPoint.Y) > 0)
            {
                mTopBeam = mrPart;
            }
        }

        /// <summary>
        /// 判断门式框架底部的梁;
        /// </summary>
        public void JudgeBottomBeam(CMrPart mrPart)
        {
            double dblWidth = mrPart.GetXSpaceValue();

            double mainBeamWidth = mMainBeam.GetXSpaceValue();

            //顶部梁的长度应该大于主梁的五分之四;
            if (dblWidth < 4 * mainBeamWidth / 5.0)
            {
                return;
            }

            if (mBottonBeam == null)
            {
                mBottonBeam = mrPart;

                return;
            }

            Point maxYPoint = mrPart.GetMaxYPoint();

            Point bottomMaxYPoint = mBottonBeam.GetMaxYPoint();

            if (CDimTools.GetInstance().CompareTwoDoubleValue(maxYPoint.Y, bottomMaxYPoint.Y) < 0)
            {
                mBottonBeam = mrPart;
            }
        }

        /// <summary>
        /// 判断第三种情况中左下边的板;
        /// </summary>
        /// <param name="mrPart"></param>
        public void JudgeLeftAndRightBottomBeam(CMrPart mrPart)
        {
            double dblWidth = mrPart.GetXSpaceValue();

            double mainBeamWidth = mMainBeam.GetXSpaceValue();

            //左下边的梁与右下边的梁长度小于总长的1/2，大于总长的1/3;
            if (dblWidth < mainBeamWidth / 2.0 && dblWidth > mainBeamWidth / 3)
            {
                Point minXPoint = mrPart.GetMinXPoint();
                Point maxXPoint = mrPart.GetMaxXPoint();

                if (maxXPoint.X < mainBeamWidth / 2.0)
                {
                    mLeftBottomBeam = mrPart;
                }
                if (minXPoint.X > mainBeamWidth / 2.0)
                {
                    mRightBottomBeam = mrPart;
                }
            }
        }

        /// <summary>
        /// 把顶视图需要标注的零部件添加到管理器的链表中;
        /// </summary>
        /// <param name="mrPart"></param>
        public void AppendUpDimPart(CMrPart mrPart)
        {
            mDimPartList.Add(mrPart);
        }

        /// <summary>
        /// 得到与给定点最相近的零部件;
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public CMrPart GetMostNearPart(Point point)
        {
            CMrPart mNearPart = null;

            double dblNearDistance = double.MaxValue;

            foreach (CMrPart mrPart in mDimPartList)
            {
                Point pt1 = mrPart.mLeftTopPoint;
                
                Point pt2 = mrPart.mLeftBottomPoint;

                double distance = CDimTools.GetInstance().ComputePointToLineDistance(point, pt1, pt2);

                if (distance < dblNearDistance)
                {
                    mNearPart = mrPart;

                    dblNearDistance = distance;
                }
            }

            return mNearPart;
        }

        /// <summary>
        /// 得到顶板向上的标注向量;
        /// </summary>
        /// <returns></returns>
        public Vector GetTopBeamUpDimVector()
        {
            CMrPart topBeam = CMrBeamDoorManager.GetInstance().mTopBeam;

            Point leftTopPt = topBeam.mLeftTopPoint;
            Point rightTopPt = topBeam.mRightTopPoint;

            MrSlopeType slopeType = CDimTools.GetInstance().JudgeLineSlope(leftTopPt, rightTopPt);

            Vector normal = new Vector(rightTopPt.X - leftTopPt.X, rightTopPt.Y - leftTopPt.Y, 0);

            Vector upDimVector = new Vector(normal.Y, -normal.X, 0);

            if (slopeType == MrSlopeType.MORETHAN_ZERO)
            {
                upDimVector.X = -Math.Abs(upDimVector.X);
                upDimVector.Y = Math.Abs(upDimVector.Y);
                upDimVector.Z = 0;
            }
            else
            {
                upDimVector.X = Math.Abs(upDimVector.X);
                upDimVector.Y = Math.Abs(upDimVector.Y);
                upDimVector.Z = 0;
            }

            return upDimVector;
        }

        /// <summary>
        /// 得到左侧挡板零件向左的标注向量;
        /// </summary>
        /// <returns></returns>
        public Vector GetLeftBeamLeftDimVector()
        {
            Vector normal = mLeftBeam.mNormal;

            Vector dimVector = new Vector();

            //根据挡板的斜率来判断标注的方向;
            MrSlopeType slopeType = CDimTools.GetInstance().JudgeLineSlope(mLeftBeam.mLeftTopPoint, mLeftBeam.mLeftBottomPoint);

            if (slopeType == MrSlopeType.MORETHAN_ZERO)
            {
                dimVector = new Vector(-Math.Abs(normal.X), Math.Abs(normal.Y), 0);
            }
            else
            {
                dimVector = new Vector(-Math.Abs(normal.X), -Math.Abs(normal.Y), 0);
            }

            return dimVector;
        }

        /// <summary>
        /// 得到右侧挡板零件向右的标注向量;
        /// </summary>
        /// <returns></returns>
        public Vector GetRightBeamRightDimVector()
        {
            Vector normal = mRightBeam.mNormal;

            Vector dimVector = new Vector();

            //根据挡板的斜率来判断标注的方向;
            MrSlopeType slopeType = CDimTools.GetInstance().JudgeLineSlope(mRightBeam.mRightTopPoint, mRightBeam.mRightBottomPoint);

            if (slopeType == MrSlopeType.MORETHAN_ZERO)
            {
                dimVector = new Vector(Math.Abs(normal.X), -Math.Abs(normal.Y), 0);
            }
            else
            {
                dimVector = new Vector(Math.Abs(normal.X), Math.Abs(normal.Y), 0);
            }

            return dimVector;
        }

        /// <summary>
        /// 更新上部板和下部板的四个角点;
        /// </summary>
        public void UpdatePartBoxPoint(CMrPart mrPart)
        {
            Point minX = mrPart.GetMinXPoint();
            Point maxX = mrPart.GetMaxXPoint();
            Point minY = mrPart.GetMinYPoint();
            Point maxY = mrPart.GetMaxYPoint();
            
            Point minXmaxY = mrPart.GetMinXMaxYPoint();
            Point minXminY = mrPart.GetMinXMinYPoint();
            Point maxXmaxY = mrPart.GetMaxXMaxYPoint();
            Point maxXminY = mrPart.GetMaxXMinYPoint();

            MrSlopeType mrYSlopeType = CDimTools.GetInstance().JudgeLineSlope(maxY, minY);

            //1.如果斜率小于零;
            if (mrYSlopeType == MrSlopeType.LESSTHAN_ZERO)
            {
                if (CDimTools.GetInstance().CompareTwoDoubleValue(minXmaxY.X, minXminY.X) == 0
                    && CDimTools.GetInstance().CompareTwoDoubleValue(minXmaxY.Y, minXminY.Y) != 0)
                {
                    mrPart.mLeftBottomPoint = minXminY;
                    mrPart.mLeftTopPoint = minXmaxY;

                    if (CDimTools.GetInstance().CompareTwoDoubleValue(maxXmaxY.X, maxXminY.X) == 0
                        && CDimTools.GetInstance().CompareTwoDoubleValue(maxXmaxY.Y, maxXminY.Y) != 0)
                    {
                        mrPart.mRightTopPoint = maxXmaxY;
                        mrPart.mRightBottomPoint = maxXminY;
                    }
                    else
                    {
                        mrPart.mRightTopPoint = maxX;
                        mrPart.mRightBottomPoint = minY;
                    }
                }
                else
                {
                    mrPart.mLeftBottomPoint = minX;
                    mrPart.mLeftTopPoint = maxY;

                    if (CDimTools.GetInstance().CompareTwoDoubleValue(maxXmaxY.X, maxXminY.X) == 0
                         && CDimTools.GetInstance().CompareTwoDoubleValue(maxXmaxY.Y, maxXminY.Y) != 0)
                    {
                        mrPart.mRightTopPoint = maxXmaxY;
                        mrPart.mRightBottomPoint = maxXminY;
                    }
                    else
                    {
                        mrPart.mRightTopPoint = maxX;
                        mrPart.mRightBottomPoint = minY;
                    }
                }
            }
            //2.如果斜率大于零;
            else if (mrYSlopeType == MrSlopeType.MORETHAN_ZERO)
            {
                if (CDimTools.GetInstance().CompareTwoDoubleValue(minXmaxY.X, minXminY.X) == 0 
                    && CDimTools.GetInstance().CompareTwoDoubleValue(minXmaxY.Y,minXminY.Y) != 0)
                {
                    mrPart.mLeftBottomPoint = minXminY;
                    mrPart.mLeftTopPoint = minXmaxY;

                    if (CDimTools.GetInstance().CompareTwoDoubleValue(maxXmaxY.X, maxXminY.X) == 0
                        && CDimTools.GetInstance().CompareTwoDoubleValue(maxXmaxY.Y, maxXminY.Y) != 0)
                    {
                        mrPart.mRightTopPoint = maxXmaxY;
                        mrPart.mRightBottomPoint = maxXminY;
                    }
                    else
                    {
                        mrPart.mRightTopPoint = maxY;
                        mrPart.mRightBottomPoint = maxX;
                    }
                }
                else
                {
                    mrPart.mLeftBottomPoint = minY;
                    mrPart.mLeftTopPoint = minX;

                    if (CDimTools.GetInstance().CompareTwoDoubleValue(maxXmaxY.X, maxXminY.X) == 0
                         && CDimTools.GetInstance().CompareTwoDoubleValue(maxXmaxY.Y, maxXminY.Y) != 0)
                    {
                        mrPart.mRightTopPoint = maxXmaxY;
                        mrPart.mRightBottomPoint = maxXminY;
                    }
                    else
                    {
                        mrPart.mRightTopPoint = maxY;
                        mrPart.mRightBottomPoint = maxX;
                    }
                }
            }
            //3.如果斜率为无穷;
            else if(mrYSlopeType==MrSlopeType.INFINITY)
            {
                mrPart.mLeftBottomPoint = mrPart.GetMinXMinYPoint();
                mrPart.mLeftTopPoint = mrPart.GetMinXMaxYPoint();
                mrPart.mRightTopPoint = mrPart.GetMaxXMaxYPoint();
                mrPart.mRightBottomPoint = mrPart.GetMaxXMinYPoint();
            }
        }


        /// <summary>
        /// 构建与顶部板平行的零部件与其最近的零部件之间的关系;
        /// </summary>
        /// <param name="mrPartList"></param>
        public void BuildMostNearPartToPartList(List<CMrPart> mrPartList)
        {
            mDicPartToPartList.Clear();

            foreach (CMrPart mrPart in mrPartList)
            {
                Vector normal=mrPart.mNormal;

                if (!mrPart.IsHaveBolt())
                {
                    continue;
                }

                //如果零部件的法向与主梁顶板的法向平行;
                if (CDimTools.GetInstance().IsTwoVectorParallel(normal, mTopBeam.mNormal))
                {
                    CMrBoltArray mrBoltArray = mrPart.GetBoltArrayList()[0];
                    Point boltPoint = mrBoltArray.GetMaxXPoint();

                    //需要寻找与该零件最近的零部件;
                    CMrPart mostNearPart = CMrBeamDoorManager.GetInstance().GetMostNearPart(boltPoint);

                    if (mDicPartToPartList.ContainsKey(mostNearPart))
                    {
                        List<CMrPart> dimPartList = mDicPartToPartList[mostNearPart];
                        dimPartList.Add(mrPart);
                    }
                    else
                    {
                        List<CMrPart> dimPartList = new List<CMrPart>();
                        dimPartList.Add(mrPart);
                        mDicPartToPartList[mostNearPart] = dimPartList;
                    }
                }
            }
        }

        /// <summary>
        /// 计算上翼板的参数;
        /// </summary>
        public void ComputeTypeMiddleParas()
        {
            Vector topNormal = mTopBeam.mNormal;
            
            MrSlopeType mrSlopeType = CDimTools.GetInstance().JudgeLineSlope(new Point(0, 0, 0), topNormal);

            Point minXmaxYPt = mTopBeam.GetMinXMaxYPoint();
            Point maxXmaxYPt = mTopBeam.GetMaxXMaxYPoint();

            if (Math.Abs(minXmaxYPt.Y - maxXmaxYPt.Y) > 10)
            {
                mMidMaxPoint = new Point((minXmaxYPt.X + maxXmaxYPt.X) / 2.0, (minXmaxYPt.Y + maxXmaxYPt.Y) / 2.0, 0);

                mType = MrBeamDoorType.TypeMiddle3;
            }
            else if (mrSlopeType == MrSlopeType.LESSTHAN_ZERO)
            {
                //1.计算出中心Y值最大的点;
                mMidMaxPoint = mTopBeam.GetMaxYPoint();
                mType = MrBeamDoorType.TypeMiddle1;    
            }
            else if (mrSlopeType == MrSlopeType.MORETHAN_ZERO)
            {
                mMidMaxPoint = mTopBeam.GetMinYPoint();
                mType = MrBeamDoorType.TypeMiddle2;   
            }

            //2.计算出左上角和左下角的点;
            List<Point> pointList = mTopBeam.GetPointList();
            List<Point> newPointList = new List<Point>();

            foreach (Point point in pointList)
            {
                if (point.X < mMidMaxPoint.X)
                {
                    newPointList.Add(point);
                }
            }

            Point minY = CDimTools.GetInstance().GetMinYPoint(newPointList);
            Point maxY = CDimTools.GetInstance().GetMaxYPoint(newPointList);

            mTopBeam.mLeftBottomPoint = minY;
            mTopBeam.mLeftTopPoint = maxY;
           
            newPointList.Clear();

            //3.计算右上角和右下角的点;
            foreach (Point point in pointList)
            {
                if (point.X > mMidMaxPoint.X)
                {
                    newPointList.Add(point);
                }
            }

            maxY = CDimTools.GetInstance().GetMaxYPoint(newPointList);
            minY = CDimTools.GetInstance().GetMinYPoint(newPointList);

            mTopBeam.mRightTopPoint = maxY;
            mTopBeam.mRightBottomPoint = minY;

            //4.计算左右两边的法向;
            Vector normal = mTopBeam.mNormal;

            if (mType == MrBeamDoorType.TypeMiddle1)
            {
                mLeftTopVector = new Vector(-Math.Abs(normal.X), Math.Abs(normal.Y), 0);
                mRightTopVector = new Vector(Math.Abs(normal.X), Math.Abs(normal.Y), 0);
            }
            else if (mType == MrBeamDoorType.TypeMiddle2)
            {
                mLeftTopVector = new Vector(Math.Abs(normal.X), Math.Abs(normal.Y), 0);
                mRightTopVector = new Vector(-Math.Abs(normal.X), Math.Abs(normal.Y), 0);
            }
            else if (mType == MrBeamDoorType.TypeMiddle3)
            {
                mLeftTopVector = normal;
                mRightTopVector = normal;
            }
        }
    }
}
