using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
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
using RoundpayFinTech.AppCode.Model.ICollect;
using RoundpayFinTech.AppCode.Model.MoneyTransfer;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.StaticModel.MoneyTransfer;
using RoundpayFinTech.AppCode.ThirdParty.CashFree;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.MiddleLayer.Dmt_Api
{
    public class CashFreePayoutML : IMoneyTransferAPIML
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        private readonly IConfiguration Configuration;
        private readonly IConnectionConfiguration _c;
        private readonly CashFreeAppSetting appSetting;
        private readonly IDAL _dal;
        private string _Token;
        private string _TokenCollect;
        private string _APIGroupCode;
        private IErrorCodeML errorCodeML;

        public CashFreePayoutML(IHttpContextAccessor accessor, IHostingEnvironment env, IDAL dal, string APIGroupCode)
        {
            _accessor = accessor;
            _env = env;
            var builder = new ConfigurationBuilder()
                 .SetBasePath(Directory.GetCurrentDirectory());
            builder.AddJsonFile((_env.IsProduction() ? "appsettings.json" : "appsettings.Development.json"));
            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
            _dal = dal;
            appSetting = AppSetting();
            _APIGroupCode = APIGroupCode;
            errorCodeML = new ErrorCodeML(_accessor, _env, false);
        }
        private CashFreeAppSetting AppSetting()
        {
            var setting = new CashFreeAppSetting();
            try
            {
                setting = new CashFreeAppSetting
                {
                    CLIENTID = Configuration["DMR:CASHFREE:CLIENTID"],
                    BaseURL = Configuration["DMR:CASHFREE:BaseURL"],
                    Host = Configuration["DMR:CASHFREE:Host"],
                    SECRETKEY = Configuration["DMR:CASHFREE:SECRETKEY"],
                    CollectHost = Configuration["DMR:CASHFREE:CollectHost"],
                    CollectClientID = Configuration["DMR:CASHFREE:CollectClientID"],
                    CollectSecretKey = Configuration["DMR:CASHFREE:CollectSecretKey"],
                };
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "CashFreeAppSetting",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser
                });
            }
            return setting;
        }
        private void CFAuthorization()
        {
            string _request = string.Empty, _response = string.Empty;

            var headers = new Dictionary<string, string>
            {
                { "X-Client-Id", appSetting.CLIENTID },
                { "X-Client-Secret", appSetting.SECRETKEY }
            };
            _request = appSetting.BaseURL + "v1/authorize";
            var apiResp = AppWebRequest.O.HWRPost(_request, "", headers);
            _response = apiResp;
            if (!string.IsNullOrEmpty(apiResp))
            {
                try
                {
                    var res = JsonConvert.DeserializeObject<CFTokenGen>(apiResp);
                    if (res != null)
                    {
                        if (res.data != null)
                            _Token = res.data.token;
                    }
                }
                catch (Exception ex)
                {
                    _response = "Exception:" + ex.Message + "||" + _response;
                }
            }


            if (!string.IsNullOrEmpty(_request) || !string.IsNullOrEmpty(_response))
            {
                IProcedure proc = new ProcLogAPITokenGeneration(_dal);
                proc.Call(new CommonReq
                {
                    str = APICode.CASHFREE,
                    CommonStr = _request,
                    CommonStr2 = _response
                });
            }
        }
        private void CACAuthorization()
        {
            string _request = string.Empty, _response = string.Empty;

            var headers = new Dictionary<string, string>
            {
                { "X-Client-Id", appSetting.CollectClientID },
                { "X-Client-Secret", appSetting.CollectSecretKey }
            };
            _request = appSetting.CollectHost + "cac/v1/authorize";
            var apiResp = AppWebRequest.O.HWRPost(_request, "", headers);
            _response = apiResp;
            if (!string.IsNullOrEmpty(apiResp))
            {
                try
                {
                    var res = JsonConvert.DeserializeObject<CFTokenGen>(apiResp);
                    if (res != null)
                    {
                        if (res.data != null)
                            _TokenCollect = res.data.token;
                    }
                }
                catch (Exception ex)
                {
                    _response = "Exception:" + ex.Message + "||" + _response;
                }
            }


            if (!string.IsNullOrEmpty(_request) || !string.IsNullOrEmpty(_response))
            {
                IProcedure proc = new ProcLogAPITokenGeneration(_dal);
                proc.Call(new CommonReq
                {
                    str = APICode.CASHFREE,
                    CommonStr = _request,
                    CommonStr2 = _response
                });
            }
        }

        public ICollectStatusResponse StatusCheckICollect(string RefID)
        {
            ICollectStatusResponse collectStatusResponse = new ICollectStatusResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = "No Response"
            };
            CACAuthorization();
            StringBuilder _URL = new StringBuilder(appSetting.CollectHost);
            _URL.Append("cac/v1/fetchPaymentById/");
            _URL.Append(RefID ?? string.Empty);
            var res = new MSenderLoginResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError,
                ErrorCode = ErrorCodes.Transaction_Successful
            };
            var headers = new Dictionary<string, string>
            {
                { "Authorization", "Bearer " + _TokenCollect}
            };

            string resp = string.Empty;
            try
            {
                resp = AppWebRequest.O.CallUsingHttpWebRequest_GETAsync(_URL.ToString(), headers).Result;
                if (resp != null)
                {
                    var apiResp = JsonConvert.DeserializeObject<CaashfreeCollectStatuscheckresp>(resp);
                    if (apiResp.status.Equals("SUCCESS"))
                    {
                        collectStatusResponse.Statuscode = ErrorCodes.One;
                        collectStatusResponse.Msg = apiResp.message;
                        if (apiResp.data != null)
                        {
                            if (apiResp.data.payment != null)
                            {
                                collectStatusResponse.Amount = apiResp.data.payment.amount;
                                collectStatusResponse.UTR = apiResp.data.payment.utr;
                            }
                        }
                    }
                    else
                    {
                        collectStatusResponse.Msg = apiResp.message;
                    }
                }
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "StatusCheckICollect",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser
                });
            }
            new ProcUpdateLogDMRReqResp(_dal).Call(new DMTReqRes
            {
                APIID = 0,
                Method = "StatusCheckICollect",
                RequestModeID = 0,
                Request = _URL.ToString() + JsonConvert.SerializeObject(headers),
                Response = resp ?? string.Empty
            });
            return collectStatusResponse;
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

        public MSenderCreateResp SenderKYC(MTAPIRequest request)
        {
            throw new NotImplementedException();
        }

        public MSenderCreateResp SenderEKYC(MTAPIRequest request)
        {
            throw new NotImplementedException();
        }

        public MSenderCreateResp SenderResendOTP(MTAPIRequest request)
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
        public MSenderLoginResponse CreateBeneficiary(MTAPIRequest request)
        {
            CFAuthorization();
            var res = new MSenderLoginResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError,
                ErrorCode = ErrorCodes.Transaction_Successful
            };
            var headers = new Dictionary<string, string>
            {
                { "Authorization", "Bearer " + _Token }
            };
            var beneRequest = new CFPAddBeneReq
            {
                beneId = "CF" + DateTime.Now.ToString("yyMdHmsfff"),
                name = request.mBeneDetail.BeneName,
                email = request.EmailID,
                phone = request.mBeneDetail.MobileNo,
                bankAccount = request.mBeneDetail.AccountNo,
                ifsc = request.mBeneDetail.IFSC,
                address1 = request.Address,
                city = request.City,
                state = request.StateName,
                pincode = request.Pincode.ToString()
            };
            string URL = appSetting.BaseURL + "v1/addBeneficiary";
            string resp = string.Empty;
            try
            {
                resp = AppWebRequest.O.PostJsonDataUsingHWRTLS(URL, beneRequest, headers).Result;
                if (resp != null)
                {
                    var apiResp = JsonConvert.DeserializeObject<CFPAddBeneResp>(resp);
                    if (apiResp.status.Equals("SUCCESS"))
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
                            _APICode = APICode.CASHFREE,
                            _BankID = request.mBeneDetail.BankID,
                            _CashFreeID = beneRequest.beneId,
                            _BeneAPIID = request.APIID
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
                    else if (apiResp.status.Equals("ERROR") && apiResp.subCode.Equals("409"))
                    {
                        var beneID = GetBeneIDByAccDetails(request).BeneID;
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
                            _APICode = APICode.CASHFREE,
                            _BankID = request.mBeneDetail.BankID,
                            _CashFreeID = beneID
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
                    else
                    {
                        res.Statuscode = ErrorCodes.Minus1;
                        res.Msg = ErrorCodes.BENEFLD;
                        res.ErrorCode = ErrorCodes.Transaction_Failed_Replace;
                    }
                }
                else
                {
                    res.Statuscode = ErrorCodes.Minus1;
                    res.Msg = ErrorCodes.BENEFLD;
                    res.ErrorCode = ErrorCodes.Transaction_Failed_Replace;
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
        public MBeneficiaryResp GetBeneficiary(MTAPIRequest request)
        {
            var res = new MBeneficiaryResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = nameof(DMTErrorCodes.Beneficiary_not_found).Replace("_", " "),
                ErrorCode = DMTErrorCodes.Beneficiary_not_found
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
                                MobileNo = r._MobileNo,
                                BeneID = r._ID.ToString(),
                                BankID = r._BankID,
                                CashFreeID = r._CashFreeID
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
                    ClassName = GetType().ToString(),
                    FuncName = "GetBeneficiary",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = request.UserID
                });
            }
            return res;
        }
        public MSenderCreateResp GenerateOTP(MTAPIRequest request)
        {
            throw new NotImplementedException();
        }
        public MSenderLoginResponse ValidateBeneficiary(MTAPIRequest request)
        {
            throw new NotImplementedException();
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
        public MSenderLoginResponse RemoveBeneficiaryValidate(MTAPIRequest request)
        {
            throw new NotImplementedException();
        }
        public DMRTransactionResponse VerifyAccount(MTAPIRequest request)
        {
            CFAuthorization();
            var res = new DMRTransactionResponse
            {
                Statuscode = RechargeRespType.PENDING,
                Msg = RechargeRespType._PENDING,
                VendorID = "",
                LiveID = ""
            };
            var headers = new Dictionary<string, string>
            {
                { "Authorization", "Bearer " + _Token }
            };
            StringBuilder sb = new StringBuilder(appSetting.BaseURL + "v1.2/validation/bankDetails?name={NAME}&phone={SENDER}&bankAccount={ACCOUNT}&ifsc={IFSC}");
            sb.Replace("{NAME}", request.mBeneDetail.BeneName);
            sb.Replace("{SENDER}", request.SenderMobile);
            sb.Replace("{ACCOUNT}", request.mBeneDetail.AccountNo);
            sb.Replace("{IFSC}", request.mBeneDetail.IFSC);
            var resp = string.Empty;
            try
            {
                resp = AppWebRequest.O.CallUsingHttpWebRequest_GETAsync(sb.ToString(), headers).Result;
                if (resp != null)
                {
                    var apiResp = JsonConvert.DeserializeObject<CFAccVerify>(resp);

                    if (apiResp != null)
                    {
                        if (apiResp.status.Equals("SUCCESS"))
                        {
                            if (apiResp.accountStatus.Equals("VALID"))
                            {
                                res.LiveID = apiResp.data.utr;
                                res.Statuscode = RechargeRespType.SUCCESS;
                                res.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                                res.ErrorCode = ErrorCodes.Transaction_Successful;
                                res.BeneName = apiResp.data.nameAtBank;
                            }
                            else if (apiResp.accountStatus.Equals("INVALID"))
                            {
                                var eFromDB = errorCodeML.GetAPIErrorCodeDescription(request.APIGroupCode, apiResp.message);
                                if (!string.IsNullOrEmpty(eFromDB.Code))
                                {
                                    res.Statuscode = eFromDB.Status;
                                    res.Msg = eFromDB.Error.Replace("{REPLACE}", apiResp.message);
                                    res.ErrorCode = Convert.ToInt32(eFromDB.Code.Trim());
                                    res.LiveID = res.Msg;
                                }
                                else
                                {
                                    res.Statuscode = RechargeRespType.FAILED;
                                    res.Msg = apiResp.message;
                                    res.ErrorCode = ErrorCodes.Transaction_Replace;
                                    res.LiveID = res.Msg;
                                }
                                //res.Statuscode = res.Statuscode == 0 ? RechargeRespType.PENDING : res.Statuscode;
                            }
                        }
                        else
                        {
                            res.Statuscode = RechargeRespType.FAILED;
                            res.Msg = apiResp.message;
                            res.ErrorCode = ErrorCodes.Unknown_Error;
                            res.LiveID = res.Msg;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                resp = " Exception:" + ex.Message + " | " + resp;
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "VerifyAccount",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = request.UserID
                });
            }
            res.Request = sb.ToString() + "?" + sb.ToString() + JsonConvert.SerializeObject(headers);
            new ProcUpdateLogDMRReqResp(_dal).Call(new DMTReqRes
            {
                APIID = request.APIID,
                Method = "VerifyAccount",
                RequestModeID = request.RequestMode,
                Request = res.Request,
                Response = resp ?? string.Empty,
                SenderNo = request.SenderMobile,
                UserID = request.UserID,
                TID = request.TID.ToString()
            });
            res.Response = resp;
            return res;
        }
        public DMRTransactionResponse AccountTransfer(MTAPIRequest request)
        {
            CFAuthorization();
            var res = new DMRTransactionResponse
            {
                Statuscode = RechargeRespType.PENDING,
                Msg = RechargeRespType._PENDING,
                VendorID = "",
                LiveID = "",
                ErrorCode = ErrorCodes.Request_Accpeted
            };

            var beneRes = GetBeneIDByAccDetails(request);
            if (!string.IsNullOrEmpty(beneRes.BeneID))
            {
                request.mBeneDetail.BeneID = beneRes.BeneID;
            }
            var headers = new Dictionary<string, string>
            {
                { "Authorization", "Bearer " + _Token }
            };
            var accTrReq = new
            {
                beneId = request.mBeneDetail.BeneID,
                amount = request.Amount,
                transferId = request.TID
            };
            string URL = appSetting.BaseURL + "v1/requestAsyncTransfer";
            string resp = string.Empty;

            try
            {
                resp = AppWebRequest.O.PostJsonDataUsingHWRTLS(URL, accTrReq, headers).Result;
                if (resp != null)
                {
                    var cFAccTRResp = JsonConvert.DeserializeObject<CFAccTRResp>(resp);

                    if (cFAccTRResp.status.Equals("ACCEPTED"))
                    {
                        res.Statuscode = RechargeRespType.PENDING;
                        res.Msg = nameof(ErrorCodes.Request_Accpeted).Replace("_", " ");
                        res.ErrorCode = ErrorCodes.Request_Accpeted;
                        res.LiveID = string.Empty;
                        res.VendorID = cFAccTRResp.data.referenceId;
                    }
                    else if (cFAccTRResp.status.Equals("SUCCESS"))
                    {
                        res.Statuscode = RechargeRespType.SUCCESS;
                        res.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                        res.ErrorCode = ErrorCodes.Transaction_Successful;
                        res.LiveID = string.Empty;
                        res.VendorID = cFAccTRResp.data.referenceId;
                    }
                    else if (cFAccTRResp.status.Equals("FAILURE"))
                    {
                        res.LiveID = "";
                        res.VendorID = cFAccTRResp.data.referenceId;
                        res.Statuscode = RechargeRespType.FAILED;
                        res.Msg = nameof(DMTErrorCodes.Transaction_Failed).Replace("_", " ");
                        res.ErrorCode = DMTErrorCodes.Transaction_Failed;
                    }
                }
                else
                {
                    res.LiveID = nameof(ErrorCodes.ServiceDown);
                    res.Statuscode = RechargeRespType.FAILED;
                    res.Msg = nameof(ErrorCodes.ServiceDown);
                    res.ErrorCode = ErrorCodes.ServiceDown;
                }
            }
            catch (Exception ex)
            {
                resp = " Exception:" + ex.Message + " | " + resp;
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
                Request = URL + JsonConvert.SerializeObject(accTrReq) + JsonConvert.SerializeObject(headers),
                Response = resp ?? string.Empty,
                SenderNo = request.SenderMobile,
                UserID = request.UserID,
                TID = request.TID.ToString()
            };
            new ProcUpdateLogDMRReqResp(_dal).Call(dMTReq);
            res.Request = dMTReq.Request;
            res.Response = dMTReq.Response;
            var VendorID = res.VendorID;
            if (res.Statuscode == RechargeRespType.PENDING)
            {
                var tempres = LoopStatusCheck(request.TID, request.TransactionID, request.RequestMode, request.UserID, request.APIID, VendorID, request.TransMode == "NEFT", request.APIGroupCode).Result;
                res.Statuscode = tempres.Statuscode;
                res.LiveID = tempres.LiveID;
                res.VendorID = tempres.VendorID;
                res.Msg = tempres.Msg;
                res.ErrorCode = tempres.ErrorCode;
                res.TID = request.TID;
            }
            return res;
        }
        private MSenderLoginResponse GetBeneIDByAccDetails(MTAPIRequest request)
        {
            var res = new MSenderLoginResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError,
                ErrorCode = ErrorCodes.Transaction_Successful
            };
            CFAuthorization();
            var headers = new Dictionary<string, string>
            {
                { "Authorization", "Bearer " + _Token }
            };
            StringBuilder sb = new StringBuilder(appSetting.BaseURL + "v1/getBeneId?bankAccount={ACCOUNT}&ifsc={IFSC}");
            sb.Replace("{ACCOUNT}", request.mBeneDetail.AccountNo);
            sb.Replace("{IFSC}", request.mBeneDetail.IFSC);
            var resp = string.Empty;
            try
            {
                resp = AppWebRequest.O.CallUsingHttpWebRequest_GETAsync(sb.ToString(), headers).Result;
                if (resp != null)
                {
                    var apiResp = JsonConvert.DeserializeObject<CFBeneIDResp>(resp);
                    if (apiResp.status.Equals("SUCCESS"))
                    {
                        res.BeneID = apiResp.data.beneId;
                    }
                    else
                    {
                        res = CreateBeneForAccTrf(request);
                    }
                }
            }
            catch (Exception ex)
            {
                resp = " Exception:" + ex.Message + " | " + resp;
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "CashFreeGetBeneID",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = request.UserID
                });
            }
            var Request = sb.ToString() + "?" + JsonConvert.SerializeObject(headers);
            new ProcUpdateLogDMRReqResp(_dal).Call(new DMTReqRes
            {
                APIID = request.APIID,
                Method = "CashFreeGetBeneID",
                RequestModeID = 1,
                Request = Request,
                Response = resp ?? string.Empty,
                SenderNo = request.SenderMobile,
                UserID = request.UserID,
                TID = "0"
            });
            return res;
        }
        public DMRTransactionResponse GetTransactionStatus(int TID, string VendorID, int RequestMode, int UserID, int APIID, string APIGroupCode = "")
        {
            CFAuthorization();
            var res = new DMRTransactionResponse
            {
                Statuscode = RechargeRespType.PENDING,
                Msg = nameof(RechargeRespType.PENDING),
                ErrorCode = ErrorCodes.Request_Accpeted
            };
            var headers = new Dictionary<string, string>
            {
                { "Authorization", "Bearer " + _Token }
            };
            StringBuilder sb = new StringBuilder(appSetting.BaseURL + "v1/getTransferStatus?referenceId={REFID}");
            sb.Replace("{REFID}", VendorID);
            string resp = string.Empty;
            try
            {
                resp = AppWebRequest.O.CallUsingHttpWebRequest_GETAsync(sb.ToString(), headers).Result;
                if (resp != null)
                {
                    var cFStsChkResp = JsonConvert.DeserializeObject<CFStsChkResp>(resp);
                    if (cFStsChkResp != null)
                    {
                        if (cFStsChkResp.data.transfer != null)
                        {
                            if (cFStsChkResp.data.transfer.status.Equals("REJECTED"))
                            {
                                res.VendorID = VendorID;
                                //res.LiveID = cFStsChkResp.data.transfer.reason.Contains("suffi") ? ErrorCodes.Down : cFStsChkResp.data.transfer.reason;
                                var eFromDB = errorCodeML.GetAPIErrorCodeDescription(APIGroupCode, cFStsChkResp.data.transfer.reason);
                                if (!string.IsNullOrEmpty(eFromDB.Code))
                                {
                                    res.Statuscode = eFromDB.Status;
                                    res.Msg = eFromDB.Error.Replace("{REPLACE}", cFStsChkResp.data.transfer.reason);
                                    res.ErrorCode = Convert.ToInt32(eFromDB.Code.Trim());
                                    res.LiveID = res.Msg;
                                }
                                else
                                {
                                    res.Msg = cFStsChkResp.data.transfer.reason;
                                    res.ErrorCode = ErrorCodes.Transaction_Replace;
                                    res.LiveID = res.Msg;
                                }
                                res.Statuscode = res.Statuscode == 0 ? RechargeRespType.PENDING : res.Statuscode;
                            }
                            else if (cFStsChkResp.data.transfer.status.Equals("SUCCESS"))
                            {
                                res.VendorID = VendorID;
                                res.LiveID = cFStsChkResp.data.transfer.utr;
                                res.Statuscode = RechargeRespType.SUCCESS;
                                res.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                                res.ErrorCode = ErrorCodes.Transaction_Successful;
                            }
                            else
                            {
                                var eFromDB = errorCodeML.GetAPIErrorCodeDescription(APIGroupCode, cFStsChkResp.data.transfer.reason);
                                if (!string.IsNullOrEmpty(eFromDB.Code))
                                {
                                    res.Statuscode = eFromDB.Status;
                                    res.Msg = eFromDB.Error.Replace("{REPLACE}", cFStsChkResp.data.transfer.reason);
                                    res.ErrorCode = Convert.ToInt32(eFromDB.Code.Trim());
                                    res.LiveID = res.Msg;
                                }
                                else
                                {
                                    res.Msg = cFStsChkResp.data.transfer.reason;
                                    res.ErrorCode = ErrorCodes.Transaction_Replace;
                                    res.LiveID = res.Msg;
                                }
                                res.Statuscode = res.Statuscode == 0 ? RechargeRespType.PENDING : res.Statuscode;
                            }
                        }
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
                APIID = APIID,
                Method = "GetTransactionStatus",
                RequestModeID = RequestMode,
                Request = sb.ToString() + "?" + JsonConvert.SerializeObject(headers),
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
        private async Task<DMRTransactionResponse> LoopStatusCheck(int TID, string TransactionID, int RMode, int UserID, int APIID, string VendorID, bool IsNeft = false, string APIGroupCode = "")
        {
            var res = new DMRTransactionResponse
            {
                Statuscode = RechargeRespType.PENDING,
                Msg = nameof(ErrorCodes.Request_Accpeted)
            };
            int i = 0;
            int LoopTill = 12;
            int delayTime = 1;
            while (i < LoopTill)
            {
                i++;
                if (res.Statuscode == RechargeRespType.PENDING)
                {
                    await Task.Delay(delayTime * 1000).ConfigureAwait(false);
                    delayTime = 5;
                    res = GetTransactionStatus(TID, VendorID, RMode, UserID, APIID, APIGroupCode);
                    if (res.Statuscode != RechargeRespType.PENDING || IsNeft)
                    {
                        i = LoopTill;
                    }
                    if (res.Statuscode == RechargeRespType.PENDING && IsNeft)
                    {
                        res.Statuscode = RechargeRespType.SUCCESS;
                    }
                }
                else
                {
                    i = LoopTill;
                }
            }
            return res;
        }
        private MSenderLoginResponse CreateBeneForAccTrf(MTAPIRequest request)
        {
            CFAuthorization();
            var res = new MSenderLoginResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError,
                ErrorCode = ErrorCodes.Transaction_Successful
            };
            var headers = new Dictionary<string, string>
            {
                { "Authorization", "Bearer " + _Token }
            };
            var beneRequest = new CFPAddBeneReq
            {
                beneId = "CF" + DateTime.Now.ToString("yyMdHmsfff"),
                name = request.mBeneDetail.BeneName,
                email = request.EmailID,
                phone = request.mBeneDetail.MobileNo,
                bankAccount = request.mBeneDetail.AccountNo,
                ifsc = request.mBeneDetail.IFSC,
                address1 = request.Address,
                city = request.City,
                state = request.StateName,
                pincode = request.Pincode.ToString()
            };
            res.BeneID = beneRequest.beneId;
            string URL = appSetting.BaseURL + "v1/addBeneficiary";
            string resp = string.Empty;
            try
            {
                resp = AppWebRequest.O.PostJsonDataUsingHWRTLS(URL, beneRequest, headers).Result;
                if (resp != null)
                {
                    var apiResp = JsonConvert.DeserializeObject<CFPAddBeneResp>(resp);
                    if (apiResp.status.Equals("SUCCESS"))
                    {
                        res.BeneID = beneRequest.beneId;
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
                            _APICode = APICode.CASHFREE,
                            _BankID = request.mBeneDetail.BankID,
                            _CashFreeID = beneRequest.beneId,
                            _BeneAPIID = request.APIID
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
                    //else if (apiResp.status.Equals("ERROR") && apiResp.subCode.Equals("409"))
                    //{
                    //    var beneID = GetBeneIDByAccDetails(request.mBeneDetail.AccountNo, request.mBeneDetail.IFSC, request.UserID, request.SenderMobile, request.APIID);
                    //    var param = new BenificiaryDetail
                    //    {
                    //        _SenderMobileNo = request.SenderMobile,
                    //        _Name = request.mBeneDetail.BeneName,
                    //        _AccountNumber = request.mBeneDetail.AccountNo,
                    //        _MobileNo = request.mBeneDetail.MobileNo,
                    //        _IFSC = request.mBeneDetail.IFSC,
                    //        _BankName = request.mBeneDetail.BankName,
                    //        _EntryBy = request.UserID,
                    //        _VerifyStatus = 1,
                    //        _APICode = APICode.CASHFREE,
                    //        _BankID = request.mBeneDetail.BankID,
                    //        _CashFreeID = beneID,
                    //        _BeneAPIID = request.APIID
                    //    };
                    //    var resdb = (BenificiaryModel)new ProcAddBenificiary(_dal).Call(param);
                    //    if (resdb.Statuscode == ErrorCodes.One)
                    //    {
                    //        res.Statuscode = ErrorCodes.One;
                    //        res.Msg = ErrorCodes.BENESCS;
                    //        res.ErrorCode = ErrorCodes.Transaction_Successful;
                    //        res.BeneID = beneID;
                    //    }
                    //    else
                    //    {
                    //        res.Msg = resdb.Msg;
                    //    }
                    //}
                    else
                    {
                        res.Statuscode = ErrorCodes.Minus1;
                        res.Msg = ErrorCodes.BENEFLD;
                        res.ErrorCode = ErrorCodes.Transaction_Failed_Replace;
                    }
                }
                else
                {
                    res.Statuscode = ErrorCodes.Minus1;
                    res.Msg = ErrorCodes.BENEFLD;
                    res.ErrorCode = ErrorCodes.Transaction_Failed_Replace;
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
            var dMTReq = new DMTReqRes
            {
                APIID = 0,
                Method = "CreateBeneficiary",
                RequestModeID = request.RequestMode,
                Request = URL + "?" + JsonConvert.SerializeObject(beneRequest),
                Response = resp,
                SenderNo = string.Empty,
                UserID = request.UserID,
                TID = "0"
            };
            return res;
        }
    }
}
