using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Model
{
    public class GIAppSetting {
        public string _URL { get; set; }
    }
    public class GenerateURLResp
    {
        public int Statuscode { get; set; }
        public string Msg { get; set; }
        public string RedirectURL { get; set; }
    }
    public class GeneralInsuranceDBResponse
    {
        public int Statuscode { get; set; }
        public string Msg { get; set; }
        public string GenerateTokenURL { get; set; }
        public string RechType { get; set; }
        public string AgentID { get; set; }
        public string Token { get; set; }
    }
    public class GIRedirectModel {
        public IDictionary<string, string> dic { get; set; }
        public string URL { get; set; }
    }
}
