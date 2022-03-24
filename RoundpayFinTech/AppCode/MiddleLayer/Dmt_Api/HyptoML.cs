using Fintech.AppCode.DB;
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
using RoundpayFinTech.AppCode.ThirdParty.Hypto;
using System;
using System.Collections.Generic;
using System.IO;


namespace RoundpayFinTech.AppCode.MiddleLayer.Dmt_Api
{
    public class HyptoML : IMoneyTransferAPIML
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        private readonly IConfiguration Configuration;
        private readonly int _APIID;
        private readonly IDAL _dal;
        private readonly HyptoAppSetting apiSetting;

        public HyptoML(IHttpContextAccessor accessor, IHostingEnvironment env, int APIID, IDAL dal)
        {
            _APIID = APIID;
            _accessor = accessor;
            _env = env;
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory());
            builder.AddJsonFile((_env.IsProduction() ? "appsettings.json" : "appsettings.Development.json"));
            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
            _dal = dal;
            apiSetting = AppSetting();
        }

        public HyptoAppSetting AppSetting()
        {
            var setting = new HyptoAppSetting();
            try
            {
                setting = new HyptoAppSetting
                {
                    BaseURL = Configuration["DMR:HYPTO:BaseURL"],
                    Auth = Configuration["DMR:HYPTO:Auth"]
                };
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "HyptoAppSetting",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser
                });
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
                LiveID = ""
            };

            var param = new
            {
                number = request.mBeneDetail.AccountNo,
                ifsc = request.mBeneDetail.IFSC,
                reference_number = request.TransactionID
            };

            var _URL = apiSetting.BaseURL + "verify/bank_account";
            string response = string.Empty;
            try
            {
                response = AppWebRequest.O.PostJsonDataUsingHWRTLS(_URL, param, new Dictionary<string, string>
                {
                    { "Authorization",apiSetting.Auth},
                    { ContentType.Self,ContentType.application_json}
                }).Result;
                if (!string.IsNullOrEmpty(response))
                {
                    var _apiRes = JsonConvert.DeserializeObject<HyptoResponseModel>(response);
                    if (_apiRes != null)
                    {
                        if (_apiRes.success)
                        {
                            if (_apiRes.data != null)
                            {
                                res.VendorID = _apiRes.data.reference_number;
                                if (_apiRes.data.status.Equals("COMPLETED"))
                                {
                                    res.Statuscode = RechargeRespType.SUCCESS;
                                    res.Msg = _apiRes.message;
                                    res.BeneName = _apiRes.data.verify_account_holder ?? string.Empty;
                                    res.LiveID = _apiRes.data.bank_ref_num.ToString();
                                    res.ErrorCode = ErrorCodes.Transaction_Successful;
                                }
                                else if (_apiRes.data.status.Equals("FAILED"))
                                {
                                    res.Statuscode = RechargeRespType.FAILED;
                                    res.Msg = _apiRes.data.verify_reason ?? string.Empty;
                                    if (res.Msg.Contains("suff"))
                                    {
                                        res.ErrorCode = DMTErrorCodes.Declined_by_ServiceProvider;
                                        res.Msg = nameof(DMTErrorCodes.Declined_by_ServiceProvider).Replace("_", " ");
                                    }
                                    else
                                    {
                                        res.Msg = string.Format("{0}", res.Msg);
                                        res.ErrorCode = 158;
                                    }
                                    res.LiveID = res.Msg;
                                }
                                else
                                {
                                    res.Statuscode = RechargeRespType.PENDING;
                                    res.Msg = nameof(ErrorCodes.Request_Accpeted).Replace("_", " ");
                                    res.ErrorCode = ErrorCodes.Request_Accpeted;
                                    res.LiveID = res.Msg;
                                }
                            }
                            else
                            {
                                res.Statuscode = RechargeRespType.PENDING;
                                res.Msg = _apiRes.message;
                            }
                        }
                        else
                        {
                            IErrorCodeML errorCodeML = new ErrorCodeML(_accessor, _env, false);
                            var eFromDB = errorCodeML.GetAPIErrorCodeDescription(request.APIGroupCode, _apiRes.message);
                            if (!string.IsNullOrEmpty(eFromDB.Code))
                            {
                                res.Statuscode = eFromDB.Status;
                                res.Msg = eFromDB.Error.Replace("{REPLACE}", _apiRes.message);
                                res.ErrorCode = Convert.ToInt32(eFromDB.Code.Trim());
                                res.LiveID = res.Msg;
                            }
                            res.Statuscode = res.Statuscode == 0 ? RechargeRespType.PENDING : res.Statuscode;
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
                    FuncName = "VerifyAccount",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = request.UserID
                });
            }
            res.Request = _URL + "&RequestJson=" + JsonConvert.SerializeObject(param);
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
                ErrorCode = ErrorCodes.Request_Accpeted
            };
            var param = new
            {
                amount = request.Amount,
                payment_type = "IMPS",
                ifsc = request.mBeneDetail.IFSC,
                number = request.mBeneDetail.AccountNo,
                note = "Fund Transfer",
                udf1 = "",
                udf2 = "",
                udf3 = "",
                reference_number = request.TransactionID,
                connected_banking = false
            };

            var _URL = apiSetting.BaseURL + "transfers/initiate";
            string response = string.Empty;

            try
            {
                response = AppWebRequest.O.PostJsonDataUsingHWRTLS(_URL, param, new Dictionary<string, string>
                {
                    { "Authorization",apiSetting.Auth},
                    { ContentType.Self,ContentType.application_json}
                }).Result;

                if (!string.IsNullOrEmpty(response))
                {
                    var _apiRes = JsonConvert.DeserializeObject<HyptoResponseModel>(response);
                    if (_apiRes != null)
                    {
                        if (_apiRes.success)
                        {
                            if (_apiRes.data != null)
                            {
                                res.VendorID = _apiRes.data.id.ToString();
                                if (_apiRes.data.status.Equals("COMPLETED"))
                                {
                                    res.Statuscode = RechargeRespType.SUCCESS;
                                    res.Msg = _apiRes.message;
                                    res.LiveID = _apiRes.data.bank_ref_num.ToString();
                                    res.ErrorCode = ErrorCodes.Transaction_Successful;
                                }
                                else if (_apiRes.data.status.Equals("FAILED"))
                                {
                                    res.Statuscode = RechargeRespType.FAILED;
                                    res.Msg = _apiRes.data.verify_reason ?? string.Empty;
                                    if (res.Msg.Contains("suff"))
                                    {
                                        res.ErrorCode = DMTErrorCodes.Declined_by_ServiceProvider;
                                        res.Msg = nameof(DMTErrorCodes.Declined_by_ServiceProvider).Replace("_", " ");
                                    }
                                    else
                                    {
                                        res.Msg = string.Format("{0}", res.Msg);
                                        res.ErrorCode = 158;
                                    }
                                    res.LiveID = res.Msg;
                                }
                                else if (_apiRes.data.status.Equals("PENDING"))
                                {
                                    res.Statuscode = RechargeRespType.PENDING;
                                    res.Msg = nameof(ErrorCodes.Request_Accpeted).Replace("_", " ");
                                    res.ErrorCode = ErrorCodes.Request_Accpeted;
                                    res.LiveID = res.Msg;
                                }
                                else
                                {
                                    res.Statuscode = RechargeRespType.PENDING;
                                    res.Msg = nameof(ErrorCodes.Request_Accpeted).Replace("_", " ");
                                    res.ErrorCode = ErrorCodes.Request_Accpeted;
                                    res.LiveID = res.Msg;
                                }
                            }
                            else
                            {
                                res.Statuscode = RechargeRespType.PENDING;
                                res.Msg = _apiRes.message;
                            }
                        }
                        else
                        {
                            IErrorCodeML errorCodeML = new ErrorCodeML(_accessor, _env, false);
                            var eFromDB = errorCodeML.GetAPIErrorCodeDescription(request.APIGroupCode, _apiRes.message);
                            if (!string.IsNullOrEmpty(eFromDB.Code))
                            {
                                res.Statuscode = eFromDB.Status;
                                res.Msg = eFromDB.Error.Replace("{REPLACE}", _apiRes.message);
                                res.ErrorCode = Convert.ToInt32(eFromDB.Code.Trim());
                                res.LiveID = res.Msg;
                            }
                            res.Statuscode = res.Statuscode == 0 ? RechargeRespType.PENDING : res.Statuscode;
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                response = " Exception:" + ex.Message + " | " + response;
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
                Request = _URL + JsonConvert.SerializeObject(param),
                Response = response ?? string.Empty,
                SenderNo = request.SenderMobile,
                UserID = request.UserID,
                TID = request.TID.ToString()
            };
            new ProcUpdateLogDMRReqResp(_dal).Call(dMTReq);
            res.Request = dMTReq.Request;
            res.Response = dMTReq.Response;
            return res;
        }
        public DMRTransactionResponse GetTransactionStatus(int TID, string TransactionID, string VendorID, int RequestMode, int UserID, int APIID, string APIGroupCode = "")
        {
            var res = new DMRTransactionResponse
            {
                Statuscode = RechargeRespType.PENDING,
                Msg = nameof(RechargeRespType.PENDING),
                ErrorCode = ErrorCodes.Request_Accpeted
            };
            var _URL = apiSetting.BaseURL + "transfers/status/" + TransactionID;
            string response = string.Empty;
            try
            {
                response = AppWebRequest.O.CallUsingHttpWebRequest_GETAsync(_URL, new Dictionary<string, string>
                {
                    { "Authorization",apiSetting.Auth},
                    { ContentType.Self,ContentType.application_json}
                }).Result;
                if (response != null)
                {
                    //var cFStsChkResp = JsonConvert.DeserializeObject<CFStsChkResp>(response);
                    //if (cFStsChkResp != null)
                    //{
                    //    if (cFStsChkResp.data.transfer != null)
                    //    {
                    //        if (cFStsChkResp.data.transfer.status.Equals("REJECTED"))
                    //        {
                    //            res.VendorID = VendorID;
                    //            res.LiveID = cFStsChkResp.data.transfer.reason.Contains("suffi") ? ErrorCodes.Down : cFStsChkResp.data.transfer.reason;
                    //            res.Statuscode = RechargeRespType.FAILED;
                    //            res.Msg = nameof(ErrorCodes.Unknown_Error).Replace("_", " ");
                    //            res.ErrorCode = ErrorCodes.Unknown_Error;
                    //        }
                    //        else if (cFStsChkResp.data.transfer.status.Equals("SUCCESS"))
                    //        {
                    //            res.VendorID = VendorID;
                    //            res.LiveID = string.Empty;//cFStsChkResp.data.transfer;
                    //            res.Statuscode = RechargeRespType.SUCCESS;
                    //            res.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                    //            res.ErrorCode = ErrorCodes.Transaction_Successful;
                    //        }
                    //        else
                    //        {
                    //            res.Statuscode = RechargeRespType.FAILED;
                    //            res.Msg = nameof(ErrorCodes.Unknown_Error).Replace("_", " ");
                    //            res.ErrorCode = ErrorCodes.Unknown_Error;
                    //        }
                    //    }
                    //}
                }
            }
            catch (Exception ex)
            {
                response = " Exception:" + ex.Message + " | " + response;
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
                Request = _URL,
                Response = response,
                SenderNo = string.Empty,
                UserID = UserID,
                TID = TID.ToString()
            };
            new ProcUpdateLogDMRReqResp(_dal).Call(dMTReq);
            res.Request = dMTReq.Request;
            res.Response = response;
            return res;
        }
    }
}