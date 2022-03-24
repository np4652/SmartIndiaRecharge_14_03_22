using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model.Employee;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;


namespace RoundpayFinTech.AppCode.DL.Employee
{
    public class ProcEmpDownline : IProcedure
    {
        private readonly IDAL _dal;
        public ProcEmpDownline(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var LoginID = (int)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginID", LoginID)
            };
            
            var data = new List<EmpDownlineUser>();
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                    if (dt.Rows.Count > 0)
                    {
                        foreach (DataRow dr in dt.Rows)
                        {
                            data.Add(new EmpDownlineUser
                            {
                                UserID = Convert.ToInt32(dr["_UserID"]),
                                UserMobile = Convert.ToString(dr["_MobileNo"]),
                                Role = Convert.ToString(dr["_Role"]),
                                State = Convert.ToString(dr["_State"]),
                                City = Convert.ToString(dr["_City"]),
                                Prefix = Convert.ToString(dr["_Prefix"]),
                                UserName = Convert.ToString(dr["_UserName"]),
                                Attandance=Convert.ToBoolean(dr["Attandance"])
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
                    UserId = LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return data;
        }
        public object Call() => throw new NotImplementedException();
        public string GetName() => "Proc_EmpDownline";
    }
}