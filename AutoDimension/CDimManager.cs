using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tekla.Structures.Model;
using Tekla.Structures.Drawing;
using System.Windows.Forms;

using Tekla.Structures;
using Tekla.Structures.Geometry3d;

using TSD = Tekla.Structures.Drawing;
using TSM = Tekla.Structures.Model;
using AutoDimension.Entity;
using System.Threading;

namespace AutoDimension
{
    /// <summary>
    /// 标注管理类;
    /// </summary>
    public class CDimManager
    {
        /// <summary>
        /// 主窗口;
        /// </summary>
        public NewMainForm mMainForm = null;

        /// <summary>
        /// tekla的model对象;
        /// </summary>
        public static Model mModel = null;

        /// <summary>
        /// 单例;
        /// </summary>
        private static CDimManager mInstance = null;

        /// <summary>
        /// 判断是否重新获取了列表;
        /// </summary>
        private bool mbUpdate = false;

        /// <summary>
        /// 选择的所有标注对象的列表;
        /// </summary>
        public List<CMrAssemblyDrawing> mrAssemblyDrawingList = new List<CMrAssemblyDrawing>();

        /// <summary>
        /// 图纸标记与图纸标注类型（标注类型：柱标注和梁标注）;
        /// </summary>
        public Dictionary<string, string> mDicMarkNumberToType = new Dictionary<string, string>();

        /// <summary>
        /// 私有构造函数;
        /// </summary>
        private CDimManager()
        {

        }

        /// <summary>
        /// 获取唯一的实例;
        /// </summary>
        /// <returns></returns>
        public static CDimManager GetInstance()
        {
            if (null == mInstance)
            {
                mInstance = new CDimManager();
            }
            return mInstance;
        }

        /// <summary>
        /// 获取唯一的Model;
        /// </summary>
        /// <returns></returns>
        public Model GetModel()
        {
            return mModel;
        }

        /// <summary>
        /// 初始化;
        /// </summary>
        public void Init()
        {
            if(mModel==null)
            {
                mModel = new Model();
            }
        }

        /// <summary>
        /// 获取选择的图纸列表;
        /// </summary>
        public void GetSelDrawingList()
        {
            mbUpdate = true;

            mDicMarkNumberToType.Clear();
            mrAssemblyDrawingList.Clear();

            List<CMrAssemblyDrawing> assemblyDrawingList = CDimTools.GetInstance().GetSelMrAssemblyDrawingList();

            foreach (CMrAssemblyDrawing assemblyDrawing in assemblyDrawingList)
            {
                mrAssemblyDrawingList.Add(assemblyDrawing);
            }
        }

        /// <summary>
        /// 构建图纸编号与图纸标注类型的映射表;
        /// </summary>
        public void BuildDrawingMarkNumberDic()
        {
            if (mbUpdate == true)
            {
                foreach (CMrAssemblyDrawing drawing in mrAssemblyDrawingList)
                {
                    string strMarkNumber = drawing.GetMarkNumber();

                    if (!mDicMarkNumberToType.ContainsKey(strMarkNumber))
                    {
                        mDicMarkNumberToType.Add(strMarkNumber, "梁标注");
                    }
                }
                mbUpdate = false;
            }
        }

        /// <summary>
        /// 初始化图纸列表;
        /// </summary>
        public void InitMrAssemblyDrawingList()
        {
            if (mrAssemblyDrawingList.Count == 0)
            {
                return;
            }

            foreach (CMrAssemblyDrawing drawing in mrAssemblyDrawingList)
            {
                string strMarkNumber = drawing.GetMarkNumber();

                if (mDicMarkNumberToType.ContainsKey(strMarkNumber))
                {
                    string strValue = mDicMarkNumberToType[strMarkNumber];

                    drawing.mDimType = strValue;
                }
            }
        }

        /// <summary>
        /// 创建梁的前视图标注;
        /// </summary>
        public void CreateBeamFrontViewDim()
        {
            ViewBase frontViewBase = null;
            PointList pointList = new PointList();
            CDimTools.GetInstance().PickPoints(1, ref pointList, ref frontViewBase);

            if(frontViewBase!=null)
            {
                frontViewBase.Select();
                TSD.View frontView = frontViewBase as TSD.View;
                CBeamFrontView mFrontView = new CBeamFrontView(frontView, mModel);

                InitMainPart();
                InitView(frontView);
                mFrontView.CreateDim();
            }
        }

