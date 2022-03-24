using RoundpayFinTech.AppCode.Model.ProcModel;
using System.Collections.Generic;

namespace RoundpayFinTech.AppCode.Model
{
    public class APIDetailViewModel
    {
        public int OpTypeId { get; set; }
        public IEnumerable<APIDetail> APIs { get; set; }
    }
}
