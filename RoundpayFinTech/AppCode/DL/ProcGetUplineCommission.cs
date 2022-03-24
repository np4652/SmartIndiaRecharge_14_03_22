using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetUplineCommission : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcGetUplineCommission(IDAL dal) {
            _dal = dal;
        }
        public async Task<object> Call(object obj)
        {
            CommonReq req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT",req.LoginTypeID),
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@TID",req.CommonInt)
            };
            List<ProcRechargeReportResponse> _alist = new List<ProcRechargeReportResponse>();
            try
            {
                DataTable dt = await _dal.GetByProcedureAsync(GetName(),param);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        var procRechargeReportResponse = new ProcRechargeReportResponse
                        {
                            Role = row["_Role"] is DBNull ? "" : row["_Role"].ToString(),
                            Outlet =row["_Outletname"] is DBNull?"": row["_Outletname"].ToString(),
                            OutletNo = row["_MobileNo"] is DBNull ? "" : row["_MobileNo"].ToString(),
                            CommType = row["_CommType"] is DBNull ? false : Convert.ToBoolean(row["_CommType"]),
                            Commission = row["_Comm"] is DBNull ? 0M : Convert.ToDecimal(row["_Comm"]),
                            UserID = row["_UserID"] is DBNull ? 0 : Convert.ToInt32(row["_UserID"]),
                            Prefix = row["_Prefix"] is DBNull ? "" : row["_Prefix"].ToString(),
                            GSTAmount= row["_Comm"] is DBNull ? 0M : Convert.ToDecimal(row["GSTTaxAmount"]),
                            TDSAmount = row["_Comm"] is DBNull ? 0M : Convert.ToDecimal(row["TDSAmount"])
                        };
                        _alist.Add(procRechargeReportResponse);
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return _alist;
        }

        public Task<object> Call()
        {
            throw new NotImplementedException();
        }

        public string GetName()
        {
            return "proc_GetUplineCommission";
        }
    }
}
