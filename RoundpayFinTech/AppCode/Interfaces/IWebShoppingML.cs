using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Microsoft.AspNetCore.Http;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.App;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.Model.Shopping;
using RoundpayFinTech.AppCode.Model.Shopping.WebShopping.ViewModel;
using System.Collections.Generic;
using System.Threading.Tasks;
using NewShoping = RoundpayFinTech.AppCode.Model.Shopping.WebShopping.ViewModel;

namespace RoundpayFinTech.AppCode.Interfaces
{
    interface IWebShoppingML
    {
        NewShoping.Data GetwebsiteInfo(string WebsiteId);
        List<MainCategoriesProduct> GetProductDetailByCategoryID(int id, int MainCategorieId, int CategoryID);
        Task<NewArrivalOnSaleProducts> NewArrival_OnSaleProductAsync(string WebsiteId);
        Task<AllProductInfo> QuickViewApiAsync(int POSId);
        SubCategoryListResponse GetAllSubCategoryList(int id, int CategoryID);
        Task<AllProductInfo> GetAllProductInfo(int POSId, string LoginId);
        Task<ProductOptionSetResponse> GetAllProductsetList(ProductSetInfo productSetInfo);
        List<SimilarProducts> GetAllSimilarItems(int POSId);
        Task<FilterListResponse> GetAllFilterList(FilterOptionRequest filterOptionRequest);
        List<KeywordList> GetKeywordList(string WebsiteId, string SearchKeyword);
        List<RecentViewModel> GetRecentViewItems(string WebsiteId, int CustomerId);
        List<WishListResponse> GetAllWishListItems(string WebsiteId, int CustomerId);
        Task<FilterListResponse> GetSearchKeyword(KeywordSearch keywordSearch);
        List<RecentViewModel> GetRecentViewItems(string BrowserId, string WebSiteId);
         CheckPinCodeStatus CheckDelivery(string PinCode);

        RoundpayFinTech.AppCode.Model.ResponseStatus AddRecentViewProduct(int UserID, string BrowserId, int ProductDetailID);
        ShoppingHeaderMenus ShoppingHeaderMenus(string WebsiteId);
        WebSite ShoppingWebsiteInfo(string WebsiteId);
        GetKeywordResponse ShoppingGetKeywordDetails(string Keyword = "");
    }
}
