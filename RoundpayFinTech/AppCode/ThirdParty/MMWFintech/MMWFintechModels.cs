
using System;
using System.Collections.Generic;

namespace RoundpayFinTech
{
    public class MMWFintechCodes
    {
        public const int Msg200 = 200;
        public const int Msg404 = 404;
        public const int MsgSuccess = 2;
        public const int MsgFailed = 1;
        public const int MsgPending = 0;
        public const string MsgSAS = "Sender Add Successfully";
        public const string MsgSenderFound = "Sender detail find successfully";
        public const string MsgSenderNotFound = "Sender not available please Register. Registration otp sent successfully on your Mobile.";
        public const string MsgOtpExp = "Otp Expire or Invalid. Please Resend otp.";
        public const string MsgOtpSent = "OTP Sent Successfully";
        public const string MsgBenAdded = "Beneficiary Add Successfully";
        public const string MsgVerifyBen = "Request Process succesfully";        
    }
    public class MMWFintechAppSetting
    {
        public string BaseURL { get; set; }
        public string APIKey { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
    public class MMWFintechReq
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string APIKey { get; set; }
        public string MobileNo { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Pincode { get; set; }
        public string OTC { get; set; }
        public string SenderMobile { get; set; }
        public string BenName { get; set; }
        public string IFSC { get; set; }
        public string AccountNumber { get; set; }
        public string BankName { get; set; }
        public string CustomerNumber { get; set; }
        public string BeneficiaryId { get; set; }
        public string mobileNumber { get; set; }
        public string BenId { get; set; }
        public string unique_request_number { get; set; }
        public string payment_mode { get; set; }
        public decimal amount { get; set; }
        public string ClientTransactionId { get; set; }
    }
    public class MMWFintechResp
    {
        public bool MessageStatus { get; set; }
        public string Message { get; set; }
        public int Status { get; set; }
        public JsonResult JsonResult { get; set; }
    }

    public class MMWFintechObjResp
    {
        public bool MessageStatus { get; set; }
        public string Message { get; set; }
        public int Status { get; set; }
        public object JsonResult { get; set; }
    }
    

    public class SenderInformation
    {
        public int Id { get; set; }
        public string MobileNo { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PinCode { get; set; }
        public bool Verified { get; set; }
    }
    public class BeneficiaryList
    {
        public string Id { get; set; }
        public string BenName { get; set; }
        public string BankName { get; set; }
        public string AccountNumber { get; set; }
        public string IFSC { get; set; }
        public bool VerifyStatus { get; set; }
        public string CustomerNumber { get; set; }
        public DateTime CreatedDate { get; set; }
    }
    public class JsonResult
    {
        public string BenName { get; set; }
        public string ISValid { get; set; }
        public decimal TotalLimit { get; set; }
        public decimal UsedLimit { get; set; }
        public decimal RemainingLimit { get; set; }
        public SenderInformation SenderInformation { get; set; }
        public List<BeneficiaryList> BeneficiaryList { get; set; }
        public string Uniquerequestnumber { get; set; }
        public string SystemTransactionId { get; set; }
        public string Failurereason { get; set; }
        public string BeneficiaryId { get; set; }
        public string Uniquetransactionreference { get; set; }
        public string Paymentmode { get; set; }
        public string Amount { get; set; }
        public string Beneficiarybankname { get; set; }
        public string Beneficiaryaccountname { get; set; }
        public string Beneficiary_accountnumber { get; set; }
        public string Beneficiaryaccountifsc { get; set; }
        public int TransactionStatus { get; set; }
        public string TransactionStatusMessage { get; set; }
    }


}
