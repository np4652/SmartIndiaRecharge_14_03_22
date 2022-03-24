using Fintech.AppCode.Configuration;
using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using RoundpayFinTech.AppCode.DL;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.StaticModel.MoneyTransfer;
using RoundpayFinTech.AppCode.ThirdParty.CyberPlate;
using System.Threading.Tasks;
using RoundpayFinTech.AppCode.Model.BBPS;
using Fintech.AppCode.HelperClass;
using RoundpayFinTech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.MiddleLayer.Dmt_Api;
using RoundpayFinTech.AppCode.ThirdParty.BillAvenue;
using RoundpayFinTech.AppCode.Configuration;
using RoundpayFinTech.AppCode.Model.Recharge;
using RoundpayFinTech.AppCode.ThirdParty.AxisBank;
using RoundpayFinTech.AppCode.ThirdParty.PayU;
using System;
using Validators;

namespace RoundpayFinTech.AppCode.MiddleLayer
{
    public class BBPSML
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        private readonly ISession _session;
        private readonly IDAL _dal;
        private readonly IConnectionConfiguration _c;
        private readonly IRequestInfo _rinfo;
        private readonly LoginResponse _lr;
        private readonly IUserML userML;
        public BBPSML(IHttpContextAccessor accessor, IHostingEnvironment env, bool IsSessionCheck = true)
        {
            _accessor = accessor;
            _env = env;
            _c = new ConnectionConfiguration(_accessor, _env);
            _dal = new DAL(_c.GetConnectionString());
            _rinfo = new RequestInfo(_accessor, _env);
            if (IsSessionCheck)
            {
                _session = _accessor.HttpContext.Session;
                _lr = _session.GetObjectFromJson<LoginResponse>(SessionKeys.LoginResponse);
                userML = new UserML(_lr);
            }
        }
        public BBPSML(LoginResponse lr) => _lr = lr;
        public async Task<BBPSResponse> FetchBillML(TransactionServiceReq _req)
        {
            var bbpsResponse = new BBPSResponse
            {
                Statuscode = ErrorCodes.Minus1,
                ErrorMsg = ErrorCodes.URLNOTFOUND
            };
            bool IsBBPSInStaging = false;
            if (_lr.LoginTypeID == LoginType.ApplicationUser)
            {
                _req.UserID = _lr.UserID;
                _req.RequestModeID = RequestMode.PANEL;
                _req.RequestIP = _rinfo.GetRemoteIP();
                _req.OutletID = _lr.OutletID;
                _req.Browser = _rinfo.GetBrowserFullInfo();
                IProcedureAsync proc = new ProcGenerateBillFetchURL(_dal);
                var tresp = (TransactionServiceResp)await proc.Call(_req).ConfigureAwait(false);
                if (tresp.Statuscode == ErrorCodes.One)
                {
                    if (tresp.CurrentAPI != null)
                    {
                        IsBBPSInStaging = tresp.CurrentAPI.APICode.Equals(APICode.BILLAVENUE);
                        if (string.IsNullOrEmpty(tresp.CurrentAPI.URL))
                        {
                            bbpsResponse.ErrorMsg = ErrorCodes.URLNOTFOUND;
                            bbpsResponse.IsEditable = true;
                        }
                        else
                        {
                            var objBBPSLog = new BBPSLog
                            {
                                APIID = tresp.CurrentAPI.ID,
                                OPID = _req.OID.ToString(),
                                SessionNo = "A" + tresp.TID,
                                BillNumber = _req.AccountNo,
                                RequestURL = tresp.CurrentAPI.URL,
                                UserID = _req.UserID,
                                Optional1 = _req.Optional1,
                                Optional2 = _req.Optional2,
                                Optional3 = _req.Optional3,
                                Optional4 = _req.Optional4,
                                PAN = tresp.PAN,
                                AadharNo = tresp.Aadhar,
                                CustomerName = tresp.CustomerName,
                                CustomerMobile = _req.CustomerNumber,
                                CircleCode = tresp.CircleCode,
                                AccountNumber = _req.AccountNo,
                                Pincode = tresp.Pincode,
                                GeoLocation = _req.GEOCode,
                                aPIDetail = tresp.CurrentAPI,
                                Amount = tresp.CurrentAPI.APICode == APICode.CYBERPLAT ? 100.00M : _req.AmountR,
                                APIReqHelper = new BBPSAPIReqHelper
                                {
                                    AccountNoKey = tresp.AccountNoKey,
                                    RegxAccount = tresp.RegxAccount,
                                    BillerID = tresp.BillerID,
                                    InitChanel = tresp.InitChanel,
                                    IPAddress = _req.RequestIP,
                                    MAC = tresp.MAC,
                                    EarlyPaymentAmountKey = tresp.EarlyPaymentAmountKey,
                                    LatePaymentAmountKey = tresp.LatePaymentAmountKey,
                                    EarlyPaymentDateKey = tresp.EarlyPaymentDateKey,
                                    OpParams = tresp.OpParams,
                                    BillMonthKey = tresp.BillMonthKey,
                                    APIOpTypeID = tresp.APIOpType
                                },
                                OutletMobileNo = tresp.OutletMobile
                            };
                            if (objBBPSLog.helper == null)
                            {
                                objBBPSLog.helper = new BBPSLogReqHelper();
                            }
                            if (objBBPSLog.helper.tpBFAInfo == null)
                            {
                                objBBPSLog.helper.tpBFAInfo = new System.Data.DataTable();
                                objBBPSLog.helper.tpBFAInfo.Columns.Add("InfoName", typeof(string));
                                objBBPSLog.helper.tpBFAInfo.Columns.Add("InfoValue", typeof(string));
                                objBBPSLog.helper.tpBFAInfo.Columns.Add("Ind", typeof(int));
                            }
                            if (objBBPSLog.helper.tpBFAmountOps == null)
                            {
                                objBBPSLog.helper.tpBFAmountOps = new System.Data.DataTable();
                                objBBPSLog.helper.tpBFAmountOps.Columns.Add("AmountKey", typeof(string));
                                objBBPSLog.helper.tpBFAmountOps.Columns.Add("AmountValue", typeof(string));
                                objBBPSLog.helper.tpBFAmountOps.Columns.Add("Ind", typeof(int));
                            }
                            if (objBBPSLog.helper.tpBFInputParam == null)
                            {
                                objBBPSLog.helper.tpBFInputParam = new System.Data.DataTable();
                                objBBPSLog.helper.tpBFInputParam.Columns.Add("ParamName", typeof(string));
                                objBBPSLog.helper.tpBFInputParam.Columns.Add("ParamValue", typeof(string));
                                objBBPSLog.helper.tpBFInputParam.Columns.Add("Ind", typeof(int));
                            }
                            var opml = new OperatorML(_accessor, _env);
                            objBBPSLog.APIOptionalList = opml.AOPMapping(tresp.CurrentAPI.ID, _req.OID);
                            var TransHelper = new TransactionHelper(_dal, _accessor, _env);
                            bbpsResponse = await TransHelper.HitFetchBillAPI(objBBPSLog).ConfigureAwait(false);
                            //if (!bbpsResponse.IsHardCoded)
                            //{
                            //    await TransHelper.ResponseHandlerFetchBill(bbpsResponse, objBBPSLog).ConfigureAwait(false);
                            //}
                            if (bbpsResponse.Statuscode == ErrorCodes.One)
                            {
                                bbpsResponse.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                            }
                        }
                    }
                    else
                    {
                        bbpsResponse.IsShowMsgOnly = true;
                        bbpsResponse.ErrorMsg = "(NFA)Operator Down!";
                        bbpsResponse.Statuscode = ErrorCodes.Minus1;
                    }
                }
                else
                {
                    bbpsResponse.IsShowMsgOnly = true;
                    bbpsResponse.ErrorMsg = tresp.Msg;
                }
                if (tresp.ExactNess != EXACTNESS.Exact && bbpsResponse.Statuscode != ErrorCodes.Minus1)
                {
                    bbpsResponse.IsEditable = true;
                    bbpsResponse.IsEnablePayment = true;
                }
                bbpsResponse.BillerPaymentModes = tresp.BillerPaymentModes;
                bbpsResponse.Exactness = tresp.ExactNess;
            }
            bbpsResponse.ErrorMsg = string.IsNullOrEmpty(bbpsResponse.ErrorMsg) ? bbpsResponse.Msg : bbpsResponse.ErrorMsg;
            bbpsResponse.RefferenceID = string.IsNullOrEmpty(bbpsResponse.RefferenceID) ? "RefferenceID" : bbpsResponse.RefferenceID;
            bbpsResponse.IsBBPSInStaging = ApplicationSetting.IsBBPSInStaging && IsBBPSInStaging;
            return bbpsResponse;
        }
        public async Task<BBPSResponse> FetchBillMLApp(TransactionServiceReq _req)
        {
            var bbpsResponse = new BBPSResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.URLNOTFOUND
            };
            //bool IsBBPSInStaging = false;
            _req.RequestIP = _rinfo.GetRemoteIP();
            _req.Browser = _rinfo.GetBrowserFullInfo();
            IProcedureAsync proc = new ProcGenerateBillFetchURL(_dal);
            var tresp = (TransactionServiceResp)await proc.Call(_req).ConfigureAwait(false);
            if (tresp.Statuscode == ErrorCodes.One)
            {
                if (tresp.CurrentAPI != null)
                {
                    //IsBBPSInStaging = tresp.CurrentAPI.APICode.Equals(APICode.BILLAVENUE);
                    if (string.IsNullOrEmpty(tresp.CurrentAPI.URL))
                    {
                        bbpsResponse.ErrorMsg = ErrorCodes.URLNOTFOUND;
                        bbpsResponse.IsEditable = true;
                    }
                    else
                    {
                        var objBBPSLog = new BBPSLog
                        {
                            APIID = tresp.CurrentAPI.ID,
                            OPID = _req.OID.ToString(),
                            SessionNo = "A" + tresp.TID,
                            BillNumber = _req.AccountNo,
                            RequestURL = tresp.CurrentAPI.URL,
                            UserID = _req.UserID,
                            Optional1 = _req.Optional1,
                            Optional2 = _req.Optional2,
                            Optional3 = _req.Optional3,
                            Optional4 = _req.Optional4,
                            PAN = tresp.PAN,
                            AadharNo = tresp.Aadhar,
                            CustomerName = tresp.CustomerName,
                            CustomerMobile = _req.CustomerNumber,
                            CircleCode = tresp.CircleCode,
                            AccountNumber = _req.AccountNo,
                            Pincode = tresp.Pincode,
                            GeoLocation = _req.GEOCode,
                            aPIDetail = tresp.CurrentAPI,
                            Amount = tresp.CurrentAPI.APICode == APICode.CYBERPLAT ? 100.00M : _req.AmountR,
                            APIReqHelper = new BBPSAPIReqHelper
                            {
                                AccountNoKey = tresp.AccountNoKey,
                                RegxAccount = tresp.RegxAccount,
                                BillerID = tresp.BillerID,
                                InitChanel = tresp.InitChanel,
                                IPAddress = _req.RequestIP,
                                MAC = tresp.MAC,
                                EarlyPaymentAmountKey = tresp.EarlyPaymentAmountKey,
                                LatePaymentAmountKey = tresp.LatePaymentAmountKey,
                                EarlyPaymentDateKey = tresp.EarlyPaymentDateKey,
                                OpParams = tresp.OpParams,
                                BillMonthKey = tresp.BillMonthKey,
                                APIOpTypeID = tresp.APIOpType
                            },
                            OutletMobileNo = tresp.OutletMobile,
                            IMEI = _req.IMEI
                        };
                        if (objBBPSLog.helper == null)
                        {
                            objBBPSLog.helper = new BBPSLogReqHelper();
                        }
                        if (objBBPSLog.helper.tpBFAInfo == null)
                        {
                            objBBPSLog.helper.tpBFAInfo = new System.Data.DataTable();
                            objBBPSLog.helper.tpBFAInfo.Columns.Add("InfoName", typeof(string));
                            objBBPSLog.helper.tpBFAInfo.Columns.Add("InfoValue", typeof(string));
                            objBBPSLog.helper.tpBFAInfo.Columns.Add("Ind", typeof(int));
                        }
                        if (objBBPSLog.helper.tpBFAmountOps == null)
                        {
                            objBBPSLog.helper.tpBFAmountOps = new System.Data.DataTable();
                            objBBPSLog.helper.tpBFAmountOps.Columns.Add("AmountKey", typeof(string));
                            objBBPSLog.helper.tpBFAmountOps.Columns.Add("AmountValue", typeof(string));
                            objBBPSLog.helper.tpBFAmountOps.Columns.Add("Ind", typeof(int));
                        }
                        if (objBBPSLog.helper.tpBFInputParam == null)
                        {
                            objBBPSLog.helper.tpBFInputParam = new System.Data.DataTable();
                            objBBPSLog.helper.tpBFInputParam.Columns.Add("ParamName", typeof(string));
                            objBBPSLog.helper.tpBFInputParam.Columns.Add("ParamValue", typeof(string));
                            objBBPSLog.helper.tpBFInputParam.Columns.Add("Ind", typeof(int));
                        }
                        var opml = new OperatorML(_accessor, _env, false);
                        objBBPSLog.APIOptionalList = opml.AOPMappingAPP(tresp.CurrentAPI.ID, _req.OID);
                        var TransHelper = new TransactionHelper(_dal, _accessor, _env);
                        bbpsResponse = await TransHelper.HitFetchBillAPI(objBBPSLog).ConfigureAwait(false);
                        //if (!bbpsResponse.IsHardCoded)
                        //{
                        //    await TransHelper.ResponseHandlerFetchBill(bbpsResponse, objBBPSLog).ConfigureAwait(false);
                        //}
                        if (bbpsResponse.Statuscode == ErrorCodes.One)
                        {
                            bbpsResponse.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                        }
                    }
                }
                else
                {
                    bbpsResponse.IsShowMsgOnly = true;
                    bbpsResponse.ErrorMsg = "(NFA)Operator Down!";
                    bbpsResponse.Statuscode = ErrorCodes.Minus1;
                }
            }
            else
            {
                bbpsResponse.IsShowMsgOnly = true;
                bbpsResponse.ErrorMsg = tresp.Msg;
            }
            bbpsResponse.ErrorMsg = string.IsNullOrEmpty(bbpsResponse.ErrorMsg) ? bbpsResponse.Msg : bbpsResponse.ErrorMsg;
            if (tresp.ExactNess != EXACTNESS.Exact && bbpsResponse.Statuscode != ErrorCodes.Minus1)
            {
                bbpsResponse.IsEditable = true;
                bbpsResponse.IsEnablePayment = true;
            }
            bbpsResponse.RefferenceID = string.IsNullOrEmpty(bbpsResponse.RefferenceID) ? "RefferenceID" : bbpsResponse.RefferenceID;
            bbpsResponse.Exactness = tresp.ExactNess;
            return bbpsResponse;
        }
        #region OnboardSection
        public ValidateOuletModel CheckOutletStatusForServices(CheckOutletStatusReqModel checkOutletStatusReq)
        {

            var resp = new ValidateOuletModel
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = nameof(ErrorCodes.Invalid_Access),
                ErrorCode = ErrorCodes.Invalid_Access,
                IsConfirmation = true,
                IsShowMsg = true,
                ResponseStatusForAPI = new OnboardAPIResponseStatus
                {
                    Statuscode = ErrorCodes.Minus1,
                    Msg = nameof(ErrorCodes.Invalid_Access),
                    ErrorCode = ErrorCodes.Invalid_Access,
                }
            };
            if (checkOutletStatusReq.LoginTypeID == LoginType.ApplicationUser && checkOutletStatusReq.LoginID > 1)
            {
                IProcedure _proc = new ProcValidateOutletForOperator(_dal);
                var CheckOutLetStatusResp = (ValidateAPIOutletResp)_proc.Call(new CommonReq
                {
                    LoginID = checkOutletStatusReq.LoginID,
                    CommonInt = checkOutletStatusReq.OutletID,
                    CommonInt2 = checkOutletStatusReq.OID,
                    CommonStr = checkOutletStatusReq.SPKey,
                    CommonInt3 = checkOutletStatusReq.PartnerID
                });
                resp.IsTakeCustomerNum = CheckOutLetStatusResp.IsTakeCustomerNum;
                resp.CommonStr4 = CheckOutLetStatusResp.PANID;
                resp.ResponseStatusForAPI.ServiceCode = CheckOutLetStatusResp.SCode;
                resp.ResponseStatusForAPI.KYCStatus = CheckOutLetStatusResp.KYCStatus;
                resp.ResponseStatusForAPI.VerifyStatus = CheckOutLetStatusResp.VerifyStatus;
                CheckOutLetStatusResp.OTP = checkOutletStatusReq.OTP;
                CheckOutLetStatusResp.OTPRefID = checkOutletStatusReq.OTPRefID;
                CheckOutLetStatusResp.PIDATA = checkOutletStatusReq.PidData;
                CheckOutLetStatusResp.BioAuthType = checkOutletStatusReq.BioAuthType;
                CheckOutLetStatusResp.IP = _rinfo.GetRemoteIP() == "::1" ? "122.176.71.56" : _rinfo.GetRemoteIP();

                if (!string.IsNullOrEmpty(checkOutletStatusReq.PidData))
                {
                    PidData pidData = new PidData();
                    pidData = XMLHelper.O.DesrializeToObject(pidData, checkOutletStatusReq.PidData, "PidData", true);
                    CheckOutLetStatusResp.pidData = pidData;
                }
                if (CheckOutLetStatusResp.Statuscode == ErrorCodes.Minus1)
                {
                    resp.Statuscode = CheckOutLetStatusResp.Statuscode;
                    resp.Msg = CheckOutLetStatusResp.Msg;
                    resp.ErrorCode = CheckOutLetStatusResp.ErrorCode;
                    resp.ResponseStatusForAPI.Msg = resp.Msg;
                    resp.ResponseStatusForAPI.ErrorCode = resp.ErrorCode;
                    if (resp.Msg.Contains("(Redirect)"))
                    {
                        resp.Msg = resp.Msg.Replace("(Redirect)", "");
                        resp.IsRedirection = true;
                        return resp;
                    }
                    foreach (var item in ErrorCodes.DownStrs)
                    {
                        if (resp.IsDown)
                            break;
                        if (resp.Msg.Contains(item))
                            resp.IsDown = true;
                    }
                    if (!resp.IsDown)
                    {
                        if (resp.Msg.Contains("(Unauthorized)"))
                        {
                            resp.IsUnathorized = true;
                        }
                    }
                    return resp;
                }
                if (CheckOutLetStatusResp.APICode.In(APICode.PANMITRA, APICode.CASHPOINTINDIA) && !CheckOutLetStatusResp.SCode.Equals(ServiceCode.PSAService))
                {
                    resp.Statuscode = ErrorCodes.Minus1;
                    resp.Msg = "Invalid request to process";
                    return resp;
                }
                if (checkOutletStatusReq.RMode == RequestMode.SDK && (!CheckOutLetStatusResp.SCode.In(ServiceCode.AEPS, ServiceCode.MiniBank) || checkOutletStatusReq.OutletID < 10000))
                {
                    resp.Statuscode = ErrorCodes.Minus1;
                    resp.Msg = "Invalid request to process";
                    return resp;
                }
                if (CheckOutLetStatusResp.APICode.EndsWith("FNTH")) // && CheckOutLetStatusResp.IsOutletRequired
                {
                    IOnboardingML onboardingML1 = new OnboardingML(_accessor, _env);
                    if (onboardingML1.ISKYCRequired(CheckOutLetStatusResp) == false)
                    {
                        resp.Statuscode = ErrorCodes.One;
                        resp.Msg = ErrorCodes.KYCNOTREQUIRED;
                        resp.ErrorCode = ErrorCodes.Transaction_Successful;
                        resp.IsConfirmation = false;
                        resp.IsShowMsg = false;
                        return resp;
                    }
                }
                if (CheckOutLetStatusResp.SCode.In(ServiceCode.AEPS, ServiceCode.MiniBank) && CheckOutLetStatusResp.APICode.In(APICode.FINGPAY, APICode.SPRINT, APICode.INSTANTPAY, APICode.RPFINTECH, APICode.TPFINTECH) && CheckOutLetStatusResp.KYCStatus == KYCStatusType.COMPLETED && CheckOutLetStatusResp.AEPSStatus == UserStatus.ACTIVE)
                {
                    resp.Statuscode = ErrorCodes.One;
                    resp.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", "");
                    resp.ErrorCode = ErrorCodes.Transaction_Successful;
                    resp.InInterface = CheckOutLetStatusResp.APICode.Equals(APICode.FINGPAY);
                    resp.InterfaceType = AEPSInterfaceType.FINGPAY;
                    if (CheckOutLetStatusResp.APICode == APICode.FINGPAY && CheckOutLetStatusResp.APIEKYCStatus != UserStatus.NOTREGISTRED)
                    {
                        var TwowayAuthRequired = string.IsNullOrEmpty(CheckOutLetStatusResp.TwoWayAuthDate);
                        if (TwowayAuthRequired == false)
                        {
                            TwowayAuthRequired = (DateTime.Now - Convert.ToDateTime(CheckOutLetStatusResp.TwoWayAuthDate)).Days > 0;
                        }
                        if (TwowayAuthRequired)
                        {
                            resp.IsBioMetricRequired = true;
                            resp.BioAuthType = BioMetricAuthType.Transactional;
                            if (checkOutletStatusReq.IsVerifyBiometric && checkOutletStatusReq.BioAuthType == BioMetricAuthType.Transactional && string.IsNullOrEmpty(checkOutletStatusReq.PidData) == false)
                            {
                                var onboardingML = new OnboardingML(_accessor, _env);
                                var twowayResp = onboardingML.DoTwowayAuthentication(CheckOutLetStatusResp);
                                if (twowayResp.Statuscode == ErrorCodes.One)
                                {
                                    resp.IsBioMetricRequired = false;
                                    resp.BioAuthType = 0;
                                }
                                else
                                {
                                    resp.Statuscode = ErrorCodes.Minus1;
                                    resp.Msg = twowayResp.Msg;
                                }
                            }
                            resp.ResponseStatusForAPI.Statuscode = resp.Statuscode;
                            resp.ResponseStatusForAPI.Msg = resp.Msg;
                            resp.ResponseStatusForAPI.IsBioMetricRequired = resp.IsBioMetricRequired;
                            resp.ResponseStatusForAPI.BioAuthType = resp.BioAuthType;
                            return resp;
                        }
                    }
                    if (CheckOutLetStatusResp.APICode.In(APICode.RPFINTECH, APICode.TPFINTECH))
                    {
                        resp.InterfaceType = CheckOutLetStatusResp.APICode == APICode.RPFINTECH ? AEPSInterfaceType.RPFINTECH : AEPSInterfaceType.TECHPAY;
                    }
                    else if (CheckOutLetStatusResp.APICode == APICode.SPRINT)
                    {
                        resp.InInterface = true;
                        resp.InterfaceType = AEPSInterfaceType.SPRINT;
                    }
                    else if (CheckOutLetStatusResp.APICode == APICode.INSTANTPAY)
                    {
                        resp.InInterface = true;
                        resp.InterfaceType = AEPSInterfaceType.INSTANTPAY;
                    }
                    if (resp.InterfaceType.In(AEPSInterfaceType.FINGPAY, AEPSInterfaceType.SPRINT, AEPSInterfaceType.INSTANTPAY))
                    {
                        if (checkOutletStatusReq.RMode.In(RequestMode.APPS, RequestMode.SDK))
                        {

                            IAPISetting aPISetting = new OnboardingML(_accessor, _env);
                            string APIPartnerID = "APIPartnerID", API;
                            if (resp.InterfaceType == AEPSInterfaceType.SPRINT)
                            {
                                var seting = aPISetting.GetSprintSetting();
                                APIPartnerID = seting.PartnerID;
                            }
                            else
                            {
                                var fingPaySetting = aPISetting.GetFingpay();
                                APIPartnerID = fingPaySetting.superMerchantId;
                            }

                            resp.SDKDetail = new AppSDKDetail
                            {
                                APIOutletID = CheckOutLetStatusResp.AEPSID,
                                APIOutletPassword = "1234",//HashEncryption.O.MD5Hash("1234").ToLower(),
                                APIPartnerID = APIPartnerID,
                                APIOutletMob = CheckOutLetStatusResp.OutletMobile
                            };
                            resp.SDKType = resp.InterfaceType;
                            resp.InterfaceType = AEPSInterfaceType.NO;
                            resp.InInterface = false;
                            if (CheckOutLetStatusResp.APICode == APICode.SPRINT)
                                return resp;
                        }
                        else if (checkOutletStatusReq.RMode == RequestMode.API)
                        {
                            IAPIUserMiddleLayer _apiML = new APIUserML(_accessor, _env);
                            var genres = _apiML.GenerateAEPSURL(new PartnerAPIRequest
                            {
                                t = resp.InterfaceType,
                                PartnerID = CheckOutLetStatusResp.PartnerID,
                                UserID = checkOutletStatusReq.LoginID,
                                OutletID = checkOutletStatusReq.OutletID,
                                Token = checkOutletStatusReq.Token
                            }).Result;
                            if (genres.Statuscode == ErrorCodes.Minus1)
                            {
                                resp.Statuscode = ErrorCodes.Minus1;
                                resp.Msg = genres.Msg;
                                resp.ResponseStatusForAPI.Statuscode = resp.Statuscode;
                                resp.ResponseStatusForAPI.Msg = resp.Msg;
                                resp.ErrorCode = ErrorCodes.Invalid_Parameter;
                                resp.ResponseStatusForAPI.ErrorCode = ErrorCodes.Invalid_Parameter;
                                return resp;
                            }
                            else
                            {
                                resp.Msg = ErrorCodes.ServiceActivate.Replace("{SERVICE}", "AEPS");
                                resp.ErrorCode = ErrorCodes.Transaction_Successful;
                                resp.IsConfirmation = false;
                                resp.CommonStr2 = genres.RedirectURL;
                                resp.Statuscode = ErrorCodes.One;

                                resp.ResponseStatusForAPI.Statuscode = ErrorCodes.One;
                                resp.ResponseStatusForAPI.Msg = resp.Msg;
                                resp.ResponseStatusForAPI.ErrorCode = resp.ErrorCode;
                                resp.ResponseStatusForAPI.KYCStatus = CheckOutLetStatusResp.KYCStatus;
                                resp.ResponseStatusForAPI.ServiceOutletID = checkOutletStatusReq.OutletID.ToString();
                                resp.ResponseStatusForAPI.ServiceStatus = CheckOutLetStatusResp.AEPSStatus;
                                resp.ResponseStatusForAPI.RedirectURL = genres.RedirectURL;
                            }
                        }
                    }
                    if (resp.InterfaceType.In(AEPSInterfaceType.RPFINTECH, AEPSInterfaceType.TECHPAY))
                    {
                        if (CheckOutLetStatusResp.SCode.In(ServiceCode.MiniBank, ServiceCode.AEPS) && checkOutletStatusReq.RMode.In(RequestMode.APPS, RequestMode.SDK))
                        {
                            resp.SDKType = resp.InterfaceType == AEPSInterfaceType.RPFINTECH ? AEPSInterfaceType.RPFINTECH : AEPSInterfaceType.TECHPAY;
                            resp.InterfaceType = AEPSInterfaceType.NO;
                            resp.InInterface = false;
                            var aPISetting = new FintechAPIML(_accessor, _env, CheckOutLetStatusResp.APICode, CheckOutLetStatusResp.APIID, _dal);
                            var fintechSetting = aPISetting.AppSetting();
                            resp.SDKDetail = new AppSDKDetail
                            {
                                APIOutletID = CheckOutLetStatusResp.AEPSID,
                                APIOutletPassword = fintechSetting.PIN,
                                APIPartnerID = fintechSetting.UserID.ToString(),
                                APIOutletMob = CheckOutLetStatusResp.MobileNo
                            };
                        }
                    }
                    if (checkOutletStatusReq.RMode.In(RequestMode.APPS, RequestMode.SDK, RequestMode.API) && CheckOutLetStatusResp.APIEKYCStatus != UserStatus.NOTREGISTRED)
                    {
                        resp.ResponseStatusForAPI.Statuscode = resp.Statuscode;
                        resp.ResponseStatusForAPI.Msg = resp.Msg;
                        resp.ResponseStatusForAPI.ErrorCode = resp.ErrorCode;
                        resp.ResponseStatusForAPI.ServiceStatus = CheckOutLetStatusResp.AEPSStatus;
                        resp.ResponseStatusForAPI.ServiceOutletID = checkOutletStatusReq.OutletID.ToString();
                        return resp;
                    }
                }
                if (!CheckOutLetStatusResp.IsOutletRequired || (CheckOutLetStatusResp.IsOutletRequired && (CheckOutLetStatusResp.FixedOutletID ?? string.Empty).Length > 0 && (CheckOutLetStatusResp.SCode.In(ServiceCode.MoneyTransfer, ServiceCode.BBPSService))))
                {
                    //Outlet not required                    
                    if (CheckOutLetStatusResp.SCode.Equals(ServiceCode.AEPS) || CheckOutLetStatusResp.SCode.Equals(ServiceCode.PSAService))
                    {
                        resp.Msg = ErrorCodes.SystemErrorDown;
                        resp.ErrorCode = ErrorCodes.ServiceDown;
                        resp.IsConfirmation = true;
                    }
                    else
                    {
                        if (CheckOutLetStatusResp.SCode.Equals(ServiceCode.GenralInsurance))
                        {
                            if ((CheckOutLetStatusResp.APICode ?? string.Empty).Equals(string.Empty))
                            {
                                resp.Msg = "(NACF)" + ErrorCodes.Down;
                                resp.ErrorCode = 152;
                                resp.IsDown = true;
                                resp.ResponseStatusForAPI.Statuscode = resp.Statuscode;
                                resp.ResponseStatusForAPI.Msg = resp.Msg;
                                resp.ResponseStatusForAPI.ErrorCode = resp.ErrorCode;
                                return resp;
                            }
                            var GIML = new GeneralInsuranceML(_accessor, _env);
                            var GIMLResp = GIML.GenerateTokenAndURLForRedirection(CheckOutLetStatusResp, checkOutletStatusReq.RMode);
                            resp.Msg = GIMLResp.Msg;
                            if (GIMLResp.Statuscode == ErrorCodes.Minus1)
                            {
                                resp.ResponseStatusForAPI.Statuscode = resp.Statuscode;
                                resp.ResponseStatusForAPI.Msg = resp.Msg;
                                resp.ResponseStatusForAPI.ErrorCode = resp.ErrorCode;
                                return resp;
                            }
                            resp.Statuscode = GIMLResp.Statuscode;
                            resp.IsRedirection = true;
                            resp.CommonStr2 = GIMLResp.RedirectURL;
                            resp.ErrorCode = ErrorCodes.Transaction_Successful;
                            resp.IsConfirmation = false;
                            resp.ResponseStatusForAPI.Statuscode = resp.Statuscode;
                            resp.ResponseStatusForAPI.Msg = resp.Msg;
                            resp.ResponseStatusForAPI.ErrorCode = resp.ErrorCode;
                            resp.ResponseStatusForAPI.ServiceStatus = CheckOutLetStatusResp.BBPSStatus;
                            resp.ResponseStatusForAPI.ServiceOutletID = checkOutletStatusReq.OutletID.ToString();
                            return resp;
                        }
                        if (CheckOutLetStatusResp.SCode.Equals(ServiceCode.BBPSService) && !CheckOutLetStatusResp.APICode.In(APICode.RPFINTECH, APICode.TPFINTECH) && CheckOutLetStatusResp.OutletID > 0 && CheckOutLetStatusResp.KYCStatus == KYCStatusType.COMPLETED && checkOutletStatusReq.RMode == RequestMode.API)
                        {
                            resp.ResponseStatusForAPI.Statuscode = ErrorCodes.One;
                            resp.ResponseStatusForAPI.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                            resp.ResponseStatusForAPI.ErrorCode = resp.ErrorCode;
                            resp.ResponseStatusForAPI.ServiceStatus = UserStatus.ACTIVE;
                            resp.ResponseStatusForAPI.ServiceOutletID = checkOutletStatusReq.OutletID.ToString();
                            return resp;
                        }
                        if (CheckOutLetStatusResp.SCode.Equals(ServiceCode.MoneyTransfer) && checkOutletStatusReq.RMode == RequestMode.API)
                        {
                            resp.ResponseStatusForAPI.Statuscode = ErrorCodes.One;
                            resp.ResponseStatusForAPI.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                            resp.ResponseStatusForAPI.ErrorCode = resp.ErrorCode;
                            resp.ResponseStatusForAPI.ServiceStatus = UserStatus.ACTIVE;
                            resp.ResponseStatusForAPI.ServiceOutletID = checkOutletStatusReq.OutletID.ToString();
                            return resp;
                        }
                        resp.Statuscode = ErrorCodes.One;
                        resp.Msg = ErrorCodes.KYCNOTREQUIRED;
                        resp.ErrorCode = ErrorCodes.Transaction_Successful;
                        resp.IsConfirmation = false;
                        resp.IsShowMsg = false;
                    }
                    resp.ResponseStatusForAPI.Statuscode = resp.Statuscode;
                    resp.ResponseStatusForAPI.Msg = resp.Msg;
                    resp.ResponseStatusForAPI.ErrorCode = resp.ErrorCode;
                    return resp;
                }
                if (!CheckOutLetStatusResp.IsOutletFound)
                {
                    //Invalid outlet selection
                    resp.Msg = ErrorCodes.Error212;
                    resp.ErrorCode = ErrorCodes.Document_Not_Completed;
                    resp.IsRedirection = true;
                    resp.IsRejected = true;
                    resp.ResponseStatusForAPI.Statuscode = ErrorCodes.Minus1;
                    resp.ResponseStatusForAPI.Msg = resp.Msg;
                    resp.ResponseStatusForAPI.ErrorCode = resp.ErrorCode;
                    return resp;
                }
                if (CheckOutLetStatusResp.KYCStatus == KYCStatusType.INCOMPLETE)
                {
                    //Complete your KYC
                    resp.Msg = ErrorCodes.Error212;
                    resp.ErrorCode = ErrorCodes.Document_Not_Completed;
                    resp.IsRedirection = true;
                    resp.IsIncomplete = true;
                    resp.ResponseStatusForAPI.Statuscode = ErrorCodes.Minus1;
                    resp.ResponseStatusForAPI.Msg = resp.Msg;
                    resp.ResponseStatusForAPI.ErrorCode = resp.ErrorCode;
                    return resp;
                }
                if (CheckOutLetStatusResp.KYCStatus == KYCStatusType.REJECTED)
                {
                    //Your KYC has been rejected due to admin remark and Complete your KYC
                    resp.Msg = ErrorCodes.KYCREJECTED.Replace("{MSG}", CheckOutLetStatusResp.AdminRejectRemark ?? string.Empty);
                    resp.ErrorCode = ErrorCodes.Document_Not_Completed;
                    resp.IsRedirection = true;
                    resp.IsRejected = true;
                    resp.ResponseStatusForAPI.Statuscode = ErrorCodes.Minus1;
                    resp.ResponseStatusForAPI.Msg = resp.Msg;
                    resp.ResponseStatusForAPI.ErrorCode = resp.ErrorCode;
                    return resp;
                }
                if (CheckOutLetStatusResp.KYCStatus == KYCStatusType.REKYC)
                {
                    //REKYC condition
                    resp.Msg = ErrorCodes.REKYC;
                    resp.ErrorCode = ErrorCodes.Document_ReKYC;
                    resp.IsRedirection = true;
                    resp.IsRejected = true;
                    resp.ResponseStatusForAPI.Statuscode = ErrorCodes.Minus1;
                    resp.ResponseStatusForAPI.Msg = resp.Msg;
                    resp.ResponseStatusForAPI.ErrorCode = resp.ErrorCode;
                    return resp;
                }
                if (CheckOutLetStatusResp.KYCStatus == KYCStatusType.APPLIED)
                {
                    //Applied condition
                    resp.Msg = ErrorCodes.KYCPENDING;
                    resp.ErrorCode = ErrorCodes.Document_Applied;
                    resp.IsWaiting = true;
                    resp.ResponseStatusForAPI.Statuscode = ErrorCodes.Minus1;
                    resp.ResponseStatusForAPI.Msg = resp.Msg;
                    resp.ResponseStatusForAPI.ErrorCode = resp.ErrorCode;
                    return resp;
                }
                if ((CheckOutLetStatusResp.APICode ?? string.Empty).Equals(string.Empty) && !CheckOutLetStatusResp.APIType.In(APITypes.Lapu, APITypes.Manual))
                {
                    resp.Msg = "(NACF)" + ErrorCodes.Down;
                    resp.ErrorCode = 152;
                    resp.IsDown = true;
                    resp.ResponseStatusForAPI.Statuscode = ErrorCodes.Minus1;
                    resp.ResponseStatusForAPI.Msg = resp.Msg;
                    resp.ResponseStatusForAPI.ErrorCode = resp.ErrorCode;
                    return resp;
                }
                if (CheckOutLetStatusResp.APICode == APICode.YESBANK && string.IsNullOrEmpty(CheckOutLetStatusResp.UIDToken) && CheckOutLetStatusResp.SCode.Equals(ServiceCode.MoneyTransfer))
                {
                    resp.Msg = "EKYC required";
                    resp.ErrorCode = ErrorCodes.Document_ReKYC;
                    resp.IsEKYCRequired = true;
                    resp.Aadhar = CheckOutLetStatusResp.AADHAR;
                    resp.ResponseStatusForAPI.Statuscode = ErrorCodes.Minus1;
                    resp.ResponseStatusForAPI.Msg = resp.Msg;
                    resp.ResponseStatusForAPI.ErrorCode = resp.ErrorCode;
                    return resp;
                }
                if (CheckOutLetStatusResp.IsOutletManual)
                {
                    if (CheckOutLetStatusResp.APIOutletVerifyStatus != UserStatus.ACTIVE && (CheckOutLetStatusResp.FixedOutletID ?? string.Empty).Length < 1 && !string.IsNullOrEmpty(CheckOutLetStatusResp.APICode))
                    {
                        if (CheckOutLetStatusResp.SCode.Equals(ServiceCode.Travel))
                        {
                            resp.Msg = ErrorCodes.ServicePending.Replace("{SERVICE}", "Travel");
                            resp.ErrorCode = ErrorCodes.Document_Applied;
                            resp.IsWaiting = true;
                            resp.ResponseStatusForAPI.Statuscode = ErrorCodes.One;
                            resp.ResponseStatusForAPI.Msg = resp.Msg;
                            resp.ResponseStatusForAPI.ErrorCode = resp.ErrorCode;
                            resp.ResponseStatusForAPI.ServiceOutletID = checkOutletStatusReq.OutletID.ToString();
                            resp.ResponseStatusForAPI.ServiceStatus = CheckOutLetStatusResp.RailStatus;
                            return resp;
                        }
                        if (CheckOutLetStatusResp.SCode.Equals(ServiceCode.PSAService))
                        {
                            resp.Msg = ErrorCodes.ServicePending.Replace("{SERVICE}", "PSA");
                            resp.ErrorCode = ErrorCodes.Document_Applied;
                            resp.IsWaiting = true;
                            resp.ResponseStatusForAPI.Statuscode = ErrorCodes.One;
                            resp.ResponseStatusForAPI.Msg = resp.Msg;
                            resp.ResponseStatusForAPI.ErrorCode = resp.ErrorCode;
                            resp.ResponseStatusForAPI.ServiceOutletID = CheckOutLetStatusResp.PANID;
                            resp.ResponseStatusForAPI.ServiceStatus = CheckOutLetStatusResp.PANStatus;
                            return resp;
                        }
                        if (CheckOutLetStatusResp.SCode.Equals(ServiceCode.AEPS))
                        {
                            resp.Msg = ErrorCodes.ServicePending.Replace("{SERVICE}", "AEPS");
                            resp.ErrorCode = ErrorCodes.Document_Applied;
                            resp.IsWaiting = true;
                            resp.ResponseStatusForAPI.Statuscode = ErrorCodes.One;
                            resp.ResponseStatusForAPI.Msg = resp.Msg;
                            resp.ResponseStatusForAPI.ErrorCode = resp.ErrorCode;
                            resp.ResponseStatusForAPI.ServiceOutletID = checkOutletStatusReq.OutletID.ToString();
                            resp.ResponseStatusForAPI.ServiceStatus = CheckOutLetStatusResp.AEPSStatus;
                            return resp;
                        }
                        if (CheckOutLetStatusResp.SCode.Equals(ServiceCode.BBPSService))
                        {
                            resp.Msg = ErrorCodes.ServicePending.Replace("{SERVICE}", "BBPSService");
                            resp.ErrorCode = ErrorCodes.Document_Applied;
                            resp.IsWaiting = true;
                            resp.ResponseStatusForAPI.Statuscode = ErrorCodes.One;
                            resp.ResponseStatusForAPI.Msg = resp.Msg;
                            resp.ResponseStatusForAPI.ErrorCode = resp.ErrorCode;
                            resp.ResponseStatusForAPI.ServiceOutletID = checkOutletStatusReq.OutletID.ToString();
                            resp.ResponseStatusForAPI.ServiceStatus = CheckOutLetStatusResp.BBPSStatus;
                            return resp;
                        }
                        if (CheckOutLetStatusResp.SCode.Equals(ServiceCode.MoneyTransfer))
                        {
                            resp.Msg = ErrorCodes.ServicePending.Replace("{SERVICE}", "DMTService");
                            resp.ErrorCode = ErrorCodes.Document_Applied;
                            resp.IsWaiting = true;
                            resp.ResponseStatusForAPI.Statuscode = ErrorCodes.One;
                            resp.ResponseStatusForAPI.Msg = resp.Msg;
                            resp.ResponseStatusForAPI.ErrorCode = resp.ErrorCode;
                            resp.ResponseStatusForAPI.ServiceOutletID = checkOutletStatusReq.OutletID.ToString();
                            resp.ResponseStatusForAPI.ServiceStatus = CheckOutLetStatusResp.DMTStatus;
                            return resp;
                        }
                        if (CheckOutLetStatusResp.SCode.Equals(ServiceCode.MiniBank))
                        {
                            resp.Msg = ErrorCodes.ServicePending.Replace("{SERVICE}", "MiniBankService");
                            resp.ErrorCode = ErrorCodes.Document_Applied;
                            resp.IsWaiting = true;
                            resp.ResponseStatusForAPI.Statuscode = ErrorCodes.One;
                            resp.ResponseStatusForAPI.Msg = resp.Msg;
                            resp.ResponseStatusForAPI.ErrorCode = resp.ErrorCode;
                            resp.ResponseStatusForAPI.ServiceOutletID = checkOutletStatusReq.OutletID.ToString();
                            resp.ResponseStatusForAPI.ServiceStatus = CheckOutLetStatusResp.AEPSStatus;
                            return resp;
                        }
                    }

                    if (CheckOutLetStatusResp.SCode.Equals(ServiceCode.PSAService) && CheckOutLetStatusResp.PANStatus != UserStatus.ACTIVE)
                    {
                        resp.ResponseStatusForAPI.Statuscode = ErrorCodes.One;
                        resp.ResponseStatusForAPI.Msg = resp.Msg;
                        resp.ResponseStatusForAPI.ErrorCode = resp.ErrorCode;
                        resp.ResponseStatusForAPI.ServiceOutletID = CheckOutLetStatusResp.PANID;
                        resp.ResponseStatusForAPI.ServiceStatus = CheckOutLetStatusResp.PANStatus;
                        if (string.IsNullOrEmpty(CheckOutLetStatusResp.APICode) && CheckOutLetStatusResp.PANStatus.In(UserStatus.NOTREGISTRED, UserStatus.REJECTED))
                        {
                            IOnboardingML onboardingML = new OnboardingML(_accessor, _env);
                            var manPSAResp = onboardingML.CallManualPSA(CheckOutLetStatusResp);
                            resp.Statuscode = manPSAResp.Statuscode;
                            resp.Msg = manPSAResp.Msg;
                            resp.ErrorCode = manPSAResp.ErrorCode;
                            resp.ResponseStatusForAPI.Statuscode = resp.Statuscode;
                            resp.ResponseStatusForAPI.Msg = resp.Msg;
                            resp.ResponseStatusForAPI.ErrorCode = resp.ErrorCode;
                            if (manPSAResp.ResponseStatusForAPI != null)
                            {
                                resp.ResponseStatusForAPI.ServiceOutletID = manPSAResp.ResponseStatusForAPI.ServiceOutletID;
                                resp.ResponseStatusForAPI.ServiceStatus = manPSAResp.ResponseStatusForAPI.ServiceStatus;
                            }
                            return resp;
                        }
                        else
                        {
                            if ((CheckOutLetStatusResp.FixedOutletID ?? string.Empty).Length < 1)
                            {
                                resp.Msg = ErrorCodes.ServicePending.Replace("{SERVICE}", "PSA");
                                resp.ErrorCode = ErrorCodes.Document_Applied;
                                resp.IsWaiting = true;
                                return resp;
                            }
                            else
                            {
                                resp.Msg = ErrorCodes.ServiceActivate.Replace("{SERVICE}", "PSA");
                                resp.ErrorCode = ErrorCodes.Transaction_Successful;
                                resp.IsConfirmation = false;
                                resp.Statuscode = ErrorCodes.One;
                                return resp;
                            }
                        }
                    }

                    if (CheckOutLetStatusResp.SCode.In(ServiceCode.AEPS, ServiceCode.MiniBank))
                    {
                        if (CheckOutLetStatusResp.AEPSStatus != UserStatus.ACTIVE)
                        {
                            if ((CheckOutLetStatusResp.FixedOutletID ?? string.Empty).Length < 1)
                            {
                                resp.Msg = ErrorCodes.ServicePending.Replace("{SERVICE}", "AEPS");
                                resp.ErrorCode = ErrorCodes.Document_Applied;
                                resp.IsWaiting = true;
                                resp.ResponseStatusForAPI.Statuscode = ErrorCodes.One;
                                resp.ResponseStatusForAPI.Msg = resp.Msg;
                                resp.ResponseStatusForAPI.ErrorCode = resp.ErrorCode;
                                resp.ResponseStatusForAPI.ServiceOutletID = checkOutletStatusReq.OutletID.ToString();
                                resp.ResponseStatusForAPI.ServiceStatus = CheckOutLetStatusResp.AEPSStatus;
                                return resp;
                            }
                        }
                        if (CheckOutLetStatusResp.APICode.In(APICode.FINGPAY, APICode.SPRINT))
                        {
                            resp.InInterface = true;
                            resp.InterfaceType = CheckOutLetStatusResp.APICode == APICode.SPRINT ? AEPSInterfaceType.SPRINT : AEPSInterfaceType.FINGPAY;
                            if (CheckOutLetStatusResp.SCode.In(ServiceCode.MiniBank, ServiceCode.AEPS) && checkOutletStatusReq.RMode == RequestMode.APPS)
                            {
                                resp.SDKType = AEPSInterfaceType.FINGPAY;
                                resp.InterfaceType = AEPSInterfaceType.NO;
                                resp.InInterface = false;
                                var APIPartnerID = string.Empty;
                                IAPISetting aPISetting = new OnboardingML(_accessor, _env);
                                if (CheckOutLetStatusResp.APICode == APICode.SPRINT)
                                {
                                    resp.SDKType = AEPSInterfaceType.SPRINT;
                                    var seting = aPISetting.GetSprintSetting();
                                    APIPartnerID = seting.PartnerID;
                                }
                                else
                                {
                                    var fingPaySetting = aPISetting.GetFingpay();
                                    APIPartnerID = fingPaySetting.superMerchantId;
                                }

                                resp.SDKDetail = new AppSDKDetail
                                {
                                    APIOutletID = CheckOutLetStatusResp.AEPSID,
                                    APIOutletPassword = "1234",//HashEncryption.O.MD5Hash("1234").ToLower(),
                                    APIPartnerID = APIPartnerID,
                                    APIOutletMob = CheckOutLetStatusResp.MobileNo,
                                    ServiceOutletPIN = CheckOutLetStatusResp.APIOutletPIN
                                };
                            }
                            resp.ResponseStatusForAPI.Statuscode = ErrorCodes.One;
                            resp.ResponseStatusForAPI.Msg = resp.Msg;
                            resp.ResponseStatusForAPI.ErrorCode = resp.ErrorCode;
                            resp.ResponseStatusForAPI.ServiceOutletID = checkOutletStatusReq.OutletID.ToString();
                            resp.ResponseStatusForAPI.KYCStatus = CheckOutLetStatusResp.KYCStatus;
                            if (CheckOutLetStatusResp.KYCStatus == KYCStatusType.COMPLETED)
                            {
                                resp.ResponseStatusForAPI.ServiceStatus = UserStatus.ACTIVE;
                            }
                            else if (CheckOutLetStatusResp.KYCStatus == KYCStatusType.APPLIED)
                            {
                                resp.ResponseStatusForAPI.ServiceStatus = UserStatus.APPLIED;
                            }
                            else if (CheckOutLetStatusResp.KYCStatus.In(KYCStatusType.REKYC, KYCStatusType.REJECTED))
                            {
                                resp.ResponseStatusForAPI.ServiceStatus = UserStatus.REJECTED;
                            }
                            else
                            {
                                resp.ResponseStatusForAPI.ServiceStatus = CheckOutLetStatusResp.AEPSStatus;
                            }
                            resp.ResponseStatusForAPI.RedirectURL = string.Empty;
                            if (checkOutletStatusReq.RMode == RequestMode.API)
                            {
                                IAPIUserMiddleLayer _apiML = new APIUserML(_accessor, _env);
                                var genres = _apiML.GenerateAEPSURL(new PartnerAPIRequest
                                {
                                    t = resp.InterfaceType,
                                    PartnerID = CheckOutLetStatusResp.PartnerID,
                                    UserID = checkOutletStatusReq.LoginID,
                                    OutletID = checkOutletStatusReq.OutletID,
                                    Token = checkOutletStatusReq.Token
                                }).Result;
                                if (genres.Statuscode == ErrorCodes.Minus1)
                                {
                                    resp.Statuscode = ErrorCodes.Minus1;
                                    resp.Msg = genres.Msg;
                                    return resp;
                                }
                                else
                                {
                                    resp.Msg = ErrorCodes.ServiceActivate.Replace("{SERVICE}", "AEPS");
                                    resp.ErrorCode = ErrorCodes.Transaction_Successful;
                                    resp.IsConfirmation = false;
                                    resp.CommonStr2 = genres.RedirectURL;
                                    resp.Statuscode = ErrorCodes.One;

                                    resp.ResponseStatusForAPI.Statuscode = ErrorCodes.One;
                                    resp.ResponseStatusForAPI.Msg = resp.Msg;
                                    resp.ResponseStatusForAPI.ErrorCode = resp.ErrorCode;
                                    resp.ResponseStatusForAPI.ServiceOutletID = checkOutletStatusReq.OutletID.ToString();
                                    resp.ResponseStatusForAPI.RedirectURL = genres.RedirectURL;

                                }
                            }
                            return resp;
                        }
                        if (CheckOutLetStatusResp.APICode == APICode.MOSAMBEE)
                        {
                            resp.InInterface = true;
                            resp.InterfaceType = AEPSInterfaceType.MOSAMBEE;
                            if (CheckOutLetStatusResp.SCode.In(ServiceCode.MiniBank, ServiceCode.AEPS) && checkOutletStatusReq.RMode == RequestMode.APPS)
                            {
                                resp.SDKType = AEPSInterfaceType.MOSAMBEE;
                                resp.InterfaceType = AEPSInterfaceType.NO;
                                resp.InInterface = false;
                                IAPISetting aPISetting = new OnboardingML(_accessor, _env);
                                var fingPaySetting = aPISetting.GetFingpay();
                                resp.SDKDetail = new AppSDKDetail
                                {
                                    APIOutletID = CheckOutLetStatusResp.AEPSID,
                                    APIOutletPassword = CheckOutLetStatusResp.APIOutletPassword,
                                    APIPartnerID = fingPaySetting.superMerchantId,
                                    APIOutletMob = CheckOutLetStatusResp.MobileNo,
                                    ServiceOutletPIN = CheckOutLetStatusResp.APIOutletPIN
                                };
                            }
                            resp.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                            resp.Statuscode = ErrorCodes.One;
                            resp.ResponseStatusForAPI.Statuscode = ErrorCodes.One;
                            resp.ResponseStatusForAPI.Msg = resp.Msg;
                            resp.ResponseStatusForAPI.ErrorCode = resp.ErrorCode;
                            resp.ResponseStatusForAPI.ServiceOutletID = checkOutletStatusReq.OutletID.ToString();
                            resp.ResponseStatusForAPI.KYCStatus = CheckOutLetStatusResp.KYCStatus;
                            if (CheckOutLetStatusResp.KYCStatus == KYCStatusType.COMPLETED)
                            {
                                resp.ResponseStatusForAPI.ServiceStatus = UserStatus.ACTIVE;
                            }
                            else if (CheckOutLetStatusResp.KYCStatus == KYCStatusType.APPLIED)
                            {
                                resp.ResponseStatusForAPI.ServiceStatus = UserStatus.APPLIED;
                            }
                            else if (CheckOutLetStatusResp.KYCStatus.In(KYCStatusType.REKYC, KYCStatusType.REJECTED))
                            {
                                resp.ResponseStatusForAPI.ServiceStatus = UserStatus.REJECTED;
                            }
                            else
                            {
                                resp.ResponseStatusForAPI.ServiceStatus = CheckOutLetStatusResp.AEPSStatus;
                            }
                            resp.ResponseStatusForAPI.RedirectURL = string.Empty;
                            if (checkOutletStatusReq.RMode == RequestMode.API)
                            {
                                IAPIUserMiddleLayer _apiML = new APIUserML(_accessor, _env);
                                var genres = _apiML.GenerateAEPSURL(new PartnerAPIRequest
                                {
                                    t = AEPSInterfaceType.MOSAMBEE,
                                    PartnerID = CheckOutLetStatusResp.PartnerID,
                                    UserID = checkOutletStatusReq.LoginID,
                                    OutletID = checkOutletStatusReq.OutletID,
                                    Token = checkOutletStatusReq.Token
                                }).Result;
                                if (genres.Statuscode == ErrorCodes.Minus1)
                                {
                                    resp.Statuscode = ErrorCodes.Minus1;
                                    resp.Msg = genres.Msg;
                                    return resp;
                                }
                                else
                                {
                                    resp.Msg = ErrorCodes.ServiceActivate.Replace("{SERVICE}", "AEPS");
                                    resp.ErrorCode = ErrorCodes.Transaction_Successful;
                                    resp.IsConfirmation = false;
                                    resp.CommonStr2 = genres.RedirectURL;
                                    resp.Statuscode = ErrorCodes.One;

                                    resp.ResponseStatusForAPI.Statuscode = ErrorCodes.One;
                                    resp.ResponseStatusForAPI.Msg = resp.Msg;
                                    resp.ResponseStatusForAPI.ErrorCode = resp.ErrorCode;
                                    resp.ResponseStatusForAPI.ServiceOutletID = checkOutletStatusReq.OutletID.ToString();
                                    resp.ResponseStatusForAPI.RedirectURL = genres.RedirectURL;

                                }
                            }
                            return resp;
                        }
                        if (CheckOutLetStatusResp.APICode.In(APICode.RPFINTECH, APICode.TPFINTECH))
                        {
                            resp.InInterface = true;
                            resp.InterfaceType = CheckOutLetStatusResp.APICode == APICode.RPFINTECH ? AEPSInterfaceType.RPFINTECH : AEPSInterfaceType.TECHPAY;
                            if (CheckOutLetStatusResp.SCode.In(ServiceCode.MiniBank, ServiceCode.AEPS) && checkOutletStatusReq.RMode.In(RequestMode.APPS, RequestMode.SDK))
                            {
                                resp.SDKType = CheckOutLetStatusResp.APICode == APICode.RPFINTECH ? AEPSInterfaceType.RPFINTECH : AEPSInterfaceType.TECHPAY;
                                resp.InterfaceType = AEPSInterfaceType.NO;
                                resp.InInterface = false;
                                var aPISetting = new FintechAPIML(_accessor, _env, CheckOutLetStatusResp.APICode, CheckOutLetStatusResp.APIID, _dal);
                                var fintechSetting = aPISetting.AppSetting();
                                resp.SDKDetail = new AppSDKDetail
                                {
                                    APIOutletID = CheckOutLetStatusResp.AEPSID,
                                    APIOutletPassword = fintechSetting.PIN,
                                    APIPartnerID = fintechSetting.UserID.ToString(),
                                    APIOutletMob = CheckOutLetStatusResp.MobileNo
                                };
                            }
                            resp.ResponseStatusForAPI.Statuscode = ErrorCodes.One;
                            resp.ResponseStatusForAPI.Msg = resp.Msg;
                            resp.ResponseStatusForAPI.ErrorCode = resp.ErrorCode;
                            resp.ResponseStatusForAPI.ServiceOutletID = checkOutletStatusReq.OutletID.ToString();
                            resp.ResponseStatusForAPI.ServiceStatus = CheckOutLetStatusResp.AEPSStatus;
                            resp.ResponseStatusForAPI.RedirectURL = string.Empty;
                            return resp;
                        }
                    }
                    if (CheckOutLetStatusResp.SCode.Equals(ServiceCode.BBPSService) && CheckOutLetStatusResp.BBPSStatus != UserStatus.ACTIVE)
                    {
                        if ((CheckOutLetStatusResp.FixedOutletID ?? string.Empty).Length < 1)
                        {
                            resp.Msg = ErrorCodes.ServicePending.Replace("{SERVICE}", "BBPSService");
                            resp.ErrorCode = ErrorCodes.Document_Applied;
                            resp.IsWaiting = true;
                            resp.ResponseStatusForAPI.Statuscode = ErrorCodes.One;
                            resp.ResponseStatusForAPI.Msg = resp.Msg;
                            resp.ResponseStatusForAPI.ErrorCode = resp.ErrorCode;
                            resp.ResponseStatusForAPI.ServiceOutletID = checkOutletStatusReq.OutletID.ToString();
                            resp.ResponseStatusForAPI.ServiceStatus = CheckOutLetStatusResp.BBPSStatus;
                            return resp;
                        }
                        else
                        {
                            resp.Msg = ErrorCodes.ServiceActivate.Replace("{SERVICE}", "BBPS");
                            resp.IsConfirmation = false;
                            resp.Statuscode = ErrorCodes.One;
                            resp.IsShowMsg = false;
                            resp.ResponseStatusForAPI.Statuscode = ErrorCodes.One;
                            resp.ResponseStatusForAPI.Msg = resp.Msg;
                            resp.ResponseStatusForAPI.ErrorCode = resp.ErrorCode;
                            resp.ResponseStatusForAPI.ServiceOutletID = checkOutletStatusReq.OutletID.ToString();
                            resp.ResponseStatusForAPI.ServiceStatus = UserStatus.ACTIVE;
                        }

                    }
                    if (CheckOutLetStatusResp.SCode.Equals(ServiceCode.Travel) && CheckOutLetStatusResp.RailStatus != UserStatus.ACTIVE)
                    {
                        if ((CheckOutLetStatusResp.FixedOutletID ?? string.Empty).Length < 1)
                        {
                            resp.Msg = ErrorCodes.ServicePending.Replace("{SERVICE}", "Travel");
                            resp.ErrorCode = ErrorCodes.Document_Applied;
                            resp.IsWaiting = true;
                            resp.ResponseStatusForAPI.Statuscode = ErrorCodes.One;
                            resp.ResponseStatusForAPI.Msg = resp.Msg;
                            resp.ResponseStatusForAPI.ErrorCode = resp.ErrorCode;
                            resp.ResponseStatusForAPI.ServiceOutletID = checkOutletStatusReq.OutletID.ToString();
                            resp.ResponseStatusForAPI.ServiceStatus = CheckOutLetStatusResp.RailStatus;
                            return resp;
                        }
                        else
                        {
                            resp.Msg = ErrorCodes.ServiceActivate.Replace("{SERVICE}", "Travel");
                            resp.IsConfirmation = false;
                            resp.Statuscode = ErrorCodes.One;
                            resp.IsShowMsg = false;
                            resp.ResponseStatusForAPI.Statuscode = ErrorCodes.One;
                            resp.ResponseStatusForAPI.Msg = resp.Msg;
                            resp.ResponseStatusForAPI.ErrorCode = resp.ErrorCode;
                            resp.ResponseStatusForAPI.ServiceOutletID = checkOutletStatusReq.OutletID.ToString();
                            resp.ResponseStatusForAPI.ServiceStatus = UserStatus.ACTIVE;
                        }

                    }
                    if (CheckOutLetStatusResp.SCode.Equals(ServiceCode.MoneyTransfer) && CheckOutLetStatusResp.DMTStatus != UserStatus.ACTIVE)
                    {
                        resp.ResponseStatusForAPI.Statuscode = ErrorCodes.One;
                        resp.ResponseStatusForAPI.Msg = resp.Msg;
                        resp.ResponseStatusForAPI.ErrorCode = resp.ErrorCode;
                        resp.ResponseStatusForAPI.ServiceOutletID = checkOutletStatusReq.OutletID.ToString();
                        if ((CheckOutLetStatusResp.FixedOutletID ?? string.Empty).Length < 1)
                        {
                            resp.Msg = ErrorCodes.ServicePending.Replace("{SERVICE}", "DMTService");
                            resp.ErrorCode = ErrorCodes.Document_Applied;
                            resp.IsWaiting = true;
                            resp.ResponseStatusForAPI.ServiceStatus = CheckOutLetStatusResp.DMTStatus;
                            return resp;
                        }
                        else
                        {
                            resp.Msg = ErrorCodes.ServiceActivate.Replace("{SERVICE}", "DMT");
                            resp.ErrorCode = ErrorCodes.Transaction_Successful;
                            resp.IsConfirmation = false;
                            resp.IsShowMsg = false;
                            resp.Statuscode = ErrorCodes.One;
                            resp.ResponseStatusForAPI.ServiceStatus = UserStatus.ACTIVE;
                            return resp;
                        }
                    }
                }

                var _OutletServicePlusReqModel = new OutletServicePlusReqModel
                {
                    _ValidateAPIOutletResp = CheckOutLetStatusResp,
                    IsOnboarding = (CheckOutLetStatusResp.APIOutletVerifyStatus == UserStatus.NOTREGISTRED || CheckOutLetStatusResp.APIOutletVerifyStatus == UserStatus.REJECTED),
                    ServicPlusOTP = checkOutletStatusReq.OTP ?? string.Empty,
                    RequestIP = _rinfo.GetRemoteIP()
                };
                if (CheckOutLetStatusResp.SCode.Equals(ServiceCode.PSAService) && CheckOutLetStatusResp.APICode.In(APICode.MAHAGRAM, APICode.PANMITRA, APICode.CASHPOINTINDIA) && (CheckOutLetStatusResp.PANStatus == UserStatus.NOTREGISTRED || CheckOutLetStatusResp.PANStatus == UserStatus.REJECTED))
                {
                    _OutletServicePlusReqModel.IsOnboarding = true;
                }

                if (CheckOutLetStatusResp.SCode.In(ServiceCode.AEPS, ServiceCode.MoneyTransfer, ServiceCode.MiniBank) && CheckOutLetStatusResp.APICode == APICode.MAHAGRAM)
                {
                    _OutletServicePlusReqModel.IsOnboarding = CheckOutLetStatusResp.AEPSStatus.In(UserStatus.NOTREGISTRED) || CheckOutLetStatusResp.DMTStatus.In(UserStatus.NOTREGISTRED);
                }
                if (CheckOutLetStatusResp.APICode.In(APICode.FINGPAY))
                {
                    if (CheckOutLetStatusResp.SCode.In(ServiceCode.AEPS, ServiceCode.MiniBank))
                    {
                        if (CheckOutLetStatusResp.AEPSStatus.In(UserStatus.NOTREGISTRED, UserStatus.REJECTED))
                        {
                            _OutletServicePlusReqModel.IsOnboarding = true;
                        }
                        else if (CheckOutLetStatusResp.AEPSStatus == UserStatus.APPLIED)
                        {
                            resp.Msg = ErrorCodes.ServicePending.Replace("{SERVICE}", "AEPS");
                            resp.ErrorCode = ErrorCodes.Document_Applied;
                            resp.IsWaiting = true;
                            resp.ResponseStatusForAPI.Statuscode = ErrorCodes.One;
                            resp.ResponseStatusForAPI.Msg = resp.Msg;
                            resp.ResponseStatusForAPI.ErrorCode = resp.ErrorCode;
                            resp.ResponseStatusForAPI.ServiceOutletID = checkOutletStatusReq.OutletID.ToString();
                            resp.ResponseStatusForAPI.ServiceStatus = CheckOutLetStatusResp.AEPSStatus;
                            return resp;
                        }
                        else if (CheckOutLetStatusResp.APIEKYCStatus == UserStatus.NOTREGISTRED)
                        {
                            IOnboardingML onboardingML = new OnboardingML(_accessor, _env);
                            if (CheckOutLetStatusResp.OTPRefID < 1)
                            {
                                //Call Generate OTP                                
                                var gOTPResp = onboardingML.CallGenerateOTP(CheckOutLetStatusResp);
                                if (gOTPResp.IsOTPRequired == true || gOTPResp.Statuscode == ErrorCodes.Minus1)
                                {
                                    resp.Statuscode = gOTPResp.Statuscode;
                                    resp.Msg = gOTPResp.Msg;
                                    resp.OTPRefID = gOTPResp.OTPRefID;
                                    resp.IsOTPRequired = gOTPResp.IsOTPRequired;
                                    resp.ResponseStatusForAPI.Statuscode = resp.Statuscode;
                                    resp.ResponseStatusForAPI.Msg = resp.Msg;
                                    resp.ResponseStatusForAPI.ErrorCode = resp.ErrorCode;
                                    resp.ResponseStatusForAPI.ServiceOutletID = checkOutletStatusReq.OutletID.ToString();
                                    resp.ResponseStatusForAPI.ServiceStatus = CheckOutLetStatusResp.AEPSStatus;
                                    resp.ResponseStatusForAPI.IsOTPRequired = resp.IsOTPRequired;
                                    resp.ResponseStatusForAPI.OTPRefID = resp.OTPRefID;
                                    return resp;
                                }
                            }
                            else
                            {
                                if (checkOutletStatusReq.IsVerifyBiometric == false)
                                {
                                    if (string.IsNullOrEmpty(checkOutletStatusReq.OTP))
                                    {
                                        resp.Statuscode = ErrorCodes.Minus1;
                                        resp.Msg = nameof(ErrorCodes.Invalid_OTP).Replace("_", " ");
                                        resp.ResponseStatusForAPI.Statuscode = resp.Statuscode;
                                        resp.ResponseStatusForAPI.Msg = resp.Msg;
                                        resp.ResponseStatusForAPI.ErrorCode = resp.ErrorCode;
                                        resp.ResponseStatusForAPI.ServiceOutletID = checkOutletStatusReq.OutletID.ToString();
                                        resp.ResponseStatusForAPI.ServiceStatus = CheckOutLetStatusResp.AEPSStatus;
                                        return resp;
                                    }
                                    var validateResp = onboardingML.CallValidateOTP(CheckOutLetStatusResp);
                                    resp.Statuscode = validateResp.Statuscode;
                                    resp.Msg = validateResp.Msg;
                                    resp.IsShowMsg = resp.Statuscode == ErrorCodes.Minus1;
                                    resp.InInterface = resp.IsShowMsg == false;
                                    resp.OTPRefID = validateResp.OTPRefID;
                                    resp.IsBioMetricRequired = validateResp.IsBioMetricRequired;
                                    resp.BioAuthType = BioMetricAuthType.Live;
                                    resp.ResponseStatusForAPI.BioAuthType = resp.BioAuthType;
                                    resp.ResponseStatusForAPI.Statuscode = resp.Statuscode;
                                    resp.ResponseStatusForAPI.Msg = resp.Msg;
                                    resp.ResponseStatusForAPI.ErrorCode = resp.ErrorCode;
                                    resp.ResponseStatusForAPI.ServiceOutletID = checkOutletStatusReq.OutletID.ToString();
                                    resp.ResponseStatusForAPI.ServiceStatus = CheckOutLetStatusResp.AEPSStatus;
                                    resp.ResponseStatusForAPI.IsBioMetricRequired = resp.IsBioMetricRequired;

                                    resp.ResponseStatusForAPI.OTPRefID = resp.OTPRefID;
                                    return resp;
                                }
                                else
                                {
                                    if (string.IsNullOrEmpty(checkOutletStatusReq.PidData))
                                    {
                                        resp.Statuscode = ErrorCodes.Minus1;
                                        resp.Msg = "(Invalid PID data)Should not change anything in capture response should send as it is.";
                                        resp.ResponseStatusForAPI.Statuscode = resp.Statuscode;
                                        resp.ResponseStatusForAPI.Msg = resp.Msg;
                                        resp.ResponseStatusForAPI.ErrorCode = resp.ErrorCode;
                                        resp.ResponseStatusForAPI.ServiceOutletID = checkOutletStatusReq.OutletID.ToString();
                                        resp.ResponseStatusForAPI.ServiceStatus = CheckOutLetStatusResp.AEPSStatus;
                                        return resp;
                                    }
                                    var validateResp = onboardingML.CallValidateBiometric(CheckOutLetStatusResp);
                                    resp.Statuscode = validateResp.Statuscode;
                                    resp.Msg = validateResp.Msg;
                                    resp.IsShowMsg = true;
                                    resp.ResponseStatusForAPI.Statuscode = resp.Statuscode;
                                    resp.ResponseStatusForAPI.Msg = resp.Msg;
                                    resp.ResponseStatusForAPI.ErrorCode = resp.ErrorCode;
                                    resp.ResponseStatusForAPI.ServiceOutletID = checkOutletStatusReq.OutletID.ToString();
                                    resp.ResponseStatusForAPI.ServiceStatus = CheckOutLetStatusResp.AEPSStatus;
                                    return resp;
                                }
                            }
                        }
                        else
                        {
                            var TwowayAuthRequired = string.IsNullOrEmpty(CheckOutLetStatusResp.TwoWayAuthDate);
                            if (TwowayAuthRequired == false)
                            {
                                TwowayAuthRequired = (DateTime.Now - Convert.ToDateTime(CheckOutLetStatusResp.TwoWayAuthDate)).Days > 0;
                            }
                            if (TwowayAuthRequired)
                            {

                            }
                        }
                    }
                    else
                    {
                        return resp;
                    }
                }
                if (CheckOutLetStatusResp.APICode == APICode.SPRINT && CheckOutLetStatusResp.SCode.In(ServiceCode.AEPS, ServiceCode.MiniBank) && CheckOutLetStatusResp.AEPSStatus != UserStatus.ACTIVE)
                {
                    _OutletServicePlusReqModel.IsOnboardingStatusCheck = true;
                    _OutletServicePlusReqModel.IsOnboarding = false;
                }
                if (_OutletServicePlusReqModel.IsOnboarding)
                {
                    IOnboardingML onboardingML = new OnboardingML(_accessor, _env);
                    if (!(CheckOutLetStatusResp.SCode.In(ServiceCode.PSAService) && CheckOutLetStatusResp.APICode.In(APICode.RPFINTECH, APICode.TPFINTECH, APICode.MAHAGRAM, APICode.EKO2)))
                    {
                        if (CheckOutLetStatusResp.APIOutletVerifyStatus != UserStatus.REJECTED)
                        {
                            var outletStsChkResp = onboardingML.CallOnboardingStatusCheck(_OutletServicePlusReqModel, false);
                            if (outletStsChkResp.IsEditProfile)
                            {
                                resp.Msg = ErrorCodes.KYCREJECTED.Replace("{MSG}", "Service Provider");
                                resp.ErrorCode = ErrorCodes.Document_Not_Completed;
                                resp.IsRedirection = true;
                                resp.IsRejected = true;
                                resp.ResponseStatusForAPI.Statuscode = ErrorCodes.One;
                                resp.ResponseStatusForAPI.Msg = resp.Msg;
                                resp.ResponseStatusForAPI.ErrorCode = resp.ErrorCode;
                                resp.ResponseStatusForAPI.KYCStatus = CheckOutLetStatusResp.KYCStatus;
                                resp.ResponseStatusForAPI.ServiceOutletID = checkOutletStatusReq.OutletID.ToString();
                                resp.ResponseStatusForAPI.ServiceStatus = UserStatus.REJECTED;
                                return resp;
                            }
                            if (outletStsChkResp.Statuscode == ErrorCodes.One)
                            {
                                resp.Msg = ErrorCodes.KYCPENDING;
                                resp.ErrorCode = ErrorCodes.Document_Applied;
                                resp.IsWaiting = true;
                                resp.ResponseStatusForAPI.Statuscode = ErrorCodes.One;
                                resp.ResponseStatusForAPI.Msg = resp.Msg;
                                resp.ResponseStatusForAPI.ErrorCode = resp.ErrorCode;
                                resp.ResponseStatusForAPI.KYCStatus = CheckOutLetStatusResp.KYCStatus;
                                resp.ResponseStatusForAPI.ServiceOutletID = checkOutletStatusReq.OutletID.ToString();
                                resp.ResponseStatusForAPI.ServiceStatus = UserStatus.APPLIED;
                                return resp;
                            }
                        }
                    }
                    if (CheckOutLetStatusResp.APICode == APICode.INSTANTPAY)
                    {

                        if (checkOutletStatusReq.OTPRefID < 1)
                        {
                            var otpResp = onboardingML.CallGenerateOTP(_OutletServicePlusReqModel._ValidateAPIOutletResp);
                            resp.Statuscode = otpResp.Statuscode;
                            resp.Msg = otpResp.Msg;
                            resp.ErrorCode = ErrorCodes.OTP_for_verification_has_been_sent_successfully;
                            resp.IsOTPRequired = otpResp.IsOTPRequired;
                            resp.OTPRefID = otpResp.OTPRefID;
                            resp.ResponseStatusForAPI.Statuscode = resp.Statuscode;
                            resp.ResponseStatusForAPI.Msg = resp.Msg;
                            resp.ResponseStatusForAPI.ErrorCode = resp.ErrorCode;
                            resp.ResponseStatusForAPI.IsOTPRequired = resp.IsOTPRequired;
                            resp.ResponseStatusForAPI.OTPRefID = resp.OTPRefID;
                            resp.ResponseStatusForAPI.KYCStatus = CheckOutLetStatusResp.KYCStatus;
                            resp.ResponseStatusForAPI.ServiceStatus = UserStatus.NOTREGISTRED;
                            return resp;
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(checkOutletStatusReq.OTP))
                            {
                                resp.Statuscode = ErrorCodes.Minus1;
                                resp.Msg = nameof(ErrorCodes.Invalid_OTP).Replace("_", " ");
                                resp.ResponseStatusForAPI.Statuscode = resp.Statuscode;
                                resp.ResponseStatusForAPI.Msg = resp.Msg;
                                resp.ResponseStatusForAPI.ErrorCode = resp.ErrorCode;
                                resp.ResponseStatusForAPI.ServiceOutletID = checkOutletStatusReq.OutletID.ToString();
                                resp.ResponseStatusForAPI.ServiceStatus = CheckOutLetStatusResp.AEPSStatus;
                                return resp;
                            }
                            var validateResp = onboardingML.CallValidateOTP(CheckOutLetStatusResp);
                            resp.Statuscode = validateResp.Statuscode;
                            resp.Msg = validateResp.Msg;
                            resp.IsShowMsg = resp.Statuscode == ErrorCodes.Minus1;
                            resp.ResponseStatusForAPI.Statuscode = resp.Statuscode;
                            resp.ResponseStatusForAPI.Msg = resp.Msg;
                            resp.ResponseStatusForAPI.ErrorCode = resp.ErrorCode;
                            resp.ResponseStatusForAPI.ServiceOutletID = checkOutletStatusReq.OutletID.ToString();
                            resp.ResponseStatusForAPI.ServiceStatus = CheckOutLetStatusResp.AEPSStatus;
                            resp.ResponseStatusForAPI.OTPRefID = resp.OTPRefID;
                            return resp;
                        }
                    }
                    var respStatus = onboardingML.CallOnboarding(_OutletServicePlusReqModel);
                    if (respStatus.Statuscode == ErrorCodes.One)
                    {
                        resp.Msg = ErrorCodes.KYCPENDING;
                        resp.ErrorCode = ErrorCodes.Document_Applied;
                        resp.IsWaiting = true;
                        resp.ResponseStatusForAPI.Statuscode = ErrorCodes.One;
                        resp.ResponseStatusForAPI.Msg = resp.Msg;
                        resp.ResponseStatusForAPI.ErrorCode = resp.ErrorCode;
                        resp.ResponseStatusForAPI.KYCStatus = CheckOutLetStatusResp.KYCStatus;
                        resp.ResponseStatusForAPI.ServiceOutletID = CheckOutLetStatusResp.SCode == ServiceCode.PSAService ? string.Empty : checkOutletStatusReq.OutletID.ToString();
                        resp.ResponseStatusForAPI.ServiceStatus = UserStatus.APPLIED;
                        return resp;
                    }
                    resp.Msg = respStatus.Msg;
                    resp.ErrorCode = respStatus.ErrorCode;
                    resp.ResponseStatusForAPI.Statuscode = ErrorCodes.Minus1;
                    resp.ResponseStatusForAPI.Msg = resp.Msg;
                    resp.ResponseStatusForAPI.ErrorCode = resp.ErrorCode;
                    return resp;
                }
                else
                {
                    _OutletServicePlusReqModel.IsOnboardingStatusCheck = CheckOutLetStatusResp.APIOutletVerifyStatus == UserStatus.APPLIED || CheckOutLetStatusResp.APIOutletDocVerifyStatus == UserStatus.APPLIED || CheckOutLetStatusResp.AEPSStatus == UserStatus.APPLIED || CheckOutLetStatusResp.DMTStatus == UserStatus.APPLIED || CheckOutLetStatusResp.APICode == APICode.SPRINT;
                    if (CheckOutLetStatusResp.SCode.Equals(ServiceCode.PSAService) && CheckOutLetStatusResp.APICode == APICode.MAHAGRAM)
                    {
                        _OutletServicePlusReqModel.IsOnboardingStatusCheck = CheckOutLetStatusResp.PANStatus == UserStatus.APPLIED;
                    }
                    if (CheckOutLetStatusResp.SCode.In(ServiceCode.AEPS, ServiceCode.MoneyTransfer, ServiceCode.MiniBank) && CheckOutLetStatusResp.APICode == APICode.MAHAGRAM)
                    {
                        _OutletServicePlusReqModel.IsOnboardingStatusCheck = (CheckOutLetStatusResp.SCode.In(ServiceCode.AEPS, ServiceCode.MiniBank) && CheckOutLetStatusResp.AEPSStatus == UserStatus.APPLIED);
                    }
                    if (CheckOutLetStatusResp.APICode.In(APICode.RPFINTECH, APICode.TPFINTECH) && ((CheckOutLetStatusResp.BBPSStatus.In(UserStatus.APPLIED, UserStatus.ACTIVE) && CheckOutLetStatusResp.SCode == ServiceCode.BBPSService) || (CheckOutLetStatusResp.DMTStatus.In(UserStatus.APPLIED, UserStatus.ACTIVE) && CheckOutLetStatusResp.SCode == ServiceCode.MoneyTransfer) || (CheckOutLetStatusResp.SCode.In(ServiceCode.AEPS, ServiceCode.MiniBank) && CheckOutLetStatusResp.AEPSStatus.In(UserStatus.APPLIED, UserStatus.ACTIVE)) || (CheckOutLetStatusResp.PANStatus.In(UserStatus.APPLIED, UserStatus.ACTIVE, UserStatus.NOTREGISTRED) && CheckOutLetStatusResp.SCode.Equals(ServiceCode.PSAService) && !string.IsNullOrEmpty(CheckOutLetStatusResp.APIOutletID))))
                    {
                        _OutletServicePlusReqModel.IsOnboardingStatusCheck = false;
                    }

                    if (_OutletServicePlusReqModel.IsOnboardingStatusCheck)
                    {
                        IOnboardingML onboardingML = new OnboardingML(_accessor, _env);
                        var outletStsChkResp = onboardingML.CallOnboardingStatusCheck(_OutletServicePlusReqModel, true);
                        if (outletStsChkResp.IsEditProfile)
                        {
                            resp.Msg = ErrorCodes.KYCREJECTED.Replace("{MSG}", "Service Provider");
                            resp.ErrorCode = ErrorCodes.Document_Not_Completed;
                            resp.IsRedirection = true;
                            resp.IsRejected = true;
                            resp.ResponseStatusForAPI.Statuscode = ErrorCodes.One;
                            resp.ResponseStatusForAPI.Msg = resp.Msg;
                            resp.ResponseStatusForAPI.ErrorCode = resp.ErrorCode;
                            resp.ResponseStatusForAPI.KYCStatus = CheckOutLetStatusResp.KYCStatus;
                            resp.ResponseStatusForAPI.ServiceOutletID = CheckOutLetStatusResp.SCode == ServiceCode.PSAService ? string.Empty : checkOutletStatusReq.OutletID.ToString();
                            resp.ResponseStatusForAPI.ServiceStatus = UserStatus.REJECTED;
                            return resp;
                        }
                        if (outletStsChkResp.IsRedirection)
                        {
                            resp.Statuscode = ErrorCodes.One;
                            resp.Msg = "Redirect to URL";
                            resp.ErrorCode = outletStsChkResp.ErrorCode;
                            resp.IsRedirectToExternal = true;
                            resp.IsConfirmation = false;
                            resp.CommonStr = outletStsChkResp.RedirectionUrI;
                            resp.ResponseStatusForAPI.Statuscode = resp.Statuscode;
                            resp.ResponseStatusForAPI.Msg = resp.Msg;
                            resp.ResponseStatusForAPI.ErrorCode = resp.ErrorCode;
                            resp.ResponseStatusForAPI.KYCStatus = CheckOutLetStatusResp.KYCStatus;
                            resp.ResponseStatusForAPI.ServiceOutletID = CheckOutLetStatusResp.SCode == ServiceCode.PSAService ? string.Empty : checkOutletStatusReq.OutletID.ToString();
                            resp.ResponseStatusForAPI.ServiceStatus = UserStatus.APPLIED;
                            if (checkOutletStatusReq.RMode == RequestMode.APPS || checkOutletStatusReq.RMode == RequestMode.SDK && CheckOutLetStatusResp.APICode == APICode.SPRINT)
                            {
                                SprintBBPSML sbML = new SprintBBPSML(_accessor, _env, _dal);
                                var sprintAppSetting = sbML.GetSetting();
                                resp.SDKType = AEPSInterfaceType.SPRINT;
                                resp.SDKDetail = new AppSDKDetail
                                {
                                    APIOutletID= checkOutletStatusReq.OutletID.ToString(),
                                    APIOutletMob= CheckOutLetStatusResp.OutletMobile,
                                    OutletName=CheckOutLetStatusResp.OutletName,
                                    APIPartnerID= sprintAppSetting.PartnerID,
                                    EmailID= CheckOutLetStatusResp.EmailID,
                                    ServiceOutletPIN= sprintAppSetting.JWTKey,
                                };
                            }
                            return resp;
                        }
                        if (outletStsChkResp.Statuscode == ErrorCodes.One)
                        {
                            resp.Msg = ErrorCodes.KYCPENDING;
                            resp.ErrorCode = ErrorCodes.Document_Applied;
                            resp.IsWaiting = true;
                            resp.ResponseStatusForAPI.Statuscode = ErrorCodes.One;
                            resp.ResponseStatusForAPI.Msg = resp.Msg;
                            resp.ResponseStatusForAPI.ErrorCode = resp.ErrorCode;
                            resp.ResponseStatusForAPI.KYCStatus = CheckOutLetStatusResp.KYCStatus;
                            resp.ResponseStatusForAPI.ServiceOutletID = CheckOutLetStatusResp.SCode == ServiceCode.PSAService ? string.Empty : checkOutletStatusReq.OutletID.ToString();

                            resp.ResponseStatusForAPI.ServiceStatus = UserStatus.APPLIED;
                            return resp;
                        }
                        resp.Msg = outletStsChkResp.Msg;
                        resp.ErrorCode = outletStsChkResp.ErrorCode;
                        resp.ResponseStatusForAPI.Statuscode = ErrorCodes.Minus1;
                        resp.ResponseStatusForAPI.Msg = resp.Msg;
                        resp.ResponseStatusForAPI.ErrorCode = resp.ErrorCode;
                        return resp;
                    }

                }
                if (!_OutletServicePlusReqModel.IsOnboardingStatusCheck)
                {
                    if (CheckOutLetStatusResp.IsBBPS)
                    {
                        _OutletServicePlusReqModel._ValidateAPIOutletResp.SCode = ServiceCode.BBPSService;
                        _OutletServicePlusReqModel.IsBBPSServicePlus = CheckOutLetStatusResp.BBPSStatus != UserStatus.ACTIVE;
                        if (_OutletServicePlusReqModel.IsBBPSServicePlus)
                        {
                            IOnboardingML onboardingML = new OnboardingML(_accessor, _env);
                            var sPlusResp = onboardingML.CallBBPSServicePlus(_OutletServicePlusReqModel);
                            if (sPlusResp.Statuscode == ErrorCodes.One)
                            {
                                resp.IsOTPRequired = sPlusResp.IsOTPRequired;
                                resp.ResponseStatusForAPI.IsOTPRequired = sPlusResp.IsOTPRequired;
                                resp.ResponseStatusForAPI.Statuscode = ErrorCodes.One;
                                resp.ResponseStatusForAPI.Msg = resp.Msg;
                                resp.ResponseStatusForAPI.ErrorCode = resp.ErrorCode;
                                if (resp.IsOTPRequired)
                                {
                                    resp.ResponseStatusForAPI.KYCStatus = UserStatus.APPLIED;
                                    resp.ResponseStatusForAPI.ServiceOutletID = checkOutletStatusReq.OutletID.ToString();
                                    resp.ResponseStatusForAPI.ServiceStatus = UserStatus.APPLIED;
                                    resp.Msg = "To activate BBPS service validate OTP";
                                }
                                else
                                {
                                    resp.Msg = ErrorCodes.ServiceActivate.Replace("{SERVICE}", "BBPS");
                                    resp.IsConfirmation = false;
                                    resp.Statuscode = ErrorCodes.One;
                                    resp.IsShowMsg = false;
                                    resp.ResponseStatusForAPI.KYCStatus = UserStatus.APPLIED;
                                    resp.ResponseStatusForAPI.ServiceOutletID = checkOutletStatusReq.OutletID.ToString();
                                    resp.ResponseStatusForAPI.ServiceStatus = UserStatus.APPLIED;
                                }
                            }
                            else
                            {
                                resp.Msg = ErrorCodes.NotActivateService.Replace("{SERVICE}", "BBPS");
                                resp.ResponseStatusForAPI.Statuscode = ErrorCodes.Minus1;
                                resp.ResponseStatusForAPI.Msg = resp.Msg;
                                resp.ResponseStatusForAPI.ErrorCode = resp.ErrorCode;
                            }
                        }
                        else
                        {
                            resp.Msg = ErrorCodes.ServiceActivate.Replace("{SERVICE}", "BBPS");
                            resp.IsConfirmation = false;
                            resp.Statuscode = ErrorCodes.One;
                            resp.IsShowMsg = false;
                            resp.ResponseStatusForAPI.Statuscode = ErrorCodes.One;
                            resp.ResponseStatusForAPI.Msg = resp.Msg;
                            resp.ResponseStatusForAPI.ErrorCode = resp.ErrorCode;
                            resp.ResponseStatusForAPI.KYCStatus = UserStatus.ACTIVE;
                            resp.ResponseStatusForAPI.ServiceOutletID = checkOutletStatusReq.OutletID.ToString();
                            resp.ResponseStatusForAPI.ServiceStatus = UserStatus.ACTIVE;
                        }
                    }
                    else if (CheckOutLetStatusResp.SCode.In(ServiceCode.AEPS, ServiceCode.MiniBank))
                    {
                        _OutletServicePlusReqModel.IsAEPSServicePlus = CheckOutLetStatusResp.AEPSStatus != UserStatus.ACTIVE;
                        var IsPanel = checkOutletStatusReq.RMode == RequestMode.PANEL;
                        if (_OutletServicePlusReqModel.IsAEPSServicePlus || (checkOutletStatusReq.RMode.In(RequestMode.API, RequestMode.APPS, RequestMode.PANEL)))
                        {
                            IOnboardingML onboardingML = new OnboardingML(_accessor, _env);
                            var sPlusResp = onboardingML.CallAEPSServicePlus(_OutletServicePlusReqModel);
                            if (sPlusResp.Statuscode == ErrorCodes.One)
                            {
                                if (sPlusResp.ServiceStatus.In(UserStatus.APPLIED, UserStatus.NOTREGISTRED))
                                {
                                    resp.Msg = ErrorCodes.ServicePending.Replace("{SERVICE}", "AEPS");
                                    resp.ErrorCode = ErrorCodes.Document_Applied;
                                    resp.IsWaiting = true;

                                    resp.ResponseStatusForAPI.Statuscode = ErrorCodes.One;
                                    resp.ResponseStatusForAPI.Msg = resp.Msg;
                                    resp.ResponseStatusForAPI.ErrorCode = resp.ErrorCode;
                                    resp.ResponseStatusForAPI.KYCStatus = CheckOutLetStatusResp.KYCStatus;
                                    resp.ResponseStatusForAPI.ServiceOutletID = checkOutletStatusReq.OutletID.ToString();
                                    resp.ResponseStatusForAPI.ServiceStatus = sPlusResp.ServiceStatus;
                                    resp.ResponseStatusForAPI.RedirectURL = sPlusResp.AEPSURL;
                                }
                                else if (sPlusResp.ServiceStatus == UserStatus.REJECTED)
                                {
                                    resp.Msg = ErrorCodes.NotActivateService.Replace("{SERVICE}", "AEPS");
                                    resp.ResponseStatusForAPI.Statuscode = ErrorCodes.Minus1;
                                    resp.ResponseStatusForAPI.Msg = resp.Msg;
                                    resp.ResponseStatusForAPI.ErrorCode = resp.ErrorCode;
                                }
                                else
                                {
                                    resp.Msg = ErrorCodes.ServiceActivate.Replace("{SERVICE}", "AEPS");
                                    resp.ErrorCode = ErrorCodes.Transaction_Successful;
                                    resp.IsConfirmation = false;
                                    resp.CommonStr2 = checkOutletStatusReq.RMode == RequestMode.APPS ? string.Empty : sPlusResp.AEPSURL;
                                    resp.Statuscode = ErrorCodes.One;
                                    resp.IsBioMetricRequired = sPlusResp.IsBioMetricRequired;
                                    resp.BioAuthType = sPlusResp.BioAuthType;
                                    resp.ResponseStatusForAPI.BioAuthType = resp.BioAuthType;
                                    resp.IsOTPRequired = sPlusResp.IsOTPRequired;
                                    resp.OTPRefID = sPlusResp.OTPRefID;
                                    resp.ResponseStatusForAPI.Statuscode = ErrorCodes.One;
                                    resp.ResponseStatusForAPI.Msg = resp.Msg;
                                    resp.ResponseStatusForAPI.ErrorCode = resp.ErrorCode;
                                    resp.ResponseStatusForAPI.KYCStatus = CheckOutLetStatusResp.KYCStatus;
                                    resp.ResponseStatusForAPI.ServiceOutletID = checkOutletStatusReq.OutletID.ToString();
                                    resp.ResponseStatusForAPI.ServiceStatus = sPlusResp.ServiceStatus;
                                    resp.ResponseStatusForAPI.RedirectURL = sPlusResp.AEPSURL;
                                    resp.ResponseStatusForAPI.OTPRefID = resp.OTPRefID;
                                    resp.ResponseStatusForAPI.IsOTPRequired = resp.IsOTPRequired;
                                    resp.ResponseStatusForAPI.IsBioMetricRequired = resp.IsBioMetricRequired;
                                }
                                return resp;
                            }
                            else
                            {
                                resp.Msg = ErrorCodes.NotActivateService.Replace("{SERVICE}", "AEPS");
                                resp.ResponseStatusForAPI.Statuscode = ErrorCodes.Minus1;
                                resp.ResponseStatusForAPI.Msg = resp.Msg;
                                resp.ResponseStatusForAPI.ErrorCode = resp.ErrorCode;
                                resp.IsBioMetricRequired = sPlusResp.IsBioMetricRequired;
                                resp.BioAuthType = sPlusResp.BioAuthType;
                                resp.ResponseStatusForAPI.BioAuthType = resp.BioAuthType;
                                resp.IsOTPRequired = sPlusResp.IsOTPRequired;
                                resp.OTPRefID = sPlusResp.OTPRefID;
                                resp.ResponseStatusForAPI.Statuscode = ErrorCodes.One;
                                resp.ResponseStatusForAPI.Msg = resp.Msg;
                                resp.ResponseStatusForAPI.ErrorCode = resp.ErrorCode;
                                resp.ResponseStatusForAPI.KYCStatus = CheckOutLetStatusResp.KYCStatus;
                                resp.ResponseStatusForAPI.ServiceOutletID = checkOutletStatusReq.OutletID.ToString();
                                resp.ResponseStatusForAPI.ServiceStatus = sPlusResp.ServiceStatus;
                                resp.ResponseStatusForAPI.RedirectURL = sPlusResp.AEPSURL;
                                resp.ResponseStatusForAPI.OTPRefID = resp.OTPRefID;
                                resp.ResponseStatusForAPI.IsOTPRequired = resp.IsOTPRequired;
                                resp.ResponseStatusForAPI.IsBioMetricRequired = resp.IsBioMetricRequired;
                            }
                            resp.Msg = sPlusResp.Msg;
                            resp.ErrorCode = sPlusResp.ErrorCode;
                        }
                        else if (!_OutletServicePlusReqModel.IsAEPSServicePlus)
                        {
                            IOnboardingML onboardingML = new OnboardingML(_accessor, _env);
                            var _apiRes = onboardingML.CallBCService(_OutletServicePlusReqModel);
                            if (_apiRes.Statuscode == ErrorCodes.One)
                            {
                                resp.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                                resp.ErrorCode = ErrorCodes.Transaction_Successful;
                                resp.Statuscode = ErrorCodes.One;
                                resp.BCResponse = _apiRes.Table;
                                if (CheckOutLetStatusResp.APICode.In(APICode.MAHAGRAM, APICode.ROUNDPAY) && checkOutletStatusReq.RMode.In(RequestMode.APPS, RequestMode.SDK))
                                {
                                    resp.SDKType = CheckOutLetStatusResp.APICode.Equals(APICode.MAHAGRAM) ? AEPSInterfaceType.MAHAGRAM : AEPSInterfaceType.ROUNDPAY;
                                    resp.InterfaceType = AEPSInterfaceType.NO;
                                    resp.InInterface = false;

                                    if (_apiRes.Table != null)
                                    {
                                        if (_apiRes.Table.Count > 0)
                                        {
                                            resp.SDKDetail = new AppSDKDetail
                                            {
                                                APIOutletID = _apiRes.Table[0].BCID,
                                                APIOutletMob = _apiRes.Table[0].Mobileno,
                                                APIOutletPassword = _apiRes.Table[0].SecretKey,
                                                APIPartnerID = _apiRes.Table[0].CPID,
                                                BCResponse = _apiRes.Table
                                            };
                                        }
                                    }
                                }
                                return resp;
                            }
                            else
                            {
                                resp.Msg = ErrorCodes.NotActivateService.Replace("{SERVICE}", "BC Detail");
                                resp.ErrorCode = _apiRes.ErrorCode;
                                resp.ResponseStatusForAPI.Statuscode = ErrorCodes.Minus1;
                                resp.ResponseStatusForAPI.Msg = resp.Msg;
                                resp.ResponseStatusForAPI.ErrorCode = resp.ErrorCode;
                                return resp;
                            }
                        }
                    }
                    else if (CheckOutLetStatusResp.SCode.Equals(ServiceCode.PSAService))
                    {
                        if (CheckOutLetStatusResp.PANStatus == UserStatus.APPLIED)
                        {
                            IOnboardingML onboardingML = new OnboardingML(_accessor, _env);
                            var outletStsChkResp = onboardingML.CallOnboardingStatusCheck(_OutletServicePlusReqModel, true);
                            if (outletStsChkResp.Statuscode == ErrorCodes.One)
                            {
                                resp.Msg = ErrorCodes.ServicePending.Replace("{SERVICE}", "PSA");
                                resp.ErrorCode = ErrorCodes.Document_Applied;
                                resp.IsWaiting = true;
                                resp.ResponseStatusForAPI.Statuscode = ErrorCodes.One;
                                resp.ResponseStatusForAPI.Msg = resp.Msg;
                                resp.ResponseStatusForAPI.ErrorCode = resp.ErrorCode;
                                resp.ResponseStatusForAPI.KYCStatus = CheckOutLetStatusResp.KYCStatus;
                                resp.ResponseStatusForAPI.ServiceStatus = UserStatus.APPLIED;
                                return resp;
                            }
                        }
                        else
                        {
                            _OutletServicePlusReqModel.IsPANServicePlus = CheckOutLetStatusResp.PANStatus != UserStatus.ACTIVE;
                            if (_OutletServicePlusReqModel.IsPANServicePlus)
                            {
                                IOnboardingML onboardingML = new OnboardingML(_accessor, _env);
                                var sPlusResp = onboardingML.CallPSAServicePlus(_OutletServicePlusReqModel);
                                if (sPlusResp.Statuscode == ErrorCodes.One)
                                {
                                    resp.Msg = ErrorCodes.ServiceActivate.Replace("{SERVICE}", "PSA");
                                    resp.ErrorCode = ErrorCodes.Transaction_Successful;
                                    resp.IsConfirmation = false;
                                    resp.Statuscode = ErrorCodes.One;
                                    resp.ResponseStatusForAPI.Statuscode = ErrorCodes.One;
                                    resp.ResponseStatusForAPI.Msg = resp.Msg;
                                    resp.ResponseStatusForAPI.ErrorCode = resp.ErrorCode;
                                    resp.ResponseStatusForAPI.KYCStatus = CheckOutLetStatusResp.KYCStatus;
                                    resp.ResponseStatusForAPI.ServiceOutletID = CheckOutLetStatusResp.PANID;
                                    resp.ResponseStatusForAPI.ServiceStatus = UserStatus.ACTIVE;
                                    return resp;
                                }
                                resp.Msg = sPlusResp.Msg;
                                resp.ErrorCode = sPlusResp.ErrorCode;
                            }
                            else
                            {
                                if ((CheckOutLetStatusResp.PANID ?? string.Empty).Length == 0)
                                {
                                    resp.Msg = "PSA has been activated but detail could not be found. Contact to customercare";
                                    resp.ErrorCode = ErrorCodes.Unknown_Error;
                                    resp.IsConfirmation = true;
                                    resp.ResponseStatusForAPI.Statuscode = ErrorCodes.Minus1;
                                    resp.ResponseStatusForAPI.Msg = resp.Msg;
                                    resp.ResponseStatusForAPI.ErrorCode = resp.ErrorCode;
                                    resp.ResponseStatusForAPI.KYCStatus = CheckOutLetStatusResp.KYCStatus;
                                }
                                else
                                {
                                    resp.Msg = ErrorCodes.ServiceActivate.Replace("{SERVICE}", "PSA");
                                    resp.ErrorCode = ErrorCodes.Transaction_Successful;
                                    resp.IsConfirmation = false;
                                    resp.Statuscode = ErrorCodes.One;
                                    resp.ResponseStatusForAPI.Statuscode = ErrorCodes.One;
                                    resp.ResponseStatusForAPI.Msg = resp.Msg;
                                    resp.ResponseStatusForAPI.ErrorCode = resp.ErrorCode;
                                    resp.ResponseStatusForAPI.KYCStatus = CheckOutLetStatusResp.KYCStatus;
                                    resp.ResponseStatusForAPI.ServiceOutletID = CheckOutLetStatusResp.PANID;
                                    resp.ResponseStatusForAPI.ServiceStatus = UserStatus.ACTIVE;
                                }
                                return resp;
                            }
                        }

                    }
                    else if (CheckOutLetStatusResp.SCode.Equals(ServiceCode.MoneyTransfer))
                    {
                        _OutletServicePlusReqModel.IsDMRServicePlus = CheckOutLetStatusResp.DMTStatus != UserStatus.ACTIVE;
                        if (_OutletServicePlusReqModel.IsDMRServicePlus)
                        {
                            IOnboardingML onboardingML = new OnboardingML(_accessor, _env);
                            var sPlusResp = onboardingML.CallDMTServicePlus(_OutletServicePlusReqModel);
                            if (sPlusResp.Statuscode == ErrorCodes.One)
                            {
                                resp.Msg = ErrorCodes.ServiceActivate.Replace("{SERVICE}", "DMT");
                                resp.ErrorCode = ErrorCodes.Transaction_Successful;
                                resp.IsConfirmation = false;
                                resp.IsShowMsg = false;
                                resp.Statuscode = ErrorCodes.One;
                                resp.ResponseStatusForAPI.Statuscode = ErrorCodes.One;
                                resp.ResponseStatusForAPI.Msg = resp.Msg;
                                resp.ResponseStatusForAPI.ErrorCode = resp.ErrorCode;
                                resp.ResponseStatusForAPI.KYCStatus = CheckOutLetStatusResp.KYCStatus;
                                resp.ResponseStatusForAPI.ServiceOutletID = checkOutletStatusReq.OutletID.ToString();
                                resp.ResponseStatusForAPI.ServiceStatus = UserStatus.ACTIVE;
                                return resp;
                            }
                            resp.Msg = sPlusResp.Msg;
                            resp.ErrorCode = sPlusResp.ErrorCode;
                        }
                        else
                        {
                            resp.Msg = ErrorCodes.ServiceActivate.Replace("{SERVICE}", "DMT");
                            resp.ErrorCode = ErrorCodes.Transaction_Successful;
                            resp.IsConfirmation = false;
                            resp.IsShowMsg = false;
                            resp.Statuscode = ErrorCodes.One;
                            resp.ResponseStatusForAPI.Statuscode = ErrorCodes.One;
                            resp.ResponseStatusForAPI.Msg = resp.Msg;
                            resp.ResponseStatusForAPI.ErrorCode = resp.ErrorCode;
                            resp.ResponseStatusForAPI.KYCStatus = CheckOutLetStatusResp.KYCStatus;
                            resp.ResponseStatusForAPI.ServiceOutletID = checkOutletStatusReq.OutletID.ToString();
                            resp.ResponseStatusForAPI.ServiceStatus = UserStatus.ACTIVE;
                            return resp;
                        }
                    }
                    else if (CheckOutLetStatusResp.SCode.Equals(ServiceCode.Travel))
                    {
                        _OutletServicePlusReqModel.IsTravelServicePlus = CheckOutLetStatusResp.RailStatus != UserStatus.ACTIVE;
                        if (_OutletServicePlusReqModel.IsTravelServicePlus)
                        {
                            IOnboardingML onboardingML = new OnboardingML(_accessor, _env);
                            var sPlusResp = onboardingML.CallRailServicePlus(_OutletServicePlusReqModel);
                            if (sPlusResp.Statuscode == ErrorCodes.One)
                            {
                                resp.Msg = ErrorCodes.ServiceActivate.Replace("{SERVICE}", "Travel");
                                resp.ErrorCode = ErrorCodes.Transaction_Successful;
                                resp.IsConfirmation = false;
                                resp.IsShowMsg = false;
                                resp.Statuscode = ErrorCodes.One;
                                resp.ResponseStatusForAPI.Statuscode = ErrorCodes.One;
                                resp.ResponseStatusForAPI.Msg = resp.Msg;
                                resp.ResponseStatusForAPI.ErrorCode = resp.ErrorCode;
                                resp.ResponseStatusForAPI.KYCStatus = CheckOutLetStatusResp.KYCStatus;
                                resp.ResponseStatusForAPI.ServiceOutletID = checkOutletStatusReq.OutletID.ToString();
                                resp.ResponseStatusForAPI.ServiceStatus = UserStatus.ACTIVE;
                                return resp;
                            }
                            resp.Msg = sPlusResp.Msg;
                            resp.ErrorCode = sPlusResp.ErrorCode;
                        }
                        else
                        {
                            resp.Msg = ErrorCodes.ServiceActivate.Replace("{SERVICE}", "Travel");
                            resp.ErrorCode = ErrorCodes.Transaction_Successful;
                            resp.IsConfirmation = false;
                            resp.IsShowMsg = false;
                            resp.Statuscode = ErrorCodes.One;
                            resp.ResponseStatusForAPI.Statuscode = ErrorCodes.One;
                            resp.ResponseStatusForAPI.Msg = resp.Msg;
                            resp.ResponseStatusForAPI.ErrorCode = resp.ErrorCode;
                            resp.ResponseStatusForAPI.KYCStatus = CheckOutLetStatusResp.KYCStatus;
                            resp.ResponseStatusForAPI.ServiceOutletID = checkOutletStatusReq.OutletID.ToString();
                            resp.ResponseStatusForAPI.ServiceStatus = UserStatus.ACTIVE;
                            return resp;
                        }
                    }
                    else
                    {
                        resp.Msg = nameof(ErrorCodes.Transaction_Successful);
                        resp.ErrorCode = ErrorCodes.Transaction_Successful;
                        resp.IsConfirmation = false;
                        resp.ResponseStatusForAPI.Statuscode = ErrorCodes.Minus1;
                        resp.ResponseStatusForAPI.Msg = "Invalid Service Selection";
                        resp.ResponseStatusForAPI.ErrorCode = ErrorCodes.Request_Accpeted;
                    }
                }
            }
            return resp;
        }
        #endregion
        private IDAL ChangeConString(string _date)
        {
            var __dal = _dal;
            if (Validate.O.IsDateIn_dd_MMM_yyyy_Format(_date))
            {
                TypeMonthYear typeMonthYear = ConnectionStringHelper.O.GetTypeMonthYear(Convert.ToDateTime(_date.Replace(" ", "/")));
                if (typeMonthYear.ConType != ConnectionStringType.DBCon)
                {
                    __dal = new DAL(_c.GetConnectionString(typeMonthYear.ConType, (typeMonthYear.MM ?? "") + "_" + (typeMonthYear.YYYY ?? "")));
                }
            }
            return __dal;
        }
        public BBPSComplainAPIResponse MakeBBPSComplain(GenerateBBPSComplainProcReq complainReq)
        {
            var __dal = _dal;
            if (!string.IsNullOrEmpty(complainReq.TransactionID))
            {
                string TransDate = ConnectionStringHelper.O.ConvertTransactionIDTo_dd_MMM_yyyy(complainReq.TransactionID);
                __dal = ChangeConString(TransDate);
            }
            var compResp = new BBPSComplainAPIResponse();
            IProcedure proc = new ProcGenerateBBPSComplain(__dal);
            var procRes = (GenerateBBPSComplainProcResp)proc.Call(complainReq);
            compResp.Statuscode = procRes.Statuscode;
            compResp.Msg = procRes.Msg;
            if (procRes.Statuscode == ErrorCodes.One)
            {
                if (procRes.APICode.Equals(APICode.BILLAVENUE))
                {
                    BillAvenueML billAvenueML = new BillAvenueML(_accessor, _env, _dal);
                    compResp = billAvenueML.ComplainBillAvenue(new BBPSComplainRequest
                    {
                        APIOutletID = procRes.APIOutletID,
                        BillerID = procRes.BillerID,
                        TransactionID = procRes.TransactionID,
                        VendorID = procRes.VendorID,
                        ComplainType = complainReq.ComplainType,
                        Description = complainReq.Description,
                        Reason = complainReq.Reason,
                        ParticipationType = complainReq.ParticipationType,
                        MobileNo = complainReq.MobileNo
                    });
                }
                else if (procRes.APICode.Equals(APICode.AXISBANK))
                {
                    AxisBankBBPSML axisBankBBPSML = new AxisBankBBPSML(_accessor, _env, _dal);
                    compResp = axisBankBBPSML.AxisBankRaiseComplain(new BBPSComplainRequest
                    {
                        APIOutletID = procRes.APIOutletID,
                        BillerID = procRes.BillerID,
                        TransactionID = procRes.TransactionID,
                        VendorID = procRes.VendorID,
                        ComplainType = complainReq.ComplainType,
                        Description = complainReq.Description,
                        Reason = complainReq.Reason,
                        ParticipationType = complainReq.ParticipationType,
                        MobileNo = complainReq.MobileNo
                    });
                }
                else if (procRes.APICode.Equals(APICode.PAYUBBPS))
                {
                    PayuBBPSML axisBankBBPSML = new PayuBBPSML(_accessor, _env, _dal);
                    compResp = axisBankBBPSML.RaiseComplain(new BBPSComplainRequest
                    {
                        APIOutletID = procRes.APIOutletID,
                        BillerID = procRes.BillerID,
                        TransactionID = procRes.TransactionID,
                        VendorID = procRes.VendorID,
                        ComplainType = complainReq.ComplainType,
                        Description = complainReq.Description,
                        Reason = complainReq.Reason,
                        ParticipationType = complainReq.ParticipationType,
                        MobileNo = complainReq.MobileNo
                    });
                }
                if (compResp.Statuscode > 0)
                {
                    compResp.TableID = procRes.TableID;
                    IProcedure procUpdate = new ProcUpdateBBPSComplain(_dal);
                    procUpdate.Call(compResp);
                    if (compResp.Statuscode == ErrorCodes.One)
                    {
                        var alertParam = new AlertReplacementModel
                        {
                            WID = procRes.WID,
                            UserMobileNo = complainReq.MobileNo,
                            TransactionID = procRes.TransactionID,
                            FormatID = MessageFormat.BBPSSuccess,
                            DATETIME = DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt")
                        };
                        IAlertML alertMl = new AlertML(_accessor, _env);
                        alertMl.BBPSComplainRegistrationAlert(alertParam);
                    }
                }

            }
            return compResp;
        }
        public BBPSComplainAPIResponse TrackBBPSComplain(CommonReq commonReq)
        {
            var compResp = new BBPSComplainAPIResponse();
            IProcedure proc = new ProcTrackBBPSComplain(_dal);
            var procRes = (GenerateBBPSComplainProcResp)proc.Call(commonReq);
            compResp.Statuscode = procRes.Statuscode;
            compResp.Msg = procRes.Msg;
            if (procRes.Statuscode == ErrorCodes.One)
            {
                if (procRes.APICode.Equals(APICode.BILLAVENUE))
                {
                    BillAvenueML billAvenueML = new BillAvenueML(_accessor, _env, _dal);
                    compResp = billAvenueML.TrackComplainBillAvenue(new BBPSComplainRequest
                    {
                        APIOutletID = procRes.APIOutletID,
                        BillerID = procRes.BillerID,
                        TransactionID = procRes.TransactionID,
                        VendorID = procRes.VendorID
                    });
                }
                else if (procRes.APICode.Equals(APICode.AXISBANK))
                {
                    AxisBankBBPSML axisBankBBPSML = new AxisBankBBPSML(_accessor, _env, _dal);
                    compResp = axisBankBBPSML.AxisBankComplainStatus(new BBPSComplainRequest
                    {
                        APIOutletID = procRes.APIOutletID,
                        BillerID = procRes.BillerID,
                        TransactionID = procRes.TransactionID,
                        VendorID = procRes.VendorID,
                        ComplaintID = procRes.ReferenceID
                    });
                    procRes.ComplainAssignedTo = compResp.ComplainAssignedTo;
                    procRes.ComplainReason = compResp.ComplainReason;
                    procRes.ComplainStatus_ = compResp.ComplainStatus;
                }
                else if (procRes.APICode.Equals(APICode.PAYUBBPS))
                {
                    PayuBBPSML payuBBPSML = new PayuBBPSML(_accessor, _env, _dal);
                    compResp = payuBBPSML.CheckComplainStatus(new BBPSComplainRequest
                    {
                        APIOutletID = procRes.APIOutletID,
                        BillerID = procRes.BillerID,
                        TransactionID = procRes.TransactionID,
                        VendorID = procRes.VendorID
                    });
                    procRes.ComplainAssignedTo = compResp.ComplainAssignedTo;
                    procRes.ComplainReason = compResp.ComplainReason;
                    procRes.ComplainStatus_ = compResp.ComplainStatus;
                }
                if (compResp.Statuscode > 0)
                {
                    compResp.TableID = procRes.TableID;
                    IProcedure procUpdate = new ProcUpdateBBPSComplain(_dal);
                    procUpdate.Call(compResp);
                }
            }
            return compResp;
        }

        public GenerateBBPSComplainProcResp BBPSComplainStatusCheck(string liveid, int comType)
        {
            var commonReq = new CommonReq
            {
                UserID = _lr.UserID,
                CommonStr = liveid,
                CommonInt = 0,
                CommonInt2 = comType
            };

            IProcedure proc = new ProcTrackBBPSComplain(_dal);
            var procRes = (GenerateBBPSComplainProcResp)proc.Call(commonReq);
            if (procRes.Statuscode == ErrorCodes.One)
            {
                if (procRes.APICode.Equals(APICode.BILLAVENUE))
                {
                    BillAvenueML billAvenueML = new BillAvenueML(_accessor, _env, _dal);
                    var compResp = billAvenueML.TrackComplainBillAvenue(new BBPSComplainRequest
                    {
                        APIOutletID = procRes.APIOutletID,
                        BillerID = procRes.BillerID,
                        TransactionID = procRes.TransactionID,
                        VendorID = procRes.VendorID
                    });
                    procRes.ComplainAssignedTo = compResp.ComplainAssignedTo;
                    procRes.ComplainReason = compResp.ComplainReason;
                    procRes.ComplainStatus_ = compResp.ComplainStatus;
                }
                else if (procRes.APICode.Equals(APICode.AXISBANK))
                {
                    AxisBankBBPSML axisBankBBPSML = new AxisBankBBPSML(_accessor, _env, _dal);
                    var compResp = axisBankBBPSML.AxisBankComplainStatus(new BBPSComplainRequest
                    {
                        APIOutletID = procRes.APIOutletID,
                        BillerID = procRes.BillerID,
                        TransactionID = procRes.TransactionID,
                        VendorID = procRes.VendorID,
                        ComplaintID = procRes.ReferenceID
                    });
                    procRes.ComplainAssignedTo = compResp.ComplainAssignedTo;
                    procRes.ComplainReason = compResp.ComplainReason;
                    procRes.ComplainStatus_ = compResp.ComplainStatus;
                }
                else if (procRes.APICode.Equals(APICode.PAYUBBPS))
                {
                    PayuBBPSML payuBBPSML = new PayuBBPSML(_accessor, _env, _dal);
                    var compResp = payuBBPSML.CheckComplainStatus(new BBPSComplainRequest
                    {
                        APIOutletID = procRes.APIOutletID,
                        BillerID = procRes.BillerID,
                        TransactionID = procRes.TransactionID,
                        VendorID = procRes.VendorID
                    });
                    procRes.ComplainAssignedTo = compResp.ComplainAssignedTo;
                    procRes.ComplainReason = compResp.ComplainReason;
                    procRes.ComplainStatus_ = compResp.ComplainStatus;
                }
            }
            return procRes;
        }
    }
}
