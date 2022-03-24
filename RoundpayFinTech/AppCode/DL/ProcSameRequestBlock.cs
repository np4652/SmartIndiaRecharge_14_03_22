using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcSameRequestBlock : IProcedure
    {
        private readonly IDAL _dal;
        public ProcSameRequestBlock(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (BlockSameRequest)obj;
            SqlParameter[] param = {
                new SqlParameter("@Method",req.Method??string.Empty),
                new SqlParameter("@Request",req.Request??string.Empty),
                new SqlParameter("@IP",req.IP??string.Empty),
                new SqlParameter("@Browser",req.Browser??string.Empty)
            };
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    return Convert.ToInt16(dt.Rows[0][0]) == 1;
                }
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = 1,
                    UserId = 1
                });
            }
            return false;
        }
        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_SameRequestBlock";
    }
}
