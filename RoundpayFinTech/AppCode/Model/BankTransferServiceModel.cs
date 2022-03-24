using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Model
{
    public class BankServiceReq
    {
        public int LoginID { get; set; }
        public int WalletRequestID { get; set; }
        public int RequestModeID { get; set; }
        public string RequestIP { get; set; }
    }
    public class BankServiceResp
    {
        public int Statuscode { get; set; }
        public string Msg { get; set; }
        public int TID{ get; set; }
        public string TransactionID{ get; set; }
        public string AccountNumber { get; set; }
        public string AccountHolder { get; set; }
        public string IFSC{ get; set; }
        public string OutletMobile { get; set; }
        public string APICode { get; set; }
        public string APIOpCode { get; set; }
        public int UserID { get; set; }
        public decimal Amount { get; set; }
        public int APIID { get; set; }
        public int OID { get; set; }
        public int TransactionMode { get; set; }
        public string BankName { get; set; }
        public string EmailID { get; set; }
        public int BankID { get; set; }
        public string BrandName { get; set; }
        public string APIOutletID { get; set; }
        public string WebsiteName { get; set; }
        public string APIGroupCode { get; set; }
    }
}
