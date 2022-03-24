using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Model.Lookup
{
    public class _Records
    {
        public int status { get; set; }
        public string @operator { get; set; }
        public object segment { get; set; }
        public string circle { get; set; }
        public string comcircle { get; set; }
    }

    public class _Result
    {
        public string tel { get; set; }
        public _Records records { get; set; }
        public double time { get; set; }
    }

    public class HlrLookUPMyPlan
    {
        public bool status { get; set; }
        public string msg { get; set; }
        public _Result result { get; set; }
    }
    

}
