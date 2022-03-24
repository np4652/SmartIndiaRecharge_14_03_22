using System.Collections.Generic;

namespace RoundpayFinTech.AppCode.Model.Shopping
{
    public class OnChangeFilter
    {
        public int Statuscode { get; set; }
        public string Msg { get; set; }
        public int ProductDetailID { get; set; }
        public int Quantity { get; set; }
        public decimal SellingPrice { get; set; }
        public decimal Mrp{ get; set; }
        public List<string> Files { get; set; }
    }
}
