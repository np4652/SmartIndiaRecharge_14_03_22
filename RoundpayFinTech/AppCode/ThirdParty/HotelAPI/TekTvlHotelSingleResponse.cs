using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.ThirdParty.HotelAPI
{
    public class TekTvlSingleHotelInfoApiRequest
    {
        public string HotelCode { get; set; }
        public string ResultIndex { get; set; }
        public string EndUserIp { get; set; }
        public string TokenId { get; set; }
        public string TraceId { get; set; }
        public string CategoryId { get; set; }

        //Extra parameters
        public string NoOfRooms { get; set; }
        public string CheckInDate { get; set; }
        public string CheckOutDate { get; set; }
        public string NoOfPersons { get; set; }
        public string NoOfChild { get; set; }
    }

    public class TekTvlError
    {
        public int ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
        public int TID { get; set; }
    }

    public class TekTvlAttraction
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }

    public class TekTvlHotelDetails
    {
        public string HotelCode { get; set; }
        public string HotelName { get; set; }
        public int StarRating { get; set; }
        public object HotelURL { get; set; }
        public string Description { get; set; }
        public List<TekTvlAttraction> Attractions { get; set; }
        public List<string> HotelFacilities { get; set; }
        public object HotelPolicy { get; set; }
        public object SpecialInstructions { get; set; }
        public object HotelPicture { get; set; }
        public List<string> Images { get; set; }
        public string Address { get; set; }
        public string CountryName { get; set; }
        public object PinCode { get; set; }
        public string HotelContactNo { get; set; }
        public string FaxNumber { get; set; }
        public object Email { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public object RoomData { get; set; }
        public object RoomFacilities { get; set; }
        public object Services { get; set; }
        public string ResultIndex { get; set; }

        //Extra parameters
        public string NoOfRooms { get; set; }
        public string CheckInDate { get; set; }
        public string CheckOutDate { get; set; }
        public string NoOfPersons { get; set; }
        public string NoOfChild { get; set; }
    }

    public class TekTvlHotelInfoResult
    {
        public int ResponseStatus { get; set; }
        public TekTvlError Error { get; set; }
        public string TraceId { get; set; }
        public TekTvlHotelDetails HotelDetails { get; set; }
    }

    public class TekTvlHotelSingleResponse
    {
        public TekTvlHotelInfoResult HotelInfoResult { get; set; }
        public string CategoryId { get; set; }
        public string searchJson { get; set; }
    }
}
