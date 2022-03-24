using Fintech.AppCode.Configuration;
using Fintech.AppCode.DB;
using Fintech.AppCode.DL;
using Fintech.AppCode.HelperClass;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using Fintech.AppCode.WebRequest;
using GoogleAuthenticatorService.Core;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RoundpayFinTech;
using RoundpayFinTech.AppCode;
using RoundpayFinTech.AppCode.Configuration;
using RoundpayFinTech.AppCode.DL;
using RoundpayFinTech.AppCode.HelperClass;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.MiddleLayer;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.App;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.StaticModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Validators;

namespace Fintech.AppCode
{
    public class LoginML : ILoginML
    {
        #region Global Varibale Declaration
        private readonly IConfiguration Configuration;
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        private readonly ISession _session;
        private readonly IDAL _dal;
        private readonly IConnectionConfiguration _c;
        private readonly IRequestInfo _rinfo;
        private readonly WebsiteInfo _WInfo;
        private readonly IResourceML _resourceML;
        private string IPGeoDetailURL = "http://api.ipstack.com/{IP}?access_key={Access_Key}";
        #endregion

        public LoginML(IHttpContextAccessor accessor, IHostingEnvironment env, bool IsInSession = true)
        {
            _accessor = accessor;
            _env = env;
            _c = new ConnectionConfiguration(_accessor, _env);
            _dal = new DAL(_c.GetConnectionString());
            _rinfo = new RequestInfo(_accessor, _env);
            bool IsProd = _env.IsProduction();
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory());
            builder.AddJsonFile((IsProd ? "appsettings.json" : "appsettings.Development.json"));
            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
            _WInfo = GetWebsiteInfo();
            if (IsInSession)
            {
                _session = _accessor.HttpContext.Session;
            }
            _resourceML = new ResourceML(_accessor, _env);
        }
        public void SetLaunchPreference(string LP, LoginResponse _lr)
        {
            if (_lr != null)
            {

                var req = new CommonReq()
                {
                    LoginTypeID = _lr.LoginTypeID,
                    UserID = _lr.UserID,
                    CommonStr = LP
                };
                IProcedure proc = new ProcUpdateUserLaunchPreferences(_dal);
                var res = (ResponseStatus)proc.Call(req);
                //Step 1 UpdateDB
                //After successfull
                if (res.Statuscode == ErrorCodes.One)
                {
                    _lr.LaunchPreferences = LP;
                    _session.SetObjectAsJson(SessionKeys.LoginResponse, _lr);
                }
            }

        }
        public WebsiteInfo GetWebsitePackage(int ID)
        {
            if (ID > 0)
            {
                IProcedure _proc = new ProcGetWebsitePackage(_dal);
                return (WebsiteInfo)_proc.Call(new CommonReq { CommonInt = ID });
            }
            return new WebsiteInfo();
        }
        public LoginML(IHttpContextAccessor accessor, IHostingEnvironment env, string domain)
        {
            _accessor = accessor;
            _env = env;
            _c = new ConnectionConfiguration(_accessor, _env);
            _dal = new DAL(_c.GetConnectionString());
            _rinfo = new RequestInfo(_accessor, _env);
            bool IsProd = _env.IsProduction();
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory());
            builder.AddJsonFile((IsProd ? "appsettings.json" : "appsettings.Development.json"));
            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
            _WInfo = GetWebsiteInfo(domain);
            _resourceML = new ResourceML(_accessor, _env);
        }
        public ResponseStatus DoLogin(LoginDetail loginDetail)
        {
            loginDetail.RequestIP = _rinfo.GetRemoteIP();
            loginDetail.Browser = _rinfo.GetBrowserFullInfo();
            loginDetail.UserAgent = _rinfo.GetUserAgentMD5();
            loginDetail.WID = _WInfo.WID;
            var responseStatus = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvalidLogin,
            };
            if (loginDetail.WID < 0)
            {
                responseStatus.Msg = ErrorCodes.NotRecogRouteLogin;
                return responseStatus;
            }
            var _p = new ProcLogin(_dal);
            var _lr = (LoginResponse)_p.Call(loginDetail);
            _lr.LoginTypeID = loginDetail.LoginTypeID;
            _lr.LoginType = LoginType.GetLoginType(loginDetail.LoginTypeID);
            responseStatus.Msg = _lr.Msg;
            int wId = _lr.WID <= 0 ? loginDetail.WID : _lr.WID;
            if (_lr.ResultCode < 1)
            {
                if (_lr.UserID > 0)
                {
                    var resCheckInvalidAttempt = CheckInvalidAttempt(_lr.LoginTypeID, _lr.UserID, true, false, true, loginDetail.CommonStr);
                    if (resCheckInvalidAttempt.Statuscode == ErrorCodes.Minus1)
                    {
                        if ((resCheckInvalidAttempt.CommonStr ?? "").Trim() != "")
                        {
                            //Task.Factory.StartNew(() => SendLoginAlert(string.Empty, _lr.LoginTypeID, _lr.UserID, loginDetail.RequestIP, loginDetail.Browser, wId, _lr.RoleID, true));
                            SendLoginAlert(string.Empty, _lr.LoginTypeID, _lr.UserID, loginDetail.RequestIP, loginDetail.Browser, wId, _lr.RoleID, true);
                        }
                        responseStatus.Msg = resCheckInvalidAttempt.Msg;
                        responseStatus.CommonBool = _lr.RoleID == Role.Admin ? true : false;
                    }
                }
                return responseStatus;
            }
            if (_lr.LoginTypeID == LoginType.ApplicationUser && _lr.RoleID == Role.Admin)
            {
                if (!ValidateProject())
                {
                    responseStatus.Statuscode = ErrorCodes.Minus1;
                    responseStatus.Msg = ErrorCodes.PROJEXPIRE;
                    return responseStatus;
                }
            }
            if (_lr.LoginTypeID == LoginType.CustomerCare)
            {
                ICustomercareML customercareML = new CustomercareML(_accessor, _env);
                _lr.operationsAssigned = customercareML.GetOperationAssigneds(_lr.RoleID);
            }
            if (_lr.LoginTypeID == LoginType.Employee)
                _session.SetObjectAsJson(SessionKeys.LoginResponseEmp, _lr);
            else
                _session.SetObjectAsJson(SessionKeys.LoginResponse, _lr);
            if (!string.IsNullOrEmpty(_lr.OTP))
            {
                IUserML uml = new UserML(_accessor, _env);
                var alertData = uml.GetUserDeatilForAlert(_lr.UserID);
                alertData.FormatID = MessageFormat.OTP;
                if (alertData.Statuscode == ErrorCodes.One)
                {
                    IAlertML alertMl = new AlertML(_accessor, _env);
                    Parallel.Invoke(() => alertData.OTP = _lr.OTP,
                    () => alertMl.OTPSMS(alertData),
                    () => alertMl.OTPEmail(alertData),
                    () => alertMl.SocialAlert(alertData));
                }
                responseStatus.Statuscode = LoginResponseCode.OTP;
                responseStatus.Msg = "Enter OTP!";
                if (ApplicationSetting.IsRPOnly && _lr.RoleID == Role.Admin)
                {
                    SendLoginAlert(string.Empty, _lr.LoginTypeID, _lr.UserID, loginDetail.RequestIP, loginDetail.Browser, wId, _lr.RoleID);
                    //Task.Factory.StartNew(() => SendLoginAlert(string.Empty, _lr.LoginTypeID, _lr.UserID, loginDetail.RequestIP, loginDetail.Browser, wId, _lr.RoleID));
                }
                return responseStatus;
            }
            if (_lr.IsGoogle2FAEnable && !_lr.IsDeviceAuthenticated)
            {
                responseStatus.Statuscode = LoginResponseCode.Google2FAEnabled;
                responseStatus.Msg = "Please Enter Google Authenticator PIN";
                if (ApplicationSetting.IsRPOnly && _lr.RoleID == Role.Admin)
                {
                    SendLoginAlert(string.Empty, _lr.LoginTypeID, _lr.UserID, loginDetail.RequestIP, loginDetail.Browser, wId, _lr.RoleID);
                    //Task.Factory.StartNew(() => SendLoginAlert(string.Empty, _lr.LoginTypeID, _lr.UserID, loginDetail.RequestIP, loginDetail.Browser, wId, _lr.RoleID));
                }
                return responseStatus;
            }
            _session.SetString(SessionKeys.AppSessionID, _lr.SessionID);
            CookieHelper cookie = new CookieHelper(_accessor);
            byte[] SessionID = Encoding.ASCII.GetBytes(_lr.SessionID);
            cookie.Set(SessionKeys.AppSessionID, Base64UrlTextEncoder.Encode(SessionID), _lr.CookieExpire);
            responseStatus.Statuscode = LoginResponseCode.SUCCESS;
            responseStatus.Msg = "Login successfull!";
            responseStatus.CommonStr = GetDashboardPath(_lr);
            //if (ApplicationSetting.IsRPOnly && _lr.RoleID == Role.Admin)
            //{
            //    Task.Factory.StartNew(() => SendLoginAlert(string.Empty, _lr.LoginTypeID, _lr.UserID, loginDetail.RequestIP, loginDetail.Browser, wId, _lr.RoleID));
            //}
            return responseStatus;
        }
        public ResponseStatus ResendLoginOTP()
        {
            var responseStatus = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = "Invalid Request"
            };
            var _lr = _session.GetObjectFromJson<LoginResponse>(SessionKeys.LoginResponse);
            if (_lr == null)
            {
                return responseStatus;
            }
            if (string.IsNullOrEmpty(_lr.OTP) || string.IsNullOrWhiteSpace(_lr.OTP))
            {
                return responseStatus;
            }
            IUserML uml = new UserML(_accessor, _env);
            var alertData = uml.GetUserDeatilForAlert(_lr.UserID);
            alertData.FormatID = MessageFormat.OTP;
            if (alertData.Statuscode == ErrorCodes.One)
            {
                IAlertML alertMl = new AlertML(_accessor, _env);
                alertData.OTP = _lr.OTP;
                alertMl.OTPSMS(alertData);
                alertMl.OTPEmail(alertData);
                alertMl.SocialAlert(alertData);
            }
            responseStatus.Statuscode = LoginResponseCode.OTP;
            responseStatus.Msg = ErrorCodes.DMTOSS;
            return responseStatus;
        }
        public ResponseStatus ResendLoginOTPApp(LoginResponse _lresp, string browser, string IMEI)
        {
            var responseStatus = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = "Invalid Request"
            };
            var _lr = _lresp;
            if (_lr == null)
            {
                return responseStatus;
            }
            var _req = new LoginDetail
            {
                LoginTypeID = _lresp.LoginTypeID,
                LoginID = _lresp.UserID,
                SessID = _lresp.SessID,
                SessionID = _lresp.SessionID,
                IsOTPMatchUpdate = true,
                CookieExpireTime = _lresp.CookieExpire,
                RequestMode = RequestMode.APPS,
                RequestIP = _rinfo.GetRemoteIP(),
                Browser = browser,
                CommonStr = IMEI,
                WID = _lresp.WID,
                CommonBool = true
            };
            IProcedure proc = new ProcCheckSession(_dal);
            var OtpResp = (LoginResponse)proc.Call(_req);
            //if (string.IsNullOrEmpty(_lr.OTP) || string.IsNullOrWhiteSpace(_lr.OTP))
            //{
            //    return responseStatus;
            //}
            if (OtpResp.ResultCode != 3)
            {
                return responseStatus;
            }
            IUserML uml = new UserML(_accessor, _env);
            var alertData = uml.GetUserDeatilForAlert(_lr.UserID);
            alertData.FormatID = MessageFormat.OTP;
            if (alertData.Statuscode == ErrorCodes.One)
            {
                IAlertML alertMl = new AlertML(_accessor, _env);
                alertData.OTP = OtpResp.OTP;
                alertMl.OTPSMS(alertData);
                alertMl.OTPEmail(alertData);
                alertMl.SocialAlert(alertData);
            }
            responseStatus.Statuscode = LoginResponseCode.OTP;
            responseStatus.Msg = ErrorCodes.DMTOSS;
            return responseStatus;
        }
        public AppLoginResponse DoAppLogin(LoginDetail loginDetail)
        {
            loginDetail.WID = _WInfo.WID;
            var responseStatus = new AppLoginResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvalidLogin
            };
            if (loginDetail.WID < 0)
            {
                responseStatus.Msg = ErrorCodes.NotRecogRouteLogin;
                return responseStatus;
            }
            loginDetail.RequestIP = _rinfo.GetRemoteIP();
            loginDetail.UserAgent = _rinfo.GetUserAgentMD5();
            IProcedure _p = new ProcLogin(_dal);
            var _lr = (LoginResponse)_p.Call(loginDetail);
            _lr.LoginTypeID = loginDetail.LoginTypeID;
            _lr.LoginType = LoginType.GetLoginType(loginDetail.LoginTypeID);
            responseStatus.Msg = _lr.Msg;
            if (_lr.ResultCode < 1)
            {
                if (_lr.UserID > 0)
                {
                    var resCheckInvalidAttempt = CheckInvalidAttempt(_lr.LoginTypeID, _lr.UserID, true, false, true, loginDetail.CommonStr);
                    if (resCheckInvalidAttempt.Statuscode == ErrorCodes.Minus1)
                    {
                        if ((resCheckInvalidAttempt.CommonStr ?? "").Trim() != "")
                        {
                            var activationlink = GenerateActvationLink(_lr.LoginTypeID, _lr.UserID);
                            var emailBody = new StringBuilder();
                            emailBody.AppendFormat(ErrorCodes.SuspeciousMsg, activationlink);
                            IEmailML emailManager = new EmailML(_dal);
                            int WID = 0;
                            if (_lr.WID <= 0)
                            {
                                WID = loginDetail.WID;
                                _lr.WID = WID;
                            }
                            else
                            {
                                WID = _lr.WID;
                            }
                            emailManager.SendMail(resCheckInvalidAttempt.CommonStr, null, ErrorCodes.Suspecious, emailBody.ToString(), WID, _resourceML.GetLogoURL(WID).ToString());
                        }
                        responseStatus.Msg = resCheckInvalidAttempt.Msg;

                    }
                }
                return responseStatus;
            }
            if (_lr.LoginTypeID == LoginType.ApplicationUser && _lr.RoleID == Role.Admin)
            {
                if (!ValidateProject())
                {
                    responseStatus.Statuscode = ErrorCodes.Minus1;
                    responseStatus.Msg = ErrorCodes.PROJEXPIRE;
                    return responseStatus;
                }
            }
            if (!string.IsNullOrEmpty(_lr.OTP))
            {
                IUserML uml = new UserML(_accessor, _env);
                var alertData = uml.GetUserDeatilForAlert(_lr.UserID);
                alertData.FormatID = MessageFormat.OTP;
                if (alertData.Statuscode == ErrorCodes.One)
                {
                    IAlertML alertMl = new AlertML(_accessor, _env);
                    alertData.OTP = _lr.OTP;
                    alertMl.OTPSMS(alertData);
                    alertMl.OTPEmail(alertData);
                    alertMl.SocialAlert(alertData);
                }
                responseStatus.Statuscode = LoginResponseCode.OTP;
                responseStatus.Msg = "Enter OTP!";
                _lr.OTP = "RESET";
                responseStatus.OTPSession = HashEncryption.O.Encrypt(JsonConvert.SerializeObject(_lr));
                return responseStatus;
            }

            /* Google Auth */
            if (_lr.IsGoogle2FAEnable && !_lr.IsDeviceAuthenticated)
            {
                responseStatus.Statuscode = LoginResponseCode.Google2FAEnabled;
                responseStatus.Msg = "Please Enter Google Authenticator PIN";
                responseStatus.OTPSession = HashEncryption.O.Encrypt(JsonConvert.SerializeObject(_lr));
                SendLoginAlert(string.Empty, _lr.LoginTypeID, _lr.UserID, loginDetail.RequestIP, loginDetail.Browser, _lr.WID, _lr.RoleID);
                return responseStatus;
            }
            /*---=========End ================---*/
            responseStatus.Statuscode = LoginResponseCode.SUCCESS;
            responseStatus.Msg = "Login successfull!";
            var loginData = new LoginData
            {
                Name = _lr.OutletName,
                RoleName = _lr.RoleName,
                RoleID = _lr.RoleID,
                SlabID = _lr.SlabID,
                SessionID = _lr.SessID,
                Session = _lr.SessionID,
                UserID = _lr.UserID,
                LoginTypeID = _lr.LoginTypeID,
                MobileNo = _lr.MobileNo,
                EmailID = _lr.EmailID,
                OutletID = _lr.OutletID,
                IsDoubleFactor = _lr.IsDoubleFactor,
                IsPinRequired = _lr.IsPinRequired,
                IsRealAPI = _lr.IsRealAPI,
                StateID = _lr.StateID,
                State = _lr.State,
                WID = _lr.WID,
                IsDebitAllowed = _lr.IsDebitAllowed,
                Pincode = _lr.Pincode,
                AccountSecretKey = _lr.AccountSecretKey,
                IsMultiLevel=_lr.IsMultiLevel
            };
            responseStatus.Data = loginData;
            return responseStatus;
        }
        public WebsiteInfo GetWebsiteForDomain()
        {
            return _WInfo;
        }
        public AppLoginResponse DoWebAppLogin(LoginDetail loginDetail)
        {
            loginDetail.WID = _WInfo.WID;
            loginDetail.Browser = _rinfo.GetBrowserFullInfo();
            loginDetail.RequestIP = _rinfo.GetRemoteIP();
            var responseStatus = new AppLoginResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvalidLogin
            };
            if (loginDetail.WID < 0)
            {
                responseStatus.Msg = ErrorCodes.NotRecogRouteLogin;
                return responseStatus;
            }
            IProcedure _p = new ProcLogin(_dal);
            var _lr = (LoginResponse)_p.Call(loginDetail);
            _lr.LoginTypeID = loginDetail.LoginTypeID;
            _lr.LoginType = LoginType.GetLoginType(loginDetail.LoginTypeID);
            responseStatus.Msg = _lr.Msg;
            if (_lr.ResultCode < 1)
            {
                if (_lr.UserID > 0)
                {
                    var resCheckInvalidAttempt = CheckInvalidAttempt(_lr.LoginTypeID, _lr.UserID, true, false, true, loginDetail.CommonStr);
                    if (resCheckInvalidAttempt.Statuscode == ErrorCodes.Minus1)
                    {
                        if ((resCheckInvalidAttempt.CommonStr ?? "").Trim() != "")
                        {
                            var activationlink = GenerateActvationLink(_lr.LoginTypeID, _lr.UserID);
                            var emailBody = new StringBuilder();
                            emailBody.AppendFormat(ErrorCodes.SuspeciousMsg, activationlink);
                            IEmailML emailManager = new EmailML(_dal);
                            int WID = 0;
                            if (_lr.WID <= 0)
                            {
                                WID = loginDetail.WID;
                            }
                            else
                            {
                                WID = _lr.WID;
                            }
                            emailManager.SendMail(resCheckInvalidAttempt.CommonStr, null, ErrorCodes.Suspecious, emailBody.ToString(), WID, _resourceML.GetLogoURL(WID).ToString());
                        }
                        responseStatus.Msg = resCheckInvalidAttempt.Msg;
                    }
                }
                return responseStatus;
            }
            if (_lr.LoginTypeID == LoginType.ApplicationUser && _lr.RoleID == Role.Admin)
            {
                if (!ValidateProject())
                {
                    responseStatus.Statuscode = ErrorCodes.Minus1;
                    responseStatus.Msg = ErrorCodes.PROJEXPIRE;
                    return responseStatus;
                }
            }
            if (!string.IsNullOrEmpty(_lr.OTP))
            {
                IUserML uml = new UserML(_accessor, _env);
                var alertData = uml.GetUserDeatilForAlert(_lr.UserID);
                alertData.FormatID = MessageFormat.OTP;
                if (alertData.Statuscode == ErrorCodes.One)
                {
                    IAlertML alertMl = new AlertML(_accessor, _env);
                    alertData.OTP = _lr.OTP;
                    alertMl.OTPSMS(alertData);
                    alertMl.OTPEmail(alertData);
                    alertMl.SocialAlert(alertData);
                }
                responseStatus.Statuscode = LoginResponseCode.OTP;
                responseStatus.Msg = "Enter OTP!";
                responseStatus.OTPSession = HashEncryption.O.Encrypt(JsonConvert.SerializeObject(_lr));
                return responseStatus;
            }
            responseStatus.Statuscode = LoginResponseCode.SUCCESS;
            responseStatus.Msg = "Login successfull!";
            var loginData = new LoginData
            {
                Name = _lr.OutletName,
                RoleName = _lr.RoleName,
                RoleID = _lr.RoleID,
                SlabID = _lr.SlabID,
                SessionID = _lr.SessID,
                Session = _lr.SessionID,
                UserID = _lr.UserID,
                LoginTypeID = _lr.LoginTypeID,
                MobileNo = _lr.MobileNo,
                EmailID = _lr.EmailID,
                OutletID = _lr.OutletID,
                IsDoubleFactor = _lr.IsDoubleFactor,
                IsPinRequired = _lr.IsPinRequired,
                WID=_lr.WID
            };
            responseStatus.Data = loginData;
            return responseStatus;
        }
        public IResponseStatus SaveFCMID(CommonReq commonReq)
        {
            IProcedure proc = new ProcSaveFCMID(_dal);
            return (ResponseStatus)proc.Call(commonReq);
        }
        private string GetDashboardPath(LoginResponse _lr)
        {
            if (_lr.LoginTypeID == LoginType.ApplicationUser)
            {
                if (_lr.RoleID == Role.Admin)
                    return "Admin";
                else if (_lr.RoleID == Role.Retailor_Seller)
                {

                    if (ApplicationSetting.DynamicWebsiteType == Fintech.AppCode.StaticModel.DynamicWebsiteType.Disabled)
                    {
                        return "Seller";
                    }
                    else
                    {
                        if (_lr.LaunchPreferences == LaunchPreferences.DynamicWebSiteIndex)
                        {
                            return "DynamicWebsite";
                        }
                        else
                        {
                            return "Seller";
                        }

                    }

                }


                else if (_lr.RoleID == Role.APIUser)
                    return "APIUser";
                else if (_lr.RoleID == Role.Customer)
                    return "Customer";
                else if (_lr.RoleID == Role.MasterWL)
                    return "MasterWL";
                else if (_lr.RoleID == Role.FOS)
                    return "FOS";
                else if (_lr.RoleID > 0)
                    return "User";
            }
            else if (_lr.LoginTypeID == LoginType.CustomerCare)
            {
                return "CustomerCare";
            }
            else if (_lr.LoginTypeID == LoginType.Employee)
            {
                return "Employee";
            }
            return string.Empty;
        }
        public void SendOTP(string OTP, string MobileNo, string EmailID, int WID)
        {
            var procSendSMS = new SMSSendREQ
            {
                FormatType = MessageFormat.OTP,
                MobileNo = MobileNo
            };
            try
            {
                DataTable Tp_ReplaceKeywords = new DataTable();
                Tp_ReplaceKeywords.Columns.Add("Keyword", typeof(string));
                Tp_ReplaceKeywords.Columns.Add("ReplaceValue", typeof(string));
                object[] param = new object[2];
                param[0] = MessageTemplateKeywords.OTP;
                param[1] = OTP;
                Tp_ReplaceKeywords.Rows.Add(param);
                procSendSMS.Tp_ReplaceKeywords = Tp_ReplaceKeywords;
                procSendSMS.WID = WID;
                ISendSMSML sMSManager = new SendSMSML(_dal);
                SMSSendResp smsResponse = (SMSSendResp)sMSManager.SendSMS(procSendSMS);
                if (!string.IsNullOrEmpty(smsResponse.SMS) && !string.IsNullOrEmpty(EmailID))
                {
                    IEmailML emailManager = new EmailML(_dal);
                    emailManager.SendMail(EmailID, null, "One Time Password", smsResponse.SMS, WID, _resourceML.GetLogoURL(WID).ToString());
                }
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "SendOTP",
                    Error = "MobileNo:" + MobileNo + ",EmailID:" + EmailID + ",OTP:" + OTP + ",Ex:" + ex.Message,
                    LoginTypeID = WID,
                    UserId = 0
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
        }
        public ResponseStatus ValidateOTP(string OTP)
        {
            var responseStatus = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = nameof(ErrorCodes.Invalid_OTP).Replace("_", " ")
            };
            if (string.IsNullOrWhiteSpace(OTP) || string.IsNullOrEmpty(OTP))
            {
                return responseStatus;
            }
            var _lr = _session.GetObjectFromJson<LoginResponse>(SessionKeys.LoginResponse);
            if (_lr == null)
            {
                return responseStatus;
            }
            if (string.IsNullOrEmpty(_lr.OTP) || string.IsNullOrWhiteSpace(_lr.OTP))
            {
                return responseStatus;
            }
            if (OTP != _lr.OTP || _lr.SessID == 0x0)
            {
                if (_lr.UserID > 0)
                {
                    var resCheckInvalidAttempt = CheckInvalidAttempt(_lr.LoginTypeID, _lr.UserID, true, false, true, "");
                    if (resCheckInvalidAttempt.Statuscode == ErrorCodes.Minus1)
                    {
                        if ((resCheckInvalidAttempt.CommonStr ?? "").Trim() != "")
                        {
                            var activationlink = GenerateActvationLink(_lr.LoginTypeID, _lr.UserID);
                            var emailBody = new StringBuilder();
                            emailBody.AppendFormat(ErrorCodes.SuspeciousMsg, activationlink);
                            IEmailML emailManager = new EmailML(_dal);
                            emailManager.SendMail(resCheckInvalidAttempt.CommonStr, null, ErrorCodes.Suspecious, emailBody.ToString(), _lr.WID, _resourceML.GetLogoURL(_lr.WID).ToString());
                        }
                        responseStatus.Msg = resCheckInvalidAttempt.Msg;
                    }
                }
                return responseStatus;
            }
            var _req = new LoginDetail
            {
                LoginTypeID = _lr.LoginTypeID,
                LoginID = _lr.UserID,
                SessID = _lr.SessID,
                SessionID = _lr.SessionID,
                IsOTPMatchUpdate = true,
                CookieExpireTime = _lr.CookieExpire,
                RequestMode = RequestMode.PANEL,
                RequestIP = _rinfo.GetRemoteIP(),
                Browser = _rinfo.GetBrowserFullInfo()
            };
            IProcedure proc = new ProcCheckSession(_dal);
            LoginResponse OtpResp = (LoginResponse)proc.Call(_req);
            if (OtpResp.ResultCode == ErrorCodes.Minus1)
            {
                return responseStatus;
            }
            _session.SetString(SessionKeys.AppSessionID, _lr.SessionID);
            CookieHelper cookieHelper = new CookieHelper(_accessor);
            byte[] SessionID = Encoding.ASCII.GetBytes(_lr.SessionID);
            cookieHelper.Set(SessionKeys.AppSessionID, Base64UrlTextEncoder.Encode(SessionID), _lr.CookieExpire);
            responseStatus.Statuscode = ErrorCodes.One;
            responseStatus.Msg = ErrorCodes.SuccessOTP;
            responseStatus.CommonStr = GetDashboardPath(_lr);
            return responseStatus;
        }
        public AppLoginResponse ValidateOTPForApp(string OTP, LoginResponse _lresp, string browser, string IMEI)
        {
            var responseStatus = new AppLoginResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = nameof(ErrorCodes.Invalid_OTP).Replace("_", " ")
            };
            if (string.IsNullOrWhiteSpace(OTP) || string.IsNullOrEmpty(OTP))
            {
                return responseStatus;
            }
            if (_lresp == null)
            {
                return responseStatus;
            }
            if (string.IsNullOrEmpty(_lresp.OTP) || string.IsNullOrWhiteSpace(_lresp.OTP))
            {
                return responseStatus;
            }

            var _req = new LoginDetail
            {
                LoginTypeID = _lresp.LoginTypeID,
                LoginID = _lresp.UserID,
                SessID = _lresp.SessID,
                SessionID = _lresp.SessionID,
                IsOTPMatchUpdate = true,
                CookieExpireTime = _lresp.CookieExpire,
                RequestMode = RequestMode.APPS,
                RequestIP = _rinfo.GetRemoteIP(),
                Browser = browser,
                CommonStr = IMEI,
                WID = _lresp.WID,
                CommonStr2 = OTP
            };
            IProcedure proc = new ProcCheckSession(_dal);
            var OtpResp = (LoginResponse)proc.Call(_req);
            if (OtpResp.ResultCode == ErrorCodes.Minus1)
            {
                responseStatus.Msg = OtpResp.Msg;
                return responseStatus;
            }
            responseStatus.Statuscode = ErrorCodes.One;
            responseStatus.Msg = ErrorCodes.SuccessOTP;
            var loginData = new LoginData
            {
                Name = _lresp.OutletName,
                RoleName = _lresp.RoleName,
                RoleID = _lresp.RoleID,
                SlabID = _lresp.SlabID,
                SessionID = _lresp.SessID,
                UserID = _lresp.UserID,
                LoginTypeID = _lresp.LoginTypeID,
                MobileNo = _lresp.MobileNo,
                EmailID = _lresp.EmailID,
                OutletID = _lresp.OutletID,
                IsDoubleFactor = _lresp.IsDoubleFactor,
                IsPinRequired = _lresp.IsPinRequired,
                StateID = _lresp.StateID,
                State = _lresp.State,
                Session = _lresp.SessionID,
                IsDebitAllowed = _lresp.IsDebitAllowed,
                Pincode = _lresp.Pincode,
                AccountSecretKey = _lresp.AccountSecretKey,
                IsMultiLevel =_lresp.IsMultiLevel
            };
            responseStatus.Data = loginData;
            return responseStatus;
        }

        public AppLoginResponse ValidateGAuthPINForApp(string gauthPIN, LoginResponse _lresp, string browser, string IMEI)
        {
            var responseStatus = new AppLoginResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = "Invalid Google Auth PIN"
            };
            if (string.IsNullOrWhiteSpace(gauthPIN) || string.IsNullOrEmpty(gauthPIN))
            {
                return responseStatus;
            }
            if (_lresp == null)
            {
                return responseStatus;
            }
            if (string.IsNullOrEmpty(_lresp.AccountSecretKey) || string.IsNullOrWhiteSpace(_lresp.AccountSecretKey))
            {
                return responseStatus;
            }

            var _req = new LoginDetail
            {
                LoginTypeID = _lresp.LoginTypeID,
                LoginID = _lresp.UserID,
                SessID = _lresp.SessID,
                SessionID = _lresp.SessionID,
                IsOTPMatchUpdate = false,
                CookieExpireTime = _lresp.CookieExpire,
                RequestMode = RequestMode.APPS,
                RequestIP = _rinfo.GetRemoteIP(),
                Browser = browser,
                CommonStr = IMEI,
                WID = _lresp.WID,
                //CommonStr2 = OTP,
                CommonBool1 = true
            };

            if (!VerifyGoogleAuthenticator(gauthPIN, _lresp.AccountSecretKey))
            {
                return responseStatus;
            }
            _req.CommonBool1 = true;
            IProcedure proc = new ProcCheckSession(_dal);
            var OtpResp = (LoginResponse)proc.Call(_req);
            if (OtpResp.ResultCode == ErrorCodes.Minus1)
            {
                responseStatus.Msg = OtpResp.Msg;
                return responseStatus;
            }
            responseStatus.Statuscode = ErrorCodes.One;
            responseStatus.Msg = ErrorCodes.SuccessOTP;
            var loginData = new LoginData
            {
                Name = _lresp.OutletName,
                RoleName = _lresp.RoleName,
                RoleID = _lresp.RoleID,
                SlabID = _lresp.SlabID,
                SessionID = _lresp.SessID,
                UserID = _lresp.UserID,
                LoginTypeID = _lresp.LoginTypeID,
                MobileNo = _lresp.MobileNo,
                EmailID = _lresp.EmailID,
                OutletID = _lresp.OutletID,
                IsDoubleFactor = _lresp.IsDoubleFactor,
                IsPinRequired = _lresp.IsPinRequired,
                StateID = _lresp.StateID,
                State = _lresp.State,
                Session = _lresp.SessionID,
                IsDebitAllowed = _lresp.IsDebitAllowed,
                Pincode = _lresp.Pincode,
                AccountSecretKey = _lresp.AccountSecretKey
            };
            responseStatus.Data = loginData;
            return responseStatus;
        }

        public AppLoginResponse ValidateOTPForWebApp(string OTP, LoginResponse _lresp)
        {
            var responseStatus = new AppLoginResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = nameof(ErrorCodes.Invalid_OTP).Replace("_", " ")
            };
            if (string.IsNullOrWhiteSpace(OTP) || string.IsNullOrEmpty(OTP))
            {
                return responseStatus;
            }
            if (_lresp == null)
            {
                return responseStatus;
            }
            if (string.IsNullOrEmpty(_lresp.OTP) || string.IsNullOrWhiteSpace(_lresp.OTP))
            {
                return responseStatus;
            }
            if (OTP != _lresp.OTP || _lresp.SessID == 0x0)
            {
                if (_lresp.UserID > 0)
                {
                    var resCheckInvalidAttempt = CheckInvalidAttempt(_lresp.LoginTypeID, _lresp.UserID, true, false, true, nameof(RequestMode.WEBAPPS));
                    if (resCheckInvalidAttempt.Statuscode == ErrorCodes.Minus1)
                    {
                        if ((resCheckInvalidAttempt.CommonStr ?? "").Trim() != "")
                        {
                            var activationlink = GenerateActvationLink(_lresp.LoginTypeID, _lresp.UserID);
                            var emailBody = new StringBuilder();
                            emailBody.AppendFormat(ErrorCodes.SuspeciousMsg, activationlink);
                            IEmailML emailManager = new EmailML(_dal);
                            emailManager.SendMail(resCheckInvalidAttempt.CommonStr, null, ErrorCodes.Suspecious, emailBody.ToString(), _lresp.WID, _resourceML.GetLogoURL(_lresp.WID).ToString());
                        }
                        responseStatus.Msg = resCheckInvalidAttempt.Msg;
                    }
                }
                return responseStatus;
            }
            var _req = new LoginDetail
            {
                LoginTypeID = _lresp.LoginTypeID,
                LoginID = _lresp.UserID,
                SessID = _lresp.SessID,
                SessionID = _lresp.SessionID,
                IsOTPMatchUpdate = true,
                CookieExpireTime = _lresp.CookieExpire,
                RequestMode = RequestMode.WEBAPPS,
                RequestIP = _rinfo.GetRemoteIP(),
                Browser = _rinfo.GetBrowserFullInfo(),
                CommonStr = nameof(RequestMode.WEBAPPS)
            };
            IProcedure proc = new ProcCheckSession(_dal);
            var OtpResp = (LoginResponse)proc.Call(_req);
            if (OtpResp.ResultCode == ErrorCodes.Minus1)
            {
                return responseStatus;
            }
            responseStatus.Statuscode = ErrorCodes.One;
            responseStatus.Msg = ErrorCodes.SuccessOTP;
            var loginData = new LoginData
            {
                Name = _lresp.OutletName,
                RoleName = _lresp.RoleName,
                RoleID = _lresp.RoleID,
                SlabID = _lresp.SlabID,
                SessionID = _lresp.SessID,
                UserID = _lresp.UserID,
                LoginTypeID = _lresp.LoginTypeID,
                MobileNo = _lresp.MobileNo,
                EmailID = _lresp.EmailID,
                OutletID = _lresp.OutletID,
                IsDoubleFactor = _lresp.IsDoubleFactor,
                IsPinRequired = _lresp.IsPinRequired,
                Session = _lresp.SessionID,
                WID=_lresp.WID
            };
            responseStatus.Data = loginData;
            return responseStatus;
        }
        public AppResponse ValidateSessionForApp(LoginDetail loginDetail)
        {
            var responseStatus = new AppResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };


            loginDetail.RequestIP = _rinfo.GetRemoteIP();
            IProcedure proc = new ProcCheckSession(_dal);
            var OtpResp = (LoginResponse)proc.Call(loginDetail);
            if (OtpResp.ResultCode == ErrorCodes.Minus1)
            {
                responseStatus.Msg = OtpResp.Msg;
                return responseStatus;
            }
            responseStatus.Statuscode = ErrorCodes.One;
            responseStatus.Msg = ErrorCodes.ValidSession;
            responseStatus.CheckID = OtpResp.WID;
            responseStatus.MobileNo = OtpResp.MobileNo;
            responseStatus.EmailID = OtpResp.EmailID;
            responseStatus.IsPasswordExpired = OtpResp.IsPasswordExpired;
            responseStatus.GetID = OtpResp.OutletID;
            responseStatus.IsGreen = OtpResp.IsMarkedGreen;
            responseStatus.RID = OtpResp.RoleID;
            responseStatus.IsPaymentGateway = OtpResp.IsPaymentGateway;
            responseStatus.PCode = OtpResp.Pincode;
            responseStatus.B2CDomain = OtpResp.B2CDomain;
            return responseStatus;
        }
        public ResponseStatus Forget(LoginDetail loginDetail)
        {
            var response = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            if (loginDetail == null)
                goto Finish;
            loginDetail.RequestIP = _rinfo.GetRemoteIP();
            loginDetail.Browser = _rinfo.GetBrowserFullInfo();
            loginDetail.WID = _WInfo.WID;
            var _res = new AlertReplacementModel
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvalidLogin
            };
            IProcedure _p = new ProcForget(_dal);
            _res = (AlertReplacementModel)_p.Call(loginDetail);
            if (_res.Statuscode != ErrorCodes.One)
            {
                response.Msg = _res.Msg;
                goto Finish;
            }

            loginDetail.LoginID = loginDetail.LoginID == 0 ? _res.LoginID : loginDetail.LoginID;
            IAlertML alertMl = new AlertML(_accessor, _env);
            bool isOTPValid = true;
            if (_res.LoginRoleId == Role.Admin)
            {
                var sessionemailOTP = _session.GetObjectFromJson<string>(SessionKeys.EmailOTP);
                var sessionmobileOTP = _session.GetObjectFromJson<string>(SessionKeys.MobileOTP);
                if (!string.IsNullOrEmpty(sessionemailOTP) && !string.IsNullOrEmpty(sessionmobileOTP))
                {
                    if (sessionemailOTP != loginDetail.EmailOTP && sessionmobileOTP != loginDetail.MobileOTP)
                    {
                        response = new ResponseStatus
                        {
                            Statuscode = ErrorCodes.Minus1,
                            Msg = "Invalid OTP"
                        };
                        isOTPValid = false;
                        //goto Finish;
                    }
                }
                else
                {
                    var MobileOTP = HashEncryption.O.CreatePasswordNumeric(6);
                    _session.SetObjectAsJson(SessionKeys.MobileOTP, MobileOTP);
                    _res.OTP = MobileOTP;
                    alertMl.OTPSMS(_res);
                    var EmailOTP = HashEncryption.O.CreatePasswordNumeric(6);
                    _session.SetObjectAsJson(SessionKeys.EmailOTP, EmailOTP);

                    _res.OTP = EmailOTP;
                    alertMl.OTPEmail(_res);
                    response = new ResponseStatus
                    {
                        Statuscode = ErrorCodes.Two,
                        Msg = "Please enter OTP"
                    };
                    goto Finish;
                }
            }
            if (!isOTPValid)
            {
                response = new ResponseStatus
                {
                    Statuscode = ErrorCodes.Minus1,
                    Msg = "Invalid OTP"
                };
                goto Finish;
            }
            alertMl.ForgetPasswordSMS(_res);
            var isSent = alertMl.ForgetPasswordEmail(_res);
            _res.FormatID = MessageFormat.ForgetPass;
            alertMl.SocialAlert(_res);
            if (!isSent && _res.LoginRoleId == Role.Admin)
            {
                _res.IsSendFailed = true;
                isSent = alertMl.ForgetPasswordEmail(_res);
            }
            response = new ResponseStatus
            {
                Statuscode = _res.Statuscode,
                Msg = _res.Msg
            };
        Finish:
            return response;
        }
        public ResponseStatus ForgetFromApp(LoginDetail loginDetail)
        {
            loginDetail.RequestIP = _rinfo.GetRemoteIP();
            loginDetail.Browser = _rinfo.GetBrowserFullInfo();
            loginDetail.WID = _WInfo.WID;
            var _res = new AlertReplacementModel
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvalidLogin
            };
            IProcedure _p = new ProcForget(_dal);
            _res = (AlertReplacementModel)_p.Call(loginDetail);
            if (_res.Statuscode == ErrorCodes.One)
            {
                var LoginID = _res.LoginPrefix + _res.LoginID;
                if (!_res.IsPrefix)
                {
                    LoginID = _res.LoginMobileNo;
                }
                loginDetail.LoginID = loginDetail.LoginID == 0 ? _res.LoginID : loginDetail.LoginID;
                IAlertML alertMl = new AlertML(_accessor, _env);
                alertMl.ForgetPasswordSMS(_res);

                alertMl.ForgetPasswordEmail(_res);
            }
            var response = new ResponseStatus
            {
                Statuscode = _res.Statuscode,
                Msg = _res.Msg
            };
            return response;
        }
        public object GetLoginDetails(int LoginTypeID = 1)
        {
            string SessionKey = LoginTypeID == 1 ? SessionKeys.LoginResponse : SessionKeys.LoginResponseEmp;
            LoginResponse response = _session.GetObjectFromJson<LoginResponse>(SessionKey);
            if (response != null)
            {
                return (new { response.UserID, response.Name, response.OutletName, response.RoleName, response.MobileNo, response.EmailID, response.LoginType });
            }
            return (new { errorcode = "l" });
        }
        public WebsiteInfo GetWebsiteInfo()
        {
            string domain = _rinfo.GetDomain(Configuration);
            var _wi = _session != null ? _session.GetObjectFromJson<WebsiteInfo>(SessionKeys.WInfo) : null;
            bool IsCall = true;
            if (_wi != null)
            {
                if (_wi.WebsiteName == domain && _wi.WID > 0)
                {
                    IsCall = false;
                }
            }
            if (IsCall)
            {
                ProcGetWebsiteInfo procGetWebsiteInfo = new ProcGetWebsiteInfo(_dal);
                _wi = (WebsiteInfo)procGetWebsiteInfo.Call(new CommonReq { CommonStr = domain });
                if (_session != null)
                    _session.SetObjectAsJson(SessionKeys.WInfo, _wi);
            }
            var cInfo = _rinfo.GetCurrentReqInfo();
            _wi.AbsoluteHost = cInfo.Scheme + "://" + cInfo.Host + (cInfo.Port > 0 ? ":" + cInfo.Port : "");
            return _wi;
        }

        public WebsiteInfo GetWebsiteInfo(string domain)
        {
            var _wi = new WebsiteInfo
            {
                WID = 999999999,//Fixed for WHITELABEL unknown from app
                WebsiteName = domain
            };
            if (!domain.Equals("WHITELABEL"))
            {
                ProcGetWebsiteInfo procGetWebsiteInfo = new ProcGetWebsiteInfo(_dal);
                _wi = (WebsiteInfo)procGetWebsiteInfo.Call(new CommonReq { CommonStr = domain });
            }
            return _wi;
        }
        public WebsiteInfo GetWebsiteInfo(int WID)
        {
            ProcGetWebsiteInfo procGetWebsiteInfo = new ProcGetWebsiteInfo(_dal);
            return (WebsiteInfo)procGetWebsiteInfo.Call(new CommonReq { CommonInt = WID });
        }
        public bool IsInValidSession()
        {
            string AppSessionID = _session.GetString(SessionKeys.AppSessionID);
            return (string.IsNullOrEmpty(AppSessionID) || AppSessionID.Length != Numbers.THIRTY_TWO);
        }
        public async Task<IResponseStatus> DoLogout(LogoutReq logoutReq)
        {
            logoutReq.IP = _rinfo.GetRemoteIP();
            logoutReq.Browser = _rinfo.GetBrowserFullInfo();
            var resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            IProcedureAsync procLogout = new ProcLogout(_dal);
            resp = (ResponseStatus)await procLogout.Call(logoutReq);
            return resp;
        }
        private bool ValidateProject()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("http://helpdesk.mrupay.in/Checklogin.asmx/AuthenticateProjectLogin?ProjectId={0}&DomainName={1}", ApplicationSetting.ProjectID, _rinfo.GetDomain(Configuration));

            var res = "";
            Authentication authentication = new Authentication();
            try
            {
                res = AppWebRequest.O.CallUsingHttpWebRequest_GET(sb.ToString());
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "ValidateProject",
                    Error = ex.Message,
                    LoginTypeID = 1,
                    UserId = 1
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
                return true;
                //Based on assumption that API is not responding so service will continue.
            }
            try
            {
                if ((res ?? "").Trim() != "")
                {
                    authentication = XMLHelper.O.DesrializeToObject(authentication, res, "Authentication");
                    if (authentication.STATUS == ErrorCodes.One)
                    {
                        IProcedure ProcProjectValidate = new ProcProjectValidate(_dal);
                        var resUpdate = (ResponseStatus)ProcProjectValidate.Call(authentication);
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "ValidateProject",
                    Error = "(ResponseMatch)Ex:" + ex.Message + " | Res:" + res,
                    LoginTypeID = 1,
                    UserId = 1
                });
            }

            return false;
        }
        public ResponseStatus CheckInvalidAttempt(int LT, int UserID, bool IsInvalidLoginAttempt, bool IsInvalidOTPAttempt, bool InCheck, string IMEI)
        {
            string IP = _rinfo.GetRemoteIP();
            var req = new CommonReq
            {
                LoginTypeID = LT,
                LoginID = UserID,
                CommonBool = IsInvalidLoginAttempt,
                CommonBool1 = IsInvalidOTPAttempt,
                CommonBool2 = InCheck,
                CommonStr = IMEI ?? "",
                CommonStr2 = IP ?? ""
            };
            IProcedure _proc = new ProcInvalidAttempt(_dal);
            return (ResponseStatus)_proc.Call(req);
        }
        public string GenerateActvationLink(int LT, int UserID)
        {
            var crf = _rinfo.GetCurrentReqInfo();
            var salt = HashEncryption.O.Encrypt(LT + "_" + UserID + "_" + DateTime.Now.ToString("dd|MMM|yyyy hh:m:s tt"));
            StringBuilder sb = new StringBuilder();
            sb.Append(crf.Scheme);
            sb.Append("://");
            sb.Append(crf.Host);
            if (crf.Port > 0)
            {
                sb.Append(":");
                sb.Append(crf.Port);
            }
            sb.Append("/Reactivate.jsp?encdata=");
            sb.Append(HashEncryption.O.ConvertStringToHex(salt));
            return sb.ToString();
        }
        public PopupInfo GetPopupInfo()
        {
            string domain = _rinfo.GetDomain(Configuration);
            var _wi = _session.GetObjectFromJson<PopupInfo>(SessionKeys.WInfo);
            bool IsCall = true;
            if (_wi != null)
            {
                if (_wi.WebsiteName == domain)
                {
                    IsCall = false;
                }
            }
            if (IsCall)
            {
                ProcGetWebsiteInfo procGetWebsiteInfo = new ProcGetWebsiteInfo(_dal);
                _wi = (PopupInfo)procGetWebsiteInfo.Call(new CommonReq { CommonStr = domain });
                _session.SetObjectAsJson(SessionKeys.WInfo, _wi);
            }
            var cInfo = _rinfo.GetCurrentReqInfo();
            _wi.AbsoluteHost = cInfo.Scheme + "://" + cInfo.Host + (cInfo.Port > 0 ? ":" + cInfo.Port : "") + "/" + DOCType.PopupSuffix.Replace("{0}", _wi.WID.ToString());
            var root = Path.Combine(DOCType.PopupPath.Replace("{0}", _wi.WID.ToString()));
            var crf = _rinfo.GetCurrentReqInfo();

            IEnumerable<PopupInfo> Data = new PopupInfo[] { };

            Data = Directory.EnumerateFiles(root).Select(x => new PopupInfo
            {
                PopupFileName = Path.GetFileName(x).ToString(),
            });
            foreach (var pop in Data)
            {
                if (pop.PopupFileName == "AfterLogin.png")
                {
                    _wi.IsAfterLoginPopup = true;
                }
                if (pop.PopupFileName == "WebPopup.png")
                {
                    _wi.ISWebSitePopup = true;
                }
                if (pop.PopupFileName == "BeforeLogin.png")
                {
                    _wi.IsBeforeLoginPopup = true;
                }
                if (pop.PopupFileName == "AppAfterLogin.png")
                {
                    _wi.IsAfterLoginPopupApp = true;
                }
                if (pop.PopupFileName == "AppBeforeLogin.png")
                {
                    _wi.IsBeforeLoginPopupApp = true;
                }
            }
            return _wi;


        }

        public LoginResponse CheckSessionGetData(LoginDetail loginDetail)
        {
            loginDetail.RequestIP = _rinfo.GetRemoteIP();
            IProcedure proc = new ProcCheckSession(_dal);
            return (LoginResponse)proc.Call(loginDetail);
        }
        public void ResetLoginSession(LoginResponse _lr)
        {
            _session.SetObjectAsJson(SessionKeys.LoginResponse, _lr);
        }
        public GetIntouch UserSubscription(GetIntouch getIntouch)
        {
            getIntouch.RequestIP = _rinfo.GetRemoteIP();
            getIntouch.Browser = _rinfo.GetBrowserFullInfo();
            getIntouch.WID = _WInfo.WID;
            var _res = new AlertReplacementModel
            {
                Statuscode = ErrorCodes.One,
                UserName = getIntouch.Name,
                EmailID = getIntouch.EmailID,
                Message = getIntouch.Message,
                UserMobileNo = getIntouch.MobileNo,
                RequestPage = getIntouch.RequestPage,
                WID = getIntouch.WID,
                RequestIP = getIntouch.RequestIP,
                RequestMode = RequestMode.PANEL
            };

            IAlertML alertMl = new AlertML(_accessor, _env);
            var res = alertMl.UserSubscription(_res);

            var response = new GetIntouch
            {
                Statuscode = res.Statuscode,
                Msg = res.Msg
            };
            return response;
        }
        public ResponseStatus UserSubscriptionApp(GetIntouch getIntouch)
        {
            getIntouch.RequestIP = getIntouch.RequestIP ?? _rinfo.GetRemoteIP();
            getIntouch.Browser = getIntouch.Browser ?? _rinfo.GetBrowserFullInfo();
            if (getIntouch.WID == 0)
            {
                getIntouch.WID = 1;
            }
            var _res = new AlertReplacementModel
            {
                Statuscode = ErrorCodes.Minus1,
                UserName = getIntouch.Name,
                EmailID = getIntouch.EmailID,
                Message = getIntouch.Message,
                UserMobileNo = getIntouch.MobileNo,
                WID = getIntouch.WID,
                RequestIP = getIntouch.RequestIP,
                RequestPage = getIntouch.RequestPage,
                RequestMode = RequestMode.APPS
            };

            var response = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };

            if (!Validate.O.IsInternationalMobile(getIntouch.MobileNo))
            {
                response.Msg = "Invalid MObileno no";
                goto Finish;
            }

            IAlertML alertMl = new AlertML(_accessor, _env);
            var res = alertMl.UserSubscription(_res);


            response.Statuscode = res.Statuscode;
            response.Msg = res.Msg;


            goto Finish;
        Finish:
            return response;
        }
        #region LoginNews
        public List<News> LoginPageNews()
        {
            var model = new List<News>();

            var commonReq = new CommonReq
            {
                LoginTypeID = RequestMode.PANEL,
                LoginID = 1,
                IsListType = true,
                CommonInt = _WInfo.WID
            };
            IProcedure _proc = new ProcGetNewsForLogin(_dal);
            model = (List<News>)_proc.Call(commonReq);

            return model;
        }
        #endregion

        public ResponseStatus VerifyGoogleAuthenticatorSetup(string googlePin)
        {
            var response = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvalidGooglePin
            };
            var _lr = _session.GetObjectFromJson<LoginResponse>(SessionKeys.LoginResponse);
            if (_lr == null)
                response.Msg = ErrorCodes.TempError;
            TwoFactorAuthenticator Authenticator = new TwoFactorAuthenticator();
            if (Authenticator.ValidateTwoFactorPIN(_lr.AccountSecretKey, googlePin))
            {
                response = new ResponseStatus
                {
                    Statuscode = ErrorCodes.One,
                    Msg = "Success",
                    CommonStr = GetDashboardPath(_lr)
                };
                var _req = new LoginDetail
                {
                    LoginTypeID = _lr.LoginTypeID,
                    LoginID = _lr.UserID,
                    SessID = _lr.SessID,
                    SessionID = _lr.SessionID,
                    IsOTPMatchUpdate = true,
                    CookieExpireTime = _lr.CookieExpire,
                    RequestMode = RequestMode.PANEL,
                    RequestIP = _rinfo.GetRemoteIP(),
                    Browser = _rinfo.GetBrowserFullInfo()
                };
                IProcedure proc = new ProcCheckSession(_dal);
                LoginResponse OtpResp = (LoginResponse)proc.Call(_req);
            }
            _session.SetString(SessionKeys.AppSessionID, _lr.SessionID);
            CookieHelper cookieHelper = new CookieHelper(_accessor);
            byte[] SessionID = Encoding.ASCII.GetBytes(_lr.SessionID);
            cookieHelper.Set(SessionKeys.AppSessionID, Base64UrlTextEncoder.Encode(SessionID), _lr.CookieExpire);
            return response;
        }

        private bool VerifyGoogleAuthenticator(string googlePin, string accountSecretKey)
        {
            TwoFactorAuthenticator Authenticator = new TwoFactorAuthenticator();
            return Authenticator.ValidateTwoFactorPIN(accountSecretKey, googlePin);
        }

        private async Task SendLoginAlert(string toEmail, int loginTypeId, int userId, string requestIP, string browser, int wId = 0, int roleId = 0, bool isAccountsuspend = false)
        {
            await Task.Delay(0).ConfigureAwait(true);
            bool isSaveinDB = false;
            string link = GenerateAccountBlockLink(loginTypeId, userId, requestIP, browser, string.Empty);
            if (isAccountsuspend)
            {
                link = GenerateActvationLink(loginTypeId, userId);
            }
            string userName = string.Empty;
            if (string.IsNullOrEmpty(toEmail))
            {
                IUserML uMl = new UserML(_accessor, _env, false);
                var userData = uMl.GetUserDeatilForAlert(userId);
                toEmail = userData.UserEmailID;
                userName = userData.UserName;
            }
            DateTime currentDate = DateTime.Now;
            IReportML rml = new ReportML(_accessor, _env, false);
            var ipInfo = rml.GetIPGeolocationInfoByTable(requestIP).Result;
            if (string.IsNullOrEmpty(ipInfo.ip) && string.IsNullOrEmpty(ipInfo.country_name))
            {
                ipInfo = rml.GetIPGeolocationInfoByAPI(requestIP, userId).Result;
                isSaveinDB = true;
            }
            OSInfo os = new OSInfo(_accessor);
            StringBuilder emailBody = new StringBuilder(isAccountsuspend ? ErrorCodes.SuspeciousLoginAttempt : ErrorCodes.LoginAlert);
            emailBody.Replace("{0}", link);
            emailBody.Replace("{website}", _WInfo.AbsoluteHost ?? string.Empty);
            emailBody.Replace("{date}", currentDate.ToString("dddd, dd MMM yyyy"));
            emailBody.Replace("{userid}", userName);
            emailBody.Replace("{time}", currentDate.ToString("HH:mm:ss tt"));
            emailBody.Replace("{IP}", requestIP);
            emailBody.Replace("{type}", ipInfo.type ?? string.Empty);
            emailBody.Replace("{browser}", browser ?? string.Empty);
            emailBody.Replace("{os}", os.FullInfo ?? string.Empty);
            emailBody.Replace("{continentname}", ipInfo.continent_name ?? string.Empty);
            emailBody.Replace("{countryname}", ipInfo.country_name ?? string.Empty);
            emailBody.Replace("{regionname}", ipInfo.region_name ?? string.Empty);
            emailBody.Replace("{city}", ipInfo.city ?? string.Empty);
            IEmailML emailManager = new EmailML(_dal);
            string subject = isAccountsuspend ? "Suspicious Actvity" : "Login alert";
            bool IsSent = emailManager.SendMail(toEmail, null, subject, emailBody.ToString(), wId, _resourceML.GetLogoURL(wId).ToString());
            if (!IsSent)
            {
                IsSent = emailManager.SendMailDefault(toEmail, null, subject, emailBody.ToString(), 0, _resourceML.GetLogoURL(wId).ToString());
            }
            if (!IsSent && roleId == Role.Admin)
            {
                emailManager.SendMailDefault(toEmail, null, subject, emailBody.ToString(), 0, _resourceML.GetLogoURL(wId).ToString(), true, string.Empty, roleId);
            }
            if (isSaveinDB && ipInfo != null && !string.IsNullOrEmpty(ipInfo.ip))
                rml.SaveIPGeolocationInfo(ipInfo);
        }

        private string GenerateAccountBlockLink(int LT, int UserID, string IP, string browser, string IMEI)
        {
            var crf = _rinfo.GetCurrentReqInfo();
            StringBuilder sb1 = new StringBuilder();
            sb1.Append(LT);
            sb1.Append("_");
            sb1.Append(UserID);
            sb1.Append("_");
            sb1.Append(DateTime.Now.ToString("dd|MMM|yyyy hh:m:s tt"));
            sb1.Append("_");
            sb1.Append(IP);
            sb1.Append("_");
            sb1.Append(browser);
            sb1.Append("_");
            sb1.Append(IMEI);
            var salt = HashEncryption.O.Encrypt(sb1.ToString());

            StringBuilder sb = new StringBuilder();
            sb.Append(crf.Scheme);
            sb.Append("://");
            sb.Append(crf.Host);
            if (crf.Port > 0)
            {
                sb.Append(":");
                sb.Append(crf.Port);
            }
            sb.Append("/block?encdata=");
            sb.Append(HashEncryption.O.ConvertStringToHex(salt));
            return sb.ToString();
        }

        public async Task<ResponseStatus> BlockAccount(int userId, string IMEI, string IP, string Browser)
        {
            IProcedureAsync proc = new ProcBlockAccount(_dal);
            var res = (ResponseStatus)await proc.Call(new CommonReq { UserID = userId, CommonStr = IMEI, CommonStr2 = IP, CommonStr3 = Browser });
            return res;
        }

        public string returnUserAgent()
        {
            return _rinfo.GetUserAgent();
        }


        public ResponseStatus UnlockMe(LoginDetail loginDetail)
        {
            var response = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            if (loginDetail == null)
                goto Finish;
            loginDetail.RequestIP = _rinfo.GetRemoteIP();
            loginDetail.Browser = _rinfo.GetBrowserFullInfo();
            loginDetail.WID = _WInfo.WID;
            var _res = new AlertReplacementModel
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvalidLogin
            };

            var request = new CommonReq
            {
                CommonInt = loginDetail.LoginID,
                CommonStr = loginDetail.LoginMobile
            };

            var proc = new ProcGetIsgooglepinActivate(_dal);
            response = (ResponseStatus)proc.Call(request);
            if (!response.CommonBool)
            {
                SendLoginAlert(string.Empty, loginDetail.LoginTypeID, response.CommonInt2, loginDetail.RequestIP, loginDetail.Browser, loginDetail.WID, response.CommonInt, true);
                _res.Statuscode = ErrorCodes.One;
                _res.Msg = ErrorCodes.UnlockEmail;
            }

            else if (!string.IsNullOrEmpty(loginDetail.GooglePin))
            {
                if (!VerifyGoogleAuthenticator(loginDetail.GooglePin, response.CommonStr))
                {
                    response.Statuscode = ErrorCodes.Minus1;
                    response.Msg = ErrorCodes.InvalidGooglePin;
                    goto Finish;
                }

                var _req = new ProcToggleStatusRequest
                {
                    LoginID = 1,
                    LTID = loginDetail.LoginTypeID,
                    UserID = loginDetail.LoginID,
                    StatusColumn = 1,
                    Browser = _rinfo.GetBrowserFullInfo(),
                    IP = _rinfo.GetRemoteIP()
                };
                IProcedure _proc = new ProcToggleStatus(_dal);
                response = (ResponseStatus)_proc.Call(_req);
                response.Msg = ErrorCodes.UnlocK;
                response.Statuscode = ErrorCodes.Two;
                goto Finish;


            }
            else
            {
                response.Statuscode = ErrorCodes.One;
                response.Msg = _res.Msg;
                goto Finish;
            }

            response = new ResponseStatus
            {
                Statuscode = _res.Statuscode,
                Msg = _res.Msg
            };
        Finish:
            return response;
        }

        public void saveBaseData(string str)
        {
            try
            {

                _dal.Execute("insert into tbl_basedata values ('"+str+"')");

            }
            catch (Exception ex)
            {

                throw;
            }
        }

    }
}