using Fintech.AppCode.Model;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Model.ProcModel
{
    public class DMRTransactionRequest
    {
        public int UserID { get; set; }
        public int OutletID { get; set; }
        public string AccountNo { get; set; }
        public decimal AmountR { get; set; }
        public string APIRequestID { get; set; }
        public int RequestModeID { get; set; }
        public string RequestIP { get; set; }
        public string BeneID { get; set; }
        public string SenderNo { get; set; }
        public int APIID { get; set; }
        public int OPType { get; set; }
        public string IFSC { get; set; }
        public string TransMode { get; set; }
        public string GroupID { get; set; }
        public string IMEI { get; set; }
        public string Bank { get; set; }
        public string BeneName { get; set; }
        public string SecureKey { get; set; }
        public int BankID { get; set; }
        public int OID { get; set; }
        public int APISenderLimit { get; set; }
        public int AmountWithoutSplit { get; set; }
        public int CBA { get; set; }
        public int TransactedAmount { get; set; }
        public int AccountTableID { get; set; }
        public bool IsInternal { get; set; }
    }
    public class DMRTransactionResponse
    {
        public string VendorID2 { get; set; }
        public int Statuscode { get; set; }
        public string Msg { get; set; }
        public int TID { get; set; }
        public string LiveID { get; set; }
        public string TransactionID { get; set; }
        public decimal Balance { get; set; }
        public string VendorID { get; set; }
        public string Status { get; set; }
        public string Request { get; set; }
        public string Response { get; set; }
        public string BeneName { get; set; }
        public string SenderName { get; set; }
        public string GroupID { get; set; }
        public int ErrorCode { get; set; }
        public string BankName { get; set; }
        public string BankCode { get; set; }
        public Int16 IsIMPS { get; set; }
        public Int16 IsNEFT { get; set; }
        public bool IsInternalSender { get; set; }
        public string IFSC { get; set; }
        public string EKOBankID { get; set; }
        public string PanNo { get; set; }
        public string Pincode { get; set; }
        public string LatLong { get; set; }
        public string APIGroupCode { get; set; }
        public string AccountHolder { get; set; }
        public string Bank { get; set; }
        public string BrandName { get; set; }
        public string TransMode { get; set; }
        public bool IsRefundAvailable { get; set; }
        public string SenderMobileNo { get; set; }
        public string RStsType { get; set; }
        public string RDMTTxnID { get; set; }

    }
    public class DMTReq
    {
        public int ChanelType { get; set; }
        public string SenderNO { get; set; }
        public int BeneAPIID { get; set; }

        public bool IsValidate { get; set; }
        public int UserID { get; set; }
        public string UserMobileNo { get; set; }
        public string EmailID { get; set; }
        public int OutletID { get; set; }
        public int StateID { get; set; }
        public int RequestMode { get; set; }
        public int LT { get; set; }
        public int ApiID { get; set; }
        public string TID { get; set; }
        public int BankID { get; set; }
        public string ReffID { get; set; }
        public string APIOutletID { get; set; }
        public string Domain { get; set; }
    }
    public class DMTReqRes
    {
        public string TID { get; set; }
        public int APIID { get; set; }
        public string Request { get; set; }
        public string Response { get; set; }
        public string SenderNo { get; set; }
        public int RequestModeID { get; set; }
        public string Method { get; set; }
        public int UserID { get; set; }
    }
    public class ChargeAmount : ResponseStatus
    {
        public decimal Charged { get; set; }
    }
    public class _SenderRequest
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string LastName { get; set; }
        public string MobileNo { get; set; }
        public string Pincode { get; set; }
        public string Address { get; set; }
        public string OTP { get; set; }
        public string MahagramStateCode { get; set; }
        public string Dob { get; set; }
        public string NameOnKYC { get; set; }
        public string AadharNo { get; set; }
        public string PANNo { get; set; }
        public IFormFile AadharFront { get; set; }
        public IFormFile AadharBack { get; set; }
        public IFormFile SenderPhoto { get; set; }
        public IFormFile PAN { get; set; }
    }
    public class SenderRequest : _SenderRequest
    {
        public int OID { get; set; }
        public int UserID { get; set; }
        public string Area { get; set; }
        public string City { get; set; }
        public string Districtname { get; set; }
        public string Statename { get; set; }
        public int StateID { get; set; }
        public string VerifySta { get; set; }
        public string ReffID { get; set; }
        public string RequestNo { get; set; }
        public int _VerifyStatus { get; set; }
        public bool IsNotCheckLimit { get; set; }
        public int SelfRefID { get; set; }
        public int PinCode { get; set; }
        public string PidData { get; set; }
        public string VID { get; set; }
        public string UIDToken { get; set; }
    }
    public class SenderInfo : SenderRequest
    {
        public int Statuscode { get; set; }
        public string Msg { get; set; }
        public int WID { get; set; }

    }
    public class SenderLimitModel
    {
        public decimal LimitUsed { get; set; }
        public decimal SenderLimit { get; set; }
        public string SenderName { get; set; }
    }
    public class CreateSen
    {
        public SenderRequest senderRequest { get; set; }
        public DMTReq dMTReq { get; set; }
        public string OTP { get; set; }
    }
    public class BeneDetail
    {
        public string MobileNo { get; set; }
        public string BeneName { get; set; }
        public string IFSC { get; set; }
        public string AccountNo { get; set; }
        public string BankName { get; set; }
        public int BankID { get; set; }
        public string BeneID { get; set; }
        public int TransMode { get; set; }
        public bool IsVerified { get; set; } = true;
    }
    public class AddBeni : BeneDetail
    {
        public string SenderMobileNo { get; set; }
        public string SID { get; set; }
        public int OID { get; set; }
        public int ttype { get; set; }
    }
    public class ReqSendMoney
    {
        public string BeneID { get; set; }
        public string MobileNo { get; set; }

        public string IFSC { get; set; }
        public string AccountNo { get; set; }
        public int Amount { get; set; }
        public bool Channel { get; set; }
        public string Bank { get; set; }
        public string BeneName { get; set; }
        public string SecKey { get; set; }
        public string BeneMobile { get; set; }
        public int BankID { get; set; }
        public int o { get; set; }
        public string APIRequestID { get; set; }
        public string RefferenceID { get; set; }
    }
    public class BeniRespones : ResponseStatus
    {
        public List<AddBeni> addBeni { get; set; }
    }
    public class DMRReceiptDetail
    {
        public decimal RequestedAmount { get; set; }
        public string LiveID { get; set; }
        public string Status { get; set; }
        public int Statuscode { get; set; }
        public string TransactionID { get; set; }
        public int TID { get; set; }
    }
    public class TransactionDetail
    {
        public string CompanyEmail { get; set; }
        public int WID { get; set; }
        public string SupportEmail { get; set; }
        public string PhoneNoSupport { get; set; }
        public string MobileSupport { get; set; }
        public string Email { get; set; }
        public string IFSC { get; set; }
        public string Channel { get; set; }
        public string Account { get; set; }
        public string BankName { get; set; }
        public string SenderNo { get; set; }
        public string BeneName { get; set; }
        public int UserID { get; set; }
        public string MobileNo { get; set; }
        public string Name { get; set; }
        public int OID { get; set; }
        public string OPName { get; set; }
        public string DisplayAccount { get; set; }
        public string Address { get; set; }
        public string PinCode { get; set; }
        public string CompanyAddress { get; set; }
        public bool IsInvoice { get; set; }
        public bool IsBBPS { get; set; }
        public DateTime EntryDate { get; set; }
        public List<DMRReceiptDetail> lists { get; set; }
        public decimal convenientFee { get; set; }
        public string SenderName { get; set; }
        public string TransactionID { get; set; }
        public string LiveID { get; set; }
        public int TID { get; set; }
        public int Statuscode { get; set; }
        public bool IsError { get; set; }
        public string Status { get; set; }
        public decimal RequestedAmount { get; set; }
        public string CompanyName { get; set; }
        public string O15 { get; set; }
        public string O16 { get; set; }
        public string O17 { get; set; }
        public string CustomerMobile { get; set; }
        public string BillerID { get; set; }
        public string AccountNoKey { get; set; }
        public string CompanyPhone { get; set; }
        public string CompanyMobile { get; set; }
    }
}
