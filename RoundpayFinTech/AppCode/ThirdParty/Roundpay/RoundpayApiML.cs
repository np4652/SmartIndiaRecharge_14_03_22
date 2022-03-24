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
using RoundpayFinTech.AppCode.MiddleLayer;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.StaticModel.MoneyTransfer;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Validators;

namespace RoundpayFinTech.AppCode.ThirdParty.Roundpay
{
    public class RoundpayApiML : IRoundpayApiML
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        private readonly IDAL _dal;
        private readonly IConnectionConfiguration _c;

        public RoundpayApiML(IHttpContextAccessor accessor, IHostingEnvironment env)
        {
            _accessor = accessor;
            _env = env;
            _c = new ConnectionConfiguration(_accessor, _env);
            _dal = new DAL(_c.GetConnectionString());
        }

        public OutletAPIStatusUpdate CheckOnboardStatus(RoundpayApiRequestModel ObjRoundpayApiRequestModel)
        {
            string Resp = string.Empty, PosRequest = string.Empty;
            var OutReqRes = new OutletAPIStatusUpdate
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            try
            {
                PosRequest = ObjRoundpayApiRequestModel.Token + "&Phone1=" + ObjRoundpayApiRequestModel.Phone1;
                Resp = AppWebRequest.O.CallUsingHttpWebRequest_POST(ObjRoundpayApiRequestModel.BaseUrl.Replace("{FNAME}", "Status_Check"), PosRequest);
                if (Validate.O.ValidateJSON(Resp ?? string.Empty))
                {

                    var _apiRes = JsonConvert.DeserializeObject<CheckStatusAtRoundpay>(Resp);
                    if ((_apiRes.STATUS ?? string.Empty) == RechargeRespType._FAILED)
                    {
                        OutReqRes.Msg = ErrorCodes.DMTONE;
                    }
                    else if ((_apiRes.STATUS ?? string.Empty) == "SUCCESS")
                    {
                        OutReqRes.Statuscode = ErrorCodes.One;
                        OutReqRes.APIOutletID = _apiRes.OutletId;
                        OutReqRes.BBPSID = _apiRes.OutletId;
                        OutReqRes.AEPSID = _apiRes.OutletId;
                        OutReqRes.DMTID = _apiRes.OutletId;
                        //APIOutlet
                        if (_apiRes.Outlet.ToUpper().Equals(RndKYCOutLetStatus.Approved.ToUpper()))
                        {
                            OutReqRes.APIOutletStatus = RndKYCOutLetStatus._Approved;
                        }
                        else if (_apiRes.Outlet.ToUpper().Equals(RndKYCOutLetStatus.Rejected.ToUpper()))
                        {
                            OutReqRes.APIOutletStatus = RndKYCOutLetStatus._Rejected;
                        }
                        else if (_apiRes.Outlet.ToUpper().Equals(RndKYCOutLetStatus.Pending.ToUpper()))
                        {
                            OutReqRes.APIOutletStatus = RndKYCOutLetStatus._Pending;
                        }

                        //KYC
                        if (_apiRes.KYC.ToUpper().Equals(RndKYCOutLetStatus.Approved.ToUpper()))
                        {
                            OutReqRes.KYCStatus = RndKYCOutLetStatus._Approved;
                        }
                        else if (_apiRes.KYC.ToUpper().Equals(RndKYCOutLetStatus.Rejected.ToUpper()))
                        {
                            OutReqRes.KYCStatus = RndKYCOutLetStatus._Rejected;
                        }
                        else if (_apiRes.KYC.ToUpper().Equals(RndKYCOutLetStatus.Pending.ToUpper()))
                        {
                            OutReqRes.KYCStatus = RndKYCOutLetStatus._Pending;
                        }

                        //AEPS
                        if (_apiRes.AEPS.ToUpper().Equals(RndKYCOutLetStatus.Approved.ToUpper()))
                        {
                            OutReqRes.AEPSStatus = RndKYCOutLetStatus._Approved;
                        }
                        else if (_apiRes.AEPS.ToUpper().Equals(RndKYCOutLetStatus.Pending.ToUpper()))
                        {
                            OutReqRes.AEPSStatus = RndKYCOutLetStatus._Pending;
                        }
                        else if (_apiRes.AEPS.ToUpper().Equals(RndKYCOutLetStatus.Rejected.ToUpper()))
                        {
                            OutReqRes.AEPSStatus = RndKYCOutLetStatus._Rejected;
                        }

                        //BBPS
                        if (_apiRes.BBPS.ToUpper().Equals(RndKYCOutLetStatus.Approved.ToUpper()))
                        {
                            OutReqRes.BBPSStatus = RndKYCOutLetStatus._Approved;
                        }
                        else if (_apiRes.BBPS.ToUpper().Equals(RndKYCOutLetStatus.Pending.ToUpper()))
                        {
                            OutReqRes.BBPSStatus = RndKYCOutLetStatus._Pending;
                        }
                        else if (_apiRes.BBPS.ToUpper().Equals(RndKYCOutLetStatus.Rejected.ToUpper()))
                        {
                            OutReqRes.BBPSStatus = RndKYCOutLetStatus._Rejected;
                        }
                        //DMT
                        OutReqRes.DMTStatus = RndKYCOutLetStatus._Approved;

                        //PSA
                        if (_apiRes.PSA.ToUpper().Equals(RndKYCOutLetStatus.Approved.ToUpper()))
                        {
                            OutReqRes.PSAStatus = RndKYCOutLetStatus._Approved;
                        }
                        else if (_apiRes.PSA.ToUpper().Equals(RndKYCOutLetStatus.Pending.ToUpper()))
                        {
                            OutReqRes.PSAStatus = RndKYCOutLetStatus._Pending;
                        }
                        else if (_apiRes.PSA.ToUpper().Equals(RndKYCOutLetStatus.Rejected.ToUpper()))
                        {
                            OutReqRes.PSAStatus = RndKYCOutLetStatus._Rejected;
                        }
                        OutReqRes.PSAID = _apiRes.PsaId;
                        OutReqRes.PSARequestID = Validate.O.IsNumeric(_apiRes.Panrequestid ?? string.Empty) ? Convert.ToInt32(_apiRes.Panrequestid) : 0;
                    }
                }
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Status_Check",
                    Error = ex.Message,
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
                OutReqRes.Msg = ErrorCodes.TempError;
                Resp = ex.Message + "_" + Resp;
            }
            #region LogCheckOnboardStatus
            var OnboradReq = new OnboardingLog
            {
                APIID = ObjRoundpayApiRequestModel.APIID,
                Method = "CheckOnboardStatus",
                Request = PosRequest,
                Response = Resp
            };
            new ProcUpdateLogOnboardingReqResp(_dal).Call(OnboradReq);
            #endregion
            return OutReqRes;
        }

