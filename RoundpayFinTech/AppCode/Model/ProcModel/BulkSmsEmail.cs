using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Model.ProcModel
{
    public class BulkSmsEmail
    {
        public IEnumerable<SMSAPIDetail> smsApi { get; set; }
        public List<RoleMaster> Roles { get; set; }
    }
    public class MasterMessage
    {
        public int ID { get; set; }
        public string FormatType { get; set; }
        public string Remark { get; set; }
    }
    public class Notification
    {
        public int ID { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }  
        public string ImageUrl { get; set; }
        public string Message { get; set; }
        public string Response { get; set; }
        public string EntryDate { get; set; }
        public string FCMID { get; set; }
        public int WID { get; set; }
        public int LoginID { get; set; }
        public int LT { get; set; }
        public int UserID { get; set; }
        public IFormFile file { get; set; }
    }
    public class OpertorMessage
    {
        public IEnumerable<MasterMessage> MasterMessage { get; set; }
        public IEnumerable<OperatorDetail> Operator { get; set; }
    }

    public class NotificationServiceReq
    {
        public int Statuscode { get; set; }
        public string Msg { get; set; }
        public int UserID { get; set; }
        public string Mobile { get; set; }
        public string EmailID { get; set; }
        public string FCMID { get; set; }
        public string Title { get; set; }
        public int FormatID { get; set; }
        public int RequestMode { get; set; }
        public string HitableAPIUserUrl { get; set; }
        public ReplacementModelForSMS replacementModelForSMS { get; set; }
        public MessageTemplate messsageTemplate { get; set; }
        public Notification notification{ get; set; }
        public int RechargeStatus { get; set; }
        public int WID { get; set; }
    }
}
