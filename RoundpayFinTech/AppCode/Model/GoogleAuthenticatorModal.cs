

namespace RoundpayFinTech.AppCode.Model
{
    public class GoogleAuthenticatorModal
    {
        public int StatusCode { get; set; }
        public string Msg { get; set; }
        public string Account { get; set; }
        public string AccountSecretKey { get; set; }
        public string ManualEntryKey { get; set; }
        public string QrCodeSetupImageUrl { get; set; }
        public bool AlreadyRegistered { get; set; }
        public bool IsEnabled { get; set; }
    }
}
