using Fintech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System.Collections.Generic;
using System.Data;

namespace RoundpayFinTech.AppCode.Interfaces
{
    public interface ISendSMSML
    {
        void SendUserReg(string LoginID, string Password, string MobileNo, string EmailID, int WID, string Logo);
        void SendSMS(DataTable Tp_ReplaceKeywords, string MobileNo, string EmailID, int WID, bool WithMail, string MailSub, int FormatType, string Logo);
        object SendSMS(SMSSendREQ _req);
        void UpdateSMSResponse(SMSUpdateREQ _req);
        void SendUserForget(string LoginID, string Password, string Pin, string MobileNo, string EmailID, int WID, string Logo);
        string CallSendSMSAPI(SendSMSRequest _req);
    }
    public interface IAPIUserML
    {
        IResponseStatus SetGetToken();
        IEnumerable<UserCallBackModel> GetCallBackUrl();
        UserCallBackModel GetCallBackUrl(int Type);
        IResponseStatus SaveCallback(UserCallBackModel req);
    }    
}

