using Fintech.AppCode.Model;
using System.Collections.Generic;

namespace RoundpayFinTech.AppCode.Model.ProcModel
{
    public class OutletsOfUsersList
    {
        public int ResultCode { get; set; }
        public string Msg { get; set; }
        public int _ServiceID { get; set; }
        public int _ID { get; set; }
        public string _Prefix { get; set; }
        public int _UserID { get; set; }
        public string DisplayUserID { get; set; }
        public string UserName { get; set; }
        public string UserMobile { get; set; }
        public string _Name { get; set; }
        public string _Company { get; set; }
        public string _MobileNo { get; set; }
        public string _EmailID { get; set; }
        public string _Pincode { get; set; }
        public string _Address { get; set; }
        public string _PAN { get; set; }
        public string _AADHAR { get; set; }
        public int _VerifyStatus { get; set; }
        public string VerifyStatus { get; set; }
        public bool _IsActive { get; set; }
        public string _OType { get; set; }
        public int _EntryBy { get; set; }
        public string _EntryDate { get; set; }
        public int _ModifyBy { get; set; }
        public string _ModifyDate { get; set; }
        public int _RoleID { get; set; }
        public string Role { get; set; }
        public bool _IsOutsider { get; set; }
        public int _KYCStatus { get; set; }
        public string KYCStatus { get; set; }
        public string _State { get; set; }
        public string _City { get; set; }
        public string _DOB { get; set; }
        public string _shopType { get; set; }
        public string _Qualification { get; set; }
        public string _Poupulation { get; set; }
        public string _LocationType { get; set; }
        public string _Landmark { get; set; }
        public string _AlternateMobile { get; set; }
        public string _latlong { get; set; }
        public string _BankName { get; set; }
        public string _IFSC { get; set; }
        public string _AccountNumber { get; set; }
        public string _AccountHolder { get; set; }
        public int BBPSStatus { get; set; }
        public string _BBPSStatus { get; set; }
        public int AEPSStatus { get; set; }
        public string _AEPSStatus { get; set; }
        public int PSAStatus { get; set; }
        public string _PSAStatus { get; set; }
        public int DMTStatus { get; set; }
        public string _DMTStatus { get; set; }
        public string DeviceId { get; set; }
        public string _MATMStatus { get; set; }
        public int MATMStatus { get; set; }
        public int ApiId { get; set; }
        public string ApiName { get; set; }
        public string ApiOutletId { get; set; }
        public int IRCTCStaus { get; set; }
        public string _IRCTCStaus { get; set; }
        public string IRCTCID { get; set; }
        public string IRCTCExpiry { get; set; }
    }
    public class OutletsOfUsersListIndex
    {
        public int OutletID { get; set; }
        public int APIOutletID { get; set; }
        public int verifyStatus { get; set; }
        public int DocVerifyStatus { get; set; }
        public int BBPSID { get; set; }
        public int BBPSStatus { get; set; }
        public int AEPSID { get; set; }
        public int AEPSStatus { get; set; }
        public int PANRequestID { get; set; }
        public int PANID { get; set; }
        public int PANStatus { get; set; }
        public int DMTID { get; set; }
        public int DMTStatus { get; set; }
        public int Password { get; set; }
        public int Pin { get; set; }
    }

    public class _OutletsOfUsersList
    {
        public int OutletID { get; set; }
        public string APIOutletID { get; set; }
        public int verifyStatus { get; set; }
        public int DocVerifyStatus { get; set; }
        public string BBPSID { get; set; }
        public int BBPSStatus { get; set; }
        public string AEPSID { get; set; }
        public int AEPSStatus { get; set; }
        public int PANRequestID { get; set; }
        public string PANID { get; set; }
        public int PANStatus { get; set; }
        public string DMTID { get; set; }
        public int DMTStatus { get; set; }
        public string Password { get; set; }
        public string Pin { get; set; }
    }

    public class OutletsReqData : CommonReq
    {
        public List<_OutletsOfUsersList> OutletsUserList { get; set; }
        public int APIId { get; set; }
    }
    public class UserDataForAEPS
    {
        public int Statuscode { get; set; }
        public string Msg { get; set; }
        public int UserID { get; set; }
        public int OutletID { get; set; }
        public int APIID { get; set; }
        public string MobileNo { get; set; }
        public string LatLong { get; set; }
        public string Pincode { get; set; }
        public string APIOutletID { get; set; }
        public string APICode { get; set; }
        public string TransactionID { get; set; }
    }

    public class AddmAtmModel
    {
        public int LoginID { get; set; }
        public int LT { get; set; }
        public int Statuscode { get; set; }
        public string Msg { get; set; }
        public int UserID { get; set; }
        public string DeviceId { get; set; }
    }

    public class MAtmModel
    {
        public int StatusCode { get; set; }
        public string Msg { get; set; }
        public int ID { get; set; }
        public int UserID { get; set; }
        public int RoleID { get; set; }
        public string Name { get; set; }
        public string RoleName { get; set; }
        public string Company { get; set; }
        public string MobileNo { get; set; }
        public string EmailId { get; set; }
        public string PartnerName { get; set; }
        public string OutletName { get; set; }
        public string mAtamSerialNo { get; set; }
        public int mAtamStatus { get; set; }
        public string ExID { get; set; }
        public string ExName { get; set; }
        public string Address { get; set; }
        public string State { get; set; }
        public string City { get; set; }
        public string Pincode { get; set; }
        public string Pan { get; set; }
        public string KYCDoc { get; set; }
    }

    public class MAtmModelResp
    {
        public List<MAtmModel> MAtmModelR { get; set; }
        public bool IsAdmin { get; set; }
    }

    public class MAtmFilterModel
    {
        public int LT { get; set; }
        public int LoginID { get; set; }
        public int UserID { get; set; }
        public int TopRows { get; set; }
        public string Name { get; set; }
        public string MobileNo { get; set; }
        public string mAtamSerialNo { get; set; }
        public int mAtamStatus { get; set; }
        public int Criteria { get; set; }
        public string CriteriaText { get; set; }
        public bool IsExport { get; set; }
    }

    public class mAtmRequestResponse
    {
        public int StatusCode { get; set; }
        public string Msg { get; set; }
        public int LT { get; set; }
        public int LoginID { get; set; }
        public int ID { get; set; }
        public int Status { get; set; }
    }
}
