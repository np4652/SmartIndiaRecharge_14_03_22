using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.ThirdParty.HotelAPI
{
    public class HotelModel
    {
        public int ID { get; set; }
        public string CityName { get; set; }
        public string CountryCode { get; set; }
        public string CountryName { get; set; }
        public int DestinationID { get; set; }
        public string StateProvince { get; set; }
        public Int16 DestType { get; set; }
        public DateTime EntryDate { get; set; }
        public DateTime ModifyDate { get; set; }
        public bool IsTop { get; set; }
        public List<HotelModel> hotelList { get; set; }
    }
    public class DestinationSearchStaticDataResult
    {
        public string Status { get; set; }
        public string TokenId { get; set; }

        public string TraceId { get; set; }

        public TekTvlErrorModel Error { get; set; }
        //public TekTvlDestinations Destinations { get; set; }
        public List<TekTvlDestinations> Destinations { get; set; }
    }
    public class HotelModelResponce : TekTvlAppsetting
    {
        public string Status { get; set; }
        public string TokenId { get; set; }
        public string CountryCode { get; set; }
        public string SearchType { get; set; }
        public TekTvlErrorModel Error { get; set; }
        public TekTvlMember Member { get; set; }
        public TekTvlDestinations Destinations { get; set; }

    }
    public class TekTvlTopCities
    {
        public int cityId { get; set; }
        public string cityName { get; set; }
        public string countryCode { get; set; }
        public string countryName { get; set; }
        public string cityCode { get; set; }
        public int NewCityId { get; set; }
    }
    public class TekTvlErrorModel
    {
        public string ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
    }
    public class TekTvlAppsetting
    {
        public string ClientId { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string EndUserIp { get; set; }
    }
    public class TekTvlMember
    {

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string MemberId { get; set; }
        public string AgencyId { get; set; }
        public string LoginName { get; set; }
        public string LoginDetails { get; set; }
        public bool isPrimary { get; set; }
    }
    public class TekTvlDestinations
    {
        public string CityName { get; set; }
        public string CountryCode { get; set; }
        public string CountryName { get; set; }
        public int DestinationId { get; set; }
        // public int HotelCode { get; set; }
        public string StateProvince { get; set; }
        public int Type { get; set; }
        public bool IsTop { get; set; }
        //public List<TekTvlDestinations> Destinations { get; set; }
    }


    public class TekTvlHotelcode
    {
        public string CityName { get; set; }
        public string CountryCode { get; set; }
        public string CountryName { get; set; }
        public int DestinationId { get; set; }
        public int HotelCode { get; set; }
        public string StateProvince { get; set; }
        public int Type { get; set; }
        public bool IsTop { get; set; }
        //public List<TekTvlDestinations> Destinations { get; set; }
    }


    public class TekTvlHotelSearchRequest : TekTvlRoomGuest
    {
        public string CheckInDate { get; set; }
        public string CheckOutDate { get; set; }
        public int NoOfNights { get; set; }
        public string CountryCode { get; set; }
        public int CityId { get; set; }
        public bool IsTBOMapped { get; set; }
        public int ResultCount { get; set; }
        public string PreferredCurrency { get; set; }
        public string GuestNationality { get; set; }
        public int NoOfRooms { get; set; }
        public string PreferredHotel { get; set; }
        public int MaxRating { get; set; }

        public int MinRating { get; set; }
        public int ReviewScore { get; set; }
        public bool IsNearBySearchAllowed { get; set; }
        public string EndUserIp { get; set; }
        public string TokenId { get; set; }
        public TekTvlRoomGuest[] RoomGuests { get; set; }

    }
    public class TekTvlRoomGuest
    {
        public int NoOfAdults { get; set; }
        public int NoOfChild { get; set; }
        public int[] ChildAge { get; set; }
    }
    public class TekTvlSearchingResponse
    {
        public TekTvlHotelSearchResult HotelSearchResult { get; set; }
        public string searchJson { get; set; }
    }
    public class TekTvlHotelSearchResult
    {
        public TekTvlErrorModel error { get; set; }
        public string CityId { get; set; }
        public string TraceId { get; set; }
        public string Remarks { get; set; }
        public string PreferredCurrency { get; set; }
        public string ResponseStatus { get; set; }

        public string CheckInDate { get; set; }
        public string CheckOutDate { get; set; }
        public int NoOfRooms { get; set; }
        public int NoOfPersons { get; set; }
        public int NoOfChild { get; set; }
        public List<TekTvlHotelResults> HotelResults { get; set; }
        public List<TekTvlRoomGuest> RoomGuests { get; set; }
    }
    public class TekTvlHotelResults
    {
        public int ResultIndex { get; set; }
        public string Status { get; set; }
        public string HotelCode { get; set; }
        public string HotelName { get; set; }
        public string HotelCategory { get; set; }
        public int StarRating { get; set; }
        public string HotelDescription { get; set; }
        public string HotelPromotion { get; set; }
        public string HotelPolicy { get; set; }
        public string HotelPicture { get; set; }
        public string HotelAddress { get; set; }
        public string HotelContactNo { get; set; }
        public object HotelMap { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public object HotelLocation { get; set; }
        public object SupplierPrice { get; set; }
        public bool IsTBOMapped { get; set; }
        public TekTvlPrice Price { get; set; }
        public List<TekTvlSupplierHotelCodes> SupplierHotelCodes { get; set; }
        public TekTvlErrorModel error { get; set; }
        public List<TekTvlHotelResults> HotelResult { get; set; }
    }
    public class TekTvlPrice
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
    public class TekTvlSupplierHotelCodes
    {
        public string CategoryId { get; set; }
        public int CategoryIndex { get; set; }
    }
    public class TekTvlApiErrorLog
    {

        public string Method { get; set; }
        public string Request { get; set; }
        public string Response { get; set; }
        public string EntryBy { get; set; }
        public DateTime EntryDate { get; set; }
        public string IP { get; set; }
        public string Browser { get; set; }

    }

    public class TekTvlHotelMasterApiRequest
    {
        public string CityId { get; set; }
        public string ClientId { get; set; }
        public string HotelId { get; set; }
        public string EndUserIp { get; set; }
        public string TokenId { get; set; }
        public bool IsCompactData { get; set; }
    }

    public class TekTvlHotelMasterApiResponse
    {
        public TekTvlErrorModel Error { get; set; }
        public string HotelData { get; set; }
        public string Status { get; set; }
        public string TokenId { get; set; }
    }

    public class TekTvlHotelValues
    {
        public enum HotelRetating
        {
            MinRating = 1,
            MaxRating = 5
        }
    }
    public class HotelBook
    {
        public int LT { get; set; }
        public int UserID { get; set; }
        public string AccountNo { get; set; }
        public string TokenID { get; set; }
        public string TraceID { get; set; }
        public int RequestModeID { get; set; }
        public string HotelData { get; set; }
        public int BookingId { get; set; }
        public string EndUserIP { get; set; }
        public int TotalPrice { get; set; }
        public string OPID { get; set; }
        public string AvailabilityType { get; set; }
        public string HotelCode { get; set; }
        public string HotelName { get; set; }
        public int NoOfRooms { get; set; }
        public string ConfirmationNo { get; set; }
        public int HotelBookingStatus { get; set; }
        public bool IsPriceChanged { get; set; }
        //DataTable Guest Model
        public DataTable DTGuestDetails { get; set; }
        public int TID { get; set; }
        public int Childs { get; set; }
        public int Adults { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public int DestinationID { get; set; }


    }



    public class HotelReceiptGuest
    {
        public string Lead { get; set; }
        public string Child { get; set; }
        public string Title { get; set; }
        public string Name { get; set; }
        public string MobileNo { get; set; }
        public int Age { get; set; }
        public string EmailID { get; set; }
    }
    public class HotelReceipt : TransactionDetail
    {
        public int Statuscode { get; set; }
        public bool IsError { get; set; }
        public string Msg { get; set; }
        public string HotelName { get; set; }
        public int Amount { get; set; }
        public int NoOfRooms { get; set; }
        public int NoOfAdults { get; set; }
        public int NoOfChilds { get; set; }
        public string CheckInDate { get; set; }
        public string CheckOutDate { get; set; }
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
        public string TranDate { get; set; }
        public string OPName { get; set; }
        public List<HotelReceiptGuest> HotelReceiptGuest { get; set; }
    }
}
 