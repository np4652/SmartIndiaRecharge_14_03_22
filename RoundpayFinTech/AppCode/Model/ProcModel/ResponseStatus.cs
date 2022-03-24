using Fintech.AppCode.Interfaces;

namespace RoundpayFinTech.AppCode.Model
{
    public class ResponseStatus : IResponseStatus
    {
        public int Statuscode { get; set; }
        public string Msg { get; set; }
        public int CommonInt { get; set; }
        public int CommonInt2 { get; set; }
        public string CommonStr { get; set; }
        public string CommonStr2 { get; set; }
        public string CommonStr3 { get; set; }
        public string CommonStr4 { get; set; }
        public int CommonInt3 { get; set; }
        public bool CommonBool { get; set; }
        public bool CommonBool2 { get; set; }
        public char Flag { get; set; }
        public int ErrorCode { get; set; }
        public string ErrorMsg { get; set; }
        public string ReffID { get; set; }
        public int Status { get; set; }
        public OnboardAPIResponseStatus ResponseStatusForAPI { get; set; }
        public bool ShouldSerializeResponseStatusForAPI() => false;
        public byte[] ByteArray{get;set;}
    }
    public class OnboardAPIResponseStatus
    {
        public int Statuscode { get; set; }
        public int ErrorCode { get; set; }
        public string Msg { get; set; }
        public int KYCStatus { get; set; }
        public int VerifyStatus { get; set; }
        public int ServiceStatus { get; set; }
        public string ServiceOutletID { get; set; }
       
        public string ServiceCode { get; set; }
        public string RedirectURL { get; set; }
        public bool IsOTPRequired { get; set; }
        public bool IsBioMetricRequired { get; set; }
        public int BioAuthType { get; set; }
        public string OTP { get; set; }
        public int OTPRefID { get; set; }
    }
    public class ResponseStatusBalnace
    {
        public int Statuscode { get; set; }

        public string Msg { get; set; }
        public string UserMobile { get; set; }
        public decimal Balance { get; set; }
        public string TransactionID { get; set; }
        public string Status { get; set; }
        public int UserID { get; set; }
    }
}
