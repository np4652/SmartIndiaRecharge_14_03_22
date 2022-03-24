using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.Model.Shopping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Model.App
{
    #region Shoping For App
    public class AllAvailableFilterReq : AppSessionReq
    {
        public int CID { get; set; }
        public int SID { get; set; }
        public int S2ID { get; set; }
    }

    public class AllAvailableFilterRes : AppResponse
    {
        public IEnumerable<FilterWithOptions> Filters { get; set; }
        public IEnumerable<Brand> Brands { get; set; }
    }
    public class ShippingAddressResponse : AppResponse
    {
        public IEnumerable<ShippingAddress> Addresses { get; set; }
    }

    public class AddAddressRequest : AppSessionReq
    {
        public int ID { get; set; }
        public string Title { get; set; }
        public string CustomerName { get; set; }
        public string MobileNo { get; set; }
        public string Address { get; set; }
        public int CityID { get; set; }
        public int StateID { get; set; }
        public int PIN { get; set; }
        public string Landmark { get; set; }
        public string Area { get; set; }
        public bool IsDefault { get; set; }
        public bool IsDeleted { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public bool IsB2C { get; set; }
    }

    public class AddShippingAddressRes : AppResponse
    {
        public int Id { get; set; }
    }
    public class ShoppingMenuResponse : AppResponse
    {
        public int DefaultMenuLevel { get; set; }
        public IEnumerable<ShoppingMenu> GetShoppingMenu { get; set; }
    }

    public class ProductFilterRequest : AppSessionReq
    {
        public int CategoryID { get; set; }
        public int SubCategoryID1 { get; set; }
        public int SubCategoryID2 { get; set; }
        public List<string> Filters { get; set; }
        public string BrandIDs { get; set; }
    }

    public class ExtendProductFilterRequest : ProductFilterRequest
    {
        public string FilterIds { get; set; }
        public string OptionIds { get; set; }
    }

    public class ProductDetailForApp
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public int ProductDetailID { get; set; }
        public string ProductCode { get; set; }
        public string Batch { get; set; }
        public string VendorName { get; set; }
        public string BrandName { get; set; }
        public decimal MRP { get; set; }
        public decimal SellingPrice { get; set; }
        public decimal Discount { get; set; }
        public bool DiscountType { get; set; }
        public decimal ShippingCharges { get; set; }
        public int Quantity { get; set; }
        public string ImgUrl { get; set; }
        public string AdditionalTitle { get; set; }
    }

    public class ProductResponse : AppResponse
    {
        public IEnumerable<ProductDetailForApp> Products { get; set; }
        public IEnumerable<FilterWithOptions> Filters { get; set; }
    }
    public class ProductDescriptionRequest : AppSessionReq
    {
        public int ProductID { get; set; }
        public int ProductDetailID { get; set; }
    }

    public class ProductDescriptionResponse : AppResponse
    {
        public int ProductID { get; set; }
        public int ProductDetailID { get; set; }
        public IEnumerable<string> Files { get; set; }
        public IEnumerable<FilterOption> selectedOption { get; set; }
        public IEnumerable<FilterWithOptions> FilterDetail { get; set; }
        public int Quantity { get; set; }
        public decimal ShippingCharges { get; set; }
        public decimal MRP { get; set; }
        public decimal Discount { get; set; }
        public bool DiscountType { get; set; }
        public decimal SellingPrice { get; set; }
        public string ProductName { get; set; }
        public string Specification { get; set; }
        public string Discription { get; set; }
        public string CommonDiscription { get; set; }
        public string AdditionalTitle { get; set; }
    }
    public class OnFilterChnageRequest : AppRequest
    {
        public int ProductID { get; set; }
        public int ProductDetailID { get; set; }
        public List<string> Filters { get; set; }
    }

    public class OnFilterChnageResponse : AppResponse
    {
        public int ProductDetailID { get; set; }
        public int Quantity { get; set; }
        public decimal SellingPrice { get; set; }
        public decimal Mrp { get; set; }
        public List<string> Files { get; set; }
    }

    public class AddToCartReq : AppSessionReq
    {
        public int ProductID { get; set; }
        public int ProductDetailID { get; set; }
        public int Quantity { get; set; }
        public bool IsB2C { get; set; }
    }

    public class AddToCartResponse : AppResponse
    {
        public int TotalItem { get; set; }
    }

    public class CartDetailResponse : AppResponse
    {
        public IEnumerable<CartDetailForApp> CartDetail { get; set; }
    }

    public class CartDetailForApp
    {
        public int ID { get; set; }
        public int ProductID { get; set; }
        public int ProductDetailID { get; set; }
        public int Quantity { get; set; }
        public string ProductCode { get; set; }
        public string Batch { get; set; }
        public decimal MRP { get; set; }
        public decimal SellingPrice { get; set; }
        public decimal Discount { get; set; }
        public bool DiscountType { get; set; }
        public string ProductName { get; set; }
        public string ImgUrl { get; set; }
    }

    public class ItemInCartResponse : AppResponse
    {
        public decimal TCost { get; set; }
        public int TQuantity { get; set; }
    }

    public class ProceedToPayResponse : AppResponse
    {
        public int Quantity { get; set; }
        public int PrimaryDeductionPer { get; set; }
        public decimal TotalCost { get; set; }
        public decimal TotalMRP { get; set; }
        public decimal TotalDiscount { get; set; }
        public decimal PDeduction { get; set; }
        public decimal SDeduction { get; set; }
        public decimal ShippingCharge { get; set; }
        public string PWallet { get; set; }
        public string SWallet { get; set; }

        public ShippingAddress Address { get; set; }
        public IEnumerable<CartDetailForApp> CartDetails { get; set; }
        public List<ShippingAddress> AddressList { get; set; }
    }

    public class RemoveFromCartReq : AppSessionReq
    {
        public int ID { get; set; }
        public bool IsB2C { get; set; }
    }

    public class PlaceOrderReq : AppSessionReq
    {
        public int AddressID { get; set; }
        public bool IsB2C { get; set; }
    }

    public class ChangeQuantityReq : AppSessionReq
    {
        public int ItemID { get; set; }
        public int Quantity { get; set; }
        public bool IsB2C { get; set; }
    }

    public class ChangeQuantityResponse : AppResponse
    {
        public string Cost { get; set; }
    }

    public class OrderListResponse : AppResponse
    {
        public IEnumerable<OrderList> Orders { get; set; }
    }

    public class OrderDetailReq : AppSessionReq
    {
        public int OrderId { get; set; }
        public bool IsB2C { get; set; }
    }

    public class OrderDeatilResponse : AppResponse
    {
        public IEnumerable<OrderDetailList> OrderDetail { get; set; }
    }

    public class ChangeOrderStatusReq : AppSessionReq
    {
        public int OrderId { get; set; }
        public int Status { get; set; }
    }
    public class GetStatesResponse : AppResponse
    {
        public IEnumerable<StateMaster> States { get; set; }
    }

    public class CityReq : AppRequest
    {
        public int StateID { get; set; }
        public bool IsB2C { get; set; }
    }
    public class GetCityResponse : AppResponse
    {
        public IEnumerable<City> Cities { get; set; }
    }

    public class ShoppingPincodeDetail 
    {
        public int Statuscode { get; set; }
        public string Msg { get; set; }
        public int ID { get; set; }
        public int CityId { get; set; }
        public string City { get; set; }
        public string Area { get; set; }
        public string Districtname { get; set; }
        public int StateId { get; set; }
        public string Statename { get; set; }
        public string Pincode { get; set; }
        public int ReachInHour { get; set; }
        public int ExpectedDeliverInDays { get; set; }
        public bool IsDeliveryOff { get; set; }
    }

    public class GetAreabyPincodeResp : AppResponse
    {
        public IEnumerable<ShoppingPincodeDetail> Data { get; set; }
    }

    public class VendorLocationReq : AppSessionReq
    {
        public int VendorId { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
    }

    public class VendorLocationResp : AppResponse
    {
        public int VendorId { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
    }

    public class DPLocationReq : AppSessionReq
    {
        public int Id { get; set; }
        public int Status { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public int OrderDetailId { get; set; }
    }

    public class DPLoginResp : AppResponse
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Mobile { get; set; }
        public string VehicleNumber { get; set; }
    }

    public class LoginDeliveryPersonnelReq : AppRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Token { get; set; }
        public string IP { get; set; }
        public string Browser { get; set; }
        public int RequestMode { get; set; }

    }

    public class LoginDeliveryPersonnelResp : DeliveryPersonnel
    {
        public int Statuscode { get; set; }
        public int UserID { get; set; }
        public string Msg { get; set; }
        public int SessID { get; set; }
        public string SessionID { get; set; }
        public string CookieExpire { get; set; }
        public string OTP { get; set; }
        public bool IsAppValid { get; set; }
        public bool IsVersionValid { get; set; }
    }

    public class DPToken : AppSessionReq
    {
        public string Token { get; set; }
    }

    public class OrderDeliveryReq : AppSessionReq
    {
        public int OrderDetailId { get; set; }
    }

    public class AppOrderDeliveryResp : AppResponse
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int OrderDetailId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal TotalAmount { get; set; }
        public int CustomerAddressId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerAddress { get; set; }
        public string CustomerPinCode { get; set; }
        public string CustomerMobile { get; set; }
        public string CustomerArea { get; set; }
        public string CustomerLandmark { get; set; }
        public string CustomerLat { get; set; }
        public string CustomerLong { get; set; }
        public string VendorOutlet { get; set; }
        public string VendorMobile { get; set; }
        public string VendorAddress { get; set; }
        public string VendorLat { get; set; }
        public string VendorLong { get; set; }
        public int DPId { get; set; }
        public string DPLat { get; set; }
        public string DPLong { get; set; }
        public bool IsPicked { get; set; }
        public bool IsDelivered { get; set; }
    }

    public class AppDeliveryDashBoardResp : AppResponse
    {
        public int Status { get; set; }
        public string Msg { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; }
        public string Mobile { get; set; }
        public bool IsAssigned { get; set; }
        public bool IsAvailable { get; set; }
        public List<OrderDeliveryResp> OrderList { get; set; }
    }
    #endregion
}
