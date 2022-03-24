using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System.Collections.Generic;

namespace RoundpayFinTech.Models
{
    public class DMRModel
    {
        public int ID { get; set; }
        public string Name { get; set; }
    }

    public class SlabDetailDisplayModel
    {
        public IEnumerable<SlabDetailDisplayLvl> slbModel { get; set; }
        public bool IsAPIUser { get; set; }
        public bool IsCircleSlabAllowed { get; set; }
        public IEnumerable<OperatorOptionalStuff> Optionals { get; set; }
    }
}
