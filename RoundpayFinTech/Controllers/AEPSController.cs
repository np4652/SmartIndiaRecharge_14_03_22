using Fintech.AppCode.Configuration;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.MiddleLayer;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.Model.SDK;
using RoundpayFinTech.AppCode.StaticModel.MoneyTransfer;
using RoundpayFinTech.AppCode.ThirdParty.Fingpay;
using RoundpayFinTech.AppCode.ThirdParty.Mosambee;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Validators;
namespace RoundpayFinTech.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class AEPSController : Controller
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;

        public AEPSController(IHttpContextAccessor accessor, IHostingEnvironment env)
        {
            _accessor = accessor;
            _env = env;
        }
        [HttpGet("AEPS/Roundpay/CheckStatus")]
        public async Task<IActionResult> CheckStatusRP(string Txntype, string TerminalId, string TransactionId, string Amount, string TxnStatus, string BankIIN, string OutletId, string BcId)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("AEPS/Roundpay/CheckStatus?Txntype=");
            sb.Append(Txntype ?? string.Empty);
            sb.Append("&TerminalId=");
            sb.Append(TerminalId ?? string.Empty);
            sb.Append("&TransactionId=");
            sb.Append(TransactionId ?? string.Empty);
            sb.Append("&Amount=");
            sb.Append(Amount ?? string.Empty);
            sb.Append("&TxnStatus=");
            sb.Append(TxnStatus ?? string.Empty);
            sb.Append("&BankIIN=");
            sb.Append(BankIIN ?? string.Empty);
            sb.Append("&OutletId=");
            sb.Append(OutletId ?? string.Empty);
            sb.Append("&BcId=");
            sb.Append(BcId ?? string.Empty);
            var ml = new CallbackML(_accessor, _env);
            var res = ml.LogCallBackRequestBool(new CallbackData { Content = sb.ToString(), Method = "Get", Path = "AEPS/Roundpay/CheckStatus" }).Result;
            IMiniBankML miniBankML = new AEPSML(_accessor, _env, false);
            var mbResp = await miniBankML.MakeMiniBankTransaction(new MiniBankTransactionServiceReq
            {
                AmountR = Validate.O.IsNumeric((Amount ?? string.Empty).Trim().Replace(".", "")) ? Convert.ToDecimal(Amount.Trim()) : 0M,
                APIOpCode = SPKeys.AepsCashWithdrawal,
                TXNType = Txntype,
                APICode = APICode.ROUNDPAY,
                LoginID = 1,
                OutletID = OutletId,
                RequestModeID = RequestMode.API,
                VendorID = TransactionId,
                BankIIN = BankIIN,
                BCID = BcId,
                TerminalID = TerminalId,
                RequestURL = sb.ToString()
            }).ConfigureAwait(false);

            var rResp = new RoundpayAEPSResp
            {
                STATUS = mbResp.Statuscode == ErrorCodes.One ? RechargeRespType._SUCCESS : RechargeRespType._FAILED,
                Balance = mbResp.Balance.ToString(),
                TransactionID = mbResp.TID.ToString(),
                VendorID = TransactionId,
                BCID = BcId,
                Msg = mbResp.Msg
            };
            await SaveLog(APICode.ROUNDPAY, "CheckStatusRP", JsonConvert.SerializeObject(rResp)).ConfigureAwait(false);
            return Json(rResp);
        }

        [HttpGet("AEPS/Roundpay/UpdateStatus")]
        public async Task<IActionResult> UpdateStatusRP(string TransactionId, string VenderId, string Status, string rrn)

        {
            StringBuilder sb = new StringBuilder();
            sb.Append("AEPS/Roundpay/UpdateStatus?TransactionId=");
            sb.Append(TransactionId ?? string.Empty);
            sb.Append("&VenderId=");
            sb.Append(VenderId ?? string.Empty);
            sb.Append("&TransactionId=");
            sb.Append(TransactionId ?? string.Empty);
            sb.Append("&Status=");
            sb.Append(Status ?? string.Empty);
            sb.Append("&rrn=");
            sb.Append(rrn ?? string.Empty);
            var ml = new CallbackML(_accessor, _env);
            var res = ml.LogCallBackRequestBool(new CallbackData { Content = sb.ToString(), Method = "Get", Path = "AEPS/Roundpay/UpdateStatus" }).Result;
            var sttaus = Status == RechargeRespType._SUCCESS ? RechargeRespType.SUCCESS : RechargeRespType.PENDING;
            sttaus = Status == RechargeRespType._FAILED || Status == RechargeRespType._FAILURE ? RechargeRespType.FAILED : sttaus;

            IMiniBankML miniBankML = new AEPSML(_accessor, _env, false);
            var mbResp = miniBankML.UpdateMiniBankResponse(new MiniBankTransactionServiceResp
            {
                TID = Validate.O.IsNumeric((TransactionId ?? string.Empty)) ? Convert.ToInt32(TransactionId) : 0,
                VendorID = VenderId,
                LT = 1,
                LoginID = 1,
                LiveID = rrn,
                Statuscode = sttaus,
                RequestPage = "Callback",
                Req = sb.ToString()
            });
            var rResp = new AEPSRoundpayStatusCommon
            {
                STATUS = mbResp.Statuscode == ErrorCodes.One ? RechargeRespType._SUCCESS : RechargeRespType._FAILED,
                Msg = mbResp.Msg
            };
            await SaveLog(APICode.ROUNDPAY, "UpdateStatusRP", JsonConvert.SerializeObject(rResp)).ConfigureAwait(false);
            return Json(rResp);
        }

        //http://roundpayapi.com/Api/AepsService.asmx/CheckStatus?Txntype=CW&Timestamp=21-11-2019%2010:34:46&BcId=BC406244598&TerminalId=17522&TransactionId=MH57507B4CA5E8405B8C54C3BA8761410D%7C2235XXXX5750&Amount=2000&TxnStatus=Pending&BankIIN=608112&TxnMedium=1&EndCustMobile=8509171215&RouteType=2:45.249.111.172
        [HttpGet("AEPS/Mahagram/CheckStatus")]
        public async Task<IActionResult> CheckStatusMG(string Txntype, string Timestamp, string BcId, string TerminalId, string TransactionId, string Amount, string TxnStatus, string BankIIN, int TxnMedium, string EndCustMobile, int RouteType)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("AEPS/Mahagram/CheckStatus?Txntype=");
            sb.Append(Txntype ?? string.Empty);
            sb.Append("&Timestamp=");
            sb.Append(Timestamp ?? string.Empty);
            sb.Append("&BcId=");
            sb.Append(BcId ?? string.Empty);
            sb.Append("&TerminalId=");
            sb.Append(TerminalId ?? string.Empty);
            sb.Append("&TransactionId=");
            sb.Append(TransactionId ?? string.Empty);
            sb.Append("&Amount=");
            sb.Append(Amount ?? string.Empty);
            sb.Append("&TxnStatus=");
            sb.Append(TxnStatus ?? string.Empty);
            sb.Append("&BankIIN=");
            sb.Append(BankIIN ?? string.Empty);
            sb.Append("&TxnMedium=");
            sb.Append(TxnMedium);
            sb.Append("&EndCustMobile=");
            sb.Append(EndCustMobile);
            sb.Append("&RouteType=");
            sb.Append(RouteType);
            var ml = new CallbackML(_accessor, _env);
            var res = ml.LogCallBackRequestBool(new CallbackData { Content = sb.ToString(), Method = "Get", Path = "AEPS/Mahagram/CheckStatus" }).Result;
            IMiniBankML miniBankML = new AEPSML(_accessor, _env, false);
            var mbResp = await miniBankML.MakeMiniBankTransaction(new MiniBankTransactionServiceReq
            {
                AmountR = Validate.O.IsNumeric((Amount ?? string.Empty).Replace(".", "")) ? Convert.ToDecimal(Amount) : 0M,
                APIOpCode = SPKeys.AepsCashWithdrawal,
                TXNType = Txntype,
                APICode = APICode.MAHAGRAM,
                LoginID = 1,
                OutletIDSelf = Validate.O.IsNumeric(TerminalId ?? string.Empty) ? Convert.ToInt32(TerminalId) : 0,
                RequestModeID = RequestMode.API,
                VendorID = TransactionId,
                BankIIN = BankIIN,
                BCID = BcId,
                TerminalID = TerminalId,
                OutletID = EndCustMobile,
                RequestURL = sb.ToString()
            }).ConfigureAwait(false);

            var rResp = new RoundpayAEPSResp
            {
                STATUS = mbResp.Statuscode == ErrorCodes.One ? RechargeRespType._SUCCESS : RechargeRespType._FAILED,
                Balance = mbResp.Balance.ToString(),
                TransactionID = mbResp.TID.ToString(),
                VendorID = TransactionId,
                BCID = BcId,
                Msg = mbResp.Msg
            };
            await SaveLog(APICode.MAHAGRAM, "CheckStatusMG", JsonConvert.SerializeObject(rResp));
            return Json(rResp);
        }
        //http://roundpayapi.com/Api/AepsService.asmx/UpdateStatus?TransactionId=A1911211034515372052&VenderId=MH57507B4CA5E8405B8C54C3BA8761410D%7C2235XXXX5750&Status=SUCCESS&BcCode=BC406244598&rrn=932510860182:45.249.111.172
        [HttpGet("AEPS/Mahagram/UpdateStatus")]
        public async Task<IActionResult> UpdateStatusMG(string TransactionId, string VenderId, string Status, string BcCode, string rrn)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("AEPS/Mahagram/UpdateStatus?TransactionId=");
            sb.Append(TransactionId ?? string.Empty);
            sb.Append("&VenderId=");
            sb.Append(VenderId ?? string.Empty);
            sb.Append("&TransactionId=");
            sb.Append(TransactionId ?? string.Empty);
            sb.Append("&Status=");
            sb.Append(Status ?? string.Empty);
            sb.Append("&BcCode=");
            sb.Append(BcCode ?? string.Empty);
            sb.Append("&rrn=");
            sb.Append(rrn ?? string.Empty);
            var ml = new CallbackML(_accessor, _env);
            var res = ml.LogCallBackRequestBool(new CallbackData { Content = sb.ToString(), Method = "Get", Path = "AEPS/Mahagram/UpdateStatus" }).Result;
            var sttaus = Status == RechargeRespType._SUCCESS ? RechargeRespType.SUCCESS : RechargeRespType.PENDING;
            sttaus = Status == RechargeRespType._FAILED || Status == RechargeRespType._FAILURE ? RechargeRespType.FAILED : sttaus;

            IMiniBankML miniBankML = new AEPSML(_accessor, _env, false);
            var mbResp = miniBankML.UpdateMiniBankResponse(new MiniBankTransactionServiceResp
            {
                TID = Validate.O.IsNumeric((TransactionId ?? string.Empty)) ? Convert.ToInt32(TransactionId) : 0,
                VendorID = VenderId,
                LT = 1,
                LoginID = 1,
                LiveID = rrn,
                Statuscode = sttaus,
                RequestPage = "Callback",
                Req = sb.ToString()
            });
            var rResp = new AEPSRoundpayStatusCommon
            {
                STATUS = mbResp.Statuscode == ErrorCodes.One ? RechargeRespType._SUCCESS : RechargeRespType._FAILED,
                Msg = mbResp.Msg
            };
            await SaveLog(APICode.MAHAGRAM, "UpdateStatusMG", JsonConvert.SerializeObject(rResp)).ConfigureAwait(false);
            return Json(rResp);
        }

        [HttpGet("AEPS/RPFintech/StatusCheck")]
        public async Task<IActionResult> RPFintechStatusCheck(int TID, int OutletID, int Amount, string SPKey, string TxnType)
        {
            TxnType = string.IsNullOrEmpty(TxnType) ? "CW" : TxnType;
            var ml = new CallbackML(_accessor, _env);
            StringBuilder sb = new StringBuilder();
            sb.Append("TID=");
            sb.Append(TID);
            sb.Append("&OutletID=");
            sb.Append(OutletID);
            sb.Append("&Amount=");
            sb.Append(Amount);
            sb.Append("&SPKey=");
            sb.Append(SPKey ?? string.Empty);
            sb.Append("&TxnType=");
            sb.Append(TxnType ?? string.Empty);
            var res = ml.LogCallBackRequestBool(new CallbackData { Content = sb.ToString(), Method = "RPFintechStatusCheck", Path = "AEPS/RPFintech/StatusCheck" }).Result;
            var cResp = new RPFintechCallbackResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };

            if (!string.IsNullOrEmpty(SPKey))
            {
                if (SPKey.In(SPKeys.Aadharpay, SPKeys.MATMCashWithdrawal, SPKeys.AepsMiniStatement, SPKeys.AepsCashWithdrawal, SPKeys.AepsMiniStatement, SPKeys.CashAtPOS, SPKeys.CreditCardsStandard, SPKeys.DebitCards, SPKeys.MposEMI, SPKeys.MposUPI, SPKeys.MposWallet, SPKeys.CreditCardPremium, SPKeys.CreditCardInternational, SPKeys.DebitCardRupay, SPKeys.AepsCashDeposit))
                {
                    IMiniBankML miniBankML = new AEPSML(_accessor, _env, false);
                    var mbResp = await miniBankML.MakeMiniBankTransaction(new MiniBankTransactionServiceReq
                    {
                        AmountR = Amount,
                        APIOpCode = SPKey,
                        TXNType = TxnType,
                        APICode = APICode.RPFINTECH,
                        LoginID = 1,
                        OutletID = OutletID.ToString(),
                        RequestModeID = RequestMode.API,
                        VendorID = TID.ToString()
                    }).ConfigureAwait(false);
                    cResp.Statuscode = mbResp.Statuscode;
                    cResp.Msg = mbResp.Msg;
                    if (cResp.Statuscode == ErrorCodes.One)
                    {
                        cResp.APIRequestID = mbResp.TID.ToString();
                    }
                }

            }
            return Json(cResp);
        }

        [HttpGet("AEPS/RPFintech/Updatestatus")]
        public IActionResult RPFintechUpdate(int TID, int OutletID, int Amount, string SPKey, string BankBalance, int Status, string AccountNo, string LiveID, string BankName)
        {
            var ml = new CallbackML(_accessor, _env);
            StringBuilder sb = new StringBuilder();
            sb.Append("TID=");//VendorID
            sb.Append(TID);
            sb.Append("&OutletID=");
            sb.Append(OutletID);
            sb.Append("&Amount=");
            sb.Append(Amount);
            sb.Append("&SPKey=");
            sb.Append(SPKey ?? string.Empty);
            sb.Append("&BankBalance=");
            sb.Append(BankBalance ?? "0");
            sb.Append("&Status=");
            sb.Append(Status);
            sb.Append("&AccountNo=");
            sb.Append(AccountNo ?? string.Empty);
            sb.Append("&LiveID=");
            sb.Append(LiveID ?? string.Empty);
            sb.Append("&BankName=");
            sb.Append(BankName ?? string.Empty);
            var res = ml.LogCallBackRequestBool(new CallbackData { Content = sb.ToString(), Method = "Get", Path = "AEPS/RPFintech/Updatestatus" }).Result;
            var cResp = new RPFintechCallbackResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            if (!string.IsNullOrEmpty(SPKey))
            {
                if (SPKey.In(SPKeys.Aadharpay, SPKeys.MATMCashWithdrawal, SPKeys.AepsMiniStatement, SPKeys.AepsCashWithdrawal, SPKeys.CashAtPOS, SPKeys.CreditCardsStandard, SPKeys.DebitCards, SPKeys.MposEMI, SPKeys.MposUPI, SPKeys.MposWallet, SPKeys.CreditCardPremium, SPKeys.CreditCardInternational, SPKeys.DebitCardRupay, SPKeys.AepsCashDeposit))
                {
                    IMiniBankML miniBankML = new AEPSML(_accessor, _env, false);
                    var mbResp = miniBankML.UpdateMiniBankResponse(new MiniBankTransactionServiceResp
                    {
                        Amount = Amount,
                        VendorID = TID.ToString(),
                        LT = 1,
                        LoginID = 1,
                        CardNumber = AccountNo,
                        BankBalance = BankBalance,
                        LiveID = LiveID,
                        APIOutletID = OutletID.ToString(),
                        Statuscode = Status,
                        BankName = BankName
                    });
                    cResp.Statuscode = mbResp.Statuscode;
                    cResp.Msg = mbResp.Msg;
                }
            }
            return Json(cResp);
        }

        [HttpGet("AEPS/Fingpay/StatusCheck")]
        [HttpPost("AEPS/Fingpay/StatusCheck")]

        public IActionResult FingpayStatusCheck()
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
            return Ok();
        }

        [HttpGet("AEPS/Fingpay/Updatestatus")]
        [HttpPost("AEPS/Fingpay/Updatestatus")]
        public IActionResult FingpayUpdateStatus()
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
            if (Is)
            {
                if (callbackAPIReq.Content != null)
                {
                    if (callbackAPIReq.Content.Contains("typeOfTransaction"))
                    {
                        var fingpayRes = JsonConvert.DeserializeObject<FPCallbackUpdateStatus>(resp.ToString());
                        if (fingpayRes.typeOfTransaction.StartsWith("MATM"))
                        {
                            IMiniBankML miniBankML = new AEPSML(_accessor, _env, false);
                            var res = miniBankML.MBStatusCheck(new MBStatusCheckRequest
                            {
                                TID = Convert.ToInt32(fingpayRes.merchantRefNo),
                                APIStatus = RechargeRespType.PENDING,
                                VendorID = fingpayRes.fpTransactionId,
                                RequestPage = nameof(RequestMode.API),
                                OutletID = Convert.ToInt32(fingpayRes.merchantID.Replace("FP", "")),
                                AccountNo = fingpayRes.cardNumber,
                                BankName = fingpayRes.bankName,
                                SDKMsg = fingpayRes.transactionStatus
                            });
                        }
                    }
                }
            }
            return Ok();
        }

        [HttpGet("AEPS/Mosambee/Updatestatus")]
        [HttpPost("AEPS/Mosambee/Updatestatus")]
        public async Task<IActionResult> MosambeeStatusCheck()
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
                        if (request.Form != null)
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
            var cResp = new RPFintechCallbackResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            var mosambeeResp = JsonConvert.DeserializeObject<MosambeeRoot>(resp.ToString());

            IMiniBankML miniBankML = new AEPSML(_accessor, _env, false);
            var mBnkReq = new MiniBankTransactionServiceReq
            {
                AmountR = Validate.O.IsNumeric((mosambeeResp.transactionAmount ?? string.Empty).Replace(".", "")) ? Convert.ToDecimal(mosambeeResp.transactionAmount) : 0M,
                TXNType = "CW",//mosambeeResp.transactionType.ToString(),
                APICode = APICode.MOSAMBEE,
                LoginID = 1,
                OutletIDSelf = 0,
                RequestModeID = RequestMode.API,
                VendorID = mosambeeResp.transactionID,
                //BankIIN = mosambeeResp.transactionRRN,
                BCID = mosambeeResp.merchantId,
                TerminalID = mosambeeResp.transactionTerminalId,
                OutletID = mosambeeResp.name,
                RequestURL = "AEPS/Mosambee/Updatestatus?" + resp.ToString()
            };
            if (mosambeeResp.transactionTypeId.ToString().In("36", "7", "39"))
                mBnkReq.APIOpCode = mosambeeResp.creditDebitCardType;
            else
                mBnkReq.APIOpCode = mosambeeResp.transactionTypeId.ToString();
            var mbResp = await miniBankML.MakeMiniBankTransaction(mBnkReq).ConfigureAwait(false);
            if (mbResp.Statuscode == ErrorCodes.One)
            {
                var sttaus = mosambeeResp.transactionStatus.Contains("Approved or completed") ? RechargeRespType.SUCCESS : RechargeRespType.PENDING;
                //sttaus = mosambeeResp.responseCode == RechargeRespType._FAILED || mosambeeResp.responseCode == RechargeRespType._FAILURE ? RechargeRespType.FAILED : sttaus;

                var umbResp = miniBankML.UpdateMiniBankResponse(new MiniBankTransactionServiceResp
                {
                    TID = mbResp.TID,
                    VendorID = mosambeeResp.transactionID,
                    LT = 1,
                    LoginID = 1,
                    LiveID = mosambeeResp.transactionRRN,
                    Statuscode = sttaus,
                    RequestPage = "Callback",
                    Req = "AEPS/Mosambee/Updatestatus?" + resp.ToString(),
                    Amount = Convert.ToInt32(mosambeeResp.transactionAmount.Contains('.') ? mosambeeResp.transactionAmount.Split('.')[0] : mosambeeResp.transactionAmount)
                });
                cResp.Statuscode = umbResp.Statuscode;
                cResp.Msg = umbResp.Msg;
            }
            else
            {
                cResp.Statuscode = mbResp.Statuscode;
                cResp.Msg = mbResp.Msg;
            }
            return Json(cResp);
        }
        private async Task SaveLog(string APICode, string Method, string Response)
        {
            try
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
                        rbody = await reader.ReadToEndAsync().ConfigureAwait(false);
                    }
                    req = GetAbsoluteURI() + "?" + rbody;
                }
                else
                {
                    req = GetAbsoluteURI() + request.QueryString.ToString();
                }
                IAEPSControllerHelper aepsML = new AEPSML(_accessor, _env, false);
                await aepsML.SaveAEPSLog(APICode, Method, req, Response).ConfigureAwait(false);
            }
            catch (Exception)
            {

            }
        }
        private string GetAbsoluteURI()
        {
            var request = HttpContext.Request;
            return request.Scheme + "://" + request.Host + request.Path;
        }
    }
}
