using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcPSTReportForUser : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcPSTReportForUser(IDAL dal) => _dal = dal;
        public async Task<object> Call(object obj)
        {
            var _req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT", _req.LoginTypeID),
                new SqlParameter("@LoginID", _req.LoginID),
                new SqlParameter("@Date", _req.CommonStr ?? DateTime.Now.ToString("dd MMM yyyy"))
            };
            var _res = new PSTReportUser();
            try
            {
                var dt = await _dal.GetByProcedureAsync(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    //FTD
                    _res.PriFTD = dt.Rows[0]["_CurrentDayPri"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[0]["_CurrentDayPri"].ToString());
                    _res.SecFTD = dt.Rows[0]["_CurrentDaySec"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[0]["_CurrentDaySec"].ToString());
                    _res.TerFTD = dt.Rows[0]["_CurrentDayTer"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[0]["_CurrentDayTer"].ToString());

                    //Last Day
                    _res.PriLastDay = dt.Rows[0]["_TodayPri"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[0]["_TodayPri"].ToString());
                    _res.SecLastDay = dt.Rows[0]["_TodaySec"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[0]["_TodaySec"].ToString());
                    _res.TerLastDay = dt.Rows[0]["_TodayTer"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[0]["_TodayTer"].ToString());

                    //Month Till Date
                    _res.PriMonthTillDate = dt.Rows[0]["_Pri"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[0]["_Pri"].ToString());
                    _res.SecMonthTillDate = dt.Rows[0]["_Sec"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[0]["_Sec"].ToString());
                    _res.TerMonthTillDate = dt.Rows[0]["_Ter"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[0]["_Ter"].ToString());

                    //Last Month Till Date
                    _res.PriLastMonthTillDate = dt.Rows[0]["_PriLastMonthTillDate"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[0]["_PriLastMonthTillDate"].ToString());
                    _res.SecLastMonthTillDate = dt.Rows[0]["_SecLastMonthTillDate"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[0]["_SecLastMonthTillDate"].ToString());
                    _res.TerLastMonthTillDate = dt.Rows[0]["_TerLastMonthTillDate"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[0]["_TerLastMonthTillDate"].ToString());

                    //Difference in Percentage
                    _res.Pri_LMTD_MTD_Diff = dt.Rows[0]["Pri_LMTD_MTD_Diff"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[0]["Pri_LMTD_MTD_Diff"].ToString());
                    _res.Sec_LMTD_MTD_Diff = dt.Rows[0]["Sec_LMTD_MTD_Diff"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[0]["Sec_LMTD_MTD_Diff"].ToString());
                    _res.Ter_LMTD_MTD_Diff = dt.Rows[0]["Ter_LMTD_MTD_Diff"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[0]["Ter_LMTD_MTD_Diff"].ToString());

                    _res.Pri_LastDay_Current_Diff = dt.Rows[0]["Pri_LastDay_Current_Diff"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[0]["Pri_LastDay_Current_Diff"].ToString());
                    _res.Sec_LastDay_Current_Diff = dt.Rows[0]["Sec_LastDay_Current_Diff"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[0]["Sec_LastDay_Current_Diff"].ToString());
                    _res.Ter_LastDay_Current_Diff = dt.Rows[0]["Ter_LastDay_Current_Diff"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[0]["Ter_LastDay_Current_Diff"].ToString());
                }
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = _req.LoginTypeID,
                    UserId = _req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return _res;
        }

        public Task<object> Call() => throw new NotImplementedException();

        public string GetName() => "proc_GetPriSecTerUsersData";
    }
}
