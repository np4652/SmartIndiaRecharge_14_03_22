using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Model.ProcModel
{
    public class SMSSendREQ
    {
        public int FormatType { get; set; }
        public string MobileNo { get; set; }
        public DataTable Tp_ReplaceKeywords { get; set; }
        public string GeneralSMS { get; set; }
        public int WID {get;set;}
    }
    public class SMSREqSendBulk: SMSSendREQ
    {
        public int ApiID { get; set; }
        public bool IsLapu { get; set; }
    }
    public class SMSSendBulk
    {
        public int APIID { get; set; }
        public string SMS { get; set; }
        public string SmsURL { get; set; }
        public string APIMethod { get; set; }
        public string MobileNo { get; set; }
        public bool IsLapu { get; set; }
        public bool IsSend { get; set; }
        public string ApiResp { get; set; }
        public int WID { get; set; }
    }
    public class SentSMSRequest
    {
        public int LoginTypeID { get; set; }
        public int LoginID { get; set; }
        public string MobileNo { get; set; }
        public int Top { get; set; }
        public string Message { get; set; }
    }
    public class SentSmsResponse
    {
        public bool IsRead { get; set; }
        public string TransactionId { get; set; }
        public int UserID { get; set; }
        public string Name { get; set; }
        public string ServiceType { get; set; }
        public string MobileNo { get; set; }
        public string Message { get; set; }
        public string Response { get; set; }
        public string ReqURL { get; set; }
        public string EntryDate { get; set; }
        public int Status { get; set; }

    }
    
}
