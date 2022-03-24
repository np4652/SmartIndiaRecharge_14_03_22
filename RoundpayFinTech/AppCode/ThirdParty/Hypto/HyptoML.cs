using Fintech.AppCode.DB;
using Fintech.AppCode.StaticModel;
using Fintech.AppCode.WebRequest;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RoundpayFinTech.AppCode.DL;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.IO;

namespace RoundpayFinTech.AppCode.ThirdParty.Hypto
{
    public class HyptoML: IVerificationAPI
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        private readonly IConfiguration Configuration;
        private readonly HyptoAppSetting appSetting;
        private readonly IDAL _dal;

        public HyptoML(IHttpContextAccessor accessor, IHostingEnvironment env, IDAL dal)
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
        private HyptoAppSetting AppSetting()
        {
            var setting = new HyptoAppSetting();
            try
            {
                setting = new HyptoAppSetting
                {
                    BaseURL = Configuration["VERIFICATION:HYPTO:BaseURL"],
                    Auth = Configuration["VERIFICATION:HYPTO:Auth"]
                };
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "HyptoAppSetting",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser
                });
            }
            return setting;
        }

        public VerificationServiceRes VerifyAccount(VericationServiceReq serviceReq)
        {
            var res = new VerificationServiceRes
            {
                Statuscode=ErrorCodes.One,
                AccountNo= serviceReq.AccountNo,
                ErrorCode=ErrorCodes.Request_Accpeted
            };
            try
            {
                var param = new
                {
                    number = serviceReq.AccountNo,
                    ifsc = serviceReq.IFSC,
                    reference_number = "TID"+serviceReq.TID,
                    Authorization = appSetting.Auth
                };
                res.Req = appSetting.BaseURL+ "verify/bank_account?RequestJson=" + JsonConvert.SerializeObject(param); ;
                res.Resp= AppWebRequest.O.PostJsonDataUsingHWRTLS(appSetting.BaseURL + "verify/bank_account", param, new Dictionary<string, string>
                {
                    { "Authorization",param.Authorization},
                    { ContentType.Self,ContentType.application_json}
                }).Result;
                var _apiRes = JsonConvert.DeserializeObject<HyptoResponseModel>(res.Resp);
                if (_apiRes != null)
                {
                    if (_apiRes.success)
                    {
                        if (_apiRes.data != null)
                        {
                            res.VendorID = _apiRes.data.reference_number;
                            if (_apiRes.data.status.Equals("COMPLETED"))
                            {
                                res.Statuscode = RechargeRespType.SUCCESS;
                                res.Msg = _apiRes.message;
                                res.AccountHolder = _apiRes.data.verify_account_holder ?? string.Empty;
                                res.LiveID = _apiRes.data.id.ToString();
                                res.ErrorCode = ErrorCodes.Transaction_Successful;
                            }
                            else if (_apiRes.data.status.Equals("FAILED"))
                            {
                                res.Statuscode = RechargeRespType.FAILED;
                                res.Msg = _apiRes.data.verify_reason ?? string.Empty;
                                res.Msg = res.Msg.Contains("suff") ? nameof(DMTErrorCodes.Declined_by_ServiceProvider).Replace("_", " ") : res.Msg;
                                res.LiveID = res.Msg;
                            }
                            else
                            {
                                res.Statuscode = RechargeRespType.PENDING;
                                res.Msg = nameof(ErrorCodes.Request_Accpeted).Replace("_", " ");
                                res.ErrorCode = ErrorCodes.Request_Accpeted;
                                res.LiveID = res.Msg;
                            }
                        }
                        else
                        {
                            res.Statuscode = RechargeRespType.PENDING;
                            res.Msg = _apiRes.message;
                        }
                    }
                    else
                    {
                        res.Statuscode = RechargeRespType.FAILED;
                        res.Msg = _apiRes.message;
                        res.LiveID = _apiRes.message;
                        if (res.LiveID.ToLower().Contains("nsuffici"))
                        {
                            res.Statuscode = RechargeRespType.PENDING;
                            res.Msg = nameof(ErrorCodes.Request_Accpeted).Replace("_", " ");
                            res.ErrorCode = ErrorCodes.Request_Accpeted;
                            res.LiveID = res.Msg;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                res.Resp = ex.Message + "|" + res.Resp;
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "VerifyAccount",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = 0
                });
            }
            return res;
        }

        public VerificationServiceRes VerifyUPIID(VericationServiceReq serviceReq)
        {
            var res = new VerificationServiceRes
            {
                Statuscode = ErrorCodes.One,
                AccountNo = serviceReq.AccountNo,
                ErrorCode = ErrorCodes.Request_Accpeted
            };
            try
            {
                var param = new
                {
                    upi_id = serviceReq.AccountNo,
                    reference_number = "TID" + serviceReq.TID,
                    Authorization = appSetting.Auth
                };
                res.Req = appSetting.BaseURL + "verify/upi_id?RequestJson=" + JsonConvert.SerializeObject(param);
                res.Resp = AppWebRequest.O.PostJsonDataUsingHWRTLS(appSetting.BaseURL + "verify/upi_id", param, new Dictionary<string, string>
                {
                    { "Authorization",param.Authorization},
                    { ContentType.Self,ContentType.application_json}
                }).Result;
                var _apiRes = JsonConvert.DeserializeObject<HyptoResponseModel>(res.Resp);
                if (_apiRes != null)
                {
                    if (_apiRes.success)
                    {
                        if (_apiRes.data != null)
                        {
                            res.VendorID = _apiRes.data.reference_number;
                            if (_apiRes.data.status.Equals("COMPLETED"))
                            {
                                res.Statuscode = RechargeRespType.SUCCESS;
                                res.Msg = _apiRes.message;
                                res.AccountHolder = _apiRes.data.verify_upi_id_holder ?? string.Empty;
                                res.LiveID = _apiRes.data.bank_ref_num.ToString();
                                res.VendorID = _apiRes.data.id.ToString();
                                res.ErrorCode = ErrorCodes.Transaction_Successful;
                            }
                            else if (_apiRes.data.status.Equals("FAILED"))
                            {
                                res.Statuscode = RechargeRespType.FAILED;
                                res.Msg = _apiRes.data.verify_reason ?? string.Empty;
                                res.Msg = res.Msg.Contains("suff") ? nameof(DMTErrorCodes.Declined_by_ServiceProvider).Replace("_", " ") : res.Msg;
                                res.LiveID = res.Msg;
                            }
                            else
                            {
                                res.Statuscode = RechargeRespType.PENDING;
                                res.Msg = nameof(ErrorCodes.Request_Accpeted).Replace("_", " ");
                                res.ErrorCode = ErrorCodes.Request_Accpeted;
                                res.LiveID = res.Msg;
                            }
                        }
                        else
                        {
                            res.Statuscode = RechargeRespType.PENDING;
                            res.Msg = _apiRes.message;
                        }
                    }
                    else
                    {
                        res.Statuscode = RechargeRespType.FAILED;
                        res.Msg = _apiRes.message;
                        res.LiveID = _apiRes.message;
                        if (res.LiveID.ToLower().Contains("nsuffici"))
                        {
                            res.Statuscode = RechargeRespType.PENDING;
                            res.Msg = nameof(ErrorCodes.Request_Accpeted).Replace("_", " ");
                            res.ErrorCode = ErrorCodes.Request_Accpeted;
                            res.LiveID = res.Msg;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                res.Resp = ex.Message + "|" + res.Resp;
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "VerifyUPIID",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = 0
                });
            }
            return res;
        }
    }
}
