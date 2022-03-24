namespace RoundpayFinTech.AppCode.Model
{
    public class WalletType
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public bool InFundProcess { get; set; }
        public bool IsPackageDedectionForRetailor { get; set; }
    }
}
