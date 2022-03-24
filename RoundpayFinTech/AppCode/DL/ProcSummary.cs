using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.Model.Report;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcSummary : IProcedure
    {
        private readonly IDAL _dal;
        public ProcSummary(IDAL dal)
        {
            _dal = dal;
        }
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@LoginTypeID",req.LoginTypeID)
            };
            var resp = new DealerSummary();
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    resp.TotalUsers = dt.Rows[0]["TotalUsers"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["TotalUsers"].ToString());
                    resp.ActiveUsers = dt.Rows[0]["ActiveUsers"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["ActiveUsers"].ToString());
                    resp.BalWise1 = dt.Rows[0]["BalWise1"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["BalWise1"].ToString());
                    resp.BalWise2 = dt.Rows[0]["BalWise2"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["BalWise2"].ToString());
                }
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = req.LoginTypeID,
                    UserId = req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return resp;
        }

        public object Call()
        {
            throw new NotImplementedException();
        }

        public string GetName()
        {
            return "proc_summary";
        }
    }
    public class ProcBCAgentSummary : IProcedure
    {
        private readonly IDAL _dal;
        public ProcBCAgentSummary(IDAL dal)
        {
            _dal = dal;
        }
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@LoginTypeID",req.LoginTypeID)
            };
            var resp = new BCAgentSummary();
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    resp.Total = dt.Rows[0]["Total"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["Total"].ToString());
                    resp.Approve = dt.Rows[0]["Approve"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["Approve"].ToString());
                    resp.Reject = dt.Rows[0]["Reject"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["Reject"].ToString());
                    resp.Pending = dt.Rows[0]["Pending"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["Pending"].ToString());
                }
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = req.LoginTypeID,
                    UserId = req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return resp;
        }

        public object Call()
        {
            throw new NotImplementedException();
        }

        public string GetName()
        {
            return "proc_BCAgentSummary";
        }
    }
}
