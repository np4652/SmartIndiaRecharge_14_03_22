using System.Collections.Generic;

namespace RoundpayFinTech.AppCode.Model.ProcModel
{
    public class PackageMaster
    {
        public int PackageId { get; set; }
        public decimal Commission { get; set; }
        public string PackageName { get; set; }
        public bool IsActive { get; set; }
        public bool IsDefault { get; set; }
        public decimal PackageCost { get; set; }
        public string ModifyDate { get; set; }
        public bool IsCurrent { get; set; }
        public List<PkgLevelCommission> RoleDetail { get; set; }
    }

    public class PkgLevelCommission
    {
        public int RoleId { get; set; }
        public string Role { get; set; }
        public decimal Commission { get; set; }
        public bool CommType { get; set; }
        public string ModifyDate { get; set; }
    }

    public class PkgLevelCommissionReq: PkgLevelCommission
    {
        public int LoginId { get; set; }
        public int LoginTypeId { get; set; }
        public int PackageId { get; set; }
        public int ActionType { get; set; }
    }

    public class PackageCommission:PackageMaster
    {
        public List<PackageMaster> PackageDetail { get; set; }
        public List<ServiceMaster> Services { get; set; }
        public List<PackageMaster> ParentPackageDetail { get; set;}
        public List<PkgLevelCommissionReq> CommissionDetail { get; set;}
    }

    public class PackageAvailableModel : PkgLevelCommissionReq
    {
        public int UserId { get; set; }
        public bool IsAvailable { get; set; }
        public bool SelfAssigned { get; set; }
        public decimal cost { get; set; }
        public int AvailablePackageId { get; set; }
        public string IP { get; set; }
        public string Browser { get; set; }
    }
    public class AvailablePackageMaster
    {
        public bool IsChannel { get; set; }
        public bool IsParent { get; set; }
        public bool IsEndUser { get; set; }
        public PackageCommission Data { get; set; }
    }

    public class PackageUpgradeRequest {
        public int LT { get; set; }
        public int LoginID { get; set; }
        public int PackageID { get; set; }
        public int RequestMode { get; set; }
        public string IP { get; set; }
        public string Browser { get; set; }
    }
}