        /// <summary>
        /// 梁的顶视图标注;
        /// </summary>
        public void CreateBeamTopViewDim()
        {
            ViewBase topViewBase = null;
            PointList pointList = new PointList();
            CDimTools.GetInstance().PickPoints(1, ref pointList, ref topViewBase);

            if (topViewBase != null)
            {
                topViewBase.Select();

                TSD.View topView = topViewBase as TSD.View;
                CBeamTopView mTopView = new CBeamTopView(topView, mModel);

                InitMainPart();
                InitView(topView);
                mTopView.CreateDim();
            }
        }

        /// <summary>
        /// 梁的剖视图标注;
        /// </summary>
        public void CreateBeamSectionView()
        {
            ViewBase sectionViewBase = null;
            PointList pointList = new PointList();
            CDimTools.GetInstance().PickPoints(1, ref pointList, ref sectionViewBase);

            if(sectionViewBase!=null)
            {
                sectionViewBase.Select();
                TSD.View sectionView = sectionViewBase as TSD.View;
                CBeamSectionView mSectionView = new CBeamSectionView(sectionView, mModel);

                InitMainPart();
                InitView(sectionView);
                mSectionView.CreateDim();
            }
        }

        /// <summary>
        /// 线程图纸标注函数;
        /// </summary>
        /// <param name="message"></param>
        private void DimDrawingThreadFunc(object message)
        {
            DrawingHandler drawingHandler = new DrawingHandler();

            int nIndex = 0;
            int nCount = mrAssemblyDrawingList.Count;
            mMainForm.SetWholeProgressMax(nCount);

            string strCurrentTips = "";
            string strWholeTips = "";
            foreach (CMrAssemblyDrawing mrDrawing in mrAssemblyDrawingList)
            {
                nIndex++;
                mMainForm.SetWholeProgressPos(nIndex);
                strWholeTips = "总进度：" + nIndex + "/" + nCount.ToString();
                mMainForm.SetWholeLabelText(strWholeTips);

                int nViewIndex = 0;
                AssemblyDrawing assemblyDrawing = mrDrawing.mAssemblyDring;
                Identifier assemblyDrawingIdentifier = assemblyDrawing.AssemblyIdentifier;

                if (assemblyDrawing == null)
                {
                    continue;
                }
                try
                {
                    Assembly assembly = new Assembly
                    {
                        Identifier = assemblyDrawing.AssemblyIdentifier
                    };

                    assembly.Select();
                    Identifier identifier = assembly.GetMainPart().Identifier;
                    Beam modelObject = mModel.SelectModelObject(identifier) as Beam;
                    drawingHandler.SetActiveDrawing(assemblyDrawing, true);
                    DrawingObjectEnumerator allViews = assemblyDrawing.GetSheet().GetAllViews();

                    //1.判断是否需要创建自动剖面;
                    if (CCommonPara.mAutoSectionType == MrAutoSectionType.MrListDim || CCommonPara.mAutoSectionType == MrAutoSectionType.MrTwoTypeDim)
                    {
                        CreateAutoSection(mrDrawing.mDimType, modelObject, assemblyDrawing);
                    }

                    int nViewCount = allViews.GetSize();
                    mMainForm.SetCurrentProgressMax(nViewCount);
                   
                    while (allViews.MoveNext())
                    {
                        if (allViews.Current != null)
                        {
                            nViewIndex++;
                            mMainForm.SetCurrentProgressPos(nViewIndex);
                            strCurrentTips = "当前进度：" + nViewIndex + "/" + nViewCount.ToString();
                            mMainForm.SetCurrentLabelText(strCurrentTips);

                            TSD.View view = allViews.Current as TSD.View;
                            if (view != null)
                            {
                                DrawDrawingByView(view, modelObject, mrDrawing.mDimType);
                            }
                        }
                    }
                    assemblyDrawing.IsFrozen = true;
                    assemblyDrawing.PlaceViews();
                    assemblyDrawing.Modify();
                    drawingHandler.SaveActiveDrawing();
                    drawingHandler.CloseActiveDrawing();
                }
                catch (System.Exception ex)
                {
                    string strName = assemblyDrawing.Name;
                    MessageBox.Show("图纸" + strName + "标注失败,请确认模型或图纸内是否有问题。异常信息：" + ex.Message);
                    drawingHandler.SaveActiveDrawing();
                    drawingHandler.CloseActiveDrawing();

                    nIndex++;
                    mMainForm.SetWholeProgressPos(nIndex);
                    strWholeTips = "总进度：" + nIndex + "/" + nCount.ToString();
                    mMainForm.SetWholeLabelText(strWholeTips);
                    continue;
                }
            }

            MessageBox.Show("扬州Tekla培训石头哥提示您标注结束,感谢您对智能标注系统的支持。");
        }

