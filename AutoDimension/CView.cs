using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using AutoDimension.Entity;

using Tekla.Structures.Drawing;
using Tekla.Structures.Model;
using Tekla.Structures.Geometry3d;
using Tekla.Structures;

using TSD = Tekla.Structures.Drawing;
using TSM = Tekla.Structures.Model;

namespace AutoDimension
{
    /// <summary>
    /// view类,TopView与FrontView派生于此;
    /// </summary>
    public  class CView
    {
        /// <summary>
        ///对应的TSD中的View;
        /// </summary>
        protected TSD.View mViewBase = null;

        /// <summary>
        /// 视图区内的主梁;
        /// </summary>
        protected CMrPart mMainBeam = null;

        /// <summary>
        /// Tekla的模型对象;
        /// </summary>
        protected Model mModel = null;

        /// <summary>
        /// 前视图中零部件的链表;
        /// </summary>
        protected List<CMrPart> mMrPartList = new List<CMrPart>();

        /// <summary>
        /// 前视图中螺钉组的链表;
        /// </summary>
        protected List<CMrBoltArray> mMrBoltArrayList = new List<CMrBoltArray>();

        /// <summary>
        /// 顶视图标注线程锁字符串对象;
        /// </summary>
        protected String mLockString = "AutoDimension";

        /// <summary>
        /// 构造函数;
        /// </summary>
        /// <param name="viewBase"></param>
        public CView(TSD.View viewBase)
        {
            mViewBase = viewBase;

            SetDimProperty(mViewBase);
        }

        /// <summary>
        /// 添加部件对象;
        /// </summary>
        protected void AppendMrPart(CMrPart mrPart)
        {
            mMrPartList.Add(mrPart);
        }

        /// <summary>
        /// 设置用户自定义属性到Tekla的视图对象中;
        /// </summary>
        /// <param name="vie"></param>
        protected void SetDimProperty(TSD.View view)
        {
            view.SetUserProperty(CCommonPara.mDimPropKey, CCommonPara.mUserPropValue);
        }

        /// <summary>
        /// 获取视图中所有的零部件对象;
        /// </summary>
        /// <returns></returns>
        public List<CMrPart> GetMrPartList()
        {
            return mMrPartList;
        }
    }
}
