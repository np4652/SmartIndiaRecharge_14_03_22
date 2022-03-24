
namespace RoundpayFinTech.AppCode.Model
{
    public class PaymentGateway:MasterPaymentGateway
    {
        public int ID { get; set; }
        public int PGID { get; set; }
        public int WID { get; set; }
        public string MerchantID { get; set; }
        public string MerchantKey { get; set; }
        public string ENVCode { get; set; }
        public string IndustryType { get; set; }
        public string SuccessURL { get; set; }
        public string FailedURL { get; set; }
        public bool IsActive { get; set; }
        public int EntryBy { get; set; }
        public string EntryDate { get; set; }
        public int ModifyBy { get; set; }
        public string ModifyDate { get; set; }
        public int AgentType { get; set; }
    }
}
