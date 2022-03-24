using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Model
{
    public class VerificationInput
    {
        public int UserID { get; set; }
        public int OID { get; set; }
        public string SPKey { get; set; }
        public string AccountNo { get; set; }
        public string IFSC { get; set; }
        public string APIRequestID { get; set; }
        public int RequestModeID{ get; set; }
    }
    public class VerificationOutput
    {
        public int Statuscode { get; set; }
        public string Msg { get; set; }
        public int ErrorCode{ get; set; }
        public string RPID{ get; set; }
        public string LiveID{ get; set; }
        public string APIRequestID{ get; set; }
        public string AccountHolder{ get; set; }
        public string AccountNo{ get; set; }
    }
}
