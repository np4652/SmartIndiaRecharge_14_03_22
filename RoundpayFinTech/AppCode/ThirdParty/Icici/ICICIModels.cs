using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.ThirdParty.Icici
{
    public class ICICIAppSetting
    {
        public string BC { get; set; }
        public string PassCode { get; set; }
        public string BaseURL { get; set; }
    }
    public class ICICIReqModel
    {
        public string BeneAccNo { get; set; }
        public string BeneIFSC { get; set; }
        public string Amount { get; set; }
        public string TranRefNo { get; set; }
        public string PaymentRef { get; set; }
        public string RemName { get; set; }
        public string RemMobile { get; set; }
        public string RetailerCode { get; set; }
        public string TransactionDate { get; set; }
        public string PassCode { get; set; }
    }

    public class ICICIImpsResponse
    {
        public string ActCode { get; set; }
        public string Response { get; set; }
        public string BeneName { get; set; }
        public string TranRefNo { get; set; }
        public string BankRRN { get; set; }
        public string PaymentRef { get; set; }
        public string TranDateTime { get; set; }
        public string TranAmount { get; set; }
        public string BeneMMID { get; set; }
        public string BeneMobile { get; set; }
        public string BeneAccNo { get; set; }
        public string BeneIFSC { get; set; }
        public string RemName { get; set; }
        public string RemMobile { get; set; }
        public string RetailerCode { get; set; }
    }
    #region ICICPayout Models
    public class ICICPayoutResponse
    {
        public string response { get; set; }
        public string UTRNUMBER { get; set; }
        public string REQID { get; set; }
        public string STATUS { get; set; }
        public object RESPONSECODE { get; set; }
        public string MESSAGE { get; set; }
        public string message { get; set; }
    }
    public class ICICIPayoutRequest
    {
        public string AGGRID { get; set; }
        public string AGGRNAME { get; set; }
        public string CORPID { get; set; }
        public string USERID { get; set; }
        public string URN { get; set; }
        public string UNIQUEID { get; set; }
        public string DEBITACC { get; set; }
        public string CREDITACC { get; set; }
        public string IFSC { get; set; }
        public int AMOUNT { get; set; }
        public string CURRENCY { get; set; }// "INR";
        public string TXNTYPE { get; set; }
        public string PAYEENAME { get; set; }
        public string REMARKS { get; set; }
        public char WORKFLOW_REQD { get; set; }
        public string ALIASID { get; set; }
    }
    public class ICICIPayoutAppSetting
    {
        public string DEBITACC { get; set; }
        public string PAYEENAME { get; set; }
        public string CORPID { get; set; }
        public string USERID { get; set; }
        public string AGGRNAME { get; set; }
        public string ALIASID { get; set; }
        public string AGGRID { get; set; }
        public string URN { get; set; }
        public string APIKey { get; set; }
        public string BaseURL { get; set; }
        public string merchantId { get; set; }
        public string merchantName { get; set; }
        public string CollectAPIRequestURL { get; set; }
        public string CollectAPIRequestURLAPP { get; set; }
        public string CollectAPIAppStatusCallbackURL { get; set; }
        public string CollectAPIAppStatusURL { get; set; }
        public string CollectAPIWebStatusURL { get; set; }
        public string CollectAPIRefundURL { get; set; }
        public string QRIntent { get; set; }
        public string terminalId { get; set; }
        public string merchantVPA { get; set; }
        public string CollectVirtualCode { get; set; }
        public string CollectIFSC { get; set; }
        public string CollectBranch { get; set; }
        public string CollectBeneName { get; set; }
        public string HYPTO_VerificationURL { get; set; }
        public string HYPTO_VerifyAuth { get; set; }
    }
    #endregion

    public class ICICCollectStatusReq
    {
        public string merchantId { get; set; }
        public string subMerchantId { get; set; }
        public string terminalId { get; set; }
        public string merchantTranId { get; set; }
    }
    public class ICICUPIStatusReq: ICICCollectStatusReq
    {
        public string transactionType { get; set; }
        public string BankRRN { get; set; }
    }
    public class ICICICollectPayReqQR
    {
        public string merchantId { get; set; }
        public string terminalId { get; set; }
        public string merchantTranId { get; set; }
        public string amount { get; set; }
        public string billNumber { get; set; }
    }
    public class ICICICollectPayReq : ICICCollectStatusReq
    {
        public string payerVa { get; set; }
        public string amount { get; set; }
        public string note { get; set; }
        public string collectByDate { get; set; }
        public string merchantName { get; set; }
        public string subMerchantName { get; set; }
        public string billNumber { get; set; }
    }
    public class ICICICollectPayRes
    {
        public string response { get; set; }
        public string Response { get; set; }
        public string merchantId { get; set; }
        public string subMerchantId { get; set; }
        public string terminalId { get; set; }
        public string Success { get; set; }
        public string success { get; set; }
        public string Message { get; set; }
        public string merchantTranId { get; set; }
        public string BankRRN { get; set; }
        public string OriginalBankRRN { get; set; }
        public string refId { get; set; }
        public string status { get; set; }
    }

    public class ICICICallBackRes
    {
        public string merchantId { get; set; }
        public string subMerchantId { get; set; }
        public string terminalId { get; set; }
        public string BankRRN { get; set; }
        public string merchantTranId { get; set; }
        public string PayerName { get; set; }
        public string PayerMobile { get; set; }
        public string PayerVA { get; set; }
        public string PayerAmount { get; set; }
        public string Amount { get; set; }
        public string TxnStatus { get; set; }
        public string success { get; set; }
        public string response { get; set; }
        public string message { get; set; }
        public string OriginalBankRRN { get; set; }
        public string status { get; set; }
        public string TxnInitDate { get; set; }
        public string TxnCompletionDate { get; set; }
    }

    public class ICICIRefundReq
    {
        public string merchantId { get; set; }
        public string subMerchantId { get; set; }
        public string originalBankRRN { get; set; }
        public string merchantTranId { get; set; }
        public string originalmerchantTranId { get; set; }
        public string refundAmount { get; set; }
        public string payeeVA { get; set; }
        public string note { get; set; }
        public string onlineRefund { get; set; }
    }
    public class ICICIRefundRes
    {

    }


}
