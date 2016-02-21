using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tekla.Structures.Drawing;
using Tekla.Structures.Model;

using TSD=Tekla.Structures.Drawing;
using TSM=Tekla.Structures.Model;
using AutoDimension.Entity;
using Tekla.Structures.Geometry3d;

namespace AutoDimension
{
    /// <summary>
    /// 门式框架结构自动剖面类;
    /// </summary>
    public class CBeamAutoSectionDoor
    {
         /// <summary>
        /// 剖面标记的索引数组;
        /// </summary>
        public string[] mSectionMarkArray = { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N","O","P","Q","R" };

        /// <summary>
        /// 主梁对象;
        /// </summary>
        public TSM.Part mMainPart=null;

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
        private CBeamDoorFrontView mBeamFrontView=null;

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
        public CBeamAutoSectionDoor(TSM.Part mainPart, AssemblyDrawing assemblyDrawing)
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

            if (CMrBeamDoorManager.GetInstance().mType == MrBeamDoorType.TypeNormal)
            {
                BuildNormalNeedSectionPartList();
                CreateNormalSection();
            }
            else if(CMrBeamDoorManager.GetInstance().mType ==MrBeamDoorType.TypeMiddle1
                || CMrBeamDoorManager.GetInstance().mType == MrBeamDoorType.TypeMiddle2
                || CMrBeamDoorManager.GetInstance().mType == MrBeamDoorType.TypeMiddle3)
            {
                BuildMiddleNeedSectionPartList();
                CreateMiddleSection();
            }
            mAssemblyDrawing.PlaceViews();
            mAssemblyDrawing.Modify();
        }

        /// <summary>
        /// 获取图纸中视图信息;
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

                int size = view.GetObjects(new Type[]{typeof(TSD.Part)}).GetSize();

                if (size == 0||size==1)
                {
                    continue;
                }

                Vector xVector=new Vector(1,0,0);
                Vector yVector=new Vector(0,1,0);
                Vector zVector=new Vector(0,0,1);

                CDimTools.GetInstance().InitMrPart(mMainPart, view, mainBeam);

                Vector vector = mainBeam.mNormal;
                double mainBeamWidth = Math.Abs(mainBeam.GetMaxXPoint().X - mainBeam.GetMinXPoint().X);

                Point viewMinPoint = view.RestrictionBox.MinPoint;
                Point viewMaxPoint = view.RestrictionBox.MaxPoint;
                double viewWidth = Math.Abs(viewMaxPoint.X - viewMinPoint.X);
               
                //顶视图标注;
                if (CDimTools.GetInstance().IsTwoVectorParallel(vector, yVector)
                    && CDimTools.GetInstance().CompareTwoDoubleValue(viewWidth, mainBeamWidth) >= 0)
                {

                }
                //前视图标注;
                else if (CDimTools.GetInstance().IsTwoVectorParallel(vector, zVector)&&
                    CDimTools.GetInstance().CompareTwoDoubleValue(viewWidth, mainBeamWidth) >= 0)
                {
                       mFrontView = view;
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

            mBeamFrontView = new CBeamDoorFrontView(mFrontView, CDimManager.GetInstance().GetModel());
            mBeamFrontView.InitFrontView();
            mrPartList = mBeamFrontView.GetMrPartList();
            mFrontView.SetUserProperty(CCommonPara.mAutoSectionPropKey, CCommonPara.mUserPropValue);
        }

