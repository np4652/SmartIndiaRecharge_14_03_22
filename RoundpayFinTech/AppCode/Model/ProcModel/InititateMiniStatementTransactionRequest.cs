using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Model.ProcModel
{
    public class InititateMiniStatementTransactionRequest
    {
        public int UserID { get; set; }
        public int OutletID { get; set; }
        public int OID { get; set; }
        public string AccountNo{ get; set; }
        public string BankName{ get; set; }
        public string BankIIN{ get; set; }
        public string APICode{ get; set; }
        public string APIOutletID{ get; set; }
        public string VendorID{ get; set; }
        public string APIOpCode{ get; set; }
        public string APIRequestID{ get; set; }
        public int RequestModeID{ get; set; }
        public string IP{ get; set; }
        public string Browser{ get; set; }
        public string SPKey { get; set; }
    }
}
