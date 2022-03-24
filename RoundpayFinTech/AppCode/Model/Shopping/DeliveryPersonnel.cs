

using RoundpayFinTech.AppCode.Model.App;
using System.Collections.Generic;

namespace RoundpayFinTech.AppCode.Model.Shopping
{
    public class DeliveryPersonnel
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Mobile { get; set; }
        public string DOB { get; set; }
        public string Address { get; set; }
        public string Area { get; set; }
        public int CityId { get; set; }
        public string City { get; set; }
        public string Pincode { get; set; }
        public string Location { get; set; }
        public string VehicleNumber { get; set; }
        public string Aadhar { get; set; }
        public string DLId { get; set; }
        public bool IsActive { get; set; }
        public string Password { get; set; }
    }

    public class DeliveryPersonnelList
    {
        public int Status { get; set; }
        public string Msg { get; set; }
        public List<DeliveryPersonnel> DeliveryPersonnels { get; set; }
    }

    public class AUDeliverPersonnel : DeliveryPersonnel
    {
        public int Status { get; set; }
        public string Msg { get; set; }
        public int LT { get; set; }
        public int LoginID { get; set; }
        public List<City> Cities { get; set; }
    }

    public class LoginDeliveryPersonnel : DeliveryPersonnel
    {
        public int Status { get; set; }
        public string Msg { get; set; }
        public int SessId { get; set; }
        public string SessionKey { get; set; }
        public string CookieExpire { get; set; }
    }

    public class OrderDeliveryResp
    {
        public int Status { get; set; }
        public string Msg { get; set; }
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
        public string DPName { get; set; }
        public string DPMobile { get; set; }
        public string DPLat { get; set; }
        public string DPLong { get; set; }
        public bool IsPicked { get; set; }
        public bool IsDelivered { get; set; }
    }

    public class DeliveryDashboard
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

    public class DPLocationList
    {
        public int Status { get; set; }
        public string Msg { get; set; }
        public List<DPLocationHistory> Locations { get; set; }
    }

    public class DPLocationHistory
    {
        public int Id { get; set; }
        public string Lat { get; set; }
        public string Long { get; set; }
        public int DeliveryTypeID { get; set; }
        public string EntryDate { get; set; }
    }
}
