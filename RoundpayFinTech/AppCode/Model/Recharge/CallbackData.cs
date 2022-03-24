namespace Fintech.AppCode.Model
{

    public class CallbackData
    {
        public int ID { get; set; }
        public string Content { get; set; }
        public string Method { get; set; }
        public string Scheme { get; set; }
        public string Path { get; set; }
        public string RequestIP { get; set; }
        public string RequestBrowser { get; set; }
        public string EntryDate { get; set; }
        public int APIID { get; set; }
        public bool? InActiveMode { get; set; }
    }
    public class _CallbackData
    {
        public int LoginID { get; set; }
        public string RequestPage { get; set; }
        public int BookingStatus { get; set; }
        public int TID { get; set; }
        public int TransactionStatus { get; set; }
        public string LiveID { get; set; }
        public string VendorID { get; set; }
        public int Statuscode { get; set; }
        public string Msg { get; set; }
        public string ErrorCode { get; set; }
        public int UserID { get; set; }
        public bool IsBBPS { get; set; }
        public int RequestMode { get; set; }
        public string UpdateUrl { get; set; }
        public string TransactionID { get; set; }
        public string CustomerNumber { get; set; }
        public decimal Balance { get; set; }
        public decimal APIBalance { get; set; }
        public string RequestIP { get; set; }
        public string Browser { get; set; }
        public int LT { get; set; }
        public bool IsCallbackFound { get; set; }
        public string APICode { get; set; }
        public decimal RequestedAmount { get; set; }
        public string AccountNo { get; set; }
        public string Operator { get; set; }
        public string MobileNo { get; set; }
        public string FCMID { get; set; }
        public string EmailID { get; set; }
        public string Company { get; set; }
        public string CompanyDomain { get; set; }
        public string SupportNumber { get; set; }
        public string SupportEmail { get; set; }
        public string AccountContact { get; set; }
        public string AccountEmail { get; set; }
        public string CompanyAddress { get; set; }
        public string UserName { get; set; }
        public string UserMobileNo { get; set; }
        public string BrandName { get; set; }
        public string OutletName { get; set; }
        public bool IsInternalSender { get; set; }
        public string Optional2 { get; set; }
        public string Optional3 { get; set; }
        public string Optional4 { get; set; }
        public string O10 { get; set; }
        public string Request { get; set; }
        public string Response { get; set; }
        public string TechnicianName { get; set; }
        public string TechnicianMobile { get; set; }
        public string CustomerID { get; set; }
        public string STBID { get; set; }
        public string VCNO { get; set; }
        public string InstallationTime { get; set; }
        public string InstalltionCharges { get; set; }
        public string ApprovalTime { get; set; }
        public int WID { get; set; }
        public int APIID { get; set; }
        public int RefundStatus { get; set; }
        public string ConversationID { get; set; }
        public string UserEmailID { get; set; }
        public string SCode { get; set; }
        public int TotalToken { get; set; }
        public string UserWhatsappNo { get; set; }
        public string UserTelegram { get; set; }
        public string UserHangout { get; set; }
    }

    
    public class RefundRequestData
    {
        public int LT { get; set; }
        public int LoginID { get; set; }
        public string RequestPage { get; set; }
        public int TID { get; set; }
        public string TransactionID { get; set; }
        public int RefundStatus { get; set; }
        public int Statuscode { get; set; }
        public string Msg { get; set; }
        public int UserID { get; set; }
        public int RequestMode { get; set; }
        public string RequestIP { get; set; }
        public string Browser { get; set; }
        public string Remark { get; set; }
        public decimal ChargedAmount { get; set; }
        public string Account { get; set; }
        public bool IsSameDay { get; set; }
        public int WalletID { get; set; }
        public int ServiceID { get; set; }
        public int WID { get; set; }
        public string CallbackURL { get; set; }
        public string UserMobileNo { get; set; }
        public string UserFCMID { get; set; }
        public string UserEmailID { get; set; }
        public string OperatorName { get; set; }
        public string LiveID { get; set; }
        public string Company { get; set; }
        public string CompanyDomain { get; set; }
        public string SupportNumber { get; set; }
        public string SupportEmail { get; set; }
        public string AccountContact { get; set; }
        public string AccountEmail { get; set; }
        public string CompanyAddress { get; set; }
        public string OutletName { get; set; }
        public string UserName { get; set; }
        public string UserWhatsappNo { get; set; }
        public string UserTelegram { get; set; }
        public string UserHangout { get; set; }
        public string ConversationID { get; set; }
        public string BrandName { get; set; }
        public decimal Amount { get; set; }
        public decimal BalanceAmount { get; set; }

    }

    public class DMRUpdateRequest
    {
        public int LoginID { get; set; }
        public int TID { get; set; }
        public int Type { get; set; }
        public string Msg { get; set; }
        public string LiveID { get; set; }
        public string VendorID { get; set; }
        public string RequestPage { get; set; }
        public string Request { get; set; }
        public string Response { get; set; }
        public string IP { get; set; }
        public string Browser { get; set; }
    }

    public class DMRUpdateResponse
    {
        public int ResultID { get; set; }
        public string MSG { get; set; }
        public int UserID { get; set; }
        public string TransactionID { get; set; }
        public decimal ChargedAmount { get; set; }
        public string AccountNo { get; set; }
        public bool IsSameDay { get; set; }
    }
    public class PGCallbackData
    {
        public string RequestMehtod { get; set; }
        public string CallbackData { get; set; }
        public int PGID { get; set; }
    }
}
