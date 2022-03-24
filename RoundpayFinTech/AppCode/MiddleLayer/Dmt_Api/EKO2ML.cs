using Fintech.AppCode.Configuration;
using Fintech.AppCode.DB;
using Fintech.AppCode.StaticModel;
using Fintech.AppCode.WebRequest;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Org.BouncyCastle.Ocsp;
using RoundpayFinTech.AppCode.DL;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.MoneyTransfer;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.ThirdParty.Eko;
using RoundpayFinTech.AppCode.ThirdParty.Roundpay;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Validators;

namespace RoundpayFinTech.AppCode.MiddleLayer.Dmt_Api
{
    public partial class EKO2ML
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        private readonly IConfiguration Configuration;
        private readonly IDAL _dal;
        private readonly EKOAppSetting _eKOAppSetting;
        private readonly int _APIID;
        public EKO2ML(IHttpContextAccessor accessor, IHostingEnvironment env, IDAL dal, int APIID, string APICode = "EKO2")
        {
            _accessor = accessor;
            _env = env;
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory());
            builder.AddJsonFile((_env.IsProduction() ? "appsettings.json" : "appsettings.Development.json"));
            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
            _dal = dal;
            _eKOAppSetting = EKO2AppSetting(APICode);
            _APIID = APIID;
        }
        private EKOAppSetting EKO2AppSetting(string APICode)
        {
            var setting = new EKOAppSetting();
            try
            {
                setting = new EKOAppSetting
                {
                    EKOKey = Configuration["DMR:"+ APICode + ":EKOKey"],
                    InitiatorKey = Configuration["DMR:" + APICode + ":InitiatorKey"],
                    DeveloperKey = Configuration["DMR:" + APICode + ":DeveloperKey"],
                    BaseURL = Configuration["DMR:" + APICode + ":BaseURL"],
                    OnBoardingURL = Configuration["DMR:" + APICode + ":OnBoardingURL"]
                };
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "EKO2AppSetting",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser
                });
            }
            return setting;
        }
        public OutletAPIStatusUpdate AgentOnboarding(EKO2OnboardRequest onboardRequest)
        {
            var OutReqRes = new OutletAPIStatusUpdate
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            if (onboardRequest != null)
            {

                StringBuilder sb = new StringBuilder(@"{""line"": ""{line}"",""city"":""{city}"",""state"":""{state}"",""pincode"":""{pincode}"",""district"":""{district}"",""area"":""{area}""}");
                sb.Replace("{line}", onboardRequest.line);
                sb.Replace("{city}", onboardRequest.city);
                sb.Replace("{state}", onboardRequest.state);
                sb.Replace("{pincode}", onboardRequest.pincode.ToString());
                sb.Replace("{district}", onboardRequest.district);
                sb.Replace("{area}", onboardRequest.area);


                onboardRequest.residence_address = sb.ToString();
                string ekoTimeStamp = EKOAPIHelper.O.GetSecretKeyTimeStamp();
                string URL = _eKOAppSetting.OnBoardingURL + "user/onboard";
                var headers = new Dictionary<string, string>
                {
                    { EKOAPIHelper.SecretKeyTimeStamp, ekoTimeStamp },
                    { EKOAPIHelper.SecretKey, EKOAPIHelper.O.GetSecretKey(ekoTimeStamp,_eKOAppSetting.EKOKey) },
                    { ContentType.Self,ContentType.x_wwww_from_urlencoded},
                    { EKOAPIHelper.DeveloperKey, _eKOAppSetting.DeveloperKey }
                };
                //1992 - 05 - 10


                StringBuilder sbPutData = new StringBuilder("initiator_id={initiator_id}&pan_number={pan_number}&mobile={mobile}&first_name={first_name}&last_name={last_name}&email={email}&residence_address={residence_address}&dob={dob}&shop_name={shop_name}");
                sbPutData.Replace("{initiator_id}", _eKOAppSetting.InitiatorKey ?? string.Empty);
                sbPutData.Replace("{pan_number}", onboardRequest.pan_number ?? string.Empty);
                sbPutData.Replace("{mobile}", onboardRequest.mobile ?? string.Empty);
                sbPutData.Replace("{first_name}", onboardRequest.first_name ?? string.Empty);
                sbPutData.Replace("{last_name}", onboardRequest.last_name ?? string.Empty);
                sbPutData.Replace("{email}", onboardRequest.email ?? string.Empty);
                sbPutData.Replace("{residence_address}", onboardRequest.residence_address ?? string.Empty);
                sbPutData.Replace("{dob}", onboardRequest.dob ?? string.Empty);
                sbPutData.Replace("{shop_name}", onboardRequest.shop_name ?? string.Empty);

                string Req = string.Format("{0}{1}?{2}", URL, JsonConvert.SerializeObject(headers), sbPutData.ToString());
                string Resp = string.Empty;

                try
                {
                    Resp = AppWebRequest.O.HWRPUTAsync(URL, sbPutData.ToString(), headers).Result;
                    if (!string.IsNullOrEmpty(Resp))
                    {
                        var ekoResp = JsonConvert.DeserializeObject<EKOClassess>(Resp);
                        if ((ekoResp.status ?? -1) == 0 && ekoResp.response_type_id == 1290)
                        {
                            OutReqRes.Statuscode = ErrorCodes.One;
                            OutReqRes.Msg = ErrorCodes.OutletRegistered;
                            OutReqRes.APIOutletID = ekoResp.data.user_code;
                            OutReqRes.APIOutletStatus = RndKYCOutLetStatus._Approved;
                            OutReqRes.KYCStatus = RndKYCOutLetStatus._Approved;

                            OutReqRes.BBPSID = ekoResp.data.user_code;
                            OutReqRes.BBPSStatus = RndKYCOutLetStatus._NotApplied;
                            OutReqRes.AEPSID = ekoResp.data.user_code;
                            OutReqRes.AEPSStatus = RndKYCOutLetStatus._NotApplied;
                            OutReqRes.DMTID = ekoResp.data.user_code;
                            OutReqRes.DMTStatus = RndKYCOutLetStatus._Approved;
                            OutReqRes.PSAID = string.Empty;
                            OutReqRes.PSAStatus = RndKYCOutLetStatus._NotApplied;
                        }
                        else
                        {
                            OutReqRes.APIOutletStatus = RndKYCOutLetStatus._Rejected;
                            OutReqRes.BBPSStatus = RndKYCOutLetStatus._Rejected;
                            OutReqRes.AEPSStatus = RndKYCOutLetStatus._Rejected;
                            OutReqRes.DMTStatus = RndKYCOutLetStatus._Rejected;
                            OutReqRes.Msg = string.IsNullOrEmpty(ekoResp.message) ? ErrorCodes.FailedToSubmit : ekoResp.message;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Resp = " Exception:" + ex.Message + " | " + Resp;
                    var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                    {
                        ClassName = GetType().Name,
                        FuncName = "AgentOnboarding",
                        Error = ex.Message,
                        LoginTypeID = 1,
                        UserId = 1
                    });
                }
                #region APILogOutletRegistration
                new ProcUpdateLogOnboardingReqResp(_dal).Call(new OnboardingLog
                {
                    APIID = _APIID,
                    Method = "AgentOnboarding",
                    Request = Req,
                    Response = Resp,

                });
                #endregion
            }
            return OutReqRes;
        }
    }
    public partial class EKO2ML : IMoneyTransferAPIML
    {
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
                customer_id = request.SenderMobile,
                user_code = request.APIOutletID
            };
            string ekoTimeStamp = EKOAPIHelper.O.GetSecretKeyTimeStamp();
            StringBuilder sbURL = new StringBuilder("{BaseURL}customers/mobile_number:{mobile_number}?initiator_id={initiator_id}&user_code={user_code}");
            sbURL.Replace("{BaseURL}", _eKOAppSetting.BaseURL);
            sbURL.Replace("{mobile_number}", eKORequest.customer_id);
            sbURL.Replace("{initiator_id}", _eKOAppSetting.InitiatorKey);
            sbURL.Replace("{user_code}", eKORequest.user_code);

            var headers = new Dictionary<string, string>
            {
                { EKOAPIHelper.SecretKeyTimeStamp, ekoTimeStamp },
                { EKOAPIHelper.SecretKey, EKOAPIHelper.O.GetSecretKey(ekoTimeStamp,_eKOAppSetting.EKOKey) },
                { EKOAPIHelper.DeveloperKey, _eKOAppSetting.DeveloperKey }
            };

            string resp = string.Empty;
            try
            {
                resp = AppWebRequest.O.CallUsingHttpWebRequest_GETAsync(sbURL.ToString(), headers).Result;
                var ekoResp = JsonConvert.DeserializeObject<EKOClassess>(resp);
                if (ekoResp == null)
                {
                    res.Msg = nameof(DMTErrorCodes.Sender_Not_Found).Replace("_", " ");
                    res.ErrorCode = DMTErrorCodes.Sender_Not_Found;
                    res.IsSenderNotExists = true;
                }
                else if (ekoResp.status == null || ekoResp.message.Contains("Verification pending"))
                {
                    res.Msg = nameof(DMTErrorCodes.Sender_Not_Found).Replace("_", " ");
                    res.ErrorCode = DMTErrorCodes.Sender_Not_Found;
                    res.IsSenderNotExists = true;
                }
                else if (ekoResp.status == 463 || ekoResp.response_type_id == -1 || ekoResp.message.Contains("Verification pending"))
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
                        res.RemainingLimit = Convert.ToDecimal(ekoResp.data.available_limit - ekoResp.data.used_limit);
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
                APIID = _APIID,
                Method = "GetSender",
                RequestModeID = request.RequestMode,
                Request = sbURL.ToString() + JsonConvert.SerializeObject(eKORequest) + JsonConvert.SerializeObject(headers),
                Response = resp,
                SenderNo = request.SenderMobile,
                UserID = request.UserID,
                TID = request.TransactionID
            });
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
                bank_id = BankDetail.EKO_BankID + string.Empty,
                user_code = request.APIOutletID
            };
            string ekoTimeStamp = EKOAPIHelper.O.GetSecretKeyTimeStamp();
            string URL = string.Format("{0}customers/mobile_number:{1}/recipients/{2}:{3}", _eKOAppSetting.BaseURL, eKORequest.customer_id, eKORequest.recipient_id_type, eKORequest.id);
            var headers = new Dictionary<string, string>
            {
                { EKOAPIHelper.SecretKeyTimeStamp, ekoTimeStamp },
                { EKOAPIHelper.SecretKey, EKOAPIHelper.O.GetSecretKey(ekoTimeStamp,_eKOAppSetting.EKOKey) },
                { ContentType.Self,ContentType.x_wwww_from_urlencoded},
                { EKOAPIHelper.DeveloperKey, _eKOAppSetting.DeveloperKey }
            };
            string PutData = string.Format("recipient_name={0}&initiator_id={1}&recipient_mobile={2}&recipient_type={3}&bank_id={4}&user_code={5}", eKORequest.recipient_name, (_eKOAppSetting.InitiatorKey ?? string.Empty), eKORequest.recipient_mobile, eKORequest.recipient_type, eKORequest.bank_id, eKORequest.user_code);

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
            StringBuilder sb = new StringBuilder(@"{""line"":""{line}"",""city"":""{city}"",""state"":""{state}"",""pincode"":""{pincode}"",""district"":""{district}"",""area"":""{area}""}");
            sb.Replace("{line}", request.Area);
            sb.Replace("{city}", request.City);
            sb.Replace("{state}", request.StateName);
            sb.Replace("{pincode}", request.Pincode.ToString());
            sb.Replace("{district}", request.Districtname);
            sb.Replace("{area}", request.Area);
            var eKORequest = new EKORequest
            {
                customer_id = request.SenderMobile ?? string.Empty,
                name = string.Format("{0} {1}", request.FirstName, request.LastName),
                residence_address = sb.ToString()
            };
            string ekoTimeStamp = EKOAPIHelper.O.GetSecretKeyTimeStamp();
            string URL = string.Format("{0}customers/mobile_number:{1}", _eKOAppSetting.BaseURL, eKORequest.customer_id);
            var headers = new Dictionary<string, string>
            {
                { EKOAPIHelper.SecretKeyTimeStamp, ekoTimeStamp },
                { EKOAPIHelper.SecretKey, EKOAPIHelper.O.GetSecretKey(ekoTimeStamp,_eKOAppSetting.EKOKey) },
                { ContentType.Self,ContentType.x_wwww_from_urlencoded},
                { EKOAPIHelper.DeveloperKey, _eKOAppSetting.DeveloperKey }
            };
            string PutData = string.Format("initiator_id={0}&user_code={1}&pipe=9&name={2}&residence_address={3}&dob={4}", (_eKOAppSetting.InitiatorKey ?? string.Empty), request.APIOutletID, eKORequest.name, eKORequest.residence_address, Convert.ToDateTime(request.DOB).ToString("yyyy-MM-dd"));
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
                        if (ekoResp.status == 0 && ekoResp.response_type_id == 327)
                        {
                            if (ekoResp.data != null)
                            {
                                if (!string.IsNullOrEmpty(ekoResp.data.otp_ref_id))
                                {
                                    res.Statuscode = ErrorCodes.One;
                                    res.Msg = nameof(DMTErrorCodes.Partial_Registration_Please_verify_sender_with_OTP).Replace("_", " ");
                                    res.IsOTPGenerated = true;
                                    res.ErrorCode = DMTErrorCodes.Partial_Registration_Please_verify_sender_with_OTP;
                                    res.ReferenceID = ekoResp.data.otp_ref_id;
                                    res.IsOTPResendAvailble = true;
                                }
                            }
                        }
                        else if (ekoResp.status == 17 && ekoResp.response_type_id == -1)
                        {
                            res.Msg = nameof(DMTErrorCodes.Sender_Already_Exist).Replace("_", " ");
                            res.ErrorCode = DMTErrorCodes.Sender_Already_Exist;
                        }
                        else if (ekoResp.status == 612 && ekoResp.response_type_id == 327)
                        {
                            res.Msg = ekoResp.message;
                            res.ErrorCode = ErrorCodes.Unknown_Error;
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
                Request = URL + JsonConvert.SerializeObject(headers) + "?" + PutData.ToString(),
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
            var PostData = new StringBuilder();
            PostData.Append("initiator_id={initiator_id}&user_code={user_code}&pipe=9");
            PostData.Replace("{initiator_id}", (_eKOAppSetting.InitiatorKey ?? string.Empty));
            PostData.Replace("{user_code}", (request.APIOutletID ?? string.Empty));

            string resp = string.Empty;
            try
            {
                resp = AppWebRequest.O.HWRPostAsync(URL, PostData.ToString(), headers).Result;
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
                            res.ReferenceID = ekoResp.data.otp_ref_id;
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
                Request = URL + JsonConvert.SerializeObject(headers) + "?" + PostData.ToString(),
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
            string URL = _eKOAppSetting.BaseURL + "customers/mobile_number:" + eKORequest.customer_id + "/recipients?initiator_id=" + _eKOAppSetting.InitiatorKey + "&user_code=" + (request.APIOutletID ?? string.Empty);

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
            var PostData = new StringBuilder();
            PostData.Append("initiator_id={initiator_id}&user_code={user_code}&pipe=9");
            PostData.Replace("{initiator_id}", (_eKOAppSetting.InitiatorKey ?? string.Empty));
            PostData.Replace("{user_code}", (request.APIOutletID ?? string.Empty));

            string resp = string.Empty;
            try
            {
                resp = AppWebRequest.O.HWRPostAsync(URL, PostData.ToString(), headers).Result;
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
                    FuncName = "SenderResendOTP",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = request.UserID
                });
            }
            new ProcUpdateLogDMRReqResp(_dal).Call(new DMTReqRes
            {
                APIID = _APIID,
                Method = "SenderResendOTP",
                RequestModeID = request.RequestMode,
                Request = URL + JsonConvert.SerializeObject(headers) + "?" + PostData.ToString(),
                Response = resp,
                SenderNo = request.SenderMobile,
                UserID = request.UserID,
                TID = request.TransactionID
            });
            return res;
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
                otp = request.OTP,
                otp_ref_id = request.ReferenceID??string.Empty
            };
            string ekoTimeStamp = EKOAPIHelper.O.GetSecretKeyTimeStamp();
            var URL = string.Format("{0}customers/verification/otp:{1}", _eKOAppSetting.BaseURL, eKORequest.otp);
            var headers = new Dictionary<string, string>
            {
                { EKOAPIHelper.SecretKeyTimeStamp, ekoTimeStamp },
                { EKOAPIHelper.SecretKey, EKOAPIHelper.O.GetSecretKey(ekoTimeStamp,_eKOAppSetting.EKOKey) },
                { ContentType.Self,ContentType.x_wwww_from_urlencoded},
                { EKOAPIHelper.DeveloperKey, _eKOAppSetting.DeveloperKey }
            };

            StringBuilder sbPutData = new StringBuilder("initiator_id={initiator_id}&id_type=mobile_number&id={id}&user_code={user_code}&pipe=9&otp_ref_id={otp_ref_id}");
            sbPutData.Replace("{initiator_id}" ,(_eKOAppSetting.InitiatorKey ?? string.Empty));
            sbPutData.Replace("{id}",eKORequest.customer_id);
            sbPutData.Replace("{user_code}", request.APIOutletID??string.Empty);
            sbPutData.Replace("{otp_ref_id}",(eKORequest.otp_ref_id ?? string.Empty));
            
            string resp = string.Empty;
            try
            {
                resp = AppWebRequest.O.HWRPUTAsync(URL, sbPutData.ToString(), headers).Result;
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
                            if (ekoResp.data.state.Equals("2"))
                            {
                                res.Statuscode = ErrorCodes.One;
                                res.Msg = nameof(DMTErrorCodes.Sender_Created_Successfully).Replace("_", " ");
                                res.ErrorCode = DMTErrorCodes.Sender_Created_Successfully;
                            }
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
            new ProcUpdateLogDMRReqResp(_dal).Call(new DMTReqRes
            {
                APIID = _APIID,
                Method = "VerifySender",
                RequestModeID = request.RequestMode,
                Request = URL + JsonConvert.SerializeObject(eKORequest) + JsonConvert.SerializeObject(headers) + "?" + sbPutData.ToString(),
                Response = resp,
                SenderNo = request.SenderMobile,
                UserID = request.UserID,
                TID = request.TransactionID
            });
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
            StringBuilder sbPutData = new StringBuilder("initiator_id={initiator_id}&user_code={user_code}");
            sbPutData.Replace("{initiator_id}", (_eKOAppSetting.InitiatorKey ?? string.Empty));
            sbPutData.Replace("{user_code}", request.APIOutletID);
            string resp = "";
            try
            {
                resp = AppWebRequest.O.HWRDELETEAsync(URL, sbPutData.ToString(), headers).Result;
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
                Request = URL + JsonConvert.SerializeObject(eKORequest) + JsonConvert.SerializeObject(headers) + "?" + sbPutData.ToString(),
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
            StringBuilder sbPostData = new StringBuilder("initiator_id={initiator_id}&customer_id={customer_id}&client_ref_id={client_ref_id}&user_code={user_code}");
            sbPostData.Replace("{initiator_id}", (_eKOAppSetting.InitiatorKey ?? string.Empty));
            sbPostData.Replace("{customer_id}", eKORequest.customer_id);
            sbPostData.Replace("{client_ref_id}", eKORequest.client_ref_id);
            sbPostData.Replace("{user_code}", request.APIOutletID);

            string response = string.Empty;
            try
            {
                response = AppWebRequest.O.HWRPostAsync(URL, sbPostData.ToString(), headers).Result;
                var ekoResp = JsonConvert.DeserializeObject<EKOClassess>(response);
                if (ekoResp != null)
                {
                    if (ekoResp.status != null)
                    {
                        if (ekoResp.status == 0 && ekoResp.response_type_id == 61)
                        {
                            res.Statuscode = RechargeRespType.SUCCESS;
                            res.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                            res.ErrorCode = ErrorCodes.Transaction_Successful;
                            res.VendorID = ekoResp.data.tid ?? string.Empty;
                            res.BeneName = ekoResp.data.recipient_name ?? string.Empty;
                            res.LiveID = ekoResp.data.account ?? string.Empty;
                        }
                        else
                        {
                            IErrorCodeML errorCodeML = new ErrorCodeML(_accessor, _env, false);
                            var eFromDB = errorCodeML.GetAPIErrorCodeDescription(request.APIGroupCode, nameof(ekoResp.status) + ekoResp.status + "_" + ekoResp.response_type_id);
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
                Request = URL + JsonConvert.SerializeObject(headers) + sbPostData.ToString(),
                Response = response,
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
                pincode = request.Pincode.ToString(),
                latlong = string.Format("{0},{1}", request.Lattitude.PadRight(11, '0'), request.Longitude.PadRight(11, '0')),
                client_ref_id = request.TID.ToString(),
                user_code = request.APIOutletID
            };
            string ekoTimeStamp = EKOAPIHelper.O.GetSecretKeyTimeStamp();
            string URL = string.Format("{0}transactions", _eKOAppSetting.BaseURL);
            var headers = new Dictionary<string, string>
            {
                { EKOAPIHelper.SecretKeyTimeStamp, ekoTimeStamp },
                { EKOAPIHelper.SecretKey, EKOAPIHelper.O.GetSecretKey(ekoTimeStamp,_eKOAppSetting.EKOKey) },
                { ContentType.Self,ContentType.x_wwww_from_urlencoded},
                { EKOAPIHelper.DeveloperKey, _eKOAppSetting.DeveloperKey }
            };
            StringBuilder sbPostData = new StringBuilder("recipient_id={recipient_id}&amount={amount}&timestamp={timestamp}&currency=INR&customer_id={customer_id}&initiator_id={initiator_id}&client_ref_id={client_ref_id}&state={state}&channel={channel}&latlong={latlong}&user_code={user_code}");
            sbPostData.Replace("{recipient_id}", eKORequest.recipient_id);
            sbPostData.Replace("{amount}", eKORequest.amount.ToString());
            sbPostData.Replace("{timestamp}", eKORequest.timestamp);
            sbPostData.Replace("{customer_id}", eKORequest.customer_id);
            sbPostData.Replace("{initiator_id}", (_eKOAppSetting.InitiatorKey ?? string.Empty));
            sbPostData.Replace("{client_ref_id}", eKORequest.client_ref_id);
            sbPostData.Replace("{state}", eKORequest.state.ToString());
            sbPostData.Replace("{channel}", eKORequest.channel.ToString());
            sbPostData.Replace("{latlong}", eKORequest.latlong);
            sbPostData.Replace("{user_code}", eKORequest.user_code);

            
            string response = string.Empty;
            try
            {
                response = AppWebRequest.O.HWRPostAsync(URL, sbPostData.ToString(), headers).Result;
                var ekoResp = JsonConvert.DeserializeObject<EKOClassess>(response);
                if (ekoResp != null)
                {
                    if (ekoResp.status != null)
                    {
                        if (ekoResp.status == 0)
                        {
                            res.VendorID = ekoResp.data.tid ?? "";
                            res.LiveID = ekoResp.data.bank_ref_num ?? "";
                            //res. = ekoResp.data.sender_name ?? "";
                            int ekoTxStatus = Convert.ToInt16(Validate.O.IsNumeric(ekoResp.data.tx_status ?? string.Empty) ? ekoResp.data.tx_status : "-1");
                            if (ekoTxStatus == 0)
                            {
                                res.Statuscode = RechargeRespType.SUCCESS;
                                res.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                                res.ErrorCode = ErrorCodes.Transaction_Successful;
                            }
                            else
                            {
                                IErrorCodeML errorCodeML = new ErrorCodeML(_accessor, _env, false);
                                var eFromDB = errorCodeML.GetAPIErrorCodeDescription(request.APIGroupCode, nameof(ekoResp.status) + ekoResp.status+"_"+ekoResp.response_type_id);
                                if (!string.IsNullOrEmpty(eFromDB.Code))
                                {
                                    res.Statuscode = eFromDB.Status;
                                    res.Msg = eFromDB.Error.Replace("{REPLACE}", ekoResp.message);
                                    res.ErrorCode = Convert.ToInt32(eFromDB.Code.Trim());
                                    res.LiveID = res.Msg;
                                }
                                res.Statuscode = res.Statuscode == 0 ? RechargeRespType.PENDING : res.Statuscode;
                            }
                        }
                        else
                        {
                            IErrorCodeML errorCodeML = new ErrorCodeML(_accessor, _env, false);
                            var eFromDB = errorCodeML.GetAPIErrorCodeDescription(request.APIGroupCode, nameof(ekoResp.status) + ekoResp.status + "_" + ekoResp.response_type_id);
                            if (!string.IsNullOrEmpty(eFromDB.Code))
                            {
                                res.Statuscode = eFromDB.Status;
                                res.Msg = eFromDB.Error.Replace("{REPLACE}", ekoResp.message);
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
            res.Request = URL + JsonConvert.SerializeObject(eKORequest) + JsonConvert.SerializeObject(headers) + "?" + sbPostData.ToString();
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
        public async Task<DMRTransactionResponse> GetTransactionStatus(int TID, int RequestMode, int UserID, string SenderNo, string VendorID,string APIOutletID,string APIGroupCode)
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
            string URL = _eKOAppSetting.BaseURL + "transactions/client_ref_id:" + eKORequest.client_ref_id + "?initiator_id=" + _eKOAppSetting.InitiatorKey + "&user_code=" + (APIOutletID??string.Empty);
            if ((VendorID ?? string.Empty).Length > 0)
            {
                URL = _eKOAppSetting.BaseURL + "transactions/" + eKORequest.id + "?initiator_id=" + _eKOAppSetting.InitiatorKey + "&user_code=" + (APIOutletID ?? string.Empty);
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
                            else if (ekoTxStatus == 4)
                            {
                                res.Statuscode = RechargeRespType.REFUND;
                                res.Msg = nameof(DMTErrorCodes.Transaction_Status_Updated_Refund).Replace("_", " ");
                                res.LiveID = ekoResp.data.bank_ref_num;
                                res.ErrorCode = DMTErrorCodes.Transaction_Status_Updated_Refund;
                            }
                        }
                        else
                        {
                            IErrorCodeML errorCodeML = new ErrorCodeML(_accessor, _env, false);
                            var eFromDB = errorCodeML.GetAPIErrorCodeDescription(APIGroupCode, nameof(ekoResp.status) + ekoResp.status + "_" + ekoResp.response_type_id);
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
            catch (Exception ex)
            {
                resp = " Exception:" + ex.Message + " | " + resp;
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "GetTransactionStatus",
                    Error = ex.Message,
                    LoginTypeID = 1,
                    UserId = UserID
                });
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
        public async Task<ResponseStatus> Refund(int TID, string VendorID, int RequestMode, int UserID, string SenderNo, string OTP, string APIOutletID,int State = 1)
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
            StringBuilder sbPostData = new StringBuilder("initiator_id={initiator_id}&otp={otp}&state={state}&client_ref_id={client_ref_id}&user_code={user_code}");
            sbPostData.Replace("{initiator_id}", (_eKOAppSetting.InitiatorKey ?? string.Empty));
            sbPostData.Replace("{otp}", eKORequest.otp);
            sbPostData.Replace("{state}", State.ToString());
            sbPostData.Replace("{client_ref_id}", eKORequest.client_ref_id);
            sbPostData.Replace("{user_code}", APIOutletID);
            
            string resp = string.Empty;
            try
            {
                resp = await AppWebRequest.O.HWRPostAsync(URL, sbPostData.ToString(), headers).ConfigureAwait(false);

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

        public async Task<ResponseStatus> ResendRefundOTP(string VendorID, int RequestMode, int UserID, string SenderNo, int TID,string APIOutletID)
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
            StringBuilder sbPutData = new StringBuilder("initiator_id={initiator_id}&user_code={user_code}");
            sbPutData.Replace("{initiator_id}", (_eKOAppSetting.InitiatorKey ?? string.Empty));
            sbPutData.Replace("{user_code}", (APIOutletID ?? string.Empty));

            string resp = string.Empty;
            try
            {
                resp = await AppWebRequest.O.HWRPostAsync(URL, sbPutData.ToString(), headers);
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
                resp = "Exception:" + ex.Message + " | " + resp;
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
    }
}
