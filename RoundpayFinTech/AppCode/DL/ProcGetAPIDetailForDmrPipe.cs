using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetAPIDetailForDmrPipe : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcGetAPIDetailForDmrPipe(IDAL dal) => _dal = dal;
        public async Task<object> Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
            new SqlParameter("@LoginID",req.LoginID),
            new SqlParameter("@OID",req.CommonInt),
            new SqlParameter("@OPID",req.CommonInt2)
            };
            var res = new APIDetailForDMTPipe
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError,
                ErrorCode = ErrorCodes.Unknown_Error
            };
            try
            {
                DataTable dt = await _dal.GetByProcedureAsync(GetName(), param).ConfigureAwait(false);
                if (dt.Rows.Count > 0)
                {
                    res.Statuscode = dt.Rows[0][0] is DBNull ? res.Statuscode : Convert.ToInt16(dt.Rows[0][0]);
                    res.Msg = dt.Rows[0]["Msg"] is DBNull ? string.Empty : dt.Rows[0]["Msg"].ToString();
                    res.ErrorCode = dt.Rows[0]["ErrorCode"] is DBNull ? res.Statuscode : Convert.ToInt32(dt.Rows[0]["ErrorCode"]);
                    if (res.Statuscode == ErrorCodes.One)
                    {
                        res.APIs = new List<APIDetail>();
                        foreach (DataRow item in dt.Rows)
                        {
                            res.APIs.Add(new APIDetail
                            {
                                ID = item["_APIID"] is DBNull ? 0 : Convert.ToInt32(item["_APIID"]),
                                APICode = item["_APICode"] is DBNull ? string.Empty : item["_APICode"].ToString(),
                                Name = item["_Name"] is DBNull ? string.Empty : item["_Name"].ToString(),
                                MaxLimitPerTransaction = item["_MaxLimitPerTransaction"] is DBNull ? 0 : Convert.ToInt32(item["_MaxLimitPerTransaction"]),
                                APIType = item["_APIType"] is DBNull ? 0 : Convert.ToInt16(item["_APIType"])
                            });
                        }
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
                    UserId = req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }

        public Task<object> Call()
        {
            throw new NotImplementedException();
        }

        public string GetName() => "proc_GetAPIDetailForDmrPipe";
    }
}
