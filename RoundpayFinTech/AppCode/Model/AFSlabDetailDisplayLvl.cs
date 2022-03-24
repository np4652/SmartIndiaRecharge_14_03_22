using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Model
{
    public class AFSlabDetailDisplayLvl
    {
        public int VID { get; set; }
        public string VName { get; set; }
        public List<SlabRoleCommission> RoleCommission { get; set; }
    }
}
