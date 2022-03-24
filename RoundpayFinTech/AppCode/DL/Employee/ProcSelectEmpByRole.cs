using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.Employee;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL.Employee
{
    public class ProcSelectEmpByRole : IProcedure
    {
        private readonly IDAL _dal;
        public ProcSelectEmpByRole(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT",req.LoginTypeID),
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@RoleId",req.CommonInt),
                new SqlParameter("@OnlyChildlren",req.CommonBool)
        };
            var res = new List<EList>();

            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);

                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        var elist = new EList
                        {
                            EmpID = Convert.ToInt32(row["_ID"] is DBNull ? 0 : row["_ID"]),
                            Name = row["_name"] is DBNull ? "" : row["_name"].ToString(),
                            EmpRole = row["_role"] is DBNull ? "" : row["_role"].ToString(),
                            MobileNo = row["_MobileNo"] is DBNull ? "" : row["_MobileNo"].ToString(),
                        };
                        res.Add(elist);
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorLog errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = 1,
                    UserId = 1
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }
        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_SelectEmpByRole";
    }
}
