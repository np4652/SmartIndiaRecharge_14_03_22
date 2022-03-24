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
    public class SecurePaymentML : IMoneyTransferAPIML
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        private readonly IConfiguration Configuration;
        private readonly IConnectionConfiguration _c;
        private readonly SPAPISetting appSetting;
        private readonly IDAL _dal;
        private readonly string _APICode;
        private readonly int _APIID;
        private readonly IRequestInfo _rinfo;

        public SecurePaymentML(IHttpContextAccessor accessor, IHostingEnvironment env, IDAL dal, string APICode)
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
            _rinfo = new RequestInfo(_accessor, _env);
        }
        public SPAPISetting AppSetting()
        {
            var setting = new SPAPISetting();
            try
            {
                setting = new SPAPISetting
                {
                    BaseURL = Configuration["DMR:" + _APICode + ":BaseURL"],
                    Token = Configuration["DMR:" + _APICode + ":Token"]
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
                        CommonInt2 = request.APIID
                    }).Result;
                    res.RemainingLimit = senderLimit.SenderLimit - senderLimit.LimitUsed;
                    res.AvailbleLimit = senderLimit.SenderLimit;
                    res.Statuscode = ErrorCodes.One;
                    res.Msg = ErrorCodes.DMTSLS;
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
            var res = new DMRTransactionResponse
            {
                Statuscode = RechargeRespType.PENDING,
                Msg = RechargeRespType._PENDING,
                VendorID = "",
                LiveID = "",
                ErrorCode = ErrorCodes.Request_Accpeted
            };
            var dataRequest = new SQPModelReq
            {
                token = appSetting.Token,
                account = request.Amount.ToString(),
                ifsc = request.mBeneDetail.IFSC,
                apitxnid = request.TID.ToString()
            };

            string URL = appSetting.BaseURL + "payout/beneverify";
            string apiResp = string.Empty;
            try
            {
                apiResp = AppWebRequest.O.PostJsonDataUsingHWR(URL, dataRequest);
                if (apiResp != null)
                {
                    var sqrResp = JsonConvert.DeserializeObject<SQPModelResp>(apiResp);
                    if (sqrResp.status != null)
                    {
                        if (sqrResp.status.Equals("ERR"))
                        {
                            res.LiveID = (sqrResp.message ?? string.Empty).Contains("suff") ? nameof(DMTErrorCodes.Declined_by_ServiceProvider).Replace("_", " ") : sqrResp.message;
                            res.Statuscode = RechargeRespType.FAILED;
                            res.Msg = nameof(DMTErrorCodes.Transaction_Failed).Replace("_", " ");
                            res.ErrorCode = DMTErrorCodes.Transaction_Failed;
                        }
                        else if (sqrResp.status.Equals("TXN"))
                        {
                            res.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                            res.ErrorCode = ErrorCodes.Transaction_Successful;
                            res.LiveID = sqrResp.rrn;
                            //res.VendorID = fintechResp.RPID;
                            res.Statuscode = RechargeRespType.SUCCESS;
                            res.BeneName = sqrResp.name;
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

            var dataRequest = new SQPModelReq
            {
                token = appSetting.Token,
                amount = request.Amount.ToString(),
                name = request.mBeneDetail.BeneName,
                account = request.mBeneDetail.AccountNo,
                ifsc = request.mBeneDetail.IFSC,
                bank = request.mBeneDetail.BankName,
                apitxnid = "TID" + request.TID.ToString(),
                callback = "http://" + request.WebsiteName + "/Callback/" + request.APIID,
                ip = request.IPAddress
            };

            string URL = appSetting.BaseURL + "payout";
            string apiResp = string.Empty;
            try
            {
                apiResp = AppWebRequest.O.PostJsonDataUsingHWR(URL, dataRequest);
                if (apiResp != null)
                {
                    var spqResp = JsonConvert.DeserializeObject<SQPModelResp>(apiResp);
                    if (spqResp != null)
                    {
                        if (spqResp.status.Equals("ERR"))
                        {
                            res.LiveID = (spqResp.message ?? string.Empty).Contains("suff") ? nameof(DMTErrorCodes.Declined_by_ServiceProvider).Replace("_", " ") : spqResp.message;
                            res.Statuscode = RechargeRespType.FAILED;
                            res.Msg = nameof(DMTErrorCodes.Transaction_Failed).Replace("_", " ");
                            res.ErrorCode = DMTErrorCodes.Transaction_Failed;
                        }
                        else if (spqResp.status.Equals("TXN") && spqResp.message.Equals("Transaction Successfull"))
                        {
                            res.LiveID = "";//spqResp.LiveID;
                            res.VendorID = "";// spqResp.RPID;
                            res.Statuscode = 1;// spqResp.Status;
                            res.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                            res.ErrorCode = ErrorCodes.Transaction_Successful;
                            res.BeneName = "";// spqResp.BeneName;
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
                TID = "TID" + request.TID.ToString()
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

            var dataRequest = new SQPModelReq
            {
                token = appSetting.Token,
                txnid = "TID" + TID.ToString(),
            };

            string URL = appSetting.BaseURL + "status";
            string resp = string.Empty;
            try
            {
                resp = AppWebRequest.O.PostJsonDataUsingHWR(URL, dataRequest);
                if (resp != null)
                {
                    var sqrResp = JsonConvert.DeserializeObject<SQPModelResp>(resp);
                    if (sqrResp != null)
                    {
                        if (sqrResp.status != null)
                        {
                            if (sqrResp.status.Equals("TXN") && sqrResp.trans_status.Equals("success"))
                            {
                                res.VendorID = "";// sqrResp.TransactionID;
                                res.LiveID = "";// sqrResp.LiveID;
                                res.Statuscode = RechargeRespType.SUCCESS;
                                res.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                                res.ErrorCode = ErrorCodes.Transaction_Successful;
                                res.BeneName = "";// sqrResp.BeneName;
                            }
                            if (sqrResp.status.Equals("ERR"))
                            {
                                res.VendorID = "";// sqrResp.TransactionID;
                                res.LiveID = "";// sqrResp.Message.Contains("suffi") ? ErrorCodes.Down : sqrResp.Message;
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
                Request = URL,
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
}