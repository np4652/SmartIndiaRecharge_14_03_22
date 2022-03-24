using Fintech.AppCode;
using Fintech.AppCode.DB;
using Fintech.AppCode.HelperClass;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using Fintech.AppCode.WebRequest;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using RoundpayFinTech.AppCode.DL;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.MiddleLayer;
using RoundpayFinTech.AppCode.Model.BBPS;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.Model.Recharge;
using RoundpayFinTech.AppCode.ThirdParty.CyberPlate;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Security.Claims;
using System.Text;

namespace RoundpayFinTech
{
    public partial class SprintBBPSML
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        private readonly IConfiguration Configuration;
        private readonly IConnectionConfiguration _c;
        private readonly SprintJsonSettings appSetting;
        private readonly IDAL _dal;
        private string _JWTToken = string.Empty;

        public SprintBBPSML(IHttpContextAccessor accessor, IHostingEnvironment env, IDAL dal)
        {
            _accessor = accessor;
            _env = env;
            var builder = new ConfigurationBuilder()
                 .SetBasePath(Directory.GetCurrentDirectory());
            builder.AddJsonFile((_env.IsProduction() ? "appsettings.json" : "appsettings.Development.json"));
            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
            _dal = dal;
            appSetting = AppSetting();
        }
        public SprintJsonSettings GetSetting() {
            return appSetting;
        }
        private SprintJsonSettings AppSetting()
        {
            try
            {
                return new SprintJsonSettings
                {
                    AEPSBaseURL = Configuration["DMR:SPRINT:AEPSBaseURL"],
                    OnboardURL = Configuration["DMR:SPRINT:OnboardURL"],
                    FetchBillURL = Configuration["DMR:SPRINT:FetchBillURL"],
                    PayBillURL = Configuration["DMR:SPRINT:PayBillURL"],
                    StatusCheckURL = Configuration["DMR:SPRINT:StatusCheckURL"],
                    UserName = Configuration["DMR:SPRINT:UserName"],
                    Password = Configuration["DMR:SPRINT:Password"],
                    PartnerID = Configuration["DMR:SPRINT:PartnerID"],
                    JWTKey = Configuration["DMR:SPRINT:JWTKey"],
                    Authorisedkey = Configuration["DMR:SPRINT:Authorisedkey"],
                    IV = Configuration["DMR:SPRINT:IV"],
                    Key = Configuration["DMR:SPRINT:Key"]
                };
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "SprintJsonSettings",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser
                });
            }
            return new SprintJsonSettings();
        }

        public bool IsParnerIDIsValid(string PartnerID)
        {
            return (appSetting.PartnerID??string.Empty)== (PartnerID??string.Empty) && !string.IsNullOrEmpty(appSetting.PartnerID);
        }
        private void TokenGeneration()
        {
            long unixSeconds = DateTimeOffset.Now.ToUnixTimeSeconds();
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(appSetting.JWTKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var claims = new[] {
                new Claim("timestamp", unixSeconds.ToString()),
                new Claim("partnerId", appSetting.PartnerID),
                new Claim("reqid", Guid.NewGuid().ToString().Replace("-",""))
            };
            var token = new JwtSecurityToken(null, null, claims, null, signingCredentials: credentials);
            _JWTToken = new JwtSecurityTokenHandler().WriteToken(token).ToString();
        }
        public string DecryptToken(string encData)
        {
            if (!string.IsNullOrEmpty(encData))
            {
                try
                {
                    var handler = new JwtSecurityTokenHandler();
                    var jsonToken = handler.ReadToken(encData);
                    var resp = jsonToken as JwtSecurityToken;
                    return JsonConvert.SerializeObject(resp);
                }
                catch (Exception)
                {
                }
            }
            return string.Empty;
        }

        public GenerateOTPModel GetCallbackURL(ValidateAPIOutletResp _ValidateAPIOutletResp)
        {
            var res = new GenerateOTPModel
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            TokenGeneration();
            var headers = new Dictionary<string, string>
            {
                { "Token", _JWTToken},
                { "Authorisedkey", appSetting.Authorisedkey}
            };
            ILoginML loginML = new LoginML(_accessor, _env, false);
            var _WInfo = loginML.GetWebsiteInfo();
            var domain = _WInfo.WID == 1 ? _WInfo.AbsoluteHost : _WInfo.MainDomain;
            //domain = "http://rezopay.com";
            var req = new
            {
                merchantcode = _ValidateAPIOutletResp.OutletID,
                mobile = _ValidateAPIOutletResp.MobileNo,
                is_new = "0",
                email = _ValidateAPIOutletResp.EmailID,
                firm = _ValidateAPIOutletResp.OutletName,
                callback = domain + "/Callback/PaySprintOnboard"
            };
            var response = string.Empty;
            var request = appSetting.OnboardURL + JsonConvert.SerializeObject(headers) + JsonConvert.SerializeObject(req);
            try
            {
                response = AppWebRequest.O.PostJsonDataUsingHWRTLS(appSetting.OnboardURL, req, headers).Result;

                if (!string.IsNullOrEmpty(response))
                {
                    var apiResp = JsonConvert.DeserializeObject<SpringOnboardGetOnboardURLModel>(response);
                    if (apiResp.response_code == 1)
                    {
                        res.IsRedirection = true;
                        res.RedirectURL = apiResp.redirecturl;
                        res.Statuscode = ErrorCodes.One;
                        res.Msg = ErrorCodes.SUCCESS;
                        res.ErrorCode = ErrorCodes.Transaction_Successful;
                    }
                    else
                    {
                        res.Msg = apiResp.message;
                    }
                }
            }
            catch (Exception ex)
            {

                response = ex.Message + "|" + response;
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "SPRINT GetCallbackURL",
                    Error = response,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = 1
                });
            }
            #region APILogOutletRegistration
            new ProcUpdateLogOnboardingReqResp(_dal).Call(new OnboardingLog
            {
                APIID = _ValidateAPIOutletResp.APIID,
                Method = "SPRINT.GetCallbackURL",
                Request = request,
                Response = response,

            });
            #endregion
            return res;

        }
        public BBPSResponse FetchBill(BBPSLog bbpsLog)
        {
            TokenGeneration();
            var billResponse = new BBPSResponse
            {
                IsEditable = false,
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.URLNOTFOUND,
                ErrorCode = ErrorCodes.Unknown_Error.ToString(),
                ErrorMsg = ErrorCodes.URLNOTFOUND,
                IsEnablePayment = false
            };
            var headers = new Dictionary<string, string>
            {
                { "Token", _JWTToken}
                //{ "Authorisedkey", appSetting.Authorisedkey}
            };

            var deviceDetailsAGT = new SprintFetchBill
            {
                @operator = bbpsLog.aPIDetail.APIOpCode,
                canumber = bbpsLog.AccountNumber,
                mode = bbpsLog.aPIDetail.RefferenceKey,
                ad1 = bbpsLog.Optional1,
                ad2 = bbpsLog.Optional2,
                ad3 = bbpsLog.Optional3,
                ad4 = bbpsLog.Optional4
            };

            var response = string.Empty;
            var request = appSetting.FetchBillURL + "?" + JsonConvert.SerializeObject(deviceDetailsAGT) + "|" + JsonConvert.SerializeObject(headers);
            bbpsLog.Request = request;
            try
            {
                response = AppWebRequest.O.PostJsonDataUsingHWRTLS(appSetting.FetchBillURL, deviceDetailsAGT, headers).Result;
                if (!string.IsNullOrEmpty(response))
                {
                    var bResp = JsonConvert.DeserializeObject<SprintFetchBillResp>(response);
                    if (bResp != null)
                    {
                        if (bResp.response_code == 1)
                        {
                            bbpsLog.helper.Status = RechargeRespType.SUCCESS;
                            billResponse.Statuscode = ErrorCodes.One;
                            billResponse.ErrorCode = ErrorCodes.Transaction_Successful.ToString();
                            billResponse.ErrorMsg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                            billResponse.Msg = bResp.message;
                            billResponse.IsEnablePayment = true;
                            billResponse.BillNumber = bbpsLog.AccountNumber;
                            billResponse.DueDate = bResp.duedate;
                            billResponse.Amount = bResp.amount;
                            billResponse.CustomerName = bResp.name;
                        }
                        else
                        {
                            bbpsLog.helper.Status = RechargeRespType.FAILED;
                            billResponse.IsShowMsgOnly = true;
                            IErrorCodeML errorCodeML = new ErrorCodeML(_accessor, _env, false);
                            var eFromDB = errorCodeML.GetAPIErrorCodeDescription(bbpsLog.aPIDetail.GroupCode, bResp.response_code.ToString());
                            billResponse.ErrorMsg = billResponse.Msg = bbpsLog.helper.Reason = (eFromDB.Error ?? string.Empty).Replace("{AccountKey}", bbpsLog.APIReqHelper.AccountNoKey);
                            billResponse.ErrorCode = eFromDB.Code;
                            billResponse.IsShowMsgOnly = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                response = ex.Message + "|" + response;
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "FetchBill",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = 1
                });
                billResponse.IsShowMsgOnly = true;
            }
            bbpsLog.Response = response;
            return billResponse;
        }


        public string BillPayment(SPPAYBillReq bBPSRequest, TransactionHelper objTransactionHelper, RechargeAPIHit rechargeAPIHit)
        {
            TokenGeneration();
            var returnResp = string.Empty;
            var resp = string.Empty;
            try
            {
                var headers = new Dictionary<string, string>
                {
                    { "Token", _JWTToken}
                    //{ "Authorisedkey", appSetting.Authorisedkey}
                };

                resp = AppWebRequest.O.PostJsonDataUsingHWRTLS(appSetting.PayBillURL, bBPSRequest, headers).Result;

                if (!string.IsNullOrEmpty(resp))
                {
                    returnResp = resp;
                }
                rechargeAPIHit.aPIDetail.URL = appSetting.PayBillURL + "|" + JsonConvert.SerializeObject(headers) + "|" + JsonConvert.SerializeObject(bBPSRequest);
            }
            catch (Exception ex)
            {
                returnResp = ex.Message + "|" + resp;
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Sprint BillPayment",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = 1
                });
            }

            rechargeAPIHit.Response = returnResp;
            //objTransactionHelper.UpdateAPIResponse(Convert.ToInt32(bBPSRequest.referenceid), rechargeAPIHit);
            return returnResp;
        }

        public HitRequestResponseModel BillPaymentStatus(string _tid)
        {
            TokenGeneration();

            var returnResp = new HitRequestResponseModel();
            var headers = new Dictionary<string, string>
            {
                { "Token", _JWTToken}
                //{ "Authorisedkey", appSetting.Authorisedkey}
            };
            var req = new
            {
                referenceid = _tid
            };

            string response = string.Empty;

            returnResp.Request = appSetting.StatusCheckURL + "?" + JsonConvert.SerializeObject(req) + "|" + JsonConvert.SerializeObject(headers);
            try
            {
                response = AppWebRequest.O.PostJsonDataUsingHWRTLS(appSetting.StatusCheckURL, req, headers).Result;

                if (string.IsNullOrEmpty(response))
                {
                    returnResp.Response = response;
                }
            }
            catch (Exception ex)
            {
                response = ex.Message + "|" + response;
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "SPRINT BillPaymentStatus",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = 1
                });
            }
            returnResp.Response = response;
            return returnResp;
        }
    }
}
