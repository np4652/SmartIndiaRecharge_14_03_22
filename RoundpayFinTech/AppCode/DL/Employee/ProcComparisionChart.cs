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
    public class ProcComparisionChart : IProcedure
    {
        private readonly IDAL _dal;
        public ProcComparisionChart(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var LoginID = (int)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginID", LoginID)
            };
            
            var data= new List<ComparisionChart>();
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                    if (dt.Rows.Count > 0)
                    {
                        foreach (DataRow dr in dt.Rows)
                        {
                            data.Add(new ComparisionChart
                            {
                                Type = Convert.ToString(dr["_Type"]),
                                LM = dr["_LM"] is DBNull ? 0 : Convert.ToDecimal(dr["_LM"]),
                                LMTD = dr["_LMTD"] is DBNull ? 0 : Convert.ToDecimal(dr["_LMTD"]),
                                MTD = dr["_MTD"] is DBNull ? 0 : Convert.ToDecimal(dr["_MTD"]),
                                Growth = dr["_Growth"] is DBNull ? 0 : Convert.ToDecimal(dr["_Growth"]),
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
        public string GetName() => "Proc_ComparisionChart";
    }
}