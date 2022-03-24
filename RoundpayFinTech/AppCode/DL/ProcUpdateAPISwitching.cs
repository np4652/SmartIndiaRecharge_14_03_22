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
    public class ProcUpdateAPISwitching : IProcedure
    {
        private readonly IDAL _dal;
        public ProcUpdateAPISwitching(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            APISwitchedReq _req = (APISwitchedReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT", _req.LoginTypeID),
                new SqlParameter("@LoginID", _req.LoginID),
                new SqlParameter("@OID", _req.aPISwitched.OID),
                new SqlParameter("@APIID", _req.aPISwitched.APIID),
                new SqlParameter("@MaxCount", _req.aPISwitched.MaxCount),
                new SqlParameter("@IsActive", _req.aPISwitched.IsActive),
                new SqlParameter("@IP", _req.CommonStr),
                new SqlParameter("@Browser", _req.CommonStr2),
                new SqlParameter("@FailoverCount", _req.aPISwitched.FailoverCount)
            };
            var _resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    _resp.Statuscode = Convert.ToInt32(dt.Rows[0][0]);
                    _resp.Msg = dt.Rows[0]["Msg"].ToString();
                }
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = _req.LoginTypeID,
                    UserId = _req.LoginID
                });
            }
            return _resp;
        }

        public object Call() => throw new NotImplementedException();

        public string GetName() => "proc_UpdateAPISwitching";
    }
}
