namespace RoundpayFinTech.AppCode.Model.ProcModel
{
    public class TokenGenerationModel
    {
        public int UserID { get; set; }
        public int OutletID { get; set; }
        public int OID { get; set; }
        public int APIID { get; set; }
        public string TransactionID { get; set; }
        public int RequestMode { get; set; }
        public string APIRequestID { get; set; }
        public string VendorID { get; set; }
    }
}
