using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Globalization;

namespace RoundpayFinTech.AppCode.Model.MoneyTransfer
{
    #region MTRequest
    public class MTCommonRequest
    {
        public int UserID { get; set; }
        public int OutletID { get; set; }
        public int OID { get; set; }
        public string SenderMobile { get; set; }
        public string ReferenceID { get; set; }
        public int RequestMode { get; set; }
    }
    public class MTGetChargeRequest : MTCommonRequest
    {
        public int Amount { get; set; }
    }
    public class MTOTPRequest : MTCommonRequest
    {
        public string OTP { get; set; }
    }
    public class MTSenderDetail : MTCommonRequest
    {
        public string FName { get; set; }
        public string LName { get; set; }
        public string Address { get; set; }
        public int Pincode { get; set; }
        public string DOB { get; set; }//dd MMM yyyy
        public string OTP { get; set; }//dd MMM yyyy
        #region MandatoryForKYC
        public string NameOnKYC { get; set; }
        public string AadharNo { get; set; }
        public string PANNo { get; set; }
        public IFormFile AadharFront { get; set; }
        public IFormFile AadharBack { get; set; }
        public IFormFile SenderPhoto { get; set; }
        public IFormFile PAN { get; set; }
        #endregion
        public string PidData { get; set; }
    }

    public class MTBeneficiaryAddRequest : MTCommonRequest
    {
        public int BankID { get; set; }
        public MBeneDetail BeneDetail { get; set; }
    }
    public class MBeneVerifyRequest : MTCommonRequest
    {
        public string MobileNo { get; set; }
        public string AccountNo { get; set; }
        public string OTP { get; set; }
        public string BeneficiaryID { get; set; }
        public string BeneficiaryName { get; set; }
        public string IFSC { get; set; }
        public string APIRequestID { get; set; }
        public int BankID { get; set; }
        public string Bank { get; set; }
        public string IMEI { get; set; }
        public string TransMode { get; set; }
        public int Amount { get; set; }
        public int AccountTableID { get; set; }
        public string SecureKey { get; set; }
        public bool IsInternal { get; set; }
    }
    #endregion
    #region MTResponse
    public class MTCommon
    {
        public int Statuscode { get; set; }
        public string Msg { get; set; }
        public int ErrorCode { get; set; }
        public string ReferenceID { get; set; }
    }
    public class MSenderModel : MTCommon
    {
        public string SenderName { get; set; }
        public string SenderMobile { get; set; }
        public int KYCStatus { get; set; }
        public bool IsSenderNotExists { get; set; }
        public bool IsEKYCAvailable { get; set; }
        public bool IsOTPGenerated { get; set; }
        public bool IsNotActive { get; set; }
        public bool IsNotCheckLimit { get; set; }
    }
    public class MSenderCreateResp : MTCommon
    {
        public bool IsOTPGenerated { get; set; }
        public bool IsOTPResendAvailble { get; set; }
        public string OTP { get; set; }
        public bool ShouldSerializeOTP() => false;
        public int WID { get; set; }
        public bool ShouldSerializeWID() => false;
    }
    public class MBeneficiaryResp : MTCommon
    {
        public IEnumerable<MBeneDetail> Beneficiaries { get; set; }
    }
    public class MBeneDetail
    {
        public string MobileNo { get; set; }
        public string BeneName { get; set; }
        public string IFSC { get; set; }
        public string AccountNo { get; set; }
        public string BankName { get; set; }
        public int BankID { get; set; }
        public string BeneID { get; set; }
        public bool IsVerified { get; set; } = true;
        public int TransMode { get; set; }
        public bool IMPSStatus { get; set; }
        public bool NEFTStatus { get; set; }
        public string CashFreeID { get; set; }

    }
    public class MSenderLoginResponse : MSenderModel
    {
        public decimal RemainingLimit { get; set; }
        public decimal AvailbleLimit { get; set; }
        public string BeneID { get; set; }
        public bool ShouldSerializeBeneID() => false;
    }
    public class MDMTResponse : MSenderModel
    {
        public string TransactionID { get; set; }
        public int TID { get; set; }
        public string GroupID { get; set; }
        public string Status { get; set; }
        public string LiveID { get; set; }
        public string APIRequestID { get; set; }
        public string BeneName { get; set; }
        public int BankID { get; set; }
        public string Bank { get; set; }
        public string BrandName { get; set; }
        public decimal Balance { get; set; }
    }
    #endregion
    #region MTAPIModels
    public class MTAPIRequest
    {
        public string APIOpCode{ get; set; }
        public string APIGroupCode{ get; set; }
        public string SenderMobile { get; set; }
        public string ReferenceID { get; set; }
        public string APIOutletID { get; set; }
        public int UserID { get; set; }
        public int APIID { get; set; }
        public int TID { get; set; }
        public string TransactionID { get; set; }
        public int RequestMode { get; set; }
        public string NameOnKYC { get; set; }
        public string AadharNo { get; set; }
        public string PANNo { get; set; }
        public string AadharFrontURL { get; set; }
        public string AadharBackURL { get; set; }
        public string SenderPhotoURL { get; set; }
        public string PANURL { get; set; }
        public string City { get; set; }
        public int StateID { get; set; }
        public string StateName { get; set; }
        public string Area { get; set; }
        public string Address { get; set; }
        public string Districtname { get; set; }
        public int Pincode { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string DOB { get; set; }
        public string OTP { get; set; }
        public string UserName { get; set; }
        public string UserMobile { get; set; }
        public string EmailID { get; set; }
        public MBeneDetail mBeneDetail { get; set; }
        public int BankID { get; set; }
        public int Amount { get; set; }
        public string TransMode { get; set; }
        public string VID { get; set; }
        public string UIDToken { get; set; }
        public string PidData { get; set; }
        public string APICode { get; set; }
        public string IPAddress { get; set; }
        public string WebsiteName { get; set; }
        public string Lattitude { get; set; }
        public string Longitude { get; set; }
        public bool IsPayout { get; set; }
        public string GroupID { get; set; }
        public bool IsRefundReq { get; set; }
    }
    public class MTAPIResponse : MTCommon
    {

    }
    public class MDMTResponseAPI : MTCommon
    {
        public string TransactionID { get; set; }
        public string LiveID { get; set; }
        public string APIRequestID { get; set; }
        public decimal Balance { get; set; }
    }
    #endregion

    public class ColllectUPPayReqModel
    {
        public int UserID { get; set; }
        public decimal Amount { get; set; }
        public string UPIID { get; set; }
        public int TID { get; set; }
        public string TransactionID { get; set; }
        public string StatusCheckType { get; set; } = "P";
        public string LiveID { get; set; }
    }
    public class CollectUPPayResponse:ResponseStatus
    {
        public string BankRRN { get; set; }
        public string VendorID { get; set; }
        public string Remark { get; set; }
        public string Req { get; set; }
        public string Resp { get; set; }
        public string MerchantVPA { get; set; }
        public bool ShouldSerializeReq() => false;
        public bool ShouldSerializeResp() => false;
    }
    
}
