using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.Employee;
using RoundpayFinTech.AppCode.Model.ProcModel;

namespace RoundpayFinTech.AppCode.DL.Employee
{
    public class ProcGetLastSevenDayPSTDataForEmp : IProcedure
    {
        private IDAL _dal;
        public ProcGetLastSevenDayPSTDataForEmp(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var LoginID = (int)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginID", LoginID)
            };
            var res = new List<PSTDataList>();
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        res.Add(new PSTDataList
                        {
                            TransactionDate = Convert.ToString(dr["_TransactionDate"]),
                            Primary = dr["_Primary"] is DBNull ? 0 : Convert.ToDecimal(dr["_Primary"]),
                            Secoundary = dr["_Secoundry"] is DBNull ? 0 : Convert.ToDecimal(dr["_Secoundry"]),
                            Tertiary = dr["_Tertiary"] is DBNull ? 0 : Convert.ToDecimal(dr["_Tertiary"]),
                        });
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
                    LoginTypeID = 3,
                    UserId = LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }

            return res;
        }

        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_GetLastSevenDayPSTDataForEmp";
    }
}
