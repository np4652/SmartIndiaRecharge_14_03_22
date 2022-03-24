using Fintech.AppCode.DB;
using Fintech.AppCode.HelperClass;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcPSATransactionService : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcPSATransactionService(IDAL dal) => _dal = dal;
        public async Task<object> Call(object obj)
        {
            var _req = (TransactionServiceReq)obj;
            var _res = new TransactionServiceResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            SqlParameter[] param ={
                new SqlParameter("@OID", _req.OID),
                new SqlParameter("@UserID", _req.UserID),
                new SqlParameter("@AccountNo", _req.AccountNo ?? string.Empty),
                new SqlParameter("@AmountR", Convert.ToInt32(_req.AmountR)),
                new SqlParameter("@OPID", _req.OPID?? string.Empty),
                new SqlParameter("@APIRequestID", _req.APIRequestID ?? string.Empty),
                new SqlParameter("@RequestModeID", _req.RequestModeID),
                new SqlParameter("@RequestIP", _req.RequestIP??""),
                new SqlParameter("@Optional1", _req.Optional1 ?? string.Empty),
                new SqlParameter("@Optional2", _req.Optional2 ?? string.Empty),
                new SqlParameter("@Optional3", _req.Optional3 ?? string.Empty),
                new SqlParameter("@Optional4", _req.Optional4 ?? string.Empty),
                new SqlParameter("@OutletID", _req.OutletID),
                new SqlParameter("@REFID", _req.RefID ?? string.Empty),
                new SqlParameter("@GEOCode", _req.GEOCode ?? string.Empty),
                new SqlParameter("@CustomerNumber", _req.CustomerNumber?? string.Empty),
                new SqlParameter("@PinCode", _req.PinCode ?? string.Empty),
                new SqlParameter("@IMEI", _req.IMEI ?? string.Empty),
                new SqlParameter("@SecurityKey",HashEncryption.O.Encrypt(_req.SecurityKey??string.Empty))
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
                        _res.OutletMobile = dt.Rows[0]["OutletMobile"] is DBNull ? string.Empty : dt.Rows[0]["OutletMobile"].ToString();
                        _res.GeoCode = _req.GEOCode;
                        _res.Pincode = _req.PinCode;
                        _res.TotalToken = dt.Rows[0]["_TotalToken"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_TotalToken"]);
                        _res.OPID = dt.Rows[0]["_OPID"] is DBNull ? string.Empty : dt.Rows[0]["_OPID"].ToString();
                        
                        if (ds.Tables.Count > 1)
                        {
                            var dtCurrentAPI = ds.Tables[1];
                            if (dtCurrentAPI.Rows.Count > 0)
                            {
                                var CurrentAPI = new APIDetail
                                {
                                    ID = dtCurrentAPI.Rows[0]["_ID"] is DBNull ? 0 : Convert.ToInt32(dtCurrentAPI.Rows[0]["_ID"]),
                                    Name = dtCurrentAPI.Rows[0]["_Name"] is DBNull ? string.Empty : dtCurrentAPI.Rows[0]["_Name"].ToString(),
                                    URL = dtCurrentAPI.Rows[0]["_URL"] is DBNull ? string.Empty : dtCurrentAPI.Rows[0]["_URL"].ToString(),
                                    APIType = dtCurrentAPI.Rows[0]["_APIType"] is DBNull ? 0 : Convert.ToInt32(dtCurrentAPI.Rows[0]["_APIType"]),
                                    RequestMethod = dtCurrentAPI.Rows[0]["_RequestMethod"] is DBNull ? string.Empty : dtCurrentAPI.Rows[0]["_RequestMethod"].ToString(),
                                    ResponseTypeID = dtCurrentAPI.Rows[0]["_ResponseTypeID"] is DBNull ? 0 : Convert.ToInt32(dtCurrentAPI.Rows[0]["_ResponseTypeID"]),
                                    LiveID = dtCurrentAPI.Rows[0]["_LiveID"] is DBNull ? string.Empty : dtCurrentAPI.Rows[0]["_LiveID"].ToString(),
                                    VendorID = dtCurrentAPI.Rows[0]["_VendorID"] is DBNull ? string.Empty : dtCurrentAPI.Rows[0]["_VendorID"].ToString(),
                                    IsOutletRequired = dtCurrentAPI.Rows[0]["_IsOutletRequired"] is DBNull ? false : Convert.ToBoolean(dtCurrentAPI.Rows[0]["_IsOutletRequired"]),
                                    FixedOutletID = dtCurrentAPI.Rows[0]["_FixedOutletID"] is DBNull ? string.Empty : dtCurrentAPI.Rows[0]["_FixedOutletID"].ToString(),
                                    FailCode = dtCurrentAPI.Rows[0]["_FailCode"] is DBNull ? string.Empty : dtCurrentAPI.Rows[0]["_FailCode"].ToString(),
                                    SuccessCode = dtCurrentAPI.Rows[0]["_SuccessCode"] is DBNull ? string.Empty : dtCurrentAPI.Rows[0]["_SuccessCode"].ToString(),
                                    APIOpCode = dtCurrentAPI.Rows[0]["APIOpCode"] is DBNull ? string.Empty : dtCurrentAPI.Rows[0]["APIOpCode"].ToString(),
                                    CommType = dtCurrentAPI.Rows[0]["_CommType"] is DBNull ? false : Convert.ToBoolean(dtCurrentAPI.Rows[0]["_CommType"]),
                                    Comm = dtCurrentAPI.Rows[0]["_Comm"] is DBNull ? 0.00M : Convert.ToDecimal(dtCurrentAPI.Rows[0]["_Comm"]),
                                    APIOutletID = dtCurrentAPI.Rows[0]["_APIOutletID"] is DBNull ? string.Empty : dtCurrentAPI.Rows[0]["_APIOutletID"].ToString(),
                                    StatusName = dtCurrentAPI.Rows[0]["_StatusName"] is DBNull ? string.Empty : dtCurrentAPI.Rows[0]["_StatusName"].ToString(),
                                    APICode = dtCurrentAPI.Rows[0]["_APICode"] is DBNull ? string.Empty : dtCurrentAPI.Rows[0]["_APICode"].ToString(),
                                    MsgKey = dtCurrentAPI.Rows[0]["_MsgKey"] is DBNull ? string.Empty : dtCurrentAPI.Rows[0]["_MsgKey"].ToString(),
                                    ErrorCodeKey = dtCurrentAPI.Rows[0]["_ErrorCodeKey"] is DBNull ? string.Empty : dtCurrentAPI.Rows[0]["_ErrorCodeKey"].ToString(),
                                    GroupID = dtCurrentAPI.Rows[0]["_GroupID"] is DBNull ? 0 : Convert.ToInt32(dtCurrentAPI.Rows[0]["_GroupID"]),
                                    GroupCode = dtCurrentAPI.Rows[0]["_GroupCode"] is DBNull ? string.Empty : dtCurrentAPI.Rows[0]["_GroupCode"].ToString(),
                                    PANID = dtCurrentAPI.Rows[0]["_PANID"] is DBNull ? string.Empty : dtCurrentAPI.Rows[0]["_PANID"].ToString()
                                };
                                _res.CurrentAPI = CurrentAPI;
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
        public string GetName() => "proc_PSATransactionService";
    }
}
