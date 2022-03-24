using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcCalaculateCommission : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcCalaculateCommission(IDAL dal) => _dal = dal;
        public async Task<object> Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@UserID",req.LoginID),
                new SqlParameter("@OID",req.CommonInt),
                new SqlParameter("@Amount",req.CommonDecimal)
            };
            var res = new CommissionDisplay();
            try
            {
                var dt = await _dal.GetByProcedureAsync(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    if (!dt.Columns.Contains("Msg"))
                    {
                        res.Commission = dt.Rows[0]["_CommAmount"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[0]["_CommAmount"]);
                        res.CommType = dt.Rows[0]["_CommType"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_CommType"]);
                        res.RCommission = dt.Rows[0]["_RCommission"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[0]["_RCommission"]);
                        res.RCommType = dt.Rows[0]["_RealCommType"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_RealCommType"]);
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

        public string GetName() => "proc_CalaculateCommission";
    }
}
