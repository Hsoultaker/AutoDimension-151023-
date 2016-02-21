using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Tekla.Structures.Geometry3d;

namespace AutoDimension.Entity
{
    /// <summary>
    /// 剪切板管理器,剪切板存在于柱标注的顶视图与前视图中;
    /// </summary>
    public class CMrClipPlateManager
    {
        /// <summary>
        /// 剪切板的链表;
        /// </summary>
        private List<CMrClipPlate> mMrClipPlateList = new List<CMrClipPlate>();

        /// <summary>
        /// 单一实例;
        /// </summary>
        private static CMrClipPlateManager mInstance = null;

        /// <summary>
        /// 当前对应的视图类型;
        /// </summary>
        public MrViewType mViewType;

        /// <summary>
        /// 获取单例;
        /// </summary>
        /// <returns></returns>
        public static CMrClipPlateManager GetInstance()
        {
            if(null==mInstance)
            {
                mInstance = new CMrClipPlateManager();
            }
            return mInstance;
        }

        /// <summary>
        /// 获取剪切板的链表;
        /// </summary>
        /// <returns></returns>
        public List<CMrClipPlate> GetMrClipPlateList()
        {
            return mMrClipPlateList;
        }

        /// <summary>
        /// 构建剪切板,剪切板的构建主要是针对柱的顶视图中;
        /// </summary>
        public void BuildMrClipPlate(List<CMrPart> mrPartList,MrViewType mrViewType)
        {
            this.mViewType = mrViewType;

            Vector zVector = new Vector(0, 0, 1);

            foreach(CMrPart mrPart in mrPartList)
            {
                if (IsOutsidePlate(mrPart))
                {
                    continue;
                }

                Vector normal=mrPart.mNormal;

                if(CDimTools.GetInstance().IsTwoVectorParallel(normal,zVector))
                {
                    CMrPart topPart = FindTopPart(mrPart,mrPartList);

                    CMrPart bottomPart = FindBottomPart(mrPart,mrPartList);

                    if(topPart != null && bottomPart != null)
                    {
                        CMrClipPlate mrClipPlate = new CMrClipPlate(topPart,mrPart,bottomPart);

                        mMrClipPlateList.Add(mrClipPlate);
                    }
                }
            }
        }

