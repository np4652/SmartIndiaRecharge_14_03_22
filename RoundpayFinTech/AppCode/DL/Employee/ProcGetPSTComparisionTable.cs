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
    public class ProcGetPSTComparisionTable : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetPSTComparisionTable(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var LoginID = (int)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginID", LoginID)
            };

            var data = new List<PSTComparisionTable>();
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                    if (dt.Rows.Count > 0)
                    {
                        foreach (DataRow dr in dt.Rows)
                        {
                            data.Add(new PSTComparisionTable
                            {
                                UserID = Convert.ToInt32(dr["_UserID"]),
                                UserName = Convert.ToString(dr["_USer"]),
                               Pri1=dr["Pri1"] is DBNull?0:Convert.ToDecimal(dr["Pri1"]),
                               Pri2=dr["Pri2"] is DBNull?0:Convert.ToDecimal(dr["Pri2"]),
                               Pri3=dr["Pri3"] is DBNull?0:Convert.ToDecimal(dr["Pri3"]),
                               Pri4=dr["Pri4"] is DBNull?0:Convert.ToDecimal(dr["Pri4"]),
                               Pri5=dr["Pri5"] is DBNull?0:Convert.ToDecimal(dr["Pri5"]),
                               Pri6=dr["Pri6"] is DBNull?0:Convert.ToDecimal(dr["Pri6"]),
                               Pri7=dr["Pri7"] is DBNull?0:Convert.ToDecimal(dr["Pri7"]),
                               Pri8=dr["Pri8"] is DBNull?0:Convert.ToDecimal(dr["Pri8"]),
                               Pri9=dr["Pri9"] is DBNull?0:Convert.ToDecimal(dr["Pri9"]),
                               Pri10=dr["Pri10"] is DBNull?0:Convert.ToDecimal(dr["Pri10"]),
                               Sec1=dr["Sec1"] is DBNull?0:Convert.ToDecimal(dr["Sec1"]),
                               Sec2=dr["Sec2"] is DBNull?0:Convert.ToDecimal(dr["Sec2"]),
                               Sec3=dr["Sec3"] is DBNull?0:Convert.ToDecimal(dr["Sec3"]),
                               Sec4=dr["Sec4"] is DBNull?0:Convert.ToDecimal(dr["Sec4"]),
                               Sec5=dr["Sec5"] is DBNull?0:Convert.ToDecimal(dr["Sec5"]),
                               Sec6=dr["Sec6"] is DBNull?0:Convert.ToDecimal(dr["Sec6"]),
                               Sec7=dr["Sec7"] is DBNull?0:Convert.ToDecimal(dr["Sec7"]),
                               Sec8=dr["Sec8"] is DBNull?0:Convert.ToDecimal(dr["Sec8"]),
                               Sec9=dr["Sec9"] is DBNull?0:Convert.ToDecimal(dr["Sec9"]),
                               Sec10=dr["Sec10"] is DBNull?0:Convert.ToDecimal(dr["Sec10"]),
                               Ter1=dr["Ter1"] is DBNull?0:Convert.ToDecimal(dr["Ter1"]),
                               Ter2=dr["Ter2"] is DBNull?0:Convert.ToDecimal(dr["Ter2"]),
                               Ter3=dr["Ter3"] is DBNull?0:Convert.ToDecimal(dr["Ter3"]),
                               Ter4=dr["Ter4"] is DBNull?0:Convert.ToDecimal(dr["Ter4"]),
                               Ter5=dr["Ter5"] is DBNull?0:Convert.ToDecimal(dr["Ter5"]),
                               Ter6=dr["Ter6"] is DBNull?0:Convert.ToDecimal(dr["Ter6"]),
                               Ter7=dr["Ter7"] is DBNull?0:Convert.ToDecimal(dr["Ter7"]),
                               Ter8=dr["Ter8"] is DBNull?0:Convert.ToDecimal(dr["Ter8"]),
                               Ter9=dr["Ter9"] is DBNull?0:Convert.ToDecimal(dr["Ter9"]),
                               Ter10=dr["Ter10"] is DBNull?0:Convert.ToDecimal(dr["Ter10"]),
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
        public string GetName() => "Proc_GetPSTComparisionTable";
    }
}