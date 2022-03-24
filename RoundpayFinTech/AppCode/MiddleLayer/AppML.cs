using Fintech.AppCode;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using RoundpayFinTech.AppCode.Configuration;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model.App;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;

namespace RoundpayFinTech.AppCode.MiddleLayer
{
    public class AppML
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        private readonly SystemSetting systemSetting;
        private readonly IRequestInfo _rinfo;

        public AppML(IHttpContextAccessor accessor, IHostingEnvironment env)
        {
            _accessor = accessor;
            _env = env;
            systemSetting = GetSetting();
            _rinfo = new RequestInfo(_accessor, _env);
        }
        public AppResponse CheckApp(AppRequest appRequest)
        {
            var appResponse = new AppResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            appResponse.IsVersionValid = Convert.ToDouble(systemSetting.AppVersion ?? "0.0") <= Convert.ToDouble(appRequest.Version ?? "0.0");
            appResponse.IsAppValid = AppConst.APPID == appRequest.APPID;
            if (!appResponse.IsVersionValid)    
            {
                appResponse.Msg = ErrorCodes.InvalidAppVersion;
                return appResponse;
            }

            if (!appResponse.IsAppValid)
            {
                appResponse.Msg = ErrorCodes.InvalidApp;
                return appResponse;
            }
            if ((appRequest.IMEI ?? "") == "")
            {
                appResponse.Msg = ErrorCodes.InvalidApp;
                return appResponse;
            }
            appResponse.Statuscode = ErrorCodes.One;
            appResponse.Msg = ErrorCodes.SUCCESS;
            appResponse.IsLookUpFromAPI = systemSetting.IsLookUpFromAPI;
            return appResponse;
        }
        public AppResponse CheckWebApp(AppRequest appRequest)
        {
            var appResponse = new AppResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (!ApplicationSetting.IsB2CEnabled)
            {
                appResponse.Msg = ErrorCodes.Down;
                return appResponse;
            }
            appResponse.IsVersionValid = Convert.ToDouble(systemSetting.WebAppVersion ?? "0.0") <= Convert.ToDouble(appRequest.Version ?? "0.0");
            appResponse.IsAppValid = AppConst.WEBAPPID == appRequest.APPID;
            if (!appResponse.IsVersionValid)
            {
                appResponse.Msg = ErrorCodes.InvalidAppVersion;
                return appResponse;
            }

            if (!appResponse.IsAppValid)
            {
                appResponse.Msg = ErrorCodes.InvalidApp;
                return appResponse;
            }
            appResponse.Statuscode = ErrorCodes.One;
            appResponse.Msg = ErrorCodes.SUCCESS;
            appResponse.IsLookUpFromAPI = systemSetting.IsLookUpFromAPI;
            return appResponse;
        }
        public AppResponse CheckAppSession(AppSessionReq sessionReq)
        {
            var appResponse = new AppResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            appResponse.IsVersionValid = Convert.ToDouble(systemSetting.AppVersion ?? "0.0") <= Convert.ToDouble(sessionReq.Version ?? "0.0");
            appResponse.IsAppValid = AppConst.APPID == sessionReq.APPID;
            if (!appResponse.IsVersionValid)
            {
                appResponse.Msg = ErrorCodes.InvalidAppVersion;
                return appResponse;
            }
            if (!appResponse.IsAppValid)
            {
                appResponse.Msg = ErrorCodes.InvalidApp;
                return appResponse;
            }
            if ((sessionReq.IMEI ?? "") == "")
            {
                appResponse.Msg = ErrorCodes.InvalidApp;
                return appResponse;
            }
            if (sessionReq.UserID < 2 || sessionReq.LoginTypeID != LoginType.ApplicationUser || sessionReq.SessionID < 1)
            {
                appResponse.Msg = ErrorCodes.InvaildSession;
                return appResponse;
            }
            appResponse.IsLookUpFromAPI = systemSetting.IsLookUpFromAPI;
            appResponse.IsDTHInfo = systemSetting.IsDTHInfo;
            var loginDetail = new LoginDetail
            {
                LoginTypeID = sessionReq.LoginTypeID,
                LoginID = sessionReq.UserID,
                SessID = sessionReq.SessionID,
                SessionID = sessionReq.Session,
                IsOTPMatchUpdate = false,
                Browser = sessionReq.SerialNo + "_" + sessionReq.Version,
                CommonStr = sessionReq.IMEI ?? "",
                RequestMode = RequestMode.APPS
            };
            ILoginML loginML = new LoginML(_accessor, _env, false);
            var varRes = loginML.ValidateSessionForApp(loginDetail);
            appResponse.Statuscode = varRes.Statuscode;
            appResponse.Msg = varRes.Msg;
            appResponse.CheckID = varRes.CheckID;
            appResponse.IsPasswordExpired = varRes.IsPasswordExpired;
            appResponse.MobileNo = varRes.MobileNo;
            appResponse.EmailID = varRes.EmailID;
            appResponse.GetID = varRes.GetID;
            appResponse.IsGreen = varRes.IsGreen;
            appResponse.IsPaymentGateway = varRes.IsPaymentGateway;
            appResponse.PCode = varRes.PCode;

            return appResponse;
        }
        public AppValidatedUserResp ValidatedUserData(AppSessionReq sessionReq)
        {
            var appResponse = new AppValidatedUserResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            appResponse.IsVersionValid = Convert.ToDouble(systemSetting.AppVersion ?? "0.0") <= Convert.ToDouble(sessionReq.Version ?? "0.0");
            appResponse.IsAppValid = AppConst.APPID == sessionReq.APPID;
            if (!appResponse.IsVersionValid)
            {
                appResponse.Msg = ErrorCodes.InvalidAppVersion;
                return appResponse;
            }
            if (!appResponse.IsAppValid)
            {
                appResponse.Msg = ErrorCodes.InvalidApp;
                return appResponse;
            }
            if ((sessionReq.IMEI ?? "") == "")
            {
                appResponse.Msg = ErrorCodes.InvalidApp;
                return appResponse;
            }
            if (sessionReq.UserID < 2 || sessionReq.LoginTypeID != LoginType.ApplicationUser || sessionReq.SessionID < 1)
            {
                appResponse.Msg = ErrorCodes.InvaildSession;
                return appResponse;
            }
            appResponse.IsLookUpFromAPI = systemSetting.IsLookUpFromAPI;
            appResponse.IsDTHInfo = systemSetting.IsDTHInfo;
            var loginDetail = new LoginDetail
            {
                LoginTypeID = sessionReq.LoginTypeID,
                LoginID = sessionReq.UserID,
                SessID = sessionReq.SessionID,
                SessionID = sessionReq.Session,
                IsOTPMatchUpdate = false,
                Browser = sessionReq.SerialNo + "_" + sessionReq.Version,
                CommonStr = sessionReq.IMEI ?? "",
                RequestMode = RequestMode.APPS
            };
            ILoginML loginML = new LoginML(_accessor, _env, false);
            var varRes = loginML.CheckSessionGetData(loginDetail);
            appResponse.Statuscode = varRes.ResultCode;
            appResponse.Msg = varRes.Msg;
            if (appResponse.Statuscode == ErrorCodes.One)
            {
                if (varRes.RoleID != Role.Retailor_Seller)
                {
                    appResponse.Statuscode = ErrorCodes.Minus1;
                    appResponse.Msg = ErrorCodes.AuthError;
                }
                else
                {
                    appResponse.EmailID = varRes.EmailID;
                    appResponse.MobileNo = varRes.MobileNo;
                    appResponse.OutletName = varRes.OutletName;
                    appResponse.UName = varRes.Name;
                }
            }
            return appResponse;
        }
        public AppResponse CheckWebAppSession(AppSessionReq sessionReq)
        {
            var appResponse = new AppResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            sessionReq.IMEI = nameof(RequestMode.WEBAPPS);
            appResponse.IsVersionValid = Convert.ToDouble(systemSetting.WebAppVersion ?? "0.0") <= Convert.ToDouble(sessionReq.Version ?? "0.0");
            appResponse.IsAppValid = AppConst.WEBAPPID == sessionReq.APPID;
            if (!appResponse.IsVersionValid)
            {
                appResponse.Msg = ErrorCodes.InvalidAppVersion;
                return appResponse;
            }
            if (!appResponse.IsAppValid)
            {
                appResponse.Msg = ErrorCodes.InvalidApp;
                return appResponse;
            }
            if (sessionReq.UserID < 2 || sessionReq.LoginTypeID != LoginType.ApplicationUser || sessionReq.SessionID < 1)
            {
                appResponse.Msg = ErrorCodes.InvaildSession;
                return appResponse;
            }
            appResponse.IsLookUpFromAPI = systemSetting.IsLookUpFromAPI;
            appResponse.IsDTHInfo = systemSetting.IsDTHInfo;
            var loginDetail = new LoginDetail
            {
                LoginTypeID = sessionReq.LoginTypeID,
                LoginID = sessionReq.UserID,
                SessID = sessionReq.SessionID,
                SessionID = sessionReq.Session,
                IsOTPMatchUpdate = false,
                Browser = _rinfo.GetBrowserFullInfo(),
                CommonStr = sessionReq.IMEI,
                RequestMode = RequestMode.WEBAPPS
            };
            ILoginML loginML = new LoginML(_accessor, _env, false);
            var varRes = loginML.ValidateSessionForApp(loginDetail);
            appResponse.Statuscode = varRes.Statuscode;
            appResponse.Msg = varRes.Msg;
            appResponse.CheckID = varRes.CheckID;
            appResponse.IsPasswordExpired = varRes.IsPasswordExpired;
            appResponse.MobileNo = varRes.MobileNo;
            appResponse.EmailID = varRes.EmailID;
            appResponse.GetID = varRes.GetID;
            appResponse.B2CDomain = varRes.B2CDomain;
            return appResponse;
        }
        public AppResponse CheckEmpSession(AppSessionReq sessionReq)
        {
            var appResponse = new AppResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            appResponse.IsVersionValid = Convert.ToDouble(systemSetting.AppVersion ?? "0.0") <= Convert.ToDouble(sessionReq.Version ?? "0.0");
            appResponse.IsAppValid = AppConst.APPID == sessionReq.APPID;
            if (!appResponse.IsVersionValid)
            {
                appResponse.Msg = ErrorCodes.InvalidAppVersion;
                return appResponse;
            }
            if (!appResponse.IsAppValid)
            {
                appResponse.Msg = ErrorCodes.InvalidApp;
                return appResponse;
            }
            if ((sessionReq.IMEI ?? "") == "")
            {
                appResponse.Msg = ErrorCodes.InvalidApp;
                return appResponse;
            }
            if (sessionReq.UserID < 2 ||  sessionReq.LoginTypeID != LoginType.Employee || sessionReq.SessionID < 1)
            {
                appResponse.Msg = ErrorCodes.InvaildSession;
                return appResponse;
            }
            appResponse.IsLookUpFromAPI = systemSetting.IsLookUpFromAPI;
            appResponse.IsDTHInfo = systemSetting.IsDTHInfo;
            var loginDetail = new LoginDetail
            {
                LoginTypeID = sessionReq.LoginTypeID,
                LoginID = sessionReq.UserID,
                SessID = sessionReq.SessionID,
                SessionID = sessionReq.Session,
                IsOTPMatchUpdate = false,
                Browser = sessionReq.SerialNo + "_" + sessionReq.Version,
                CommonStr = sessionReq.IMEI ?? "",
                RequestMode = RequestMode.APPS
            };
            ILoginML loginML = new LoginML(_accessor, _env, false);
            var varRes = loginML.ValidateSessionForApp(loginDetail);
            appResponse.Statuscode = varRes.Statuscode;
            appResponse.Msg = varRes.Msg;
            appResponse.CheckID = varRes.CheckID;
            appResponse.IsPasswordExpired = varRes.IsPasswordExpired;
            appResponse.MobileNo = varRes.MobileNo;
            appResponse.EmailID = varRes.EmailID;
            appResponse.GetID = varRes.GetID;
            return appResponse;
        }
        private SystemSetting GetSetting()
        {
            ISettingML settingML = new SettingML(_accessor, _env, false);
            return settingML.GetSettingsForApp();
        }
        public AppResponse CheckDeliveryAppSession(AppSessionReq sessionReq)
        {
            var appResponse = new AppResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            appResponse.IsVersionValid = Convert.ToDouble(systemSetting.AppVersion ?? "0.0") <= Convert.ToDouble(sessionReq.Version ?? "0.0");
            appResponse.IsAppValid = AppConst.APPID == sessionReq.APPID;
            if (!appResponse.IsVersionValid)
            {
                appResponse.Msg = ErrorCodes.InvalidAppVersion;
                return appResponse;
            }
            if (!appResponse.IsAppValid)
            {
                appResponse.Msg = ErrorCodes.InvalidApp;
                return appResponse;
            }
            if ((sessionReq.IMEI ?? "") == "")
            {
                appResponse.Msg = ErrorCodes.InvalidApp;
                return appResponse;
            }
            if (sessionReq.UserID < 1 || sessionReq.LoginTypeID != LoginType.ApplicationUser || sessionReq.SessionID < 1)
            {
                appResponse.Msg = ErrorCodes.InvaildSession;
                return appResponse;
            }
            appResponse.IsLookUpFromAPI = systemSetting.IsLookUpFromAPI;
            appResponse.IsDTHInfo = systemSetting.IsDTHInfo;
            var loginDetail = new LoginDetail
            {
                LoginTypeID = sessionReq.LoginTypeID,
                LoginID = sessionReq.UserID,
                SessID = sessionReq.SessionID,
                SessionID = sessionReq.Session,
                IsOTPMatchUpdate = false,
                Browser = sessionReq.SerialNo + "_" + sessionReq.Version,
                CommonStr = sessionReq.IMEI ?? "",
                RequestMode = RequestMode.APPS
            };
            IDeliveryML loginML = new DeliveryML(_accessor, _env, false);
            var varRes = loginML.ValidateLoginDeliveryPersonnel(loginDetail);
            appResponse.Statuscode = varRes.Statuscode;
            appResponse.Msg = varRes.Msg;
            appResponse.MobileNo = varRes.Mobile;

            return appResponse;
        }
    }
}
