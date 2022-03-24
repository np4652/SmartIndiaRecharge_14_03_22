using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.Model.Reports.Filter;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class GetOpTypeByService : IProcedureAsync
    {
        private readonly IDAL _dal;
        public GetOpTypeByService(IDAL dal) => _dal = dal;

        public async Task<object> Call(object obj)
        {
            var req = (_RechargeReportFilter)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT", req.LT)
            };
            var res = new List<OpTypeMaster>();
            try
            {
                DataTable dt = await _dal.GetByProcedureAsync(GetName(), param).ConfigureAwait(false);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        res.Add(new OpTypeMaster
                        {
                            ID = Convert.ToInt32(dr["_ID"]),
                            OpType = Convert.ToString(dr["_OpType"]),                            
                            ServiceTypeID = Convert.ToInt32(dr["_ServiceTypeID"]),
                            SCode = Convert.ToString(dr["_SCode"]),                            
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = 1,
                    UserId = 1
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }

            return res;
        }

        public Task<object> Call()
        {
            throw new NotImplementedException();
        }

        public string GetName() => "select s._SCode,o.* from Master_Optype o , Master_Service s where o._ServiceTypeID=s._ID";
        
    }
}
