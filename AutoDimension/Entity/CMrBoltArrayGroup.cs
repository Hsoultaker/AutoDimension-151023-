using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tekla.Structures.Geometry3d;

namespace AutoDimension.Entity
{
    /// <summary>
    /// 具有相同属性的螺钉组的集合，主要指螺钉组的方向向量相同;
    /// </summary>
    public class CMrBoltArrayGroup
    {
        /// <summary>
        /// 所有相同螺钉组的法向;
        /// </summary>
        public Vector normal = null;

        /// <summary>
        /// 螺钉组合中的螺钉组链表;
        /// </summary>
        public List<CMrBoltArray> mrBoltArrayList = new List<CMrBoltArray>();

        /// <summary>
        /// 添加到螺钉组合的链表中;
        /// </summary>
        /// <param name="mrBoltArray"></param>
        public void AppendMrBoltArray(CMrBoltArray mrBoltArray)
        {
            if (mrBoltArrayList.Contains(mrBoltArray))
            {
                return;
            }
            mrBoltArrayList.Add(mrBoltArray);
        }

        /// <summary>
        /// 获得最小Y值的点;
        /// </summary>
        /// <returns></returns>
        public Point GetMinYPoint()
        {
            Point minYPt = new Point();
            double minY = int.MaxValue;

            foreach (CMrBoltArray mrBoltArray in mrBoltArrayList)
            {
                Point boltminYPt = mrBoltArray.GetMinYPoint();
                
                if ( boltminYPt.Y < minY)
                {
                    minY = boltminYPt.Y;
                    minYPt = boltminYPt;
                }
            }
            return minYPt;
        }

        /// <summary>
        /// 获得最大Y值的点;
        /// </summary>
        /// <returns></returns>
        public Point GetMaxYPoint()
        {
            Point maxYPt = new Point();
            double maxY = int.MinValue;

            foreach (CMrBoltArray mrBoltArray in mrBoltArrayList)
            {
                Point boltMaxYPt = mrBoltArray.GetMaxYPoint();

                if (boltMaxYPt.Y > maxY)
                {
                    maxY = boltMaxYPt.Y;
                    maxYPt = boltMaxYPt;
                }
            }
            return maxYPt;
        }
    }
}
