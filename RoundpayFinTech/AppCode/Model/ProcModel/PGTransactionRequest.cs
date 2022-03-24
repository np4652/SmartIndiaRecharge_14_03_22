namespace RoundpayFinTech.AppCode.Model.ProcModel
{
    public class PGTransactionRequest
    {
        public int UserID { get; set; }
        public decimal AmountR { get; set; }
        public int UPGID { get; set; }
        public int OID { get; set; }
        public int WalletID { get; set; }
        public int RequestMode { get; set; }
        public string Browser { get; set; }
        public string RequestIP { get; set; }
        public string IMEI { get; set; }
    }
    public class PGTransactionResponse
    {
        public int Statuscode { get; set; }
        public string Msg { get; set; }
        public int UserID { get; set; }
        public decimal Amount { get; set; }
        public int TID { get; set; }
        public string TransactionID { get; set; }
        public string PGName { get; set; }
        public string URL { get; set; }
        public string StatusCheckURL { get; set; }
        public int PGID { get; set; }
        public string MerchantID { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Pincode { get; set; }
        public string MerchantKey { get; set; }
        public string ENVCode { get; set; }
        public string IndustryType { get; set; }
        public string SuccessURL { get; set; }
        public string FailedURL { get; set; }
        public string MobileNo { get; set; }
        public string EmailID { get; set; }
        public string Name { get; set; }
        public string CompanyName { get; set; }
        public int WID { get; set; }
        public string OPID { get; set; }
        public string Domain { get; set; }
        public string VPA { get; set; }
        public bool IsLive { get; set; }
    }
}
