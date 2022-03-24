using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.MoneyTransfer;
using RoundpayFinTech.AppCode.Model.Shopping;
using System.Collections.Generic;

namespace RoundpayFinTech.AppCode.Model.ProcModel
{
    public class APIRequest
    {
        public int UserID { get; set; }
        public string Token { get; set; }
        public int Format { get; set; }
        public int OutletID { get; set; }
        public int PartnerID { get; set; }
        public int LoginType { get; set; }
    }
    public class APIBillerRequest : APIRequest {
        public int OpTypeID { get; set; }
        public string BillerID { get; set; }
    }
    public class PartnerAPIRequest: APIRequest
    {
        public string SPKey { get; set; }
        public int t { get; set; }
    }
    public class ServiceResponse
    {
        public int Statuscode { get; set; }
        public string Msg { get; set; }
        public int Errorcode { get; set; }
        public object data { get; set; }
    }
    public class AllResourceModalForAPIUsers
    {
        public IEnumerable<BankMasterSuperClass> Banks { get; set; }
        public IEnumerable<StateMaster> States { get; set; }
        public IEnumerable<City> Cities { get; set; }
        public object KYCDocs { get; set; }

        public object OtherData { get; set; }
    }
    #region APIRequestOFTransaction
    public class RechargeAPIRequest : APIRequest
    {
        public string Account { get; set; }
        public decimal Amount { get; set; }
        public string SPKey { get; set; }
        public string APIRequestID { get; set; }
        public string Optional1 { get; set; }
        public string Optional2 { get; set; }
        public string Optional3 { get; set; }
        public string Optional4 { get; set; }
        public string RefID { get; set; }
        public string GEOCode { get; set; }
        public string CustomerNumber { get; set; }
        public string Pincode { get; set; }
        public string SecurityKey { get; set; }
        public int FetchBillID { get; set; }
        public bool IsReal { get; set; }
        
    }
    public class _RechargeAPIRequest : RechargeAPIRequest
    {
        public string IPAddress { get; set; }
        public int RequestMode { get; set; }
        public int OID { get; set; }
        public string IMEI { get; set; }
        public int PromoCodeID { get; set; }
        public int CCFAmount { get; set; }
        public string PaymentMode { get; set; }
    }
    public class StatusAPIRequest : APIRequest
    {
        public string AgentID { get; set; }
        public string RPID { get; set; }
        public string Optional1 { get; set; }
    }
    public class RefundAPIRequest : APIRequest
    {
        public string RPID { get; set; }
        public string OTP { get; set; }
        public bool IsResend { get; set; }
    }
    public class _StatusAPIRequest : StatusAPIRequest
    {
        public string IPAddress { get; set; }
        public int RequestMode { get; set; }
        public int LoginID { get; set; }
        public bool IsPageCall { get; set; }
    }

    public class RefundRequest : APIRequest
    {
        public int TID { get; set; }
        public string RPID { get; set; }
        public bool IsResend { get; set; }
        public int RequestMode { get; set; }
        public string OTP { get; set; }
    }
    public class RefundRequestReq : CommonReq
    {
        public RefundRequest refundRequest { get; set; }
    }
    public class _RefundRequest : RefundRequest
    {
        public string IPAddress { get; set; }
    }
    public class WTRRequest : RefundRequest
    {
        public string RightAccount { get; set; }
    }
    #endregion

    #region APIRequestForOutlet
    public class APIRequestOutlet : APIRequest
    {
        public string SPkey { get; set; }
        public OutletRequest data { get; set; }
        public OutletKYCDoc kycDoc { get; set; }
    }
    public class OutletRequest
    {
        public int OutletID { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string Company { get; set; }
        public string MobileNo { get; set; }
        public string EmailID { get; set; }
        public string Pincode { get; set; }
        public string Address { get; set; }
        public string PAN { get; set; }
        public string AADHAR { get; set; }
        public string OType { get; set; }
        public string OTP { get; set; }
        public string PidData { get; set; }
        public int OTPRefID { get; set; }
        public int BioAuthType { get; set; }
        public int CityID { get; set; }
        public int StateID { get; set; }
        public string DOB { get; set; }
        public string ShopType { get; set; }
        public string Qualification { get; set; }
        public string Poupulation { get; set; }
        public string LocationType { get; set; }
        public string Landmark { get; set; }
        public string AlternateMobile { get; set; }
        public string Lattitude { get; set; }
        public string Longitude { get; set; }
        public string BankName { get; set; }
        public string IFSC { get; set; }
        public string BranchName { get; set; }
        public string AccountNumber { get; set; }
        public string AccountHolder { get; set; }
    }
    public class OutletKYCDoc
    {
        public string PAN { get; set; }
        public string AADHAR { get; set; }
        public string PHOTO { get; set; }
        public string GSTRegistration { get; set; }
        public string BusinessAddressProof { get; set; }
        public string CancelledCheque { get; set; }
        public string ServiceAggreement { get; set; }
        public string PASSBOOK { get; set; }
        public string VoterID { get; set; }
        public string DrivingLicense { get; set; }
        public string ShopImage { get; set; }
    }
    public class APIUserOutletData
    {
        public int OutletID { get; set; }
        public int KYCStatus { get; set; }
        public string KYCDescription { get; set; }
    }
    #endregion

