using System.ServiceModel;
using System.Runtime.Serialization;
using System.Collections.Generic;

namespace WcfService
{
    [DataContract]
    public class Point
    {
         [DataMember]
        public double X;
         [DataMember]
        public double Y;
         [DataMember]
        public double Z;
    }
    [DataContract]
    public class User
    {
        [DataMember]
        public int SerialNo;
        [DataMember]
        public int Purview;
        [DataMember]
        public string UserName;
        [DataMember]
        public string Telephone;
        [DataMember]
        public string Email;
        [DataMember]
        public string QQ;
        [DataMember]
        public string Address;
        [DataMember]
        public string Company;
        [DataMember]
        public string Remark;
    }
    [ServiceContract]
    public interface IDimService
    {
        [OperationContract]
        User GetUser(ulong UserID);
        [OperationContract]
        void CloseService();
        [OperationContract]
        string GetUserAuthority(ulong UserID, string UserKey);
        [OperationContract]
        string GetUserName();
        [OperationContract]
        bool IsPartInViewBox(Point partMinPoint, Point partMaxPoint, Point viewMinPoint, Point viewMaxPoint);
        [OperationContract]
        int JudgeLineSlope(Point pt1, Point pt2);
        [OperationContract]
        bool IsTwoPointEqual(Point point1, Point point2);
        [OperationContract]
        bool IsLineIntersectWithLineSegment(Point pt1, Point pt2, Point pt3, Point pt4, out Point intersectPt);
    }
}