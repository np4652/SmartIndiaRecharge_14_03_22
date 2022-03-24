using Fintech.AppCode.Configuration;
using Fintech.AppCode.DB;
using Fintech.AppCode.HelperClass;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using Fintech.AppCode.WebRequest;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using RoundpayFinTech.AppCode.DL;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.MoneyTransfer;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.ThirdParty.Roundpay;
using RoundpayFinTech.AppCode.ThirdParty.YesBank;
using System;
using System.Collections.Generic;
using System.IO;
using Validators;

namespace RoundpayFinTech.AppCode.MiddleLayer.Dmt_Api
{
    public class YesBankML : IMoneyTransferAPIML
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        private readonly IConfiguration Configuration;
        private readonly IConnectionConfiguration _c;
        private readonly YesBankAppSetting appSetting;
        private readonly IDAL _dal;
        private static string YesBankSession;
        private static string YesBankWadh;
        private static DateTime YesBankExpiry;
        private static string TIDPrefix = "RTID";
        public YesBankML(IHttpContextAccessor accessor, IHostingEnvironment env, IDAL dal)
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
        private YesBankAppSetting AppSetting()
        {
            var setting = new YesBankAppSetting();
            try
            {
                setting = new YesBankAppSetting
                {
                    BCAGENT = Configuration["DMR:YESBNK:BCAGENT"],
                    CPID = Configuration["DMR:YESBNK:CPID"],
                    S = Configuration["DMR:YESBNK:S"],
                    P = Configuration["DMR:YESBNK:P"],
                    USERNAME = Configuration["DMR:YESBNK:USERNAME"],
                    PASSWORD = Configuration["DMR:YESBNK:PASSWORD"],
                    BASEURL = Configuration["DMR:YESBNK:BaseURL"]
                };
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "YesBankAppSetting",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser
                });
            }
            return setting;
        }

        private bool EKYCChannelPartnerLogin()
        {
            bool IsCallAPI = false;
            if (string.IsNullOrEmpty(YesBankSession))
            {
                IsCallAPI = true;
            }
            else if (YesBankExpiry == null)
            {
                IsCallAPI = true;
            }
            else if (YesBankExpiry < DateTime.Now)
            {
                IsCallAPI = true;
            }
            if (IsCallAPI)
            {
                var res = new channelpartnerloginforekycres();
                var request = new channelpartnerloginreqforekyc
                {
                    bcagent = appSetting.BCAGENT,
                    username = appSetting.USERNAME,
                    password = appSetting.PASSWORD
                };
                var _URL = appSetting.BASEURL;
                try
                {
                    var xmlReq = XMLHelper.O.SerializeToXml(request, null);
                    var _apiResp = AppWebRequest.O.CallUsingHttpWebRequest_POST(_URL, xmlReq, ContentType.application_atom_plus_xml);
                    if (!string.IsNullOrEmpty(_apiResp))
                    {
                        res = XMLHelper.O.DesrializeToObject(res, _apiResp, "");
                        if (res.status == 1)
                        {
                            YesBankSession = res.sessionid;
                            YesBankExpiry = Convert.ToDateTime(Validate.O.DTYB(res.timeout));
                            YesBankWadh = res.wadh;
                        }
                    }
                }
                catch (Exception ex)
                {
                    var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                    {
                        ClassName = GetType().Name,
                        FuncName = "EKYCChannelPartnerLogin",
                        Error = ex.Message,
                        LoginTypeID = LoginType.ApplicationUser,
                        UserId = 1
                    });
                }
            }
            if (string.IsNullOrEmpty(YesBankSession))
            {
                return false;
            }
            else if (YesBankExpiry == null)
            {
                return false;
            }
            else if (YesBankExpiry > DateTime.Now)
            {
                return true;
            }
            return false;
        }

        public MSenderCreateResp CreateSender(MTAPIRequest request)
        {
            EKYCChannelPartnerLogin();
            var res = new MSenderCreateResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError,
                ErrorCode = ErrorCodes.Unknown_Error
            };
            var req = new senderregistrationrmreq
            {
                header = new YesHeader
                {
                    sessionid = YesBankSession
                },
                bcagent = appSetting.BCAGENT,
                sendername = request.FirstName + " " + request.LastName,
                sendermobilenumber = request.SenderMobile,
                dob = Convert.ToDateTime(request.DOB).ToString("MM/dd/yyyy"),//
                pincode = request.Pincode,
                cityname = request.City,
                statename = request.StateName,
                senderaddress1 = request.Address,
                lsenderaddress = request.Address,
                senderaddress2 = request.Address,
                lpincode = request.Pincode,
                alternatenumber = request.SenderMobile,
                channelpartnerrefno = request.TransactionID,
                idproof = " ",
                idproofnumber = " ",
                idproofissuedate = " ",
                idproofexpirydate = " ",
                idproofissueplace = " ",
                lcityname = request.City,
                lstatename = request.StateName
            };
            var _URL = appSetting.BASEURL;
            var xmlReq = XMLHelper.O.SerializeToXml(req, null);
            xmlReq = xmlReq.Replace("> <", "><");
            string response = string.Empty;
            try
            {
                response = AppWebRequest.O.CallUsingHttpWebRequest_POST(_URL, xmlReq, ContentType.application_atom_plus_xml);
                if (!string.IsNullOrEmpty(response))
                {
                    var resErr = GetErrorIfExists(response);
                    if (resErr.Statuscode == ErrorCodes.One)
                    {
                        var _apiRes = new senderregistrationrmres();
                        _apiRes = XMLHelper.O.DesrializeToObject(_apiRes, response, null);
                        if (_apiRes.status == 1)
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
                                ReffID = _apiRes.remitterid.ToString()
                            });
                            res.Statuscode = ErrorCodes.One;
                            res.Msg = nameof(DMTErrorCodes.Sender_Created_Successfully).Replace("_", " ");
                            res.ErrorCode = DMTErrorCodes.Sender_Created_Successfully;
                            res.ReferenceID = _apiRes.senderid.ToString();
                        }
                        else
                        {
                            res.Msg = nameof(DMTErrorCodes.Transaction_Failed).Replace("_", " ");
                        }
                    }
                    else
                    {
                        res.Msg = resErr.Msg;
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
                Request = _URL + "|" + xmlReq,
                Response = response,
                SenderNo = request.SenderMobile,
                UserID = request.UserID,
                TID = request.TransactionID
            });
            return res;
        }
        public MSenderLoginResponse GetSender(MTAPIRequest request)
        {
            EKYCChannelPartnerLogin();
            var res = new MSenderLoginResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = nameof(ErrorCodes.Unknown_Error).Replace("_", " "),
                ErrorCode = ErrorCodes.Unknown_Error
            };
            var req = new senderdetailsreq
            {
                header = new YesHeader
                {
                    sessionid = YesBankSession
                },
                bcagent = appSetting.BCAGENT,
                mobilenumber = request.SenderMobile,
                flag = 1
            };
            var _URL = appSetting.BASEURL;
            var xmlReq = XMLHelper.O.SerializeToXml(req, null);
            string response = string.Empty;
            try
            {
                response = AppWebRequest.O.CallUsingHttpWebRequest_POST(_URL, xmlReq, ContentType.application_atom_plus_xml);
                if (!string.IsNullOrEmpty(response))
                {
                    var errorCheck = GetErrorIfExists(response);
                    if (errorCheck.Statuscode == ErrorCodes.One)
                    {
                        response = response.Replace("<mmid />", "").Replace("<bankbenename />", "").Replace("<receiveremailid />", "");
                        var _apiRes = new senderdetailsres();
                        _apiRes = XMLHelper.O.DesrializeToObject(_apiRes, response, null);
                        if (_apiRes.status == 1)
                        {
                            res.SenderName = _apiRes.senderdetail.sendername;
                            res.ReferenceID = _apiRes.senderdetail.senderid.ToString();
                            if (_apiRes.senderdetail.kycstatus == 2)
                                res.KYCStatus = SenderKYCStatus.ACTIVE;
                            if (_apiRes.senderdetail.kycstatus == 3)
                                res.KYCStatus = SenderKYCStatus.REJECTED;
                            if (_apiRes.senderdetail.kycstatus == 1)
                                res.KYCStatus = SenderKYCStatus.APPLIED;
                            res.IsEKYCAvailable = true;
                            res.RemainingLimit = Convert.ToDecimal(_apiRes.senderdetail.AviableLimit);
                            res.AvailbleLimit = Convert.ToDecimal(_apiRes.senderdetail.AviableLimit);
                            res.Statuscode = ErrorCodes.One;
                            res.Msg = nameof(DMTErrorCodes.Detail_Found_Successfully).Replace("_", " ");
                            res.ErrorCode = DMTErrorCodes.Detail_Found_Successfully;
                        }
                        else
                        {
                            res.Statuscode = ErrorCodes.Minus1;
                            res.Msg = nameof(DMTErrorCodes.Sender_Not_Found).Replace("_", " ");
                            res.IsSenderNotExists = true;
                            res.ErrorCode = DMTErrorCodes.Sender_Not_Found;
                        }
                    }
                    else
                    {
                        res.Statuscode = ErrorCodes.Minus1;
                        res.Msg = errorCheck.Msg;
                        res.IsSenderNotExists = true;
                        res.ErrorCode = DMTErrorCodes.Sender_Not_Found;
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
                Request = _URL + xmlReq,
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
            EKYCChannelPartnerLogin();
            var res = new MSenderCreateResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError,
                ErrorCode = ErrorCodes.Unknown_Error
            };
            var getsenderDataAPIRes = GetRDDataOfAgentOrSender(request);
            if (getsenderDataAPIRes.Statuscode == ErrorCodes.Minus1)
            {
                res.Msg = getsenderDataAPIRes.Msg;
                return res;
            }
            request.UIDToken = getsenderDataAPIRes.CommonStr;
            var dbres = (new ProcUpdateSender(_dal).Call(new SenderRequest
            {
                MobileNo = request.SenderMobile,
                AadharNo = request.AadharNo,
                UserID = request.UserID,
                UIDToken = request.UIDToken
            })) as SenderInfo;
            request.City = dbres.City;
            request.StateName = dbres.Statename;
            var req = new Ekycsenderupdationmreq
            {
                header = new YesHeader
                {
                    sessionid = YesBankSession
                },
                senderid = Convert.ToInt32(request.ReferenceID),
                bcagent = appSetting.BCAGENT,
                sendername = request.NameOnKYC,
                sendermobilenumber = request.SenderMobile,
                dob = request.DOB,
                pincode = request.Pincode.ToString(),
                city = request.City,
                state = request.StateName,
                channelpartnerrefno = request.TransactionID,
                uidtoken = request.UIDToken
            };
            var _URL = appSetting.BASEURL;
            var xmlReq = XMLHelper.O.SerializeToXml(req, null);
            string response = string.Empty;
            try
            {
                response = AppWebRequest.O.CallUsingHttpWebRequest_POST(_URL, xmlReq, ContentType.application_atom_plus_xml);
                if (!string.IsNullOrEmpty(response))
                {
                    var errorCheck = GetErrorIfExists(response);
                    if (errorCheck.Statuscode == ErrorCodes.One)
                    {
                        var _apiRes = new Ekycsenderupdationmres();
                        _apiRes = XMLHelper.O.DesrializeToObject(_apiRes, response, null);
                        if (_apiRes.status == 1)
                        {
                            res.ReferenceID = _apiRes.remitterid.ToString();
                            res.Statuscode = ErrorCodes.One;
                            res.ErrorCode = ErrorCodes.Transaction_Successful;
                        }
                        else
                        {
                            res.Statuscode = ErrorCodes.Minus1;
                            res.Msg = nameof(DMTErrorCodes.Transaction_Failed).Replace("_", " ");
                            res.ErrorCode = DMTErrorCodes.Transaction_Failed;
                        }
                    }
                    else
                    {
                        res.Statuscode = ErrorCodes.Minus1;
                        res.Msg = errorCheck.Msg;
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
                    FuncName = "SenderEKYC",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = request.UserID
                });
            }
            var dMTReq = new DMTReqRes
            {
                APIID = request.APIID,
                Method = "SenderEKYC",
                RequestModeID = request.RequestMode,
                Request = _URL + xmlReq,
                Response = response,
                SenderNo = request.SenderMobile,
                UserID = request.UserID,
                TID = request.TransactionID
            };
            new ProcUpdateLogDMRReqResp(_dal).Call(dMTReq);
            return res;
        }
        public MSenderCreateResp SenderKYC(MTAPIRequest request)
        {
            throw new NotImplementedException();
        }
        public MSenderCreateResp SenderResendOTP(MTAPIRequest request)
        {
            EKYCChannelPartnerLogin();
            var senderCreateResp = new MSenderCreateResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = nameof(ErrorCodes.Unknown_Error).Replace("_", " "),
                ErrorCode = ErrorCodes.Unknown_Error
            };
            var req = new senderregistrationresendotpreq
            {
                header = new YesHeader
                {
                    sessionid = YesBankSession
                },
                senderid = Convert.ToInt32(request.ReferenceID)
            };
            var _URL = appSetting.BASEURL;
            var xmlReq = XMLHelper.O.SerializeToXml(req, null);
            string response = string.Empty;
            try
            {
                response = AppWebRequest.O.CallUsingHttpWebRequest_POST(_URL, xmlReq, ContentType.application_atom_plus_xml);
                if (!string.IsNullOrEmpty(response))
                {
                    var errorCheck = GetErrorIfExists(response);
                    if (errorCheck.Statuscode == ErrorCodes.One)
                    {
                        var _apiRes = new senderregistrationresendotpres();
                        _apiRes = XMLHelper.O.DesrializeToObject(_apiRes, response, null);
                        if (_apiRes.status == 1)
                        {
                            senderCreateResp.Statuscode = ErrorCodes.One;
                            senderCreateResp.Msg = nameof(DMTErrorCodes.OTP_for_verification_has_been_sent_successfully).Replace("_", " ");
                            senderCreateResp.ErrorCode = DMTErrorCodes.OTP_for_verification_has_been_sent_successfully;
                            senderCreateResp.IsOTPGenerated = true;
                            senderCreateResp.IsOTPResendAvailble = true;
                        }
                        else
                        {
                            senderCreateResp.Statuscode = ErrorCodes.One;
                            senderCreateResp.Msg = nameof(DMTErrorCodes.Sender_Not_Found).Replace("_", " ");
                            senderCreateResp.ErrorCode = DMTErrorCodes.Sender_Not_Found;
                            senderCreateResp.IsOTPGenerated = true;
                        }
                    }
                    else
                    {
                        senderCreateResp.Msg = errorCheck.Msg;
                        senderCreateResp.ErrorCode = ErrorCodes.Unknown_Error;
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
                Request = _URL + xmlReq,
                Response = response,
                SenderNo = request.SenderMobile,
                UserID = request.UserID,
                TID = request.TransactionID
            });
            return senderCreateResp;
        }
        public MSenderLoginResponse VerifySender(MTAPIRequest request)
        {
            EKYCChannelPartnerLogin();
            var res = new MSenderLoginResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = nameof(ErrorCodes.Unknown_Error).Replace("_", ""),
                ErrorCode = ErrorCodes.Unknown_Error
            };
            var req = new senderregistrationvalidatereq
            {
                header = new YesHeader
                {
                    sessionid = YesBankSession
                },
                senderid = Convert.ToInt32(request.ReferenceID),
                mobilenumber = request.SenderMobile,
                verficationcode = Convert.ToInt32(request.OTP),
                channelpartnerrefno = request.TransactionID
            };
            var _URL = appSetting.BASEURL;
            var xmlReq = XMLHelper.O.SerializeToXml(req, null);
            string response = string.Empty;
            try
            {
                response = AppWebRequest.O.CallUsingHttpWebRequest_POST(_URL, xmlReq, ContentType.application_atom_plus_xml);
                if (!string.IsNullOrEmpty(response))
                {
                    var errorCheck = GetErrorIfExists(response);
                    if (errorCheck.Statuscode == ErrorCodes.One)
                    {
                        var _apiRes = new senderregistrationvalidateres();
                        _apiRes = XMLHelper.O.DesrializeToObject(_apiRes, response, null);
                        if (_apiRes.status == 1)
                        {
                            res.Statuscode = ErrorCodes.One;
                            res.Msg = ErrorCodes.DMTSCS;
                            res.ErrorCode = ErrorCodes.Transaction_Successful;
                        }
                        else
                        {
                            res.Msg = nameof(ErrorCodes.Invalid_OTP).Replace("_", " ");
                            res.ErrorCode = ErrorCodes.Invalid_OTP;
                        }
                    }
                    else
                    {
                        res.Msg = errorCheck.Msg;
                        res.ErrorCode = ErrorCodes.Unknown_Error;
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
                Request = _URL + xmlReq,
                Response = response,
                SenderNo = request.SenderMobile,
                UserID = request.UserID,
                TID = request.TransactionID
            });
            return res;
        }
        public MSenderLoginResponse CreateBeneficiary(MTAPIRequest request)
        {
            EKYCChannelPartnerLogin();
            var res = new MSenderLoginResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            var req = new receiverregistrationreq
            {

                header = new YesHeader
                {
                    sessionid = YesBankSession
                },
                bcagent = appSetting.BCAGENT,
                senderid = Convert.ToInt32(request.ReferenceID),
                receivername = request.mBeneDetail.BeneName,
                accountnumber = request.mBeneDetail.AccountNo,
                ifscode = request.mBeneDetail.IFSC,
                receivermobilenumber = request.mBeneDetail.MobileNo,
                receiveremailid = string.IsNullOrEmpty(request.EmailID) ? " " : request.EmailID,
                relationshipid = 0,
                mmid = " ",
                flag = request.mBeneDetail.TransMode == 1 ? 2 : 3,
                channelpartnerrefno = request.TID.ToString()
            };
            var _URL = appSetting.BASEURL;
            var xmlReq = XMLHelper.O.SerializeToXml(req, null);
            xmlReq = xmlReq.Replace("> <", "><");
            string response = string.Empty;
            try
            {
                response = AppWebRequest.O.CallUsingHttpWebRequest_POST(_URL, xmlReq, ContentType.application_atom_plus_xml);
                if (!string.IsNullOrEmpty(response))
                {
                    var errorCheck = GetErrorIfExists(response);
                    if (errorCheck.Statuscode == ErrorCodes.One)
                    {
                        var _apiRes = new receiverregistrationres();
                        _apiRes = XMLHelper.O.DesrializeToObject(_apiRes, response, null);
                        if (_apiRes.status == 1)
                        {
                            res.Statuscode = ErrorCodes.One;
                            res.Msg = ErrorCodes.BENESCS;
                            res.ErrorCode = ErrorCodes.Transaction_Successful;
                            res.BeneID = _apiRes.beneficiaryid.ToString();
                            res.ReferenceID = _apiRes.senderid.ToString();
                            return res;
                        }
                        else
                        {
                            res.Msg = nameof(DMTErrorCodes.Transaction_Failed).Replace("_", " ");
                        }
                    }
                    else
                    {
                        res.Msg = errorCheck.Msg;
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
                Request = _URL + "|" + xmlReq,
                Response = response,
                SenderNo = request.SenderMobile,
                UserID = request.UserID,
                TID = request.TransactionID
            });
            return res;
        }
        public MBeneficiaryResp GetBeneficiary(MTAPIRequest request)
        {
            EKYCChannelPartnerLogin();
            var res = new MBeneficiaryResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = nameof(ErrorCodes.Unknown_Error).Replace("_", " "),
                ErrorCode = ErrorCodes.Unknown_Error
            };
            var req = new senderdetailsreq
            {
                header = new YesHeader
                {
                    sessionid = YesBankSession
                },
                bcagent = appSetting.BCAGENT,
                mobilenumber = request.SenderMobile,
                flag = 1
            };
            var _URL = appSetting.BASEURL;
            var xmlReq = XMLHelper.O.SerializeToXml(req, null);
            string response = string.Empty;
            try
            {
                response = AppWebRequest.O.CallUsingHttpWebRequest_POST(_URL, xmlReq, ContentType.application_atom_plus_xml);
                if (!string.IsNullOrEmpty(response))
                {
                    var errorCheck = GetErrorIfExists(response);
                    if (errorCheck.Statuscode == ErrorCodes.One)
                    {
                        response = response.Replace("<mmid />", "").Replace("<bankbenename />", "").Replace("<receiveremailid />", "");
                        var _apiRes = new senderdetailsres();
                        _apiRes = XMLHelper.O.DesrializeToObject(_apiRes, response, null);
                        if (_apiRes.status == 1)
                        {
                            var Beneficiaries = new List<MBeneDetail>();
                            foreach (var item in _apiRes.receiverdetail)
                            {
                                if (item.receiverstatus.In(0, 1))
                                {
                                    Beneficiaries.Add(new MBeneDetail
                                    {
                                        AccountNo = item.accountnumber,
                                        BankName = item.bank,
                                        IFSC = item.ifscode,
                                        BeneName = item.receivername,
                                        MobileNo = item.receivermobilenumber,
                                        BeneID = item.receiverid.ToString(),
                                        IsVerified = item.receiverstatus == 1,
                                        IMPSStatus = item.impsstatus == 1,
                                        NEFTStatus = item.neftflag == 1
                                    });

                                }
                            }
                            res.Beneficiaries = Beneficiaries;
                            res.ErrorCode = ErrorCodes.Transaction_Successful;
                        }
                        else
                        {
                            res.Msg = nameof(DMTErrorCodes.Transaction_Failed);
                            res.ErrorCode = DMTErrorCodes.Transaction_Failed;
                        }
                    }
                    else
                    {
                        res.Msg = errorCheck.Msg;
                        res.ErrorCode = ErrorCodes.Unknown_Error;
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
                Request = _URL + "|" + xmlReq,
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
            EKYCChannelPartnerLogin();
            var senderCreateResp = new MSenderCreateResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = nameof(ErrorCodes.Unknown_Error).Replace("_", " "),
                ErrorCode = ErrorCodes.Unknown_Error
            };
            var req = new receiverresendotpreq
            {
                header = new YesHeader
                {
                    sessionid = YesBankSession
                },
                senderid = Convert.ToInt32(request.ReferenceID),
                receiverid = Convert.ToInt32(request.mBeneDetail.BeneID)
            };
            var _URL = appSetting.BASEURL;
            var xmlReq = XMLHelper.O.SerializeToXml(req, null);
            string response = string.Empty;
            try
            {
                response = AppWebRequest.O.CallUsingHttpWebRequest_POST(_URL, xmlReq, ContentType.application_atom_plus_xml);
                if (!string.IsNullOrEmpty(response))
                {
                    var errorCheck = GetErrorIfExists(response);
                    if (errorCheck.Statuscode == ErrorCodes.One)
                    {
                        var _apiRes = new receiverresendotpres();
                        _apiRes = XMLHelper.O.DesrializeToObject(_apiRes, response, null);
                        if (_apiRes.status == 1)
                        {
                            senderCreateResp.Statuscode = ErrorCodes.One;
                            senderCreateResp.Msg = nameof(DMTErrorCodes.OTP_for_verification_has_been_sent_successfully).Replace("_", " ");
                            senderCreateResp.ErrorCode = DMTErrorCodes.OTP_for_verification_has_been_sent_successfully;
                            senderCreateResp.IsOTPGenerated = true;
                            senderCreateResp.IsOTPResendAvailble = true;
                        }
                        else
                        {
                            senderCreateResp.Msg = nameof(DMTErrorCodes.Sender_Not_Found).Replace("_", " ");
                            senderCreateResp.ErrorCode = DMTErrorCodes.Sender_Not_Found;
                        }
                    }
                    else
                    {
                        senderCreateResp.Msg = errorCheck.Msg;
                        senderCreateResp.ErrorCode = DMTErrorCodes.Sender_Not_Found;
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
                Request = _URL + "|" + xmlReq,
                Response = response,
                SenderNo = request.SenderMobile,
                UserID = request.UserID,
                TID = request.TransactionID
            });
            return senderCreateResp;
        }
        public MSenderLoginResponse ValidateBeneficiary(MTAPIRequest request)
        {
            EKYCChannelPartnerLogin();
            var mSenderLoginResponse = new MSenderLoginResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = nameof(ErrorCodes.Unknown_Error).Replace("_", " "),
                ErrorCode = ErrorCodes.Unknown_Error
            };
            var req = new receiverregistrationvalidatereq
            {
                header = new YesHeader
                {
                    sessionid = YesBankSession
                },
                receiverid = Convert.ToInt32(request.mBeneDetail.BeneID),
                verficationcode = Convert.ToInt32(request.OTP)
            };
            var _URL = appSetting.BASEURL;
            var xmlReq = XMLHelper.O.SerializeToXml(req, null);
            string response = string.Empty;
            try
            {
                response = AppWebRequest.O.CallUsingHttpWebRequest_POST(_URL, xmlReq, ContentType.application_atom_plus_xml);
                if (!string.IsNullOrEmpty(response))
                {
                    var _apiRes = new receiverregistrationvalidateres();
                    _apiRes = XMLHelper.O.DesrializeToObject(_apiRes, response, null);
                    if (Validate.O.IsNumeric(_apiRes.status))
                    {
                        if (_apiRes.status == "1")
                        {
                            mSenderLoginResponse.Statuscode = ErrorCodes.One;
                            mSenderLoginResponse.Msg = nameof(DMTErrorCodes.Beneficiary_Added_Successfully).Replace("_", " ");
                            mSenderLoginResponse.ErrorCode = DMTErrorCodes.Beneficiary_Added_Successfully;
                        }
                        else
                        {
                            mSenderLoginResponse.Msg = nameof(ErrorCodes.Invalid_OTP).Replace("_", " ");
                            mSenderLoginResponse.ErrorCode = ErrorCodes.Invalid_OTP;
                        }
                    }
                    else
                    {
                        mSenderLoginResponse.Msg = _apiRes.status;
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
                Request = _URL + "|" + xmlReq,
                Response = response,
                SenderNo = request.SenderMobile,
                UserID = request.UserID,
                TID = request.TransactionID
            });
            return mSenderLoginResponse;
        }
        public MSenderLoginResponse RemoveBeneficiary(MTAPIRequest request)
        {
            EKYCChannelPartnerLogin();
            var senderLoginResponse = new MSenderLoginResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError,
                ErrorCode = ErrorCodes.Unknown_Error
            };
            var req = new receiverdeletereq
            {
                header = new YesHeader
                {
                    sessionid = YesBankSession
                },
                senderid = Convert.ToInt32(request.ReferenceID),
                receiverid = Convert.ToInt32(request.mBeneDetail.BeneID)
            };
            var _URL = appSetting.BASEURL;
            var xmlReq = XMLHelper.O.SerializeToXml(req, null);
            string response = string.Empty;
            try
            {
                response = AppWebRequest.O.CallUsingHttpWebRequest_POST(_URL, xmlReq, ContentType.application_atom_plus_xml);
                if (!string.IsNullOrEmpty(response))
                {
                    var errorCheck = GetErrorIfExists(response);
                    if (errorCheck.Statuscode == ErrorCodes.One)
                    {
                        var _apiRes = new receiverdeleteres();
                        _apiRes = XMLHelper.O.DesrializeToObject(_apiRes, response, null);
                        if (_apiRes.status == 1)
                        {
                            senderLoginResponse.Statuscode = ErrorCodes.One;
                            senderLoginResponse.Msg = nameof(DMTErrorCodes.OTP_for_verification_has_been_sent_successfully).Replace("_", " ");
                            senderLoginResponse.ErrorCode = DMTErrorCodes.OTP_for_verification_has_been_sent_successfully;
                            senderLoginResponse.IsOTPGenerated = true;
                        }
                        else
                        {
                            senderLoginResponse.Msg = nameof(DMTErrorCodes.Beneficiary_not_found).Replace("_", " ");
                            senderLoginResponse.ErrorCode = DMTErrorCodes.Beneficiary_not_found;
                        }
                    }
                    else
                    {
                        senderLoginResponse.Msg = errorCheck.Msg;
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
                Request = _URL + "|" + xmlReq,
                Response = response,
                SenderNo = request.SenderMobile,
                UserID = request.UserID,
                TID = request.TransactionID
            });
            return senderLoginResponse;
        }
        public MSenderLoginResponse RemoveBeneficiaryValidate(MTAPIRequest request)
        {
            EKYCChannelPartnerLogin();
            var mSenderLoginResponse = new MSenderLoginResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = nameof(ErrorCodes.Unknown_Error).Replace("_", " "),
                ErrorCode = ErrorCodes.Unknown_Error
            };
            var req = new receiverdeletevalidationreq
            {
                header = new YesHeader
                {
                    sessionid = YesBankSession
                },
                receiverid = Convert.ToInt32(request.mBeneDetail.BeneID),
                verficationcode = Convert.ToInt32(request.OTP)
            };
            var _URL = appSetting.BASEURL;
            var xmlReq = XMLHelper.O.SerializeToXml(req, null);
            string response = string.Empty;
            try
            {
                response = AppWebRequest.O.CallUsingHttpWebRequest_POST(_URL, xmlReq, ContentType.application_atom_plus_xml);
                if (!string.IsNullOrEmpty(response))
                {
                    var errorCheck = GetErrorIfExists(response);
                    if (errorCheck.Statuscode == ErrorCodes.One)
                    {
                        var _apiRes = new receiverregistrationvalidateres();
                        _apiRes = XMLHelper.O.DesrializeToObject(_apiRes, response, "receiverdeletevalidationres", true);
                        if (Validate.O.IsNumeric(_apiRes.status))
                        {
                            if (_apiRes.status == "1")
                            {
                                mSenderLoginResponse.Statuscode = ErrorCodes.One;
                                mSenderLoginResponse.Msg = nameof(DMTErrorCodes.Beneficiary_Deactivated_or_Deleted).Replace("_", " ");
                                mSenderLoginResponse.ErrorCode = DMTErrorCodes.Beneficiary_Deactivated_or_Deleted;
                            }
                            else
                            {
                                mSenderLoginResponse.Msg = nameof(ErrorCodes.Invalid_OTP).Replace("_", " ");
                                mSenderLoginResponse.ErrorCode = ErrorCodes.Invalid_OTP;
                                mSenderLoginResponse.IsOTPGenerated = true;
                            }
                        }
                        else
                        {
                            mSenderLoginResponse.Msg = _apiRes.status;
                        }
                    }
                    else
                    {
                        mSenderLoginResponse.Msg = errorCheck.Msg;
                        mSenderLoginResponse.Msg = nameof(ErrorCodes.Invalid_OTP).Replace("_", " ");
                        mSenderLoginResponse.ErrorCode = ErrorCodes.Invalid_OTP;
                        mSenderLoginResponse.IsOTPGenerated = true;
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
                Request = _URL + "|" + xmlReq,
                Response = response,
                SenderNo = request.SenderMobile,
                UserID = request.UserID,
                TID = request.TransactionID
            });
            return mSenderLoginResponse;
        }
        public DMRTransactionResponse VerifyAccount(MTAPIRequest request)
        {
            EKYCChannelPartnerLogin();
            var res = new DMRTransactionResponse
            {
                Statuscode = RechargeRespType.PENDING,
                Msg = RechargeRespType._PENDING,
                VendorID = "",
                LiveID = ""
            };
            var req = new beneficiaryaccountvericifationreq
            {
                header = new YesHeader
                {
                    sessionid = YesBankSession
                },
                senderid = Convert.ToInt32(request.ReferenceID),
                accountnumber = request.mBeneDetail.AccountNo,
                bcagent = appSetting.BCAGENT,
                ifscode = request.mBeneDetail.IFSC,
                channelpartnerrefno = TIDPrefix+request.TID
            };
            var _URL = appSetting.BASEURL;
            var xmlReq = XMLHelper.O.SerializeToXml(req, null);
            string response = string.Empty;
            try
            {
                response = AppWebRequest.O.CallUsingHttpWebRequest_POST(_URL, xmlReq, ContentType.application_atom_plus_xml);
                if (!string.IsNullOrEmpty(response))
                {
                    var errorCheck = GetErrorIfExists(response);
                    if (errorCheck.Statuscode == ErrorCodes.One)
                    {
                        var _apiRes = new beneficiaryaccountvericifationres();
                        _apiRes = XMLHelper.O.DesrializeToObject(_apiRes, response, null);
                        _apiRes.status = _apiRes.status == null ? 1 : _apiRes.status;
                        if (_apiRes.status == 0)
                        {
                            res.Statuscode = RechargeRespType.FAILED;
                            res.Msg = nameof(DMTErrorCodes.Transaction_Failed).Replace("_", " ");
                            res.LiveID = _apiRes.yesbanktransactionid;
                            res.VendorID = _apiRes.yesbanktransactionid;
                            res.ErrorCode = DMTErrorCodes.Transaction_Failed;
                        }
                        else
                        {
                            var tempres = LoopStatusCheck(request.TID.ToString(), request.RequestMode, request.UserID, request.APIID);
                            res.Statuscode = tempres.Statuscode;
                            res.Msg = tempres.Msg;
                            res.ErrorCode = tempres.ErrorCode;
                            res.LiveID = tempres.LiveID;
                            res.VendorID = _apiRes.yesbanktransactionid;
                            res.BeneName = _apiRes.benename ?? "";
                        }
                    }
                    else
                    {
                        res.Statuscode = RechargeRespType.FAILED;
                        res.Msg = nameof(DMTErrorCodes.Transaction_Failed).Replace("_", " ");
                        res.LiveID = errorCheck.Msg;
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
                    FuncName = "VerifyAccount",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = request.UserID
                });
            }
            res.Request = _URL + "|" + xmlReq;
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
            EKYCChannelPartnerLogin();
            var res = new DMRTransactionResponse
            {
                Statuscode = RechargeRespType.PENDING,
                Msg = RechargeRespType._PENDING,
                VendorID = "",
                LiveID = ""
            };
            var req = new transactionreq
            {
                header = new YesHeader
                {
                    sessionid = YesBankSession
                },
                senderid = Convert.ToInt32(request.ReferenceID),
                receiverid = Convert.ToInt32(request.mBeneDetail.BeneID),
                bcagent = appSetting.BCAGENT,
                amount = request.Amount,
                cpid = appSetting.CPID,
                channelpartnerrefno = TIDPrefix + request.TID,
                remarks = "PAYMENTFOR" + request.TID,
                flag = request.TransMode == "IMPS" ? 2 : 3
            };
            var _URL = appSetting.BASEURL;
            var xmlReq = XMLHelper.O.SerializeToXml(req, null);
            string response = string.Empty;
            try
            {
                response = AppWebRequest.O.CallUsingHttpWebRequest_POST(_URL, xmlReq, ContentType.application_atom_plus_xml);
                if (!string.IsNullOrEmpty(response))
                {
                    var errorCheck = GetErrorIfExists(response);
                    if (errorCheck.Statuscode == ErrorCodes.One)
                    {
                        var _apiRes = new transactionres();
                        _apiRes = XMLHelper.O.DesrializeToObject(_apiRes, response, null);
                        _apiRes.status = _apiRes.status == null ? 1 : _apiRes.status;
                        if (_apiRes.status == 0)
                        {
                            res.Statuscode = RechargeRespType.FAILED;
                            res.Msg = nameof(DMTErrorCodes.Transaction_Failed).Replace("_", " ");
                            res.ErrorCode = DMTErrorCodes.Transaction_Failed;
                            res.LiveID = _apiRes.remarks;
                            res.VendorID = _apiRes.YEStransactionid;
                        }
                        else
                        {
                            var tempres = LoopStatusCheck(request.TID.ToString(), request.RequestMode, request.UserID, request.APIID);
                            res.Statuscode = tempres.Statuscode;
                            res.Msg = tempres.Msg;
                            res.ErrorCode = tempres.ErrorCode;
                            res.LiveID = string.IsNullOrEmpty(tempres.LiveID) ? _apiRes.bankrefno.ToString() : tempres.LiveID;
                            res.VendorID = _apiRes.YEStransactionid;
                        }
                    }
                    else
                    {
                        res.Statuscode = RechargeRespType.FAILED;
                        res.Msg = errorCheck.Msg;
                        res.ErrorCode = ErrorCodes.Unknown_Error;
                        res.LiveID = errorCheck.Msg;
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
            res.Request = _URL + "|" + xmlReq;
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
        private DMRTransactionResponse LoopStatusCheck(string TransactionID, int RequestMode, int UserID, int APIID)
        {
            var res = new DMRTransactionResponse
            {
                Statuscode = RechargeRespType.PENDING,
                Msg = nameof(ErrorCodes.Request_Accpeted).Replace("_", " "),
                ErrorCode = ErrorCodes.Request_Accpeted
            };
            int i = 0;
            while (i < 10)
            {
                i++;
                if (res.Statuscode == RechargeRespType.PENDING)
                {
                    res = GetTransactionStatus(TransactionID, RequestMode, UserID, APIID);
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
        public DMRTransactionResponse GetTransactionStatus(string TID, int RequestMode, int UserID, int APIID)
        {
            EKYCChannelPartnerLogin();
            var res = new DMRTransactionResponse
            {
                Statuscode = RechargeRespType.PENDING,
                Msg = nameof(RechargeRespType.PENDING),
                ErrorCode = ErrorCodes.Request_Accpeted
            };
            var req = new transactionrequeryreq
            {
                header = new YesHeader
                {
                    sessionid = YesBankSession
                },
                bcagent = appSetting.BCAGENT,
                channelpartnerrefno = TIDPrefix + TID
            };
            var _URL = appSetting.BASEURL;
            var xmlReq = XMLHelper.O.SerializeToXml(req, null);
            string response = string.Empty;
            try
            {
                response = AppWebRequest.O.CallUsingHttpWebRequest_POST(_URL, xmlReq, ContentType.application_atom_plus_xml);

                if (!string.IsNullOrEmpty(response))
                {
                    var errorCheck = GetErrorIfExists(response);
                    if (errorCheck.Statuscode == ErrorCodes.One)
                    {
                        var _apiRes = new transactionrequeryres();
                        _apiRes = XMLHelper.O.DesrializeToObject(_apiRes, response, null);
                        if (_apiRes != null)
                        {
                            res.VendorID = _apiRes.yesbanktransactionid;
                            res.LiveID = _apiRes.yesbanktransactionid;

                            if (_apiRes.status == YesBankTransactionStatus.Credited)
                            {
                                res.Statuscode = RechargeRespType.SUCCESS;
                                res.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                                res.ErrorCode = ErrorCodes.Transaction_Successful;
                            }
                            else if (_apiRes.status == YesBankTransactionStatus.Failure)
                            {
                                res.Statuscode = RechargeRespType.FAILED;
                                res.Msg = _apiRes.Bankremarks;
                                res.ErrorCode = DMTErrorCodes.Transaction_Failed;
                                res.LiveID = _apiRes.Bankremarks;
                            }
                            else
                            {
                                res.Statuscode = RechargeRespType.PENDING;
                                res.Msg = nameof(ErrorCodes.Request_Accpeted).Replace("_", " ");
                                res.ErrorCode = ErrorCodes.Request_Accpeted;
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
            new ProcUpdateLogDMRReqResp(_dal).Call(new DMTReqRes
            {
                APIID = APIID,
                Method = "GetTransactionStatus",
                RequestModeID = RequestMode,
                Request = _URL + xmlReq,
                Response = response,
                SenderNo = string.Empty,
                UserID = UserID,
                TID = TID.ToString()
            });
            res.Request = _URL + xmlReq;
            res.Response = response;
            return res;
        }
        private ResponseStatus GetErrorIfExists(string response)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.One,
                Msg = "No Error"
            };
            if ((response ?? string.Empty).Contains("errorres>"))
            {
                var _apiRes = new errorres();
                _apiRes = XMLHelper.O.DesrializeToObject(_apiRes, response, "", true);
                if (_apiRes != null)
                {
                    res.Statuscode = ErrorCodes.Minus1;
                    res.Msg = _apiRes.description;
                }
            }
            return res;
        }
        //private IResponseStatus GetRDDataOfSender(MTAPIRequest request)
        //{
        //    EKYCChannelPartnerLogin();
        //    var res = new ResponseStatus
        //    {
        //        Statuscode = ErrorCodes.Minus1,
        //        Msg = ErrorCodes.AnError
        //    };
        //    var DeviceDataXml = new EkycsenderregistrationrmforRDres
        //    {
        //        AadhaarNo = request.AadharNo,
        //        DeviceDataXml = request.PidData,
        //        sessionid = YesBankSession,
        //        Wadh = YesBankWadh
        //    };
        //    var strDeviceDataXml = XMLHelper.O.SerializeToXml(DeviceDataXml, null);
        //    //var Encryptstring = HashEncryption.O.Encrypt(strDeviceDataXml, appSetting.P, appSetting.S);
        //    var Encryptstring = AesBase64Wrapper.EncryptAndEncode(strDeviceDataXml);
        //    var req = new EkycsenderregistrationrmforRDreq
        //    {
        //        Encryptstring = Encryptstring
        //    };
        //    var _URL = appSetting.BASEURL;
        //    var xmlReq = XMLHelper.O.SerializeToXml(req, null);
        //    string response = string.Empty;
        //    try
        //    {
        //        response = AppWebRequest.O.CallUsingHttpWebRequest_POST(_URL, xmlReq, ContentType.application_atom_plus_xml);
        //        if (!string.IsNullOrEmpty(response))
        //        {
        //            res = GetErrorIfExists(response);
        //            if (res.Statuscode == ErrorCodes.One)
        //            {
        //                var _apiRes = new senderekycregistrationres();
        //                _apiRes = XMLHelper.O.DesrializeToObject(_apiRes, response, null);
        //                if (_apiRes.status == 1)
        //                {
        //                    res.Statuscode = ErrorCodes.One;
        //                    res.Msg = ErrorCodes.DMTSCS;
        //                    res.ErrorCode = ErrorCodes.Transaction_Successful;
        //                    res.CommonStr = _apiRes.uidtoken;
        //                }
        //                else
        //                {
        //                    res.Statuscode = ErrorCodes.Minus1;
        //                    res.Msg = "Finger data match failure from bank";
        //                    res.ErrorCode = DMTErrorCodes.Transaction_Failed;
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        response = ex.Message + "|" + response;
        //        var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
        //        {
        //            ClassName = GetType().Name,
        //            FuncName = "GetRDDataOfSender",
        //            Error = ex.Message,
        //            LoginTypeID = LoginType.ApplicationUser,
        //            UserId = request.UserID
        //        });
        //    }
        //    new ProcUpdateLogDMRReqResp(_dal).Call(new DMTReqRes
        //    {
        //        APIID = request.APIID,
        //        Method = "GetRDDataOfSender",
        //        RequestModeID = request.RequestMode,
        //        Request = _URL,
        //        Response = response,
        //        SenderNo = request.SenderMobile,
        //        UserID = request.UserID,
        //        TID = request.TransactionID
        //    });
        //    return res;
        //}
        public ResponseStatus GetRDDataOfAgentOrSender(MTAPIRequest request)
        {
            EKYCChannelPartnerLogin();
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            var DeviceDataXml = new Ekycagentregistrationrmreqwithout_en
            {
                AadhaarNo = request.AadharNo,                
                AgentID= string.IsNullOrEmpty(request.APIOutletID)?appSetting.BCAGENT: request.APIOutletID,
                DeviceCertExpiryDate=" ",
                DeviceHmacXml=" ",
                DeviceDataXml = "PIDDATA",
                DeviceSerialNumber = " ",
                DeviceSessionKey = " ",
                DeviceType = " ",
                DeviceVersionNumber=" ",
                AadhaarNumberType = 0,
                sessionid = YesBankSession,
                Wadh = YesBankWadh
            };
            var strDeviceDataXml = XMLHelper.O.SerializeToXml(DeviceDataXml, "EkycAgentregistrationrmforRDres");
            strDeviceDataXml = strDeviceDataXml.Replace("PIDDATA", "<![CDATA["+request.PidData.Replace(@"<?xml version=""1.0""?>","") + "]]>").Replace("> <","><");
            //var Encryptstring = HashEncryption.O.Encrypt(strDeviceDataXml, appSetting.P, appSetting.S);
            var Encryptstring = AesBase64Wrapper.EncryptAndEncode(strDeviceDataXml);
            var req = new EkycsenderregistrationrmforRDreq
            {
                Encryptstring = Encryptstring
            };
            var _URL = appSetting.BASEURL;
            var xmlReq = XMLHelper.O.SerializeToXml(req, "EkycAgentregistrationrmforRDreq");
            string response = string.Empty;
            try
            {
                response = AppWebRequest.O.CallUsingHttpWebRequest_POST(_URL, xmlReq, ContentType.application_atom_plus_xml);
                if (!string.IsNullOrEmpty(response))
                {
                    res = GetErrorIfExists(response);
                    if (res.Statuscode == ErrorCodes.One)
                    {
                        var _apiRes = new senderekycregistrationres();
                        _apiRes = XMLHelper.O.DesrializeToObject(_apiRes, response, "agentekycregistrationres", true);

                        if (_apiRes.status == 1)
                        {
                            res.Statuscode = ErrorCodes.One;
                            res.Msg = ErrorCodes.DMTSCS;
                            res.ErrorCode = ErrorCodes.Transaction_Successful;
                            res.CommonStr = _apiRes.uidtoken;
                        }
                        else
                        {
                            res.Statuscode = ErrorCodes.Minus1;
                            res.Msg = "Finger data match failure from bank";
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
                    FuncName = "GetRDDataOfAgent",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = request.UserID
                });
            }
            new ProcUpdateLogDMRReqResp(_dal).Call(new DMTReqRes
            {
                APIID = request.APIID,
                Method = "GetRDDataOfAgent",
                RequestModeID = request.RequestMode,
                Request = _URL,
                Response = response,
                SenderNo = request.SenderMobile,
                UserID = request.UserID,
                TID = request.TransactionID
            });
            return res;
        }
        public OutletAPIStatusUpdate AgentEKYC(bcagentregistrationforekycreq req, int APIID)
        {
            EKYCChannelPartnerLogin();
            var OutReqRes = new OutletAPIStatusUpdate
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            req.header = new YesHeader
            {
                sessionid = YesBankSession
            };
            var _URL = appSetting.BASEURL;
            var xmlReq = XMLHelper.O.SerializeToXml(req, null);
            xmlReq = xmlReq.Replace("> <", "><");
            string response = string.Empty;
            try
            {
                response = AppWebRequest.O.CallUsingHttpWebRequest_POST(_URL, xmlReq, ContentType.application_atom_plus_xml);
                if (!string.IsNullOrEmpty(response))
                {
                    var errorCheck = GetErrorIfExists(response);
                    if (errorCheck.Statuscode == ErrorCodes.One)
                    {
                        var _apiRes = new bcagentregistrationforekycres();
                        _apiRes = XMLHelper.O.DesrializeToObject(_apiRes, response, null);
                        if (_apiRes.status == 1)
                        {
                            OutReqRes.Statuscode = ErrorCodes.One;
                            OutReqRes.Msg = ErrorCodes.OutletRegistered;
                            OutReqRes.APIOutletID = _apiRes.bcagentid;
                            OutReqRes.APIOutletStatus = RndKYCOutLetStatus._Approved;
                            OutReqRes.KYCStatus = RndKYCOutLetStatus._Approved;
                            OutReqRes.DMTID = _apiRes.bcagentid;
                            OutReqRes.DMTStatus = RndKYCOutLetStatus._Approved;
                        }
                        else
                        {
                            OutReqRes.Statuscode = ErrorCodes.Minus1;
                            OutReqRes.Msg = _apiRes.description;
                        }
                    }
                    else
                    {
                        OutReqRes.Statuscode = ErrorCodes.Minus1;
                        OutReqRes.Msg = errorCheck.Msg;
                    }
                }
            }
            catch (Exception ex)
            {
                response = ex.Message + "|" + response;
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "AgentEKYC",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = Convert.ToInt32(req.bcagentid)
                });
            }
            #region APILogOutletRegistration
            var OnboradReq = new OnboardingLog
            {
                APIID = APIID,
                Method = "AgentEKYC",
                Request = _URL + "|" + xmlReq,
                Response = response
            };
            new ProcUpdateLogOnboardingReqResp(_dal).Call(OnboradReq);
            #endregion
            return OutReqRes;
        }
    }
}
