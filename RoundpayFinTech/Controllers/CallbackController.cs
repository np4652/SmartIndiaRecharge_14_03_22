using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Fintech.AppCode.Configuration;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using RoundpayFinTech.AppCode.Classes;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.MiddleLayer;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.StaticModel.MoneyTransfer;
using RoundpayFinTech.AppCode.ThirdParty.GIBL;
using RoundpayFinTech.AppCode.ThirdParty.WhatsappAPI;

namespace RoundpayFinTech.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public partial class CallbackController : Controller
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        private readonly IUnitOfWork uow;
        public CallbackController(IHttpContextAccessor accessor, IHostingEnvironment env, IUnitOfWork UOW)
        {
            _accessor = accessor;
            _env = env;
            uow = UOW;
        }
        [Route("Callback/{rpapid}")]
        public async Task<IActionResult> Index(int rpapid)
        {
            StringBuilder resp = new StringBuilder("");
            var request = HttpContext.Request;
            bool IsIPValidation = true;
            var callbackAPIReq = new CallbackData
            {
                Method = request.Method,
                APIID = rpapid,
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
                        IsIPValidation = IsIPValidate(request);
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
            ICallbackML ml = new CallbackML(_accessor, _env);
            return Ok(await ml.LogCallBackRequest(callbackAPIReq, IsIPValidation).ConfigureAwait(false));
        }
        [HttpPost]
        [Route("Callback/Outlet/CheckBal")]
        [Route("Outlet/CheckBal")]
        public IActionResult CheckBal(string RAILID)
        {
            var cllbak = new CallbackML(_accessor, _env);
            cllbak.SaveCallBack(RAILID);
            return Json(cllbak.GetUserBal(15, RAILID));
        }
        [HttpPost]
        [Route("Callback/Outlet/Transaction")]
        [Route("Outlet/Transaction")]
        public IActionResult Transaction(string UserID, string TranID, DateTime TranDate, decimal TranAmount, decimal PGCHARGE, string BalanceRequestResponse, string CoRelationID)
        {
            var _req = new ServiceTransactionRequest
            {
                AccountNo = TranID,
                AmountR = TranAmount + PGCHARGE,
                OutletID = UserID,
                APIID = 15,
                OPType = 18,
                RequestModeID = RequestMode.PANEL,
                RequestRespones = BalanceRequestResponse,
                VenderID = CoRelationID
            };
            var cllbak = new CallbackML(_accessor, _env);
            cllbak.SaveCallBack(JsonConvert.SerializeObject(_req).ToString());
            return Json(cllbak.ServiceTransaction(_req));
        }
        [HttpPost]
        [Route("Callback/Outlet/RequestOTP")]
        [Route("Outlet/RequestOTP")]
        public IActionResult RequestOTP(string MerchantID, string UserID, string Password, string ApiRequestID)
        {
            var cllbak = new CallbackML(_accessor, _env);
            return Json(cllbak.RequestOTP(MerchantID, UserID, Password, ApiRequestID));
        }
        #region Shopping
        [HttpGet]
        [Route("Callback/Shopping/GetOTP")]
        [Route("Shopping/GetOTP")]
        public IActionResult GetOTP(string PartnerMobile, string AgentMobile, string SessionID)
        {
            IShoppingCallback cllbak = new CallbackML(_accessor, _env);
            var res = cllbak.GenerateShoppingOTP(new ShoppingOTPReq
            {
                WLMobile = PartnerMobile,
                UserMobile = AgentMobile,
                RequestSession = SessionID
            });
            return Json(new { res.Statuscode, res.Msg, RefferenceID = res.CommonInt });
        }
        [HttpGet]
        [Route("Callback/Shopping/MatchOTP")]
        [Route("Shopping/MatchOTP")]
        public IActionResult MatchOTP(string OTP, string AgentMobile, string SessionID, int RefferenceID)
        {
            IShoppingCallback cllbak = new CallbackML(_accessor, _env);
            var res = cllbak.MatchShoppingOTP(new ShoppingOTPReq
            {
                UserMobile = AgentMobile,
                RequestSession = SessionID,
                OTP = OTP,
                RefferenceID = RefferenceID
            });
            return Json(new { res.Statuscode, res.Msg, Token = res.CommonStr });
        }
        [HttpGet]
        [Route("Callback/Shopping/Debit/{id}")]
        [Route("Shopping/Debit/{id}")]
        public IActionResult Shopping(int id, string Token, string ApiRequestID, decimal Amount, string OTP, string SessionID, int RefferenceID)
        {
            var cllbak = new CallbackML(_accessor, _env);
            var Apires = new ResponseStatusBalnace
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            var _req = new ServiceTransactionRequest
            {
                AccountNo = ApiRequestID,
                AmountR = Amount,
                OPType = OPTypes.Shopping,
                RequestModeID = RequestMode.PANEL,
                VenderID = ApiRequestID,
                APIRequestID = ApiRequestID,
                APIID = id,
                Token = Token,
                RefferenceID = RefferenceID,
                OTP = OTP,
                RequestSession = SessionID
            };
            cllbak.SaveCallBack(JsonConvert.SerializeObject(_req).ToString());
            Apires = cllbak.ServiceTransaction(_req);
            return Json(new { Apires.Statuscode, Apires.Msg, Apires.TransactionID, Apires.Balance });
        }
        [HttpGet]
        [Route("Callback/Shopping/Balance")]
        [Route("Shopping/Balance")]
        public IActionResult GetBalance(string Token)
        {
            var cllbak = new CallbackML(_accessor, _env);
            var Apires = new ResponseStatusBalnace
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            var _req = new ServiceTransactionRequest
            {
                OPType = OPTypes.Shopping,
                Token = Token,
                IsBalance = true
            };
            cllbak.SaveCallBack(JsonConvert.SerializeObject(_req).ToString());
            Apires = cllbak.ServiceTransaction(_req);
            return Json(new { Apires.Statuscode, Apires.Msg, Apires.TransactionID, Apires.Balance });
        }
        #endregion


        [Route("Callback/GI/APIWaletDebit")]
        [Route("GI/APIWaletDebit")]
        public IActionResult APIWaletDebit(GIAPIWaleUpdateModel gIAPIWaleUpdate)
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
            var resAP = ml.LogCallBackRequestBool(callbackAPIReq).Result;

            if (gIAPIWaleUpdate.status == ErrorCodes.One)
            {
                var req = new GIUpdateRequestModel
                {
                    APICode = APICode.APIWALE,
                    APIOpCode = gIAPIWaleUpdate.insurancetype ?? string.Empty,
                    RechType = gIAPIWaleUpdate.ServiceCode ?? string.Empty,
                    TransactionID = gIAPIWaleUpdate.SessionKey ?? string.Empty,
                    LiveID = gIAPIWaleUpdate.TranRef,
                    VendorID = gIAPIWaleUpdate.RelatedRef,
                    AccountNo = gIAPIWaleUpdate.RelatedRef,
                    ActualAmount = gIAPIWaleUpdate.ActualAmount,
                    ODAmount = gIAPIWaleUpdate.OD,
                    OutletID = gIAPIWaleUpdate.UserCode
                };
                GeneralInsuranceML generalInsuranceML = new GeneralInsuranceML(_accessor, _env);
                var res = generalInsuranceML.DoGITransaction(req);
                return Json(new { RESPONSESTATUS = res.Statuscode.ToString(), message = res.Msg });
            }
            return Json(new { RESPONSESTATUS = "0", message = "Invalid debit request" });
        }

        [Route("Callback/GI/APIWaletCredit")]
        [Route("GI/APIWaletCredit")]
        public IActionResult APIWaletCredit(GIAPIWaleUpdateModel gIAPIWaleUpdate)
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
            var resD = ml.LogCallBackRequestBool(callbackAPIReq).Result;

            var req = new GIUpdateRequestModel
            {
                APICode = APICode.APIWALE,
                APIOpCode = gIAPIWaleUpdate.insurancetype ?? string.Empty,
                RechType = gIAPIWaleUpdate.ServiceCode ?? string.Empty,
                TransactionID = gIAPIWaleUpdate.SessionKey ?? string.Empty,
                LiveID = gIAPIWaleUpdate.TranRef,
                VendorID = gIAPIWaleUpdate.RelatedRef,
                AccountNo = gIAPIWaleUpdate.RelatedRef,
                ActualAmount = gIAPIWaleUpdate.ActualAmount,
                ODAmount = gIAPIWaleUpdate.OD,
                OutletID = gIAPIWaleUpdate.UserCode
            };
            GeneralInsuranceML generalInsuranceML = new GeneralInsuranceML(_accessor, _env);
            var res = generalInsuranceML.DoGIUpdateTransaction(req);
            return Json(new { RESPONSESTATUS = res.Statuscode.ToString(), message = res.Msg });
        }

        [Route("Callback/GI/GIBLWalletDebit")]
        [Route("GI/GIBLWalletDebit")]
        public IActionResult GIBLWalletDebit()
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
            GIBLPremDeductResponse gIBLPremDeductResponse = new GIBLPremDeductResponse
            {
                status = 0,
                message = RechargeRespType._FAILED
            };
            if (Is)
            {
                var mlObj = new GIBLML(_accessor, _env);
                var serializedRes = mlObj.SerializeDebitRequest(callbackAPIReq.Content);
                if (serializedRes.Status > 1)
                {
                    var GIML = new GeneralInsuranceML(_accessor, _env);
                    var giRes = GIML.DoGIDebit(serializedRes);
                    gIBLPremDeductResponse.status = giRes.Statuscode == ErrorCodes.One ? 1 : 0;
                    gIBLPremDeductResponse.message = gIBLPremDeductResponse.status == ErrorCodes.One ? RechargeRespType._SUCCESS : gIBLPremDeductResponse.message;
                }
            }
            return Json(gIBLPremDeductResponse);
        }

        [Route("Callback/GI/GIBLWalletCredit")]
        [Route("GI/GIBLWalletCredit")]
        public IActionResult GIBLWalletCredit()
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
            GIBLPremDeductResponse gIBLPremDeductResponse = new GIBLPremDeductResponse
            {
                status = 0,
                message = RechargeRespType._FAILED
            };
            if (Is)
            {
                var mlObj = new GIBLML(_accessor, _env);
                var serializedRes = mlObj.SerializeUpdateRequest(callbackAPIReq.Content);
                if (serializedRes.Status > 1)
                {
                    var GIML = new GeneralInsuranceML(_accessor, _env);
                    var giRes = GIML.DoGIUpdateTransaction(serializedRes);
                    gIBLPremDeductResponse.status = giRes.Statuscode == ErrorCodes.One ? 1 : 0;
                    gIBLPremDeductResponse.message = gIBLPremDeductResponse.status == ErrorCodes.One ? RechargeRespType._SUCCESS : gIBLPremDeductResponse.message;
                }
            }
            return Json(gIBLPremDeductResponse);
        }

        [Route("Callback/WhatsappUpdate")]
        [Route("WhatsappUpdate")]
        public IActionResult WhatsappUpdate()
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

            var res = ml.LogCallBackRequestBool(callbackAPIReq).Result;
            try
            {
                WhatsappReceiveMsgResp jsonresponse = JsonConvert.DeserializeObject<WhatsappReceiveMsgResp>(resp.ToString());
                var retresp = ml.ProcSaveWhatsappReceiceMessage(jsonresponse);
                /* Auto Reply*/
                if (retresp.Statuscode == 1)
                {
                    AutoReply(retresp.text, retresp.waId, retresp.senderName, WhatsappAPICode.ALERTHUB, retresp.CCID, retresp.CCName, retresp.SenderNO, retresp.GroupID, retresp.conversationId, retresp.QuoteMsg, retresp.ReplyJID);
                    PendingNotifications p = new PendingNotifications();
                    if (retresp.FormatType == "Dispute")
                    {
                        var _req = new RefundRequestReq
                        {
                            refundRequest = new RefundRequest
                            {
                                TID = Convert.ToInt32(retresp.TID),
                                RPID = retresp.TransactionId,
                                UserID = retresp.UserId
                            }
                        };
                        IAppReportML rml = new ReportML(_accessor, _env, false);
                        rml.MarkDisputeApp(_req);
                        p.PendingRefundNotificationAsync(_accessor, _env, retresp.UserId, true, 0, string.Empty, jsonresponse.text);
                    }
                    if (retresp.FormatType == "RechargePending")
                    {
                        p.PendingRechargeNotificationAsync(_accessor, _env, retresp.UserId, true, string.Empty, jsonresponse.text);
                    }
                }
                /* End Of Auto Reply*/
            }
            catch { }
            return Json(res);
        }

        [Route("Callback/Wati/{wpsender}")]
        [Route("Wati/{wpsender}")]
        public IActionResult Wati(int wpsender)
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

            var res = ml.LogCallBackRequestBool(callbackAPIReq).Result;
            try
            {
                WhatsappReceiveMsgResp jsonresponse = JsonConvert.DeserializeObject<WhatsappReceiveMsgResp>(resp.ToString());
                jsonresponse.SenderNoID = wpsender;
                // In Wati Api id is Used as conversationId, id is uniq as convertation
                jsonresponse.conversationId = jsonresponse.id;
                var retresp = ml.ProcSaveWhatsappReceiceMessage(jsonresponse);
                /* Auto Reply*/
                if (retresp.Statuscode == 1)
                {
                    AutoReply(retresp.text, retresp.waId, retresp.senderName, WhatsappAPICode.ALERTHUB, retresp.CCID, retresp.CCName, retresp.SenderNO, retresp.GroupID, retresp.conversationId, retresp.QuoteMsg, retresp.ReplyJID);
                    PendingNotifications p = new PendingNotifications();
                    if (retresp.FormatType == "Dispute")
                    {
                        var _req = new RefundRequestReq
                        {
                            refundRequest = new RefundRequest
                            {
                                TID = Convert.ToInt32(retresp.TID),
                                RPID = retresp.TransactionId,
                                UserID = retresp.UserId,
                                LoginType = 1
                            },
                            LoginTypeID = 1,
                            LoginID = retresp.UserId

                        };
                        IAppReportML rml = new ReportML(_accessor, _env, false);
                        rml.MarkDisputeApp(_req);
                        p.PendingRefundNotificationAsync(_accessor, _env, retresp.UserId, true, retresp.TransactionAPIId, string.Empty, jsonresponse.text);
                    }
                    if (retresp.FormatType == "RechargePending")
                    {
                        p.PendingRechargeNotificationAsync(_accessor, _env, retresp.UserId, true, string.Empty, jsonresponse.text);
                    }
                }
                /* End Of Auto Reply*/


            }
            catch { }
            return Json(res);
        }

        [Route("Callback/AlertHub/{wpsender}")]
        [Route("AlertHub/{wpsender}")]
        public IActionResult AlertHub(int wpsender)
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
            var res = ml.LogCallBackRequestBool(callbackAPIReq).Result;
            try
            {
                WhatsappAlertHubCallBack jsonresponse = JsonConvert.DeserializeObject<WhatsappAlertHubCallBack>(resp.ToString());
                jsonresponse.SenderNoID = wpsender;
                var retresp = ml.ProcSaveWhatsappReceiceMessageAlertHub(jsonresponse);
                /* Auto Reply*/
                if (retresp.Statuscode == 1)
                {
                    AutoReply(retresp.text, retresp.waId, retresp.senderName, WhatsappAPICode.ALERTHUB, retresp.CCID, retresp.CCName, retresp.SenderNO, retresp.GroupID, retresp.conversationId, retresp.QuoteMsg, retresp.ReplyJID);
                    PendingNotifications p = new PendingNotifications();
                    if (retresp.FormatType == "Dispute")
                    {
                        var _req = new RefundRequestReq
                        {
                            refundRequest = new RefundRequest
                            {
                                TID = Convert.ToInt32(retresp.TID),
                                RPID = retresp.TransactionId,
                                UserID = retresp.UserId,
                                LoginType = 1
                            },
                            LoginTypeID = 1,
                            LoginID = retresp.UserId
                        };
                        IAppReportML rml = new ReportML(_accessor, _env, false);
                        rml.MarkDisputeApp(_req);
                        p.PendingRefundNotificationAsync(_accessor, _env, retresp.UserId, true, retresp.TransactionAPIId, string.Empty, jsonresponse.Info.message);
                    }
                    if (retresp.FormatType == "RechargePending")
                    {
                        p.PendingRechargeNotificationAsync(_accessor, _env, retresp.UserId, true, string.Empty, jsonresponse.Info.message);
                    }
                }
                /* End Of Auto Reply*/
            }
            catch
            {
            }
            return Json(res);
        }

        [Route("Callback/WATeam/{wpsender}")]
        [Route("WATeam/{wpsender}")]
        public IActionResult WATeam(int wpsender)
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

            var res = ml.LogCallBackRequestBool(callbackAPIReq).Result;
            try
            {
                WATeamCallBackRes jsonresponse = JsonConvert.DeserializeObject<WATeamCallBackRes>(resp.ToString());
                jsonresponse.SenderID = wpsender;
                var retresp = ml.ProcSaveWhatsappReceiceMessageWATeam(jsonresponse);
                /* Auto Reply*/
                if (retresp.Statuscode == 1)
                {
                    AutoReply(retresp.text, retresp.waId, retresp.senderName, WhatsappAPICode.ALERTHUB, retresp.CCID, retresp.CCName, retresp.SenderNO, retresp.GroupID, retresp.conversationId, retresp.QuoteMsg, retresp.ReplyJID);
                    PendingNotifications p = new PendingNotifications();
                    if (retresp.FormatType == "Dispute")
                    {
                        var _req = new RefundRequestReq
                        {
                            refundRequest = new RefundRequest
                            {
                                TID = Convert.ToInt32(retresp.TID),
                                RPID = retresp.TransactionId,
                                UserID = retresp.UserId,
                                LoginType = 1
                            },
                            LoginTypeID = 1,
                            LoginID = retresp.UserId
                        };
                        IAppReportML rml = new ReportML(_accessor, _env, false);
                        rml.MarkDisputeApp(_req);
                        p.PendingRefundNotificationAsync(_accessor, _env, retresp.UserId, true, retresp.TransactionAPIId, string.Empty, jsonresponse.messages?.FirstOrDefault().text.body);
                    }
                    if (retresp.FormatType == "RechargePending")
                    {
                        p.PendingRechargeNotificationAsync(_accessor, _env, retresp.UserId, true, string.Empty, jsonresponse.messages?.FirstOrDefault().text.body);
                    }
                }
                /* End Of Auto Reply*/
            }
            catch
            {
            }
            return Json(res);
        }

        private void AutoReply(string WM, string MT, string SN, string apcde, int CCID, string CCName, string SenderNo, string GroupID, string ConversationID, string QuoteMsg, string ReplyJID)
        {
            //IWhatsappML whatsappML = new WhatsappML(_accessor, _env);
            var wc = new WhatsappConversation();
            {
                wc.ContactId = !string.IsNullOrEmpty(GroupID) ? GroupID : MT;
                wc.SenderName = SN;
                wc.Text = WM;
                wc.LoginTypeID = 1;
                wc.CCID = CCID;
                wc.CCName = CCName;
                wc.Type = "text";
                wc.Id = 0;
                wc.APICODE = apcde;
                wc.SenderNo = SenderNo;
                wc.conversationId = ConversationID;
                wc.QuoteMsg = QuoteMsg;
                wc.ReplyJID = ReplyJID;
            };
            //whatsappML.SendWhatsappSessionMessageAllAPI(wc, true);
            uow.whatsappML.SendWhatsappSessionMessageAllAPI(wc, true);
        }
        private bool IsIPValidate(HttpRequest request)
        {
            var querystring = request.Query;
            string[] matchingkeys = new string[] { "status", "srsid", "agentid", "opratorid", "msg", "lapubal" };
            int matchCount = 0;
            if (matchingkeys.Length <= querystring.Keys.Count)
            {
                foreach (var item in matchingkeys)
                {
                    foreach (var key in querystring.Keys)
                    {
                        if (item.ToLower() == key.ToString().ToLower())
                        {
                            matchCount++;
                            break;
                        }
                    }
                }
            }
            return !(matchCount == matchingkeys.Length);
        }


        [Route("Callback/SWMsgConvID/{TMsg}/{ConvID}")]
        [Route("SWMsgConvID/{TMsg}/{ConvID}")]
        public IActionResult SWMsgConvID(string TMsg, string ConvID)
        {
            var wc = new WhatsappConversation()
            {
                Text = TMsg,
                conversationId = ConvID

            };
            return Json(uow.whatsappML.SendWhatsappSessionMessageAllAPI(wc, true));
        }



        [Route("Callback/WhatsappService")]
        [Route("WhatsappService")]
        public IActionResult WhatsappService()
        {
            IWhatsappML whatsappML = new WhatsappML(_accessor, _env);
            dynamic v = whatsappML.WhatsappSenderNoService(true);
            return Ok(v);
        }


    }
}