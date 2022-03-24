using Fintech.AppCode;
using Fintech.AppCode.Configuration;
using Fintech.AppCode.DB;
using Fintech.AppCode.HelperClass;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.Model.Reports;
using Fintech.AppCode.StaticModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using RoundpayFinTech.AppCode.Configuration;
using RoundpayFinTech.AppCode.DL;
using RoundpayFinTech.AppCode.HelperClass;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.MiddleLayer;
using RoundpayFinTech.AppCode.MiddleLayer.Dmt_Api;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.App;
using RoundpayFinTech.AppCode.Model.DepartmentModel;
using RoundpayFinTech.AppCode.Model.MoneyTransfer;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.StaticModel.MoneyTransfer;
using RoundpayFinTech.AppCode.ThirdParty.Paytm;
using RoundpayFinTech.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Validators;

namespace RoundpayFinTech.AppCode
{

    public class UserML : IUserML, IFundProcessML, IKYCML, IAdminML, IUserAPPML, IAPIUserML, IIPAddressML, IUserWebsite
    {
        private readonly IConfiguration Configuration;
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        private readonly ISession _session;
        private readonly IDAL _dal;
        private readonly IConnectionConfiguration _c;
        private readonly IRequestInfo _rinfo;
        private readonly WebsiteInfo _WInfo;
        private readonly LoginResponse _lr;
        private readonly IResourceML _resourceML;
        private readonly LoginResponse _lrEmp;

