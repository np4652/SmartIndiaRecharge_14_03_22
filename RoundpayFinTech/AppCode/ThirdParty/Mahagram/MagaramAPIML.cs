using Fintech.AppCode.Configuration;
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
using RoundpayFinTech.AppCode.MiddleLayer;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.Model.Recharge;
using RoundpayFinTech.AppCode.StaticModel.MoneyTransfer;
using RoundpayFinTech.AppCode.ThirdParty.Roundpay;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Validators;

namespace RoundpayFinTech.AppCode.ThirdParty.Mahagram
{
    public partial class MahagramAPIML : IMahagramAPIML
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        private readonly IDAL _dal;

        public IConfigurationRoot Configuration { get; }

        private readonly IConnectionConfiguration _c;
        private readonly string _BaseURL;
        private readonly string SALTKEY;
        private readonly string SECRECTKEY;
        public MahagramAPIML(IHttpContextAccessor accessor, IHostingEnvironment env, string BaseURL)
        {
            _accessor = accessor;
            _env = env;
            _c = new ConnectionConfiguration(_accessor, _env);
            _dal = new DAL(_c.GetConnectionString());
            if (string.IsNullOrEmpty(BaseURL))
            {
                var builder = new ConfigurationBuilder()
                 .SetBasePath(Directory.GetCurrentDirectory());
                builder.AddJsonFile((_env.IsProduction() ? "appsettings.json" : "appsettings.Development.json"));
                builder.AddEnvironmentVariables();
                var Configuration = builder.Build();
                if (Configuration != null)
                {
                    _BaseURL = Configuration["SERVICESETTING:MHAGM:OnboardingSetting:BaseURL"];
                    var Token = Configuration["SERVICESETTING:MHAGM:OnboardingSetting:Token"];
                    if (Token != null)
                    {
                        var setSplit = Token.Split('&');
                        if (setSplit.Length == 3)
                        {
                            SALTKEY = setSplit[0].Contains("=") ? setSplit[0].Split("=")[1] : string.Empty;
                            SECRECTKEY = setSplit[1].Contains("=") ? setSplit[1].Split("=")[1] : string.Empty;
                        }
                    }
                }
            }
            else
            {
                _BaseURL = BaseURL;
            }

        }
        //IF BC Not Found Then Call
        public OutletAPIStatusUpdate GetBCCode(MGBCGetCodeRequest MGReq, int APIID)
        {
            var OutReqRes = new OutletAPIStatusUpdate
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            //string URL = "http://45.249.111.172/Airtel/AEPS/GetBcCode/Airtel/AEPS/GetBcCode";
            string URL = "http://45.249.111.172/Airtel/AEPS/GetBcCode";
            string MGResp = string.Empty;
            try
            {
                MGResp = AppWebRequest.O.PostJsonDataUsingHWR(URL, MGReq);
                var MGResponse = JsonConvert.DeserializeObject<MGOnboardingResponse[]>(MGResp);
                OutReqRes.Msg = ErrorCodes.FailedToSubmit;
                if (MGResponse != null)
                {
                    if (MGResponse[0].StatusCode == "002")
                    {
                        OutReqRes.Msg = ErrorCodes.DMTONE;
                    }
                    if (MGResponse[0].StatusCode == "001")
                    {
                        OutReqRes.Statuscode = ErrorCodes.One;
                        OutReqRes.APIOutletID = MGResponse[0].BcCode;
                        OutReqRes.AEPSID = MGResponse[0].BcCode;
                        OutReqRes.DMTID = MGResponse[0].BcCode;
                        OutReqRes.KYCStatus = RndKYCOutLetStatus._Approved;
                        OutReqRes.APIOutletStatus = RndKYCOutLetStatus._Approved;
                        OutReqRes.AEPSStatus = RndKYCOutLetStatus._Pending;
                        OutReqRes.DMTStatus = RndKYCOutLetStatus._Pending;
                    }
                }
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "GetBCCode",
                    Error = ex.Message,
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
                MGResp = ex.Message + "_" + MGResp;
                OutReqRes.Msg = ErrorCodes.TempError;
            }
            #region APILogGetBCCode
            var OnboradReq = new OnboardingLog
            {
                APIID = APIID,
                Method = "GetBCCode",
                Request = URL + "|" + JsonConvert.SerializeObject(MGReq),
                Response = MGResp
            };
            new ProcUpdateLogOnboardingReqResp(_dal).Call(OnboradReq);
            #endregion
            return OutReqRes;
        }
        //OutRegistration
        public OutletAPIStatusUpdate APIBCRegistration(MGOnboardingReq MGReq, int APIID)
        {
            var OutReqRes = new OutletAPIStatusUpdate
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            string URL = _BaseURL + "AEPS/APIBCRegistration";
            string MGResp = string.Empty;
            try
            {
                MGResp = AppWebRequest.O.PostJsonDataUsingHWR(URL, MGReq);
                var MGResponse = JsonConvert.DeserializeObject<MGOnboardingResponse[]>(MGResp);
                OutReqRes.Msg = ErrorCodes.FailedToSubmit;
                if (MGResponse != null)
                {
                    if (MGResponse[0].Statuscode == "000")
                    {
                        OutReqRes.Statuscode = ErrorCodes.One;
                        OutReqRes.Msg = ErrorCodes.OutletRegistered;
                        OutReqRes.APIOutletID = MGResponse[0].bc_id;
                        OutReqRes.APIOutletStatus = RndKYCOutLetStatus._Pending;
                        OutReqRes.KYCStatus = RndKYCOutLetStatus._Pending;
                        OutReqRes.AEPSID = MGResponse[0].bc_id;
                        OutReqRes.AEPSStatus = RndKYCOutLetStatus._Pending;
                        OutReqRes.DMTID = MGResponse[0].bc_id;
                        OutReqRes.DMTStatus = RndKYCOutLetStatus._Pending;
                        OutReqRes.PSAID = string.Empty;
                    }
                    OutReqRes.Msg = MGResponse[0].Message;
                }
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "APIBCRegistration",
                    Error = ex.Message,
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
                MGResp = ex.Message + "_" + MGResp;
                OutReqRes.Msg = ErrorCodes.TempError;
            }
            #region APILogOutletRegistration
            var OnboradReq = new OnboardingLog
            {
                APIID = APIID,
                Method = "APIBCRegistration",
                Request = URL + "|" + JsonConvert.SerializeObject(MGReq),
                Response = MGResp
            };
            new ProcUpdateLogOnboardingReqResp(_dal).Call(OnboradReq);
            #endregion
            return OutReqRes;
        }
        //CheckOnboardStatus
        public OutletAPIStatusUpdate APIBCStatus(MGBCStatusRequest MGReq, int APIID)
        {
            var OutReqRes = new OutletAPIStatusUpdate
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            string URL = _BaseURL + "AEPS/APIBCStatus";
            string MGResp = string.Empty;
            try
            {
                //[{"bc_id":"BC867229513","status":"Rejected","remarks":"Invalid Format KYC","StatusCode":"001","Message":"BcFound"}]
                MGResp = AppWebRequest.O.PostJsonDataUsingHWR(URL, MGReq);
                var MGResponse = JsonConvert.DeserializeObject<MGOnboardingResponse[]>(MGResp);
                if (MGResponse != null)
                {
                    if (MGResponse[0].StatusCode == "001")
                    {
                        OutReqRes.Statuscode = ErrorCodes.One;
                        OutReqRes.APIOutletID = MGResponse[0].bc_id;
                        OutReqRes.AEPSID = MGResponse[0].bc_id;
                        OutReqRes.DMTID = MGResponse[0].bc_id;
                        if (MGResponse[0].status == "Active")
                        {
                            OutReqRes.KYCStatus = RndKYCOutLetStatus._Approved;
                            OutReqRes.APIOutletStatus = RndKYCOutLetStatus._Approved;
                            OutReqRes.AEPSStatus = RndKYCOutLetStatus._Approved;
                            OutReqRes.DMTStatus = RndKYCOutLetStatus._Approved;
                        }
                        else if (MGResponse[0].status == "Pending")
                        {
                            OutReqRes.KYCStatus = RndKYCOutLetStatus._Approved;
                            OutReqRes.APIOutletStatus = RndKYCOutLetStatus._Approved;
                            OutReqRes.AEPSStatus = RndKYCOutLetStatus._Pending;
                            OutReqRes.DMTStatus = RndKYCOutLetStatus._Pending;
                        }
                        else if (MGResponse[0].status == "Rejected")
                        {
                            OutReqRes.KYCStatus = RndKYCOutLetStatus._Approved;
                            OutReqRes.APIOutletStatus = RndKYCOutLetStatus._Approved;
                            OutReqRes.AEPSStatus = RndKYCOutLetStatus._Rejected;
                            OutReqRes.DMTStatus = RndKYCOutLetStatus._Rejected;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "APIBCStatus",
                    Error = ex.Message,
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
                OutReqRes.Msg = ErrorCodes.TempError;
                MGResp = ex.Message + "_" + MGResp;
            }
            #region LogCheckOnboardStatus
            var OnboradReq = new OnboardingLog
            {
                APIID = APIID,
                Method = "APIBCStatus",
                Request = URL + "|" + JsonConvert.SerializeObject(MGReq),
                Response = MGResp
            };
            new ProcUpdateLogOnboardingReqResp(_dal).Call(OnboradReq);
            #endregion
            return OutReqRes;
        }
        //AEPS ServicePlus
        public OutletAPIStatusUpdate BCInitiate(MGInitiateRequest MGReq, int APIID)
        {
            var OutReqRes = new OutletAPIStatusUpdate
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            string URL = _BaseURL + "AEPS/BCInitiate";
            string MGResp = string.Empty;
            try
            {
                MGResp = AppWebRequest.O.PostJsonDataUsingHWR(URL, MGReq);
                var MGResponse = JsonConvert.DeserializeObject<MGOnboardingResponse[]>(MGResp);
                OutReqRes.Msg = ErrorCodes.FailedToSubmit;
                if (MGResponse != null)
                {
                    if (MGResponse[0].StatusCode == "001")
                    {
                        OutReqRes.Statuscode = ErrorCodes.One;
                        OutReqRes.Msg = ErrorCodes.OutletRegistered;
                        OutReqRes.AEPSURL = "https://icici.bankmitra.org/Location.aspx?text=" + MGResponse[0].Result;
                        OutReqRes.AEPSStatus = RndKYCOutLetStatus._Approved;
                    }
                }
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "BCInitiate",
                    Error = ex.Message,
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
                MGResp = ex.Message + "_" + MGResp;
                OutReqRes.Msg = ErrorCodes.TempError;
            }
            #region APILogBCInitiate
            var OnboradReq = new OnboardingLog
            {
                APIID = APIID,
                Method = "BCInitiate",
                Request = URL + "|" + JsonConvert.SerializeObject(MGReq),
                Response = MGResp
            };
            new ProcUpdateLogOnboardingReqResp(_dal).Call(OnboradReq);
            #endregion
            return OutReqRes;
        }
        //PSA Registration
        public OutletAPIStatusUpdate UTIRegistration(MGPSARequest MGReq, int APIID)
        {
            var OutReqRes = new OutletAPIStatusUpdate
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            string URL = _BaseURL + "UTI/UATInsUTIAgent";
            string MGResp = string.Empty;
            try
            {
                MGResp = AppWebRequest.O.PostJsonDataUsingHWR(URL, MGReq);
                var MGResponse = JsonConvert.DeserializeObject<MGPSAResponse[]>(MGResp);
                OutReqRes.Msg = ErrorCodes.FailedToSubmit;
                if (MGResponse != null)
                {
                    if (MGResponse.Length > 0 && MGResponse[0].StatusCode.Equals("000"))
                    {
                        OutReqRes.Statuscode = ErrorCodes.One;
                        OutReqRes.Msg = ErrorCodes.OutletRegistered;
                        OutReqRes.PSAID = MGResponse[0].psaid;
                        if (Validate.O.IsNumeric(MGResponse[0].Request ?? string.Empty))
                        {
                            OutReqRes.PSARequestID = Convert.ToInt32(MGResponse[0].Request);
                        }
                        if (MGResponse[0].Status.Equals("Pending"))
                            OutReqRes.PSAStatus = RndKYCOutLetStatus._Pending;
                    }
                    else
                    {
                        OutReqRes.Msg = MGResponse[0].Message;
                    }
                }
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "UTIRegistration",
                    Error = ex.Message,
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
                MGResp = ex.Message + "_" + MGResp;
                OutReqRes.Msg = ErrorCodes.TempError;
            }
            #region APILogOutletRegistration
            var OnboradReq = new OnboardingLog
            {
                APIID = APIID,
                Method = "UTIRegistration",
                Request = URL + "|" + JsonConvert.SerializeObject(MGReq),
                Response = MGResp
            };
            new ProcUpdateLogOnboardingReqResp(_dal).Call(OnboradReq);
            #endregion
            return OutReqRes;
        }
        //PSA Update Registration
        public OutletAPIStatusUpdate UTIRegistrationUpdate(MGPSARequest MGReq, int APIID)
        {
            var OutReqRes = new OutletAPIStatusUpdate
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            string URL = _BaseURL + "UTI/UATUpdateRejectedRequest";
            string MGResp = string.Empty;
            try
            {
                MGResp = AppWebRequest.O.PostJsonDataUsingHWR(URL, MGReq);
                var MGResponse = JsonConvert.DeserializeObject<MGPSAResponse[]>(MGResp);
                OutReqRes.Msg = ErrorCodes.FailedToSubmit;
                if (MGResponse != null)
                {
                    if (MGResponse.Length > 0 && MGResponse[0].StatusCode.Equals("000"))
                    {
                        OutReqRes.Statuscode = ErrorCodes.One;
                        OutReqRes.Msg = ErrorCodes.OutletRegistered;
                        OutReqRes.PSAID = MGResponse[0].psaid;
                        if (Validate.O.IsNumeric(MGResponse[0].Request ?? string.Empty))
                        {
                            OutReqRes.PSARequestID = Convert.ToInt32(MGResponse[0].Request);
                        }
                        if (MGResponse[0].Status.Equals("Pending"))
                            OutReqRes.PSAStatus = RndKYCOutLetStatus._Pending;
                    }
                    else
                    {
                        OutReqRes.Msg = MGResponse[0].Message;
                    }
                }
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "UTIRegistrationUpdate",
                    Error = ex.Message,
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
                MGResp = ex.Message + "_" + MGResp;
                OutReqRes.Msg = ErrorCodes.TempError;
            }
            #region APILogOutletRegistration
            var OnboradReq = new OnboardingLog
            {
                APIID = APIID,
                Method = "UTIRegistrationUpdate",
                Request = URL + "|" + JsonConvert.SerializeObject(MGReq),
                Response = MGResp
            };
            new ProcUpdateLogOnboardingReqResp(_dal).Call(OnboradReq);
            #endregion
            return OutReqRes;
        }
        //PSA Update Registration
        public OutletAPIStatusUpdate UTIAgentStatuscheck(MGPSARequest MGReq, int APIID)
        {
            var OutReqRes = new OutletAPIStatusUpdate
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            string URL = _BaseURL + "UTI/UATUTIAgentRequestStatus";
            string MGResp = string.Empty;
            try
            {
                MGResp = AppWebRequest.O.PostJsonDataUsingHWR(URL, MGReq);
                var MGResponse = JsonConvert.DeserializeObject<MGPSAResponse[]>(MGResp);
                OutReqRes.Msg = ErrorCodes.FailedToSubmit;
                if (MGResponse != null)
                {
                    if (MGResponse.Length > 0 && MGResponse[0].StatusCode.Equals("000"))
                    {
                        OutReqRes.Statuscode = ErrorCodes.One;
                        OutReqRes.Msg = ErrorCodes.OutletRegistered;
                        OutReqRes.PSAID = MGResponse[0].psaid;
                        if (Validate.O.IsNumeric(MGResponse[0].RequestId ?? string.Empty))
                        {
                            OutReqRes.PSARequestID = Convert.ToInt32(MGResponse[0].RequestId);
                        }

                        if (MGResponse[0].status == "Approved")
                        {
                            OutReqRes.PSAStatus = RndKYCOutLetStatus._Approved;
                        }
                        else if (MGResponse[0].status == "Pending")
                        {
                            OutReqRes.PSAStatus = RndKYCOutLetStatus._Pending;
                        }
                        else if (MGResponse[0].status == "Rejected")
                        {
                            OutReqRes.PSAStatus = RndKYCOutLetStatus._Rejected;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "UTIAgentStatuscheck",
                    Error = ex.Message,
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
                MGResp = ex.Message + "_" + MGResp;
                OutReqRes.Msg = ErrorCodes.TempError;
            }
            #region APILogOutletRegistration
            var OnboradReq = new OnboardingLog
            {
                APIID = APIID,
                Method = "UTIAgentStatuscheck",
                Request = URL + "|" + JsonConvert.SerializeObject(MGReq),
                Response = MGResp
            };
            new ProcUpdateLogOnboardingReqResp(_dal).Call(OnboradReq);
            #endregion
            return OutReqRes;
        }
        //Coupon Request
        public async Task<PSAResponse> UTICouponRequest(MGCouponRequest MGReq)
        {
            var psaResponse = new PSAResponse
            {
                Status = RechargeRespType.PENDING,
                Msg = ErrorCodes.NORESPONSE
            };
            psaResponse.Req = JsonConvert.SerializeObject(MGReq);
            string URL = _BaseURL + "UTI/UATInsCouponRequest";
            string MGResp = string.Empty;
            try
            {
                MGResp = await AppWebRequest.O.PostJsonDataUsingHWRAsync(URL, MGReq).ConfigureAwait(false);
                var MGResponse = JsonConvert.DeserializeObject<MGCouponResponse[]>(MGResp);
                if (MGResponse != null)
                {
                    if (MGResponse.Length > 0 && MGResponse[0].StatusCode.Equals("000"))
                    {
                        psaResponse.Msg = MGResponse[0].Message ?? string.Empty;
                        if (MGResponse[0].status.Equals("Pending"))
                        {
                            psaResponse.Status = RechargeRespType.PENDING;
                            psaResponse.LiveID = MGResponse[0].ReferenceId;
                            psaResponse.VendorID = MGResponse[0].RequestId;
                        }
                        if (MGResponse[0].status.Equals("Success"))
                        {
                            psaResponse.Status = RechargeRespType.SUCCESS;
                            psaResponse.LiveID = MGResponse[0].ReferenceId;
                            psaResponse.Msg = nameof(ErrorCodes.Transaction_Successful);
                            psaResponse.VendorID = MGResponse[0].RequestId;
                        }
                        if (MGResponse[0].status.Equals("Failed"))
                        {
                            psaResponse.Status = RechargeRespType.FAILED;
                            psaResponse.VendorID = MGResponse[0].RequestId;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "CouponRequest",
                    Error = ex.Message,
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
                psaResponse.Msg = ErrorCodes.TempError;
                MGResp = ex.Message + "_" + MGResp;
            }
            psaResponse.Resp = MGResp;
            return psaResponse;
        }
        //Coupon Request

        public HitRequestResponseModel UTICouponStatus(MGCouponRequest MGReq)
        {
            var returnResp = new HitRequestResponseModel();

            var appSetting = GetApiPSAAppSetting(APICode.MAHAGRAM);
            if ((appSetting.onboardingSetting.Token ?? string.Empty).Contains("&"))
            {
                var setSplit = appSetting.onboardingSetting.Token.Split('&');
                if (setSplit.Length == 2)
                {
                    MGReq.securityKey = setSplit[0].Contains("=") ? setSplit[0].Split("=")[1] : string.Empty;
                    MGReq.createdby = setSplit[1].Contains("=") ? setSplit[1].Split("=")[1] : string.Empty;
                }
            }
            string URL = appSetting.onboardingSetting.BaseURL + "UTI/UATUTICouponRequestStatus";
            var PSAReq = new
            {
                securityKey = MGReq.securityKey,
                createdby = MGReq.createdby,
                requestid = MGReq.requestid
            };
            string MGResp = string.Empty;
            try
            {
                MGResp = AppWebRequest.O.PostJsonDataUsingHWR(URL, PSAReq);
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "CouponRequest",
                    Error = ex.Message,
                });
                MGResp = ex.Message + "_" + MGResp;
            }
            returnResp.Request = URL + JsonConvert.SerializeObject(PSAReq);
            returnResp.Response = MGResp;
            return returnResp;
        }

        public MiniBankTransactionServiceResp MiniBankStatusCheck(MBStatusCheckRequest request)
        {
            var response = new MiniBankTransactionServiceResp
            {
                Statuscode = RechargeRespType.PENDING,
                Msg = ErrorCodes.NORESPONSE
            };
            var MGReq = new MHMinATMStatuscheckRequest();
            var MGAEPSReq = new MHAEPSStatuscheckRequest();
            string URL = string.Empty;
            string MGResp = string.Empty;

            if (request.SCode.Equals(ServiceCode.AEPS))
            {
                string vendorID = string.Empty;
                if (request.VendorID.Contains("|"))

                    vendorID = request.VendorID.Split('|')[0];
                else
                    vendorID = request.VendorID;

                MGAEPSReq.stanno = vendorID;
                MGAEPSReq.Saltkey = SALTKEY;
                MGAEPSReq.Secretkey = SECRECTKEY;

                URL = _BaseURL + "Common/CheckAePSTxnStatus";
                MGResp = AppWebRequest.O.PostJsonDataUsingHWR(URL, MGAEPSReq);
            }
            else
            {
                MGReq.referenceid = request.VendorID;
                MGReq.saltkey = SALTKEY;
                MGReq.secretekey = SECRECTKEY;
                URL = _BaseURL + "MICROATM/GetMATMtxnStatus";
                MGResp = AppWebRequest.O.PostJsonDataUsingHWR(URL, MGReq);
            }

            try
            {
                var MGResponse = JsonConvert.DeserializeObject<MGMiniATMStatuscheckResponse>(MGResp);
                if (MGResponse != null)
                {
                    if (MGResponse.statuscode.Equals("000"))
                    {
                        response.Msg = MGResponse.message ?? string.Empty;
                        if (MGResponse.Data.Count > 0)
                        {
                            if (request.SCode.Equals(ServiceCode.AEPS))
                            {
                                response.VendorID = MGResponse.Data[0].stan_no;
                                response.APIOutletID = MGResponse.Data[0].bc_id;
                                response.BankName = MGResponse.Data[0].bankname;
                            }
                            else
                            {
                                response.VendorID = MGResponse.Data[0].stanno;
                                response.APIOutletID = MGResponse.Data[0].bcid;
                                response.BankName = MGResponse.Data[0].udf1;
                            }
                            response.Amount = Convert.ToInt32(MGResponse.Data[0].amount);
                            response.LiveID = MGResponse.Data[0].rrn;
                            response.Msg = MGResponse.Data[0].bankmessage;
                            response.CardNumber = MGResponse.Data[0].cardno;
                            response.BankTransactionDate = MGResponse.Data[0].createdate;

                            response.BankBalance = MGResponse.Data[0].udf2;
                            if (MGResponse.Data[0].status.ToUpper().Equals("SUCCESS"))
                            {
                                response.Statuscode = RechargeRespType.SUCCESS;
                            }
                            if (MGResponse.Data[0].status.In("Failed", "FAILURE"))
                            {
                                response.Statuscode = RechargeRespType.FAILED;
                            }
                            if (MGResponse.Data[0].status.Equals("Pending"))
                            {
                                response.Statuscode = RechargeRespType.PENDING;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "MiniBankStatusCheck",
                    Error = ex.Message,
                });
                response.Msg = ErrorCodes.TempError;
                MGResp = ex.Message + "_" + MGResp;
            }
            if (request.SCode.Equals(ServiceCode.AEPS))

                response.Req = URL + "?" + JsonConvert.SerializeObject(MGAEPSReq);
            else
                response.Req = URL + "?" + JsonConvert.SerializeObject(MGReq);

            response.Resp = MGResp;
            return response;
        }


        private APIAppSetting GetApiPSAAppSetting(string _APICode)
        {
            var res = new APIAppSetting();
            if (!string.IsNullOrEmpty(_APICode) && !string.IsNullOrEmpty(_APICode))
            {
                string SERVICESETTING = "SERVICESETTING:" + _APICode;
                var OnboardingSetting = SERVICESETTING + ":PSA";
                try
                {
                    res.onboardingSetting = new OnboardingSetting
                    {
                        BaseURL = Configuration[OnboardingSetting + ":BaseURL"],
                        Token = Configuration[OnboardingSetting + ":Token"]
                    };
                }

                catch (Exception ex)
                {
                    var errorLog = new ErrorLog
                    {
                        ClassName = GetType().Name,
                        FuncName = "GetApiAppSetting",
                        Error = "Exception:APICode=" + (_APICode ?? string.Empty) + ",SCode=PSA [" + ex.Message,
                    };
                    var _ = new ProcPageErrorLog(_dal).Call(errorLog);

                }
            }
            return res;
        }
    }
}
