namespace RoundpayFinTech.AppCode.Model
{
    public class PendingRechargeNotification
    {
        public string AccountNo { get; set; }
        public string WhatsappNo { get; set; }
        public string HangoutId { get; set; }
        public string TelegramNo { get; set; }
        public string Duration { get; set; }
        public int APIID { get; set; }
        public string APICode { get; set; }
        public string Name { get; set; }
        public int CCID { get; set; }
        public string CCName { get; set; }
    }
}
