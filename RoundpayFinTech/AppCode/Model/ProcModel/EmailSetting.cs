using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Model.ProcModel
{
    public class EmailSetting
    {
        public string FromEmail { get; set; }
        public string MailUserID { get; set; }
        public string Password { get; set; }
        public string HostName { get; set; }
        public int Port { get; set; }
        public string EmailUserID { get; set; }
        public bool IsActive { get; set; }
        public bool IsSSL { get; set; }
      
    }
    

}