    #region APIRequestForMoneyTransfer
    public class DMTAPIRequest : APIRequest
    {
        public string SenderMobile { get; set; }
        public string ReferenceID { get; set; }
        public string OTP { get; set; }
        public string SPKey { get; set; }
        public string APIRequestID { get; set; }
        
    }
    public class CreateSednerReq : DMTAPIRequest
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Pincode { get; set; }
        public string Address { get; set; }
        public string DOB { get; set; }
    }
    public class CreateBeneReq : DMTAPIRequest
    {
        public string BeneName { get; set; }
        public string BeneMobile { get; set; }
        public string BeneAccountNumber { get; set; }
        public string BankName { get; set; }
        public string IFSC { get; set; }
        public int BankID { get; set; }
        public int TransMode { get; set; }//1 - IMPS, 2- NEFT
    }
    public class DMTTransactionReq : CreateBeneReq
    {
        public string BeneID { get; set; }
        public int Amount { get; set; }
    }
    public class PayoutTransactionRequest : APIRequest
    {
        public XPressServiceProcRequest PayoutRequest { get; set; }
    }
    public class DeleteBeneReq : DMTAPIRequest
    {
        public string BeneID { get; set; }
    }
    public class DMTAPIResponse
    {
        public int Statuscode { get; set; }
        public string Message { get; set; }
        public int ErrorCode { get; set; }

    }
    public class DMTTransactionResponse : DMTAPIResponse
    {
        public int Status { get; set; }
        public string BeneName { get; set; }
        public string RPID { get; set; }
        public string LiveID { get; set; }
    }
    public class SenderLoginResponse : DMTAPIResponse
    {
        public string SenderName { get; set; }
        public string SenderMobile { get; set; }
        public decimal AvailbleLimit { get; set; }
        public decimal TotalLimit { get; set; }
        public int KYCStatus { get; set; }
        public string ReferenceID { get; set; }
        public bool IsOTPRequired { get; set; }
        public bool IsSenderNotExists { get; set; }
        public bool IsEKYCAvailable { get; set; }
        public bool IsActive { get; set; }
    }
    public class SenderCreateResponse : DMTAPIResponse
    {
        public string ReferenceID { get; set; }
        public string BeneID { get; set; }
        public bool IsOTPRequired { get; set; }
        public bool IsOTPResendAvailble { get; set; }
    }
    public class BeneResponse : DMTAPIResponse
    {
        public IEnumerable<APIBeneficary> Beneficiaries { get; set; }
    }
    public class APIDMTBeneficiaryResponse : DMTAPIResponse
    {
        public IEnumerable<MBeneDetail> Beneficiaries { get; set; }
    }
    public class APIBeneficary
    {
        public string BeneName { get; set; }
        public string BeneMobile { get; set; }
        public string BeneAccountNumber { get; set; }
        public string BankName { get; set; }
        public string IFSC { get; set; }
        public int BankID { get; set; }
        public string BeneID { get; set; }
        public bool IsVerified { get; set; }
    }
    public class DMRStatusReq
    {
        public string TransactionID { get; set; }
        public string VendorID { get; set; }
        public string TransDate { get; set; }
        public string APICode { get; set; }
        public int APIID { get; set; }
        public string CSTATUS { get; set; }
    }
    #endregion

    public class ValidataRechargeApirequest
    {
        private string _sPKey;
        public int LoginID { get; set; }
        public string Token { get; set; }
        public string IPAddress { get; set; }
        public string SPKey { get => _sPKey ?? "SPKEY"; set => _sPKey = value; }
        public int OID { get; set; }
    }
    public class ValidataRechargeApiResp
    {
        public int Statuscode { get; set; }
        public string Msg { get; set; }
        public int OID { get; set; }
        public int OpType { get; set; }
        public int CircleValidationType { get; set; }
        public int LookupAPIID { get; set; }
        public string LookupReqID { get; set; }
        public string Operator { get; set; }
        public bool IsBBPS { get; set; }
        public bool IsBilling { get; set; }
        public string SPKey { get; set; }
        public string ErrorCode { get; set; }
        public string MobileU { get; set; }
        public string DBGeoCode { get; set; }
        public int OpGroupID { get; set; }
        public string SCode { get; set; }
        public List<OperatorParams> OpParams { get; set; }
    }
}