        /// <summary>
        /// 构建正常情况下需要创建剖面的零部件;
        /// </summary>
        private void BuildNormalNeedSectionPartList()
        {
            CMrPart mLeftBeam = CMrBeamDoorManager.GetInstance().mLeftBeam;
            CMrPart mRightBeam = CMrBeamDoorManager.GetInstance().mRightBeam;
            CMrPart mTopBeam = CMrBeamDoorManager.GetInstance().mTopBeam;
            CMrPart mBottomBeam = CMrBeamDoorManager.GetInstance().mBottonBeam;

            //1.左右侧挡板需要进行剖视;
            mAllSectionPartList.Add(mLeftBeam);
            mAllSectionPartList.Add(mRightBeam);

            Point leftTopPt = mTopBeam.mLeftTopPoint;
            Point rightTopPt = mTopBeam.mRightTopPoint;

            Point leftBottomPt = mBottomBeam.mLeftBottomPoint;
            Point rightBottomPt = mBottomBeam.mRightBottomPoint;

            Vector directVector = new Vector(rightTopPt.X - leftTopPt.X, rightTopPt.Y - leftTopPt.Y, rightTopPt.Z - leftTopPt.Z);

            mrPartList.Remove(CMrMainBeam.GetInstance());
            mrPartList.Remove(mTopBeam);
            mrPartList.Remove(mBottomBeam);

            foreach (CMrPart mrPart in mrPartList)
            {
                //排除在主梁外面的零部件;
                if (CDimTools.GetInstance().IsThePointOnLine(mrPart.GetMaxYPoint(), leftTopPt, rightTopPt) > 0 ||
                    CDimTools.GetInstance().IsThePointOnLine(mrPart.GetMinYPoint(), leftBottomPt, rightBottomPt) < 0)
                {
                    continue;
                }

                Vector normal = mrPart.mNormal;

                //判断主梁的螺钉是否在板内，如果在则需要剖面;
                if (CDimTools.GetInstance().IsTwoVectorParallel(normal, new Vector(0, 0, 1)))
                {
                    if (JudgeMainBeamBoltInPlate(mrPart))
                    {
                        mAllSectionPartList.Add(mrPart);
                    }
                }

                if (!mrPart.IsHaveBolt())
                {
                    continue;
                }
                if (CDimTools.GetInstance().IsTwoVectorParallel(normal, new Vector(1, 0, 0)) ||
                    CDimTools.GetInstance().IsTwoVectorParallel(normal, new Vector(0, 0, 1)) ||
                    CDimTools.GetInstance().IsTwoVectorParallel(normal, directVector) ||
                    CDimTools.GetInstance().IsTwoVectorVertical(normal, directVector))
                {
                    mAllSectionPartList.Add(mrPart);
                }
            }
        }

