namespace RoundpayFinTech.AppCode.DL
{
    public class RefundRequestResponse
    {
        public int Statuscode { get; set; }
        public int ErrorCode { get; set; }
        public string Msg { get; set; }
        public string Account { get; set; }
        public decimal Amount { get; set; }
        public int Type { get; set; }
        public int RefundStatus { get; set; }
        public int TID { get; set; }
        public int ServiceID { get; set; }
        public int APIID { get; set; }
        public int UserID { get; set; }
        public string RefundRemark { get; set; }
        public decimal Balance { get; set; }
        public string DisputeURL { get; set; }
        public string TransactionID { get; set; }
        public string VendorID { get; set; }
        public string APICode { get; set; }
        public string Optional2 { get; set; }
        public string APIOutletID { get; set; }
        public string TransactionReqID { get; set; }
        public string MobileNo { get; set; }
        public bool IsBBPS { get; set; }
        public bool IsOTPRequired { get; set; }
        public string GroupIID { get; set; }
        public int OutletID { get; set; }
        public bool IsResendBtnHide { get; set; }

    }
}
