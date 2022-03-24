using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Model
{
    public class SocialMessage
    {
        public int LT { get; set; }
        public int LoginID { get; set; }
        public string UserName { get; set; }
        public DataTable Data { get; set; }
        public bool IsBulk{ get; set; }
        public string SendTo { get; set; }
        public string SenderName { get; set; }
        public string APICode { get; set; }
        public string SCANNO { get; set; } // Sender No
        public string CountryCode { get; set; }
        public string Message { get; set; }
        public int SocialAlertType { get; set; }
        public int SMSAPIID { get; set; }
        public string ConversationId { get; set; }
        public int CCID { get; set; }
        public string CCName { get; set; }
    }
}
