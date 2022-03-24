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
    interface IShoppingML
    {
        #region ShoppingSetting
        IEnumerable<ShoppingPincodeDetail> GetAreaByPincode(int Pincode, int UserId = 0, int LoginTypeId = 0);
        ShoppingSettings GetShoppingSettings();
        #endregion
        #region Category
        ResponseStatus UpdateShoppingCategory(CommonReq req);
        ShoppingCategory GetShoppingCategoryByID(int id);     
        IEnumerable<ShoppingCategory> GetShoppingCategory(int id = 0);     
        #endregion

        #region Main Category New
        ResponseStatus UpdateShoppingMainCategoryNew(CommonReq req);
        NewShoping.Menu GetShoppingMainCategoryByIDNew(int id);
        IEnumerable<NewShoping.Menu> GetShoppingMainCategoryNew(int id = 0);
        #endregion

        #region Category New
        NewShoping.CategoryList GetShoppingCategoryByIDNew(int id);
        ResponseStatus UpdateShoppingCategoryNew(CommonReq req);
        IEnumerable<NewShoping.CategoryList> GetCategory(int cid = 0, int id = 0);

        #endregion

        #region Sub Category New
        ResponseStatus UpdateSubCategoryNew(CommonReq req);
        ShoppingSubCategoryLvl2 GetSubCategoryByIDNew(int id);
        IEnumerable<NewShoping.SubCategory> GetSubCategoryNew(int sid = 0, int id = 0);
        #endregion

        #region SubCategory Level-1
        ShoppingSubCategoryLvl1 GetShoppingSubCategoryByID(int id);
        ResponseStatus UpdateShoppingSubCategoryLvl1(CommonReq req);
        IEnumerable<ShoppingSubCategoryLvl1> GetSubCategoryLvl1(int cid = 0, int id = 0);
        #endregion

        #region SubCategory Level-2
        ResponseStatus UpdateSubCategoryLvl2(CommonReq req);
        ShoppingSubCategoryLvl2 GetSubCategoryLvl2ByID(int id);
        IEnumerable<ShoppingSubCategoryLvl2> GetSubCategoryLvl2(int sid = 0, int id = 0);
        #endregion

        #region SubCategory Level-3
        ResponseStatus UpdateSubCategoryLvl3(CommonReq req);
        IEnumerable<ShoppingSubCategoryLvl3> GetSubCategoryLvl3(int sid);
        ShoppingSubCategoryLvl3 GetSubCategoryLvl3ByID(int id);
        #endregion

        #region Filter
        ResponseStatus UpdateFilter(CommonReq req);
        Filter GetFilterByID(int id);
        IEnumerable<Filter> GetFilter();
        IEnumerable<Filter> GetFilterForMapping(int CID);
        ResponseStatus UpdateMappedFilter(CommonReq req);
        IEnumerable<UOM> GetUom();
        IEnumerable<Colors> GetColors();
        IEnumerable<FilterOption> GetFilterOption(int FilterID);
        ResponseStatus UpdateFilterOption(CommonReq req);
        FilterOption GetFilterOptionByID(int id);
        #endregion

        #region Product
        ResponseStatus AddMasterProduct(MasterProduct req);
        ResponseStatus AddProduct(ProductDetail req);
        IResponseStatus UploadProductImage(IFormFile file, int ProductID, int ProductDetailID, string ImgName, int count);
        IResponseStatus UploadIcon(IFormFile file, int id, string pathConcat = "", string fileNameConcat = "");
        IEnumerable<MasterProduct> GetMasterProduct(MasterProduct req);
        MasterProduct GetMasterProductById(int PID);
        IEnumerable<Brand> GetBrand(int cid, int sid = 0);
        IEnumerable<FilterWithOptions> GetRequeiredFilter(int cid, int sid, int sid2, string filters);
        IEnumerable<ProductDetail> GetProductDetails(int ProductID);
        ProductWithMaster GetAllProducts(int CID, int SID1, int SID2);
        IEnumerable<ProductDetail> GetProductForIndex(ProductFilter p);
        IEnumerable<ProductDetail> GetProductTrending(ProductFilter p);
        IEnumerable<ProductDetail> GetProductNewArrival(ProductFilter p);
        IEnumerable<ProductDetail> GetProductSimilar(ProductFilter p);
        IEnumerable<ProductDetail> GetFilteredProduct(ProductFilter p);
        IEnumerable<ProductDetail> GetTrendingProducts( );
        #endregion

        #region Commission
        IEnumerable<MasterRole> ShoppingCommissionRoles();
        ShoppingCommissionExtend GetShopppingSlabCommission(ShoppingCommissionReq req);
        ResponseStatus UpdateShoppingComm(ShoppingCommissionReq req);
        ResponseStatus GetUserCommission(int productDeatilId, int userId = 0);
        #endregion

        #region FrontEndImage
        BannerList GetBanners();
        FEImgList GetFEImageList(int id, int catId);
        IResponseStatus UploadFEImage(IFormFile file, CommonReq req);
        IResponseStatus UpdateFEImg(int id, bool isActive, bool isDelete);
        #endregion

        IEnumerable<FilterWithOptions> GetAvailableFilter(int PID, int PDetailId);
        ResponseStatus SaveBrand(Brand req);
        ResponseStatus SaveVendor(Vendors req);
        IEnumerable<ShoppingMenu> GetShoppingMenu();
        IEnumerable<Vendors> GetVendors();
        IEnumerable<VendorMaster> GetVendorList();
        ResponseStatus ChangeECommVendorStatus(string id, bool val);
        ResponseStatus GetECommVendorLocation(int id, int userid = 0);
        ResponseStatus UpdateECommVendorLocation(int id, string latitude, string longitude, int userid = 0);
        #region cart
        Task<ResponseStatus> AddToCart(int ProductDetailID, int Quantity, int UserID = 0);
        Task<IEnumerable<CartDetail>> CartDetail(int UserID = 0);
        Task<IEnumerable<RecentViewModel>> RecentViewDetails(int UserID = 0, string WebSiteID = "1");
        ItemInCart ItemInCart(int UserID = 0);
        Task<ResponseStatus> RemoveItemFromCart(int ID, int UserID = 0, int ProductDetailId = 0, bool RemoveAll = false);
        Task<ResponseStatus> ChangeQuantity(int ItemID, int Quantity, int UserID = 0);
        Task<ResponseStatus> ChangeQuantityByPdId(int PdId, int Quantity, int UserID = 0);
        ProceedToPay ProceedToPay(int UserID=0);
        Task<ResponseStatus> PlaceOrder(PlaceOrder order);
        Task<OnChangeFilter> OnChangeFilter(int ProductID, int ProductDeatilID, List<string> Filters);
        Task<ProductDetailForUser> OnChangeFilterForApp(int ProductID, int ProductDeatilID, List<string> Filters, int UserID = 0);
        Task<OnChangeFilter> OnPageChangeFilter(int ProductID, int ProductDeatilID, string Filters);
        Task<ProductDetailForUser> ProDescription(int ProductDeatilID, int UserID = 0);

        #endregion

        #region OrderDetail
        IEnumerable<OrderList> GetOrderHistory(OrderModel req);
        IEnumerable<OrderDetailList> GetOrderDetailList(CommonReq req);
        IEnumerable<OrderDetailList> getOrderDetails(int OrderID, int UserID = 0);
        IEnumerable<OrderReport> getOrderReport(OrderModel req);
        ResponseStatus ChangeOrderStatus(CommonReq req);
        IResponseStatus ChangePartialOrderStatus(ChangeOrderStatus req);
        #endregion
        ResponseStatus DeleteShippingAddress(int ID);
        ShippingAddress AddShippingAddress(SAddress param);
        Task<AddProductModal> AddProductModal(int ProductDetailID);
        IEnumerable<City> Cities(int StateID);
        IEnumerable<StateMaster> States();
        ResponseStatus AddToWishList(int ProductDetailID, int LoginId = 0);
        ResponseStatus RemoveFromWishList(int ProductDetailID,int LoginId, int id = 0, bool RemoveAll = false);
        ItemInCart WishListCount(int UserID = 0);
        Task<IEnumerable<CartDetail>> WishlistDetail(int UserID = 0);
        ResponseStatus DeleteProductDetail(int ProductDetailID, bool IsDeleted);
        ResponseStatus StockUpdation(int ProductDetailID, int Quantity, string Remark);
        IEnumerable<ShippingAddress> GetShippingAddresses(int UserID = 0);
        ShoppingShipping GetShippingAddress(int ID);
        ShoppingShipping GetShippingAddressByID(int ID);
        IEnumerable<Brand> GetBranddetail(int CategoryID);
        IEnumerable<Brand> GetBrandById(int BrandId = 0);
        IEnumerable<AppOrderModel> AppOrderList(CommonReq req);

        #region UserShopping
        UserInfoModel GetUserInfo();
        IEnumerable<CategoriesForIndex> GetCategoriesForUserIndex(ProductFilter p);
        #endregion

    }
}
