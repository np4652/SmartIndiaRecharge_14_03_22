using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.Model.Reports;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.Model.Reports.Filter;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcCustomerCareActivityReport : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcCustomerCareActivityReport(IDAL dal)
        {
            _dal = dal;
        }
        public async Task<object> Call(object obj)
        {
            ForDateFilter _req = (ForDateFilter)obj;
            SqlParameter[] param = new SqlParameter[5];
            param[0] = new SqlParameter("@LoginID", _req.LoginID);
            param[1] = new SqlParameter("@MobileNo", _req.UserMob??"");
            param[2] = new SqlParameter("@FromDate", _req.FromDate??"");
            param[3] = new SqlParameter("@ToDate", _req.ToDate??"");
            param[4] = new SqlParameter("@OperationID", _req.EventID);
            List<CustomerCareActivity> _res = new List<CustomerCareActivity>();
            try
            {
                DataTable dt = await _dal.GetByProcedureAsync(GetName(), param);
                if (dt.Rows.Count > 0 && !dt.Columns.Contains("Msg"))
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        CustomerCareActivity _activity = new CustomerCareActivity
                        {
                            ID = row["_ID"] is DBNull?0:Convert.ToInt32(row["_ID"]),
                            Customercare = row["Costomercare"] is DBNull ? "" : row["Costomercare"].ToString(),
                            Designation = row["Designation"] is DBNull ? "" : row["Designation"].ToString(),
                            MobileNo = row["_MobileNo"] is DBNull ? "" : row["_MobileNo"].ToString(),
                            Activity = row["_Activity"] is DBNull ? "" : row["_Activity"].ToString(),
                            OperationName = row["_OperationName"] is DBNull ? "" : row["_OperationName"].ToString(),
                            IP = row["_IP"] is DBNull ? "" : row["_IP"].ToString(),
                            Browser = row["_Browser"] is DBNull ? "" : row["_Browser"].ToString(),
                            EntryDate = row["_EntryDate"] is DBNull ? "" : row["_EntryDate"].ToString()
                        };
                        _res.Add(_activity);
                    }
                }
            }
            catch (Exception ex)
            {
                //var _ = _err.FnErrorLog(GetType().Name, "Call", ex.Message, _req.LoginID);
            }
            return _res;
        }

        public Task<object> Call()
        {
            throw new NotImplementedException();
        }

        public string GetName()
        {
            return "proc_CustomerCareActivityReport";
        }
    }
}