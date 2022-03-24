using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.Report;
using System.Collections.Generic;

namespace RoundpayFinTech.AppCode.Model
{
    public class APIDenomination
    {
        public int OID { get; set; }
        public string OPName { get; set; }
        public int APIId { get; set; }
        public string APIName { get; set; }
        public int DenomID { get; set; }
        public int DRangeID { get; set; }
        public List<APIDenomModal> DenomDetail { get; set; }
        public List<APIDRangeModal> DenomRangeDetail { get; set; }
    }

    public class APIDenomModal
    {
        public int DnomID { get; set; }
        public int Amount { get; set; }
        public bool IsDnomActive { get; set; }
        public int HitCountD { get; set; }
        public int MaxCountD { get; set; }
    }

    public class APIDRangeModal
    {
        public int DRangeID { get; set; }
        public int HitCountDR { get; set; }
        public int MaxCountDR { get; set; }
        public string DRange { get; set; }
        public bool IsDRangeActive { get; set; }
    }

    public class APIDenominationReq:CommonReq
    {
        public int OID { get; set; }
        public string OPName { get; set; }
        public int APIId { get; set; }
        public int MaxCount { get; set; }
        public string APIName { get; set; }
        public int DenomID { get; set; }
        public int DRangeID { get; set; }
        public bool Action { get; set; }
        public bool IsDnomActive { get; set; }
        public bool IsDRangeActive { get; set; }
        //public IEnumerable<CirlceMaster> Cirlces { get; set; }

        public int CircleID { get; set; }
        public IEnumerable<CirlceMaster> Cirlces { get; set; }
    }
}
