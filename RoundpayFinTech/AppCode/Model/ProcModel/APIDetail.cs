using System.Collections.Generic;

namespace RoundpayFinTech.AppCode.Model.ProcModel
{
    public class APIGroupDetail
    {
        public int GroupID { get; set; }
        public string GroupName { get; set; }
        public string GroupCode { get; set; }
        public string RequestMethod { get; set; }
        public string StatusName { get; set; }
        public string FailCode { get; set; }
        public string SuccessCode { get; set; }
        public string ErrorCodeKey { get; set; }
        public string LiveID { get; set; }
        public string VendorID { get; set; }
        public int ResponseTypeID { get; set; }
        public string MsgKey { get; set; }
        public string BillNoKey { get; set; }
        public string BillDateKey { get; set; }
        public string DueDateKey { get; set; }
        public string BillAmountKey { get; set; }
        public string CustomerNameKey { get; set; }
        public string BillStatusKey { get; set; }
        public string BillStatusValue { get; set; }
        public string BalanceKey { get; set; }
        public string AdditionalInfoListKey { get; set; }
        public string AdditionalInfoKey { get; set; }
        public string AdditionalInfoValue { get; set; }
        public string PANID { get; set; }
        public string ValidationStatusKey { get; set; }
        public string ValidationStatusValue { get; set; }
        public string APIOutletIDMob { get; set; }        
        public string GeoCodeAGT { get; set; }        
        public string GeoCodeMOB { get; set; }        
        public string GeoCodeINT { get; set; }        
        public string HookTIDKey { get; set; }        
        public string HookStatusKey { get; set; }        
        public int HookResTypeID { get; set; }        
        public string HookVendorKey { get; set; }        
        public string HookLiveIDKey { get; set; }        
        public string HookMsgKey { get; set; }        
        public string HookBalanceKey { get; set; }        
        public string HookSuccessCode { get; set; }        
        public string HookFailCode { get; set; }
        public string BillFetchAPICode { get; set; }
        public string FirstDelimiter { get; set; }
        public string SecondDelimiter { get; set; }
        public string HookFirstDelimiter { get; set; }
        public string HookSecondDelimiter { get; set; }
        public string HookErrorCodeKey { get; set; }
    }
    public class APIDetail : APIGroupDetail
    {
        public int ID { get; set; }
        public int APIType { get; set; }
        public string _APIType { get; set; }
        public string OnlineOutletID { get; set; }
        public string Name { get; set; }
        public string URL { get; set; }
        public string APICode { get; set; }
        public string StatusCheckURL { get; set; }
        public string BalanceURL { get; set; }
        public string DisputeURL { get; set; }
        public string FetchBillURL { get; set; }
        public bool ActiveSts { get; set; }
        public bool IsOutletRequired { get; set; }
        public string FixedOutletID { get; set; }
        public string Remark { get; set; }
        public int EntryBy { get; set; }
        public string EntryDate { get; set; }
        public int ModifyBy { get; set; }
        public string ModifyDate { get; set; }
        public bool IsOpDownAllow { get; set; }
        public bool IsActive { get; set; }
        public string APIOpCode { get; set; }
        public string APIOpCodeCircle { get; set; }
        public decimal Comm { get; set; }
        public bool CommType { get; set; }
        public bool IsBBPS { get; set; }
        public bool IsOutletManual { get; set; }
        public string APIOutletID { get; set; }
        public int SurchargeType { get; set; }
        public int TransactionType { get; set; }
        public int ContentType { get; set; }
        public bool IsAmountInto100 { get; set; }
        public string BillReqMethod { get; set; }
        public string RefferenceKey { get; set; }        
        public string RechType { get; set; }
        public int MaxLimitPerTransaction { get; set; }
        public int BillResTypeID { get; set; }
        public int DFormatID { get; set; }
        public int SwitchingID { get; set; }
        public bool InSwitch { get; set; }
        public bool IsStatusBulkCheck { get; set; }
        public bool IsInternalSender { get; set; }
        public bool Default { get; set; }
        public bool IsWhatsApp { get; set; }
        public bool IsHangout { get; set; }
        public int WID { get; set; }
        public bool IsDMT { get; set; }
        public bool IsRealTime { get; set; }
        public bool IsAEPS { get; set; }

        public string VenderMail { get; set; }
        public string HandoutID { get; set; }
        public string Mobileno { get; set; }
        public string WhatsAppNo { get; set; }
        public string ValidateURL { get; set; }
        public int PartnerUserID { get; set; }
        public bool IsTelegram { get; set; }
    }
    public class APIDetailReq : APIDetail
    {
        public int LoginID { get; set; }
        public int LT { get; set; }
        public string IP { get; set; }
        public string Browser { get; set; }

    }
    public class ErrorLog
    {
        public string ClassName { get; set; }
        public string FuncName { get; set; }
        public int UserId { get; set; }
        public string Error { get; set; }
        public string EntryDate { get; set; }
        public int LoginTypeID { get; set; }
    }
    public class SMSAPIDetail
    {
        public bool IsTelegram { get; set; }
        public int Statuscode { get; set; }
        public int ID { get; set; }
        public int APIType { get; set; }
        public string _APIType { get; set; }
        public int TransactionType { get; set; }
        public string Name { get; set; }
        public string URL { get; set; }
        public bool IsActive { get; set; }
        public int WID { get; set; }
        public string APIMethod { get; set; }
        public int ResType { get; set; }
        public bool Default { get; set; }
        public bool IsWhatsApp { get; set; }
        public bool IsMultipleAllowed { get; set; }
        public bool IsHangout { get; set; }
        public string APICode { get; set; }
    }
    public class EmailAPIDetail
    {
        public int ID { get; set; }
        public string FromEmail { get; set; }
        public string SendEmail { get; set; }
        public string Password { get; set; }
        public string HostName { get; set; }
        public string UserMailID { get; set; }
        public int Port { get; set; }
        public bool IsActive { get; set; }
        public bool IsEmailVerified { get; set; }
        public bool IsSSL { get; set; }
        public bool IsDefault { get; set; }
        public int WID { get; set; }

        public List<EmailProvider> Provider { get; set; }

    }

    public class EmailProvider
    {
        public string ProviderID { get; set; }
        public string ProviderName { get; set; }
        public string HostName { get; set; }
        public string SMTPPort { get; set; }
        public bool IsSSL { get; set; }

    }

    public class EmailAPIDetailReq : EmailAPIDetail
    {
        public int LoginID { get; set; }
        public int LT { get; set; }
        public string IP { get; set; }
        public string Browser { get; set; }
    }
    public class APIDetailForDMTPipe
    {
        public int Statuscode { get; set; }
        public string Msg { get; set; }
        public int ErrorCode { get; set; }
        public List<APIDetail> APIs { get; set; }
    }
    public class ApiListModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
    public class CardAccountMapping
    {

        public int LoginID { get; set; }
        public int LT { get; set; }
        public int ID { get; set; }
        public string UserID { get; set; }
        public string AccountNo { get; set; }
        public string Validfrom { get; set; }
        public string ValidThru { get; set; }
        public int CommonInt { get; set; }
    }
    public class WhatsappAPIDetail : APIDetailReq
    {
        public int Statuscode { get; set; }
        public int APIID { get; set; }
        public string DEPID { get; set; }
    }

}
