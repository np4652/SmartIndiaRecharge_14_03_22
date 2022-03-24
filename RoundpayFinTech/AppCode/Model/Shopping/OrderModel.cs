using System.Collections.Generic;

namespace RoundpayFinTech.AppCode.Model.Shopping
{
    public class OrderModel:CommonFilter
    {
        public string OrderMode { get; set; } = "O";
        public string OrderDetailMode { get; set; } = "D";
        public int OrderId { get; set; }
        public int VendorId { get; set; }
        public int LoginId { get; set; }
        public int RequestedMode { get; set; }
        public int StatusID { get; set; }
        public int CategoryId { get; set; }
        public string CCMobile { get; set; }
        public string CCName { get; set; }
    }

    public class OrderList:SAddress
    {
        public int OrderId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal TotalCost { get; set; }
        public decimal TotalShipping { get; set; }
        public string DeliveryAddress { get; set; }
        public int Status { get; set; }
        public int PIN { get; set; }
        public string MobileNo { get; set; }
        public string LandMark { get; set; }
        public string RetailerName{ get; set; }
        public string OutletName{ get; set; }
        public string RetailerMobile{ get; set; }
        public decimal Opening{ get; set; }
        public decimal Closing{ get; set; }
        public decimal Deduction{ get; set; }
        public decimal Shipping{ get; set; }
        
    }

    public class OrderDetailList
    {
        public string EntryDate { get; set; }
        public decimal TotalCost { get; set; }
        public int UserId { get; set; }
        public int OrderDetailId { get; set; }
        public int OrderId { get; set; }
        public int ProductDetailId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal MRP { get; set; }
        public decimal Discount { get; set; }
        public string ProductName { get; set; }
        public string ImgUrl { get; set; }
        public string VendorName{ get; set; }
        public bool IsPaid{ get; set; }
        public bool IsOrderClosed{ get; set; }
        public int OrderStatus{ get; set; }
    }

    public class OrderHistory
    {
        public int Role { get; set; }
        public IEnumerable<OrderList> Orders { get; set; }
    }
    public class OrderDetail
    {
        public int Role { get; set; }
        public IEnumerable<OrderDetailList> OrderList { get; set; }

        public OrderDetail()
        {
            OrderList = new List<OrderDetailList>();
        }
    }
    public class AppOrderModel
    {
        public int StatusCode { get; set; }
        public string Msg { get; set; }
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public int Quantity { get; set; }
        public decimal TotalCost { get; set; }
        public string OrderDate { get; set; }
        public int Status { get; set; }
        public int RequestMode { get; set; }
        public decimal TotalShipping { get; set; }
        public decimal TotalMRP { get; set; }
        public decimal TotalDiscount { get; set; }
        public decimal TotalRequestedAmount { get; set; }
        public decimal TotalDebit { get; set; }
        public IEnumerable<AppOrderDetailModel> OrderDetailList { get; set; }
        public SAddress ShippingAddress { get; set; }
    }
    public class AppOrderDetailModel
    {
        public int OrderDetailId { get; set; }
        public int OrderId { get; set; }
        public decimal MRP { get; set; }
        public decimal Discount { get; set; }
        public bool DiscountType { get; set; }
        public int Quantity { get; set; }
        public int Status { get; set; }
        public bool IsPaid { get; set; }
        public bool IsOrderClosed { get; set; }
        public decimal SellingPrice { get; set; }
        public decimal ShippingCharge { get; set; }
        public int ShippingMode { get; set; }
        public int ProductId { get; set; }
        public int ProductDetailId { get; set; }
        public string ProductName { get; set; }
        public string ProductImage { get; set; }
    }

    public class AppOrderResp
    {
        public int StatusCode { get; set; }
        public string Msg { get; set; }
        public IEnumerable<AppOrderModel> Order { get; set; }
    }

    public class OrderStatusResp : ResponseStatus
    {
        public List<DeliveryAlert> deliveryAlerts { get; set; }
    }

    public class DeliveryAlert
    {
        public int DPId { get; set; }
        public int OrderDetailId { get; set; }
        public string Token { get; set; }
        public string VendorLat { get; set; }
        public string VendorLong { get; set; }
    }
}
