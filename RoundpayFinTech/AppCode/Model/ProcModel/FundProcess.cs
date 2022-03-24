using Fintech.AppCode.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Model.ProcModel
{
    
    public class FundProcess
    {
        public int UserID { get; set; }     
        public decimal Amount { get; set; }
        public bool OType { get; set; }
        public string Remark { get; set; }      
        public int RequestMode { get; set; }
        public int WalletType { get; set; }
        public int PaymentId { get; set; }
        public int RMode { get; set; }
        public string SecurityKey { get; set; }
        public bool IsMarkCredit { get; set; }
    }
    public class FundProcessReq : CommonReq
    {
        public int Statuscode { get; set; }
        public bool IsMarkCredit { get; set; }
        public string Msg { get; set; }
        public string MobileNo { get; set; }
        public string FromUserName { get; set; }
        public string Company { get; set; }
        public string UserName { get; set; }
        public string FromMobileNo { get; set; }
        public string FromEmailID { get; set; }
        public string EMail { get; set; }
        public string TransactionID { get; set; }
        public int TID { get; set; }
        public int WID { get; set; }
        public decimal Amount { get; set; }
        public decimal CBalance_U { get; set; }
        public decimal CBalance_L { get; set; }
        public decimal Balance_L { get; set; }
        public FundProcess fundProcess { get; set; }
        public string UserFCMID { get; set; }
        public string LoginFCMID { get; set; }
        public  bool IsDebitWithApproval { get; set; }
    }
    public class FundRequest
    {
        public int LoginID { get; set; }
        public int BankId { get; set; }
        public int PaymentId { get; set; }
        public decimal Amount { get; set; }
        public string TransactionId { get; set; }
        public string MobileNo { get; set; }
        public string AccountHolderName { get; set; }
        public string ChequeNo { get; set; }
        public string CardNo { get; set; }
        public int LoginTypeID { get; set; }
        public int ToUserID { get; set; }
        public int WalletTypeID { get; set; }
        public string UPIID { get; set; }
        public string Branch { get; set; }
        public string RImage { get; set; }
        public int OrderID { get; set; }
        public int SessionID { get; set; }
        public string IMEI { get; set; }
        public string AppVersion { get; set; }
        public string Checksum { get; set; }        
        public bool IsAuto { get; set; }      
    }

    public class FundRequestRes
    {
        public int ID { get; set; }
        public int Statuscode { get; set; }
        public string Msg { get; set; }
        public string UserName { get; set; }
        public string UserMobileNo { get; set; }
        public string ToMobileNo { get; set; }
        public string ToEmail { get; set; }
        public string FCMID { get; set; }
        public int WID { get; set; }
    }
    public class FundRequestToRole : CommonReq
    {
        public int ToId { get; set; }
        public string ToRole { get; set; }
        public int FromId { get; set; }
        public string FromRole { get; set; }
        public bool IsUpline { get; set; }
        public bool IsActive { get; set; }
        public List<FundRequestToRole> ToRoles { get; set; }
        public int Action { get; set; }
    }
}
