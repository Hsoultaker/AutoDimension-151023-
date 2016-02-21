using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ServiceModel.Activation;
using System.Runtime.Serialization;
using System.Drawing;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Collections;

namespace WcfService
{   

    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    
    public class DimService : IDimService
    {
        static public Dictionary<string,ulong> UserIDlst =new Dictionary<string,ulong>();

        /// <summary>
        /// 比较点的X值的大小函数;
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        static public int ComparePointX(Point x, Point y)
        {
            if (x.X > y.X)
            {
                return 1;
            }
            else if (x.X < y.X)
            {
                return -1;
            }
            return 0;
        }
   
        public static double mDblError = 1e-2;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="UserID"></param>
        /// <returns></returns>
        #region ICalculate Members

        public void CloseService()
        {
           // string id = HttpContext.Current.Session.SessionID.ToString();
           ulong UserID = (ulong)HttpContext.Current.Session["__UserID"];

//            string key=null;
//            
//            foreach (KeyValuePair<string, ulong> kv in UserIDlst)
//            {
//                if (kv.Value == UserID)
//                {
//                    key = kv.Key;
//                }
//            }
//            if (key != null)
//            {
//                UserIDlst.Remove(key);
//                //HttpContext.Current.Session.Abandon();
//            }

            foreach (ulong id in UserIDlst.Values)
            {
                if (id == UserID)
                {
                    HttpContext.Current.Session.Abandon();
                }
            }
            
            HttpContext.Current.Session["__UserID"] = null;
            HttpContext.Current.Session["__UserAuthority"] = null;
        }
        public string GetUserAuthority(ulong UserID, string UserKey)
        {
           // string id = HttpContext.Current.Session.SessionID.ToString();
//             foreach (ulong val in UserIDlst.Values)
//             {
//                 if (val == UserID) return "-1";
//             }
           
            DataClassesDataContext dc = new DataClassesDataContext();          
           
            var userInfolst = from g in dc.UserInfo
                              where g.SerialNo_ == (int)UserID
                              select g;
            foreach (UserInfo userInfo in userInfolst)
            {
                HttpContext.Current.Session["__UserID"] = UserID;  
                HttpContext.Current.Session["__UserAuthority"] = userInfo.Purview;
                CEncryptionTools encryptionTools = new CEncryptionTools(UserID);
                UserKey = encryptionTools.Decode(UserKey);
                string[] vals = UserKey.Split(';');
                if (vals != null)
                {
                    if (vals.Length == 3)
                    {
                        UseRecord useRecord = new UseRecord();
                        useRecord.SerialNo_ = (int)UserID;
                        useRecord.Date = DateTime.Now;
                        useRecord.IP1 = vals[0];
                        useRecord.MAC = vals[1];
                        useRecord.IP2 = vals[2];
                        dc.UseRecord.InsertOnSubmit(useRecord);
                        dc.SubmitChanges();
                        if (!UserIDlst.ContainsKey(UserKey))
                            UserIDlst.Add(UserKey, UserID);
                        return encryptionTools.Encode(userInfo.Purview.ToString());
                    }
                } 
            }
            HttpContext.Current.Session["__UserID"] =null;
            return null;
           
        }
        public User GetUser(ulong UserID)
        {
            DataClassesDataContext dc = new DataClassesDataContext();
            var userInfolst = from g in dc.UserInfo
                              where g.SerialNo_ == (int)UserID
                              select g;
            foreach (UserInfo userInfo in userInfolst)
            {
                User user = new User();
                user.Company = userInfo.Company;
                user.UserName = userInfo.UserName;
                user.SerialNo = userInfo.SerialNo_;
                user.Purview = userInfo.Purview;
                HttpContext.Current.Session["__UserID"] = UserID;

                HttpContext.Current.Session["__UserAuthority"] = userInfo.Purview;
                return user;

            }
            HttpContext.Current.Session["__UserID"] = null;
            return null;
        }
        public string GetUserName()
        {
            if (HttpContext.Current.Session["__UserID"]!=null)
              return HttpContext.Current.Session["__UserID"].ToString();
            else
                return null;
        }
        public bool IsPartInViewBox(Point partMinPoint, Point partMaxPoint, Point viewMinPoint, Point viewMaxPoint)
        {
            if (HttpContext.Current.Session["__UserID"] == null) return false;
            bool bflag1 = false;
            bool bflag2 = false;
            if (partMinPoint.X >= viewMinPoint.X && partMinPoint.X <= viewMaxPoint.X
               && partMinPoint.Y >= viewMinPoint.Y && partMinPoint.Y <= viewMaxPoint.Y)
            {
                bflag1 = true;
            }
            if (partMaxPoint.X >= viewMinPoint.X && partMaxPoint.X <= viewMaxPoint.X
              && partMaxPoint.Y >= viewMinPoint.Y && partMaxPoint.Y <= viewMaxPoint.Y)
            {
                bflag2 = true;
            }

            if (bflag1 && bflag2)
            {
                return true;
            }

            return false;
        }
        public int JudgeLineSlope(Point pt1, Point pt2)
        {
            if (HttpContext.Current.Session["__UserID"] == null) return 0;
            if (Math.Abs(pt1.X - pt2.X) < mDblError)
            {
                //return MrSlopeType.INFINITY;
                return 4;
            }
            if (Math.Abs(pt1.Y - pt2.Y) < mDblError)
            {
               // return MrSlopeType.EQUAL_ZERO;
                return 3;
            }

            double k = 0.0;
            k = (pt2.Y - pt1.Y) / (pt2.X - pt1.X);

            if (k > mDblError)
            {
                //return MrSlopeType.MORETHAN_ZERO;
                return 1;
            }
            else
            {
               // return MrSlopeType.LESSTHAN_ZERO;
                return 1;
            }
        }
        public bool IsTwoPointEqual(Point point1, Point point2)
        {
            if (HttpContext.Current.Session["__UserID"] == null) return false;

            if (Math.Abs(point1.X - point2.X) <mDblError &&
                Math.Abs(point1.Y - point2.Y) < mDblError &&
                Math.Abs(point1.Z - point2.Z) < mDblError)
            {
                return true;
            }
            return false;
        }
        public bool IsLineIntersectWithLineSegment(Point pt1, Point pt2, Point pt3, Point pt4, out Point intersectPt)
        {
            intersectPt = new Point();
            if (HttpContext.Current.Session["__UserID"] == null) return false;
            //如果直线1的斜率不等于无穷;
            if (pt2.X - pt1.X != 0)
            {
                double k1 = (pt2.Y - pt1.Y) / (pt2.X - pt1.X);
                double b1 = pt2.Y - k1 * pt1.X;

                double k2 = (pt4.Y - pt3.Y) / (pt4.X - pt3.X);
                double b2 = pt4.Y - k2 * pt3.X;

                double x = 0.0;
                double y = 0.0;

                //如果线段2的斜率不等于无穷;
                if (pt4.X - pt3.X != 0)
                {
                    x = -(b2 - b1) / (k2 - k1);
                    y = k1 * x + b1;
                }
                else
                {
                    x = pt3.X;
                    y = k1 * x + b1;
                }
                if (x >= pt3.X && x <= pt4.X && y >= pt3.Y && y <= pt4.Y)
                {
                   // intersectPt = new Point(x, y, 0);
                    intersectPt.X = x;
                    intersectPt.Y = y;
                    intersectPt.Z = 0;
                    return true;
                }
                if (x <= pt3.X && x >= pt4.X && y <= pt3.Y && y >= pt4.Y)
                {
                    //intersectPt = new Point(x, y, 0);
                    intersectPt.X = x;
                    intersectPt.Y = y;
                    intersectPt.Z = 0;
                    return true;
                }
            }
            //如果直线1的斜率是无穷;
            else
            {
                double x = 0.0;
                double y = 0.0;

                double k2 = (pt4.Y - pt3.Y) / (pt4.X - pt3.X);
                double b2 = pt4.Y - k2 * pt3.X;

                //如果线段2的斜率不等于无穷;
                if (pt4.X - pt3.X != 0)
                {
                    x = pt1.X;
                    y = k2 * x + b2;
                }
                else
                {
                    return false;
                }
                if (x >= pt3.X && x <= pt4.X && y >= pt3.Y && y <= pt4.Y)
                {
                    //intersectPt = new Point(x, y, 0);
                    intersectPt.X = x;
                    intersectPt.Y = y;
                    intersectPt.Z = 0;
                    return true;
                }
                if (x <= pt3.X && x >= pt4.X && y <= pt3.Y && y >= pt4.Y)
                {
                    //intersectPt = new Point(x, y, 0);
                    intersectPt.X = x;
                    intersectPt.Y = y;
                    intersectPt.Z = 0;
                    return true;
                }
            }
            return false;
        }

        #endregion
    }
}