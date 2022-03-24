using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System.Collections.Generic;

namespace RoundpayFinTech.AppCode.Model
{
    public class RangeModel
    {
        public int ID { get; set; }
        public int OpTypeId { get; set; }
        public int MaxRange { get; set; }
        public int MinRange { get; set; }
        public string OpType { get; set; }
        public string ModifyDate { get; set; }
        public List<OpTypeMaster> OpTypes { get; set; }
    }

    public class RangeModelReq: CommonReq
    {
        public RangeModel Detail { get; set; }
    }
}
