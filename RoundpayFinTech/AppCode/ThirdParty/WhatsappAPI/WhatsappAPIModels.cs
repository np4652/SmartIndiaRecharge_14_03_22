using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.ThirdParty.WhatsappAPI
{

    public class WhatsappSetting
    {
        public string BaseUrl { get; set; }
        public string BearerToken { get; set; }
        public string APIKEY { get; set; }
        public string Url { get; set; }
        public string WACID { get; set; }

    }


    public class WhatsappGetMessageTempResp
    {
        public string result { get; set; }
        public List<WhatsappMessageTemplate> messageTemplates { get; set; }
        public Link link { get; set; }
    }

    public class WhatsappGetContactResp
    {
        public string result { get; set; }
        public List<ContactList> contact_list { get; set; }
        public Link link { get; set; }

    }

    public class WhatsappSendMessageReq
    {
        public string WpMessage { get; set; }
        public string WpNumber { get; set; }
    }



    public class WhatsappSTMRequest
    {
        public string template_name { get; set; }
        public string broadcast_name { get; set; }
        public string parameters { get; set; }

    }

    public class WhatsappSTMResponse
    {
        public string result { get; set; }
        public string info { get; set; }
        public string validWhatsAppNumber { get; set; }

    }
    public class SendSessionMessageResponsefailed
    {
        public object result { get; set; }
        // public string result { get; set; }
        public int statuscode { get; set; }

        public string message { get; set; }

        public string ticketStatus { get; set; }

        public string msg { get; set; }
        public string info { get; set; }
        //  public Message message { get; set; }
    }
    public class SendSessionMessageResponse
    {
        public object result { get; set; }
        // public string result { get; set; }
        public int statuscode { get; set; }

        // public string message { get; set; }
        public message message { get; set; }
        //  public string message { get; set; }
        public string ticketStatus { get; set; }
        public string Data { get; set; }
        public string conversationId { get; set; }
        public string msg { get; set; }
        public string info { get; set; }
        //  public Message message { get; set; }
    }


    public class WhatsappLanguage
    {
        public string key { get; set; }
        public string value { get; set; }
        public string text { get; set; }
    }

    public class Parameter
    {
        public string text { get; set; }
        public string phoneNumber { get; set; }
        public string url { get; set; }
        public object urlOriginal { get; set; }
        public string urlType { get; set; }
        public object buttonParamMapping { get; set; }
    }

    public class Button
    {
        public string type { get; set; }
        public Parameter parameter { get; set; }
    }

    public class WhatsappMessageTemplate
    {
        public string id { get; set; }
        public string elementName { get; set; }
        public string category { get; set; }
        public object hsm { get; set; }
        public object hsmOriginal { get; set; }
        public string status { get; set; }
        public WhatsappLanguage language { get; set; }
        public DateTime lastModified { get; set; }
        public string type { get; set; }
        public object header { get; set; }
        public string body { get; set; }
        public string bodyOriginal { get; set; }
        public string footer { get; set; }
        public List<Button> buttons { get; set; }
        public string buttonsType { get; set; }
    }


    public class Link
    {
        public object prevPage { get; set; }
        public object nextPage { get; set; }
        public int pageNumber { get; set; }
        public int pageSize { get; set; }
        public int total { get; set; }
    }

    public class Parametere
    {
        public string name { get; set; }
        public string value { get; set; }
    }

    public class CustomParam
    {
        public string name { get; set; }
        public string value { get; set; }
    }

    public class ContactList
    {
        public string id { get; set; }
        public string wAid { get; set; }
        public string firstName { get; set; }
        public string fullName { get; set; }
        public string phone { get; set; }
        public object source { get; set; }
        public string contactStatus { get; set; }
        public string photo { get; set; }
        public string created { get; set; }
        public List<object> tags { get; set; }
        public List<CustomParam> customParams { get; set; }
        public bool optedIn { get; set; }
        public bool isDeleted { get; set; }
        public DateTime lastUpdated { get; set; }
        public bool allowBroadcast { get; set; }
        public bool allowSMS { get; set; }
        public List<string> teamIds { get; set; }
        public bool isInFlow { get; set; }
        public string lastFlowId { get; set; }
        public string currentFlowNodeId { get; set; }
    }

    public class Model
    {
        public List<string> ids { get; set; }
    }

    public class Root
    {
        public bool result { get; set; }
        public string phone_number { get; set; }
        public string template_name { get; set; }
        public List<Parametere> parameteres { get; set; }
        public ContactList contact { get; set; }
        public Model model { get; set; }
        public bool validWhatsAppNumber { get; set; }
    }


    public class Media
    {
        public string id { get; set; }
        public string mimeType { get; set; }
        public string caption { get; set; }
    }
    public class message
    {
        public string whatsappMessageId { get; set; }
        public string localMessageId { get; set; }
        public string text { get; set; }
        public Media media { get; set; }
        public object messageContact { get; set; }
        public object location { get; set; }
        public string type { get; set; }
        public string time { get; set; }
        public int status { get; set; }
        public object statusString { get; set; }
        public bool isOwner { get; set; }
        public bool isUnread { get; set; }
        public string ticketId { get; set; }
        public object avatarUrl { get; set; }
        public object assignedId { get; set; }
        public object operatorName { get; set; }
        public object replyContextId { get; set; }
        public int sourceType { get; set; }
        public object failedDetail { get; set; }
        public object messageReferral { get; set; }
        public string id { get; set; }
        public DateTime created { get; set; }
        public string conversationId { get; set; }

    }

    public class SendMessage
    {
        public bool ok { get; set; }
        public string result { get; set; }
        public message message { get; set; }
    }

    public class watsappfile
    {
        public object file { get; set; }

    }
   
    public class WhatsappAPIAlertHub
    {
        public string requestid { get; set; }  //Random UniqID
        public string jid { get; set; }  //Mobile No Of User
        public string content { get; set; }   //Message Text
        public string messagetype { get; set; } //Message Type
        public string APIURL { get; set; }
        public string ScanNo { get; set; }
        public string ConversationID { get; set; }
        public string QuoteMsg { get; set; }
        public string ReplyJID { get; set; }
    }
    public class WhatsappAPIReqRes
    {

        public string status { get; set; }
        public string status_code { get; set; }
        public string description { get; set; }
        public string conversationId { get; set; }
        public string datetime { get; set; }
    }
    public class WhatsappAPIWaTeam
    {
        public string recipient_type { get; set; }
        public string type { get; set; }
        public text text { get; set; }
        public image image { get; set; }
        public string to { get; set; }
        public bool success { get; set; }
        public string meta { get; set; }
        public List<messages> messages { get; set; }
    }
    public class messages
    {
        public string id { get; set; }
    }


    public class text
    {
        public string body { get; set; }
    }

    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
    public class Profile
    {
        public string name { get; set; }
    }

    public class Contact
    {
        public Profile profile { get; set; }
        public string wa_id { get; set; }


    }


    public class image
    {
        public string id { get; set; }
        public string mime_type { get; set; }
        public string sha256 { get; set; }
        public string link { get; set; }
    }
    public class Message
    {
        public string from { get; set; }
        public string id { get; set; }
        public text text { get; set; }
        public image image { get; set; }
        public string timestamp { get; set; }
        public string type { get; set; }
    }




    public class WATeamCallBackRes
    {
        public int SenderID { get; set; }
        public List<Contact> contacts { get; set; }
        public List<Message> messages { get; set; }
    }

  
    public class SaveWhtsappGroup
    {
        public string MobileNo { get; set; }
        public string GroupName { get; set; }
        public string GroupID { get; set; }
        public int Apiid { get; set; }
        public string SenderNo{ get; set; }
    }



    //Whatsapp Group start
    public class AlertHubWhatsapGroup
    {
        public string Msg { get; set; }
        public int StautusCode { get; set; }
        public string status { get; set; }
        public string status_code { get; set; }
        public string description { get; set; }
        public Data data { get; set; }
        public string datetime { get; set; }
    }
    public class Data
    {
        public string group_subject { get; set; }
        public string group_description { get; set; }
        public List<GroupParticipant> group_participants { get; set; }
        public List<string> groups { get; set; }
    }

    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
    public class GroupParticipant
    {
        public string jid { get; set; }
        public int isAdmin { get; set; }
    }

    //Whatsapp Group End


    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
    public class Info
    {
        public string msgid { get; set; }
        public int timestamp { get; set; }
        public string message { get; set; }
        public string caption { get; set; }
        public string imgtype { get; set; }
        public string imgurl { get; set; }
        public int is_forwarded { get; set; }
    }

    public class Quoted
    {
        public string jid { get; set; }
        public string msgid { get; set; }
        public string msgtype { get; set; }
        public string message { get; set; }
        public int is_forwarded { get; set; }
    }

    public class WhatsappAlertHubCallBack
    {
        public string msgtype { get; set; }
        public int SenderNoID { get; set; }
        public string receiver { get; set; }
        public string jid { get; set; }
        public int is_group_msg { get; set; }
        public Info Info { get; set; }
        public string groupid { get; set; }
        public Quoted Quoted { get; set; }
    }






}

