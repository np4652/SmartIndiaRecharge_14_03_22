using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Model
{
    public class SMSUpdateREQ
    {
            public int SMSID { get; set; }
            public int Status { get; set; }
            public string Response { get; set; }
            public string ResponseID { get; set; }
    }
}
