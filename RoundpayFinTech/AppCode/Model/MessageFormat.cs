using Fintech.AppCode.Model;

namespace RoundpayFinTech.AppCode.Model
{
    public class MessageTemplateParam : CommonReq
    {
        public int ID { get; set; }
        public int FormatID { get; set; }
        public int TemplateType { get; set; }
        public string Subject { get; set; }
        public string Template { get; set; }
        public string EmailTemplate { get; set; }
        public string AlertTemplate { get; set; }
        public bool IsEnable { get; set; }
        public bool IsEnableSMS { get; set; }
        public bool IsEnableNotification { get; set; }
        public bool IsEnableWebNotification { get; set; }
        public bool IsEnableEmail { get; set; }
        public int APIID { get; set; }
        public string TemplateID { get; set; }
        public int WID { get; set; }
        public bool IsEnableSocialAlert { get; set; }
        public bool SocialAlertTemplate { get; set; }
        public bool IsEnableWhatsApp { get; set; }
        public bool IsEnableHangout { get; set; }
        public bool IsEnableTelegram { get; set; }

        public string WhatsAppTemplateID { get; set; }
    }
}
