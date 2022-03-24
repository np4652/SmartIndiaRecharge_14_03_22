using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.Model.Reports;
using RoundpayFinTech.AppCode.Model.Report;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetNumberList : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetNumberList(IDAL dal) {
            _dal = dal;
        }
        public object Call(object obj)
        {
            CommonReq req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT",req.LoginTypeID),
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@OID", req.CommonInt)
        };            
            
            var res = new NumberSeriesListWithCircle();
            try
            {
                DataTable dt = _dal.GetByProcedureAdapter(GetName(), param);
                var numberSeries = new List<NumberSeries>();
                if (dt.Rows.Count > 0) {
                    foreach (DataRow row in dt.Rows)
                    {
                        var series = new NumberSeries
                        {
                            CircleID = row["CircleID"] is DBNull?0: Convert.ToInt16(row["CircleID"]),
                            Circle = row["Circle"] is DBNull ? "": row["Circle"].ToString(),
                            Number = row["Number"] is DBNull ? "" : row["Number"].ToString(),
                            OID = row["OID"] is DBNull ? 0 : Convert.ToInt16(row["OID"])
                        };
                        numberSeries.Add(series);
                    }
                    res.SeriesList = numberSeries;
                    List<CirlceMaster> circles = numberSeries.GroupBy(x => new { x.CircleID, x.Circle }).Select(g => new CirlceMaster { ID = g.Key.CircleID, Circle = g.Key.Circle }).ToList();
                    res.CircleList = circles;
                }
            }
            catch (Exception)
            {
            }

            return res;
        }

        public object Call()
        {
            throw new NotImplementedException();
        }

        public string GetName()
        {
            return "proc_GetNumberList";
        }
    }
}
