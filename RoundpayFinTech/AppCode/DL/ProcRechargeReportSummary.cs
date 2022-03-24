using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcRechargeReportSummary : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcRechargeReportSummary(IDAL dal) => _dal = dal;
        public string GetName() => "proc_RechargeReportSummary";
        public async Task<object> Call(object obj)
        {
            CommonReq req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@ReportType", req.CommonInt)
            };
            

            var res = new RechargeReportSummary();            
            try
            {
                DataTable dt = await _dal.GetByProcedureAsync(GetName(), param);
                if (dt.Rows.Count > 0) {
                    if (dt.Columns.Contains("TotalSuccessNo")) {
                        res.TotalSuccessNo = dt.Rows[0]["TotalSuccessNo"] is DBNull ? 0 : Convert.ToInt64(dt.Rows[0]["TotalSuccessNo"]);
                        res.TotalSuccessAmount = dt.Rows[0]["TotalSuccessAmount"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[0]["TotalSuccessAmount"]);
                        res.TotalFailedNo = dt.Rows[0]["TotalFailedNo"] is DBNull ? 0 : Convert.ToInt64(dt.Rows[0]["TotalFailedNo"]);
                        res.TotalFailedAmount = dt.Rows[0]["TotalFailedAmt"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[0]["TotalFailedAmt"]);
                        res.TotalPendingNo = dt.Rows[0]["TotalPendingNo"] is DBNull ? 0 : Convert.ToInt64(dt.Rows[0]["TotalPendingNo"]);
                        res.TotalPendingAmount = dt.Rows[0]["TotalPendingAmt"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[0]["TotalPendingAmt"]);
                    }
                }
            }
            catch (Exception er)
            { }
            return res;
        }

        public Task<object> Call() => throw new NotImplementedException();
    }
}
