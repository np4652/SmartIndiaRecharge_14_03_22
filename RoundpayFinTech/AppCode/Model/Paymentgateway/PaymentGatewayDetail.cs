using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.ThirdParty.CashFree;
using RoundpayFinTech.AppCode.ThirdParty.Paytm;
using RoundpayFinTech.AppCode.ThirdParty.Razorpay;
using System.Collections.Generic;

namespace RoundpayFinTech.AppCode.Model.Paymentgateway
{
    public class PGDisplayModel
    {
        public int OID { get; set; }
        public int UPGID { get; set; }
        public int WalletID { get; set; }
        public int Amount { get; set; }
        public string VPA { get; set; }
        public IEnumerable<PaymentGatewayModel> PGs { get; set; }
        public UserBalnace UB { get; set; }
        public IEnumerable<OperatorDetail> modes { get; set; }
    }
    public class PaymentGatewayDetail
    {
        public int PGID { get; set; }
        public int UPGID { get; set; }
        public string PG { get; set; }
        public string URL { get; set; }
        public string StatusCheckURL { get; set; }
        public string Code { get; set; }
        public string MerchantID { get; set; }
        public string MerchantKey { get; set; }
        public string ENVCode { get; set; }
        public string IndustryType { get; set; }
        public string SuccessURL { get; set; }
        public string FailedURL { get; set; }
        public int AgentType { get; set; }
    }

    public class PaymentGatewayModel
    {
        public int ID { get; set; }
        public string PG { get; set; }
        public int PGType { get; set; }
        public int AgentType { get; set; }
    }

    public class PGModelForRedirection
    {
        public int PGType { get; set; }
        public int TID { get; set; }
        public int Statuscode { get; set; }
        public string Msg { get; set; }
        public string URL { get; set; }
        public IDictionary<string, string> KeyVals { get; set; }
        public RazorpayRequest RPayRequest { get; set; }
        public PaytmJSRequest paytmJSRequest { get; set; }
        public CashfreeOrderResponse CashfreeResponse { get; set; }
        public CashFreeResponseForApp CashFreeResponseForApp { get; set; }
    }
    public class PaytmJSRequest
    {
        public string OrderID { get; set; }
        public string Amount { get; set; }
        public string TokenType { get; set; }
        public string Token { get; set; }
        public string MID { get; set; }
        public string PayMode { get; set; }
        public string CallbackUrl { get; set; }
    }
    public class PGModelForApp
    {
        public int Statuscode { get; set; }
        public string Msg { get; set; }
        public int PGID { get; set; }
        public int TID { get; set; }
        public string TransactionID { get; set; }
        public string Token { get; set; }
        public PaytmPGRequest requestPTM { get; set; }
        public RazorpayRequest RPayRequest { get; set; }
        public PGModelForRedirection AggrePayRequest { get; set; }
        public PGModelForRedirection UPIGatewayRequest { get; set; }
        public CashFreeResponseForApp CashFreeResponse { get; set; }
    }
}
