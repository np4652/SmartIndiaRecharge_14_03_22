using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcDashBoardDetail : IProcedure
    {
        private readonly IDAL _dal;
        public ProcDashBoardDetail(IDAL dal) => _dal = dal;
        public object Call(object obj) => throw new NotImplementedException();

        public object Call()
        {
            var dashboard = new Dashboard();
            try
            {
                var DashChart = new List<Dashboard_Chart>();
                DataSet ds = _dal.GetByProcedureAdapterDS(GetName());
                if (ds.Tables.Count ==1)
                {
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        var dsChart = new Dashboard_Chart
                        {
                            Dispute = Convert.ToDecimal(row["Dispute"]),
                            DisputeCount = Convert.ToInt16(row["DisputeCount"]),
                            FAmount = Convert.ToDecimal(row["FAmount"]),
                            FCount = Convert.ToInt16(row["FCount"]),
                            PAmount = Convert.ToDecimal(row["PAmount"]),
                            PCount = Convert.ToInt16(row["PCount"]),
                            SAmount = Convert.ToDecimal(row["SAmount"]),
                            SCount = Convert.ToInt16(row["SCount"]),
                            OpTypeID = Convert.ToInt16(row["_opTypeID"]),
                            OpType = row["_opType"].ToString()
                        };
                        DashChart.Add(dsChart);
                    }
                }
                if (ds.Tables.Count > 1)
                {
                    
                }
                dashboard.Chart = DashChart;
            }
            catch (Exception ex)
            {
                //var _ = _err.FnErrorLog(GetType().Name, "Call", ex.Message, _req.LoginID);
            }
            return dashboard;
        }

        public string GetName() => "proc_DashBoarDetail";
    }
}
