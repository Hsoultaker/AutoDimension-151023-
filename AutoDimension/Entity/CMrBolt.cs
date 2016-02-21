using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tekla.Structures.Geometry3d;

using TSD = Tekla.Structures.Drawing;

namespace AutoDimension.Entity
{
    /// <summary>
    /// 螺钉对象;
    /// </summary>
    public class CMrBolt:CMrEntity
    {
        /// <summary>
        /// 螺钉的位置;
        /// </summary>
        public Point mPosition;

        public CMrBolt()
        {
            mName = "Bolt";
        }
    }
}
