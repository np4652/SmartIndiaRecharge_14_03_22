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
    public class ProcInitiateEKYCCallRequest : IProcedure
    {
        private readonly IDAL _dal;
        public ProcInitiateEKYCCallRequest(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = { 
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@APIID",req.CommonInt)
            };
            try
            {
                return Convert.ToInt32(_dal.GetByProcedure(GetName(), param).Rows[0][0]);
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = req.LoginTypeID,
                    UserId = req.LoginID
                });
            }
            return 0;
        }

        public object Call()
        {
            throw new NotImplementedException();
        }

        public string GetName() => "proc_InitiateEKYCCallRequest";
    }
}
