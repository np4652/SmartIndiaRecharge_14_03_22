using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcUpdateFreezeStatus : IProcedure
    {
        private readonly IDAL _dal;
        public ProcUpdateFreezeStatus(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (CommonReq)obj;

            SqlParameter[] param = { 
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@FrozenDuration",req.CommonInt)
            };
            try
            {
                var dt = _dal.GetByProcedure(GetName(),param);
                return true;
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = 1,
                    UserId = req.LoginID
                });
            }
            return false;
        }

        public object Call() => throw new NotImplementedException();

        public string GetName() => "proc_UpdateFreezeStatus";
    }
}
