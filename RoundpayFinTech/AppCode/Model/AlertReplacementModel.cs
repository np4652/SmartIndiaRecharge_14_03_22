using RoundpayFinTech.AppCode.Model;
using System.Collections.Generic;

namespace RoundpayFinTech
{
    public class AlertReplacementModel : _AlertReplacementModel
    {
        public string KycRejectReason { get; set; }
        public string OutletID { get; set; }
        public string OutletMobile { get; set; }
        public int RefundStatus { get; set; }
        public int KYCStatus { get; set; }
        public string URL { get; set; }
        public int LoginID { get; set; }
        public string LoginPrefix { get; set; }
        public int CCID { get; set; }
        public string CCName { get; set; }
        public string APICode { get; set; }
        public string LoginUserName { get; set; }
        public string LoginMobileNo { get; set; }
        public string LoginEmailID { get; set; }
        public string Password { get; set; }
        public string PinPassword { get; set; }
        public decimal Amount { get; set; }
        public string BalanceAmount { get; set; }
        public string UserName { get; set; }
        public string OutletName { get; set; }
        public string OTP { get; set; }
        public string UserFCMID { get; set; }
        public bool IsPrefix { get; set; }
        public string UserPrefix { get; set; }
        public string LoginFCMID { get; set; }
        public decimal LoginCurrentBalance { get; set; }
        public decimal UserCurrentBalance { get; set; }
        public int UserID { get; set; }
        public int TID { get; set; }
        public string TransactionID { get; set; }
        public string Company { get; set; }
        public string CompanyDomain { get; set; }
        public string CompanyAddress { get; set; }
        public string BrandName { get; set; }
        public string SupportNumber { get; set; }
        public string SupportEmail { get; set; }
        public string AccountsContactNo { get; set; }
        public string AccountEmail { get; set; }
        public string UserEmailID { get; set; }
        public string AccountNo { get; set; }
        public string UserMobileNo { get; set; }
        public string EmailID { get; set; }
        public string SenderName { get; set; }
        public string TransMode { get; set; }
        public string UTRorRRN { get; set; }
        public string IFSC { get; set; }
        public string DATETIME { get; set; }
        public string AccountNumber { get; set; }
        public string RequestIP { get; set; }
        public string RequestPage { get; set; }
        public string Message { get; set; }
        public int RequestMode { get; set; }
        public string Operator { get; set; }
        public decimal UserAlertBalance { get; set; }
        public string LiveID { get; set; }
        public string RequestStatus { get; set; }
        public int FormatID { get; set; }
        public string NotificationTitle { get; set; }
        public string UserIds { get; set; }
        public string MobileNos { get; set; }
        public string SocialIDs { get; set; }
        public string WhatsappNo { get; set; }
        public string WhatsappConversationID { get; set; }
        public string TelegramNo { get; set; }
        public string HangoutNo { get; set; }
        public string WhatsappNoL { get; set; }
        public string TelegramNoL { get; set; }
        public string HangoutNoL { get; set; }
        public int SocialAlertType { get; set; }
        public string SocialID { get; set; }
        public string Roles { get; set; }
        public string Subject { get; set; }
        public string Duration { get; set; }
        public List<string> bccList { get; set; }
        public int LoginRoleId { get; set; }
        public bool IsSendFailed { get; set; }
    }

    public class _AlertReplacementModel
    {
        public int Statuscode { get; set; }
        public int ID { get; set; }
        public string Msg { get; set; }
        public string CommonStr { get; set; }
        public int WID { get; set; }
        public string CouponCode { get; set; }
        public int CouponQty { get; set; }
        public int CouponValdity { get; set; }
        public string CommonStr2 { get; set; }
    }
}