using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tekla.Structures.Geometry3d;

namespace AutoDimension.Entity
{
    /// <summary>
    /// 具有相同属性的零件组对象;
    /// </summary>
    public class CMrPartGroup
    {
        /// <summary>
        /// 所有相同零件的法向;
        /// </summary>
        public Vector normal = null;

        /// <summary>
        /// 零件组合中的零件链表;
        /// </summary>
        public List<CMrPart> mrPartList = new List<CMrPart>();

        /// <summary>
        /// 添加到螺钉组合的链表中;
        /// </summary>
        /// <param name="CMrPart"></param>
        public void AppendMrPart(CMrPart mrPart)
        {
            if (mrPartList.Contains(mrPart))
            {
                return;
            }
            mrPartList.Add(mrPart);
        }
    }
}
