using Fintech.AppCode.Configuration;
using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using Fintech.AppCode.WebRequest;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RoundpayFinTech.AppCode;
using RoundpayFinTech.AppCode.DL;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.MiddleLayer;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Fintech.AppCode
{
    public sealed class RailwayML : IRailwayML
    {
        #region Global Varibale Declaration
        private readonly IConfiguration Configuration;
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        private readonly ISession _session;
        private readonly IDAL _dal;
        private readonly IConnectionConfiguration _c;
        private readonly IRequestInfo _rinfo;
        private readonly IRKeysSettingsModel _validKeys;
        private readonly IRKeysSettingsModel _validKeysLive;
        #endregion
        public RailwayML(IHttpContextAccessor accessor, IHostingEnvironment env, bool IsInSession = true)
        {
            _accessor = accessor;
            _env = env;
            _c = new ConnectionConfiguration(_accessor, _env);
            _dal = new DAL(_c.GetConnectionString());
            _rinfo = new RequestInfo(_accessor, _env);
            if (IsInSession)
            {
                _session = _accessor.HttpContext.Session;
                bool IsProd = _env.IsProduction();
                var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory());
                builder.AddJsonFile((IsProd ? "appsettings.json" : "appsettings.Development.json"));
                builder.AddEnvironmentVariables();
                Configuration = builder.Build();
            }
            _validKeys = AppSetting();
            _validKeysLive = AppSettingLive();
        }

        public ResponseStatus MatchRedirectedDomain()
        {
            var cInfo = _rinfo.GetCurrentReqInfo();
            ILoginML loginML = new LoginML(_accessor, _env, false);
            var winfo = loginML.GetWebsiteInfo(1);

            return new ResponseStatus
            {
                CommonBool = (winfo.RedirectDomain ?? string.Empty).Replace("http://", "").Replace("https://", "") == cInfo.Host,
                CommonStr = (winfo.RedirectDomain ?? string.Empty)
            };
        }
        #region Actual Implementaion
        public IRViewModel Decode(string url, bool IsLive = false)
        {
            IRViewModel resp = new IRViewModel
            {
                StatusCode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            try
            {
                var decodedUrl = (IRRequestModel)DecodeUrl(url, false, IsLive);
                if (decodedUrl != null)
                {
                    var model = new IRSaveModel
                    {
                        EncodedRequest = url,
                        merchantCode = decodedUrl.merchantCode,
                        reservationId = decodedUrl.reservationId,
                        txnAmount = decodedUrl.txnAmount,
                        currencyType = decodedUrl.currencyType,
                        appCode = decodedUrl.appCode,
                        pymtMode = decodedUrl.pymtMode,
                        txnDate = decodedUrl.txnDate,
                        securityId = decodedUrl.securityId,
                        RU = decodedUrl.RU,
                        checkSum = decodedUrl.checkSum,
                        TranStatus = 0,
                        IsDoubleVerification = false
                    };
                    var res = SaveHandledRequest(model);
                    if (res.StatusCode == ErrorCodes.One)
                    {
                        resp.StatusCode = ErrorCodes.One;
                        resp.Msg = res.Msg;
                        resp.Id = res.IRSaveID;
                        resp.IRSaveID = res.IRSaveID;
                        resp.reservationId = res.reservationId;
                        resp.txnAmount = res.txnAmount;
                        resp.MerchantCode = IsLive ? _validKeysLive.merchantCode : _validKeys.merchantCode;
                    }
                    else
                    {
                        res.StatusCode = ErrorCodes.Minus1;
                        TransactionReturnHit(res);
                    }
                }
                else
                {
                    LogRailwayReqResp(new LogRailwayReqRespModel()
                    {
                        MethodName = "Decode_Null",
                        Request = url,
                        Response = JsonConvert.SerializeObject(resp) //+ ":hitres:" + JsonConvert.SerializeObject(hitRes)
                    });
                }
            }
            catch (Exception ex)
            {
                LogRailwayReqResp(new LogRailwayReqRespModel()
                {
                    MethodName = "Ex_Decode",
                    Request = url,
                    Response = JsonConvert.SerializeObject(resp) //+ ":hitres:" + JsonConvert.SerializeObject(hitRes)
                });
            }

            return resp;
        }
        public IRViewModel ValidateIRLogin(IRViewModel req)
        {
            IRViewModel res = new IRViewModel
            {
                StatusCode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            var valRes = ValidateRailUser(req.LoginId, req.mobile);
            res.LoginId = req.LoginId;
            res.mobile = req.mobile;
            res.MerchantCode = _validKeys.merchantCode;
            res.reservationId = req.reservationId;
            res.txnAmount = req.txnAmount;
            res.IRSaveID = req.IRSaveID;
            res.StatusCode = valRes.Statuscode;
            res.Msg = valRes.Msg;
            return res;
        }
        public RailValidateResponse ReGenerateOTP()
        {
            var res = new RailValidateResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            var otpSession = _session.GetObjectFromJson<RailOTPSession>(SessionKeys.RAILOTP);
            if (otpSession.AtTime != null)
            {
                TimeSpan ts = DateTime.Now - otpSession.AtTime;
                if (ts.TotalMinutes > 0)
                {
                    res = ValidateRailUser(otpSession.RailID, otpSession.Mobile);
                    return res;
                }
            }
            if (otpSession.IsOutsider)
            {
                var PHitURL = MakeURLForOTP(otpSession.OTPURL, otpSession.RailID, otpSession.RefID);
                var CBResp = string.Empty;
                try
                {
                    CBResp = AppWebRequest.O.CallUsingHttpWebRequest_GET(PHitURL);
                }
                catch (Exception ex)
                {
                    CBResp = "ExceptionRP:" + ex.Message;
                }
                if (!string.IsNullOrEmpty(CBResp) && !(CBResp ?? string.Empty).StartsWith("ExceptionRP:"))
                {
                    var PartnerResp = JsonConvert.DeserializeObject<RailValidateResponse>(CBResp);
                    res.Statuscode = PartnerResp.Statuscode;
                    if (PartnerResp.Statuscode == ErrorCodes.Minus1)
                    {
                        res.Msg = string.Format("{0}{1}", "(AP)", PartnerResp.Msg);
                    }
                    else
                    {
                        res.Msg = nameof(DMTErrorCodes.OTP_for_verification_has_been_sent_successfully).Replace("_", " ");
                        _session.SetObjectAsJson(SessionKeys.RAILOTP, new RailOTPSession
                        {
                            RefID = PartnerResp.RefID,
                            IsOutsider = true,
                            AtTime = otpSession.AtTime,
                            Mobile = otpSession.Mobile,
                            OTPURL = otpSession.OTPURL,
                            MatchOTPURL = otpSession.MatchOTPURL,
                            RailID = otpSession.RailID,
                            OutletUserID = otpSession.OutletUserID,
                            EmailID = otpSession.EmailID
                        });
                    }
                }

                if (!string.IsNullOrEmpty(CBResp))
                {
                    try
                    {
                        IProcedureAsync _procHit = new ProcUpdateAPIURLHitting(_dal);
                        var _req = new APIURLHitting
                        {
                            UserID = otpSession.OutletUserID,
                            TransactionID = string.Empty,
                            URL = PHitURL,
                            Response = CBResp
                        };
                        var _ = _procHit.Call(_req).Result;
                    }
                    catch (Exception ex)
                    {
                        var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                        {
                            ClassName = GetType().Name,
                            FuncName = "ReGenerateOTP URLHitting",
                            Error = ex.Message + "||" + CBResp,
                            LoginTypeID = 1,
                            UserId = 1
                        });
                    }
                }
            }
            else
            {
                OTPForRail(otpSession.OTP, otpSession.Mobile, 1, otpSession.OutletUserID, otpSession.EmailID);
                res.Msg = nameof(DMTErrorCodes.OTP_for_verification_has_been_sent_successfully).Replace("_", " ");
                _session.SetObjectAsJson(SessionKeys.RAILOTP, new RailOTPSession
                {
                    OTP = otpSession.OTP,
                    AtTime = otpSession.AtTime,
                    OutletUserID = otpSession.OutletUserID,
                    Mobile = otpSession.Mobile,
                    RailID = otpSession.RailID,
                    EmailID = otpSession.EmailID
                });
            }
            return res;
        }
        public IRViewModel IRProcessRequest(IRViewModel req)
        {
            IRViewModel res = new IRViewModel
            {
                StatusCode = ErrorCodes.Minus1,
                Msg = ErrorCodes.FAILED
            };
            var procResp = MatchRailOTP(req.IRSaveID, req.otp);
            res.StatusCode = procResp.Statuscode;
            res.Msg = procResp.Msg;

            return res;
        }
        public IRViewModel DoubleVerificationDecode(string url, bool IsLive = false)
        {
            var returnRes = new IRViewModel
            {
                StatusCode = ErrorCodes.Minus1,
                Msg = ErrorCodes.FAILED
            };
            var res = new IRSaveModel
            {
                StatusCode = ErrorCodes.Minus1,
                Msg = ErrorCodes.FAILED
            };
            var decodedUrl = (DoubleVerificationRequest)DecodeUrl(url, true, IsLive);
            try
            {
                if (decodedUrl != null)
                {
                    //returnRes.StatusCode = decodedUrl.StatusCode;
                    returnRes.Msg = decodedUrl.Msg;
                    returnRes.MerchantCode = decodedUrl.merchantCode;
                    returnRes.reservationId = decodedUrl.reservationId;
                    returnRes.txnAmount = Convert.ToDecimal(decodedUrl.txnAmount);
                    returnRes.IRSaveID = decodedUrl.bankTxnId.Contains("IRID") ? int.Parse(decodedUrl.bankTxnId.Replace("IRID", "")) : 0;
                    returnRes.BankTranId = returnRes.IRSaveID != 0 ? null : decodedUrl.bankTxnId;

                    if (decodedUrl.StatusCode == ErrorCodes.Minus1)
                    {
                        LogRailwayReqResp(new LogRailwayReqRespModel()
                        {
                            MethodName = "DVValidationErr",
                            Request = url,
                            Response = JsonConvert.SerializeObject(decodedUrl) //+ ":hitres:" + JsonConvert.SerializeObject(hitRes)
                        });
                        returnRes.StatusCode = decodedUrl.StatusCode;
                        return returnRes;
                    }

                    IRSaveModel model = new IRSaveModel()
                    {
                        EncodedRequest = url,
                        reservationId = decodedUrl.reservationId,
                        txnAmount = Convert.ToDecimal(decodedUrl.txnAmount),
                        IRSaveID = decodedUrl.bankTxnId.Contains("IRID") ? int.Parse(decodedUrl.bankTxnId.Replace("IRID", "")) : 0,
                        checkSum = decodedUrl.checkSum,
                        IsDoubleVerification = true
                    };
                    res = SaveHandledRequest(model);
                    returnRes.StatusCode = res.StatusCode;
                    returnRes.Msg = res.Msg;
                    if (res.StatusCode == ErrorCodes.One)
                    {
                        UpdateRailService(0, res.IRSaveID, 2, model.txnAmount, model.reservationId);
                    }
                    else
                    {
                        UpdateRailService(0, res.IRSaveID, 3, model.txnAmount, model.reservationId);
                    }
                }
                else
                {
                    LogRailwayReqResp(new LogRailwayReqRespModel()
                    {
                        MethodName = "DoubleVerificationDecode_decodedUrl_Null",
                        Request = url,
                        Response = IsLive.ToString()
                    });
                }
            }
            catch (Exception ex)
            {
                LogRailwayReqResp(new LogRailwayReqRespModel()
                {
                    MethodName = "Ex_DoubleVerificationDecode",
                    Request = url + IsLive.ToString(),
                    Response = ex.Message
                });
            }

            //var hitRes = TransactionReturnHit(res);
            LogRailwayReqResp(new LogRailwayReqRespModel()
            {
                MethodName = "DoubleVerificationDecode",
                Request = url,
                Response = JsonConvert.SerializeObject(res) //+ ":hitres:" + JsonConvert.SerializeObject(hitRes)
            });
            return returnRes;
        }
        public void LogRailwayReqResp(LogRailwayReqRespModel req)
        {
            req.Browser = _rinfo.GetBrowserFullInfo();
            req.IP = _rinfo.GetRemoteIP();
            IProcedure proc = new ProcLogRailwayReqResp(_dal);
            var res = (object)proc.Call(req);
        }
        public IRReturnHitModel TransactionReturnHit(IRSaveModel iRSaveModel, bool IsLive = false)
        {
            IRReturnHitModel responseStatus = new IRReturnHitModel()
            {
                ErrorCode = "400",
                Msg = ErrorCodes.FAILED
            };
            try
            {
                var model = new IRResponseModel
                {
                    merchantCode = !string.IsNullOrEmpty(iRSaveModel.merchantCode) ? iRSaveModel.merchantCode : (IsLive ? _validKeysLive.merchantCode : _validKeys.merchantCode),
                    reservationId = iRSaveModel.reservationId,
                    txnAmount = iRSaveModel.txnAmount.ToString(),
                    status = iRSaveModel.StatusCode == 1 ? "0" : "1",
                    statusDesc = !string.IsNullOrEmpty(iRSaveModel.Msg) ? iRSaveModel.Msg : (iRSaveModel.StatusCode == 1 ? "Transaction Success" : "Transaction Failed"),
                    bankTxnId = iRSaveModel.BankTranId == null ? "IRID" + iRSaveModel.IRSaveID : iRSaveModel.BankTranId
                };
                responseStatus.EncResp = PrepareEncodedResponse(model, IsLive);
                responseStatus.ErrorCode = iRSaveModel.StatusCode == 1 ? "200" : "400";
                responseStatus.Msg = iRSaveModel.StatusCode == 1 ? "SUCCESS" : "FAILURE";

                if (string.IsNullOrEmpty(iRSaveModel.RU))
                {
                    IProcedure proc = new ProcRailwayGetRU(_dal);
                    var procRes = (IRSaveModel)proc.Call(iRSaveModel);
                    iRSaveModel.RU = procRes.RU ?? null;
                }

                if (iRSaveModel != null && !string.IsNullOrEmpty(iRSaveModel.RU))
                {
                    responseStatus.RU = iRSaveModel.RU;
                }

                LogRailwayReqResp(new LogRailwayReqRespModel()
                {
                    MethodName = "TransactionReturnHit",
                    Request = JsonConvert.SerializeObject(model),
                    Response = JsonConvert.SerializeObject(responseStatus)
                });
            }
            catch (Exception ex)
            {
                responseStatus.Msg = ex.Message;
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "HitIR",
                    Error = ex.Message,
                    LoginTypeID = 1,
                    UserId = 0
                });
            }
            return responseStatus;
        }

        //------------------------------------------------------//
        #region Partner Callback
        public RailValidateResponse GenerateRailOTPForCallback(string APICode, int APIOutletID, int RefID)
        {
            IProcedure proc = new ProcRailOutletGenerateOTP(_dal);
            var otpResp = (RailOutletGenerateOTPResp)proc.Call(new CommonReq
            {
                CommonStr = APICode,
                CommonInt = APIOutletID,
                CommonInt2 = RefID,
                CommonStr2 = _rinfo.GetRemoteIP()
            });
            if (otpResp.Statuscode == ErrorCodes.One)
            {
                OTPForRail(otpResp.OTP, otpResp.Mobile, 1, otpResp.UserID, otpResp.EmailID);
            }
            return new RailValidateResponse
            {
                Statuscode = otpResp.Statuscode,
                Msg = otpResp.Msg,
                RefID = otpResp.RefID
            };
        }
        public RailValidateResponse MatchRailOTPFromCallback(int RailID, int RefID, string OTP)
        {
            IProcedure proc = new ProcValidateRailOTP(_dal);
            var matchResp = (ResponseStatus)proc.Call(new CommonReq
            {
                CommonInt = RailID,
                CommonInt2 = RefID,
                CommonStr = OTP,
                CommonStr2 = _rinfo.GetRemoteIP()
            });
            return new RailValidateResponse
            {
                Statuscode = matchResp.Statuscode,
                Msg = matchResp.Msg,
                APIRequestID = matchResp.CommonStr
            };
        }
        public RailValidateResponse DoDebitFromCallback(string AgentID, string RPID, string AccountNo, decimal AmountR, int OutletID, string APICode)
        {
            return DoRailwayTransaction(new RailwayServiceProcReq
            {
                APIRequestID = AgentID,
                VendorID = RPID,
                AccountNo = AccountNo,
                AmountR = AmountR,
                OutletID = OutletID,
                IsExternal = true,
                APICode = APICode,
                RequestIP = _rinfo.GetRemoteIP()
            });
        }
        #endregion


        //------------------------------------------------------//
        #region Private Methods
        private IRKeysSettingsModel AppSetting()
        {
            var setting = new IRKeysSettingsModel();
            try
            {
                setting = new IRKeysSettingsModel
                {
                    keyString = Configuration["IRail:keyString"],
                    ivString = Configuration["IRail:ivString"],
                    merchantCode = Configuration["IRail:merchantCode"],
                    reservationIdLength = int.Parse(Configuration["IRail:reservationIdLength"]),
                    txnAmtLength = int.Parse(Configuration["IRail:txnAmtLength"]),
                    currencyType = Configuration["IRail:currencyType"],
                    appCodeLength = int.Parse(Configuration["IRail:appCodeLength"]),
                    pymtModeLength = int.Parse(Configuration["IRail:pymtModeLength"]),
                    txnDateLength = int.Parse(Configuration["IRail:txnDateLength"]),
                    securityId = Configuration["IRail:securityId"],
                    OTPValidity = int.Parse(Configuration["IRail:OTPValidity"])
                };
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "IRKeysSettingsModel",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return setting;
        }

        private IRKeysSettingsModel AppSettingLive()
        {
            var setting = new IRKeysSettingsModel();
            try
            {
                setting = new IRKeysSettingsModel
                {
                    keyString = Configuration["IRailLive:keyString"],
                    ivString = Configuration["IRailLive:ivString"],
                    merchantCode = Configuration["IRailLive:merchantCode"],
                    reservationIdLength = int.Parse(Configuration["IRailLive:reservationIdLength"]),
                    txnAmtLength = int.Parse(Configuration["IRailLive:txnAmtLength"]),
                    currencyType = Configuration["IRailLive:currencyType"],
                    appCodeLength = int.Parse(Configuration["IRailLive:appCodeLength"]),
                    pymtModeLength = int.Parse(Configuration["IRailLive:pymtModeLength"]),
                    txnDateLength = int.Parse(Configuration["IRailLive:txnDateLength"]),
                    securityId = Configuration["IRailLive:securityId"],
                    OTPValidity = int.Parse(Configuration["IRailLive:OTPValidity"])
                };
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "IRKeysSettingsModel",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return setting;
        }
        private object DecodeUrl(string url, bool IsDoubleVerification = false, bool IsLive = false)
        {
            object res = null;
            try
            {
                var decryptedText = Decrypt(url, IsLive);
                if (!string.IsNullOrWhiteSpace(decryptedText))
                {
                    var splitUrl = decryptedText.Split('|');
                    splitUrl = splitUrl.Take(splitUrl.Length - 1).ToArray();
                    var UrlForChecksum = string.Join('|', splitUrl);
                    var checkSum = ComputeSha256Hash(UrlForChecksum).ToUpper();
                    decryptedText = "{\"" + decryptedText.Replace("=", "\":\"").Replace("|", "\",\"") + "\"}";
                    if (!IsDoubleVerification)
                    {
                        IRRequestModel requestModel = new IRRequestModel();
                        requestModel = JsonConvert.DeserializeObject<IRRequestModel>(decryptedText);
                        if (IsLive)
                        {
                            if (requestModel != null && _validKeysLive != null && requestModel.merchantCode == _validKeysLive.merchantCode && requestModel.reservationId.Length == _validKeysLive.reservationIdLength && requestModel.currencyType == _validKeysLive.currencyType || string.IsNullOrWhiteSpace(requestModel.appCode) && !(string.IsNullOrWhiteSpace(requestModel.pymtMode)) && requestModel.pymtMode.Length <= _validKeysLive.pymtModeLength && requestModel.txnDate.Length == _validKeysLive.txnDateLength && requestModel.securityId == _validKeysLive.securityId)
                            {
                                if (checkSum == requestModel.checkSum)
                                {
                                    requestModel.StatusCode = ErrorCodes.One;
                                    requestModel.Msg = ErrorCodes.SUCCESS;
                                    res = requestModel;
                                }
                            }
                        }
                        else
                        {
                            if (requestModel != null && _validKeys != null && requestModel.merchantCode == _validKeys.merchantCode && requestModel.reservationId.Length == _validKeys.reservationIdLength && requestModel.currencyType == _validKeys.currencyType || string.IsNullOrWhiteSpace(requestModel.appCode) && !(string.IsNullOrWhiteSpace(requestModel.pymtMode)) && requestModel.pymtMode.Length <= _validKeys.pymtModeLength && requestModel.txnDate.Length == _validKeys.txnDateLength && requestModel.securityId == _validKeys.securityId)
                            {
                                if (checkSum == requestModel.checkSum)
                                {
                                    requestModel.StatusCode = ErrorCodes.One;
                                    requestModel.Msg = ErrorCodes.SUCCESS;
                                    res = requestModel;
                                }
                            }
                        }
                    }
                    else
                    {
                        DoubleVerificationRequest requestModel = new DoubleVerificationRequest();
                        requestModel = JsonConvert.DeserializeObject<DoubleVerificationRequest>(decryptedText);
                        if (requestModel == null)
                        {
                            requestModel.StatusCode = ErrorCodes.Minus1;
                            requestModel.Msg = "Invalid request";
                            res = requestModel;
                            return res;
                        }
                        if (IsLive)
                        {
                            if (requestModel.merchantCode != _validKeysLive.merchantCode)
                            {
                                requestModel.StatusCode = ErrorCodes.Minus1;
                                requestModel.Msg = "Invalid merchantCode";
                                res = requestModel;
                                return res;
                            }

                            if (requestModel.reservationId.Length != _validKeysLive.reservationIdLength)
                            {
                                requestModel.StatusCode = ErrorCodes.Minus1;
                                requestModel.Msg = "Invalid reservationId";
                                res = requestModel;
                                return res;
                            }
                        }
                        else
                        {
                            if (requestModel.merchantCode != _validKeys.merchantCode)
                            {
                                requestModel.StatusCode = ErrorCodes.Minus1;
                                requestModel.Msg = "Invalid merchantCode";
                                res = requestModel;
                                return res;
                            }

                            if (requestModel.reservationId.Length != _validKeys.reservationIdLength)
                            {
                                requestModel.StatusCode = ErrorCodes.Minus1;
                                requestModel.Msg = "Invalid reservationId";
                                res = requestModel;
                                return res;
                            }
                        }

                        if (string.IsNullOrEmpty(requestModel.bankTxnId) || !requestModel.bankTxnId.Contains("IRID"))
                        {
                            requestModel.StatusCode = ErrorCodes.Minus1;
                            requestModel.Msg = "Invalid bankTxnId";
                            res = requestModel;
                            return res;
                        }
                        if (checkSum == requestModel.checkSum)
                        {
                            requestModel.StatusCode = ErrorCodes.One;
                            requestModel.Msg = ErrorCodes.SUCCESS;
                            res = requestModel;
                        }
                    }
                }
                else
                {
                    LogRailwayReqResp(new LogRailwayReqRespModel()
                    {
                        MethodName = "DecodeUrl_Null",
                        Request = IsDoubleVerification.ToString() + "_" + url,
                        Response = string.Empty
                    });
                }
            }
            catch (Exception ex)
            {
                LogRailwayReqResp(new LogRailwayReqRespModel()
                {
                    MethodName = "Ex_DecodeUrl",
                    Request = IsDoubleVerification.ToString() + "_" + url,
                    Response = ex.Message
                });
            }

            return res;
        }
        private static string ComputeSha256Hash(string rawData)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
        private string Decrypt(string cipherData, bool IsLive = false)
        {
            byte[] key; byte[] iv;
            if (IsLive)
            {
                key = Encoding.UTF8.GetBytes(_validKeysLive.keyString);
                iv = Encoding.UTF8.GetBytes(_validKeysLive.ivString);
            }
            else
            {
                key = Encoding.UTF8.GetBytes(_validKeys.keyString);
                iv = Encoding.UTF8.GetBytes(_validKeys.ivString);
            }
            try
            {
                using (var rijndaelManaged =
                       new RijndaelManaged { Key = key, IV = iv, Mode = CipherMode.CBC })
                using (var memoryStream =
                       new MemoryStream(FromHex(cipherData)))
                using (var cryptoStream =
                       new CryptoStream(memoryStream,
                           rijndaelManaged.CreateDecryptor(key, iv),
                           CryptoStreamMode.Read))
                {
                    return new StreamReader(cryptoStream).ReadToEnd();
                }
            }
            catch (CryptographicException e)
            {
                return "";
            }
        }
        private byte[] FromHex(string hex)
        {
            byte[] raw = null;
            try
            {
                hex = hex.Replace("-", "");
                raw = new byte[hex.Length / 2];
                for (int i = 0; i < raw.Length; i++)
                {
                    raw[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
                }
            }
            catch (Exception ex)
            {
                throw ex;

            }
            return raw;
        }
        private byte[] EncryptStringToBytes_Aes(string plainText, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException("plainText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");
            byte[] encrypted;

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;
                aesAlg.Mode = CipherMode.CBC;
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }
            return encrypted;
        }
        private IRSaveModel SaveHandledRequest(IRSaveModel req)
        {
            req.Browser = _rinfo.GetBrowserFullInfo();
            req.IP = _rinfo.GetRemoteIP();
            IProcedure proc = new ProcRailwaySave(_dal);
            return (IRSaveModel)proc.Call(req);
        }
        private RailValidateResponse ValidateRailUser(int RailID, string Mobile)
        {
            var res = new RailValidateResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            IProcedure proc = new ProcValidateRailOutlet(_dal);
            var procRes = (ValidateRailOutletRespProc)proc.Call(new CommonReq
            {
                CommonInt = RailID,
                CommonStr = Mobile ?? string.Empty
            });
            res.Statuscode = procRes.Statuscode;
            res.Msg = procRes.Msg;
            if (procRes.Statuscode == ErrorCodes.One)
            {
                var PHitURL = MakeURLForOTP(procRes.OTPURLRail, procRes.OutletID, 0);
                if (procRes.IsOutsider)
                {
                    var CBResp = string.Empty;
                    try
                    {
                        CBResp = AppWebRequest.O.CallUsingHttpWebRequest_GET(PHitURL);
                    }
                    catch (Exception ex)
                    {
                        CBResp = "ExceptionRP:" + ex.Message;
                    }
                    if (!string.IsNullOrEmpty(CBResp) && !(CBResp ?? string.Empty).Contains("ExceptionRP:"))
                    {
                        var PartnerResp = JsonConvert.DeserializeObject<RailValidateResponse>(CBResp);
                        res.Statuscode = PartnerResp.Statuscode;
                        if (PartnerResp.Statuscode == ErrorCodes.Minus1)
                        {
                            res.Msg = string.Format("{0}{1}", "(AP)", PartnerResp.Msg);
                        }
                        else
                        {
                            res.Msg = nameof(DMTErrorCodes.OTP_for_verification_has_been_sent_successfully).Replace("_", " ");
                            _session.SetObjectAsJson(SessionKeys.RAILOTP, new RailOTPSession
                            {
                                RefID = PartnerResp.RefID,
                                IsOutsider = true,
                                AtTime = DateTime.Now,
                                OTPURL = procRes.OTPURLRail,
                                OutletUserID = procRes.OutletUserID,
                                Mobile = Mobile,
                                RailID = procRes.OutletID,
                                MatchOTPURL = procRes.MatchOTPURLRail,
                                EmailID = procRes.EmailID
                            });
                        }
                    }

                    if (!string.IsNullOrEmpty(CBResp))
                    {
                        try
                        {
                            IProcedureAsync _procHit = new ProcUpdateAPIURLHitting(_dal);
                            var _req = new APIURLHitting
                            {
                                UserID = procRes.OutletUserID,
                                TransactionID = procRes.OTP,
                                URL = PHitURL,
                                Response = CBResp
                            };
                            var _ = _procHit.Call(_req).Result;
                        }
                        catch (Exception ex)
                        {
                            var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                            {
                                ClassName = GetType().Name,
                                FuncName = "ValidateRailUser URLHitting",
                                Error = ex.Message + "||" + CBResp,
                                LoginTypeID = 1,
                                UserId = 1
                            });
                        }
                    }
                }
                else
                {
                    OTPForRail(procRes.OTP, Mobile, 1, procRes.OutletUserID, procRes.EmailID);
                    res.Msg = nameof(DMTErrorCodes.OTP_for_verification_has_been_sent_successfully).Replace("_", " ");
                    DateTime atTime = DateTime.Now;
                    if (_env.IsProduction())
                    {
                        var localDt = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss");
                        atTime = DateTime.Parse(localDt);
                    }
                    _session.SetObjectAsJson(SessionKeys.RAILOTP, new RailOTPSession
                    {
                        OTP = procRes.OTP,
                        AtTime = atTime,
                        OutletUserID = procRes.OutletUserID,
                        Mobile = Mobile,
                        RailID = procRes.OutletID,
                        EmailID = procRes.EmailID
                    });
                }
            }
            return res;
        }
        private string MakeURLForOTP(string URL, int RailID, long RefID)
        {
            StringBuilder sb = new StringBuilder(URL);
            sb.Replace("{RailID}", RailID.ToString());
            sb.Replace("{RefID}", RefID.ToString());
            return sb.ToString();
        }
        private string MakeURLForOTPMatch(string URL, int RailID, long RefID, string OTP)
        {
            StringBuilder sb = new StringBuilder(URL);
            sb.Replace("{RailID}", RailID.ToString());
            sb.Replace("{RefID}", RefID.ToString());
            sb.Replace("{OTP}", OTP);
            return sb.ToString();
        }
        private void OTPForRail(string OTP, string MobileNo, int WID, int UserID, string EmailID)
        {
            IUserML uml = new UserML(_accessor, _env);
            var alertData = uml.GetUserDeatilForAlert(UserID);
            if (alertData.Statuscode == ErrorCodes.One)
            {
                IAlertML alertMl = new AlertML(_accessor, _env);
                alertData.OTP = OTP;
                alertData.UserMobileNo = MobileNo;
                alertData.WID = WID;
                alertData.UserID = UserID;
                alertMl.OTPSMS(alertData);
                if (string.IsNullOrEmpty(EmailID))
                {
                    alertData.EmailID = EmailID;
                    alertMl.OTPEmail(alertData);
                }
            }
        }
        private RailValidateResponse MatchRailOTP(int IRSaveID, string OTP)
        {
            var res = new RailValidateResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = nameof(ErrorCodes.Invalid_OTP).Replace("_", " ")
            };
            var IsOTPMatched = false;
            var otpSession = new RailOTPSession();
            try
            {
                otpSession = _session.GetObjectFromJson<RailOTPSession>(SessionKeys.RAILOTP);
            }
            catch (Exception ex)
            {
                var v = _session.GetString(SessionKeys.RAILOTP);
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "MatchRailOTP otpSession",
                    Error = ex.Message + "||" + (v ?? string.Empty),
                    LoginTypeID = 1,
                    UserId = 1
                });
                res.Msg = "Session Expired(Exception)";
                return res;
            }
            var APIRequestID = string.Empty;
            if (otpSession.AtTime != null)
            {
                DateTime curTime = DateTime.Now;
                if (_env.IsProduction())
                {
                    var localDt = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss");
                    curTime = DateTime.Parse(localDt);
                }
                var SessionTime = otpSession.AtTime.AddMinutes(_validKeys.OTPValidity);
                if (curTime > SessionTime)
                {
                    res.Statuscode = ErrorCodes.Minus1;
                    res.Msg = nameof(DMTErrorCodes.OTP_Expired).Replace("_", " ");
                    return res;
                }
            }
            if (otpSession.IsOutsider)
            {
                var PartnerMatchOTPURL = MakeURLForOTPMatch(otpSession.MatchOTPURL, otpSession.RailID, otpSession.RefID, OTP);
                var CBResp = string.Empty;
                try
                {
                    CBResp = AppWebRequest.O.CallUsingHttpWebRequest_GET(PartnerMatchOTPURL);
                }
                catch (Exception ex)
                {
                    CBResp = "ExceptionRP:" + ex.Message + "|" + CBResp;
                    var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                    {
                        ClassName = GetType().Name,
                        FuncName = "MatchRailOTP URLHitting",
                        Error = ex.Message,
                        LoginTypeID = 1,
                        UserId = 1
                    });
                }
                if (!string.IsNullOrEmpty(CBResp) && !(CBResp ?? string.Empty).Contains("ExceptionRP:"))
                {
                    var PartnerResp = JsonConvert.DeserializeObject<RailValidateResponse>(CBResp);
                    res.Statuscode = PartnerResp.Statuscode;
                    APIRequestID = PartnerResp.APIRequestID;
                    if (PartnerResp.Statuscode == ErrorCodes.Minus1)
                    {
                        res.Msg = string.Format("{0}{1}", "(AP)", PartnerResp.Msg);
                    }
                    else
                    {
                        IsOTPMatched = true;
                    }
                }
                else
                {
                    
                }

                if (!string.IsNullOrEmpty(CBResp))
                {
                    try
                    {
                        IProcedureAsync _procHit = new ProcUpdateAPIURLHitting(_dal);
                        var _req = new APIURLHitting
                        {
                            UserID = otpSession.OutletUserID,
                            TransactionID = string.Empty,
                            URL = PartnerMatchOTPURL,
                            Response = CBResp
                        };
                        var _ = _procHit.Call(_req).Result;
                    }
                    catch (Exception ex)
                    {
                        var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                        {
                            ClassName = GetType().Name,
                            FuncName = "MatchRailOTP URLHitting",
                            Error = ex.Message + "||" + CBResp,
                            LoginTypeID = 1,
                            UserId = 1
                        });
                    }
                }
            }
            else
            {
                IsOTPMatched = otpSession.OTP == OTP;
            }
            if (IsOTPMatched)
            {
                res.Statuscode = ErrorCodes.One;
                res.Msg = nameof(DMTErrorCodes.OTP_verified_successfully).Replace("_", " ");
                res = DoRailwayTransaction(new RailwayServiceProcReq
                {
                    IRSaveID = IRSaveID,
                    UserID = otpSession.OutletUserID,
                    OutletID = otpSession.RailID,
                    RequestIP = _rinfo.GetRemoteIP(),
                    APIRequestID = APIRequestID
                });
                if (otpSession.IsOutsider && res.Statuscode == ErrorCodes.Minus1 && res.ProcRes != null)
                {
                    if (res.ProcRes.TID > 0)
                    {
                        UpdateRailService(res.ProcRes.TID, IRSaveID, RechargeRespType.FAILED, 0, string.Empty);
                    }
                }
            }
            return res;
        }
        private RailValidateResponse DoRailwayTransaction(RailwayServiceProcReq railwayServiceProcReq)
        {
            var res = new RailValidateResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = nameof(ErrorCodes.Invalid_OTP).Replace("_", " ")
            };
            railwayServiceProcReq.RequestIP = _rinfo.GetRemoteIP();
            IProcedure proc = new ProcRailwayService(_dal);
            res.ProcRes = (RailwayServiceProcRes)proc.Call(railwayServiceProcReq);
            res.Statuscode = res.ProcRes.Statuscode;
            res.Msg = res.ProcRes.Msg;
            if (res.ProcRes.Statuscode == ErrorCodes.One)
            {
                if (res.ProcRes.IsExternal)
                {
                    if (!string.IsNullOrEmpty(res.ProcRes.DebitURLRail))
                    {
                        var CBResp = string.Empty;
                        try
                        {
                            CBResp = AppWebRequest.O.CallUsingHttpWebRequest_GET(res.ProcRes.DebitURLRail);
                        }
                        catch (Exception ex)
                        {
                            CBResp = "ExceptionRP:" + ex.Message;
                        }
                        if (!string.IsNullOrEmpty(CBResp) && !(CBResp ?? string.Empty).StartsWith("ExceptionRP:"))
                        {
                            var PartnerResp = JsonConvert.DeserializeObject<RailValidateResponse>(CBResp);
                            res.Statuscode = PartnerResp.Statuscode;
                            if (PartnerResp.Statuscode == ErrorCodes.Minus1)
                            {
                                res.Msg = string.Format("{0}{1}", "(AP)", PartnerResp.Msg);
                            }
                        }
                        else
                        {
                            res.Statuscode = ErrorCodes.Minus1;
                            res.Msg = "Exception occured!";
                        }
                        if (!string.IsNullOrEmpty(CBResp))
                        {
                            try
                            {
                                IProcedureAsync _procHit = new ProcUpdateAPIURLHitting(_dal);
                                var _req = new APIURLHitting
                                {
                                    TransactionID = string.Empty,
                                    URL = res.ProcRes.DebitURLRail,
                                    Response = CBResp
                                };
                                var _ = _procHit.Call(_req).Result;
                            }
                            catch (Exception ex)
                            {
                                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                                {
                                    ClassName = GetType().Name,
                                    FuncName = "DebitURLRail URLHitting",
                                    Error = ex.Message + "||" + CBResp,
                                    LoginTypeID = LoginType.ApplicationUser,
                                    UserId = railwayServiceProcReq.UserID
                                });
                            }
                        }
                    }
                }
            }
            return res;
        }
        private void UpdateRailService(int TID, int IRSaveID, int Type, decimal AmountR, string AccountNo)
        {
            try
            {
                IProcedure proc = new ProcUpdateRailwayService(_dal);
                var res = (_CallbackData)proc.Call(new UpdateRailServiceProcReq
                {
                    TID = TID,
                    IRSaveID = IRSaveID,
                    Type = Type,
                    AmountR = AmountR,
                    AccountNo = AccountNo
                });
                if (res.Statuscode == ErrorCodes.One)
                {
                    var CBResp = string.Empty;
                    try
                    {
                        CBResp = AppWebRequest.O.CallUsingHttpWebRequest_GET(res.UpdateUrl);
                    }
                    catch (Exception ex)
                    {
                        CBResp = "ExceptionRP:" + ex.Message;
                    }
                    if (!string.IsNullOrEmpty(CBResp) && !(CBResp ?? string.Empty).StartsWith("ExceptionRP:"))
                    {
                        var PartnerResp = JsonConvert.DeserializeObject<RailValidateResponse>(CBResp);
                        res.Statuscode = PartnerResp.Statuscode;
                        if (PartnerResp.Statuscode == ErrorCodes.Minus1)
                        {
                            res.Msg = string.Format("{0}{1}", "(AP)", PartnerResp.Msg);
                        }
                    }

                    if (!string.IsNullOrEmpty(CBResp))
                    {
                        try
                        {
                            IProcedureAsync _procHit = new ProcUpdateAPIURLHitting(_dal);
                            var _req = new APIURLHitting
                            {
                                UserID = res.UserID,
                                TransactionID = string.Empty,
                                URL = res.UpdateUrl,
                                Response = CBResp
                            };
                            var _ = _procHit.Call(_req).Result;
                        }
                        catch (Exception ex)
                        {
                            var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                            {
                                ClassName = GetType().Name,
                                FuncName = "UpdateRailService URLHitting",
                                Error = ex.Message + "||" + CBResp,
                                LoginTypeID = 1,
                                UserId = 1
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogRailwayReqResp(new LogRailwayReqRespModel()
                {
                    MethodName = "Ex_UpdateRailService",
                    Request = TID.ToString() + "_" + IRSaveID.ToString() + "_" + Type.ToString() + "_" + AmountR.ToString() + "_" + AccountNo,
                    Response = ex.Message
                });
            }

            //IRSaveModel iRSaveModel = new IRSaveModel
            //{
            //    reservationId = AccountNo,
            //    txnAmount = AmountR,
            //    IRSaveID = IRSaveID,
            //    StatusCode = Type==3 ? ErrorCodes.Minus1 : (res.Statuscode == ErrorCodes.One ? ErrorCodes.One : ErrorCodes.Minus1)
            //};
            //TransactionReturnHit(iRSaveModel);
        }
        private string PrepareEncodedResponse(IRResponseModel req, bool IsLive = false)
        {
            string strReq = JsonConvert.SerializeObject(req);
            strReq = strReq.Replace("{\"", "").Replace("\":\"", "=").Replace("\",\"", "|");
            strReq = strReq.Replace("|checkSum\":null}", "");
            var checkSum = ComputeSha256Hash(strReq).ToUpper();
            strReq += "|checkSum=" + checkSum;
            byte[] keyByte = IsLive ? Encoding.ASCII.GetBytes(_validKeysLive.keyString) : Encoding.ASCII.GetBytes(_validKeys.keyString);
            byte[] ivByte = IsLive ? Encoding.ASCII.GetBytes(_validKeysLive.ivString) : Encoding.ASCII.GetBytes(_validKeys.ivString);
            var r = EncryptStringToBytes_Aes(strReq, keyByte, ivByte);
            string bitString = BitConverter.ToString(r);
            bitString = bitString.Replace("-", "");
            return bitString;
        }
        private IResponseStatus HitIR(string url, string enc)
        {
            IResponseStatus response = new ResponseStatus()
            {
                Statuscode = ErrorCodes.Minus1,
            };
            if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(enc) || !Uri.IsWellFormedUriString(url, UriKind.Absolute))
            {
                return response;
            }
            try
            {
                url = url + "?encdata=";
                var res = AppWebRequest.O.CallUsingHttpWebRequest_POST(url, enc);
                LogRailwayReqResp(new LogRailwayReqRespModel()
                {
                    MethodName = "HitIR",
                    Request = url + enc,
                    Response = JsonConvert.SerializeObject(res)
                });
                response.Statuscode = ErrorCodes.One;
            }
            catch (Exception ex)
            {
                response.Msg = ex.Message;
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "HitIR",
                    Error = ex.Message,
                    LoginTypeID = 1,
                    UserId = 0
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return response;
        }
        #endregion
        #endregion

        #region Unused
        private string DecryptStringFromBytes_Aes(byte[] cipherText, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException("cipherText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");

            // Declare the string used to hold
            // the decrypted text.
            string plaintext = null;

            // Create an Aes object
            // with the specified key and IV.
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;
                aesAlg.Mode = CipherMode.CBC;
                //aesAlg.Padding = PaddingMode.Zeros;

                // Create a decryptor to perform the stream transform.
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for decryption.
                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            //csDecrypt.Write(cipherText, 0, cipherText.Length);
                            //csDecrypt.FlushFinalBlock();
                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string.
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }

            return plaintext;
        }
        private static string DecryptToBytesUsingCBC(byte[] toDecrypt, byte[] Key, byte[] IV)
        {
            byte[] src = toDecrypt;
            byte[] dest = new byte[src.Length];
            using (var aes = new AesCryptoServiceProvider())
            {
                //aes.BlockSize = 128;
                //aes.KeySize = 128;
                aes.IV = IV;
                aes.Key = Key;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.Zeros;
                // decryption
                using (ICryptoTransform decrypt = aes.CreateDecryptor(aes.Key, aes.IV))
                {
                    byte[] decryptedText = decrypt.TransformFinalBlock(src, 0, src.Length);

                    return Encoding.UTF8.GetString(decryptedText);
                }
            }
        }
        //private string Encrypt(string cipherData)
        //{

        //    byte[] key = Encoding.UTF8.GetBytes(keyString);
        //    byte[] iv = Encoding.UTF8.GetBytes(ivString);

        //    try
        //    {
        //        using (var rijndaelManaged =
        //               new RijndaelManaged { Key = key, IV = iv, Mode = CipherMode.CBC })
        //        using (var memoryStream =
        //               new MemoryStream(FromHex(cipherData)))
        //        using (var cryptoStream =
        //               new CryptoStream(memoryStream,
        //                   rijndaelManaged.CreateEncryptor(key, iv),
        //                   CryptoStreamMode.Read))
        //        {
        //            return new StreamReader(cryptoStream).ReadToEnd();
        //        }
        //    }
        //    catch (CryptographicException e)
        //    {
        //        return "";
        //    }
        //    // You may want to catch more exceptions here...
        //}
        #endregion


        public string TestPrepareEncodedResponse(TestIRRequestModel req)
        {
            string strReq = JsonConvert.SerializeObject(req);
            strReq = strReq.Replace("{\"", "").Replace("\":\"", "=").Replace("\",\"", "|");
            strReq = strReq.Replace("|checkSum\":null}", "");
            var checkSum = ComputeSha256Hash(strReq).ToUpper();
            strReq += "|checkSum=" + checkSum;
            byte[] keyByte = Encoding.ASCII.GetBytes(_validKeysLive.keyString);
            byte[] ivByte = Encoding.ASCII.GetBytes(_validKeys.ivString);
            var r = EncryptStringToBytes_Aes(strReq, keyByte, ivByte);
            string bitString = BitConverter.ToString(r);
            bitString = bitString.Replace("-", "");
            return bitString;
        }
    }
}
