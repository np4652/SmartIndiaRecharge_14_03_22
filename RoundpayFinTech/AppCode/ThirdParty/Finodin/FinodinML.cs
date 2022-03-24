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
using RoundpayFinTech.AppCode.ThirdParty.Instantpay;
using System;
using System.Collections.Generic;
using System.IO;

namespace RoundpayFinTech.AppCode.MiddleLayer.Dmt_Api
{

    public partial class FinodinML : IMoneyTransferAPIML
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        private readonly IConfiguration Configuration;
        private readonly string _APICode;
        private readonly string _APIGroupCode;
        private string _FinodinToken;
        private readonly int _APIID;
        private readonly IDAL _dal;
        private readonly FinodinAppSetting apiSetting;
        private IErrorCodeML errorCodeML;

        public FinodinML(IHttpContextAccessor accessor, IHostingEnvironment env, IDAL dal, string APICode, int APIID, string APIGroupCode)
        {
            _APICode = APICode;
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
            _APIGroupCode = APIGroupCode;
            errorCodeML = new ErrorCodeML(_accessor, _env, false);
        }
        public FinodinAppSetting AppSetting()
        {
            var setting = new FinodinAppSetting();
            try
            {
                setting = new FinodinAppSetting
                {
                    BaseURL = Configuration["DMR:" + _APICode + ":BaseURL"],
                    username = Configuration["DMR:" + _APICode + ":username"],
                    password = Configuration["DMR:" + _APICode + ":password"]
                };
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "FinodinAppSetting:" + (_APICode ?? string.Empty),
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return setting;
        }
        private void FinodinLogin(int rMode, string senderMob, int userId, int tid)
        {
            _FinodinToken = string.Empty;
            var loginReq = new
            {
                username = apiSetting.username,
                password = apiSetting.password
            };
            var response = string.Empty;
            var _URL = apiSetting.BaseURL + "Login";
            try
            {
                response = AppWebRequest.O.PostJsonDataUsingHWR(_URL, loginReq);
                if (!string.IsNullOrEmpty(response))
                {
                    var _apiRes = JsonConvert.DeserializeObject<FinodinLoginResp>(response);
                    if (_apiRes != null)
                    {
                        if (_apiRes.statusCode == ErrorCodes.One)
                        {
                            _FinodinToken = _apiRes.token;
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
                    FuncName = "FinodinLogin:" + (_APICode ?? string.Empty),
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser
                });
            }
            new ProcUpdateLogDMRReqResp(_dal).Call(new DMTReqRes
            {
                APIID = _APIID,
                Method = "FinodinLogin",
                RequestModeID = rMode,
                Request = _URL + "|" + JsonConvert.SerializeObject(loginReq),
                Response = response,
                SenderNo = senderMob,
                UserID = userId,
                TID = tid.ToString()
            });
        }
        public MSenderCreateResp CreateSender(MTAPIRequest request)
        {
            FinodinLogin(request.RequestMode, request.SenderMobile, request.UserID, request.TID);
            var res = new MSenderCreateResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError,
                ErrorCode = ErrorCodes.Unknown_Error
            };
            var req = new
            {
                token = _FinodinToken,
                apiSource = request.APIOpCode,
                subAgentID = request.APIOutletID,
                mobile = request.SenderMobile,
                remName = request.FirstName + " " + request.LastName,
                address = request.Address,
                city = request.City,
                state = request.StateName,
                pinCode = request.Pincode
            };
            string response = string.Empty;
            var _URL = apiSetting.BaseURL + "RemitterRegister";
            try
            {
                response = AppWebRequest.O.PostJsonDataUsingHWR(_URL, req);
                if (!string.IsNullOrEmpty(response))
                {

                    var _apiRes = JsonConvert.DeserializeObject<FinodinResp>(response);
                    if (_apiRes != null)
                    {
                        if (_apiRes.statusCode == (int)FinodinErrCode.success && _apiRes.description.Contains("Please use OTP to validate remitter"))
                        {
                            res.Statuscode = ErrorCodes.One;
                            res.Msg = nameof(DMTErrorCodes.Partial_Registration_Please_verify_sender_with_OTP).Replace("_", " ");
                            res.ErrorCode = DMTErrorCodes.Partial_Registration_Please_verify_sender_with_OTP;
                            res.IsOTPGenerated = true;
                            res.IsOTPResendAvailble = true;
                        }
                        else if (_apiRes.statusCode == (int)FinodinErrCode.Error)
                        {
                            res.Statuscode = ErrorCodes.Minus1;
                            res.Msg = nameof(DMTErrorCodes.Transaction_Failed).Replace("_", " ");
                            res.ErrorCode = DMTErrorCodes.Transaction_Failed;
                        }
                        else
                        {
                            errorCodeML = new ErrorCodeML(_accessor, _env, false);
                            var eFromDB = errorCodeML.GetAPIErrorCodeDescription(_APIGroupCode, _apiRes.description);
                            if (!string.IsNullOrEmpty(eFromDB.Code))
                            {
                                res.Statuscode = eFromDB.Status;
                                res.Msg = eFromDB.Error.Replace("{REPLACE}", _apiRes.description);
                                res.ErrorCode = Convert.ToInt32(eFromDB.Code.Trim());
                            }
                            res.Statuscode = res.Statuscode == 0 ? RechargeRespType.PENDING : res.Statuscode;
                        }
                    }
                    else
                    {
                        res.Statuscode = ErrorCodes.Minus1;
                        res.Msg = nameof(DMTErrorCodes.Transaction_Failed).Replace("_", " ");
                        res.ErrorCode = DMTErrorCodes.Transaction_Failed;
                    }
                }
            }
            catch (Exception ex)
            {
                response = ex.Message + "|" + response;
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "CreateSender",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = 1
                });
            }
            new ProcUpdateLogDMRReqResp(_dal).Call(new DMTReqRes
            {
                APIID = request.APIID,
                Method = "CreateSender",
                RequestModeID = request.RequestMode,
                Request = _URL + "|" + JsonConvert.SerializeObject(req),
                Response = response,
                SenderNo = request.SenderMobile,
                UserID = request.UserID,
                TID = request.TransactionID
            });
            return res;
        }
        public MSenderLoginResponse GetSender(MTAPIRequest request)
        {
            FinodinLogin(request.RequestMode, request.SenderMobile, request.UserID, request.TID);
            var res = new MSenderLoginResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = nameof(ErrorCodes.Unknown_Error).Replace("_", " "),
                ErrorCode = ErrorCodes.Unknown_Error
            };
            var req = new
            {
                token = _FinodinToken,
                apiSource = request.APIOpCode,
                subAgentID = request.APIOutletID,
                mobile = request.SenderMobile
            };
            string response = string.Empty;
            var _URL = apiSetting.BaseURL + "RemitterSearch";

            try
            {
                response = AppWebRequest.O.PostJsonDataUsingHWR(_URL, req);

                if (!string.IsNullOrEmpty(response))
                {
                    var _apiRes = JsonConvert.DeserializeObject<FinodinResp>(response);
                    if (_apiRes != null)
                    {

                        if (_apiRes.statusCode == (int)FinodinErrCode.RemitterNotFound && _apiRes.description.Contains(""))
                        {
                            res.Msg = _apiRes.description;
                            res.ErrorCode = DMTErrorCodes.Sender_Not_Found;
                            res.IsSenderNotExists = true;
                            res.Statuscode = ErrorCodes.One;
                        }
                        else if (_apiRes.statusCode == ErrorCodes.One && _apiRes.description.Equals("success"))
                        {
                            res.Statuscode = ErrorCodes.One;
                            res.Msg = nameof(DMTErrorCodes.Detail_Found_Successfully).Replace("_", " ");
                            res.ErrorCode = DMTErrorCodes.Detail_Found_Successfully;
                            res.SenderName = _apiRes.remName;
                            res.RemainingLimit = _apiRes.remLimit;
                        }
                        else if (_apiRes.statusCode == (int)FinodinErrCode.Error && _apiRes.description.Contains("RemitterSearch faild. Could not connect to origin"))
                        {
                            res.Statuscode = ErrorCodes.Minus1;
                            res.Msg = _apiRes.description;
                            res.ErrorCode = ErrorCodes.Minus1;
                        }
                        else
                        {
                            res.Statuscode = ErrorCodes.Minus1;
                            res.Msg = nameof(ErrorCodes.FAILED);
                            res.ErrorCode = ErrorCodes.Minus1;
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                response = ex.Message + "|" + response;
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
            var dMTReq = new DMTReqRes
            {
                APIID = request.APIID,
                Method = "GetSender",
                RequestModeID = request.RequestMode,
                Request = _URL + JsonConvert.SerializeObject(req),
                Response = response,
                SenderNo = request.SenderMobile,
                UserID = request.UserID,
                TID = request.TransactionID
            };
            new ProcUpdateLogDMRReqResp(_dal).Call(dMTReq);
            return res;
        }
        public MSenderCreateResp SenderEKYC(MTAPIRequest request)
        {
            throw new NotImplementedException(); ;
        }
        public MSenderCreateResp SenderKYC(MTAPIRequest request)
        {
            throw new NotImplementedException();
        }
        public MSenderCreateResp SenderResendOTP(MTAPIRequest request)
        {
            FinodinLogin(request.RequestMode, request.SenderMobile, request.UserID, request.TID);

            var senderCreateResp = new MSenderCreateResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = nameof(ErrorCodes.Unknown_Error).Replace("_", " "),
                ErrorCode = ErrorCodes.Unknown_Error
            };

            var req = new
            {
                token = _FinodinToken,
                apiSource = string.Empty,
                subAgentID = string.Empty,
                mobile = string.Empty,
                remName = string.Empty,
                address = string.Empty,
                city = string.Empty,
                state = string.Empty,
                pinCode = string.Empty
            };
            string response = string.Empty;
            var _URL = apiSetting.BaseURL + "RemitterRegister";
            try
            {
                response = AppWebRequest.O.PostJsonDataUsingHWRTLS(_URL, req).Result;
                if (!string.IsNullOrEmpty(response))
                {
                    var _apiRes = JsonConvert.DeserializeObject<MMWFintechObjResp>(response);
                    if (_apiRes != null)
                    {
                        if (_apiRes.Message.Equals(MMWFintechCodes.MsgOtpSent) && _apiRes.Status.Equals(MMWFintechCodes.Msg200) && _apiRes.MessageStatus == true)
                        {
                            senderCreateResp.Statuscode = ErrorCodes.One;
                            senderCreateResp.Msg = _apiRes.Message;
                            senderCreateResp.IsOTPGenerated = true;
                            senderCreateResp.IsOTPResendAvailble = true;
                            senderCreateResp.ErrorCode = DMTErrorCodes.OTP_for_verification_has_been_sent_successfully;

                        }
                        else
                        {
                            senderCreateResp.Statuscode = ErrorCodes.Minus1;
                            senderCreateResp.Msg = nameof(ErrorCodes.FAILED);
                            senderCreateResp.ErrorCode = ErrorCodes.Minus1;
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
                    FuncName = "SenderResendOTP",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = request.UserID
                });
            }
            new ProcUpdateLogDMRReqResp(_dal).Call(new DMTReqRes
            {
                APIID = request.APIID,
                Method = "SenderResendOTP",
                RequestModeID = request.RequestMode,
                Request = _URL + JsonConvert.SerializeObject(req),
                Response = response,
                SenderNo = request.SenderMobile,
                UserID = request.UserID,
                TID = request.TransactionID
            });
            return senderCreateResp;
        }
        public MSenderLoginResponse VerifySender(MTAPIRequest request)
        {
            FinodinLogin(request.RequestMode, request.SenderMobile, request.UserID, request.TID);
            var res = new MSenderLoginResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = nameof(ErrorCodes.Unknown_Error).Replace("_", ""),
                ErrorCode = ErrorCodes.Unknown_Error
            };
            var req = new
            {
                token = _FinodinToken,
                apiSource = request.APIOpCode,
                subAgentID = request.APIOutletID,
                mobile = request.SenderMobile,
                otp = request.OTP,
                remID = request.SenderMobile
            };
            string response = string.Empty;
            var _URL = apiSetting.BaseURL + "RemitterRegisterValidate";
            try
            {
                response = AppWebRequest.O.PostJsonDataUsingHWR(_URL, req);
                if (!string.IsNullOrEmpty(response))
                {
                    var _apiRes = JsonConvert.DeserializeObject<MMWFintechObjResp>(response);
                    if (_apiRes != null)
                    {
                        if (_apiRes.Message.Equals(MMWFintechCodes.MsgSAS) && _apiRes.Status == MMWFintechCodes.Msg200 && _apiRes.MessageStatus == true)
                        {
                            res.Statuscode = ErrorCodes.One;
                            res.Msg = nameof(DMTErrorCodes.Sender_Created_Successfully);
                            res.ErrorCode = DMTErrorCodes.Sender_Created_Successfully;

                            var senderRes = (SenderInfo)new ProcVerySenderOTP(_dal).Call(new CommonReq
                            {
                                CommonStr = request.SenderMobile,
                                CommonStr2 = request.OTP,
                                CommonInt = request.UserID
                            });

                        }
                        else if (_apiRes.Message.Equals(MMWFintechCodes.MsgOtpExp) && _apiRes.Status == MMWFintechCodes.Msg200 && _apiRes.MessageStatus == false)
                        {
                            res.Statuscode = ErrorCodes.Minus1;
                            res.Msg = nameof(DMTErrorCodes.OTP_Expired) + "! Please Resend it!";
                            res.ErrorCode = DMTErrorCodes.OTP_Expired;
                        }
                        else
                        {
                            res.Statuscode = ErrorCodes.Minus1;
                            res.Msg = nameof(DMTErrorCodes.Transaction_Failed);
                            res.ErrorCode = DMTErrorCodes.Transaction_Failed;
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
                    FuncName = "VerifySender",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = request.UserID
                });
            }
            new ProcUpdateLogDMRReqResp(_dal).Call(new DMTReqRes
            {
                APIID = request.APIID,
                Method = "VerifySender",
                RequestModeID = request.RequestMode,
                Request = _URL + JsonConvert.SerializeObject(req),
                Response = response,
                SenderNo = request.SenderMobile,
                UserID = request.UserID,
                TID = request.TransactionID
            });
            return res;
        }
        public MSenderLoginResponse CreateBeneficiary(MTAPIRequest request)
        {
            FinodinLogin(request.RequestMode, request.SenderMobile, request.UserID, request.TID);
            var res = new MSenderLoginResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            var req = new
            {
                token = _FinodinToken,
                apiSource = request.APIOpCode,
                subAgentID = request.APIOutletID,
                mobile = request.SenderMobile,
                otp = "",
                remID = request.SenderMobile,
                benName = request.mBeneDetail.BeneName,
                benMobile = request.mBeneDetail.MobileNo,
                bankName = request.mBeneDetail.BankName,
                accountnumber = request.mBeneDetail.AccountNo,
                ifsc = request.mBeneDetail.IFSC
            };
            string response = string.Empty;
            var _URL = apiSetting.BaseURL + "BeneficiaryAdd";

            try
            {
                response = AppWebRequest.O.PostJsonDataUsingHWR(_URL, req);
                if (!string.IsNullOrEmpty(response))
                {
                    var _apiRes = JsonConvert.DeserializeObject<FinodinResp>(response);
                    if (_apiRes != null)
                    {
                        if (_apiRes.statusCode == (int)FinodinErrCode.success && _apiRes.description.Equals(nameof(FinodinErrCode.success)))
                        {
                            res.Statuscode = ErrorCodes.One;
                            res.Msg = nameof(DMTErrorCodes.Beneficiary_Added_Successfully);
                            res.ErrorCode = DMTErrorCodes.Beneficiary_Added_Successfully;
                        }
                        else if (_apiRes.statusCode == (int)FinodinErrCode.Error)
                        {
                            res.Statuscode = ErrorCodes.Minus1;
                            res.Msg = _apiRes.description;
                            res.ErrorCode = DMTErrorCodes.Transaction_Failed;
                        }
                        else
                        {
                            res.Statuscode = ErrorCodes.Minus1;
                            res.Msg = nameof(DMTErrorCodes.Transaction_Failed);
                            res.ErrorCode = DMTErrorCodes.Transaction_Failed;
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
                    FuncName = "CreateBeneficiary",
                    Error = response,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = request.UserID
                });
            }
            new ProcUpdateLogDMRReqResp(_dal).Call(new DMTReqRes
            {
                APIID = request.APIID,
                Method = "CreateBeneficiary",
                RequestModeID = request.RequestMode,
                Request = _URL + "|" + JsonConvert.SerializeObject(req),
                Response = response,
                SenderNo = request.SenderMobile,
                UserID = request.UserID,
                TID = request.TransactionID
            });
            return res;
        }
        public MBeneficiaryResp GetBeneficiary(MTAPIRequest request)
        {
            FinodinLogin(request.RequestMode, request.SenderMobile, request.UserID, request.TID);
            var res = new MBeneficiaryResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = nameof(ErrorCodes.Unknown_Error).Replace("_", " "),
                ErrorCode = ErrorCodes.Unknown_Error
            };
            var req = new
            {
                token = _FinodinToken,
                apiSource = request.APIOpCode,
                subAgentID = request.APIOutletID,
                mobile = request.SenderMobile
            };
            string response = string.Empty;
            var _URL = apiSetting.BaseURL + "BeneficiaryList";
            try
            {
                response = AppWebRequest.O.PostJsonDataUsingHWRTLS(_URL, req).Result;
                if (!string.IsNullOrEmpty(response))
                {
                    var _apiRes = JsonConvert.DeserializeObject<FinodinResp>(response);
                    if (_apiRes != null)
                    {
                        if (_apiRes.statusCode == (int)FinodinErrCode.success && _apiRes.description.Equals("success"))
                        {
                            res.Statuscode = ErrorCodes.One;
                            res.Msg = nameof(ErrorCodes.Transaction_Successful);
                            res.ErrorCode = ErrorCodes.Transaction_Successful;
                            if (_apiRes.beneficiaries != null)
                            {
                                if (_apiRes.beneficiaries != null)
                                {
                                    var Beneficiaries = new List<MBeneDetail>();
                                    foreach (var item in _apiRes.beneficiaries)
                                    {
                                        Beneficiaries.Add(new MBeneDetail
                                        {
                                            AccountNo = item.accountnumber,
                                            BankName = item.bank,
                                            IFSC = item.ifscode,
                                            BeneName = item.benName,
                                            BeneID = item.benID,
                                            IsVerified = true
                                        });
                                        //var param = new BenificiaryDetail
                                        //{
                                        //    _SenderMobileNo = request.SenderMobile,
                                        //    _Name = item.BenName,
                                        //    _AccountNumber = item.AccountNumber,
                                        //    _MobileNo = request.SenderMobile,
                                        //    _IFSC = item.IFSC,
                                        //    _BankName = item.BankName,
                                        //    _EntryBy = request.UserID,
                                        //    _VerifyStatus = Convert.ToInt32(item.VerifyStatus),
                                        //    _APICode = _APICode,
                                        //    _BeneAPIID = Convert.ToInt32(item.Id)
                                        //};
                                        //var resdb = (BenificiaryModel)new ProcAddBenificiary(_dal).Call(param);
                                    }
                                    res.Beneficiaries = Beneficiaries;
                                    res.ErrorCode = ErrorCodes.Transaction_Successful;
                                }
                            }
                        }
                        else if(_apiRes.statusCode == (int)FinodinErrCode.Error)
                        {
                            res.Msg = nameof(DMTErrorCodes.Beneficiary_not_found).Replace("_", " ");
                            res.ErrorCode = DMTErrorCodes.Beneficiary_not_found;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                response = ex.Message + "|" + response;
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "GetBeneficiary",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = request.UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            var dMTReq = new DMTReqRes
            {
                APIID = request.APIID,
                Method = "GetBeneficiary",
                RequestModeID = request.RequestMode,
                Request = _URL + "|" + JsonConvert.SerializeObject(req),
                Response = response,
                SenderNo = request.SenderMobile,
                UserID = request.UserID,
                TID = request.TransactionID
            };
            new ProcUpdateLogDMRReqResp(_dal).Call(dMTReq);
            return res;
        }
        private IEnumerable<MBeneDetail> List<T>(T recipientList)
        {
            throw new NotImplementedException();
        }
        public MSenderCreateResp GenerateOTP(MTAPIRequest request)
        {
            FinodinLogin(request.RequestMode, request.SenderMobile, request.UserID, request.TID);

            var senderCreateResp = new MSenderCreateResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = nameof(ErrorCodes.Unknown_Error).Replace("_", " "),
                ErrorCode = ErrorCodes.Unknown_Error
            };

            var resDB = (new ProcGetBenificiaryByBeneID(_dal).Call(new DMTReq { SenderNO = request.SenderMobile, BeneAPIID = Convert.ToInt32(request.mBeneDetail.BeneID) })) as BenificiaryDetail;


            var req = new
            {
                token = _FinodinToken,
                apiSource = string.Empty,
                subAgentID = string.Empty,
                mobile = string.Empty,
                benID = string.Empty,
                otp = string.Empty
            };
            string response = string.Empty;
            var _URL = apiSetting.BaseURL + "BeneficiaryAddValidate";
            try
            {
                response = AppWebRequest.O.PostJsonDataUsingHWRTLS(_URL, req).Result;
                if (!string.IsNullOrEmpty(response))
                {
                    var _apiRes = JsonConvert.DeserializeObject<MMWFintechResp>(response);
                    if (_apiRes.Message.Equals(MMWFintechCodes.MsgVerifyBen) && _apiRes.Status.Equals(MMWFintechCodes.Msg200) && _apiRes.MessageStatus == true)
                    {
                        if (_apiRes.JsonResult != null)
                        {
                            if (_apiRes.JsonResult.ISValid.Equals("true"))
                            {
                                senderCreateResp.Statuscode = -2;
                                senderCreateResp.Msg = _apiRes.Message;
                                senderCreateResp.ErrorCode = ErrorCodes.One;
                            }
                        }
                    }
                    else
                    {
                        senderCreateResp.Statuscode = ErrorCodes.Minus1;
                        senderCreateResp.Msg = nameof(ErrorCodes.FAILED);
                        senderCreateResp.ErrorCode = ErrorCodes.Minus1;
                    }

                }
            }
            catch (Exception ex)
            {
                response = ex.Message + "|" + response;
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
                Request = _URL + "|" + JsonConvert.SerializeObject(req),
                Response = response,
                SenderNo = request.SenderMobile,
                UserID = request.UserID,
                TID = request.TransactionID
            });
            return senderCreateResp;
        }
        public MSenderLoginResponse ValidateBeneficiary(MTAPIRequest request)
        {
            FinodinLogin(request.RequestMode, request.SenderMobile, request.UserID, request.TID);

            var mSenderLoginResponse = new MSenderLoginResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = nameof(ErrorCodes.Unknown_Error).Replace("_", " "),
                ErrorCode = ErrorCodes.Unknown_Error
            };
            var req = new
            {
                token = _FinodinToken,
                apiSource = string.Empty,
                subAgentID = string.Empty,
                mobile = string.Empty,
                benID = string.Empty,
                otp = string.Empty
            };
            string response = string.Empty;
            var _URL = apiSetting.BaseURL + "BeneficiaryAddValidate";
            try
            {
                response = AppWebRequest.O.PostJsonDataUsingHWRTLS(_URL, req).Result;
                if (!string.IsNullOrEmpty(response))
                {
                    var _apiRes = JsonConvert.DeserializeObject<MMWFintechResp>(response);
                    if (_apiRes != null)
                    {
                        //mSenderLoginResponse.Statuscode = _apiRes.Statuscode;
                        //mSenderLoginResponse.Msg = _apiRes.Message;
                        //mSenderLoginResponse.ErrorCode = _apiRes.ErrorCode;
                    }
                }
            }
            catch (Exception ex)
            {
                response = ex.Message + "|" + response;
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "ValidateBeneficiary",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = request.UserID
                });
            }
            new ProcUpdateLogDMRReqResp(_dal).Call(new DMTReqRes
            {
                APIID = request.APIID,
                Method = "ValidateBeneficiary",
                RequestModeID = request.RequestMode,
                Request = _URL + "|" + JsonConvert.SerializeObject(req),
                Response = response,
                SenderNo = request.SenderMobile,
                UserID = request.UserID,
                TID = request.TransactionID
            });
            return mSenderLoginResponse;
        }
        public MSenderLoginResponse RemoveBeneficiary(MTAPIRequest request)
        {
            FinodinLogin(request.RequestMode, request.SenderMobile, request.UserID, request.TID);

            var senderLoginResponse = new MSenderLoginResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError,
                ErrorCode = ErrorCodes.Unknown_Error
            };

            var req = new
            {
                token = _FinodinToken,
                apiSource = string.Empty,
                subAgentID = string.Empty,
                mobile = string.Empty,
                remID = string.Empty,
                benID = string.Empty
            };
            string response = string.Empty;
            var _URL = apiSetting.BaseURL + "BeneficiaryDelete";
            try
            {
                response = AppWebRequest.O.PostJsonDataUsingHWRTLS(_URL, req).Result;
                if (!string.IsNullOrEmpty(response))
                {
                    var _apiRes = JsonConvert.DeserializeObject<MMWFintechResp>(response);
                    if (_apiRes != null)
                    {
                        //if (_apiRes.statuscode.Equals("TXN") && _apiRes.data != null)
                        //{
                        //    senderLoginResponse.Statuscode = ErrorCodes.One;
                        //    senderLoginResponse.IsOTPGenerated = true;
                        //    senderLoginResponse.Msg = nameof(DMTErrorCodes.OTP_for_verification_has_been_sent_successfully);
                        //    senderLoginResponse.ErrorCode = DMTErrorCodes.OTP_for_verification_has_been_sent_successfully;
                        //}
                        //else
                        //{
                        //    senderLoginResponse.Statuscode = ErrorCodes.Minus1;
                        //    senderLoginResponse.Msg = nameof(DMTErrorCodes.Transaction_Failed);
                        //    senderLoginResponse.ErrorCode = DMTErrorCodes.Transaction_Failed;
                        //}
                    }
                }
            }
            catch (Exception ex)
            {
                response = ex.Message + "|" + response;
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "RemoveBeneficiary",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = request.UserID
                });
            }
            new ProcUpdateLogDMRReqResp(_dal).Call(new DMTReqRes
            {
                APIID = request.APIID,
                Method = "RemoveBeneficiary",
                RequestModeID = request.RequestMode,
                Request = _URL + "|" + JsonConvert.SerializeObject(req),
                Response = response,
                SenderNo = request.SenderMobile,
                UserID = request.UserID,
                TID = request.TransactionID
            });
            return senderLoginResponse;
        }
        public MSenderLoginResponse RemoveBeneficiaryValidate(MTAPIRequest request)
        {
            FinodinLogin(request.RequestMode, request.SenderMobile, request.UserID, request.TID);

            var mSenderLoginResponse = new MSenderLoginResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = nameof(ErrorCodes.Unknown_Error).Replace("_", " "),
                ErrorCode = ErrorCodes.Unknown_Error
            };
            var req = new
            {
                token = _FinodinToken,
                apiSource = string.Empty,
                subAgentID = string.Empty,
                mobile = string.Empty,
                benID = string.Empty,
                otp = string.Empty
            };
            string response = string.Empty;
            var _URL = apiSetting.BaseURL + "BeneficiaryDeleteValidate";
            try
            {
                response = AppWebRequest.O.PostJsonDataUsingHWRTLS(_URL, req).Result;
                if (!string.IsNullOrEmpty(response))
                {
                    var _apiRes = JsonConvert.DeserializeObject<IPayDMRResp>(response);
                    if (_apiRes != null)
                    {
                        if (_apiRes.statuscode.Equals("TXN"))
                        {
                            mSenderLoginResponse.Statuscode = ErrorCodes.One;
                            mSenderLoginResponse.Msg = nameof(DMTErrorCodes.Beneficiary_Deactivated_or_Deleted);
                            mSenderLoginResponse.ErrorCode = DMTErrorCodes.Beneficiary_Deactivated_or_Deleted;
                        }
                        else
                        {
                            mSenderLoginResponse.Statuscode = ErrorCodes.Minus1;
                            mSenderLoginResponse.Msg = nameof(DMTErrorCodes.Transaction_Failed);
                            mSenderLoginResponse.ErrorCode = DMTErrorCodes.Transaction_Failed;
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
                    FuncName = "RemoveBeneficiaryValidate",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = request.UserID
                });
            }
            new ProcUpdateLogDMRReqResp(_dal).Call(new DMTReqRes
            {
                APIID = request.APIID,
                Method = "RemoveBeneficiaryValidate",
                RequestModeID = request.RequestMode,
                Request = _URL + "|" + JsonConvert.SerializeObject(req),
                Response = response,
                SenderNo = request.SenderMobile,
                UserID = request.UserID,
                TID = request.TransactionID
            });
            return mSenderLoginResponse;
        }
        public DMRTransactionResponse VerifyAccount(MTAPIRequest request)
        {
            FinodinLogin(request.RequestMode, request.SenderMobile, request.UserID, request.TID);
            var res = new DMRTransactionResponse
            {
                Statuscode = RechargeRespType.PENDING,
                Msg = RechargeRespType._PENDING,
                VendorID = "",
                LiveID = ""
            };
            var req = new
            {
                token = _FinodinToken,
                apiSource = request.APIOpCode,
                subAgentID = request.APIOutletID,
                mobile = request.SenderMobile,
                remID = request.SenderMobile,
                ifsc = request.mBeneDetail.IFSC,
                accountnumber = request.mBeneDetail.AccountNo,
                clientRefID = request.TID
            };
            string response = string.Empty;
            var _URL = apiSetting.BaseURL + "BeneficiaryAccountVerify";
            try
            {
                response = AppWebRequest.O.PostJsonDataUsingHWR(_URL, req);
                if (!string.IsNullOrEmpty(response))
                {
                    if (response != null)
                    {
                        var _apiResp = JsonConvert.DeserializeObject<FinodinResp>(response);
                        if (_apiResp != null)
                        {
                            if (_apiResp.statusCode == (int)FinodinErrCode.success && _apiResp.description.Equals("success"))
                            {
                                if (_apiResp.transaction != null)
                                {
                                    if (_apiResp.transaction.status.Equals("Confirmed"))
                                    {
                                        res.Statuscode = RechargeRespType.SUCCESS;
                                        res.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                                        res.ErrorCode = ErrorCodes.Transaction_Successful;
                                        res.BeneName = _apiResp.transaction.benName;
                                        res.LiveID = _apiResp.transaction.rrn;
                                        res.VendorID = _apiResp.transaction.tranID;
                                    }
                                }
                                
                                
                                //if (apiResp.JsonResult != null)
                                //{
                                //    if (apiResp.JsonResult.ISValid.Equals("true"))
                                //    {
                                //        res.Statuscode = RechargeRespType.SUCCESS;
                                //        res.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                                //        res.ErrorCode = ErrorCodes.Transaction_Successful;
                                //        res.BeneName = apiResp.JsonResult.BenName;
                                //        res.LiveID = string.Empty;
                                //        res.VendorID = string.Empty;
                                //    }
                                //    else if (apiResp.JsonResult.ISValid.Equals("false"))
                                //    {
                                //        res.Statuscode = ErrorCodes.Minus1;
                                //        res.Msg = nameof(ErrorCodes.FAILED);
                                //        res.ErrorCode = ErrorCodes.Minus1;
                                //    }
                                //}
                            }
                            else
                            {
                                errorCodeML = new ErrorCodeML(_accessor, _env, false);
                                var eFromDB = errorCodeML.GetAPIErrorCodeDescription(_APIGroupCode, _apiResp.description);
                                if (!string.IsNullOrEmpty(eFromDB.Code))
                                {
                                    res.Statuscode = eFromDB.Status;
                                    res.Msg = eFromDB.Error.Replace("{REPLACE}", _apiResp.description);
                                    res.ErrorCode = Convert.ToInt32(eFromDB.Code.Trim());
                                    res.LiveID = res.Msg;
                                }
                                res.Statuscode = res.Statuscode == 0 ? RechargeRespType.PENDING : res.Statuscode;
                            }
                            //else
                            //{
                            //    res.Statuscode = ErrorCodes.Minus1;
                            //    res.Msg = nameof(ErrorCodes.FAILED);
                            //    res.ErrorCode = ErrorCodes.Minus1;
                            //}
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
            res.Request = _URL + "|" + JsonConvert.SerializeObject(req);
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
            FinodinLogin(request.RequestMode, request.SenderMobile, request.UserID, request.TID);

            var res = new DMRTransactionResponse
            {
                Statuscode = RechargeRespType.PENDING,
                Msg = RechargeRespType._PENDING,
                VendorID = "",
                LiveID = ""
            };
            var req = new
            {
                token = _FinodinToken,
                apiSource = request.APIOpCode,
                subAgentID = request.APIOutletID,
                mobile = request.SenderMobile,
                remID = request.SenderMobile,
                benID = request.mBeneDetail.BeneID,
                amount = request.Amount,
                tranType = request.TransMode,
                clientRefIDs = request.TID
            };
            
            string response = string.Empty;
            var _URL = apiSetting.BaseURL + "Transactions";

            try
            {
                response = AppWebRequest.O.PostJsonDataUsingHWR(_URL, req);
                if (!string.IsNullOrEmpty(response))
                {
                    var _apiRes = JsonConvert.DeserializeObject<FDAccTrResp>(response);
                    if (_apiRes != null)
                    {
                        if (_apiRes.statusCode == (int)FinodinErrCode.success && _apiRes.description.Equals("success"))
                        {
                            if (_apiRes.transaction != null)
                            {
                                if (_apiRes.transaction[0].status.Equals("Unknown") && _apiRes.transaction[0].error.Contains("The request to the origin timed out"))
                                {
                                    res.Statuscode = RechargeRespType.FAILED;
                                    res.Msg = _apiRes.transaction[0].error;
                                    res.ErrorCode = 0;
                                    res.VendorID = _apiRes.transaction[0].tranID;
                                    res.LiveID = _apiRes.transaction[0].error;
                                    res.BeneName = request.mBeneDetail.BeneName;
                                }
                                else if (_apiRes.transaction[0].status.Equals("Pending"))
                                {
                                    res.Statuscode = RechargeRespType.PENDING;
                                    res.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                                    res.ErrorCode = ErrorCodes.Transaction_Successful;
                                    res.VendorID = _apiRes.transaction[0].tranID;
                                }
                            }
                        }
                        else
                        {
                            errorCodeML = new ErrorCodeML(_accessor, _env, false);
                            var eFromDB = errorCodeML.GetAPIErrorCodeDescription(_APIGroupCode, _apiRes.description);
                            if (!string.IsNullOrEmpty(eFromDB.Code))
                            {
                                res.Statuscode = eFromDB.Status;
                                res.Msg = eFromDB.Error.Replace("{REPLACE}", _apiRes.description);
                                res.ErrorCode = Convert.ToInt32(eFromDB.Code.Trim());
                                res.LiveID = res.Msg;
                            }
                            res.Statuscode = res.Statuscode == 0 ? RechargeRespType.PENDING : res.Statuscode;
                        }



                        //    if (_apiRes.JsonResult != null)
                        //{
                        //    if (_apiRes.JsonResult.TransactionStatus.Equals(MMWFintechCodes.MsgSuccess) && _apiRes.Status.Equals(MMWFintechCodes.Msg200) && _apiRes.MessageStatus == true)
                        //    {
                        //        res.Statuscode = RechargeRespType.SUCCESS;
                        //        res.Msg = _apiRes.Message;
                        //        res.ErrorCode = 0;
                        //        res.VendorID = _apiRes.JsonResult.SystemTransactionId;
                        //        res.LiveID = _apiRes.JsonResult.Uniquetransactionreference;
                        //        res.BeneName = request.mBeneDetail.BeneName;

                        //    }
                        //    else if (_apiRes.JsonResult.TransactionStatus.Equals(MMWFintechCodes.MsgFailed) && _apiRes.Status.Equals(MMWFintechCodes.Msg200) && _apiRes.MessageStatus == true)
                        //    {
                        //        res.Statuscode = RechargeRespType.FAILED;
                        //        res.Msg = _apiRes.Message;
                        //        res.ErrorCode = 0;
                        //        res.VendorID = _apiRes.JsonResult.SystemTransactionId;
                        //        res.LiveID = _apiRes.JsonResult.Uniquetransactionreference;
                        //        res.BeneName = request.mBeneDetail.BeneName;
                        //    }
                        //    else if (_apiRes.JsonResult.TransactionStatus.Equals(MMWFintechCodes.MsgPending) && _apiRes.Status.Equals(MMWFintechCodes.Msg200) && _apiRes.MessageStatus == true)
                        //    {
                        //        res.Statuscode = RechargeRespType.PENDING;
                        //        res.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                        //        res.ErrorCode = ErrorCodes.Transaction_Successful;
                        //        res.VendorID = _apiRes.JsonResult.SystemTransactionId;
                        //    }
                        //    else
                        //    {
                        //        IErrorCodeML errorCodeML = new ErrorCodeML(_accessor, _env, false);
                        //        var eFromDB = errorCodeML.GetAPIErrorCodeDescription(request.APIGroupCode, _apiRes.Status.ToString());
                        //        if (!string.IsNullOrEmpty(eFromDB.Code))
                        //        {
                        //            res.Statuscode = eFromDB.Status;
                        //            res.Msg = eFromDB.Error.Replace("{REPLACE}", _apiRes.Status.ToString());
                        //            res.ErrorCode = Convert.ToInt32(eFromDB.Code.Trim());
                        //            res.LiveID = res.Msg;
                        //        }
                        //        res.Statuscode = res.Statuscode == 0 ? RechargeRespType.PENDING : res.Statuscode;
                        //    }
                        //}
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
            res.Request = _URL + "|" + JsonConvert.SerializeObject(req);
            res.Response = response;
            new ProcUpdateLogDMRReqResp(_dal).Call(new DMTReqRes
            {
                APIID = request.APIID,
                Method = "AccountTransfer",
                RequestModeID = request.RequestMode,
                Request = res.Request,
                Response = res.Response,
                SenderNo = request.SenderMobile,
                UserID = request.UserID,
                TID = request.TID.ToString()
            });
            return res;
        }
        //private DMRTransactionResponse LoopStatusCheck(int TID, int RequestMode, int UserID, int APIID)
        //{
        //    var res = new DMRTransactionResponse
        //    {
        //        Statuscode = RechargeRespType.PENDING,
        //        Msg = nameof(ErrorCodes.Request_Accpeted).Replace("_", " "),
        //        ErrorCode = ErrorCodes.Request_Accpeted
        //    };
        //    int i = 0, LoopCount = 1;
        //    while (i < LoopCount)
        //    {
        //        i++;
        //        if (res.Statuscode == RechargeRespType.PENDING)
        //        {
        //            res = GetTransactionStatus(TID, RequestMode, UserID, APIID);
        //            if (res.Statuscode != RechargeRespType.PENDING)
        //            {
        //                i = LoopCount;
        //            }
        //        }
        //        else
        //        {
        //            i = LoopCount;
        //        }
        //    }
        //    return res;
        //}
        public DMRTransactionResponse GetTransactionStatus(int TID, int UserID, int RequestMode,string APIOpCode, string APIOutletID)
        {
            FinodinLogin(RequestMode, string.Empty, UserID, TID);
            //string TransactionID,  int UserID, int APIID, string VendorID
            var res = new DMRTransactionResponse
            {
                Statuscode = RechargeRespType.PENDING,
                Msg = nameof(RechargeRespType.PENDING),
                ErrorCode = ErrorCodes.Request_Accpeted
            };
            var req = new
            {
                token = _FinodinToken,
                apiSource = APIOpCode,
                subAgentID = APIOutletID,
                clientRefID = TID
            };
            string response = string.Empty;
            var _URL = apiSetting.BaseURL + "TransactionQuery";
            try
            {
                response = AppWebRequest.O.PostJsonDataUsingHWR(_URL, req);
                if (response != null)
                {
                    var rpFintechSts = JsonConvert.DeserializeObject<FDAccTrResp>(response);
                    if (rpFintechSts != null)
                    {
                        if (rpFintechSts.transaction != null)
                        {
                            //if (rpFintechSts.JsonResult.TransactionStatus == MMWFintechCodes.MsgSuccess)
                            //{
                            //    res.VendorID = rpFintechSts.JsonResult.SystemTransactionId;
                            //    res.LiveID = rpFintechSts.JsonResult.Uniquetransactionreference;
                            //    res.Statuscode = RechargeRespType.SUCCESS;
                            //    res.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                            //    res.ErrorCode = ErrorCodes.Transaction_Successful;
                            //}
                            //if (rpFintechSts.JsonResult.TransactionStatus == MMWFintechCodes.MsgSuccess)
                            //{
                            //    res.VendorID = rpFintechSts.JsonResult.SystemTransactionId;
                            //    res.LiveID = rpFintechSts.Message.Contains("suffi") ? ErrorCodes.Down : rpFintechSts.Message;
                            //    res.Statuscode = RechargeRespType.FAILED;
                            //    res.Msg = nameof(ErrorCodes.Unknown_Error).Replace("_", " ");
                            //    res.ErrorCode = ErrorCodes.Unknown_Error;
                            //}
                            
                        }
                        else
                        {
                            var eFromDB = errorCodeML.GetAPIErrorCodeDescription(_APIGroupCode, rpFintechSts.description);
                            if (!string.IsNullOrEmpty(eFromDB.Code))
                            {
                                res.Statuscode = eFromDB.Status;
                                res.Msg = eFromDB.Error.Replace("{REPLACE}", rpFintechSts.description);
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
                    FuncName = "GetTransactionStatus",
                    Error = ex.Message,
                    LoginTypeID = 1,
                    UserId = UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }

            var dMTReq = new DMTReqRes
            {
                APIID = _APIID,
                Method = "GetTransactionStatus",
                RequestModeID = RequestMode,
                Request = _URL + "|" + JsonConvert.SerializeObject(req),
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

        //public ResponseStatus RefundOTP(string VendorID, int RequestMode, int UserID, string SenderNo, int TID, int APIID)
        //{
        //    var res = new ResponseStatus
        //    {
        //        Statuscode = ErrorCodes.Minus1,
        //        Msg = nameof(ErrorCodes.Unknown_Error).Replace("_", " "),
        //        ErrorCode = ErrorCodes.Unknown_Error
        //    };
        //    if (!ChannelPartnerLogin())
        //    {
        //        res.Msg = "instituteId expired";
        //        return res;
        //    }
        //    var req = new refundotpreq
        //    {
        //        header = new RBLHeader
        //        {
        //            sessionToken = RBLSession
        //        },
        //        RBLtransactionid = VendorID
        //    };
        //    var xmlReq = XMLHelper.O.SerializeToXml(req, null);
        //    string response = string.Empty;
        //    var _URL = appSetting.DMTServiceURL + string.Format("?client_id={0}&client_secret={1}", appSetting.CLIENTID, appSetting.CLIENTSECRET);
        //    string authString = string.Format("{0}:{1}", appSetting.LDAP, appSetting.LDAPPASSWORD);
        //    var headers = new Dictionary<string, string>
        //    {
        //        { "Authorization", "Basic "+ Convert.ToBase64String(Encoding.UTF8.GetBytes(authString)) }
        //    };
        //    try
        //    {
        //        string KeyPath = Path.Combine(Directory.GetCurrentDirectory(), RBLPFXFile);
        //        response = AppWebRequest.O.PostJsonDataUsingHWRTLSWithCertificate(_URL, xmlReq, headers, KeyPath, appSetting.PFXPD).Result;
        //        if (!string.IsNullOrEmpty(response))
        //        {
        //            var errorCheck = GetErrorIfExists(response);
        //            if (errorCheck.Statuscode == ErrorCodes.One)
        //            {
        //                var _apiRes = new refundotpres();
        //                _apiRes = XMLHelper.O.DesrializeToObject(_apiRes, response, null);
        //                if ((_apiRes.status ?? 0) == 1)
        //                {
        //                    res.Statuscode = ErrorCodes.One;
        //                    res.Msg = nameof(DMTErrorCodes.OTP_for_verification_has_been_sent_successfully).Replace("_", " ");
        //                    res.ErrorCode = DMTErrorCodes.OTP_for_verification_has_been_sent_successfully;
        //                }
        //                else
        //                {

        //                    res.Msg = nameof(DMTErrorCodes.Declined_by_ServiceProvider).Replace("_", " ");
        //                    res.ErrorCode = DMTErrorCodes.Declined_by_ServiceProvider;
        //                }
        //            }
        //            else
        //            {

        //                res.Msg = errorCheck.Msg;
        //                res.ErrorCode = DMTErrorCodes.Declined_by_ServiceProvider;
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        response = " Exception:" + ex.Message + " | " + response;
        //        var errorLog = new ErrorLog
        //        {
        //            ClassName = GetType().Name,
        //            FuncName = "ResendRefundOTP",
        //            Error = ex.Message,
        //            LoginTypeID = 1,
        //            UserId = UserID
        //        };
        //        var _ = new ProcPageErrorLog(_dal).Call(errorLog);
        //    }
        //    var dMTReq = new DMTReqRes
        //    {
        //        APIID = APIID,
        //        Method = "RefundOTP",
        //        RequestModeID = RequestMode,
        //        Request = _URL + xmlReq + JsonConvert.SerializeObject(headers),
        //        Response = response,
        //        SenderNo = SenderNo,
        //        UserID = UserID,
        //        TID = TID.ToString()
        //    };
        //    new ProcUpdateLogDMRReqResp(_dal).Call(dMTReq);
        //    return res;
        //}
        //public ResponseStatus Refund(int TID, string VendorID, int RequestMode, int UserID, string SenderNo, string OTP, int APIID)
        //{
        //    var res = new ResponseStatus
        //    {
        //        Statuscode = ErrorCodes.Minus1,
        //        Msg = nameof(ErrorCodes.Unknown_Error).Replace("_", " "),
        //        ErrorCode = ErrorCodes.Unknown_Error
        //    };
        //    if (!ChannelPartnerLogin())
        //    {
        //        res.Msg = "instituteId expired";
        //        return res;
        //    }
        //    var req = new refundreq
        //    {
        //        header = new RBLHeader
        //        {
        //            sessionToken = RBLSession
        //        },
        //        bcagent = appSetting.BCAGENT,
        //        channelpartnerrefno = TIDPrefix + TID,
        //        verficationcode = OTP,
        //        flag = 1
        //    };
        //    var xmlReq = XMLHelper.O.SerializeToXml(req, null);
        //    string response = string.Empty;
        //    var _URL = appSetting.DMTServiceURL + string.Format("?client_id={0}&client_secret={1}", appSetting.CLIENTID, appSetting.CLIENTSECRET);
        //    string authString = string.Format("{0}:{1}", appSetting.LDAP, appSetting.LDAPPASSWORD);
        //    var headers = new Dictionary<string, string>
        //    {
        //        { "Authorization", "Basic "+ Convert.ToBase64String(Encoding.UTF8.GetBytes(authString)) }
        //    };
        //    try
        //    {
        //        string KeyPath = Path.Combine(Directory.GetCurrentDirectory(), RBLPFXFile);
        //        response = AppWebRequest.O.PostJsonDataUsingHWRTLSWithCertificate(_URL, xmlReq, headers, KeyPath, appSetting.PFXPD).Result;
        //        if (!string.IsNullOrEmpty(response))
        //        {
        //            var errorCheck = GetErrorIfExists(response);
        //            if (errorCheck.Statuscode == ErrorCodes.One)
        //            {
        //                var _apiRes = new refundres();
        //                _apiRes = XMLHelper.O.DesrializeToObject(_apiRes, response, null);
        //                if ((_apiRes.status ?? 0) == 1)
        //                {
        //                    res.Statuscode = ErrorCodes.One;
        //                    res.Msg = nameof(DMTErrorCodes.Transaction_Status_Updated_Refund).Replace("_", " ");
        //                    res.ErrorCode = DMTErrorCodes.Transaction_Status_Updated_Refund;
        //                }
        //                else
        //                {
        //                    res.Msg = nameof(ErrorCodes.Invalid_OTP).Replace("_", " ");
        //                    res.ErrorCode = ErrorCodes.Invalid_OTP;
        //                }
        //            }
        //            else
        //            {
        //                res.Msg = errorCheck.Msg;
        //                res.ErrorCode = ErrorCodes.Unknown_Error;
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        response = " Exception:" + ex.Message + " | " + response;
        //        var errorLog = new ErrorLog
        //        {
        //            ClassName = GetType().Name,
        //            FuncName = "Refund",
        //            Error = ex.Message,
        //            LoginTypeID = 1,
        //            UserId = UserID
        //        };
        //        var _ = new ProcPageErrorLog(_dal).Call(errorLog);
        //    }
        //    var dMTReq = new DMTReqRes
        //    {
        //        APIID = APIID,
        //        Method = "Refund",
        //        RequestModeID = RequestMode,
        //        Request = _URL + xmlReq + JsonConvert.SerializeObject(headers),
        //        Response = response,
        //        SenderNo = SenderNo,
        //        UserID = UserID,
        //        TID = TID.ToString()
        //    };
        //    new ProcUpdateLogDMRReqResp(_dal).Call(dMTReq);
        //    res.CommonStr = dMTReq.Request;
        //    res.CommonStr2 = dMTReq.Response;
        //    return res;
        //}
    }

}