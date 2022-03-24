using Fintech.AppCode.Configuration;
using Fintech.AppCode.DB;
using Fintech.AppCode.HelperClass;
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
using RoundpayFinTech.AppCode.ThirdParty.Roundpay;
using System;
using System.Globalization;
using System.IO;
using System.Text;
using Validators;

namespace RoundpayFinTech.AppCode.MiddleLayer.Dmt_Api
{
    public partial class FintechAPIML
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        private readonly IConfiguration Configuration;

        private readonly string _APICode;
        private readonly int _APIID;
        private readonly IDAL _dal;
        private readonly FintechAPISetting apiSetting;
        public FintechAPIML(IHttpContextAccessor accessor, IHostingEnvironment env, string APICode, int APIID, IDAL dal)
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
                    OutletID = Convert.ToInt32(Configuration["DMR:" + _APICode + ":OutletID"])
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
        public OutletAPIStatusUpdate SaveOutletOfAPIUser(FintechAPIRequestModel fintechRequest)
        {
            string Req = string.Empty;
            string Resp = string.Empty;
            var OutReqRes = new OutletAPIStatusUpdate
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            fintechRequest.UserID = apiSetting.UserID;
            fintechRequest.Token = apiSetting.Token;
            try
            {
                Req = apiSetting.BaseURL + "API/Outlet/Register?" + JsonConvert.SerializeObject(fintechRequest);
                Resp = AppWebRequest.O.PostJsonDataUsingHWR(apiSetting.BaseURL + "API/Outlet/Register/", fintechRequest);
                if (Validate.O.ValidateJSON(Resp ?? string.Empty))
                {
                    var _apiRes = JsonConvert.DeserializeObject<FintechAPIResponse>(Resp);
                    if (_apiRes != null)
                    {
                        if (_apiRes.Statuscode == ErrorCodes.One)
                        {
                            OutReqRes.Statuscode = ErrorCodes.One;
                            OutReqRes.Msg = ErrorCodes.OutletRegistered;
                            OutReqRes.APIOutletID = _apiRes.data.OutletID.ToString();
                            OutReqRes.APIOutletStatus = RndKYCOutLetStatus._Pending;
                            OutReqRes.KYCStatus = RndKYCOutLetStatus._Pending;
                            OutReqRes.BBPSID = _apiRes.data.OutletID.ToString();
                            OutReqRes.BBPSStatus = RndKYCOutLetStatus._Pending;
                            OutReqRes.AEPSID = _apiRes.data.OutletID.ToString();
                            OutReqRes.AEPSStatus = RndKYCOutLetStatus._Pending;
                            OutReqRes.DMTID = _apiRes.data.OutletID.ToString();
                            OutReqRes.DMTStatus = RndKYCOutLetStatus._Pending;
                            OutReqRes.PSAID = string.Empty;
                            OutReqRes.PSAStatus = RndKYCOutLetStatus._NotApplied;
                        }
                        else
                        {
                            OutReqRes.APIOutletStatus = RndKYCOutLetStatus._Rejected;
                            OutReqRes.BBPSStatus = RndKYCOutLetStatus._Rejected;
                            OutReqRes.AEPSStatus = RndKYCOutLetStatus._Rejected;
                            OutReqRes.DMTStatus = RndKYCOutLetStatus._Rejected;
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
                    FuncName = "SaveOutletOfAPIUser",
                    Error = ex.Message,
                });
                Resp = ex.Message + "_" + Resp;
                OutReqRes.Msg = ErrorCodes.TempError;
            }
            #region APILogOutletRegistration
            new ProcUpdateLogOnboardingReqResp(_dal).Call(new OnboardingLog
            {
                APIID = _APIID,
                Method = "SaveOutletOfAPIUser",
                Request = Req,
                Response = Resp,

            });
            #endregion
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
            fintechRequest.UserID = apiSetting.UserID;
            fintechRequest.Token = apiSetting.Token;
            try
            {
                Req = apiSetting.BaseURL + "API/Outlet/Update?" + JsonConvert.SerializeObject(fintechRequest);
                Resp = AppWebRequest.O.PostJsonDataUsingHWR(apiSetting.BaseURL + "API/Outlet/Update/", fintechRequest);
                if (Validate.O.ValidateJSON(Resp ?? string.Empty))
                {
                    var _apiRes = JsonConvert.DeserializeObject<FintechAPIResponse>(Resp);
                    if (_apiRes != null)
                    {
                        if (_apiRes.Statuscode == ErrorCodes.One)
                        {
                            OutReqRes.Statuscode = ErrorCodes.One;
                            OutReqRes.Msg = ErrorCodes.OutletRegistered;
                            OutReqRes.APIOutletID = _apiRes.data.OutletID.ToString();
                            OutReqRes.APIOutletStatus = RndKYCOutLetStatus._Pending;
                            OutReqRes.KYCStatus = RndKYCOutLetStatus._Pending;
                            OutReqRes.BBPSID = _apiRes.data.OutletID.ToString();
                            OutReqRes.BBPSStatus = RndKYCOutLetStatus._Pending;
                            OutReqRes.AEPSID = _apiRes.data.OutletID.ToString();
                            OutReqRes.AEPSStatus = RndKYCOutLetStatus._Pending;
                            OutReqRes.DMTID = _apiRes.data.OutletID.ToString();
                            OutReqRes.DMTStatus = RndKYCOutLetStatus._Pending;
                            OutReqRes.PSAID = string.Empty;
                            OutReqRes.PSAStatus = RndKYCOutLetStatus._NotApplied;
                        }
                        else
                        {
                            OutReqRes.APIOutletStatus = RndKYCOutLetStatus._Rejected;
                            OutReqRes.BBPSStatus = RndKYCOutLetStatus._Rejected;
                            OutReqRes.AEPSStatus = RndKYCOutLetStatus._Rejected;
                            OutReqRes.DMTStatus = RndKYCOutLetStatus._Rejected;
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
                    FuncName = "UpdateOutletOfAPIUser",
                    Error = ex.Message,
                });
                Resp = ex.Message + "_" + Resp;
                OutReqRes.Msg = ErrorCodes.TempError;
            }
            #region APILogOutletRegistration
            new ProcUpdateLogOnboardingReqResp(_dal).Call(new OnboardingLog
            {
                APIID = _APIID,
                Method = "UpdateOutletOfAPIUser",
                Request = Req,
                Response = Resp,

            });
            #endregion
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
            fintechRequest.UserID = apiSetting.UserID;
            fintechRequest.Token = apiSetting.Token;
            try
            {
                Req = apiSetting.BaseURL + "API/Outlet/CheckStatus?" + JsonConvert.SerializeObject(fintechRequest);
                Resp = AppWebRequest.O.PostJsonDataUsingHWR(apiSetting.BaseURL + "API/Outlet/CheckStatus/", fintechRequest);
                if (Validate.O.ValidateJSON(Resp ?? string.Empty))
                {
                    var _apiRes = JsonConvert.DeserializeObject<FintechAPIResponse>(Resp);
                    if (_apiRes != null)
                    {
                        if (_apiRes.Statuscode == ErrorCodes.One)
                        {
                            OutReqRes.Statuscode = ErrorCodes.One;
                            OutReqRes.Msg = ErrorCodes.OutletRegistered;
                            OutReqRes.APIOutletID = _apiRes.data.OutletID.ToString();
                            OutReqRes.APIOutletStatus = _apiRes.data.VerifyStatus;
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
                            OutReqRes.BBPSID = _apiRes.data.OutletID.ToString();
                            OutReqRes.BBPSStatus = RndKYCOutLetStatus._Pending;
                            OutReqRes.AEPSID = _apiRes.data.OutletID.ToString();
                            OutReqRes.AEPSStatus = RndKYCOutLetStatus._Pending;
                            OutReqRes.DMTID = _apiRes.data.OutletID.ToString();
                            OutReqRes.DMTStatus = RndKYCOutLetStatus._Pending;
                            OutReqRes.PSAID = string.Empty;
                            OutReqRes.PSAStatus = RndKYCOutLetStatus._NotApplied;
                        }
                        else
                        {
                            OutReqRes.APIOutletStatus = RndKYCOutLetStatus._Rejected;
                            OutReqRes.BBPSStatus = RndKYCOutLetStatus._Rejected;
                            OutReqRes.AEPSStatus = RndKYCOutLetStatus._Rejected;
                            OutReqRes.DMTStatus = RndKYCOutLetStatus._Rejected;
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
        public OutletAPIStatusUpdate CheckOutletServiceStatus(FintechAPIRequestModel fintechRequest, string SCode)
        {
            string Req = string.Empty;
            string Resp = string.Empty;
            var OutReqRes = new OutletAPIStatusUpdate
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            fintechRequest.UserID = apiSetting.UserID;
            fintechRequest.Token = apiSetting.Token;
            try
            {
                Req = apiSetting.BaseURL + "API/Outlet/ServiceStatus?" + JsonConvert.SerializeObject(fintechRequest);
                Resp = AppWebRequest.O.PostJsonDataUsingHWR(apiSetting.BaseURL + "API/Outlet/ServiceStatus/", fintechRequest);
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
                            OutReqRes.BioAuthType = _apiRes.data.BioAuthType;
                            OutReqRes.IsOTPRequired = _apiRes.data.IsOTPRequired;
                            OutReqRes.IsBioMetricRequired = _apiRes.data.IsBioMetricRequired;
                            OutReqRes.OTPRefID = _apiRes.data.OTPRefID;
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
                            else if (SCode == ServiceCode.Travel)
                            {
                                OutReqRes.RailID = OutReqRes.APIOutletStatus == UserStatus.REJECTED ? OutReqRes.APIOutletID : _apiRes.data.ServiceOutletID;
                                OutReqRes.RailStatus = OutReqRes.APIOutletStatus == UserStatus.REJECTED ? OutReqRes.APIOutletStatus : _apiRes.data.ServiceStatus;
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
        public bool CheckServiceReuiredKYC(string SCode)
        {
            string Req = string.Empty;
            string Resp = string.Empty;
            var IsReuired = false;
            try
            {
                Req = apiSetting.BaseURL + "API/Service/KYCStatus?SPKey=" + (SCode ?? string.Empty);
                Resp = AppWebRequest.O.CallUsingHttpWebRequest_GET(Req);
                IsReuired = (Resp ?? string.Empty).ToUpper() == "TRUE";
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "CheckServiceReuiredKYC",
                    Error = ex.Message,
                });
                Resp = ex.Message + "_" + Resp;
            }
            #region APILogOutletRegistration
            new ProcUpdateLogOnboardingReqResp(_dal).Call(new OnboardingLog
            {
                APIID = _APIID,
                Method = "CheckServiceReuiredKYC",
                Request = Req,
                Response = Resp,

            });
            #endregion
            return IsReuired;
        }

        public BillerAPIResponse GetRPBillerByType(int OPType)
        {
            string Req = string.Empty;
            string Resp = string.Empty;
            var OutReqRes = new BillerAPIResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            var fintechRequest = new APIBillerRequest
            {
                UserID = apiSetting.UserID,
                Token = apiSetting.Token,
                Format = ResponseType.JSON,
                OpTypeID = OPType
            };
            try
            {
                Req = apiSetting.BaseURL + "API/GetBillerByType?" + JsonConvert.SerializeObject(fintechRequest);
                Resp = AppWebRequest.O.PostJsonDataUsingHWR(apiSetting.BaseURL + "API/GetBillerByType/", fintechRequest);
                if (Validate.O.ValidateJSON(Resp ?? string.Empty))
                {
                    OutReqRes = JsonConvert.DeserializeObject<BillerAPIResponse>(Resp);
                }
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "GetRPBillerByType",
                    Error = ex.Message,
                });
                Resp = ex.Message + "_" + Resp;
                OutReqRes.Msg = ErrorCodes.TempError;
            }
            #region APILogOutletRegistration
            IOperatorML opML = new OperatorML(_accessor, _env, false);
            opML.UpdateBillerLog(new Fintech.AppCode.Model.CommonReq
            {
                CommonInt = 0,
                CommonInt2 = 0,
                CommonStr = Req,
                CommonStr2 = Resp
            });
            #endregion
            return OutReqRes;
        }
        public BillerAPIResponse GetRPBillerByBillerID(string RPBillerID)
        {

            string Req = string.Empty;
            string Resp = string.Empty;
            var OutReqRes = new BillerAPIResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            var fintechRequest = new APIBillerRequest
            {
                UserID = apiSetting.UserID,
                Token = apiSetting.Token,
                Format = ResponseType.JSON,
                OpTypeID = 0,
                BillerID = RPBillerID
            };
            try
            {
                Req = apiSetting.BaseURL + "API/GetBillerByBillerID?" + JsonConvert.SerializeObject(fintechRequest);
                Resp = AppWebRequest.O.PostJsonDataUsingHWR(apiSetting.BaseURL + "API/GetBillerByBillerID/", fintechRequest);
                if (Validate.O.ValidateJSON(Resp ?? string.Empty))
                {
                    OutReqRes = JsonConvert.DeserializeObject<BillerAPIResponse>(Resp);
                }
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "GetRPBillerByBillerID",
                    Error = ex.Message,
                });
                Resp = ex.Message + "_" + Resp;
                OutReqRes.Msg = ErrorCodes.TempError;
            }
            #region APILogOutletRegistration
            IOperatorML opML = new OperatorML(_accessor, _env, false);
            opML.UpdateBillerLog(new Fintech.AppCode.Model.CommonReq
            {
                CommonInt = 0,
                CommonInt2 = 0,
                CommonStr = Req,
                CommonStr2 = Resp
            });
            #endregion
            return OutReqRes;
        }
    }
    public partial class FintechAPIML : IMoneyTransferAPIML
    {
        public MSenderCreateResp CreateSender(MTAPIRequest request)
        {

            var res = new MSenderCreateResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError,
                ErrorCode = ErrorCodes.Unknown_Error
            };

            var req = new CreateSednerReq
            {
                UserID = apiSetting.UserID,
                Token = apiSetting.Token,
                OutletID = Validate.O.IsNumeric(request.APIOutletID ?? string.Empty) ? Convert.ToInt32(request.APIOutletID) : 0,
                SPKey = request.APIOpCode,
                SenderMobile = request.SenderMobile,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Address = request.Address,
                DOB = request.DOB,
                Pincode = request.Pincode.ToString(),
                OTP = request.OTP,
                ReferenceID = request.ReferenceID
            };
            string response = string.Empty;
            var _URL = apiSetting.BaseURL + "APIMoneyTransfer/CreateSender";
            try
            {
                response = AppWebRequest.O.PostJsonDataUsingHWRTLS(_URL, req).Result;
                if (!string.IsNullOrEmpty(response))
                {
                    var _apiRes = JsonConvert.DeserializeObject<SenderCreateResponse>(response);
                    if (_apiRes != null)
                    {
                        res.Statuscode = _apiRes.Statuscode;
                        res.Msg = _apiRes.Message;
                        res.ErrorCode = _apiRes.ErrorCode;
                        res.ReferenceID = _apiRes.ReferenceID;
                        res.IsOTPGenerated = _apiRes.IsOTPRequired;
                        res.IsOTPResendAvailble = _apiRes.IsOTPResendAvailble;

                        if (_apiRes.Statuscode == ErrorCodes.One)
                        {
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
                                ReffID = _apiRes.ReferenceID.ToString()
                            });
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

            var req = new DMTAPIRequest
            {
                UserID = apiSetting.UserID,
                Token = apiSetting.Token,
                OutletID = Validate.O.IsNumeric(request.APIOutletID ?? string.Empty) ? Convert.ToInt32(request.APIOutletID) : 0,
                SPKey = request.APIOpCode,
                SenderMobile = request.SenderMobile,
                ReferenceID = request.ReferenceID
            };
            string response = string.Empty;
            var _URL = apiSetting.BaseURL + "APIMoneyTransfer/GetSender";
            try
            {
                response = AppWebRequest.O.PostJsonDataUsingHWRTLS(_URL, req).Result;
                if (!string.IsNullOrEmpty(response))
                {
                    var _apiRes = JsonConvert.DeserializeObject<SenderLoginResponse>(response);
                    if (_apiRes != null)
                    {
                        res.Statuscode = _apiRes.Statuscode;
                        res.Msg = _apiRes.Message;
                        res.ErrorCode = _apiRes.ErrorCode;
                        res.IsEKYCAvailable = _apiRes.IsEKYCAvailable;
                        res.IsOTPGenerated = _apiRes.IsOTPRequired;
                        res.IsSenderNotExists = _apiRes.IsSenderNotExists;
                        res.IsNotActive = !_apiRes.IsActive;

                        res.SenderName = _apiRes.SenderName;
                        res.ReferenceID = _apiRes.ReferenceID;
                        res.KYCStatus = _apiRes.KYCStatus;
                        res.RemainingLimit = _apiRes.AvailbleLimit;
                        res.AvailbleLimit = _apiRes.TotalLimit;
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


            var req = new DMTAPIRequest
            {
                UserID = apiSetting.UserID,
                Token = apiSetting.Token,
                OutletID = Validate.O.IsNumeric(request.APIOutletID ?? string.Empty) ? Convert.ToInt32(request.APIOutletID) : 0,
                SPKey = request.APIOpCode,
                SenderMobile = request.SenderMobile,
                ReferenceID = request.ReferenceID
            };
            string response = string.Empty;
            var _URL = apiSetting.BaseURL + "APIMoneyTransfer/SenderResendOTP";
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
                        senderCreateResp.IsOTPGenerated = _apiRes.IsOTPRequired;
                        senderCreateResp.IsOTPResendAvailble = _apiRes.IsOTPResendAvailble;
                        senderCreateResp.ErrorCode = _apiRes.ErrorCode;
                        senderCreateResp.ReferenceID = _apiRes.ReferenceID;
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
            var req = new DMTAPIRequest
            {
                UserID = apiSetting.UserID,
                Token = apiSetting.Token,
                OutletID = Validate.O.IsNumeric(request.APIOutletID ?? string.Empty) ? Convert.ToInt32(request.APIOutletID) : 0,
                SPKey = request.APIOpCode,
                SenderMobile = request.SenderMobile,
                OTP = request.OTP,
                ReferenceID = request.ReferenceID
            };
            string response = string.Empty;
            var _URL = apiSetting.BaseURL + "APIMoneyTransfer/VerifySender";
            try
            {
                response = AppWebRequest.O.PostJsonDataUsingHWRTLS(_URL, req).Result;
                if (!string.IsNullOrEmpty(response))
                {
                    var _apiRes = JsonConvert.DeserializeObject<DMTAPIResponse>(response);
                    if (_apiRes != null)
                    {
                        res.Statuscode = _apiRes.Statuscode;
                        res.Msg = _apiRes.Message;
                        res.ErrorCode = _apiRes.ErrorCode;
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
            var req = new CreateBeneReq
            {
                UserID = apiSetting.UserID,
                Token = apiSetting.Token,
                OutletID = Validate.O.IsNumeric(request.APIOutletID ?? string.Empty) ? Convert.ToInt32(request.APIOutletID) : 0,
                SPKey = request.APIOpCode,
                SenderMobile = request.SenderMobile,
                OTP = request.OTP,
                ReferenceID = request.ReferenceID,
                BeneAccountNumber = request.mBeneDetail.AccountNo,
                BankID = request.mBeneDetail.BankID,
                BankName = request.mBeneDetail.BankName,
                BeneName = request.mBeneDetail.BeneName,
                IFSC = request.mBeneDetail.IFSC,
                BeneMobile = request.mBeneDetail.MobileNo,
                TransMode = request.mBeneDetail.TransMode
            };
            string response = string.Empty;
            var _URL = apiSetting.BaseURL + "APIMoneyTransfer/AddBeneficiary";
            try
            {
                response = AppWebRequest.O.PostJsonDataUsingHWRTLS(_URL, req).Result;
                if (!string.IsNullOrEmpty(response))
                {
                    var _apiRes = JsonConvert.DeserializeObject<SenderCreateResponse>(response);
                    if (_apiRes != null)
                    {
                        res.Statuscode = _apiRes.Statuscode;
                        res.Msg = _apiRes.Message;
                        res.ErrorCode = _apiRes.ErrorCode;
                        res.BeneID = _apiRes.BeneID;
                        res.ReferenceID = _apiRes.ReferenceID;
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
            var req = new DMTAPIRequest
            {
                UserID = apiSetting.UserID,
                Token = apiSetting.Token,
                OutletID = Validate.O.IsNumeric(request.APIOutletID ?? string.Empty) ? Convert.ToInt32(request.APIOutletID) : 0,
                SPKey = request.APIOpCode,
                SenderMobile = request.SenderMobile,
                OTP = request.OTP,
                ReferenceID = request.ReferenceID
            };
            string response = string.Empty;
            var _URL = apiSetting.BaseURL + "APIMoneyTransfer/GetBeneficiary";
            try
            {
                response = AppWebRequest.O.PostJsonDataUsingHWRTLS(_URL, req).Result;
                if (!string.IsNullOrEmpty(response))
                {
                    var _apiRes = JsonConvert.DeserializeObject<APIDMTBeneficiaryResponse>(response);
                    if (_apiRes != null)
                    {
                        res.Statuscode = _apiRes.Statuscode;
                        res.Msg = _apiRes.Message;
                        res.ErrorCode = _apiRes.ErrorCode;
                        if (_apiRes.Statuscode == ErrorCodes.One)
                        {
                            res.Beneficiaries = _apiRes.Beneficiaries;
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
        public MSenderCreateResp GenerateOTP(MTAPIRequest request)
        {
            var senderCreateResp = new MSenderCreateResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = nameof(ErrorCodes.Unknown_Error).Replace("_", " "),
                ErrorCode = ErrorCodes.Unknown_Error
            };
            var req = new DeleteBeneReq
            {
                UserID = apiSetting.UserID,
                Token = apiSetting.Token,
                OutletID = Validate.O.IsNumeric(request.APIOutletID ?? string.Empty) ? Convert.ToInt32(request.APIOutletID) : 0,
                SPKey = request.APIOpCode,
                SenderMobile = request.SenderMobile,
                ReferenceID = request.ReferenceID,
                BeneID = request.mBeneDetail.BeneID
            };
            string response = string.Empty;
            var _URL = apiSetting.BaseURL + "APIMoneyTransfer/GenerateBenficiaryOTP";
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
            var req = new DMTTransactionReq
            {
                UserID = apiSetting.UserID,
                Token = apiSetting.Token,
                OutletID = Validate.O.IsNumeric(request.APIOutletID ?? string.Empty) ? Convert.ToInt32(request.APIOutletID) : 0,
                SPKey = request.APIOpCode,
                SenderMobile = request.SenderMobile,
                ReferenceID = request.ReferenceID,
                BeneID = request.mBeneDetail.BeneID,
                BeneAccountNumber = request.mBeneDetail.AccountNo,
                OTP = request.OTP,
                BeneMobile = request.mBeneDetail.MobileNo
            };
            string response = string.Empty;
            var _URL = apiSetting.BaseURL + "APIMoneyTransfer/ValidateBeneficiary";
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
            var req = new DeleteBeneReq
            {
                UserID = apiSetting.UserID,
                Token = apiSetting.Token,
                OutletID = Validate.O.IsNumeric(request.APIOutletID ?? string.Empty) ? Convert.ToInt32(request.APIOutletID) : 0,
                SPKey = request.APIOpCode,
                SenderMobile = request.SenderMobile,
                ReferenceID = request.ReferenceID,
                BeneID = request.mBeneDetail.BeneID,
                OTP = request.OTP
            };
            string response = string.Empty;
            var _URL = apiSetting.BaseURL + "APIMoneyTransfer/DeleteBeneficiary";
            try
            {
                response = AppWebRequest.O.PostJsonDataUsingHWRTLS(_URL, req).Result;
                if (!string.IsNullOrEmpty(response))
                {
                    var _apiRes = JsonConvert.DeserializeObject<SenderCreateResponse>(response);
                    if (_apiRes != null)
                    {
                        senderLoginResponse.Statuscode = _apiRes.Statuscode;
                        senderLoginResponse.Msg = _apiRes.Message;
                        senderLoginResponse.ErrorCode = _apiRes.ErrorCode;
                        senderLoginResponse.IsOTPGenerated = _apiRes.IsOTPRequired;
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
            var req = new DeleteBeneReq
            {
                UserID = apiSetting.UserID,
                Token = apiSetting.Token,
                OutletID = Validate.O.IsNumeric(request.APIOutletID ?? string.Empty) ? Convert.ToInt32(request.APIOutletID) : 0,
                SPKey = request.APIOpCode,
                SenderMobile = request.SenderMobile,
                ReferenceID = request.ReferenceID,
                BeneID = request.mBeneDetail.BeneID,
                OTP = request.OTP
            };
            string response = string.Empty;
            var _URL = apiSetting.BaseURL + "APIMoneyTransfer/ValidateRemoveBeneficiaryOTP";
            try
            {
                response = AppWebRequest.O.PostJsonDataUsingHWRTLS(_URL, req).Result;
                if (!string.IsNullOrEmpty(response))
                {
                    var _apiRes = JsonConvert.DeserializeObject<SenderCreateResponse>(response);
                    if (_apiRes != null)
                    {
                        mSenderLoginResponse.Statuscode = _apiRes.Statuscode;
                        mSenderLoginResponse.Msg = _apiRes.Message;
                        mSenderLoginResponse.ErrorCode = _apiRes.ErrorCode;
                        mSenderLoginResponse.IsOTPGenerated = _apiRes.IsOTPRequired;
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
            var req = new DMTTransactionReq
            {
                UserID = apiSetting.UserID,
                Token = apiSetting.Token,
                OutletID = string.IsNullOrEmpty(request.APIOutletID) ? apiSetting.OutletID : Convert.ToInt32(request.APIOutletID),
                SPKey = request.APIOpCode,
                SenderMobile = request.SenderMobile,
                ReferenceID = request.ReferenceID,
                BeneID = request.mBeneDetail.BeneID ?? string.Empty,
                BeneAccountNumber = request.mBeneDetail.AccountNo,
                BankID = request.mBeneDetail.BankID,
                BankName = request.mBeneDetail.BankName,
                BeneName = request.mBeneDetail.BeneName ?? string.Empty,
                TransMode = request.TransMode.Equals("IMPS") ? 1 : 2,
                APIRequestID = request.TID.ToString(),
                IFSC = request.mBeneDetail.IFSC
            };
            string response = string.Empty;
            var _URL = apiSetting.BaseURL + "APIMoneyTransfer/VerifyAccount";
            try
            {
                response = AppWebRequest.O.PostJsonDataUsingHWRTLS(_URL, req).Result;
                if (!string.IsNullOrEmpty(response))
                {
                    var _apiRes = JsonConvert.DeserializeObject<DMTTransactionResponse>(response);
                    if (_apiRes != null)
                    {
                        res.Statuscode = _apiRes.Status;
                        res.Msg = _apiRes.Message;
                        res.ErrorCode = _apiRes.ErrorCode;
                        res.LiveID = _apiRes.LiveID;
                        res.VendorID = _apiRes.RPID;
                        res.BeneName = _apiRes.BeneName;
                        if (_apiRes.Status == RechargeRespType.FAILED)
                        {
                            if (_apiRes.Message.ToLower().Contains("insuf"))
                            {
                                res.Msg = ErrorCodes.Down;
                                res.ErrorCode = DMTErrorCodes.Declined_by_ServiceProvider;
                                res.LiveID = res.Msg;
                                if (_APICode == "RPFNTH" && ApplicationSetting.IsRPFintechDMTInsfficientPending)
                                {
                                    res.Statuscode = RechargeRespType.PENDING;
                                    res.Msg = nameof(ErrorCodes.Request_Accpeted).Replace("_", " ");
                                    res.ErrorCode = ErrorCodes.Request_Accpeted;
                                    res.LiveID = string.Empty;
                                }
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
            var req = new DMTTransactionReq
            {
                UserID = apiSetting.UserID,
                Token = apiSetting.Token,
                OutletID = Validate.O.IsNumeric(request.APIOutletID ?? string.Empty) ? Convert.ToInt32(request.APIOutletID) : 0,
                SPKey = request.APIOpCode,
                SenderMobile = request.SenderMobile,
                ReferenceID = request.ReferenceID,
                BeneID = request.mBeneDetail.BeneID,
                BeneAccountNumber = request.mBeneDetail.AccountNo,
                BankID = request.mBeneDetail.BankID,
                BankName = request.mBeneDetail.BankName,
                BeneName = request.mBeneDetail.BeneName,
                BeneMobile = request.mBeneDetail.MobileNo,
                TransMode = request.TransMode.Equals("IMPS") ? 1 : 2,
                APIRequestID = request.TID.ToString(),
                IFSC = request.mBeneDetail.IFSC,
                Amount = request.Amount

            };
            string response = string.Empty;
            var _URL = apiSetting.BaseURL + "APIMoneyTransfer/AccountTransfer";
            try
            {
                response = AppWebRequest.O.PostJsonDataUsingHWRTLS(_URL, req).Result;
                if (!string.IsNullOrEmpty(response))
                {
                    var _apiRes = JsonConvert.DeserializeObject<DMTTransactionResponse>(response);
                    if (_apiRes != null)
                    {
                        res.Statuscode = _apiRes.Status;
                        res.Msg = _apiRes.Message;
                        res.ErrorCode = _apiRes.ErrorCode;
                        res.LiveID = _apiRes.LiveID;
                        res.VendorID = _apiRes.RPID;
                        res.BeneName = _apiRes.BeneName;
                        if (_apiRes.Status == RechargeRespType.FAILED)
                        {
                            if (_apiRes.Message.ToLower().Contains("insuf"))
                            {
                                res.Msg = ErrorCodes.Down;
                                res.ErrorCode = DMTErrorCodes.Declined_by_ServiceProvider;
                                res.LiveID = res.Msg;
                                if (_APICode == "RPFNTH" && ApplicationSetting.IsRPFintechDMTInsfficientPending)
                                {
                                    res.Statuscode = RechargeRespType.PENDING;
                                    res.Msg = nameof(ErrorCodes.Request_Accpeted).Replace("_", " ");
                                    res.ErrorCode = ErrorCodes.Request_Accpeted;
                                    res.LiveID = string.Empty;
                                }
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
            sb.Append(apiSetting.UserID);
            sb.Append("&Token=");
            sb.Append(apiSetting.Token);
            sb.Append("&RPID=");
            sb.Append(VendorID);
            sb.Append("&AgentID=");
            sb.Append(TransactionID);
            sb.Append("&Optional1=");
            sb.Append(Convert.ToDateTime(fromD).ToString("dd MMM yyyy", CultureInfo.InvariantCulture));

            string URL = apiSetting.BaseURL + "API/TransactionStatusCheck?" + sb.ToString();
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
        //        res.Msg = "Token expired";
        //        return res;
        //    }
        //    var req = new refundotpreq
        //    {
        //        header = new RBLHeader
        //        {
        //            sessiontoken = RBLSession
        //        },
        //        RBLtransactionid = VendorID
        //    };
        //    var xmlReq = XMLHelper.O.SerializeToXml(req, null);
        //    string response = string.Empty;
        //    var _URL = appSetting.BASEURL + string.Format("?client_id={0}&client_secret={1}", appSetting.CLIENTID, appSetting.CLIENTSECRET);
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
        //        res.Msg = "Token expired";
        //        return res;
        //    }
        //    var req = new refundreq
        //    {
        //        header = new RBLHeader
        //        {
        //            sessiontoken = RBLSession
        //        },
        //        bcagent = appSetting.BCAGENT,
        //        channelpartnerrefno = TIDPrefix + TID,
        //        verficationcode = OTP,
        //        flag = 1
        //    };
        //    var xmlReq = XMLHelper.O.SerializeToXml(req, null);
        //    string response = string.Empty;
        //    var _URL = appSetting.BASEURL + string.Format("?client_id={0}&client_secret={1}", appSetting.CLIENTID, appSetting.CLIENTSECRET);
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