        /// <summary>
        /// 判断零件是否是外围板;
        /// </summary>
        /// <param name="mrPart"></param>
        /// <returns></returns>
        private bool IsOutsidePlate(CMrPart mrPart)
        {
            Vector normal = mrPart.mNormal;
            double minZ = mrPart.GetMinZPoint().Z;
            double maxZ = mrPart.GetMaxZPoint().Z;

            double mainBeamMinZ = CMrMainBeam.GetInstance().GetMinZPoint().Z;
            double mainBeamMaxZ = CMrMainBeam.GetInstance().GetMaxZPoint().Z;

            if (CDimTools.GetInstance().CompareTwoDoubleValue(minZ, mainBeamMaxZ) >= 0)
            {
                return true;
            }
            if (CDimTools.GetInstance().CompareTwoDoubleValue(maxZ, mainBeamMinZ) <= 0)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 寻找剪切板上面的一块板;
        /// </summary>
        /// <param name="mMidPart"></param>
        /// <param name="mrPartList"></param>
        /// <returns></returns>
        private CMrPart FindTopPart(CMrPart mMidPart, List<CMrPart> mrPartList)
        {
            Vector midVector = mMidPart.mNormal;

            Point midPartMaxYPoint = mMidPart.GetMaxYPoint();

            Vector yVector = new Vector(0, 1, 0);

            foreach (CMrPart mrPart in mrPartList)
            {
                if (!CDimTools.GetInstance().IsTwoVectorParallel(mrPart.mNormal, yVector))
                {
                    continue;
                }

                if (mrPart == mMidPart)
                {
                    continue;
                }

                if (mViewType == MrViewType.CylinderTopView)
                {
                    if (mrPart.GetCylinderTopViewInfo().mPostionType != mMidPart.GetCylinderTopViewInfo().mPostionType)
                    {
                        continue;
                    }
                }
                else if (mViewType == MrViewType.CylinderFrontView)
                {
                    if (mrPart.GetCylinderFrontViewInfo().mPostionType != mMidPart.GetCylinderFrontViewInfo().mPostionType)
                    {
                        continue;
                    }
                }

                Point topPartMinYPoint = mrPart.GetMinYPoint();

                if (CDimTools.GetInstance().CompareTwoDoubleValue(topPartMinYPoint.Y, midPartMaxYPoint.Y) == 0)
                {
                    return mrPart;
                }
            }
            return null;
        }

        /// <summary>
        /// 寻找剪切板下面的一块板;
        /// </summary>
        /// <param name="mMidPart"></param>
        /// <param name="mrPartList"></param>
        /// <returns></returns>
        private CMrPart FindBottomPart(CMrPart mMidPart, List<CMrPart> mrPartList)
        {
            Vector midVector = mMidPart.mNormal;

            Point midPartMinYPoint = mMidPart.GetMinYPoint();

            Vector yVector = new Vector(0, 1, 0);

            foreach (CMrPart mrPart in mrPartList)
            {
                if (!CDimTools.GetInstance().IsTwoVectorParallel(mrPart.mNormal, yVector))
                {
                    continue;
                }
                
                if (mrPart == mMidPart)
                {
                    continue;
                }

                if (mViewType == MrViewType.CylinderTopView)
                {
                    if (mrPart.GetCylinderTopViewInfo().mPostionType != mMidPart.GetCylinderTopViewInfo().mPostionType)
                    {
                        continue;
                    }
                }
                else if (mViewType == MrViewType.CylinderFrontView)
                {
                    if (mrPart.GetCylinderFrontViewInfo().mPostionType != mMidPart.GetCylinderFrontViewInfo().mPostionType)
                    {
                        continue;
                    }
                }

                Point bottomPartMaxYPoint = mrPart.GetMaxYPoint();

                if (CDimTools.GetInstance().CompareTwoDoubleValue(bottomPartMaxYPoint.Y, midPartMinYPoint.Y) == 0)
                {
                    return mrPart;
                }
            }

            return null;
        }

        /// <summary>
        /// 判断该板是否属于剪切板;
        /// </summary>
        /// <param name="mrPart"></param>
        /// <param name="nIndex">在剪切板中的位置</param>
        /// <returns></returns>
        public bool IsInClipPlate(CMrPart mrPart, out MrClipPlatePosType mrClipTypePosType)
        {
            mrClipTypePosType = MrClipPlatePosType.NONE;

            foreach (CMrClipPlate mrClipPlate in mMrClipPlateList)
            {
                mrClipTypePosType = mrClipPlate.IsHaveThePart(mrPart);

                if (mrClipTypePosType != MrClipPlatePosType.NONE)
                {
                    return true;   
                }
            }
            return false;
        }

        /// <summary>
        /// 判断零件是否是剪切板中的一块板;
        /// </summary>
        /// <param name="mrPart"></param>
        /// <returns></returns>
        public bool IsInClipPlate(CMrPart mrPart)
        {
            MrClipPlatePosType mrClipTypePosType = MrClipPlatePosType.NONE;

            foreach (CMrClipPlate mrClipPlate in mMrClipPlateList)
            {
                mrClipTypePosType = mrClipPlate.IsHaveThePart(mrPart);

                if (mrClipTypePosType != MrClipPlatePosType.NONE)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 根据零件找到指定的剪切板;
        /// </summary>
        /// <param name="mrPart"></param>
        /// <returns></returns>
        public CMrClipPlate GetMrClipPlate(CMrPart mrPart)
        {
            foreach (CMrClipPlate mrClipPlate in mMrClipPlateList)
            {
                CMrPart mrTopPart = mrClipPlate.mMrTopPart;
                CMrPart mrMidPart = mrClipPlate.mMrMidPart;
                CMrPart mrBottomPart = mrClipPlate.mMrBottomPart;

                if(mrPart==mrTopPart)
                {
                    return mrClipPlate;
                }
                else if(mrPart==mrMidPart)
                {
                    return mrClipPlate;
                }
                else if (mrPart == mrBottomPart)
                {
                    return mrClipPlate;
                }
            }
            return null;
        }
    }
}
