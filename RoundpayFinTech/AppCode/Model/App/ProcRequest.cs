namespace RoundpayFinTech.AppCode.Model.App
{
    public class GenerateOrderUPIRequest
    {
        public int UserID { get; set; }
        public int Amount { get; set; }
        public int SessionID { get; set; }
        public string UPIID { get; set; }
        public string RequestIP { get; set; }
        public string Browser { get; set; }
        public string IMEI { get; set; }
        public string AppVersion { get; set; }
        public int OrderID { get; set; }
    }
}
