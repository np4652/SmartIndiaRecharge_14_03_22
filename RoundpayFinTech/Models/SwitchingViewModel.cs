using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.Models
{
    public class SwitchingViewModel
    {
        public IEnumerable<OpTypeMaster> opTypes{ get; set; }
        public bool IsMultipleMobileAllowed{ get; set; }
    }
}
