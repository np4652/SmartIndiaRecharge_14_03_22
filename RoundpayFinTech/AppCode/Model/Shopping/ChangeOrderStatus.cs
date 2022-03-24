using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Model.Shopping
{
    public class ChangeOrderStatus
    {
        public int LT { get; set; }
        public int LoginID { get; set; }
        public int OrderID { get; set; }
        public int OrderDetailID { get; set; }
        public int StatusID { get; set; }
    }
}
