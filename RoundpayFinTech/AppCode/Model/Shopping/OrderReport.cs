using RoundpayFinTech.AppCode.Model.Shopping.WebShopping.ViewModel;
using System.Collections.Generic;

namespace RoundpayFinTech.AppCode.Model.Shopping
{
    public class OrderReport
    {

        public int SrNo { get; set; }
        public int Statuscode { get; set; }
        public string Msg { get; set; }
        public int ProductDetailID { get; set; }
        public int ProductID { get; set; }
        public string ProductImage { get; set; }
        public int OrderDetailID { get; set; }
        public int OrderID { get; set; }
        public string OrderDate { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string Address { get; set; }
        public string ProductCode { get; set; } 
        public decimal DebitAmount { get; set; }     
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal MRP { get; set; }
        public decimal Discount { get; set; }
        public decimal SellingPrice { get; set; }
        public decimal ShippingCharge { get; set; }
        public decimal RequestAmount { get; set; }
        public decimal UserCommAmount { get; set; }
        public decimal GSTRate { get; set; }
        public decimal GSTAmt { get; set; }
        public decimal TDSRate { get; set; }
        public decimal TDSAmt { get; set; }
        public decimal PDeduction { get; set; }
        public decimal AdminCommission { get; set; }
        public decimal RetailCommission { get; set; }
        public decimal TeamCommission{ get; set; }
        public string VendorName{ get; set; }     
        public decimal VendorPayble { get; set; }
        public bool IsVendorPaid { get; set; }
        public string Status { get; set; }
        public int OrderStatusID { get; set; }
        public int Role { get; set; }
     
        public decimal SDeduction { get; set; }
        
     
       

        public IEnumerable<Menu> Category { get; set; }
        public IEnumerable<Vendors> Vendors { get; set; }

    }

    public class OrderReportModel
    {
        public int Role { get; set; }
        public IEnumerable<OrderReport> OrderReport { get; set; }
    }

    public class ShoppingShipping : OrderReport
    {
        public int id { get; set; }
        public string CustomerName { get; set; }
        public string Address { get; set; }
        public string Area { get; set; }
        public string LandMark { get; set; }
        public string MobileNo { get; set; }
        public int CityId { get; set; }
        public int StateId { get; set; }
        public string CityName { get; set; }
        public string Title { get; set; }
        public string StateName { get; set; }
        public string Pin { get; set; }

    }
}
