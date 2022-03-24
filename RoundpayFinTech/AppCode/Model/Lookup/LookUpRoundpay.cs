using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Model.Lookup
{
    public class Response
    {
        public string reqid { get; set; }
        public string lookup_number { get; set; }
        public string old_operator { get; set; }
        public string old_circle { get; set; }
        public string new_operator { get; set; }
        public string new_circle { get; set; }
        public string ported { get; set; }
        public string status_code { get; set; }
    }

    public class Billing
    {
        public double opbal { get; set; }
        public double charged_amt { get; set; }
        public double clbal { get; set; }
    }

    public class LookUpRoundpayRes
    {
        public List<Response> response { get; set; }
        public List<Billing> billing { get; set; }
    }
}
