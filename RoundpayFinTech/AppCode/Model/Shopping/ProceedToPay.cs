using Microsoft.AspNetCore.Mvc.Rendering;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System.Collections.Generic;

namespace RoundpayFinTech.AppCode.Model.Shopping
{
    public class ProceedToPay
    {
        public int Statuscode { get; set; }
        public string Msg { get; set; }
        public int Quantity { get; set; }
        public int PrimaryDeductionPer { get; set; }
        public decimal TotalSellingPrice { get; set; }
        public decimal TotalCost { get; set; }
        public decimal TotalMRP { get; set; }
        public decimal TotalDiscount { get; set; }
        public decimal PDeduction { get; set; }
        public decimal SDeduction { get; set; }
        public decimal ShippingCharge { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public int PINCode { get; set; }
        public string UserName { get; set; }
        public string ContactNumber { get; set; }
        public string PWallet { get; set; }
        public string SWallet { get; set; }
        public IEnumerable<ShippingAddress> Addresses { get; set; }
        public IEnumerable<CartDetail> CartDetails { get; set; }

    }

    public class ShippingAddress
    {
        public int Statuscode { get; set; }
        public string Msg { get; set; }
        public int ID { get; set; }
        public int UserId { get; set; }
        public string Address { get; set; }
        public string CustomerName { get; set; }
        public string Title { get; set; }
        public string City { get; set; }
        public string AddressOnly { get; set; }
        public string PinCode { get; set; }
        public string Landmark { get; set; }
        public string MobileNo { get; set; }
        public bool IsDefault { get; set; }
        public string Area { get; set; }
        public int CityId { get; set; }
        public int StateId { get; set; }
        public string State { get; set; }
        public bool IsDeleted { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
    }


    public class City
    {
        public int CityID { get; set; }
        public string CityName { get; set; }
    }

    public class ShippingAddressModal
    {
        public ShoppingShipping Shipping { get; set; }
        public IEnumerable<City> Cities { get; set; }
        public IEnumerable<StateMaster> States { get; set; }
    }
}
