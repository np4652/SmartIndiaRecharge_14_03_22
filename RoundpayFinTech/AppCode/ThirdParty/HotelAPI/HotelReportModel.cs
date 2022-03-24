using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.ThirdParty.HotelAPI
{

    public class HotelReport
    {
        public bool IsAdmin { get; set; }
        public IEnumerable<BookingDetails> BookingDetails { get; set; }
    }
    public class BookingDetails
    {
        public string TransactionID { get; set; }
        public string Token { get; set; }
        public string HotelName { get; set; }
        public string Type_ { get; set; }
        public string Destination { get; set; }
        public string CheckIn { get; set; }
        public string CheckOut { get; set; }
        public int TotalGuest { get; set; }
        public string BookingID { get; set; }
        public decimal Amount { get; set; }
        public decimal Balance { get; set; }
        public int Credited { get; set; }
        public int Debit { get; set; }
        public int Comm { get; set; }
        public int ClosingBalance { get; set; }
        public string CancellationStatus { get; set; }
        public string Operator { get; set; }
        public string RequestMode { get; set; }
        public string EntryDate { get; set; }
        public int RequestModeID { get; set; }
        public int TID { get; set; }
        public int _Type { get; set; }
        public decimal LastBalance { get; set; }
        public decimal RequestedAmount { get; set; }
        public decimal Commission { get; set; }
        public int OID { get; set; }
        public bool CommType { get; set; }
    }

    public class CancelBookingRequest
    {
        public string BookingMode { get; set; }
        public int RequestType { get; set; }
        public int BookingId { get; set; }
        public string EndUserIp { get; set; }
        public string TokenId { get; set; }
        public string Remark { get; set; }
    }
   

    public class HotelChangeRequestResult
    {
        public int ChangeRequestStatus { get; set; }
        public int ResponseStatus { get; set; }
        public Error Error { get; set; }
        public string TraceId { get; set; }
        public int ChangeRequestId { get; set; }
    }

    public class CancelBookingResponse
    {
        public HotelChangeRequestResult HotelChangeRequestResult { get; set; }
    }

}
