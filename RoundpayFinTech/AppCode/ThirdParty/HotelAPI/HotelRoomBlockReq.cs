using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.ThirdParty.HotelAPI
{
    public class HotelRoomBlockReq
    {
        public string ResultIndex { get; set; }
        public string HotelCode { get; set; }
        public string HotelName { get; set; }
        public string GuestNationality { get; set; }
        public int NoOfRooms { get; set; }
        public string ClientReferenceNo { get; set; }
        public bool IsVoucherBooking { get; set; }
        public string EndUserIp { get; set; }
        public string TokenId { get; set; }
        public string TraceId { get; set; }
        public string CategoryId { get; set; }
        public List<HotelRoomsDetails> HotelRoomsDetails { get; set; }
    }

    public class HotelRoomsDetails
    {
        public int RoomIndex { get; set; }
        public string RoomTypeCode { get; set; }
        public string RoomTypeName { get; set; }
        public string RatePlanCode { get; set; }
        public object BedTypeCode { get; set; }
        public object SmokingPreference { get; set; }
        public object Supplements { get; set; }
        public string CategoryId { get; set; }
        public BlockRoomPrice Price { get; set; }
    }
    public class BlockRoomPrice
    {
        public string CurrencyCode { get; set; }
        public decimal RoomPrice { get; set; }
        public decimal Tax { get; set; }
        public decimal ExtraGuestCharge { get; set; }
        public decimal ChildCharge { get; set; }
        public decimal OtherCharges { get; set; }
        public decimal Discount { get; set; }
        public decimal PublishedPrice { get; set; }
        public int PublishedPriceRoundedOff { get; set; }
        public decimal OfferedPrice { get; set; }
        public int OfferedPriceRoundedOff { get; set; }
        public decimal AgentCommission { get; set; }
        public decimal AgentMarkUp { get; set; }
        public decimal ServiceTax { get; set; }
        public decimal TDS { get; set; }
    }
   

    public class DayRate
    {
        public double Amount { get; set; }
        public DateTime Date { get; set; }
    }

    public class GST
    {
        public double CGSTAmount { get; set; }
        public int CGSTRate { get; set; }
        public double CessAmount { get; set; }
        public double CessRate { get; set; }
        public double IGSTAmount { get; set; }
        public int IGSTRate { get; set; }
        public double SGSTAmount { get; set; }
        public int SGSTRate { get; set; }
        public double TaxableAmount { get; set; }
    }

    public class Price
    {
        public string CurrencyCode { get; set; }
        public double RoomPrice { get; set; }
        public double Tax { get; set; }
        public double ExtraGuestCharge { get; set; }
        public double ChildCharge { get; set; }
        public double OtherCharges { get; set; }
        public double Discount { get; set; }
        public double PublishedPrice { get; set; }
        public int PublishedPriceRoundedOff { get; set; }
        public double OfferedPrice { get; set; }
        public int OfferedPriceRoundedOff { get; set; }
        public double AgentCommission { get; set; }
        public double AgentMarkUp { get; set; }
        public double ServiceTax { get; set; }
        public double TCS { get; set; }
        public double TDS { get; set; }
        public int ServiceCharge { get; set; }
        public double TotalGSTAmount { get; set; }
        public GST GST { get; set; }
    }

    public class CancellationPolicy
    {
        public int Charge { get; set; }
        public int ChargeType { get; set; }
        public string Currency { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }

    public class HotelRoomsDetail
    {
        public string AvailabilityType { get; set; }
        public int ChildCount { get; set; }
        public bool RequireAllPaxDetails { get; set; }
        public int RoomId { get; set; }
        public int RoomStatus { get; set; }
        public int RoomIndex { get; set; }
        public string RoomTypeCode { get; set; }
        public string RoomDescription { get; set; }
        public string RoomTypeName { get; set; }
        public string RatePlanCode { get; set; }
        public int RatePlan { get; set; }
        public string RatePlanName { get; set; }
        public string InfoSource { get; set; }
        public string SequenceNo { get; set; }
        public List<DayRate> DayRates { get; set; }
        public bool IsPerStay { get; set; }
        public object SupplierPrice { get; set; }
        public Price Price { get; set; }
        public string RoomPromotion { get; set; }
        public List<string> Amenities { get; set; }
        public List<string> Amenity { get; set; }
        public string SmokingPreference { get; set; }
        public List<object> BedTypes { get; set; }
        public List<object> HotelSupplements { get; set; }
        public DateTime LastCancellationDate { get; set; }
        public string SupplierSpecificData { get; set; }
        public List<CancellationPolicy> CancellationPolicies { get; set; }
        public DateTime LastVoucherDate { get; set; }
        public string CancellationPolicy { get; set; }
        public List<string> Inclusion { get; set; }
        public bool IsPassportMandatory { get; set; }
        public bool IsPANMandatory { get; set; }
    }

    public class BlockRoomResult
    {
        public string TraceId { get; set; }
        public int ResponseStatus { get; set; }
        public Error Error { get; set; }
        public bool IsCancellationPolicyChanged { get; set; }
        public bool IsHotelPolicyChanged { get; set; }
        public bool IsPriceChanged { get; set; }
        public bool IsPackageFare { get; set; }
        public bool IsDepartureDetailsMandatory { get; set; }
        public bool IsPackageDetailsMandatory { get; set; }
        public string AvailabilityType { get; set; }
        public bool GSTAllowed { get; set; }
        public string HotelNorms { get; set; }
        public string HotelName { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public int StarRating { get; set; }
        public string HotelPolicyDetail { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public bool BookingAllowedForRoamer { get; set; }
        public List<object> AncillaryServices { get; set; }
        public List<HotelRoomsDetail> HotelRoomsDetails { get; set; }
    }

    public class BlockRoomResponse
    {
        public BlockRoomResult BlockRoomResult { get; set; }
    }
    public class blockresponse
    {
        public int Statuscode { get; set; }
        public bool IsError { get; set; }
        public string Msg { get; set; }
        public string HotelName { get; set; }
        public int  Amount { get; set; }
        public int  NoOfRooms { get; set; }
        public int  NoOfAdults { get; set; }
        public int  NoOfChilds { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public bool IsBlocked { get; set; }
        public string Blockstatus { get; set; }
        public string TrnsactionID { get; set; }
        public string DestinationName { get; set; }
        public bool IsPolicychanges { get; set; }
        public bool IsPriceChange { get; set; }
        public string AvailabilityType { get; set; }
        public string BookingId { get; set; }
        public string ConfirmationNo { get; set; }
        public string AccountNo { get; set; }
        public DateTime TranDate { get; set; }
        public string OPName { get; set; }
    }

}
