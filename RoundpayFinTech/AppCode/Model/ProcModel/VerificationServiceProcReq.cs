using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Model.ProcModel
{
    public class VerificationServiceProcReq
    {
        public int UserID { get; set; }
        public int OID { get; set; }
        public string AccountNo { get; set; }
        public string Optional1 { get; set; }
        public string Optional2 { get; set; }
        public string Optional3 { get; set; }
        public string Optional4 { get; set; }
        public string IP { get; set; }
        public int BankID { get; set; }
        public string BankHandle { get; set; }
        public int RequestMode { get; set; }
        public string APIRequestID { get; set; }
        public string SPKey { get; set; }
    }
    public class VerificationServiceProcRes
    {
        public int Statuscode { get; set; }
        public string Msg { get; set; }
        public int TID { get; set; }
        public string TransactionID { get; set; }
        public decimal Balance{ get; set; }
        public int ErrorCode{ get; set; }
        public string APICode { get; set; }
        public int APIID { get; set; }
        public int OpTypeID { get; set; }
    }
}
