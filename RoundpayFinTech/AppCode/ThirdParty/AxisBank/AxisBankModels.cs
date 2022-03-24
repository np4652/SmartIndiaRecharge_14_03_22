using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.ThirdParty.AxisBank
{
    public class AxisBankSetting
    {
        public string axischannelid { get; set; }
        public string axisclientid { get; set; }
        public string axisclientsecret { get; set; }
        public string axisencryptionkey { get; set; }
        public string axissaltkey { get; set; }
        public string axischannelpassword { get; set; }
        public string axisbodychannelid { get; set; }
        public string BillerListURL { get; set; }
        public string BillerFieldsURL { get; set; }
        public string BillerDetailsURL { get; set; }
        public string BillFetchURL { get; set; }
        public string FetchedBillURL { get; set; }
        public string PaymentURL { get; set; }
        public string PaymentStatusURL { get; set; }
        public string RaiseComplainURL { get; set; }
        public string ComplainStatusURL { get; set; }
        public string accountNumber { get; set; }
        public string accountHolderName { get; set; }
    }

    public class AxisBankResp
    {
        public string UTR { get; set; }
        public string REMITTERACCOUNTNUMBER { get; set; }
        public string DUMMY1 { get; set; }
        public string REMITTERACCOUNTNAME { get; set; }
        public string AMOUNT { get; set; }

        public string CORPCODE { get; set; }
    }

    public class AxisBankBillerListObject
    {
        public string statusCode { get; set; }
        public string statusMessage { get; set; }
        public string traceId { get; set; }
        public List<AxisBankBillerListModel> data { get; set; }
    }
    public class AxisBankBillerListModel
    {
        public string billerId { get; set; }
        public string exactness { get; set; }
        public string name { get; set; }
        public string type { get; set; }
    }
    public class AxisBankBillerDetailsModel
    {
        public string dataType { get; set; }
    }
    public class AxisBankBillerFieldsResponse
    {
        public string statusCode { get; set; }
        public string statusMessage { get; set; }
        public string traceId { get; set; }
        public List<AxisBankBillerFieldsModel> data { get; set; }
    }
    public class AxisBankBillerFieldsModel
    {
        public string dataType { get; set; }
        public string internalName { get; set; }
        public bool isMandatory { get; set; }
        public string maxLength { get; set; }
        public string minLength { get; set; }
        public string name { get; set; }
        public object regex { get; set; }
        public object values { get; set; }
        public object variable { get; set; }
    }
    public class AxisBankBillerDetailResponse
    {
        public AxisBankBillerDetailData data { get; set; }
        public string statusCode { get; set; }
        public string statusMessage { get; set; }
        public string traceId { get; set; }
    }
    public class AxisBankBillerDetailData
    {
        public string blrAcceptsAdhoc { get; set; }
        public List<AxisBankBillerAddInfo> blrAdditionalInfo { get; set; }
        public string blrAliasName { get; set; }
        public string blrCategoryName { get; set; }
        public string blrCoverage { get; set; }
        public List<AxisBankBillerCustParams> blrCustomerParams { get; set; }
        public string blrDescription { get; set; }
        public int blrEffctvFrom { get; set; }
        public int blrEffctvTo { get; set; }
        public string blrPmtAmtExactness { get; set; }
        public string blrId { get; set; }
        public string blrMode { get; set; }
        public string blrName { get; set; }
        public string blrOwnership { get; set; }
        public List<AxisBankBillerPaymentChannel> blrPaymentChannels { get; set; }
        public List<AxisBankBillerOperatorDictionary> blrOperatorDictonary { get; set; }
        public List<AxisBankBillerPaymentModes> blrPaymentModes { get; set; }
        public AxisBankBillerResponseParam blrResponseParams { get; set; }        
        public string fetchRequirement { get; set; }
        public object supportValidationApi { get; set; }
    }
    public class AxisBankBillerAddInfo
    {
        public string dataType { get; set; }
        public bool optional { get; set; }
        public string paramName { get; set; }
    }
    public class AxisBankBillerCustParams
    {
        public string dataType { get; set; }
        public string maxLength { get; set; }
        public string maxValue { get; set; }
        public string minLength { get; set; }
        public bool optional { get; set; }
        public string paramName { get; set; }
        public object regex { get; set; }
        public object values { get; set; }
    }
    public class AxisBankBillerPaymentChannel
    {
        public long? maxLimit { get; set; }
        public int? minLimit { get; set; }
        public string paymentChannel { get; set; }


    }

    public class AxisBankBillerOperatorDictionary
    {
        
        public int? ParamID { get; set; }
        public int? Ind { get; set; }
        public int? OID { get; set; }
        public string DropDownValue { get; set; }

    }
    public class AxisBankBillerPaymentModes
    {
        public long? maxLimit { get; set; }
        public int minLimit { get; set; }
        public string paymentMode { get; set; }
    }
    public class AxisBankBillerAmtOption
    {
        public List<string> amountBreakupSet { get; set; }
    }
    public class AxisBankBillerResponseParam
    {
        public List<AxisBankBillerAmtOption> amountOptions { get; set; }
    }
    public class AxisBankBillFetchReqModel
    {
        public AxisBankAgent agent { get; set; }
        public string billerId { get; set; }
        public string mobileNumber { get; set; }
        public string categoryCode { get; set; }
        public List<AxisBankNameValue> customerParams { get; set; }
    }
    public class AxisBankAgent
    {
        public string app { get; set; }
        public string channel { get; set; }
        public string geocode { get; set; }
        public string id { get; set; }
        public string ifsc { get; set; }
        public string imei { get; set; }
        public string ip { get; set; }
        public string mac { get; set; }
        public string mobile { get; set; }
        public string os { get; set; }
        public string postalCode { get; set; }
        public string terminalId { get; set; }
    }
    public class AxisBankNameValue
    {
        public string name { get; set; }
        public string value { get; set; }
    }

    public class AxisBankBillFetchPreResponse
    {
        public AxisBankBillFetchPreData data { get; set; }
        public string statusCode { get; set; }
        public string statusMessage { get; set; }
        public string traceId { get; set; }
    }
    public class AxisBankBillFetchPreData
    {
        public string billFetchId { get; set; }
        public string context { get; set; }
    }
    public class AxisBankBillFetchResponse
    {
        public string statusCode { get; set; }
        public string statusMessage { get; set; }
        public string traceId { get; set; }
        public AxisBankBillFetchData data { get; set; }
    }
    public class AxisBankBillFetchData
    {
        public string context { get; set; }
        public string fetchAPIStatus { get; set; }
        public AxisBankDataBill bill { get; set; }
        public List<AxisBankPaymentModes> paymentLimits { get; set; }
    }
    public class AxisBankDataBill
    {
        public string amount { get; set; }
        public string billDate { get; set; }
        public string billNumber { get; set; }
        public string billPeriod { get; set; }
        public string customerName { get; set; }
        public string customerRefId { get; set; }
        public string dueDate { get; set; }
        public List<AxisBankAdditionalInfo> additionalInfo { get; set; }
    }
    public class AxisBankAdditionalInfo
    {
        public string name { get; set; }
        public string value { get; set; }
    }
    public class AxisBankPaymentModes
    {
        public long maxLimit { get; set; }
        public int minLimit { get; set; }
        public string paymentMode { get; set; }
    }
    public class AxisBankMakePaymentRequest
    {
        public string amount { get; set; }
        public string txnMode { get; set; }
        public string referenceId { get; set; }
        public string context { get; set; }
        public AxisBankRemitter remittanceDetails { get; set; }
    }
    public class AxisBankRemitter
    {
        public string accountNumber { get; set; }
        public string accountHolderName { get; set; }
    }
    public class AxisBankPaymentResponse
    {
        public string statusCode { get; set; }
        public string statusMessage { get; set; }
        public string traceId { get; set; }
        public AxisBankPaymentData data { get; set; }
    }
    public class AxisBankPaymentData
    {
        public string referenceId { get; set; }
        public string bbpsTxnId { get; set; }
        public string context { get; set; }
        public string sourceRefNum { get; set; }
    }
    public class AxisBankComplainStatusModel
    {
        public string statusCode { get; set; }
        public string statusMessage { get; set; }
        public string traceId { get; set; }
        public AxisBankComplainData data { get; set; }
    }
    public class AxisBankComplainData
    {
        public string assignedTo { get; set; }
        public string complaintID { get; set; }
        public string isComplaintOpen { get; set; }
        public string npciMessageId { get; set; }
        public string npciRefId { get; set; }
        public string sequenceNumber { get; set; }
        public string status { get; set; }
        public string transactionResponseCode { get; set; }
        public string transactionResponseReason { get; set; }
    }
}
