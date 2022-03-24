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
using RoundpayFinTech.AppCode.HelperClass;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.MoneyTransfer;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.ThirdParty.FINO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.MiddleLayer.Dmt_Api
{
    public class FINOML : IMoneyTransferAPIML
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        private readonly IDAL _dal;
        private readonly IConfiguration Configuration;
        private readonly FINOAppSetting appSetting;
        public FINOML(IHttpContextAccessor accessor, IHostingEnvironment env, IDAL dal)
        {
            _accessor = accessor;
            _env = env;
            _dal = dal;
            var builder = new ConfigurationBuilder()
                 .SetBasePath(Directory.GetCurrentDirectory());
            builder.AddJsonFile((_env.IsProduction() ? "appsettings.json" : "appsettings.Development.json"));
            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
            appSetting = AppSetting();
        }
        private FINOAppSetting AppSetting()
        {
            var res = new FINOAppSetting { };
            try
            {
                res.BaseURL = Configuration["DMR:FINO:BaseURL"];
                res.HeaderKey = Configuration["DMR:FINO:HeaderKey"];
                res.BodyKey = Configuration["DMR:FINO:BodyKey"];
                res.ClientId = Configuration["DMR:FINO:ClientId"];
                res.AuthKey = Configuration["DMR:FINO:AuthKey"];
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "FINOSetting",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser
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

        public MSenderCreateResp GenerateOTP(MTAPIRequest request)
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
        public MSenderCreateResp SenderKYC(MTAPIRequest request)
        {
            throw new NotImplementedException();
        }

        public MSenderCreateResp SenderResendOTP(MTAPIRequest request)
        {
            throw new NotImplementedException();
        }
        public MSenderCreateResp SenderEKYC(MTAPIRequest request)
        {
            throw new NotImplementedException();
        }


        public MSenderLoginResponse ValidateBeneficiary(MTAPIRequest request)
        {
            throw new NotImplementedException();
        }
        public MBeneficiaryResp GetBeneficiary(MTAPIRequest request)
        {
            return DMTAPIHelperML.GetBeneficiary(request, _dal, GetType().Name);
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
            var senderRequest = (SenderRequest)new ProcGetSender(_dal).Call(request.SenderMobile);
            var dataRequest = new FINOTransferRequest
            {
                Amount = 1,
                BeneAccountNo = request.mBeneDetail.AccountNo,
                BeneIFSCCode = request.mBeneDetail.IFSC,
                BeneName = (beneficiaryModel.Name ?? "") == "" ? "Bene Name" : beneficiaryModel.Name,
                CustomerMobileNo = request.SenderMobile,
                CustomerName = senderRequest.Name,
                RFU1 = "Verify Account " + request.TID,
                ClientUniqueID = "TID" + request.TID
            };
            string URL = appSetting.BaseURL + "IMPSRequest";
            res.Request = URL;
            var headerRequest = new FINOHeaderRequest
            {
                ClientId = appSetting.ClientId,
                AuthKey = appSetting.AuthKey
            };
            string apiResp = string.Empty;
            res.Request = res.Request + "?H:" + JsonConvert.SerializeObject(headerRequest);
            string Authentication = AESEncryption.OpenSSLEncrypt(JsonConvert.SerializeObject(headerRequest), appSetting.HeaderKey);

            var headers = new Dictionary<string, string>
            {
                { "Authentication", Authentication }
            };
            var encryptedBody = AESEncryption.OpenSSLEncrypt(JsonConvert.SerializeObject(dataRequest), appSetting.BodyKey);
            res.Request = res.Request + "H:" + JsonConvert.SerializeObject(headerRequest) + JsonConvert.SerializeObject(headers) + "B:" + JsonConvert.SerializeObject(dataRequest) + encryptedBody;
            try
            {

                apiResp = AppWebRequest.O.PostJsonDataUsingHWRTLS(URL, encryptedBody, headers).Result;
                res.Response = apiResp;
                if (apiResp != null)
                {
                    var finoResp = JsonConvert.DeserializeObject<FINOTransferResponse>(apiResp);
                    if (finoResp != null)
                    {
                        if (finoResp.ResponseCode != null)
                        {
                            res.VendorID = finoResp.RequestID;
                            if (finoResp.ResponseData != null)
                            {
                                var ResponseData = AESEncryption.OpenSSLDecrypt(finoResp.ResponseData, appSetting.BodyKey);
                                if (ResponseData != null)
                                {
                                    res.Response = res.Response + ResponseData;
                                    var finoTrnsferDetail = JsonConvert.DeserializeObject<FINOTrnsferDetail>(ResponseData);
                                    if (finoTrnsferDetail != null)
                                    {
                                        if ((finoTrnsferDetail.ActCode ?? "act") == "0")
                                        {
                                            res.LiveID = finoTrnsferDetail.TxnID;
                                            res.Statuscode = RechargeRespType.SUCCESS;
                                            res.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                                            res.ErrorCode = ErrorCodes.Transaction_Successful;
                                            res.BeneName = finoTrnsferDetail.BeneName;
                                        }
                                        else
                                        {
                                            IErrorCodeML errorCodeML = new ErrorCodeML(_accessor, _env, false);
                                            var eFromDB = errorCodeML.GetAPIErrorCodeDescription(request.APIGroupCode, finoTrnsferDetail.ActCode);
                                            if (!string.IsNullOrEmpty(eFromDB.Code))
                                            {
                                                res.Statuscode = eFromDB.Status;
                                                //res.Msg = eFromDB.Error.Replace("{REPLACE}", finoTrnsferDetail.TxnDescription);
                                                res.Msg = eFromDB.Error.Replace("{REPLACE}", finoTrnsferDetail.MessageString);
                                                res.ErrorCode = Convert.ToInt32(eFromDB.Code.Trim());
                                                res.LiveID = res.Msg;
                                            }
                                            res.Statuscode = res.Statuscode == 0 ? RechargeRespType.PENDING : res.Statuscode;
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
                apiResp = " Exception:" + ex.Message + " | " + apiResp;
                res.Response = apiResp;
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "VerifyAccount",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = request.UserID
                });
            }
            var dMTReq = new DMTReqRes
            {
                APIID = request.APIID,
                Method = "VerifyAccount",
                RequestModeID = request.RequestMode,
                Request = res.Request,
                Response = res.Response,
                SenderNo = request.SenderMobile,
                UserID = request.UserID,
                TID = request.TID.ToString()
            };
            new ProcUpdateLogDMRReqResp(_dal).Call(dMTReq);

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
            }
            var senderRequest = (SenderRequest)new ProcGetSender(_dal).Call(request.SenderMobile);
            var dataRequest = new FINOTransferRequest
            {
                Amount = request.Amount,
                BeneAccountNo = request.mBeneDetail.AccountNo,
                BeneIFSCCode = request.mBeneDetail.IFSC,
                BeneName = (beneficiaryModel.Name ?? "") == "" ? "Bene Name" : beneficiaryModel.Name,
                CustomerMobileNo = request.SenderMobile,
                CustomerName = senderRequest.Name,
                RFU1 = "Money transfer " + request.TID,
                ClientUniqueID = "TID" + request.TID
            };
            string URL = appSetting.BaseURL + (request.TransMode == "NEFT" ? "NEFTRequest" : "IMPSRequest");
            res.Request = URL;
            var headerRequest = new FINOHeaderRequest
            {
                ClientId = appSetting.ClientId,
                AuthKey = appSetting.AuthKey
            };
            string apiResp = string.Empty;
            res.Request = res.Request + "?H:" + JsonConvert.SerializeObject(headerRequest);
            string Authentication = AESEncryption.OpenSSLEncrypt(JsonConvert.SerializeObject(headerRequest), appSetting.HeaderKey);

            var headers = new Dictionary<string, string>
            {
                { "Authentication", Authentication }
            };
            var encryptedBody = AESEncryption.OpenSSLEncrypt(JsonConvert.SerializeObject(dataRequest), appSetting.BodyKey);
            res.Request = res.Request + "H:" + JsonConvert.SerializeObject(headerRequest) + JsonConvert.SerializeObject(headers) + "B:" + JsonConvert.SerializeObject(dataRequest) + encryptedBody;
            try
            {

                apiResp = AppWebRequest.O.PostJsonDataUsingHWRTLS(URL, encryptedBody, headers).Result;
                res.Response = apiResp;
                if (apiResp != null)
                {
                    var finoResp = JsonConvert.DeserializeObject<FINOTransferResponse>(apiResp);
                    if (finoResp != null)
                    {
                        if (finoResp.ResponseCode != null)
                        {
                            res.VendorID = finoResp.RequestID;
                            if (finoResp.ResponseData != null)
                            {
                                var ResponseData = AESEncryption.OpenSSLDecrypt(finoResp.ResponseData, appSetting.BodyKey);
                                if (ResponseData != null)
                                {
                                    res.Response = res.Response + ResponseData;
                                    var finoTrnsferDetail = JsonConvert.DeserializeObject<FINOTrnsferDetail>(ResponseData);
                                    if (finoTrnsferDetail != null)
                                    {
                                        if ((finoTrnsferDetail.ActCode ?? "act") == "0")
                                        {
                                            res.LiveID = finoTrnsferDetail.TxnID;
                                            res.Statuscode = RechargeRespType.SUCCESS;
                                            res.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                                            res.ErrorCode = ErrorCodes.Transaction_Successful;
                                            res.BeneName = finoTrnsferDetail.BeneName;
                                        }
                                        else
                                        {
                                            IErrorCodeML errorCodeML = new ErrorCodeML(_accessor, _env, false);
                                            var eFromDB = errorCodeML.GetAPIErrorCodeDescription(request.APIGroupCode, finoTrnsferDetail.ActCode);
                                            if (!string.IsNullOrEmpty(eFromDB.Code))
                                            {
                                                res.Statuscode = eFromDB.Status;
                                                res.Msg = eFromDB.Error.Replace("{REPLACE}", finoTrnsferDetail.TxnDescription);
                                                res.ErrorCode = Convert.ToInt32(eFromDB.Code.Trim());
                                                res.LiveID = res.Msg;
                                            }
                                            res.Statuscode = res.Statuscode == 0 ? RechargeRespType.PENDING : res.Statuscode;
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
                apiResp = " Exception:" + ex.Message + " | " + apiResp;
                res.Response = apiResp;
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "AccountTransfer",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = request.UserID
                });
            }
            var dMTReq = new DMTReqRes
            {
                APIID = request.APIID,
                Method = "AccountTransfer",
                RequestModeID = request.RequestMode,
                Request = res.Request,
                Response = res.Response,
                SenderNo = request.SenderMobile,
                UserID = request.UserID,
                TID = request.TID.ToString()
            };
            new ProcUpdateLogDMRReqResp(_dal).Call(dMTReq);

            return res;
        }

        public DMRTransactionResponse GetTransactionStatus(int TID, int RequestMode, int UserID, int APIID, string APIGroupCode)
        {

            var res = new DMRTransactionResponse
            {
                Statuscode = RechargeRespType.PENDING,
                Msg = nameof(RechargeRespType.PENDING),
                ErrorCode = ErrorCodes.Request_Accpeted
            };
            var dataRequest = new
            {
                ClientUniqueID = "TID" + TID
            };
            string URL = appSetting.BaseURL + "txnstatusrequest";
            res.Request = URL;
            var headerRequest = new FINOHeaderRequest
            {
                ClientId = appSetting.ClientId,
                AuthKey = appSetting.AuthKey
            };
            string apiResp = string.Empty;
            
            string Authentication = AESEncryption.OpenSSLEncrypt(JsonConvert.SerializeObject(headerRequest), appSetting.HeaderKey);
            

            var headers = new Dictionary<string, string>
            {
                { "Authentication", Authentication }
            };
            res.Request = res.Request + "?H:" + JsonConvert.SerializeObject(headerRequest)+JsonConvert.SerializeObject(headers);
            var encryptedBody = AESEncryption.OpenSSLEncrypt(JsonConvert.SerializeObject(dataRequest), appSetting.BodyKey);
            res.Request = res.Request + "B:" + JsonConvert.SerializeObject(dataRequest)+ encryptedBody;
            try
            {

                apiResp = AppWebRequest.O.PostJsonDataUsingHWRTLS(URL, encryptedBody, headers).Result;
                res.Response = apiResp;
                if (apiResp != null)
                {
                    var finoResp = JsonConvert.DeserializeObject<FINOTransferResponse>(apiResp);
                    if (finoResp != null)
                    {
                        if (finoResp.ResponseCode != null)
                        {
                            res.VendorID = finoResp.RequestID;
                            if (finoResp.ResponseData != null)
                            {
                                var ResponseData = AESEncryption.OpenSSLDecrypt(finoResp.ResponseData, appSetting.BodyKey);
                                if (ResponseData != null)
                                {
                                    res.Response = res.Response + ResponseData;
                                    var finoTrnsferDetail = JsonConvert.DeserializeObject<FINOTrnsferDetail>(ResponseData);
                                    if (finoTrnsferDetail != null)
                                    {
                                        if ((finoTrnsferDetail.ActCode ?? "act") == "0")
                                        {
                                            res.LiveID = finoTrnsferDetail.TxnID;
                                            res.Statuscode = RechargeRespType.SUCCESS;
                                            res.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                                            res.ErrorCode = ErrorCodes.Transaction_Successful;
                                            res.BeneName = finoTrnsferDetail.BeneName;
                                        }
                                        else
                                        {
                                            IErrorCodeML errorCodeML = new ErrorCodeML(_accessor, _env, false);
                                            var eFromDB = errorCodeML.GetAPIErrorCodeDescription(APIGroupCode, finoTrnsferDetail.ActCode);
                                            if (!string.IsNullOrEmpty(eFromDB.Code))
                                            {
                                                res.Statuscode = eFromDB.Status;
                                                res.Msg = eFromDB.Error.Replace("{REPLACE}", finoTrnsferDetail.MessageString);
                                                res.ErrorCode = Convert.ToInt32(eFromDB.Code.Trim());
                                                res.LiveID = res.Msg;
                                            }
                                            res.Statuscode = res.Statuscode == 0 ? RechargeRespType.PENDING : res.Statuscode;
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
                apiResp = " Exception:" + ex.Message + " | " + apiResp;
                res.Response = apiResp;
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "GetTransactionStatus",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = UserID
                });
            }
            var dMTReq = new DMTReqRes
            {
                APIID = APIID,
                Method = "GetTransactionStatus",
                RequestModeID = RequestMode,
                Request = res.Request,
                Response = res.Response,
                SenderNo = string.Empty,
                UserID = UserID,
                TID = TID.ToString()
            };
            new ProcUpdateLogDMRReqResp(_dal).Call(dMTReq);

            return res;
        }
        public void GetBalance()
        {
            var res = new DMRTransactionResponse
            {
                Statuscode = RechargeRespType.PENDING,
                Msg = nameof(RechargeRespType.PENDING),
                ErrorCode = ErrorCodes.Request_Accpeted
            };
            string URL = appSetting.BaseURL + "GetPartnersBalanceRequest";
            res.Request = URL;
            var headerRequest = new FINOHeaderRequest
            {
                ClientId = appSetting.ClientId,
                AuthKey = appSetting.AuthKey
            };
            string apiResp = string.Empty;
            res.Request = res.Request + "?H:" + JsonConvert.SerializeObject(headerRequest);
            string Authentication = AESEncryption.OpenSSLEncrypt(JsonConvert.SerializeObject(headerRequest), appSetting.HeaderKey);

            var headers = new Dictionary<string, string>
            {
                { "Authentication", Authentication }
            };
            var encryptedBody = string.Empty;
            try
            {

                apiResp = AppWebRequest.O.PostJsonDataUsingHWRTLS(URL, encryptedBody, headers).Result;
                res.Response = apiResp;
                if (apiResp != null)
                {
                    var finoResp = JsonConvert.DeserializeObject<FINOTransferResponse>(apiResp);
                    if (finoResp != null)
                    {
                        if (finoResp.ResponseCode != null)
                        {
                            res.VendorID = finoResp.RequestID;
                            if (finoResp.ResponseData != null)
                            {
                                var ResponseData = AESEncryption.OpenSSLDecrypt(finoResp.ResponseData, appSetting.BodyKey);
                                if (ResponseData != null)
                                {
                                    var finoTrnsferDetail = JsonConvert.DeserializeObject<FinoBalance>(ResponseData);
                                    if (finoTrnsferDetail != null)
                                    {

                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                apiResp = " Exception:" + ex.Message + " | " + apiResp;
                res.Response = apiResp;
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "GetBalance",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = 1
                });
            }
        }

    }
}