        public OutletAPIStatusUpdate OutRegistration(RoundpayApiRequestModel ObjRoundpayApiRequestModel)
        {
            string Resp = "";
            var OutReqRes = new OutletAPIStatusUpdate
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            #region RequestIntialization
            StringBuilder sbPostRequest = new StringBuilder();
            sbPostRequest.AppendFormat(ObjRoundpayApiRequestModel.Token ?? "");
            sbPostRequest.AppendFormat("&Name={0}", ObjRoundpayApiRequestModel.Name ?? string.Empty);
            sbPostRequest.AppendFormat("&LastName={0}", ObjRoundpayApiRequestModel.LastName ?? string.Empty);
            sbPostRequest.AppendFormat("&DOB={0}", (ObjRoundpayApiRequestModel.DOB ?? string.Empty).Contains(" ") ? ObjRoundpayApiRequestModel.DOB.Replace(" ", "/") : (ObjRoundpayApiRequestModel.DOB ?? string.Empty));
            sbPostRequest.AppendFormat("&Pincode={0}", ObjRoundpayApiRequestModel.Pincode ?? string.Empty);
            sbPostRequest.AppendFormat("&Address={0}", ObjRoundpayApiRequestModel.Address ?? string.Empty);
            sbPostRequest.AppendFormat("&Area={0}", ObjRoundpayApiRequestModel.Area ?? string.Empty);
            sbPostRequest.AppendFormat("&Landmark={0}", ObjRoundpayApiRequestModel.Landmark ?? string.Empty);
            sbPostRequest.AppendFormat("&Phone1={0}", ObjRoundpayApiRequestModel.Phone1 ?? string.Empty);
            sbPostRequest.AppendFormat("&Phone2={0}", ObjRoundpayApiRequestModel.Phone2 ?? string.Empty);
            sbPostRequest.AppendFormat("&Emailid={0}", ObjRoundpayApiRequestModel.Emailid ?? string.Empty);
            sbPostRequest.AppendFormat("&Pan={0}", ObjRoundpayApiRequestModel.Pan ?? string.Empty);
            sbPostRequest.AppendFormat("&PANLink={0}", ObjRoundpayApiRequestModel.PANLink ?? string.Empty);
            sbPostRequest.AppendFormat("&Aadhaar={0}", ObjRoundpayApiRequestModel.Aadhaar ?? string.Empty);
            sbPostRequest.AppendFormat("&AadharLink={0}", ObjRoundpayApiRequestModel.AadharLink ?? string.Empty);
            sbPostRequest.AppendFormat("&ShopType={0}", ObjRoundpayApiRequestModel.ShopType ?? string.Empty);
            sbPostRequest.AppendFormat("&ShopLink={0}", ObjRoundpayApiRequestModel.ShopLink ?? string.Empty);
            sbPostRequest.AppendFormat("&Qualification={0}", ObjRoundpayApiRequestModel.Qualification ?? string.Empty);
            sbPostRequest.AppendFormat("&Population={0}", ObjRoundpayApiRequestModel.Poupulation ?? string.Empty);
            sbPostRequest.AppendFormat("&Latitude={0}", ObjRoundpayApiRequestModel.Latitude ?? string.Empty);
            sbPostRequest.AppendFormat("&Longitude={0}", ObjRoundpayApiRequestModel.Longitude ?? string.Empty);
            sbPostRequest.AppendFormat("&AreaType={0}", ObjRoundpayApiRequestModel.AreaType ?? string.Empty);
            sbPostRequest.AppendFormat("&StateId={0}", ObjRoundpayApiRequestModel.StateId);
            sbPostRequest.AppendFormat("&DistrictId={0}", ObjRoundpayApiRequestModel.DistrictId);
            #endregion
            try
            {
                Resp = AppWebRequest.O.CallUsingHttpWebRequest_POST(ObjRoundpayApiRequestModel.BaseUrl.Replace("{FNAME}", "OutletRegistration"), sbPostRequest.ToString());
                if (Validate.O.ValidateJSON(Resp ?? string.Empty))
                {
                    var _apiRes = JsonConvert.DeserializeObject<OutletRegRespRoundpayModel>(Resp);
                    if ((_apiRes.STATUS ?? string.Empty) == RechargeRespType._FAILED)
                    {
                        if ((_apiRes.MESSAGE ?? string.Empty) == "Outlet Exists")
                        {
                            OutReqRes.Msg = ErrorCodes.OutletExists;
                        }
                        else
                        {
                            OutReqRes.Msg = string.IsNullOrEmpty(_apiRes.MESSAGE) ? ErrorCodes.FailedToSubmit : _apiRes.MESSAGE;
                        }
                    }
                    else if (!string.IsNullOrEmpty(_apiRes.OutletId))
                    {
                        OutReqRes.Statuscode = ErrorCodes.One;
                        OutReqRes.Msg = ErrorCodes.OutletRegistered;
                        OutReqRes.APIOutletID = _apiRes.OutletId;
                        OutReqRes.APIOutletStatus = RndKYCOutLetStatus._Pending;
                        OutReqRes.KYCStatus = RndKYCOutLetStatus._Pending;
                        OutReqRes.BBPSID = _apiRes.OutletId;
                        OutReqRes.BBPSStatus = RndKYCOutLetStatus._NotApplied;
                        OutReqRes.AEPSID = _apiRes.OutletId;
                        OutReqRes.AEPSStatus = RndKYCOutLetStatus._NotApplied;
                        OutReqRes.DMTID = _apiRes.OutletId;
                        OutReqRes.DMTStatus = RndKYCOutLetStatus._NotApplied;
                        OutReqRes.PSAID = string.Empty;
                        OutReqRes.PSAStatus = RndKYCOutLetStatus._NotApplied;
                    }
                }
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "OutletRegistration",
                    Error = ex.Message,
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
                Resp = ex.Message + "_" + Resp;
                OutReqRes.Msg = ErrorCodes.TempError;
            }
            #region APILogOutletRegistration
            var OnboradReq = new OnboardingLog
            {
                APIID = ObjRoundpayApiRequestModel.APIID,
                Method = "OutRegistration",
                Request = sbPostRequest.ToString(),
                Response = Resp,

            };
            new ProcUpdateLogOnboardingReqResp(_dal).Call(OnboradReq);
            #endregion
            return OutReqRes;
        }

