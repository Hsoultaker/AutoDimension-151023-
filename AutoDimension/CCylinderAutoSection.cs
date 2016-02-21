using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Tekla.Structures.Model;
using Tekla.Structures.Drawing;

using TSM = Tekla.Structures.Model;
using TSD = Tekla.Structures.Drawing;
using AutoDimension.Entity;
using Tekla.Structures.Geometry3d;

namespace AutoDimension
{
    /// <summary>
    /// 框架结构柱自动剖面类;
    /// </summary>
    public class CCylinderAutoSection
    {
        /// <summary>
        /// 剖面标记的索引数组;
        /// </summary>
        public string[] mSectionMarkArray = { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N","O","P","Q","R"
                                            ,"S","T","U","V","W","X","Y","Z","AA","AB","AC","AD","AE","AF","AG"};

        /// <summary>
        /// 主梁对象;
        /// </summary>
        public TSM.Part mMainPart = null;

        /// <summary>
        /// 当前图纸;
        /// </summary>
        public AssemblyDrawing mAssemblyDrawing = null;

        /// <summary>
        /// 当前剖面索引;
        /// </summary>
        private int mSectionMarkIndex = -1;

        /// <summary>
        /// Tekla定义的前视图View对象;
        /// </summary>
        private TSD.View mFrontView = null;

        /// <summary>
        /// MrRender定义的梁的前视图对象;
        /// </summary>
        private CCylinderFrontView mCylinderFrontView = null;

        /// <summary>
        /// 前视图中所有零件的链表;
        /// </summary>
        private List<CMrPart> mrPartList = new List<CMrPart>();

        /// <summary>
        /// 所有的剖面视图;
        /// </summary>
        private List<TSD.View> mAllSectionViewList = new List<TSD.View>();

        /// <summary>
        /// 所有需要进行剖视的零件链表;
        /// </summary>
        private List<CMrPart> mAllSectionPartList = new List<CMrPart>();

        /// <summary>
        /// 主梁的剖面链表;
        /// </summary>
        private List<CMrSection> mSectionList = new List<CMrSection>();

        /// <summary>
        /// 构造函数;
        /// </summary>
        /// <param name="mainPart"></param>
        /// <param name="assemblyDrawing"></param>
        public CCylinderAutoSection(TSM.Part mainPart, AssemblyDrawing assemblyDrawing)
        {
            this.mMainPart = mainPart;
            this.mAssemblyDrawing = assemblyDrawing;
        }

        /// <summary>
        /// 创建自动剖面;
        /// </summary>
        public void CreateSection()
        {
            GetDrawingViewInfo();

            if (mFrontView == null)
            {
                return;
            }

            DeleteAllSection();
            DeleteAllSectionMark();

            InitFrontView();
            BuildNeedSectionPartList();
            
            CreateAllPartSection();

            mAssemblyDrawing.PlaceViews();
            mAssemblyDrawing.Modify();
        }

