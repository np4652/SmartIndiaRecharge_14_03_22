using Fintech.AppCode.Configuration;
using Fintech.AppCode.DB;
using Fintech.AppCode.StaticModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using RoundpayFinTech.AppCode.DL;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using paytm;
using Newtonsoft.Json;
using Fintech.AppCode.WebRequest;
using System.Threading.Tasks;
using RoundpayFinTech.AppCode.Model;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model.MoneyTransfer;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode;
using RoundpayFinTech.AppCode.MiddleLayer.Dmt_Api;

namespace RoundpayFinTech.AppCode.ThirdParty.Paytm
{
    public partial class PaytmML
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        private readonly IConfiguration Configuration;
        private readonly IConnectionConfiguration _c;
        private readonly PaytmAppSetting paytmAppSetting;
        private readonly int _APIID;
        private readonly ILoginML _loginML;

        public PaytmML(IHttpContextAccessor accessor, IHostingEnvironment env, int APIID)
        {
            _accessor = accessor;
            _env = env;
            var builder = new ConfigurationBuilder()
                 .SetBasePath(Directory.GetCurrentDirectory());
            builder.AddJsonFile((_env.IsProduction() ? "appsettings.json" : "appsettings.Development.json"));
            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
            _c = new ConnectionConfiguration(_accessor, _env);
            _dal = new DAL(_c.GetConnectionString());
            paytmAppSetting = PAYTMAppSetting();
            _APIID = APIID;
            _loginML = new LoginML(_accessor, _env);
        }
        private PaytmAppSetting PAYTMAppSetting()
        {
            var setting = new PaytmAppSetting();
            try
            {
                setting = new PaytmAppSetting
                {
                    MID = Configuration["DMR:PAYTM:MID"],
                    MERCHANTKEY = Configuration["DMR:PAYTM:MERCHANTKEY"],
                    PAYOUTBASEURL = Configuration["DMR:PAYTM:PAYOUTBASEURL"],
                    VERIFYBASEURL = Configuration["DMR:PAYTM:VERIFYBASEURL"],
                    STATUSCHECKURL = Configuration["DMR:PAYTM:STATUSCHECKURL"],
                    GUID = Configuration["DMR:PAYTM:GUID"]
                };
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "PaytmAppSetting",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return setting;
        }
        public async Task<ResponseStatus> CheckSender(DMTReq _req)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            try
            {
                var senderRequest = (SenderRequest)new ProcGetSender(_dal).Call(_req.SenderNO);
                if (string.IsNullOrWhiteSpace(senderRequest.Name))
                {
                    res.Statuscode = ErrorCodes.One;
                    res.Msg = ErrorCodes.DMTSNE;
                    res.CommonInt = ErrorCodes.One;
                    return res;
                }
                else if (senderRequest._VerifyStatus == ErrorCodes.Two)
                {
                    var procSender = new ProcGetSenderLimit(_dal);
                    var senderLimit = (SenderLimitModel)await procSender.Call(new CommonReq
                    {
                        CommonInt = senderRequest.ID,
                        CommonInt2 = _APIID
                    }).ConfigureAwait(false);
                    res.CommonStr = (senderLimit.SenderLimit - senderLimit.LimitUsed).ToString();
                    res.Statuscode = ErrorCodes.One;
                    res.Msg = ErrorCodes.DMTSLS;
                    res.CommonStr2 = senderRequest.Name;
                }
                else if (senderRequest._VerifyStatus == ErrorCodes.One)
                {
                    res.Statuscode = ErrorCodes.One;
                    res.Msg = ErrorCodes.DMTSNE;
                    res.CommonInt = ErrorCodes.One;
                }
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "CheckSender",
                    Error = ex.Message,
                    LoginTypeID = _req.LT,
                    UserId = _req.UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }

        public ResponseStatus CreateSender(CreateSen _req)
        {
            var dbres = new SenderInfo();
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            try
            {
                _req.senderRequest.UserID = _req.dMTReq.UserID;
                dbres = (new ProcUpdateSender(_dal).Call(_req.senderRequest)) as SenderInfo;
                if (dbres.Statuscode == ErrorCodes.Minus1)
                {
                    res.Msg = dbres.Msg;
                    return res;
                }
                if (dbres.Statuscode == ErrorCodes.One && dbres._VerifyStatus != ErrorCodes.Two)
                {
                    res.Statuscode = ErrorCodes.One;
                    res.Msg = ErrorCodes.DMTOSS;
                    res.CommonInt = ErrorCodes.One;
                    res.CommonStr = dbres.OTP;
                    res.CommonInt2 = dbres.WID;
                }
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "CreateSender",
                    Error = ex.Message,
                    LoginTypeID = _req.dMTReq.LT,
                    UserId = _req.dMTReq.UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            DMTReqRes dMTReq = new DMTReqRes
            {
                APIID = _req.dMTReq.ApiID,
                Method = "CreateSender",
                RequestModeID = _req.dMTReq.RequestMode,
                Request = JsonConvert.SerializeObject(_req),
                Response = JsonConvert.SerializeObject(res),
                SenderNo = _req.dMTReq.SenderNO,
                UserID = _req.dMTReq.UserID,
                TID = _req.dMTReq.TID
            };
            new ProcUpdateLogDMRReqResp(_dal).Call(dMTReq);
            return res;
        }

        public BeniRespones GetBeneficiary(DMTReq _req)
        {
            var res = new BeniRespones
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            try
            {
                var resDB = (new ProcGetBenificiary(_dal).Call(_req)) as BenificiaryModel;
                res.Statuscode = resDB.Statuscode;
                res.Msg = resDB.Msg;
                if (resDB != null && resDB.Statuscode == ErrorCodes.One)
                {
                    var ListBeni = new List<AddBeni>();
                    if (resDB.benificiaries != null && resDB.benificiaries.Count > 0)
                    {
                        foreach (var r in resDB.benificiaries)
                        {
                            var addBeni = new AddBeni
                            {
                                AccountNo = r._AccountNumber,
                                BankName = r._BankName,
                                IFSC = r._IFSC,
                                BeneName = r._Name,
                                MobileNo = r._BankName,
                                BeneID = r._ID.ToString()
                            };
                            ListBeni.Add(addBeni);
                        }
                    }
                    res.addBeni = ListBeni;
                }
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "GetBeneficiary",
                    Error = ex.Message,
                    LoginTypeID = _req.LT,
                    UserId = _req.UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            var dMTReq = new DMTReqRes
            {
                APIID = _req.ApiID,
                Method = "GetBeneficiary",
                RequestModeID = _req.RequestMode,
                Request = "|" + JsonConvert.SerializeObject(_req),
                Response = "|" + JsonConvert.SerializeObject(res),
                SenderNo = _req.SenderNO,
                UserID = _req.UserID,
                TID = _req.TID
            };
            new ProcUpdateLogDMRReqResp(_dal).Call(dMTReq);
            return res;
        }

        public ResponseStatus VerifySender(DMTReq _req, string OTP)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            try
            {
                var req = new CommonReq
                {
                    CommonStr = _req.SenderNO,
                    CommonStr2 = OTP,
                    CommonInt = _req.UserID
                };
                var senderRes = (SenderInfo)new ProcVerySenderOTP(_dal).Call(req);
                if (senderRes.Statuscode == ErrorCodes.Minus1)
                {
                    res.Msg = senderRes.Msg;
                    return res;
                }
                else if (senderRes.Statuscode == ErrorCodes.One)
                {
                    res.Statuscode = ErrorCodes.One;
                    res.Msg = ErrorCodes.DMTSCS;
                }
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "VerifySender",
                    Error = ex.Message,
                    LoginTypeID = _req.LT,
                    UserId = _req.UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            var dMTReq = new DMTReqRes
            {
                APIID = _req.ApiID,
                Method = "VerifySender",
                RequestModeID = _req.RequestMode,
                Request = JsonConvert.SerializeObject(_req),
                Response = JsonConvert.SerializeObject(res),
                SenderNo = _req.SenderNO,
                UserID = _req.UserID,
                TID = _req.TID
            };
            new ProcUpdateLogDMRReqResp(_dal).Call(dMTReq);
            return res;
        }

        public ResponseStatus CreateBeneficiary(AddBeni addBeni, DMTReq _req)
        {
            string response = "", request = "";
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            try
            {
                var param = new BenificiaryDetail
                {
                    _SenderMobileNo = addBeni.SenderMobileNo,
                    _Name = addBeni.BeneName,
                    _AccountNumber = addBeni.AccountNo,
                    _MobileNo = addBeni.MobileNo,
                    _IFSC = addBeni.IFSC,
                    _BankName = addBeni.BankName,
                    _EntryBy = _req.UserID,
                    _VerifyStatus = 1,
                    _BankID = addBeni.BankID
                };
                var resdb = (BenificiaryModel)new ProcAddBenificiary(_dal).Call(param);
                if (resdb.Statuscode == ErrorCodes.One)
                {
                    res.Statuscode = ErrorCodes.One;
                    res.Msg = ErrorCodes.BENESCS;
                    return res;
                }
                else
                {
                    res.Msg = resdb.Msg;
                }
            }
            catch (Exception ex)
            {
                response = ex.Message + "|" + response;
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "CreateBeneficiary",
                    Error = ex.Message,
                    LoginTypeID = _req.LT,
                    UserId = _req.UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            var dMTReq = new DMTReqRes
            {
                APIID = _req.ApiID,
                Method = "CreateBeneficiary",
                RequestModeID = _req.RequestMode,
                Request = request + "|" + JsonConvert.SerializeObject(_req),
                Response = response + "|" + JsonConvert.SerializeObject(res),
                SenderNo = _req.SenderNO,
                UserID = _req.UserID,
                TID = _req.TID
            };
            new ProcUpdateLogDMRReqResp(_dal).Call(dMTReq);
            return res;
        }
        public async Task<DMRTransactionResponse> VerifyBeneficiary(DMTReq _req, ReqSendMoney sendMoney, DMRTransactionResponse res)
        {
            //{"subwalletGuid":"a4928669-24e7-49ca-bb6e-7952a94fccfe","orderId":"R2078112257370A649","beneficiaryAccount":"35540100005691","beneficiaryIFSC":"BARB0GANAPA"}
            //https://dashboard.paytm.com/bpay/api/v1/beneficiary/validate
            res.Statuscode = RechargeRespType.PENDING;
            res.Msg = nameof(ErrorCodes.Request_Accpeted).Replace("_", " ");
            res.ErrorCode = ErrorCodes.Request_Accpeted;
            res.LiveID = string.Empty;

            var dataRequest = new PaytmDataRequest
            {
                orderId = res.TID.ToString(),
                subwalletGuid = paytmAppSetting.GUID,
                beneficiaryAccount = sendMoney.AccountNo,
                beneficiaryIFSC = sendMoney.IFSC
            };
            string URL = paytmAppSetting.PAYOUTBASEURL + "beneficiary/validate";
            string checksum = CheckSum.generateCheckSumByJson(paytmAppSetting.MERCHANTKEY, JsonConvert.SerializeObject(dataRequest));
            var headers = new Dictionary<string, string>
                {
                    { "x-mid", paytmAppSetting.MID },
                    { "x-checksum", checksum }
                };
            string apiResp = string.Empty;
            try
            {
                apiResp = await AppWebRequest.O.PostJsonDataUsingHWRTLS(URL, dataRequest, headers).ConfigureAwait(false);
                if (apiResp != null)
                {
                    var paytmResp = JsonConvert.DeserializeObject<PaytmResponse>(apiResp);
                    if (paytmResp.status.Equals(RechargeRespType._SUCCESS))
                    {
                        res.VendorID = paytmResp.statusCode;
                        res.LiveID = paytmResp.statusMessage;
                        res.Statuscode = RechargeRespType.SUCCESS;
                        res.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                        res.ErrorCode = ErrorCodes.Transaction_Successful;
                    }
                    else if (paytmResp.status.Equals("FAILURE"))
                    {
                        res.Statuscode = RechargeRespType.FAILED;
                        res.Msg = paytmResp.statusMessage;
                        res.ErrorCode = ErrorCodes.Unknown_Error;
                        res.LiveID = res.Msg;
                        if (paytmResp.statusCode.Equals("DE_705"))
                        {
                            res.Msg = nameof(ErrorCodes.Unknown_Error).Replace("_", " ");
                            res.ErrorCode = ErrorCodes.Unknown_Error;
                            res.LiveID = res.Msg + " DE_705";
                        }
                    }
                    else
                    {
                        res.Msg = paytmResp.statusMessage;

                    }
                }
            }
            catch (Exception ex)
            {
                apiResp = " Exception:" + ex.Message + " | " + apiResp;
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "VerifyBeneficiary",
                    Error = ex.Message,
                    LoginTypeID = _req.LT,
                    UserId = _req.UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            var dMTReq = new DMTReqRes
            {
                APIID = _APIID,
                Method = "VerifyBeneficiary",
                RequestModeID = _req.RequestMode,
                Request = URL + JsonConvert.SerializeObject(dataRequest) + JsonConvert.SerializeObject(headers),
                Response = apiResp ?? string.Empty,
                SenderNo = _req.SenderNO,
                UserID = _req.UserID,
                TID = res.TID.ToString()
            };
            new ProcUpdateLogDMRReqResp(_dal).Call(dMTReq);
            res.Request = dMTReq.Request;
            res.Response = dMTReq.Response;
            if (res.Statuscode == RechargeRespType.PENDING)
            {
                var tempres = await LoopStatusCheck(res.TID, _req.RequestMode, _req.UserID, IsNeft: true).ConfigureAwait(false);
                res.Statuscode = tempres.Statuscode;
                res.LiveID = tempres.LiveID;
                res.VendorID = tempres.VendorID;
                res.Msg = tempres.Msg;
                res.ErrorCode = tempres.ErrorCode;
                res.BeneName = tempres.BeneName;
            }
            return res;
        }
        public async Task<DMRTransactionResponse> SendMoneyPayout(DMTReq _req, ReqSendMoney sendMoney, DMRTransactionResponse res)
        {

            res.Statuscode = RechargeRespType.PENDING;
            res.Msg = nameof(ErrorCodes.Request_Accpeted).Replace("_", " ");
            res.ErrorCode = ErrorCodes.Request_Accpeted;
            res.LiveID = string.Empty;
            var WebInfo = _loginML.GetWebsiteInfo();
            var dataRequest = new PaytmDataRequest
            {
                orderId = res.TID.ToString(),
                subwalletGuid = paytmAppSetting.GUID,
                amount = sendMoney.Amount.ToString(),
                purpose = "OTHERS",
                date = DateTime.Now.ToString("yyyy-MM-dd"),
                callbackUrl = WebInfo.MainDomain + "/Callback/" + _APIID,
                beneficiaryAccount = _req.ChanelType == PaymentMode_.UPI ? "" : sendMoney.AccountNo,
                beneficiaryVPA = _req.ChanelType != PaymentMode_.UPI ? "" : sendMoney.AccountNo,
                beneficiaryIFSC = sendMoney.IFSC,
                //beneficiaryPhoneNo = _req.SenderNO,
                transactionType = "NON_CASHBACK",
                beneficiaryName = sendMoney.BeneName
            };

            string URL = string.Empty;
            if (_req.ChanelType == PaymentMode_.PAYTM_IMPS || _req.ChanelType == PaymentMode_.PAYTM_Neft || _req.ChanelType == PaymentMode_.UPI || _req.ChanelType == PaymentMode_.PAYTM_RTGS)
            {
                URL = paytmAppSetting.PAYOUTBASEURL + "disburse/order/bank";
                dataRequest.transferMode = _req.ChanelType == PaymentMode_.PAYTM_IMPS ? "IMPS" : (_req.ChanelType == PaymentMode_.PAYTM_Neft ? "NEFT" : (_req.ChanelType == PaymentMode_.UPI ? "UPI" : (_req.ChanelType == PaymentMode_.PAYTM_RTGS ? "RTGS" : "")));
            }
            else if (_req.ChanelType == PaymentMode_.WalletTransfer)
            {
                dataRequest.transferMode = "WalletTransfer";
                URL = paytmAppSetting.PAYOUTBASEURL + "disburse/order/wallet/loyalty";
            }

            string checksum = CheckSum.generateCheckSumByJson(paytmAppSetting.MERCHANTKEY, JsonConvert.SerializeObject(dataRequest));
            var headers = new Dictionary<string, string>
                {
                    { "x-mid", paytmAppSetting.MID },
                    { "x-checksum", checksum }
                };
            string apiResp = string.Empty;
            try
            {
                apiResp = await AppWebRequest.O.PostJsonDataUsingHWRTLS(URL, dataRequest, headers).ConfigureAwait(false);
                if (apiResp != null)
                {
                    var paytmResp = JsonConvert.DeserializeObject<PaytmResponse>(apiResp);
                    if (paytmResp.status.Equals(RechargeRespType._SUCCESS))
                    {
                        res.VendorID = paytmResp.statusCode;
                        res.LiveID = paytmResp.result==null? paytmResp .result.rrn: paytmResp.statusMessage;
                        res.Statuscode = RechargeRespType.SUCCESS;
                        res.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                        res.ErrorCode = ErrorCodes.Transaction_Successful;
                    }
                    else if (paytmResp.status.Equals("FAILURE"))
                    {
                        res.Statuscode = RechargeRespType.FAILED;
                        res.Msg = paytmResp.statusMessage;
                        res.ErrorCode = ErrorCodes.Unknown_Error;
                        res.LiveID = res.Msg;
                        if (paytmResp.statusCode.Equals("DE_705"))
                        {
                            res.Msg = nameof(ErrorCodes.Unknown_Error).Replace("_", " ");
                            res.ErrorCode = ErrorCodes.Unknown_Error;
                            res.LiveID = res.Msg + " DE_705";
                        }
                    }
                    else
                    {
                        res.Msg = paytmResp.statusMessage;

                    }
                }
            }
            catch (Exception ex)
            {
                apiResp = " Exception:" + ex.Message + " | " + apiResp;
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "SendMoneyPayout",
                    Error = ex.Message,
                    LoginTypeID = _req.LT,
                    UserId = _req.UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            var dMTReq = new DMTReqRes
            {
                APIID = _APIID,
                Method = "SendMoneyPayout",
                RequestModeID = _req.RequestMode,
                Request = URL + JsonConvert.SerializeObject(dataRequest) + JsonConvert.SerializeObject(headers),
                Response = apiResp ?? string.Empty,
                SenderNo = _req.SenderNO,
                UserID = _req.UserID,
                TID = res.TID.ToString()
            };
            new ProcUpdateLogDMRReqResp(_dal).Call(dMTReq);
            res.Request = dMTReq.Request;
            res.Response = dMTReq.Response;
            if (res.Statuscode == RechargeRespType.PENDING)
            {
                var tempres = await LoopStatusCheck(res.TID, _req.RequestMode, _req.UserID, _req.ChanelType == PaymentMode_.PAYTM_Neft).ConfigureAwait(false);
                res.Statuscode = tempres.Statuscode;
                res.LiveID = tempres.LiveID;
                res.VendorID = tempres.VendorID;
                res.Msg = tempres.Msg;
                res.ErrorCode = tempres.ErrorCode;
            }
            return res;
        }
        private async Task<DMRTransactionResponse> LoopStatusCheck(int TID, int RMode, int UserID, bool IsNeft)
        {
            var res = new DMRTransactionResponse
            {
                Statuscode = RechargeRespType.PENDING,
                Msg = nameof(ErrorCodes.Request_Accpeted)
            };
            int i = 0;
            int LoopTill = IsNeft ? 1 : (RMode == RequestMode.API ? 10 : 10);
            while (i < LoopTill)
            {
                i++;
                if (res.Statuscode == RechargeRespType.PENDING)
                {
                    await Task.Delay(10 * 1000).ConfigureAwait(false);
                    res = await GetTransactionStatus(TID, RMode, UserID).ConfigureAwait(false);
                    if (res.Statuscode != RechargeRespType.PENDING)
                    {
                        i = LoopTill;
                    }
                }
                else
                {
                    i = LoopTill;
                }
            }
            return res;
        }
        public async Task<DMRTransactionResponse> GetTransactionStatus(int TID, int RequestMode, int UserID)
        {
            var res = new DMRTransactionResponse
            {
                Statuscode = RechargeRespType.PENDING,
                Msg = nameof(RechargeRespType.PENDING),
                ErrorCode = ErrorCodes.Request_Accpeted
            };
            var dataRequest = new PaytmStatusCheck
            {
                orderId = TID.ToString()
            };
            string checksum = CheckSum.generateCheckSumByJson(paytmAppSetting.MERCHANTKEY, JsonConvert.SerializeObject(dataRequest));
            var headers = new Dictionary<string, string>
            {
                { "x-mid", paytmAppSetting.MID },
                { "x-checksum", checksum }
            };
            string URL = paytmAppSetting.STATUSCHECKURL;
            string resp = string.Empty;
            try
            {
                resp = await AppWebRequest.O.PostJsonDataUsingHWRTLS(URL, dataRequest, headers).ConfigureAwait(false);
                if (resp != null)
                {
                    var paytmResp = JsonConvert.DeserializeObject<PaytmResponse>(resp);
                    if (paytmResp.status.Equals(RechargeRespType._SUCCESS))
                    {
                        res.VendorID = paytmResp.result.paytmOrderId;
                        res.LiveID = paytmResp.result.rrn;
                        res.Statuscode = RechargeRespType.SUCCESS;
                        res.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                        res.ErrorCode = ErrorCodes.Transaction_Successful;
                        res.BeneName = paytmResp.result.beneficiaryName;
                    }
                    else if (paytmResp.status.Equals("FAILURE"))
                    {
                        res.Statuscode = RechargeRespType.FAILED;
                        res.Msg = paytmResp.statusMessage;
                        res.ErrorCode = ErrorCodes.Unknown_Error;
                        res.LiveID = res.Msg;
                        if (paytmResp.statusCode.Equals("DE_705"))
                        {
                            res.Msg = nameof(ErrorCodes.Unknown_Error).Replace("_", " ");
                            res.ErrorCode = ErrorCodes.Unknown_Error;
                            res.LiveID = res.Msg + " DE_705";
                        }
                    }
                    else
                    {
                        res.Msg = paytmResp.statusMessage;
                    }
                }
            }
            catch (Exception ex)
            {
                resp = " Exception:" + ex.Message + " | " + resp;
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "GetTransactionStatus",
                    Error = ex.Message,
                    LoginTypeID = 1,
                    UserId = UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            var dMTReq = new DMTReqRes
            {
                APIID = _APIID,
                Method = "GetTransactionStatus",
                RequestModeID = RequestMode,
                Request = URL + JsonConvert.SerializeObject(dataRequest) + JsonConvert.SerializeObject(headers),
                Response = resp,
                SenderNo = string.Empty,
                UserID = UserID,
                TID = TID.ToString()
            };
            new ProcUpdateLogDMRReqResp(_dal).Call(dMTReq);
            res.Request = dMTReq.Request;
            res.Response = resp;
            return res;
        }
    }

    public partial class PaytmML : IMoneyTransferAPIML
    {
        
        public PaytmML(IHttpContextAccessor accessor, IHostingEnvironment env, IDAL dal)
        {
            _accessor = accessor;
            _env = env;
            var builder = new ConfigurationBuilder()
                 .SetBasePath(Directory.GetCurrentDirectory());
            builder.AddJsonFile((_env.IsProduction() ? "appsettings.json" : "appsettings.Development.json"));
            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
            _dal = dal;
            paytmAppSetting = PAYTMAppSetting();
            _loginML = new LoginML(_accessor, _env);
        }

        public PaytmML(IHttpContextAccessor accessor, IHostingEnvironment env, IDAL dal,int APIID)
        {
            _accessor = accessor;
            _env = env;
            var builder = new ConfigurationBuilder()
                 .SetBasePath(Directory.GetCurrentDirectory());
            builder.AddJsonFile((_env.IsProduction() ? "appsettings.json" : "appsettings.Development.json"));
            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
            _dal = dal;
            paytmAppSetting = PAYTMAppSetting();
            _loginML = new LoginML(_accessor, _env);
            _APIID = APIID;
        }

        public MSenderLoginResponse GetSender(MTAPIRequest request)
        {
            var res = new MSenderLoginResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = nameof(ErrorCodes.Unknown_Error).Replace("_", " "),
                ErrorCode = ErrorCodes.Unknown_Error
            };
            try
            {
                var senderRequest = (SenderRequest)new ProcGetSender(_dal).Call(request.SenderMobile);
                if (string.IsNullOrWhiteSpace(senderRequest.Name))
                {
                    res.Statuscode = ErrorCodes.One;
                    res.Msg = nameof(DMTErrorCodes.Sender_Not_Found).Replace("_", " ");
                    res.IsSenderNotExists = true;
                    res.ErrorCode = DMTErrorCodes.Sender_Not_Found;
                    return res;
                }
                else if (senderRequest._VerifyStatus == ErrorCodes.Two)
                {
                    var procSender = new ProcGetSenderLimit(_dal);
                    var senderLimit = (SenderLimitModel)procSender.Call(new CommonReq
                    {
                        CommonInt = senderRequest.ID,
                        CommonInt2 = request.APIID
                    }).Result;
                    res.RemainingLimit = senderLimit.SenderLimit - senderLimit.LimitUsed;
                    res.AvailbleLimit = senderLimit.SenderLimit;
                    res.Statuscode = ErrorCodes.One;
                    res.Msg = nameof(DMTErrorCodes.Detail_Found_Successfully).Replace("_", " ");
                    res.ErrorCode = DMTErrorCodes.Detail_Found_Successfully;
                    res.SenderMobile = request.SenderMobile;
                    res.KYCStatus = SenderKYCStatus.ACTIVE;
                    res.SenderName = senderRequest.Name;
                    res.IsNotCheckLimit = senderRequest.IsNotCheckLimit;
                }
                else if (senderRequest._VerifyStatus == ErrorCodes.One)
                {
                    res.Statuscode = ErrorCodes.One;
                    res.Msg = nameof(DMTErrorCodes.Sender_Not_Found).Replace("_", " ");
                    res.IsSenderNotExists = true;
                    res.ErrorCode = DMTErrorCodes.Sender_Not_Found;
                }
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "GetSender",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = request.UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }
        public MSenderLoginResponse CreateBeneficiary(MTAPIRequest request)
        {
            var res = new MSenderLoginResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError,
                ErrorCode = ErrorCodes.Transaction_Successful
            };
            try
            {
                var param = new BenificiaryDetail
                {
                    _SenderMobileNo = request.SenderMobile,
                    _Name = request.mBeneDetail.BeneName,
                    _AccountNumber = request.mBeneDetail.AccountNo,
                    _MobileNo = request.mBeneDetail.MobileNo,
                    _IFSC = request.mBeneDetail.IFSC,
                    _BankName = request.mBeneDetail.BankName,
                    _EntryBy = request.UserID,
                    _VerifyStatus = 1,
                    _APICode = request.APICode,
                    _BankID = request.mBeneDetail.BankID
                };
                var resdb = (BenificiaryModel)new ProcAddBenificiary(_dal).Call(param);
                if (resdb.Statuscode == ErrorCodes.One)
                {
                    res.Statuscode = ErrorCodes.One;
                    res.Msg = ErrorCodes.BENESCS;
                    res.ErrorCode = ErrorCodes.Transaction_Successful;
                }
                else
                {
                    res.Msg = resdb.Msg;
                }
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "CreateBeneficiary",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = request.UserID
                });
            }
            return res;
        }
        public MSenderCreateResp CreateSender(MTAPIRequest request)
        {
            var res = new MSenderCreateResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError,
                ErrorCode = ErrorCodes.Unknown_Error
            };
            try
            {
                var dbres = (new ProcUpdateSender(_dal).Call(new SenderRequest
                {
                    Name = request.FirstName + " " + request.LastName,
                    MobileNo = request.SenderMobile,
                    Pincode = request.Pincode.ToString(),
                    Address = request.Address,
                    City = request.City,
                    StateID = request.StateID,
                    AadharNo = request.AadharNo,
                    Dob = request.DOB,
                    UserID = request.UserID
                })) as SenderInfo;
                if (dbres.Statuscode == ErrorCodes.Minus1)
                {
                    res.Msg = dbres.Msg;
                    return res;
                }
                if (dbres.Statuscode == ErrorCodes.One && dbres._VerifyStatus != ErrorCodes.Two)
                {
                    res.Statuscode = ErrorCodes.One;
                    res.Msg = nameof(DMTErrorCodes.Partial_Registration_Please_verify_sender_with_OTP).Replace("_", " ");
                    res.ErrorCode = DMTErrorCodes.Partial_Registration_Please_verify_sender_with_OTP;
                    res.IsOTPGenerated = true;
                    res.OTP = dbres.OTP;
                    res.WID = dbres.WID;
                }
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "CreateSender",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = request.UserID
                });
            }
            new ProcUpdateLogDMRReqResp(_dal).Call(new DMTReqRes
            {
                APIID = request.APIID,
                Method = "CreateSender",
                RequestModeID = request.RequestMode,
                Request = "Internal",
                Response = JsonConvert.SerializeObject(res),
                SenderNo = request.SenderMobile,
                UserID = request.UserID,
                TID = request.TransactionID
            });
            return res;
        }

        public MSenderCreateResp GenerateOTP(MTAPIRequest request)
        {
            throw new NotImplementedException();
        }

        public MBeneficiaryResp GetBeneficiary(MTAPIRequest request)
        {
            return DMTAPIHelperML.GetBeneficiary(request, _dal, GetType().Name);
        }

        public MSenderCreateResp SenderKYC(MTAPIRequest request)
        {
            throw new NotImplementedException();
        }

        public MSenderCreateResp SenderResendOTP(MTAPIRequest request)
        {
            throw new NotImplementedException();
        }

        public MSenderLoginResponse ValidateBeneficiary(MTAPIRequest request)
        {
            throw new NotImplementedException();
        }

        public MSenderLoginResponse VerifySender(MTAPIRequest request)
        {
            var res = new MSenderLoginResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError,
                ErrorCode = ErrorCodes.Unknown_Error
            };
            try
            {
                var req = new CommonReq
                {
                    CommonStr = request.SenderMobile,
                    CommonStr2 = request.OTP,
                    CommonInt = request.UserID
                };
                var senderRes = (SenderInfo)new ProcVerySenderOTP(_dal).Call(req);
                if (senderRes.Statuscode == ErrorCodes.Minus1)
                {
                    res.Msg = senderRes.Msg;
                    return res;
                }
                else if (senderRes.Statuscode == ErrorCodes.One)
                {
                    res.Statuscode = ErrorCodes.One;
                    res.Msg = ErrorCodes.DMTSCS;
                    res.ErrorCode = ErrorCodes.Transaction_Successful;
                }
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "VerifySender",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = request.UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }

        public MSenderLoginResponse RemoveBeneficiary(MTAPIRequest request)
        {
            var res = new MSenderLoginResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError,
                ErrorCode = ErrorCodes.Unknown_Error
            };
            try
            {
                var _res = (ResponseStatus)new ProcRemoveBeneficiaryNew(_dal).Call(new CommonReq
                {
                    LoginID = request.UserID,
                    CommonInt = Convert.ToInt32(request.mBeneDetail.BeneID),
                    CommonStr = request.SenderMobile
                });
                res.Statuscode = _res.Statuscode;
                res.Msg = _res.Msg;
                res.ErrorCode = _res.ErrorCode;
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "RemoveBeneficiary",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = request.UserID
                });
            }
            return res;
        }
        public DMRTransactionResponse VerifyAccount(MTAPIRequest request)
        {
            var res = new DMRTransactionResponse
            {
                Statuscode = RechargeRespType.PENDING,
                Msg = RechargeRespType._PENDING,
                VendorID = "",
                LiveID = ""
            };
            var dataRequest = new PaytmDataRequest
            {
                orderId = request.TID.ToString(),
                subwalletGuid = paytmAppSetting.GUID,
                beneficiaryAccount = request.mBeneDetail.AccountNo,
                beneficiaryIFSC = request.mBeneDetail.IFSC
            };
            string URL = paytmAppSetting.VERIFYBASEURL + "beneficiary/validate";
            string checksum = CheckSum.generateCheckSumByJson(paytmAppSetting.MERCHANTKEY, JsonConvert.SerializeObject(dataRequest));
            var headers = new Dictionary<string, string>
                {
                    { "x-mid", paytmAppSetting.MID },
                    { "x-checksum", checksum }
                };
            string apiResp = string.Empty;
            try
            {
                apiResp = AppWebRequest.O.PostJsonDataUsingHWRTLS(URL, dataRequest, headers).Result;
                if (apiResp != null)
                {
                    var paytmResp = JsonConvert.DeserializeObject<PaytmResponse>(apiResp);
                    if (paytmResp.status.Equals(RechargeRespType._SUCCESS))
                    {
                        res.VendorID = paytmResp.statusCode;
                        res.LiveID = paytmResp.result==null? paytmResp.statusMessage: paytmResp.result.rrn;
                        res.Statuscode = RechargeRespType.SUCCESS;
                        res.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                        res.ErrorCode = ErrorCodes.Transaction_Successful;
                        res.BeneName = paytmResp.result.beneficiaryName;
                    }
                    else if (paytmResp.status.Equals("FAILURE"))
                    {
                        res.Statuscode = RechargeRespType.FAILED;
                        res.Msg = paytmResp.statusMessage;
                        res.ErrorCode = ErrorCodes.Unknown_Error;
                        res.LiveID = res.Msg;
                        if (paytmResp.statusCode.Equals("DE_705"))
                        {
                            res.Msg = nameof(ErrorCodes.Unknown_Error).Replace("_", " ");
                            res.ErrorCode = ErrorCodes.Unknown_Error;
                            res.LiveID = res.Msg + " DE_705";
                        }
                    }
                    else
                    {
                        res.Msg = paytmResp.statusMessage;

                    }
                }
            }
            catch (Exception ex)
            {
                apiResp = " Exception:" + ex.Message + " | " + apiResp;
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "VerifyAccount",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = request.UserID
                });
            }
            res.Request = URL + JsonConvert.SerializeObject(dataRequest) + JsonConvert.SerializeObject(headers);
            new ProcUpdateLogDMRReqResp(_dal).Call(new DMTReqRes
            {
                APIID = request.APIID,
                Method = "VerifyAccount",
                RequestModeID = request.RequestMode,
                Request = res.Request,
                Response = apiResp ?? string.Empty,
                SenderNo = request.SenderMobile,
                UserID = request.UserID,
                TID = request.TID.ToString()
            });
            res.Response = apiResp;
            if (res.Statuscode == RechargeRespType.PENDING)
            {
                var tempres = LoopStatusCheck(request.TID, request.RequestMode, request.UserID,false).Result;
                res.Statuscode = tempres.Statuscode;
                res.LiveID = tempres.LiveID;
                res.VendorID = tempres.VendorID;
                res.Msg = tempres.Msg;
                res.ErrorCode = tempres.ErrorCode;
                res.BeneName = tempres.BeneName;
            }
            return res;
        }
        public DMRTransactionResponse AccountTransfer(MTAPIRequest request)
        {
            var res = new DMRTransactionResponse
            {
                Statuscode = RechargeRespType.PENDING,
                Msg = RechargeRespType._PENDING,
                VendorID = "",
                LiveID = "",
                ErrorCode = ErrorCodes.Request_Accpeted
            };
            var WebInfo = _loginML.GetWebsiteInfo();
            var dataRequest = new PaytmDataRequest
            {
                orderId = request.TID.ToString(),
                subwalletGuid = paytmAppSetting.GUID,
                amount = request.Amount.ToString(),
                purpose = "OTHERS",
                date = DateTime.Now.ToString("yyyy-MM-dd"),
                callbackUrl = WebInfo.MainDomain + "/Callback/" + _APIID,
                beneficiaryAccount = request.mBeneDetail.AccountNo,
                beneficiaryIFSC = request.mBeneDetail.IFSC,
                //beneficiaryPhoneNo = _req.SenderNO,
                transactionType = "NON_CASHBACK",
                beneficiaryName = request.mBeneDetail.BeneName
            };
            int ChanelType = request.TransMode.Equals("IMPS") ? PaymentMode_.PAYTM_IMPS : PaymentMode_.PAYTM_Neft;
            string URL = string.Empty;
            if (ChanelType == PaymentMode_.PAYTM_IMPS || ChanelType == PaymentMode_.PAYTM_Neft || ChanelType == PaymentMode_.UPI || ChanelType == PaymentMode_.PAYTM_RTGS)
            {
                URL = paytmAppSetting.PAYOUTBASEURL + "disburse/order/bank";
                dataRequest.transferMode = ChanelType == PaymentMode_.PAYTM_IMPS ? "IMPS" : (ChanelType == PaymentMode_.PAYTM_Neft ? "NEFT" : (ChanelType == PaymentMode_.UPI ? "UPI" : (ChanelType == PaymentMode_.PAYTM_RTGS ? "RTGS" : "")));
            }
            else if (ChanelType == PaymentMode_.WalletTransfer)
            {
                dataRequest.transferMode = "WalletTransfer";
                URL = paytmAppSetting.PAYOUTBASEURL + "disburse/order/wallet/loyalty";
            }

            string checksum = CheckSum.generateCheckSumByJson(paytmAppSetting.MERCHANTKEY, JsonConvert.SerializeObject(dataRequest));
            var headers = new Dictionary<string, string>
                {
                    { "x-mid", paytmAppSetting.MID },
                    { "x-checksum", checksum }
                };
            string apiResp = string.Empty;
            try
            {
                apiResp = AppWebRequest.O.PostJsonDataUsingHWRTLS(URL, dataRequest, headers).Result;
                if (apiResp != null)
                {
                    var paytmResp = JsonConvert.DeserializeObject<PaytmResponse>(apiResp);
                    if (paytmResp.status.Equals(RechargeRespType._SUCCESS))
                    {
                        res.VendorID = paytmResp.statusCode;
                        res.LiveID = paytmResp.result == null ? paytmResp.statusMessage : paytmResp.result.rrn; 
                        res.Statuscode = RechargeRespType.SUCCESS;
                        res.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                        res.ErrorCode = ErrorCodes.Transaction_Successful;
                    }
                    else if (paytmResp.status.Equals("FAILURE"))
                    {
                        res.Statuscode = RechargeRespType.FAILED;
                        res.Msg = paytmResp.statusMessage;
                        res.ErrorCode = ErrorCodes.Unknown_Error;
                        res.LiveID = res.Msg;
                        if (paytmResp.statusCode.Equals("DE_705"))
                        {
                            res.Msg = nameof(ErrorCodes.Unknown_Error).Replace("_", " ");
                            res.ErrorCode = ErrorCodes.Unknown_Error;
                            res.LiveID = res.Msg + " DE_705";
                        }
                    }
                    else
                    {
                        res.Msg = paytmResp.statusMessage;

                    }
                }
            }
            catch (Exception ex)
            {
                apiResp = " Exception:" + ex.Message + " | " + apiResp;
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "AccountTransfer",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = request.UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            var dMTReq = new DMTReqRes
            {
                APIID = request.APIID,
                Method = "AccountTransfer",
                RequestModeID = request.RequestMode,
                Request = URL + JsonConvert.SerializeObject(dataRequest) + JsonConvert.SerializeObject(headers),
                Response = apiResp ?? string.Empty,
                SenderNo = request.SenderMobile,
                UserID = request.UserID,
                TID = request.TID.ToString()
            };
            new ProcUpdateLogDMRReqResp(_dal).Call(dMTReq);
            res.Request = dMTReq.Request;
            res.Response = dMTReq.Response;
            if (res.Statuscode == RechargeRespType.PENDING)
            {
                var tempres = LoopStatusCheck(request.TID, request.RequestMode, request.UserID, ChanelType == PaymentMode_.PAYTM_Neft).Result;
                res.Statuscode = tempres.Statuscode;
                res.LiveID = tempres.LiveID;
                res.VendorID = tempres.VendorID;
                res.Msg = tempres.Msg;
                res.ErrorCode = tempres.ErrorCode;
            }
            return res;
        }

        public MSenderLoginResponse RemoveBeneficiaryValidate(MTAPIRequest request)
        {
            throw new NotImplementedException();
        }

        public MSenderCreateResp SenderEKYC(MTAPIRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
