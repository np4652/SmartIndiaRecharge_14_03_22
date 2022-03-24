using Fintech.AppCode.DB;
using Fintech.AppCode.HelperClass;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcTransactionService : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcTransactionService(IDAL dal) => _dal = dal;
        public async Task<object> Call(object obj)
        {
            var _req = (TransactionServiceReq)obj;
            var _res = new TransactionServiceResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError,
                OpParams = new List<OperatorParams>(),
                AInfos=new List<TranAdditionalInfo>()
            };
            SqlParameter[] param ={
                new SqlParameter("@OID", _req.OID),
                new SqlParameter("@CircleID", _req.CircleID == 0 ? -1 : _req.CircleID),
                new SqlParameter("@UserID", _req.UserID),
                new SqlParameter("@AccountNo", _req.AccountNo ?? ""),
                new SqlParameter("@AmountR", _req.AmountR),
                new SqlParameter("@OPID", _req.OPID?? ""),
                new SqlParameter("@APIRequestID", _req.APIRequestID ?? ""),
                new SqlParameter("@RequestModeID", _req.RequestModeID),
                new SqlParameter("@RequestIP", _req.RequestIP??""),
                new SqlParameter("@Optional1", _req.Optional1 ?? ""),
                new SqlParameter("@Optional2", _req.Optional2 ?? ""),
                new SqlParameter("@Optional3", _req.Optional3 ?? ""),
                new SqlParameter("@Optional4", _req.Optional4 ?? ""),
                new SqlParameter("@OutletID", _req.OutletID),
                new SqlParameter("@REFID", _req.RefID ?? ""),
                new SqlParameter("@GEOCode", _req.GEOCode ?? ""),
                new SqlParameter("@CustomerNumber", _req.CustomerNumber?? ""),
                new SqlParameter("@PinCode", _req.PinCode ?? ""),
                new SqlParameter("@IMEI", _req.IMEI ?? ""),
                new SqlParameter("@SecurityKey",HashEncryption.O.Encrypt(_req.SecurityKey??"")),
                new SqlParameter("@IsReal",_req.IsReal),
                new SqlParameter("@FetchBillID",_req.FetchBillID),
                new SqlParameter("@PaymentMode",_req.PaymentMode??string.Empty)
            };
            try
            {
                var ds = await _dal.GetByProcedureAdapterDSAsync(GetName(), param).ConfigureAwait(false);
                if (ds.Tables.Count > 0)
                {
                    var dt = ds.Tables[0];
                    _res.Statuscode = Convert.ToInt32(dt.Rows[0][0]);
                    _res.Msg = dt.Rows[0]["Msg"].ToString();
                    if (dt.Columns.Contains("_ErrorCode"))
                    {
                        _res.ErrorCode = dt.Rows[0]["_ErrorCode"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_ErrorCode"]);
                    }
                    if (_res.Statuscode == ErrorCodes.One)
                    {
                        _res.TID = Convert.ToInt32(dt.Rows[0]["TID"] is DBNull ? 0 : dt.Rows[0]["TID"]);
                        _res.WID = Convert.ToInt32(dt.Rows[0]["_WID"] is DBNull ? 0 : dt.Rows[0]["_WID"]);
                        _res.TransactionID = dt.Rows[0]["TransactionID"].ToString();
                        _res.Balance = dt.Rows[0]["Balance"] is DBNull ? 0.00M : Convert.ToDecimal(dt.Rows[0]["Balance"]);
                        _res.IsBBPS = dt.Rows[0]["IsBBPS"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["IsBBPS"]);
                        _res.OutletMobile = dt.Rows[0]["OutletMobile"] is DBNull ? "" : dt.Rows[0]["OutletMobile"].ToString();
                        _res.PAN = dt.Rows[0]["_PAN"] is DBNull ? "" : dt.Rows[0]["_PAN"].ToString();
                        _res.Aadhar = dt.Rows[0]["_AADHAR"] is DBNull ? "" : dt.Rows[0]["_AADHAR"].ToString();
                        
                        _res.IsAmountInto100 = Convert.ToBoolean(dt.Rows[0]["_IsAmountInto100"] is DBNull ? false : dt.Rows[0]["_IsAmountInto100"]);
                        _res.GeoCode = _req.GEOCode;
                        _res.Pincode = _req.PinCode;
                        _res.CustomerNumber = _req.CustomerNumber;
                        _res.RegxAccount = dt.Rows[0]["_RegxAccount"] is DBNull ? string.Empty : dt.Rows[0]["_RegxAccount"].ToString();
                        _res.EarlyPaymentAmountKey = dt.Rows[0]["_EarlyPaymentAmountKey"] is DBNull ? string.Empty : dt.Rows[0]["_EarlyPaymentAmountKey"].ToString();
                        _res.LatePaymentAmountKey = dt.Rows[0]["_LatePaymentAmountKey"] is DBNull ? string.Empty : dt.Rows[0]["_LatePaymentAmountKey"].ToString();
                        _res.EarlyPaymentDateKey = dt.Rows[0]["_EarlyPaymentDateKey"] is DBNull ? string.Empty : dt.Rows[0]["_EarlyPaymentDateKey"].ToString();
                        _res.BillFetchResponse = dt.Rows[0]["_BillFetchResponse"] is DBNull ? string.Empty : dt.Rows[0]["_BillFetchResponse"].ToString();
                        _res.BillerID = dt.Rows[0]["_BillerID"] is DBNull ? string.Empty : dt.Rows[0]["_BillerID"].ToString();
                        _res.InitChanel = dt.Rows[0]["_InitChanel"] is DBNull ? string.Empty : dt.Rows[0]["_InitChanel"].ToString();
                        _res.MAC= dt.Rows[0]["_MAC"] is DBNull ? string.Empty : dt.Rows[0]["_MAC"].ToString();
                        _res.PaymentMode = dt.Rows[0]["_PaymentMode"] is DBNull ? string.Empty : dt.Rows[0]["_PaymentMode"].ToString();
                        _res.BillDate = dt.Rows[0]["_BillDate"] is DBNull ? string.Empty : dt.Rows[0]["_BillDate"].ToString();
                        _res.EarlyPaymentDate = dt.Rows[0]["_EarlyPaymentDate"] is DBNull ? string.Empty : dt.Rows[0]["_EarlyPaymentDate"].ToString();
                        _res.DueDate = dt.Rows[0]["_DueDate"] is DBNull ? string.Empty : dt.Rows[0]["_DueDate"].ToString();
                        _res.PaymentModeInAPI = dt.Rows[0]["_PaymentModeInAPI"] is DBNull ? string.Empty : dt.Rows[0]["_PaymentModeInAPI"].ToString();
                        _res.CaptureInfo = dt.Rows[0]["_CaptureInfo"] is DBNull ? string.Empty : dt.Rows[0]["_CaptureInfo"].ToString();
                        _res.UserMobileNo = dt.Rows[0]["_UserMobileNo"] is DBNull ? string.Empty : dt.Rows[0]["_UserMobileNo"].ToString();
                        _res.UserName = dt.Rows[0]["_UserName"] is DBNull ? string.Empty : dt.Rows[0]["_UserName"].ToString();
                        _res.UserEmailID = dt.Rows[0]["_UserEmailID"] is DBNull ? string.Empty : dt.Rows[0]["_UserEmailID"].ToString();
                        _res.IsINT = dt.Rows[0]["_IsINT"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsINT"]);
                        _res.IsMOB = dt.Rows[0]["_IsMOB"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsMOB"]);
                        _res.BillerAdhoc = dt.Rows[0]["_BillerAdhoc"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_BillerAdhoc"]);
                        _res.ExactNess = dt.Rows[0]["_ExactNess"] is DBNull ? 0 : Convert.ToInt16(dt.Rows[0]["_ExactNess"]);
                        _res.IsBilling = dt.Rows[0]["_IsBilling"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsBilling"]);
                        _res.IsBillValidation = dt.Rows[0]["_IsBillValidation"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsBillValidation"]);
                        _res.AccountNoKey = dt.Rows[0]["_AccountNoKey"] is DBNull ? string.Empty : dt.Rows[0]["_AccountNoKey"].ToString();
                        _res.APIContext = dt.Rows[0]["_APIContext"] is DBNull ? string.Empty : dt.Rows[0]["_APIContext"].ToString();
                        _res.AccountHolder = dt.Rows[0]["_AccountHolder"] is DBNull ? string.Empty : dt.Rows[0]["_AccountHolder"].ToString();
                        _res.APIOpType = dt.Rows[0]["_APIOpType"] is DBNull ? string.Empty : dt.Rows[0]["_APIOpType"].ToString();
                        _res.REFID = dt.Rows[0]["_REFID"] is DBNull ? string.Empty : dt.Rows[0]["_REFID"].ToString();
                        _res.Operator = dt.Rows[0]["_Operator"] is DBNull ? string.Empty : dt.Rows[0]["_Operator"].ToString();
                        if (ds.Tables.Count > 1)
                        {
                            var dtCurrentAPI = ds.Tables[1];
                            if (dtCurrentAPI.Rows.Count > 0)
                            {
                                var CurrentAPI = new APIDetail
                                {
                                    ID = dtCurrentAPI.Rows[0]["_ID"] is DBNull ? 0 : Convert.ToInt32(dtCurrentAPI.Rows[0]["_ID"]),
                                    Name = dtCurrentAPI.Rows[0]["_Name"] is DBNull ? "" : dtCurrentAPI.Rows[0]["_Name"].ToString(),
                                    URL = dtCurrentAPI.Rows[0]["_URL"] is DBNull ? "" : dtCurrentAPI.Rows[0]["_URL"].ToString(),
                                    APIType = dtCurrentAPI.Rows[0]["_APIType"] is DBNull ? 0 : Convert.ToInt32(dtCurrentAPI.Rows[0]["_APIType"]),
                                    RequestMethod = dtCurrentAPI.Rows[0]["_RequestMethod"] is DBNull ? "" : dtCurrentAPI.Rows[0]["_RequestMethod"].ToString(),
                                    ResponseTypeID = dtCurrentAPI.Rows[0]["_ResponseTypeID"] is DBNull ? 0 : Convert.ToInt32(dtCurrentAPI.Rows[0]["_ResponseTypeID"]),
                                    LiveID = dtCurrentAPI.Rows[0]["_LiveID"] is DBNull ? "" : dtCurrentAPI.Rows[0]["_LiveID"].ToString(),
                                    VendorID = dtCurrentAPI.Rows[0]["_VendorID"] is DBNull ? "" : dtCurrentAPI.Rows[0]["_VendorID"].ToString(),
                                    IsOutletRequired = dtCurrentAPI.Rows[0]["_IsOutletRequired"] is DBNull ? false : Convert.ToBoolean(dtCurrentAPI.Rows[0]["_IsOutletRequired"]),
                                    FixedOutletID = dtCurrentAPI.Rows[0]["_FixedOutletID"] is DBNull ? "" : dtCurrentAPI.Rows[0]["_FixedOutletID"].ToString(),
                                    OnlineOutletID = dtCurrentAPI.Rows[0]["_OnlineOutletID"] is DBNull ? "" : dtCurrentAPI.Rows[0]["_OnlineOutletID"].ToString(),
                                    FailCode = dtCurrentAPI.Rows[0]["_FailCode"] is DBNull ? "" : dtCurrentAPI.Rows[0]["_FailCode"].ToString(),
                                    SuccessCode = dtCurrentAPI.Rows[0]["_SuccessCode"] is DBNull ? "" : dtCurrentAPI.Rows[0]["_SuccessCode"].ToString(),
                                    APIOpCode = dtCurrentAPI.Rows[0]["APIOpCode"] is DBNull ? "" : dtCurrentAPI.Rows[0]["APIOpCode"].ToString(),
                                    APIOpCodeCircle = dtCurrentAPI.Rows[0]["_APIOpCodeCircle"] is DBNull ? "" : dtCurrentAPI.Rows[0]["_APIOpCodeCircle"].ToString(),
                                    CommType = dtCurrentAPI.Rows[0]["_CommType"] is DBNull ? false : Convert.ToBoolean(dtCurrentAPI.Rows[0]["_CommType"]),
                                    Comm = dtCurrentAPI.Rows[0]["_Comm"] is DBNull ? 0.00M : Convert.ToDecimal(dtCurrentAPI.Rows[0]["_Comm"]),
                                    APIOutletID = dtCurrentAPI.Rows[0]["_APIOutletID"] is DBNull ? "" : dtCurrentAPI.Rows[0]["_APIOutletID"].ToString(),
                                    StatusName = dtCurrentAPI.Rows[0]["_StatusName"] is DBNull ? "" : dtCurrentAPI.Rows[0]["_StatusName"].ToString(),
                                    APICode = dtCurrentAPI.Rows[0]["_APICode"] is DBNull ? "" : dtCurrentAPI.Rows[0]["_APICode"].ToString(),
                                    MsgKey = dtCurrentAPI.Rows[0]["_MsgKey"] is DBNull ? "" : dtCurrentAPI.Rows[0]["_MsgKey"].ToString(),
                                    ErrorCodeKey = dtCurrentAPI.Rows[0]["_ErrorCodeKey"] is DBNull ? "" : dtCurrentAPI.Rows[0]["_ErrorCodeKey"].ToString(),
                                    GroupID = dtCurrentAPI.Rows[0]["_GroupID"] is DBNull ? 0 : Convert.ToInt32(dtCurrentAPI.Rows[0]["_GroupID"]),
                                    GroupCode = dtCurrentAPI.Rows[0]["_GroupCode"] is DBNull ? "" : dtCurrentAPI.Rows[0]["_GroupCode"].ToString(),
                                    RechType = dtCurrentAPI.Rows[0]["_RechType"] is DBNull ? "" : dtCurrentAPI.Rows[0]["_RechType"].ToString(),
                                    ValidateURL = dtCurrentAPI.Rows[0]["_ValidateURL"] is DBNull ? "" : dtCurrentAPI.Rows[0]["_ValidateURL"].ToString(),
                                    ContentType = dtCurrentAPI.Rows[0]["_ContentType"] is DBNull ? 0 : Convert.ToInt16(dtCurrentAPI.Rows[0]["_ContentType"]),
                                    IsAmountInto100 = Convert.ToBoolean(dtCurrentAPI.Rows[0]["_IsAmountInto100"] is DBNull ? false : dtCurrentAPI.Rows[0]["_IsAmountInto100"]),
                                    SwitchingID = Convert.ToInt32(dtCurrentAPI.Rows[0]["_SwitchingID"] is DBNull ? 0 : dtCurrentAPI.Rows[0]["_SwitchingID"]),
                                    ValidationStatusKey = dtCurrentAPI.Rows[0]["_ValidationStatusKey"] is DBNull ? string.Empty: dtCurrentAPI.Rows[0]["_ValidationStatusKey"].ToString(),
                                    ValidationStatusValue = dtCurrentAPI.Rows[0]["_ValidationStatusValue"] is DBNull ? string.Empty: dtCurrentAPI.Rows[0]["_ValidationStatusValue"].ToString(),
                                    RefferenceKey = dtCurrentAPI.Rows[0]["_RefKey"] is DBNull ? string.Empty: dtCurrentAPI.Rows[0]["_RefKey"].ToString(),
                                    FirstDelimiter = dtCurrentAPI.Rows[0]["_FirstDelemeter"] is DBNull ? string.Empty : dtCurrentAPI.Rows[0]["_FirstDelemeter"].ToString(),
                                    SecondDelimiter = dtCurrentAPI.Rows[0]["_SecondDelemeter"] is DBNull ? string.Empty : dtCurrentAPI.Rows[0]["_SecondDelemeter"].ToString(),
                                    HookFirstDelimiter = dtCurrentAPI.Rows[0]["_HookFirstDelimiter"] is DBNull ? string.Empty : dtCurrentAPI.Rows[0]["_HookFirstDelimiter"].ToString(),
                                    HookSecondDelimiter = dtCurrentAPI.Rows[0]["_HookSecondDelimiter"] is DBNull ? string.Empty : dtCurrentAPI.Rows[0]["_HookSecondDelimiter"].ToString(),
                                    HookErrorCodeKey = dtCurrentAPI.Rows[0]["_HookErrorCodeKey"] is DBNull ? string.Empty : dtCurrentAPI.Rows[0]["_HookErrorCodeKey"].ToString(),
                                };
                                if (!string.IsNullOrEmpty(CurrentAPI.APIOpCodeCircle))
                                {
                                    CurrentAPI.APIOpCode = CurrentAPI.APIOpCodeCircle;
                                }
                                _res.CurrentAPI = CurrentAPI;
                            }
                        }
                        if (ds.Tables.Count >= 3)
                        {
                            var dtMoreAPI = ds.Tables[2];
                            if (dtMoreAPI.Rows.Count > 0)
                            {
                                var MoreAPIs = new List<APIDetail>();
                                foreach (DataRow row in dtMoreAPI.Rows)
                                {
                                    var MoreAPI = new APIDetail
                                    {
                                        ID = row["_ID"] is DBNull ? 0 : Convert.ToInt32(row["_ID"]),
                                        Name = row["_Name"] is DBNull ? "" : row["_Name"].ToString(),
                                        URL = row["_URL"] is DBNull ? "" : row["_URL"].ToString(),
                                        APIType = row["_APIType"] is DBNull ? 0 : Convert.ToInt32(row["_APIType"]),
                                        RequestMethod = row["_RequestMethod"] is DBNull ? "" : row["_RequestMethod"].ToString(),
                                        ResponseTypeID = row["_ResponseTypeID"] is DBNull ? 0 : Convert.ToInt32(row["_ResponseTypeID"]),
                                        LiveID = row["_LiveID"] is DBNull ? "" : row["_LiveID"].ToString(),
                                        VendorID = row["_VendorID"] is DBNull ? "" : row["_VendorID"].ToString(),
                                        IsOutletRequired = row["_IsOutletRequired"] is DBNull ? false : Convert.ToBoolean(row["_IsOutletRequired"]),
                                        FixedOutletID = row["_FixedOutletID"] is DBNull ? "" : row["_FixedOutletID"].ToString(),
                                        OnlineOutletID = row["_OnlineOutletID"] is DBNull ? "" : row["_OnlineOutletID"].ToString(),
                                        FailCode = row["_FailCode"] is DBNull ? "" : row["_FailCode"].ToString(),
                                        SuccessCode = row["_SuccessCode"] is DBNull ? "" : row["_SuccessCode"].ToString(),
                                        APIOpCode = row["APIOpCode"] is DBNull ? "" : row["APIOpCode"].ToString(),
                                        APIOpCodeCircle = row["_APIOpCodeCircle"] is DBNull ? "" : row["_APIOpCodeCircle"].ToString(),
                                        CommType = row["_CommType"] is DBNull ? false : Convert.ToBoolean(row["_CommType"]),
                                        Comm = row["_Comm"] is DBNull ? 0.00M : Convert.ToDecimal(row["_Comm"]),
                                        APIOutletID = row["_APIOutletID"] is DBNull ? "" : row["_APIOutletID"].ToString(),
                                        StatusName = row["_StatusName"] is DBNull ? "" : row["_StatusName"].ToString(),
                                        APICode = row["_APICode"] is DBNull ? "" : row["_APICode"].ToString(),
                                        MsgKey = row["_MsgKey"] is DBNull ? "" : row["_MsgKey"].ToString(),
                                        ErrorCodeKey = row["_ErrorCodeKey"] is DBNull ? "" : row["_ErrorCodeKey"].ToString(),
                                        GroupID = row["_GroupID"] is DBNull ? 0 : Convert.ToInt32(row["_GroupID"]),
                                        GroupCode = row["_GroupCode"] is DBNull ? "" : row["_GroupCode"].ToString(),
                                        RechType = row["_RechType"] is DBNull ? "" : row["_RechType"].ToString(),
                                        ValidateURL = row["_ValidateURL"] is DBNull ? "" : row["_ValidateURL"].ToString(),
                                        ContentType = row["_ContentType"] is DBNull ? 0 : Convert.ToInt16(row["_ContentType"]),
                                        IsAmountInto100 = Convert.ToBoolean(row["_IsAmountInto100"] is DBNull ? false : row["_IsAmountInto100"]),
                                        SwitchingID = Convert.ToInt32(row["_SwitchingID"] is DBNull ? 0 : row["_SwitchingID"]),
                                        ValidationStatusKey = row["_ValidationStatusKey"] is DBNull ? string.Empty: row["_ValidationStatusKey"].ToString(),
                                        ValidationStatusValue = row["_ValidationStatusValue"] is DBNull ? string.Empty: row["_ValidationStatusValue"].ToString(),
                                        RefferenceKey = row["_RefKey"] is DBNull ? string.Empty: row["_RefKey"].ToString(),
                                        FirstDelimiter = row["_FirstDelemeter"] is DBNull ? string.Empty: row["_FirstDelemeter"].ToString(),
                                        SecondDelimiter = row["_SecondDelemeter"] is DBNull ? string.Empty: row["_SecondDelemeter"].ToString(),
                                        HookFirstDelimiter = row["_HookFirstDelimiter"] is DBNull ? string.Empty: row["_HookFirstDelimiter"].ToString(),
                                        HookSecondDelimiter = row["_HookSecondDelimiter"] is DBNull ? string.Empty: row["_HookSecondDelimiter"].ToString(),
                                        HookErrorCodeKey = row["_HookErrorCodeKey"] is DBNull ? string.Empty: row["_HookErrorCodeKey"].ToString(),
                                    };
                                    if (!string.IsNullOrEmpty(MoreAPI.APIOpCodeCircle))
                                    {
                                        MoreAPI.APIOpCode = MoreAPI.APIOpCodeCircle;
                                    }
                                    MoreAPIs.Add(MoreAPI);
                                }
                                _res.MoreAPIs = MoreAPIs;
                            }
                        }
                        if (ds.Tables.Count >= 4)
                        {
                            var dtOpParams = ds.Tables[3];
                            foreach (DataRow item in dtOpParams.Rows)
                            {
                                _res.OpParams.Add(new OperatorParams
                                {
                                    DataType = item["_DataType"] is DBNull ? string.Empty : item["_DataType"].ToString(),
                                    Ind = item["_Ind"] is DBNull ? 0 : Convert.ToInt32(item["_Ind"]),
                                    IsAccountNo = item["_IsAccountNo"] is DBNull ? false : Convert.ToBoolean(item["_IsAccountNo"]),
                                    IsOptional = item["_IsOptional"] is DBNull ? false : Convert.ToBoolean(item["_IsOptional"]),
                                    MaxLength = item["_MaxLength"] is DBNull ? 0 : Convert.ToInt32(item["_MaxLength"]),
                                    MinLength = item["_MinLength"] is DBNull ? 0 : Convert.ToInt32(item["_MinLength"]),
                                    Param = item["_ParamName"] is DBNull ? string.Empty : item["_ParamName"].ToString(),
                                    RegEx = item["_RegEx"] is DBNull ? string.Empty : item["_RegEx"].ToString()
                                });
                            }
                        }
                        if (ds.Tables.Count >= 5)
                        {
                            var dtAInfo = ds.Tables[4];
                            foreach (DataRow item in dtAInfo.Rows)
                            {
                                _res.AInfos.Add(new TranAdditionalInfo
                                {
                                    InfoName = item["_InfoName"] is DBNull ? string.Empty : item["_InfoName"].ToString(),
                                    InfoValue = item["_InfoValue"] is DBNull ? string.Empty : item["_InfoValue"].ToString(),
                                });
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
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = _req.UserID
                });
            }
            return _res;
        }
        public Task<object> Call() => throw new NotImplementedException();
        public string GetName() => "proc_TransactionServiceWithSwitch_BP";
    }
}
