using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace AutoDimension
{
    /// <summary>
    /// 柱标注顶视图配置;
    /// </summary>
    public class CCylinderTopViewSetting
    {
        public const string mstrAixLine = "轴线";
        public const string mstrWorkPointToMainPart = "工作点与主零件距离";
        public const string mstrSupportPlate = "支撑及牛腿";
        public const string mstrConnentPlateOnSupport = "牛腿上连接板";
        public const string mstrVerticalConnectPlate = "垂直连接及加筋板";
        public const string mstrHorizontalConnentPlate = "水平连接板(无孔)";
        public const string mstrMainPartLength = "主部件长度";
        public const string mstrAngleSheet = "角钢";
        public const string mstrBolt = "主部件螺栓";
        public const string mstrWorkPointToWorkPoint = "工作点到工作点";
        public const string mstrCutting = "切割";

        /// <summary>
        /// 标注对象字符串与标注设置的映射表;
        /// </summary>
        public Dictionary<string, CDimAndMarkFlag> mDimObjectDictionary = new Dictionary<string, CDimAndMarkFlag>();

        /// <summary>
        /// 中文名字和英文名字的映射表;
        /// </summary>
        public Dictionary<string, string> mChsNameToEngNameDictionary = new Dictionary<string, string>();

        /// <summary>
        /// 英文名字和中文名字的映射表;
        /// </summary>
        public Dictionary<string, string> mEngNameToChsNameDictionary = new Dictionary<string, string>();

        /// <summary>
        /// 梁顶视图采用总尺寸标注;
        /// </summary>
        public bool mDimOverallSize = true;

        /// <summary>
        /// 梁前视图采用分尺寸标注;
        /// </summary>
        public bool mDimSize = false;

        /// <summary>
        /// 根据标注类型名字查找当前设置的值;
        /// </summary>
        /// <param name="strName"></param>
        /// <returns></returns>
        public bool FindDimValueByName(string strName)
        {
            if (mDimObjectDictionary.ContainsKey(strName))
            {
                CDimAndMarkFlag dimMarkFlag = mDimObjectDictionary[strName];

                return dimMarkFlag.mbDim;
            }
            return false;
        }

        /// <summary>
        /// 根据名字设置值;
        /// </summary>
        /// <param name="strName"></param>
        /// <param name="bValue"></param>
        /// <returns></returns>
        public bool SetDimValueByName(string strName, bool bValue)
        {
            if (mDimObjectDictionary.ContainsKey(strName))
            {
                CDimAndMarkFlag dimMarkFlag = mDimObjectDictionary[strName];

                dimMarkFlag.mbDim = bValue;
            }
            return true;
        }

        /// <summary>
        /// 根据标注类型名字查找当前设置的值;
        /// </summary>
        /// <param name="strName"></param>
        /// <returns></returns>
        public bool FindMarkValueByName(string strName)
        {
            if (mDimObjectDictionary.ContainsKey(strName))
            {
                CDimAndMarkFlag dimMarkFlag = mDimObjectDictionary[strName];

                return dimMarkFlag.mbMark;
            }
            return false;
        }

        /// <summary>
        /// 根据名字设置值;
        /// </summary>
        /// <param name="strName"></param>
        /// <param name="bValue"></param>
        /// <returns></returns>
        public bool SetMarkValueByName(string strName, bool bValue)
        {
            if (mDimObjectDictionary.ContainsKey(strName))
            {
                CDimAndMarkFlag dimMarkFlag = mDimObjectDictionary[strName];

                dimMarkFlag.mbMark = bValue;
            }
            return true;
        }

        /// <summary>
        /// 保存配置文件到XmlNode中;
        /// </summary>
        /// <param name="xmlNode"></param>
        public void SaveSettingToXml(XmlDocument xmlDoc, XmlNode xmlNode)
        {
            foreach (KeyValuePair<string, CDimAndMarkFlag> kv in mDimObjectDictionary)
            {
                string strName = kv.Key;
                CDimAndMarkFlag value = kv.Value;

                bool bDimFlag = value.mbDim;
                bool bMarkFlag = value.mbMark;

                XmlElement xmlElement = xmlDoc.CreateElement(mChsNameToEngNameDictionary[strName]);

                if (bDimFlag)
                {
                    xmlElement.SetAttribute("Dim", "1");
                }
                else
                {
                    xmlElement.SetAttribute("Dim", "0");
                }
                if (bMarkFlag)
                {
                    xmlElement.SetAttribute("Mark", "1");
                }
                else
                {
                    xmlElement.SetAttribute("Mark", "0");
                }
                xmlNode.AppendChild(xmlElement);
            }
        }


        /// <summary>
        /// 从Xml文件中读取配置信息;
        /// </summary>
        /// <param name="xmlDoc"></param>
        /// <param name="xmlNode"></param>
        public void ReadSettingFromXml(XmlDocument xmlDoc, XmlNode xmlNode)
        {
            XmlNodeList list = xmlNode.ChildNodes;

            foreach (XmlNode childNode in list)
            {
                XmlElement xmlElement = childNode as XmlElement;
                string strName = xmlElement.LocalName;
                string strDim = xmlElement.GetAttribute("Dim");
                string strMark = xmlElement.GetAttribute("Mark");

                string strChsName = mEngNameToChsNameDictionary[strName];
                CDimAndMarkFlag dimAndMarkFlag = mDimObjectDictionary[strChsName];

                if (strDim.Equals("1"))
                {
                    dimAndMarkFlag.mbDim = true;
                }
                else
                {
                    dimAndMarkFlag.mbDim = false;
                }
                if (strMark.Equals("1"))
                {
                    dimAndMarkFlag.mbMark = true;
                }
                else
                {
                    dimAndMarkFlag.mbMark = false;
                }
            }
        }

        /// <summary>
        /// 顶视图标注设置;
        /// </summary>
        public CCylinderTopViewSetting()
        {
            //标注设置;
            CDimAndMarkFlag markDimFlag = new CDimAndMarkFlag();
            mDimObjectDictionary.Add(mstrAixLine, markDimFlag);
            mChsNameToEngNameDictionary.Add(mstrAixLine, "AixLine");
            mEngNameToChsNameDictionary.Add("AixLine", mstrAixLine);

            markDimFlag = new CDimAndMarkFlag();
            mDimObjectDictionary.Add(mstrWorkPointToMainPart, markDimFlag);
            mChsNameToEngNameDictionary.Add(mstrWorkPointToMainPart, "WorkPointToMainPart");
            mEngNameToChsNameDictionary.Add("WorkPointToMainPart", mstrWorkPointToMainPart);

            markDimFlag = new CDimAndMarkFlag();
            mDimObjectDictionary.Add(mstrSupportPlate, markDimFlag);
            mChsNameToEngNameDictionary.Add(mstrSupportPlate, "SupportPlate");
            mEngNameToChsNameDictionary.Add("SupportPlate", mstrSupportPlate);

            markDimFlag=new CDimAndMarkFlag();
            mDimObjectDictionary.Add(mstrConnentPlateOnSupport,markDimFlag);
            mChsNameToEngNameDictionary.Add(mstrConnentPlateOnSupport, "ConnentPlateOnSupport");
            mEngNameToChsNameDictionary.Add("ConnentPlateOnSupport", mstrConnentPlateOnSupport);

            markDimFlag = new CDimAndMarkFlag();
            mDimObjectDictionary.Add(mstrVerticalConnectPlate, markDimFlag);
            mChsNameToEngNameDictionary.Add(mstrVerticalConnectPlate, "VerticalConnectPlate");
            mEngNameToChsNameDictionary.Add("VerticalConnectPlate", mstrVerticalConnectPlate);

            markDimFlag = new CDimAndMarkFlag();
            mDimObjectDictionary.Add(mstrHorizontalConnentPlate, markDimFlag);
            mChsNameToEngNameDictionary.Add(mstrHorizontalConnentPlate, "HorizontalConnentPlate");
            mEngNameToChsNameDictionary.Add("HorizontalConnentPlate", mstrHorizontalConnentPlate);

            markDimFlag = new CDimAndMarkFlag();
            mDimObjectDictionary.Add(mstrMainPartLength, markDimFlag);
            mChsNameToEngNameDictionary.Add(mstrMainPartLength, "MainPartLength");
            mEngNameToChsNameDictionary.Add("MainPartLength", mstrMainPartLength);

            markDimFlag = new CDimAndMarkFlag();
            mDimObjectDictionary.Add(mstrAngleSheet, markDimFlag);
            mChsNameToEngNameDictionary.Add(mstrAngleSheet, "AngleSheet");
            mEngNameToChsNameDictionary.Add("AngleSheet", mstrAngleSheet);

            markDimFlag = new CDimAndMarkFlag();
            mDimObjectDictionary.Add(mstrBolt, markDimFlag);
            mChsNameToEngNameDictionary.Add(mstrBolt, "Bolt");
            mEngNameToChsNameDictionary.Add("Bolt", mstrBolt);

            markDimFlag = new CDimAndMarkFlag();
            mDimObjectDictionary.Add(mstrWorkPointToWorkPoint, markDimFlag);
            mChsNameToEngNameDictionary.Add(mstrWorkPointToWorkPoint, "WorkPointToWorkPoint");
            mEngNameToChsNameDictionary.Add("WorkPointToWorkPoint", mstrWorkPointToWorkPoint);

            markDimFlag = new CDimAndMarkFlag();
            mDimObjectDictionary.Add(mstrCutting, markDimFlag);
            mChsNameToEngNameDictionary.Add(mstrCutting, "Cutting");
            mEngNameToChsNameDictionary.Add("Cutting", mstrCutting);
        }
    }

    /// <summary>
    /// 柱标注前视图配置;
    /// </summary>
    public class CCylinderFrontViewSetting
    {
        public const string mstrAixLine = "轴线";
        public const string mstrWorkPointToMainPart = "工作点与主零件距离";
        public const string mstrSupportPlate = "斜支撑及牛腿";
        public const string mstrConnentPlateOnSupport = "牛腿上连接板";
        public const string mstrVerticalConnectPlate = "垂直连接及加筋板";
        public const string mstrOutConnectPlate = "外围连接板";
        public const string mstrHorizontalConnentPlate = "水平连接板(无孔)";
        public const string mstrMainPartLength = "主部件长度";
        public const string mstrAngleSheet = "角钢";
        public const string mstrMainBeamMiddlePart = "腹板上竖直部件";
        public const string mstrBolt = "主部件螺栓";
        public const string mstrWorkPointToWorkPoint = "工作点到工作点";
        public const string mstrCutting = "切割";
        public const string mstrBoltClosed = "螺栓尺寸封闭";

        /// <summary>
        /// 梁顶视图采用总尺寸标注;
        /// </summary>
        public bool mDimOverallSize = true;

        /// <summary>
        /// 梁前视图采用分尺寸标注;
        /// </summary>
        public bool mDimSize = false;

        /// <summary>
        /// 标注对象字符串与标注设置的映射表;
        /// </summary>
        public Dictionary<string, CDimAndMarkFlag> mDimObjectDictionary = new Dictionary<string, CDimAndMarkFlag>();

        /// <summary>
        /// 中文名字和英文名字的映射表;
        /// </summary>
        public Dictionary<string, string> mChsNameToEngNameDictionary = new Dictionary<string, string>();

        /// <summary>
        /// 英文名字和中文名字的映射表;
        /// </summary>
        public Dictionary<string, string> mEngNameToChsNameDictionary = new Dictionary<string, string>();

        /// <summary>
        /// 根据标注类型名字查找当前设置的值;
        /// </summary>
        /// <param name="strName"></param>
        /// <returns></returns>
        public bool FindDimValueByName(string strName)
        {
            if (mDimObjectDictionary.ContainsKey(strName))
            {
                CDimAndMarkFlag dimMarkFlag = mDimObjectDictionary[strName];

                return dimMarkFlag.mbDim;
            }
            return false;
        }

        /// <summary>
        /// 根据名字设置值;
        /// </summary>
        /// <param name="strName"></param>
        /// <param name="bValue"></param>
        /// <returns></returns>
        public bool SetDimValueByName(string strName, bool bValue)
        {
            if (mDimObjectDictionary.ContainsKey(strName))
            {
                CDimAndMarkFlag dimMarkFlag = mDimObjectDictionary[strName];

                dimMarkFlag.mbDim = bValue;
            }
            return true;
        }

        /// <summary>
        /// 根据标注类型名字查找当前设置的值;
        /// </summary>
        /// <param name="strName"></param>
        /// <returns></returns>
        public bool FindMarkValueByName(string strName)
        {
            if (mDimObjectDictionary.ContainsKey(strName))
            {
                CDimAndMarkFlag dimMarkFlag = mDimObjectDictionary[strName];

                return dimMarkFlag.mbMark;
            }
            return false;
        }

        /// <summary>
        /// 根据名字设置值;
        /// </summary>
        /// <param name="strName"></param>
        /// <param name="bValue"></param>
        /// <returns></returns>
        public bool SetMarkValueByName(string strName, bool bValue)
        {
            if (mDimObjectDictionary.ContainsKey(strName))
            {
                CDimAndMarkFlag dimMarkFlag = mDimObjectDictionary[strName];

                dimMarkFlag.mbMark = bValue;
            }
            return true;
        }

        /// <summary>
        /// 保存配置文件到XmlNode中;
        /// </summary>
        /// <param name="xmlNode"></param>
        public void SaveSettingToXml(XmlDocument xmlDoc, XmlNode xmlNode)
        {
            foreach (KeyValuePair<string, CDimAndMarkFlag> kv in mDimObjectDictionary)
            {
                string strName = kv.Key;
                CDimAndMarkFlag value = kv.Value;

                bool bDimFlag = value.mbDim;
                bool bMarkFlag = value.mbMark;

                XmlElement xmlElement = xmlDoc.CreateElement(mChsNameToEngNameDictionary[strName]);

                if (bDimFlag)
                {
                    xmlElement.SetAttribute("Dim", "1");
                }
                else
                {
                    xmlElement.SetAttribute("Dim", "0");
                }
                if (bMarkFlag)
                {
                    xmlElement.SetAttribute("Mark", "1");
                }
                else
                {
                    xmlElement.SetAttribute("Mark", "0");
                }
                xmlNode.AppendChild(xmlElement);
            }
        }

        /// <summary>
        /// 从Xml文件中读取配置信息;
        /// </summary>
        /// <param name="xmlDoc"></param>
        /// <param name="xmlNode"></param>
        public void ReadSettingFromXml(XmlDocument xmlDoc, XmlNode xmlNode)
        {
            XmlNodeList list = xmlNode.ChildNodes;

            foreach (XmlNode childNode in list)
            {
                XmlElement xmlElement = childNode as XmlElement;
                string strName = xmlElement.LocalName;
                string strDim = xmlElement.GetAttribute("Dim");
                string strMark = xmlElement.GetAttribute("Mark");

                string strChsName = mEngNameToChsNameDictionary[strName];
                CDimAndMarkFlag dimAndMarkFlag = mDimObjectDictionary[strChsName];

                if (strDim.Equals("1"))
                {
                    dimAndMarkFlag.mbDim = true;
                }
                else
                {
                    dimAndMarkFlag.mbDim = false;
                }
                if (strMark.Equals("1"))
                {
                    dimAndMarkFlag.mbMark = true;
                }
                else
                {
                    dimAndMarkFlag.mbMark = false;
                }
            }
        }

        /// <summary>
        /// 前视图标注设置;
        /// </summary>
        public CCylinderFrontViewSetting()
        {
            CDimAndMarkFlag dimMarkFlag = new CDimAndMarkFlag();
            mDimObjectDictionary.Add(mstrAixLine, dimMarkFlag);
            mChsNameToEngNameDictionary.Add(mstrAixLine, "AixLine");
            mEngNameToChsNameDictionary.Add("AixLine", mstrAixLine);

            dimMarkFlag = new CDimAndMarkFlag();
            mDimObjectDictionary.Add(mstrWorkPointToMainPart, dimMarkFlag);
            mChsNameToEngNameDictionary.Add(mstrWorkPointToMainPart, "WorkPointToMainPart");
            mEngNameToChsNameDictionary.Add("WorkPointToMainPart", mstrWorkPointToMainPart);

            dimMarkFlag = new CDimAndMarkFlag();
            mDimObjectDictionary.Add(mstrSupportPlate, dimMarkFlag);
            mChsNameToEngNameDictionary.Add(mstrSupportPlate, "SupportPlate");
            mEngNameToChsNameDictionary.Add("SupportPlate", mstrSupportPlate);

            dimMarkFlag = new CDimAndMarkFlag();
            mDimObjectDictionary.Add(mstrConnentPlateOnSupport, dimMarkFlag);
            mChsNameToEngNameDictionary.Add(mstrConnentPlateOnSupport, "ConnentPlateOnSupport");
            mEngNameToChsNameDictionary.Add("ConnentPlateOnSupport", mstrConnentPlateOnSupport);

            dimMarkFlag = new CDimAndMarkFlag();
            mDimObjectDictionary.Add(mstrVerticalConnectPlate, dimMarkFlag);
            mChsNameToEngNameDictionary.Add(mstrVerticalConnectPlate, "VerticalConnectPlate");
            mEngNameToChsNameDictionary.Add("VerticalConnectPlate", mstrVerticalConnectPlate);

            dimMarkFlag = new CDimAndMarkFlag();
            mDimObjectDictionary.Add(mstrOutConnectPlate, dimMarkFlag);
            mChsNameToEngNameDictionary.Add(mstrOutConnectPlate, "OutConnectPlate");
            mEngNameToChsNameDictionary.Add("OutConnectPlate", mstrOutConnectPlate);

            dimMarkFlag = new CDimAndMarkFlag();
            mDimObjectDictionary.Add(mstrHorizontalConnentPlate, dimMarkFlag);
            mChsNameToEngNameDictionary.Add(mstrHorizontalConnentPlate, "HorizontalConnentPlate");
            mEngNameToChsNameDictionary.Add("HorizontalConnentPlate", mstrHorizontalConnentPlate);

            dimMarkFlag = new CDimAndMarkFlag();
            mDimObjectDictionary.Add(mstrMainPartLength, dimMarkFlag);
            mChsNameToEngNameDictionary.Add(mstrMainPartLength, "MainPartLength");
            mEngNameToChsNameDictionary.Add("MainPartLength", mstrMainPartLength);

            dimMarkFlag = new CDimAndMarkFlag();
            mDimObjectDictionary.Add(mstrAngleSheet, dimMarkFlag);
            mChsNameToEngNameDictionary.Add(mstrAngleSheet, "AngleSheet");
            mEngNameToChsNameDictionary.Add("AngleSheet", mstrAngleSheet);

            dimMarkFlag = new CDimAndMarkFlag();
            mDimObjectDictionary.Add(mstrMainBeamMiddlePart, dimMarkFlag);
            mChsNameToEngNameDictionary.Add(mstrMainBeamMiddlePart, "MainBeamMiddlePart");
            mEngNameToChsNameDictionary.Add("MainBeamMiddlePart", mstrMainBeamMiddlePart);

            dimMarkFlag = new CDimAndMarkFlag();
            mDimObjectDictionary.Add(mstrBolt, dimMarkFlag);
            mChsNameToEngNameDictionary.Add(mstrBolt, "Bolt");
            mEngNameToChsNameDictionary.Add("Bolt", mstrBolt);

            dimMarkFlag = new CDimAndMarkFlag();
            mDimObjectDictionary.Add(mstrWorkPointToWorkPoint, dimMarkFlag);
            mChsNameToEngNameDictionary.Add(mstrWorkPointToWorkPoint, "WorkPointToWorkPoint");
            mEngNameToChsNameDictionary.Add("WorkPointToWorkPoint", mstrWorkPointToWorkPoint);

            dimMarkFlag = new CDimAndMarkFlag();
            mDimObjectDictionary.Add(mstrCutting, dimMarkFlag);
            mChsNameToEngNameDictionary.Add(mstrCutting, "Cutting");
            mEngNameToChsNameDictionary.Add("Cutting", mstrCutting);

            dimMarkFlag = new CDimAndMarkFlag();
            mDimObjectDictionary.Add(mstrBoltClosed, dimMarkFlag);
            mChsNameToEngNameDictionary.Add(mstrBoltClosed, "BoltClosed");
            mEngNameToChsNameDictionary.Add("BoltClosed", mstrBoltClosed);
        }
    }

    /// <summary>
    /// 梁标注剖视图设置;
    /// </summary>
    public class CCylinderSectionViewSetting
    {
        /// <summary>
        /// 标注对象字符串与标注设置的映射表;
        /// </summary>
        public Dictionary<string, CDimAndMarkFlag> mDimObjectDictionary = new Dictionary<string, CDimAndMarkFlag>();

        /// <summary>
        /// 根据标注类型名字查找当前设置的值;
        /// </summary>
        /// <param name="strName"></param>
        /// <returns></returns>
        public bool FindDimValueByName(string strName)
        {
            if (mDimObjectDictionary.ContainsKey(strName))
            {
                CDimAndMarkFlag dimMarkFlag = mDimObjectDictionary[strName];

                return dimMarkFlag.mbDim;
            }
            return false;
        }

        /// <summary>
        /// 根据名字设置值;
        /// </summary>
        /// <param name="strName"></param>
        /// <param name="bValue"></param>
        /// <returns></returns>
        public bool SetDimValueByName(string strName, bool bValue)
        {
            if (mDimObjectDictionary.ContainsKey(strName))
            {
                CDimAndMarkFlag dimMarkFlag = mDimObjectDictionary[strName];

                dimMarkFlag.mbDim = bValue;
            }
            return true;
        }

        /// <summary>
        /// 根据标注类型名字查找当前设置的值;
        /// </summary>
        /// <param name="strName"></param>
        /// <returns></returns>
        public bool FindMarkValueByName(string strName)
        {
            if (mDimObjectDictionary.ContainsKey(strName))
            {
                CDimAndMarkFlag dimMarkFlag = mDimObjectDictionary[strName];

                return dimMarkFlag.mbMark;
            }
            return false;
        }

        /// <summary>
        /// 根据名字设置值;
        /// </summary>
        /// <param name="strName"></param>
        /// <param name="bValue"></param>
        /// <returns></returns>
        public bool SetMarkValueByName(string strName, bool bValue)
        {
            if (mDimObjectDictionary.ContainsKey(strName))
            {
                CDimAndMarkFlag dimMarkFlag = mDimObjectDictionary[strName];

                dimMarkFlag.mbMark = bValue;
            }
            return true;
        }

        /// <summary>
        /// 剖视图标注设置;
        /// </summary>
        public CCylinderSectionViewSetting()
        {
            CDimAndMarkFlag dimMarkFlg = new CDimAndMarkFlag();
            mDimObjectDictionary.Add("水平连接板", dimMarkFlg);

            dimMarkFlg = new CDimAndMarkFlag();
            mDimObjectDictionary.Add("尺寸封闭", dimMarkFlg);
        }
    }


    /// <summary>
    /// 梁标注配置;
    /// </summary>
    public class CCylinderDimSetting
    {
        /// <summary>
        /// 单例;
        /// </summary>
        private static CCylinderDimSetting mInstance = null;

        /// <summary>
        /// 顶视图标注设置;
        /// </summary>
        public CCylinderTopViewSetting mTopViewSetting = null;

        /// <summary>
        /// 前视图标注设置;
        /// </summary>
        public CCylinderFrontViewSetting mFrontViewSetting = null;

        /// <summary>
        /// 剖视图标注设置;
        /// </summary>
        public CCylinderSectionViewSetting mSectionViewSetting = null;

        /// <summary>
        /// 私有构造函数;
        /// </summary>
        private CCylinderDimSetting()
        {
            mTopViewSetting = new CCylinderTopViewSetting();
            mFrontViewSetting = new CCylinderFrontViewSetting();
            mSectionViewSetting = new CCylinderSectionViewSetting();
        }

        /// <summary>
        /// 获取唯一的实例;
        /// </summary>
        /// <returns></returns>
        public static CCylinderDimSetting GetInstance()
        {
            if (null == mInstance)
            {
                mInstance = new CCylinderDimSetting();
            }

            return mInstance;
        }
    }
}