        /// <summary>
        /// 绘制选择的多张图纸;
        /// </summary>
        public void DrawSelectListDrawing()
        {
            //启动初始化函数;
            Thread thread = new Thread(new ParameterizedThreadStart(DimDrawingThreadFunc));
            thread.Start();
        }

        /// <summary>
        /// 根据选择的View来进行标注;
        /// </summary>
        private void DrawDrawingByView(TSD.View view,TSM.Part mainPart,string strDimType)
        {
            view.Select();

            //创建自定义的主梁对象;
            CMrMainBeam mainBeam = new CMrMainBeam(mainPart, null);
            CMrMainBeam.SetInstance(mainBeam);

            double dblFlangeThickness = 0.0;
            double dblWebThickness = 0.0;

            mainPart.GetReportProperty("PROFILE.FLANGE_THICKNESS", ref dblFlangeThickness);
            mainPart.GetReportProperty("PROFILE.WEB_THICKNESS", ref dblWebThickness);

            mainBeam.mFlangeThickness = dblFlangeThickness;
            mainBeam.mWebThickness = dblWebThickness;

            int size = view.GetObjects(new Type[]{typeof(TSD.Part)}).GetSize();

            //如果零部件个数为0或1分别表示空视图和零件图则不进行标注;
            if (size == 0 ||size == 1)
            {
                return;
            }
            
            Vector xVector = new Vector(1, 0, 0);
            Vector yVector = new Vector(0, 1, 0);
            Vector zVector = new Vector(0, 0, 1);

            CDimTools.GetInstance().InitMrPart(mainPart, view, mainBeam);
            Vector vector = mainBeam.mNormal;

            double minY = mainBeam.GetMinYPoint().Y;
            double maxY = mainBeam.GetMaxYPoint().Y;
            double minX = mainBeam.GetMinXPoint().X;
            double maxX = mainBeam.GetMaxXPoint().X;

            double mainBeamHeight = Math.Abs(maxY - minY);
            double mainBeamWidth = Math.Abs(maxX - minX);

            if (strDimType == "梁标注")
            {
                Point viewMinPoint = view.RestrictionBox.MinPoint;
                Point viewMaxPoint = view.RestrictionBox.MaxPoint;
                double viewWidth = Math.Abs(viewMaxPoint.X - viewMinPoint.X);

                //顶视图标注;
                if (CDimTools.GetInstance().IsTwoVectorParallel(vector,yVector) &&
                     CDimTools.GetInstance().CompareTwoDoubleValue(viewWidth, mainBeamWidth) >= 0)
                {
                    CBeamTopView mTopView = new CBeamTopView(view, mModel);
                    InitView(view);
                    mTopView.CreateDim();
                }
                //主视图标注;
                else if (CDimTools.GetInstance().IsTwoVectorParallel(vector, zVector) &&
                     CDimTools.GetInstance().CompareTwoDoubleValue(viewWidth, mainBeamWidth) >= 0)
                {
                    CBeamFrontView mFrontView = new CBeamFrontView(view, mModel);
                    InitView(view);
                    mFrontView.CreateDim();
                }
                //剖视图标注;
                else 
                {
                    CBeamSectionView mSectionView = new CBeamSectionView(view, mModel);
                    InitView(view);
                    mSectionView.CreateDim();
                }
            }
            else if (strDimType == "柱标注")
            {
                Point viewMinPoint = view.RestrictionBox.MinPoint;
                Point viewMaxPoint = view.RestrictionBox.MaxPoint;
                double viewHeight = Math.Abs(viewMaxPoint.Y - viewMinPoint.Y);

                //顶视图标注;
                if (CDimTools.GetInstance().IsTwoVectorParallel(vector, xVector) && 
                    CDimTools.GetInstance().CompareTwoDoubleValue(viewHeight, mainBeamHeight) >= 0)
                {
                    CCylinderTopView mTopView = new CCylinderTopView(view, mModel);
                    InitView(view);
                    mTopView.CreateDim();
                }
                //主视图标注;
                else if (CDimTools.GetInstance().IsTwoVectorParallel(vector, zVector) &&
                    CDimTools.GetInstance().CompareTwoDoubleValue(viewHeight, mainBeamHeight) >= 0)
                {
                    CCylinderFrontView mFrontView = new CCylinderFrontView(view, mModel);
                    InitView(view);
                    mFrontView.CreateDim();
                }
                //剖视图标注;
                else
                {
                    CCylinderSectionView mSectionView = new CCylinderSectionView(view, mModel);
                    InitView(view);
                    mSectionView.CreateDim();
                }
            }
        }
        
