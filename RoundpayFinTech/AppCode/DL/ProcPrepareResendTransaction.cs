using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.Model.Report;
using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcPrepareResendTransaction : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcPrepareResendTransaction(IDAL dal) => _dal = dal;
        public async Task<object> Call(object obj)
        {
            var _req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT", _req.LoginTypeID),
                new SqlParameter("@LoginID", _req.LoginID),
                new SqlParameter("@APIID",_req.CommonInt),
                new SqlParameter("@TID", _req.CommonInt2),
                new SqlParameter("@IP", _req.CommonStr?? ""),
                new SqlParameter("@Browser", _req.CommonStr2 ?? "")
            };
            var _res = new PrepairedTransactionReq
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            try
            {
                var dt = await _dal.GetByProcedureAsync(GetName(), param).ConfigureAwait(false);
                if (dt.Rows.Count > 0)
                {
                    _res.Statuscode = Convert.ToInt32(dt.Rows[0][0]);
                    _res.Msg = dt.Rows[0]["Msg"].ToString();
                    if (_res.Statuscode == ErrorCodes.One)
                    {
                        _res.TID = dt.Rows[0]["TID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["TID"]);
                        _res.WID = dt.Rows[0]["_WID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_WID"]);
                        _res.TransactionID = dt.Rows[0]["_TransactionID"].ToString();
                        _res.Operator = dt.Rows[0]["_Operator"] is DBNull ? string.Empty : dt.Rows[0]["_Operator"].ToString();
                        _res.CustomerNumber = dt.Rows[0]["_CustomerNumber"] is DBNull ? string.Empty : dt.Rows[0]["_CustomerNumber"].ToString();
                        _res.IsBBPS = dt.Rows[0]["_IsBBPS"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsBBPS"]);
                        _res.UserID = Convert.ToInt32(dt.Rows[0]["_UserID"]);
                        _res.AccountNo = dt.Rows[0]["_Account"].ToString();
                        _res.RequestedAmount = Convert.ToDecimal(dt.Rows[0]["_RequestedAmount"]);
                        _res.OID = Convert.ToInt32(dt.Rows[0]["_OID"]);
                        _res.Optional1 = dt.Rows[0]["_Optional1"] is DBNull ? "" : dt.Rows[0]["_Optional1"].ToString();
                        _res.Optional2 = dt.Rows[0]["_Optional2"].ToString();
                        _res.Optional3 = dt.Rows[0]["_Optional3"].ToString();
                        _res.Optional4 = dt.Rows[0]["_Optional4"].ToString();
                        _res.PAN = dt.Rows[0]["_PAN"].ToString();
                        _res.Aadhar = dt.Rows[0]["_AADHAR"].ToString();
                        _res.aPIDetail = new APIDetail
                        {
                            ID = dt.Rows[0]["_APIID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_APIID"]),
                            URL = dt.Rows[0]["_URL"] is DBNull ? string.Empty : dt.Rows[0]["_URL"].ToString(),
                            APIType = Convert.ToInt16(dt.Rows[0]["_APIType"] is DBNull ? false : dt.Rows[0]["_APIType"]),
                            Name = dt.Rows[0]["_APIName"] is DBNull ? "" : dt.Rows[0]["_APIName"].ToString(),
                            RequestMethod = dt.Rows[0]["_RequestMethod"] is DBNull ? "" : dt.Rows[0]["_RequestMethod"].ToString(),
                            StatusName = dt.Rows[0]["_StatusName"] is DBNull ? "" : dt.Rows[0]["_StatusName"].ToString(),
                            SuccessCode = dt.Rows[0]["_SuccessCode"] is DBNull ? "" : dt.Rows[0]["_SuccessCode"].ToString(),
                            LiveID = dt.Rows[0]["_LiveID"] is DBNull ? "" : dt.Rows[0]["_LiveID"].ToString(),
                            VendorID = dt.Rows[0]["_VendorID"] is DBNull ? "" : dt.Rows[0]["_VendorID"].ToString(),
                            ResponseTypeID = Convert.ToInt32(dt.Rows[0]["_ResponseTypeID"] is DBNull ? false : dt.Rows[0]["_ResponseTypeID"]),
                            APIOpCode = dt.Rows[0]["APIOPCode"] is DBNull ? "" : dt.Rows[0]["APIOPCode"].ToString(),
                            CommType = dt.Rows[0]["APIComType"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["APIComType"]),
                            MsgKey = dt.Rows[0]["_MsgKey"] is DBNull ? "" : dt.Rows[0]["_MsgKey"].ToString(),
                            ErrorCodeKey = dt.Rows[0]["_ErrorCodeKey"] is DBNull ? "" : dt.Rows[0]["_ErrorCodeKey"].ToString(),
                            GroupID = dt.Rows[0]["_GroupID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_GroupID"]),
                            ContentType = dt.Rows[0]["_ContentType"] is DBNull ? 0 : Convert.ToInt16(dt.Rows[0]["_ContentType"]),
                            GroupCode = dt.Rows[0]["_GroupCode"] is DBNull ? string.Empty : dt.Rows[0]["_GroupCode"].ToString(),
                            FailCode = dt.Rows[0]["_FailCode"] is DBNull ? string.Empty : dt.Rows[0]["_FailCode"].ToString()
                        };
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
                    LoginTypeID = _req.LoginTypeID,
                    UserId = _req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return _res;
        }

        public Task<object> Call() => throw new NotImplementedException();

        public string GetName() => "proc_PrepareResendTransaction";
    }
}