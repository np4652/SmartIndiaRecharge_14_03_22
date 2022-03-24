

using System.Collections.Generic;

namespace RoundpayFinTech.AppCode.Model
{
    public class WebNotification
    {
        public int ID { get; set; }
        public int UserID { get; set; }
        public string UserMobileNo { get; set; }
        public string UserName { get; set; }
        public int LoginID { get; set; }
        public int Statuscode { get; set; }
        public string Title { get; set; }
        public string Notification { get; set; }        
        public bool IsSeen { get; set; }        
        public bool IsActive { get; set; }        
        public string Msg { get; set; }
        public string EntryDate{ get; set; }
        public int FormatID{ get; set; }
        public string Operator { get; set; }
        public string Img { get; set; }
        public string RoleId { get; set; }
    }

    public class WebNotificationModel
    {
        public List<WebNotification> Notification { get; set; }
        public bool IsWebNotification { get; set; }
    }
}
