
using System;
using System.Collections.Generic;

namespace RoundpayFinTech
{
    public class GPCodes
    {
        public const int Msg200 = 200;
        public const int Msg404 = 404;
        public const string MsgSuccess = "SUCCESS";
        public const string MsgFailed = "FAILED";
        public const int MsgPending = 0;
        public const string MsgSAS = "User mobile number successfully verify";
        public const string MsgSenderFound = "Sender detail find successfully";
        public const string MsgSenderNotFound = "Mobile number not registered";
        public const string MsgOtpExp = "Otp Expire or Invalid. Please Resend otp.";
        public const string MsgOtpSent = "User registered successfully! OTP Sent to Mobile Number";
        public const string MsgBenAdded = "Bank add successfully! OTP Sent to Mobile Number";
        public const string MsgVerifyBen = "Beneficiary Verification Success";        
        public const string SubWNF = "Subwallet not Found";        
    }
    public class GoterPayAppSetting
    {
        public string BaseURL { get; set; }
        public string MID { get; set; }
        public string MKEY { get; set; }
    }

    public class GoterPayResp
    {
        public string Name { get; set; }
        public string Status { get; set; }
        public string Mobile { get; set; }
        public string Limit { get; set; }
        public string Balance { get; set; }
        public string resText { get; set; }
        public string utrNo { get; set; }

    }

    public class GPVerifyResp
    {
        public string TxnId { get; set; }
        public string status { get; set; }
        public string AccountName { get; set; }
        public string AccountNumber { get; set; }
        public string utrno { get; set; }
        public string Fees { get; set; }
        public string bal { get; set; }
        public string resText { get; set; }
        public string verifyStatus { get; set; }
    }

    public class GPBene
    {
        public string beneficiaryId { get; set; }
        public string AccountNumber { get; set; }
        public string BeneficiaryName { get; set; }
        public string IfscCode { get; set; }
        public string NameVerify { get; set; }
        public string AccountType { get; set; }
    }

}
