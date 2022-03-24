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
    public class ProcLogUserActivity : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcLogUserActivity(IDAL dal)
        {
            _dal = dal;
        }
        public async Task<object> Call(object obj)
        {
            ForDateFilter _req = (ForDateFilter)obj;
            SqlParameter[] param = new SqlParameter[4];
            param[0] = new SqlParameter("@LoginID", _req.LoginID);
            param[1] = new SqlParameter("@MobileNo", _req.UserMob??string.Empty);
            param[2] = new SqlParameter("@FromDate", _req.FromDate??"");
            param[3] = new SqlParameter("@ToDate", _req.ToDate??"");
            List<UserActivityLog> _res = new List<UserActivityLog>();
            try
            {
                DataTable dt = await _dal.GetByProcedureAsync(GetName(), param);
                if (dt.Rows.Count > 0 && !dt.Columns.Contains("Msg"))
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        UserActivityLog _activity = new UserActivityLog
                        {
                            
                            Name = row["Name"] is DBNull ? "" : row["Name"].ToString(),
                            Action = row["Action"] is DBNull ? "" : row["Action"].ToString(),
                            MobileNo = row["Mobile"] is DBNull ? "" : row["Mobile"].ToString(),
                            EntryDate = row["Date"] is DBNull ? "" : row["Date"].ToString()
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
            return "proc_Log_UserActivity";
        }
    }
}