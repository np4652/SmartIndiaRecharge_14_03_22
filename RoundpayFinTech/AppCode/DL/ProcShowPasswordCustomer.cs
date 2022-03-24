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
    public class ProcShowPasswordCustomer : IProcedure
    {
        private readonly IDAL _dal;
        public ProcShowPasswordCustomer(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            SqlParameter[] param = {
                new SqlParameter("@CusID", (int)obj),
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
        public string GetName() => "select _Password from tbl_CustomerCare where _ID=@CusID";
    }
}
