namespace RoundpayFinTech.AppCode.Model.Recharge
{
    public class PaymentConfirmationModel
    {
        public bool IsBBPSInStaging{ get; set; }
        public int Statuscode { get; set; }
        public string Msg { get; set; }
        public string title { get; set; }
        public string LiveID { get; set; }
        public string TransactionID { get; set; }
        public string Amount { get; set; }
        public string BillerID { get; set; }
        public string BillerName { get; set; }
        public string CustomerName { get; set; }
        public string CustomerMobile { get; set; }
        public string AccountNoKey { get; set; }
        public string AccountNo { get; set; }
        public string TransactionDate { get; set; }
        public string PayMode { get; set; }
        public int CCF{ get; set; }
    }
}
