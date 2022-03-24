using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGenerateBillFetchURL : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcGenerateBillFetchURL(IDAL dal) => _dal = dal;
        public async Task<object> Call(object obj)
        {
            var _req = (TransactionServiceReq)obj;
            var _res = new TransactionServiceResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError,
                OpParams = new System.Collections.Generic.List<OperatorParams>(),
                OpOptionalDic=new System.Collections.Generic.List<OperatorOptionalDictionary>()
            };
            SqlParameter[] param ={
                new SqlParameter("@UserID", _req.UserID),
                new SqlParameter("@OID", _req.OID == 0 ? -1 : _req.OID),
                new SqlParameter("@AccountNo", _req.AccountNo),
                new SqlParameter("@APIRequestID", _req.APIRequestID??string.Empty),
                new SqlParameter("@RequestModeID", _req.RequestModeID),
                new SqlParameter("@RequestIP", _req.RequestIP?? string.Empty),
                new SqlParameter("@OutletID", _req.OutletID),
                new SqlParameter("@browser", _req.Browser ?? string.Empty),
                new SqlParameter("@CircleID", _req.CircleID==0?-1:_req.CircleID),
                new SqlParameter("@CustomerNo",_req.CustomerNumber??string.Empty),
                new SqlParameter("@Optional1",_req.Optional1??string.Empty),
                new SqlParameter("@Optional2",_req.Optional2??string.Empty),
                new SqlParameter("@Optional3",_req.Optional3??string.Empty),
                new SqlParameter("@Optional4",_req.Optional4??string.Empty),
                new SqlParameter("@GEOCode",_req.GEOCode??string.Empty)
            };
            try
            {
                var ds = await _dal.GetByProcedureAdapterDSAsync(GetName(), param).ConfigureAwait(false);
                if (ds.Tables.Count > 0)
                {
                    var dt = ds.Tables[0];
                    _res.Statuscode = Convert.ToInt32(dt.Rows[0][0]);
                    _res.Msg = dt.Rows[0]["Msg"].ToString();
                    if (_res.Statuscode == ErrorCodes.One)
                    {
                        _res.TID = Convert.ToInt32(dt.Rows[0]["TID"] is DBNull ? 0 : dt.Rows[0]["TID"]);
                        _res.TransactionID = dt.Rows[0]["TransactionID"].ToString();
                        _res.Balance = dt.Rows[0]["Balance"] is DBNull ? 0.00M : Convert.ToDecimal(dt.Rows[0]["Balance"]);
                        _res.IsBBPS = dt.Rows[0]["IsBBPS"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["IsBBPS"]);
                        _res.OutletMobile = dt.Rows[0]["OutletMobile"] is DBNull ? string.Empty : dt.Rows[0]["OutletMobile"].ToString();
                        _res.GeoCode = _req.GEOCode;
                        _res.Pincode = dt.Rows[0]["_Pincode"] is DBNull ? string.Empty : dt.Rows[0]["_Pincode"].ToString();
                        _res.IsPartialPay = dt.Rows[0]["IsPartial"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["IsPartial"]);
                        _res.CircleCode = dt.Rows[0]["_RechType"] is DBNull ? string.Empty : dt.Rows[0]["_RechType"].ToString();
                        _res.CustomerName = dt.Rows[0]["_OutletName"] is DBNull ? string.Empty : dt.Rows[0]["_OutletName"].ToString();
                        _res.PAN = dt.Rows[0]["_PAN"] is DBNull ? string.Empty : dt.Rows[0]["_PAN"].ToString();
                        _res.Aadhar = dt.Rows[0]["_Aadhar"] is DBNull ? string.Empty : dt.Rows[0]["_Aadhar"].ToString();
                        _res.BillerID = dt.Rows[0]["_BillerID"] is DBNull ? string.Empty : dt.Rows[0]["_BillerID"].ToString();
                        _res.InitChanel = dt.Rows[0]["_InitChanel"] is DBNull ? string.Empty : dt.Rows[0]["_InitChanel"].ToString();
                        _res.MAC = dt.Rows[0]["_MAC"] is DBNull ? string.Empty : dt.Rows[0]["_MAC"].ToString();
                        _res.AccountNoKey = dt.Rows[0]["_AccountNoKey"] is DBNull ? string.Empty : dt.Rows[0]["_AccountNoKey"].ToString();
                        _res.RegxAccount = dt.Rows[0]["_RegxAccount"] is DBNull ? string.Empty : dt.Rows[0]["_RegxAccount"].ToString();
                        _res.EarlyPaymentAmountKey = dt.Rows[0]["_EarlyPaymentAmountKey"] is DBNull ? string.Empty : dt.Rows[0]["_EarlyPaymentAmountKey"].ToString();
                        _res.LatePaymentAmountKey = dt.Rows[0]["_LatePaymentAmountKey"] is DBNull ? string.Empty : dt.Rows[0]["_LatePaymentAmountKey"].ToString();
                        _res.EarlyPaymentDateKey = dt.Rows[0]["_EarlyPaymentDateKey"] is DBNull ? string.Empty : dt.Rows[0]["_EarlyPaymentDateKey"].ToString();
                        _res.BillMonthKey = dt.Rows[0]["_BillMonthKey"] is DBNull ? string.Empty : dt.Rows[0]["_BillMonthKey"].ToString();
                        _res.BillerPaymentModes = dt.Rows[0]["_BillerPaymentModes"] is DBNull ? string.Empty : dt.Rows[0]["_BillerPaymentModes"].ToString();
                        _res.ExactNess = dt.Rows[0]["_ExactNess"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_ExactNess"]);
                        _res.APIOpType = dt.Rows[0]["_APIOpType"] is DBNull ? string.Empty : dt.Rows[0]["_APIOpType"].ToString();
                        if (ds.Tables.Count > 1)
                        {
                            var dtCurrentAPI = ds.Tables[1];
                            if (dtCurrentAPI.Rows.Count > 0)
                            {
                                _res.CurrentAPI = new APIDetail
                                {
                                    ID = dtCurrentAPI.Rows[0]["_ID"] is DBNull ? 0 : Convert.ToInt32(dtCurrentAPI.Rows[0]["_ID"]),
                                    OnlineOutletID = dtCurrentAPI.Rows[0]["_OnlineOutletID"] is DBNull ? string.Empty : dtCurrentAPI.Rows[0]["_OnlineOutletID"].ToString(),
                                    Name = dtCurrentAPI.Rows[0]["_Name"] is DBNull ? string.Empty : dtCurrentAPI.Rows[0]["_Name"].ToString(),
                                    URL = dtCurrentAPI.Rows[0]["_URL"] is DBNull ? string.Empty : dtCurrentAPI.Rows[0]["_URL"].ToString(),
                                    APIType = dtCurrentAPI.Rows[0]["_APIType"] is DBNull ? 0 : Convert.ToInt32(dtCurrentAPI.Rows[0]["_APIType"]),
                                    RequestMethod = dtCurrentAPI.Rows[0]["_RequestMethod"] is DBNull ? string.Empty : dtCurrentAPI.Rows[0]["_RequestMethod"].ToString(),
                                    ResponseTypeID = dtCurrentAPI.Rows[0]["_ResponseTypeID"] is DBNull ? 0 : Convert.ToInt32(dtCurrentAPI.Rows[0]["_ResponseTypeID"]),
                                    StatusName = dtCurrentAPI.Rows[0]["_StatusName"] is DBNull ? string.Empty : dtCurrentAPI.Rows[0]["_StatusName"].ToString(),
                                    LiveID = dtCurrentAPI.Rows[0]["_LiveID"] is DBNull ? string.Empty : dtCurrentAPI.Rows[0]["_LiveID"].ToString(),
                                    VendorID = dtCurrentAPI.Rows[0]["_VendorID"] is DBNull ? string.Empty : dtCurrentAPI.Rows[0]["_VendorID"].ToString(),
                                    IsOutletRequired = dtCurrentAPI.Rows[0]["_IsOutletRequired"] is DBNull ? false : Convert.ToBoolean(dtCurrentAPI.Rows[0]["_IsOutletRequired"]),
                                    FixedOutletID = dtCurrentAPI.Rows[0]["_FixedOutletID"] is DBNull ? string.Empty : dtCurrentAPI.Rows[0]["_FixedOutletID"].ToString(),
                                    FailCode = dtCurrentAPI.Rows[0]["_FailCode"] is DBNull ? string.Empty : dtCurrentAPI.Rows[0]["_FailCode"].ToString(),
                                    SuccessCode = dtCurrentAPI.Rows[0]["_SuccessCode"] is DBNull ? string.Empty : dtCurrentAPI.Rows[0]["_SuccessCode"].ToString(),
                                    APIOpCode = dtCurrentAPI.Rows[0]["APIOpCode"] is DBNull ? string.Empty : dtCurrentAPI.Rows[0]["APIOpCode"].ToString(),
                                    APICode = dtCurrentAPI.Rows[0]["APICode"] is DBNull ? string.Empty : dtCurrentAPI.Rows[0]["APICode"].ToString(),
                                    Comm = dtCurrentAPI.Rows[0]["_Comm"] is DBNull ? 0.00M : Convert.ToDecimal(dtCurrentAPI.Rows[0]["_Comm"]),
                                    APIOutletID = dtCurrentAPI.Rows[0]["_APIOutletID"] is DBNull ? string.Empty : dtCurrentAPI.Rows[0]["_APIOutletID"].ToString(),
                                    MsgKey = dtCurrentAPI.Rows[0]["_MsgKey"] is DBNull ? string.Empty : dtCurrentAPI.Rows[0]["_MsgKey"].ToString(),
                                    BillNoKey = dtCurrentAPI.Rows[0]["_BillNoKey"] is DBNull ? string.Empty : dtCurrentAPI.Rows[0]["_BillNoKey"].ToString(),
                                    BillDateKey = dtCurrentAPI.Rows[0]["_BillDateKey"] is DBNull ? string.Empty : dtCurrentAPI.Rows[0]["_BillDateKey"].ToString(),
                                    DueDateKey = dtCurrentAPI.Rows[0]["_DueDateKey"] is DBNull ? string.Empty : dtCurrentAPI.Rows[0]["_DueDateKey"].ToString(),
                                    BillAmountKey = dtCurrentAPI.Rows[0]["_BillAmountKey"] is DBNull ? string.Empty : dtCurrentAPI.Rows[0]["_BillAmountKey"].ToString(),
                                    CustomerNameKey = dtCurrentAPI.Rows[0]["_CustomerNameKey"] is DBNull ? string.Empty : dtCurrentAPI.Rows[0]["_CustomerNameKey"].ToString(),
                                    ErrorCodeKey = dtCurrentAPI.Rows[0]["_ErrorCodeKey"] is DBNull ? string.Empty : dtCurrentAPI.Rows[0]["_ErrorCodeKey"].ToString(),
                                    GroupID = dtCurrentAPI.Rows[0]["_GroupID"] is DBNull ? 0 : Convert.ToInt32(dtCurrentAPI.Rows[0]["_GroupID"]),
                                    GroupCode = dtCurrentAPI.Rows[0]["_GroupCode"] is DBNull ? string.Empty : dtCurrentAPI.Rows[0]["_GroupCode"].ToString(),
                                    BillStatusKey = dtCurrentAPI.Rows[0]["_BillStatusKey"] is DBNull ? string.Empty : dtCurrentAPI.Rows[0]["_BillStatusKey"].ToString(),
                                    BillStatusValue = dtCurrentAPI.Rows[0]["_BillStatusValue"] is DBNull ? string.Empty : dtCurrentAPI.Rows[0]["_BillStatusValue"].ToString(),
                                    BillReqMethod = dtCurrentAPI.Rows[0]["_BillReqMethod"] is DBNull ? string.Empty : dtCurrentAPI.Rows[0]["_BillReqMethod"].ToString(),
                                    BillResTypeID = dtCurrentAPI.Rows[0]["_BillResTypeID"] is DBNull ? 0 : Convert.ToInt16(dtCurrentAPI.Rows[0]["_BillResTypeID"]),
                                    ContentType = dtCurrentAPI.Rows[0]["_ContentType"] is DBNull ? 0 : Convert.ToInt16(dtCurrentAPI.Rows[0]["_ContentType"]),
                                    RefferenceKey = dtCurrentAPI.Rows[0]["_RefKey"] is DBNull ? string.Empty : dtCurrentAPI.Rows[0]["_RefKey"].ToString(),
                                    AdditionalInfoListKey = dtCurrentAPI.Rows[0]["_AdditionalInfoListKey"] is DBNull ? string.Empty : dtCurrentAPI.Rows[0]["_AdditionalInfoListKey"].ToString(),
                                    AdditionalInfoKey = dtCurrentAPI.Rows[0]["_AdditionalInfoKey"] is DBNull ? string.Empty : dtCurrentAPI.Rows[0]["_AdditionalInfoKey"].ToString(),
                                    AdditionalInfoValue = dtCurrentAPI.Rows[0]["_AdditionalInfoValue"] is DBNull ? string.Empty : dtCurrentAPI.Rows[0]["_AdditionalInfoValue"].ToString()
                                };
                            }
                            var dtOpParams = ds.Tables.Count >= 3 ? ds.Tables[2] : null;
                            if (dtOpParams != null)
                            {
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
                                        RegEx = item["_RegEx"] is DBNull ? string.Empty : item["_RegEx"].ToString(),
                                        IsCustomerNo = item["_IsCustomerNo"] is DBNull ? false : Convert.ToBoolean(item["_IsCustomerNo"]),
                                        IsDropDown = item["_IsDropDown"] is DBNull ? false : Convert.ToBoolean(item["_IsDropDown"]),
                                        OptionalID = item["_ID"] is DBNull ? 0 : Convert.ToInt32(item["_ID"])
                                    });
                                }
                            }
                            var dtOpParamsdtOpDic = ds.Tables.Count == 4 ? ds.Tables[3] : null;
                            if (dtOpParamsdtOpDic.Rows.Count > 0)
                            {
                                foreach (DataRow item in dtOpParamsdtOpDic.Rows)
                                {
                                    _res.OpOptionalDic.Add(new OperatorOptionalDictionary
                                    {
                                        OptionalID = item["_OptionalID"] is DBNull ? 0 : Convert.ToInt32(item["_OptionalID"]),
                                        Value = item["_Value"] is DBNull ? string.Empty : item["_Value"].ToString()
                                    });
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
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = _req.UserID
                });
            }
            return _res;
        }
        public Task<object> Call() => throw new NotImplementedException();
        public string GetName() => "proc_GenerateBillFetchURL";
    }
}
