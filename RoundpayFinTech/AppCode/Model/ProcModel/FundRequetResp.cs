using System.Collections.Generic;

namespace RoundpayFinTech.AppCode.Model.ProcModel
{
    public class FundRequetResp
    {
        public bool IsSelf { get; set; }
        public int PaymentId { get; set; }
        public int UserId { get; set; }
        public int LT { get; set; }
        public string Bank { get; set; }
        public string AccountNo { get; set; }
        public string MODE { get; set; }
        public string TransactionId { get; set; }
        public string MobileNo { get; set; }
        public string ChequeNo { get; set; }
        public string AccountHolder { get; set; }
        public string CardNumber { get; set; }
        public string EntryDate { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; }
        public string ApproveDate { get; set; }
        public int StatusCode { get; set; }
        public string Description { get; set; }
        public string UserName { get; set; }
        public string UserMobile { get; set; }
        public string ToDate { get; set; }
        public int _TMode { get; set; }
        public decimal CommRate { get; set; }
        public string Remark { get; set; }
        public string ApproveName { get; set; }
        public string ApproveMobile { get; set; }
        public int WalletTypeID { get; set; }
        public string WalletType { get; set; }
        public string UPIID { get; set; }
        public string Branch { get; set; }
        public string ReceiptURL { get; set; }
        public IEnumerable<MasterRejectReason> MasterRR { get; set; }
        public FundRequetResp FundReject { get; set; }
        public int KYCStatus { get; set; }
        public string RoleName { get; set; }
    }
    public class FundOrderFilter {
        public int LT { get; set; }
        public int LoginID { get; set; }
        public string FromDate {get;set;}
        public  string ToDate {get;set;}
        public  string UMobile {get;set;}
        public  int TMode {get;set;}
        public  int RSts {get;set;}
        public  string AccountNo {get;set;}
        public  string TransactionID {get;set;}
        public  int Top {get;set;}
        public bool IsSelf { get; set; }
        public int Criteria { get; set; }
        public string CriteriaText { get; set; }
        public int CCID { get; set; }
        public string CCMobileNo { get; set; }
        public int UserID { get; set; }
        public int WalletTypeID { get; set; }
    }
    public class FundTransferModel {
        public FundRequetResp fundRequetResp { get; set; }
        public UserBalnace userBalnace { get; set; }
        public bool IsDoubleFactor { get; set; }
        public bool IsAdmin { get; set; }
    }
    public class MasterRejectReason
    {
        public int ID { get; set; }
        public string Reason { get; set; }

    }


    public class FundRequestShow
    {
        public int UserRoleID { get; set; }
        public List<FundRequetResp> FundRequests { get; set; }
    }

    public class DebitFundrequest:CommonFilter
    {

        public int ID { get; set; }
        public string ToName { get; set; }
        public string FromName { get; set; }
        public string  Amount { get; set; }
        public string  Status { get; set; }
        public string WalletID { get; set; }
        public string  RequetedDate { get; set; }
        public string Remark { get; set; }
        public bool IsAccountStatmentEntry { get; set; }
        public string MobileNo1 { get; set; }

    }
    public class DebitFundrequestExl
    {

        
        public string User { get; set; }
        public string From_Name { get; set; }
        public string Amount { get; set; }
        public string Status { get; set; }
        
        public string RequetedDate { get; set; }
        public string Remark { get; set; }
        public bool IsAccountStatmentEntry { get; set; }
       

    }



}
