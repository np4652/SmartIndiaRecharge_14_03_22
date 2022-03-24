namespace RoundpayFinTech.Models
{
    public class WalletStatus
    {
        public int Statuscode { get; set; }
        public string Msg { get; set; }
        public decimal Balance { get; set; }
        public bool IsAddWallet { get; set; }
    }
}
