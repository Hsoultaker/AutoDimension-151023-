﻿//------------------------------------------------------------------------------
// <auto-generated>
//     此代码由工具生成。
//     运行时版本:4.0.30319.34209
//
//     对此文件的更改可能会导致不正确的行为，并且如果
//     重新生成代码，这些更改将会丢失。
// </auto-generated>
//------------------------------------------------------------------------------

namespace AutoDimension.ServiceReference {
    using System.Runtime.Serialization;
    using System;
    
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="User", Namespace="http://schemas.datacontract.org/2004/07/WcfService")]
    [System.SerializableAttribute()]
    public partial class User : object, System.Runtime.Serialization.IExtensibleDataObject, System.ComponentModel.INotifyPropertyChanged {
        
        [System.NonSerializedAttribute()]
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string AddressField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string CompanyField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string EmailField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private int PurviewField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string QQField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string RemarkField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private int SerialNoField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string TelephoneField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string UserNameField;
        
        [global::System.ComponentModel.BrowsableAttribute(false)]
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData {
            get {
                return this.extensionDataField;
            }
            set {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string Address {
            get {
                return this.AddressField;
            }
            set {
                if ((object.ReferenceEquals(this.AddressField, value) != true)) {
                    this.AddressField = value;
                    this.RaisePropertyChanged("Address");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string Company {
            get {
                return this.CompanyField;
            }
            set {
                if ((object.ReferenceEquals(this.CompanyField, value) != true)) {
                    this.CompanyField = value;
                    this.RaisePropertyChanged("Company");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string Email {
            get {
                return this.EmailField;
            }
            set {
                if ((object.ReferenceEquals(this.EmailField, value) != true)) {
                    this.EmailField = value;
                    this.RaisePropertyChanged("Email");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public int Purview {
            get {
                return this.PurviewField;
            }
            set {
                if ((this.PurviewField.Equals(value) != true)) {
                    this.PurviewField = value;
                    this.RaisePropertyChanged("Purview");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string QQ {
            get {
                return this.QQField;
            }
            set {
                if ((object.ReferenceEquals(this.QQField, value) != true)) {
                    this.QQField = value;
                    this.RaisePropertyChanged("QQ");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string Remark {
            get {
                return this.RemarkField;
            }
            set {
                if ((object.ReferenceEquals(this.RemarkField, value) != true)) {
                    this.RemarkField = value;
                    this.RaisePropertyChanged("Remark");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public int SerialNo {
            get {
                return this.SerialNoField;
            }
            set {
                if ((this.SerialNoField.Equals(value) != true)) {
                    this.SerialNoField = value;
                    this.RaisePropertyChanged("SerialNo");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string Telephone {
            get {
                return this.TelephoneField;
            }
            set {
                if ((object.ReferenceEquals(this.TelephoneField, value) != true)) {
                    this.TelephoneField = value;
                    this.RaisePropertyChanged("Telephone");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string UserName {
            get {
                return this.UserNameField;
            }
            set {
                if ((object.ReferenceEquals(this.UserNameField, value) != true)) {
                    this.UserNameField = value;
                    this.RaisePropertyChanged("UserName");
                }
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="Point", Namespace="http://schemas.datacontract.org/2004/07/WcfService")]
    [System.SerializableAttribute()]
    public partial class Point : object, System.Runtime.Serialization.IExtensibleDataObject, System.ComponentModel.INotifyPropertyChanged {
        
        [System.NonSerializedAttribute()]
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private double XField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private double YField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private double ZField;
        
        [global::System.ComponentModel.BrowsableAttribute(false)]
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData {
            get {
                return this.extensionDataField;
            }
            set {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public double X {
            get {
                return this.XField;
            }
            set {
                if ((this.XField.Equals(value) != true)) {
                    this.XField = value;
                    this.RaisePropertyChanged("X");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public double Y {
            get {
                return this.YField;
            }
            set {
                if ((this.YField.Equals(value) != true)) {
                    this.YField = value;
                    this.RaisePropertyChanged("Y");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public double Z {
            get {
                return this.ZField;
            }
            set {
                if ((this.ZField.Equals(value) != true)) {
                    this.ZField = value;
                    this.RaisePropertyChanged("Z");
                }
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="ServiceReference.IDimService")]
    public interface IDimService {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IDimService/GetUser", ReplyAction="http://tempuri.org/IDimService/GetUserResponse")]
        AutoDimension.ServiceReference.User GetUser(ulong UserID);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IDimService/CloseService", ReplyAction="http://tempuri.org/IDimService/CloseServiceResponse")]
        void CloseService();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IDimService/GetUserAuthority", ReplyAction="http://tempuri.org/IDimService/GetUserAuthorityResponse")]
        string GetUserAuthority(ulong UserID, string UserKey);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IDimService/GetUserName", ReplyAction="http://tempuri.org/IDimService/GetUserNameResponse")]
        string GetUserName();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IDimService/IsPartInViewBox", ReplyAction="http://tempuri.org/IDimService/IsPartInViewBoxResponse")]
        bool IsPartInViewBox(AutoDimension.ServiceReference.Point partMinPoint, AutoDimension.ServiceReference.Point partMaxPoint, AutoDimension.ServiceReference.Point viewMinPoint, AutoDimension.ServiceReference.Point viewMaxPoint);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IDimService/JudgeLineSlope", ReplyAction="http://tempuri.org/IDimService/JudgeLineSlopeResponse")]
        int JudgeLineSlope(AutoDimension.ServiceReference.Point pt1, AutoDimension.ServiceReference.Point pt2);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IDimService/IsTwoPointEqual", ReplyAction="http://tempuri.org/IDimService/IsTwoPointEqualResponse")]
        bool IsTwoPointEqual(AutoDimension.ServiceReference.Point point1, AutoDimension.ServiceReference.Point point2);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IDimService/IsLineIntersectWithLineSegment", ReplyAction="http://tempuri.org/IDimService/IsLineIntersectWithLineSegmentResponse")]
        bool IsLineIntersectWithLineSegment(out AutoDimension.ServiceReference.Point intersectPt, AutoDimension.ServiceReference.Point pt1, AutoDimension.ServiceReference.Point pt2, AutoDimension.ServiceReference.Point pt3, AutoDimension.ServiceReference.Point pt4);
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface IDimServiceChannel : AutoDimension.ServiceReference.IDimService, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class DimServiceClient : System.ServiceModel.ClientBase<AutoDimension.ServiceReference.IDimService>, AutoDimension.ServiceReference.IDimService {
        
        public DimServiceClient() {
        }
        
        public DimServiceClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public DimServiceClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public DimServiceClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public DimServiceClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        public AutoDimension.ServiceReference.User GetUser(ulong UserID) {
            return base.Channel.GetUser(UserID);
        }
        
        public void CloseService() {
            base.Channel.CloseService();
        }
        
        public string GetUserAuthority(ulong UserID, string UserKey) {
            return base.Channel.GetUserAuthority(UserID, UserKey);
        }
        
        public string GetUserName() {
            return base.Channel.GetUserName();
        }
        
        public bool IsPartInViewBox(AutoDimension.ServiceReference.Point partMinPoint, AutoDimension.ServiceReference.Point partMaxPoint, AutoDimension.ServiceReference.Point viewMinPoint, AutoDimension.ServiceReference.Point viewMaxPoint) {
            return base.Channel.IsPartInViewBox(partMinPoint, partMaxPoint, viewMinPoint, viewMaxPoint);
        }
        
        public int JudgeLineSlope(AutoDimension.ServiceReference.Point pt1, AutoDimension.ServiceReference.Point pt2) {
            return base.Channel.JudgeLineSlope(pt1, pt2);
        }
        
        public bool IsTwoPointEqual(AutoDimension.ServiceReference.Point point1, AutoDimension.ServiceReference.Point point2) {
            return base.Channel.IsTwoPointEqual(point1, point2);
        }
        
        public bool IsLineIntersectWithLineSegment(out AutoDimension.ServiceReference.Point intersectPt, AutoDimension.ServiceReference.Point pt1, AutoDimension.ServiceReference.Point pt2, AutoDimension.ServiceReference.Point pt3, AutoDimension.ServiceReference.Point pt4) {
            return base.Channel.IsLineIntersectWithLineSegment(out intersectPt, pt1, pt2, pt3, pt4);
        }
    }
}
