using Fintech.AppCode;
using Fintech.AppCode.Configuration;
using Fintech.AppCode.HelperClass;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using Fintech.AppCode.WebRequest;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using QRCoder;
using RoundpayFinTech.AppCode;
using RoundpayFinTech.AppCode.Configuration;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.MiddleLayer;
using RoundpayFinTech.AppCode.MiddleLayer.Dmt_Api;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.App;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.Model.Reports.Filter;
using RoundpayFinTech.AppCode.Model.ROffer;
using RoundpayFinTech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.StaticModel.MoneyTransfer;
using RoundpayFinTech.AppCode.ThirdParty.CyberPlate;
using RoundpayFinTech.AppCode.ThirdParty.Paytm;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Validators;

namespace RoundpayFinTech.Controllers
{
    [DisableCors]
    [ApiExplorerSettings(IgnoreApi = true)]
    public partial class AppController : Controller
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        private readonly AppML appML;
        public AppController(IHttpContextAccessor accessor, IHostingEnvironment env)
        {
            _accessor = accessor;
            _env = env;
            appML = new AppML(_accessor, _env);
        }
        #region RequiredSection
        [HttpPost,Route("/App/ForgetPassword")]
        public async Task<IActionResult> ForgetPassword([FromBody] LoginRequest loginRequest)
        {
            var appResponse = new AppResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            var appRequest = new AppRequest
            {
                APPID = loginRequest.APPID,
                IMEI = loginRequest.IMEI,
                RegKey = loginRequest.RegKey,
                SerialNo = loginRequest.SerialNo,
                Version = loginRequest.Version
            };
            appResponse = appML.CheckApp(appRequest);
            if (appResponse.Statuscode == ErrorCodes.One)
            {
                var loginDetail = new LoginDetail
                {
                    LoginTypeID = loginRequest.LoginTypeID,
                    RequestIP = loginRequest.IMEI,
                    Browser = loginRequest.SerialNo + "_" + loginRequest.Version,
                    LoginMobile = loginRequest.UserID,
                    Password = loginRequest.Password
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
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "ForgetPassword",
                CommonStr2 = JsonConvert.SerializeObject(loginRequest),
                CommonStr3 = JsonConvert.SerializeObject(appResponse)
            });
            return Json(appResponse);
        }
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            var loginResponse = new AppLoginResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvalidLogin
            };
            var appRequest = new AppRequest
            {
                APPID = loginRequest.APPID,
                IMEI = loginRequest.IMEI,
                RegKey = loginRequest.RegKey,
                SerialNo = loginRequest.SerialNo,
                Version = loginRequest.Version
            };
            var appResp = appML.CheckApp(appRequest);

            if (appResp.Statuscode == ErrorCodes.One)
            {
                await Task.Delay(0).ConfigureAwait(false);
                if (loginRequest == null)
                {
                    return Json(loginResponse);
                }

                var loginDetail = new LoginDetail
                {
                    LoginTypeID = loginRequest.LoginTypeID,
                    Browser = loginRequest.SerialNo + "_" + loginRequest.Version,
                    LoginMobile = loginRequest.UserID,
                    Password = loginRequest.Password,
                    CommonStr = loginRequest.IMEI ?? ""
                };
                if (!loginDetail.LoginTypeID.In(LoginType.ApplicationUser, LoginType.CustomerCare, LoginType.Employee))
                {
                    loginResponse.Msg = "Choose user login type!";
                    return Json(loginResponse);
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
                        return Json(loginResponse);
                    string loginID = Validate.O.LoginID(loginDetail.LoginMobile);
                    if (!Validate.O.IsNumeric(loginID))
                    {
                        return Json(loginResponse);
                    }
                    loginDetail.LoginID = Convert.ToInt32(loginID);
                    loginDetail.LoginMobile = "";
                }
                loginDetail.RequestMode = RequestMode.APPS;
                loginDetail.Password = HashEncryption.O.Encrypt(loginDetail.Password);
                ILoginML _loginML = new LoginML(_accessor, _env, loginRequest.Domain);
                loginResponse = _loginML.DoAppLogin(loginDetail);
            }
            else
            {
                loginResponse.Msg = appResp.Msg;
            }
            loginResponse.IsAppValid = appResp.IsAppValid;
            loginResponse.IsVersionValid = appResp.IsVersionValid;
            loginResponse.IsDTHInfo = ApplicationSetting.IsDTHInfo;
            loginResponse.IsRoffer = ApplicationSetting.IsRoffer;
            loginResponse.IsHeavyRefresh = ApplicationSetting.IsHeavyRefresh;
            loginResponse.IsTargetShow = ApplicationSetting.IsTarget;
            loginResponse.IsRealAPIPerTransaction = ApplicationSetting.IsRealAPIPerTransaction;
            loginResponse.IsAdminFlatComm = false;
            loginResponse.ActiveFlatType = ApplicationSetting.ActiveFlatType;
            loginResponse.IsDenominationIncentive = ApplicationSetting.IsDenominationIncentive;
            loginResponse.IsAutoBilling = ApplicationSetting.IsAutoBilling;
            loginResponse.WithCustomLoginID = ApplicationSetting.WithCustomLoginID;
            loginResponse.IsVirtualAccountInternal = ApplicationSetting.IsVirtualAccountInternal;
            loginResponse.IsReferral = ApplicationSetting.IsReferral;
            loginResponse.IsAccountStatement = ApplicationSetting.IsAccountStatement;
            loginResponse.IsAreaMaster = ApplicationSetting.IsAreaMaster;
            loginResponse.IsPlanServiceUpdated = ApplicationSetting.IsPlanServiceUpdated;
            loginResponse.IsMultiLevel = ApplicationSetting.IsMultiLevel && loginResponse.IsMultiLevel;

            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "Login",
                CommonStr2 = JsonConvert.SerializeObject(loginRequest),
                CommonStr3 = JsonConvert.SerializeObject(loginResponse)
            });
            return Json(loginResponse);
        }
        [HttpPost]
        public async Task<IActionResult> ValidateOTP([FromBody] OTPRequest oTPRequest)
        {
            var loginResponse = new AppLoginResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = nameof(ErrorCodes.Invalid_OTP).Replace("_", " ")
            };
            var appRequest = new AppRequest
            {
                APPID = oTPRequest.APPID,
                IMEI = oTPRequest.IMEI,
                RegKey = oTPRequest.RegKey,
                SerialNo = oTPRequest.SerialNo,
                Version = oTPRequest.Version
            };
            var appResp = appML.CheckApp(appRequest);
            try
            {
                var OTPSession = HashEncryption.O.Decrypt(oTPRequest.OTPSession);
                var _lr = JsonConvert.DeserializeObject<LoginResponse>(OTPSession);
                ILoginML loginML = new LoginML(_accessor, _env, oTPRequest.Domain);
                await Task.Delay(0).ConfigureAwait(false);
                loginResponse = loginML.ValidateOTPForApp(oTPRequest.OTP, _lr, oTPRequest.SerialNo + "_" + oTPRequest.Version, oTPRequest.IMEI ?? "");
            }
            catch (Exception ex)
            {

            }
            loginResponse.IsAppValid = appResp.IsAppValid;
            loginResponse.IsVersionValid = appResp.IsVersionValid;
            loginResponse.IsDTHInfo = ApplicationSetting.IsDTHInfo;
            loginResponse.IsRoffer = ApplicationSetting.IsRoffer;
            loginResponse.IsHeavyRefresh = ApplicationSetting.IsHeavyRefresh;
            loginResponse.IsTargetShow = ApplicationSetting.IsTarget;
            loginResponse.IsRealAPIPerTransaction = ApplicationSetting.IsRealAPIPerTransaction;
            loginResponse.IsAdminFlatComm = false;
            loginResponse.ActiveFlatType = ApplicationSetting.ActiveFlatType;
            loginResponse.IsDenominationIncentive = ApplicationSetting.IsDenominationIncentive;
            loginResponse.IsAutoBilling = ApplicationSetting.IsAutoBilling;
            loginResponse.WithCustomLoginID = ApplicationSetting.WithCustomLoginID;
            loginResponse.IsVirtualAccountInternal = ApplicationSetting.IsVirtualAccountInternal;
            loginResponse.IsMultiLevel = ApplicationSetting.IsMultiLevel && loginResponse.IsMultiLevel;

            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "ValidateOTP",
                CommonStr2 = JsonConvert.SerializeObject(oTPRequest),
                CommonStr3 = JsonConvert.SerializeObject(loginResponse)
            });
            return Json(loginResponse);
        }
        /*========== Validate Google Auth PIN =======================*/
        [HttpPost]
        public async Task<IActionResult> ValidateGAuthPIN([FromBody] OTPRequest oTPRequest)
        {
            var loginResponse = new AppLoginResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = "Invalid Google PIN"
            };
            var appRequest = new AppRequest
            {
                APPID = oTPRequest.APPID,
                IMEI = oTPRequest.IMEI,
                RegKey = oTPRequest.RegKey,
                SerialNo = oTPRequest.SerialNo,
                Version = oTPRequest.Version
            };
            var appResp = appML.CheckApp(appRequest);
            try
            {
                var OTPSession = HashEncryption.O.Decrypt(oTPRequest.OTPSession);
                var _lr = JsonConvert.DeserializeObject<LoginResponse>(OTPSession);
                ILoginML loginML = new LoginML(_accessor, _env, oTPRequest.Domain);
                await Task.Delay(0).ConfigureAwait(false);
                loginResponse = loginML.ValidateGAuthPINForApp(oTPRequest.OTP, _lr, oTPRequest.SerialNo + "_" + oTPRequest.Version, oTPRequest.IMEI ?? "");
            }
            catch (Exception ex)
            {

            }
            loginResponse.IsAppValid = appResp.IsAppValid;
            loginResponse.IsVersionValid = appResp.IsVersionValid;
            loginResponse.IsDTHInfo = ApplicationSetting.IsDTHInfo;
            loginResponse.IsRoffer = ApplicationSetting.IsRoffer;
            loginResponse.IsHeavyRefresh = ApplicationSetting.IsHeavyRefresh;
            loginResponse.IsTargetShow = ApplicationSetting.IsTarget;
            loginResponse.IsRealAPIPerTransaction = ApplicationSetting.IsRealAPIPerTransaction;
            loginResponse.IsAdminFlatComm = false;
            loginResponse.ActiveFlatType = ApplicationSetting.ActiveFlatType;
            loginResponse.IsDenominationIncentive = ApplicationSetting.IsDenominationIncentive;
            loginResponse.IsAutoBilling = ApplicationSetting.IsAutoBilling;
            loginResponse.WithCustomLoginID = ApplicationSetting.WithCustomLoginID;
            loginResponse.IsVirtualAccountInternal = ApplicationSetting.IsVirtualAccountInternal;
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "ValidateOTP",
                CommonStr2 = JsonConvert.SerializeObject(oTPRequest),
                CommonStr3 = JsonConvert.SerializeObject(loginResponse)
            });
            return Json(loginResponse);
        }
        /*===========================================================*/
        [HttpPost]
        public async Task<IActionResult> ResendOTP([FromBody] OTPRequest oTPRequest)
        {
            var response = new AppResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = nameof(ErrorCodes.Invalid_OTP).Replace("_", " ")
            };
            response = appML.CheckApp(oTPRequest);
            try
            {
                var OTPSession = HashEncryption.O.Decrypt(oTPRequest.OTPSession);
                var _lr = JsonConvert.DeserializeObject<LoginResponse>(OTPSession);
                ILoginML loginML = new LoginML(_accessor, _env, oTPRequest.Domain);
                await Task.Delay(0).ConfigureAwait(false);
                var res = loginML.ResendLoginOTPApp(_lr, oTPRequest.SerialNo + "_" + oTPRequest.Version, oTPRequest.IMEI ?? string.Empty);
                response.Statuscode = res.Statuscode;
                response.Msg = res.Msg;
            }
            catch (Exception ex)
            {

            }

            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "ResendOTP",
                CommonStr2 = JsonConvert.SerializeObject(oTPRequest),
                CommonStr3 = JsonConvert.SerializeObject(response)
            });
            return Json(response);
        }
        [HttpPost]
        public async Task<IActionResult> GetBalance([FromBody] AppSessionReq balanceReq)
        {
            var balanceResponse = new BalanceResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvaildSession
            };

            var appResp = appML.CheckAppSession(balanceReq);
            balanceResponse.IsAppValid = appResp.IsAppValid;
            balanceResponse.IsVersionValid = appResp.IsVersionValid;
            balanceResponse.IsPasswordExpired = appResp.IsPasswordExpired;
            balanceResponse.Statuscode = appResp.Statuscode;
            balanceResponse.IsLookUpFromAPI = appResp.IsLookUpFromAPI;
            balanceResponse.IsRoffer = ApplicationSetting.IsRoffer;
            balanceResponse.IsDTHInfo = ApplicationSetting.IsDTHInfo;
            balanceResponse.IsMoveToPrepaid = ApplicationSetting.IsMoveToPrepaid;
            balanceResponse.IsMoveToUtility = ApplicationSetting.IsMoveToUtility;
            balanceResponse.IsFlatCommission = ApplicationSetting.ActiveFlatType == FlatTypeMaster.ByAdminOnly;
            balanceResponse.ActiveFlatType = ApplicationSetting.ActiveFlatType;
            balanceResponse.IsReferral = ApplicationSetting.IsReferral;
            balanceResponse.IsBulkQRGeneration = ApplicationSetting.IsBulkQRGeneration;
            balanceResponse.IsSattlemntAccountVerify = ApplicationSetting.IsSattlemntAccountVerify;
            balanceResponse.IsEKYCForced = ApplicationSetting.IsEKYCForced;
            if (ApplicationSetting.IsDTHInfoCall && appResp.IsDTHInfo)
                balanceResponse.IsDTHInfoCall = true;
            else
                balanceResponse.IsDTHInfoCall = false;

            balanceResponse.IsShowPDFPlan = ApplicationSetting.IsShowPDFPlan;
            if (appResp.Statuscode == ErrorCodes.One)
            {
                balanceResponse.Statuscode = appResp.Statuscode;
                if (!appResp.IsPasswordExpired)
                {
                    await Task.Delay(0).ConfigureAwait(false);
                    IUserML userML = new UserML(_accessor, _env, false);
                    balanceResponse.Statuscode = ErrorCodes.One;
                    balanceResponse.Msg = RechargeRespType._SUCCESS;
                    balanceResponse.Data = userML.GetUserBalnace(balanceReq.UserID, balanceReq.LoginTypeID);
                    if (ApplicationSetting.IsAutoBilling)
                    {
                        userML.GetAutoBillingProcess(balanceReq.UserID);
                    }
                }
            }
            else
            {
                balanceResponse.Msg = appResp.Msg;
            }

            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "GetBalance",
                CommonStr2 = JsonConvert.SerializeObject(balanceReq),
                CommonStr3 = JsonConvert.SerializeObject(balanceResponse)
            });
            return Json(balanceResponse);
        }
        [HttpPost]
        public async Task<IActionResult> GeUserCommissionRate([FromBody] AppUserRequest balanceReq)
        {
            var balanceResponse = new CommRateResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvaildSession
            };

            var appResp = appML.CheckAppSession(balanceReq);
            balanceResponse.IsAppValid = appResp.IsAppValid;
            balanceResponse.IsVersionValid = appResp.IsVersionValid;
            balanceResponse.IsPasswordExpired = appResp.IsPasswordExpired;
            balanceResponse.Statuscode = appResp.Statuscode;
            balanceResponse.IsLookUpFromAPI = appResp.IsLookUpFromAPI;
            balanceResponse.IsRoffer = ApplicationSetting.IsRoffer;
            balanceResponse.IsDTHInfo = ApplicationSetting.IsDTHInfo;
            if (ApplicationSetting.IsDTHInfoCall && appResp.IsDTHInfo)
                balanceResponse.IsDTHInfoCall = true;
            else
                balanceResponse.IsDTHInfoCall = false;

            balanceResponse.IsShowPDFPlan = ApplicationSetting.IsShowPDFPlan;
            if (appResp.Statuscode == ErrorCodes.One)
            {
                balanceResponse.Statuscode = appResp.Statuscode;
                if (!appResp.IsPasswordExpired)
                {
                    await Task.Delay(0).ConfigureAwait(false);
                    IUserML userML = new UserML(_accessor, _env, false);
                    balanceResponse.Statuscode = ErrorCodes.One;
                    balanceResponse.Msg = RechargeRespType._SUCCESS;
                    var res = userML.GetUserBalnace(balanceReq.UID, balanceReq.LoginTypeID);
                    balanceResponse.CommRate = res.CommRate;
                }
            }
            else
            {
                balanceResponse.Msg = appResp.Msg;
            }

            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "GeUserCommissionRate",
                CommonStr2 = JsonConvert.SerializeObject(balanceReq),
                CommonStr3 = JsonConvert.SerializeObject(balanceResponse)
            });
            return Json(balanceResponse);
        }
        [HttpPost]
        public async Task<IActionResult> GetWalletType([FromBody] AppSessionReq appSessionReq)
        {
            var walletResponse = new WalletResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvaildSession
            };

            var appResp = appML.CheckAppSession(appSessionReq);
            walletResponse.IsAppValid = appResp.IsAppValid;
            walletResponse.IsVersionValid = appResp.IsVersionValid;
            walletResponse.IsPasswordExpired = appResp.IsPasswordExpired;
            walletResponse.Statuscode = appResp.Statuscode;
            if (appResp.Statuscode == ErrorCodes.One)
            {
                walletResponse.Statuscode = appResp.Statuscode;
                if (!appResp.IsPasswordExpired)
                {
                    await Task.Delay(0).ConfigureAwait(false);
                    IReportML reportML = new ReportML(_accessor, _env, false);
                    walletResponse.Statuscode = ErrorCodes.One;
                    walletResponse.Msg = RechargeRespType._SUCCESS;
                    walletResponse.WalletTypes = reportML.GetWalletTypes();
                    IUserML userML = new UserML(_accessor, _env, false);
                    walletResponse.moveToWalletMappings = userML.GetMoveToWalletsMap();
                }
            }
            else
            {
                walletResponse.Msg = appResp.Msg;
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "GetWalletType",
                CommonStr2 = JsonConvert.SerializeObject(appSessionReq),
                CommonStr3 = JsonConvert.SerializeObject(walletResponse)
            });
            return Json(walletResponse);
        }
        [HttpPost]
        public async Task<IActionResult> GetNumberList([FromBody] AppRequest appRequest)
        {
            var operatorResponse = new OperatorResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvaildSession
            };
            var appResp = appML.CheckApp(appRequest);
            operatorResponse.IsAppValid = appResp.IsAppValid;
            operatorResponse.IsVersionValid = appResp.IsVersionValid;
            IOperatorAppML opml = new OperatorML(_accessor, _env, false);
            operatorResponse.Statuscode = ErrorCodes.One;
            operatorResponse.Msg = ErrorCodes.SUCCESS;
            var opNumberData = new OpNumberData
            {
                NumSeries = await opml.NumberList().ConfigureAwait(false),
                Operators = opml.GetOperatorsApp(Role.Retailor_Seller),
                Cirlces = await opml.CircleList().ConfigureAwait(false)
            };
            operatorResponse.Data = opNumberData;
            operatorResponse.IsLookUpFromAPI = appResp.IsLookUpFromAPI;
            operatorResponse.IsTakeCustomerNo = ApplicationSetting.IsTakeCustomerNo;
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "GetNumberList",
                CommonStr2 = JsonConvert.SerializeObject(appRequest),
                CommonStr3 = JsonConvert.SerializeObject(operatorResponse)
            });
            return Json(operatorResponse);
        }
        [HttpPost]
        public IActionResult GetOperators([FromBody] AppRequest appRequest)
        {
            var operatorResponse = new AppServiceProviders
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvaildSession
            };
            var appResp = appML.CheckApp(appRequest);
            operatorResponse.IsAppValid = appResp.IsAppValid;
            operatorResponse.IsVersionValid = appResp.IsVersionValid;
            IOperatorAppML opml = new OperatorML(_accessor, _env, false);
            operatorResponse.Statuscode = ErrorCodes.One;
            operatorResponse.Msg = ErrorCodes.SUCCESS;
            operatorResponse.Operators = opml.GetOperatorsApp(Role.Retailor_Seller);
            operatorResponse.IsLookUpFromAPI = appResp.IsLookUpFromAPI;
            operatorResponse.IsTakeCustomerNo = ApplicationSetting.IsTakeCustomerNo;
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "GetOperators",
                CommonStr2 = JsonConvert.SerializeObject(appRequest),
                CommonStr3 = JsonConvert.SerializeObject(operatorResponse)
            });
            return Json(operatorResponse);
        }
        [HttpPost]
        public IActionResult GetOperatorSession([FromBody] AppSessionReq appRequest)
        {
            var operatorResponse = new AppServiceProviders
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvaildSession
            };
            var appResp = appML.CheckAppSession(appRequest);
            operatorResponse.IsAppValid = appResp.IsAppValid;
            operatorResponse.IsVersionValid = appResp.IsVersionValid;
            IOperatorAppML opml = new OperatorML(_accessor, _env, false);
            operatorResponse.Statuscode = ErrorCodes.One;
            operatorResponse.Msg = ErrorCodes.SUCCESS;
            operatorResponse.Operators = opml.GetOperatorsSession(appRequest.UserID, appResp.RID);
            operatorResponse.IsLookUpFromAPI = appResp.IsLookUpFromAPI;
            operatorResponse.IsTakeCustomerNo = ApplicationSetting.IsTakeCustomerNo;
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "GetOperatorSession",
                CommonStr2 = JsonConvert.SerializeObject(appRequest),
                CommonStr3 = JsonConvert.SerializeObject(operatorResponse)
            });
            return Json(operatorResponse);
        }
        [HttpPost]
        public IActionResult GetOperatorsActive([FromBody] AppCommonRequest appRequest)
        {
            var operatorResponse = new AppServiceProviders
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvaildSession
            };
            var appResp = appML.CheckAppSession(appRequest);
            operatorResponse.IsAppValid = appResp.IsAppValid;
            operatorResponse.IsVersionValid = appResp.IsVersionValid;
            operatorResponse.Statuscode = appResp.Statuscode;
            operatorResponse.Msg = appResp.Msg;
            if (appResp.Statuscode == ErrorCodes.One)
            {
                IOperatorML opml = new OperatorML(_accessor, _env, false);
                operatorResponse.Operators = opml.GetActiveOperators(appRequest.UserID, appRequest.OType);
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "GetOperatorsActive",
                CommonStr2 = JsonConvert.SerializeObject(appRequest),
                CommonStr3 = JsonConvert.SerializeObject(operatorResponse)
            });
            return Json(operatorResponse);
        }
        [HttpPost]
        public async Task<IActionResult> GetCircles([FromBody] AppRequest appRequest)
        {
            var circleResponse = new AppCircles
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvaildSession
            };
            var appResp = appML.CheckApp(appRequest);
            circleResponse.IsAppValid = appResp.IsAppValid;
            circleResponse.IsVersionValid = appResp.IsVersionValid;
            IOperatorAppML opml = new OperatorML(_accessor, _env, false);
            circleResponse.Statuscode = ErrorCodes.One;
            circleResponse.Msg = ErrorCodes.SUCCESS;
            circleResponse.Cirlces = await opml.CircleList().ConfigureAwait(false);
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "GetCircles",
                CommonStr2 = JsonConvert.SerializeObject(appRequest),
                CommonStr3 = JsonConvert.SerializeObject(circleResponse)
            });
            return Json(circleResponse);
        }
        public async Task<IActionResult> GetNumberSeries([FromBody] AppRequest appRequest)
        {
            var numberSeries = new AppNumberSeries
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvaildSession
            };
            var appResp = appML.CheckApp(appRequest);
            numberSeries.IsAppValid = appResp.IsAppValid;
            numberSeries.IsVersionValid = appResp.IsVersionValid;
            IOperatorAppML opml = new OperatorML(_accessor, _env, false);
            numberSeries.Statuscode = ErrorCodes.One;
            numberSeries.Msg = ErrorCodes.SUCCESS;
            numberSeries.NumSeries = await opml.NumberList().ConfigureAwait(false);
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "GetNumberSeries",
                CommonStr2 = JsonConvert.SerializeObject(appRequest),
                CommonStr3 = JsonConvert.SerializeObject(numberSeries)
            });
            return Json(numberSeries);
        }
        [HttpPost]
        public async Task<IActionResult> GetHLRLookUp([FromBody] AppHLRLookupReq hlrLookupReq)
        {
            var hlrLookupResponse = new AppHLRLookupResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvaildSession
            };
            AppResponse appResp = appML.CheckAppSession(hlrLookupReq);
            hlrLookupResponse.IsAppValid = appResp.IsAppValid;
            hlrLookupResponse.IsVersionValid = appResp.IsVersionValid;
            hlrLookupResponse.IsPasswordExpired = appResp.IsPasswordExpired;
            hlrLookupResponse.Statuscode = appResp.Statuscode;
            if (appResp.Statuscode == ErrorCodes.One)
            {
                if (!appResp.IsPasswordExpired)
                {
                    var req = new CommonReq
                    {
                        LoginTypeID = hlrLookupReq.LoginTypeID,
                        LoginID = hlrLookupReq.UserID,
                        CommonStr = hlrLookupReq.Mobile
                    };
                    await Task.Delay(0);
                    IAppReportML reportML = new ReportML(_accessor, _env);
                    var respStatus = (HLRResponseStatus)reportML.CheckNumberSeriesExist(req);
                    hlrLookupResponse.Statuscode = respStatus.Statuscode;
                    hlrLookupResponse.Msg = respStatus.Msg;
                    hlrLookupResponse.OID = respStatus.CommonInt;
                    hlrLookupResponse.CircleID = respStatus.CommonInt2;
                }
            }
            else
            {
                hlrLookupResponse.Msg = appResp.Msg;
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "GetHLRLookUp",
                CommonStr2 = JsonConvert.SerializeObject(hlrLookupReq),
                CommonStr3 = JsonConvert.SerializeObject(hlrLookupResponse)
            });
            return Json(hlrLookupResponse);
        }
        [HttpPost]
        public async Task<IActionResult> GetOpTypes([FromBody] AppSessionReq balanceReq)
        {
            var servicesAssigned = new ServicesAssigned
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvaildSession
            };
            AppResponse appResp = appML.CheckAppSession(balanceReq);
            servicesAssigned.IsAppValid = appResp.IsAppValid;
            servicesAssigned.IsVersionValid = appResp.IsVersionValid;
            servicesAssigned.IsPasswordExpired = appResp.IsPasswordExpired;
            servicesAssigned.Statuscode = appResp.Statuscode;
            if (appResp.Statuscode == ErrorCodes.One)
            {
                if (!appResp.IsPasswordExpired)
                {
                    await Task.Delay(0).ConfigureAwait(false);
                    ISellerML sellerML = new SellerML(_accessor, _env, false);
                    servicesAssigned.Statuscode = ErrorCodes.One;
                    servicesAssigned.Msg = RechargeRespType._SUCCESS;
                    var package_ClData = new Package_ClData
                    {
                        AssignedOpTypes = sellerML.GetPackage(balanceReq.UserID)
                    };
                    servicesAssigned.Data = package_ClData;
                }
            }
            else
            {
                servicesAssigned.Msg = appResp.Msg;
            }
            servicesAssigned.IsAddMoneyEnable = ApplicationSetting.IsAddMoneyEnable;// && appResp.CheckID == ErrorCodes.One;
            servicesAssigned.IsPaymentGatway = ApplicationSetting.IsPaymentGatway;
            servicesAssigned.IsUPI = ApplicationSetting.IsUPI;
            servicesAssigned.IsUPIQR = ApplicationSetting.IsUPIQR && (ApplicationSetting.IsFraudPrevention && appResp.IsGreen || !ApplicationSetting.IsFraudPrevention) && (appResp.IsPaymentGateway);
            servicesAssigned.IsECollectEnable = ApplicationSetting.IsECollectEnable;
            servicesAssigned.IsDMTWithPipe = ApplicationSetting.IsDMTWithPIPE;
            servicesAssigned.IsBulkQRGeneration = ApplicationSetting.IsBulkQRGeneration;
            servicesAssigned.IsCoin = ApplicationSetting.IsCOIN;
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "GetOpTypes",
                CommonStr2 = JsonConvert.SerializeObject(balanceReq),
                CommonStr3 = JsonConvert.SerializeObject(servicesAssigned)
            });
            return Json(servicesAssigned);
        }
        [HttpPost]
        public IActionResult GetOpTypesIndustryWise([FromBody] AppSessionReq balanceReq)
        {
            var resApp = new IndustryTypeAppModel
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvaildSession
            };
            AppResponse appResp = appML.CheckAppSession(balanceReq);
            resApp.IsAppValid = appResp.IsAppValid;
            resApp.IsVersionValid = appResp.IsVersionValid;
            resApp.IsPasswordExpired = appResp.IsPasswordExpired;
            resApp.Statuscode = appResp.Statuscode;
            if (appResp.Statuscode == ErrorCodes.One)
            {
                if (!appResp.IsPasswordExpired)
                {
                    IOperatorAppML opML = new OperatorML(_accessor, _env, false);
                    resApp.data = opML.GetIndustryWiseOpTypeList();
                }
            }
            else
            {
                resApp.Msg = appResp.Msg;
            }

            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "GetOpTypesIndustryWise",
                CommonStr2 = JsonConvert.SerializeObject(balanceReq),
                CommonStr3 = JsonConvert.SerializeObject(resApp)
            });
            return Json(resApp);
        }
        [HttpPost]
        public async Task<IActionResult> CallOnboarding([FromBody] AppOnboardCheckReq onboardCheckReq)
        {
            var onboardResponse = new AppOnboardResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvaildSession
            };
            var appRequest = new AppSessionReq
            {
                APPID = onboardCheckReq.APPID,
                IMEI = onboardCheckReq.IMEI,
                LoginTypeID = onboardCheckReq.LoginTypeID,
                UserID = onboardCheckReq.UserID,
                SessionID = onboardCheckReq.SessionID,
                RegKey = onboardCheckReq.RegKey,
                SerialNo = onboardCheckReq.SerialNo,
                Version = onboardCheckReq.Version,
                Session = onboardCheckReq.Session
            };
            var appResp = appML.CheckAppSession(appRequest);
            onboardResponse.IsAppValid = appResp.IsAppValid;
            onboardResponse.IsVersionValid = appResp.IsVersionValid;
            onboardResponse.IsPasswordExpired = appResp.IsPasswordExpired;
            onboardResponse.Statuscode = appResp.Statuscode;
            if (appResp.Statuscode == ErrorCodes.One)
            {
                if (!appResp.IsPasswordExpired)
                {
                    await Task.Delay(0).ConfigureAwait(false);
                    BBPSML bbpsML = new BBPSML(_accessor, _env, false);
                    var onboadresp = bbpsML.CheckOutletStatusForServices(new CheckOutletStatusReqModel
                    {
                        LoginTypeID = LoginType.ApplicationUser,
                        LoginID = onboardCheckReq.UserID,
                        OID = onboardCheckReq.OID,
                        OutletID = appResp.GetID,
                        RMode = RequestMode.APPS,
                        OTP= onboardCheckReq.OTP,
                        PidData = onboardCheckReq.PidData,
                        OTPRefID= onboardCheckReq.OTPRefID,
                        IsVerifyBiometric= onboardCheckReq.IsBio,
                        BioAuthType = onboardCheckReq.BioAuthType
                    });
                    onboardResponse.Statuscode = onboadresp.Statuscode;
                    onboardResponse.Msg = onboadresp.Msg;
                    onboardResponse.IsConfirmation = onboadresp.IsConfirmation;
                    onboardResponse.IsRedirection = onboadresp.IsRedirection;
                    onboardResponse.IsOTPRequired = onboadresp.IsOTPRequired;
                    onboardResponse.IsBioMetricRequired = onboadresp.IsBioMetricRequired;
                    onboardResponse.BioAuthType = onboadresp.BioAuthType;
                    onboardResponse.IsRedirectToExternal = onboadresp.IsRedirectToExternal;
                    onboardResponse.ExternalURL = onboadresp.IsRedirectToExternal ? onboadresp.CommonStr:string.Empty;
                    onboardResponse.OTPRefID = onboadresp.OTPRefID;
                    onboardResponse.IsDown = onboadresp.IsDown;
                    onboardResponse.IsWaiting = onboadresp.IsWaiting;
                    onboardResponse.IsRejected = onboadresp.IsRejected;
                    onboardResponse.IsIncomplete = onboadresp.IsIncomplete;
                    onboardResponse.IsUnathorized = onboadresp.IsUnathorized;
                    onboardResponse.BCResponse = onboadresp.BCResponse;
                    onboardResponse.PANID = onboadresp.CommonStr4;
                    onboardResponse.GIURL = onboadresp.CommonStr2;
                    onboardResponse.IsShowMsg = onboadresp.IsShowMsg;
                    onboardResponse.InInterface = onboadresp.InInterface;
                    onboardResponse.InterfaceType = onboadresp.InterfaceType;
                    onboardResponse.SDKType = onboadresp.SDKType;
                    onboardResponse.SDKDetail = onboadresp.SDKDetail;

                }
            }
            else
            {
                onboardResponse.Msg = appResp.Msg;
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "CallOnboarding",
                CommonStr2 = JsonConvert.SerializeObject(onboardCheckReq),
                CommonStr3 = JsonConvert.SerializeObject(onboardResponse)
            });
            return Json(onboardResponse);
        }
        [HttpPost]
        public async Task<IActionResult> GetOperatorOptionals([FromBody] OIDSessonRequest appSessionReq)
        {
            var operatoOptionalsResponse = new OperatoOptionalsResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvaildSession
            };
            var appResp = appML.CheckAppSession(appSessionReq);
            operatoOptionalsResponse.IsAppValid = appResp.IsAppValid;
            operatoOptionalsResponse.IsVersionValid = appResp.IsVersionValid;
            operatoOptionalsResponse.IsPasswordExpired = appResp.IsPasswordExpired;
            operatoOptionalsResponse.Statuscode = appResp.Statuscode;
            if (appResp.Statuscode == ErrorCodes.One)
            {
                if (!appResp.IsPasswordExpired)
                {
                    await Task.Delay(0).ConfigureAwait(false);
                    IOperatorAppML operatorAppML = new OperatorML(_accessor, _env, false);
                    operatoOptionalsResponse.Statuscode = ErrorCodes.One;
                    operatoOptionalsResponse.Msg = RechargeRespType._SUCCESS;
                    var commonReq = new CommonReq
                    {
                        LoginID = appSessionReq.UserID,
                        CommonInt = appSessionReq.OID,
                        LoginTypeID = appSessionReq.LoginTypeID
                    };

                    operatoOptionalsResponse.Data = operatorAppML.OperatorOptionalApp(commonReq);
                }
            }
            else
            {
                operatoOptionalsResponse.Msg = appResp.Msg;
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "GetOperatorOptionals",
                CommonStr2 = JsonConvert.SerializeObject(appSessionReq),
                CommonStr3 = JsonConvert.SerializeObject(operatoOptionalsResponse)
            });
            return Json(operatoOptionalsResponse);
        }
        [HttpPost]
        public async Task<IActionResult> Logout([FromBody] AppLogoutRequest logOutReq)
        {
            var appResponse = new AppResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvalidLogin
            };
            var appRequest = new AppSessionReq
            {
                APPID = logOutReq.APPID,
                IMEI = logOutReq.IMEI,
                LoginTypeID = logOutReq.LoginTypeID,
                UserID = logOutReq.UserID,
                SessionID = logOutReq.SessionID,
                RegKey = logOutReq.RegKey,
                SerialNo = logOutReq.SerialNo,
                Version = logOutReq.Version,
                Session = logOutReq.Session
            };
            appResponse = appML.CheckAppSession(logOutReq);
            if (appResponse.Statuscode == ErrorCodes.One)
            {
                var logoutReq = new LogoutReq
                {
                    LT = logOutReq.LoginTypeID,
                    LoginID = logOutReq.UserID,
                    SessID = logOutReq.SessionID,
                    ULT = logOutReq.LoginTypeID,
                    UserID = logOutReq.UserID,
                    RequestMode = RequestMode.APPS,
                    SessionType = logOutReq.SessType
                };
                ILoginML loginML = new LoginML(_accessor, _env, false);
                var resp = await loginML.DoLogout(logoutReq).ConfigureAwait(false);
                appResponse.Statuscode = resp.Statuscode;
                appResponse.Msg = resp.Msg;
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "Logout",
                CommonStr2 = JsonConvert.SerializeObject(logOutReq),
                CommonStr3 = JsonConvert.SerializeObject(appResponse)
            });
            return Json(appResponse);
        }
        [HttpPost]
        public async Task<IActionResult> UpdateFCMID([FromBody] AppFCMIDRequest appFCMIDRequest)
        {
            var appResponse = new AppResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvalidLogin
            };
            var appRequest = new AppSessionReq
            {
                APPID = appFCMIDRequest.APPID,
                IMEI = appFCMIDRequest.IMEI,
                LoginTypeID = appFCMIDRequest.LoginTypeID,
                UserID = appFCMIDRequest.UserID,
                SessionID = appFCMIDRequest.SessionID,
                RegKey = appFCMIDRequest.RegKey,
                SerialNo = appFCMIDRequest.SerialNo,
                Version = appFCMIDRequest.Version,
                Session = appFCMIDRequest.Session
            };
            appResponse = appML.CheckAppSession(appFCMIDRequest);
            if (appResponse.Statuscode == ErrorCodes.One)
            {
                var req = new CommonReq
                {
                    LoginTypeID = appFCMIDRequest.LoginTypeID,
                    LoginID = appFCMIDRequest.UserID,
                    CommonStr = appFCMIDRequest.FCMID ?? ""
                };
                await Task.Delay(0);
                ILoginML loginML = new LoginML(_accessor, _env, false);
                var resp = loginML.SaveFCMID(req);
                appResponse.Statuscode = resp.Statuscode;
                appResponse.Msg = resp.Msg;
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "UpdateFCMID",
                CommonStr2 = JsonConvert.SerializeObject(appFCMIDRequest),
                CommonStr3 = JsonConvert.SerializeObject(appResponse)
            });
            return Json(appResponse);
        }
        #region Company Profile
        [HttpPost]
        public IActionResult GetCompanyProfile([FromBody] AppSessionReq appSessionReq)
        {
            var appRechargeRespose = new AppReportResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            var CompanyProfile = new CompanyProfileDetail();
            var appResp = appML.CheckAppSession(appSessionReq);
            appRechargeRespose.IsAppValid = appResp.IsAppValid;
            appRechargeRespose.IsVersionValid = appResp.IsVersionValid;
            appRechargeRespose.CheckID = appResp.CheckID;
            if (appResp.Statuscode == ErrorCodes.One)
            {
                IUserAPPML ml = new UserML(_accessor, _env, false);
                CompanyProfile = ml.GetCompanyProfileApp(appRechargeRespose.CheckID);
                appRechargeRespose.Statuscode = ErrorCodes.One;
                appRechargeRespose.Msg = ErrorCodes.SUCCESS;
                appRechargeRespose.CompanyProfile = CompanyProfile;

            }
            else
                appRechargeRespose.Msg = appResp.Msg;
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "GetCompanyProfile",
                CommonStr2 = JsonConvert.SerializeObject(appSessionReq),
                CommonStr3 = JsonConvert.SerializeObject(appRechargeRespose)
            });
            return Json(appRechargeRespose);
        }
        #endregion
        #endregion
        #region TransactionSection
        [HttpPost]
        public IActionResult GetPincodeArea([FromBody] AppPincodeRequest transactionRequest)
        {
            var appResponse = new AppPincodeResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvaildSession
            };

            var appResp = appML.CheckAppSession(transactionRequest);
            appResponse.IsAppValid = appResp.IsAppValid;
            appResponse.IsVersionValid = appResp.IsVersionValid;
            appResponse.IsPasswordExpired = appResp.IsPasswordExpired;
            appResponse.Statuscode = appResp.Statuscode;
            if (appResp.Statuscode == ErrorCodes.One)
            {
                if (!appResp.IsPasswordExpired)
                {
                    var _req = new CommonReq
                    {
                        LoginID = transactionRequest.UserID,
                        LoginTypeID = LoginType.ApplicationUser,
                        CommonInt2 = transactionRequest.Pincode
                    };
                    IUserML UML = new UserML(_accessor, _env, false);
                    appResponse.Areas = UML.GetPincodeArea(_req);

                    appResponse.Statuscode = ErrorCodes.One;
                    appResponse.Msg = ErrorCodes.SUCCESS;
                    appResponse.IsAppValid = appResp.IsAppValid;
                    appResponse.IsVersionValid = appResp.IsVersionValid;
                }
            }
            else
            {
                appResponse.Msg = appResp.Msg;
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "GetPincodeArea",
                CommonStr2 = JsonConvert.SerializeObject(transactionRequest),
                CommonStr3 = JsonConvert.SerializeObject(appResponse)
            });
            return Json(appResponse);
        }
        [HttpPost]
        public async Task<IActionResult> DTHSubscription([FromBody] AppDTHSubscriptionReq transactionRequest)
        {
            var appTransactionRes = new AppTransactionRes
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvaildSession
            };

            var appResp = appML.CheckAppSession(transactionRequest);
            appTransactionRes.IsAppValid = appResp.IsAppValid;
            appTransactionRes.IsVersionValid = appResp.IsVersionValid;
            appTransactionRes.IsPasswordExpired = appResp.IsPasswordExpired;
            appTransactionRes.Statuscode = appResp.Statuscode;
            if (appResp.Statuscode == ErrorCodes.One)
            {
                if (!appResp.IsPasswordExpired)
                {
                    ISellerML sellerML = new SellerML(_accessor, _env, false);
                    var res = await sellerML.DoDTHSubscription(new DTHConnectionServiceRequest
                    {
                        PID = transactionRequest.PID,
                        Customer = transactionRequest.Customer + ' ' + transactionRequest.Surname,
                        CustomerNumber = transactionRequest.CustomerNumber,
                        Address = transactionRequest.Address,
                        Pincode = transactionRequest.Pincode,
                        UserID = transactionRequest.UserID,
                        IMEI = transactionRequest.IMEI,
                        SecurityKey = transactionRequest.SecurityKey,
                        Gender = transactionRequest.Gender,
                        AreaID = transactionRequest.AreaID
                    }).ConfigureAwait(false);

                    appTransactionRes.Statuscode = res.Statuscode;
                    appTransactionRes.Msg = res.Msg;
                    appTransactionRes.TransactionID = res.CommonStr;
                    appTransactionRes.IsAppValid = appResp.IsAppValid;
                    appTransactionRes.IsVersionValid = appResp.IsVersionValid;
                }
            }
            else
            {
                appTransactionRes.Msg = appResp.Msg;
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "Transaction",
                CommonStr2 = JsonConvert.SerializeObject(transactionRequest),
                CommonStr3 = JsonConvert.SerializeObject(appTransactionRes)
            });
            return Json(appTransactionRes);
        }
        [HttpPost]
        public async Task<IActionResult> Transaction([FromBody] AppTransactionReq transactionRequest)
        {
            var appTransactionRes = new AppTransactionRes
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvaildSession
            };
            var appRequest = new AppSessionReq
            {
                APPID = transactionRequest.APPID,
                IMEI = transactionRequest.IMEI,
                LoginTypeID = transactionRequest.LoginTypeID,
                UserID = transactionRequest.UserID,
                SessionID = transactionRequest.SessionID,
                RegKey = transactionRequest.RegKey,
                SerialNo = transactionRequest.SerialNo,
                Version = transactionRequest.Version,
                Session = transactionRequest.Session
            };
            var appResp = appML.CheckAppSession(appRequest);
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
                        OutletID = appResp.GetID,
                        GEOCode = string.IsNullOrEmpty(transactionRequest.GeoCode) ? "25.5601,74.3401" : transactionRequest.GeoCode,
                        Pincode = appResp.PCode,
                        SecKey = transactionRequest.SecurityKey ?? "",
                        RequestMode = RequestMode.APPS,
                        IsReal = transactionRequest.IsReal,
                        ReferenceID = transactionRequest.RefID,
                        FetchBillID = transactionRequest.FetchBillID

                    };
                    if (appRechargeRequest.GEOCode.Length > 15)
                    {
                        var a = appRechargeRequest.GEOCode.Split(',')[0];
                        var b = appRechargeRequest.GEOCode.Split(',')[1];

                        a = Validate.O.IsNumeric(a.Replace(".", "")) ? string.Format("{0:0.0000}", Convert.ToDecimal(a)) : "25.5601";
                        b = Validate.O.IsNumeric(b.Replace(".", "")) ? string.Format("{0:0.0000}", Convert.ToDecimal(b)) : "74.3401";

                        appRechargeRequest.GEOCode = a + "," + b;
                    }
                    ISellerML sellerML = new SellerML(_accessor, _env, false);
                    appTransactionRes = await sellerML.AppRecharge(appRechargeRequest).ConfigureAwait(false);
                    appTransactionRes.IsAppValid = appResp.IsAppValid;
                    appTransactionRes.IsVersionValid = appResp.IsVersionValid;
                }
            }
            else
            {
                appTransactionRes.Msg = appResp.Msg;
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "Transaction",
                CommonStr2 = JsonConvert.SerializeObject(transactionRequest),
                CommonStr3 = JsonConvert.SerializeObject(appTransactionRes)
            });
            return Json(appTransactionRes);
        }
        [HttpPost]
        public async Task<IActionResult> FetchBill([FromBody] AppTransactionReq transactionRequest)
        {
            var appBillfetchResponse = new AppBillfetchResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvaildSession
            };

            var appResp = appML.CheckAppSession(transactionRequest);
            appBillfetchResponse.IsAppValid = appResp.IsAppValid;
            appBillfetchResponse.IsVersionValid = appResp.IsVersionValid;
            appBillfetchResponse.IsPasswordExpired = appResp.IsPasswordExpired;
            appBillfetchResponse.Statuscode = appResp.Statuscode;
            appBillfetchResponse.Msg = appResp.Msg;
            if (appResp.Statuscode == ErrorCodes.One)
            {
                if (!appResp.IsPasswordExpired)
                {
                    var transactionServiceReq = new TransactionServiceReq
                    {
                        UserID = transactionRequest.UserID,
                        OID = transactionRequest.OID,
                        AccountNo = transactionRequest.AccountNo,
                        OutletID = appResp.GetID,
                        Optional1 = transactionRequest.O1,
                        Optional2 = transactionRequest.O2,
                        Optional3 = transactionRequest.O3,
                        Optional4 = transactionRequest.O4,
                        RefID = transactionRequest.RefID,
                        GEOCode = string.IsNullOrEmpty(transactionRequest.GeoCode) ? "25.5601,74.3401" : transactionRequest.GeoCode,
                        CustomerNumber = transactionRequest.CustomerNo,
                        RequestModeID = RequestMode.APPS,
                        IMEI = transactionRequest.IMEI
                    };
                    if (transactionServiceReq.GEOCode.Length > 15)
                    {
                        var a = transactionServiceReq.GEOCode.Split(',')[0];
                        var b = transactionServiceReq.GEOCode.Split(',')[1];
                        a = Validate.O.IsNumeric(a.Replace(".", "")) ? string.Format("{0:0.0000}", Convert.ToDecimal(a)) : "25.5601";
                        b = Validate.O.IsNumeric(b.Replace(".", "")) ? string.Format("{0:0.0000}", Convert.ToDecimal(b)) : "74.3401";

                        transactionServiceReq.GEOCode = a + "," + b;
                    }
                    var bbpsML = new BBPSML(_accessor, _env, false);
                    appBillfetchResponse.bBPSResponse = await bbpsML.FetchBillMLApp(transactionServiceReq).ConfigureAwait(false);
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
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "FetchBill",
                CommonStr2 = JsonConvert.SerializeObject(transactionRequest),
                CommonStr3 = JsonConvert.SerializeObject(appBillfetchResponse)
            });
            return Json(appBillfetchResponse);
        }
        #endregion
        #region PSATransaction
        [HttpPost]
        public async Task<IActionResult> PSATransaction([FromBody] AppTransactionReq transactionRequest)
        {
            var appTransactionRes = new AppTransactionRes
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvaildSession
            };
            var appRequest = new AppSessionReq
            {
                APPID = transactionRequest.APPID,
                IMEI = transactionRequest.IMEI,
                LoginTypeID = transactionRequest.LoginTypeID,
                UserID = transactionRequest.UserID,
                SessionID = transactionRequest.SessionID,
                RegKey = transactionRequest.RegKey,
                SerialNo = transactionRequest.SerialNo,
                Version = transactionRequest.Version,
                Session = transactionRequest.Session
            };
            var appResp = appML.CheckAppSession(appRequest);
            appTransactionRes.IsAppValid = appResp.IsAppValid;
            appTransactionRes.IsVersionValid = appResp.IsVersionValid;
            appTransactionRes.IsPasswordExpired = appResp.IsPasswordExpired;
            appTransactionRes.Statuscode = appResp.Statuscode;
            if (appResp.Statuscode == ErrorCodes.One)
            {
                if (!appResp.IsPasswordExpired)
                {
                    var appPSARequest = new _RechargeAPIRequest
                    {
                        UserID = transactionRequest.UserID,
                        Account = transactionRequest.AccountNo,//PANID
                        Amount = transactionRequest.Amount,//TotalToken
                        OID = transactionRequest.OID,
                        RequestMode = RequestMode.APPS,
                        SecurityKey = transactionRequest.SecurityKey ?? string.Empty,
                        IMEI = transactionRequest.IMEI,
                        OutletID = transactionRequest.OutletID
                    };
                    ISellerML sellerML = new SellerML(_accessor, _env, false);
                    var PSARes = await sellerML.PSATransaction(appPSARequest).ConfigureAwait(false);
                    appTransactionRes.Statuscode = PSARes.Statuscode;
                    appTransactionRes.Msg = PSARes.Msg;
                    appTransactionRes.TransactionID = PSARes.CommonStr;
                    appTransactionRes.LiveID = PSARes.CommonStr2;
                    appTransactionRes.IsAppValid = appResp.IsAppValid;
                    appTransactionRes.IsVersionValid = appResp.IsVersionValid;
                }
            }
            else
            {
                appTransactionRes.Msg = appResp.Msg;
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "PSATransaction",
                CommonStr2 = JsonConvert.SerializeObject(transactionRequest),
                CommonStr3 = JsonConvert.SerializeObject(appTransactionRes)
            });
            return Json(appTransactionRes);
        }
        #endregion
        #region ReportSection
        [HttpPost]
        public async Task<IActionResult> RechargeReport([FromBody] AppRechargeReportReq appRechargeReportReq)
        {
            var appRechargeRespose = new AppReportResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };

            var appResp = appML.CheckAppSession(appRechargeReportReq);
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
                        TransactionID = appRechargeReportReq.TransactionID,
                        OID = appRechargeReportReq.OID,
                        Status = appRechargeReportReq.Status,
                        TopRows = appRechargeReportReq.TopRows,
                        FromDate = appRechargeReportReq.FromDate,
                        ToDate = appRechargeReportReq.ToDate,
                        IsExport = appRechargeReportReq.IsExport,
                        LoginID = appRechargeReportReq.UserID,
                        LT = appRechargeReportReq.LoginTypeID,
                        OPTypeID = appRechargeReportReq.OpTypeID,
                        IsRecent = appRechargeReportReq.IsRecent,
                        OutletNo = appRechargeReportReq.ChildMobile
                    };

                    appRechargeRespose.Statuscode = ErrorCodes.One;
                    appRechargeRespose.Msg = ErrorCodes.SUCCESS;
                    appRechargeRespose.RechargeReport = await appReportML.GetAppRechargeReport(_filter).ConfigureAwait(false);
                }
            }
            else
            {
                appRechargeRespose.Msg = appResp.Msg;
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "RechargeReport",
                CommonStr2 = JsonConvert.SerializeObject(appRechargeReportReq),
                CommonStr3 = JsonConvert.SerializeObject(appRechargeRespose)
            });
            return Json(appRechargeRespose);
        }
        [HttpPost]
        public IActionResult TransactionReceipt([FromBody] AppReceiptRequest appReceiptRequest)
        {
            var appRechargeRespose = new AppReportResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };

            var appResp = appML.CheckAppSession(appReceiptRequest);
            appRechargeRespose.IsAppValid = appResp.IsAppValid;
            appRechargeRespose.IsVersionValid = appResp.IsVersionValid;
            appRechargeRespose.IsPasswordExpired = appResp.IsPasswordExpired;
            appRechargeRespose.Statuscode = appResp.Statuscode;
            if (appResp.Statuscode == ErrorCodes.One)
            {
                if (!appResp.IsPasswordExpired)
                {
                    SellerML sellerML = new SellerML(_accessor, _env, false);
                    appRechargeRespose.receiptDetail = sellerML.TransactionReceiptApp(appReceiptRequest.TID, appReceiptRequest.TransactionID, appReceiptRequest.ConvenientFee, appReceiptRequest.UserID);
                }
            }
            else
            {
                appRechargeRespose.Msg = appResp.Msg;
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "TransactionReceipt",
                CommonStr2 = JsonConvert.SerializeObject(appReceiptRequest),
                CommonStr3 = JsonConvert.SerializeObject(appRechargeRespose)
            });
            return Json(appRechargeRespose);
        }
        public async Task<IActionResult> DTHSubscriptionReport([FromBody] AppRechargeReportReq appRechargeReportReq)
        {
            var appRechargeRespose = new AppReportResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };

            var appResp = appML.CheckAppSession(appRechargeReportReq);
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
                        TransactionID = appRechargeReportReq.TransactionID,
                        OID = appRechargeReportReq.OID,
                        Status = appRechargeReportReq.Status,
                        TopRows = appRechargeReportReq.TopRows,
                        FromDate = appRechargeReportReq.FromDate,
                        ToDate = appRechargeReportReq.ToDate,
                        IsExport = appRechargeReportReq.IsExport,
                        LoginID = appRechargeReportReq.UserID,
                        LT = appRechargeReportReq.LoginTypeID,
                        OPTypeID = appRechargeReportReq.OpTypeID,
                        IsRecent = appRechargeReportReq.IsRecent,
                        OutletNo = appRechargeReportReq.ChildMobile,
                        BookingStatus = appRechargeReportReq.BookingStatus
                    };

                    appRechargeRespose.Statuscode = ErrorCodes.One;
                    appRechargeRespose.Msg = ErrorCodes.SUCCESS;
                    appRechargeRespose.DTHSubscriptions = await appReportML.GetDthsubscriptionReport(_filter).ConfigureAwait(false);
                }
            }
            else
            {
                appRechargeRespose.Msg = appResp.Msg;
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "DTHSubscriptionReport",
                CommonStr2 = JsonConvert.SerializeObject(appRechargeReportReq),
                CommonStr3 = JsonConvert.SerializeObject(appRechargeRespose)
            });
            return Json(appRechargeRespose);
        }
        [HttpPost]
        public async Task<IActionResult> AEPSReport([FromBody] AppRechargeReportReq appRechargeReportReq)
        {
            var appRechargeRespose = new AppReportResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            var appResp = appML.CheckAppSession(appRechargeReportReq);
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
                        TransactionID = appRechargeReportReq.TransactionID,
                        OID = appRechargeReportReq.OID,
                        Status = appRechargeReportReq.Status,
                        TopRows = appRechargeReportReq.TopRows,
                        FromDate = appRechargeReportReq.FromDate,
                        ToDate = appRechargeReportReq.ToDate,
                        IsExport = appRechargeReportReq.IsExport,
                        LoginID = appRechargeReportReq.UserID,
                        LT = appRechargeReportReq.LoginTypeID,
                        OPTypeID = appRechargeReportReq.OpTypeID,
                        IsRecent = appRechargeReportReq.IsRecent,
                        OutletNo = appRechargeReportReq.ChildMobile
                    };
                    appRechargeRespose.Statuscode = ErrorCodes.One;
                    appRechargeRespose.Msg = ErrorCodes.SUCCESS;
                    appRechargeRespose.AEPsDetail = await appReportML.GetAEPSReport(_filter).ConfigureAwait(false);
                }
            }
            else
            {
                appRechargeRespose.Msg = appResp.Msg;
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "AEPSReport",
                CommonStr2 = JsonConvert.SerializeObject(appRechargeReportReq),
                CommonStr3 = JsonConvert.SerializeObject(appRechargeRespose)
            });
            return Json(appRechargeRespose);
        }
        [HttpPost]
        public IActionResult RefundLog([FromBody] AppRefundLogReq appRefundLogReq)
        {
            var appRechargeRespose = new AppReportResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            var appRequest = new AppSessionReq
            {
                APPID = appRefundLogReq.APPID,
                IMEI = appRefundLogReq.IMEI,
                LoginTypeID = appRefundLogReq.LoginTypeID,
                UserID = appRefundLogReq.UserID,
                SessionID = appRefundLogReq.SessionID,
                RegKey = appRefundLogReq.RegKey,
                SerialNo = appRefundLogReq.SerialNo,
                Version = appRefundLogReq.Version,
                Session = appRefundLogReq.Session
            };
            var appResp = appML.CheckAppSession(appRequest);
            appRechargeRespose.IsAppValid = appResp.IsAppValid;
            appRechargeRespose.IsVersionValid = appResp.IsVersionValid;
            appRechargeRespose.IsPasswordExpired = appResp.IsPasswordExpired;
            appRechargeRespose.Statuscode = appResp.Statuscode;
            if (appResp.Statuscode == ErrorCodes.One)
            {
                if (!appResp.IsPasswordExpired)
                {
                    IAppReportML appReportML = new ReportML(_accessor, _env, false);
                    var filter = new RefundLogFilter
                    {
                        TopRows = appRefundLogReq.TopRows,
                        Status = appRefundLogReq.Status,
                        DateType = appRefundLogReq.DateType,
                        FromDate = appRefundLogReq.FromDate,
                        ToDate = appRefundLogReq.ToDate,
                        Criteria = appRefundLogReq.Criteria,
                        CriteriaText = appRefundLogReq.CriteriaText,
                        LoginTypeID = appRefundLogReq.LoginTypeID,
                        LoginID = appRefundLogReq.UserID
                    };
                    appRechargeRespose.Statuscode = ErrorCodes.One;
                    appRechargeRespose.Msg = ErrorCodes.SUCCESS;
                    appRechargeRespose.RefundLog = appReportML.GetRefundLogApp(filter);
                }
            }
            else
            {
                appRechargeRespose.Msg = appResp.Msg;
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "RefundLog",
                CommonStr2 = JsonConvert.SerializeObject(appRefundLogReq),
                CommonStr3 = JsonConvert.SerializeObject(appRechargeRespose)
            });
            return Json(appRechargeRespose);
        }
        [HttpPost]
        public async Task<IActionResult> LedgerReport([FromBody] AppLedgerReq appLedgerReq)
        {
            var appReportResponse = new AppReportResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            var appRequest = new AppSessionReq
            {
                APPID = appLedgerReq.APPID,
                IMEI = appLedgerReq.IMEI,
                LoginTypeID = appLedgerReq.LoginTypeID,
                UserID = appLedgerReq.UserID,
                SessionID = appLedgerReq.SessionID,
                RegKey = appLedgerReq.RegKey,
                SerialNo = appLedgerReq.SerialNo,
                Version = appLedgerReq.Version,
                Session = appLedgerReq.Session
            };
            var appResp = appML.CheckAppSession(appRequest);
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
                        LT = appLedgerReq.LoginTypeID,
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
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "LedgerReport",
                CommonStr2 = JsonConvert.SerializeObject(appLedgerReq),
                CommonStr3 = JsonConvert.SerializeObject(appReportResponse)
            });
            return Json(appReportResponse);
        }
        [HttpPost]
        public async Task<IActionResult> FundDCReport([FromBody] AppFundDCReq appFundDCReq)
        {
            var appReportResponse = new AppReportResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            var appRequest = new AppSessionReq
            {
                APPID = appFundDCReq.APPID,
                IMEI = appFundDCReq.IMEI,
                LoginTypeID = appFundDCReq.LoginTypeID,
                UserID = appFundDCReq.UserID,
                SessionID = appFundDCReq.SessionID,
                RegKey = appFundDCReq.RegKey,
                SerialNo = appFundDCReq.SerialNo,
                Version = appFundDCReq.Version,
                Session = appFundDCReq.Session
            };
            var appResp = appML.CheckAppSession(appRequest);
            appReportResponse.IsAppValid = appResp.IsAppValid;
            appReportResponse.IsVersionValid = appResp.IsVersionValid;
            appReportResponse.IsPasswordExpired = appResp.IsPasswordExpired;
            appReportResponse.Statuscode = appResp.Statuscode;
            if (appResp.Statuscode == ErrorCodes.One)
            {
                if (!appResp.IsPasswordExpired)
                {
                    IAppReportML appReportML = new ReportML(_accessor, _env, false);
                    var _filter = new ULFundReceiveReportFilter
                    {
                        ServiceID = appFundDCReq.ServiceID,
                        LoginId = appFundDCReq.UserID,
                        MobileNo = appFundDCReq.AccountNo,
                        TID = appFundDCReq.TransactionID,
                        FDate = appFundDCReq.FromDate,
                        TDate = appFundDCReq.ToDate,
                        IsSelf = appFundDCReq.IsSelf,
                        WalletTypeID = appFundDCReq.WalletTypeID,
                        OtherUserMob = appFundDCReq.OtherUserMob,
                        LT = appFundDCReq.LoginTypeID,
                        UserID = appFundDCReq.UserID
                    };
                    appReportResponse.Statuscode = ErrorCodes.One;
                    appReportResponse.Msg = ErrorCodes.SUCCESS;
                    appReportResponse.FundDCReport = await appReportML.GetAppUserFundReceive(_filter);
                }
            }
            else
            {
                appReportResponse.Msg = appResp.Msg;
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "FundDCReport",
                CommonStr2 = JsonConvert.SerializeObject(appFundDCReq),
                CommonStr3 = JsonConvert.SerializeObject(appReportResponse)
            });
            return Json(appReportResponse);
        }
        [HttpPost]
        public async Task<IActionResult> FundOrderReport([FromBody] AppFundOrderReportReq appFundOrderReportReq)
        {
            var appReportResponse = new AppReportResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            var appRequest = new AppSessionReq
            {
                APPID = appFundOrderReportReq.APPID,
                IMEI = appFundOrderReportReq.IMEI,
                LoginTypeID = appFundOrderReportReq.LoginTypeID,
                UserID = appFundOrderReportReq.UserID,
                SessionID = appFundOrderReportReq.SessionID,
                RegKey = appFundOrderReportReq.RegKey,
                SerialNo = appFundOrderReportReq.SerialNo,
                Version = appFundOrderReportReq.Version,
                Session = appFundOrderReportReq.Session
            };
            var appResp = appML.CheckAppSession(appRequest);
            appReportResponse.IsAppValid = appResp.IsAppValid;
            appReportResponse.IsVersionValid = appResp.IsVersionValid;
            appReportResponse.IsPasswordExpired = appResp.IsPasswordExpired;
            appReportResponse.Statuscode = appResp.Statuscode;
            if (appResp.Statuscode == ErrorCodes.One)
            {
                if (!appResp.IsPasswordExpired)
                {
                    IAppReportML appReportML = new ReportML(_accessor, _env, false);
                    var _filter = new FundOrderFilter
                    {
                        LT = appFundOrderReportReq.LoginTypeID,
                        LoginID = appFundOrderReportReq.UserID,
                        FromDate = appFundOrderReportReq.FromDate ?? DateTime.Now.ToString("dd MMM yyyy"),
                        ToDate = appFundOrderReportReq.ToDate ?? DateTime.Now.ToString("dd MMM yyyy"),
                        UMobile = appFundOrderReportReq.UMobile ?? "",
                        TMode = appFundOrderReportReq.TMode,
                        RSts = appFundOrderReportReq.Status,
                        AccountNo = appFundOrderReportReq.AccountNo ?? "",
                        TransactionID = appFundOrderReportReq.TransactionID ?? "",
                        Top = appFundOrderReportReq.TopRows,
                        IsSelf = appFundOrderReportReq.IsSelf
                    };
                    appReportResponse.Statuscode = ErrorCodes.One;
                    appReportResponse.Msg = ErrorCodes.SUCCESS;
                    await Task.Delay(0);
                    appReportResponse.FundOrderReport = appReportML.GetUserFundReportApp(_filter);
                }
            }
            else
            {
                appReportResponse.Msg = appResp.Msg;
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "FundOrderReport",
                CommonStr2 = JsonConvert.SerializeObject(appFundOrderReportReq),
                CommonStr3 = JsonConvert.SerializeObject(appReportResponse)
            });
            return Json(appReportResponse);
        }
        [HttpPost]
        public async Task<IActionResult> UserDaybook([FromBody] AppReportCommon appReportCommon)
        {
            var appReportResponse = new AppReportResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            var appRequest = new AppSessionReq
            {
                APPID = appReportCommon.APPID,
                IMEI = appReportCommon.IMEI,
                LoginTypeID = appReportCommon.LoginTypeID,
                UserID = appReportCommon.UserID,
                SessionID = appReportCommon.SessionID,
                RegKey = appReportCommon.RegKey,
                SerialNo = appReportCommon.SerialNo,
                Version = appReportCommon.Version,
                Session = appReportCommon.Session
            };
            var appResp = appML.CheckAppSession(appRequest);
            appReportResponse.IsAppValid = appResp.IsAppValid;
            appReportResponse.IsVersionValid = appResp.IsVersionValid;
            appReportResponse.IsPasswordExpired = appResp.IsPasswordExpired;
            appReportResponse.Statuscode = appResp.Statuscode;
            if (appResp.Statuscode == ErrorCodes.One)
            {
                if (!appResp.IsPasswordExpired)
                {
                    IAppReportML appReportML = new ReportML(_accessor, _env, false);
                    var commonReq = new CommonReq
                    {
                        CommonStr = appReportCommon.FromDate ?? DateTime.Now.ToString("dd MMM yyyy"),

                        LoginTypeID = appReportCommon.LoginTypeID,
                        LoginID = appReportCommon.UserID,
                        CommonStr2 = appReportCommon.AccountNo ?? "" //Child UserMobileNo/ID
                    };
                    commonReq.CommonStr3 = appReportCommon.ToDate ?? commonReq.CommonStr;
                    appReportResponse.Statuscode = ErrorCodes.One;
                    appReportResponse.Msg = ErrorCodes.SUCCESS;
                    await Task.Delay(0);
                    appReportResponse.UserDaybookReport = appReportML.UserDaybookApp(commonReq);
                }
            }
            else
            {
                appReportResponse.Msg = appResp.Msg;
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "UserDaybook",
                CommonStr2 = JsonConvert.SerializeObject(appReportCommon),
                CommonStr3 = JsonConvert.SerializeObject(appReportResponse)
            });
            return Json(appReportResponse);
        }
        [HttpPost]
        public async Task<IActionResult> UserDaybookDMR([FromBody] AppReportCommon appReportCommon)
        {
            var appReportResponse = new AppReportResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            var appRequest = new AppSessionReq
            {
                APPID = appReportCommon.APPID,
                IMEI = appReportCommon.IMEI,
                LoginTypeID = appReportCommon.LoginTypeID,
                UserID = appReportCommon.UserID,
                SessionID = appReportCommon.SessionID,
                RegKey = appReportCommon.RegKey,
                SerialNo = appReportCommon.SerialNo,
                Version = appReportCommon.Version,
                Session = appReportCommon.Session
            };
            var appResp = appML.CheckAppSession(appRequest);
            appReportResponse.IsAppValid = appResp.IsAppValid;
            appReportResponse.IsVersionValid = appResp.IsVersionValid;
            appReportResponse.IsPasswordExpired = appResp.IsPasswordExpired;
            appReportResponse.Statuscode = appResp.Statuscode;
            if (appResp.Statuscode == ErrorCodes.One)
            {
                if (!appResp.IsPasswordExpired)
                {
                    IAppReportML appReportML = new ReportML(_accessor, _env, false);
                    var commonReq = new CommonReq
                    {
                        CommonStr = appReportCommon.FromDate ?? DateTime.Now.ToString("dd MMM yyyy"),
                        LoginTypeID = appReportCommon.LoginTypeID,
                        LoginID = appReportCommon.UserID,
                        CommonStr2 = appReportCommon.AccountNo ?? "" //Child UserMobileNo/ID
                    };
                    commonReq.CommonStr3 = appReportCommon.ToDate ?? commonReq.CommonStr;
                    appReportResponse.Statuscode = ErrorCodes.One;
                    appReportResponse.Msg = ErrorCodes.SUCCESS;
                    await Task.Delay(0);
                    appReportResponse.UserDaybookDMRReport = appReportML.UserDaybookDMRApp(commonReq);
                }
            }
            else
            {
                appReportResponse.Msg = appResp.Msg;
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "UserDaybookDMR",
                CommonStr2 = JsonConvert.SerializeObject(appReportCommon),
                CommonStr3 = JsonConvert.SerializeObject(appReportResponse)
            });
            return Json(appReportResponse);
        }
        [HttpPost]
        public async Task<IActionResult> DMTReport([FromBody] AppDMTReportReq appDMTReportReq)
        {
            var appRechargeRespose = new AppReportResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };

            var appResp = appML.CheckAppSession(appDMTReportReq);
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
                        AccountNo = appDMTReportReq.AccountNo,
                        TransactionID = appDMTReportReq.TransactionID,
                        Status = appDMTReportReq.Status,
                        TopRows = appDMTReportReq.TopRows,
                        FromDate = appDMTReportReq.FromDate,
                        ToDate = appDMTReportReq.ToDate,
                        IsExport = appDMTReportReq.IsExport,
                        LoginID = appDMTReportReq.UserID,
                        LT = appDMTReportReq.LoginTypeID,
                        OutletNo = appDMTReportReq.ChildMobile
                    };

                    appRechargeRespose.Statuscode = ErrorCodes.One;
                    appRechargeRespose.Msg = ErrorCodes.SUCCESS;
                    appRechargeRespose.DMTReport = await appReportML.GetAppDMRReport(_filter);
                }
            }
            else
            {
                appRechargeRespose.Msg = appResp.Msg;
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "DMTReport",
                CommonStr2 = JsonConvert.SerializeObject(appDMTReportReq),
                CommonStr3 = JsonConvert.SerializeObject(appRechargeRespose)
            });
            return Json(appRechargeRespose);
        }
        [HttpPost]
        public async Task<IActionResult> FundOrderPending([FromBody] AppSessionReq appSessionReq)
        {
            var appReportResponse = new AppReportResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };

            var appResp = appML.CheckAppSession(appSessionReq);
            appReportResponse.IsAppValid = appResp.IsAppValid;
            appReportResponse.IsVersionValid = appResp.IsVersionValid;
            appReportResponse.IsPasswordExpired = appResp.IsPasswordExpired;
            appReportResponse.Statuscode = appResp.Statuscode;
            if (appResp.Statuscode == ErrorCodes.One)
            {
                if (!appResp.IsPasswordExpired)
                {
                    IAppReportML appReportML = new ReportML(_accessor, _env, false);
                    var commonReq = new CommonReq
                    {
                        LoginTypeID = appSessionReq.LoginTypeID,
                        LoginID = appSessionReq.UserID
                    };
                    appReportResponse.Statuscode = ErrorCodes.One;
                    appReportResponse.Msg = ErrorCodes.SUCCESS;
                    await Task.Delay(0);
                    appReportResponse.FundRequestForApproval = appReportML.GetUserFundReportApprovalApp(commonReq);
                }
            }
            else
            {
                appReportResponse.Msg = appResp.Msg;
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "FundOrderPending",
                CommonStr2 = JsonConvert.SerializeObject(appSessionReq),
                CommonStr3 = JsonConvert.SerializeObject(appReportResponse)
            });
            return Json(appReportResponse);
        }
        [HttpPost]
        public async Task<IActionResult> GetAppNews([FromBody] AppNewsRequest appNewsRequest)
        {
            var appReportResponse = new AppReportResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };

            var appResp = appML.CheckAppSession(appNewsRequest);
            appReportResponse.IsAppValid = appResp.IsAppValid;
            appReportResponse.IsVersionValid = appResp.IsVersionValid;
            appReportResponse.IsPasswordExpired = appResp.IsPasswordExpired;
            appReportResponse.Statuscode = appResp.Statuscode;

            if (appResp.Statuscode == ErrorCodes.One)
            {
                INewsAppML newsAppML = new NewsML(_accessor, _env, false);
                var commonReq = new CommonReq
                {
                    LoginTypeID = appNewsRequest.LoginTypeID,
                    LoginID = appNewsRequest.UserID,
                    IsListType = appNewsRequest.IsLoginNews,
                    CommonInt = appResp.CheckID
                };
                appReportResponse.Statuscode = ErrorCodes.One;
                appReportResponse.Msg = ErrorCodes.SUCCESS;
                await Task.Delay(0);
                appReportResponse.NewsContent = newsAppML.GetNewsRoleNewsForApp(commonReq);
            }
            else
            {
                appReportResponse.Msg = appResp.Msg;
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "GetAppNews",
                CommonStr2 = JsonConvert.SerializeObject(appNewsRequest),
                CommonStr3 = JsonConvert.SerializeObject(appReportResponse)
            });
            return Json(appReportResponse);
        }
        [HttpPost]
        public async Task<IActionResult> RefundRequest([FromBody] AppRefundRequest appRefundRequest)
        {
            var appResponse = new AppDoubleFactorResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };

            var appResp = appML.CheckAppSession(appRefundRequest);
            appResponse.IsAppValid = appResp.IsAppValid;
            appResponse.IsVersionValid = appResp.IsVersionValid;
            appResponse.IsPasswordExpired = appResp.IsPasswordExpired;
            appResponse.Statuscode = appResp.Statuscode;
            if (appResponse.Statuscode == ErrorCodes.One)
            {
                if (!appResponse.IsPasswordExpired)
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
                    appResponse.Statuscode = resRR.Statuscode;
                    appResponse.Msg = resRR.Msg;
                    appResponse.IsOTPRequired = resRR.IsOTPRequired;
                }
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "RefundRequest",
                CommonStr2 = JsonConvert.SerializeObject(appRefundRequest),
                CommonStr3 = JsonConvert.SerializeObject(appResponse)
            });
            return Json(appResponse);
        }
        [HttpPost]
        public async Task<IActionResult> WTRLog([FromBody] AppRefundLogReq appWTRReq)
        {
            var appRechargeRespose = new AppReportResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };

            var appResp = appML.CheckAppSession(appWTRReq);
            appRechargeRespose.IsAppValid = appResp.IsAppValid;
            appRechargeRespose.IsVersionValid = appResp.IsVersionValid;
            appRechargeRespose.IsPasswordExpired = appResp.IsPasswordExpired;
            appRechargeRespose.Statuscode = appResp.Statuscode;
            await Task.Delay(0);
            if (appResp.Statuscode == ErrorCodes.One)
            {
                if (!appResp.IsPasswordExpired)
                {
                    IAppReportML appReportML = new ReportML(_accessor, _env, false);
                    var filter = new _RefundLogFilter
                    {
                        LoginTypeID = appWTRReq.LoginTypeID,
                        LoginID = appWTRReq.UserID,
                        TopRows = appWTRReq.TopRows,
                        Status = appWTRReq.Status,
                        OID = appWTRReq.OID,
                        DateType = appWTRReq.DateType,
                        FromDate = appWTRReq.FromDate,
                        ToDate = appWTRReq.ToDate,
                        Criteria = appWTRReq.Criteria,
                        CriteriaText = (appWTRReq.CriteriaText ?? "").Trim()
                    };
                    appRechargeRespose.Statuscode = ErrorCodes.One;
                    appRechargeRespose.Msg = ErrorCodes.SUCCESS;
                    appRechargeRespose.RefundLog = appReportML.GetWTRLogApp(filter);
                }
            }
            else
            {
                appRechargeRespose.Msg = appResp.Msg;
            }

            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "WTRLog",
                CommonStr2 = JsonConvert.SerializeObject(appWTRReq),
                CommonStr3 = JsonConvert.SerializeObject(appRechargeRespose)
            });
            return Json(appRechargeRespose);
        }
        public async Task<IActionResult> MoveToBankReport([FromBody] AppReportCommon appReportCommon)
        {
            var appRechargeRespose = new AppReportResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };

            var appResp = appML.CheckAppSession(appReportCommon);
            appRechargeRespose.IsAppValid = appResp.IsAppValid;
            appRechargeRespose.IsVersionValid = appResp.IsVersionValid;
            appRechargeRespose.IsPasswordExpired = appResp.IsPasswordExpired;
            appRechargeRespose.Statuscode = appResp.Statuscode;
            await Task.Delay(0);
            if (appResp.Statuscode == ErrorCodes.One)
            {
                if (!appResp.IsPasswordExpired)
                {
                    IAppReportML appReportML = new ReportML(_accessor, _env, false);
                    var filter = new WalletRequest
                    {
                        LoginTypeID = appReportCommon.LoginTypeID,
                        LoginID = appReportCommon.UserID,
                        Status = appReportCommon.Status,
                        FDate = appReportCommon.FromDate,
                        TDate = appReportCommon.ToDate,
                        CommonStr = appReportCommon.TransMode,
                        ID = 0,
                        Mobile = appReportCommon.ChildMobile,
                        TransactionId = appReportCommon.TransactionID,
                        CommonInt = appReportCommon.Status,
                        CommonInt2 = 50,
                        CommonInt3 = appReportCommon.OID
                    };
                    appRechargeRespose.Statuscode = ErrorCodes.One;
                    appRechargeRespose.Msg = ErrorCodes.SUCCESS;
                    appRechargeRespose.MoveToWalletReport = appReportML.appGetWalletRequestReport(filter);
                }
            }
            else
            {
                appRechargeRespose.Msg = appResp.Msg;
            }

            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "MoveToBankReport",
                CommonStr2 = JsonConvert.SerializeObject(appReportCommon),
                CommonStr3 = JsonConvert.SerializeObject(appRechargeRespose)
            });
            return Json(appRechargeRespose);
        }
        #endregion
        #region Target
        [HttpPost]
        public async Task<IActionResult> GetTargetAchieved([FromBody] AppTargetRequest appTargetRequest)
        {
            var appRechargeRespose = new AppReportResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };

            var appResp = appML.CheckAppSession(appTargetRequest);
            appRechargeRespose.IsAppValid = appResp.IsAppValid;
            appRechargeRespose.IsVersionValid = appResp.IsVersionValid;
            appRechargeRespose.IsPasswordExpired = appResp.IsPasswordExpired;
            appRechargeRespose.Statuscode = appResp.Statuscode;
            if (appResp.Statuscode == ErrorCodes.One)
            {
                if (!appResp.IsPasswordExpired)
                {
                    IAppReportML appReportML = new ReportML(_accessor, _env, false);
                    var _filter = new CommonReq
                    {
                        LoginTypeID = appTargetRequest.LoginTypeID,
                        LoginID = appTargetRequest.UserID,
                        IsListType = appTargetRequest.IsTotal
                    };
                    appRechargeRespose.Statuscode = ErrorCodes.One;
                    appRechargeRespose.Msg = ErrorCodes.SUCCESS;
                    appRechargeRespose.TargetAchieveds = await appReportML.GetTargetAchieveds(_filter);
                }
            }
            else
            {
                appRechargeRespose.Msg = appResp.Msg;
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "GetTargetAchieved",
                CommonStr2 = JsonConvert.SerializeObject(appTargetRequest),
                CommonStr3 = JsonConvert.SerializeObject(appRechargeRespose)
            });
            return Json(appRechargeRespose);
        }
        #endregion
        #region UserRelated
        [HttpPost]
        public async Task<IActionResult> AppUserReffDetail([FromBody] AppUserRequest appUserRequest)
        {
            var appResp = new AppUserRefferalDetail
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (appUserRequest.LoginTypeID == LoginType.ApplicationUser)
            {
                var appResponse = appML.CheckAppSession(appUserRequest);
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
                        var userRequset = new UserRequset
                        {
                            LoginTypeID = appUserRequest.LoginTypeID,
                            LoginID = appUserRequest.UserID,
                            MobileNo = (appUserRequest.RefferalID ?? "") == "" ? appUserRequest.UserID.ToString() : appUserRequest.RefferalID
                        };
                        IUserAPPML userML = new UserML(_accessor, _env, false);
                        await Task.Delay(0);
                        appResp.userRegModel = userML.GetAppUserReffDeatil(userRequset);
                    }
                }
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "AppUserReffDetail",
                CommonStr2 = JsonConvert.SerializeObject(appUserRequest),
                CommonStr3 = JsonConvert.SerializeObject(appResp)
            });
            return Json(appResp);
        }
        [HttpPost]
        public async Task<IActionResult> AppUserRegistraion([FromBody] AppUserCreate appUserCreate)
        {
            var appResp = new AppResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (appUserCreate.LoginTypeID == LoginType.ApplicationUser)
            {

                var appResponse = appML.CheckAppSession(appUserCreate);
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
                        appUserCreate.userCreate.RequestModeID = RequestMode.APPS;
                        appUserCreate.userCreate.LoginID = appUserCreate.UserID;
                        appUserCreate.userCreate.LTID = appUserCreate.LoginTypeID;
                        IUserAPPML userML = new UserML(_accessor, _env, false);
                        await Task.Delay(0).ConfigureAwait(false);
                        var resp = userML.CallCreateUserApp(appUserCreate.userCreate);
                        appResp.Statuscode = resp.Statuscode;
                        appResp.Msg = resp.Msg;
                    }
                }
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "AppUserRegistraion",
                CommonStr2 = JsonConvert.SerializeObject(appUserCreate),
                CommonStr3 = JsonConvert.SerializeObject(appResp)
            });
            return Json(appResp);
        }
        [HttpPost]
        public async Task<IActionResult> AppUserSignup([FromBody] AppUserCreate appUserCreate)
        {
            var appResp = new AppResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };

            appUserCreate.LoginTypeID = LoginType.ApplicationUser;
            var appResponse = appML.CheckApp(appUserCreate);
            appResp.Statuscode = appResponse.Statuscode;
            appResp.Msg = appResponse.Msg;
            appResp.IsAppValid = appResponse.IsAppValid;
            appResp.IsVersionValid = appResponse.IsVersionValid;

            appResp.Statuscode = appResponse.Statuscode;
            if (appResp.Statuscode == ErrorCodes.One)
            {
                appUserCreate.userCreate.RequestModeID = RequestMode.APPS;
                ILoginML _loginML = new LoginML(_accessor, _env, appUserCreate.Domain);
                var webInfo = _loginML.GetWebsiteForDomain();
                appUserCreate.userCreate.WID = webInfo.WID;
                IUserAPPML userML = new UserML(_accessor, _env, false);
                await Task.Delay(0);
                var resp = userML.CallSignupFromApp(appUserCreate.userCreate);
                appResp.Statuscode = resp.Statuscode;
                appResp.Msg = resp.Msg;
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "AppUserSignup",
                CommonStr2 = JsonConvert.SerializeObject(appUserCreate),
                CommonStr3 = JsonConvert.SerializeObject(appResp)
            });
            return Json(appResp);
        }
        [HttpPost]
        public async Task<IActionResult> AppUserList([FromBody] AppUserRequest appUserRequest)
        {
            var appResp = new AppReportResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (appUserRequest.LoginTypeID == LoginType.ApplicationUser)
            {
                var appResponse = appML.CheckAppSession(appUserRequest);
                appResp.Statuscode = appResponse.Statuscode;
                appResp.Msg = appResponse.Msg;
                appResp.IsPasswordExpired = appResponse.IsPasswordExpired;
                appResp.Statuscode = appResponse.Statuscode;
                if (appResp.Statuscode == ErrorCodes.One)
                {
                    if (!appResp.IsPasswordExpired)
                    {
                        var req = new CommonReq
                        {
                            LoginID = appUserRequest.UserID,
                            LoginTypeID = appUserRequest.LoginTypeID,
                            CommonInt2 = appUserRequest.RoleID,
                            CommonInt = appUserRequest.UserID,
                            CommonBool = true
                        };
                        IUserAPPML userML = new UserML(_accessor, _env, false);
                        await Task.Delay(0);
                        IEnumerable<UserReportBulk> UserList = userML.BulkActionApp(req);
                        appResp.UserList = UserList;
                    }
                }
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "AppUserList",
                CommonStr2 = JsonConvert.SerializeObject(appUserRequest),
                CommonStr3 = JsonConvert.SerializeObject(appResp)
            });
            return Json(appResp);
        }
        [HttpPost]
        public async Task<IActionResult> AppUserChildRoles([FromBody] AppSessionReq appSessionReq)
        {
            var appResp = new AppReportResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (appSessionReq.LoginTypeID == LoginType.ApplicationUser)
            {
                var appResponse = appML.CheckAppSession(appSessionReq);
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
                        IUserAPPML userML = new UserML(_accessor, _env, false);
                        await Task.Delay(0);
                        IEnumerable<RoleMaster> UserRoles = userML.GetUserChildRolesApp(appSessionReq.UserID);
                        appResp.ChildRoles = UserRoles;
                    }
                }
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "AppUserChildRoles",
                CommonStr2 = JsonConvert.SerializeObject(appSessionReq),
                CommonStr3 = JsonConvert.SerializeObject(appResp)
            });
            return Json(appResp);
        }
        [HttpPost]
        public async Task<IActionResult> ChangeUserStatus([FromBody] AppUserRequest appSessionReq)
        {
            var appResponse = new AppResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (appSessionReq.LoginTypeID == LoginType.ApplicationUser)
            {
                var appRequest = new AppSessionReq
                {
                    APPID = appSessionReq.APPID ?? "",
                    IMEI = appSessionReq.IMEI ?? "",
                    LoginTypeID = appSessionReq.LoginTypeID,
                    UserID = appSessionReq.UserID,
                    SessionID = appSessionReq.SessionID,
                    RegKey = appSessionReq.RegKey ?? "",
                    SerialNo = appSessionReq.SerialNo ?? "",
                    Version = appSessionReq.Version ?? "",
                    Session = appSessionReq.Session
                };
                appResponse = appML.CheckAppSession(appRequest);
                if (appResponse.Statuscode == ErrorCodes.One)
                {
                    if (!appResponse.IsPasswordExpired)
                    {
                        IUserAPPML userML = new UserML(_accessor, _env, false);
                        await Task.Delay(0);
                        var procToggleStatusRequest = new ProcToggleStatusRequest
                        {
                            LTID = appSessionReq.LoginTypeID,
                            LoginID = appSessionReq.UserID,
                            UserID = appSessionReq.UID
                        };
                        var RespSts = userML.ChangeUserStatusFromApp(procToggleStatusRequest);
                        appResponse.Statuscode = RespSts.Statuscode;
                        appResponse.Msg = RespSts.Msg;
                    }
                }
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "ChangeUserStatus",
                CommonStr2 = JsonConvert.SerializeObject(appSessionReq),
                CommonStr3 = JsonConvert.SerializeObject(appResponse)
            });
            return Json(appResponse);
        }
        [HttpPost]
        public IActionResult ChangeDFStatus([FromBody] AppDoubleFactorReq appSessionReq)
        {
            var appDoubleFactorResp = new AppDoubleFactorResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (appSessionReq.LoginTypeID == LoginType.ApplicationUser)
            {
                var appRequest = new AppSessionReq
                {
                    APPID = appSessionReq.APPID ?? "",
                    IMEI = appSessionReq.IMEI ?? "",
                    LoginTypeID = appSessionReq.LoginTypeID,
                    UserID = appSessionReq.UserID,
                    SessionID = appSessionReq.SessionID,
                    RegKey = appSessionReq.RegKey ?? "",
                    SerialNo = appSessionReq.SerialNo ?? "",
                    Version = appSessionReq.Version ?? "",
                    Session = appSessionReq.Session
                };
                var appResponse = appML.CheckAppSession(appRequest);
                appDoubleFactorResp.IsAppValid = appResponse.IsAppValid;
                appDoubleFactorResp.IsVersionValid = appResponse.IsVersionValid;
                appDoubleFactorResp.IsPasswordExpired = appResponse.IsPasswordExpired;
                if (appResponse.Statuscode == ErrorCodes.One)
                {
                    if (!appResponse.IsPasswordExpired)
                    {
                        IResponseStatus _res = new ResponseStatus
                        {
                            Statuscode = ErrorCodes.Minus1,
                            Msg = ErrorCodes.AuthError
                        };
                        IUserML userML = new UserML(_accessor, _env, false);
                        var procToggleStatusRequest = new ProcToggleStatusRequest
                        {
                            LTID = appSessionReq.LoginTypeID,
                            LoginID = appSessionReq.UserID,
                            UserID = appSessionReq.UserID,
                            Is = appSessionReq.IsDoubleFactor
                        };
                        var CallMethod = false;
                        if (!appSessionReq.IsDoubleFactor)
                        {
                            bool IsResend = (appSessionReq.OTP ?? "").ToLower().Equals(nameof(appSessionReq.OTP).ToLower());
                            var otpres = userML.GetOTPFromURL(appSessionReq.RefID);
                            var _otp = otpres.CommonStr ?? "";
                            if ((appSessionReq.OTP ?? "").Trim().Length > 0 && !IsResend)
                            {
                                if (!_otp.Equals(appSessionReq.OTP))
                                {
                                    _res.Msg = nameof(ErrorCodes.Invalid_OTP).Replace("_", " ");
                                }
                                else
                                {
                                    CallMethod = true;
                                }
                            }
                            else
                            {
                                IsResend = (_otp ?? "").Length == 6 ? IsResend : false;
                                var o = (_otp ?? "").Length == 6 ? _otp : HashEncryption.O.CreatePasswordNumeric(6);

                                IUserML uml = new UserML(_accessor, _env);
                                var alertData = uml.GetUserDeatilForAlert(appSessionReq.UserID);
                                if (alertData.Statuscode == ErrorCodes.One)
                                {
                                    IAlertML alertMl = new AlertML(_accessor, _env);
                                    alertData.OTP = o;
                                    _res = alertMl.OTPSMS(alertData);
                                    alertMl.OTPEmail(alertData);
                                }
                                if (_res.Statuscode == ErrorCodes.One)
                                {
                                    _res.Statuscode = ErrorCodes.One;
                                    if (!IsResend)
                                    {
                                        appDoubleFactorResp.RefID = HashEncryption.O.EncryptForURL(appSessionReq.LoginTypeID + "_" + appSessionReq.UserID + "_" + o + "_" + DateTime.Now.ToString("dd|MMM|yyyy hh:m:s tt"));
                                        appDoubleFactorResp.IsOTPRequired = true;
                                        _res.Msg = "OTP has been sent successfully!";
                                    }
                                    else
                                    {
                                        appDoubleFactorResp.RefID = appSessionReq.RefID;
                                        appDoubleFactorResp.IsOTPRequired = true;
                                        _res.Msg = "OTP has been resend successfully!";
                                    }
                                }
                                else
                                {
                                    _res.Msg = "OTP couldn't not be sent!";
                                }
                            }
                        }
                        else
                        {
                            CallMethod = true;

                        }
                        if (CallMethod)
                        {
                            _res = userML.ChangeDoubleFactorFromApp(procToggleStatusRequest);
                        }
                        appDoubleFactorResp.Statuscode = _res.Statuscode;
                        appDoubleFactorResp.Msg = _res.Msg;
                    }
                }
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "ChangeDFStatus",
                CommonStr2 = JsonConvert.SerializeObject(appSessionReq),
                CommonStr3 = JsonConvert.SerializeObject(appDoubleFactorResp)
            });
            return Json(appDoubleFactorResp);
        }
        [HttpPost]
        public IActionResult ChangeRealAPIStatus([FromBody] AppRealAPIChangeReq appSessionReq)
        {
            var appResponse = new AppResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (appSessionReq.LoginTypeID == LoginType.ApplicationUser)
            {
                appResponse = appML.CheckAppSession(appSessionReq);
                if (appResponse.Statuscode == ErrorCodes.One)
                {
                    if (!appResponse.IsPasswordExpired)
                    {
                        ISlabML slabML = new SlabML(_accessor, _env, false);
                        var commonReq = new CommonReq
                        {
                            LoginID = appSessionReq.UserID,
                            CommonBool = appSessionReq.Is
                        };
                        var _res = slabML.RealAPIStatusUpdate(commonReq);
                        appResponse.Statuscode = _res.Statuscode;
                        appResponse.Msg = _res.Msg;
                    }
                }
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "ChangeRealAPIStatus",
                CommonStr2 = JsonConvert.SerializeObject(appSessionReq),
                CommonStr3 = JsonConvert.SerializeObject(appResponse)
            });
            return Json(appResponse);
        }
        [HttpPost]
        public IActionResult ChangePinOrPassword([FromBody] AppChangePRequest appSessionReq)
        {
            var appResponse = new AppResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (appSessionReq.LoginTypeID == LoginType.ApplicationUser)
            {
                var appRequest = new AppSessionReq
                {
                    APPID = appSessionReq.APPID ?? "",
                    IMEI = appSessionReq.IMEI ?? "",
                    LoginTypeID = appSessionReq.LoginTypeID,
                    UserID = appSessionReq.UserID,
                    SessionID = appSessionReq.SessionID,
                    RegKey = appSessionReq.RegKey ?? "",
                    SerialNo = appSessionReq.SerialNo ?? "",
                    Version = appSessionReq.Version ?? "",
                    Session = appSessionReq.Session
                };
                appResponse = appML.CheckAppSession(appRequest);
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
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "ChangePinOrPassword",
                CommonStr2 = JsonConvert.SerializeObject(appSessionReq),
                CommonStr3 = JsonConvert.SerializeObject(appResponse)
            });
            return Json(appResponse);
        }
        [HttpPost]
        public IActionResult FOSRetailerList([FromBody] AppUserRequest appUserRequest)
        {
            var appResp = new AppReportResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (appUserRequest.LoginTypeID == LoginType.ApplicationUser)
            {
                var appResponse = appML.CheckAppSession(appUserRequest);
                appResp.Statuscode = appResponse.Statuscode;
                appResp.Msg = appResponse.Msg;
                appResp.IsPasswordExpired = appResponse.IsPasswordExpired;
                appResp.Statuscode = appResponse.Statuscode;
                if (appResp.Statuscode == ErrorCodes.One)
                {
                    if (!appResp.IsPasswordExpired)
                    {
                        var req = new CommonReq
                        {
                            LoginID = appUserRequest.UserID,
                            LoginTypeID = appUserRequest.LoginTypeID,
                            CommonInt2 = appUserRequest.RoleID,
                            CommonInt = appUserRequest.UserID,
                            CommonBool = true
                        };
                        IUserAPPML userML = new UserML(_accessor, _env, false);
                        UserList UserList = userML.GetFOSList(req);
                        appResp.FOSList = UserList;
                    }
                }
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "AppFOSUserList",
                CommonStr2 = JsonConvert.SerializeObject(appUserRequest),
                CommonStr3 = JsonConvert.SerializeObject(appResp)
            });
            return Json(appResp);
        }
        #endregion
        #region UserProfileSection
        [HttpPost]
        public async Task<IActionResult> GetProfile([FromBody] AppUserRequest appUserRequest)
        {
            var appResp = new AppUserProfile
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (appUserRequest.LoginTypeID == LoginType.ApplicationUser)
            {
                var appRequest = new AppSessionReq
                {
                    APPID = appUserRequest.APPID ?? "",
                    IMEI = appUserRequest.IMEI ?? "",
                    LoginTypeID = appUserRequest.LoginTypeID,
                    UserID = appUserRequest.UserID,
                    SessionID = appUserRequest.SessionID,
                    RegKey = appUserRequest.RegKey ?? "",
                    SerialNo = appUserRequest.SerialNo ?? "",
                    Version = appUserRequest.Version ?? "",
                    Session = appUserRequest.Session
                };
                var appResponse = appML.CheckAppSession(appRequest);
                appResp.Statuscode = appResponse.Statuscode;
                appResp.Msg = appResponse.Msg;
                appResp.IsAppValid = appResponse.IsAppValid;
                appResp.IsVersionValid = appResponse.IsVersionValid;
                appResp.IsPasswordExpired = appResponse.IsPasswordExpired;
                appResp.Statuscode = appResponse.Statuscode;
                if (appResp.Statuscode == ErrorCodes.One)
                {
                    var commonReq = new CommonReq
                    {
                        LoginTypeID = appUserRequest.LoginTypeID,
                        LoginID = appUserRequest.UserID,
                        CommonInt = appUserRequest.UID == 0 ? appUserRequest.UserID : appUserRequest.UID
                    };
                    IUserAPPML userML = new UserML(_accessor, _env, false);
                    await Task.Delay(0);
                    appResp.UserInfo = userML.GetEditUserForApp(commonReq);
                }
            }
            return Json(appResp);
        }
        [HttpPost]
        public async Task<IActionResult> UpdateProfile([FromBody] AppUserCreate appUserEdit)
        {
            var appResp = new AppResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (appUserEdit.LoginTypeID == LoginType.ApplicationUser)
            {
                var appRequest = new AppSessionReq
                {
                    APPID = appUserEdit.APPID ?? "",
                    IMEI = appUserEdit.IMEI ?? "",
                    LoginTypeID = appUserEdit.LoginTypeID,
                    UserID = appUserEdit.UserID,
                    SessionID = appUserEdit.SessionID,
                    RegKey = appUserEdit.RegKey ?? "",
                    SerialNo = appUserEdit.SerialNo ?? "",
                    Version = appUserEdit.Version ?? "",
                    Session = appUserEdit.Session
                };
                appResp = appML.CheckAppSession(appRequest);

                if (appResp.Statuscode == ErrorCodes.One)
                {
                    if (!appResp.IsPasswordExpired)
                    {
                        appUserEdit.editUser.LT = appUserEdit.LoginTypeID;
                        appUserEdit.editUser.LoginID = appUserEdit.UserID;

                        IUserAPPML userML = new UserML(_accessor, _env, false);
                        await Task.Delay(0).ConfigureAwait(false);
                        var res = userML.UpdateUserFromApp(appUserEdit.editUser);
                        appResp.Statuscode = res.Statuscode;
                        appResp.Msg = res.Msg;
                    }
                }
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "UpdateProfile",
                CommonStr2 = JsonConvert.SerializeObject(appUserEdit),
                CommonStr3 = JsonConvert.SerializeObject(appResp)
            });
            return Json(appResp);
        }
        [HttpPost]
        public async Task<IActionResult> UploadProfile(string UserRequest, IFormFile file)
        {
            var appResp = new AppResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if ((UserRequest ?? "") == "")
                return Json(appResp);
            AppUserKYCUpload appUserRequest;
            try
            {
                appUserRequest = JsonConvert.DeserializeObject<AppUserKYCUpload>(UserRequest);
            }
            catch (Exception ex)
            {
                appResp.Msg = ErrorCodes.InvalidParam;
                return Json(appResp);
            }
            if (appUserRequest.LoginTypeID == LoginType.ApplicationUser)
            {
                var appRequest = new AppSessionReq
                {
                    APPID = appUserRequest.APPID ?? "",
                    IMEI = appUserRequest.IMEI ?? "",
                    LoginTypeID = appUserRequest.LoginTypeID,
                    UserID = appUserRequest.UserID,
                    SessionID = appUserRequest.SessionID,
                    RegKey = appUserRequest.RegKey ?? "",
                    SerialNo = appUserRequest.SerialNo ?? "",
                    Version = appUserRequest.Version ?? "",
                    Session = appUserRequest.Session
                };
                appResp = appML.CheckAppSession(appRequest);
                if (appResp.Statuscode == ErrorCodes.One)
                {
                    if (!appResp.IsPasswordExpired)
                    {
                        await Task.Delay(0);
                        IResourceML ml = new ResourceML(_accessor, _env);
                        var res = ml.UploadProfile(file, appUserRequest.UserID, appUserRequest.LoginTypeID);
                        appResp.Statuscode = res.Statuscode;
                        appResp.Msg = res.Msg;
                    }
                }
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "UploadProfile",
                CommonStr2 = JsonConvert.SerializeObject(appUserRequest),
                CommonStr3 = JsonConvert.SerializeObject(appResp)
            });
            return Json(appResp);
        }
        #region Update Bank
        [HttpPost]
        public async Task<IActionResult> AppUpdateBank([FromBody] AppUserCreate appUserEdit)
        {
            var appResp = new AppResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (appUserEdit.LoginTypeID == LoginType.ApplicationUser)
            {
                var appRequest = new AppSessionReq
                {
                    APPID = appUserEdit.APPID ?? "",
                    IMEI = appUserEdit.IMEI ?? "",
                    LoginTypeID = appUserEdit.LoginTypeID,
                    UserID = appUserEdit.UserID,
                    SessionID = appUserEdit.SessionID,
                    RegKey = appUserEdit.RegKey ?? "",
                    SerialNo = appUserEdit.SerialNo ?? "",
                    Version = appUserEdit.Version ?? "",
                    Session = appUserEdit.Session
                };
                appResp = appML.CheckAppSession(appRequest);

                if (appResp.Statuscode == ErrorCodes.One)
                {
                    if (!appResp.IsPasswordExpired)
                    {
                        appUserEdit.editUser.LT = appUserEdit.LoginTypeID;
                        appUserEdit.editUser.LoginID = appUserEdit.UserID;

                        IUserAPPML userML = new UserML(_accessor, _env, false);
                        await Task.Delay(0);
                        var res = userML.UserBankRequestApp(appUserEdit.editUser);
                        appResp.Statuscode = res.Statuscode;
                        appResp.Msg = res.Msg;
                    }
                }
            }
            return Json(appResp);
        }
        #endregion
        #endregion
        #region UserKYCSection
        [HttpPost]
        public async Task<IActionResult> AppDocumentDetails([FromBody] AppUserRequest appUserRequest)
        {
            var appResp = new AppUserDocuments
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (appUserRequest.LoginTypeID == LoginType.ApplicationUser)
            {
                var appRequest = new AppSessionReq
                {
                    APPID = appUserRequest.APPID ?? "",
                    IMEI = appUserRequest.IMEI ?? "",
                    LoginTypeID = appUserRequest.LoginTypeID,
                    UserID = appUserRequest.UserID,
                    SessionID = appUserRequest.SessionID,
                    RegKey = appUserRequest.RegKey ?? "",
                    SerialNo = appUserRequest.SerialNo ?? "",
                    Version = appUserRequest.Version ?? "",
                    Session = appUserRequest.Session
                };
                var appResponse = appML.CheckAppSession(appRequest);
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
                        var docTypeMaster = new DocTypeMaster
                        {
                            LoginTypeID = appUserRequest.LoginTypeID,
                            LoginId = appUserRequest.UserID,
                            UserId = appUserRequest.UID,
                            OutletID = appResponse.GetID
                        };
                        IUserAPPML userML = new UserML(_accessor, _env, false);
                        await Task.Delay(0);
                        appResp.UserDox = userML.GetDocumentsForApp(docTypeMaster);
                    }
                }
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "AppDocumentDetails",
                CommonStr2 = JsonConvert.SerializeObject(appUserRequest),
                CommonStr3 = JsonConvert.SerializeObject(appResp)
            });
            return Json(appResp);
        }
        [HttpPost]
        public async Task<IActionResult> UploadDocs(string UserRequest, IFormFile file)
        {
            var appResp = new AppResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if ((UserRequest ?? "") == "")
                return Json(appResp);
            AppUserKYCUpload appUserRequest;
            try
            {
                appUserRequest = JsonConvert.DeserializeObject<AppUserKYCUpload>(UserRequest);
            }
            catch (Exception ex)
            {
                appResp.Msg = ErrorCodes.InvalidParam;
                return Json(appResp);
            }
            if (appUserRequest.LoginTypeID == LoginType.ApplicationUser)
            {
                var appRequest = new AppSessionReq
                {
                    APPID = appUserRequest.APPID ?? "",
                    IMEI = appUserRequest.IMEI ?? "",
                    LoginTypeID = appUserRequest.LoginTypeID,
                    UserID = appUserRequest.UserID,
                    SessionID = appUserRequest.SessionID,
                    RegKey = appUserRequest.RegKey ?? "",
                    SerialNo = appUserRequest.SerialNo ?? "",
                    Version = appUserRequest.Version ?? "",
                    Session = appUserRequest.Session
                };
                appResp = appML.CheckAppSession(appRequest);
                if (appResp.Statuscode == ErrorCodes.One)
                {
                    if (!appResp.IsPasswordExpired)
                    {
                        await Task.Delay(0);
                        IUserAPPML userAPPML = new UserML(_accessor, _env, false);
                        var res = userAPPML.UploadDocumentsForApp(file, appUserRequest.DocTypeID, appUserRequest.UID, appUserRequest.LoginTypeID, appUserRequest.UserID);
                        appResp.Statuscode = res.Statuscode;
                        appResp.Msg = res.Msg;
                    }
                }
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "UploadDocs",
                CommonStr2 = JsonConvert.SerializeObject(appUserRequest),
                CommonStr3 = JsonConvert.SerializeObject(appResp)
            });
            return Json(appResp);
        }
        [HttpPost]
        public async Task<IActionResult> UpdateKYCStatus([FromBody] AppKYCUpdateReq appKYCUpdateReq)
        {
            var appResp = new AppResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (appKYCUpdateReq.LoginTypeID == LoginType.ApplicationUser)
            {
                var appRequest = new AppSessionReq
                {
                    APPID = appKYCUpdateReq.APPID ?? "",
                    IMEI = appKYCUpdateReq.IMEI ?? "",
                    LoginTypeID = appKYCUpdateReq.LoginTypeID,
                    UserID = appKYCUpdateReq.UserID,
                    SessionID = appKYCUpdateReq.SessionID,
                    RegKey = appKYCUpdateReq.RegKey ?? "",
                    SerialNo = appKYCUpdateReq.SerialNo ?? "",
                    Version = appKYCUpdateReq.Version ?? "",
                    Session = appKYCUpdateReq.Session
                };
                appResp = appML.CheckAppSession(appRequest);
                if (appResp.Statuscode == ErrorCodes.One)
                {
                    if (!appResp.IsPasswordExpired)
                    {
                        var req = new KYCStatusReq
                        {
                            LT = appKYCUpdateReq.LoginTypeID,
                            LoginID = appKYCUpdateReq.UserID,
                            OutletID = appKYCUpdateReq.OutletID,
                            KYCStatus = KYCStatusType.APPLIED,
                            RequestModeID = RequestMode.APPS
                        };
                        IUserAPPML userML = new UserML(_accessor, _env, false);
                        await Task.Delay(0);
                        var res = userML.ChangeKYCStatusForApp(req);
                        appResp.Statuscode = res.Statuscode;
                        appResp.Msg = res.Msg;
                    }
                }
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "UpdateKYCStatus",
                CommonStr2 = JsonConvert.SerializeObject(appKYCUpdateReq),
                CommonStr3 = JsonConvert.SerializeObject(appResp)
            });
            return Json(appResp);
        }
        #endregion
        #region AppFundRequestSection
        [HttpPost]
        public async Task<IActionResult> FundRequestTo([FromBody] AppSessionReq appSessionReq)
        {
            var appFundRequestToResponse = new AppFundRequestToResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            var appResp = appML.CheckAppSession(appSessionReq);
            appFundRequestToResponse.IsAppValid = appResp.IsAppValid;
            appFundRequestToResponse.IsVersionValid = appResp.IsVersionValid;
            appFundRequestToResponse.IsPasswordExpired = appResp.IsPasswordExpired;
            appFundRequestToResponse.Statuscode = appResp.Statuscode;
            if (appResp.Statuscode == ErrorCodes.One)
            {
                if (!appResp.IsPasswordExpired)
                {
                    IFundProcessML userML = new UserML(_accessor, _env, false);
                    var _filter = new CommonReq
                    {
                        LoginTypeID = appSessionReq.LoginTypeID,
                        LoginID = appSessionReq.UserID
                    };
                    appFundRequestToResponse.Statuscode = ErrorCodes.One;
                    appFundRequestToResponse.Msg = ErrorCodes.SUCCESS;
                    await Task.Delay(0);

                    appFundRequestToResponse.FundRequestToUsers = userML.FundRequestToUserApp(_filter);
                }
            }
            else
            {
                appFundRequestToResponse.Msg = appResp.Msg;
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "FundRequestTo",
                CommonStr2 = JsonConvert.SerializeObject(appSessionReq),
                CommonStr3 = JsonConvert.SerializeObject(appFundRequestToResponse)
            });
            return Json(appFundRequestToResponse);
        }
        [HttpPost]
        public async Task<IActionResult> GetBankAndPaymentMode([FromBody] AppGetBankRequest appGetBankRequest)
        {
            var appBankResponse = new AppBankResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            var appRequest = new AppSessionReq
            {
                APPID = appGetBankRequest.APPID ?? "",
                IMEI = appGetBankRequest.IMEI ?? "",
                LoginTypeID = appGetBankRequest.LoginTypeID,
                UserID = appGetBankRequest.UserID,
                SessionID = appGetBankRequest.SessionID,
                RegKey = appGetBankRequest.RegKey ?? "",
                SerialNo = appGetBankRequest.SerialNo ?? "",
                Version = appGetBankRequest.Version ?? "",
                Session = appGetBankRequest.Session
            };
            var appResp = appML.CheckAppSession(appRequest);
            appBankResponse.IsAppValid = appResp.IsAppValid;
            appBankResponse.IsVersionValid = appResp.IsVersionValid;
            appBankResponse.IsPasswordExpired = appResp.IsPasswordExpired;
            appBankResponse.Statuscode = appResp.Statuscode;
            if (appResp.Statuscode == ErrorCodes.One)
            {
                if (!appResp.IsPasswordExpired)
                {
                    var _filter = new CommonReq
                    {

                        LoginTypeID = appGetBankRequest.LoginTypeID,
                        LoginID = appGetBankRequest.UserID,
                        CommonInt = appGetBankRequest.ParentID
                    };
                    appBankResponse.Statuscode = ErrorCodes.One;
                    appBankResponse.Msg = ErrorCodes.SUCCESS;
                    await Task.Delay(0);
                    IBankML bankML = new BankML(_accessor, _env, false);
                    appBankResponse.Banks = bankML.BanksAndPaymentModes(_filter);
                }
            }
            else
            {
                appBankResponse.Msg = appResp.Msg;
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "GetBankAndPaymentMode",
                CommonStr2 = JsonConvert.SerializeObject(appGetBankRequest),
                CommonStr3 = JsonConvert.SerializeObject(appBankResponse)
            });
            return Json(appBankResponse);
        }
        [HttpPost]
        public async Task<IActionResult> GetBank([FromBody] AppSessionReq appRequest)
        {
            var appBankResponse = new AppBankResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            var appResp = appML.CheckAppSession(appRequest);
            appBankResponse.IsAppValid = appResp.IsAppValid;
            appBankResponse.IsVersionValid = appResp.IsVersionValid;
            appBankResponse.IsPasswordExpired = appResp.IsPasswordExpired;
            appBankResponse.Statuscode = appResp.Statuscode;
            if (appResp.Statuscode == ErrorCodes.One)
            {
                if (!appResp.IsPasswordExpired)
                {
                    var _filter = new CommonReq
                    {
                        LoginTypeID = appRequest.LoginTypeID,
                        LoginID = appRequest.UserID,
                    };
                    appBankResponse.Statuscode = ErrorCodes.One;
                    appBankResponse.Msg = ErrorCodes.SUCCESS;
                    await Task.Delay(0);
                    IBankML bankML = new BankML(_accessor, _env, false);
                    appBankResponse.Banks = bankML.WhiteLabelBanksForApp(_filter);
                }
            }
            else
            {
                appBankResponse.Msg = appResp.Msg;
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "GetBank",
                CommonStr2 = JsonConvert.SerializeObject(appRequest),
                CommonStr3 = JsonConvert.SerializeObject(appBankResponse)
            });
            return Json(appBankResponse);
        }
        //[HttpPost]
        //public async Task<IActionResult> AppFundOrder([FromBody]AppFundRequest appFundRequest)
        //{
        //    var appResp = new AppResponse
        //    {
        //        Statuscode = ErrorCodes.Minus1,
        //        Msg = ErrorCodes.AuthError
        //    };
        //    if (appFundRequest.LoginTypeID == LoginType.ApplicationUser)
        //    {
        //        var appRequest = new AppSessionReq
        //        {
        //            APPID = appFundRequest.APPID ?? "",
        //            IMEI = appFundRequest.IMEI ?? "",
        //            LoginTypeID = appFundRequest.LoginTypeID,
        //            UserID = appFundRequest.UserID,
        //            SessionID = appFundRequest.SessionID,
        //            RegKey = appFundRequest.RegKey ?? "",
        //            SerialNo = appFundRequest.SerialNo ?? "",
        //            Version = appFundRequest.Version ?? "",
        //            Session = appFundRequest.Session
        //        };
        //        appResp = appML.CheckAppSession(appRequest);
        //        if (appResp.Statuscode == ErrorCodes.One)
        //        {
        //            if (!appResp.IsPasswordExpired)
        //            {
        //                var _filter = new FundRequest
        //                {
        //                    BankId = appFundRequest.BankID,
        //                    Amount = appFundRequest.Amount,
        //                    TransactionId = appFundRequest.TransactionID ?? "",
        //                    MobileNo = appFundRequest.MobileNo ?? "",
        //                    ChequeNo = appFundRequest.ChequeNo ?? "",
        //                    CardNo = appFundRequest.CardNo ?? "",
        //                    LoginID = appFundRequest.UserID,
        //                    AccountHolderName = appFundRequest.AccountHolderName ?? "",
        //                    PaymentId = appFundRequest.PaymentID,
        //                    WalletTypeID = appFundRequest.WalletTypeID,
        //                    Branch = appFundRequest.Branch,
        //                    UPIID = appFundRequest.UPIID
        //                };
        //                IUserAPPML userML = new UserML(_accessor, _env, false);
        //                await Task.Delay(0);
        //                var iresp = userML.FundRequestOperationApp(_filter);
        //                appResp.Statuscode = iresp.Statuscode;
        //                appResp.Msg = iresp.Msg;
        //            }
        //        }
        //    }
        //    new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
        //    {
        //        CommonStr = "AppFundOrder",
        //        CommonStr2 = JsonConvert.SerializeObject(appFundRequest),
        //        CommonStr3 = JsonConvert.SerializeObject(appResp)
        //    });
        //    return Json(appResp);
        //}
        [HttpPost]
        public async Task<IActionResult> AppGenerateOrderForUPI([FromBody] AppFundRequest appFundRequest)
        {
            var appResponseForUPIOrder = new AppResponseForUPIOrder
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (appFundRequest.LoginTypeID == LoginType.ApplicationUser)
            {
                var appResp = appML.CheckAppSession(appFundRequest);
                appResponseForUPIOrder.IsAppValid = appResp.IsAppValid;
                appResponseForUPIOrder.IsVersionValid = appResp.IsVersionValid;
                appResponseForUPIOrder.IsPasswordExpired = appResp.IsPasswordExpired;
                appResponseForUPIOrder.Statuscode = appResp.Statuscode;

                if (appResp.Statuscode == ErrorCodes.One)
                {
                    if (!appResp.IsPasswordExpired)
                    {
                        if (ApplicationSetting.IsGenerateOrderForUPI)
                        {
                            var generateOrderUPIRequest = new GenerateOrderUPIRequest
                            {
                                UserID = appFundRequest.UserID,
                                SessionID = appFundRequest.SessionID,
                                IMEI = appFundRequest.IMEI,
                                AppVersion = appFundRequest.Version,
                                Amount = Convert.ToInt32(appFundRequest.Amount),
                                UPIID = appFundRequest.UPIID
                            };
                            IUserAPPML userML = new UserML(_accessor, _env, false);
                            var iresp = await Task.FromResult(userML.GenerateOrderForUPI(generateOrderUPIRequest)).ConfigureAwait(false);
                            appResponseForUPIOrder.Statuscode = iresp.Statuscode;
                            appResponseForUPIOrder.Msg = iresp.Msg;
                            appResponseForUPIOrder.OrderID = iresp.CommonInt;
                        }
                        else
                        {
                            appResponseForUPIOrder.Statuscode = ErrorCodes.Minus1;
                            appResponseForUPIOrder.Msg = ErrorCodes.AuthError;
                        }
                    }
                }
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "AppGenerateOrderForUPI",
                CommonStr2 = JsonConvert.SerializeObject(appFundRequest),
                CommonStr3 = JsonConvert.SerializeObject(appResponseForUPIOrder)
            });
            return Json(appResponseForUPIOrder);
        }
        [HttpPost]
        public async Task<IActionResult> AppFundOrder(string UserFundRequest, IFormFile file)
        {
            var appResp = new AppResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if ((UserFundRequest ?? "") == "")
                return Json(appResp);
            AppFundRequest appFundRequest;
            try
            {
                appFundRequest = JsonConvert.DeserializeObject<AppFundRequest>(UserFundRequest);
            }
            catch (Exception ex)
            {
                appResp.Msg = ErrorCodes.InvalidParam;
                return Json(appResp);
            }
            if (appFundRequest.LoginTypeID == LoginType.ApplicationUser)
            {
                var appRequest = new AppSessionReq
                {
                    APPID = appFundRequest.APPID ?? "",
                    IMEI = appFundRequest.IMEI ?? "",
                    LoginTypeID = appFundRequest.LoginTypeID,
                    UserID = appFundRequest.UserID,
                    SessionID = appFundRequest.SessionID,
                    RegKey = appFundRequest.RegKey ?? "",
                    SerialNo = appFundRequest.SerialNo ?? "",
                    Version = appFundRequest.Version ?? "",
                    Session = appFundRequest.Session
                };
                appResp = appML.CheckAppSession(appRequest);
                if (appResp.Statuscode == ErrorCodes.One)
                {
                    if (!appResp.IsPasswordExpired)
                    {
                        var _filter = new FundRequest
                        {
                            BankId = appFundRequest.BankID,
                            Amount = appFundRequest.Amount,
                            TransactionId = appFundRequest.TransactionID ?? string.Empty,
                            MobileNo = appFundRequest.MobileNo ?? string.Empty,
                            ChequeNo = appFundRequest.ChequeNo ?? string.Empty,
                            CardNo = appFundRequest.CardNo ?? string.Empty,
                            LoginID = appFundRequest.UserID,
                            AccountHolderName = appFundRequest.AccountHolderName ?? string.Empty,
                            PaymentId = appFundRequest.PaymentID,
                            WalletTypeID = appFundRequest.WalletTypeID,
                            Branch = appFundRequest.Branch,
                            UPIID = appFundRequest.UPIID,
                            OrderID = appFundRequest.OrderID,
                            Checksum = appFundRequest.Checksum,
                            AppVersion = appFundRequest.Version,
                            IMEI = appFundRequest.IMEI,
                            SessionID = appFundRequest.SessionID
                        };
                        IUserAPPML userML = new UserML(_accessor, _env, false);
                        await Task.Delay(0).ConfigureAwait(false);
                        if (appFundRequest.IsImage == true)
                        {
                            IBannerML bannerML = new ResourceML(_accessor, _env);
                            var _lr = new LoginResponse
                            {
                                LoginTypeID = LoginType.ApplicationUser,
                            };
                            var _res = bannerML.UploadReceipt(file, _filter.BankId, _lr);
                            if (_res.Statuscode == ErrorCodes.One)
                            {
                                _filter.RImage = _res.CommonStr;
                                var iresp = userML.FundRequestOperationApp(_filter);
                                appResp.Statuscode = iresp.Statuscode;
                                appResp.Msg = iresp.Msg;
                            }
                            else
                            {
                                appResp.Statuscode = _res.Statuscode;
                                appResp.Msg = _res.Msg;
                            }
                        }
                        else
                        {
                            var iresp = userML.FundRequestOperationApp(_filter);
                            appResp.Statuscode = iresp.Statuscode;
                            appResp.Msg = iresp.Msg;
                        }
                    }
                }
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "AppFundOrder",
                CommonStr2 = JsonConvert.SerializeObject(appFundRequest),
                CommonStr3 = JsonConvert.SerializeObject(appResp)
            });
            return Json(appResp);
        }
        [HttpPost]
        public async Task<IActionResult> AppFundTransfer([FromBody] AppFundProcessReq appFundProcessReq)
        {
            var appResp = new AppResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (appFundProcessReq.LoginTypeID == LoginType.ApplicationUser)
            {
                var appRequest = new AppSessionReq
                {
                    APPID = appFundProcessReq.APPID ?? "",
                    IMEI = appFundProcessReq.IMEI ?? "",
                    LoginTypeID = appFundProcessReq.LoginTypeID,
                    UserID = appFundProcessReq.UserID,
                    SessionID = appFundProcessReq.SessionID,
                    RegKey = appFundProcessReq.RegKey ?? "",
                    SerialNo = appFundProcessReq.SerialNo ?? "",
                    Version = appFundProcessReq.Version ?? "",
                    Session = appFundProcessReq.Session
                };
                appResp = appML.CheckAppSession(appRequest);
                if (appResp.Statuscode == ErrorCodes.One)
                {
                    if (!appResp.IsPasswordExpired)
                    {
                        var fundProcessReq = new FundProcessReq
                        {
                            LoginID = appFundProcessReq.UserID,
                            LoginTypeID = appFundProcessReq.LoginTypeID,
                            fundProcess = new FundProcess
                            {
                                UserID = appFundProcessReq.UID,
                                Amount = appFundProcessReq.Amount,
                                OType = appFundProcessReq.OType,
                                Remark = appFundProcessReq.Remark ?? string.Empty,
                                WalletType = appFundProcessReq.WalletType,
                                PaymentId = appFundProcessReq.PaymentID,
                                RequestMode = RequestMode.APPS,
                                SecurityKey = appFundProcessReq.SecurityKey,
                                IsMarkCredit = appFundProcessReq.IsMarkCredit
                            }
                        };

                        IUserAPPML userML = new UserML(_accessor, _env, false);
                        await Task.Delay(0);
                        var iresp = userML.FundTransferApp(fundProcessReq);
                        appResp.Statuscode = iresp.Statuscode;
                        appResp.Msg = iresp.Msg;
                    }
                }
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "AppFundTransfer",
                CommonStr2 = JsonConvert.SerializeObject(appFundProcessReq),
                CommonStr3 = JsonConvert.SerializeObject(appResp)
            });
            return Json(appResp);
        }
        [HttpPost]
        public async Task<IActionResult> AppFundReject([FromBody] AppFundProcessReq appFundProcessReq)
        {
            var appResp = new AppResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (appFundProcessReq.LoginTypeID == LoginType.ApplicationUser)
            {
                var appRequest = new AppSessionReq
                {
                    APPID = appFundProcessReq.APPID ?? "",
                    IMEI = appFundProcessReq.IMEI ?? "",
                    LoginTypeID = appFundProcessReq.LoginTypeID,
                    UserID = appFundProcessReq.UserID,
                    SessionID = appFundProcessReq.SessionID,
                    RegKey = appFundProcessReq.RegKey ?? "",
                    SerialNo = appFundProcessReq.SerialNo ?? "",
                    Version = appFundProcessReq.Version ?? "",
                    Session = appFundProcessReq.Session
                };
                appResp = appML.CheckAppSession(appRequest);
                if (appResp.Statuscode == ErrorCodes.One)
                {
                    if (!appResp.IsPasswordExpired)
                    {
                        var fundProcessReq = new FundProcessReq
                        {
                            LoginID = appFundProcessReq.UserID,
                            LoginTypeID = appFundProcessReq.LoginTypeID,
                            fundProcess = new FundProcess
                            {
                                Remark = appFundProcessReq.Remark ?? "",
                                PaymentId = appFundProcessReq.PaymentID,
                                RequestMode = RequestMode.APPS
                            }
                        };

                        IUserAPPML userML = new UserML(_accessor, _env, false);
                        await Task.Delay(0);
                        var iresp = userML.FundRejectFromApp(fundProcessReq);
                        appResp.Statuscode = iresp.Statuscode;
                        appResp.Msg = iresp.Msg;
                    }
                }
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "AppFundReject",
                CommonStr2 = JsonConvert.SerializeObject(appFundProcessReq),
                CommonStr3 = JsonConvert.SerializeObject(appResp)
            });
            return Json(appResp);
        }
        #endregion
        #region PlanAPI
        [HttpPost]
        public async Task<IActionResult> ROffer([FromBody] AppSimplePlanReq appSimplePlanReq)
        {
            var appROfferResp = new AppROfferResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvalidParam
            };
            var appResp = appML.CheckApp(appSimplePlanReq);
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
                else if (ApplicationSetting.PlanType == PlanType.Roundpay)
                {
                    IRoundpayPlan RPPlan = new PlansAPIML(_accessor, _env);
                    appROfferResp.Data = plansAPIML.GetRofferRoundpay(appSimplePlanReq.AccountNo, appSimplePlanReq.OID);
                }
                else if (ApplicationSetting.PlanType == PlanType.PLANAPI)
                {
                    IPlanAPIPlan RPPlan = new PlansAPIML(_accessor, _env);
                    appROfferResp.DataPA = plansAPIML.GetRofferPLANAPI(appSimplePlanReq.AccountNo, appSimplePlanReq.OID);
                }
                else if (ApplicationSetting.PlanType == PlanType.CYRUS)
                {
                    ICyrusAPIPlan cyrusRofferPlan = new PlansAPIML(_accessor, _env);
                    appROfferResp.DataCyrRof = cyrusRofferPlan.GetRofferCYRUS(appSimplePlanReq.AccountNo, appSimplePlanReq.OID);
                }
                else if (ApplicationSetting.PlanType == PlanType.MYPLAN)
                {
                    IMyPlanAPI imyplan = new PlansAPIML(_accessor, _env);

                    appROfferResp.MyPlanData = imyplan.GetRofferMyPlanApi(appSimplePlanReq.AccountNo, appSimplePlanReq.OID);
                }
                appROfferResp.Statuscode = ErrorCodes.One;
                appROfferResp.Msg = ErrorCodes.SUCCESS;
            }
            else
            {
                appROfferResp.Msg = appResp.Msg;
            }
            plansAPIML.LogAppReqResp(new CommonReq
            {
                CommonStr = "ROffer",
                CommonStr2 = JsonConvert.SerializeObject(appSimplePlanReq),
                CommonStr3 = JsonConvert.SerializeObject(appROfferResp)
            });
            return Json(appROfferResp);
        }
        [HttpPost]
        public async Task<IActionResult> SimplePlan([FromBody] AppSimplePlanReq appSimplePlanReq)
        {
            var appSimplePlanResp = new AppSimplePlanResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvalidParam
            };

            var appResp = appML.CheckApp(appSimplePlanReq);
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
                else if (ApplicationSetting.PlanType == PlanType.Roundpay)
                {
                    IRoundpayPlan RPPlan = new PlansAPIML(_accessor, _env);
                    appSimplePlanResp.DataRP = plansAPIML.GetSimplePlanRoundpay(appSimplePlanReq.CircleID, appSimplePlanReq.OID);
                }
                else if (ApplicationSetting.PlanType == PlanType.PLANAPI)
                {
                    IPlanAPIPlan RPPlan = new PlansAPIML(_accessor, _env);
                    appSimplePlanResp.DataPA = plansAPIML.GetSimplePlanAPI(appSimplePlanReq.CircleID, appSimplePlanReq.OID);
                }
                else if (ApplicationSetting.PlanType == PlanType.MYPLAN)
                {
                    IMyPlanAPI RPPlan = new PlansAPIML(_accessor, _env);
                    appSimplePlanResp.MyPlanData = plansAPIML.GetSimpleMyPlanAPI(appSimplePlanReq.CircleID, appSimplePlanReq.OID);
                }
                appSimplePlanResp.Statuscode = ErrorCodes.One;
                appSimplePlanResp.Msg = ErrorCodes.SUCCESS;
            }
            else
            {
                appSimplePlanResp.Msg = appResp.Msg;
            }
            plansAPIML.LogAppReqResp(new CommonReq
            {
                CommonStr = "SimplePlan",
                CommonStr2 = JsonConvert.SerializeObject(appSimplePlanReq),
                CommonStr3 = JsonConvert.SerializeObject(appSimplePlanResp)
            });
            return Json(appSimplePlanResp);
        }
        [HttpPost]
        public async Task<IActionResult> DTHCustomerInfo([FromBody] AppSimplePlanReq appSimplePlanReq)
        {
            var appCustInfo = new AppDTHCustInfo
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvalidParam
            };

            AppResponse appResp = appML.CheckApp(appSimplePlanReq);
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
                else if (ApplicationSetting.PlanType == PlanType.Roundpay)
                {
                    IRoundpayPlan RPPlan = new PlansAPIML(_accessor, _env);
                    appCustInfo.Data = plansAPIML.GetDTHCustInfoRoundpay(appSimplePlanReq.AccountNo, appSimplePlanReq.OID);
                }
                else if (ApplicationSetting.PlanType == PlanType.PLANAPI)
                {
                    IPlanAPIPlan RPPlan = new PlansAPIML(_accessor, _env);
                    appCustInfo.DataPA = plansAPIML.GetDTHCustInfoPlanAPI(appSimplePlanReq.AccountNo, appSimplePlanReq.OID);
                }
                else if (ApplicationSetting.PlanType == PlanType.MYPLAN)
                {
                    IMyPlanAPI RPPlan = new PlansAPIML(_accessor, _env);
                    appCustInfo.MyPlanData = plansAPIML.GetDTHCustInfoMyPlan(appSimplePlanReq.AccountNo, appSimplePlanReq.OID);
                }
                appCustInfo.Statuscode = ErrorCodes.One;
                appCustInfo.Msg = ErrorCodes.SUCCESS;
            }
            else
            {
                appCustInfo.Msg = appResp.Msg;
            }
            plansAPIML.LogAppReqResp(new CommonReq
            {
                CommonStr = "DTHCustomerInfo",
                CommonStr2 = JsonConvert.SerializeObject(appSimplePlanReq),
                CommonStr3 = JsonConvert.SerializeObject(appCustInfo)
            });
            return Json(appCustInfo);
        }
        [HttpPost]
        public async Task<IActionResult> DTHSimplePlanInfo([FromBody] AppSimplePlanReq appSimplePlanReq)
        {
            var appDTHSimplePlanResp = new AppDTHSimplePlanResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvalidParam
            };
            var appRequest = new AppRequest
            {
                APPID = appSimplePlanReq.APPID,
                IMEI = appSimplePlanReq.IMEI,
                Version = appSimplePlanReq.Version,
                RegKey = appSimplePlanReq.RegKey,
                SerialNo = appSimplePlanReq.SerialNo
            };
            var appResp = appML.CheckApp(appRequest);
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
                else if (ApplicationSetting.PlanType == PlanType.Roundpay)
                {

                    IRoundpayPlan RPPlan = new PlansAPIML(_accessor, _env);
                    if (ApplicationSetting.IsDTHPlanWithChannelList)
                        appDTHSimplePlanResp.DataRPDTHWithPackage = plansAPIML.RPDTHSimplePlansOfPackages(appSimplePlanReq.OID);
                    else
                        appDTHSimplePlanResp.DataRP = plansAPIML.GetDTHSimplePlanRoundpay(appSimplePlanReq.OID);

                }
                else if (ApplicationSetting.PlanType == PlanType.PLANAPI)
                {
                    IPlanAPIPlan RPPlan = new PlansAPIML(_accessor, _env);
                    appDTHSimplePlanResp.DataPA = plansAPIML.GetDTHSimplePlanAPI(appSimplePlanReq.OID);
                }
                else if (ApplicationSetting.PlanType == PlanType.MYPLAN)
                {
                    IMyPlanAPI RPPlan = new PlansAPIML(_accessor, _env);
                    appDTHSimplePlanResp.MyPlanData = plansAPIML.GetDthSimpleMyPlanApi(appSimplePlanReq.OID);
                }
                else if (ApplicationSetting.PlanType == PlanType.MYPLAN)
                {
                    IMyPlanAPI RPPlan = new PlansAPIML(_accessor, _env);
                    appDTHSimplePlanResp.Data = plansAPIML.AppGetDthSimpleMyPlan(appSimplePlanReq.OID);
                }
                appDTHSimplePlanResp.Statuscode = ErrorCodes.One;
                appDTHSimplePlanResp.Msg = ErrorCodes.SUCCESS;
            }
            else
            {
                appDTHSimplePlanResp.Msg = appResp.Msg;
            }
            plansAPIML.LogAppReqResp(new CommonReq
            {
                CommonStr = "DTHSimplePlanInfo",
                CommonStr2 = JsonConvert.SerializeObject(appSimplePlanReq),
                CommonStr3 = JsonConvert.SerializeObject(appDTHSimplePlanResp)
            });
            return Json(appDTHSimplePlanResp);
        }
        [HttpPost]
        public async Task<IActionResult> DTHChannelPlanInfo([FromBody] AppSimplePlanReq appSimplePlanReq)
        {
            var appDTHSimplePlanResp = new AppDTHSimplePlanResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvalidParam
            };
            var appRequest = new AppRequest
            {
                APPID = appSimplePlanReq.APPID,
                IMEI = appSimplePlanReq.IMEI,
                Version = appSimplePlanReq.Version,
                RegKey = appSimplePlanReq.RegKey,
                SerialNo = appSimplePlanReq.SerialNo
            };
            var appResp = appML.CheckApp(appRequest);
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
            plansAPIML.LogAppReqResp(new CommonReq
            {
                CommonStr = "DTHChannelPlanInfo",
                CommonStr2 = JsonConvert.SerializeObject(appSimplePlanReq),
                CommonStr3 = JsonConvert.SerializeObject(appDTHSimplePlanResp)
            });
            return Json(appDTHSimplePlanResp);
        }
        [HttpPost]
        public async Task<IActionResult> DTHHeavyRefresh([FromBody] AppSimplePlanReq appSimplePlanReq)
        {
            var appMplanDTHHeavyRefreshResp = new AppMplanDTHHeavyRefresh
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvalidParam
            };
            var appRequest = new AppRequest
            {
                APPID = appSimplePlanReq.APPID,
                IMEI = appSimplePlanReq.IMEI,
                Version = appSimplePlanReq.Version,
                RegKey = appSimplePlanReq.RegKey,
                SerialNo = appSimplePlanReq.SerialNo
            };
            var appResp = appML.CheckApp(appRequest);
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
                else if (ApplicationSetting.PlanType == PlanType.Roundpay)
                {
                    appMplanDTHHeavyRefreshResp.DataRP = plansAPIML.GetDTHRPHeavyRefresh(appSimplePlanReq.AccountNo, appSimplePlanReq.OID);
                }

                appMplanDTHHeavyRefreshResp.Statuscode = ErrorCodes.One;
                appMplanDTHHeavyRefreshResp.Msg = ErrorCodes.SUCCESS;
            }
            else
            {
                appMplanDTHHeavyRefreshResp.Msg = appResp.Msg;
            }
            plansAPIML.LogAppReqResp(new CommonReq
            {
                CommonStr = "DTHHeavyRefresh",
                CommonStr2 = JsonConvert.SerializeObject(appSimplePlanReq),
                CommonStr3 = JsonConvert.SerializeObject(appMplanDTHHeavyRefreshResp)
            });
            return Json(appMplanDTHHeavyRefreshResp);
        }
        #endregion
        #region RNPPlans
        [HttpPost]
        public async Task<IActionResult> RechSimplePlan([FromBody] AppSimplePlanReq appSimplePlanReq)
        {
            var appSimplePlanResp = new AppMNPSimpResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvalidParam
            };
            var appResp = appML.CheckApp(appSimplePlanReq);
            appSimplePlanResp.IsAppValid = appResp.IsAppValid;
            appSimplePlanResp.IsVersionValid = appResp.IsVersionValid;
            var plansAPIML = new PlansAPIML(_accessor, _env, false);
            if (appResp.Statuscode == ErrorCodes.One)
            {
                await Task.Delay(0);
                var plans = new PlansAPIML(_accessor, _env);
                appSimplePlanResp.Data = plans.AppSimplePlan(appSimplePlanReq.OID, appSimplePlanReq.CircleID);
                appSimplePlanResp.Statuscode = ErrorCodes.One;
                appSimplePlanResp.Msg = ErrorCodes.SUCCESS;
            }
            else
            {
                appSimplePlanResp.Msg = appResp.Msg;
            }
            plansAPIML.LogAppReqResp(new CommonReq
            {
                CommonStr = "RechSimplePlan",
                CommonStr2 = JsonConvert.SerializeObject(appSimplePlanReq),
                CommonStr3 = JsonConvert.SerializeObject(appSimplePlanResp)
            });
            return Json(appSimplePlanResp);
        }
        [HttpPost]
        public async Task<IActionResult> GetDTHSimplePlan([FromBody] AppSimplePlanReq appSimplePlanReq)
        {
            var appSimplePlanResp = new AppMNPSimpResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvalidParam
            };
            var appResp = appML.CheckApp(appSimplePlanReq);
            appSimplePlanResp.IsAppValid = appResp.IsAppValid;
            appSimplePlanResp.IsVersionValid = appResp.IsVersionValid;
            var plansAPIML = new PlansAPIML(_accessor, _env, false);
            if (appResp.Statuscode == ErrorCodes.One)
            {
                await Task.Delay(0);
                var plans = new PlansAPIML(_accessor, _env);
                appSimplePlanResp.Data = plans.AppDTHPlan(appSimplePlanReq.OID);
                appSimplePlanResp.Statuscode = ErrorCodes.One;
                appSimplePlanResp.Msg = ErrorCodes.SUCCESS;
            }
            else
            {
                appSimplePlanResp.Msg = appResp.Msg;
            }
            plansAPIML.LogAppReqResp(new CommonReq
            {
                CommonStr = "GetDTHSimplePlan",
                CommonStr2 = JsonConvert.SerializeObject(appSimplePlanReq),
                CommonStr3 = JsonConvert.SerializeObject(appSimplePlanResp)
            });
            return Json(appSimplePlanResp);
        }
        [HttpPost]
        public async Task<IActionResult> GetDTHPlanByLang([FromBody] AppSimplePlanReq appSimplePlanReq)
        {
            var appSimplePlanResp = new AppMNPSimpResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvalidParam
            };
            var appResp = appML.CheckApp(appSimplePlanReq);
            appSimplePlanResp.IsAppValid = appResp.IsAppValid;
            appSimplePlanResp.IsVersionValid = appResp.IsVersionValid;
            var plansAPIML = new PlansAPIML(_accessor, _env, false);
            if (appResp.Statuscode == ErrorCodes.One)
            {
                await Task.Delay(0);
                var plans = new PlansAPIML(_accessor, _env);
                appSimplePlanResp.Data = plans.AppDTHLang(appSimplePlanReq.OID, appSimplePlanReq.Language);
                appSimplePlanResp.Statuscode = ErrorCodes.One;
                appSimplePlanResp.Msg = ErrorCodes.SUCCESS;
            }
            else
            {
                appSimplePlanResp.Msg = appResp.Msg;
            }
            plansAPIML.LogAppReqResp(new CommonReq
            {
                CommonStr = "GetDTHSimplePlan",
                CommonStr2 = JsonConvert.SerializeObject(appSimplePlanReq),
                CommonStr3 = JsonConvert.SerializeObject(appSimplePlanResp)
            });
            return Json(appSimplePlanResp);
        }
        [HttpPost]
        public async Task<IActionResult> GetDTHChannelList([FromBody] AppSimplePlanReq appSimplePlanReq)
        {
            var appSimplePlanResp = new AppMNPSimpResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvalidParam
            };
            var appResp = appML.CheckApp(appSimplePlanReq);
            appSimplePlanResp.IsAppValid = appResp.IsAppValid;
            appSimplePlanResp.IsVersionValid = appResp.IsVersionValid;
            var plansAPIML = new PlansAPIML(_accessor, _env, false);
            if (appResp.Statuscode == ErrorCodes.One)
            {
                await Task.Delay(0);
                var plans = new PlansAPIML(_accessor, _env);
                appSimplePlanResp.Data = plans.AppDTHChannels(Convert.ToInt32(appSimplePlanReq.PackageID));
                appSimplePlanResp.Statuscode = ErrorCodes.One;
                appSimplePlanResp.Msg = ErrorCodes.SUCCESS;
            }
            else
            {
                appSimplePlanResp.Msg = appResp.Msg;
            }
            plansAPIML.LogAppReqResp(new CommonReq
            {
                CommonStr = "GetDTHChannelList",
                CommonStr2 = JsonConvert.SerializeObject(appSimplePlanReq),
                CommonStr3 = JsonConvert.SerializeObject(appSimplePlanResp)
            });
            return Json(appSimplePlanResp);
        }
        [HttpPost]
        public async Task<IActionResult> GetRNPRoffer([FromBody] AppSimplePlanReq appSimplePlanReq)
        {
            var appSimplePlanResp = new AppMNPSimpResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvalidParam
            };
            var appResp = appML.CheckApp(appSimplePlanReq);
            appSimplePlanResp.IsAppValid = appResp.IsAppValid;
            appSimplePlanResp.IsVersionValid = appResp.IsVersionValid;
            var plansAPIML = new PlansAPIML(_accessor, _env, false);
            if (appResp.Statuscode == ErrorCodes.One)
            {
                await Task.Delay(0);
                var plans = new PlansAPIML(_accessor, _env);
                var rofr = plans.GetRNPRoffer(appSimplePlanReq.AccountNo, appSimplePlanReq.OID);
                if (rofr.StatusCode == ErrorCodes.One)
                {
                    appSimplePlanResp.RofferData = rofr.RofferData;
                    appSimplePlanResp.Statuscode = ErrorCodes.One;
                    appSimplePlanResp.Msg = ErrorCodes.SUCCESS;
                }
            }
            else
            {
                appSimplePlanResp.Msg = appResp.Msg;
            }
            plansAPIML.LogAppReqResp(new CommonReq
            {
                CommonStr = "GetRNPRoffer",
                CommonStr2 = JsonConvert.SerializeObject(appSimplePlanReq),
                CommonStr3 = JsonConvert.SerializeObject(appSimplePlanResp)
            });
            return Json(appSimplePlanResp);
        }
        [HttpPost]
        public async Task<IActionResult> GetRNPDTHCustInfo([FromBody] AppSimplePlanReq appSimplePlanReq)
        {
            var appSimplePlanResp = new AppMNPSimpResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvalidParam
            };
            var appResp = appML.CheckApp(appSimplePlanReq);
            appSimplePlanResp.IsAppValid = appResp.IsAppValid;
            appSimplePlanResp.IsVersionValid = appResp.IsVersionValid;
            var plansAPIML = new PlansAPIML(_accessor, _env, false);
            if (appResp.Statuscode == ErrorCodes.One)
            {
                await Task.Delay(0);
                var plans = new PlansAPIML(_accessor, _env);
                var dthCIData = plans.GetRNPDTHCustInfo(appSimplePlanReq.OID, appSimplePlanReq.AccountNo);
                if (dthCIData.StatusCode == ErrorCodes.One)
                {
                    appSimplePlanResp.DTHCIData = dthCIData;
                    appSimplePlanResp.Statuscode = ErrorCodes.One;
                    appSimplePlanResp.Msg = ErrorCodes.SUCCESS;
                }
            }
            else
            {
                appSimplePlanResp.Msg = appResp.Msg;
            }
            plansAPIML.LogAppReqResp(new CommonReq
            {
                CommonStr = "GetRNPDTHCustInfo",
                CommonStr2 = JsonConvert.SerializeObject(appSimplePlanReq),
                CommonStr3 = JsonConvert.SerializeObject(appSimplePlanResp)
            });
            return Json(appSimplePlanResp);
        }
        [HttpPost]
        public async Task<IActionResult> GetRNPDTHHeavyRefresh([FromBody] AppSimplePlanReq appSimplePlanReq)
        {
            var appSimplePlanResp = new AppMNPSimpResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvalidParam
            };
            var appResp = appML.CheckApp(appSimplePlanReq);
            appSimplePlanResp.IsAppValid = appResp.IsAppValid;
            appSimplePlanResp.IsVersionValid = appResp.IsVersionValid;
            var plansAPIML = new PlansAPIML(_accessor, _env, false);
            if (appResp.Statuscode == ErrorCodes.One)
            {
                await Task.Delay(0);
                var plans = new PlansAPIML(_accessor, _env);
                var dthHRData = plans.GetRNPDTHHeavyRefresh(appSimplePlanReq.OID, appSimplePlanReq.AccountNo);
                if (dthHRData.StatusCode == ErrorCodes.One)
                {
                    appSimplePlanResp.DTHHRData = dthHRData;
                    appSimplePlanResp.Statuscode = ErrorCodes.One;
                    appSimplePlanResp.Msg = ErrorCodes.SUCCESS;
                }
            }
            else
            {
                appSimplePlanResp.Msg = appResp.Msg;
            }
            plansAPIML.LogAppReqResp(new CommonReq
            {
                CommonStr = "GetRNPDTHHeavyRefresh",
                CommonStr2 = JsonConvert.SerializeObject(appSimplePlanReq),
                CommonStr3 = JsonConvert.SerializeObject(appSimplePlanResp)
            });
            return Json(appSimplePlanResp);
        }
        #endregion
        #region BannerRegion
        [HttpPost]
        public IActionResult GetAppBanner([FromBody] AppSessionReq appSessionReq)
        {
            AppReportResponse appRechargeRespose = new AppReportResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            AppResponse appResp = appML.CheckAppSession(appSessionReq);
            appRechargeRespose.IsAppValid = appResp.IsAppValid;
            appRechargeRespose.IsVersionValid = appResp.IsVersionValid;
            appRechargeRespose.CheckID = appResp.CheckID;
            if (appResp.Statuscode == ErrorCodes.One)
            {
                IBannerML ml = new ResourceML(_accessor, _env);
                appRechargeRespose.Banners = ml.GetBanners(appRechargeRespose.CheckID.ToString());
                appRechargeRespose.AppLogoUrl = ml.GetSavedAppImage(appRechargeRespose.CheckID);
                appRechargeRespose.Statuscode = ErrorCodes.One;
                appRechargeRespose.Msg = ErrorCodes.SUCCESS;
            }
            else
                appRechargeRespose.Msg = appResp.Msg;
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "GetAppBanner",
                CommonStr2 = JsonConvert.SerializeObject(appSessionReq),
                CommonStr3 = JsonConvert.SerializeObject(appRechargeRespose)
            });
            return Json(appRechargeRespose);
        }
        #endregion
        #region Notifications
        [HttpPost]
        public IActionResult GetAppNotification([FromBody] AppSessionReq appSessionReq)
        {
            var appRechargeRespose = new AppReportResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            var appResp = appML.CheckAppSession(appSessionReq);
            appRechargeRespose.IsAppValid = appResp.IsAppValid;
            appRechargeRespose.IsVersionValid = appResp.IsVersionValid;
            appRechargeRespose.CheckID = appResp.CheckID;
            appRechargeRespose.IsPasswordExpired = appResp.IsPasswordExpired;
            appRechargeRespose.Statuscode = appResp.Statuscode;
            if (appResp.Statuscode == ErrorCodes.One)
            {
                IUserAPPML ml = new UserML(_accessor, _env, false);
                appRechargeRespose.Notifications = ml.GetNotificationsApp(appSessionReq.UserID);
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
        #endregion
        #region MoveToWallet
        [HttpPost]
        public IActionResult GetTransactionMode([FromBody] AppSessionReq appRequest)
        {
            var appResponse = new AppTransactionModeResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvalidLogin
            };
            AppResponse appResp = appML.CheckAppSession(appRequest);
            appResponse.IsAppValid = appResp.IsAppValid;
            appResponse.IsVersionValid = appResp.IsVersionValid;
            appResponse.IsPasswordExpired = appResp.IsPasswordExpired;
            appResponse.Statuscode = appResp.Statuscode;
            if (appResp.Statuscode == ErrorCodes.One)
            {
                var commonReq = new CommonReq
                {
                    LoginID = appRequest.UserID,
                    LoginTypeID = appRequest.LoginTypeID
                };
                appResponse.Statuscode = ErrorCodes.One;
                appResponse.Msg = ErrorCodes.SUCCESS;
                IUserAPPML userML = new UserML(_accessor, _env, false);
                var resp = userML.GetTransactionModes(commonReq);
                appResponse.TransactionModes = resp;
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "GetTransactionMode",
                CommonStr2 = JsonConvert.SerializeObject(appRequest),
                CommonStr3 = JsonConvert.SerializeObject(appResponse)
            });
            return Json(appResponse);
        }
        public async Task<IActionResult> MoveToWallet([FromBody] AppMoveToWalletReq appRequest)
        {
            var appResponse = new AppResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvalidLogin
            };
            appResponse = appML.CheckAppSession(appRequest);

            if (appResponse.Statuscode == ErrorCodes.One)
            {
                var commonReq = new CommonReq
                {
                    LoginID = appRequest.UserID,
                    LoginTypeID = appRequest.LoginTypeID,
                    CommonInt = appRequest.MTWID,
                    CommonDecimal = appRequest.Amount,
                    CommonInt2 = appRequest.OID
                };

                IUserAPPML userML = new UserML(_accessor, _env, false);
                var resp = await userML.MoveToWalletApp(commonReq).ConfigureAwait(false);
                appResponse.Statuscode = resp.Statuscode;
                appResponse.Msg = resp.Msg;
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "MoveToWallet",
                CommonStr2 = JsonConvert.SerializeObject(appRequest),
                CommonStr3 = JsonConvert.SerializeObject(appResponse)
            });
            return Json(appResponse);
        }
        #endregion
        #region Wrong2Right
        public async Task<IActionResult> MakeW2RRequest([FromBody] AppW2RReq appRequest)
        {
            var appResponse = new AppResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvalidLogin
            };
            appResponse = appML.CheckAppSession(appRequest);
            if (appResponse.Statuscode == ErrorCodes.One)
            {
                var _req = new WTRRequest
                {
                    UserID = appRequest.UserID,
                    LoginType = appRequest.LoginTypeID,
                    TID = appRequest.TID,
                    RPID = appRequest.RPID,
                    RightAccount = appRequest.RightAccount
                };
                IAppReportML ml = new ReportML(_accessor, _env, false);
                var resp = await ml.MarkWrong2RightApp(_req);
                appResponse.Statuscode = resp.Statuscode;
                appResponse.Msg = resp.Msg;
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "MakeW2RRequest",
                CommonStr2 = JsonConvert.SerializeObject(appRequest),
                CommonStr3 = JsonConvert.SerializeObject(appResponse)
            });
            return Json(appResponse);
        }
        #endregion
        #region ApplicationSetting
        [HttpPost]
        public IActionResult GetApplicationSetting([FromBody] AppSessionReq appSessionReq)
        {
            var appResponse = new AppSetting
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            var appResp = appML.CheckAppSession(appSessionReq);
            appResponse.IsAppValid = appResp.IsAppValid;
            appResponse.IsVersionValid = appResp.IsVersionValid;
            appResponse.CheckID = appResp.CheckID;
            appResponse.IsPasswordExpired = appResp.IsPasswordExpired;
            appResponse.Statuscode = appResp.Statuscode;
            if (appResp.Statuscode == ErrorCodes.One)
            {
                appResponse.Statuscode = ErrorCodes.One;
                appResponse.Msg = ErrorCodes.SUCCESS;
                appResponse.PlanType = ApplicationSetting.PlanType;
            }
            else
                appResponse.Msg = appResp.Msg;
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "GetApplicationSetting",
                CommonStr2 = JsonConvert.SerializeObject(appSessionReq),
                CommonStr3 = JsonConvert.SerializeObject(appResponse)
            });
            return Json(appResponse);
        }

        #endregion
        [HttpPost]
        public IActionResult Get(string P)
        {
            return Ok(HashEncryption.O.Decrypt(P));
        }
        [HttpGet]
        public IActionResult Test(string s)
        {
            s = @"{""response"":{""reqid"":""20476694"",""CustomerID"":""TN66AB6911"",""ResultID"":1354127,""due_amt"":100,""status_code"":""RCS"",""desc"":""Request Completed Successfully"",""BillDetail"":{""CustomerName"":""PRIYA"",""Amount"":100,""DueDate"":null,""BillDate"":null,""BillNumber"":""BBPS20210623205915AA9"",""BillPeriod"":null},""AdditionalInformation"":[{""name"":""Wallet Balance"",""value"":""125.00""},{""name"":""Maximum Recharge Amount"",""value"":""10000.0""}],""response_time"":""2021-06-23 20:59:15""},""billing"":""Billing Not Applicable""}";
            List<BillAdditionalInfo> billAdditionalInfo = new List<BillAdditionalInfo>();
            var ds = new System.Data.DataSet();
            ds = ToDataSet.O.ReadDataFromJson(s);
            if (ds.Tables.Count > 0)
            {
                string AddionalInfoListKey = "AdditionalInformation";
                string AddionalInfonameKey = "name";
                string AddionalInfoValueKey = "value";
                System.Data.DataTable dt = ds.Tables[AddionalInfoListKey];

                if (dt.Rows.Count > 0)
                {
                    if (dt.Columns.Contains(AddionalInfonameKey) && dt.Columns.Contains(AddionalInfoValueKey))
                    {
                        foreach (System.Data.DataRow item in dt.Rows)
                        {
                            billAdditionalInfo.Add(new BillAdditionalInfo
                            {
                                InfoName = item[AddionalInfonameKey].ToString(),
                                InfoValue = item[AddionalInfoValueKey].ToString()
                            });
                        }
                    }
                }
            }
            return Json(billAdditionalInfo);
        }
        public class SerailizeJson
        {
            public BeneDetail one { get; set; }
            public BeneDetail two { get; set; }
        }
        [HttpGet]
        public IActionResult GP()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("0000058501SM000002320000023200000217");
            sb.Append(Environment.NewLine);
            sb.Append("inist_in 00000040");
            sb.Append(Environment.NewLine);
            sb.Append("00000000");
            sb.Append(Environment.NewLine);
            sb.Append("BEGIN");
            sb.Append(Environment.NewLine);
            sb.Append("ERROR = 0");
            sb.Append(Environment.NewLine);
            sb.Append("RESULT = 0");
            sb.Append(Environment.NewLine);
            sb.Append("ADDINFO=%7B%22state%22%3A%222b569a4d-f8ac-5f2f-bb56-b2664b8ec54f%22%2C%22status%22%3A%22success%22%2C%22response_code%22%3A0%7D");
            sb.Append(Environment.NewLine);
            sb.Append("DATE=19.07.2019 12:29:41");
            sb.Append(Environment.NewLine);
            sb.Append("ERRMSG=Transaction Successful");
            sb.Append(Environment.NewLine);
            sb.Append("SESSION=ThuJul18201915909");
            sb.Append(Environment.NewLine);
            sb.Append(Environment.NewLine);
            sb.Append("END");
            sb.Append(Environment.NewLine);
            sb.Append("BEGIN SIGNATURE");
            sb.Append(Environment.NewLine);
            sb.Append("iQCRAwkBAAAAKF0xqaUBAWJvA/4+c+w/Gd/koQaXGbqtxZwPFmtROhNh6aY3XSM43CpcHyB6DqWiV/ua4cpnZd8Oqf/0gamxgTkT0EiMe8Kj4cm75apkemrO1EbPAfnKeoSlCEBWho0eeunmWt6tjhVnluVmfMAGQ3aXCAk/xlPuBok8rxpFH/daDE9uPy4jsszQ5rABxw===S4V7");
            sb.Append(Environment.NewLine);
            sb.Append("END SIGNATURE");


            CyberPlateML cyberPlateML = new CyberPlateML(null);

            return Json(cyberPlateML);
        }
        #region CallMe
        [HttpPost]
        public IActionResult GetCallMeUserReq([FromBody] CallMeReqApp appSessionReq)
        {
            var appResponse = new AppResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            appResponse = appML.CheckAppSession(appSessionReq);
            if (appResponse.Statuscode == ErrorCodes.One)
            {
                var seller = new SellerML(_accessor, _env);
                CommonReq req = new CommonReq
                {
                    LoginID = appSessionReq.UserID,
                    LoginTypeID = appSessionReq.LoginTypeID,
                    CommonStr = appSessionReq.mobileNo
                };
                appResponse = seller.AppCallMeUserRequest(req);
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "GetCallMeUserReq",
                CommonStr2 = JsonConvert.SerializeObject(appSessionReq),
                CommonStr3 = JsonConvert.SerializeObject(appResponse)
            });
            return Json(appResponse);
        }
        #endregion
        #region PaymentGateway
        [HttpPost]
        public IActionResult PaymentModeForAddmoney([FromBody] AppSessionReq appSessionReq)
        {
            var appResp = new AppPaymentModeForPGRes
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };

            var appResponse = appML.CheckAppSession(appSessionReq);
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
                    IOperatorML opML = new OperatorML(_accessor, _env);
                    appResp.PaymentModes = opML.GetPaymentModesOp(appSessionReq.UserID);
                }
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "PaymentModeForAddmoney",
                CommonStr2 = JsonConvert.SerializeObject(appSessionReq),
                CommonStr3 = JsonConvert.SerializeObject(appResp)
            });
            return Json(appResp);
        }
        [HttpPost]
        public IActionResult ChoosePaymentGateway([FromBody] AppPGChooseReq appSessionReq)
        {
            var appResp = new AppPaymentGatewayType
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };

            var appResponse = appML.CheckAppSession(appSessionReq);
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
                    IPaymentGatewayML gatewayML = new PaymentGatewayML(_accessor, _env);
                    appResp.PGs = gatewayML.GetPGDetailsUser(appResponse.CheckID, appSessionReq.IsUPI);
                }
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "ChoosePaymentGateway",
                CommonStr2 = JsonConvert.SerializeObject(appSessionReq),
                CommonStr3 = JsonConvert.SerializeObject(appResp)
            });
            return Json(appResp);
        }
        [HttpPost]
        public IActionResult PGatewayTransaction([FromBody] PGInitiatePGRequest pGInitiatePGRequest)
        {
            var appResp = new PGInitiatePGResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };

            var appResponse = appML.CheckAppSession(pGInitiatePGRequest);
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
                    IPaymentGatewayML gatewayML = new PaymentGatewayML(_accessor, _env);
                    appResp.pGModelForApp = gatewayML.IntiatePGTransactionForApp(pGInitiatePGRequest.UserID, pGInitiatePGRequest.Amount, pGInitiatePGRequest.UPGID, pGInitiatePGRequest.OID, pGInitiatePGRequest.WalletID, pGInitiatePGRequest.IMEI);
                }
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "PGatewayTransaction",
                CommonStr2 = JsonConvert.SerializeObject(pGInitiatePGRequest),
                CommonStr3 = JsonConvert.SerializeObject(appResp)
            });
            return Json(appResp);
        }
        [HttpPost]
        public async Task<IActionResult> PayTMTransactionUpdate([FromBody] PGUpdate pGUpdate)
        {
            var appResp = new AppResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            appResp = appML.CheckAppSession(pGUpdate);
            if (appResp.Statuscode == ErrorCodes.One)
            {
                if (!appResp.IsPasswordExpired)
                {
                    IPaymentGatewayML paymentGatewayML = new PaymentGatewayML(_accessor, _env);
                    var TID = 0;
                    var TransactionID = string.Empty;
                    if (Validate.O.IsNumeric(pGUpdate.PaytmCallbackResp.ORDERID ?? string.Empty))
                    {
                        TID = Convert.ToInt32(pGUpdate.PaytmCallbackResp.ORDERID);
                    }
                    if (!string.IsNullOrEmpty(pGUpdate.PaytmCallbackResp.body) || !string.IsNullOrEmpty(pGUpdate.PaytmCallbackResp.response))
                    {

                        var pGData = new PayTMCallbackTextResp();
                        if (Validate.O.ValidateJSON(pGUpdate.PaytmCallbackResp.body))
                        {
                            if (pGUpdate.PaytmCallbackResp.body.Contains("txnInfo"))
                            {
                                pGData = JsonConvert.DeserializeObject<PayTMCallbackTextResp>(pGUpdate.PaytmCallbackResp.body);
                            }
                            else
                            {
                                pGData.txnInfo = JsonConvert.DeserializeObject<PayTMCallbackTextInfo>(pGUpdate.PaytmCallbackResp.body);
                            }
                        }
                        if (Validate.O.ValidateJSON(pGUpdate.PaytmCallbackResp.response))
                        {
                            if (pGUpdate.PaytmCallbackResp.response.Contains("txnInfo"))
                            {
                                pGData = JsonConvert.DeserializeObject<PayTMCallbackTextResp>(pGUpdate.PaytmCallbackResp.response);
                            }
                            else
                            {
                                pGData.txnInfo = JsonConvert.DeserializeObject<PayTMCallbackTextInfo>(pGUpdate.PaytmCallbackResp.response);
                            }
                        }
                        if (pGData != null)
                        {
                            if (pGData.txnInfo != null)
                            {
                                pGUpdate.PaytmCallbackResp.CHECKSUMHASH = pGData.txnInfo.CHECKSUMHASH;
                                pGUpdate.PaytmCallbackResp.CURRENCY = pGData.txnInfo.CURRENCY;
                                pGUpdate.PaytmCallbackResp.RESPMSG = pGData.txnInfo.RESPMSG;
                                pGUpdate.PaytmCallbackResp.MID = pGData.txnInfo.MID;
                                pGUpdate.PaytmCallbackResp.RESPCODE = pGData.txnInfo.RESPCODE;
                                pGUpdate.PaytmCallbackResp.TXNID = pGData.txnInfo.TXNID;
                                pGUpdate.PaytmCallbackResp.TXNAMOUNT = pGData.txnInfo.TXNAMOUNT;
                                pGUpdate.PaytmCallbackResp.ORDERID = pGData.txnInfo.ORDERID;
                                pGUpdate.PaytmCallbackResp.STATUS = pGData.txnInfo.STATUS;
                                pGUpdate.PaytmCallbackResp.BANKTXNID = pGData.txnInfo.BANKTXNID;
                            }
                        }
                    }
                    await paymentGatewayML.SavePGTransactionLog(PaymentGatewayType.PAYTM, TID, (pGUpdate.PaytmCallbackResp != null ? JsonConvert.SerializeObject(pGUpdate.PaytmCallbackResp) : string.Empty), string.Empty, string.Empty, RequestMode.API, false, 0, string.Empty);
                    var res = paymentGatewayML.UpdateFromPayTMCallback(pGUpdate.PaytmCallbackResp);
                    appResp.Statuscode = res.Statuscode;
                    appResp.Msg = res.Msg;
                }
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "PayTMTransactionUpdate",
                CommonStr2 = JsonConvert.SerializeObject(pGUpdate),
                CommonStr3 = JsonConvert.SerializeObject(appResp)
            });
            return Json(appResp);
        }
        [HttpPost]
        public async Task<IActionResult> AggrePayTransactionUpdate([FromBody] PGUpdate pGUpdate)
        {
            var appResp = new AppResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };

            appResp = appML.CheckAppSession(pGUpdate);
            if (appResp.Statuscode == ErrorCodes.One)
            {
                if (!appResp.IsPasswordExpired)
                {
                    IPaymentGatewayML paymentGatewayML = new PaymentGatewayML(_accessor, _env);

                    await paymentGatewayML.SavePGTransactionLog(PaymentGatewayType.AGRPAY, pGUpdate.TID, JsonConvert.SerializeObject(pGUpdate), string.Empty, string.Empty, RequestMode.APPS, false, 0, string.Empty).ConfigureAwait(false);
                    var resUpdate = paymentGatewayML.UpdateFromAggrePayApp(pGUpdate.TID, pGUpdate.Amount, pGUpdate.Hash);
                    appResp.Statuscode = resUpdate.Statuscode;
                    appResp.Msg = resUpdate.Msg;
                }
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "AggrePayTransactionUpdate",
                CommonStr2 = JsonConvert.SerializeObject(pGUpdate),
                CommonStr3 = JsonConvert.SerializeObject(appResp)
            });
            return Json(appResp);
        }
        [HttpPost]
        public IActionResult IntiateUPI([FromBody] UPIInitiateRequest uPIInitiateRequest)
        {
            var appResp = new UPIIntiateResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };

            var appResponse = appML.CheckAppSession(uPIInitiateRequest);
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
                    var iniResp = gatewayML.InitiateUPIPaymentForApp(uPIInitiateRequest.UserID, uPIInitiateRequest.Amount, UPGID, uPIInitiateRequest.OID, uPIInitiateRequest.WalletID, uPIInitiateRequest.IMEI);

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
        [HttpPost]
        public async Task<IActionResult> UPIPaymentUpdate([FromBody] UPIUpdate uPIUpdate)
        {
            var appResp = new AppResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };

            appResp = appML.CheckAppSession(uPIUpdate);
            if (appResp.Statuscode == ErrorCodes.One)
            {
                if (!appResp.IsPasswordExpired)
                {
                    IPaymentGatewayML paymentGatewayML = new PaymentGatewayML(_accessor, _env);
                    var TID = 0;
                    var TransactionID = (uPIUpdate.TID ?? string.Empty).Replace("TID", "");
                    TID = Validate.O.IsNumeric(TransactionID) ? Convert.ToInt32(TransactionID) : 0;
                    var res = paymentGatewayML.CheckPGTransactionStatus(new CommonReq
                    {
                        LoginTypeID = LoginType.ApplicationUser,
                        LoginID = uPIUpdate.UserID,
                        CommonInt = TID
                    });
                    appResp.Statuscode = res.Statuscode;
                    appResp.Msg = res.Msg;
                }
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "UPIPaymentUpdate",
                CommonStr2 = JsonConvert.SerializeObject(uPIUpdate),
                CommonStr3 = JsonConvert.SerializeObject(appResp)
            });
            return Json(appResp);
        }
        [HttpPost]
        public async Task<IActionResult> CashFreeTransactionUpdate([FromBody] PGUpdate pGUpdate)
        {
            var appResp = new AppResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            appResp = appML.CheckAppSession(pGUpdate);
            if (appResp.Statuscode == ErrorCodes.One)
            {
                if (!appResp.IsPasswordExpired)
                {
                    IPaymentGatewayML paymentGatewayML = new PaymentGatewayML(_accessor, _env);
                    pGUpdate.PaytmCallbackResp.ORDERID = pGUpdate.PaytmCallbackResp.ORDERID.Replace("TID", string.Empty, StringComparison.OrdinalIgnoreCase);
                    if (Validate.O.IsNumeric(pGUpdate.PaytmCallbackResp.ORDERID ?? string.Empty))
                    {
                        pGUpdate.TID = Convert.ToInt32(pGUpdate.PaytmCallbackResp.ORDERID);
                    }
                    if (Validate.O.IsDecimal(pGUpdate.PaytmCallbackResp.TXNAMOUNT ?? string.Empty))
                    {
                        pGUpdate.Amount = Convert.ToInt32(Convert.ToDecimal(pGUpdate.PaytmCallbackResp.TXNAMOUNT));
                    }
                    await paymentGatewayML.SavePGTransactionLog(PaymentGatewayType.CASHFREE, pGUpdate.TID, (pGUpdate.PaytmCallbackResp != null ? JsonConvert.SerializeObject(pGUpdate.PaytmCallbackResp) : string.Empty), string.Empty, string.Empty, RequestMode.API, false, 0, string.Empty);
                    var res = paymentGatewayML.UpdateFromCashFreeApp(pGUpdate.TID, pGUpdate.Amount, pGUpdate.Hash);
                    appResp.Statuscode = res.Statuscode;
                    appResp.Msg = res.Msg;
                }
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "PayTMTransactionUpdate",
                CommonStr2 = JsonConvert.SerializeObject(pGUpdate),
                CommonStr3 = JsonConvert.SerializeObject(appResp)
            });
            return Json(appResp);
        }
        #endregion
        #region ThirdPartyAppRequest
        [HttpPost]
        public IActionResult ValidateUserForThirdParty([FromBody] AppSessionReq appSessionReq)
        {
            var appValidatedUserResp = new AppValidatedUserResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvaildRoute
            };
            ICallbackML callbackML = new CallbackML(_accessor, _env);
            if (callbackML.ValidateCallbackIP())
            {
                if (appSessionReq != null)
                {
                    appValidatedUserResp = appML.ValidatedUserData(appSessionReq);
                }
                else
                {
                    appValidatedUserResp.Msg = "Incomplete parameter!";
                }
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "ValidateUserForThirdParty",
                CommonStr2 = JsonConvert.SerializeObject(appSessionReq),
                CommonStr3 = JsonConvert.SerializeObject(appValidatedUserResp)
            });
            return Json(appValidatedUserResp);
        }
        #endregion
        #region AEPSRegion
        [HttpPost]
        public IActionResult GetAEPSBanks([FromBody] AppSessionReq appSessionReq)
        {
            var aepsBanks = new AEPSBanksResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };

            AppResponse appResp = appML.CheckAppSession(appSessionReq);
            aepsBanks.IsAppValid = appResp.IsAppValid;
            aepsBanks.IsVersionValid = appResp.IsVersionValid;
            aepsBanks.CheckID = appResp.CheckID;
            if (appResp.Statuscode == ErrorCodes.One)
            {
                IBankML bankML = new BankML(_accessor, _env, false);
                var banks = bankML.AEPSBankMasters();
                aepsBanks.aepsBanks = banks;
                aepsBanks.Statuscode = ErrorCodes.One;
                aepsBanks.Msg = ErrorCodes.SUCCESS;
            }
            else
                aepsBanks.Msg = appResp.Msg;
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "GetAEPSBanks",
                CommonStr2 = JsonConvert.SerializeObject(appSessionReq),
                CommonStr3 = JsonConvert.SerializeObject(aepsBanks)
            });
            return Json(aepsBanks);
        }
        [HttpPost]
        public async Task<IActionResult> GetBalanceAEPS([FromBody] AEPSBalanceRequest balanceReq)
        {
            var balanceResponse = new AEPSBalanceResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvaildSession
            };

            var appResp = appML.CheckAppSession(balanceReq);
            balanceResponse.IsAppValid = appResp.IsAppValid;
            balanceResponse.IsVersionValid = appResp.IsVersionValid;
            balanceResponse.IsPasswordExpired = appResp.IsPasswordExpired;
            balanceResponse.Statuscode = appResp.Statuscode;
            if (appResp.Statuscode == ErrorCodes.One)
            {
                balanceResponse.Statuscode = appResp.Statuscode;
                balanceResponse.Msg = appResp.Msg;
                if (!appResp.IsPasswordExpired)
                {
                    IDeviceML deviceMl = new DeviceML(_accessor, _env, false);
                    var balRes = await deviceMl.CheckBalance(balanceReq.pidData, balanceReq.Aadhar, balanceReq.BankIIN, balanceReq.InterfaceType, balanceReq.UserID, appResp.GetID, RequestMode.APPS, 0, balanceReq.IMEI, balanceReq.Lattitude, balanceReq.Longitude,balanceReq.pidDataXML).ConfigureAwait(false);
                    balanceResponse.Statuscode = balRes.Statuscode;
                    balanceResponse.Msg = balRes.Msg;
                    if (balRes.Statuscode == ErrorCodes.One)
                    {
                        balanceResponse.Balance = balRes.Balance;
                    }
                }
            }
            else
            {
                balanceResponse.Msg = appResp.Msg;
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "GetBalanceAEPS",
                CommonStr2 = JsonConvert.SerializeObject(balanceReq),
                CommonStr3 = JsonConvert.SerializeObject(balanceResponse)
            });
            return Json(balanceResponse);
        }
        [HttpPost]
        public async Task<IActionResult> AEPSWithdrawal([FromBody] AEPSBalanceRequest balanceReq)
        {
            var aEPSWithResponse = new AEPSWithResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvaildSession
            };

            var appResp = appML.CheckAppSession(balanceReq);
            aEPSWithResponse.IsAppValid = appResp.IsAppValid;
            aEPSWithResponse.IsVersionValid = appResp.IsVersionValid;
            aEPSWithResponse.IsPasswordExpired = appResp.IsPasswordExpired;
            aEPSWithResponse.Statuscode = appResp.Statuscode;
            if (appResp.Statuscode == ErrorCodes.One)
            {
                aEPSWithResponse.Statuscode = appResp.Statuscode;
                aEPSWithResponse.Msg = appResp.Msg;
                if (!appResp.IsPasswordExpired)
                {
                    IDeviceML deviceMl = new DeviceML(_accessor, _env, false);
                    var withRes = await deviceMl.Withdrawl(balanceReq.pidData, balanceReq.Aadhar, balanceReq.BankIIN, balanceReq.InterfaceType, balanceReq.Amount, balanceReq.UserID, appResp.GetID, RequestMode.APPS, 0, balanceReq.IMEI, balanceReq.Lattitude, balanceReq.Longitude,balanceReq.pidDataXML).ConfigureAwait(false);
                    aEPSWithResponse.Statuscode = withRes.Statuscode;
                    aEPSWithResponse.Msg = withRes.Msg;
                    if (withRes.Statuscode == ErrorCodes.One)
                    {
                        aEPSWithResponse.Status = withRes.Status;
                        aEPSWithResponse.LiveID = withRes.LiveID;
                        aEPSWithResponse.TransactionID = withRes.TransactionID;
                        aEPSWithResponse.ServerDate = DateTime.Now.ToString("dd MMM yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                        aEPSWithResponse.Balance = withRes.Balance;
                    }
                }
            }
            else
            {
                aEPSWithResponse.Msg = appResp.Msg;
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "AEPSWithdrawal",
                CommonStr2 = JsonConvert.SerializeObject(balanceReq),
                CommonStr3 = JsonConvert.SerializeObject(aEPSWithResponse)
            });
            return Json(aEPSWithResponse);
        }
        [HttpPost]
        public async Task<IActionResult> GenerateDepositOTP([FromBody] AEPSBalanceRequest balanceReq)
        {
            var aEPSDepositOTPResponse = new AEPSDepositOTPResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvaildSession
            };

            var appResp = appML.CheckAppSession(balanceReq);
            aEPSDepositOTPResponse.IsAppValid = appResp.IsAppValid;
            aEPSDepositOTPResponse.IsVersionValid = appResp.IsVersionValid;
            aEPSDepositOTPResponse.IsPasswordExpired = appResp.IsPasswordExpired;
            aEPSDepositOTPResponse.Statuscode = appResp.Statuscode;
            if (appResp.Statuscode == ErrorCodes.One)
            {
                aEPSDepositOTPResponse.Statuscode = appResp.Statuscode;
                aEPSDepositOTPResponse.Msg = appResp.Msg;
                if (!appResp.IsPasswordExpired)
                {
                    IDeviceML deviceMl = new DeviceML(_accessor, _env, false);
                    var depositRes = await deviceMl.DepositGenerateOTP(new DepositRequest
                    {
                        UserID = balanceReq.UserID,
                        OutletID = appResp.GetID,
                        InterfaceType = balanceReq.InterfaceType,
                        AccountNo = balanceReq.Aadhar,
                        IIN = balanceReq.BankIIN,
                        Amount = balanceReq.Amount,
                        RMode = RequestMode.APPS,
                        IMEI = balanceReq.IMEI,
                        Lattitude = balanceReq.Lattitude,
                        Longitude = balanceReq.Longitude
                    }).ConfigureAwait(false);
                    aEPSDepositOTPResponse.Statuscode = depositRes.Statuscode;
                    aEPSDepositOTPResponse.Msg = depositRes.Msg;
                    aEPSDepositOTPResponse.IsOTPRequired = depositRes.Statuscode == ErrorCodes.One;
                    if (aEPSDepositOTPResponse.IsOTPRequired)
                    {
                        aEPSDepositOTPResponse.Reff1 = depositRes.TransactionID;
                        aEPSDepositOTPResponse.Reff2 = depositRes.RefferenceNo;
                        aEPSDepositOTPResponse.Reff3 = depositRes.VendorID;
                    }
                }
            }
            else
            {
                aEPSDepositOTPResponse.Msg = appResp.Msg;
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "GenerateDepositOTP",
                CommonStr2 = JsonConvert.SerializeObject(balanceReq),
                CommonStr3 = JsonConvert.SerializeObject(aEPSDepositOTPResponse)
            });
            return Json(aEPSDepositOTPResponse);
        }
        [HttpPost]
        public async Task<IActionResult> VerifyDepositOTP([FromBody] AEPSBalanceRequest balanceReq)
        {
            var aEPSWithResponse = new AEPSWithResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvaildSession
            };

            var appResp = appML.CheckAppSession(balanceReq);
            aEPSWithResponse.IsAppValid = appResp.IsAppValid;
            aEPSWithResponse.IsVersionValid = appResp.IsVersionValid;
            aEPSWithResponse.IsPasswordExpired = appResp.IsPasswordExpired;
            aEPSWithResponse.Statuscode = appResp.Statuscode;
            if (appResp.Statuscode == ErrorCodes.One)
            {
                aEPSWithResponse.Statuscode = appResp.Statuscode;
                aEPSWithResponse.Msg = appResp.Msg;
                if (!appResp.IsPasswordExpired)
                {
                    IDeviceML deviceMl = new DeviceML(_accessor, _env, false);
                    var depositRes = await deviceMl.DepositVerifyOTP(new DepositRequest
                    {
                        UserID = balanceReq.UserID,
                        OutletID = appResp.GetID,
                        InterfaceType = balanceReq.InterfaceType,
                        AccountNo = balanceReq.Aadhar,
                        IIN = balanceReq.BankIIN,
                        Amount = balanceReq.Amount,
                        RMode = RequestMode.APPS,
                        OTP = balanceReq.OTP,
                        Reff1 = balanceReq.Reff1,
                        Reff2 = balanceReq.Reff2,
                        Reff3 = balanceReq.Reff3,
                        IMEI = balanceReq.IMEI,
                        Lattitude = balanceReq.Lattitude,
                        Longitude = balanceReq.Longitude
                    }).ConfigureAwait(false);
                    aEPSWithResponse.Statuscode = depositRes.Statuscode;
                    aEPSWithResponse.Msg = depositRes.Msg;
                    if (depositRes.Statuscode == ErrorCodes.One)
                    {
                        aEPSWithResponse.Status = depositRes.Status;
                        aEPSWithResponse.LiveID = depositRes.LiveID;
                        aEPSWithResponse.TransactionID = depositRes.TransactionID;
                        aEPSWithResponse.ServerDate = DateTime.Now.ToString("dd MMM yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                        aEPSWithResponse.Balance = depositRes.Balance;
                        aEPSWithResponse.BeneficiaryName = depositRes.BeneficaryName;
                    }
                }
            }
            else
            {
                aEPSWithResponse.Msg = appResp.Msg;
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "VerifyDepositOTP",
                CommonStr2 = JsonConvert.SerializeObject(balanceReq),
                CommonStr3 = JsonConvert.SerializeObject(aEPSWithResponse)
            });
            return Json(aEPSWithResponse);
        }
        [HttpPost]
        public async Task<IActionResult> DepositNow([FromBody] AEPSBalanceRequest balanceReq)
        {
            var aEPSWithResponse = new AEPSWithResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvaildSession
            };

            var appResp = appML.CheckAppSession(balanceReq);
            aEPSWithResponse.IsAppValid = appResp.IsAppValid;
            aEPSWithResponse.IsVersionValid = appResp.IsVersionValid;
            aEPSWithResponse.IsPasswordExpired = appResp.IsPasswordExpired;
            aEPSWithResponse.Statuscode = appResp.Statuscode;
            if (appResp.Statuscode == ErrorCodes.One)
            {
                aEPSWithResponse.Statuscode = appResp.Statuscode;
                aEPSWithResponse.Msg = appResp.Msg;
                if (!appResp.IsPasswordExpired)
                {
                    IDeviceML deviceMl = new DeviceML(_accessor, _env, false);
                    var depositRes = await deviceMl.DepositAccount(new DepositRequest
                    {
                        UserID = balanceReq.UserID,
                        OutletID = appResp.GetID,
                        InterfaceType = balanceReq.InterfaceType,
                        AccountNo = balanceReq.Aadhar,
                        IIN = balanceReq.BankIIN,
                        Amount = balanceReq.Amount,
                        RMode = RequestMode.APPS,
                        OTP = balanceReq.OTP,
                        Reff1 = balanceReq.Reff1,
                        Reff2 = balanceReq.Reff2,
                        Reff3 = balanceReq.Reff3,
                        IMEI = balanceReq.IMEI,
                        Lattitude = balanceReq.Lattitude,
                        Longitude = balanceReq.Longitude
                    }).ConfigureAwait(false);
                    aEPSWithResponse.Statuscode = depositRes.Statuscode;
                    aEPSWithResponse.Msg = depositRes.Msg;
                    if (depositRes.Statuscode == ErrorCodes.One)
                    {
                        aEPSWithResponse.Status = depositRes.Status;
                        aEPSWithResponse.LiveID = depositRes.LiveID;
                        aEPSWithResponse.TransactionID = depositRes.TransactionID;
                        aEPSWithResponse.ServerDate = DateTime.Now.ToString("dd MMM yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                        aEPSWithResponse.Balance = depositRes.Balance;
                    }
                }
            }
            else
            {
                aEPSWithResponse.Msg = appResp.Msg;
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "DepositNow",
                CommonStr2 = JsonConvert.SerializeObject(balanceReq),
                CommonStr3 = JsonConvert.SerializeObject(aEPSWithResponse)
            });
            return Json(aEPSWithResponse);
        }
        [HttpPost]
        public async Task<IActionResult> Aadharpay([FromBody] AEPSBalanceRequest balanceReq)
        {
            var aEPSWithResponse = new AEPSWithResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvaildSession
            };

            var appResp = appML.CheckAppSession(balanceReq);
            aEPSWithResponse.IsAppValid = appResp.IsAppValid;
            aEPSWithResponse.IsVersionValid = appResp.IsVersionValid;
            aEPSWithResponse.IsPasswordExpired = appResp.IsPasswordExpired;
            aEPSWithResponse.Statuscode = appResp.Statuscode;
            if (appResp.Statuscode == ErrorCodes.One)
            {
                aEPSWithResponse.Statuscode = appResp.Statuscode;
                aEPSWithResponse.Msg = appResp.Msg;
                if (!appResp.IsPasswordExpired)
                {
                    IDeviceML deviceMl = new DeviceML(_accessor, _env, false);
                    var withRes = await deviceMl.Aadharpay(balanceReq.pidData, balanceReq.Aadhar, balanceReq.BankIIN, balanceReq.InterfaceType, balanceReq.Amount, balanceReq.UserID, appResp.GetID, RequestMode.APPS, 0, balanceReq.IMEI, balanceReq.Lattitude, balanceReq.Longitude, balanceReq.pidDataXML).ConfigureAwait(false);
                    aEPSWithResponse.Statuscode = withRes.Statuscode;
                    aEPSWithResponse.Msg = withRes.Msg;
                    if (withRes.Statuscode == ErrorCodes.One)
                    {
                        aEPSWithResponse.Status = withRes.Status;
                        aEPSWithResponse.LiveID = withRes.LiveID;
                        aEPSWithResponse.TransactionID = withRes.TransactionID;
                        aEPSWithResponse.ServerDate = DateTime.Now.ToString("dd MMM yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                        aEPSWithResponse.Balance = withRes.Balance;
                    }
                }
            }
            else
            {
                aEPSWithResponse.Msg = appResp.Msg;
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "Aadharpay",
                CommonStr2 = JsonConvert.SerializeObject(balanceReq),
                CommonStr3 = JsonConvert.SerializeObject(aEPSWithResponse)
            });
            return Json(aEPSWithResponse);
        }
        [HttpPost]
        public async Task<IActionResult> BankMiniStatement([FromBody] AEPSBalanceRequest balanceReq)
        {
            var miniSTMTResponse = new BankMiniSTMTResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvaildSession
            };

            var appResp = appML.CheckAppSession(balanceReq);
            miniSTMTResponse.IsAppValid = appResp.IsAppValid;
            miniSTMTResponse.IsVersionValid = appResp.IsVersionValid;
            miniSTMTResponse.IsPasswordExpired = appResp.IsPasswordExpired;
            miniSTMTResponse.Statuscode = appResp.Statuscode;
            if (appResp.Statuscode == ErrorCodes.One)
            {
                miniSTMTResponse.Statuscode = appResp.Statuscode;
                miniSTMTResponse.Msg = appResp.Msg;
                if (!appResp.IsPasswordExpired)
                {
                    IDeviceML deviceMl = new DeviceML(_accessor, _env, false);
                    var miniSTMTRes = deviceMl.MiniStatement(balanceReq.pidData, balanceReq.Aadhar, balanceReq.BankName, balanceReq.BankIIN, balanceReq.InterfaceType, balanceReq.UserID, appResp.GetID, RequestMode.APPS, 0, SPKeys.AepsMiniStatement, balanceReq.IMEI, balanceReq.Lattitude, balanceReq.Longitude,balanceReq.pidDataXML);
                    miniSTMTResponse.Statuscode = miniSTMTRes.Statuscode == RechargeRespType.FAILED || miniSTMTRes.Statuscode == ErrorCodes.Minus1 ? ErrorCodes.Minus1 : ErrorCodes.One;
                    miniSTMTResponse.Msg = miniSTMTRes.Msg;
                    if (miniSTMTRes.Statuscode == ErrorCodes.One)
                    {
                        miniSTMTResponse.Balance = miniSTMTRes.Balance.ToString();
                        miniSTMTResponse.Statements = miniSTMTRes.Statements;
                    }
                }
            }
            else
            {
                miniSTMTResponse.Msg = appResp.Msg;
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "BankMiniStatement",
                CommonStr2 = JsonConvert.SerializeObject(balanceReq),
                CommonStr3 = JsonConvert.SerializeObject(miniSTMTResponse)
            });
            return Json(miniSTMTResponse);
        }
        #endregion
        [HttpPost]
        public async Task<IActionResult> AppUserpartialUpdate([FromBody] AppUserCreate appUserEdit)
        {
            var appResp = new AppResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (appUserEdit.LoginTypeID == LoginType.ApplicationUser)
            {
                var appRequest = new AppSessionReq
                {
                    APPID = appUserEdit.APPID ?? "",
                    IMEI = appUserEdit.IMEI ?? "",
                    LoginTypeID = appUserEdit.LoginTypeID,
                    UserID = appUserEdit.UserID,
                    SessionID = appUserEdit.SessionID,
                    RegKey = appUserEdit.RegKey ?? "",
                    SerialNo = appUserEdit.SerialNo ?? "",
                    Version = appUserEdit.Version ?? "",
                    Session = appUserEdit.Session
                };
                appResp = appML.CheckAppSession(appRequest);

                if (appResp.Statuscode == ErrorCodes.One)
                {
                    if (!appResp.IsPasswordExpired)
                    {
                        appUserEdit.editUser.LT = appUserEdit.LoginTypeID;
                        appUserEdit.editUser.LoginID = appUserEdit.UserID;

                        IUserAPPML userML = new UserML(_accessor, _env, false);
                        await Task.Delay(0);
                        var res = userML.UserBankRequestApp(appUserEdit.editUser);
                        appResp.Statuscode = res.Statuscode;
                        appResp.Msg = res.Msg;
                    }
                }
            }
            return Json(appResp);
        }
        [HttpPost]
        public async Task<IActionResult> UserSubscriptionApp([FromBody] UserSubscriptionApp UserSubApp)
        {
            var appResponse = new AppResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            var appRequest = new AppRequest
            {
                APPID = UserSubApp.APPID,
                IMEI = UserSubApp.IMEI,
                RegKey = UserSubApp.RegKey,
                SerialNo = UserSubApp.SerialNo,
                Version = UserSubApp.Version
            };
            appResponse = appML.CheckApp(appRequest);
            if (appResponse.Statuscode == ErrorCodes.One)
            {
                var getIntouch = new GetIntouch
                {
                    RequestIP = UserSubApp.IMEI,
                    Browser = UserSubApp.SerialNo + "_" + UserSubApp.Version,
                    Name = UserSubApp.Name,
                    EmailID = UserSubApp.EmailID,
                    Message = UserSubApp.Message,
                    MobileNo = UserSubApp.MobileNo,
                    RequestPage = UserSubApp.RequestPage,
                    WID = appResponse.CheckID

                };

                ILoginML _loginML = new LoginML(_accessor, _env, false);
                await Task.Delay(0);
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
        #region Signup Referral
        [HttpPost]
        public IActionResult GetRoleForReferral([FromBody] AppRequestReferral appRequest)
        {
            var appResponse = new AppReportResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            var appResp = appML.CheckApp(appRequest);
            appResponse.Statuscode = appResp.Statuscode;
            appResponse.Msg = appResp.Msg;
            if (appResponse.Statuscode == ErrorCodes.One)
            {
                ISettingML _settingML = new SettingML(_accessor, _env);
                appResponse.ChildRoles = _settingML.GetRoleForReferral(appRequest.ReferralID);
                appResponse.Statuscode = ErrorCodes.One;
                appResponse.Msg = ErrorCodes.SUCCESS;
            }
            return Json(appResponse);
        }
        #endregion
        public IActionResult GetPopupAfterLogin([FromBody] AppSessionReq appSessionReq)
        {
            var appPopupResponse = new AppPopupResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            AppResponse appResp = appML.CheckAppSession(appSessionReq);
            appPopupResponse.IsAppValid = appResp.IsAppValid;
            appPopupResponse.IsVersionValid = appResp.IsVersionValid;
            if (appResp.Statuscode == ErrorCodes.One)
            {
                IBannerML ml = new ResourceML(_accessor, _env);
                appPopupResponse.Popup = ml.GetAfterLoginPoup(appResp.CheckID.ToString());
                appPopupResponse.Statuscode = ErrorCodes.One;
                appPopupResponse.Msg = ErrorCodes.SUCCESS;
            }
            else
                appPopupResponse.Msg = appResp.Msg;
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "GetPopupAfterLogin",
                CommonStr2 = JsonConvert.SerializeObject(appSessionReq),
                CommonStr3 = JsonConvert.SerializeObject(appPopupResponse)
            });
            return Json(appPopupResponse);
        }
        #region package
        [HttpPost]
        public async Task<IActionResult> GetAvailablePackages([FromBody] AppSessionReq appRequest)
        {
            var appResponse = new AvailablePackageResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvaildSession
            };
            var appResp = appML.CheckAppSession(appRequest);
            appResponse.IsAppValid = appResp.IsAppValid;
            appResponse.IsVersionValid = appResp.IsVersionValid;
            appResponse.IsPasswordExpired = appResp.IsPasswordExpired;
            appResponse.Statuscode = appResp.Statuscode;
            if (appResp.Statuscode == ErrorCodes.One)
            {
                if (!appResp.IsPasswordExpired)
                {
                    IPackageML mL = new PackageML(_accessor, _env, false);
                    var Res = await Task.FromResult(mL.GetAvailablePackagesForApp(appRequest.UserID, LoginType.ApplicationUser)).ConfigureAwait(false);
                    appResponse.PDetail = Res.PDetail;
                    appResponse.Statuscode = Res.Statuscode;
                    appResponse.Msg = Res.Msg;
                }
            }
            else
            {
                appResponse.Msg = appResp.Msg;
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "GetAvailablePackages",
                CommonStr2 = JsonConvert.SerializeObject(appRequest),
                CommonStr3 = JsonConvert.SerializeObject(appResponse)
            });
            return Json(appResponse);
        }
        [HttpPost]
        public async Task<IActionResult> UpgradePackage([FromBody] UpgradePackageReq appRequest)
        {
            var appResponse = new AppResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvaildSession
            };
            appResponse = appML.CheckAppSession(appRequest);

            if (appResponse.Statuscode == ErrorCodes.One)
            {
                if (!appResponse.IsPasswordExpired)
                {
                    IPackageML mL = new PackageML(_accessor, _env, false);
                    var Res = await Task.FromResult(mL.UpgradePackageForApp(appRequest)).ConfigureAwait(false);
                    appResponse.Statuscode = Res.Statuscode;
                    appResponse.Msg = Res.Msg;
                }
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "UpgradePackage",
                CommonStr2 = JsonConvert.SerializeObject(appRequest),
                CommonStr3 = JsonConvert.SerializeObject(appResponse)
            });
            return Json(appResponse);
        }
        #endregion
        [HttpPost]
        public async Task<IActionResult> RSlabRangDetail([FromBody] SlabRangDetailReq appRequest)
        {
            var appResponse = new SlabRangDetailRes
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvaildSession
            };
            var appResp = appML.CheckAppSession(appRequest);

            appResponse.IsAppValid = appResp.IsAppValid;
            appResponse.IsVersionValid = appResp.IsVersionValid;
            appResponse.IsPasswordExpired = appResp.IsPasswordExpired;
            appResponse.Statuscode = appResp.Statuscode;
            if (appResponse.Statuscode == ErrorCodes.One)
            {
                if (!appResponse.IsPasswordExpired)
                {
                    ISlabML mL = new SlabML(_accessor, _env, false);
                    var Res = await Task.FromResult(mL.GetSlabRangeDetailForApp(appRequest)).ConfigureAwait(false);
                    appResponse.Statuscode = Res.Statuscode;
                    appResponse.Msg = Res.Msg;
                    appResponse.SlabRangeDetail = Res.SlabRangeDetail;
                }
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "RSlabRangDetail",
                CommonStr2 = JsonConvert.SerializeObject(appRequest),
                CommonStr3 = JsonConvert.SerializeObject(appResponse)
            });
            return Json(appResponse);
        }
        [HttpPost]
        public async Task<IActionResult> GetDTHPackage([FromBody] DTHPackageRequest appRequest)
        {
            var appResponse = new DTHPackageResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvaildSession
            };
            var appResp = appML.CheckAppSession(appRequest);

            appResponse.IsAppValid = appResp.IsAppValid;
            appResponse.IsVersionValid = appResp.IsVersionValid;
            appResponse.IsPasswordExpired = appResp.IsPasswordExpired;
            appResponse.Statuscode = appResp.Statuscode;
            if (appResponse.Statuscode == ErrorCodes.One)
            {
                if (!appResponse.IsPasswordExpired)
                {
                    IOperatorML mL = new OperatorML(_accessor, _env, false);
                    var Res = await Task.FromResult(mL.GetDTHPackageForApp(appRequest)).ConfigureAwait(false);
                    appResponse.Statuscode = Res.Statuscode;
                    appResponse.Msg = Res.Msg;
                    appResponse.DTHPackage = Res.DTHPackage;
                }
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "GetDTHPackage",
                CommonStr2 = JsonConvert.SerializeObject(appRequest),
                CommonStr3 = JsonConvert.SerializeObject(appResponse)
            });
            return Json(appResponse);
        }
        [HttpPost]
        public async Task<IActionResult> DTHChannelByPackageID([FromBody] DTHChannelRequest appRequest)
        {
            var appResponse = new DTHChannelResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvaildSession
            };
            var appResp = appML.CheckAppSession(appRequest);

            appResponse.IsAppValid = appResp.IsAppValid;
            appResponse.IsVersionValid = appResp.IsVersionValid;
            appResponse.IsPasswordExpired = appResp.IsPasswordExpired;
            appResponse.Statuscode = appResp.Statuscode;
            if (appResponse.Statuscode == ErrorCodes.One)
            {
                if (!appResponse.IsPasswordExpired)
                {
                    IOperatorML mL = new OperatorML(_accessor, _env, false);
                    var Res = await Task.FromResult(mL.DTHChannelByPackageForApp(appRequest)).ConfigureAwait(false);
                    appResponse.Statuscode = Res.Statuscode;
                    appResponse.Msg = Res.Msg;
                    appResponse.DTHChannels = Res.DTHChannels;
                }
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "DTHChannelByPackageID",
                CommonStr2 = JsonConvert.SerializeObject(appRequest),
                CommonStr3 = JsonConvert.SerializeObject(appResponse)
            });
            return Json(appResponse);
        }
        [HttpPost]
        public async Task<IActionResult> GetDTHPlanListByLanguage([FromBody] AppSimplePlanReq appSimplePlanReq)
        {
            var appDTHSimplePlanResp = new AppDTHSimplePlanResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvalidParam
            };
            var appRequest = new AppRequest
            {
                APPID = appSimplePlanReq.APPID,
                IMEI = appSimplePlanReq.IMEI,
                Version = appSimplePlanReq.Version,
                RegKey = appSimplePlanReq.RegKey,
                SerialNo = appSimplePlanReq.SerialNo
            };
            var appResp = appML.CheckApp(appRequest);
            appDTHSimplePlanResp.IsAppValid = appResp.IsAppValid;
            appDTHSimplePlanResp.IsVersionValid = appResp.IsVersionValid;
            var plansAPIML = new PlansAPIML(_accessor, _env, false);
            if (appResp.Statuscode == ErrorCodes.One)
            {
                await Task.Delay(0);

                if (ApplicationSetting.PlanType == PlanType.Roundpay)
                {
                    IRoundpayPlan RPPlan = new PlansAPIML(_accessor, _env);
                    appDTHSimplePlanResp.DataRPDTHWithPackage = plansAPIML.RPDTHPlanListByLanguage(appSimplePlanReq.OID, appSimplePlanReq.Language);
                }
                appDTHSimplePlanResp.Statuscode = ErrorCodes.One;
                appDTHSimplePlanResp.Msg = ErrorCodes.SUCCESS;
            }
            else
            {
                appDTHSimplePlanResp.Msg = appResp.Msg;
            }
            plansAPIML.LogAppReqResp(new CommonReq
            {
                CommonStr = "DTHChannelPlanInfo",
                CommonStr2 = JsonConvert.SerializeObject(appSimplePlanReq),
                CommonStr3 = JsonConvert.SerializeObject(appDTHSimplePlanResp)
            });
            return Json(appDTHSimplePlanResp);
        }
        [HttpPost]
        public IActionResult GetVideo([FromBody] AppSessionReq appSessionReq)
        {
            var AppVideoResponse = new AppReportResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            var appResp = appML.CheckAppSession(appSessionReq);
            AppVideoResponse.IsAppValid = appResp.IsAppValid;
            AppVideoResponse.IsVersionValid = appResp.IsVersionValid;
            AppVideoResponse.CheckID = appResp.CheckID;
            AppVideoResponse.IsPasswordExpired = appResp.IsPasswordExpired;
            AppVideoResponse.Statuscode = appResp.Statuscode;
            var req = new CommonReq
            {
                LoginTypeID = appSessionReq.LoginTypeID,
                LoginID = appSessionReq.UserID
            };
            if (appResp.Statuscode == ErrorCodes.One)
            {
                IResourceML ml = new ResourceML(_accessor, _env);
                AppVideoResponse.VideoList = ml.GetvideolinkApp(req);
                AppVideoResponse.Statuscode = ErrorCodes.One;
                AppVideoResponse.Msg = ErrorCodes.SUCCESS;
            }
            else
                AppVideoResponse.Msg = appResp.Msg;
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "GetVideo",
                CommonStr2 = JsonConvert.SerializeObject(appSessionReq),
                CommonStr3 = JsonConvert.SerializeObject(AppVideoResponse)
            });
            return Json(AppVideoResponse);
        }
        [HttpPost]
        public IActionResult CheckFlagsEmail([FromBody] AppSessionReq appSessionReq)
        {
            var appResponse = new FlagChecksResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            var appResp = appML.CheckAppSession(appSessionReq);
            appResponse.IsAppValid = appResp.IsAppValid;
            appResponse.IsVersionValid = appResp.IsVersionValid;
            appResponse.CheckID = appResp.CheckID;
            appResponse.IsPasswordExpired = appResp.IsPasswordExpired;
            appResponse.Statuscode = appResp.Statuscode;
            if (appResp.Statuscode == ErrorCodes.One)
            {
                IUserML ml = new UserML(_accessor, _env, false);
                var socialSetting = ml.GetSetting(appSessionReq.UserID);
                #region IsSocialAlertDone
                appResponse.IsSocialAlert = ApplicationSetting.IsSocialAlert && (string.IsNullOrEmpty(socialSetting.WhatsappNo) || string.IsNullOrEmpty(socialSetting.TelegramNo) || string.IsNullOrEmpty(socialSetting.HangoutId)) ? false : true;
                #endregion
                if (ApplicationSetting.IsEmailVefiricationRequired)
                    appResponse.IsEmailVerified = ml.IsEMailVerified(appSessionReq.UserID);
                else
                    appResponse.IsEmailVerified = true;
                appResponse.Statuscode = ErrorCodes.One;
                appResponse.Msg = ErrorCodes.SUCCESS;
            }
            else
                appResponse.Msg = appResp.Msg;
            return Json(appResponse);
        }
        [HttpPost]
        public IActionResult SendEmailVerification([FromBody] AppSessionReq appSessionReq)
        {
            var appResponse = new AppResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            var appResp = appML.CheckAppSession(appSessionReq);
            appResponse.IsAppValid = appResp.IsAppValid;
            appResponse.IsVersionValid = appResp.IsVersionValid;
            appResponse.CheckID = appResp.CheckID;
            appResponse.IsPasswordExpired = appResp.IsPasswordExpired;
            appResponse.Statuscode = appResp.Statuscode;
            if (appResp.Statuscode == ErrorCodes.One)
            {
                IUserML ml = new UserML(_accessor, _env);
                var reqParam = new LoginResponse();
                reqParam.UserID = appSessionReq.UserID;
                reqParam.WID = appResp.CheckID;
                reqParam.EmailID = appResp.EmailID;
                if (ml.SendVerifyEmailLink(reqParam))
                {
                    appResponse.Statuscode = ErrorCodes.One;
                    appResponse.Msg = "Verification link send to your register e-mail id";
                }
            }
            else
                appResponse.Msg = appResp.Msg;
            return Json(appResponse);
        }
        [HttpPost]
        public IActionResult SaveSocialAlertSetting([FromBody] SocialAlertSettingRequest appSessionReq)
        {
            var appResponse = new AppResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            var appResp = appML.CheckAppSession(appSessionReq);
            appResponse.IsAppValid = appResp.IsAppValid;
            appResponse.IsVersionValid = appResp.IsVersionValid;
            appResponse.CheckID = appResp.CheckID;
            appResponse.IsPasswordExpired = appResp.IsPasswordExpired;
            appResponse.Statuscode = appResp.Statuscode;
            if (appResp.Statuscode == ErrorCodes.One)
            {
                var param = new LowBalanceSetting
                {
                    LT = 1,
                    LoginID = appSessionReq.UserID,
                    WhatsappNo = appSessionReq.WhatsappNo,
                    TelegramNo = appSessionReq.TelegramNo,
                    HangoutId = appSessionReq.HangoutId
                };
                IUserML ml = new UserML(_accessor, _env, false);
                var res = (ResponseStatus)ml.SaveLowBalanceSetting(param);
                appResponse.Statuscode = res.Statuscode;
                appResponse.Msg = res.Msg;
            }
            else
                appResponse.Msg = appResp.Msg;
            return Json(appResponse);
        }
        #region AffliateShoping
        [HttpPost]
        public async Task<IActionResult> GetAfProducts([FromBody] AppRequest appRequest)
        {
            var Response = new AfItemResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvaildSession
            };
            var appResp = appML.CheckApp(appRequest);
            Response.IsAppValid = appResp.IsAppValid;
            Response.IsVersionValid = appResp.IsVersionValid;
            try
            {
                IAffiliationML ml = new AffiliationML(_accessor, _env);
                var res = ml.GetAfItemsForDisplay();
                Response.data = res.data;
                Response.Statuscode = ErrorCodes.One;
                Response.Msg = ErrorCodes.SUCCESS;
            }
            catch (Exception ex)
            {

            }
            return Json(Response);
        }
        #endregion
        #region MiniBankRegion
        [HttpPost]
        public async Task<IActionResult> InitiateMiniBank([FromBody] MiniBankIntiateRequest intiateRequest)
        {
            var initiateResponse = new MiniBankInitiateResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvaildSession
            };

            var appResp = appML.CheckAppSession(intiateRequest);
            initiateResponse.IsAppValid = appResp.IsAppValid;
            initiateResponse.IsVersionValid = appResp.IsVersionValid;
            initiateResponse.IsPasswordExpired = appResp.IsPasswordExpired;
            initiateResponse.Statuscode = appResp.Statuscode;
            initiateResponse.Msg = appResp.Msg;
            if (appResp.Statuscode == ErrorCodes.One)
            {
                if (!appResp.IsPasswordExpired)
                {
                    var _APICode = string.Empty;
                    if (intiateRequest.SDKType == AEPSInterfaceType.FINGPAY)
                    {
                        _APICode = APICode.FINGPAY;
                    }
                    else if (intiateRequest.SDKType == AEPSInterfaceType.MAHAGRAM)
                    {
                        _APICode = APICode.MAHAGRAM;
                    }
                    else if (intiateRequest.SDKType == AEPSInterfaceType.MOSAMBEE)
                    {
                        _APICode = APICode.MOSAMBEE;
                    }
                    IMiniBankML miniBankML = new AEPSML(_accessor, _env, false);
                    var mbResp = await miniBankML.MakeMiniBankTransaction(new MiniBankTransactionServiceReq
                    {
                        AmountR = intiateRequest.Amount,
                        OID = intiateRequest.OID,
                        TXNType = "CW",
                        APICode = _APICode,
                        LoginID = intiateRequest.UserID,
                        OutletIDSelf = appResp.GetID,
                        RequestModeID = RequestMode.APPS
                    }).ConfigureAwait(false);
                    initiateResponse.Statuscode = mbResp.Statuscode;
                    initiateResponse.Msg = mbResp.Msg;
                    if (mbResp.Statuscode == ErrorCodes.One)
                    {
                        initiateResponse.tid = mbResp.TID;
                    }
                }
            }
            else
            {
                initiateResponse.Msg = appResp.Msg;
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "InitiateMiniBank",
                CommonStr2 = JsonConvert.SerializeObject(intiateRequest),
                CommonStr3 = JsonConvert.SerializeObject(initiateResponse)
            });
            return Json(initiateResponse);
        }
        [HttpPost]
        public IActionResult UpdateMiniBankStatus([FromBody] AppMinBankupdateRequest minBankupdateRequest)
        {
            var appResponse = new MiniBankStatusResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvaildSession
            };

            var appResp = appML.CheckAppSession(minBankupdateRequest);
            appResponse.IsAppValid = appResp.IsAppValid;
            appResponse.IsVersionValid = appResp.IsVersionValid;
            appResponse.IsPasswordExpired = appResp.IsPasswordExpired;
            appResponse.Statuscode = appResp.Statuscode;
            appResponse.Msg = appResp.Msg;
            if (appResponse.Statuscode == ErrorCodes.One)
            {
                IMiniBankML miniBankML = new AEPSML(_accessor, _env, false);
                appResponse.data = miniBankML.MBStatusCheck(new MBStatusCheckRequest
                {
                    TID = minBankupdateRequest.TID,
                    APIStatus = minBankupdateRequest.APIStatus,
                    VendorID = minBankupdateRequest.VendorID,
                    RequestPage = nameof(RequestMode.APPS),
                    OutletID = appResp.GetID,
                    AccountNo = minBankupdateRequest.AccountNo,
                    BankName = minBankupdateRequest.BankName,
                    SDKMsg = minBankupdateRequest.Remark
                });
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "UpdateMiniBankStatus",
                CommonStr2 = JsonConvert.SerializeObject(minBankupdateRequest),
                CommonStr3 = JsonConvert.SerializeObject(appResponse)
            });
            return Json(appResponse);
        }
        #endregion
        [HttpPost]
        public async Task<IActionResult> AutoBillingUpdateApp([FromBody] AppAutoBillModel appSessionReq)
        {
            var appResponse = new AppResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (appSessionReq.LoginTypeID == LoginType.ApplicationUser)
            {
                var appRequest = new AppSessionReq
                {
                    APPID = appSessionReq.APPID ?? "",
                    IMEI = appSessionReq.IMEI ?? "",
                    LoginTypeID = appSessionReq.LoginTypeID,
                    UserID = appSessionReq.UserID,
                    SessionID = appSessionReq.SessionID,
                    RegKey = appSessionReq.RegKey ?? "",
                    SerialNo = appSessionReq.SerialNo ?? "",
                    Version = appSessionReq.Version ?? "",
                    Session = appSessionReq.Session
                };
                appResponse = appML.CheckAppSession(appRequest);
                if (appResponse.Statuscode == ErrorCodes.One)
                {
                    if (!appResponse.IsPasswordExpired)
                    {
                        IUserML userML = new UserML(_accessor, _env, false);
                        await Task.Delay(0);
                        var autoBillingModel = new AutoBillingModel
                        {
                            LT = appSessionReq.LoginTypeID,
                            LoginID = appSessionReq.UserID,
                            UserId = appSessionReq.UserIdInput,
                            IsAutoBilling = appSessionReq.IsAutoBilling,
                            FromFOSAB = appSessionReq.FromFOSAB,
                            BalanceForAB = appSessionReq.BalanceForAB,
                            AlertBalance = appSessionReq.AlertBalance,
                            MaxBillingCountAB = appSessionReq.MaxBillingCountAB,
                            MaxCreditLimitAB = appSessionReq.MaxCreditLimitAB,
                            MaxTransferLimitAB = appSessionReq.MaxTransferLimitAB,
                            UserIdBulk = ""
                        };
                        var RespSts = userML.UpdateAutoBilling(autoBillingModel);
                        appResponse.Statuscode = RespSts.Statuscode;
                        appResponse.Msg = RespSts.Msg;
                    }
                }
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "AutoBillingUpdateApp",
                CommonStr2 = JsonConvert.SerializeObject(appSessionReq),
                CommonStr3 = JsonConvert.SerializeObject(appResponse)
            });
            return Json(appResponse);
        }
        #region MNPKendra
        [HttpPost]
        public IActionResult GetMNPStatus([FromBody] AppSessionReq appSessionReq)
        {
            var appResponse = new MNPStsResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            var chkresp = appML.CheckAppSession(appSessionReq);
            if (chkresp.Statuscode == ErrorCodes.One)
            {
                var userML = new UserML(_accessor, _env);
                CommonReq req = new CommonReq
                {
                    LoginTypeID = LoginType.ApplicationUser,
                    LoginID = appSessionReq.UserID
                };
                appResponse = userML.GetMNPStatus(req);
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "GetMNPStatus",
                CommonStr2 = JsonConvert.SerializeObject(appSessionReq),
                CommonStr3 = JsonConvert.SerializeObject(appResponse)
            });
            return Json(appResponse);
        }
        [HttpPost]
        public IActionResult UserMNPRegistration([FromBody] MNPRegistration mNPRegistration)
        {
            var appResponse = new MNPStsResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            var chkresp = appML.CheckAppSession(mNPRegistration);
            if (chkresp.Statuscode == ErrorCodes.One)
            {
                var userML = new UserML(_accessor, _env);

                appResponse = userML.UserMNPRegistration(mNPRegistration);
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "UserMNPRegistration",
                CommonStr2 = JsonConvert.SerializeObject(mNPRegistration),
                CommonStr3 = JsonConvert.SerializeObject(appResponse)
            });
            return Json(appResponse);
        }
        [HttpPost]
        public IActionResult UserMNPClaim([FromBody] MNPClaimReq mNPClaimReq)
        {
            var appResponse = new AppResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            appResponse = appML.CheckAppSession(mNPClaimReq);
            if (appResponse.Statuscode == ErrorCodes.One)
            {
                var userML = new UserML(_accessor, _env);
                var _resp = userML.UserMNPRequest(mNPClaimReq);
                appResponse.Statuscode = _resp.Statuscode;
                appResponse.Msg = _resp.Msg;
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "UserMNPClaim",
                CommonStr2 = JsonConvert.SerializeObject(mNPClaimReq),
                CommonStr3 = JsonConvert.SerializeObject(appResponse)
            });
            return Json(appResponse);
        }
        public IActionResult GetUserClaimsReport([FromBody] MNPClaimDataReq mNPClaimDataReq)
        {
            var appResponse = new MNPClaimData
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            var _checkSession = appML.CheckAppSession(mNPClaimDataReq);
            if (_checkSession.Statuscode == ErrorCodes.One)
            {
                appResponse.IsVersionValid = _checkSession.IsVersionValid;
                appResponse.IsAppValid = _checkSession.IsAppValid;
                appResponse.IsPasswordExpired = _checkSession.IsPasswordExpired;
                var userML = new UserML(_accessor, _env);
                appResponse.MNPClaimsList = userML.GetUserClaimsReport(mNPClaimDataReq);
                if (appResponse.MNPClaimsList.Count > 0)
                {
                    appResponse.Statuscode = ErrorCodes.One;
                    appResponse.Msg = "Record Found!";
                }
                else
                {
                    appResponse.Statuscode = ErrorCodes.One;
                    appResponse.Msg = "No Record Found!";
                }
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "GetUserClaimsReport",
                CommonStr2 = JsonConvert.SerializeObject(mNPClaimDataReq),
                CommonStr3 = JsonConvert.SerializeObject(appResponse)
            });
            return Json(appResponse);
        }
        #endregion
        #region WalletToWalletFT
        [HttpPost]
        public IActionResult GetUDetailByMob([FromBody] WtWUserDetailReq appSessionReq)
        {
            var appResponse = new WtWUserDetailResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            var chkSess = appML.CheckAppSession(appSessionReq);
            if (chkSess.Statuscode == ErrorCodes.One)
            {
                IUserML uml = new UserML(_accessor, _env);
                var req = new CommonReq
                {
                    LoginTypeID = LoginType.ApplicationUser,
                    LoginID = appSessionReq.UserID,
                    CommonStr = appSessionReq.MobileNo
                };
                appResponse.UDetailsWTW = uml.GetUserByMobile(req);
                appResponse.Statuscode = chkSess.Statuscode;
                appResponse.Msg = chkSess.Msg;
                appResponse.IsVersionValid = chkSess.IsVersionValid;
                appResponse.IsAppValid = chkSess.IsAppValid;
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "GetUDetailByMob",
                CommonStr2 = JsonConvert.SerializeObject(appSessionReq),
                CommonStr3 = JsonConvert.SerializeObject(appResponse)
            });
            return Json(appResponse);
        }
        [HttpPost]
        public IActionResult WalletToWalletFT([FromBody] WTWFTReq appSessionReq)
        {
            var appResponse = new AppResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            appResponse = appML.CheckAppSession(appSessionReq);
            if (appResponse.Statuscode == ErrorCodes.One)
            {
                var seller = new SellerML(_accessor, _env);
                CommonReq req = new CommonReq
                {
                    LoginTypeID = appSessionReq.LoginTypeID,
                    LoginID = appSessionReq.UserID,
                    UserID = appSessionReq.UID,
                    CommonDecimal = appSessionReq.Amount,
                    CommonStr = appSessionReq.Remark,
                    CommonStr2 = appSessionReq.PIN,
                    CommonInt = appSessionReq.WalletID
                };
                IUserML uml = new UserML(_accessor, _env);
                var res = uml.WTWFT(req);
                appResponse.Statuscode = res.Statuscode;
                appResponse.Msg = res.Msg;
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "WalletToWalletFT",
                CommonStr2 = JsonConvert.SerializeObject(appSessionReq),
                CommonStr3 = JsonConvert.SerializeObject(appResponse)
            });
            return Json(appResponse);
        }
        #endregion
        #region FOSAccountStatement
        public async Task<IActionResult> AccStmtAndColl([FromBody] AppASLedgerReq appReportCommon)
        {
            var appReportResponse = new AppReportResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            var appRequest = new AppSessionReq
            {
                APPID = appReportCommon.APPID,
                IMEI = appReportCommon.IMEI,
                LoginTypeID = appReportCommon.LoginTypeID,
                UserID = appReportCommon.UserID,
                SessionID = appReportCommon.SessionID,
                RegKey = appReportCommon.RegKey,
                SerialNo = appReportCommon.SerialNo,
                Version = appReportCommon.Version,
                Session = appReportCommon.Session
            };
            var appResp = appML.CheckAppSession(appRequest);
            appReportResponse.IsAppValid = appResp.IsAppValid;
            appReportResponse.IsVersionValid = appResp.IsVersionValid;
            appReportResponse.IsPasswordExpired = appResp.IsPasswordExpired;
            appReportResponse.Statuscode = appResp.Statuscode;
            if (appResp.Statuscode == ErrorCodes.One)
            {
                if (!appResp.IsPasswordExpired)
                {
                    IAppReportML appReportML = new ReportML(_accessor, _env, false);
                    var _filter = new ULedgerReportFilter
                    {
                        UID = appReportCommon.UserID,
                        AreaID = appReportCommon.AreaID,
                        UType = appReportCommon.UType,
                        TopRows = appReportCommon.TopRows,
                        FromDate_F = appReportCommon.FromDate,
                        ToDate_F = appReportCommon.ToDate,
                        RequestMode = RequestMode.APPS
                    };
                    appReportResponse.Statuscode = ErrorCodes.One;
                    appReportResponse.Msg = ErrorCodes.SUCCESS;
                    appReportResponse.ASCReport = appReportML.GetASCSummary(_filter);
                }
            }
            else
            {
                appReportResponse.Msg = appResp.Msg;
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "LedgerReport",
                CommonStr2 = JsonConvert.SerializeObject(appReportCommon),
                CommonStr3 = JsonConvert.SerializeObject(appReportResponse)
            });
            return Json(appReportResponse);
        }
        public async Task<IActionResult> GetASSumm([FromBody] AppASLedgerReq appReportCommon)
        {
            var appReportResponse = new AppReportResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            var appRequest = new AppSessionReq
            {
                APPID = appReportCommon.APPID,
                IMEI = appReportCommon.IMEI,
                LoginTypeID = appReportCommon.LoginTypeID,
                UserID = appReportCommon.UserID,
                SessionID = appReportCommon.SessionID,
                RegKey = appReportCommon.RegKey,
                SerialNo = appReportCommon.SerialNo,
                Version = appReportCommon.Version,
                Session = appReportCommon.Session
            };
            var appResp = appML.CheckAppSession(appRequest);
            appReportResponse.IsAppValid = appResp.IsAppValid;
            appReportResponse.IsVersionValid = appResp.IsVersionValid;
            appReportResponse.IsPasswordExpired = appResp.IsPasswordExpired;
            appReportResponse.Statuscode = appResp.Statuscode;
            if (appResp.Statuscode == ErrorCodes.One)
            {
                if (!appResp.IsPasswordExpired)
                {
                    IAppReportML appReportML = new ReportML(_accessor, _env, false);
                    var _filter = new ULedgerReportFilter
                    {
                        UID = appReportCommon.UserID,
                        UType = appReportCommon.UType,
                        TopRows = appReportCommon.TopRows,
                        FromDate_F = appReportCommon.FromDate,
                        ToDate_F = appReportCommon.ToDate,
                        RequestMode = RequestMode.APPS,
                        Mobile_F = appReportCommon.Mobile
                    };
                    appReportResponse.Statuscode = ErrorCodes.One;
                    appReportResponse.Msg = ErrorCodes.SUCCESS;
                    appReportResponse.accountStatementSummary = await appReportML.AppGetAccountStatement(_filter);
                }
            }
            else
            {
                appReportResponse.Msg = appResp.Msg;
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "LedgerReport",
                CommonStr2 = JsonConvert.SerializeObject(appReportCommon),
                CommonStr3 = JsonConvert.SerializeObject(appReportResponse)
            });
            return Json(appReportResponse);
        }
        public async Task<IActionResult> GetASCBanks([FromBody] AppSessionReq appReportCommon)
        {
            var appReportResponse = new ASColBanksResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            var appRequest = new AppSessionReq
            {
                APPID = appReportCommon.APPID,
                IMEI = appReportCommon.IMEI,
                LoginTypeID = appReportCommon.LoginTypeID,
                UserID = appReportCommon.UserID,
                SessionID = appReportCommon.SessionID,
                RegKey = appReportCommon.RegKey,
                SerialNo = appReportCommon.SerialNo,
                Version = appReportCommon.Version,
                Session = appReportCommon.Session
            };
            var appResp = appML.CheckAppSession(appRequest);
            appReportResponse.IsAppValid = appResp.IsAppValid;
            appReportResponse.IsVersionValid = appResp.IsVersionValid;
            appReportResponse.IsPasswordExpired = appResp.IsPasswordExpired;
            appReportResponse.Statuscode = appResp.Statuscode;
            if (appResp.Statuscode == ErrorCodes.One)
            {
                if (!appResp.IsPasswordExpired)
                {
                    IAppReportML appReportML = new ReportML(_accessor, _env, false);

                    appReportResponse.Statuscode = ErrorCodes.One;
                    appReportResponse.Msg = ErrorCodes.SUCCESS;
                    appReportResponse.Banks = appReportML.GetASBanks(appReportCommon.UserID);
                }
            }
            else
            {
                appReportResponse.Msg = appResp.Msg;
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "GetASCBanks",
                CommonStr2 = JsonConvert.SerializeObject(appReportCommon),
                CommonStr3 = JsonConvert.SerializeObject(appReportResponse)
            });
            return Json(appReportResponse);
        }
        public async Task<IActionResult> ASPayCollect([FromBody] AppFosCollectionReq appReportCommon)
        {
            var appReportResponse = new AppResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            var appRequest = new AppSessionReq
            {
                APPID = appReportCommon.APPID,
                IMEI = appReportCommon.IMEI,
                LoginTypeID = appReportCommon.LoginTypeID,
                UserID = appReportCommon.UserID,
                SessionID = appReportCommon.SessionID,
                RegKey = appReportCommon.RegKey,
                SerialNo = appReportCommon.SerialNo,
                Version = appReportCommon.Version,
                Session = appReportCommon.Session
            };
            var appResp = appML.CheckAppSession(appRequest);
            appReportResponse.IsAppValid = appResp.IsAppValid;
            appReportResponse.IsVersionValid = appResp.IsVersionValid;
            appReportResponse.IsPasswordExpired = appResp.IsPasswordExpired;
            appReportResponse.Statuscode = appResp.Statuscode;
            if (appResp.Statuscode == ErrorCodes.One)
            {
                if (!appResp.IsPasswordExpired)
                {
                    IAppReportML appReportML = new ReportML(_accessor, _env, false);
                    var _filter = new ASCollectionReq
                    {
                        LoginID = appReportCommon.UserID,
                        UserID = appReportCommon.UID,
                        CollectionMode = appReportCommon.CollectionMode,
                        Amount = appReportCommon.Amount,
                        Remark = appReportCommon.Remark,
                        BankName = appReportCommon.BankName,
                        UTR = appReportCommon.UTR,
                        RequestMode = RequestMode.APPS
                    };
                    var _resp = appReportML.ASPaymentCollection(_filter);
                    if (_resp.Statuscode == ErrorCodes.One)
                    {
                        appReportResponse.Statuscode = ErrorCodes.One;
                        appReportResponse.Msg = _resp.Msg;
                    }
                    else
                    {
                        appReportResponse.Statuscode = ErrorCodes.Minus1;
                        appReportResponse.Msg = _resp.Msg;
                    }
                }
            }
            else
            {
                appReportResponse.Msg = appResp.Msg;
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "ASPayCollect",
                CommonStr2 = JsonConvert.SerializeObject(appReportCommon),
                CommonStr3 = JsonConvert.SerializeObject(appReportResponse)
            });
            return Json(appReportResponse);
        }
        [HttpPost]
        public IActionResult AppFOSRetailerList([FromBody] AppFOSUserReq appUserRequest)
        {
            var appResp = new AppReportResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (appUserRequest.LoginTypeID == LoginType.ApplicationUser)
            {
                var appResponse = appML.CheckAppSession(appUserRequest);
                appResp.Statuscode = appResponse.Statuscode;
                appResp.Msg = appResponse.Msg;
                appResp.IsPasswordExpired = appResponse.IsPasswordExpired;
                appResp.Statuscode = appResponse.Statuscode;
                if (appResp.Statuscode == ErrorCodes.One)
                {
                    if (!appResp.IsPasswordExpired)
                    {
                        IUserML userML = new UserML(_accessor, _env);
                        var f = new CommonFilter
                        {
                            LT = appUserRequest.LoginTypeID,
                            MobileNo = appUserRequest.MobileNo,
                            Name = appUserRequest.Name,
                            RoleID = appUserRequest.RoleID,
                            UserID = appUserRequest.UserID,
                            RequestMode = RequestMode.APPS,
                            TopRows = appUserRequest.TopRows

                        };
                        var res = userML.AppGetList(f);
                        appResp.FOSList = res;
                    }
                }
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "AppFOSRetailerList",
                CommonStr2 = JsonConvert.SerializeObject(appUserRequest),
                CommonStr3 = JsonConvert.SerializeObject(appResp)
            });
            return Json(appResp);
        }
        public async Task<IActionResult> AppGetAM([FromBody] AppSessionReq appReportCommon)
        {
            var appReportResponse = new AppAMResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            var appRequest = new AppSessionReq
            {
                APPID = appReportCommon.APPID,
                IMEI = appReportCommon.IMEI,
                LoginTypeID = appReportCommon.LoginTypeID,
                UserID = appReportCommon.UserID,
                SessionID = appReportCommon.SessionID,
                RegKey = appReportCommon.RegKey,
                SerialNo = appReportCommon.SerialNo,
                Version = appReportCommon.Version,
                Session = appReportCommon.Session
            };
            var appResp = appML.CheckAppSession(appRequest);
            appReportResponse.IsAppValid = appResp.IsAppValid;
            appReportResponse.IsVersionValid = appResp.IsVersionValid;
            appReportResponse.IsPasswordExpired = appResp.IsPasswordExpired;
            appReportResponse.Statuscode = appResp.Statuscode;
            if (appResp.Statuscode == ErrorCodes.One)
            {
                if (!appResp.IsPasswordExpired)
                {
                    appReportResponse.Msg = appResp.Msg;
                    appReportResponse.Statuscode = appResp.Statuscode;
                    IReportML ml = new ReportML(_accessor, _env);
                    appReportResponse.AreaMaster = ml.GetAreaMaster(appReportCommon.UserID);
                }
            }
            else
            {
                appReportResponse.Msg = appResp.Msg;
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "AppGetAM",
                CommonStr2 = JsonConvert.SerializeObject(appReportCommon),
                CommonStr3 = JsonConvert.SerializeObject(appReportResponse)
            });
            return Json(appReportResponse);
        }
        public async Task<IActionResult> GetASCollectBank([FromBody] AppSessionReq appReportCommon)
        {
            var appReportResponse = new AppASColBanks
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            var appRequest = new AppSessionReq
            {
                APPID = appReportCommon.APPID,
                IMEI = appReportCommon.IMEI,
                LoginTypeID = appReportCommon.LoginTypeID,
                UserID = appReportCommon.UserID,
                SessionID = appReportCommon.SessionID,
                RegKey = appReportCommon.RegKey,
                SerialNo = appReportCommon.SerialNo,
                Version = appReportCommon.Version,
                Session = appReportCommon.Session
            };
            var appResp = appML.CheckAppSession(appRequest);
            appReportResponse.IsAppValid = appResp.IsAppValid;
            appReportResponse.IsVersionValid = appResp.IsVersionValid;
            appReportResponse.IsPasswordExpired = appResp.IsPasswordExpired;
            appReportResponse.Statuscode = appResp.Statuscode;
            if (appResp.Statuscode == ErrorCodes.One)
            {
                if (!appResp.IsPasswordExpired)
                {
                    appReportResponse.Msg = appResp.Msg;
                    appReportResponse.Statuscode = appResp.Statuscode;
                    IReportML ml = new ReportML(_accessor, _env);
                    appReportResponse.Banks = ml.GetASBanks(appReportCommon.UserID);
                }
            }
            else
            {
                appReportResponse.Msg = appResp.Msg;
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "GetASCollectBank",
                CommonStr2 = JsonConvert.SerializeObject(appReportCommon),
                CommonStr3 = JsonConvert.SerializeObject(appReportResponse)
            });
            return Json(appReportResponse);
        }
        public async Task<IActionResult> MapUserToArea([FromBody] AppASLedgerReq appReportCommon)
        {
            var appReportResponse = new AppReportResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            var appRequest = new AppSessionReq
            {
                APPID = appReportCommon.APPID,
                IMEI = appReportCommon.IMEI,
                LoginTypeID = appReportCommon.LoginTypeID,
                UserID = appReportCommon.UserID,
                SessionID = appReportCommon.SessionID,
                RegKey = appReportCommon.RegKey,
                SerialNo = appReportCommon.SerialNo,
                Version = appReportCommon.Version,
                Session = appReportCommon.Session
            };
            var appResp = appML.CheckAppSession(appRequest);
            appReportResponse.IsAppValid = appResp.IsAppValid;
            appReportResponse.IsVersionValid = appResp.IsVersionValid;
            appReportResponse.IsPasswordExpired = appResp.IsPasswordExpired;
            appReportResponse.Statuscode = appResp.Statuscode;
            if (appResp.Statuscode == ErrorCodes.One)
            {
                if (!appResp.IsPasswordExpired)
                {
                    IReportML ml = new ReportML(_accessor, _env);
                    var res = ml.MapUserArea(new CommonReq
                    {
                        UserID = appReportCommon.UID,
                        CommonInt = appReportCommon.AreaID
                    });
                    appReportResponse.Statuscode = res.Statuscode;
                    appReportResponse.Msg = res.Msg;
                }
            }
            else
            {
                appReportResponse.Msg = appResp.Msg;
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "MapUserToArea",
                CommonStr2 = JsonConvert.SerializeObject(appReportCommon),
                CommonStr3 = JsonConvert.SerializeObject(appReportResponse)
            });
            return Json(appReportResponse);
        }
        #endregion
        #region RefferalContent
        public IActionResult GetAppRefferalContent([FromBody] AppSessionReq appSessionReq)
        {
            AppReportResponse appRechargeRespose = new AppReportResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            AppResponse appResp = appML.CheckAppSession(appSessionReq);
            appRechargeRespose.IsAppValid = appResp.IsAppValid;
            appRechargeRespose.IsVersionValid = appResp.IsVersionValid;
            appRechargeRespose.CheckID = appResp.CheckID;
            if (appResp.Statuscode == ErrorCodes.One)
            {
                IBannerML ml = new ResourceML(_accessor, _env);
                IWebsiteML websiteML = new WebsiteML(_accessor, _env);
                appRechargeRespose.RefferalContent = websiteML.GetAppWebsiteContent(appRechargeRespose.CheckID);
                appRechargeRespose.RefferalImage = ml.GetSavedRefferalImage(appRechargeRespose.CheckID.ToString());
                appRechargeRespose.Statuscode = ErrorCodes.One;
                appRechargeRespose.Msg = ErrorCodes.SUCCESS;
            }
            else
                appRechargeRespose.Msg = appResp.Msg;
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "GetRefferalBanner",
                CommonStr2 = JsonConvert.SerializeObject(appSessionReq),
                CommonStr3 = JsonConvert.SerializeObject(appRechargeRespose)
            });
            return Json(appRechargeRespose);
        }
        #endregion

        //
        //Summary:
        //  This region is used to Get Banklist and Agent Registration of Rdaddy and for further uses
        #region HelpingFunction
        [HttpPost]
        public IActionResult RDaddyIntAgentReg([FromBody] RDaddyIntARegReq req)
        {
            //{"FirstName": "","LastName": "","MobileNo": "","OutletName": "","Address": "","City": "","PinCode": "","PAN": "","Aadhar": ""}
            string resp = string.Empty;
            var userML = new UserML(_accessor, _env);
            var res = userML.RDaddyIntAgentReg(req);
            return Json(res);
        }
        [HttpPost]
        [Route("GetBankList")]
        public IActionResult GetBankList([FromBody] RDaddyIntARegReq req)
        {
            //{"FirstName": "","LastName": "","MobileNo": "","OutletName": "","Address": "","City": "","PinCode": "","PAN": "","Aadhar": ""}
            string resp = string.Empty;
            var userML = new UserML(_accessor, _env);
            var res = userML.RDaddyBankList(req);
            return Json(res);
        }
        #endregion
        [HttpPost]
        public IActionResult GetAccountOpeningBanner([FromBody] AppRechargeReportReq appRechargeReportReq)
        {

            var appRechargeRespose = new AppReportResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };

            var appResp = appML.CheckAppSession(appRechargeReportReq);
            appRechargeRespose.IsAppValid = appResp.IsAppValid;
            appRechargeRespose.IsVersionValid = appResp.IsVersionValid;
            appRechargeRespose.IsPasswordExpired = appResp.IsPasswordExpired;
            appRechargeRespose.Statuscode = appResp.Statuscode;
            if (appResp.Statuscode == ErrorCodes.One)
            {
                if (!appResp.IsPasswordExpired)
                {
                    IOperatorML opml = new OperatorML(_accessor, _env, false);

                    appRechargeRespose.Statuscode = ErrorCodes.One;
                    appRechargeRespose.Msg = ErrorCodes.SUCCESS;
                    appRechargeRespose.accountOpeningDeatils = opml.GetAccountOpeningRedirectionDataByOpType(new CommonReq
                    {
                        CommonInt = appRechargeReportReq.OpTypeID,
                        LoginID = appRechargeReportReq.UserID
                    });
                    appRechargeRespose.IsDrawOpImage = ApplicationSetting.IsDrawOpImage;
                }
            }
            else
            {
                appRechargeRespose.Msg = appResp.Msg;
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "GetAccountOpeningBanner",
                CommonStr2 = JsonConvert.SerializeObject(appRechargeReportReq),
                CommonStr3 = JsonConvert.SerializeObject(appRechargeRespose)
            });
            return Json(appRechargeRespose);
        }
        #region BulkQRGeneration
        public async Task<IActionResult> MapQRToUser([FromBody] AppMapQRToUser appMapQRToUser)
        {
            var appResponse = new AppResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            var appRequest = new AppSessionReq
            {
                APPID = appMapQRToUser.APPID,
                IMEI = appMapQRToUser.IMEI,
                LoginTypeID = appMapQRToUser.LoginTypeID,
                UserID = appMapQRToUser.UserID,
                SessionID = appMapQRToUser.SessionID,
                RegKey = appMapQRToUser.RegKey,
                SerialNo = appMapQRToUser.SerialNo,
                Version = appMapQRToUser.Version,
                Session = appMapQRToUser.Session
            };
            var appResp = appML.CheckAppSession(appRequest);
            appResponse.IsAppValid = appResp.IsAppValid;
            appResponse.IsVersionValid = appResp.IsVersionValid;
            appResponse.IsPasswordExpired = appResp.IsPasswordExpired;
            appResponse.Statuscode = appResp.Statuscode;
            if (appResp.Statuscode == ErrorCodes.One)
            {
                if (!appResp.IsPasswordExpired)
                {
                    IAppReportML appReportML = new ReportML(_accessor, _env, false);
                    var _qrD = appReportML.MapQRToUser(appMapQRToUser.QRIntent, appMapQRToUser.UserID, appMapQRToUser.LoginTypeID);
                    appResponse.Statuscode = _qrD.Statuscode;
                    appResponse.Msg = _qrD.Msg;
                }
            }
            else
            {
                appResponse.Msg = appResp.Msg;
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "MapQRToUser",
                CommonStr2 = JsonConvert.SerializeObject(appMapQRToUser),
                CommonStr3 = JsonConvert.SerializeObject(appResponse)
            });
            return Json(appResponse);
        }
        #endregion
        /* For Testing purpose only -- If in case of android request user-agent exists or not*/
        [HttpPost]
        public IActionResult chkUserAgent()
        {
            var mL = new LoginML(_accessor, _env, false);
            return Json(mL.returnUserAgent());
        }
        /*End*/

        [HttpPost]
        public IActionResult chkUserAgent1([FromForm]string str)
        {
            try
            {
                var mL = new LoginML(_accessor, _env, false);
                mL.saveBaseData(str);
                string imageName = Guid.NewGuid().ToString() + ".png";
                //set the image path
                string path = Path.Combine(Directory.GetCurrentDirectory(), "Image/Website/1/"); //Path
                if (!System.IO.Directory.Exists(path))
                {
                    System.IO.Directory.CreateDirectory(path); //Create directory if it doesn't exist
                }
                var data = Encoding.UTF8.GetBytes(str);
                var outputBytes = Encoding.Convert(Encoding.UTF8, Encoding.ASCII, data);
                var output = Encoding.ASCII.GetString(data);

                var data2 = System.Web.HttpUtility.UrlDecode(str);
                byte[] imageBytes1 = Convert.FromBase64String(data2);
                byte[] imageBytes2 = Convert.FromBase64String(output);
                byte[] imageBytes = Convert.FromBase64String(str);
                System.IO.File.WriteAllBytes(path + imageName, imageBytes);
                return Ok(imageBytes.ToString()); ;
            }
            catch (Exception ex)
            {
                return Ok(ex.ToString());
            }

        }

        [HttpPost]
        public async Task<IActionResult> GetLevel([FromBody] AppSessionReq appSessionReq)
        {
            var walletResponse = new WalletResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvaildSession
            };

            var appResp = appML.CheckAppSession(appSessionReq);
            walletResponse.IsAppValid = appResp.IsAppValid;
            walletResponse.IsVersionValid = appResp.IsVersionValid;
            walletResponse.IsPasswordExpired = appResp.IsPasswordExpired;
            walletResponse.Statuscode = appResp.Statuscode;
            if (appResp.Statuscode == ErrorCodes.One)
            {
                walletResponse.Statuscode = appResp.Statuscode;
                if (!appResp.IsPasswordExpired)
                {
                    IUserML ml = new UserML(_accessor, _env,false);
                    var res = await ml.GetLevel(appSessionReq.UserID);
                }
            }
            else
            {
                walletResponse.Msg = appResp.Msg;
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "GetWalletType",
                CommonStr2 = JsonConvert.SerializeObject(appSessionReq),
                CommonStr3 = JsonConvert.SerializeObject(walletResponse)
            });
            return Json(walletResponse);
        }
        [HttpPost]
        public async Task<IActionResult> GetTeam([FromBody] AppSessionReq appSessionReq)
        {
            var operatoOptionalsResponse = new OperatoOptionalsResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvaildSession
            };
            var appResp = appML.CheckAppSession(appSessionReq);
            operatoOptionalsResponse.IsAppValid = appResp.IsAppValid;
            operatoOptionalsResponse.IsVersionValid = appResp.IsVersionValid;
            operatoOptionalsResponse.IsPasswordExpired = appResp.IsPasswordExpired;
            operatoOptionalsResponse.Statuscode = appResp.Statuscode;
            if (appResp.Statuscode == ErrorCodes.One)
            {
                IUserML ml = new UserML(_accessor, _env, false);
                var res = await ml.GetTeam(appSessionReq.UserID);
            }
            else
            {
                operatoOptionalsResponse.Msg = appResp.Msg;
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "GetOperatorOptionals",
                CommonStr2 = JsonConvert.SerializeObject(appSessionReq),
                CommonStr3 = JsonConvert.SerializeObject(operatoOptionalsResponse)
            });
            return Json(operatoOptionalsResponse);
        }
    }
}