namespace RoundpayFinTech.AppCode.Model
{
    public class VericationServiceReq
    {
        public string AccountNo { get; set; }
        public string IFSC { get; set; }
        public int TID { get; set; }
    }
    public class VerificationServiceRes
    {
        public int Statuscode { get; set; }
        public string Msg { get; set; }
        public int ErrorCode { get; set; }
        public string AccountHolder{ get; set; }
        public string AccountNo{ get; set; }
        public string LiveID{ get; set; }
        public string VendorID{ get; set; }
        public string Req{ get; set; }
        public string Resp{ get; set; }
    }
}
