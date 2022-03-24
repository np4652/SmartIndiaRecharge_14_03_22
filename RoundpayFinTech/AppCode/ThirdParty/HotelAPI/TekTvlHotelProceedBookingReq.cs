using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.ThirdParty.HotelAPI
{
    public class TekTvlHotelProceedBookingReq
    {
        public string ResultIndex { get; set; }
        public string HotelCode { get; set; }
        public string HotelName { get; set; }
        public int NoOfRooms { get; set; }
        public int NoOfPerson { get; set; }
        public int NoOfChild { get; set; }
        public string TraceId { get; set; }
        public string CheckInDate { get; set; }
        public string CheckOutDate { get; set; }
        public string RoomIndexes { get; set; }
        public string searchJson { get; set; }
        public bool IsPANMandatory { get; set; }
        public bool IsPassportMandatory { get; set; }
       public blockresponse blockresponse { get; set; }


    }
}
