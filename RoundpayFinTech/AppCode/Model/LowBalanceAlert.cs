
namespace RoundpayFinTech.AppCode.Model
{
    public class LowBalanceAlert
    {
        public int ID { get; set; }
        public string UserName { get; set; }
        public string MobileNo { get; set; }
        public string EmailID { get; set; }
        public string FCMID { get; set; }
        public decimal Balance { get; set; }
        public decimal AlertBalance { get; set; }
        public int WID { get; set; }
        public bool IsLowBalance { get; set; }
    }
}
