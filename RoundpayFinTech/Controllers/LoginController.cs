using System;
using Fintech.AppCode;
using Fintech.AppCode.Configuration;
using Fintech.AppCode.HelperClass;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RoundpayFinTech.AppCode;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using Validators;
using RoundpayFinTech.AppCode.Configuration;
using RoundpayFinTech.Models;
using RoundpayFinTech.AppCode.MiddleLayer;
using System.Collections.Generic;
using System.Linq;
using GoogleAuthenticatorService.Core;
using System.Reflection;
using RoundpayFinTech.AppCode.HelperClass;
using System.Threading.Tasks;

namespace RoundpayFinTech.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class LoginController : Controller
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        private readonly ILoginML _loginML;
        public LoginController(IHttpContextAccessor accessor, IHostingEnvironment env)
        {
            _accessor = accessor;
            _env = env;
            _loginML = new LoginML(_accessor, _env);
        }
        [HttpGet]
        public IActionResult Index()
        {
            IFintechShoppingML shoppingML = new FintechShoppingML(_accessor, _env);
            if (shoppingML.IsShoppingDomain())
            {
                return RedirectPermanent("/Shopping");
            }
            else
            {
                var WebInfo = _loginML.GetWebsiteInfo();
                UserML userML = new UserML(_accessor, _env);
                var Cprofile = userML.GetCompanyProfileApp(WebInfo.WID);
                var loginPageModel = new LoginPageModel
                {
                    WID = WebInfo.WID,
                    Host = WebInfo.AbsoluteHost,
                    ThemeID = WebInfo.ThemeId,
                    AppName = Cprofile.AppName,
                    CustomerCareMobileNos = Cprofile.CustomerCareMobileNos,
                    CustomerPhoneNos = Cprofile.CustomerPhoneNos
                };
                if (loginPageModel.ThemeID == 4)
                {
                    IBannerML bannerML = new ResourceML(_accessor, _env);
                    loginPageModel.BGServiceImgURLs = bannerML.SiteGetServices(loginPageModel.WID, loginPageModel.ThemeID);
                }
                if (loginPageModel.ThemeID == 5)
                {
                    return View("Index_5", loginPageModel);
                }
                return View(loginPageModel);
            }
        }

        [HttpPost]
        [Route("/Login")]
        public IActionResult LoginCheck([FromBody] LoginDetail loginDetail)
        {
            IResponseStatus responseStatus = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvalidLogin
            };
            if (loginDetail == null)
            {
                return Json(responseStatus);
            }
            if (!loginDetail.LoginTypeID.In(LoginType.ApplicationUser, LoginType.CustomerCare, LoginType.Employee))
            {
                responseStatus.Msg = "Choose user login type!";
                return Json(responseStatus);
            }
            if (ApplicationSetting.WithCustomLoginID && loginDetail.LoginTypeID == LoginType.ApplicationUser)
            {
                loginDetail.Prefix = string.Empty;
                loginDetail.CommonStr2 = loginDetail.LoginMobile;
                loginDetail.LoginMobile = string.Empty;
            }
            else if (!Validate.O.IsMobile(loginDetail.LoginMobile))
            {
                loginDetail.Prefix = Validate.O.Prefix(loginDetail.LoginMobile);
                if (Validate.O.IsNumeric(loginDetail.Prefix))
                    return Json(responseStatus);
                string loginID = Validate.O.LoginID(loginDetail.LoginMobile);
                if (!Validate.O.IsNumeric(loginID))
                {
                    return Json(responseStatus);
                }
                loginDetail.LoginID = Convert.ToInt32(loginID);
                loginDetail.LoginMobile = "";
            }
            IRequestInfo _rinfo = new RequestInfo(_accessor, _env);
            if (_rinfo.GetCurrentReqInfo().Scheme.Equals("https") && (loginDetail.Longitude==0 || loginDetail.Latitude == 0))
            {
                responseStatus.Statuscode = -2;
                responseStatus.Msg = "Please allow location first";
                return Json(responseStatus);
            }
            loginDetail.RequestMode = RequestMode.PANEL;
            loginDetail.Password = HashEncryption.O.Encrypt(loginDetail.Password);
            responseStatus = _loginML.DoLogin(loginDetail);
            return Json(new { responseStatus.Statuscode, responseStatus.Msg, Path = responseStatus.CommonStr, IsBrowserBlock = responseStatus.CommonBool });
        }

        [HttpPost]
        [Route("/ResendOTP")]
        public IActionResult ResendOTP()
        {
            var responseStatus = _loginML.ResendLoginOTP();
            return Json(new { responseStatus.Statuscode, responseStatus.Msg });
        }

        [HttpPost]
        public IActionResult OTP([FromBody] LoginDetail loginDetail)
        {
            var responseStatus = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvalidLogin
            };
            if (loginDetail == null)
                return Json(new { statuscode = LoginResponseCode.FAILED });
            loginDetail.RequestMode = RequestMode.PANEL;

            responseStatus = _loginML.ValidateOTP(loginDetail.OTP);
            return Json(new { responseStatus.Statuscode, responseStatus.Msg, Path = responseStatus.CommonStr });
        }

        [HttpPost]
        [Route("/Login/LoginInfo")]
        [Route("/LoginInfo")]
        public IActionResult LoginInfo([FromBody] LoginDetail loginDetail)
        {
            loginDetail = loginDetail != null ? loginDetail : new LoginDetail();
            loginDetail.LoginTypeID = loginDetail.LoginTypeID == 0 ? 1 : loginDetail.LoginTypeID;
            return Json(_loginML.GetLoginDetails(loginDetail.LoginTypeID));
        }

        [HttpPost]
        [Route("/forgetPopUp")]
        public IActionResult forgetPopUp()
        {
            return PartialView("Partial/_forget");
        }
        [HttpPost]
        [Route("forget")]
        public IActionResult Forget([FromBody] LoginDetail loginDetail)
        {
            var responseStatus = new ForgetPasss
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvalidLogin
            };
            if (loginDetail == null)
            {
                return Json(responseStatus);
            }
            if (!loginDetail.LoginTypeID.In(LoginType.ApplicationUser, LoginType.CustomerCare, LoginType.Employee))
            {
                responseStatus.Msg = "Choose user login type!";
                return Json(responseStatus);
            }
            if (!Validate.O.IsMobile(loginDetail.LoginMobile))
            {
                loginDetail.Prefix = Validate.O.Prefix(loginDetail.LoginMobile);
                if (Validate.O.IsNumeric(loginDetail.Prefix))
                    return Json(responseStatus);
                string loginID = Validate.O.LoginID(loginDetail.LoginMobile);
                if (!Validate.O.IsNumeric(loginID))
                {
                    return Json(responseStatus);
                }
                loginDetail.LoginID = Convert.ToInt32(loginID);
                loginDetail.LoginMobile = "";
            }
            loginDetail.RequestMode = RequestMode.PANEL;
            return Json(_loginML.Forget(loginDetail));
        }

        [HttpGet]
        [Route("Reactivate.jsp")]
        public IActionResult ReActivateAccount(string encdata)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = "Oops! The link you are requested is not valid."
            };
            var decsalt = HashEncryption.O.ConvertHexToString(encdata);
            var decdata = HashEncryption.O.Decrypt(decsalt);
            if (decdata.Split('_').Length == 3)
            {
                var dt = Convert.ToDateTime(decdata.Split('_')[2].Replace("|", "/"));
                var ts = DateTime.Now - dt;
                if (ts.TotalHours < 24)
                {
                    try
                    {
                        var LT = Convert.ToInt32(decdata.Split('_')[0]);
                        var UserID = Convert.ToInt32(decdata.Split('_')[1]);
                        var lml = new LoginML(_accessor, _env, false);

                        var loginAttempt = lml.CheckInvalidAttempt(LT, UserID, false, false, false, "");
                        if (loginAttempt.Statuscode == ErrorCodes.One)
                        {
                            if (loginAttempt.Msg.Contains(((char)160).ToString()))
                            {
                                res.Statuscode = ErrorCodes.One;
                                res.Msg = "Wow! Your account has been reactivated.";
                            }
                        }

                    }
                    catch (Exception)
                    {
                    }

                }
                else
                {
                    res.Msg = "Oops! Your reactivation link has been expired.";
                }
            }
            return View("ActivateAccount", res);
        }
        [HttpPost]
        [Route("/BeforeLoginPopup")]
        public IActionResult BeforeWebsitePopUp()
        {
            var WebInfo = _loginML.GetPopupInfo();
            if (WebInfo.IsBeforeLoginPopup)
            {
                return PartialView("Partial/_BeforLoginPopup", WebInfo);
            }
            else
            {
                return Ok();
            }
        }
        [HttpGet]
        [Route("/Signup")]
        public IActionResult SignUp(int rid)
        {
            if (!ApplicationSetting.IsSingupPageOff)
            {
                rid = rid < 1 ? 1 : rid;
                ISettingML _settingML = new SettingML(_accessor, _env);
                var WebInfo = _loginML.GetWebsiteInfo();
                UserML userML = new UserML(_accessor, _env);
                var Cprofile = userML.GetCompanyProfileApp(WebInfo.WID);
                var loginPageModel = new LoginPageModel
                {
                    WID = WebInfo.WID,
                    Host = WebInfo.AbsoluteHost,
                    ThemeID = WebInfo.ThemeId,
                    AppName = Cprofile.AppName,
                    CustomerCareMobileNos = Cprofile.CustomerCareMobileNos,
                    CustomerPhoneNos = Cprofile.CustomerPhoneNos,
                    referralRoleMaster = new ReferralRoleMaster
                    {
                        ReferralID = rid,
                        Roles = _settingML.GetRoleForReferral(rid)
                    }
                };
                if (loginPageModel.ThemeID == 4)
                {
                    IBannerML bannerML = new ResourceML(_accessor, _env);
                    loginPageModel.BGServiceImgURLs = bannerML.SiteGetServices(loginPageModel.WID, loginPageModel.ThemeID);
                }
                return View("SignUp", loginPageModel);
            }
            return Ok();
        }


        [HttpPost]
        [Route("Signup")]
        public IActionResult Signup([FromBody] UserCreate UserCreate)
        {
            if (!ApplicationSetting.IsSingupPageOff)
            {
                IUserML _UserML = new UserML(_accessor, _env);
                IResponseStatus _resp = _UserML.CallSignup(UserCreate);
                return Json(_resp);
            }
            return Ok();
        }

        #region IsEmailVerified

        [HttpPost]
        [Route("/IsEmailVerified")]
        public IActionResult IsEmailVerified(int OID)
        {
            bool res = false;
            if (ApplicationSetting.IsEmailVefiricationRequired)
            {
                IUserML ml = new UserML(_accessor, _env);
                res = ml.IsEMailVerified();
            }
            return Json(new { IsRequired = ApplicationSetting.IsEmailVefiricationRequired, IsEmailVerified = res });
        }

        [HttpPost]
        [Route("/SendVerifyEmail")]
        public IActionResult SendVerifyEmail()
        {
            IUserML ml = new UserML(_accessor, _env);
            var res = ml.SendVerifyEmailLink();
            return Json(new { status = res });
        }

        [HttpGet]
        [Route("/VerifyEmail")]
        public IActionResult VerifyEmail(string encdata)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = "Oops! The link you are requested is not valid."
            };
            var decsalt = HashEncryption.O.ConvertHexToString(encdata);
            var decdata = HashEncryption.O.Decrypt(decsalt);
            if (decdata.Split('_').Length == 3)
            {
                var userID = decdata.Split('_')[1];
                var dt = Convert.ToDateTime(decdata.Split('_')[2].Replace("|", "/"));
                var ts = DateTime.Now - dt;
                if (ts.TotalHours < 24)
                {
                    try
                    {
                        var LT = Convert.ToInt32(decdata.Split('_')[0]);
                        var UserID = Convert.ToInt32(decdata.Split('_')[1]);
                        IUserML mL = new UserML(_accessor, _env, false);
                        res = mL.EMailVerify(Convert.ToInt32(userID));
                        res.CommonStr = "EmailVerification";
                    }
                    catch (Exception)
                    {

                    }
                }
                else
                {
                    res.Msg = "Oops! Your Verification link has been expired.";
                }
            }
            return View("~/Views/Login/EmailVerification.cshtml", res);
        }
        #endregion
        [HttpPost]
        [Route("l-news")]
        public IActionResult NewsOnLogin()
        {
            var model = new List<News>();
            model = _loginML.LoginPageNews();
            string NewsDetail = "";
            if (model.Any())
            {
                NewsDetail = string.Join(" || ", model.Select(x => x.NewsDetail)).Replace("</p> || <p>", " || ").Replace("<p>", "<span>").Replace("</p>", "</span>").Replace("<span>&nbsp;</span>", "");
            }
            return Json(new { NewsDetail });
        }

        public bool VerifyGoogleAuthenticatorSetup(string googlePin, string accountSecretKey)
        {
            TwoFactorAuthenticator Authenticator = new TwoFactorAuthenticator();
            return Authenticator.ValidateTwoFactorPIN(accountSecretKey, googlePin);
        }

        [HttpPost]
        public IActionResult VerifyGoogleAuthenticatorSetup([FromBody] LoginDetail loginDetail)
        {
            var response = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = "Please Fill GooglePin."
            };
            if (loginDetail != null && !string.IsNullOrEmpty(loginDetail.GooglePin))
                response = _loginML.VerifyGoogleAuthenticatorSetup(loginDetail.GooglePin);
            return Json(new { response.Statuscode, response.Msg, Path = response.CommonStr });
        }

        [HttpGet]
        [Route("block")]
        public async Task<IActionResult> BlockAsync(string encdata)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = "Oops! The link you are requested is not valid."
            };
            try
            {
                var decsalt = HashEncryption.O.ConvertHexToString(encdata);
                var decdata = HashEncryption.O.Decrypt(decsalt);
                var info = decdata.Split('_');
                if (info.Length >= 3)
                {
                    var dt = Convert.ToDateTime(info[2].Replace("|", "/"));
                    var ts = DateTime.Now - dt;
                    if (ts.TotalHours < 24)
                    {
                        var LT = Convert.ToInt32(info[0]);
                        int UserID = Convert.ToInt32(info[1]);
                        string ip = info[3];
                        string browser = info[4];
                        string IMEI = info[5];
                        var lml = new LoginML(_accessor, _env, false);
                        res = await lml.BlockAccount(UserID, IMEI, ip, browser);
                    }
                    else
                    {
                        res.Msg = "Oops! Your link has been expired.";
                    }
                }
            }
            catch (Exception)
            {

            }
            return View("ActivateAccount", res);
        }





        [HttpPost]
        [Route("/Unlockme")]
        public IActionResult Unlockme()
        {
            return PartialView("Partial/_UnlockMe");
        }
        [HttpPost]
        [Route("Unlock")]
        public IActionResult Unlock([FromBody] LoginDetail loginDetail)
        {
            var responseStatus = new ForgetPasss
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvalidLogin
            };
            if (loginDetail == null)
            {
                return Json(responseStatus);
            }
            if (!loginDetail.LoginTypeID.In(LoginType.ApplicationUser, LoginType.CustomerCare, LoginType.Employee))
            {
                responseStatus.Msg = "Choose user Unlock  type!";
                return Json(responseStatus);
            }
            if (!Validate.O.IsMobile(loginDetail.LoginMobile))
            {
                loginDetail.Prefix = Validate.O.Prefix(loginDetail.LoginMobile);
                if (Validate.O.IsNumeric(loginDetail.Prefix))
                    return Json(responseStatus);
                string loginID = Validate.O.LoginID(loginDetail.LoginMobile);
                if (!Validate.O.IsNumeric(loginID))
                {
                    return Json(responseStatus);
                }
                loginDetail.LoginID = Convert.ToInt32(loginID);
                loginDetail.LoginMobile = "";
            }
            loginDetail.RequestMode = RequestMode.PANEL;
            return Json(_loginML.UnlockMe(loginDetail));
        }

    }
}