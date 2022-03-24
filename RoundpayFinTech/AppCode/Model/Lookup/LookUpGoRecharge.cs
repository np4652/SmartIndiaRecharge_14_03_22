using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Model.Lookup
{
    public class LookUpGoRechargeReq
    {
        public string CorporateNo { get; set; }
        public string MobileNo { get; set; }
        public string SystemReferenceNo { get; set; }
        public string APIChecksum { get; set; }
    }

    public class LookUpGoRechargeRes
    {
        public string MobileNo { get; set; }
        public string SystemReferenceNo { get; set; }
        public string CorpRefNo { get; set; }
        public string CurrentOperator { get; set; }
        public string CurrentLocation { get; set; }
        public string PreviousOperator { get; set; }
        public string PreviousLocation { get; set; }
        public object Ported { get; set; }
        public string Charged { get; set; }
        public string Error { get; set; }
    }
}
