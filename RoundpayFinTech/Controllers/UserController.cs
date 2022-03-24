using Fintech.AppCode;
using Fintech.AppCode.Configuration;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using GoogleAuthenticatorService.Core;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RoundpayFinTech.AppCode;
using RoundpayFinTech.AppCode.HelperClass;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.MiddleLayer;
using RoundpayFinTech.AppCode.Model;
using System.Threading.Tasks;

namespace RoundpayFinTech.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class UserController : Controller
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        private readonly ISession _session;
        private readonly LoginResponse _lr;
        private readonly ILoginML loginML;
        public UserController(IHttpContextAccessor accessor, IHostingEnvironment env)
        {
            _accessor = accessor;
            _env = env;
            _session = _accessor.HttpContext.Session;
            _lr = _session.GetObjectFromJson<LoginResponse>(SessionKeys.LoginResponse);
            loginML = new LoginML(_accessor, _env);
        }
        [HttpPost]
        [Route("MiddleDashBoard")]
        public IActionResult MiddleDashBoard()
        {
            IUserML uML = new UserML(_accessor, _env);
            return Json(uML.MiddleDashBoard());
        }

        public IActionResult Index()
        {
            if (loginML.IsInValidSession())
            {
                return RedirectToAction("Index", "Login");
            }
            return View();
        }

        #region googleAuthenticator
        [HttpPost, Route("/GoogleAuthenticatorSetup")]
        public IActionResult GoogleAuthenticatorSetup(string otp)
        {
            IResponseStatus response = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            var sessionOTP = _session.GetString(SessionKeys.CommonOTP);
            IAdminML ml = new UserML(_accessor, _env);
            if (string.IsNullOrEmpty(sessionOTP))
            {
                response = ml.SendUTROTP(_lr.UserID);
                response.Statuscode = 2;
                response.Msg = "OTP sent to registered e-mail";
                return Json(response);
            }
            else
            {
                if (string.IsNullOrEmpty(otp))
                {
                    response.Statuscode = 2;
                    response.Msg = "Please Enter OTP";
                    ml.SendUTROTP(_lr.UserID);
                    return Json(response);
                }
                if (!sessionOTP.Equals(otp))
                {
                    response.Statuscode = ErrorCodes.Minus1;
                    response.Msg = "Invalid OTP";
                    return Json(response);
                }
            }
            _session.SetString(SessionKeys.CommonOTP, string.Empty);
            return PartialView("~/Views/Login/Partial/_GoogleAuthenticatorSetup.cshtml", initializeGoogleAuth());
        }

        [HttpPost, ValidateAntiForgeryToken, Route("/CompleteGoogleAuthenticatorSetup")]
        public async Task<IActionResult> CompleteGoogleAuthenticatorSetup(string googlePin, string accountSecretKey)
        {
            var response = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = "Google PIN not matched"
            };
            TwoFactorAuthenticator Authenticator = new TwoFactorAuthenticator();
            if (Authenticator.ValidateTwoFactorPIN(accountSecretKey, googlePin))
            {
                var ML = new UserML(_accessor, _env);
                response = (ResponseStatus)await ML.EnableGoogleAuthenticator(true, accountSecretKey,userId:_lr.UserID).ConfigureAwait(true);
                if (response.Statuscode == ErrorCodes.One)
                {
                    _lr.AccountSecretKey = accountSecretKey;
                    _lr.IsGoogle2FAEnable = true;
                    _session.SetObjectAsJson(SessionKeys.LoginResponse, _lr);
                }
            }
            return Json(response);
        }

        [HttpPost, ValidateAntiForgeryToken, Route("/EnableGoogleAuthentication")]
        public async Task<IActionResult> EnableGoogleAuthentication(bool isEnable, string otp, int userId = 0)
        {
            userId = (_lr.RoleID == Role.Admin && userId > 0) ? userId : _lr.UserID;
            IResponseStatus response = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = "Google PIN not matched"
            };
            if (!isEnable)
            {
                var sessionOTP = _session.GetString(SessionKeys.CommonOTP);
                if (string.IsNullOrEmpty(sessionOTP))
                {
                    IAdminML ml = new UserML(_accessor, _env);
                    response = ml.SendUTROTP(userId);
                    response.Statuscode = 2;
                    goto Finish;
                }
                if (otp != sessionOTP)
                {
                    response = new ResponseStatus
                    {
                        Statuscode = userId > 0 ? 2 : ErrorCodes.Minus1,
                        Msg = "Invalid OTP"
                    };
                    goto Finish;
                }
                _session.SetString(SessionKeys.CommonOTP, string.Empty);
            }
            var ML = new UserML(_accessor, _env);
            response = await ML.EnableGoogleAuthenticator(isEnable, string.Empty, true, userId = userId);
            if (response.Statuscode == ErrorCodes.One)
            {
                _lr.IsGoogle2FAEnable = isEnable;
                _session.SetObjectAsJson(SessionKeys.LoginResponse, _lr);
            }
        Finish:
            return Json(response);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult SendOTPToRestGAuth()
        {
            IResponseStatus response = new ResponseStatus();
            IAdminML ml = new UserML(_accessor, _env);
            response = ml.SendUTROTP(_lr.UserID);
            return Json(response);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetGoogleAuthenticator(string otp, int userId = 0)
        {
            string returnType = "html";
            GoogleAuthenticatorModal res = new GoogleAuthenticatorModal
            {
                StatusCode = ErrorCodes.Minus1
            };
            var sessionOTP = _session.GetString(SessionKeys.CommonOTP);
            if (otp != sessionOTP)
            {
                res.Msg = "Invalid OTP";
                returnType = "json";
                goto Finish;
            }
            userId = userId == 0 ? _lr.UserID : userId;
            IUserML ml = new UserML(_accessor, _env);
            var response = (ResponseStatus)await ml.ResetGoogleAuthenticator(userId).ConfigureAwait(true);
            if (response.Statuscode == ErrorCodes.One)
            {

                _lr.AccountSecretKey = string.Empty;
                _session.SetObjectAsJson(SessionKeys.LoginResponse, _lr);
                res = initializeGoogleAuth();
                res.StatusCode = ErrorCodes.One;
            }
        Finish:
            if (returnType == "json")
                return Json(res);
            else
                return PartialView("~/Views/Login/Partial/_GoogleAuthenticatorSetup.cshtml", res);
        }
        private GoogleAuthenticatorModal initializeGoogleAuth()
        {
            GoogleAuthenticatorManager guManager = new GoogleAuthenticatorManager();
            var userId = _lr.Prefix + _lr.UserID;
            var SetupResult = guManager.LoadSharedKeyAndQrCodeUrl(userId, _lr.AccountSecretKey);
            var response = new GoogleAuthenticatorModal
            {
                AccountSecretKey = SetupResult.AccountSecretKey,
                QrCodeSetupImageUrl = SetupResult.QrCodeSetupImageUrl,
                AlreadyRegistered = !string.IsNullOrEmpty(_lr.AccountSecretKey) ? true : false,
                IsEnabled = _lr.IsGoogle2FAEnable
            };
            return response ?? new GoogleAuthenticatorModal();
        }
        #endregion
    }
}