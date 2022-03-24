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
    public class ProcSetAPISwitching : IProcedure
    {
        private readonly IDAL _dal;
        public ProcSetAPISwitching(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var _req = (APISwitchedReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT", _req.LoginTypeID),
                new SqlParameter("@LoginID", _req.LoginID),
                new SqlParameter("@OID", _req.aPISwitched.OID),
                new SqlParameter("@APIID", _req.aPISwitched.APIID),
                new SqlParameter("@RealAPIID", _req.aPISwitched.RealAPIID),
                new SqlParameter("@IP", _req.CommonStr??string.Empty),
                new SqlParameter("@Browser", _req.CommonStr2??string.Empty),
                new SqlParameter("@APIIDRetailor", _req.aPISwitched.BackupAPIIDRetailor)
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
                    _resp.Msg = dt.Rows[0]["Msg"].ToString();
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
            return _resp;
        }

        public object Call() => throw new NotImplementedException();

        public string GetName() => "proc_SetAPISwitching";
    }
}