        #region DashboardRelated
        public Dashboard DashBoard()
        {
            IProcedure f = new ProcDashBoardDetail(_dal);
            return (Dashboard)f.Call();
        }
        #endregion
        public UserML(IHttpContextAccessor accessor, IHostingEnvironment env, bool IsInSession = true)
        {
            _accessor = accessor;
            _env = env;
            _c = new ConnectionConfiguration(_accessor, _env);
            _dal = new DAL(_c.GetConnectionString());
            _rinfo = new RequestInfo(_accessor, _env);
            if (IsInSession)
            {
                _session = _accessor.HttpContext.Session;
                bool IsProd = _env.IsProduction();
                var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory());
                builder.AddJsonFile((IsProd ? "appsettings.json" : "appsettings.Development.json"));
                builder.AddEnvironmentVariables();
                Configuration = builder.Build();
                _WInfo = new LoginML(_accessor, _env).GetWebsiteInfo();
                _lr = _session.GetObjectFromJson<LoginResponse>(SessionKeys.LoginResponse);
                _lrEmp = _session.GetObjectFromJson<LoginResponse>(SessionKeys.LoginResponseEmp);
            }
            _resourceML = new ResourceML(_accessor, _env);

        }
        public UserML(LoginResponse lr)
        {
            _lr = lr;
        }
        #region IAPIUserMLRegion
        public IResponseStatus SetGetToken()
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (_lr.RoleID == Role.APIUser && LoginType.ApplicationUser == _lr.LoginTypeID)
            {
                IProcedure proc = new ProcAPIUserTokenChange(_dal);
                res = (ResponseStatus)proc.Call(_lr.UserID);
            }
            return res;
        }
        public IEnumerable<UserCallBackModel> GetCallBackUrl()
        {
            var _res = new List<UserCallBackModel>();
            if (_lr.RoleID == Role.APIUser && _lr.LoginTypeID == LoginType.ApplicationUser)
            {
                var req = new UserCallBackModel
                {
                    UserID = _lr.UserID,
                    CallbackType = 0
                };
                IProcedure _proc = new ProcGetUserCallbackUrl(_dal);
                _res = (List<UserCallBackModel>)_proc.Call(req);
            }
            return _res;
        }
        public UserCallBackModel GetCallBackUrl(int Type)
        {
            var _res = new UserCallBackModel();
            if (_lr.RoleID == Role.APIUser && _lr.LoginTypeID == LoginType.ApplicationUser)
            {
                var req = new UserCallBackModel
                {
                    UserID = _lr.UserID,
                    CallbackType = Type,
                    CallBackTypeList = new List<CallbackTypeModel>()
                };
                IProcedure _proc = new ProcGetUserCallbackUrl(_dal);
                var lst = (List<UserCallBackModel>)_proc.Call(req);
                if (lst.Count > 0)
                {
                    _res = lst[0];
                }
                _proc = new ProcGetCallbackType(_dal);
                _res.CallBackTypeList = (List<CallbackTypeModel>)_proc.Call(_lr.UserID);
            }
            return _res;
        }
        public IResponseStatus SaveCallback(UserCallBackModel req)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (req.CallbackType == 3 && string.IsNullOrEmpty(req.UpdateUrl))
            {
                res.Msg = "Please Fill Update URL!";
                return res;
            }
            if (_lr.LoginTypeID == LoginType.ApplicationUser && _lr.RoleID == Role.APIUser)
            {
                req.UserID = _lr.UserID;
                IProcedure _proc = new ProcCallbackUrlCU(_dal);
                res = (ResponseStatus)_proc.Call(req);
            }
            return res;
        }
        #endregion

        #region IPAddress
        public IEnumerable<IPAddressModel> GetIPAddress(string MobileNo, int ID)
        {
            var res = new List<IPAddressModel>();
            if (_lr.LoginTypeID == LoginType.ApplicationUser && _lr.RoleID.In(Role.Admin, Role.APIUser))
            {
                var req = new CommonReq
                {
                    LoginTypeID = _lr.LoginTypeID,
                    LoginID = _lr.UserID,
                    CommonInt = ID,
                    CommonInt2 = _lr.RoleID == Role.Admin ? 0 : _lr.UserID,
                    CommonStr = MobileNo ?? string.Empty
                };
                if (_lr.RoleID == Role.APIUser)
                {
                    req.CommonStr = MobileNo ?? string.Empty;
                }
                else if (!Validate.O.IsMobile(MobileNo ?? string.Empty) && (MobileNo ?? string.Empty).Length > 0)
                {
                    req.CommonInt2 = Convert.ToInt32(MobileNo);
                    req.CommonStr = string.Empty;
                }
                IProcedure _proc = new ProcGetIPAddress(_dal);
                res = (List<IPAddressModel>)_proc.Call(req);
            }
            return res;
        }

        public IResponseStatus SaveIPAddress(IPAddressModel iPAddressModel)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            var chkOTP = _session.GetString(SessionKeys.CommonOTP);
            if (iPAddressModel.OTP != chkOTP)
            {
                res.Statuscode = ErrorCodes.Minus1;
                res.Msg = "Wrong OTP Entered";
                return res;
            }

            if (_lr.LoginTypeID == LoginType.ApplicationUser && _lr.RoleID.In(Role.Admin, Role.APIUser))
            {
                iPAddressModel.LT = _lr.LoginTypeID;
                iPAddressModel.LoginID = _lr.UserID;
                iPAddressModel.UserID = _lr.UserID;
                iPAddressModel.FromBrowser = _rinfo.GetBrowserFullInfo();
                iPAddressModel.FromIPAddress = _rinfo.GetRemoteIP();
                iPAddressModel.Source = "Panel";
                IProcedure _proc = new ProcIPAddressSave(_dal);
                res = (ResponseStatus)_proc.Call(iPAddressModel);
            }
            return res;
        }

        public IResponseStatus RemoveIp(int Id)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            var req = new CommonReq
            {
                CommonInt = _lr.LoginTypeID,
                CommonInt2 = _lr.UserID,
                CommonInt3 = Id
            };

            if (_lr.LoginTypeID == LoginType.ApplicationUser && _lr.RoleID.In(Role.Admin, Role.APIUser))
            {
                IProcedure _proc = new ProcRemoveIp(_dal);
                res = (ResponseStatus)_proc.Call(req);
            }
            return res;
        }
        public IResponseStatus UpdateIpStatus(int Id, bool sts)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            var req = new CommonReq
            {
                CommonInt = _lr.LoginTypeID,
                CommonInt2 = _lr.UserID,
                CommonInt3 = Id,
                CommonBool = sts,
                CommonStr = _rinfo.GetRemoteIP(),
                CommonStr2 = _rinfo.GetBrowserFullInfo()
            };

            if (_lr.LoginTypeID == LoginType.ApplicationUser && _lr.RoleID.In(Role.Admin, Role.APIUser))
            {
                IProcedure _proc = new ProcUpdateIpAddress(_dal);
                res = (ResponseStatus)_proc.Call(req);
            }
            return res;
        }
        public IResponseStatus SendIPOTP()
        {
            IResponseStatus res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            var alertData = GetUserDeatilForAlert(_lr.UserID);
            if (alertData.Statuscode == ErrorCodes.One)
            {
                IAlertML alertMl = new AlertML(_accessor, _env);
                alertData.OTP = HashEncryption.O.CreatePasswordNumeric(6);
                _session.SetString(SessionKeys.CommonOTP, alertData.OTP);
                alertMl.OTPSMS(alertData);
                res = alertMl.OTPEmail(alertData);
                res.Msg = "Enter OTP";
            }

            return res;
        }
        #endregion

        #region MyRegion
        public ResponseStatus CallCreateUser(UserCreate _req)
        {
            var _resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            if (ApplicationSetting.WithCustomLoginID)
            {
                IProcedure _pro = new ProcCheckCustomLogin(_dal);
                var request = new UserDuplicate
                {
                    CustomLoginID = _req.CustomLoginID,
                    LoginID = _req.UserID
                };
                var response = (ResponseStatus)_pro.Call(request);
                if (response.Statuscode == ErrorCodes.Minus1)
                {
                    response.Msg = ErrorCodes.DCLOEx;
                    return response;
                }
            }
            if (_lr.LoginTypeID == LoginType.ApplicationUser || IsCustomerCareAuthorised(ActionCodes.CreateUser))
            {
                string Token = HashEncryption.O.Decrypt(_req.Token ?? "");
                if (!Validate.O.IsNumeric(Token))
                {
                    _resp.Msg = ErrorCodes.InvalidParam + " Referal";
                    return _resp;
                }
                if (_req.RoleID < 2)
                {
                    _resp.Msg = ErrorCodes.InvalidParam + " Role";
                    return _resp;
                }
                if (_req.SlabID < 1 && !_lr.IsAdminDefined)
                {
                    _resp.Msg = ErrorCodes.InvalidParam + " Slab";
                    return _resp;
                }
                if (Validate.O.IsNumeric(_req.Name ?? "") || (_req.Name ?? "").Length > 100)
                {
                    _resp.Msg = ErrorCodes.InvalidParam + " Name";
                    return _resp;
                }
                if (Validate.O.IsNumeric(_req.OutletName ?? "") || (_req.OutletName ?? "").Length > 100)
                {
                    _resp.Msg = ErrorCodes.InvalidParam + " Outlet Name";
                    return _resp;
                }
                if (!Validate.O.IsMobile(_req.MobileNo ?? ""))
                {
                    _resp.Msg = ErrorCodes.InvalidParam + " Mobile Number";
                    return _resp;
                }
                if (string.IsNullOrEmpty(_req.EmailID) || !_req.EmailID.Contains("@") || !_req.EmailID.Contains("."))
                {
                    _resp.Msg = ErrorCodes.InvalidParam + " EmailID";
                    return _resp;
                }
                if (!Validate.O.IsPinCode(_req.Pincode ?? ""))
                {
                    _resp.Msg = ErrorCodes.InvalidParam + " Pincode";
                    return _resp;
                }
                if (_req.IsWebsite && ApplicationSetting.IsWhitelabel)
                {
                    if ((_req.WebsiteName ?? "").IndexOf('.') < 0 || (_req.WebsiteName ?? "").Trim() == "")
                    {
                        _resp.Msg = ErrorCodes.InvalidParam + " WebsiteName";
                        return _resp;
                    }
                    _req.WebsiteName = _req.WebsiteName.Replace("https://", "").Replace("http://", "").Replace("www.", "").Replace(" ", "").ToLower();
                }
                else
                {
                    _req.WebsiteName = string.Empty;
                }

                _req.ReferalID = Convert.ToInt32(Token);
                _req.LoginID = _lr.UserID;
                _req.LTID = _lr.LoginTypeID;
                _req.Browser = _rinfo.GetBrowserFullInfo();
                _req.IP = _rinfo.GetRemoteIP();
                _req.Password = ApplicationSetting.IsPasswordNumeric ? HashEncryption.O.CreatePasswordNumeric(8) : HashEncryption.O.CreatePassword(8);
                _req.Pin = HashEncryption.O.CreatePasswordNumeric(4);
                IProcedure _p = new ProcUserCreate(_dal);
                var res = (AlertReplacementModel)_p.Call(_req);
                _resp.Msg = res.Msg;
                if (res.Statuscode == ErrorCodes.One)
                {
                    _resp.Statuscode = res.Statuscode;

                    if (_req.IsWebsite && res.WID > 0)
                    {
                        _resourceML.CreateWebsiteDirectory(res.WID, FolderType.Website);
                    }
                    IAlertML alertMl = new AlertML(_accessor, _env);
                    Parallel.Invoke(() => alertMl.RegistrationSMS(res),
                    () => alertMl.RegistrationEmail(res),
                    () => alertMl.SocialAlert(res));
                }
            }
            return _resp;
        }
        public ResponseStatus CallSignup(UserCreate _req)
        {
            var _resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            if (_WInfo.WID > 0)
            {
                if (_req.RoleID < 2)
                {
                    _resp.Msg = ErrorCodes.InvalidParam + " Role";
                    return _resp;
                }

                if (Validate.O.IsNumeric(_req.Name ?? "") || (_req.Name ?? "").Length > 100)
                {
                    _resp.Msg = ErrorCodes.InvalidParam + " Name";
                    return _resp;
                }
                if (Validate.O.IsNumeric(_req.OutletName ?? "") || (_req.OutletName ?? "").Length > 100)
                {
                    _resp.Msg = ErrorCodes.InvalidParam + " Outlet Name";
                    return _resp;
                }
                if (!Validate.O.IsMobile(_req.MobileNo ?? ""))
                {
                    _resp.Msg = ErrorCodes.InvalidParam + " Mobile Number";
                    return _resp;
                }
                if (string.IsNullOrEmpty(_req.EmailID) || !_req.EmailID.Contains("@") || !_req.EmailID.Contains("."))
                {
                    _resp.Msg = ErrorCodes.InvalidParam + " EmailID";
                    return _resp;
                }
                if (!Validate.O.IsPinCode(_req.Pincode ?? ""))
                {
                    _resp.Msg = ErrorCodes.InvalidParam + " Pincode";
                    return _resp;
                }
                _req.RequestModeID = RequestMode.PANEL;
                _req.WID = _WInfo.WID;
                _req.Browser = _rinfo.GetBrowserFullInfo();
                _req.IP = _rinfo.GetRemoteIP();
                _req.Password = ApplicationSetting.IsPasswordNumeric ? HashEncryption.O.CreatePasswordNumeric(8) : HashEncryption.O.CreatePassword(8);
                _req.Pin = HashEncryption.O.CreatePasswordNumeric(4);
                IProcedure _p = new ProcSignup(_dal);
                var res = (AlertReplacementModel)_p.Call(_req);
                _resp.Msg = res.Msg;
                if (res.Statuscode == ErrorCodes.One)
                {
                    _resp.Statuscode = res.Statuscode;

                    if (_req.IsWebsite && res.WID > 0)
                    {
                        _resourceML.CreateWebsiteDirectory(res.WID, FolderType.Website);
                    }
                    IAlertML alertMl = new AlertML(_accessor, _env);
                    Parallel.Invoke(() => alertMl.RegistrationSMS(res),
                   () => alertMl.RegistrationEmail(res),
                   () => alertMl.SocialAlert(res));
                }
            }
            return _resp;
        }

        public CompanyProfileDetail GetCompanyProfile(int WID)
        {
            IProcedure proc = new ProcCompanyProfile(_dal);
            return (CompanyProfileDetail)proc.Call(WID);
        }
        public LowBalanceSetting GetSetting(int UserID = 0)
        {
            var userID = _lr != null ? _lr.UserID : UserID;
            IProcedure proc = new ProcGetUserLowBalanceSetting(_dal);
            return (LowBalanceSetting)proc.Call(userID);
        }
        public IResponseStatus CallCreateUserApp(UserCreate _req)
        {
            var _resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            if (ApplicationSetting.WithCustomLoginID)
            {
                IProcedure _pro = new ProcCheckCustomLogin(_dal);
                var request = new UserDuplicate
                {
                    CustomLoginID = _req.CustomLoginID,
                    LoginID = _req.UserID
                };
                var response = (ResponseStatus)_pro.Call(request);
                if (response.Statuscode == ErrorCodes.Minus1)
                {
                    response.Msg = ErrorCodes.DCLOEx;
                    return response;
                }
            }
            string Token = HashEncryption.O.Decrypt(_req.Token ?? "");
            if (!Validate.O.IsNumeric(Token))
            {
                _resp.Msg = ErrorCodes.InvalidParam + " Referal";
                return _resp;
            }
            if (_req.RoleID < 2)
            {
                _resp.Msg = ErrorCodes.InvalidParam + " Role";
                return _resp;
            }
            if (_req.SlabID < 1 && !_req.IsAdminDefined)
            {
                _resp.Msg = ErrorCodes.InvalidParam + " Slab";
                return _resp;
            }
            if (Validate.O.IsNumeric(_req.Name ?? "") || (_req.Name ?? "").Length > 100)
            {
                _resp.Msg = ErrorCodes.InvalidParam + " Name";
                return _resp;
            }
            if (Validate.O.IsNumeric(_req.OutletName ?? "") || (_req.OutletName ?? "").Length > 100)
            {
                _resp.Msg = ErrorCodes.InvalidParam + " Outlet Name";
                return _resp;
            }
            if (!Validate.O.IsMobile(_req.MobileNo ?? ""))
            {
                _resp.Msg = ErrorCodes.InvalidParam + " Mobile Number";
                return _resp;
            }
            if (string.IsNullOrEmpty(_req.EmailID) || !_req.EmailID.Contains("@") || !_req.EmailID.Contains("."))
            {
                _resp.Msg = ErrorCodes.InvalidParam + " EmailID";
                return _resp;
            }
            if (!Validate.O.IsPinCode(_req.Pincode ?? ""))
            {
                _resp.Msg = ErrorCodes.InvalidParam + " Pincode";
                return _resp;
            }
            if (_req.IsWebsite)
            {
                if ((_req.WebsiteName ?? "").IndexOf('.') < 0 || (_req.WebsiteName ?? "").Trim() == "")
                {
                    _resp.Msg = ErrorCodes.InvalidParam + " WebsiteName";
                    return _resp;
                }
                _req.WebsiteName = _req.WebsiteName.Replace("https://", "").Replace("http://", "").Replace("www.", "").Replace(" ", "").ToLower();
            }
            _req.ReferalID = Convert.ToInt32(Token);
            _req.Browser = _rinfo.GetBrowserFullInfo();
            _req.IP = _rinfo.GetRemoteIP();
            _req.Password = ApplicationSetting.IsPasswordNumeric ? HashEncryption.O.CreatePasswordNumeric(8) : HashEncryption.O.CreatePassword(8);
            _req.Pin = HashEncryption.O.CreatePasswordNumeric(4);
            IProcedure _p = new ProcUserCreate(_dal);
            var res = (AlertReplacementModel)_p.Call(_req);
            _resp.Msg = res.Msg;
            if (res.Statuscode == ErrorCodes.One)
            {
                _resp.Statuscode = res.Statuscode;
                IAlertML alertMl = new AlertML(_accessor, _env);
                Parallel.Invoke(() => alertMl.RegistrationSMS(res),
                   () => alertMl.RegistrationEmail(res),
                   () => alertMl.SocialAlert(res));
            }
            return _resp;
        }

        public IResponseStatus CallSignupFromApp(UserCreate _req)
        {
            var _resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            if (_req.RoleID < 2)
            {
                _resp.Msg = ErrorCodes.InvalidParam + " Role";
                return _resp;
            }
            if (Validate.O.IsNumeric(_req.Name ?? "") || (_req.Name ?? "").Length > 100)
            {
                _resp.Msg = ErrorCodes.InvalidParam + " Name";
                return _resp;
            }
            if (Validate.O.IsNumeric(_req.OutletName ?? "") || (_req.OutletName ?? "").Length > 100)
            {
                _resp.Msg = ErrorCodes.InvalidParam + " Outlet Name";
                return _resp;
            }
            if (!Validate.O.IsMobile(_req.MobileNo ?? ""))
            {
                _resp.Msg = ErrorCodes.InvalidParam + " Mobile Number";
                return _resp;
            }
            if (string.IsNullOrEmpty(_req.EmailID) || !_req.EmailID.Contains("@") || !_req.EmailID.Contains("."))
            {
                _resp.Msg = ErrorCodes.InvalidParam + " EmailID";
                return _resp;
            }
            if (!Validate.O.IsPinCode(_req.Pincode ?? ""))
            {
                _resp.Msg = ErrorCodes.InvalidParam + " Pincode";
                return _resp;
            }
            if (_req.RequestModeID == RequestMode.API)
            {
                if (!Validate.O.IsMobile(_req.ReferralNo))
                {
                    _resp.Msg = string.Format("{0} ReferralNo", ErrorCodes.InvalidParam);
                    return _resp;
                }
                if ((_req.OtherUserID ?? string.Empty).Length > 10)
                {
                    _resp.Msg = string.Format("{0} OtherUserID lenght must not be greater than 10", ErrorCodes.InvalidParam);
                    return _resp;
                }
            }
            _req.Browser = _rinfo.GetBrowserFullInfo();
            _req.IP = _rinfo.GetRemoteIP();
            _req.Password = ApplicationSetting.IsPasswordNumeric ? HashEncryption.O.CreatePasswordNumeric(8) : HashEncryption.O.CreatePassword(8);
            _req.Pin = HashEncryption.O.CreatePasswordNumeric(4);
            IProcedure _p = new ProcSignup(_dal);
            var res = (AlertReplacementModel)_p.Call(_req);
            _resp.Msg = res.Msg;
            if (res.Statuscode == ErrorCodes.One)
            {
                _resp.Statuscode = res.Statuscode;
                IAlertML alertMl = new AlertML(_accessor, _env);
                Parallel.Invoke(() => alertMl.RegistrationSMS(res),
                   () => alertMl.RegistrationEmail(res),
                   () => alertMl.SocialAlert(res));
            }
            return _resp;
        }
        public async Task<IResponseStatus> CallSignupWebApp(UserCreate _req)
        {
            var _resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            if (Validate.O.IsNumeric(_req.Name ?? string.Empty) || (_req.Name ?? string.Empty).Length > 100)
            {
                _resp.Msg = ErrorCodes.InvalidParam + " Name";
                return _resp;
            }
            if (!Validate.O.IsMobile(_req.MobileNo ?? string.Empty))
            {
                _resp.Msg = ErrorCodes.InvalidParam + " Mobile Number";
                return _resp;
            }
            if (string.IsNullOrEmpty(_req.EmailID) || !_req.EmailID.Contains("@") || !_req.EmailID.Contains("."))
            {
                _resp.Msg = ErrorCodes.InvalidParam + " EmailID";
                return _resp;
            }
            if (!Validate.O.IsPinCode(_req.Pincode ?? string.Empty))
            {
                _resp.Msg = ErrorCodes.InvalidParam + " Pincode";
                return _resp;
            }
            if ((_req.Password ?? string.Empty).Length < 8 || (_req.Password ?? string.Empty).Length > 12)
            {
                _resp.Msg = ErrorCodes.InvalidParam + " Password";
                return _resp;
            }
            _req.Browser = _rinfo.GetBrowserFullInfo();
            _req.IP = _rinfo.GetRemoteIP();
            IProcedureAsync _p = new ProcSignupCustomer(_dal);
            var res = (AlertReplacementModel)await _p.Call(_req).ConfigureAwait(false);
            _resp.Msg = res.Msg;
            if (res.Statuscode == ErrorCodes.One)
            {
                _resp.Statuscode = res.Statuscode;
                IAlertML alertMl = new AlertML(_accessor, _env);
                alertMl.RegistrationSMS(res);
                alertMl.RegistrationEmail(res);
            }
            return _resp;
        }
        public MiddleLayerUser MiddleDashBoard()
        {
            if (_lr.LoginTypeID == LoginType.ApplicationUser && !IsEndUser() && _lr.RoleID != Role.Admin)
            {
                IProcedure f = new ProcUserMiddleLayer(_dal);
                return (MiddleLayerUser)f.Call(_lr.UserID);
            }
            return new MiddleLayerUser();
        }
        public UserDetail GetUserDetailByID(string MobileNo)
        {
            var resp = new UserDetail
            {
                ResultCode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvalidParam
            };
            if (string.IsNullOrEmpty(MobileNo) || (MobileNo ?? "").Length > 10)
                return resp;

            var req = new UserRequset
            {
                LoginID = _lr.UserID,
                LoginTypeID = _lr.LoginTypeID
            };
            bool InSession = false;
            if (Validate.O.IsMobile(MobileNo ?? ""))
            {
                req.MobileNo = MobileNo;
                InSession = _lr.LoginTypeID == LoginType.ApplicationUser && _lr.MobileNo == req.MobileNo;
            }
            else
            {
                req.UserId = Convert.ToInt32(MobileNo);
                InSession = _lr.LoginTypeID == LoginType.ApplicationUser && _lr.UserID == req.UserId;
            }
            if (!InSession)
            {
                IProcedure proc = new ProcGetUserByID(_dal);
                resp = (UserDetail)proc.Call(req);
            }
            else
            {
                resp.ResultCode = ErrorCodes.One;
                resp.Msg = ErrorCodes.SUCCESS;
                resp.UserID = _lr.UserID;
                resp.RoleID = _lr.RoleID;
                resp.SlabID = _lr.SlabID;
                resp.Name = _lr.Name;
                resp.OutletName = _lr.OutletName;
                resp.ReferalID = _lr.ReferalID;
                resp.EmailID = _lr.EmailID;
                resp.MobileNo = _lr.MobileNo;
                resp.IsGSTApplicable = _lr.IsGSTApplicable;
                resp.IsTDSApplicable = _lr.IsTDSApplicable;
                resp.IsVirtual = _lr.IsVirtual;
                resp.IsWebsite = _lr.IsWebsite;
                resp.IsAdminDefined = _lr.IsAdminDefined;
            }
            return resp;
        }
        public UserDetail GetAppUserDetailByID(UserRequset req)
        {
            string MobileNo = req.MobileNo ?? "0";
            var resp = new UserDetail
            {
                ResultCode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvalidParam
            };
            if (req.UserId == 0 && (string.IsNullOrEmpty(req.MobileNo) || !Validate.O.IsNumeric(req.MobileNo ?? "") || (req.MobileNo ?? "").Length > 10))
                return resp;

            if (!Validate.O.IsMobile(req.MobileNo ?? "") && (req.MobileNo ?? "").Length > 0)
            {
                req.UserId = Convert.ToInt32(MobileNo);
            }
            IProcedure proc = new ProcGetUserByID(_dal);
            resp = (UserDetail)proc.Call(req);

            return resp;
        }
        public UserRegModel GetReffDeatil(string MobileNo)
        {
            var _res = new UserRegModel
            {
                userInfo = GetUserDetailByID(MobileNo),
                Input = MobileNo
            };

            _res.IsError = _res.userInfo.UserID == 0;
            if (!_res.IsError)
                _res.Token = HashEncryption.O.Encrypt(_res.userInfo.UserID + "");

            IProcedure proc = new ProcUserSelectRoleSlab(_dal);
            _res.roleSlab = (UserRoleSlab)proc.Call(_res.userInfo.UserID);
            return _res;
        }
        public UserRegModel GetAppUserReffDeatil(UserRequset req)
        {
            var _res = new UserRegModel
            {
                userInfo = GetAppUserDetailByID(req),
                Input = req.MobileNo
            };

            _res.IsError = _res.userInfo.UserID == 0;
            if (!_res.IsError)
                _res.Token = HashEncryption.O.Encrypt(_res.userInfo.UserID + "");

            IProcedure proc = new ProcUserSelectRoleSlab(_dal);
            _res.roleSlab = (UserRoleSlab)proc.Call(_res.userInfo.UserID);
            return _res;
        }
        public UserRegModel GetReffDeatilFromBulk(string MobileNo)
        {
            var _res = new UserRegModel
            {
                userInfo = GetUserDetailByID(MobileNo),
                Input = MobileNo,
                roleSlab = new UserRoleSlab()
            };

            _res.IsError = _res.userInfo.UserID == 0;
            if (!_res.IsError)
                _res.Token = HashEncryption.O.Encrypt(_res.userInfo.UserID + "");
            IProcedure proc = new SelectRole(_dal);
            _res.roleSlab.Roles = (List<RoleMaster>)proc.Call(_res.userInfo.UserID);
            return _res;
        }
        public IEnumerable<RoleMaster> GetUserChildRolesApp(int UserID)
        {
            IProcedure proc = new SelectRole(_dal);
            return (List<RoleMaster>)proc.Call(UserID);
        }
        public GetChangeSlab fn_GetChangeSlab(int UserID)
        {
            CommonReq req = new CommonReq
            {
                LoginID = _lr.UserID,
                LoginTypeID = _lr.LoginTypeID,
                CommonInt = UserID,

            };
            IProcedure proc = new ProcGetSlabChange(_dal);
            return (GetChangeSlab)proc.Call(req);
        }
        public ResponseStatus UpdateChangeSlab(int UserID, int SlabID, string pinPassword)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if ((_lr.LoginTypeID == LoginType.ApplicationUser && !IsEndUser()) || IsCustomerCareAuthorised(ActionCodes.AddEditSLAB))
            {
                var req = new CommonReq
                {
                    LoginID = _lr.UserID,
                    LoginTypeID = _lr.LoginTypeID,
                    CommonInt = UserID,
                    CommonInt2 = SlabID,
                    CommonStr = _rinfo.GetBrowser(),
                    CommonStr2 = _rinfo.GetRemoteIP(),
                    CommonStr3 = HashEncryption.O.Encrypt(pinPassword ?? string.Empty)
                };
                IProcedure proc = new ProcUpdateSlabChange(_dal);
                res = (ResponseStatus)proc.Call(req);
            }
            return res;
        }

        public IResponseStatus UpdateChangeRole(int UserID, int RoleID, int LoginId, int LoginTypeId)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if ((LoginTypeId == LoginType.ApplicationUser && !IsEndUser()) || IsCustomerCareAuthorised(ActionCodes.AddEditSLAB))
            {
                var req = new CommonReq
                {
                    LoginID = LoginId,
                    LoginTypeID = LoginTypeId,
                    CommonInt = UserID,
                    CommonInt2 = RoleID,
                    CommonStr = _rinfo.GetBrowser(),
                    CommonStr2 = _rinfo.GetRemoteIP()
                };
                IProcedure proc = new ProcUpdateRoleChange(_dal);
                res = (ResponseStatus)proc.Call(req);
            }
            return res;
        }
        public bool IsCustomerCareAuthorised(string OperationCode)
        {
            var loginResp = chkAlternateSession();
            if (loginResp != null)
            {
                if (LoginType.CustomerCare == loginResp.LoginTypeID)
                {
                    var OperationsAssigned = loginResp.operationsAssigned ?? new List<OperationAssigned>();
                    if (OperationsAssigned.Any())
                    {
                        if (OperationsAssigned.Any(x => x.OperationCode == OperationCode && x.IsActive))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        public bool IsEndUser()
        {
            var loginRes = chkAlternateSession();
            return (loginRes.RoleID.In(Role.APIUser, Role.Retailor_Seller, Role.Customer) && loginRes.LoginTypeID == LoginType.ApplicationUser) || loginRes.LoginTypeID == LoginType.Employee;
        }
        public IResponseStatus ChangeOTPStatus(int UserID, int type, bool Is)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            if ((_lr.LoginTypeID == LoginType.ApplicationUser) || IsCustomerCareAuthorised(ActionCodes.ChangeOTPStatus) || IsCustomerCareAuthorised(ActionCodes.ChangeUserStatus))
            {
                var _req = new ProcToggleStatusRequest
                {
                    LoginID = _lr.UserID,
                    LTID = _lr.LoginTypeID,
                    UserID = UserID,
                    StatusColumn = type,
                    Browser = _rinfo.GetBrowserFullInfo(),
                    IP = _rinfo.GetRemoteIP(),
                    Is = Is
                };
                IProcedure _proc = new ProcToggleStatus(_dal);
                _res = (ResponseStatus)_proc.Call(_req);
            }
            return _res;
        }
        public IResponseStatus ChangeVirtualStatus(int UserID)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            if (_lr.LoginTypeID == LoginType.ApplicationUser && _lr.UserID == 1)
            {
                var _req = new CommonReq
                {
                    LoginID = _lr.UserID,
                    LoginTypeID = _lr.LoginTypeID,
                    CommonInt = UserID
                };
                IProcedure _proc = new ProcUpdateVirtualStatus(_dal);
                _res = (ResponseStatus)_proc.Call(_req);
            }
            return _res;
        }
        public IResponseStatus ChangeFlatCommStatus(int UserID)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            if (_lr.LoginTypeID == LoginType.ApplicationUser && _lr.UserID == 1)
            {
                var _req = new CommonReq
                {
                    LoginID = _lr.UserID,
                    LoginTypeID = _lr.LoginTypeID,
                    CommonInt = UserID
                };
                IProcedure _proc = new ProcUpdateFlatCommission(_dal);
                _res = (ResponseStatus)_proc.Call(_req);
            }
            return _res;
        }
        public IResponseStatus ChangeUserStatusFromApp(ProcToggleStatusRequest _req)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (_req.LTID == LoginType.ApplicationUser && _req.LoginID != _req.UserID)
            {
                _req.StatusColumn = 1;
                _req.Browser = _rinfo.GetBrowserFullInfo();
                _req.IP = _rinfo.GetRemoteIP();
                IProcedure _proc = new ProcToggleStatus(_dal);
                _res = (ResponseStatus)_proc.Call(_req);
            }
            return _res;
        }
        public IResponseStatus ChangeDoubleFactorFromApp(ProcToggleStatusRequest _req)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (_req.LTID == LoginType.ApplicationUser && _req.LoginID == _req.UserID)
            {
                _req.StatusColumn = 3;
                _req.Browser = _rinfo.GetBrowserFullInfo();
                _req.IP = _rinfo.GetRemoteIP();
                IProcedure _proc = new ProcToggleStatus(_dal);
                _res = (ResponseStatus)_proc.Call(_req);
            }
            return _res;
        }
        //public IResponseStatus UpdateUser(GetEditUser _req)
        //{
        //    var _resp = new ResponseStatus
        //    {
        //        Statuscode = ErrorCodes.Minus1,
        //        Msg = ErrorCodes.AuthError
        //    };
        //    if (_lr.LoginTypeID == LoginType.ApplicationUser || IsCustomerCareAuthorised(ActionCodes.EditUser))
        //    {
        //        if (Validate.O.IsNumeric(_req.Name ?? "") || (_req.Name ?? "").Length > 100)
        //        {
        //            _resp.Msg = ErrorCodes.InvalidParam + " Name";
        //            return _resp;
        //        }
        //        if (Validate.O.IsNumeric(_req.OutletName ?? "") || (_req.OutletName ?? "").Length > 100)
        //        {
        //            _resp.Msg = ErrorCodes.InvalidParam + " Outlet Name";
        //            return _resp;
        //        }
        //        if (!Validate.O.IsMobile(_req.MobileNo ?? ""))
        //        {
        //            _resp.Msg = ErrorCodes.InvalidParam + " Mobile Number";
        //            return _resp;
        //        }
        //        if (!(_req.EmailID ?? "").Contains("@"))
        //        {
        //            _resp.Msg = ErrorCodes.InvalidParam + " EmailID";
        //            return _resp;
        //        }
        //        if (!Validate.O.IsPinCode(_req.Pincode ?? ""))
        //        {
        //            _resp.Msg = ErrorCodes.InvalidParam + " Pincode";
        //            return _resp;
        //        }
        //        if ((_req.Address ?? "").Length > 300)
        //        {
        //            _resp.Msg = ErrorCodes.InvalidParam + " Address";
        //            return _resp;
        //        }
        //        if ((_req.City ?? "").Length > 50)
        //        {
        //            _resp.Msg = ErrorCodes.InvalidParam + " City";
        //            return _resp;
        //        }
        //        if (!(_lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser))
        //        {
        //            if (!Validate.O.IsPAN(_req.PAN ?? ""))
        //            {
        //                _resp.Msg = ErrorCodes.InvalidParam + " PAN";
        //                return _resp;
        //            }
        //            if (!Validate.O.IsAADHAR(_req.AADHAR ?? ""))
        //            {
        //                _resp.Msg = ErrorCodes.InvalidParam + " AADHAR";
        //                return _resp;
        //            }
        //        }
        //        if (_req.IsWebsite && !(_req.WebsiteName ?? "").Contains("."))
        //        {
        //            _resp.Msg = ErrorCodes.InvalidParam + "Domain Name";
        //            return _resp;
        //        }
        //        _req.LoginID = _lr.UserID;
        //        _req.LT = _lr.LoginTypeID;
        //        _req.IP = _rinfo.GetRemoteIP();
        //        _req.Browser = _rinfo.GetBrowser();
        //        IProcedure proc = new ProcUpdateUser(_dal);
        //        _resp = (ResponseStatus)proc.Call(_req);
        //        if (_resp.Statuscode == ErrorCodes.One)
        //        {
        //            if (_req.IsWebsite && _resp.CommonInt > 0)
        //            {
        //                _resourceML.CreateWebsiteDirectory(_resp.CommonInt, FolderType.Website);
        //            }
        //        }
        //        _req.WID = _resp.CommonInt;
        //    }
        //    return _resp;
        //}
        public IResponseStatus UpdateUserFromApp(GetEditUser _req)
        {
            var _resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (_req.LT == LoginType.ApplicationUser)
            {
                if (Validate.O.IsNumeric(_req.Name ?? "") || (_req.Name ?? "").Length > 100)
                {
                    _resp.Msg = ErrorCodes.InvalidParam + " Name";
                    return _resp;
                }
                if (Validate.O.IsNumeric(_req.OutletName ?? "") || (_req.OutletName ?? "").Length > 100)
                {
                    _resp.Msg = ErrorCodes.InvalidParam + " Outlet Name";
                    return _resp;
                }
                if (!Validate.O.IsMobile(_req.MobileNo ?? ""))
                {
                    _resp.Msg = ErrorCodes.InvalidParam + " Mobile Number";
                    return _resp;
                }
                if (!(_req.EmailID ?? "").Contains("@"))
                {
                    _resp.Msg = ErrorCodes.InvalidParam + " EmailID";
                    return _resp;
                }
                if (!Validate.O.IsPinCode(_req.Pincode ?? ""))
                {
                    _resp.Msg = ErrorCodes.InvalidParam + " Pincode";
                    return _resp;
                }
                if ((_req.Address ?? "").Length > 300)
                {
                    _resp.Msg = ErrorCodes.InvalidParam + " Address";
                    return _resp;
                }
                if ((_req.City ?? "").Length > 50)
                {
                    _resp.Msg = ErrorCodes.InvalidParam + " City";
                    return _resp;
                }
                if (!Validate.O.IsPAN(_req.PAN ?? ""))
                {
                    _resp.Msg = ErrorCodes.InvalidParam + " PAN";
                    return _resp;
                }
                if (!Validate.O.IsAADHAR(_req.AADHAR ?? ""))
                {
                    _resp.Msg = ErrorCodes.InvalidParam + " AADHAR";
                    return _resp;
                }
                _req.IP = _rinfo.GetRemoteIP();
                _req.Browser = _rinfo.GetBrowser();
                IProcedure proc = new ProcUpdateUser(_dal);
                _resp = (ResponseStatus)proc.Call(_req);
            }
            return _resp;

        }
        public IResponseStatus UpdateFlatComm(decimal comm, int UserID)
        {
            var _resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (_lr.LoginTypeID == LoginType.ApplicationUser || IsCustomerCareAuthorised(ActionCodes.EditUser))
            {
                var _req = new CommonReq
                {
                    LoginID = _lr.UserID,
                    LoginTypeID = _lr.LoginTypeID,
                    CommonInt = UserID,
                    CommonDecimal = comm,
                    CommonStr = _rinfo.GetRemoteIP(),
                    CommonStr2 = _rinfo.GetBrowser()
                };
                IProcedure proc = new ProcUpdateFlatComm(_dal);
                _resp = (ResponseStatus)proc.Call(_req);
            }
            return _resp;
        }

        public IResponseStatus PartialUpdate(UserCreate param)
        {
            var _resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (_lr.LoginTypeID == LoginType.ApplicationUser || IsCustomerCareAuthorised(ActionCodes.EditUser))
            {
                var _req = new UserCreate
                {
                    LoginID = _lr.UserID,
                    LTID = _lr.LoginTypeID,
                    UserID = param.UserID,
                    IsGSTApplicable = param.IsGSTApplicable,
                    IsTDSApplicable = param.IsTDSApplicable,
                    DMRModelID = param.DMRModelID,
                    IsWebsite = param.IsWebsite,
                    WebsiteName = param.WebsiteName,
                    IP = _rinfo.GetRemoteIP(),
                    Browser = _rinfo.GetBrowser()
                };
                IProcedure proc = new ProcPartialUserUpdate(_dal);
                _resp = (ResponseStatus)proc.Call(_req);
            }
            return _resp;
        }
        public UserRoleSlab GetRoleSlab()
        {
            var UserID = 0;
            if (ApplicationSetting.IsAccountStatement && _lr.RoleID == Role.FOS)
            {
                IProcedure procUpline = new ProcGetDirectPrentOfUser(_dal);
                var parent = (UserInfo)procUpline.Call(new CommonReq
                {
                    LoginTypeID = _lr.LoginTypeID,
                    LoginID = _lr.UserID
                });
                UserID = parent.UserID;
            }
            else
            {
                UserID = _lr.LoginTypeID == LoginType.CustomerCare ? 1 : _lr.UserID;
            }
            IProcedure proc = new ProcUserSelectRoleSlab(_dal);
            return (UserRoleSlab)proc.Call(UserID);
        }
        public List<RoleMaster> GetChildRole()
        {
            var res = new List<RoleMaster>();
            if (!_lr.RoleID.In(Role.APIUser, Role.Retailor_Seller))
            {
                IProcedure proc = new FnGetChildRoles(_dal);
                res = (List<RoleMaster>)proc.Call(_lr.RoleID);
            }
            return res;
        }
        public List<RoleMaster> GetChildRole(int RoleID)
        {
            var res = new List<RoleMaster>();
            if (!RoleID.In(Role.APIUser, Role.Retailor_Seller))
            {
                IProcedure proc = new FnGetChildRoles(_dal);
                res = (List<RoleMaster>)proc.Call(RoleID);
            }
            return res;
        }
        //public UserList GetList(CommonFilter _filter)
        //{
        //    var _resp = new UserList();
        //    var UserID = 0;
        //    if (ApplicationSetting.IsAccountStatement && _lr.RoleID == Role.FOS)
        //    {
        //        IProcedure procUpline = new ProcGetDirectPrentOfUser(_dal);
        //        var parent = (UserInfo)procUpline.Call(new CommonReq
        //        {
        //            LoginTypeID = _lr.LoginTypeID,
        //            LoginID = _lr.UserID
        //        });
        //        UserID = parent.UserID;
        //    }

        //    if ((_lr.LoginTypeID == LoginType.ApplicationUser && !IsEndUser()) || IsCustomerCareAuthorised(ActionCodes.ShowUser))
        //    {
        //        var validate = Validate.O;
        //        var _req = new UserRequest
        //        {
        //            LoginID = (ApplicationSetting.IsAccountStatement && _lr.RoleID == Role.FOS) ? UserID : _lr.UserID,
        //            SortByID = _filter.SortByID,
        //            LTID = _lr.LoginTypeID,
        //            Browser = _rinfo.GetBrowserFullInfo(),
        //            IP = _rinfo.GetRemoteIP(),
        //            RoleID = _filter.RoleID,
        //            IsDesc = _filter.IsDesc,
        //            TopRows = _filter.TopRows,
        //            btnID = _filter.btnID
        //        };
        //        if (_filter.Criteria > 0)
        //        {
        //            if ((_filter.CriteriaText ?? "") == "")
        //            {
        //                return _resp;
        //            }
        //        }
        //        if (_filter.Criteria == Criteria.OutletMobile)
        //        {
        //            if (!validate.IsMobile(_filter.CriteriaText))
        //            {
        //                return _resp;
        //            }
        //            _req.MobileNo = _filter.CriteriaText;
        //        }
        //        if (_filter.Criteria == Criteria.EmailID)
        //        {
        //            if (!validate.IsEmail(_filter.CriteriaText))
        //            {
        //                return _resp;
        //            }
        //            _req.EmailID = _filter.CriteriaText;
        //        }
        //        if (_filter.Criteria == Criteria.Name)
        //        {
        //            if (validate.IsNumeric(_filter.CriteriaText) || _filter.CriteriaText.Length > 101)
        //            {
        //                return _resp;
        //            }
        //            _req.Name = _filter.CriteriaText;
        //        }
        //        if (_filter.Criteria == Criteria.UserID)
        //        {
        //            var Prefix = Validate.O.Prefix(_filter.CriteriaText);
        //            if (Validate.O.IsNumeric(Prefix))
        //                _filter.UserID = Validate.O.IsNumeric(_filter.CriteriaText) ? Convert.ToInt32(_filter.CriteriaText) : _filter.UserID;
        //            var uid = Validate.O.LoginID(_filter.CriteriaText);
        //            _req.UserID = Validate.O.IsNumeric(uid) ? Convert.ToInt32(uid) : _filter.UserID;
        //        }
        //        IProcedure proc = new ProcUserList(_dal);
        //        _resp = (UserList)proc.Call(_req);
        //        if (_lr.LoginTypeID == LoginType.ApplicationUser && !IsEndUser() || IsCustomerCareAuthorised(ActionCodes.ShowUser))
        //        {
        //            _resp.CanChangeOTPStatus = (_lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser) || IsCustomerCareAuthorised(ActionCodes.ChangeOTPStatus);
        //            _resp.CanChangeUserStatus = _lr.LoginTypeID == LoginType.ApplicationUser || IsCustomerCareAuthorised(ActionCodes.ChangeUserStatus);
        //            _resp.CanAssignPackage = _lr.LoginTypeID == LoginType.ApplicationUser || IsCustomerCareAuthorised(ActionCodes.PackageTransfer);
        //            _resp.CanFundTransfer = _lr.LoginTypeID == LoginType.ApplicationUser || IsCustomerCareAuthorised(ActionCodes.FundTransfer);
        //            _resp.CanEdit = _lr.LoginTypeID == LoginType.ApplicationUser || IsCustomerCareAuthorised(ActionCodes.EditUser);
        //            _resp.CanChangeSlab = ((_lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser) || (_lr.LoginTypeID == LoginType.ApplicationUser && !IsEndUser() && !_lr.IsAdminDefined)) || IsCustomerCareAuthorised(ActionCodes.AddEditSLAB);
        //            _resp.CanChangeRole = (_lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser) || IsCustomerCareAuthorised(ActionCodes.AddEditSLAB);
        //            _resp.LoginID = (_lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser || _lr.LoginTypeID == LoginType.CustomerCare) ? 1 : _lr.UserID;
        //            _resp.CanAssignAvailablePackage = (_lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser);
        //            _resp.CanRegeneratePassword = (_lr.LoginTypeID == LoginType.ApplicationUser && !IsEndUser()) || IsCustomerCareAuthorised(ActionCodes.RegenratePassword);
        //            _resp.CanCalculateCommissionFromCircle = (_lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser) || IsCustomerCareAuthorised(ActionCodes.CanCalculateCommissionFromCircle);
        //        }
        //    }
        //    return _resp;
        //}

        public UserList GetList(CommonFilter _filter)
        {
            var _resp = new UserList();
            var UserID = 0;
            if (ApplicationSetting.IsAccountStatement && _filter.LoginRoleID == Role.FOS)
            {
                IProcedure procUpline = new ProcGetDirectPrentOfUser(_dal);
                var parent = (UserInfo)procUpline.Call(new CommonReq
                {
                    LoginTypeID = _filter.LT,
                    LoginID = _filter.LoginID
                });
                UserID = parent.UserID;
            }

            if ((_filter.LT == LoginType.ApplicationUser && !IsEndUser()) || IsCustomerCareAuthorised(ActionCodes.ShowUser))
            {
                var validate = Validate.O;
                var _req = new UserRequest
                {
                    LoginID = (ApplicationSetting.IsAccountStatement && _filter.LoginRoleID == Role.FOS) ? UserID : _filter.LoginID,
                    SortByID = _filter.SortByID,
                    LTID = _filter.LT,
                    Browser = _rinfo.GetBrowserFullInfo(),
                    IP = _rinfo.GetRemoteIP(),
                    RoleID = _filter.RoleID,
                    IsDesc = _filter.IsDesc,
                    TopRows = _filter.TopRows,
                    btnID = _filter.btnID
                };
                if (_filter.Criteria > 0)
                {
                    if ((_filter.CriteriaText ?? "") == "")
                    {
                        return _resp;
                    }
                }
                if (_filter.Criteria == Criteria.OutletMobile)
                {
                    if (!validate.IsMobile(_filter.CriteriaText))
                    {
                        return _resp;
                    }
                    _req.MobileNo = _filter.CriteriaText;
                }
                if (_filter.Criteria == Criteria.EmailID)
                {
                    if (!validate.IsEmail(_filter.CriteriaText))
                    {
                        return _resp;
                    }
                    _req.EmailID = _filter.CriteriaText;
                }
                if (_filter.Criteria == Criteria.Name)
                {
                    if (validate.IsNumeric(_filter.CriteriaText) || _filter.CriteriaText.Length > 101)
                    {
                        return _resp;
                    }
                    _req.Name = _filter.CriteriaText;
                }
                if (_filter.Criteria == Criteria.SlabName)
                {
                    if (validate.IsNumeric(_filter.CriteriaText) || _filter.CriteriaText.Length > 101)
                    {
                        return _resp;
                    }
                    _req.SlabName = _filter.CriteriaText;
                }
                if (_filter.Criteria == Criteria.UserID)
                {
                    var Prefix = Validate.O.Prefix(_filter.CriteriaText);
                    if (Validate.O.IsNumeric(Prefix))
                        _filter.UserID = Validate.O.IsNumeric(_filter.CriteriaText) ? Convert.ToInt32(_filter.CriteriaText) : _filter.UserID;
                    var uid = Validate.O.LoginID(_filter.CriteriaText);
                    _req.UserID = Validate.O.IsNumeric(uid) ? Convert.ToInt32(uid) : _filter.UserID;
                }
                IProcedure proc = new ProcUserList(_dal);
                _resp = (UserList)proc.Call(_req);
                if (_filter.LT == LoginType.ApplicationUser && !IsEndUser() || IsCustomerCareAuthorised(ActionCodes.ShowUser))
                {
                    _resp.CanChangeOTPStatus = (_filter.LoginRoleID == Role.Admin && _filter.LT == LoginType.ApplicationUser) || IsCustomerCareAuthorised(ActionCodes.ChangeOTPStatus);
                    _resp.CanChangeUserStatus = _filter.LT == LoginType.ApplicationUser || IsCustomerCareAuthorised(ActionCodes.ChangeUserStatus);
                    _resp.CanAssignPackage = _filter.LT == LoginType.ApplicationUser || IsCustomerCareAuthorised(ActionCodes.PackageTransfer);
                    _resp.CanFundTransfer = _filter.LT == LoginType.ApplicationUser || IsCustomerCareAuthorised(ActionCodes.FundTransfer);
                    _resp.CanEdit = _filter.LT == LoginType.ApplicationUser || IsCustomerCareAuthorised(ActionCodes.EditUser);
                    _resp.CanChangeSlab = ((_filter.LoginRoleID == Role.Admin && _filter.LT == LoginType.ApplicationUser) || (_filter.LT == LoginType.ApplicationUser && !IsEndUser() && !_filter.IsAdminDefined)) || IsCustomerCareAuthorised(ActionCodes.AddEditSLAB);
                    _resp.CanChangeRole = (_filter.LoginRoleID == Role.Admin && _filter.LT == LoginType.ApplicationUser) || IsCustomerCareAuthorised(ActionCodes.AddEditSLAB);
                    _resp.LoginID = (_filter.LoginRoleID == Role.Admin && _filter.LT == LoginType.ApplicationUser || _filter.LT == LoginType.CustomerCare) ? 1 : _filter.LoginID;
                    _resp.CanAssignAvailablePackage = (_filter.LoginRoleID == Role.Admin && _filter.LT == LoginType.ApplicationUser);
                    _resp.CanRegeneratePassword = (_filter.LT == LoginType.ApplicationUser && !IsEndUser()) || IsCustomerCareAuthorised(ActionCodes.RegenratePassword);
                    _resp.CanCalculateCommissionFromCircle = (_filter.LoginRoleID == Role.Admin && _filter.LT == LoginType.ApplicationUser) || IsCustomerCareAuthorised(ActionCodes.CanCalculateCommissionFromCircle);
                }
            }
            return _resp;
        }
        public UserList GetListChild(int ReffID, bool IsUp)
        {
            var _filter = new CommonReq
            {
                LoginID = _lr.UserID,
                LoginTypeID = _lr.LoginTypeID,
                CommonInt = ReffID,
                IsListType = IsUp
            };
            var _resp = new UserList();
            if ((_lr.LoginTypeID == LoginType.ApplicationUser && !IsEndUser()) || IsCustomerCareAuthorised(ActionCodes.ShowUser))
            {
                IProcedure proc = new ProcUserListChild(_dal);
                _resp.userReports = (List<UserReport>)proc.Call(_filter);
                if (_lr.LoginTypeID == LoginType.ApplicationUser && !IsEndUser() || IsCustomerCareAuthorised(ActionCodes.ShowUser))
                {
                    _resp.CanChangeOTPStatus = (_lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser) || IsCustomerCareAuthorised(ActionCodes.ChangeOTPStatus);
                    _resp.CanChangeUserStatus = _lr.LoginTypeID == LoginType.ApplicationUser || IsCustomerCareAuthorised(ActionCodes.ChangeUserStatus);
                    _resp.CanAssignPackage = _lr.LoginTypeID == LoginType.ApplicationUser || IsCustomerCareAuthorised(ActionCodes.PackageTransfer);
                    _resp.CanFundTransfer = _lr.LoginTypeID == LoginType.ApplicationUser || IsCustomerCareAuthorised(ActionCodes.FundTransfer);
                    _resp.CanEdit = _lr.LoginTypeID == LoginType.ApplicationUser || IsCustomerCareAuthorised(ActionCodes.EditUser);
                    _resp.CanChangeSlab = ((_lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser) || (_lr.LoginTypeID == LoginType.ApplicationUser && !IsEndUser() && !_lr.IsAdminDefined)) || IsCustomerCareAuthorised(ActionCodes.AddEditSLAB);
                    _resp.CanChangeRole = (_lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser) || IsCustomerCareAuthorised(ActionCodes.AddEditSLAB);
                    _resp.LoginID = (_lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser || _lr.LoginTypeID == LoginType.CustomerCare) ? 1 : _lr.UserID;
                    _resp.CanAssignAvailablePackage = (_lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser);
                }
            }
            return _resp;
        }
        public UserList GetFOSList(CommonFilter _filter)
        {
            var _resp = new UserList();
            if ((_lr.LoginTypeID == LoginType.ApplicationUser && !IsEndUser()) || IsCustomerCareAuthorised(ActionCodes.ShowUser))
            {
                var validate = Validate.O;
                var _req = new UserRequest
                {
                    LoginID = _lr.UserID,
                    SortByID = _filter.SortByID,
                    LTID = _lr.LoginTypeID,
                    Browser = _rinfo.GetBrowserFullInfo(),
                    IP = _rinfo.GetRemoteIP(),
                    RoleID = _filter.RoleID,
                    IsDesc = _filter.IsDesc,
                    IsFOSListAdmin = _filter.IsFOSListAdmin,

                };
                if (_filter.IsFOSListAdmin)
                {
                    _req.MobileNo = _filter.CriteriaText;
                }

                if (_filter.Criteria > 0)
                {
                    if ((_filter.CriteriaText ?? "") == "")
                    {
                        return _resp;
                    }
                }
                if (_filter.Criteria == Criteria.OutletMobile)
                {
                    if (!validate.IsMobile(_filter.CriteriaText))
                    {
                        return _resp;
                    }
                    _req.MobileNo = _filter.CriteriaText;
                }
                if (_filter.Criteria == Criteria.EmailID)
                {
                    if (!validate.IsEmail(_filter.CriteriaText))
                    {
                        return _resp;
                    }
                    _req.EmailID = _filter.CriteriaText;
                }
                if (_filter.Criteria == Criteria.Name)
                {
                    if (validate.IsNumeric(_filter.CriteriaText) || _filter.CriteriaText.Length > 101)
                    {
                        return _resp;
                    }
                    _req.Name = _filter.CriteriaText;
                }
                if (_filter.Criteria == Criteria.UserID)
                {
                    var Prefix = Validate.O.Prefix(_filter.CriteriaText);
                    if (Validate.O.IsNumeric(Prefix))
                        _filter.UserID = Validate.O.IsNumeric(_filter.CriteriaText) ? Convert.ToInt32(_filter.CriteriaText) : _filter.UserID;
                    var uid = Validate.O.LoginID(_filter.CriteriaText);
                    _req.UserID = Validate.O.IsNumeric(uid) ? Convert.ToInt32(uid) : _filter.UserID;
                }
                IProcedure proc = new ProcFOSList(_dal);
                _resp.userReports = (List<UserReport>)proc.Call(_req);
                if (_lr.LoginTypeID == LoginType.ApplicationUser && !IsEndUser() || IsCustomerCareAuthorised(ActionCodes.ShowUser))
                {
                    _resp.CanChangeOTPStatus = (_lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser) || IsCustomerCareAuthorised(ActionCodes.ChangeOTPStatus);
                    _resp.CanChangeUserStatus = _lr.LoginTypeID == LoginType.ApplicationUser || IsCustomerCareAuthorised(ActionCodes.ChangeUserStatus);
                    _resp.CanAssignPackage = _lr.LoginTypeID == LoginType.ApplicationUser || IsCustomerCareAuthorised(ActionCodes.PackageTransfer);
                    _resp.CanFundTransfer = _lr.LoginTypeID == LoginType.ApplicationUser || IsCustomerCareAuthorised(ActionCodes.FundTransfer);
                    _resp.CanEdit = _lr.LoginTypeID == LoginType.ApplicationUser || IsCustomerCareAuthorised(ActionCodes.EditUser);
                    _resp.CanChangeSlab = (_lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser) || IsCustomerCareAuthorised(ActionCodes.AddEditSLAB);
                    _resp.LoginID = (_lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser || _lr.LoginTypeID == LoginType.CustomerCare) ? 1 : _lr.UserID;
                }
            }
            return _resp;
        }
        public IResponseStatus ChangeFOSColSts(int UserID)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            if (_lr.LoginTypeID == LoginType.ApplicationUser)
            {
                var _req = new CommonReq
                {
                    LoginID = UserID
                };
                IProcedure _proc = new ProcChangeFOSColSts(_dal);
                _res = (ResponseStatus)_proc.Call(_req);
            }
            return _res;
        }
        public IResponseStatus FundTransfer(FundProcess fundProcess)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if ((_lr.LoginTypeID == LoginType.ApplicationUser && !IsEndUser()) || IsCustomerCareAuthorised(ActionCodes.FundTransfer))
            {
                if (_lr.IsDoubleFactor && string.IsNullOrEmpty(fundProcess.SecurityKey))
                {
                    res.Msg = "Please Fill Security Key";
                    return res;
                }
                var req = new FundProcessReq
                {
                    LoginTypeID = _lr.LoginTypeID,
                    LoginID = _lr.UserID,
                    fundProcess = fundProcess,
                    CommonStr = _rinfo.GetRemoteIP(),
                    CommonStr2 = _rinfo.GetBrowser()
                    //IsDebitWithApproval= ApplicationSetting.IsDebitWithApproval
                };
                var _res = new AlertReplacementModel();
                if (ApplicationSetting.IsDebitWithApproval && fundProcess.OType == true && _lr.LoginTypeID != 1)
                {
                    IProcedure proc = new ProcFundProcessDebitApprova(_dal);
                    _res = (AlertReplacementModel)proc.Call(req);
                }
                else
                {
                    IProcedure proc = new ProcFundProcess(_dal);
                    _res = (AlertReplacementModel)proc.Call(req);

                }

                res.Statuscode = _res.Statuscode;
                res.Msg = _res.Msg;
                if (_res.Statuscode == ErrorCodes.One)
                {
                    try
                    {
                        //IUserWebsite _userWebsite = new UserML(_accessor, _env);
                        var ml = new AlertML(_accessor, _env);
                        if (!fundProcess.OType)
                        {

                            #region To
                            _res.FormatID = MessageFormat.FundReceive;
                            _res.NotificationTitle = "Fund Receive";
                            Parallel.Invoke(() => ml.FundReceiveSMS(_res),
                                () => ml.FundReceiveEmail(_res),
                                () => ml.FundReceiveNotification(_res),
                                () => ml.WebNotification(_res),
                                () => ml.SocialAlert(_res));
                            #endregion

                            #region From
                            _res.FormatID = MessageFormat.FundTransfer;
                            _res.NotificationTitle = "Fund Transfer";
                            _res.WhatsappNo = _res.WhatsappNoL;
                            _res.TelegramNo = _res.TelegramNoL;
                            _res.HangoutNo = _res.HangoutNoL;

                            Parallel.Invoke(() => ml.FundTransferSMS(_res),
                                () => ml.FundTransferEmail(_res),
                                () => ml.FundTransferNotification(_res),
                                () => ml.WebNotification(_res),
                                () => ml.SocialAlert(_res));
                            #endregion
                        }
                        else
                        {
                            #region From
                            _res.FormatID = MessageFormat.FundCredit;
                            _res.NotificationTitle = "Fund Credit";
                            Parallel.Invoke(() => ml.FundCreditSMS(_res),
                                () => ml.FundCreditEmail(_res),
                                () => ml.FundCreditNotification(_res),
                                () => ml.WebNotification(_res),
                                () => ml.SocialAlert(_res));
                            #endregion

                            #region To
                            _res.FormatID = MessageFormat.FundDebit;
                            _res.NotificationTitle = "Fund Debit";
                            Parallel.Invoke(() => ml.FundDebitSMS(_res),
                                () => ml.FundDebitEmail(_res),
                                () => ml.FundDebitNotification(_res),
                                () => ml.WebNotification(_res),
                                () => ml.SocialAlert(_res));
                            #endregion
                        }
                    }
                    catch (Exception ex)
                    {
                        var errorLog = new ErrorLog
                        {
                            ClassName = GetType().Name,
                            FuncName = "FundTransfer",
                            Error = ex.Message,
                            LoginTypeID = _lr.LoginTypeID,
                            UserId = _lr.UserID
                        };
                        var _ = new ProcPageErrorLog(_dal).Call(errorLog);
                    }
                }
            }
            return res;
        }
        public IResponseStatus FundTransferApp(FundProcessReq fundProcessreq)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };

            fundProcessreq.CommonStr = _rinfo.GetRemoteIP();
            fundProcessreq.CommonStr2 = _rinfo.GetBrowser();
            IProcedure proc = new ProcFundProcess(_dal);
            var _res = (AlertReplacementModel)proc.Call(fundProcessreq);
            res.Statuscode = _res.Statuscode;
            res.Msg = _res.Msg;
            if (_res.Statuscode == ErrorCodes.One)
            {
                IUserWebsite _userWebsite = new UserML(_accessor, _env);
                var ml = new AlertML(_accessor, _env);
                if (!fundProcessreq.fundProcess.OType)
                {

                    #region To
                    _res.FormatID = MessageFormat.FundReceive;
                    _res.NotificationTitle = "Fund Receive";
                    Parallel.Invoke(() => ml.FundReceiveSMS(_res),
                        () => ml.FundReceiveEmail(_res),
                        () => ml.FundReceiveNotification(_res),
                        () => ml.WebNotification(_res),
                        () => ml.SocialAlert(_res));
                    #endregion

                    #region From
                    _res.FormatID = MessageFormat.FundTransfer;
                    _res.NotificationTitle = "Fund Transfer";
                    _res.WhatsappNo = _res.WhatsappNoL;
                    _res.TelegramNo = _res.TelegramNoL;
                    _res.HangoutNo = _res.HangoutNoL;
                    Parallel.Invoke(() => ml.FundTransferSMS(_res),
                        () => ml.FundTransferEmail(_res),
                        () => ml.FundTransferNotification(_res),
                        () => ml.WebNotification(_res),
                        () => ml.SocialAlert(_res));
                    #endregion
                }
                else
                {
                    #region From
                    _res.FormatID = MessageFormat.FundCredit;
                    _res.NotificationTitle = "Fund Credit";
                    Parallel.Invoke(() => ml.FundCreditSMS(_res),
                        () => ml.FundCreditEmail(_res),
                        () => ml.FundCreditNotification(_res),
                        () => ml.WebNotification(_res),
                        () => ml.SocialAlert(_res));
                    #endregion

                    #region To
                    _res.FormatID = MessageFormat.FundDebit;
                    _res.NotificationTitle = "Fund Debit";
                    Parallel.Invoke(() => ml.FundDebitSMS(_res),
                        () => ml.FundDebitEmail(_res),
                        () => ml.FundDebitNotification(_res),
                        () => ml.WebNotification(_res),
                        () => ml.SocialAlert(_res));
                    #endregion
                }
            }
            return res;
        }
        public FundRequetResp GetUserFundTransferData(int PID)
        {
            var res = new FundRequetResp();
            if ((_lr.LoginTypeID == LoginType.ApplicationUser && !IsEndUser()) || IsCustomerCareAuthorised(ActionCodes.FundTransfer))
            {
                if (PID > 0)
                {
                    //Get payemnt request data
                    IProcedure _proc = new ProcGetFundTransferData(_dal);
                    res = (FundRequetResp)_proc.Call(PID);
                }
            }
            return res;
        }
        public IResponseStatus FundReject(FundProcess fundProcess)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if ((_lr.LoginTypeID == LoginType.ApplicationUser && !IsEndUser()) || IsCustomerCareAuthorised(ActionCodes.FundTransfer))
            {
                var req = new FundProcessReq
                {
                    LoginTypeID = _lr.LoginTypeID,
                    LoginID = _lr.UserID,
                    fundProcess = fundProcess,
                    CommonStr = _rinfo.GetRemoteIP(),
                    CommonStr2 = _rinfo.GetBrowser()
                };
                IProcedure _proc = new ProcFundRejectProcess(_dal);
                _res = (ResponseStatus)_proc.Call(req);
            }
            return _res;
        }
        public IResponseStatus FundRejectFromApp(FundProcessReq req)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            req.CommonStr = _rinfo.GetRemoteIP();
            req.CommonStr2 = _rinfo.GetBrowser();
            IProcedure _proc = new ProcFundRejectProcess(_dal);
            _res = (ResponseStatus)_proc.Call(req);
            return _res;
        }
        public UserInfo GetUser(string MobileNo, int UT = 1)
        {
            UserInfo userData = new UserInfo();
            if ((_lr.LoginTypeID == LoginType.ApplicationUser) || IsCustomerCareAuthorised(ActionCodes.APISwitch))
            {
                var req = new CommonReq
                {
                    LoginTypeID = _lr.LoginTypeID,
                    LoginID = _lr.UserID,
                    CommonInt = UT,
                    CommonStr = Validate.O.IsMobile(MobileNo ?? "") ? MobileNo : ""
                };
                if (!Validate.O.IsMobile(MobileNo))
                {
                    var Prefix = Validate.O.Prefix(MobileNo);
                    if (Validate.O.IsNumeric(Prefix))
                        req.CommonInt = Validate.O.IsNumeric(MobileNo) ? Convert.ToInt32(MobileNo) : req.CommonInt;
                    var uid = Validate.O.LoginID(MobileNo);
                    req.CommonInt2 = Validate.O.IsNumeric(uid) ? Convert.ToInt32(uid) : req.CommonInt2;
                    req.CommonStr = "";
                }
                IProcedure proc = new ProcGetUserByDetail(_dal);
                userData = (UserInfo)proc.Call(req);
            }
            return userData;
        }
        //public ResponseStatus UpdateUserDenominationStatus(int UserID)
        //{
        //    var res = new ResponseStatus
        //    {
        //        Statuscode = ErrorCodes.Minus1,
        //        Msg = ErrorCodes.AuthError
        //    };
        //    if (_lr.LoginTypeID == LoginType.ApplicationUser && _lr.UserID == 1 || IsCustomerCareAuthorised(ActionCodes.APISwitch))
        //    {
        //        ProcUpdateUserwiseAPISwitch proc = new ProcUpdateUserwiseAPISwitch(_dal);
        //        if (proc.UpdateIsDenominationSwitchBlock(UserID))
        //        {
        //            res.Statuscode = ErrorCodes.One;
        //            res.Msg = "Status changed successfully";
        //        }
        //        else
        //        {
        //            res.Msg = ErrorCodes.TempError;
        //        }
        //    }
        //    return res;
        //}
        public IEnumerable<PaymentModeMaster> PaymentModes()
        {
            var bMList = new List<PaymentModeMaster>();
            if (_lr.LoginTypeID == LoginType.ApplicationUser && _lr.RoleID != Role.Admin)
            {
                CommonReq commonReq = new CommonReq
                {
                    LoginID = _lr.UserID,
                    LoginTypeID = _lr.LoginTypeID
                };
                IProcedure proc = new ProcGetPaymentMode(_dal);
                bMList = (List<PaymentModeMaster>)proc.Call(commonReq);
            }

            return bMList;
        }
        public IEnumerable<PaymentModeMaster> PaymentModesForApp(CommonReq commonReq)
        {
            var res = new List<PaymentModeMaster>();
            if (commonReq.LoginTypeID == LoginType.ApplicationUser)
            {
                IProcedure proc = new ProcGetPaymentMode(_dal);
                res = (List<PaymentModeMaster>)proc.Call(commonReq);
            }

            return res;
        }
        public IEnumerable<FundRequestToUser> FundRequestToUser()
        {
            var _res = new List<FundRequestToUser>();
            if (_lr.LoginTypeID == LoginType.ApplicationUser && _lr.RoleID != Role.Admin)
            {
                var commonReq = new CommonReq
                {
                    LoginID = _lr.UserID,
                    LoginTypeID = _lr.LoginTypeID
                };
                IProcedure proc = new ProcFundRequestToUser(_dal);
                _res = (List<FundRequestToUser>)proc.Call(commonReq);
            }
            return _res;
        }
        public IEnumerable<FundRequestToUser> FundRequestToUserApp(CommonReq commonReq)
        {
            var _res = new List<FundRequestToUser>();
            if (commonReq.LoginTypeID == LoginType.ApplicationUser)
            {
                IProcedure proc = new ProcFundRequestToUser(_dal);
                _res = (List<FundRequestToUser>)proc.Call(commonReq);
            }
            return _res;
        }
        public IResponseStatus FundRequestOperation(FundRequest obj)
        {
            var res = new ResponseStatus()
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (_lr.RoleID.In(Role.Admin) && _lr.LoginTypeID != LoginType.ApplicationUser)
            {
                return res;
            }
            obj.LoginID = _lr.UserID;
            IProcedure _proc = new ProcUserFundRequest(_dal);
            var _res = (AlertReplacementModel)_proc.Call(obj);
            res.Statuscode = _res.Statuscode;
            res.Msg = _res.Msg;
            IAlertML alert = new AlertML(_accessor, _env);
            Parallel.Invoke(() => alert.FundOrderSMS(_res),
                () => alert.FundOrderEmail(_res),
                () => alert.FundOrderNotification(_res),
                () => alert.SocialAlert(_res),

                async () => await alert.WebNotification(_res));

            return res;
        }

        public IResponseStatus FundRequestOperationApp(FundRequest fundRequest)
        {
            fundRequest.IsAuto = false;
            if (fundRequest.OrderID > 0 && !string.IsNullOrEmpty(fundRequest.Checksum))
            {
                var generateOrderUPIRequest = new GenerateOrderUPIRequest
                {
                    Amount = Convert.ToInt32(fundRequest.Amount),
                    OrderID = fundRequest.OrderID,
                    AppVersion = fundRequest.AppVersion,
                    Browser = _rinfo.GetBrowserFullInfo(),
                    RequestIP = _rinfo.GetRemoteIP(),
                    IMEI = fundRequest.IMEI,
                    SessionID = fundRequest.SessionID,
                    UPIID = fundRequest.UPIID,
                    UserID = fundRequest.LoginID
                };
                IProcedure procedure = new ProcValidateUPIOrder(_dal);
                var checkRes = (ResponseStatus)procedure.Call(generateOrderUPIRequest);
                if (checkRes.Statuscode == ErrorCodes.One)
                {
                    if (!string.IsNullOrEmpty(checkRes.CommonStr))
                    {
                        //OrderKey|OrderID|Amount|SessionID|UserID|UPIID
                        StringBuilder checkData = new StringBuilder();
                        checkData.Append(checkRes.CommonStr);
                        checkData.Append("|");
                        checkData.Append(generateOrderUPIRequest.OrderID);
                        checkData.Append("|");
                        checkData.Append(generateOrderUPIRequest.Amount);
                        checkData.Append("|");
                        checkData.Append(generateOrderUPIRequest.SessionID);
                        checkData.Append("|");
                        checkData.Append(generateOrderUPIRequest.UserID);
                        checkData.Append("|");
                        checkData.Append(generateOrderUPIRequest.UPIID);
                        string ch = HashEncryption.O.AppEncryptPayment(checkData.ToString());
                        fundRequest.IsAuto = ch.Equals(fundRequest.Checksum);
                    }
                }
            }
            IResponseStatus res = new ResponseStatus()
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            IProcedure _proc = new ProcUserFundRequest(_dal);
            var _res = (AlertReplacementModel)_proc.Call(fundRequest);
            res.Statuscode = _res.Statuscode;
            res.Msg = _res.Msg;
            IAlertML alert = new AlertML(_accessor, _env);

            Parallel.Invoke(() => alert.FundOrderSMS(_res),
            () => alert.FundOrderEmail(_res),
            () => alert.FundOrderNotification(_res),
            async () => await alert.WebNotification(_res));

            return res;
        }
        public IEnumerable<PaymentModeMaster> PaymentModes(int BankID = 0)
        {
            var bMList = new List<PaymentModeMaster>();
            if (_lr.LoginTypeID == LoginType.ApplicationUser && _lr.RoleID != Role.Admin)
            {
                CommonReq commonReq = new CommonReq
                {
                    LoginID = _lr.UserID,
                    LoginTypeID = _lr.LoginTypeID,
                    CommonInt = BankID
                };
                IProcedure proc = new ProcGetPaymentMode(_dal);
                bMList = (List<PaymentModeMaster>)proc.Call(commonReq);
            }

            return bMList;
        }

        public UserBalnace GetUserBalnace(int UID)
        {
            var _res = new UserBalnace();
            var loginResp = chkAlternateSession();
            if (loginResp != null)
            {
                if (loginResp.LoginTypeID == LoginType.ApplicationUser || IsCustomerCareAuthorised(ActionCodes.FundTransfer) || IsCustomerCareAuthorised(ActionCodes.ShowUserLedger))
                {
                    var commonReq = new CommonReq
                    {
                        LoginID = UID == 0 ? (loginResp.LoginTypeID == LoginType.CustomerCare ? 1 : loginResp.UserID) : UID,
                        LoginTypeID = loginResp.LoginTypeID == LoginType.CustomerCare ? LoginType.ApplicationUser : loginResp.LoginTypeID
                    };
                    IProcedure proc = new ProcGetUserBal(_dal);
                    _res = (UserBalnace)proc.Call(commonReq);
                }
            }
            return _res;
        }
        public UserBalnace GetUserBalnace(int UserID, int LoginTypeID)
        {
            var _res = new UserBalnace();
            if (LoginTypeID == LoginType.ApplicationUser)
            {
                var commonReq = new CommonReq
                {
                    LoginID = UserID,
                    LoginTypeID = LoginTypeID
                };
                IProcedure proc = new ProcGetUserBal(_dal);
                _res = (UserBalnace)proc.Call(commonReq);
            }
            return _res;
        }
        public List<MoveToWalletMapping> GetMoveToWalletsMap()
        {
            IProcedure proc = new ProcGetMoveToWalletMapping(_dal);
            return (List<MoveToWalletMapping>)proc.Call();
        }
        public List<MasterCompanyType> GetCompanyTypeMaster(CommonReq commonReq)
        {
            IProcedure _proc = new ProcGetCompanyTypeMaster(_dal);
            return (List<MasterCompanyType>)_proc.Call(commonReq);
        }
        public GetEditUser GetEditUser(int UserID)
        {
            var res = new GetEditUser();
            if (_lr.LoginTypeID == LoginType.ApplicationUser || IsCustomerCareAuthorised(ActionCodes.EditUser))
            {
                var req = new CommonReq
                {
                    LoginID = _lr.UserID,
                    LoginTypeID = _lr.LoginTypeID,
                    CommonInt = UserID
                };
                IProcedure proc = new ProcEditUserDetail(_dal);
                res = (GetEditUser)proc.Call(req);
                IProcedure proc1 = new ProcGetState(_dal);
                res.States = (List<StateMaster>)proc1.Call();
            }
            return res;
        }
        public IEnumerable<DMRModel> GetDMRModelList()
        {
            ProcEditUserDetail procEditUserDetail = new ProcEditUserDetail(_dal);
            return (List<DMRModel>)procEditUserDetail.GetDMRModels();
        }
        public GetEditUser GetEditUserForApp(CommonReq commonReq)
        {
            IProcedure proc = new ProcEditUserDetail(_dal);
            return (GetEditUser)proc.Call(commonReq);
        }
        public PincodeDetail GetPinCodeDetail(string Pincode)
        {
            IProcedure proc = new ProcGetPinCodeDetail(_dal);
            return (PincodeDetail)proc.Call(Pincode);
        }
        public IResponseStatus UpdateProfile(GetEditUser _req)
        {
            IResponseStatus _resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };

            if (Validate.O.IsNumeric(_req.Name ?? "") || (_req.Name ?? "").Length > 100)
            {
                _resp.Msg = ErrorCodes.InvalidParam + " Name";
                return _resp;
            }
            if (Validate.O.IsNumeric(_req.OutletName ?? "") || (_req.OutletName ?? "").Length > 100)
            {
                _resp.Msg = ErrorCodes.InvalidParam + " Outlet Name";
                return _resp;
            }

            if (!Validate.O.IsEmail(_req.EmailID ?? ""))
            {
                _resp.Msg = ErrorCodes.InvalidParam + " EmailID";
                return _resp;
            }
            if (!Validate.O.IsPinCode(_req.Pincode ?? ""))
            {
                _resp.Msg = ErrorCodes.InvalidParam + " Pincode";
                return _resp;
            }
            if ((_req.Address ?? "").Length > 300)
            {
                _resp.Msg = ErrorCodes.InvalidParam + " Address";
                return _resp;
            }
            if ((_req.City ?? "").Length > 50)
            {
                _resp.Msg = ErrorCodes.InvalidParam + " City";
                return _resp;
            }
            if (!Validate.O.IsPAN(_req.PAN ?? ""))
            {
                _resp.Msg = ErrorCodes.InvalidParam + " PAN";
                return _resp;
            }
            if (!Validate.O.IsAADHAR(_req.AADHAR ?? ""))
            {
                _resp.Msg = ErrorCodes.InvalidParam + " AADHAR";
                return _resp;
            }
            if (_req.EmailID != _lr.EmailID)
            {

                if (string.IsNullOrWhiteSpace(_session.GetString(SessionKeys.EmailVerify)) || (_req.MobileNo ?? "") == "")
                {
                    string key = HashEncryption.O.CreatePasswordNumeric(6);
                    _session.SetString(SessionKeys.EmailVerify, key);
                    IEmailML emailManager = new EmailML(_dal);
                    string str = "Please verify your emailid with otp: " + key;
                    emailManager.SendMail(_req.EmailID, null, "One Time Password", str, _WInfo.WID, _resourceML.GetLogoURL(_WInfo.WID).ToString());
                    _resp.CommonInt = ErrorCodes.One;
                    return _resp;
                }
                else
                {
                    string key = _session.GetString(SessionKeys.EmailVerify);
                    if ((_req.MobileNo ?? "").Length < 6)
                    {
                        _resp.Msg = ErrorCodes.InvalidParam + " OTP";
                        return _resp;
                    }
                    else if (key == "")
                    {
                        _resp.Msg = ErrorCodes.InvalidParam + " OTP";
                        return _resp;
                    }
                    else if (key != _req.MobileNo)
                    {
                        _resp.Msg = nameof(ErrorCodes.Invalid_OTP).Replace("_", " ");
                        return _resp;
                    }
                }
            }

            _req.LoginID = _lr.UserID;
            _req.LT = _lr.LoginTypeID;
            _req.IP = _rinfo.GetRemoteIP();
            _req.Browser = _rinfo.GetBrowserFullInfo();
            IProcedure proc = new ProcUpdateProfile(_dal);
            _resp = (ResponseStatus)proc.Call(_req);
            if (_resp.Statuscode == ErrorCodes.One && _req.EmailID != _lr.EmailID)
            {
                LoginResponse loginres = _lr;
                loginres.EmailID = _req.EmailID;
                _session.SetObjectAsJson(SessionKeys.LoginResponse, loginres);
            }
            return _resp;
        }
        public IResponseStatus UserBankRequest(GetEditUser _req)
        {
            var _resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (_lr.LoginTypeID == LoginType.ApplicationUser || IsCustomerCareAuthorised(ActionCodes.EditUser))
            {

                _req.LoginID = _lr.UserID;
                _req.LT = _lr.LoginTypeID;
                IProcedure proc = new ProcUserBankRequest(_dal);
                _resp = (ResponseStatus)proc.Call(_req);
            }
            return _resp;
        }
        public UserBalnace ShowTotalChildBalance(int UserID)
        {
            var res = new UserBalnace();


            if (_lr.LoginTypeID == LoginType.ApplicationUser || IsCustomerCareAuthorised(ActionCodes.ShowUser))
            {
                var req = new CommonReq
                {
                    LoginID = _lr.UserID,
                    LoginTypeID = _lr.LoginTypeID,
                    CommonInt = UserID
                };
                IProcedure proc = new ProcGetChildsBalance(_dal);
                res = (UserBalnace)proc.Call(req);
            }
            return res;
        }
        public List<ResponseStatus> GetUpperRole(int ID)
        {
            IProcedure f = new procGetUpper(_dal);
            return (List<ResponseStatus>)f.Call(ID);
        }
        #endregion

        #region ChangePassword
        public AlertReplacementModel ChangePin(ChangePassword ChPass)
        {
            var _res = new AlertReplacementModel
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            if ((ChPass.NewPassword ?? "").Length < 4 || (ChPass.NewPassword ?? "").Length > 6 || !Validate.O.IsNumeric(ChPass.NewPassword ?? "a"))
            {
                _res.Msg = ErrorCodes.InvalidPinLength;
                return _res;
            }
            if ((ChPass.OldPassword ?? "").Length < 8 || (ChPass.OldPassword ?? "").Length > 12)
            {
                _res.Msg = "Old Password is invalid";
                return _res;
            }

            var req = new LoginReq
            {
                CommonStr = ChPass.NewPassword,
                CommonStr2 = ChPass.OldPassword,
                LoginID = _lr.UserID,
                LoginTypeID = _lr.LoginTypeID,
                Browser = _rinfo.GetBrowser(),
                RequestIP = _rinfo.GetRemoteIP(),
                RequestMode = RequestMode.PANEL,
                CommonInt = _lr.SessID
            };
            IProcedure proc = new ProcChangePin(_dal);
            return (AlertReplacementModel)proc.Call(req);
        }

        public AlertReplacementModel ChangePin(ChangePassword ChPass, int LoginTypeID, int UserID)
        {
            var _res = new AlertReplacementModel
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            if ((ChPass.NewPassword ?? "").Length < 4 || (ChPass.NewPassword ?? "").Length > 6 || !Validate.O.IsNumeric(ChPass.NewPassword ?? "a"))
            {
                _res.Msg = ErrorCodes.InvalidPinLength;
                return _res;
            }
            if ((ChPass.OldPassword ?? "").Length < 8 || (ChPass.OldPassword ?? "").Length > 12)
            {
                _res.Msg = "Old Password is invalid";
                return _res;
            }

            var req = new LoginReq
            {
                CommonStr = ChPass.NewPassword,
                CommonStr2 = ChPass.OldPassword,
                LoginID = UserID,
                LoginTypeID = LoginTypeID,
                Browser = _rinfo.GetBrowser(),
                RequestIP = _rinfo.GetRemoteIP(),
                RequestMode = RequestMode.APPS,
                CommonInt = ChPass.SessID
            };
            IProcedure proc = new ProcChangePin(_dal);
            return (AlertReplacementModel)proc.Call(req);
        }

        public AlertReplacementModel ChangePassword(ChangePassword ChPass)
        {
            AlertReplacementModel _res = new AlertReplacementModel
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            if ((ChPass.NewPassword ?? "").Length < 8 || (ChPass.NewPassword ?? "").Length > 12)
            {
                _res.Msg = "New Password length should be 8 to 12 with AlphaNumeric";
                return _res;
            }
            if ((ChPass.OldPassword ?? "").Length < 8 || (ChPass.OldPassword ?? "").Length > 12)
            {
                _res.Msg = "Old Password is invalid";
                return _res;
            }

            var req = new LoginReq
            {
                CommonStr = ChPass.NewPassword,
                CommonStr2 = ChPass.OldPassword,
                LoginID = _lr.UserID,
                LoginTypeID = _lr.LoginTypeID,
                Browser = _rinfo.GetBrowser(),
                RequestIP = _rinfo.GetRemoteIP(),
                RequestMode = RequestMode.PANEL,
                CommonInt = _lr.SessID
            };
            IProcedure proc = new ProcChangePassword(_dal);
            return (AlertReplacementModel)proc.Call(req);
        }

        public AlertReplacementModel ChangePassword(ChangePassword ChPass, int LoginTypeID, int UserID)
        {
            AlertReplacementModel _res = new AlertReplacementModel
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            if ((ChPass.NewPassword ?? "").Length < 8 || (ChPass.NewPassword ?? "").Length > 12)
            {
                _res.Msg = "New Password length should be 8 to 12 with AlphaNumeric";
                return _res;
            }
            if ((ChPass.OldPassword ?? "").Length < 8 || (ChPass.OldPassword ?? "").Length > 12)
            {
                _res.Msg = "Old Password is invalid";
                return _res;
            }

            var req = new LoginReq
            {
                CommonStr = ChPass.NewPassword,
                CommonStr2 = ChPass.OldPassword,
                LoginID = UserID,
                LoginTypeID = LoginTypeID,
                Browser = _rinfo.GetBrowser(),
                RequestIP = _rinfo.GetRemoteIP(),
                RequestMode = RequestMode.APPS,
                CommonInt = ChPass.SessID
            };
            //IProcedure proc = new ProcChangePassword(_dal);
            //return (AlertReplacementModel)proc.Call(req);
            IProcedure proc = new ProcChangePassword(_dal);
            var _changePassword = (AlertReplacementModel)proc.Call(req);
            _res.Msg = _changePassword.Msg;
            _res.Statuscode = _changePassword.Statuscode;
            return _res;

        }
        #endregion
        #region IUserWebsite
        public IResponseStatus CompanyProfileCU(CompanyProfileDetailReq req)
        {
            ILoginML loginML = new LoginML(_accessor, _env);
            req.LoginID = _lr.UserID;
            req.LT = _lr.LoginTypeID;
            req.website = req.website.Replace("https://", "").Replace("http://", "").Replace("www.", "").Replace(" ", "").ToLower();
            IProcedure proc = new ProcCompanyProfileCU(_dal);
            return (ResponseStatus)proc.Call(req);

        }
        public CompanyProfileDetail GetCompanyProfileUser(int UserId)
        {
            IProcedure proc = new ProcCompanyProfileUserWise(_dal);
            return (CompanyProfileDetail)proc.Call(UserId);
        }
        #endregion
        #region KYCSection
        public IEnumerable<DocTypeMaster> GetDocTypeMaster(bool IsOutlet)
        {
            var list = new List<DocTypeMaster>();
            if (_lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser || IsCustomerCareAuthorised(ActionCodes.KYCDocumentEdit))
            {
                CommonReq req = new CommonReq
                {
                    LoginID = _lr.UserID,
                    LoginTypeID = _lr.LoginTypeID
                };
                if (!IsOutlet)
                {
                    IProcedure _proc = new ProcGetDocTypeDetails(_dal);
                    list = (List<DocTypeMaster>)_proc.Call(req);
                }
                else
                {
                    IProcedure _proc = new ProcGetDocTypeDetailsOfOutlets(_dal);
                    list = (List<DocTypeMaster>)_proc.Call(req);
                }
            }
            return list;
        }
        public IResponseStatus UpdateDocTypeMaster(DocTypeMaster docTypeMaster)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (_lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser || IsCustomerCareAuthorised(ActionCodes.KYCDocumentEdit))
            {
                docTypeMaster.UserId = _lr.UserID;
                docTypeMaster.LoginTypeID = _lr.LoginTypeID;

                IProcedure _proc = new ProcUpdateDocType(_dal);
                _res = (ResponseStatus)_proc.Call(docTypeMaster);
            }

            return _res;
        }

        public IEnumerable<DocTypeMaster> GetDocuments(int uid)
        {
            var list = new List<DocTypeMaster>();
            if (_lr.LoginTypeID == LoginType.ApplicationUser || IsCustomerCareAuthorised(ActionCodes.UploadKYC))
            {
                var req = new DocTypeMaster()
                {
                    LoginTypeID = _lr.LoginTypeID,
                    LoginId = _lr.UserID,
                    UserId = _lr.LoginTypeID == LoginType.ApplicationUser && uid == 0 ? _lr.UserID : uid,
                    OutletID = _lr.LoginTypeID == LoginType.ApplicationUser && uid == 0 ? (_lr.OutletID == 0 ? -1 : 0) : 0,
                };
                IProcedure _proc = new ProcGetUserDocuments(_dal);
                list = (List<DocTypeMaster>)_proc.Call(req);
                if (list != null && _lr.OutletID == 0)
                {
                    if (list.Count > 0)
                    {
                        if (list[0].OutletID > 0)
                        {
                            LoginResponse loginres = _lr;
                            loginres.OutletID = list[0].OutletID;
                            _session.SetObjectAsJson(SessionKeys.LoginResponse, loginres);
                        }
                    }
                }
            }
            return list;
        }
        public IEnumerable<DocTypeMaster> GetDocumentsForApp(DocTypeMaster docTypeMaster)
        {
            IProcedure _proc = new ProcGetUserDocuments(_dal);
            return (List<DocTypeMaster>)_proc.Call(docTypeMaster);
        }

        public IResponseStatus UploadDocuments(IFormFile file, int dtype, int uid)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (_lr.LoginTypeID == LoginType.ApplicationUser || IsCustomerCareAuthorised(ActionCodes.UploadKYC))
            {
                if (!dtype.In(DOCType.PAN, DOCType.AADHAR, DOCType.PHOTO, DOCType.GSTRegistration, DOCType.BusinessAddressProof, DOCType.CancelledCheque, DOCType.ServiceAggreement, DOCType.PASSBOOK, DOCType.VoterID, DOCType.DrivingLicense, DOCType.ShopImage))
                {
                    _res.Msg = "Invalid Document Type";
                    return _res;
                }
                if (!file.ContentType.Any())
                {
                    _res.Msg = "File not found!";
                    return _res;
                }
                if (file.Length < 1)
                {
                    _res.Msg = "Empty file not allowed!";
                    return _res;
                }
                //if (file.Length / 1024 > 200)
                //{
                //    _res.Msg = "File size exceeded! Not more than 200 KB is allowed";
                //    return _res;
                //}
                var filename = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                string ext = Path.GetExtension(filename);
                byte[] filecontent = null;
                using (var ms = new MemoryStream())
                {
                    file.CopyTo(ms);
                    filecontent = ms.ToArray();
                }
                if (!Validate.O.IsFileAllowed(filecontent, ext))
                {
                    _res.Msg = "Invalid File Format!";
                    return _res;
                }
                uid = _lr.LoginTypeID == LoginType.ApplicationUser && uid == 0 ? _lr.UserID : uid;
                string dbFileName = dtype + "_" + uid + ext;
                bool IsFileUploaded = true;
                try
                {
                    //IEKYCML userML = new EKYCML(_accessor, _env);
                    //var details = userML.GetEKYCDetailOfUser(new CommonReq
                    //{
                    //    LoginID = _lr.UserID
                    //});

                    //if (ApplicationSetting.IsEKYCForced)
                    //{
                    //    if (!details.IsEKYCDone)
                    //    {
                    //        _res.Msg = "First complete EKYC then upload docs";
                    //        return _res;
                    //    }
                    //    if (dtype.In(DOCType.PAN))
                    //    {
                    //        OCRHelper ocr = new OCRHelper();
                    //        string imgText = ocr.ReadTextFromImage(new OcrModel
                    //        {
                    //            Image = file,
                    //            DestinationLanguage = DestinationLanguage.English
                    //        });
                    //        if (!string.IsNullOrEmpty(imgText))
                    //        {
                    //            if (details.IsGSTIN)
                    //            {
                    //                if (!string.IsNullOrEmpty(details.PanOfDirector))
                    //                {
                    //                    if (!imgText.Contains(details.PanOfDirector))
                    //                    {
                    //                        _res.Msg = "(NOPAN)Invalid PAN Image";
                    //                        return _res;
                    //                    }
                    //                    if (!imgText.Contains(details.Name))
                    //                    {
                    //                        _res.Msg = "(NONAME)Invalid PAN Image";
                    //                        return _res;
                    //                    }
                    //                }
                    //                else
                    //                {
                    //                    if (!imgText.Contains(details.PAN))
                    //                    {
                    //                        _res.Msg = "(NOPAN)Invalid PAN Image";
                    //                        return _res;
                    //                    }
                    //                    if (!imgText.Contains(details.OutletName))
                    //                    {
                    //                        _res.Msg = "(NOPAN)Invalid PAN Image";
                    //                        return _res;
                    //                    }
                    //                }
                    //            }

                    //        }
                    //    }
                    //}
                    string savefilename = DOCType.DocFilePath + dbFileName;
                    if (file.Length / 1024 > 200)
                    {
                        ImageResizer imageResizer = new ImageResizer(200 * 1024, filecontent, ext.ToLower());
                        filecontent = imageResizer.ScaleImage();
                    }
                    using (FileStream fs = new FileStream(savefilename, FileMode.Create))
                    {
                        fs.Write(filecontent, 0, filecontent.Length);
                        fs.Flush();
                    }
                }
                catch (Exception ex)
                {
                    IsFileUploaded = false;
                    _res.Msg = "File can not be uploaded! Try again later..";
                    var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                    {
                        ClassName = GetType().Name,
                        FuncName = "UploadDocuments",
                        Error = ex.Message,
                        LoginTypeID = _lr.LoginTypeID,
                        UserId = _lr.UserID
                    });
                }
                if (IsFileUploaded)
                {
                    var dr = new DocumentReq
                    {
                        LT = _lr.LoginTypeID,
                        LoginID = _lr.UserID,
                        URL = dbFileName,
                        DocType = dtype,
                        ChildUserID = uid,
                        IP = _rinfo.GetRemoteIP(),
                        Browser = _rinfo.GetBrowserFullInfo(),
                        APIUserOutletID = 0
                    };
                    IProcedure _proc = new ProcUploadDocument(_dal);
                    _res = (ResponseStatus)_proc.Call(dr);
                }
            }

            return _res;
        }

        public IResponseStatus UploadDocumentsForApp(IFormFile file, int dtype, int uid, int LoginTypeID, int LoginID)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };

            if (!dtype.In(DOCType.PAN, DOCType.AADHAR, DOCType.PHOTO, DOCType.GSTRegistration, DOCType.BusinessAddressProof, DOCType.CancelledCheque, DOCType.ServiceAggreement, DOCType.ShopImage, DOCType.DrivingLicense, DOCType.PASSBOOK, DOCType.VoterID))
            {
                _res.Msg = "Invalid Document Type";
                return _res;
            }
            if (!file.ContentType.Any())
            {
                _res.Msg = "File not found!";
                return _res;
            }
            if (file.Length < 1)
            {
                _res.Msg = "Empty file not allowed!";
                return _res;
            }
            //if (file.Length / 1024 > 200)
            //{
            //    _res.Msg = "File size exceeded! Not more than 200 KB is allowed";
            //    return _res;
            //}

            var filename = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
            string ext = Path.GetExtension(filename);
            byte[] filecontent = null;
            using (var ms = new MemoryStream())
            {
                file.CopyTo(ms);
                filecontent = ms.ToArray();
            }
            if (!Validate.O.IsFileAllowed(filecontent, ext))
            {
                _res.Msg = "Invalid File Format!";
                return _res;
            }


            uid = LoginTypeID == LoginType.ApplicationUser && uid == 0 ? LoginID : uid;
            string dbFileName = dtype + "_" + LoginID + ext;
            bool IsFileUploaded = true;
            try
            {
                if (file.Length / 1024 > 200)
                {
                    ImageResizer imageResizer = new ImageResizer(200 * 1024, filecontent, ext.ToLower());
                    filecontent = imageResizer.ScaleImage();
                }
                string savefilename = DOCType.DocFilePath + dbFileName;
                using (FileStream fs = new FileStream(savefilename, FileMode.Create))
                {
                    fs.Write(filecontent, 0, filecontent.Length);
                    fs.Flush();
                }
            }
            catch (Exception ex)
            {
                IsFileUploaded = false;
                _res.Msg = "File can not be uploaded! Try again later..";
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "UploadDocuments",
                    Error = ex.Message,
                    LoginTypeID = LoginTypeID,
                    UserId = LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            if (IsFileUploaded)
            {
                var dr = new DocumentReq
                {
                    LT = LoginTypeID,
                    LoginID = LoginID,
                    URL = dbFileName,
                    DocType = dtype,
                    ChildUserID = uid,
                    IP = _rinfo.GetRemoteIP(),
                    Browser = _rinfo.GetBrowserFullInfo(),
                    APIUserOutletID = 0
                };
                IProcedure _proc = new ProcUploadDocument(_dal);
                _res = (ResponseStatus)_proc.Call(dr);
            }

            return _res;
        }

        public async Task<IEnumerable<GetEditUser>> GetKYCUsers(UserRequest req)
        {
            var res = new List<GetEditUser>();
            if (_lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser || IsCustomerCareAuthorised(ActionCodes.VerifyDocuments))
            {
                req.LTID = _lr.LoginTypeID;
                req.LoginID = _lr.UserID;
                IProcedureAsync proc = new ProcGetPendingKYCUsers(_dal);
                res = (List<GetEditUser>)await proc.Call(req);
            }
            return res;
        }


        public IEnumerable<DocTypeMaster> GetDocumentsForApproval(int UserID, int OutletID)
        {
            var list = new List<DocTypeMaster>();
            if (_lr.LoginTypeID == LoginType.ApplicationUser && UserID != _lr.UserID || IsCustomerCareAuthorised(ActionCodes.VerifyDocuments))
            {
                var req = new DocTypeMaster()
                {
                    LoginTypeID = _lr.LoginTypeID,
                    LoginId = _lr.UserID,
                    UserId = UserID,
                    OutletID = OutletID
                };
                IProcedure _proc = new ProcGetUserDocuments(_dal);
                list = (List<DocTypeMaster>)_proc.Call(req);
            }

            return list;
        }
        public ResponseStatus DownloadKYC(int DocID)
        {
            var req = new CommonReq
            {
                LoginID = _lr.UserID,
                LoginTypeID = _lr.LoginTypeID,
                CommonInt = DocID
            };
            IProcedure proc = new ProcGetDocument(_dal);
            return (ResponseStatus)proc.Call(req);
        }
        public ResponseStatus ApproveKYCDoc(int ID, int VerifyStatus, string DRemark)
        {
            var doc = new DocTypeMaster
            {
                LoginId = _lr.UserID,
                ID = ID,
                VerifyStatus = VerifyStatus,
                DRemark = DRemark,
                LoginTypeID = _lr.LoginTypeID,
                DocName = _rinfo.GetRemoteIP(),
                DocUrl = _rinfo.GetBrowserFullInfo()
            };
            IProcedure _proc = new ProcApproveKYCDocument(_dal);
            return (ResponseStatus)_proc.Call(doc);
        }
        public IResponseStatus ChangeKYCStatus(KYCStatusReq kYCStatusReq)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (_lr.LoginTypeID == LoginType.ApplicationUser || IsCustomerCareAuthorised(ActionCodes.ApproveKYC))
            {
                kYCStatusReq.LT = _lr.LoginTypeID;
                kYCStatusReq.LoginID = _lr.UserID;
                kYCStatusReq.IP = _rinfo.GetRemoteIP();
                kYCStatusReq.Browser = _rinfo.GetBrowserFullInfo();

                IProcedure _proc = new ProcUpdateKYCStatus(_dal);
                res = (ResponseStatus)_proc.Call(kYCStatusReq);
            }
            return res;
        }
        public IResponseStatus ChangeKYCStatusForApp(KYCStatusReq kYCStatusReq)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (kYCStatusReq.LT == LoginType.ApplicationUser)
            {
                kYCStatusReq.IP = _rinfo.GetRemoteIP();
                kYCStatusReq.Browser = _rinfo.GetBrowserFullInfo();

                IProcedure _proc = new ProcUpdateKYCStatus(_dal);
                res = (ResponseStatus)_proc.Call(kYCStatusReq);
            }
            return res;
        }
        public List<KYCDoc> GetOnBoardKyc(int UserID, int OutletID)
        {
            var _lst = new List<KYCDoc>();
            if (OutletID > 0)
            {
                var req = new CommonReq
                {
                    LoginID = UserID,
                    CommonInt = OutletID
                };
                IProcedure proc = new ProcGetDocumentOnboard(_dal);
                _lst = (List<KYCDoc>)proc.Call(req);
            }
            return _lst;
        }

        public ResponseStatus CheckKycStatus(int UserID)
        {
            var req = new CommonReq
            {
                LoginID = UserID == 0 ? _lr.UserID : UserID,
                LoginTypeID = _lr.LoginTypeID
            };
            IProcedure _proc = new ProcCheckKycStatus(_dal);
            var res = (ResponseStatus)_proc.Call(req);
            return res;
        }

        public IResponseStatus UpdateUser(GetEditUser _req)
        {
            var _resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (ApplicationSetting.WithCustomLoginID)
            {

                IProcedure _pro = new ProcCheckCustomLogin(_dal);
                var request = new UserDuplicate
                {
                    CustomLoginID = _req.CustomLoginID,
                    LoginID = _req.UserID
                };
                var response = (ResponseStatus)_pro.Call(request);
                if (response.Statuscode == ErrorCodes.Minus1)
                {
                    response.Msg = ErrorCodes.DCLOEx;
                    return response;
                }
            }
            if (_lr.LoginTypeID == LoginType.ApplicationUser && _lr.RoleID != Role.Admin)
            {
                _req.UserID = _req.UserID == 0 || _lr.RoleID.In(Role.APIUser, Role.Customer, Role.Retailor_Seller) ? _lr.UserID : _req.UserID;
                _req.LoginID = _lr.UserID;
                _req.LT = _lr.LoginTypeID;
                _req.MobileNo = string.IsNullOrEmpty(_req.MobileNo) || _lr.RoleID.In(Role.APIUser, Role.Customer, Role.Retailor_Seller) ? _lr.MobileNo : _req.MobileNo;
            }
            if (_lr.LoginTypeID == LoginType.ApplicationUser || IsCustomerCareAuthorised(ActionCodes.EditUser))
            {
                if (Validate.O.IsNumeric(_req.Name ?? "") || (_req.Name ?? "").Length > 100)
                {
                    _resp.Msg = ErrorCodes.InvalidParam + " Name";
                    return _resp;
                }
                if (Validate.O.IsNumeric(_req.OutletName ?? "") || (_req.OutletName ?? "").Length > 100)
                {
                    _resp.Msg = ErrorCodes.InvalidParam + " Outlet Name";
                    return _resp;
                }
                if (!Validate.O.IsMobile(_req.MobileNo ?? ""))
                {
                    _resp.Msg = ErrorCodes.InvalidParam + " Mobile Number";
                    return _resp;
                }
                double dDiff = (DateTime.Now - Convert.ToDateTime(_req.DOB)).TotalDays / 365;
                if (dDiff < 18 || dDiff > 100)
                {
                    _resp.Msg = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " DOB. Age must be between 18 and 100";
                    return _resp;
                }
                if (!(_req.EmailID ?? "").Contains("@"))
                {
                    _resp.Msg = ErrorCodes.InvalidParam + " EmailID";
                    return _resp;
                }
                if (!Validate.O.IsPinCode(_req.Pincode ?? ""))
                {
                    _resp.Msg = ErrorCodes.InvalidParam + " Pincode";
                    return _resp;
                }
                if ((_req.Address ?? "").Length > 300)
                {
                    _resp.Msg = ErrorCodes.InvalidParam + " Address";
                    return _resp;
                }
                if ((_req.City ?? "").Length > 50)
                {
                    _resp.Msg = ErrorCodes.InvalidParam + " City";
                    return _resp;
                }
                if (!(_lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser))
                {
                    if (!Validate.O.IsPAN(_req.PAN ?? ""))
                    {
                        _resp.Msg = ErrorCodes.InvalidParam + " PAN";
                        return _resp;
                    }
                    if (!Validate.O.IsAADHAR(_req.AADHAR ?? ""))
                    {
                        _resp.Msg = ErrorCodes.InvalidParam + " AADHAR";
                        return _resp;
                    }
                }
                if (_req.IsWebsite && !(_req.WebsiteName ?? "").Contains("."))
                {
                    _resp.Msg = ErrorCodes.InvalidParam + "Domain Name";
                    return _resp;
                }

                if (_req.IsRegisteredWithGST && (string.IsNullOrEmpty(_req.GSTIN) || _req.GSTIN.Length != 15))
                {
                    _resp.Msg = "Invalid GSTIN";
                    return _resp;
                }
                _req.LoginID = _lr.UserID;
                _req.LT = _lr.LoginTypeID;
                _req.IP = _rinfo.GetRemoteIP();
                _req.Browser = _rinfo.GetBrowser();
                _req.CustomLoginID = _req.CustomLoginID;
                IProcedure proc = new ProcUpdateUser(_dal);
                _resp = (ResponseStatus)proc.Call(_req);
                if (_resp.Statuscode == ErrorCodes.One)
                {
                    if (_req.IsWebsite && _resp.CommonInt > 0)
                    {
                        _resourceML.CreateWebsiteDirectory(_resp.CommonInt, FolderType.Website);
                    }
                }
                _req.WID = _resp.CommonInt;
            }
            return _resp;
        }
        public IResponseStatus SaveLowBalanceSetting(LowBalanceSetting param)
        {
            var _resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (_lr != null)
            {
                param.LT = _lr.LoginTypeID;
                param.LoginID = _lr.UserID;
            }
            IProcedure proc = new ProcSaveLowBalanceSetting(_dal);
            _resp = (ResponseStatus)proc.Call(param);
            return _resp;
        }

        #endregion

        #region AddWalletSection
        public async Task<IResponseStatus> AddWallet(FundProcessReq _req)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (_lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser || IsCustomerCareAuthorised(ActionCodes.AddWallet))
            {
                _req.LoginTypeID = _lr.LoginTypeID;
                _req.LoginID = _lr.UserID;
                _req.CommonStr = _rinfo.GetRemoteIP();
                _req.CommonStr2 = _rinfo.GetBrowser();
                IProcedureAsync _proc = new ProcAddWallet(_dal);
                _res = (ResponseStatus)await _proc.Call(_req);
            }
            return _res;
        }
        public string ShowPassword(int ID)
        {
            string password = string.Empty;
            IProcedure _proc = new ProcShowPassword(_dal);
            var p = (string)_proc.Call(ID);
            if (!p.Equals(string.Empty))
            {
                password = HashEncryption.O.Decrypt(p);
            }
            return password;
        }

        public string ShowPasswordCustomer(int ID)
        {
            if (_lr.RoleID == Role.Admin && _lr.RoleID == Role.Admin)
            {
                IProcedure _proc = new ProcShowPasswordCustomer(_dal);
                var p = (string)_proc.Call(ID);
                if (!p.Equals(string.Empty))
                {
                    return HashEncryption.O.Decrypt(p);
                }
            }
            return string.Empty;
        }
        #endregion

        #region BulkAction
        public List<UserReportBulk> BulkAction(CommonReq req)
        {
            var res = new List<UserReportBulk>();
            if (_lr.LoginTypeID == LoginType.ApplicationUser || IsCustomerCareAuthorised(ActionCodes.EditUser))
            {
                req.LoginID = _lr.UserID;
                req.LoginTypeID = _lr.LoginTypeID;
                IProcedure proc = new ProcGetUserbulkAction(_dal);
                res = (List<UserReportBulk>)proc.Call(req);
            }
            return res;
        }
        public IEnumerable<UserReportBulk> BulkActionApp(CommonReq req)
        {
            IProcedure proc = new ProcGetUserbulkAction(_dal);
            var res = (List<UserReportBulk>)proc.Call(req);
            return res;
        }
        public ResponseStatus GetUserFromRole(CommonReq req)
        {
            IProcedure proc = new ProcGetUserRole(_dal);
            return (ResponseStatus)proc.Call(req);
        }
        public async Task<List<BulkExcel>> BulkActionFixedRoles(CommonReq req)
        {
            var resp = new List<BulkExcel>();
            if (ApplicationSetting.IsRoleFixed)
            {
                var bulkAction = BulkAction(req);
                if (bulkAction.Any())
                {
                    IProcedureAsync procUpline = new ProcGetPrentsOfUser(_dal);
                    foreach (var item in bulkAction)
                    {
                        var bulkExcel = new BulkExcel
                        {
                            UserID = item.Prefix + item.ID,
                            UserName = item.OutletName,
                            MobileNo = item.MobileNo,
                            WhatsappNumber = item.WhatsAppNumber,
                            EMailID = item.EMail,
                            IsEMailVerified = item.EmailVerifiedStatus,
                            Balance = item.Balance,
                            UBalance = item.UBalance,
                            BBalance = item.BBalance,
                            PBalance = item.PacakgeBalance,
                            Role = item.Role,
                            Capping = item.Capping,
                            Referral = item.JoinBy,
                            KYCStatus = item.KYCStatus,
                            PackageName = item.PackageName,
                            RentalStatus = item.RentalStatus,
                            RentalAmt = item.RentalAmt,
                            IsAutoBilling = item.IsAutoBilling,
                            MaxBillingCountAB = item.MaxBillingCountAB,
                            FromFOSAB = item.FromFOSAB,
                            BalanceForAB = item.BalanceForAB,
                            MaxCreditLimitAB = item.MaxCreditLimitAB,
                            MaxTransferLimitAB = item.MaxTransferLimitAB
                        };
                        var parents = (List<UserInfo>)await procUpline.Call(new CommonReq
                        {
                            LoginTypeID = _lr.LoginTypeID,
                            LoginID = _lr.UserID,
                            CommonInt = item.ID
                        }).ConfigureAwait(false);
                        if (parents.Any())
                        {
                            foreach (var p in parents)
                            {
                                if (p.RoleID == FixedRole.SubAdmin)
                                {
                                    bulkExcel.SA = p.OutletName;
                                    bulkExcel.SAMobile = p.MobileNo;
                                    bulkExcel.SAID = p.Prefix + p.UserID;
                                }
                                if (p.RoleID == FixedRole.MasterDistributer)
                                {
                                    bulkExcel.MD = p.OutletName;
                                    bulkExcel.MDMobile = p.MobileNo;
                                    bulkExcel.MDID = p.Prefix + p.UserID;
                                }
                                if (p.RoleID == FixedRole.Distributor)
                                {
                                    bulkExcel.DT = p.OutletName;
                                    bulkExcel.DTMobile = p.MobileNo;
                                    bulkExcel.DTID = p.Prefix + p.UserID;
                                }
                                if (p.RoleID == FixedRole.Retailor)
                                {
                                    bulkExcel.RT = p.OutletName;
                                    bulkExcel.RTMobile = p.MobileNo;
                                    bulkExcel.RTID = p.Prefix + p.UserID;
                                }
                            }
                        }
                        resp.Add(bulkExcel);
                    }
                }
            }
            return resp;
        }
        public IResponseStatus DoBulkAction(BulkActionReq req)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (req.ActionID == 0)
            {
                res.Msg = "Please select any action";
                return res;
            }

            if (req.ActionID == BulkUserAction.Intro)
            {
                if (req.ToIntro.Equals("0"))
                {
                    res.Msg = "Invalid action change ReferalID";
                    return res;
                }
                if (req.Users.Equals("-1"))
                {
                    res.Msg = "Choose any user from list";
                    return res;
                }
                if (req.RoleID < 1 && req.Users.Split(",").Length > 2)
                {
                    res.Msg = "Select any role!";
                    return res;
                }
            }

            var bulk = new BulkAct
            {
                Act = req,
                IntoID = Convert.ToInt32(HashEncryption.O.Decrypt(req.Token)),
                LoginID = _lr.UserID,
                LTID = _lr.LoginTypeID
            };
            if (_lr.LoginTypeID == LoginType.ApplicationUser || IsCustomerCareAuthorised(ActionCodes.EditUser))
            {
                IProcedure proc = new ProcUserBulkAction(_dal);
                res = (ResponseStatus)proc.Call(bulk);
            }
            return res;
        }
        public ResponseStatus BulkDebitCredit(CommonReq commonReq)
        {
            var _resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };

            commonReq.LoginID = _lr.UserID;
            commonReq.LoginTypeID = _lr.LoginTypeID;
            commonReq.CommonStr2 = _rinfo.GetRemoteIP();
            commonReq.CommonStr3 = _rinfo.GetBrowserFullInfo();

            IProcedure _proc = new ProcBulkDebitCredit(_dal);
            _resp = (ResponseStatus)_proc.Call(commonReq);
            return _resp;
        }
        public BulkSmsEmail GetBulkSms()
        {
            ISMSAPIML ML = new APIML(_accessor, _env);
            var bulk = new BulkSmsEmail();
            if (_lr.LoginTypeID == LoginType.ApplicationUser && _lr.RoleID == Role.Admin || IsCustomerCareAuthorised(ActionCodes.BulkEmail) || IsCustomerCareAuthorised(ActionCodes.BulkSMS))
            {
                var UserID = _lr.LoginTypeID == LoginType.CustomerCare ? 1 : _lr.UserID;
                IProcedure proc = new SelectRole(_dal);
                bulk.Roles = (List<RoleMaster>)proc.Call(UserID);
                bulk.smsApi = ML.GetSMSAPIDetail();

            }
            return bulk;
        }

        public BulkSmsEmail GetBulkSmsSocial()
        {
            ISMSAPIML ML = new APIML(_accessor, _env);
            var bulk = new BulkSmsEmail();
            if (_lr.LoginTypeID == LoginType.ApplicationUser && _lr.RoleID == Role.Admin || IsCustomerCareAuthorised(ActionCodes.BulkEmail) || IsCustomerCareAuthorised(ActionCodes.BulkSMS))
            {
                var UserID = _lr.LoginTypeID == LoginType.CustomerCare ? 1 : _lr.UserID;
                IProcedure proc = new SelectRole(_dal);
                bulk.Roles = (List<RoleMaster>)proc.Call(UserID);
                bulk.smsApi = ML.GetSMSAPIDetail();
                bulk.smsApi = bulk.smsApi.Where(w => w.IsWhatsApp || w.IsTelegram || w.IsHangout);

            }
            return bulk;
        }
        public OpertorMessage GetOpertorMessage()
        {
            OpertorMessage res = new OpertorMessage();
            IOperatorML operatorML = new OperatorML(_accessor, _env);
            res.Operator = operatorML.GetOperators();
            IProcedure proc = new procGetMessage(_dal);
            res.MasterMessage = (List<MasterMessage>)proc.Call();
            return res;
        }
        public MessageTemplate GetMessageFormat(int FormatID)
        {
            IProcedure proc = new ProcGetMessageFormat(_dal);
            return (MessageTemplate)proc.Call(FormatID);
        }
        public IResponseStatus SendBulkSms(int ApiID, string MobileNOs, string Message)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            if (string.IsNullOrEmpty(MobileNOs))
            {
                res.Msg = "Invaild MobileMo";
                return res;
            }
            if (!MobileNOs.Contains(","))
            {
                res.Msg = "Invaild MobileMo";
                return res;
            }
            if (MobileNOs.Split(",").Length < 1)
            {
                res.Msg = "Invaild MobileMo";
                return res;
            }
            if (string.IsNullOrEmpty(Message))
            {
                res.Msg = "Fill message";
                return res;
            }
            if (ApiID < 1)
            {
                res.Msg = "Invaild API";
                return res;
            }
            if (_lr.LoginTypeID == LoginType.ApplicationUser && _lr.RoleID == Role.Admin || IsCustomerCareAuthorised(ActionCodes.BulkSMS))
            {
                ISMSAPIML ML = new APIML(_accessor, _env);
                SendSMSML sendSMSML = new SendSMSML(_dal);
                var detail = ML.GetSMSAPIDetailByID(ApiID);
                if (detail != null)
                {
                    var smsApi = new SMSSendBulk
                    {
                        APIID = detail.ID,
                        APIMethod = detail.APIMethod,
                        IsLapu = false,
                        SmsURL = detail.URL,
                        MobileNo = MobileNOs.TrimEnd(','),
                        WID = detail.WID,
                        SMS = Message
                    };
                    res = sendSMSML.SendSMSBulk(smsApi);
                }
            }
            return res;
        }
        public IResponseStatus SendBulkEmail(string Emails, string Message)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = "Mail " + ErrorCodes.NotSent
            };
            if (string.IsNullOrEmpty(Emails))
            {
                res.Msg = "Invaild EmailDs";
                return res;
            }
            if (!Emails.Contains(","))
            {
                res.Msg = "Invaild EmailDs";
                return res;
            }
            if (Emails.Split(",").Length < 1)
            {
                res.Msg = "Invaild EmailDs";
                return res;
            }
            if (string.IsNullOrEmpty(Message))
            {
                res.Msg = "Fill message";
                return res;
            }

            if (_lr.LoginTypeID == LoginType.ApplicationUser && _lr.RoleID == Role.Admin || IsCustomerCareAuthorised(ActionCodes.BulkEmail))
            {

                IEmailML emailManager = new EmailML(_dal);
                List<string> bccList = new List<string>();
                foreach (var item in Emails.Split(","))
                {
                    bccList.Add(item);
                }
                if (emailManager.SendMail("support@roundpay.in", bccList, "alert", Message, _WInfo.WID, _resourceML.GetLogoURL(_WInfo.WID).ToString()))
                {
                    res.Statuscode = ErrorCodes.One;
                    res.Msg = "Mail " + ErrorCodes.Sent;
                }
            }
            return res;
        }
        public IResponseStatus SendNotification(Notification req)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            if (string.IsNullOrEmpty(req.Title))
            {
                res.Msg = "Fill title";
                return res;
            }

            if (string.IsNullOrEmpty(req.Message))
            {
                res.Msg = "Fill message";
                return res;
            }
            bool IsFileUploaded = true; string dbFileName = req.Url, ImgID = string.Empty;
            if (req.file != null)
            {
                if (!req.file.ContentType.Any())
                {
                    res.Msg = "File not found!";
                    return res;
                }
                if (req.file.Length < 1)
                {
                    res.Msg = "Empty file not allowed!";
                    return res;
                }
                if (req.file.Length / 1024 > 200)
                {
                    res.Msg = "File size exceeded! Not more than 200 KB is allowed";
                    return res;
                }
                var filename = ContentDispositionHeaderValue.Parse(req.file.ContentDisposition).FileName.Trim('"');
                string ext = Path.GetExtension(filename);
                byte[] filecontent = null;
                using (var ms = new MemoryStream())
                {
                    req.file.CopyTo(ms);
                    filecontent = ms.ToArray();
                }
                if (!Validate.O.IsFileAllowed(filecontent, ext))
                {
                    res.Msg = "Invalid File Format!";
                    return res;
                }
                ImgID = HashEncryption.O.CreatePassword(8);
                dbFileName = DOCType.Notification + "_" + _lr.UserID + ImgID + ext;
                try
                {
                    string savefilename = DOCType.ImgNotification + dbFileName;
                    using (FileStream fs = File.Create(savefilename))
                    {
                        req.file.CopyTo(fs);
                        fs.Flush();
                    }
                }
                catch (Exception ex)
                {
                    IsFileUploaded = false;
                    res.Msg = "File can not be uploaded! Try again later..";
                    ErrorLog errorLog = new ErrorLog
                    {
                        ClassName = GetType().Name,
                        FuncName = "UploadDocuments",
                        Error = ex.Message,
                        LoginTypeID = _lr.LoginTypeID,
                        UserId = _lr.UserID
                    };
                    var _ = new ProcPageErrorLog(_dal).Call(errorLog);
                }
            }
            if (IsFileUploaded)
            {
                if (_lr.LoginTypeID == LoginType.ApplicationUser && _lr.RoleID == Role.Admin || IsCustomerCareAuthorised(ActionCodes.BulkNotification))
                {
                    req.WID = _WInfo.WID;
                    req.LT = _lr.LoginTypeID;
                    req.ImageUrl = dbFileName;
                    req.LT = _lr.LoginTypeID;
                    req.UserID = -1;
                    var sendSMSML = new SendSMSML(_dal);
                    req.Response = sendSMSML.SendNotification(req.Message, (req.ImageUrl ?? "").Trim(), req.Title, req.Url);
                    IProcedure proc = new ProcSendNotification(_dal);
                    res = (ResponseStatus)proc.Call(req);
                }
            }
            return res;
        }
        public List<Notification> GetNotifications()
        {
            if (_lr.LoginTypeID == LoginType.ApplicationUser || IsCustomerCareAuthorised(ActionCodes.BulkNotification))
            {
                IProcedure _Proc = new ProcGetNotifications(_dal);
                return (List<Notification>)_Proc.Call(_lr.UserID);
            }
            return null;
        }
        public List<Notification> GetNotificationsApp(int UserID)
        {
            IProcedure _Proc = new ProcGetNotifications(_dal);
            return (List<Notification>)_Proc.Call(UserID);
        }
        public IResponseStatus RemoveNotification(int ID)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (_lr.LoginTypeID == LoginType.ApplicationUser && _lr.RoleID == Role.Admin)
            {
                IProcedure _proc = new ProcDeleteNotification(_dal);
                res = (ResponseStatus)_proc.Call(ID);
            }
            return res;
        }
        #endregion

        public AlertReplacementModel GetUserDeatilForAlert(int UserID)
        {
            var req = new CommonReq
            {
                LoginID = UserID
            };
            IProcedure proc = new ProcUserDetailForAlert(_dal);
            var res = (AlertReplacementModel)proc.Call(req);
            return res;
        }

        #region OTPRelated
        public bool SendUserOTPForCommonThings(string OTP, int WID, string MobileNo, string EmailID)
        {
            var procSendSMS = new SMSSendREQ
            {
                FormatType = MessageFormat.OTP,
                MobileNo = MobileNo
            };
            try
            {
                var Tp_ReplaceKeywords = new DataTable();
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
                    FuncName = "SendUserOTPForCommonThings",
                    Error = "MobileNo:" + MobileNo + ",EmailID:" + EmailID + ",OTP:" + OTP + ",Ex:" + ex.Message,
                    LoginTypeID = WID,
                    UserId = 0
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return true;
        }
        public IResponseStatus GetOTPFromURL(string encdata)
        {
            IResponseStatus res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = nameof(ErrorCodes.Invalid_OTP).Replace("_", " ")
            };
            string decdata = HashEncryption.O.DecryptForURL(encdata ?? "");
            if (decdata.Split('_').Length == 4)
            {
                var dt = Convert.ToDateTime(decdata.Split('_')[3].Replace("|", "/"));
                var ts = DateTime.Now - dt;
                if (ts.TotalHours < 24)
                {
                    try
                    {
                        res.CommonStr = decdata.Split('_')[2];
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            return res;
        }
        #endregion
        #region CompanyProfileSection
        public CompanyProfileDetail GetCompanyProfileApp(int WID)
        {
            IProcedure proc = new ProcCompanyProfile(_dal);
            return (CompanyProfileDetail)proc.Call(WID);
        }
        #endregion

        #region MoveToWallet
        public async Task<IResponseStatus> MoveToWallet(CommonReq commonReq)
        {
            commonReq.LoginID = _lr.UserID;
            commonReq.LoginTypeID = _lr.LoginTypeID;
            var _resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            IProcedure proc = new ProcMoveToWalletActive(_dal);
            _resp = (ResponseStatus)proc.Call(commonReq);
            if (_resp.Statuscode == ErrorCodes.One && _resp.CommonInt > 0 && _resp.CommonBool)
            {
                var bankServiceReq = new BankServiceReq
                {
                    LoginID = _lr.UserID,
                    WalletRequestID = _resp.CommonInt,
                    RequestModeID = RequestMode.PANEL
                };
                _resp = await CallBankTransfer(bankServiceReq).ConfigureAwait(false);
            }
            return _resp;
        }
        public async Task<ResponseStatus> CallBankTransfer(BankServiceReq bankServiceReq)
        {
            var resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.One,
                Msg = nameof(ErrorCodes.Request_Accpeted)
            };
            bankServiceReq.RequestIP = _rinfo.GetRemoteIP();
            IProcedure _proc = new ProcBankTranferService(_dal);
            var res = (BankServiceResp)_proc.Call(bankServiceReq);
            if (res.Statuscode == ErrorCodes.Minus1)
            {
                resp.Msg = res.Msg;
                return resp;
            }

            var APIRes = new DMRTransactionResponse
            {
                Statuscode = -3
            };
            if (res.Statuscode == ErrorCodes.One)
            {
                if (res.APICode == APICode.PAYTM)
                {
                    var dMTReq = new DMTReq
                    {
                        Domain = string.Empty,
                        SenderNO = res.OutletMobile,
                        ChanelType = res.TransactionMode,
                        TID = res.TransactionID
                    };
                    var sendMoney = new ReqSendMoney
                    {
                        Amount = Convert.ToInt32(res.Amount),
                        AccountNo = res.AccountNumber,
                        IFSC = res.IFSC,
                        BeneName = res.AccountHolder
                    };
                    var dMRTransactionResponse = new DMRTransactionResponse
                    {
                        TID = res.TID,
                        TransactionID = res.TransactionID
                    };
                    var paytmML = new PaytmML(_accessor, _env, res.APIID);
                    APIRes = await paytmML.SendMoneyPayout(dMTReq, sendMoney, dMRTransactionResponse).ConfigureAwait(false);
                    resp.Msg = APIRes.Msg;
                }
                else if (res.APICode == APICode.ICICIBANKPAYOUT)
                {
                    var dMTReq = new DMTReq
                    {
                        Domain = string.Empty,
                        SenderNO = res.OutletMobile,
                        ChanelType = res.TransactionMode,
                        TID = res.TransactionID
                    };
                    var sendMoney = new ReqSendMoney
                    {
                        Amount = Convert.ToInt32(res.Amount),
                        AccountNo = res.AccountNumber,
                        IFSC = res.IFSC
                    };
                    var dMRTransactionResponse = new DMRTransactionResponse
                    {
                        TID = res.TID,
                        TransactionID = res.TransactionID
                    };
                    var iCICIPayoutML = new ICICIPayoutML(_accessor, _env, res.APIID);
                    APIRes = await iCICIPayoutML.ICICIPayout(dMTReq, sendMoney, dMRTransactionResponse).ConfigureAwait(false);
                    resp.Msg = APIRes.Msg;
                }
                else if (res.APICode == APICode.PAYU)
                {
                    var tAPIRequest = new MTAPIRequest
                    {
                        APIID = res.APIID,
                        TransactionID = res.TransactionID,
                        TID = res.TID,
                        SenderMobile = res.OutletMobile,
                        RequestMode = bankServiceReq.RequestModeID,
                        UserID = res.UserID,
                        mBeneDetail = new MBeneDetail
                        {
                            IFSC = res.IFSC,
                            AccountNo = res.AccountNumber,
                            MobileNo = res.OutletMobile,
                            BeneName = res.AccountHolder,
                            BankName = res.BankName
                        },
                        Amount = Convert.ToInt32(res.Amount),
                        TransMode = res.TransactionMode == PaymentMode_.PAYTM_IMPS ? "IMPS" : "NEFT",
                        IsPayout = true
                    };
                    IMoneyTransferAPIML moneyTransferAPIML = new PayUML(_accessor, _env, _dal);
                    APIRes = moneyTransferAPIML.AccountTransfer(tAPIRequest);
                    if (APIRes != null)
                    {
                        resp.Msg = APIRes.Msg;
                    }
                }
                else if (res.APICode.EndsWith(APICode.RAZORPAYOUT))
                {
                    var tAPIRequest = new MTAPIRequest
                    {
                        APIID = res.APIID,
                        TransactionID = res.TransactionID,
                        TID = res.TID,
                        SenderMobile = res.OutletMobile,
                        UserName = res.AccountHolder,
                        RequestMode = bankServiceReq.RequestModeID,
                        UserID = res.UserID,
                        mBeneDetail = new MBeneDetail
                        {
                            IFSC = res.IFSC,
                            AccountNo = res.AccountNumber,
                            MobileNo = res.OutletMobile,
                            BeneName = res.AccountHolder,
                            BankName = res.BankName
                        },
                        Amount = Convert.ToInt32(res.Amount),
                        TransMode = res.TransactionMode == PaymentMode_.PAYTM_IMPS ? "IMPS" : "NEFT",
                        IsPayout = true,
                        EmailID = res.EmailID,
                        APIGroupCode = res.APIGroupCode
                    };
                    IMoneyTransferAPIML moneyTransferAPIML = new RZRPayoutML(_accessor, _env, _dal, res.APICode);
                    APIRes = moneyTransferAPIML.AccountTransfer(tAPIRequest);
                    if (APIRes != null)
                    {
                        resp.Msg = APIRes.Msg;
                    }
                }
                else if (res.APICode == APICode.RPFINTECH && res.APIOpCode.Equals(APIOPCODE.RPXPRESS))
                {
                    var tAPIRequest = new MTAPIRequest
                    {
                        APIID = res.APIID,
                        TransactionID = res.TransactionID,
                        TID = res.TID,
                        SenderMobile = res.OutletMobile,
                        UserName = res.AccountHolder,
                        RequestMode = bankServiceReq.RequestModeID,
                        UserID = res.UserID,
                        mBeneDetail = new MBeneDetail
                        {
                            IFSC = res.IFSC,
                            AccountNo = res.AccountNumber,
                            MobileNo = res.OutletMobile,
                            BeneName = res.AccountHolder,
                            BankName = res.BankName,
                            BankID = res.BankID
                        },
                        Amount = Convert.ToInt32(res.Amount),
                        TransMode = res.TransactionMode == PaymentMode_.PAYTM_IMPS ? "IMPS" : "NEFT",
                        IsPayout = true,
                        EmailID = res.EmailID
                    };
                    IMoneyTransferAPIML moneyTransferAPIML = new FintechPayoutML(_accessor, _env, _dal, APICode.RPFINTECH);
                    APIRes = moneyTransferAPIML.AccountTransfer(tAPIRequest);
                    if (APIRes != null)
                    {
                        resp.Msg = APIRes.Msg;
                    }
                }
                else if (res.APICode == APICode.EKOPAYOUT)
                {
                    var tAPIRequest = new MTAPIRequest
                    {
                        APIID = res.APIID,
                        TransactionID = res.TransactionID,
                        TID = res.TID,
                        SenderMobile = res.OutletMobile,
                        UserName = res.AccountHolder,
                        RequestMode = bankServiceReq.RequestModeID,
                        UserID = res.UserID,
                        mBeneDetail = new MBeneDetail
                        {
                            IFSC = res.IFSC,
                            AccountNo = res.AccountNumber,
                            MobileNo = res.OutletMobile,
                            BeneName = res.AccountHolder,
                            BankName = res.BankName,
                            BankID = res.BankID
                        },
                        Amount = Convert.ToInt32(res.Amount),
                        TransMode = res.TransactionMode == PaymentMode_.PAYTM_IMPS ? "IMPS" : "NEFT",
                        IsPayout = true,
                        EmailID = res.EmailID,
                        APIOutletID = res.APIOutletID
                    };
                    IMoneyTransferAPIML moneyTransferAPIML = new EKOPayoutML(_accessor, _env, _dal);
                    APIRes = moneyTransferAPIML.AccountTransfer(tAPIRequest);
                }
                else if (res.APICode == APICode.SECUREPAYMENT)
                {
                    var tAPIRequest = new MTAPIRequest
                    {
                        APIID = res.APIID,
                        TransactionID = res.TransactionID,
                        TID = res.TID,
                        SenderMobile = res.OutletMobile,
                        UserName = res.AccountHolder,
                        RequestMode = bankServiceReq.RequestModeID,
                        UserID = res.UserID,
                        IPAddress = _rinfo.GetRemoteIP(),
                        WebsiteName = res.WebsiteName,
                        mBeneDetail = new MBeneDetail
                        {
                            IFSC = res.IFSC,
                            AccountNo = res.AccountNumber,
                            MobileNo = res.OutletMobile,
                            BeneName = res.AccountHolder,
                            BankName = res.BankName,
                            BankID = res.BankID
                        },
                        Amount = Convert.ToInt32(res.Amount),
                        TransMode = res.TransactionMode == PaymentMode_.PAYTM_IMPS ? "IMPS" : "NEFT",
                        IsPayout = true,
                        EmailID = res.EmailID
                    };
                    IMoneyTransferAPIML moneyTransferAPIML = new SecurePaymentML(_accessor, _env, _dal, APICode.SECUREPAYMENT);
                    APIRes = moneyTransferAPIML.AccountTransfer(tAPIRequest);
                    if (APIRes != null)
                    {
                        resp.Msg = APIRes.Msg;
                    }
                }
                else if (res.APICode.Equals(APICode.OPENBANK))
                {
                    var tAPIRequest = new MTAPIRequest
                    {
                        APIID = res.APIID,
                        TransactionID = res.TransactionID,
                        TID = res.TID,
                        SenderMobile = res.OutletMobile,
                        UserName = res.AccountHolder,
                        RequestMode = bankServiceReq.RequestModeID,
                        UserID = res.UserID,
                        IPAddress = _rinfo.GetRemoteIP(),
                        WebsiteName = res.WebsiteName,
                        mBeneDetail = new MBeneDetail
                        {
                            IFSC = res.IFSC,
                            AccountNo = res.AccountNumber,
                            MobileNo = res.OutletMobile,
                            BeneName = res.AccountHolder,
                            BankName = res.BankName,
                            BankID = res.BankID
                        },
                        Amount = Convert.ToInt32(res.Amount),
                        TransMode = res.TransactionMode == PaymentMode_.PAYTM_IMPS ? "IMPS" : "NEFT",
                        IsPayout = true,
                        EmailID = res.EmailID
                    };
                    IMoneyTransferAPIML moneyTransferAPIML = new OpenBankML(_accessor, _env);
                    APIRes = moneyTransferAPIML.AccountTransfer(tAPIRequest);
                }
                else if (res.APICode == APICode.HYPTO)
                {
                    var tAPIRequest = new MTAPIRequest
                    {
                        APIID = res.APIID,
                        TransactionID = res.TransactionID,
                        TID = res.TID,
                        SenderMobile = res.OutletMobile,
                        UserName = res.AccountHolder,
                        RequestMode = bankServiceReq.RequestModeID,
                        UserID = res.UserID,
                        mBeneDetail = new MBeneDetail
                        {
                            IFSC = res.IFSC,
                            AccountNo = res.AccountNumber,
                            MobileNo = res.OutletMobile,
                            BeneName = res.AccountHolder,
                            BankName = res.BankName,
                            BankID = res.BankID
                        },
                        Amount = Convert.ToInt32(res.Amount),
                        TransMode = Validate.O.PaymentMODEMTTM(res.TransactionMode),
                        IsPayout = true,
                        EmailID = res.EmailID,
                        APIOutletID = res.APIOutletID
                    };
                    IMoneyTransferAPIML moneyTransferAPIML = new HyptoML(_accessor, _env, tAPIRequest.APIID, _dal);
                    APIRes = moneyTransferAPIML.AccountTransfer(tAPIRequest);
                }
            }
            var IsInternalSender = true;
            if (APIRes.Statuscode > 0)
            {
                APIRes.TID = APIRes.TID == 0 ? res.TID : APIRes.TID;
                var sameUpdate = (ResponseStatus)(new ProcUpdateDMRTransaction(_dal)).Call(APIRes);
                if (sameUpdate.Statuscode == ErrorCodes.Minus1)
                {
                    IsInternalSender = false;
                }
            }
            if (APIRes.Statuscode == RechargeRespType.SUCCESS && IsInternalSender)
            {
                //Only for Internal Sender
                new AlertML(_accessor, _env, false).PayoutSMS(new AlertReplacementModel
                {
                    LoginID = res.UserID,
                    UserMobileNo = res.OutletMobile,
                    WID = 1,
                    Amount = Convert.ToInt32(res.Amount),
                    AccountNo = res.AccountNumber ?? string.Empty,
                    SenderName = res.AccountHolder ?? string.Empty,
                    TransMode = res.TransactionMode == PaymentMode_.PAYTM_IMPS ? "IMPS" : "NEFT",
                    UTRorRRN = APIRes.LiveID ?? string.Empty,
                    IFSC = res.IFSC ?? string.Empty,
                    BrandName = res.BrandName
                });
            }
            return resp;
        }
        public async Task<IResponseStatus> MoveToWalletApp(CommonReq commonReq)
        {
            var _resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            IProcedure proc = new ProcMoveToWalletActive(_dal);
            _resp = (ResponseStatus)proc.Call(commonReq);
            if (_resp.Statuscode == ErrorCodes.One && _resp.CommonInt > 0 && _resp.CommonBool)
            {
                var bankServiceReq = new BankServiceReq
                {
                    LoginID = commonReq.LoginID,
                    WalletRequestID = _resp.CommonInt,
                    RequestModeID = RequestMode.APPS
                };
                _resp = await CallBankTransfer(bankServiceReq).ConfigureAwait(false);
            }
            return _resp;
        }
        public IResponseStatus BankTransfer(WalletRequest req)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            req.LoginID = _lr.UserID;
            req.LoginTypeID = _lr.LoginTypeID;

            DataTable dt = new DataTable();
            dt.Columns.Add("_ID");
            dt.Columns.Add("_BankRRN");
            foreach (var item in req.PayIds)
            {
                DataRow dr = dt.NewRow();
                dr["_ID"] = item.ID;
                dr["_BankRRN"] = item.BankRRN;
                dt.Rows.Add(dr);
            }
            req.dt = dt;
            IProcedure proc = new ProcMoveToBank(_dal);
            return (ResponseStatus)proc.Call(req);
        }
        public IEnumerable<TransactionMode> GetTransactionMode()
        {
            CommonReq commonReq = new CommonReq();
            commonReq.LoginID = _lr.UserID;
            commonReq.LoginTypeID = _lr.LoginTypeID;

            IProcedure proc = new ProcGetTransactionMode(_dal);
            return (List<TransactionMode>)proc.Call(commonReq);

        }
        public TransactionMode GetTransactionMode(CommonReq commonReq)
        {
            commonReq.LoginID = _lr.UserID;
            commonReq.LoginTypeID = _lr.LoginTypeID;
            IProcedure proc = new ProcGetTransactionMode(_dal);
            return (TransactionMode)proc.Call(commonReq);

        }
        public IEnumerable<TransactionMode> GetTransactionModes(CommonReq commonReq)
        {
            IProcedure proc = new ProcGetTransactionMode(_dal);
            return (List<TransactionMode>)proc.Call(commonReq);
        }
        public IResponseStatus GenerateOrderForUPI(GenerateOrderUPIRequest orderUPIRequest)
        {
            orderUPIRequest.RequestIP = _rinfo.GetRemoteIP();
            orderUPIRequest.Browser = _rinfo.GetBrowserFullInfo();
            IProcedure proc = new ProcGenerateOrderForUPI(_dal);
            var res = (ResponseStatus)proc.Call(orderUPIRequest);
            if (res.Statuscode == ErrorCodes.One)
            {
                var sendSMSML = new SendSMSML(_dal);
                sendSMSML.SendNotification(res.CommonStr2, res.CommonStr);
            }
            return res;
        }
        #endregion
        #region FundRequestToRole
        public List<FundRequestToRole> GetFundRequestToRole()
        {
            var res = new List<FundRequestToRole>();
            if (_lr.LoginTypeID == LoginType.ApplicationUser)
            {
                var req = new CommonReq
                {
                    LoginID = _lr.UserID,
                    LoginTypeID = _lr.LoginTypeID
                };
                IProcedure proc = new ProcGetFundRequestToRole(_dal);
                var list = (List<FundRequestToRole>)proc.Call(req);
                if (list.Count > 0)
                {
                    var _res = list.Select(s => new { s.FromId, s.FromRole, s.IsUpline }).Distinct().ToList();
                    foreach (var i in _res)
                    {
                        var obj = new FundRequestToRole
                        {
                            FromId = i.FromId,
                            FromRole = i.FromRole,
                            ToRoles = list.Where(w => w.FromId == i.FromId).ToList(),
                            IsUpline = i.IsUpline,
                        };
                        res.Add(obj);
                    }
                }
            }
            return res;
        }

        public IResponseStatus UpdateFundRequestToRole(FundRequestToRole fundRequest)
        {
            IResponseStatus _res = new ResponseStatus()
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (_lr.LoginTypeID == LoginType.ApplicationUser && _lr.RoleID == Role.Admin)
            {
                fundRequest.LoginID = _lr.UserID;
                fundRequest.LoginTypeID = _lr.LoginTypeID;
                IProcedure _proc = new ProcUpdateFundRequestToRole(_dal);
                _res = (IResponseStatus)_proc.Call(fundRequest);
            }
            return _res;
        }
        #endregion
        public IResponseStatus UpdateTransactionMode(CommonReq commonReq)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            commonReq.LoginID = _lr.UserID;
            commonReq.LoginTypeID = _lr.LoginTypeID;
            IProcedure proc = new ProcUpdateTransactionMode(_dal);
            res = (ResponseStatus)proc.Call(commonReq);
            return res;
        }

        public IResponseStatus Regeneratepassword(int UserID)
        {
            IResponseStatus _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            var regeneratePassword = new RegeneratePassword
            {
                OldPassword = "0",
                SessID = 0,
                NewPassword = ApplicationSetting.IsPasswordNumeric ? HashEncryption.O.CreatePasswordNumeric(8) : HashEncryption.O.CreatePassword(8),
                Pin = HashEncryption.O.CreatePasswordNumeric(4),
                UserID = UserID
            };
            if ((regeneratePassword.NewPassword ?? "").Length < 8 || (regeneratePassword.NewPassword ?? "").Length > 12)
            {
                _res.Msg = "New Password length should be 8 to 12 with AlphaNumeric";
                return _res;
            }
            var req = new LoginReq
            {
                CommonStr = regeneratePassword.NewPassword,
                CommonStr2 = regeneratePassword.OldPassword,
                CommonStr3 = regeneratePassword.Pin,

                LoginID = _lr.UserID,
                CommonInt2 = regeneratePassword.UserID,
                LoginTypeID = _lr.LoginTypeID,
                Browser = _rinfo.GetBrowser(),
                RequestIP = _rinfo.GetRemoteIP(),
                RequestMode = RequestMode.PANEL,
                CommonInt = regeneratePassword.SessID
            };
            IProcedure proc = new ProcChangePassword(_dal);
            var resp = (AlertReplacementModel)proc.Call(req);
            _res.Statuscode = resp.Statuscode;
            _res.Msg = resp.Msg;
            if (_res.Statuscode == ErrorCodes.One)
            {
                IAlertML alertMl = new AlertML(_accessor, _env);
                alertMl.ForgetPasswordSMS(resp);
                alertMl.ForgetPasswordEmail(resp);
            }
            _res.CommonStr = string.Empty;
            _res.CommonStr2 = string.Empty;
            _res.CommonStr3 = string.Empty;
            _res.CommonStr4 = string.Empty;
            return _res;
        }
        #region Callme
        public IEnumerable<UserCallMeModel> GetCallMeRequests(int t)
        {
            ProcCallMeUserRequest proc = new ProcCallMeUserRequest(_dal);
            return proc.GetCallMeRequest(t);
        }

        public ResponseStatus UpdateCallMeHistory(UserCallMeModel data)
        {
            var req = new CommonReq()
            {
                LoginTypeID = _lr.LoginTypeID,
                LoginID = _lr.UserID,
                CommonStr = data.CallHistory,
                CommonInt = data.ID,
                CommonInt2 = data.StatusID
            };
            IProcedure proc = new ProcUpdateCallMeHistory(_dal);
            var res = (AlertReplacementModel)proc.Call(req);
            var resp = new ResponseStatus
            {
                Statuscode = res.Statuscode,
                Msg = res.Msg
            };
            if (res.Statuscode == ErrorCodes.One && data.StatusID == 3)
            {
                IAlertML ml = new AlertML(_accessor, _env);
                Parallel.Invoke(() => ml.CallNotPickedSMS(res),
                () => ml.CallNotPickedEmail(res),
                () => ml.CallNotPickedNotification(res),
                async () => await ml.WebNotification(res).ConfigureAwait(false));
            }
            return resp;
        }



        public int CallMeRequestCount()
        {
            ProcCallMeUserRequest proc = new ProcCallMeUserRequest(_dal);
            int count = 0;
            count = proc.CallMeRequestCount();
            return count;
        }
        #endregion
        public UserRegModel GetRoleSignUp()
        {
            var _res = new UserRegModel
            {
                roleSlab = new UserRoleSlab()
            };
            IProcedure proc = new ProcGetRoleSignUp(_dal);
            _res.roleSlab.Roles = (List<RoleMaster>)proc.Call();
            return _res;
        }
        public ResponseStatus GetKYCStatus(int UID)
        {
            var doc = new CommonReq
            {
                LoginID = _lr.UserID,
                CommonInt = UID,
            };
            IProcedure _proc = new ProcGetKycStatus(_dal);
            var _res = (AlertReplacementModel)_proc.Call(doc);

            if (_res.KYCStatus.In(3, 4, 5))
            {
                bool IsApproved = false;
                if (_res.KYCStatus == 3)
                {
                    IsApproved = true;
                }
                if (_res.KYCStatus == 4 || _res.KYCStatus == 5)
                {
                    IsApproved = false;
                }
                IAlertML alertMl = new AlertML(_accessor, _env);
                _res.FormatID = IsApproved ? MessageFormat.KYCApproved : MessageFormat.KYCReject;
                _res.NotificationTitle = IsApproved ? nameof(MessageFormat.KYCApproved) : nameof(MessageFormat.KYCReject);
                Parallel.Invoke(() => alertMl.KYCApprovalSMS(_res, IsApproved),
                    () => alertMl.KYCApprovalEmail(_res, IsApproved),
                    () => alertMl.KYCApprovalNotification(_res, IsApproved),
                    () => alertMl.SocialAlert(_res),
                    async () => await alertMl.WebNotification(_res).ConfigureAwait(false));
            }
            return new ResponseStatus();
        }
        public IResponseStatus UpdateInvoiceByAdmin(int UserID, bool Is)
        {
            IProcedure proc = new ProcUpdateInvoiceByAdminStatus(_dal);
            return (ResponseStatus)proc.Call(new CommonReq
            {
                LoginTypeID = _lr.LoginTypeID,
                LoginID = _lr.UserID,
                CommonInt = UserID,
                CommonBool = Is
            });
        }
        public IResponseStatus UpdateMarkRG(int UserID, bool Is)
        {
            IProcedure proc = new ProcUpdateMarkRGAdminStatus(_dal);
            return (ResponseStatus)proc.Call(new CommonReq
            {
                LoginTypeID = _lr.LoginTypeID,
                LoginID = _lr.UserID,
                CommonInt = UserID,
                CommonBool = Is
            });
        }
        public IResponseStatus UserBankRequestApp(GetEditUser _req)
        {
            var _resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (_req.LT == LoginType.ApplicationUser)
            {
                _req.IP = _rinfo.GetRemoteIP();
                _req.Browser = _rinfo.GetBrowser();
                IProcedure proc = new ProcUserBankRequest(_dal);
                _resp = (ResponseStatus)proc.Call(_req);
            }
            return _resp;

        }
        #region Customercare

        public GetinTouctListModel GetUserSubcription(CommonReq _req)
        {
            var res = new GetinTouctListModel();
            if (_lr.RoleID > 0)
            {
                IProcedure proc = new ProcGetUserSubscription(_dal);
                res = (GetinTouctListModel)proc.Call(_req);
            }
            return res;
        }

        public IResponseStatus AssignuserSubscription(CommonReq _req)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            if ((_lr.LoginTypeID == LoginType.ApplicationUser))
            {
                IProcedure _proc = new ProcAssignUserSubscription(_dal);
                _res = (ResponseStatus)_proc.Call(_req);
            }
            return _res;
        }
        public GetinTouctListModel ProcGetUserSubscriptionCusCare(CommonReq _req)
        {
            var res = new GetinTouctListModel();
            if (_lr.RoleID > 0)
            {
                IProcedure proc = new ProcGetUserSubscriptionCusCare(_dal);
                res = (GetinTouctListModel)proc.Call(_req);
            }
            return res;
        }
        public IResponseStatus UpdateuserSubscriptionStatusCuscare(CommonReq _req)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            if ((_lr.LoginTypeID == LoginType.CustomerCare))
            {
                IProcedure _proc = new ProcUpdateUserSubscriptionStatusCusCare(_dal);
                _res = (ResponseStatus)_proc.Call(_req);
            }
            return _res;
        }

        public IResponseStatus UpdateuserSubscriptionRemarksCuscare(CommonReq _req)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            if ((_lr.LoginTypeID == LoginType.CustomerCare))
            {
                IProcedure _proc = new ProcUpdateCustomerRemarks(_dal);
                _res = (ResponseStatus)_proc.Call(_req);
            }
            return _res;
        }
        public IResponseStatus RemoveUserSubscription(CommonReq _req)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            if ((_lr.LoginTypeID == LoginType.ApplicationUser))
            {
                IProcedure _proc = new ProcRemoveUserSubscription(_dal);
                _res = (ResponseStatus)_proc.Call(_req);
            }
            return _res;
        }
        #endregion
        public List<MasterRole> GetMasterRole()
        {
            var req = new CommonReq
            {
                CommonInt2 = _lr.UserID,
                CommonInt = _lr.LoginTypeID,
            };
            IProcedure proc = new ProcGetMasterRole(_dal);
            var res = (List<MasterRole>)proc.Call(req);
            return res;
        }

        public ResponseStatus UpdateMasterRoel(int Id, int RegCharge)
        {
            var req = new CommonReq
            {
                CommonInt = Id,
                CommonInt2 = RegCharge,
                CommonInt3 = _lr.LoginTypeID,
                CommonInt4 = _lr.UserID
            };


            IProcedure proc = new ProcUpdateMasterRole(_dal);
            var res = (ResponseStatus)proc.Call(req);
            return res;
        }
        public bool IsEMailVerified(int UserID = 0)
        {
            var UID = _lr != null ? _lr.UserID : UserID;
            IProcedure proc = new ProcIsEmailVerified(_dal);
            var res = (bool)proc.Call(UID);
            return res;
        }
        public bool SendVerifyEmailLink(LoginResponse _Applr = null)
        {
            var req = _lr != null ? _lr : _Applr;
            var activationlink = GenrateVerifiyEmailLink(1, req.UserID);
            var emailBody = new StringBuilder();
            emailBody.AppendFormat(ErrorCodes.EmailVerifyMsg, activationlink);
            IEmailML emailManager = new EmailML(_dal);
            var res = emailManager.SendMail(req.EmailID, null, ErrorCodes.EmailVerify, emailBody.ToString(), req.WID, _resourceML.GetLogoURL(req.WID).ToString());
            return res;
        }

        public string GenrateVerifiyEmailLink(int LT, int UserID)
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
            sb.Append("/VerifyEmail?encdata=");
            sb.Append(HashEncryption.O.ConvertStringToHex(salt));
            return sb.ToString();
        }


        public UserList GetFOSList(CommonReq _filter)
        {
            var _resp = new UserList();
            var _req = new UserRequest
            {
                LoginID = _filter.LoginID,
                LTID = _filter.LoginTypeID,
                Browser = _rinfo.GetBrowserFullInfo(),
                IP = _rinfo.GetRemoteIP(),
                RoleID = _filter.CommonInt2,

            };
            if (_filter.LoginTypeID == LoginType.ApplicationUser)
            {

                IProcedure proc = new ProcUserListFOS(_dal);
                _resp = (UserList)proc.Call(_req);
                if (_filter.LoginTypeID == LoginType.ApplicationUser)
                {
                    _resp.CanFundTransfer = _filter.LoginTypeID == LoginType.ApplicationUser;
                }
            }
            return _resp;
        }
        #region Pin Code Area

        public IEnumerable<PincodeDetail> GetPincodeArea(CommonReq req)
        {
            IProcedure proc = new ProcGetPincodearea(_dal);
            return (List<PincodeDetail>)proc.Call(req);
        }
        public IResponseStatus SaveHour(CommonReq _req)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (_lr.LoginTypeID == LoginType.ApplicationUser && _lr.RoleID == Role.Admin)
            {
                IProcedure proc = new procUpdatePincodeAreaHour(_dal);
                return (IResponseStatus)proc.Call(_req);
            }
            return res;
        }
        #endregion
        public ResponseStatus EMailVerify(int userID)
        {
            IProcedure proc = new ProcEmailVerify(_dal);
            var res = (ResponseStatus)proc.Call(userID);
            return res;
        }
        public DTHSubscriptionReport GetBookingStatus(int ID, string TransactionID)
        {
            if (ID > 0)
            {
                IProcedure _proc = new ProcGetBookingStatus(_dal);
                return (DTHSubscriptionReport)_proc.Call(new CommonReq { CommonInt = ID });
            }
            return new DTHSubscriptionReport();
        }
        public async Task<LeadSummary> GetLeadSummary(int CustomerID)
        {

            var _res = new LeadSummary();
            if (_lr.LoginTypeID == LoginType.ApplicationUser)
            {
                var procReq = new CommonReq
                {
                    LoginID = _lr.LoginTypeID == LoginType.CustomerCare ? 1 : _lr.UserID,
                    CommonInt = CustomerID
                };
                IProcedureAsync _proc = new ProcLeadSummary(_dal);
                _res = (LeadSummary)await _proc.Call(procReq);
            }
            if (_lr.LoginTypeID == LoginType.CustomerCare)
            {
                var procReq = new CommonReq
                {
                    LoginID = _lr.LoginTypeID == LoginType.CustomerCare ? 1 : _lr.UserID,
                    CommonInt = _lr.UserID
                };
                IProcedureAsync _proc = new ProcLeadSummary(_dal);
                _res = (LeadSummary)await _proc.Call(procReq);
            }
            return _res;
        }
        public IEnumerable<CustomerCareDetails> GetCustomercare()
        {

            var Gcus = new List<CustomerCareDetails>();

            CommonReq commonReq = new CommonReq
            {
                LoginID = _lr.UserID,
                LoginTypeID = _lr.LoginTypeID
            };
            IProcedure proc = new ProcGetCustomercare(_dal);
            Gcus = (List<CustomerCareDetails>)proc.Call(commonReq);

            return Gcus;
        }
        public IResponseStatus DeleteWebNotification(string Ids, int Action = 2)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (_lr.LoginTypeID == LoginType.ApplicationUser && _lr.RoleID == Role.Admin)
            {
                var req = new CommonReq
                {
                    CommonInt = Action,
                    CommonStr = Ids
                };
                IProcedure _proc = new ProcDeleteWebNotification(_dal);
                res = (ResponseStatus)_proc.Call(req);
            }
            return res;
        }
        public IResponseStatus UpdateFOS(GetEditUser _req)
        {
            var _resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            }
            ;
            if (_lr.LoginTypeID == LoginType.ApplicationUser && _lr.RoleID != Role.Admin)
            {
                _req.UserID = _req.UserID == 0 || _lr.RoleID.In(Role.APIUser, Role.Customer, Role.Retailor_Seller) ? _lr.UserID : _req.UserID;
                _req.LoginID = _lr.UserID;
                _req.LT = _lr.LoginTypeID;
                _req.MobileNo = string.IsNullOrEmpty(_req.MobileNo) || _lr.RoleID.In(Role.APIUser, Role.Customer, Role.Retailor_Seller) ? _lr.MobileNo : _req.MobileNo;
            }
            if (_lr.LoginTypeID == LoginType.ApplicationUser || IsCustomerCareAuthorised(ActionCodes.EditUser))
            {
                if (Validate.O.IsNumeric(_req.Name ?? "") || (_req.Name ?? "").Length > 100)
                {
                    _resp.Msg = ErrorCodes.InvalidParam + " Name";
                    return _resp;
                }
                if (Validate.O.IsNumeric(_req.OutletName ?? "") || (_req.OutletName ?? "").Length > 100)
                {
                    _resp.Msg = ErrorCodes.InvalidParam + " Outlet Name";
                    return _resp;
                }
                if (!Validate.O.IsMobile(_req.MobileNo ?? ""))
                {
                    _resp.Msg = ErrorCodes.InvalidParam + " Mobile Number";
                    return _resp;
                }
                if (!(_req.EmailID ?? "").Contains("@"))
                {
                    _resp.Msg = ErrorCodes.InvalidParam + " EmailID";
                    return _resp;
                }
                if (!Validate.O.IsPinCode(_req.Pincode ?? ""))
                {
                    _resp.Msg = ErrorCodes.InvalidParam + " Pincode";
                    return _resp;
                }
                if ((_req.Address ?? "").Length > 300)
                {
                    _resp.Msg = ErrorCodes.InvalidParam + " Address";
                    return _resp;
                }
                if ((_req.City ?? "").Length > 50)
                {
                    _resp.Msg = ErrorCodes.InvalidParam + " City";
                    return _resp;
                }

                if (_req.IsWebsite && !(_req.WebsiteName ?? "").Contains("."))
                {
                    _resp.Msg = ErrorCodes.InvalidParam + "Domain Name";
                    return _resp;
                }


                _req.LoginID = _lr.UserID;
                _req.LT = _lr.LoginTypeID;
                _req.IP = _rinfo.GetRemoteIP();
                _req.Browser = _rinfo.GetBrowser();
                IProcedure proc = new ProcUpdateFOS(_dal);
                _resp = (ResponseStatus)proc.Call(_req);
                _req.WID = _resp.CommonInt;
            }
            return _resp;
        }
        public IResponseStatus RegisterasVendor(int UserID)
        {
            var req = new CommonReq
            {
                LoginID = _lr.UserID,
                LoginTypeID = _lr.LoginTypeID,
                CommonInt = UserID
            };
            IProcedure _proc = new ProcRegisterAsVendor(_dal);
            var res = (ResponseStatus)_proc.Call(req);
            return res;
        }

        public async Task<ResponseStatus> IsVendor(int UserID = 0)
        {
            int LoginID = UserID > 0 ? UserID : _lr.UserID;

            IProcedureAsync _proc = new ProcIsVendor(_dal);

            var res = (ResponseStatus)await _proc.Call(LoginID).ConfigureAwait(false);
            return res;
        }
        private LoginResponse chkAlternateSession()
        {
            var result = new LoginResponse();
            if (_lr != null)
            {
                result = _lr;
            }
            if (_lrEmp != null)
            {
                result = _lrEmp;
            }
            return result;
        }
        public AutoBillingModel GetAutoBillingDetail(int id)
        {
            var res = new AutoBillingModel();
            IProcedure _proc = new ProcAutoBilling(_dal);
            res = (AutoBillingModel)_proc.Call(id);
            return res;
        }

        public async Task<List<AlertReplacementModel>> GetAutoBillingProcess(int userId)
        {
            var res = new List<AlertReplacementModel>();
            if (ApplicationSetting.IsAutoBilling)
            {
                IProcedureAsync _proc = new procAutoBillingProcess(_dal);
                res = (List<AlertReplacementModel>)await _proc.Call(userId);
                foreach (var row in res)
                {
                    if (row.Statuscode == ErrorCodes.One && row.TID > 0)
                    {
                        try
                        {
                            var ml = new AlertML(_accessor, _env);
                            #region To
                            row.FormatID = MessageFormat.FundReceive;
                            row.NotificationTitle = "Fund Receive";
                            Parallel.Invoke(() => ml.FundReceiveSMS(row),
                                () => ml.FundReceiveEmail(row),
                                () => ml.FundReceiveNotification(row),
                                () => ml.WebNotification(row),
                                () => ml.SocialAlert(row));
                            #endregion

                            #region From
                            row.FormatID = MessageFormat.FundTransfer;
                            row.NotificationTitle = "Fund Transfer";
                            row.WhatsappNo = row.WhatsappNoL;
                            row.TelegramNo = row.TelegramNoL;
                            row.HangoutNo = row.HangoutNoL;

                            Parallel.Invoke(() => ml.FundTransferSMS(row),
                                () => ml.FundTransferEmail(row),
                                () => ml.FundTransferNotification(row),
                                () => ml.WebNotification(row),
                                () => ml.SocialAlert(row));
                            #endregion
                        }
                        catch (Exception ex)
                        {
                            var errorLog = new ErrorLog
                            {
                                ClassName = GetType().Name,
                                FuncName = "FundTransfer",
                                Error = ex.Message,
                                LoginTypeID = _lr.LoginTypeID,
                                UserId = _lr.UserID
                            };
                            var _ = new ProcPageErrorLog(_dal).Call(errorLog);
                        }
                    }
                }
            }
            return res;
        }
        public IResponseStatus UpdateAutoBilling(AutoBillingModel req)
        {
            IResponseStatus res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.NORESPONSE
            };
            req = req != null ? req : new AutoBillingModel();
            if (req.LT == LoginType.ApplicationUser)
            {
                req.Browser = _rinfo.GetBrowserFullInfo();
                req.IP = _rinfo.GetRemoteIP();
                IProcedure _proc = new ProcAutoBillingUpdate(_dal);
                res = (ResponseStatus)_proc.Call(req);
            }
            return res;
        }
        public ResponseStatus UploadInvoice(IFormFile file, string InvoiceMonth, bool IsRCM)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };

            if (!_lr.RoleID.In(Role.APIUser))
            {
                _res.Msg = "Invalid Access";
                return _res;
            }
            if (string.IsNullOrEmpty(InvoiceMonth))
            {
                _res.Msg = "Invalid Invoice month";
                return _res;
            }
            if (!file.ContentType.Any())
            {
                _res.Msg = "File not found!";
                return _res;
            }
            if (file.Length < 1)
            {
                _res.Msg = "Empty file not allowed!";
                return _res;
            }
            if (file.Length / 1024 > 200)
            {
                _res.Msg = "File size exceeded! Not more than 200 KB is allowed";
                return _res;
            }
            var filename = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
            string ext = Path.GetExtension(filename);
            byte[] filecontent = null;
            using (var ms = new MemoryStream())
            {
                file.CopyTo(ms);
                filecontent = ms.ToArray();
            }
            if (!Validate.O.IsFileAllowed(filecontent, ext))
            {
                _res.Msg = "Invalid File Format!";
                return _res;
            }
            string dbFileName = (IsRCM ? "RCM_" : "P2A_") + _lr.UserID + "_" + InvoiceMonth.Replace(" ", "_");
            bool IsFileUploaded = true;
            try
            {
                string[] Files = Directory.GetFiles(DOCType.InvoiceFilePath);
                foreach (string f in Files)
                {
                    if (f.ToUpper().StartsWith(dbFileName.ToUpper()))
                    {
                        File.Delete(f);
                    }
                }
                string savefilename = DOCType.InvoiceFilePath + dbFileName + ext;
                using (FileStream fs = File.Create(savefilename))
                {
                    file.CopyTo(fs);
                    fs.Flush();
                }
            }
            catch (Exception ex)
            {
                IsFileUploaded = false;
                _res.Msg = "File can not be uploaded! Try again later..";
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "UploadInvoice",
                    Error = ex.Message,
                    LoginTypeID = _lr.LoginTypeID,
                    UserId = _lr.UserID
                });
            }
            if (IsFileUploaded)
            {
                DAL __dal = new DAL(_c.GetConnectionString(1));
                ProcUploadP2AInvoice _proc = new ProcUploadP2AInvoice(__dal);
                _res = (ResponseStatus)_proc.Call(new CommonReq
                {
                    LoginID = _lr.UserID,
                    CommonStr = InvoiceMonth,
                    CommonStr2 = dbFileName
                });
            }
            return _res;
        }
        public IEnumerable<DocTypeMaster> GetUserDocumentsList(int UserID)
        {
            var list = new List<DocTypeMaster>();
            if (_lr.LoginTypeID == LoginType.ApplicationUser)
            {
                var req = new DocTypeMaster()
                {
                    LoginTypeID = _lr.LoginTypeID,
                    LoginId = _lr.UserID,
                    UserId = UserID
                };
                IProcedure _proc = new ProcGetUserDocumentsList(_dal);
                list = (List<DocTypeMaster>)_proc.Call(req);
            }
            return list;
        }
        #region GetAllUplines
        public IEnumerable<UserInfo> GetAllUplines(int UserID)
        {
            var res = new List<UserInfo>();

            var req = new CommonReq
            {
                LoginTypeID = _lr.LoginTypeID,
                LoginID = _lr.UserID,
                UserID = UserID,
                CommonInt = _lr.RoleID
            };
            IProcedure _proc = new ProcGetAllUpLines(_dal);
            res = (List<UserInfo>)_proc.Call(req);
            return res;
        }
        #endregion
        #region MTWRSetting
        public SettlementSetting GetSettlementSetting(int uid)
        {
            var res = new SettlementSetting();
            var commonReq = new CommonReq
            {
                LoginTypeID = _lr.LoginTypeID,
                LoginID = _lr.UserID,
                CommonInt = uid
            };
            IProcedure _proc = new ProcGetSettlementSetting(_dal);
            res = (SettlementSetting)_proc.Call(commonReq);
            return res;
        }

        public ResponseStatus GetSettlementSetting(UserwiseSettSetting userwiseSettSetting)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.NORESPONSE
            };
            var commReq = new CommonReq
            {
                LoginTypeID = _lr.LoginTypeID,
                LoginID = _lr.UserID,
                UserID = userwiseSettSetting.UserID,
                CommonInt = userwiseSettSetting.MTRWSettleType,
                CommonInt2 = userwiseSettSetting.MTRWSettleTypeMB,
                CommonBool = userwiseSettSetting.IsOWSettleAsBank
            };
            IProcedure _proc = new ProcUpdateSettlementSetting(_dal);
            res = (ResponseStatus)_proc.Call(commReq);
            return res;
        }
        #endregion

        public SettlementSetting GetSettlementSettingSeller()
        {
            var res = new SettlementSetting();
            var commonReq = new CommonReq
            {
                LoginTypeID = _lr.LoginTypeID,
                LoginID = 1,
                CommonInt = _lr.UserID
            };
            IProcedure _proc = new ProcGetSettlementSetting(_dal);
            res = (SettlementSetting)_proc.Call(commonReq);
            return res;
        }
        public IEnumerable<MasterRejectReason> MASTERRR()
        {
            List<MasterRejectReason> lst = new List<MasterRejectReason>();
            if (_lr.LoginTypeID == LoginType.ApplicationUser)
            {
                MasterVendorModel req = new MasterVendorModel
                {
                    LoginID = _lr.UserID,
                    LoginTypeID = _lr.LoginTypeID
                };
                IProcedure _proc = new ProcGetRejectReason(_dal);
                lst = (List<MasterRejectReason>)_proc.Call(req);
                MasterRejectReason defaultItem = new MasterRejectReason
                {
                    ID = 0,
                    Reason = "Select Reason"
                };
                lst.Insert(0, defaultItem);
            }
            return lst;
        }

        public IEnumerable<BonafideAccount> GetBonafideAccount(CommonReq _req)
        {
            var res = new List<BonafideAccount>();
            if (_lr.RoleID > 0)
            {
                IProcedure proc = new ProcBonafideAccount(_dal);
                res = (List<BonafideAccount>)proc.Call(_req);
            }
            return res;
        }
        public IResponseStatus BonafideAccountSetting(int AccountID, bool IsDelete)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            if ((_lr.LoginTypeID == LoginType.ApplicationUser))
            {
                var _req = new BonafideAccount
                {
                    LoginID = _lr.UserID,
                    LTID = _lr.LoginTypeID,
                    ID = AccountID,
                    IsDelete = IsDelete
                };
                IProcedure _proc = new ProcBonafideAccountSetting(_dal);
                _res = (ResponseStatus)_proc.Call(_req);
            }
            return _res;
        }

        public IResponseStatus UpdatCalCommCir(int UserID, bool Is)
        {
            IProcedure proc = new ProcUpdateCalCommCircle(_dal);
            return (ResponseStatus)proc.Call(new CommonReq
            {
                LoginTypeID = _lr.LoginTypeID,
                LoginID = _lr.UserID,
                CommonInt = UserID,
                CommonBool = Is
            });
        }
        public ResponseStatus ChangeAPISwSts(int Id)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            var req = new CommonReq
            {
                LoginTypeID = _lr.LoginTypeID,
                LoginID = _lr.UserID,
                CommonInt = Id
            };

            if (_lr.LoginTypeID == LoginType.ApplicationUser && _lr.RoleID.In(Role.Admin))
            {
                IProcedure _proc = new ProcChangeAPIInSwitchSts(_dal);
                res = (ResponseStatus)_proc.Call(req);
            }
            return res;
        }
        #region MNPKendra
        public MNPStsResp GetMNPStatus(CommonReq commonReq)
        {
            IProcedure proc = new ProcGetMNPStatus(_dal);
            MNPStsResp res = (MNPStsResp)proc.Call(commonReq);
            return res;
        }
        public MNPStsResp UserMNPRegistration(MNPRegistration mNPRegistration)
        {
            IProcedure proc = new ProcMNPUserRegistration(_dal);
            MNPStsResp res = (MNPStsResp)proc.Call(mNPRegistration);
            if (res.Statuscode == ErrorCodes.One)
            {
                res.MNPStatus = MNPStatus.PENDING;
                res.MNPRemark = MNPStatus._PENDINGMsg;
            }
            return res;
        }
        public ResponseStatus UserMNPRequest(MNPClaimReq mNPClaimReq)
        {
            IProcedure proc = new ProcUserMNPClaim(_dal);
            return (ResponseStatus)proc.Call(mNPClaimReq);
        }
        public List<MNPClaims> GetUserClaimsReport(MNPClaimDataReq mNPClaimDataReq)
        {
            IProcedure proc = new ProcGetUserClaims(_dal);
            return (List<MNPClaims>)proc.Call(mNPClaimDataReq);
        }
        #endregion
        #region WalletToWallet
        public WTWUserInfo GetUserByMobile(CommonReq commonReq)
        {
            WTWUserInfo userData = new WTWUserInfo();
            if (!Validate.O.IsMobile(commonReq.CommonStr) || string.IsNullOrEmpty(commonReq.CommonStr))
            {
                userData.StatusCode = ErrorCodes.Minus1;
                userData.Msg = "Wrong Mobile No!";
                return userData;
            }
            IProcedure proc = new ProcGetUserByMobile(_dal);
            userData = (WTWUserInfo)proc.Call(commonReq);
            return userData;
        }

        public ResponseStatus WTWFT(CommonReq commonReq)
        {
            var _resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            IProcedure _proc = new ProcWalletToWalletFT(_dal);
            _resp = (ResponseStatus)_proc.Call(commonReq);
            return _resp;
        }
        #endregion
        public ResponseStatus PGStsChange(int UserID)
        {
            IProcedure proc = new ProcUpdatePGStatus(_dal);
            return (ResponseStatus)proc.Call(new CommonReq
            {
                LoginTypeID = _lr.LoginTypeID,
                LoginID = _lr.UserID,
                UserID = UserID
            });
        }
        public ResponseStatus PGDownLineStsChange(int UserID)
        {
            IProcedure proc = new ProcUpdatePGDownLineStatus(_dal);
            return (ResponseStatus)proc.Call(new CommonReq
            {
                LoginTypeID = _lr.LoginTypeID,
                LoginID = _lr.UserID,
                UserID = UserID
            });
        }

        #region MNP

        public MNPUserResp GetMNPUser(int UserID, string s = "")
        {
            var _resp = new MNPUserResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            var _req = new MNPUser { ID = 0, UserID = UserID, VerifyStatus = 0 };
            IProcedure _p = new ProcGetMNPUser(_dal);
            _resp.list = (List<MNPUser>)_p.Call(_req);

            return _resp;
        }

        public MNPDetailsResp GetMNPUserByID(MNPUser req)
        {
            var _resp = new MNPDetailsResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            IProcedure _p = new ProcGetMNPUser(_dal);
            _resp.data = (MNPUser)_p.Call(req);
            if (_resp.data != null)
            {
                _resp.Statuscode = 1;
                _resp.Msg = "Record found";
            }
            return _resp;
        }


        public IResponseStatus UpdateMNPStatus(int Status, int ID, string UserName, string Password, string Remark, string Demo)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            if ((_lr.LoginTypeID == LoginType.ApplicationUser))
            {
                var _req = new CommonReq
                {
                    LoginID = _lr.UserID,
                    CommonInt = Status,
                    CommonStr = UserName,
                    CommonStr2 = Password,
                    CommonStr3 = Remark,
                    CommonStr4 = Demo,
                    UserID = ID,

                };
                IProcedure _proc = new ProcUpdateMNPStatus(_dal);
                _res = (ResponseStatus)_proc.Call(_req);
            }
            return _res;
        }


        public MNPUserResp GetMNPCliam(int UserID, string s = "")
        {
            var _resp = new MNPUserResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            var _req = new MNPUser { ID = 0, UserID = UserID, VerifyStatus = 0 };
            IProcedure _p = new ProcGetMNPClaim(_dal);
            _resp.list = (List<MNPUser>)_p.Call(_req);

            return _resp;
        }

        public MNPDetailsResp GetMNPClaimByID(MNPUser req)
        {
            var _resp = new MNPDetailsResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            IProcedure _p = new ProcGetMNPClaim(_dal);
            _resp.data = (MNPUser)_p.Call(req);
            if (_resp.data != null)
            {
                _resp.Statuscode = 1;
                _resp.Msg = "Record found";
            }
            return _resp;
        }


        public IResponseStatus UpdateMNPClaiimStatus(int Status, int ID, string Remark, decimal Amount, string FRCDate, string FRCDemoNo, string FRCType, string FRCDoneDate)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            if ((_lr.LoginTypeID == LoginType.ApplicationUser))
            {
                var _req = new MNPUser
                {
                    UserID = _lr.UserID,
                    VerifyStatus = Status,
                    Remark = Remark,
                    FRCDate = FRCDate,
                    FRCDemoNumber = FRCDemoNo,
                    FRCType = FRCType,
                    FRCDoneDate = FRCDoneDate,
                    ID = ID,
                    Amount = Amount,
                };
                IProcedure _proc = new ProcUpdateMNPClaimStatus(_dal);
                _res = (ResponseStatus)_proc.Call(_req);
                if (_res.CommonInt2 == ErrorCodes.Two && _res.CommonInt > 1)
                {
                    IProcedure proc = new ProcFundProcess(_dal);
                    var _ = (AlertReplacementModel)proc.Call(new FundProcessReq
                    {
                        LoginTypeID = _lr.LoginTypeID,
                        LoginID = _lr.UserID,
                        fundProcess = new FundProcess
                        {
                            UserID = _res.CommonInt,
                            Amount = Amount,
                            OType = false,
                            RequestMode = RequestMode.PANEL,
                            WalletType = 1,
                            Remark = "Fund Transfer Against MNP Claim",
                            SecurityKey = HashEncryption.O.Decrypt(_res.CommonStr)
                        },
                        CommonStr = _rinfo.GetRemoteIP(),
                        CommonStr2 = _rinfo.GetBrowser()
                    });
                }
            }
            return _res;
        }



        #endregion

        #region AccountStatement

        public UserList AppGetList(CommonFilter _filter)
        {
            var _resp = new UserList();
            var UserID = 0;

            if (ApplicationSetting.IsAccountStatement)
            {
                IProcedure procUpline = new ProcGetDirectPrentOfUser(_dal);
                var parent = (UserInfo)procUpline.Call(new CommonReq
                {
                    LoginTypeID = _filter.LT,
                    LoginID = _filter.UserID
                });
                UserID = parent.UserID;
            }

            if ((_filter.LT == LoginType.ApplicationUser && !IsEndUser()) || IsCustomerCareAuthorised(ActionCodes.ShowUser))
            {
                var validate = Validate.O;
                var _req = new UserRequest
                {
                    LoginID = UserID,
                    SortByID = false,
                    LTID = _filter.RequestMode == RequestMode.APPS ? _filter.LT : _lr.LoginTypeID,
                    Browser = _rinfo.GetBrowserFullInfo(),
                    IP = _rinfo.GetRemoteIP(),
                    RoleID = 0,
                    IsDesc = false,
                    TopRows = _filter.TopRows,
                    btnID = 1
                };
                if (_filter.Criteria > 0)
                {
                    if ((_filter.CriteriaText ?? "") == "")
                    {
                        return _resp;
                    }
                }
                if (_filter.Criteria == Criteria.OutletMobile)
                {
                    if (!validate.IsMobile(_filter.CriteriaText))
                    {
                        return _resp;
                    }
                    _req.MobileNo = _filter.CriteriaText;
                }
                if (_filter.Criteria == Criteria.EmailID)
                {
                    if (!validate.IsEmail(_filter.CriteriaText))
                    {
                        return _resp;
                    }
                    _req.EmailID = _filter.CriteriaText;
                }
                if (_filter.Criteria == Criteria.Name)
                {
                    if (validate.IsNumeric(_filter.CriteriaText) || _filter.CriteriaText.Length > 101)
                    {
                        return _resp;
                    }
                    _req.Name = _filter.CriteriaText;
                }
                if (_filter.Criteria == Criteria.UserID)
                {
                    var Prefix = Validate.O.Prefix(_filter.CriteriaText);
                    if (Validate.O.IsNumeric(Prefix))
                        _filter.UserID = Validate.O.IsNumeric(_filter.CriteriaText) ? Convert.ToInt32(_filter.CriteriaText) : _filter.UserID;
                    var uid = Validate.O.LoginID(_filter.CriteriaText);
                    _req.UserID = Validate.O.IsNumeric(uid) ? Convert.ToInt32(uid) : _filter.UserID;
                }
                IProcedure proc = new ProcUserList(_dal);
                _resp = (UserList)proc.Call(_req);
            }
            return _resp;
        }

        #endregion

        public IResponseStatus ChangeSlabStatus(int ID)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            if ((_lr.LoginTypeID == LoginType.ApplicationUser))
            {
                CommonReq commonReq = new CommonReq
                {
                    LoginID = _lr.UserID,
                    LoginTypeID = _lr.LoginTypeID,
                    CommonInt = ID
                };
                IProcedure _proc = new ProcUpdateSlabStatus(_dal);
                _res = (ResponseStatus)_proc.Call(commonReq);
            }
            return _res;
        }
        public ResponseStatus DoDthConn(DthConnect DTHconn)
        {
            var _resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            DTHconn.LoginID = _lr.UserID;
            DTHconn.LoginTypeID = _lr.LoginTypeID;

            IProcedure _proc = new ProcDTHLeadConnection(_dal);
            _resp = (ResponseStatus)_proc.Call(DTHconn);
            return _resp;
        }

        public ResponseStatus DoDthComplain(DthConnect DTHconn)
        {
            var _resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            DTHconn.LoginID = _lr.UserID;
            DTHconn.LoginTypeID = _lr.LoginTypeID;

            IProcedure _proc = new ProcDTHLeadComplain(_dal);
            _resp = (ResponseStatus)_proc.Call(DTHconn);
            return _resp;
        }


        public ResponseStatus DoDthConnApp(DTHLead DTHconn)
        {
            var _resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            DTHconn.LoginID = DTHconn.UserID;
            DTHconn.LoginTypeID = DTHconn.LoginTypeID;
            IProcedure _proc = new ProcDTHLeadConnection(_dal);
            _resp = (ResponseStatus)_proc.Call(new DthConnect
            {
                LoginTypeID = DTHconn.LoginTypeID,
                LoginID = DTHconn.UserID,
                Name = DTHconn.Name,
                OID = DTHconn.OID,
                Mobile = DTHconn.Mobile,
                Pincode = DTHconn.Pincode,
                Address = DTHconn.Address

            });
            return _resp;
        }


        public ResponseStatus DoDthComplainApp(DTHLead DTHconn)
        {
            var _resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            DTHconn.LoginID = DTHconn.UserID;
            DTHconn.LoginTypeID = DTHconn.LoginTypeID;
            IProcedure _proc = new ProcDTHLeadComplain(_dal);
            _resp = (ResponseStatus)_proc.Call(new DthConnect
            {
                LoginTypeID = DTHconn.LoginTypeID,
                LoginID = DTHconn.UserID,
                Name = DTHconn.Name,
                OID = DTHconn.OID,
                Mobile = DTHconn.Mobile,
                Remark = DTHconn.Remark,
                Pincode = DTHconn.Pincode,
                Address = DTHconn.Address

            });
            return _resp;
        }



        #region AdditionalService
        public GetAddService GetAddServiceSts(int LT, int userid, int outletID)
        {
            var _resp = new GetAddService();
            ISellerML s_ML = new SellerML(_accessor, _env);
            IProcedure proc = new ProcGetAdditionalService(_dal);
            _resp = (GetAddService)proc.Call(new CommonReq
            {
                LoginTypeID = LT,
                UserID = userid,
                CommonInt = outletID
            });
            return _resp;
        }


        public ResponseStatus ActivateAddServiceSts(int LT, int userid, int uid, int opTypeId, int oid, int outletID)

        {

            var _resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            IProcedure proc = new ProcActivateAdditionalService(_dal);
            var actServ = (ResponseStatus)proc.Call(new ActAddonSerReq
            {
                LoginTypeID = LT,
                LoginID = userid,
                OutletID = outletID,
                OpTypeID = opTypeId,
                OID = oid,
                IP = _rinfo.GetBrowserFullInfo(),
                Browser = _rinfo.GetRemoteIP(),
                UID = uid
            });
            _resp.Statuscode = actServ.Statuscode;
            _resp.Msg = actServ.Msg;
            return _resp;
        }
        #endregion


        public IEnumerable<DocTypeMaster> UsersKYCDetails(int UserID)
        {
            var list = new List<DocTypeMaster>();
            if (_lr.LoginTypeID == LoginType.ApplicationUser && UserID != _lr.UserID || IsCustomerCareAuthorised(ActionCodes.VerifyDocuments))
            {
                var req = new DocTypeMaster()
                {
                    LoginTypeID = _lr.LoginTypeID,
                    LoginId = _lr.UserID,
                    UserId = UserID
                };
                IProcedure _proc = new ProcUsersKYCDetails(_dal);
                list = (List<DocTypeMaster>)_proc.Call(req);
            }
            return list;
        }

        public string RDaddyIntAgentReg(RDaddyIntARegReq req)
        {
            var _resp = string.Empty;
            var moneyTransferAPIML = new RechargeDaddyML(_accessor, _env, _dal);
            return moneyTransferAPIML.InternalAgentRegistration(req.FirstName, req.LastName, req.MobileNo, req.OutletName, req.Address, req.City, req.PinCode, req.PAN, req.Aadhar);
        }
        public ResponseStatus RDaddyBankList(RDaddyIntARegReq req)
        {
            var _resp = string.Empty;
            var moneyTransferAPIML = new RechargeDaddyML(_accessor, _env, _dal);
            return moneyTransferAPIML.RDaddyGetBankList(req.FirstName);
        }
        #region UTR Excel or Bank reconcillation
        public IResponseStatus SendUTROTP(int userId)
        {
            IResponseStatus res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            var alertData = GetUserDeatilForAlert(userId);
            if (alertData.Statuscode == ErrorCodes.One)
            {
                IAlertML alertMl = new AlertML(_accessor, _env);
                alertData.OTP = HashEncryption.O.CreatePasswordNumeric(6);
                _session.SetString(SessionKeys.CommonOTP, alertData.OTP);
                //alertMl.OTPSMS(alertData);
                res = alertMl.OTPEmail(alertData);
                res.Msg = "Enter OTP";
            }
            return res;
        }

        public async Task<IResponseStatus> UploadUTRExcelAsync(List<UTRExcel> records)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if ((_lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser))
            {
                var _req = new UTRExcelReq
                {
                    LoginTypeID = _lr.LoginTypeID,
                    LoginID = _lr.UserID,
                    Record = records,
                    CommonStr = _rinfo.GetRemoteIP(),
                    CommonStr2 = _rinfo.GetBrowser(),
                };
                IProcedureAsync _proc = new ProcInsertUTRExcel(_dal);
                res = (ResponseStatus)await _proc.Call(_req).ConfigureAwait(false);
            }
            return res;
        }

        public async Task<IEnumerable<UTRExcelMaster>> GetUTRExcelAsync()
        {
            var res = new List<UTRExcelMaster>();
            if ((_lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser))
            {
                IProcedureAsync _proc = new ProcgetUploadedUTRFiles(_dal);
                res = (List<UTRExcelMaster>)await _proc.Call().ConfigureAwait(false);
            }
            return res;
        }

        public async Task<IEnumerable<UTRExcel>> GetUTRDetailExcelAsync(int FileId)
        {
            var res = new List<UTRExcel>();
            if ((_lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser))
            {
                var req = new CommonReq
                {
                    CommonInt = FileId
                };
                IProcedureAsync _proc = new ProcGetUTRDetailExcel(_dal);
                res = (List<UTRExcel>)await _proc.Call(req).ConfigureAwait(false);
            }
            return res;
        }
        #endregion

        #region AgreementApproval
        public AAUserData GetUserAggrement(AAUserReq aAUserReq)
        {
            var _resp = new AAUserData
            {
                StatusCode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            if ((_lr.LoginTypeID == LoginType.ApplicationUser && _lr.RoleID == Role.Admin) || IsCustomerCareAuthorised(ActionCodes.AgreementApproval))
            {
                if (aAUserReq.SearchType == Criteria.UserID)
                {
                    var Prefix = Validate.O.Prefix(aAUserReq.Search);
                    if (Validate.O.IsNumeric(Prefix))
                        aAUserReq.UserID = Validate.O.IsNumeric(aAUserReq.Search) ? Convert.ToInt32(aAUserReq.Search) : aAUserReq.UserID;
                    var uid = Validate.O.LoginID(aAUserReq.Search);
                    aAUserReq.UserID = Validate.O.IsNumeric(uid) ? Convert.ToInt32(uid) : aAUserReq.UserID;

                }
                if (aAUserReq.SearchType == Criteria.OutletMobile)
                {
                    if (!Validate.O.IsMobile(aAUserReq.Search))
                    {
                        return _resp;
                    }
                    aAUserReq.MobileNo = aAUserReq.Search;
                }
                aAUserReq.LoginID = _lr.UserID;
                aAUserReq.LoginTypeID = _lr.LoginTypeID;
                IProcedure _proc = new ProcGetAPIUserAA(_dal);
                _resp = (AAUserData)_proc.Call(aAUserReq);
            }
            return _resp;
        }
        public ResponseStatus UpdateUAA(UpdateUserReq updateUserReq)
        {
            var _resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            if ((_lr.LoginTypeID == LoginType.ApplicationUser && _lr.RoleID == Role.Admin) || IsCustomerCareAuthorised(ActionCodes.AgreementApproval))
            {
                if (string.IsNullOrEmpty(updateUserReq.OutletName))
                {
                    _resp.Msg = "Invalid Parameter :- " + nameof(updateUserReq.OutletName);
                    return _resp;
                }
                if (string.IsNullOrEmpty(updateUserReq.OutletName))
                {
                    _resp.Msg = "Invalid Parameter :- " + nameof(updateUserReq.OutletName);
                    return _resp;
                }
                if (!Validate.O.IsPAN(updateUserReq.PAN))
                {
                    _resp.Msg = "Invalid Parameter :- " + nameof(updateUserReq.PAN);
                    return _resp;
                }
                if (!Validate.O.IsAADHAR(updateUserReq.AADHAR))
                {
                    _resp.Msg = "Invalid Parameter :- " + nameof(updateUserReq.AADHAR);
                    return _resp;
                }
                if (string.IsNullOrEmpty(updateUserReq.AgreementRemark))
                {
                    _resp.Msg = "Invalid Parameter :- " + nameof(updateUserReq.AgreementRemark);
                    return _resp;
                }
                updateUserReq.LoginID = _lr.UserID;
                updateUserReq.LoginTypeID = _lr.LoginTypeID;
                updateUserReq.IP = _rinfo.GetRemoteIP();
                updateUserReq.Browser = _rinfo.GetBrowser();
                IProcedure _proc = new ProcUpdateUserAA(_dal);
                _resp = (ResponseStatus)_proc.Call(updateUserReq);
            }
            return _resp;
        }
        #endregion

        public IResponseStatus ToggleCandebitStatus(int UserID, bool Is)
        {
            IProcedure proc = new ProcUpdateCanDebit(_dal);
            return (ResponseStatus)proc.Call(new CommonReq
            {
                LoginTypeID = _lr.LoginTypeID,
                LoginID = _lr.UserID,
                CommonInt = UserID,
                CommonBool = Is
            });
        }

        public IResponseStatus ToggleCandebitDownLineStatus(int UserID, bool Is)
        {
            IProcedure proc = new ProcUpdateCanDebitDownline(_dal);
            return (ResponseStatus)proc.Call(new CommonReq
            {
                LoginTypeID = _lr.LoginTypeID,
                LoginID = _lr.UserID,
                CommonInt = UserID,
                CommonBool = Is
            });
        }

        public IEnumerable<DebitFundrequest> DebitFundRequest(DebitFundrequest data)
        {
            var _resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            List<DebitFundrequest> lst = new List<DebitFundrequest>();
            if (_lr.LoginTypeID == LoginType.ApplicationUser)
            {

                data.LoginID = _lr.UserID;
                data.LT = _lr.LoginTypeID;
                if (data.Criteria != 0 && String.IsNullOrEmpty(data.CriteriaText))
                {
                    return lst;

                }

                IProcedure _proc = new ProcGetDebitFundRequest(_dal);
                lst = (List<DebitFundrequest>)_proc.Call(data);

            }
            return lst;
        }

        public IResponseStatus DebitRquesttStatus(int TableID, int status, bool MarkAsDebit, string Remark)
        {
            IProcedure proc = new ProcDebitRquesttStatus(_dal);
            return (ResponseStatus)proc.Call(new CommonReq
            {
                LoginTypeID = _lr.LoginTypeID,
                LoginID = _lr.UserID,
                CommonInt = TableID,
                CommonInt2 = status,
                CommonStr = _rinfo.GetBrowser(),
                CommonStr2 = _rinfo.GetLocalIP(),
                CommonBool = MarkAsDebit,
                CommonStr3 = Remark
            });
        }


        #region ID limit Transfer
        public IResponseStatus IDLimit(GetAddService req)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            req.CommonInt = _lr.UserID;
            req.CommonInt2 = _lr.LoginTypeID;

            DataTable dt = new DataTable();
            dt.Columns.Add("_OID");
            dt.Columns.Add("_OpTypeID");
            dt.Columns.Add("_IDLimit");
            foreach (var item in req.AddonServList)
            {
                DataRow dr = dt.NewRow();
                dr["_OID"] = item.OID;
                dr["_OpTypeID"] = item.OpTypeID;
                dr["_IDLimit"] = item.IDLimit;
                dt.Rows.Add(dr);
            }
            req.dt = dt;
            IProcedure proc = new ProcIDLimit(_dal);
            return (ResponseStatus)proc.Call(req);
        }


        public GetAddService GetAddServiceIDLimittransfer(int LT, int userid)
        {
            var _resp = new GetAddService();
            ISellerML s_ML = new SellerML(_accessor, _env);
            IProcedure proc = new procGetAdditionalServiceIDTransfer(_dal);
            _resp = (GetAddService)proc.Call(new CommonReq
            {
                LoginTypeID = LT,
                UserID = userid
            });
            return _resp;
        }

        #endregion

        public async Task<IResponseStatus> EnableGoogleAuthenticator(bool IsGoogle2FAEnable, string AccountSecretKey = "", bool Action = false, int userId = 0)
        {
            IResponseStatus resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            var _proc = new ProcEnable_2FA_Credentials(_dal);
            resp = (ResponseStatus)await _proc.Call(new CommonReq
            {
                UserID = userId,
                CommonBool = IsGoogle2FAEnable,
                CommonStr = AccountSecretKey,
                CommonBool1 = Action
            });
            return resp;
        }

        public async Task<IResponseStatus> ResetGoogleAuthenticator(int userId)
        {
            IResponseStatus res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            var _proc = new ProcResetGoogleAuthenticator(_dal);
            res = (ResponseStatus)await _proc.Call(userId);
            return res;
        }

        public UserList GetDebitRquesttStatus()
        {
            IProcedure proc = new ProcCanDabitStatus(_dal);
            return (UserList)proc.Call(new CommonReq
            {
                LoginTypeID = _lr.LoginTypeID,
                LoginID = _lr.UserID,

            });
        }

        public async Task<IEnumerable<UTRExcelMaster>> GetUTRStatementAsync()
        {
            var res = new List<UTRExcelMaster>();
            if ((_lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser))
            {
                IProcedureAsync _proc = new ProcgetUploadedUTRStatmentFiles(_dal);
                res = (List<UTRExcelMaster>)await _proc.Call().ConfigureAwait(false);
            }
            return res;
        }

        public async Task<IEnumerable<UtrStatementUpload>> DownloadUTRStatementAsync(int FiledID)
        {
            var res = new List<UtrStatementUpload>();
            if ((_lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser))
            {
                var req = new CommonReq
                {
                    CommonInt = FiledID

                };
                IProcedureAsync _proc = new ProcgetUploadedUTRFilesDownLoad(_dal);
                res = (List<UtrStatementUpload>)await _proc.Call(req).ConfigureAwait(false);
            }
            return res;
        }
        public IResponseStatus SwitchIMPStoNEFT(bool Status)
        {
            var _resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            var Req = new CommonReq
            {
                LoginID = _lr.UserID,
                CommonBool = Status,
            };
            IProcedure _proc = new ProcUpdateIMPStoNEFT(_dal);
            _resp = (ResponseStatus)_proc.Call(Req);
            return _resp;
        }

        public IResponseStatus _WallettoWallet(CommonReq commonreq)
        {
            var _resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            IProcedure _proc = new proc_WalletToWalletFTB2C(_dal);
            _resp = (ResponseStatus)_proc.Call(commonreq);
            return _resp;
        }

        public IResponseStatus UpdateUserFromAppB2C(GetEditUser _req)
        {
            var _resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (_req.LT == LoginType.ApplicationUser)
            {
                if (Validate.O.IsNumeric(_req.Name ?? "") || (_req.Name ?? "").Length > 100)
                {
                    _resp.Msg = ErrorCodes.InvalidParam + " Name";
                    return _resp;
                }
                if (Validate.O.IsNumeric(_req.OutletName ?? "") || (_req.OutletName ?? "").Length > 100)
                {
                    _resp.Msg = ErrorCodes.InvalidParam + " Outlet Name";
                    return _resp;
                }
                //if (!Validate.O.IsMobile(_req.MobileNo ?? ""))
                //{
                //    _resp.Msg = ErrorCodes.InvalidParam + " Mobile Number";
                //    return _resp;
                //}
                //if (!(_req.EmailID ?? "").Contains("@"))
                //{
                //    _resp.Msg = ErrorCodes.InvalidParam + " EmailID";
                //    return _resp;
                //}
                if (!Validate.O.IsPinCode(_req.Pincode ?? ""))
                {
                    _resp.Msg = ErrorCodes.InvalidParam + " Pincode";
                    return _resp;
                }
                if ((_req.Address ?? "").Length > 300)
                {
                    _resp.Msg = ErrorCodes.InvalidParam + " Address";
                    return _resp;
                }
                //if ((_req.City ?? "").Length > 50)
                //{
                //    _resp.Msg = ErrorCodes.InvalidParam + " City";
                //    return _resp;
                //}
                if (!string.IsNullOrEmpty(_req.PAN) && !Validate.O.IsPAN(_req.PAN ?? ""))
                {
                    _resp.Msg = ErrorCodes.InvalidParam + " PAN";
                    return _resp;
                }
                if (!string.IsNullOrEmpty(_req.AADHAR) && !Validate.O.IsAADHAR(_req.AADHAR ?? ""))
                {
                    _resp.Msg = ErrorCodes.InvalidParam + " AADHAR";
                    return _resp;
                }
                _req.IP = _rinfo.GetRemoteIP();
                _req.Browser = _rinfo.GetBrowser();
                IProcedure proc = new ProcUpdateUserB2C(_dal);
                _resp = (ResponseStatus)proc.Call(_req);
            }
            return _resp;

        }
        public IEnumerable<UserBillFetchReport> GetUserBillFetch(UserBillFetchReport Data)
        {
            var list = new List<UserBillFetchReport>();
            var _proc = new Proc_UserBillFetch(_dal);
            list = (List<UserBillFetchReport>)_proc.Call(Data);
            return list;
        }

        public async Task<IEnumerable<Team>> GetTeam(int UserId)
        {
            IProcedureAsync _proc = new ProcGetTeam(_dal);
            var res = (List<Team>)await _proc.Call(UserId).ConfigureAwait(false);
            return res;
        }

        public async Task<IEnumerable<Level>> GetLevel(int UserId)
        {
            IProcedureAsync _proc = new ProcGetLevel(_dal);
            var res = (List<Level>)await _proc.Call(UserId).ConfigureAwait(false);
            return res;
        }


        public ResponseStatus CheckUserByUIDandToken(JWTReqUsers jWTReqUsers)
        {
            var resp = new ResponseStatus { 
                Statuscode = -1,
                Msg = ErrorCodes.AnError
            };
            IProcedure _proc = new proc_CheckUserUIDAndToken(_dal);
            resp = (ResponseStatus)_proc.Call(new CommonReq { 
                UserID = jWTReqUsers.UserID,
                CommonStr = jWTReqUsers.UserToken
            });
            return resp;
        }

    }
}

