using Newtonsoft.Json;

namespace RoundpayFinTech.AppCode.Model.ProcModel
{
    public class AEPSTransactionServiceReq
    {
        public string OutletID { get; set; }
        public decimal AmountR { get; set; }
        public string VendorID { get; set; }
        public string BankIIN { get; set; }
        public string TXNType { get; set; }
        public string TerminalID { get; set; }
        public string BCID { get; set; }
        public string APICode { get; set; }
        public string RequestIP { get; set; }
        public string RequestURL { get; set; }
        public int Status { get; set; }
        public int TID { get; set; }
        public int OutletIDSelf { get; set; }
        public int LoginID { get; set; }
        public int RequestModeID { get; set; }
        public string LiveID { get; set; }
        public string TransactionID { get; set; }
        public string Req { get; set; }
        public string Resp { get; set; }
        public string UpdatePage { get; set; }
    }
    public class MiniBankTransactionServiceReq : AEPSTransactionServiceReq
    {
        public int OID { get; set; }
        public int OpTypeID { get; set; }
        public string APIOpCode { get; set; }
        public string AccountNo { get; set; }
    }
    public class MBStatusCheckRequest
    {
        public int TID { get; set; }
        public string TransactionID { get; set; }
        public string VendorID { get; set; }
        public string RequestPage { get; set; }
        public int APIStatus { get; set; }
        public string SDKMsg { get; set; }
        public int Amount{ get; set; }
        public int OutletID { get; set; }
        public string AccountNo { get; set; }
        public string BankName { get; set; }
        public string SCode { get; set; }
    }
    public class MBStatuscheckResponseApp
    {
        public int Statuscode { get; set; }
        public string Msg { get; set; }
        public string BankName { get; set; }
        public string Balance { get; set; }
        public string TransactionTime { get; set; }
        public string LiveID { get; set; }
        public string CardNumber { get; set; }
        public int Amount { get; set; }
        public string TransactionID { get; set; }
        public string Request { get; set; }
        public string Response { get; set; }
    }
    public class AEPSTransactionServiceResp
    {
        public int Statuscode { get; set; }
        public string Msg { get; set; }
        public string TransactionID { get; set; }
        public string APIOutletID { get; set; }
        public string OutletName { get; set; }
        public string CallBackURL { get; set; }
        public string BillOpCode { get; set; }
        public int UserID { get; set; }
        public int TID { get; set; }
        public decimal Balance { get; set; }
    }
    public class MiniBankTransactionServiceResp : AEPSTransactionServiceResp
    {
        public int LT { get; set; }
        public int LoginID { get; set; }
        public string LiveID { get; set; }
        public string VendorID { get; set; }
        public int Amount { get; set; }
        public string Req { get; set; }
        public string Resp { get; set; }
        public string IPAddress { get; set; }
        public string Browser { get; set; }
        public string Remark { get; set; }
        public string RequestPage { get; set; }
        public string CardNumber { get; set; }
        public string BankBalance { get; set; }
        public string BankTransactionDate { get; set; }
        public string BankName { get; set; }
        public string CallBackURL { get; set; }
        public string APIRequestID { get; set; }
        
    }

    public class AEPSRoundpayStatusCommon
    {
        [JsonProperty("STATUS")]
        public string STATUS { get; set; }
        [JsonProperty("MESSAGE")]
        public string Msg { get; set; }
    }
    public class RoundpayAEPSResp : AEPSRoundpayStatusCommon
    {
        [JsonProperty("VENDOR_ID")]
        public string VendorID { get; set; }

        [JsonProperty("BC_ID")]
        public string BCID { get; set; }

        [JsonProperty("TRANSACTION_ID")]
        public string TransactionID { get; set; }

        [JsonProperty("USER_BAL")]
        public string Balance { get; set; }
    }
}
