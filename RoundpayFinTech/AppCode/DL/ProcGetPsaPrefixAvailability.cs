using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.Employee;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetPsaPrefixAvailability : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetPsaPrefixAvailability(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT",req.LoginTypeID),
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@PsaKey",req.CommonStr)
            };
            bool res = false;
            try
            {
                var dt = _dal.GetByProcedure(GetName(),param);
                if (dt !=  null && dt.Rows.Count > 0)
                {
                   if(Convert.ToInt32(dt.Rows[0]["Status"]) == 1 &&  Convert.ToString(dt.Rows[0]["Msg"]) == "Success")
                    {
                        res = true;
                    }
                }
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = req.LoginTypeID,
                    UserId = req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }
        public object Call() => throw new NotImplementedException();
        public string GetName() => "Proc_GetUpdatePsaPrefix";
    }
}