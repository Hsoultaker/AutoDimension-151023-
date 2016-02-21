using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tekla.Structures.Drawing;

namespace AutoDimension.Entity
{
    public class CMrAssemblyDrawing
    {
        /// <summary>
        /// tekla中的图纸对象;
        /// </summary>
        public AssemblyDrawing mAssemblyDring=null;

        /// <summary>
        /// 标注类型;
        /// </summary>
        public string mDimType = "柱标注";

        /// <summary>
        /// 图纸名称;
        /// </summary>
        public string mName = "";

        /// <summary>
        /// 图纸标记;
        /// </summary>
        public string mMark = "";

        /// <summary>
        /// 标题1;
        /// </summary>
        public string mTitle1 = "";

        /// <summary>
        /// 标题2;
        /// </summary>
        public string mTitle2 = "";

        /// <summary>
        /// 标题3；
        /// </summary>
        public string mTitle3 = "";

        /// <summary>
        /// 构造函数;
        /// </summary>
        /// <param name="assemblyDrawing"></param>
        public CMrAssemblyDrawing(AssemblyDrawing assemblyDrawing)
        {
            this.mAssemblyDring = assemblyDrawing;
        }

        /// <summary>
        /// 获取图纸标记号,标记号是图纸标记点前面的字符并且去掉前后的数字;
        /// </summary>
        /// <returns></returns>
        public string GetMarkNumber()
        {
            if (mMark == null || mMark == "")
            {
                return "";
            }
            string strMarkNumber = mMark;

            strMarkNumber = strMarkNumber.TrimEnd(']');
            strMarkNumber = strMarkNumber.TrimStart('[');

            int nIndex = strMarkNumber.IndexOf('.');

            strMarkNumber = strMarkNumber.Substring(0, nIndex );

            strMarkNumber = strMarkNumber.TrimStart(new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' });
            strMarkNumber = strMarkNumber.TrimEnd(new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' });

            return strMarkNumber;
        }
    }
}
