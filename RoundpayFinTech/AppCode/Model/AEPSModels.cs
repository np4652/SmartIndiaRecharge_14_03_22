using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Model
{
    public class AEPSUniversalRequest
    {
        public string UserAgent { get; set; }
        public string Lattitude { get; set; }
        public string Longitude { get; set; }
        public string MobileNo { get; set; }
        public string AdharNo { get; set; }
        public int RequestMode { get; set; }
        public string IPAddress { get; set; }
        public string TransactionID { get; set; }
        public int TID { get; set; }
        public string BankIIN { get; set; }
        public string PIDDATA { get; set; }
        public PidData PIDData { get; set; }
        public string APIOpCode { get; set; }
        public string APIOutletID { get; set; }
        public decimal Amount { get; set; }
        public int ThreewayStatus { get; set; }
    }
}
