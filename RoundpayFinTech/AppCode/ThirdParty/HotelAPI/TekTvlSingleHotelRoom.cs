using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.ThirdParty.HotelAPI
{
    public class TekTvlSingleHotelRoomReq
    {
        public string HotelCode { get; set; }
        public string ResultIndex { get; set; }
        public string EndUserIp { get; set; }
        public string TokenId { get; set; }
        public string TraceId { get; set; }
    }

    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
    public class TekTvlRoomCombination
    {
        public List<int> RoomIndex { get; set; }
    }

    public class TekTvlRoomCombinationsArray
    {
        public string CategoryId { get; set; }
        public string InfoSource { get; set; }
        public bool IsPolicyPerStay { get; set; }
        public List<TekTvlRoomCombination> RoomCombination { get; set; }
    }

    public class TekTvlRoomError
    {
        public int ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class TekTvlRoomDayRate
    {
        public double Amount { get; set; }
        public DateTime Date { get; set; }
    }

    public class TekTvlRoomGST
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

    public class TekTvlRoomPrice
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
        public TekTvlRoomGST GST { get; set; }
    }

    public class TekTvlRoomCancellationPolicy
    {
        public int Charge { get; set; }
        public int ChargeType { get; set; }
        public string Currency { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }

    public class TekTvlRoomHotelRoomsDetail
    {
        public string AvailabilityType { get; set; }
        public string CategoryId { get; set; }
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
        public List<TekTvlRoomDayRate> DayRates { get; set; }
        public bool IsPerStay { get; set; }
        public object SupplierPrice { get; set; }
        public TekTvlRoomPrice Price { get; set; }
        public string RoomPromotion { get; set; }
        public List<string> Amenities { get; set; }
        public List<string> Amenity { get; set; }
        public object SmokingPreference { get; set; }
        public List<object> BedTypes { get; set; }
        public List<object> HotelSupplements { get; set; }
        public DateTime LastCancellationDate { get; set; }
        public List<TekTvlRoomCancellationPolicy> CancellationPolicies { get; set; }
        public DateTime LastVoucherDate { get; set; }
        public string CancellationPolicy { get; set; }
        public List<string> Inclusion { get; set; }
        public bool IsPassportMandatory { get; set; }
        public bool IsPANMandatory { get; set; }
    }

    public class TekTvlRoomGetHotelRoomResult
    {
        public List<TekTvlRoomCombinationsArray> RoomCombinationsArray { get; set; }
        public TekTvlRoomCombinationsArray RoomCombinations { get; set; }
        public int ResponseStatus { get; set; }
        public TekTvlRoomError Error { get; set; }
        public string TraceId { get; set; }
        public bool IsUnderCancellationAllowed { get; set; }
        public bool IsPolicyPerStay { get; set; }
        public List<TekTvlRoomHotelRoomsDetail> HotelRoomsDetails { get; set; }
    }

    public class TekTvlSingleHotelRoomRes
    {
        public TekTvlRoomGetHotelRoomResult GetHotelRoomResult { get; set; }
    }

    public class Error
    {
        public int ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class BookResult
    {
        public bool VoucherStatus { get; set; }
        public int ResponseStatus { get; set; }
        public Error Error { get; set; }
        public string TraceId { get; set; }
        public int Status { get; set; }
        public string HotelBookingStatus { get; set; }
        public string ConfirmationNo { get; set; }
        public string BookingRefNo { get; set; }
        public int BookingId { get; set; }
        public bool IsPriceChanged { get; set; }
        public bool IsCancellationPolicyChanged { get; set; }
    }

    public class HotelRoomBookRepsonse
    {
        public BookResult BookResult { get; set; }
    }

    public class PanPassort
    {
        public string availabilityType { get; set; }
        public string categoryId { get; set; }
        public int childCount { get; set; }
        public bool requireAllPaxDetails { get; set; }
        public int roomId { get; set; }
        public int roomStatus { get; set; }
        public int roomIndex { get; set; }
        public string roomTypeCode { get; set; }
        public string roomDescription { get; set; }
        public string roomTypeName { get; set; }
        public string ratePlanCode { get; set; }
        public int ratePlan { get; set; }
        public string ratePlanName { get; set; }
        public string infoSource { get; set; }
        public string sequenceNo { get; set; }
        
        public bool isPerStay { get; set; }
        public object supplierPrice { get; set; }
      
        public string roomPromotion { get; set; }
        public List<string> amenities { get; set; }
        public List<string> amenity { get; set; }
        public string smokingPreference { get; set; }
        public List<object> bedTypes { get; set; }
        public List<object> hotelSupplements { get; set; }
        public DateTime lastCancellationDate { get; set; }
    
        public DateTime lastVoucherDate { get; set; }
        public string cancellationPolicy { get; set; }
        public List<string> inclusion { get; set; }
        public bool isPassportMandatory { get; set; }
        public bool isPANMandatory { get; set; }
    }
}
