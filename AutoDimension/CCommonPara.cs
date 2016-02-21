using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Tekla.Structures.Model;

using TSM = Tekla.Structures.Model;
using AutoDimension.Entity;

namespace AutoDimension
{
    /// <summary>
    /// 共同的参数类;
    /// </summary>
    public class CCommonPara
    {
        /// <summary>
        /// 是否启用加密;
        /// </summary>
        public static bool mBEnableLock = true;

        /// <summary>
        /// double数值的误差;
        /// </summary>
        public static double mDblError = 1e-2;

        /// <summary>
        /// double数值的负数误差精度;
        /// </summary>
        public static double mNegativeDblError = -1e-2;

        /// <summary>
        /// 当前视图的缩放比;
        /// </summary>
        public static double mViewScale = 0.0;

        /// <summary>
        /// 零件标记的倾斜角度;已经无效了;
        /// </summary>
        public static double mPartMarkAngle 
        {
            get
            {
                if(mViewScale==10)
                {
                     return 45 * Math.PI / 180;
                }
                else if(mViewScale==15)
                {
                    return 40 * Math.PI / 180;
                }
                else if (mViewScale == 20)
                {
                    return 35 * Math.PI / 180;
                }
                else if (mViewScale == 25)
                {
                    return 30 * Math.PI / 180;
                }
                else
                {
                    return (450 / mViewScale) * (Math.PI / 180);
                }
            }
        }

        /// <summary>
        /// 零件标记的竖直高度与直线长度;已经无效了;
        /// </summary>
        public static double mPartMarkLength
        {
            get
            {
                if (mViewScale == 10)
                {
                    return 150;
                }
                else if (mViewScale == 15)
                {
                    return 140;
                }
                else if (mViewScale == 20)
                {
                    return 130;
                }
                else if (mViewScale == 25)
                {
                    return 120;
                }
                else if(mViewScale==30)
                {
                    return 110;
                }
                else
                {
                    return 1500 / mViewScale;
                }
            }
        }

        /// <summary>
        /// 螺钉标记的倾斜角度;已经无效了;
        /// </summary>
        public static double mBoltMarkAngle
        {
            get
            {
                if (mViewScale == 10)
                {
                    return 45 * Math.PI / 180;
                }
                else if (mViewScale == 15)
                {
                    return 40 * Math.PI / 180;
                }
                else if (mViewScale == 20)
                {
                    return 35 * Math.PI / 180;
                }
                else if (mViewScale == 25)
                {
                    return 30 * Math.PI / 180;
                }
                else if(mViewScale==30)
                {
                    return 30 * Math.PI / 180;
                }
                else
                {
                    return (450 / mViewScale) * (Math.PI / 180);
                }
            }
        }

        /// <summary>
        /// 主梁上螺栓的标记长度;已经无效了;
        /// </summary>
        public static double mBoltMarkLength  
        {
            get
            {
                if(mViewScale==10)
                {
                    return 180;
                }
                else if(mViewScale==15)
                {
                    return 170;
                }
                else if(mViewScale==20)
                {
                    return 160;
                }
                else if(mViewScale==25)
                {
                    return 160;
                }
                else if(mViewScale==30)
                {
                    return 160;
                }
                else
                {
                    return 1800 / mViewScale;
                }
            }
        }

        /// <summary>
        /// 自身标注时默认的标注距离;
        /// </summary>
        public static double mDefaultDimDistance
        {
            get
            {
                if (mViewScale > 0)
                {
                    return mDefaultDimDistanceWithScale * mViewScale;
                }
                else
                {
                    return 150;
                }
            }
        }

        /// <summary>
        /// 和比例相关的默认标注距离，尺寸线与轮廓边的距离;
        /// </summary>
        public static double mDefaultDimDistanceWithScale = 15;

        /// <summary>
        /// 默认的两条尺寸线之间的距离;
        /// </summary>
        public static double mDefaultTwoDimLineGap
        {
            get
            {
                if (mViewScale > 0)
                {
                    return mDefaultTwoDimLineGapWithScale * mViewScale;
                }
                else
                {
                    return 200;
                }
            }
        }

