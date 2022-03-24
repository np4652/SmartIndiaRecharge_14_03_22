using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Model
{
    public class IRKeysSettingsModel
    {
        public string keyString { get; set; }
        public string ivString { get; set; }
        public string merchantCode { get; set; }
        public int reservationIdLength { get; set; }
        public int txnAmtLength { get; set; }
        public string currencyType { get; set; }
        public int appCodeLength { get; set; }
        public int pymtModeLength { get; set; }
        public int txnDateLength { get; set; }
        public string securityId { get; set; }
        public int OTPValidity { get; set; }
    }
    public class IRRequestModel
    {
        public int StatusCode { get; set; }
        public string Msg { get; set; }
        public string merchantCode { get; set; }
        public string reservationId { get; set; }
        public decimal txnAmount { get; set; }
        public string currencyType { get; set; }
        public string appCode { get; set; }
        public string pymtMode { get; set; }
        public string txnDate { get; set; }
        public string securityId { get; set; }
        public string RU { get; set; }
        public string checkSum { get; set; }
    }

    public class IRResponseModel
    {
        public string merchantCode { get; set; }
        public string reservationId { get; set; }
        public string txnAmount { get; set; }
        public string bankTxnId { get; set; }
        public string status { get; set; }
        public string statusDesc { get; set; }
        public string checkSum { get; set; }
    }

    public class IRViewModel
    {
        public int StatusCode { get; set; }
        public string Msg { get; set; }
        public int Id { get; set; }
        public int IRSaveID { get; set; }
        public string reservationId { get; set; }
        public decimal txnAmount { get; set; }
        public decimal balanceAmount { get; set; }
        public int LoginId { get; set; }
        public string password { get; set; }
        public string mobile { get; set; }
        public string otp { get; set; }
        public string IP { get; set; }
        public string Browser { get; set; }
        public string MerchantCode { get; set; }
        public bool IsConfirm { get; set; }
        public string BankTranId { get; set; }
        public bool IsLive { get; set; }
    }

    public class IRSaveModel : IRRequestModel
    {
        public int UserId { get; set; } //internal id of user
        public string Mobile { get; set; }
        public int LoginId { get; set; }
        public string UserName { get; set; }
        public string Request { get; set; }
        public string EncodedRequest { get; set; }
        public string Response { get; set; }
        public string EncodedResponse { get; set; }
        public int IRSaveID { get; set; }
        public string IP { get; set; }
        public string Browser { get; set; }
        public string OTP { get; set; }
        public int TranStatus { get; set; }
        public bool IsDoubleVerification { get; set; }
        public decimal Balance { get; set; }
        public string BankTranId { get; set; }
    }

    public class DoubleVerificationRequest
    {
        public int StatusCode { get; set; }
        public string Msg { get; set; }
        public string merchantCode { get; set; }
        public string reservationId { get; set; }
        public string bankTxnId { get; set; }
        public string txnAmount { get; set; }
        public string checkSum { get; set; }
    }

    public class LogRailwayReqRespModel
    {
        public int Id { get; set; }
        public string MethodName { get; set; }
        public string Request { get; set; }
        public string Response { get; set; }
        public string IP { get; set; }
        public string Browser { get; set; }
    }

    public class ValidateRailOutletRespProc
    {
        public int Statuscode { get; set; }
        public string Msg { get; set; }
        public string OTP { get; set; }
        public int OutletUserID { get; set; }
        public string OutletName { get; set; }
        public int PartnerID { get; set; }
        public string PartnerName { get; set; }
        public string PartnerMobile { get; set; }
        public string OTPURLRail { get; set; }
        public string MatchOTPURLRail { get; set; }
        public bool IsOutsider { get; set; }
        public int OutletID { get; set; }
        public string RU { get; set; }
        public string EmailID { get; set; }
    }
    public class RailOutletGenerateOTPResp
    {
        public int Statuscode { get; set; }
        public string Msg { get; set; }
        public string OTP { get; set; }
        public string Mobile { get; set; }
        public string EmailID { get; set; }
        public int RefID { get; set; }
        public int UserID { get; set; }
    }
    public class RailValidateResponse
    {
        public int Statuscode { get; set; }
        public string Msg { get; set; }
        public long RefID { get; set; }
        public string APIRequestID { get; set; }
        public string RU { get; set; }
        public RailwayServiceProcRes ProcRes { get; set; }

        public bool ShouldSerializeProcRes() => false;
    }
    public class RailOTPSession
    {
        public long RefID { get; set; }
        public string OTP { get; set; }
        public int OutletUserID { get; set; }
        public string OTPURL { get; set; }
        public string MatchOTPURL { get; set; }
        public string Mobile { get; set; }
        public string EmailID { get; set; }
        public int RailID { get; set; }
        public bool IsOutsider { get; set; }
        public DateTime AtTime { get; set; }
    }
    public class RailwayServiceProcReq
    {
        public int OutletID { get; set; }
        public int IRSaveID { get; set; }
        public int UserID { get; set; }
        public string APICode { get; set; }
        public string AccountNo { get; set; }
        public decimal AmountR { get; set; }
        public string APIRequestID { get; set; }
        public string VendorID { get; set; }
        public bool IsExternal { get; set; }
        public string RequestIP { get; set; }
    }
    public class RailwayServiceProcRes
    {
        public int Statuscode { get; set; }
        public string Msg { get; set; }
        public int TID { get; set; }
        public string TransactionID { get; set; }
        public decimal Balance { get; set; }
        public bool IsExternal { get; set; }
        public string DebitURLRail { get; set; }
    }
    public class UpdateRailServiceProcReq
    {
        public int TID { get; set; }
        public int IRSaveID { get; set; }
        public int Type { get; set; }
        public decimal AmountR { get; set; }
        public string AccountNo { get; set; }
    }

    public class ViewErrorModel
    {
        public string ErrorCode { get; set; }
        public string Msg { get; set; }
    }

    public class IRReturnHitModel
    {
        public string ErrorCode { get; set; }
        public string Msg { get; set; }
        public string RU { get; set; }
        public string EncResp { get; set; }
        public bool IsDV { get; set; }
    }


    public class TestIRRequestModel
    {
        public string merchantCode { get; set; }
        public string reservationId { get; set; }
        public string txnAmount { get; set; }
        public string bankTxnId { get; set; }
        public string checkSum { get; set; }
    }
}
