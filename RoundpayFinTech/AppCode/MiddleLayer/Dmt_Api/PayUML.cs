using Fintech.AppCode.DB;
using Fintech.AppCode.HelperClass;
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
using RoundpayFinTech.AppCode.Model.MoneyTransfer;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.StaticModel.MoneyTransfer;
using RoundpayFinTech.AppCode.ThirdParty.PayU;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.MiddleLayer.Dmt_Api
{
    public class PayUML : IMoneyTransferAPIML
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        private readonly IConfiguration Configuration;
        private readonly IConnectionConfiguration _c;
        private readonly PayUSetting appSetting;
        private readonly IDAL _dal;
        private static long TOKEN_CREATED = 0;
        private static int EXPIRES_IN = 0;
        private static string TOKEN = string.Empty;
        private static string TOKEN_REFRESH = string.Empty;
        public PayUML(IHttpContextAccessor accessor, IHostingEnvironment env, IDAL dal)
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
        }
        private PayUSetting AppSetting()
        {
            var setting = new PayUSetting();
            try
            {
                setting = new PayUSetting
                {
                    CLIENTID = Configuration["DMR:PAYU:CLIENTID"],
                    USERNAME = Configuration["DMR:PAYU:USERNAME"],
                    PASSWORD = Configuration["DMR:PAYU:PASSWORD"],
                    PAYOUTID = Configuration["DMR:PAYU:PAYOUTID"],
                    BASEURL = Configuration["DMR:PAYU:BaseURL"],
                    AUTHURL = Configuration["DMR:PAYU:AUTHURL"]
                };
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "PayUSetting",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser
                });
            }
            return setting;
        }
        private void TokenGeneration()
        {
            string _request = string.Empty, _response = string.Empty;
            if (EXPIRES_IN == 0 || string.IsNullOrEmpty(TOKEN))
            {
                var req = new PayUTokenRequest
                {
                    client_id = appSetting.CLIENTID,
                    grant_type = "password",
                    password = appSetting.PASSWORD,
                    username = appSetting.USERNAME,
                    scope = "create_payout_transactions"
                };
                StringBuilder sb = new StringBuilder();
                sb.Append(nameof(req.client_id));
                sb.Append("=");
                sb.Append(req.client_id);
                sb.Append("&");
                sb.Append(nameof(req.grant_type));
                sb.Append("=");
                sb.Append(req.grant_type);
                sb.Append("&");
                sb.Append(nameof(req.username));
                sb.Append("=");
                sb.Append(req.username);
                sb.Append("&");
                sb.Append(nameof(req.password));
                sb.Append("=");
                sb.Append(req.password);
                sb.Append("&");
                sb.Append(nameof(req.scope));
                sb.Append("=");
                sb.Append(req.scope);
                _request = appSetting.AUTHURL + "?" + sb.ToString();
                var apiResp = AppWebRequest.O.CallUsingHttpWebRequest_POST(appSetting.AUTHURL, sb.ToString());
                _response = apiResp;
                if (!string.IsNullOrEmpty(apiResp))
                {
                    try
                    {
                        var res = JsonConvert.DeserializeObject<PayUTokenResponse>(apiResp);
                        if (res.expires_in > 0)
                        {
                            EXPIRES_IN = res.expires_in;
                            TOKEN_CREATED = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                            TOKEN = res.access_token;
                            TOKEN_REFRESH = res.refresh_token;
                        }
                    }
                    catch (Exception ex)
                    {
                        _response = "Exception:" + ex.Message + "||" + _response;
                    }
                }
            }
            else
            {
                if ((EXPIRES_IN - (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - TOKEN_CREATED) / 1000) < 2)
                {
                    var req = new PayUTokenRequest
                    {
                        client_id = appSetting.CLIENTID,
                        grant_type = "refresh_token",
                        password = appSetting.PASSWORD,
                        username = appSetting.USERNAME,
                        scope = "create_payout_transactions"
                    };
                    StringBuilder sb = new StringBuilder();
                    sb.Append(nameof(req.client_id));
                    sb.Append("=");
                    sb.Append(req.client_id);
                    sb.Append("&");
                    sb.Append(nameof(req.grant_type));
                    sb.Append("=");
                    sb.Append(req.grant_type);
                    sb.Append("&");
                    sb.Append("refresh_token=");
                    sb.Append(TOKEN_REFRESH);
                    _request = appSetting.AUTHURL + "?" + sb.ToString();
                    var apiResp = AppWebRequest.O.CallUsingHttpWebRequest_POST(appSetting.AUTHURL, sb.ToString());
                    _response = apiResp;
                    if (!string.IsNullOrEmpty(apiResp))
                    {
                        try
                        {
                            var res = JsonConvert.DeserializeObject<PayUTokenResponse>(apiResp);
                            if (res.expires_in > 0)
                            {
                                EXPIRES_IN = res.expires_in;
                                TOKEN_CREATED = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                                TOKEN = res.access_token;
                                TOKEN_REFRESH = res.refresh_token;
                            }
                        }
                        catch (Exception ex)
                        {
                            _response = "Exception:" + ex.Message + "||" + _response;
                        }
                    }
                }
            }
            if (!string.IsNullOrEmpty(_request) || !string.IsNullOrEmpty(_response))
            {
                IProcedure proc = new ProcLogAPITokenGeneration(_dal);
                proc.Call(new CommonReq
                {
                    str = APICode.PAYU,
                    CommonStr = _request,
                    CommonStr2 = _response
                });
            }
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

        public MBeneficiaryResp GetBeneficiary(MTAPIRequest request)
        {
            return DMTAPIHelperML.GetBeneficiary(request, _dal, GetType().Name);
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
            TokenGeneration();
            var res = new DMRTransactionResponse
            {
                Statuscode = RechargeRespType.PENDING,
                Msg = RechargeRespType._PENDING,
                VendorID = "",
                LiveID = ""
            };

            StringBuilder sb = new StringBuilder();
            sb.Append("merchantRefId=TID");
            sb.Append(request.TID);
            sb.Append("&accountNumber=");
            sb.Append(request.mBeneDetail.AccountNo);
            sb.Append("&ifscCode=");
            sb.Append(request.mBeneDetail.IFSC);

            var headers = new Dictionary<string, string>
            {
                { "authorization", "Bearer "+ TOKEN },
                { "payoutMerchantId", appSetting.PAYOUTID },
                { "Content-Type", ContentType.x_wwww_from_urlencoded }
            };
            string URL = appSetting.BASEURL + "payment/verifyAccount";

            string resp = string.Empty;
            try
            {
                resp = AppWebRequest.O.HWRPost(URL, sb.ToString(), headers);
                if (resp != null)
                {
                    var apiResp = JsonConvert.DeserializeObject<PayUVerifyAccountResponse>(resp);

                    if (apiResp.status != null)
                    {
                        if (apiResp.status == 0)
                        {
                            if (apiResp.data.accountExists == "YES")
                            {
                                res.LiveID = apiResp.data.beneficiaryName;
                                res.Statuscode = RechargeRespType.SUCCESS;
                                res.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                                res.ErrorCode = ErrorCodes.Transaction_Successful;
                                res.BeneName = apiResp.data.beneficiaryName;
                            }
                            else
                            {
                                res.Statuscode = RechargeRespType.FAILED;
                                res.Msg = apiResp.msg.Contains("suff") ? ErrorCodes.Down : apiResp.msg;
                                res.ErrorCode = ErrorCodes.Unknown_Error;
                                res.LiveID = res.Msg;
                            }

                        }
                        else
                        {
                            res.Statuscode = RechargeRespType.FAILED;
                            res.Msg = apiResp.msg.Contains("suff") ? ErrorCodes.Down : apiResp.msg;
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
            res.Request = URL + "?" + sb.ToString() + JsonConvert.SerializeObject(headers);
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
            TokenGeneration();
            var res = new DMRTransactionResponse
            {
                Statuscode = RechargeRespType.PENDING,
                Msg = RechargeRespType._PENDING,
                VendorID = "",
                LiveID = "",
                ErrorCode = ErrorCodes.Request_Accpeted
            };
            var beneficiaryModel = new BeneficiaryModel
            {
                ID = Convert.ToInt32(request.mBeneDetail.BeneID)
            };
            IProcedure _proc = new GetBeneficaryByID(_dal);
            if (!request.IsPayout)
            {
                beneficiaryModel = (BeneficiaryModel)_proc.Call(beneficiaryModel);
            }
            else
            {
                beneficiaryModel.MobileNo = request.mBeneDetail.MobileNo;
                beneficiaryModel.Name = request.mBeneDetail.BeneName;
            }

            var dataRequest = new List<PayUPayoutRequest> {
                new PayUPayoutRequest
                {
                    amount=request.Amount,
                    batchId=request.TransactionID,
                    beneficiaryAccountNumber= request.mBeneDetail.AccountNo,
                    beneficiaryEmail= string.Empty,
                    beneficiaryIfscCode= request.mBeneDetail.IFSC,
                    beneficiaryMobile= beneficiaryModel.MobileNo??string.Empty,
                    beneficiaryName= beneficiaryModel.Name??string.Empty,
                    merchantRefId="TID"+request.TID,
                    paymentType= request.TransMode,
                    purpose="Money transfer "+request.TID
                }
            };
            string URL = appSetting.BASEURL + "payment";

            var headers = new Dictionary<string, string>
            {
                { "authorization", "Bearer "+ TOKEN },
                { "payoutMerchantId", appSetting.PAYOUTID }
            };
            string apiResp = string.Empty;
            try
            {
                apiResp = AppWebRequest.O.PostJsonDataUsingHWRTLS(URL, dataRequest, headers).Result;
                if (apiResp != null)
                {
                    var payUResp = JsonConvert.DeserializeObject<PayUPayoutResponse>(apiResp);
                    if (payUResp.status != null)
                    {
                        if (payUResp.status == ErrorCodes.Minus1)
                        {
                            res.LiveID = payUResp.msg;
                            res.Statuscode = RechargeRespType.FAILED;
                            res.Msg = nameof(DMTErrorCodes.Transaction_Failed).Replace("_", " ");
                            res.ErrorCode = DMTErrorCodes.Transaction_Failed;
                            if (payUResp.data != null)
                            {
                                res.LiveID = payUResp.data[0].error;
                            }
                        }
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
                var tempres = LoopStatusCheck(request.TID, request.TransactionID, request.RequestMode, request.UserID, request.APIID).Result;
                res.Statuscode = tempres.Statuscode;
                res.LiveID = tempres.LiveID;
                res.VendorID = tempres.VendorID;
                res.Msg = tempres.Msg;
                res.ErrorCode = tempres.ErrorCode;
                res.TID = request.TID;
            }
            return res;
        }

        public DMRTransactionResponse GetTransactionStatus(int TID, string TransactionID, int RequestMode, int UserID, int APIID)
        {
            TokenGeneration();
            var res = new DMRTransactionResponse
            {
                Statuscode = RechargeRespType.PENDING,
                Msg = nameof(RechargeRespType.PENDING),
                ErrorCode = ErrorCodes.Request_Accpeted
            };
            var fromD = ConnectionStringHelper.O.ConvertTransactionIDTo_dd_MMM_yyyy(TransactionID);
            var dataRequest = new PayUStatusCheckRequest
            {
                merchantRefId = "TID" + TID.ToString(),
                from = Convert.ToDateTime(fromD).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                transferStatus = "QUEUED",
                batchId = TransactionID
            };
            StringBuilder sb = new StringBuilder();
            sb.Append(nameof(dataRequest.merchantRefId));
            sb.Append("=");
            sb.Append(dataRequest.merchantRefId);
            //sb.Append("&");
            //sb.Append(nameof(dataRequest.from));
            //sb.Append("=");
            //sb.Append(dataRequest.from);
            //sb.Append("&t=");
            //sb.Append(dataRequest.from);
            //sb.Append("&");
            //sb.Append(nameof(dataRequest.batchId));
            //sb.Append("=");
            //sb.Append(dataRequest.batchId);
            //sb.Append("&");
            //sb.Append(nameof(dataRequest.transferStatus));
            //sb.Append("=");
            //sb.Append(dataRequest.transferStatus);
            var headers = new Dictionary<string, string>
            {
                { "authorization", "Bearer "+ TOKEN },
                { "payoutMerchantId", appSetting.PAYOUTID },
                { "Content-Type", ContentType.x_wwww_from_urlencoded }
            };
            string URL = appSetting.BASEURL + "payment/listTransactions";
            string resp = string.Empty;
            try
            {
                resp = AppWebRequest.O.HWRPost(URL, sb.ToString(), headers);
                if (resp != null)
                {
                    var payUList = JsonConvert.DeserializeObject<PayUStatusReponse>(resp);
                    if (payUList != null)
                    {
                        if ((payUList.status ?? -2) == 0)
                        {
                            if (payUList.data != null)
                            {
                                if (payUList.data.transactionDetails != null)
                                {
                                    if (payUList.data.transactionDetails.Count > 0)
                                    {
                                        var tdetail = payUList.data.transactionDetails[0];
                                        if (tdetail.txnStatus == RechargeRespType._SUCCESS)
                                        {
                                            res.VendorID = tdetail.txnId;
                                            res.LiveID = tdetail.bankTransactionRefNo;
                                            res.Statuscode = RechargeRespType.SUCCESS;
                                            res.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                                            res.ErrorCode = ErrorCodes.Transaction_Successful;
                                            res.BeneName = tdetail.beneficiaryName;
                                        }
                                        if (tdetail.txnStatus == RechargeRespType._FAILED)
                                        {
                                            res.VendorID = tdetail.txnId;
                                            res.LiveID = tdetail.msg.Contains("suffi") ? ErrorCodes.Down : tdetail.msg;
                                            res.Statuscode = RechargeRespType.FAILED;
                                            res.Msg = nameof(ErrorCodes.Unknown_Error).Replace("_", " ");
                                            res.ErrorCode = ErrorCodes.Unknown_Error;
                                        }
                                    }
                                }
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
                Request = URL + "?" + sb.ToString() + JsonConvert.SerializeObject(headers),
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
        private async Task<DMRTransactionResponse> LoopStatusCheck(int TID, string TransactionID, int RequestMode, int UserID, int APIID)
        {
            var res = new DMRTransactionResponse
            {
                Statuscode = RechargeRespType.PENDING,
                Msg = nameof(ErrorCodes.Request_Accpeted)
            };
            int i = 0;
            while (i < 10)
            {
                i++;
                if (res.Statuscode == RechargeRespType.PENDING)
                {
                    await Task.Delay(10 * 1000).ConfigureAwait(false);
                    res = GetTransactionStatus(TID, TransactionID, RequestMode, UserID, APIID);
                    if (res.Statuscode != RechargeRespType.PENDING)
                    {
                        i = 10;
                    }
                }
                else
                {
                    i = 10;
                }
            }
            return res;
        }
    }
}
