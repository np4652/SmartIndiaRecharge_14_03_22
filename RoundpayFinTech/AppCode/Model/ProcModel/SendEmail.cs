using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Model.ProcModel
{
    public class SendEmail
    {
        public int ID { get; set; }
        public string From { get; set; }
        public string Recipients { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public bool IsSent { get; set; }
        public int WID { get; set; }
        public string UserName { get; set; }
        public string UserMobileNo { get; set; }
        public string EmailID { get; set; }
        public string Message { get; set; }
        public int RequestMode { get; set; }
        public string RequestIP { get; set; }
        public string RequestPage { get; set; }

    }
}