        /// <summary>
        /// 初始化主梁对象;
        /// </summary>
        /// <returns></returns>
        private void InitMainPart()
        {
            DrawingHandler drawingHandler = new DrawingHandler();

	        if (mModel.GetConnectionStatus() && drawingHandler.GetConnectionStatus())
	        {
		        DrawingHandler.SetMessageExecutionStatus(DrawingHandler.MessageExecutionModeEnum.INSTANT);
		        AssemblyDrawing assemblyDrawing = drawingHandler.GetActiveDrawing() as AssemblyDrawing;
		        if (assemblyDrawing != null)
                {
                    Assembly assembly = new Assembly
				    {
					    Identifier = assemblyDrawing.AssemblyIdentifier
				    };

				    assembly.Select();
                    TSM.Part mainPart=assembly.GetMainPart() as TSM.Part;

                    //创建自定义的主梁对象;
                    CMrMainBeam mainBeam = new CMrMainBeam(mainPart, null);
                    CMrMainBeam.SetInstance(mainBeam);
                  
                    double dblFlangeThickness = 0.0;
                    double dblWebThickness = 0.0;
                  
                    //assembly.GetMainPart().GetReportProperty(CDogTools.GetInstance().GetFLANGEStr(), ref dblFlangeThickness);//YB 01 101 6 PROFILE.FLANGE_THICKNESS
                    //assembly.GetMainPart().GetReportProperty(CDogTools.GetInstance().GetWEBStr(), ref dblWebThickness);      //111 6  PROFILE.WEB_THICKNESS

                    assembly.GetMainPart().GetReportProperty("PROFILE.FLANGE_THICKNESS", ref dblFlangeThickness);
                    assembly.GetMainPart().GetReportProperty("PROFILE.WEB_THICKNESS", ref dblWebThickness);

                    mainBeam.mFlangeThickness = dblFlangeThickness;
                    mainBeam.mWebThickness = dblWebThickness;                  
                }
            }
        }

        /// <summary>
        /// 初始化视图;
        /// </summary>
        /// <param name="view"></param>
        private void InitView(TSD.View view)
        {
            Point maxPoint = view.RestrictionBox.MaxPoint;
            Point minPoint = view.RestrictionBox.MinPoint;

            CCommonPara.mViewMaxX = maxPoint.X;
            CCommonPara.mViewMinX = minPoint.X;
            CCommonPara.mViewMinY = minPoint.Y;
            CCommonPara.mViewMaxY = maxPoint.Y;
            CCommonPara.mViewScale = view.Attributes.Scale;
        }

        /// <summary>
        /// 创建柱的前视图标注;
        /// </summary>
        public void CreateCylinderFrontViewDim()
        {
            ViewBase frontViewBase = null;
            PointList pointList = new PointList();
            CDimTools.GetInstance().PickPoints(1, ref pointList, ref frontViewBase);

            if (frontViewBase != null)
            {
                frontViewBase.Select();
                TSD.View frontView = frontViewBase as TSD.View;
                CCylinderFrontView mFrontView = new CCylinderFrontView(frontView, mModel);

                InitMainPart();
                InitView(frontView);
                mFrontView.CreateDim();
            }
        }

        /// <summary>
        /// 柱的顶视图标注;
        /// </summary>
        public void CreateCylinderTopViewDim()
        {
            ViewBase topViewBase = null;
            PointList pointList = new PointList();
            CDimTools.GetInstance().PickPoints(1, ref pointList, ref topViewBase);

            if (topViewBase != null)
            {
                topViewBase.Select();

                TSD.View topView = topViewBase as TSD.View;
                CCylinderTopView mTopView = new CCylinderTopView(topView, mModel);

                InitMainPart();
                InitView(topView);
                mTopView.CreateDim();
            }
        }

