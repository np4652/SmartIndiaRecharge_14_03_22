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
    public class ProcGetEmpTodayLivePST : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetEmpTodayLivePST(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var LoginID = (int)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginID", LoginID)
            };

            var data = new List<TodayLivePST>();
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                    if (dt.Rows.Count > 0)
                    {
                        foreach (DataRow dr in dt.Rows)
                        {
                            data.Add(new TodayLivePST
                            {
                                Type = Convert.ToString(dr["_Type"]),
                                TotalAmount = dr["_TotalAmount"] is DBNull?0: Convert.ToDecimal(dr["_TotalAmount"]),
                                TotalUser = dr["_TotalUser"] is DBNull?0: Convert.ToInt32(dr["_TotalUser"]),
                                UniqueUser = dr["_UniqueUser"] is DBNull ? 0 : Convert.ToInt32(dr["_UniqueUser"]),
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
        public string GetName() => "Proc_GetEmpTodayLivePST";
    }
}