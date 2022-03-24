using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetCommissionForOperator : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcGetCommissionForOperator(IDAL dal) => _dal = dal;

        public async Task<object> Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@UserID",req.LoginID),
                new SqlParameter("@OID",req.CommonInt)
            };
            var res = new SlabCommission();
            try
            {
                var dt = await _dal.GetByProcedureAsync(GetName(), param);
                if (dt.Rows.Count > 0) {
                    if (!dt.Columns.Contains("Msg"))
                    {
                        res.Comm = dt.Rows[0]["_Comm"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[0]["_Comm"]);
                        res.AmtType = dt.Rows[0]["_AmtType"] is DBNull ? 0 : Convert.ToInt16(dt.Rows[0]["_AmtType"]);
                        res.CommType = dt.Rows[0]["_CommType"] is DBNull ? 0 : Convert.ToInt16(dt.Rows[0]["_CommType"]);
                        res.RComm = dt.Rows[0]["_RComm"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[0]["_RComm"]);
                        res.RAmtType = dt.Rows[0]["_RAmtType"] is DBNull ? 0 : Convert.ToInt16(dt.Rows[0]["_RAmtType"]);
                        res.RCommType = dt.Rows[0]["_RCommType"] is DBNull ? 0 : Convert.ToInt16(dt.Rows[0]["_RCommType"]);
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return res;
        }

        public Task<object> Call()
        {
            throw new NotImplementedException();
        }

        public string GetName() => "proc_GetCommissionForOperator";
    }
}
