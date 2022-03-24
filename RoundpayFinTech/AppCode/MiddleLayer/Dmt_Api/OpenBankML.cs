using Fintech.AppCode.Configuration;
using Fintech.AppCode.DB;
using Fintech.AppCode.HelperClass;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using Fintech.AppCode.WebRequest;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RoundpayFinTech.AppCode.DL;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.MoneyTransfer;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.ThirdParty.OpenBank;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.MiddleLayer.Dmt_Api
{
    public partial class OpenBankML
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        private readonly IConfiguration Configuration;
        private readonly IConnectionConfiguration _c;
        private readonly OpenBankAppSetting openBankAppSetting;
        private readonly int _APIID;
        private readonly IDAL _dal;
        public OpenBankML(IHttpContextAccessor accessor, IHostingEnvironment env, int APIID)
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
            openBankAppSetting = OPENBankAppSetting();
            _APIID = APIID;
        }
        private OpenBankAppSetting OPENBankAppSetting()
        {
            var setting = new OpenBankAppSetting();
            try
            {
                setting = new OpenBankAppSetting
                {
                    Version = Convert.ToInt16(Configuration["DMR:OPENBNK:version"]),
                    SecretKey = Configuration["DMR:OPENBNK:secretKey"],
                    AccessKey = Configuration["DMR:OPENBNK:accessKey"],
                    debit_account_number = Configuration["DMR:OPENBNK:debit_account_number"],
                    BaseURL = Configuration["DMR:OPENBNK:BaseURL"],
                    VerificationURL = Configuration["DMR:OPENBNK:VerificationURL"],
                    VerifyAuth = Configuration["DMR:OPENBNK:VerifyAuth"]
                };
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "OpenBankAppSetting",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return setting;
        }

        //ProcGetSenderLimit
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
                    _BankID=addBeni.BankID
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

        public DMRTransactionResponse Verification(DMTReq _req, ReqSendMoney sendMoney, DMRTransactionResponse res)
        {
            res.Statuscode = RechargeRespType.PENDING;
            res.Msg = RechargeRespType._PENDING;
            res.VendorID = "";
            res.LiveID = "";
            res = DMTAPIHelperML.AccountVerification(openBankAppSetting.VerificationURL, openBankAppSetting.VerifyAuth, sendMoney.AccountNo, sendMoney.IFSC, _req.TID, _dal);
            new ProcUpdateLogDMRReqResp(_dal).Call(new DMTReqRes
            {
                APIID = _req.ApiID,
                Method = "Verification",
                RequestModeID = _req.RequestMode,
                Request = res.Request,
                Response = res.Response,
                SenderNo = _req.SenderNO,
                UserID = _req.UserID,
                TID = _req.TID
            });
            res.Request = string.Empty;
            res.Response = string.Empty;
            return res;
        }
        public async Task<DMRTransactionResponse> InitiatePayouts(DMTReq _req, ReqSendMoney sendMoney, DMRTransactionResponse res)
        {
            res.Statuscode = RechargeRespType.PENDING;
            res.Msg = nameof(ErrorCodes.Request_Accpeted).Replace("_", " ");
            res.ErrorCode = ErrorCodes.Request_Accpeted;
            res.LiveID = res.Msg;

            if (openBankAppSetting != null)
            {
                string response = "", request = "";
                try
                {
                    SenderRequest senderRequest = (SenderRequest)new ProcGetSender(_dal).Call(_req.SenderNO);
                    var client_timestamp_header = (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000).ToString();
                    var client_request_method = "POST";
                    var body_request = new OpenBankIntiateReq
                    {
                        bene_account_number = sendMoney.AccountNo,
                        ifsc_code = sendMoney.IFSC,
                        recepient_name = senderRequest.Name,
                        email_id = _req.EmailID,//EmailID of User
                        mobile_number = _req.SenderNO,
                        debit_account_number = openBankAppSetting.debit_account_number,
                        amount = sendMoney.Amount + ".00",
                        transaction_types_id = sendMoney.Channel ? OpenTransactionType.IMPS : OpenTransactionType.NEFT,
                        merchant_ref_id = res.TID.ToString(),
                        otp = "123456",
                        purpose = "integration " + res.TID
                    };
                    var reqString = client_timestamp_header + client_request_method + JsonConvert.SerializeObject(body_request);

                    reqString = reqString.Replace(@"\r\n", string.Empty);
                    reqString = reqString.Replace(" ", string.Empty);
                    var ReqSignature = HashEncryption.O.SHA256_ComputeHash(reqString, openBankAppSetting.SecretKey);
                    var headers = new Dictionary<string, string>
                    {
                         { "Authorization","Bearer " + openBankAppSetting.AccessKey+":"+ReqSignature},
                         { ContentType.Self,ContentType.application_json},
                        { "X-O-Timestamp",client_timestamp_header}
                    };
                    var URL = openBankAppSetting.BaseURL + "payouts";
                    string postData = JsonConvert.SerializeObject(body_request);
                    request = URL + JsonConvert.SerializeObject(headers) + "?" + postData;
                    response = await AppWebRequest.O.HWRPostAsync(URL, postData, headers).ConfigureAwait(false);
                    var _apiRes = JsonConvert.DeserializeObject<OpenBankResp>(response);
                    if (_apiRes != null)
                    {
                        if (_apiRes.status > 0)
                        {
                            if (_apiRes.status == 200 && _apiRes.data != null)
                            {
                                if (_apiRes.data.transaction_status_id == 15)
                                {
                                    res.Statuscode = RechargeRespType.SUCCESS;
                                    res.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                                    res.ErrorCode = ErrorCodes.Transaction_Successful;
                                    res.LiveID = _apiRes.data.open_transaction_ref_id ?? string.Empty;
                                    res.VendorID = _apiRes.data.open_transaction_ref_id ?? string.Empty;
                                }
                                else if (_apiRes.data.transaction_status_id == 17)
                                {
                                    res.Statuscode = RechargeRespType.FAILED;
                                    res.Msg = _apiRes.data.bank_error_message;
                                    res.ErrorCode = ErrorCodes.Unknown_Error;
                                    res.LiveID = _apiRes.data.bank_error_message;
                                }
                                else
                                {
                                    res.Statuscode = RechargeRespType.PENDING;
                                    res.Msg = nameof(ErrorCodes.Request_Accpeted).Replace("_", " ");
                                    res.ErrorCode = ErrorCodes.Request_Accpeted;
                                    res.LiveID = _apiRes.data.open_transaction_ref_id;
                                }
                            }
                            else if (_apiRes.status == 502 && _apiRes.data != null)
                            {
                                res.Statuscode = RechargeRespType.FAILED;
                                res.Msg = _apiRes.data.bank_error_message;
                                res.ErrorCode = ErrorCodes.Unknown_Error;
                                res.LiveID = _apiRes.data.bank_error_message;
                            }
                            else if (_apiRes.status == 422 && _apiRes.data != null)
                            {
                                res.Statuscode = RechargeRespType.FAILED;
                                res.Msg = _apiRes.data.bank_error_message;
                                res.ErrorCode = ErrorCodes.Unknown_Error;
                                res.LiveID = _apiRes.data.bank_error_message;
                            }
                            else if (_apiRes.status == 412)
                            {
                                res.Statuscode = RechargeRespType.FAILED;
                                res.Msg = _apiRes.message;
                                res.ErrorCode = ErrorCodes.Unknown_Error;
                                res.LiveID = _apiRes.message;
                            }
                        }
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
                    Request = request,
                    Response = response,
                    SenderNo = _req.SenderNO,
                    UserID = _req.UserID,
                    TID = _req.TID
                };
                new ProcUpdateLogDMRReqResp(_dal).Call(dMTReq);
            }
            return res;
        }

        public async Task<DMRTransactionResponse> GetPayout(int TID, int RequestMode, int UserID)
        {
            var res = new DMRTransactionResponse
            {
                Statuscode = RechargeRespType.PENDING,
                Msg = nameof(RechargeRespType.PENDING),
                ErrorCode = ErrorCodes.Request_Accpeted
            };
            var client_timestamp_header = (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000).ToString();
            var client_request_method = "GET";
            var reqString = client_timestamp_header + client_request_method + TID;

            var ReqSignature = HashEncryption.O.SHA256_ComputeHash(reqString, openBankAppSetting.SecretKey);
            var headers = new Dictionary<string, string>
            {
                    { "Authorization","Bearer " + openBankAppSetting.AccessKey+":"+ReqSignature},
                    { ContentType.Self,ContentType.application_json},
                { "X-O-Timestamp",client_timestamp_header}
            };
            string URL = openBankAppSetting.BaseURL + "payouts/" + TID;
            string resp = string.Empty, request = string.Empty;
            try
            {
                resp = await AppWebRequest.O.CallUsingHttpWebRequest_GETAsync(URL, headers).ConfigureAwait(false);
                var _apiRes = JsonConvert.DeserializeObject<OpenBankResp>(resp);
                if (_apiRes != null)
                {
                    request = URL + JsonConvert.SerializeObject(headers);
                    if (_apiRes.status > 0)
                    {
                        if (_apiRes.status == 200 && _apiRes.data != null)
                        {
                            if (_apiRes.data.transaction_status_id == 15)
                            {
                                res.Statuscode = RechargeRespType.SUCCESS;
                                res.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                                res.ErrorCode = ErrorCodes.Transaction_Successful;
                                res.LiveID = _apiRes.data.open_transaction_ref_id ?? string.Empty;
                                res.VendorID = _apiRes.data.open_transaction_ref_id ?? string.Empty;
                            }
                            else if (_apiRes.data.transaction_status_id == 17)
                            {
                                res.Statuscode = RechargeRespType.FAILED;
                                res.Msg = _apiRes.data.bank_error_message;
                                res.ErrorCode = ErrorCodes.Unknown_Error;
                                res.LiveID = _apiRes.data.bank_error_message;
                            }
                            else
                            {
                                res.Statuscode = RechargeRespType.PENDING;
                                res.Msg = nameof(ErrorCodes.Request_Accpeted).Replace("_", " ");
                                res.ErrorCode = ErrorCodes.Request_Accpeted;
                                res.LiveID = _apiRes.data.open_transaction_ref_id;
                            }
                        }
                        else if (_apiRes.status == 502 && _apiRes.data != null)
                        {
                            res.Statuscode = RechargeRespType.FAILED;
                            res.Msg = _apiRes.data.bank_error_message;
                            res.ErrorCode = ErrorCodes.Unknown_Error;
                            res.LiveID = _apiRes.data.bank_error_message;
                        }
                        else if (_apiRes.status == 422 && _apiRes.data != null)
                        {
                            res.Statuscode = RechargeRespType.FAILED;
                            res.Msg = _apiRes.data.bank_error_message;
                            res.ErrorCode = ErrorCodes.Unknown_Error;
                            res.LiveID = _apiRes.data.bank_error_message;
                        }
                        else if (_apiRes.status == 412)
                        {
                            res.Statuscode = RechargeRespType.FAILED;
                            res.Msg = _apiRes.message;
                            res.ErrorCode = ErrorCodes.Unknown_Error;
                            res.LiveID = _apiRes.message;
                        }
                    }
                }
                res.Request = request;
                res.Response = resp;
            }
            catch (Exception ex)
            {
                resp = " Exception:" + ex.Message + " | " + resp;
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "GetPayout",
                    Error = ex.Message,
                    LoginTypeID = 1,
                    UserId = UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            var dMTReq = new DMTReqRes
            {
                APIID = _APIID,
                Method = "GetPayout",
                RequestModeID = RequestMode,
                Request = res.Request,
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
    public partial class OpenBankML : IMoneyTransferAPIML
    {
        public OpenBankML(IHttpContextAccessor accessor, IHostingEnvironment env)
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
            openBankAppSetting = OPENBankAppSetting();
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
                    _BankID = request.BankID
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
            res = DMTAPIHelperML.AccountVerification(openBankAppSetting.VerificationURL, openBankAppSetting.VerifyAuth, request.mBeneDetail.AccountNo, request.mBeneDetail.IFSC, request.TID.ToString(), _dal);
            new ProcUpdateLogDMRReqResp(_dal).Call(new DMTReqRes
            {
                APIID = request.APIID,
                Method = "Verification",
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
                ErrorCode = ErrorCodes.Request_Accpeted
            };
            if (openBankAppSetting != null)
            {
                string response = "";
                try
                {
                    SenderRequest senderRequest = (SenderRequest)new ProcGetSender(_dal).Call(request.SenderMobile);
                    var client_timestamp_header = (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000).ToString();
                    var client_request_method = "POST";
                    var body_request = new OpenBankIntiateReq
                    {
                        bene_account_number = request.mBeneDetail.AccountNo,
                        ifsc_code = request.mBeneDetail.IFSC,
                        recepient_name = senderRequest.Name,
                        email_id = request.EmailID,//EmailID of User
                        mobile_number = request.SenderMobile,
                        debit_account_number = openBankAppSetting.debit_account_number,
                        amount = request.Amount + ".00",
                        transaction_types_id = request.TransMode.Equals("IMPS") ? OpenTransactionType.IMPS : OpenTransactionType.NEFT,
                        merchant_ref_id = res.TID.ToString(),
                        otp = "123456",
                        purpose = "integration " + res.TID
                    };
                    var reqString = client_timestamp_header + client_request_method + JsonConvert.SerializeObject(body_request);

                    reqString = reqString.Replace(@"\r\n", string.Empty);
                    reqString = reqString.Replace(" ", string.Empty);
                    var ReqSignature = HashEncryption.O.SHA256_ComputeHash(reqString, openBankAppSetting.SecretKey);
                    var headers = new Dictionary<string, string>
                    {
                         { "Authorization","Bearer " + openBankAppSetting.AccessKey+":"+ReqSignature},
                         { ContentType.Self,ContentType.application_json},
                        { "X-O-Timestamp",client_timestamp_header}
                    };
                    var URL = openBankAppSetting.BaseURL + "payouts";
                    string postData = JsonConvert.SerializeObject(body_request);
                    res.Request = URL + JsonConvert.SerializeObject(headers) + "?" + postData;
                    response = AppWebRequest.O.HWRPostAsync(URL, postData, headers).Result;
                    var _apiRes = JsonConvert.DeserializeObject<OpenBankResp>(response);
                    if (_apiRes != null)
                    {
                        if (_apiRes.status > 0)
                        {
                            if (_apiRes.status == 200 && _apiRes.data != null)
                            {
                                if (_apiRes.data.transaction_status_id == 15)
                                {
                                    res.Statuscode = RechargeRespType.SUCCESS;
                                    res.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                                    res.ErrorCode = ErrorCodes.Transaction_Successful;
                                    res.LiveID = _apiRes.data.open_transaction_ref_id ?? string.Empty;
                                    res.VendorID = _apiRes.data.open_transaction_ref_id ?? string.Empty;
                                }
                                else if (_apiRes.data.transaction_status_id == 17)
                                {
                                    res.Statuscode = RechargeRespType.FAILED;
                                    res.Msg = _apiRes.data.bank_error_message;
                                    res.ErrorCode = ErrorCodes.Unknown_Error;
                                    res.LiveID = _apiRes.data.bank_error_message;
                                }
                                else
                                {
                                    res.Statuscode = RechargeRespType.PENDING;
                                    res.Msg = nameof(ErrorCodes.Request_Accpeted).Replace("_", " ");
                                    res.ErrorCode = ErrorCodes.Request_Accpeted;
                                    res.LiveID = _apiRes.data.open_transaction_ref_id;
                                }
                            }
                            else if (_apiRes.status == 502 && _apiRes.data != null)
                            {
                                res.Statuscode = RechargeRespType.FAILED;
                                res.Msg = _apiRes.data.bank_error_message;
                                res.ErrorCode = ErrorCodes.Unknown_Error;
                                res.LiveID = _apiRes.data.bank_error_message;
                            }
                            else if (_apiRes.status == 422 && _apiRes.data != null)
                            {
                                res.Statuscode = RechargeRespType.FAILED;
                                res.Msg = _apiRes.data.bank_error_message;
                                res.ErrorCode = ErrorCodes.Unknown_Error;
                                res.LiveID = _apiRes.data.bank_error_message;
                            }
                            else if (_apiRes.status == 412)
                            {
                                res.Statuscode = RechargeRespType.FAILED;
                                res.Msg = _apiRes.message;
                                res.ErrorCode = ErrorCodes.Unknown_Error;
                                res.LiveID = _apiRes.message;
                            }
                        }
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