        /// <summary>
        /// 判断主梁的螺钉是否在该平板内;
        /// </summary>
        private bool JudgeMainBeamBoltInPlate(CMrPart mrPart)
        {
            List<CMrBoltArray> mrBoltArrayList = CMrMainBeam.GetInstance().GetBoltArrayList();

            double minX = mrPart.GetMinXPoint().X;
            double maxX = mrPart.GetMaxXPoint().X;
            double minY = mrPart.GetMinYPoint().Y;
            double maxY = mrPart.GetMaxYPoint().Y;

            foreach (CMrBoltArray mrBoltArray in mrBoltArrayList)
            {
                List<Point> pointList = mrBoltArray.GetBoltPointList();

                foreach (Point pt in pointList)
                {
                    if (pt.X < maxX && pt.X > minX && pt.Y < maxY && pt.Y > minY)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 创建门式框架结构左右两边梁的剖面;
        /// </summary>
        private void CreateNormalSection()
        {
            CDimTools.GetInstance().SortMrPartByMinX(mAllSectionPartList);

            CMrPart mLeftBeam = CMrBeamDoorManager.GetInstance().mLeftBeam;
            CMrPart mRightBeam = CMrBeamDoorManager.GetInstance().mRightBeam;
            CMrPart mTopBeam = CMrBeamDoorManager.GetInstance().mTopBeam;

            Point leftTopPt = mTopBeam.mLeftTopPoint;
            Point rightTopPt = mTopBeam.mRightTopPoint;

            Vector directVector = new Vector(rightTopPt.X - leftTopPt.X, rightTopPt.Y - leftTopPt.Y, rightTopPt.Z - leftTopPt.Z);

            foreach (CMrPart mrPart in mAllSectionPartList)
            {
                Vector normal = mrPart.mNormal;

                CMrSection mrSection = new CMrSection(MrSectionType.MrSectionBeamDoor);
                mrSection.AppendSectionPart(mrPart);

                if (CDimTools.GetInstance().IsTwoVectorParallel(normal, directVector)||
                    CDimTools.GetInstance().IsTwoVectorParallel(normal, new Vector(1,0,0)))
                {
                    Point minXPt = mrPart.GetMinXPoint();
                    Point maxXpt = mrPart.GetMaxXPoint();

                    mrSection.mSectionMidX = (minXPt.X + maxXpt.X) / 2.0;
                    mrSection.mSectionMode = MrSectionMode.MrHorizontal;
                }
                else if (CDimTools.GetInstance().IsTwoVectorParallel(normal, new Vector(0, 0, 1))||
                    CDimTools.GetInstance().IsTwoVectorVertical(normal, directVector))
                {
                    Point minXPt = mrPart.GetMinXPoint();
                    Point maxXPt = mrPart.GetMaxXPoint();
                    Point minYPt = mrPart.GetMinYPoint();
                    Point maxYpt = mrPart.GetMaxYPoint();

                    mrSection.mSectionMinX = minXPt.X;
                    mrSection.mSectionMaxX = maxXPt.X;
                    mrSection.mSectionMidY = (minYPt.Y + maxYpt.Y) / 2.0;
                    mrSection.mSectionMode = MrSectionMode.MrVertical;
                }

                //1.如果零部件已经存在于剖面中则直接返回;
                if (IsPartInSection(mrPart, mrSection.mSectionMode))
                {
                    continue;
                }

                //2.把所有属于该剖面的零部件加入;
                AddPartInSection(mrSection);

                mSectionList.Add(mrSection);

                if (mrSection.mSectionMode == MrSectionMode.MrHorizontal)
                {
                    CreateHorizontalNormalSection(mrSection,mrPart);
                }
                else if (mrSection.mSectionMode == MrSectionMode.MrVertical)
                {
                    CreateVerticalNormalSection(mrSection,mrPart);
                }
            }
        }

        /// <summary>
        /// 创建水平方向的剖面;
        /// </summary>
        /// <param name="mrSection"></param>
        private void CreateHorizontalNormalSection(CMrSection mrSection,CMrPart mrPart)
        {
            CMrPart mTopBeam = CMrBeamDoorManager.GetInstance().mTopBeam;
            CMrPart mBottomBeam = CMrBeamDoorManager.GetInstance().mBottonBeam;

            Point tBLeftTopPt = mTopBeam.mLeftTopPoint;
            Point tBRightTopPt = mTopBeam.mRightTopPoint;

            Point bBLeftTopPt = mBottomBeam.mLeftTopPoint;
            Point bBRightTopPt = mBottomBeam.mRightTopPoint;

            //1.判断是否需要创建该剖面或者只是添加零件标记;
            CMrSection mrSameSection = null;

            View.ViewAttributes viewAttributes = new View.ViewAttributes();
            viewAttributes.LoadAttributes(CCommonPara.mSectionAttPath);

            SectionMarkBase.SectionMarkAttributes sectionMarkAttributes = new SectionMarkBase.SectionMarkAttributes();
            sectionMarkAttributes.LoadAttributes(CCommonPara.mSectionMarkNotePath);

            View sectionView = null;
            SectionMark setionMark = null;

            Point sectionMinYPt = new Point();
            Point sectionMaxYPt = new Point();

            mrSection.GetSectionMinYAndMaxYPoint(ref sectionMinYPt, ref sectionMaxYPt);

            double dblY=50;

            Point startPt = null;
            Point endPt = null;

            Point partLeftBottomPt = mrPart.mLeftBottomPoint;
            Point partLeftTopPt = mrPart.mLeftTopPoint;

            //2.如果Y值最大的点在顶部梁的上方;
            if (CDimTools.GetInstance().IsThePointOnLine(sectionMaxYPt, tBLeftTopPt, tBRightTopPt) > 0)
            {
                Point newMaxYPt = new Point(sectionMaxYPt.X, sectionMaxYPt.Y + dblY, 0);
                Point footPt = CDimTools.GetInstance().ComputeFootPointToLine(newMaxYPt, partLeftBottomPt, partLeftTopPt);
                startPt = footPt;
            }
            else
            {
                Point footPt=CDimTools.GetInstance().ComputeFootPointToLine(partLeftTopPt, tBLeftTopPt, tBRightTopPt);
                Point newMaxYPt = new Point(footPt.X, footPt.Y + dblY, 0);
                footPt = CDimTools.GetInstance().ComputeFootPointToLine(newMaxYPt, partLeftBottomPt, partLeftTopPt);
                startPt = footPt;
            }

            //3.如果Y值最小的点在底部梁的下方;
            if (CDimTools.GetInstance().IsThePointOnLine(sectionMinYPt, bBLeftTopPt, bBRightTopPt) < 0)
            {
                Point newMinYPt = new Point(sectionMinYPt.X, sectionMinYPt.Y - dblY, 0);
                Point footPt = CDimTools.GetInstance().ComputeFootPointToLine(newMinYPt, partLeftBottomPt, partLeftTopPt);
                endPt = footPt;
            }
            else
            {
                Point footPt = CDimTools.GetInstance().ComputeFootPointToLine(partLeftBottomPt, bBLeftTopPt, bBRightTopPt);
                Point newMinYPt = new Point(footPt.X, footPt.Y - dblY, 0);
                footPt = CDimTools.GetInstance().ComputeFootPointToLine(newMinYPt, partLeftBottomPt, partLeftTopPt);
                endPt = footPt;
            }
  
            if (CCommonPara.mHorizontalSection == MrSectionOrientation.MrSectionRight)
            {
                Point tempPt = startPt;
                startPt = endPt;
                endPt = startPt;
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
        /// 创建竖直方向上的剖面;
        /// </summary>
        /// <param name="mrSection"></param>
        /// <param name="mrPart"></param>
        private void CreateVerticalNormalSection(CMrSection mrSection, CMrPart mrPart)
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

            CMrPart mTopBeam = CMrBeamDoorManager.GetInstance().mTopBeam;
            Point tBLeftTopPt = mTopBeam.mLeftTopPoint;
            Point tBRightTopPt = mTopBeam.mRightTopPoint;
            Vector directVector = new Vector(tBRightTopPt.X - tBLeftTopPt.X, tBRightTopPt.Y - tBLeftTopPt.Y, tBRightTopPt.Z - tBLeftTopPt.Z);

            Point leftTopPt = null;
            Point rightTopPt = null;

            if(CDimTools.GetInstance().JudgeLineSlope(new Point(0,0,0),directVector)==MrSlopeType.MORETHAN_ZERO)
            {
                leftTopPt = mrPart.GetMinXPoint();
                rightTopPt = mrPart.GetMaxYPoint();
            }
            else if(CDimTools.GetInstance().JudgeLineSlope(new Point(0,0,0),directVector)==MrSlopeType.LESSTHAN_ZERO)
            {
                leftTopPt = mrPart.GetMaxYPoint();
                rightTopPt = mrPart.GetMaxXPoint();
            }
            else
            {
                leftTopPt = mrPart.GetMinXMaxYPoint();
                rightTopPt = mrPart.GetMaxXMaxYPoint();
            }

            Point newPt = new Point(rightTopPt.X + dblX, rightTopPt.Y, 0);
            Point startPt = CDimTools.GetInstance().ComputeFootPointToLine(newPt, leftTopPt, rightTopPt);

            newPt = new Point(leftTopPt.X - dblX, leftTopPt.Y, 0);
            Point endPt = CDimTools.GetInstance().ComputeFootPointToLine(newPt, leftTopPt, rightTopPt);

            if (CCommonPara.mVerticalSection == MrSectionOrientation.MrSectionDown)
            {
                Point tempPt = startPt;
                startPt = endPt;
                endPt = startPt;
            }

            bool bNeedCreateView = IsTheSectionNeedCreateView(mrSection, ref mrSameSection);

            if (bNeedCreateView)
            {
                mSectionMarkIndex++;
                mrSection.mSectionMark = mSectionMarkArray[mSectionMarkIndex];

                View.CreateSectionView(mFrontView, startPt, endPt, new Point(0, 0, 0), CCommonPara.mDblSectionUpDepth
                , CCommonPara.mDblSectionDownDepth, viewAttributes, sectionMarkAttributes, out sectionView, out setionMark);
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
        private bool IsPartInSection(CMrPart mrPart, MrSectionMode sectionMode)
        {
            foreach (CMrSection mrSection in mSectionList)
            {
                if (mrSection.mSectionMode != sectionMode)
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
                foreach (CMrPart mrPart in mrPartList)
                {
                    double dblMinX = mrPart.GetMinXPoint().X;
                    double dblMaxX = mrPart.GetMaxXPoint().X;
                    double dblMaxY = mrPart.GetMaxYPoint().Y;
                    double dblMinY = mrPart.GetMinYPoint().Y;

                    if (dblMaxX < dblLeftX || dblMinX > dblRightX)
                    {
                        continue;
                    }
                    mrSection.AppendSectionPart(mrPart);
                }
            }
            else if (mrSection.mSectionMode == MrSectionMode.MrVertical)
            {
                double sectionMidY = mrSection.mSectionMidY;
                double sectionMinX = mrSection.mSectionMinX;
                double sectionMaxX = mrSection.mSectionMaxX;
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
                    double dblMinX = mrPart.GetMinXPoint().X;
                    double dblMaxX = mrPart.GetMaxXPoint().X;

                    if (dblMaxY < dblBottomY || dblMinY > dblTopY)
                    {
                        continue;
                    }
                    if (dblMaxX < sectionMinX || dblMinX > sectionMaxX)
                    {
                        continue;
                    }

                    mrSection.AppendSectionPart(mrPart);
                }
            }
        }

        /// <summary>
        /// 构建中间需要进行剖面的零部件;
        /// </summary>
        private void BuildMiddleNeedSectionPartList()
        {
            CMrPart mLeftBeam = CMrBeamDoorManager.GetInstance().mLeftBeam;
            CMrPart mRightBeam = CMrBeamDoorManager.GetInstance().mRightBeam;
            CMrPart mTopBeam = CMrBeamDoorManager.GetInstance().mTopBeam;
            CMrPart mLeftBottomBeam = CMrBeamDoorManager.GetInstance().mLeftBottomBeam;
            CMrPart mRightBottomBeam = CMrBeamDoorManager.GetInstance().mRightBottomBeam;

            //1.左右侧挡板需要进行剖视;
            mAllSectionPartList.Add(mLeftBeam);
            mAllSectionPartList.Add(mRightBeam);

            Vector leftTopVector = CMrBeamDoorManager.GetInstance().mLeftTopVector;
            Vector rightTopVector = CMrBeamDoorManager.GetInstance().mRightTopVector;
            Point midMaxYPt = CMrBeamDoorManager.GetInstance().mMidMaxPoint;

            Point leftTopPt = mTopBeam.mLeftTopPoint;
            Point rightTopPt = mTopBeam.mRightTopPoint;
            Point leftBottomPt = mLeftBottomBeam.mLeftBottomPoint;
            Point rightBottomPt = mRightBottomBeam.mRightBottomPoint;

            Vector leftDirectVector = new Vector(midMaxYPt.X - leftTopPt.X, midMaxYPt.Y - leftTopPt.Y, 0);
            Vector rightDirectVector = new Vector(midMaxYPt.X - rightTopPt.X, midMaxYPt.Y - rightTopPt.Y, 0);

            mrPartList.Remove(CMrMainBeam.GetInstance());
            mrPartList.Remove(mTopBeam);
            mrPartList.Remove(mLeftBottomBeam);
            mrPartList.Remove(mRightBottomBeam);

            foreach (CMrPart mrPart in mrPartList)
            {
                //排除在主梁外面的零部件;
                if (CDimTools.GetInstance().IsThePointOnLine(mrPart.GetMaxYPoint(), leftTopPt, midMaxYPt) > 0 ||
                    CDimTools.GetInstance().IsThePointOnLine(mrPart.GetMaxYPoint(), rightTopPt, midMaxYPt) > 0 )
                {
                    continue;
                }

                Vector normal = mrPart.mNormal;

                //判断主梁的螺钉是否在板内，如果在则需要剖面,该板中间会有孔，但是没有螺栓;
                if (CDimTools.GetInstance().IsTwoVectorParallel(normal, new Vector(0, 0, 1)))
                {
                    if (JudgeMainBeamBoltInPlate(mrPart))
                    {
                        mAllSectionPartList.Add(mrPart);
                    }
                }

                if (!mrPart.IsHaveBolt())
                {
                    continue;
                }
                if (CDimTools.GetInstance().IsTwoVectorParallel(normal, new Vector(1, 0, 0)) ||
                    CDimTools.GetInstance().IsTwoVectorParallel(normal, new Vector(0, 1, 0)) ||
                    CDimTools.GetInstance().IsTwoVectorParallel(normal, leftDirectVector) ||
                    CDimTools.GetInstance().IsTwoVectorVertical(normal, leftDirectVector) ||
                    CDimTools.GetInstance().IsTwoVectorParallel(normal, rightDirectVector) ||
                    CDimTools.GetInstance().IsTwoVectorVertical(normal, rightDirectVector))
                {
                    mAllSectionPartList.Add(mrPart);
                }
            }
        }

        /// <summary>
        /// 创建门式框架结构中间梁的剖面;
        /// </summary>
        private void CreateMiddleSection()
        {
            CDimTools.GetInstance().SortMrPartByMinX(mAllSectionPartList);

            CMrPart mLeftBeam = CMrBeamDoorManager.GetInstance().mLeftBeam;
            CMrPart mRightBeam = CMrBeamDoorManager.GetInstance().mRightBeam;
            CMrPart mTopBeam = CMrBeamDoorManager.GetInstance().mTopBeam;
            
            Point midMaxPoint = CMrBeamDoorManager.GetInstance().mMidMaxPoint;
            Point leftTopPt = mTopBeam.mLeftTopPoint;
            Point rightTopPt = mTopBeam.mRightTopPoint;

            Vector leftDirectVector = new Vector(midMaxPoint.X - leftTopPt.X, midMaxPoint.Y - leftTopPt.Y, 0);
            Vector rightDirectVector = new Vector(midMaxPoint.X - rightTopPt.X, midMaxPoint.Y - rightTopPt.Y, 0);

            foreach (CMrPart mrPart in mAllSectionPartList)
            {
                Vector normal = mrPart.mNormal;

                CMrSection mrSection = new CMrSection(MrSectionType.MrSectionBeamDoor);
                mrSection.AppendSectionPart(mrPart);

                if (CDimTools.GetInstance().IsTwoVectorParallel(normal, leftDirectVector) ||
                    CDimTools.GetInstance().IsTwoVectorParallel(normal, rightDirectVector) ||
                    CDimTools.GetInstance().IsTwoVectorParallel(normal, new Vector(1, 0, 0)))
                {
                    Point minXPt = mrPart.GetMinXPoint();
                    Point maxXpt = mrPart.GetMaxXPoint();
                    mrSection.mSectionMidX = (minXPt.X + maxXpt.X) / 2.0;
                    mrSection.mSectionMode = MrSectionMode.MrHorizontal;
                }
                else if (CDimTools.GetInstance().IsTwoVectorParallel(normal, new Vector(0, 0, 1)) ||
                    CDimTools.GetInstance().IsTwoVectorParallel(normal, new Vector(0, 1, 0)) ||
                    CDimTools.GetInstance().IsTwoVectorVertical(normal, leftDirectVector) ||
                    CDimTools.GetInstance().IsTwoVectorVertical(normal, rightDirectVector))
                {
                    Point minXPt = mrPart.GetMinXPoint();
                    Point maxXPt = mrPart.GetMaxXPoint();
                    Point minYPt = mrPart.GetMinYPoint();
                    Point maxYpt = mrPart.GetMaxYPoint();

                    mrSection.mSectionMinX = minXPt.X;
                    mrSection.mSectionMaxX = maxXPt.X;
                    mrSection.mSectionMidY = (minYPt.Y + maxYpt.Y) / 2.0;
                    mrSection.mSectionMode = MrSectionMode.MrVertical;
                }

                //1.如果零部件已经存在于剖面中则直接返回;
                if (IsPartInSection(mrPart, mrSection.mSectionMode))
                {
                    continue;
                }

                //2.把所有属于该剖面的零部件加入;
                AddPartInSection(mrSection);

                mSectionList.Add(mrSection);

                if (mrSection.mSectionMode == MrSectionMode.MrHorizontal)
                {
                    CreateHorizontalMiddleSection(mrSection, mrPart);
                }
                else if (mrSection.mSectionMode == MrSectionMode.MrVertical)
                {
                    CreateVerticalMiddleSection(mrSection, mrPart);
                }
            }
        }

        /// <summary>
        /// 创建水平方向的剖面;
        /// </summary>
        /// <param name="mrSection"></param>
        private void CreateHorizontalMiddleSection(CMrSection mrSection, CMrPart mrPart)
        {
            CMrPart mTopBeam = CMrBeamDoorManager.GetInstance().mTopBeam;
            CMrPart mLeftBottomBeam = CMrBeamDoorManager.GetInstance().mLeftBottomBeam;
            CMrPart mRightBottomBeam = CMrBeamDoorManager.GetInstance().mRightBottomBeam;

            //1.判断是否需要创建该剖面或者只是添加零件标记;
            CMrSection mrSameSection = null;

            View.ViewAttributes viewAttributes = new View.ViewAttributes();
            viewAttributes.LoadAttributes(CCommonPara.mSectionAttPath);

            SectionMarkBase.SectionMarkAttributes sectionMarkAttributes = new SectionMarkBase.SectionMarkAttributes();
            sectionMarkAttributes.LoadAttributes(CCommonPara.mSectionMarkNotePath);

            View sectionView = null;
            SectionMark setionMark = null;

            Point sectionMinYPt = new Point();
            Point sectionMaxYPt = new Point();

            mrSection.GetSectionMinYAndMaxYPoint(ref sectionMinYPt, ref sectionMaxYPt);

            double dblY = 50;

            Point startPt = null;
            Point endPt = null;
            
            Point tBLeftTopPt = null;
            Point tBRightTopPt = null;
            
            Point bBLeftTopPt = null;
            Point bBRightTopPt = null;

            Point maxXPt = mrPart.GetMaxXPoint();
            Point midMaxYPt = CMrBeamDoorManager.GetInstance().mMidMaxPoint;

            if (maxXPt.X < midMaxYPt.X)
            {
                tBLeftTopPt = mTopBeam.mLeftTopPoint;
                tBRightTopPt = midMaxYPt;

                bBLeftTopPt = mLeftBottomBeam.mLeftTopPoint;
                bBRightTopPt = mLeftBottomBeam.mRightTopPoint;
            }
            else if (maxXPt.X > midMaxYPt.X)
            {
                tBLeftTopPt = midMaxYPt;
                tBRightTopPt = mTopBeam.mRightTopPoint;

                bBLeftTopPt = mRightBottomBeam.mLeftTopPoint;
                bBRightTopPt = mRightBottomBeam.mRightTopPoint;
            }
           
            Point partLeftBottomPt = mrPart.mLeftBottomPoint;
            Point partLeftTopPt = mrPart.mLeftTopPoint;

            //2.如果Y值最大的点在顶部梁的上方;
            if (CDimTools.GetInstance().IsThePointOnLine(sectionMaxYPt, tBLeftTopPt, tBRightTopPt) > 0)
            {
                Point newMaxYPt = new Point(sectionMaxYPt.X, sectionMaxYPt.Y + dblY, 0);
                Point footPt = CDimTools.GetInstance().ComputeFootPointToLine(newMaxYPt, partLeftBottomPt, partLeftTopPt);
                startPt = footPt;
            }
            else
            {
                Point footPt = CDimTools.GetInstance().ComputeFootPointToLine(partLeftTopPt, tBLeftTopPt, tBRightTopPt);
                Point newMaxYPt = new Point(footPt.X, footPt.Y + dblY, 0);
                footPt = CDimTools.GetInstance().ComputeFootPointToLine(newMaxYPt, partLeftBottomPt, partLeftTopPt);
                startPt = footPt;
            }

            //3.如果Y值最小的点在底部梁的下方;
            if (CDimTools.GetInstance().IsThePointOnLine(sectionMinYPt, bBLeftTopPt, bBRightTopPt) < 0)
            {
                Point newMinYPt = new Point(sectionMinYPt.X, sectionMinYPt.Y - dblY, 0);
                Point footPt = CDimTools.GetInstance().ComputeFootPointToLine(newMinYPt, partLeftBottomPt, partLeftTopPt);
                endPt = footPt;
            }
            else
            {
                Point footPt = CDimTools.GetInstance().ComputeFootPointToLine(partLeftBottomPt, bBLeftTopPt, bBRightTopPt);
                Point newMinYPt = new Point(footPt.X, footPt.Y - dblY, 0);
                footPt = CDimTools.GetInstance().ComputeFootPointToLine(newMinYPt, partLeftBottomPt, partLeftTopPt);
                endPt = footPt;
            }

            if (CCommonPara.mHorizontalSection == MrSectionOrientation.MrSectionRight)
            {
                Point tempPt = startPt;
                startPt = endPt;
                endPt = startPt;
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
        /// 创建水平方向的剖面;
        /// </summary>
        /// <param name="mrSection"></param>
        private void CreateVerticalMiddleSection(CMrSection mrSection, CMrPart mrPart)
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

            CMrPart mTopBeam = CMrBeamDoorManager.GetInstance().mTopBeam;
            Point tBLeftTopPt = mTopBeam.mLeftTopPoint;
            Point tBRightTopPt = mTopBeam.mRightTopPoint;
            Point midMaxYPt = CMrBeamDoorManager.GetInstance().mMidMaxPoint;

            Vector leftDirectVector = new Vector(midMaxYPt.X - tBLeftTopPt.X, midMaxYPt.Y - tBLeftTopPt.Y, 0);
            Vector rightDirectVector = new Vector(midMaxYPt.X - tBRightTopPt.X, midMaxYPt.Y - tBRightTopPt.Y, 0);

            Point leftTopPt = null;
            Point rightTopPt = null;

            if (CDimTools.GetInstance().IsTwoVectorParallel(mrPart.mNormal, new Vector(0, 1, 0)))
            {                        
                leftTopPt = mrPart.GetMinXMaxYPoint();
                rightTopPt = mrPart.GetMaxXMaxYPoint();
            }
            else if (CDimTools.GetInstance().JudgeLineSlope(new Point(0, 0, 0), leftDirectVector) == MrSlopeType.MORETHAN_ZERO
                || CDimTools.GetInstance().JudgeLineSlope(new Point(0, 0, 0), rightDirectVector) == MrSlopeType.MORETHAN_ZERO)
            {
                leftTopPt = mrPart.GetMinXPoint();
                rightTopPt = mrPart.GetMaxYPoint();
            }
            else if (CDimTools.GetInstance().JudgeLineSlope(new Point(0, 0, 0), leftDirectVector) == MrSlopeType.LESSTHAN_ZERO
                || CDimTools.GetInstance().JudgeLineSlope(new Point(0, 0, 0), rightDirectVector) == MrSlopeType.LESSTHAN_ZERO)
            {
                leftTopPt = mrPart.GetMaxYPoint();
                rightTopPt = mrPart.GetMaxXPoint();
            }

            Point newPt = new Point(rightTopPt.X + dblX, rightTopPt.Y, 0);
            Point startPt = CDimTools.GetInstance().ComputeFootPointToLine(newPt, leftTopPt, rightTopPt);

            newPt = new Point(leftTopPt.X - dblX, leftTopPt.Y, 0);
            Point endPt = CDimTools.GetInstance().ComputeFootPointToLine(newPt, leftTopPt, rightTopPt);

            if (CCommonPara.mVerticalSection == MrSectionOrientation.MrSectionDown)
            {
                Point tempPt = startPt;
                startPt = endPt;
                endPt = startPt;
            }

            bool bNeedCreateView = IsTheSectionNeedCreateView(mrSection, ref mrSameSection);

            if (bNeedCreateView)
            {
                mSectionMarkIndex++;
                mrSection.mSectionMark = mSectionMarkArray[mSectionMarkIndex];

                View.CreateSectionView(mFrontView, startPt, endPt, new Point(0, 0, 0), CCommonPara.mDblSectionUpDepth
                , CCommonPara.mDblSectionDownDepth, viewAttributes, sectionMarkAttributes, out sectionView, out setionMark);
            }
        }
    }
}
