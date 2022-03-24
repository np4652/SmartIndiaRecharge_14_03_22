using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Model.Shopping
{
    public class VendorMaster
    {
        public int Id { get; set; }
        public string VendorName { get; set; }
        public bool IsActive { get; set; }
        public int VendorUserID { get; set; }
        public bool IsB2BAllowed { get; set; }
        public bool IsOnlyB2B { get; set; }
    }
}
