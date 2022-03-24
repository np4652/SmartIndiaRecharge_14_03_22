using Fintech.AppCode.DB;
using Fintech.AppCode.StaticModel;
using Fintech.AppCode.WebRequest;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RoundpayFinTech.AppCode.DL;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.ThirdParty.Roundpay;
using System;
using System.IO;
using System.Text;
using Validators;

namespace RoundpayFinTech
{
    public class CashPointIndiaML
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        private readonly IConfiguration Configuration;
        private CashPointIndiaAppSetting appSetting;
        private readonly IDAL _dal;
        public CashPointIndiaML(IHttpContextAccessor accessor, IHostingEnvironment env, IDAL dal)
        {
            _accessor = accessor;
            _env = env;
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory());
            builder.AddJsonFile((_env.IsProduction() ? "appsettings.json" : "appsettings.Development.json"));
            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
            appSetting = AppSetting();
            _dal = dal;
        }
        private CashPointIndiaAppSetting AppSetting()
        {
            var setting = new CashPointIndiaAppSetting();
            try
            {
                setting = new CashPointIndiaAppSetting
                {
                    api_key = Configuration["SERVICESETTING:CPIPAN:api_key"],
                    BaseURL = Configuration["SERVICESETTING:CPIPAN:BaseURL"]
                };
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "PANMitraAppSetting:PNMTRA",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser
                });
            }
            return setting;
        }

        public OutletAPIStatusUpdate VLEIDCreate(CashPointIndiaOnboardRequest onboardRequest, int APIID, int OutletID)
        {
            string Req = string.Empty;
            string Resp = string.Empty;
            var OutReqRes = new OutletAPIStatusUpdate
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            onboardRequest.api_key = appSetting.api_key;
            try
            {

                StringBuilder sbvData = new StringBuilder("api_key={api_key}&vle_id={vle_id}&vle_name={vle_name}&vle_mob={vle_mob}&vle_email={vle_email}&vle_shop={vle_shop}&vle_loc={vle_loc}&vle_state={vle_state}&vle_pin={vle_pin}&vle_uid={vle_uid}&vle_pan={vle_pan}");
                sbvData.Replace("{api_key}", onboardRequest.api_key);
                sbvData.Replace("{vle_id}", onboardRequest.vle_id);
                sbvData.Replace("{vle_name}", onboardRequest.vle_name);
                sbvData.Replace("{vle_mob}", onboardRequest.vle_mob);
                sbvData.Replace("{vle_email}", onboardRequest.vle_email);
                sbvData.Replace("{vle_shop}", onboardRequest.vle_shop);
                sbvData.Replace("{vle_loc}", onboardRequest.vle_loc);
                sbvData.Replace("{vle_state}", onboardRequest.vle_state);
                sbvData.Replace("{vle_pin}", onboardRequest.vle_pin);
                sbvData.Replace("{vle_uid}", onboardRequest.vle_uid);
                sbvData.Replace("{vle_pan}", onboardRequest.vle_pan);
                Req = appSetting.BaseURL + "add_vle.php?" + sbvData.ToString();
                Resp = AppWebRequest.O.CallUsingHttpWebRequest_GET(Req);
                if (Validate.O.ValidateJSON(Resp ?? string.Empty))
                {
                    var _apiRes = JsonConvert.DeserializeObject<CashPointIndiaResponse>(Resp);
                    if (_apiRes != null)
                    {
                        if (_apiRes.status == RechargeRespType._SUCCESS)
                        {
                            OutReqRes.Statuscode = ErrorCodes.One;
                            OutReqRes.Msg = ErrorCodes.OutletRegistered;
                            OutReqRes.APIOutletID = OutletID.ToString();
                            OutReqRes.APIOutletStatus = RndKYCOutLetStatus._Approved;
                            OutReqRes.KYCStatus = RndKYCOutLetStatus._Approved;
                            OutReqRes.BBPSID = OutletID.ToString();
                            OutReqRes.BBPSStatus = RndKYCOutLetStatus._NotApplied;
                            OutReqRes.AEPSID = OutletID.ToString();
                            OutReqRes.AEPSStatus = RndKYCOutLetStatus._NotApplied;
                            OutReqRes.DMTID = OutletID.ToString();
                            OutReqRes.DMTStatus = RndKYCOutLetStatus._NotApplied;
                            OutReqRes.PSAID = _apiRes.vle_id;
                            OutReqRes.PSAStatus = RndKYCOutLetStatus._Pending;
                        }
                        else
                        {
                            OutReqRes.APIOutletStatus = RndKYCOutLetStatus._Rejected;
                            OutReqRes.Msg = string.IsNullOrEmpty(_apiRes.message) ? ErrorCodes.FailedToSubmit : _apiRes.message;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "VLEIDCreate",
                    Error = ex.Message,
                });
                Resp = ex.Message + "_" + Resp;
                OutReqRes.Msg = ErrorCodes.TempError;
            }
            #region APILogOutletRegistration
            new ProcUpdateLogOnboardingReqResp(_dal).Call(new OnboardingLog
            {
                APIID = APIID,
                Method = "VLEIDCreate",
                Request = Req,
                Response = Resp
            });
            #endregion
            return OutReqRes;
        }

        public OutletAPIStatusUpdate VLEIDStatus(CashPointIndiaOnboardRequest onboardRequest, int APIID, int OutletID)
        {
            string Req = string.Empty;
            string Resp = string.Empty;
            var OutReqRes = new OutletAPIStatusUpdate
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };

            onboardRequest.api_key = appSetting.api_key;
            try
            {

                StringBuilder sbvData = new StringBuilder("api_key={api_key}&vle_id={vle_id}");
                sbvData.Replace("{api_key}", onboardRequest.api_key);
                sbvData.Replace("{vle_id}", onboardRequest.vle_id);
                Req = appSetting.BaseURL + "vle_status.php?" + sbvData.ToString();
                Resp = AppWebRequest.O.CallUsingHttpWebRequest_GET(Req);
                if (Validate.O.ValidateJSON(Resp ?? string.Empty))
                {
                    var _apiRes = JsonConvert.DeserializeObject<CashPointIndiaResponse>(Resp);
                    if (_apiRes != null)
                    {
                        if (_apiRes.status == RechargeRespType._SUCCESS)
                        {
                            OutReqRes.Statuscode = ErrorCodes.One;
                            OutReqRes.Msg = ErrorCodes.OutletRegistered;
                            OutReqRes.APIOutletID = OutletID.ToString();
                            OutReqRes.APIOutletStatus = RndKYCOutLetStatus._Approved;
                            OutReqRes.KYCStatus = RndKYCOutLetStatus._Approved;
                            OutReqRes.BBPSID = OutletID.ToString();
                            OutReqRes.BBPSStatus = RndKYCOutLetStatus._NotApplied;
                            OutReqRes.AEPSID = OutletID.ToString();
                            OutReqRes.AEPSStatus = RndKYCOutLetStatus._NotApplied;
                            OutReqRes.DMTID = OutletID.ToString();
                            OutReqRes.DMTStatus = RndKYCOutLetStatus._NotApplied;
                            OutReqRes.PSAID = _apiRes.vle_id;
                            if (_apiRes.vle_status == "APPROVED")
                            {
                                OutReqRes.PSAStatus = RndKYCOutLetStatus._Approved;
                            }
                            else if (_apiRes.vle_status == "PENDING")
                            {
                                OutReqRes.PSAStatus = RndKYCOutLetStatus._Pending;
                            }
                            else
                            {
                                OutReqRes.PSAStatus = RndKYCOutLetStatus._Rejected;
                            }
                        }
                        else
                        {
                            OutReqRes.APIOutletStatus = RndKYCOutLetStatus._Rejected;
                            OutReqRes.BBPSStatus = RndKYCOutLetStatus._Rejected;
                            OutReqRes.AEPSStatus = RndKYCOutLetStatus._Rejected;
                            OutReqRes.DMTStatus = RndKYCOutLetStatus._Rejected;
                            OutReqRes.Msg = string.IsNullOrEmpty(_apiRes.message) ? ErrorCodes.FailedToSubmit : _apiRes.message;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "VLEIDStatus",
                    Error = ex.Message,
                });
                Resp = ex.Message + "_" + Resp;
                OutReqRes.Msg = ErrorCodes.TempError;
            }
            #region APILogOutletRegistration
            new ProcUpdateLogOnboardingReqResp(_dal).Call(new OnboardingLog
            {
                APIID = APIID,
                Method = "VLEIDStatus",
                Request = Req,
                Response = Resp,

            });
            #endregion
            return OutReqRes;
        }

        public PSAResponse CouponRequestAPI(CashPointIndiaRequest mitraRequest)
        {
            string Resp = string.Empty;
            var res = new PSAResponse
            {
                Status = RechargeRespType.PENDING,
                Msg = ErrorCodes.NORESPONSE
            };
            mitraRequest.api_key = appSetting.api_key;
            try
            {

                StringBuilder sbvData = new StringBuilder("api_key={api_key}&vle_id={vle_id}&type={type}&qty={qty}");
                sbvData.Replace("{api_key}", mitraRequest.api_key);
                sbvData.Replace("{vle_id}", mitraRequest.vle_id);
                sbvData.Replace("{type}", mitraRequest.type);
                sbvData.Replace("{qty}", mitraRequest.qty);
                res.Req = appSetting.BaseURL + "coupon_req.php?" + sbvData.ToString();
                Resp = AppWebRequest.O.CallUsingHttpWebRequest_GET(res.Req);
                if (Validate.O.ValidateJSON(Resp ?? string.Empty))
                {
                    var _apiRes = JsonConvert.DeserializeObject<CashPointIndiaResponse>(Resp);
                    if (_apiRes != null)
                    {
                        res.LiveID = _apiRes.order_id;
                        res.VendorID = _apiRes.order_id;
                        if (_apiRes.status == RechargeRespType._SUCCESS)
                        {
                            res.Status = RechargeRespType.SUCCESS;
                            res.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                            res.ErrorCode = ErrorCodes.Transaction_Successful;
                        }
                        else if (_apiRes.status == RechargeRespType._FAILED)
                        {
                            res.Status = RechargeRespType.FAILED;
                            res.Msg = _apiRes.message;
                            res.ErrorCode = ErrorCodes.Unknown_Error;
                        }
                        else
                        {
                            res.Msg = nameof(ErrorCodes.Request_Accpeted).Replace("_", " ");
                            res.ErrorCode = ErrorCodes.Request_Accpeted;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "CouponRequestAPI",
                    Error = ex.Message,
                });
                Resp = ex.Message + "_" + Resp;
            }
            res.Resp = Resp;
            return res;
        }

        public PSAResponse CouponStatusAPI(CashPointIndiaRequest mitraRequest)
        {
            string Resp = string.Empty;
            var res = new PSAResponse
            {
                Status = RechargeRespType.PENDING,
                Msg = ErrorCodes.NORESPONSE
            };
            mitraRequest.api_key = appSetting.api_key;
            try
            {

                StringBuilder sbvData = new StringBuilder("api_key={api_key}&order_id={order_id}");
                sbvData.Replace("{api_key}", mitraRequest.api_key);
                sbvData.Replace("{order_id}", mitraRequest.order_id);
                res.Req = appSetting.BaseURL + "coupon_status.php?" + sbvData.ToString();
                Resp = AppWebRequest.O.CallUsingHttpWebRequest_GET(res.Req);
                if (Validate.O.ValidateJSON(Resp ?? string.Empty))
                {
                    var _apiRes = JsonConvert.DeserializeObject<CashPointIndiaResponse>(Resp);
                    if (_apiRes != null)
                    {
                        res.LiveID = _apiRes.order_id;
                        res.VendorID = _apiRes.order_id;
                        if (_apiRes.status == RechargeRespType._SUCCESS)
                        {
                            res.Status = RechargeRespType.SUCCESS;
                            res.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                            res.ErrorCode = ErrorCodes.Transaction_Successful;
                        }
                        else if (_apiRes.status == RechargeRespType._FAILED)
                        {
                            res.Status = RechargeRespType.FAILED;
                            res.Msg = _apiRes.message;
                            res.ErrorCode = ErrorCodes.Unknown_Error;
                        }
                        else
                        {
                            res.Msg = nameof(ErrorCodes.Request_Accpeted).Replace("_", " ");
                            res.ErrorCode = ErrorCodes.Request_Accpeted;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "CouponRequestAPI",
                    Error = ex.Message,
                });
                Resp = ex.Message + "_" + Resp;
            }
            res.Resp = Resp;
            return res;
        }

        public void BalanceCheck(APIBalanceResponse res)
        {
            string Resp = string.Empty;
            try
            {

                StringBuilder sbvData = new StringBuilder("api_key={api_key}");
                sbvData.Replace("{api_key}", appSetting.api_key);
                res.Request = appSetting.BaseURL + "balance.php?" + sbvData.ToString();
                Resp = AppWebRequest.O.CallUsingHttpWebRequest_GET(res.Request);
                if (Validate.O.ValidateJSON(Resp ?? string.Empty))
                {
                    var _apiRes = JsonConvert.DeserializeObject<CashPointIndiaResponse>(Resp);
                    if (_apiRes != null)
                    {
                        res.Balance = string.IsNullOrEmpty(_apiRes.balance)?Convert.ToDecimal(_apiRes.balance):0;
                    }
                }
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "BalanceCheck",
                    Error = ex.Message,
                });
                Resp = ex.Message + "_" + Resp;
            }
            res.Response = Resp;
        }
    }
}
