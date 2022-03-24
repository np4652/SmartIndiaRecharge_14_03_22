using Fintech.AppCode.Model;
using System.Collections.Generic;

namespace RoundpayFinTech.AppCode.Model
{
    public class DenominationRange
    {
        public int ID { get; set; }
        public int Min { get; set; }
        public int Max { get; set; }
        public int EntryBy { get; set; }
        public string EntryDate { get; set; }
        public int ModifyBy { get; set; }
        public string ModifyDate { get; set; }
        public int IsDel { get; set; }
    }

    public class DenominationRangeReq : CommonReq
    {
        public DenominationRange Detail { get; set; }
    }

    public class DenominationRangeList
    {
        public List<DenominationRange> Detail { get; set; }
        public int AdminOrAPIUser { get; set; }
    }
}
