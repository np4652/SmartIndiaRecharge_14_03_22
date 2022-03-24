namespace RoundpayFinTech.AppCode.Model.SDK
{
    public class SDKRequest
    {
        public int? UserID { get; set; }
        public string PIN { get; set; }
        public string Token { get; set; }
        public int? OutletID { get; set; }
        public int? PartnerID { get; set; }
        public string SPKey { get; set; }
    }
    public class SDKIntitateRequest : SDKRequest
    {
        public int? SDKType { get; set; }
        public int? Amount { get; set; }
        public int? TID { get; set; }
        public int? APIStatus { get; set; }
        public string SDKMsg { get; set; }
        public string VendorID { get; set; }
        public string AccountNo { get; set; }
        public PidData pidData { get; set; }
        public string pidDataXML { get; set; }
        public string Aadhar { get; set; }
        public string BankIIN { get; set; }
        public string BankName { get; set; }
        public string Reff1 { get; set; }
        public string Reff2 { get; set; }
        public string Reff3 { get; set; }
        public string OTP { get; set; }
        public string IMEI { get; set; }
        public string Lattitude { get; set; }
        public string Longitude { get; set; }
    }
    public class SDKVendorDetail
    {
        public string SuperMerchantID { get; set; }
        public string APIOutletID { get; set; }
    }
    public class SDKResponse
    {
        public int Statuscode { get; set; }
        public string Msg { get; set; }
        public int OID { get; set; }
    }
    public class SDKDetailResponse : SDKResponse
    {
        public object data { get; set; }
    }

    public class RPFintechCallbackResponse
    {
        public int Statuscode { get; set; }
        public string Msg { get; set; }
        public string APIRequestID { get; set; }
        public string OutletID { get; set; }
        public string Amount { get; set; }
        public string SPKey { get; set; }
        public string BankBalance { get; set; }
        public string Status { get; set; }
        public string AccountNo { get; set; }
    }
}
