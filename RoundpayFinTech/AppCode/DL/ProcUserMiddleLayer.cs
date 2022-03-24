using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
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
   
    public class ProcUserMiddleLayer : IProcedure
    {
        private readonly IDAL _dal;
        public ProcUserMiddleLayer(IDAL dal)
        {
            _dal = dal;
        }
        public object Call()
        {
            throw new NotImplementedException();
        }

        public object Call(object obj)
        {
            int  LoginID = (int)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginID", LoginID)
            };
            var dashboard = new MiddleLayerUser();
            var dash = new List<MiddleUser>();
            var DashChart = new List<Dashboard_Chart>();
            try
            {
              
                DataSet ds = _dal.GetByProcedureAdapterDS(GetName(),param);
                if (ds.Tables.Count > 0)
                {
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        MiddleUser dss = new MiddleUser
                        {
                            Role = row["_Role"].ToString(),
                            Status=Convert.ToInt32(row["Status"]),
                            TranUser= Convert.ToInt32(row["_TranUse"])
                        };
                        dash.Add(dss);
                    }
                }
                if (ds.Tables.Count > 1)
                {
                    foreach (DataRow row in ds.Tables[1].Rows)
                    {
                        //PCount PAmount SCount SAmount FCount FAmount Dispute _OpType
                        Dashboard_Chart dsChart = new Dashboard_Chart
                        {
                            Dispute = Convert.ToDecimal(row["Dispute"]),
                            //DisputeCount = Convert.ToInt16(row["DisputeCount"]),
                            FAmount = Convert.ToDecimal(row["FAmount"]),
                            FCount = Convert.ToInt16(row["FCount"]),
                            PAmount = Convert.ToDecimal(row["PCount"]),
                            PCount = Convert.ToInt16(row["PAmount"]),
                            SAmount = Convert.ToDecimal(row["SAmount"]),
                            SCount = Convert.ToInt16(row["SCount"]),
                           // OpTypeID = Convert.ToInt16(row["_opTypeID"]),
                            OpType= row["_opType"].ToString()
                        };
                        DashChart.Add(dsChart);
                    }
                }
                
            }
            catch (Exception ex)
            {
                //var _ = _err.FnErrorLog(GetType().Name, "Call", ex.Message, _req.LoginID);
            }
            dashboard.Chart = DashChart;
            dashboard.Users = dash;
            return dashboard;
        }

        public string GetName()
        {
            return "proc_middleUserDashboard";
        }
    }
}
