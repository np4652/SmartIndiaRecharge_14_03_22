using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Model.ProcModel
{
    public class UPIPaymentProcReq
    {
        public int UserID { get; set; }
        public string AccountNo { get; set; }
        public decimal AmountR { get; set; }
        public string APIRequestID { get; set; }
        public string IPAddress { get; set; }
        public int RequestMode { get; set; }
    }
    public class UPIPaymentProcRes
    {
        public int Statuscode { get; set; }
        public string Msg  { get; set; }
        public int ErrorCode { get; set; }
        public int TID { get; set; }
        public string TransactionID { get; set; }
        public decimal Balance { get; set; }
        public string APICode { get; set; }
        public int APIID { get; set; }
        public string Latlong { get; set; }
        public string EmailID { get; set; }
        public string OutletMobile { get; set; }
        public string OpCode { get; set; }
    }
}
