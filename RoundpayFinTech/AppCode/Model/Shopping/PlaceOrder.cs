
namespace RoundpayFinTech.AppCode.Model.Shopping
{
    public class PlaceOrder
    {
        public int UserID { get; set; }
        public int AddressID { get; set; }
        public int RequestMode { get; set; }
        public decimal PDeduction { get; set; }
        public decimal SDeduction { get; set; }
    }
}
