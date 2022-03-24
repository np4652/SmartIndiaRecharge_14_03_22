namespace RoundpayFinTech.AppCode.Model.ProcModel
{
    public class UpdatePGTransactionRequest
    {
        public int PGID { get; set; }
        public int TID { get; set; }
        public int Type { get; set; }
        public string VendorID { get; set; }
        public string LiveID { get; set; }
        public int Amount { get; set; }
        public string PaymentModeSpKey { get; set; }
        public string Remark { get; set; }
        public string RequestIP { get; set; }
        public string Browser { get; set; }
        public string Signature { get; set; }
        public string RequestPage { get; set; }
    }
}
