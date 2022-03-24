using Fintech.AppCode.DB;
using Fintech.AppCode.HelperClass;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcShowPassword : IProcedure
    {
        private readonly IDAL _dal;
        public ProcShowPassword(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            SqlParameter[] param = {
                new SqlParameter("@UserID", (int)obj),
            };
            var _res = string.Empty;
            try
            {
                var dt = _dal.Get(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    _res = dt.Rows[0]["_Password"] is DBNull ? string.Empty : dt.Rows[0]["_Password"].ToString();
                }
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = 1,
                    UserId = 1
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return _res;
        }
        public object Call() => throw new NotImplementedException();
        public string GetName() => "select _Password from tbl_UsersLogin where _UserID<>1 and _UserID=@UserID";
    }
}
