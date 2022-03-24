

namespace RoundpayFinTech.AppCode.Model
{
    public class LowBalanceSetting
    {
        public int ID { get; set; }
       
        public int AlertBalance { get; set; }
        public string MobileNos { get; set; }
        public string Emails { get; set; }
        public string MobileNo { get; set; }
        public string EmailID { get; set; }
        public string WhatsappNo { get; set; }
        public string TelegramNo { get; set; }
        public string HangoutId { get; set; }

        public int LT { get; set; }
        public int LoginID { get; set; }
        public bool IsEmailVerified { get; set; }
    }
}
