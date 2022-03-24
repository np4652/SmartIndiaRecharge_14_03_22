using Fintech.AppCode.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Model
{
    public class AffiliateCategory : CommonReq
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public bool IsActive { get; set; }
    }

    public class AffiliateVendorCommission:CommonReq
    {
        public int OID { get; set; }
        public string Operator { get; set; }
        public decimal Commission { get; set; }
        public int AmtType { get; set; }
    }

    public class AfVendorCommission
    {
        public int VendorID { get; set; }
        public IEnumerable<AffiliateVendorCommission> Commissions { get; set; }
    }
}
