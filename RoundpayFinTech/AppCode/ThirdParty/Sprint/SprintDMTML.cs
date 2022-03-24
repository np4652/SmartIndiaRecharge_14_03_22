using Fintech.AppCode.Configuration;
using Fintech.AppCode.DB;
using Fintech.AppCode.HelperClass;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using Fintech.AppCode.WebRequest;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using RoundpayFinTech.AppCode.DL;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.MoneyTransfer;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Security.Claims;
using System.Text;
using Validators;

namespace RoundpayFinTech.AppCode.MiddleLayer.Dmt_Api
{
    public partial class SprintDMTML
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        private readonly IConfiguration Configuration;
        private readonly string _APICode;
        private readonly int _APIID;
        private readonly IDAL _dal;
        private readonly SprintJsonSettings appSetting;
        private string _JWTToken = string.Empty;


        public SprintDMTML(IHttpContextAccessor accessor, IHostingEnvironment env, string APICode, int APIID, IDAL dal)
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
            appSetting = AppSetting();
        }
        private SprintJsonSettings AppSetting()
        {
            try
            {
                return new SprintJsonSettings
                {
                    FetchBillURL = Configuration["DMR:SPRINT:FetchBillURL"],
                    PayBillURL = Configuration["DMR:SPRINT:PayBillURL"],
                    StatusCheckURL = Configuration["DMR:SPRINT:StatusCheckURL"],
                    DMTURL = Configuration["DMR:SPRINT:DMTURL"],
                    UserName = Configuration["DMR:SPRINT:UserName"],
                    Password = Configuration["DMR:SPRINT:Password"],
                    PartnerID = Configuration["DMR:SPRINT:PartnerID"],
                    JWTKey = Configuration["DMR:SPRINT:JWTKey"],
                    Authorisedkey = Configuration["DMR:SPRINT:Authorisedkey"],
                };
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "SprintJsonSettings",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser
                });
            }
            return new SprintJsonSettings();
        }

        private void TokenGeneration()
        {
            long unixSeconds = DateTimeOffset.Now.ToUnixTimeSeconds();
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(appSetting.JWTKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var claims = new[] {
                new Claim("timestamp", unixSeconds.ToString()),
                new Claim("partnerId", appSetting.PartnerID),
                new Claim("reqid", Guid.NewGuid().ToString().Replace("-",""))
            };
            var token = new JwtSecurityToken(null, null, claims, null, signingCredentials: credentials);
            _JWTToken = new JwtSecurityTokenHandler().WriteToken(token).ToString();
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
    public partial class SprintDMTML : IMoneyTransferAPIML
    {
        public MSenderCreateResp CreateSender(MTAPIRequest request)
        {
            var res = new MSenderCreateResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError,
                ErrorCode = ErrorCodes.Unknown_Error
            };
            TokenGeneration();
            var headers = new Dictionary<string, string>
            {
                { "Token", _JWTToken}
                //{ "Authorisedkey", appSetting.Authorisedkey}
            };
            var spGetRemReq = new SPDMTReq
            {
                mobile = request.SenderMobile,
                bank3_flag = "yes"
            };

            string response = string.Empty;
            var _URL = appSetting.DMTURL + "/remitter/queryremitter";
            try
            {
                response = AppWebRequest.O.PostJsonDataUsingHWRTLS(_URL, spGetRemReq, headers).Result;

                if (!string.IsNullOrEmpty(response))
                {
                    var _apiRes = JsonConvert.DeserializeObject<SPDMTResp>(response);
                    if (_apiRes != null)
                    {
                        if (_apiRes.message.Equals(SPDMTErrorCodes.RMTNtReg) && _apiRes.response_code.Equals(SPDMTErrorCodes.RespZero) && _apiRes.status == false)
                        {
                            res.Statuscode = ErrorCodes.One;
                            res.Msg = _apiRes.message;
                            res.ErrorCode = DMTErrorCodes.Sender_Not_Found;
                            string oTP = HashEncryption.O.CreatePasswordNumeric(6);

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
                                UserID = request.UserID,
                                OTP = oTP
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
                                res.ReferenceID = _apiRes.stateresp;
                                //res.IsOTPResendAvailble = true;
                                var senderRes = (SenderInfo)new ProcVerySenderOTP(_dal).Call(new CommonReq
                                {
                                    CommonStr = request.SenderMobile,
                                    CommonStr2 = oTP,
                                    CommonInt = request.UserID,
                                });
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
                Request = "Internal Sender",
                Response = "Internal Sender",
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
            TokenGeneration();
            var headers = new Dictionary<string, string>
            {
                { "Token", _JWTToken},
                //{ "Authorisedkey", appSetting.Authorisedkey}
            };

            var spGetRemReq = new SPDMTReq
            {
                mobile = request.SenderMobile,
                bank3_flag = "yes"
            };
            string response = string.Empty;
            var _URL = appSetting.DMTURL + "/remitter/queryremitter";

            try
            {
                response = AppWebRequest.O.PostJsonDataUsingHWRTLS(_URL, spGetRemReq, headers).Result;

                if (!string.IsNullOrEmpty(response))
                {
                    var _apiRes = JsonConvert.DeserializeObject<SPDMTResp>(response);
                    if (_apiRes != null)
                    {

                        if (_apiRes.message.Equals(SPDMTErrorCodes.RMTNtReg) && _apiRes.response_code.Equals(SPDMTErrorCodes.RespZero) && _apiRes.status == false)
                        {
                            res.Statuscode = ErrorCodes.One;
                            res.Msg = _apiRes.message;
                            res.ErrorCode = DMTErrorCodes.Sender_Not_Found;
                            res.IsSenderNotExists = true;
                        }
                        else if (_apiRes.message.Equals(SPDMTErrorCodes.RMTFoundSccs) && _apiRes.response_code.Equals(SPDMTErrorCodes.RespOne) && _apiRes.status == true)
                        {
                            res.Statuscode = ErrorCodes.One;
                            res.Msg = nameof(DMTErrorCodes.Detail_Found_Successfully).Replace("_", " ");
                            res.ErrorCode = DMTErrorCodes.Detail_Found_Successfully;
                            if (_apiRes.data != null)
                            {
                                res.SenderName = _apiRes.data.fname + " " + _apiRes.data.lname;
                                //res.ReferenceID = _apiRes.data.Id.ToString();
                                res.RemainingLimit = 0;
                                res.AvailbleLimit = _apiRes.data.bank1_limit + _apiRes.data.bank2_limit + _apiRes.data.bank3_limit;
                                //Inserting Data
                                var dbres = (new ProcUpdateSender(_dal).Call(new SenderRequest
                                {
                                    Name = _apiRes.data.fname + " " + _apiRes.data.lname,
                                    MobileNo = _apiRes.data.mobile.ToString(),
                                    Pincode = request.Pincode.ToString(),
                                    Address = request.Address,
                                    City = request.City,
                                    StateID = request.StateID,
                                    AadharNo = request.AadharNo,
                                    Dob = request.DOB,
                                    UserID = request.UserID
                                })) as SenderInfo;
                                var senderRes = (SenderInfo)new ProcVerySenderOTP(_dal).Call(new CommonReq
                                {
                                    CommonStr = request.SenderMobile,
                                    CommonStr2 = dbres.OTP,
                                    CommonInt = request.UserID,
                                });
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
                Request = _URL + "?" + JsonConvert.SerializeObject(spGetRemReq) + "|" + JsonConvert.SerializeObject(headers),
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

            //var req = new MMWFintechReq
            //{
            //    Username = apiSetting.Username,
            //    Password = apiSetting.Password,
            //    APIKey = apiSetting.APIKey,
            //    SenderMobile = request.SenderMobile,
            //};

            //string response = string.Empty;

            //var _URL = apiSetting.BaseURL + "/api/DMR242ResndRegistrationOtp";
            //try
            //{
            //    response = AppWebRequest.O.PostJsonDataUsingHWRTLS(_URL, req).Result;
            //    if (!string.IsNullOrEmpty(response))
            //    {
            //        var _apiRes = JsonConvert.DeserializeObject<MMWFintechObjResp>(response);
            //        if (_apiRes != null)
            //        {
            //            if (_apiRes.Message.Equals(MMWFintechCodes.MsgOtpSent) && _apiRes.Status.Equals(MMWFintechCodes.Msg200) && _apiRes.MessageStatus == true)
            //            {
            //                senderCreateResp.Statuscode = ErrorCodes.One;
            //                senderCreateResp.Msg = _apiRes.Message;
            //                senderCreateResp.IsOTPGenerated = true;
            //                senderCreateResp.IsOTPResendAvailble = true;
            //                senderCreateResp.ErrorCode = DMTErrorCodes.OTP_for_verification_has_been_sent_successfully;

            //            }
            //            else
            //            {
            //                senderCreateResp.Statuscode = ErrorCodes.Minus1;
            //                senderCreateResp.Msg = nameof(ErrorCodes.FAILED);
            //                senderCreateResp.ErrorCode = ErrorCodes.Minus1;
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
            TokenGeneration();
            var headers = new Dictionary<string, string>
            {
                { "Token", _JWTToken}
                //{ "Authorisedkey", appSetting.Authorisedkey}
            };

            var senderRequest = (SenderRequest)new ProcGetSender(_dal).Call(request.SenderMobile);
            var spGetRemReq = new SPDMTReq
            {
                mobile = request.SenderMobile,
                firstname = senderRequest.Name.Split(' ')[0].ToString(),
                lastname = senderRequest.Name.Split(' ')[1].ToString(),
                address = senderRequest.Address,
                otp = request.OTP,
                pincode = senderRequest.Pincode,
                stateresp = request.ReferenceID,
                bank3_flag = "yes",
                dob = senderRequest.Dob,
                gst_state = request.StateID.ToString()
            };

            string response = string.Empty;
            var _URL = appSetting.DMTURL + "/remitter/registerremitter";
            try
            {
                response = AppWebRequest.O.PostJsonDataUsingHWRTLS(_URL, spGetRemReq, headers).Result;
                if (!string.IsNullOrEmpty(response))
                {
                    var _apiRes = JsonConvert.DeserializeObject<SPDMTResp>(response);
                    if (_apiRes != null)
                    {
                        if (_apiRes.message.Equals(SPDMTErrorCodes.MsgSAS) && _apiRes.response_code == SPDMTErrorCodes.RespOne && _apiRes.status == true)
                        {
                            res.Statuscode = ErrorCodes.One;
                            res.Msg = nameof(DMTErrorCodes.Sender_Created_Successfully);
                            res.ErrorCode = DMTErrorCodes.Sender_Created_Successfully;
                        }
                        else if (_apiRes.status == false && _apiRes.response_code == 3 && _apiRes.message.Contains(SPDMTErrorCodes.MsgInvalidOTP))
                        {
                            res.Msg = nameof(ErrorCodes.Invalid_OTP).Replace("_", " ");
                            res.ErrorCode = ErrorCodes.Invalid_OTP;
                            res.Statuscode = _apiRes.response_code;
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
                Request = _URL + "?" + JsonConvert.SerializeObject(spGetRemReq) + "|" + JsonConvert.SerializeObject(headers),
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
            TokenGeneration();
            var headers = new Dictionary<string, string>
            {
                { "Token", _JWTToken}
                //{ "Authorisedkey", appSetting.Authorisedkey}
            };
            IBankML bankML = new BankML(_accessor, _env, false);
            var BankDetail = bankML.BankMasters(request.BankID);
            var spGetRemReq = new SPDMTReq
            {
                mobile = request.SenderMobile,
                benename = request.mBeneDetail.BeneName,
                bankid = BankDetail.SprintBankID,
                accno = request.mBeneDetail.AccountNo,
                ifsccode = request.mBeneDetail.IFSC,
                verified = "1",
                gst_state = request.StateID.ToString(),
                dob = request.DOB,
                address = request.Address,
                pincode = request.Pincode.ToString(),
                bank3_flag = "yes"
            };
            string response = string.Empty;
            var _URL = appSetting.DMTURL + "/beneficiary/registerbeneficiary";

            try
            {
                response = AppWebRequest.O.PostJsonDataUsingHWRTLS(_URL, spGetRemReq, headers).Result;
                if (!string.IsNullOrEmpty(response))
                {
                    var _apiRes = JsonConvert.DeserializeObject<SPDMTResp>(response);
                    if (_apiRes != null)
                    {
                        if (_apiRes.message.Equals(SPDMTErrorCodes.BeneAddedSuc) && _apiRes.response_code == SPDMTErrorCodes.RespOne && _apiRes.status == true)
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
                Request = _URL + "?" + JsonConvert.SerializeObject(spGetRemReq) + "|" + JsonConvert.SerializeObject(headers),
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
            TokenGeneration();
            var headers = new Dictionary<string, string>
            {
                { "Token", _JWTToken}
                //{ "Authorisedkey", appSetting.Authorisedkey}
            };
            var spGetRemReq = new SPDMTReq
            {
                mobile = request.SenderMobile
            };
            string response = string.Empty;
            var _URL = appSetting.DMTURL + "/beneficiary/registerbeneficiary/fetchbeneficiary";
            try
            {
                response = AppWebRequest.O.PostJsonDataUsingHWRTLS(_URL, spGetRemReq, headers).Result;

                if (!string.IsNullOrEmpty(response))
                {
                    var _apiRes = JsonConvert.DeserializeObject<SPDMTBeneResp>(response);
                    if (_apiRes != null)
                    {
                        if (_apiRes.message.Equals(SPDMTErrorCodes.GeBenSuccess) && _apiRes.response_code.Equals(SPDMTErrorCodes.RespOne) && _apiRes.status == true)
                        {
                            res.Statuscode = ErrorCodes.One;
                            res.Msg = nameof(ErrorCodes.Transaction_Successful);
                            res.ErrorCode = ErrorCodes.Transaction_Successful;
                            if (_apiRes.data != null)
                            {
                                if (_apiRes.data.Count > 0)
                                {
                                    var Beneficiaries = new List<MBeneDetail>();
                                    foreach (var item in _apiRes.data)
                                    {
                                        Beneficiaries.Add(new MBeneDetail
                                        {
                                            BankID = Convert.ToInt32(item.bankid),
                                            AccountNo = item.accno,
                                            BankName = item.bankname,
                                            IFSC = item.ifsc,
                                            BeneName = item.name == string.Empty ? "Test" : item.name,
                                            BeneID = item.bene_id,
                                            IsVerified = item.verified == "1" ? true : false
                                        });
                                        var param = new BenificiaryDetail
                                        {
                                            _SenderMobileNo = request.SenderMobile,
                                            _Name = item.name == string.Empty ? "Test" : item.name,
                                            _AccountNumber = item.accno,
                                            _MobileNo = request.SenderMobile,
                                            _IFSC = item.ifsc,
                                            _BankName = item.bankname,
                                            _BankID = Convert.ToInt32(item.bankid),
                                            _EntryBy = request.UserID,
                                            _VerifyStatus = item.verified == "1" ? 1 : 0,
                                            _APICode = _APICode,
                                            _BeneAPIID = Convert.ToInt32(item.bene_id)
                                        };
                                        var resdb = (BenificiaryModel)new ProcAddBenificiary(_dal).Call(param);
                                    }
                                    res.Beneficiaries = Beneficiaries;
                                    res.ErrorCode = ErrorCodes.Transaction_Successful;
                                }
                                else
                                {
                                    res.Statuscode = ErrorCodes.One;
                                    res.Msg = nameof(DMTErrorCodes.Beneficiary_not_found).Replace("_", " ");
                                    res.ErrorCode = DMTErrorCodes.Beneficiary_not_found;
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
                Request = _URL + "?" + JsonConvert.SerializeObject(spGetRemReq) + "|" + JsonConvert.SerializeObject(headers),
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

            TokenGeneration();
            var headers = new Dictionary<string, string>
            {
                { "Token", _JWTToken}
                //{ "Authorisedkey", appSetting.Authorisedkey}
            };
            var resDB = (new ProcGetBenificiaryByBeneID(_dal).Call(new DMTReq { SenderNO = request.SenderMobile, BeneAPIID = Convert.ToInt32(request.mBeneDetail.BeneID) })) as BenificiaryDetail;
            Guid refID = Guid.NewGuid();
            var spGetRemReq = new SPDMTReq
            {
                mobile = request.SenderMobile,
                benename = resDB._Name == "" ? "Test" : resDB._Name,
                bankid = resDB._BankID,
                accno = resDB._AccountNumber,
                verified = "0",
                gst_state = request.StateID.ToString(),
                dob = request.DOB,
                address = request.Address,
                pincode = request.Pincode.ToString(),
                bank3_flag = "yes",
                referenceid = Convert.ToInt32(HashEncryption.O.CreatePasswordNumeric(9)),
                bene_id = Convert.ToInt32(request.mBeneDetail.BeneID)
            };

            string response = string.Empty;
            var _URL = appSetting.DMTURL + "/beneficiary/registerbeneficiary/benenameverify";

            try
            {
                response = AppWebRequest.O.PostJsonDataUsingHWRTLS(_URL, spGetRemReq, headers).Result;
                if (!string.IsNullOrEmpty(response))
                {
                    var _apiRes = JsonConvert.DeserializeObject<SPDMTResp>(response);
                    if (_apiRes.txn_status == 1 && _apiRes.response_code.Equals(SPDMTErrorCodes.RespOne) && _apiRes.status == true)
                    {
                        senderCreateResp.Statuscode = -2;
                        senderCreateResp.Msg = _apiRes.message;
                        senderCreateResp.ErrorCode = ErrorCodes.One;
                    }
                    else if (_apiRes.message.Equals(SPDMTErrorCodes.MsgTanInProc) && _apiRes.response_code.Equals(SPDMTErrorCodes.RespOne) && _apiRes.status == true && _apiRes.txn_status.Equals(SPDMTErrorCodes.MsgTxnStsFour))
                    {
                        senderCreateResp.Statuscode = -2;
                        senderCreateResp.Msg = nameof(ErrorCodes.FAILED);
                        senderCreateResp.ErrorCode = ErrorCodes.Minus1;
                    }
                    else
                    {
                        senderCreateResp.Statuscode = -2;
                        senderCreateResp.Msg = _apiRes.message;
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
                Request = _URL + "|" + JsonConvert.SerializeObject(headers) + "|" + JsonConvert.SerializeObject(spGetRemReq),
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
            //var req = new MMWFintechReq
            //{
            //    Username = apiSetting.Username,
            //    Password = apiSetting.Password,
            //    APIKey = apiSetting.APIKey,
            //    BenName = request.mBeneDetail.BeneName,
            //    IFSC = request.mBeneDetail.IFSC,
            //    AccountNumber = request.mBeneDetail.AccountNo,
            //    BankName = request.mBeneDetail.BankName,
            //    CustomerNumber = request.SenderMobile,
            //};
            //string response = string.Empty;
            //var _URL = apiSetting.BaseURL + "/api/DMR242VerifyBeneficiary";
            //try
            //{
            //    response = AppWebRequest.O.PostJsonDataUsingHWRTLS(_URL, req).Result;
            //    if (!string.IsNullOrEmpty(response))
            //    {
            //        var _apiRes = JsonConvert.DeserializeObject<MMWFintechResp>(response);
            //        if (_apiRes != null)
            //        {
            //            //mSenderLoginResponse.Statuscode = _apiRes.Statuscode;
            //            //mSenderLoginResponse.Msg = _apiRes.Message;
            //            //mSenderLoginResponse.ErrorCode = _apiRes.ErrorCode;
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    response = ex.Message + "|" + response;
            //    var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
            //    {
            //        ClassName = GetType().Name,
            //        FuncName = "ValidateBeneficiary",
            //        Error = ex.Message,
            //        LoginTypeID = LoginType.ApplicationUser,
            //        UserId = request.UserID
            //    });
            //}
            //new ProcUpdateLogDMRReqResp(_dal).Call(new DMTReqRes
            //{
            //    APIID = request.APIID,
            //    Method = "ValidateBeneficiary",
            //    RequestModeID = request.RequestMode,
            //    Request = _URL + "|" + JsonConvert.SerializeObject(req),
            //    Response = response,
            //    SenderNo = request.SenderMobile,
            //    UserID = request.UserID,
            //    TID = request.TransactionID
            //});
            return mSenderLoginResponse;
        }
        public MSenderLoginResponse RemoveBeneficiary(MTAPIRequest request)
        {
            var res = new MSenderLoginResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError,
                ErrorCode = ErrorCodes.Unknown_Error
            };

            TokenGeneration();
            var headers = new Dictionary<string, string>
            {
                { "Token", _JWTToken}
                //{ "Authorisedkey", appSetting.Authorisedkey}
            };

            var spGetRemReq = new SPDMTReq
            {
                mobile = request.SenderMobile,
                bene_id = Convert.ToInt32(request.mBeneDetail.BeneID)
            };
            string response = string.Empty;
            var _URL = appSetting.DMTURL + "/beneficiary/registerbeneficiary/deletebeneficiary";
            try
            {
                response = AppWebRequest.O.PostJsonDataUsingHWRTLS(_URL, spGetRemReq, headers).Result;
                if (!string.IsNullOrEmpty(response))
                {
                    var _apiRes = JsonConvert.DeserializeObject<SPDMTResp>(response);
                    if (_apiRes != null)
                    {
                        if (_apiRes.message.Equals(SPDMTErrorCodes.BeneRemovedSuc) && _apiRes.response_code == SPDMTErrorCodes.RespOne && _apiRes.status == true)
                        {
                            res.Statuscode = ErrorCodes.One;
                            res.Msg = nameof(DMTErrorCodes.Beneficiary_Deactivated_or_Deleted);
                            res.ErrorCode = DMTErrorCodes.Beneficiary_Deactivated_or_Deleted;
                        }
                        else
                        {
                            res.Statuscode = ErrorCodes.Minus1;
                            res.Msg = nameof(DMTErrorCodes.Beneficiary_not_found);
                            res.ErrorCode = DMTErrorCodes.Beneficiary_not_found;
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
                Request = _URL + "|" + JsonConvert.SerializeObject(headers) + "|" + JsonConvert.SerializeObject(spGetRemReq),
                Response = response,
                SenderNo = request.SenderMobile,
                UserID = request.UserID,
                TID = request.TransactionID
            });
            return res;
        }
        public MSenderLoginResponse RemoveBeneficiaryValidate(MTAPIRequest request)
        {
            var mSenderLoginResponse = new MSenderLoginResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = nameof(ErrorCodes.Unknown_Error).Replace("_", " "),
                ErrorCode = ErrorCodes.Unknown_Error
            };

            return mSenderLoginResponse;
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
            //return mSenderLoginResponse;
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

            TokenGeneration();
            var headers = new Dictionary<string, string>
            {
                { "Token", _JWTToken}
                //{ "Authorisedkey", appSetting.Authorisedkey}
            };
            IBankML bankML = new BankML(_accessor, _env, false);
            var BankDetail = bankML.BankMasters(request.mBeneDetail.BankID);

            var spGetRemReq = new SPDMTVAReq
            {
                mobile = request.SenderMobile,
                benename = string.IsNullOrEmpty(request.mBeneDetail.BeneName) ? "Test" : request.mBeneDetail.BeneName,
                bankid = BankDetail.SprintBankID,
                accno = request.mBeneDetail.AccountNo,
                gst_state = request.StateID.ToString(),
                dob = request.DOB,
                address = request.Address,
                pincode = request.Pincode.ToString(),
                referenceid = request.TID
            };
            string response = string.Empty;
            var _URL = appSetting.DMTURL + "/beneficiary/registerbeneficiary/benenameverify";
            try
            {
                response = "{\"status\":true,\"response_code\":1,\"utr\":null,\"ackno\":3074428,\"txn_status\":0,\"benename\":null,\"message\":\"Fund transfer is failed due to invalid beneficiary account number\"}";

                //AppWebRequest.O.PostJsonDataUsingHWRTLS(_URL, spGetRemReq, headers).Result;

                if (!string.IsNullOrEmpty(response))
                {
                    if (response != null)
                    {
                        var apiResp = JsonConvert.DeserializeObject<SPDMTResp>(response);
                        if (apiResp != null)
                        {
                            if (apiResp.message.Equals(SPDMTErrorCodes.MsgTranSucc) && apiResp.response_code.Equals(SPDMTErrorCodes.RespOne) && apiResp.status == true)
                            {
                                res.Statuscode = RechargeRespType.SUCCESS;
                                res.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                                res.ErrorCode = ErrorCodes.Transaction_Successful;
                                res.BeneName = apiResp.benename;
                                res.LiveID = apiResp.utr;
                                res.VendorID = apiResp.ackno.ToString();
                            }
                            else if (apiResp.message.In(SPDMTErrorCodes.MsgInvalidAcc) && apiResp.response_code.Equals(SPDMTErrorCodes.RespOne) && apiResp.status == true)
                            {
                                res.Msg = apiResp.message;
                                res.ErrorCode = ErrorCodes.Minus1;
                                res.Statuscode = SPDMTErrorCodes.RespThree;
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
            res.Request = _URL + "|" + JsonConvert.SerializeObject(headers) + "|" + JsonConvert.SerializeObject(spGetRemReq);
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
            TokenGeneration();
            var headers = new Dictionary<string, string>
            {
                { "Token", _JWTToken}
                //{ "Authorisedkey", appSetting.Authorisedkey}
            };
            var spGetRemReq = new SPDMTReq
            {
                mobile = request.SenderMobile,
                referenceid = request.TID,
                pipe = request.APIOpCode,//"pipe":"bank1|bank2|bank3",
                pincode = request.Pincode.ToString(),
                address = request.Address,
                dob = request.DOB,
                gst_state = request.StateID.ToString(),
                bene_id = Convert.ToInt32(request.mBeneDetail.BeneID),
                txntype = request.TransMode,
                amount = request.Amount
            };

            string response = string.Empty;
            var _URL = appSetting.DMTURL + "/transact/transact";

            try
            {
                response = AppWebRequest.O.PostJsonDataUsingHWRTLS(_URL, spGetRemReq, headers).Result;

                if (!string.IsNullOrEmpty(response))
                {
                    var _apiRes = JsonConvert.DeserializeObject<SPDMTResp>(response);
                    if (_apiRes != null)
                    {

                        if (_apiRes.response_code.Equals(SPDMTErrorCodes.RespOne) && _apiRes.status == true)
                        {
                            res.Statuscode = RechargeRespType.SUCCESS;
                            res.Msg = _apiRes.message;
                            res.ErrorCode = 0;
                            res.VendorID = _apiRes.ackno.ToString();
                            res.LiveID = _apiRes.utr;
                            res.BeneName = request.mBeneDetail.BeneName;

                        }
                        else if (_apiRes.status == true && _apiRes.txn_status == 3 && _apiRes.response_code == 0)
                        {
                            res.Statuscode = RechargeRespType.FAILED;
                            res.Msg = _apiRes.message;
                            res.ErrorCode = 0;
                            res.VendorID = _apiRes.ackno.ToString();
                            res.LiveID = string.Empty;
                            res.BeneName = request.mBeneDetail.BeneName;
                        }
                        else if (_apiRes.status == true && _apiRes.txn_status == 2 && _apiRes.response_code == 6)
                        {
                            res.Statuscode = RechargeRespType.PENDING;
                            res.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                            res.ErrorCode = ErrorCodes.Transaction_Successful;
                            res.VendorID = _apiRes.ackno.ToString();
                        }
                        else
                        {
                            res.Statuscode = RechargeRespType.FAILED;
                            res.Msg = _apiRes.message;
                            res.ErrorCode = 0;
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
            res.Request = _URL + "|" + JsonConvert.SerializeObject(headers) + "|" + JsonConvert.SerializeObject(spGetRemReq);
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
            TokenGeneration();
            var headers = new Dictionary<string, string>
            {
                { "Token", _JWTToken}
                //{ "Authorisedkey", appSetting.Authorisedkey}
            };
            var spGetRemReq = new SPDMTReq
            {
                referenceid = TID
            };
            string response = string.Empty;
            var _URL = appSetting.DMTURL + "/transact/transact/querytransact";
            try
            {
                response = AppWebRequest.O.PostJsonDataUsingHWRTLS(_URL, spGetRemReq, headers).Result;
                if (response != null)
                {
                    var rpFintechSts = JsonConvert.DeserializeObject<SPDMTResp>(response);
                    if (rpFintechSts != null)
                    {

                        if (rpFintechSts.response_code == 1 && rpFintechSts.txn_status == 1)
                        {
                            res.VendorID = rpFintechSts.ackno.ToString();
                            res.LiveID = rpFintechSts.utr;
                            res.Statuscode = RechargeRespType.SUCCESS;
                            res.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                            res.ErrorCode = ErrorCodes.Transaction_Successful;
                        }
                        if (rpFintechSts.response_code == 0 && rpFintechSts.txn_status == 0)
                        {
                            res.VendorID = rpFintechSts.ackno.ToString();
                            res.LiveID = rpFintechSts.utr.ToString();
                            res.Statuscode = RechargeRespType.FAILED;
                            res.Msg = nameof(ErrorCodes.Unknown_Error).Replace("_", " ");
                            res.ErrorCode = ErrorCodes.Unknown_Error;
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
                Request = _URL + "|" + JsonConvert.SerializeObject(headers) + "|" + JsonConvert.SerializeObject(spGetRemReq),
                SenderNo = string.Empty,
                UserID = UserID,
                TID = TID.ToString()
            };
            new ProcUpdateLogDMRReqResp(_dal).Call(dMTReq);
            res.Request = dMTReq.Request;
            res.Response = response;
            return res;
        }

        public ResponseStatus RefundOTP(string VendorID, int RequestMode, int UserID, string SenderNo, int TID, int APIID)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = nameof(ErrorCodes.Unknown_Error).Replace("_", " "),
                ErrorCode = ErrorCodes.Unknown_Error
            };
            if (string.IsNullOrEmpty(VendorID))
                VendorID = "0";

            TokenGeneration();
            var headers = new Dictionary<string, string>
            {
                { "Token", _JWTToken}
                //{ "Authorisedkey", appSetting.Authorisedkey}
            };

            var spGetRemReq = new SPDMTReq
            {
                referenceid = TID,
                ackno = Convert.ToInt32(VendorID)
            };
            string response = string.Empty;
            var _URL = appSetting.DMTURL + "/refund/refund/resendotp";
            try
            {
                response = AppWebRequest.O.PostJsonDataUsingHWRTLS(_URL, spGetRemReq, headers).Result;
                if (!string.IsNullOrEmpty(response))
                {
                    var _apiRes = JsonConvert.DeserializeObject<SPDMTResp>(response);
                    if (_apiRes != null)
                    {
                        if (_apiRes.message.Equals(SPDMTErrorCodes.RefundOTPSuc) && _apiRes.response_code == SPDMTErrorCodes.RespOne && _apiRes.status == true)
                        {
                            res.Statuscode = ErrorCodes.One;
                            res.Msg = nameof(DMTErrorCodes.OTP_for_verification_has_been_sent_successfully).Replace("_", " ");
                            res.ErrorCode = DMTErrorCodes.OTP_for_verification_has_been_sent_successfully;
                        }
                        else if (_apiRes.message.Equals(SPDMTErrorCodes.NoRecFound) && _apiRes.response_code == SPDMTErrorCodes.RespThree && _apiRes.status == false)
                        {
                            res.Statuscode = ErrorCodes.Minus1;
                            res.Msg = _apiRes.message;
                            res.ErrorCode = ErrorCodes.Minus1;
                        }
                        else
                        {
                            res.Msg = nameof(DMTErrorCodes.Declined_by_ServiceProvider).Replace("_", " ");
                            res.ErrorCode = DMTErrorCodes.Declined_by_ServiceProvider;
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
                    FuncName = "ResendRefundOTP",
                    Error = ex.Message,
                    LoginTypeID = 1,
                    UserId = UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            var dMTReq = new DMTReqRes
            {
                APIID = APIID,
                Method = "RefundOTP",
                RequestModeID = RequestMode,
                Request = _URL + JsonConvert.SerializeObject(spGetRemReq) + JsonConvert.SerializeObject(headers),
                Response = response,
                SenderNo = SenderNo,
                UserID = UserID,
                TID = TID.ToString()
            };
            new ProcUpdateLogDMRReqResp(_dal).Call(dMTReq);
            return res;
        }
        public ResponseStatus Refund(int TID, string VendorID, int RequestMode, int UserID, string SenderNo, string OTP, int APIID)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = nameof(ErrorCodes.Unknown_Error).Replace("_", " "),
                ErrorCode = ErrorCodes.Unknown_Error
            };
            TokenGeneration();
            var headers = new Dictionary<string, string>
            {
                { "Token", _JWTToken}
                //{ "Authorisedkey", appSetting.Authorisedkey}
            };
            var spGetRemReq = new SPDMTReq
            {
                referenceid = TID,
                ackno = Convert.ToInt32(VendorID),
                otp = OTP
            };
            string response = string.Empty;
            var _URL = appSetting.DMTURL + "/refund/refund/";
            try
            {
                response = AppWebRequest.O.PostJsonDataUsingHWRTLS(_URL, spGetRemReq, headers).Result;
                if (!string.IsNullOrEmpty(response))
                {
                    var _apiRes = JsonConvert.DeserializeObject<SPDMTResp>(response);
                    if (_apiRes != null)
                    {
                        if (_apiRes.status == false && _apiRes.response_code == 3 && _apiRes.message.Equals(SPDMTErrorCodes.MsgInvalidOTP))
                        {
                            res.Msg = nameof(ErrorCodes.Invalid_OTP).Replace("_", " ");
                            res.ErrorCode = ErrorCodes.Invalid_OTP;
                        }
                        else if (_apiRes.status == true && _apiRes.response_code == 1 && _apiRes.message.Equals(SPDMTErrorCodes.TranSuccRef))
                        {
                            res.Statuscode = ErrorCodes.One;
                            res.Msg = nameof(DMTErrorCodes.Transaction_Status_Updated_Refund).Replace("_", " ");
                            res.ErrorCode = DMTErrorCodes.Transaction_Status_Updated_Refund;
                        }
                    }
                    else 
                    {
                        res.Msg = nameof(ErrorCodes.Invalid_OTP).Replace("_", " ");
                        res.ErrorCode = ErrorCodes.Invalid_OTP;
                    }
                }
            }
            catch (Exception ex)
            {
                response = " Exception:" + ex.Message + " | " + response;
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Refund",
                    Error = ex.Message,
                    LoginTypeID = 1,
                    UserId = UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            var dMTReq = new DMTReqRes
            {
                APIID = APIID,
                Method = "Refund",
                RequestModeID = RequestMode,
                Request = _URL + JsonConvert.SerializeObject(spGetRemReq) + JsonConvert.SerializeObject(headers),
                Response = response,
                SenderNo = SenderNo,
                UserID = UserID,
                TID = TID.ToString()
            };
            new ProcUpdateLogDMRReqResp(_dal).Call(dMTReq);
            res.CommonStr = dMTReq.Request;
            res.CommonStr2 = dMTReq.Response;
            return res;
        }
    }
}