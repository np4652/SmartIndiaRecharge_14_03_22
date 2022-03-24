namespace RoundpayFinTech.AppCode.Model
{
    public class OnboardingSetting
    {
        public string BaseURL { get; set; }
        public string Token { get; set; }
         
    }
    public class APIAppSetting {
        public OnboardingSetting onboardingSetting { get; set; }
    }
    public class FingpayAPISetting 
    {
        public string MERCHANTName { get; set; }
        public string MerchantPin { get; set; }
        public string superMerchantId { get; set; }
        public string secretKey { get; set; }
    }
}
