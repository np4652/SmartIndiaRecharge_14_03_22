using Fintech.AppCode;
using Fintech.AppCode.Configuration;
using Fintech.AppCode.DB;
using Fintech.AppCode.HelperClass;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using Fintech.AppCode.WebRequest;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using RoundpayFinTech.AppCode.Configuration;
using RoundpayFinTech.AppCode.DL;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.ThirdParty.WhatsappAPI;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Validators;

namespace RoundpayFinTech.AppCode.MiddleLayer
{
    public class CallbackML : ICallbackML, IShoppingCallback
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        private readonly ISession _session;
        private readonly ConnectionConfiguration _c;
        private readonly IDAL _dal;
        private readonly LoginResponse _lr;
        private readonly IRequestInfo _info;
        private readonly IUserML userML;
        private readonly IErrorCodeMLParent _errorCodeML;
        public CallbackML(IHttpContextAccessor accessor, IHostingEnvironment env, bool InSession = true)
        {
            _accessor = accessor;
            _env = env;
            _c = new ConnectionConfiguration(_accessor, _env);
            _session = _accessor.HttpContext.Session;
            _dal = new DAL(_c.GetConnectionString());
            _info = new RequestInfo(_accessor, _env);
            _errorCodeML = new ErrorCodeML(_dal);
            if (InSession)
            {
                _lr = _session.GetObjectFromJson<LoginResponse>(SessionKeys.LoginResponse);
                userML = new UserML(_lr);
            }
        }


