
using System.Collections.Generic;

namespace RoundpayFinTech.AppCode.Model
{
    public class WhatsappBotDic : CommonProReq
    {
        public int KeyId { get; set; }
        public string Key { get; set; }
        public string FormatType { get; set; }
        public int GroupId { get; set; }
        public string ReplyType { get; set; }
        public string ReplyText1 { get; set; }
        public string ReplyText2 { get; set; }
        public string ReplyText3 { get; set; }
        public bool IsActive { get; set; }
        public string Action { get; set; }
        public List<WADicFormatType> FormatTypes { get; set; }

        public WhatsappBotDic()
        {
            var format = new List<WADicFormatType>();
            format.Add(new WADicFormatType
            {
                Text = "Recharge Pending",
                Value = "RechargePending",
                IsSelected = false
            });
            format.Add(new WADicFormatType
            {
                Text = "Dispute",
                Value = "Dispute",
                IsSelected = false
            });
            format.Add(new WADicFormatType
            {
                Text = "Fund Request",
                Value = "Fund Request",
                IsSelected = false
            });
            format.Add(new WADicFormatType
            {
                Text = "Status",
                Value = "Status",
                IsSelected = false
            });
            format.Add(new WADicFormatType
            {
                Text = "KYC",
                Value = "KYC",
                IsSelected = false
            });
            format.Add(new WADicFormatType
            {
                Text = "Other",
                Value = "Other",
                IsSelected = false
            });
            this.FormatTypes = format;
        }

    }

    

    public class WADicFormatType
    {
        public string Text { get; set; }
        public string Value { get; set; }
        public bool IsSelected { get; set; }
    }
    public class WhatsappBotKey
    {
        public int KeyId { get; set; }
        public string Key { get; set; }
    }
    public class WhatsappBotDicViewModel
    {
        public string FormatType { get; set; }
        public string ReplyType { get; set; }
        public string ReplyText1 { get; set; }
        public string ReplyText2 { get; set; }
        public string ReplyText3 { get; set; }
        public bool IsActive { get; set; }
        public IEnumerable<WhatsappBotKey> Keys { get; set; }
    }
}
