using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetAPICircleCode : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcGetAPICircleCode(IDAL dal) => _dal = dal;

        public Task<object> Call(object obj) => throw new NotImplementedException();

        public async Task<object> Call()
        {
            var res = new List<APICircleCode>();
            try
            {
                DataTable dt = await _dal.GetByProcedureAsync(GetName());
                foreach (DataRow row in dt.Rows)
                {
                    res.Add(new APICircleCode
                    {
                        CircleID = row["_CircleID"] is DBNull ? (short)0 : Convert.ToInt32(row["_CircleID"]),
                        APIID = row["_APIID"] is DBNull ? 0 : Convert.ToInt32(row["_APIID"]),
                        APIType = row["_APIType"] is DBNull ? 0 : Convert.ToInt32(row["_APIType"]),
                        APIName = Convert.ToString(row["_APIName"]),
                        APIcirclecode = Convert.ToString(row["_APIcirclecode"])
                    });
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

        public string GetName() => "Proc_GetAPICircleCode";
    }
}
