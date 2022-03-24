using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetOpListBySerivces : IProcedureAsync
    {
        private readonly IDAL _dal;

        public ProcGetOpListBySerivces(IDAL dal) => _dal = dal;
        public async Task<object> Call(object obj)
        {
            string ServiceTypeIDs = (string)obj;

            SqlParameter[] param = new SqlParameter[1];
            param[0] = new SqlParameter("@ServiceTypeIDs", ServiceTypeIDs);

            var _res = new List<OperatorDetail>();
            try
            {
                DataTable dt = await _dal.GetByProcedureAsync(GetName(), param).ConfigureAwait(false);
                if (dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        var operatorMaster = new OperatorDetail
                        {
                            OID = Convert.ToInt32(dt.Rows[i]["ID"]),
                            Name = dt.Rows[i]["Name"].ToString(),                            
                            AllowedChannel = dt.Rows[i]["_AllowedChannel"] is DBNull?3: Convert.ToInt16(dt.Rows[i]["_AllowedChannel"])                          
                        };
                        _res.Add(operatorMaster);
                    }
                }

            }
            catch (Exception ex)
            {
               // var _ = _err.FnErrorLog(GetType().Name, "Call", ex.Message, 1000);
            }
            return _res;
        }

        public Task<object> Call() => throw new NotImplementedException();
        public string GetName() => "proc_GetOpListBySerivces";
    }
}