using Newtonsoft.Json;
using RoundpayFinTech.Models;
using System.Collections.Generic;
namespace RoundpayFinTech
{
    public class SprintOnboardJWTDecodeModel : JWTTokenModel
    {
        public SprintOnboardCallbackData payload { get; set; }
    }
    public class SprintOnboardCallbackData
    {
        public string refno { get; set; }
        public string txnid { get; set; }
        public string status { get; set; }
        public string statusbank1 { get; set; }
        public string mobile { get; set; }
        public string partnerid { get; set; }
        public string merchantcode { get; set; }
        public string is_icici_kyc { get; set; }
        public SprintonboardBank bank { get; set; }
    }
    public class SprintonboardBank
    {
        [JsonProperty("Bank-1")]
        public int Bank1 { get; set; }
        [JsonProperty("Bank-2")]
        public int Bank2 { get; set; }
        [JsonProperty("Bank-3")]
        public int Bank3 { get; set; }
    }
    public class SpringOnboardGetOnboardURLModel
    {
        public bool status { get; set; }
        public int response_code { get; set; }
        public string redirecturl { get; set; }
        public int onboard_pending { get; set; }
        public string message { get; set; }
    }
    public class SPRINTCommonResponseModel
    {
        public bool status { get; set; }
        public int? response_code { get; set; }
        public string message { get; set; }
        public object request { get; set; }
    }
    public class SPRINTAEPSTransactionResponse : SPRINTCommonResponseModel
    {
        public string ackno { get; set; }
        public decimal amount { get; set; }
        public string balanceamount { get; set; }
        public string bankrrn { get; set; }
        public string bankiin { get; set; }
        public string errorcode { get; set; }
        public string clientrefno { get; set; }
        public string txnstatus { get; set; }
        public List<SprintMinistatementModel> ministatement { get; set; }
    }
    public class SprintMinistatementModel
    {
        public string date { get; set; }
        public string txnType { get; set; }
        public string amount { get; set; }
        public string narration { get; set; }
    }
    public class SprintJsonSettings
    {
        public string AEPSBaseURL { get; set; }
        public string OnboardURL { get; set; }
        public string FetchBillURL { get; set; }
        public string PayBillURL { get; set; }
        public string StatusCheckURL { get; set; }
        public string DMTURL { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string PartnerID { get; set; }
        public string JWTKey { get; set; }
        public string Authorisedkey { get; set; }
        public string IV { get; set; }
        public string Key { get; set; }
    }
    public class SprintFetchBill
    {
        public string @operator { get; set; }
        public string canumber { get; set; }
        public string mode { get; set; }
        public string ad1 { get; set; }
        public string ad2 { get; set; }
        public string ad3 { get; set; }
        public string ad4 { get; set; }
    }
    public class SprintFetchBillResp
    {
        public int response_code { get; set; }
        public bool status { get; set; }
        public string amount { get; set; }
        public string name { get; set; }
        public string duedate { get; set; }
        public SPBillFetch bill_fetch { get; set; }
        public string ad2 { get; set; }
        public string ad3 { get; set; }
        public string message { get; set; }
    }
    public class SPBillFetch
    {
        public string name { get; set; }
        public string ad2 { get; set; }
        public string ad3 { get; set; }
        public string billAmount { get; set; }
        public string billnetamount { get; set; }
        public string billdate { get; set; }
        public string dueDate { get; set; }
        public bool acceptPayment { get; set; }
        public bool acceptPartPay { get; set; }
        public string cellNumber { get; set; }
        public string userName { get; set; }
    }
    public class SPPAYBillReq
    {
        public string @operator { get; set; }
        public string canumber { get; set; }
        public string amount { get; set; }
        public string referenceid { get; set; }
        public string latitude { get; set; }
        public string longitude { get; set; }
        public string mode { get; set; }
        public SPBillPay bill_fetch { get; set; }
        public string ad1 { get; set; }
        public string ad2 { get; set; }
        public string ad3 { get; set; }
        public string ad4 { get; set; }
    }
    public class SPBillPay
    {
        public string billAmount { get; set; }
        public string billnetamount { get; set; }
        public string billdate { get; set; }
        public string dueDate { get; set; }
        public bool acceptPayment { get; set; }
        public bool acceptPartPay { get; set; }
        public string cellNumber { get; set; }
        public string userName { get; set; }
    }
    public class SPDMTErrorCodes
    {
        public const int RespZero = 0;
        public const int RespOne = 1;
        public const int RespThree = 3;
        public const int MsgTxnStsFour = 4;
        public const string RMTNtReg = "Remitter not registered OTP sent for new registration.";
        public const string RMTFoundSccs = "Remitter details fetch successfully.";
        public const string MsgSAS = "Remitter Successfully Registered";
        public const string GeBenSuccess = "Beneficiary fetched successfully.";
        public const string BeneAddedSuc = "Receiver account successfully added.";
        public const string BeneRemovedSuc = "Beneficiary record deleted successfully.";
        public const string MsgTanInProc = "Your transaction is in process.";
        public const string RefundOTPSuc = "Refund Otp Successfully Sent.";
        public const string MsgInvalidOTP = "Invalid OTP";
        public const string MsgTranSucc = "Transaction Successful";
        public const string MsgInvalidAcc = "Fund transfer is failed due to invalid beneficiary account number";
        public const string NoRecFound = "No record found.";
        public const string TranSuccRef = "Transaction Successfully Refunded";
    }
    public class SPDMTReq
    {
        public string mobile { get; set; }
        public string bank3_flag { get; set; }
        public string firstname { get; set; }
        public string lastname { get; set; }
        public string address { get; set; }
        public string otp { get; set; }
        public string pincode { get; set; }
        public string stateresp { get; set; }
        public string dob { get; set; }
        public string gst_state { get; set; }
        public string benename { get; set; }
        public int bankid { get; set; }
        public string accno { get; set; }
        public string ifsccode { get; set; }
        public string verified { get; set; }
        public string pipe { get; set; }
        public string txntype { get; set; }
        public int bene_id { get; set; }
        public int referenceid { get; set; }
        public int amount { get; set; }
        public int ackno { get; set; }
    }
    public class SPDMTResp
    {
        public int response_code { get; set; }
        public bool status { get; set; }
        public string message { get; set; }
        public string stateresp { get; set; }
        public SPDMTData data { get; set; }
        public string utr { get; set; }
        public int ackno { get; set; }
        public int txn_status { get; set; }
        public string benename { get; set; }
    }
    public class SPDMTData
    {
        public object fname { get; set; }
        public object lname { get; set; }
        public object mobile { get; set; }
        public int status { get; set; }
        public int bank3_limit { get; set; }
        public int bank2_limit { get; set; }
        public int bank1_limit { get; set; }
    }
    public class SPDMTBeneResp
    {
        public int response_code { get; set; }
        public bool status { get; set; }
        public string message { get; set; }
        public string stateresp { get; set; }
        public List<SPBeneDetails> data { get; set; }
    }
    public class SPBeneDetails
    {
        public string bene_id { get; set; }
        public string bankid { get; set; }
        public string bankname { get; set; }
        public string name { get; set; }
        public string accno { get; set; }
        public string ifsc { get; set; }
        public string verified { get; set; }
        public string banktype { get; set; }
        public bool paytm { get; set; }
    }
    public class SPDMTVAReq
    {
        public string mobile { get; set; }
        public string benename { get; set; }
        public int bankid { get; set; }
        public string accno { get; set; }
        public string gst_state { get; set; }
        public string dob { get; set; }
        public string address { get; set; }
        public string pincode { get; set; }
        public int referenceid { get; set; }
        public string ifsccode { get; set; }
        public string bene_id { get; set; }
    }
}
