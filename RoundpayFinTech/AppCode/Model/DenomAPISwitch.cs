using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Model
{
    public class DenomAPISwitch
    {
        public List<OperatorDetail> Operators { get; set; }
        public List<APIDetail> APIList { get; set; }
    }
}
