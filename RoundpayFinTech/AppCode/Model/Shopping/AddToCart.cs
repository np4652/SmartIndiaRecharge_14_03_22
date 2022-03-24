using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Model.Shopping
{
    public class AddToCartRequest
    {
        public int LoginID { get; set; }
        public int ProductID { get; set; }
        public int ProductDeatilID { get; set; }
        public int Quantity { get; set; }
        public string FilterIds { get; set; }
        public string OptionIds { get; set; }
    }
    public class RecentViewRequest
    {
        public int LoginID { get; set; }
     
        public int ProductDeatilID { get; set; }
    }
}
