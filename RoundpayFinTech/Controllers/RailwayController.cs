using Fintech.AppCode;
using Fintech.AppCode.Configuration;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using RoundpayFinTech.AppCode.MiddleLayer;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.StaticModel.MoneyTransfer;
using System;
using System.Text;

namespace RoundpayFinTech.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class RailwayController : Controller
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        private readonly IRailwayML _ml;
        public RailwayController(IHttpContextAccessor accessor, IHostingEnvironment env)
        {
            _accessor = accessor;
            _env = env;
            _ml = new RailwayML(_accessor, _env);
        }

        [HttpPost]
        [Route("IRLogin")]
        public IActionResult IRLogin(string encdata)
        {
            var responseSts = _ml.MatchRedirectedDomain();
            if (!responseSts.CommonBool && !string.IsNullOrEmpty(responseSts.CommonStr))
            {
                responseSts.CommonStr = responseSts.CommonStr + "/IRLogin";
                responseSts.CommonStr2 = encdata;
                responseSts.CommonStr3 = "encdata";
                return View("RedirectToPost", responseSts);
            }
            if (string.IsNullOrEmpty(encdata.Trim()))
            {
                ViewErrorModel errormodel = new ViewErrorModel
                {
                    ErrorCode = "403"
                };
                return RedirectToActionPermanent("UnAuthorizedAccess", errormodel);
            }
            var res = _ml.Decode(encdata);
            _ml.LogRailwayReqResp(new LogRailwayReqRespModel()
            {
                MethodName = "IRLogin",
                Request = encdata,
                Response = JsonConvert.SerializeObject(res)
            });
            if (res != null && res.StatusCode == ErrorCodes.One)
            {
                MaintainSession(res, true);
                return View(res);
            }
            else
            {
                ViewErrorModel errormodel = new ViewErrorModel
                {
                    ErrorCode = "403"
                };
                return RedirectToActionPermanent("UnAuthorizedAccess", errormodel);
            }
        }

        [HttpPost]
        [Route("IRLoginLive")]
        public IActionResult IRLoginLive(string encdata)
        {
            var responseSts = _ml.MatchRedirectedDomain();
            if (!responseSts.CommonBool && !string.IsNullOrEmpty(responseSts.CommonStr))
            {
                responseSts.CommonStr = responseSts.CommonStr + "/IRLoginLive";
                responseSts.CommonStr2 = encdata;
                responseSts.CommonStr3 = "encdata";
                return View("RedirectToPost", responseSts);
            }
            if (string.IsNullOrEmpty(encdata.Trim()))
            {
                ViewErrorModel errormodel = new ViewErrorModel
                {
                    ErrorCode = "403",
                    Msg="NO ENC DATA!"
                };
                return RedirectToActionPermanent("UnAuthorizedAccess", errormodel);
            }
            var res = _ml.Decode(encdata, true);
            _ml.LogRailwayReqResp(new LogRailwayReqRespModel()
            {
                MethodName = "IRLoginLive",
                Request = encdata,
                Response = JsonConvert.SerializeObject(res)
            });
            if (res != null && res.StatusCode == ErrorCodes.One)
            {
                res.IsLive = true;
                MaintainSession(res, true);
                return View("IRLogin", res);
            }
            else
            {
                var errormodel = new ViewErrorModel
                {
                    ErrorCode = "403",
                    Msg=res.Msg
                };
                return RedirectToActionPermanent("UnAuthorizedAccess", errormodel);
            }
        }

        [HttpPost]
        [Route("IRUserLogin")]
        public IActionResult IRUserLogin(IRViewModel model)
        {
            var res = new IRViewModel
            {
                StatusCode = ErrorCodes.Minus1,
                Msg = ErrorCodes.FAILED
            };
            if (InSession())
            {
                TempData["authAtmpt"] = Convert.ToInt32(TempData.Peek("authAtmpt")) + 1;
                if (model.LoginId < 0 || string.IsNullOrEmpty(model.mobile))
                {
                    model.StatusCode = ErrorCodes.Minus1;
                    model.Msg = ErrorCodes.InvalidLogin;
                    MaintainSession();
                    return View("IRLogin", model);
                }
                model.reservationId = TempData.Peek("resId").ToString();
                model.txnAmount = Convert.ToDecimal(TempData.Peek("amt"));
                model.IRSaveID = Convert.ToInt32(TempData.Peek("tran"));
                MaintainSession();
                res = _ml.ValidateIRLogin(model);
                if (res.StatusCode == ErrorCodes.One)
                {
                    return View("IRPayment", res);
                }
                else
                {
                    return View("IRLogin", res);
                }
            }
            _ml.LogRailwayReqResp(new LogRailwayReqRespModel()
            {
                MethodName = "IRUserLogin",
                Request = JsonConvert.SerializeObject(model),
                Response = JsonConvert.SerializeObject(res)
            });
            ViewErrorModel errormodel = new ViewErrorModel { ErrorCode = "401",Msg=res.Msg };
            return RedirectToActionPermanent("UnAuthorizedAccess", errormodel);
        }
        [HttpPost]
        [Route("ResendIROtp")]
        public IActionResult ResendOtp()
        {
            var req = new IRViewModel();
            var res = new IRViewModel
            {
                StatusCode = ErrorCodes.Minus1,
                Msg = ErrorCodes.FAILED
            };
            if (InSession())
            {
                req.reservationId = TempData.Peek("resId").ToString();
                req.txnAmount = Convert.ToDecimal(TempData.Peek("amt"));
                req.IRSaveID = Convert.ToInt32(TempData.Peek("tran"));
                MaintainSession();
                var mlResp = _ml.ReGenerateOTP();
                res.StatusCode = mlResp.Statuscode;
                res.Msg = "OTP resent successfully";
                return Json(res);
            }
            _ml.LogRailwayReqResp(new LogRailwayReqRespModel()
            {
                MethodName = "ResendOtp",
                Request = JsonConvert.SerializeObject(req),
                Response = JsonConvert.SerializeObject(res)
            });
            ViewErrorModel errormodel = new ViewErrorModel { ErrorCode = "401" };
            return RedirectToActionPermanent("UnAuthorizedAccess", errormodel);
        }
        [HttpPost]
        public IActionResult ProcessTransaction(IRViewModel model)
        {
            if (InSession())
            {
                if (!model.IsConfirm)
                {
                    model.StatusCode = ErrorCodes.Minus1;
                    model.Msg = "Please check \"I Agree\" checkbox to proceed.";
                    model.reservationId = TempData.Peek("resId").ToString();
                    model.txnAmount = Convert.ToDecimal(TempData.Peek("amt"));
                    model.IRSaveID = Convert.ToInt32(TempData.Peek("tran"));
                    MaintainSession();
                    return View("IRPayment", model);
                }
                if (string.IsNullOrEmpty(model.otp))
                {
                    model.StatusCode = ErrorCodes.Minus1;
                    model.Msg = nameof(ErrorCodes.Invalid_OTP).Replace("_", " ");
                    TempData["otpAtmpt"] = Convert.ToInt32(TempData.Peek("otpAtmpt")) + 1;
                    model.reservationId = TempData.Peek("resId").ToString();
                    model.txnAmount = Convert.ToDecimal(TempData.Peek("amt"));
                    model.IRSaveID = Convert.ToInt32(TempData.Peek("tran"));
                    MaintainSession();
                    return View("IRPayment", model);
                }
                TempData["otpAtmpt"] = Convert.ToInt32(TempData.Peek("otpAtmpt")) + 1;
                model.reservationId = TempData.Peek("resId").ToString();
                model.txnAmount = Convert.ToDecimal(TempData.Peek("amt"));
                model.IRSaveID = Convert.ToInt32(TempData.Peek("tran"));
                MaintainSession();
                var res = _ml.IRProcessRequest(model);
                if (res.StatusCode != ErrorCodes.One && res.Msg == nameof(ErrorCodes.Invalid_OTP).Replace("_", " "))
                {
                    return View("IRPayment", res);
                }
                _ml.LogRailwayReqResp(new LogRailwayReqRespModel()
                {
                    MethodName = "ProcessTransaction",
                    Request = JsonConvert.SerializeObject(model),
                    Response = JsonConvert.SerializeObject(res)
                });
                var saveModel = new IRSaveModel
                {
                    reservationId = TempData.Peek("resId").ToString(),
                    txnAmount = Convert.ToDecimal(TempData.Peek("amt")),
                    StatusCode = res.StatusCode,
                    IRSaveID = Convert.ToInt32(TempData.Peek("tran")),
                    Msg = res.Msg
                };
                var returnRes = _ml.TransactionReturnHit(saveModel, Convert.ToBoolean(TempData.Peek("IsLive")));
                returnRes.ErrorCode = "/images/IR/" + returnRes.ErrorCode + ".jpg";
                returnRes.Msg = returnRes.Msg + "[" + res.Msg + "]";
                return View("IR200", returnRes);
            }

            ViewErrorModel errormodel = new ViewErrorModel { ErrorCode = "401"};
            return RedirectToActionPermanent("UnAuthorizedAccess", errormodel);
        }
        [HttpPost]
        [Route("IRDeclineTransaction")]
        public IActionResult DeclineTransaction()
        {
            var req = new IRViewModel();
            var res = new IRViewModel
            {
                StatusCode = ErrorCodes.Minus1,
                Msg = ErrorCodes.FAILED
            };
            ReturnHit(ErrorCodes.Minus1);
            _ml.LogRailwayReqResp(new LogRailwayReqRespModel()
            {
                MethodName = "DeclineTransaction",
                Request = JsonConvert.SerializeObject(req),
                Response = JsonConvert.SerializeObject(res)
            });
            res.StatusCode = ErrorCodes.One;
            return Json(res);
        }

        [Route("doubleverification")]
        public string DoubleVerification(string encdata, string command)
        {
            string resp = "";
            var request = HttpContext.Request;
            _ml.LogRailwayReqResp(new LogRailwayReqRespModel()
            {
                MethodName = "doubleverificationLog",
                Request = encdata == null ? "null" : encdata,
                Response = request.Path + '_' + request.Method + '_' + command
            });
            if (string.IsNullOrEmpty(encdata))
            {
                return resp;
            }
            try
            {
                var res = _ml.DoubleVerificationDecode(encdata);
                IRSaveModel saveModel = new IRSaveModel()
                {
                    StatusCode = res.StatusCode,
                    reservationId = res.reservationId,
                    txnAmount = res.txnAmount,
                    IRSaveID = res.IRSaveID,
                    Msg = res.Msg,
                    merchantCode = res.MerchantCode,
                    BankTranId = res.BankTranId
                };
                var returnRes = _ml.TransactionReturnHit(saveModel);
                _ml.LogRailwayReqResp(new LogRailwayReqRespModel()
                {
                    MethodName = "doubleverificationSuccess",
                    Request = encdata == null ? "null" : encdata,
                    Response = JsonConvert.SerializeObject(returnRes)
                });
                resp = returnRes.EncResp;
            }
            catch (Exception ex)
            {
                _ml.LogRailwayReqResp(new LogRailwayReqRespModel()
                {
                    MethodName = "doubleverificationException",
                    Request = encdata == null ? "null" : encdata,
                    Response = ex.Message
                });
            }
            return resp;
        }

        [Route("doubleverificationLive")]
        public IActionResult DoubleVerificationLive(string encdata, string command)
        {
            string resp = "";
            var request = HttpContext.Request;
            _ml.LogRailwayReqResp(new LogRailwayReqRespModel()
            {
                MethodName = "doubleverificationLiveLog",
                Request = encdata == null ? "null" : encdata,
                Response = request.Path + '_' + request.Method + '_' + command
            });
            if (string.IsNullOrEmpty(encdata))
            {
                return Ok(resp);
            }
            try
            {
                var res = _ml.DoubleVerificationDecode(encdata, true);
                IRSaveModel saveModel = new IRSaveModel()
                {
                    StatusCode = res.StatusCode,
                    reservationId = res.reservationId,
                    txnAmount = res.txnAmount,
                    IRSaveID = res.IRSaveID,
                    Msg = res.Msg,
                    merchantCode = res.MerchantCode,
                    BankTranId = res.BankTranId
                };
                var returnRes = _ml.TransactionReturnHit(saveModel, true);
                _ml.LogRailwayReqResp(new LogRailwayReqRespModel()
                {
                    MethodName = "doubleverificationSuccess",
                    Request = encdata == null ? "null" : encdata,
                    Response = JsonConvert.SerializeObject(returnRes)
                });
                resp = returnRes.EncResp;
            }
            catch (Exception ex)
            {
                _ml.LogRailwayReqResp(new LogRailwayReqRespModel()
                {
                    MethodName = "doubleverificationException",
                    Request = encdata == null ? "null" : encdata,
                    Response = ex.Message
                });
            }
            return Ok(resp);
        }

        [HttpGet]
        [Route("UnAuthorizedAccess")]
        public IActionResult UnAuthorizedAccess(ViewErrorModel model = null, bool IsDeclined = false)
        {
            DestroySession();
            if ((model == null || model.ErrorCode == null) && IsDeclined)
            {
                ViewErrorModel errormodel = new ViewErrorModel { ErrorCode = "/images/IR/200.jpg", Msg = "Your transaction has been declined successfully" };
                return View(errormodel);
            }
            if (model == null || model.ErrorCode == null)
            {
                ViewErrorModel res = new ViewErrorModel { ErrorCode = "/images/IR/401.jpg", Msg = "Session Expired" };
                return View(res);
            }

            model.ErrorCode = "/images/IR/" + model.ErrorCode + ".jpg";
            return View(model);
        }
        [HttpGet]
        public IActionResult PageNotFound()
        {
            return View("IR404");
        }

        #region PartnerCallback
        [HttpGet]
        public IActionResult GenerateOTPRP(int RailID, int RefID)
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
            IRailwayML railwayML = new RailwayML(_accessor, _env, false);
            return Json(railwayML.GenerateRailOTPForCallback(APICode.RPFINTECH, RailID, RefID));
        }
        [HttpGet]
        public IActionResult ValidateOTPRP(int RailID, int RefID, string OTP)
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
            IRailwayML railwayML = new RailwayML(_accessor, _env, false);
            return Json(railwayML.MatchRailOTPFromCallback(RailID, RefID, OTP));
        }
        [HttpGet]
        public IActionResult DebitRP(string AgentID, string RPID, string AccountNo, decimal AmountR, int OutletID)
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
            IRailwayML railwayML = new RailwayML(_accessor, _env, false);
            return Json(railwayML.DoDebitFromCallback(AgentID, RPID, AccountNo, AmountR, OutletID, APICode.RPFINTECH));
        }
        #endregion

        #region PrivateMethods
        private void MaintainSession(IRViewModel req = null, bool IsNew = false)
        {
            if (IsNew)
            {
                TempData["resId"] = req.reservationId;
                TempData["amt"] = req.txnAmount;
                TempData["tran"] = req.IRSaveID;
                TempData["authAtmpt"] = 0;
                TempData["otpAtmpt"] = 0;
                TempData["InTime"] = DateTime.Now;
                TempData["IsLive"] = req.IsLive;
            }
            else
            {
                TempData.Keep("resId");
                TempData.Keep("amt");
                TempData.Keep("tran");
                TempData.Keep("authAtmpt");
                TempData.Keep("otpAtmpt");
                TempData.Keep("InTime");
                TempData.Keep("IsLive");
            }
        }

        private void DestroySession()
        {
            TempData.Remove("resId");
            TempData.Remove("amt");
            TempData.Remove("tran");
            TempData.Remove("authAtmpt");
            TempData.Remove("otpAtmpt");
            TempData.Remove("InTime");
            TempData.Remove("IsLive");
        }

        private bool InSession()
        {
            if (TempData["resId"] == null) return false;
            else if (TempData["amt"] == null) return false;
            else if (TempData["tran"] == null) return false;
            else if (TempData["authAtmpt"] == null) return false;
            else if (TempData["otpAtmpt"] == null) return false;
            else if (Convert.ToInt32(TempData["authAtmpt"]) > ErrorCodes.RailwayLoginAttempt)
            {
                ReturnHit(ErrorCodes.Minus1);
                return false;
            }
            else if (Convert.ToInt32(TempData["otpAtmpt"]) > ErrorCodes.RailwayLoginAttempt)
            {
                ReturnHit(ErrorCodes.Minus1);
                return false;
            }
            else return true;
        }

        private void ReturnHit(int status)
        {
            _ml.TransactionReturnHit(new IRSaveModel
            {
                reservationId = TempData["resId"].ToString(),
                txnAmount = Convert.ToDecimal(TempData["amt"]),
                StatusCode = status,
                IRSaveID = Convert.ToInt32(TempData["tran"]),
                Msg = ErrorCodes.FAILED
            });
        }
        #endregion

        [HttpPost]
        [Route("TestDV")]
        public string TestGetDV([FromBody] TestIRRequestModel req)
        {
            return _ml.TestPrepareEncodedResponse(req);
        }       




    }
}