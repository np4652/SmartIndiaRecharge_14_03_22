using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Model.ProcModel
{
    public class ShoppingOTPReq
    {
        public string UserMobile { get; set; }
        public string WLMobile { get; set; }
        public string RequestSession { get; set; }
        public string OTP { get; set; }
        public string IPAddress { get; set; }
        public int RefferenceID { get; set; }
    }
}
