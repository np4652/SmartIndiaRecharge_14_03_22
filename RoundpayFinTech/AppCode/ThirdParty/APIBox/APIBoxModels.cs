namespace RoundpayFinTech.AppCode.ThirdParty.APIBox
{
    public class APIBoxSetting
    {
        public string Token{ get; set; }
        public string BaseURL { get; set; }
        public string VerificationAPICode { get; set; }
        public string WalletNo { get; set; }
    }
    public class APIBoxModel
    {
        public APIBoxResponse response { get; set; }
    }
    public class APIBoxResponse
    {
        public string OrderId { get; set; }
        public string reqid { get; set; }
        public string UTR { get; set; }
        public string Wallet { get; set; }
        public string BeneficiaryName { get; set; }
        public string Status { get; set; }
        public string response_time { get; set; }
        public string status_code { get; set; }
        public string desc { get; set; }
        public object billing { get; set; }
    }
}
