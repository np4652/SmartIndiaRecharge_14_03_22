using RoundpayFinTech.AppCode.ThirdParty.Roundpay;
using System.Collections.Generic;

namespace RoundpayFinTech.AppCode.Model.ProcModel
{
    public class ValidateAPIOutletResp : GetEditUser
    {
        public int Statuscode { get; set; }
        public string OutletMobile { get; set; }
        public bool IsTakeCustomerNum { get; set; }
        public bool IsOutletActive { get; set; }
        public int OutletVerifyStatus { get; set; }
        public int OType { get; set; }
        public int BioAuthType { get; set; }
        public string APICode { get; set; }
        public int APIID { get; set; }
        public string TransactionID { get; set; }
        public string APIOutletID { get; set; }
        public int APIOutletVerifyStatus { get; set; }
        public int APIOutletDocVerifyStatus { get; set; }
        public int OTPRefID { get; set; }
        public string AdminRejectRemark { get; set; }
        public bool IsActive { get; set; }
        public string Remark { get; set; }
        public int PANRequestID { get; set; }
        public int VerifyStatus { get; set; }
        public bool IsOutletRequired { get; set; }
        public int ErrorCode { get; set; }
        public bool IsOutletFound { get; set; }
        public bool IsAPIOutletFound { get; set; }
        public string BBPSID { get; set; }
        public int BBPSStatus { get; set; }
        public string AEPSID { get; set; }
        public int AEPSStatus { get; set; }
        public string PANID { get; set; }
        public int PANStatus { get; set; }
        public string DMTID { get; set; }
        public int DMTStatus { get; set; }
        public string RailID { get; set; }
        public int RailStatus { get; set; }
        public int OID { get; set; }
        public string SCode { get; set; }
        public bool IsBBPS { get; set; }
        public int ServiceTypeID { get; set; }
        public string OPID { get; set; }
        public int OPTypeID { get; set; }
        public string FixedOutletID { get; set; }
        public bool IsOutletManual { get; set; }
        public int SenderLimit { get; set; }
        public int MaxLimitPerTransaction { get; set; }
        public int CBA { get; set; }
        public string VID { get; set; }
        public string UIDToken { get; set; }
        public string BranchName { get; set; }
        public string APIOpCode { get; set; }
        public int APIType { get; set; }
        public string APIGroupCode { get; set; }
        public string WebsiteName { get; set; }
        public string APIOutletPassword { get; set; }
        public string APIOutletPIN { get; set; }
        public int PartnerID { get; set; }
        public int EKYCID { get; set; }
        public int APIEKYCStatus { get; set; }
        public string TwoWayAuthDate { get; set; }
        public string OTP { get; set; }
        public string APIReferenceID { get; set; }
        public string APIHash { get; set; }
        public PidData pidData { get; set; }
        public string PIDATA { get; set; }
    }
    public class CheckOutletStatusReqModel
    {
        public int LoginTypeID { get; set; }
        public int LoginID { get; set; }
        public int OID { get; set; }
        public int BioAuthType { get; set; }
        public int OutletID { get; set; }
        public string SPKey { get; set; }
        public string OTP { get; set; }
        public int RMode { get; set; }
        public int PartnerID { get; set; }
        public string Token { get; set; }
        public int OTPRefID { get; set; }
        public string PidData { get; set; }
        public bool IsVerifyBiometric { get; set; }
    }
    public class ValidateOuletModel : ResponseStatus
    {
        public bool IsConfirmation { get; set; }
        public bool IsTakeCustomerNum { get; set; }
        public bool IsRedirection { get; set; }
        public bool IsRedirectToExternal { get; set; }
        public bool IsOTPRequired { get; set; }
        public bool IsBioMetricRequired { get; set; }
        public int BioAuthType { get; set; }
        public int OTPRefID { get; set; }
        public bool IsDown { get; set; }
        public bool IsWaiting { get; set; }
        public bool IsRejected { get; set; }
        public bool IsIncomplete { get; set; }
        public bool IsUnathorized { get; set; }
        public bool IsShowMsg { get; set; }
        public bool InInterface { get; set; }
        public short SDKType { get; set; }
        public AppSDKDetail SDKDetail { get; set; }
        public bool IsEKYCRequired { get; set; }
        public string Aadhar { get; set; }
        public short InterfaceType { get; set; }
        public List<_BCResponse> BCResponse { get; set; }
    }
    public class AppSDKDetail
    {
        public string APIOutletID { get; set; }
        public string APIOutletPassword { get; set; }
        public string APIPartnerID { get; set; }
        public string APIOutletMob { get; set; }
        public string ServiceOutletPIN { get; set; }
        public string EmailID { get; set; }
        public string OutletName { get; set; }
        public List<_BCResponse> BCResponse { get; set; }
    }

    public class OnboardingStatusCheckModel : ResponseStatus
    {
        public bool IsAPIOutletExists { get; set; }
        public bool IsEditProfile { get; set; }
        public bool IsRedirection { get; set; }
        public string RedirectionUrI { get; set; }
    }
    public class ServicePlusStatusModel : ResponseStatus
    {
        public bool IsOTPRequired { get; set; }
        public bool IsBioMetricRequired { get; set; }
        public int BioAuthType { get; set; }
        public int OTPRefID { get; set; }
        public string AEPSURL { get; set; }
        public int ServiceStatus { get; set; }

    }

    public class GenerateOTPModel : ResponseStatus
    {
        public bool IsOTPRequired { get; set; }
        public bool IsBioMetricRequired { get; set; }
        public int OTPRefID { get; set; }
        public bool IsRedirection { get; set; }
        public string RedirectURL { get; set; }
    }
    public class OutletServicePlusReqModel
    {
        public ValidateAPIOutletResp _ValidateAPIOutletResp { get; set; }
        public bool IsOnboarding { get; set; }
        public bool IsOnboardingStatusCheck { get; set; }
        public bool IsBBPSServicePlus { get; set; }
        public bool IsAEPSServicePlus { get; set; }
        public bool IsPANServicePlus { get; set; }
        public bool IsDMRServicePlus { get; set; }
        public bool IsTravelServicePlus { get; set; }
        public string ServicPlusOTP { get; set; }
        public string RequestIP { get; set; }
    }

    public class OutletEKYCRequest
    {
        public string PidData { get; set; }
        public string AadaarNo { get; set; }
        public int UserID { get; set; }
        public int OutletID { get; set; }
    }
    public class OutletbBiometricModel {
        public int OID { get; set; }
        public int RefID { get; set; }
        public int BioAuthType { get; set; }
    }
}
