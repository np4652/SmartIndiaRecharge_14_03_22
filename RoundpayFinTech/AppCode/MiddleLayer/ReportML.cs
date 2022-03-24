using Fintech.AppCode;
using Fintech.AppCode.Configuration;
using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.Model.Reports;
using Fintech.AppCode.StaticModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using RoundpayFinTech.AppCode.DL;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.Model.Reports.Filter;
using System;
using System.Collections.Generic;
using Validators;
using System.Linq;
using System.Threading.Tasks;
using Fintech.AppCode.HelperClass;
using Fintech.AppCode.WebRequest;
using RoundpayFinTech.Models;
using RoundpayFinTech.AppCode.Model.Report;
using System.Text;
using RoundpayFinTech.AppCode.StaticModel.MoneyTransfer;
using System.Data;
using RoundpayFinTech.AppCode.Configuration;
using RoundpayFinTech.AppCode.MiddleLayer.Dmt_Api;
using RoundpayFinTech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.ThirdParty.Paytm;
using System.IO;
using System.Drawing;
using RoundpayFinTech.AppCode.ThirdParty.Fingpay;
using RoundpayFinTech.AppCode.ThirdParty.Mahagram;
using RoundpayFinTech.AppCode.Model.ROffer;
using RoundpayFinTech.AppCode.Model.Recharge;
using RoundpayFinTech.AppCode.Model.BBPS;
using RoundpayFinTech.AppCode.Model.MoneyTransfer;
using Newtonsoft.Json;
using RoundpayFinTech.AppCode.ThirdParty.HotelAPI;

namespace RoundpayFinTech.AppCode.MiddleLayer
{
    public partial class ReportML : IReportML, IRechargeReportML, IDMRReportML, IRefundReportML, IAppReportML, IOutletML, IBillFetchReportML
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        private readonly ISession _session;
        private IDAL _dal;
        private readonly IConnectionConfiguration _c;
        private readonly IRequestInfo _rinfo;
        private readonly WebsiteInfo _WInfo;
        private readonly LoginResponse _lr;
        private readonly IUserML userML;
        private readonly LoginResponse _lrEmp;
        private string IPGeoDetailURL = "http://api.ipstack.com/{IP}?access_key={Access_Key}";
        public ReportML(IHttpContextAccessor accessor, IHostingEnvironment env, bool InSession = true)
        {
            _accessor = accessor;
            _env = env;
            _c = new ConnectionConfiguration(_accessor, _env);
            _dal = new DAL(_c.GetConnectionString());
            _rinfo = new RequestInfo(_accessor, _env);
            if (InSession)
            {
                _session = _accessor.HttpContext.Session;
                _WInfo = new LoginML(_accessor, _env).GetWebsiteInfo();
                _lr = _session.GetObjectFromJson<LoginResponse>(SessionKeys.LoginResponse);
                userML = new UserML(_lr);
                _lrEmp = _session.GetObjectFromJson<LoginResponse>(SessionKeys.LoginResponseEmp);
            }
        }
        #region RequestMenu
        public IEnumerable<CallbackRequests> CallbackReport(int Top, string SearchText)
        {
            var _res = new List<CallbackRequests>();
            if ((_lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser) || userML.IsCustomerCareAuthorised(ActionCodes.ShowCallbackRequests))
            {
                var req = new CommonReq
                {
                    LoginID = _lr.UserID,
                    CommonInt = Top,
                    CommonStr = SearchText
                };
                IProcedure _proc = new ProcGetCallbackData(_dal);
                _res = (List<CallbackRequests>)_proc.Call(req);
            }
            return _res;
        }
        #endregion

        #region RechargeReportSection
        public async Task<IEnumerable<ProcRechargeReportResponse>> GetUplineCommission(int TID, string TransactionID)
        {
            var _lst = new List<ProcRechargeReportResponse>();

            if (_lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.ShowRechargeReport))
            {
                string TransDate = string.IsNullOrEmpty(TransactionID) ? DateTime.Now.ToString("dd MMM yyyy") : ConnectionStringHelper.O.ConvertTransactionIDTo_dd_MMM_yyyy(TransactionID);
                _dal = ChangeConString(TransDate);
                var req = new CommonReq
                {
                    LoginTypeID = _lr.LoginTypeID,
                    LoginID = _lr.UserID,
                    CommonInt = TID
                };
                IProcedureAsync _proc = new ProcGetUplineCommission(_dal);
                _lst = (List<ProcRechargeReportResponse>)await _proc.Call(req);
            }
            return _lst;
        }
        public async Task<RechargeReportSummary> GetTransactionSummary(int RType)
        {
            if (RType == 0)
            {
                RType = ReportType.Recharge;
            }
            var _res = new RechargeReportSummary();

            var loginResp = chkAlternateSession();
            if (loginResp != null)
            {
                if (loginResp.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.ShowRechargeReport))
                {
                    var procReq = new CommonReq
                    {
                        LoginID = loginResp.LoginTypeID == LoginType.CustomerCare ? 1 : loginResp.UserID,
                        CommonInt = RType
                    };
                    IProcedureAsync _proc = new ProcRechargeReportSummary(_dal);
                    _res = (RechargeReportSummary)await _proc.Call(procReq);
                }
            }
            return _res;
        }


        public async Task<List<ProcRechargeReportResponse>> GetRechargeReport(RechargeReportFilter filter)
        {
            var _lst = new List<ProcRechargeReportResponse>();
            var IsScanMoreDB = false;
            var loginRes = chkAlternateSession();
            if (loginRes != null)
            {
                if (loginRes.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.ShowRechargeReport) || loginRes.LoginTypeID == LoginType.Employee)
                {
                    var validate = Validate.O;
                    var _filter = new _RechargeReportFilter
                    {
                        LT = loginRes.LoginTypeID,
                        LoginID = loginRes.UserID,
                        TopRows = filter.TopRows,
                        Status = filter.Status,
                        APIID = filter.APIID,
                        OID = filter.OID,
                        FromDate = filter.FromDate,
                        ToDate = filter.ToDate,
                        Criteria = filter.Criteria,
                        CriteriaText = filter.CriteriaText,
                        IsExport = filter.IsExport,
                        OPTypeID = filter.OPTypeID,
                        RequestModeID = filter.RequestModeID,
                        CircleID = filter.CircleID,
                        SwitchID = filter.SwitchID,
                    };
                    try
                    {
                        if (filter.Criteria > 0)
                        {
                            if ((filter.CriteriaText ?? "") == "")
                            {
                                return _lst;
                            }
                        }
                        if (_filter.Criteria == Criteria.OutletMobile)
                        {
                            if (!validate.IsMobile(_filter.CriteriaText))
                            {
                                return _lst;
                            }
                            _filter.OutletNo = _filter.CriteriaText;
                        }
                        if (_filter.Criteria == Criteria.AccountNo)
                        {
                            _filter.AccountNo = _filter.CriteriaText;
                            if (string.IsNullOrEmpty(_filter.AccountNo))
                                return _lst;
                            if (ApplicationSetting.IsSingleDB)
                                _filter.FromDate = DateTime.Now.AddMonths(-2).ToString("dd MMM yyyy");
                            else
                                IsScanMoreDB = true;

                        }
                        if (_filter.Criteria == Criteria.TransactionID)
                        {
                            if (_filter.CriteriaText[0] != Criteria.StartCharOFTransaction)
                            {
                                return _lst;
                            }
                            _filter.TransactionID = _filter.CriteriaText;
                        }
                        if (_filter.Criteria == Criteria.TID)
                        {
                            try
                            {
                                _filter.TID = Convert.ToInt32(_filter.CriteriaText);
                            }
                            catch (Exception)
                            {
                                return _lst;
                            }
                        }
                        if (_filter.Criteria == Criteria.APIRequestID)
                        {
                            _filter.APIRequestID = _filter.CriteriaText;
                        }
                        if (_filter.Criteria == Criteria.VendorID)
                        {
                            _filter.VendorID = _filter.CriteriaText;
                        }
                        if (_filter.Criteria == Criteria.CCID)
                        {
                            if (!validate.IsNumeric(_filter.CriteriaText))
                            {
                                return _lst;
                            }
                            _filter.CCID = Convert.ToInt32(_filter.CriteriaText);
                        }
                        if (_filter.Criteria == Criteria.CCMobileNo)
                        {
                            _filter.CCMobileNo = _filter.CriteriaText;
                        }
                        if (_filter.Criteria == Criteria.LiveID)
                        {
                            _filter.LiveID = _filter.CriteriaText;
                        }
                        if (_filter.Criteria == Criteria.UserID)
                        {
                            var Prefix = Validate.O.Prefix(filter.CriteriaText);
                            if (Validate.O.IsNumeric(Prefix))
                                _filter.UserID = Validate.O.IsNumeric(filter.CriteriaText) ? Convert.ToInt32(filter.CriteriaText) : _filter.UserID;
                            var uid = Validate.O.LoginID(filter.CriteriaText);
                            _filter.UserID = Validate.O.IsNumeric(uid) ? Convert.ToInt32(uid) : _filter.UserID;
                        }
                    }
                    catch (Exception)
                    {
                        return _lst;
                    }

                    _filter.FromDate = string.IsNullOrEmpty(_filter.FromDate) ? DateTime.Now.ToString("dd MMM yyyy") : _filter.FromDate;
                    if (IsScanMoreDB && _filter.FromDate == DateTime.Now.ToString("dd MMM yyyy"))
                    {
                        IProcedureAsync _proc = new ProcRechargeReport(_dal);
                        _lst = (List<ProcRechargeReportResponse>)await _proc.Call(_filter).ConfigureAwait(false);
                        _dal = new DAL(_c.GetConnectionString(1));
                        _proc = new ProcRechargeReport(_dal);
                        var _lstSecond = (List<ProcRechargeReportResponse>)await _proc.Call(_filter).ConfigureAwait(false);
                        if (_lst == null)
                        {
                            _lst = _lstSecond;
                        }
                        else if (_lst.Count == 0)
                        {
                            _lst = _lstSecond;
                        }
                        else
                        {
                            _lst.AddRange(_lstSecond);
                        }
                        if (ApplicationSetting.IsMultiMonthDB && _lst.Count == 0)
                        {
                            var fromDate = Convert.ToDateTime(_filter.FromDate.Replace(" ", "/"));
                            if ((fromDate.Month < DateTime.Now.Month && fromDate.Year == DateTime.Now.Year || fromDate.Year < DateTime.Now.Year) || (_filter.Criteria == Criteria.AccountNo && fromDate.Month == DateTime.Now.Month))
                            {
                                if (_filter.Criteria == Criteria.AccountNo && fromDate.Month == DateTime.Now.Month && fromDate.Year <= DateTime.Now.Year)
                                {
                                    fromDate.AddMonths(-1);
                                }
                                _dal = new DAL(_c.GetConnectionString(2, fromDate.ToString("MM_yyyy")));
                                _proc = new ProcRechargeReport(_dal);
                                _lstSecond = (List<ProcRechargeReportResponse>)await _proc.Call(_filter).ConfigureAwait(false);
                                if (_lst == null)
                                {
                                    _lst = _lstSecond;
                                }
                                else if (_lst.Count == 0)
                                {
                                    _lst = _lstSecond;
                                }
                                else
                                {
                                    _lst.AddRange(_lstSecond);
                                }
                                fromDate = fromDate.AddMonths(-1);
                                _dal = new DAL(_c.GetConnectionString(2, fromDate.ToString("MM_yyyy")));
                                _proc = new ProcRechargeReport(_dal);
                                _lstSecond = (List<ProcRechargeReportResponse>)await _proc.Call(_filter).ConfigureAwait(false);
                                if (_lst == null)
                                {
                                    _lst = _lstSecond;
                                }
                                else if (_lst.Count == 0)
                                {
                                    _lst = _lstSecond;
                                }
                                else
                                {
                                    _lst.AddRange(_lstSecond);
                                }
                            }
                        }

                    }
                    else
                    {
                        _dal = ChangeConString(_filter.FromDate);
                        IProcedureAsync _proc = new ProcRechargeReport(_dal);
                        _lst = (List<ProcRechargeReportResponse>)await _proc.Call(_filter).ConfigureAwait(false);
                    }
                }
            }
            return _lst;
        }
        public async Task<List<ProcRechargeReportResponse>> GetTopRecentTransactions(int OpTypeID, int Top)
        {
            IProcedureAsync _proc = new ProcRecentTopTransaction(_dal);
            return (List<ProcRechargeReportResponse>)await _proc.Call(new CommonReq
            {
                CommonInt = OpTypeID,
                CommonInt2 = Top,
                LoginID = _lr.UserID
            }).ConfigureAwait(false);
        }
        public async Task<List<ProcRechargeReportResponse>> GetTopRecentTransactions(int OpTypeID, int Top, int LoginID, int OutletID)
        {
            IProcedureAsync _proc = new ProcRecentTopTransaction(_dal);
            return (List<ProcRechargeReportResponse>)await _proc.Call(new CommonReq
            {
                CommonInt = OpTypeID,
                CommonInt2 = Top,
                LoginID = LoginID,
                CommonInt3 = OutletID
            }).ConfigureAwait(false);
        }
        public async Task<List<ProcRechargeReportResponse>> GetDisplayLive(RechargeReportFilter filter)
        {
            var _lst = new List<ProcRechargeReportResponse>();
            if (_lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.ShowRechargeReport))
            {
                var _filter = new _RechargeReportFilter
                {
                    LT = _lr.LoginTypeID,
                    LoginID = _lr.UserID,
                    Status = filter.Status
                };
                IProcedureAsync _proc = new ProcDisplayLive(_dal);
                _lst = (List<ProcRechargeReportResponse>)await _proc.Call(_filter).ConfigureAwait(false);
            }
            return _lst;
        }
        public async Task<List<ProcRechargeReportResponse>> GetRechargeFailToSuccess(RechargeReportFilter filter)
        {
            var _lst = new List<ProcRechargeReportResponse>();

            if (_lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.ShowRechargeReport))
            {
                Validate validate = Validate.O;
                _RechargeReportFilter _filter = new _RechargeReportFilter
                {
                    LT = _lr.LoginTypeID,
                    LoginID = _lr.UserID,
                    TopRows = filter.TopRows,
                    Status = filter.Status,
                    APIID = filter.APIID,
                    OID = filter.OID,
                    FromDate = filter.FromDate,
                    ToDate = filter.ToDate,
                    Criteria = filter.Criteria,
                    CriteriaText = filter.CriteriaText,
                    IsExport = filter.IsExport,
                    CircleID = filter.CircleID,
                    SwitchID = filter.SwitchID
                };
                if (_filter.Criteria == Criteria.OutletMobile)
                {
                    if (!validate.IsMobile(_filter.CriteriaText ?? ""))
                    {
                        return _lst;
                    }
                    _filter.OutletNo = _filter.CriteriaText;
                }
                if (_filter.Criteria == Criteria.AccountNo)
                {
                    if (string.IsNullOrEmpty(_filter.CriteriaText ?? ""))
                    {
                        return _lst;
                    }
                    _filter.AccountNo = _filter.CriteriaText;
                }
                if (_filter.Criteria == Criteria.TransactionID)
                {
                    if (string.IsNullOrEmpty(_filter.CriteriaText))
                    {
                        return _lst;
                    }
                    if (_filter.CriteriaText[0] != Criteria.StartCharOFTransaction)
                    {
                        return _lst;
                    }
                    _filter.TransactionID = _filter.CriteriaText;
                }
                if (_filter.Criteria == Criteria.TID)
                {
                    try
                    {
                        _filter.TID = Convert.ToInt32(_filter.CriteriaText);
                    }
                    catch (Exception)
                    {
                        return _lst;
                    }
                }
                if (_filter.Criteria == Criteria.APIRequestID)
                {
                    if (string.IsNullOrEmpty(_filter.CriteriaText))
                    {
                        return _lst;
                    }
                    _filter.APIRequestID = _filter.CriteriaText;
                }
                if (_filter.Criteria == Criteria.VendorID)
                {
                    if (string.IsNullOrEmpty(_filter.CriteriaText))
                    {
                        return _lst;
                    }
                    _filter.VendorID = _filter.CriteriaText;
                }
                _filter.FromDate = string.IsNullOrEmpty(_filter.FromDate) ? DateTime.Now.ToString("dd MMM yyyy") : _filter.FromDate;
                _dal = ChangeConString(_filter.FromDate);
                IProcedureAsync _proc = new ProcRechargeFailToSuccess(_dal);
                _lst = (List<ProcRechargeReportResponse>)await _proc.Call(_filter);
            }
            return _lst;
        }
        public async Task<List<RechargeReportResponse>> GetRechargeReportRole(RechargeReportFilter filter)
        {
            var _lst = new List<RechargeReportResponse>();

            if (_lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.ShowRechargeReport))
            {
                var validate = Validate.O;
                var _filter = new _RechargeReportFilter
                {
                    LT = _lr.LoginTypeID,
                    LoginID = _lr.UserID,
                    TopRows = filter.TopRows,
                    Status = filter.Status,
                    APIID = filter.APIID,
                    OID = filter.OID,
                    FromDate = filter.FromDate,
                    ToDate = filter.ToDate,
                    Criteria = filter.Criteria,
                    CriteriaText = filter.CriteriaText,
                    IsExport = filter.IsExport
                };
                if (filter.Criteria > 0)
                {
                    if ((filter.CriteriaText ?? "") == "")
                    {
                        return _lst;
                    }
                }
                if (_filter.Criteria == Criteria.OutletMobile)
                {
                    if (!validate.IsMobile(_filter.CriteriaText))
                    {
                        return _lst;
                    }
                    _filter.OutletNo = _filter.CriteriaText;
                }
                if (_filter.Criteria == Criteria.AccountNo)
                {
                    _filter.AccountNo = _filter.CriteriaText;
                }
                if (_filter.Criteria == Criteria.TransactionID)
                {
                    if (_filter.CriteriaText[0] != Criteria.StartCharOFTransaction)
                    {
                        return _lst;
                    }
                    _filter.TransactionID = _filter.CriteriaText;
                }
                if (_filter.Criteria == Criteria.TID)
                {
                    try
                    {
                        _filter.TID = Convert.ToInt32(_filter.CriteriaText);
                    }
                    catch (Exception)
                    {
                        return _lst;
                    }
                }
                if (_filter.Criteria == Criteria.APIRequestID)
                {
                    _filter.APIRequestID = _filter.CriteriaText;
                }
                if (_filter.Criteria == Criteria.VendorID)
                {
                    _filter.VendorID = _filter.CriteriaText;
                }
                if (_filter.Criteria == Criteria.CCID)
                {
                    if (!validate.IsNumeric(_filter.CriteriaText))
                    {
                        return _lst;
                    }
                    _filter.CCID = Convert.ToInt32(_filter.CriteriaText);
                }
                if (_filter.Criteria == Criteria.CCMobileNo)
                {
                    _filter.CCMobileNo = _filter.CriteriaText;
                }
                if (_filter.Criteria == Criteria.UserID)
                {
                    var Prefix = Validate.O.Prefix(filter.CriteriaText);
                    if (Validate.O.IsNumeric(Prefix))
                        _filter.UserID = Validate.O.IsNumeric(filter.CriteriaText) ? Convert.ToInt32(filter.CriteriaText) : _filter.UserID;
                    var uid = Validate.O.LoginID(filter.CriteriaText);
                    _filter.UserID = Validate.O.IsNumeric(uid) ? Convert.ToInt32(uid) : _filter.UserID;
                }
                _filter.FromDate = string.IsNullOrEmpty(_filter.FromDate) ? DateTime.Now.ToString("dd MMM yyyy") : _filter.FromDate;
                _dal = ChangeConString(_filter.FromDate);
                IProcedureAsync _proc = new ProcRechargeReportwithRole(_dal);
                _lst = (List<RechargeReportResponse>)await _proc.Call(_filter);
            }
            return _lst;
        }
        public async Task<IEnumerable<ProcApiURlRequestResponse>> GetRechargeApiResponses(int TID, string TransactionID)
        {
            List<ProcApiURlRequestResponse> _lst = new List<ProcApiURlRequestResponse>();
            ProcApiURlRequestResponse _res = new ProcApiURlRequestResponse();
            if (_lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.ShowRechargeReport))
            {
                var req = new CommonReq
                {
                    LoginTypeID = _lr.LoginTypeID,
                    LoginID = _lr.UserID,
                    CommonInt = TID
                };
                string TransDate = "";
                if (Validate.O.IsTransactionIDValid(TransactionID))
                {
                    TransDate = ConnectionStringHelper.O.ConvertTransactionIDTo_dd_MMM_yyyy(TransactionID);
                    _dal = ChangeConString(TransDate);
                }
                IProcedureAsync _proc = new ProcGetApiRequestReponse(_dal);
                _lst = (List<ProcApiURlRequestResponse>)await _proc.Call(req).ConfigureAwait(false);
            }
            return _lst;
        }
        public async Task<IResponseStatus> UpdateTransactionStatus(_CallbackData callbackData)
        {
            IResponseStatus _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if ((_lr.RoleID == Role.Admin || (_lr.IsWLAPIAllowed && ApplicationSetting.IsWLAPIAllowed)) && _lr.LoginTypeID == LoginType.ApplicationUser || (userML.IsCustomerCareAuthorised(ActionCodes.PageSuccess) && callbackData.TransactionStatus == RechargeRespType.SUCCESS) || (userML.IsCustomerCareAuthorised(ActionCodes.PageFailed) && callbackData.TransactionStatus == RechargeRespType.FAILED))
            {
                callbackData.RequestIP = _rinfo.GetRemoteIP();
                callbackData.Browser = _rinfo.GetBrowserFullInfo();
                callbackData.LiveID = callbackData.Msg;
                callbackData.LoginID = _lr.UserID;
                callbackData.LT = _lr.LoginTypeID;
                string TransDate = "";
                if (Validate.O.IsTransactionIDValid(callbackData.TransactionID))
                {
                    TransDate = ConnectionStringHelper.O.ConvertTransactionIDTo_dd_MMM_yyyy(callbackData.TransactionID);
                    _dal = ChangeConString(TransDate);
                }
                IProcedureAsync procAPIResponse = new ProcAPIResponse(_dal);
                callbackData = (_CallbackData)await procAPIResponse.Call(callbackData).ConfigureAwait(false);
                if (callbackData.Statuscode == ErrorCodes.Minus1)
                {
                    _res.Msg = callbackData.Msg;
                }
                else if (callbackData.Statuscode == ErrorCodes.One)
                {
                    if (callbackData.RequestMode == RequestMode.API && !string.IsNullOrEmpty(callbackData.UpdateUrl))
                    {
                        ICallbackML callbackML = new CallbackML(_accessor, _env);
                        var _ = callbackML.UpdateAPIURLHitting(callbackData);
                    }
                    if (callbackData.RequestMode.In(RequestMode.APPS, RequestMode.SMS))
                    {
                        bool IsSuccess = false;
                        IAlertML alertMl = new AlertML(_accessor, _env);
                        if (callbackData.TransactionStatus == RechargeRespType.SUCCESS)
                        {
                            IsSuccess = true;
                        }
                        var alertParam = new AlertReplacementModel
                        {
                            AccountNo = callbackData.AccountNo,
                            UserMobileNo = callbackData.MobileNo,
                            Amount = callbackData.RequestedAmount,
                            Operator = callbackData.Operator,
                            LiveID = callbackData.LiveID,
                            WID = callbackData.WID,
                            UserFCMID = callbackData.FCMID,
                            LoginID = callbackData.LoginID,
                            TransactionID = callbackData.TransactionID,
                            LoginCurrentBalance = callbackData.APIBalance,
                            Company = callbackData.Company,
                            CompanyDomain = callbackData.CompanyDomain,
                            CompanyAddress = callbackData.CompanyAddress,
                            BrandName = callbackData.BrandName,
                            OutletName = callbackData.OutletName,
                            SupportEmail = callbackData.SupportEmail,
                            SupportNumber = callbackData.SupportNumber,
                            AccountEmail = callbackData.AccountEmail,
                            AccountsContactNo = callbackData.AccountContact,
                            UserID = callbackData.UserID,
                            UserName = callbackData.UserName,
                            UserCurrentBalance = callbackData.Balance
                        };
                        if (callbackData.RequestMode == RequestMode.APPS)
                        {
                            alertMl.RecharegeSuccessNotification(alertParam, IsSuccess);
                        }
                        else
                        {
                            alertMl.RecharegeSuccessSMS(alertParam, IsSuccess);
                        }
                    }
                    _res.Msg = "Transaction " + (callbackData.TransactionStatus == RechargeRespType.SUCCESS ? RechargeRespType._SUCCESS : RechargeRespType._FAILED) + " Updated for TID:" + callbackData.TID + " and TransactionID:" + (callbackData.TransactionID ?? "");
                }
                _res.Statuscode = callbackData.Statuscode;
            }
            return _res;
        }

        public async Task<IResponseStatus> UpdateRefund(_CallbackData callbackData)
        {
            IResponseStatus _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            callbackData.RequestIP = _rinfo.GetRemoteIP();
            callbackData.Browser = _rinfo.GetBrowserFullInfo();
            callbackData.LiveID = callbackData.Msg;
            IProcedureAsync procAPIResponse = new ProcAPIResponse(_dal);
            callbackData = (_CallbackData)await procAPIResponse.Call(callbackData);
            if (callbackData.Statuscode == ErrorCodes.Minus1)
            {
                _res.Msg = callbackData.Msg;
            }
            else if (callbackData.Statuscode == ErrorCodes.One)
            {
                if (callbackData.RequestMode == RequestMode.API && !string.IsNullOrEmpty(callbackData.UpdateUrl))
                {
                    ICallbackML callbackML = new CallbackML(_accessor, _env);
                    var _ = callbackML.UpdateAPIURLHitting(callbackData);
                }
                if (callbackData.RequestMode.In(RequestMode.APPS, RequestMode.SMS))
                {
                    var sendSMSML = new SendSMSML(_accessor, _env, false);
                    var formatid = sendSMSML.GetRechargeFormatID(callbackData.TransactionStatus);
                    var commonReq = new CommonReq
                    {
                        LoginTypeID = LoginType.ApplicationUser,
                        LoginID = callbackData.UserID,
                        CommonInt = sendSMSML.GetRechargeFormatID(callbackData.TransactionStatus),
                        CommonInt2 = callbackData.TID
                    };
                    var sendDetail = sendSMSML.GetNotificationSendDetail(commonReq);
                    sendDetail.messsageTemplate = sendSMSML.GetRechargeMessage(callbackData.TransactionStatus, sendDetail.replacementModelForSMS);
                    sendDetail.notification = sendSMSML.GetNotificationForRecharge(callbackData.TransactionStatus, sendDetail.messsageTemplate.Msg);
                    sendSMSML.SendNotificationSmsAndEMAILToUser(sendDetail);
                }
                _res.Msg = "Transaction " + (callbackData.TransactionStatus == RechargeRespType.SUCCESS ? RechargeRespType._SUCCESS : RechargeRespType._FAILED) + " Updated for TransactionID:" + (callbackData.TransactionID ?? "");
                _res.Statuscode = callbackData.Statuscode;

            }
            return _res;
        }
        public async Task<RefundRequestResponse> MarkDispute(RefundRequest _req)
        {
            var _res = new RefundRequestResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            bool IsAuthorisedUser = false;
            if (_lr != null)
            {
                IsAuthorisedUser = _lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.MarkDispute);
                _req.Token = null;
            }
            else
            {
                IsAuthorisedUser = (_req.UserID > 1 && _req.Token != null);
            }
            if (IsAuthorisedUser)
            {
                if (_req.UserID == 0)
                {
                    _req.UserID = _lr.UserID;
                }
                if (Validate.O.IsTransactionIDValid(_req.RPID))
                {
                    var refundRequestReq = new RefundRequestReq
                    {
                        refundRequest = _req,
                        LoginTypeID = _lr == null ? LoginType.ApplicationUser : _lr.LoginTypeID,
                        LoginID = _lr == null ? _req.UserID : _lr.UserID,
                        CommonStr = _rinfo.GetRemoteIP(),
                        CommonStr2 = _rinfo.GetBrowser()
                    };
                    string TransDate = ConnectionStringHelper.O.ConvertTransactionIDTo_dd_MMM_yyyy(_req.RPID);
                    _dal = ChangeConString(TransDate);
                    IProcedureAsync _proc = new ProcMakeRefundRequest(_dal);
                    _res = (RefundRequestResponse)await _proc.Call(refundRequestReq).ConfigureAwait(false);
                    if (_res.Statuscode == ErrorCodes.One && _res.ServiceID == ServiceType.MoneyTransfer && _res.APICode.Equals(APICode.EKO))
                    {
                        //EKO Status check process
                        var ekoObj = new EKOML(_accessor, _env, _res.APIID);
                        if ((_req.OTP ?? string.Empty).Length > 0)
                        {
                            var dMRTransactionResponse = await ekoObj.Refund(_res.TID, _res.VendorID, _req.RequestMode, _res.UserID, _res.Optional2, _req.OTP).ConfigureAwait(false);
                            _res.Statuscode = dMRTransactionResponse.Statuscode;
                            _res.Msg = dMRTransactionResponse.Msg;
                            _res.ErrorCode = dMRTransactionResponse.ErrorCode;
                            if (dMRTransactionResponse.Statuscode == ErrorCodes.One)
                            {
                                var callbackData = new _CallbackData
                                {
                                    TID = _res.TID,
                                    TransactionID = _res.TransactionID,
                                    RequestPage = "RefundOTP",
                                    RequestIP = _rinfo.GetRemoteIP(),
                                    Browser = _rinfo.GetBrowserFullInfo(),
                                    TransactionStatus = RechargeRespType.REFUND,
                                    VendorID = _res.VendorID,
                                    LT = refundRequestReq.LoginTypeID,
                                    LoginID = _res.UserID
                                };
                                await UpdateRefund(callbackData).ConfigureAwait(false);
                            }
                        }
                        else if (_req.IsResend)
                        {
                            var dMRTransactionResponse = await ekoObj.ResendRefundOTP(_res.VendorID, _req.RequestMode, _res.UserID, _res.Optional2, _res.TID).ConfigureAwait(false);
                            _res.Statuscode = dMRTransactionResponse.Statuscode;
                            _res.Msg = dMRTransactionResponse.Msg;
                            _res.ErrorCode = dMRTransactionResponse.ErrorCode;
                            if (_res.Statuscode == ErrorCodes.Minus1)
                            {
                                _res.Msg = "OTP could not be resent!";
                            }
                        }
                        else
                        {
                            var dMRTransactionResponse = await ekoObj.GetTransactionStatus(_res.TID, _req.RequestMode, _res.UserID, _res.Optional2, _res.VendorID).ConfigureAwait(false);
                            if (dMRTransactionResponse.IsRefundAvailable)
                            {
                                _res.Statuscode = ErrorCodes.One;
                                _res.IsOTPRequired = true;
                                _res.Msg = dMRTransactionResponse.Msg;
                                _res.ErrorCode = dMRTransactionResponse.ErrorCode;
                            }
                            else if (dMRTransactionResponse.Statuscode.In(RechargeRespType.REFUND, RechargeRespType.FAILED))
                            {
                                _res.Statuscode = ErrorCodes.One;
                                _res.Msg = nameof(DMTErrorCodes.Transaction_Status_Updated_Refund).Replace("_", " ");
                                _res.ErrorCode = DMTErrorCodes.Transaction_Status_Updated_Refund;
                                var callbackData = new _CallbackData
                                {
                                    TID = _res.TID,
                                    TransactionID = _res.TransactionID,
                                    RequestPage = "RefundOTP_WO",
                                    RequestIP = _rinfo.GetRemoteIP(),
                                    Browser = _rinfo.GetBrowserFullInfo(),
                                    TransactionStatus = RechargeRespType.REFUND,
                                    VendorID = _res.VendorID,
                                    LT = refundRequestReq.LoginTypeID,
                                    LoginID = _res.UserID
                                };
                                await UpdateRefund(callbackData).ConfigureAwait(false);
                            }
                            else
                            {
                                _res.Statuscode = ErrorCodes.Minus1;
                                _res.Msg = "Currently refund not available";
                            }
                        }
                    }
                    else if (_res.Statuscode == ErrorCodes.One && _res.ServiceID == ServiceType.MoneyTransfer && _res.APICode.Equals(APICode.EKO2))
                    {
                        //EKO Status check process
                        var ekoObj = new EKO2ML(_accessor, _env, _dal, _res.APIID);
                        if ((_req.OTP ?? string.Empty).Length > 0)
                        {
                            var dMRTransactionResponse = await ekoObj.Refund(_res.TID, _res.VendorID, _req.RequestMode, _res.UserID, _res.Optional2, _req.OTP, _res.APIOutletID).ConfigureAwait(false);
                            _res.Statuscode = dMRTransactionResponse.Statuscode;
                            _res.Msg = dMRTransactionResponse.Msg;
                            _res.ErrorCode = dMRTransactionResponse.ErrorCode;
                            if (dMRTransactionResponse.Statuscode == ErrorCodes.One)
                            {
                                var callbackData = new _CallbackData
                                {
                                    TID = _res.TID,
                                    TransactionID = _res.TransactionID,
                                    RequestPage = "RefundOTP",
                                    RequestIP = _rinfo.GetRemoteIP(),
                                    Browser = _rinfo.GetBrowserFullInfo(),
                                    TransactionStatus = RechargeRespType.REFUND,
                                    VendorID = _res.VendorID,
                                    LT = refundRequestReq.LoginTypeID,
                                    LoginID = _res.UserID
                                };
                                await UpdateRefund(callbackData).ConfigureAwait(false);
                            }
                        }
                        else if (_req.IsResend)
                        {
                            var dMRTransactionResponse = await ekoObj.ResendRefundOTP(_res.VendorID, _req.RequestMode, _res.UserID, _res.Optional2, _res.TID, _res.APIOutletID).ConfigureAwait(false);
                            _res.Statuscode = dMRTransactionResponse.Statuscode;
                            _res.Msg = dMRTransactionResponse.Msg;
                            _res.ErrorCode = dMRTransactionResponse.ErrorCode;
                            if (_res.Statuscode == ErrorCodes.Minus1)
                            {
                                _res.Msg = "OTP could not be resent!";
                            }
                        }
                        else
                        {
                            var dMRTransactionResponse = await ekoObj.GetTransactionStatus(_res.TID, _req.RequestMode, _res.UserID, _res.Optional2, _res.VendorID, _res.APIOutletID, "").ConfigureAwait(false);
                            if (dMRTransactionResponse.IsRefundAvailable)
                            {
                                _res.Statuscode = ErrorCodes.One;
                                _res.IsOTPRequired = true;
                                _res.Msg = dMRTransactionResponse.Msg;
                                _res.ErrorCode = dMRTransactionResponse.ErrorCode;
                                var respOTP = ekoObj.ResendRefundOTP(_res.VendorID, _req.RequestMode, _res.UserID, _res.Optional2, _res.TID, _res.APIOutletID).Result;
                                if (respOTP.Statuscode == ErrorCodes.Minus1)
                                {
                                    _res.IsOTPRequired = false;
                                    _res.Msg = "OTP could not be resent!";
                                    _res.Statuscode = ErrorCodes.Minus1;
                                }
                            }
                            else if (dMRTransactionResponse.Statuscode.In(RechargeRespType.REFUND, RechargeRespType.FAILED))
                            {
                                _res.Statuscode = ErrorCodes.One;
                                _res.Msg = nameof(DMTErrorCodes.Transaction_Status_Updated_Refund).Replace("_", " ");
                                _res.ErrorCode = DMTErrorCodes.Transaction_Status_Updated_Refund;
                                var callbackData = new _CallbackData
                                {
                                    TID = _res.TID,
                                    TransactionID = _res.TransactionID,
                                    RequestPage = "RefundOTP_WO",
                                    RequestIP = _rinfo.GetRemoteIP(),
                                    Browser = _rinfo.GetBrowserFullInfo(),
                                    TransactionStatus = RechargeRespType.REFUND,
                                    VendorID = _res.VendorID,
                                    LT = refundRequestReq.LoginTypeID,
                                    LoginID = _res.UserID
                                };
                                await UpdateRefund(callbackData).ConfigureAwait(false);
                            }
                            else
                            {
                                _res.Statuscode = ErrorCodes.Minus1;
                                _res.Msg = "Currently refund not available";
                            }
                        }
                    }
                    #region Bill avenue
                    else if (_res.Statuscode == ErrorCodes.One && _res.ServiceID == ServiceType.MoneyTransfer && _res.APICode.Equals(APICode.BILLAVENUE))
                    {
                        //Bill Avenue Status check process
                        var bAObj = new BillAvenueMT_ML(_accessor, _env, _res.APICode, _res.APIID, _dal);

                        var mTAPIReq = new MTAPIRequest
                        {
                            TID = _res.TID,
                            VID = _res.VendorID,
                            TransactionID = _res.TransactionID,
                            OTP = _req.OTP,
                            UserID = _res.UserID,
                            APIID = _res.APIID,
                            RequestMode = _req.RequestMode,
                            GroupID = _res.GroupIID,
                            IsRefundReq = true
                        };

                        var BVStsCheck = bAObj.GetTransactionStatus(mTAPIReq);

                        if (!string.IsNullOrEmpty(BVStsCheck.RDMTTxnID) && BVStsCheck.RStsType.Equals("T"))
                        {
                            mTAPIReq.TransactionID = BVStsCheck.RDMTTxnID;
                            if ((_req.OTP ?? string.Empty).Length > 0)
                            {
                                var dMRTransactionResponse = bAObj.ResendRefundOTP(mTAPIReq);

                                if (dMRTransactionResponse.Statuscode == ErrorCodes.One)
                                {
                                    _res.Msg = nameof(DMTErrorCodes.Transaction_Status_Updated_Refund).Replace("_", " ");
                                    _res.ErrorCode = DMTErrorCodes.Transaction_Status_Updated_Refund;
                                    var callbackData = new _CallbackData
                                    {
                                        TID = _res.TID,
                                        TransactionID = _res.TransactionID,
                                        RequestPage = "RefundOTP_WO",
                                        RequestIP = _rinfo.GetRemoteIP(),
                                        Browser = _rinfo.GetBrowserFullInfo(),
                                        TransactionStatus = RechargeRespType.REFUND,
                                        VendorID = _res.VendorID,
                                        LT = refundRequestReq.LoginTypeID,
                                        LoginID = _res.UserID
                                    };
                                    await UpdateRefund(callbackData).ConfigureAwait(false);
                                }
                                else
                                {
                                    _res.Statuscode = ErrorCodes.Minus1;
                                    _res.Msg = "Currently refund not available";
                                }
                            }
                            else
                            {
                                var dMRTransactionResponse = bAObj.Refund(mTAPIReq);
                                _res.Statuscode = dMRTransactionResponse.Statuscode;
                                _res.Msg = dMRTransactionResponse.Msg;
                                _res.ErrorCode = dMRTransactionResponse.ErrorCode;
                                if (dMRTransactionResponse.IsRefundAvailable)
                                {
                                    _res.Statuscode = ErrorCodes.One;
                                    _res.IsOTPRequired = true;
                                    _res.IsResendBtnHide = true;
                                    _res.Msg = dMRTransactionResponse.Msg;
                                    _res.ErrorCode = dMRTransactionResponse.ErrorCode;
                                }

                                else if (dMRTransactionResponse.Statuscode.In(RechargeRespType.REFUND, RechargeRespType.FAILED))
                                {
                                    _res.Statuscode = ErrorCodes.One;
                                    _res.Msg = nameof(DMTErrorCodes.Transaction_Status_Updated_Refund).Replace("_", " ");
                                    _res.ErrorCode = DMTErrorCodes.Transaction_Status_Updated_Refund;
                                    var callbackData = new _CallbackData
                                    {
                                        TID = _res.TID,
                                        TransactionID = _res.TransactionID,
                                        RequestPage = "RefundOTP_WO",
                                        RequestIP = _rinfo.GetRemoteIP(),
                                        Browser = _rinfo.GetBrowserFullInfo(),
                                        TransactionStatus = RechargeRespType.REFUND,
                                        VendorID = _res.VendorID,
                                        LT = refundRequestReq.LoginTypeID,
                                        LoginID = _res.UserID
                                    };
                                    await UpdateRefund(callbackData).ConfigureAwait(false);
                                }
                            }
                        }
                        else
                        {
                            _res.Statuscode = ErrorCodes.Minus1;
                            _res.Msg = BVStsCheck.Msg;
                        }
                    }
                    #endregion
                    else if (_res.Statuscode == ErrorCodes.One && _res.ServiceID == ServiceType.MoneyTransfer && _res.APICode.Equals(APICode.RBLMT))
                    {
                        //RBLBank Status check process
                        var rblObj = new RBLML(_accessor, _env, _dal);
                        if ((_req.OTP ?? string.Empty).Length > 0)
                        {
                            var dMRTransactionResponse = rblObj.Refund(_res.TID, _res.VendorID, _req.RequestMode, _res.UserID, _res.Optional2, _req.OTP, _res.APIID);
                            _res.Statuscode = dMRTransactionResponse.Statuscode;
                            _res.Msg = dMRTransactionResponse.Msg;
                            _res.ErrorCode = dMRTransactionResponse.ErrorCode;
                            if (dMRTransactionResponse.Statuscode == ErrorCodes.One)
                            {
                                var callbackData = new _CallbackData
                                {
                                    TID = _res.TID,
                                    TransactionID = _res.TransactionID,
                                    RequestPage = "RefundOTP",
                                    RequestIP = _rinfo.GetRemoteIP(),
                                    Browser = _rinfo.GetBrowserFullInfo(),
                                    TransactionStatus = RechargeRespType.REFUND,
                                    VendorID = _res.VendorID,
                                    LT = refundRequestReq.LoginTypeID,
                                    LoginID = _res.UserID
                                };
                                await UpdateRefund(callbackData).ConfigureAwait(false);
                            }
                        }
                        else if (_req.IsResend)
                        {
                            var dMRTransactionResponse = rblObj.RefundOTP(_res.VendorID, _req.RequestMode, _res.UserID, _res.Optional2, _res.TID, _res.APIID);
                            _res.Statuscode = dMRTransactionResponse.Statuscode;
                            _res.Msg = dMRTransactionResponse.Msg;
                            _res.ErrorCode = dMRTransactionResponse.ErrorCode;
                            if (_res.Statuscode == ErrorCodes.Minus1)
                            {
                                _res.Msg = "OTP could not be resent!";
                            }
                        }
                        else
                        {
                            var dMRTransactionResponse = rblObj.GetTransactionStatus(_res.TID, _req.RequestMode, _res.UserID, _res.APIID);
                            if (dMRTransactionResponse.IsRefundAvailable)
                            {
                                _res.Statuscode = ErrorCodes.One;
                                _res.IsOTPRequired = true;
                                _res.Msg = dMRTransactionResponse.Msg;
                                _res.ErrorCode = dMRTransactionResponse.ErrorCode;
                            }
                            else if (dMRTransactionResponse.Statuscode.In(RechargeRespType.REFUND, RechargeRespType.FAILED))
                            {
                                _res.Statuscode = ErrorCodes.One;
                                _res.Msg = nameof(DMTErrorCodes.Transaction_Status_Updated_Refund).Replace("_", " ");
                                _res.ErrorCode = DMTErrorCodes.Transaction_Status_Updated_Refund;
                                var callbackData = new _CallbackData
                                {
                                    TID = _res.TID,
                                    TransactionID = _res.TransactionID,
                                    RequestPage = "RefundOTP_WO",
                                    RequestIP = _rinfo.GetRemoteIP(),
                                    Browser = _rinfo.GetBrowserFullInfo(),
                                    TransactionStatus = RechargeRespType.REFUND,
                                    VendorID = _res.VendorID,
                                    LT = refundRequestReq.LoginTypeID,
                                    LoginID = _res.UserID
                                };
                                await UpdateRefund(callbackData).ConfigureAwait(false);
                            }
                            else
                            {
                                _res.Statuscode = ErrorCodes.Minus1;
                                _res.Msg = "Currently refund not available";
                            }
                        }
                    }
                    else if (_res.Statuscode == ErrorCodes.One && _res.ServiceID == ServiceType.MoneyTransfer && _res.APICode.Equals(APICode.SPRINT))
                    {
                        //Paysprint Status check process
                        var spdmtOBJ = new SprintDMTML(_accessor, _env, APICode.SPRINT, _res.APIID, _dal);
                        if ((_req.OTP ?? string.Empty).Length > 0)
                        {
                            var dMRTransactionResponse = spdmtOBJ.Refund(_res.TID, _res.VendorID, _req.RequestMode, _res.UserID, _res.Optional2, _req.OTP, _res.APIID);
                            _res.Statuscode = dMRTransactionResponse.Statuscode;
                            _res.Msg = dMRTransactionResponse.Msg;
                            _res.ErrorCode = dMRTransactionResponse.ErrorCode;
                            if (dMRTransactionResponse.Statuscode == ErrorCodes.One)
                            {
                                var callbackData = new _CallbackData
                                {
                                    TID = _res.TID,
                                    TransactionID = _res.TransactionID,
                                    RequestPage = "RefundOTP",
                                    RequestIP = _rinfo.GetRemoteIP(),
                                    Browser = _rinfo.GetBrowserFullInfo(),
                                    TransactionStatus = RechargeRespType.REFUND,
                                    VendorID = _res.VendorID,
                                    LT = refundRequestReq.LoginTypeID,
                                    LoginID = _res.UserID
                                };
                                await UpdateRefund(callbackData).ConfigureAwait(false);
                            }
                        }
                        else if (_req.IsResend)
                        {
                            var dMRTransactionResponse = spdmtOBJ.RefundOTP(_res.VendorID, _req.RequestMode, _res.UserID, _res.Optional2, _res.TID, _res.APIID);
                            //string VendorID, int RequestMode, int UserID, string SenderNo, int TID, int APIID
                            _res.Statuscode = dMRTransactionResponse.Statuscode;
                            _res.Msg = dMRTransactionResponse.Msg;
                            _res.ErrorCode = dMRTransactionResponse.ErrorCode;
                            if (_res.Statuscode == ErrorCodes.Minus1)
                            {
                                _res.Msg = "OTP could not be resent!";
                            }
                        }
                        else
                        {
                            var dMRTransactionResponse = spdmtOBJ.RefundOTP(_res.VendorID, _req.RequestMode, _res.UserID, _res.Optional2, _res.TID, _res.APIID);
                            if (dMRTransactionResponse.Statuscode == ErrorCodes.One)
                            {
                                _res.Statuscode = ErrorCodes.One;
                                _res.IsOTPRequired = true;
                                _res.IsResendBtnHide = false;
                                _res.Msg = dMRTransactionResponse.Msg;
                                _res.ErrorCode = dMRTransactionResponse.ErrorCode;
                            }
                            else
                            {
                                _res.Statuscode = ErrorCodes.Minus1;
                                _res.Msg = "Currently refund not available";
                            }
                        }
                    }
                    else if ((_res.DisputeURL ?? "").Length > 20)
                    {
                        try
                        {
                            var __ = AppWebRequest.O.CallUsingHttpWebRequest_GETAsync(_res.DisputeURL);
                        }
                        catch (Exception ex)
                        {
                            var errorLog = new ErrorLog
                            {
                                ClassName = GetType().Name,
                                FuncName = "MarkDispute",
                                Error = "Exception In DisputeURL Call:" + ex.Message,
                                LoginTypeID = refundRequestReq.LoginTypeID,
                                UserId = refundRequestReq.LoginID
                            };
                            var _ = new ProcPageErrorLog(_dal).Call(errorLog);
                        }
                    }
                    #region Findodin
                    else if (_res.Statuscode == ErrorCodes.One && _res.ServiceID == ServiceType.MoneyTransfer && _res.APICode.Equals(APICode.FINODIN))
                    {
                        //RBLBank Status check process
                        var rblObj = new RBLML(_accessor, _env, _dal);
                        if ((_req.OTP ?? string.Empty).Length > 0)
                        {
                            var dMRTransactionResponse = rblObj.Refund(_res.TID, _res.VendorID, _req.RequestMode, _res.UserID, _res.Optional2, _req.OTP, _res.APIID);
                            _res.Statuscode = dMRTransactionResponse.Statuscode;
                            _res.Msg = dMRTransactionResponse.Msg;
                            _res.ErrorCode = dMRTransactionResponse.ErrorCode;
                            if (dMRTransactionResponse.Statuscode == ErrorCodes.One)
                            {
                                var callbackData = new _CallbackData
                                {
                                    TID = _res.TID,
                                    TransactionID = _res.TransactionID,
                                    RequestPage = "RefundOTP",
                                    RequestIP = _rinfo.GetRemoteIP(),
                                    Browser = _rinfo.GetBrowserFullInfo(),
                                    TransactionStatus = RechargeRespType.REFUND,
                                    VendorID = _res.VendorID,
                                    LT = refundRequestReq.LoginTypeID,
                                    LoginID = _res.UserID
                                };
                                await UpdateRefund(callbackData).ConfigureAwait(false);
                            }
                        }
                        else if (_req.IsResend)
                        {
                            var dMRTransactionResponse = rblObj.RefundOTP(_res.VendorID, _req.RequestMode, _res.UserID, _res.Optional2, _res.TID, _res.APIID);
                            _res.Statuscode = dMRTransactionResponse.Statuscode;
                            _res.Msg = dMRTransactionResponse.Msg;
                            _res.ErrorCode = dMRTransactionResponse.ErrorCode;
                            if (_res.Statuscode == ErrorCodes.Minus1)
                            {
                                _res.Msg = "OTP could not be resent!";
                            }
                        }
                        else
                        {
                            var dMRTransactionResponse = rblObj.GetTransactionStatus(_res.TID, _req.RequestMode, _res.UserID, _res.APIID);
                            if (dMRTransactionResponse.IsRefundAvailable)
                            {
                                _res.Statuscode = ErrorCodes.One;
                                _res.IsOTPRequired = true;
                                _res.Msg = dMRTransactionResponse.Msg;
                                _res.ErrorCode = dMRTransactionResponse.ErrorCode;
                            }
                            else if (dMRTransactionResponse.Statuscode.In(RechargeRespType.REFUND, RechargeRespType.FAILED))
                            {
                                _res.Statuscode = ErrorCodes.One;
                                _res.Msg = nameof(DMTErrorCodes.Transaction_Status_Updated_Refund).Replace("_", " ");
                                _res.ErrorCode = DMTErrorCodes.Transaction_Status_Updated_Refund;
                                var callbackData = new _CallbackData
                                {
                                    TID = _res.TID,
                                    TransactionID = _res.TransactionID,
                                    RequestPage = "RefundOTP_WO",
                                    RequestIP = _rinfo.GetRemoteIP(),
                                    Browser = _rinfo.GetBrowserFullInfo(),
                                    TransactionStatus = RechargeRespType.REFUND,
                                    VendorID = _res.VendorID,
                                    LT = refundRequestReq.LoginTypeID,
                                    LoginID = _res.UserID
                                };
                                await UpdateRefund(callbackData).ConfigureAwait(false);
                            }
                            else
                            {
                                _res.Statuscode = ErrorCodes.Minus1;
                                _res.Msg = "Currently refund not available";
                            }
                        }
                    }
                    #endregion


                    if (_res.IsBBPS && _res.APICode.In(APICode.BILLAVENUE, APICode.AXISBANK, APICode.PAYUBBPS) && _res.Statuscode == ErrorCodes.One)
                    {
                        var _Void = RaiseBBPSComplainWithoutSession(new GenerateBBPSComplainProcReq
                        {
                            ComplainType = BBPSComplainType.Transaction,
                            Description = "Generated by API",
                            Reason = "Raised Dispute Transaction",
                            MobileNo = _res.MobileNo,
                            TransactionDoneAt = 1,
                            UserID = _res.UserID,
                            StoreOutletID = _res.OutletID,
                            ParticipationType = string.Empty,
                            TransactionID = _res.TransactionID
                        });
                    }
                }
                else
                {
                    _res.Msg = "Invalid TransactionID!";
                }
            }
            return _res;
        }
        public async Task<RefundRequestResponse> MarkDisputeApp(RefundRequestReq refundRequestReq)
        {
            var _res = new RefundRequestResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (refundRequestReq.refundRequest.TID < 1)
            {
                _res.Msg = ErrorCodes.InvalidParam;
                return _res;
            }
            if (!Validate.O.IsTransactionIDValid(refundRequestReq.refundRequest.RPID ?? ""))
            {
                _res.Msg = ErrorCodes.InvalidParam;
                return _res;
            }
            refundRequestReq.CommonStr = _rinfo.GetRemoteIP();
            refundRequestReq.CommonStr2 = _rinfo.GetBrowser();
            if (refundRequestReq.refundRequest.UserID > 1)
            {

                string TransDate = ConnectionStringHelper.O.ConvertTransactionIDTo_dd_MMM_yyyy(refundRequestReq.refundRequest.RPID);
                _dal = ChangeConString(TransDate);
                IProcedureAsync _proc = new ProcMakeRefundRequest(_dal);
                _res = (RefundRequestResponse)await _proc.Call(refundRequestReq).ConfigureAwait(false);
                if (_res.Statuscode == ErrorCodes.One && _res.ServiceID == ServiceType.MoneyTransfer && _res.APICode == APICode.EKO)
                {
                    //EKO Status check process
                    var ekoObj = new EKOML(_accessor, _env, _res.APIID);
                    if ((refundRequestReq.refundRequest.OTP ?? string.Empty).Length > 0)
                    {
                        var dMRTransactionResponse = await ekoObj.Refund(_res.TID, _res.VendorID, refundRequestReq.refundRequest.RequestMode, _res.UserID, _res.Optional2, refundRequestReq.refundRequest.OTP).ConfigureAwait(false);
                        _res.Statuscode = dMRTransactionResponse.Statuscode;
                        _res.Msg = dMRTransactionResponse.Msg;
                        _res.ErrorCode = dMRTransactionResponse.ErrorCode;
                        if (dMRTransactionResponse.Statuscode == ErrorCodes.One)
                        {
                            var callbackData = new _CallbackData
                            {
                                TID = _res.TID,
                                TransactionID = _res.TransactionID,
                                RequestPage = "RefundOTP",
                                RequestIP = _rinfo.GetRemoteIP(),
                                Browser = _rinfo.GetBrowserFullInfo(),
                                TransactionStatus = RechargeRespType.REFUND,
                                VendorID = _res.VendorID,
                                LT = refundRequestReq.LoginTypeID,
                                LoginID = _res.UserID
                            };
                            await UpdateRefund(callbackData).ConfigureAwait(false);
                        }
                    }
                    else if (refundRequestReq.refundRequest.IsResend)
                    {
                        var dMRTransactionResponse = await ekoObj.ResendRefundOTP(_res.VendorID, refundRequestReq.refundRequest.RequestMode, _res.UserID, _res.Optional2, _res.TID).ConfigureAwait(false);
                        _res.Statuscode = dMRTransactionResponse.Statuscode;
                        _res.Msg = dMRTransactionResponse.Msg;
                        _res.ErrorCode = dMRTransactionResponse.ErrorCode;
                        if (_res.Statuscode == ErrorCodes.Minus1)
                        {
                            _res.Msg = "OTP could not be resent!";
                        }
                    }
                    else
                    {
                        var dMRTransactionResponse = await ekoObj.GetTransactionStatus(_res.TID, refundRequestReq.refundRequest.RequestMode, _res.UserID, _res.Optional2, _res.VendorID).ConfigureAwait(false);
                        if (dMRTransactionResponse.IsRefundAvailable)
                        {
                            _res.Statuscode = ErrorCodes.One;
                            _res.IsOTPRequired = true;
                            _res.Msg = dMRTransactionResponse.Msg;
                            _res.ErrorCode = dMRTransactionResponse.ErrorCode;
                        }
                        else if (dMRTransactionResponse.Statuscode.In(RechargeRespType.REFUND, RechargeRespType.FAILED))
                        {
                            _res.Statuscode = ErrorCodes.One;
                            _res.Msg = nameof(DMTErrorCodes.Transaction_Status_Updated_Refund).Replace("_", " ");
                            _res.ErrorCode = DMTErrorCodes.Transaction_Status_Updated_Refund;
                            var callbackData = new _CallbackData
                            {
                                TID = _res.TID,
                                TransactionID = _res.TransactionID,
                                RequestPage = "RefundOTP_WO",
                                RequestIP = _rinfo.GetRemoteIP(),
                                Browser = _rinfo.GetBrowserFullInfo(),
                                TransactionStatus = RechargeRespType.REFUND,
                                VendorID = _res.VendorID,
                                LT = refundRequestReq.LoginTypeID,
                                LoginID = _res.UserID
                            };
                            await UpdateRefund(callbackData);
                        }
                        else
                        {
                            _res.Statuscode = ErrorCodes.Minus1;
                            _res.Msg = "Currently refund not available";
                        }
                    }
                }
                else if (_res.Statuscode == ErrorCodes.One && _res.ServiceID == ServiceType.MoneyTransfer && _res.APICode.Equals(APICode.RBLMT))
                {
                    //RBLBank Status check process
                    var rblObj = new RBLML(_accessor, _env, _dal);
                    if ((refundRequestReq.refundRequest.OTP ?? string.Empty).Length > 0)
                    {
                        var dMRTransactionResponse = rblObj.Refund(_res.TID, _res.VendorID, refundRequestReq.refundRequest.RequestMode, _res.UserID, _res.Optional2, refundRequestReq.refundRequest.OTP, _res.APIID);
                        _res.Statuscode = dMRTransactionResponse.Statuscode;
                        _res.Msg = dMRTransactionResponse.Msg;
                        _res.ErrorCode = dMRTransactionResponse.ErrorCode;
                        if (dMRTransactionResponse.Statuscode == ErrorCodes.One)
                        {
                            var callbackData = new _CallbackData
                            {
                                TID = _res.TID,
                                TransactionID = _res.TransactionID,
                                RequestPage = "RefundOTP",
                                RequestIP = _rinfo.GetRemoteIP(),
                                Browser = _rinfo.GetBrowserFullInfo(),
                                TransactionStatus = RechargeRespType.REFUND,
                                VendorID = _res.VendorID,
                                LT = refundRequestReq.LoginTypeID,
                                LoginID = _res.UserID
                            };
                            await UpdateRefund(callbackData).ConfigureAwait(false);
                        }
                    }
                    else if (refundRequestReq.refundRequest.IsResend)
                    {
                        var dMRTransactionResponse = rblObj.RefundOTP(_res.VendorID, refundRequestReq.refundRequest.RequestMode, _res.UserID, _res.Optional2, _res.TID, _res.APIID);
                        _res.Statuscode = dMRTransactionResponse.Statuscode;
                        _res.Msg = dMRTransactionResponse.Msg;
                        _res.ErrorCode = dMRTransactionResponse.ErrorCode;
                        if (_res.Statuscode == ErrorCodes.Minus1)
                        {
                            _res.Msg = "OTP could not be resent!";
                        }
                    }
                    else
                    {
                        var dMRTransactionResponse = rblObj.GetTransactionStatus(_res.TID, refundRequestReq.refundRequest.RequestMode, _res.UserID, _res.APIID);
                        if (dMRTransactionResponse.IsRefundAvailable)
                        {
                            _res.Statuscode = ErrorCodes.One;
                            _res.IsOTPRequired = true;
                            _res.Msg = dMRTransactionResponse.Msg;
                            _res.ErrorCode = dMRTransactionResponse.ErrorCode;
                        }
                        else if (dMRTransactionResponse.Statuscode.In(RechargeRespType.REFUND, RechargeRespType.FAILED))
                        {
                            _res.Statuscode = ErrorCodes.One;
                            _res.Msg = nameof(DMTErrorCodes.Transaction_Status_Updated_Refund).Replace("_", " ");
                            _res.ErrorCode = DMTErrorCodes.Transaction_Status_Updated_Refund;
                            var callbackData = new _CallbackData
                            {
                                TID = _res.TID,
                                TransactionID = _res.TransactionID,
                                RequestPage = "RefundOTP_WO",
                                RequestIP = _rinfo.GetRemoteIP(),
                                Browser = _rinfo.GetBrowserFullInfo(),
                                TransactionStatus = RechargeRespType.REFUND,
                                VendorID = _res.VendorID,
                                LT = refundRequestReq.LoginTypeID,
                                LoginID = _res.UserID
                            };
                            await UpdateRefund(callbackData).ConfigureAwait(false);
                        }
                        else
                        {
                            _res.Statuscode = ErrorCodes.Minus1;
                            _res.Msg = "Currently refund not available";
                        }
                    }
                }
                else if ((_res.DisputeURL ?? "").Length > 20)
                {
                    try
                    {
                        var __ = AppWebRequest.O.CallUsingHttpWebRequest_GETAsync(_res.DisputeURL);
                    }
                    catch (Exception ex)
                    {
                        var errorLog = new ErrorLog
                        {
                            ClassName = GetType().Name,
                            FuncName = "MarkDisputeApp",
                            Error = "Exception In DisputeURL Call:" + ex.Message,
                            LoginTypeID = refundRequestReq.LoginTypeID,
                            UserId = refundRequestReq.LoginID
                        };
                        var _ = new ProcPageErrorLog(_dal).Call(errorLog);
                    }
                }
                if (_res.IsBBPS && _res.APICode.Equals(APICode.BILLAVENUE) && _res.Statuscode == ErrorCodes.One)
                {
                    var _Void = RaiseBBPSComplainWithoutSession(new GenerateBBPSComplainProcReq
                    {
                        ComplainType = BBPSComplainType.Transaction,
                        Description = "Generated by API",
                        Reason = "Raised Dispute Transaction",
                        MobileNo = _res.MobileNo,
                        TransactionDoneAt = 1,
                        UserID = _res.UserID,
                        StoreOutletID = _res.OutletID,
                        ParticipationType = string.Empty,
                        TransactionID = _res.TransactionID
                    });
                }
            }
            return _res;
        }
        public async Task<RefundRequestResponse> MarkDispute(RefundRequestReq refundRequestReq)
        {
            var _res = new RefundRequestResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (!Validate.O.IsTransactionIDValid(refundRequestReq.refundRequest.RPID ?? string.Empty))
            {
                _res.Msg = ErrorCodes.InvalidParam;
                return _res;
            }
            refundRequestReq.CommonStr = _rinfo.GetRemoteIP();
            refundRequestReq.CommonStr2 = _rinfo.GetBrowser();
            if (refundRequestReq.refundRequest.UserID > 1)
            {

                string TransDate = ConnectionStringHelper.O.ConvertTransactionIDTo_dd_MMM_yyyy(refundRequestReq.refundRequest.RPID);
                _dal = ChangeConString(TransDate);
                IProcedureAsync _proc = new ProcMakeRefundRequest(_dal);
                _res = (RefundRequestResponse)await _proc.Call(refundRequestReq).ConfigureAwait(false);
                if (_res.Statuscode == ErrorCodes.One && _res.ServiceID == ServiceType.MoneyTransfer && _res.APICode == APICode.EKO)
                {
                    //EKO Status check process
                    var ekoObj = new EKOML(_accessor, _env, _res.APIID);
                    if ((refundRequestReq.refundRequest.OTP ?? string.Empty).Length > 0)
                    {
                        var dMRTransactionResponse = await ekoObj.Refund(_res.TID, _res.VendorID, refundRequestReq.refundRequest.RequestMode, _res.UserID, _res.Optional2, refundRequestReq.refundRequest.OTP).ConfigureAwait(false);
                        _res.Statuscode = dMRTransactionResponse.Statuscode;
                        _res.Msg = dMRTransactionResponse.Msg;
                        _res.ErrorCode = dMRTransactionResponse.ErrorCode;
                        if (dMRTransactionResponse.Statuscode == ErrorCodes.One)
                        {
                            var callbackData = new _CallbackData
                            {
                                TID = _res.TID,
                                TransactionID = _res.TransactionID,
                                RequestPage = "RefundOTP",
                                RequestIP = _rinfo.GetRemoteIP(),
                                Browser = _rinfo.GetBrowserFullInfo(),
                                TransactionStatus = RechargeRespType.REFUND,
                                VendorID = _res.VendorID,
                                LT = LoginType.ApplicationUser,
                                LoginID = _res.UserID
                            };
                            await UpdateRefund(callbackData).ConfigureAwait(false);

                        }
                    }
                    else if (refundRequestReq.refundRequest.IsResend)
                    {
                        var dMRTransactionResponse = await ekoObj.ResendRefundOTP(_res.VendorID, refundRequestReq.refundRequest.RequestMode, _res.UserID, _res.Optional2, _res.TID).ConfigureAwait(false);
                        _res.Statuscode = dMRTransactionResponse.Statuscode;
                        _res.Msg = dMRTransactionResponse.Msg;
                        _res.ErrorCode = dMRTransactionResponse.ErrorCode;
                        if (_res.Statuscode == ErrorCodes.Minus1)
                        {
                            _res.Msg = "OTP could not be resent!";
                        }
                    }
                    else
                    {
                        var dMRTransactionResponse = await ekoObj.GetTransactionStatus(_res.TID, refundRequestReq.refundRequest.RequestMode, _res.UserID, _res.Optional2, _res.VendorID).ConfigureAwait(false);
                        if (dMRTransactionResponse.IsRefundAvailable)
                        {
                            _res.Statuscode = ErrorCodes.One;
                            _res.IsOTPRequired = true;
                            _res.Msg = dMRTransactionResponse.Msg;
                            _res.ErrorCode = dMRTransactionResponse.ErrorCode;
                        }
                        else if (dMRTransactionResponse.Statuscode.In(RechargeRespType.REFUND, RechargeRespType.FAILED))
                        {
                            _res.Statuscode = ErrorCodes.One;
                            _res.Msg = nameof(DMTErrorCodes.Transaction_Status_Updated_Refund).Replace("_", " ");
                            _res.ErrorCode = DMTErrorCodes.Transaction_Status_Updated_Refund;
                            var callbackData = new _CallbackData
                            {
                                TID = _res.TID,
                                TransactionID = _res.TransactionID,
                                RequestPage = "RefundOTP_WO",
                                RequestIP = _rinfo.GetRemoteIP(),
                                Browser = _rinfo.GetBrowserFullInfo(),
                                TransactionStatus = RechargeRespType.REFUND,
                                VendorID = _res.VendorID,
                                LT = refundRequestReq.LoginTypeID,
                                LoginID = _res.UserID
                            };
                            await UpdateRefund(callbackData).ConfigureAwait(false);
                        }
                        else
                        {
                            _res.Statuscode = ErrorCodes.Minus1;
                            _res.Msg = "Currently refund not available";
                        }
                    }
                }
                else if (_res.Statuscode == ErrorCodes.One && _res.ServiceID == ServiceType.MoneyTransfer && _res.APICode.Equals(APICode.RBLMT))
                {
                    //RBLBank Status check process
                    var rblObj = new RBLML(_accessor, _env, _dal);
                    if ((refundRequestReq.refundRequest.OTP ?? string.Empty).Length > 0)
                    {
                        var dMRTransactionResponse = rblObj.Refund(_res.TID, _res.VendorID, refundRequestReq.refundRequest.RequestMode, _res.UserID, _res.Optional2, refundRequestReq.refundRequest.OTP, _res.APIID);
                        _res.Statuscode = dMRTransactionResponse.Statuscode;
                        _res.Msg = dMRTransactionResponse.Msg;
                        _res.ErrorCode = dMRTransactionResponse.ErrorCode;
                        if (dMRTransactionResponse.Statuscode == ErrorCodes.One)
                        {
                            var callbackData = new _CallbackData
                            {
                                TID = _res.TID,
                                TransactionID = _res.TransactionID,
                                RequestPage = "RefundOTP",
                                RequestIP = _rinfo.GetRemoteIP(),
                                Browser = _rinfo.GetBrowserFullInfo(),
                                TransactionStatus = RechargeRespType.REFUND,
                                VendorID = _res.VendorID,
                                LT = refundRequestReq.LoginTypeID,
                                LoginID = _res.UserID
                            };
                            await UpdateRefund(callbackData).ConfigureAwait(false);
                        }
                    }
                    else if (refundRequestReq.refundRequest.IsResend)
                    {
                        var dMRTransactionResponse = rblObj.RefundOTP(_res.VendorID, refundRequestReq.refundRequest.RequestMode, _res.UserID, _res.Optional2, _res.TID, _res.APIID);
                        _res.Statuscode = dMRTransactionResponse.Statuscode;
                        _res.Msg = dMRTransactionResponse.Msg;
                        _res.ErrorCode = dMRTransactionResponse.ErrorCode;
                        if (_res.Statuscode == ErrorCodes.Minus1)
                        {
                            _res.Msg = "OTP could not be resent!";
                        }
                    }
                    else
                    {
                        var dMRTransactionResponse = rblObj.GetTransactionStatus(_res.TID, refundRequestReq.refundRequest.RequestMode, _res.UserID, _res.APIID);
                        if (dMRTransactionResponse.IsRefundAvailable)
                        {
                            _res.Statuscode = ErrorCodes.One;
                            _res.IsOTPRequired = true;
                            _res.Msg = dMRTransactionResponse.Msg;
                            _res.ErrorCode = dMRTransactionResponse.ErrorCode;
                        }
                        else if (dMRTransactionResponse.Statuscode.In(RechargeRespType.REFUND, RechargeRespType.FAILED))
                        {
                            _res.Statuscode = ErrorCodes.One;
                            _res.Msg = nameof(DMTErrorCodes.Transaction_Status_Updated_Refund).Replace("_", " ");
                            _res.ErrorCode = DMTErrorCodes.Transaction_Status_Updated_Refund;
                            var callbackData = new _CallbackData
                            {
                                TID = _res.TID,
                                TransactionID = _res.TransactionID,
                                RequestPage = "RefundOTP_WO",
                                RequestIP = _rinfo.GetRemoteIP(),
                                Browser = _rinfo.GetBrowserFullInfo(),
                                TransactionStatus = RechargeRespType.REFUND,
                                VendorID = _res.VendorID,
                                LT = refundRequestReq.LoginTypeID,
                                LoginID = _res.UserID
                            };
                            await UpdateRefund(callbackData).ConfigureAwait(false);
                        }
                        else
                        {
                            _res.Statuscode = ErrorCodes.Minus1;
                            _res.Msg = "Currently refund not available";
                        }
                    }
                }
                else if ((_res.DisputeURL ?? "").Length > 20)
                {
                    try
                    {
                        var __ = AppWebRequest.O.CallUsingHttpWebRequest_GETAsync(_res.DisputeURL);
                    }
                    catch (Exception ex)
                    {
                        var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                        {
                            ClassName = GetType().Name,
                            FuncName = "MarkDisputeAPI",
                            Error = "Exception In DisputeURL Call:" + ex.Message,
                            LoginTypeID = refundRequestReq.LoginTypeID,
                            UserId = refundRequestReq.LoginID
                        });
                    }
                }
                if (_res.IsBBPS && _res.APICode.In(APICode.BILLAVENUE, APICode.AXISBANK, APICode.PAYUBBPS) && _res.Statuscode == ErrorCodes.One)
                {
                    var _Void = RaiseBBPSComplainWithoutSession(new GenerateBBPSComplainProcReq
                    {
                        ComplainType = BBPSComplainType.Transaction,
                        Description = "Generated by API",
                        Reason = "Raised Dispute Transaction",
                        MobileNo = _res.MobileNo,
                        TransactionDoneAt = 1,
                        UserID = _res.UserID,
                        StoreOutletID = _res.OutletID,
                        ParticipationType = string.Empty,
                        TransactionID = _res.TransactionID
                    });
                }
            }
            return _res;
        }
        public BBPSComplainAPIResponse RaiseBBPSComplainWithoutSession(GenerateBBPSComplainProcReq req)
        {
            var res = new BBPSComplainAPIResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            BBPSML bBPSML = new BBPSML(_accessor, _env, false);
            res = bBPSML.MakeBBPSComplain(req);
            if (res.Statuscode == 3)
            {
                res.Statuscode = -1;
            }
            return res;
        }

        #region AppRechargeReportSection
        public async Task<List<ProcRechargeReportResponse>> GetAppRechargeReport(_RechargeReportFilter _filter)
        {
            var _lst = new List<ProcRechargeReportResponse>();
            var IsScanMoreDB = false;
            if (_filter.IsRecent && !ApplicationSetting.IsSingleDB)
            {
                _filter.FromDate = GetUserLastTransactionDate(_filter.LT, _filter.LoginID, ServiceType.Recharge);
            }
            if (!string.IsNullOrEmpty(_filter.AccountNo))
            {
                if (ApplicationSetting.IsSingleDB)
                    _filter.FromDate = DateTime.Now.AddMonths(-2).ToString("dd MMM yyyy");
                else
                    IsScanMoreDB = true;
            }
            _filter.FromDate = string.IsNullOrEmpty(_filter.FromDate) ? DateTime.Now.ToString("dd MMM yyyy") : _filter.FromDate;
            if (IsScanMoreDB)
            {
                IProcedureAsync _proc = new ProcRechargeReport(_dal);
                _lst = (List<ProcRechargeReportResponse>)await _proc.Call(_filter).ConfigureAwait(false);
                _dal = new DAL(_c.GetConnectionString(1));
                _proc = new ProcRechargeReport(_dal);
                var _lstSecond = (List<ProcRechargeReportResponse>)await _proc.Call(_filter).ConfigureAwait(false);
                if (_lst == null)
                {
                    _lst = _lstSecond;
                }
                else if (_lst.Count == 0)
                {
                    _lst = _lstSecond;
                }
                else
                {
                    _lst.AddRange(_lstSecond);
                }
            }
            else
            {
                _dal = ChangeConString(_filter.FromDate);
                IProcedureAsync _proc = new ProcRechargeReport(_dal);
                _lst = (List<ProcRechargeReportResponse>)await _proc.Call(_filter).ConfigureAwait(false);
            }
            return _lst;
        }

        public string GetUserLastTransactionDate(int lt, int UserID, int ServiceID)
        {
            string lastDate = "";
            var req = new CommonReq
            {
                LoginTypeID = lt,
                LoginID = UserID,
                CommonInt = ServiceID
            };
            IProcedure _proc = new ProcGetLastTransactionDate(_dal);
            lastDate = _proc.Call(req).ToString();

            return lastDate;
        }
        #endregion
        #endregion
        #region AEPSReportSection
        public async Task<List<ProcRechargeReportResponse>> GetAEPSReport(RechargeReportFilter filter)
        {
            var _lst = new List<ProcRechargeReportResponse>();

            if (_lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.ShowAEPSReport))
            {
                var validate = Validate.O;
                var _filter = new _RechargeReportFilter
                {
                    LT = _lr.LoginTypeID,
                    LoginID = _lr.UserID,
                    TopRows = filter.TopRows,
                    Status = filter.Status,
                    APIID = filter.APIID,
                    FromDate = filter.FromDate,
                    ToDate = filter.ToDate,
                    Criteria = filter.Criteria,
                    CriteriaText = filter.CriteriaText,
                    IsExport = filter.IsExport,
                    RequestModeID = filter.RequestModeID,
                    OPTypeID = filter.OPTypeID,
                    OID = filter.OID
                };
                if (filter.Criteria > 0)
                {
                    if ((filter.CriteriaText ?? "") == "")
                    {
                        return _lst;
                    }
                }
                if (_filter.Criteria == Criteria.OutletMobile)
                {
                    if (!validate.IsMobile(_filter.CriteriaText))
                    {
                        return _lst;
                    }
                    _filter.OutletNo = _filter.CriteriaText;
                }
                if (_filter.Criteria == Criteria.AccountNo)
                {
                    _filter.AccountNo = _filter.CriteriaText;
                }
                if (_filter.Criteria == Criteria.LiveID)
                {
                    _filter.LiveID = _filter.CriteriaText;
                }
                if (_filter.Criteria == Criteria.TransactionID)
                {
                    if (_filter.CriteriaText[0] != Criteria.StartCharOFTransaction)
                    {
                        return _lst;
                    }
                    _filter.TransactionID = _filter.CriteriaText;
                }
                if (_filter.Criteria == Criteria.TID)
                {
                    try
                    {
                        _filter.TID = Convert.ToInt32(_filter.CriteriaText);
                    }
                    catch (Exception)
                    {
                        return _lst;
                    }
                }
                if (_filter.Criteria == Criteria.APIRequestID)
                {
                    _filter.APIRequestID = _filter.CriteriaText;
                }
                if (_filter.Criteria == Criteria.VendorID)
                {
                    _filter.VendorID = _filter.CriteriaText;
                }
                if (_filter.Criteria == Criteria.CCID)
                {
                    if (!validate.IsNumeric(_filter.CriteriaText))
                    {
                        return _lst;
                    }
                    _filter.CCID = Convert.ToInt32(_filter.CriteriaText);
                }
                if (_filter.Criteria == Criteria.CCMobileNo)
                {
                    _filter.CCMobileNo = _filter.CriteriaText;
                }
                if (_filter.Criteria == Criteria.UserID)
                {
                    var Prefix = Validate.O.Prefix(filter.CriteriaText);
                    if (Validate.O.IsNumeric(Prefix))
                        _filter.UserID = Validate.O.IsNumeric(filter.CriteriaText) ? Convert.ToInt32(filter.CriteriaText) : _filter.UserID;
                    var uid = Validate.O.LoginID(filter.CriteriaText);
                    _filter.UserID = Validate.O.IsNumeric(uid) ? Convert.ToInt32(uid) : _filter.UserID;
                }
                _filter.FromDate = string.IsNullOrEmpty(_filter.FromDate) ? DateTime.Now.ToString("dd MMM yyyy") : _filter.FromDate;
                _dal = ChangeConString(_filter.FromDate);
                IProcedureAsync _proc = new ProcAEPSTransactionReport(_dal);
                _lst = (List<ProcRechargeReportResponse>)await _proc.Call(_filter).ConfigureAwait(false);
            }
            return _lst;
        }
        public async Task<IEnumerable<ProcRechargeReportResponse>> GetAEPSReport(_RechargeReportFilter _filter)
        {
            var _lst = new List<ProcRechargeReportResponse>();
            if (_filter.IsRecent && !ApplicationSetting.IsSingleDB)
            {
                _filter.FromDate = GetUserLastTransactionDate(_filter.LT, _filter.LoginID, ServiceType.Recharge);
            }
            if (!string.IsNullOrEmpty(_filter.AccountNo))
            {
                if (ApplicationSetting.IsSingleDB)
                    _filter.FromDate = DateTime.Now.AddMonths(-2).ToString("dd MMM yyyy");
            }
            _filter.FromDate = string.IsNullOrEmpty(_filter.FromDate) ? DateTime.Now.ToString("dd MMM yyyy") : _filter.FromDate;
            _dal = ChangeConString(_filter.FromDate);
            IProcedureAsync _proc = new ProcAEPSTransactionReport(_dal);
            _lst = (List<ProcRechargeReportResponse>)await _proc.Call(_filter);
            return _lst;
        }
        #endregion
        #region DMTReportSection
        public async Task<List<ProcDMRTransactionResponse>> GetDMRReport(RechargeReportFilter filter)
        {
            var _lst = new List<ProcDMRTransactionResponse>();
            var loginResp = chkAlternateSession();
            if (loginResp != null)
            {
                if (loginResp.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.ShowDMTReport) || loginResp.LoginTypeID == LoginType.Employee)
                {
                    var validate = Validate.O;
                    var _filter = new _RechargeReportFilter
                    {
                        LT = loginResp.LoginTypeID,
                        LoginID = loginResp.UserID,
                        TopRows = filter.TopRows,
                        Status = filter.Status,
                        APIID = filter.APIID,
                        OPTypeID = filter.OPTypeID,
                        OID = filter.OID,
                        FromDate = filter.FromDate,
                        ToDate = filter.ToDate,
                        Criteria = filter.Criteria,
                        CriteriaText = filter.CriteriaText,
                        IsExport = filter.IsExport,
                        RequestModeID = filter.RequestModeID,
                        DMRModelID = filter.DMRModelID,
                        BankName = filter.BankName
                    };
                    if (filter.Criteria > 0)
                    {
                        if ((filter.CriteriaText ?? "") == "")
                        {
                            return _lst;
                        }
                    }
                    if (_filter.Criteria == Criteria.OutletMobile)
                    {
                        if (!validate.IsMobile(_filter.CriteriaText))
                        {
                            return _lst;
                        }
                        _filter.OutletNo = _filter.CriteriaText;
                    }
                    if (_filter.Criteria == Criteria.AccountNo)
                    {
                        _filter.AccountNo = _filter.CriteriaText;
                        if (string.IsNullOrEmpty(_filter.AccountNo))
                            return _lst;
                        if (ApplicationSetting.IsSingleDB)
                            _filter.FromDate = DateTime.Now.AddMonths(-2).ToString("dd MMM yyyy");
                    }
                    if (_filter.Criteria == Criteria.TransactionID)
                    {
                        if (_filter.CriteriaText[0] != Criteria.StartCharOFTransaction)
                        {
                            return _lst;
                        }
                        _filter.TransactionID = _filter.CriteriaText;
                    }
                    if (_filter.Criteria == Criteria.TID)
                    {
                        try
                        {
                            _filter.TID = Convert.ToInt32(_filter.CriteriaText);
                        }
                        catch (Exception)
                        {
                            return _lst;
                        }
                    }
                    if (_filter.Criteria == Criteria.APIRequestID)
                    {
                        _filter.APIRequestID = _filter.CriteriaText;
                    }
                    if (_filter.Criteria == Criteria.VendorID)
                    {
                        _filter.VendorID = _filter.CriteriaText;
                    }
                    if (_filter.Criteria == Criteria.UserOutletMobile)
                    {
                        if (!validate.IsMobile(_filter.CriteriaText))
                        {
                            return _lst;
                        }
                        _filter.UserOutletMobile = _filter.CriteriaText;
                    }
                    if (_filter.Criteria == Criteria.SenderMobile)
                    {
                        if (!validate.IsMobile(_filter.CriteriaText))
                        {
                            return _lst;
                        }
                        _filter.SenderMobile = _filter.CriteriaText;
                    }
                    if (_filter.Criteria == Criteria.CCID)
                    {
                        if (!validate.IsNumeric(_filter.CriteriaText))
                        {
                            return _lst;
                        }
                        _filter.CCID = Convert.ToInt32(_filter.CriteriaText);
                    }
                    if (_filter.Criteria == Criteria.CCMobileNo)
                    {
                        _filter.CCMobileNo = _filter.CriteriaText;
                    }
                    if (_filter.Criteria == Criteria.UserID)
                    {
                        var Prefix = Validate.O.Prefix(filter.CriteriaText);
                        if (Validate.O.IsNumeric(Prefix))
                            _filter.UserID = Validate.O.IsNumeric(filter.CriteriaText) ? Convert.ToInt32(filter.CriteriaText) : _filter.UserID;
                        var uid = Validate.O.LoginID(filter.CriteriaText);
                        _filter.UserID = Validate.O.IsNumeric(uid) ? Convert.ToInt32(uid) : _filter.UserID;
                    }

                    _filter.FromDate = string.IsNullOrEmpty(_filter.FromDate) ? DateTime.Now.ToString("dd MMM yyyy") : _filter.FromDate;
                    _dal = ChangeConString(_filter.FromDate);
                    IProcedureAsync _proc = new ProcDMRTransactionReport(_dal);
                    _lst = (List<ProcDMRTransactionResponse>)await _proc.Call(_filter);
                }
            }

            return _lst;
        }
        public async Task<List<DMRReportResponse>> GetDMRReportRole(RechargeReportFilter filter)
        {
            var _lst = new List<DMRReportResponse>();

            if (_lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.ShowRechargeReport))
            {
                Validate validate = Validate.O;
                _RechargeReportFilter _filter = new _RechargeReportFilter
                {
                    LoginID = _lr.UserID,
                    TopRows = filter.TopRows,
                    Status = filter.Status,
                    APIID = filter.APIID,
                    OID = filter.OID,
                    FromDate = filter.FromDate,
                    ToDate = filter.ToDate,
                    Criteria = filter.Criteria,
                    CriteriaText = filter.CriteriaText,
                    IsExport = filter.IsExport
                };
                if (filter.Criteria > 0)
                {
                    if ((filter.CriteriaText ?? "") == "")
                    {
                        return _lst;
                    }
                }
                if (_filter.Criteria == Criteria.OutletMobile)
                {
                    if (!validate.IsMobile(_filter.CriteriaText))
                    {
                        return _lst;
                    }
                    _filter.OutletNo = _filter.CriteriaText;
                }
                if (_filter.Criteria == Criteria.AccountNo)
                {
                    _filter.AccountNo = _filter.CriteriaText;
                }
                if (_filter.Criteria == Criteria.TransactionID)
                {
                    if (_filter.CriteriaText[0] != Criteria.StartCharOFTransaction)
                    {
                        return _lst;
                    }
                    _filter.TransactionID = _filter.CriteriaText;
                }
                if (_filter.Criteria == Criteria.TID)
                {
                    try
                    {
                        _filter.TID = Convert.ToInt32(_filter.CriteriaText);
                    }
                    catch (Exception)
                    {
                        return _lst;
                    }
                }
                if (_filter.Criteria == Criteria.APIRequestID)
                {
                    _filter.APIRequestID = _filter.CriteriaText;
                }
                if (_filter.Criteria == Criteria.VendorID)
                {
                    _filter.VendorID = _filter.CriteriaText;
                }
                if (_filter.Criteria == Criteria.UserOutletMobile)
                {
                    if (!validate.IsMobile(_filter.CriteriaText))
                    {
                        return _lst;
                    }
                    _filter.UserOutletMobile = _filter.CriteriaText;
                }
                if (_filter.Criteria == Criteria.SenderMobile)
                {
                    if (!validate.IsMobile(_filter.CriteriaText))
                    {
                        return _lst;
                    }
                    _filter.SenderMobile = _filter.CriteriaText;
                }
                if (_filter.Criteria == Criteria.CCID)
                {
                    if (!validate.IsNumeric(_filter.CriteriaText ?? ""))
                    {
                        return _lst;
                    }
                    _filter.CCID = Convert.ToInt32(_filter.CriteriaText);
                }
                if (_filter.Criteria == Criteria.CCMobileNo)
                {
                    _filter.CCMobileNo = _filter.CriteriaText;
                }
                if (_filter.Criteria == Criteria.UserID)
                {
                    var Prefix = Validate.O.Prefix(filter.CriteriaText);
                    if (Validate.O.IsNumeric(Prefix))
                        _filter.UserID = Validate.O.IsNumeric(filter.CriteriaText) ? Convert.ToInt32(filter.CriteriaText) : _filter.UserID;
                    var uid = Validate.O.LoginID(filter.CriteriaText);
                    _filter.UserID = Validate.O.IsNumeric(uid) ? Convert.ToInt32(uid) : _filter.UserID;
                }
                _filter.FromDate = string.IsNullOrEmpty(_filter.FromDate) ? DateTime.Now.ToString("dd MMM yyyy") : _filter.FromDate;
                _dal = ChangeConString(_filter.FromDate);
                IProcedureAsync _proc = new ProcDMRReportwithRole(_dal);
                _lst = (List<DMRReportResponse>)await _proc.Call(_filter);
            }
            return _lst;
        }

        #region AppRechargeReportSection
        public async Task<List<ProcDMRTransactionResponse>> GetAppDMRReport(_RechargeReportFilter _filter)
        {
            var _lst = new List<ProcDMRTransactionResponse>();
            if (_filter.IsRecent && !ApplicationSetting.IsSingleDB)
            {
                _filter.FromDate = GetUserLastTransactionDate(_filter.LT, _filter.LoginID, ServiceType.Recharge);
            }
            if (!string.IsNullOrEmpty(_filter.AccountNo))
            {
                if (ApplicationSetting.IsSingleDB)
                    _filter.FromDate = DateTime.Now.AddMonths(-2).ToString("dd MMM yyyy");
            }

            _filter.FromDate = string.IsNullOrEmpty(_filter.FromDate) ? DateTime.Now.ToString("dd MMM yyyy") : _filter.FromDate;
            _dal = ChangeConString(_filter.FromDate);
            ProcDMRTransactionReport _proc = new ProcDMRTransactionReport(_dal);
            _lst = (List<ProcDMRTransactionResponse>)await _proc.Call(_filter);
            return _lst;
        }
        #endregion
        #endregion

        #region RefundReport Section
        public RefundAPITransactios GetRefundLog(RefundLogFilter filter)
        {
            var data = new RefundAPITransactios();
            if (_lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.ShowRefundRequestHistory))
            {
                Validate validate = Validate.O;
                var _filter = new _RefundLogFilter
                {
                    LoginTypeID = _lr.LoginTypeID,
                    LoginID = _lr.UserID,
                    TopRows = filter.TopRows,
                    Status = filter.Status,
                    APIID = filter.APIID,
                    OID = filter.OID,
                    DateType = filter.DateType,
                    FromDate = filter.FromDate,
                    ToDate = filter.ToDate,
                    Criteria = filter.Criteria,
                    CriteriaText = (filter.CriteriaText ?? "").Trim(),
                    IsDMR = filter.IsDMR,
                    IsReport = filter.IsReport
                };
                if (_filter.TopRows > 0)
                {
                    if (_filter.Criteria == Criteria.OutletMobile)
                    {
                        if (!validate.IsMobile(_filter.CriteriaText ?? ""))
                            return data;
                        _filter.OutletNo = _filter.CriteriaText;
                    }
                    if (_filter.Criteria == Criteria.AccountNo)
                    {
                        if (string.IsNullOrEmpty(_filter.CriteriaText ?? ""))
                            return data;
                        _filter.AccountNo = _filter.CriteriaText;
                    }
                    if (_filter.Criteria == Criteria.TransactionID)
                    {
                        if (string.IsNullOrEmpty(_filter.CriteriaText))
                            return data;
                        if (_filter.CriteriaText[0] != Criteria.StartCharOFTransaction)
                            return data;
                        _filter.TransactionID = _filter.CriteriaText;
                    }
                    if (_filter.Criteria == Criteria.TID)
                    {
                        if (!validate.IsNumeric(_filter.CriteriaText ?? ""))
                            return data;
                        _filter.TID = Convert.ToInt32(_filter.CriteriaText);
                    }
                    if (_filter.Criteria == Criteria.UserID)
                    {
                        _filter.UserID = GetUserIDFromPrefixedID(_filter.CriteriaText);
                    }
                }
                else
                {
                    _filter.DateType = 3;
                    _filter.FromDate = DateTime.Now.AddDays(-45).ToString("dd MMM yyyy");
                    _filter.ToDate = DateTime.Now.ToString("dd MMM yyyy");
                    _filter.Status = 2;
                    _filter.TopRows = 1000;
                }
                IProcedure _proc = new ProcGetRefundLog(_dal);
                var _lst = (List<RefundTransaction>)_proc.Call(_filter);

                if (_lst.Count > 0)
                {
                    var pendingCount = _lst.GroupBy(x => new { x.APIID, x.APIName })
                    .Select(g => new PendingCountAPIWise { APIID = g.Key.APIID, APIName = g.Key.APIName, Count = g.Count() }).ToList();
                    data.PendingAPI = pendingCount;
                }
                data.RefundTransaction = _lst;
            }
            return data;
        }

        public IEnumerable<RefundTransaction> GetRefundLogApp(RefundLogFilter filter)
        {
            var _lst = new List<RefundTransaction>();
            Validate validate = Validate.O;
            var _filter = new _RefundLogFilter
            {
                LoginTypeID = filter.LoginTypeID,
                LoginID = filter.LoginID,
                TopRows = filter.TopRows,
                Status = filter.Status,
                DateType = filter.DateType,
                FromDate = filter.FromDate,
                ToDate = filter.ToDate,
                Criteria = filter.Criteria,
                CriteriaText = (filter.CriteriaText ?? "").Trim(),
                IsReport = true
            };
            if (_filter.TopRows > 0)
            {
                if (_filter.Criteria == Criteria.OutletMobile)
                {
                    if (!validate.IsMobile(_filter.CriteriaText ?? ""))
                        return _lst;
                    _filter.OutletNo = _filter.CriteriaText;
                }
                if (_filter.Criteria == Criteria.AccountNo)
                {
                    if (string.IsNullOrEmpty(_filter.CriteriaText ?? ""))
                        return _lst;
                    _filter.AccountNo = _filter.CriteriaText;
                }
                if (_filter.Criteria == Criteria.TransactionID)
                {
                    if (string.IsNullOrEmpty(_filter.CriteriaText))
                        return _lst;
                    if (_filter.CriteriaText[0] != Criteria.StartCharOFTransaction)
                        return _lst;
                    _filter.TransactionID = _filter.CriteriaText;
                }
                if (_filter.Criteria == Criteria.TID)
                {
                    if (!validate.IsNumeric(_filter.CriteriaText ?? ""))
                        return _lst;
                    _filter.TID = Convert.ToInt32(_filter.CriteriaText);
                }
                if (_filter.Criteria == Criteria.UserID)
                {
                    _filter.UserID = GetUserIDFromPrefixedID(_filter.CriteriaText);
                }
            }
            else
            {
                _filter.DateType = 3;
                _filter.FromDate = DateTime.Now.AddDays(-45).ToString("dd MMM yyyy");
                _filter.ToDate = DateTime.Now.ToString("dd MMM yyyy");
                _filter.Status = 2;
                _filter.TopRows = 1000;
            }
            IProcedure _proc = new ProcGetRefundLog(_dal);
            _lst = (List<RefundTransaction>)_proc.Call(_filter);
            return _lst;
        }

        public async Task<IResponseStatus> AcceptOrRejectRefundRequest(RefundRequestData refundRequestData)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (_lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.ShowRefundRequest) || (ApplicationSetting.IsWLAPIAllowed && _lr.RoleID == Role.MasterWL && _lr.IsWLAPIAllowed))
            {
                refundRequestData.LT = _lr.LoginTypeID;
                refundRequestData.LoginID = _lr.UserID;
                refundRequestData.TransactionID = refundRequestData.TransactionID.Contains((char)160) ? refundRequestData.TransactionID.Replace((char)160 + "", "") : refundRequestData.TransactionID;
                if (!Validate.O.IsTransactionIDValid(refundRequestData.TransactionID))
                {
                    _res.Msg = "Invalid Transaction ID";
                    return _res;
                }
                string TransDate = ConnectionStringHelper.O.ConvertTransactionIDTo_dd_MMM_yyyy(refundRequestData.TransactionID);
                _dal = ChangeConString(TransDate);

                refundRequestData.RequestIP = _rinfo.GetRemoteIP();
                refundRequestData.Browser = _rinfo.GetBrowserFullInfo() + "_" + _rinfo.GetLocalIP();

                IProcedureAsync _proc = new ProcUpdateTransactionRefundStatus(_dal);
                refundRequestData = (RefundRequestData)await _proc.Call(refundRequestData).ConfigureAwait(false);
                _res.Statuscode = refundRequestData.Statuscode;
                _res.Msg = refundRequestData.Msg;
                if ((refundRequestData.CallbackURL ?? string.Empty).Length > 6)
                {
                    var _callbackData = new _CallbackData
                    {
                        LT = 1,
                        UserID = refundRequestData.UserID,
                        UpdateUrl = refundRequestData.CallbackURL,
                        TransactionID = refundRequestData.TransactionID
                    };
                    var callback = new CallbackML(_accessor, _env, false);
                    await callback.UpdateAPIURLHitting(_callbackData).ConfigureAwait(false);
                }
                if (refundRequestData.Statuscode == ErrorCodes.Minus1)//|| refundRequestData.IsSameDay
                {
                    return _res;
                }
                if (refundRequestData.Statuscode == ErrorCodes.One)
                {
                    if (refundRequestData.RefundStatus == RefundType.REFUNDED || refundRequestData.RefundStatus == RefundType.REJECTED)
                    {
                        try
                        {
                            bool IsRejected = refundRequestData.RefundStatus == RefundType.REFUNDED ? false : true;
                            var alertParam = new AlertReplacementModel
                            {
                                WID = refundRequestData.WID,
                                AccountNo = refundRequestData.Account,
                                UserMobileNo = refundRequestData.UserMobileNo,
                                UserEmailID = refundRequestData.UserEmailID,
                                UserFCMID = refundRequestData.UserFCMID,
                                Amount = refundRequestData.Amount,
                                Operator = refundRequestData.OperatorName,
                                LiveID = refundRequestData.LiveID,
                                UserCurrentBalance = refundRequestData.BalanceAmount,
                                TransactionID = refundRequestData.TransactionID,
                                LoginID = refundRequestData.LoginID,
                                Company = refundRequestData.Company,
                                CompanyDomain = refundRequestData.CompanyDomain,
                                SupportEmail = refundRequestData.SupportEmail,
                                SupportNumber = refundRequestData.SupportNumber,
                                AccountEmail = refundRequestData.AccountEmail,
                                AccountsContactNo = refundRequestData.AccountContact,
                                CompanyAddress = refundRequestData.CompanyAddress,
                                OutletName = refundRequestData.OutletName,
                                BrandName = refundRequestData.BrandName,
                                UserName = refundRequestData.UserName,
                                FormatID = !IsRejected ? MessageFormat.RechargeRefund : MessageFormat.RechargeRefundReject,
                                NotificationTitle = !IsRejected ? "Recharge Refund" : "Recharge Refund Rejected",
                                WhatsappNo = refundRequestData.UserWhatsappNo,
                                TelegramNo = refundRequestData.UserTelegram,
                                HangoutNo = refundRequestData.UserHangout,
                                WhatsappConversationID = refundRequestData.ConversationID
                            };
                            if (string.IsNullOrEmpty(alertParam.WhatsappNo))
                            {
                                alertParam.WhatsappNo = alertParam.UserMobileNo;
                            }
                            IAlertML alertMl = new AlertML(_accessor, _env);
                            Parallel.Invoke(() => alertMl.RechargeRefundSMS(alertParam, IsRejected),
                                () => alertMl.RechargeRefundEmail(alertParam, IsRejected),
                                () => alertMl.RechargeRefundNotification(alertParam, IsRejected),
                               async () => await alertMl.WebNotification(alertParam).ConfigureAwait(false),
                               async () => await alertMl.SocialAlert(alertParam).ConfigureAwait(false));
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                }
            }
            return _res;

        }


        #endregion

        #region PendingAction
        public async Task<IResponseStatus> CheckStatusAsync(int TID, string TransactionID = "")
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (((_lr.RoleID == Role.Admin || (_lr.IsWLAPIAllowed && ApplicationSetting.IsWLAPIAllowed)) && _lr.LoginTypeID == LoginType.ApplicationUser) || userML.IsCustomerCareAuthorised(ActionCodes.ShowPending))
            {
                var _req = new CommonReq
                {
                    LoginTypeID = _lr.LoginTypeID,
                    LoginID = _lr.UserID,
                    CommonInt = TID,
                    CommonStr = _rinfo.GetRemoteIP(),
                    CommonStr2 = (_rinfo.GetBrowserFullInfo() ?? string.Empty) + "_" + (_rinfo.GetLocalIP() ?? string.Empty)
                };
                string TransDate = "";
                if (Validate.O.IsTransactionIDValid(TransactionID))
                {
                    TransDate = ConnectionStringHelper.O.ConvertTransactionIDTo_dd_MMM_yyyy(TransactionID);
                    _dal = ChangeConString(TransDate);
                }

                IProcedureAsync _proc = new ProcGetStatusCheckURL(_dal);
                var gETStatusCheckURL = (GETStatusCheckURL)await _proc.Call(_req).ConfigureAwait(false);
                _res.Msg = gETStatusCheckURL.Msg;
                if (gETStatusCheckURL.Statuscode == ErrorCodes.Minus1)
                {
                    return _res;
                }
                if (gETStatusCheckURL.RechargeAPI.aPIDetail.APICode.Equals(APICode.MAHAGRAM))
                {
                    gETStatusCheckURL.RechargeAPI.MGPSARequestID = gETStatusCheckURL.VendorID;
                }
                var typeMonthYear = new TypeMonthYear();
                if (TransDate != "")
                {
                    typeMonthYear = ConnectionStringHelper.O.GetTypeMonthYear(Convert.ToDateTime(TransDate.Replace(" ", "/")));
                }
                var transactionHelper = new TransactionHelper(_dal, _accessor, _env);
                if (gETStatusCheckURL.RechargeAPI.aPIDetail.DFormatID > 0 && gETStatusCheckURL.RechargeAPI.aPIDetail.URL.Contains(Replacement.DATE))
                {
                    ReplaceTDate(gETStatusCheckURL);
                }
                var rechargeAPIHit = await transactionHelper.HitRechargeAPI(gETStatusCheckURL.RechargeAPI, new TransactionServiceReq { IsStatusCheck = true, TransactionReqID = gETStatusCheckURL.TransactionReqID, VendorID = gETStatusCheckURL.VendorID, APIContext = gETStatusCheckURL.APIContext, TID = gETStatusCheckURL.TID }, new TransactionServiceResp { }).ConfigureAwait(false);
                var tstatus = await transactionHelper.MatchResponse(gETStatusCheckURL.OID, rechargeAPIHit, string.Empty).ConfigureAwait(false);
                _res.Statuscode = gETStatusCheckURL.Statuscode;
                _res.Msg = TID + "" + (char)160 + gETStatusCheckURL.TransactionID + (char)160 + rechargeAPIHit.aPIDetail.URL + (char)160 + rechargeAPIHit.Response;
                if (typeMonthYear.ConType == ConnectionStringType.DBCon)
                {
                    await transactionHelper.UpdateAPIResponse(TID, rechargeAPIHit).ConfigureAwait(false);
                    if (tstatus.Status.In(RechargeRespType.SUCCESS, RechargeRespType.FAILED))
                    {
                        var callbackData = new _CallbackData
                        {
                            TID = TID,
                            TransactionID = gETStatusCheckURL.TransactionID,
                            Msg = tstatus.OperatorID,
                            RequestPage = "StatusCheck",
                            RequestIP = _rinfo.GetRemoteIP(),
                            Browser = _rinfo.GetBrowserFullInfo(),
                            LoginID = _lr.UserID,
                            TransactionStatus = tstatus.Status,
                            VendorID = tstatus.VendorID,
                            LiveID = tstatus.OperatorID,
                            LT = _lr.LoginTypeID
                        };
                        await UpdateTransactionStatus(callbackData).ConfigureAwait(false);
                        if (tstatus.Status == RechargeRespType.SUCCESS && gETStatusCheckURL.IsBBPS && string.IsNullOrEmpty(gETStatusCheckURL.CustomerNumber))
                        {
                            var alertParam = new AlertReplacementModel
                            {
                                WID = gETStatusCheckURL.WID,
                                AccountNo = gETStatusCheckURL.Account,
                                UserMobileNo = gETStatusCheckURL.CustomerNumber,
                                Amount = gETStatusCheckURL.AmountR,
                                Operator = gETStatusCheckURL.Operator,
                                TransactionID = gETStatusCheckURL.TransactionID,
                                FormatID = MessageFormat.BBPSSuccess,
                                DATETIME = DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt")
                            };
                            IAlertML alertMl = new AlertML(_accessor, _env);
                            alertMl.BBPSSuccessSMS(alertParam);
                        }
                    }
                }
            }
            return _res;
        }
        public async Task<IResponseStatus> CheckAEPSStatusAsync(int TID, string TransactionID = "")
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if ((_lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser) || userML.IsCustomerCareAuthorised(ActionCodes.ShowPending))
            {
                var _req = new CommonReq { LoginTypeID = _lr.LoginTypeID, LoginID = _lr.UserID, CommonInt = TID, CommonStr = _rinfo.GetRemoteIP(), CommonStr2 = (_rinfo.GetBrowserFullInfo() ?? "") + "_" + (_rinfo.GetLocalIP() ?? "") };
                string TransDate = string.Empty;
                if (Validate.O.IsTransactionIDValid(TransactionID))
                {
                    TransDate = ConnectionStringHelper.O.ConvertTransactionIDTo_dd_MMM_yyyy(TransactionID);
                    _dal = ChangeConString(TransDate);
                }
                IProcedureAsync _proc = new ProcGetStatusCheckURL(_dal);
                var gETStatusCheckURL = (GETStatusCheckURL)await _proc.Call(_req).ConfigureAwait(false);
                _res.Msg = gETStatusCheckURL.Msg;
                if (gETStatusCheckURL.Statuscode == ErrorCodes.Minus1)
                {
                    return _res;
                }
                MiniBankTransactionServiceResp apiReqForProc = null;
                if (gETStatusCheckURL.RechargeAPI.aPIDetail.APICode == APICode.FINGPAY)
                {
                    FingpayML fingpayML = new FingpayML(_dal);
                    IAPISetting aPISetting = new OnboardingML(_accessor, _env);
                    var fingPaySetting = aPISetting.GetFingpay();
                    var transactionServiceReq = new MBStatusCheckRequest
                    {
                        TID = TID,
                        OutletID = gETStatusCheckURL.OutletID,
                        TransactionID = TransactionID
                    };

                    if (gETStatusCheckURL.SCode == ServiceCode.AEPS)
                    {
                        var apiResp = fingpayML.AEPSStatusCheck(fingPaySetting, transactionServiceReq);
                        apiReqForProc = new MiniBankTransactionServiceResp
                        {
                            LT = 1,
                            LoginID = 1,
                            Statuscode = apiResp.Statuscode,
                            VendorID = transactionServiceReq.VendorID,
                            Amount = apiResp.Amount,
                            LiveID = apiResp.LiveID,
                            Req = apiResp.Req,
                            Resp = apiResp.Resp,
                            RequestPage = "StatusCheck",
                            Remark = apiResp.Msg,
                            CardNumber = apiResp.CardNumber,
                            TID = transactionServiceReq.TID,
                            IPAddress = _rinfo.GetRemoteIP(),
                            Browser = _rinfo.GetBrowser(),
                            BankBalance = apiResp.BankBalance,
                            BankName = apiResp.BankName
                        };
                        gETStatusCheckURL.RequestURL = apiResp.Req;
                        gETStatusCheckURL.Response = apiResp.Resp;
                        gETStatusCheckURL.Statuscode = apiResp.Statuscode;
                    }
                    if (gETStatusCheckURL.SCode == ServiceCode.MiniBank)
                    {
                        var apiResp = fingpayML.MiniBankStatusCheck(fingPaySetting, transactionServiceReq);
                        apiReqForProc = new MiniBankTransactionServiceResp
                        {
                            LT = 1,
                            LoginID = 1,
                            Statuscode = apiResp.Statuscode,
                            VendorID = transactionServiceReq.VendorID,
                            Amount = apiResp.Amount,
                            LiveID = apiResp.LiveID,
                            Req = apiResp.Req,
                            Resp = apiResp.Resp,
                            RequestPage = "StatusCheck",
                            Remark = apiResp.Msg,
                            CardNumber = apiResp.CardNumber,
                            TID = transactionServiceReq.TID,
                            IPAddress = _rinfo.GetRemoteIP(),
                            Browser = _rinfo.GetBrowser(),
                            BankBalance = apiResp.BankBalance,
                            BankName = apiResp.BankName
                        };
                        gETStatusCheckURL.RequestURL = apiResp.Req;
                        gETStatusCheckURL.Response = apiResp.Resp;
                        gETStatusCheckURL.Statuscode = apiResp.Statuscode;
                    }
                }
                else if (gETStatusCheckURL.RechargeAPI.aPIDetail.APICode == APICode.SPRINT)
                {
                    SprintBBPSML sprint = new SprintBBPSML(_accessor, _env, _dal);
                    if (gETStatusCheckURL.SCode.In(ServiceCode.AEPS, ServiceCode.MiniBank))
                    {
                        var apiResp = new MiniBankTransactionServiceResp
                        {
                            Statuscode = RechargeRespType.PENDING,
                            Msg = ErrorCodes.NORESPONSE
                        };
                        if (gETStatusCheckURL.SPKey == SPKeys.Aadharpay)
                        {
                            apiResp = sprint.AadharPayStatusCheck(new AEPSUniversalRequest
                            {
                                TID = TID,
                                TransactionID = TransactionID
                            });
                        }
                        else
                        {
                            apiResp = sprint.StatusCheck(new AEPSUniversalRequest
                            {
                                TID = TID,
                                TransactionID = TransactionID
                            });
                        }
                        apiReqForProc = new MiniBankTransactionServiceResp
                        {
                            LT = 1,
                            LoginID = 1,
                            Statuscode = apiResp.Statuscode,
                            VendorID = apiResp.VendorID,
                            Amount = apiResp.Amount,
                            LiveID = apiResp.LiveID,
                            Req = apiResp.Req,
                            Resp = apiResp.Resp,
                            RequestPage = "StatusCheck",
                            Remark = apiResp.Msg,
                            CardNumber = apiResp.CardNumber,
                            TID = TID,
                            IPAddress = _rinfo.GetRemoteIP(),
                            Browser = _rinfo.GetBrowser(),
                            BankBalance = apiResp.BankBalance,
                            BankName = apiResp.BankName
                        };
                        gETStatusCheckURL.RequestURL = apiResp.Req;
                        gETStatusCheckURL.Response = apiResp.Resp;
                        gETStatusCheckURL.Statuscode = apiResp.Statuscode;

                    }

                }
                else if (gETStatusCheckURL.RechargeAPI.aPIDetail.APICode == APICode.INSTANTPAY) {
                    InstantPayUserOnboarding instantPay = new InstantPayUserOnboarding(_accessor, _env, gETStatusCheckURL.RechargeAPI.aPIDetail.APICode, gETStatusCheckURL.RechargeAPI.aPIDetail.ID, _dal);
                    var apiResp = new MiniBankTransactionServiceResp
                    {
                        Statuscode = RechargeRespType.PENDING,
                        Msg = ErrorCodes.NORESPONSE
                    };
                    apiResp = instantPay.StatusCheck(new AEPSUniversalRequest
                    {
                        TID = TID,
                        TransactionID = TransactionID
                    });
                    apiReqForProc = new MiniBankTransactionServiceResp
                    {
                        LT = 1,
                        LoginID = 1,
                        Statuscode = apiResp.Statuscode,
                        VendorID = apiResp.VendorID,
                        Amount = apiResp.Amount,
                        LiveID = apiResp.LiveID,
                        Req = apiResp.Req,
                        Resp = apiResp.Resp,
                        RequestPage = "StatusCheck",
                        Remark = apiResp.Msg,
                        CardNumber = apiResp.CardNumber,
                        TID = TID,
                        IPAddress = _rinfo.GetRemoteIP(),
                        Browser = _rinfo.GetBrowser(),
                        BankBalance = apiResp.BankBalance,
                        BankName = apiResp.BankName
                    };
                    gETStatusCheckURL.RequestURL = apiResp.Req;
                    gETStatusCheckURL.Response = apiResp.Resp;
                    gETStatusCheckURL.Statuscode = apiResp.Statuscode;
                }
                else if (gETStatusCheckURL.RechargeAPI.aPIDetail.APICode == APICode.MAHAGRAM)
                {
                    if (string.IsNullOrWhiteSpace(gETStatusCheckURL.VendorID))
                    {
                        _res.Statuscode = gETStatusCheckURL.Statuscode < 2 ? gETStatusCheckURL._Type : gETStatusCheckURL.Statuscode;
                        _res.Msg = TID + "" + (char)160 + gETStatusCheckURL.TransactionID + (char)160 + ErrorCodes.No_URL + (char)160 + ErrorCodes.No_Vendor_ID_Found;
                        return _res;
                    }
                    else
                    {
                        IMiniBankML miniBankML = new AEPSML(_accessor, _env, false);
                        if (gETStatusCheckURL.SCode.In(ServiceCode.AEPS, ServiceCode.MiniBank))
                        {
                            var transactionServiceReq = miniBankML.MBStatusCheck(new MBStatusCheckRequest
                            {
                                TID = TID,
                                APIStatus = gETStatusCheckURL._Type,
                                VendorID = gETStatusCheckURL.VendorID,
                                SCode = gETStatusCheckURL.SCode
                            });
                            gETStatusCheckURL.RequestURL = transactionServiceReq.Request;
                            gETStatusCheckURL.Response = transactionServiceReq.Response;
                            gETStatusCheckURL.Statuscode = transactionServiceReq.Statuscode;
                        }
                    }
                }
                if (apiReqForProc != null)
                {
                    apiReqForProc.TransactionID = TransactionID;
                    IMiniBankML mbML = new AEPSML(_accessor, _env, false);
                    var rsStatus = mbML.UpdateMiniBankResponse(apiReqForProc);
                }
                _res.Statuscode = gETStatusCheckURL.Statuscode < 2 ? gETStatusCheckURL._Type : gETStatusCheckURL.Statuscode;
                _res.Msg = TID + "" + (char)160 + gETStatusCheckURL.TransactionID + (char)160 + gETStatusCheckURL.RequestURL + (char)160 + gETStatusCheckURL.Response;
            }
            return _res;
        }
        public async Task<IResponseStatus> CheckStatusDMRAsync(int TID, string TransactionID, bool FromBulk = false)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if ((_lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser) || userML.IsCustomerCareAuthorised(ActionCodes.ShowPending))
            {
                var _req = new CommonReq { LoginTypeID = _lr.LoginTypeID, LoginID = _lr.UserID, CommonInt = TID, CommonStr = _rinfo.GetRemoteIP(), CommonStr2 = (_rinfo.GetBrowserFullInfo() ?? "") + "_" + (_rinfo.GetLocalIP() ?? "") };
                string TransDate = string.Empty;
                if (Validate.O.IsTransactionIDValid(TransactionID))
                {
                    TransDate = ConnectionStringHelper.O.ConvertTransactionIDTo_dd_MMM_yyyy(TransactionID);
                    _dal = ChangeConString(TransDate);
                }
                IProcedureAsync _proc = new ProcGetStatusCheckURL(_dal);
                var gETStatusCheckURL = (GETStatusCheckURL)await _proc.Call(_req).ConfigureAwait(false);
                _res.Msg = gETStatusCheckURL.Msg;
                if (gETStatusCheckURL.Statuscode == ErrorCodes.Minus1)
                {
                    return _res;
                }
                var transactionStatus = new TransactionStatus
                {
                    Status = gETStatusCheckURL._Type,
                    VendorID = gETStatusCheckURL.VendorID
                };
                var rechargeAPIHit = new RechargeAPIHit();
                if (gETStatusCheckURL.RechargeAPI.aPIDetail.APICode == APICode.AMAH && (gETStatusCheckURL.RechargeAPI.aPIDetail.IsStatusBulkCheck && FromBulk || !FromBulk))
                {
                    var wareWL = new MobileWareML(_accessor, _env);
                    var searchResult = wareWL.SearchTransaction(gETStatusCheckURL.RechargeAPI.aPIDetail.URL, gETStatusCheckURL.TID.ToString(), gETStatusCheckURL.VendorID, gETStatusCheckURL.RechargeAPI.aPIDetail.ID);
                    _res.Statuscode = gETStatusCheckURL.Statuscode;
                    rechargeAPIHit.Response = searchResult.Response;
                    rechargeAPIHit.aPIDetail = new APIDetail
                    {
                        URL = searchResult.Request,
                        ID = gETStatusCheckURL.RechargeAPI.aPIDetail.ID
                    };
                    rechargeAPIHit.aPIDetail.ID = gETStatusCheckURL.RechargeAPI.aPIDetail.ID;
                    _res.Msg = TID + string.Empty + (char)160 + gETStatusCheckURL.TransactionID + (char)160 + searchResult.Request + (char)160 + searchResult.Response;
                }
                else if (gETStatusCheckURL.RechargeAPI.aPIDetail.APICode == APICode.MRUY && (gETStatusCheckURL.RechargeAPI.aPIDetail.IsStatusBulkCheck && FromBulk || !FromBulk))
                {

                }
                else if (gETStatusCheckURL.RechargeAPI.aPIDetail.APICode == APICode.EKO && (gETStatusCheckURL.RechargeAPI.aPIDetail.IsStatusBulkCheck && FromBulk || !FromBulk))
                {
                    var ekoObj = new EKOML(_accessor, _env, gETStatusCheckURL.RechargeAPI.aPIDetail.ID);
                    var dMRTransactionResponse = await ekoObj.GetTransactionStatus(gETStatusCheckURL.TID, gETStatusCheckURL.RequestMode, gETStatusCheckURL.UserID, gETStatusCheckURL.Optional2, gETStatusCheckURL.VendorID);
                    transactionStatus.Status = dMRTransactionResponse.Statuscode;
                    transactionStatus.OperatorID = dMRTransactionResponse.LiveID;
                    rechargeAPIHit.aPIDetail = new APIDetail
                    {
                        URL = dMRTransactionResponse.Request,
                        ID = gETStatusCheckURL.RechargeAPI.aPIDetail.ID
                    };
                    rechargeAPIHit.Response = dMRTransactionResponse.Response;
                    _res.Statuscode = gETStatusCheckURL.Statuscode;
                    _res.Msg = TID + "" + (char)160 + gETStatusCheckURL.TransactionID + (char)160 + rechargeAPIHit.aPIDetail.URL + (char)160 + rechargeAPIHit.Response;
                }
                else if (gETStatusCheckURL.RechargeAPI.aPIDetail.APICode == APICode.EKO2 && (gETStatusCheckURL.RechargeAPI.aPIDetail.IsStatusBulkCheck && FromBulk || !FromBulk))
                {

                    var ekoObj = new EKO2ML(_accessor, _env, _dal, gETStatusCheckURL.RechargeAPI.aPIDetail.ID);
                    //GetTransactionStatus(int TID, int RequestMode, int UserID, string SenderNo, string VendorID)
                    var dMRTransactionResponse = await ekoObj.GetTransactionStatus(gETStatusCheckURL.TID, gETStatusCheckURL.RequestMode, gETStatusCheckURL.UserID, gETStatusCheckURL.Optional2, gETStatusCheckURL.VendorID, gETStatusCheckURL.RechargeAPI.aPIDetail.APIOutletID, gETStatusCheckURL.RechargeAPI.aPIDetail.GroupCode).ConfigureAwait(false);
                    transactionStatus.Status = dMRTransactionResponse.Statuscode;
                    transactionStatus.OperatorID = dMRTransactionResponse.LiveID;
                    rechargeAPIHit.aPIDetail = new APIDetail
                    {
                        URL = dMRTransactionResponse.Request,
                        ID = gETStatusCheckURL.RechargeAPI.aPIDetail.ID
                    };
                    rechargeAPIHit.Response = dMRTransactionResponse.Response;
                    _res.Statuscode = gETStatusCheckURL.Statuscode;
                    _res.Msg = TID + "" + (char)160 + gETStatusCheckURL.TransactionID + (char)160 + rechargeAPIHit.aPIDetail.URL + (char)160 + rechargeAPIHit.Response;
                }
                #region Bill Avenue status check
                else if (gETStatusCheckURL.RechargeAPI.aPIDetail.APICode == APICode.BILLAVENUE && (gETStatusCheckURL.RechargeAPI.aPIDetail.IsStatusBulkCheck && FromBulk || !FromBulk))
                {
                    var mTAPIReq = new MTAPIRequest
                    {
                        TID = gETStatusCheckURL.TID,
                        VID = gETStatusCheckURL.VendorID,
                        TransactionID = gETStatusCheckURL.GroupIID,
                        GroupID = Convert.ToString(gETStatusCheckURL.GroupIID),
                        UserID = gETStatusCheckURL.UserID,
                        APIID = gETStatusCheckURL.RechargeAPI.aPIDetail.ID,
                        RequestMode = gETStatusCheckURL.RequestMode
                    };

                    var billaveObj = new BillAvenueMT_ML(_accessor, _env, gETStatusCheckURL.RechargeAPI.aPIDetail.APICode, gETStatusCheckURL.RechargeAPI.aPIDetail.ID, _dal);
                    var dMTTransactionURL = billaveObj.AppSetting().DMTTransactionURL;
                    var dMRTransactionResponse = billaveObj.GetTransactionStatus(mTAPIReq);
                    transactionStatus.Status = dMRTransactionResponse.Statuscode;
                    transactionStatus.OperatorID = dMRTransactionResponse.LiveID;
                    rechargeAPIHit.aPIDetail = new APIDetail
                    {
                        URL = dMTTransactionURL,
                        ID = gETStatusCheckURL.RechargeAPI.aPIDetail.ID
                    };
                    rechargeAPIHit.Response = dMRTransactionResponse.Response;
                    _res.Statuscode = gETStatusCheckURL.Statuscode;
                    _res.Msg = TID + "" + (char)160 + gETStatusCheckURL.TransactionID + (char)160 + rechargeAPIHit.aPIDetail.URL + (char)160 + rechargeAPIHit.Response;
                }
                #endregion
                else if (gETStatusCheckURL.RechargeAPI.aPIDetail.APICode == APICode.OPENBANK && (gETStatusCheckURL.RechargeAPI.aPIDetail.IsStatusBulkCheck && FromBulk || !FromBulk))
                {
                    var obML = new OpenBankML(_accessor, _env, gETStatusCheckURL.RechargeAPI.aPIDetail.ID);
                    var dMRTransactionResponse = await obML.GetPayout(gETStatusCheckURL.TID, gETStatusCheckURL.RequestMode, gETStatusCheckURL.UserID).ConfigureAwait(false);
                    transactionStatus.Status = dMRTransactionResponse.Statuscode;
                    transactionStatus.OperatorID = dMRTransactionResponse.LiveID;
                    rechargeAPIHit.aPIDetail = new APIDetail
                    {
                        URL = dMRTransactionResponse.Request,
                        ID = gETStatusCheckURL.RechargeAPI.aPIDetail.ID
                    };
                    rechargeAPIHit.Response = dMRTransactionResponse.Response;
                    _res.Statuscode = gETStatusCheckURL.Statuscode;
                    _res.Msg = TID + "" + (char)160 + gETStatusCheckURL.TransactionID + (char)160 + rechargeAPIHit.aPIDetail.URL + (char)160 + rechargeAPIHit.Response;
                }
                else if (gETStatusCheckURL.RechargeAPI.aPIDetail.APICode == APICode.PAYTM && (gETStatusCheckURL.RechargeAPI.aPIDetail.IsStatusBulkCheck && FromBulk || !FromBulk))
                {
                    var paytm = new PaytmML(_accessor, _env, gETStatusCheckURL.RechargeAPI.aPIDetail.ID);
                    var dMRTransactionResponse = await paytm.GetTransactionStatus(gETStatusCheckURL.TID, gETStatusCheckURL.RequestMode, gETStatusCheckURL.UserID).ConfigureAwait(false);
                    transactionStatus.Status = dMRTransactionResponse.Statuscode;
                    transactionStatus.OperatorID = dMRTransactionResponse.LiveID;
                    rechargeAPIHit.aPIDetail = new APIDetail
                    {
                        URL = dMRTransactionResponse.Request,
                        ID = gETStatusCheckURL.RechargeAPI.aPIDetail.ID
                    };
                    rechargeAPIHit.Response = dMRTransactionResponse.Response;
                    _res.Statuscode = gETStatusCheckURL.Statuscode;
                    _res.Msg = TID + "" + (char)160 + gETStatusCheckURL.TransactionID + (char)160 + rechargeAPIHit.aPIDetail.URL + (char)160 + rechargeAPIHit.Response;
                }
                else if (gETStatusCheckURL.RechargeAPI.aPIDetail.APICode.EndsWith(APICode.RAZORPAYOUT) && (gETStatusCheckURL.RechargeAPI.aPIDetail.IsStatusBulkCheck && FromBulk || !FromBulk))
                {

                    if (string.IsNullOrEmpty(gETStatusCheckURL.VendorID) && string.IsNullOrEmpty(gETStatusCheckURL.VendorID2))
                    {
                        _res.Statuscode = ErrorCodes.Minus1;
                        _res.Msg = "No VendorID Found!";
                        return _res;
                    }
                    var moneyTransferAPIML = new RZRPayoutML(_accessor, _env, _dal, gETStatusCheckURL.RechargeAPI.aPIDetail.APICode);
                    var dMRTransactionResponse = moneyTransferAPIML.GetTransactionStatus(gETStatusCheckURL.TID, gETStatusCheckURL.TransactionID, gETStatusCheckURL.RequestMode, gETStatusCheckURL.UserID, gETStatusCheckURL.RechargeAPI.aPIDetail.ID, gETStatusCheckURL.VendorID, gETStatusCheckURL.RechargeAPI.aPIDetail.GroupCode, gETStatusCheckURL.VendorID2);
                    transactionStatus.Status = dMRTransactionResponse.Statuscode;
                    transactionStatus.OperatorID = dMRTransactionResponse.LiveID;
                    rechargeAPIHit.aPIDetail = new APIDetail
                    {
                        URL = dMRTransactionResponse.Request,
                        ID = gETStatusCheckURL.RechargeAPI.aPIDetail.ID
                    };
                    rechargeAPIHit.Response = dMRTransactionResponse.Response;
                    _res.Statuscode = gETStatusCheckURL.Statuscode;
                    _res.Msg = TID + "" + (char)160 + gETStatusCheckURL.TransactionID + (char)160 + rechargeAPIHit.aPIDetail.URL + (char)160 + rechargeAPIHit.Response;
                }
                else if (gETStatusCheckURL.RechargeAPI.aPIDetail.APICode.EndsWith("FNTH") && (gETStatusCheckURL.RechargeAPI.aPIDetail.IsStatusBulkCheck && FromBulk || !FromBulk))
                {
                    var moneyTransferAPIML = new FintechPayoutML(_accessor, _env, _dal, gETStatusCheckURL.RechargeAPI.aPIDetail.APICode);
                    var dMRTransactionResponse = moneyTransferAPIML.GetTransactionStatus(gETStatusCheckURL.TID, gETStatusCheckURL.TransactionID, gETStatusCheckURL.RequestMode, gETStatusCheckURL.UserID, gETStatusCheckURL.RechargeAPI.aPIDetail.ID, gETStatusCheckURL.VendorID);
                    transactionStatus.Status = dMRTransactionResponse.Statuscode;
                    transactionStatus.OperatorID = dMRTransactionResponse.LiveID;
                    rechargeAPIHit.aPIDetail = new APIDetail
                    {
                        URL = dMRTransactionResponse.Request,
                        ID = gETStatusCheckURL.RechargeAPI.aPIDetail.ID
                    };
                    rechargeAPIHit.Response = dMRTransactionResponse.Response;
                    _res.Statuscode = gETStatusCheckURL.Statuscode;
                    _res.Msg = TID + "" + (char)160 + gETStatusCheckURL.TransactionID + (char)160 + rechargeAPIHit.aPIDetail.URL + (char)160 + rechargeAPIHit.Response;
                }
                else if (gETStatusCheckURL.RechargeAPI.aPIDetail.APICode == APICode.RBLMT && (gETStatusCheckURL.RechargeAPI.aPIDetail.IsStatusBulkCheck && FromBulk || !FromBulk))
                {
                    var moneyTransferAPIML = new RBLML(_accessor, _env, _dal);
                    var dMRTransactionResponse = moneyTransferAPIML.GetTransactionStatus(gETStatusCheckURL.TID, gETStatusCheckURL.RequestMode, gETStatusCheckURL.UserID, gETStatusCheckURL.RechargeAPI.aPIDetail.ID);
                    transactionStatus.Status = dMRTransactionResponse.Statuscode;
                    transactionStatus.OperatorID = dMRTransactionResponse.LiveID;
                    rechargeAPIHit.aPIDetail = new APIDetail
                    {
                        URL = dMRTransactionResponse.Request,
                        ID = gETStatusCheckURL.RechargeAPI.aPIDetail.ID
                    };
                    rechargeAPIHit.Response = dMRTransactionResponse.Response;
                    _res.Statuscode = gETStatusCheckURL.Statuscode;
                    _res.Msg = TID + "" + (char)160 + gETStatusCheckURL.TransactionID + (char)160 + rechargeAPIHit.aPIDetail.URL + (char)160 + rechargeAPIHit.Response;
                }
                else if (gETStatusCheckURL.RechargeAPI.aPIDetail.APICode == APICode.PAYUDMT && (gETStatusCheckURL.RechargeAPI.aPIDetail.IsStatusBulkCheck && FromBulk || !FromBulk))
                {
                    var moneyTransferAPIML = new PayUDmtML(_accessor, _env, _dal);
                    var dMRTransactionResponse = moneyTransferAPIML.GetTransactionStatus(gETStatusCheckURL.TID, gETStatusCheckURL.TransactionID, gETStatusCheckURL.RequestMode, gETStatusCheckURL.UserID, gETStatusCheckURL.RechargeAPI.aPIDetail.ID);
                    transactionStatus.Status = dMRTransactionResponse.Statuscode;
                    transactionStatus.OperatorID = dMRTransactionResponse.LiveID;
                    rechargeAPIHit.aPIDetail = new APIDetail
                    {
                        URL = dMRTransactionResponse.Request,
                        ID = gETStatusCheckURL.RechargeAPI.aPIDetail.ID
                    };
                    rechargeAPIHit.Response = dMRTransactionResponse.Response;
                    _res.Statuscode = gETStatusCheckURL.Statuscode;
                    _res.Msg = TID + "" + (char)160 + gETStatusCheckURL.TransactionID + (char)160 + rechargeAPIHit.aPIDetail.URL + (char)160 + rechargeAPIHit.Response;
                }
                else if (gETStatusCheckURL.RechargeAPI.aPIDetail.APICode.In(APICode.INSTANTPayDirect, APICode.INSTANTPAY) && (gETStatusCheckURL.RechargeAPI.aPIDetail.IsStatusBulkCheck && FromBulk || !FromBulk))
                {
                    var moneyTransferAPIML = new IPayPayoutDirectML(_accessor, _env, _dal, gETStatusCheckURL.RechargeAPI.aPIDetail.APICode);
                    var dMRTransactionResponse = moneyTransferAPIML.GetTransactionStatus(gETStatusCheckURL.TID, gETStatusCheckURL.TransactionID, gETStatusCheckURL.RequestMode, gETStatusCheckURL.UserID, gETStatusCheckURL.RechargeAPI.aPIDetail.ID);
                    transactionStatus.Status = dMRTransactionResponse.Statuscode;
                    transactionStatus.OperatorID = dMRTransactionResponse.LiveID;
                    rechargeAPIHit.aPIDetail = new APIDetail
                    {
                        URL = dMRTransactionResponse.Request,
                        ID = gETStatusCheckURL.RechargeAPI.aPIDetail.ID
                    };
                    rechargeAPIHit.Response = dMRTransactionResponse.Response;
                    _res.Statuscode = gETStatusCheckURL.Statuscode;
                    _res.Msg = TID + "" + (char)160 + gETStatusCheckURL.TransactionID + (char)160 + rechargeAPIHit.aPIDetail.URL + (char)160 + rechargeAPIHit.Response;
                }
                else if (gETStatusCheckURL.RechargeAPI.aPIDetail.APICode == APICode.RECHARGEDADDY && (gETStatusCheckURL.RechargeAPI.aPIDetail.IsStatusBulkCheck && FromBulk || !FromBulk))
                {
                    var moneyTransferAPIML = new RechargeDaddyML(_accessor, _env, _dal);
                    var dMRTransactionResponse = moneyTransferAPIML.GetTransactionStatus(gETStatusCheckURL.TID, gETStatusCheckURL.TransactionID, gETStatusCheckURL.RequestMode, gETStatusCheckURL.UserID, gETStatusCheckURL.RechargeAPI.aPIDetail.ID, gETStatusCheckURL.VendorID, gETStatusCheckURL.RechargeAPI.aPIDetail.GroupCode);
                    transactionStatus.Status = dMRTransactionResponse.Statuscode;
                    transactionStatus.OperatorID = dMRTransactionResponse.LiveID;
                    rechargeAPIHit.aPIDetail = new APIDetail
                    {
                        URL = dMRTransactionResponse.Request,
                        ID = gETStatusCheckURL.RechargeAPI.aPIDetail.ID
                    };
                    rechargeAPIHit.Response = dMRTransactionResponse.Response;
                    _res.Statuscode = gETStatusCheckURL.Statuscode;
                    _res.Msg = TID + "" + (char)160 + gETStatusCheckURL.TransactionID + (char)160 + rechargeAPIHit.aPIDetail.URL + (char)160 + rechargeAPIHit.Response;
                }
                else if (gETStatusCheckURL.RechargeAPI.aPIDetail.APICode == APICode.PayOneMoney && (gETStatusCheckURL.RechargeAPI.aPIDetail.IsStatusBulkCheck && FromBulk || !FromBulk))
                {
                    var moneyTransferAPIML = new PayOneMoneyML(_accessor, _env, _dal);
                    var dMRTransactionResponse = moneyTransferAPIML.GetTransactionStatus(gETStatusCheckURL.TID, gETStatusCheckURL.TransactionID, gETStatusCheckURL.RequestMode, gETStatusCheckURL.UserID, gETStatusCheckURL.RechargeAPI.aPIDetail.ID, gETStatusCheckURL.VendorID, gETStatusCheckURL.RechargeAPI.aPIDetail.GroupCode);
                    transactionStatus.Status = dMRTransactionResponse.Statuscode;
                    transactionStatus.OperatorID = dMRTransactionResponse.LiveID;
                    rechargeAPIHit.aPIDetail = new APIDetail
                    {
                        URL = dMRTransactionResponse.Request,
                        ID = gETStatusCheckURL.RechargeAPI.aPIDetail.ID
                    };
                    rechargeAPIHit.Response = dMRTransactionResponse.Response;
                    _res.Statuscode = gETStatusCheckURL.Statuscode;
                    _res.Msg = TID + "" + (char)160 + gETStatusCheckURL.TransactionID + (char)160 + rechargeAPIHit.aPIDetail.URL + (char)160 + rechargeAPIHit.Response;
                }
                else if (gETStatusCheckURL.RechargeAPI.aPIDetail.APICode == APICode.SECUREPAYMENT && (gETStatusCheckURL.RechargeAPI.aPIDetail.IsStatusBulkCheck && FromBulk || !FromBulk))
                {
                    var moneyTransferAPIML = new SecurePaymentML(_accessor, _env, _dal, APICode.SECUREPAYMENT);
                    var dMRTransactionResponse = moneyTransferAPIML.GetTransactionStatus(gETStatusCheckURL.TID, gETStatusCheckURL.TransactionID, gETStatusCheckURL.RequestMode, gETStatusCheckURL.UserID, gETStatusCheckURL.RechargeAPI.aPIDetail.ID, gETStatusCheckURL.VendorID);
                    transactionStatus.Status = dMRTransactionResponse.Statuscode;
                    transactionStatus.OperatorID = dMRTransactionResponse.LiveID;
                    rechargeAPIHit.aPIDetail = new APIDetail
                    {
                        URL = dMRTransactionResponse.Request,
                        ID = gETStatusCheckURL.RechargeAPI.aPIDetail.ID
                    };
                    rechargeAPIHit.Response = dMRTransactionResponse.Response;
                    _res.Statuscode = gETStatusCheckURL.Statuscode;
                    _res.Msg = TID + "" + (char)160 + gETStatusCheckURL.TransactionID + (char)160 + rechargeAPIHit.aPIDetail.URL + (char)160 + rechargeAPIHit.Response;
                }
                else if (gETStatusCheckURL.RechargeAPI.aPIDetail.APICode == APICode.MAHAGRAM && (gETStatusCheckURL.RechargeAPI.aPIDetail.IsStatusBulkCheck && FromBulk || !FromBulk))
                {
                    var moneyTransferAPIML = new MahagramAPIML(_accessor, _env, _dal);
                    var dMRTransactionResponse = moneyTransferAPIML.GetTransactionStatus(gETStatusCheckURL.TID, gETStatusCheckURL.TransactionID, gETStatusCheckURL.RequestMode, gETStatusCheckURL.UserID, gETStatusCheckURL.RechargeAPI.aPIDetail.ID, gETStatusCheckURL.VendorID, gETStatusCheckURL.VendorID2);
                    transactionStatus.Status = dMRTransactionResponse.Statuscode;
                    transactionStatus.OperatorID = dMRTransactionResponse.LiveID;
                    rechargeAPIHit.aPIDetail = new APIDetail
                    {
                        URL = dMRTransactionResponse.Request,
                        ID = gETStatusCheckURL.RechargeAPI.aPIDetail.ID
                    };
                    rechargeAPIHit.Response = dMRTransactionResponse.Response;
                    _res.Statuscode = gETStatusCheckURL.Statuscode;
                    _res.Msg = TID + "" + (char)160 + gETStatusCheckURL.TransactionID + (char)160 + rechargeAPIHit.aPIDetail.URL + (char)160 + rechargeAPIHit.Response;
                }
                else if (gETStatusCheckURL.RechargeAPI.aPIDetail.APICode == APICode.MMWFintech && (gETStatusCheckURL.RechargeAPI.aPIDetail.IsStatusBulkCheck && FromBulk || !FromBulk))
                {


                    var moneyTransferAPIML = new MMWFintechML(_accessor, _env, APICode.MMWFintech, gETStatusCheckURL.RechargeAPI.aPIDetail.ID, _dal);
                    var dMRTransactionResponse = moneyTransferAPIML.GetTransactionStatus(gETStatusCheckURL.TID, gETStatusCheckURL.UserID, gETStatusCheckURL.RechargeAPI.aPIDetail.ID, gETStatusCheckURL.RequestMode);
                    transactionStatus.Status = dMRTransactionResponse.Statuscode;
                    transactionStatus.OperatorID = dMRTransactionResponse.LiveID;
                    rechargeAPIHit.aPIDetail = new APIDetail
                    {
                        URL = dMRTransactionResponse.Request,
                        ID = gETStatusCheckURL.RechargeAPI.aPIDetail.ID
                    };
                    rechargeAPIHit.Response = dMRTransactionResponse.Response;
                    _res.Statuscode = gETStatusCheckURL.Statuscode;
                    _res.Msg = TID + "" + (char)160 + gETStatusCheckURL.TransactionID + (char)160 + rechargeAPIHit.aPIDetail.URL + (char)160 + rechargeAPIHit.Response;
                }
                else if (gETStatusCheckURL.RechargeAPI.aPIDetail.APICode == APICode.SPRINT && (gETStatusCheckURL.RechargeAPI.aPIDetail.IsStatusBulkCheck && FromBulk || !FromBulk))
                {
                    var moneyTransferAPIML = new SprintDMTML(_accessor, _env, APICode.SPRINT, gETStatusCheckURL.RechargeAPI.aPIDetail.ID, _dal);
                    var dMRTransactionResponse = moneyTransferAPIML.GetTransactionStatus(gETStatusCheckURL.TID, gETStatusCheckURL.UserID, gETStatusCheckURL.RechargeAPI.aPIDetail.ID, gETStatusCheckURL.RequestMode);
                    transactionStatus.Status = dMRTransactionResponse.Statuscode;
                    transactionStatus.OperatorID = dMRTransactionResponse.LiveID;
                    rechargeAPIHit.aPIDetail = new APIDetail
                    {
                        URL = dMRTransactionResponse.Request,
                        ID = gETStatusCheckURL.RechargeAPI.aPIDetail.ID
                    };
                    rechargeAPIHit.Response = dMRTransactionResponse.Response;
                    _res.Statuscode = gETStatusCheckURL.Statuscode;
                    _res.Msg = TID + "" + (char)160 + gETStatusCheckURL.TransactionID + (char)160 + rechargeAPIHit.aPIDetail.URL + (char)160 + rechargeAPIHit.Response;
                }
                else if (gETStatusCheckURL.RechargeAPI.aPIDetail.APICode == APICode.FINO && (gETStatusCheckURL.RechargeAPI.aPIDetail.IsStatusBulkCheck && FromBulk || !FromBulk))
                {
                    var moneyTransferAPIML = new FINOML(_accessor, _env, _dal);
                    var dMRTransactionResponse = moneyTransferAPIML.GetTransactionStatus(gETStatusCheckURL.TID, gETStatusCheckURL.UserID, gETStatusCheckURL.RequestMode, gETStatusCheckURL.RechargeAPI.aPIDetail.ID, gETStatusCheckURL.RechargeAPI.aPIDetail.GroupCode);
                    transactionStatus.Status = dMRTransactionResponse.Statuscode;
                    transactionStatus.OperatorID = dMRTransactionResponse.LiveID;
                    rechargeAPIHit.aPIDetail = new APIDetail
                    {
                        URL = dMRTransactionResponse.Request,
                        ID = gETStatusCheckURL.RechargeAPI.aPIDetail.ID
                    };
                    rechargeAPIHit.Response = dMRTransactionResponse.Response;
                    _res.Statuscode = gETStatusCheckURL.Statuscode;
                    _res.Msg = TID + "" + (char)160 + gETStatusCheckURL.TransactionID + (char)160 + rechargeAPIHit.aPIDetail.URL + (char)160 + rechargeAPIHit.Response;
                }
                else if (gETStatusCheckURL.RechargeAPI.aPIDetail.APICode == APICode.CASHFREE && (gETStatusCheckURL.RechargeAPI.aPIDetail.IsStatusBulkCheck && FromBulk || !FromBulk))
                {
                    var _apiGroupCode = gETStatusCheckURL.RechargeAPI.aPIDetail.GroupCode;
                    var moneyTransferAPIML = new CashFreePayoutML(_accessor, _env, _dal, _apiGroupCode);
                    var dMRTransactionResponse = moneyTransferAPIML.GetTransactionStatus(gETStatusCheckURL.TID, gETStatusCheckURL.VendorID, gETStatusCheckURL.RequestMode, gETStatusCheckURL.UserID, gETStatusCheckURL.RechargeAPI.aPIDetail.ID, _apiGroupCode);
                    transactionStatus.Status = dMRTransactionResponse.Statuscode;
                    transactionStatus.OperatorID = dMRTransactionResponse.LiveID;
                    rechargeAPIHit.aPIDetail = new APIDetail
                    {
                        URL = dMRTransactionResponse.Request,
                        ID = gETStatusCheckURL.RechargeAPI.aPIDetail.ID
                    };
                    rechargeAPIHit.Response = dMRTransactionResponse.Response;
                    _res.Statuscode = gETStatusCheckURL.Statuscode;
                    _res.Msg = TID + "" + (char)160 + gETStatusCheckURL.TransactionID + (char)160 + rechargeAPIHit.aPIDetail.URL + (char)160 + rechargeAPIHit.Response;
                }
                else if (gETStatusCheckURL.RechargeAPI.aPIDetail.APICode == APICode.HYPTO && (gETStatusCheckURL.RechargeAPI.aPIDetail.IsStatusBulkCheck && FromBulk || !FromBulk))
                {
                    var moneyTransferAPIML = new HyptoML(_accessor, _env, gETStatusCheckURL.RechargeAPI.aPIDetail.ID, _dal);
                    var dMRTransactionResponse = moneyTransferAPIML.GetTransactionStatus(gETStatusCheckURL.TID, gETStatusCheckURL.TransactionID, gETStatusCheckURL.VendorID, gETStatusCheckURL.RequestMode, gETStatusCheckURL.UserID, gETStatusCheckURL.RechargeAPI.aPIDetail.ID, gETStatusCheckURL.RechargeAPI.aPIDetail.GroupCode);
                    transactionStatus.Status = dMRTransactionResponse.Statuscode;
                    transactionStatus.OperatorID = dMRTransactionResponse.LiveID;
                    rechargeAPIHit.aPIDetail = new APIDetail
                    {
                        URL = dMRTransactionResponse.Request,
                        ID = gETStatusCheckURL.RechargeAPI.aPIDetail.ID
                    };
                    rechargeAPIHit.Response = dMRTransactionResponse.Response;
                    _res.Statuscode = gETStatusCheckURL.Statuscode;
                    _res.Msg = TID + "" + (char)160 + gETStatusCheckURL.TransactionID + (char)160 + rechargeAPIHit.aPIDetail.URL + (char)160 + rechargeAPIHit.Response;
                }
                else if (gETStatusCheckURL.RechargeAPI.aPIDetail.APICode == APICode.FINODIN && (gETStatusCheckURL.RechargeAPI.aPIDetail.IsStatusBulkCheck && FromBulk || !FromBulk))
                {
                    var _apiID = gETStatusCheckURL.RechargeAPI.aPIDetail.ID;
                    var _apiGroupCode = gETStatusCheckURL.RechargeAPI.aPIDetail.GroupCode;
                    var moneyTransferAPIML = new FinodinML(_accessor, _env, _dal, APICode.FINODIN, _apiID, _apiGroupCode);
                    
                    var dMRTransactionResponse = moneyTransferAPIML.GetTransactionStatus(gETStatusCheckURL.TID, gETStatusCheckURL.UserID, gETStatusCheckURL.RequestMode, gETStatusCheckURL.APIOPCode, gETStatusCheckURL.APIOutletID);
                    transactionStatus.Status = dMRTransactionResponse.Statuscode;
                    transactionStatus.OperatorID = dMRTransactionResponse.LiveID;
                    rechargeAPIHit.aPIDetail = new APIDetail
                    {
                        URL = dMRTransactionResponse.Request,
                        ID = gETStatusCheckURL.RechargeAPI.aPIDetail.ID
                    };
                    rechargeAPIHit.Response = dMRTransactionResponse.Response;
                    _res.Statuscode = gETStatusCheckURL.Statuscode;
                    _res.Msg = TID + "" + (char)160 + gETStatusCheckURL.TransactionID + (char)160 + rechargeAPIHit.aPIDetail.URL + (char)160 + rechargeAPIHit.Response;
                }



                if (rechargeAPIHit.aPIDetail == null)
                {
                    _res.Msg = "No API Found";
                    return _res;
                }
                var typeMonthYear = new TypeMonthYear();
                if (TransDate != string.Empty)
                {
                    typeMonthYear = ConnectionStringHelper.O.GetTypeMonthYear(Convert.ToDateTime(TransDate.Replace(" ", "/")));
                }
                var transactionHelper = new TransactionHelper(_dal, _accessor, _env);
                await transactionHelper.UpdateAPIResponse(TID, rechargeAPIHit).ConfigureAwait(false);
                if (transactionStatus.Status.In(RechargeRespType.SUCCESS, RechargeRespType.FAILED))
                {
                    var callbackData = new _CallbackData
                    {
                        TID = TID,
                        TransactionID = gETStatusCheckURL.TransactionID,
                        Msg = transactionStatus.OperatorID,
                        RequestPage = "StatusCheck",
                        RequestIP = _rinfo.GetRemoteIP(),
                        Browser = _rinfo.GetBrowserFullInfo(),
                        LoginID = _lr.UserID,
                        TransactionStatus = transactionStatus.Status,
                        VendorID = transactionStatus.VendorID,
                        LiveID = transactionStatus.OperatorID,
                        LT = _lr.LoginTypeID
                    };
                    var updateRes = await UpdateTransactionStatus(callbackData).ConfigureAwait(false);
                    _res.Statuscode = updateRes.Statuscode;
                    _res.Msg = updateRes.Msg;
                    if (transactionStatus.Status == RechargeRespType.SUCCESS)
                    {

                        //Only for Internal Sender
                        new AlertML(_accessor, _env, false).PayoutSMS(new AlertReplacementModel
                        {
                            LoginID = gETStatusCheckURL.UserID,
                            UserMobileNo = gETStatusCheckURL.Optional2,
                            WID = 1,
                            Amount = Convert.ToInt32(gETStatusCheckURL.AmountR),
                            AccountNo = gETStatusCheckURL.Account ?? string.Empty,
                            SenderName = gETStatusCheckURL.SenderName ?? string.Empty,
                            TransMode = gETStatusCheckURL.Optional4,
                            UTRorRRN = transactionStatus.OperatorID ?? string.Empty,
                            IFSC = gETStatusCheckURL.Optional3 ?? string.Empty,
                            BrandName = gETStatusCheckURL.BrandName ?? string.Empty
                        });
                    }
                }
            }
            return _res;
        }
        private void ReplaceTDate(GETStatusCheckURL gETStatusCheckURL)
        {
            string dt = string.Empty;
            if (gETStatusCheckURL.RechargeAPI.aPIDetail.DFormatID == 1)
            {
                dt = DateTime.Parse(gETStatusCheckURL.TDate).ToString(DATEFormatType.DF1);
            }
            if (gETStatusCheckURL.RechargeAPI.aPIDetail.DFormatID == 2)
            {
                dt = DateTime.Parse(gETStatusCheckURL.TDate).ToString(DATEFormatType.DF2);
            }
            if (gETStatusCheckURL.RechargeAPI.aPIDetail.DFormatID == 3)
            {
                dt = DateTime.Parse(gETStatusCheckURL.TDate).ToString(DATEFormatType.DF3);
            }
            if (gETStatusCheckURL.RechargeAPI.aPIDetail.DFormatID == 4)
            {
                dt = DateTime.Parse(gETStatusCheckURL.TDate).ToString(DATEFormatType.DF4);
            }
            if (gETStatusCheckURL.RechargeAPI.aPIDetail.DFormatID == 5)
            {
                dt = DateTime.Parse(gETStatusCheckURL.TDate).ToString(DATEFormatType.DF5);
            }
            if (gETStatusCheckURL.RechargeAPI.aPIDetail.DFormatID == 6)
            {
                dt = DateTime.Parse(gETStatusCheckURL.TDate).ToString(DATEFormatType.DF6);
            }
            if (gETStatusCheckURL.RechargeAPI.aPIDetail.DFormatID == 7)
            {
                dt = DateTime.Parse(gETStatusCheckURL.TDate).ToString(DATEFormatType.DF7);
            }
            gETStatusCheckURL.RechargeAPI.aPIDetail.URL = gETStatusCheckURL.RechargeAPI.aPIDetail.URL.Replace(Replacement.DATE, dt);
        }
        public async Task<IResponseStatus> ResendTransactionsAsync(string TIDs, int APIID)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (((_lr.RoleID == Role.Admin || (_lr.IsWLAPIAllowed && ApplicationSetting.IsWLAPIAllowed)) && _lr.LoginTypeID == LoginType.ApplicationUser) || userML.IsCustomerCareAuthorised(ActionCodes.PendingResend))
            {
                var _req = new CommonReq
                {
                    LoginTypeID = _lr.LoginTypeID,
                    LoginID = _lr.UserID,
                    CommonInt = APIID,
                    str = TIDs,
                    CommonStr = _rinfo.GetRemoteIP(),
                    CommonStr2 = _rinfo.GetBrowserFullInfo()
                };

                IProcedureAsync _proc = new ProcPrepareResendTransactions(_dal);
                var resendableTransactions = (List<PrepairedTransactionReq>)await _proc.Call(_req).ConfigureAwait(false);
                if (resendableTransactions.Count > 0)
                {
                    _res.Statuscode = resendableTransactions[0].Statuscode;
                    _res.Msg = resendableTransactions[0].Msg;
                    if (resendableTransactions[0].Statuscode == ErrorCodes.Minus1)
                    {
                        return _res;
                    }
                    _res.Statuscode = ErrorCodes.One;
                    _res.Msg = "All selected transaction has been sent to " + (resendableTransactions[0].aPIDetail.Name ?? "") + " successfully!";
                    if (resendableTransactions[0].aPIDetail.APIType != APITypes.Lapu)
                    {
                        foreach (PrepairedTransactionReq resendTransaction in resendableTransactions)
                        {
                            TransactionHelper transactionHelper = new TransactionHelper(_dal, _accessor, _env);
                            var TransRequest = new TransactionServiceReq
                            {
                                OID = resendTransaction.OID,
                                AccountNo = resendTransaction.AccountNo,
                                AmountR = resendTransaction.RequestedAmount,
                                Optional1 = resendTransaction.Optional1,
                                Optional2 = resendTransaction.Optional2,
                                Optional3 = resendTransaction.Optional3,
                                Optional4 = resendTransaction.Optional4,
                                UserID = resendTransaction.UserID,
                                OPID = "",
                                APIRequestID = "",
                                CircleID = -1,
                                Extra = "",
                                PAN = resendTransaction.PAN,
                                Aadhar = resendTransaction.Aadhar
                            };
                            TransactionStatus tstatus = await transactionHelper.HitGetStatus(resendTransaction.TID, TransRequest, resendTransaction.aPIDetail, null, true, new TransactionServiceResp { }).ConfigureAwait(false);
                            tstatus.TID = resendTransaction.TID;
                            tstatus.UserID = resendTransaction.UserID;

                            #region UpdateTransactionSatus
                            IProcedureAsync _updateProc = new ProcUpdateTransactionServiceStatus(_dal);
                            var callbackData = (_CallbackData)await _updateProc.Call(tstatus).ConfigureAwait(false);
                            if (callbackData.Statuscode == ErrorCodes.One && !string.IsNullOrEmpty(callbackData.UpdateUrl))
                            {
                                ICallbackML callbackML = new CallbackML(_accessor, _env);
                                var _ = callbackML.UpdateAPIURLHitting(callbackData);
                            }
                            #endregion
                            if (tstatus.Status == RechargeRespType.SUCCESS && resendTransaction.IsBBPS && string.IsNullOrEmpty(resendTransaction.CustomerNumber))
                            {
                                var alertParam = new AlertReplacementModel
                                {
                                    WID = resendTransaction.WID,
                                    AccountNo = resendTransaction.AccountNo,
                                    UserMobileNo = resendTransaction.CustomerNumber,
                                    Amount = resendTransaction.RequestedAmount,
                                    Operator = resendTransaction.Operator,
                                    TransactionID = resendTransaction.TransactionID,
                                    FormatID = MessageFormat.BBPSSuccess,
                                    DATETIME = DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt")
                                };
                                IAlertML alertMl = new AlertML(_accessor, _env);
                                alertMl.BBPSSuccessSMS(alertParam);
                            }
                        }
                    }
                }
            }
            return _res;
        }
        public async Task<IResponseStatus> ResendTransactionAsync(int TID, int APIID)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if ((_lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser) || userML.IsCustomerCareAuthorised(ActionCodes.PendingResend))
            {
                var _req = new CommonReq
                {
                    LoginTypeID = _lr.LoginTypeID,
                    LoginID = _lr.UserID,
                    CommonInt = APIID,
                    CommonInt2 = TID,
                    CommonStr = _rinfo.GetRemoteIP(),
                    CommonStr2 = _rinfo.GetBrowserFullInfo()
                };
                IProcedureAsync _proc = new ProcPrepareResendTransaction(_dal);
                var resendableTransactions = (PrepairedTransactionReq)await _proc.Call(_req).ConfigureAwait(false);
                _res.Statuscode = resendableTransactions.Statuscode;
                _res.Msg = resendableTransactions.Msg;
                if (_res.Statuscode == ErrorCodes.One)
                {
                    _res.Msg = "Resend Success";
                    if (resendableTransactions.aPIDetail.APIType != APITypes.Lapu)
                    {
                        var transactionHelper = new TransactionHelper(_dal, _accessor, _env);
                        var TransRequest = new TransactionServiceReq
                        {
                            OID = resendableTransactions.OID,
                            AccountNo = resendableTransactions.AccountNo,
                            AmountR = resendableTransactions.RequestedAmount,
                            Optional1 = resendableTransactions.Optional1,
                            Optional2 = resendableTransactions.Optional2,
                            Optional3 = resendableTransactions.Optional3,
                            Optional4 = resendableTransactions.Optional4,
                            UserID = resendableTransactions.UserID,

                            OPID = string.Empty,
                            APIRequestID = string.Empty,
                            CircleID = -1,
                            Extra = string.Empty,
                            PAN = resendableTransactions.PAN,
                            Aadhar = resendableTransactions.Aadhar
                        };
                        var tstatus = await transactionHelper.HitGetStatus(resendableTransactions.TID, TransRequest, resendableTransactions.aPIDetail, null, true, new TransactionServiceResp { }).ConfigureAwait(false);
                        tstatus.TID = resendableTransactions.TID;
                        tstatus.UserID = resendableTransactions.UserID;
                        #region UpdateTransactionSatus
                        IProcedureAsync _updateProc = new ProcUpdateTransactionServiceStatus(_dal);
                        var callbackData = (_CallbackData)await _updateProc.Call(tstatus).ConfigureAwait(false);
                        if (callbackData.Statuscode == ErrorCodes.One)
                        {
                            if (callbackData.RequestMode == RequestMode.API && !string.IsNullOrEmpty(callbackData.UpdateUrl))
                            {
                                ICallbackML callbackML = new CallbackML(_accessor, _env);
                                var _ = callbackML.UpdateAPIURLHitting(callbackData);
                            }
                            else if (callbackData.RequestMode == RequestMode.APPS)
                            {
                                #region SEND_Notification
                                if (tstatus.Status == RechargeRespType.SUCCESS || tstatus.Status == RechargeRespType.FAILED)
                                {
                                    bool IsSuccess = false;
                                    IAlertML alertMl = new AlertML(_accessor, _env);
                                    if (tstatus.Status == RechargeRespType.SUCCESS)
                                    {
                                        IsSuccess = true;
                                    }
                                    var alertParam = new AlertReplacementModel
                                    {
                                        AccountNo = callbackData.AccountNo,
                                        UserMobileNo = callbackData.MobileNo,
                                        Amount = callbackData.RequestedAmount,
                                        LiveID = callbackData.LiveID,
                                        WID = callbackData.WID,
                                        UserFCMID = callbackData.FCMID,
                                        LoginID = _req.LoginID,
                                        TransactionID = callbackData.TransactionID,
                                        LoginCurrentBalance = callbackData.Balance,
                                        Company = callbackData.Company,
                                        CompanyDomain = callbackData.CompanyDomain,
                                        SupportEmail = callbackData.SupportEmail,
                                        SupportNumber = callbackData.SupportNumber,
                                        AccountEmail = callbackData.AccountEmail,
                                        AccountsContactNo = callbackData.AccountContact,
                                        UserID = callbackData.UserID,
                                        BalanceAmount = Convert.ToString(callbackData.Balance)
                                    };
                                    alertMl.RecharegeSuccessNotification(alertParam, IsSuccess);
                                }
                                #endregion
                            }
                        }
                        #endregion
                        if (tstatus.Status == RechargeRespType.SUCCESS && resendableTransactions.IsBBPS && string.IsNullOrEmpty(resendableTransactions.CustomerNumber))
                        {
                            var alertParam = new AlertReplacementModel
                            {
                                WID = resendableTransactions.WID,
                                AccountNo = resendableTransactions.AccountNo,
                                UserMobileNo = resendableTransactions.CustomerNumber,
                                Amount = resendableTransactions.RequestedAmount,
                                Operator = resendableTransactions.Operator,
                                TransactionID = resendableTransactions.TransactionID,
                                FormatID = MessageFormat.BBPSSuccess,
                                DATETIME = DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt")
                            };
                            IAlertML alertMl = new AlertML(_accessor, _env);
                            alertMl.BBPSSuccessSMS(alertParam);
                        }
                    }
                }
            }
            return _res;
        }

        public async Task<IResponseStatus> ResendTransactionsAsync(string TIDs)
        {
            //MoneyTransfer
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if ((_lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser) || userML.IsCustomerCareAuthorised(ActionCodes.PendingResend))
            {
                var _req = new CommonReq
                {
                    LoginTypeID = _lr.LoginTypeID,
                    LoginID = _lr.UserID,
                    str = TIDs,
                    CommonStr = _rinfo.GetRemoteIP(),
                    CommonStr2 = _rinfo.GetBrowserFullInfo()
                };
                IProcedureAsync _proc = new ProcPrepareResendDMRTransactions(_dal);
                var resendableTransactions = (List<PrepairedTransactionReq>)await _proc.Call(_req);
                if (resendableTransactions.Count > 0)
                {
                    _res.Statuscode = resendableTransactions[0].Statuscode;
                    _res.Msg = resendableTransactions[0].Msg;
                    if (_res.Statuscode == ErrorCodes.Minus1)
                    {
                        return _res;
                    }

                    StringBuilder sb = new StringBuilder();
                    DMTResponse dMTResponse;
                    //MoneyTransferOperation moneyTransferOperation = new MoneyTransferOperation(_accessor, _env);

                    //foreach (PrepairedTransactionReq resendTransaction in resendableTransactions)
                    //{
                    //    TransactionStatus transactionStatus = new TransactionStatus
                    //    {
                    //        UserID = resendTransaction.UserID,
                    //        APIID = resendTransaction.APIID,
                    //        TID = resendTransaction.TID,
                    //        Status = RechargeRespType.PENDING
                    //    };
                    //    dMTResponse = new DMTResponse
                    //    {
                    //        STATUS = RechargeRespType._PENDING,
                    //        MSG = "Request could not be completed!"
                    //    };
                    //    dMTResponse = await moneyTransferOperation.TransferFundHelper(resendTransaction, transactionStatus, dMTResponse);
                    //    sb.AppendFormat("For TID:{0} | NewStatus:{1} | Msg:{2} | LiveID:{3} <br/>", transactionStatus.TID + "", dMTResponse.STATUS, dMTResponse.MSG, dMTResponse.OPID);
                    //}
                    //_res.Description = sb.ToString();
                }
            }
            return _res;
        }

        #endregion

        #region SendSMSReport
        public IEnumerable<SentSmsResponse> GetSentSMSreport(SentSMSRequest req)
        {
            var _res = new List<SentSmsResponse>();
            if (_lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.ShowSendSMS))
            {
                req.LoginID = _lr.UserID;
                IProcedure _proc = new ProcGetSentSMS(_dal);
                _res = (List<SentSmsResponse>)_proc.Call(req);
            }
            return _res;
        }
        #endregion

        #region LedgerSection
        public List<ProcAdminLedgerResponse> GetAdminLedgerList(ALedgerReportFilter filter)
        {
            var _lst = new List<ProcAdminLedgerResponse>();
            var _res = new ProcAdminLedgerResponse();
            if (_lr.LoginTypeID == LoginType.ApplicationUser && _lr.RoleID == Role.Admin || userML.IsCustomerCareAuthorised(ActionCodes.ShowAdminLedger))
            {
                var validate = Validate.O;
                if (!string.IsNullOrEmpty(filter.Mobile_F))
                {
                    if (!validate.IsMobile(filter.Mobile_F))
                        return _lst;
                }
                var _req = new ProcAdminLedgerRequest
                {
                    LT = _lr.LoginTypeID,
                    LoginID = _lr.UserID,
                    DebitCredit_F = filter.DebitCredit_F,
                    Service_F = filter.Service_F,
                    TopRows = filter.TopRows,
                    FromDate_F = filter.FromDate_F,
                    ToDate_F = filter.ToDate_F,
                    WalletTypeID = filter.WalletTypeID
                };
                if (filter.Criteria > 0)
                {
                    if ((filter.CriteriaText ?? "") == "")
                    {
                        return _lst;
                    }
                }
                if (filter.Criteria == Criteria.OutletMobile)
                {
                    if (!validate.IsMobile(filter.CriteriaText))
                    {
                        return _lst;
                    }
                    _req.Mobile_F = filter.CriteriaText;
                }

                if (filter.Criteria == Criteria.TransactionID)
                {
                    if (filter.CriteriaText[0] != Criteria.StartCharOFTransaction)
                    {
                        return _lst;
                    }
                    _req.TransactionId_F = filter.CriteriaText;
                }
                if (filter.Criteria == Criteria.CCID)
                {
                    if (!Validate.O.IsNumeric(filter.CriteriaText))
                    {
                        return _lst;
                    }
                    if (Validate.O.IsMobile(filter.CriteriaText))
                    {
                        return _lst;
                    }
                    _req.CCID = Convert.ToInt32(filter.CriteriaText);
                }
                if (filter.Criteria == Criteria.CCMobileNo)
                {
                    _req.CCMobileNo = filter.CriteriaText;
                }
                if (filter.Criteria == Criteria.UTR)
                {
                    _req.UTR_F = filter.CriteriaText;
                }
                if (filter.Criteria == Criteria.UserID)
                {
                    var Prefix = Validate.O.Prefix(filter.CriteriaText);
                    if (Validate.O.IsNumeric(Prefix))
                        _req.UserID = Validate.O.IsNumeric(filter.CriteriaText) ? Convert.ToInt32(filter.CriteriaText) : _req.UserID;
                    var uid = Validate.O.LoginID(filter.CriteriaText);
                    _req.UserID = Validate.O.IsNumeric(uid) ? Convert.ToInt32(uid) : _req.UserID;
                }
                _req.FromDate_F = string.IsNullOrEmpty(_req.FromDate_F) ? DateTime.Now.ToString("dd MMM yyyy") : _req.FromDate_F;
                _dal = ChangeConString(_req.FromDate_F);
                IProcedure _proc = new ProcAdminLedger(_dal);
                _lst = (List<ProcAdminLedgerResponse>)_proc.Call(_req);
            }
            return _lst;
        }
        public async Task<List<ProcUserLedgerResponse>> GetUserLedgerList(ULedgerReportFilter filter)
        {
            var _lst = new List<ProcUserLedgerResponse>();
            var loginResp = chkAlternateSession();
            if (loginResp != null)
            {
                if (loginResp.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.ShowUserLedger) || loginResp.LoginTypeID == LoginType.Employee)
                {
                    var validate = Validate.O;
                    if ((filter.TransactionId_F ?? "") != "" && !validate.IsTransactionIDValid((filter.TransactionId_F ?? "")))
                    {
                        return _lst;
                    }
                    var _req = new ProcUserLedgerRequest
                    {
                        LT = loginResp.LoginTypeID,
                        LoginID = loginResp.UserID,
                        Mobile_F = filter.Mobile_F ?? "",
                        DebitCredit_F = filter.DebitCredit_F,
                        FromDate_F = !string.IsNullOrEmpty(filter.FromDate_F) ? filter.FromDate_F : null,
                        ToDate_F = !string.IsNullOrEmpty(filter.ToDate_F) ? filter.ToDate_F : null,
                        TransactionId_F = filter.TransactionId_F,
                        TopRows = filter.TopRows,
                        WalletTypeID = filter.WalletTypeID
                    };

                    if (filter.Mobile_F != "")
                    {
                        if (!Validate.O.IsMobile(filter.Mobile_F))
                        {
                            var Prefix = Validate.O.Prefix(filter.Mobile_F);
                            if (Validate.O.IsNumeric(Prefix))
                                _req.UserID = Validate.O.IsNumeric(filter.Mobile_F) ? Convert.ToInt32(filter.Mobile_F) : _req.UserID;
                            var uid = Validate.O.LoginID(filter.Mobile_F);
                            _req.UserID = Validate.O.IsNumeric(uid) ? Convert.ToInt32(uid) : _req.UserID;
                            _req.Mobile_F = "";
                        }
                    }
                    _req.FromDate_F = string.IsNullOrEmpty(_req.FromDate_F) ? DateTime.Now.ToString("dd MMM yyyy") : _req.FromDate_F;
                    _dal = ChangeConString(_req.FromDate_F);
                    IProcedureAsync _proc = new ProcUserLedger(_dal);
                    _lst = (List<ProcUserLedgerResponse>)await _proc.Call(_req);
                }
            }
            return _lst;
        }

        #region AppUserLedgerSection
        public async Task<IEnumerable<ProcUserLedgerResponse>> GetAppUserLedgerList(ProcUserLedgerRequest filter)
        {
            var _lst = new List<ProcUserLedgerResponse>();
            var _res = new ProcUserLedgerResponse();
            Validate validate = Validate.O;
            if ((filter.TransactionId_F ?? "") != "" && !validate.IsTransactionIDValid((filter.TransactionId_F ?? "")))
            {
                return _lst;
            }
            if (filter.Mobile_F != "")
            {
                if (!Validate.O.IsMobile(filter.Mobile_F))
                {
                    var Prefix = Validate.O.Prefix(filter.Mobile_F);
                    if (Validate.O.IsNumeric(Prefix))
                        filter.UserID = Validate.O.IsNumeric(filter.Mobile_F) ? Convert.ToInt32(filter.Mobile_F) : filter.UserID;
                    var uid = Validate.O.LoginID(filter.Mobile_F);
                    filter.UserID = Validate.O.IsNumeric(uid) ? Convert.ToInt32(uid) : filter.UserID;
                    filter.Mobile_F = "";
                }
            }
            filter.FromDate_F = string.IsNullOrEmpty(filter.FromDate_F) ? DateTime.Now.ToString("dd MMM yyyy") : filter.FromDate_F;
            filter.ToDate_F = string.IsNullOrEmpty(filter.ToDate_F) ? DateTime.Now.ToString("dd MMM yyyy") : filter.ToDate_F;
            _dal = ChangeConString(filter.FromDate_F);
            IProcedureAsync _proc = new ProcUserLedger(_dal);
            _lst = (List<ProcUserLedgerResponse>)await _proc.Call(filter);
            return _lst;
        }
        #endregion
        #endregion

        #region TargetReport
        public async Task<IEnumerable<TargetAchieved>> GetTargetAchieveds(CommonReq commonReq)
        {
            var targetAchieved = new List<TargetAchieved>();
            IProcedureAsync proc = new ProcGetTargetAchievedTillDate(_dal);
            targetAchieved = (List<TargetAchieved>)await proc.Call(commonReq);
            return targetAchieved;
        }
        #endregion

        public List<DashboardData> TotalBalRoleWise()
        {
            IProcedure f = new ProcGetBalnaceRoleWise(_dal);
            return (List<DashboardData>)f.Call();
        }
        public AccountSummary GetAccountSummary(ULedgerReportFilter filter)
        {
            var _res = new AccountSummary
            {
                StatusCode = ErrorCodes.Minus1,
                Status = ErrorCodes.AuthError
            };

            var loginResp = chkAlternateSession();
            if (loginResp != null)
            {
                if (loginResp.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.ShowUserLedger))
                {
                    var _req = new ProcUserLedgerRequest
                    {
                        LT = loginResp.LoginTypeID,
                        LoginID = loginResp.UserID,
                        Mobile_F = filter.Mobile_F,
                        FromDate_F = filter.FromDate_F,
                        ToDate_F = filter.ToDate_F,
                        WalletTypeID = filter.WalletTypeID
                    };
                    if (filter.Mobile_F != "")
                    {
                        if (!Validate.O.IsMobile(filter.Mobile_F))
                        {
                            var Prefix = Validate.O.Prefix(filter.Mobile_F);
                            if (Validate.O.IsNumeric(Prefix))
                                _req.UserID = Validate.O.IsNumeric(filter.Mobile_F) ? Convert.ToInt32(filter.Mobile_F) : _req.UserID;
                            var uid = Validate.O.LoginID(filter.Mobile_F);
                            _req.UserID = Validate.O.IsNumeric(uid) ? Convert.ToInt32(uid) : _req.UserID;
                            _req.Mobile_F = "";
                        }
                    }
                    _req.FromDate_F = string.IsNullOrEmpty(_req.FromDate_F) ? DateTime.Now.ToString("dd MMM yyyy") : _req.FromDate_F;
                    _dal = ChangeConString(_req.FromDate_F);
                    IProcedure _proc = new ProcAccountSummary(_dal);
                    _res = (AccountSummary)_proc.Call(_req);
                }
            }
            return _res;
        }
        public AccountSummary GetAccountSummaryDashboard()
        {
            var _res = new AccountSummary
            {
                StatusCode = ErrorCodes.Minus1,
                Status = ErrorCodes.AuthError
            };
            if (_lr.LoginTypeID == LoginType.ApplicationUser && _lr.RoleID != Role.Admin)
            {
                var _req = new ProcUserLedgerRequest
                {
                    LT = _lr.LoginTypeID,
                    LoginID = _lr.UserID
                };
                IProcedure _proc = new ProcAccountSummaryDashboard(_dal);
                _res = (AccountSummary)_proc.Call(_req);
            }
            return _res;
        }

        public Dashboard_Chart TotalPending()
        {
            IProcedure f = new procTotalPending(_dal);
            return (Dashboard_Chart)f.Call();
        }

        public AccountSummary AdminAccountSummary()
        {
            IProcedure f = new ProcAdminAccountSummary(_dal);
            return (AccountSummary)f.Call();
        }

        public int FundCount(int userID)
        {
            IProcedure f = new ProcFundCount(_dal);
            return (int)f.Call(userID);
        }

        public async Task<PendingTransactios> PendingTransaction(int APIID, int OID, int RepType, string SenderNo = "")
        {
            PendingTransactios transactios = new PendingTransactios();
            if ((_lr.RoleID == Role.Admin || (_lr.IsWLAPIAllowed && ApplicationSetting.IsWLAPIAllowed)) && _lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.ShowPending))
            {
                var req = new CommonReq
                {
                    LoginTypeID = _lr.LoginTypeID,
                    LoginID = _lr.UserID,
                    CommonInt = APIID,
                    CommonInt2 = OID,
                    CommonInt3 = RepType,
                    CommonStr = SenderNo
                };
                IProcedureAsync _proc = new ProcGetPendingTransactions(_dal);
                var pendings = (List<PendingTransaction>)await _proc.Call(req).ConfigureAwait(false);
                transactios.Pendings = pendings;
                if (pendings.Count > 0 && RepType == ReportType.Recharge)
                {
                    var pendingCount = pendings.GroupBy(x => new { x.APIID, x.APIName, x.APIType })
                    .Select(g => new PendingCountAPIWise { APIID = g.Key.APIID, APIType = g.Key.APIType, APIName = g.Key.APIName, Count = g.Count() }).ToList();
                    transactios.PendingAPI = pendingCount;
                }
            }
            return transactios;
        }
        public async Task<IEnumerable<ResponseStatus>> PendingTIDTransID(int RepType)
        {
            var res = new List<ResponseStatus>();
            PendingTransactios transactios = new PendingTransactios();
            if (_lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.ShowPending))
            {
                var req = new CommonReq
                {
                    LoginTypeID = _lr.LoginTypeID,
                    LoginID = _lr.UserID,
                    CommonInt = 0,
                    CommonInt2 = 0,
                    CommonInt3 = RepType,
                    CommonStr = ""
                };
                IProcedureAsync _proc = new ProcGetPendingTransactions(_dal);
                var pendings = (List<PendingTransaction>)await _proc.Call(req).ConfigureAwait(false);
                if (pendings != null)
                {
                    if (pendings.Count > 0)
                    {
                        res = pendings.Select(x => new ResponseStatus { CommonInt = x.TID, CommonStr = x.TransactionID }).ToList();
                    }
                }
            }
            return res;
        }
        #region PESTransactions
        public async Task<PendingTransactios> PESTransaction(int OID)
        {
            PendingTransactios transactios = new PendingTransactios();
            if (_lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.ShowPending))
            {
                var req = new CommonReq
                {
                    LoginTypeID = _lr.LoginTypeID,
                    LoginID = _lr.UserID,
                    CommonInt2 = OID,
                };
                IProcedureAsync _proc = new ProcPESPendingTran(_dal);
                var pendings = (List<PendingTransaction>)await _proc.Call(req);
                transactios.Pendings = pendings;
                if (pendings.Count > 0)
                {
                    var pendingCount = pendings.GroupBy(x => new { x.APIID, x.APIName })
                    .Select(g => new PendingCountAPIWise { APIID = g.Key.APIID, APIName = g.Key.APIName, Count = g.Count() }).ToList();
                    transactios.PendingAPI = pendingCount;
                }
            }
            return transactios;
        }
        public List<PESApprovedDocument> GetPESApprovedDocument(int id)
        {
            var _resp = new List<PESApprovedDocument>();

            string _pesDocPath = DOCType.PESDocument + id;
            var s = Directory.GetFiles(_pesDocPath);
            foreach (var item in s)
            {

                var data = new PESApprovedDocument
                {
                    PESImage = item.ToString()
                };
                _resp.Add(data);
            }
            return _resp;
        }
        public async Task<IEnumerable<PESReportViewModel>> GetPESDetails(int TID, string TransactionID)
        {
            var _lst = new List<PESReportViewModel>();
            if (_lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.ShowRechargeReport))
            {
                var req = new CommonReq
                {
                    LoginTypeID = _lr.LoginTypeID,
                    LoginID = _lr.UserID,
                    CommonInt = TID
                };
                string TransDate = "";
                if (Validate.O.IsTransactionIDValid(TransactionID))
                {
                    TransDate = ConnectionStringHelper.O.ConvertTransactionIDTo_dd_MMM_yyyy(TransactionID);
                    _dal = ChangeConString(TransDate);
                }
                IProcedureAsync _proc = new ProcGetPESDetails(_dal);
                _lst = (List<PESReportViewModel>)await _proc.Call(req);
            }
            return _lst;
        }
        #endregion

        public IResponseStatus UpdateRequestSent(int APIID)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (((_lr.RoleID == Role.Admin || (_lr.IsWLAPIAllowed && ApplicationSetting.IsWLAPIAllowed)) && _lr.LoginTypeID == LoginType.ApplicationUser) || userML.IsCustomerCareAuthorised(ActionCodes.PendingResend))
            {
                var _req = new CommonReq
                {
                    LoginTypeID = _lr.LoginTypeID,
                    LoginID = _lr.UserID,
                    CommonInt = APIID,
                    CommonStr = _rinfo.GetRemoteIP(),
                    CommonStr2 = _rinfo.GetBrowser()
                };
                IProcedure _proc = new ProcUpdateRequestSent(_dal);
                _res = (ResponseStatus)_proc.Call(_req);
            }
            return _res;
        }

        #region EndUsersOnly
        public List<ProcFundReceiveStatementResponse> GetUserFundReceive(ULFundReceiveReportFilter filter)
        {
            var loginRes = chkAlternateSession();
            var _lst = new List<ProcFundReceiveStatementResponse>();
            var _res = new ProcFundReceiveStatementResponse();
            if (loginRes.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.ShowFundDebitCredit) || loginRes.LoginTypeID == LoginType.Employee)
            {
                Validate validate = Validate.O;
                var item = new ULFundReceiveReportFilter()
                {
                    LoginId = loginRes.UserID,
                    LT = loginRes.LoginTypeID,
                    FDate = filter.FDate ?? DateTime.Now.ToString("dd MMM yyyy"),
                    TDate = filter.TDate ?? DateTime.Now.ToString("dd MMM yyyy"),
                    TID = filter.TID,
                    ServiceID = filter.ServiceID,
                    MobileNo = filter.MobileNo ?? string.Empty,
                    IsSelf = filter.IsSelf,
                    WalletTypeID = filter.WalletTypeID,
                    OtherUserMob = filter.OtherUserMob ?? ""
                };
                item.UserID = GetUserIDFromPrefixedID(item.MobileNo);
                if (item.UserID > 0)
                {
                    item.MobileNo = "";
                }
                item.OtherUserID = GetUserIDFromPrefixedID(item.OtherUserMob);
                if (item.OtherUserID > 0)
                {
                    item.OtherUserMob = "";
                }
                _dal = ChangeConString(item.FDate);
                IProcedure _proc = new ProcFundReceiveStatement(_dal);
                _lst = (List<ProcFundReceiveStatementResponse>)_proc.Call(item);
            }
            return _lst;
        }
        public async Task<IEnumerable<ProcFundReceiveStatementResponse>> GetAppUserFundReceive(ULFundReceiveReportFilter filter)
        {
            filter.UserID = GetUserIDFromPrefixedID(filter.MobileNo);
            if (filter.UserID > 0)
            {
                filter.MobileNo = "";
            }
            filter.OtherUserID = GetUserIDFromPrefixedID(filter.OtherUserMob);
            if (filter.OtherUserID > 0)
            {
                filter.OtherUserMob = "";
            }
            var _lst = new List<ProcFundReceiveStatementResponse>();
            Validate validate = Validate.O;
            filter.FDate = filter.FDate ?? DateTime.Now.ToString("dd MMM yyyy");
            _dal = ChangeConString(filter.FDate);
            IProcedure _proc = new ProcFundReceiveStatement(_dal);
            _lst = (List<ProcFundReceiveStatementResponse>)_proc.Call(filter);
            await Task.Delay(0);
            return _lst;
        }
        public ProcFundReceiveInvoiceResponse GetUserFundReceiveInvoice(string TransactionID)
        {
            var _res = new ProcFundReceiveInvoiceResponse();

            if (_lr.LoginTypeID == LoginType.ApplicationUser)
            {
                string TransDate = "";
                if (Validate.O.IsTransactionIDValid(TransactionID))
                {
                    TransDate = ConnectionStringHelper.O.ConvertTransactionIDTo_dd_MMM_yyyy(TransactionID);
                    _dal = ChangeConString(TransDate);
                }
                var req = new CommonReq
                {
                    LoginID = _lr.UserID,
                    CommonStr = TransactionID
                };
                IProcedure _proc = new ProcFundReceiveInvoice(_dal);
                _res = (ProcFundReceiveInvoiceResponse)_proc.Call(req);
            }


            return _res;
        }
        public List<FundRequetResp> GetUserFundReport(FundOrderFilter fundOrderFilter)
        {
            var _lst = new List<FundRequetResp>();
            if (_lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.ShowFundRequestHistory))
            {
                fundOrderFilter.LT = _lr.LoginTypeID;
                fundOrderFilter.LoginID = _lr.UserID;
                fundOrderFilter.FromDate = fundOrderFilter.FromDate ?? DateTime.Now.ToString("dd MMM yyyy");
                fundOrderFilter.ToDate = fundOrderFilter.ToDate ?? DateTime.Now.ToString("dd MMM yyyy");
                if (fundOrderFilter.Criteria > 0)
                {
                    if ((fundOrderFilter.CriteriaText ?? "") == "")
                    {
                        return _lst;
                    }
                }
                if (fundOrderFilter.Criteria == Criteria.OutletMobile)
                {
                    if (!Validate.O.IsMobile(fundOrderFilter.CriteriaText))
                    {
                        return _lst;
                    }
                    fundOrderFilter.UMobile = fundOrderFilter.CriteriaText;
                }
                if (fundOrderFilter.Criteria == Criteria.TransactionID)
                {
                    fundOrderFilter.TransactionID = fundOrderFilter.CriteriaText;
                }
                if (fundOrderFilter.Criteria == Criteria.AccountNo)
                {
                    fundOrderFilter.AccountNo = fundOrderFilter.CriteriaText;
                }
                if (fundOrderFilter.Criteria == Criteria.CCID)
                {
                    fundOrderFilter.CCID = Convert.ToInt32(fundOrderFilter.CriteriaText);
                }
                if (fundOrderFilter.Criteria == Criteria.CCMobileNo)
                {
                    fundOrderFilter.CCMobileNo = fundOrderFilter.CriteriaText;
                }
                if (fundOrderFilter.Criteria == Criteria.UserID)
                {
                    var Prefix = Validate.O.Prefix(fundOrderFilter.CriteriaText);
                    if (Validate.O.IsNumeric(Prefix))
                        fundOrderFilter.UserID = Validate.O.IsNumeric(fundOrderFilter.CriteriaText) ? Convert.ToInt32(fundOrderFilter.CriteriaText) : fundOrderFilter.UserID;
                    var uid = Validate.O.LoginID(fundOrderFilter.CriteriaText);
                    fundOrderFilter.UserID = Validate.O.IsNumeric(uid) ? Convert.ToInt32(uid) : fundOrderFilter.UserID;
                }
                //if (fundOrderFilter.FromDate != DateTime.Now.ToString("dd MMM yyyy"))
                //    _dal = ChangeConString(fundOrderFilter.FromDate);
                IProcedure _proc = new ProcUserFundRequestReport(_dal);
                _lst = (List<FundRequetResp>)_proc.Call(fundOrderFilter);
            }
            return _lst;
        }

        public IEnumerable<FundRequetResp> GetUserFundReportApp(FundOrderFilter fundOrderFilter)
        {
            var _lst = new List<FundRequetResp>();
            if (fundOrderFilter.LT == LoginType.ApplicationUser)
            {
                fundOrderFilter.FromDate = fundOrderFilter.FromDate ?? DateTime.Now.ToString("dd MMM yyyy");
                fundOrderFilter.ToDate = fundOrderFilter.ToDate ?? DateTime.Now.ToString("dd MMM yyyy");
                //if (fundOrderFilter.FromDate != DateTime.Now.ToString("dd MMM yyyy"))
                //    _dal = ChangeConString(fundOrderFilter.FromDate);
                IProcedure _proc = new ProcUserFundRequestReport(_dal);
                _lst = (List<FundRequetResp>)_proc.Call(fundOrderFilter);
            }
            return _lst;
        }
        public IEnumerable<FundRequetResp> GetUserFundReportApproval()
        {
            var _lst = new List<FundRequetResp>();
            if (!_lr.RoleID.In(Role.Customer, Role.APIUser, Role.Retailor_Seller, Role.FOS) || userML.IsCustomerCareAuthorised(ActionCodes.ShowFundRequest))
            {
                var req = new CommonReq
                {
                    LoginTypeID = _lr.LoginTypeID,
                    LoginID = _lr.UserID
                };
                IProcedure _proc = new ProcGetFundRequestForApproval(_dal);
                _lst = (List<FundRequetResp>)_proc.Call(req);
            }
            return _lst;
        }
        public IEnumerable<FundRequetResp> GetUserFundReportApprovalApp(CommonReq commonReq)
        {
            IProcedure _proc = new ProcGetFundRequestForApproval(_dal);
            return (List<FundRequetResp>)_proc.Call(commonReq);
        }
        #endregion

        #region DaybookSection
        public List<Daybook> AdminDayBook(string FromDate, string ToDate, int APIID, int OID, string Mobile_F)
        {
            var _res = new List<Daybook>();
            if (_lr.RoleID == Role.Admin || userML.IsCustomerCareAuthorised(ActionCodes.ShowAdminDaybook))
            {
                var _req = new ForDateFilter
                {
                    LT = _lr.LoginTypeID,
                    LoginID = _lr.UserID,
                    FromDate = FromDate,
                    ToDate = ToDate,
                    EventID = APIID,
                    Top = OID,
                    UserMob = Mobile_F ?? ""
                };
                if (!string.IsNullOrEmpty(Mobile_F))
                {
                    if (!Validate.O.IsMobile(Mobile_F))
                    {
                        var Prefix = Validate.O.Prefix(Mobile_F);
                        if (Validate.O.IsNumeric(Prefix))
                            _req.UserID = Validate.O.IsNumeric(Mobile_F) ? Convert.ToInt32(Mobile_F) : _req.UserID;
                        var uid = Validate.O.LoginID(Mobile_F);
                        _req.UserID = Validate.O.IsNumeric(uid) ? Convert.ToInt32(uid) : _req.UserID;
                        _req.UserMob = "";
                    }
                }

                _dal = ChangeConString(_req.FromDate);
                IProcedure _proc = new ProcAPIDaybook(_dal);
                return (List<Daybook>)_proc.Call(_req);
            }
            return _res;
        }
        public IEnumerable<APIDaybookDatewise> AdminDayBookDateAPIwise(string FromDate, string ToDate, int APIID)
        {
            var _res = new List<APIDaybookDatewise>();
            if (_lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.ShowAdminDaybook))
            {
                var _req = new ForDateFilter
                {
                    LT = _lr.LoginTypeID,
                    LoginID = _lr.UserID,
                    FromDate = FromDate,
                    ToDate = ToDate,
                    EventID = APIID
                };
                _dal = ChangeConString(_req.FromDate);
                IProcedure _proc = new ProcAPIDaybookDatewise(_dal);
                var daybook = (List<Daybook>)_proc.Call(_req);
                if (daybook.Any())
                {
                    var Grouped = daybook.GroupBy(x => new { x.EntryDate }).Select(g => new APIDaybookDatewise { TDate = g.Key.EntryDate.ToString("dd MMM yyyy"), EntryDate = g.Key.EntryDate, TotalHits = g.Sum(s => s.TotalHits), TotalAmount = g.Sum(s => s.TotalAmount), SuccessHits = g.Sum(s => s.SuccessHits), SuccessAmount = g.Sum(s => s.SuccessAmount), RefundHits = g.Sum(s => s.RefundHits), RefundAmount = g.Sum(s => s.RefundAmount), FailedHits = g.Sum(s => s.FailedHits), FailedAmount = g.Sum(s => s.FailedAmount), PendingHits = g.Sum(s => s.PendingHits), PendingAmount = g.Sum(s => s.PendingAmount), APICommission = g.Sum(s => s.APICommission), Commission = g.Sum(s => s.Commission), TDSAmount = g.Sum(s => s.TDSAmount), Profit = g.Sum(s => s.Profit), TeamCommission = g.Sum(s => s.TeamCommission), Incentive = g.Sum(s => s.Incentive) }).OrderBy(x => x.EntryDate).ToList();
                    foreach (var item in Grouped)
                    {
                        item.Daybooks = daybook.Where(x => x.EntryDate == item.EntryDate).Select(g => new Daybook { Operator = g.Operator, TotalHits = g.TotalHits, TotalAmount = g.TotalAmount, SuccessHits = g.SuccessHits, SuccessAmount = g.SuccessAmount, RefundHits = g.RefundHits, RefundAmount = g.RefundAmount, FailedHits = g.FailedHits, FailedAmount = g.FailedAmount, PendingHits = g.PendingHits, PendingAmount = g.PendingAmount, APICommission = g.APICommission, Commission = g.Commission, TDSAmount = g.TDSAmount, Profit = g.Profit, TeamCommission = g.TeamCommission, Incentive = g.Incentive }).OrderBy(x => x.Operator).ToList();
                        _res.Add(item);
                    }
                }
            }
            return _res;
        }
        public IEnumerable<APIDaybookDatewise> AdminDayBookDateAPIwiseNew(string FromDate, string ToDate, int APIID)
        {
            var _res = new List<APIDaybookDatewise>();
            if (_lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.ShowAdminDaybook))
            {
                var _req = new ForDateFilter
                {
                    LT = _lr.LoginTypeID,
                    LoginID = _lr.UserID,
                    FromDate = FromDate,
                    ToDate = ToDate,
                    EventID = APIID
                };
                _dal = ChangeConString(_req.FromDate);
                IProcedure _proc = new ProcDaybookDateAPIwise(_dal);
                var daybook = (List<Daybook>)_proc.Call(_req);
                if (daybook.Any())
                {
                    var Grouped = daybook.GroupBy(x => new { x.EntryDate, x.API }).Select(g => new APIDaybookDatewise { TDate = g.Key.EntryDate.ToString("dd MMM yyyy"), EntryDate = g.Key.EntryDate, API = g.Key.API, TotalHits = g.Sum(s => s.TotalHits), TotalAmount = g.Sum(s => s.TotalAmount), SuccessHits = g.Sum(s => s.SuccessHits), SuccessAmount = g.Sum(s => s.SuccessAmount), RefundAmount = g.Sum(s => s.RefundAmount), RefundHits = g.Sum(s => s.RefundHits), FailedHits = g.Sum(s => s.FailedHits), FailedAmount = g.Sum(s => s.FailedAmount), PendingHits = g.Sum(s => s.PendingHits), PendingAmount = g.Sum(s => s.PendingAmount), APICommission = g.Sum(s => s.APICommission), Commission = g.Sum(s => s.Commission), TDSAmount = g.Sum(s => s.TDSAmount), Profit = g.Sum(s => s.Profit), TeamCommission = g.Sum(s => s.TeamCommission), Incentive = g.Sum(s => s.Incentive) }).OrderBy(d => d.API).ThenBy(x => x.EntryDate).ToList();
                    foreach (var item in Grouped)
                    {
                        item.Daybooks = daybook.Where(x => x.EntryDate == item.EntryDate && x.API == item.API).Select(g => new Daybook { Operator = g.Operator, TotalHits = g.TotalHits, API = g.API, TotalAmount = g.TotalAmount, SuccessHits = g.SuccessHits, SuccessAmount = g.SuccessAmount, RefundHits = g.RefundHits, RefundAmount = g.RefundAmount, FailedHits = g.FailedHits, FailedAmount = g.FailedAmount, PendingHits = g.PendingHits, PendingAmount = g.PendingAmount, APICommission = g.APICommission, Commission = g.Commission, TDSAmount = g.TDSAmount, Profit = g.Profit, TeamCommission = g.TeamCommission, Incentive = g.Incentive }).OrderBy(x => x.Operator).ToList();
                        _res.Add(item);
                    }
                }
            }
            return _res;
        }
        public List<Daybook> AdminDayBookDaywise(string FromDate, string ToDate)
        {
            var _res = new List<Daybook>();
            if (_lr.RoleID == Role.Admin || userML.IsCustomerCareAuthorised(ActionCodes.ShowAdminDaybook))
            {
                var _req = new ForDateFilter
                {
                    LT = _lr.LoginTypeID,
                    LoginID = _lr.UserID,
                    FromDate = FromDate,
                    ToDate = ToDate
                };
                _req.FromDate = string.IsNullOrEmpty(_req.FromDate) ? DateTime.Now.ToString("dd MMM yyyy") : _req.FromDate;
                _dal = ChangeConString(_req.FromDate);
                IProcedure _proc = new ProcDaybookDaywise(_dal);
                return (List<Daybook>)_proc.Call(_req);
            }
            return _res;
        }
        public List<DMRDaybook> AdminDayBookDMR(string FromDate, string ToDate)
        {
            var _res = new List<DMRDaybook>();
            if (_lr.RoleID == Role.Admin || userML.IsCustomerCareAuthorised(ActionCodes.ShowAdminDaybook))
            {
                var _req = new ForDateFilter
                {
                    FromDate = FromDate,
                    ToDate = ToDate
                };
                _req.FromDate = string.IsNullOrEmpty(_req.FromDate) ? DateTime.Now.ToString("dd MMM yyyy") : _req.FromDate;
                _dal = ChangeConString(_req.FromDate);
                IProcedure _proc = new ProcAdminDayBookDMR(_dal);
                return (List<DMRDaybook>)_proc.Call(_req);
            }
            return _res;
        }
        public List<Daybook> UserDaybook(string FromDate, string ToDate, string Mob)
        {
            var _res = new List<Daybook>();
            var loginResp = chkAlternateSession();
            if (loginResp != null)
            {
                if (loginResp.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.ShowUserDaybook) || loginResp.LoginTypeID == LoginType.Employee)
                {
                    var _req = new CommonReq
                    {
                        CommonStr = FromDate,
                        CommonStr3 = ToDate,
                        LoginTypeID = loginResp.LoginTypeID,
                        LoginID = loginResp.UserID,
                        CommonStr2 = Mob ?? ""
                    };
                    if (userML.IsEndUser())
                    {
                        _req.CommonStr2 = "";
                    }
                    if (_req.CommonStr2 != "")
                    {
                        if (!Validate.O.IsMobile(_req.CommonStr2))
                        {
                            var Prefix = Validate.O.Prefix(_req.CommonStr2);
                            if (Validate.O.IsNumeric(Prefix))
                                _req.CommonInt = Validate.O.IsNumeric(_req.CommonStr2) ? Convert.ToInt32(_req.CommonStr2) : _req.CommonInt;
                            var uid = Validate.O.LoginID(_req.CommonStr2);
                            _req.CommonInt = Validate.O.IsNumeric(uid) ? Convert.ToInt32(uid) : _req.CommonInt;
                            _req.CommonStr2 = "";
                        }
                    }
                    _dal = ChangeConString(_req.CommonStr);
                    IProcedure _proc = new ProcUserDayBook(_dal);
                    _res = (List<Daybook>)_proc.Call(_req);
                }
            }
            return _res;
        }
        public IEnumerable<Daybook> UserDaybookIncentive(string FromDate, string ToDate, string Mob, int OID)
        {
            var _res = new List<Daybook>();
            if (_lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.ShowUserDaybook))
            {
                var _req = new CommonReq
                {
                    CommonStr = FromDate,
                    CommonStr3 = ToDate,
                    LoginTypeID = _lr.LoginTypeID,
                    LoginID = _lr.UserID,
                    CommonStr2 = Mob ?? "",
                    CommonInt2 = OID
                };
                if (userML.IsEndUser())
                {
                    _req.CommonStr2 = "";
                }
                if (_req.CommonStr2 != "")
                {
                    if (!Validate.O.IsMobile(_req.CommonStr2))
                    {
                        var Prefix = Validate.O.Prefix(_req.CommonStr2);
                        if (Validate.O.IsNumeric(Prefix))
                            _req.CommonInt = Validate.O.IsNumeric(_req.CommonStr2) ? Convert.ToInt32(_req.CommonStr2) : _req.CommonInt;
                        var uid = Validate.O.LoginID(_req.CommonStr2);
                        _req.CommonInt = Validate.O.IsNumeric(uid) ? Convert.ToInt32(uid) : _req.CommonInt;
                        _req.CommonStr2 = "";
                    }
                }
                _dal = ChangeConString(_req.CommonStr);
                IProcedure _proc = new ProcGetIncentiveByOperator(_dal);
                return (List<Daybook>)_proc.Call(_req);
            }
            return _res;
        }
        public IEnumerable<Daybook> CircleIncentive(string FromDate, string ToDate, string Mob, int OID)
        {
            var _res = new List<Daybook>();
            if (_lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.ShowUserDaybook))
            {
                var _req = new CommonReq
                {
                    CommonStr = FromDate,
                    CommonStr3 = ToDate,
                    LoginTypeID = _lr.LoginTypeID,
                    LoginID = _lr.UserID,
                    CommonStr2 = Mob ?? "",
                    CommonInt2 = OID
                };
                if (userML.IsEndUser())
                {
                    _req.CommonStr2 = "";
                }
                if (_req.CommonStr2 != "")
                {
                    if (!Validate.O.IsMobile(_req.CommonStr2))
                    {
                        var Prefix = Validate.O.Prefix(_req.CommonStr2);
                        if (Validate.O.IsNumeric(Prefix))
                            _req.CommonInt = Validate.O.IsNumeric(_req.CommonStr2) ? Convert.ToInt32(_req.CommonStr2) : _req.CommonInt;
                        var uid = Validate.O.LoginID(_req.CommonStr2);
                        _req.CommonInt = Validate.O.IsNumeric(uid) ? Convert.ToInt32(uid) : _req.CommonInt;
                        _req.CommonStr2 = "";
                    }
                }
                _dal = ChangeConString(_req.CommonStr);
                IProcedure _proc = new ProcGetCircleIncentiveByOperator(_dal);
                return (List<Daybook>)_proc.Call(_req);
            }
            return _res;
        }
        public IEnumerable<Daybook> DenominationTransactions(string FromDate, string ToDate, int UserID, int OID)
        {
            var _res = new List<Daybook>();
            if (_lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.ShowUserDaybook))
            {
                var _req = new CommonReq
                {
                    CommonStr = FromDate,
                    CommonStr2 = ToDate,
                    LoginTypeID = _lr.LoginTypeID,
                    LoginID = _lr.UserID,
                    CommonInt = UserID,
                    CommonInt2 = OID
                };
                _dal = ChangeConString(_req.CommonStr);
                IProcedure _proc = new ProcGetDenominationTotalTransactions(_dal);
                return (List<Daybook>)_proc.Call(_req);
            }
            return _res;
        }
        public List<DMRDaybook> UserDaybookDMR(string FromDate, string ToDate, string Mob)
        {
            var _res = new List<DMRDaybook>();
            if (_lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.ShowUserDaybook))
            {
                var _req = new CommonReq
                {
                    CommonStr = FromDate,
                    CommonStr3 = ToDate,
                    LoginTypeID = _lr.LoginTypeID,
                    LoginID = _lr.UserID,
                    CommonStr2 = Mob ?? ""
                };
                if (userML.IsEndUser())
                {
                    _req.CommonStr2 = "";
                }
                if (_req.CommonStr2 != "")
                {
                    if (!Validate.O.IsMobile(_req.CommonStr2))
                    {
                        var Prefix = Validate.O.Prefix(_req.CommonStr2);
                        if (Validate.O.IsNumeric(Prefix))
                            _req.CommonInt = Validate.O.IsNumeric(_req.CommonStr2) ? Convert.ToInt32(_req.CommonStr2) : _req.CommonInt;
                        var uid = Validate.O.LoginID(_req.CommonStr2);
                        _req.CommonInt = Validate.O.IsNumeric(uid) ? Convert.ToInt32(uid) : _req.CommonInt;
                        _req.CommonStr2 = "";
                    }
                }
                _dal = ChangeConString(_req.CommonStr);
                IProcedure _proc = new ProcUserDayBookDMR(_dal);
                return (List<DMRDaybook>)_proc.Call(_req);
            }
            return _res;
        }
        public IEnumerable<Daybook> UserDaybookApp(CommonReq commonReq)
        {
            if (commonReq.CommonStr2 != "")
            {
                if (!Validate.O.IsMobile(commonReq.CommonStr2))
                {
                    var Prefix = Validate.O.Prefix(commonReq.CommonStr2);
                    if (Validate.O.IsNumeric(Prefix))
                        commonReq.CommonInt = Validate.O.IsNumeric(commonReq.CommonStr2) ? Convert.ToInt32(commonReq.CommonStr2) : commonReq.CommonInt;
                    var uid = Validate.O.LoginID(commonReq.CommonStr2);
                    commonReq.CommonInt = Validate.O.IsNumeric(uid) ? Convert.ToInt32(uid) : commonReq.CommonInt;
                    commonReq.CommonStr2 = "";
                }
            }
            _dal = ChangeConString(commonReq.CommonStr);
            IProcedure _proc = new ProcUserDayBook(_dal);
            return (List<Daybook>)_proc.Call(commonReq);
        }
        public IEnumerable<DMRDaybook> UserDaybookDMRApp(CommonReq commonReq)
        {
            if (commonReq.CommonStr2 != "")
            {
                if (!Validate.O.IsMobile(commonReq.CommonStr2))
                {
                    var Prefix = Validate.O.Prefix(commonReq.CommonStr2);
                    if (Validate.O.IsNumeric(Prefix))
                        commonReq.CommonInt = Validate.O.IsNumeric(commonReq.CommonStr2) ? Convert.ToInt32(commonReq.CommonStr2) : commonReq.CommonInt;
                    var uid = Validate.O.LoginID(commonReq.CommonStr2);
                    commonReq.CommonInt = Validate.O.IsNumeric(uid) ? Convert.ToInt32(uid) : commonReq.CommonInt;
                    commonReq.CommonStr2 = "";
                }
            }
            _dal = ChangeConString(commonReq.CommonStr);
            IProcedure _proc = new ProcUserDayBookDMR(_dal);
            return (List<DMRDaybook>)_proc.Call(commonReq);
        }
        #endregion

        #region New Type Salesummary
        public async Task<List<UserRolewiseTransaction>> GetUSalesSummary(string FromDate, string ToDate, int RoleID, int UserID)
        {
            var _res = new List<UserRolewiseTransaction>();
            if (_lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.ShowUserSalesSummary))
            {
                var _req = new CommonReq
                {
                    LoginTypeID = _lr.LoginTypeID,
                    LoginID = _lr.UserID,
                    CommonInt = UserID,
                    CommonInt2 = RoleID,
                    CommonStr = FromDate,
                    CommonStr2 = ToDate
                };
                _dal = ChangeConString(FromDate);
                IProcedureAsync _proc = new ProcUserRolewiseTransaction(_dal);
                _res = (List<UserRolewiseTransaction>)await _proc.Call(_req).ConfigureAwait(false);
            }
            return _res;
        }
        public async Task<List<UserRolewiseTransaction>> GetUSalesSummaryDate(string FromDate, string ToDate, int RoleID, int UserID, bool IsSelf)
        {
            var _res = new List<UserRolewiseTransaction>();
            if (_lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.ShowUserSalesSummary))
            {
                var _req = new CommonReq
                {
                    LoginTypeID = _lr.LoginTypeID,
                    LoginID = _lr.UserID,
                    CommonInt = UserID,
                    CommonInt2 = RoleID,
                    CommonStr = FromDate,
                    CommonStr2 = ToDate,
                    CommonBool = IsSelf
                };
                if (!ApplicationSetting.IsSingleDB)
                    _dal = ChangeConString(FromDate);
                IProcedureAsync _proc = new ProcUserRolewiseTransactionDate(_dal);
                _res = (List<UserRolewiseTransaction>)await _proc.Call(_req).ConfigureAwait(false);
            }
            return _res;
        }
        public async Task<List<UserRolewiseTransaction>> GetUSalesSummaryOperator(string FromDate, string ToDate, int UserID, int OID, int OpTypeID)
        {
            var _res = new List<UserRolewiseTransaction>();
            if (_lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.ShowUserSalesSummary))
            {
                var _req = new CommonReq
                {
                    LoginTypeID = _lr.LoginTypeID,
                    LoginID = _lr.UserID,
                    CommonInt = UserID,
                    CommonStr = FromDate,
                    CommonStr2 = ToDate,
                    CommonInt2 = OID,
                    CommonInt4 = OpTypeID
                };
                if (!ApplicationSetting.IsSingleDB)
                    _dal = ChangeConString(FromDate);
                IProcedureAsync _proc = new ProcUserRolewiseTransactionOP(_dal);
                _res = (List<UserRolewiseTransaction>)await _proc.Call(_req).ConfigureAwait(false);
            }
            return _res;
        }
        public async Task<List<UserRolewiseTransaction>> GetUSalesSummaryOperator(string FromDate, string ToDate, int UserID, int OID, int OpTypeID, bool IsDate)
        {
            var _res = new List<UserRolewiseTransaction>();
            if (_lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.ShowUserSalesSummary))
            {
                var _req = new CommonReq
                {
                    LoginTypeID = _lr.LoginTypeID,
                    LoginID = _lr.UserID,
                    CommonInt = UserID,
                    CommonStr = FromDate,
                    CommonStr2 = ToDate,
                    CommonInt2 = OID,
                    CommonInt4 = OpTypeID,
                    CommonBool = IsDate
                };
                if (!ApplicationSetting.IsSingleDB)
                    _dal = ChangeConString(FromDate);
                IProcedureAsync _proc = new ProcUserRolewiseTransactionOP(_dal);
                _res = (List<UserRolewiseTransaction>)await _proc.Call(_req).ConfigureAwait(false);
            }
            return _res;
        }
        #endregion

        #region Old Type Sales Summary
        public List<SalesSummaryOpWise> GetSalesSummary(string FromDate, string ToDate, string UMobile)
        {
            List<SalesSummaryOpWise> _res = new List<SalesSummaryOpWise>();
            if ((_lr.LoginTypeID == LoginType.ApplicationUser && !userML.IsEndUser()) || userML.IsCustomerCareAuthorised(ActionCodes.ShowAdminSalesSummary))
            {
                _RechargeReportFilter _req = new _RechargeReportFilter
                {
                    LoginID = _lr.UserID,
                    FromDate = FromDate,
                    ToDate = ToDate,
                    OutletNo = UMobile ?? "",
                    LT = _lr.LoginTypeID
                };

                _dal = ChangeConString(_req.FromDate);
                List<SalesSummary> salesSummaries = new List<SalesSummary>();
                ProcSalesSummary _proc = new ProcSalesSummary(_dal);
                salesSummaries = (List<SalesSummary>)_proc.Call(_req);
                if (salesSummaries.Count > 0)
                {
                    List<SalesSummaryOpWise> salesSummaryOpWises = salesSummaries.GroupBy(x => new { x.OID, x.Operator })
                    .Select(g => new SalesSummaryOpWise { _OID = g.Key.OID, _Operator = g.Key.Operator, TAmount = g.Sum(s => s.Amount), TAmountR = g.Sum(s => s.AmountR), TLoginCom = g.Sum(s => s.LoginComm) }).ToList();
                    foreach (SalesSummaryOpWise ssow in salesSummaryOpWises)
                    {
                        ssow.OpSalesSummary = salesSummaries
                            .OrderByDescending(x => x.AmountR)
                            .Where(x => x.OID == ssow._OID)
                            .Select(g => new SalesSummary { UserID = g.UserID, OutletMobile = g.OutletMobile, OutletName = g.OutletName, Amount = g.Amount, AmountR = g.AmountR, LoginComm = g.LoginComm }).ToList();
                        _res.Add(ssow);
                    }
                }
            }
            return _res;
        }
        public async Task<List<SalesSummaryUserDateWise>> GetAPIUserSalesSummary(string FromDate, string ToDate, string UMobile)
        {
            var _res = new List<SalesSummaryUserDateWise>();
            var loginResp = chkAlternateSession();
            if (loginResp != null)
            {
                if (loginResp.LoginTypeID == LoginType.ApplicationUser && loginResp.RoleID.In(1, 2) || userML.IsCustomerCareAuthorised(ActionCodes.ShowUserSalesSummary))
                {
                    var _req = new ForDateFilter
                    {
                        LT = loginResp.LoginTypeID,
                        LoginID = loginResp.UserID,
                        FromDate = FromDate,
                        ToDate = ToDate,
                        UserMob = UMobile ?? ""
                    };
                    if (userML.IsEndUser())
                    {
                        _req.UserMob = "";
                    }
                    _dal = ChangeConString(_req.FromDate);
                    List<SalesSummary> salesSummaries = new List<SalesSummary>();
                    IProcedureAsync _proc = new ProcAPIUserSalesSummary(_dal);
                    salesSummaries = (List<SalesSummary>)await _proc.Call(_req).ConfigureAwait(false);
                    if (salesSummaries.Count > 0)
                    {
                        List<SalesSummaryUserDateWise> salesSummaryUDateWises = salesSummaries
                            .OrderBy(x => x.OutletName)
                            .GroupBy(x => new { x.UserID, x.OutletMobile, x.OutletName })
                        .Select(g => new SalesSummaryUserDateWise { _UserID = g.Key.UserID, _OutletName = g.Key.OutletName, _OutletMobile = g.Key.OutletMobile, TAmount = g.Sum(s => s.Amount), TAmountR = g.Sum(s => s.AmountR), TFAmount = g.Sum(s => s.FailedAmount), TFAmountR = g.Sum(s => s.FailedAmountR), TGSTAmount = g.Sum(s => s.GSTAmount), TTDSAmount = g.Sum(s => s.TDSAmount) }).ToList();
                        foreach (var ssudw in salesSummaryUDateWises)
                        {
                            ssudw.OpSalesSummary = salesSummaries
                                .Where(x => x.UserID == ssudw._UserID)
                                .GroupBy(x => new { x.ServiceID, x.OID, x.Operator })
                                .Select(g => new SalesSummary { ServiceID = g.Key.ServiceID, OID = g.Key.OID, Operator = g.Key.Operator, Amount = g.Sum(y => y.Amount), AmountR = g.Sum(y => y.AmountR), FailedAmount = g.Sum(y => y.FailedAmount), FailedAmountR = g.Sum(y => y.FailedAmountR), GSTAmount = g.Sum(y => y.GSTAmount), TDSAmount = g.Sum(y => y.TDSAmount) }).ToList();
                            _res.Add(ssudw);
                        }
                    }
                }
            }
            return _res;
        }
        public async Task<List<SalesSummaryUserDateWise>> GetAPIUserSalesSummaryDate(string FromDate, string ToDate, string UMobile)
        {
            var _res = new List<SalesSummaryUserDateWise>();

            var loginResp = chkAlternateSession();
            if (string.IsNullOrEmpty(UMobile) || (UMobile ?? string.Empty).Equals(loginResp.MobileNo))
            {
                return _res;
            }
            if (loginResp != null)
            {
                if (loginResp.LoginTypeID == LoginType.ApplicationUser && loginResp.RoleID.In(1, 2) || userML.IsCustomerCareAuthorised(ActionCodes.ShowUserSalesSummary))
                {
                    var _req = new ForDateFilter
                    {
                        LT = loginResp.LoginTypeID,
                        LoginID = loginResp.UserID,
                        FromDate = FromDate,
                        ToDate = ToDate,
                        UserMob = UMobile ?? ""
                    };
                    if (userML.IsEndUser())
                    {
                        _req.UserMob = "";
                    }
                    _dal = ChangeConString(_req.FromDate);
                    List<SalesSummary> salesSummaries = new List<SalesSummary>();
                    IProcedureAsync _proc = new ProcAPIUserSalesSummary(_dal);
                    salesSummaries = (List<SalesSummary>)await _proc.Call(_req).ConfigureAwait(false);
                    if (salesSummaries.Count > 0)
                    {
                        List<SalesSummaryUserDateWise> salesSummaryUDateWises = salesSummaries
                            .OrderBy(x => x.EntryDate)
                            .GroupBy(x => new { x.EntryDate })
                        .Select(g => new SalesSummaryUserDateWise { _EntryDate = g.Key.EntryDate, _OutletName = g.Max(m => m.OutletName), _OutletMobile = g.Max(m => m.OutletMobile), TAmount = g.Sum(s => s.Amount), TAmountR = g.Sum(s => s.AmountR), TFAmount = g.Sum(s => s.FailedAmount), TFAmountR = g.Sum(s => s.FailedAmountR), TGSTAmount = g.Sum(s => s.GSTAmount), TTDSAmount = g.Sum(s => s.TDSAmount) }).ToList();
                        foreach (var ssudw in salesSummaryUDateWises)
                        {
                            ssudw.OpSalesSummary = salesSummaries
                                .Where(x => x.EntryDate == ssudw._EntryDate)
                                .GroupBy(x => new { x.ServiceID, x.OID, x.Operator })
                                .Select(g => new SalesSummary { ServiceID = g.Key.ServiceID, OID = g.Key.OID, Operator = g.Key.Operator, Amount = g.Sum(y => y.Amount), AmountR = g.Sum(y => y.AmountR), FailedAmount = g.Sum(y => y.FailedAmount), FailedAmountR = g.Sum(y => y.FailedAmountR), GSTAmount = g.Sum(y => y.GSTAmount), TDSAmount = g.Sum(y => y.TDSAmount) }).ToList();
                            _res.Add(ssudw);
                        }
                    }
                }
            }
            return _res;
        }
        public List<SalesSummaryUserDateWise> GetUOpDateWiseTransaction(string FromDate, string ToDate, string UMobile)
        {
            List<SalesSummaryUserDateWise> _res = new List<SalesSummaryUserDateWise>();
            if (_lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.ShowUserSalesSummary))
            {
                var _req = new ForDateFilter
                {
                    LT = _lr.LoginTypeID,
                    LoginID = _lr.UserID,
                    FromDate = FromDate,
                    ToDate = ToDate,
                    UserMob = UMobile ?? ""
                };
                if (userML.IsEndUser())
                {
                    _req.UserMob = "";
                }
                _dal = ChangeConString(_req.FromDate);
                List<SalesSummary> salesSummaries = new List<SalesSummary>();
                IProcedure _proc = new ProcTransactionUserOpDateWise(_dal);
                salesSummaries = (List<SalesSummary>)_proc.Call(_req);
                if (salesSummaries.Count > 0)
                {
                    List<SalesSummaryUserDateWise> salesSummaryUDateWises = salesSummaries
                        .GroupBy(x => new { x.EntryDate })
                    .Select(g => new SalesSummaryUserDateWise { _EntryDate = g.Key.EntryDate, _OutletName = g.Max(s => s.OutletName), TAmount = g.Sum(s => s.Amount), TAmountR = g.Sum(s => s.AmountR), TFAmount = g.Sum(s => s.FailedAmount), TFAmountR = g.Sum(s => s.FailedAmountR) }).ToList();
                    foreach (var ssudw in salesSummaryUDateWises)
                    {
                        ssudw.OpSalesSummary = salesSummaries
                            .Where(x => x.EntryDate == ssudw._EntryDate)
                            .Select(g => new SalesSummary { Operator = g.Operator, Amount = g.Amount, AmountR = g.AmountR, FailedAmount = g.FailedAmount, FailedAmountR = g.FailedAmountR }).ToList();
                        _res.Add(ssudw);
                    }
                }
            }
            return _res;
        }
        public IEnumerable<AdminTransactionSummary> GetUSalesSummaryByAdmin(string FromDate, string ToDate)
        {
            var TRNSUMMARY = new List<AdminTransactionSummary>();
            var _res = new List<SalesSummaryUserDateWise>();
            var _req = new ForDateFilter
            {
                LT = LoginType.ApplicationUser,
                LoginID = 1,
                FromDate = FromDate,
                ToDate = ToDate
            };
            _dal = ChangeConString(_req.FromDate);
            var salesSummaries = new List<SalesSummary>();
            IProcedure _proc = new ProcUserSalesSummaryRoleWiseAdmin(_dal);
            salesSummaries = (List<SalesSummary>)_proc.Call(_req);
            if (salesSummaries.Count > 0)
            {
                var salesSummaryUDateWises = salesSummaries
                    .OrderBy(x => x.EntryDate)
                    .GroupBy(x => new { x.EntryDate, x.UserID, x.OutletMobile })
                .Select(g => new SalesSummaryUserDateWise { _EntryDate = g.Key.EntryDate, _UserID = g.Key.UserID, _OutletMobile = g.Key.OutletMobile, TAmount = g.Sum(s => s.Amount), TAmountR = g.Sum(s => s.AmountR), TSuccess = g.Sum(s => s.SCount), TFail = g.Sum(s => s.FCount) }).ToList();
                var adminReport = salesSummaryUDateWises.Select(x => new AdminTransactionSummary { MobileNo = x._OutletMobile, TotalTran = x.TSuccess, TotalAmt = x.TAmountR }).ToList();
                TRNSUMMARY = adminReport;
            }
            return TRNSUMMARY;
        }
        #endregion

        #region CustomerCareReports
        public async Task<List<CustomerCareActivity>> GetCCareActivity(string FromDate, string ToDate, string MobileNo, int OperationID)
        {
            if (_lr.RoleID == Role.Admin)
            {
                ForDateFilter _req = new ForDateFilter
                {
                    LoginID = _lr.UserID,
                    FromDate = FromDate ?? DateTime.Now.ToString("dd MMM yyyy"),
                    ToDate = ToDate ?? DateTime.Now.ToString("dd MMM yyyy"),
                    UserMob = MobileNo,
                    EventID = OperationID
                };
                _dal = ChangeConString(_req.FromDate);
                ProcCustomerCareActivityReport _proc = new ProcCustomerCareActivityReport(_dal);
                return (List<CustomerCareActivity>)await _proc.Call(_req);
            }
            return new List<CustomerCareActivity>();
        }
        #endregion

        #region MessageTemplate
        public MessageTemplate GetTemplate(int FormatID)
        {
            IProcedure proc = new procGetMessageAll(_dal);
            IProcedure _proc = new ProcMessageTemplate(_dal);
            MessageTemplate template = new MessageTemplate
            {
                MasterMessage = (IEnumerable<MasterMessage>)proc.Call(),
                MessageTemp = (IEnumerable<MessageTemplateKeyword>)_proc.Call(FormatID)
            };
            return template;
        }

        public IResponseStatus UpdateMessageFormat(MessageTemplateParam param)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (param.FormatID == 0)
            {
                _res.Msg = "Please select formatID";
                return _res;
            }
            if (string.IsNullOrEmpty(param.Subject))
            {
                _res.Msg = "Please Fill Subject";
                return _res;
            }
            if (string.IsNullOrEmpty(param.Template))
            {
                _res.Msg = "Please fill message";
                return _res;
            }
            if (param.TemplateType == 0)
            {
                _res.Msg = "Please Select Template Type";
                return _res;
            }
            if (string.IsNullOrEmpty(param.TemplateID) && param.TemplateType == 1)
            {
                _res.Msg = "Please Fill Template ID";
                return _res;
            }
            if (string.IsNullOrEmpty(param.WhatsAppTemplateID) && param.TemplateType == 7)
            {
                _res.Msg = "Please Fill Whats App Template ID";
                return _res;
            }
            param.WID = _lr.WID;
            //if (param.WID <= 0)
            //{
            //    _res.Msg = "Please Enter Valid White Level Id";
            //    return _res;
            //}
            if (param.TemplateType == MessageTemplateType.SMSTamplate)
            {
                param.IsEnableSMS = param.IsEnable;
            }
            if (param.TemplateType == MessageTemplateType.EmailTamplate)
            {
                param.IsEnableEmail = param.IsEnable;
            }
            if (param.TemplateType == MessageTemplateType.AlertTamplate)
            {
                param.IsEnableNotification = param.IsEnable;
            }
            if (param.TemplateType == MessageTemplateType.WebNotification)
            {
                param.IsEnableWebNotification = param.IsEnable;
            }
            if (param.TemplateType == MessageTemplateType.HangoutAlert)
            {
                param.IsEnableHangout = param.IsEnable;
            }
            if (param.TemplateType == MessageTemplateType.WhatsappAlert)
            {
                param.IsEnableWhatsApp = param.IsEnable;
            }
            if (param.TemplateType == MessageTemplateType.TelegramAlert)
            {
                param.IsEnableTelegram = param.IsEnable;

            }
            param.LoginTypeID = _lr.LoginTypeID;
            param.LoginID = _lr.UserID;
            IProcedure proc = new ProcUpdateMessageTemplatecs(_dal);
            return (ResponseStatus)proc.Call(param);
        }

        #endregion

        #region NumberSeries
        public NumberSeriesListWithCircle GetNumberSeriesListWithCircles(int _OID)
        {
            var resp = new NumberSeriesListWithCircle();
            if ((_lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser) || userML.IsCustomerCareAuthorised(ActionCodes.ShowNumberSeries))
            {
                var req = new CommonReq
                {
                    LoginTypeID = _lr.LoginTypeID,
                    LoginID = _lr.UserID,
                    CommonInt = _OID
                };
                IProcedure _proc = new ProcGetNumberList(_dal);
                resp = (NumberSeriesListWithCircle)_proc.Call(req);
            }
            return resp;
        }
        public ResponseStatus UpdateNumberSeries(int _OID, int _CircleID, string _Number, string _OldNumber, char _F, int _ID)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if ((_lr.LoginTypeID == LoginType.ApplicationUser && _lr.RoleID == Role.Admin) || userML.IsCustomerCareAuthorised(ActionCodes.ShowNumberSeries))
            {
                Validate validate = Validate.O;
                if ((_Number ?? "") != "-1")
                {
                    if (!validate.IsNumeric(_Number) || (_Number ?? "").Length != 4)
                    {
                        res.Msg = "Invalid series should be numeric and length 4";
                        return res;
                    }
                }
                if (_F == 48)
                {
                    var req = new CommonReq
                    {
                        LoginTypeID = _lr.LoginTypeID,
                        LoginID = _lr.UserID,
                        CommonInt = _OID,
                        CommonInt2 = _CircleID,
                        str = _Number,
                        CommonStr = _rinfo.GetRemoteIP(),
                        CommonStr2 = _rinfo.GetBrowserFullInfo(),
                        CommonStr3 = _OldNumber ?? ""
                    };
                    IProcedure _proc = new ProcUpdateNumberSeries(_dal);
                    res = (ResponseStatus)_proc.Call(req);
                }
                if (res.Flag == 'C' || _F.In('D', 'R', 'U'))
                {
                    res.Flag = res.Flag == 'C' ? res.Flag : _F;
                    res.CommonInt = _ID > 0 ? _ID : res.CommonInt;
                    var helperResp = UpdateNumberSeriesHelper(res.CommonInt, _Number, _OID, _CircleID, res.Flag);
                    res.Statuscode = helperResp.Statuscode;
                    res.Msg = helperResp.Msg;
                }
            }
            return res;
        }
        public IResponseStatus UpdateNumberSeriesHelper(int ID, string Number, int OID, int CircleID, char Flag)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if ((_lr.LoginTypeID == LoginType.ApplicationUser && _lr.RoleID == Role.Admin) || userML.IsCustomerCareAuthorised(ActionCodes.ShowNumberSeries))
            {
                var req = new CommonReq
                {
                    LoginTypeID = _lr.LoginTypeID,
                    LoginID = _lr.UserID,
                    CommonInt = OID,
                    CommonInt2 = CircleID,
                    CommonStr = Number,
                    CommonChar = Flag,
                    CommonInt3 = ID
                };
                IProcedure _proc = new ProcNumerSeriesCRUD(_dal);
                res = (ResponseStatus)_proc.Call(req);
            }
            return res;
        }
        public HLRResponseStatus CheckNumberSeriesExist(string _Number)
        {
            var res = new HLRResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if ((_lr.LoginTypeID == LoginType.ApplicationUser && (_lr.RoleID == Role.Admin || _lr.RoleID == Role.Retailor_Seller)) || userML.IsCustomerCareAuthorised(ActionCodes.ShowNumberSeries))
            {
                var req = new CommonReq
                {
                    LoginTypeID = _lr.LoginTypeID,
                    LoginID = _lr.UserID,
                    CommonStr = _Number,
                    CommonStr2 = SPKeys.HLRVerification
                };
                IProcedure _proc = new ProcCheckNumberSeries(_dal);
                res = (HLRResponseStatus)_proc.Call(req);
                if ((res.CommonInt > 0 && res.CommonInt2 > 0) && string.IsNullOrEmpty(res.CommonStr3))
                {
                    return res;
                }
                else if ((res.HLRAPIs != null) && (res.Statuscode == ErrorCodes.One))
                {
                    if (res.HLRAPIs.Count > 0)
                    {
                        foreach (var item in res.HLRAPIs)
                        {
                            res = HitHLRAPIs(item.APICode, item.APIURL, item.APIID, _lr.UserID, _Number);
                            if (!string.IsNullOrEmpty(res.CommonStr))
                            {
                                if (res.CommonStr.Equals(LookupAPIType.AirtelPPHLR))
                                {
                                    return res;
                                }
                            }
                            if ((res.CommonInt > 0) && !string.IsNullOrEmpty(res.CommonStr2))
                            {
                                return res;
                            }
                        }
                    }
                    return res;
                }
            }
            return res;
        }
        public HLRResponseStatus HitHLRAPIs(string ApiCode, string URL, int ApiId, int UserId, string MobNo)
        {
            var res = new HLRResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (ApiCode.Equals(LookupAPIType.PLANAPI))
            {
                ILookUpPlanAPI _ILookUp = new LookupML(_accessor, _env);
                res = _ILookUp.GetLookUp(URL, ApiId, UserId);
            }
            else if (ApiCode.Equals(LookupAPIType.GoRecharge))
            {
                ILookUpGoRecharge _ILookUp = new LookupML(_accessor, _env);
                res = _ILookUp.GetLookUpGoRecharge(URL, ApiId, UserId, MobNo);
            }
            else if (ApiCode.Equals(LookupAPIType.Roundpay))
            {
                ILookUpRoundpay _ILookUp = new LookupML(_accessor, _env);
                res = _ILookUp.GetLookUpRoundpay(URL, ApiId, UserId);
            }
            else if (ApiCode.Equals(LookupAPIType.APIBox))
            {
                ILookUpAPIBox _ILookUp = new LookupML(_accessor, _env);
                res = _ILookUp.GetLookUpAPIBox(URL, ApiId, UserId);
            }
            else if (ApiCode.Equals(LookupAPIType.MPLAN))
            {
                ILookUpMPLAN _ILookUp = new LookupML(_accessor, _env);
                res = _ILookUp.GetLookUpMplan(URL, ApiId, UserId);
            }
            else if (ApiCode.Equals(LookupAPIType.MYPLAN))
            {
                ILookUpMyPlan _ILookUp = new LookupML(_accessor, _env);
                res = _ILookUp.GetLookUpApiMyPlans(URL, ApiId, UserId);
            }
            else if (ApiCode.Equals(LookupAPIType.VASTWEB))
            {
                ILookUpVASTWEB _ILookUp = new LookupML(_accessor, _env);
                res = _ILookUp.GetHLRVastWeb(URL, res.CommonInt3, UserId, MobNo);
            }
            else if (ApiCode.Equals(LookupAPIType.INFOAPIHLR))
            {
                ILookUpInfoAPI _ILookUp = new LookupML(_accessor, _env);
                res = _ILookUp.GetHLRINFOAPI(URL, res.CommonInt3, UserId, MobNo);
            }
            else if (ApiCode.Equals(LookupAPIType.AirtelPPHLR))
            {
                ILookUpAirtelPP _ILookUp = new LookupML(_accessor, _env);
                res = _ILookUp.GetHLRAirtelPostpaid(URL, res.CommonInt3, UserId, MobNo);
            }
            return res;
        }
        public HLRResponseStatus CheckNumberSeriesExist(CommonReq req)
        {
            var res = new HLRResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };

            req.CommonStr2 = SPKeys.HLRVerification;
            IProcedure _proc = new ProcCheckNumberSeries(_dal);
            res = (HLRResponseStatus)_proc.Call(req);
            if ((res.CommonInt > 0 && res.CommonInt2 > 0) && string.IsNullOrEmpty(res.CommonStr3))
            {
                return res;
            }
            else if ((res.HLRAPIs != null) && (res.Statuscode == ErrorCodes.One))
            {
                if (res.HLRAPIs.Count > 0)
                {
                    foreach (var item in res.HLRAPIs)
                    {
                        res = HitHLRAPIs(item.APICode, item.APIURL, item.APIID, req.LoginID, req.CommonStr);
                        if ((res.CommonInt > 0) && !string.IsNullOrEmpty(res.CommonStr2))
                        {
                            return res;
                        }
                    }
                }
                return res;
            }
            return res;
        }
        #endregion

        #region SettlementRegion
        public List<AdminSettlement> GetAdminSettlement(string FromDate, string ToDate, int WalletTypeID)
        {
            var res = new List<AdminSettlement>();
            if (_lr.LoginTypeID == LoginType.ApplicationUser && _lr.RoleID == Role.Admin && (FromDate ?? "") != "" && (ToDate ?? "") != "")
            {
                var req = new CommonReq
                {
                    CommonStr = FromDate,
                    CommonStr2 = ToDate,
                    CommonInt = WalletTypeID
                };
                _dal = ChangeConString(req.CommonStr);
                IProcedure _proc = new ProcAdminSettlement(_dal);
                res = (List<AdminSettlement>)_proc.Call(req);
            }
            return res;
        }

        public async Task<List<UserSettlement>> GetUserSettlement(SettlementFilter filter)
        {
            var _lst = new List<UserSettlement>();
            if (_lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.ShowUserLedger))
            {
                var validate = Validate.O;
                filter.LT = _lr.LoginTypeID;
                filter.LoginID = _lr.UserID;
                if (filter.Mobile != "" && filter.Mobile != null)
                {
                    if (!Validate.O.IsMobile(filter.Mobile))
                    {
                        var Prefix = Validate.O.Prefix(filter.Mobile);
                        if (Validate.O.IsNumeric(Prefix))
                            filter.UserID = Validate.O.IsNumeric(filter.Mobile) ? Convert.ToInt32(filter.Mobile) : filter.UserID;
                        var uid = Validate.O.LoginID(filter.Mobile);
                        filter.UserID = Validate.O.IsNumeric(uid) ? Convert.ToInt32(uid) : filter.UserID;
                        filter.Mobile = "";
                    }
                }
                filter.FromDate = string.IsNullOrEmpty(filter.FromDate) ? DateTime.Now.ToString("dd MMM yyyy") : filter.FromDate;
                _dal = ChangeConString(filter.FromDate);
                IProcedureAsync _proc = new ProcGetUserSettlement(_dal);
                _lst = (List<UserSettlement>)await _proc.Call(filter).ConfigureAwait(false);
            }
            return _lst;
        }

        #endregion

        public IEnumerable<WalletType> GetWalletTypes()
        {
            IProcedure proc = new ProcFetchWallets(_dal);
            return (List<WalletType>)proc.Call();
        }
        private IDAL ChangeConString(string _date)
        {
            if (Validate.O.IsDateIn_dd_MMM_yyyy_Format(_date))
            {
                TypeMonthYear typeMonthYear = ConnectionStringHelper.O.GetTypeMonthYear(Convert.ToDateTime(_date.Replace(" ", "/")));
                if (typeMonthYear.ConType != ConnectionStringType.DBCon)
                {
                    _dal = new DAL(_c.GetConnectionString(typeMonthYear.ConType, (typeMonthYear.MM ?? "") + "_" + (typeMonthYear.YYYY ?? "")));
                }
            }
            return _dal;
        }
        #region LogDetails
        public object GetLogDetails(CommonReq req)
        {
            req.LoginID = _lr.UserID;
            req.LoginTypeID = _lr.LoginTypeID;
            IProcedure _IProc = new Proc_GetLogDetails(_dal);
            return _IProc.Call(req);
        }
        #endregion
        public List<WalletRequest> GetWalletRequestReport(WalletRequest req)
        {
            req.LoginID = _lr.UserID;
            req.LoginTypeID = _lr.LoginTypeID;
            IProcedure _IProc = new ProcGetSettlementReport(_dal);
            return (List<WalletRequest>)_IProc.Call(req);
        }

        public DataTable GetSettlementExcel(WalletRequest req)
        {

            List<WalletRequest> _list = new List<WalletRequest>();
            _list = GetWalletRequestReport(req);
            DataTable dt = new DataTable();
            dt.Columns.Add("Payment Mode");
            dt.Columns.Add("Bene Code");
            dt.Columns.Add("Bene A/c No.");
            dt.Columns.Add("Amount");
            dt.Columns.Add("Bene Name");
            dt.Columns.Add("Drawee Location");
            dt.Columns.Add("Print Location");
            dt.Columns.Add("Bene Addr 1");
            dt.Columns.Add("Bene Addr 2");
            dt.Columns.Add("City - Pincode");
            dt.Columns.Add("State");
            dt.Columns.Add("Zipcode");
            dt.Columns.Add("Instrument Ref No.");
            dt.Columns.Add("Customer Ref No.");
            dt.Columns.Add("Payment Detail 1");
            dt.Columns.Add("Payment Detail 2");
            dt.Columns.Add("Payment Detail 3");
            dt.Columns.Add("Payment Detail 4");
            dt.Columns.Add("Payment Detail 5");
            dt.Columns.Add("Payment Detail 6");
            dt.Columns.Add("Payment Detail 7");
            dt.Columns.Add("Instrument No");
            dt.Columns.Add("Inst. Date");
            dt.Columns.Add("MICR No");
            dt.Columns.Add("IFSC code");
            dt.Columns.Add("Bene Bank Name");
            dt.Columns.Add("Bene Bank Branch");
            dt.Columns.Add("Bene Email ID");
            dt.Columns.Add("Source Current Account Number");
            dt.Columns.Add("Remarks 1");
            dt.Columns.Add("Remarks 2");

            foreach (var item in _list)
            {
                DataRow dr = dt.NewRow();
                dr["Bene Code"] = item.UserRoleId;
                dr["Amount"] = item.Amount;
                dr["Customer Ref No."] = item.TID;
                dr["Payment Mode"] = item.TransMode;
                dr["Source Current Account Number"] = "201000042872";
                dt.Rows.Add(dr);
            }
            return dt;
        }
        public DataTable GetBeneficieryList(WalletRequest req)
        {
            req.LoginID = _lr.UserID;
            req.LoginTypeID = _lr.LoginTypeID;
            IProcedure _IProc = new ProcGetBeneList(_dal);
            List<WalletRequest> _list = new List<WalletRequest>();
            _list = (List<WalletRequest>)_IProc.Call(req);
            DataTable dt = new DataTable();
            dt.Columns.Add("BeneCode");
            dt.Columns.Add("BenName");
            dt.Columns.Add("Address1");
            dt.Columns.Add("Address2");
            dt.Columns.Add("City");
            dt.Columns.Add("State");
            dt.Columns.Add("Zip_Code");
            dt.Columns.Add("Phone");
            dt.Columns.Add("Email");
            dt.Columns.Add("BeneficiaryAccountNo");
            dt.Columns.Add("InputOnlyInternalFundTransferAccountno");
            dt.Columns.Add("Delivery_Address1");
            dt.Columns.Add("Delivery_Address2");
            dt.Columns.Add("Delivery_City");
            dt.Columns.Add("Delivery_State");
            dt.Columns.Add("Delivery_Zip_Code");
            dt.Columns.Add("PrintLocation");
            dt.Columns.Add("CustomerID");
            dt.Columns.Add("IFSC");
            dt.Columns.Add("MailTo");
            dt.Columns.Add("NEFT");
            dt.Columns.Add("RTGS");
            dt.Columns.Add("CHQ");
            dt.Columns.Add("DD");
            dt.Columns.Add("IFTO");
            dt.Columns.Add("FirstLinePrint");

            foreach (var item in _list)
            {
                DataRow dr = dt.NewRow();
                dr["BeneCode"] = item.UserRoleId;
                dr["BenName"] = item.UserName;
                dr["Address1"] = item.Address;
                dr["City"] = item.City;
                dr["State"] = item.State;
                dr["Zip_Code"] = item.Pincode;
                dr["Phone"] = item.Mobile;
                dr["Email"] = item.Email;

                if (item.BankName.Contains("INDUSIND"))
                {
                    dr["BeneficiaryAccountNo"] = "";
                    dr["InputOnlyInternalFundTransferAccountno"] = item.AccountNumber;
                    dr["NEFT"] = "I";
                }
                else
                {
                    dr["BeneficiaryAccountNo"] = item.AccountNumber;
                    dr["InputOnlyInternalFundTransferAccountno"] = "";
                    dr["NEFT"] = "N";
                }

                dr["CustomerID"] = "32358194";
                dr["IFSC"] = item.IFSC;
                dt.Rows.Add(dr);
            }
            return dt;
        }
        private int GetUserIDFromPrefixedID(string Mobile)
        {
            if (Validate.O.IsMobile(Mobile) || string.IsNullOrEmpty(Mobile))
            {
                return 0;
            }
            try
            {
                return Validate.O.IsAlphaNumeric(Mobile) ? Convert.ToInt32(Validate.O.LoginID(Mobile)) : Convert.ToInt32(Mobile);
            }
            catch (Exception)
            {

                return 0;
            }
        }
        #region Wrong2Right
        public async Task<RefundRequestResponse> MarkWrong2Right(WTRRequest _req)
        {
            var _res = new RefundRequestResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = "Invalid Access!"
            };
            if (_lr.RoleID.In(Role.Admin, Role.APIUser, Role.Retailor_Seller) && _lr.LoginTypeID == LoginType.ApplicationUser)
            {
                _req.UserID = _lr.UserID;
                _req.LoginType = _lr.LoginTypeID;
                var _V = Validate.O;
                if (_V.IsTransactionIDValid(_req.RPID))
                {
                    string TransDate = ConnectionStringHelper.O.ConvertTransactionIDTo_dd_MMM_yyyy(_req.RPID);
                    _dal = ChangeConString(TransDate);
                    ProcMakeWTRRequest _proc = new ProcMakeWTRRequest(_dal);
                    _res = (RefundRequestResponse)await _proc.Call(_req);
                }
                else
                {
                    _res.Msg = "Invalid TransactionID!";
                }
            }
            return _res;
        }
        public async Task<RefundRequestResponse> MarkWrong2RightApp(WTRRequest _req)
        {
            RefundRequestResponse _res = new RefundRequestResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = "Invalid Access!"
            };

            Validate _V = Validate.O;
            if (_V.IsTransactionIDValid(_req.RPID))
            {
                string TransDate = ConnectionStringHelper.O.ConvertTransactionIDTo_dd_MMM_yyyy(_req.RPID);
                _dal = ChangeConString(TransDate);
                ProcMakeWTRRequest _proc = new ProcMakeWTRRequest(_dal);
                _res = (RefundRequestResponse)await _proc.Call(_req);
            }
            else
            {
                _res.Msg = "Invalid TransactionID!";
            }

            return _res;
        }
        public List<RefundTransaction> GetWTRLog(RefundLogFilter filter)
        {
            var _lst = new List<RefundTransaction>();
            var validate = Validate.O;
            var _filter = new _RefundLogFilter
            {
                LoginTypeID = _lr.LoginTypeID,
                LoginID = _lr.UserID,
                TopRows = filter.TopRows,
                Status = filter.Status,
                APIID = filter.APIID,
                OID = filter.OID,
                DateType = filter.DateType,
                FromDate = filter.FromDate,
                ToDate = filter.ToDate,
                Criteria = filter.Criteria,
                CriteriaText = (filter.CriteriaText ?? "").Trim()
            };
            if (_filter.TopRows > 0)
            {
                if (_filter.Criteria == Criteria.OutletMobile)
                {
                    if (!validate.IsMobile(_filter.CriteriaText ?? ""))
                        return _lst;
                    _filter.OutletNo = _filter.CriteriaText;
                }
                if (_filter.Criteria == Criteria.AccountNo)
                {
                    if (string.IsNullOrEmpty(_filter.CriteriaText ?? ""))
                        return _lst;
                    _filter.AccountNo = _filter.CriteriaText;
                }
                if (_filter.Criteria == Criteria.RightAccountNo)
                {
                    if (string.IsNullOrEmpty(_filter.CriteriaText ?? ""))
                        return _lst;
                    _filter.RightAccountNo = _filter.CriteriaText;
                }
                if (_filter.Criteria == Criteria.TransactionID)
                {
                    if (string.IsNullOrEmpty(_filter.CriteriaText))
                        return _lst;
                    if (_filter.CriteriaText[0] != Criteria.StartCharOFTransaction)
                        return _lst;
                    _filter.TransactionID = _filter.CriteriaText;
                }
                if (_filter.Criteria == Criteria.TID)
                {
                    if (!validate.IsNumeric(_filter.CriteriaText ?? ""))
                        return _lst;
                    _filter.TID = Convert.ToInt32(_filter.CriteriaText);
                }
            }
            else
            {
                _filter.DateType = 3;
                _filter.FromDate = DateTime.Now.AddDays(-60).ToString("dd MMM yyyy");
                _filter.ToDate = DateTime.Now.ToString("dd MMM yyyy");
                _filter.Status = 2;
                _filter.TopRows = 1000;
            }

            _filter.FromDate = string.IsNullOrEmpty(_filter.FromDate) ? DateTime.Now.ToString("dd MMM yyyy") : _filter.FromDate;
            //_dal = ChangeConString(_filter.FromDate);
            IProcedure _proc = new ProcGetWTRLog(_dal);
            _lst = (List<RefundTransaction>)_proc.Call(_filter);
            return _lst;
        }



        public List<RefundTransaction> GetWTRLogApp(_RefundLogFilter _filter)
        {
            var _lst = new List<RefundTransaction>();
            var validate = Validate.O;

            if (_filter.TopRows > 0)
            {
                if (_filter.Criteria == Criteria.OutletMobile)
                {
                    if (!validate.IsMobile(_filter.CriteriaText ?? ""))
                        return _lst;
                    _filter.OutletNo = _filter.CriteriaText;
                }
                if (_filter.Criteria == Criteria.AccountNo)
                {
                    if (string.IsNullOrEmpty(_filter.CriteriaText ?? ""))
                        return _lst;
                    _filter.AccountNo = _filter.CriteriaText;
                }
                if (_filter.Criteria == Criteria.RightAccountNo)
                {
                    if (string.IsNullOrEmpty(_filter.CriteriaText ?? ""))
                        return _lst;
                    _filter.RightAccountNo = _filter.CriteriaText;
                }
                if (_filter.Criteria == Criteria.TransactionID)
                {
                    if (string.IsNullOrEmpty(_filter.CriteriaText))
                        return _lst;
                    if (_filter.CriteriaText[0] != Criteria.StartCharOFTransaction)
                        return _lst;
                    _filter.TransactionID = _filter.CriteriaText;
                }
                if (_filter.Criteria == Criteria.TID)
                {
                    if (!validate.IsNumeric(_filter.CriteriaText ?? ""))
                        return _lst;
                    _filter.TID = Convert.ToInt32(_filter.CriteriaText);
                }
            }
            else
            {
                _filter.DateType = 3;
                _filter.FromDate = DateTime.Now.AddDays(-60).ToString("dd MMM yyyy");
                _filter.ToDate = DateTime.Now.ToString("dd MMM yyyy");
                _filter.Status = 2;
                _filter.TopRows = 1000;
            }
            _filter.FromDate = string.IsNullOrEmpty(_filter.FromDate) ? DateTime.Now.ToString("dd MMM yyyy") : _filter.FromDate;
            //_dal = ChangeConString(_filter.FromDate);
            IProcedure _proc = new ProcGetWTRLog(_dal);
            _lst = (List<RefundTransaction>)_proc.Call(_filter);
            return _lst;
        }
        public IResponseStatus UpdateWTRStatus(RefundRequestData refundRequestData)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            if (_lr.RoleID == Role.Admin || userML.IsCustomerCareAuthorised(ActionCodes.ShowRefundRequest))
            {
                refundRequestData.LoginID = _lr.UserID;

                refundRequestData.RequestIP = _rinfo.GetRemoteIP();
                refundRequestData.Browser = _rinfo.GetBrowserFullInfo() + "_" + _rinfo.GetLocalIP();
                IProcedure _proc = new ProcUpdateW2RStatus(_dal);
                _res = (ResponseStatus)_proc.Call(refundRequestData);
            }
            return _res;
        }
        #endregion
        #region outlet
        public async Task<List<OutletsOfUsersList>> GetOutletUserList(OuletOfUsersListFilter filter)
        {
            var _lst = new List<OutletsOfUsersList>();

            if (_lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.ShowRechargeReport))
            {
                var validate = Validate.O;
                var _filter = new _OuletOfUsersListFilter
                {
                    LT = _lr.LoginTypeID,
                    LoginID = _lr.UserID,
                    TopRows = filter.TopRows,
                    Mobile_F = filter.MobileOrUserID ?? "",
                    KycStatus = filter.KycStatus,
                    VerifyStatus = filter.VerifyStatus,
                    Criteria = filter.Criteria,
                    CriteriaText = filter.CriteriaText,
                    ApiId = filter.ApiId
                };

                if (_filter.Mobile_F != "")
                {
                    if (!Validate.O.IsMobile(_filter.Mobile_F))
                    {
                        var Prefix = Validate.O.Prefix(_filter.Mobile_F);
                        if (Validate.O.IsNumeric(Prefix))
                            _filter.UserID = Validate.O.IsNumeric(_filter.Mobile_F) ? Convert.ToInt32(_filter.Mobile_F) : _filter.UserID;
                        var uid = Validate.O.LoginID(_filter.Mobile_F);
                        _filter.UserID = Validate.O.IsNumeric(uid) ? Convert.ToInt32(uid) : _filter.UserID;
                        _filter.Mobile_F = "";
                    }
                }

                if (filter.Criteria > 0)
                {
                    if ((filter.CriteriaText ?? "") == "")
                    {
                        return _lst;
                    }
                }
                if (_filter.Criteria == Criteria.OutletMobile)
                {
                    if (!validate.IsMobile(_filter.CriteriaText))
                    {
                        return _lst;
                    }
                    _filter.OutletMobile = _filter.CriteriaText;
                }
                if (_filter.Criteria == Criteria.OutletID)
                {
                    _filter.OutletID = Convert.ToInt32(_filter.CriteriaText);
                }

                if (_filter.Criteria == Criteria.PAN)
                {
                    _filter.PAN = _filter.CriteriaText;
                }
                if (_filter.Criteria == Criteria.Adhar)
                {
                    _filter.Adhar = _filter.CriteriaText;
                }
                if (_filter.Criteria == Criteria.DeviceId)
                {
                    _filter.DeviceId = _filter.CriteriaText;
                }

                if (filter.ServiceId == 5)
                {
                    _filter.ApiStatus = filter.ServiceStatusId;
                    _filter.ServiceId = 0;
                    _filter.ServiceStatusId = 5;
                }
                else
                {
                    _filter.ServiceId = filter.ServiceId;
                    _filter.ServiceStatusId = filter.ServiceStatusId;
                }


                IProcedureAsync _proc = new ProcOutletsOfUsersList(_dal);
                _lst = (List<OutletsOfUsersList>)await _proc.Call(_filter);
            }
            return _lst;
        }

        public OutletsOfUsersList GetOutletUserList(OuletOfUsersListFilter filter, int LoginID)
        {
            var _resp = new OutletsOfUsersList();

            var _filter = new _OuletOfUsersListFilter
            {
                LT = LoginType.ApplicationUser,
                LoginID = LoginID,
                TopRows = 1,
                OutletID = filter.Criteria
            };

            IProcedureAsync _proc = new ProcOutletsOfUsersList(_dal);
            var _lst = (List<OutletsOfUsersList>)_proc.Call(_filter).Result;
            if (_lst.Count > 0)
            {
                _resp = _lst.FirstOrDefault();
            }
            return _resp;
        }
        public List<ApiListModel> GetApiList()
        {
            var _lst = new List<ApiListModel>();

            if (_lr.LoginTypeID == LoginType.ApplicationUser)
            {
                var validate = Validate.O;
                var _filter = new CommonFilter
                {
                    LT = _lr.LoginTypeID,
                    LoginID = _lr.UserID
                };

                IProcedure _proc = new ProcGetApiList(_dal);
                _lst = (List<ApiListModel>)_proc.Call(_filter);
            }
            return _lst;
        }

        public async Task<IResponseStatus> UploadOutletUsersExcel(OutletsReqData reqData)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if ((_lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser) || userML.IsCustomerCareAuthorised(ActionCodes.PendingResend))
            {
                var _req = new OutletsReqData
                {
                    LoginTypeID = _lr.LoginTypeID,
                    LoginID = _lr.UserID,
                    //CommonInt = APIID,
                    APIId = reqData.APIId,
                    OutletsUserList = reqData.OutletsUserList,
                    CommonStr = _rinfo.GetRemoteIP(),
                    CommonStr2 = _rinfo.GetBrowser(),
                };
                IProcedureAsync _proc = new ProcInsertBulkOutletUser(_dal);
                _res = (ResponseStatus)await _proc.Call(_req).ConfigureAwait(false);
            }
            return _res;
        }

        public IEnumerable<ApiWiseDetail> GetApiWiseDetail(int OutletID)
        {
            var _lst = new List<ApiWiseDetail>();
            var Req = new CommonReq
            {
                LoginTypeID = _lr.LoginTypeID,
                LoginID = _lr.UserID,
                CommonInt = OutletID
            };
            if (_lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.ShowRechargeReport))
            {
                IProcedure _proc = new ProcOutletAPIWiseDetail(_dal);
                _lst = (List<ApiWiseDetail>)_proc.Call(Req);
            }
            return _lst;
        }
        #endregion

        public IEnumerable<Dashboard_Chart> GetTodaySummaryChart()
        {
            if (!(_lr.RoleID == Role.APIUser && _lr.LoginTypeID == LoginType.ApplicationUser))
                return new List<Dashboard_Chart>();
            var param = new CommonReq
            {
                LoginTypeID = _lr.LoginTypeID,
                LoginID = _lr.UserID,
                CommonStr = _rinfo.GetRemoteIP(),
                CommonStr2 = _rinfo.GetBrowser()
            };
            IProcedure f = new ProcGetTodaySummaryChart(_dal);
            return (List<Dashboard_Chart>)f.Call(param);
        }
        public IEnumerable<Users> _GetUserList(CommonReq req)
        {
            var param = new CommonReq
            {
                LoginTypeID = _lr.LoginTypeID,
                LoginID = _lr.UserID,
                CommonBool = req.CommonBool,       // For IsViewed
                CommonStr3 = req.CommonStr3,
                CommonStr4 = req.CommonStr4
            };
            // For UserID
            if (req.CommonInt == 1)
            {
                param.CommonInt = int.Parse(req.CommonStr);
            }
            // For User Mobile
            if (req.CommonInt == 2)
            {
                param.CommonStr = req.CommonStr;
            }
            // For IntroducerID
            if (req.CommonInt == 3)
            {
                param.CommonInt2 = Convert.ToInt32(req.CommonStr);
            }
            // For User Mobile
            if (req.CommonInt == 4)
            {
                param.CommonStr2 = req.CommonStr;
            }
            IProcedure f = new ProcUserReport(_dal);
            return (List<Users>)f.Call(param);
        }

        public IResponseStatus UpdateUser(int UserID)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            if (_lr.RoleID == Role.Admin || userML.IsCustomerCareAuthorised(ActionCodes.ShowRefundRequest))
            {
                IProcedure _proc = new ProcUpdateViewStatus(_dal);
                _res = (ResponseStatus)_proc.Call(UserID);
            }
            return _res;
        }
        public IResponseStatus TotalPendingViews()
        {
            IProcedure f = new ProcCountPendingViews(_dal);
            return (ResponseStatus)f.Call();
        }
        #region TopPerformer
        public IEnumerable<TopPerformer> GetTopPerformer(CommonReq req)
        {
            var param = new CommonReq
            {
                CommonInt = req.CommonInt,          // For Services
            };
            if (req.CommonStr == "WeakWise")
            {
                param.CommonStr2 = DateTime.Now.AddDays(req.CommonInt2 == 2 ? -14 : -7).ToString("dd MMM yyyy");
                param.CommonStr3 = DateTime.Now.AddDays(req.CommonInt2 == 2 ? -7 : 0).ToString("dd MMM yyyy");
            }
            else
            {
                DateTime date = DateTime.Now.AddMonths(req.CommonInt3 - 1);
                var firstDayOfMonth = new DateTime(date.Year, date.Month, 1);
                var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
                param.CommonStr2 = firstDayOfMonth.ToString("dd MMM yyyy");
                param.CommonStr3 = lastDayOfMonth.ToString("dd MMM yyyy");
            }
            _dal = new DAL(_c.GetConnectionString(1));
            IProcedure f = new ProcTopPerformer(_dal);
            var _res = (List<TopPerformer>)f.Call(param);
            return _res;
        }
        #endregion

        #region PGTransaction Report        
        public async Task<List<TransactionPG>> GetPGTransactionReport(TransactionPG filter)
        {
            var _lst = new List<TransactionPG>();

            if (_lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.ShowRechargeReport))
            {
                var validate = Validate.O;
                filter.LoginID = _lr.UserID;
                filter.LT = _lr.LoginTypeID;
                if (filter.Criteria > 0)
                {
                    if ((filter.CriteriaText ?? "") == "")
                    {
                        return _lst;
                    }
                }
                if (filter.Criteria == Criteria.OutletMobile)
                {
                    if (!validate.IsMobile(filter.CriteriaText))
                    {
                        return _lst;
                    }
                    filter.MobileNo = filter.CriteriaText;
                }
                if (filter.Criteria == Criteria.UserID)
                {
                    var Prefix = Validate.O.Prefix(filter.CriteriaText);
                    if (Validate.O.IsNumeric(Prefix))
                        filter.UserID = Validate.O.IsNumeric(filter.CriteriaText) ? Convert.ToInt32(filter.CriteriaText) : filter.UserID;
                    var uid = Validate.O.LoginID(filter.CriteriaText);
                    filter.UserID = Validate.O.IsNumeric(uid) ? Convert.ToInt32(uid) : filter.UserID;
                }

                if (filter.Criteria == Criteria.TransactionID)
                {
                    filter.TransactionID = filter.CriteriaText;
                }
                if (filter.Criteria == Criteria.TID)
                {
                    if (!validate.IsNumeric(filter.CriteriaText))
                    {
                        return _lst;
                    }
                    filter.TID = Convert.ToInt32(filter.CriteriaText);
                }
                if (filter.Criteria == Criteria.VendorID)
                {
                    filter.VendorID = filter.CriteriaText;
                }
                filter.FromDate = string.IsNullOrEmpty(filter.FromDate) ? DateTime.Now.ToString("dd MMM yyyy") : filter.FromDate;
                //_dal = ChangeConString(filter.FromDate);
                IProcedureAsync _proc = new ProcTransactionPGReport(_dal);
                _lst = (List<TransactionPG>)await _proc.Call(filter).ConfigureAwait(false);
            }
            return _lst;
        }

        public async Task<List<TransactionPG>> GetPendingPGTransactionReport()
        {
            var _lst = new List<TransactionPG>();

            if (_lr.LoginTypeID.In(LoginType.ApplicationUser, LoginType.CustomerCare) || userML.IsCustomerCareAuthorised(ActionCodes.ShowRechargeReport))
            {
                TransactionPG filter = new TransactionPG
                {
                    LoginID = _lr.UserID,
                    LT = _lr.LoginTypeID,
                    Type = RechargeRespType.PENDING,
                    TopRows = 100,
                    FromDate = DateTime.Now.AddDays(-30).ToString("dd MMM yyyy"),
                };
                IProcedureAsync _proc = new ProcTransactionPGReport(_dal);
                _lst = (List<TransactionPG>)await _proc.Call(filter);
            }
            return _lst;
        }
        public TransactionPGLogDetail GetTransactionPGLog(TransactionPGLogDetail param)
        {
            var _lst = new TransactionPGLogDetail();

            if (_lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.ShowRechargeReport))
            {
                //if (Validate.O.IsTransactionIDValid(param.TransactionID))
                //{
                //    var TransDate = ConnectionStringHelper.O.ConvertTransactionIDTo_dd_MMM_yyyy(param.TransactionID);
                //    _dal = ChangeConString(TransDate);
                //}
                IProcedure _proc = new ProcTransactionPGLog(_dal);
                _lst = (TransactionPGLogDetail)_proc.Call(param);
            }
            return _lst;
        }

        public IResponseStatus ChangeTransactionPGStatus(TransactionPG param)
        {
            var _res = new ResponseStatus();
            if (_lr.RoleID == Role.Admin || userML.IsCustomerCareAuthorised(ActionCodes.ShowRefundRequest))
            {
                param.LT = _lr.LoginTypeID;
                param.LoginID = _lr.UserID;
                param.RequestIP = _rinfo.GetRemoteIP();
                param.Browser = _rinfo.GetBrowserFullInfo();
                IProcedure _proc = new ProcPgTransactionFromPage(_dal);
                _res = (ResponseStatus)_proc.Call(param);
            }
            return _res;
        }
        #endregion
        public List<WalletRequest> appGetWalletRequestReport(WalletRequest req)
        {
            IProcedure _IProc = new ProcGetSettlementReport(_dal);
            return (List<WalletRequest>)_IProc.Call(req);
        }

        public IEnumerable<GetEditUser> GetUserBankUpdateRequest()
        {
            var _lst = new List<GetEditUser>();
            if (_lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.ShowFundRequest))
            {
                var req = new CommonReq
                {
                    LoginTypeID = _lr.LoginTypeID,
                    LoginID = _lr.UserID
                };
                IProcedure _proc = new ProcGetBankUpdateRequest(_dal);
                _lst = (List<GetEditUser>)_proc.Call(req);
            }
            return _lst;
        }

        public async Task<IResponseStatus> AcceptOrRejectBankupdateRequest(GetEditUser RequestData)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (_lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser)
            {
                RequestData.LT = _lr.LoginTypeID;
                RequestData.LoginID = _lr.UserID;
                IProcedureAsync _proc = new ProcUpdateBankDetails(_dal);
                var alertParam = (AlertReplacementModel)await _proc.Call(RequestData);
                _res.Statuscode = alertParam.Statuscode;
                _res.Msg = alertParam.Msg;
                alertParam.RequestStatus = RequestData.RequestStatus;
                IAlertML alertMl = new AlertML(_accessor, _env);
                Parallel.Invoke(() => Task.Run(async () => await alertMl.UserPartialApprovalSMS(alertParam).ConfigureAwait(false)),
                () => Task.Run(async () => await alertMl.UserPartialApprovalEmail(alertParam).ConfigureAwait(false)),
                () => Task.Run(async () => await alertMl.WebNotification(alertParam).ConfigureAwait(false)));
            }
            return _res;
        }


        #region Sahred-Component
        public async Task<PSTReportUser> GetPriSecTerUserData()
        {
            var _resp = new PSTReportUser();
            CommonReq req = new CommonReq();
            if (_lr.LoginTypeID == LoginType.ApplicationUser)
            {
                req.LoginID = _lr.UserID;
                req.LoginTypeID = _lr.LoginTypeID;
                req.CommonStr = DateTime.Now.ToString("dd MMM yyyy");
                IProcedureAsync _proc = new ProcPSTReportForUser(_dal);
                _resp = (PSTReportUser)await _proc.Call(req).ConfigureAwait(false);
            }
            return _resp;
        }

        public AccountSummaryTable GetAccountSummaryTable()
        {
            var _res = new AccountSummaryTable();
            var _req = new CommonReq
            {
                LoginTypeID = _lr.LoginTypeID,
                LoginID = _lr.UserID
            };
            IProcedure _proc = new ProcAccountSummaryTable(_dal);
            _res = (AccountSummaryTable)_proc.Call(_req);
            return _res;
        }

        public DealerSummary GetDealerSummary()
        {
            CommonReq req = new CommonReq
            {
                LoginID = _lr.UserID,
                LoginTypeID = _lr.LoginTypeID
            };

            IProcedure procedure = new ProcSummary(_dal);
            var res = (DealerSummary)procedure.Call(req);
            return res;
        }
        public async Task<List<ProcRecentTransactionCounts>> GetRecentDaysTransactions(int OpTypeID, int ServiceTypeID)
        {
            IProcedureAsync _proc = new ProcRecentDaysTransaction(_dal);
            return (List<ProcRecentTransactionCounts>)await _proc.Call(new CommonReq
            {
                CommonInt = OpTypeID,
                CommonInt2 = ServiceTypeID,
                LoginID = _lr.UserID
            }).ConfigureAwait(false);
        }

        public async Task<List<ProcRecentTransactionCounts>> GetRecentDaysPriSecTer(int PriSecTerType)
        {
            IProcedureAsync _proc = new ProcRecentDaysPriSecTer(_dal);
            return (List<ProcRecentTransactionCounts>)await _proc.Call(new CommonReq
            {
                CommonInt = PriSecTerType,
                LoginID = _lr.UserID
            }).ConfigureAwait(false);
        }
        #endregion

    }

    public partial class ReportML : IInvoiceReportML
    {
        public InvoiceResponseModel GetInvoiceData(string Mobile, string Month, int InvoiceType)
        {
            var res = new InvoiceResponseModel
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (_lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.ShowGSTReport))
            {
                var req = new CommonReq
                {
                    LoginTypeID = _lr.LoginTypeID,
                    LoginID = _lr.UserID,
                    CommonInt = InvoiceType,
                    CommonStr = Mobile ?? string.Empty,
                    CommonStr2 = Month
                };
                if (!userML.IsCustomerCareAuthorised(ActionCodes.ShowGSTReport) && _lr.LoginTypeID == LoginType.ApplicationUser && _lr.RoleID != Role.Admin)
                    req.CommonStr = string.Empty;
                try
                {
                    if (ApplicationSetting.IsMultiMonthDB)
                    {
                        _dal = new DAL(_c.GetConnectionString(1));
                    }
                    else
                    {
                        _dal = ChangeConString(Convert.ToDateTime((Month ?? DateTime.Now.ToString("MMM yyyy"))).ToString("dd MMM yyyy"));
                    }
                }
                catch (Exception)
                {
                }

                IProcedure proc = new ProcGenerateInvoice(_dal);
                var procRes = (List<InvoiceResponse>)proc.Call(req);
                if (procRes.Count > 0)
                {
                    res.Statuscode = ErrorCodes.One;
                    res.Msg = ErrorCodes.SUCCESS;
                    var usersData = procRes[0];
                    if (usersData != null)
                    {
                        res.WID = usersData.WID;
                        res.UserAddress = usersData.Address;
                        res.UserState = usersData.State;
                        res.UserID = usersData.UserID;
                        res.UserOutletName = usersData.OutletName;
                        res.UserEmailID = usersData.EmailID;
                        res.UserPAN = usersData.PAN;
                        res.UserGST = usersData.GSTIN;
                        res.InvoiceNo = usersData.InvoiceRefID;
                        res.InvoiceMonth = Month;
                        res.StartDate = Convert.ToDateTime(Month).ToString("dd/MMM/yyyy");
                        res.EndDate = Convert.ToDateTime(Month).AddMonths(1).AddDays(-1).ToString("dd/MMM/yyyy");
                    }
                    if (res.WID > 0)
                    {
                        _dal = new DAL(_c.GetConnectionString());
                        IProcedure procCompanydata = new ProcGetInvoiceCompanyDetail(_dal);
                        res.CompanyDetail = (InvoiceCompanyDetail)procCompanydata.Call(new CommonReq
                        {
                            CommonInt = res.WID,
                            CommonStr = Month
                        });
                    }
                    res.InvoiceList = procRes;
                }
                else
                {
                    res.Msg = ErrorCodes.NODATA;
                }
            }

            return res;
        }
        public IEnumerable<CalculatedGSTEntry> GetGSTSummary(GSTReportFilter gSTReportFilter)
        {
            List<CalculatedGSTEntry> calculatedGSTEntries = new List<CalculatedGSTEntry>();
            if ((_lr.LoginTypeID == LoginType.ApplicationUser) || userML.IsCustomerCareAuthorised(ActionCodes.ShowGSTReport))
            {
                gSTReportFilter.LoginTypeID = _lr.LoginTypeID;
                gSTReportFilter.LoginID = _lr.UserID;
                gSTReportFilter.GSTMonth = gSTReportFilter.GSTMonth ?? DateTime.Now.ToString("MMM yyyy");
                if (ApplicationSetting.IsMultiMonthDB)
                {
                    _dal = new DAL(_c.GetConnectionString(1));
                }
                else
                {
                    _dal = ChangeConString(Convert.ToDateTime(gSTReportFilter.GSTMonth).ToString("dd MMM yyyy"));
                }

                IProcedure proc = new ProcGSTReportP2PAndP2A(_dal);
                calculatedGSTEntries = (List<CalculatedGSTEntry>)proc.Call(gSTReportFilter);
            }
            return calculatedGSTEntries;
        }
        public IEnumerable<CalculatedGSTEntry> GetTDSSummary(GSTReportFilter gSTReportFilter)
        {
            List<CalculatedGSTEntry> calculatedGSTEntries = new List<CalculatedGSTEntry>();
            if ((_lr.LoginTypeID == LoginType.ApplicationUser) || userML.IsCustomerCareAuthorised(ActionCodes.ShowGSTReport))
            {
                gSTReportFilter.LoginTypeID = _lr.LoginTypeID;
                gSTReportFilter.LoginID = _lr.UserID;
                gSTReportFilter.GSTMonth = gSTReportFilter.GSTMonth ?? DateTime.Now.ToString("MMM yyyy");
                if (ApplicationSetting.IsMultiMonthDB)
                {
                    _dal = new DAL(_c.GetConnectionString(1));
                }
                else
                {
                    _dal = ChangeConString(Convert.ToDateTime(gSTReportFilter.GSTMonth).ToString("dd MMM yyyy"));
                }
                IProcedure proc = new ProcGetTDSSummary(_dal);
                calculatedGSTEntries = (List<CalculatedGSTEntry>)proc.Call(gSTReportFilter);
            }
            return calculatedGSTEntries;
        }
        public InvoiceResponseModel GetInvoiceSummary(string Mobile, string Month)
        {
            var res = new InvoiceResponseModel
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (_lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.ShowGSTReport))
            {
                var req = new CommonReq
                {
                    LoginTypeID = _lr.LoginTypeID,
                    LoginID = _lr.UserID,
                    CommonStr = Mobile ?? string.Empty,
                    CommonStr2 = Month
                };
                if (!userML.IsCustomerCareAuthorised(ActionCodes.ShowGSTReport) && _lr.LoginTypeID == LoginType.ApplicationUser && _lr.RoleID != Role.Admin)
                    req.CommonStr = string.Empty;
                try
                {
                    if (ApplicationSetting.IsMultiMonthDB)
                    {
                        _dal = new DAL(_c.GetConnectionString(1));
                    }
                    else
                    {
                        _dal = ChangeConString(Convert.ToDateTime((Month ?? DateTime.Now.ToString("MMM yyyy"))).ToString("dd MMM yyyy"));
                    }
                }
                catch (Exception)
                {
                }

                IProcedure proc = new ProcGenerateInvoiceSumary(_dal);
                res.invoiceSummaries = (List<InvoiceSummaryResponse>)proc.Call(req);
            }
            return res;
        }
        public IEnumerable<InvoiceDetail> GetInvoiceMonths(string Mobile)
        {
            var res = new List<InvoiceDetail>();
            if (_lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.ShowGSTReport))
            {
                var req = new CommonReq
                {
                    LoginTypeID = _lr.LoginTypeID,
                    LoginID = _lr.UserID,
                    CommonStr = (_lr.RoleID == Role.Admin ? (Mobile ?? string.Empty) : _lr.MobileNo)
                };
                if (!userML.IsCustomerCareAuthorised(ActionCodes.ShowGSTReport) && _lr.LoginTypeID == LoginType.ApplicationUser && _lr.RoleID != Role.Admin)
                    req.CommonStr = string.Empty;
                _dal = new DAL(_c.GetConnectionString(1));
                IProcedure _proc = new ProcGetInvoices(_dal);
                res = (List<InvoiceDetail>)_proc.Call(req);
            }

            return res;
        }
        public List<InvoiceSettings> InvoiceSettings(bool IsDisable, int ID, int OpsCode)
        {
            var res = new List<InvoiceSettings>();
            if (_lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser)
            {
                var req = new CommonReq
                {
                    LoginID = _lr.UserID,
                    CommonInt = ID,
                    CommonBool = IsDisable,
                    CommonInt2 = OpsCode
                };
                _dal = new DAL(_c.GetConnectionString(1));
                IProcedure _proc = new ProcInvoiceSettingsOps(_dal);
                res = (List<InvoiceSettings>)_proc.Call(req);
            }
            return res;
        }
        #region PrimarySecondaryTertiary
        public async Task<List<PSTReport>> GetPrimarySecoundaryReport(CommonFilter filter)
        {
            var _resp = new List<PSTReport>();

            if (_lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.ShowRechargeReport))
            {
                filter.LoginID = _lr.UserID;
                filter.LT = _lr.LoginTypeID;
                filter.FromDate = string.IsNullOrEmpty(filter.FromDate) ? DateTime.Now.ToString("dd MMM yyyy") : filter.FromDate;
                _dal = ChangeConString(filter.FromDate);
                IProcedureAsync _proc = new ProcPSTReport(_dal);
                _resp = (List<PSTReport>)await _proc.Call(filter).ConfigureAwait(false);
            }
            return _resp;
        }
        public List<PSTReport> PSTDeatilReport(CommonFilter filter)
        {
            var _resp = new List<PSTReport>();

            if (_lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.ShowRechargeReport))
            {
                filter.FromDate = string.IsNullOrEmpty(filter.FromDate) ? DateTime.Now.ToString("dd MMM yyyy") : filter.FromDate;
                _dal = ChangeConString(filter.FromDate);
                IProcedure _proc = new ProcPSTDetailReport(_dal);
                _resp = (List<PSTReport>)_proc.Call(filter);
            }
            return _resp;
        }
        #endregion
        public List<ResponseStatus> getRequestApproval(CommonReq req)
        {
            var _resp = new List<ResponseStatus>();

            if (_lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.ShowRechargeReport))
            {
                IProcedure _proc = new ProcgetRequestApproval(_dal);
                _resp = (List<ResponseStatus>)_proc.Call(req);
            }
            return _resp;
        }
        public List<P2AInvoiceListModel> P2AInvoiceApprovalList(string Mobile)
        {
            _dal = new DAL(_c.GetConnectionString(1));
            IProcedure proc = new ProcGetP2AInvoiceUploaded(_dal);
            return (List<P2AInvoiceListModel>)proc.Call(new CommonReq
            {
                LoginTypeID = _lr.LoginTypeID,
                LoginID = _lr.UserID,
                CommonStr = Mobile
            });
        }
        #region DTHSubscription
        public async Task<List<DTHSubscriptionReport>> GetDthsubscriptionPendings()
        {
            List<DTHSubscriptionReport> _lst = null;
            var _filter = new CommonReq
            {
                LoginTypeID = _lr.LoginTypeID,
                LoginID = _lr.UserID
            };
            IProcedureAsync _proc = new ProcDthsubscriptionpendings(_dal);
            _lst = (List<DTHSubscriptionReport>)await _proc.Call(_filter).ConfigureAwait(false);
            return _lst;
        }


        public async Task<List<DTHSubscriptionReport>> GetDthsubscriptionReport(RechargeReportFilter filter)
        {
            List<DTHSubscriptionReport> _lst = null;
            var IsScanMoreDB = false;
            if (_lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.ShowRechargeReport))
            {
                var validate = Validate.O;
                var _filter = new _RechargeReportFilter
                {
                    LT = _lr.LoginTypeID,
                    LoginID = _lr.UserID,
                    TopRows = filter.TopRows,
                    Status = filter.Status,
                    APIID = filter.APIID,
                    OID = filter.OID,
                    FromDate = filter.FromDate,
                    ToDate = filter.ToDate,
                    Criteria = filter.Criteria,
                    CriteriaText = filter.CriteriaText,
                    IsExport = filter.IsExport,
                    OPTypeID = filter.OPTypeID,
                    RequestModeID = filter.RequestModeID,
                    BookingStatus = filter.BookingStatus
                };
                if (filter.Criteria > 0)
                {
                    if ((filter.CriteriaText ?? "") == "")
                    {
                        return _lst;
                    }
                }
                if (filter.Criteria == Criteria.OutletMobile)
                {
                    if (!validate.IsMobile(_filter.CriteriaText))
                    {
                        return _lst;
                    }
                    _filter.OutletNo = _filter.CriteriaText;
                }
                if (filter.Criteria == Criteria.AccountNo)
                {
                    _filter.AccountNo = _filter.CriteriaText;
                    if (string.IsNullOrEmpty(_filter.AccountNo))
                        return _lst;
                    if (ApplicationSetting.IsSingleDB)
                        _filter.FromDate = DateTime.Now.AddMonths(-2).ToString("dd MMM yyyy");
                    else
                        IsScanMoreDB = true;

                }
                if (filter.Criteria == Criteria.TransactionID)
                {
                    if (_filter.CriteriaText[0] != Criteria.StartCharOFTransaction)
                    {
                        return _lst;
                    }
                    _filter.TransactionID = _filter.CriteriaText;
                }
                if (filter.Criteria == Criteria.TID)
                {
                    try
                    {
                        _filter.TID = Convert.ToInt32(_filter.CriteriaText);
                    }
                    catch (Exception)
                    {
                        return _lst;
                    }
                }
                if (filter.Criteria == Criteria.APIRequestID)
                {
                    _filter.APIRequestID = _filter.CriteriaText;
                }
                if (filter.Criteria == Criteria.VendorID)
                {
                    _filter.VendorID = _filter.CriteriaText;
                }
                if (filter.Criteria == Criteria.CCID)
                {
                    if (!validate.IsNumeric(_filter.CriteriaText))
                    {
                        return _lst;
                    }
                    _filter.CCID = Convert.ToInt32(_filter.CriteriaText);
                }
                if (filter.Criteria == Criteria.CMobileNo)
                {
                    _filter.CMobileNo = _filter.CriteriaText;
                }
                if (filter.Criteria == Criteria.LiveID)
                {
                    _filter.LiveID = _filter.CriteriaText;
                }
                if (filter.Criteria == Criteria.UserID)
                {
                    var Prefix = Validate.O.Prefix(filter.CriteriaText);
                    if (Validate.O.IsNumeric(Prefix))
                        _filter.UserID = Validate.O.IsNumeric(filter.CriteriaText) ? Convert.ToInt32(filter.CriteriaText) : _filter.UserID;
                    var uid = Validate.O.LoginID(filter.CriteriaText);
                    _filter.UserID = Validate.O.IsNumeric(uid) ? Convert.ToInt32(uid) : _filter.UserID;
                }
                _filter.FromDate = string.IsNullOrEmpty(_filter.FromDate) ? DateTime.Now.ToString("dd MMM yyyy") : _filter.FromDate;

                IProcedureAsync _proc = new ProcDthsubscriptionReport(_dal);

                _proc = new ProcDthsubscriptionReport(_dal);
                _lst = (List<DTHSubscriptionReport>)await _proc.Call(_filter).ConfigureAwait(false);

            }
            return _lst;
        }
        public async Task<List<DTHSubscriptionReport>> GetDthsubscriptionReport(_RechargeReportFilter _filter)
        {
            List<DTHSubscriptionReport> _lst = null;
            var IsScanMoreDB = false;

            var validate = Validate.O;


            if (_filter.Criteria == Criteria.OutletMobile)
            {
                if (!validate.IsMobile(_filter.CriteriaText))
                {
                    return _lst;
                }
                _filter.OutletNo = _filter.CriteriaText;
            }
            if (_filter.Criteria == Criteria.AccountNo)
            {
                _filter.AccountNo = _filter.CriteriaText;
                if (string.IsNullOrEmpty(_filter.AccountNo))
                    return _lst;
                if (ApplicationSetting.IsSingleDB)
                    _filter.FromDate = DateTime.Now.AddMonths(-2).ToString("dd MMM yyyy");
                else
                    IsScanMoreDB = true;

            }
            if (_filter.Criteria == Criteria.TransactionID)
            {
                if (_filter.CriteriaText[0] != Criteria.StartCharOFTransaction)
                {
                    return _lst;
                }
                _filter.TransactionID = _filter.CriteriaText;
            }
            if (_filter.Criteria == Criteria.TID)
            {
                try
                {
                    _filter.TID = Convert.ToInt32(_filter.CriteriaText);
                }
                catch (Exception)
                {
                    return _lst;
                }
            }
            if (!string.IsNullOrEmpty(_filter.AccountNo))
            {
                if (ApplicationSetting.IsSingleDB)
                    _filter.FromDate = DateTime.Now.AddMonths(-2).ToString("dd MMM yyyy");
                else
                    IsScanMoreDB = true;
            }
            _filter.FromDate = string.IsNullOrEmpty(_filter.FromDate) ? DateTime.Now.ToString("dd MMM yyyy") : _filter.FromDate;

            IProcedureAsync _proc = new ProcDthsubscriptionReport(_dal);
            if (IsScanMoreDB)
            {
                _lst = (List<DTHSubscriptionReport>)await _proc.Call(_filter).ConfigureAwait(false);
                _dal = new DAL(_c.GetConnectionString(1));
                _proc = new ProcDthsubscriptionReport(_dal);
                var _lstSecond = (List<DTHSubscriptionReport>)await _proc.Call(_filter).ConfigureAwait(false);
                if (_lst == null)
                {
                    _lst = _lstSecond;
                }
                else if (_lst.Count == 0)
                {
                    _lst = _lstSecond;
                }
                else
                {
                    _lst.AddRange(_lstSecond);
                }
            }
            else
            {
                _dal = ChangeConString(_filter.FromDate);
                _proc = new ProcDthsubscriptionReport(_dal);
                _lst = (List<DTHSubscriptionReport>)await _proc.Call(_filter).ConfigureAwait(false);
            }
            return _lst;
        }


        public IResponseStatus UpdateBookingStatus(_CallbackData callbackData)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (_lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser || (userML.IsCustomerCareAuthorised(ActionCodes.PageSuccess) && callbackData.BookingStatus.In(BookingStatusType.ForwardToEngineer, BookingStatusType.Installing, BookingStatusType.Completed)) || (userML.IsCustomerCareAuthorised(ActionCodes.PageFailed) && callbackData.BookingStatus == BookingStatusType.Rejected))
            {
                callbackData.RequestIP = _rinfo.GetRemoteIP();
                callbackData.Browser = _rinfo.GetBrowserFullInfo();
                callbackData.LoginID = _lr.UserID;
                callbackData.LT = _lr.LoginTypeID;
                string TransDate = ConnectionStringHelper.O.ConvertTransactionIDTo_dd_MMM_yyyy(callbackData.TransactionID);
                _dal = ChangeConString(TransDate);
                IProcedure _procUpdate = new ProcUpdateBookingStatus(_dal);
                callbackData = (_CallbackData)_procUpdate.Call(callbackData);
                _res.Msg = callbackData.Msg;
                _res.Statuscode = callbackData.Statuscode;

            }
            return _res;
        }

        #endregion
        #region UserActivityLog
        public async Task<List<UserActivityLog>> GetUserActivity(string FromDate, string ToDate, string MobileNo)
        {
            if (_lr.RoleID == Role.Admin)
            {
                ForDateFilter _req = new ForDateFilter
                {
                    LoginID = _lr.UserID,
                    FromDate = FromDate ?? DateTime.Now.ToString("dd MMM yyyy"),
                    ToDate = ToDate ?? DateTime.Now.ToString("dd MMM yyyy"),
                    UserMob = MobileNo == null ? "" : MobileNo
                };
                _dal = ChangeConString(_req.FromDate);
                ProcLogUserActivity _proc = new ProcLogUserActivity(_dal);
                return (List<UserActivityLog>)await _proc.Call(_req);
            }
            return new List<UserActivityLog>();
        }
        #endregion
        public IResponseStatus CheckUploadPartnerImages(IFormFile file, string ImageType)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            using (var image = Image.FromStream(file.OpenReadStream()))
            {
                var width = image.Width;
                var height = image.Height;
                if (ImageType == FolderType.Logo)
                {
                    if (width >= UploadImageCheck.LogoMinWidth && height >= UploadImageCheck.LogoMinHeight)
                    {
                        _res.Statuscode = ErrorCodes.One;
                        _res.Msg = ErrorCodes.SUCCESS;
                    }
                }
                else if (ImageType == FolderType.BgImage)
                {
                    if (width >= UploadImageCheck.BgMinWidth && height >= UploadImageCheck.BgMinHeight)
                    {
                        _res.Statuscode = ErrorCodes.One;
                        _res.Msg = ErrorCodes.SUCCESS;
                    }
                }
            }
            return _res;
        }
        #region WebNotification
        public async Task<List<WebNotification>> GetWebNotification(bool IsShowAll)
        {
            var req = new CommonReq
            {
                LoginID = _lr.UserID,
                CommonBool = IsShowAll
            };
            IProcedureAsync _proc = new ProcGetWebNotification(_dal);
            var _resp = (List<WebNotification>)await _proc.Call(req);
            if (!IsShowAll)
            {
                CookieHelper cookie = new CookieHelper(_accessor);
                string _cookie = cookie.Get(SessionKeys.NotificationID);

                var cookieList = !string.IsNullOrEmpty(_cookie) ? _cookie.Split(',').ToList() : new List<string>();
                if (cookieList != null && cookieList.Count > 0)
                {
                    foreach (var item in cookieList)
                    {
                        _resp.RemoveAll(x => x.ID == Convert.ToInt32(item.Split('_')[0]) && x.EntryDate.Replace(" ", "") == item.Split('_')[1]);
                    }
                }
            }
            return _resp;
        }

        public ResponseStatus CloseNotification(int id, int userID, string EntryDate, string bulkNotification = "")
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            if (userID == -1)
            {
                CookieHelper cookie = new CookieHelper(_accessor);
                string preData = cookie.Get(SessionKeys.NotificationID);
                if (bulkNotification != "")
                {
                    preData = string.IsNullOrEmpty(preData) ? "" : preData + ",";
                    bulkNotification = preData + bulkNotification;
                    cookie.Set(SessionKeys.NotificationID, bulkNotification, _lr.CookieExpire);
                }
                else
                {
                    preData = string.IsNullOrEmpty(preData) ? "" : preData + ",";
                    StringBuilder sb = new StringBuilder(preData);
                    sb.Append("{0}_{1}");
                    sb.Replace("{0}", id.ToString());
                    sb.Replace("{1}", EntryDate.Replace(" ", ""));
                    cookie.Set(SessionKeys.NotificationID, sb.ToString(), _lr.CookieExpire);
                }

                res.Statuscode = ErrorCodes.One;
                res.Msg = "Seen";
            }
            else
            {
                IProcedure _proc = new ProcCloseNotification(_dal);
                res = (ResponseStatus)_proc.Call(id);
            }

            return res;
        }

        public async Task markallread()
        {
            List<WebNotification> notifications = await GetWebNotification(false).ConfigureAwait(false);
            string _name = string.Empty;
            if (notifications != null && notifications.Count > 0)
            {
                foreach (var item in notifications)
                {
                    if (item.UserID == -1)
                    {
                        _name = _name == "" ? item.ID + "_" + item.EntryDate.Replace(" ", "") : _name + "," + item.ID + "_" + item.EntryDate.Replace(" ", "");
                    }
                    else
                    {
                        CloseNotification(item.ID, item.UserID, item.EntryDate);
                    }
                }
                if (!string.IsNullOrEmpty(_name))
                {
                    CloseNotification(0, -1, "", _name);
                }
            }
        }
        public List<WebNotification> WebNotificationsReport(CommonFilter Req)
        {
            IProcedure proc = new ProcWebNotificationReport(_dal);
            var res = (List<WebNotification>)proc.Call(Req);
            return res;
        }

        public ResponseStatus DeactiveNotification(int ID, bool IsActive)
        {
            var Req = new CommonReq
            {
                CommonInt = ID,
                CommonBool = IsActive
            };
            IProcedure proc = new ProcDeactiveNotification(_dal);
            var res = (ResponseStatus)proc.Call(Req);
            return res;
        }
        #endregion
        private LoginResponse chkAlternateSession()
        {
            var result = new LoginResponse();
            if (_lr != null)
            {
                result = _lr;
            }
            if (_lrEmp != null)
            {
                result = _lrEmp;
            }
            return result;
        }
        public async Task<List<MAtmModel>> GetmAtmRequestList(MAtmFilterModel filter)
        {
            var _lst = new List<MAtmModel>();

            if ((_lr.LoginTypeID == LoginType.ApplicationUser && _lr.RoleID == Role.Admin) || userML.IsCustomerCareAuthorised(ActionCodes.ShowMaTMRequest))
            {
                var validate = Validate.O;
                var _filter = new MAtmFilterModel
                {
                    LT = _lr.LoginTypeID,
                    LoginID = _lr.UserID,
                    TopRows = filter.TopRows,
                    MobileNo = filter.MobileNo ?? "",
                    mAtamSerialNo = filter.mAtamSerialNo,
                    mAtamStatus = filter.mAtamStatus,
                    Criteria = filter.Criteria,
                    CriteriaText = filter.CriteriaText
                };

                if (_filter.MobileNo != "")
                {
                    if (!Validate.O.IsMobile(_filter.MobileNo))
                    {
                        var Prefix = Validate.O.Prefix(_filter.MobileNo);
                        if (Validate.O.IsNumeric(Prefix))
                            _filter.UserID = Validate.O.IsNumeric(_filter.MobileNo) ? Convert.ToInt32(_filter.MobileNo) : _filter.UserID;
                        var uid = Validate.O.LoginID(_filter.MobileNo);
                        _filter.UserID = Validate.O.IsNumeric(uid) ? Convert.ToInt32(uid) : _filter.UserID;
                        _filter.MobileNo = "";
                    }
                }

                if (filter.Criteria > 0)
                {
                    if ((filter.CriteriaText ?? "") == "")
                    {
                        return _lst;
                    }
                }

                IProcedureAsync _proc = new ProcGetmAtmRequestList(_dal);
                _lst = (List<MAtmModel>)await _proc.Call(_filter).ConfigureAwait(false);
            }
            return _lst;
        }

        public async Task<mAtmRequestResponse> UpdatemAtmRequestList(int id, int status)
        {
            mAtmRequestResponse model = new mAtmRequestResponse()
            {
                StatusCode = ErrorCodes.Minus1,
                Msg = "Error! Could not proceed"
            };
            if (_lr.LoginTypeID == LoginType.ApplicationUser && _lr.RoleID == Role.Admin)
            {
                model.LoginID = _lr.UserID;
                model.LT = _lr.LoginTypeID;
                model.ID = id;
                model.Status = status;
                IProcedureAsync _proc = new ProcUpdatemAtmRequest(_dal);
                var res = (mAtmRequestResponse)await _proc.Call(model);
                model.StatusCode = res.StatusCode;
                model.Msg = res.Msg;
            }
            return model;
        }

        #region AutoClearance
        public async Task LoopRechTransactionStatus()
        {
            int LastID = 0;
            int newLastID;
        StartHere:
            newLastID = LastID;
            var proc = new ProcPendingAutoClearance(_dal);
            var req = new CommonReq
            {
                CommonInt = LastID,
                CommonInt2 = 1
            };
            var res = (List<ResponseStatus>)proc.Call(req);

            if (res.Count > 0)
            {
                foreach (var item in res)
                {
                    LastID = item.CommonInt;
                    IRechargeReportML rml = new ReportML(_accessor, _env);
                    var _ = await rml.CheckStatusAsync(item.CommonInt2, item.CommonStr ?? string.Empty).ConfigureAwait(false);
                }
                if (LastID != newLastID)
                {
                    goto StartHere;
                }
            }

        }
        public async Task LoopDMTTransactionStatus()
        {
            int LastID = 0;
            int newLastID;
        StartHere:
            newLastID = LastID;
            var proc = new ProcPendingAutoClearance(_dal);
            var req = new CommonReq
            {
                CommonInt = LastID,
                CommonInt2 = 2
            };
            var res = (List<ResponseStatus>)proc.Call(req);

            if (res.Count > 0)
            {
                foreach (var item in res)
                {
                    LastID = item.CommonInt;
                    IDMRReportML rml = new ReportML(_accessor, _env);
                    await rml.CheckStatusDMRAsync(item.CommonInt2, item.CommonStr ?? string.Empty).ConfigureAwait(false);
                }
                if (LastID != newLastID)
                {
                    goto StartHere;
                }
            }

        }
        public void LoopAEPSTransactionStatus()
        {
            int LastID = 0;
            int newLastID;
        StartHere:
            newLastID = LastID;
            var proc = new ProcPendingAutoClearance(_dal);
            var req = new CommonReq
            {
                CommonInt = LastID,
                CommonInt2 = 17
            };
            var res = (List<ResponseStatus>)proc.Call(req);

            if (res.Count > 0)
            {
                foreach (var item in res)
                {
                    LastID = item.CommonInt;
                    IRechargeReportML rml = new ReportML(_accessor, _env);
                    var aStsRes = rml.CheckAEPSStatusAsync(item.CommonInt2, item.CommonStr ?? string.Empty).Result;
                }
                if (LastID != newLastID)
                {
                    goto StartHere;
                }
            }
        }

        #endregion

        #region AccountStatement
        public async Task<List<ProcUserLedgerResponse>> GetAccountStatement(ULedgerReportFilter filter)
        {
            var _lst = new List<ProcUserLedgerResponse>();
            var loginResp = chkAlternateSession();
            if (loginResp != null)
            {
                if (loginResp.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.ShowUserLedger) || loginResp.LoginTypeID == LoginType.Employee)
                {
                    var validate = Validate.O;
                    if ((filter.TransactionId_F ?? "") != "" && !validate.IsTransactionIDValid((filter.TransactionId_F ?? "")))
                    {
                        return _lst;
                    }
                    var _req = new ProcUserLedgerRequest
                    {
                        LT = loginResp.LoginTypeID,
                        LoginID = loginResp.UserID,
                        Mobile_F = filter.Mobile_F ?? "",
                        DebitCredit_F = filter.DebitCredit_F,
                        FromDate_F = !string.IsNullOrEmpty(filter.FromDate_F) ? filter.FromDate_F : null,
                        ToDate_F = !string.IsNullOrEmpty(filter.ToDate_F) ? filter.ToDate_F : null,
                        TransactionId_F = filter.TransactionId_F,
                        TopRows = filter.TopRows
                    };

                    if (filter.Mobile_F != "")
                    {
                        if (!Validate.O.IsMobile(filter.Mobile_F))
                        {
                            var Prefix = Validate.O.Prefix(filter.Mobile_F);
                            if (Validate.O.IsNumeric(Prefix))
                                _req.UserID = Validate.O.IsNumeric(filter.Mobile_F) ? Convert.ToInt32(filter.Mobile_F) : _req.UserID;
                            var uid = Validate.O.LoginID(filter.Mobile_F);
                            _req.UserID = Validate.O.IsNumeric(uid) ? Convert.ToInt32(uid) : _req.UserID;
                            _req.Mobile_F = "";
                        }
                    }
                    IProcedureAsync _proc = new ProcAccountStatement(_dal);
                    _lst = (List<ProcUserLedgerResponse>)await _proc.Call(_req);
                }
            }
            return _lst;
        }
        public async Task<List<ProcUserLedgerResponse>> AppGetAccountStatement(ULedgerReportFilter filter)
        {
            var _lst = new List<ProcUserLedgerResponse>();
            var validate = Validate.O;
            if ((filter.TransactionId_F ?? "") != "" && !validate.IsTransactionIDValid((filter.TransactionId_F ?? "")))
            {
                return _lst;
            }
            var _req = new ProcUserLedgerRequest
            {
                LT = LoginType.ApplicationUser,
                LoginID = filter.UID,
                Mobile_F = filter.Mobile_F ?? "",
                DebitCredit_F = filter.DebitCredit_F,
                FromDate_F = !string.IsNullOrEmpty(filter.FromDate_F) ? filter.FromDate_F : null,
                ToDate_F = !string.IsNullOrEmpty(filter.ToDate_F) ? filter.ToDate_F : null,
                TransactionId_F = filter.TransactionId_F,
                TopRows = filter.TopRows
            };
            if (filter.Mobile_F != "")
            {
                if (!Validate.O.IsMobile(filter.Mobile_F))
                {
                    var Prefix = Validate.O.Prefix(filter.Mobile_F);
                    if (Validate.O.IsNumeric(Prefix))
                        _req.UserID = Validate.O.IsNumeric(filter.Mobile_F) ? Convert.ToInt32(filter.Mobile_F) : _req.UserID;
                    var uid = Validate.O.LoginID(filter.Mobile_F);
                    _req.UserID = Validate.O.IsNumeric(uid) ? Convert.ToInt32(uid) : _req.UserID;
                    _req.Mobile_F = "";
                }
            }
            IProcedureAsync _proc = new ProcAccountStatement(_dal);
            _lst = (List<ProcUserLedgerResponse>)await _proc.Call(_req);
            return _lst;
        }

        public AccountSummary GetASSummary(ULedgerReportFilter filter)
        {
            var _res = new AccountSummary
            {
                StatusCode = ErrorCodes.Minus1,
                Status = ErrorCodes.AuthError
            };

            var loginResp = chkAlternateSession();
            if (loginResp != null)
            {
                if (loginResp.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.ShowUserLedger))
                {
                    var _req = new ProcUserLedgerRequest
                    {
                        LT = loginResp.LoginTypeID,
                        LoginID = loginResp.UserID,
                        Mobile_F = filter.Mobile_F,
                        FromDate_F = filter.FromDate_F,
                        ToDate_F = filter.ToDate_F,
                        WalletTypeID = filter.WalletTypeID
                    };
                    if (filter.Mobile_F != "")
                    {
                        if (!Validate.O.IsMobile(filter.Mobile_F))
                        {
                            var Prefix = Validate.O.Prefix(filter.Mobile_F);
                            if (Validate.O.IsNumeric(Prefix))
                                _req.UserID = Validate.O.IsNumeric(filter.Mobile_F) ? Convert.ToInt32(filter.Mobile_F) : _req.UserID;
                            var uid = Validate.O.LoginID(filter.Mobile_F);
                            _req.UserID = Validate.O.IsNumeric(uid) ? Convert.ToInt32(uid) : _req.UserID;
                            _req.Mobile_F = "";
                        }
                    }
                    IProcedure _proc = new ProcASHSummarry(_dal);
                    _res = (AccountSummary)_proc.Call(_req);
                }
            }
            return _res;
        }
        public List<AccountSummary> GetASCSummary(ULedgerReportFilter filter)
        {
            var _res = new List<AccountSummary>();
            var _req = new ProcUserLedgerRequest
            {
                LoginID = filter.RequestMode == RequestMode.APPS ? filter.UID : _lr.UserID,
                FromDate_F = filter.FromDate_F,
                ToDate_F = filter.ToDate_F,
                AreaID = filter.AreaID,
                UType = filter.UType,
                IsArea = ApplicationSetting.IsAreaMaster
            };
            IProcedure _proc = new ProcASCSummary(_dal);
            _res = (List<AccountSummary>)_proc.Call(_req);

            return _res;
        }
        public List<ASBanks> GetASBanks(int UID)
        {
            var _res = new List<ASBanks>();
            IProcedure _proc = new ProcGetTopIntroBank(_dal);
            _res = (List<ASBanks>)_proc.Call(UID);
            return _res;
        }
        public ResponseStatus ASPaymentCollection(ASCollectionReq aSCollectionReq)
        {
            var _resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            aSCollectionReq.LoginID = aSCollectionReq.RequestMode == RequestMode.APPS ? aSCollectionReq.LoginID : _lr.UserID;
            if (aSCollectionReq.UserID == 0)
            {
                _resp.Statuscode = ErrorCodes.Minus1;
                _resp.Msg = "No User Found!";
                return _resp;
            }
            if (aSCollectionReq.CollectionMode.ToUpper().Equals("BANK") && aSCollectionReq.BankName < 1)
            {
                _resp.Statuscode = ErrorCodes.Minus1;
                _resp.Msg = "No Bank Found!";
                return _resp;
            }
            if (aSCollectionReq.Amount < 1)
            {
                _resp.Statuscode = ErrorCodes.Minus1;
                _resp.Msg = "Wrong Amount!";
                return _resp;
            }
            IProcedure _proc = new ProcASPaymentCollection(_dal);
            _resp = (ResponseStatus)_proc.Call(aSCollectionReq);
            return _resp;
        }
        public List<ASAreaMaster> GetAreaMaster(int uid)
        {
            var _resp = new List<ASAreaMaster>();
            IProcedure _proc = new ProcGetAreaMaster(_dal);
            _resp = (List<ASAreaMaster>)_proc.Call(uid);
            return _resp;
        }
        public ResponseStatus AreaMasterCU(ASAreaMaster aSAreaMaster)
        {
            var _resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            aSAreaMaster.UserID = _lr.UserID;
            IProcedure _proc = new ProcAreaUserWiseCU(_dal);
            _resp = (ResponseStatus)_proc.Call(aSAreaMaster);
            return _resp;
        }
        public ResponseStatus MapUserArea(CommonReq commonReq)
        {
            var _resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            IProcedure _proc = new ProcMapUserArea(_dal);
            _resp = (ResponseStatus)_proc.Call(commonReq);
            return _resp;
        }
        public List<ASAreaMaster> GetAreaFPC(int uid)
        {
            var _resp = new List<ASAreaMaster>();
            IProcedure _proc = new ProcGetAreaForPC(_dal);
            _resp = (List<ASAreaMaster>)_proc.Call(uid);
            return _resp;
        }
        public List<UserReport> GetUListForVoucherEntry(int UID)
        {
            var _filter = new CommonFilter
            {
                TopRows = 3999,
                btnID = 1,
                SortByID = false
            };
            var _resp = new List<UserReport>();
            //var _ = new ProcGetAreaForPC(_dal);
            //var sts = (bool)_.CheckECollectionSts(UID);
            if (!ApplicationSetting.IsAreaMaster && _lr.RoleID == Role.FOS)
            {
                IFOSML _IFOSML = new FOSML(_accessor, _env);
                _resp = _IFOSML.GetListFOS(_filter).userReports;
            }
            else
            {
                IUserML userML = new UserML(_accessor, _env);
                _resp = userML.GetList(_filter).userReports;
            }
            if (_resp != null)
            {
                for (int i = 0; i < _resp.Count; i++)
                {
                    _resp[i].OutletName = _resp[i].OutletName + " [" + _resp[i].MobileNo + "]";
                }
            }
            return _resp;
        }
        #endregion

        #region Shared-Components
        public BCAgentSummary GetBCAgentSummary()
        {
            CommonReq req = new CommonReq
            {
                LoginID = _lr.UserID,
                LoginTypeID = _lr.LoginTypeID
            };

            IProcedure procedure = new ProcBCAgentSummary(_dal);
            var res = (BCAgentSummary)procedure.Call(req);
            return res;
        }

        public async Task<List<ServiceMaster>> GetUsedServicesList()
        {
            IProcedureAsync _proc = new ProcGetUsedServices(_dal);
            return (List<ServiceMaster>)await _proc.Call(new CommonReq
            {
                LoginID = _lr.UserID
            }).ConfigureAwait(false);
        }

        public async Task<List<MostUsedServices>> GetMostUsedServices()
        {
            IProcedureAsync _proc = new ProcMostUsedServices(_dal);
            return (List<MostUsedServices>)await _proc.Call(new CommonReq
            {
                LoginID = _lr.UserID
            }).ConfigureAwait(false);
        }

        public async Task<List<ProcRecentTransactionCounts>> GetMonthWeekDaysTransactions(string ActivityType, int RequestedDataType, int ServiceTypeID)
        {
            IProcedureAsync _proc = new ProcMonthWeekDaysTransaction(_dal);
            return (List<ProcRecentTransactionCounts>)await _proc.Call(new CommonReq
            {
                CommonStr = ActivityType,
                CommonInt = RequestedDataType,
                LoginTypeID = _lr.LoginTypeID,
                LoginID = _lr.UserID,
                CommonInt2 = ServiceTypeID
            }).ConfigureAwait(false);
        }

        public async Task<List<ProcRecentTransactionCounts>> GetTodayTransactionStatus(int RequestedDataType)
        {
            IProcedureAsync _proc = new ProcTodayTransactionStatus(_dal);
            return (List<ProcRecentTransactionCounts>)await _proc.Call(new CommonReq
            {
                CommonInt = RequestedDataType,
                LoginID = _lr.UserID
            }).ConfigureAwait(false);
        }

        public async Task<List<OutletsOfUsersList>> GetRecentOutletUserList(int TopRow)
        {
            var _lst = new List<OutletsOfUsersList>();

            IProcedureAsync _proc = new ProcRecentOutletsOfUsersList(_dal);
            _lst = (List<OutletsOfUsersList>)await _proc.Call(new CommonReq
            {
                LoginTypeID = _lr.LoginTypeID,
                LoginID = _lr.UserID,
                CommonInt = TopRow
            }).ConfigureAwait(false);
            return _lst;
        }

        public async Task<List<LoginDetail>> GetRecentLoginActivity(int TopRow)
        {
            var _lst = new List<LoginDetail>();

            IProcedureAsync _proc = new ProcRecentLoginActivity(_dal);
            _lst = (List<LoginDetail>)await _proc.Call(new CommonReq
            {
                LoginTypeID = _lr.LoginTypeID,
                LoginID = _lr.UserID,
                CommonInt = TopRow
            }).ConfigureAwait(false);
            return _lst;
        }

        public async Task<List<ProcRecentTransactionCounts>> GetRecentTransactionActivity()
        {
            IProcedureAsync _proc = new ProcRecentTransactionActivity(_dal);
            return (List<ProcRecentTransactionCounts>)await _proc.Call(new CommonReq
            {
                LoginID = _lr.UserID
            }).ConfigureAwait(false);
        }

        public async Task<List<ProcRecentTransactionCounts>> GetDateOpTypeWiseTransactionStatus(string RequestedDate, int OpTypeID, int RequestedDataType)
        {
            IProcedureAsync _proc = new ProcOpTypeWiseTransactionStatus(_dal);
            return (List<ProcRecentTransactionCounts>)await _proc.Call(new CommonReq
            {
                CommonInt = RequestedDataType,
                LoginID = _lr.UserID,
                CommonStr = RequestedDate,
                CommonInt2 = OpTypeID
            }).ConfigureAwait(false);
        }

        #endregion
        public Dashboard_Chart RPADCount()
        {
            if (!(_lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser))
                return new Dashboard_Chart();
            IProcedure f = new procTotalPending(_dal);
            return (Dashboard_Chart)f.Call();
        }
        public Dashboard_Chart RPADCountByUserID()
        {
            IProcedure f = new procGetPendingAndDisputeByUser(_dal);
            return (Dashboard_Chart)f.Call(new CommonReq
            {
                LoginTypeID = _lr.LoginTypeID,
                LoginID = _lr.UserID
            });
        }
        public IEnumerable<BankHoliday> GetUpcomingHolidays()
        {
            IProcedure _proc = new ProcGetUpcomingHoliday(_dal);
            return (List<BankHoliday>)_proc.Call(new CommonReq
            {
                LoginTypeID = _lr.LoginTypeID,
                LoginID = _lr.UserID
            });
        }

        public async Task<List<OutletsOfUsersList>> GetRecentOutletUserListByUserID()
        {
            var _lst = new List<OutletsOfUsersList>();
            IProcedureAsync _proc = new ProcOutletsOfUsersListByUserID(_dal);
            _lst = (List<OutletsOfUsersList>)await _proc.Call(new CommonReq
            {
                LoginTypeID = _lr.LoginTypeID,
                LoginID = _lr.UserID
            }).ConfigureAwait(false);
            return _lst;
        }

        public BBPSComplainAPIResponse RaiseBBPSComplain(GenerateBBPSComplainProcReq req)
        {
            var res = new BBPSComplainAPIResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (_lr.RoleID.In(Role.Retailor_Seller, Role.APIUser))
            {
                req.UserID = _lr.UserID;
                BBPSML bBPSML = new BBPSML(_accessor, _env, false);
                res = bBPSML.MakeBBPSComplain(req);
                if (res.Statuscode == 3)
                {
                    res.Statuscode = -1;
                }
            }
            return res;
        }
        public BBPSComplainAPIResponse TrackBBPSComplain(int TableID)
        {
            var res = new BBPSComplainAPIResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (_lr.RoleID.In(Role.Retailor_Seller, Role.APIUser, Role.Admin))
            {
                BBPSML bBPSML = new BBPSML(_accessor, _env, false);
                res = bBPSML.TrackBBPSComplain(new CommonReq
                {
                    UserID = _lr.UserID,
                    CommonInt = TableID,
                    CommonStr = string.Empty
                });
                if (res.Statuscode == 3)
                {
                    res.Statuscode = -1;
                }
            }
            return res;
        }

        public IEnumerable<BBPSComplainReport> BBPSComplainReport()
        {
            var res = new List<BBPSComplainReport>();
            if (_lr.RoleID.In(Role.Retailor_Seller, Role.APIUser, Role.Admin))
            {
                CommonReq req = new CommonReq();
                req.LoginID = _lr.UserID;
                req.LoginTypeID = _lr.LoginTypeID;

                IProcedure proc = new ProcBBPSComplainReport(_dal);
                res = (List<BBPSComplainReport>)proc.Call(req);
            }

            return res;
        }
        #region BBPS-Transaction-Status-Check
        public TransactionResponse GetBBPSTransactionStatusCheck(string RPID, string FromDate, string ToDate, string CustMob)
        {
            string TransDate = string.IsNullOrEmpty(RPID) ? FromDate : ConnectionStringHelper.O.ConvertTransactionIDTo_dd_MMM_yyyy(RPID);

            var commonReq = new CommonReq
            {
                LoginID = _lr.UserID,
                CommonStr = RPID,
                CommonBool = true,
                CommonStr3 = FromDate,
                CommonStr4 = ToDate,
                str = CustMob
            };
            IProcedureAsync _proc = new ProcGetTransactionStatus(ChangeConString(TransDate));
            return (TransactionResponse)_proc.Call(commonReq).Result;
        }
        #endregion

        #region BillFetchReportSection

        public async Task<List<ProcBillFetchReportResponse>> GetBillFetchReport(RechargeReportFilter filter)
        {
            var _lst = new List<ProcBillFetchReportResponse>();
            var IsScanMoreDB = false;
            var loginRes = chkAlternateSession();
            if (loginRes != null)
            {
                if (loginRes.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.ShowRechargeReport) || loginRes.LoginTypeID == LoginType.Employee)
                {
                    var validate = Validate.O;
                    var _filter = new _BillFetchReportFilter
                    {
                        LT = loginRes.LoginTypeID,
                        LoginID = loginRes.UserID,
                        TopRows = filter.TopRows,
                        Status = filter.Status,
                        API = filter.API,
                        OID = filter.OID,
                        FromDate = filter.FromDate,
                        ToDate = filter.ToDate,
                        Criteria = filter.Criteria,
                        CriteriaText = filter.CriteriaText,
                        IsExport = filter.IsExport,
                        OPTypeID = filter.OPTypeID,
                        RequestModeID = filter.RequestModeID,
                        CircleID = filter.CircleID,
                        SwitchID = filter.SwitchID,
                    };
                    try
                    {
                        if (filter.Criteria > 0)
                        {
                            if ((filter.CriteriaText ?? "") == "")
                            {
                                return _lst;
                            }
                        }
                        if (_filter.Criteria == Criteria.OutletMobile)
                        {
                            if (!validate.IsMobile(_filter.CriteriaText))
                            {
                                return _lst;
                            }
                            _filter.OutletNo = _filter.CriteriaText;
                        }
                        if (_filter.Criteria == Criteria.AccountNo)
                        {
                            _filter.AccountNo = _filter.CriteriaText;
                            if (string.IsNullOrEmpty(_filter.AccountNo))
                                return _lst;
                            if (ApplicationSetting.IsSingleDB)
                                _filter.FromDate = DateTime.Now.AddMonths(-2).ToString("dd MMM yyyy");
                            else
                                IsScanMoreDB = true;

                        }
                        if (_filter.Criteria == Criteria.BillNumber)
                        {
                            _filter.BillNumber = _filter.CriteriaText;
                        }
                        if (_filter.Criteria == Criteria.APIRequestID)
                        {
                            _filter.APIRequestID = _filter.CriteriaText;
                        }

                    }
                    catch (Exception)
                    {
                        return _lst;
                    }

                    _filter.FromDate = string.IsNullOrEmpty(_filter.FromDate) ? DateTime.Now.ToString("dd MMM yyyy") : _filter.FromDate;
                    if (IsScanMoreDB)
                    {
                        IProcedureAsync _proc = new ProcBillFetchReport(_dal);
                        _lst = (List<ProcBillFetchReportResponse>)await _proc.Call(_filter).ConfigureAwait(false);
                        _dal = new DAL(_c.GetConnectionString(1));
                        _proc = new ProcBillFetchReport(_dal);
                        var _lstSecond = (List<ProcBillFetchReportResponse>)await _proc.Call(_filter).ConfigureAwait(false);
                        if (_lst == null)
                        {
                            _lst = _lstSecond;
                        }
                        else if (_lst.Count == 0)
                        {
                            _lst = _lstSecond;
                        }
                        else
                        {
                            _lst.AddRange(_lstSecond);
                        }
                    }
                    else
                    {
                        //_dal = ChangeConString(_filter.FromDate);
                        IProcedureAsync _proc = new ProcBillFetchReport(_dal);
                        _lst = (List<ProcBillFetchReportResponse>)await _proc.Call(_filter).ConfigureAwait(false);
                    }
                }
            }
            return _lst;
        }

        public async Task<IEnumerable<ProcApiURlRequestResponse>> GetBillFetchApiResponses(string TID, string TransactionID)
        {
            List<ProcApiURlRequestResponse> _lst = new List<ProcApiURlRequestResponse>();
            ProcApiURlRequestResponse _res = new ProcApiURlRequestResponse();
            if (_lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.ShowBillFetchReport))
            {
                var req = new CommonReq
                {
                    LoginTypeID = _lr.LoginTypeID,
                    LoginID = _lr.UserID,
                    CommonStr = TID
                };
                string TransDate = "";
                if (Validate.O.IsTransactionIDValid(TransactionID))
                {
                    TransDate = ConnectionStringHelper.O.ConvertTransactionIDTo_dd_MMM_yyyy(TransactionID);
                    _dal = ChangeConString(TransDate);
                }
                IProcedureAsync _proc = new ProcGetBillFetchApiRequestReponse(_dal);
                _lst = (List<ProcApiURlRequestResponse>)await _proc.Call(req).ConfigureAwait(false);
            }
            return _lst;
        }

        #endregion
        public IEnumerable<OperatorParam> GetOperatorParam(int OID)
        {
            IProcedure _proc = new ProcGetOperatorParamByID(_dal);
            return (List<OperatorParam>)_proc.Call(new CommonReq
            {
                LoginTypeID = _lr.LoginTypeID,
                LoginID = _lr.UserID,
                CommonInt = OID,
            });
        }
        public async Task<List<PendingRechargeNotification>> PendingRechargeNotification(bool isCallByAutoRply = false, int userId = 0, int LoginTypeId = 1)
        {
            List<PendingRechargeNotification> transactios = new List<PendingRechargeNotification>();
            var req = new CommonReq
            {
                LoginTypeID = LoginTypeId,
                LoginID = userId
            };
            if (!isCallByAutoRply)
            {
                if ((_lr.RoleID == Role.Admin || (_lr.IsWLAPIAllowed && ApplicationSetting.IsWLAPIAllowed)) && _lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.ShowPending))
                {
                    req = new CommonReq
                    {
                        LoginTypeID = _lr.LoginTypeID,
                        LoginID = _lr.UserID
                    };
                }
            }
            IProcedureAsync _proc = new ProcGetPendingRechargeForNotification(_dal);
            transactios = (List<PendingRechargeNotification>)await _proc.Call(req).ConfigureAwait(false);
            return transactios;
        }

        public async Task<List<PendingRechargeNotification>> PendingRefundNotification(bool isCallByAutoRply = false, int userId = 0, int LoginTypeId = 1, int APIId = 0)
        {
            List<PendingRechargeNotification> transactios = new List<PendingRechargeNotification>();
            var req = new CommonReq
            {
                LoginTypeID = LoginTypeId,
                LoginID = userId,
                CommonInt = APIId
            };
            if (!isCallByAutoRply)
            {
                if ((_lr.RoleID == Role.Admin || (_lr.IsWLAPIAllowed && ApplicationSetting.IsWLAPIAllowed)) && _lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.ShowPending))
                {
                    req = new CommonReq
                    {
                        LoginTypeID = _lr.LoginTypeID,
                        LoginID = _lr.UserID
                    };
                }
            }
            IProcedureAsync _proc = new ProcGetPendingRefundForNotification(_dal);
            transactios = (List<PendingRechargeNotification>)await _proc.Call(req);

            return transactios;
        }

        #region BulkQRGeneration
        public async Task<QRGenerationReq> GetQRGenerationData(QRFilter qRFilter)
        {
            var _lst = new QRGenerationReq();
            var _req = new CommonReq
            {
                LoginTypeID = _lr.LoginTypeID,
                LoginID = _lr.UserID,
                CommonInt = qRFilter.PageSize,
                CommonInt2 = qRFilter.PageNo

            };
            if (qRFilter.FilterID != -1)
            {
                if (string.IsNullOrEmpty(qRFilter.FilterText))
                {
                    _lst.Statuscode = ErrorCodes.Minus1;
                    _lst.Msg = "Please enter some search string!";
                    return _lst;
                }
                if (qRFilter.FilterID == 3 && !Validate.O.IsMobile(qRFilter.FilterText))
                {
                    _lst.Statuscode = ErrorCodes.Minus1;
                    _lst.Msg = "Please Enter Correct Mobile No!";
                    return _lst;
                }
                if (qRFilter.FilterID == 1)
                    _req.CommonStr = qRFilter.FilterText;
                if (qRFilter.FilterID == 2)
                    _req.CommonStr2 = qRFilter.FilterText;
                if (qRFilter.FilterID == 3)
                    _req.CommonStr3 = qRFilter.FilterText;
            }

            IProcedureAsync _proc = new ProcGetQRGenerationData(_dal);
            _lst = (QRGenerationReq)await _proc.Call(_req).ConfigureAwait(false);
            return _lst;
        }
        public async Task<ResponseStatus> BulkQRGeneration(int Qty)
        {
            var resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            if (!Validate.O.IsNumeric(Qty.ToString()) || Qty < 1)
            {
                resp.Msg = "Please Enter a valid Number or integer!";
                return resp;
            }

            IProcedureAsync _proc = new ProcInitiateQRGeneration(_dal);
            while (Qty > 0)
            {
                resp = (ResponseStatus)await _proc.Call(new CommonReq
                {
                    LoginTypeID = _lr.LoginTypeID,
                    LoginID = _lr.UserID
                }).ConfigureAwait(false);
                Qty--;
            }
            return resp;
        }
        public ResponseStatus DownloadQR(int RefID)
        {
            var resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            if (_lr.RoleID != Role.Admin)
            {
                resp.Msg = "Wrong User Type!";
                return resp;
            }
            IProcedure proc = new ProcGetQRStocksByID(_dal);
            resp = (ResponseStatus)proc.Call(new CommonReq
            {
                LoginTypeID = _lr.LoginTypeID,
                LoginID = _lr.UserID,
                CommonInt = RefID
            });
            if (resp.Statuscode == ErrorCodes.One && string.IsNullOrEmpty(resp.CommonStr2))
            {
                IPaymentGatewayML pg = new PaymentGatewayML(_accessor, _env);
                resp = pg.GetQRGeneration(_lr.LoginTypeID, _lr.UserID, RefID, resp.CommonStr);
            }
            return resp;
        }
        public ResponseStatus GetQRStockDataByID(int RefID, int LT, int UID)
        {
            IProcedure proc = new ProcGetQRStocksByID(_dal);
            return (ResponseStatus)proc.Call(new CommonReq
            {
                LoginTypeID = LT,
                LoginID = UID,
                CommonInt = RefID
            });
        }
        public ResponseStatus MapQRToUser(string QRIntent, int UserID, int LT)
        {
            var resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            if (string.IsNullOrEmpty(QRIntent) && !QRIntent.Contains("&tr"))
            {
                resp.Msg = "Wrong QR!";
                return resp;
            }
            try
            {
                int startind = QRIntent.IndexOf("&tr=") + 4;
                int length = QRIntent.IndexOf("&am=") - startind;
                var RefID = QRIntent.Substring(startind, length);
                IProcedure proc = new ProcMapQRToUser(_dal);
                resp = (ResponseStatus)proc.Call(new CommonReq
                {
                    LoginTypeID = LT,
                    LoginID = UserID,
                    CommonInt = Convert.ToInt32(RefID.Substring(0, RefID.IndexOf("AUTOID")))
                });
            }
            catch (Exception ex)
            { }
            return resp;
        }
        #endregion

        public async Task<IPGeolocationInfo> GetIPGeolocationInfoByTable(string IP)
        {
            IPGeolocationInfo res = new IPGeolocationInfo();
            IProcedureAsync proc = new ProcGetIPGeolocationInfo(_dal);
            res = (IPGeolocationInfo)await proc.Call(IP);
            return res;
        }

        public async Task<IPGeolocationInfo> GetIPGeolocationInfoByAPI(string IP, int UserId)
        {
            IPGeolocationInfo res = new IPGeolocationInfo();
            try
            {
                IPGeoDetailURL = IPGeoDetailURL.Replace("{IP}", IP).Replace("{Access_Key}", "765380adc32b2a7c36aac4d4bdc2ab16");
                var response = await AppWebRequest.O.CallUsingHttpWebRequest_GETAsync(IPGeoDetailURL).ConfigureAwait(true);
                res = JsonConvert.DeserializeObject<IPGeolocationInfo>(response);
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "GetIPGeolocationInfoByAPI",
                    Error = ex.Message,
                    LoginTypeID = 1,
                    UserId = UserId
                });
            }
            return res ?? new IPGeolocationInfo();
        }

        public async Task<ResponseStatus> SaveIPGeolocationInfo(IPGeolocationInfo detail)
        {
            IProcedureAsync proc = new ProcSaveIPGeolocationInfo(_dal);
            var res = (ResponseStatus)await proc.Call(detail);
            return res;
        }

       
        public IEnumerable<SattlementAccountModels> GetUserBankUpdateRequest1(SattlementAccountModels data)
        {
            var _lst = new List<SattlementAccountModels>();
            if (_lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.ShowFundRequest))
            {
                var req = new SattlementAccountModels
                {
                    LoginTypeID = _lr.LoginTypeID,
                    LoginID = _lr.UserID,
                    VerificationStatus = data.VerificationStatus,
                    ApprovalStatus = data.ApprovalStatus,
                    CommonInt = data.CommonInt,
                    CommonStr3 = data.CommonStr3,
                    CommonInt2 = data.CommonInt2
                };
                IProcedure _proc = new ProcGetBankUpdateRequest(_dal);
                _lst = (List<SattlementAccountModels>)_proc.Call(req);
            }
            return _lst;
        }



        #region IPGeoLocation

        public IPStatusResp CheckIPGeoLocationInfo(IPGeoLocInfoReq iPGeoLocInfoReq)
        {
            var _resp = new IPStatusResp()
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            var req = new CommonReq
            {
                LoginTypeID = iPGeoLocInfoReq.LoginTypeID,
                LoginID = iPGeoLocInfoReq.UserID,
                CommonStr = iPGeoLocInfoReq.IPInfo,
                CommonStr2 = SPKeys.IPGeoLocationInfo
            };
            IProcedure _proc = new ProcCheckIPGeoLocInfo(_dal);
            var ipRes = (IPResponseStatus)_proc.Call(req);
            if (ipRes.Statuscode == ErrorCodes.One && ipRes.Msg.Contains("IP Found Successfully"))
            {
                _resp.Statuscode = ipRes.Statuscode;
                _resp.Msg = ipRes.Msg;
            }
            else if (ipRes.Statuscode == ErrorCodes.One && ipRes.Msg.Contains("Success"))
            {
                if (ipRes.IPGeoAPIs.Count > 0)
                {
                    foreach (var item in ipRes.IPGeoAPIs)
                    {
                        _resp = HitIPGeoAPIs(item.APICode, item.APIURL);
                        if (_resp.Statuscode == ErrorCodes.One)
                            break;
                        else
                            _resp = HitIPGeoAPIs(item.APICode, item.APIURL);
                    }
                }
            }
            else
            {
                _resp.Statuscode = ipRes.Statuscode;
                _resp.Msg = ipRes.Msg;
            }

            return _resp;

        }

        private IPStatusResp HitIPGeoAPIs(string ApiCode, string URL)
        {
            var res = new IPStatusResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (ApiCode.Equals(APICode.IPSTACK))
            {
                var webResult = AppWebRequest.O.CallUsingHttpWebRequest_GET(URL);
                if (!string.IsNullOrEmpty(webResult))
                {
                    var pResp = JsonConvert.DeserializeObject<IPStackAPIResp>(webResult);
                    if (pResp.error != null)
                    {
                        res.Statuscode = ErrorCodes.Minus1;
                        res.Msg = "No Response From API!";
                        return res;
                    }
                    else
                    {
                        res.Statuscode = ErrorCodes.One;
                        res.Msg = "IP Record Found Successfully!";
                        res.IP = pResp.ip;
                        res.Type = pResp.type;
                        res.Continent_Code = pResp.continent_code;
                        res.Continent_Name = pResp.continent_name;
                        res.Country_Code = pResp.country_code;
                        res.Country_Name = pResp.country_name;
                        res.Region_Code = pResp.region_code;
                        res.Region_Name = pResp.region_code;
                        res.City = pResp.city;
                        res.Pincode = pResp.zip;
                        res.Latitude = pResp.latitude.ToString();
                        res.Longitude = pResp.longitude.ToString();
                        res.Capital = pResp.location.capital;
                        if (pResp.location.languages != null)
                        {
                            res.LanguageName = pResp.location.languages[0].name;
                            res.LanguageNative = pResp.location.languages[0].native;
                        }
                    }

                }
            }
            return res;
        }

        public async Task<ResponseStatus> AuthenticateAPIReqForIPGeoInfo(int userId, string token)
        {
            var res = new ResponseStatus()
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            var validateReq = new ValidataRechargeApirequest
            {
                LoginID = userId,
                IPAddress = _rinfo.GetRemoteIP(),
                Token = token,
                SPKey = SPKeys.IPGeoLocationInfo,
                OID = 0
            };
            IProcedureAsync _procValidate = new ProcValidataRechargeApirequest(_dal);
            var validateRes = (ValidataRechargeApiResp)await _procValidate.Call(validateReq).ConfigureAwait(false);
            res.Statuscode = validateRes.Statuscode;
            res.Msg = validateRes.Msg;
            return res;
        }
        #endregion
        public async Task<List<BookingDetails>> GetHotelReport(RechargeReportFilter filter)
        {
            var _lst = new List<BookingDetails>();
            var IsScanMoreDB = false;
            var loginRes = chkAlternateSession();
            if (loginRes != null)
            {
                if (loginRes.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.ShowRechargeReport) || loginRes.LoginTypeID == LoginType.Employee)
                {
                    var validate = Validate.O;
                    var _filter = new _RechargeReportFilter
                    {
                        LT = loginRes.LoginTypeID,
                        LoginID = loginRes.UserID,
                        TopRows = filter.TopRows,
                        Status = filter.Status,
                        APIID = filter.APIID,
                        OID = filter.OID,
                        FromDate = filter.FromDate,
                        ToDate = filter.ToDate,
                        Criteria = filter.Criteria,
                        CriteriaText = filter.CriteriaText,
                        IsExport = filter.IsExport,
                        OPTypeID = filter.OPTypeID,
                        RequestModeID = filter.RequestModeID,
                        CircleID = filter.CircleID,
                        SwitchID = filter.SwitchID,
                    };
                    try
                    {
                        if (filter.Criteria > 0)
                        {
                            if ((filter.CriteriaText ?? "") == "")
                            {
                                return _lst;
                            }
                        }
                        if (_filter.Criteria == Criteria.OutletMobile)
                        {
                            if (!validate.IsMobile(_filter.CriteriaText))
                            {
                                return _lst;
                            }
                            _filter.OutletNo = _filter.CriteriaText;
                        }
                        if (_filter.Criteria == Criteria.AccountNo)
                        {
                            _filter.AccountNo = _filter.CriteriaText;
                            if (string.IsNullOrEmpty(_filter.AccountNo))
                                return _lst;
                            if (ApplicationSetting.IsSingleDB)
                                _filter.FromDate = DateTime.Now.AddMonths(-2).ToString("dd MMM yyyy");
                            else
                                IsScanMoreDB = true;

                        }
                        if (_filter.Criteria == Criteria.TransactionID)
                        {
                            if (_filter.CriteriaText[0] != Criteria.StartCharOFTransaction)
                            {
                                return _lst;
                            }
                            _filter.TransactionID = _filter.CriteriaText;
                        }
                        if (_filter.Criteria == Criteria.TID)
                        {
                            try
                            {
                                _filter.TID = Convert.ToInt32(_filter.CriteriaText);
                            }
                            catch (Exception)
                            {
                                return _lst;
                            }
                        }
                        if (_filter.Criteria == Criteria.APIRequestID)
                        {
                            _filter.APIRequestID = _filter.CriteriaText;
                        }
                        if (_filter.Criteria == Criteria.VendorID)
                        {
                            _filter.VendorID = _filter.CriteriaText;
                        }
                        if (_filter.Criteria == Criteria.CCID)
                        {
                            if (!validate.IsNumeric(_filter.CriteriaText))
                            {
                                return _lst;
                            }
                            _filter.CCID = Convert.ToInt32(_filter.CriteriaText);
                        }
                        if (_filter.Criteria == Criteria.CCMobileNo)
                        {
                            _filter.CCMobileNo = _filter.CriteriaText;
                        }
                        if (_filter.Criteria == Criteria.LiveID)
                        {
                            _filter.LiveID = _filter.CriteriaText;
                        }
                        if (_filter.Criteria == Criteria.UserID)
                        {
                            var Prefix = Validate.O.Prefix(filter.CriteriaText);
                            if (Validate.O.IsNumeric(Prefix))
                                _filter.UserID = Validate.O.IsNumeric(filter.CriteriaText) ? Convert.ToInt32(filter.CriteriaText) : _filter.UserID;
                            var uid = Validate.O.LoginID(filter.CriteriaText);
                            _filter.UserID = Validate.O.IsNumeric(uid) ? Convert.ToInt32(uid) : _filter.UserID;
                        }
                    }
                    catch (Exception)
                    {
                        return _lst;
                    }

                    _filter.FromDate = string.IsNullOrEmpty(_filter.FromDate) ? DateTime.Now.ToString("dd MMM yyyy") : _filter.FromDate;

                    IProcedureAsync _proc = new ProcselectHotelReport(_dal);
                    return _lst = (List<BookingDetails>)await _proc.Call(_filter).ConfigureAwait(false);

                }

            }
            return _lst;
        }


        public async Task<HotelReceipt> HotelReceipt(int BookingID, int TID)
        {
            var creq = new CommonReq()
            {
                CommonInt = BookingID,
                CommonInt2 = TID,
                CommonInt3 = _lr.UserID
            };
            IProcedureAsync _proc = new ProcHotelReceipt(_dal);
            return (HotelReceipt)await _proc.Call(creq).ConfigureAwait(false);
        }




        public async Task<RechargeReportSummary> GetHotelSummary(int RType)
        {
            if (RType == 0)
            {
                RType = ReportType.Recharge;
            }
            var _res = new RechargeReportSummary();

            var loginResp = chkAlternateSession();
            if (loginResp != null)
            {
                if (loginResp.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.ShowRechargeReport))
                {
                    var procReq = new CommonReq
                    {
                        LoginID = loginResp.LoginTypeID == LoginType.CustomerCare ? 1 : loginResp.UserID,
                        CommonInt = RType
                    };
                    IProcedureAsync _proc = new ProcHotelReportSummary(_dal);
                    _res = (RechargeReportSummary)await _proc.Call(procReq);
                }
            }
            return _res;
        }



        public async Task<List<ProcUserLedgerResponse>> GetUserPackageLimitLedgerrList(ULedgerReportFilter filter)
        {
            var _lst = new List<ProcUserLedgerResponse>();
            var loginResp = chkAlternateSession();
            if (loginResp != null)
            {
                if (loginResp.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.ShowUserLedger) || loginResp.LoginTypeID == LoginType.Employee)
                {
                    var validate = Validate.O;
                    if ((filter.TransactionId_F ?? "") != "" && !validate.IsTransactionIDValid((filter.TransactionId_F ?? "")))
                    {
                        return _lst;
                    }
                    var _req = new ProcUserLedgerRequest
                    {
                        LT = loginResp.LoginTypeID,
                        LoginID = loginResp.UserID,
                        Mobile_F = filter.Mobile_F ?? "",
                        DebitCredit_F = filter.DebitCredit_F,
                        FromDate_F = !string.IsNullOrEmpty(filter.FromDate_F) ? filter.FromDate_F : null,
                        ToDate_F = !string.IsNullOrEmpty(filter.ToDate_F) ? filter.ToDate_F : null,
                        TransactionId_F = filter.TransactionId_F,
                        TopRows = filter.TopRows,
                        WalletTypeID = filter.WalletTypeID
                    };

                    if (filter.Mobile_F != "")
                    {
                        if (!Validate.O.IsMobile(filter.Mobile_F))
                        {
                            var Prefix = Validate.O.Prefix(filter.Mobile_F);
                            if (Validate.O.IsNumeric(Prefix))
                                _req.UserID = Validate.O.IsNumeric(filter.Mobile_F) ? Convert.ToInt32(filter.Mobile_F) : _req.UserID;
                            var uid = Validate.O.LoginID(filter.Mobile_F);
                            _req.UserID = Validate.O.IsNumeric(uid) ? Convert.ToInt32(uid) : _req.UserID;
                            _req.Mobile_F = "";
                        }
                    }
                    _req.FromDate_F = string.IsNullOrEmpty(_req.FromDate_F) ? DateTime.Now.ToString("dd MMM yyyy") : _req.FromDate_F;
                    _dal = ChangeConString(_req.FromDate_F);
                    IProcedureAsync _proc = new ProcUserPackageLimitLedger(_dal);
                    _lst = (List<ProcUserLedgerResponse>)await _proc.Call(_req);
                }
            }
            return _lst;
        }

        public IEnumerable<MasterPGateway> GetActivePaymentGateway()
        {
            var _MasterPG = new List<MasterPGateway>();
            IProcedure _proc = new ProcGetActivePaymentGateway(_dal);
            _MasterPG = (List<MasterPGateway>)_proc.Call();
            return _MasterPG;
        }



        #region RailwayOutlet
        public async Task<IEnumerable<GetEditUser>> GetRailKYCUsers(UserRequest req)
        {
            var res = new List<GetEditUser>();
            if (_lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser)
            {
                req.LTID = _lr.LoginTypeID;
                req.LoginID = _lr.UserID;
                IProcedureAsync proc = new ProcGetRailwayKycPendings(_dal);
                res = (List<GetEditUser>)await proc.Call(req);
            }
            return res;
        }

        public IResponseStatus ActivateRailPending(int outletID, string IRCTCID, int Status)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (_lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser)
            {
                var _req = new CommonReq
                {
                    LoginTypeID = _lr.LoginTypeID,
                    LoginID = _lr.UserID,
                    CommonInt = outletID,
                    CommonInt2 = Status == 2 ? Status : 3,
                    CommonStr = IRCTCID
                };
                IProcedure _proc = new ProcUpdateRailwayKYCStatus(_dal);
                _res = (ResponseStatus)_proc.Call(_req);
            }
            return _res;
        }
        #endregion

        public async Task<List<MasterPaymentGateway>> GetMasterPaymentGateway(int id)
        {
            IProcedureAsync _proc = new ProcGetMasterPaymentGateway(_dal);
            var resp = (List<MasterPaymentGateway>)await _proc.Call(id).ConfigureAwait(false);
            return resp ?? new List<MasterPaymentGateway>();
        }
        public async Task<List<PaymentGateway>> GetPaymentGateway(CommonReq filter)
        {
            IProcedureAsync _proc = new ProcGetPaymentGateway(_dal);
            var resp = (List<PaymentGateway>)await _proc.Call(filter).ConfigureAwait(false);
            return resp ?? new List<PaymentGateway>();
        }

        public IResponseStatus UpdateMasterPaymentGateway(MasterPaymentGateway AddData)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            IProcedure _proc = new ProcUpdateMasterPaymentGateway(_dal);
            _res = (ResponseStatus)_proc.Call(AddData);
            return _res;
        }

        public IResponseStatus UpdatePaymentGateway(PaymentGateway AddData)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            IProcedure _proc = new ProcUpdatePaymentGateway(_dal);
            _res = (ResponseStatus)_proc.Call(AddData);
            return _res;
        }
       
        public AgreementApprovalNotification GetAgreementApprovalNotification(int UserID)
        {
            IProcedure _proc = new ProcGetAgreementApprovalNotification(_dal);
            var resp = (AgreementApprovalNotification)_proc.Call(UserID);
            return resp ?? new AgreementApprovalNotification();
        }

        public async Task<BillFetchSummary> GetBillFetchSummary(int UserID)
        {
            IProcedureAsync _proc = new ProcBillFetchSummary(_dal);
            var resp = (BillFetchSummary)await _proc.Call(UserID);
            return resp ?? new BillFetchSummary();
        }
    }
}
