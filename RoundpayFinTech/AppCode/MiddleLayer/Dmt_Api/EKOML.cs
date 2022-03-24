using Fintech.AppCode.Configuration;
using Fintech.AppCode.DB;
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
using RoundpayFinTech.AppCode.ThirdParty.Eko;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Validators;

namespace RoundpayFinTech.AppCode.MiddleLayer.Dmt_Api
{
    public partial class EKOML
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        private readonly IConfiguration Configuration;
        private readonly IDAL _dal;
        private readonly IConnectionConfiguration _c;
        private readonly EKOAppSetting _eKOAppSetting;
        private readonly int _APIID;
        public EKOML(IHttpContextAccessor accessor, IHostingEnvironment env, int APIID)
        {
            _accessor = accessor;
            _env = env;
            var builder = new ConfigurationBuilder()
                 .SetBasePath(Directory.GetCurrentDirectory());
            builder.AddJsonFile((_env.IsProduction() ? "appsettings.json" : "appsettings.Development.json"));
            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
            _c = new ConnectionConfiguration(_accessor, _env);
            _dal = new DAL(_c.GetConnectionString());
            _eKOAppSetting = EKOAppSetting();
            _APIID = APIID;
        }
        private EKOAppSetting EKOAppSetting()
        {
            var setting = new EKOAppSetting();
            try
            {
                setting = new EKOAppSetting
                {
                    EKOKey = Configuration["DMR:EKO:EKOKey"],
                    InitiatorKey = Configuration["DMR:EKO:InitiatorKey"],
                    DeveloperKey = Configuration["DMR:EKO:DeveloperKey"],
                    BaseURL = Configuration["DMR:EKO:BaseURL"]
                };
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "EKOAppSetting",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return setting;
        }
        #region EKO
        public async Task<ResponseStatus> GetCustomerInformation(DMTReq _req)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.One,
                Msg = nameof(ErrorCodes.Unknown_Error).Replace("_", " "),
                ErrorCode = ErrorCodes.Unknown_Error
            };
            var eKORequest = new EKORequest
            {
                customer_id = _req.SenderNO
            };
            string ekoTimeStamp = EKOAPIHelper.O.GetSecretKeyTimeStamp();
            string URL = _eKOAppSetting.BaseURL + "customers/mobile_number:" + eKORequest.customer_id + "?initiator_id=" + _eKOAppSetting.InitiatorKey;
            var headers = new Dictionary<string, string>
            {
                { EKOAPIHelper.SecretKeyTimeStamp, ekoTimeStamp },
                { EKOAPIHelper.SecretKey, EKOAPIHelper.O.GetSecretKey(ekoTimeStamp,_eKOAppSetting.EKOKey) },
                { EKOAPIHelper.DeveloperKey, _eKOAppSetting.DeveloperKey }
            };

            string resp = string.Empty;
            try
            {
                resp = await AppWebRequest.O.CallUsingHttpWebRequest_GETAsync(URL, headers);
                var ekoResp = JsonConvert.DeserializeObject<EKOClassess>(resp);
                if (ekoResp == null)
                {
                    res.Msg = nameof(DMTErrorCodes.Sender_Not_Found).Replace("_", " ");
                    res.CommonInt = ErrorCodes.One;
                    res.ErrorCode = DMTErrorCodes.Sender_Not_Found;
                }
                else if (ekoResp.status == null)
                {
                    res.Msg = nameof(DMTErrorCodes.Sender_Not_Found).Replace("_", " ");
                    res.CommonInt = ErrorCodes.One;
                    res.ErrorCode = DMTErrorCodes.Sender_Not_Found;
                }
                else if (ekoResp.status == 463 || ekoResp.response_type_id == -1)
                {
                    res.Msg = nameof(DMTErrorCodes.Sender_Not_Found).Replace("_", " ");
                    res.CommonInt = ErrorCodes.One;
                    res.ErrorCode = DMTErrorCodes.Sender_Not_Found;
                }
                else if (ekoResp.status == 0 && ekoResp.response_type_id.In(33, 37,332))
                {
                    if (ekoResp.response_type_id == 37)
                    {
                        res.Statuscode = ErrorCodes.Minus1;
                        res.Msg = nameof(DMTErrorCodes.Partial_Registration_Please_verify_sender_with_OTP).Replace("_", " ");
                        res.CommonInt = ErrorCodes.One;
                        res.ErrorCode = DMTErrorCodes.Partial_Registration_Please_verify_sender_with_OTP;
                    }
                    else
                    {
                        res.Msg = nameof(DMTErrorCodes.Detail_Found_Successfully).Replace("_", " ");
                        res.ErrorCode = DMTErrorCodes.Detail_Found_Successfully;
                        res.CommonStr = ekoResp.data.available_limit.ToString();
                        res.CommonStr2 = ekoResp.data.mobile;
                        if (ekoResp.data.state.Equals("2") || ekoResp.data.state.Equals("1"))
                        {
                            res.Status = SenderKYCStatus.NOTREGISTRED;
                        }
                        else if (ekoResp.data.state.Equals("4"))
                        {
                            res.Status = SenderKYCStatus.ACTIVE;
                        }
                        else if (ekoResp.data.state_desc.ToUpper().Equals("REJECTED"))
                        {
                            res.Status = SenderKYCStatus.REJECTED;
                        }
                        else
                        {
                            res.Status = SenderKYCStatus.APPLIED;
                        }
                    }
                }
                else
                {
                    res.Msg = nameof(DMTErrorCodes.Sender_Not_Found).Replace("_", " ");
                    res.CommonInt = ErrorCodes.One;
                    res.ErrorCode = DMTErrorCodes.Sender_Not_Found;
                }
            }
            catch (Exception ex)
            {
                resp = " Exception:" + ex.Message + " | " + resp;
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "GetCustomerInformation",
                    Error = ex.Message,
                    LoginTypeID = _req.LT,
                    UserId = _req.UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }

