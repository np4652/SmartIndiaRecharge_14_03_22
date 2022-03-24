using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcPgTransactionFromPage : IProcedure
    {
        private readonly IDAL _dal;
        public ProcPgTransactionFromPage(IDAL dal) => _dal = dal;
        public string GetName() => "proc_PgTransactionFromPage";
        public object Call(object obj)
        {
            var _req = (TransactionPG)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT", _req.LT),
                new SqlParameter("@LoginID", _req.LoginID),
                new SqlParameter("@TID", _req.TID),
                new SqlParameter("@Type", _req.Type),
                new SqlParameter("@Remark", String.IsNullOrEmpty(_req.Remark)?"":_req.Remark),
                new SqlParameter("@RequestIP", String.IsNullOrEmpty(_req.RequestIP)?"":_req.RequestIP),
                new SqlParameter("@Browser", String.IsNullOrEmpty(_req.Browser)?"":_req.Browser),
            };
            var _resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    _resp.Statuscode = Convert.ToInt32(dt.Rows[0][0]);
                    _resp.Msg = Convert.ToString(dt.Rows[0]["Msg"]);
                }
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = _req.LT,
                    UserId = _req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return _resp;
        }
        public object Call() => throw new NotImplementedException();
    }
}
