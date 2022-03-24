using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Model
{
    public class TargetModelLvl
    {
        public int TargetType { get; set; }
        public int SlabID { get; set; }
        public List<RoleMaster> Roles { get; set; }
        public List<ServiceMaster> Services { get; set; }
        public List<OperatorDetail> Operators { get; set; }
        public List<OpTypeMaster> OpTypes { get; set; }
    }
}
