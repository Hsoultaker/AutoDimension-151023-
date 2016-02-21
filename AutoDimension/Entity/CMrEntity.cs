using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tekla.Structures.Drawing;
using Tekla.Structures.Model;

using TSD = Tekla.Structures.Drawing;
using TSM = Tekla.Structures.Model;

namespace AutoDimension
{
    public class CMrEntity
    {
        /// <summary>
        /// 实体名字;
        /// </summary>
        protected String mName;

        /// <summary>
        /// 设置部件的名字;
        /// </summary>
        /// <param name="strName"></param>
        public void SetName(String strName)
        {
            mName = strName;
        }

        /// <summary>
        /// 获取零件的名字;
        /// </summary>
        public String GetName()
        {
            return mName;
        }
    }
}
