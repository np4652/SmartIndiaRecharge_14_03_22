using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.App;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fintech.AppCode.Interfaces
{
    public interface ILoginML
    {

        ResponseStatus UnlockMe(LoginDetail loginDetail);
        Task<ResponseStatus> BlockAccount(int userId, string IMEI, string IP, string Browser);
        List<News> LoginPageNews();
        object GetLoginDetails(int LoginTypeID = 1);
        WebsiteInfo GetWebsitePackage(int ID);
        ResponseStatus UserSubscriptionApp(GetIntouch getIntouch);
        GetIntouch UserSubscription(GetIntouch getIntouch);
        ResponseStatus DoLogin(LoginDetail loginDetail);
        void SendOTP(string OTP, string MobileNo, string EmailID, int WID);
        ResponseStatus ValidateOTP(string OTP);
        //object GetLoginDetails();
        WebsiteInfo GetWebsiteInfo();
        WebsiteInfo GetWebsiteForDomain();
        WebsiteInfo GetWebsiteInfo(int WID);
        bool IsInValidSession();
        AppLoginResponse DoAppLogin(LoginDetail loginDetail);
        ResponseStatus Forget(LoginDetail loginDetail);
        AppLoginResponse ValidateOTPForApp(string OTP, LoginResponse _lr, string browser, string IMEI);
        AppLoginResponse ValidateGAuthPINForApp(string gauthPIN, LoginResponse _lresp, string browser, string IMEI);
        AppResponse ValidateSessionForApp(LoginDetail loginDetail);
        Task<IResponseStatus> DoLogout(LogoutReq logoutReq);
        IResponseStatus SaveFCMID(CommonReq commonReq);
        ResponseStatus ForgetFromApp(LoginDetail loginDetail);
        PopupInfo GetPopupInfo();
        AppLoginResponse DoWebAppLogin(LoginDetail loginDetail);
        AppLoginResponse ValidateOTPForWebApp(string OTP, LoginResponse _lresp);
        void ResetLoginSession(LoginResponse _lr);
        ResponseStatus ResendLoginOTP();
        ResponseStatus ResendLoginOTPApp(LoginResponse _lresp,string browser, string IMEI);
        LoginResponse CheckSessionGetData(LoginDetail loginDetail);
        ResponseStatus VerifyGoogleAuthenticatorSetup(string googlePin);
        void SetLaunchPreference(string LP, LoginResponse _lr);
    }
    public interface ICreateUserML
    {
        ResponseStatus CallCreateUser(UserCreate _req);
    }
    public interface IResponseStatus
    {
        int Statuscode { get; set; }
        string Msg { get; set; }
        int CommonInt { get; set; }
        int CommonInt2 { get; set; }
        string CommonStr { get; set; }
        string CommonStr2 { get; set; }
        string CommonStr3 { get; set; }
        string CommonStr4 { get; set; }
        bool CommonBool { get; set; }
        string ReffID { get; set; }
        int ErrorCode { get; set; }
        string ErrorMsg { get; set; }
        int Status { get; set; }

    }

}
