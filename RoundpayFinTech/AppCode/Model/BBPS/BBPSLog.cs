using RoundpayFinTech.AppCode.Model.ProcModel;
using System.Collections.Generic;
using System.Data;

namespace RoundpayFinTech.AppCode.Model.BBPS
{

    public class BBPSLog
    {
        public string BillNumber { get; set; }
        public string BillDate { get; set; }
        public string DueDate { get; set; }
        public decimal Amount { get; set; }
        public string CustomerName { get; set; }
        public string CustomerMobile { get; set; }
        public string AccountNumber { get; set; }
        public string OPID { get; set; }
        public string Authenticator3 { get; set; }
        public int UserID { get; set; }
        public int APIID { get; set; }
        public string RequestURL { get; set; }
        public string Request { get; set; }
        public string Response { get; set; }
        public string SessionNo { get; set; }
        public string Optional1 { get; set; }
        public string Optional2 { get; set; }
        public string Optional3 { get; set; }
        public string Optional4 { get; set; }
        public ApiOperatorOptionalMappingModel APIOptionalList { get; set; }
        public APIDetail aPIDetail { get; set; }
        public string Pincode { get; set; }
        public string PAN { get; set; }
        public string AadharNo { get; set; }
        public string GeoLocation { get; set; }
        public string CircleCode { get; set; }
        public string BillPeriod { get; set; }
        public BBPSAPIReqHelper APIReqHelper { get; set; }
        public BBPSLogReqHelper helper { get; set; }
        public string BillMonth { get; set; }
        public string APIContext { get; set; }
        public string OutletMobileNo { get; set; }
        public string IMEI { get; set; }
    }
    public class BBPSPaymentRequest
    {
        public decimal RequestedAmount { get; set; }
        public string PaymentMode { get; set; }
        public string AccountNo { get; set; }
        public string Optional1 { get; set; }
        public string Optional2 { get; set; }
        public string Optional3 { get; set; }
        public string Optional4 { get; set; }
        public string APIOutletID { get; set; }
        public bool billerAdhoc { get; set; }
        public int ExactNess { get; set; }
        public string IPAddress { get; set; }
        public string InitChanel { get; set; }
        public string IMEI { get; set; }
        public string MAC { get; set; }
        public string CustomerMobile { get; set; }
        public string BillerID { get; set; }
        public int TID { get; set; }
        public string TransactionID { get; set; }
        public string FetchBillResponse { get; set; }
        public string EarlyPaymentAmountKey { get; set; }
        public string LatePaymentAmountKey { get; set; }
        public string BillDate { get; set; }
        public string EarlyPaymentDate { get; set; }
        public string DueDate { get; set; }
        public List<OperatorParams> OpParams { get; set; }
        public List<TranAdditionalInfo> AInfos { get; set; }
        public int CCFAmount { get; set; }
        public string PaymentModeInAPI { get; set; }
        public string CaptureInfo { get; set; }
        public string UserMobileNo { get; set; }
        public string UserName { get; set; }
        public string UserEmailID { get; set; }
        public bool IsINT { get; set; }
        public bool IsMOB { get; set; }
        public bool IsQuickPay { get; set; }
        public bool IsValidation { get; set; }
        public string APIContext { get; set; }
        public string AccountHolder { get; set; }
        public string APIOpType { get; set; }
        public string AccountNoKey { get; set; }
        public string GeoLocation { get; set; }
        public string Pincode { get; set; }
        public string OutletName { get; set; }
        public string OutletMobile { get; set; }
        public string OutletEmail { get; set; }
    }
    public class BBPSAPIReqHelper
    {
        public string BillerID { get; set; }
        public string APIOpTypeID { get; set; }
        public string AccountNoKey { get; set; }
        public string RegxAccount { get; set; }
        public string IPAddress { get; set; }
        public string InitChanel { get; set; }
        public string MAC { get; set; }
        public string EarlyPaymentAmountKey { get; set; }
        public string LatePaymentAmountKey { get; set; }
        public string EarlyPaymentDateKey { get; set; }
        public string BillMonthKey { get; set; }
        public List<OperatorParams> OpParams { get; set; }
    }
    public class BBPSLogReqHelper
    {
        public DataTable tpBFAInfo { get; set; }
        public DataTable tpBFAmountOps { get; set; }
        public DataTable tpBFInputParam { get; set; }
        public string AmountName1 { get; set; }
        public string AmountValue1 { get; set; }
        public string AmountName2 { get; set; }
        public string AmountValue2 { get; set; }
        public string AmountName3 { get; set; }
        public string AmountValue3 { get; set; }
        public string AmountName4 { get; set; }
        public string AmountValue4 { get; set; }
        public string InfoName1 { get; set; }
        public string InfoValue1 { get; set; }
        public string InfoName2 { get; set; }
        public string InfoValue2 { get; set; }
        public string InfoName3 { get; set; }
        public string InfoValue3 { get; set; }
        public string InfoName4 { get; set; }
        public string InfoValue4 { get; set; }
        public string Param1 { get; set; }
        public string ParamValue1 { get; set; }
        public string Param2 { get; set; }
        public string ParamValue2 { get; set; }
        public string Param3 { get; set; }
        public string ParamValue3 { get; set; }
        public string Param4 { get; set; }
        public string ParamValue4 { get; set; }
        public string Param5 { get; set; }
        public string ParamValue5 { get; set; }
        public string RefferenceID { get; set; }
        public decimal EarlyPaymentAmount { get; set; }
        public decimal LatePaymentAmount { get; set; }
        public string EarlyPaymentDate { get; set; }
        public int Status { get; set; }
        public string Reason { get; set; }
    }
    public static class BBPSComplainType
    {
        public const int Service = 1;
        public const int Transaction = 2;
    }
    
}
