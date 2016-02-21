using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AutoDimension.Entity
{
    /// <summary>
    /// 部件的位置类型;
    /// </summary>
    public enum MrPositionType
    {
        NONE=0,
        UP = 1,
        DOWM = 2,
        MIDDLE = 3,
        RIGHT=4,
        LEFT=5
    }

    /// <summary>
    /// 斜率类型;
    /// </summary>
    public enum MrSlopeType
    {
        MORETHAN_ZERO = 1,
        LESSTHAN_ZERO = 2,
        EQUAL_ZERO = 3,
        INFINITY = 4
    }

    /// <summary>
    /// 螺钉组在二维上的形状类型;
    /// </summary>
    public enum MrBoltArrayShapeType
    {
       ARRAY=1,         //竖直或水平形状;
       OBLIQUELINE = 2  //倾斜的;
    }

    /// <summary>
    /// 梁的类型;
    /// </summary>
    public enum MrBeamType
    {
        BEAM=1,
        CONTOURPLATE=2,
        POLYBEAM=3,
        MAINBEAM=4
    }

    /// <summary>
    /// 檩托板类型;
    /// </summary>
    public enum MrApronPlateType
    {
        Type1=0,    //类型1;    
        Type2=1     //类型2;
    }

    /// <summary>
    /// 剪切板的位置;
    /// </summary>
    public enum MrClipPlatePosType
    {
        NONE=0,
        TOP = 1,
        MIDDLE = 2,
        BOTTOM = 3
    }

    /// <summary>
    /// 剖面的方式;
    /// </summary>
    public enum MrSectionMode
    {
        None = 1,         //无效的剖面;
        MrHorizontal = 2, //水平的剖面;
        MrVertical = 3,   //竖直的剖面;
    }

    /// <summary>
    /// 剖面的类型;
    /// </summary>
    public enum MrSectionType
    {
        MrSectionBeam=1,
        MrSectionCylinder=2,
        MrSectionBeamDoor=3,
        MrSectionCylinderDoor=4,
    }

    /// <summary>
    /// 剖面的方向;
    /// </summary>
    public enum MrSectionOrientation
    {
        MrSectionLeft = 0,      //向左;
        MrSectionRight = 1,     //向右;
        MrSectionUp=2,          //向上;
        MrSectionDown=3,        //向下;
    }

    /// <summary>
    /// 什么情况下进行自动剖面设置;
    /// </summary>
    public enum MrAutoSectionType
    {
        MrNone=0,           //不自动剖面;
        MrOneKeyDim=1,      //一键标注时自动剖面;
        MrListDim=2,        //列表标注时剖面;
        MrTwoTypeDim=3,     //两者都进行剖面;
    }


    /// <summary>
    /// 视图类型;
    /// </summary>
    public enum MrViewType
    {
        NONE=0,
        BeamTopView=1,
        BeamFrontView=2,
        BeamSectionView=3,
        CylinderTopView=4,
        CylinderFrontView=5,
        CylinderSectionView=6
    }

    /// <summary>
    /// 模块类型;
    /// </summary>
    public enum MrModuleType
    {
        NONE = 0,
        DimTOP = 2,
        DimBefore=4,
        DimSection=8,
        GetList=16,
        SetDim=32,
        DimList=64        
    }
}
