using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;

namespace RoundpayFinTech.AppCode.Model
{

    public class BA_ConstValue
    {
        public const string SENDERDETAILS = "SenderDetails";
        public const string SENDERREGISTER = "SenderRegister";
        public const string VERIFYSENDER = "VerifySender";
        public const string GETSENDER = "GetRecipient";
        public const string DELRECIPIENT = "DelRecipient";
        public const string ALLRECIPIENT = "AllRecipient";
        public const string REGISTERRECIPIENT = "RegRecipient";
        public const string FUNDTRANSFER = "FundTransfer";
        public const string BANKLIST = "BankList";
        public const string IMPS = "IMPS";
        public const string NEFT = "NEFT";
        public const string AGT = "AGT";
        public const string VERIFYACCOUNT = "VerifyBankAcct";
        public const string REFUND = "TxnRefund";
        public const string REFUNDOTP = "VerifyRefundOtp";
        public const string TransactionStatus = "MultiTxnStatus";

    }
    public static class EXACTNESS {
        public const int Exact = 1;
        public const int ExactAndAbove = 2;
        public const int ExactAndBelow = 3;
        public const int All = 4;
    }
    public class BAAPISetting
    {
        public string accessCode { get; set; }
        public string agentId { get; set; }
        public string DMTServiceURL { get; set; }
        public string DMTTransactionURL { get; set; }
        public string BillerInfoURL { get; set; }
        public string BillFetchURL { get; set; }
        public string BillPaymentURL { get; set; }
        public string StatusCheckURL { get; set; }
        public string BillValidationURL { get; set; }
        public string ComplaintRegURL { get; set; }
        public string ComplaintTrackURL { get; set; }
        public string DepositEnquiryURL { get; set; }
        public string instituteId { get; set; }
        public string Key { get; set; }
        public string ver { get; set; }
    }
    public class dmtServiceRequest
    {
        public string requestType { get; set; }
        public string agentId { get; set; }
        public string initChannel { get; set; }
        public string senderMobileNumber { get; set; }
        public string senderMobileNo { get; set; }
        public string txnType { get; set; }
        public string txnId { get; set; }
        public string uniqueRefId { get; set; }
        public string senderName { get; set; }
        public string senderPin { get; set; }
        public string otp { get; set; }
        public string additionalRegData { get; set; }
        public string recipientId { get; set; }
        public string txnAmount { get; set; }
        public string recipientName { get; set; }
        public string recipientMobileNumber { get; set; }
        public string bankCode { get; set; }
        public string bankAccountNumber { get; set; }
        public string ifsc { get; set; }
        public string convFee { get; set; }

    }
    public class dmtServiceResponse
    {
        public string additionalLimitAvailable { get; set; }
        public decimal availableLimit { get; set; }
        public string mobileVerified { get; set; }
        public string respDesc { get; set; }
        public string responseCode { get; set; }
        public string responseReason { get; set; }
        public string senderCity { get; set; }
        public string senderMobileNumber { get; set; }
        public string senderName { get; set; }
        public decimal totalLimit { get; set; }
        public decimal usedLimit { get; set; }
        public string additionalRegData { get; set; }
        public List<dmtRecipient> recipientList { get; set; }
        public BAErrorInfo errorInfo { get; set; }
        public string impsName { get; set; }
    }
    public class BAErrorInfo
    {
        public BAError error { get; set; }
    }
    public class BAError
    {
        public string errorCode { get; set; }
        public string errorMessage { get; set; }
    }
    public class dmtRecipient
    {
        public string bankAccountNumber { get; set; }
        public string bankCode { get; set; }
        public string bankName { get; set; }
        public string ifsc { get; set; }
        public string isVerified { get; set; }
        public int recipientId { get; set; }
        public string recipientName { get; set; }
        public string recipientStatus { get; set; }
        public string verifiedName { get; set; }
    }
    public class bankInfoArray
    {
        public string accountVerificationAllowed { get; set; }
        public string bankCode { get; set; }
        public string bankName { get; set; }
        public string impsAllowed { get; set; }
        public string neftAllowed { get; set; }
    }
    public class dmtTransactionResponse
    {
        public string respDesc { get; set; }
        public string responseCode { get; set; }
        public string responseReason { get; set; }
        public string senderMobileNo { get; set; }
        public string uniqueRefId { get; set; }
        public string txnId { get; set; }
        public string refundTxnId { get; set; }
        public List<fundDetail> fundTransferDetails { get; set; }
    }
    public class fundDetail
    {
        public string uniqueRefId { get; set; }
        public string bankTxnId { get; set; }
        public string custConvFee { get; set; }
        public string DmtTxnId { get; set; }
        public string impsName { get; set; }
        public string refId { get; set; }
        public string txnAmount { get; set; }
        public string txnStatus { get; set; }
    }

