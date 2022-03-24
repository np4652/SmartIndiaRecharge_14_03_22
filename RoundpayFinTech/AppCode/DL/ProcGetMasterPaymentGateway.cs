using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetMasterPaymentGateway : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcGetMasterPaymentGateway(IDAL dal) => _dal = dal;
        public async Task<object> Call(object obj)
        {
            var req = (int)obj;
            SqlParameter[] param = {
                new SqlParameter("@ID", req)
            };
            var _alist = new List<MasterPaymentGateway>();
            try
            {
                var dt = await _dal.GetByProcedureAsync(GetName(), param).ConfigureAwait(false);
                foreach (DataRow row in dt.Rows)
                {
                    _alist.Add(new MasterPaymentGateway
                    {
                        ID = Convert.ToInt32(row["_ID"]),
                        Name = row["_Name"] == null ? "" : row["_Name"].ToString(),
                        URL = row["_URL"] == null ? "" : row["_URL"].ToString(),
                        StatusCheckURL = row["_StatusCheckURL"] == null ? "" : row["_StatusCheckURL"].ToString(),
                        EntryDate = row["_EntryDate"] == null ? "" : row["_EntryDate"].ToString(),
                        ModifyDate = row["_ModifyDate"] == null ? "" : row["_ModifyDate"].ToString(),
                        Code = row["_Code"] == null ? "" : row["_Code"].ToString(),
                        IsUPI = row["_IsUPI"] is DBNull ? false : Convert.ToBoolean(row["_IsUPI"]),
                        IsLive = row["_IsLive"] is DBNull ? false : Convert.ToBoolean(row["_IsLive"])
                    });
                }
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return _alist;
        }

        public Task<object> Call() => throw new NotImplementedException();

        public string GetName() => "Proc_GetMasterPaymentGateway";
    }
}


