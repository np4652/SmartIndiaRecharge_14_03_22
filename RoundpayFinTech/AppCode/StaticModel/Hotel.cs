
namespace RoundpayFinTech.AppCode.StaticModel
{
    public static class HotelOPID
    {
        public const string HotelOPIDDomestic = "DH01";
        public const string HotelOPIDInternational = "IH01";
    }
    public static class HotelBookingStatus
    {
        public const string BookingConfirmed = "Confirmed";
        public const string BookingBlocked = "Pending";
        public const string BookingFailed = "Failed";
    }
    public static class HotelBookingCancelStatus
    {
        public const int NotSet =0;
        public const int Pending = 1;
        public const int InProgress = 2;
        public const int Processed = 3;
        public const int Rejected = 4;

        public const string _NotSet = "NotSet";
        public const string _Pending = "Pending";
        public const string _InProgress = "InProgress";
        public const string _Processed = "Processed";
        public const string _Rejected = "Rejected";
    }
}
