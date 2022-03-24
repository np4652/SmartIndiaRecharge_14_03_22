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
    public class ProcLeadSummary : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcLeadSummary(IDAL dal) => _dal = dal;
        public string GetName() => "proc_LeadSummary";
        public async Task<object> Call(object obj)
        {
            CommonReq req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@CustomerID", req.CommonInt)
            };
            

            var res = new LeadSummary();            
            try
            {
                DataTable dt = await _dal.GetByProcedureAsync(GetName(), param);
                if (dt.Rows.Count > 0) {
                    
                        res.TotalMaturedNo = dt.Rows[0]["_Matured"] is DBNull ? 0 : Convert.ToInt64(dt.Rows[0]["_Matured"]);
                        res.TotalRequestNo = dt.Rows[0]["_Request"] is DBNull ? 0 : Convert.ToInt64(dt.Rows[0]["_Request"]);
                        res.TotalFollowUPNo = dt.Rows[0]["_FollowUP"] is DBNull ? 0 : Convert.ToInt64(dt.Rows[0]["_FollowUP"]);
                        res.TotalTransferNo = dt.Rows[0]["_Transfer"] is DBNull ? 0 : Convert.ToInt64(dt.Rows[0]["_Transfer"]);
                        res.TotalJunkNo = dt.Rows[0]["_Junk"] is DBNull ? 0 : Convert.ToInt64(dt.Rows[0]["_Junk"]);
                }
            }
            catch (Exception er)
            { }
            return res;
        }

        public Task<object> Call() => throw new NotImplementedException();
    }
}
