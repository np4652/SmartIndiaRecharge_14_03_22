using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Model.Recharge
{
    public class LookupDetail
    {
        public int LoginID { get; set; }
        public string LookupNumber { get; set; }
        public int OID { get; set; }
        public int LookupAPIID { get; set; }
        public bool ForSave { get; set; }
        public string CurrentOperator { get; set; }
        public string CurrentCircle { get; set; }
        public string OldOperator { get; set; }
        public string OldCircle { get; set; }
        public bool IsPorted { get; set; }
        public string LookupResponse { get; set; }
        public int CircleID { get; set; }
        public int FoundOID { get; set; }
        public string FoundOperator { get; set; }
    }
}
