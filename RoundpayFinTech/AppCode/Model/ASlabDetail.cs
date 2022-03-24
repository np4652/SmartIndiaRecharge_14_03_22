using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Model
{
    public class ASlabDetail:CommonReq
    {
        public int ID { get; set; }
        public int RoleID { get; set; }
        public int VendorID { get; set; }
        public string VendorName { get; set; }
        public int SLABID { get; set; }
        public int OID { get; set; }
        public int AmtType { get; set; }
        public int CommType { get; set; }
        public decimal CommAmount { get; set; }
        public string ModifyDate { get; set; }
    }

    public class ASlabDetailModel
    {
        public int SlabID { get; set; }
        public int RoleID { get; set; }
        public int OID { get; set; }
        public bool IsAdminDefined { get; set; }
        public IEnumerable<ASlabDetail> CommissitionDetail { get; set; }
        public IEnumerable<ASlabDetail> ParentDetail { get; set; }
        public IEnumerable<AffiliateCategory> AfCategories { get; set; }
        public IEnumerable<RoleMaster> Roles { get; set; }        
        public IEnumerable<AffiliateVendors> Vendors { get; set; }

        public IEnumerable<OperatorDetail> Operators { get; set; }
    }
}
