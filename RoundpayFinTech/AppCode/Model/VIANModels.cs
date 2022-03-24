using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Model
{
    public class VIANCallBackRequest
    {
        public int MVBID { get; set; }
        public string MVBToken { get; set; }
        public string AgentID { get; set; }
        public string VIAN { get; set; }
        public decimal Amount { get; set; }
    }
    public class VIANCallbackStatusResponse
    {
        public int Statuscode { get; set; }
        public string Msg { get; set; }
        public string AgentID { get; set; }
        public string RPID { get; set; }
        public object data { get; set; }
    }
}