        public OutletAPIStatusUpdate ServicePlus(RoundpayApiRequestModel ObjRoundpayApiRequestModel)
        {
            //BBPS,PSA,AEPS
            string Resp = "";
            StringBuilder PosRequest = new StringBuilder();
            var OutReqRes = new OutletAPIStatusUpdate
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            try
            {
                PosRequest.Append(ObjRoundpayApiRequestModel.Token);
                PosRequest.Append("&");
                if (ObjRoundpayApiRequestModel.Scode == ServiceCode.BBPSService)
                {
                    if ((ObjRoundpayApiRequestModel.OTP ?? string.Empty).Length > 3)
                    {
                        PosRequest.AppendFormat("Service={0}_{1}&OutletId={2}", "BBPS", ObjRoundpayApiRequestModel.OTP, ObjRoundpayApiRequestModel.APIOutletID);
                    }
                    else
                    {
                        PosRequest.AppendFormat("Service={0}&OutletId={1}", "BBPS", ObjRoundpayApiRequestModel.APIOutletID);
                    }
                }
                if (ObjRoundpayApiRequestModel.Scode == ServiceCode.AEPS)
                {
                    PosRequest.AppendFormat("Service={0}&OutletId={1}", "AEPS", ObjRoundpayApiRequestModel.APIOutletID);
                }
                if (ObjRoundpayApiRequestModel.Scode == ServiceCode.PSAService)
                {
                    PosRequest.AppendFormat("Service={0}&OutletId={1}", "PSA", ObjRoundpayApiRequestModel.APIOutletID);
                }

                Resp = AppWebRequest.O.CallUsingHttpWebRequest_POST(ObjRoundpayApiRequestModel.BaseUrl.Replace("{FNAME}", "ServicePlus"), PosRequest.ToString());
                if (Validate.O.ValidateJSON(Resp ?? string.Empty))
                {
                    var _apiRes = JsonConvert.DeserializeObject<ServicePlus>(Resp);
                    if (_apiRes != null)
                    {
                        var IsAlready = (_apiRes.MESSAGE ?? string.Empty).Contains("Already Approved");
                        if (_apiRes.STATUS == "SUCCESS" || IsAlready)
                        {
                            OutReqRes.Statuscode = ErrorCodes.One;
                            OutReqRes.Msg = nameof(ErrorCodes.Transaction_Successful);
                            if (ObjRoundpayApiRequestModel.Scode == ServiceCode.BBPSService)
                            {
                                if (_apiRes.OTPSTATUS == ErrorCodes.One.ToString())
                                {
                                    OutReqRes.BBPSStatus = RndKYCOutLetStatus._Pending;
                                    OutReqRes.Msg = ErrorCodes.DMTOSS;
                                    OutReqRes.IsOTPRequired = true;
                                }
                                else
                                {
                                    OutReqRes.BBPSStatus = RndKYCOutLetStatus._Approved;
                                }
                            }
                            else if (ObjRoundpayApiRequestModel.Scode == ServiceCode.AEPS)
                            {
                                OutReqRes.AEPSStatus = RndKYCOutLetStatus._Pending;
                                if ((_apiRes.KEY ?? string.Empty).Length > 5)
                                {
                                    OutReqRes.AEPSURL = _apiRes.KEY;
                                    OutReqRes.AEPSStatus = RndKYCOutLetStatus._Approved;
                                }
                                else
                                {
                                    OutReqRes.Msg = _apiRes.MESSAGE;
                                }

                            }
                            else if (ObjRoundpayApiRequestModel.Scode == ServiceCode.PSAService)
                            {
                                OutReqRes.PSAStatus = RndKYCOutLetStatus._Pending;
                                OutReqRes.PSARequestID = Validate.O.IsNumeric(_apiRes.REQUESTID ?? string.Empty) ? Convert.ToInt32(_apiRes.REQUESTID.Trim()) : 0;
                                OutReqRes.PSAID = _apiRes.PSAID;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "ServicePlus",
                    Error = ex.Message,
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
                OutReqRes.Msg = ErrorCodes.TempError;
                Resp = ex.Message + "_" + Resp;
            }
            #region APILogOutletRegistration
            var OnboradReq = new OnboardingLog
            {
                APIID = ObjRoundpayApiRequestModel.APIID,
                Method = "ServicePlus",
                Request = PosRequest.ToString(),
                Response = Resp,

            };
            new ProcUpdateLogOnboardingReqResp(_dal).Call(OnboradReq);
            #endregion
            return OutReqRes;
        }

        public BCResponse GetBCDetail(RoundpayApiRequestModel ObjRoundpayApiRequestModel)
        {
            string Resp = "";
            StringBuilder PosRequest = new StringBuilder();
            var OutReqRes = new BCResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            try
            {
                PosRequest.Append("_UMobile=acecbefa4a4b527e2e958a0ca0e6b2e5");
                PosRequest.Append(ErrorCodes.CharOneSixty);
                PosRequest.Append("860409041990047");
                PosRequest.Append(ErrorCodes.CharOneSixty);
                PosRequest.Append(ObjRoundpayApiRequestModel.Phone1);
                PosRequest.Append(ErrorCodes.CharOneSixty);
                PosRequest.Append("db53998b73decf33d2982aa508d50dee&_Password=" + ObjRoundpayApiRequestModel.Token.Split('&')[1].Split('=')[1]);
                PosRequest.Append("&AepsUserid=" + ObjRoundpayApiRequestModel.Token.Split('&')[0].Split('=')[1]);
                PosRequest.Append("&AEPSPass=" + ObjRoundpayApiRequestModel.Token.Split('&')[1].Split('=')[1]);
                Resp = AppWebRequest.O.CallUsingHttpWebRequest_GET("http://roundpayapi.com/api/B2BSecureService.asmx/GetBC_Detail?" + PosRequest.ToString());
                if (Validate.O.ValidateJSON(Resp ?? string.Empty))
                {
                    var _apiRes = JsonConvert.DeserializeObject<BCResponse>(Resp);
                    if (_apiRes.RESPONSESTATUS == "1")
                    {
                        OutReqRes.Statuscode = ErrorCodes.One;
                        OutReqRes.Msg = nameof(ErrorCodes.Transaction_Successful);
                        OutReqRes.Table = _apiRes.Table;
                    }
                    else
                    {
                        OutReqRes.Msg = _apiRes.MESSAGE;
                    }
                }
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "GetBCDetail",
                    Error = ex.Message,
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
                OutReqRes.Msg = ErrorCodes.TempError;
                Resp = ex.Message + "_" + Resp;
            }
            #region APILogOutletRegistration
            var OnboradReq = new OnboardingLog
            {
                APIID = ObjRoundpayApiRequestModel.APIID,
                Method = "GetBCDetail",
                Request = "http://roundpayapi.com/api/B2BSecureService.asmx/GetBC_Detail?" + PosRequest.ToString(),
                Response = Resp,

            };
            new ProcUpdateLogOnboardingReqResp(_dal).Call(OnboradReq);
            #endregion
            return OutReqRes;
        }

        public async Task<PSAResponse> CouponRequest(RoundpayApiRequestModel ObjRoundpayApiRequestModel)
        {
            //http://roundpayapi.com/Api/PanCard.asmx/CouponRequest?totalcoupon=string&psaid=string&agentid=string&UMobileNo=string&Password=string
            var psaResponse = new PSAResponse
            {
                Status = RechargeRespType.PENDING,
                Msg = ErrorCodes.NORESPONSE
            };
            string Resp = "";
            StringBuilder PosRequest = new StringBuilder();
            try
            {
                PosRequest.Append(ObjRoundpayApiRequestModel.BaseUrl);
                PosRequest.Append("?");
                PosRequest.Append("totalcoupon=");
                PosRequest.Append(ObjRoundpayApiRequestModel.totalcoupon);
                PosRequest.Append("&psaid=");
                PosRequest.Append(ObjRoundpayApiRequestModel.psaid);
                PosRequest.Append("&agentid=");
                PosRequest.Append(ObjRoundpayApiRequestModel.agentid);
                PosRequest.Append("&");
                PosRequest.Append(ObjRoundpayApiRequestModel.Token);

                PosRequest.Replace("{FNAME}", "CouponRequest");
                psaResponse.Req = PosRequest.ToString();
                Resp = await AppWebRequest.O.CallUsingHttpWebRequest_GETAsync(PosRequest.ToString());
                if (Resp.Contains(@"<?xml"))
                {
                    Resp = Resp.Replace(@" <?xml version=""1.0"" encoding=""utf-8""?>", string.Empty).Trim();
                    var _apiRes = new PANResponseRoundpay();
                    _apiRes = XMLHelper.O.DesrializeToObject(_apiRes, Resp, "PAN", true);
                    if (_apiRes != null)
                    {
                        if (_apiRes.RESPONSESTATUS.ToUpper().Trim() == RechargeRespType._SUCCESS)
                        {
                            psaResponse.Status = RechargeRespType.SUCCESS;
                            psaResponse.Msg = nameof(ErrorCodes.Transaction_Successful);
                            psaResponse.LiveID = _apiRes.REQUESTID;
                        }
                        else
                        {
                            psaResponse.Msg = _apiRes.MESSAGE ?? string.Empty;
                            psaResponse.Status = _apiRes.RESPONSESTATUS.ToUpper().Trim() == RechargeRespType._FAILED ? RechargeRespType.FAILED : RechargeRespType.PENDING;
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
                Resp = ex.Message + "_" + Resp;
            }
            psaResponse.Resp = Resp;
            return psaResponse;
        }

        public GenerateURLResp GenerateToken(GeneralInsuranceDBResponse generalInsuranceDBResponse)
        {
            var res = new GenerateURLResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.NORESPONSE
            };
            var resp = string.Empty;
            try
            {
                resp = AppWebRequest.O.CallUsingHttpWebRequest_GET(generalInsuranceDBResponse.GenerateTokenURL);
                if (!string.IsNullOrEmpty(resp))
                {
                    var _apiRes = new GenerateTokenResponseRP();
                    _apiRes = XMLHelper.O.DesrializeToObject(_apiRes, resp, "TransactionRes", true);
                    if (Convert.ToInt16(_apiRes.Status ?? "0") == ErrorCodes.One)
                    {
                        res.RedirectURL = "/GIRedirect?code=" + APICode.APIWALE + "&prodid=" + generalInsuranceDBResponse.RechType + "&tranid=" + generalInsuranceDBResponse.Token + "&outlet=" + generalInsuranceDBResponse.AgentID;
                        res.Statuscode = ErrorCodes.One;
                        res.Msg = ErrorCodes.SUCCESS;
                    }
                    else
                    {
                        res.Statuscode = ErrorCodes.Minus1;
                        res.Msg = "Service Down";
                    }
                }

            }
            catch (Exception ex)
            {
            }
            return res;
        }
    }

}





