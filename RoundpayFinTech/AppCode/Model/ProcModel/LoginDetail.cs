using Microsoft.AspNetCore.Mvc.Rendering;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.DepartmentModel;
using RoundpayFinTech.AppCode.Model.Employee;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;

namespace Fintech.AppCode.Model
{
    public class CommonReq
    {
        public int LoginID { get; set; }
        public int LoginTypeID { get; set; }
        public int UserID { get; set; }
        public int CommonInt { get; set; }
        public int CommonInt2 { get; set; }
        public string CommonStr { get; set; }
        public string CommonStr1 { get; set; }
        public string CommonStr2 { get; set; }
        public bool IsListType { get; set; }
        public string str { get; set; }
        public int CommonInt3 { get; set; }
        public decimal CommonDecimal { get; set; }
        public int CommonInt4 { get; set; }
        public string CommonStr3 { get; set; }
        public string CommonStr4 { get; set; }
        public string CommonStr5 { get; set; }
        public bool CommonBool { get; set; }
        public bool CommonBool1 { get; set; }
        public bool CommonBool2 { get; set; }
        public char CommonChar { get; set; }
    }
    public class LoginReq : CommonReq
    {
        public int RequestMode { get; set; }
        public string RequestIP { get; set; }
        public string Browser { get; set; }
        public string MobileNo { get; set; }
    }
    public class LoginDetail : LoginReq
    {
        public DataTable Tp_SaveWhatsappContact { get; set; }
        public DataTable Tp_SaveWhatsappMsgTemplate { get; set; }
        public string Password { get; set; }
        public string AccountSecretKey { get; set; }
        public string OTP { get; set; }
        public string GooglePin { get; set; }
        public string EmailOTP { get; set; }
        public string MobileOTP { get; set; }
        public int WID { get; set; }
        public string Prefix { get; set; }
        public string LoginMobile { get; set; }
        public int SessID { get; set; }
        public string SessionID { get; set; }
        public bool IsOTPMatchUpdate { get; set; }
        public DateTime? CookieExpireTime { get; set; }
        public string UserAgent { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public bool IsActive { get; set; }
    }
    public class LogoutReq
    {
        public int LT { get; set; }
        public int LoginID { get; set; }
        public int SessID { get; set; }
        public int ULT { get; set; }
        public int UserID { get; set; }
        public int SessionType { get; set; }
        public int RequestMode { get; set; }
        public string IP { get; set; }
        public string Browser { get; set; }
    }
    public class UserQRInfo
    {
        public string BankName { get; set; }
        public string IFSC { get; set; }
        public string VirtualAccount { get; set; }
        public string Branch { get; set; }
        public string BeneName { get; set; }
        public UserSmartDetail userSDetail { get; set; }
    }
    public class UserInfo
    {
        public int ResultCode { get; set; }
        public string Msg { get; set; }
        public int UserID { get; set; }
        public string MobileNo { get; set; }
        public string Name { get; set; }
        public string RequestStatus { get; set; }
        public int RequestID { get; set; }
        public string OutletName { get; set; }
        public string EmailID { get; set; }
        public int RoleID { get; set; }
        public string Role { get; set; }
        public int ReferalID { get; set; }
        public int SlabID { get; set; }
        public bool IsGSTApplicable { get; set; }
        public bool IsTDSApplicable { get; set; }
        public bool IsDenominationSwtichBlock { get; set; }
        public int DMRModelID { get; set; }
        public bool IsVirtual { get; set; }
        public bool IsWebsite { get; set; }
        public bool IsAdminDefined { get; set; }
        public bool IsSurchargeGST { get; set; }
        public string Prefix { get; set; }
        public int OutletID { get; set; }
        public string Pincode { get; set; }
        public int WID { get; set; }
        public bool CanDebit { get; set; }
        public bool IsDoubleFactor { get; set; }
        public bool IsPasswordExpired { get; set; }
        public bool IsPinRequired { get; set; }
        public int StateID { get; set; }
        public int CityID { get; set; }
        public string State { get; set; }
        public string CustomLoginID { get; set; }
        public bool IsRealAPI { get; set; }
        public bool IsSwitchIMPStoNEFT { get; set; }
        public int EKYCID { get; set; }
        public bool IsCalculateCommissionFromCircle { get; set; }
        public bool IsFlatCommission { get; set; }
        public bool IsWLAPIAllowed { get; set; }
        public bool IsMarkedGreen { get; set; }
        public List<UserBlockDetail> blockDetails { get; set; }
        public int AreaID { get; set; }
        public bool IsPaymentGateway { get; set; }
        public bool IsDebitAllowed { get; set; }
        public string B2CDomain { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        
        public string LaunchPreferences { get; set; }
        public bool IsMultiLevel { get; set; }
    }
    public class UserBlockDetail
    {
        public int SwitchingTypeID { get; set; }
        public bool IsActive { get; set; }
    }
    public class LoginResponse : UserInfo
    {
        public string OTP { get; set; }
        public string SessionID { get; set; }
        public int SessID { get; set; }
        public string RoleName { get; set; }
        public DateTime CookieExpire { get; set; }
        public int LoginTypeID { get; set; }
        public string LoginType { get; set; }
        public IEnumerable<OperationAssigned> operationsAssigned { get; set; }
        public string Token { get; set; }
        public bool IsGoogle2FAEnable { get; set; }
        public string AccountSecretKey { get; set; }
        public bool IsDeviceAuthenticated { get; set; }
    }
    public class UserRequset : LoginReq
    {
        public int UserId { get; set; }
    }
    public class UserDetail : UserInfo
    {
        public string Token { get; set; }
    }
    public class SlabMaster
    {
        public int ID { get; set; }
        public string Slab { get; set; }
        public bool IsRealSlab { get; set; }
        public string EntryDate { get; set; }
        public string ModifyDate { get; set; }
        public bool IsActive { get; set; }
        public string Remark { get; set; }
        public bool IsAdminDefined { get; set; }
        public bool IsDefault { get; set; }
        public bool IsB2B { get; set; }
        public Int16 DMRModelID { get; set; }
        public string DMRModel { get; set; }
        public bool IsSigunupSlabID { get; set; }
        public bool IsShowMore { get; set; }
        public bool SelfAssigned { get; set; }
        public decimal PackageCost { get; set; }
        public int TotalUser { get; set; }
        public int IsMultiLevel { get; set; }
        public int ValidityInDays { get; set; }
        public int DailyHitCount { get; set; }
    }
    public class SlabMasterReq : CommonReq
    {
        public SlabMaster slabMaster { get; set; }
    }
    public class SlabModel
    {
        public bool IsAdmin { get; set; }
        public bool IsWebsite { get; set; }
        public IEnumerable<SlabMaster> slabMasters { get; set; }
        public SlabMaster slabMaster { get; set; }
        public SelectList DMRModelSelect { get; set; }        
        public IEnumerable<RangeSlabDetailDisplayLvl> LVRangeSlab { get; set; }
    }
    public class RoleMaster
    {
        public int ID { get; set; }
        public string Role { get; set; }
        public int Ind { get; set; }
        public bool IsActive { get; set; }
        public string Prefix { get; set; }
    }
    public class UserRoleSlab
    {
        public List<RoleMaster> Roles { get; set; }
        public List<SlabMaster> Slabs { get; set; }
    }
    public class GetChangeSlab : ResponseStatus
    {
        public bool IsDoubleFactor { get; set; }
        public List<SlabMaster> Slabs { get; set; }
    }
    public class UserCreate : UserInfo
    {
        public string Address { get; set; }
        public string WhatsAppNumber { get; set; }
        public string Pin { get; set; }
        public int LoginID { get; set; }
        public int LoginWID { get; set; }
        public int LTID { get; set; }
        public string Password { get; set; }
        public string WebsiteName { get; set; }
        public string Token { get; set; }
        public string IP { get; set; }
        public string Browser { get; set; }
        public string ReferralNo { get; set; }
        public string OtherUserID { get; set; }
        public int RequestModeID { get; set; }
        public decimal CommRate { get; set; }
        //public string CustomLoginID { get; set; }
    }
    public class UserDuplicate
    {
        public string CustomLoginID { get; set; }
        public int LoginID { get; set; }
    }
    public class CommonEditRequest
    {
        public int LoginID { get; set; }
        public int LTID { get; set; }
        public int UserID { get; set; }
        public bool Is { get; set; }
    }
    public class ProcToggleStatusRequest : CommonEditRequest
    {
        public int StatusColumn { get; set; }
        public string IP { get; set; }
        public string Browser { get; set; }
    }
    public class PincodeDetail : StateMaster
    {
        public int Statuscode { get; set; }
        public string Msg { get; set; }
        public string City { get; set; }
        public string Area { get; set; }
        public string Districtname { get; set; }
        public string Statename { get; set; }
        public string Pincode { get; set; }
        public string Lat { get; set; }
        public string Long { get; set; }
        public int ID { get; set; }
        public int ReachInHour { get; set; }
        public int ExpectedDeliverInDays { get; set; }
        public bool IsDeliveryOff { get; set; }
    }
    public class ChangePassword
    {
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
        public int SessID { get; set; }
    }
    public class ForgetPasss : ResponseStatus
    {
        public string Password { get; set; }
        public string EmailID { get; set; }
        public string Prefix { get; set; }
        public string MobileNo { get; set; }
        public int UserID { get; set; }
        public string PIN { get; set; }
        public bool IsPrefix { get; set; }
    }

