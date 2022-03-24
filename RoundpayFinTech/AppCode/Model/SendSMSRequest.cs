using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Model
{
    public class SendSMSRequest
    {       
        public int SMSID { get; set; }
        public int APIID { get; set; }
        public string SMS { get; set; }
        public string SmsURL { get; set; }
        public string APIMethod { get; set; }
        public string TransactionID { get; set; }
        public string MobileNo { get; set; }
        public bool IsLapu { get; set; }
        public bool IsSend { get; set; }
        public string ApiResp { get; set; }
    }

    public class SMSResponse
    {
        public int SMSID { get; set; }
        public int WID { get; set; }
        public string MobileNo { get; set; }
        public string SMS { get; set; }
        public int Status { get; set; }
        public string TransactionID { get; set; }
        public string Response { get; set; }
        public string ResponseID { get; set; }
        public string ReqURL { get; set; }
        public int SocialAlertType { get; set; }
    }
}
