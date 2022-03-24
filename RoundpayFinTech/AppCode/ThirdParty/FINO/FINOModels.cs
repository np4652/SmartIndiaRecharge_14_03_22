using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.ThirdParty.FINO
{
    public class FINOAppSetting {
        public string BaseURL { get; set; }
        public string HeaderKey { get; set; }
        public string BodyKey { get; set; }
        public string ClientId { get; set; }
        public string AuthKey { get; set; }
    }
    public class FINOHeaderRequest {
        public string ClientId { get; set; }
        public string AuthKey { get; set; }
    }
    public class FINOTransferRequest
    {
        public string ClientUniqueID { get; set; }
        public string CustomerMobileNo { get; set; }
        public string BeneIFSCCode { get; set; }
        public string BeneAccountNo { get; set; }
        public string BeneName { get; set; }
        public decimal Amount { get; set; }
        public string CustomerName { get; set; }
        public string RFU1 { get; set; }
        public string RFU2 { get; set; }
        public string RFU3 { get; set; }
    }
    public class FINOTransferResponse
    {
        public string RequestID { get; set; }
        public int? ResponseCode { get; set; }
        public string MessageString { get; set; }
        public string DispalyMessage { get; set; }
        public string ClientUniqueID { get; set; }
        public string ResponseData { get; set; }
    }
    public class FINOTrnsferDetail {
        public string ActCode { get; set; }
        public string TxnID { get; set; }
        public decimal AmountRequested { get; set; }
        public decimal ChargesDeducted { get; set; }
        public decimal TotalAmount { get; set; }
        public string BeneName { get; set; }
        public string Rfu1 { get; set; }
        public string Rfu2 { get; set; }
        public string Rfu3 { get; set; }
        public string TransactionDatetime { get; set; }
        public string TxnDescription { get; set; }
        public string MessageString { get; set; }

    }
    public class FinoBalance {
        public decimal AvailableBalance { get; set; }
    }
}
