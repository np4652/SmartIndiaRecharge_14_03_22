using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.Shopping;
using RoundpayFinTech.AppCode.Model.Shopping.WebShopping.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Model.ShopingWebSite
{
    public class ShoppingDashBoardInfo
    {
        public int Id { get; set; }
        public bool Status { get; set; }
        public string Message { get; set; }
        public RoundpayFinTech.AppCode.Model.Shopping.WebShopping.ViewModel.Data Data { get; set; }
        
    }
    public class ShoppingWebSiteLists: NewArrivalOnSaleProducts
    {
        public List<ProductsList> BannerProductsFront { get; set; }
        public List<ProductsList> BannerProductsRight { get; set; }
        public IEnumerable<TopCategories> topcategories { get; set; }
        public IEnumerable<Menu> menus { get; set; }
        
      
    }

    public class ShoppingWebsiteCartDetails 
    {
        public int Statuscode { get; set; }
        public string Msg { get; set; }
        public List<CartDetail> CartDetail { get; set; }
        public ItemInCart ItemInCart { get; set; }
    }

}
