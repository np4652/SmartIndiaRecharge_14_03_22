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
    public class procTodayTransactors : IProcedure
    {
        private IDAL _dal;
        public procTodayTransactors(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginID", req.LoginID),
                new SqlParameter("@ReqType", req.CommonInt)
            };

            var res = new List<TodayTransactorsModal>();
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        res.Add(new TodayTransactorsModal
                        {
                            UserId=Convert.ToInt32(dr["_UserId"]),
                            UserName=Convert.ToString(dr["_Name"]),
                            OutletName = Convert.ToString(dr["_OutletName"]),
                            MobilleNo=Convert.ToString(dr["_MobileNo"]),
                            TransactionCount=Convert.ToInt32(dr["TransactionCount"]),
                            Amount = Convert.ToDecimal(dr["_Amount"]),

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
                    LoginTypeID = req.LoginTypeID,
                    UserId = req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }

            return res;
        }

        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_TodayTransactors";
    }
}