    public class BulkActionReq
    {
        public int ActionID { get; set; }
        public int WalletType { get; set; }
        public decimal Amount { get; set; }
        public int Status { get; set; }
        public string Users { get; set; }
        public bool IsAll { get; set; }
        public bool IsWhole { get; set; }
        public int RoleID { get; set; }
        public string Token { get; set; }
        public string FromIntro { get; set; }
        public string ToIntro { get; set; }
    }
    public class BulkAct
    {
        public int LoginID { get; set; }
        public int LTID { get; set; }
        public BulkActionReq Act { get; set; }
        public int IntoID { get; set; }
    }

    public class BulkExcel : ExcelData
    {
        public string UserID { get; set; }
        public string UserName { get; set; }
        public string MobileNo { get; set; }
        public string WhatsappNumber { get; set; }
        public string EMailID { get; set; }
        public string IsEMailVerified { get; set; }
        public string Role { get; set; }
        public decimal Balance { get; set; }
        public decimal UBalance { get; set; }
        public decimal BBalance { get; set; }
        public decimal PBalance { get; set; }
        public decimal Capping { get; set; }
        public string Referral { get; set; }
        public string KYCStatus { get; set; }
        public string RentalStatus { get; set; }
        public decimal RentalAmt { get; set; }
        public string PackageName { get; set; }
        public bool IsAutoBilling { get; set; }
        public bool FromFOSAB { get; set; }
        public int MaxBillingCountAB { get; set; }
        public int BalanceForAB { get; set; }
        public int MaxCreditLimitAB { get; set; }
        public int MaxTransferLimitAB { get; set; }
    }
    public class ExcelData
    {
        public string SA { get; set; }
        public string SAMobile { get; set; }
        public string SAID { get; set; }
        public string MD { get; set; }
        public string MDMobile { get; set; }
        public string MDID { get; set; }
        public string DT { get; set; }
        public string DTMobile { get; set; }
        public string DTID { get; set; }
        public string RT { get; set; }
        public string RTMobile { get; set; }
        public string RTID { get; set; }
    }
    public class RegeneratePassword
    {
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
        public string Pin { get; set; }
        public int SessID { get; set; }
        public int UserID { get; set; }

    }
    public class GetIntouch
    {