        /// <summary>
        /// 和比例相关的默认标注距离，尺寸线与尺寸线之间的距离;
        /// </summary>
        public static double mDefaultTwoDimLineGapWithScale = 15;

        /// <summary>
        /// 当螺钉距离主梁边缘很近时,如果小于该阈值则需要把螺钉尺寸标注到视图包围盒外部;
        /// </summary>
        public static double mDefaultDimDistanceThreshold = 200;

        /// <summary>
        /// 针对主梁上的螺钉组如果方向向量相同并且两者之间的距离小于等于该间隙，则组合起来进行标注;
        /// </summary>
        public static double mDefaultTwoBoltArrayGap = 150;

        /// <summary>
        /// 针对主梁上的零件如果法向向量相同并且两者之间的距离小于等于该间隙，则组合起来进行标注;
        /// </summary>
        public static double mDefalutTwoPartGap = 100;

        /// <summary>
        /// 当前视图的最小的X值;
        /// </summary>
        public static double mViewMinX = double.MaxValue;

        /// <summary>
        /// 当前视图的最小的Y值;
        /// </summary>
        public static double mViewMinY = double.MaxValue;

        /// <summary>
        /// 当前视图的最大X值;
        /// </summary>
        public static double mViewMaxX = double.MinValue;

        /// <summary>
        /// 当前视图的最大Y值;
        /// </summary>
        public static double mViewMaxY = double.MinValue;

        /// <summary>
        /// 总尺寸标注属性文件路径;
        /// </summary>
        public static string mMainSizeDimPath = null;

        /// <summary>
        /// 分尺寸标注属性文件路径;
        /// </summary>
        public static string mSizeDimPath = null;

        /// <summary>
        /// 角度尺寸标注属性文件路径;
        /// </summary>
        public static string mAngleDimPath = null;

        /// <summary>
        /// 主零件标记属性文件路径;
        /// </summary>
        public static string mMainPartNotePath = null;

        /// <summary>
        /// 次部件标记属性文件路径;
        /// </summary>
        public static string mPartNotePath = null;

        /// <summary>
        /// 螺栓标记属性文件路径;
        /// </summary>
        public static string mBoltNotePath = null;

        /// <summary>
        /// 螺栓孔标记属性文件路径;
        /// </summary>
        public static string mBoltHoleNotePath = null;

        /// <summary>
        /// 剖视图的属性文件路径;
        /// </summary>
        public static string mSectionAttPath = null;

        /// <summary>
        /// 剖视图的标记属性文件路径;
        /// </summary>
        public static string mSectionMarkNotePath = null;

        /// <summary>
        /// 自定义标注属性Key;
        /// </summary>
        public static string mDimPropKey = "Dim";

        /// <summary>
        /// 自定义剖面属性Key;
        /// </summary>
        public static string mAutoSectionPropKey = "AutoSection";

        /// <summary>
        /// 自定义属性值设置到Tekla的视图对象中;
        /// </summary>
        public static int mUserPropValue = 500;

        /// <summary>
        /// 是否强制删除所有的剖面，即使已经标注过的;
        /// </summary>
        public static bool mbForceDeleteSection = false;

        /// <summary>
        /// 水平方向的剖面默认方向;
        /// </summary>
        public static MrSectionOrientation mHorizontalSection = MrSectionOrientation.MrSectionRight;

        /// <summary>
        /// 竖直方向的剖面默认方向;
        /// </summary>
        public static MrSectionOrientation mVerticalSection = MrSectionOrientation.MrSectionDown;

        /// <summary>
        /// 向下打剖面的深度;
        /// </summary>
        public static double mDblSectionUpDepth = 100;

        /// <summary>
        /// 向上打剖面的深度;
        /// </summary>
        public static double mDblSectionDownDepth = 100;

        /// <summary>
        /// 是否相同的剖面不显示剖面标记;
        /// </summary>
        public static bool mbShowSameSectionMark = true;

        /// <summary>
        /// 在什么情况下进行自动剖面;
        /// </summary>
        public static MrAutoSectionType mAutoSectionType = MrAutoSectionType.MrNone;

    }
}
