namespace RoundpayFinTech.AppCode.Model.ProcModel
{
    public class TransactionPGLog
    {
        public int TID { get; set; }
        public int PGID { get; set; }
        public string Log { get; set; }
        public string RequestIP { get; set; }
        public string Browser { get; set; }
        public string TransactionID { get; set; }
        public string Checksum { get; set; }        
        public string VendorID { get; set; }
        public string Request { get; set; }
        public string Response { get; set; }
        public int RequestMode { get; set; }
        public decimal Amount { get; set; }
        public bool IsRequestGenerated { get; set; }
        public string StatuscheckURL { get; set; }
        public string MerchantID { get; set; }
        public string MerchantKEY { get; set; }
    }
}
