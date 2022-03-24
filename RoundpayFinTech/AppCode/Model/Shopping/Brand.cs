

using RoundpayFinTech.AppCode.Model.Shopping.WebShopping.ViewModel;
using System.Collections.Generic;

namespace RoundpayFinTech.AppCode.Model.Shopping
{
    public class Brand
    {
        public int LT { get; set; }
        public int LoginID { get; set; }
        public int CategoryID { get; set; }
        public int BrandId { get; set; }
        public string BrandName { get; set; }
        public string CategoryName { get; set; }
        public bool IsActive { get; set; }
    }

    public class BrandEdit : Brand
    {
        public IEnumerable<Menu> Categories { get; set; }

        //public IEnumerable<ShoppingCategory> Categories { get; set; }
    }
}
