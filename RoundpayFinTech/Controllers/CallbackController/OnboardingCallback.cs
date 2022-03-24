using Fintech.AppCode.Configuration;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.MiddleLayer;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.StaticModel.MoneyTransfer;
using RoundpayFinTech.AppCode.ThirdParty.Zoop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Validators;

namespace RoundpayFinTech.Controllers
{
    public partial class CallbackController : Controller
    {
        [Route("Callback/PaySprintOnboard")]
        public async Task<IActionResult> PaySprintOnboard()
        {
            String strText = "Onboarding at bank not completed";
            StringBuilder resp = new StringBuilder("");
            var request = HttpContext.Request;
            var callbackAPIReq = new CallbackData
            {
                Method = request.Method,
                APIID = 0,
                Content = resp.ToString(),
                Scheme = request.Scheme,
                Path = request.Path,
                InActiveMode = false
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
            var ml = new CallbackML(_accessor, _env);
            var Is = await ml.LogCallBackRequestBool(callbackAPIReq).ConfigureAwait(false);
            if (!string.IsNullOrEmpty(callbackAPIReq.Content))
            {
                if (callbackAPIReq.Content.Contains("data="))
                {
                    var dataSplit = callbackAPIReq.Content.Split("data=");
                    if (dataSplit.Length == 2)
                    {
                        var encData = dataSplit[1];
                        if (!string.IsNullOrEmpty(encData))
                        {
                            try
                            {
                                OnboardingML onboardingML = new OnboardingML(_accessor, _env);
                                var decResp = onboardingML.DecodeJWT(APICode.SPRINT, encData);
                                if (!string.IsNullOrEmpty(decResp))
                                {
                                    if (Validate.O.ValidateJSON(decResp))
                                    {
                                        var sprintResp = JsonConvert.DeserializeObject<SprintOnboardJWTDecodeModel>(decResp);
                                        if (sprintResp != null)
                                        {
                                            var apiResp = new OutletAPIStatusUpdateReq
                                            {

                                            };
                                            if (sprintResp.payload != null)
                                            {
                                                if (sprintResp.payload.status == "1")
                                                {
                                                    apiResp.MyPartnerIDInAPI = sprintResp.payload.partnerid;
                                                    apiResp.OutletID = Convert.ToInt32(sprintResp.payload.merchantcode ?? "0");
                                                    apiResp.APIOutletID = sprintResp.payload.merchantcode;
                                                    apiResp._APICode = APICode.SPRINT;
                                                    apiResp.UserID = 1;
                                                    apiResp.APIOutletStatus = UserStatus.ACTIVE;
                                                    apiResp.KYCStatus = UserStatus.ACTIVE;
                                                    apiResp.IsVerifyStatusUpdate = true;
                                                    apiResp.Statuscode = ErrorCodes.One;
                                                    apiResp.AEPSID = sprintResp.payload.merchantcode;
                                                    apiResp.AEPSStatus = UserStatus.ACTIVE;
                                                    apiResp.IsAEPSUpdate = true;
                                                    apiResp.IsAEPSUpdateStatus = true;
                                                    strText = "Onboarding at bank completed sucessfully.";
                                                }
                                                else if (sprintResp.payload.status == "0")
                                                {
                                                    apiResp.MyPartnerIDInAPI = sprintResp.payload.partnerid;
                                                    apiResp.OutletID = Convert.ToInt32(sprintResp.payload.merchantcode ?? "0");
                                                    apiResp.APIOutletID = sprintResp.payload.merchantcode;
                                                    apiResp._APICode = APICode.SPRINT;
                                                    apiResp.UserID = 1;
                                                    apiResp.APIOutletStatus = UserStatus.REJECTED;
                                                    apiResp.KYCStatus = UserStatus.REJECTED;
                                                    apiResp.IsVerifyStatusUpdate = true;
                                                    apiResp.Statuscode = ErrorCodes.One;
                                                    apiResp.AEPSID = sprintResp.payload.merchantcode;
                                                    apiResp.AEPSStatus = UserStatus.REJECTED;
                                                    apiResp.IsAEPSUpdate = true;
                                                    apiResp.IsAEPSUpdateStatus = true;

                                                }
                                            }

                                            if (apiResp.IsAEPSUpdate)
                                            {
                                                onboardingML.CallUpdateFromCallBack(apiResp);
                                            }
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                            }
                        }
                    }
                }
            }

            var html = new StringBuilder(@"<!DOCTYPE html>
                                                    <html>
<head><script>
(()=>{
setTimeout(function(){
var win = window.open('about: blank', '_self');
win.close();
},2000);
        })();</script></head>
                                                    <body>
                                                        <center><h2>{TEXT}</h2></center>
                                                    </body>
                                                    </html>"); html.Replace("{TEXT}", strText);


            return Content(html.ToString(), contentType: "text/html; charset=utf-8");
        }

        [Route("Callback/PaySprintTransaction")]
        public async Task<IActionResult> PaySprintTransaction()
        {
            StringBuilder resp = new StringBuilder("");
            var request = HttpContext.Request;
            var callbackAPIReq = new CallbackData
            {
                Method = request.Method,
                APIID = 0,
                Content = resp.ToString(),
                Scheme = request.Scheme,
                Path = request.Path,
                InActiveMode = false
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
            var ml = new CallbackML(_accessor, _env);
            var Is = await ml.LogCallBackRequestBool(callbackAPIReq).ConfigureAwait(false);
            if (!string.IsNullOrEmpty(callbackAPIReq.Content))
            {
                if (callbackAPIReq.Content.Contains("data="))
                {
                    var dataSplit = callbackAPIReq.Content.Split("data=");
                    if (dataSplit.Length == 2)
                    {
                        var encData = dataSplit[1];
                        if (!string.IsNullOrEmpty(encData))
                        {
                            try
                            {
                                //do here decode using jwt key
                            }
                            catch (Exception ex)
                            {
                            }
                        }
                    }
                }
            }

            return Json(new { status = 200, message = "Transaction completed successfully" });
        }

        [Route("Callback/ZoopWebSDKWebHook/{initiateid}")]
        public async Task<IActionResult> ZoopWebSDKWebHook(int initiateid)
        {
            StringBuilder resp = new StringBuilder("");
            var request = HttpContext.Request;
            var callbackAPIReq = new CallbackData
            {
                Method = request.Method,
                APIID = 0,
                Content = resp.ToString(),
                Scheme = request.Scheme,
                Path = request.Path,
                InActiveMode = false
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
            var ml = new CallbackML(_accessor, _env);
            var Is = await ml.LogCallBackRequestBool(callbackAPIReq).ConfigureAwait(false);
            ml.MakeMakeFileLog(callbackAPIReq);
            if (!string.IsNullOrEmpty(callbackAPIReq.Content))
            {
                if (callbackAPIReq.Content.Contains("request_id"))
                {
                    var jsonResp = JsonConvert.DeserializeObject<ZoopModelResponseWebhookDigilocker>(callbackAPIReq.Content);
                    EKYCML eKYCML = new EKYCML(_accessor,_env);
                    var aadharResp = new ZoopML(_accessor, _env, eKYCML.GetDAL(), 0).AadharEKYCFromCallback(jsonResp);
                    aadharResp.InitiateID = initiateid;
                    eKYCML.UpdateEKYCFromCallBack(aadharResp);
                }
            }

            return Json(new { status = 200, message = "Transaction completed successfully" });
        }
    }
}
