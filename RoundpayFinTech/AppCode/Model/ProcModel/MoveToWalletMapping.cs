namespace RoundpayFinTech.AppCode.Model.ProcModel
{
    public class MoveToWalletMapping
    {
        public int ID { get; set; }
        public int FromWalletID { get; set; }
        public int ToWalletID { get; set; }
        public string FromWalletType { get; set; }
        public string ToWalletType { get; set; }
    }
}
