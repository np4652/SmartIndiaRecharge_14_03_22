using RoundpayFinTech.AppCode.Model.ProcModel;

namespace RoundpayFinTech.AppCode.Model
{
    public class SPAPISetting
    {
        public string BaseURL { get; set; }
        public string Token { get; set; }
    }

    public class SQPModelReq
    {
        public string token { get; set; }
        public string amount { get; set; }
        public string name { get; set; }
        public string account { get; set; }
        public string ifsc { get; set; }
        public string bank { get; set; }
        public string apitxnid { get; set; }
        public string callback { get; set; }
        public string ip { get; set; }
        public string txnid { get; set; }
    }
    public class SQPModelResp
    {
        public string status { get; set; }
        public string trans_status { get; set; }
        public string message { get; set; }
        public string rrn { get; set; }
        public string name { get; set; }
    }
}