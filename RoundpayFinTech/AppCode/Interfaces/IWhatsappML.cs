using Fintech.AppCode.Model;
using Microsoft.AspNetCore.Http;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.ThirdParty.WhatsappAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Interfaces
{
    public interface IWhatsappML
    {
        GetWhatsappContactListModel ProcGetWhatsappContacts(CommonReq _req);
        Task<WhatsappConversation> SaveWhatsappConvertation(WhatsappConversation wc);
        GetWhatsappContactListModel GetWhatsappSenderNoList(CommonReq _req);
        GetWhatsappContactListModel ProcGetWhatsappContactsSearch(CommonReq _req);

        GetWhatsappContactListModel ProcSearchUserContacts(CommonReq _req);
        GetWhatsappContactListModel GetWhatsappConversation(CommonReq _req);

        GetWhatsappContact ProcSaveWhatsappContacts(CommonReq _req);

        WhatsappMessageTemplate ProcSaveWhatsappMsgTemplate(CommonReq _req);
        Task<WhatsappConversation> SendWhatsappSessionMessageAllAPI(WhatsappConversation wc, bool Isautoreply = false);
        //Task<WhatsappConversation> SendWhatsappSessionMessageAllAPI(WhatsappConversation _req, IFormFile file , string imgscrnshot = "", bool Isautoreply = false);
        Task<WhatsappConversation> SendWhatsappForwardMessageAllAPI(string textmessage);


        WhatsappConversation ProcUpdatewhatsappMsgStatus(WhatsappConversation _req);

        ResponseStatus UpdateWhatsappTask(int ContactID, int Task);

        GetWhatsappContact getWhatsappContactList();

        WhatsappReceiveMsgResp WhatsappReceiveMsgResp(CommonReq _req);
        WhatsappResponse ProcSyncUserWatsappContacts(CommonReq _req);
      
        Task<WhatsappGetContactResp> WhatsappGetContact();
        Task<WhatsappGetMessageTempResp> WhatsappGetMessageTemplate();

        Task<SendSessionMessageResponse> WhatsappSendSessionMessage(WhatsappConversation wc);

        Task<SendSessionMessageResponse> WhatsappForwardSessionMessage(string FDMsgs);
        Task<byte[]> GetWatsappFile(string url, string ac);

        Task<byte[]> GetWatsappVideoFile(string url);

        WhatsappResponse SelectNewMsg(CommonReq _req);

        Task<SendSessionMessageResponse> sendSessionFileMessage(string Wm, string Wn, IFormFile fd, string MimeType, string FileName);
        //ResponseStatus UploadOperatorIcon(IFormFile file, int OID);

        Task<AlertHubWhatsapGroup> SaveWhatsappGroup(CommonReq _req);



        dynamic WhatsappSenderNoService(bool b);

    }
}
