using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Tekla.Structures.Geometry3d;
using Tekla.Structures.Drawing;

namespace AutoDimension.Entity
{
    /// <summary>
    /// 标注的集合;
    /// </summary>
    public class CMrDimSet
    {
        /// <summary>
        /// 标注点的集合;
        /// </summary>
        private List<Point> mPointList=new List<Point>();

        /// <summary>
        /// 标注的向量;
        /// </summary>
        public Vector mDimVector = null;

        /// <summary>
        /// 标注的在向量方向上的距离;
        /// </summary>
        public double mDimDistance = 0.0;

        /// <summary>
        /// 获得标注集合中点的数量;
        /// </summary>
        public int Count
        {
            get
            {
                return mPointList.Count;
            }
        }

        /// <summary>
        /// 获得待标注的点的链表;
        /// </summary>
        /// <returns></returns>
        public List<Point> GetDimPointList()
        {
            return mPointList;
        }

        /// <summary>
        /// 添加点到标注集合的链表中;
        /// </summary>
        /// <param name="point"></param>
        public void AddPoint(Point point)
        {
            point.Z = 0;

            if (mPointList.Contains(point))
            {
                return;
            }

            mPointList.Add(point);
        }
        
        /// <summary>
        /// 添加点的链表集合;
        /// </summary>
        /// <param name="pointList"></param>
        public void AddRange(List<Point> pointList)
        {
            foreach(Point point in pointList)
            {
                point.Z = 0;

                if(mPointList.Contains(point))
                {
                    continue;
                }
                
                mPointList.Add(point);
            }
        }

        /// <summary>
        /// 根据索引获取点;
        /// </summary>
        /// <param name="nIndex"></param>
        /// <returns></returns>
        public Point GetPoint(int nIndex)
        {
            if(nIndex >= Count)
            {
                return null;
            }

            return mPointList[nIndex];
        }

        /// <summary>
        /// 清空所有的点;
        /// </summary>
        public void Clear()
        {
            mPointList.Clear();
        }
    }
}
