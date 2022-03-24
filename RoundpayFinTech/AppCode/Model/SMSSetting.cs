namespace RoundpayFinTech.AppCode.Model
{
    public class SMSSetting
    {
        public int ID { get; set; }
        public int Statuscode { get; set; }
        public string Msg { get; set; }
        public int FormatID { get; set; }
        public string Template { get; set; }
        public string Subject { get; set; }
        public bool IsEnableSMS { get; set; }
        public int APIType { get; set; }
        public string URL { get; set; }
        public string APIMethod { get; set; }
        public int APIID { get; set; }
        public int ResType { get; set; }
        public int SMSID { get; set; }
        public string SenderID { get; set; }
        public string MobileNos { get; set; }
        public string SMS { get; set; }
        public int WID { get; set; }
        public bool IsLapu { get; set; }
    }

    public class EmailSettingswithFormat
    {
        public int ID { get; set; }
        public int Statuscode { get; set; }
        public string Msg { get; set; }
        public int FormatID { get; set; }
        public string EmailTemplate { get; set; }
        public string Subject { get; set; }
        public bool IsEnableEmail { get; set; }
        public string FromEmail { get; set; }
        public string SaleEmail { get; set; }
        public string HostName { get; set; }
        public int Port { get; set; }
        public bool IsSSL { get; set; }
        public string MailUserID { get; set; }
        public string Password { get; set; }
        public int WID { get; set; }
    }
}
