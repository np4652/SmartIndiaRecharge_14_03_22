
using RoundpayFinTech.AppCode.Model;

namespace Fintech.AppCode.Interfaces
{
    public interface IRailwayML
    {
        ResponseStatus MatchRedirectedDomain();
        //void UpdateRailService(int TID, int IRSaveID, int Type, decimal AmountR, string AccountNo);
        RailValidateResponse DoDebitFromCallback(string AgentID, string RPID, string AccountNo, decimal AmountR, int OutletID, string APICode);
        IRViewModel Decode(string url, bool IsLive = false);
        IRViewModel ValidateIRLogin(IRViewModel req);
        IRViewModel IRProcessRequest(IRViewModel req);
        IRViewModel DoubleVerificationDecode(string url, bool IsLive = false);
        void LogRailwayReqResp(LogRailwayReqRespModel req);
        IRReturnHitModel TransactionReturnHit(IRSaveModel iRSaveModel, bool IsLive = false);
        //RailValidateResponse ValidateRailUser(int RailID, string Mobile);
        RailValidateResponse GenerateRailOTPForCallback(string APICode, int APIOutletID, int RefID);
        RailValidateResponse ReGenerateOTP();
        //RailValidateResponse MatchRailOTP(int IRSaveID, string OTP);
        RailValidateResponse MatchRailOTPFromCallback(int RailID, int RefID, string OTP);

        //Testing
        string TestPrepareEncodedResponse(TestIRRequestModel req);
    }
}
