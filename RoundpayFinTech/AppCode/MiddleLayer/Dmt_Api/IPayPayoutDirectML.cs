using Fintech.AppCode.Configuration;
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
using RoundpayFinTech.AppCode.ThirdParty.Instantpay;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.MiddleLayer.Dmt_Api
{
    public class IPayPayoutDirectML : IMoneyTransferAPIML
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        private readonly IConfiguration Configuration;
        private readonly IConnectionConfiguration _c;
        private readonly InstantPayAppSetting appSetting;
        private readonly IDAL _dal;

        public IPayPayoutDirectML(IHttpContextAccessor accessor, IHostingEnvironment env, IDAL dal)
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
        public IPayPayoutDirectML(IHttpContextAccessor accessor, IHostingEnvironment env, IDAL dal,string APICode)
        {
            _accessor = accessor;
            _env = env;
            var builder = new ConfigurationBuilder()
                 .SetBasePath(Directory.GetCurrentDirectory());
            builder.AddJsonFile((_env.IsProduction() ? "appsettings.json" : "appsettings.Development.json"));
            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
            _dal = dal;
            appSetting = AppSetting(APICode);
        }
        private InstantPayAppSetting AppSetting()
        {
            var setting = new InstantPayAppSetting();
            try
            {
                setting = new InstantPayAppSetting
                {
                    BaseURL = Configuration["DMR:IPAY:BaseURL"],
                    Token = Configuration["DMR:IPAY:Token"]
                };
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "InstantPayAppSetting",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser
                });
            }
            return setting;
        }
        private InstantPayAppSetting AppSetting(string _APICode)
        {
            var setting = new InstantPayAppSetting();
            try
            {
                setting = new InstantPayAppSetting
                {
                    Token = Configuration["DMR:" + _APICode + ":Token"],
                    BaseURL = Configuration["DMR:" + _APICode + ":BaseURL"]
                };
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "InstantPayAppSetting",
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
            var apiRequest = new IPayPayoutRequest
            {
                token = appSetting.Token,
                request = new IPayPayoutRequestHelper
                {
                    sp_key = "DPN",
                    external_ref = "TID" + request.TID,
                    credit_account = request.mBeneDetail.AccountNo,
                    credit_amount = "1",
                    credit_rmn = string.Empty,
                    ifs_code = request.mBeneDetail.IFSC,
                    bene_name = request.mBeneDetail.BeneName ?? string.Empty,
                    upi_mode = string.Empty,
                    vpa = string.Empty,
                    latitude = (string.IsNullOrEmpty(request.Lattitude) ? "26.8678" : request.Lattitude).PadRight(7,'0'),
                    longitude = (string.IsNullOrEmpty(request.Longitude) ? "80.9832" : request.Longitude).PadRight(7,'0'),
                    endpoint_ip = request.IPAddress,
                    otp_auth = "0",
                    otp = string.Empty,
                    remarks = "verification",
                    alert_mobile = request.SenderMobile,
                    alert_email = request.EmailID
                }
            };
            string URL = appSetting.BaseURL + "ws/payouts/direct";
            string resp = string.Empty;
            try
            {
                resp = AppWebRequest.O.PostJsonDataUsingHWR(URL, apiRequest);
                if (resp != null)
                {
                    var apiResp = JsonConvert.DeserializeObject<IPayPayoutResponse>(resp);
                    if (apiResp.statuscode != null)
                    {
                        if (apiResp.statuscode.Equals("TXN"))
                        {
                            if (apiResp.data != null)
                            {
                                if (apiResp.data.payout != null)
                                {
                                    res.LiveID = apiResp.data.payout.credit_refid.ToString();
                                    res.Statuscode = RechargeRespType.SUCCESS;
                                    res.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                                    res.ErrorCode = ErrorCodes.Transaction_Successful;
                                    res.BeneName = apiResp.data.payout.name;
                                }
                                res.VendorID = apiResp.data.ipay_id;
                            }
                        }
                        else if (apiResp.statuscode.In("ERR", "SPD", "IAT", "SPE"))
                        {
                            res.Statuscode = RechargeRespType.FAILED;
                            res.Msg = apiResp.status.Contains("suff") ? ErrorCodes.Down : apiResp.status;
                            res.ErrorCode = ErrorCodes.Unknown_Error;
                            res.LiveID = res.Msg;
                        }
                        else
                        {
                            res.Statuscode = RechargeRespType.PENDING;
                            res.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                            res.ErrorCode = ErrorCodes.Transaction_Successful;
                            if (apiResp.data != null)
                            {
                                if (apiResp.data.payout != null)
                                {
                                    res.LiveID = apiResp.data.payout.credit_refid.ToString();
                                    res.BeneName = apiResp.data.payout.name;
                                }
                                res.VendorID = apiResp.data.ipay_id;
                            }
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
            res.Request = URL + "?" + JsonConvert.SerializeObject(apiRequest);
            var dMTReq = new DMTReqRes
            {
                APIID = request.APIID,
                Method = "VerifyAccount",
                RequestModeID = request.RequestMode,
                Request = res.Request,
                Response = resp ?? string.Empty,
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
            beneficiaryModel = (BeneficiaryModel)_proc.Call(beneficiaryModel);

            var apiRequest = new IPayPayoutRequest
            {
                token = appSetting.Token,
                request = new IPayPayoutRequestHelper
                {
                    sp_key = request.TransMode.Equals("IMPS") ? "DPN" : "BPN",
                    external_ref = "TID" + request.TID,
                    credit_account = request.mBeneDetail.AccountNo,
                    credit_amount = request.Amount.ToString(),
                    credit_rmn = string.Empty,
                    ifs_code = request.mBeneDetail.IFSC,
                    bene_name = beneficiaryModel.Name ?? string.Empty,
                    upi_mode = string.Empty,
                    vpa = string.Empty,
                    latitude = (string.IsNullOrEmpty(request.Lattitude) ? "26.8678" : request.Lattitude).PadRight(7,'0'),
                    longitude = (string.IsNullOrEmpty(request.Longitude) ? "80.9832" : request.Longitude).PadRight(7,'0'),
                    endpoint_ip = request.IPAddress,
                    otp_auth = "0",
                    otp = string.Empty,
                    remarks = "payout",
                    alert_mobile = request.SenderMobile,
                    alert_email = request.EmailID
                }
            };

            string URL = appSetting.BaseURL + "ws/payouts/direct";
            string resp = string.Empty;
            try
            {
                resp = AppWebRequest.O.PostJsonDataUsingHWR(URL, apiRequest);
                if (resp != null)
                {
                    var apiResp = JsonConvert.DeserializeObject<IPayPayoutResponse>(resp);
                    if (apiResp.statuscode != null)
                    {
                        if (apiResp.statuscode.Equals("TXN"))
                        {
                            if (apiResp.data != null)
                            {
                                if (apiResp.data.payout != null)
                                {
                                    res.LiveID = apiResp.data.payout.credit_refid.ToString();
                                    res.Statuscode = RechargeRespType.SUCCESS;
                                    res.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                                    res.ErrorCode = ErrorCodes.Transaction_Successful;
                                    res.BeneName = apiResp.data.payout.name;
                                }
                                res.VendorID = apiResp.data.ipay_id;
                            }
                        }
                        else if (apiResp.statuscode.In("ERR", "SPD", "IAT","SPE"))
                        {
                            res.Statuscode = RechargeRespType.FAILED;
                            res.Msg = apiResp.status.Contains("suff") ? ErrorCodes.Down : apiResp.status;
                            res.ErrorCode = ErrorCodes.Unknown_Error;
                            res.LiveID = res.Msg;
                        }
                        else
                        {
                            res.Statuscode = RechargeRespType.PENDING;
                            res.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                            res.ErrorCode = ErrorCodes.Transaction_Successful;
                            if (apiResp.data != null)
                            {
                                if (apiResp.data.payout != null)
                                {
                                    res.LiveID = apiResp.data.payout.credit_refid.ToString();
                                    res.BeneName = apiResp.data.payout.name;
                                }
                                res.VendorID = apiResp.data.ipay_id;
                            }


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
                    FuncName = "AccountTransfer",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = request.UserID
                });
            }
            res.Request = URL + "?" + JsonConvert.SerializeObject(apiRequest);
            var dMTReq = new DMTReqRes
            {
                APIID = request.APIID,
                Method = "AccountTransfer",
                RequestModeID = request.RequestMode,
                Request = res.Request,
                Response = resp ?? string.Empty,
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
            var res = new DMRTransactionResponse
            {
                Statuscode = RechargeRespType.PENDING,
                Msg = nameof(RechargeRespType.PENDING),
                ErrorCode = ErrorCodes.Request_Accpeted
            };
            var fromD = ConnectionStringHelper.O.ConvertTransactionIDTo_dd_MMM_yyyy(TransactionID);

            var dataRequest = new
            {
                token = appSetting.Token,
                request = new
                {
                    external_id = "TID" + TID,
                    transaction_date = Convert.ToDateTime(fromD).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
                }
            };
            string URL = appSetting.BaseURL + "ws/status/checkbyexternalid";
            string resp = string.Empty;
            try
            {
                resp = AppWebRequest.O.PostJsonDataUsingHWR(URL, dataRequest);
                if (resp != null)
                {
                    var ipaystsResp = JsonConvert.DeserializeObject<IPayStatusCheckExternalResponse>(resp);
                    if (ipaystsResp != null)
                    {
                        if (ipaystsResp.statuscode.Equals("TXN"))
                        {
                            if (ipaystsResp.data != null)
                            {
                                res.VendorID = ipaystsResp.data.order_id;
                                res.LiveID = ipaystsResp.data.serviceprovider_id;
                                if (ipaystsResp.data.additional_details != null)
                                {
                                    res.BeneName = ipaystsResp.data.additional_details.beneficiary_name;
                                }
                                if (ipaystsResp.data.transaction_status.Equals(RechargeRespType._SUCCESS))
                                {
                                    res.Statuscode = RechargeRespType.SUCCESS;
                                    res.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                                    res.ErrorCode = ErrorCodes.Transaction_Successful;
                                }
                                if (ipaystsResp.data.transaction_status.In(RechargeRespType._FAILED,RechargeRespType._REFUND))
                                {
                                    res.LiveID = ipaystsResp.data.transaction_description.Contains("suffi") ? ErrorCodes.Down : ipaystsResp.data.transaction_description;
                                    res.Statuscode = RechargeRespType.FAILED;
                                    res.Msg = nameof(ErrorCodes.Unknown_Error).Replace("_", " ");
                                    res.ErrorCode = ErrorCodes.Unknown_Error;
                                }
                            }
                        }
                        else if (ipaystsResp.statuscode.In("ERR", "SPD", "IAT", "SPE"))
                        {
                            res.Statuscode = RechargeRespType.FAILED;
                            res.Msg = ipaystsResp.status.Contains("suff") ? ErrorCodes.Down : ipaystsResp.status;
                            res.ErrorCode = ErrorCodes.Unknown_Error;
                            res.LiveID = res.Msg;
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
                Request = URL + "?" + JsonConvert.SerializeObject(dataRequest),
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

        public DMRTransactionResponse UPIAccountTransfer(MTAPIRequest request)
        {
            var res = new DMRTransactionResponse
            {
                Statuscode = RechargeRespType.PENDING,
                Msg = RechargeRespType._PENDING,
                VendorID = "",
                LiveID = "",
                ErrorCode = ErrorCodes.Request_Accpeted
            };


            var apiRequest = new IPayPayoutRequest
            {
                token = appSetting.Token,
                request = new IPayPayoutRequestHelper
                {
                    sp_key = "PPN",
                    external_ref = "TID" + request.TID,
                    credit_account = string.Empty,
                    credit_amount = request.Amount.ToString(),
                    credit_rmn = string.Empty,
                    ifs_code = string.Empty,
                    bene_name = request.mBeneDetail.BeneName ?? string.Empty,
                    upi_mode = "VPA",
                    vpa = request.mBeneDetail.AccountNo,
                    latitude = (string.IsNullOrEmpty(request.Lattitude) ? "26.8678" : request.Lattitude).PadRight(7, '0'),
                    longitude = (string.IsNullOrEmpty(request.Longitude) ? "80.9832" : request.Longitude).PadRight(7, '0'),
                    endpoint_ip = request.IPAddress,
                    otp_auth = "0",
                    otp = string.Empty,
                    remarks = ("UPIPayment")
                }
            };

            string URL = appSetting.BaseURL + "ws/payouts/direct";
            string resp = string.Empty;
            try
            {
                resp = AppWebRequest.O.PostJsonDataUsingHWR(URL, apiRequest);
                if (resp != null)
                {
                    var apiResp = JsonConvert.DeserializeObject<IPayPayoutResponse>(resp);
                    if (apiResp.statuscode != null)
                    {
                        if (apiResp.statuscode.Equals("TXN"))
                        {
                            if (apiResp.data != null)
                            {
                                if (apiResp.data.payout != null)
                                {
                                    res.LiveID = apiResp.data.payout.credit_refid.ToString();
                                    res.Statuscode = RechargeRespType.SUCCESS;
                                    res.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                                    res.ErrorCode = ErrorCodes.Transaction_Successful;
                                    res.BeneName = apiResp.data.payout.name;
                                }
                                res.VendorID = apiResp.data.ipay_id;
                            }
                        }
                        else if (apiResp.statuscode.In("ERR", "SPD", "IAT", "SPE"))
                        {
                            res.Statuscode = RechargeRespType.FAILED;
                            res.Msg = apiResp.status.Contains("suff") ? ErrorCodes.Down : apiResp.status;
                            res.ErrorCode = ErrorCodes.Unknown_Error;
                            res.LiveID = res.Msg;
                        }

                        else
                        {
                            res.Statuscode = RechargeRespType.PENDING;
                            res.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                            res.ErrorCode = ErrorCodes.Transaction_Successful;
                            if (apiResp.data != null)
                            {
                                if (apiResp.data.payout != null)
                                {
                                    res.LiveID = apiResp.data.payout.credit_refid.ToString();
                                    res.BeneName = apiResp.data.payout.name;
                                }
                                res.VendorID = apiResp.data.ipay_id;
                            }


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
                    FuncName = "UPIAccountTransfer",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = request.UserID
                });
            }
            res.Request = URL + "?" + JsonConvert.SerializeObject(apiRequest);
            var dMTReq = new DMTReqRes
            {
                APIID = request.APIID,
                Method = "UPIAccountTransfer",
                RequestModeID = request.RequestMode,
                Request = res.Request,
                Response = resp ?? string.Empty,
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
    }
}
