using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Model.Lookup
{
    public class LookupMPlanAPIResponse
    {
        public string tel { get; set; }
        public _LookupMPlanAPIResponse records { get; set; }
        public double time { get; set; }
    }
    public class _LookupMPlanAPIResponse
    {
        public int status { get; set; }
        public string Operator { get; set; }
        public string circle { get; set; }
        public string comcircle { get; set; }
        public string segment { get; set; }
        public string desc { get; set; }
    }
}
