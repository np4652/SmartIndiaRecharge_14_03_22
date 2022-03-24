using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.ThirdParty.HotelAPI
{
    public class TekTvlPriceReq
    {
        public string CurrencyCode { get; set; }
        public string RoomPrice { get; set; }
        public string Tax { get; set; }
        public string ExtraGuestCharge { get; set; }
        public string ChildCharge { get; set; }
        public string OtherCharges { get; set; }
        public string Discount { get; set; }
        public string PublishedPrice { get; set; }
        public string PublishedPriceRoundedOff { get; set; }
        public string OfferedPrice { get; set; }
        public string OfferedPriceRoundedOff { get; set; }
        public string AgentCommission { get; set; }
        public string AgentMarkUp { get; set; }
        public string ServiceTax { get; set; }
        public string TDS { get; set; }
    }

    public class TekTvlHotelPassengerReq
    {
        public string Title { get; set; }
        public string FirstName { get; set; }
        public object Middlename { get; set; }
        public string LastName { get; set; }
        public object Phoneno { get; set; }
        public object Email { get; set; }
        public int PaxType { get; set; }
        public bool LeadPassenger { get; set; }
        public int Age { get; set; }
        public object PassportNo { get; set; }
        public Nullable<DateTime> PassportIssueDate { get; set; }
        public Nullable<DateTime> PassportExpDate { get; set; }

        public string PAN { get; set; }
    }

    public class TekTvlHotelRoomsDetailReq
    {
        public string RoomIndex { get; set; }
        public string RoomTypeCode { get; set; }
        public string RoomTypeName { get; set; }
        public string RatePlanCode { get; set; }
        public object BedTypeCode { get; set; }
        public object SmokingPreference { get; set; }
        public object Supplements { get; set; }
        public string CategoryId { get; set; }
        public TekTvlPriceReq Price { get; set; }
        public List<TekTvlHotelPassengerReq> HotelPassenger { get; set; }
    }
    public class TekTvlHotelBookRoomReq
    {
        public string ResultIndex { get; set; }
        public string HotelCode { get; set; }
        public string HotelName { get; set; }
        public string GuestNationality { get; set; }
        public string NoOfRooms { get; set; }
        public string ClientReferenceNo { get; set; }
        public string IsVoucherBooking { get; set; }
        public bool IsPackageFare { get; set; }
        public string CategoryId { get; set; }
        public List<TekTvlHotelRoomsDetailReq> HotelRoomsDetails { get; set; }
        public string EndUserIp { get; set; }
        public string TokenId { get; set; }
        public string TraceId { get; set; }
    }

    public class HotelApiReqRes
    {
        public string ReqUrl { get; set; }
        public string Request { get; set; }
        public string Response { get; set; }
        public string ClassName { get; set; }
        public string Method { get; set; }
        public string EndUserIP { get; set; }
        public int  UserID { get; set; }
    }

}
