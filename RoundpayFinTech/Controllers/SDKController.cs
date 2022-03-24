using Fintech.AppCode.Configuration;
using Fintech.AppCode.Model;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using RoundpayFinTech.AppCode.Configuration;
using RoundpayFinTech.AppCode.MiddleLayer;
using RoundpayFinTech.AppCode.Model.Recharge;
using RoundpayFinTech.AppCode.Model.SDK;
using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Validators;

namespace RoundpayFinTech.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class SDKController : Controller
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        public SDKController(IHttpContextAccessor accessor, IHostingEnvironment env)
        {
            _accessor = accessor;
            _env = env;
        }
        [HttpPost("SDK/Detail")]
        public async Task<IActionResult> GetSDKDetail([FromBody] SDKRequest sDKRequest)
        {
            if (ApplicationSetting.IsAEPSAPIResale || ApplicationSetting.IsMiniBankAPIResale)
            {
                var ML = new SDKML(_accessor, _env);
                var res = ML.GetSDKDetail(sDKRequest);
                var aPIReqResp = new APIReqResp
                {
                    UserID = sDKRequest.UserID ?? 0,
                    Request = JsonConvert.SerializeObject(sDKRequest),
                    Response = JsonConvert.SerializeObject(res),
                    Method = "GetSDKDetail"
                };
                await ML.SaveAPILog(aPIReqResp).ConfigureAwait(false);
                return Json(res);
            }
            return Forbid();
        }
        [HttpPost("SDK/InitiateMiniBank")]
        public async Task<IActionResult> InitiateMiniBank([FromBody] SDKIntitateRequest sDKRequest)
        {
            if (ApplicationSetting.IsMiniBankAPIResale)
            {
                var ML = new SDKML(_accessor, _env);
                var res = await ML.InitiateMiniBank(sDKRequest).ConfigureAwait(false);
                var aPIReqResp = new APIReqResp
                {
                    UserID = sDKRequest.UserID ?? 0,
                    Request = JsonConvert.SerializeObject(sDKRequest),
                    Response = JsonConvert.SerializeObject(res),
                    Method = "InitiateMiniBank"
                };
                await ML.SaveAPILog(aPIReqResp).ConfigureAwait(false);
                return Json(res);
            }
            return Forbid();
        }
        [HttpPost("SDK/UpdateMiniBankStatus")]
        public async Task<IActionResult> UpdateMiniBankStatus()
        {
            if (ApplicationSetting.IsMiniBankAPIResale)
            {
                StringBuilder resp = new StringBuilder("");
                var request = HttpContext.Request;
                var callbackAPIReq = new CallbackData
                {
                    Method = request.Method,
                    APIID = 0,
                    Content = resp.ToString(),
                    Scheme = request.Scheme,
                    Path = request.Path
                };
                try
                {
                    if (request.Method == "POST")
                    {
                        if (request.HasFormContentType)
                        {
                            if (request.Form.Keys.Count > 0)
                            {
                                foreach (var item in request.Form.Keys)
                                {
                                    request.Form.TryGetValue(item, out StringValues strVal);
                                    if (resp.Length == 0)
                                    {
                                        resp.AppendFormat("{0}={1}", item, strVal);
                                    }
                                    else
                                    {
                                        resp.AppendFormat("&{0}={1}", item, strVal);
                                    }
                                }
                            }
                        }
                        else
                        {
                            resp = new StringBuilder(await request.GetRawBodyStringAsync().ConfigureAwait(false));

                        }
                    }
                    else
                    {
                        resp = new StringBuilder(request.QueryString.ToString());
                    }
                    if (resp.Length == 0)
                    {
                        resp = new StringBuilder(request.QueryString.ToString());
                    }
                }
                catch (Exception ex)
                {
                    resp = new StringBuilder(ex.Message);
                }
                callbackAPIReq.Content = WebUtility.UrlDecode(resp.ToString());
                SDKIntitateRequest sDKRequest = null;
                if ((callbackAPIReq.Content ?? string.Empty).ToLower().Contains("tid"))
                {
                    if (Validate.O.ValidateJSON(callbackAPIReq.Content))
                    {
                        sDKRequest = JsonConvert.DeserializeObject<SDKIntitateRequest>(callbackAPIReq.Content);
                    }
                }
                sDKRequest = sDKRequest == null ? new SDKIntitateRequest() : sDKRequest;
                var ML = new SDKML(_accessor, _env);
                var res = ML.UpdateMiniBankStatus(sDKRequest);
                var aPIReqResp = new APIReqResp
                {
                    UserID = sDKRequest.UserID ?? 0,
                    Request = callbackAPIReq.Content,
                    Response = JsonConvert.SerializeObject(res),
                    Method = "UpdateMiniBankStatus"
                };
                await ML.SaveAPILog(aPIReqResp).ConfigureAwait(false);
                return Json(res);
            }
            return Forbid();
        }
        [HttpPost("SDK/Bank")]
        public IActionResult GetBank()
        {
            var ML = new SDKML(_accessor, _env);
            var res = ML.GetBank();
            return Json(res);
        }

        [HttpPost("SDK/AEPSWithdrawal")]
        public async Task<IActionResult> AEPSWithdrawal([FromBody] SDKIntitateRequest sDKRequest)
        {
            if (ApplicationSetting.IsAEPSAPIResale)
            {
                var ML = new SDKML(_accessor, _env);
                var res = await ML.AEPSWithdrawal(sDKRequest).ConfigureAwait(false);
                var aPIReqResp = new APIReqResp
                {
                    UserID = sDKRequest.UserID ?? 0,
                    Request = JsonConvert.SerializeObject(sDKRequest),
                    Response = JsonConvert.SerializeObject(res),
                    Method = "AEPSWithdrawal"
                };
                await ML.SaveAPILog(aPIReqResp).ConfigureAwait(false);
                return Json(res);
            }
            return Forbid();
        }
        [HttpPost("SDK/GetBalanceAEPS")]
        public async Task<IActionResult> GetBalanceAEPS([FromBody] SDKIntitateRequest sDKRequest)
        {
            if (ApplicationSetting.IsAEPSAPIResale || ApplicationSetting.IsMiniBankAPIResale)
            {
                var ML = new SDKML(_accessor, _env);
                var res = await ML.GetBalanceAEPS(sDKRequest).ConfigureAwait(false);
                var aPIReqResp = new APIReqResp
                {
                    UserID = sDKRequest.UserID ?? 0,
                    Request = JsonConvert.SerializeObject(sDKRequest),
                    Response = JsonConvert.SerializeObject(res),
                    Method = "GetBalanceAEPS"
                };
                await ML.SaveAPILog(aPIReqResp).ConfigureAwait(false);
                return Json(res);
            }
            return Forbid();
        }
        [HttpPost("SDK/MiniStatement")]
        public async Task<IActionResult> GetMiniStatement([FromBody] SDKIntitateRequest sDKRequest)
        {
            if (ApplicationSetting.IsAEPSAPIResale || ApplicationSetting.IsMiniBankAPIResale)
            {
                var ML = new SDKML(_accessor, _env);
                var res = ML.GetMiniStatement(sDKRequest);
                var aPIReqResp = new APIReqResp
                {
                    UserID = sDKRequest.UserID ?? 0,
                    Request = JsonConvert.SerializeObject(sDKRequest),
                    Response = JsonConvert.SerializeObject(res),
                    Method = "GetMiniStatement"
                };
                await ML.SaveAPILog(aPIReqResp).ConfigureAwait(false);
                return Json(res);
            }
            return Forbid();

        }
        [HttpPost("SDK/GenerateDepositOTP")]
        public async Task<IActionResult> GenerateDepositOTP([FromBody] SDKIntitateRequest sDKRequest)
        {
            if (ApplicationSetting.IsAEPSAPIResale || ApplicationSetting.IsMiniBankAPIResale)
            {
                var ML = new SDKML(_accessor, _env);
                var res = await ML.DepositGenerateOTP(sDKRequest).ConfigureAwait(false);
                var aPIReqResp = new APIReqResp
                {
                    UserID = sDKRequest.UserID ?? 0,
                    Request = JsonConvert.SerializeObject(sDKRequest),
                    Response = JsonConvert.SerializeObject(res),
                    Method = "GenerateDepositOTP"
                };
                await ML.SaveAPILog(aPIReqResp).ConfigureAwait(false);
                return Json(res);
            }
            return Forbid();
        }
        [HttpPost("SDK/VerifyDepositOTP")]
        public async Task<IActionResult> VerifyDepositOTP([FromBody] SDKIntitateRequest sDKRequest)
        {
            if (ApplicationSetting.IsAEPSAPIResale || ApplicationSetting.IsMiniBankAPIResale)
            {
                var ML = new SDKML(_accessor, _env);
                var res = await ML.VerifyDepositOTP(sDKRequest).ConfigureAwait(false);
                var aPIReqResp = new APIReqResp
                {
                    UserID = sDKRequest.UserID ?? 0,
                    Request = JsonConvert.SerializeObject(sDKRequest),
                    Response = JsonConvert.SerializeObject(res),
                    Method = "VerifyDepositOTP"
                };
                await ML.SaveAPILog(aPIReqResp).ConfigureAwait(false);
                return Json(res);
            }
            return Forbid();
        }
        [HttpPost("SDK/DepositNow")]
        public async Task<IActionResult> DepositNow([FromBody] SDKIntitateRequest sDKRequest)
        {
            if (ApplicationSetting.IsAEPSAPIResale || ApplicationSetting.IsMiniBankAPIResale)
            {
                var ML = new SDKML(_accessor, _env);
                var res = await ML.DepositAccount(sDKRequest).ConfigureAwait(false);
                var aPIReqResp = new APIReqResp
                {
                    UserID = sDKRequest.UserID ?? 0,
                    Request = JsonConvert.SerializeObject(sDKRequest),
                    Response = JsonConvert.SerializeObject(res),
                    Method = "DepositNow"
                };
                await ML.SaveAPILog(aPIReqResp).ConfigureAwait(false);
                return Json(res);
            }
            return Forbid();
        }
    }
}
