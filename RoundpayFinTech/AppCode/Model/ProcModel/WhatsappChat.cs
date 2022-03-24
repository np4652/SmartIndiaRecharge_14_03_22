using Fintech.AppCode.Model;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Model.ProcModel
{

    public class GetWhatsappContact
    {
        public int ID { get; set; }
        public string MobileNo { get; set; }
        public int APIID { get; set; }
        public string ApICode { get; set; }


        public string SenderName { get; set; }
        public string UserContact { get; set; }
        public DateTime EntryDate { get; set; }
        public DateTime ModifyDate { get; set; }
        public bool Status { get; set; }
        public string Role { get; set; }
        public int NewMsgs { get; set; }
        public int RemChatTime { get; set; }
        public string SenderMobileNo { get; set; }
        public int SenderNoID { get; set; }
        public int Task { get; set; }
        public string PrefixName { get; set; }
        

    }
    public class GetWhatsappContactListModel : LoginResponse
    {
        public List<GetWhatsappContact> GetWhatsappContactList { get; set; }
        public List<GetWhatsappContact> GetWhatsappSenderNoList { get; set; }
        public List<CustomerCareDetails> CustomerCareDetail { get; set; }
        public List<WhatsappConversation> WhatsappConversations { get; set; }
        public List<WhatsappReceiveMsgResp> WhatsappReceiveMsgResp { get; set; }
        public List<WhatsappResponse> WhatsappResponseList { get; internal set; }
        public List<WhatsappAPI> WhatsappAPIList { get; internal set; }
    }

    public class WhatsappAPI
    {
        public int APIID { get; set; }
        public string APINAME { get; set; }
        public string APICODE { get; set; }
        public string APIURL { get; set; }
        public string GroupListUrl { get; set; }
        public string GroupDetailUrl { get; set; }
    }
    public class SendWhatsappMessage
    {
        public string WpMessage { get; set; }
        public string WpNumber { get; set; }

    }

    public class WhatsappConversation
    {
        public int Id { get; set; }
        
        public string conversationId { get; set; }
        public string Msg { get; set; }
        public string APICODE { get; set; }
        public int LoginTypeID { get; set; }
        public string ContactId { get; set; }
        public string Type { get; set; }
        public string StatusString { get; set; }
        public string SenderName { get; set; }
        public string SenderNo { get; set; }
        public string Text { get; set; }
        public string EntryDate { get; set; }
        public string Cdate { get; set; }
        public string MessageDate { get; set; }
        public string MessageTime { get; set; }
        public int CCID { get; set; }
        public string CCName { get; set; }
        public string Data { get; set; }
        public int Statuscode { get; set; }
        public int MessageID { get; set; }
        public string FileName { get; set; }
        public int UnreadMessages { get; set; }
        public string absoluteurl { get; set; }
        public string APIURL { get; set; }
        public bool IsSeen { get; set; }

        public string ForwardString { get; set; }
        public string WAMobileNo { get; set; }
        public string QuoteMobileno { get; set; }
        
        public string QuoteMsg { get; set; }
        public string QuoteMsgID { get; set; }
        public string ReplyJID { get; set; }

        public string GroupID { get; set; }
        public string Screenshot { get; set; }
        public string RemChatTime { get; set; }
        public IFormFile File { get; set; }
    }

    public class WhatsappSendMsgResp
    {
        public string ContactId { get; set; }
        public string msg { get; set; }
        public string msgStatus { get; set; }
        public DateTime msgDate { get; set; }

    }
    public class WhatsappResponse
    {
        public string msg { get; set; }
        public int statuscode { get; set; }
    }
    public class GetToken
    {
        public string Token { get; set; }
        public int statuscode { get; set; }
    }

    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
    public class WhatsappReceiveMsgResp
    {
        public int LoginTypeID { get; set; }
        public int SenderNoID { get; set; }
        public string ApiCode { get; set; }
        public string id { get; set; }
        public int _id { get; set; }
        public DateTime created { get; set; }
        public string conversationId { get; set; }
        public string ticketId { get; set; }
        public string text { get; set; }
        public string type { get; set; }
        public object data { get; set; }
        public string timestamp { get; set; }
        public string statusString { get; set; }
        public string waId { get; set; }
        public object messageContact { get; set; }
        public string senderName { get; set; }
        public int Statuscode { get; set; }
        public string Msg { get; set; }
        public string _Content { get; set; }
        public int CCID { get; set; }
        public string CCName { get; set; }
        public string TransactionId { get; set; }
        public string TID { get; set; }
        public int UserId { get; set; }
        public int TransactionAPIId { get; set; }
        public string FormatType { get; set; }
        public string QuotedMsgID { get; set; }
        public string GroupID { get; set; }
        public string SenderNO { get; set; }
        public string QuoteMsg { get; set; }
        public string ReplyJID { get; set; }
        public string ReceiveMsgMobileNo { get; set; }
    }
    public class WhatsappReqRes
    {
        public string Request { get; set; }
        public string Response { get; set; }
        public string ClassName { get; set; }
        public string Method { get; set; }
    }

    public class Mobile
    {
        public string MT { get; set; }
    }

    public class ForwardMessage
    {
        public string MessageText { get; set; }
        public string APICODE { get; set; }
        public string snm { get; set; }
        public int MsgID { get; set; }
        public List<Mobile> Mobile { get; set; }
        public string SenderNo { get; set; }
    }

    public class WhatsappService
    {
        public List<WhatsappServiceSenderNo> WhatsappServiceSenderNoList { get;  set; }
    }
    public class WhatsappServiceSenderNo
    {
        public string SenderNo { get; set; }
        public string PassedChatTime { get; set; }
        public string URL { get; set; }
    }

}