        public int ID { get; set; }
        public int WID { get; set; }
        public string Name { get; set; }
        public string MobileNo { get; set; }
        public string Browser { get; set; }
        public string RequestIP { get; set; }
        public string Role { get; set; }
        public string EmailID { get; set; }
        public string Message { get; set; }
        public string CustomerCareName { get; set; }
        public string Remarks { get; set; }
        public string EntryData { get; set; }
        public string ModifyDate { get; set; }
        public string NextFollowupdate { get; set; }
        public bool IsMobileMultiple { get; set; }
        public string RequestPage { get; set; }
        public string RequestStatus { get; set; }
        public int CustomercareID { get; set; }
        public int Statuscode { get; set; }
        public string Msg { get; set; }

    }
    public class CustomerCareDetails
    {
        public int CustomercareID { get; set; }
        public string CustomerCareName { get; set; }
    }
    public class GetinTouctListModel
    {
        public IEnumerable<CustomerCareDetails> CustomerCareDetails { get; set; }
        public List<GetIntouch> GetIntouchList { get; set; }
        public List<CustomerCareDetails> CustomerCareDetail { get; set; }
        public SelectList CustomerCareList { get; set; }
    }
    public class ReferralRoleMaster
    {
        public int ReferralID { get; set; }
        public IEnumerable<RoleMaster> Roles { get; set; }
    }
    public class LeadSummary
    {
        public long TotalMaturedNo { get; set; }