        /// <summary>
        /// 获取图纸中的视图信息;
        /// </summary>
        private void GetDrawingViewInfo()
        {
            DrawingObjectEnumerator allViews = mAssemblyDrawing.GetSheet().GetAllViews();

            while (allViews.MoveNext())
            {
                if (allViews.Current == null)
                {
                    continue;
                }

                TSD.View view = allViews.Current as TSD.View;

                if (view == null)
                {
                    continue;
                }

                view.Select();

                //创建自定义的主梁对象;
                CMrMainBeam mainBeam = new CMrMainBeam(mMainPart, null);
                CMrMainBeam.SetInstance(mainBeam);

                double dblFlangeThickness = 0.0;
                double dblWebThickness = 0.0;

                mMainPart.GetReportProperty("PROFILE.FLANGE_THICKNESS", ref dblFlangeThickness);
                mMainPart.GetReportProperty("PROFILE.WEB_THICKNESS", ref dblWebThickness);

                mainBeam.mFlangeThickness = dblFlangeThickness;
                mainBeam.mWebThickness = dblWebThickness;

                int size = view.GetObjects(new Type[] { typeof(TSD.Part) }).GetSize();

                if (size == 0 || size == 1)
                {
                    continue;
                }

                Vector xVector = new Vector(1, 0, 0);
                Vector yVector = new Vector(0, 1, 0);
                Vector zVector = new Vector(0, 0, 1);

                CDimTools.GetInstance().InitMrPart(mMainPart, view, mainBeam);

                Vector vector = mainBeam.mNormal;
                double minY = mainBeam.GetMinYPoint().Y;
                double maxY = mainBeam.GetMaxYPoint().Y;
                double mainBeamHeight = Math.Abs(maxY - minY);

                //顶视图标注;
                if (CDimTools.GetInstance().IsTwoVectorParallel(vector, xVector))
                {
                    Point viewMinPoint = view.RestrictionBox.MinPoint;
                    Point viewMaxPoint = view.RestrictionBox.MaxPoint;

                    double viewHeight = Math.Abs(viewMaxPoint.Y - viewMinPoint.Y);

                    if (CDimTools.GetInstance().CompareTwoDoubleValue(viewHeight, mainBeamHeight) >= 0)
                    {
                        continue;
                    }
                    else
                    {
                        mAllSectionViewList.Add(view);
                    }
                }
                //前视图标注;
                else if (CDimTools.GetInstance().IsTwoVectorParallel(vector, zVector))
                {
                    Point viewMinPoint = view.RestrictionBox.MinPoint;
                    Point viewMaxPoint = view.RestrictionBox.MaxPoint;

                    double viewHeight = Math.Abs(viewMaxPoint.Y - viewMinPoint.Y);

                    if (CDimTools.GetInstance().CompareTwoDoubleValue(viewHeight, mainBeamHeight) >= 0)
                    {
                        mFrontView = view;
                    }
                    else
                    {
                        mAllSectionViewList.Add(view);
                    }
                }
                //剖视图标注;
                else
                {
                    mAllSectionViewList.Add(view);
                }
            }
        }

        /// <summary>
        /// 首先删除所有的剖视图,如果该剖视图被自动标注过则不再删除;
        /// </summary>
        private void DeleteAllSection()
        {
            foreach (View view in mAllSectionViewList)
            {
                if (view == null)
                {
                    continue;
                }
                view.Delete();
            }

            mAssemblyDrawing.Modify();
        }

        /// <summary>
        /// 删除所有的剖面标记;
        /// </summary>
        private void DeleteAllSectionMark()
        {
            DrawingObjectEnumerator allMarks = mFrontView.GetAllObjects(typeof(TSD.SectionMark));

            while (allMarks.MoveNext())
            {
                SectionMark markInDrawing = allMarks.Current as SectionMark;

                markInDrawing.Delete();
            }
        }

        /// <summary>
        /// 初始化前视图;
        /// </summary>
        private void InitFrontView()
        {
            if (mFrontView == null)
            {
                return;
            }
            mCylinderFrontView = new CCylinderFrontView(mFrontView, CDimManager.GetInstance().GetModel());
            mCylinderFrontView.InitFrontView();
            mrPartList = mCylinderFrontView.GetMrPartList();
        }

