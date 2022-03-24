using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using System.Threading.Tasks;


namespace RoundpayFinTech.AppCode.DL
{
    public class procCouponMasterList : IProcedure
    {
        private readonly IDAL _dal;
        public procCouponMasterList(IDAL dal) => _dal = dal;
        public string GetName() => "proc_GetCouponMasterList";
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@LT",req.LoginTypeID),
                new SqlParameter("@OID",req.CommonInt),

            };
            var res = new List<CoupanMaster>();
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {

                    foreach (DataRow row in dt.Rows)
                    {
                        var CoupanMaster = new CoupanMaster
                        {
                            ID = row["_ID"] is DBNull ? 0 : Convert.ToInt32(row["_ID"]),
                            VoucherType = row["_VoucherType"] is DBNull ? "" : row["_VoucherType"].ToString(),
                            OID = row["_OID"] is DBNull ? 0 : Convert.ToInt32(row["_OID"]),
                            OpName = row["_Name"] is DBNull ? "" : row["_Name"].ToString(),
                            Remark = row["_Remark"] is DBNull ? "" : row["_Remark"].ToString(),
                            LastModifyDate = Convert.ToDateTime(row["_ModifyDate"]).ToString("dd-MMM-yyyy hh:mm:ss tt"),
                            Max = row["_Max"] is DBNull ? 0 : Convert.ToInt32(row["_Max"]),
                            Min = row["_Min"] is DBNull ? 0 : Convert.ToInt32(row["_Min"]),
                            IsActive = row["_IsActive"] is DBNull ? true : Convert.ToBoolean(row["_IsActive"]),
                            DenominationID = row["_Amount"] is DBNull ? 0 : Convert.ToInt32(row["_Amount"])
                        };
                        res.Add(CoupanMaster);
                    }

                }
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = req.LoginTypeID,
                    UserId = req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            if (req.CommonInt == -1)
                return res;
            else
                return res;
        }

        public object Call() => throw new NotImplementedException();

    }


    public class proc_VoucherCouponService : IProcedure
    {
        private readonly IDAL _dal;
        public proc_VoucherCouponService(IDAL dal) => _dal = dal;
        public string GetName() => "proc_VoucherCouponService";
        public object Call(object obj)
        {
            var req = (CouponData)obj;
            var _res = new TransactionServiceResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError,
                OpParams = new List<OperatorParams>(),
                AInfos = new List<TranAdditionalInfo>()
            };
            SqlParameter[] param = {
                new SqlParameter("@UserID",req.LoginID),
                new SqlParameter("@Qty",req.Quantity),
                new SqlParameter("@Amount",req.Amount),
                new SqlParameter("@OID",req.OID),
                new SqlParameter("@ToEmail",req.To),
                new SqlParameter("@CustomerName",req.UserID),
                new SqlParameter("@Message",req.Message),
                new SqlParameter("@IP",req.RequestIP),
                new SqlParameter("@Browser",req.Browser),
                new SqlParameter("@RequestMode",req.RequestMode)
            };
            try
            {
                var ds = _dal.GetByProcedureAdapterDSAsync(GetName(), param).Result;
                if (ds.Tables.Count > 0)
                {
                    var dt = ds.Tables[0];
                    if (dt.Rows.Count > 0)
                    {
                        _res.Statuscode = Convert.ToInt32(dt.Rows[0][0]);
                        _res.Msg = dt.Rows[0]["Msg"] is DBNull ? "" : dt.Rows[0]["Msg"].ToString();
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
                            _res.UserMobileNo = dt.Rows[0]["_UserMobileNo"] is DBNull ? string.Empty : dt.Rows[0]["_UserMobileNo"].ToString();
                            _res.UserName = dt.Rows[0]["_UserName"] is DBNull ? string.Empty : dt.Rows[0]["_UserName"].ToString();
                            _res.UserEmailID = dt.Rows[0]["_UserEmailID"] is DBNull ? string.Empty : dt.Rows[0]["_UserEmailID"].ToString();
                            _res.APIOpType = dt.Rows[0]["_APIOpType"] is DBNull ? string.Empty : dt.Rows[0]["_APIOpType"].ToString();
                            _res.REFID = dt.Rows[0]["_REFID"] is DBNull ? string.Empty : dt.Rows[0]["_REFID"].ToString();
                            _res.Operator = dt.Rows[0]["_Operator"] is DBNull ? string.Empty : dt.Rows[0]["_Operator"].ToString();
                            _res.LiveID = dt.Rows[0]["_LiveID"] is DBNull ? string.Empty : dt.Rows[0]["_LiveID"].ToString();
                            _res.IsManual = dt.Rows[0]["_IsManual"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsManual"]);
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
                                        ValidationStatusKey = dtCurrentAPI.Rows[0]["_ValidationStatusKey"] is DBNull ? string.Empty : dtCurrentAPI.Rows[0]["_ValidationStatusKey"].ToString(),
                                        ValidationStatusValue = dtCurrentAPI.Rows[0]["_ValidationStatusValue"] is DBNull ? string.Empty : dtCurrentAPI.Rows[0]["_ValidationStatusValue"].ToString(),
                                        RefferenceKey = dtCurrentAPI.Rows[0]["_RefKey"] is DBNull ? string.Empty : dtCurrentAPI.Rows[0]["_RefKey"].ToString(),
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
                                            ValidationStatusKey = row["_ValidationStatusKey"] is DBNull ? string.Empty : row["_ValidationStatusKey"].ToString(),
                                            ValidationStatusValue = row["_ValidationStatusValue"] is DBNull ? string.Empty : row["_ValidationStatusValue"].ToString(),
                                            RefferenceKey = row["_RefKey"] is DBNull ? string.Empty : row["_RefKey"].ToString(),
                                            FirstDelimiter = row["_FirstDelemeter"] is DBNull ? string.Empty : row["_FirstDelemeter"].ToString(),
                                            SecondDelimiter = row["_SecondDelemeter"] is DBNull ? string.Empty : row["_SecondDelemeter"].ToString(),
                                            HookFirstDelimiter = row["_HookFirstDelimiter"] is DBNull ? string.Empty : row["_HookFirstDelimiter"].ToString(),
                                            HookSecondDelimiter = row["_HookSecondDelimiter"] is DBNull ? string.Empty : row["_HookSecondDelimiter"].ToString(),
                                            HookErrorCodeKey = row["_HookErrorCodeKey"] is DBNull ? string.Empty : row["_HookErrorCodeKey"].ToString(),
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
                    LoginTypeID = req.LoginTypeID,
                    UserId = req.LoginID
                });
            }
            return _res;
        }

        public object Call() => throw new NotImplementedException();



    }

}
