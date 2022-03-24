using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.ThirdParty.GIBL
{
    public class GIBLPremDeductRequest 
    {
        public long refno { get; set; }
        public string ak { get; set; }
        public string urc { get; set; }
        public string umc { get; set; }
        public string ptype { get; set; }
        public string pamt { get; set; }
        public string reqtime { get; set; }
    }
    public class GIBLPremDeductResponse {
        public string refno { get; set; }
        public int? status { get; set; }
        public string message { get; set; }
        public string resptime { get; set; }
    }
    public class GIBLPolicyConfirmReq {
        public long refno { get; set; }
        public string ak { get; set; }
        public string urc { get; set; }
        public string umc { get; set; }
        public string pamt { get; set; }
        public string pstatus { get; set; }
        public string ptype { get; set; }
        public string od_amt { get; set; }
        public string payout { get; set; }
        public string reqtime { get; set; }
    }
    public class GIBLPolicyConfirmResp
    {
        public int status { get; set; }
        public long refno { get; set; }
        public string message { get; set; }
        public string resptime { get; set; }
    }
}
