
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System.Collections.Generic;

namespace RoundpayFinTech.AppCode.Model
{
    public class DTHCommission
    {
        public int CommissionID { get; set; }
        
        public int SlabID { get; set; }
        public int OID { get; set; }
        public string Operator { get; set; }
        public string OpType { get; set; }
        public int PackageID { get; set; }     
        public string PackageName { get; set; }
        public int PackageMRP { get; set; }
        public int BookingAmount { get; set; }
        public decimal Comm { get; set; }
        public int CommType { get; set; }
        public int AmtType { get; set; }
        public int RoleID { get; set; }        
        public string ModifyDate { get; set; }
        public string SPKey { get; set; }
        public string BusinessModel { get; set; }
        public List<PackageDetail> PackageDetails { get; set; }
    }

    public class DTHCommissionModel
    {
        public int SlabID { get; set; }
        public bool IsAdminDefined { get; set; }
        public bool IsChannel { get; set; }
        public List<DTHCommission> DTHCommissions { get; set; }
        public List<DTHCommission> ParentSlabDetails { get; set; }
        public List<RoleMaster> Roles { get; set; }
        public List<DTHPackage> DTHPackage { get; set; }
        public List<OperatorDetail> Operators { get; set; }
        public int OID { get; set; }
    }

    public class DTHCommissionRequest : CommonReq
    {
        public DTHCommission Commission { get; set; }
    }

    public class DTHSlabDetailDisplay
    {
        public int PackageID { get; set; }
        public string PackageName { get; set; }
        public string Operator { get; set; }
        public string OpType { get; set; }
        public string SPKey { get; set; }        
        public string BusinessModel { get; set; }
        public decimal PackageMRP { get; set; }
        public decimal BookingAmount { get; set; }
        public List<SlabRoleCommission> RoleCommission { get; set; }
    }
}
