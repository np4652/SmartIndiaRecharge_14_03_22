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
using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace RoundpayFinTech.AppCode.MiddleLayer.Dmt_Api
{
    public class FintechPayoutML : IMoneyTransferAPIML
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        private readonly IConfiguration Configuration;
        private readonly IConnectionConfiguration _c;
        private readonly FintechAPISetting appSetting;
        private readonly IDAL _dal;
        private readonly string _APICode;
        private readonly int _APIID;

        public FintechPayoutML(IHttpContextAccessor accessor, IHostingEnvironment env, IDAL dal, string APICode)
        {
            _accessor = accessor;
            _env = env;
            var builder = new ConfigurationBuilder()
                 .SetBasePath(Directory.GetCurrentDirectory());
            builder.AddJsonFile((_env.IsProduction() ? "appsettings.json" : "appsettings.Development.json"));
            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
            _dal = dal;
            _APICode = APICode;
            appSetting = AppSetting();
        }
        public FintechAPISetting AppSetting()
        {
            var setting = new FintechAPISetting();
            try
            {
                setting = new FintechAPISetting
                {
                    BaseURL = Configuration["DMR:" + _APICode + ":BaseURL"],
                    UserID = Convert.ToInt32(Configuration["DMR:" + _APICode + ":UserID"]),
                    Token = Configuration["DMR:" + _APICode + ":Token"],
                    PIN = Configuration["DMR:" + _APICode + ":PIN"],
                    HYPTO_VerificationURL = Configuration["DMR:" + _APICode + ":HYPTO_VerificationURL"],
                    HYPTO_VerifyAuth = Configuration["DMR:" + _APICode + ":HYPTO_VerifyAuth"],
                    OutletID = Convert.ToInt32(Configuration["DMR:" + _APICode + ":OutletID"]),
                };
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "FintechAPISetting:" + (_APICode ?? string.Empty),
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return setting;
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
                if (string.IsNullOrWhiteSpace(senderRequest.Name) || senderRequest._VerifyStatus == 0)
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
                        CommonInt2 = request.APIID
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
            var res = new DMRTransactionResponse
            {
                Statuscode = RechargeRespType.PENDING,
                Msg = RechargeRespType._PENDING,
                VendorID = "",
                LiveID = "",
                ErrorCode = ErrorCodes.Request_Accpeted
            };
            var dataRequest = new
            {
                appSetting.UserID,
                appSetting.Token,
                appSetting.OutletID,
                PayoutRequest = new
                {
                    request.mBeneDetail.AccountNo,
                    AmountR = request.Amount,
                    request.mBeneDetail.BankID,
                    Bank=request.mBeneDetail.BankName,
                    request.mBeneDetail.IFSC,
                    request.SenderMobile,
                    request.mBeneDetail.BeneName,
                    BeneMobile = request.mBeneDetail.MobileNo,
                    APIRequestID = request.TID,
                    SPKey = request.TransMode
                }
            };

            string URL = appSetting.BaseURL + "API/Payout/VerifyBeneficiary";
            string apiResp = string.Empty;
            try
            {
                apiResp = AppWebRequest.O.PostJsonDataUsingHWRTLS(URL, dataRequest).Result;
                if (apiResp != null)
                {
                    var fintechResp = JsonConvert.DeserializeObject<DMTTransactionResponse>(apiResp);
                    if (fintechResp.Statuscode != null)
                    {
                        if (fintechResp.Statuscode == ErrorCodes.Minus1)
                        {
                            res.LiveID = (fintechResp.Message ?? string.Empty).Contains("suff") ? nameof(DMTErrorCodes.Declined_by_ServiceProvider).Replace("_", " ") : fintechResp.Message;
                            res.Statuscode = RechargeRespType.FAILED;
                            res.Msg = nameof(DMTErrorCodes.Transaction_Failed).Replace("_", " ");
                            res.ErrorCode = DMTErrorCodes.Transaction_Failed;
                        }
                        else
                        {
                            if (fintechResp.Status == RechargeRespType.FAILED)
                            {
                                res.LiveID = (fintechResp.Message ?? string.Empty).Contains("suff") ? nameof(DMTErrorCodes.Declined_by_ServiceProvider).Replace("_", " ") : fintechResp.Message;
                                res.Statuscode = RechargeRespType.FAILED;
                                res.Msg = nameof(DMTErrorCodes.Transaction_Failed).Replace("_", " ");
                                res.ErrorCode = DMTErrorCodes.Transaction_Failed;
                            }
                            else 
                            {
                                res.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                                res.ErrorCode = ErrorCodes.Transaction_Successful;
                            }
                            res.LiveID = fintechResp.LiveID;
                            res.VendorID = fintechResp.RPID;
                            res.Statuscode = fintechResp.Status;                            
                            res.BeneName = fintechResp.BeneName;
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
                    FuncName = "VerifyAccount",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = request.UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            var dMTReq = new DMTReqRes
            {
                APIID = request.APIID,
                Method = "VerifyAccount",
                RequestModeID = request.RequestMode,
                Request = URL + JsonConvert.SerializeObject(dataRequest),
                Response = apiResp ?? string.Empty,
                SenderNo = request.SenderMobile,
                UserID = request.UserID,
                TID = request.TID.ToString()
            };
            new ProcUpdateLogDMRReqResp(_dal).Call(dMTReq);
            res.Request = dMTReq.Request;
            res.Response = dMTReq.Response;

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
                beneficiaryModel.BankID = request.mBeneDetail.BankID;
            }
            var dataRequest = new
            {
                appSetting.UserID,
                appSetting.Token,
                appSetting.OutletID,
                PayoutRequest = new
                {
                    request.mBeneDetail.AccountNo,
                    AmountR = request.Amount,
                    beneficiaryModel.BankID,
                    request.mBeneDetail.IFSC,
                    request.SenderMobile,
                    SenderName = request.UserName,
                    SenderEmail = request.EmailID,
                    BeneName=beneficiaryModel.Name,
                    BeneMobile = beneficiaryModel.MobileNo,
                    APIRequestID = request.TID,
                    SPKey = request.TransMode
                }
            };

            string URL = appSetting.BaseURL + "API/Payout";
            string apiResp = string.Empty;
            try
            {
                apiResp = AppWebRequest.O.PostJsonDataUsingHWRTLS(URL, dataRequest).Result;
                if (apiResp != null)
                {
                    var fintechResp = JsonConvert.DeserializeObject<DMTTransactionResponse>(apiResp);
                    if (fintechResp.Statuscode != null)
                    {
                        if (fintechResp.Statuscode == ErrorCodes.Minus1)
                        {
                            res.LiveID = (fintechResp.Message ?? string.Empty).Contains("suff") ? nameof(DMTErrorCodes.Declined_by_ServiceProvider).Replace("_", " ") : fintechResp.Message;
                            res.Statuscode = RechargeRespType.FAILED;
                            res.Msg = nameof(DMTErrorCodes.Transaction_Failed).Replace("_", " ");
                            res.ErrorCode = DMTErrorCodes.Transaction_Failed;
                        }
                        else
                        {
                            res.LiveID = fintechResp.LiveID;
                            res.VendorID = fintechResp.RPID;
                            res.Statuscode = fintechResp.Status;
                            res.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                            res.ErrorCode = ErrorCodes.Transaction_Successful;
                            res.BeneName = fintechResp.BeneName;
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
                Request = URL + JsonConvert.SerializeObject(dataRequest),
                Response = apiResp ?? string.Empty,
                SenderNo = request.SenderMobile,
                UserID = request.UserID,
                TID = request.TID.ToString()
            };
            new ProcUpdateLogDMRReqResp(_dal).Call(dMTReq);
            res.Request = dMTReq.Request;
            res.Response = dMTReq.Response;
            res.TID = request.TID;
            return res;
        }

        public DMRTransactionResponse GetTransactionStatus(int TID, string TransactionID, int RequestMode, int UserID, int APIID, string VendorID)
        {
            var res = new DMRTransactionResponse
            {
                Statuscode = RechargeRespType.PENDING,
                Msg = nameof(RechargeRespType.PENDING),
                ErrorCode = ErrorCodes.Request_Accpeted
            };
            var fromD = ConnectionStringHelper.O.ConvertTransactionIDTo_dd_MMM_yyyy(TransactionID);

            StringBuilder sb = new StringBuilder();
            sb.Append("UserID=");
            sb.Append(appSetting.UserID);
            sb.Append("&Token=");
            sb.Append(appSetting.Token);
            sb.Append("&RPID=");
            sb.Append(VendorID);
            sb.Append("&AgentID=");
            sb.Append(TransactionID);
            sb.Append("&Optional1=");
            sb.Append(Convert.ToDateTime(fromD).ToString("dd MMM yyyy", CultureInfo.InvariantCulture));

            string URL = appSetting.BaseURL + "API/TransactionStatusCheck?" + sb.ToString();
            string resp = string.Empty;
            try
            {
                resp = AppWebRequest.O.CallUsingWebClient_GET(URL);
                if (resp != null)
                {
                    var rpFintechSts = JsonConvert.DeserializeObject<DMTCheckStatusResponse>(resp);
                    if (rpFintechSts != null)
                    {
                        if (rpFintechSts.Status > 0)
                        {
                            if (rpFintechSts.Status == RechargeRespType.SUCCESS)
                            {
                                res.VendorID = rpFintechSts.TransactionID;
                                res.LiveID = rpFintechSts.LiveID;
                                res.Statuscode = RechargeRespType.SUCCESS;
                                res.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                                res.ErrorCode = ErrorCodes.Transaction_Successful;
                                res.BeneName = rpFintechSts.BeneName;
                            }
                            if (rpFintechSts.Status == RechargeRespType.FAILED)
                            {
                                res.VendorID = rpFintechSts.TransactionID;
                                res.LiveID = rpFintechSts.Message.Contains("suffi") ? ErrorCodes.Down : rpFintechSts.Message;
                                res.Statuscode = RechargeRespType.FAILED;
                                res.Msg = nameof(ErrorCodes.Unknown_Error).Replace("_", " ");
                                res.ErrorCode = ErrorCodes.Unknown_Error;
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
                Request = URL ,
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

        public DMRTransactionResponse DoUPIPaymentService(MTAPIRequest request) {
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
                beneficiaryModel.BankID = request.mBeneDetail.BankID;
            }
            var dataRequest = new
            {
                appSetting.UserID,
                appSetting.Token,
                BeneAccountNumber= request.mBeneDetail.AccountNo,
                request.Amount,
                BeneName= request.mBeneDetail.BeneName,
                APIRequestID= request.TID
            };

            string URL = appSetting.BaseURL + "API/DoUPIPayment";
            string apiResp = string.Empty;
            try
            {
                apiResp = AppWebRequest.O.PostJsonDataUsingHWRTLS(URL, dataRequest).Result;
                if (apiResp != null)
                {
                    var fintechResp = JsonConvert.DeserializeObject<DMTTransactionResponse>(apiResp);
                    if (fintechResp.Statuscode != null)
                    {
                        if (fintechResp.Statuscode == ErrorCodes.Minus1)
                        {
                            res.LiveID = (fintechResp.Message ?? string.Empty).Contains("suff") ? nameof(DMTErrorCodes.Declined_by_ServiceProvider).Replace("_", " ") : fintechResp.Message;
                            res.Statuscode = RechargeRespType.FAILED;
                            res.Msg = nameof(DMTErrorCodes.Transaction_Failed).Replace("_", " ");
                            res.ErrorCode = DMTErrorCodes.Transaction_Failed;
                        }
                        else
                        {
                            res.LiveID = fintechResp.LiveID;
                            res.VendorID = fintechResp.RPID;
                            res.Statuscode = fintechResp.Status;
                            res.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                            res.ErrorCode = ErrorCodes.Transaction_Successful;
                            res.BeneName = fintechResp.BeneName;
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
                    FuncName = "DoUPIPaymentService",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = request.UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            var dMTReq = new DMTReqRes
            {
                APIID = request.APIID,
                Method = "DoUPIPaymentService",
                RequestModeID = request.RequestMode,
                Request = URL + JsonConvert.SerializeObject(dataRequest),
                Response = apiResp ?? string.Empty,
                SenderNo = request.SenderMobile,
                UserID = request.UserID,
                TID = request.TID.ToString()
            };
            new ProcUpdateLogDMRReqResp(_dal).Call(dMTReq);
            res.Request = dMTReq.Request;
            res.Response = dMTReq.Response;
            res.TID = request.TID;
            return res;
        }

    }
}