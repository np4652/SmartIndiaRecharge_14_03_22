using Fintech.AppCode.Configuration;
using Fintech.AppCode.DB;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using RoundpayFinTech.AppCode.DL;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System.IO;
using Fintech.AppCode.HelperClass;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.StaticModel.MoneyTransfer;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fintech.AppCode.WebRequest;
using RoundpayFinTech.AppCode.ThirdParty.AirtelPayment;
using System.Text;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model.MoneyTransfer;

namespace RoundpayFinTech.AppCode.MiddleLayer.Dmt_Api
{
    public partial class AirtelBankML
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        private readonly IConfiguration Configuration;
        private readonly IDAL _dal;
        private readonly IConnectionConfiguration _c;
        private readonly DMTSetting dMTSetting;

        private const char hashChar = '#';
        public AirtelBankML(IHttpContextAccessor accessor, IHostingEnvironment env)
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
            dMTSetting = GetApiDetail();

        }
        private DMTSetting GetApiDetail()
        {
            DMTSetting dMTSetting = new DMTSetting();
            try
            {
                dMTSetting.BaseURL = Configuration["DMR:" + APICode.AIRTELBANK + ":BaseURL"];
                dMTSetting.LimitURL = Configuration["DMR:" + APICode.AIRTELBANK + ":LimitURL"];
                dMTSetting.TransactionURL = Configuration["DMR:" + APICode.AIRTELBANK + ":TransactionURL"];
                dMTSetting.partnerId = Configuration["DMR:" + APICode.AIRTELBANK + ":PartnerID"];
                dMTSetting.appVersion = Configuration["DMR:" + APICode.AIRTELBANK + ":appVersion"];
                dMTSetting.salt = Configuration["DMR:" + APICode.AIRTELBANK + ":salt"];
            }
            catch { }
            return dMTSetting;
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
                SenderRequest senderRequest = (SenderRequest)new ProcGetSender(_dal).Call(_req.SenderNO);
                if (string.IsNullOrWhiteSpace(senderRequest.Name))
                {
                    res.Statuscode = ErrorCodes.One;
                    res.Msg = ErrorCodes.DMTSNE;
                    res.CommonInt = ErrorCodes.One;
                    return res;
                }
                else if (senderRequest._VerifyStatus == ErrorCodes.Two)
                {
                    res = await Get_Sender_Limit(_req).ConfigureAwait(false);
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
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "GetBeneficiary",
                    Error = ex.Message,
                    LoginTypeID = _req.LT,
                    UserId = _req.UserID
                });
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

        private async Task<ResponseStatus> Get_Sender_Limit(DMTReq _req)
        {

            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            string fesessionid = ((int)new ProcGetUniqueIDIProcedure(_dal).Call()).ToString();
            fesessionid = ConverterHelper.O.Generate10To20UniqueID(string.Empty, fesessionid);
            var sb = new StringBuilder();
            sb.Append(dMTSetting.partnerId);
            sb.Append(hashChar);
            sb.Append(_req.SenderNO);
            sb.Append(hashChar);
            sb.Append(fesessionid);
            sb.Append(hashChar);
            sb.Append(dMTSetting.salt);

            var airtelBankRequest = new AirtelSenderLimitRequest();
            if (dMTSetting != null)
            {
                airtelBankRequest.appVersion = dMTSetting.appVersion;
                airtelBankRequest.ver = dMTSetting.appVersion;
                airtelBankRequest.caf = "C2A";
                airtelBankRequest.channel = "EXTP";
                airtelBankRequest.customerId = _req.SenderNO;
                airtelBankRequest.feSessionId = fesessionid;
                airtelBankRequest.hash = HashEncryption.O.SHA512HashUTF8(sb.ToString()).ToLower(); ;
                airtelBankRequest.partnerId = dMTSetting.partnerId;
            }
            string response = "", request = "";
            try
            {
                var URL = dMTSetting.LimitURL; //+ "api/v1/sender/limit";

                request = URL + "?" + JsonConvert.SerializeObject(airtelBankRequest);
                response = await AppWebRequest.O.PostJsonDataUsingHWRTLS(URL, airtelBankRequest);
                var _apiRes = JsonConvert.DeserializeObject<AirtelBankResponse>(response);
                if (_apiRes.code == "0")
                {
                    res.Statuscode = ErrorCodes.One;
                    res.Msg = ErrorCodes.DMTSLS;
                    _apiRes.mcmv = (_apiRes.mcmv ?? "") == "" ? "0" : _apiRes.mcmv;
                    _apiRes.mcv = (_apiRes.mcv ?? "") == "" ? "0" : _apiRes.mcv;
                    res.CommonStr = (Convert.ToInt32(_apiRes.mcmv) - Convert.ToInt32(_apiRes.mcv)).ToString();
                    res.CommonStr2 = _req.SenderNO;
                }
                else
                {
                    res.Msg = _apiRes.messageText;
                }
            }
            catch (Exception ex)
            {
                response = ex.Message + "|" + response;
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Get_Sender_Limit",
                    Error = ex.Message,
                    LoginTypeID = _req.LT,
                    UserId = _req.UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            var dMTReq = new DMTReqRes
            {
                APIID = _req.ApiID,
                Method = "Get_Sender_Limit",
                RequestModeID = _req.RequestMode,
                Request = request,
                Response = response,
                SenderNo = _req.SenderNO,
                UserID = _req.UserID,
                TID = _req.TID
            };
            new ProcUpdateLogDMRReqResp(_dal).Call(dMTReq);
            return res;
        }

        public async Task<DMRTransactionResponse> SendMoney(DMTReq _req, ReqSendMoney sendMoney, DMRTransactionResponse res)
        {
            res.Statuscode = RechargeRespType.PENDING;
            res.Msg = nameof(ErrorCodes.Request_Accpeted).Replace("_", " ");
            res.ErrorCode = ErrorCodes.Request_Accpeted;
            res.LiveID = res.Msg;
            string fesessionid = ((int)new ProcGetUniqueIDIProcedure(_dal).Call()).ToString();
            fesessionid = ConverterHelper.O.Generate10To20UniqueID(fesessionid, res.TID.ToString());
            //partnerId#channel#beneAccNo#beneMobNo#amount#salt
            var sb = new StringBuilder();

            sb.Append(dMTSetting.partnerId);
            sb.Append(hashChar);
            sb.Append("EXTP");
            sb.Append(hashChar);
            sb.Append(sendMoney.AccountNo);
            sb.Append(hashChar);
            sb.Append(_req.SenderNO);
            sb.Append(hashChar);
            sb.Append(sendMoney.Amount);
            sb.Append(hashChar);
            sb.Append(dMTSetting.salt);

            if (dMTSetting != null)
            {
                string response = "", request = "";
                try
                {
                    SenderRequest senderRequest = (SenderRequest)new ProcGetSender(_dal).Call(_req.SenderNO);
                    var airtelBankRequest = new AirtelBankRequest
                    {
                        agentId = _req.UserMobileNo,
                        amount = sendMoney.Amount.ToString(),
                        bankName = sendMoney.Bank,
                        beneAccNo = sendMoney.AccountNo,
                        beneMobNo = sendMoney.MobileNo,
                        channel = "EXTP",
                        custAddress = senderRequest.Address,
                        custDob = Convert.ToDateTime(senderRequest.Dob ?? "").ToString("dd-MMM-yy"),//dd-MMM-yy
                        custFirstName = senderRequest.Name.Split(' ')[0].Length < 5 ? "Shri" + senderRequest.Name.Split(' ')[0] : senderRequest.Name.Split(' ')[0],
                        custId = _req.SenderNO,
                        custLastName = string.IsNullOrEmpty(senderRequest.LastName) ? senderRequest.Name.Split(' ')[0] : senderRequest.LastName.Split(' ')[0],
                        custPincode = senderRequest.Pincode,
                        externalRefNo = fesessionid,
                        hash = HashEncryption.O.SHA512HashUTF8(sb.ToString()).ToLower(),
                        ifsc = sendMoney.IFSC,
                        partnerId = dMTSetting.partnerId,
                        stateCode = _req.StateID.ToString()
                    };
                    var headers = new Dictionary<string, string>
                    {
                         { ContentType.Self,ContentType.application_json},
                        { "feSessionId",fesessionid}
                    };
                    var URL = dMTSetting.TransactionURL + "imps";
                    string postData = JsonConvert.SerializeObject(airtelBankRequest);
                    request = URL + JsonConvert.SerializeObject(headers) + "?" + postData;
                    response = await AppWebRequest.O.HWRPostAsync(URL, postData, headers).ConfigureAwait(false);
                    var _apiRes = JsonConvert.DeserializeObject<AirtelBankResponse>(response);
                    if (_apiRes != null)
                    {
                        if (_apiRes.meta != null && _apiRes.meta.code != null)
                        {
                            if (_apiRes.meta.code.Equals("0"))
                            {
                                res.Statuscode = RechargeRespType.SUCCESS;
                                res.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                                res.ErrorCode = ErrorCodes.Transaction_Successful;
                                res.LiveID = _apiRes.data.rrn ?? string.Empty;
                                res.VendorID = _apiRes.data.tranId ?? string.Empty;
                            }
                            else if (_apiRes.meta.code.In("934210", "126", "124", "101"))
                            {
                                res.Statuscode = RechargeRespType.PENDING;
                                res.Msg = nameof(ErrorCodes.Request_Accpeted).Replace("_", " ");
                                res.ErrorCode = ErrorCodes.Request_Accpeted;
                                res.LiveID = _apiRes.meta.description;
                            }
                            else if (_apiRes.meta.code.In("912", "9001", "302", "911", "801", "9006", "934210", "333998", "999001", "AS_111", "100", "012", "011", "010", "009", "008", "007", "006", "005", "004", "003", "002", "22", "801", "99", "1616", "M7", "10", "302", "1515", "4", "94", "92", "14", "13", "51", "3", "54", "PM0684", "12", "93097", "1", "9001", "1075", "M4", "1077", "1206", "M0", "333998", "PM0640", "999001", "PM0405", "3403", "12", "M2", "96", "BL101", "608", "1076", "20", "M3", "M5", "101", "52", "M1", "8"))
                            {
                                res.Statuscode = RechargeRespType.FAILED;
                                res.Msg = nameof(ErrorCodes.Unknown_Error).Replace("_", " ");
                                res.ErrorCode = ErrorCodes.Unknown_Error;
                                res.LiveID = _apiRes.meta.description;
                            }
                            else if (_apiRes.meta.description.Contains("insufficient"))
                            {
                                res.Msg = nameof(ErrorCodes.ServiceDown);
                                res.ErrorCode = ErrorCodes.ServiceDown;
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
    }
    public partial class AirtelBankML : IMoneyTransferAPIML
    {
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
        private async Task<MSenderLoginResponse> Get_Sender_Limit(MTAPIRequest request)
        {

            var res = new MSenderLoginResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError,
                ErrorCode = ErrorCodes.Unknown_Error
            };
            string fesessionid = ((int)new ProcGetUniqueIDIProcedure(_dal).Call()).ToString();
            fesessionid = ConverterHelper.O.Generate10To20UniqueID(string.Empty, fesessionid);
            var sb = new StringBuilder();
            sb.Append(dMTSetting.partnerId);
            sb.Append(hashChar);
            sb.Append(request.SenderMobile);
            sb.Append(hashChar);
            sb.Append(fesessionid);
            sb.Append(hashChar);
            sb.Append(dMTSetting.salt);

            var airtelBankRequest = new AirtelSenderLimitRequest();
            if (dMTSetting != null)
            {
                airtelBankRequest.appVersion = dMTSetting.appVersion;
                airtelBankRequest.ver = dMTSetting.appVersion;
                airtelBankRequest.caf = "C2A";
                airtelBankRequest.channel = "EXTP";
                airtelBankRequest.customerId = request.SenderMobile;
                airtelBankRequest.feSessionId = fesessionid;
                airtelBankRequest.hash = HashEncryption.O.SHA512HashUTF8(sb.ToString()).ToLower(); ;
                airtelBankRequest.partnerId = dMTSetting.partnerId;
            }
            var _URL = dMTSetting.LimitURL; //+ "api/v1/sender/limit";
            string response = "";
            try
            {
                response = await AppWebRequest.O.PostJsonDataUsingHWRTLS(_URL, airtelBankRequest).ConfigureAwait(false);
                var _apiRes = JsonConvert.DeserializeObject<AirtelBankResponse>(response);
                if (_apiRes.code == "0")
                {
                    res.Statuscode = ErrorCodes.One;
                    res.Msg = ErrorCodes.DMTSLS;
                    _apiRes.mcmv = (_apiRes.mcmv ?? "") == "" ? "0" : _apiRes.mcmv;
                    _apiRes.mcv = (_apiRes.mcv ?? "") == "" ? "0" : _apiRes.mcv;
                    res.RemainingLimit = Convert.ToInt32(_apiRes.mcmv) - Convert.ToInt32(_apiRes.mcv);
                    res.AvailbleLimit = Convert.ToInt32(_apiRes.mcmv);
                }
                else
                {
                    res.Msg = _apiRes.messageText;
                }
            }
            catch (Exception ex)
            {
                response = ex.Message + "|" + response;
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Get_Sender_Limit",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = request.UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            var dMTReq = new DMTReqRes
            {
                APIID = request.APIID,
                Method = "Get_Sender_Limit",
                RequestModeID = request.RequestMode,
                Request = _URL + "?" + JsonConvert.SerializeObject(airtelBankRequest),
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
            throw new NotImplementedException();
        }
        public DMRTransactionResponse VerifyAccount(MTAPIRequest request)
        {
            var res = new DMRTransactionResponse
            {
                Statuscode = RechargeRespType.PENDING,
                Msg = nameof(ErrorCodes.Request_Accpeted).Replace("_", " "),
                ErrorCode = ErrorCodes.Request_Accpeted,
                VendorID = string.Empty,
                LiveID = string.Empty
            };
            string fesessionid = ((int)new ProcGetUniqueIDIProcedure(_dal).Call()).ToString();
            fesessionid = ConverterHelper.O.Generate10To20UniqueID(fesessionid, res.TID.ToString());
            //partnerId#channel#beneAccNo#beneMobNo#amount#salt
            var sb = new StringBuilder();

            sb.Append(dMTSetting.partnerId);
            sb.Append(hashChar);
            sb.Append("EXTP");
            sb.Append(hashChar);
            sb.Append(request.mBeneDetail.AccountNo);
            sb.Append(hashChar);
            sb.Append(request.SenderMobile);
            sb.Append(hashChar);
            sb.Append(1);
            sb.Append(hashChar);
            sb.Append(dMTSetting.salt);

            if (dMTSetting != null)
            {
                string response = string.Empty;
                try
                {
                    var senderRequest = (SenderRequest)new ProcGetSender(_dal).Call(request.SenderMobile);
                    var airtelBankRequest = new AirtelBankRequest
                    {
                        agentId = request.UserMobile,
                        amount = "1",
                        bankName = request.mBeneDetail.BankName,
                        beneAccNo = request.mBeneDetail.AccountNo,
                        beneMobNo = request.mBeneDetail.MobileNo,
                        channel = "EXTP",
                        custAddress = senderRequest.Address,
                        custDob = Convert.ToDateTime(senderRequest.Dob ?? "").ToString("dd-MMM-yy"),//dd-MMM-yy
                        custFirstName = senderRequest.Name.Split(' ')[0].Length < 5 ? "Shri" + senderRequest.Name.Split(' ')[0] : senderRequest.Name.Split(' ')[0],
                        custId = request.SenderMobile,
                        custLastName = string.IsNullOrEmpty(senderRequest.LastName) ? senderRequest.Name.Split(' ')[0] : senderRequest.LastName.Split(' ')[0],
                        custPincode = senderRequest.Pincode,
                        externalRefNo = fesessionid,
                        hash = HashEncryption.O.SHA512HashUTF8(sb.ToString()).ToLower(),
                        ifsc = request.mBeneDetail.IFSC,
                        partnerId = dMTSetting.partnerId,
                        stateCode = request.StateID.ToString()
                    };
                    var headers = new Dictionary<string, string>
                    {
                         { ContentType.Self,ContentType.application_json},
                        { "feSessionId",fesessionid}
                    };
                    var _URL = dMTSetting.TransactionURL + "imps";
                    string postData = JsonConvert.SerializeObject(airtelBankRequest);
                    res.Request = _URL + JsonConvert.SerializeObject(headers) + "?" + postData;
                    response = AppWebRequest.O.HWRPostAsync(_URL, postData, headers).Result;
                    var _apiRes = JsonConvert.DeserializeObject<AirtelBankResponse>(response);
                    if (_apiRes != null)
                    {
                        if (_apiRes.meta != null && _apiRes.meta.code != null)
                        {
                            if (_apiRes.meta.code.Equals("0"))
                            {
                                res.Statuscode = RechargeRespType.SUCCESS;
                                res.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                                res.ErrorCode = ErrorCodes.Transaction_Successful;
                                res.LiveID = _apiRes.data.rrn ?? string.Empty;
                                res.VendorID = _apiRes.data.tranId ?? string.Empty;
                            }
                            else if (_apiRes.meta.code.In("934210", "126", "124", "101"))
                            {
                                res.Statuscode = RechargeRespType.PENDING;
                                res.Msg = nameof(ErrorCodes.Request_Accpeted).Replace("_", " ");
                                res.ErrorCode = ErrorCodes.Request_Accpeted;
                                res.LiveID = _apiRes.meta.description;
                            }
                            else if (_apiRes.meta.code.In("912", "9001", "302", "911", "801", "9006", "934210", "333998", "999001", "AS_111", "100", "012", "011", "010", "009", "008", "007", "006", "005", "004", "003", "002", "22", "801", "99", "1616", "M7", "10", "302", "1515", "4", "94", "92", "14", "13", "51", "3", "54", "PM0684", "12", "93097", "1", "9001", "1075", "M4", "1077", "1206", "M0", "333998", "PM0640", "999001", "PM0405", "3403", "12", "M2", "96", "BL101", "608", "1076", "20", "M3", "M5", "101", "52", "M1", "8", "003002"))
                            {
                                res.Statuscode = RechargeRespType.FAILED;
                                res.Msg = _apiRes.meta.description?? nameof(ErrorCodes.Unknown_Error).Replace("_", " ");
                                res.ErrorCode = ErrorCodes.Unknown_Error;
                                res.LiveID = _apiRes.meta.description;
                            }
                            else if (_apiRes.meta.description.ToLower().Contains("insufficient"))
                            {
                                res.Statuscode = RechargeRespType.FAILED;
                                res.Msg = nameof(ErrorCodes.ServiceDown);
                                res.ErrorCode = ErrorCodes.ServiceDown;
                                res.LiveID = res.Msg;
                            }
                        }
                    }
                    res.Response = response;
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
            string fesessionid = ((int)new ProcGetUniqueIDIProcedure(_dal).Call()).ToString();
            fesessionid = ConverterHelper.O.Generate10To20UniqueID(fesessionid, res.TID.ToString());
            //partnerId#channel#beneAccNo#beneMobNo#amount#salt
            var sb = new StringBuilder();

            sb.Append(dMTSetting.partnerId);
            sb.Append(hashChar);
            sb.Append("EXTP");
            sb.Append(hashChar);
            sb.Append(request.mBeneDetail.AccountNo);
            sb.Append(hashChar);
            sb.Append(request.SenderMobile);
            sb.Append(hashChar);
            sb.Append(request.Amount);
            sb.Append(hashChar);
            sb.Append(dMTSetting.salt);

            if (dMTSetting != null)
            {
                string response = "";
                try
                {
                    SenderRequest senderRequest = (SenderRequest)new ProcGetSender(_dal).Call(request.SenderMobile);
                    var airtelBankRequest = new AirtelBankRequest
                    {
                        agentId = request.UserMobile,
                        amount = request.Amount.ToString(),
                        bankName = request.mBeneDetail.BankName,
                        beneAccNo = request.mBeneDetail.AccountNo,
                        beneMobNo = request.mBeneDetail.MobileNo,
                        channel = "EXTP",
                        custAddress = senderRequest.Address,
                        custDob = Convert.ToDateTime(senderRequest.Dob ?? "").ToString("dd-MMM-yy"),//dd-MMM-yy
                        custFirstName = senderRequest.Name.Split(' ')[0].Length < 5 ? "Shri" + senderRequest.Name.Split(' ')[0] : senderRequest.Name.Split(' ')[0],
                        custId = request.SenderMobile,
                        custLastName = string.IsNullOrEmpty(senderRequest.LastName) ? senderRequest.Name.Split(' ')[0] : senderRequest.LastName.Split(' ')[0],
                        custPincode = senderRequest.Pincode,
                        externalRefNo = fesessionid,
                        hash = HashEncryption.O.SHA512HashUTF8(sb.ToString()).ToLower(),
                        ifsc = request.mBeneDetail.IFSC,
                        partnerId = dMTSetting.partnerId,
                        stateCode = request.StateID.ToString()
                    };
                    var headers = new Dictionary<string, string>
                    {
                         { ContentType.Self,ContentType.application_json},
                        { "feSessionId",fesessionid}
                    };
                    var _URL = dMTSetting.TransactionURL + "imps";
                    string postData = JsonConvert.SerializeObject(airtelBankRequest);
                    res.Request = _URL + JsonConvert.SerializeObject(headers) + "?" + postData;
                    response = AppWebRequest.O.HWRPostAsync(_URL, postData, headers).Result;
                    var _apiRes = JsonConvert.DeserializeObject<AirtelBankResponse>(response);
                    if (_apiRes != null)
                    {
                        if (_apiRes.meta != null && _apiRes.meta.code != null)
                        {
                            if (_apiRes.meta.code.Equals("0"))
                            {
                                res.Statuscode = RechargeRespType.SUCCESS;
                                res.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                                res.ErrorCode = ErrorCodes.Transaction_Successful;
                                res.LiveID = _apiRes.data.rrn ?? string.Empty;
                                res.VendorID = _apiRes.data.tranId ?? string.Empty;
                            }
                            else if (_apiRes.meta.code.In("934210", "126", "124", "101"))
                            {
                                res.Statuscode = RechargeRespType.PENDING;
                                res.Msg = nameof(ErrorCodes.Request_Accpeted).Replace("_", " ");
                                res.ErrorCode = ErrorCodes.Request_Accpeted;
                                res.LiveID = _apiRes.meta.description;
                            }
                            else if (_apiRes.meta.code.In("912", "9001", "302", "911", "801", "9006", "934210", "333998", "999001", "AS_111", "100", "012", "011", "010", "009", "008", "007", "006", "005", "004", "003", "002", "22", "801", "99", "1616", "M7", "10", "302", "1515", "4", "94", "92", "14", "13", "51", "3", "54", "PM0684", "12", "93097", "1", "9001", "1075", "M4", "1077", "1206", "M0", "333998", "PM0640", "999001", "PM0405", "3403", "12", "M2", "96", "BL101", "608", "1076", "20", "M3", "M5", "101", "52", "M1", "8","400", "003002"))
                            {
                                res.Statuscode = RechargeRespType.FAILED;
                                res.Msg = _apiRes.meta.description ?? nameof(ErrorCodes.Unknown_Error).Replace("_", " ");
                                res.ErrorCode = ErrorCodes.Unknown_Error;
                                res.LiveID = _apiRes.meta.description;
                            }
                            else if (_apiRes.meta.description.ToLower().Contains("insufficient"))
                            {
                                res.Statuscode = RechargeRespType.FAILED;
                                res.Msg = nameof(ErrorCodes.ServiceDown);
                                res.ErrorCode = ErrorCodes.ServiceDown;
                                res.LiveID = res.Msg;
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
