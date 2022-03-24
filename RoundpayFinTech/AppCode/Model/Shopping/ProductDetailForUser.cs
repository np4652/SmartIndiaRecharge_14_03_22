using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Model.Shopping
{
    public class ProductDetailForUser
    {
        public string Error { get; set; }
        public int ProductID { get; set; }        
        public int ProductDetailID { get; set; }
        public IEnumerable<string> Files { get; set; }
        public IEnumerable<FilterWithOptions> FilterDetail { get; set; }
        public IEnumerable<FilterOption> selectedOption { get; set; }
        public int Quantity { get; set; }
        public decimal ShippingCharges { get; set; }
        public List<string> ImgList { get; set; }
        public decimal MRP { get; set; }
        public decimal Discount { get; set; }
        public bool DiscountType { get; set; }
        public decimal SellingPrice { get; set; }
        public string ProductName{ get; set; }
        public string Specification { get; set; }
        public string Discription { get; set; }
        public string CommonDiscription { get; set; }
        public string AdditionalTitle { get; set; }
    }
}
