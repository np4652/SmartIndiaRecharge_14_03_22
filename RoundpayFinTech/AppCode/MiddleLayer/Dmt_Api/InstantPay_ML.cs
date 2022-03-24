using Fintech.AppCode.Configuration;
using Fintech.AppCode.DB;
using Fintech.AppCode.HelperClass;
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
using RoundpayFinTech_bA.AppCode.ThirdParty.BillAvenue;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Validators;

namespace RoundpayFinTech.AppCode.MiddleLayer.Dmt_Api
{
    public partial class InstantPay_ML
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        private readonly IConfiguration Configuration;

        private readonly string _APICode;
        private readonly int _APIID;
        private readonly IDAL _dal;
        private readonly IPaySetting apiSetting;


        public InstantPay_ML(IHttpContextAccessor accessor, IHostingEnvironment env, string APICode, int APIID, IDAL dal)
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
        public IPaySetting AppSetting()
        {
            var setting = new IPaySetting();
            try
            {
                setting = new IPaySetting
                {
                    Token = Configuration["DMR:" + _APICode + ":Token"],
                    BaseURL = Configuration["DMR:" + _APICode + ":BaseURL"]
                };
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "IPaySetting:" + (_APICode ?? string.Empty),
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
    public partial class InstantPay_ML : IMoneyTransferAPIML
    {
        public MSenderCreateResp CreateSender(MTAPIRequest request)
        {
            var res = new MSenderCreateResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError,
                ErrorCode = ErrorCodes.Unknown_Error
            };
            var req = new IPayDMRReq
            {
                token = apiSetting.Token,
                request = new IPayRequest
                {
                    mobile = request.SenderMobile,
                    name = request.FirstName,
                    surname = request.LastName,
                    pincode = request.Pincode.ToString(),
                    outletid = string.IsNullOrEmpty(request.APIOutletID) ? 1 : Convert.ToInt32(request.APIOutletID)
                }
            };
            string response = string.Empty;
            var _URL = apiSetting.BaseURL + "remitter";
            try
            {
                response = AppWebRequest.O.PostJsonDataUsingHWR(_URL, req);
                if (!string.IsNullOrEmpty(response))
                {
                    var _apiRes = JsonConvert.DeserializeObject<IPayDMRResp>(response);
                    if (_apiRes != null)
                    {
                        if (_apiRes.statuscode.Equals("TXN"))
                        {
                            res.Statuscode = ErrorCodes.One;
                            if (_apiRes.data.remitter.is_verified == ErrorCodes.One)
                            {
                                res.Msg = nameof(DMTErrorCodes.Sender_Created_Successfully);
                                res.ErrorCode = DMTErrorCodes.Sender_Created_Successfully;
                            }
                            else
                            {
                                res.Msg = _apiRes.status;
                                res.ErrorCode = DMTErrorCodes.OTP_for_verification_has_been_sent_successfully;
                                res.IsOTPGenerated = true;
                                res.ReferenceID = _apiRes.data.remitter.id;
                                new ProcUpdateSender(_dal).Call(new SenderRequest
                                {
                                    Name = request.FirstName + " " + request.LastName,
                                    MobileNo = request.SenderMobile,
                                    Pincode = request.Pincode.ToString(),
                                    Address = request.Address,
                                    City = request.City,
                                    StateID = request.StateID,
                                    AadharNo = request.AadharNo,
                                    Dob = request.DOB,
                                    UserID = request.UserID,
                                });
                            }
                        }
                        else if (_apiRes.statuscode.Equals("ERR"))
                        {
                            res.Statuscode = ErrorCodes.Minus1;
                            res.Msg = _apiRes.status;
                            res.ErrorCode = ErrorCodes.Unknown_Error;
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
            var req = new IPayDMRReq
            {
                token = apiSetting.Token,
                request = new IPayRequest
                {
                    mobile = request.SenderMobile,
                    outletid = string.IsNullOrEmpty(request.APIOutletID) ? 1 : Convert.ToInt32(request.APIOutletID)
                }
            };
            string response = string.Empty;
            var _URL = apiSetting.BaseURL + "remitter_details";

            try
            {
                response = AppWebRequest.O.PostJsonDataUsingHWR(_URL, req);

                if (!string.IsNullOrEmpty(response))
                {
                    var _apiRes = JsonConvert.DeserializeObject<IPayDMRResp>(response);
                    if (_apiRes != null)
                    {
                        if (!string.IsNullOrEmpty(_apiRes.statuscode))
                        {
                            if (_apiRes.statuscode.Equals("RNF"))
                            {
                                res.Statuscode = ErrorCodes.One;
                                res.Msg = _apiRes.status;
                                res.ErrorCode = DMTErrorCodes.Sender_Not_Found;
                                res.IsSenderNotExists = true;
                            }
                            else if (_apiRes.statuscode.Equals("TXN") && _apiRes.data.remitter != null)
                            {

                                res.Statuscode = ErrorCodes.One;
                                if (_apiRes.data.remitter.is_verified == 1)
                                {
                                    res.Msg = nameof(DMTErrorCodes.Detail_Found_Successfully).Replace("_", " ");
                                    res.ErrorCode = DMTErrorCodes.Detail_Found_Successfully;
                                    res.SenderName = _apiRes.data.remitter.name;
                                    res.ReferenceID = _apiRes.data.remitter.id;
                                    res.RemainingLimit = Convert.ToDecimal(_apiRes.data.remitter_limit[0].limit.remaining);
                                    res.AvailbleLimit = Convert.ToDecimal(_apiRes.data.remitter_limit[0].limit.total);
                                }
                                else if (_apiRes.data.remitter.is_verified == 0)
                                {
                                    res.Statuscode = ErrorCodes.One;
                                    res.IsOTPGenerated = true;
                                    res.ReferenceID = _apiRes.data.remitter.id;
                                    res.Msg = _apiRes.status;
                                    res.ErrorCode = DMTErrorCodes.OTP_for_verification_has_been_sent_successfully;
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


            //var req = new DMTAPIRequest
            //{
            //    UserID = 0,
            //    Token = apiSetting.instituteId,
            //    OutletID = Validate.O.IsNumeric(request.APIOutletID ?? string.Empty) ? Convert.ToInt32(request.APIOutletID) : 0,
            //    SPKey = request.APIOpCode,
            //    SenderMobile = request.SenderMobile,
            //    ReferenceID = request.ReferenceID
            //};
            //string response = string.Empty;
            //var _URL = apiSetting.DMTServiceURL + "APIMoneyTransfer/SenderResendOTP";
            //try
            //{
            //    response = AppWebRequest.O.PostJsonDataUsingHWRTLS(_URL, req).Result;
            //    if (!string.IsNullOrEmpty(response))
            //    {
            //        var _apiRes = JsonConvert.DeserializeObject<SenderCreateResponse>(response);
            //        if (_apiRes != null)
            //        {
            //            senderCreateResp.Statuscode = _apiRes.Statuscode;
            //            senderCreateResp.Msg = _apiRes.Message;
            //            senderCreateResp.IsOTPGenerated = _apiRes.IsOTPRequired;
            //            senderCreateResp.IsOTPResendAvailble = _apiRes.IsOTPResendAvailble;
            //            senderCreateResp.ErrorCode = _apiRes.ErrorCode;
            //            senderCreateResp.ReferenceID = _apiRes.ReferenceID;
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    response = ex.Message + "|" + response;
            //    var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
            //    {
            //        ClassName = GetType().Name,
            //        FuncName = "SenderResendOTP",
            //        Error = ex.Message,
            //        LoginTypeID = LoginType.ApplicationUser,
            //        UserId = request.UserID
            //    });
            //}
            //new ProcUpdateLogDMRReqResp(_dal).Call(new DMTReqRes
            //{
            //    APIID = request.APIID,
            //    Method = "SenderResendOTP",
            //    RequestModeID = request.RequestMode,
            //    Request = _URL + JsonConvert.SerializeObject(req),
            //    Response = response,
            //    SenderNo = request.SenderMobile,
            //    UserID = request.UserID,
            //    TID = request.TransactionID
            //});
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

            var req = new IPayDMRReq
            {
                token = apiSetting.Token,
                request = new IPayRequest
                {
                    remitterid = request.ReferenceID,
                    mobile = request.SenderMobile,
                    otp = request.OTP,
                    outletid = string.IsNullOrEmpty(request.APIOutletID) ? 1 : Convert.ToInt32(request.APIOutletID)
                }
            };
            string response = string.Empty;
            var _URL = apiSetting.BaseURL + "remitter_validate";
            try
            {
                response = AppWebRequest.O.PostJsonDataUsingHWR(_URL, req);
                if (!string.IsNullOrEmpty(response))
                {
                    var _apiRes = JsonConvert.DeserializeObject<IPayDMRResp>(response);
                    if (_apiRes != null)
                    {
                        if (_apiRes.statuscode.Equals("TXN"))
                        {
                            res.Statuscode = ErrorCodes.One;
                            res.Msg = nameof(DMTErrorCodes.Sender_Created_Successfully);
                            res.ErrorCode = DMTErrorCodes.Sender_Created_Successfully;
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
            var req = new IPayDMRReq
            {
                token = apiSetting.Token,
                request = new IPayRequest
                {
                    remitterid = request.ReferenceID,
                    name = request.mBeneDetail.BeneName,
                    mobile = request.mBeneDetail.MobileNo,
                    ifsc = request.mBeneDetail.IFSC,
                    account = request.mBeneDetail.AccountNo,
                    outletid = string.IsNullOrEmpty(request.APIOutletID) ? 1 : Convert.ToInt32(request.APIOutletID)
                }
            };
            string response = string.Empty;
            var _URL = apiSetting.BaseURL + "beneficiary_register";

            try
            {
                response = AppWebRequest.O.PostJsonDataUsingHWR(_URL, req);
                if (!string.IsNullOrEmpty(response))
                {
                    var _apiRes = JsonConvert.DeserializeObject<IPayCBResp>(response);
                    if (_apiRes != null)
                    {
                        if (_apiRes.statuscode.Equals("TXN") && _apiRes.data.beneficiary != null)
                        {
                            res.Statuscode = ErrorCodes.One;
                            res.Msg = nameof(DMTErrorCodes.Beneficiary_Added_Successfully);
                            res.ErrorCode = DMTErrorCodes.Beneficiary_Added_Successfully;
                            res.BeneID = _apiRes.data.beneficiary.id;
                            res.ReferenceID = _apiRes.data.remitter.id;
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
            var req = new IPayDMRReq
            {
                token = apiSetting.Token,
                request = new IPayRequest
                {
                    mobile = request.SenderMobile,
                    outletid = string.IsNullOrEmpty(request.APIOutletID) ? 1 : Convert.ToInt32(request.APIOutletID)
                }
            };
            string response = string.Empty;
            var _URL = apiSetting.BaseURL + "remitter_details";
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
                            res.Statuscode = ErrorCodes.One;
                            res.Msg = _apiRes.status;
                            res.ErrorCode = ErrorCodes.Transaction_Successful;
                            if (_apiRes.data.beneficiary != null)
                            {

                                var Beneficiaries = new List<MBeneDetail>();
                                foreach (var item in _apiRes.data.beneficiary)
                                {
                                    Beneficiaries.Add(new MBeneDetail
                                    {
                                        AccountNo = item.account,
                                        BankName = item.bank,
                                        IFSC = item.ifsc,
                                        BeneName = item.name,
                                        BeneID = item.id,
                                        IsVerified = true//item.isVerified == "N" ? false : true
                                    });
                                }
                                res.Beneficiaries = Beneficiaries;
                                res.ErrorCode = ErrorCodes.Transaction_Successful;
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
            var req = new IPayDMRReq
            {
                token = apiSetting.Token,
                request = new IPayRequest
                {
                    mobile = request.SenderMobile,
                    outletid = Convert.ToInt32(request.APIOutletID)
                }
            };
            string response = string.Empty;
            var _URL = apiSetting.BaseURL + "remitter_details";
            try
            {
                response = AppWebRequest.O.PostJsonDataUsingHWRTLS(_URL, req).Result;
                if (!string.IsNullOrEmpty(response))
                {
                    var _apiRes = JsonConvert.DeserializeObject<SenderCreateResponse>(response);
                    if (_apiRes != null)
                    {
                        senderCreateResp.Statuscode = _apiRes.Statuscode;
                        senderCreateResp.Msg = _apiRes.Message;
                        senderCreateResp.ErrorCode = _apiRes.ErrorCode;
                        senderCreateResp.IsOTPGenerated = _apiRes.IsOTPRequired;
                        senderCreateResp.IsOTPResendAvailble = _apiRes.IsOTPResendAvailble;
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
            var req = new IPayDMRReq
            {
                token = apiSetting.Token,
                request = new IPayRequest
                {
                    mobile = request.SenderMobile,
                    outletid = Convert.ToInt32(request.APIOutletID)
                }
            };
            string response = string.Empty;
            var _URL = apiSetting.BaseURL + "remitter_details";
            try
            {
                response = AppWebRequest.O.PostJsonDataUsingHWRTLS(_URL, req).Result;
                if (!string.IsNullOrEmpty(response))
                {
                    var _apiRes = JsonConvert.DeserializeObject<DMTAPIResponse>(response);
                    if (_apiRes != null)
                    {
                        mSenderLoginResponse.Statuscode = _apiRes.Statuscode;
                        mSenderLoginResponse.Msg = _apiRes.Message;
                        mSenderLoginResponse.ErrorCode = _apiRes.ErrorCode;
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
            var req = new IPayDMRReq
            {
                token = apiSetting.Token,
                request = new IPayRequest
                {
                    beneficiaryid = request.mBeneDetail.BeneID,
                    remitterid = request.ReferenceID,
                    outletid = string.IsNullOrEmpty(request.APIOutletID) ? 1 : Convert.ToInt32(request.APIOutletID)
                }
            };
            string response = string.Empty;
            var _URL = apiSetting.BaseURL + "beneficiary_remove";
            try
            {
                response = AppWebRequest.O.PostJsonDataUsingHWRTLS(_URL, req).Result;
                if (!string.IsNullOrEmpty(response))
                {
                    var _apiRes = JsonConvert.DeserializeObject<IPayDMRResp>(response);
                    if (_apiRes != null)
                    {
                        if (_apiRes.statuscode.Equals("TXN") && _apiRes.data != null)
                        {
                            senderLoginResponse.Statuscode = ErrorCodes.One;
                            senderLoginResponse.IsOTPGenerated = true;
                            senderLoginResponse.Msg = nameof(DMTErrorCodes.OTP_for_verification_has_been_sent_successfully);
                            senderLoginResponse.ErrorCode = DMTErrorCodes.OTP_for_verification_has_been_sent_successfully;
                        }
                        else if (_apiRes.statuscode.Equals("ERR") && _apiRes.data == null)
                        {
                            senderLoginResponse.Statuscode = ErrorCodes.Minus1;
                            senderLoginResponse.Msg = _apiRes.status;
                            senderLoginResponse.ErrorCode = DMTErrorCodes.OTP_limit_exceeds;
                        }
                        else
                        {
                            senderLoginResponse.Statuscode = ErrorCodes.Minus1;
                            senderLoginResponse.Msg = nameof(DMTErrorCodes.Transaction_Failed);
                            senderLoginResponse.ErrorCode = DMTErrorCodes.Transaction_Failed;
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
            var req = new IPayDMRReq
            {
                token = apiSetting.Token,
                request = new IPayRequest
                {
                    beneficiaryid = request.mBeneDetail.BeneID,
                    remitterid = request.ReferenceID,
                    otp = request.OTP,
                    outletid = string.IsNullOrEmpty(request.APIOutletID) ? 1 : Convert.ToInt32(request.APIOutletID)
                }
            };
            string response = string.Empty;
            var _URL = apiSetting.BaseURL + "beneficiary_remove_validate";
            try
            {
                response = AppWebRequest.O.PostJsonDataUsingHWRTLS(_URL, req).Result;
                if (!string.IsNullOrEmpty(response))
                {
                    var _apiRes = JsonConvert.DeserializeObject<IPBeneDelVeri>(response);
                    if (_apiRes != null)
                    {
                        if (_apiRes.statuscode.Equals("TXN"))
                        {
                            mSenderLoginResponse.Statuscode = ErrorCodes.One;
                            mSenderLoginResponse.Msg = nameof(DMTErrorCodes.Beneficiary_Deactivated_or_Deleted);
                            mSenderLoginResponse.ErrorCode = DMTErrorCodes.Beneficiary_Deactivated_or_Deleted;
                        }
                        else if (_apiRes.statuscode.Equals("ERR"))
                        {
                            mSenderLoginResponse.Statuscode = ErrorCodes.Minus1;
                            mSenderLoginResponse.Msg = nameof(DMTErrorCodes.Transaction_Failed);
                            mSenderLoginResponse.ErrorCode = DMTErrorCodes.Transaction_Failed;
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

            var res = new DMRTransactionResponse
            {
                Statuscode = RechargeRespType.PENDING,
                Msg = RechargeRespType._PENDING,
                VendorID = "",
                LiveID = ""
            };

            var req = new IPayPayoutRequest
            {
                token = apiSetting.Token,
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
                    latitude = (string.IsNullOrEmpty(request.Lattitude) ? "26.8678" : request.Lattitude).PadRight(7, '0'),
                    longitude = (string.IsNullOrEmpty(request.Longitude) ? "80.9832" : request.Longitude).PadRight(7, '0'),
                    endpoint_ip = request.IPAddress,
                    otp_auth = "0",
                    otp = string.Empty,
                    remarks = "verification",
                    alert_mobile = request.SenderMobile,
                    alert_email = request.EmailID
                }
            };
            string response = string.Empty;
            var _URL = "https://www.instantpay.in/ws/payouts/direct";
            try
            {
                response = AppWebRequest.O.PostJsonDataUsingHWR(_URL, req);
                if (!string.IsNullOrEmpty(response))
                {
                    if (response != null)
                    {
                        var apiResp = JsonConvert.DeserializeObject<IPayPayoutResponse>(response);
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
                            else if (apiResp.statuscode.Equals("ERR"))
                            {
                                IErrorCodeML errorCodeML = new ErrorCodeML(_accessor, _env, false);
                                var eFromDB = errorCodeML.GetAPIErrorCodeDescription(request.APIGroupCode, apiResp.statuscode);
                                if (!string.IsNullOrEmpty(eFromDB.Code))
                                {
                                    res.Statuscode = eFromDB.Status;
                                    res.Msg = eFromDB.Error;
                                    res.ErrorCode = Convert.ToInt32(eFromDB.Code.Trim());
                                    res.LiveID = res.Msg;
                                }
                            }
                            else
                            {
                                IErrorCodeML errorCodeML = new ErrorCodeML(_accessor, _env, false);
                                var eFromDB = errorCodeML.GetAPIErrorCodeDescription(request.APIGroupCode, apiResp.statuscode);
                                if (!string.IsNullOrEmpty(eFromDB.Code))
                                {
                                    res.Statuscode = eFromDB.Status;
                                    res.Msg = eFromDB.Error;
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
            var req = new IPayDMRReq
            {
                token = apiSetting.Token,
                request = new IPayRequest
                {
                    remittermobile = request.SenderMobile,
                    beneficiaryid = request.mBeneDetail.BeneID,
                    agentid = "TID" + request.TID.ToString(),
                    amount = request.Amount.ToString(),
                    mode = request.TransMode,
                    outletid = string.IsNullOrEmpty(request.APIOutletID) ? 1 : Convert.ToInt32(request.APIOutletID)
                }
            };
            string response = string.Empty;
            var _URL = apiSetting.BaseURL + "transfer";
            try
            {
                response = AppWebRequest.O.PostJsonDataUsingHWR(_URL, req);

                if (!string.IsNullOrEmpty(response))
                {
                    var _apiRes = JsonConvert.DeserializeObject<IPayDMRResp>(response);
                    if (_apiRes != null)
                    {
                        if (_apiRes.statuscode.Equals("TXN"))
                        {
                            res.Statuscode = RechargeRespType.SUCCESS;
                            res.Msg = _apiRes.status;
                            res.ErrorCode = 0;
                            if (_apiRes.data != null)
                            {
                                res.VendorID = _apiRes.data.ipay_id;
                                res.LiveID = _apiRes.data.ipay_id;
                                res.BeneName = request.mBeneDetail.BeneName;
                            }
                        }
                        else if (_apiRes.statuscode.Equals("ERR"))
                        {
                            IErrorCodeML errorCodeML = new ErrorCodeML(_accessor, _env, false);
                            var eFromDB = errorCodeML.GetAPIErrorCodeDescription(request.APIGroupCode, _apiRes.status);
                            if (!string.IsNullOrEmpty(eFromDB.Code))
                            {
                                res.Statuscode = eFromDB.Status;
                                res.Msg = eFromDB.Error.Replace("{REPLACE}", _apiRes.status);
                                res.ErrorCode = Convert.ToInt32(eFromDB.Code.Trim());
                                res.LiveID = res.Msg;
                            }
                            res.Statuscode = res.Statuscode == 0 ? RechargeRespType.PENDING : res.Statuscode;
                        }
                        else if (_apiRes.statuscode.Equals("TUP"))
                        {
                            res.Statuscode = RechargeRespType.PENDING;
                            res.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                            res.ErrorCode = ErrorCodes.Transaction_Successful;
                            if (_apiRes.data != null)
                            {
                                res.VendorID = _apiRes.data.ipay_id;
                            }
                        }
                        else
                        {
                            IErrorCodeML errorCodeML = new ErrorCodeML(_accessor, _env, false);
                            var eFromDB = errorCodeML.GetAPIErrorCodeDescription(request.APIGroupCode, _apiRes.statuscode);
                            if (!string.IsNullOrEmpty(eFromDB.Code))
                            {
                                res.Statuscode = eFromDB.Status;
                                res.Msg = eFromDB.Error.Replace("{REPLACE}", _apiRes.status);
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
        public DMRTransactionResponse GetTransactionStatus(int TID, string TransactionID, int RequestMode, int UserID, int APIID, string VendorID)
        {
            var res = new DMRTransactionResponse
            {
                Statuscode = RechargeRespType.PENDING,
                Msg = nameof(RechargeRespType.PENDING),
                ErrorCode = ErrorCodes.Request_Accpeted
            };
            var req = new IPayDMRReq
            {
                token = apiSetting.Token,
                request = new IPayRequest
                {
                    mobile = "",
                    outletid = 0
                }
            };
            string response = string.Empty;
            var _URL = apiSetting.BaseURL + "remitter_details";
            try
            {
                response = "";// AppWebRequest.O.CallUsingWebClient_GET(URL);
                //if (resp != null)
                //{
                //    var rpFintechSts = JsonConvert.DeserializeObject<DMTCheckStatusResponse>(resp);
                //    if (rpFintechSts != null)
                //    {
                //        if (rpFintechSts.Status > 0)
                //        {
                //            if (rpFintechSts.Status == RechargeRespType.SUCCESS)
                //            {
                //                res.VendorID = rpFintechSts.TransactionID;
                //                res.LiveID = rpFintechSts.LiveID;
                //                res.Statuscode = RechargeRespType.SUCCESS;
                //                res.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                //                res.ErrorCode = ErrorCodes.Transaction_Successful;
                //                res.BeneName = rpFintechSts.BeneName;
                //            }
                //            if (rpFintechSts.Status == RechargeRespType.FAILED)
                //            {
                //                res.VendorID = rpFintechSts.TransactionID;
                //                res.LiveID = rpFintechSts.Message.Contains("suffi") ? ErrorCodes.Down : rpFintechSts.Message;
                //                res.Statuscode = RechargeRespType.FAILED;
                //                res.Msg = nameof(ErrorCodes.Unknown_Error).Replace("_", " ");
                //                res.ErrorCode = ErrorCodes.Unknown_Error;
                //            }
                //        }
                //    }
                //}
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