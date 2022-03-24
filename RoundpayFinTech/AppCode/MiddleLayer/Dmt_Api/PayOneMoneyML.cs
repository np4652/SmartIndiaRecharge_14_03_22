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
using RoundpayFinTech.AppCode.ThirdParty.RechargeDaddy;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;

namespace RoundpayFinTech.AppCode.MiddleLayer.Dmt_Api
{
    public class PayOneMoneyML : IMoneyTransferAPIML
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        private readonly IConfiguration Configuration;
        private readonly RechargeDaddySetting appSetting;
        private readonly IDAL _dal;
        public PayOneMoneyML(IHttpContextAccessor accessor, IHostingEnvironment env, IDAL dal)
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
        private RechargeDaddySetting AppSetting()
        {
            var setting = new RechargeDaddySetting();
            try
            {
                setting = new RechargeDaddySetting
                {
                    MobileNo = Configuration["DMR:P1Money:MobileNo"],
                    APIKey = Configuration["DMR:P1Money:APIKey"],
                    AgentCode = Configuration["DMR:P1Money:AgentCode"],
                    Version = Configuration["DMR:P1Money:Version"],
                    BASEURL = Configuration["DMR:P1Money:BASEURL"]
                };
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "RechargeDaddySetting",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser
                });
            }
            return setting;
        }
        private static string strHelp = @"<?xml version=""1.0"" encoding=""utf-8""?>
<soap:Envelope
    xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/""
    xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""
    xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
    <soap:Header>
        <DMRAuthHeader
            xmlns=""http://tempuri.org/"">
            <MobileNo>{HMobileNo}</MobileNo>
            <APIKey>{HAPIKey}</APIKey>
            <ResponseType>JSON</ResponseType>
            <Checksum>{HChecksum}</Checksum>
            <Version>{HVersion}</Version>
            <AgentCode>{HAgentCode}</AgentCode>
        </DMRAuthHeader>
    </soap:Header>
    <soap:Body>
        <{METHOD}
            xmlns=""http://tempuri.org/"">
            <sRequest>{NTDREQ}</sRequest>
        </{METHOD}>
    </soap:Body>
