using RoundpayFinTech.AppCode.Model.ProcModel;

namespace RoundpayFinTech.AppCode.Model
{
    public class FintechAPISetting
    {
        public string BaseURL { get; set; }
        public int UserID { get; set; }
        public int OutletID { get; set; }
        public string Token { get; set; }
        public string PIN { get; set; }
        public string HYPTO_VerificationURL { get; set; }
        public string HYPTO_VerifyAuth { get; set; }
    }
    public class FintechAPIRequestModel : APIRequestOutlet
    {

    }
    public class FintechAPIResponse
    {
        public int Statuscode { get; set; }
        public string Msg { get; set; }
        public int Errorcode { get; set; }
        public FintechAPIData data { get; set; }
    }
    public class FintechAPIData
    {
        public int OutletID { get; set; }
        public int VerifyStatus { get; set; }
        public int KYCStatus { get; set; }
    }
    public class FintechAPIServiceResponse
    {
        public int Statuscode { get; set; }
        public string Msg { get; set; }
        public int Errorcode { get; set; }
        public OnboardAPIResponseStatus data { get; set; }
    }

    public class ManualDMTAppSetting {
        public string VerificationAPICode { get; set; }
        public string HYPTO_VerificationURL { get; set; }
        public string HYPTO_VerifyAuth { get; set; }
    }
}
