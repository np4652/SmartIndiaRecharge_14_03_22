using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Model.Shopping.WebShopping.ViewModel
{

    public class GetAllMenu
    {
        public List<Menu> maincategoryMenus { get; set; }
        public List<CategoryList> subCategoryMenus { get; set; }
        public List<ProductsList> BannerProductsFront { get; set; }
        public List<ProductsList> BannerProductsRight { get; set; }
        public List<WebSite> websiteSettings { get; set; }
    }

    public class GetQuickViewProduct
    {
        public List<AllProductInfo> AllProductInfo { get; set; }
        public List<FilterWithOption> FilterWithOption { get; set; }
        public AddressMasterResponse DefaultAddress { get; set; }

    }

    public class ShoppingHeaderMenus
    {
        public IEnumerable<Menu> menus { get; set; }
        public string Logo { get; set; }
    }







    public class Data
    {
        public int Id { get; set; }
        public IEnumerable<Menu> menus { get; set; }
        public IEnumerable<TopCategories> topcategories { get; set; }
        public IEnumerable<DealofWeek> dealofWeek { get; set; }
        public IEnumerable<MainCategoryProductList> mainCategoryProductList { get; set; }
        public WebSite websiteSettings { get; set; }
        public List<ProductsList> BannerProductsFront { get; set; }
        public List<ProductsList> BannerProductsRight { get; set; }
        public List<NewArrivalList> newArrivalList { get; set; }
        public List<NewArrivalList> onSaleList { get; set; }
        public List<NewArrivalList> bestSellerList { get; set; }
        public int WishListCount { get; set; }
        public string BrowserId { get; set; }
    }

    public class TopCategories
    {
        public int CategoryID { get; set; }
        public int ParentId { get; set; }
        public int MainCategoryID { get; set; }
        public string Name { get; set; }
        public string Image { get; set; }
        public List<SubCategoryMenus> subcategory { get; set; }
    }
    public class DealofWeek
    {
        public int Id { get; set; }
        public int POSId { get; set; }
        public int ProductId { get; set; }
        public Nullable<int> CrQty { get; set; }
        public Nullable<int> DrQty { get; set; }
        public Nullable<decimal> Mrp { get; set; }
        public Nullable<decimal> Discount { get; set; }
    }
    public class ApiClassDealofWeek : DealofWeek
    {
        public string ProductName { get; set; }
        public string ProductSetName { get; set; }
        public string SubCategoryName { get; set; }
        public string ImageUrl { get; set; }
    }
    public class MainCategoryProductList
    {
        public string BannerImage { get; set; }
        public string RedirectUrl { get; set; }
    }
    public class WebSite
    {
        public string WebsiteId { get; set; }
        public string BrandName { get; set; }
        public string WebsiteName { get; set; }
        public string Logo { get; set; }
        public int WhiteLabelId { get; set; }
        public bool IsRecharge { get; set; }
        public string Title { get; set; }
        public string Favicon { get; set; }
        public string CopyRightText { get; set; }
        public string MobileNo { get; set; }
        public string TelegramLink { get; set; }
        public string RechargeEmailId { get; set; }
        public string RechargeMobileNo { get; set; }
        public string RechargeLandLineNo { get; set; }
        public string RechargeWhatsappNo { get; set; }
        public string ShoppingWhatsappNo { get; set; }
        public string ShoppingLandLineNo { get; set; }
        public string EmailId { get; set; }
        public string Address { get; set; }
        public string FbLink { get; set; }
        public string InstagramLink { get; set; }
        public string TwitterLink { get; set; }
        public List<YoutubeLinks> YoutubeLinks { get; set; }
    }
    public class YoutubeLinks
    {
        public int Id { get; set; }
        public string VideoUrl { get; set; }
        public string VideoTitle { get; set; }
    }
    public class ProductsList
    {
        public string BannerImage { get; set; }
        public string RedirectUrl { get; set; }
    }
    public class CommonProcResp
    {
        public int Status { get; set; }
        public string Msg { get; set; }
    }
    public class NewArrivalList : CommonProcResp
    {
        public string SubCategoryName { get; set; }
        public string ProductName { get; set; }
        public string SetName { get; set; }
        public string FrontImage { get; set; }
        public string AdditionalTitle { get; set; }
        public string Title { get; set; }
        public string SmallImage { get; set; }
        public string AffiliateShareLink { get; set; }
        public string ShareLink { get; set; }
        public int POSId { get; set; }
        public int IsCartAdded { get; set; }
        public int SubCategoryId { get; set; }
        public int RemainingQuantity { get; set; }
        public decimal MRP { get; set; }
        public decimal SellingPrice { get; set; }
        public decimal Discount { get; set; }
    }
    public class NewArrivalOnSaleProducts
    {
        public List<NewArrivalList> NewArrivals { get; set; }
        public List<NewArrivalList> OnSale { get; set; }
        public List<NewArrivalList> BestSellerList { get; set; }
    }
    public class MainCategoriesProduct : CommonProcResp
    {
        public int Id { get; set; }
        public int POSId { get; set; }
        public int MainCategoryId { get; set; }
        public int ProductId { get; set; }
        public int CategoryId { get; set; }
        public string MainCategoryName { get; set; }
        public int SubCategoryId { get; set; }
        public int RemainingQuantity { get; set; }
        public int IsCartAdded { get; set; }
        public string SubCategoryName { get; set; }
        public string ProductName { get; set; }
        public string Title { get; set; }
        public string FrontImage { get; set; }
        public string SmallImage { get; set; }
        public string AffiliateShareLink { get; set; }
        public string ShareLink { get; set; }
        public decimal MRP { get; set; }
        public decimal SellingPrice { get; set; }
        public decimal Discount { get; set; }
    }
    public class AllProductInfo
    {
        public string SetName { get; set; }
        public int POSId { get; set; }
        public int ProductDetailID { get; set; }
        public List<KeyValyePairs> SpecificFeatures { get; set; }
        public string Title { get; set; }
        public string AdditionalDescription { get; set; }
        public string ProductName { get; set; }
        public string ProductCode { get; set; }
        public string SubCategoryName { get; set; }
        public int SubCategoryId { get; set; }
        public string CategoryName { get; set; }
        public int CategoryID { get; set; }
        public int IsCartAdded { get; set; }
        public int IsWishlistAdded { get; set; }
        public string MaincategoryName { get; set; }
        public int MainCategoryID { get; set; }
        public string Description { get; set; }
        public string Specification { get; set; }
        public string FrontImage { get; set; }
        public string FrontImage_1200 { get; set; }
        public string FrontImage_100 { get; set; }
        public string SmallImage { get; set; }
        public string BackImage { get; set; }
        public string BackImage_1200 { get; set; }
        public string BackImage_100 { get; set; }
        public string SideImage { get; set; }
        public string SideImage_1200 { get; set; }
        public string SideImage_100 { get; set; }
        public string AffiliateShareLink { get; set; }
        public string ShareLink { get; set; }
        public int ProductOptionSetId { get; set; }
        public decimal MRP { get; set; }
        public decimal Discount { get; set; }
        public decimal SellingPrice { get; set; }
        public decimal ShippingCharge { get; set; }
        public int RemainingQuantity { get; set; }
        public DateTime ExpectedDeliveryDate { get; set; }
        public List<FilterList> filters { get; set; }
        public AddressMasterResponse DefaultAddress { get; set; }


    }
    public class AddressMasterResponse : CommonProcResp
    {
        public int AddressId { get; set; }
        public int LoginId { get; set; }
        public string MobileNo { get; set; }
        public string Name { get; set; }
        public string AlternateMobileNo { get; set; }
        public string Pincode { get; set; }
        public string Area { get; set; }
        public string Address { get; set; }
        public string State { get; set; }
        public string City { get; set; }
        public string LandMark { get; set; }
        public string EmailId { get; set; }
        public string AddressType { get; set; }
        public bool IsDefault { get; set; }
    }
    public class FilterList
    {
        public string name { get; set; }
        public List<OptionList> option_Lists { get; set; }
    }
    public class KeyValyePairs
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }
    public class FilterWithOption : OptionList
    {
        public string FilterName { get; set; }
    }
    public class OptionList
    {
        public int Id { get; set; }
        public int POSID { get; set; }
        public string OptionName { get; set; }
        public string FrontImage_200 { get; set; }
        public bool IsSelected { get; set; }
    }
    public class SubCategoryListResponse
    {
        public List<SubCategoryProcResp> SubCategoryRepository { get; set; }
        public string CategoryName { get; set; }
        public int CategoryID { get; set; }
        public string MainCategoryName { get; set; }
        public int MainCategoryID { get; set; }
    }
    public class SubCategoryProcResp
    {
        public int SubCategoryId { get; set; }
        public string SubcategoryName { get; set; }
        public string Image { get; set; }
        public string MainCategoryName { get; set; }
        public int MainCategoryID { get; set; }
        public string CategoryName { get; set; }
        public int CategoryID { get; set; }
    }
    public class ProductSetInfo
    {
        public List<string> FilterOptionTypeIdList { get; set; }
        public int StartIndex { get; set; }
        public int PageLimitIndex { get; set; }
        public string OrderBy { get; set; }
        public string OrderByType { get; set; }
        public string WebsiteId { get; set; }
        public string FilterType { get; set; }
        public int FilterTypeId { get; set; }
        public int KeywordId { get; set; }
    }
    public class ProductOptionSetInfoList
    {
        public int POSId { get; set; }
        public string Title { get; set; }
        public string SetName { get; set; }
        public string ProductName { get; set; }
        public string SubCatName { get; set; }
        public int SubCategoryId { get; set; }
        public string CategoryName { get; set; }
        public string MainCategoryName { get; set; }
        public int MainCategoryID { get; set; }
        public int CategoryId { get; set; }
        public int RemainingQuantity { get; set; }
        public decimal SellingPrice { get; set; }
        public decimal MRP { get; set; }
        public decimal Discount { get; set; }
        public int IsCartAdded { get; set; }
        public string FrontImage { get; set; }
        public string SmallImage { get; set; }
        public string Description { get; set; }
        public string AffiliateShareLink { get; set; }
        public string ShareLink { get; set; }
        public int TotalRecords { get; set; }
    }
    public class ProductOptionSetResponse
    {
        public object ProductSetList { get; set; }
        public int TotalRecords { get; set; }
        public int CurrentIndex { get; set; }
        public int PageLimitIndex { get; set; }
    }
    public class SimilarProducts : CommonProcResp
    {
        public int ProductId { get; set; }
        public int POSId { get; set; }
        public string Title { get; set; }
        public string FrontImage { get; set; }
        public string SmallImage { get; set; }
        public string ProductName { get; set; }
        public string SubcategoryName { get; set; }
        public string AffiliateShareLink { get; set; }
        public string ShareLink { get; set; }
        public int SubcategoryId { get; set; }
        public int RemainingQuantity { get; set; }
        public decimal MRP { get; set; }
        public decimal SellingPrice { get; set; }
        public decimal Discount { get; set; }
    }
    public class FilterOptionRequest
    {
        public int FilterTypeId { get; set; }
        public string FilterType { get; set; }
        public string WebsiteId { get; set; }
    }
    public class FilterOptionList
    {
        public string FilterName { get; set; }
        public int FilterId { get; set; }
        public string OptionName { get; set; }
        public string FrontImage_200 { get; set; }
        public int FilterOptionTypeId { get; set; }
        public int POSID { get; set; }
        public int CategoryId { get; set; }
    }
    public class FilterListResponse
    {
        public List<FilterList> filterLists { get; set; }
        public int CategoryId { get; set; }
    }
    public class KeywordList
    {
        public int KeywordId { get; set; }
        public string Keyword { get; set; }
        public string SubcategoryName { get; set; }
        public string Image { get; set; }
        public int SubcategoryId { get; set; }
        public int ProductDetailId { get; set; }
        public string ProductImage { get; set; }
        public int ProductId { get; set; }
    }
    public class RecentViewModel
    {
        public int ProductId { get; set; }
        public int ProductDetailID { get; set; }

        public string ProductName { get; set; }
        public int POSId { get; set; }
        public int RemainingQuantity { get; set; }
        public int IsCartAdded { get; set; }
        public string BrowserId { get; set; }
        public string WebsiteId { get; set; }
        public string Title { get; set; }
        public string FrontImage { get; set; }
        public string SmallImage { get; set; }
        public string SubcategoryName { get; set; }
        public string AffiliateShareLink { get; set; }
        public string ShareLink { get; set; }
        public int SubcategoryId { get; set; }
        public decimal MRP { get; set; }
        public decimal SellingPrice { get; set; }
        public decimal Discount { get; set; }
    }
    public class WishList
    {
        public int POSId { get; set; }
        public int LoginId { get; set; }
        public string WebsiteId { get; set; }
    }
    public class WishListResponse : CommonProcResp
    {
        public int ProductId { get; set; }
        public int POSId { get; set; }
        public int WishListID { get; set; }
        public int RemainingStock { get; set; }
        public int IsCartAdded { get; set; }
        public string Title { get; set; }
        public string FrontImage { get; set; }
        public string SmallImage { get; set; }
        public string SetName { get; set; }
        public string AffiliateShareLink { get; set; }
        public string ShareLink { get; set; }
        public decimal MRP { get; set; }
        public decimal Discount { get; set; }
        public decimal SellingPrice { get; set; }
    }
    public class KeywordSearch
    {
        public string WebsiteId { get; set; }
        public string SearchKeyword { get; set; }
        public int StartIndex { get; set; }
        public int EndIndex { get; set; }
        public string LoginId { get; set; }
    }
    public class CheckPinCodeStatus
    {
        public string City { get; set; }
        public string State { get; set; }

    }
    public class PostalCode
    {
        public string district { get; set; }
        public int pin { get; set; }
        public double max_amount { get; set; }
        public string pre_paid { get; set; }
        public string cash { get; set; }
        public string pickup { get; set; }
        public string repl { get; set; }
        public string cod { get; set; }
        public string country_code { get; set; }
        public string sort_code { get; set; }
        public string is_oda { get; set; }
        public string state_code { get; set; }
        public double max_weight { get; set; }
    }
    public class DeliveryCode
    {
        public PostalCode postal_code { get; set; }
    }
    public class RootObject
    {
        public List<DeliveryCode> delivery_codes { get; set; }
    }
    public class DelhiveryAppSetting
    {
        public string Token { get; set; }
        public string BaseURL { get; set; }
    }

    public class GetKeywordResponse
    {
        public int MainCategoryId { get; set; }
        public string MainCategoryName { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int SubCategoryId { get; set; }
        public string SubCategoryName { get; set; }
    }

}