</soap:Envelope>";
        public MSenderLoginResponse GetSender(MTAPIRequest request)
        
        {
            var objReq = new GetSenderNTDRequest
            {
                REQTYPE = "GC",
                CUSTOMERNO = request.SenderMobile
            };
            var METHOD = "GetCustomer";
            var NTDREQ = XMLHelper.O.SerializeToXml(objReq, "NTDREQ");
            var CheckSum = HashEncryption.O.GenerateSHA512(appSetting.APIKey, NTDREQ);
            StringBuilder sb = new StringBuilder(strHelp);
            sb.Replace("{HMobileNo}", appSetting.MobileNo);
            sb.Replace("{HAPIKey}", appSetting.APIKey);
            sb.Replace("{HAgentCode}", appSetting.AgentCode);
            sb.Replace("{HChecksum}", CheckSum);
            sb.Replace("{HVersion}", appSetting.Version);
            sb.Replace("{METHOD}", METHOD);
            sb.Replace("{NTDREQ}", XMLHelper.O.EncodeForXml(NTDREQ));

            var _URL = appSetting.BASEURL;
            var res = new MSenderLoginResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = nameof(ErrorCodes.Unknown_Error).Replace("_", " "),
                ErrorCode = ErrorCodes.Unknown_Error
            };
            string req = string.Format("{0}?{1}", _URL, sb.ToString());
            string response = string.Empty;
            try
            {
                response = AppWebRequest.O.CallUsingHttpWebRequest_POST(_URL, sb.ToString(), ContentType.text_xml);

                var responseJson = ExtractJSON(response);

                if (!string.IsNullOrEmpty(responseJson))
                {
                    var _apiRes = JsonConvert.DeserializeObject<RDaddyResponse>(responseJson);
                    if (_apiRes != null)
                    {
                        if ((_apiRes.NTDRESP.STATUSCODE ?? -100) == 0)
                        {
                            res.IsNotActive = _apiRes.NTDRESP.STATUS != 1;

                            if (res.IsNotActive)
                            {
                                res.Statuscode = ErrorCodes.One;
                                res.Msg = nameof(DMTErrorCodes.Partial_Registration_Please_verify_sender_with_OTP).Replace("_", " ");
                                res.ErrorCode = DMTErrorCodes.Partial_Registration_Please_verify_sender_with_OTP;
                                res.IsOTPGenerated = true;

                            }
                            else
                            {
                                res.Statuscode = ErrorCodes.One;
                                res.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                                res.ErrorCode = ErrorCodes.Transaction_Successful;
                                res.SenderName = (_apiRes.NTDRESP.FNAME ?? string.Empty);
                                res.SenderName = !string.IsNullOrEmpty(_apiRes.NTDRESP.LNAME) ? string.Format("{0} {1}", res.SenderName, _apiRes.NTDRESP.LNAME) : res.SenderName;
                                res.KYCStatus = SenderKYCStatus.ACTIVE;
                                res.RemainingLimit = Convert.ToDecimal(_apiRes.NTDRESP.REMAIN ?? "0");
                                res.AvailbleLimit = Convert.ToDecimal(_apiRes.NTDRESP.LIMIT ?? "0");
                            }
                        }
                        else if ((_apiRes.NTDRESP.STATUSCODE ?? -100) == 1020)
                        {
                            res.IsSenderNotExists = true;//change by me
                            res.IsNotActive = false;//change by me
                            IErrorCodeML errorCodeML = new ErrorCodeML(_accessor, _env, false);
                            var eFromDB = errorCodeML.GetAPIErrorCodeDescription(request.APIGroupCode, _apiRes.NTDRESP.STATUSCODE.ToString());
                            if (!string.IsNullOrEmpty(eFromDB.Code))
                            {
                                res.Msg = eFromDB.Error;
                                res.ErrorCode = Convert.ToInt32(eFromDB.Code.Trim());
                                if (res.ErrorCode == 110)
                                    res.Statuscode = ErrorCodes.One;
                            }
                        }
                        else if (_apiRes.NTDRESP.STATUSCODE != null)
                        {
                            res.IsSenderNotExists = true;
                            IErrorCodeML errorCodeML = new ErrorCodeML(_accessor, _env, false);
                            var eFromDB = errorCodeML.GetAPIErrorCodeDescription(request.APIGroupCode, _apiRes.NTDRESP.STATUSCODE.ToString());
                            if (!string.IsNullOrEmpty(eFromDB.Code))
                            {
                                res.Msg = eFromDB.Error;
                                res.ErrorCode = Convert.ToInt32(eFromDB.Code.Trim());
                                if (res.ErrorCode == 111)
                                    res.Statuscode = ErrorCodes.One;
                            }
                        }
                        else
                        {
                            res.Msg = nameof(DMTErrorCodes.Sender_Not_Found).Replace("_", " ");
                            res.ErrorCode = DMTErrorCodes.Sender_Not_Found;
                            res.IsSenderNotExists = true;
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
                    FuncName = "GetSender",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = request.UserID
                });
            }
            new ProcUpdateLogDMRReqResp(_dal).Call(new DMTReqRes
            {
                APIID = request.APIID,
                Method = "GetSender",
                RequestModeID = request.RequestMode,
                Request = req,
                Response = response,
                SenderNo = request.SenderMobile,
                UserID = request.UserID,
                TID = request.TransactionID
            });
            return res;
        }
        public MSenderCreateResp CreateSender(MTAPIRequest request)
        {
            var res = new MSenderCreateResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = nameof(ErrorCodes.Unknown_Error).Replace("_", " "),
                ErrorCode = ErrorCodes.Unknown_Error
            };
            var objReq = new CreateSenderNTDRequest
            {
                REQTYPE = "AC",
                CUSTOMERNO = request.SenderMobile,
                FNAME = request.FirstName,
                LNAME = request.LastName,
                ANAME = request.Address,
                ADD1 = request.Area,
                ADD2 = request.Districtname,
                CITY = request.City,
                STATE = request.StateName,
                COUNTRY = "India",
                PCODE = request.Pincode.ToString()
            };
            var METHOD = "AddCustomer";
            var NTDREQ = XMLHelper.O.SerializeToXml(objReq, "NTDREQ");
            var CheckSum = HashEncryption.O.GenerateSHA512(appSetting.APIKey, NTDREQ);
            StringBuilder sb = new StringBuilder(strHelp);
            sb.Replace("{HMobileNo}", appSetting.MobileNo);
            sb.Replace("{HAPIKey}", appSetting.APIKey);
            sb.Replace("{HAgentCode}", appSetting.AgentCode);
            sb.Replace("{HChecksum}", CheckSum);
            sb.Replace("{HVersion}", appSetting.Version);
            sb.Replace("{METHOD}", METHOD);
            sb.Replace("{NTDREQ}", XMLHelper.O.EncodeForXml(NTDREQ));

            var _URL = appSetting.BASEURL;

            string req = string.Format("{0}?{1}", _URL, sb.ToString());
            string response = string.Empty;
            //string responseJson = string.Empty;
            try
            {
                response = AppWebRequest.O.CallUsingHttpWebRequest_POST(_URL, sb.ToString(), ContentType.text_xml);

                var responseJson = ExtractJSON(response);

                if (!string.IsNullOrEmpty(responseJson))
                {
                    var _apiRes = JsonConvert.DeserializeObject<RDaddyResponse>(responseJson);
                    if (_apiRes != null)
                    {
                        if ((_apiRes.NTDRESP.STATUSCODE ?? -100) == 0)
                        {
                            res.IsOTPGenerated = _apiRes.NTDRESP.OTPREQ == 1;
                            res.Statuscode = ErrorCodes.One;
                            if (res.IsOTPGenerated == true)
                            {
                                res.Msg = nameof(DMTErrorCodes.Partial_Registration_Please_verify_sender_with_OTP).Replace("_", " ");
                                res.ErrorCode = DMTErrorCodes.Partial_Registration_Please_verify_sender_with_OTP;
                            }
                            else
                            {
                                res.Msg = nameof(DMTErrorCodes.Sender_Created_Successfully).Replace("_", " ");
                                res.ErrorCode = DMTErrorCodes.Sender_Created_Successfully;
                            }
                        }
                        else if ((_apiRes.NTDRESP.STATUSCODE ?? -100) == 1025)
                        {
                            res = SenderResendOTP(request);
                        }
                        else if (_apiRes.NTDRESP.STATUSCODE != null)
                        {
                            IErrorCodeML errorCodeML = new ErrorCodeML(_accessor, _env, false);
                            var eFromDB = errorCodeML.GetAPIErrorCodeDescription(request.APIGroupCode, _apiRes.NTDRESP.STATUSCODE.ToString());
                            if (!string.IsNullOrEmpty(eFromDB.Code))
                            {
                                res.Msg = eFromDB.Error;
                                res.ErrorCode = Convert.ToInt32(eFromDB.Code.Trim());
                            }
                        }
                        else
                        {
                            res.Msg = nameof(DMTErrorCodes.Sender_Already_Exist).Replace("_", " ");
                            res.ErrorCode = DMTErrorCodes.Sender_Already_Exist;
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
                    UserId = request.UserID
                });
            }
            new ProcUpdateLogDMRReqResp(_dal).Call(new DMTReqRes
            {
                APIID = request.APIID,
                Method = "CreateSender",
                RequestModeID = request.RequestMode,
                Request = req,
                Response = response,
                SenderNo = request.SenderMobile,
                UserID = request.UserID,
                TID = request.TransactionID
            });
            return res;
        }
        public MSenderCreateResp SenderEKYC(MTAPIRequest request)
        {
            throw new NotImplementedException();
        }
        public MSenderCreateResp SenderKYC(MTAPIRequest request)
        {
            throw new NotImplementedException();
        }
        public MSenderCreateResp SenderResendOTP(MTAPIRequest request)
        {
            var res = new MSenderCreateResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError,
                ErrorCode = ErrorCodes.Unknown_Error
            };
            var objReq = new ResendCreateSenderOTPNTDReq
            {
                REQTYPE = "RACO",
                CUSTOMERNO = request.SenderMobile
            };
            var METHOD = "ResendAddCustomerOTP";
            var NTDREQ = XMLHelper.O.SerializeToXml(objReq, "NTDREQ");
            var CheckSum = HashEncryption.O.GenerateSHA512(appSetting.APIKey, NTDREQ);
            StringBuilder sb = new StringBuilder(strHelp);
            sb.Replace("{HMobileNo}", appSetting.MobileNo);
            sb.Replace("{HAPIKey}", appSetting.APIKey);
            sb.Replace("{HAgentCode}", appSetting.AgentCode);
            sb.Replace("{HChecksum}", CheckSum);
            sb.Replace("{HVersion}", appSetting.Version);
            sb.Replace("{METHOD}", METHOD);
            sb.Replace("{NTDREQ}", XMLHelper.O.EncodeForXml(NTDREQ));

            var _URL = appSetting.BASEURL;

            string req = string.Format("{0}?{1}", _URL, sb.ToString());
            string response = string.Empty;

            try
            {
                response = AppWebRequest.O.CallUsingHttpWebRequest_POST(_URL, sb.ToString(), ContentType.text_xml);


                var responseJson = ExtractJSON(response);

                if (!string.IsNullOrEmpty(responseJson))
                {
                    var _apiRes = JsonConvert.DeserializeObject<RDaddyResponse>(responseJson);
                    if (_apiRes != null)
                    {
                        if ((_apiRes.NTDRESP.STATUSCODE ?? -100) == 0)
                        {
                            res.IsOTPGenerated = _apiRes.NTDRESP.OTPREQ == 1;
                            res.Statuscode = ErrorCodes.One;
                            if (res.IsOTPGenerated == true)
                            {
                                res.IsOTPResendAvailble = true;
                                res.Msg = nameof(DMTErrorCodes.Partial_Registration_Please_verify_sender_with_OTP).Replace("_", " ");
                                res.ErrorCode = DMTErrorCodes.Partial_Registration_Please_verify_sender_with_OTP;
                            }
                            else
                            {
                                res.Msg = nameof(DMTErrorCodes.Sender_Already_Exist).Replace("_", " ");
                                res.ErrorCode = DMTErrorCodes.Sender_Already_Exist;
                            }
                        }
                        else if (_apiRes.NTDRESP.STATUSCODE != null)
                        {
                            IErrorCodeML errorCodeML = new ErrorCodeML(_accessor, _env, false);
                            var eFromDB = errorCodeML.GetAPIErrorCodeDescription(request.APIGroupCode, _apiRes.NTDRESP.STATUSCODE.ToString());
                            if (!string.IsNullOrEmpty(eFromDB.Code))
                            {
                                res.Msg = eFromDB.Error;
                                res.ErrorCode = Convert.ToInt32(eFromDB.Code.Trim());
                            }
                        }
                        else
                        {
                            res.Msg = nameof(ErrorCodes.Unknown_Error).Replace("_", " ");
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
                Request = req,
                Response = response,
                SenderNo = request.SenderMobile,
                UserID = request.UserID,
                TID = request.TransactionID
            });
            return res;
        }
        public MSenderLoginResponse VerifySender(MTAPIRequest request)
        {
            var res = new MSenderLoginResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = nameof(ErrorCodes.Unknown_Error).Replace("_", ""),
                ErrorCode = ErrorCodes.Unknown_Error
            };
            var METHOD = "ValidateCustomerOTP";
            var objReq = new VerifySenderNTDRequest
            {
                REQTYPE = "VCO",
                CUSTOMERNO = request.SenderMobile,
                OTP = request.OTP
            };
            var NTDREQ = XMLHelper.O.SerializeToXml(objReq, "NTDREQ");
            var CheckSum = HashEncryption.O.GenerateSHA512(appSetting.APIKey, NTDREQ);
            StringBuilder sb = new StringBuilder(strHelp);
            sb.Replace("{HMobileNo}", appSetting.MobileNo);
            sb.Replace("{HAPIKey}", appSetting.APIKey);
            sb.Replace("{HAgentCode}", appSetting.AgentCode);
            sb.Replace("{HChecksum}", CheckSum);
            sb.Replace("{HVersion}", appSetting.Version);
            sb.Replace("{METHOD}", METHOD);
            sb.Replace("{NTDREQ}", XMLHelper.O.EncodeForXml(NTDREQ));

            var _URL = appSetting.BASEURL;

            string req = string.Format("{0}?{1}", _URL, sb.ToString());
            string response = string.Empty;
            try
            {
                response = AppWebRequest.O.CallUsingHttpWebRequest_POST(_URL, sb.ToString(), ContentType.text_xml);
                var responseJson = ExtractJSON(response);

                if (!string.IsNullOrEmpty(responseJson))
                {
                    var _apiRes = JsonConvert.DeserializeObject<RDaddyResponse>(responseJson);
                    if (_apiRes != null)
                    {
                        if ((_apiRes.NTDRESP.STATUSCODE ?? -100) == 0)
                        {
                            res.Statuscode = ErrorCodes.One;
                            res.Msg = nameof(DMTErrorCodes.Sender_Created_Successfully).Replace("_", " ");
                            res.ErrorCode = DMTErrorCodes.Sender_Created_Successfully;
                        }
                        else if ((_apiRes.NTDRESP.STATUSCODE ?? -100).In(1026, 1000))
                        {
                            res.Msg = nameof(ErrorCodes.Invalid_OTP).Replace("_", " ");
                            res.ErrorCode = ErrorCodes.Invalid_OTP;
                        }
                        else if (_apiRes.NTDRESP.STATUSCODE != null)
                        {
                            IErrorCodeML errorCodeML = new ErrorCodeML(_accessor, _env, false);
                            var eFromDB = errorCodeML.GetAPIErrorCodeDescription(request.APIGroupCode, _apiRes.NTDRESP.STATUSCODE.ToString());
                            if (!string.IsNullOrEmpty(eFromDB.Code))
                            {
                                res.Msg = eFromDB.Error;
                                res.ErrorCode = Convert.ToInt32(eFromDB.Code.Trim());
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
                Request = req,
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
            IBankML bankML = new BankML(_accessor, _env, false);
            var BankDetail = bankML.BankMasters(request.mBeneDetail.BankID);
            if (BankDetail.RDaddyBankID < 1)
            {
                res.Msg = nameof(DMTErrorCodes.Acquiring_Bank_CBS_or_node_offline).Replace("_", " ");
                res.ErrorCode = DMTErrorCodes.Acquiring_Bank_CBS_or_node_offline;
                return res;
            }
            var objReq = new CreateBeneficiaryNTDReq
            {
                REQTYPE = "AB",
                CUSTOMERNO = request.SenderMobile,
                NAME = request.mBeneDetail.BeneName,
                MOBILENO = request.mBeneDetail.MobileNo,
                BANKID = BankDetail.Pay1MoneyBankID,
                ACCNO = request.mBeneDetail.AccountNo,
                IFSC = request.mBeneDetail.IFSC
            };
            var METHOD = "AddBeneficiary";
            var NTDREQ = XMLHelper.O.SerializeToXml(objReq, "NTDREQ");
            var CheckSum = HashEncryption.O.GenerateSHA512(appSetting.APIKey, NTDREQ);
            StringBuilder sb = new StringBuilder(strHelp);
            sb.Replace("{HMobileNo}", appSetting.MobileNo);
            sb.Replace("{HAPIKey}", appSetting.APIKey);
            sb.Replace("{HAgentCode}", appSetting.AgentCode);
            sb.Replace("{HChecksum}", CheckSum);
            sb.Replace("{HVersion}", appSetting.Version);
            sb.Replace("{METHOD}", METHOD);
            sb.Replace("{NTDREQ}", XMLHelper.O.EncodeForXml(NTDREQ));

            var _URL = appSetting.BASEURL;

            string req = string.Format("{0}?{1}", _URL, sb.ToString());
            string response = string.Empty;
            try
            {
                response = AppWebRequest.O.CallUsingHttpWebRequest_POST(_URL, sb.ToString(), ContentType.text_xml);
                var responseJson = ExtractJSON(response);

                if (!string.IsNullOrEmpty(responseJson))
                {
                    var _apiRes = JsonConvert.DeserializeObject<RDaddyResponse>(responseJson);
                    if (_apiRes != null)
                    {
                        if ((_apiRes.NTDRESP.STATUSCODE ?? -100) == 0)
                        {
                            res.Statuscode = ErrorCodes.One;
                            res.ReferenceID = _apiRes.NTDRESP.BENEID.ToString();
                            res.BeneID = _apiRes.NTDRESP.BENEID.ToString();
                            res.IsOTPGenerated = _apiRes.NTDRESP.OTPREQ == 1;
                            if (res.IsOTPGenerated)
                            {
                                res.Msg = nameof(DMTErrorCodes.OTP_for_verification_has_been_sent_successfully).Replace("_", " ");
                                res.ErrorCode = DMTErrorCodes.OTP_for_verification_has_been_sent_successfully;
                            }
                            else
                            {
                                res.Msg = nameof(DMTErrorCodes.Beneficiary_Added_Successfully).Replace("_", " ");
                                res.ErrorCode = DMTErrorCodes.Beneficiary_Added_Successfully;
                            }
                        }
                        else if (_apiRes.NTDRESP.STATUSCODE != null)
                        {
                            IErrorCodeML errorCodeML = new ErrorCodeML(_accessor, _env, false);
                            var eFromDB = errorCodeML.GetAPIErrorCodeDescription(request.APIGroupCode, _apiRes.NTDRESP.STATUSCODE.ToString());
                            if (!string.IsNullOrEmpty(eFromDB.Code))
                            {
                                res.Msg = eFromDB.Error;
                                res.ErrorCode = Convert.ToInt32(eFromDB.Code.Trim());
                            }
                            else
                            {
                                res.Msg = _apiRes.NTDRESP.STATUSMSG;
                                res.ErrorCode = ErrorCodes.Transaction_Replace;
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
                    FuncName = "AddBeneficiary",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = request.UserID
                });
            }
            new ProcUpdateLogDMRReqResp(_dal).Call(new DMTReqRes
            {
                APIID = request.APIID,
                Method = "AddBeneficiary",
                RequestModeID = request.RequestMode,
                Request = req,
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
            var objReq = new GetBeneficiaryListReq
            {
                REQTYPE = "GAB",
                CUSTOMERNO = request.SenderMobile
            };
            var METHOD = "GetAllBeneficiary";
            var NTDREQ = XMLHelper.O.SerializeToXml(objReq, "NTDREQ");
            var CheckSum = HashEncryption.O.GenerateSHA512(appSetting.APIKey, NTDREQ);
            StringBuilder sb = new StringBuilder(strHelp);
            sb.Replace("{HMobileNo}", appSetting.MobileNo);
            sb.Replace("{HAPIKey}", appSetting.APIKey);
            sb.Replace("{HAgentCode}", appSetting.AgentCode);
            sb.Replace("{HChecksum}", CheckSum);
            sb.Replace("{HVersion}", appSetting.Version);
            sb.Replace("{METHOD}", METHOD);
            sb.Replace("{NTDREQ}", XMLHelper.O.EncodeForXml(NTDREQ));

            var _URL = appSetting.BASEURL;

            string req = string.Format("{0}?{1}", _URL, sb.ToString());
            string response = string.Empty;
            try
            {
                response = AppWebRequest.O.CallUsingHttpWebRequest_POST(_URL, sb.ToString(), ContentType.text_xml);
                var responseJson = ExtractJSON(response);


                if (!string.IsNullOrEmpty(responseJson))
                {
                    var _apiRes = JsonConvert.DeserializeObject<RDaddyResponse>(responseJson);
                    if (_apiRes != null)
                    {
                        if ((_apiRes.NTDRESP.STATUSCODE ?? -100) == 0)
                        {
                            res.Statuscode = ErrorCodes.One;
                            res.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                            res.ErrorCode = ErrorCodes.Transaction_Successful;
                            if (_apiRes.NTDRESP.BENELIST != null)
                            {
                                var Beneficiaries = new List<MBeneDetail>();
                                foreach (var item in _apiRes.NTDRESP.BENELIST)
                                {
                                    Beneficiaries.Add(new MBeneDetail
                                    {
                                        AccountNo = item.ACCNO,
                                        BankName = item.BANKNAME,
                                        IFSC = item.IFSC,
                                        BeneName = item.BENENAME,
                                        MobileNo = item.MOBILENO,
                                        BeneID = item.BENEID.ToString(),
                                        IsVerified = item.STATUS == 1,
                                        NEFTStatus = true,
                                        IMPSStatus = true
                                    });
                                }
                                res.Beneficiaries = Beneficiaries;
                            }
                        }
                        else if (_apiRes.NTDRESP.STATUSCODE != null)
                        {
                            IErrorCodeML errorCodeML = new ErrorCodeML(_accessor, _env, false);
                            var eFromDB = errorCodeML.GetAPIErrorCodeDescription(request.APIGroupCode, _apiRes.NTDRESP.STATUSCODE.ToString());
                            if (!string.IsNullOrEmpty(eFromDB.Code))
                            {
                                res.Msg = eFromDB.Error;
                                res.ErrorCode = Convert.ToInt32(eFromDB.Code.Trim());
                            }
                        }
                        else
                        {
                            res.Msg = nameof(ErrorCodes.Unknown_Error).Replace("_", " ");
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
                    FuncName = "GetBeneficiary",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = request.UserID
                });
            }
            new ProcUpdateLogDMRReqResp(_dal).Call(new DMTReqRes
            {
                APIID = request.APIID,
                Method = "GetBeneficiary",
                RequestModeID = request.RequestMode,
                Request = req,
                Response = response,
                SenderNo = request.SenderMobile,
                UserID = request.UserID,
                TID = request.TransactionID
            });
            return res;
        }
        public MSenderCreateResp GenerateOTP(MTAPIRequest request)
        {
            var res = new MSenderCreateResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError,
                ErrorCode = ErrorCodes.Unknown_Error
            };
            var objReq = new ResendCreateBeneficiaryOTPNTDReq
            {
                REQTYPE = "RABO",
                CUSTOMERNO = request.SenderMobile
            };
            var METHOD = "ResendAddBeneficiaryOTP";
            var NTDREQ = XMLHelper.O.SerializeToXml(objReq, "NTDREQ");
            var CheckSum = HashEncryption.O.GenerateSHA512(appSetting.APIKey, NTDREQ);
            StringBuilder sb = new StringBuilder(strHelp);
            sb.Replace("{HMobileNo}", appSetting.MobileNo);
            sb.Replace("{HAPIKey}", appSetting.APIKey);
            sb.Replace("{HAgentCode}", appSetting.AgentCode);
            sb.Replace("{HChecksum}", CheckSum);
            sb.Replace("{HVersion}", appSetting.Version);
            sb.Replace("{METHOD}", METHOD);
            sb.Replace("{NTDREQ}", XMLHelper.O.EncodeForXml(NTDREQ));

            var _URL = appSetting.BASEURL;

            string req = string.Format("{0}?{1}", _URL, sb.ToString());
            string response = string.Empty;

            try
            {
                response = AppWebRequest.O.CallUsingHttpWebRequest_POST(_URL, sb.ToString(), ContentType.text_xml);
                var responseJson = ExtractJSON(response);

                if (!string.IsNullOrEmpty(responseJson))
                {
                    var _apiRes = JsonConvert.DeserializeObject<RDaddyResponse>(responseJson);
                    if (_apiRes != null)
                    {
                        if ((_apiRes.NTDRESP.STATUSCODE ?? -100) == 0)
                        {
                            res.IsOTPGenerated = _apiRes.NTDRESP.OTPREQ == 1;
                            res.Statuscode = ErrorCodes.One;
                            if (res.IsOTPGenerated == true)
                            {
                                res.Msg = nameof(DMTErrorCodes.OTP_for_verification_has_been_sent_successfully).Replace("_", " ");
                                res.ErrorCode = DMTErrorCodes.OTP_for_verification_has_been_sent_successfully;
                            }
                            else
                            {
                                res.Msg = nameof(DMTErrorCodes.Sender_Already_Exist).Replace("_", " ");
                                res.ErrorCode = DMTErrorCodes.Sender_Already_Exist;
                            }
                        }
                        else if (_apiRes.NTDRESP.STATUSCODE != null)
                        {
                            IErrorCodeML errorCodeML = new ErrorCodeML(_accessor, _env, false);
                            var eFromDB = errorCodeML.GetAPIErrorCodeDescription(request.APIGroupCode, _apiRes.NTDRESP.STATUSCODE.ToString());
                            if (!string.IsNullOrEmpty(eFromDB.Code))
                            {
                                res.Msg = eFromDB.Error;
                                res.ErrorCode = Convert.ToInt32(eFromDB.Code.Trim());
                            }
                        }
                        else
                        {
                            res.Msg = nameof(ErrorCodes.Unknown_Error).Replace("_", " ");
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
                Request = req,
                Response = response,
                SenderNo = request.SenderMobile,
                UserID = request.UserID,
                TID = request.TransactionID
            });
            return res;
        }
        public MSenderLoginResponse ValidateBeneficiary(MTAPIRequest request)
        {
            var mSenderLoginResponse = new MSenderLoginResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = nameof(ErrorCodes.Unknown_Error).Replace("_", " "),
                ErrorCode = ErrorCodes.Unknown_Error
            };
            var METHOD = "ValidateBeneficiaryOTP";
            var objReq = new ValidateBeneficiaryOTPNTDReq
            {
                REQTYPE = "VBO",
                CUSTOMERNO = request.SenderMobile,
                BENEID = Convert.ToInt32(request.mBeneDetail.BeneID ?? "0"),
                OTP = request.OTP
            };
            var NTDREQ = XMLHelper.O.SerializeToXml(objReq, "NTDREQ");
            var CheckSum = HashEncryption.O.GenerateSHA512(appSetting.APIKey, NTDREQ);
            StringBuilder sb = new StringBuilder(strHelp);
            sb.Replace("{HMobileNo}", appSetting.MobileNo);
            sb.Replace("{HAPIKey}", appSetting.APIKey);
            sb.Replace("{HAgentCode}", appSetting.AgentCode);
            sb.Replace("{HChecksum}", CheckSum);
            sb.Replace("{HVersion}", appSetting.Version);
            sb.Replace("{METHOD}", METHOD);
            sb.Replace("{NTDREQ}", XMLHelper.O.EncodeForXml(NTDREQ));

            var _URL = appSetting.BASEURL;
            string req = string.Format("{0}?{1}", _URL, sb.ToString());
            string response = string.Empty;
            try
            {
                response = AppWebRequest.O.CallUsingHttpWebRequest_POST(_URL, sb.ToString(), ContentType.text_xml);
                var responseJson = ExtractJSON(response);

                if (!string.IsNullOrEmpty(responseJson))
                {
                    var _apiRes = JsonConvert.DeserializeObject<RDaddyResponse>(responseJson);
                    if (_apiRes != null)
                    {
                        if ((_apiRes.NTDRESP.STATUSCODE ?? -100) == 0)
                        {
                            mSenderLoginResponse.Statuscode = ErrorCodes.One;
                            mSenderLoginResponse.Msg = nameof(DMTErrorCodes.Beneficiary_Added_Successfully).Replace("_", " ");
                            mSenderLoginResponse.ErrorCode = DMTErrorCodes.Beneficiary_Added_Successfully;
                        }
                        else if (_apiRes.NTDRESP.STATUSCODE != null)
                        {
                            IErrorCodeML errorCodeML = new ErrorCodeML(_accessor, _env, false);
                            var eFromDB = errorCodeML.GetAPIErrorCodeDescription(request.APIGroupCode, _apiRes.NTDRESP.STATUSCODE.ToString());
                            if (!string.IsNullOrEmpty(eFromDB.Code))
                            {
                                mSenderLoginResponse.Msg = eFromDB.Error;
                                mSenderLoginResponse.ErrorCode = Convert.ToInt32(eFromDB.Code.Trim());
                            }
                        }
                        else
                        {
                            mSenderLoginResponse.Msg = nameof(DMTErrorCodes.Unbale_to_Verify_Beneficiary_Please_check_your_data_and_Verify_again).Replace("_", " ");
                            mSenderLoginResponse.ErrorCode = DMTErrorCodes.Unbale_to_Verify_Beneficiary_Please_check_your_data_and_Verify_again;
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
                Request = req,
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

            var objReq = new RemoveBeneficiaryNTDReq
            {
                REQTYPE = "DB",
                CUSTOMERNO = request.SenderMobile,
                BENEID = request.mBeneDetail.BeneID
            };
            var METHOD = "DeleteBeneficiary";
            var NTDREQ = XMLHelper.O.SerializeToXml(objReq, "NTDREQ");
            var CheckSum = HashEncryption.O.GenerateSHA512(appSetting.APIKey, NTDREQ);
            StringBuilder sb = new StringBuilder(strHelp);
            sb.Replace("{HMobileNo}", appSetting.MobileNo);
            sb.Replace("{HAPIKey}", appSetting.APIKey);
            sb.Replace("{HAgentCode}", appSetting.AgentCode);
            sb.Replace("{HChecksum}", CheckSum);
            sb.Replace("{HVersion}", appSetting.Version);
            sb.Replace("{METHOD}", METHOD);
            sb.Replace("{NTDREQ}", XMLHelper.O.EncodeForXml(NTDREQ));

            var _URL = appSetting.BASEURL;
            string response = string.Empty;
            string req = string.Format("{0}?{1}", _URL, sb.ToString());
            try
            {
                response = AppWebRequest.O.CallUsingHttpWebRequest_POST(_URL, sb.ToString(), ContentType.text_xml);
                var responseJson = ExtractJSON(response);

                if (!string.IsNullOrEmpty(responseJson))
                {
                    var _apiRes = JsonConvert.DeserializeObject<RDaddyResponse>(responseJson);
                    if (_apiRes != null)
                    {
                        if ((_apiRes.NTDRESP.STATUSCODE ?? -100) == 0)
                        {
                            senderLoginResponse.IsOTPGenerated = _apiRes.NTDRESP.OTPREQ == 1;
                            senderLoginResponse.Statuscode = ErrorCodes.One;
                            senderLoginResponse.Msg = nameof(DMTErrorCodes.OTP_for_verification_has_been_sent_successfully).Replace("_", " ");
                            senderLoginResponse.ErrorCode = DMTErrorCodes.OTP_for_verification_has_been_sent_successfully;
                        }
                        else if (_apiRes.NTDRESP.STATUSCODE != null)
                        {
                            IErrorCodeML errorCodeML = new ErrorCodeML(_accessor, _env, false);
                            var eFromDB = errorCodeML.GetAPIErrorCodeDescription(request.APIGroupCode, _apiRes.NTDRESP.STATUSCODE.ToString());
                            if (!string.IsNullOrEmpty(eFromDB.Code))
                            {
                                senderLoginResponse.Msg = eFromDB.Error;
                                senderLoginResponse.ErrorCode = Convert.ToInt32(eFromDB.Code.Trim());
                            }
                        }
                        else
                        {
                            senderLoginResponse.Msg = nameof(ErrorCodes.Unknown_Error).Replace("_", " ");
                            senderLoginResponse.ErrorCode = ErrorCodes.Unknown_Error;
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
                Request = req,
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
            var METHOD = "ValidateDeleteBeneficiaryOTP";
            var objReq = new ValidateBeneficiaryOTPNTDReq
            {
                REQTYPE = "VDBO",
                CUSTOMERNO = request.SenderMobile,
                BENEID = Convert.ToInt32(request.mBeneDetail.BeneID ?? "0"),
                OTP = request.OTP
            };
            var NTDREQ = XMLHelper.O.SerializeToXml(objReq, "NTDREQ");
            var CheckSum = HashEncryption.O.GenerateSHA512(appSetting.APIKey, NTDREQ);
            StringBuilder sb = new StringBuilder(strHelp);
            sb.Replace("{HMobileNo}", appSetting.MobileNo);
            sb.Replace("{HAPIKey}", appSetting.APIKey);
            sb.Replace("{HAgentCode}", appSetting.AgentCode);
            sb.Replace("{HChecksum}", CheckSum);
            sb.Replace("{HVersion}", appSetting.Version);
            sb.Replace("{METHOD}", METHOD);
            sb.Replace("{NTDREQ}", XMLHelper.O.EncodeForXml(NTDREQ));

            var _URL = appSetting.BASEURL;
            string req = string.Format("{0}?{1}", _URL, sb.ToString());
            string response = string.Empty;
            try
            {
                response = AppWebRequest.O.CallUsingHttpWebRequest_POST(_URL, sb.ToString(), ContentType.text_xml);
                var responseJson = ExtractJSON(response);

                if (!string.IsNullOrEmpty(responseJson))
                {
                    var _apiRes = JsonConvert.DeserializeObject<RDaddyResponse>(responseJson);
                    if (_apiRes != null)
                    {
                        if ((_apiRes.NTDRESP.STATUSCODE ?? -100) == 0)
                        {
                            mSenderLoginResponse.Statuscode = ErrorCodes.One;
                            mSenderLoginResponse.Msg = nameof(DMTErrorCodes.Beneficiary_Deactivated_or_Deleted).Replace("_", " ");
                            mSenderLoginResponse.ErrorCode = DMTErrorCodes.Beneficiary_Deactivated_or_Deleted;
                        }
                        else if (_apiRes.NTDRESP.STATUSCODE != null)
                        {
                            IErrorCodeML errorCodeML = new ErrorCodeML(_accessor, _env, false);
                            var eFromDB = errorCodeML.GetAPIErrorCodeDescription(request.APIGroupCode, _apiRes.NTDRESP.STATUSCODE.ToString());
                            if (!string.IsNullOrEmpty(eFromDB.Code))
                            {
                                mSenderLoginResponse.Msg = eFromDB.Error;
                                mSenderLoginResponse.ErrorCode = Convert.ToInt32(eFromDB.Code.Trim());
                            }
                        }
                        else
                        {
                            mSenderLoginResponse.Msg = nameof(DMTErrorCodes.Unbale_to_Verify_Beneficiary_Please_check_your_data_and_Verify_again).Replace("_", " ");
                            mSenderLoginResponse.ErrorCode = DMTErrorCodes.Unbale_to_Verify_Beneficiary_Please_check_your_data_and_Verify_again;
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
                Request = req,
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
            IBankML bankML = new BankML(_accessor, _env, false);
            var BankDetail = bankML.BankMasters(request.mBeneDetail.BankID);
            if (BankDetail.RDaddyBankID < 1)
            {
                res.Statuscode = RechargeRespType.FAILED;
                res.Msg = nameof(DMTErrorCodes.Acquiring_Bank_CBS_or_node_offline).Replace("_", " ");
                res.ErrorCode = DMTErrorCodes.Acquiring_Bank_CBS_or_node_offline;
                return res;
            }
            var objReq = new VerifyAccountNTDReq
            {
                REQTYPE = "BAV",
                CUSTOMERNO = request.SenderMobile,
                BANKID = BankDetail.RDaddyBankID,
                ACCNO = request.mBeneDetail.AccountNo,
                IFSC = request.mBeneDetail.IFSC,
                REFNO = request.TID.ToString()
            };
            var METHOD = "BeneficiaryAccountVerify";
            var NTDREQ = XMLHelper.O.SerializeToXml(objReq, "NTDREQ");
            var CheckSum = HashEncryption.O.GenerateSHA512(appSetting.APIKey, NTDREQ);
            StringBuilder sb = new StringBuilder(strHelp);
            sb.Replace("{HMobileNo}", appSetting.MobileNo);
            sb.Replace("{HAPIKey}", appSetting.APIKey);
            sb.Replace("{HAgentCode}", appSetting.AgentCode);
            sb.Replace("{HChecksum}", CheckSum);
            sb.Replace("{HVersion}", appSetting.Version);
            sb.Replace("{METHOD}", METHOD);
            sb.Replace("{NTDREQ}", XMLHelper.O.EncodeForXml(NTDREQ));

            var _URL = appSetting.BASEURL;

            string req = string.Format("{0}?{1}", _URL, sb.ToString());
            string response = string.Empty;
            try
            {
                response = AppWebRequest.O.CallUsingHttpWebRequest_POST(_URL, sb.ToString(), ContentType.text_xml);
                var responseJson = ExtractJSON(response);

                if (!string.IsNullOrEmpty(responseJson))
                {
                    var _apiRes = JsonConvert.DeserializeObject<RDaddyResponse>(responseJson);
                    if (_apiRes != null)
                    {
                        if ((_apiRes.NTDRESP.STATUSCODE ?? -100) == 0)
                        {
                            res.Statuscode = RechargeRespType.SUCCESS;
                            res.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                            res.ErrorCode = ErrorCodes.Transaction_Successful;
                            res.VendorID = _apiRes.NTDRESP.VERIFYID;
                            res.LiveID = _apiRes.NTDRESP.VERIFYID;
                            res.Balance = Convert.ToDecimal(_apiRes.NTDRESP.REMAIN ?? "0");
                            res.BeneName = _apiRes.NTDRESP.NAME;
                        }
                        else if (_apiRes.NTDRESP.STATUSCODE != null)
                        {
                            IErrorCodeML errorCodeML = new ErrorCodeML(_accessor, _env, false);
                            var eFromDB = errorCodeML.GetAPIErrorCodeDescription(request.APIGroupCode, _apiRes.NTDRESP.STATUSCODE.ToString());
                            if (!string.IsNullOrEmpty(eFromDB.Code))
                            {
                                res.Statuscode = eFromDB.Status;
                                res.Msg = eFromDB.Error.Replace("{REPLACE}", _apiRes.NTDRESP.TRNSTATUSDESC);
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
                    FuncName = "VerifyAccount",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = request.UserID
                });
            }
            new ProcUpdateLogDMRReqResp(_dal).Call(new DMTReqRes
            {
                APIID = request.APIID,
                Method = "VerifyAccount",
                RequestModeID = request.RequestMode,
                Request = req,
                Response = response,
                SenderNo = request.SenderMobile,
                UserID = request.UserID,
                TID = request.TransactionID
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
            var objReq = new SendMoneyNTDReq
            {
                REQTYPE = "SM",
                CUSTOMERNO = request.SenderMobile,
                BENEID = request.mBeneDetail.BeneID,
                AMT = request.Amount,
                TRNTYPE = request.TransMode.Equals("IMPS") ? 1 : 2,
                IMPS_SCHEDULE = 0,
                REFNO = string.Format("{0}{1}", "TID", request.TID.ToString()),
                CHN = request.RequestMode == RequestMode.APPS ? "APP" : "WEB",
                CUR = "INR",
                AG_LAT = "26.8678",
                AG_LONG = "80.9832"
            };
            var METHOD = "SendMoney";
            var NTDREQ = XMLHelper.O.SerializeToXml(objReq, "NTDREQ");
            var CheckSum = HashEncryption.O.GenerateSHA512(appSetting.APIKey, NTDREQ);
            StringBuilder sb = new StringBuilder(strHelp);
            sb.Replace("{HMobileNo}", appSetting.MobileNo);
            sb.Replace("{HAPIKey}", appSetting.APIKey);
            sb.Replace("{HAgentCode}", appSetting.AgentCode);
            sb.Replace("{HChecksum}", CheckSum);
            sb.Replace("{HVersion}", appSetting.Version);
            sb.Replace("{METHOD}", METHOD);
            sb.Replace("{NTDREQ}", XMLHelper.O.EncodeForXml(NTDREQ));

            var _URL = appSetting.BASEURL;
            string req = string.Format("{0}?{1}", _URL, sb.ToString());
            string response = string.Empty;
            try
            {
                response = AppWebRequest.O.CallUsingHttpWebRequest_POST(_URL, sb.ToString(), ContentType.text_xml);
                var responseJson = ExtractJSON(response);

                if (!string.IsNullOrEmpty(responseJson))
                {
                    var _apiRes = JsonConvert.DeserializeObject<RDaddyResponse>(responseJson);
                    if (_apiRes != null)
                    {

                        if ((_apiRes.NTDRESP.STATUSCODE ?? -100) == 0)
                        {
                            if (_apiRes.NTDRESP.TRNSTATUS != null)
                            {
                                res.LiveID = _apiRes.NTDRESP.REFNO;
                                res.VendorID = _apiRes.NTDRESP.TRNID;
                                if (_apiRes.NTDRESP.TRNSTATUS == 1)
                                {
                                    res.Statuscode = RechargeRespType.SUCCESS;
                                    res.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                                    res.ErrorCode = ErrorCodes.Transaction_Successful;
                                }
                                else if (_apiRes.NTDRESP.TRNSTATUS.In(2, 3, 5))
                                {
                                    res.Statuscode = RechargeRespType.FAILED;
                                    res.Msg = nameof(DMTErrorCodes.Transaction_Failed).Replace("_", " ");
                                    res.ErrorCode = DMTErrorCodes.Transaction_Failed;
                                    res.LiveID = _apiRes.NTDRESP.TRNSTATUSDESC;
                                    IErrorCodeML errorCodeML = new ErrorCodeML(_accessor, _env, false);
                                    var eFromDB = errorCodeML.GetAPIErrorCodeDescription(request.APIGroupCode, _apiRes.NTDRESP.TRNSTATUSDESC);
                                    if (!string.IsNullOrEmpty(eFromDB.Code))
                                    {
                                        res.Msg = eFromDB.Error.Replace("{REPLACE}", _apiRes.NTDRESP.TRNSTATUSDESC);
                                        res.ErrorCode = Convert.ToInt32(eFromDB.Code.Trim());
                                        res.LiveID = res.Msg;
                                    }
                                }
                            }
                        }
                        else if ((_apiRes.NTDRESP.STATUSCODE ?? -100) == 1046)
                        {
                            res.Statuscode = RechargeRespType.FAILED;
                            res.Msg = ErrorCodes.Down;
                            res.ErrorCode = DMTErrorCodes.Declined_by_ServiceProvider;
                            res.LiveID = res.Msg;
                        }
                        else if (_apiRes.NTDRESP.STATUSCODE != null)
                        {
                            IErrorCodeML errorCodeML = new ErrorCodeML(_accessor, _env, false);
                            var eFromDB = errorCodeML.GetAPIErrorCodeDescription(request.APIGroupCode, _apiRes.NTDRESP.STATUSCODE.ToString());
                            if (!string.IsNullOrEmpty(eFromDB.Code))
                            {
                                res.Statuscode = eFromDB.Status;
                                res.Msg = eFromDB.Error.Replace("{REPLACE}", _apiRes.NTDRESP.TRNSTATUSDESC);
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
            new ProcUpdateLogDMRReqResp(_dal).Call(new DMTReqRes
            {
                APIID = request.APIID,
                Method = "AccountTransfer",
                RequestModeID = request.RequestMode,
                Request = req,
                Response = response,
                SenderNo = request.SenderMobile,
                UserID = request.UserID,
                TID = request.TransactionID
            });
            res.Request = req;
            res.Response = response;
            return res;
        }
        public DMRTransactionResponse GetTransactionStatus(int TID, string TransactionID, int RequestMode, int UserID, int APIID, string VendorID, string APIGroupCode)
        {
            var res = new DMRTransactionResponse
            {
                Statuscode = RechargeRespType.PENDING,
                Msg = nameof(RechargeRespType.PENDING),
                ErrorCode = ErrorCodes.Request_Accpeted
            };
            var objReq = new GetTransactionStatusNTDReq
            {
                REQTYPE = "GTS",
                REFNO = string.Format("{0}{1}", "TID", TID.ToString())
            };
            var METHOD = "GetTransactionStatus";
            var NTDREQ = XMLHelper.O.SerializeToXml(objReq, "NTDREQ");
            var CheckSum = HashEncryption.O.GenerateSHA512(appSetting.APIKey, NTDREQ);
            StringBuilder sb = new StringBuilder(strHelp);
            sb.Replace("{HMobileNo}", appSetting.MobileNo);
            sb.Replace("{HAPIKey}", appSetting.APIKey);
            sb.Replace("{HAgentCode}", appSetting.AgentCode);
            sb.Replace("{HChecksum}", CheckSum);
            sb.Replace("{HVersion}", appSetting.Version);
            sb.Replace("{METHOD}", METHOD);
            sb.Replace("{NTDREQ}", XMLHelper.O.EncodeForXml(NTDREQ));

            var _URL = appSetting.BASEURL;

            string req = string.Format("{0}?{1}", _URL, sb.ToString());
            string response = string.Empty;
            try
            {
                response = AppWebRequest.O.CallUsingHttpWebRequest_POST(_URL, sb.ToString(), ContentType.text_xml);
                var responseJson = ExtractJSON(response);

                if (!string.IsNullOrEmpty(responseJson))
                {
                    var _apiRes = JsonConvert.DeserializeObject<RDaddyResponse>(responseJson);
                    if (_apiRes != null)
                    {
                        if ((_apiRes.NTDRESP.STATUSCODE ?? -100) == 0)
                        {
                            if (_apiRes.NTDRESP.TRNSTATUS != null)
                            {
                                res.LiveID = _apiRes.NTDRESP.REFNO;
                                res.VendorID = _apiRes.NTDRESP.TRNID;
                                if (_apiRes.NTDRESP.TRNSTATUS == 1)
                                {
                                    res.Statuscode = RechargeRespType.SUCCESS;
                                    res.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                                    res.ErrorCode = ErrorCodes.Transaction_Successful;
                                }
                                else if (_apiRes.NTDRESP.TRNSTATUS.In(2, 3, 5))
                                {
                                    res.Statuscode = RechargeRespType.FAILED;
                                    res.Msg = nameof(DMTErrorCodes.Transaction_Failed).Replace("_", " ");
                                    res.ErrorCode = DMTErrorCodes.Transaction_Failed;
                                    res.LiveID = _apiRes.NTDRESP.TRNSTATUSDESC;
                                    IErrorCodeML errorCodeML = new ErrorCodeML(_accessor, _env, false);
                                    var eFromDB = errorCodeML.GetAPIErrorCodeDescription(APIGroupCode, _apiRes.NTDRESP.TRNSTATUSDESC);
                                    if (!string.IsNullOrEmpty(eFromDB.Code))
                                    {
                                        res.Msg = eFromDB.Error.Replace("{REPLACE}", _apiRes.NTDRESP.TRNSTATUSDESC); ;
                                        res.ErrorCode = Convert.ToInt32(eFromDB.Code.Trim());
                                        res.LiveID = res.Msg;
                                    }
                                }
                            }
                        }
                        else if ((_apiRes.NTDRESP.STATUSCODE ?? -100) == 1046)
                        {
                            res.Statuscode = RechargeRespType.FAILED;
                            res.Msg = ErrorCodes.Down;
                            res.ErrorCode = DMTErrorCodes.Declined_by_ServiceProvider;
                            res.LiveID = res.Msg;
                        }
                        else if (_apiRes.NTDRESP.STATUSCODE != null)
                        {
                            IErrorCodeML errorCodeML = new ErrorCodeML(_accessor, _env, false);
                            var eFromDB = errorCodeML.GetAPIErrorCodeDescription(APIGroupCode, _apiRes.NTDRESP.STATUSCODE.ToString());
                            if (!string.IsNullOrEmpty(eFromDB.Code))
                            {
                                res.Statuscode = eFromDB.Status;
                                res.Msg = eFromDB.Error.Replace("{REPLACE}", _apiRes.NTDRESP.TRNSTATUSDESC); ;
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
                APIID = APIID,
                Method = "GetTransactionStatus",
                RequestModeID = RequestMode,
                Request = req,
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
        private string ExtractJSON(string response)
        {
            if ((response ?? string.Empty).Contains(">{"))
            {
                int startIndex = response.IndexOf(">{") + 1;
                int endIndex = response.IndexOf("}<") + 1;
                return response.Substring(startIndex, endIndex - startIndex);
            }
            return string.Empty;
        }

        public string InternalAgentRegistration(string FNAME, string LNAME, string MNO, string FRNM, string ADD, string CITY, string PCODE, string PAN, string ANO)
        {
            string response = string.Empty;
            var objReq = new RDaddyIntAgentRegReq
            {
                REQTYPE = "ARG",
                AGC = appSetting.MobileNo,
                FNAME = FNAME,
                LNAME = LNAME,
                MNO = MNO,
                FRNM = FRNM,
                ADD = ADD,
                CITY = CITY,
                PCODE = PCODE,
                PAN = PAN,
                ANO = ANO
            };
            var METHOD = "AgentRegistration";
            var NTDREQ = XMLHelper.O.SerializeToXml(objReq, "NTDREQ");
            var CheckSum = HashEncryption.O.GenerateSHA512(appSetting.APIKey, NTDREQ);
            StringBuilder sb = new StringBuilder(strHelp);
            sb.Replace("{HMobileNo}", appSetting.MobileNo);
            sb.Replace("{HAPIKey}", appSetting.APIKey);
            sb.Replace("{HAgentCode}", appSetting.AgentCode);
            sb.Replace("{HChecksum}", CheckSum);
            sb.Replace("{HVersion}", appSetting.Version);
            sb.Replace("{METHOD}", METHOD);
            sb.Replace("{NTDREQ}", XMLHelper.O.EncodeForXml(NTDREQ));

            var _URL = appSetting.BASEURL;

            string req = string.Format("{0}?{1}", _URL, sb.ToString());

            try
            {
                response = AppWebRequest.O.CallUsingHttpWebRequest_POST(_URL, sb.ToString(), ContentType.text_xml);
                var responseJson = ExtractJSON(response);

                if (!string.IsNullOrEmpty(responseJson))
                {
                    var _apiRes = JsonConvert.DeserializeObject<RDaddyResponse>(responseJson);
                    if (_apiRes != null)
                    {

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
                    UserId = 1
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            var dMTReq = new DMTReqRes
            {
                APIID = 0,
                Method = "InternalAgentRegistration",
                RequestModeID = 1,
                Request = req,
                Response = response,
                SenderNo = string.Empty,
                UserID = 1,
                TID = "0"
            };
            new ProcUpdateLogDMRReqResp(_dal).Call(dMTReq);
            return response;
        }
        public ResponseStatus RDaddyGetBankList(string FNAME)
        {
            string response = string.Empty;
            var objReq = new RDaddyIntAgentRegReq
            {
                REQTYPE = "GBL",
            };
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            var METHOD = "GetBankList";
            var NTDREQ = XMLHelper.O.SerializeToXml(objReq, "NTDREQ");
            var CheckSum = HashEncryption.O.GenerateSHA512(appSetting.APIKey, NTDREQ);
            StringBuilder sb = new StringBuilder(strHelp);
            sb.Replace("{HMobileNo}", appSetting.MobileNo);
            sb.Replace("{HAPIKey}", appSetting.APIKey);
            sb.Replace("{HAgentCode}", appSetting.AgentCode);
            sb.Replace("{HChecksum}", CheckSum);
            sb.Replace("{HVersion}", appSetting.Version);
            sb.Replace("{METHOD}", METHOD);
            sb.Replace("{NTDREQ}", XMLHelper.O.EncodeForXml(NTDREQ));

            var _URL = appSetting.BASEURL;

            string req = string.Format("{0}?{1}", _URL, sb.ToString());

            try
            {
                response = AppWebRequest.O.CallUsingHttpWebRequest_POST(_URL, sb.ToString(), ContentType.text_xml);
                var responseJson = ExtractJSON(response);

                if (!string.IsNullOrEmpty(responseJson))
                {
                    var _apiRes = JsonConvert.DeserializeObject<RDaddyResponse>(responseJson);
                    if (_apiRes.NTDRESP.BANKLIST != null)
                    {
                        DataTable dtBankList = new DataTable();
                        dtBankList.Columns.Add("_BANKID", typeof(int));
                        dtBankList.Columns.Add("_BANKNAME", typeof(string));
                        dtBankList.Columns.Add("_ApiCode", typeof(string));
                        dtBankList.Columns.Add("_Op1", typeof(string));
                        dtBankList.Columns.Add("_Op2", typeof(string));
                        foreach (var bank in _apiRes.NTDRESP.BANKLIST)
                        {
                            dtBankList.Rows.Add(bank.BANKID,bank.BANKNAME,"PayOneMoney", "", "");
                        }
                        if (dtBankList.Rows.Count > 0)
                        {
                            IProcedure __ = new ProcBulkBankList(_dal);
                            res = (ResponseStatus)__.Call(new BulkInsertBankList
                            {
                                tp_BankList = dtBankList
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                res.Msg = " Exception:" + ex.Message + " | " + response;
                res.Statuscode = ErrorCodes.Minus1;
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "GetTransactionStatus",
                    Error = ex.Message,
                    LoginTypeID = 1,
                    UserId = 1
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }
    }
}
