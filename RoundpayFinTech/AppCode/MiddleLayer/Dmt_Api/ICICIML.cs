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
using RoundpayFinTech.AppCode.ThirdParty.Icici;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Validators;

namespace RoundpayFinTech.AppCode.MiddleLayer.Dmt_Api
{
    public partial class ICICIML
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        private readonly IConfiguration Configuration;
        private readonly IConnectionConfiguration _c;
        private readonly ICICIAppSetting appSetting;
        private readonly int _APIID;
        private readonly IDAL _dal;
        public ICICIML(IHttpContextAccessor accessor, IHostingEnvironment env, int APIID)
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
            appSetting = AppSetting();
            _APIID = APIID;
        }
        private ICICIAppSetting AppSetting()
        {
            var setting = new ICICIAppSetting();
            try
            {
                setting = new ICICIAppSetting
                {
                    BC = Configuration["DMR:ICIBNK:BC"],
                    PassCode = Configuration["DMR:ICIBNK:PassCode"],
                    BaseURL = Configuration["DMR:ICIBNK:BaseURL"]
                };
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "ICICIAppSetting",
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
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "CheckSender",
                    Error = ex.Message,
                    LoginTypeID = _req.LT,
                    UserId = _req.UserID
                });
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

        public async Task<DMRTransactionResponse> P2AAccountFundTransfer(DMTReq _req, ReqSendMoney sendMoney, DMRTransactionResponse res)
        {
            res.Statuscode = RechargeRespType.PENDING;
            res.Msg = nameof(ErrorCodes.Request_Accpeted).Replace("_", " ");
            res.ErrorCode = ErrorCodes.Request_Accpeted;
            res.LiveID = res.Msg;

            if (appSetting != null)
            {
                string response = "", request = "";
                try
                {
                    SenderRequest senderRequest = (SenderRequest)new ProcGetSender(_dal).Call(_req.SenderNO);
                    var iciciModel = new ICICIReqModel
                    {
                        Amount = sendMoney.Amount.ToString(),
                        BeneAccNo = sendMoney.AccountNo,
                        BeneIFSC = sendMoney.IFSC,
                        RemName = senderRequest.Name,
                        RemMobile = _req.SenderNO,
                        TranRefNo = res.TID.ToString(),
                        RetailerCode = "rcode",
                        PaymentRef = "FTTransferP2A" + res.TID,
                        TransactionDate = DateTime.Now.ToString("yyyyMMddhhmmss"),
                        PassCode = appSetting.PassCode
                    };
                    var sb = new StringBuilder();
                    sb.Append(nameof(iciciModel.BeneAccNo));
                    sb.Append("=");
                    sb.Append(iciciModel.BeneAccNo);
                    sb.Append("&");
                    sb.Append(nameof(iciciModel.BeneIFSC));
                    sb.Append("=");
                    sb.Append(iciciModel.BeneIFSC);
                    sb.Append("&");
                    sb.Append(nameof(iciciModel.Amount));
                    sb.Append("=");
                    sb.Append(iciciModel.Amount);
                    sb.Append("&");
                    sb.Append(nameof(iciciModel.TranRefNo));
                    sb.Append("=");
                    sb.Append(iciciModel.TranRefNo);
                    sb.Append("&");
                    sb.Append(nameof(iciciModel.PaymentRef));
                    sb.Append("=");
                    sb.Append(iciciModel.PaymentRef);
                    sb.Append("&");
                    sb.Append(nameof(iciciModel.RemName));
                    sb.Append("=");
                    sb.Append(iciciModel.RemName);
                    sb.Append("&");
                    sb.Append(nameof(iciciModel.RemMobile));
                    sb.Append("=");
                    sb.Append(iciciModel.RemMobile);
                    sb.Append("&");
                    sb.Append(nameof(iciciModel.RetailerCode));
                    sb.Append("=");
                    sb.Append(iciciModel.RetailerCode);
                    sb.Append("&");
                    sb.Append(nameof(iciciModel.TransactionDate));
                    sb.Append("=");
                    sb.Append(iciciModel.TransactionDate);
                    sb.Append("&");
                    sb.Append(nameof(iciciModel.PassCode));
                    sb.Append("=");
                    sb.Append(iciciModel.PassCode);

                    var URL = appSetting.BaseURL + "imps-web-bc/api/transaction/bc/" + appSetting.BC + "/p2a";
                    request = URL + "?" + sb.ToString();
                    response = await AppWebRequest.O.CallUsingHttpWebRequest_POSTAsync(URL, sb.ToString()).ConfigureAwait(false);
                    var _apiRes = new ICICIImpsResponse();
                    _apiRes = XMLHelper.O.DesrializeToObject(_apiRes, response, "ImpsResponse", true);
                    if (Validate.O.IsNumeric(_apiRes.ActCode ?? string.Empty))
                    {
                        var ActCode = Convert.ToInt32(_apiRes.ActCode);
                        if (ActCode == 0)
                        {
                            res.Statuscode = RechargeRespType.SUCCESS;
                            res.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                            res.ErrorCode = ErrorCodes.Transaction_Successful;
                            res.LiveID = _apiRes.BankRRN ?? string.Empty;
                            res.VendorID = _apiRes.TranRefNo ?? string.Empty;
                        }
                        else if ((ActCode >= 1 && ActCode <= 10) || (ActCode >= 12 && ActCode <= 15) || (ActCode >= 18 && ActCode <= 21))
                        {
                            res.Statuscode = RechargeRespType.FAILED;
                            res.Msg = _apiRes.Response;
                            res.ErrorCode = ErrorCodes.Unknown_Error;
                            res.LiveID = _apiRes.Response;
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

        public async Task<DMRTransactionResponse> TransactionQuery(DMTReq _req, ReqSendMoney sendMoney, DMRTransactionResponse res)
        {
            res.Statuscode = RechargeRespType.PENDING;
            res.Msg = nameof(ErrorCodes.Request_Accpeted).Replace("_", " ");
            res.ErrorCode = ErrorCodes.Request_Accpeted;
            res.LiveID = res.Msg;

            if (appSetting != null)
            {
                string response = "", request = "";
                try
                {
                    SenderRequest senderRequest = (SenderRequest)new ProcGetSender(_dal).Call(_req.SenderNO);
                    var iciciModel = new ICICIReqModel
                    {
                        Amount = sendMoney.Amount.ToString(),
                        BeneAccNo = sendMoney.AccountNo,
                        BeneIFSC = sendMoney.IFSC,
                        RemName = senderRequest.Name,
                        RemMobile = _req.SenderNO,
                        TranRefNo = res.TID.ToString(),
                        RetailerCode = "rcode",
                        PaymentRef = "FTTransferP2A" + res.TID,
                        TransactionDate = DateTime.Now.ToString("yyyyMMddhhmmss"),
                        PassCode = appSetting.PassCode
                    };
                    var sb = new StringBuilder();

                    sb.Append(nameof(iciciModel.TranRefNo));
                    sb.Append("=");
                    sb.Append(iciciModel.TranRefNo);
                    sb.Append("&");
                    sb.Append(nameof(iciciModel.PassCode));
                    sb.Append("=");
                    sb.Append(iciciModel.PassCode);

                    var URL = appSetting.BaseURL + "imps-web-bc/api/transaction/bc/" + appSetting.BC + "/query";
                    request = URL + "?" + sb.ToString();
                    response = await AppWebRequest.O.CallUsingHttpWebRequest_POSTAsync(URL, sb.ToString(), "text/plain").ConfigureAwait(false);
                    var _apiRes = new ICICIImpsResponse();
                    _apiRes = XMLHelper.O.DesrializeToObject(_apiRes, response, "ImpsResponse", true);
                    if (Validate.O.IsNumeric(_apiRes.ActCode ?? string.Empty))
                    {
                        var ActCode = Convert.ToInt32(_apiRes.ActCode);
                        if (ActCode == 0)
                        {
                            res.Statuscode = RechargeRespType.SUCCESS;
                            res.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                            res.ErrorCode = ErrorCodes.Transaction_Successful;
                            res.LiveID = _apiRes.BankRRN ?? string.Empty;
                            res.VendorID = _apiRes.TranRefNo ?? string.Empty;
                        }
                        else if ((ActCode >= 1 && ActCode <= 10) || (ActCode >= 12 && ActCode <= 15) || (ActCode >= 18 && ActCode <= 21))
                        {
                            res.Statuscode = RechargeRespType.FAILED;
                            res.Msg = _apiRes.Response;
                            res.ErrorCode = ErrorCodes.Unknown_Error;
                            res.LiveID = _apiRes.Response;
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
                        FuncName = "TransactionQuery",
                        Error = ex.Message,
                        LoginTypeID = _req.LT,
                        UserId = _req.UserID
                    };
                    var _ = new ProcPageErrorLog(_dal).Call(errorLog);
                }
                var dMTReq = new DMTReqRes
                {
                    APIID = _req.ApiID,
                    Method = "TransactionQuery",
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
    }
    public partial class ICICIML : IMoneyTransferAPIML
    {
        public ICICIML(IHttpContextAccessor accessor, IHostingEnvironment env)
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
            appSetting = AppSetting();
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
                    res.Msg = ErrorCodes.DMTSNE;
                    res.IsSenderNotExists = true;
                    return res;
                }
                else if (senderRequest._VerifyStatus == ErrorCodes.Two)
                {
                    var procSender = new ProcGetSenderLimit(_dal);
                    var senderLimit = (SenderLimitModel)procSender.Call(new CommonReq
                    {
                        CommonInt = senderRequest.ID,
                        CommonInt2 = _APIID
                    }).Result;
                    res.RemainingLimit = senderLimit.SenderLimit - senderLimit.LimitUsed;
                    res.AvailbleLimit = senderLimit.SenderLimit;
                    res.Statuscode = ErrorCodes.One;
                    res.Msg = ErrorCodes.DMTSLS;
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
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "GetSender",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = request.UserID
                });
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
                    _APICode=request.APICode,
                    _BankID=request.mBeneDetail.BankID
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
            var res = new MBeneficiaryResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError,
                ErrorCode = ErrorCodes.Unknown_Error
            };
            try
            {
                var resDB = (new ProcGetBenificiary(_dal).Call(new DMTReq { SenderNO = request.SenderMobile })) as BenificiaryModel;
                res.Statuscode = resDB.Statuscode;
                res.Msg = resDB.Msg;
                if (resDB != null && resDB.Statuscode == ErrorCodes.One)
                {
                    var Beneficiaries = new List<MBeneDetail>();
                    if (resDB.benificiaries != null && resDB.benificiaries.Count > 0)
                    {
                        foreach (var r in resDB.benificiaries)
                        {
                            Beneficiaries.Add(new MBeneDetail
                            {
                                AccountNo = r._AccountNumber,
                                BankName = r._BankName,
                                IFSC = r._IFSC,
                                BeneName = r._Name,
                                MobileNo = r._BankName,
                                BeneID = r._ID.ToString()
                            });
                        }
                    }
                    res.Beneficiaries = Beneficiaries;
                }
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "GetBeneficiary",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = request.UserID
                });
            }
            return res;
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
                    res.ErrorCode = ErrorCodes.Unknown_Error;
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
            throw new NotImplementedException();
        }

        public DMRTransactionResponse VerifyAccount(MTAPIRequest request)
        {
            throw new NotImplementedException();
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
            if (appSetting != null)
            {
                string response = "";
                try
                {
                    SenderRequest senderRequest = (SenderRequest)new ProcGetSender(_dal).Call(request.SenderMobile);
                    var iciciModel = new ICICIReqModel
                    {
                        Amount = request.Amount.ToString(),
                        BeneAccNo = request.mBeneDetail.AccountNo,
                        BeneIFSC = request.mBeneDetail.IFSC,
                        RemName = senderRequest.Name,
                        RemMobile = request.SenderMobile,
                        TranRefNo = res.TID.ToString(),
                        RetailerCode = "rcode",
                        PaymentRef = "FTTransferP2A" + res.TID,
                        TransactionDate = DateTime.Now.ToString("yyyyMMddhhmmss"),
                        PassCode = appSetting.PassCode
                    };
                    var sb = new StringBuilder();
                    sb.Append(nameof(iciciModel.BeneAccNo));
                    sb.Append("=");
                    sb.Append(iciciModel.BeneAccNo);
                    sb.Append("&");
                    sb.Append(nameof(iciciModel.BeneIFSC));
                    sb.Append("=");
                    sb.Append(iciciModel.BeneIFSC);
                    sb.Append("&");
                    sb.Append(nameof(iciciModel.Amount));
                    sb.Append("=");
                    sb.Append(iciciModel.Amount);
                    sb.Append("&");
                    sb.Append(nameof(iciciModel.TranRefNo));
                    sb.Append("=");
                    sb.Append(iciciModel.TranRefNo);
                    sb.Append("&");
                    sb.Append(nameof(iciciModel.PaymentRef));
                    sb.Append("=");
                    sb.Append(iciciModel.PaymentRef);
                    sb.Append("&");
                    sb.Append(nameof(iciciModel.RemName));
                    sb.Append("=");
                    sb.Append(iciciModel.RemName);
                    sb.Append("&");
                    sb.Append(nameof(iciciModel.RemMobile));
                    sb.Append("=");
                    sb.Append(iciciModel.RemMobile);
                    sb.Append("&");
                    sb.Append(nameof(iciciModel.RetailerCode));
                    sb.Append("=");
                    sb.Append(iciciModel.RetailerCode);
                    sb.Append("&");
                    sb.Append(nameof(iciciModel.TransactionDate));
                    sb.Append("=");
                    sb.Append(iciciModel.TransactionDate);
                    sb.Append("&");
                    sb.Append(nameof(iciciModel.PassCode));
                    sb.Append("=");
                    sb.Append(iciciModel.PassCode);

                    var URL = appSetting.BaseURL + "imps-web-bc/api/transaction/bc/" + appSetting.BC + "/p2a";
                    res.Request = URL + "?" + sb.ToString();
                    response = AppWebRequest.O.CallUsingHttpWebRequest_POSTAsync(URL, sb.ToString()).Result;
                    var _apiRes = new ICICIImpsResponse();
                    _apiRes = XMLHelper.O.DesrializeToObject(_apiRes, response, "ImpsResponse", true);
                    if (Validate.O.IsNumeric(_apiRes.ActCode ?? string.Empty))
                    {
                        var ActCode = Convert.ToInt32(_apiRes.ActCode);
                        if (ActCode == 0)
                        {
                            res.Statuscode = RechargeRespType.SUCCESS;
                            res.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                            res.ErrorCode = ErrorCodes.Transaction_Successful;
                            res.LiveID = _apiRes.BankRRN ?? string.Empty;
                            res.VendorID = _apiRes.TranRefNo ?? string.Empty;
                        }
                        else if ((ActCode >= 1 && ActCode <= 10) || (ActCode >= 12 && ActCode <= 15) || (ActCode >= 18 && ActCode <= 21))
                        {
                            res.Statuscode = RechargeRespType.FAILED;
                            res.Msg = _apiRes.Response;
                            res.ErrorCode = ErrorCodes.Unknown_Error;
                            res.LiveID = _apiRes.Response;
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
