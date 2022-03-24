using Fintech.AppCode.HelperClass;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.MiddleLayer;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model.ProcModel;
using Newtonsoft.Json;
using System;
using System.IO;
using RoundpayFinTech.AppCode.Model.Recharge;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Configuration;

namespace RoundpayFinTech.Controllers
{
    public partial class APIController : Controller, ITransactionService
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        private readonly IAPIUserMiddleLayer _apiML;
        public APIController(IHttpContextAccessor accessor, IHostingEnvironment env)
        {
            _accessor = accessor;
            _env = env;
            _apiML = new APIUserML(_accessor, _env);
        }
        #region TransactionSerivce
        public async Task<IActionResult> Balance(APIRequest req)
        {
            var resS = new StatusMsg
            {
                MSG = ErrorCodes.InvalidParam,
                ERRORCODE = ErrorCodes.Invalid_Parameter.ToString()
            };
            string respp = string.Empty;
            try
            {
                if (_apiML.BolckSameRequest("BAL", JsonConvert.SerializeObject(req)))
                {
                    StatusMsg res = await _apiML.GetBalance(req).ConfigureAwait(false);
                    if (req.Format == ResponseType.XML)
                    {
                        string resp = XMLHelper.O.SerializeToXml(res, "TransactionResponse");
                        await SaveReqResp(ServiceType.Recharge, resp).ConfigureAwait(false);
                        return Content(resp);
                    }
                    else
                    {
                        string resp = JsonConvert.SerializeObject(res);
                        await SaveReqResp(ServiceType.Recharge, resp).ConfigureAwait(false);
                        return Json(res);
                    }
                }
                resS.MSG = "Blocked";
                resS.ERRORCODE = ErrorCodes.Unknown_Error.ToString();
            }
            catch (Exception ex)
            {
                resS.STATUS = RechargeRespType.FAILED;
                resS.MSG = ErrorCodes.InvalidParam;
                resS.ERRORCODE = ErrorCodes.Invalid_Parameter.ToString();
                respp = " Exception:" + ex.Message;
            }
            respp = JsonConvert.SerializeObject(resS) + respp;
            await SaveReqResp(ServiceType.Recharge, respp).ConfigureAwait(false);
            return BadRequest(resS);
        }
        public async Task<IActionResult> StatusCheck(StatusAPIRequest req)
        {
            var resS = new StatusMsg
            {
                MSG = ErrorCodes.InvalidParam,
                ERRORCODE = ErrorCodes.Invalid_Parameter.ToString()
            };
            var respp = string.Empty;
            try
            {
                if (_apiML.BolckSameRequest("STSCHK", JsonConvert.SerializeObject(req)))
                {
                    var res = await _apiML.GetStatusCheck(req).ConfigureAwait(false);
                    if (req.Format == ResponseType.XML)
                    {
                        string resp = XMLHelper.O.SerializeToXml(res, "TransactionResponse");
                        await SaveReqResp(ServiceType.Recharge, resp).ConfigureAwait(false);
                        return Content(resp);
                    }
                    else
                    {
                        string resp = JsonConvert.SerializeObject(res);
                        await SaveReqResp(ServiceType.Recharge, resp).ConfigureAwait(false);
                        return Json(res);
                    }
                }
                resS.MSG = "Blocked";
                resS.ERRORCODE = ErrorCodes.Unknown_Error.ToString();
            }
            catch (Exception ex)
            {
                respp = " Exception:" + ex.Message;
            }
            respp = JsonConvert.SerializeObject(resS) + respp;
            await SaveReqResp(ServiceType.Recharge, respp).ConfigureAwait(false);
            return BadRequest(resS);
        }
        public async Task<IActionResult> TransactionStatusCheck(StatusAPIRequest req)
        {
            try
            {
                var res = await _apiML.GetDMTStatusCheck(req).ConfigureAwait(false);
                string resp = JsonConvert.SerializeObject(res);
                await SaveReqResp(ServiceType.MoneyTransfer, resp).ConfigureAwait(false);
                return Json(res);
            }
            catch (Exception ex)
            {
                var res = new StatusMsg
                {
                    MSG = ErrorCodes.InvalidParam,
                    ERRORCODE = ErrorCodes.Invalid_Parameter.ToString()
                };
                string resp = JsonConvert.SerializeObject(res);
                await SaveReqResp(ServiceType.Recharge, resp + " Exception:" + ex.Message).ConfigureAwait(false);
                return BadRequest(res);
            }
        }
        [HttpGet]
        public async Task<IActionResult> TransactionAPI(RechargeAPIRequest req)
        {
            try
            {
                TransactionResponse res = await _apiML.APIRecharge(req).ConfigureAwait(false);
                if (req.Format == ResponseType.XML)
                {
                    string resp = XMLHelper.O.SerializeToXml(res, "TransactionResponse");
                    await SaveReqResp(ServiceType.Recharge, resp).ConfigureAwait(false);
                    return Content(resp);
                }
                else
                {
                    string resp = JsonConvert.SerializeObject(res);
                    await SaveReqResp(ServiceType.Recharge, resp).ConfigureAwait(false);
                    return Json(res);
                }
            }
            catch (Exception ex)
            {
                var res = new StatusMsg
                {
                    STATUS = RechargeRespType.FAILED,
                    MSG = "Invalid request parameter",
                    ERRORCODE = ErrorCodes.Invalid_Parameter.ToString()
                };
                string resp = JsonConvert.SerializeObject(res);
                await SaveReqResp(ServiceType.Recharge, resp + " Exception:" + ex.Message);
                return BadRequest(res);
            }
        }
        [HttpGet]
        public async Task<IActionResult> FetchBill(RechargeAPIRequest req)
        {
            try
            {
                TransactionResponse res = await _apiML.FetchBill(req).ConfigureAwait(false);
                if (req.Format == ResponseType.XML)
                {
                    string resp = XMLHelper.O.SerializeToXml(res, "FetchBill");
                    await SaveReqResp(ServiceType.Recharge, resp).ConfigureAwait(false);
                    return Content(resp);
                }
                else
                {
                    string resp = JsonConvert.SerializeObject(res);
                    await SaveReqResp(ServiceType.Recharge, resp).ConfigureAwait(false);
                    return Json(res);
                }
            }
            catch (Exception ex)
            {
                var res = new StatusMsg
                {
                    STATUS = RechargeRespType.FAILED,
                    MSG = "Invalid request parameter",
                    ERRORCODE = ErrorCodes.Invalid_Parameter.ToString()
                };
                string resp = JsonConvert.SerializeObject(res);
                await SaveReqResp(ServiceType.Recharge, resp + " Exception:" + ex.Message).ConfigureAwait(false);
                return BadRequest(res);
            }
        }
        public async Task<IActionResult> RefundRequest(RefundAPIRequest req)
        {
            try
            {
                StatusMsg res = await _apiML.MarkDispute(req).ConfigureAwait(false);
                res.IsShow = true;
                if (req.Format == ResponseType.XML)
                {
                    string resp = XMLHelper.O.SerializeToXml(res, "TransactionResponse");
                    await SaveReqResp(ServiceType.Recharge, resp).ConfigureAwait(false);
                    return Content(resp);
                }
                else
                {
                    string resp = JsonConvert.SerializeObject(res);
                    await SaveReqResp(ServiceType.Recharge, resp).ConfigureAwait(false);
                    return Json(res);
                }
            }
            catch (Exception ex)
            {
                var res = new StatusMsg
                {
                    STATUS = RechargeRespType.FAILED,
                    MSG = "Invalid request parameter",
                    ERRORCODE = ErrorCodes.Invalid_Parameter.ToString()
                };
                string resp = JsonConvert.SerializeObject(res);
                await SaveReqResp(ServiceType.Recharge, resp + " Exception:" + ex.Message).ConfigureAwait(false);
                return BadRequest(res);
            }
        }