        public async Task<string> LogCallBackRequest(CallbackData callbackData, bool IpValidation = true)
        {
            callbackData.RequestIP = _info.GetRemoteIP();
            callbackData.RequestBrowser = _info.GetBrowserFullInfo();
            IProcedureAsync _procLog = new ProcSaveCallbackData(_dal);
            if (((bool)await _procLog.Call(callbackData).ConfigureAwait(false)) || !IpValidation)
            {
                var _res = await TransactionUpdate(callbackData.Content, callbackData.APIID).ConfigureAwait(false);
                if (_res.Statuscode != ErrorCodes.Minus1)
                {
                    if ((_res.SCode ?? string.Empty) != "GIS")
                    {
                        if (_res.IsInternalSender)
                        {
                            //var senderRequest = (SenderRequest)new ProcGetSender(_dal).Call(_res.Optional2);
                            //Only for Internal Sender
                            new AlertML(_accessor, _env, false).PayoutSMS(new AlertReplacementModel
                            {
                                LoginID = _res.UserID,
                                UserMobileNo = _res.Optional2,
                                WID = 1,
                                Amount = _res.RequestedAmount,
                                AccountNo = _res.AccountNo,
                                SenderName = _res.O10,
                                TransMode = _res.Optional4,
                                UTRorRRN = _res.LiveID,
                                IFSC = _res.Optional3,
                                BrandName = _res.BrandName
                            });
                        }
                        if (_res.RequestMode == RequestMode.API && !string.IsNullOrEmpty(_res.UpdateUrl))
                        {
                            var _ = UpdateAPIURLHitting(_res);
                        }
                        if ((_res.RequestMode.In(RequestMode.APPS, RequestMode.SMS)) || (_res.RequestMode == RequestMode.API && !string.IsNullOrEmpty(_res.UpdateUrl)) || _res.RefundStatus == RefundType.REFUNDED)
                        {
                            if (_res.TransactionStatus.In(RechargeRespType.SUCCESS, RechargeRespType.FAILED) || _res.RefundStatus == RefundType.REFUNDED)
                            {
                                bool IsSuccess = false;
                                IAlertML alertMl = new AlertML(_accessor, _env);
                                if (_res.TransactionStatus == RechargeRespType.SUCCESS)
                                {
                                    IsSuccess = true;
                                }
                                var alertParam = new AlertReplacementModel
                                {
                                    AccountNo = _res.AccountNo,
                                    UserMobileNo = _res.MobileNo,
                                    Amount = _res.RequestedAmount,
                                    UserCurrentBalance = _res.Balance,
                                    Operator = _res.Operator,
                                    LiveID = _res.LiveID,
                                    WID = _res.WID,
                                    UserFCMID = _res.FCMID,
                                    LoginID = _res.LoginID,
                                    TransactionID = _res.TransactionID,
                                    LoginCurrentBalance = _res.APIBalance,
                                    Company = _res.Company,
                                    CompanyDomain = _res.CompanyDomain,
                                    SupportEmail = _res.SupportEmail,
                                    SupportNumber = _res.SupportNumber,
                                    AccountEmail = _res.AccountEmail,
                                    AccountsContactNo = _res.AccountContact,
                                    UserID = _res.UserID,
                                    UserName = _res.UserName,
                                    OutletName = _res.OutletName,
                                    CompanyAddress = _res.CompanyAddress,
                                    BrandName = _res.BrandName,
                                    RefundStatus = _res.RefundStatus,
                                    WhatsappNo = _res.UserWhatsappNo,
                                    HangoutNo = _res.UserHangout,
                                    TelegramNo = _res.UserTelegram,
                                    UserEmailID = _res.UserEmailID,
                                    WhatsappConversationID = _res.ConversationID
                                };
                                if (string.IsNullOrEmpty(alertParam.WhatsappNo))
                                {
                                    alertParam.WhatsappNo = alertParam.UserMobileNo;
                                }
                                bool IsRejected = alertParam.RefundStatus == RefundType.REFUNDED ? false : true;
                                if (RefundType.REFUNDED == alertParam.RefundStatus)
                                {
                                    alertParam.FormatID = MessageFormat.RechargeRefund;
                                    Parallel.Invoke(() => alertMl.RechargeRefundSMS(alertParam, IsRejected),
                                   () => alertMl.RechargeRefundEmail(alertParam, IsRejected),
                                   () => alertMl.RechargeRefundNotification(alertParam, IsRejected),
                                  async () => await alertMl.WebNotification(alertParam).ConfigureAwait(false),
                                  async () => await alertMl.SocialAlert(alertParam).ConfigureAwait(false));

                                }
                                if (RefundType.REJECTED == alertParam.RefundStatus)
                                {
                                    alertParam.FormatID = MessageFormat.RechargeRefundReject;
                                    Parallel.Invoke(() => alertMl.RechargeRefundSMS(alertParam, IsRejected),
                                   () => alertMl.RechargeRefundEmail(alertParam, IsRejected),
                                   () => alertMl.RechargeRefundNotification(alertParam, IsRejected),
                                  async () => await alertMl.WebNotification(alertParam).ConfigureAwait(false),
                                  async () => await alertMl.SocialAlert(alertParam).ConfigureAwait(false));
                                }
                                if (_res.RequestMode == RequestMode.APPS)
                                {
                                    alertMl.RecharegeSuccessNotification(alertParam, IsSuccess);
                                }
                                if (_res.RequestMode == RequestMode.SMS)
                                {
                                    alertMl.RecharegeSuccessSMS(alertParam, IsSuccess);
                                }
                            }
                        }
                        if (_res.TransactionStatus == RechargeRespType.SUCCESS && _res.IsBBPS && string.IsNullOrEmpty(_res.CustomerNumber))
                        {
                            var alertParam = new AlertReplacementModel
                            {
                                WID = _res.WID,
                                AccountNo = _res.AccountNo,
                                UserMobileNo = _res.CustomerNumber,
                                Amount = _res.RequestedAmount,
                                Operator = _res.Operator,
                                TransactionID = _res.TransactionID,
                                FormatID = MessageFormat.BBPSSuccess,
                                DATETIME = DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt")
                            };
                            IAlertML alertMl = new AlertML(_accessor, _env);
                            alertMl.BBPSSuccessSMS(alertParam);
                        }
                    }
                    else
                    {
                        AlertReplacementModel param = new AlertReplacementModel();
                        param.CouponCode = _res.LiveID;
                        param.Amount = _res.RequestedAmount;
                        param.CouponQty = _res.TotalToken;
                        param.CouponValdity = 0;
                        param.UserID = _res.UserID;
                        param.UserName = _res.UserName;
                        param.UserEmailID = _res.Optional2;
                        param.LoginID = _res.UserID;
                        param.WID = _res.WID;
                        param.FormatID = 34;
                        var a = new AlertML(_accessor, _env).CouponVocherEmail(param, true);
                    }
                }
            }
            return "Final";
        }
        public async Task<bool> LogCallBackRequestBool(CallbackData callbackData)
        {
            callbackData.RequestIP = _info.GetRemoteIP();
            callbackData.RequestBrowser = _info.GetBrowserFullInfo();
            IProcedureAsync _procLog = new ProcSaveCallbackData(_dal);
            return (bool)await _procLog.Call(callbackData).ConfigureAwait(false);
        }
        public void MakeMakeFileLog(CallbackData callbackData)
        {
            try
            {
                callbackData.RequestIP = _info.GetRemoteIP();
                callbackData.RequestBrowser = _info.GetBrowserFullInfo();
                string root = @"Image/CallBackLog";
                if (!Directory.Exists(root))
                {
                    Directory.CreateDirectory(root);
                }
                string path = root + "/RP" + Guid.NewGuid();
                StringBuilder sb = new StringBuilder("****************************************************************************Header***********************************************************************\n");
                sb.Append(DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss.ffff tt"));
                sb.Append("\tMethod: ");
                sb.Append(callbackData.Method);
                sb.Append("\tPath: ");
                sb.Append(callbackData.Path);
                sb.Append("\tScheme: ");
                sb.Append(callbackData.Scheme);
                sb.Append("\tRequestIP: ");
                sb.Append(callbackData.RequestIP);
                sb.Append("\tRequestBrowser: ");
                sb.Append(callbackData.RequestBrowser);
                sb.Append("\tAPIID: ");
                sb.Append(callbackData.APIID);
                sb.Append("\n********************************************************************************************************************************************************");
                sb.Append("\n\n\tContent:\n");
                sb.Append((callbackData.Content ?? string.Empty));
                using (StreamWriter file = new StreamWriter(path, true))
                {
                    file.Write(sb.ToString());
                }

            }
            catch (Exception)
            {
            }
        }
        private async Task<_CallbackData> TransactionUpdate(string content, int APIID)
        {
            var apistatuscheck = new APISTATUSCHECK
            {
                Msg = Validate.O.ReplaceAllSpecials(content).Trim()
            };
            var callbackData = new _CallbackData();
            bool IsTextRes = true;
            if (APIID > 0)
            {
                IProcedure _procAPI = new ProcGetAPI(_dal);
                var apiRes = (APIDetail)_procAPI.Call(new CommonReq
                {
                    CommonInt = APIID,
                    LoginID = 1
                });
                if (!string.IsNullOrEmpty(apiRes.HookStatusKey) && (!string.IsNullOrEmpty(apiRes.HookVendorKey) || !string.IsNullOrEmpty(apiRes.HookTIDKey)) && apiRes.HookResTypeID.In(ResponseType.JSON, ResponseType.XML, ResponseType.Delimiter))
                {
                    TransactionHelper transactionHelper = new TransactionHelper(_dal, _accessor, _env);
                    IsTextRes = false;
                    var ds = new DataSet();
                    if (apiRes.HookResTypeID == ResponseType.JSON)
                    {
                        ds = ToDataSet.O.ReadDataFromJson(content);
                    }
                    else if (apiRes.HookResTypeID == ResponseType.XML)
                    {
                        ds = ToDataSet.O.ReadDataFromXML(content);
                    }
                    else if (apiRes.HookResTypeID == ResponseType.Delimiter)
                    {
                        if (!string.IsNullOrEmpty(apiRes.HookFirstDelimiter))
                        {
                            if (content.Contains(apiRes.HookFirstDelimiter) && !string.IsNullOrEmpty(apiRes.HookSecondDelimiter))
                            {
                                IsTextRes = content.Contains(apiRes.HookSecondDelimiter) == false;
                            }
                        }
                    }
                    if (ds.Tables.Count > 0)
                    {
                        bool IsStatusFound = false, IsTIDFound = string.IsNullOrEmpty(apiRes.HookTIDKey), IsVendorIDFound = string.IsNullOrEmpty(apiRes.HookVendorKey), IsOperatorIDFound = string.IsNullOrEmpty(apiRes.HookLiveIDKey), IsMessageFound = string.IsNullOrEmpty(apiRes.HookMsgKey);
                        foreach (DataTable tbl in ds.Tables)
                        {
                            if (!IsStatusFound)
                            {

                                var STn = transactionHelper.CheckIfKeyExistsInDatatable(tbl, apiRes.HookStatusKey);
                                if (STn.CommonBool)
                                {
                                    IsStatusFound = true;
                                    apistatuscheck.Statuscode = ErrorCodes.One;
                                    string StatusValue = tbl.Rows[0][STn.CommonStr].ToString().ToUpper();
                                    if (!string.IsNullOrEmpty(apiRes.HookSuccessCode))
                                    {
                                        var sucValArr = apiRes.HookSuccessCode.ToUpper().Split(',');
                                        apistatuscheck.Status = sucValArr.Contains(StatusValue) ? RechargeRespType.SUCCESS : apistatuscheck.Status;
                                        if (apistatuscheck.Status == RechargeRespType.PENDING && !string.IsNullOrEmpty(apiRes.HookFailCode))
                                        {
                                            var failValArr = apiRes.HookFailCode.ToUpper().Split(',');
                                            apistatuscheck.Status = failValArr.Contains(StatusValue) ? RechargeRespType.FAILED : apistatuscheck.Status;
                                        }
                                    }
                                }
                            }
                            if (!IsOperatorIDFound)
                            {
                                var LVd = transactionHelper.CheckIfKeyExistsInDatatable(tbl, apiRes.HookLiveIDKey);
                                if (LVd.CommonBool)
                                {
                                    IsOperatorIDFound = true;
                                    apistatuscheck.OperatorID = tbl.Rows[0][LVd.CommonStr].ToString();
                                }
                            }
                            if (!IsVendorIDFound)
                            {
                                var VnD = transactionHelper.CheckIfKeyExistsInDatatable(tbl, apiRes.HookVendorKey);
                                if (VnD.CommonBool)
                                {
                                    IsVendorIDFound = true;
                                    apistatuscheck.VendorID = tbl.Rows[0][VnD.CommonStr].ToString();
                                }
                            }

                            if (!IsMessageFound && apistatuscheck.Status != RechargeRespType.SUCCESS)
                            {
                                var MGK = transactionHelper.CheckIfKeyExistsInDatatable(tbl, apiRes.HookMsgKey);
                                if (MGK.CommonBool)
                                {
                                    IsMessageFound = true;
                                    apistatuscheck.ErrorCode = tbl.Rows[0][MGK.CommonStr].ToString();
                                }
                            }
                            if (IsStatusFound && IsVendorIDFound && IsOperatorIDFound && IsMessageFound)
                            {
                                break;
                            }
                        }
                    }
                    if (apiRes.HookResTypeID == ResponseType.Delimiter)
                    {
                        if (apistatuscheck.Status == 0)
                            apistatuscheck.Status = RechargeRespType.PENDING;
                        var dick = transactionHelper.GetResponseForDelimeter(content, apiRes.HookFirstDelimiter, apiRes.HookSecondDelimiter);
                        if (dick.Count > 0)
                        {
                            string StatusValue;
                            if (!string.IsNullOrEmpty(apiRes.HookSuccessCode))
                            {
                                apistatuscheck.Statuscode = ErrorCodes.One;
                                if (dick.TryGetValue(apiRes.HookStatusKey, out StatusValue))
                                {
                                    var sucValArr = apiRes.HookSuccessCode.ToUpper().Split(',');
                                    apistatuscheck.Status = sucValArr.Contains(StatusValue) ? RechargeRespType.SUCCESS : apistatuscheck.Status;
                                    if (apistatuscheck.Status == RechargeRespType.PENDING && !string.IsNullOrEmpty(apiRes.HookFailCode))
                                    {
                                        var failValArr = apiRes.HookFailCode.ToUpper().Split(',');
                                        apistatuscheck.Status = failValArr.Contains(StatusValue) ? RechargeRespType.FAILED : apistatuscheck.Status;
                                    }
                                }
                            }

                            string LiveIDValue;
                            if (!string.IsNullOrEmpty(apiRes.HookLiveIDKey))
                            {
                                if (dick.TryGetValue(apiRes.HookLiveIDKey, out LiveIDValue))
                                {
                                    apistatuscheck.OperatorID = LiveIDValue;
                                }
                            }

                            string VendorID;
                            if (!string.IsNullOrEmpty(apiRes.HookVendorKey))
                            {
                                if (dick.TryGetValue(apiRes.HookVendorKey, out VendorID))
                                {
                                    apistatuscheck.VendorID = VendorID;
                                }
                            }
                            string _TID;
                            if (!string.IsNullOrEmpty(apiRes.HookTIDKey))
                            {
                                if (dick.TryGetValue(apiRes.HookTIDKey, out _TID))
                                {
                                    apistatuscheck.TransactionID = _TID;
                                }
                            }
                            if (apistatuscheck.Status != RechargeRespType.SUCCESS)
                            {
                                string MsgKeyVal;
                                if (!string.IsNullOrEmpty(apiRes.HookMsgKey))
                                {
                                    if (dick.TryGetValue(apiRes.HookMsgKey, out MsgKeyVal))
                                    {
                                        apistatuscheck.ErrorMsg = MsgKeyVal;
                                    }
                                }

                                string ErrorCodeKeyVal;
                                if (!string.IsNullOrEmpty(apiRes.HookErrorCodeKey))
                                {
                                    if (dick.TryGetValue(apiRes.HookErrorCodeKey, out ErrorCodeKeyVal))
                                    {
                                        apistatuscheck.ErrorCode = ErrorCodeKeyVal;
                                    }
                                }
                            }
                        }
                        else
                        {
                            IsTextRes = true;
                        }
                    }
                }
            }
            if (IsTextRes)
            {
                var _proc = new ProcCheckTextResponse(_dal);
                apistatuscheck = (APISTATUSCHECK)await _proc.Call(apistatuscheck).ConfigureAwait(false);
            }
            if (apistatuscheck.Statuscode == ErrorCodes.One)
            {
                if (apistatuscheck.Status.In(RechargeRespType.FAILED, RechargeRespType.SUCCESS))
                {

                    callbackData.LiveID = apistatuscheck.OperatorID.Length > 50 ? apistatuscheck.OperatorID.Substring(0, 50) : apistatuscheck.OperatorID;
                    callbackData.VendorID = apistatuscheck.VendorID;
                    callbackData.TID = Validate.O.IsNumeric(apistatuscheck.TransactionID) ? Convert.ToInt32(apistatuscheck.TransactionID) : 0;
                    if (apistatuscheck.Status.In(RechargeRespType.FAILED))
                    {
                        if (string.IsNullOrEmpty(apistatuscheck.ErrorCode) && !string.IsNullOrEmpty(apistatuscheck.ErrorMsg))
                        {
                            var _proc = new ProcCheckTextResponse(_dal);
                            var iRespMatcher = (APISTATUSCHECK)await _proc.Call(new APISTATUSCHECK
                            {
                                Msg = Validate.O.ReplaceAllSpecials(apistatuscheck.ErrorMsg).Trim()
                            }).ConfigureAwait(false);
                            if (!string.IsNullOrEmpty(iRespMatcher.ErrorCode))
                            {
                                apistatuscheck.ErrorCode = iRespMatcher.ErrorCode;
                                callbackData.Msg = apistatuscheck.ErrorMsg;
                            }
                            else
                            {
                                apistatuscheck.ErrorCode = ErrorCodes.Unknown_Error.ToString();
                            }
                        }
                        else if (!string.IsNullOrEmpty(apistatuscheck.ErrorCode))
                        {
                            apistatuscheck.ErrorCode = apistatuscheck.ErrorCode;
                        }
                        else
                        {
                            apistatuscheck.ErrorCode = ErrorCodes.Unknown_Error.ToString();
                        }
                    }
                    else
                    {
                        apistatuscheck.ErrorCode = ErrorCodes.Transaction_Successful.ToString();
                    }
                    var errResp = _errorCodeML.Get(apistatuscheck.ErrorCode.Replace(" ", string.Empty) ?? "");
                    if (errResp.ErrType == ErrType.BillFetch)
                    {
                        apistatuscheck.Status = RechargeRespType.PENDING;
                    }
                    callbackData.TransactionStatus = apistatuscheck.Status;
                    if (callbackData.TransactionStatus == RechargeRespType.FAILED)
                    {
                        callbackData.LiveID = callbackData.Msg;
                    }
                    callbackData.ErrorCode = errResp.Code;
                    errResp.Error = errResp.Error.Replace(Replacement.AccountKey, "Account/Mobile Number");
                    callbackData.Msg = errResp.Error.Replace(Replacement.REPLACE, apistatuscheck.ErrorMsg);

                    callbackData.RequestPage = "Callback";
                    callbackData.LoginID = 1;
                    callbackData.LT = 1;
                    callbackData.RequestIP = _info.GetRemoteIP();
                    callbackData.Browser = _info.GetBrowserFullInfo();
                    callbackData.APIID = APIID;


                    if (!ApplicationSetting.IsSingleDB)
                    {
                        IProcedureAsync procAPIResponse = new ProcAPIResponse(_dal);
                        var callbck = (_CallbackData)await procAPIResponse.Call(callbackData).ConfigureAwait(false);
                        if (callbck.Statuscode == ErrorCodes.Minus1 && callbck.Msg.StartsWith("(IUR)"))
                        {
                            var _ddal = new DAL(_c.GetConnectionString(1));
                            procAPIResponse = new ProcAPIResponse(_ddal);
                            callbackData = (_CallbackData)await procAPIResponse.Call(callbackData).ConfigureAwait(false);
                            if (callbackData.Statuscode == ErrorCodes.Minus1 && callbackData.Msg.StartsWith("(IUR)"))
                            {
                                var _ddall = new DAL(_c.GetConnectionString(2, DateTime.Now.AddMonths(-1).ToString("MM_YYYY")));
                                procAPIResponse = new ProcAPIResponse(_ddall);
                                callbackData = (_CallbackData)await procAPIResponse.Call(callbackData).ConfigureAwait(false);
                            }
                        }
                        else
                        {
                            callbackData = callbck;
                        }
                    }
                    else
                    {
                        IProcedureAsync procAPIResponse = new ProcAPIResponse(_dal);
                        callbackData = (_CallbackData)await procAPIResponse.Call(callbackData).ConfigureAwait(false);
                    }
                }
            }
            return callbackData;
        }
        public async Task<bool> UpdateAPIURLHitting(_CallbackData _callbackData)
        {
            string MethodName = "UpdateAPIURLHitting";
            string Resp = "";
            try
            {
                Resp = await AppWebRequest.O.CallUsingHttpWebRequest_GETAsync(_callbackData.UpdateUrl).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Resp = ex.Message;
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = MethodName,
                    Error = ex.Message,
                    LoginTypeID = _callbackData.LT,
                    UserId = _callbackData.UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            IProcedureAsync _proc = new ProcUpdateAPIURLHitting(_dal);
            var _req = new APIURLHitting
            {
                UserID = _callbackData.UserID,
                TransactionID = _callbackData.TransactionID,
                URL = _callbackData.UpdateUrl,
                Response = Resp
            };
            await _proc.Call(_req).ConfigureAwait(false);
            return true;
        }
        public ResponseStatusBalnace GetUserBal(int ApiID, string ApiOutletID)
        {
            var _res = new ResponseStatusBalnace
            {
                Status = RechargeRespType._FAILED,
                Msg = ErrorCodes.AnError
            };
            if (string.IsNullOrEmpty(ApiOutletID) || ApiOutletID.Length < 4)
            {
                _res.Msg = "Invalid Rail id";
                return _res;
            }
            var CommReq = new CommonReq
            {
                CommonInt2 = ServiceType.Travel,
                CommonInt = ApiID,
                CommonStr = ApiOutletID,
                CommonStr2 = _info.GetRemoteIP()
            };
            IProcedure proc = new ProcGetUserfromOutletID(_dal);
            _res = (ResponseStatusBalnace)proc.Call(CommReq);
            SaveCallBack(JsonConvert.SerializeObject(_res).ToString());
            return _res;
        }
        #region ShoppingReq
        public ResponseStatus GenerateShoppingOTP(ShoppingOTPReq shoppingOTPReq)
        {
            shoppingOTPReq.IPAddress = _info.GetRemoteIP();
            var OTP = HashEncryption.O.CreatePasswordNumeric(6);
            shoppingOTPReq.OTP = HashEncryption.O.DevEncrypt(OTP);
            IProcedure proc = new ProcShopingOTPRequest(_dal);
            var res = (ResponseStatus)proc.Call(shoppingOTPReq);
            if (res.Statuscode == ErrorCodes.One)
            {
                IUserML uml = new UserML(_accessor, _env);
                var alertData = uml.GetUserDeatilForAlert(res.CommonInt2);
                if (alertData.Statuscode == ErrorCodes.One)
                {
                    IAlertML alertMl = new AlertML(_accessor, _env);
                    alertData.OTP = OTP;
                    alertMl.OTPSMS(alertData);
                    alertMl.OTPEmail(alertData);
                }
            }
            return res;
        }

        public ResponseStatus MatchShoppingOTP(ShoppingOTPReq shoppingOTPReq)
        {
            shoppingOTPReq.IPAddress = _info.GetRemoteIP();
            shoppingOTPReq.OTP = HashEncryption.O.DevEncrypt(shoppingOTPReq.OTP);
            IProcedure proc = new ProcMatchShoppingOTP(_dal);
            var res = (ResponseStatus)proc.Call(shoppingOTPReq);
            return res;
        }
        #endregion
        public ResponseStatusBalnace ServiceTransaction(ServiceTransactionRequest Req)
        {
            var _res = new ResponseStatusBalnace
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            if (!Req.IsBalance)
            {
                if (Req.AmountR < 1)
                {
                    _res.Msg = "Invalid Amount";
                    return _res;
                }
                if (string.IsNullOrEmpty(Req.AccountNo))
                {
                    _res.Msg = "Invalid Account";
                    return _res;
                }

                if (string.IsNullOrEmpty(Req.VenderID))
                {
                    _res.Msg = "Invalid VenderID";
                    return _res;
                }
            }

            Req.RequestIP = _info.GetRemoteIP();
            IProcedure proc = new ProcServiceTransaction(_dal);
            _res = (ResponseStatusBalnace)proc.Call(Req);
            SaveCallBack(JsonConvert.SerializeObject(_res).ToString());
            return _res;
        }
        public void SaveCallBack(string Url)
        {
            var req = new CommonReq
            {
                CommonStr = Url,
                CommonStr2 = _info.GetRemoteIP()
            };
            IProcedure proc = new procUpdateCallback(_dal);
            proc.Call(req);
        }
        public IResponseStatus CheckUser(CommonReq req)
        {
            IProcedure proc = new ProcMatchUserIDPassword(_dal);
            return (IResponseStatus)proc.Call(req);
        }
        public CallBackOTP RequestOTP(string MerchantID, string UserID, string Password, string ApiRequestID)
        {
            string MobileNo = "";
            int ID = 0;
            if (Validate.O.IsMobile(UserID ?? ""))
                MobileNo = UserID;
            else
                ID = Convert.ToInt32(UserID);
            var Apires = new CallBackOTP
            {
                Status = "FAILED",
                Msg = "Invalid request parameter"
            };
            if ((ID == 0 && MobileNo == "") || (Password ?? "").Length < 6 || (MerchantID ?? "").Length < 6 || (ApiRequestID ?? "").Length < 1)
            {
                SaveCallBack(JsonConvert.SerializeObject(Apires).ToString());
                return Apires;
            }
            var req = new CommonReq
            {
                LoginID = ID,
                CommonStr = Password,
                str = MerchantID,
                CommonStr2 = MobileNo,
                CommonStr3 = ApiRequestID,
            };
            IResponseStatus res = CheckUser(req);
            SaveCallBack(JsonConvert.SerializeObject(res).ToString());
            if (res.Statuscode == ErrorCodes.Minus1)
            {
                Apires.Msg = "Invalid userID or password ";
                SaveCallBack(JsonConvert.SerializeObject(Apires).ToString());
                return Apires;
            }
            ILoginML _loginML = new LoginML(_accessor, _env);
            _loginML.SendOTP(res.CommonStr4, res.CommonStr, res.CommonStr2, res.CommonInt2);
            Apires.Msg = res.Msg;
            Apires.UserID = res.CommonInt;
            Apires.ReferenceID = res.CommonStr3;
            Apires.Status = "SUCCESS";
            return Apires;
        }
        public IResponseStatus ValidateOTP(CommonReq req)
        {
            var Apires = new CallBackOTP
            {
                Status = "PANDING",
                Msg = "Something went wrong"
            };
            IProcedure proc = new ProcValidateOTP(_dal);
            return (IResponseStatus)proc.Call(req);
        }
        public async Task<string> GetLapuTransactions(string APIName)
        {
            string LastTransaction = "<LastTransaction><Table><Status>Record Not Found</Status></Table></LastTransaction>";
            try
            {
                IProcedureAsync _proc = new ProcLapuTransaction(_dal);
                var statuses = (List<LastTransaction.Status>)await _proc.Call(APIName).ConfigureAwait(false);
                if (statuses.Count > 0)
                {
                    var lastTransaction = new LastTransaction
                    {
                        Table = statuses
                    };
                    LastTransaction = XMLHelper.O.SerializeToXml(lastTransaction, null);
                }
            }
            catch (Exception ex)
            {
                LastTransaction = "<LastTransaction><Table><Status>" + ex.Message + "</Status></Table></LastTransaction>";
            }
            return LastTransaction;
        }
        public async Task<string> GetLapuSocialAlert(string APIName, int SocialAlertType)
        {
            string LastTransaction = "<LastTransactionSMS><Table><Status>Record Not Found</Status></Table></LastTransactionSMS>";
            try
            {


                var req = new CommonReq
                {

                    CommonStr = APIName,
                    CommonInt = SocialAlertType
                };
                IProcedureAsync _proc = new ProcLapuTransactionSocial(_dal);
                var statuses = (List<LastTransactionSMS.Status>)await _proc.Call(req).ConfigureAwait(false);
                if (statuses.Count > 0)
                {
                    var lastTransaction = new LastTransactionSMS
                    {
                        Table = statuses
                    };
                    LastTransaction = XMLHelper.O.SerializeToXml(lastTransaction, null);
                }
            }
            catch (Exception ex)
            {
                LastTransaction = "<LastTransactionSMS><Table><Status>" + ex.Message + "</Status></Table></LastTransactionSMS>";
            }
            return LastTransaction;
        }
        public async Task<IEnumerable<PSADetailForMachine>> GetPSADetailMachine(string Machine)
        {
            IProcedureAsync _proc = new ProcGetPSAForMachine(_dal);
            return (List<PSADetailForMachine>)await _proc.Call(Machine).ConfigureAwait(false);
        }
        public void LogPGCallback(PGCallbackData pGCallbackData)
        {
            ProcSaveCallbackData procSaveCallbackData = new ProcSaveCallbackData(_dal);
            procSaveCallbackData.SavePaymentGatewayLog(pGCallbackData);
        }
        public bool ValidateCallbackIP()
        {
            ProcGetIPAddress procGetIPAddress = new ProcGetIPAddress(_dal);
            return procGetIPAddress.ValidateCallBackIPFromDB(_info.GetRemoteIP());
        }

        public ResponseStatus UpdatePSAFromMachine(_CallbackData _Callback)
        {
            _Callback.RequestIP = _info.GetRemoteIP();
            _Callback.Browser = _info.GetBrowserFullInfo();
            IProcedure proc = new ProcUpdatePSAFromMachine(_dal);
            return (ResponseStatus)proc.Call(_Callback);
        }
        public WhatsappReceiveMsgResp ProcSaveWhatsappReceiceMessage(WhatsappReceiveMsgResp response)
        {
            var res = new WhatsappReceiveMsgResp();
            IProcedure proc = new ProcSaveWhatsappReceiceMessage(_dal);
            return res = (WhatsappReceiveMsgResp)proc.Call(response);
        }

        public WhatsappReceiveMsgResp ProcSaveWhatsappReceiceMessageAlertHub(WhatsappAlertHubCallBack response)
        {
            WhatsappReceiveMsgResp req = new WhatsappReceiveMsgResp();
            req.waId = response.jid;
            req.senderName = "";
            req.type = response.msgtype.ToLower();
            req.statusString = WhatsappStatusString.Received;
            req.SenderNoID = response.SenderNoID;
            req.conversationId = response.Info.msgid;
            if (response.is_group_msg == 1)
            {
                req.waId = response.groupid;
                req.ReceiveMsgMobileNo = response.jid;
            }
            if (response.Quoted != null)
            {
                req.QuotedMsgID = response.Quoted.msgid;
            }
            if (response.msgtype == "TEXT")
            {
                req.text = response.Info.message;
            }
            else if (response.msgtype == "IMAGE")
            {
                req.text = response.Info.caption;
                req.data = response.Info.imgurl;
            }
            IProcedure proc = new ProcSaveWhatsappReceiceMessage(_dal);
            return (WhatsappReceiveMsgResp)proc.Call(req);
        }
        public WhatsappReceiveMsgResp ProcSaveWhatsappReceiceMessageWATeam(WATeamCallBackRes response)
        {
            WhatsappReceiveMsgResp _response = new WhatsappReceiveMsgResp();
            _response.SenderNoID = response.SenderID;

            foreach (var item in response.contacts)
            {
                _response.senderName = item.profile.name;
            }
            foreach (var item in response.messages)
            {
                _response.conversationId = item.id;
                _response.waId = item.from;
                _response.type = item.type;
                _response.statusString = WhatsappStatusString.Received;
                if (item.type == "text")
                {
                    _response.text = item.text.body;
                }
                if (item.type == "image")
                {
                    _response.text = "";
                    _response.data = item.image.id;
                }
            }
            IProcedure proc = new ProcSaveWhatsappReceiceMessage(_dal);
            return (WhatsappReceiveMsgResp)proc.Call(_response);
        }
    }
}