        /// <summary>
        /// 柱的剖视图标注;
        /// </summary>
        public void CreateCylinderSectionView()
        {
            ViewBase sectionViewBase = null;
            PointList pointList = new PointList();
            CDimTools.GetInstance().PickPoints(1, ref pointList, ref sectionViewBase);

            if (sectionViewBase != null)
            {
                sectionViewBase.Select();
                TSD.View sectionView = sectionViewBase as TSD.View;
                CCylinderSectionView mSectionView = new CCylinderSectionView(sectionView, mModel);

                InitMainPart();
                InitView(sectionView);
                mSectionView.CreateDim();
            }
        }

        /// <summary>
        /// 框架结构的一键标注;
        /// </summary>
        /// <param name="nType"></param>
        public void DrawDrawingOneKey(string strDimType)
        {
            DrawingHandler drawingHandler = new DrawingHandler();
            Drawing drawing = drawingHandler.GetActiveDrawing();
            if (null == drawing)
            {
                return;
            }
            AssemblyDrawing assemblyDrawing = drawing as AssemblyDrawing;
            if (assemblyDrawing == null)
            {
                return;
            }
//             try
//             {
                Assembly assembly = new Assembly
                {
                    Identifier = assemblyDrawing.AssemblyIdentifier
                };

                assembly.Select();
                Identifier identifier = assembly.GetMainPart().Identifier;
                TSM.Part modelObject = mModel.SelectModelObject(identifier) as TSM.Part;

                //1.首先创建自动剖面;
                if (CCommonPara.mAutoSectionType == MrAutoSectionType.MrOneKeyDim || CCommonPara.mAutoSectionType == MrAutoSectionType.MrTwoTypeDim)
                {
                    CreateAutoSection(strDimType, modelObject, assemblyDrawing);
                }

                //2.对图纸进行标注;
                DrawingObjectEnumerator allViews = assemblyDrawing.GetSheet().GetAllViews();

                while (allViews.MoveNext())
                {
                    if (allViews.Current != null)
                    {
                        TSD.View view = allViews.Current as TSD.View;

                        if (view != null)
                        {
                            DrawDrawingByView(view, modelObject, strDimType);
                        }
                    }
                }
                assemblyDrawing.IsFrozen = true;
                assemblyDrawing.PlaceViews();
                assemblyDrawing.Modify();
//             }
//             catch (System.Exception ex)
//             {
//                 string strName = assemblyDrawing.Name;
//                 MessageBox.Show("图纸" + strName + "标注失败,请确认模型或图纸内是否有问题。异常信息：" + ex.Message);
//             }

            MessageBox.Show("扬州Tekla培训石头哥提示您标注结束,感谢您对智能标注系统的支持。");
        }

        /// <summary>
        /// 自动创建框架结构的剖面;
        /// </summary>
        /// <param name="strType">当前的标注类型</param>
        private void CreateAutoSection(string strType, TSM.Part mainPart, AssemblyDrawing assemblyDrawing)
        {
            if (strType.Equals("梁标注"))
            {
                CBeamAutoSection beamAutoSection = new CBeamAutoSection(mainPart, assemblyDrawing);

                beamAutoSection.CreateSection();
            }
            else if (strType.Equals("柱标注"))
            {
                CCylinderAutoSection cylinderAutoSection = new CCylinderAutoSection(mainPart, assemblyDrawing);

                cylinderAutoSection.CreateSection();
            }
        }

        /// <summary>
        /// 创建梁的门式框架结构的前视图;
        /// </summary>
        public void CreateBeamDoorFrontView()
        {
            ViewBase frontViewBase = null;
            PointList pointList = new PointList();
            CDimTools.GetInstance().PickPoints(1, ref pointList, ref frontViewBase);

            if (frontViewBase != null)
            {
                frontViewBase.Select();
                TSD.View frontView = frontViewBase as TSD.View;
                CBeamDoorFrontView mFrontView = new CBeamDoorFrontView(frontView, mModel);

                InitMainPart();
                InitView(frontView);
                mFrontView.CreateDim();
            }
        }

