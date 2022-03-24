using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.StaticModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcLapuTransaction : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcLapuTransaction(IDAL dal) => _dal = dal;
        public async Task<object> Call(object obj)
        {
            string LapuName = (string)obj;
            SqlParameter[] param = new SqlParameter[1];
            param[0] = new SqlParameter("@LapuName", LapuName ?? "SRS");
            var _res = new List<LastTransaction.Status>();
            try
            {
                DataTable dt = await _dal.GetByProcedureAsync(GetName(), param);
                foreach (DataRow row in dt.Rows)
                {
                    var status = new LastTransaction.Status
                    {
                        Mobile = row["_Account"] is DBNull ? "" : row["_Account"].ToString(),
                        Amount = row["_RequestedAmount"] is DBNull ? 0 : Convert.ToDecimal(row["_RequestedAmount"]),
                        ApiName = row["_APIName"] is DBNull ? "" : row["_APIName"].ToString(),
                        Operator = row["_Operator"] is DBNull ? "" : row["_Operator"].ToString(),
                        Code = row["_APIOpCode"] is DBNull ? "" : row["_APIOpCode"].ToString(),
                        RequestId = row["_TID"] is DBNull ? "" : row["_TID"].ToString(),
                        Date = (row["_EntryDate"] is DBNull ? DateTime.Now : Convert.ToDateTime(row["_EntryDate"].ToString())).ToString("dd-MMM-yyyy hh:mm:ss"),
                    };
                    _res.Add(status);
                }
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = 1,
                    UserId = 1
                });
            }
            return _res;
        }

        public Task<object> Call() => throw new NotImplementedException();

        public string GetName() => "proc_LapuTransaction";
    }
}