            var dMTReq = new DMTReqRes
            {
                APIID = _APIID,
                Method = "GetCustomerInformation",
                RequestModeID = _req.RequestMode,
                Request = URL + JsonConvert.SerializeObject(eKORequest) + JsonConvert.SerializeObject(headers),
                Response = resp,
                SenderNo = _req.SenderNO,
                UserID = _req.UserID,
                TID = _req.TID
            };
            new ProcUpdateLogDMRReqResp(_dal).Call(dMTReq);
            return res;
        }

        public async Task<ResponseStatus> CreateCustomer(CreateSen _req)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = nameof(ErrorCodes.Unknown_Error).Replace("_", " "),
                ErrorCode = ErrorCodes.Unknown_Error
            };
            var eKORequest = new EKORequest
            {
                customer_id = _req.senderRequest.MobileNo ?? string.Empty,
                name = _req.senderRequest.Name ?? string.Empty
            };
            string ekoTimeStamp = EKOAPIHelper.O.GetSecretKeyTimeStamp();
            string URL = _eKOAppSetting.BaseURL + "customers/mobile_number:" + eKORequest.customer_id;
            var headers = new Dictionary<string, string>
            {
                { EKOAPIHelper.SecretKeyTimeStamp, ekoTimeStamp },
                { EKOAPIHelper.SecretKey, EKOAPIHelper.O.GetSecretKey(ekoTimeStamp,_eKOAppSetting.EKOKey) },
                { ContentType.Self,ContentType.x_wwww_from_urlencoded},
                { EKOAPIHelper.DeveloperKey, _eKOAppSetting.DeveloperKey }
            };
            string PutData = "initiator_id=" + (_eKOAppSetting.InitiatorKey ?? string.Empty) + "&name=" + eKORequest.name;

            string resp = string.Empty;
            var ekoResp = new EKOClassess();
            try
            {
                resp = await AppWebRequest.O.HWRPUTAsync(URL, PutData, headers);
                ekoResp = JsonConvert.DeserializeObject<EKOClassess>(resp);
                if (ekoResp != null)
                {
                    if (ekoResp.status != null)
                    {
                        if (ekoResp.status == 17 && ekoResp.response_type_id == -1)
                        {
                            res.Msg = nameof(DMTErrorCodes.Sender_Already_Exist).Replace("_", " ");
                            res.ErrorCode = DMTErrorCodes.Sender_Already_Exist;
                        }
                        else if (ekoResp.status == 0)
                        {
                            res.Statuscode = ErrorCodes.One;
                            res.Msg = nameof(DMTErrorCodes.Partial_Registration_Please_verify_sender_with_OTP).Replace("_", " ");
                            res.CommonInt = ErrorCodes.One;
                            res.ErrorCode = DMTErrorCodes.Partial_Registration_Please_verify_sender_with_OTP;
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
                    FuncName = "CreateCustomer",
                    Error = ex.Message,
                    LoginTypeID = 1,
                    UserId = 1
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            var dMTReq = new DMTReqRes
            {
                APIID = _APIID,
                Method = "CreateCustomer",
                RequestModeID = _req.dMTReq.RequestMode,
                Request = URL + JsonConvert.SerializeObject(eKORequest) + JsonConvert.SerializeObject(headers) + "?" + PutData,
                Response = resp,
                SenderNo = _req.senderRequest.MobileNo,
                UserID = _req.dMTReq.UserID,
                TID = _req.dMTReq.TID
            };
            new ProcUpdateLogDMRReqResp(_dal).Call(dMTReq);
            return res;
        }

        public async Task<ResponseStatus> CreateCustomerforKYC(CreateSen _req, string aadhar1, string aadhar2, string photo, string panphoto)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = nameof(ErrorCodes.Unknown_Error).Replace("_", " "),
                ErrorCode = ErrorCodes.Unknown_Error
            };
            await Task.Delay(0);
            var eKORequest = new EKORequest
            {
                customer_id = _req.senderRequest.MobileNo ?? string.Empty,
                name = _req.senderRequest.NameOnKYC ?? string.Empty,
                id_proof_type_id = 15,
                id_proof = _req.senderRequest.AadharNo,
                ovd_type_id = 4,
                ovd_number = _req.senderRequest.PANNo,
                file1 = aadhar1,
                file2 = aadhar2,
                customer_photo = photo,
                ovd_image = panphoto
            };
            string ekoTimeStamp = EKOAPIHelper.O.GetSecretKeyTimeStamp();
            var URL = _eKOAppSetting.BaseURL + "customers/mobile_number:" + eKORequest.customer_id;

            var headers = new Dictionary<string, string>
            {
                { EKOAPIHelper.SecretKeyTimeStamp, ekoTimeStamp },
                { EKOAPIHelper.SecretKey, EKOAPIHelper.O.GetSecretKey(ekoTimeStamp,_eKOAppSetting.EKOKey) },
                { EKOAPIHelper.DeveloperKey, _eKOAppSetting.DeveloperKey }
            };
            var frmData = new StringBuilder();
            //initiator_id=9910028267&id_proof_type_id=15&id_proof=123456789117&name=Test&ovd_type_id=4&ovd_number=AOYPM2624w&file1&file2&customer_photo
            frmData.Append("initiator_id=");
            frmData.Append(_eKOAppSetting.InitiatorKey ?? string.Empty);
            frmData.Append("&id_proof_type_id=");
            frmData.Append(eKORequest.id_proof_type_id);
            frmData.Append("&id_proof=");
            frmData.Append(eKORequest.id_proof ?? string.Empty);
            frmData.Append("&name=");
            frmData.Append(eKORequest.name ?? string.Empty);
            frmData.Append("&ovd_type_id=");
            frmData.Append(eKORequest.ovd_type_id);
            frmData.Append("&ovd_number=");
            frmData.Append(eKORequest.ovd_number ?? string.Empty);
            frmData.Append("&file1&file2&customer_photo&ovd_image");

            var FormData = new Dictionary<string, string>
            {
                {"form-data", frmData.ToString()}
            };
            var files = new Dictionary<string, string> {
                { nameof(eKORequest.file1),eKORequest.file1},
                { nameof(eKORequest.file2),eKORequest.file2},
                {nameof(eKORequest.customer_photo),eKORequest.customer_photo},
                {nameof(eKORequest.ovd_image),eKORequest.ovd_image}
            };
            string resp = string.Empty;
            var ekoResp = new EKOClassess();
            try
            {
                resp = AppWebRequest.O.WebClientPutFiles(URL, FormData, headers, files);
                ekoResp = JsonConvert.DeserializeObject<EKOClassess>(resp);
                if (ekoResp != null)
                {
                    if (ekoResp.status != null)
                    {
                        if (ekoResp.status == 0 && ekoResp.response_type_id == 214)
                        {
                            res.Statuscode = ErrorCodes.One;
                            res.Msg = "KYC Uploaded successfully!";
                            res.ErrorCode = DMTErrorCodes.Sender_Created_Successfully;
                        }
                        else if (ekoResp.status.In(700, 1237))
                        {
                            res.Msg = ekoResp.message;
                        }
                        else
                        {
                            res.Msg = "KYC could not be updated!";
                        }
                    }
                    else
                    {
                        res.Msg = "KYC could not be updated. Try after sometime or KYC not required at this moment.";
                    }
                }
            }
            catch (Exception ex)
            {
                resp = " Exception:" + ex.Message + " | " + resp;
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "CreateCustomerforKYC",
                    Error = ex.Message,
                    LoginTypeID = 1,
                    UserId = 1
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            var dMTReq = new DMTReqRes
            {
                APIID = _APIID,
                Method = "CreateCustomerforKYC",
                RequestModeID = _req.dMTReq.RequestMode,
                Request = URL + JsonConvert.SerializeObject(eKORequest) + JsonConvert.SerializeObject(headers) + "?" + JsonConvert.SerializeObject(FormData) + JsonConvert.SerializeObject(files),
                Response = resp,
                SenderNo = _req.senderRequest.MobileNo,
                UserID = _req.dMTReq.UserID,
                TID = _req.dMTReq.TID
            };
            new ProcUpdateLogDMRReqResp(_dal).Call(dMTReq);
            return res;
        }

        public async Task<ResponseStatus> VerifyCustomerIdentity(DMTReq _req, string OTP)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = nameof(ErrorCodes.Unknown_Error).Replace("_", " "),
                ErrorCode = ErrorCodes.Unknown_Error
            };
            var eKORequest = new EKORequest
            {
                customer_id = _req.SenderNO,
                otp = OTP
            };
            string ekoTimeStamp = EKOAPIHelper.O.GetSecretKeyTimeStamp();
            string URL = _eKOAppSetting.BaseURL + "customers/verification/otp:" + eKORequest.otp;
            var headers = new Dictionary<string, string>
            {
                { EKOAPIHelper.SecretKeyTimeStamp, ekoTimeStamp },
                { EKOAPIHelper.SecretKey, EKOAPIHelper.O.GetSecretKey(ekoTimeStamp,_eKOAppSetting.EKOKey) },
                { ContentType.Self,ContentType.x_wwww_from_urlencoded},
                { EKOAPIHelper.DeveloperKey, _eKOAppSetting.DeveloperKey }
            };
            string PutData = "initiator_id=" + (_eKOAppSetting.InitiatorKey ?? string.Empty) + "&id_type=mobile_number&id=" + eKORequest.customer_id;

            string resp = string.Empty;
            try
            {
                resp = await AppWebRequest.O.HWRPUTAsync(URL, PutData, headers);
                var ekoResp = JsonConvert.DeserializeObject<EKOClassess>(resp);
                if (ekoResp != null)
                {
                    if (ekoResp.status != null)
                    {
                        if (ekoResp.status.In(100, 302) && ekoResp.response_type_id == -1)
                        {
                            res.Msg = nameof(ErrorCodes.Invalid_OTP).Replace("_", " ");
                            res.ErrorCode = ErrorCodes.Invalid_OTP;
                        }
                        else if (ekoResp.status == 303 && ekoResp.response_type_id == -1)
                        {
                            res.Msg = nameof(DMTErrorCodes.OTP_Expired).Replace("_", " ");
                            res.ErrorCode = DMTErrorCodes.OTP_Expired;
                        }
                        else if (ekoResp.status == 463 && ekoResp.response_type_id == -1)
                        {
                            res.Msg = nameof(DMTErrorCodes.Sender_Not_Found).Replace("_", " ");
                            res.ErrorCode = DMTErrorCodes.Sender_Not_Found;
                        }

                        else if (ekoResp.status == 0 && ekoResp.response_type_id.In(300, 301))
                        {
                            res.Statuscode = ErrorCodes.One;
                            res.Msg = nameof(DMTErrorCodes.Sender_Created_Successfully).Replace("_", " ");
                            res.ErrorCode = DMTErrorCodes.Sender_Created_Successfully;
                        }
                        else
                        {
                            res.Msg = nameof(ErrorCodes.Invalid_OTP).Replace("_", " ");
                            res.ErrorCode = ErrorCodes.Invalid_OTP;
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
                    FuncName = "VerifyCustomerIdentity",
                    Error = ex.Message,
                    LoginTypeID = _req.LT,
                    UserId = _req.UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            var dMTReq = new DMTReqRes
            {
                APIID = _APIID,
                Method = "VerifyCustomerIdentity",
                RequestModeID = _req.RequestMode,
                Request = URL + JsonConvert.SerializeObject(eKORequest) + JsonConvert.SerializeObject(headers) + "?" + PutData,
                Response = resp,
                SenderNo = _req.SenderNO,
                UserID = _req.UserID,
                TID = _req.TID
            };
            new ProcUpdateLogDMRReqResp(_dal).Call(dMTReq);
            return res;
        }

        public async Task<ResponseStatus> ResendOTP(DMTReq _req)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = nameof(ErrorCodes.Unknown_Error).Replace("_", " "),
                ErrorCode = ErrorCodes.Unknown_Error
            };
            var eKORequest = new EKORequest
            {
                customer_id = _req.SenderNO
            };
            string ekoTimeStamp = EKOAPIHelper.O.GetSecretKeyTimeStamp();
            string URL = _eKOAppSetting.BaseURL + "customers/mobile_number:" + eKORequest.customer_id + "/otp";

            var headers = new Dictionary<string, string>
            {
                { ContentType.Self,ContentType.x_wwww_from_urlencoded},
                { EKOAPIHelper.SecretKeyTimeStamp, ekoTimeStamp },
                { EKOAPIHelper.SecretKey, EKOAPIHelper.O.GetSecretKey(ekoTimeStamp,_eKOAppSetting.EKOKey) },
                { EKOAPIHelper.DeveloperKey, _eKOAppSetting.DeveloperKey }
            };
            string PostData = "initiator_id=" + (_eKOAppSetting.InitiatorKey ?? string.Empty);

            string resp = string.Empty;
            try
            {
                resp = await AppWebRequest.O.HWRPostAsync(URL, PostData, headers);
                var ekoResp = JsonConvert.DeserializeObject<EKOClassess>(resp);
                if (ekoResp != null)
                {
                    if (ekoResp.status != null)
                    {
                        if (ekoResp.status == 463 && ekoResp.response_type_id == -1)
                        {
                            res.Msg = nameof(DMTErrorCodes.Sender_Not_Found).Replace("_", " ");
                            res.ErrorCode = DMTErrorCodes.Sender_Not_Found;
                        }
                        else if (ekoResp.status == 0 && ekoResp.response_type_id == 322)
                        {
                            res.Msg = nameof(DMTErrorCodes.Sender_Already_Exist).Replace("_", " ");
                            res.ErrorCode = DMTErrorCodes.Sender_Already_Exist;
                        }
                        else if (ekoResp.status == 0 && ekoResp.response_type_id == 321)
                        {
                            res.Statuscode = ErrorCodes.One;
                            res.Msg = nameof(DMTErrorCodes.OTP_for_verification_has_been_sent_successfully).Replace("_", " ");
                            res.ErrorCode = DMTErrorCodes.OTP_for_verification_has_been_sent_successfully;
                            res.CommonInt = ErrorCodes.One;
                        }
                        else
                        {
                            res.Msg = nameof(DMTErrorCodes.Sender_Not_Found).Replace("_", " ");
                            res.ErrorCode = DMTErrorCodes.Sender_Not_Found;
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
                    FuncName = "ResndOTP",
                    Error = ex.Message,
                    LoginTypeID = _req.LT,
                    UserId = _req.UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            var dMTReq = new DMTReqRes
            {
                APIID = _APIID,
                Method = "ResendOTP",
                RequestModeID = _req.RequestMode,
                Request = URL + JsonConvert.SerializeObject(eKORequest) + JsonConvert.SerializeObject(headers) + "?" + PostData,
                Response = resp,
                SenderNo = _req.SenderNO,
                UserID = _req.UserID,
                TID = _req.TID
            };
            new ProcUpdateLogDMRReqResp(_dal).Call(dMTReq);
            return res;
        }

        public async Task<BeniRespones> GetListofRecipients(DMTReq _req)
        {
            var res = new BeniRespones
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = nameof(DMTErrorCodes.Beneficiary_not_found).Replace("_", " "),
                ErrorCode = DMTErrorCodes.Beneficiary_not_found
            };
            var eKORequest = new EKORequest
            {
                customer_id = _req.SenderNO
            };
            string ekoTimeStamp = EKOAPIHelper.O.GetSecretKeyTimeStamp();
            string URL = _eKOAppSetting.BaseURL + "customers/mobile_number:" + eKORequest.customer_id + "/recipients?initiator_id=" + _eKOAppSetting.InitiatorKey;

            var headers = new Dictionary<string, string>
            {
                { EKOAPIHelper.SecretKeyTimeStamp, ekoTimeStamp },
                { EKOAPIHelper.SecretKey, EKOAPIHelper.O.GetSecretKey(ekoTimeStamp,_eKOAppSetting.EKOKey) },
                { EKOAPIHelper.DeveloperKey, _eKOAppSetting.DeveloperKey }
            };

            string resp = "";
            try
            {
                resp = await AppWebRequest.O.CallUsingHttpWebRequest_GETAsync(URL, headers);
                var ekoBenes = JsonConvert.DeserializeObject<EKOClassess>(resp);
                if (ekoBenes != null)
                {
                    if (ekoBenes.status != null)
                    {
                        if (ekoBenes.status == 0 && ekoBenes.response_type_id == 23 && ekoBenes.data.recipient_list != null)
                        {
                            res.Statuscode = ErrorCodes.One;
                            res.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                            res.ErrorCode = ErrorCodes.Transaction_Successful;

                            var ListBeni = new List<AddBeni>();
                            foreach (var item in ekoBenes.data.recipient_list)
                            {
                                var addBeni = new AddBeni
                                {
                                    AccountNo = item.account,
                                    BankName = item.bank,
                                    IFSC = item.ifsc,
                                    BeneName = item.recipient_name,
                                    MobileNo = item.bank,
                                    BeneID = item.recipient_id.ToString()
                                };
                                ListBeni.Add(addBeni);
                            }
                            res.addBeni = ListBeni;
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
                    FuncName = "GetListofRecipients",
                    Error = ex.Message,
                    LoginTypeID = _req.LT,
                    UserId = _req.UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            var dMTReq = new DMTReqRes
            {
                APIID = _APIID,
                Method = "GetListofRecipients",
                RequestModeID = _req.RequestMode,
                Request = URL + JsonConvert.SerializeObject(eKORequest) + JsonConvert.SerializeObject(headers),
                Response = resp,
                SenderNo = _req.SenderNO,
                UserID = _req.UserID,
                TID = _req.TID
            };
            new ProcUpdateLogDMRReqResp(_dal).Call(dMTReq);
            return res;
        }

        public Task<EKOClassess> GetRecipientDetails(EKORequest eKORequest)
        {
            throw new NotImplementedException();
        }

        public async Task<ResponseStatus> AddRecipient(AddBeni addBeni, DMTReq _req)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = nameof(ErrorCodes.Unknown_Error).Replace("_", " "),
                ErrorCode = ErrorCodes.Unknown_Error
            };
            IBankML bankML = new BankML(_accessor, _env, false);
            var BankDetail = bankML.BankMasters(addBeni.BankID);
            var eKORequest = new EKORequest
            {
                customer_id = _req.SenderNO,
                recipient_id_type = "acc_ifsc",
                //id = addBeni.AccountNo + "_" + ((BankDetail.IFSC ?? string.Empty) == string.Empty ? (addBeni.IFSC ?? string.Empty) : BankDetail.IFSC),
                id = addBeni.AccountNo + "_" + (addBeni.IFSC ?? string.Empty),
                recipient_type = "3",
                recipient_name = addBeni.BeneName,
                recipient_mobile = addBeni.MobileNo,
                bank_id = BankDetail.EKO_BankID + string.Empty
            };
            string ekoTimeStamp = EKOAPIHelper.O.GetSecretKeyTimeStamp();
            string URL = _eKOAppSetting.BaseURL + "customers/mobile_number:" + eKORequest.customer_id + "/recipients/" + eKORequest.recipient_id_type + ":" + eKORequest.id;
            var headers = new Dictionary<string, string>
            {
                { EKOAPIHelper.SecretKeyTimeStamp, ekoTimeStamp },
                { EKOAPIHelper.SecretKey, EKOAPIHelper.O.GetSecretKey(ekoTimeStamp,_eKOAppSetting.EKOKey) },
                { ContentType.Self,ContentType.x_wwww_from_urlencoded},
                { EKOAPIHelper.DeveloperKey, _eKOAppSetting.DeveloperKey }
            };
            string PutData = "recipient_type=" + eKORequest.recipient_type + "&recipient_name=" + eKORequest.recipient_name + "&recipient_mobile=" + eKORequest.recipient_mobile + "&initiator_id=" + (_eKOAppSetting.InitiatorKey ?? string.Empty) + "&bank_id=" + eKORequest.bank_id;

            string resp = string.Empty;
            try
            {
                resp = await AppWebRequest.O.HWRPUTAsync(URL, PutData, headers);
                var ekoResp = JsonConvert.DeserializeObject<EKOClassess>(resp);
                if (ekoResp != null)
                {
                    if (ekoResp.status != null)
                    {
                        if (ekoResp.status == 39 && ekoResp.response_type_id == -1)
                        {
                            res.Msg = nameof(DMTErrorCodes.Beneficiarys_Monthly_Or_Daily_Limit_Exceed).Replace("_", " ");
                            res.ErrorCode = DMTErrorCodes.Beneficiarys_Monthly_Or_Daily_Limit_Exceed;
                        }
                        else if (ekoResp.status == 0 && ekoResp.response_type_id.In(342, 145))
                        {
                            res.Msg = nameof(DMTErrorCodes.Beneficiary_Already_Exist).Replace("_", " ");
                            res.ErrorCode = DMTErrorCodes.Beneficiary_Already_Exist;
                        }
                        else if (ekoResp.status == 0 && ekoResp.response_type_id == 43)
                        {
                            res.Statuscode = ErrorCodes.One;
                            res.Msg = nameof(DMTErrorCodes.Beneficiary_Added_Successfully).Replace("_", " ");
                            res.ErrorCode = DMTErrorCodes.Beneficiary_Added_Successfully;
                        }
                        else if (ekoResp.status == 131)
                        {
                            res.Msg = nameof(DMTErrorCodes.Invalid_Name).Replace("_", " ");
                            res.ErrorCode = DMTErrorCodes.Invalid_Name;
                        }
                        else if (ekoResp.status == 45 && ekoResp.response_status_id == 1)
                        {
                            res.Msg = nameof(DMTErrorCodes.Please_enter_correct_IFSC_code).Replace("_", " ");
                            res.ErrorCode = DMTErrorCodes.Please_enter_correct_IFSC_code;
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
                    FuncName = "AddRecipient",
                    Error = ex.Message,
                    LoginTypeID = _req.LT,
                    UserId = _req.UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            var dMTReq = new DMTReqRes
            {
                APIID = _APIID,
                Method = "AddRecipient",
                RequestModeID = _req.RequestMode,
                Request = URL + JsonConvert.SerializeObject(eKORequest) + JsonConvert.SerializeObject(headers) + "?" + PutData,
                Response = resp,
                SenderNo = _req.SenderNO,
                UserID = _req.UserID,
                TID = _req.TID
            };
            new ProcUpdateLogDMRReqResp(_dal).Call(dMTReq);
            return res;
        }

        public async Task<ResponseStatus> RemoveRecipient(DMTReq _req, string BeniID)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = nameof(ErrorCodes.Unknown_Error).Replace("_", " "),
                ErrorCode = ErrorCodes.Unknown_Error
            };
            var eKORequest = new EKORequest
            {
                customer_id = _req.SenderNO,
                recipient_id = BeniID
            };
            string ekoTimeStamp = EKOAPIHelper.O.GetSecretKeyTimeStamp();
            string URL = _eKOAppSetting.BaseURL + "customers/mobile_number:" + eKORequest.customer_id + "/recipients/recipient_id:" + eKORequest.recipient_id;
            var headers = new Dictionary<string, string>
            {
                { EKOAPIHelper.SecretKeyTimeStamp, ekoTimeStamp },
                { EKOAPIHelper.SecretKey, EKOAPIHelper.O.GetSecretKey(ekoTimeStamp,_eKOAppSetting.EKOKey) },
                { ContentType.Self,ContentType.x_wwww_from_urlencoded},
                { EKOAPIHelper.DeveloperKey, _eKOAppSetting.DeveloperKey }
            };
            string PutData = "initiator_id=" + (_eKOAppSetting.InitiatorKey ?? string.Empty);

            string resp = "";
            try
            {
                resp = await AppWebRequest.O.HWRDELETEAsync(URL, PutData, headers);
                var ekoResp = JsonConvert.DeserializeObject<EKOClassess>(resp);
                if (ekoResp != null)
                {
                    if (ekoResp.status != null)
                    {
                        if (ekoResp.status == 29 && ekoResp.response_type_id == 29)
                        {
                            res.Msg = nameof(DMTErrorCodes.Invalid_Beneficiary_ID).Replace("_", " ");
                            res.ErrorCode = DMTErrorCodes.Invalid_Beneficiary_ID;
                        }
                        if (ekoResp.status == 138 && ekoResp.response_type_id == -1)
                        {
                            res.Msg = nameof(DMTErrorCodes.Invalid_Beneficiary_ID).Replace("_", " ");
                            res.ErrorCode = DMTErrorCodes.Invalid_Beneficiary_ID;
                        }
                        if (ekoResp.status == 0 && ekoResp.response_type_id == 27)
                        {
                            res.Statuscode = ErrorCodes.One;
                            res.Msg = nameof(DMTErrorCodes.Beneficiary_Deactivated_or_Deleted).Replace("_", " ");
                            res.ErrorCode = DMTErrorCodes.Invalid_Beneficiary_ID;
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
                    FuncName = "RemoveRecipient",
                    Error = ex.Message,
                    LoginTypeID = _req.LT,
                    UserId = _req.UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            var dMTReq = new DMTReqRes
            {
                APIID = _APIID,
                Method = "RemoveRecipient",
                RequestModeID = _req.RequestMode,
                Request = URL + JsonConvert.SerializeObject(eKORequest) + JsonConvert.SerializeObject(headers) + "?" + PutData,
                Response = resp,
                SenderNo = _req.SenderNO,
                UserID = _req.UserID,
                TID = _req.TID
            };
            new ProcUpdateLogDMRReqResp(_dal).Call(dMTReq);
            return res;
        }

        public async Task<DMRTransactionResponse> AccountVerification(DMTReq _req, ReqSendMoney sendMoney, DMRTransactionResponse res)
        {
            res.Statuscode = RechargeRespType.PENDING;
            res.Msg = RechargeRespType._PENDING;
            res.VendorID = string.Empty;
            res.LiveID = string.Empty;
            //IBankML bankML = new BankML(_accessor, _env, false);
            //var BankDetail = bankML.BankMasters(sendMoney.BankID);
            var eKORequest = new EKORequest
            {
                customer_id = _req.SenderNO,
                account = sendMoney.AccountNo,
                client_ref_id = _req.TID,
                ifsc = sendMoney.IFSC
            };
            //EKORequest eKORequest, bool IsByBankCode
            string ekoTimeStamp = EKOAPIHelper.O.GetSecretKeyTimeStamp();
            string URL = string.Empty;
            bool IsByBankCode = false;//Currently Verification with IFSC
            if (IsByBankCode)
                URL = _eKOAppSetting.BaseURL + "banks/bank_code:" + eKORequest.bank_code + "/accounts/" + eKORequest.account;
            else
                URL = _eKOAppSetting.BaseURL + "banks/ifsc:" + eKORequest.ifsc + "/accounts/" + eKORequest.account;

            var headers = new Dictionary<string, string>
            {
                { EKOAPIHelper.SecretKeyTimeStamp, ekoTimeStamp },
                { EKOAPIHelper.SecretKey, EKOAPIHelper.O.GetSecretKey(ekoTimeStamp,_eKOAppSetting.EKOKey) },
                { ContentType.Self,ContentType.x_wwww_from_urlencoded},
                { EKOAPIHelper.DeveloperKey, _eKOAppSetting.DeveloperKey }
            };
            string PostData = "initiator_id=" + (_eKOAppSetting.InitiatorKey ?? string.Empty) + "&customer_id=" + eKORequest.customer_id + "&client_ref_id=" + eKORequest.client_ref_id;

            string resp = string.Empty;
            try
            {
                resp = await AppWebRequest.O.HWRPostAsync(URL, PostData, headers);

                var ekoResp = JsonConvert.DeserializeObject<EKOClassess>(resp);
                if (ekoResp != null)
                {
                    if (ekoResp.status != null)
                    {
                        if (ekoResp.status == 463 && ekoResp.response_type_id == -1 || ekoResp.status.In(132, 319))
                        {
                            res.Statuscode = RechargeRespType.FAILED;
                            res.Msg = nameof(DMTErrorCodes.Sender_Not_Found).Replace("_", " ");
                            res.ErrorCode = DMTErrorCodes.Sender_Not_Found;
                            res.LiveID = res.Msg;
                        }
                        else if (ekoResp.status == 31)
                        {
                            res.Statuscode = RechargeRespType.FAILED;
                            res.Msg = nameof(DMTErrorCodes.Invalid_Account_Number).Replace("_", " ");
                            res.ErrorCode = DMTErrorCodes.Invalid_Account_Number;
                            res.LiveID = res.Msg;
                        }
                        else if (ekoResp.status == 39 && ekoResp.response_type_id == -1 || ekoResp.status == 314)
                        {
                            res.Statuscode = RechargeRespType.FAILED;
                            res.Msg = nameof(DMTErrorCodes.You_can_not_add_more_beneficiary).Replace("_", " ");
                            res.ErrorCode = DMTErrorCodes.You_can_not_add_more_beneficiary;
                            res.LiveID = res.Msg;
                        }
                        else if (ekoResp.status == 0 && ekoResp.response_type_id == 345)
                        {
                            res.Statuscode = RechargeRespType.FAILED;
                            res.Msg = nameof(DMTErrorCodes.Beneficiary_Already_Exist).Replace("_", " ");
                            res.ErrorCode = DMTErrorCodes.Beneficiary_Already_Exist;
                            res.LiveID = res.Msg;
                        }
                        else if (ekoResp.status == 350 || (ekoResp.status == 0 && ekoResp.response_type_id == 22))
                        {
                            res.Statuscode = RechargeRespType.FAILED;
                            res.Msg = nameof(DMTErrorCodes.Unbale_to_Verify_Beneficiary_Please_check_your_data_and_Verify_again).Replace("_", " ");
                            res.ErrorCode = DMTErrorCodes.Unbale_to_Verify_Beneficiary_Please_check_your_data_and_Verify_again;
                            res.LiveID = res.Msg;
                        }
                        else if (ekoResp.status == 0 && ekoResp.response_type_id == 61)
                        {
                            res.Statuscode = RechargeRespType.SUCCESS;
                            res.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                            res.ErrorCode = ErrorCodes.Transaction_Successful;
                            res.VendorID = ekoResp.data.tid ?? string.Empty;
                            res.BeneName = ekoResp.data.recipient_name ?? string.Empty;
                            res.LiveID = ekoResp.data.account ?? string.Empty;
                        }
                        else if (ekoResp.status == 44 && ekoResp.response_type_id == 44 || (ekoResp.status.In(41, 45, 136, 508, 536, 537) || (ekoResp.status == 541 && ekoResp.response_type_id == -1)))
                        {
                            res.Statuscode = RechargeRespType.FAILED;
                            res.Msg = nameof(DMTErrorCodes.Please_enter_correct_IFSC_code).Replace("_", " ");
                            res.ErrorCode = DMTErrorCodes.Please_enter_correct_IFSC_code;
                            res.LiveID = res.Msg;
                        }
                        else if (ekoResp.status == 48)
                        {
                            res.Statuscode = RechargeRespType.FAILED;
                            res.Msg = nameof(DMTErrorCodes.Invalid_bank).Replace("_", " ");
                            res.ErrorCode = DMTErrorCodes.Invalid_bank;
                            res.LiveID = res.Msg;
                        }
                        else if (ekoResp.status == 102)
                        {
                            res.Statuscode = RechargeRespType.FAILED;
                            res.Msg = nameof(DMTErrorCodes.Invalid_Account_Number).Replace("_", " ");
                            res.ErrorCode = DMTErrorCodes.Invalid_Account_Number;
                            res.LiveID = res.Msg;
                        }
                        else if (ekoResp.status == 46 && ekoResp.response_status_id == 1)
                        {
                            res.Statuscode = RechargeRespType.FAILED;
                            res.Msg = nameof(DMTErrorCodes.Service_Error_Transaction_is_Not_Permitted_to_account).Replace("_", " ");
                            res.ErrorCode = DMTErrorCodes.Service_Error_Transaction_is_Not_Permitted_to_account;
                            res.LiveID = res.Msg;
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
                    FuncName = "AccountVerification",
                    Error = ex.Message,
                    LoginTypeID = _req.LT,
                    UserId = _req.UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            var dMTReq = new DMTReqRes
            {
                APIID = _APIID,
                Method = "AccountVerification",
                RequestModeID = _req.RequestMode,
                Request = URL + JsonConvert.SerializeObject(eKORequest) + JsonConvert.SerializeObject(headers) + "?" + PostData,
                Response = resp,
                SenderNo = _req.SenderNO,
                UserID = _req.UserID,
                TID = _req.TID
            };
            new ProcUpdateLogDMRReqResp(_dal).Call(dMTReq);
            res.Request = dMTReq.Request;
            res.Response = dMTReq.Response;
            return res;
        }

        public async Task<DMRTransactionResponse> InitiateTransaction(DMTReq _req, ReqSendMoney sendMoney, DMRTransactionResponse res)
        {
            res.Statuscode = RechargeRespType.PENDING;
            res.Msg = nameof(ErrorCodes.Request_Accpeted).Replace("_", " ");
            res.ErrorCode = ErrorCodes.Request_Accpeted;
            res.LiveID = res.Msg;
            var eKORequest = new EKORequest
            {
                customer_id = _req.SenderNO,
                recipient_id = sendMoney.BeneID,
                amount = sendMoney.Amount,
                channel = sendMoney.Channel ? 2 : 1,
                state = 1,
                timestamp = DateTimeOffset.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'"),
                hold_timeout = 100,
                merchant_document_id_type = 1,
                merchant_document_id = res.PanNo,
                pincode = res.Pincode,
                latlong = res.LatLong,
                client_ref_id = res.TID.ToString()
            };
            string ekoTimeStamp = EKOAPIHelper.O.GetSecretKeyTimeStamp();
            string URL = _eKOAppSetting.BaseURL + "transactions";
            var headers = new Dictionary<string, string>
            {
                { EKOAPIHelper.SecretKeyTimeStamp, ekoTimeStamp },
                { EKOAPIHelper.SecretKey, EKOAPIHelper.O.GetSecretKey(ekoTimeStamp,_eKOAppSetting.EKOKey) },
                { ContentType.Self,ContentType.x_wwww_from_urlencoded},
                { EKOAPIHelper.DeveloperKey, _eKOAppSetting.DeveloperKey }
            };
            string PostData = "recipient_id=" + eKORequest.recipient_id + "&amount=" + eKORequest.amount + "&initiator_id=" + (_eKOAppSetting.InitiatorKey ?? string.Empty) + "&customer_id=" + eKORequest.customer_id + "&timestamp=" + eKORequest.timestamp + "&currency=INR&customer_id=" + eKORequest.cust_id + "&client_ref_id=" + eKORequest.client_ref_id + "&hold_timeout=" + eKORequest.hold_timeout + "&state=" + eKORequest.state + "&channel=" + eKORequest.channel + "&merchant_document_id_type=" + eKORequest.merchant_document_id_type + "&merchant_document_id=" + eKORequest.merchant_document_id + "&pincode=" + eKORequest.pincode + "&latlong=" + eKORequest.latlong;
            if (eKORequest.channel == 1)
            {
                PostData += "&ifsc=" + eKORequest.ifsc;
            }
            string resp = string.Empty;
            try
            {
                resp = await AppWebRequest.O.HWRPostAsync(URL, PostData, headers).ConfigureAwait(false);
                var ekoResp = JsonConvert.DeserializeObject<EKOClassess>(resp);
                if (ekoResp != null)
                {
                    if (ekoResp.status != null)
                    {
                        if (ekoResp.status == 1217 && ekoResp.response_type_id == -1)
                        {
                            res.Statuscode = RechargeRespType.FAILED;
                            res.Msg = nameof(DMTErrorCodes.KYC_Not_Completed_Please_complete_your_KYC_to_use_this_Service).Replace("_", " ");
                            res.ErrorCode = DMTErrorCodes.KYC_Not_Completed_Please_complete_your_KYC_to_use_this_Service;
                            res.LiveID = res.Msg;
                        }
                        else if ((ekoResp.status == 463 && ekoResp.response_type_id == -1) || ekoResp.status == 319)
                        {
                            res.Statuscode = RechargeRespType.FAILED;
                            res.Msg = nameof(DMTErrorCodes.Sender_Not_Found).Replace("_", " ");
                            res.ErrorCode = DMTErrorCodes.Sender_Not_Found;
                            res.LiveID = res.Msg;
                        }
                        else if (ekoResp.status == 46)
                        {
                            res.Statuscode = RechargeRespType.FAILED;
                            res.Msg = nameof(DMTErrorCodes.Service_Error_Transaction_is_Not_Permitted_to_account).Replace("_", " ");
                            res.ErrorCode = DMTErrorCodes.Service_Error_Transaction_is_Not_Permitted_to_account;
                            res.LiveID = res.Msg;
                        }
                        else if (ekoResp.status == 48)
                        {
                            res.Statuscode = RechargeRespType.FAILED;
                            res.Msg = nameof(DMTErrorCodes.Invalid_bank).Replace("_", " ");
                            res.ErrorCode = DMTErrorCodes.Invalid_bank;
                            res.LiveID = res.Msg;
                        }
                        else if (ekoResp.status == 317)
                        {
                            res.Statuscode = RechargeRespType.FAILED;
                            res.Msg = nameof(DMTErrorCodes.NEFT_not_allowed).Replace("_", " ");
                            res.ErrorCode = DMTErrorCodes.NEFT_not_allowed;
                            res.LiveID = res.Msg;
                        }
                        else if (ekoResp.status.In(53, 344))
                        {
                            res.Statuscode = RechargeRespType.FAILED;
                            res.Msg = nameof(DMTErrorCodes.IMPS_not_allowed).Replace("_", " ");
                            res.ErrorCode = DMTErrorCodes.IMPS_not_allowed;
                            res.LiveID = res.Msg;
                        }
                        else if (ekoResp.status == 460)
                        {
                            res.Statuscode = RechargeRespType.FAILED;
                            res.Msg = nameof(DMTErrorCodes.Invalid_Channel).Replace("_", " ");
                            res.ErrorCode = DMTErrorCodes.Invalid_Channel;
                            res.LiveID = res.Msg;
                        }
                        else if (ekoResp.status == 521)
                        {
                            res.Statuscode = RechargeRespType.FAILED;
                            res.Msg = nameof(DMTErrorCodes.Please_enter_correct_IFSC_code).Replace("_", " ");
                            res.ErrorCode = DMTErrorCodes.Please_enter_correct_IFSC_code;
                            res.LiveID = res.Msg;
                        }
                        else if (ekoResp.status.In(314, 945))
                        {
                            res.Statuscode = RechargeRespType.FAILED;
                            res.Msg = nameof(DMTErrorCodes.Senders_Monthly_Or_Daily_limit_breached).Replace("_", " ");
                            res.ErrorCode = DMTErrorCodes.Senders_Monthly_Or_Daily_limit_breached;
                            res.LiveID = res.Msg;
                        }
                        else if (ekoResp.status == 313 && ekoResp.response_type_id == -1)
                        {
                            res.Statuscode = RechargeRespType.FAILED;
                            res.Msg = nameof(DMTErrorCodes.Beneficiary_not_found).Replace("_", " ");
                            res.ErrorCode = DMTErrorCodes.Beneficiary_not_found;
                            res.LiveID = res.Msg;
                        }
                        else if ((ekoResp.status == 55 && ekoResp.response_type_id == 55) || ekoResp.status == 347)
                        {
                            res.Statuscode = RechargeRespType.PENDING;
                            res.Msg = nameof(ErrorCodes.Request_Accpeted).Replace("_", " ");
                            res.ErrorCode = ErrorCodes.Request_Accpeted;
                            res.LiveID = res.Msg;
                        }
                        else if (ekoResp.status == 0 && ekoResp.response_type_id == 325)
                        {
                            res.VendorID = ekoResp.data.tid ?? "";
                            res.LiveID = ekoResp.data.bank_ref_num ?? "";
                            //res. = ekoResp.data.sender_name ?? "";
                            int ekoTxStatus = Convert.ToInt16(Validators.Validate.O.IsNumeric(ekoResp.data.tx_status ?? string.Empty) ? ekoResp.data.tx_status : "-1");
                            if (ekoTxStatus == 0)
                            {
                                res.Statuscode = RechargeRespType.SUCCESS;
                                res.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                                res.ErrorCode = ErrorCodes.Transaction_Successful;
                            }
                        }
                        //if (ekoResp.status == 544 && ekoResp.response_type_id == 544)
                        //{
                        //    //DateTime dt = DateTime.Now;
                        //    //res.STATUS = RechargeRespType._PENDING;
                        //    //if ((dt.Hour >= 21 && dt.Hour <= 23))
                        //    //{
                        //    //    res.STATUS = RechargeRespType._FAILED;
                        //    //}

                        //    //res.MSG = "Bank is not available!";
                        //}
                    }
                }
            }
            catch (Exception ex)
            {
                resp = " Exception:" + ex.Message + " | " + resp;
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "InitiateTransaction",
                    Error = ex.Message,
                    LoginTypeID = _req.LT,
                    UserId = _req.UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            var dMTReq = new DMTReqRes
            {
                APIID = _APIID,
                Method = "InitiateTransaction",
                RequestModeID = _req.RequestMode,
                Request = URL + JsonConvert.SerializeObject(eKORequest) + JsonConvert.SerializeObject(headers) + "?" + PostData,
                Response = resp,
                SenderNo = _req.SenderNO,
                UserID = _req.UserID,
                TID = _req.TID
            };
            new ProcUpdateLogDMRReqResp(_dal).Call(dMTReq);
            res.Request = dMTReq.Request;
            res.Response = dMTReq.Response;
            return res;
        }

        public async Task<DMRTransactionResponse> GetTransactionStatus(int TID, int RequestMode, int UserID, string SenderNo, string VendorID)
        {
            var res = new DMRTransactionResponse
            {
                Statuscode = RechargeRespType.PENDING,
                Msg = nameof(RechargeRespType.PENDING),
                ErrorCode = ErrorCodes.Request_Accpeted
            };
            var eKORequest = new EKORequest
            {
                client_ref_id = TID.ToString(),
                id = VendorID ?? string.Empty
            };
            string ekoTimeStamp = EKOAPIHelper.O.GetSecretKeyTimeStamp();
            string URL = _eKOAppSetting.BaseURL + "transactions/client_ref_id:" + eKORequest.client_ref_id + "?initiator_id=" + _eKOAppSetting.InitiatorKey;
            if ((VendorID ?? string.Empty).Length > 0)
            {
                URL = _eKOAppSetting.BaseURL + "transactions/" + eKORequest.id + "?initiator_id=" + _eKOAppSetting.InitiatorKey;
            }
            var headers = new Dictionary<string, string>
            {
                { EKOAPIHelper.SecretKeyTimeStamp, ekoTimeStamp },
                { EKOAPIHelper.SecretKey, EKOAPIHelper.O.GetSecretKey(ekoTimeStamp,_eKOAppSetting.EKOKey) },
                { EKOAPIHelper.DeveloperKey, _eKOAppSetting.DeveloperKey }
            };
            string resp = string.Empty;
            try
            {
                resp = await AppWebRequest.O.CallUsingHttpWebRequest_GETAsync(URL, headers);
                var ekoResp = JsonConvert.DeserializeObject<EKOClassess>(resp);
                if (ekoResp != null)
                {
                    if (ekoResp.status != null)
                    {
                        if (ekoResp.status == 0 && ekoResp.response_type_id == 70)
                        {
                            int ekoTxStatus = Convert.ToInt16(Validate.O.IsNumeric(ekoResp.data.tx_status ?? "") ? ekoResp.data.tx_status : "-1");
                            res.VendorID = ekoResp.data.tid;
                            if (ekoTxStatus == 3)
                            {
                                res.Msg = "Refundable";
                                res.IsRefundAvailable = true;
                                res.Statuscode = RechargeRespType.SUCCESS;
                                res.LiveID = ekoResp.data.bank_ref_num;
                                res.ErrorCode = ErrorCodes.Transaction_Successful;
                            }
                            else if (ekoTxStatus == 0)
                            {
                                res.Statuscode = RechargeRespType.SUCCESS;
                                res.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                                res.LiveID = ekoResp.data.bank_ref_num;
                                res.ErrorCode = ErrorCodes.Transaction_Successful;
                            }
                            else if (ekoTxStatus == 1)
                            {
                                res.Statuscode = RechargeRespType.FAILED;
                                res.Msg = nameof(DMTErrorCodes.Transaction_Failed).Replace("_", " ");
                                res.LiveID = ekoResp.data.bank_ref_num;
                                res.ErrorCode = DMTErrorCodes.Transaction_Failed;
                            }
                            else if (ekoTxStatus == 2)
                            {
                                res.Statuscode = RechargeRespType.FAILED;
                                res.Msg = nameof(DMTErrorCodes.Transaction_Failed).Replace("_", " ");
                                res.LiveID = ekoResp.data.bank_ref_num;
                                res.ErrorCode = DMTErrorCodes.Transaction_Failed;
                            }
                            else if (ekoTxStatus == 4)
                            {
                                res.Statuscode = RechargeRespType.REFUND;
                                res.Msg = nameof(DMTErrorCodes.Transaction_Status_Updated_Refund).Replace("_", " ");
                                res.LiveID = ekoResp.data.bank_ref_num;
                                res.ErrorCode = DMTErrorCodes.Transaction_Status_Updated_Refund;
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
                APIID = _APIID,
                Method = "GetTransactionStatus",
                RequestModeID = RequestMode,
                Request = URL + JsonConvert.SerializeObject(eKORequest) + JsonConvert.SerializeObject(headers),
                Response = resp,
                SenderNo = SenderNo,
                UserID = UserID,
                TID = TID.ToString()
            };
            new ProcUpdateLogDMRReqResp(_dal).Call(dMTReq);
            res.Request = dMTReq.Request;
            res.Response = resp;
            return res;
        }

        public async Task<ResponseStatus> Refund(int TID, string VendorID, int RequestMode, int UserID, string SenderNo, string OTP, int State = 1)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = nameof(ErrorCodes.Unknown_Error).Replace("_", " "),
                ErrorCode = ErrorCodes.Unknown_Error
            };
            var eKORequest = new EKORequest
            {
                tid = VendorID,
                client_ref_id = TID.ToString(),
                otp = OTP,
                state = 1
            };
            string ekoTimeStamp = EKOAPIHelper.O.GetSecretKeyTimeStamp();
            string URL = _eKOAppSetting.BaseURL + "transactions/" + eKORequest.tid + "/refund";
            var headers = new Dictionary<string, string>
            {
                { EKOAPIHelper.SecretKeyTimeStamp, ekoTimeStamp },
                { EKOAPIHelper.SecretKey, EKOAPIHelper.O.GetSecretKey(ekoTimeStamp,_eKOAppSetting.EKOKey) },
                { ContentType.Self,ContentType.x_wwww_from_urlencoded},
                { EKOAPIHelper.DeveloperKey, _eKOAppSetting.DeveloperKey }
            };
            string PostData = "initiator_id=" + (_eKOAppSetting.InitiatorKey ?? string.Empty) + "&otp=" + eKORequest.otp + "&state=" + State + "&client_ref_id=" + eKORequest.client_ref_id;

            string resp = string.Empty;
            try
            {
                resp = await AppWebRequest.O.HWRPostAsync(URL, PostData, headers).ConfigureAwait(false);

                var ekoResp = JsonConvert.DeserializeObject<EKOClassess>(resp);
                if (ekoResp.status == 355 && ekoResp.response_type_id == -1)
                {
                    res.Msg = nameof(ErrorCodes.Invalid_OTP).Replace("_", " ");
                    res.ErrorCode = ErrorCodes.Invalid_OTP;
                }
                else if (ekoResp.status == 0 && ekoResp.response_type_id == 74)
                {
                    res.Statuscode = ErrorCodes.One;
                    res.Msg = nameof(DMTErrorCodes.Transaction_Status_Updated_Refund).Replace("_", " ");
                    res.ErrorCode = DMTErrorCodes.Transaction_Status_Updated_Refund;
                }
            }
            catch (Exception ex)
            {
                resp = " Exception:" + ex.Message + " | " + resp;
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
                APIID = _APIID,
                Method = "Refund",
                RequestModeID = RequestMode,
                Request = URL + JsonConvert.SerializeObject(eKORequest) + JsonConvert.SerializeObject(headers),
                Response = resp,
                SenderNo = SenderNo,
                UserID = UserID,
                TID = TID.ToString()
            };
            new ProcUpdateLogDMRReqResp(_dal).Call(dMTReq);
            res.CommonStr = dMTReq.Request;
            res.CommonStr2 = dMTReq.Response;
            return res;
        }

        public async Task<ResponseStatus> ResendRefundOTP(string VendorID, int RequestMode, int UserID, string SenderNo, int TID)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = nameof(ErrorCodes.Unknown_Error).Replace("_", " "),
                ErrorCode = ErrorCodes.Unknown_Error
            };
            var eKORequest = new EKORequest
            {
                tid = VendorID
            };
            string ekoTimeStamp = EKOAPIHelper.O.GetSecretKeyTimeStamp();
            string URL = _eKOAppSetting.BaseURL + "transactions/" + eKORequest.tid + "/refund/otp";
            var headers = new Dictionary<string, string>
            {
                { EKOAPIHelper.SecretKeyTimeStamp, ekoTimeStamp },
                { EKOAPIHelper.SecretKey, EKOAPIHelper.O.GetSecretKey(ekoTimeStamp,_eKOAppSetting.EKOKey) },
                { ContentType.Self,ContentType.x_wwww_from_urlencoded},
                { EKOAPIHelper.DeveloperKey, _eKOAppSetting.DeveloperKey }
            };
            string PostData = "initiator_id=" + (_eKOAppSetting.InitiatorKey ?? string.Empty);

            string resp = string.Empty;
            try
            {
                resp = await AppWebRequest.O.HWRPostAsync(URL, PostData, headers);
                var ekoResp = JsonConvert.DeserializeObject<EKOClassess>(resp);
                if (ekoResp.status == 168 && ekoResp.response_type_id == -1)
                {
                    res.Msg = nameof(DMTErrorCodes.Declined_by_ServiceProvider).Replace("_", " ");
                    res.ErrorCode = DMTErrorCodes.Declined_by_ServiceProvider;
                }
                else if (ekoResp.status == 0 && ekoResp.response_type_id == 169)
                {
                    res.Statuscode = ErrorCodes.One;
                    res.Msg = nameof(DMTErrorCodes.OTP_for_verification_has_been_sent_successfully).Replace("_", " ");
                    res.ErrorCode = DMTErrorCodes.OTP_for_verification_has_been_sent_successfully;
                }
            }
            catch (Exception ex)
            {
                resp = " Exception:" + ex.Message + " | " + resp;
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
                APIID = _APIID,
                Method = "ResendRefundOTP",
                RequestModeID = RequestMode,
                Request = URL + JsonConvert.SerializeObject(eKORequest) + JsonConvert.SerializeObject(headers),
                Response = resp,
                SenderNo = SenderNo,
                UserID = UserID,
                TID = TID.ToString()
            };
            new ProcUpdateLogDMRReqResp(_dal).Call(dMTReq);
            return res;
        }
        #endregion
    }
    public partial class EKOML : IMoneyTransferAPIML
    {
        public EKOML(IHttpContextAccessor accessor, IHostingEnvironment env)
        {
            _accessor = accessor;
            _env = env;
            var builder = new ConfigurationBuilder()
                 .SetBasePath(Directory.GetCurrentDirectory());
            builder.AddJsonFile((_env.IsProduction() ? "appsettings.json" : "appsettings.Development.json"));
            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
            _c = new ConnectionConfiguration(_accessor, _env);
            _dal = new DAL(_c.GetConnectionString());
            _eKOAppSetting = EKOAppSetting();
        }
        public MSenderLoginResponse GetSender(MTAPIRequest request)
        {
            var res = new MSenderLoginResponse
            {
                Statuscode = ErrorCodes.One,
                Msg = nameof(ErrorCodes.Unknown_Error).Replace("_", " "),
                ErrorCode = ErrorCodes.Unknown_Error
            };
            var eKORequest = new EKORequest
            {
                customer_id = request.SenderMobile
            };
            string ekoTimeStamp = EKOAPIHelper.O.GetSecretKeyTimeStamp();
            string URL = _eKOAppSetting.BaseURL + "customers/mobile_number:" + eKORequest.customer_id + "?initiator_id=" + _eKOAppSetting.InitiatorKey;
            var headers = new Dictionary<string, string>
            {
                { EKOAPIHelper.SecretKeyTimeStamp, ekoTimeStamp },
                { EKOAPIHelper.SecretKey, EKOAPIHelper.O.GetSecretKey(ekoTimeStamp,_eKOAppSetting.EKOKey) },
                { EKOAPIHelper.DeveloperKey, _eKOAppSetting.DeveloperKey }
            };

            string resp = string.Empty;
            try
            {
                resp =  AppWebRequest.O.CallUsingHttpWebRequest_GETAsync(URL, headers).Result;
                var ekoResp = JsonConvert.DeserializeObject<EKOClassess>(resp);
                if (ekoResp == null)
                {
                    res.Msg = nameof(DMTErrorCodes.Sender_Not_Found).Replace("_", " ");
                    res.ErrorCode = DMTErrorCodes.Sender_Not_Found;
                    res.IsSenderNotExists = true;
                }
                else if (ekoResp.status == null)
                {
                    res.Msg = nameof(DMTErrorCodes.Sender_Not_Found).Replace("_", " ");
                    res.ErrorCode = DMTErrorCodes.Sender_Not_Found;
                    res.IsSenderNotExists = true;
                }
                else if (ekoResp.status == 463 || ekoResp.response_type_id == -1)
                {
                    res.Msg = nameof(DMTErrorCodes.Sender_Not_Found).Replace("_", " ");
                    res.ErrorCode = DMTErrorCodes.Sender_Not_Found;
                    res.IsSenderNotExists = true;
                }
                else if (ekoResp.status == 0 && ekoResp.response_type_id.In(33, 37, 332))
                {
                    if (ekoResp.response_type_id == 37)
                    {
                        res.Statuscode = ErrorCodes.One;
                        res.Msg = nameof(DMTErrorCodes.Partial_Registration_Please_verify_sender_with_OTP).Replace("_", " ");
                        res.ErrorCode = DMTErrorCodes.Partial_Registration_Please_verify_sender_with_OTP;
                        res.IsOTPGenerated = true;
                    }
                    else
                    {
                        res.Msg = nameof(DMTErrorCodes.Detail_Found_Successfully).Replace("_", " ");
                        res.ErrorCode = DMTErrorCodes.Detail_Found_Successfully;
                        res.RemainingLimit = Convert.ToDecimal(ekoResp.data.available_limit-ekoResp.data.used_limit);
                        res.AvailbleLimit = Convert.ToDecimal(ekoResp.data.available_limit);
                        res.SenderMobile = ekoResp.data.mobile;
                        res.SenderName = ekoResp.data.name;
                        if (ekoResp.data.state.Equals("2") || ekoResp.data.state.Equals("1"))
                        {
                            res.KYCStatus = SenderKYCStatus.NOTREGISTRED;
                        }
                        else if (ekoResp.data.state.Equals("4"))
                        {
                            res.KYCStatus = SenderKYCStatus.ACTIVE;
                        }
                        else if (ekoResp.data.state_desc.ToUpper().Equals("REJECTED"))
                        {
                            res.KYCStatus = SenderKYCStatus.REJECTED;
                        }
                        else
                        {
                            res.KYCStatus = SenderKYCStatus.APPLIED;
                        }
                    }
                }
                else
                {
                    res.Msg = nameof(DMTErrorCodes.Sender_Not_Found).Replace("_", " ");
                    res.ErrorCode = DMTErrorCodes.Sender_Not_Found;
                    res.IsSenderNotExists = true;
                }
            }
            catch (Exception ex)
            {
                resp = " Exception:" + ex.Message + " | " + resp;
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
                APIID = _APIID,
                Method = "GetSender",
                RequestModeID = request.RequestMode,
                Request = URL + JsonConvert.SerializeObject(eKORequest) + JsonConvert.SerializeObject(headers),
                Response = resp,
                SenderNo = request.SenderMobile,
                UserID = request.UserID,
                TID = request.TransactionID
            };
            new ProcUpdateLogDMRReqResp(_dal).Call(dMTReq);
            return res;
        }
        public MSenderCreateResp SenderKYC(MTAPIRequest request)
        {
            var res = new MSenderCreateResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = nameof(ErrorCodes.Unknown_Error).Replace("_", " "),
                ErrorCode = ErrorCodes.Unknown_Error
            };
            var eKORequest = new EKORequest
            {
                customer_id = request.SenderMobile ?? string.Empty,
                name = request.NameOnKYC ?? string.Empty,
                id_proof_type_id = 15,
                id_proof = request.AadharNo,
                ovd_type_id = 4,
                ovd_number = request.PANNo,
                file1 = request.AadharFrontURL,
                file2 = request.AadharBackURL,
                customer_photo = request.SenderPhotoURL,
                ovd_image = request.PANURL
            };
            string ekoTimeStamp = EKOAPIHelper.O.GetSecretKeyTimeStamp();
            var URL = _eKOAppSetting.BaseURL + "customers/mobile_number:" + eKORequest.customer_id;

            var headers = new Dictionary<string, string>
            {
                { EKOAPIHelper.SecretKeyTimeStamp, ekoTimeStamp },
                { EKOAPIHelper.SecretKey, EKOAPIHelper.O.GetSecretKey(ekoTimeStamp,_eKOAppSetting.EKOKey) },
                { EKOAPIHelper.DeveloperKey, _eKOAppSetting.DeveloperKey }
            };
            var frmData = new StringBuilder();
            //initiator_id=9910028267&id_proof_type_id=15&id_proof=123456789117&name=Test&ovd_type_id=4&ovd_number=AOYPM2624w&file1&file2&customer_photo
            frmData.Append("initiator_id=");
            frmData.Append(_eKOAppSetting.InitiatorKey ?? string.Empty);
            frmData.Append("&id_proof_type_id=");
            frmData.Append(eKORequest.id_proof_type_id);
            frmData.Append("&id_proof=");
            frmData.Append(eKORequest.id_proof ?? string.Empty);
            frmData.Append("&name=");
            frmData.Append(eKORequest.name ?? string.Empty);
            frmData.Append("&ovd_type_id=");
            frmData.Append(eKORequest.ovd_type_id);
            frmData.Append("&ovd_number=");
            frmData.Append(eKORequest.ovd_number ?? string.Empty);
            frmData.Append("&file1&file2&customer_photo&ovd_image");

            var FormData = new Dictionary<string, string>
            {
                {"form-data", frmData.ToString()}
            };
            var files = new Dictionary<string, string> {
                { nameof(eKORequest.file1),eKORequest.file1},
                { nameof(eKORequest.file2),eKORequest.file2},
                {nameof(eKORequest.customer_photo),eKORequest.customer_photo},
                {nameof(eKORequest.ovd_image),eKORequest.ovd_image}
            };
            string resp = string.Empty;
            var ekoResp = new EKOClassess();
            try
            {
                resp = AppWebRequest.O.WebClientPutFiles(URL, FormData, headers, files);
                ekoResp = JsonConvert.DeserializeObject<EKOClassess>(resp);
                if (ekoResp != null)
                {
                    if (ekoResp.status != null)
                    {
                        if (ekoResp.status == 0 && ekoResp.response_type_id == 214)
                        {
                            res.Statuscode = ErrorCodes.One;
                            res.Msg = "KYC Uploaded successfully!";
                            res.ErrorCode = DMTErrorCodes.Sender_Created_Successfully;
                        }
                        else if (ekoResp.status.In(700, 1237))
                        {
                            res.Msg = ekoResp.message;
                        }
                        else
                        {
                            res.Msg = "KYC could not be updated!";
                        }
                    }
                    else
                    {
                        res.Msg = "KYC could not be updated. Try after sometime or KYC not required at this moment.";
                    }
                }
            }
            catch (Exception ex)
            {
                resp = " Exception:" + ex.Message + " | " + resp;
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "SenderKYC",
                    Error = ex.Message,
                    LoginTypeID = 1,
                    UserId = 1
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            var dMTReq = new DMTReqRes
            {
                APIID = _APIID,
                Method = "SenderKYC",
                RequestModeID = request.RequestMode,
                Request = URL + JsonConvert.SerializeObject(eKORequest) + JsonConvert.SerializeObject(headers) + "?" + JsonConvert.SerializeObject(FormData) + JsonConvert.SerializeObject(files),
                Response = resp,
                SenderNo = request.SenderMobile,
                UserID = request.UserID,
                TID = request.TransactionID
            };
            new ProcUpdateLogDMRReqResp(_dal).Call(dMTReq);
            return res;
        }
        public MSenderLoginResponse CreateBeneficiary(MTAPIRequest request)
        {
            var res = new MSenderLoginResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = nameof(ErrorCodes.Unknown_Error).Replace("_", " "),
                ErrorCode = ErrorCodes.Unknown_Error
            };
            IBankML bankML = new BankML(_accessor, _env, false);
            var BankDetail = bankML.BankMasters(request.mBeneDetail.BankID);
            var eKORequest = new EKORequest
            {
                customer_id = request.SenderMobile,
                recipient_id_type = "acc_ifsc",
                //id = addBeni.AccountNo + "_" + ((BankDetail.IFSC ?? string.Empty) == string.Empty ? (addBeni.IFSC ?? string.Empty) : BankDetail.IFSC),
                id = request.mBeneDetail.AccountNo + "_" + (request.mBeneDetail.IFSC ?? string.Empty),
                recipient_type = "3",
                recipient_name = request.mBeneDetail.BeneName,
                recipient_mobile = request.mBeneDetail.MobileNo,
                bank_id = BankDetail.EKO_BankID + string.Empty
            };
            string ekoTimeStamp = EKOAPIHelper.O.GetSecretKeyTimeStamp();
            string URL = _eKOAppSetting.BaseURL + "customers/mobile_number:" + eKORequest.customer_id + "/recipients/" + eKORequest.recipient_id_type + ":" + eKORequest.id;
            var headers = new Dictionary<string, string>
            {
                { EKOAPIHelper.SecretKeyTimeStamp, ekoTimeStamp },
                { EKOAPIHelper.SecretKey, EKOAPIHelper.O.GetSecretKey(ekoTimeStamp,_eKOAppSetting.EKOKey) },
                { ContentType.Self,ContentType.x_wwww_from_urlencoded},
                { EKOAPIHelper.DeveloperKey, _eKOAppSetting.DeveloperKey }
            };
            string PutData = "recipient_type=" + eKORequest.recipient_type + "&recipient_name=" + eKORequest.recipient_name + "&recipient_mobile=" + eKORequest.recipient_mobile + "&initiator_id=" + (_eKOAppSetting.InitiatorKey ?? string.Empty) + "&bank_id=" + eKORequest.bank_id;

            string resp = string.Empty;
            try
            {
                resp = AppWebRequest.O.HWRPUTAsync(URL, PutData, headers).Result;
                var ekoResp = JsonConvert.DeserializeObject<EKOClassess>(resp);
                if (ekoResp != null)
                {
                    if (ekoResp.status != null)
                    {
                        if (ekoResp.status == 39 && ekoResp.response_type_id == -1)
                        {
                            res.Msg = nameof(DMTErrorCodes.Beneficiarys_Monthly_Or_Daily_Limit_Exceed).Replace("_", " ");
                            res.ErrorCode = DMTErrorCodes.Beneficiarys_Monthly_Or_Daily_Limit_Exceed;
                        }
                        else if (ekoResp.status == 0 && ekoResp.response_type_id.In(342, 145))
                        {
                            res.Msg = nameof(DMTErrorCodes.Beneficiary_Already_Exist).Replace("_", " ");
                            res.ErrorCode = DMTErrorCodes.Beneficiary_Already_Exist;
                        }
                        else if (ekoResp.status == 0 && ekoResp.response_type_id == 43)
                        {
                            res.Statuscode = ErrorCodes.One;
                            res.Msg = nameof(DMTErrorCodes.Beneficiary_Added_Successfully).Replace("_", " ");
                            res.ErrorCode = DMTErrorCodes.Beneficiary_Added_Successfully;
                        }
                        else if (ekoResp.status == 131)
                        {
                            res.Msg = nameof(DMTErrorCodes.Invalid_Name).Replace("_", " ");
                            res.ErrorCode = DMTErrorCodes.Invalid_Name;
                        }
                        else if (ekoResp.status == 45 && ekoResp.response_status_id == 1)
                        {
                            res.Msg = nameof(DMTErrorCodes.Please_enter_correct_IFSC_code).Replace("_", " ");
                            res.ErrorCode = DMTErrorCodes.Please_enter_correct_IFSC_code;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                resp = " Exception:" + ex.Message + " | " + resp;
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "CreateBeneficiary",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = request.UserID
                });
            }
            new ProcUpdateLogDMRReqResp(_dal).Call(new DMTReqRes
            {
                APIID = request.APIID,
                Method = "CreateBeneficiary",
                RequestModeID = request.RequestMode,
                Request = URL + JsonConvert.SerializeObject(eKORequest) + JsonConvert.SerializeObject(headers) + "?" + PutData,
                Response = resp,
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
            var eKORequest = new EKORequest
            {
                customer_id = request.SenderMobile ?? string.Empty,
                name = request.FirstName+" "+ request.LastName
            };
            string ekoTimeStamp = EKOAPIHelper.O.GetSecretKeyTimeStamp();
            string URL = _eKOAppSetting.BaseURL + "customers/mobile_number:" + eKORequest.customer_id;
            var headers = new Dictionary<string, string>
            {
                { EKOAPIHelper.SecretKeyTimeStamp, ekoTimeStamp },
                { EKOAPIHelper.SecretKey, EKOAPIHelper.O.GetSecretKey(ekoTimeStamp,_eKOAppSetting.EKOKey) },
                { ContentType.Self,ContentType.x_wwww_from_urlencoded},
                { EKOAPIHelper.DeveloperKey, _eKOAppSetting.DeveloperKey }
            };
            string PutData = "initiator_id=" + (_eKOAppSetting.InitiatorKey ?? string.Empty) + "&name=" + eKORequest.name;

            string resp = string.Empty;
            try
            {
                var ekoResp = new EKOClassess();
                resp = AppWebRequest.O.HWRPUTAsync(URL, PutData, headers).Result;
                ekoResp = JsonConvert.DeserializeObject<EKOClassess>(resp);
                if (ekoResp != null)
                {
                    if (ekoResp.status != null)
                    {
                        if (ekoResp.status == 17 && ekoResp.response_type_id == -1)
                        {
                            res.Msg = nameof(DMTErrorCodes.Sender_Already_Exist).Replace("_", " ");
                            res.ErrorCode = DMTErrorCodes.Sender_Already_Exist;
                        }
                        else if (ekoResp.status == 0)
                        {
                            res.Statuscode = ErrorCodes.One;
                            res.Msg = nameof(DMTErrorCodes.Partial_Registration_Please_verify_sender_with_OTP).Replace("_", " ");
                            res.IsOTPGenerated = true;
                            res.ErrorCode = DMTErrorCodes.Partial_Registration_Please_verify_sender_with_OTP;
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                resp = " Exception:" + ex.Message + " | " + resp;
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
                APIID = _APIID,
                Method = "CreateSender",
                RequestModeID = request.RequestMode,
                Request = URL + JsonConvert.SerializeObject(eKORequest) + JsonConvert.SerializeObject(headers) + "?" + PutData,
                Response = resp,
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
                Msg = nameof(ErrorCodes.Unknown_Error).Replace("_", " "),
                ErrorCode = ErrorCodes.Unknown_Error
            };
            var eKORequest = new EKORequest
            {
                customer_id = request.SenderMobile
            };
            string ekoTimeStamp = EKOAPIHelper.O.GetSecretKeyTimeStamp();
            string URL = _eKOAppSetting.BaseURL + "customers/mobile_number:" + eKORequest.customer_id + "/otp";

            var headers = new Dictionary<string, string>
            {
                { ContentType.Self,ContentType.x_wwww_from_urlencoded},
                { EKOAPIHelper.SecretKeyTimeStamp, ekoTimeStamp },
                { EKOAPIHelper.SecretKey, EKOAPIHelper.O.GetSecretKey(ekoTimeStamp,_eKOAppSetting.EKOKey) },
                { EKOAPIHelper.DeveloperKey, _eKOAppSetting.DeveloperKey }
            };
            string PostData = "initiator_id=" + (_eKOAppSetting.InitiatorKey ?? string.Empty);

            string resp = string.Empty;
            try
            {
                resp = AppWebRequest.O.HWRPostAsync(URL, PostData, headers).Result;
                var ekoResp = JsonConvert.DeserializeObject<EKOClassess>(resp);
                if (ekoResp != null)
                {
                    if (ekoResp.status != null)
                    {
                        if (ekoResp.status == 463 && ekoResp.response_type_id == -1)
                        {
                            res.Msg = nameof(DMTErrorCodes.Sender_Not_Found).Replace("_", " ");
                            res.ErrorCode = DMTErrorCodes.Sender_Not_Found;
                        }
                        else if (ekoResp.status == 0 && ekoResp.response_type_id == 322)
                        {
                            res.Msg = nameof(DMTErrorCodes.Sender_Already_Exist).Replace("_", " ");
                            res.ErrorCode = DMTErrorCodes.Sender_Already_Exist;
                        }
                        else if (ekoResp.status == 0 && ekoResp.response_type_id == 321)
                        {
                            res.Statuscode = ErrorCodes.One;
                            res.Msg = nameof(DMTErrorCodes.OTP_for_verification_has_been_sent_successfully).Replace("_", " ");
                            res.ErrorCode = DMTErrorCodes.OTP_for_verification_has_been_sent_successfully;
                            res.IsOTPGenerated = true;
                        }
                        else
                        {
                            res.Msg = nameof(DMTErrorCodes.Sender_Not_Found).Replace("_", " ");
                            res.ErrorCode = DMTErrorCodes.Sender_Not_Found;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                resp = " Exception:" + ex.Message + " | " + resp;
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
                APIID = _APIID,
                Method = "GenerateOTP",
                RequestModeID = request.RequestMode,
                Request = URL + JsonConvert.SerializeObject(eKORequest) + JsonConvert.SerializeObject(headers) + "?" + PostData,
                Response = resp,
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
                Msg = nameof(DMTErrorCodes.Beneficiary_not_found).Replace("_", " "),
                ErrorCode = DMTErrorCodes.Beneficiary_not_found
            };
            var eKORequest = new EKORequest
            {
                customer_id = request.SenderMobile
            };
            string ekoTimeStamp = EKOAPIHelper.O.GetSecretKeyTimeStamp();
            string URL = _eKOAppSetting.BaseURL + "customers/mobile_number:" + eKORequest.customer_id + "/recipients?initiator_id=" + _eKOAppSetting.InitiatorKey;

            var headers = new Dictionary<string, string>
            {
                { EKOAPIHelper.SecretKeyTimeStamp, ekoTimeStamp },
                { EKOAPIHelper.SecretKey, EKOAPIHelper.O.GetSecretKey(ekoTimeStamp,_eKOAppSetting.EKOKey) },
                { EKOAPIHelper.DeveloperKey, _eKOAppSetting.DeveloperKey }
            };

            string resp = "";
            try
            {
                resp = AppWebRequest.O.CallUsingHttpWebRequest_GETAsync(URL, headers).Result;
                var ekoBenes = JsonConvert.DeserializeObject<EKOClassess>(resp);
                if (ekoBenes != null)
                {
                    if (ekoBenes.status != null)
                    {
                        if (ekoBenes.status == 0 && ekoBenes.response_type_id == 23 && ekoBenes.data.recipient_list != null)
                        {
                            res.Statuscode = ErrorCodes.One;
                            res.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                            res.ErrorCode = ErrorCodes.Transaction_Successful;

                            var Beneficiaries = new List<MBeneDetail>();
                            foreach (var item in ekoBenes.data.recipient_list)
                            {
                                Beneficiaries.Add(new MBeneDetail
                                {
                                    AccountNo = item.account,
                                    BankName = item.bank,
                                    IFSC = item.ifsc,
                                    BeneName = item.recipient_name,
                                    MobileNo = item.bank,
                                    BeneID = item.recipient_id.ToString()
                                });
                            }
                            res.Beneficiaries = Beneficiaries;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                resp = " Exception:" + ex.Message + " | " + resp;
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
                APIID = _APIID,
                Method = "GetBeneficiary",
                RequestModeID = request.RequestMode,
                Request = URL + JsonConvert.SerializeObject(eKORequest) + JsonConvert.SerializeObject(headers),
                Response = resp,
                SenderNo = request.SenderMobile,
                UserID = request.UserID,
                TID = request.TransactionID
            });
            return res;
        }
        public MSenderCreateResp SenderResendOTP(MTAPIRequest request)
        {
            throw new NotImplementedException();
        }

        public MSenderLoginResponse ValidateBeneficiary(MTAPIRequest request)
        {
            throw new NotImplementedException();
        }

        public MSenderLoginResponse VerifySender(MTAPIRequest request)
        {
            var res = new MSenderLoginResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = nameof(ErrorCodes.Unknown_Error).Replace("_", " "),
                ErrorCode = ErrorCodes.Unknown_Error
            };
            var eKORequest = new EKORequest
            {
                customer_id = request.SenderMobile,
                otp = request.OTP
            };
            string ekoTimeStamp = EKOAPIHelper.O.GetSecretKeyTimeStamp();
            string URL = _eKOAppSetting.BaseURL + "customers/verification/otp:" + eKORequest.otp;
            var headers = new Dictionary<string, string>
            {
                { EKOAPIHelper.SecretKeyTimeStamp, ekoTimeStamp },
                { EKOAPIHelper.SecretKey, EKOAPIHelper.O.GetSecretKey(ekoTimeStamp,_eKOAppSetting.EKOKey) },
                { ContentType.Self,ContentType.x_wwww_from_urlencoded},
                { EKOAPIHelper.DeveloperKey, _eKOAppSetting.DeveloperKey }
            };
            string PutData = "initiator_id=" + (_eKOAppSetting.InitiatorKey ?? string.Empty) + "&id_type=mobile_number&id=" + eKORequest.customer_id;

            string resp = string.Empty;
            try
            {
                resp = AppWebRequest.O.HWRPUTAsync(URL, PutData, headers).Result;
                var ekoResp = JsonConvert.DeserializeObject<EKOClassess>(resp);
                if (ekoResp != null)
                {
                    if (ekoResp.status != null)
                    {
                        if (ekoResp.status.In(100, 302) && ekoResp.response_type_id == -1)
                        {
                            res.Msg = nameof(ErrorCodes.Invalid_OTP).Replace("_", " ");
                            res.ErrorCode = ErrorCodes.Invalid_OTP;
                        }
                        else if (ekoResp.status == 303 && ekoResp.response_type_id == -1)
                        {
                            res.Msg = nameof(DMTErrorCodes.OTP_Expired).Replace("_", " ");
                            res.ErrorCode = DMTErrorCodes.OTP_Expired;
                        }
                        else if (ekoResp.status == 463 && ekoResp.response_type_id == -1)
                        {
                            res.Msg = nameof(DMTErrorCodes.Sender_Not_Found).Replace("_", " ");
                            res.ErrorCode = DMTErrorCodes.Sender_Not_Found;
                        }

                        else if (ekoResp.status == 0 && ekoResp.response_type_id.In(300, 301))
                        {
                            res.Statuscode = ErrorCodes.One;
                            res.Msg = nameof(DMTErrorCodes.Sender_Created_Successfully).Replace("_", " ");
                            res.ErrorCode = DMTErrorCodes.Sender_Created_Successfully;
                        }
                        else
                        {
                            res.Msg = nameof(ErrorCodes.Invalid_OTP).Replace("_", " ");
                            res.ErrorCode = ErrorCodes.Invalid_OTP;
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
                    FuncName = "VerifySender",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = request.UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            var dMTReq = new DMTReqRes
            {
                APIID = _APIID,
                Method = "VerifySender",
                RequestModeID = request.RequestMode,
                Request = URL + JsonConvert.SerializeObject(eKORequest) + JsonConvert.SerializeObject(headers) + "?" + PutData,
                Response = resp,
                SenderNo = request.SenderMobile,
                UserID = request.UserID,
                TID = request.TransactionID
            };
            new ProcUpdateLogDMRReqResp(_dal).Call(dMTReq);
            return res;
        }

        public MSenderLoginResponse RemoveBeneficiary(MTAPIRequest request)
        {
            var res = new MSenderLoginResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = nameof(ErrorCodes.Unknown_Error).Replace("_", " "),
                ErrorCode = ErrorCodes.Unknown_Error
            };
            var eKORequest = new EKORequest
            {
                customer_id = request.SenderMobile,
                recipient_id = request.mBeneDetail.BeneID
            };
            string ekoTimeStamp = EKOAPIHelper.O.GetSecretKeyTimeStamp();
            string URL = _eKOAppSetting.BaseURL + "customers/mobile_number:" + eKORequest.customer_id + "/recipients/recipient_id:" + eKORequest.recipient_id;
            var headers = new Dictionary<string, string>
            {
                { EKOAPIHelper.SecretKeyTimeStamp, ekoTimeStamp },
                { EKOAPIHelper.SecretKey, EKOAPIHelper.O.GetSecretKey(ekoTimeStamp,_eKOAppSetting.EKOKey) },
                { ContentType.Self,ContentType.x_wwww_from_urlencoded},
                { EKOAPIHelper.DeveloperKey, _eKOAppSetting.DeveloperKey }
            };
            string PutData = "initiator_id=" + (_eKOAppSetting.InitiatorKey ?? string.Empty);

            string resp = "";
            try
            {
                resp = AppWebRequest.O.HWRDELETEAsync(URL, PutData, headers).Result;
                var ekoResp = JsonConvert.DeserializeObject<EKOClassess>(resp);
                if (ekoResp != null)
                {
                    if (ekoResp.status != null)
                    {
                        if (ekoResp.status == 29 && ekoResp.response_type_id == 29)
                        {
                            res.Msg = nameof(DMTErrorCodes.Invalid_Beneficiary_ID).Replace("_", " ");
                            res.ErrorCode = DMTErrorCodes.Invalid_Beneficiary_ID;
                        }
                        if (ekoResp.status == 138 && ekoResp.response_type_id == -1)
                        {
                            res.Msg = nameof(DMTErrorCodes.Invalid_Beneficiary_ID).Replace("_", " ");
                            res.ErrorCode = DMTErrorCodes.Invalid_Beneficiary_ID;
                        }
                        if (ekoResp.status == 0 && ekoResp.response_type_id == 27)
                        {
                            res.Statuscode = ErrorCodes.One;
                            res.Msg = nameof(DMTErrorCodes.Beneficiary_Deactivated_or_Deleted).Replace("_", " ");
                            res.ErrorCode = DMTErrorCodes.Invalid_Beneficiary_ID;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                resp = " Exception:" + ex.Message + " | " + resp;
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
                APIID = _APIID,
                Method = "RemoveBeneficiary",
                RequestModeID = request.RequestMode,
                Request = URL + JsonConvert.SerializeObject(eKORequest) + JsonConvert.SerializeObject(headers) + "?" + PutData,
                Response = resp,
                SenderNo = request.SenderMobile,
                UserID = request.UserID,
                TID = request.TransactionID
            });
            return res;
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
            var eKORequest = new EKORequest
            {
                customer_id = request.SenderMobile,
                account = request.mBeneDetail.AccountNo,
                client_ref_id = request.TID.ToString(),
                ifsc = request.mBeneDetail.IFSC
            };
            //EKORequest eKORequest, bool IsByBankCode
            string ekoTimeStamp = EKOAPIHelper.O.GetSecretKeyTimeStamp();
            string URL = string.Empty;
            bool IsByBankCode = false;//Currently Verification with IFSC
            if (IsByBankCode)
                URL = _eKOAppSetting.BaseURL + "banks/bank_code:" + eKORequest.bank_code + "/accounts/" + eKORequest.account;
            else
                URL = _eKOAppSetting.BaseURL + "banks/ifsc:" + eKORequest.ifsc + "/accounts/" + eKORequest.account;

            var headers = new Dictionary<string, string>
            {
                { EKOAPIHelper.SecretKeyTimeStamp, ekoTimeStamp },
                { EKOAPIHelper.SecretKey, EKOAPIHelper.O.GetSecretKey(ekoTimeStamp,_eKOAppSetting.EKOKey) },
                { ContentType.Self,ContentType.x_wwww_from_urlencoded},
                { EKOAPIHelper.DeveloperKey, _eKOAppSetting.DeveloperKey }
            };
            string PostData = "initiator_id=" + (_eKOAppSetting.InitiatorKey ?? string.Empty) + "&customer_id=" + eKORequest.customer_id + "&client_ref_id=" + eKORequest.client_ref_id;

            string response = string.Empty;
            try
            {
                response = AppWebRequest.O.HWRPostAsync(URL, PostData, headers).Result;
                var ekoResp = JsonConvert.DeserializeObject<EKOClassess>(response);
                if (ekoResp != null)
                {
                    if (ekoResp.status != null)
                    {
                        if (ekoResp.status == 463 && ekoResp.response_type_id == -1 || ekoResp.status.In(132, 319))
                        {
                            res.Statuscode = RechargeRespType.FAILED;
                            res.Msg = nameof(DMTErrorCodes.Sender_Not_Found).Replace("_", " ");
                            res.ErrorCode = DMTErrorCodes.Sender_Not_Found;
                            res.LiveID = res.Msg;
                        }
                        else if (ekoResp.status == 31)
                        {
                            res.Statuscode = RechargeRespType.FAILED;
                            res.Msg = nameof(DMTErrorCodes.Invalid_Account_Number).Replace("_", " ");
                            res.ErrorCode = DMTErrorCodes.Invalid_Account_Number;
                            res.LiveID = res.Msg;
                        }
                        else if (ekoResp.status == 39 && ekoResp.response_type_id == -1 || ekoResp.status == 314)
                        {
                            res.Statuscode = RechargeRespType.FAILED;
                            res.Msg = nameof(DMTErrorCodes.You_can_not_add_more_beneficiary).Replace("_", " ");
                            res.ErrorCode = DMTErrorCodes.You_can_not_add_more_beneficiary;
                            res.LiveID = res.Msg;
                        }
                        else if (ekoResp.status == 0 && ekoResp.response_type_id == 345)
                        {
                            res.Statuscode = RechargeRespType.FAILED;
                            res.Msg = nameof(DMTErrorCodes.Beneficiary_Already_Exist).Replace("_", " ");
                            res.ErrorCode = DMTErrorCodes.Beneficiary_Already_Exist;
                            res.LiveID = res.Msg;
                        }
                        else if (ekoResp.status == 350 || (ekoResp.status == 0 && ekoResp.response_type_id == 22))
                        {
                            res.Statuscode = RechargeRespType.FAILED;
                            res.Msg = nameof(DMTErrorCodes.Unbale_to_Verify_Beneficiary_Please_check_your_data_and_Verify_again).Replace("_", " ");
                            res.ErrorCode = DMTErrorCodes.Unbale_to_Verify_Beneficiary_Please_check_your_data_and_Verify_again;
                            res.LiveID = res.Msg;
                        }
                        else if (ekoResp.status == 0 && ekoResp.response_type_id == 61)
                        {
                            res.Statuscode = RechargeRespType.SUCCESS;
                            res.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                            res.ErrorCode = ErrorCodes.Transaction_Successful;
                            res.VendorID = ekoResp.data.tid ?? string.Empty;
                            res.BeneName = ekoResp.data.recipient_name ?? string.Empty;
                            res.LiveID = ekoResp.data.account ?? string.Empty;
                        }
                        else if (ekoResp.status == 44 && ekoResp.response_type_id == 44 || (ekoResp.status.In(41, 45, 136, 508, 536, 537) || (ekoResp.status == 541 && ekoResp.response_type_id == -1)))
                        {
                            res.Statuscode = RechargeRespType.FAILED;
                            res.Msg = nameof(DMTErrorCodes.Please_enter_correct_IFSC_code).Replace("_", " ");
                            res.ErrorCode = DMTErrorCodes.Please_enter_correct_IFSC_code;
                            res.LiveID = res.Msg;
                        }
                        else if (ekoResp.status == 48)
                        {
                            res.Statuscode = RechargeRespType.FAILED;
                            res.Msg = nameof(DMTErrorCodes.Invalid_bank).Replace("_", " ");
                            res.ErrorCode = DMTErrorCodes.Invalid_bank;
                            res.LiveID = res.Msg;
                        }
                        else if (ekoResp.status == 102)
                        {
                            res.Statuscode = RechargeRespType.FAILED;
                            res.Msg = nameof(DMTErrorCodes.Invalid_Account_Number).Replace("_", " ");
                            res.ErrorCode = DMTErrorCodes.Invalid_Account_Number;
                            res.LiveID = res.Msg;
                        }
                        else if (ekoResp.status == 46 && ekoResp.response_status_id == 1)
                        {
                            res.Statuscode = RechargeRespType.FAILED;
                            res.Msg = nameof(DMTErrorCodes.Service_Error_Transaction_is_Not_Permitted_to_account).Replace("_", " ");
                            res.ErrorCode = DMTErrorCodes.Service_Error_Transaction_is_Not_Permitted_to_account;
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
                    FuncName = "VerifyAccount",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = request.UserID
                });
            }
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
                LiveID = "",
                ErrorCode = ErrorCodes.Request_Accpeted
            };
            var eKORequest = new EKORequest
            {
                customer_id = request.SenderMobile,
                recipient_id = request.mBeneDetail.BeneID,
                amount = request.Amount,
                channel = request.TransMode.Equals("IMPS") ? 2 : 1,
                state = 1,
                timestamp = DateTimeOffset.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'"),
                hold_timeout = 100,
                merchant_document_id_type = 1,
                merchant_document_id = res.PanNo,
                pincode = res.Pincode,
                latlong = res.LatLong,
                client_ref_id = res.TID.ToString()
            };
            string ekoTimeStamp = EKOAPIHelper.O.GetSecretKeyTimeStamp();
            string URL = _eKOAppSetting.BaseURL + "transactions";
            var headers = new Dictionary<string, string>
            {
                { EKOAPIHelper.SecretKeyTimeStamp, ekoTimeStamp },
                { EKOAPIHelper.SecretKey, EKOAPIHelper.O.GetSecretKey(ekoTimeStamp,_eKOAppSetting.EKOKey) },
                { ContentType.Self,ContentType.x_wwww_from_urlencoded},
                { EKOAPIHelper.DeveloperKey, _eKOAppSetting.DeveloperKey }
            };
            string PostData = "recipient_id=" + eKORequest.recipient_id + "&amount=" + eKORequest.amount + "&initiator_id=" + (_eKOAppSetting.InitiatorKey ?? string.Empty) + "&customer_id=" + eKORequest.customer_id + "&timestamp=" + eKORequest.timestamp + "&currency=INR&customer_id=" + eKORequest.cust_id + "&client_ref_id=" + eKORequest.client_ref_id + "&hold_timeout=" + eKORequest.hold_timeout + "&state=" + eKORequest.state + "&channel=" + eKORequest.channel + "&merchant_document_id_type=" + eKORequest.merchant_document_id_type + "&merchant_document_id=" + eKORequest.merchant_document_id + "&pincode=" + eKORequest.pincode + "&latlong=" + eKORequest.latlong;
            if (eKORequest.channel == 1)
            {
                PostData += "&ifsc=" + eKORequest.ifsc;
            }
            string response = string.Empty;
            try
            {
                response = AppWebRequest.O.HWRPostAsync(URL, PostData, headers).Result;
                var ekoResp = JsonConvert.DeserializeObject<EKOClassess>(response);
                if (ekoResp != null)
                {
                    if (ekoResp.status != null)
                    {
                        if (ekoResp.status == 1217 && ekoResp.response_type_id == -1)
                        { 
                            res.Statuscode = RechargeRespType.FAILED;
                            res.Msg = nameof(DMTErrorCodes.KYC_Not_Completed_Please_complete_your_KYC_to_use_this_Service).Replace("_", " ");
                            res.ErrorCode = DMTErrorCodes.KYC_Not_Completed_Please_complete_your_KYC_to_use_this_Service;
                            res.LiveID = res.Msg;
                        }
                        else if ((ekoResp.status == 463 && ekoResp.response_type_id == -1) || ekoResp.status == 319)
                        {
                            res.Statuscode = RechargeRespType.FAILED;
                            res.Msg = nameof(DMTErrorCodes.Sender_Not_Found).Replace("_", " ");
                            res.ErrorCode = DMTErrorCodes.Sender_Not_Found;
                            res.LiveID = res.Msg;
                        }
                        else if (ekoResp.status == 46)
                        {
                            res.Statuscode = RechargeRespType.FAILED;
                            res.Msg = nameof(DMTErrorCodes.Service_Error_Transaction_is_Not_Permitted_to_account).Replace("_", " ");
                            res.ErrorCode = DMTErrorCodes.Service_Error_Transaction_is_Not_Permitted_to_account;
                            res.LiveID = res.Msg;
                        }
                        else if (ekoResp.status == 48)
                        {
                            res.Statuscode = RechargeRespType.FAILED;
                            res.Msg = nameof(DMTErrorCodes.Invalid_bank).Replace("_", " ");
                            res.ErrorCode = DMTErrorCodes.Invalid_bank;
                            res.LiveID = res.Msg;
                        }
                        else if (ekoResp.status == 317)
                        {
                            res.Statuscode = RechargeRespType.FAILED;
                            res.Msg = nameof(DMTErrorCodes.NEFT_not_allowed).Replace("_", " ");
                            res.ErrorCode = DMTErrorCodes.NEFT_not_allowed;
                            res.LiveID = res.Msg;
                        }
                        else if (ekoResp.status.In(53, 344))
                        {
                            res.Statuscode = RechargeRespType.FAILED;
                            res.Msg = nameof(DMTErrorCodes.IMPS_not_allowed).Replace("_", " ");
                            res.ErrorCode = DMTErrorCodes.IMPS_not_allowed;
                            res.LiveID = res.Msg;
                        }
                        else if (ekoResp.status == 460)
                        {
                            res.Statuscode = RechargeRespType.FAILED;
                            res.Msg = nameof(DMTErrorCodes.Invalid_Channel).Replace("_", " ");
                            res.ErrorCode = DMTErrorCodes.Invalid_Channel;
                            res.LiveID = res.Msg;
                        }
                        else if (ekoResp.status == 521)
                        {
                            res.Statuscode = RechargeRespType.FAILED;
                            res.Msg = nameof(DMTErrorCodes.Please_enter_correct_IFSC_code).Replace("_", " ");
                            res.ErrorCode = DMTErrorCodes.Please_enter_correct_IFSC_code;
                            res.LiveID = res.Msg;
                        }
                        else if (ekoResp.status.In(314, 945))
                        {
                            res.Statuscode = RechargeRespType.FAILED;
                            res.Msg = nameof(DMTErrorCodes.Senders_Monthly_Or_Daily_limit_breached).Replace("_", " ");
                            res.ErrorCode = DMTErrorCodes.Senders_Monthly_Or_Daily_limit_breached;
                            res.LiveID = res.Msg;
                        }
                        else if (ekoResp.status == 313 && ekoResp.response_type_id == -1)
                        {
                            res.Statuscode = RechargeRespType.FAILED;
                            res.Msg = nameof(DMTErrorCodes.Beneficiary_not_found).Replace("_", " ");
                            res.ErrorCode = DMTErrorCodes.Beneficiary_not_found;
                            res.LiveID = res.Msg;
                        }
                        else if ((ekoResp.status == 55 && ekoResp.response_type_id == 55) || ekoResp.status == 347)
                        {
                            res.Statuscode = RechargeRespType.PENDING;
                            res.Msg = nameof(ErrorCodes.Request_Accpeted).Replace("_", " ");
                            res.ErrorCode = ErrorCodes.Request_Accpeted;
                            res.LiveID = res.Msg;
                        }
                        else if (ekoResp.status == 0 && ekoResp.response_type_id == 325)
                        {
                            res.VendorID = ekoResp.data.tid ?? "";
                            res.LiveID = ekoResp.data.bank_ref_num ?? "";
                            //res. = ekoResp.data.sender_name ?? "";
                            int ekoTxStatus = Convert.ToInt16(Validators.Validate.O.IsNumeric(ekoResp.data.tx_status ?? string.Empty) ? ekoResp.data.tx_status : "-1");
                            if (ekoTxStatus == 0)
                            {
                                res.Statuscode = RechargeRespType.SUCCESS;
                                res.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                                res.ErrorCode = ErrorCodes.Transaction_Successful;
                            }
                        }
                        //if (ekoResp.status == 544 && ekoResp.response_type_id == 544)
                        //{
                        //    //DateTime dt = DateTime.Now;
                        //    //res.STATUS = RechargeRespType._PENDING;
                        //    //if ((dt.Hour >= 21 && dt.Hour <= 23))
                        //    //{
                        //    //    res.STATUS = RechargeRespType._FAILED;
                        //    //}

                        //    //res.MSG = "Bank is not available!";
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
            res.Request = URL + JsonConvert.SerializeObject(eKORequest) + JsonConvert.SerializeObject(headers) + "?" + PostData;
            res.Response = response;
            new ProcUpdateLogDMRReqResp(_dal).Call(new DMTReqRes
            {
                APIID = request.APIID,
                Method = "AccountTransfer",
                RequestModeID = request.RequestMode,
                Request = res.Request,
                Response = response,
                SenderNo = request.SenderMobile,
                UserID = request.UserID,
                TID = request.TID.ToString()
            });
            return res;
        }
        public MSenderLoginResponse RemoveBeneficiaryValidate(MTAPIRequest request)
        {
            throw new NotImplementedException();
        }
        public MSenderCreateResp SenderEKYC(MTAPIRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
