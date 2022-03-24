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
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcLogout : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcLogout(IDAL dal) => _dal = dal;
        public async Task<object> Call(object obj)
        {
            var req = (LogoutReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT",req.LT),
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@SessID",req.SessID),
                new SqlParameter("@ULT",req.ULT),
                new SqlParameter("@UserID",req.UserID),
                new SqlParameter("@SessionType",req.SessionType),
                new SqlParameter("@RequestMode",req.RequestMode),
                new SqlParameter("@IP",req.IP??""),
                new SqlParameter("@Browser",req.Browser??"")
            };
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            try
            {
                var dt = await _dal.GetByProcedureAsync(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    res.Statuscode = dt.Rows[0][0] is DBNull ? -1 : Convert.ToInt16(dt.Rows[0][0]);
                    res.Msg = dt.Rows[0]["Msg"] is DBNull ? "" : dt.Rows[0]["Msg"].ToString();
                }
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = req.LT,
                    UserId = req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }

        public Task<object> Call() => throw new NotImplementedException();

        public string GetName() => "proc_Logout";
    }
}
