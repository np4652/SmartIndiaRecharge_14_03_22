namespace RoundpayFinTech.AppCode.ThirdParty.Paytm
{
    public class PaytmAppSetting
    {
        public string MID { get; set; }
        public string MERCHANTKEY { get; set; }
        public string PAYOUTBASEURL { get; set; }
        public string VERIFYBASEURL { get; set; }
        public string STATUSCHECKURL { get; set; }
        public string GUID { get; set; }
    }
    public class PaytmPGRequest
    {
        public string MID { get; set; }
        public string ORDER_ID { get; set; }
        public string CUST_ID { get; set; }
        public string TXN_AMOUNT { get; set; }
        public string CHANNEL_ID { get; set; }
        public string WEBSITE { get; set; }
        public string MOBILE_NO { get; set; }
        public string EMAIL { get; set; }
        public string INDUSTRY_TYPE_ID { get; set; }
        public string CALLBACK_URL { get; set; }
        public string MERC_UNQ_REF { get; set; }
        public string PAYMENT_MODE_ONLY { get; set; }
        public string AUTH_MODE { get; set; }
        public string PAYMENT_TYPE_ID { get; set; }
        public string BANK_CODE { get; set; }
        public string CHECKSUMHASH { get; set; }
    }
    public class PaytmPGResponse
    {
        public string CURRENCY { get; set; }
        public string GATEWAYNAME { get; set; }
        public string RESPMSG { get; set; }
        public string BANKNAME { get; set; }
        public string PAYMENTMODE { get; set; }
        public string MID { get; set; }
        public string RESPCODE { get; set; }
        public string TXNID { get; set; }
        public string TXNAMOUNT { get; set; }
        public string ORDERID { get; set; }
        public string STATUS { get; set; }
        public string BANKTXNID { get; set; }
        public string TXNDATE { get; set; }
        public string CHECKSUMHASH { get; set; }
        public string FLYINGARROW { get; set; }
        public string body { get; set; }
        public string response { get; set; }
    }

    public class PayTMCallbackTextResp
    {
        public PayTMCallbackTextInfo txnInfo { get; set; }
    }
    public class PayTMCallbackTextInfo
    {
        public string BANKTXNID { get; set; }
        public string CHECKSUMHASH { get; set; }
        public string CURRENCY { get; set; }
        public string MID { get; set; }
        public string ORDERID { get; set; }
        public string RESPCODE { get; set; }
        public string RESPMSG { get; set; }
        public string STATUS { get; set; }
        public string TXNAMOUNT { get; set; }
        public string TXNID { get; set; }
    }

    public class PayTMJSPGResponse
    {
        public PayTMJSHead head { get; set; }
        public PayTMJSBody body { get; set; }
    }
    public class PayTMJSHead
    {
        public string responseTimestamp { get; set; }
        public string version { get; set; }
        public string signature { get; set; }
    }
    public class PayTMJSBody
    {
        public string txnId { get; set; }
        public string bankTxnId { get; set; }
        public string orderId { get; set; }
        public string txnAmount { get; set; }
        public string txnType { get; set; }
        public string gatewayName { get; set; }
        public string bankName { get; set; }
        public string mid { get; set; }
        public string paymentMode { get; set; }
        public PayTMResultInfo resultInfo { get; set; }
        public class PayTMResultInfo
        {
            public string resultStatus { get; set; }
            public string resultCode { get; set; }
            public string resultMsg { get; set; }
        }
    }
    public class CommonPGResponse
    {
        public string TID { get; set; }
        public string Amount { get; set; }
        public string TransactionID { get; set; }
        public string status { get; set; }
        public string reason { get; set; }
    }

    public class PAYTMResponseType
    {
        public const string SUCCESS = "TXN_SUCCESS";
        public const string FAILED = "TXN_FAILURE";
    }
    public class PaytmDataRequest
    {
        public string subwalletGuid { get; set; }
        public string orderId { get; set; }
        public string beneficiaryAccount { get; set; }
        public string beneficiaryIFSC { get; set; }
        public string purpose { get; set; }
        public string amount { get; set; }
        public string transferMode { get; set; }
        public string transactionType { get; set; }
        public string callbackUrl { get; set; }
        public string beneficiaryVPA { get; set; }
        //public string beneficiaryPhoneNo { get; set; }
        //public bool validateBeneficiary { get; set; }
        public string beneficiaryName { get; set; }
        public string date { get; set; }

    }
    public class PaytmValidateRequest
    {
        public string subwalletGuid { get; set; }
        public string orderId { get; set; }
        public string beneficiaryAccount { get; set; }
        public string beneficiaryIFSC { get; set; }

    }
    public class PaytmStatusCheck
    {
        public string orderId { get; set; }
    }

    public class PaytmResult
    {
        public string mid { get; set; }
        public string orderId { get; set; }
        public string paytmOrderId { get; set; }
        public string amount { get; set; }
        public string commissionAmount { get; set; }
        public string tax { get; set; }
        public string rrn { get; set; }
        public string beneficiaryName { get; set; }
    }

    public class PaytmResponse
    {
        public string status { get; set; }
        public string statusCode { get; set; }
        public string statusMessage { get; set; }
        public PaytmResult result { get; set; }
    }
    public class PaytmTokenResponse
    {
        public PaytmHead head { get; set; }
        public PaytmBody body { get; set; }
    }
    public class PaytmHead
    {
        public string responseTimestamp { get; set; }
        public string version { get; set; }
        public string clientId { get; set; }
        public string signature { get; set; }
    }
    public class PaytmBody
    {
        public string txnToken { get; set; }
        public bool isPromoCodeValid { get; set; }
        public bool authenticated { get; set; }
        public PaytmResultInfo resultInfo { get; set; }
    }
    public class PaytmResultInfo
    {
        public string resultStatus { get; set; }
        public string resultCode { get; set; }
        public string resultMsg { get; set; }
    }
}
