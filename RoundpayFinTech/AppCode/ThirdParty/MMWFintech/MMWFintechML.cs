using Fintech.AppCode.Configuration;
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
using System;
using System.Collections.Generic;
using System.IO;
using Validators;

namespace RoundpayFinTech.AppCode.MiddleLayer.Dmt_Api
{
    public partial class MMWFintechML
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        private readonly IConfiguration Configuration;

        private readonly string _APICode;
        private readonly int _APIID;
        private readonly IDAL _dal;
        private readonly MMWFintechAppSetting apiSetting;


        public MMWFintechML(IHttpContextAccessor accessor, IHostingEnvironment env, string APICode, int APIID, IDAL dal)
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
        }
        public MMWFintechAppSetting AppSetting()
        {
            var setting = new MMWFintechAppSetting();
            try
            {
                setting = new MMWFintechAppSetting
                {
                    APIKey = Configuration["DMR:" + _APICode + ":APIKey"],
                    BaseURL = Configuration["DMR:" + _APICode + ":BaseURL"],
                    Username = Configuration["DMR:" + _APICode + ":Username"],
                    Password = Configuration["DMR:" + _APICode + ":Password"]
                };
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "MMWFintechAppSetting:" + (_APICode ?? string.Empty),
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return setting;
        }
        public OutletAPIStatusUpdate SaveOutletOfAPIUser(FintechAPIRequestModel fintechRequest)
        {
            string Req = string.Empty;
            string Resp = string.Empty;

            var OutReqRes = new OutletAPIStatusUpdate
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            //fintechRequest.UserID = 0;
            //fintechRequest.Token = apiSetting.instituteId;
            //try
            //{
            //    Req = apiSetting.DMTServiceURL + "API/Outlet/Register?" + JsonConvert.SerializeObject(fintechRequest);
            //    Resp = AppWebRequest.O.PostJsonDataUsingHWR(apiSetting.DMTServiceURL + "API/Outlet/Register/", fintechRequest);
            //    if (Validate.O.ValidateJSON(Resp ?? string.Empty))
            //    {
            //        var _apiRes = JsonConvert.DeserializeObject<FintechAPIResponse>(Resp);
            //        if (_apiRes != null)
            //        {
            //            if (_apiRes.Statuscode == ErrorCodes.One)
            //            {
            //                OutReqRes.Statuscode = ErrorCodes.One;
            //                OutReqRes.Msg = ErrorCodes.OutletRegistered;
            //                OutReqRes.APIOutletID = _apiRes.data.OutletID.ToString();
            //                OutReqRes.APIOutletStatus = RndKYCOutLetStatus._Pending;
            //                OutReqRes.KYCStatus = RndKYCOutLetStatus._Pending;
            //                OutReqRes.BBPSID = _apiRes.data.OutletID.ToString();
            //                OutReqRes.BBPSStatus = RndKYCOutLetStatus._Pending;
            //                OutReqRes.AEPSID = _apiRes.data.OutletID.ToString();
            //                OutReqRes.AEPSStatus = RndKYCOutLetStatus._Pending;
            //                OutReqRes.DMTID = _apiRes.data.OutletID.ToString();
            //                OutReqRes.DMTStatus = RndKYCOutLetStatus._Pending;
            //                OutReqRes.PSAID = string.Empty;
            //                OutReqRes.PSAStatus = RndKYCOutLetStatus._NotApplied;
            //            }
            //            else
            //            {
            //                OutReqRes.APIOutletStatus = RndKYCOutLetStatus._Rejected;
            //                OutReqRes.BBPSStatus = RndKYCOutLetStatus._Rejected;
            //                OutReqRes.AEPSStatus = RndKYCOutLetStatus._Rejected;
            //                OutReqRes.DMTStatus = RndKYCOutLetStatus._Rejected;
            //                OutReqRes.Msg = string.IsNullOrEmpty(_apiRes.Msg) ? ErrorCodes.FailedToSubmit : _apiRes.Msg;
            //            }
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
            //    {
            //        ClassName = GetType().Name,
            //        FuncName = "SaveOutletOfAPIUser",
            //        Error = ex.Message,
            //    });
            //    Resp = ex.Message + "_" + Resp;
            //    OutReqRes.Msg = ErrorCodes.TempError;
            //}
            //#region APILogOutletRegistration
            //new ProcUpdateLogOnboardingReqResp(_dal).Call(new OnboardingLog
            //{
            //    APIID = _APIID,
            //    Method = "SaveOutletOfAPIUser",
            //    Request = Req,
            //    Response = Resp,

            //});
            //#endregion

            return OutReqRes;
        }
        public OutletAPIStatusUpdate UpdateOutletOfAPIUser(FintechAPIRequestModel fintechRequest)
        {
            string Req = string.Empty;
            string Resp = string.Empty;
            var OutReqRes = new OutletAPIStatusUpdate
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            //fintechRequest.UserID = 0;
            //fintechRequest.Token = apiSetting.instituteId;
            //try
            //{
            //    Req = apiSetting.DMTServiceURL + "API/Outlet/Update?" + JsonConvert.SerializeObject(fintechRequest);
            //    Resp = AppWebRequest.O.PostJsonDataUsingHWR(apiSetting.DMTServiceURL + "API/Outlet/Update/", fintechRequest);
            //    if (Validate.O.ValidateJSON(Resp ?? string.Empty))
            //    {
            //        var _apiRes = JsonConvert.DeserializeObject<FintechAPIResponse>(Resp);
            //        if (_apiRes != null)
            //        {
            //            if (_apiRes.Statuscode == ErrorCodes.One)
            //            {
            //                OutReqRes.Statuscode = ErrorCodes.One;
            //                OutReqRes.Msg = ErrorCodes.OutletRegistered;
            //                OutReqRes.APIOutletID = _apiRes.data.OutletID.ToString();
            //                OutReqRes.APIOutletStatus = RndKYCOutLetStatus._Pending;
            //                OutReqRes.KYCStatus = RndKYCOutLetStatus._Pending;
            //                OutReqRes.BBPSID = _apiRes.data.OutletID.ToString();
            //                OutReqRes.BBPSStatus = RndKYCOutLetStatus._Pending;
            //                OutReqRes.AEPSID = _apiRes.data.OutletID.ToString();
            //                OutReqRes.AEPSStatus = RndKYCOutLetStatus._Pending;
            //                OutReqRes.DMTID = _apiRes.data.OutletID.ToString();
            //                OutReqRes.DMTStatus = RndKYCOutLetStatus._Pending;
            //                OutReqRes.PSAID = string.Empty;
            //                OutReqRes.PSAStatus = RndKYCOutLetStatus._NotApplied;
            //            }
            //            else
            //            {
            //                OutReqRes.APIOutletStatus = RndKYCOutLetStatus._Rejected;
            //                OutReqRes.BBPSStatus = RndKYCOutLetStatus._Rejected;
            //                OutReqRes.AEPSStatus = RndKYCOutLetStatus._Rejected;
            //                OutReqRes.DMTStatus = RndKYCOutLetStatus._Rejected;
            //                OutReqRes.Msg = string.IsNullOrEmpty(_apiRes.Msg) ? ErrorCodes.FailedToSubmit : _apiRes.Msg;
            //            }
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
            //    {
            //        ClassName = GetType().Name,
            //        FuncName = "UpdateOutletOfAPIUser",
            //        Error = ex.Message,
            //    });
            //    Resp = ex.Message + "_" + Resp;
            //    OutReqRes.Msg = ErrorCodes.TempError;
            //}
            //#region APILogOutletRegistration
            //new ProcUpdateLogOnboardingReqResp(_dal).Call(new OnboardingLog
            //{
            //    APIID = _APIID,
            //    Method = "UpdateOutletOfAPIUser",
            //    Request = Req,
            //    Response = Resp,

            //});
            //#endregion
            return OutReqRes;
        }
        public OutletAPIStatusUpdate CheckOutletStatus(FintechAPIRequestModel fintechRequest)
        {
            string Req = string.Empty;
            string Resp = string.Empty;
            var OutReqRes = new OutletAPIStatusUpdate
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            //fintechRequest.UserID = 0;
            //fintechRequest.Token = apiSetting.instituteId;
            //try
            //{
            //    Req = apiSetting.DMTServiceURL + "API/Outlet/CheckStatus?" + JsonConvert.SerializeObject(fintechRequest);
            //    Resp = AppWebRequest.O.PostJsonDataUsingHWR(apiSetting.DMTServiceURL + "API/Outlet/CheckStatus/", fintechRequest);
            //    if (Validate.O.ValidateJSON(Resp ?? string.Empty))
            //    {
            //        var _apiRes = JsonConvert.DeserializeObject<FintechAPIResponse>(Resp);
            //        if (_apiRes != null)
            //        {
            //            if (_apiRes.Statuscode == ErrorCodes.One)
            //            {
            //                OutReqRes.Statuscode = ErrorCodes.One;
            //                OutReqRes.Msg = ErrorCodes.OutletRegistered;
            //                OutReqRes.APIOutletID = _apiRes.data.OutletID.ToString();
            //                OutReqRes.APIOutletStatus = _apiRes.data.VerifyStatus;
            //                if (_apiRes.data.KYCStatus == 3)
            //                {
            //                    OutReqRes.KYCStatus = UserStatus.ACTIVE;
            //                }
            //                if (_apiRes.data.KYCStatus == 2)
            //                {
            //                    OutReqRes.KYCStatus = UserStatus.APPLIED;
            //                }
            //                if (_apiRes.data.KYCStatus.In(4, 5))
            //                {
            //                    OutReqRes.KYCStatus = UserStatus.REJECTED;
            //                }
            //                OutReqRes.BBPSID = _apiRes.data.OutletID.ToString();
            //                OutReqRes.BBPSStatus = RndKYCOutLetStatus._Pending;
            //                OutReqRes.AEPSID = _apiRes.data.OutletID.ToString();
            //                OutReqRes.AEPSStatus = RndKYCOutLetStatus._Pending;
            //                OutReqRes.DMTID = _apiRes.data.OutletID.ToString();
            //                OutReqRes.DMTStatus = RndKYCOutLetStatus._Pending;
            //                OutReqRes.PSAID = string.Empty;
            //                OutReqRes.PSAStatus = RndKYCOutLetStatus._NotApplied;
            //            }
            //            else
            //            {
            //                OutReqRes.APIOutletStatus = RndKYCOutLetStatus._Rejected;
            //                OutReqRes.BBPSStatus = RndKYCOutLetStatus._Rejected;
            //                OutReqRes.AEPSStatus = RndKYCOutLetStatus._Rejected;
            //                OutReqRes.DMTStatus = RndKYCOutLetStatus._Rejected;
            //                OutReqRes.Msg = string.IsNullOrEmpty(_apiRes.Msg) ? ErrorCodes.FailedToSubmit : _apiRes.Msg;
            //            }
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
            //    {
            //        ClassName = GetType().Name,
            //        FuncName = "CheckOutletStatus",
            //        Error = ex.Message,
            //    });
            //    Resp = ex.Message + "_" + Resp;
            //    OutReqRes.Msg = ErrorCodes.TempError;
            //}
            //#region APILogOutletRegistration
            //new ProcUpdateLogOnboardingReqResp(_dal).Call(new OnboardingLog
            //{
            //    APIID = _APIID,
            //    Method = "CheckOutletStatus",
            //    Request = Req,
            //    Response = Resp,

            //});
            //#endregion
            return OutReqRes;
        }
        public OutletAPIStatusUpdate CheckOutletServiceStatus(FintechAPIRequestModel fintechRequest, string SCode)
        {
            string Req = string.Empty;
            string Resp = string.Empty;
            var OutReqRes = new OutletAPIStatusUpdate
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            fintechRequest.UserID = 0;
            //fintechRequest.Token = apiSetting.instituteId;
            try
            {
                //Req = apiSetting.DMTServiceURL + "API/Outlet/ServiceStatus?" + JsonConvert.SerializeObject(fintechRequest);
                //Resp = AppWebRequest.O.PostJsonDataUsingHWR(apiSetting.DMTServiceURL + "API/Outlet/ServiceStatus/", fintechRequest);
                if (Validate.O.ValidateJSON(Resp ?? string.Empty))
                {
                    var _apiRes = JsonConvert.DeserializeObject<FintechAPIServiceResponse>(Resp);
                    if (_apiRes != null)
                    {
                        OutReqRes.Statuscode = _apiRes.Statuscode;
                        OutReqRes.Msg = _apiRes.Msg;
                        if (_apiRes.data != null)
                        {
                            OutReqRes.APIOutletID = fintechRequest.data.OutletID.ToString();
                            if (_apiRes.data.KYCStatus == 3)
                            {
                                OutReqRes.KYCStatus = UserStatus.ACTIVE;
                            }
                            if (_apiRes.data.KYCStatus == 2)
                            {
                                OutReqRes.KYCStatus = UserStatus.APPLIED;
                            }
                            if (_apiRes.data.KYCStatus.In(4, 5))
                            {
                                OutReqRes.KYCStatus = UserStatus.REJECTED;
                            }
                            OutReqRes.APIOutletStatus = OutReqRes.KYCStatus;
                        }
                        if (_apiRes.Statuscode == ErrorCodes.One)
                        {
                            OutReqRes.IsOTPRequired = _apiRes.data.IsOTPRequired;
                            if (SCode.Equals(ServiceCode.BBPSService) && _apiRes.data.ServiceCode.Equals(ServiceCode.BBPSService))
                            {
                                OutReqRes.BBPSID = OutReqRes.APIOutletStatus == UserStatus.REJECTED ? OutReqRes.APIOutletID : _apiRes.data.ServiceOutletID;
                                OutReqRes.BBPSStatus = OutReqRes.APIOutletStatus == UserStatus.REJECTED ? OutReqRes.APIOutletStatus : _apiRes.data.ServiceStatus;
                            }
                            else if (SCode.In(ServiceCode.AEPS, ServiceCode.MiniBank) && _apiRes.data.ServiceCode.In(ServiceCode.AEPS, ServiceCode.MiniBank))
                            {
                                OutReqRes.AEPSID = OutReqRes.APIOutletStatus == UserStatus.REJECTED ? OutReqRes.APIOutletID : _apiRes.data.ServiceOutletID;
                                OutReqRes.AEPSStatus = OutReqRes.APIOutletStatus == UserStatus.REJECTED ? OutReqRes.APIOutletStatus : _apiRes.data.ServiceStatus;
                                OutReqRes.AEPSURL = _apiRes.data.RedirectURL;
                            }
                            else if (SCode.Equals(ServiceCode.MoneyTransfer) && _apiRes.data.ServiceCode.Equals(ServiceCode.MoneyTransfer))
                            {
                                OutReqRes.DMTID = OutReqRes.APIOutletStatus == UserStatus.REJECTED ? OutReqRes.APIOutletID : _apiRes.data.ServiceOutletID;
                                OutReqRes.DMTStatus = OutReqRes.APIOutletStatus == UserStatus.REJECTED ? OutReqRes.APIOutletStatus : _apiRes.data.ServiceStatus;
                            }
                            else if (SCode.Equals(ServiceCode.PSAService) && _apiRes.data.ServiceCode.Equals(ServiceCode.PSAService))
                            {
                                OutReqRes.PSAID = OutReqRes.APIOutletStatus == UserStatus.REJECTED ? OutReqRes.APIOutletID : _apiRes.data.ServiceOutletID;
                                OutReqRes.PSAStatus = OutReqRes.APIOutletStatus == UserStatus.REJECTED ? OutReqRes.APIOutletStatus : _apiRes.data.ServiceStatus;
                            }
                        }
                        else
                        {
                            //OutReqRes.BBPSStatus = RndKYCOutLetStatus._Rejected;
                            //OutReqRes.AEPSStatus = RndKYCOutLetStatus._Rejected;
                            //OutReqRes.DMTStatus = RndKYCOutLetStatus._Rejected;
                            OutReqRes.Msg = string.IsNullOrEmpty(_apiRes.Msg) ? ErrorCodes.FailedToSubmit : _apiRes.Msg;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "CheckOutletStatus",
                    Error = ex.Message,
                });
                Resp = ex.Message + "_" + Resp;
                OutReqRes.Msg = ErrorCodes.TempError;
            }
            #region APILogOutletRegistration
            new ProcUpdateLogOnboardingReqResp(_dal).Call(new OnboardingLog
            {
                APIID = _APIID,
                Method = "CheckOutletStatus",
                Request = Req,
                Response = Resp,

            });
            #endregion
            return OutReqRes;
        }
    }
    public partial class MMWFintechML : IMoneyTransferAPIML
    {
        public MSenderCreateResp CreateSender(MTAPIRequest request)
        {
            var res = new MSenderCreateResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError,
                ErrorCode = ErrorCodes.Unknown_Error
            };
            var req = new MMWFintechReq
            {
                Username = apiSetting.Username,
                Password = apiSetting.Password,
                APIKey = apiSetting.APIKey,
                SenderMobile = request.SenderMobile
            };
            string response = string.Empty;
            var _URL = apiSetting.BaseURL + "/api/DMR242CheckSender";
            try
            {
                response = AppWebRequest.O.PostJsonDataUsingHWR(_URL, req);
                if (!string.IsNullOrEmpty(response))
                {

                    var _apiRes = JsonConvert.DeserializeObject<MMWFintechResp>(response);
                    if (_apiRes != null)
                    {
                        if (_apiRes.Message.Equals(MMWFintechCodes.MsgSenderNotFound) && _apiRes.Status.Equals(MMWFintechCodes.Msg200) && _apiRes.MessageStatus == false)
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
                                res.IsOTPResendAvailble = true;
                            }
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
            var res = new MSenderLoginResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = nameof(ErrorCodes.Unknown_Error).Replace("_", " "),
                ErrorCode = ErrorCodes.Unknown_Error
            };
            var req = new MMWFintechReq
            {
                Username = apiSetting.Username,
                Password = apiSetting.Password,
                APIKey = apiSetting.APIKey,
                SenderMobile = request.SenderMobile,
            };
            string response = string.Empty;
            var _URL = apiSetting.BaseURL + "/api/DMR242CheckSender";

            try
            {
                response = AppWebRequest.O.PostJsonDataUsingHWR(_URL, req);

                if (!string.IsNullOrEmpty(response))
                {
                    var _apiRes = JsonConvert.DeserializeObject<MMWFintechResp>(response);
                    if (_apiRes != null)
                    {

                        if (_apiRes.Message.Equals(MMWFintechCodes.MsgSenderNotFound) && _apiRes.Status.Equals(MMWFintechCodes.Msg200) && _apiRes.MessageStatus == false)
                        {
                            res.Msg = _apiRes.Message;
                            res.ErrorCode = DMTErrorCodes.Sender_Not_Found;
                            res.IsSenderNotExists = true;
                            res.Statuscode = ErrorCodes.One;
                        }
                        else if (_apiRes.Message.Equals(MMWFintechCodes.MsgSenderFound) && _apiRes.Status.Equals(MMWFintechCodes.Msg200) && _apiRes.MessageStatus == true)
                        {
                            res.Statuscode = ErrorCodes.One;
                            res.Msg = nameof(DMTErrorCodes.Detail_Found_Successfully).Replace("_", " ");
                            res.ErrorCode = DMTErrorCodes.Detail_Found_Successfully;
                            if (_apiRes.JsonResult != null)
                            {
                                res.SenderName = _apiRes.JsonResult.SenderInformation.FirstName + " " + _apiRes.JsonResult.SenderInformation.LastName;
                                res.ReferenceID = _apiRes.JsonResult.SenderInformation.Id.ToString();
                                res.RemainingLimit = _apiRes.JsonResult.RemainingLimit;
                                res.AvailbleLimit = _apiRes.JsonResult.TotalLimit;
                            }
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

            var senderCreateResp = new MSenderCreateResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = nameof(ErrorCodes.Unknown_Error).Replace("_", " "),
                ErrorCode = ErrorCodes.Unknown_Error
            };

            var req = new MMWFintechReq
            {
                Username = apiSetting.Username,
                Password = apiSetting.Password,
                APIKey = apiSetting.APIKey,
                SenderMobile = request.SenderMobile,
            };

            string response = string.Empty;

            var _URL = apiSetting.BaseURL + "/api/DMR242ResndRegistrationOtp";
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
            var res = new MSenderLoginResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = nameof(ErrorCodes.Unknown_Error).Replace("_", ""),
                ErrorCode = ErrorCodes.Unknown_Error
            };
            var senderRequest = (SenderRequest)new ProcGetSender(_dal).Call(request.SenderMobile);
            var req = new MMWFintechReq
            {
                Username = apiSetting.Username,
                Password = apiSetting.Password,
                APIKey = apiSetting.APIKey,
                MobileNo = senderRequest.MobileNo,
                FirstName = senderRequest.Name,
                LastName = senderRequest.LastName,
                Pincode = senderRequest.Pincode,
                OTC = request.OTP
            };

            string response = string.Empty;
            var _URL = apiSetting.BaseURL + "/api/DMR242AddSenderAPI";
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

            var res = new MSenderLoginResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            var req = new MMWFintechReq
            {
                Username = apiSetting.Username,
                Password = apiSetting.Password,
                APIKey = apiSetting.APIKey,
                BenName = request.mBeneDetail.BeneName,
                IFSC = request.mBeneDetail.IFSC,
                AccountNumber = request.mBeneDetail.AccountNo,
                BankName = request.mBeneDetail.BankName,
                CustomerNumber = request.SenderMobile,
            };
            string response = string.Empty;
            var _URL = apiSetting.BaseURL + "/api/DMR242AddBeneficiaryList";

            try
            {
                response = AppWebRequest.O.PostJsonDataUsingHWR(_URL, req);
                if (!string.IsNullOrEmpty(response))
                {
                    var _apiRes = JsonConvert.DeserializeObject<MMWFintechObjResp>(response);
                    if (_apiRes != null)
                    {
                        if (_apiRes.Message.Equals(MMWFintechCodes.MsgBenAdded) && _apiRes.Status.Equals(MMWFintechCodes.Msg200) && _apiRes.MessageStatus == true)
                        {
                            res.Statuscode = ErrorCodes.One;
                            res.Msg = nameof(DMTErrorCodes.Beneficiary_Added_Successfully);
                            res.ErrorCode = DMTErrorCodes.Beneficiary_Added_Successfully;
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
            var res = new MBeneficiaryResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = nameof(ErrorCodes.Unknown_Error).Replace("_", " "),
                ErrorCode = ErrorCodes.Unknown_Error
            };
            var req = new MMWFintechReq
            {
                Username = apiSetting.Username,
                Password = apiSetting.Password,
                APIKey = apiSetting.APIKey,
                SenderMobile = request.SenderMobile,
            };
            string response = string.Empty;
            var _URL = apiSetting.BaseURL + "/api/DMR242CheckSender";
            try
            {
                response = AppWebRequest.O.PostJsonDataUsingHWRTLS(_URL, req).Result;
                if (!string.IsNullOrEmpty(response))
                {
                    var _apiRes = JsonConvert.DeserializeObject<MMWFintechResp>(response);
                    if (_apiRes != null)
                    {
                        if (_apiRes.Message.Equals(MMWFintechCodes.MsgSenderFound) && _apiRes.Status.Equals(MMWFintechCodes.Msg200) && _apiRes.MessageStatus == true)
                        {
                            res.Statuscode = ErrorCodes.One;
                            res.Msg = nameof(ErrorCodes.Transaction_Successful);
                            res.ErrorCode = ErrorCodes.Transaction_Successful;
                            if (_apiRes.JsonResult != null)
                            {
                                if (_apiRes.JsonResult.BeneficiaryList != null)
                                {
                                    var Beneficiaries = new List<MBeneDetail>();
                                    foreach (var item in _apiRes.JsonResult.BeneficiaryList)
                                    {
                                        Beneficiaries.Add(new MBeneDetail
                                        {
                                            AccountNo = item.AccountNumber,
                                            BankName = item.BankName,
                                            IFSC = item.IFSC,
                                            BeneName = item.BenName,
                                            BeneID = item.Id,
                                            IsVerified = item.VerifyStatus
                                        });
                                        var param = new BenificiaryDetail
                                        {
                                            _SenderMobileNo = request.SenderMobile,
                                            _Name = item.BenName,
                                            _AccountNumber = item.AccountNumber,
                                            _MobileNo = request.SenderMobile,
                                            _IFSC = item.IFSC,
                                            _BankName = item.BankName,
                                            _EntryBy = request.UserID,
                                            _VerifyStatus = Convert.ToInt32(item.VerifyStatus),
                                            _APICode = _APICode,
                                            _BeneAPIID = Convert.ToInt32(item.Id)
                                        };
                                        var resdb = (BenificiaryModel)new ProcAddBenificiary(_dal).Call(param);
                                    }
                                    res.Beneficiaries = Beneficiaries;
                                    res.ErrorCode = ErrorCodes.Transaction_Successful;
                                }
                            }
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
            var senderCreateResp = new MSenderCreateResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = nameof(ErrorCodes.Unknown_Error).Replace("_", " "),
                ErrorCode = ErrorCodes.Unknown_Error
            };

            var resDB = (new ProcGetBenificiaryByBeneID(_dal).Call(new DMTReq { SenderNO = request.SenderMobile, BeneAPIID = Convert.ToInt32(request.mBeneDetail.BeneID) })) as BenificiaryDetail;


            var req = new MMWFintechReq
            {
                Username = apiSetting.Username,
                Password = apiSetting.Password,
                APIKey = apiSetting.APIKey,
                BenName = resDB._Name,
                IFSC = resDB._IFSC,
                AccountNumber = resDB._AccountNumber,
                BankName = resDB._BankName,
                CustomerNumber = request.SenderMobile,
            };
            string response = string.Empty;
            var _URL = apiSetting.BaseURL + "/api/DMR242VerifyBeneficiary";
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
            var mSenderLoginResponse = new MSenderLoginResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = nameof(ErrorCodes.Unknown_Error).Replace("_", " "),
                ErrorCode = ErrorCodes.Unknown_Error
            };
            var req = new MMWFintechReq
            {
                Username = apiSetting.Username,
                Password = apiSetting.Password,
                APIKey = apiSetting.APIKey,
                BenName = request.mBeneDetail.BeneName,
                IFSC = request.mBeneDetail.IFSC,
                AccountNumber = request.mBeneDetail.AccountNo,
                BankName = request.mBeneDetail.BankName,
                CustomerNumber = request.SenderMobile,
            };
            string response = string.Empty;
            var _URL = apiSetting.BaseURL + "/api/DMR242VerifyBeneficiary";
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
            var senderLoginResponse = new MSenderLoginResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError,
                ErrorCode = ErrorCodes.Unknown_Error
            };

            var req = new MMWFintechReq
            {
                Username = apiSetting.Username,
                Password = apiSetting.Password,
                APIKey = apiSetting.APIKey,
                BeneficiaryId = request.mBeneDetail.BeneID,
                SenderMobile = request.SenderMobile,
            };
            string response = string.Empty;
            var _URL = apiSetting.BaseURL + "/api/DMR242DeleteBeneficiary";
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
            var mSenderLoginResponse = new MSenderLoginResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = nameof(ErrorCodes.Unknown_Error).Replace("_", " "),
                ErrorCode = ErrorCodes.Unknown_Error
            };
            //var req = new IPayDMRReq
            //{
            //    token = apiSetting.Token,
            //    request = new IPayRequest
            //    {
            //        beneficiaryid = request.mBeneDetail.BeneID,
            //        remitterid = request.ReferenceID,
            //        otp = request.OTP,
            //        outletid = string.IsNullOrEmpty(request.APIOutletID) ? 1 : Convert.ToInt32(request.APIOutletID)
            //    }
            //};
            //string response = string.Empty;
            //var _URL = apiSetting.BaseURL + "beneficiary_remove_validate";
            //try
            //{
            //    response = AppWebRequest.O.PostJsonDataUsingHWRTLS(_URL, req).Result;
            //    if (!string.IsNullOrEmpty(response))
            //    {
            //        var _apiRes = JsonConvert.DeserializeObject<IPayDMRResp>(response);
            //        if (_apiRes != null)
            //        {
            //            if (_apiRes.statuscode.Equals("TXN"))
            //            {
            //                mSenderLoginResponse.Statuscode = ErrorCodes.One;
            //                mSenderLoginResponse.Msg = nameof(DMTErrorCodes.Beneficiary_Deactivated_or_Deleted);
            //                mSenderLoginResponse.ErrorCode = DMTErrorCodes.Beneficiary_Deactivated_or_Deleted;
            //            }
            //            else
            //            {
            //                mSenderLoginResponse.Statuscode = ErrorCodes.Minus1;
            //                mSenderLoginResponse.Msg = nameof(DMTErrorCodes.Transaction_Failed);
            //                mSenderLoginResponse.ErrorCode = DMTErrorCodes.Transaction_Failed;
            //            }
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    response = ex.Message + "|" + response;
            //    var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
            //    {
            //        ClassName = GetType().Name,
            //        FuncName = "RemoveBeneficiaryValidate",
            //        Error = ex.Message,
            //        LoginTypeID = LoginType.ApplicationUser,
            //        UserId = request.UserID
            //    });
            //}
            //new ProcUpdateLogDMRReqResp(_dal).Call(new DMTReqRes
            //{
            //    APIID = request.APIID,
            //    Method = "RemoveBeneficiaryValidate",
            //    RequestModeID = request.RequestMode,
            //    Request = _URL + "|" + JsonConvert.SerializeObject(req),
            //    Response = response,
            //    SenderNo = request.SenderMobile,
            //    UserID = request.UserID,
            //    TID = request.TransactionID
            //});
            return mSenderLoginResponse;
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

            var req = new MMWFintechReq
            {
                Username = apiSetting.Username,
                Password = apiSetting.Password,
                APIKey = apiSetting.APIKey,
                BenName = string.Empty,
                IFSC = request.mBeneDetail.IFSC,
                AccountNumber = request.mBeneDetail.AccountNo,
                BankName = request.mBeneDetail.BankName,
                CustomerNumber = request.SenderMobile,
            };
            string response = string.Empty;
            var _URL = apiSetting.BaseURL + "/api/DMR242VerifyBeneficiary";
            try
            {
                response = AppWebRequest.O.PostJsonDataUsingHWR(_URL, req);
                if (!string.IsNullOrEmpty(response))
                {
                    if (response != null)
                    {
                        var apiResp = JsonConvert.DeserializeObject<MMWFintechResp>(response);
                        if (apiResp != null)
                        {
                            if (apiResp.Message.Equals(MMWFintechCodes.MsgVerifyBen) && apiResp.Status.Equals(MMWFintechCodes.Msg200) && apiResp.MessageStatus == true)
                            {
                                if (apiResp.JsonResult != null)
                                {
                                    if (apiResp.JsonResult.ISValid.Equals("true"))
                                    {
                                        res.Statuscode = RechargeRespType.SUCCESS;
                                        res.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                                        res.ErrorCode = ErrorCodes.Transaction_Successful;
                                        res.BeneName = apiResp.JsonResult.BenName;
                                        res.LiveID = string.Empty;
                                        res.VendorID = string.Empty;
                                    }
                                    else if (apiResp.JsonResult.ISValid.Equals("false"))
                                    {
                                        res.Statuscode = ErrorCodes.Minus1;
                                        res.Msg = nameof(ErrorCodes.FAILED);
                                        res.ErrorCode = ErrorCodes.Minus1;
                                    }
                                }
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
            var res = new DMRTransactionResponse
            {
                Statuscode = RechargeRespType.PENDING,
                Msg = RechargeRespType._PENDING,
                VendorID = "",
                LiveID = ""
            };
            var req = new
            {
                Username = apiSetting.Username,
                Password = apiSetting.Password,
                APIKey = apiSetting.APIKey,
                BenId = request.mBeneDetail.BeneID,
                unique_request_number = request.TID.ToString(),
                payment_mode = request.TransMode,
                amount = Convert.ToDecimal(request.Amount),
                mobileNumber = request.SenderMobile
            };
            string response = string.Empty;

            var _URL = apiSetting.BaseURL + "/api/DMR242Transfer";

            try
            {
                response = AppWebRequest.O.PostJsonDataUsingHWR(_URL, req);

                if (!string.IsNullOrEmpty(response))
                {
                    var _apiRes = JsonConvert.DeserializeObject<MMWFintechResp>(response);
                    if (_apiRes != null)
                    {
                        if (_apiRes.JsonResult != null)
                        {
                            if (_apiRes.JsonResult.TransactionStatus.Equals(MMWFintechCodes.MsgSuccess) && _apiRes.Status.Equals(MMWFintechCodes.Msg200) && _apiRes.MessageStatus == true)
                            {
                                res.Statuscode = RechargeRespType.SUCCESS;
                                res.Msg = _apiRes.Message;
                                res.ErrorCode = 0;
                                res.VendorID = _apiRes.JsonResult.SystemTransactionId;
                                res.LiveID = _apiRes.JsonResult.Uniquetransactionreference;
                                res.BeneName = request.mBeneDetail.BeneName;

                            }
                            else if (_apiRes.JsonResult.TransactionStatus.Equals(MMWFintechCodes.MsgFailed) && _apiRes.Status.Equals(MMWFintechCodes.Msg200) && _apiRes.MessageStatus == true)
                            {
                                res.Statuscode = RechargeRespType.FAILED;
                                res.Msg = _apiRes.Message;
                                res.ErrorCode = 0;
                                res.VendorID = _apiRes.JsonResult.SystemTransactionId;
                                res.LiveID = _apiRes.JsonResult.Uniquetransactionreference;
                                res.BeneName = request.mBeneDetail.BeneName;
                            }
                            else if (_apiRes.JsonResult.TransactionStatus.Equals(MMWFintechCodes.MsgPending) && _apiRes.Status.Equals(MMWFintechCodes.Msg200) && _apiRes.MessageStatus == true)
                            {
                                res.Statuscode = RechargeRespType.PENDING;
                                res.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                                res.ErrorCode = ErrorCodes.Transaction_Successful;
                                res.VendorID = _apiRes.JsonResult.SystemTransactionId;
                            }
                            else
                            {
                                IErrorCodeML errorCodeML = new ErrorCodeML(_accessor, _env, false);
                                var eFromDB = errorCodeML.GetAPIErrorCodeDescription(request.APIGroupCode, _apiRes.Status.ToString());
                                if (!string.IsNullOrEmpty(eFromDB.Code))
                                {
                                    res.Statuscode = eFromDB.Status;
                                    res.Msg = eFromDB.Error.Replace("{REPLACE}", _apiRes.Status.ToString());
                                    res.ErrorCode = Convert.ToInt32(eFromDB.Code.Trim());
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
        public DMRTransactionResponse GetTransactionStatus(int TID, int UserID, int APIID, int RequestMode)
        {
            //string TransactionID,  int UserID, int APIID, string VendorID
            var res = new DMRTransactionResponse
            {
                Statuscode = RechargeRespType.PENDING,
                Msg = nameof(RechargeRespType.PENDING),
                ErrorCode = ErrorCodes.Request_Accpeted
            };
            var req = new
            {
                Username = apiSetting.Username,
                Password = apiSetting.Password,
                APIKey = apiSetting.APIKey,
                ClientTransactionId = TID.ToString()
            };
            string response = string.Empty;
            var _URL = apiSetting.BaseURL + "/api/DMR242TransactionStatusByClientId";
            try
            {
                response = AppWebRequest.O.PostJsonDataUsingHWR(_URL, req);
                if (response != null)
                {
                    var rpFintechSts = JsonConvert.DeserializeObject<MMWFintechResp>(response);
                    if (rpFintechSts != null)
                    {
                        if (rpFintechSts.JsonResult != null)
                        {
                            if (rpFintechSts.JsonResult.TransactionStatus == MMWFintechCodes.MsgSuccess)
                            {
                                res.VendorID = rpFintechSts.JsonResult.SystemTransactionId;
                                res.LiveID = rpFintechSts.JsonResult.Uniquetransactionreference;
                                res.Statuscode = RechargeRespType.SUCCESS;
                                res.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                                res.ErrorCode = ErrorCodes.Transaction_Successful;
                            }
                            if (rpFintechSts.JsonResult.TransactionStatus == MMWFintechCodes.MsgSuccess)
                            {
                                res.VendorID = rpFintechSts.JsonResult.SystemTransactionId;
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