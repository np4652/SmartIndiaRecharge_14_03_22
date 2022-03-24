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
using RoundpayFinTech_bA.AppCode.ThirdParty.BillAvenue;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Validators;
using System.Linq;

namespace RoundpayFinTech.AppCode.MiddleLayer.Dmt_Api
{
    public partial class BillAvenueMT_ML
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        private readonly IConfiguration Configuration;

        private readonly string _APICode;
        private readonly int _APIID;
        private readonly IDAL _dal;

        public readonly BAAPISetting apiSetting;

        public BillAvenueMT_ML(IHttpContextAccessor accessor, IHostingEnvironment env, string APICode, int APIID, IDAL dal)
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
        public BAAPISetting AppSetting()
        {
            var setting = new BAAPISetting();
            try
            {
                setting = new BAAPISetting
                {
                    accessCode = Configuration["DMR:" + _APICode + ":accessCode"],
                    agentId = Configuration["DMR:" + _APICode + ":agentId"],
                    DMTServiceURL = Configuration["DMR:" + _APICode + ":DMTServiceURL"],
                    DMTTransactionURL = Configuration["DMR:" + _APICode + ":DMTTransactionURL"],
                    instituteId = Configuration["DMR:" + _APICode + ":instituteId"],
                    Key = Configuration["DMR:" + _APICode + ":Key"],
                    ver = Configuration["DMR:" + _APICode + ":ver"]
                };
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "BAAPISetting:" + (_APICode ?? string.Empty),
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
            fintechRequest.Token = apiSetting.instituteId;
            try
            {
                Req = apiSetting.DMTServiceURL + "API/Outlet/ServiceStatus?" + JsonConvert.SerializeObject(fintechRequest);
                Resp = AppWebRequest.O.PostJsonDataUsingHWR(apiSetting.DMTServiceURL + "API/Outlet/ServiceStatus/", fintechRequest);
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
    public partial class BillAvenueMT_ML : IMoneyTransferAPIML
    {

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
        public MSenderCreateResp CreateSender(MTAPIRequest request)
        {
            var res = new MSenderCreateResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError,
                ErrorCode = ErrorCodes.Unknown_Error
            };
            var req = new dmtServiceRequest
            {
                requestType = BA_ConstValue.SENDERREGISTER,
                senderMobileNumber = request.SenderMobile,
                txnType = BA_ConstValue.IMPS,
                senderName = request.FirstName + "" + request.LastName,
                senderPin = request.Pincode.ToString(),
                uniqueRefId = request.TransactionID.PadRight(35, 'M')               
            };

            string response = string.Empty;
            var _URL = apiSetting.DMTServiceURL;
            BA_AesCryptUtil bA_AesCryptUtil = new BA_AesCryptUtil(apiSetting.Key);
            var xmlReq = XMLHelper.O.SerializeToXml(req, "dmtServiceRequest", false);
            var encRequest = bA_AesCryptUtil.encrypt(xmlReq.ToString());
            var reqID = request.TID.ToString();
            //string vData = string.Format("accessCode={0}&encRequest={1}&requestId={2}&ver={3}&instituteId={4}", apiSetting.accessCode, encRequest, reqID.PadRight(35, 'M'), apiSetting.ver, apiSetting.instituteId);
            StringBuilder vData = new StringBuilder("accessCode={accessCode}&encRequest={encRequest}&requestId={requestId}&ver={ver}&instituteId={instituteId}");
            vData.Replace("{accessCode}", apiSetting.accessCode);
            vData.Replace("{encRequest}", encRequest);
            vData.Replace("{requestId}", request.TransactionID.PadRight(35, 'M'));
            vData.Replace("{ver}", apiSetting.ver);
            vData.Replace("{instituteId}", apiSetting.instituteId);

            try
            {
                response = AppWebRequest.O.CallUsingHttpWebRequest_POST(_URL, vData.ToString());
                //response = @"d5ae2f06dcdb1967";
                var decResp = bA_AesCryptUtil.decrypt(response);
                response += "/AfterDecrypt: " + decResp;

                if (!string.IsNullOrEmpty(response))
                {
                    var objBADMT = new dmtServiceResponse();
                    var _apiRes = XMLHelper.O.DesrializeToObject(objBADMT, decResp, null);
                    if (_apiRes != null)
                    {
                        if (_apiRes.responseCode.Equals("000"))
                        {
                            // res.Statuscode = Convert.ToInt32(_apiRes.responseCode);
                            res.Statuscode = ErrorCodes.One;
                            res.Msg = _apiRes.respDesc;
                            res.IsOTPGenerated = true;
                            res.IsOTPResendAvailble = true;
                            res.ReferenceID = _apiRes.additionalRegData;
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
            var xdmtReq = new dmtServiceRequest();
            var req = new dmtServiceRequest
            {
                requestType = BA_ConstValue.SENDERDETAILS,
                senderMobileNumber = request.SenderMobile,
                txnType = BA_ConstValue.IMPS,
                //uniqueRefId = request.TransactionID.PadRight(35, 'M')
            };
            string response = string.Empty;
            var _URL = apiSetting.DMTServiceURL;

            BA_AesCryptUtil bA_AesCryptUtil = new BA_AesCryptUtil(apiSetting.Key);
            var xmlReq = XMLHelper.O.SerializeToXml(req, "dmtServiceRequest", false);
            var encRequest = bA_AesCryptUtil.encrypt(xmlReq.ToString());
            var reqID = request.TID.ToString();
            StringBuilder sbvData = new StringBuilder("accessCode={accessCode}&encRequest={encRequest}&requestId={requestId}&ver={ver}&instituteId={instituteId}");
            sbvData.Replace("{accessCode}", apiSetting.accessCode);
            sbvData.Replace("{encRequest}", encRequest);
            sbvData.Replace("{requestId}", request.TransactionID.PadRight(35, 'M'));
            sbvData.Replace("{ver}", apiSetting.ver);
            sbvData.Replace("{instituteId}", apiSetting.instituteId);

            try
            {
                response = AppWebRequest.O.CallUsingHttpWebRequest_POST(_URL, sbvData.ToString());
                //response = @"d5aef5c0";
                var decResp = bA_AesCryptUtil.decrypt(response);
                response += "/AfterDecrypt: " + decResp;
                if (!string.IsNullOrEmpty(response))
                {
                    var objBADMT = new dmtServiceResponse();
                    var _apiRes = XMLHelper.O.DesrializeToObject(objBADMT, decResp, null);
                    if (_apiRes != null)
                    {
                        if (!string.IsNullOrEmpty(_apiRes.responseCode))
                        {
                            if (_apiRes.responseCode.Equals("200") && _apiRes.errorInfo.error.errorCode.Equals("DMT050"))
                            {
                                res.Statuscode = ErrorCodes.One;
                                res.Msg = _apiRes.errorInfo.error.errorMessage;
                                res.ErrorCode = DMTErrorCodes.Sender_Not_Found;
                                res.IsSenderNotExists = true;
                            }
                            else if (_apiRes.responseCode.Equals("000"))
                            {
                                res.Statuscode = ErrorCodes.One;
                                res.Msg = nameof(DMTErrorCodes.Detail_Found_Successfully).Replace("_", " ");
                                res.ErrorCode = DMTErrorCodes.Detail_Found_Successfully;
                                res.SenderName = _apiRes.senderName;
                                res.RemainingLimit = _apiRes.availableLimit;
                                res.AvailbleLimit = _apiRes.totalLimit;
                            }
                            else if (_apiRes.responseCode.Equals("200") && string.IsNullOrEmpty(_apiRes.errorInfo.error.errorCode))
                            {
                                res.Statuscode = ErrorCodes.Minus1;
                                res.Msg = nameof(ErrorCodes.FAILED);
                                res.ErrorCode = Convert.ToInt32(_apiRes.responseCode);
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
                Request = _URL + sbvData.ToString() + JsonConvert.SerializeObject(req),
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

            var xdmtReq = new dmtServiceRequest();
            var req = new dmtServiceRequest
            {
                requestType = BA_ConstValue.VERIFYSENDER,
                senderMobileNumber = request.SenderMobile,
                txnType = BA_ConstValue.IMPS,
                otp = request.OTP,
                additionalRegData = request.ReferenceID
            };

            string response = string.Empty;
            var _URL = apiSetting.DMTServiceURL;

            BA_AesCryptUtil bA_AesCryptUtil = new BA_AesCryptUtil(apiSetting.Key);
            var xmlReq = XMLHelper.O.SerializeToXml(req, "dmtServiceRequest", false);
            var encRequest = bA_AesCryptUtil.encrypt(xmlReq.ToString());
            var reqID = request.TID.ToString();
            StringBuilder sbvData = new StringBuilder("accessCode={accessCode}&encRequest={encRequest}&requestId={requestId}&ver={ver}&instituteId={instituteId}");
            sbvData.Replace("{accessCode}", apiSetting.accessCode);
            sbvData.Replace("{encRequest}", encRequest);
            sbvData.Replace("{requestId}", request.TransactionID.PadRight(35, 'M'));
            sbvData.Replace("{ver}", apiSetting.ver);
            sbvData.Replace("{instituteId}", apiSetting.instituteId);
            try
            {
                response = AppWebRequest.O.CallUsingHttpWebRequest_POST(_URL, sbvData.ToString());
                //response = @"d5ae2f06dcd7aa10834241274355c4e218abedbb1f097e6a461";
                var decResp = bA_AesCryptUtil.decrypt(response);
                response += "/AfterDecrypt: " + decResp;
                if (!string.IsNullOrEmpty(response))
                {
                    var objBADMT = new dmtServiceResponse();
                    var _apiRes = XMLHelper.O.DesrializeToObject(objBADMT, decResp, null);
                    if (_apiRes != null)
                    {
                        if (_apiRes.responseCode == "000" && _apiRes.responseReason == "Successful")
                        {
                            res.Statuscode = ErrorCodes.One;
                            res.Msg = nameof(DMTErrorCodes.Sender_Created_Successfully).Replace("_", " ");
                            res.ErrorCode = DMTErrorCodes.Sender_Created_Successfully;
                        }
                        else if (_apiRes.responseCode == "200" && _apiRes.errorInfo.error.errorCode == "DMT031")
                        {
                            res.Msg = nameof(ErrorCodes.Invalid_OTP).Replace("_", " ");
                            res.ErrorCode = ErrorCodes.Invalid_OTP;
                        }
                        else if (_apiRes.responseCode != null)
                        {
                            IErrorCodeML errorCodeML = new ErrorCodeML(_accessor, _env, false);
                            var eFromDB = errorCodeML.GetAPIErrorCodeDescription(request.APIGroupCode, _apiRes.responseCode);
                            if (!string.IsNullOrEmpty(eFromDB.Code))
                            {
                                res.Msg = eFromDB.Error;
                                res.ErrorCode = Convert.ToInt32(eFromDB.Code.Trim());
                            }
                        }
                        //else
                        //{
                        //    res.Statuscode = ErrorCodes.Minus1;
                        //    res.Msg = _apiRes.respDesc;
                        //    res.ErrorCode = Convert.ToInt32(_apiRes.responseCode);
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
                //Request = _URL + JsonConvert.SerializeObject(req),
                Request = _URL + sbvData.ToString() + JsonConvert.SerializeObject(req),
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
            var xdmtReq = new dmtServiceRequest();
            IBankML bankML = new BankML(_accessor, _env, false);
            var BankDetail = bankML.BankMasters(request.BankID);
            if (string.IsNullOrEmpty(BankDetail.BAVENVEBankID))
            {
                res.Statuscode = RechargeRespType.FAILED;
                res.Msg = nameof(DMTErrorCodes.Acquiring_Bank_CBS_or_node_offline).Replace("_", " ");
                res.ErrorCode = DMTErrorCodes.Acquiring_Bank_CBS_or_node_offline;
                return res;
            }
            var req = new dmtServiceRequest
            {
                requestType = BA_ConstValue.REGISTERRECIPIENT,
                senderMobileNumber = request.SenderMobile,
                txnType = BA_ConstValue.IMPS,
                recipientName = request.mBeneDetail.BeneName,
                recipientMobileNumber = request.mBeneDetail.MobileNo,
                bankCode = BankDetail.BAVENVEBankID,
                bankAccountNumber = request.mBeneDetail.AccountNo,
                ifsc = request.mBeneDetail.IFSC
            };

            string response = string.Empty;
            var _URL = apiSetting.DMTServiceURL;

            BA_AesCryptUtil bA_AesCryptUtil = new BA_AesCryptUtil(apiSetting.Key);
            var xmlReq = XMLHelper.O.SerializeToXml(req, "dmtServiceRequest", false);
            var encRequest = bA_AesCryptUtil.encrypt(xmlReq.ToString());
            var reqID = request.TID.ToString();
            StringBuilder sbvData = new StringBuilder("accessCode={accessCode}&encRequest={encRequest}&requestId={requestId}&ver={ver}&instituteId={instituteId}");
            sbvData.Replace("{accessCode}", apiSetting.accessCode);
            sbvData.Replace("{encRequest}", encRequest);
            sbvData.Replace("{requestId}", request.TransactionID.PadRight(35, 'M'));
            sbvData.Replace("{ver}", apiSetting.ver);
            sbvData.Replace("{instituteId}", apiSetting.instituteId);

            try
            {
                response = AppWebRequest.O.CallUsingHttpWebRequest_POST(_URL, sbvData.ToString());
                var decResp = bA_AesCryptUtil.decrypt(response);
                response += "/AfterDecrypt: " + decResp;
                if (!string.IsNullOrEmpty(response))
                {
                    var objBADMT = new dmtServiceResponse();
                    var _apiRes = XMLHelper.O.DesrializeToObject(objBADMT, decResp, null);
                    if (_apiRes != null)
                    {
                        if (_apiRes.responseCode.Equals("000"))
                        {
                            res.Statuscode = ErrorCodes.One;
                            res.Msg = _apiRes.respDesc;
                            res.ErrorCode = Convert.ToInt32(_apiRes.responseCode);
                            //res.BeneID = _apiRes.BeneID;
                            //res.ReferenceID = _apiRes.ReferenceID;
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
            var xdmtReq = new dmtServiceRequest();
            var req = new dmtServiceRequest
            {
                requestType = BA_ConstValue.ALLRECIPIENT,
                senderMobileNumber = request.SenderMobile,
                txnType = BA_ConstValue.IMPS
            };

            string response = string.Empty;
            var _URL = apiSetting.DMTServiceURL;

            BA_AesCryptUtil bA_AesCryptUtil = new BA_AesCryptUtil(apiSetting.Key);
            var xmlReq = XMLHelper.O.SerializeToXml(req, "dmtServiceRequest", false);
            var encRequest = bA_AesCryptUtil.encrypt(xmlReq.ToString());
            var reqID = request.TID.ToString();
            StringBuilder sbvData = new StringBuilder("accessCode={accessCode}&encRequest={encRequest}&requestId={requestId}&ver={ver}&instituteId={instituteId}");
            sbvData.Replace("{accessCode}", apiSetting.accessCode);
            sbvData.Replace("{encRequest}", encRequest);
            sbvData.Replace("{requestId}",  request.TransactionID.PadRight(35, 'M'));
            sbvData.Replace("{ver}", apiSetting.ver);
            sbvData.Replace("{instituteId}", apiSetting.instituteId);
            try
            {
                response = AppWebRequest.O.CallUsingHttpWebRequest_POST(_URL, sbvData.ToString());

                var decResp = bA_AesCryptUtil.decrypt(response);
                response += "/AfterDecrypt: " + decResp;
                if (!string.IsNullOrEmpty(response))
                {
                    var objBADMT = new dmtServiceResponse
                    {
                        recipientList = new List<dmtRecipient>
                        {
                            new dmtRecipient{ }
                        }
                    };

                    var _apiRes = XMLHelper.O.DesrializeToObject(objBADMT, decResp, null);
                    if (_apiRes != null)
                    {
                        if (_apiRes.responseCode.Equals("000"))
                        {
                            res.Statuscode = ErrorCodes.One;
                            res.Msg = _apiRes.respDesc;
                            res.ErrorCode = Convert.ToInt32(_apiRes.responseCode);
                            if (_apiRes.recipientList != null)
                            {
                                //res.Beneficiaries = IEnumerable<dmtRecipient>(_apiRes.recipientList.dmtRecipient);
                                var Beneficiaries = new List<MBeneDetail>();
                                foreach (var item in _apiRes.recipientList)
                                {
                                    Beneficiaries.Add(new MBeneDetail
                                    {
                                        AccountNo = item.bankAccountNumber,
                                        BankName = item.bankName,
                                        IFSC = item.ifsc,
                                        BeneName = item.recipientName,
                                        BeneID = item.recipientId.ToString(),
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
            var req = new DeleteBeneReq
            {
                UserID = 0,
                Token = apiSetting.instituteId,
                OutletID = Validate.O.IsNumeric(request.APIOutletID ?? string.Empty) ? Convert.ToInt32(request.APIOutletID) : 0,
                SPKey = request.APIOpCode,
                SenderMobile = request.SenderMobile,
                ReferenceID = request.ReferenceID,
                BeneID = request.mBeneDetail.BeneID
            };
            string response = string.Empty;
            var _URL = apiSetting.DMTServiceURL + "APIMoneyTransfer/GenerateBenficiaryOTP";
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
                UserID = 0,
                Token = apiSetting.instituteId,
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
            var _URL = apiSetting.DMTServiceURL + "APIMoneyTransfer/ValidateBeneficiary";
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
            var xdmtReq = new dmtServiceRequest();
            var req = new dmtServiceRequest
            {
                requestType = BA_ConstValue.DELRECIPIENT,
                senderMobileNumber = request.SenderMobile,
                txnType = BA_ConstValue.IMPS,
                recipientId = request.mBeneDetail.BeneID
            };

            string response = string.Empty;
            var _URL = apiSetting.DMTServiceURL;

            BA_AesCryptUtil bA_AesCryptUtil = new BA_AesCryptUtil(apiSetting.Key);
            var xmlReq = XMLHelper.O.SerializeToXml(req, "dmtServiceRequest", false);
            var encRequest = bA_AesCryptUtil.encrypt(xmlReq.ToString());
            var reqID = request.TID.ToString();
            StringBuilder sbvData = new StringBuilder("accessCode={accessCode}&encRequest={encRequest}&requestId={requestId}&ver={ver}&instituteId={instituteId}");
            sbvData.Replace("{accessCode}", apiSetting.accessCode);
            sbvData.Replace("{encRequest}", encRequest);
            sbvData.Replace("{requestId}", request.TransactionID.PadRight(35, 'M'));
            sbvData.Replace("{ver}", apiSetting.ver);
            sbvData.Replace("{instituteId}", apiSetting.instituteId);
            try
            {
                response = AppWebRequest.O.CallUsingHttpWebRequest_POST(_URL, sbvData.ToString());
                var decResp = bA_AesCryptUtil.decrypt(response);
                response += "/AfterDecrypt: " + decResp;
                if (!string.IsNullOrEmpty(response))
                {
                    var objBADMT = new dmtServiceResponse();
                    var _apiRes = XMLHelper.O.DesrializeToObject(objBADMT, decResp, null);
                    if (_apiRes != null)
                    {
                        if (_apiRes.responseCode.Equals("000"))
                        {
                            senderLoginResponse.Statuscode = ErrorCodes.One;
                            senderLoginResponse.Msg = _apiRes.respDesc;
                            senderLoginResponse.ErrorCode = Convert.ToInt32(_apiRes.responseCode);
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
                //Request = _URL + "|" + JsonConvert.SerializeObject(req),
                Request = _URL + sbvData.ToString() + JsonConvert.SerializeObject(req),
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
                UserID = 0,
                Token = apiSetting.instituteId,
                OutletID = Validate.O.IsNumeric(request.APIOutletID ?? string.Empty) ? Convert.ToInt32(request.APIOutletID) : 0,
                SPKey = request.APIOpCode,
                SenderMobile = request.SenderMobile,
                ReferenceID = request.ReferenceID,
                BeneID = request.mBeneDetail.BeneID,
                OTP = request.OTP
            };
            string response = string.Empty;
            var _URL = apiSetting.DMTServiceURL + "APIMoneyTransfer/ValidateRemoveBeneficiaryOTP";
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
            var xdmtReq = new dmtServiceRequest();
            var req = new dmtServiceRequest
            {
                requestType = BA_ConstValue.VERIFYACCOUNT,
                agentId = apiSetting.agentId,
                initChannel = BA_ConstValue.AGT,
                senderMobileNumber = request.SenderMobile,
                bankAccountNumber = request.mBeneDetail.AccountNo,
                ifsc = request.mBeneDetail.IFSC,
                //bankCode= request.mBeneDetail.IFSC,
                bankCode = request.mBeneDetail.IFSC.Substring(0, 4),
                recipientName = request.mBeneDetail.BeneName
            };
            string response = string.Empty;
            var _URL = apiSetting.DMTServiceURL;

            BA_AesCryptUtil bA_AesCryptUtil = new BA_AesCryptUtil(apiSetting.Key);
            var xmlReq = XMLHelper.O.SerializeToXml(req, "dmtServiceRequest", false);
            var encRequest = bA_AesCryptUtil.encrypt(xmlReq.ToString());
            var reqID = request.TID.ToString();
            StringBuilder sbvData = new StringBuilder("accessCode={accessCode}&encRequest={encRequest}&requestId={requestId}&ver={ver}&instituteId={instituteId}");
            sbvData.Replace("{accessCode}", apiSetting.accessCode);
            sbvData.Replace("{encRequest}", encRequest);
            sbvData.Replace("{requestId}", request.TransactionID.PadRight(35, 'M'));
            sbvData.Replace("{ver}", apiSetting.ver);
            sbvData.Replace("{instituteId}", apiSetting.instituteId);

            try
            {
                response = AppWebRequest.O.CallUsingHttpWebRequest_POST(_URL, sbvData.ToString());
                var decResp = bA_AesCryptUtil.decrypt(response);
                response += "/AfterDecrypt: " + decResp;
                if (!string.IsNullOrEmpty(response))
                {
                    var objBADMT = new dmtServiceResponse();
                    var _apiRes = XMLHelper.O.DesrializeToObject(objBADMT, decResp, null);
                    if (_apiRes != null)
                    {
                        if (_apiRes.responseCode.Equals("000"))
                        {
                            //res.Statuscode = RechargeRespType.SUCCESS;
                            //res.Msg = _apiRes.respDesc;
                            res.ErrorCode = 0;
                            res.SenderName = _apiRes.impsName;
                            res.Response = _apiRes.respDesc;
                            res.Statuscode = RechargeRespType.SUCCESS;
                            //res.Statuscode = Convert.ToInt32(_apiRes.responseCode);
                            res.Msg = _apiRes.responseReason;
                            res.SenderMobileNo = _apiRes.senderMobileNumber;
                            res.BeneName = _apiRes.impsName;
                        }
                        else
                        {
                            //res.Msg = nameof(DMTErrorCodes.Transaction_Failed);
                            //res.ErrorCode = DMTErrorCodes.Transaction_Failed;
                            //res.LiveID = res.Msg;
                            res.Statuscode = Convert.ToInt32(_apiRes.responseCode);
                            res.Status = _apiRes.responseReason;
                            res.Msg = _apiRes.errorInfo.error.errorMessage;
                            res.ErrorCode = Convert.ToInt32(_apiRes.errorInfo.error.errorCode);
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
                //Request = res.Request,
                Request = _URL + sbvData.ToString() + JsonConvert.SerializeObject(req),
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
            var convFee = request.Amount * .01 < 10 ? 10 : (request.Amount * .01);
            var req = new dmtServiceRequest
            {
                requestType = BA_ConstValue.FUNDTRANSFER,
                senderMobileNo = request.SenderMobile,
                senderMobileNumber = request.SenderMobile,
                agentId = apiSetting.agentId,
                initChannel = BA_ConstValue.AGT,
                recipientId = request.mBeneDetail.BeneID,
                txnAmount = (request.Amount * 100).ToString(),
                convFee = (convFee * 100).ToString(),
                txnType = BA_ConstValue.IMPS,
            };

            string response = string.Empty;
            var _URL = apiSetting.DMTTransactionURL;
            BA_AesCryptUtil bA_AesCryptUtil = new BA_AesCryptUtil(apiSetting.Key);
            var xmlReq = XMLHelper.O.SerializeToXml(req, "dmtTransactionRequest", false);
            var encRequest = bA_AesCryptUtil.encrypt(xmlReq.ToString());
            var reqID = request.TID.ToString();
            string gid = Guid.NewGuid().ToString("N");
            char pad = '0';
            StringBuilder sbvData = new StringBuilder("accessCode={accessCode}&encRequest={encRequest}&requestId={requestId}&ver={ver}&instituteId={instituteId}");
            sbvData.Replace("{accessCode}", apiSetting.accessCode);
            sbvData.Replace("{encRequest}", encRequest);
            sbvData.Replace("{requestId}", gid.PadLeft(35, pad));
            sbvData.Replace("{ver}", apiSetting.ver);
            sbvData.Replace("{instituteId}", apiSetting.instituteId);
            try
            {
                response = AppWebRequest.O.CallUsingHttpWebRequest_POST(_URL, sbvData.ToString());
                var decResp = bA_AesCryptUtil.decrypt(response);
                response += "/AfterDecrypt: " + decResp;
                if (!string.IsNullOrEmpty(response))
                {
                    var objBADMT = new dmtTransactionResponse
                    {
                        fundTransferDetails = new List<fundDetail>
                        {
                            new fundDetail{ }
                        }
                    };
                    var _apiRes = XMLHelper.O.DesrializeToObject(objBADMT, decResp, null);
                    if (_apiRes != null)
                    {
                        if (_apiRes.responseCode.Equals("000"))
                        {
                            res.Statuscode = RechargeRespType.SUCCESS;
                            res.Msg = _apiRes.respDesc;
                            res.ErrorCode = 0;
                            if (_apiRes.fundTransferDetails != null && _apiRes.fundTransferDetails.Count > 0)
                            {
                                res.VendorID = _apiRes.fundTransferDetails[0].refId.ToString();
                                res.LiveID = _apiRes.fundTransferDetails[0].bankTxnId.ToString();
                                res.BeneName = _apiRes.fundTransferDetails[0].impsName.ToString();
                            }
                        }
                        else
                        {
                            res.Msg = nameof(DMTErrorCodes.Transaction_Failed);
                            res.ErrorCode = DMTErrorCodes.Transaction_Failed;
                            res.LiveID = res.Msg;
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
                //Request = res.Request,
                Request = _URL + sbvData.ToString() + JsonConvert.SerializeObject(req),
                Response = res.Response,
                SenderNo = request.SenderMobile,
                UserID = request.UserID,
                TID = request.TID.ToString()
            });
            return res;
        }

        public DMRTransactionResponse Refund(MTAPIRequest request)
        {
            var res = new DMRTransactionResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = nameof(ErrorCodes.Unknown_Error).Replace("_", " "),
                ErrorCode = ErrorCodes.Unknown_Error
            };
        
            string gid = Guid.NewGuid().ToString("N");
            char pad = '0';
            gid = gid.PadLeft(35, pad);

            var reqID = gid;
            var req = new dmtServiceRequest
            {
                requestType = BA_ConstValue.REFUND,
                agentId = apiSetting.agentId,
                initChannel = BA_ConstValue.AGT,
                txnId = request.TransactionID
            };

            string response = string.Empty;
            var _URL = apiSetting.DMTTransactionURL;
            BA_AesCryptUtil bA_AesCryptUtil = new BA_AesCryptUtil(apiSetting.Key);
            var xmlReq = XMLHelper.O.SerializeToXml(req, "dmtTransactionRequest", false);
            var encRequest = bA_AesCryptUtil.encrypt(xmlReq.ToString());

            StringBuilder sbvData = new StringBuilder("accessCode={accessCode}&encRequest={encRequest}&requestId={requestId}&ver={ver}&instituteId={instituteId}");
            sbvData.Replace("{accessCode}", apiSetting.accessCode);
            sbvData.Replace("{encRequest}", encRequest);
            sbvData.Replace("{requestId}", reqID);
            sbvData.Replace("{ver}", apiSetting.ver);
            sbvData.Replace("{instituteId}", apiSetting.instituteId);

            try
            {
                response = AppWebRequest.O.CallUsingHttpWebRequest_POST(_URL, sbvData.ToString());

                var decResp = bA_AesCryptUtil.decrypt(response);
                response += "/AfterDecrypt: " + decResp;

                if (!string.IsNullOrEmpty(response))
                {
                    var objBADMT = new dmtTransactionResponse();
                    var _apiRes = XMLHelper.O.DesrializeToObject(objBADMT, decResp, null);
                    if (_apiRes != null)
                    {
                        if (_apiRes.responseCode.Equals("000"))
                        {
                            res.Statuscode = RechargeRespType.SUCCESS;
                            res.Msg = nameof(DMTErrorCodes.OTP_for_verification_has_been_sent_successfully).Replace("_", " ");
                            res.ErrorCode = DMTErrorCodes.OTP_for_verification_has_been_sent_successfully;
                            res.IsRefundAvailable = true;
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
                response = ex.Message + "|" + response;
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Refund",
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
                Method = "Refund",
                RequestModeID = request.RequestMode,
                Request = _URL + sbvData.ToString() + JsonConvert.SerializeObject(req),
                Response = res.Response,
                SenderNo = request.SenderMobile,
                UserID = request.UserID,
                TID = request.TID.ToString()
            });
            return res;
        }

        public DMRTransactionResponse ResendRefundOTP(MTAPIRequest request)
        {
            var res = new DMRTransactionResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = nameof(ErrorCodes.Unknown_Error).Replace("_", " "),
                ErrorCode = ErrorCodes.Unknown_Error
            };

            string gid = Guid.NewGuid().ToString("N");
            char pad = '0';
            gid = gid.PadLeft(35, pad);

            var reqID = gid;
            var gd = request.TransactionID.ToString();

            if (request.TID < 114712)

                gd = request.GroupID.PadRight(35, 'M').ToString();

            else
                gd = request.TID.ToString().PadRight(35, 'M');

            var req = new dmtServiceRequest
            {
                requestType = BA_ConstValue.REFUNDOTP,
                agentId = apiSetting.agentId,
                initChannel = BA_ConstValue.AGT,
                txnId = request.TransactionID,
                uniqueRefId = gd,
                otp = request.OTP
            };

            string response = string.Empty;
            var _URL = apiSetting.DMTTransactionURL;
            BA_AesCryptUtil bA_AesCryptUtil = new BA_AesCryptUtil(apiSetting.Key);
            var xmlReq = XMLHelper.O.SerializeToXml(req, "dmtTransactionRequest", false);
            var encRequest = bA_AesCryptUtil.encrypt(xmlReq.ToString());


            StringBuilder sbvData = new StringBuilder("accessCode={accessCode}&encRequest={encRequest}&requestId={requestId}&ver={ver}&instituteId={instituteId}");
            sbvData.Replace("{accessCode}", apiSetting.accessCode);
            sbvData.Replace("{encRequest}", encRequest);
            sbvData.Replace("{requestId}", reqID);
            sbvData.Replace("{ver}", apiSetting.ver);
            sbvData.Replace("{instituteId}", apiSetting.instituteId);

            try
            {
                response = AppWebRequest.O.CallUsingHttpWebRequest_POST(_URL, sbvData.ToString());

                var decResp = bA_AesCryptUtil.decrypt(response);

                response += "/AfterDecrypt: " + decResp;


                if (!string.IsNullOrEmpty(response))
                {
                    var objBADMT = new dmtTransactionResponse
                    {
                        fundTransferDetails = new List<fundDetail>
                        {
                            new fundDetail{ }
                        }
                    };
                    var _apiRes = XMLHelper.O.DesrializeToObject(objBADMT, decResp, null);
                    if (_apiRes != null)
                    {
                        if (_apiRes.responseCode.Equals("000"))
                        {
                            res.Statuscode = RechargeRespType.SUCCESS;
                            res.Msg = _apiRes.respDesc;
                            res.ErrorCode = 0;
                            if (_apiRes.fundTransferDetails != null && _apiRes.fundTransferDetails.Count > 0)
                            {
                                res.Statuscode = ErrorCodes.One;
                                res.Msg = nameof(DMTErrorCodes.Transaction_Status_Updated_Refund).Replace("_", " ");
                                res.ErrorCode = DMTErrorCodes.Transaction_Status_Updated_Refund;
                            }
                            else if (!string.IsNullOrEmpty(_apiRes.txnId) && !string.IsNullOrEmpty(_apiRes.refundTxnId))
                            {
                                res.Statuscode = ErrorCodes.One;
                                res.Msg = nameof(DMTErrorCodes.Transaction_Status_Updated_Refund).Replace("_", " ");
                                res.ErrorCode = DMTErrorCodes.Transaction_Status_Updated_Refund;
                            }
                        }
                        else
                        {
                            res.Msg = nameof(DMTErrorCodes.Transaction_Failed);
                            res.ErrorCode = DMTErrorCodes.Transaction_Failed;
                            res.LiveID = res.Msg;
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
                    FuncName = "ResendRefundOTP",
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
                Method = "ResendRefundOTP",
                RequestModeID = request.RequestMode,
                Request = _URL + sbvData.ToString() + JsonConvert.SerializeObject(req),
                Response = res.Response,
                SenderNo = request.SenderMobile,
                UserID = request.UserID,
                TID = request.TID.ToString()
            });
            return res;
        }
        public DMRTransactionResponse GetTransactionStatus(MTAPIRequest request)
        {
            var res = new DMRTransactionResponse
            {
                Statuscode = RechargeRespType.PENDING,
                Msg = RechargeRespType._PENDING,
                VendorID = string.Empty,
                LiveID = string.Empty,
                ErrorCode = ErrorCodes.Request_Accpeted
            };

            string gid = Guid.NewGuid().ToString("N");
            char pad = '0';
            gid = gid.PadLeft(35, pad);

            var reqID = gid;

            var gd = request.TransactionID.ToString();

            if (request.TID < 114712)

                gd = request.GroupID.PadRight(35, 'M').ToString();

            else

                gd = request.TID.ToString().PadRight(35, 'M');


            var req = new dmtServiceRequest
            {
                requestType = BA_ConstValue.TransactionStatus,
                agentId = apiSetting.agentId,
                initChannel = BA_ConstValue.AGT,
                uniqueRefId = gd               
            };

            string response = string.Empty;
            var _URL = apiSetting.DMTTransactionURL;
            BA_AesCryptUtil bA_AesCryptUtil = new BA_AesCryptUtil(apiSetting.Key);
            var xmlReq = XMLHelper.O.SerializeToXml(req, "dmtTransactionRequest", false);

            var encRequest = bA_AesCryptUtil.encrypt(xmlReq.ToString());


            StringBuilder sbvData = new StringBuilder("accessCode={accessCode}&encRequest={encRequest}&requestId={requestId}&ver={ver}&instituteId={instituteId}");
            sbvData.Replace("{accessCode}", apiSetting.accessCode);
            sbvData.Replace("{encRequest}", encRequest);
            sbvData.Replace("{requestId}", reqID);
            sbvData.Replace("{ver}", apiSetting.ver);
            sbvData.Replace("{instituteId}", apiSetting.instituteId);

            try
            {
                response = AppWebRequest.O.CallUsingHttpWebRequest_POST(_URL, sbvData.ToString());       
                var decResp = bA_AesCryptUtil.decrypt(response);

                response += "/AfterDecrypt: " + decResp;

                if (!string.IsNullOrEmpty(response))
                {
                    var objBADMT = new dmtTransactionResponse
                    {
                        fundTransferDetails = new List<fundDetail>
                        {
                            new fundDetail{ }
                        }
                    };
                    var _apiRes = XMLHelper.O.DesrializeToObject(objBADMT, decResp, null);
                    if (request.IsRefundReq)
                    {
                        res.RDMTTxnID = _apiRes.fundTransferDetails[0].DmtTxnId;
                        res.RStsType = _apiRes.fundTransferDetails[0].txnStatus;
                        res.Msg = "Non Refundable";
                    }
                       
                    if (_apiRes != null)
                    {
                        foreach (var a in _apiRes.fundTransferDetails)
                        {
                            if (a.txnStatus != null)
                            {
                                if (a.txnStatus == "T")
                                {
                                    res.Msg = "Refundable";
                                    res.IsRefundAvailable = true;
                                    res.Statuscode = RechargeRespType.SUCCESS;
                                    res.LiveID = _apiRes.fundTransferDetails[0].bankTxnId;
                                    res.ErrorCode = ErrorCodes.Transaction_Successful;
                                }
                                else if (a.txnStatus == "C")
                                {
                                    res.Statuscode = RechargeRespType.SUCCESS;
                                    res.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                                    res.LiveID = _apiRes.fundTransferDetails[0].bankTxnId;
                                    res.ErrorCode = ErrorCodes.Transaction_Successful;
                                }
                                else
                                {
                                    res.Statuscode = RechargeRespType.FAILED;
                                    res.Msg = nameof(DMTErrorCodes.Transaction_Failed).Replace("_", " ");
                                    res.LiveID = _apiRes.fundTransferDetails[0].bankTxnId.ToString();
                                    res.ErrorCode = DMTErrorCodes.Transaction_Failed;
                                }
                            }
                            else
                            {
                                res.Msg = nameof(DMTErrorCodes.Transaction_Failed);
                                res.ErrorCode = DMTErrorCodes.Transaction_Failed;
                                res.LiveID = res.Msg;
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
                    FuncName = "GetTransactionStatus",
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
                Method = "GetTransactionStatus",
                RequestModeID = request.RequestMode,
                //Request = res.Request,
                Request = _URL + sbvData.ToString() + JsonConvert.SerializeObject(req),
                Response = res.Response,
                SenderNo = request.SenderMobile,
                UserID = request.UserID,
                TID = request.TID.ToString()
            });
            return res;
        }


    }

}