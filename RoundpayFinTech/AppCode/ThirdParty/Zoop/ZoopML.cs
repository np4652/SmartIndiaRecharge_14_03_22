using Fintech.AppCode.DB;
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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.ThirdParty.Zoop
{
    public class ZoopML : IMoneyTransferAPIML
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        private readonly IConfiguration Configuration;
        private readonly ZoopAppSetting appSetting;
        private readonly IDAL _dal;
        private readonly int _APIID;
        public ZoopML(IHttpContextAccessor accessor, IHostingEnvironment env, IDAL dal, int APIID)
        {
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
        private ZoopAppSetting AppSetting()
        {
            try
            {
                return new ZoopAppSetting
                {
                    BaseURL = Configuration["EKYC:ZOOP:BaseURL"],
                    APIKey = Configuration["EKYC:ZOOP:APIKey"],
                    AppID = Configuration["EKYC:ZOOP:AppID"]
                };
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "ZoopAppSetting",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser
                });
            }
            return new ZoopAppSetting();
        }
        public EKYCByAadharProcReq AadharEKYCFromCallback(ZoopModelResponseWebhookDigilocker zoopModel)
        {
            var ekycReq = new EKYCByAadharProcReq
            {

            };
            string DOB = string.Empty;
            if (zoopModel != null)
            {
                if (zoopModel.success)
                {
                    ekycReq.VendorID = zoopModel.request_id;
                    if (zoopModel.result != null)
                    {
                        if (zoopModel.result.Count > 0)
                        {
                            var AddharResp = zoopModel.result.Where(x => x.doctype == "ADHAR").ToList();
                            if (AddharResp != null)
                            {
                                if (AddharResp.Count > 0)
                                {
                                    if (AddharResp[0].data_json != null)
                                    {
                                        if (AddharResp[0].data_json.KycRes != null)
                                        {
                                            if (AddharResp[0].data_json.KycRes.UidData != null)
                                            {
                                                ekycReq.APIStatus = true;
                                                ekycReq.HasImage = true;
                                                ekycReq.AadhaarNo = AddharResp[0].data_json.KycRes.UidData.uid;
                                                ekycReq.Country = AddharResp[0].data_json.KycRes.UidData.Poa.country;
                                                ekycReq.District = AddharResp[0].data_json.KycRes.UidData.Poa.dist;
                                                DOB = AddharResp[0].data_json.KycRes.UidData.Poi.dob;
                                                ekycReq.FullName = AddharResp[0].data_json.KycRes.UidData.Poi.name;
                                                ekycReq.Gender = AddharResp[0].data_json.KycRes.UidData.Poi.gender;
                                                ekycReq.House = AddharResp[0].data_json.KycRes.UidData.Poa.house;
                                                ekycReq.IsMobileVerified = false;
                                                ekycReq.Landmark = AddharResp[0].data_json.KycRes.UidData.Poa.lm;
                                                ekycReq.ParentName = AddharResp[0].data_json.KycRes.UidData.Poa.co;
                                                ekycReq.Pincode = AddharResp[0].data_json.KycRes.UidData.Poa.pc;
                                                ekycReq.PostOffice = AddharResp[0].data_json.KycRes.UidData.Poa.vtc;
                                                ekycReq.Profile = AddharResp[0].data_json.KycRes.UidData.Pht;
                                                ekycReq.State = AddharResp[0].data_json.KycRes.UidData.Poa.state;
                                                ekycReq.Street = AddharResp[0].data_json.KycRes.UidData.Poa.street;
                                                ekycReq.ShareCode = AddharResp[0].data_json.KycRes.code;
                                                ekycReq.SubDistrict = AddharResp[0].data_json.KycRes.UidData.Poa.dist;
                                                ekycReq.VTC = AddharResp[0].data_json.KycRes.UidData.Poa.vtc;
                                                ekycReq.DirectorName = ekycReq.FullName;
                                            }
                                        }

                                    }
                                }
                            }
                        }
                    }
                }
            }
            if (!string.IsNullOrEmpty(DOB))
            {
                var splitDob = DOB.Split("-");
                if (splitDob.Length == 3)
                {
                    DOB = splitDob[1] + "-" + splitDob[0] + "-" + splitDob[2];
                }
                ekycReq.DOB = Convert.ToDateTime(DOB).ToString("dd MMM yyyy");
            }
            return ekycReq;
        }
        public InitiateEKYCResponse InitiateCall(int InitiateID)
        {
            var res = new InitiateEKYCResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.Down
            };
            var headers = new Dictionary<string, string> {
                {"api-key",appSetting.APIKey },
                {"app-id",appSetting.AppID }
            };
            /**
             * "docs": [
                        "ADHAR",
                        "PANCR",
                        "RVCER",
                        "DRVLC"
                      ], // At least one doctype is required
             * ***/
            ILoginML loginML = new Fintech.AppCode.LoginML(_accessor, _env, false);
            var _WInfo = loginML.GetWebsiteInfo();

            //var WebHookURL = (_WInfo.WID == 1 ? _WInfo.AbsoluteHost : _WInfo.MainDomain)+"/Callback/ZoopWebSDKWebHook/{initiateid}";
            var WebHookURL = "https://roundpay.net/Callback/ZoopWebSDKWebHook/{initiateid}";
            WebHookURL = WebHookURL.Replace("{initiateid}", InitiateID.ToString());
            var datarequest = new
            {
                docs = new string[] { "ADHAR" },
                purpose = "For EKYC",
                response_url = WebHookURL
            };
            var _URL = (appSetting.BaseURL ?? string.Empty) + "identity/digilocker/v1/init";
            var Req = string.Empty;
            var Respon = string.Empty;
            try
            {
                Req = _URL + JsonConvert.SerializeObject(headers) + JsonConvert.SerializeObject(datarequest);
                var apiResp = AppWebRequest.O.PostJsonDataUsingHWRTLS(_URL, datarequest, headers).Result;
                Respon = apiResp;
                if (!string.IsNullOrEmpty(apiResp))
                {
                    var adharResp = JsonConvert.DeserializeObject<ZoopDgLockerResponse>(apiResp);
                    if (adharResp.success)
                    {
                        res.Statuscode = ErrorCodes.One;
                        res.Msg = ErrorCodes.SUCCESS;
                        res.VendorID = adharResp.request_id;
                        res.SecurityKey = adharResp.webhook_security_key;
                    }
                    else
                    {
                        res.Statuscode = ErrorCodes.Minus1;
                        res.Msg = adharResp.response_message;
                    }
                }
            }
            catch (Exception ex)
            {

                Respon = "Exception:" + ex.Message + Respon;
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "InitiateCall",
                    Error = ex.Message,
                    LoginTypeID = 1,
                    UserId = 1
                });
            }
            IProcedure _proc = new ProcLogEKYCAPIReqResp(_dal);
            _proc.Call(new CommonReq
            {
                CommonInt = _APIID,
                CommonStr = Req,
                CommonStr2 = Respon
            });
            return res;
        }
        public EKYCByGSTINModel ValidateGSTINAdvance(string GSTIN)
        {
            var res = new EKYCByGSTINModel
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.Down
            };
            var headers = new Dictionary<string, string> {
                {"api-key",appSetting.APIKey },
                {"app-id",appSetting.AppID }
            };
            var dataRequest = new
            {
                data = new
                {
                    business_gstin_number = GSTIN,
                    consent = "Y",
                    consent_text = "I hear by declare my consent agreement for fetching my information via ZOOP API"
                }
            };
            var _URL = (appSetting.BaseURL ?? string.Empty) + "api/v1/in/merchant/gstin/advance";
            var Req = string.Empty;
            var Respon = string.Empty;
            try
            {
                Req = _URL + JsonConvert.SerializeObject(headers) + JsonConvert.SerializeObject(dataRequest);
                var apiResp = AppWebRequest.O.PostJsonDataUsingHWRTLS(_URL, dataRequest, headers).Result;
                Respon = apiResp;
                if (!string.IsNullOrEmpty(apiResp))
                {
                    var gstResp = JsonConvert.DeserializeObject<ZoopModelResponseGST>(apiResp);
                    if (gstResp.success && gstResp.response_code == "100")
                    {
                        res.Statuscode = ErrorCodes.One;
                        res.Msg = ErrorCodes.SUCCESS;
                        res.GSTIN = GSTIN;
                        if (gstResp.result != null)
                        {
                            res.LegalName = gstResp.result.legal_name;
                            if (gstResp.result.contact != null)
                            {
                                res.MobileNo = gstResp.result.contact.mobile_no ?? string.Empty;
                                res.EmailID = gstResp.result.contact.email ?? string.Empty;
                            }
                            res.TradeName = gstResp.result.trade_name ?? string.Empty;
                            if (gstResp.result.authorized_signatory != null)
                            {
                                if (gstResp.result.authorized_signatory.Count > 0)
                                {
                                    res.AuthorisedSignatory = string.Join(",", gstResp.result.authorized_signatory);
                                }
                            }
                            res.AgreegateturnOver = gstResp.result.aggregate_turn_over;
                            res.CentralJurisdiction = gstResp.result.central_jurisdiction;
                            if (gstResp.result.primary_business_address != null)
                            {
                                res.Address = gstResp.result.primary_business_address.registered_address;
                                res.MobileNo = gstResp.result.primary_business_address.mobile_no;
                            }
                            res.StateJurisdiction = gstResp.result.state_jurisdiction;
                            res.RegisterDate = gstResp.result.register_date;
                        }
                    }
                    else
                    {
                        res.Statuscode = ErrorCodes.Minus1;
                        res.Msg = gstResp.response_message;
                    }
                }
            }
            catch (Exception ex)
            {
                Respon = "Exception:" + Respon + ex.Message;
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "ValidateGSTINAdvance",
                    Error = ex.Message,
                    LoginTypeID = 1,
                    UserId = 1
                });
            }
            IProcedure _proc = new ProcLogEKYCAPIReqResp(_dal);
            _proc.Call(new CommonReq
            {
                CommonInt = _APIID,
                CommonStr = Req,
                CommonStr2 = Respon
            });
            return res;
        }

        public EKYCByAadharModelOTP GenerateAadharOTP(string AadhaarNo, string Director)
        {
            var res = new EKYCByAadharModelOTP
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.Down
            };
            var headers = new Dictionary<string, string> {
                {"api-key",appSetting.APIKey },
                {"app-id",appSetting.AppID }
            };
            var datarequest = new
            {
                data = new
                {
                    customer_aadhaar_number = AadhaarNo,
                    consent = "Y",
                    consent_text = "I hear by declare my consent agreement for fetching my information via ZOOP API"
                }
            };
            var _URL = (appSetting.BaseURL ?? string.Empty) + "in/identity/okyc/otp/request";
            var Req = string.Empty;
            var Respon = string.Empty;
            try
            {
                Req = _URL + JsonConvert.SerializeObject(headers) + JsonConvert.SerializeObject(datarequest);
                var apiResp = AppWebRequest.O.PostJsonDataUsingHWRTLS(_URL, datarequest, headers).Result;
                Respon = apiResp;
                if (!string.IsNullOrEmpty(apiResp))
                {
                    var adharResp = JsonConvert.DeserializeObject<ZoopModelResponseAadharOTP>(apiResp);
                    if (adharResp.success && adharResp.response_code == "100")
                    {
                        res.Statuscode = ErrorCodes.One;
                        res.Msg = ErrorCodes.SUCCESS;
                        if (adharResp.result != null)
                        {
                            res.IsOTPSent = adharResp.result.is_otp_sent;
                            res.IsNumberLinked = adharResp.result.is_number_linked;
                            res.IsAadharValid = adharResp.result.is_aadhaar_valid;
                            if (adharResp.result.is_otp_sent)
                            {
                                IProcedure procedure = new ProcAadharOTPReference(_dal);
                                var refResp = (ResponseStatus)procedure.Call(new CommonReq
                                {
                                    CommonStr = AadhaarNo,
                                    CommonInt = _APIID,
                                    CommonStr2 = adharResp.request_id,
                                    CommonStr3 = Director
                                });
                                res.ReferenceID = refResp.CommonInt;
                            }
                        }
                    }
                    else
                    {
                        res.Statuscode = ErrorCodes.Minus1;
                        res.Msg = adharResp.response_message;
                    }
                }
            }
            catch (Exception ex)
            {

                Respon = "Exception:" + ex.Message + Respon;
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "GenerateAadharOTP",
                    Error = ex.Message,
                    LoginTypeID = 1,
                    UserId = 1
                });
            }
            IProcedure _proc = new ProcLogEKYCAPIReqResp(_dal);
            _proc.Call(new CommonReq
            {
                CommonInt = _APIID,
                CommonStr = Req,
                CommonStr2 = Respon
            });
            return res;
        }

        public EKYCByAadharModel ValidateAadharOTP(string ReferenceID, string OTP)
        {
            var res = new EKYCByAadharModel
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = nameof(ErrorCodes.Invalid_OTP).Replace("_", " ")
            };
            var headers = new Dictionary<string, string> {
                {"api-key",appSetting.APIKey },
                {"app-id",appSetting.AppID }
            };
            var dataRequest = new
            {
                data = new
                {
                    request_id = ReferenceID,
                    otp = OTP,
                    consent = "Y",
                    consent_text = "I hear by declare my consent agreement for fetching my information via ZOOP API"
                }
            };
            var _URL = (appSetting.BaseURL ?? string.Empty) + "in/identity/okyc/otp/verify";
            var Req = string.Empty;
            var Respon = string.Empty;
            try
            {
                Req = _URL + JsonConvert.SerializeObject(headers) + JsonConvert.SerializeObject(dataRequest);
                var apiResp = AppWebRequest.O.PostJsonDataUsingHWRTLS(_URL, dataRequest, headers).Result;
                Respon = apiResp;
                var dob = string.Empty;
                if (!string.IsNullOrEmpty(apiResp))
                {
                    var adharResp = JsonConvert.DeserializeObject<ZoopModelResponseAadhar>(apiResp);
                    if (adharResp.success && adharResp.response_code == "100")
                    {
                        res.Statuscode = ErrorCodes.One;
                        res.Msg = ErrorCodes.SUCCESS;
                        if (adharResp.result != null)
                        {
                            res.FullName = adharResp.result.user_full_name;
                            res.Gender = adharResp.result.user_gender;
                            res.HasImage = adharResp.result.user_has_image;
                            if (adharResp.result.user_address != null)
                            {
                                res.House = adharResp.result.user_address.house;
                                res.Landmark = adharResp.result.user_address.landmark;
                                res.Location = adharResp.result.user_address.loc;
                                res.Pincode = adharResp.result.address_zip;
                                res.PostOffice = adharResp.result.user_address.po;
                                res.State = adharResp.result.user_address.state;
                                res.Street = adharResp.result.user_address.street;
                                res.SubDistrict = adharResp.result.user_address.subdist;
                                res.District = adharResp.result.user_address.dist;
                                res.VTC = adharResp.result.user_address.vtc;
                                res.Country = adharResp.result.user_address.country;
                            }
                            dob = adharResp.result.user_dob;
                            res.IsMobileVerified = adharResp.result.user_mobile_verified;
                            res.ParentName = adharResp.result.user_parent_name;
                            res.Profile = adharResp.result.user_profile_image;
                            res.ShareCode = adharResp.result.aadhaar_share_code;
                        }
                    }
                    else
                    {
                        res.Statuscode = ErrorCodes.Minus1;
                        res.Msg = adharResp.response_message;
                    }
                    if (!string.IsNullOrEmpty(dob))
                    {
                        var splitDob = dob.Split("/");
                        if (splitDob.Length == 3)
                        {
                            dob = splitDob[1] + "/" + splitDob[0] + "/" + splitDob[2];
                        }
                        res.DOB = Convert.ToDateTime(dob).ToString("dd MMM yyyy");
                    }
                }
            }
            catch (Exception ex)
            {

                Respon = "Exception:" + ex.Message + Respon;
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "GenerateAadharOTP",
                    Error = ex.Message,
                    LoginTypeID = 1,
                    UserId = 1
                });
            }
            IProcedure _proc = new ProcLogEKYCAPIReqResp(_dal);
            _proc.Call(new CommonReq
            {
                CommonInt = _APIID,
                CommonStr = Req,
                CommonStr2 = Respon
            });
            return res;
        }

        public EKYCByPANModel ValidatePANNumber(string PANNumber)
        {
            var res = new EKYCByPANModel
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.Down
            };
            var headers = new Dictionary<string, string> {
                {"api-key",appSetting.APIKey },
                {"app-id",appSetting.AppID }
            };
            var dataRequest = new
            {
                data = new
                {
                    customer_pan_number = PANNumber,
                    consent = "Y",
                    consent_text = "I hear by declare my consent agreement for fetching my information via ZOOP API"
                }
            };
            var _URL = (appSetting.BaseURL ?? string.Empty) + "api/v1/in/identity/pan/advance";
            var Req = string.Empty;
            var Respon = string.Empty;
            try
            {
                Req = _URL + JsonConvert.SerializeObject(headers) + JsonConvert.SerializeObject(dataRequest);
                var apiResp = AppWebRequest.O.PostJsonDataUsingHWRTLS(_URL, dataRequest, headers).Result;
                Respon = apiResp;
                if (!string.IsNullOrEmpty(apiResp))
                {
                    var adharResp = JsonConvert.DeserializeObject<ZoopModelResponsePAN>(apiResp);
                    if (adharResp.success && adharResp.response_code == "100")
                    {
                        res.Statuscode = ErrorCodes.One;
                        res.Msg = ErrorCodes.SUCCESS;
                        if (adharResp.result != null)
                        {
                            res.FullName = adharResp.result.name_on_card;
                            res.FirstName = adharResp.result.user_first_name;
                            res.LastName = adharResp.result.user_last_name;
                            res.LastName = adharResp.result.user_last_name;
                            res.Title = adharResp.result.user_title;
                            res.IsAadharSeeded = adharResp.result.aadhaar_seeding_status == "SEEDING SUCCESSFUL";
                            res.IsPANValid = adharResp.result.pan_status == "VALID";
                        }
                    }
                    else
                    {
                        res.Statuscode = ErrorCodes.Minus1;
                        res.Msg = adharResp.response_message;
                    }
                }
            }
            catch (Exception ex)
            {

                Respon = "Exception:" + ex.Message + Respon;
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "GenerateAadharOTP",
                    Error = ex.Message,
                    LoginTypeID = 1,
                    UserId = 1
                });
            }
            IProcedure _proc = new ProcLogEKYCAPIReqResp(_dal);
            _proc.Call(new CommonReq
            {
                CommonInt = _APIID,
                CommonStr = Req,
                CommonStr2 = Respon
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
                LiveID = "",
                ErrorCode = ErrorCodes.Request_Accpeted
            };
            var headers = new Dictionary<string, string> {
                {"api-key",appSetting.APIKey },
                {"app-id",appSetting.AppID }
            };
            var dataRequest = new
            {
                data = new
                {
                    account_number = request.mBeneDetail.AccountNo,
                    ifsc = request.mBeneDetail.IFSC,
                    consent = "Y",
                    consent_text = "I hear by declare my consent agreement for fetching my information via ZOOP API"
                }
            };
            var _URL = (appSetting.BaseURL ?? string.Empty) + "api/v1/in/financial/bav/lite";
            try
            {
                res.Request = _URL + JsonConvert.SerializeObject(headers) + JsonConvert.SerializeObject(dataRequest);
                var apiResp = AppWebRequest.O.PostJsonDataUsingHWRTLS(_URL, dataRequest, headers).Result;
                res.Response = apiResp;
                if (!string.IsNullOrEmpty(apiResp))
                {
                    var acctResp = JsonConvert.DeserializeObject<ZoopModelResponseBankAccount>(apiResp);
                    if (acctResp.success && acctResp.response_code == "100")
                    {

                        if (acctResp.result != null)
                        {
                            res.LiveID = acctResp.result.bank_ref_no;
                            res.BeneName = acctResp.result.beneficiary_name;
                            if (acctResp.result.verification_status == "VERIFIED")
                            {
                                res.Statuscode = RechargeRespType.SUCCESS;
                                res.Msg = ErrorCodes.SUCCESS;
                            }
                            else
                            {
                                res.Statuscode = RechargeRespType.FAILED;
                                res.Msg = acctResp.response_message;
                            }
                        }
                    }
                    else
                    {
                        res.Statuscode = RechargeRespType.FAILED;
                        res.Msg = acctResp.response_message;
                    }
                }
            }
            catch (Exception ex)
            {

                res.Response = "Exception:" + ex.Message + res.Response;
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "VerifyAccount",
                    Error = ex.Message,
                    LoginTypeID = 1,
                    UserId = 1
                });
            }
            var dMTReq = new DMTReqRes
            {
                APIID = request.APIID,
                Method = "VerifyAccount",
                RequestModeID = request.RequestMode,
                Request = res.Request,
                Response = res.Response,
                SenderNo = request.SenderMobile,
                UserID = request.UserID,
                TID = request.TID.ToString()
            };
            new ProcUpdateLogDMRReqResp(_dal).Call(dMTReq);
            return res;
        }

        public MSenderCreateResp CreateSender(MTAPIRequest request)
        {
            throw new NotImplementedException();
        }

        public MSenderLoginResponse GetSender(MTAPIRequest request)
        {
            throw new NotImplementedException();
        }

        public MSenderCreateResp SenderKYC(MTAPIRequest request)
        {
            throw new NotImplementedException();
        }

        public MSenderCreateResp SenderEKYC(MTAPIRequest request)
        {
            throw new NotImplementedException();
        }

        public MSenderCreateResp SenderResendOTP(MTAPIRequest request)
        {
            throw new NotImplementedException();
        }

        public MSenderLoginResponse VerifySender(MTAPIRequest request)
        {
            throw new NotImplementedException();
        }

        public MSenderLoginResponse CreateBeneficiary(MTAPIRequest request)
        {
            throw new NotImplementedException();
        }

        public MBeneficiaryResp GetBeneficiary(MTAPIRequest request)
        {
            throw new NotImplementedException();
        }

        public MSenderCreateResp GenerateOTP(MTAPIRequest request)
        {
            throw new NotImplementedException();
        }

        public MSenderLoginResponse ValidateBeneficiary(MTAPIRequest request)
        {
            throw new NotImplementedException();
        }

        public MSenderLoginResponse RemoveBeneficiary(MTAPIRequest request)
        {
            throw new NotImplementedException();
        }

        public MSenderLoginResponse RemoveBeneficiaryValidate(MTAPIRequest request)
        {
            throw new NotImplementedException();
        }

        public DMRTransactionResponse AccountTransfer(MTAPIRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
