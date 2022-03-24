using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Model.Shopping
{
    public class ProductFilterDetail
    {
        public int ID { get; set; }
        public int ProductID { get; set; }
        public int ProductDetailID { get; set; }
        public int FilterID { get; set; }
        public int FilterOptionID { get; set; }
    }
}
