using System.Collections.Generic;

namespace RoundpayFinTech.AppCode.StaticModel.MoneyTransfer
{
    public class MWAppSetting
    {
        public string Token { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string AuthUrl { get; set; }
        public string BaseURL { get; set; }
        public string CheckSumUrl { get; set; }
        public string VerificationAPICode { get; set; }
    }
    public class MaheshcommParam
    {
        public string customerId { get; set; }
        public string agentCode { get; set; }
    }
    public class MaheshcommParam2 : MaheshcommParam
    {
        public string udf1 { get; set; }
        public string udf2 { get; set; }
    }
    public class SendOTP : MaheshcommParam
    {
        public string otpType { get; set; }
        public string txnId { get; set; }
    }
    public class CreateSender : MaheshcommParam
    {
        public string name { get; set; }
        public string address { get; set; }
        public string dateOfBirth { get; set; }
        public string otp { get; set; }
    }
    public class AddReceipent : MaheshcommParam2
    {
        public string mobileNo { get; set; }
        public string recipientName { get; set; }
        public string recipientType { get; set; }
    }
    public class VerifyBene : MaheshcommParam2
    {
        public string clientRefId { get; set; }
        public string currency { get; set; }
        public string channel { get; set; }
        public string recipientType { get; set; }

    }
    public class DeleteRecepient : MaheshcommParam
    {
        public string recipientId { get; set; }
    }
    public class SendMoney : MaheshcommParam
    {
        public string recSeqId { get; set; }
        public string amount { get; set; }
        public string clientRefId { get; set; }
        public string currency { get; set; }
        public string channel { get; set; }
        public string tp1 { get; set; }
        public string tp2 { get; set; }
        public string tp3 { get; set; }
        public string tp4 { get; set; }
        public string tp5 { get; set; }
        public string tp6 { get; set; }
    }
    //checkSender
    public class RecipientList
    {
        public string recipientType { get; set; }
        public string recipientId { get; set; }
        public string recipientName { get; set; }
        public string bankName { get; set; }
        public string mobileNo { get; set; }
        public string udf1 { get; set; }
        public string udf2 { get; set; }
    }
    public class Response
    {
        public string createdDate { get; set; }
        public string walletbal { get; set; }
        public string totalMonthlyLimit { get; set; }
        public string customerId { get; set; }
        public string name { get; set; }
        public string kycstatus { get; set; }
        public string dateOfBirth { get; set; }
    }
    public class CheckSenderRespones
    {
        public Response response { get; set; }
        public string txnId { get; set; }
        public string errorCode { get; set; }
        public string errorMsg { get; set; }
        public string token { get; set; }
    }
    public class SendOTPChild
    {
        public string customerId { get; set; }
        public string otp { get; set; }
        public string initiatorId { get; set; }
    }
    public class SendOTPRespones
    {
        public SendOTPChild response { get; set; }
        public string txnId { get; set; }
        public string errorCode { get; set; }
        public string errorMsg { get; set; }
        public string token { get; set; }
    }
    public class createResponse
    {
        public string customerId { get; set; }
    }
    public class ResponesCreateSender
    {
        public createResponse response { get; set; }
        public string txnId { get; set; }
        public string errorCode { get; set; }
        public string errorMsg { get; set; }
        public string token { get; set; }
    }
    public class BeniResponse
    {
        public List<RecipientList> recipientList { get; set; }
    }
    public class GetBeneficiary
    {
        public BeniResponse response { get; set; }
        public string txnId { get; set; }
        public string errorCode { get; set; }
        public string errorMsg { get; set; }
        public string token { get; set; }
    }
    public class CreateBeniResponse
    {
        public string txnStatus { get; set; }
        public string serviceTax { get; set; }
        public string clientRefId { get; set; }
        public string fee { get; set; }
        public string name { get; set; }
        public string customerId { get; set; }
        public string bankName { get; set; }
        public string initiatorId { get; set; }
        public string impsRespCode { get; set; }
        public string impsRespMessage { get; set; }
        public string txnId { get; set; }
        public string timestamp { get; set; }
    }
    public class VerificationBene
    {
        public CreateBeniResponse response { get; set; }
        public string txnId { get; set; }
        public string errorCode { get; set; }
        public string errorMsg { get; set; }
        public string token { get; set; }
    }
    public class CreateBene
    {
        public string recipientType { get; set; }
        public string recipientId { get; set; }
        public string recipientName { get; set; }
    }
    public class CreateBeneResponse
    {
        public CreateBene response { get; set; }
        public string txnId { get; set; }
        public string errorCode { get; set; }
        public string errorMsg { get; set; }
        public string token { get; set; }
    }
    public class SearhTrax
    {
        public decimal amount { get; set; }
        public decimal charge { get; set; }
        public decimal gst { get; set; }
        public string tp10 { get; set; }
        public string rrn { get; set; }
        public string txnStatus { get; set; }
        public string tp1 { get; set; }
        public string tp3 { get; set; }
        public string tp2 { get; set; }
        public string tp5 { get; set; }
        public string tp4 { get; set; }
        public decimal commission { get; set; }
        public string tp7 { get; set; }
        public string tp6 { get; set; }
        public string tp9 { get; set; }
        public string tp8 { get; set; }
        public string txnId { get; set; }
    }
    public class SearhTraxResp
    {
        public SearhTrax response { get; set; }
        public string errorCode { get; set; }
        public string errorMsg { get; set; }
        public string token { get; set; }
    }
}
