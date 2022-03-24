using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Model.Lookup
{
    public class APIBoxResponse
    {
        public string lookup_number { get; set; }
        public string country { get; set; }
        public string operator_circle { get; set; }
        public string old_operator_circle { get; set; }
        public string new_operator_circle { get; set; }
        public bool? ported { get; set; }
        public string status_code { get; set; }
    }

    public class APIBoxBilling
    {
        public string opbal { get; set; }
        public double charged_amt { get; set; }
        public double clbal { get; set; }
    }

    public class LookUpAPIBoxRes
    {
        public APIBoxResponse response { get; set; }
        public APIBoxBilling billing { get; set; }
    }
}
