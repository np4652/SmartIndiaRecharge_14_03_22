using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
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
    public class ProcgetUploadedUTRFiles : IProcedureAsync
    {
        private IDAL _dal;

        public ProcgetUploadedUTRFiles(IDAL dal) => _dal = dal;
        public Task<object> Call(object obj) => throw new NotImplementedException();

        public async Task<object> Call()
        {
            var resp = new List<UTRExcelMaster>();
            try
            {
                var dt = await _dal.GetAsync(GetName()).ConfigureAwait(true);
                if (dt != null && dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        var data = new UTRExcelMaster
                        {
                            FileId = row["_Id"] is DBNull ? 0 : Convert.ToInt32(row["_Id"]),
                            FileName = row["_FileName"] is DBNull ? string.Empty : Convert.ToString(row["_FileName"]),
                            EntryDate = row["_EnrtyDate"] is DBNull ? string.Empty : Convert.ToString(row["_EnrtyDate"]),
                            IsJobDone = row["_IsJobDone"] is DBNull ? false: Convert.ToBoolean(row["_IsJobDone"]),
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
        public string GetName() => @"select _ID,_FileName,convert(varchar,_EnrtyDate,106) as _EnrtyDate ,_IsJobDone from [dbo].[tbl_UTRExcelMaster](nolock)";
    }
}