        /// <summary>
        /// 创建梁的门式框架结构的剖视图;
        /// </summary>
        public void CreateBeamDoorSectionView()
        {
            ViewBase sectionViewBase = null;
            PointList pointList = new PointList();
            CDimTools.GetInstance().PickPoints(1, ref pointList, ref sectionViewBase);

            if (sectionViewBase != null)
            {
                sectionViewBase.Select();
                TSD.View sectionView = sectionViewBase as TSD.View;
                CBeamDoorSectionView mSectionView = new CBeamDoorSectionView(sectionView, mModel);

                InitMainPart();
                InitView(sectionView);
                mSectionView.CreateDim();
            }
        }

        /// <summary>
        /// 创建柱的门式框架结构前视图;
        /// </summary>
        public void CreateCylinderDoorFrontView()
        {
            ViewBase frontViewBase = null;
            PointList pointList = new PointList();
            CDimTools.GetInstance().PickPoints(1, ref pointList, ref frontViewBase);

            if (frontViewBase != null)
            {
                frontViewBase.Select();
                TSD.View frontView = frontViewBase as TSD.View;
                CCylinderDoorFrontView mFrontView = new CCylinderDoorFrontView(frontView, mModel);

                InitMainPart();
                InitView(frontView);
                mFrontView.CreateDim();
            }
        }

        /// <summary>
        /// 创建柱的门式框架结构顶视图;
        /// </summary>
        public void CreateCylinderDoorTopView()
        {
            ViewBase topViewBase = null;
            PointList pointList = new PointList();
            CDimTools.GetInstance().PickPoints(1, ref pointList, ref topViewBase);

            if (topViewBase != null)
            {
                topViewBase.Select();
                TSD.View topView = topViewBase as TSD.View;
                CCylinderDoorTopView mTopView = new CCylinderDoorTopView(topView, mModel);

                InitMainPart();
                InitView(topView);
                mTopView.CreateDim();
            }
        }

        /// <summary>
        /// 创建柱的门式框架结构剖视图;
        /// </summary>
        public void CreateCylinderDoorSectionView()
        {
            ViewBase sectionViewBase = null;
            PointList pointList = new PointList();
            CDimTools.GetInstance().PickPoints(1, ref pointList, ref sectionViewBase);

            if (sectionViewBase != null)
            {
                sectionViewBase.Select();
                TSD.View sectionView = sectionViewBase as TSD.View;
                CCylinderDoorSectionView mSectionView = new CCylinderDoorSectionView(sectionView, mModel);

                InitMainPart();
                InitView(sectionView);
                mSectionView.CreateDim();
            }
        }

        /// <summary>
        /// 绘制门式框架结构中选择的多张图纸;
        /// </summary>
        public void DrawSelectListDrawingDoor()
        {
            DrawingHandler drawingHandler = new DrawingHandler();

            foreach (CMrAssemblyDrawing mrDrawing in mrAssemblyDrawingList)
            {
                AssemblyDrawing assemblyDrawing = mrDrawing.mAssemblyDring;

                Identifier assemblyDrawingIdentifier = assemblyDrawing.AssemblyIdentifier;

                if (assemblyDrawing == null)
                {
                    continue;
                }
                try
                {
                    Assembly assembly = new Assembly
                    {
                        Identifier = assemblyDrawing.AssemblyIdentifier
                    };

                    assembly.Select();
                    Identifier identifier = assembly.GetMainPart().Identifier;
                    Beam modelObject = mModel.SelectModelObject(identifier) as Beam;
                    drawingHandler.SetActiveDrawing(assemblyDrawing, true);
                    DrawingObjectEnumerator allViews = assemblyDrawing.GetSheet().GetAllViews();

                    while (allViews.MoveNext())
                    {
                        if (allViews.Current != null)
                        {
                            TSD.View view = allViews.Current as TSD.View;

                            if (view != null)
                            {
                                DrawDrawingDoorByView(view, modelObject, mrDrawing.mDimType);
                            }
                        }
                    }
                    assemblyDrawing.IsFrozen = true;
                    assemblyDrawing.PlaceViews();
                    assemblyDrawing.Modify();
                    drawingHandler.SaveActiveDrawing();
                    drawingHandler.CloseActiveDrawing();
                }
                catch (System.Exception ex)
                {
                    string strName = assemblyDrawing.Name;

                    MessageBox.Show("图纸" + strName + "标注失败,请确认模型或图纸内是否有问题。异常信息：" + ex.Message);
                    drawingHandler.SaveActiveDrawing();
                    drawingHandler.CloseActiveDrawing();
                    continue;
                }
            }
            MessageBox.Show("扬州Tekla培训石头哥提示您标注结束,感谢您对智能标注系统的支持。");
        }


