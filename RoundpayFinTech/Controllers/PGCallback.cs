using Fintech.AppCode.Configuration;
using Fintech.AppCode.HelperClass;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using RoundpayFinTech.AppCode.HelperClass;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.MiddleLayer;
using RoundpayFinTech.AppCode.MiddleLayer.Dmt_Api;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.ThirdParty.AggrePay;
using RoundpayFinTech.AppCode.ThirdParty.AxisBank;
using RoundpayFinTech.AppCode.ThirdParty.CashFree;
using RoundpayFinTech.AppCode.ThirdParty.Icici;
using RoundpayFinTech.AppCode.ThirdParty.Paytm;
using RoundpayFinTech.AppCode.ThirdParty.PayU;
using RoundpayFinTech.AppCode.ThirdParty.Razorpay;
using RoundpayFinTech.AppCode.ThirdParty.UPIGateway;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Validators;

namespace RoundpayFinTech.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class PGCallbackController : Controller
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        public PGCallbackController(IHttpContextAccessor accessor, IHostingEnvironment env)
        {
            _accessor = accessor;
            _env = env;
        }
        [HttpPost]
        public async Task<IActionResult> Paytm([FromForm] PaytmPGResponse paytmPGResponse)
        {
            var TID = 0;
            var TransactionID = string.Empty;
            if (Validate.O.IsNumeric(paytmPGResponse.ORDERID ?? string.Empty))
            {
                TID = Convert.ToInt32(paytmPGResponse.ORDERID);
            }
            IPaymentGatewayML paymentGatewayML = new PaymentGatewayML(_accessor, _env);
            await paymentGatewayML.SavePGTransactionLog(PaymentGatewayType.PAYTM, TID, (paytmPGResponse != null ? JsonConvert.SerializeObject(paytmPGResponse) : string.Empty), TransactionID, string.Empty, RequestMode.API, false, 0, string.Empty).ConfigureAwait(false);
            var pResp = paymentGatewayML.UpdateFromPayTMCallback(paytmPGResponse);

            var FLYINGARROW = string.Format("http://{0}/PGCallback/CommonPG", pResp.CommonStr ?? string.Empty);
            var commonPGResp = new CommonPGResponse
            {
                TID = paytmPGResponse.ORDERID ?? string.Empty,
                Amount = paytmPGResponse.TXNAMOUNT ?? string.Empty,
                TransactionID = paytmPGResponse.TXNID ?? string.Empty,
                status = pResp.Statuscode.ToString(),
                reason = pResp.Msg ?? string.Empty
            };
            if (pResp.CommonInt < 2)
            {
                var html = new StringBuilder(@"<html><head><script>
                                (()=>{
                                        var obj={TID:""{TID}"",Amount:""{Amount}"",TransactionID:""{TransactionID}"",statuscode:""{status}"",reason:""{reason}"",origin:""addMoney""}
                                        localStorage.setItem('obj', JSON.stringify(obj));
                                        window.close()
                                   })();</script></head><body><h6>Redirect to site.....</h6></body></html>");
                html.Replace("{TID}", commonPGResp.TID);
                html.Replace("{Amount}", commonPGResp.Amount);
                html.Replace("{TransactionID}", commonPGResp.TransactionID);
                html.Replace("{status}", commonPGResp.status);
                html.Replace("{reason}", commonPGResp.reason);
                return Content(html.ToString(), contentType: "text/html; charset=utf-8");
            }
            else
            {
                var html = new StringBuilder(@"<!DOCTYPE html>
<html>
<body>
    <form action=""{FLYINGARROW}"" method=""post"" onload=""document.forms[0].submit()"">
        <input type=""hidden"" id=""TID"" name=""TID"" value=""{TID}""><br>
        <input type=""hidden"" id=""Amount"" name=""Amount"" value=""{Amount}"">
        <input type=""hidden"" id=""TransactionID"" name=""TransactionID"" value=""{TransactionID}""><br>
        <input type=""hidden"" id=""status"" name=""status"" value=""{status}""><br>
        <input type=""hidden"" id=""reason"" name=""reason"" value=""{reason}""><br>
    </form>
</body>
</html>");
                html.Replace("{FLYINGARROW}", FLYINGARROW);
                html.Replace("{TID}", commonPGResp.TID);
                html.Replace("{Amount}", commonPGResp.Amount);
                html.Replace("{TransactionID}", commonPGResp.TransactionID);
                html.Replace("{status}", commonPGResp.status);
                html.Replace("{reason}", commonPGResp.reason);

                return Content(html.ToString(), contentType: "text/html; charset=utf-8");
            }

            // return View("PGConfirmation", Is.Statuscode == ErrorCodes.One);
        }
        [HttpPost]
        public async Task<IActionResult> CommonPG([FromForm] CommonPGResponse commonPGResp)
        {
            StringBuilder html = new StringBuilder(@"<html><head><script>
                                (()=>{
                                        var obj={TID:""{TID}"",Amount:""{Amount}"",TransactionID:""{TransactionID}"",statuscode:""{status}"",reason:""{reason}"",origin:""addMoney""}
                                        localStorage.setItem('obj', JSON.stringify(obj));
                                        window.close()
                                   })();</script></head><body><h6>Redirect to site.....</h6></body></html>");
            html.Replace("{TID}", commonPGResp.TID);
            html.Replace("{Amount}", commonPGResp.Amount);
            html.Replace("{TransactionID}", commonPGResp.TransactionID);
            html.Replace("{status}", commonPGResp.status);
            html.Replace("{reason}", commonPGResp.reason);
            return Content(html.ToString(), contentType: "text/html; charset=utf-8");
        }
        [Route("PGCallback/ICICI")]
        [Route("PGCallback/ICICIPayout")]
        public async Task<IActionResult> ICICI()
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
            var res = new ICICIModelResp
            {
                CODE = "06",
                SuccessANDRejected = "Rejected"
            };
            var req = new ICICIModelReq();
            if ((callbackAPIReq.Content ?? string.Empty).Contains("CustomerCode") || (callbackAPIReq.Content ?? string.Empty).Contains("ClientCode") && Is)
            {
                if (Validate.O.ValidateJSON(callbackAPIReq.Content))
                {
                    try
                    {
                        req = JsonConvert.DeserializeObject<ICICIModelReq>(callbackAPIReq.Content);
                        IciciML iciciML = new IciciML(_accessor, _env, false);
                        res = iciciML.ValidateICICIData(req);
                    }
                    catch (Exception ex)
                    {

                    }

                }
            }
            if (!string.IsNullOrEmpty(req.ClientCode))
            {
                return Json(new ICICIModelRespp
                {
                    SuccessANDRejected = res.SuccessANDRejected,
                    CODE = res.CODE
                });
            }
            else
            {
                return Json(res);
            }
        }
        [Route("PGCallback/AXISCDMCALLBACK")]
        public async Task<IActionResult> AXISBank()
        {
            StringBuilder resp = new StringBuilder();
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
            var res = new ICICIModelResp
            {
                CODE = "06",
                SuccessANDRejected = "Rejected"
            };
            if ((callbackAPIReq.Content ?? string.Empty).Contains("UTR") && Is)
            {
                if (Validate.O.ValidateJSON(callbackAPIReq.Content))
                {
                    try
                    {
                        var req = JsonConvert.DeserializeObject<AxisBankResp>(callbackAPIReq.Content);
                        IciciML iciciML = new IciciML(_accessor, _env, false);
                        res = iciciML.ValidateAxisData(req);
                    }
                    catch (Exception ex)
                    {
                    }

                }
            }
            return Json(res);
        }
        [Route("PGCallback/IPAYECOLLECT")]
        public async Task<IActionResult> InstantPayCollect(string ipay_id, string agent_id, string opr_id, decimal amount, string sp_key, string ssp_key, string optional1, string optional2, string optional3, string optional4, string status, string res_code, string res_msg)
        {
            StringBuilder resp = new StringBuilder();
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

            if (Is)
            {
                IciciML iciciML = new IciciML(_accessor, _env, false);
                var res = iciciML.ValidateIpayData(ipay_id, agent_id, opr_id, amount, sp_key, ssp_key, optional1, optional2, optional3, optional4, status, res_code, res_msg);
                return Json(new { res.Statuscode, res.Msg });
            }
            return Json(new { Statuscode = ErrorCodes.Minus1, Msg = "Invalid Request!" });
        }
        public IActionResult ICICIUPI()
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
                        resp = new StringBuilder(request.GetRawBodyStringAsync().Result);

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
            callbackAPIReq.Content = resp.ToString();
            var ml = new CallbackML(_accessor, _env);
            var Is = ml.LogCallBackRequestBool(callbackAPIReq).Result;
            try
            {
                byte[] dataOutput = Convert.FromBase64String(callbackAPIReq.Content);
                var responseDecrypt = HashEncryption.O.DecryptUsingPrivatePFXKey(dataOutput, ICICIPayoutML.PrivateKeyFilePathUPI);
                callbackAPIReq.Content = responseDecrypt;
                Is = ml.LogCallBackRequestBool(callbackAPIReq).Result;
                //Consider that IP whitelisted
                if (Is)
                {
                    if ((responseDecrypt ?? string.Empty).Contains("merchantTranId"))
                    {
                        var req = JsonConvert.DeserializeObject<ICICICallBackRes>(responseDecrypt);
                        if (req.merchantTranId.Contains("TIDQR"))
                        {
                            if (!req.merchantTranId.StartsWith("TIDQR"))
                            {
                                string UserID = req.merchantTranId.Substring(0, req.merchantTranId.IndexOf("TIDQR"));
                                if (req.TxnStatus.Equals("SUCCESS"))
                                {
                                    IciciML iciciML = new IciciML(_accessor, _env, false);
                                    iciciML.ValidateICICIDataQR(new ICICIModelReq
                                    {
                                        UTR = req.BankRRN,
                                        VirtualACCode = UserID,
                                        AMT = req.PayerAmount,
                                        PayeeAccountNumber = req.PayerVA,
                                        CustomerCode = req.subMerchantId,
                                        PayeeName = req.PayerName
                                    });
                                }
                            }
                        }
                        else if (req.merchantTranId.Contains("AUTOID"))
                        {
                            if (!req.merchantTranId.StartsWith("AUTOID"))
                            {
                                string tabID = req.merchantTranId.Substring(0, req.merchantTranId.IndexOf("AUTOID"));
                                IReportML rmL = new ReportML(_accessor, _env);
                                var UserID = rmL.GetQRStockDataByID(Convert.ToInt32(tabID), 1, 1).CommonInt;
                                if (req.TxnStatus.Equals("SUCCESS"))
                                {
                                    IciciML iciciML = new IciciML(_accessor, _env, false);
                                    iciciML.ValidateICICIDataQR(new ICICIModelReq
                                    {
                                        UTR = req.BankRRN,
                                        VirtualACCode = UserID.ToString(),
                                        AMT = req.PayerAmount,
                                        PayeeAccountNumber = req.PayerVA,
                                        CustomerCode = req.subMerchantId,
                                        PayeeName = req.PayerName
                                    });
                                }
                            }
                        }
                        else
                        {
                            var tiD = req.merchantTranId.Replace("TID", "");
                            var Amount = Convert.ToInt32(req.PayerAmount.Split('.')[0]);
                            var txnStatus = req.TxnStatus == "SUCCESS" ? RechargeRespType.SUCCESS : RechargeRespType.FAILED;
                            if (Validate.O.IsNumeric(tiD) && txnStatus == RechargeRespType.SUCCESS)
                            {
                                IUPIPaymentML uPIPaymentML = new PaymentGatewayML(_accessor, _env);
                                uPIPaymentML.UpdateUPIPaymentStatus(new AppCode.Model.ProcModel.UpdatePGTransactionRequest
                                {
                                    TID = Convert.ToInt32(tiD),
                                    Type = txnStatus,
                                    LiveID = req.BankRRN,
                                    PGID = PaymentGatewayType.ICICIUPI,
                                    PaymentModeSpKey = PaymentGatewayTranMode.UPIICI,
                                    Remark = req.PayerVA,
                                    RequestPage = "Callback",
                                    Amount = Amount
                                });
                            }
                        }

                    }

                }
            }
            catch (Exception ex)
            {

            }
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> RazorPaySuccess([FromForm] RazorPaySuccessResp paytmPGResponse)
        {
            var pML = new PaymentGatewayML(_accessor, _env);
            var res = pML.UpdateRazorPaySuccess(paytmPGResponse);
            StringBuilder html = new StringBuilder(@"<html><head><script>
                                (()=>{
                                        var obj={TID:""{TID}"",Amount:""{Amount}"",TransactionID:""{TransactionID}"",statuscode:""{status}"",reason:""{reason}"",origin:""addMoney""}
                                        localStorage.setItem('obj', JSON.stringify(obj));
                                        window.close()
                                   })();</script></head><body><h6>Redirect to site.....</h6></body></html>");
            html.Replace("{TID}", res.CommonInt.ToString());
            html.Replace("{Amount}", res.CommonInt2.ToString());
            html.Replace("{TransactionID}", res.CommonStr ?? string.Empty);
            html.Replace("{status}", res.Statuscode.ToString());
            html.Replace("{reason}", res.Msg ?? string.Empty);
            return Content(html.ToString(), contentType: "text/html; charset=utf-8");
        }

        public async Task<IActionResult> Razorpay()
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
                        resp = new StringBuilder(request.GetRawBodyStringAsync().Result);
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
            var Is = ml.LogCallBackRequestBool(callbackAPIReq).Result;
            try
            {
                if (callbackAPIReq.Content.Contains("payload"))
                {
                    var razorPayCallbackResp = JsonConvert.DeserializeObject<RazorPayCallbackResp>(callbackAPIReq.Content);
                    var pgml = new PaymentGatewayML(_accessor, _env);
                    var _res = pgml.UpdateRazorPaySuccess(razorPayCallbackResp);

                }
            }
            catch (Exception ex)
            {

            }
            //RazorPayCallbackResp razorPayCallbackResp
            return Ok();
        }
        [HttpPost]
        public async Task<IActionResult> AggrePay([FromForm] AggrePayResponse aggrePayResponse)
        {
            var TID = 0;
            var TransactionID = string.Empty;
            if (Validate.O.IsNumeric(aggrePayResponse.order_id ?? string.Empty))
            {
                TID = Convert.ToInt32(aggrePayResponse.order_id);
            }
            IPaymentGatewayML paymentGatewayML = new PaymentGatewayML(_accessor, _env);
            await paymentGatewayML.SavePGTransactionLog(PaymentGatewayType.AGRPAY, TID, (aggrePayResponse != null ? JsonConvert.SerializeObject(aggrePayResponse) : string.Empty), TransactionID, string.Empty, RequestMode.API, false, 0, string.Empty).ConfigureAwait(false);
            var Is = paymentGatewayML.UpdateFromAggrePayCallback(aggrePayResponse);
            StringBuilder html = new StringBuilder(@"<html><head><script>
                                (()=>{
                                        var obj={TID:""{TID}"",Amount:""{Amount}"",TransactionID:""{TransactionID}"",statuscode:""{status}"",reason:""{reason}"",origin:""addMoney""}
                                        localStorage.setItem('obj', JSON.stringify(obj));
                                        window.close()
                                   })();</script></head><body><h6>Redirect to site.....</h6></body></html>");


            html.Replace("{TID}", aggrePayResponse.order_id ?? string.Empty);
            html.Replace("{Amount}", aggrePayResponse.amount.ToString());
            html.Replace("{TransactionID}", aggrePayResponse.transaction_id ?? string.Empty);
            html.Replace("{status}", Is.Statuscode.ToString());
            html.Replace("{reason}", aggrePayResponse.response_message);
            return Content(html.ToString(), contentType: "text/html; charset=utf-8");
        }
        public async Task<IActionResult> AggrePayHook(AggrePayResponse aggrePayResponse)
        {
            var TID = 0;
            var TransactionID = string.Empty;
            if (Validate.O.IsNumeric(aggrePayResponse.order_id ?? string.Empty))
            {
                TID = Convert.ToInt32(aggrePayResponse.order_id);
            }
            IPaymentGatewayML paymentGatewayML = new PaymentGatewayML(_accessor, _env);
            await paymentGatewayML.SavePGTransactionLog(PaymentGatewayType.AGRPAY, TID, (aggrePayResponse != null ? JsonConvert.SerializeObject(aggrePayResponse) : string.Empty), TransactionID, string.Empty, RequestMode.API, false, 0, string.Empty).ConfigureAwait(false);
            var Is = paymentGatewayML.UpdateFromAggrePayCallback(aggrePayResponse);
            return Ok();
            // return View("PGConfirmation", Is.Statuscode == ErrorCodes.One);
            //return Json(aggrePayResponse);
        }
        public async Task<IActionResult> AggrePayApp([FromForm] AggrePayResponse aggrePayResponse)
        {
            var TID = 0;
            var TransactionID = string.Empty;
            if (Validate.O.IsNumeric(aggrePayResponse.order_id ?? string.Empty))
            {
                TID = Convert.ToInt32(aggrePayResponse.order_id);
            }
            IPaymentGatewayML paymentGatewayML = new PaymentGatewayML(_accessor, _env);
            await paymentGatewayML.SavePGTransactionLog(PaymentGatewayType.AGRPAY, TID, (aggrePayResponse != null ? JsonConvert.SerializeObject(aggrePayResponse) : string.Empty), TransactionID, string.Empty, RequestMode.API, false, 0, string.Empty).ConfigureAwait(false);
            return Ok("Final");
        }
        public IActionResult ConvertSize(string name, int size)
        {
            var imgPath = DOCType.DocFilePath + name;
            var imgBase64 = ConverterHelper.O.ConvertImagebase64(imgPath);
            byte[] b = Validate.O.TryToConnvertBase64String(imgBase64);
            var fileSize = Validate.O.CalculateSizeOfBase64File(imgBase64);
            int increesedPercent = 0;
            if (fileSize > size * 1024)
            {
                ImageResizer imageResizer = new ImageResizer(size * 1024, b, Validate.O.GetIfAllowedExtensionIsExists(b).ToLower());
                b = imageResizer.ScaleImage();
            }
            return File(b, "image/png");
        }
        public async Task<IActionResult> VIANStatusCheck(int MVBID, string MVBToken, string AgentID, string VIAN)
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
                        resp = new StringBuilder(request.GetRawBodyStringAsync().Result);

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
            callbackAPIReq.Content = resp.ToString();
            var ml = new CallbackML(_accessor, _env);
            var Is = ml.LogCallBackRequestBool(callbackAPIReq).Result;
            //VIANCallBackRequest
            IPaymentGatewayML paymentGatewayML = new PaymentGatewayML(_accessor, _env);
            var serviceRes = paymentGatewayML.CallVIANService(new AppCode.Model.ProcModel.VIANCallbackRequest
            {
                MVBID = MVBID,
                AgentID = AgentID,
                MVBToken = MVBToken,
                Operation = "ST",
                VIAN = VIAN
            });
            return Json(serviceRes);
        }
        public async Task<IActionResult> VIANCredit(int MVBID, string MVBToken, string AgentID, string VIAN, decimal Amount)
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
                        resp = new StringBuilder(request.GetRawBodyStringAsync().Result);

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
            callbackAPIReq.Content = resp.ToString();
            var ml = new CallbackML(_accessor, _env);
            var Is = ml.LogCallBackRequestBool(callbackAPIReq).Result;
            IPaymentGatewayML paymentGatewayML = new PaymentGatewayML(_accessor, _env);
            var serviceRes = paymentGatewayML.CallVIANService(new AppCode.Model.ProcModel.VIANCallbackRequest
            {
                MVBID = MVBID,
                AgentID = AgentID,
                MVBToken = MVBToken,
                Operation = "CR",
                VIAN = VIAN,
                Amount = Amount
            });
            return Json(serviceRes);
        }
        public async Task<IActionResult> UPIGatewayRedirect(int TID)
        {
            return Ok("Transaction Intitated wait for 5 minutes");
        }
        public async Task<IActionResult> UPIGatewayHook()
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
                        resp = new StringBuilder(request.GetRawBodyStringAsync().Result);
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
            var Is = ml.LogCallBackRequestBool(callbackAPIReq).Result;
            if (Is && callbackAPIReq.Content.Contains("upi_txn_id"))
            {
                var uPIGatewayRes = JsonConvert.DeserializeObject<UPIGatewayResponse>(callbackAPIReq.Content);
                if (uPIGatewayRes != null)
                {
                    if (uPIGatewayRes.client_txn_id > 0)
                    {
                        var req = new UpdatePGTransactionRequest
                        {
                            TID = uPIGatewayRes.client_txn_id,
                            PGID = PaymentGatewayType.UPIGATEWAY,
                            LiveID = uPIGatewayRes.upi_txn_id,
                            PaymentModeSpKey = PaymentGatewayTranMode.UPIICI,
                            Type = RechargeRespType.PENDING
                        };
                        if (uPIGatewayRes.status == "failure")
                        {
                            req.Type = RechargeRespType.FAILED;
                        }
                        else if (uPIGatewayRes.status == "success")
                        {
                            req.Type = RechargeRespType.SUCCESS;
                        }
                        var pgml = new PaymentGatewayML(_accessor, _env);
                        var _res = pgml.UpdateUPIGatewayStatus(req);
                    }
                }

            }
            return Ok();
        }
        public IActionResult RazorPaySmartCollect()
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
                        resp = new StringBuilder(request.GetRawBodyStringAsync().Result);

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
            callbackAPIReq.Content = resp.ToString();
            var ml = new CallbackML(_accessor, _env);
            var Is = ml.LogCallBackRequestBool(callbackAPIReq).Result;

            if (Is)
            {
                if (!string.IsNullOrEmpty(callbackAPIReq.Content))
                {
                    if (callbackAPIReq.Content.Contains("captured"))
                    {
                        var RzpayResp = JsonConvert.DeserializeObject<RZRSmartPayCallbackModel>(callbackAPIReq.Content);
                        if (RzpayResp != null)
                        {
                            if (RzpayResp.payload != null)
                            {
                                if (RzpayResp.payload.payment != null)
                                {
                                    if (RzpayResp.payload.payment.entity != null)
                                    {
                                        if (RzpayResp.payload.payment.entity.status != null)
                                        {
                                            IciciML objML = new IciciML(_accessor, _env, false);
                                            var res = objML.ValidateRazorpayData(RzpayResp.payload.payment.entity.customer_id, RzpayResp.payload.payment.entity.amount, RzpayResp.payload.payment.entity.id, RzpayResp.payload.payment.entity.status, RzpayResp.payload.payment.entity.id);
                                        }
                                    }
                                }
                            }

                        }
                    }
                }
            }
            return Ok("Razorpay");
        }

        [HttpPost]
        public async Task<IActionResult> CashFreeStatusCheck(string TID)
        {
            IPaymentGatewayML _ml = new PaymentGatewayML(_accessor, _env);
            var res = _ml.CashFreePgStatusCheck(string.Empty, string.Empty, TID);
            return Json(new { status = res.status, TID = res.TID, Amount = res.orderAmount, TransactionID = res.TransactionId, statuscode = res.StatusCode });
        }
        [HttpGet]
        [Route("/CashFreereturn")]
        public async Task<IActionResult> CashFreereturn(string order_id, string order_token)
        {
            /* Check status from db*/
            IPaymentGatewayML _ml = new PaymentGatewayML(_accessor, _env);
            var _res = _ml.CashFreePgStatusCheck(order_id, order_token);
            StringBuilder html = new StringBuilder(@"<html><head><script>
                                (()=>{
                                        var obj={TID:""{TID}"",Amount:""{Amount}"",TransactionID:""{TransactionID}"",status:""{status}"",statuscode:""{statuscode}"",reason:""{reason}"",origin:""addMoney"",gateway:""CashFree""}
                                        localStorage.setItem('obj', JSON.stringify(obj));
                                        window.close()
                                   })();</script></head><body><h6>Redirect to site.....</h6></body></html>");
            html.Replace("{TID}", _res.TID);
            html.Replace("{Amount}", _res.orderAmount.ToString());
            html.Replace("{TransactionID}", _res.TransactionId);
            html.Replace("{status}", _res.status);
            html.Replace("{statuscode}", _res.StatusCode.ToString());
            html.Replace("{reason}", String.Empty);
            return Content(html.ToString(), contentType: "text/html; charset=utf-8");
        }

        [HttpGet, HttpPost]
        [Route("/CashFreenotify")]
        public async Task<IActionResult> CashFreenotify(CashfreeCallbackResponse param)//(string orderId,decimal orderAmount,int referenceId,string txStatus,string paymentMode,string txMsg,string txTime,string signature)
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
                        resp = new StringBuilder(request.GetRawBodyStringAsync().Result);
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
                var cMl = new CallbackML(_accessor, _env);
                //cMl.ErrorLog(GetType().Name, "CashFreenotify", resp.ToString());
            }
            callbackAPIReq.Content = WebUtility.UrlDecode(resp.ToString());
            var ml = new CallbackML(_accessor, _env);
            var Is = ml.LogCallBackRequestBool(callbackAPIReq).Result;
            IPaymentGatewayML _ml = new PaymentGatewayML(_accessor, _env);
            var _res = _ml.UpdateCashfreeResponse(param);
            return Ok();
        }

        [Route("PGCallback/CashFreeCollect")]
        public IActionResult CashFreeCollect()
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
                        resp = new StringBuilder(request.GetRawBodyStringAsync().Result);

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
            callbackAPIReq.Content = resp.ToString();
            var ml = new CallbackML(_accessor, _env);
            var Is = ml.LogCallBackRequestBool(callbackAPIReq).Result;

            if (Is)
            {
                if (!string.IsNullOrEmpty(callbackAPIReq.Content))
                {
                    if (callbackAPIReq.Content.Contains("vAccountNumber"))
                    {
                        var cashFreeResp = JsonConvert.DeserializeObject<CashfreeColectResponse>(callbackAPIReq.Content);
                        if (cashFreeResp != null)
                        {
                            if (cashFreeResp.CashFreeEvent != null)
                            {
                                IciciML objML = new IciciML(_accessor, _env, false);
                                var res = objML.ValidateCashfreeData(cashFreeResp.vAccountNumber, cashFreeResp.amount, cashFreeResp.referenceId.ToString(), cashFreeResp.CashFreeEvent, cashFreeResp.utr ?? string.Empty);
                            }

                        }
                    }
                }
            }
            return Ok("Cashfree");
        }

        [Route("/PGCallback/PayUnotify")]
        public async Task<IActionResult> PayUnotify(PayUResponse request)
        {
            IPaymentGatewayML paymentGatewayML = new PaymentGatewayML(_accessor, _env);
            var pResp = paymentGatewayML.UpdateFromPayUCallback(request);
            StringBuilder html = new StringBuilder(@"<html><head><script>
                                (()=>{
                                        var obj={TID:""{TID}"",Amount:""{Amount}"",TransactionID:""{TransactionID}"",statuscode:""{status}"",reason:""{reason}"",origin:""addMoney"",gateway:""PayU""}
                                        localStorage.setItem('obj', JSON.stringify(obj));
                                        window.close()
                                   })();</script></head><body><h6>Redirect to site.....</h6></body></html>");
            html.Replace("{TID}", request.txnid);
            html.Replace("{Amount}", request.amount.ToString());
            html.Replace("{TransactionID}", request.mihpayid);
            html.Replace("{status}", pResp.Statuscode.ToString());
            html.Replace("{reason}", request.field9);
            return Content(html.ToString(), contentType: "text/html; charset=utf-8");
        }
    }
}
