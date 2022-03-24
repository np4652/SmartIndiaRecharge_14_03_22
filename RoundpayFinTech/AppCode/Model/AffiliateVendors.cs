using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Model
{
    public class AffiliateVendors
    {
        public int Id { get; set; }
        public string VendorName { get; set; }
        public int Type { get; set; }
        public bool IsActive { get; set; }
        public string LastUpdatedOn { get; set; }
        
    }
}
