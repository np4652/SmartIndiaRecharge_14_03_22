using System.Collections.Generic;

namespace RoundpayFinTech.AppCode.ThirdParty.CyberPlate
{
    public class CyberPlatRequestModel
    {
        public string SD { get; set; }
        public string AP { get; set; }
        public string OP { get; set; }
        public string SESSION { get; set; }
        public string NUMBER { get; set; }
        public string ACCOUNT { get; set; }
        public string Authenticator3 { get; set; }
        public string AMOUNT { get; set; }
        public string AMOUNT_ALL { get; set; }
        public string TERM_ID { get; set; }
        public string COMMENT { get; set; }
    }
    public class CyberPlateBBPSResponse : BBPSResponse
    {
        public string DATE { get; set; }
        public string SESSION { get; set; }
        public string ERROR { get; set; }
        public string RESULT { get; set; }
        public string TRANSID { get; set; }
        public string PRICE { get; set; }
        public string AUTHCODE { get; set; }
        public string TRNXSTATUS { get; set; }
        public string ERRMSG { get; set; }
        public string RefID { get; set; }
    }

    public class BBPSResponse
    {
        public bool IsBBPSInStaging { get; set; }
        public int FetchBillID { get; set; }
        public int Statuscode { get; set; }
        public string Msg { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorMsg { get; set; }
        public bool IsEditable { get; set; }
        public bool IsEnablePayment { get; set; }
        public bool IsShowMsgOnly { get; set; }
        public bool IsHardCoded { get; set; }
        public string CustomerName { get; set; }
        public string BillNumber { get; set; }
        public string BillDate { get; set; }
        public string DueDate { get; set; }
        public string Amount { get; set; }
        public string BillPeriod { get; set; }
        public string RefferenceID { get; set; }
        public List<BillDateAmount> BillDates { get; set; }
        public List<BillAmountOption> billAmountOptions { get; set; }
        public string BillAmountKey { get; set; }
        public List<BillAdditionalInfo> billAdditionalInfo { get; set; }
        public string BillMonth { get; set; }
        public string BillerPaymentModes { get; set; }
        public int Exactness{ get; set; }
    }
    public class BillDateAmount
    {
        public string DueDate { get; set; }
        public string Month { get; set; }
        public string Amount { get; set; }
        public string DateValue { get; set; }
        public string Remark { get; set; }
        public bool IsFull { get; set; }
    }
    public class BillAmountOption
    {
        public string AmountName { get; set; }
        public decimal AmountValue { get; set; }
    }
    public class BillAdditionalInfo
    {
        public string InfoName { get; set; }
        public string InfoValue { get; set; }
    }
   
    public class BBPSViewBillResponse
    {
        public string STATUS { get; set; }
        public string MOBILE { get; set; }
        public string RPID { get; set; }
        public string AGENTID { get; set; }
        public string OPID { get; set; }
        public string CustomerName { get; set; }
        public string BillDate { get; set; }
        public string BillPeriod { get; set; }
        public string DueDate { get; set; }
        public string MSG { get; set; }
        public string DueAmt { get; set; }
    }

    public class CyberPalteResponse
    {
        public string DATE { get; set; }
        public string SESSION { get; set; }
        public string ERROR { get; set; }
        public string REST { get; set; }
        public string RESULT { get; set; }
        public string ERRMSG { get; set; }
        public string ADDINFO { get; set; }
        public string TRANSID { get; set; }
        public string AUTHCODE { get; set; }
        public string TRNXSTATUS { get; set; }
        public string OPERATOR_ERROR_MESSAGE { get; set; }
    }
    public class CyberAddInfo
    {
        public string state { get; set; }
        public string status { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string message { get; set; }
        public decimal amount { get; set; }
        public int response_code { get; set; }
        public string customerMobile { get; set; }
        public string txn_id { get; set; }
        public string mw_txn_id { get; set; }
        public CyberExtraInfo extra_info { get; set; }
        public string beneficiaryId { get; set; }
        public List<CyberBeneficiary> beneficiaries { get; set; }
        public int totalCount { get; set; }
        public string rrn { get; set; }
        public string transactionDate { get; set; }
        public decimal limit { get; set; }
        public decimal limitLeft { get; set; }
        public string statuscode { get; set; }
        public CyberInstanpayBeneList data { get; set; }
    }
    public class CyberAddInfoIPay
    {
        public string status { get; set; }
        public string statuscode { get; set; }
        public CyberInstanpayBeneCreation data { get; set; }
    }
    public class CyberAddInfoIPayData
    {
        public string status { get; set; }
        public string statuscode { get; set; }
        public string data { get; set; }

    }
    public class CyberInstanpayBeneCreation
    {
        public CyberBeneficiary beneficiary { get; set; }
        public CyberRemitter remitter { get; set; }
        public int otp { get; set; }
    }
    public class CyberInstanpayBeneList
    {
        public string remarks { get; set; }
        public string ref_no { get; set; }
        public string opr_id { get; set; }
        public string name { get; set; }
        public double amount { get; set; }
        public string bankrefno { get; set; }
        public string ipay_id { get; set; }
        public string benename { get; set; }
        public string verification_status { get; set; }
        public double charged_amt { get; set; }
        public List<CyberBeneficiary> beneficiary { get; set; }
        public CyberRemitter remitter { get; set; }
    }
    public class CyberBeneficiary
    {
        public string beneficiaryId { get; set; }
        public string imps { get; set; }
        public string status { get; set; }
        public string account { get; set; }
        public string name { get; set; }
        public string mobile { get; set; }
        public string ifsc { get; set; }
        public string bank { get; set; }
        public string last_success_name { get; set; }
        public string id { get; set; }
        public string last_success_date { get; set; }
        public string last_success_imps { get; set; }
        public CyberAccountDetail accountDetail { get; set; }
    }
    public class CyberAccountDetail
    {
        public string accountNumber { get; set; }
        public string ifscCode { get; set; }
        public string bankName { get; set; }
        public string accountHolderName { get; set; }
    }

    public class CyberExtraInfo
    {
        public string totalAmount { get; set; }
        public string beneficiaryName { get; set; }
        public string commission { get; set; }
    }
    public class CyberRemitter
    {
        public string kycstatus { get; set; }
        public int consumedlimit { get; set; }
        public string name { get; set; }
        public int remaininglimit { get; set; }
        public int is_verified { get; set; }
        public string state { get; set; }
        public string pincode { get; set; }
        public string kycdocs { get; set; }
        public int perm_txn_limit { get; set; }
        public string city { get; set; }
        public string id { get; set; }
        public string address { get; set; }
    }
}
