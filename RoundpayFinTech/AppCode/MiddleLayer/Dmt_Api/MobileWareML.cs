using Fintech.AppCode.Configuration;
using Fintech.AppCode.DB;
using Fintech.AppCode.StaticModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using RoundpayFinTech.AppCode.DL;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System.IO;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.StaticModel.MoneyTransfer;
using com.mobileware.transxt;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model.MoneyTransfer;

namespace RoundpayFinTech.AppCode.MiddleLayer
{
    public partial class MobileWareML
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        private readonly IConfiguration Configuration;
        private readonly IDAL _dal;
        private readonly IConnectionConfiguration _c;
        private readonly MWAppSetting appSetting;
        public MobileWareML(IHttpContextAccessor accessor, IHostingEnvironment env)
        {
            _accessor = accessor;
            _env = env;
            bool IsProd = _env.IsProduction();
            var builder = new ConfigurationBuilder()
                 .SetBasePath(Directory.GetCurrentDirectory());
            builder.AddJsonFile((IsProd ? "appsettings.json" : "appsettings.Development.json"));
            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
            _c = new ConnectionConfiguration(_accessor, _env);
            _dal = new DAL(_c.GetConnectionString());
            appSetting = GetApiDetail();
        }
        private MWAppSetting GetApiDetail()
        {
            var setting = new MWAppSetting();
            try
            {
                setting.Token = Configuration["DMR:AMAH:Token"];
                setting.Username = Configuration["DMR:AMAH:Username"];
                setting.Password = Configuration["DMR:AMAH:Password"];
                setting.BaseURL = Configuration["DMR:AMAH:BaseURL"];
                setting.AuthUrl = Configuration["DMR:AMAH:AuthUrl"];
                setting.CheckSumUrl = Configuration["DMR:AMAH:CheckSumUrl"];

                TransxtConfig.AuthToken = setting.Token;
                TransxtConfig.Username = setting.Username;
                TransxtConfig.Password = setting.Password;
                TransxtConfig.AuthUrl = setting.AuthUrl;
                TransxtConfig.CheckSumUrl = setting.CheckSumUrl;
            }
            catch { }
            return setting;
        }
        public ResponseStatus CheckSender(DMTReq _req)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            string url = appSetting.BaseURL + "fetchcustomer", response = "", request = "";
            try
            {
                var param = new MaheshcommParam
                {
                    agentCode = "1",
                    customerId = _req.SenderNO
                };
                request = JsonConvert.SerializeObject(param);
                response = TransXTCommunicator.execute(request, url);
                CheckSenderRespones _apiRes = JsonConvert.DeserializeObject<CheckSenderRespones>(response);
                if (_apiRes != null)
                {
                    if (_apiRes.errorCode == "00")
                    {
                        res.Statuscode = ErrorCodes.One;
                        res.Msg = ErrorCodes.DMTSLS;
                        res.CommonStr = _apiRes.response.walletbal;
                        res.CommonStr2 = _apiRes.response.name;
                    }
                    else if (_apiRes.errorCode == "E1504")
                    {
                        res.Statuscode = ErrorCodes.One;
                        res.Msg = ErrorCodes.DMTSNE;
                        res.CommonInt = ErrorCodes.One;
                    }
                    else
                    {
                        res.Msg = _apiRes.errorMsg;
                    }
                }
                else
                {
                    res.Msg = ErrorCodes.NORESPONSE;
                }
            }
            catch (Exception ex)
            {
                response = ex.Message + "|" + response;
                ErrorLog errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "CheckSander",
                    Error = ex.Message,
                    LoginTypeID = _req.LT,
                    UserId = _req.UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            var dMTReq = new DMTReqRes
            {
                APIID = _req.ApiID,
                Method = "CheckSander",
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
        public ResponseStatus SenderOTPResend(DMTReq _req, int Type)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            string url = appSetting.BaseURL + "otp", response = "", request = "";
            SendOTP param = new SendOTP
            {
                agentCode = "1",
                customerId = _req.SenderNO,
                otpType = Type.ToString(),
                txnId = _req.TID
            };
            try
            {
                request = JsonConvert.SerializeObject(param);
                response = TransXTCommunicator.execute(request, url);
                SendOTPRespones _apiRes = JsonConvert.DeserializeObject<SendOTPRespones>(response);
                if (_apiRes.errorCode == "00")
                {
                    res.Statuscode = ErrorCodes.One;
                    res.Msg = ErrorCodes.DMTOSS;
                    res.CommonInt = ErrorCodes.One;
                }
                else
                {
                    res.Msg = _apiRes.errorMsg;
                }
            }
            catch (Exception ex)
            {
                response = ex.Message + "|" + response;
                ErrorLog errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "SenderOTPResend",
                    Error = ex.Message,
                    LoginTypeID = _req.LT,
                    UserId = _req.UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            DMTReqRes dMTReq = new DMTReqRes
            {
                APIID = _req.ApiID,
                Method = "SenderOTPResend",
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
        public ResponseStatus CreateSender(CreateSen _req, int Type)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            string url = appSetting.BaseURL + "otp", response = "", request = "";
            SendOTP param = new SendOTP
            {
                agentCode = "1",
                customerId = _req.dMTReq.SenderNO,
                otpType = Type.ToString(),
                txnId = _req.dMTReq.TID
            };
            try
            {
                request = JsonConvert.SerializeObject(param);
                response = TransXTCommunicator.execute(request, url);
                SendOTPRespones _apiRes = JsonConvert.DeserializeObject<SendOTPRespones>(response);
                if (_apiRes.errorCode == "00")
                {
                    if (Type == 1)
                    {
                        _req.senderRequest.UserID = _req.dMTReq.UserID;
                        _req.senderRequest.ReffID = _apiRes.response.initiatorId;
                        _req.senderRequest.RequestNo = _apiRes.txnId;
                        _req.senderRequest.Name = _req.senderRequest.Name + " " + _req.senderRequest.LastName;
                        new ProcUpdateSender(_dal).Call(_req.senderRequest);
                    }
                    res.Statuscode = ErrorCodes.One;
                    res.Msg = ErrorCodes.DMTOSS;
                    res.CommonInt = ErrorCodes.One;
                }
                else
                {
                    res.Msg = _apiRes.errorMsg;
                }
            }
            catch (Exception ex)
            {
                response = ex.Message + "|" + response;
                ErrorLog errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "CreateSender",
                    Error = ex.Message,
                    LoginTypeID = _req.dMTReq.LT,
                    UserId = _req.dMTReq.UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            var dMTReq = new DMTReqRes
            {
                APIID = _req.dMTReq.ApiID,
                Method = "CreateSender",
                RequestModeID = _req.dMTReq.RequestMode,
                Request = request + "|" + JsonConvert.SerializeObject(_req),
                Response = response + "|" + JsonConvert.SerializeObject(res),
                SenderNo = _req.dMTReq.SenderNO,
                UserID = _req.dMTReq.UserID,
                TID = _req.dMTReq.TID
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
            SenderRequest senderRequest = (SenderRequest)new ProcGetSender(_dal).Call(_req.SenderNO);
            if (string.IsNullOrWhiteSpace(senderRequest.Name))
            {
                res.Msg = ErrorCodes.DMTSNE;
                return res;
            }
            string url = appSetting.BaseURL + "createcustomer", response = "", request = "";
            try
            {
                CreateSender param = new CreateSender
                {
                    agentCode = "1",
                    customerId = _req.SenderNO,
                    address = senderRequest.Address,
                    dateOfBirth = Convert.ToDateTime(senderRequest.Dob).ToString("yyyy-MM-dd"),
                    name = senderRequest.Name,
                    otp = OTP
                };
                request = JsonConvert.SerializeObject(param);
                response = TransXTCommunicator.execute(request, url);
                ResponesCreateSender _apiRes = JsonConvert.DeserializeObject<ResponesCreateSender>(response);
                if (_apiRes.errorCode == "00")
                {
                    res.Statuscode = ErrorCodes.One;
                    res.Msg = ErrorCodes.DMTSCS;
                }
                else
                {
                    res.Msg = _apiRes.errorMsg;
                }
            }
            catch (Exception ex)
            {
                response = ex.Message + "|" + response;
                ErrorLog errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "VerifySender",
                    Error = ex.Message,
                    LoginTypeID = _req.LT,
                    UserId = _req.UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            DMTReqRes dMTReq = new DMTReqRes
            {
                APIID = _req.ApiID,
                Method = "CheckSander",
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
        public ResponseStatus CreateBeneficiary(AddBeni addBeni, DMTReq _req)
        {
            string url = appSetting.BaseURL + "addrecipient", response = "", request = "";
            ResponseStatus res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };

            try
            {
                AddReceipent param = new AddReceipent
                {
                    agentCode = "1",
                    customerId = addBeni.SenderMobileNo,
                    mobileNo = addBeni.MobileNo,
                    recipientName = addBeni.BeneName,
                    recipientType = "2",
                    udf1 = addBeni.AccountNo,
                    udf2 = addBeni.IFSC,

                };
                request = JsonConvert.SerializeObject(param);
                response = TransXTCommunicator.execute(JsonConvert.SerializeObject(param), url);
                CreateBeneResponse _apiRes = JsonConvert.DeserializeObject<CreateBeneResponse>(response);
                if (_apiRes.errorCode == "00")
                {
                    res.Statuscode = ErrorCodes.One;
                    res.Msg = ErrorCodes.BENESCS;
                    return res;
                }
                else
                {
                    res.Msg = _apiRes.errorMsg;
                }
            }
            catch (Exception ex)
            {
                response = ex.Message + "|" + response;
                ErrorLog errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "CreateBeneficiary",
                    Error = ex.Message,
                    LoginTypeID = _req.LT,
                    UserId = _req.UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            DMTReqRes dMTReq = new DMTReqRes
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
        public DMRTransactionResponse Verification(DMTReq _req, ReqSendMoney sendMoney, DMRTransactionResponse res)
        {
            res.Statuscode = RechargeRespType.PENDING;
            res.Msg = RechargeRespType._PENDING;
            res.VendorID = "";
            res.LiveID = "";
            string url = appSetting.BaseURL + "recipientenquiry", response = "", request = "";
            try
            {
                var param = new VerifyBene
                {
                    agentCode = "1",
                    customerId = sendMoney.MobileNo,
                    recipientType = "2",
                    udf1 = sendMoney.AccountNo,
                    udf2 = sendMoney.IFSC,
                    channel = "1",
                    clientRefId = _req.TID,
                    currency = "INR"
                };
                request = JsonConvert.SerializeObject(param);
                response = TransXTCommunicator.execute(JsonConvert.SerializeObject(param), url);
                VerificationBene _apiRes = JsonConvert.DeserializeObject<VerificationBene>(response);
                if (_apiRes.errorCode == "00" || _apiRes.errorCode == "E1640")
                {
                    res.Statuscode = RechargeRespType.SUCCESS;
                    res.Msg = _apiRes.errorMsg;
                    res.BeneName = _apiRes.response.name ?? "";
                    res.VendorID = _apiRes.txnId;
                    res.LiveID = _apiRes.response.txnId;
                }
                else if (_apiRes.errorCode.In("E1501", "F0013"))
                {
                    res.Statuscode = RechargeRespType.PENDING;
                    res.Msg = _apiRes.errorMsg;
                    res.VendorID = _apiRes.txnId;
                    res.LiveID = res.Msg;
                }
                else
                {
                    res.Statuscode = RechargeRespType.FAILED;
                    res.Msg = _apiRes.errorMsg.ToLower().Contains("sufficient") ? ErrorCodes.Down : _apiRes.errorMsg;
                    res.VendorID = _apiRes.txnId;
                    res.LiveID = res.Msg;
                }
                res.Request = request;
                res.Response = response;
            }
            catch (Exception ex)
            {
                response = ex.Message + "|" + response;
                ErrorLog errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Verification",
                    Error = ex.Message,
                    LoginTypeID = _req.LT,
                    UserId = _req.UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            DMTReqRes dMTReq = new DMTReqRes
            {
                APIID = _req.ApiID,
                Method = "Verification",
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
        public ResponseStatus DeleteBeneficiary(DMTReq _req, string BeniID)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            string url = appSetting.BaseURL + "deleterecipient", response = "", request = "";
            DeleteRecepient param = new DeleteRecepient
            {
                agentCode = "1",
                customerId = _req.SenderNO,
                recipientId = BeniID
            };
            try
            {
                request = JsonConvert.SerializeObject(param);
                response = TransXTCommunicator.execute(request, url);
                ResponesCreateSender _apiRes = JsonConvert.DeserializeObject<ResponesCreateSender>(response);
                if (_apiRes.errorCode == "00")
                {
                    res.Statuscode = ErrorCodes.One;
                    res.Msg = ErrorCodes.DMTDBS;
                }
                else
                {
                    res.Msg = _apiRes.errorMsg;
                }
            }
            catch (Exception ex)
            {
                response = ex.Message + "|" + response;
                ErrorLog errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "DeleteBeneficiary",
                    Error = ex.Message,
                    LoginTypeID = _req.LT,
                    UserId = _req.UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            DMTReqRes dMTReq = new DMTReqRes
            {
                APIID = _req.ApiID,
                Method = "DeleteBeneficiary",
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
        public BeniRespones GetBeneficiary(DMTReq _req)
        {
            var res = new BeniRespones
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            string url = appSetting.BaseURL + "fetchallrecipient", response = "", request = "";
            try
            {
                MaheshcommParam param = new MaheshcommParam
                {
                    agentCode = "1",
                    customerId = _req.SenderNO
                };
                request = JsonConvert.SerializeObject(param);
                response = TransXTCommunicator.execute(request, url);
                GetBeneficiary _ApiRes = JsonConvert.DeserializeObject<GetBeneficiary>(response);
                if (_ApiRes.errorCode == "00")
                {
                    res.Statuscode = ErrorCodes.One;
                    res.Msg = ErrorCodes.SUCCESS;
                    List<AddBeni> ListBeni = new List<AddBeni>();
                    foreach (var item in _ApiRes.response.recipientList)
                    {
                        var addBeni = new AddBeni
                        {
                            AccountNo = item.udf1,
                            BankName = item.bankName,
                            IFSC = item.udf2,
                            BeneName = item.recipientName,
                            MobileNo = item.mobileNo,
                            BeneID = item.recipientId
                        };
                        ListBeni.Add(addBeni);
                    }
                    res.addBeni = ListBeni;
                }
                else
                {
                    res.Msg = _ApiRes.errorMsg;
                }
            }
            catch (Exception ex)
            {
                response = ex.Message + "|" + response;
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
            DMTReqRes dMTReq = new DMTReqRes
            {
                APIID = _req.ApiID,
                Method = "GetBeneficiary",
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
        public DMRTransactionResponse SendMoney(DMTReq _req, ReqSendMoney sendMoney, DMRTransactionResponse res)
        {
            res.Statuscode = RechargeRespType.PENDING;
            res.Msg = RechargeRespType._PENDING;
            string url = appSetting.BaseURL + "doremit", response = "", request = "";
            try
            {
                var param = new SendMoney
                {
                    agentCode = "1",
                    customerId = _req.SenderNO,
                    amount = sendMoney.Amount + ".00",
                    channel = sendMoney.Channel ? "1" : "2",
                    recSeqId = sendMoney.BeneID,
                    currency = "INR",
                    clientRefId = _req.TID,
                    tp1 = "",
                    tp2 = "",
                    tp3 = "",
                    tp4 = "",
                    tp5 = "",
                    tp6 = ""
                };
                request = JsonConvert.SerializeObject(param);
                response = TransXTCommunicator.execute(request, url);
                var _apiRes = JsonConvert.DeserializeObject<VerificationBene>(response);
                if (_apiRes.errorCode == "00" || _apiRes.errorCode == "E1640")
                {
                    res.Statuscode = RechargeRespType.SUCCESS;
                    res.Msg = _apiRes.errorMsg;
                    res.LiveID = _apiRes.response.txnId ?? "";
                    res.VendorID = _apiRes.txnId;
                }
                else if (_apiRes.errorCode != "E1501" && _apiRes.errorCode != "F0013")
                {
                    res.Statuscode = RechargeRespType.FAILED;
                    res.Msg = _apiRes.errorCode.In("E0049", "E0906") ? ErrorCodes.Down : _apiRes.errorMsg;
                    res.LiveID = (_apiRes.errorMsg ?? "").Contains("Insufficient") ? string.Empty : _apiRes.errorMsg;
                    res.VendorID = _apiRes.txnId;
                }
                else
                {
                    res.Statuscode = RechargeRespType.PENDING;
                    res.Msg = _apiRes.errorMsg ?? "";
                    res.LiveID = (_apiRes.errorMsg ?? "").Contains("Insufficient") ? string.Empty : _apiRes.errorMsg;
                    res.VendorID = _apiRes.txnId ?? "";
                }
                res.Request = request;
                res.Response = response;
            }
            catch (Exception ex)
            {
                response = ex.Message + "|" + response;
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "SendMoney",
                    Error = ex.Message,
                    LoginTypeID = _req.LT,
                    UserId = _req.UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            var dMTReq = new DMTReqRes
            {
                APIID = _req.ApiID,
                Method = "SendMoney",
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
        public DMRTransactionResponse SearchTransaction(string URL, string RPID, string VendorID, int APIID = 0)
        {
            var res = new DMRTransactionResponse
            {
                Statuscode = RechargeRespType.PENDING,
                Msg = RechargeRespType._PENDING,
                Request = URL
            };
            string response = "", request = "";
            var param = new
            {
                txnid = VendorID,
                clientRefId = RPID
            };
            request = JsonConvert.SerializeObject(param);
            res.Request = res.Request + "?" + request;
            try
            {
                response = TransXTCommunicator.execute(request, URL);
                var _apiRes = JsonConvert.DeserializeObject<SearhTraxResp>(response);
                if (_apiRes.errorCode == "00" || _apiRes.errorCode == "E1640")
                {
                    res.Statuscode = RechargeRespType.SUCCESS;
                    res.Msg = _apiRes.errorMsg;
                    res.LiveID = _apiRes.response.txnId ?? "";
                    res.VendorID = VendorID ?? "";
                }
                else if (_apiRes.errorCode.In("E1502", "E1657", "E0049", "E1613"))
                {
                    res.Statuscode = RechargeRespType.FAILED;
                    res.Msg = _apiRes.errorCode == "E0049" ? ErrorCodes.Down : _apiRes.errorMsg;
                    res.LiveID = res.Msg;
                    res.VendorID = VendorID ?? "";
                }
                else
                {
                    res.Statuscode = RechargeRespType.PENDING;
                    res.Msg = _apiRes.errorMsg ?? "";
                    res.LiveID = _apiRes.errorMsg ?? "";
                    res.VendorID = VendorID ?? "";
                }
                res.Request = request;
                res.Response = response;
            }
            catch (Exception ex)
            {
                response = ex.Message + "|" + response;
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "SearchTransaction",
                    Error = ex.Message + "_against RPID:" + (RPID ?? "") + ":VendorID" + VendorID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            var dMTReq = new DMTReqRes
            {
                APIID = APIID,
                Method = "SearchTransaction",
                RequestModeID = 0,
                Request = request,
                Response = response,
                SenderNo = "",
                UserID = 0,
                TID = ""
            };
            new ProcUpdateLogDMRReqResp(_dal).Call(dMTReq);
            return res;
        }
    }
    public partial class MobileWareML : IMoneyTransferAPIML
    {
        public MSenderLoginResponse GetSender(MTAPIRequest request)
        {
            var res = new MSenderLoginResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = nameof(ErrorCodes.Unknown_Error).Replace("_", " "),
                ErrorCode = ErrorCodes.Unknown_Error
            };
            string _URL = appSetting.BaseURL + "fetchcustomer", response = string.Empty;
            var param = new MaheshcommParam
            {
                agentCode = "1",
                customerId = request.SenderMobile
            };
            try
            {
                response = TransXTCommunicator.execute(JsonConvert.SerializeObject(param), _URL);
                CheckSenderRespones _apiRes = JsonConvert.DeserializeObject<CheckSenderRespones>(response);
                if (_apiRes != null)
                {
                    if (_apiRes.errorCode == "00")
                    {
                        res.Statuscode = ErrorCodes.One;
                        res.Msg = ErrorCodes.DMTSLS;
                        res.ErrorCode = ErrorCodes.Transaction_Successful;
                        res.RemainingLimit = Convert.ToDecimal(_apiRes.response.walletbal);
                        res.AvailbleLimit = Convert.ToDecimal(_apiRes.response.totalMonthlyLimit);
                        res.KYCStatus = SenderKYCStatus.ACTIVE;
                        res.SenderName = _apiRes.response.name;
                    }
                    else if (_apiRes.errorCode == "E1504")
                    {
                        res.Statuscode = ErrorCodes.One;
                        res.Msg = nameof(DMTErrorCodes.Sender_Not_Found).Replace("_", " ");
                        res.IsSenderNotExists = true;
                        res.ErrorCode = DMTErrorCodes.Sender_Not_Found;
                    }
                    else
                    {
                        res.Msg = _apiRes.errorMsg;
                    }
                }
                else
                {
                    res.Msg = ErrorCodes.NORESPONSE;
                }
            }
            catch (Exception ex)
            {
                response = ex.Message + "|" + response;
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
            var dMTReq = new DMTReqRes
            {
                APIID = request.APIID,
                Method = "GetSender",
                RequestModeID = request.RequestMode,
                Request = JsonConvert.SerializeObject(param),
                Response = response,
                SenderNo = request.SenderMobile,
                UserID = request.UserID,
                TID = request.TransactionID
            };
            new ProcUpdateLogDMRReqResp(_dal).Call(dMTReq);
            return res;
        }
        public MSenderLoginResponse CreateBeneficiary(MTAPIRequest request)
        {
            string url = appSetting.BaseURL + "addrecipient", response = "";
            var res = new MSenderLoginResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError,
                ErrorCode = ErrorCodes.Unknown_Error
            };
            var param = new AddReceipent
            {
                agentCode = "1",
                customerId = request.SenderMobile,
                mobileNo = request.mBeneDetail.MobileNo,
                recipientName = request.mBeneDetail.BeneName,
                recipientType = "2",
                udf1 = request.mBeneDetail.AccountNo,
                udf2 = request.mBeneDetail.IFSC,

            };
            try
            {
                response = TransXTCommunicator.execute(JsonConvert.SerializeObject(param), url);
                var _apiRes = JsonConvert.DeserializeObject<CreateBeneResponse>(response);
                if (_apiRes.errorCode == "00")
                {
                    res.Statuscode = ErrorCodes.One;
                    res.Msg = ErrorCodes.BENESCS;
                    res.ErrorCode = ErrorCodes.Transaction_Successful;
                    return res;
                }
                else
                {
                    res.Msg = _apiRes.errorMsg;
                }
            }
            catch (Exception ex)
            {
                response = ex.Message + "|" + response;
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "CreateBeneficiary",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = request.UserID
                });
            }
            new ProcUpdateLogDMRReqResp(_dal).Call(new DMTReqRes
            {
                APIID = request.APIID,
                Method = "CreateBeneficiary",
                RequestModeID = request.RequestMode,
                Request = JsonConvert.SerializeObject(param),
                Response = response,
                SenderNo = request.SenderMobile,
                UserID = request.UserID,
                TID = request.TransactionID
            });
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
            string _URL = appSetting.BaseURL + "otp", response = "";
            var param = new SendOTP
            {
                agentCode = "1",
                customerId = request.SenderMobile,
                otpType = "1",
                txnId = request.TransactionID
            };
            try
            {
                response = TransXTCommunicator.execute(JsonConvert.SerializeObject(param), _URL);
                SendOTPRespones _apiRes = JsonConvert.DeserializeObject<SendOTPRespones>(response);
                if (_apiRes.errorCode == "00")
                {
                    new ProcUpdateSender(_dal).Call(new SenderRequest
                    {
                        Name = request.FirstName + " " + request.LastName,
                        MobileNo = request.SenderMobile,
                        Pincode = request.Pincode.ToString(),
                        Address = request.Address,
                        City = request.City,
                        StateID = request.StateID,
                        AadharNo = request.AadharNo,
                        Dob = request.DOB,
                        UserID = request.UserID,
                        ReffID = _apiRes.response.initiatorId,
                        RequestNo = _apiRes.txnId
                    });
                    res.Statuscode = ErrorCodes.One;
                    res.Msg = nameof(DMTErrorCodes.Partial_Registration_Please_verify_sender_with_OTP).Replace("_", " ");
                    res.ErrorCode = DMTErrorCodes.Partial_Registration_Please_verify_sender_with_OTP;
                    res.IsOTPGenerated = true;
                }
                else
                {
                    res.Msg = _apiRes.errorMsg;
                }
            }
            catch (Exception ex)
            {
                response = ex.Message + "|" + response;
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
                Request = _URL + "|" + JsonConvert.SerializeObject(param),
                Response = response + "|" + JsonConvert.SerializeObject(res),
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
            var res = new MBeneficiaryResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError,
                ErrorCode = ErrorCodes.Unknown_Error
            };
            string _URL = appSetting.BaseURL + "fetchallrecipient", response = "";
            var param = new MaheshcommParam
            {
                agentCode = "1",
                customerId = request.SenderMobile
            };
            try
            {

                response = TransXTCommunicator.execute(JsonConvert.SerializeObject(param), _URL);
                var _ApiRes = JsonConvert.DeserializeObject<GetBeneficiary>(response);
                if (_ApiRes.errorCode == "00")
                {
                    res.Statuscode = ErrorCodes.One;
                    res.Msg = ErrorCodes.SUCCESS;
                    var Beneficiaries = new List<MBeneDetail>();
                    foreach (var item in _ApiRes.response.recipientList)
                    {
                        Beneficiaries.Add(new MBeneDetail
                        {
                            AccountNo = item.udf1,
                            BankName = item.bankName,
                            IFSC = item.udf2,
                            BeneName = item.recipientName,
                            MobileNo = item.mobileNo,
                            BeneID = item.recipientId
                        });
                    }
                    res.Beneficiaries = Beneficiaries;
                }
                else
                {
                    res.Msg = _ApiRes.errorMsg;
                }
            }
            catch (Exception ex)
            {
                response = ex.Message + "|" + response;
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "GetBeneficiary",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = request.UserID
                });
            }
            new ProcUpdateLogDMRReqResp(_dal).Call(new DMTReqRes
            {
                APIID = request.APIID,
                Method = "GetBeneficiary",
                RequestModeID = request.RequestMode,
                Request = _URL + "|" + JsonConvert.SerializeObject(param),
                Response = response,
                SenderNo = request.SenderMobile,
                UserID = request.UserID,
                TID = request.TransactionID
            });
            return res;
        }
        public MSenderCreateResp SenderKYC(MTAPIRequest request)
        {
            throw new NotImplementedException();
        }

        public MSenderCreateResp SenderResendOTP(MTAPIRequest request)
        {
            var res = new MSenderCreateResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError,
                ErrorCode = ErrorCodes.Unknown_Error
            };
            string _URL = appSetting.BaseURL + "otp", response = "";
            var param = new SendOTP
            {
                agentCode = "1",
                customerId = request.SenderMobile,
                otpType = "1",
                txnId = request.TransactionID
            };
            try
            {
                response = TransXTCommunicator.execute(JsonConvert.SerializeObject(param), _URL);
                var _apiRes = JsonConvert.DeserializeObject<SendOTPRespones>(response);
                if (_apiRes.errorCode == "00")
                {
                    res.Statuscode = ErrorCodes.One;
                    res.Msg = ErrorCodes.DMTOSS;
                    res.IsOTPGenerated = true;
                }
                else
                {
                    res.Msg = _apiRes.errorMsg;
                }
            }
            catch (Exception ex)
            {
                response = ex.Message + "|" + response;
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "SenderResendOTP",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = request.UserID
                });
            }
            new ProcUpdateLogDMRReqResp(_dal).Call(new DMTReqRes
            {
                APIID = request.APIID,
                Method = "SenderResendOTP",
                RequestModeID = request.RequestMode,
                Request = _URL + "|" + JsonConvert.SerializeObject(param),
                Response = response,
                SenderNo = request.SenderMobile,
                UserID = request.UserID,
                TID = request.TransactionID
            });
            return res;
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
            var senderRequest = (SenderRequest)new ProcGetSender(_dal).Call(request.SenderMobile);
            if (string.IsNullOrWhiteSpace(senderRequest.Name))
            {
                res.Msg = ErrorCodes.DMTSNE;
                return res;
            }
            string url = appSetting.BaseURL + "createcustomer", response = string.Empty;
            var param = new CreateSender
            {
                agentCode = "1",
                customerId = request.SenderMobile,
                address = senderRequest.Address,
                dateOfBirth = Convert.ToDateTime(senderRequest.Dob).ToString("yyyy-MM-dd"),
                name = senderRequest.Name,
                otp = request.OTP
            };
            try
            {
                response = TransXTCommunicator.execute(JsonConvert.SerializeObject(param), url);
                var _apiRes = JsonConvert.DeserializeObject<ResponesCreateSender>(response);
                if (_apiRes.errorCode == "00")
                {
                    res.Statuscode = ErrorCodes.One;
                    res.Msg = ErrorCodes.DMTSCS;
                    res.ErrorCode = ErrorCodes.Transaction_Successful;
                }
                else
                {
                    res.Msg = nameof(ErrorCodes.Invalid_OTP).Replace("_", " ");
                    res.ErrorCode = ErrorCodes.Invalid_OTP;
                }
            }
            catch (Exception ex)
            {
                response = ex.Message + "|" + response;
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
            var dMTReq = new DMTReqRes
            {
                APIID = request.APIID,
                Method = "VerifySender",
                RequestModeID = request.RequestMode,
                Request = url + "|" + JsonConvert.SerializeObject(param),
                Response = response,
                SenderNo = request.SenderMobile,
                UserID = request.UserID,
                TID = request.TransactionID
            };
            new ProcUpdateLogDMRReqResp(_dal).Call(dMTReq);
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
            string url = appSetting.BaseURL + "deleterecipient", response = "";
            var param = new DeleteRecepient
            {
                agentCode = "1",
                customerId = request.SenderMobile,
                recipientId = request.mBeneDetail.BeneID
            };
            try
            {
                response = TransXTCommunicator.execute(JsonConvert.SerializeObject(param), url);
                var _apiRes = JsonConvert.DeserializeObject<ResponesCreateSender>(response);
                if (_apiRes.errorCode == "00")
                {
                    res.Statuscode = ErrorCodes.One;
                    res.Msg = ErrorCodes.DMTDBS;
                    res.ErrorCode = ErrorCodes.Transaction_Successful;
                }
                else
                {
                    res.Msg = _apiRes.errorMsg;
                }
            }
            catch (Exception ex)
            {
                response = ex.Message + "|" + response;
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "RemoveBeneficiary",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = request.UserID
                });
            }
            new ProcUpdateLogDMRReqResp(_dal).Call(new DMTReqRes
            {
                APIID = request.APIID,
                Method = "RemoveBeneficiary",
                RequestModeID = request.RequestMode,
                Request = url + "|" + JsonConvert.SerializeObject(param),
                Response = response,
                SenderNo = request.SenderMobile,
                UserID = request.UserID,
                TID = request.TransactionID
            });
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
            string _URL = appSetting.BaseURL + "recipientenquiry", response = "";
            var param = new VerifyBene
            {
                agentCode = "1",
                customerId = request.SenderMobile,
                recipientType = "2",
                udf1 = request.mBeneDetail.AccountNo,
                udf2 = request.mBeneDetail.IFSC,
                channel = "1",
                clientRefId = request.TID.ToString(),
                currency = "INR"
            };
            try
            {
                res.Request = _URL + "?" + JsonConvert.SerializeObject(param);
                response = TransXTCommunicator.execute(JsonConvert.SerializeObject(param), _URL);
                VerificationBene _apiRes = JsonConvert.DeserializeObject<VerificationBene>(response);
                if (_apiRes.errorCode == "00" || _apiRes.errorCode == "E1640")
                {
                    res.Statuscode = RechargeRespType.SUCCESS;
                    res.Msg = _apiRes.errorMsg;
                    res.BeneName = _apiRes.response.name ?? "";
                    res.VendorID = _apiRes.txnId;
                    res.LiveID = _apiRes.response.txnId;
                }
                else if (_apiRes.errorCode.In("E1501"))
                {
                    res.Statuscode = RechargeRespType.PENDING;
                    res.Msg = _apiRes.errorMsg;
                    res.VendorID = _apiRes.txnId;
                    res.LiveID = res.Msg;
                }
                else
                {
                    res.Statuscode = RechargeRespType.FAILED;
                    res.Msg = _apiRes.errorMsg.ToLower().Contains("sufficient") ? ErrorCodes.Down : _apiRes.errorMsg;
                    res.VendorID = _apiRes.txnId;
                    res.LiveID = res.Msg;
                }
                
                
            }
            catch (Exception ex)
            {
                response = ex.Message + "|" + response;
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "VerifyAccount",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = request.UserID
                });
            }
            res.Response = response;
            new ProcUpdateLogDMRReqResp(_dal).Call(new DMTReqRes
            {
                APIID = request.APIID,
                Method = "VerifyAccount",
                RequestModeID = request.RequestMode,
                Request = res.Request,
                Response = res.Response,
                SenderNo = request.SenderMobile,
                UserID = request.UserID,
                TID = request.TID.ToString()
            });
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
                ErrorCode=ErrorCodes.Request_Accpeted
            };
            string _URL = appSetting.BaseURL + "doremit", response = "";
            var param = new SendMoney
            {
                agentCode = "1",
                customerId = request.SenderMobile,
                amount = request.Amount + ".00",
                channel = request.TransMode.Equals("IMPS") ? "1" : "2",
                recSeqId = request.mBeneDetail.BeneID,
                currency = "INR",
                clientRefId = request.TID.ToString(),
                tp1 = "",
                tp2 = "",
                tp3 = "",
                tp4 = "",
                tp5 = "",
                tp6 = ""
            };
            try
            {
                response = TransXTCommunicator.execute(JsonConvert.SerializeObject(param), _URL);
                var _apiRes = JsonConvert.DeserializeObject<VerificationBene>(response);
                if (_apiRes.errorCode == "00" || _apiRes.errorCode == "E1640")
                {
                    res.Statuscode = RechargeRespType.SUCCESS;
                    res.Msg = _apiRes.errorMsg;
                    res.LiveID = _apiRes.response.txnId ?? "";
                    res.VendorID = _apiRes.txnId;
                    res.ErrorCode = ErrorCodes.Transaction_Successful;
                }
                else if (_apiRes.errorCode != "E1501")
                {
                    res.Statuscode = RechargeRespType.FAILED;
                    res.Msg = _apiRes.errorCode == "E0049" ? ErrorCodes.Down : _apiRes.errorMsg;
                    res.LiveID = res.Msg;
                    res.VendorID = _apiRes.txnId;
                    res.ErrorCode = ErrorCodes.Unknown_Error;
                }
                else
                {
                    res.Statuscode = RechargeRespType.PENDING;
                    res.Msg = _apiRes.errorMsg ?? "";
                    res.LiveID = _apiRes.errorMsg ?? "";
                    res.VendorID = _apiRes.txnId ?? "";
                }                
            }
            catch (Exception ex)
            {
                response = ex.Message + "|" + response;
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "AccountTransfer",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = request.UserID
                });
            }
            res.Request = _URL+"?"+ JsonConvert.SerializeObject(param);
            res.Response = response;
            new ProcUpdateLogDMRReqResp(_dal).Call(new DMTReqRes
            {
                APIID = request.APIID,
                Method = "AccountTransfer",
                RequestModeID = request.RequestMode,
                Request = res.Request,
                Response = response,
                SenderNo = request.SenderMobile,
                UserID = request.UserID,
                TID = request.TID.ToString()
            });
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
