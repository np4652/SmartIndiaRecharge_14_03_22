using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    //proc_MakeWTRRequest
    public class ProcMakeWTRRequest : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcMakeWTRRequest(IDAL dal)
        {
            _dal = dal;
        }
        public async Task<object> Call(object obj)
        {
            var _req = (WTRRequest)obj;
            SqlParameter[] param = {
             new SqlParameter("@LoginID", _req.UserID),
             new SqlParameter("@LT", _req.LoginType),
             new SqlParameter("@TID", _req.TID),
             new SqlParameter("@TransactionID", _req.RPID),
             new SqlParameter("@RightAccount", _req.RightAccount)
            };

            var _res = new RefundRequestResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            try
            {
                var dt = await _dal.GetByProcedureAsync(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    _res.Statuscode = Convert.ToInt32(dt.Rows[0][0]);
                    _res.Msg = dt.Rows[0]["Msg"].ToString();
                    _res.Account = dt.Rows[0]["_Account"] is DBNull ? "" : dt.Rows[0]["_Account"].ToString();
                    _res.Amount = dt.Rows[0]["_Amount"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[0]["_Amount"]);
                    _res.Type = dt.Rows[0]["_Type"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_Type"]);
                    _res.RefundStatus = dt.Rows[0]["_RefundStatus"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_RefundStatus"]);
                    _res.RefundRemark = dt.Rows[0]["_RefundRemark"] is DBNull ? "" : dt.Rows[0]["_RefundRemark"].ToString();
                    _res.Balance = dt.Rows[0]["_Balance"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[0]["_Balance"]);
                    _res.DisputeURL = dt.Rows[0]["DisputeURL"] is DBNull ? "" : dt.Rows[0]["DisputeURL"].ToString();
                }
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = _req.LoginType,
                    UserId = _req.UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return _res;
        }

        public Task<object> Call()
        {
            throw new NotImplementedException();
        }
        public string GetName() => "proc_MakeWTRRequest";
    }
}