using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Model.Shopping
{
    public class UserInfoModel
    {
        public int Statuscode { get; set; }
        public string Msg { get; set; }
        public int LoginID { get; set; }
        public string SessionID { get; set; }
        public int UserID { get; set; }
        public string MobileNo { get; set; }
        public string Name { get; set; }
        public string RequestStatus { get; set; }
        public int RequestID { get; set; }
        public string OutletName { get; set; }
        public string EmailID { get; set; }
        public int RoleID { get; set; }
        public string Role { get; set; }
    }
}