    #region BillAvenueBillPayementModels
    public class BABillValidationRequest
    {
        public string agentId { get; set; }
        public string billerId { get; set; }
        public List<input> inputParams { get; set; }
    }
    public class BABillValidationResponse
    {
        public string responseCode { get; set; }
        public string responseReason { get; set; }
        public string complianceCode { get; set; }
        public string complianceReason { get; set; }
        public string approvalRefNo { get; set; }
        public List<info> additionalInfo { get; set; }
        public BAErrorInfo errorInfo { get; set; }
    }
    public class BABillFetchRequest
    {
        public string agentId { get; set; }
        public string billerId { get; set; }
        public BAAgentDeviceInfo agentDeviceInfo { get; set; }
        public BACustomerInfo customerInfo { get; set; }
        public List<input> inputParams { get; set; }
    }
    public class input
    {
        public string paramName { get; set; }
        public string paramValue { get; set; }
    }
    public class BAAgentDeviceInfo
    {
        public string ip { get; set; }
        public string initChannel { get; set; }
        public string mac { get; set; }
        public string os { get; set; }
        public string imei { get; set; }
        public string app { get; set; }
    }
    public class BACustomerInfo
    {
        public string customerMobile { get; set; }
        public string customerEmail { get; set; }
        public string customerAdhaar { get; set; }
        public string customerPan { get; set; }
    }
    public class BABillFetchResponse
    {
        public string responseCode { get; set; }
        public List<input> inputParams { get; set; }
        public BABillerResponse billerResponse { get; set; }
        public List<info> additionalInfo { get; set; }
        public BAErrorInfo errorInfo { get; set; }
    }
    public class BABillerResponse
    {
        public List<option> amountOptions { get; set; }
        public string billAmount { get; set; }
        public string billDate { get; set; }
        public string billNumber { get; set; }
        public string billPeriod { get; set; }
        public string customerName { get; set; }
        public string dueDate { get; set; }
        public class option
        {
            public string amountName { get; set; }
            public string amountValue { get; set; }
        }

    }
    public class BABillPaymentRequest
    {
        public string agentId { get; set; }

        public bool billerAdhoc { get; set; }
        public BAAgentDeviceInfo agentDeviceInfo { get; set; }
        public BACustomerInfo customerInfo { get; set; }
        public string billerId { get; set; }
        public List<input> inputParams { get; set; }
        public BABillerResponse billerResponse { get; set; }
        public List<info> additionalInfo { get; set; }
        public BAAmountInfo amountInfo { get; set; }
        public BAPaymentMethod paymentMethod { get; set; }
        public List<info> paymentInfo { get; set; }
    }
    public class info
    {
        public string infoName { get; set; }
        public string infoValue { get; set; }
    }
    public class BAAmountInfo
    {
        public string amount { get; set; }
        public string currency { get; set; }
        public string custConvFee { get; set; }

