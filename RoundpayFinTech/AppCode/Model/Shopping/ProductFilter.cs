using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Model.Shopping
{
    public class ProductFilter
    {
        public int UserId { get; set; }
        public int LoginTypeId { get; set; }
        public int CategoryID { get; set; }
        public int SubCategoryID1 { get; set; }
        public int SubCategoryID2 { get; set; }
        public int ProductId { get; set; }
        public int ProductDetailId { get; set; }
        public string FilterIds { get; set; }
        public string OptionIds { get; set; }
        public List<string> Filters { get; set; }
        public string BrandIDs { get; set; }
        public string PriceRange { get; set; }
    }
}
