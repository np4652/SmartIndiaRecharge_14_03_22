using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.Employee;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL.Employee
{
    public class ProcGetLeadStats : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcGetLeadStats(IDAL dal) => _dal = dal;
        public async Task<object> Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT",req.LoginTypeID),
                new SqlParameter("@LoginID",req.LoginID)
            };
            var res = new LeadStats();
            try
            {
                DataTable dt = await _dal.GetByProcedureAsync(GetName(), param).ConfigureAwait(true);
                if (dt.Rows.Count > 0)
                {
                    res = new LeadStats
                    {
                        Total = dt.Rows[0]["Total"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["Total"]),
                        Followup = dt.Rows[0]["Followup"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["Followup"]),
                        Junk = dt.Rows[0]["Junk"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["Junk"]),
                        Matured = dt.Rows[0]["Matured"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["Matured"]),
                        TodayFollowup = dt.Rows[0]["TodayFollowup"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["TodayFollowup"]),
                    };
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

        public Task<object> Call() => throw new NotImplementedException();

        public string GetName() => "proc_GetLeadStats";
    }
}
