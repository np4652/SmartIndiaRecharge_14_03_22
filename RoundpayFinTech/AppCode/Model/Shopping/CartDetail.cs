
using System.Collections.Generic;

namespace RoundpayFinTech.AppCode.Model.Shopping
{
    public class CartDetail
    {
        public int ID { get; set; }
        public int UserID { get; set; }
        public int ProductID { get; set; }
        public int ProductDetailID { get; set; }
        public int Quantity { get; set; }
        public string ProductCode { get; set; }
        public string Batch { get; set; }
        public decimal MRP { get; set; }
        public decimal SellingPrice { get; set; }
        public decimal Discount { get; set; }
        public bool DiscountType { get; set; }
        public string Description { get; set; }
        public string ProductName { get; set; }
        public string ImgUrl { get; set; }
        public string AdditionalTitle { get; set; }
    }

    public class ItemInCart
    {
        public int Statuscode { get; set; }
        public string Msg { get; set; }
        public int TQuantity { get; set; }
        public decimal TCost { get; set; }
    }

    public class ECommUserDetail
    {
        public List<AddToCartRequest> CartDetail { get; set; }
        public List<int> Wishlist { get; set; }
        public List<RecentViewRequest> RecentView { get; set; }
    }
   
}
