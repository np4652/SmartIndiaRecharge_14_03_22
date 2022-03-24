namespace RoundpayFinTech.AppCode.Model.ProcModel
{
    public class XPressServiceProcRequest
    {
        public int UserID { get; set; }
        public int OutletID { get; set; }
        public string AccountNo { get; set; }
        public int AmountR { get; set; }
        public int BankID { get; set; }
        public string Bank { get; set; }
        public string IFSC { get; set; }
        public string SenderName { get; set; }
        public string SenderEmail { get; set; }
        public string SenderMobile { get; set; }
        public string BeneName { get; set; }
        public string BeneMobile { get; set; }
        public int APIID { get; set; }
        public string IMEI { get; set; }
        public string ExtraParam { get; set; }
        public string SecurityKey { get; set; }
        public string APIRequestID { get; set; }
        public int RequestModeID { get; set; }
        public string RequestIP { get; set; }
        public string SPKey { get; set; }
        public int OID { get; set; }
    }
}
