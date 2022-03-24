using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RoundpayFinTech.AppCode.Model.App;
using Microsoft.AspNetCore.Hosting;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using Fintech.AppCode.Configuration;
using Validators;
using System;
using Fintech.AppCode.HelperClass;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode;
using RoundpayFinTech.AppCode.MiddleLayer;
using Newtonsoft.Json;
using RoundpayFinTech.AppCode.Configuration;
using Microsoft.AspNetCore.Cors;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.Model.Reports.Filter;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.StaticModel;
using System.Linq;
using RoundpayFinTech.Models;
using RoundpayFinTech.AppCode.HelperClass;
using QRCoder;
using RoundpayFinTech.AppCode.Model.ROffer;

namespace RoundpayFinTech.Controllers
{
    [EnableCors("AllowWEBAAP")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class WebAppController : Controller
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        private readonly AppML appML;
        private readonly ISession _session;
        public WebAppController(IHttpContextAccessor accessor, IHostingEnvironment env)
        {
            _accessor = accessor;
            _env = env;
            appML = new AppML(_accessor, _env);
        }
        public IActionResult Get() {
            string jsn = @"{""error"":""0"",""DATA"":{""VC"":""3031563647"",""Name"":""MR Rakesh Barman"",""Rmn"":""8367829930"",""Balance"":""219.42"",""Monthly"":"""",""Next Recharge Date"":"""",""Plan"":"""",""Address"":""Ghagra Alipurduar"",""City"":"""",""District"":""24"",""State"":"""",""PIN Code"":""736121""},""Message"":""Offer Successfully Checked""}";
           
            return Json(DynamicHelper.O.GetKeyValuePairs(jsn, "DATA", false));
        }
        #region RequiredSection
        [HttpPost]
        public async Task<IActionResult> GetBalance()
        {
            var balanceResponse = new BalanceResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvaildSession
            };
            var readHeader = ReadHeader();
            var balanceReq = new AppSessionReq
            {
                APPID = readHeader.AppID,
                Version = readHeader.Version,
                Session = readHeader.Session,
                SessionID = readHeader.SessionID,
                LoginTypeID = LoginType.ApplicationUser,
                UserID = readHeader.UserID
            };

            var appResp = appML.CheckWebAppSession(balanceReq);
            balanceResponse.IsAppValid = appResp.IsAppValid;
            balanceResponse.IsVersionValid = appResp.IsVersionValid;
            balanceResponse.IsPasswordExpired = appResp.IsPasswordExpired;
            balanceResponse.Statuscode = appResp.Statuscode;
            balanceResponse.IsLookUpFromAPI = appResp.IsLookUpFromAPI;
            balanceResponse.IsDTHInfoCall = ApplicationSetting.IsDTHInfoCall;
            balanceResponse.IsShowPDFPlan = ApplicationSetting.IsShowPDFPlan;
            balanceResponse.IsWalletToWallet= ApplicationSetting.IsWalletToWallet;
            balanceResponse.IsReferral = ApplicationSetting.IsReferral;
            if (appResp.Statuscode == ErrorCodes.One)
            {
                balanceResponse.Statuscode = appResp.Statuscode;
                if (!appResp.IsPasswordExpired)
                {
                    await Task.Delay(0);
                    IUserML userML = new UserML(_accessor, _env, false);
                    balanceResponse.Statuscode = ErrorCodes.One;
                    balanceResponse.Msg = RechargeRespType._SUCCESS;
                    balanceResponse.Data = userML.GetUserBalnace(balanceReq.UserID, balanceReq.LoginTypeID);
                }
            }
            else
            {
                balanceResponse.Msg = appResp.Msg;
            }
            new PlansAPIML(_accessor, _env, false).LogWebAppReqResp(new CommonReq
            {
                CommonStr = "GetBalance",
                CommonStr2 = JsonConvert.SerializeObject(balanceReq),
                CommonStr3 = JsonConvert.SerializeObject(balanceResponse)
            });
            return Json(balanceResponse);
        }
        [HttpPost]
        public IActionResult GetAppNotification([FromBody] AppSessionReq appSessionReq)
        {
            var appRechargeRespose = new AppReportResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            var readHeader = ReadHeader();
            var notificationreq = new AppSessionReq
            {
                APPID = readHeader.AppID,
                Version = readHeader.Version,
                Session = readHeader.Session,
                SessionID = readHeader.SessionID,
                LoginTypeID = LoginType.ApplicationUser,
                UserID = readHeader.UserID
            };

            var appResp = appML.CheckWebAppSession(notificationreq);
            appRechargeRespose.IsAppValid = appResp.IsAppValid;
            appRechargeRespose.IsVersionValid = appResp.IsVersionValid;
            appRechargeRespose.CheckID = appResp.CheckID;
            appRechargeRespose.IsPasswordExpired = appResp.IsPasswordExpired;
            appRechargeRespose.Statuscode = appResp.Statuscode;
            if (appResp.Statuscode == ErrorCodes.One)
            {
                IUserAPPML ml = new UserML(_accessor, _env, false);
                appRechargeRespose.Notifications = ml.GetNotificationsApp(notificationreq.UserID);
                appRechargeRespose.Statuscode = ErrorCodes.One;
                appRechargeRespose.Msg = ErrorCodes.SUCCESS;
            }
            else
                appRechargeRespose.Msg = appResp.Msg;
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "GetAppNotification",
                CommonStr2 = JsonConvert.SerializeObject(appSessionReq),
                CommonStr3 = JsonConvert.SerializeObject(appRechargeRespose)
            });
            return Json(appRechargeRespose);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            var appResponse = new AppResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvalidLogin
            };
            var readHeader = ReadHeader();
            var logOutReq = new AppLogoutRequest
            {
                APPID = readHeader.AppID,
                Version = readHeader.Version,
                Session = readHeader.Session,
                SessionID = readHeader.SessionID,
                LoginTypeID = LoginType.ApplicationUser,
                UserID = readHeader.UserID
            };

            appResponse = appML.CheckWebAppSession(logOutReq);
            if (appResponse.Statuscode == ErrorCodes.One)
            {
                var logoutReq = new LogoutReq
                {
                    LT = logOutReq.LoginTypeID,
                    LoginID = logOutReq.UserID,
                    SessID = logOutReq.SessionID,
                    ULT = logOutReq.LoginTypeID,
                    UserID = logOutReq.UserID,
                    RequestMode = RequestMode.WEBAPPS,
                    SessionType = SessionType.Single
                };
                ILoginML loginML = new LoginML(_accessor, _env, false);
                var resp = await loginML.DoLogout(logoutReq);
                appResponse.Statuscode = resp.Statuscode;
                appResponse.Msg = resp.Msg;
            }
            new PlansAPIML(_accessor, _env, false).LogWebAppReqResp(new CommonReq
            {
                CommonStr = "Logout",
                CommonStr2 = JsonConvert.SerializeObject(logOutReq),
                CommonStr3 = JsonConvert.SerializeObject(appResponse)
            });
            return Json(appResponse);
        }
        #endregion

        #region TransactionRelated
        [HttpPost]//1
        public async Task<IActionResult> Transaction([FromBody] AppTransactionReq transactionRequest)
        {
            var appTransactionRes = new AppTransactionRes
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvaildSession
            };
            var readHeader = ReadHeader();
            transactionRequest.APPID = readHeader.AppID;
            transactionRequest.Version = readHeader.Version;
            transactionRequest.Session = readHeader.Session;
            transactionRequest.SessionID = readHeader.SessionID;
            transactionRequest.LoginTypeID = LoginType.ApplicationUser;
            transactionRequest.UserID = readHeader.UserID;
            var appResp = appML.CheckWebAppSession(transactionRequest);
            appTransactionRes.IsAppValid = appResp.IsAppValid;
            appTransactionRes.IsVersionValid = appResp.IsVersionValid;
            appTransactionRes.IsPasswordExpired = appResp.IsPasswordExpired;
            appTransactionRes.Statuscode = appResp.Statuscode;
            if (appResp.Statuscode == ErrorCodes.One)
            {
                if (!appResp.IsPasswordExpired)
                {
                    var appRechargeRequest = new AppRechargeRequest
                    {
                        UserID = transactionRequest.UserID,
                        OID = transactionRequest.OID,
                        Mob = transactionRequest.AccountNo,
                        Amt = transactionRequest.Amount,
                        IMEI = transactionRequest.IMEI,
                        CustNo = transactionRequest.CustomerNo,
                        O1 = transactionRequest.O1,
                        O2 = transactionRequest.O2,
                        O3 = transactionRequest.O3,
                        O4 = transactionRequest.O4,
                        GEOCode = transactionRequest.GeoCode,
                        Pincode = "123434",
                        SecKey = transactionRequest.SecurityKey ?? "",
                        RequestMode = RequestMode.WEBAPPS,
                        PromoCodeID = transactionRequest.PromoCodeID > 0 ? transactionRequest.PromoCodeID : 0,
                        FetchBillID = 0
                    };
                    ISellerML sellerML = new SellerML(_accessor, _env, false);
                    appTransactionRes = await sellerML.AppRecharge(appRechargeRequest);
                    appTransactionRes.IsAppValid = appResp.IsAppValid;
                    appTransactionRes.IsVersionValid = appResp.IsVersionValid;
                }
            }
            else
            {
                appTransactionRes.Msg = appResp.Msg;
            }
            new PlansAPIML(_accessor, _env, false).LogWebAppReqResp(new CommonReq
            {
                CommonStr = "Transaction",
                CommonStr2 = JsonConvert.SerializeObject(transactionRequest),
                CommonStr3 = JsonConvert.SerializeObject(appTransactionRes)
            });
            return Json(appTransactionRes);
        }
        [HttpPost]//2
        public async Task<IActionResult> FetchBill([FromBody] AppTransactionReq transactionRequest)
        {
            var appBillfetchResponse = new AppBillfetchResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvaildSession
            };
            var readHeader = ReadHeader();
            transactionRequest.APPID = readHeader.AppID;
            transactionRequest.Version = readHeader.Version;
            transactionRequest.Session = readHeader.Session;
            transactionRequest.SessionID = readHeader.SessionID;
            transactionRequest.LoginTypeID = LoginType.ApplicationUser;
            transactionRequest.UserID = readHeader.UserID;
            var appResp = appML.CheckWebAppSession(transactionRequest);
            appBillfetchResponse.IsAppValid = appResp.IsAppValid;
            appBillfetchResponse.IsVersionValid = appResp.IsVersionValid;
            appBillfetchResponse.IsPasswordExpired = appResp.IsPasswordExpired;
            appBillfetchResponse.Statuscode = appResp.Statuscode;
            if (appResp.Statuscode == ErrorCodes.One)
            {
                if (!appResp.IsPasswordExpired)
                {
                    var transactionServiceReq = new TransactionServiceReq
                    {
                        UserID = transactionRequest.UserID,
                        OID = transactionRequest.OID,
                        AccountNo = transactionRequest.AccountNo,
                        OutletID = transactionRequest.OutletID,
                        Optional1 = transactionRequest.O1,
                        Optional2 = transactionRequest.O2,
                        Optional3 = transactionRequest.O3,
                        Optional4 = transactionRequest.O4,
                        RefID = transactionRequest.RefID,
                        GEOCode = transactionRequest.GeoCode,
                        CustomerNumber = transactionRequest.CustomerNo,
                        RequestModeID = RequestMode.WEBAPPS
                    };
                    var bbpsML = new BBPSML(_accessor, _env, false);
                    appBillfetchResponse.bBPSResponse = await bbpsML.FetchBillMLApp(transactionServiceReq);
                    if (appBillfetchResponse.bBPSResponse.Amount != null)
                    {
                        appBillfetchResponse.Statuscode = ErrorCodes.One;
                        appBillfetchResponse.Msg = ErrorCodes.ValidSession;
                    }
                }
            }
            else
            {
                appBillfetchResponse.Msg = appResp.Msg;
            }
            new PlansAPIML(_accessor, _env, false).LogWebAppReqResp(new CommonReq
            {
                CommonStr = "FetchBill",
                CommonStr2 = JsonConvert.SerializeObject(transactionRequest),
                CommonStr3 = JsonConvert.SerializeObject(appBillfetchResponse)
            });
            return Json(appBillfetchResponse);
        }
        #endregion

        #region ReportingSection
        [HttpPost]//3
        public async Task<IActionResult> RechargeReport([FromBody] AppRechargeReportReq appRechargeReportReq)
        {
            var appRechargeRespose = new AppReportResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            var readHeader = ReadHeader();
            appRechargeReportReq.APPID = readHeader.AppID;
            appRechargeReportReq.Version = readHeader.Version;
            appRechargeReportReq.Session = readHeader.Session;
            appRechargeReportReq.SessionID = readHeader.SessionID;
            appRechargeReportReq.LoginTypeID = LoginType.ApplicationUser;
            appRechargeReportReq.UserID = readHeader.UserID;
            var appResp = appML.CheckWebAppSession(appRechargeReportReq);
            appRechargeRespose.IsAppValid = appResp.IsAppValid;
            appRechargeRespose.IsVersionValid = appResp.IsVersionValid;
            appRechargeRespose.IsPasswordExpired = appResp.IsPasswordExpired;
            appRechargeRespose.Statuscode = appResp.Statuscode;
            if (appResp.Statuscode == ErrorCodes.One)
            {
                if (!appResp.IsPasswordExpired)
                {
                    IAppReportML appReportML = new ReportML(_accessor, _env, false);
                    var _filter = new _RechargeReportFilter
                    {
                        AccountNo = appRechargeReportReq.AccountNo,
                        OID = appRechargeReportReq.OID,
                        Status = appRechargeReportReq.Status,
                        TopRows = appRechargeReportReq.TopRows,
                        FromDate = appRechargeReportReq.FromDate,
                        ToDate = appRechargeReportReq.ToDate,
                        IsExport = appRechargeReportReq.IsExport,
                        LoginID = appRechargeReportReq.UserID,
                        LT = appRechargeReportReq.LoginTypeID,
                        OPTypeID = appRechargeReportReq.OpTypeID,
                        IsRecent = appRechargeReportReq.IsRecent
                    };
                    appRechargeRespose.Statuscode = ErrorCodes.One;
                    appRechargeRespose.Msg = ErrorCodes.SUCCESS;
                    appRechargeRespose.RechargeReport = await appReportML.GetAppRechargeReport(_filter);
                }
            }
            else
            {
                appRechargeRespose.Msg = appResp.Msg;
            }
            new PlansAPIML(_accessor, _env, false).LogWebAppReqResp(new CommonReq
            {
                CommonStr = "RechargeReport",
                CommonStr2 = JsonConvert.SerializeObject(appRechargeReportReq),
                CommonStr3 = JsonConvert.SerializeObject(appRechargeRespose)
            });
            return Json(appRechargeRespose);
        }
        [HttpPost]//4
        public async Task<IActionResult> LedgerReport([FromBody] AppLedgerReq appLedgerReq)
        {
            var appReportResponse = new AppReportResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            var readHeader = ReadHeader();
            appLedgerReq.APPID = readHeader.AppID;
            appLedgerReq.Version = readHeader.Version;
            appLedgerReq.Session = readHeader.Session;
            appLedgerReq.SessionID = readHeader.SessionID;
            appLedgerReq.LoginTypeID = LoginType.ApplicationUser;
            appLedgerReq.UserID = readHeader.UserID;
            var appResp = appML.CheckWebAppSession(appLedgerReq);
            appReportResponse.IsAppValid = appResp.IsAppValid;
            appReportResponse.IsVersionValid = appResp.IsVersionValid;
            appReportResponse.IsPasswordExpired = appResp.IsPasswordExpired;
            appReportResponse.Statuscode = appResp.Statuscode;
            if (appResp.Statuscode == ErrorCodes.One)
            {
                if (!appResp.IsPasswordExpired)
                {
                    IAppReportML appReportML = new ReportML(_accessor, _env, false);
                    var _filter = new ProcUserLedgerRequest
                    {
                        Mobile_F = appLedgerReq.AccountNo,
                        LT = LoginType.ApplicationUser,
                        LoginID = appLedgerReq.UserID,
                        DebitCredit_F = appLedgerReq.Status,
                        TopRows = appLedgerReq.TopRows,
                        FromDate_F = appLedgerReq.FromDate,
                        ToDate_F = appLedgerReq.ToDate,
                        TransactionId_F = appLedgerReq.TransactionID,
                        WalletTypeID = appLedgerReq.WalletTypeID
                    };
                    appReportResponse.Statuscode = ErrorCodes.One;
                    appReportResponse.Msg = ErrorCodes.SUCCESS;
                    appReportResponse.LedgerReport = await appReportML.GetAppUserLedgerList(_filter);
                }
            }
            else
            {
                appReportResponse.Msg = appResp.Msg;
            }
            new PlansAPIML(_accessor, _env, false).LogWebAppReqResp(new CommonReq
            {
                CommonStr = "LedgerReport",
                CommonStr2 = JsonConvert.SerializeObject(appLedgerReq),
                CommonStr3 = JsonConvert.SerializeObject(appReportResponse)
            });
            return Json(appReportResponse);
        }
        #endregion

        #region SignupSection
        [HttpPost]//5
        public async Task<IActionResult> Signup([FromBody] WebUserCreate appUserCreate)
        {
            var appResp = new AppResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            var readHeader = ReadHeader();
            appUserCreate.APPID = readHeader.AppID;
            appUserCreate.Version = readHeader.Version;
            var appResponse = appML.CheckWebApp(appUserCreate);
            appResp.Statuscode = appResponse.Statuscode;
            appResp.Msg = appResponse.Msg;
            appResp.IsAppValid = appResponse.IsAppValid;
            appResp.IsVersionValid = appResponse.IsVersionValid;
            appResp.Statuscode = appResponse.Statuscode;
            if (appResp.Statuscode == ErrorCodes.One)
            {
                IUserAPPML userML = new UserML(_accessor, _env, true);
                await Task.Delay(0);
                var userCreate = new UserCreate
                {
                    Name = appUserCreate.Name,
                    MobileNo = appUserCreate.MobileNo,
                    EmailID = appUserCreate.EmailID,
                    Password = appUserCreate.Password,
                    Pincode = appUserCreate.Pincode,
                    Address = appUserCreate.Address,
                    ReferralNo= appUserCreate.ReferralNo
                };
                var resp = await userML.CallSignupWebApp(userCreate);
                appResp.Statuscode = resp.Statuscode;
                appResp.Msg = resp.Msg;
            }
            new PlansAPIML(_accessor, _env, false).LogWebAppReqResp(new CommonReq
            {
                CommonStr = "Signup",
                CommonStr2 = JsonConvert.SerializeObject(appUserCreate),
                CommonStr3 = JsonConvert.SerializeObject(appResp)
            });
            return Json(appResp);
        }
        #endregion

        #region LoginSection
        [HttpPost]//6
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            var loginResponse = new AppLoginResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvalidLogin
            };
            var readHeader = ReadHeader();
            loginRequest.APPID = readHeader.AppID;
            loginRequest.Version = readHeader.Version;
            loginRequest.Domain = readHeader.Domain;
            var appResp = appML.CheckWebApp(loginRequest);
            if (appResp.Statuscode == ErrorCodes.One)
            {
                await Task.Delay(0);
                if (loginRequest == null)
                {
                    return Json(loginResponse);
                }
                var loginDetail = new LoginDetail
                {
                    LoginTypeID = LoginType.ApplicationUser,
                    LoginMobile = loginRequest.UserID,
                    Password = loginRequest.Password,
                    RequestMode = RequestMode.WEBAPPS,
                    CommonStr = nameof(RequestMode.WEBAPPS)
                };
                loginDetail.Password = HashEncryption.O.Encrypt(loginDetail.Password);
                ILoginML _loginML = new LoginML(_accessor, _env, loginRequest.Domain);
                loginResponse = _loginML.DoWebAppLogin(loginDetail);
            }
            else
            {
                loginResponse.Msg = appResp.Msg;
            }
            loginResponse.IsAppValid = appResp.IsAppValid;
            loginResponse.IsVersionValid = appResp.IsVersionValid;

            new PlansAPIML(_accessor, _env, false).LogWebAppReqResp(new CommonReq
            {
                CommonStr = "Login",
                CommonStr2 = JsonConvert.SerializeObject(loginRequest),
                CommonStr3 = JsonConvert.SerializeObject(loginResponse)
            });

            return Json(loginResponse);
        }

        [HttpPost]//7
        public async Task<IActionResult> ValidateOTP([FromBody] OTPRequest oTPRequest)
        {
            var loginResponse = new AppLoginResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = nameof(ErrorCodes.Invalid_OTP).Replace("_", " ")
            };
            var readHeader = ReadHeader();
            oTPRequest.APPID = readHeader.AppID;
            oTPRequest.Version = readHeader.Version;
            oTPRequest.Domain = readHeader.Domain;
            var appResp = appML.CheckWebApp(oTPRequest);
            try
            {
                var OTPSession = HashEncryption.O.Decrypt(oTPRequest.OTPSession);
                var _lr = JsonConvert.DeserializeObject<LoginResponse>(OTPSession);
                ILoginML loginML = new LoginML(_accessor, _env, oTPRequest.Domain);
                await Task.Delay(0);
                loginResponse = loginML.ValidateOTPForWebApp(oTPRequest.OTP, _lr);
            }
            catch (Exception ex)
            {
            }
            loginResponse.IsAppValid = appResp.IsAppValid;
            loginResponse.IsVersionValid = appResp.IsVersionValid;
            new PlansAPIML(_accessor, _env, false).LogWebAppReqResp(new CommonReq
            {
                CommonStr = "ValidateOTP",
                CommonStr2 = JsonConvert.SerializeObject(oTPRequest),
                CommonStr3 = JsonConvert.SerializeObject(loginResponse)
            });
            return Json(loginResponse);
        }
        #endregion

        #region OperatorRelated
        [HttpPost]//8
        public async Task<IActionResult> GetOpTypes()
        {
            var servicesAssigned = new ServicesAssigned
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.NODATA
            };
            var readHeader = ReadHeader();
            var appRequest = new AppRequest
            {
                APPID = readHeader.AppID,
                Version = readHeader.Version
            };
            var appResp = appML.CheckWebApp(appRequest);
            servicesAssigned.IsAppValid = appResp.IsAppValid;
            servicesAssigned.IsVersionValid = appResp.IsVersionValid;
            servicesAssigned.IsAddMoneyEnable = ApplicationSetting.IsAddMoneyEnable;// && appResp.CheckID == ErrorCodes.One;
            servicesAssigned.IsPaymentGatway = ApplicationSetting.IsPaymentGatway;
            servicesAssigned.IsUPI = ApplicationSetting.IsUPI;
            servicesAssigned.IsUPIQR = ApplicationSetting.IsUPIQR && (ApplicationSetting.IsFraudPrevention && appResp.IsGreen || !ApplicationSetting.IsFraudPrevention) && (appResp.IsPaymentGateway);
            servicesAssigned.IsECollectEnable = ApplicationSetting.IsECollectEnable;
            servicesAssigned.IsDMTWithPipe = ApplicationSetting.IsDMTWithPIPE;
            servicesAssigned.IsBulkQRGeneration = ApplicationSetting.IsBulkQRGeneration;
            servicesAssigned.IsCoin = ApplicationSetting.IsCOIN;
            servicesAssigned.Statuscode = appResp.Statuscode;
            if (appResp.Statuscode == ErrorCodes.One)
            {
                await Task.Delay(0);
                ISellerML sellerML = new SellerML(_accessor, _env, false);
                servicesAssigned.Statuscode = ErrorCodes.One;
                servicesAssigned.Msg = RechargeRespType._SUCCESS;
                var package_ClData = new Package_ClData
                {
                    AssignedOpTypes = sellerML.GetPackage(-1000)
                };
                servicesAssigned.Data = package_ClData;
            }
            else
            {
                servicesAssigned.Msg = appResp.Msg;
            }

            new PlansAPIML(_accessor, _env, false).LogWebAppReqResp(new CommonReq
            {
                CommonStr = "GetOpTypes",
                CommonStr2 = JsonConvert.SerializeObject(appRequest),
                CommonStr3 = JsonConvert.SerializeObject(servicesAssigned)
            });
            return Json(servicesAssigned);
        }

        [HttpPost]//9
        public async Task<IActionResult> GetNumberList()
        {
            var operatorResponse = new OperatorResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvaildSession
            };
            var readHeader = ReadHeader();
            var appRequest = new AppRequest
            {
                APPID = readHeader.AppID,
                Version = readHeader.Version
            };

            var appResp = appML.CheckWebApp(appRequest);
            operatorResponse.IsAppValid = appResp.IsAppValid;
            operatorResponse.IsVersionValid = appResp.IsVersionValid;
            IOperatorAppML opml = new OperatorML(_accessor, _env, false);
            operatorResponse.Statuscode = ErrorCodes.One;
            operatorResponse.Msg = ErrorCodes.SUCCESS;
            var opNumberData = new OpNumberData
            {
                NumSeries = await opml.NumberList(),
                Operators = opml.GetOperatorsApp(Role.Customer),
                Cirlces = await opml.CircleList()
            };
            operatorResponse.Data = opNumberData;
            operatorResponse.IsLookUpFromAPI = appResp.IsLookUpFromAPI;
            operatorResponse.IsDTHInfo = ApplicationSetting.IsDTHInfo;
            operatorResponse.IsRoffer = ApplicationSetting.IsRoffer;
            operatorResponse.IsHeavyRefresh = ApplicationSetting.IsHeavyRefresh;
            new PlansAPIML(_accessor, _env, false).LogWebAppReqResp(new CommonReq
            {
                CommonStr = "GetNumberList",
                CommonStr2 = JsonConvert.SerializeObject(appRequest),
                CommonStr3 = JsonConvert.SerializeObject(operatorResponse)
            });
            return Json(operatorResponse);
        }
        #endregion

        #region Amit

        [HttpPost]//10
        public async Task<IActionResult> GetBanner([FromBody] BannerRequest request)
        {
            var appResp = new BannerResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            var readHeader = ReadHeader();
            request.APPID = readHeader.AppID;
            request.Version = readHeader.Version;
            request.Domain = readHeader.Domain;
            var appResponse = appML.CheckWebApp(request);
            appResp.Statuscode = appResponse.Statuscode;
            appResp.Msg = appResponse.Msg;
            appResp.IsAppValid = appResponse.IsAppValid;
            appResp.IsVersionValid = appResponse.IsVersionValid;
            appResp.Statuscode = appResponse.Statuscode;
            if (appResp.Statuscode == ErrorCodes.One)
            {
                LoginML _loginML = new LoginML(_accessor, _env);
                var Winfo = _loginML.GetWebsiteInfo(request.Domain);
                IBannerML bannerML = new ResourceML(_accessor, _env);
                await Task.Delay(0);
                var resp = bannerML.GetB2CBanners(Winfo.WID.ToString(), request.OpType);
                appResp.Statuscode = ErrorCodes.One;
                appResp.Msg = "";
                appResp.BannerUrl = resp;
            }
            new PlansAPIML(_accessor, _env, false).LogWebAppReqResp(new CommonReq
            {
                CommonStr = "GetBanner",
                CommonStr2 = JsonConvert.SerializeObject(request),
                CommonStr3 = JsonConvert.SerializeObject(appResp)
            });
            return Json(appResp);
        }
        #endregion

        #region PlanAPI
        [HttpPost]//11
        public async Task<IActionResult> ROffer([FromBody] AppSimplePlanReq appSimplePlanReq)
        {
            var appROfferResp = new AppROfferResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvalidParam
            };
            var readHeader = ReadHeader();
            appSimplePlanReq.APPID = readHeader.AppID;
            appSimplePlanReq.Version = readHeader.Version;
            var appResp = appML.CheckWebApp(appSimplePlanReq);
            appROfferResp.IsAppValid = appResp.IsAppValid;
            appROfferResp.IsVersionValid = appResp.IsVersionValid;
            var plansAPIML = new PlansAPIML(_accessor, _env, false);
            if (appResp.Statuscode == ErrorCodes.One)
            {
                await Task.Delay(0);
                if (ApplicationSetting.PlanType == PlanType.MPLAN)
                {
                    IMplan implan = new PlansAPIML(_accessor, _env);
                    appROfferResp.Data = plansAPIML.GetRoffer(appSimplePlanReq.AccountNo, appSimplePlanReq.OID);
                }
                if (ApplicationSetting.PlanType == PlanType.Roundpay)
                {
                    IRoundpayPlan RPPlan = new PlansAPIML(_accessor, _env);
                    appROfferResp.Data = plansAPIML.GetRofferRoundpay(appSimplePlanReq.AccountNo, appSimplePlanReq.OID);
                }
                if (ApplicationSetting.PlanType == PlanType.PLANAPI)
                {
                    IPlanAPIPlan RPPlan = new PlansAPIML(_accessor, _env);
                    appROfferResp.DataPA = plansAPIML.GetRofferPLANAPI(appSimplePlanReq.AccountNo, appSimplePlanReq.OID);
                }
                appROfferResp.Statuscode = ErrorCodes.One;
                appROfferResp.Msg = ErrorCodes.SUCCESS;
            }
            else
            {
                appROfferResp.Msg = appResp.Msg;
            }
            plansAPIML.LogWebAppReqResp(new CommonReq
            {
                CommonStr = "ROffer",
                CommonStr2 = JsonConvert.SerializeObject(appSimplePlanReq),
                CommonStr3 = JsonConvert.SerializeObject(appROfferResp)
            });
            return Json(appROfferResp);
        }
        [HttpPost]//12
        public async Task<IActionResult> SimplePlan([FromBody] AppSimplePlanReq appSimplePlanReq)
        {
            var appSimplePlanResp = new AppSimplePlanResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvalidParam
            };
            var readHeader = ReadHeader();
            appSimplePlanReq.APPID = readHeader.AppID;
            appSimplePlanReq.Version = readHeader.Version;
            var appResp = appML.CheckWebApp(appSimplePlanReq);
            appSimplePlanResp.IsAppValid = appResp.IsAppValid;
            appSimplePlanResp.IsVersionValid = appResp.IsVersionValid;
            var plansAPIML = new PlansAPIML(_accessor, _env, false);
            if (appResp.Statuscode == ErrorCodes.One)
            {
                await Task.Delay(0);
                if (ApplicationSetting.PlanType == PlanType.MPLAN)
                {
                    IMplan implan = new PlansAPIML(_accessor, _env);
                    appSimplePlanResp.Data = plansAPIML.GetSimplePlan(appSimplePlanReq.CircleID, appSimplePlanReq.OID);
                }
                if (ApplicationSetting.PlanType == PlanType.Roundpay)
                {
                    IRoundpayPlan RPPlan = new PlansAPIML(_accessor, _env);
                    appSimplePlanResp.DataRP = plansAPIML.GetSimplePlanRoundpay(appSimplePlanReq.CircleID, appSimplePlanReq.OID);
                }
                if (ApplicationSetting.PlanType == PlanType.PLANAPI)
                {
                    IPlanAPIPlan RPPlan = new PlansAPIML(_accessor, _env);
                    appSimplePlanResp.DataPA = plansAPIML.GetSimplePlanAPI(appSimplePlanReq.CircleID, appSimplePlanReq.OID);
                }
                appSimplePlanResp.Statuscode = ErrorCodes.One;
                appSimplePlanResp.Msg = ErrorCodes.SUCCESS;
            }
            else
            {
                appSimplePlanResp.Msg = appResp.Msg;
            }
            plansAPIML.LogWebAppReqResp(new CommonReq
            {
                CommonStr = "SimplePlan",
                CommonStr2 = JsonConvert.SerializeObject(appSimplePlanReq),
                CommonStr3 = JsonConvert.SerializeObject(appSimplePlanResp)
            });
            return Json(appSimplePlanResp);
        }
        [HttpPost]//13
        public async Task<IActionResult> DTHCustomerInfo([FromBody] AppSimplePlanReq appSimplePlanReq)
        {
            var appCustInfo = new AppDTHCustInfo
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvalidParam
            };
            var readHeader = ReadHeader();
            appSimplePlanReq.APPID = readHeader.AppID;
            appSimplePlanReq.Version = readHeader.Version;
            AppResponse appResp = appML.CheckWebApp(appSimplePlanReq);
            appCustInfo.IsAppValid = appResp.IsAppValid;
            appCustInfo.IsVersionValid = appResp.IsVersionValid;
            var plansAPIML = new PlansAPIML(_accessor, _env, false);
            if (appResp.Statuscode == ErrorCodes.One)
            {
                await Task.Delay(0);
                if (ApplicationSetting.PlanType == PlanType.MPLAN)
                {
                    IMplan implan = new PlansAPIML(_accessor, _env);
                    appCustInfo.Data = plansAPIML.GetDTHCustInfo(appSimplePlanReq.AccountNo, appSimplePlanReq.OID);
                }
                if (ApplicationSetting.PlanType == PlanType.Roundpay)
                {
                    IRoundpayPlan RPPlan = new PlansAPIML(_accessor, _env);
                    appCustInfo.Data = plansAPIML.GetDTHCustInfoRoundpay(appSimplePlanReq.AccountNo, appSimplePlanReq.OID);
                }
                if (ApplicationSetting.PlanType == PlanType.PLANAPI)
                {
                    IPlanAPIPlan RPPlan = new PlansAPIML(_accessor, _env);
                    appCustInfo.DataPA = plansAPIML.GetDTHCustInfoPlanAPI(appSimplePlanReq.AccountNo, appSimplePlanReq.OID);
                }
                appCustInfo.Statuscode = ErrorCodes.One;
                appCustInfo.Msg = ErrorCodes.SUCCESS;
            }
            else
            {
                appCustInfo.Msg = appResp.Msg;
            }
            plansAPIML.LogWebAppReqResp(new CommonReq
            {
                CommonStr = "DTHCustomerInfo",
                CommonStr2 = JsonConvert.SerializeObject(appSimplePlanReq),
                CommonStr3 = JsonConvert.SerializeObject(appCustInfo)
            });
            return Json(appCustInfo);
        }
        [HttpPost]//14
        public async Task<IActionResult> DTHSimplePlanInfo([FromBody] AppSimplePlanReq appSimplePlanReq)
        {
            var appDTHSimplePlanResp = new AppDTHSimplePlanResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvalidParam
            };
            var readHeader = ReadHeader();
            appSimplePlanReq.APPID = readHeader.AppID;
            appSimplePlanReq.Version = readHeader.Version;
            var appResp = appML.CheckWebApp(appSimplePlanReq);
            appDTHSimplePlanResp.IsAppValid = appResp.IsAppValid;
            appDTHSimplePlanResp.IsVersionValid = appResp.IsVersionValid;
            var plansAPIML = new PlansAPIML(_accessor, _env, false);
            if (appResp.Statuscode == ErrorCodes.One)
            {
                await Task.Delay(0);
                if (ApplicationSetting.PlanType == PlanType.MPLAN)
                {
                    IMplan implan = new PlansAPIML(_accessor, _env);
                    appDTHSimplePlanResp.Data = plansAPIML.GetDTHSimplePlan(appSimplePlanReq.OID);
                }
                if (ApplicationSetting.PlanType == PlanType.Roundpay)
                {
                    IRoundpayPlan RPPlan = new PlansAPIML(_accessor, _env);
                    appDTHSimplePlanResp.DataRP = plansAPIML.GetDTHSimplePlanRoundpay(appSimplePlanReq.OID);
                }
                if (ApplicationSetting.PlanType == PlanType.PLANAPI)
                {
                    IPlanAPIPlan RPPlan = new PlansAPIML(_accessor, _env);
                    appDTHSimplePlanResp.DataPA = plansAPIML.GetDTHSimplePlanAPI(appSimplePlanReq.OID);
                }
                appDTHSimplePlanResp.Statuscode = ErrorCodes.One;
                appDTHSimplePlanResp.Msg = ErrorCodes.SUCCESS;
            }
            else
            {
                appDTHSimplePlanResp.Msg = appResp.Msg;
            }
            plansAPIML.LogWebAppReqResp(new CommonReq
            {
                CommonStr = "DTHSimplePlanInfo",
                CommonStr2 = JsonConvert.SerializeObject(appSimplePlanReq),
                CommonStr3 = JsonConvert.SerializeObject(appDTHSimplePlanResp)
            });
            return Json(appDTHSimplePlanResp);
        }
        [HttpPost]//15
        public async Task<IActionResult> DTHChannelPlanInfo([FromBody] AppSimplePlanReq appSimplePlanReq)
        {
            var appDTHSimplePlanResp = new AppDTHSimplePlanResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvalidParam
            };
            var readHeader = ReadHeader();
            appSimplePlanReq.APPID = readHeader.AppID;
            appSimplePlanReq.Version = readHeader.Version;
            var appResp = appML.CheckWebApp(appSimplePlanReq);
            appDTHSimplePlanResp.IsAppValid = appResp.IsAppValid;
            appDTHSimplePlanResp.IsVersionValid = appResp.IsVersionValid;
            var plansAPIML = new PlansAPIML(_accessor, _env, false);
            if (appResp.Statuscode == ErrorCodes.One)
            {
                await Task.Delay(0);
                if (ApplicationSetting.PlanType == PlanType.MPLAN)
                {
                    IMplan implan = new PlansAPIML(_accessor, _env);
                    appDTHSimplePlanResp.Data = plansAPIML.GetDTHChannelPlan(appSimplePlanReq.OID);
                }
                if (ApplicationSetting.PlanType == PlanType.Roundpay)
                {
                    IRoundpayPlan RPPlan = new PlansAPIML(_accessor, _env);
                    appDTHSimplePlanResp.DataRPDTHChannelList = plansAPIML.RPDTHSimplePlansChannelList(appSimplePlanReq.PackageID, appSimplePlanReq.OID);
                }
                if (ApplicationSetting.PlanType == PlanType.PLANAPI)
                {
                    IPlanAPIPlan RPPlan = new PlansAPIML(_accessor, _env);
                    appDTHSimplePlanResp.DataPA = plansAPIML.GetDTHChannelPlanAPI(appSimplePlanReq.OID);
                }
                appDTHSimplePlanResp.Statuscode = ErrorCodes.One;
                appDTHSimplePlanResp.Msg = ErrorCodes.SUCCESS;
            }
            else
            {
                appDTHSimplePlanResp.Msg = appResp.Msg;
            }
            plansAPIML.LogWebAppReqResp(new CommonReq
            {
                CommonStr = "DTHChannelPlanInfo",
                CommonStr2 = JsonConvert.SerializeObject(appSimplePlanReq),
                CommonStr3 = JsonConvert.SerializeObject(appDTHSimplePlanResp)
            });
            return Json(appDTHSimplePlanResp);
        }
        [HttpPost]//16
        public async Task<IActionResult> DTHHeavyRefresh([FromBody] AppSimplePlanReq appSimplePlanReq)
        {
            var appMplanDTHHeavyRefreshResp = new AppMplanDTHHeavyRefresh
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvalidParam
            };
            var readHeader = ReadHeader();
            appSimplePlanReq.APPID = readHeader.AppID;
            appSimplePlanReq.Version = readHeader.Version;
            var appResp = appML.CheckWebApp(appSimplePlanReq);
            appMplanDTHHeavyRefreshResp.IsAppValid = appResp.IsAppValid;
            appMplanDTHHeavyRefreshResp.IsVersionValid = appResp.IsVersionValid;
            var plansAPIML = new PlansAPIML(_accessor, _env, false);
            if (appResp.Statuscode == ErrorCodes.One)
            {
                await Task.Delay(0);
                if (ApplicationSetting.PlanType == PlanType.MPLAN)
                {
                    IMplan implan = new PlansAPIML(_accessor, _env);
                    appMplanDTHHeavyRefreshResp.Data = plansAPIML.GetDTHHeavyRefresh(appSimplePlanReq.AccountNo, appSimplePlanReq.OID);
                }

                appMplanDTHHeavyRefreshResp.Statuscode = ErrorCodes.One;
                appMplanDTHHeavyRefreshResp.Msg = ErrorCodes.SUCCESS;
            }
            else
            {
                appMplanDTHHeavyRefreshResp.Msg = appResp.Msg;
            }
            plansAPIML.LogWebAppReqResp(new CommonReq
            {
                CommonStr = "DTHHeavyRefresh",
                CommonStr2 = JsonConvert.SerializeObject(appSimplePlanReq),
                CommonStr3 = JsonConvert.SerializeObject(appMplanDTHHeavyRefreshResp)
            });
            return Json(appMplanDTHHeavyRefreshResp);
        }
        #endregion
        private HeaderInfo ReadHeader()
        {
            var headerInfo = new HeaderInfo();
            try
            {
                HttpContext.Request.Headers.TryGetValue(nameof(headerInfo.AppID), out var AppID);
                HttpContext.Request.Headers.TryGetValue(nameof(headerInfo.Version), out var Version);
                HttpContext.Request.Headers.TryGetValue(nameof(headerInfo.UserID), out var UserID);
                HttpContext.Request.Headers.TryGetValue(nameof(headerInfo.SessionID), out var SessionID);
                HttpContext.Request.Headers.TryGetValue(nameof(headerInfo.Session), out var Session);
                HttpContext.Request.Headers.TryGetValue(nameof(headerInfo.OutletID), out var OutletID);
                HttpContext.Request.Headers.TryGetValue(nameof(headerInfo.Domain), out var Domain);
                headerInfo.AppID = AppID.Count > 0 ? AppID[0] : string.Empty;
                headerInfo.Version = Version.Count > 0 ? Version[0] : string.Empty;
                headerInfo.UserID = UserID.Count > 0 ? (Validate.O.IsNumeric(UserID[0] ?? string.Empty) ? Convert.ToInt32(UserID[0]) : 0) : 0;
                headerInfo.OutletID = OutletID.Count > 0 ? (Validate.O.IsNumeric(OutletID[0] ?? string.Empty) ? Convert.ToInt32(OutletID[0]) : 0) : 0;
                headerInfo.SessionID = SessionID.Count > 0 ? (Validate.O.IsNumeric(SessionID[0] ?? string.Empty) ? Convert.ToInt32(SessionID[0]) : 0) : 0;
                headerInfo.Session = Session.Count > 0 ? Session[0] : string.Empty;
                headerInfo.Domain = Domain.Count > 0 ? Domain[0] : string.Empty;
            }
            catch (Exception)
            {
            }
            return headerInfo;
        }

        public IActionResult GetResult()
        {
            var KeyVals = new Dictionary<string, object>
                {
                    { "api_key", "" },
                    { "order_id", "" },
                    { "mode", "" },
                    { "amount", 100.00 }
                };
            string[] hash_columns = {
            "amount",
            "api_key",
            "city",
            "country",
            "currency",
            "description",
            "email",
            "mode",
            "name",
            "order_id",
            "phone",
            "return_url",
            "state",
            "udf1",
            "udf2",
            "udf3",
            "udf4",
            "udf5",
            "zip_code"
            };
            return Json(new { KeyVals, KeyVals.Keys, hash_columns });
        }

        [HttpPost]//17
        public IActionResult CheckNumberSeries([FromBody] AppHLRLookupReq appsimpleReq)
        {
            var appchkNumSeriesResp = new AppSimplePlanResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvalidParam
            };
            var readHeader = ReadHeader();
            appsimpleReq.APPID = readHeader.AppID;
            appsimpleReq.Version = readHeader.Version;
            var appResp = appML.CheckWebApp(appsimpleReq);
            appchkNumSeriesResp.IsAppValid = appResp.IsAppValid;
            appchkNumSeriesResp.IsVersionValid = appResp.IsVersionValid;
            var plansAPIML = new PlansAPIML(_accessor, _env, false);
            if (appResp.Statuscode == ErrorCodes.One)
            {
                var req = new CommonReq
                {
                    LoginTypeID = LoginType.ApplicationUser,
                    LoginID = appsimpleReq.UserID == 0 ? 1 : appsimpleReq.UserID,
                    CommonStr = appsimpleReq.Mobile
                };
                IAppReportML rML = new ReportML(_accessor, _env);
                var result = (HLRResponseStatus)rML.CheckNumberSeriesExist(req);
                appchkNumSeriesResp.Statuscode = result.Statuscode;
                appchkNumSeriesResp.Msg = result.Msg;
                appchkNumSeriesResp.HLRResponce = new HLRResponseStatus();
                appchkNumSeriesResp.HLRResponce = result;
            }
            else
            {
                appchkNumSeriesResp.Msg = appResp.Msg;
            }
            plansAPIML.LogWebAppReqResp(new CommonReq
            {
                CommonStr = "CheckNumberSeries",
                CommonStr2 = JsonConvert.SerializeObject(appsimpleReq),
                CommonStr3 = JsonConvert.SerializeObject(appchkNumSeriesResp)
            });
            return Json(appchkNumSeriesResp);
        }
        [HttpPost]//18
        public IActionResult CheckIsLookUpFromAPI([FromBody] AppSimplePlanReq appsimpleReq)
        {
            var chkIsLookUpFromAPIResp = new BalanceResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvalidParam,
                IsLookUpFromAPI = false
            };
            var readHeader = ReadHeader();
            appsimpleReq.APPID = readHeader.AppID;
            appsimpleReq.Version = readHeader.Version;
            var appResp = appML.CheckWebApp(appsimpleReq);
            chkIsLookUpFromAPIResp.IsAppValid = appResp.IsAppValid;
            chkIsLookUpFromAPIResp.IsVersionValid = appResp.IsVersionValid;
            chkIsLookUpFromAPIResp.IsLookUpFromAPI = appResp.IsLookUpFromAPI;
            chkIsLookUpFromAPIResp.IsRoffer = ApplicationSetting.IsRoffer;
            chkIsLookUpFromAPIResp.IsDTHInfo = ApplicationSetting.IsDTHInfo;
            chkIsLookUpFromAPIResp.IsMoveToPrepaid = ApplicationSetting.IsMoveToPrepaid;
            chkIsLookUpFromAPIResp.IsMoveToUtility = ApplicationSetting.IsMoveToUtility;
            chkIsLookUpFromAPIResp.IsFlatCommission = ApplicationSetting.ActiveFlatType == FlatTypeMaster.ByAdminOnly;
            if (ApplicationSetting.IsDTHInfoCall && appResp.IsDTHInfo)
                chkIsLookUpFromAPIResp.IsDTHInfoCall = true;
            else
                chkIsLookUpFromAPIResp.IsDTHInfoCall = false;
            var plansAPIML = new PlansAPIML(_accessor, _env, false);
            if (appResp.Statuscode == ErrorCodes.One)
            {

                chkIsLookUpFromAPIResp.Statuscode = ErrorCodes.One;
                chkIsLookUpFromAPIResp.Msg = ErrorCodes.SUCCESS;
            }
            else
            {
                chkIsLookUpFromAPIResp.Msg = appResp.Msg;
            }
            plansAPIML.LogWebAppReqResp(new CommonReq
            {
                CommonStr = "CheckIsLookUpFromAPI",
                CommonStr2 = JsonConvert.SerializeObject(appsimpleReq),
                CommonStr3 = JsonConvert.SerializeObject(chkIsLookUpFromAPIResp)
            });
            return Json(chkIsLookUpFromAPIResp);
        }

        [HttpPost]//19
        public async Task<IActionResult> RefundRequest([FromBody] AppRefundRequest appRefundRequest)
        {
            var RefundReqresp = new AppDoubleFactorResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError

            };
            var readHeader = ReadHeader();
            appRefundRequest.APPID = readHeader.AppID;
            appRefundRequest.Version = readHeader.Version;
            appRefundRequest.UserID = readHeader.UserID;
            appRefundRequest.Session = readHeader.Session;
            appRefundRequest.SessionID = readHeader.SessionID;
            appRefundRequest.LoginTypeID = LoginType.ApplicationUser;
            var appResp = appML.CheckWebAppSession(appRefundRequest);
            RefundReqresp.IsAppValid = appResp.IsAppValid;
            RefundReqresp.IsVersionValid = appResp.IsVersionValid;
            RefundReqresp.IsPasswordExpired = appResp.IsPasswordExpired;
            var plansAPIML = new PlansAPIML(_accessor, _env, false);
            if (appResp.Statuscode == ErrorCodes.One)
            {
                if (!appResp.IsPasswordExpired)
                {
                    IAppReportML appReportML = new ReportML(_accessor, _env, false);
                    var filter = new RefundRequestReq
                    {
                        refundRequest = new RefundRequest
                        {
                            TID = appRefundRequest.TID,
                            RPID = appRefundRequest.TransactionID,
                            UserID = appRefundRequest.UserID,
                            OTP = appRefundRequest.OTP,
                            IsResend = appRefundRequest.IsResend
                        },
                        LoginTypeID = LoginType.ApplicationUser,
                        LoginID = appRefundRequest.UserID
                    };
                    var resRR = await appReportML.MarkDisputeApp(filter).ConfigureAwait(false);
                    RefundReqresp.Statuscode = resRR.Statuscode;
                    RefundReqresp.Msg = resRR.Msg;
                    RefundReqresp.IsOTPRequired = resRR.IsOTPRequired;
                }
            }
            else
            {
                RefundReqresp.Msg = appResp.Msg;
            }
            plansAPIML.LogWebAppReqResp(new CommonReq
            {
                CommonStr = "RefundRequest",
                CommonStr2 = JsonConvert.SerializeObject(appRefundRequest),
                CommonStr3 = JsonConvert.SerializeObject(RefundReqresp)
            });
            return Json(RefundReqresp);
        }
        ////Forget Password////

        [HttpPost]//20
        public async Task<IActionResult> ForgetPassword([FromBody] LoginRequest loginRequest)
        {
            var appResponse = new AppResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            var readHeader = ReadHeader();
            loginRequest.APPID = readHeader.AppID;
            loginRequest.Version = readHeader.Version;
            loginRequest.Domain = readHeader.Domain;
            appResponse = appML.CheckWebApp(loginRequest);
            if (appResponse.Statuscode == ErrorCodes.One)
            {
                var loginDetail = new LoginDetail
                {
                    LoginTypeID = LoginType.ApplicationUser,
                    LoginMobile = loginRequest.UserID
                };
                if (!loginDetail.LoginTypeID.In(LoginType.ApplicationUser, LoginType.CustomerCare, LoginType.Employee))
                {
                    appResponse.Msg = "Choose user login type!";
                    return Json(appResponse);
                }
                if (!Validate.O.IsMobile(loginDetail.LoginMobile))
                {
                    loginDetail.Prefix = Validate.O.Prefix(loginDetail.LoginMobile);
                    if (Validate.O.IsNumeric(loginDetail.Prefix))
                        return Json(appResponse);
                    string loginID = Validate.O.LoginID(loginDetail.LoginMobile);
                    if (!Validate.O.IsNumeric(loginID))
                    {
                        return Json(appResponse);
                    }
                    loginDetail.LoginID = Convert.ToInt32(loginID);
                    loginDetail.LoginMobile = "";
                }
                if ((loginRequest.Domain ?? "") == "")
                {
                    appResponse.Msg = "(ND)Request couldn't be identified!";
                    return Json(appResponse);
                }
                ILoginML _loginML = new LoginML(_accessor, _env, loginRequest.Domain);
                await Task.Delay(0);
                var responseStatus = _loginML.ForgetFromApp(loginDetail);
                appResponse.Statuscode = responseStatus.Statuscode;
                appResponse.Msg = responseStatus.Msg;
            }
            new PlansAPIML(_accessor, _env, false).LogWebAppReqResp(new CommonReq
            {
                CommonStr = "ForgetPassword",
                CommonStr2 = JsonConvert.SerializeObject(loginRequest),
                CommonStr3 = JsonConvert.SerializeObject(appResponse)
            });
            return Json(appResponse);
        }


        #region Company Profile
        [HttpPost]//21
        public async Task<IActionResult> GetCompanyProfile()
        {
            var appResp = new CompanyProfileDetail
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            var readHeader = ReadHeader();
            var apprequest = new AppRequest
            {
                LoginTypeID = LoginType.ApplicationUser,
                APPID = readHeader.AppID,
                Version = readHeader.Version
            };

            var appResponse = appML.CheckWebApp(apprequest);
            appResp.Statuscode = appResponse.Statuscode;
            appResp.Msg = appResponse.Msg;
            if (appResp.Statuscode == ErrorCodes.One)
            {
                ILoginML _loginML = new LoginML(_accessor, _env, readHeader.Domain);
                var webInfo = _loginML.GetWebsiteForDomain();
                if (appResp.Statuscode == ErrorCodes.One)
                {
                    IUserAPPML ml = new UserML(_accessor, _env, false);
                    appResp = ml.GetCompanyProfileApp(webInfo.WID);
                }
            }
            new PlansAPIML(_accessor, _env, false).LogWebAppReqResp(new CommonReq
            {
                CommonStr = "GetCompanyProfile",
                CommonStr2 = JsonConvert.SerializeObject(apprequest),
                CommonStr3 = JsonConvert.SerializeObject(appResp)
            });
            return Json(appResp);
        }

        [HttpPost]//22
        public async Task<IActionResult> UserSubscriptionApp([FromBody] UserSubscriptionApp UserSubApp)
        {
            var appResponse = new AppResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            var readHeader = ReadHeader();
            var appRequest = new AppRequest
            {
                APPID = readHeader.AppID,
                Version = readHeader.Version
            };
            appResponse = appML.CheckWebApp(appRequest);
            if (appResponse.Statuscode == ErrorCodes.One)
            {
                var getIntouch = new GetIntouch
                {
                    Name = UserSubApp.Name,
                    EmailID = UserSubApp.EmailID,
                    Message = UserSubApp.Message,
                    MobileNo = UserSubApp.MobileNo,
                    RequestPage = "B2CContactUs",
                    WID = appResponse.CheckID
                };

                ILoginML _loginML = new LoginML(_accessor, _env, false);
                await Task.Delay(0).ConfigureAwait(false);
                var responseStatus = _loginML.UserSubscriptionApp(getIntouch);
                appResponse.Statuscode = responseStatus.Statuscode;
                appResponse.Msg = responseStatus.Msg;
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "UserSubscription",
                CommonStr2 = JsonConvert.SerializeObject(UserSubApp),
                CommonStr3 = JsonConvert.SerializeObject(appResponse)
            });
            return Json(appResponse);
        }

        [HttpPost]//23
        public async Task<IActionResult> GetProfile()
        {
            var appResp = new WebAppUserProfileResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            var readHeader = ReadHeader();
            var appRequest = new AppSessionReq
            {
                APPID = readHeader.AppID ?? "",
                LoginTypeID = LoginType.ApplicationUser,
                UserID = readHeader.UserID,
                SessionID = readHeader.SessionID,
                Version = readHeader.Version ?? "",
                Session = readHeader.Session
            };
            var appResponse = appML.CheckWebAppSession(appRequest);
            appResp.Statuscode = appResponse.Statuscode;
            appResp.Msg = appResponse.Msg;
            appResp.IsAppValid = appResponse.IsAppValid;
            appResp.IsVersionValid = appResponse.IsVersionValid;
            appResp.IsPasswordExpired = appResponse.IsPasswordExpired;
            appResp.Statuscode = appResponse.Statuscode;
            if (appResp.Statuscode == ErrorCodes.One)
            {
                IUserAPPML userML = new UserML(_accessor, _env, false);
                await Task.Delay(0).ConfigureAwait(false);
                var userInfo = userML.GetEditUserForApp(new CommonReq
                {
                    LoginTypeID = LoginType.ApplicationUser,
                    LoginID = readHeader.UserID,
                    CommonInt = readHeader.UserID
                });
                if (!string.IsNullOrEmpty(userInfo.Name))
                {
                    appResp.Name = userInfo.Name;
                    appResp.OutletName = userInfo.OutletName;
                    appResp.EmailID = userInfo.EmailID;
                    appResp.MobileNo = userInfo.MobileNo;
                    appResp.AlternateMobile = userInfo.AlternateMobile;
                    appResp.DOB = userInfo.DOB;
                    appResp.Pincode = userInfo.Pincode;
                    appResp.PAN = userInfo.PAN;
                    appResp.City = userInfo.City;
                    appResp.State = userInfo.State;
                    appResp.Address = userInfo.Address;
                    appResp.State = userInfo.StateName;
                    appResp.Aadhar = userInfo.AADHAR;
                    appResp.ProfilePic = userInfo.ProfilePic;
                    
                }
            }
            return Json(appResp);
        }
        [HttpPost]//24
        public IActionResult GetMembershipType()
        {
            var appResp = new WebMemberTypeModel
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            var readHeader = ReadHeader();
            var appRequest = new AppSessionReq
            {
                APPID = readHeader.AppID ?? "",
                LoginTypeID = LoginType.ApplicationUser,
                UserID = readHeader.UserID,
                SessionID = readHeader.SessionID,
                Version = readHeader.Version ?? "",
                Session = readHeader.Session
            };
            var appResponse = appML.CheckWebAppSession(appRequest);
            appResp.Statuscode = appResponse.Statuscode;
            appResp.Msg = appResponse.Msg;
            appResp.IsAppValid = appResponse.IsAppValid;
            appResp.IsVersionValid = appResponse.IsVersionValid;
            appResp.IsPasswordExpired = appResponse.IsPasswordExpired;
            appResp.Statuscode = appResponse.Statuscode;
            if (appResp.Statuscode == ErrorCodes.One)
            {
                IMembershipML memberML = new MembershipML(_accessor, _env, false);
                appResp.MemberTypes = memberML.GetB2CMemberShipType(readHeader.UserID);
            }
            if (ApplicationSetting.IsmultipleMembershipAllowed)
            {
                appResp.MemberTypes = appResp.MemberTypes.Select(w => {
                    w.IsIDActive = false;
                    return w;
                });

            }
            return Json(appResp);
        }
        [HttpPost]//25
        public IActionResult PurchaseMemberShip([FromBody] CommonWeRequest commonWeRequest)
        {
            var appResp = new AppResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            var readHeader = ReadHeader();
            var appRequest = new AppSessionReq
            {
                APPID = readHeader.AppID ?? "",
                LoginTypeID = LoginType.ApplicationUser,
                UserID = readHeader.UserID,
                SessionID = readHeader.SessionID,
                Version = readHeader.Version ?? "",
                Session = readHeader.Session
            };
            var appResponse = appML.CheckWebAppSession(appRequest);
            appResp.Statuscode = appResponse.Statuscode;
            appResp.Msg = appResponse.Msg;
            appResp.IsAppValid = appResponse.IsAppValid;
            appResp.IsVersionValid = appResponse.IsVersionValid;
            appResp.IsPasswordExpired = appResponse.IsPasswordExpired;
            appResp.Statuscode = appResponse.Statuscode;
            if (appResp.Statuscode == ErrorCodes.One)
            {
                IMembershipML memberML = new MembershipML(_accessor, _env, false);
                var ress = memberML.GetMemberShip(new CommonReq
                {
                    LoginID = readHeader.UserID,
                    CommonInt = commonWeRequest.ID
                });
                appResp.Statuscode = ress.Statuscode;
                appResp.Msg = ress.Msg;
            }
            return Json(appResp);
        }

        [HttpPost]//26
        public IActionResult GetAllCoupons()
        {
            IEnumerable<B2CMemberCouponDetail> res = new List<B2CMemberCouponDetail>();
            var appResp = new AppResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            var readHeader = ReadHeader();
            var appRequest = new AppSessionReq
            {
                APPID = readHeader.AppID ?? "",
                LoginTypeID = LoginType.ApplicationUser,
                UserID = readHeader.UserID,
                SessionID = readHeader.SessionID,
                Version = readHeader.Version ?? "",
                Session = readHeader.Session
            };
            var appResponse = appML.CheckWebAppSession(appRequest);
            appResp.Statuscode = appResponse.Statuscode;
            appResp.Msg = appResponse.Msg;
            appResp.IsAppValid = appResponse.IsAppValid;
            appResp.IsVersionValid = appResponse.IsVersionValid;
            appResp.IsPasswordExpired = appResponse.IsPasswordExpired;
            appResp.Statuscode = appResponse.Statuscode;
            if (appResp.Statuscode == ErrorCodes.One)
            {
                IMembershipML memberML = new MembershipML(_accessor, _env, false);
                res = memberML.GetB2CCoupon(readHeader.UserID);
            }
            return Json(res);
        }
        [HttpPost]//27
        public IActionResult RedeemCoupon([FromBody] CommonWeRequest commonWeRequest)
        {
            var appResp = new AppResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            var readHeader = ReadHeader();
            var appRequest = new AppSessionReq
            {
                APPID = readHeader.AppID ?? "",
                LoginTypeID = LoginType.ApplicationUser,
                UserID = readHeader.UserID,
                SessionID = readHeader.SessionID,
                Version = readHeader.Version ?? "",
                Session = readHeader.Session
            };
            var appResponse = appML.CheckWebAppSession(appRequest);
            appResp.Statuscode = appResponse.Statuscode;
            appResp.Msg = appResponse.Msg;
            appResp.IsAppValid = appResponse.IsAppValid;
            appResp.IsVersionValid = appResponse.IsVersionValid;
            appResp.IsPasswordExpired = appResponse.IsPasswordExpired;
            appResp.Statuscode = appResponse.Statuscode;
            if (appResp.Statuscode == ErrorCodes.One)
            {
                IMembershipML memberML = new MembershipML(_accessor, _env, false);
                var ress = memberML.RedeemCoupon(readHeader.UserID, commonWeRequest.StringID);
                appResp.Statuscode = ress.Statuscode;
                appResp.Msg = ress.Msg;
            }
            return Json(appResp);
        }
        [HttpPost]//28
        public async Task<IActionResult> UploadProfilePic(IFormFile file)
        {
            var appResp = new AppResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            var appRequest = new AppSessionReq();
            var readHeader = ReadHeader();
            appRequest.APPID = readHeader.AppID ?? "";
            appRequest.LoginTypeID = LoginType.ApplicationUser;
            appRequest.UserID = readHeader.UserID;
            appRequest.SessionID = readHeader.SessionID;
            appRequest.Version = readHeader.Version ?? "";
            appRequest.Session = readHeader.Session;
            appResp = appML.CheckWebAppSession(appRequest);
            if (appResp.Statuscode == ErrorCodes.One)
            {
                if (!appResp.IsPasswordExpired)
                {
                    await Task.Delay(0).ConfigureAwait(false);
                    IResourceML ml = new ResourceML(_accessor, _env);
                    var res = ml.UploadProfile(file, appRequest.UserID, appRequest.LoginTypeID);
                    appResp.Statuscode = res.Statuscode;
                    appResp.Msg = res.Msg;
                }
            }
            new PlansAPIML(_accessor, _env, false).LogWebAppReqResp(new CommonReq
            {
                CommonStr = "UploadProfile",
                CommonStr2 = JsonConvert.SerializeObject(appRequest),
                CommonStr3 = JsonConvert.SerializeObject(appResp)
            });
            return Json(appResp);
        }
        [HttpGet]//29
        public IActionResult GetProfilePic(int i, string n)
        {

            Byte[] rgbValues = null;
            if (System.IO.File.Exists(DOCType.ProfileImagePath + i + ".png"))
            {

                rgbValues = System.IO.File.ReadAllBytes(DOCType.ProfileImagePath + i + ".png");   // You can use your own method over here.         

            }
            else
            {
                Bitmap image = new Bitmap(_env.WebRootPath + "/images/avatar/defualt.png", true);
                Graphics g = Graphics.FromImage(image);
                int x, y;

                // Loop through the images pixels to reset color.

                if (!String.IsNullOrEmpty(n))
                {


                    string charFirstName = ""; string charLastName = "";
                    var NameArray = n.Split(" ");
                    for (x = 0; x < image.Width; x++)
                    {
                        for (y = 0; y < image.Height; y++)
                        {
                            Color pixelColor = image.GetPixel(x, y);
                            if (NameArray.Length == 1)
                            {
                                Color newColor = Color.FromArgb(pixelColor.R, 255, 80, 80);
                                image.SetPixel(x, y, newColor);
                            }
                            else if (NameArray.Length > 1)
                            {
                                Color newColor = Color.FromArgb(pixelColor.R, 0, 85, 128);
                                image.SetPixel(x, y, newColor);
                            }

                        }
                    }
                    charFirstName = NameArray[0].ToCharArray()[0].ToString();
                    if (NameArray.Length > 1)
                    {
                        charLastName = NameArray[NameArray.Length - 1].ToCharArray()[0].ToString();
                    }

                    if (NameArray.Length == 1)
                    {
                        RectangleF rectf = new RectangleF(45, 50, 90, 50);

                        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                        g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;

                        g.DrawString(charFirstName + (charLastName == "" ? "" : charLastName), new Font("Serif", 24), Brushes.White, rectf);
                    }

                    else if (NameArray.Length > 1)
                    {
                        RectangleF rectf = new RectangleF(40, 50, 90, 50);
                        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                        g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;

                        g.DrawString(charFirstName + (charLastName == "" ? "" : charLastName), new Font("Serif", 24), Brushes.White, rectf);
                    }




                }
                //image.Save(_env.WebRootPath + "/images/avatar/new.png");
                using (var stream = new MemoryStream())
                {
                    image.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                    rgbValues = stream.ToArray();
                    image.Dispose();
                }


            }
            return File(rgbValues, "Image/png", "ProfilePic.png");

        }
        [HttpPost]//30
        public async Task<IActionResult> UpdateProfile([FromBody] AppUserCreate appUserEdit)
        {
            var appResp = new AppResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            appUserEdit.LoginTypeID = LoginType.ApplicationUser;
            if (appUserEdit.LoginTypeID == LoginType.ApplicationUser)
            {
                var readHeader = ReadHeader();
                var appRequest = new AppSessionReq
                {
                    APPID = readHeader.AppID ?? "",
                    LoginTypeID = appUserEdit.LoginTypeID,
                    UserID = readHeader.UserID,
                    SessionID = readHeader.SessionID,
                    Version = readHeader.Version ?? "",
                    Session = readHeader.Session
                };
                appResp = appML.CheckWebAppSession(appRequest);

                if (appResp.Statuscode == ErrorCodes.One)
                {
                    if (!appResp.IsPasswordExpired)
                    {
                        appUserEdit.editUser.LT = appUserEdit.LoginTypeID;
                        appUserEdit.editUser.UserID = readHeader.UserID;

                        IUserAPPML userML = new UserML(_accessor, _env, false);
                        await Task.Delay(0).ConfigureAwait(false);
                        var res = userML.UpdateUserFromAppB2C(appUserEdit.editUser);
                        appResp.Statuscode = res.Statuscode;
                        appResp.Msg = res.Msg;
                    }
                }
            }
            new PlansAPIML(_accessor, _env, false).LogWebAppReqResp(new CommonReq
            {
                CommonStr = "UpdateProfile",
                CommonStr2 = JsonConvert.SerializeObject(appUserEdit),
                CommonStr3 = JsonConvert.SerializeObject(appResp)
            });
            return Json(appResp);
        }

        [HttpPost]//31
        public IActionResult GetPromoCodeByOpTypeOID([FromBody] PromoCode promocode)
        {
            var PromoCodeResp = new PromoCode
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            var readHeader = ReadHeader();
            var appRequest = new AppRequest
            {
                APPID = readHeader.AppID ?? "",
                LoginTypeID = promocode.LT,
                Version = readHeader.Version ?? "",


            };
            var appResp = appML.CheckWebApp(appRequest);

            if (appResp.Statuscode == ErrorCodes.One)
            {
                if (!appResp.IsPasswordExpired)
                {
                    promocode.LT = LoginType.ApplicationUser;
                    IPromoCodeML promoCodeML = new PromoCodeML(_accessor, _env, false);
                    var res = promoCodeML.GetPromoCodeByOpTypeOID(promocode);
                    PromoCodeResp.lstPromoCode = res;
                }
            }
            new PlansAPIML(_accessor, _env, false).LogWebAppReqResp(new CommonReq
            {
                CommonStr = "GetPromoCodeByOpTypeOID",
                CommonStr2 = JsonConvert.SerializeObject(PromoCodeResp),
                CommonStr3 = JsonConvert.SerializeObject(PromoCodeResp)
            });
            return Json(PromoCodeResp);
        }

        #endregion
        #region PaymentGateway
        [HttpPost]//32
        public IActionResult GetPGDetail([FromBody] PGWebRequestModel requestModel)
        {
            var appResp = new PGInitiatePGResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
                
            };
            AppSessionReq pGInitiatePGRequest = new AppSessionReq();
            var readHeader = ReadHeader();
            pGInitiatePGRequest.LoginTypeID = LoginType.ApplicationUser;
            pGInitiatePGRequest.APPID = readHeader.AppID;
            pGInitiatePGRequest.Version = readHeader.Version;
            pGInitiatePGRequest.UserID = readHeader.UserID;
            pGInitiatePGRequest.SessionID = readHeader.SessionID;
            pGInitiatePGRequest.Session = readHeader.Session;
            var host = _accessor.HttpContext.Request.Host.Value;
            string RequestReferer = _accessor.HttpContext.Request.Headers["Referer"];
            if (!string.IsNullOrEmpty(RequestReferer))
            {
                RequestReferer = RequestReferer.Split("://")[1];
                RequestReferer = RequestReferer.Split("/")[0];
            }
            var appResponse = appML.CheckWebAppSession(pGInitiatePGRequest);
            if (appResponse.Statuscode == ErrorCodes.One)
            {
                if (!appResponse.B2CDomain.Equals(RequestReferer ?? string.Empty))
                {
                    return BadRequest("Do not Refresh");
                }
                else
                {
                    //Uri address8 = new Uri(host);
                    //if (!(address8.Scheme == Uri.UriSchemeHttps) || !(address8.Scheme == Uri.UriSchemeHttp))
                    host = Convert.ToString("https://" + host);
                }
                if (!appResponse.IsPasswordExpired)
                {
                    IPaymentGatewayML gatewayML = new PaymentGatewayML(_accessor, _env);
                    int pg = 0;
                    if (string.IsNullOrEmpty(requestModel.vpa))
                    {
                        var pgs = (requestModel.id > 0 && requestModel.a > 0 && requestModel.w > 0 ? gatewayML.GetPGDetailsUser(appResponse.CheckID, false) : null);
                        if (pgs != null)
                        {
                            if (pgs.Any())
                            {
                                var pgTempList = pgs.Where(x => x.AgentType == PGAgentType.CustomerOnly || x.AgentType == PGAgentType.Both).Select(x => x.ID).ToList();
                                if (pgTempList != null)
                                {
                                    if (pgTempList.Count > 0)
                                    {
                                        pg = pgTempList.First();
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        var pgs = (requestModel.id > 0 && requestModel.a > 0 && requestModel.w > 0 ? gatewayML.GetPGDetails(appResponse.CheckID, true) : null);
                        if (pgs != null)
                        {
                            if (pgs.Any())
                            {
                                pg = pgs.Where(x => x.AgentType == PGAgentType.Both).Select(x => x.UPGID).First();
                            }
                        }
                    }
                    if (ApplicationSetting.IsAddMoneyEnable && ApplicationSetting.IsUPI && pg==4)
                    {
                        IUPIPaymentML upi= new PaymentGatewayML(_accessor, _env);
                        appResp.pGModelforUpi = upi.InitiateUPIPaymentForWeb(pGInitiatePGRequest.UserID, requestModel.a, pg, requestModel.id, 1, requestModel.vpa);
                        appResp.Statuscode = appResp.pGModelforUpi.Statuscode;
                        appResp.Msg = appResp.pGModelforUpi.Msg;
                        appResp.pGModelForWeb= new AppCode.Model.Paymentgateway.PGModelForRedirection();
                        appResp.pGModelForWeb.PGType = 3;
                        appResp.pGModelForWeb.TID = appResp.pGModelforUpi.CommonInt;
                        appResp.Statuscode = appResp.pGModelforUpi.Statuscode;


                    }
                    if (ApplicationSetting.IsAddMoneyEnable && ApplicationSetting.IsPaymentGatway)
                    {
                        appResp.pGModelForWeb = gatewayML.IntiatePGTransactionForWeb(pGInitiatePGRequest.UserID, requestModel.a, pg, requestModel.id, 1, host, requestModel.vpa);
                        appResp.Statuscode = appResp.pGModelForWeb.Statuscode;
                        appResp.Msg = appResp.pGModelForWeb.Msg;
                    }
                    

                }


            }
            return Json(appResp);
        }
        [HttpPost]//33
        public IActionResult CheckPGStatus([FromBody] PGStatusCheckRequestModel requestModel)
        {
            var appResp = new PGInitiatePGResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            AppSessionReq pGInitiatePGRequest = new AppSessionReq();
            var readHeader = ReadHeader();
            pGInitiatePGRequest.LoginTypeID = LoginType.ApplicationUser;
            pGInitiatePGRequest.APPID = readHeader.AppID;
            pGInitiatePGRequest.Version = readHeader.Version;
            pGInitiatePGRequest.UserID = readHeader.UserID;
            pGInitiatePGRequest.SessionID = readHeader.SessionID;
            pGInitiatePGRequest.Session = readHeader.Session;
            string RequestReferer = _accessor.HttpContext.Request.Headers["Referer"];
            if (!string.IsNullOrEmpty(RequestReferer))
            {
                RequestReferer = RequestReferer.Split("://")[1];
                RequestReferer = RequestReferer.Split("/")[0];
            }
            var appResponse = appML.CheckWebAppSession(pGInitiatePGRequest);
            if (appResponse.Statuscode == ErrorCodes.One)
            {
                if (!appResponse.B2CDomain.Equals(RequestReferer ?? string.Empty))
                {
                    return BadRequest("Do not Refresh");
                }

                if (!appResponse.IsPasswordExpired)
                {
                    IPaymentGatewayML gatewayML = new PaymentGatewayML(_accessor, _env);
                    var pgRes = gatewayML.CheckPGTransactionStatus(new CommonReq
                    {
                        LoginTypeID = 1,
                        LoginID = pGInitiatePGRequest.UserID,
                        CommonInt = requestModel.OrderID
                    });
                    if (pgRes.Statuscode != ErrorCodes.Minus1)
                    {
                        appResp.Statuscode = ErrorCodes.One;
                    }
                    appResp.Msg = pgRes.Msg;
                    appResp.Status = pgRes.Statuscode;
                }
            }
            return Json(appResp);
        }

        #endregion

        [HttpPost]
        public IActionResult ChangePinOrPassword([FromBody] AppChangePRequest appSessionReq)
        {
            var appResponse = new AppResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            var readHeader = ReadHeader();
            appSessionReq.APPID = readHeader.AppID;
            appSessionReq.Version = readHeader.Version;
            appSessionReq.UserID = readHeader.UserID;
            appSessionReq.Session = readHeader.Session;
            appSessionReq.SessionID = readHeader.SessionID;
            appSessionReq.LoginTypeID = LoginType.ApplicationUser;
            appResponse = appML.CheckWebAppSession(appSessionReq);
            if (appResponse.Statuscode == ErrorCodes.One)
            {
                IUserAPPML userML = new UserML(_accessor, _env, false);
                var changePassword = new ChangePassword
                {
                    OldPassword = appSessionReq.OldP,
                    NewPassword = appSessionReq.NewP,
                    SessID = appSessionReq.SessionID
                };
                AlertReplacementModel _res = new AlertReplacementModel();
                if (!appSessionReq.IsPin)
                {
                    _res = userML.ChangePassword(changePassword, appSessionReq.LoginTypeID, appSessionReq.UserID);
                    appResponse.Statuscode = _res.Statuscode;
                    appResponse.Msg = _res.Msg;
                    appResponse.CheckID = 0;
                    appResponse.MobileNo = "";
                    appResponse.EmailID = "";
                }
                else
                {

                    _res = userML.ChangePin(changePassword, appSessionReq.LoginTypeID, appSessionReq.UserID);
                    appResponse.Statuscode = _res.Statuscode;
                    appResponse.Msg = _res.Msg;
                    appResponse.CheckID = 0;
                    appResponse.MobileNo = "";
                    appResponse.EmailID = "";
                }
            }
            new PlansAPIML(_accessor, _env, false).LogWebAppReqResp(new CommonReq
            {
                CommonStr = "ChangePinOrPassword",
                CommonStr2 = JsonConvert.SerializeObject(appSessionReq),
                CommonStr3 = JsonConvert.SerializeObject(appResponse)
            });
            return Json(appResponse);
        }

        [HttpPost]
        public async Task<IActionResult> GetPaymentMode()
        {
            var paymentmoderesponse = new OperatorResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvaildSession,

            };
            var readHeader = ReadHeader();
            var paymentmode = new AppSessionReq
            {
                APPID = readHeader.AppID,
                Version = readHeader.Version,
                Session = readHeader.Session,
                SessionID = readHeader.SessionID,
                LoginTypeID = LoginType.ApplicationUser,
                UserID = readHeader.UserID
            };

            var appResp = appML.CheckWebAppSession(paymentmode);
            paymentmoderesponse.IsAppValid = appResp.IsAppValid;
            paymentmoderesponse.IsVersionValid = appResp.IsVersionValid;
            paymentmoderesponse.IsPasswordExpired = appResp.IsPasswordExpired;
            paymentmoderesponse.Statuscode = appResp.Statuscode;
            paymentmoderesponse.IsLookUpFromAPI = appResp.IsLookUpFromAPI;
            paymentmoderesponse.IsDTHInfoCall = ApplicationSetting.IsDTHInfoCall;
            paymentmoderesponse.IsShowPDFPlan = ApplicationSetting.IsShowPDFPlan;
            if (appResp.Statuscode == ErrorCodes.One)
            {
                paymentmoderesponse.Statuscode = appResp.Statuscode;

                if (!appResp.IsPasswordExpired)
                {

                    var opNumberData = new OpNumberData
                    {

                        Operators = new OperatorML(_accessor, _env, false).GetPaymentModesOp(paymentmode.UserID)

                    };
                    await Task.Delay(0);
                    IUserML userML = new UserML(_accessor, _env, false);
                    paymentmoderesponse.Statuscode = ErrorCodes.One;
                    paymentmoderesponse.Msg = RechargeRespType._SUCCESS;
                    paymentmoderesponse.Data = opNumberData;
                }
            }
            else
            {
                paymentmoderesponse.Msg = appResp.Msg;
            }
            return Json(paymentmoderesponse);
        }

        #region WalletToWallet
        [HttpPost]
        public async Task<IActionResult> GetUserInfo([FromBody] CallMeReqApp callmereqapp)//for wallet
        {
            var wtw = new WebWTWUserInfo
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvaildSession,

            };
            var readHeader = ReadHeader();
            var pm = new AppSessionReq
            {
                APPID = readHeader.AppID,
                Version = readHeader.Version,
                Session = readHeader.Session,
                SessionID = readHeader.SessionID,
                LoginTypeID = LoginType.ApplicationUser,
                UserID = readHeader.UserID
            };

            var appResp = appML.CheckWebAppSession(pm);
            wtw.IsAppValid = appResp.IsAppValid;
            wtw.IsVersionValid = appResp.IsVersionValid;
            wtw.IsPasswordExpired = appResp.IsPasswordExpired;
            wtw.Statuscode = appResp.Statuscode;
            wtw.IsLookUpFromAPI = appResp.IsLookUpFromAPI;
            //wtw.IsDTHInfoCall = ApplicationSetting.IsDTHInfoCall;
            wtw.IsShowPDFPlan = ApplicationSetting.IsShowPDFPlan;
            if (appResp.Statuscode == ErrorCodes.One)
            {
                wtw.Statuscode = appResp.Statuscode;

                if (!appResp.IsPasswordExpired)
                {
                    var req = new CommonReq
                    {
                        LoginTypeID = pm.LoginTypeID,
                        LoginID = pm.UserID,
                        CommonStr = callmereqapp.mobileNo
                    };

                    await Task.Delay(0);
                    IUserML userML = new UserML(_accessor, _env, false);
                    var info = userML.GetUserByMobile(req);
                    if(info.StatusCode== ErrorCodes.One)
                    {
                        if (info.UserID == pm.UserID)
                        {
                            wtw.Statuscode = ErrorCodes.Minus1;
                            wtw.Msg ="Invalid User.";
                        }
                        else
                        {
                            wtw.Msg = info.Msg;
                            wtw.wtwuserinfo = info;
                        }
                        
                    }
                    else
                    {
                        wtw.Statuscode = info.StatusCode;
                        wtw.Msg = info.Msg;
                    }
                    

                    
                   
                }
            }
            return Json(wtw);
        }

        [HttpPost]
        public async Task<IActionResult> WalletToWalletTransfer([FromBody] PGWebRequestModel requestModel)//for wallet
        {
            var wtw = new WebWTWUserInfo
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvaildSession,

            };
            var readHeader = ReadHeader();
            var paymentmode = new AppSessionReq
            {
                APPID = readHeader.AppID,
                Version = readHeader.Version,
                Session = readHeader.Session,
                SessionID = readHeader.SessionID,
                LoginTypeID = LoginType.ApplicationUser,
                UserID = readHeader.UserID
            };

            if(paymentmode.UserID== requestModel.id)
            {
                wtw.Msg = "Invalid User";
                return Json(wtw);
            }
            var appResp = appML.CheckWebAppSession(paymentmode);
            wtw.IsAppValid = appResp.IsAppValid;
            wtw.IsVersionValid = appResp.IsVersionValid;
            wtw.IsPasswordExpired = appResp.IsPasswordExpired;
            wtw.Statuscode = appResp.Statuscode;
            wtw.IsLookUpFromAPI = appResp.IsLookUpFromAPI;
            //wtw.IsDTHInfoCall = ApplicationSetting.IsDTHInfoCall;
            wtw.IsShowPDFPlan = ApplicationSetting.IsShowPDFPlan;
            if (appResp.Statuscode == ErrorCodes.One)
            {
                wtw.Statuscode = appResp.Statuscode;

                if (!appResp.IsPasswordExpired)
                {
                    var req = new CommonReq
                    {
                        LoginTypeID = paymentmode.LoginTypeID,
                        LoginID = paymentmode.UserID,
                        UserID= requestModel.id,
                        CommonDecimal= requestModel.a,
                        CommonStr= requestModel.vpa
                    };

                    await Task.Delay(0);
                    IUserML userML = new UserML(_accessor, _env, false);
                    var info = userML._WallettoWallet(req);
                    
                    return Json(info);

                }
            }
            return Json(wtw);
        }
        #endregion




        [HttpPost]
        public async Task<IActionResult>  UploadAdvertisement(FileUploadAdvertisementRequest data)
        {
            var wtw = new WebWTWUserInfo
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvaildSession,

            };
            var readHeader = ReadHeader();
            var para = new AppSessionReq
            {
                APPID = readHeader.AppID,
                Version = readHeader.Version,
                Session = readHeader.Session,
                SessionID = readHeader.SessionID,
                LoginTypeID = LoginType.ApplicationUser,
                UserID = readHeader.UserID
            };

            
            var appResp = appML.CheckWebAppSession(para);
            wtw.IsAppValid = appResp.IsAppValid;
            wtw.IsVersionValid = appResp.IsVersionValid;
            wtw.IsPasswordExpired = appResp.IsPasswordExpired;
            wtw.Statuscode = appResp.Statuscode;
            wtw.IsLookUpFromAPI = appResp.IsLookUpFromAPI;
            //wtw.IsDTHInfoCall = ApplicationSetting.IsDTHInfoCall;
            wtw.IsShowPDFPlan = ApplicationSetting.IsShowPDFPlan;
            if (appResp.Statuscode == ErrorCodes.One)
            {
                wtw.Statuscode = appResp.Statuscode;

                if (!appResp.IsPasswordExpired)
                {
                    

                    await Task.Delay(0);
                    IAdvertisementML adv = new AdvertisementML(_accessor, _env, false);
                    data.UserID = para.UserID;
                    var info = adv.UpdateAdvertisement(data);
                    return Json(info);

                }
            }
            return Json(wtw);




            return null;
        }

        [HttpPost]
        public async Task<IActionResult> GetAdvertisementPackage()
        {
            var wtw = new WebWTWUserInfo
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvaildSession,

            };
            var readHeader = ReadHeader();
            var para = new AppSessionReq
            {
                APPID = readHeader.AppID,
                Version = readHeader.Version,
                Session = readHeader.Session,
                SessionID = readHeader.SessionID,
                LoginTypeID = LoginType.ApplicationUser,
                UserID = readHeader.UserID
            };


            var appResp = appML.CheckWebAppSession(para);
            wtw.IsAppValid = appResp.IsAppValid;
            wtw.IsVersionValid = appResp.IsVersionValid;
            wtw.IsPasswordExpired = appResp.IsPasswordExpired;
            wtw.Statuscode = appResp.Statuscode;
            wtw.IsLookUpFromAPI = appResp.IsLookUpFromAPI;
         
            if (appResp.Statuscode == ErrorCodes.One)
            {
                wtw.Statuscode = appResp.Statuscode;

                if (!appResp.IsPasswordExpired)
                {
                    await Task.Delay(0);
                    IAdvertisementML adv = new AdvertisementML(_accessor, _env, false);
                    var info = adv.GetAdvertisementPackage();

                    var ret = new
                    {
                        Statuscode = ErrorCodes.One,
                        Msg = ErrorCodes.SUCCESS,
                        package = info

                    };
                    return Json(ret);
                }
            }
            return Json(wtw);




            return null;
        }
        [HttpPost]
        public async Task<IActionResult> GetAdvertisementList()
        {
            var wtw = new WebWTWUserInfo
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvaildSession,

            };
            var readHeader = ReadHeader();
            var para = new AppSessionReq
            {
                APPID = readHeader.AppID,
                Version = readHeader.Version,
                Session = readHeader.Session,
                SessionID = readHeader.SessionID,
                LoginTypeID = LoginType.ApplicationUser,
                UserID = readHeader.UserID
            };


            var appResp = appML.CheckWebAppSession(para);
            wtw.IsAppValid = appResp.IsAppValid;
            wtw.IsVersionValid = appResp.IsVersionValid;
            wtw.IsPasswordExpired = appResp.IsPasswordExpired;
            wtw.Statuscode = appResp.Statuscode;
            wtw.IsLookUpFromAPI = appResp.IsLookUpFromAPI;

            if (appResp.Statuscode == ErrorCodes.One)
            {
                wtw.Statuscode = appResp.Statuscode;

                if (!appResp.IsPasswordExpired)
                {
                    await Task.Delay(0);
                    IAdvertisementML adv = new AdvertisementML(_accessor, _env, false);
                    AdvertisementRequest advertisementrequest = new AdvertisementRequest();
                    advertisementrequest.UserID = para.UserID;
                    var info = adv.GetAdvertisement(advertisementrequest);

                   
                    return Json(info);
                }
            }
            return Json(wtw);




            return null;
        }


        [HttpPost]//9
        public async Task<IActionResult> GetAdvertisementListFooter()
        {
            var operatorResponse = new OperatorResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvaildSession
            };
            var readHeader = ReadHeader();
            var appRequest = new AppRequest
            {
                APPID = readHeader.AppID,
                Version = readHeader.Version
            };

            var appResp = appML.CheckWebApp(appRequest);
            await Task.Delay(0);
            IAdvertisementML adv = new AdvertisementML(_accessor, _env, false);
           
            var info = adv._GetAdvertisement();


            return Json(info);
        }

         [HttpPost]
        public async Task<IActionResult> PayWithQR()
        {
            var paymentmoderesponse = new OperatorResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvaildSession,

            };
            var readHeader = ReadHeader();
            var paymentmode = new AppSessionReq
            {
                APPID = readHeader.AppID,
                Version = readHeader.Version,
                Session = readHeader.Session,
                SessionID = readHeader.SessionID,
                LoginTypeID = LoginType.ApplicationUser,
                UserID = readHeader.UserID
            };

            var appResp = appML.CheckWebAppSession(paymentmode);
            paymentmoderesponse.IsAppValid = appResp.IsAppValid;
            paymentmoderesponse.IsVersionValid = appResp.IsVersionValid;
            paymentmoderesponse.IsPasswordExpired = appResp.IsPasswordExpired;
            paymentmoderesponse.Statuscode = appResp.Statuscode;
            paymentmoderesponse.IsLookUpFromAPI = appResp.IsLookUpFromAPI;
            paymentmoderesponse.IsDTHInfoCall = ApplicationSetting.IsDTHInfoCall;
            paymentmoderesponse.IsShowPDFPlan = ApplicationSetting.IsShowPDFPlan;
            if (appResp.Statuscode == ErrorCodes.One)
            {
                IUPIPaymentML paymentML = new PaymentGatewayML(_accessor, _env);
                var res = paymentML.GetUPIQR(paymentmode.LoginTypeID, paymentmode.UserID,0);
                if (res.Statuscode == ErrorCodes.One && !string.IsNullOrEmpty(res.CommonStr4))
                {

                    QRCodeGenerator qRCodeGenerator = new QRCodeGenerator();
                    QRCodeData QCD = qRCodeGenerator.CreateQrCode(res.CommonStr4, QRCodeGenerator.ECCLevel.Q);
                    QRCode qRCode = new QRCode(QCD);
                    return File(ConverterHelper.BitmapToBytesCode(qRCode.GetGraphic(20)), "image/png");
                }
                else
                {
                    string msg = res.Statuscode == ErrorCodes.One ? "QR Data Not Found" : res.Msg;
                    Bitmap b = new Bitmap(500, 500);
                    Graphics g = Graphics.FromImage(b);
                    g.DrawString(msg, new Font("Arial", 36), Brushes.Red, new Point(10, 10));
                    return File(ConverterHelper.BitmapToBytesCode(b), "image/png");
                }



            }
            else
            {
                Bitmap b = new Bitmap(500, 500);
                Graphics g = Graphics.FromImage(b);
                g.DrawString(ErrorCodes.InvaildSession, new Font("Arial", 36), Brushes.Red, new Point(10, 10));
                return File(ConverterHelper.BitmapToBytesCode(b), "image/png");
            }
            
        }



        [HttpPost]
        public async Task<IActionResult> UserVADetail([FromBody] CallMeReqApp callmereqapp)//for wallet
        {
            var wtw = new WebWTWUserInfo
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvaildSession,

            };
            var readHeader = ReadHeader();
            var paymentmode = new AppSessionReq
            {
                APPID = readHeader.AppID,
                Version = readHeader.Version,
                Session = readHeader.Session,
                SessionID = readHeader.SessionID,
                LoginTypeID = LoginType.ApplicationUser,
                UserID = readHeader.UserID
            };

            var appResp = appML.CheckWebAppSession(paymentmode);
            wtw.IsAppValid = appResp.IsAppValid;
            wtw.IsVersionValid = appResp.IsVersionValid;
            wtw.IsPasswordExpired = appResp.IsPasswordExpired;
            wtw.Statuscode = appResp.Statuscode;
            wtw.IsLookUpFromAPI = appResp.IsLookUpFromAPI;
            //wtw.IsDTHInfoCall = ApplicationSetting.IsDTHInfoCall;
            wtw.IsShowPDFPlan = ApplicationSetting.IsShowPDFPlan;
            if (appResp.Statuscode == ErrorCodes.One)
            {
                wtw.Statuscode = appResp.Statuscode;

                if (!appResp.IsPasswordExpired)
                {

                    IUPIPaymentML paymentML = new PaymentGatewayML(_accessor, _env);
                    var res = paymentML.GetUPIQRBankDetail(paymentmode.LoginTypeID, paymentmode.UserID);
                    var res1 = new
                    {
                        Statuscode = ErrorCodes.One,
                        Msg = ErrorCodes.SUCCESS,
                        res = res
                    };
                    return Json(res1);
                }
                
            }
            return Json(wtw);
        }

        [HttpPost]
        [DisableCors]
        public IActionResult IntiateUPI([FromBody] UPIInitiateRequest uPIInitiateRequest)
        {
            var appResp = new UPIIntiateResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            var readHeader = ReadHeader();
            var paymentmode = new AppSessionReq
            {
                APPID = readHeader.AppID,
                Version = readHeader.Version,
                Session = readHeader.Session,
                SessionID = readHeader.SessionID,
                LoginTypeID = LoginType.ApplicationUser,
                UserID = readHeader.UserID
            };

            var appResponse = appML.CheckWebAppSession(paymentmode);
            appResp.Statuscode = appResponse.Statuscode;
            appResp.Msg = appResponse.Msg;
            appResp.IsAppValid = appResponse.IsAppValid;
            appResp.IsVersionValid = appResponse.IsVersionValid;
            appResp.IsPasswordExpired = appResponse.IsPasswordExpired;
            appResp.Statuscode = appResponse.Statuscode;

            if (appResp.Statuscode == ErrorCodes.One)
            {
                if (!appResp.IsPasswordExpired)
                {
                    var gatewayML = new PaymentGatewayML(_accessor, _env);
                    var gtres = gatewayML.GetPGDetailsUser(appResponse.CheckID, true);
                    var UPGID = 0;
                    if (gtres.Any())
                    {
                        UPGID = gtres.Select(x => x.ID).FirstOrDefault();
                    }
                    var iniResp = gatewayML.InitiateUPIPaymentForApp(paymentmode.UserID, uPIInitiateRequest.Amount, UPGID, uPIInitiateRequest.OID, uPIInitiateRequest.WalletID, uPIInitiateRequest.IMEI);

                    appResp.Statuscode = iniResp.Statuscode;
                    appResp.Msg = iniResp.Msg;
                    appResp.TID = "TID" + iniResp.CommonInt;
                    appResp.BankOrderID = iniResp.VendorID;
                    appResp.MVPA = iniResp.MerchantVPA;
                    appResp.TerminalID = iniResp.CommonStr2;
                }
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "IntiateUPI",
                CommonStr2 = JsonConvert.SerializeObject(uPIInitiateRequest),
                CommonStr3 = JsonConvert.SerializeObject(appResp)
            });
            return Json(appResp);
        }

        [HttpPost]//8
        public async Task<IActionResult> B2COpDetail([FromBody] AppSessionReq data)
        {
            var servicesAssigned = new ServicesAssigned
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.NODATA
            };
            var readHeader = ReadHeader();
            var appRequest = new AppRequest
            {
                APPID = readHeader.AppID,
                Version = readHeader.Version
            };
            var appResp = appML.CheckWebApp(appRequest);
            servicesAssigned.IsAppValid = appResp.IsAppValid;
            servicesAssigned.IsVersionValid = appResp.IsVersionValid;
            servicesAssigned.Statuscode = appResp.Statuscode;
            if (appResp.Statuscode == ErrorCodes.One)
            {
                await Task.Delay(0);
                ISellerML sellerML = new SellerML(_accessor, _env, false);
                IOperatorML operatorML = new OperatorML(_accessor, _env,false);
                IOperatorAppML operatorAppML = new OperatorML(_accessor, _env, false);
                var a=operatorML.GetOperators().Where(x=>x.OID == data.OID).FirstOrDefault();
                var commonReq = new CommonReq
                {
                    LoginID = 1,
                    CommonInt = data.OID,
                    LoginTypeID = 1
                };

                var b = operatorAppML.OperatorOptionalApp(commonReq);

              
                return Json(new
                {
                    Statuscode = ErrorCodes.One,
                    Msg = ErrorCodes.SUCCESS,
                    OperatorOptional = b,
                    OperatorDetail = a
                });
            }
            else
            {
                servicesAssigned.Msg = appResp.Msg;
            }
            
            return Json(servicesAssigned);
        }



    }
}

