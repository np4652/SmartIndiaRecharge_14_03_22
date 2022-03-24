using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetAllAPI : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcGetAllAPI(IDAL dal) => _dal = dal;
        public async Task<object> Call(object obj)
        {
            var response = new List<APIDetail>();
            var opTypeId = (int)obj;
            SqlParameter[] param = {
                new SqlParameter("@opTypeId",opTypeId)
            };
            try
            {
                DataTable dt = await _dal.GetAsync(GetName(), param).ConfigureAwait(true);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        response.Add(new APIDetail
                        {
                            ID = row["_ID"] is DBNull ? 0 : Convert.ToInt32(row["_ID"]),
                            Name = row["_Name"] is DBNull ? string.Empty : row["_Name"].ToString(),
                            APIType = row["_APIType"] is DBNull ? 0 : Convert.ToInt32(row["_APIType"]),
                            APICode = row["_APICode"] is DBNull ? string.Empty : row["_APICode"].ToString(),
                            GroupID = row["_GroupID"] is DBNull ? 0 : Convert.ToInt32(row["_GroupID"]),
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = 1
                });
            }
            return response ?? new List<APIDetail>();
        }

        public Task<object> Call() => throw new NotImplementedException();

        public string GetName() => @"Select _ID,_Name,_APIType,_APICode,_GroupID From tbl_API(nolock)";
    }
}
