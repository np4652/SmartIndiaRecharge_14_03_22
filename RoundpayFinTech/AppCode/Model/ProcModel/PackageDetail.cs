using Fintech.AppCode.Model;
using System.Collections.Generic;

namespace RoundpayFinTech.AppCode.Model.ProcModel
{
    public class ServiceMaster {
        public int ServiceID { get; set; }
        public string ServiceName { get; set; }
        public int PackageId { get; set; }
        public bool IsActive { get; set; }
        public bool IsServiceActive { get; set; }
        public bool IsVisible { get; set; }
        public string SCode { get; set; }
        public int WalletTypeID { get; set; }
        public bool selfAssigned { get; set; }
        public decimal Charge { get; set; }
    }
    public class PackageDetail: ServiceMaster
    {
        public int ID { get; set; }
        public string ModifyDate { get; set; }
    }
    public class PackageModel {
        public List<SlabMaster> Slabs { get; set; }
        public List<PackageDetail> Packages { get; set; }
        public List<ServiceMaster> Services { get; set; }
        public List<PackageAvailableModel> AvailablePackage { get; set; }
        public List<VasUserPackage> vasUserPackage { get; set; }
    }

    public class VasUserPackage
    {
        public int VasPackageID { get; set; }
        public bool IsExpired { get; set; }
        public string PlanExpirationDate { get; set; }
    }
}
