using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.Model.Report;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Model
{
    public class APICircleCode
    {
        public int LT { get; set; }
        public int LoginID { get; set; }
        public int CircleID { get; set; }
        public int APIID { get; set; }
        public int APIType { get; set; }
        public string APIName { get; set; }
        public string APIcirclecode { get; set; }
    }

    public class APICircleCodeModel
    {
        public IEnumerable<CirlceMaster> Circles { get; set; }
        public IEnumerable<APICircleCode> APICircleCode { get; set; }
        public IEnumerable<APIDetail> APIs { get; set; }
    }
}
