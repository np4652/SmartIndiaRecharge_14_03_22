namespace RoundpayFinTech.Models
{
    public class MessageBoxModel
    {
        public string H { get; set; }
        public int MBType { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string ButtonText { get; set; }
        public string ButtonID { get; set; }
        public bool IsCancel { get; set; }
        public string CancelText { get; set; }
        public string CancelID { get; set; }
        public bool IsOTP { get; set; }
        public int OTPRefID{ get; set; }
    }
}