        /// <summary>
        /// 根据视图绘制门式框架结构的图纸;
        /// </summary>
        /// <param name="view"></param>
        /// <param name="mainPart"></param>
        /// <param name="strDimType"></param>
        private void DrawDrawingDoorByView(TSD.View view, TSM.Part mainPart, string strDimType)
        {
            view.Select();

            //创建自定义的主梁对象;
            CMrMainBeam mainBeam = new CMrMainBeam(mainPart, null);
            CMrMainBeam.SetInstance(mainBeam);

            double dblFlangeThickness = 0.0;
            double dblWebThickness = 0.0;

            mainPart.GetReportProperty("PROFILE.FLANGE_THICKNESS", ref dblFlangeThickness);
            mainPart.GetReportProperty("PROFILE.WEB_THICKNESS", ref dblWebThickness);

            mainBeam.mFlangeThickness = dblFlangeThickness;
            mainBeam.mWebThickness = dblWebThickness;

            int size = view.GetObjects(new Type[] { typeof(TSD.Part) }).GetSize();

            //如果零部件个数为0或1分别表示空视图和零件图则不进行标注;
            if (size == 0 || size == 1)
            {
                return;
            }

            Vector xVector = new Vector(1, 0, 0);
            Vector yVector = new Vector(0, 1, 0);
            Vector zVector = new Vector(0, 0, 1);

            CDimTools.GetInstance().InitMrPart(mainPart, view, mainBeam);
            Vector vector = mainBeam.mNormal;

            double minY = mainBeam.GetMinYPoint().Y;
            double maxY = mainBeam.GetMaxYPoint().Y;
            double minX = mainBeam.GetMinXPoint().X;
            double maxX = mainBeam.GetMaxXPoint().X;

            double mainBeamHeight = Math.Abs(maxY - minY);
            double mainBeamWidth = Math.Abs(maxX - minX);

            if (strDimType == "梁标注")
            {
                Point viewMinPoint = view.RestrictionBox.MinPoint;
                Point viewMaxPoint = view.RestrictionBox.MaxPoint;
                double viewWidth = Math.Abs(viewMaxPoint.X - viewMinPoint.X);

                //顶视图不需要进行标注;
                if (CDimTools.GetInstance().IsTwoVectorParallel(vector, yVector)
                    && CDimTools.GetInstance().CompareTwoDoubleValue(viewWidth, mainBeamWidth) >= 0)
                {

                }
                //主视图标注;
                else if (CDimTools.GetInstance().IsTwoVectorParallel(vector, zVector)
                    && CDimTools.GetInstance().CompareTwoDoubleValue(viewWidth, mainBeamWidth) >= 0 )
                {
                    CBeamDoorFrontView mFrontView = new CBeamDoorFrontView(view, mModel);
                    InitView(view);
                    mFrontView.CreateDim();
                }
                //剖视图标注;
                else
                {
                    CBeamDoorSectionView mSectionView = new CBeamDoorSectionView(view, mModel);
                    InitView(view);
                    mSectionView.CreateDim();
                }
            }
            else if (strDimType == "柱标注")
            {
                Point viewMinPoint = view.RestrictionBox.MinPoint;
                Point viewMaxPoint = view.RestrictionBox.MaxPoint;
                double viewHeight = Math.Abs(viewMaxPoint.Y - viewMinPoint.Y);

                //顶视图标注;
                if (CDimTools.GetInstance().IsTwoVectorParallel(vector, xVector)
                    && CDimTools.GetInstance().CompareTwoDoubleValue(viewHeight, mainBeamHeight) >= 0)
                {
                    CCylinderDoorTopView mTopView = new CCylinderDoorTopView(view, mModel);
                    InitView(view);
                    mTopView.CreateDim();
                }
                //主视图标注;
                else if (CDimTools.GetInstance().IsTwoVectorParallel(vector, zVector)
                     && CDimTools.GetInstance().CompareTwoDoubleValue(viewHeight, mainBeamHeight) >= 0)
                {
                    CCylinderDoorFrontView mFrontView = new CCylinderDoorFrontView(view, mModel);
                    InitView(view);
                    mFrontView.CreateDim();
                }
                //剖视图标注;
                else
                {
                    CCylinderDoorSectionView mSectionView = new CCylinderDoorSectionView(view, mModel);
                    InitView(view);
                    mSectionView.CreateDim();
                }
            }
        }

