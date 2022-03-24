using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Model.ProcModel
{
    public class CallBackOTP
    {
   
        public string Msg { get; set; }
        public string Status { get; set; }
        public string ReferenceID { get; set; }
        public int UserID { get; set; }
    }
}
