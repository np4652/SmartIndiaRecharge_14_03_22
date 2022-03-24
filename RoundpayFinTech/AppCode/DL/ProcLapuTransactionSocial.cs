using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.StaticModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcLapuTransactionSocial : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcLapuTransactionSocial(IDAL dal)=>_dal=dal;
        public async Task<object> Call(object obj)
        {
            CommonReq _req = (CommonReq)obj;
            SqlParameter[] param = { 
                new SqlParameter("@LapuName",_req.CommonStr??string.Empty),
                new SqlParameter("@SocialAlertType",_req.CommonInt)
            };
            var _res = new List<LastTransactionSMS.Status>();
            try
            {
                DataTable dt = await _dal.GetByProcedureAsync(GetName(), param).ConfigureAwait(false);
                foreach (DataRow row in dt.Rows)
                {
                    _res.Add(new LastTransactionSMS.Status
                    {
                        Mobile = row["_SendTo"] is DBNull ? "" : row["_SendTo"].ToString(),
                        Message = row["_Message"] is DBNull ? "" : row["_Message"].ToString(),
                        TransactionID = row["_TransactionID"] is DBNull ? "" : row["_TransactionID"].ToString()
                    });
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

        public Task<object> Call()=>throw new NotImplementedException();

        public string GetName() => "proc_LapuTransactionSocial";
    }
}