        /// <summary>
        /// 门式框架结构的一键标注;
        /// </summary>
        /// <param name="nType"></param>
        public void DrawDrawingDoorOneKey(string strDimType)
        {
            DrawingHandler drawingHandler = new DrawingHandler();
            Drawing drawing = drawingHandler.GetActiveDrawing();
            if (null == drawing)
            {
                return;
            }
            AssemblyDrawing assemblyDrawing = drawing as AssemblyDrawing;
            if (assemblyDrawing == null)
            {
                return;
            }
            try
            {
                Assembly assembly = new Assembly
                {
                    Identifier = assemblyDrawing.AssemblyIdentifier
                };

                assembly.Select();
                Identifier identifier = assembly.GetMainPart().Identifier;
                TSM.Part modelObject = mModel.SelectModelObject(identifier) as TSM.Part;

                if (modelObject == null)
                {
                    MessageBox.Show("标注失败,无法获取主梁对象！");
                    return;
                }

                //1.首先创建门刚的自动剖面;
                //CreateAutoSectionDoor(strDimType, modelObject, assemblyDrawing);

                DrawingObjectEnumerator allViews = assemblyDrawing.GetSheet().GetAllViews();
               
                while (allViews.MoveNext())
                {
                    if (allViews.Current != null)
                    {
                        TSD.View view = allViews.Current as TSD.View;

                        if (view != null)
                        {
                            DrawDrawingDoorByView(view, modelObject, strDimType);
                        }
                    }
                }
                assemblyDrawing.IsFrozen = true;
                assemblyDrawing.PlaceViews();
                assemblyDrawing.Modify();
            }
            catch (System.Exception ex)
            {
                string strName = assemblyDrawing.Name;

                MessageBox.Show("图纸" + strName + "标注失败,请确认模型或图纸内是否有问题。异常信息：" + ex.Message);
            }

            MessageBox.Show("扬州Tekla培训石头哥提示您标注结束,感谢您对智能标注系统的支持。");
        }

        /// <summary>
        /// 自动创建门式框架结构的剖面;
        /// </summary>
        /// <param name="strType"></param>
        private void CreateAutoSectionDoor(string strType, TSM.Part mainPart, AssemblyDrawing assemblyDrawing)
        {
            if (strType.Equals("梁标注"))
            {
                CBeamAutoSectionDoor beamAutoSection = new CBeamAutoSectionDoor(mainPart, assemblyDrawing);
                beamAutoSection.CreateSection();
            }
            else if (strType.Equals("柱标注"))
            {
                CCylinderAutoSectionDoor cylinderAutoSection = new CCylinderAutoSectionDoor(mainPart, assemblyDrawing);
                cylinderAutoSection.CreateSection();
            }
        }

        /// <summary>
        /// 框架结构的一键标注;
        /// </summary>
        /// <param name="nType"></param>
        public void CreateAutoSectionTest(string strDimType,int nFlag)
        {
            DrawingHandler drawingHandler = new DrawingHandler();
            Drawing drawing = drawingHandler.GetActiveDrawing();
            if (null == drawing)
            {
                return;
            }
            AssemblyDrawing assemblyDrawing = drawing as AssemblyDrawing;
            if (assemblyDrawing == null)
            {
                return;
            }
            try
            {
                Assembly assembly = new Assembly
                {
                    Identifier = assemblyDrawing.AssemblyIdentifier
                };
                assembly.Select();

                if (nFlag == 0)
                {
                    Identifier identifier = assembly.GetMainPart().Identifier;
                    TSM.Part modelObject = mModel.SelectModelObject(identifier) as TSM.Part;
                    CreateAutoSection(strDimType, modelObject, assemblyDrawing);
                }
                else if (nFlag == 1)
                {
                    Identifier identifier = assembly.GetMainPart().Identifier;
                    TSM.Part modelObject = mModel.SelectModelObject(identifier) as TSM.Part;
                    CreateAutoSectionDoor(strDimType, modelObject, assemblyDrawing);
                }
            }
            catch(Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
    }
}
