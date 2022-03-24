using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetStatusCheckURL : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcGetStatusCheckURL(IDAL dal) => _dal = dal;
        public async Task<object> Call(object obj)
        {
            var _req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT", _req.LoginTypeID),
                new SqlParameter("@LoginID", _req.LoginID),
                new SqlParameter("@TID", _req.CommonInt),
                new SqlParameter("@IP", _req.CommonStr ?? string.Empty),
                new SqlParameter("@Browser", _req.CommonStr2 ?? string.Empty)
            };            
            var _res = new GETStatusCheckURL
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError,
                TID=_req.CommonInt
            };
            try
            {
                var dt = await _dal.GetByProcedureAsync(GetName(), param).ConfigureAwait(false);
                if (dt.Rows.Count > 0)
                {
                    _res.Statuscode = Convert.ToInt32(dt.Rows[0][0]);
                    _res.Msg = dt.Rows[0]["Msg"].ToString();
                    if (_res.Statuscode == ErrorCodes.One) {
                        _res.TransactionID = dt.Rows[0]["TranscationID"] is DBNull ? string.Empty : dt.Rows[0]["TranscationID"].ToString();
                        _res.Operator = dt.Rows[0]["_Operator"] is DBNull ? string.Empty : dt.Rows[0]["_Operator"].ToString();
                        _res.CustomerNumber = dt.Rows[0]["_CustomerNumber"] is DBNull ? string.Empty : dt.Rows[0]["_CustomerNumber"].ToString();
                        _res.VendorID = dt.Rows[0]["VendorID"] is DBNull ? string.Empty : dt.Rows[0]["VendorID"].ToString();
                        _res.GroupIID = dt.Rows[0]["_GroupIID"] is DBNull ? string.Empty : dt.Rows[0]["_GroupIID"].ToString();
                        _res._Type = Convert.ToInt32(dt.Rows[0]["_Type"]);
                        _res.IsBBPS = dt.Rows[0]["_IsBBPS"] is DBNull?false:Convert.ToBoolean(dt.Rows[0]["_IsBBPS"]);
                        _res.UserID = dt.Rows[0]["_UserID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_UserID"]);
                        _res.WID = dt.Rows[0]["_WID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_WID"]);
                        _res.RequestMode = dt.Rows[0]["_RequestMode"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_RequestMode"]);
                        _res.AmountR = dt.Rows[0]["_AmountR"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[0]["_AmountR"]);
                        _res.Optional2 = dt.Rows[0]["_Optional2"] is DBNull ? string.Empty : dt.Rows[0]["_Optional2"].ToString();
                        _res.Account = dt.Rows[0]["_Account"] is DBNull ? string.Empty : dt.Rows[0]["_Account"].ToString();
                        _res.SenderName = dt.Rows[0]["_SenderName"] is DBNull ? string.Empty : dt.Rows[0]["_SenderName"].ToString();
                        _res.Optional3 = dt.Rows[0]["_Optional3"] is DBNull ? string.Empty : dt.Rows[0]["_Optional3"].ToString();
                        _res.Optional4 = dt.Rows[0]["_Optional4"] is DBNull ? string.Empty : dt.Rows[0]["_Optional4"].ToString();
                        _res.APIContext = dt.Rows[0]["_APIContext"] is DBNull ? string.Empty : dt.Rows[0]["_APIContext"].ToString();
                        _res.RechargeAPI = new RechargeAPIHit
                        {
                            aPIDetail = new APIDetail {
                                APIType = dt.Rows[0]["_APIType"] is DBNull ? 0 : Convert.ToInt16(dt.Rows[0]["_APIType"]),
                                URL = dt.Rows[0]["RequestURL"] is DBNull ? string.Empty : dt.Rows[0]["RequestURL"].ToString(),
                                StatusName = dt.Rows[0]["_StatusName"] is DBNull ? string.Empty : dt.Rows[0]["_StatusName"].ToString(),                               
                                SuccessCode = dt.Rows[0]["_SuccessCode"] is DBNull ? string.Empty : dt.Rows[0]["_SuccessCode"].ToString(),
                                FailCode = dt.Rows[0]["_FailCode"] is DBNull ? string.Empty : dt.Rows[0]["_FailCode"].ToString(),
                                LiveID = dt.Rows[0]["_LiveID"] is DBNull ? string.Empty : dt.Rows[0]["_LiveID"].ToString(),
                                VendorID = dt.Rows[0]["_VendorID"] is DBNull ? string.Empty : dt.Rows[0]["_VendorID"].ToString(),
                                RequestMethod = dt.Rows[0]["_RequestMethod"] is DBNull ? string.Empty : dt.Rows[0]["_RequestMethod"].ToString(),
                                ID = dt.Rows[0]["_APIID"] is DBNull ? 0 : Convert.ToInt16(dt.Rows[0]["_APIID"]),
                                ResponseTypeID = dt.Rows[0]["_ResponseTypeID"] is DBNull ? 0 : Convert.ToInt16(dt.Rows[0]["_ResponseTypeID"]),
                                APICode=dt.Rows[0]["APICode"] is DBNull?"": dt.Rows[0]["APICode"].ToString(),
                                MsgKey = dt.Rows[0]["_MsgKey"] is DBNull ? string.Empty : dt.Rows[0]["_MsgKey"].ToString(),
                                ErrorCodeKey = dt.Rows[0]["_ErrorCodeKey"] is DBNull ? string.Empty : dt.Rows[0]["_ErrorCodeKey"].ToString(),
                                GroupID = dt.Rows[0]["_GroupID"] is DBNull ? 0 : Convert.ToInt16(dt.Rows[0]["_GroupID"]),
                                GroupCode = dt.Rows[0]["_GroupCode"] is DBNull ? string.Empty : dt.Rows[0]["_GroupCode"].ToString(),
                                DFormatID = dt.Rows[0]["_DFormatID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_DFormatID"]),
                                IsStatusBulkCheck = dt.Rows[0]["_IsStatusBulkCheck"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsStatusBulkCheck"]),
                                IsInternalSender = dt.Rows[0]["_IsInternalSender"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsInternalSender"]),
                                APIOutletID = dt.Rows[0]["_APIOutletID"] is DBNull ? string.Empty : dt.Rows[0]["_APIOutletID"].ToString()
                            },
                            ServiceID = dt.Rows[0]["_ServiceID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_ServiceID"])                           
                        };
                        _res.OID = dt.Rows[0]["OID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["OID"]);
                        _res.TDate = dt.Rows[0]["_TDate"] is DBNull ? string.Empty : dt.Rows[0]["_TDate"].ToString();
                        _res.SCode = dt.Rows[0]["_SCode"] is DBNull ? string.Empty : dt.Rows[0]["_SCode"].ToString();
                        _res.OutletID = dt.Rows[0]["_OutletID"] is DBNull ? 0:Convert.ToInt32(dt.Rows[0]["_OutletID"]);
                        _res.BrandName = dt.Rows[0]["_BrandName"] is DBNull ? string.Empty : dt.Rows[0]["_BrandName"].ToString();
                        _res.APIOutletID = dt.Rows[0]["_APIOutletID"] is DBNull ? string.Empty : dt.Rows[0]["_APIOutletID"].ToString();
                        _res.VendorID2 = dt.Rows[0]["_VendorID2"] is DBNull ? string.Empty : dt.Rows[0]["_VendorID2"].ToString();
                        _res.TransactionReqID = dt.Rows[0]["_TransactionReqID"] is DBNull ? string.Empty : dt.Rows[0]["_TransactionReqID"].ToString();
                        _res.SPKey = dt.Rows[0]["_OPID"] is DBNull ? string.Empty : dt.Rows[0]["_OPID"].ToString();
                        _res.APIOPCode = dt.Rows[0]["_APIOPCode"] is DBNull ? string.Empty : dt.Rows[0]["_APIOPCode"].ToString();
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
                    UserId = _req.LoginID
                });
            }
            return _res;
        }
        public Task<object> Call() => throw new NotImplementedException();
        public string GetName() => "proc_GetStatusCheckURL";
    }
}