        [HttpPost]
        public async Task<IActionResult> GetBillerByType([FromBody] APIBillerRequest req)
        {
            try
            {
                var res = await _apiML.GetRPBillerByType(req).ConfigureAwait(false);
                if (req.Format == ResponseType.XML)
                {
                    string resp = XMLHelper.O.SerializeToXml(res, "GetBillerByType");
                    await SaveReqResp(ServiceType.Recharge, resp).ConfigureAwait(false);
                    return Content(resp);
                }
                else
                {
                    string resp = JsonConvert.SerializeObject(res);
                    await SaveReqResp(ServiceType.Recharge, resp).ConfigureAwait(false);
                    return Json(res);
                }
            }
            catch (Exception ex)
            {
                var res = new StatusMsg
                {
                    STATUS = RechargeRespType.FAILED,
                    MSG = "Invalid request parameter",
                    ERRORCODE = ErrorCodes.Invalid_Parameter.ToString()
                };
                string resp = JsonConvert.SerializeObject(res);
                await SaveReqResp(ServiceType.Recharge, resp + " Exception:" + ex.Message).ConfigureAwait(false);
                return BadRequest(res);
            }
        }
        [HttpPost]
        public async Task<IActionResult> GetBillerByBillerID([FromBody] APIBillerRequest req)
        {
            try
            {
                var res = await _apiML.GetRPBillerByID(req).ConfigureAwait(false);
                if (req.Format == ResponseType.XML)
                {
                    string resp = XMLHelper.O.SerializeToXml(res, "GetBillerByBillerID");
                    await SaveReqResp(ServiceType.Recharge, resp).ConfigureAwait(false);
                    return Content(resp);
                }
                else
                {
                    string resp = JsonConvert.SerializeObject(res);
                    await SaveReqResp(ServiceType.Recharge, resp).ConfigureAwait(false);
                    return Json(res);
                }
            }
            catch (Exception ex)
            {
                var res = new StatusMsg
                {
                    STATUS = RechargeRespType.FAILED,
                    MSG = "Invalid request parameter",
                    ERRORCODE = ErrorCodes.Invalid_Parameter.ToString()
                };
                string resp = JsonConvert.SerializeObject(res);
                await SaveReqResp(ServiceType.Recharge, resp + " Exception:" + ex.Message).ConfigureAwait(false);
                return BadRequest(res);
            }
        }
        #endregion
        #region PANService
        [HttpGet]
        public async Task<IActionResult> PANServiceAPI(RechargeAPIRequest req)
        {
            try
            {
                TransactionResponse res = await _apiML.APIPANService(req).ConfigureAwait(false);
                string resp = JsonConvert.SerializeObject(res);
                await SaveReqResp(ServiceType.PSAService, resp).ConfigureAwait(false);
                return Json(res);
            }
            catch (Exception ex)
            {
                var res = new StatusMsg
                {
                    STATUS = RechargeRespType.FAILED,
                    MSG = "Invalid request parameter",
                    ERRORCODE = ErrorCodes.Invalid_Parameter.ToString()
                };
                string resp = JsonConvert.SerializeObject(res);
                await SaveReqResp(ServiceType.PSAService, resp + " Exception:" + ex.Message).ConfigureAwait(false);
                return BadRequest(res);
            }
        }
        #endregion
        private async Task SaveReqResp(int UserID, string resp)
        {
            string req = "";
            var request = HttpContext.Request;
            if (request.Method == "POST")
            {
                string rbody = "";
                if (request.Body.CanSeek)
                    request.Body.Position = 0;
                using (var reader = new StreamReader(request.Body))
                {
                    rbody = await reader.ReadToEndAsync();
                }
                req = GetAbsoluteURI() + "?" + rbody;
            }
            else
            {
                req = GetAbsoluteURI() + request.QueryString.ToString();
            }
            var aPIReqResp = new APIReqResp
            {
                UserID = UserID,
                Request = req,
                Response = resp,
                Method = request.Method
            };
            await _apiML.SaveAPILog(aPIReqResp).ConfigureAwait(false);
        }
        private string GetAbsoluteURI()
        {
            var request = HttpContext.Request;
            return request.Scheme + "://" + request.Host + request.Path;
        }
        #region IPGeoLocation
        [HttpPost]
        public IActionResult CheckIPGeoInfo([FromBody] ApiIPGeoLocInfoReq apiIPGeoLocInfoReq)
        {
            var res = new IPStatusResp()
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            IReportML reportML = new ReportML(_accessor, _env);
            var userAuth = reportML.AuthenticateAPIReqForIPGeoInfo(apiIPGeoLocInfoReq.UserID, apiIPGeoLocInfoReq.Token).Result;
            if (userAuth.Statuscode == ErrorCodes.One)
            {
                res = reportML.CheckIPGeoLocationInfo(new IPGeoLocInfoReq
                {
                    LoginTypeID = 1,
                    UserID = apiIPGeoLocInfoReq.UserID,
                    IPInfo = apiIPGeoLocInfoReq.IPInfo
                });
            }
            else
            {
                res.Statuscode = userAuth.Statuscode;
                res.Msg = userAuth.Msg;
            }
            return Json(res);
        }
        #endregion

        [HttpGet]
        public IActionResult SetApplicationSetting(bool Is)
        {
            ApplicationSetting.IsEKYCForced = Is;
            return Ok(ApplicationSetting.IsEKYCForced);
        }
        [HttpGet]
        public IActionResult Get()
        {
            SprintBBPSML sprintBBPSML = new SprintBBPSML(_accessor, _env, null);
            
            return Ok(sprintBBPSML.BankList());
        }
    }

}