        public long TotalRequestNo { get; set; }

        public long TotalFollowUPNo { get; set; }

        public long TotalTransferNo { get; set; }
        public long TotalJunkNo { get; set; }

    }
    #region WalletToWallet

    public class WTWUserInfo
    {
        public bool IsDoubleFactor { get; set; }
        public int StatusCode { get; set; }
        public string Msg { get; set; }
        public int UserID { get; set; }
        public string OutletName { get; set; }
        public int RoleID { get; set; }
        public string MobileNo { get; set; }
        public bool IsPrepaidB { get; set; }
        public bool IsUtilityB { get; set; }
        public bool IsBankB { get; set; }
    }
    #endregion
    public class Membership
    {
        public bool IsAdmin { get; set; }
        public bool IsWebsite { get; set; }
        public IEnumerable<MembershipMaster> membershipMasters { get; set; }
        public MembershipMaster membershipMaster { get; set; }
        public IEnumerable<SlabMaster> slabMasters { get; set; }


    }
    public class MembershipMaster
    {
        public int ID { get; set; }
        public string MemberShipType { get; set; }
        public int CouponCount { get; set; }
        public decimal CouponValue { get; set; }
        public bool IsCouponAllowed { get; set; }
        public string Remark { get; set; }
        public int CouponValidityDays { get; set; }
        public bool IsActive { get; set; }
        public decimal Cost { get; set; } 
        public string EntryBy { get; set; }
        public string EntryDate { get; set; }
        public string ModifyBy { get; set; }
        public string ModifyDate { get; set; }
        public int SlabID { get; set; }
        public int MinInterval { get; set; }
        public string SlabName { get; set; }
        public bool IsAdminDefined { get; set; }
        public double ReferralIncome { get; set; }
        public int PackageValidity { get; set; }
    }
    public class MembershipmasterB2C {
        public bool IsIDActive { get; set; }
        public int ID { get; set; }
        public string MemberShipType { get; set; }
        public int CouponCount { get; set; }
        public decimal CouponValue { get; set; }
        public bool IsCouponAllowed { get; set; }
        public string Remark { get; set; }
        public int CouponValidityDays { get; set; }
        public decimal Cost { get; set; }
    }
    public class B2CMemberCouponDetail {
        public int ID { get; set; }
        public string CouponCode { get; set; }
        public string CouponExpiry { get; set; }
        public string RedeemDate { get; set; }
        public bool IsRedeemed { get; set; }
        public decimal CouponValue { get; set; }
    }
    public class MembershipMasteReq : CommonReq
    {
        public MembershipMaster memMaster { get; set; }
    }
}
