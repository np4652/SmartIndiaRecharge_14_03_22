using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Model
{
    public class AvailablePackage:ResponseStatus
    {
        public List<PackageMaster> Packages { get; set; }
        public List<ServiceMaster> Services { get; set; }
    }
    public class _AvailablePackage: PackageMaster
    {
        public bool IsEndUser { get; set; }
        public bool IsAdminDefined { get; set; }
        public List<ServiceMaster> Services { get; set; }
    }
}
