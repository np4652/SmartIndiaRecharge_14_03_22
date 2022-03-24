using Fintech.AppCode.Configuration;
using Fintech.AppCode.DB;
using Fintech.AppCode.HelperClass;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using RoundpayFinTech.AppCode.Configuration;
using RoundpayFinTech.AppCode.DL;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.BBPS;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.Model.ROffer;
using RoundpayFinTech.AppCode.StaticModel.MoneyTransfer;
using RoundpayFinTech.AppCode.ThirdParty.CyberPlate;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Validators;

namespace RoundpayFinTech.AppCode.MiddleLayer
{
    public sealed class TransactionML : ITransactionML
    {
        private readonly IDAL _dal;
        private readonly IRequestInfo _info;
        public TransactionML(IDAL dal, IRequestInfo info)
        {
            _dal = dal;
            _info = info;
        }
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        public TransactionML(IHttpContextAccessor accessor, IHostingEnvironment env, IDAL dal, IRequestInfo info)
        {
            _accessor = accessor;
            _env = env;
            _dal = dal;
            _info = info;
        }
        public async Task<TransactionResponse> DoTransaction(_RechargeAPIRequest req)
        {
            var res = new TransactionResponse
            {
                ACCOUNT = req.Account,
                AMOUNT = req.Amount,
                AGENTID = req.APIRequestID,
                STATUS = RechargeRespType.FAILED
            };
            #region RequestValidationFromCode  
            /**
             * Request validation from Code started
             * **/
            var validator = Validate.O;
            if (req.UserID < 2)
            {
                res.ERRORCODE = ErrorCodes.Invalid_Parameter.ToString();
                res.MSG = "Unauthorised access!";
                return res;
            }

            if (!validator.IsAlphaNumeric(req.Account ?? string.Empty) && !(req.Account ?? string.Empty).Contains("-") && !(req.Account ?? string.Empty).Contains("_"))
            {
                res.ERRORCODE = ErrorCodes.Invalid_Parameter.ToString();
                res.MSG = ErrorCodes.InvalidParam + " Account";
                return res;
            }
            if (req.Amount < 1)
            {
                res.ERRORCODE = ErrorCodes.Invalid_Parameter.ToString();
                res.MSG = ErrorCodes.InvalidParam + " Amount";
                return res;
            }
            if (req.RequestMode == RequestMode.APPS && ((req.IMEI ?? "").Length < 15 || (req.IMEI ?? "").Equals("00000000")))
            {
                res.MSG = ErrorCodes.InvalidParam + " IMEI";
                return res;
            }
            if (req.RequestMode == RequestMode.API && (req.Token ?? "").Length != 32)
            {
                res.ERRORCODE = ErrorCodes.Invalid_Parameter.ToString();
                res.MSG = ErrorCodes.InvalidParam + " Token";
                return res;
            }
            #endregion
            #region RequestValidationFromDB
            /**
             * Request validation from DB started
             * **/
            var validateReq = new ValidataRechargeApirequest
            {
                LoginID = req.UserID,
                IPAddress = req.IPAddress,
                Token = req.Token,
                SPKey = req.SPKey,
                OID = req.OID
            };
            IProcedureAsync _procValidate = new ProcValidataRechargeApirequest(_dal);
            var validateRes = (ValidataRechargeApiResp)await _procValidate.Call(validateReq).ConfigureAwait(false);
            if (validateRes.Statuscode == ErrorCodes.Minus1)
            {
                res.MSG = validateRes.Msg;
                res.ERRORCODE = validateRes.ErrorCode;
                return res;
            }
            if ((validateRes.SCode ?? string.Empty) != "GIS")
            {
                if (validateRes.OpParams != null)
                {
                    if (validateRes.OpParams.Count > 0)
                    {
                        var AccountParam = validateRes.OpParams.Where(w => w.IsAccountNo == true).FirstOrDefault();
                        if (!string.IsNullOrEmpty(AccountParam.Param))
                        {
                            AccountParam.GetFormatedError(nameof(req.Account), req.Account ?? string.Empty);
                            if (AccountParam.IsErrorFound)
                            {
                                res.ERRORCODE = ErrorCodes.Invalid_Parameter.ToString();
                                res.MSG = string.IsNullOrEmpty(AccountParam.Remark) ? AccountParam.FormatedError : AccountParam.Remark;
                                return res;
                            }
                            validateRes.OpParams.RemoveAll(x => x.IsAccountNo == true);
                        }
                        for (int i = 0; i < validateRes.OpParams.Count; i++)
                        {
                            var OpParam = validateRes.OpParams[i];
                            if (OpParam.IsOptional == false)
                            {
                                if (i == 0)
                                {
                                    OpParam.GetFormatedError(nameof(req.Optional1), req.Optional1 ?? string.Empty);
                                }
                                if (i == 1)
                                {
                                    OpParam.GetFormatedError(nameof(req.Optional2), req.Optional2 ?? string.Empty);
                                }
                                if (i == 2)
                                {
                                    OpParam.GetFormatedError(nameof(req.Optional3), req.Optional3 ?? string.Empty);
                                }
                                if (i == 3)
                                {
                                    OpParam.GetFormatedError(nameof(req.Optional4), req.Optional4 ?? string.Empty);
                                }
                                if (OpParam.IsErrorFound)
                                {
                                    res.ERRORCODE = ErrorCodes.Invalid_Parameter.ToString();
                                    res.MSG = string.IsNullOrEmpty(OpParam.Remark) ? OpParam.FormatedError : OpParam.Remark;
                                    return res;
                                }
                            }
                        }
                    }
                }
                #endregion
                if (validateRes.IsBBPS)
                {
                    //if ((req.FetchBillID==0) && string.IsNullOrEmpty(req.RefID) && validateRes.IsBilling)
                    //{
                    //    res.ERRORCODE = ErrorCodes.Invalid_Parameter.ToString();
                    //    res.MSG = ErrorCodes.InvalidParam + " RefID";
                    //    return res;
                    //}

                    if (string.IsNullOrEmpty(req.GEOCode))
                    {
                        res.ERRORCODE = ErrorCodes.Invalid_Parameter.ToString();
                        res.MSG = ErrorCodes.InvalidParam + " GEOCode";
                        return res;
                    }
                    req.GEOCode = GenerateGeoCode(req.GEOCode);
                    if (!Validate.O.IsMobile(req.CustomerNumber ?? string.Empty))
                    {
                        req.CustomerNumber = validateRes.MobileU;
                    }
                    if (!Validate.O.IsPinCode(req.Pincode ?? ""))
                    {
                        res.ERRORCODE = ErrorCodes.Invalid_Parameter.ToString();
                        res.MSG = ErrorCodes.InvalidParam + " Pincode";
                        return res;
                    }
                    if (req.FetchBillID == 0 && validateRes.IsBilling)
                    {
                        var transactionServiceReq = new TransactionServiceReq
                        {
                            UserID = req.UserID,
                            OID = req.OID,
                            AccountNo = req.Account,
                            OutletID = req.OutletID,
                            Optional1 = req.Optional1 ?? string.Empty,
                            Optional2 = req.Optional2 ?? string.Empty,
                            Optional3 = req.Optional3 ?? string.Empty,
                            Optional4 = req.Optional4 ?? string.Empty,
                            RefID = req.RefID,
                            GEOCode = string.IsNullOrEmpty(req.GEOCode) ? "25.5601,74.3401" : req.GEOCode,
                            CustomerNumber = req.CustomerNumber ?? string.Empty,
                            RequestModeID = RequestMode.APPS
                        };
                        if (transactionServiceReq.GEOCode.Length > 15)
                        {
                            var a = transactionServiceReq.GEOCode.Split(',')[0];
                            var b = transactionServiceReq.GEOCode.Split(',')[1];
                            a = Validate.O.IsNumeric(a.Replace(".", "")) ? string.Format("{0:0.0000}", Convert.ToDecimal(a)) : "25.5601";
                            b = Validate.O.IsNumeric(b.Replace(".", "")) ? string.Format("{0:0.0000}", Convert.ToDecimal(b)) : "74.3401";

                            transactionServiceReq.GEOCode = a + "," + b;
                        }
                        var bbpsML = new BBPSML(_accessor, _env, false);
                        var bBPSResponse = await bbpsML.FetchBillMLApp(transactionServiceReq).ConfigureAwait(false);
                        if (bBPSResponse != null)
                        {
                            req.FetchBillID = bBPSResponse.FetchBillID;
                        }
                    }
                }
                else
                {
                    if (!validator.IsAlphaNumeric(req.Account ?? string.Empty))
                    {
                        res.ERRORCODE = ErrorCodes.Invalid_Parameter.ToString();
                        res.MSG = ErrorCodes.InvalidParam + " Account";
                        return res;
                    }
                }
                #region CircleValidation
                /**
                 * Circle validation started
                 * **/
                const int CValidationType = 2;
                int CircleID = -1;
                try
                {
                    if (validateRes.CircleValidationType == CValidationType)
                    {
                        #region LookupSection
                        var lookupReq = new CommonReq
                        {
                            LoginTypeID = LoginType.ApplicationUser,
                            LoginID = req.UserID,
                            CommonStr = req.Account,
                            CommonStr2 = validateRes.OpType == OPTypes.Postpaid ? validateRes.SPKey : SPKeys.HLRVerification
                        };
                        IProcedure _procLookup = new ProcCheckNumberSeries(_dal);
                        var lookupRes = (HLRResponseStatus)_procLookup.Call(lookupReq);
                        if (lookupRes != null && lookupRes.Statuscode == ErrorCodes.One)
                        {
                            if ((lookupRes.CommonInt > 0 && lookupRes.CommonInt2 > 0) && string.IsNullOrEmpty(lookupRes.CommonStr3))
                            {
                                if (lookupRes.CommonInt != validateRes.OID)
                                {
                                    if ((validateRes.OpGroupID == 0 || (lookupRes.Status != validateRes.OpGroupID)) && !lookupRes.CommonBool2)
                                    {
                                        res.MSG = nameof(ErrorCodes.Invalid_choice_of_operater_number_belongs_to_oprater_).Replace("_", " ") + lookupRes.CommonStr2;
                                        res.ERRORCODE = ErrorCodes.Invalid_choice_of_operater_number_belongs_to_oprater_.ToString();
                                        return res;
                                    }
                                }
                                CircleID = lookupRes.CommonInt2;
                            }
                            else
                            {
                                bool IsLookupAPIHit = false;
                                try
                                {
                                    if (lookupRes.HLRAPIs.Count > 0)
                                    {
                                        foreach (var item in lookupRes.HLRAPIs)
                                        {
                                            IsLookupAPIHit = true;
                                            IReportML _rml = new ReportML(_accessor, _env);
                                            lookupRes = _rml.HitHLRAPIs(item.APICode, item.APIURL, item.APIID, req.UserID, req.Account);
                                            if (!string.IsNullOrEmpty(lookupRes.CommonStr))
                                            {
                                                if (lookupRes.CommonStr.Equals(LookupAPIType.AirtelPPHLR) && validateRes.OpType == OPTypes.Postpaid)
                                                {
                                                    if (!lookupRes.CommonBool)
                                                    {
                                                        res.STATUS = RechargeRespType.FAILED;
                                                        res.MSG = "Please check Your Transaction Account. Its seems Prepaid Number.";
                                                        res.ERRORCODE = "283";
                                                        return res;
                                                    }
                                                }
                                            }
                                            if ((lookupRes.CommonInt > 0) && !string.IsNullOrEmpty(lookupRes.CommonStr2))
                                                break;
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    var errorLog = new ErrorLog
                                    {
                                        ClassName = GetType().Name,
                                        FuncName = "CircleValidation Call",
                                        Error = (req.Account ?? string.Empty) + ":" + ex.Message,
                                        LoginTypeID = 1,
                                        UserId = 1
                                    };
                                    var _ = new ProcPageErrorLog(_dal).Call(errorLog);
                                }

                                if (lookupRes != null)
                                {
                                    if (lookupRes.CommonInt > 0 && lookupRes.CommonInt2 > 0 && IsLookupAPIHit)
                                    {
                                        if (lookupRes.CommonInt != validateRes.OID)
                                        {
                                            if ((validateRes.OpGroupID == 0 || (lookupRes.Status != validateRes.OpGroupID)) && !lookupRes.CommonBool)
                                            {
                                                res.MSG = nameof(ErrorCodes.Invalid_choice_of_operater_number_belongs_to_oprater_).Replace("_", " ") + lookupRes.CommonStr2;
                                                res.ERRORCODE = ErrorCodes.Invalid_choice_of_operater_number_belongs_to_oprater_.ToString();
                                                return res;
                                            }
                                        }
                                    }
                                    CircleID = lookupRes.CommonInt2 > 0 ? lookupRes.CommonInt2 : ErrorCodes.Minus1;
                                }
                                else
                                {
                                    CircleID = ErrorCodes.Minus1;
                                }
                            }
                        }
                        #endregion
                    }
                }
                catch (Exception ex)
                {
                    var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                    {
                        ClassName = GetType().Name,
                        FuncName = "CircleValidation",
                        Error = ex.Message,
                        LoginTypeID = 1,
                        UserId = 1
                    });
                }
                #endregion
                res.STATUS = RechargeRespType.PENDING;
                var TSReq = new TransactionServiceReq
                {
                    OID = validateRes.OID,
                    CircleID = CircleID,
                    UserID = req.UserID,
                    AccountNo = req.Account,
                    AmountR = req.Amount,
                    OPID = validateRes.SPKey,
                    APIRequestID = req.APIRequestID,
                    RequestModeID = req.RequestMode,
                    RequestIP = req.IPAddress,
                    Optional1 = req.Optional1,
                    Optional2 = req.Optional2,
                    Optional3 = req.Optional3,
                    Optional4 = req.Optional4,
                    OutletID = req.OutletID,
                    RefID = req.RefID,
                    GEOCode = req.GEOCode,
                    CustomerNumber = req.CustomerNumber,
                    PinCode = req.Pincode,
                    IMEI = req.IMEI ?? string.Empty,
                    SecurityKey = req.SecurityKey ?? string.Empty,
                    IsReal = req.IsReal,
                    FetchBillID = req.FetchBillID,
                    CCFAmount = req.CCFAmount,
                    PaymentMode = req.PaymentMode
                };
                IProcedureAsync proc = new ProcTransactionService(_dal);
                var TSResp = (TransactionServiceResp)await proc.Call(TSReq).ConfigureAwait(false);
                #region ReadTransactionStatusAndUpdate
                res.BAL = TSResp.Balance;
                if (TSResp.Statuscode == ErrorCodes.Minus1)
                {
                    res.STATUS = RechargeRespType.FAILED;
                    res.MSG = TSResp.Msg;
                    res.ERRORCODE = TSResp.ErrorCode.ToString();
                    return res;
                }
                else if (TSResp.Statuscode == ErrorCodes.One)
                {
                    if (string.IsNullOrEmpty(TSReq.PaymentMode))
                    {
                        TSReq.PaymentMode = TSResp.PaymentMode;
                    }
                    TSReq.PAN = TSResp.PAN;
                    TSReq.Aadhar = TSResp.Aadhar;
                    res.RPID = TSResp.TransactionID;
                    TSResp.CurrentAPI.IsBBPS = TSResp.IsBBPS;
                    req.RefID = string.IsNullOrEmpty(TSResp.REFID) ? req.RefID : TSResp.REFID;
                    var transactionHelper = new TransactionHelper(_dal, _accessor, _env);
                    var tstatus = await transactionHelper.HitGetStatus(TSResp.TID, TSReq, TSResp.CurrentAPI, TSResp.MoreAPIs, false, TSResp).ConfigureAwait(false);
                    tstatus.TID = TSResp.TID;
                    tstatus.UserID = req.UserID;
                    res.OPID = tstatus.OperatorID;
                    res.MSG = tstatus.ErrorMsg;
                    res.ERRORCODE = tstatus.ErrorCode;
                    res.VendorID = tstatus.VendorID;
                    if (tstatus.Status == RechargeRespType.SUCCESS)
                    {
                        res.IsBBPS = TSResp.IsBBPS;
                    }
                    res.STATUS = tstatus.Status;
                    bool IsSameSessionFailed = tstatus.Status == RechargeRespType.FAILED;
                    #region UpdateTransactionSatus
                    IProcedureAsync _updateProc = new ProcUpdateTransactionServiceStatus(_dal);
                    if (tstatus.APIType == APITypes.Lapu)
                    {
                        if (tstatus.APIID != TSResp.CurrentAPI.ID)
                        {
                            tstatus.Status = RechargeRespType.REQUESTSENT;
                        }
                        tstatus.OperatorID = nameof(ErrorCodes.Request_Accpeted).Replace("_", " ");
                        res.OPID = string.Empty;
                        res.MSG = tstatus.OperatorID;
                        res.ERRORCODE = ErrorCodes.Request_Accpeted.ToString();
                        res.STATUS = RechargeRespType.PENDING;
                    }
                    var CallbackData_ = new _CallbackData();
                    if (!TSResp.CurrentAPI.APIType.In(APITypes.Lapu, APITypes.Manual) || tstatus.Status == RechargeRespType.REQUESTSENT)
                    {
                        CallbackData_ = (_CallbackData)await _updateProc.Call(tstatus).ConfigureAwait(false);
                        if (CallbackData_.Statuscode == ErrorCodes.Minus1 && tstatus.Status.In(RechargeRespType.SUCCESS, RechargeRespType.FAILED))
                        {
                            res.STATUS = RechargeRespType.PENDING;
                        }
                    }
                    if (CallbackData_.TransactionStatus > 1)
                    {
                        res.STATUS = CallbackData_.TransactionStatus;
                        res.OPID = CallbackData_.LiveID;
                    }

                    #endregion

                    if (res.STATUS == RechargeRespType.PENDING)
                    {
                        res.MSG = RechargeRespType._PENDING;
                        res.ERRORCODE = ErrorCodes.Request_Accpeted.ToString();
                    }
                    else if (res.STATUS == RechargeRespType.SUCCESS)
                    {
                        res.MSG = RechargeRespType._SUCCESS;
                        res.ERRORCODE = ErrorCodes.Transaction_Successful.ToString();
                        if (TSResp.IsBBPS && !string.IsNullOrEmpty(req.CustomerNumber))
                        {
                            var alertParam = new AlertReplacementModel
                            {
                                LoginID = req.UserID,
                                WID = TSResp.WID,
                                AccountNo = req.Account,
                                UserMobileNo = req.CustomerNumber,
                                Amount = req.Amount,
                                Operator = validateRes.Operator,
                                TransactionID = TSResp.TransactionID,
                                FormatID = MessageFormat.BBPSSuccess,
                                DATETIME = DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt")
                            };
                            IAlertML alertMl = new AlertML(_accessor, _env);
                            alertMl.BBPSSuccessSMS(alertParam);
                        }
                    }
                    else if (res.STATUS == RechargeRespType.FAILED && !IsSameSessionFailed)
                    {
                        res.MSG = string.IsNullOrEmpty(CallbackData_.LiveID) ? RechargeRespType._FAILED : CallbackData_.LiveID;
                        res.ERRORCODE = ErrorCodes.Transaction_Failed_Replace.ToString();
                    }
                }
                #endregion
            }
            else
            {
                res.STATUS = RechargeRespType.PENDING;
                var TSReqCoupon = new TransactionServiceReq
                {
                    OID = validateRes.OID,
                    UserID = req.UserID,
                    AccountNo = req.Account,
                    AmountR = req.Amount,
                    OPID = validateRes.SPKey,
                    APIRequestID = req.APIRequestID,
                    RequestModeID = req.RequestMode,
                    RequestIP = req.IPAddress,
                    Optional1 = req.Optional1,
                    Optional2 = req.Optional2,
                    Optional3 = req.Optional3,
                    Optional4 = req.Optional4,
                    OutletID = req.OutletID,
                    RefID = req.RefID,
                    CustomerNumber = req.CustomerNumber,
                    PinCode = req.Pincode,
                    IMEI = req.IMEI ?? string.Empty
                };
                IProcedure proc = new proc_VoucherCouponService(_dal);
                var TRespCoupon = (TransactionServiceResp)proc.Call(new CouponData
                {
                    LoginID = req.UserID,
                    Quantity = Validate.O.IsNumeric(req.Optional1 ?? string.Empty) ? Convert.ToInt32(req.Optional1) : 1,
                    Amount = req.Amount,
                    OID = validateRes.OID,
                    To = req.Optional2,
                    CommonStr = req.CustomerNumber,
                    Message = req.Optional3,
                    RequestIP = req.IPAddress,
                    Browser = _info.GetBrowser(),
                    RequestMode = req.RequestMode
                });
                if (TRespCoupon.Statuscode == ErrorCodes.Minus1)
                {
                    res.STATUS = RechargeRespType.FAILED;
                    res.MSG = TRespCoupon.Msg;
                    res.ERRORCODE = TRespCoupon.ErrorCode.ToString();
                    return res;
                }
                var transactionHelper = new TransactionHelper(_dal, _accessor, _env);
                var tstatus = await transactionHelper.HitGetStatus(TRespCoupon.TID, TSReqCoupon, TRespCoupon.CurrentAPI, TRespCoupon.MoreAPIs, false, TRespCoupon).ConfigureAwait(false);
                tstatus.TID = TRespCoupon.TID;
                tstatus.UserID = req.UserID;
                res.OPID = tstatus.OperatorID;
                res.MSG = tstatus.ErrorMsg;
                res.ERRORCODE = tstatus.ErrorCode;
                res.VendorID = tstatus.VendorID;

                res.STATUS = tstatus.Status;
                bool IsSameSessionFailed = tstatus.Status == RechargeRespType.FAILED;
                #region UpdateTransactionSatus
                IProcedureAsync _updateProc = new ProcUpdateTransactionServiceStatus(_dal);
                if (tstatus.APIType == APITypes.Lapu)
                {
                    if (tstatus.APIID != TRespCoupon.CurrentAPI.ID)
                    {
                        tstatus.Status = RechargeRespType.REQUESTSENT;
                    }
                    tstatus.OperatorID = nameof(ErrorCodes.Request_Accpeted).Replace("_", " ");
                    res.OPID = string.Empty;
                    res.MSG = tstatus.OperatorID;
                    res.ERRORCODE = ErrorCodes.Request_Accpeted.ToString();
                    res.STATUS = RechargeRespType.PENDING;
                }
                var CallbackData_ = new _CallbackData();
                if (!TRespCoupon.CurrentAPI.APIType.In(APITypes.Lapu, APITypes.Manual) || tstatus.Status == RechargeRespType.REQUESTSENT)
                {
                    CallbackData_ = (_CallbackData)await _updateProc.Call(tstatus).ConfigureAwait(false);
                    if (CallbackData_.Statuscode == ErrorCodes.Minus1 && tstatus.Status.In(RechargeRespType.SUCCESS, RechargeRespType.FAILED))
                    {
                        res.STATUS = RechargeRespType.PENDING;
                    }
                }
                if (CallbackData_.TransactionStatus > 1)
                {
                    res.STATUS = CallbackData_.TransactionStatus;
                    res.OPID = CallbackData_.LiveID;
                }
                #endregion
                if (res.STATUS == RechargeRespType.PENDING)
                {
                    res.MSG = RechargeRespType._PENDING;
                    res.ERRORCODE = ErrorCodes.Request_Accpeted.ToString();
                }
                else if (res.STATUS == RechargeRespType.SUCCESS)
                {
                    res.MSG = RechargeRespType._SUCCESS;
                    res.ERRORCODE = ErrorCodes.Transaction_Successful.ToString();
                    AlertReplacementModel param = new AlertReplacementModel();
                    param.CouponCode = string.IsNullOrEmpty(res.OPID) ? TRespCoupon.LiveID : res.OPID;
                    param.Amount = req.Amount;
                    param.CouponQty = Validate.O.IsNumeric(req.Optional1 ?? string.Empty) ? Convert.ToInt32(req.Optional1) : 1;
                    param.CouponValdity = 0;
                    param.UserID = req.UserID;
                    param.UserName = TRespCoupon.UserName;
                    param.UserEmailID = req.Optional2;
                    param.LoginID = req.UserID;
                    param.WID = 1;
                    param.FormatID = 34;
                    var a = new AlertML(_accessor, _env).CouponVocherEmail(param, true);
                }
                else if (res.STATUS == RechargeRespType.FAILED && !IsSameSessionFailed)
                {
                    res.MSG = string.IsNullOrEmpty(CallbackData_.LiveID) ? RechargeRespType._FAILED : CallbackData_.LiveID;
                    res.ERRORCODE = ErrorCodes.Transaction_Failed_Replace.ToString();
                }
            }
            return res;
        }
        public async Task<TransactionResponse> DoFetchBill(_RechargeAPIRequest req)
        {
            var res = new TransactionResponse
            {
                ACCOUNT = req.Account,
                AMOUNT = req.Amount,
                AGENTID = req.APIRequestID,
                STATUS = RechargeRespType.FAILED,
                ERRORCODE = ErrorCodes.Unable_to_get_bill_details_Please_try_again_after_few_minute.ToString(),
                MSG = nameof(ErrorCodes.Unable_to_get_bill_details_Please_try_again_after_few_minute).Replace("_", " "),
                IsBillFetch = true
            };
            #region RequestValidationFromCode  
            /**
             * Request validation from Code started
             * **/
            var validator = Validate.O;
            if (req.UserID < 2)
            {
                res.ERRORCODE = ErrorCodes.Invalid_Parameter.ToString();
                res.MSG = "Unauthorised access!";
                return res;
            }

            if (!validator.IsAlphaNumeric(req.Account ?? string.Empty) && !(req.Account ?? string.Empty).Contains("-") && !(req.Account ?? string.Empty).Contains("_"))
            {
                res.ERRORCODE = ErrorCodes.Invalid_Parameter.ToString();
                res.MSG = ErrorCodes.InvalidParam + " Account";
                return res;
            }
            if (validator.IsGeoCodeInValid(req.GEOCode))
            {
                res.ERRORCODE = ErrorCodes.Invalid_Parameter.ToString();
                res.MSG = ErrorCodes.InvalidParam + " GEOCode. Latitude and Longitude must be followed by 4 decimal palces";
                return res;
            }
            if (req.RequestMode == RequestMode.API && (req.Token ?? "").Length != 32)
            {
                res.ERRORCODE = ErrorCodes.Invalid_Parameter.ToString();
                res.MSG = ErrorCodes.InvalidParam + " Token";
                return res;
            }
            if (!Validate.O.IsPinCode(req.Pincode ?? string.Empty))
            {
                res.ERRORCODE = ErrorCodes.Invalid_Parameter.ToString();
                res.MSG = ErrorCodes.InvalidParam + nameof(req.Pincode);
                return res;
            }
            if (req.OutletID < 10000)
            {
                res.ERRORCODE = ErrorCodes.Invalid_Parameter.ToString();
                res.MSG = ErrorCodes.InvalidParam + nameof(req.OutletID);
                return res;
            }
            if (!Validate.O.IsMobile(req.CustomerNumber))
            {
                res.ERRORCODE = ErrorCodes.Invalid_Parameter.ToString();
                res.MSG = ErrorCodes.InvalidParam + nameof(req.CustomerNumber);
                return res;
            }
            #endregion
            #region RequestValidationFromDB
            /**
             * Request validation from DB started
             * **/
            var validateReq = new ValidataRechargeApirequest
            {
                LoginID = req.UserID,
                IPAddress = req.IPAddress,
                Token = req.Token,
                SPKey = req.SPKey,
                OID = req.OID
            };
            IProcedureAsync _procValidate = new ProcValidataRechargeApirequest(_dal);
            var validateRes = (ValidataRechargeApiResp)await _procValidate.Call(validateReq).ConfigureAwait(false);
            if (validateRes.Statuscode == ErrorCodes.Minus1)
            {
                res.MSG = validateRes.Msg;
                res.ERRORCODE = validateRes.ErrorCode;
                return res;
            }
            if (validateRes.OpParams != null)
            {
                if (validateRes.OpParams.Count > 0)
                {
                    var AccountParam = validateRes.OpParams.Where(w => w.IsAccountNo == true).FirstOrDefault();
                    if (!string.IsNullOrEmpty(AccountParam.Param))
                    {
                        AccountParam.GetFormatedError(nameof(req.Account), req.Account ?? string.Empty);
                        if (AccountParam.IsErrorFound)
                        {
                            res.ERRORCODE = ErrorCodes.Invalid_Parameter.ToString();
                            res.MSG = string.IsNullOrEmpty(AccountParam.Remark) ? AccountParam.FormatedError : AccountParam.Remark;
                            return res;
                        }
                        validateRes.OpParams.RemoveAll(x => x.IsAccountNo == true);
                    }
                    for (int i = 0; i < validateRes.OpParams.Count; i++)
                    {
                        var OpParam = validateRes.OpParams[i];
                        if (OpParam.IsOptional == false)
                        {
                            if (i == 0)
                            {
                                OpParam.GetFormatedError(nameof(req.Optional1), req.Optional1 ?? string.Empty);

                            }
                            if (i == 1)
                            {
                                OpParam.GetFormatedError(nameof(req.Optional2), req.Optional2 ?? string.Empty);
                            }
                            if (i == 2)
                            {
                                OpParam.GetFormatedError(nameof(req.Optional3), req.Optional3 ?? string.Empty);
                            }
                            if (i == 3)
                            {
                                OpParam.GetFormatedError(nameof(req.Optional4), req.Optional4 ?? string.Empty);
                            }
                            if (OpParam.IsErrorFound)
                            {
                                res.ERRORCODE = ErrorCodes.Invalid_Parameter.ToString();
                                res.MSG = string.IsNullOrEmpty(OpParam.Remark) ? OpParam.FormatedError : OpParam.Remark;
                                return res;
                            }
                        }
                    }
                }
            }
            #endregion
            if (validateRes.IsBilling)
            {
                var transactionServiceReq = new TransactionServiceReq
                {
                    UserID = req.UserID,
                    OID = validateRes.OID,
                    AccountNo = req.Account,
                    OutletID = req.OutletID,
                    Optional1 = req.Optional1,
                    Optional2 = req.Optional2,
                    Optional3 = req.Optional3,
                    Optional4 = req.Optional4,
                    RefID = req.RefID,
                    GEOCode = req.GEOCode,
                    PinCode = req.Pincode,
                    CustomerNumber = req.CustomerNumber,
                    RequestModeID = RequestMode.API,
                    APIRequestID = req.APIRequestID,
                    RequestIP = req.IPAddress
                };
                var bbpsResponse = new BBPSResponse
                {
                    Statuscode = ErrorCodes.Minus1,
                    ErrorCode = ErrorCodes.Unable_to_get_bill_details_Please_try_again_after_few_minute.ToString(),
                    ErrorMsg = nameof(ErrorCodes.Unable_to_get_bill_details_Please_try_again_after_few_minute).Replace("_", " ")
                };
                IProcedureAsync proc = new ProcGenerateBillFetchURL(_dal);
                var tresp = (TransactionServiceResp)await proc.Call(transactionServiceReq).ConfigureAwait(false);
                if (tresp.Statuscode == ErrorCodes.One)
                {
                    if (tresp.CurrentAPI != null)
                    {
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
                                OPID = transactionServiceReq.OID.ToString(),
                                SessionNo = "A" + tresp.TID,
                                BillNumber = transactionServiceReq.AccountNo,
                                RequestURL = tresp.CurrentAPI.URL,
                                UserID = transactionServiceReq.UserID,
                                Optional1 = transactionServiceReq.Optional1,
                                Optional2 = transactionServiceReq.Optional2,
                                Optional3 = transactionServiceReq.Optional3,
                                Optional4 = transactionServiceReq.Optional4,
                                aPIDetail = tresp.CurrentAPI,
                                Amount = tresp.CurrentAPI.APICode == APICode.CYBERPLAT ? 100.00M : req.Amount,
                                AccountNumber = transactionServiceReq.AccountNo,
                                GeoLocation = transactionServiceReq.GEOCode,
                                Pincode = tresp.Pincode,
                                PAN = tresp.PAN,
                                AadharNo = tresp.Aadhar,
                                CustomerName = tresp.CustomerName,
                                CustomerMobile = transactionServiceReq.CustomerNumber,
                                CircleCode = tresp.CircleCode,
                                APIReqHelper = new BBPSAPIReqHelper
                                {
                                    AccountNoKey = tresp.AccountNoKey,
                                    RegxAccount = tresp.RegxAccount,
                                    BillerID = tresp.BillerID,
                                    InitChanel = tresp.InitChanel,
                                    IPAddress = transactionServiceReq.RequestIP,
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
                            objBBPSLog.APIOptionalList = opml.AOPMappingAPP(tresp.CurrentAPI.ID, transactionServiceReq.OID);
                            var TransHelper = new TransactionHelper(_dal, _accessor, _env);
                            bbpsResponse = await TransHelper.HitFetchBillAPI(objBBPSLog).ConfigureAwait(false);
                            //if (!bbpsResponse.IsHardCoded)
                            //{
                            //    await TransHelper.ResponseHandlerFetchBill(bbpsResponse, objBBPSLog).ConfigureAwait(false);
                            //}
                            if (bbpsResponse.Statuscode == ErrorCodes.One)
                            {
                                bbpsResponse.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                                bbpsResponse.ErrorCode = ErrorCodes.Transaction_Successful.ToString();
                                bbpsResponse.ErrorMsg = bbpsResponse.Msg;
                            }
                        }
                    }
                    else
                    {
                        bbpsResponse.IsShowMsgOnly = true;
                        bbpsResponse.ErrorMsg = "(NFA)Service Provider Error, Try Later!";
                        bbpsResponse.Statuscode = ErrorCodes.Minus1;
                        bbpsResponse.ErrorCode = "407";
                    }
                }
                else
                {
                    bbpsResponse.IsShowMsgOnly = true;
                    bbpsResponse.ErrorMsg = tresp.Msg;
                    bbpsResponse.Msg = tresp.Msg;
                    bbpsResponse.ErrorCode = tresp.ErrorCode.ToString();

                }
                bbpsResponse.ErrorMsg = string.IsNullOrEmpty(bbpsResponse.ErrorMsg) ? bbpsResponse.Msg : bbpsResponse.ErrorMsg;

                var amt = (bbpsResponse.Amount ?? string.Empty).Trim().Length > 0 ? Convert.ToDecimal((bbpsResponse.Amount ?? string.Empty).Trim()) : 0;
                if (bbpsResponse.Statuscode != ErrorCodes.Minus1 && amt > 0)
                {
                    res.STATUS = RechargeRespType.SUCCESS;
                    res.ERRORCODE = bbpsResponse.ErrorCode;
                    res.MSG = bbpsResponse.ErrorMsg.ToString();
                    res.CUSTOMERNAME = bbpsResponse.CustomerName;
                    res.BILLDATE = bbpsResponse.BillDate;
                    res.BILLNUMBER = bbpsResponse.BillNumber;
                    res.BILPERIOD = bbpsResponse.BillPeriod;
                    res.CUSTOMERNAME = bbpsResponse.CustomerName;
                    res.DUEAMOUNT = amt;
                    res.DUEDATE = bbpsResponse.DueDate;
                    res.REFID = bbpsResponse.RefferenceID;
                    res.FetchBillID = bbpsResponse.FetchBillID;
                }
                else
                {
                    res.ERRORCODE = bbpsResponse.ErrorCode;
                    res.MSG = bbpsResponse.ErrorMsg;
                }
            }
            else
            {
                res.ERRORCODE = ErrorCodes.Bill_fetch_service_not_available_for_provider_OPERATOR.ToString();
                res.MSG = nameof(ErrorCodes.Bill_fetch_service_not_available_for_provider_OPERATOR).Replace("_", " ").Replace("OPERATOR", validateRes.Operator);
            }
            if (res.STATUS == RechargeRespType.FAILED && res.ERRORCODE == " ")
            {
                res.ERRORCODE = ErrorCodes.Unable_to_get_bill_details_Please_try_again_after_few_minute.ToString();
                res.MSG = nameof(ErrorCodes.Unable_to_get_bill_details_Please_try_again_after_few_minute).Replace("_", " ");
            }
            return res;
        }

        public async Task<TransactionResponse> CheckStatus(_StatusAPIRequest req, IConnectionConfiguration _c)
        {
            req.IPAddress = _info.GetRemoteIP();
            var res = new TransactionResponse
            {
                AGENTID = req.AgentID ?? string.Empty,
                RPID = req.RPID ?? string.Empty,
                MSG = "No record found",
                ERRORCODE = ErrorCodes.Request_Accpeted.ToString()
            };
            var IsDateRequiredFromDB = false;
            #region RequestValidationFromCode  
            /**
             * Request validation from Code started
             * **/
            if (req.UserID < 2)
            {
                res.MSG = nameof(ErrorCodes.Invalid_Access);
                res.ERRORCODE = ErrorCodes.Invalid_Access.ToString();
                return res;
            }
            if ((string.IsNullOrEmpty(req.Token) || req.Token.Length != 32) && !req.IsPageCall)
            {
                res.MSG = "Invalid Credentials!";
                res.ERRORCODE = ErrorCodes.Invalid_Access_Token.ToString();
                return res;
            }
            if (string.IsNullOrEmpty(req.AgentID) && string.IsNullOrEmpty(req.RPID))
            {
                res.MSG = nameof(ErrorCodes.Invalid_Parameter) + " AgentID and TransactionID both are empty!";
                res.ERRORCODE = ErrorCodes.Invalid_Parameter.ToString();
                return res;
            }
            if (string.IsNullOrEmpty(req.RPID))
            {
                if (!string.IsNullOrEmpty(req.AgentID))
                {
                    if (!Validate.O.IsDateIn_dd_MMM_yyyy_Format(req.Optional1 ?? ""))
                    {
                        IsDateRequiredFromDB = true;
                    }
                }

            }
            if (string.IsNullOrEmpty(req.AgentID) && !Validate.O.IsTransactionIDValid(req.RPID ?? ""))
            {
                res.MSG = nameof(ErrorCodes.Invalid_Parameter) + "No transaction found for given RPID!";
                res.ERRORCODE = ErrorCodes.Invalid_Parameter.ToString();
                return res;
            }
            #endregion

            #region RequestValidationFromDB
            /**
             * Request validation from DB started
             * **/
            var validateReq = new ValidataRechargeApirequest
            {
                LoginID = req.UserID,
                IPAddress = req.IPAddress,
                Token = req.Token,
                SPKey = "SPKEY"
            };
            IProcedureAsync _procValidate = new ProcValidataRechargeApirequest(_dal);
            var validateRes = (ValidataRechargeApiResp)await _procValidate.Call(validateReq).ConfigureAwait(false);
            if (validateRes.Statuscode == ErrorCodes.Minus1)
            {
                res.MSG = validateRes.Msg;
                res.ERRORCODE = validateRes.ErrorCode;
                return res;
            }
            #endregion
            #region APIRequestIDDB
            if (IsDateRequiredFromDB)
            {
                req.Optional1 = new ProcGetTransactionStatus(_dal).GetDateForAPIRequestID(req.AgentID, req.UserID);
                if (Validate.O.IsDateIn_dd_MMM_yyyy_Format(req.Optional1 ?? "") == false)
                {
                    res.MSG = nameof(ErrorCodes.Invalid_Parameter) + " Invalid parameter Optional1 date format should be in \"dd MMM yyyy\"!";
                    res.ERRORCODE = ErrorCodes.Invalid_Parameter.ToString();
                    return res;
                }
            }

            #endregion
            #region StatusCheckPart
            string TransDate = string.IsNullOrEmpty(req.RPID) ? req.Optional1 : ConnectionStringHelper.O.ConvertTransactionIDTo_dd_MMM_yyyy(req.RPID);

            var commonReq = new CommonReq
            {
                LoginID = req.UserID,
                CommonStr = req.RPID,
                CommonStr2 = req.AgentID
            };
            IProcedureAsync _proc = new ProcGetTransactionStatus(ChangeConString(TransDate, _c));
            res = (TransactionResponse)await _proc.Call(commonReq).ConfigureAwait(false);
            #endregion

            return res;
        }
        public async Task<DMTCheckStatusResponse> CheckDMTStatus(_StatusAPIRequest req, IConnectionConfiguration _c)
        {
            req.IPAddress = _info.GetRemoteIP();
            var res = new DMTCheckStatusResponse
            {
                AgentID = req.AgentID ?? string.Empty,
                TransactionID = req.RPID ?? string.Empty,
                Message = "No record found",
                ErrorCode = ErrorCodes.Request_Accpeted
            };
            #region RequestValidationFromCode  
            /**
             * Request validation from Code started
             * **/
            if (req.UserID < 2)
            {
                res.Message = nameof(ErrorCodes.Invalid_Access);
                res.ErrorCode = ErrorCodes.Invalid_Access;
                return res;
            }
            if ((string.IsNullOrEmpty(req.Token) || req.Token.Length != 32) && !req.IsPageCall)
            {
                res.Message = "Invalid Credentials!";
                res.ErrorCode = ErrorCodes.Invalid_Access_Token;
                return res;
            }
            if (string.IsNullOrEmpty(req.AgentID) && string.IsNullOrEmpty(req.RPID))
            {

                res.Message = nameof(ErrorCodes.Invalid_Parameter) + " AgentID and TransactionID both are empty!";
                res.ErrorCode = ErrorCodes.Invalid_Parameter;
                return res;
            }
            if (string.IsNullOrEmpty(req.RPID) && !string.IsNullOrEmpty(req.AgentID) && !Validate.O.IsDateIn_dd_MMM_yyyy_Format(req.Optional1 ?? ""))
            {
                res.Message = nameof(ErrorCodes.Invalid_Parameter) + " Invalid parameter Optional1 date format should be in \"dd MMM yyyy\"!";
                res.ErrorCode = ErrorCodes.Invalid_Parameter;
                return res;
            }
            if (string.IsNullOrEmpty(req.AgentID) && !Validate.O.IsTransactionIDValid(req.RPID ?? ""))
            {
                res.Message = nameof(ErrorCodes.Invalid_Parameter) + "No transaction found for given RPID!";
                res.ErrorCode = ErrorCodes.Invalid_Parameter;
                return res;
            }
            #endregion

            #region RequestValidationFromDB
            /**
             * Request validation from DB started
             * **/
            var validateReq = new ValidataRechargeApirequest
            {
                LoginID = req.UserID,
                IPAddress = req.IPAddress,
                Token = req.Token,
                SPKey = "SPKEY"
            };
            IProcedureAsync _procValidate = new ProcValidataRechargeApirequest(_dal);
            var validateRes = (ValidataRechargeApiResp)await _procValidate.Call(validateReq);
            if (validateRes.Statuscode == ErrorCodes.Minus1)
            {
                res.Message = validateRes.Msg;
                res.ErrorCode = Convert.ToInt32(string.IsNullOrEmpty(validateRes.ErrorCode) ? "0" : validateRes.ErrorCode);
                return res;
            }
            #endregion

            #region StatusCheckPart
            string TransDate = string.IsNullOrEmpty(req.RPID) ? req.Optional1 : ConnectionStringHelper.O.ConvertTransactionIDTo_dd_MMM_yyyy(req.RPID);

            var commonReq = new CommonReq
            {
                LoginID = req.UserID,
                CommonStr = req.RPID,
                CommonStr2 = req.AgentID
            };
            IProcedureAsync _proc = new ProcGetDMTTransactionStatus(ChangeConString(TransDate, _c));
            res = (DMTCheckStatusResponse)await _proc.Call(commonReq).ConfigureAwait(false);
            #endregion
            return res;
        }

        private DAL ChangeConString(string _date, IConnectionConfiguration _c)
        {
            DAL dal = new DAL(_c.GetConnectionString());
            if (Validate.O.IsDateIn_dd_MMM_yyyy_Format(_date))
            {
                TypeMonthYear typeMonthYear = ConnectionStringHelper.O.GetTypeMonthYear(Convert.ToDateTime(_date.Replace(" ", "/")));
                if (typeMonthYear.ConType != ConnectionStringType.DBCon)
                {
                    dal = new DAL(_c.GetConnectionString(typeMonthYear.ConType, (typeMonthYear.MM ?? "") + "_" + (typeMonthYear.YYYY ?? "")));
                }
            }
            return dal;
        }

        public async Task<TransactionResponse> DoPSATransaction(_RechargeAPIRequest req)
        {
            var res = new TransactionResponse
            {
                ACCOUNT = req.Account,
                AMOUNT = req.Amount,
                AGENTID = req.APIRequestID,
                STATUS = RechargeRespType.FAILED
            };
            #region RequestValidationFromCode  
            /**
             * Request validation from Code started
             * **/
            var validator = Validate.O;
            if (req.UserID < 2)
            {
                res.ERRORCODE = ErrorCodes.Invalid_Parameter.ToString();
                res.MSG = "Unauthorised access!";
                return res;
            }

            if (!validator.IsAlphaNumeric(req.Account ?? ""))
            {
                res.ERRORCODE = ErrorCodes.Invalid_Parameter.ToString();
                res.MSG = ErrorCodes.InvalidParam + " Account";
                return res;
            }
            if (req.Amount < 1)
            {
                res.ERRORCODE = ErrorCodes.Invalid_Parameter.ToString();
                res.MSG = ErrorCodes.InvalidParam + " Amount";
                return res;
            }
            if (req.RequestMode == RequestMode.APPS && (req.IMEI ?? "").Length < 15)
            {
                res.MSG = ErrorCodes.InvalidParam + " IMEI";
                return res;
            }
            if (req.RequestMode == RequestMode.API && (req.Token ?? "").Length != 32)
            {
                res.ERRORCODE = ErrorCodes.Invalid_Parameter.ToString();
                res.MSG = ErrorCodes.InvalidParam + " Token";
                return res;
            }
            #endregion
            #region RequestValidationFromDB
            /**
             * Request validation from DB started
             * **/
            var validateReq = new ValidataRechargeApirequest
            {
                LoginID = req.UserID,
                IPAddress = req.IPAddress,
                Token = req.Token,
                SPKey = req.SPKey,
                OID = req.OID
            };
            IProcedureAsync _procValidate = new ProcValidataRechargeApirequest(_dal);
            var validateRes = (ValidataRechargeApiResp)await _procValidate.Call(validateReq).ConfigureAwait(false);
            if (validateRes.Statuscode == ErrorCodes.Minus1)
            {
                res.MSG = validateRes.Msg;
                return res;
            }
            #endregion

            res.STATUS = RechargeRespType.PENDING;
            var TSReq = new TransactionServiceReq
            {
                OID = validateRes.OID,
                UserID = req.UserID,
                AccountNo = req.Account,
                AmountR = req.Amount,
                OPID = validateRes.SPKey,
                APIRequestID = req.APIRequestID,
                RequestModeID = req.RequestMode,
                RequestIP = req.IPAddress,
                OutletID = req.OutletID,
                IMEI = req.IMEI ?? "",
                SecurityKey = req.SecurityKey ?? string.Empty
            };
            IProcedureAsync proc = new ProcPSATransactionService(_dal);
            var TSResp = (TransactionServiceResp)await proc.Call(TSReq).ConfigureAwait(false);

            #region ReadTransactionStatusAndUpdate
            res.BAL = TSResp.Balance;
            if (TSResp.Statuscode == ErrorCodes.Minus1)
            {
                res.STATUS = RechargeRespType.FAILED;
                res.MSG = TSResp.Msg;
                return res;
            }
            else if (TSResp.Statuscode == ErrorCodes.One)
            {
                res.RPID = TSResp.TransactionID;
                TSResp.AccountNo = TSReq.AccountNo;
                IPSATransaction pSATransaction = new OnboardingML(_accessor, _env);
                await pSATransaction.CallPSATransaction(TSResp, res).ConfigureAwait(false);

                #region UpdateTransactionSatus
                IProcedureAsync _updateProc = new ProcUpdateTransactionServiceStatus(_dal);
                var tstatus = new TransactionStatus
                {
                    UserID = TSReq.UserID,
                    TID = TSResp.TID,
                    APIID = TSResp.CurrentAPI.ID,
                    Status = res.STATUS,
                    APIOpCode = TSResp.CurrentAPI.APIOpCode,
                    APIName = TSResp.CurrentAPI.Name,
                    APICommType = TSResp.CurrentAPI.CommType,
                    APIComAmt = TSResp.CurrentAPI.Comm,
                    OperatorID = res.OPID ?? string.Empty,
                    VendorID = res.VendorID
                };
                await _updateProc.Call(tstatus).ConfigureAwait(false);
                #endregion
            }
            #endregion
            return res;
        }

        public async Task<GenerateURLResponse> GenerateAEPSURL(PartnerAPIRequest aPIRequest)
        {
            IProcedureAsync proc = new ProcGenerateAEPSURL(_dal);
            var procRes = (ResponseStatus)await proc.Call(new CommonReq
            {
                LoginID = aPIRequest.UserID,
                CommonStr = aPIRequest.Token,
                CommonInt = aPIRequest.PartnerID,
                CommonInt2 = aPIRequest.OutletID,
                CommonStr2 = _info.GetRemoteIP()
            }).ConfigureAwait(false);
            var res = new GenerateURLResponse
            {
                Statuscode = procRes.Statuscode,
                Msg = procRes.Msg,
                Errorcode = procRes.ErrorCode
            };
            if (res.Statuscode == ErrorCodes.One)
            {
                var URLResp = new AEPSURLSessionResp
                {
                    APIUserID = aPIRequest.UserID,
                    OutletID = aPIRequest.OutletID,
                    PartnerID = aPIRequest.PartnerID,
                    TransactionID = procRes.CommonStr,
                    URLID = procRes.CommonInt,
                    t = aPIRequest.t
                };
                res.URLSession = HttpUtility.UrlEncode(HashEncryption.O.Encrypt(JsonConvert.SerializeObject(URLResp)));
                res.RedirectURL = string.Format("{0}/AEPS/RP-Redirect?UrlSession={1}", procRes.CommonStr2 ?? string.Empty, res.URLSession ?? string.Empty);
            }
            return res;
        }

        public async Task<ResponseStatus> ValidateAEPSURL(AEPSURLSessionResp aEPSURLSessionResp)
        {
            IProcedureAsync proc = new ProcValidateAEPSURLRequest(_dal);
            return (ResponseStatus)await proc.Call(new CommonReq
            {
                LoginID = aEPSURLSessionResp.APIUserID,
                CommonStr = aEPSURLSessionResp.TransactionID,
                CommonInt = aEPSURLSessionResp.PartnerID,
                CommonInt2 = aEPSURLSessionResp.OutletID,
                CommonInt3 = aEPSURLSessionResp.URLID,
                CommonInt4 = aEPSURLSessionResp.t
            }).ConfigureAwait(false);
        }



        private string GenerateGeoCode(string GeoCode)
        {
            var a = GeoCode.Split(',')[0];
            var b = GeoCode.Contains(",") ? GeoCode.Split(',')[1] : string.Empty;
            a = Validate.O.IsNumeric(a.Replace(".", "")) ? string.Format("{0:00.0000}", Convert.ToDecimal(a)) : "25.5601";
            b = Validate.O.IsNumeric(b.Replace(".", "")) ? string.Format("{0:00.0000}", Convert.ToDecimal(b)) : "74.3401";
            return a + "," + b;
        }
    }
}
