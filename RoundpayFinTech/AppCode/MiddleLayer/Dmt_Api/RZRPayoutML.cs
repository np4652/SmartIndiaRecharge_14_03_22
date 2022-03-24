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
using RoundpayFinTech.AppCode.Configuration;
using RoundpayFinTech.AppCode.DL;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.MoneyTransfer;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.ThirdParty.Razorpay;
using RoundpayFinTech.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.MiddleLayer.Dmt_Api
{
    public class RZRPayoutML : IMoneyTransferAPIML
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        private readonly IConfiguration Configuration;
        private readonly IConnectionConfiguration _c;
        private readonly RZRPayoutAppSetting appSetting;
        private readonly IDAL _dal;
        public RZRPayoutML(IHttpContextAccessor accessor, IHostingEnvironment env, IDAL dal, string apiCode)
        {
            _accessor = accessor;
            _env = env;
            var builder = new ConfigurationBuilder()
                 .SetBasePath(Directory.GetCurrentDirectory());
            builder.AddJsonFile((_env.IsProduction() ? "appsettings.json" : "appsettings.Development.json"));
            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
            _dal = dal;
            appSetting = AppSetting(apiCode);
        }
        private RZRPayoutAppSetting AppSetting(string apiCode)
        {
            var setting = new RZRPayoutAppSetting();
            try
            {
                setting = new RZRPayoutAppSetting
                {
                    BaseURL = Configuration["DMR:" + apiCode + ":BaseURL"],
                    KeyID = Configuration["DMR:" + apiCode + ":KeyID"],
                    SecretID = Configuration["DMR:" + apiCode + ":SecretID"],
                    AccountNumber = Configuration["DMR:" + apiCode + ":AccountNumber"],
                    HYPTO_VerificationURL = Configuration["DMR:" + apiCode + ":HYPTO_VerificationURL"],
                    HYPTO_VerifyAuth = Configuration["DMR:" + apiCode + ":HYPTO_VerifyAuth"],
                };
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "RZRPayoutAppSetting",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser
                });
            }
            return setting;
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
            var res = new MSenderCreateResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = nameof(DMTErrorCodes.Beneficiary_not_found).Replace("_", " "),
                ErrorCode = DMTErrorCodes.Beneficiary_not_found
            };
            string URL = string.Format("{0}{1}{2}", appSetting.BaseURL, "fund_accounts/" + request.mBeneDetail.BeneID);
            var resp = string.Empty;
            string authString = string.Format("{0}:{1}", appSetting.KeyID, appSetting.SecretID);
            var headers = new Dictionary<string, string>
            {
               { "authorization", "Bearer "+ Convert.ToBase64String(Encoding.UTF8.GetBytes(authString)) }
            };
            var dataRequest = new
            {
                active = true
            };
            try
            {
                resp = AppWebRequest.O.PatchJsonDataUsingHWRTLS(URL, dataRequest, headers).Result;
                if (resp != null)
                {
                    var apiResp = JsonConvert.DeserializeObject<RZRPayFundAccountEntity>(resp);
                    if (apiResp != null)
                    {
                        if (apiResp.active)
                        {
                            res.Statuscode = -2;
                            res.Msg = "Beneficiary Updated Successfully";
                            res.ErrorCode = ErrorCodes.Transaction_Successful;
                        }
                    }
                    else
                    {
                        res.Msg = ErrorCodes.NODATA;
                    }
                }
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "GenerateOTP",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = request.UserID
                });
            }
            new ProcUpdateLogDMRReqResp(_dal).Call(new DMTReqRes
            {
                APIID = request.APIID,
                Method = "GenerateOTP",
                RequestModeID = request.RequestMode,
                Request = string.Format("{0}?{1}", URL, JsonConvert.SerializeObject(headers)),
                Response = resp,
                SenderNo = request.SenderMobile,
                UserID = request.UserID,
                TID = request.TransactionID
            });
            return res;
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
            res = DMTAPIHelperML.AccountVerification(appSetting.HYPTO_VerificationURL, appSetting.HYPTO_VerifyAuth, request.mBeneDetail.AccountNo, request.mBeneDetail.IFSC, request.TID.ToString(), _dal);
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
            var beneficiaryModel = new BeneficiaryModel
            {
                ID = Convert.ToInt32(request.mBeneDetail.BeneID)
            };
            var senderRequest = new SenderRequest();
            IProcedure _proc = new GetBeneficaryByID(_dal);
            if (request.IsPayout)
            {
                beneficiaryModel.Name = request.mBeneDetail.BeneName;
                senderRequest.MobileNo = request.SenderMobile;
                senderRequest.Name = request.UserName;
            }
            else
            {
                beneficiaryModel = (BeneficiaryModel)_proc.Call(beneficiaryModel);
                senderRequest = (SenderRequest)new ProcGetSender(_dal).Call(request.SenderMobile);
            }
            res.SenderName = senderRequest.Name;
            string URL = appSetting.BaseURL + "payouts";
            var dataRequest = new object();
            if ((request.TransMode ?? string.Empty) == nameof(PaymentMode_.UPI))
            {
                dataRequest = new
                {
                    account_number = appSetting.AccountNumber ?? string.Empty,
                    amount = request.Amount * 100,
                    currency = "INR",
                    mode = request.TransMode,
                    purpose = "payout",
                    queue_if_low_balance = ApplicationSetting.IsRPOnly ? true : false,
                    reference_id = "TID" + request.TID,
                    narration = request.SenderMobile + " " + request.TID,
                    fund_account = new
                    {
                        account_type = "vpa",
                        vpa = new
                        {
                            address = request.mBeneDetail.AccountNo
                        },
                        contact = new
                        {
                            name = senderRequest.Name.Trim().Length < 4 ? string.Format("{0} {1}", senderRequest.Name.Trim().PadLeft(2, 'S'), senderRequest.Name.Trim().PadLeft(2, 'S')) : senderRequest.Name.Trim(),
                            email = request.EmailID,
                            contact = request.SenderMobile,
                            type = "customer",
                            reference_id = "TID" + request.TID
                        }
                    }
                };
            }
            else
            {
                dataRequest = new
                {
                    account_number = appSetting.AccountNumber ?? string.Empty,
                    amount = request.Amount * 100,
                    currency = "INR",
                    mode = request.TransMode,
                    purpose = "payout",
                    queue_if_low_balance = ApplicationSetting.IsRPOnly ? true : false,
                    reference_id = "TID" + request.TID,
                    narration = request.SenderMobile + " " + request.TID,
                    fund_account = new
                    {
                        account_type = "bank_account",
                        bank_account = new
                        {
                            name = beneficiaryModel.Name.Trim().Length < 4 ? string.Format("{0} {1}", beneficiaryModel.Name.Trim().PadLeft(2, 'B'), beneficiaryModel.Name.Trim().PadLeft(2, 'B')) : beneficiaryModel.Name.Trim(),
                            ifsc = (request.mBeneDetail.IFSC ?? string.Empty).Trim(),
                            account_number = request.mBeneDetail.AccountNo
                        },
                        contact = new
                        {
                            name = senderRequest.Name.Trim().Length < 4 ? string.Format("{0} {1}", senderRequest.Name.Trim().PadLeft(2, 'S'), senderRequest.Name.Trim().PadLeft(2, 'S')) : senderRequest.Name.Trim(),
                            email = request.EmailID,
                            contact = request.SenderMobile,
                            type = "customer",
                            reference_id = "TID" + request.TID
                        }
                    }
                };
            }

            string apiResp = string.Empty;
            string authString = string.Format("{0}:{1}", appSetting.KeyID, appSetting.SecretID);
            var headers = new Dictionary<string, string>
            {
               { "authorization", "Basic "+ Convert.ToBase64String(Encoding.UTF8.GetBytes(authString)) }
            };
            try
            {
                apiResp = AppWebRequest.O.PostJsonDataUsingHWRTLS(URL, dataRequest, headers).Result;
                if (apiResp != null)
                {
                    var rzrPayResp = JsonConvert.DeserializeObject<RZRPayoutDirectResponse>(apiResp);
                    res.VendorID = rzrPayResp.id;
                    res.LiveID = rzrPayResp.utr;
                    if (rzrPayResp.status.Equals("processed"))
                    {
                        res.Statuscode = RechargeRespType.SUCCESS;
                        res.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                        res.ErrorCode = ErrorCodes.Transaction_Successful;
                    }
                    else if (rzrPayResp.status.In("rejected", "cancelled", "reversed", "failed"))
                    {
                        res.Statuscode = RechargeRespType.FAILED;
                        res.Msg = rzrPayResp.failure_reason;
                        res.ErrorCode = ErrorCodes.Transaction_Replace;
                        res.LiveID = res.Msg;
                        if (rzrPayResp.failure_reason.Contains("nsuffici"))
                        {
                            res.Msg = nameof(ErrorCodes.Unknown_Error).Replace("_", " ");
                            res.ErrorCode = ErrorCodes.Unknown_Error;
                            res.LiveID = res.Msg;
                        }
                    }
                    else
                    {
                        res.Statuscode = RechargeRespType.PENDING;
                        res.Msg = nameof(ErrorCodes.Request_Accpeted).Replace("_", " ");
                        res.ErrorCode = ErrorCodes.Request_Accpeted;
                    }
                }
            }
            catch (Exception ex)
            {
                var resError = ErrorResponse(apiResp);
                if (resError.Statuscode == ErrorCodes.Minus1)
                {
                    IErrorCodeML errorCodeML = new ErrorCodeML(_accessor, _env, false);
                    var eFromDB = errorCodeML.GetAPIErrorCodeDescription(request.APIGroupCode, resError.Msg);
                    if (!string.IsNullOrEmpty(eFromDB.Code))
                    {
                        res.Statuscode = eFromDB.Status;
                        res.Msg = eFromDB.Error.Replace("{REPLACE}", resError.Msg);
                        res.ErrorCode = Convert.ToInt32(eFromDB.Code.Trim());
                        res.LiveID = res.Msg;
                    }
                    res.Statuscode = res.Statuscode == 0 ? RechargeRespType.PENDING : res.Statuscode;
                }
                else
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
            }
            var dMTReq = new DMTReqRes
            {
                APIID = request.APIID,
                Method = "AccountTransfer",
                RequestModeID = request.RequestMode,
                Request = string.Format("{0}?{1}|{2}", URL, JsonConvert.SerializeObject(dataRequest), JsonConvert.SerializeObject(headers)),
                Response = apiResp ?? string.Empty,
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
        private ResponseStatus ErrorResponse(string apiRes)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.One,
                Msg = "No Error"
            };
            try
            {
                var APIRes = JsonConvert.DeserializeObject<RZRPayError>(apiRes);
                if (APIRes.error != null)
                {
                    res.Statuscode = ErrorCodes.Minus1;
                    res.Msg = APIRes.error.description;
                }
            }
            catch (Exception)
            {

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
        public DMRTransactionResponse GetTransactionStatus(int TID, string TransactionID, int RequestMode, int UserID, int APIID, string VendorID, string APIGroupCode, string VendorID2)
        {
            var res = new DMRTransactionResponse
            {
                Statuscode = RechargeRespType.PENDING,
                Msg = nameof(RechargeRespType.PENDING),
                ErrorCode = ErrorCodes.Request_Accpeted,
                VendorID = VendorID
            };

            string authString = string.Format("{0}:{1}", appSetting.KeyID, appSetting.SecretID);
            var headers = new Dictionary<string, string>
            {
               { "authorization", "Basic "+ Convert.ToBase64String(Encoding.UTF8.GetBytes(authString)) }
            };
            string URL = string.IsNullOrEmpty(VendorID2) ? string.Format("{0}{1}{2}", appSetting.BaseURL, "payouts/", VendorID) : string.Format((appSetting.BaseURL + "payouts?account_number={0}&reference_id={1}"), VendorID2, ("TID" + TID));
            string resp = string.Empty;
            try
            {
                resp = AppWebRequest.O.CallUsingHttpWebRequest_GETAsync(URL, headers).Result;
                if (resp != null)
                {
                    if (string.IsNullOrEmpty(VendorID2))
                    {
                        var rzrPayResp = JsonConvert.DeserializeObject<RZRPayoutDirectResponse>(resp);
                        res.VendorID = rzrPayResp.id;
                        res.LiveID = rzrPayResp.utr;
                        if (rzrPayResp.status.Equals("processed"))
                        {
                            res.Statuscode = RechargeRespType.SUCCESS;
                            res.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                            res.ErrorCode = ErrorCodes.Transaction_Successful;
                        }
                        else if (rzrPayResp.status.In("rejected", "cancelled", "reversed", "failed"))
                        {
                            res.Statuscode = RechargeRespType.FAILED;
                            res.Msg = rzrPayResp.failure_reason;
                            res.ErrorCode = ErrorCodes.Transaction_Replace;
                            res.LiveID = res.Msg;
                            if (rzrPayResp.failure_reason.Contains("nsuffici"))
                            {
                                res.Msg = nameof(ErrorCodes.Unknown_Error).Replace("_", " ");
                                res.ErrorCode = ErrorCodes.Unknown_Error;
                                res.LiveID = res.Msg;
                            }
                        }
                    }
                    else
                    {
                        var rzrPayResp = JsonConvert.DeserializeObject<RZRVendorID2Response>(resp);
                        if (rzrPayResp.items != null)
                        {
                            if (rzrPayResp.items.Count > 0)
                            {
                                res.VendorID = rzrPayResp.items[0].id;
                                res.LiveID = rzrPayResp.items[0].utr;
                                if (rzrPayResp.items[0].status.Equals("processed"))
                                {
                                    res.Statuscode = RechargeRespType.SUCCESS;
                                    res.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                                    res.ErrorCode = ErrorCodes.Transaction_Successful;
                                }
                                else if (rzrPayResp.items[0].status.In("rejected", "cancelled", "reversed", "failed"))
                                {
                                    res.Statuscode = RechargeRespType.FAILED;
                                    res.Msg = rzrPayResp.items[0].failure_reason ?? string.Empty;
                                    res.ErrorCode = ErrorCodes.Transaction_Replace;
                                    res.LiveID = res.Msg;
                                    if ((rzrPayResp.items[0].failure_reason ?? string.Empty).Contains("nsuffici"))
                                    {
                                        res.Msg = nameof(ErrorCodes.Unknown_Error).Replace("_", " ");
                                        res.ErrorCode = ErrorCodes.Unknown_Error;
                                        res.LiveID = res.Msg;
                                    }
                                }
                            }
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                var resError = ErrorResponse(resp);
                if (resError.Statuscode == ErrorCodes.Minus1)
                {
                    IErrorCodeML errorCodeML = new ErrorCodeML(_accessor, _env, false);
                    var eFromDB = errorCodeML.GetAPIErrorCodeDescription(APIGroupCode, resError.Msg);
                    if (!string.IsNullOrEmpty(eFromDB.Code))
                    {
                        res.Statuscode = eFromDB.Status;
                        res.Msg = eFromDB.Error.Replace("{REPLACE}", resError.Msg);
                        res.ErrorCode = Convert.ToInt32(eFromDB.Code.Trim());
                        res.LiveID = res.Msg;
                    }
                    res.Statuscode = res.Statuscode == 0 ? RechargeRespType.PENDING : res.Statuscode;
                }
                else
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

            }
            var dMTReq = new DMTReqRes
            {
                APIID = APIID,
                Method = "GetTransactionStatus",
                RequestModeID = RequestMode,
                Request = string.Format("{0}?{1}", URL, JsonConvert.SerializeObject(headers)),
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
                    res = GetTransactionStatus(TID, TransactionID, RMode, UserID, APIID, VendorID, APIGroupCode, "");
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

    }
}