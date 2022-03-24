using System;
using System.Collections.Generic;
namespace RoundpayFinTech.AppCode.Model
{
    public class PopupInfo
    {

        public bool IsAfterLoginPopup { get; set;}
        public bool IsBeforeLoginPopup { get; set;}
        public bool ISWebSitePopup { get; set;}
        public string PopupFileName { get; set; }
        public int WID { get; set; }
        public string WebsiteName { get; set; }
        public string AbsoluteHost { get; set; }
        public bool IsAfterLoginPopupApp { get; set; }
        public bool IsBeforeLoginPopupApp { get; set; }
    }
   

}
