using RoundpayFinTech.AppCode.Model.ProcModel;
using System.Collections.Generic;

namespace Fintech.AppCode.StaticModel
{
    public class MessageFormat
    {
        public const int Registration = 1;
        public const int FundTransfer = 2;
        public const int FundReceive = 3;
        public const int OTP = 4;
        public const int FundDebit = 5;
        public const int FundCredit = 6;
        public const int RechargeAccept = 7;
        public const int RechargeSuccess = 8;
        public const int RechargeFailed = 9;
        public const int OperatorUPMessage = 10;
        public const int OperatorDownMessage = 11;
        public const int RechargeRefund = 12;
        public const int InvoiceFundCredit = 13;
        public const int LowBalanceFormat = 14;
        public const int ForgetPass = 15;
        public const int senderRegistrationOTP = 16;
        public const int BenificieryRegistrationOTP = 17;
        public const int KYCApproved = 18;
        public const int KYCReject = 19;
        public const int FundOrderAlert = 20;
        public const int UserPartialApproval = 21;
        public const int UserSubscription = 22;
        public const int ThankYou = 23;
        public const int CallNotPicked = 24;
        public const int BirthdayWish = 25;
        public const int MarginRevised = 26;
        public const int RechargeRefundReject = 27;
        public const int Payout = 28;
        public const int BBPSSuccess = 29;
        public const int BBPSComplainRegistration = 30;
        public const int PendingRechargeNotification = 31;
        public const int PendingRefundNotification = 32;
    }

    public class MessageTemplateKeyword
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Keyword { get; set; }
        public int FormatID { get; set; }
        public bool IsActive { get; set; }
    }
    public class MessageTemplate
    {
        public IEnumerable<MessageTemplateKeyword> MessageTemp { get; set; }
        public IEnumerable<MasterMessage> MasterMessage { get; set; }
        public int FormatID { get; set; }
        public int APIID{ get; set; }
        public string Subject { get; set; }
        public string Template { get; set; }
        public string EmailTemplate { get; set; }
        public string AlertTemplate { get; set; }
        public string SocialAlertTemplate { get; set; }
        public bool IsEnableSMS { get; set; }
        public bool IsEnableNotificaion { get; set; }
        public bool IsEnableWebNotification { get; set; }
        public bool IsEnableSocialAlert { get; set; }
        public bool IsEnableEmail { get; set; }
        public int Statuscode { get; set; }
        public string Msg { get; set; }
        public string TemplateID { get; set; }
        public int WID { get; set; }
        public bool IsEnableWhatsApp { get; set; }
        public bool IsEnableHangout { get; set; }
        public bool IsEnableTelegram { get; set; }
        public int WhatsappAPIID { get; set; }
        public int HangoutAPIID { get; set; }
        public int TelegramAPIID { get; set; }
        public string WhatsAppTemplateID { get; set; }
        public string WebNotificationTemplate { get; set; }
        public string WhatsAppTemplate { get; set; }
        public string HangoutTemplate { get; set; }
        public string TelegramTemplate { get; set; }
    }
}
