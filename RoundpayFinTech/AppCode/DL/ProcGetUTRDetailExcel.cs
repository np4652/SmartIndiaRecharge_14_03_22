using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetUTRDetailExcel : IProcedureAsync
    {
        private IDAL _dal;
        public ProcGetUTRDetailExcel(IDAL dal) => _dal = dal;
        public async Task<object> Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@FileId",req.CommonInt)
            };
            var resp = new List<UTRExcel>();
            try
            {
                var dt = await _dal.GetAsync(GetName(), param).ConfigureAwait(true);
                if (dt != null && dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        var data = new UTRExcel
                        {
                            FileId = row["_Id"] is DBNull ? 0 : Convert.ToInt32(row["_Id"]),
                            UserIdentity = row["_UserIdentity"] is DBNull ? string.Empty : Convert.ToString(row["_UserIdentity"]),
                            UTR = row["_UTR"] is DBNull ? string.Empty : Convert.ToString(row["_UTR"]),
                            Status = row["_Status"] is DBNull ? string.Empty : Convert.ToString(row["_Status"]),
                            Amount = row["_Amount"] is DBNull ? 0 : Convert.ToDecimal(row["_Amount"]),
                        };
                        resp.Add(data);
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorLog errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = 1,
                    UserId = 1
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return resp;
        }
        public Task<object> Call() => throw new NotImplementedException();
        public string GetName() => @"select * from [dbo].[tbl_UTRExcelDetail](nolock) where _FileId=@FileId";
    }
}
