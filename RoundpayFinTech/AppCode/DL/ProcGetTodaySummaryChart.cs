using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetTodaySummaryChart : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetTodaySummaryChart(IDAL dal) => _dal = dal;

        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@UserID",req.LoginID)
            };
            List<Dashboard_Chart> DashChart = new List<Dashboard_Chart>();
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 1)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        Dashboard_Chart dsChart = new Dashboard_Chart
                        {
                            Dispute = Convert.ToDecimal(row["Dispute"]),
                            DisputeCount = Convert.ToInt16(row["DisputeCount"]),
                            FAmount = Convert.ToDecimal(row["FAmount"]),
                            FCount = Convert.ToInt16(row["FCount"]),
                            PAmount = Convert.ToDecimal(row["PCount"]),
                            PCount = Convert.ToInt16(row["PAmount"]),
                            SAmount = Convert.ToDecimal(row["SAmount"]),
                            SCount = Convert.ToInt16(row["SCount"]),
                            OpTypeID = Convert.ToInt16(row["_opTypeID"]),
                            OpType = row["_opType"].ToString()
                        };
                        DashChart.Add(dsChart);
                    }
                }
            }
            catch (Exception ex)
            {
                //var _ = _err.FnErrorLog(GetType().Name, "Call", ex.Message, _req.LoginID);
            }
            return DashChart;
        }

        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_GetServiceChartData";
    }
}
