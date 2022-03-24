using Fintech.AppCode.Configuration;
using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using Fintech.AppCode.WebRequest;
using FinTech_EasyAllPay.AppCode.MiddleLayer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using RoundpayFinTech;
using RoundpayFinTech.AppCode.DL;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.MiddleLayer;
using RoundpayFinTech.AppCode.MiddleLayer.Dmt_Api;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.BBPS;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.StaticModel.MoneyTransfer;
using RoundpayFinTech.AppCode.ThirdParty.AxisBank;
using RoundpayFinTech.AppCode.ThirdParty.BillAvenue;
using RoundpayFinTech.AppCode.ThirdParty.CyberPlate;
using RoundpayFinTech.AppCode.ThirdParty.EasyPay;
using RoundpayFinTech.AppCode.ThirdParty.Mahagram;
using RoundpayFinTech.AppCode.ThirdParty.PanMitra;
using RoundpayFinTech.AppCode.ThirdParty.PayU;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Validators;

namespace Fintech.AppCode.HelperClass
{
    public class TransactionHelper
    {
        private readonly IDAL _dal;
        private readonly ToDataSet _toDataSet;
        private readonly IErrorCodeMLParent _errCodeML;
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        public TransactionHelper(IDAL dal, IHttpContextAccessor accessor, IHostingEnvironment env)
        {
            _dal = dal;
            _toDataSet = ToDataSet.O;
            _errCodeML = new ErrorCodeML(_dal);
            _accessor = accessor;
            _env = env;
        }
        #region TransactionRelated
        public async Task<TransactionStatus> HitGetStatus(int TID, TransactionServiceReq TransRequest, APIDetail currentAPI, List<APIDetail> moreAPIs, bool IsReplaceCurrent, TransactionServiceResp TransResponse)
        {
            var tstatus = new TransactionStatus
            {
                Status = RechargeRespType.PENDING
            };
            if (currentAPI.APIType == APITypes.Lapu)
                return tstatus;
            if (currentAPI.APIType == APITypes.Manual)
            {
                tstatus.Status = RechargeRespType.SUCCESS;
                return tstatus;
            }
            var moreAPIsTemp = new List<APIDetail>
            {
                currentAPI
            };

            if (moreAPIs != null)
            {
                moreAPIsTemp.AddRange(moreAPIs);
            }
            moreAPIs = moreAPIsTemp;
            //if (moreAPIs.Count > 1)
            //    moreAPIs.Reverse();

            foreach (var API in moreAPIs)
            {
                var _HitAPI = new RechargeAPIHit
                {
                    aPIDetail = API
                };
                _HitAPI.aPIDetail.IsBBPS = currentAPI.IsBBPS;
                tstatus.APIID = _HitAPI.aPIDetail.ID;
                tstatus.APIType = _HitAPI.aPIDetail.APIType;
                tstatus.APIOpCode = _HitAPI.aPIDetail.APIOpCode;
                tstatus.APIName = _HitAPI.aPIDetail.Name;
                tstatus.APICommType = _HitAPI.aPIDetail.CommType;
                tstatus.APIComAmt = _HitAPI.aPIDetail.Comm;
                tstatus.APIGroupCode = _HitAPI.aPIDetail.GroupCode;
                tstatus.SwitchingID = _HitAPI.aPIDetail.SwitchingID;

                if (tstatus.APIType == APITypes.Lapu)
                {
                    tstatus.Status = RechargeRespType.PENDING;
                    break;
                }
                if (_HitAPI.aPIDetail.ID != currentAPI.ID || IsReplaceCurrent)
                {
                    _HitAPI = URLReplacement(TID, TransRequest, _HitAPI, TransResponse.IsBBPS && TransResponse.IsBillValidation);
                }
                else
                {
                    _HitAPI.aPIDetail.URL = API.URL;
                    _HitAPI.aPIDetail.ValidateURL = API.ValidateURL;
                }
                if (_HitAPI.aPIDetail.APICode == APICode.CYBERPLAT)
                {
                    _HitAPI.CPTRNXRequest = new CyberPlatRequestModel
                    {
                        NUMBER = TransRequest.AccountNo,
                        SESSION = "A" + TID,
                    };
                    _HitAPI.CPTRNXRequest.AMOUNT = TransRequest.AmountR.ToString();
                    #region RegionForAOMapping
                    var bBPSLog = new BBPSLog
                    {
                        Optional1 = TransRequest.Optional1,
                        Optional2 = TransRequest.Optional2,
                        Optional3 = TransRequest.Optional3,
                        Optional4 = TransRequest.Optional4
                    };
                    bBPSLog.APIOptionalList = AOPMapping(_HitAPI.aPIDetail.ID, TransRequest.OID);
                    new CyberPlateML(_dal).MatchOptionalParam(bBPSLog);
                    _HitAPI.CPTRNXRequest.ACCOUNT = bBPSLog.AccountNumber;
                    _HitAPI.CPTRNXRequest.Authenticator3 = bBPSLog.Authenticator3;
                    #endregion
                }
                if (_HitAPI.aPIDetail.APICode == APICode.JUSTRECHARGE)
                {
                    _HitAPI.JRRechReq = new JRRechargeReq
                    {
                        rURL = _HitAPI.aPIDetail.URL,
                        Mobile = TransRequest.AccountNo,
                        Provider = _HitAPI.aPIDetail.APIOpCode,
                        Location = "",
                        Amount = Convert.ToInt32(TransRequest.AmountR),
                        ServiceType = _HitAPI.aPIDetail.RechType,
                        SystemReference = TID.ToString(),
                        IsPostpaid = TransRequest.IsPostpaid ? "Y" : "N",
                        NickName = ""
                    };
                }
                if (_HitAPI.aPIDetail.APICode == APICode.EASYPAY)
                {
                    var easypayML = new EasypayML(_accessor, _env, _dal);
                    _HitAPI = easypayML.MakePaymentURL(TID, TransRequest, _HitAPI);
                }

                var rechargeAPIHit = await HitRechargeAPI(_HitAPI, TransRequest, TransResponse).ConfigureAwait(false);

                await UpdateAPIResponse(TID, rechargeAPIHit).ConfigureAwait(false);

                if (!string.IsNullOrEmpty(rechargeAPIHit.Response) && !rechargeAPIHit.IsException)
                {
                    tstatus = await MatchResponse(TransRequest.OID, rechargeAPIHit, TransResponse.AccountNoKey).ConfigureAwait(false);
                }
                else
                {
                    tstatus.Status = RechargeRespType.PENDING;
                }
                if (tstatus.Status == RechargeRespType.FAILED)
                {
                    if (!tstatus.IsResend)
                    {
                        break;
                    }
                }
                else
                {
                    break;
                }
            }
            return tstatus;
        }
        public async Task<TransactionStatus> MatchResponse(int OID, RechargeAPIHit rechargeAPIHit, string AccountNoKey, bool IsValidation = false)
        {
            const string MethodName = "MatchResponse";
            var tstatus = new TransactionStatus
            {
                Status = RechargeRespType.PENDING,
                APIID = rechargeAPIHit.aPIDetail.ID,
                APIType = rechargeAPIHit.aPIDetail.APIType,
                APIOpCode = rechargeAPIHit.aPIDetail.APIOpCode,
                APIName = rechargeAPIHit.aPIDetail.Name,
                APICommType = rechargeAPIHit.aPIDetail.CommType,
                APIComAmt = rechargeAPIHit.aPIDetail.Comm,
                APIGroupCode = rechargeAPIHit.aPIDetail.GroupCode,
                ErrorCode = ErrorCodes.Request_Accpeted.ToString(),
                ErrorMsg = nameof(ErrorCodes.Request_Accpeted),
                SwitchingID = rechargeAPIHit.aPIDetail.SwitchingID
            };
            try
            {
                var IsOtherCondition = false;
                var OtherConditionMatchString = string.Empty;
                var errorCodes = new ErrorCodeDetail();
                var ds = new DataSet();
                if (rechargeAPIHit.aPIDetail.ResponseTypeID == ResponseType.JSON)
                {
                    ds = _toDataSet.ReadDataFromJson(rechargeAPIHit.Response);
                }
                else if (rechargeAPIHit.aPIDetail.ResponseTypeID == ResponseType.XML)
                {
                    ds = _toDataSet.ReadDataFromXML(rechargeAPIHit.Response);
                }
                else if (rechargeAPIHit.aPIDetail.ResponseTypeID == ResponseType.Delimiter)
                {
                    IsOtherCondition = true;
                    if (!string.IsNullOrEmpty(rechargeAPIHit.aPIDetail.FirstDelimiter))
                    {
                        if (rechargeAPIHit.Response.Contains(rechargeAPIHit.aPIDetail.FirstDelimiter) && !string.IsNullOrEmpty(rechargeAPIHit.aPIDetail.SecondDelimiter))
                        {
                            IsOtherCondition = rechargeAPIHit.Response.Contains(rechargeAPIHit.aPIDetail.SecondDelimiter) == false;
                        }
                    }
                }
                else
                {
                    #region MatchResponseFor String,CSV and Other
                    IsOtherCondition = true;
                    OtherConditionMatchString = rechargeAPIHit.Response ?? string.Empty;
                    #endregion
                }
                #region MatchResponseForXML&JSON
                var IsErrorCodeFound = false;
                var IsMessageFound = false;
                if (ds.Tables.Count > 0 && !IsOtherCondition)
                {
                    bool IsStatusFound = string.IsNullOrEmpty(rechargeAPIHit.aPIDetail.StatusName), IsVendorIDFound = string.IsNullOrEmpty(rechargeAPIHit.aPIDetail.VendorID), IsOperatorIDFound = string.IsNullOrEmpty(rechargeAPIHit.aPIDetail.LiveID), IsRefKeyFound = string.IsNullOrEmpty(rechargeAPIHit.aPIDetail.RefferenceKey);
                    foreach (DataTable tbl in ds.Tables)
                    {
                        if (!IsStatusFound)
                        {
                            if (IsValidation)
                            {
                                var VSK = CheckIfKeyExistsInDatatable(tbl, rechargeAPIHit.aPIDetail.ValidationStatusKey);

                                if (VSK.CommonBool)
                                {
                                    IsStatusFound = true;
                                    string StatusValue = tbl.Rows[0][VSK.CommonStr].ToString().ToUpper();
                                    if (!string.IsNullOrEmpty(rechargeAPIHit.aPIDetail.ValidationStatusValue))
                                    {
                                        var sucValArr = rechargeAPIHit.aPIDetail.ValidationStatusValue.ToUpper().Split(',');
                                        tstatus.Status = sucValArr.Contains(StatusValue) ? RechargeRespType.SUCCESS : tstatus.Status;
                                    }
                                }
                            }
                            else
                            {
                                var STn = CheckIfKeyExistsInDatatable(tbl, rechargeAPIHit.aPIDetail.StatusName);
                                if (STn.CommonBool)
                                {
                                    IsStatusFound = true;
                                    string StatusValue = tbl.Rows[0][STn.CommonStr].ToString().ToUpper();
                                    if (!string.IsNullOrEmpty(rechargeAPIHit.aPIDetail.SuccessCode))
                                    {
                                        var sucValArr = rechargeAPIHit.aPIDetail.SuccessCode.ToUpper().Split(',');
                                        tstatus.Status = sucValArr.Contains(StatusValue) ? RechargeRespType.SUCCESS : tstatus.Status;
                                        if (tstatus.Status == RechargeRespType.PENDING && !string.IsNullOrEmpty(rechargeAPIHit.aPIDetail.FailCode))
                                        {
                                            var failValArr = rechargeAPIHit.aPIDetail.FailCode.ToUpper().Split(',');
                                            tstatus.Status = failValArr.Contains(StatusValue) ? RechargeRespType.FAILED : tstatus.Status;
                                        }
                                    }
                                }
                            }
                        }
                        if (!IsOperatorIDFound)
                        {
                            var LVd = CheckIfKeyExistsInDatatable(tbl, rechargeAPIHit.aPIDetail.LiveID);
                            if (LVd.CommonBool)
                            {
                                IsOperatorIDFound = true;
                                tstatus.OperatorID = tbl.Rows[0][LVd.CommonStr].ToString();
                            }
                        }
                        if (!IsVendorIDFound)
                        {
                            var VnD = CheckIfKeyExistsInDatatable(tbl, rechargeAPIHit.aPIDetail.VendorID);
                            if (VnD.CommonBool)
                            {
                                IsVendorIDFound = true;
                                tstatus.VendorID = tbl.Rows[0][VnD.CommonStr].ToString();
                            }
                        }
                        if (!IsErrorCodeFound && tstatus.Status != RechargeRespType.SUCCESS)
                        {
                            var ECD = CheckIfKeyExistsInDatatable(tbl, rechargeAPIHit.aPIDetail.ErrorCodeKey);
                            if (ECD.CommonBool)
                            {
                                IsErrorCodeFound = true;
                                tstatus.APIErrorCode = tbl.Rows[0][ECD.CommonStr].ToString();
                            }
                        }
                        if (!IsMessageFound && tstatus.Status != RechargeRespType.SUCCESS)
                        {
                            var MGK = CheckIfKeyExistsInDatatable(tbl, rechargeAPIHit.aPIDetail.MsgKey);
                            if (MGK.CommonBool)
                            {
                                IsMessageFound = true;
                                tstatus.APIMsg = tbl.Rows[0][MGK.CommonStr].ToString();
                            }
                        }
                        var RFK = CheckIfKeyExistsInDatatable(tbl, rechargeAPIHit.aPIDetail.RefferenceKey);
                        if (!IsRefKeyFound && RFK.CommonBool)
                        {
                            IsRefKeyFound = true;
                            tstatus.RefferenceID = tbl.Rows[0][RFK.CommonStr].ToString();
                        }
                        if (IsStatusFound && IsVendorIDFound && IsOperatorIDFound && IsErrorCodeFound && IsMessageFound && IsRefKeyFound)
                            break;
                    }
                }
                if (rechargeAPIHit.aPIDetail.ResponseTypeID == ResponseType.Delimiter && IsOtherCondition == false)
                {
                    var dick = GetResponseForDelimeter(rechargeAPIHit.Response, rechargeAPIHit.aPIDetail.FirstDelimiter, rechargeAPIHit.aPIDetail.SecondDelimiter);
                    if (dick.Count > 0)
                    {
                        string StatusValue;
                        if (!string.IsNullOrEmpty(rechargeAPIHit.aPIDetail.SuccessCode))
                        {
                            if (dick.TryGetValue(rechargeAPIHit.aPIDetail.StatusName, out StatusValue))
                            {
                                var sucValArr = rechargeAPIHit.aPIDetail.SuccessCode.ToUpper().Split(',');
                                tstatus.Status = sucValArr.Contains(StatusValue) ? RechargeRespType.SUCCESS : tstatus.Status;
                                if (tstatus.Status == RechargeRespType.PENDING && !string.IsNullOrEmpty(rechargeAPIHit.aPIDetail.FailCode))
                                {
                                    var failValArr = rechargeAPIHit.aPIDetail.FailCode.ToUpper().Split(',');
                                    tstatus.Status = failValArr.Contains(StatusValue) ? RechargeRespType.FAILED : tstatus.Status;
                                }
                            }
                        }

                        string LiveIDValue;
                        if (!string.IsNullOrEmpty(rechargeAPIHit.aPIDetail.LiveID))
                        {
                            if (dick.TryGetValue(rechargeAPIHit.aPIDetail.LiveID, out LiveIDValue))
                            {
                                tstatus.OperatorID = LiveIDValue;
                            }
                        }

                        string VendorID;
                        if (!string.IsNullOrEmpty(rechargeAPIHit.aPIDetail.VendorID))
                        {
                            if (dick.TryGetValue(rechargeAPIHit.aPIDetail.VendorID, out VendorID))
                            {
                                tstatus.VendorID = VendorID;
                            }
                        }
                        if ( tstatus.Status != RechargeRespType.SUCCESS)
                        {
                            string ErrorCodeKeyVal;
                            if (!string.IsNullOrEmpty(rechargeAPIHit.aPIDetail.ErrorCodeKey))
                            {
                                if (dick.TryGetValue(rechargeAPIHit.aPIDetail.ErrorCodeKey, out ErrorCodeKeyVal))
                                {
                                    tstatus.APIErrorCode = ErrorCodeKeyVal;
                                    IsErrorCodeFound = true;
                                }
                            }
                            string MsgKeyVal;
                            if (!string.IsNullOrEmpty(rechargeAPIHit.aPIDetail.MsgKey))
                            {
                                if (dick.TryGetValue(rechargeAPIHit.aPIDetail.MsgKey, out MsgKeyVal))
                                {
                                    tstatus.APIMsg = MsgKeyVal;
                                }
                            }
                        }
                        string RefferenceKeyVal;
                        if (!string.IsNullOrEmpty(rechargeAPIHit.aPIDetail.RefferenceKey))
                        {
                            if (dick.TryGetValue(rechargeAPIHit.aPIDetail.RefferenceKey, out RefferenceKeyVal))
                            {
                                tstatus.RefferenceID = RefferenceKeyVal;
                            }
                        }
                    }
                    else
                    {
                        IsOtherCondition = true;
                    }
                }

                if (tstatus.Status.In(RechargeRespType.PENDING, RechargeRespType.FAILED) && (ds.Tables.Count > 0 || rechargeAPIHit.aPIDetail.ResponseTypeID == ResponseType.Delimiter) && !IsOtherCondition)
                {
                    if (IsErrorCodeFound && (tstatus.APIErrorCode ?? string.Empty).Length > 0 && (tstatus.APIGroupCode ?? string.Empty).Length > 0)
                    {
                        var apiErrorCodes = _errCodeML.GetAPIErrorCode(new APIErrorCode { APICode = tstatus.APIErrorCode, GroupCode = tstatus.APIGroupCode });
                        if (tstatus.Status == RechargeRespType.FAILED)
                        {
                            apiErrorCodes.ECode = string.IsNullOrEmpty(apiErrorCodes.ECode) ? ErrorCodes.Unknown_Error.ToString() : apiErrorCodes.ECode;
                        }
                        if ((apiErrorCodes.ECode ?? string.Empty).Length > 0)
                        {
                            errorCodes = _errCodeML.Get(apiErrorCodes.ECode);
                            if (errorCodes.Status == RechargeRespType.FAILED && tstatus.Status == RechargeRespType.PENDING)
                            {
                                tstatus.Status = RechargeRespType.FAILED;
                            }
                        }
                    }
                    else if (IsMessageFound)
                    {
                        IsOtherCondition = true;
                        OtherConditionMatchString = tstatus.APIMsg;
                    }
                }
                if (IsOtherCondition && (OtherConditionMatchString ?? string.Empty).Trim().Length > 0)
                {
                    var apistatuscheck = new APISTATUSCHECK
                    {
                        Msg = Validate.O.ReplaceAllSpecials(OtherConditionMatchString).Trim()
                    };
                    var _proc = new ProcCheckTextResponse(_dal);
                    apistatuscheck = (APISTATUSCHECK)await _proc.Call(apistatuscheck).ConfigureAwait(false);
                    if (apistatuscheck.Statuscode == ErrorCodes.One)
                    {
                        tstatus.Status = apistatuscheck.Status.In(RechargeRespType.PENDING, RechargeRespType.FAILED, RechargeRespType.SUCCESS) ? apistatuscheck.Status : RechargeRespType.PENDING;
                        if (tstatus.Status == RechargeRespType.FAILED)
                        {
                            errorCodes = _errCodeML.Get(string.IsNullOrEmpty(apistatuscheck.ErrorCode) ? ErrorCodes.Unknown_Error.ToString() : apistatuscheck.ErrorCode);
                        }
                        else
                        {
                            errorCodes = _errCodeML.Get(string.IsNullOrEmpty(apistatuscheck.ErrorCode) ? ErrorCodes.Request_Accpeted.ToString() : apistatuscheck.ErrorCode);
                            if (errorCodes.Status == RechargeRespType.FAILED && tstatus.Status == RechargeRespType.PENDING)
                            {
                                tstatus.Status = RechargeRespType.FAILED;
                            }
                        }

                        if (errorCodes.ErrType == ErrType.BillFetch)
                        {
                            tstatus.Status = RechargeRespType.PENDING;
                        }
                        if (tstatus.Status == RechargeRespType.FAILED)
                        {
                            if (errorCodes.Status == 0)
                            {
                                apistatuscheck.OperatorID = string.IsNullOrEmpty(apistatuscheck.ErrorCode) ? (errorCodes.Error ?? nameof(ErrorCodes.Unknown_Error).Replace("_", " ")) : nameof(ErrorCodes.Unknown_Error).Replace("_", " ");
                                errorCodes.Code = apistatuscheck.ErrorCode = string.IsNullOrEmpty(apistatuscheck.ErrorCode) ? ErrorCodes.Unknown_Error.ToString() : apistatuscheck.ErrorCode;
                                errorCodes.Error = apistatuscheck.OperatorID;
                            }
                            else
                            {
                                apistatuscheck.OperatorID = errorCodes.Error;
                            }
                        }
                        tstatus.OperatorID = apistatuscheck.OperatorID;
                        tstatus.VendorID = apistatuscheck.VendorID;
                        tstatus.ErrorCode = errorCodes.Code;
                        tstatus.ErrorMsg = errorCodes.Error;
                    }
                }
                if (errorCodes.Status > 0)
                {
                    tstatus.ErrorCode = errorCodes.Code;
                    tstatus.ErrorMsg = (errorCodes.Error ?? string.Empty).Replace(Replacement.REPLACE, tstatus.APIMsg);
                    if (!string.IsNullOrEmpty(AccountNoKey))
                    {
                        tstatus.ErrorMsg = tstatus.ErrorMsg.Replace(Replacement.AccountKey, AccountNoKey);
                    }
                    else
                    {
                        tstatus.ErrorMsg = tstatus.ErrorMsg.Replace(Replacement.AccountKey, "Account/Mobile Number");
                    }
                    if (errorCodes.Status == RechargeRespType.FAILED)
                    {
                        tstatus.Status = RechargeRespType.FAILED;
                        tstatus.OperatorID = tstatus.ErrorMsg;

                        if (errorCodes.IsDown && rechargeAPIHit.aPIDetail.IsOpDownAllow)
                        {
                            await UpdateAPIDownStatus(OID, rechargeAPIHit.aPIDetail.ID).ConfigureAwait(false);
                            var errorLog = new ErrorLog
                            {
                                ClassName = GetType().Name,
                                FuncName = MethodName,
                                Error = "APIDown:" + rechargeAPIHit.aPIDetail.ID + ":" + rechargeAPIHit.aPIDetail.IsOpDownAllow,
                                LoginTypeID = LoginType.ApplicationUser,
                                UserId = OID
                            };
                            var _ = new ProcPageErrorLog(_dal).Call(errorLog);
                        }
                        else if (errorCodes.IsResend)
                        {
                            tstatus.IsResend = true;
                            var errorLog = new ErrorLog
                            {
                                ClassName = GetType().Name,
                                FuncName = MethodName,
                                Error = "ResendCondFound:" + (errorCodes.Error ?? "") + "|" + rechargeAPIHit.aPIDetail.ID,
                                LoginTypeID = LoginType.ApplicationUser,
                                UserId = OID
                            };
                            var _ = new ProcPageErrorLog(_dal).Call(errorLog);
                        }
                    }
                    else if (errorCodes.Status == RechargeRespType.SUCCESS)
                    {
                        tstatus.Status = RechargeRespType.SUCCESS;
                    }
                }
                if (tstatus.Status == RechargeRespType.SUCCESS)
                {
                    tstatus.ErrorCode = ErrorCodes.Transaction_Successful.ToString();
                    tstatus.ErrorMsg = nameof(ErrorCodes.Transaction_Successful);

                }
                #endregion
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = MethodName,
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = rechargeAPIHit.aPIDetail.ID
                };
                tstatus.Status = RechargeRespType.PENDING;
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            tstatus.ErrorCode = tstatus.ErrorCode ?? string.Empty;
            if (tstatus.Status == RechargeRespType.FAILED && (tstatus.ErrorCode.Equals(ErrorCodes.Request_Accpeted.ToString()) || string.IsNullOrEmpty(tstatus.ErrorCode)))
            {
                tstatus.ErrorCode = ErrorCodes.Unknown_Error.ToString();
                tstatus.ErrorMsg = nameof(ErrorCodes.Unknown_Error);
            }
            if (tstatus.Status == RechargeRespType.FAILED && !tstatus.IsResend && tstatus.ErrorCode.Equals(ErrorCodes.Unknown_Error.ToString()))
            {
                var errorCodes = _errCodeML.Get(ErrorCodes.Unknown_Error.ToString());
                tstatus.IsResend = errorCodes.IsResend;
                tstatus.ErrorMsg = errorCodes.Error;
                tstatus.OperatorID = errorCodes.Error;
            }
            return tstatus;
        }
        public IDictionary<string, string> GetResponseForDelimeter(string response, string FirstDelem, string SecondDelem)
        {
            var dicDelim = new Dictionary<string, string>();
            if (response[0] == '?')
            {
                response = response.Substring(1, response.Length - 1);
            }
            var splitFirst = response.Split(FirstDelem);
            foreach (var first in splitFirst)
            {
                var secondSplit = first.Split(SecondDelem);
                if (secondSplit.Length == 2)
                {
                    dicDelim.Add(secondSplit[0].ToString().Trim(), secondSplit[1].ToString().Trim());
                }
            }
            return dicDelim;
        }
        public ResponseStatus CheckIfKeyExistsInDatatable(DataTable dt, string Key)
        {
            var res = new ResponseStatus();
            if (dt != null && !string.IsNullOrEmpty(Key))
            {
                if (Key.Contains("."))
                {
                    var root = Key.Split('.')[0];
                    res.CommonStr = Key.Split('.')[1];
                    if (dt.TableName == root)
                    {
                        res.CommonBool = dt.Columns.Contains(res.CommonStr);
                    }
                }
                else
                {
                    res.CommonStr = Key;
                    res.CommonBool = dt.Columns.Contains(Key);
                }
            }
            return res;
        }
        public async Task<RechargeAPIHit> HitRechargeAPI(RechargeAPIHit rechargeAPI, TransactionServiceReq TransRequest, TransactionServiceResp TransResponse)
        {
            TransRequest.IMEI = GenerateRandomSessionNo(15);
            var rechargeAPIHit = new RechargeAPIHit()
            {
                aPIDetail = rechargeAPI.aPIDetail
            };
            try
            {
                if (rechargeAPI.aPIDetail.APICode == APICode.CYBERPLAT)
                {
                    rechargeAPIHit.Response = await CallCyberPlat(rechargeAPI).ConfigureAwait(false);
                }
                else if (rechargeAPI.aPIDetail.APICode == APICode.JUSTRECHARGE)
                {
                    JustRechargeIT_ML objJustRechargeIT_ML = new JustRechargeIT_ML();
                    rechargeAPIHit.Response = await objJustRechargeIT_ML.JRCorporateLogin(this, rechargeAPI).ConfigureAwait(false);
                }
                else if (rechargeAPI.aPIDetail.APICode == APICode.EASYPAY)
                {
                    var easypayML = new EasypayML(_accessor, _env, _dal);
                    rechargeAPIHit.Response = easypayML.DoPayment(rechargeAPI);
                }
                else if (rechargeAPI.aPIDetail.APICode == APICode.MAHAGRAM)
                {
                    var mhagmML = new MahagramAPIML(_accessor, _env, _dal);
                    var MGReq = new MGCouponRequest
                    {
                        requestid = rechargeAPI.MGPSARequestID
                    };
                    var HitReqResp = mhagmML.UTICouponStatus(MGReq);
                    rechargeAPIHit.aPIDetail.URL = HitReqResp.Request;
                    rechargeAPIHit.Response = HitReqResp.Response;
                }
                else if (rechargeAPI.aPIDetail.APICode == APICode.BILLAVENUE)
                {
                    var billAML = new BillAvenueML(_accessor, _env, _dal);
                    if (TransRequest.IsStatusCheck)
                    {
                        var HitReqResp = billAML.BillPaymentStatusFromTID(TransRequest.TID, TransRequest.TransactionReqID);
                        rechargeAPIHit.aPIDetail.URL = HitReqResp.Request;
                        rechargeAPIHit.Response = HitReqResp.Response;
                    }
                    else
                    {
                        var HitReqResp = billAML.BillPayement(new BBPSPaymentRequest
                        {
                            PaymentMode = TransRequest.PaymentMode,
                            BillerID = TransResponse.BillerID,
                            billerAdhoc = TransResponse.BillerAdhoc,
                            TID = TransResponse.TID,
                            TransactionID = TransResponse.TransactionID,
                            APIOutletID = rechargeAPI.aPIDetail.APIOutletID,
                            CustomerMobile = TransRequest.CustomerNumber,
                            InitChanel = TransResponse.InitChanel,
                            MAC = TransResponse.MAC,
                            IMEI = TransRequest.IMEI,
                            IPAddress = TransRequest.RequestIP,
                            FetchBillResponse = TransResponse.BillFetchResponse,
                            OpParams = TransResponse.OpParams,
                            AInfos = TransResponse.AInfos,
                            AccountNo = TransRequest.AccountNo,
                            Optional1 = TransRequest.Optional1,
                            Optional2 = TransRequest.Optional2,
                            Optional3 = TransRequest.Optional3,
                            Optional4 = TransRequest.Optional4,
                            ExactNess = TransResponse.ExactNess,
                            RequestedAmount = TransRequest.AmountR,
                            BillDate = TransResponse.BillDate,
                            DueDate = TransResponse.DueDate,
                            EarlyPaymentDate = TransResponse.EarlyPaymentDate,
                            EarlyPaymentAmountKey = TransResponse.EarlyPaymentAmountKey,
                            LatePaymentAmountKey = TransResponse.LatePaymentAmountKey,
                            CCFAmount = TransRequest.CCFAmount,
                            PaymentModeInAPI = TransResponse.PaymentModeInAPI,
                            CaptureInfo = TransResponse.CaptureInfo,
                            UserMobileNo = TransResponse.UserMobileNo,
                            UserName = TransResponse.UserName,
                            UserEmailID = TransResponse.UserEmailID,
                            IsINT = TransResponse.IsINT,
                            IsMOB = TransResponse.IsMOB,
                            IsQuickPay = TransResponse.IsBilling == false,
                            IsValidation = TransResponse.IsBillValidation
                        });
                        if (HitReqResp.IsValidaionResponseAlso)
                        {
                            if (!HitReqResp.IsBillValidated)
                            {
                                rechargeAPIHit.aPIDetail.URL = HitReqResp.RequestV;
                                rechargeAPIHit.Response = HitReqResp.ResponseV;
                            }
                            else
                            {
                                var rechargeAPITemp = rechargeAPIHit;
                                rechargeAPITemp.aPIDetail.URL = HitReqResp.RequestV;
                                rechargeAPITemp.Response = HitReqResp.ResponseV;
                                var _ = UpdateAPIResponse(TransResponse.TID, rechargeAPITemp);
                                rechargeAPIHit.aPIDetail.URL = HitReqResp.Request;
                                rechargeAPIHit.Response = HitReqResp.Response;
                            }
                        }
                        else
                        {
                            rechargeAPIHit.aPIDetail.URL = HitReqResp.Request;
                            rechargeAPIHit.Response = HitReqResp.Response;
                        }
                    }
                }
                else if (rechargeAPI.aPIDetail.APICode == APICode.AXISBANK)
                {
                    var axBML = new AxisBankBBPSML(_accessor, _env, _dal);
                    if (TransRequest.IsStatusCheck)
                    {
                        var HitReqResp = axBML.AxisBankGetPaymentStatus(TransRequest.APIContext);
                        rechargeAPIHit.aPIDetail.URL = HitReqResp.Request;
                        rechargeAPIHit.Response = HitReqResp.Response;
                    }
                    else
                    {
                        var HitReqResp = axBML.AxisBankMakePayment(new BBPSPaymentRequest
                        {
                            PaymentMode = TransRequest.PaymentMode,
                            BillerID = TransResponse.BillerID,
                            billerAdhoc = TransResponse.BillerAdhoc,
                            TID = TransResponse.TID,
                            TransactionID = TransResponse.TransactionID,
                            APIOutletID = rechargeAPI.aPIDetail.APIOutletID,
                            CustomerMobile = TransRequest.CustomerNumber,
                            InitChanel = TransResponse.InitChanel,
                            MAC = TransResponse.MAC,
                            IPAddress = TransRequest.RequestIP,
                            FetchBillResponse = TransResponse.BillFetchResponse,
                            OpParams = TransResponse.OpParams,
                            AInfos = TransResponse.AInfos,
                            AccountNo = TransRequest.AccountNo,
                            Optional1 = TransRequest.Optional1,
                            Optional2 = TransRequest.Optional2,
                            Optional3 = TransRequest.Optional3,
                            Optional4 = TransRequest.Optional4,
                            ExactNess = TransResponse.ExactNess,
                            RequestedAmount = TransRequest.AmountR,
                            BillDate = TransResponse.BillDate,
                            DueDate = TransResponse.DueDate,
                            EarlyPaymentDate = TransResponse.EarlyPaymentDate,
                            EarlyPaymentAmountKey = TransResponse.EarlyPaymentAmountKey,
                            LatePaymentAmountKey = TransResponse.LatePaymentAmountKey,
                            CCFAmount = TransRequest.CCFAmount,
                            PaymentModeInAPI = TransResponse.PaymentModeInAPI,
                            CaptureInfo = TransResponse.CaptureInfo,
                            UserMobileNo = TransResponse.UserMobileNo,
                            IsINT = TransResponse.IsINT,
                            IsMOB = TransResponse.IsMOB,
                            IsQuickPay = TransResponse.IsBilling == false,
                            IsValidation = TransResponse.IsBillValidation,
                            APIContext = TransResponse.APIContext,
                            AccountHolder = TransResponse.AccountHolder,
                            APIOpType = TransResponse.APIOpType,
                            AccountNoKey = TransResponse.AccountNoKey,
                            GeoLocation = TransRequest.GEOCode,
                            Pincode = TransResponse.Pincode
                        }).Result;
                        if (HitReqResp.IsValidaionResponseAlso)
                        {
                            if (!HitReqResp.IsBillValidated)
                            {
                                rechargeAPIHit.aPIDetail.URL = HitReqResp.RequestV;
                                rechargeAPIHit.Response = HitReqResp.ResponseV;
                            }
                            else
                            {
                                var rechargeAPITemp = rechargeAPIHit;
                                rechargeAPITemp.aPIDetail.URL = HitReqResp.RequestV;
                                rechargeAPITemp.Response = HitReqResp.ResponseV;
                                var _ = UpdateAPIResponse(TransResponse.TID, rechargeAPITemp);
                                rechargeAPIHit.aPIDetail.URL = HitReqResp.Request;
                                rechargeAPIHit.Response = HitReqResp.Response;
                            }
                        }
                        else
                        {
                            rechargeAPIHit.aPIDetail.URL = HitReqResp.Request;
                            rechargeAPIHit.Response = HitReqResp.Response;
                        }
                    }

                }
                else if (rechargeAPI.aPIDetail.APICode == APICode.PAYUBBPS)
                {
                    var payuBBPSML = new PayuBBPSML(_accessor, _env, _dal);
                    if (TransRequest.IsStatusCheck)
                    {
                        var HitReqResp = payuBBPSML.BillPaymentStatus(TransRequest.TID);
                        rechargeAPIHit.aPIDetail.URL = HitReqResp.Request;
                        rechargeAPIHit.Response = HitReqResp.Response;
                    }
                    else
                    {
                        var HitReqResp = payuBBPSML.BillPayment(new BBPSPaymentRequest
                        {
                            PaymentMode = TransRequest.PaymentMode,
                            BillerID = TransResponse.BillerID,
                            billerAdhoc = TransResponse.BillerAdhoc,
                            TID = TransResponse.TID,
                            TransactionID = TransResponse.TransactionID,
                            APIOutletID = rechargeAPI.aPIDetail.APIOutletID,
                            CustomerMobile = TransRequest.CustomerNumber,
                            InitChanel = TransResponse.InitChanel,
                            MAC = TransResponse.MAC,
                            IPAddress = TransRequest.RequestIP,
                            FetchBillResponse = TransResponse.BillFetchResponse,
                            OpParams = TransResponse.OpParams,
                            AInfos = TransResponse.AInfos,
                            AccountNo = TransRequest.AccountNo,
                            Optional1 = TransRequest.Optional1,
                            Optional2 = TransRequest.Optional2,
                            Optional3 = TransRequest.Optional3,
                            Optional4 = TransRequest.Optional4,
                            ExactNess = TransResponse.ExactNess,
                            RequestedAmount = TransRequest.AmountR,
                            BillDate = TransResponse.BillDate,
                            DueDate = TransResponse.DueDate,
                            EarlyPaymentDate = TransResponse.EarlyPaymentDate,
                            EarlyPaymentAmountKey = TransResponse.EarlyPaymentAmountKey,
                            LatePaymentAmountKey = TransResponse.LatePaymentAmountKey,
                            CCFAmount = TransRequest.CCFAmount,
                            PaymentModeInAPI = TransResponse.PaymentModeInAPI,
                            CaptureInfo = TransResponse.CaptureInfo,
                            UserMobileNo = TransResponse.UserMobileNo,
                            UserName = TransResponse.UserName,
                            UserEmailID = TransResponse.UserEmailID,
                            IsINT = TransResponse.IsINT,
                            IsMOB = TransResponse.IsMOB,
                            IsQuickPay = TransResponse.IsBilling == false,
                            IsValidation = TransResponse.IsBillValidation,
                            AccountNoKey = TransResponse.AccountNoKey,
                            AccountHolder = TransResponse.AccountHolder,
                            Pincode = TransResponse.Pincode,
                            GeoLocation = TransResponse.GeoCode,
                        });
                        if (HitReqResp.IsValidaionResponseAlso)
                        {
                            if (!HitReqResp.IsBillValidated)
                            {
                                rechargeAPIHit.aPIDetail.URL = HitReqResp.RequestV;
                                rechargeAPIHit.Response = HitReqResp.ResponseV;
                            }
                            else
                            {
                                var rechargeAPITemp = rechargeAPIHit;
                                rechargeAPITemp.aPIDetail.URL = HitReqResp.RequestV;
                                rechargeAPITemp.Response = HitReqResp.ResponseV;
                                var _ = UpdateAPIResponse(TransResponse.TID, rechargeAPITemp);
                                rechargeAPIHit.aPIDetail.URL = HitReqResp.Request;
                                rechargeAPIHit.Response = HitReqResp.Response;
                            }
                        }
                        else
                        {
                            rechargeAPIHit.aPIDetail.URL = HitReqResp.Request;
                            rechargeAPIHit.Response = HitReqResp.Response;
                        }
                    }
                }
                else if ((rechargeAPI.aPIDetail.APICode ?? string.Empty).EndsWith(APICode.SPRINT))
                {
                    SprintBBPSML sprintBBPSML = new SprintBBPSML(_accessor, _env, _dal);
                    var spPayReq = new SPPAYBillReq
                    {
                        @operator = rechargeAPI.aPIDetail.APIOpCode,
                        canumber = TransRequest.AccountNo,
                        amount = TransRequest.AmountR.ToString(),
                        referenceid = TransResponse.TID.ToString(),
                        latitude = TransRequest.GEOCode.Split(',')[0].ToString(),
                        longitude = TransRequest.GEOCode.Split(',')[1].ToString(),
                        mode = "online",
                        bill_fetch = new SPBillPay
                        {
                            billAmount = TransRequest.AmountR.ToString(),
                            billnetamount = TransRequest.AmountR.ToString(),
                            billdate = TransResponse.BillDate,
                            dueDate = TransResponse.DueDate,
                            acceptPayment = true,
                            acceptPartPay = false,
                            cellNumber = TransRequest.AccountNo,
                            userName = TransResponse.UserName
                        }
                    };
                    rechargeAPIHit.Response = sprintBBPSML.BillPayment(spPayReq, this, rechargeAPI);
                }

                else
                {
                    var IsValidated = true;
                    if (TransResponse.IsBBPS && !string.IsNullOrEmpty(rechargeAPI.aPIDetail.ValidateURL) && TransResponse.IsBillValidation)
                    {
                        if (rechargeAPI.aPIDetail.RequestMethod == "GET")
                        {
                            rechargeAPIHit.Response = await AppWebRequest.O.CallUsingHttpWebRequest_GETAsync(rechargeAPI.aPIDetail.ValidateURL).ConfigureAwait(false);
                        }
                        else
                        {
                            if (rechargeAPI.aPIDetail.ContentType == PostContentType.query_string)
                            {
                                rechargeAPIHit.Response = await AppWebRequest.O.CallHWRQueryString_PostAsync(rechargeAPI.aPIDetail.ValidateURL).ConfigureAwait(false);
                            }
                            else if (rechargeAPI.aPIDetail.ContentType == PostContentType.x_www_form_urlencoded)
                            {
                                if (rechargeAPI.aPIDetail.ValidateURL.Contains("?"))
                                    rechargeAPIHit.Response = await AppWebRequest.O.CallUsingHttpWebRequest_POSTAsync(rechargeAPI.aPIDetail.ValidateURL.Split('?')[0], rechargeAPI.aPIDetail.ValidateURL.Split('?')[1], ContentType.x_wwww_from_urlencoded).ConfigureAwait(false);
                            }
                            else if (rechargeAPI.aPIDetail.ContentType == PostContentType.application_json)
                            {
                                if (rechargeAPI.aPIDetail.ValidateURL.Contains("?"))
                                {
                                    var postData = rechargeAPI.aPIDetail.ValidateURL.Split('?')[1];
                                    if (Validate.O.ValidateJSON(postData))
                                    {
                                        rechargeAPIHit.Response = AppWebRequest.O.PostJsonDataUsingHWR(rechargeAPI.aPIDetail.ValidateURL.Split('?')[0], postData);
                                    }
                                }
                            }
                        }
                        RechargeAPIHit rechargeAPITemp = new RechargeAPIHit
                        {
                            aPIDetail = new APIDetail
                            {
                                ActiveSts = rechargeAPIHit.aPIDetail.ActiveSts,
                                AdditionalInfoKey = rechargeAPIHit.aPIDetail.AdditionalInfoKey,
                                AdditionalInfoListKey = rechargeAPIHit.aPIDetail.AdditionalInfoListKey,
                                AdditionalInfoValue = rechargeAPIHit.aPIDetail.AdditionalInfoValue,
                                APICode = rechargeAPIHit.aPIDetail.APICode,
                                APIOpCode = rechargeAPIHit.aPIDetail.APIOpCode,
                                APIOpCodeCircle = rechargeAPIHit.aPIDetail.APIOpCodeCircle,
                                APIOutletID = rechargeAPIHit.aPIDetail.APIOutletID,
                                APIType = rechargeAPIHit.aPIDetail.APIType,
                                BalanceKey = rechargeAPIHit.aPIDetail.BalanceKey,
                                BalanceURL = rechargeAPIHit.aPIDetail.BalanceURL,
                                BillAmountKey = rechargeAPIHit.aPIDetail.BillAmountKey,
                                BillDateKey = rechargeAPIHit.aPIDetail.BillDateKey,
                                BillNoKey = rechargeAPIHit.aPIDetail.BillNoKey,
                                BillReqMethod = rechargeAPIHit.aPIDetail.BillReqMethod,
                                BillResTypeID = rechargeAPIHit.aPIDetail.BillResTypeID,
                                BillStatusKey = rechargeAPIHit.aPIDetail.BillStatusKey,
                                BillStatusValue = rechargeAPIHit.aPIDetail.BillStatusValue,
                                Comm = rechargeAPIHit.aPIDetail.Comm,
                                CommType = rechargeAPIHit.aPIDetail.CommType,
                                ContentType = rechargeAPIHit.aPIDetail.ContentType,
                                CustomerNameKey = rechargeAPIHit.aPIDetail.CustomerNameKey,
                                Default = rechargeAPIHit.aPIDetail.Default,
                                DFormatID = rechargeAPIHit.aPIDetail.DFormatID,
                                DisputeURL = rechargeAPIHit.aPIDetail.DisputeURL,
                                DueDateKey = rechargeAPIHit.aPIDetail.DueDateKey,
                                ErrorCodeKey = rechargeAPIHit.aPIDetail.ErrorCodeKey,
                                FailCode = rechargeAPIHit.aPIDetail.FailCode,
                                FetchBillURL = rechargeAPIHit.aPIDetail.FetchBillURL,
                                FixedOutletID = rechargeAPIHit.aPIDetail.FixedOutletID,
                                GroupCode = rechargeAPIHit.aPIDetail.GroupCode,
                                GroupID = rechargeAPIHit.aPIDetail.GroupID,
                                GroupName = rechargeAPIHit.aPIDetail.GroupName,
                                HandoutID = rechargeAPIHit.aPIDetail.HandoutID,
                                ID = rechargeAPIHit.aPIDetail.ID,
                                LiveID = rechargeAPIHit.aPIDetail.LiveID,
                                MsgKey = rechargeAPIHit.aPIDetail.MsgKey,
                                Name = rechargeAPIHit.aPIDetail.Name,
                                RechType = rechargeAPIHit.aPIDetail.RechType,
                                RefferenceKey = rechargeAPIHit.aPIDetail.RefferenceKey,
                                RequestMethod = rechargeAPIHit.aPIDetail.RequestMethod,
                                MaxLimitPerTransaction = rechargeAPIHit.aPIDetail.MaxLimitPerTransaction,
                                ResponseTypeID = rechargeAPIHit.aPIDetail.ResponseTypeID,
                                OnlineOutletID = rechargeAPIHit.aPIDetail.OnlineOutletID,
                                StatusCheckURL = rechargeAPIHit.aPIDetail.StatusCheckURL,
                                StatusName = rechargeAPIHit.aPIDetail.StatusName,
                                SuccessCode = rechargeAPIHit.aPIDetail.SuccessCode,
                                TransactionType = rechargeAPIHit.aPIDetail.TransactionType,
                                URL = rechargeAPIHit.aPIDetail.ValidateURL,
                                ValidateURL = rechargeAPIHit.aPIDetail.ValidateURL,
                                ValidationStatusKey = rechargeAPIHit.aPIDetail.ValidationStatusKey,
                                ValidationStatusValue = rechargeAPIHit.aPIDetail.ValidationStatusValue,
                                VendorID = rechargeAPIHit.aPIDetail.VendorID,
                                WID = rechargeAPIHit.aPIDetail.WID
                            },
                            Response = rechargeAPIHit.Response,
                            ServiceID = rechargeAPIHit.ServiceID,
                            IsException = rechargeAPIHit.IsException
                        };
                        var tstatusValidation = await MatchResponse(TransRequest.OID, rechargeAPITemp, TransResponse.AccountNoKey, true).ConfigureAwait(false);

                        if (tstatusValidation.Status != RechargeRespType.SUCCESS)
                        {
                            rechargeAPIHit.aPIDetail.URL = rechargeAPIHit.aPIDetail.ValidateURL;
                            IsValidated = false;
                        }
                        else
                        {
                            rechargeAPIHit.aPIDetail.URL = rechargeAPIHit.aPIDetail.URL.Replace(Replacement.REFID, tstatusValidation.RefferenceID ?? string.Empty);
                            var _ = UpdateAPIResponse(TransResponse.TID, rechargeAPITemp);
                        }
                    }
                    else
                    {
                        rechargeAPIHit.aPIDetail.URL = rechargeAPIHit.aPIDetail.URL.Replace(Replacement.REFID, string.Empty);
                    }
                    if (IsValidated)
                    {
                        if (rechargeAPI.aPIDetail.APIType != APITypes.Lapu && !string.IsNullOrEmpty(rechargeAPI.aPIDetail.URL) && rechargeAPI.aPIDetail.APIType != APITypes.Manual)
                        {
                            if (rechargeAPI.aPIDetail.RequestMethod == "GET")
                            {
                                rechargeAPIHit.Response = await AppWebRequest.O.CallUsingHttpWebRequest_GETAsync(rechargeAPI.aPIDetail.URL).ConfigureAwait(false);
                            }
                            else
                            {
                                if (rechargeAPI.aPIDetail.ContentType == PostContentType.query_string)
                                {
                                    rechargeAPIHit.Response = await AppWebRequest.O.CallHWRQueryString_PostAsync(rechargeAPI.aPIDetail.URL).ConfigureAwait(false);
                                }
                                else if (rechargeAPI.aPIDetail.ContentType == PostContentType.x_www_form_urlencoded)
                                {
                                    if (rechargeAPI.aPIDetail.URL.Contains("?"))
                                        rechargeAPIHit.Response = await AppWebRequest.O.CallUsingHttpWebRequest_POSTAsync(rechargeAPI.aPIDetail.URL.Split('?')[0], rechargeAPI.aPIDetail.URL.Split('?')[1], ContentType.x_wwww_from_urlencoded).ConfigureAwait(false);
                                }
                                else if (rechargeAPI.aPIDetail.ContentType == PostContentType.application_json)
                                {
                                    if (rechargeAPI.aPIDetail.URL.Contains("?"))
                                    {
                                        var postData = rechargeAPI.aPIDetail.URL.Split('?')[1];
                                        if (Validate.O.ValidateJSON(postData))
                                        {
                                            rechargeAPIHit.Response = AppWebRequest.O.PostJsonDataUsingHWR(rechargeAPI.aPIDetail.URL.Split('?')[0], postData);
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            rechargeAPIHit.Response = string.Empty;
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                rechargeAPIHit.IsException = true;
                rechargeAPIHit.Response = ex.Message;
            }
            return rechargeAPIHit;
        }
        public async Task UpdateAPIResponse(int TID, RechargeAPIHit rechargeAPIHit)
        {
            try
            {
                var req = new TransactionReqResp
                {
                    APIID = rechargeAPIHit.aPIDetail.ID,
                    TID = TID,
                    Request = rechargeAPIHit.aPIDetail.URL,
                    Response = rechargeAPIHit.Response,
                    APIName = rechargeAPIHit.aPIDetail.Name,
                    APIOpCode = rechargeAPIHit.aPIDetail.APIOpCode,
                    APIComAmt = rechargeAPIHit.aPIDetail.Comm,
                    APICommType = rechargeAPIHit.aPIDetail.CommType
                };
                IProcedureAsync _proc = new ProcUpdateTransactionReqResp(_dal);
                var _ = await _proc.Call(req).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = rechargeAPIHit.aPIDetail.ID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
        }
        public async Task UpdateAPIResponse(TransactionReqResp req)
        {
            try
            {
                IProcedureAsync _proc = new ProcUpdateTransactionReqResp(_dal);
                var _ = await _proc.Call(req);
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = req.TID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
        }
        private async Task UpdateAPIDownStatus(int OID, int APIID)
        {
            var aPIDL = new APIDL(_dal);
            await aPIDL.DownAPI(OID, APIID).ConfigureAwait(false);
        }
        public RechargeAPIHit URLReplacement(int TID, TransactionServiceReq TRequest, RechargeAPIHit rechargeAPI, bool IsValidation)
        {
            try
            {
                if (!string.IsNullOrEmpty(rechargeAPI.aPIDetail.URL) || rechargeAPI.aPIDetail.APIType == APITypes.Lapu)
                {
                    StringBuilder sbURL = new StringBuilder(rechargeAPI.aPIDetail.URL);
                    sbURL.Replace(Replacement.MOBILE, TRequest.AccountNo ?? "");
                    sbURL.Replace(Replacement.AMOUNT, (TRequest.AmountR * (rechargeAPI.aPIDetail.IsAmountInto100 ? 100 : 1)).ToString().Split(".")[0]);
                    sbURL.Replace(Replacement._AMOUNT, (TRequest.AmountR * (rechargeAPI.aPIDetail.IsAmountInto100 ? 100 : 1)).ToString());
                    sbURL.Replace(Replacement.OPERATOR, rechargeAPI.aPIDetail.APIOpCode.Contains("|") ? rechargeAPI.aPIDetail.APIOpCode.Split('|')[0] : rechargeAPI.aPIDetail.APIOpCode);
                    sbURL.Replace(Replacement.TID, TID.ToString());
                    if (rechargeAPI.aPIDetail.APIOpCode.Contains('|'))
                    {
                        sbURL.Replace(Replacement.RECHTYPE, rechargeAPI.aPIDetail.APIOpCode.Split('|')[1]);
                        if (rechargeAPI.aPIDetail.APIOpCode.Split('|').Length > 2)
                        {
                            var lastAPICode = rechargeAPI.aPIDetail.APIOpCode.Split('|')[2];
                            if (lastAPICode.Length > 2)
                            {
                                var IsWithoutZero = lastAPICode.Substring(0, 2) == "WO";
                                var RightMost = Validate.O.IsNumeric(lastAPICode[2].ToString()) ? Convert.ToInt16(lastAPICode[2].ToString()) : 0;
                                if (RightMost == 1)
                                {
                                    var IsStartedZero = ((TRequest.Optional1 ?? string.Empty).Length > 0 ? (Validate.O.IsNumeric(TRequest.Optional1[0].ToString()) ? Convert.ToInt16(TRequest.Optional1[0].ToString()) : 1) : 1) == 0;
                                    sbURL.Replace(Replacement.OPTIONAL1, (IsWithoutZero ? (IsStartedZero ? TRequest.Optional1.Substring(1, TRequest.Optional1.Length) : TRequest.Optional1) : (IsStartedZero ? TRequest.Optional1 : '0' + TRequest.Optional1)));
                                }
                                if (RightMost == 2)
                                {
                                    var IsStartedZero = ((TRequest.Optional2 ?? string.Empty).Length > 0 ? (Validate.O.IsNumeric(TRequest.Optional2[0].ToString()) ? Convert.ToInt16(TRequest.Optional2[0].ToString()) : 1) : 1) == 0;
                                    sbURL.Replace(Replacement.OPTIONAL2, (IsWithoutZero ? (IsStartedZero ? TRequest.Optional2.Substring(1, TRequest.Optional2.Length) : TRequest.Optional2) : (IsStartedZero ? TRequest.Optional2 : '0' + TRequest.Optional2)));
                                }
                                if (RightMost == 3)
                                {
                                    var IsStartedZero = ((TRequest.Optional3 ?? string.Empty).Length > 0 ? (Validate.O.IsNumeric(TRequest.Optional3[0].ToString()) ? Convert.ToInt16(TRequest.Optional3[0].ToString()) : 1) : 1) == 0;
                                    sbURL.Replace(Replacement.OPTIONAL3, (IsWithoutZero ? (IsStartedZero ? TRequest.Optional3.Substring(1, TRequest.Optional3.Length) : TRequest.Optional3) : (IsStartedZero ? TRequest.Optional3 : '0' + TRequest.Optional3)));
                                }
                                if (RightMost == 4)
                                {
                                    var IsStartedZero = ((TRequest.Optional4 ?? string.Empty).Length > 0 ? (Validate.O.IsNumeric(TRequest.Optional4[0].ToString()) ? Convert.ToInt16(TRequest.Optional4[0].ToString()) : 1) : 1) == 0;
                                    sbURL.Replace(Replacement.OPTIONAL4, (IsWithoutZero ? (IsStartedZero ? TRequest.Optional4.Substring(1, TRequest.Optional4.Length) : TRequest.Optional4) : (IsStartedZero ? TRequest.Optional4 : '0' + TRequest.Optional4)));
                                }
                            }

                        }
                    }
                    sbURL.Replace(Replacement.OPTIONAL1, TRequest.Optional1);
                    sbURL.Replace(Replacement.OPTIONAL2, TRequest.Optional2);
                    sbURL.Replace(Replacement.OPTIONAL3, TRequest.Optional3);
                    sbURL.Replace(Replacement.OPTIONAL4, TRequest.Optional4);
                    sbURL.Replace(Replacement.OUTLETID, TRequest.OutletID.ToString());
                    sbURL.Replace(Replacement.CUSTMOB, TRequest.CustomerNumber);
                    sbURL.Replace(Replacement.GEOCODE, TRequest.GEOCode);
                    sbURL.Replace(Replacement.PINCODE, TRequest.PinCode);
                    if (!string.IsNullOrEmpty(TRequest.RefID))
                    {
                        sbURL.Replace(Replacement.REFID, TRequest.RefID);
                    }
                    rechargeAPI.aPIDetail.URL = sbURL.ToString();
                }
                if (IsValidation && !string.IsNullOrEmpty(rechargeAPI.aPIDetail.ValidateURL))
                {
                    StringBuilder sbURLV = new StringBuilder(rechargeAPI.aPIDetail.ValidateURL);
                    sbURLV.Replace(Replacement.MOBILE, TRequest.AccountNo ?? "");
                    sbURLV.Replace(Replacement.AMOUNT, (TRequest.AmountR * (rechargeAPI.aPIDetail.IsAmountInto100 ? 100 : 1)).ToString().Split(".")[0]);
                    sbURLV.Replace(Replacement._AMOUNT, (TRequest.AmountR * (rechargeAPI.aPIDetail.IsAmountInto100 ? 100 : 1)).ToString());
                    sbURLV.Replace(Replacement.OPERATOR, rechargeAPI.aPIDetail.APIOpCode.Contains("|") ? rechargeAPI.aPIDetail.APIOpCode.Split('|')[0] : rechargeAPI.aPIDetail.APIOpCode);
                    sbURLV.Replace(Replacement.TID, TID.ToString());
                    if (rechargeAPI.aPIDetail.APIOpCode.Contains('|'))
                    {
                        sbURLV.Replace(Replacement.RECHTYPE, rechargeAPI.aPIDetail.APIOpCode.Split('|')[1]);
                        if (rechargeAPI.aPIDetail.APIOpCode.Split('|').Length > 2)
                        {
                            var lastAPICode = rechargeAPI.aPIDetail.APIOpCode.Split('|')[2];
                            if (lastAPICode.Length > 2)
                            {
                                var IsWithoutZero = lastAPICode.Substring(0, 2) == "WO";
                                var RightMost = Validate.O.IsNumeric(lastAPICode[2].ToString()) ? Convert.ToInt16(lastAPICode[2].ToString()) : 0;
                                if (RightMost == 1)
                                {
                                    var IsStartedZero = ((TRequest.Optional1 ?? string.Empty).Length > 0 ? (Validate.O.IsNumeric(TRequest.Optional1[0].ToString()) ? Convert.ToInt16(TRequest.Optional1[0].ToString()) : 1) : 1) == 0;
                                    sbURLV.Replace(Replacement.OPTIONAL1, (IsWithoutZero ? (IsStartedZero ? TRequest.Optional1.Substring(1, TRequest.Optional1.Length) : TRequest.Optional1) : (IsStartedZero ? TRequest.Optional1 : '0' + TRequest.Optional1)));
                                }
                                if (RightMost == 2)
                                {
                                    var IsStartedZero = ((TRequest.Optional2 ?? string.Empty).Length > 0 ? (Validate.O.IsNumeric(TRequest.Optional2[0].ToString()) ? Convert.ToInt16(TRequest.Optional2[0].ToString()) : 1) : 1) == 0;
                                    sbURLV.Replace(Replacement.OPTIONAL2, (IsWithoutZero ? (IsStartedZero ? TRequest.Optional2.Substring(1, TRequest.Optional2.Length) : TRequest.Optional2) : (IsStartedZero ? TRequest.Optional2 : '0' + TRequest.Optional2)));
                                }
                                if (RightMost == 3)
                                {
                                    var IsStartedZero = ((TRequest.Optional3 ?? string.Empty).Length > 0 ? (Validate.O.IsNumeric(TRequest.Optional3[0].ToString()) ? Convert.ToInt16(TRequest.Optional3[0].ToString()) : 1) : 1) == 0;
                                    sbURLV.Replace(Replacement.OPTIONAL3, (IsWithoutZero ? (IsStartedZero ? TRequest.Optional3.Substring(1, TRequest.Optional3.Length) : TRequest.Optional3) : (IsStartedZero ? TRequest.Optional3 : '0' + TRequest.Optional3)));
                                }
                                if (RightMost == 4)
                                {
                                    var IsStartedZero = ((TRequest.Optional4 ?? string.Empty).Length > 0 ? (Validate.O.IsNumeric(TRequest.Optional4[0].ToString()) ? Convert.ToInt16(TRequest.Optional4[0].ToString()) : 1) : 1) == 0;
                                    sbURLV.Replace(Replacement.OPTIONAL4, (IsWithoutZero ? (IsStartedZero ? TRequest.Optional4.Substring(1, TRequest.Optional4.Length) : TRequest.Optional4) : (IsStartedZero ? TRequest.Optional4 : '0' + TRequest.Optional4)));
                                }
                            }
                        }
                    }
                    sbURLV.Replace(Replacement.OPTIONAL1, TRequest.Optional1);
                    sbURLV.Replace(Replacement.OPTIONAL2, TRequest.Optional2);
                    sbURLV.Replace(Replacement.OPTIONAL3, TRequest.Optional3);
                    sbURLV.Replace(Replacement.OPTIONAL4, TRequest.Optional4);
                    sbURLV.Replace(Replacement.OUTLETID, TRequest.OutletID.ToString());
                    sbURLV.Replace(Replacement.CUSTMOB, TRequest.CustomerNumber);
                    sbURLV.Replace(Replacement.GEOCODE, TRequest.GEOCode);
                    sbURLV.Replace(Replacement.PINCODE, TRequest.PinCode);
                    sbURLV.Replace(Replacement.REFID, TRequest.RefID);
                    rechargeAPI.aPIDetail.ValidateURL = sbURLV.ToString();
                }
            }
            catch (Exception)
            {
                throw;
            }
            return rechargeAPI;
        }
        #region Hit Cyberplat API
        private async Task<string> CallCyberPlat(RechargeAPIHit rechargeAPI)
        {
            var cyberPlateML = new CyberPlateML(_dal);
            var PaymentURL = rechargeAPI.aPIDetail.URL;
            /*call validation*/
            rechargeAPI = cyberPlateML.CyberPlatVerification(rechargeAPI);/*calling validation api*/
            var _ = UpdateAPIResponse(Convert.ToInt32(rechargeAPI.CPTRNXRequest.SESSION.Replace("A", "")), rechargeAPI);
            var matchresp = new TransactionStatus();
            matchresp = await MatchResponse(Convert.ToInt32(rechargeAPI.CPTRNXRequest.SESSION.Replace("A", "")), rechargeAPI, string.Empty);
            /*call validation*/
            if (matchresp.Status == RechargeRespType.SUCCESS)
            {
                rechargeAPI.Response = string.Empty;
                rechargeAPI.aPIDetail.URL = PaymentURL;
                rechargeAPI = cyberPlateML.CyberPlatPayment(rechargeAPI);/*calling payment api*/
                //if pending after payment then call status check
                var cp = new CyberPlateML(_dal);
                var pmtres = cp.GetResponseFromString(rechargeAPI.Response);
                if (pmtres.ERROR == "36")
                {
                    var __ = UpdateAPIResponse(Convert.ToInt32(rechargeAPI.CPTRNXRequest.SESSION.Replace("A", "")), rechargeAPI);
                    rechargeAPI.Response = string.Empty;
                    rechargeAPI.aPIDetail.URL = PaymentURL;
                    rechargeAPI = cyberPlateML.CyberPlatStatusCheck(rechargeAPI);/*calling statuscheck api*/
                }
            }
            return rechargeAPI.Response;
        }
        #endregion
        #endregion
        #region FetchBillSection
        public async Task ResponseHandlerFetchBill(BBPSResponse bBPSResponse, BBPSLog bBPSLog)
        {
            if (bBPSLog.helper == null)
            {
                bBPSLog.helper = new BBPSLogReqHelper();
            }
            string MethodName = "ResponseHandlerFetchBill";
            try
            {
                bBPSResponse.IsShowMsgOnly = true;
                var OtherConditionMatchString = string.Empty;
                var errorCodes = new ErrorCodeDetail();
                var ds = new DataSet();
                string APIErrorCode = string.Empty, APIMsg = string.Empty;
                if (bBPSLog.aPIDetail.BillResTypeID == ResponseType.JSON)
                {
                    ds = _toDataSet.ReadDataFromJson(bBPSLog.Response);
                }
                else if (bBPSLog.aPIDetail.BillResTypeID == ResponseType.XML)
                {
                    ds = _toDataSet.ReadDataFromXML(bBPSLog.Response);
                }
                else
                {
                    APIMsg = bBPSLog.Response;
                }

                #region MatchResponseForXML&JSON

                if (ds.Tables.Count > 0)
                {
                    bool IsBillNoFound = string.IsNullOrEmpty(bBPSLog.aPIDetail.BillNoKey), IsBillDateFound = string.IsNullOrEmpty(bBPSLog.aPIDetail.BillDateKey), IsDueDateFound = string.IsNullOrEmpty(bBPSLog.aPIDetail.DueDateKey), IsBillAmountKeyFound = string.IsNullOrEmpty(bBPSLog.aPIDetail.BillAmountKey), IsCustomerNameFound = string.IsNullOrEmpty(bBPSLog.aPIDetail.CustomerNameKey), IsStatusFound = string.IsNullOrEmpty(bBPSLog.aPIDetail.BillStatusKey), IsErrorCodeFound = false, IsMessageFound = false, IsRefKeyFound = string.IsNullOrEmpty(bBPSLog.aPIDetail.RefferenceKey);
                    bBPSResponse.ErrorMsg = string.Empty;

                    foreach (DataTable tbl in ds.Tables)
                    {
                        var BST = CheckIfKeyExistsInDatatable(tbl, bBPSLog.aPIDetail.BillStatusKey);
                        //BillStatusValue is known for success status only
                        if (!IsStatusFound && BST.CommonBool)
                        {
                            IsStatusFound = true;
                            string StatusValue = tbl.Rows[0][BST.CommonStr].ToString().ToUpper();
                            bool IsSuccess = false;
                            if (bBPSLog.aPIDetail.BillStatusValue != "")
                            {
                                var sucValArr = bBPSLog.aPIDetail.BillStatusValue.ToUpper().Split(',');
                                IsSuccess = sucValArr.Contains(StatusValue) ? true : false;
                            }
                            bBPSResponse.Statuscode = IsSuccess ? ErrorCodes.One : ErrorCodes.Minus1;
                        }
                        var BNK = CheckIfKeyExistsInDatatable(tbl, bBPSLog.aPIDetail.BillNoKey);
                        if (!IsBillNoFound && BNK.CommonBool)
                        {
                            IsBillNoFound = true;
                            bBPSResponse.BillNumber = tbl.Rows[0][BNK.CommonStr].ToString().ToUpper();
                        }
                        var BDK = CheckIfKeyExistsInDatatable(tbl, bBPSLog.aPIDetail.BillDateKey);
                        if (!IsBillDateFound && BDK.CommonBool)
                        {
                            IsBillDateFound = true;
                            bBPSResponse.BillDate = tbl.Rows[0][BDK.CommonStr].ToString();
                        }
                        var DDK = CheckIfKeyExistsInDatatable(tbl, bBPSLog.aPIDetail.DueDateKey);
                        if (!IsDueDateFound && DDK.CommonBool)
                        {
                            IsDueDateFound = true;
                            bBPSResponse.DueDate = tbl.Rows[0][DDK.CommonStr].ToString();
                        }
                        var BAK = CheckIfKeyExistsInDatatable(tbl, bBPSLog.aPIDetail.BillAmountKey);
                        if (!IsBillAmountKeyFound && BAK.CommonBool)
                        {
                            IsBillAmountKeyFound = true;
                            bBPSResponse.Amount = tbl.Rows[0][BAK.CommonStr].ToString().Trim();
                            IsStatusFound = true;
                            bBPSLog.Amount = Convert.ToDecimal(bBPSResponse.Amount ?? "0");
                        }
                        var CNK = CheckIfKeyExistsInDatatable(tbl, bBPSLog.aPIDetail.CustomerNameKey);
                        if (!IsCustomerNameFound && CNK.CommonBool)
                        {
                            IsCustomerNameFound = true;
                            bBPSResponse.CustomerName = tbl.Rows[0][CNK.CommonStr].ToString();
                            bBPSLog.CustomerName = bBPSResponse.CustomerName;
                        }
                        var MGK = CheckIfKeyExistsInDatatable(tbl, bBPSLog.aPIDetail.MsgKey);
                        if (!IsMessageFound && MGK.CommonBool)
                        {
                            APIMsg = tbl.Rows[0][MGK.CommonStr].ToString();
                            IsMessageFound = true;
                        }
                        var RFK = CheckIfKeyExistsInDatatable(tbl, bBPSLog.aPIDetail.RefferenceKey);
                        if (!IsRefKeyFound && RFK.CommonBool)
                        {
                            IsRefKeyFound = true;
                            bBPSResponse.RefferenceID = tbl.Rows[0][RFK.CommonStr].ToString();
                            bBPSLog.helper.RefferenceID = bBPSResponse.RefferenceID;
                        }
                        var ECK = CheckIfKeyExistsInDatatable(tbl, bBPSLog.aPIDetail.ErrorCodeKey);
                        if (!IsErrorCodeFound && ECK.CommonBool)
                        {
                            IsErrorCodeFound = true;
                            APIErrorCode = tbl.Rows[0][ECK.CommonStr].ToString();
                        }
                        if (IsBillNoFound && IsBillDateFound && IsDueDateFound && IsBillAmountKeyFound && IsCustomerNameFound && IsMessageFound && IsRefKeyFound && IsErrorCodeFound)
                            break;
                    }
                    if (!string.IsNullOrEmpty(bBPSLog.aPIDetail.AdditionalInfoListKey))
                    {
                        bBPSResponse.billAdditionalInfo = new List<BillAdditionalInfo>();
                        if (ds.Tables.Contains(bBPSLog.aPIDetail.AdditionalInfoListKey))
                        {
                            var dtAInfo = ds.Tables[bBPSLog.aPIDetail.AdditionalInfoListKey];
                            if (dtAInfo.Rows.Count > 0 && !string.IsNullOrEmpty(bBPSLog.aPIDetail.AdditionalInfoKey) && !string.IsNullOrEmpty(bBPSLog.aPIDetail.AdditionalInfoValue))
                            {
                                if (dtAInfo.Columns.Contains(bBPSLog.aPIDetail.AdditionalInfoKey) && dtAInfo.Columns.Contains(bBPSLog.aPIDetail.AdditionalInfoValue))
                                {
                                    foreach (DataRow itemAInfo in dtAInfo.Rows)
                                    {
                                        bBPSResponse.billAdditionalInfo.Add(new BillAdditionalInfo
                                        {
                                            InfoName = itemAInfo[bBPSLog.aPIDetail.AdditionalInfoKey].ToString(),
                                            InfoValue = itemAInfo[bBPSLog.aPIDetail.AdditionalInfoValue].ToString()
                                        });
                                    };
                                }
                            }
                        }
                    }
                }
                if (Validate.O.IsNumeric(((bBPSResponse.Amount ?? string.Empty)).Replace(".", "")))
                {
                    if (Convert.ToDecimal(bBPSResponse.Amount) < 1)
                    {
                        if (bBPSResponse.Statuscode == ErrorCodes.One)
                        {
                            bBPSResponse.Msg = ErrorCodes.NoBillDATA;
                            bBPSResponse.ErrorCode = "416";
                            bBPSResponse.ErrorMsg = bBPSResponse.Msg;
                        }
                        bBPSResponse.Statuscode = ErrorCodes.Minus1;
                    }
                    else
                    {
                        bBPSResponse.Statuscode = ErrorCodes.One;
                    }
                }
                else
                {
                    bBPSResponse.Statuscode = ErrorCodes.Minus1;
                }
                if (bBPSResponse.ErrorCode != "416")
                {
                    if (bBPSResponse.Statuscode == ErrorCodes.One)
                    {
                        bBPSResponse.IsEnablePayment = true;
                        bBPSResponse.IsShowMsgOnly = false;
                        bBPSLog.helper.Status = ErrorCodes.One;
                    }
                    else if (!string.IsNullOrEmpty(APIErrorCode))
                    {
                        var apiErrorCodes = _errCodeML.GetAPIErrorCode(new APIErrorCode { APICode = APIErrorCode, GroupCode = bBPSLog.aPIDetail.GroupCode, ErrorType = 7 });
                        errorCodes = _errCodeML.Get(string.IsNullOrEmpty(apiErrorCodes.ECode) ? ErrorCodes.Unable_to_get_bill_details_Please_try_again_after_few_minute.ToString() : apiErrorCodes.ECode);
                        bBPSResponse.ErrorMsg = errorCodes.Error;
                        bBPSResponse.ErrorCode = errorCodes.Code;
                        bBPSResponse.Msg = errorCodes.Error;
                        bBPSLog.helper.Reason = bBPSResponse.ErrorMsg;
                    }
                    else if (!string.IsNullOrEmpty(APIMsg))
                    {
                        OtherConditionMatchString = APIMsg;
                        bBPSResponse.ErrorMsg = string.Empty;

                        var apistatuscheck = new APISTATUSCHECK
                        {
                            Msg = Validate.O.ReplaceAllSpecials(OtherConditionMatchString).Trim()
                        };
                        var _proc = new ProcCheckTextResponse(_dal);
                        apistatuscheck = (APISTATUSCHECK)await _proc.Call(apistatuscheck).ConfigureAwait(false);
                        errorCodes = _errCodeML.Get(string.IsNullOrEmpty(apistatuscheck.ErrorCode) ? ErrorCodes.Unable_to_get_bill_details_Please_try_again_after_few_minute.ToString() : apistatuscheck.ErrorCode);
                        bBPSResponse.ErrorMsg = errorCodes.Error;
                        bBPSResponse.ErrorCode = errorCodes.Code;
                        bBPSResponse.Msg = errorCodes.Error;
                        if (apistatuscheck.Statuscode == ErrorCodes.One)
                        {
                            bBPSResponse.Statuscode = apistatuscheck.Status == RechargeRespType.SUCCESS ? bBPSResponse.Statuscode : ErrorCodes.Minus1;
                            if (errorCodes.ErrType == ErrType.BillFetch)
                            {
                                if (errorCodes.IsResend)
                                {
                                    bBPSResponse.IsEditable = true;
                                    bBPSResponse.IsEnablePayment = true;
                                    bBPSResponse.IsShowMsgOnly = false;
                                }
                            }
                        }
                    }
                }

                bBPSResponse.ErrorMsg = bBPSResponse.ErrorMsg.Replace(Replacement.REPLACE, APIMsg);
                if (!string.IsNullOrEmpty(bBPSLog.APIReqHelper.AccountNoKey))
                {
                    bBPSResponse.ErrorMsg = bBPSResponse.ErrorMsg.Replace("{AccountKey}", bBPSLog.APIReqHelper.AccountNoKey ?? string.Empty);
                }
                bBPSResponse.Msg = bBPSResponse.ErrorMsg;
                bBPSLog.helper.Reason = bBPSResponse.Msg;
                #endregion
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = MethodName,
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = bBPSLog.aPIDetail.ID
                });
            }
        }
        public async Task<BBPSResponse> HitFetchBillAPI(BBPSLog bBPSLog)
        {
            var bBPSResponse = new BBPSResponse
            {
                IsEditable = false,
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.URLNOTFOUND
            };
            bBPSLog.IMEI = GenerateRandomSessionNo(15);
            var IsSaveLog = true;
            var Resp = string.Empty;
            try
            {
                if (bBPSLog.aPIDetail.APICode == APICode.CYBERPLAT)
                {
                    IsSaveLog = true;
                    var cyberPlateML = new CyberPlateML(_dal);
                    var cyberResp = cyberPlateML.FetchBill(bBPSLog);
                    bBPSResponse.Statuscode = cyberResp.Statuscode;
                    bBPSResponse.Msg = cyberResp.Msg;
                    bBPSResponse.ErrorMsg = cyberResp.Msg;
                    bBPSResponse.IsShowMsgOnly = cyberResp.IsShowMsgOnly;
                    bBPSResponse.IsEditable = cyberResp.IsEditable;
                    bBPSResponse.IsEnablePayment = cyberResp.IsEnablePayment;
                    bBPSResponse.Amount = cyberResp.Amount;
                    bBPSResponse.BillDate = cyberResp.BillDate;
                    bBPSResponse.BillNumber = cyberResp.BillNumber;
                    bBPSResponse.CustomerName = cyberResp.CustomerName;
                    bBPSResponse.DueDate = cyberResp.DueDate;
                    bBPSResponse.IsHardCoded = true;
                }
                else if (bBPSLog.aPIDetail.APICode == APICode.EASYPAY)
                {
                    IsSaveLog = true;

                    var easypayML = new EasypayML(_accessor, _env, _dal);
                    bBPSResponse = easypayML.FetchBill(bBPSLog);
                    bBPSResponse.IsHardCoded = true;
                    Resp = bBPSLog.Response;
                }
                else if (bBPSLog.aPIDetail.APICode == APICode.BILLAVENUE)
                {
                    IsSaveLog = true;
                    BillAvenueML billAvenueML = new BillAvenueML(_accessor, _env, _dal);
                    bBPSResponse = billAvenueML.BillFetchAPI(bBPSLog);
                    bBPSResponse.IsHardCoded = true;
                }
                else if (bBPSLog.aPIDetail.APICode.Equals(APICode.AXISBANK))
                {
                    IsSaveLog = true;
                    AxisBankBBPSML axisBankBBPSML = new AxisBankBBPSML(_accessor, _env, _dal);
                    bBPSResponse = axisBankBBPSML.AxisBankBillFetchRequest(bBPSLog).Result;
                    bBPSResponse.IsHardCoded = true;
                }
                else if (bBPSLog.aPIDetail.APICode.Equals(APICode.PAYUBBPS))
                {
                    IsSaveLog = true;
                    PayuBBPSML payuBBPSML = new PayuBBPSML(_accessor, _env, _dal);
                    bBPSResponse = payuBBPSML.FetchBill(bBPSLog);
                    bBPSResponse.IsHardCoded = true;
                }
                else if ((bBPSLog.aPIDetail.APICode ?? string.Empty).EndsWith(APICode.SPRINT))
                {
                    IsSaveLog = true;
                    SprintBBPSML sprintBBPSML = new SprintBBPSML(_accessor, _env, _dal);
                    bBPSResponse = sprintBBPSML.FetchBill(bBPSLog);
                    bBPSResponse.IsHardCoded = true;
                }
                else
                {
                    if (!string.IsNullOrEmpty(bBPSLog.aPIDetail.URL))
                    {
                        if (bBPSLog.aPIDetail.BillReqMethod == "GET")
                        {
                            Resp = await AppWebRequest.O.CallUsingHttpWebRequest_GETAsync(bBPSLog.aPIDetail.URL).ConfigureAwait(false);
                        }
                        else
                        {
                            if (bBPSLog.aPIDetail.URL.Contains("?"))
                            {
                                if (bBPSLog.aPIDetail.ContentType == PostContentType.query_string)
                                {
                                    Resp = await AppWebRequest.O.CallHWRQueryString_PostAsync(bBPSLog.aPIDetail.URL);
                                }
                                else if (bBPSLog.aPIDetail.ContentType == PostContentType.x_www_form_urlencoded)
                                {
                                    if (bBPSLog.aPIDetail.URL.Contains("?"))
                                        Resp = await AppWebRequest.O.CallUsingHttpWebRequest_POSTAsync(bBPSLog.aPIDetail.URL.Split('?')[0], bBPSLog.aPIDetail.URL.Split('?')[1], ContentType.x_wwww_from_urlencoded);
                                }
                                else if (bBPSLog.aPIDetail.ContentType == PostContentType.application_json)
                                {
                                    if (bBPSLog.aPIDetail.URL.Contains("?"))
                                    {
                                        var postData = bBPSLog.aPIDetail.URL.Split('?')[1];
                                        if (Validate.O.ValidateJSON(postData))
                                        {
                                            Resp = AppWebRequest.O.PostJsonDataUsingHWR(bBPSLog.aPIDetail.URL.Split('?')[0], postData);
                                        }
                                    }
                                }
                            }
                        }
                        bBPSLog.Request = bBPSLog.aPIDetail.URL;
                        bBPSLog.Response = Resp;
                        await ResponseHandlerFetchBill(bBPSResponse, bBPSLog).ConfigureAwait(false);
                    }
                }
            }
            catch (Exception ex)
            {
                Resp = "Exception:" + ex.Message;
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "HitFetchBillAPI",
                    Error = ex.Message
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }

            if (IsSaveLog)
            {
                if (!bBPSResponse.IsHardCoded)
                    bBPSLog.Response = Resp;
                var res = UpdateAPIFetchBillResponse(bBPSLog);
                bBPSResponse.FetchBillID = res.CommonInt;
            }
            if (bBPSResponse.Statuscode == ErrorCodes.One && Convert.ToDouble(bBPSResponse.Amount ?? "0") <= 0)
            {
                bBPSResponse.IsShowMsgOnly = true;
                bBPSResponse.IsEnablePayment = false;
                bBPSResponse.Statuscode = ErrorCodes.Minus1;
                bBPSResponse.ErrorCode = ErrorCodes.No_Payment_Due.ToString();
                bBPSResponse.ErrorMsg = nameof(ErrorCodes.No_Payment_Due).Replace("_", " ");
                bBPSResponse.Msg = nameof(ErrorCodes.No_Payment_Due).Replace("_", " ");
            }
            return bBPSResponse;
        }
        public ResponseStatus UpdateAPIFetchBillResponse(BBPSLog bBPSLog)
        {
            var res = new ResponseStatus();
            try
            {
                IProcedure proc = new ProcFetchBillActive(_dal);
                res = (ResponseStatus)proc.Call(bBPSLog);
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser
                });
            }
            return res;
        }
        #endregion
        private ApiOperatorOptionalMappingModel AOPMapping(int A, int O)
        {
            var _res = new ApiOperatorOptionalMappingModel
            {
                StatusCode = ErrorCodes.Minus1,
                Msg = ErrorCodes.FAILED
            };

            var req = new GetApiOptionalParam
            {
                _APIID = A,
                _OID = O
            };
            IProcedure _proc = new ProcGetApiOperatorOptionalMapping(_dal);
            _res = (ApiOperatorOptionalMappingModel)_proc.Call(req);

            return _res;
        }

        #region HitBalanceAPI
        public async Task<APIBalanceResponse> HitGetBalance(APIDetail aPIDetail)
        {
            var balanceResponse = new APIBalanceResponse
            {
                Request = aPIDetail.BalanceURL,
                StartAt = DateTime.Now.ToString("dd MMM yyyy hh:mm:ss.ffff tt")
            };
            try
            {
                if ((aPIDetail.BalanceURL ?? string.Empty).Length > 0)
                {
                    if (aPIDetail.APICode.In(APICode.CYBERPLAT, APICode.CYBERPLATPayTM))
                    {
                        var cyberPlateML = new CyberPlateML(_dal);
                        cyberPlateML.GetCyberPlateBalance(balanceResponse, "A" + GenerateRandomSessionNo(15));
                        balanceResponse.EndAt = DateTime.Now.ToString("dd MMM yyyy hh:mm:ss.ffff tt");
                    }
                    else if (aPIDetail.APICode.Equals(APICode.PANMITRA))
                    {
                        var panmitraML = new PANMitraML(_accessor, _env, _dal);
                        panmitraML.BalanceCheck(balanceResponse);
                    }
                    else if (aPIDetail.APICode.Equals(APICode.CASHPOINTINDIA))
                    {
                        var cpiML = new CashPointIndiaML(_accessor, _env, _dal);
                        cpiML.BalanceCheck(balanceResponse);
                    }
                    else
                    {
                        aPIDetail.BalanceURL = aPIDetail.BalanceURL.Replace(Replacement.TID, GenerateRandomSessionNo(15));
                        balanceResponse.StartAt = DateTime.Now.ToString("dd MMM yyyy hh:mm:ss.ffff tt");
                        if (aPIDetail.RequestMethod == "GET")
                        {
                            balanceResponse.Response = await AppWebRequest.O.CallUsingHttpWebRequest_GETAsync(aPIDetail.BalanceURL);

                        }
                        else
                        {
                            if (aPIDetail.ContentType == PostContentType.query_string)
                            {
                                balanceResponse.Response = await AppWebRequest.O.CallHWRQueryString_PostAsync(aPIDetail.BalanceURL);
                            }
                            else if (aPIDetail.ContentType == PostContentType.x_www_form_urlencoded)
                            {
                                if (aPIDetail.BalanceURL.Contains("?"))
                                    balanceResponse.Response = await AppWebRequest.O.CallUsingHttpWebRequest_POSTAsync(aPIDetail.BalanceURL.Split('?')[0], aPIDetail.BalanceURL.Split('?')[1], ContentType.x_wwww_from_urlencoded);
                            }
                            else if (aPIDetail.ContentType == PostContentType.application_json)
                            {
                                if (aPIDetail.BalanceURL.Contains("?"))
                                {
                                    var postData = aPIDetail.BalanceURL.Split('?')[1];
                                    if (Validate.O.ValidateJSON(postData))
                                    {
                                        balanceResponse.Response = AppWebRequest.O.PostJsonDataUsingHWR(aPIDetail.BalanceURL.Split('?')[0], postData);
                                    }
                                }
                            }
                        }
                        balanceResponse.EndAt = DateTime.Now.ToString("dd MMM yyyy hh:mm:ss.ffff tt");
                        await MatchBalanceResponse(balanceResponse, aPIDetail);
                    }
                    balanceResponse.EndAt = DateTime.Now.ToString("dd MMM yyyy hh:mm:ss.ffff tt");
                }
                else
                {
                    balanceResponse.EndAt = DateTime.Now.ToString("dd MMM yyyy hh:mm:ss.ffff tt");
                }
            }
            catch (Exception ex)
            {
                balanceResponse.EndAt = DateTime.Now.ToString("dd MMM yyyy hh:mm:ss.ffff tt");
                balanceResponse.Response = ex.Message;
            }

            return balanceResponse;
        }
        private async Task MatchBalanceResponse(APIBalanceResponse balanceResponse, APIDetail aPIDetail)
        {
            if ((balanceResponse.Response ?? string.Empty).Length > 0)
            {
                bool IsOtherCondition = false;
                var ds = new DataSet();
                if (aPIDetail.ResponseTypeID == ResponseType.JSON)
                {
                    ds = _toDataSet.ReadDataFromJson(balanceResponse.Response);
                }
                else if (aPIDetail.ResponseTypeID == ResponseType.XML)
                {
                    ds = _toDataSet.ReadDataFromXML(balanceResponse.Response);
                }
                else
                {
                    #region MatchResponseFor String,CSV and Other
                    IsOtherCondition = true;
                    #endregion
                }
                if (ds.Tables.Count > 0 && !IsOtherCondition)
                {
                    bool IsBalanceKeyFound = aPIDetail.BalanceKey == "", IsBalanceFound = false;
                    foreach (DataTable tbl in ds.Tables)
                    {
                        if (!IsBalanceKeyFound)
                        {
                            if (tbl.Columns.Contains(aPIDetail.BalanceKey))
                            {
                                IsBalanceFound = true;
                                var bal = tbl.Rows[0][aPIDetail.BalanceKey].ToString().ToUpper();
                                if (Validate.O.IsNumeric((bal ?? string.Empty).Replace(".", "")))
                                {
                                    balanceResponse.Balance = Convert.ToDecimal(bal);
                                }
                            }
                            if (IsBalanceFound)
                                break;
                        }
                    }
                }
                if (IsOtherCondition)
                {
                    var apistatuscheck = new APISTATUSCHECK
                    {
                        Msg = Validate.O.ReplaceAllSpecials(balanceResponse.Response).Trim()
                    };
                    var _proc = new ProcCheckTextResponse(_dal);
                    apistatuscheck = (APISTATUSCHECK)await _proc.Call(apistatuscheck);
                    if (apistatuscheck.Statuscode == ErrorCodes.One)
                    {
                        if (Validate.O.IsNumeric((apistatuscheck.Balance ?? string.Empty).Replace(".", "")))
                        {
                            balanceResponse.Balance = Convert.ToDecimal(apistatuscheck.Balance);
                        }
                    }
                }
            }
        }
        public string GenerateRandomSessionNo(int length)
        {
            Random random = new Random();
            string text = string.Empty;
            for (int i = 0; i < length; i++)
            {
                text += random.Next(10).ToString();
            }
            return text;
        }
        #endregion
    }
}
