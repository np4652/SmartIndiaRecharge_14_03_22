using Fintech.AppCode.Configuration;
using Fintech.AppCode.DB;
using Fintech.AppCode.HelperClass;
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
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using RoundpayFinTech.AppCode.ThirdParty.RBL;
using Newtonsoft.Json;

namespace RoundpayFinTech.AppCode.MiddleLayer.Dmt_Api
{
    public class RBLML : IMoneyTransferAPIML
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        private readonly IConfiguration Configuration;
        private readonly IConnectionConfiguration _c;
        private readonly RBLAppSetting appSetting;
        private readonly IDAL _dal;
        private static string RBLSession;
        private static DateTime RBLExpiry;
        private static string TIDPrefix = "RTID";
        private const string RBLPFXFile = "Image/RBL/alankitwild.pfx";
        public RBLML(IHttpContextAccessor accessor, IHostingEnvironment env, IDAL dal)
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
        private RBLAppSetting AppSetting()
        {
            var setting = new RBLAppSetting();
            try
            {
                setting = new RBLAppSetting
                {
                    LDAP = Configuration["DMR:RBLMT:LDAP"],
                    LDAPPASSWORD = Configuration["DMR:RBLMT:LDAPPASSWORD"],
                    CLIENTID = Configuration["DMR:RBLMT:CLIENTID"],
                    CLIENTSECRET = Configuration["DMR:RBLMT:CLIENTSECRET"],
                    USERNAME = Configuration["DMR:RBLMT:USERNAME"],
                    PASSWORD = Configuration["DMR:RBLMT:PASSWORD"],
                    BCAGENT = Configuration["DMR:RBLMT:BCAGENT"],
                    BASEURL = Configuration["DMR:RBLMT:BASEURL"],
                    PFXPD = Configuration["DMR:RBLMT:PFXPD"],
                    CPID = Configuration["DMR:RBLMT:CPID"]
                };
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "RBLAppSetting",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser
                });
            }
            return setting;
        }

        public bool ChannelPartnerLogin()
        {
            bool IsCallAPI = false;
            if (string.IsNullOrEmpty(RBLSession))
            {
                IsCallAPI = true;
            }
            else if (RBLExpiry == null)
            {
                IsCallAPI = true;
            }
            else if (RBLExpiry < DateTime.Now)
            {
                IsCallAPI = true;
            }
            if (IsCallAPI)
            {
                var res = new channelpartnerloginres();
                var request = new channelpartnerloginreq
                {
                    bcagent = appSetting.BCAGENT,
                    username = appSetting.USERNAME,
                    password = appSetting.PASSWORD
                };
                var _URL = appSetting.BASEURL + string.Format("?client_id={0}&client_secret={1}", appSetting.CLIENTID, appSetting.CLIENTSECRET);
                string authString = string.Format("{0}:{1}", appSetting.LDAP, appSetting.LDAPPASSWORD);
                var headers = new Dictionary<string, string>
                {
                   { "Authorization", "Basic "+ Convert.ToBase64String(Encoding.UTF8.GetBytes(authString)) }
                };
                try
                {
                    string KeyPath = Path.Combine(Directory.GetCurrentDirectory(), RBLPFXFile);
                    var xmlReq = XMLHelper.O.SerializeToXml(request, null);
                    var _apiResp = AppWebRequest.O.PostJsonDataUsingHWRTLSWithCertificate(_URL, xmlReq, headers, KeyPath, appSetting.PFXPD).Result;
                    if (!string.IsNullOrEmpty(_apiResp))
                    {
                        res = XMLHelper.O.DesrializeToObject(res, _apiResp, "");
                        if (res.status == 1)
                        {
                            RBLSession = res.sessiontoken;
                            RBLExpiry = string.IsNullOrEmpty(RBLSession) ? DateTime.Now.AddMinutes(-10) : DateTime.Now.AddMinutes(50);
                        }
                    }
                }
                catch (Exception ex)
                {
                    var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                    {
                        ClassName = GetType().Name,
                        FuncName = "ChannelPartnerLogin",
                        Error = ex.Message,
                        LoginTypeID = LoginType.ApplicationUser,
                        UserId = 1
                    });
                }
            }
            if (string.IsNullOrEmpty(RBLSession))
            {
                return false;
            }
            else if (RBLExpiry == null)
            {
                return false;
            }
            else if (RBLExpiry > DateTime.Now)
            {
                return true;
            }
            return false;
        }

        public MSenderCreateResp CreateSender(MTAPIRequest request)
        {

            var res = new MSenderCreateResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError,
                ErrorCode = ErrorCodes.Unknown_Error
            };
            if (!ChannelPartnerLogin())
            {
                res.Msg = "Token expired";
                return res;
            }
            var req = new remitterregistrationrmreq
            {
                header = new RBLHeader
                {
                    sessiontoken = RBLSession
                },
                bcagent = appSetting.BCAGENT,
                remittername = request.FirstName + " " + request.LastName,
                remittermobilenumber = request.SenderMobile,
                pincode = request.Pincode,
                cityname = request.City,
                statename = request.StateName,
                remitteraddress1 = request.Address,
                remitteraddress2 = request.Address,
                lremitteraddress = request.Address,
                lpincode = request.Pincode,
                alternatenumber = request.SenderMobile,
                idproof = " ",
                idproofnumber = " ",
                idproofissuedate = " ",
                idproofexpirydate = " ",
                idproofissueplace = " ",
                lcityname = request.City,
                lstatename = request.StateName
            };
            var xmlReq = XMLHelper.O.SerializeToXml(req, null);
            xmlReq = xmlReq.Replace("> <", "><");
            string response = string.Empty;
            var _URL = appSetting.BASEURL + string.Format("?client_id={0}&client_secret={1}", appSetting.CLIENTID, appSetting.CLIENTSECRET);
            string authString = string.Format("{0}:{1}", appSetting.LDAP, appSetting.LDAPPASSWORD);
            var headers = new Dictionary<string, string>
            {
                { "Authorization", "Basic "+ Convert.ToBase64String(Encoding.UTF8.GetBytes(authString)) }
            };
            try
            {
                string KeyPath = Path.Combine(Directory.GetCurrentDirectory(), RBLPFXFile);
                response = AppWebRequest.O.PostJsonDataUsingHWRTLSWithCertificate(_URL, xmlReq, headers, KeyPath, appSetting.PFXPD).Result;
                if (!string.IsNullOrEmpty(response))
                {
                    var resErr = GetErrorIfExists(response);
                    if (resErr.Statuscode == ErrorCodes.One)
                    {
                        var _apiRes = new remitterregistrationrmres();
                        _apiRes = XMLHelper.O.DesrializeToObject(_apiRes, response, null);

                        if ((_apiRes.status ?? 0) == 1)
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
                            res.Msg = nameof(DMTErrorCodes.Partial_Registration_Please_verify_sender_with_OTP).Replace("_", " ");
                            res.ErrorCode = DMTErrorCodes.Partial_Registration_Please_verify_sender_with_OTP;
                            res.IsOTPGenerated = true;
                            res.IsOTPResendAvailble = true;
                            res.ReferenceID = _apiRes.remitterid.ToString();
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
            var res = new MSenderLoginResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = nameof(ErrorCodes.Unknown_Error).Replace("_", " "),
                ErrorCode = ErrorCodes.Unknown_Error
            };
            if (!ChannelPartnerLogin())
            {
                res.Msg = "Token expired";
                return res;
            }
            var req = new remitterdetailsreq
            {
                header = new RBLHeader
                {
                    sessiontoken = RBLSession
                },
                bcagent = appSetting.BCAGENT,
                mobilenumber = request.SenderMobile,
                flag = 1
            };
            var xmlReq = XMLHelper.O.SerializeToXml(req, null);
            string response = string.Empty;
            var _URL = appSetting.BASEURL + string.Format("?client_id={0}&client_secret={1}", appSetting.CLIENTID, appSetting.CLIENTSECRET);
            string authString = string.Format("{0}:{1}", appSetting.LDAP, appSetting.LDAPPASSWORD);
            var headers = new Dictionary<string, string>
            {
                { "Authorization", "Basic "+ Convert.ToBase64String(Encoding.UTF8.GetBytes(authString)) }
            };
            try
            {
                string KeyPath = Path.Combine(Directory.GetCurrentDirectory(), RBLPFXFile);
                response = AppWebRequest.O.PostJsonDataUsingHWRTLSWithCertificate(_URL, xmlReq, headers, KeyPath, appSetting.PFXPD).Result;
                if (!string.IsNullOrEmpty(response))
                {
                    var errorCheck = GetErrorIfExists(response);
                    if (errorCheck.Statuscode == ErrorCodes.One)
                    {
                        StringBuilder sbResponse = new StringBuilder(response);
                        var _apiRes = new remitterdetailsres
                        {
                            remitterdetail = new RBLRemitter { },
                            beneficiarydetail = new List<beneficiary> { new beneficiary { } }
                        };
                        var props = GetProperties.o.GetPropertiesNameOfClassRecursively(_apiRes, null);
                        foreach (string p in props)
                        {
                            sbResponse.Replace(string.Format("<{0}/>", p), "");
                            sbResponse.Replace(string.Format("<{0} />", p), "");
                        }
                        _apiRes.beneficiarydetail = null;
                        _apiRes = XMLHelper.O.DesrializeToObject(_apiRes, sbResponse.ToString(), null);
                        if ((_apiRes.status ?? 0) == 1)
                        {
                            res.SenderName = _apiRes.remitterdetail.remittername;
                            res.ReferenceID = _apiRes.remitterdetail.remitterid;
                            if (_apiRes.remitterdetail.kycstatus == 2)
                                res.KYCStatus = SenderKYCStatus.ACTIVE;
                            if (_apiRes.remitterdetail.kycstatus == 3)
                                res.KYCStatus = SenderKYCStatus.REJECTED;
                            if (_apiRes.remitterdetail.kycstatus == 1)
                                res.KYCStatus = SenderKYCStatus.APPLIED;
                            res.RemainingLimit = Convert.ToDecimal(_apiRes.remitterdetail.remaininglimit);
                            res.AvailbleLimit = Convert.ToDecimal(_apiRes.remitterdetail.remaininglimit) + Convert.ToDecimal(_apiRes.remitterdetail.consumedlimit);
                            res.Statuscode = ErrorCodes.One;
                            res.Msg = nameof(DMTErrorCodes.Detail_Found_Successfully).Replace("_", " ");
                            res.ErrorCode = DMTErrorCodes.Detail_Found_Successfully;
                            res.IsNotActive = _apiRes.remitterdetail.remitterstatus == 0;
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
            if (!ChannelPartnerLogin())
            {
                senderCreateResp.Msg = "Token expired";
                return senderCreateResp;
            }
            var req = new remitterregistrationresendotpreq
            {
                header = new RBLHeader
                {
                    sessiontoken = RBLSession
                },
                remitterid = request.ReferenceID
            };
            var xmlReq = XMLHelper.O.SerializeToXml(req, null);
            string response = string.Empty;
            var _URL = appSetting.BASEURL + string.Format("?client_id={0}&client_secret={1}", appSetting.CLIENTID, appSetting.CLIENTSECRET);
            string authString = string.Format("{0}:{1}", appSetting.LDAP, appSetting.LDAPPASSWORD);
            var headers = new Dictionary<string, string>
            {
                { "Authorization", "Basic "+ Convert.ToBase64String(Encoding.UTF8.GetBytes(authString)) }
            };
            try
            {
                string KeyPath = Path.Combine(Directory.GetCurrentDirectory(), RBLPFXFile);
                response = AppWebRequest.O.PostJsonDataUsingHWRTLSWithCertificate(_URL, xmlReq, headers, KeyPath, appSetting.PFXPD).Result;
                if (!string.IsNullOrEmpty(response))
                {
                    var errorCheck = GetErrorIfExists(response);
                    if (errorCheck.Statuscode == ErrorCodes.One)
                    {
                        var _apiRes = new remitterregistrationresendotpres();
                        _apiRes = XMLHelper.O.DesrializeToObject(_apiRes, response, null);
                        if ((_apiRes.status ?? 0) == 1)
                        {
                            senderCreateResp.Statuscode = ErrorCodes.One;
                            senderCreateResp.Msg = nameof(DMTErrorCodes.OTP_for_verification_has_been_sent_successfully).Replace("_", " ");
                            senderCreateResp.ErrorCode = DMTErrorCodes.OTP_for_verification_has_been_sent_successfully;
                            senderCreateResp.IsOTPGenerated = true;
                            senderCreateResp.IsOTPResendAvailble = true;
                            senderCreateResp.ReferenceID = request.ReferenceID;
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
            
            var res = new MSenderLoginResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = nameof(ErrorCodes.Unknown_Error).Replace("_", ""),
                ErrorCode = ErrorCodes.Unknown_Error
            };
            if (!ChannelPartnerLogin())
            {
                res.Msg = "Token expired";
                return res;
            }
            var req = new remitterregistrationvalidatereq
            {
                header = new RBLHeader
                {
                    sessiontoken = RBLSession
                },
                remitterid = request.ReferenceID,
                mobilenumber = request.SenderMobile,
                verficationcode = request.OTP,
                channelpartnerrefno = request.TransactionID
            };
            var xmlReq = XMLHelper.O.SerializeToXml(req, null);
            string response = string.Empty;
            var _URL = appSetting.BASEURL + string.Format("?client_id={0}&client_secret={1}", appSetting.CLIENTID, appSetting.CLIENTSECRET);
            string authString = string.Format("{0}:{1}", appSetting.LDAP, appSetting.LDAPPASSWORD);
            var headers = new Dictionary<string, string>
            {
                { "Authorization", "Basic "+ Convert.ToBase64String(Encoding.UTF8.GetBytes(authString)) }
            };
            try
            {
                string KeyPath = Path.Combine(Directory.GetCurrentDirectory(), RBLPFXFile);
                response = AppWebRequest.O.PostJsonDataUsingHWRTLSWithCertificate(_URL, xmlReq, headers, KeyPath, appSetting.PFXPD).Result;
                if (!string.IsNullOrEmpty(response))
                {
                    var errorCheck = GetErrorIfExists(response);
                    if (errorCheck.Statuscode == ErrorCodes.One)
                    {
                        var _apiRes = new remitterregistrationvalidateres();
                        _apiRes = XMLHelper.O.DesrializeToObject(_apiRes, response, null);
                        if ((_apiRes.Status ?? 0) == 1)
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
            
            var res = new MSenderLoginResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            if (!ChannelPartnerLogin())
            {
                res.Msg = "Token expired";
                return res;
            }
            var req = new beneficiaryregistrationreq
            {
                header = new RBLHeader
                {
                    sessiontoken = RBLSession
                },
                bcagent = appSetting.BCAGENT,
                remitterid = request.ReferenceID,
                beneficiaryname = request.mBeneDetail.BeneName,
                accountnumber = request.mBeneDetail.AccountNo,
                ifscode = request.mBeneDetail.IFSC,
                beneficiarymobilenumber = request.mBeneDetail.MobileNo,
                beneficiaryemailid = string.IsNullOrEmpty(request.EmailID) ? " " : request.EmailID,
                relationshipid = 0,
                mmid = " ",
                flag = request.mBeneDetail.TransMode == 1 ? 2 : 3,
            };
            var xmlReq = XMLHelper.O.SerializeToXml(req, null);
            xmlReq = xmlReq.Replace("> <", "><");
            string response = string.Empty;
            var _URL = appSetting.BASEURL + string.Format("?client_id={0}&client_secret={1}", appSetting.CLIENTID, appSetting.CLIENTSECRET);
            string authString = string.Format("{0}:{1}", appSetting.LDAP, appSetting.LDAPPASSWORD);
            var headers = new Dictionary<string, string>
            {
                { "Authorization", "Basic "+ Convert.ToBase64String(Encoding.UTF8.GetBytes(authString)) }
            };
            try
            {
                string KeyPath = Path.Combine(Directory.GetCurrentDirectory(), RBLPFXFile);
                response = AppWebRequest.O.PostJsonDataUsingHWRTLSWithCertificate(_URL, xmlReq, headers, KeyPath, appSetting.PFXPD).Result;
                if (!string.IsNullOrEmpty(response))
                {
                    var errorCheck = GetErrorIfExists(response);
                    if (errorCheck.Statuscode == ErrorCodes.One)
                    {
                        var _apiRes = new beneficiaryregistrationres();
                        _apiRes = XMLHelper.O.DesrializeToObject(_apiRes, response, null);
                        if ((_apiRes.status ?? 0) == 1)
                        {
                            res.Statuscode = ErrorCodes.One;
                            res.Msg = ErrorCodes.BENESCS;
                            res.ErrorCode = DMTErrorCodes.Beneficiary_Added_Successfully;
                            //res.Msg = nameof(DMTErrorCodes.OTP_for_verification_has_been_sent_successfully).Replace("_", " ");
                            //res.ErrorCode = DMTErrorCodes.OTP_for_verification_has_been_sent_successfully;
                            res.BeneID = _apiRes.beneficiaryid.ToString();
                            res.ReferenceID = _apiRes.remitterid.ToString();
                            //res.IsOTPGenerated = true;
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
            var res = new MBeneficiaryResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = nameof(ErrorCodes.Unknown_Error).Replace("_", " "),
                ErrorCode = ErrorCodes.Unknown_Error
            };
            if (!ChannelPartnerLogin())
            {
                res.Msg = "Token expired";
                return res;
            }
            var req = new remitterdetailsreq
            {
                header = new RBLHeader
                {
                    sessiontoken = RBLSession
                },
                bcagent = appSetting.BCAGENT,
                mobilenumber = request.SenderMobile,
                flag = 1
            };
            var xmlReq = XMLHelper.O.SerializeToXml(req, null);
            string response = string.Empty;
            var _URL = appSetting.BASEURL + string.Format("?client_id={0}&client_secret={1}", appSetting.CLIENTID, appSetting.CLIENTSECRET);
            string authString = string.Format("{0}:{1}", appSetting.LDAP, appSetting.LDAPPASSWORD);
            var headers = new Dictionary<string, string>
            {
                { "Authorization", "Basic "+ Convert.ToBase64String(Encoding.UTF8.GetBytes(authString)) }
            };
            try
            {
                string KeyPath = Path.Combine(Directory.GetCurrentDirectory(), RBLPFXFile);
                response = AppWebRequest.O.PostJsonDataUsingHWRTLSWithCertificate(_URL, xmlReq, headers, KeyPath, appSetting.PFXPD).Result;
                if (!string.IsNullOrEmpty(response))
                {
                    var errorCheck = GetErrorIfExists(response);
                    if (errorCheck.Statuscode == ErrorCodes.One)
                    {
                        StringBuilder sbResponse = new StringBuilder(response);
                        var _apiRes = new remitterdetailsres
                        {
                            remitterdetail = new RBLRemitter { },
                            beneficiarydetail = new List<beneficiary> { new beneficiary { } }
                        };
                        var props = GetProperties.o.GetPropertiesNameOfClassRecursively(_apiRes, null);
                        foreach (string p in props)
                        {
                            sbResponse.Replace(string.Format("<{0}/>", p), "");
                            sbResponse.Replace(string.Format("<{0} />", p), "");
                        }
                        _apiRes.beneficiarydetail = null;
                        _apiRes = XMLHelper.O.DesrializeToObject(_apiRes, sbResponse.ToString(), null);
                        if ((_apiRes.status ?? 0) == 1)
                        {
                            var Beneficiaries = new List<MBeneDetail>();
                            foreach (var item in _apiRes.beneficiarydetail)
                            {
                                if (item.beneficiarystatus.In(0, 1) && !string.IsNullOrEmpty(item.beneficiaryid))
                                {
                                    Beneficiaries.Add(new MBeneDetail
                                    {
                                        AccountNo = item.accountnumber,
                                        BankName = item.bank,
                                        IFSC = item.ifscode,
                                        BeneName = item.beneficiaryname,
                                        MobileNo = item.beneficiarymobilenumber,
                                        BeneID = item.beneficiaryid,
                                        IsVerified = item.beneficiarystatus == 1,
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
            
            var senderCreateResp = new MSenderCreateResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = nameof(ErrorCodes.Unknown_Error).Replace("_", " "),
                ErrorCode = ErrorCodes.Unknown_Error
            };
            if (!ChannelPartnerLogin())
            {
                senderCreateResp.Msg = "Token expired";
                return senderCreateResp;
            }
            var req = new beneficiaryresendotpreq
            {
                header = new RBLHeader
                {
                    sessiontoken = RBLSession
                },
                remitterid = request.ReferenceID,
                beneficiaryid = request.mBeneDetail.BeneID
            };
            var xmlReq = XMLHelper.O.SerializeToXml(req, null);
            string response = string.Empty;
            var _URL = appSetting.BASEURL + string.Format("?client_id={0}&client_secret={1}", appSetting.CLIENTID, appSetting.CLIENTSECRET);
            string authString = string.Format("{0}:{1}", appSetting.LDAP, appSetting.LDAPPASSWORD);
            var headers = new Dictionary<string, string>
            {
                { "Authorization", "Basic "+ Convert.ToBase64String(Encoding.UTF8.GetBytes(authString)) }
            };
            try
            {
                string KeyPath = Path.Combine(Directory.GetCurrentDirectory(), RBLPFXFile);
                response = AppWebRequest.O.PostJsonDataUsingHWRTLSWithCertificate(_URL, xmlReq, headers, KeyPath, appSetting.PFXPD).Result;
                if (!string.IsNullOrEmpty(response))
                {
                    var errorCheck = GetErrorIfExists(response);
                    if (errorCheck.Statuscode == ErrorCodes.One)
                    {
                        var _apiRes = new beneficiaryresendotpres();
                        _apiRes = XMLHelper.O.DesrializeToObject(_apiRes, response, null);
                        if ((_apiRes.status ?? 0) == 1)
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
            var mSenderLoginResponse = new MSenderLoginResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = nameof(ErrorCodes.Unknown_Error).Replace("_", " "),
                ErrorCode = ErrorCodes.Unknown_Error
            };
            if (!ChannelPartnerLogin())
            {
                mSenderLoginResponse.Msg = "Token expired";
                return mSenderLoginResponse;
            }
            var req = new beneficiaryregistrationvalidatereq
            {
                header = new RBLHeader
                {
                    sessiontoken = RBLSession
                },
                beneficiaryid = request.mBeneDetail.BeneID,
                verficationcode = request.OTP
            };
            var xmlReq = XMLHelper.O.SerializeToXml(req, null);
            string response = string.Empty;
            var _URL = appSetting.BASEURL + string.Format("?client_id={0}&client_secret={1}", appSetting.CLIENTID, appSetting.CLIENTSECRET);
            string authString = string.Format("{0}:{1}", appSetting.LDAP, appSetting.LDAPPASSWORD);
            var headers = new Dictionary<string, string>
            {
                { "Authorization", "Basic "+ Convert.ToBase64String(Encoding.UTF8.GetBytes(authString)) }
            };
            try
            {
                string KeyPath = Path.Combine(Directory.GetCurrentDirectory(), RBLPFXFile);
                response = AppWebRequest.O.PostJsonDataUsingHWRTLSWithCertificate(_URL, xmlReq, headers, KeyPath, appSetting.PFXPD).Result;
                if (!string.IsNullOrEmpty(response))
                {
                    var errorCheck = GetErrorIfExists(response);
                    if (errorCheck.Statuscode == ErrorCodes.One)
                    {
                        var _apiRes = new beneficiaryregistrationvalidateres();
                        _apiRes = XMLHelper.O.DesrializeToObject(_apiRes, response, null);

                        if ((_apiRes.status ?? 0) == 1)
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
                        mSenderLoginResponse.Msg = errorCheck.Msg;
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
            
            var senderLoginResponse = new MSenderLoginResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError,
                ErrorCode = ErrorCodes.Unknown_Error
            };
            if (!ChannelPartnerLogin())
            {
                senderLoginResponse.Msg = "Token expired";
                return senderLoginResponse;
            }
            var req = new beneficairydeletereq
            {
                header = new RBLHeader
                {
                    sessiontoken = RBLSession
                },
                remitterid = request.ReferenceID,
                beneficairyid = request.mBeneDetail.BeneID
            };
            var xmlReq = XMLHelper.O.SerializeToXml(req, null);
            string response = string.Empty;
            var _URL = appSetting.BASEURL + string.Format("?client_id={0}&client_secret={1}", appSetting.CLIENTID, appSetting.CLIENTSECRET);
            string authString = string.Format("{0}:{1}", appSetting.LDAP, appSetting.LDAPPASSWORD);
            var headers = new Dictionary<string, string>
            {
                { "Authorization", "Basic "+ Convert.ToBase64String(Encoding.UTF8.GetBytes(authString)) }
            };
            try
            {
                string KeyPath = Path.Combine(Directory.GetCurrentDirectory(), RBLPFXFile);
                response = AppWebRequest.O.PostJsonDataUsingHWRTLSWithCertificate(_URL, xmlReq, headers, KeyPath, appSetting.PFXPD).Result;
                if (!string.IsNullOrEmpty(response))
                {
                    var errorCheck = GetErrorIfExists(response);
                    if (errorCheck.Statuscode == ErrorCodes.One)
                    {
                        var _apiRes = new beneficiarydeleteres();
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
            
            var mSenderLoginResponse = new MSenderLoginResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = nameof(ErrorCodes.Unknown_Error).Replace("_", " "),
                ErrorCode = ErrorCodes.Unknown_Error
            };
            if (!ChannelPartnerLogin())
            {
                mSenderLoginResponse.Msg = "Token expired";
                return mSenderLoginResponse;
            }
            var req = new beneficairydeletevalidationreq
            {
                header = new RBLHeader
                {
                    sessiontoken = RBLSession
                },
                beneficairyid = request.mBeneDetail.BeneID,
                verficationcode = request.OTP
            };
            var xmlReq = XMLHelper.O.SerializeToXml(req, null);
            string response = string.Empty;
            var _URL = appSetting.BASEURL + string.Format("?client_id={0}&client_secret={1}", appSetting.CLIENTID, appSetting.CLIENTSECRET);
            string authString = string.Format("{0}:{1}", appSetting.LDAP, appSetting.LDAPPASSWORD);
            var headers = new Dictionary<string, string>
            {
                { "Authorization", "Basic "+ Convert.ToBase64String(Encoding.UTF8.GetBytes(authString)) }
            };
            try
            {
                string KeyPath = Path.Combine(Directory.GetCurrentDirectory(), RBLPFXFile);
                response = AppWebRequest.O.PostJsonDataUsingHWRTLSWithCertificate(_URL, xmlReq, headers, KeyPath, appSetting.PFXPD).Result;
                if (!string.IsNullOrEmpty(response))
                {
                    var errorCheck = GetErrorIfExists(response);
                    if (errorCheck.Statuscode == ErrorCodes.One)
                    {
                        var _apiRes = new beneficiarydeletevalidationres();
                        _apiRes = XMLHelper.O.DesrializeToObject(_apiRes, response, "receiverdeletevalidationres", true);

                        if ((_apiRes.status ?? 0) == 1)
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
            
            var res = new DMRTransactionResponse
            {
                Statuscode = RechargeRespType.PENDING,
                Msg = RechargeRespType._PENDING,
                VendorID = "",
                LiveID = ""
            };
            if (!ChannelPartnerLogin())
            {
                res.Msg = "Token expired";
                return res;
            }
            var req = new beneficiaryaccvalidationreq
            {
                header = new RBLHeader
                {
                    sessiontoken = RBLSession
                },
                remitterid = request.ReferenceID,
                accountnumber = request.mBeneDetail.AccountNo,
                beneficiarymobilenumber = request.mBeneDetail.MobileNo,
                beneficiaryname = string.IsNullOrEmpty(request.mBeneDetail.BeneName) ? "GhanShyam" : request.mBeneDetail.BeneName,
                bcagent = appSetting.BCAGENT,
                ifscode = request.mBeneDetail.IFSC,
                channelpartnerrefno = TIDPrefix + request.TID
            };
            var xmlReq = XMLHelper.O.SerializeToXml(req, null);
            string response = string.Empty;
            var _URL = appSetting.BASEURL + string.Format("?client_id={0}&client_secret={1}", appSetting.CLIENTID, appSetting.CLIENTSECRET);
            string authString = string.Format("{0}:{1}", appSetting.LDAP, appSetting.LDAPPASSWORD);
            var headers = new Dictionary<string, string>
            {
                { "Authorization", "Basic "+ Convert.ToBase64String(Encoding.UTF8.GetBytes(authString)) }
            };
            try
            {
                string KeyPath = Path.Combine(Directory.GetCurrentDirectory(), RBLPFXFile);
                response = AppWebRequest.O.PostJsonDataUsingHWRTLSWithCertificate(_URL, xmlReq, headers, KeyPath, appSetting.PFXPD).Result;
                if (!string.IsNullOrEmpty(response))
                {
                    var errorCheck = GetErrorIfExists(response);
                    if (errorCheck.Statuscode == ErrorCodes.One)
                    {
                        var _apiRes = new beneficiaryaccvalidationres();
                        _apiRes = XMLHelper.O.DesrializeToObject(_apiRes, response, null);
                        _apiRes.status = _apiRes.status == null ? 1 : _apiRes.status;
                        if (_apiRes.status == 0)
                        {
                            res.Statuscode = RechargeRespType.FAILED;
                            res.Msg = nameof(DMTErrorCodes.Transaction_Failed).Replace("_", " ");
                            res.LiveID = _apiRes.remarks;
                            res.VendorID = _apiRes.bankrefno;
                            res.ErrorCode = DMTErrorCodes.Transaction_Failed;
                        }
                        else
                        {
                            res.Statuscode = RechargeRespType.SUCCESS;
                            res.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                            res.ErrorCode = ErrorCodes.Transaction_Successful;
                            res.LiveID = _apiRes.bankrefno;
                            res.VendorID = _apiRes.NPCIResponsecode;
                            res.BeneName = _apiRes.benename ?? "";
                        }
                    }
                    else
                    {
                        res.Statuscode = RechargeRespType.FAILED;
                        res.Msg = nameof(DMTErrorCodes.Transaction_Failed).Replace("_", " ");
                        res.LiveID = (errorCheck.Msg ?? string.Empty).Contains("insuf") ? nameof(DMTErrorCodes.Declined_by_ServiceProvider) : errorCheck.Msg;
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
            var res = new DMRTransactionResponse
            {
                Statuscode = RechargeRespType.PENDING,
                Msg = RechargeRespType._PENDING,
                VendorID = "",
                LiveID = ""
            };
            if (!ChannelPartnerLogin())
            {
                res.Msg = "Token expired";
                return res;
            }
            var req = new transactionreq
            {
                header = new RBLHeader
                {
                    sessiontoken = RBLSession
                },
                remitterid = request.ReferenceID,
                beneficiaryid = request.mBeneDetail.BeneID,
                bcagent = appSetting.BCAGENT,
                amount = request.Amount,
                cpid = appSetting.CPID,
                channelpartnerrefno = TIDPrefix + request.TID,
                remarks = "PAYMENTFOR" + request.TID,
                flag = request.TransMode.Equals("IMPS") ? 2 : 3
            };
            var xmlReq = XMLHelper.O.SerializeToXml(req, null);
            string response = string.Empty;
            var _URL = appSetting.BASEURL + string.Format("?client_id={0}&client_secret={1}", appSetting.CLIENTID, appSetting.CLIENTSECRET);
            string authString = string.Format("{0}:{1}", appSetting.LDAP, appSetting.LDAPPASSWORD);
            var headers = new Dictionary<string, string>
            {
                { "Authorization", "Basic "+ Convert.ToBase64String(Encoding.UTF8.GetBytes(authString)) }
            };
            try
            {
                string KeyPath = Path.Combine(Directory.GetCurrentDirectory(), RBLPFXFile);
                response = AppWebRequest.O.PostJsonDataUsingHWRTLSWithCertificate(_URL, xmlReq, headers, KeyPath, appSetting.PFXPD).Result;
                if (!string.IsNullOrEmpty(response))
                {
                    var errorCheck = GetErrorIfExists(response);
                    if (errorCheck.Statuscode == ErrorCodes.One)
                    {
                        var _apiRes = new transactionres();
                        _apiRes = XMLHelper.O.DesrializeToObject(_apiRes, response, null);
                        _apiRes.status = _apiRes.status == null ? 1 : _apiRes.status;
                        if ((_apiRes.status ?? 0) == 0)
                        {
                            res.Statuscode = RechargeRespType.FAILED;
                            res.Msg = nameof(DMTErrorCodes.Transaction_Failed).Replace("_", " ");
                            res.ErrorCode = DMTErrorCodes.Transaction_Failed;
                            res.LiveID = _apiRes.remarks;
                            res.VendorID = _apiRes.RBLtransactionid;
                        }
                        else
                        {
                            var tempres = LoopStatusCheck(request.TID, request.RequestMode, request.UserID, request.APIID);
                            res.Statuscode = tempres.Statuscode;
                            res.Msg = tempres.Msg;
                            res.ErrorCode = tempres.ErrorCode;
                            res.LiveID = string.IsNullOrEmpty(tempres.LiveID) ? _apiRes.bankrefno.ToString() : tempres.LiveID;
                            res.VendorID = _apiRes.RBLtransactionid;
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
        private DMRTransactionResponse LoopStatusCheck(int TID, int RequestMode, int UserID, int APIID)
        {
            var res = new DMRTransactionResponse
            {
                Statuscode = RechargeRespType.PENDING,
                Msg = nameof(ErrorCodes.Request_Accpeted).Replace("_", " "),
                ErrorCode = ErrorCodes.Request_Accpeted
            };
            int i = 0, LoopCount = 1;
            while (i < LoopCount)
            {
                i++;
                if (res.Statuscode == RechargeRespType.PENDING)
                {
                    res = GetTransactionStatus(TID, RequestMode, UserID, APIID);
                    if (res.Statuscode != RechargeRespType.PENDING)
                    {
                        i = LoopCount;
                    }
                }
                else
                {
                    i = LoopCount;
                }
            }
            return res;
        }
        public DMRTransactionResponse GetTransactionStatus(int TID, int RequestMode, int UserID, int APIID)
        {
            var res = new DMRTransactionResponse
            {
                Statuscode = RechargeRespType.PENDING,
                Msg = nameof(RechargeRespType.PENDING),
                ErrorCode = ErrorCodes.Request_Accpeted
            };
            if (!ChannelPartnerLogin())
            {
                res.Msg = "Token expired";
                return res;
            }
            var req = new transactionrequeryreq
            {
                header = new RBLHeader
                {
                    sessiontoken = RBLSession
                },
                bcagent = appSetting.BCAGENT,
                channelpartnerrefno = TIDPrefix + TID
            };
            var xmlReq = XMLHelper.O.SerializeToXml(req, null);
            string response = string.Empty;
            var _URL = appSetting.BASEURL + string.Format("?client_id={0}&client_secret={1}", appSetting.CLIENTID, appSetting.CLIENTSECRET);
            string authString = string.Format("{0}:{1}", appSetting.LDAP, appSetting.LDAPPASSWORD);
            var headers = new Dictionary<string, string>
            {
                { "Authorization", "Basic "+ Convert.ToBase64String(Encoding.UTF8.GetBytes(authString)) }
            };
            try
            {
                string KeyPath = Path.Combine(Directory.GetCurrentDirectory(), RBLPFXFile);
                response = AppWebRequest.O.PostJsonDataUsingHWRTLSWithCertificate(_URL, xmlReq, headers, KeyPath, appSetting.PFXPD).Result;

                if (!string.IsNullOrEmpty(response))
                {
                    var errorCheck = GetErrorIfExists(response);
                    if (errorCheck.Statuscode == ErrorCodes.One)
                    {
                        var _apiRes = new transactionrequeryres();
                        _apiRes = XMLHelper.O.DesrializeToObject(_apiRes, response, null);
                        if (_apiRes != null)
                        {
                            res.VendorID = _apiRes.transactionid;
                            res.LiveID = _apiRes.BankReferenceNo;
                            res.Statuscode = RechargeRespType.PENDING;
                            res.Msg = nameof(ErrorCodes.Request_Accpeted).Replace("_", " ");
                            res.ErrorCode = ErrorCodes.Request_Accpeted;
                            if ((_apiRes.status ?? 0) == -1)
                            {
                                res.Statuscode = RechargeRespType.FAILED;
                                res.Msg = _apiRes.BankRemarks;
                                res.ErrorCode = DMTErrorCodes.Transaction_Failed;
                                res.LiveID = _apiRes.BankRemarks;
                            }
                            else
                            {
                                if ((_apiRes.status ?? 0) > 1)
                                {
                                    _apiRes.paymentstatus = _apiRes.paymentstatus ?? 0;
                                    if (_apiRes.paymentstatus == RBPaymentStatus.credited)
                                    {
                                        res.Statuscode = RechargeRespType.SUCCESS;
                                        res.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                                        res.ErrorCode = ErrorCodes.Transaction_Successful;
                                    }
                                    else if (_apiRes.paymentstatus.In(RBPaymentStatus.refundprocess))
                                    {
                                        res.Statuscode = RechargeRespType.FAILED;
                                        res.Msg = _apiRes.BankRemarks;
                                        res.ErrorCode = DMTErrorCodes.Transaction_Failed;
                                        res.LiveID = _apiRes.BankRemarks;
                                    }
                                    else if (_apiRes.paymentstatus == RBPaymentStatus.refund)
                                    {
                                        res.Statuscode = RechargeRespType.SUCCESS;
                                        res.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                                        res.ErrorCode = ErrorCodes.Transaction_Successful;
                                        res.IsRefundAvailable = true;
                                    }
                                }
                            }
                            if ((_apiRes.TranType ?? string.Empty).Equals("NEFT") && res.Statuscode != RechargeRespType.FAILED)
                            {
                                res.LiveID = _apiRes.UTRNo;
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
        public ResponseStatus RefundOTP(string VendorID, int RequestMode, int UserID, string SenderNo, int TID, int APIID)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = nameof(ErrorCodes.Unknown_Error).Replace("_", " "),
                ErrorCode = ErrorCodes.Unknown_Error
            };
            if (!ChannelPartnerLogin())
            {
                res.Msg = "Token expired";
                return res;
            }
            var req = new refundotpreq
            {
                header = new RBLHeader
                {
                    sessiontoken = RBLSession
                },
                RBLtransactionid = VendorID
            };
            var xmlReq = XMLHelper.O.SerializeToXml(req, null);
            string response = string.Empty;
            var _URL = appSetting.BASEURL + string.Format("?client_id={0}&client_secret={1}", appSetting.CLIENTID, appSetting.CLIENTSECRET);
            string authString = string.Format("{0}:{1}", appSetting.LDAP, appSetting.LDAPPASSWORD);
            var headers = new Dictionary<string, string>
            {
                { "Authorization", "Basic "+ Convert.ToBase64String(Encoding.UTF8.GetBytes(authString)) }
            };
            try
            {
                string KeyPath = Path.Combine(Directory.GetCurrentDirectory(), RBLPFXFile);
                response = AppWebRequest.O.PostJsonDataUsingHWRTLSWithCertificate(_URL, xmlReq, headers, KeyPath, appSetting.PFXPD).Result;
                if (!string.IsNullOrEmpty(response))
                {
                    var errorCheck = GetErrorIfExists(response);
                    if (errorCheck.Statuscode == ErrorCodes.One)
                    {
                        var _apiRes = new refundotpres();
                        _apiRes = XMLHelper.O.DesrializeToObject(_apiRes, response, null);
                        if ((_apiRes.status ?? 0) == 1)
                        {
                            res.Statuscode = ErrorCodes.One;
                            res.Msg = nameof(DMTErrorCodes.OTP_for_verification_has_been_sent_successfully).Replace("_", " ");
                            res.ErrorCode = DMTErrorCodes.OTP_for_verification_has_been_sent_successfully;
                        }
                        else
                        {

                            res.Msg = nameof(DMTErrorCodes.Declined_by_ServiceProvider).Replace("_", " ");
                            res.ErrorCode = DMTErrorCodes.Declined_by_ServiceProvider;
                        }
                    }
                    else
                    {

                        res.Msg = errorCheck.Msg;
                        res.ErrorCode = DMTErrorCodes.Declined_by_ServiceProvider;
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
                Request = _URL + xmlReq + JsonConvert.SerializeObject(headers),
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
            if (!ChannelPartnerLogin())
            {
                res.Msg = "Token expired";
                return res;
            }
            var req = new refundreq
            {
                header = new RBLHeader
                {
                    sessiontoken = RBLSession
                },
                bcagent = appSetting.BCAGENT,
                channelpartnerrefno = TIDPrefix + TID,
                verficationcode = OTP,
                flag = 1
            };
            var xmlReq = XMLHelper.O.SerializeToXml(req, null);
            string response = string.Empty;
            var _URL = appSetting.BASEURL + string.Format("?client_id={0}&client_secret={1}", appSetting.CLIENTID, appSetting.CLIENTSECRET);
            string authString = string.Format("{0}:{1}", appSetting.LDAP, appSetting.LDAPPASSWORD);
            var headers = new Dictionary<string, string>
            {
                { "Authorization", "Basic "+ Convert.ToBase64String(Encoding.UTF8.GetBytes(authString)) }
            };
            try
            {
                string KeyPath = Path.Combine(Directory.GetCurrentDirectory(), RBLPFXFile);
                response = AppWebRequest.O.PostJsonDataUsingHWRTLSWithCertificate(_URL, xmlReq, headers, KeyPath, appSetting.PFXPD).Result;
                if (!string.IsNullOrEmpty(response))
                {
                    var errorCheck = GetErrorIfExists(response);
                    if (errorCheck.Statuscode == ErrorCodes.One)
                    {
                        var _apiRes = new refundres();
                        _apiRes = XMLHelper.O.DesrializeToObject(_apiRes, response, null);
                        if ((_apiRes.status ?? 0) == 1)
                        {
                            res.Statuscode = ErrorCodes.One;
                            res.Msg = nameof(DMTErrorCodes.Transaction_Status_Updated_Refund).Replace("_", " ");
                            res.ErrorCode = DMTErrorCodes.Transaction_Status_Updated_Refund;
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
                Request = _URL + xmlReq + JsonConvert.SerializeObject(headers),
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
