using Fintech.AppCode.Model;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;

namespace RoundpayFinTech.AppCode.Model
{
    public class JRSetting
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string SecurityKey { get; set; }
        public string MD5Key_JR { get; set; }
        public string URL { get; set; }
    }

    public class JRCorporateLogin
    {
        public string SecurityKey { get; set; }
        public string EmailId { get; set; }
        public string Password { get; set; }
        public string APIChkSum { get; set; }
    }
    public class JRCorporateLoginResp
    {
        public string AuthenticationKey { get; set; }
        public string CorporateId { get; set; }
        public string Status { get; set; }
    }

    public class JRRechargeReq : JRSetting
    {
        public string rURL { get; set; }
        public string CorporateId { get; set; }
        public string SecurityKey { get; set; }
        public string AuthKey { get; set; }
        public string Mobile { get; set; }
        public string Provider { get; set; }
        public string Location { get; set; }
        public int Amount { get; set; }
        public string ServiceType { get; set; }
        public string SystemReference { get; set; }
        public string IsPostpaid { get; set; }
        public string APIChkSum { get; set; }
        public string NickName { get; set; }
    }

    public class JRRechargeResp
    { 
        public string Amount { get; set; }
        public string IsPostPaid { get; set; }
        public string MobileNo { get; set; }
        public string Provider { get; set; }
        public string Location { get; set; }
        public string ServiceType { get; set; }
        public string SystemReference { get; set; }
        public string Status { get; set; }
        public string Transactionreference { get; set; }
    }
}