        /// <summary>
        /// 构建所有需要进行打剖面的零部件;
        /// </summary>
        private void BuildNeedSectionPartList()
        {
            CMrPart mainBeam = CMrMainBeam.GetInstance();

            double mainBeamMinY = mainBeam.GetMinYPoint().Y;
            double mainBeamMaxY = mainBeam.GetMaxYPoint().Y;
            double mainBeamMinX = mainBeam.GetMinXPoint().X;
            double mainBeamMaxX = mainBeam.GetMaxXPoint().X;

            foreach (CMrPart mrPart in mrPartList)
            {
                Vector normal = mrPart.mNormal;

                if (CDimTools.GetInstance().IsTwoVectorParallel(normal, new Vector(0, 1, 0)))
                {
                    if (!mrPart.IsHaveBolt())
                    {
                        continue;
                    }

                    double partMinX = mrPart.GetMinXPoint().X;
                    double partMaxX = mrPart.GetMaxXPoint().X;

                    if (CDimTools.GetInstance().CompareTwoDoubleValue(partMaxX, mainBeamMinX) <= 0 || 
                        CDimTools.GetInstance().CompareTwoDoubleValue(partMinX, mainBeamMaxX) >= 0)
                    {
                        continue;
                    }
                    mAllSectionPartList.Add(mrPart);
                }
                else if (CDimTools.GetInstance().IsTwoVectorParallel(normal, new Vector(1, 0, 0)))
                {
                    if (!mrPart.IsHaveBolt())
                    {
                        continue;
                    }
                    double partMinX = mrPart.GetMinXPoint().X;
                    double partMaxX = mrPart.GetMaxXPoint().X;
                    double partMinY = mrPart.GetMinYPoint().Y;
                    double partMaxY = mrPart.GetMaxYPoint().Y;

                    //过滤掉双夹板;
                    if(CDimTools.GetInstance().CompareTwoDoubleValue(partMinY,mainBeamMinY) < 0 &&
                       CDimTools.GetInstance().CompareTwoDoubleValue(partMaxY, mainBeamMinY) > 0)
                    {
                        continue;
                    }
                    if (CDimTools.GetInstance().CompareTwoDoubleValue(partMinY, mainBeamMaxY) < 0 &&
                        CDimTools.GetInstance().CompareTwoDoubleValue(partMaxY, mainBeamMaxY) > 0)
                    {
                        continue;
                    }

                    if (CDimTools.GetInstance().CompareTwoDoubleValue(partMaxX, mainBeamMinX) <= 0 ||
                        CDimTools.GetInstance().CompareTwoDoubleValue(partMinX, mainBeamMaxX) >= 0)
                    {
                        mAllSectionPartList.Add(mrPart);
                    }
                }
                else if (CDimTools.GetInstance().IsTwoVectorParallel(normal, new Vector(0, 0, 1)))
                {
                    if (mrPart.IsHaveBolt())
                    {
                        double partMinX = mrPart.GetMinXPoint().X;
                        double partMaxX = mrPart.GetMaxXPoint().X;
                        double partMinY = mrPart.GetMinYPoint().Y;
                        double partMaxY = mrPart.GetMaxYPoint().Y;

                        if (CDimTools.GetInstance().CompareTwoDoubleValue(partMinX, mainBeamMaxX) >= 0 ||
                            CDimTools.GetInstance().CompareTwoDoubleValue(partMaxX, mainBeamMinX) <= 0)
                        {
                            mAllSectionPartList.Add(mrPart);
                        }
                    }
                    else //判断是否是底板零部件,底板零部件默认加入到链表中;
                    {
                        double partMinX = mrPart.GetMinXPoint().X;
                        double partMaxX = mrPart.GetMaxXPoint().X;
                        double partMaxY = mrPart.GetMaxYPoint().Y;

                        if (CDimTools.GetInstance().CompareTwoDoubleValue(partMaxY, mainBeamMinY) == 0 &&
                           partMinX < mainBeamMinX && partMaxX > mainBeamMaxX)
                        {
                            mAllSectionPartList.Add(mrPart);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 创建所有零件的剖面;
        /// </summary>
        private void CreateAllPartSection()
        {
            CDimTools.GetInstance().SortMrPartByMaxY(mAllSectionPartList);

            foreach (CMrPart mrPart in mAllSectionPartList)
            {
                Vector normal = mrPart.mNormal;

                CMrSection mrSection = new CMrSection(MrSectionType.MrSectionCylinder);
                mrSection.AppendSectionPart(mrPart);

                if (CDimTools.GetInstance().IsTwoVectorParallel(normal, new Vector(1, 0, 0)))
                {
                    Point minXPt = mrPart.GetMinXPoint();
                    Point maxXpt = mrPart.GetMaxXPoint();
                    mrSection.mSectionMidX = (minXPt.X + maxXpt.X) / 2.0;
                    mrSection.mSectionMaxY = mrPart.GetMaxYPoint().Y;
                    mrSection.mSectionMinY = mrPart.GetMinYPoint().Y;

                    mrSection.mSectionMode = MrSectionMode.MrHorizontal;
                }
                else if (CDimTools.GetInstance().IsTwoVectorParallel(normal, new Vector(0, 1, 0)))
                {
                    Point minYPt = mrPart.GetMinYPoint();
                    Point maxYpt = mrPart.GetMaxYPoint();
                    mrSection.mSectionMidY = (minYPt.Y + maxYpt.Y) / 2.0;
                    mrSection.mSectionMode = MrSectionMode.MrVertical;
                }
                else if(CDimTools.GetInstance().IsTwoVectorParallel(normal, new Vector(0, 0, 1)))
                {
                    Point minYPt = mrPart.GetMinYPoint();
                    Point maxYpt = mrPart.GetMaxYPoint();
                    mrSection.mSectionMidY = maxYpt.Y;
                    mrSection.mSectionMode = MrSectionMode.MrVertical;
                }

                //1.如果零部件已经存在于剖面中则直接返回;
                if (IsPartInSection(mrPart,mrSection.mSectionMode))
                {
                    continue;
                }

                //2.把所有属于该剖面的零部件加入;
                AddPartInSection(mrSection);

                mSectionList.Add(mrSection);

                if (mrSection.mSectionMode == MrSectionMode.MrHorizontal)
                {
                    CreateHorizontalSection(mrSection);
                }
                else if (mrSection.mSectionMode == MrSectionMode.MrVertical)
                {
                    CreateVerticalSection(mrSection);
                }
            }
        }

        /// <summary>
        /// 创建水平方向的剖面;
        /// </summary>
        private void CreateHorizontalSection(CMrSection mrSection)
        {
            //判断是否需要创建该剖面或者只是添加零件标记;
            CMrSection mrSameSection = null;

            View.ViewAttributes viewAttributes = new View.ViewAttributes();
            viewAttributes.LoadAttributes(CCommonPara.mSectionAttPath);

            SectionMarkBase.SectionMarkAttributes sectionMarkAttributes = new SectionMarkBase.SectionMarkAttributes();
            sectionMarkAttributes.LoadAttributes(CCommonPara.mSectionMarkNotePath);

            View sectionView = null;
            SectionMark setionMark = null;

            double sectionMinY = 0.0;
            double sectionMaxY = 0.0;
            double dblY = 50;

            mrSection.GetSectionMinYAndMaxY(ref sectionMinY, ref sectionMaxY);

            Point startPt = new Point(mrSection.mSectionMidX, sectionMaxY + dblY, 0);
            Point endPt = new Point(mrSection.mSectionMidX, sectionMinY - dblY, 0);

            if (CCommonPara.mHorizontalSection == MrSectionOrientation.MrSectionLeft)
            {
                Point tempPt = startPt;
                startPt = endPt;
                endPt = tempPt;
            }

            bool bNeedCreateView = IsTheSectionNeedCreateView(mrSection, ref mrSameSection);

            if (bNeedCreateView)
            {
                mSectionMarkIndex++;
                mrSection.mSectionMark = mSectionMarkArray[mSectionMarkIndex];

                View.CreateSectionView(mFrontView, startPt, endPt, new Point(0, 0, 0), CCommonPara.mDblSectionUpDepth
                , CCommonPara.mDblSectionDownDepth, viewAttributes, sectionMarkAttributes, out sectionView, out setionMark);
            }
            else if (CCommonPara.mbShowSameSectionMark)
            {
                sectionMarkAttributes.MarkName = mrSameSection.mSectionMark;
                setionMark = new SectionMark(mFrontView, startPt, endPt, sectionMarkAttributes);
                setionMark.Insert();
                mFrontView.Modify();
            }
        }

        /// <summary>
        /// 创建垂直方向的剖面;
        /// </summary>
        private void CreateVerticalSection(CMrSection mrSection)
        {
            //判断是否需要创建该剖面或者只是添加零件标记;
            CMrSection mrSameSection = null;

            View.ViewAttributes viewAttributes = new View.ViewAttributes();
            viewAttributes.LoadAttributes(CCommonPara.mSectionAttPath);

            SectionMarkBase.SectionMarkAttributes sectionMarkAttributes = new SectionMarkBase.SectionMarkAttributes();
            sectionMarkAttributes.LoadAttributes(CCommonPara.mSectionMarkNotePath);

            View sectionView = null;
            SectionMark setionMark = null;

            double dblX = 50;
            Point sectionMinXPt = null;
            Point sectionMaxXPt = null;
          
            mrSection.GetSectionMinXAndMaxXPoint(ref sectionMinXPt, ref sectionMaxXPt);

            Point startPt = new Point(sectionMaxXPt.X + dblX, mrSection.mSectionMidY, 0);
            Point endPt = new Point(sectionMinXPt.X - dblX, mrSection.mSectionMidY, 0);
         
            if (CCommonPara.mVerticalSection == MrSectionOrientation.MrSectionUp)
            {
                Point tempPt = startPt;
                startPt = endPt;
                endPt = tempPt;
            }

            bool bNeedCreateView = IsTheSectionNeedCreateView(mrSection, ref mrSameSection);

            if (bNeedCreateView)
            {
                mSectionMarkIndex++;
                mrSection.mSectionMark = mSectionMarkArray[mSectionMarkIndex];

                View.CreateSectionView(mFrontView, startPt, endPt, new Point(0, 0, 0), CCommonPara.mDblSectionUpDepth
                , CCommonPara.mDblSectionDownDepth, viewAttributes, sectionMarkAttributes, out sectionView, out setionMark);
            }
            else if (CCommonPara.mbShowSameSectionMark)
            {
                sectionMarkAttributes.MarkName = mrSameSection.mSectionMark;
                setionMark = new SectionMark(mFrontView, startPt, endPt, sectionMarkAttributes);
                setionMark.Insert();
                mFrontView.Modify();
            }
        }

        /// <summary>
        /// 判断是否需要创建剖面视图;
        /// </summary>
        /// <param name="mrSection"></param>
        /// <param name="mrSameSection"></param>
        /// <returns></returns>
        private bool IsTheSectionNeedCreateView(CMrSection mrSection, ref CMrSection mrSameSection)
        {
            foreach (CMrSection section in mSectionList)
            {
                if (section == mrSection)
                {
                    continue;
                }
                bool bRes = section.IsSameSection(mrSection);

                if (bRes)
                {
                    mrSameSection = section;
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 判断零件是否已经在剖面中;
        /// </summary>
        /// <param name="mrPart"></param>
        /// <returns></returns>
        private bool IsPartInSection(CMrPart mrPart,MrSectionMode sectionMode)
        {
            foreach (CMrSection mrSection in mSectionList)
            {
                if ((sectionMode!=MrSectionMode.None) && (mrSection.mSectionMode != sectionMode))
                {
                    continue;
                }
                if (mrSection.IsHaveThePart(mrPart))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 把零件加到该剖面中;
        /// </summary>
        /// <param name="mrSection"></param>
        private void AddPartInSection(CMrSection mrSection)
        {
            if (mrSection.mSectionMode == MrSectionMode.MrHorizontal)
            {
                double sectionMidX = mrSection.mSectionMidX;
                double sectionMinY =mrSection.mSectionMinY;
                double sectionMaxY =mrSection.mSectionMaxY;
                double dblLeftX = 0.0;
                double dblRightX = 0.0;

                if (CCommonPara.mHorizontalSection == MrSectionOrientation.MrSectionLeft)
                {
                    dblLeftX = sectionMidX - CCommonPara.mDblSectionUpDepth;
                    dblRightX = sectionMidX + CCommonPara.mDblSectionDownDepth;
                }
                else if (CCommonPara.mHorizontalSection == MrSectionOrientation.MrSectionRight)
                {
                    dblLeftX = sectionMidX - CCommonPara.mDblSectionDownDepth;
                    dblRightX = sectionMidX + CCommonPara.mDblSectionUpDepth;
                }
                foreach (CMrPart mrPart in mAllSectionPartList)
                {
                    double dblMinX = mrPart.GetMinXPoint().X;
                    double dblMaxX = mrPart.GetMaxXPoint().X;
                    double dblMaxY = mrPart.GetMaxYPoint().Y;
                    double dblMinY = mrPart.GetMinYPoint().Y;

                    if (dblMaxX < dblLeftX || dblMinX > dblRightX)
                    {
                        continue;
                    }
                    if (dblMaxY < sectionMinY || dblMinY > sectionMaxY)
                    {
                        continue;
                    }

                    mrSection.AppendSectionPart(mrPart);
                }
            }
            else if (mrSection.mSectionMode == MrSectionMode.MrVertical)
            {
                double sectionMidY = mrSection.mSectionMidY;
                double dblTopY = 0.0;
                double dblBottomY = 0.0;

                if (CCommonPara.mVerticalSection == MrSectionOrientation.MrSectionUp)
                {
                    dblTopY = sectionMidY + CCommonPara.mDblSectionUpDepth;
                    dblBottomY = sectionMidY - CCommonPara.mDblSectionDownDepth;
                }
                else if (CCommonPara.mVerticalSection == MrSectionOrientation.MrSectionDown)
                {
                    dblTopY = sectionMidY + CCommonPara.mDblSectionUpDepth;
                    dblBottomY = sectionMidY - CCommonPara.mDblSectionDownDepth;
                }
                foreach (CMrPart mrPart in mAllSectionPartList)
                {
                    double dblMinY = mrPart.GetMinYPoint().Y;
                    double dblMaxY = mrPart.GetMaxYPoint().Y;

                    if (dblMaxY < dblBottomY || dblMinY > dblTopY)
                    {
                        continue;
                    }
                    mrSection.AppendSectionPart(mrPart);
                }
            }
        }
    }
}
