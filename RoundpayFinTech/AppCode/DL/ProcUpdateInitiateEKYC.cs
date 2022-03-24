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
    public class ProcUpdateInitiateEKYC : IProcedure
    {
        private readonly IDAL _dal;
        public ProcUpdateInitiateEKYC(IDAL dal) => _dal=dal;
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = { 
                new SqlParameter("@InitiateID",req.CommonInt),
                new SqlParameter("@VendorID",req.CommonStr??string.Empty),
                new SqlParameter("@SecurityKey",req.CommonStr1??string.Empty)
            };
            try
            {
                _dal.GetByProcedure(GetName(),param);
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
            return false;
        }

        public object Call()
        {
            throw new NotImplementedException();
        }

        public string GetName() => "proc_UpdateInitiateEKYC";
    }
}
