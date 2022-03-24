using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;

namespace RoundpayFinTech.AppCode.DL.Employee
{
    public class ProcEmpSelectRole : IProcedure
    {
        private readonly IDAL _dal;
        public ProcEmpSelectRole(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            int UserID = (int)obj;
            SqlParameter[] param ={
               new SqlParameter("@EmpID", UserID)
            };
            List<RoleMaster> Roles = new List<RoleMaster>();
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        Roles.Add(new RoleMaster
                        {
                            ID = Convert.ToInt32(dr["ValueField"]),
                            Role = dr["TextField"].ToString(),
                            Ind = Convert.ToInt32(dr["Sno"])
                        });
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
                    LoginTypeID = 1,
                    UserId = 1
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return Roles;
        }

        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_Emp_SelectRole";
    }
}