        //public BAAmountTag amountTags { get; set; }
        public string amountTags { get; set; }

    }
    public class BAAmountTag
    {
        public string amountTag { get; set; }
        public string value { get; set; }
    }
    public class BAPaymentMethod
    {
        public string paymentMode { get; set; }
        public string quickPay { get; set; }
        public string splitPay { get; set; }
    }
    public class BABillerInfoRequest
    {
        public string billerId { get; set; }
    }
    public class BABillerInfoResponse
    {
        public string responseCode { get; set; }
        public BABiller biller { get; set; }
        public BAErrorInfo errorInfo { get; set; }
    }
    public class BABillerInfoResponseList
    {
        public string responseCode { get; set; }
        public List<biller> billerList { get; set; }
        public BAErrorInfo errorInfo { get; set; }
    }
    public class BABiller
    {
        public string billerDescription { get; set; }
        public string billerId { get; set; }
        public string billerName { get; set; }
        public string billerCategory { get; set; }
        public bool billerAdhoc { get; set; }
        public string billerCoverage { get; set; }
        public string billerFetchRequiremet { get; set; }
        public string billerPaymentExactness { get; set; }
        public string billerAmountOptions { get; set; }
        public string billerPaymentModes { get; set; }
        public string rechargeAmountInValidationRequest { get; set; }
        public string billerSupportBillValidation { get; set; }
        public List<paramInfo> billerInputParams { get; set; }
        public List<paymentChannelInfo> billerPaymentChannels { get; set; }
        public List<BillerOperatorDictionary> BillerOperatorDictionary { get; set; }
    }
    public class biller
    {
        public string billerId { get; set; }
        public string billerDescription { get; set; }
        public string billerName { get; set; }
        public string billerCategory { get; set; }
        public bool billerAdhoc { get; set; }
        public string billerCoverage { get; set; }
        public string billerFetchRequiremet { get; set; }
        public string billerPaymentExactness { get; set; }
        public string billerAmountOptions { get; set; }
        public string billerPaymentModes { get; set; }
        public string rechargeAmountInValidationRequest { get; set; }
        public string billerSupportBillValidation { get; set; }
        public List<paramInfo> billerInputParams = new List<paramInfo>();
        public List<paymentChannelInfo> billerPaymentChannels = new List<paymentChannelInfo>();
        public List<BillerOperatorDictionary> billerOperatorDictionary = new List<BillerOperatorDictionary>();
    }
    public class paramInfo
    {
        public string paramName { get; set; }
        public string dataType { get; set; }
        public string regEx { get; set; }
        public bool isOptional { get; set; }
        public int minLength { get; set; }
        public int maxLength { get; set; }

    }
    public class paymentChannelInfo
    {
        public string paymentChannelName { get; set; }public int minAmount { get; set; }
        public Int64 maxAmount { get; set; }
    }

    public class   BillerOperatorDictionary
    {
        public int ParamID { get; set; }
        public int Ind { get; set; }
        public int OID { get; set; }
        public string DropDownValue { get; set; }

    }


    #endregion
    public class ExtBillPayResponse
    {
        public string responseCode { get; set; }
        public string responseReason { get; set; }
        public string txnRefId { get; set; }
        public string txnRespType { get; set; }
        public List<input> inputParams { get; set; }
        public class input
        {
            public string paramName { get; set; }
            public string paramValue { get; set; }
        }
        public int CustConvFee { get; set; }
        public int RespAmount { get; set; }
        public string RespBillDate { get; set; }
        public string RespBillNumber { get; set; }
        public string RespBillPeriod { get; set; }
        public string RespCustomerName { get; set; }
        public string RespDueDate { get; set; }
        public BAErrorInfo errorInfo { get; set; }
    }
    public class BAComplaintRegistrationReq
    {
        public string complaintType { get; set; }
        public string participationType { get; set; }
        public string agentId { get; set; }
        public string txnRefId { get; set; }
        public string billerId { get; set; }
        public string complaintDesc { get; set; }
        public string servReason { get; set; }
        public string complaintDisposition { get; set; }
    }
    public class BAComplaintRegistrationResp
    {
        public string complaintAssigned { get; set; }
        public string complaintId { get; set; }
        public string responseCode { get; set; }
        public string responseReason { get; set; }
        public BAErrorInfo errorInfo { get; set; }
    }
    public class BAComplaintTrackingReq
    {
        public string complaintType { get; set; }
        public string complaintId { get; set; }
    }
    public class BAComplaintTrackingResp
    {
        public string complaintAssigned { get; set; }
        public string complaintId { get; set; }
        public string complaintStatus { get; set; }
        public string respCode { get; set; }
        public string respReason { get; set; }
        public BAErrorInfo errorInfo { get; set; }
    }
    public class BATransactionStatusReq
    {
        public string trackType { get; set; }
        public string trackValue { get; set; }
        //public string fromDate { get; set; }
        //public string toDate { get; set; }
    }
    public class BATransactionStatusResp
    {
        public string responseCode { get; set; }
        public string responseReason { get; set; }
        public List<BATransaction> txnList { get; set; }
    }
    public class BATransaction
    {
        public string agentId { get; set; }
        public string amount { get; set; }
        public string billerId { get; set; }
        public string txnDate { get; set; }
        public string txnReferenceId { get; set; }
        public string txnStatus { get; set; }
    }
}
