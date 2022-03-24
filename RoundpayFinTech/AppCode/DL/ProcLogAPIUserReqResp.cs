using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model.Recharge;
using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcLogAPIUserReqResp : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcLogAPIUserReqResp(IDAL dal) => _dal = dal;
        public async Task<object> Call(object obj)
        {
            var req = (APIReqResp)obj;
            SqlParameter[] param = {
                new SqlParameter("@UserID",req.UserID),
                new SqlParameter("@Request",req.Request??""),
                new SqlParameter("@Response",req.Response??""),
                new SqlParameter("@Method",req.Method??"")
            };
            try
            {
                await _dal.ExecuteProcedureAsync(GetName(),param).ConfigureAwait(false);
            }
            catch{}
            return true;
        }

        public Task<object> Call() => throw new NotImplementedException();

        public string GetName() => "proc_Log_APIUserReqResp";
        public async Task<object> SaveDMRAPIUserLog(APIReqResp aPIReqResp)
        {
            SqlParameter[] param = {
                new SqlParameter("@Method",aPIReqResp.Method??string.Empty),
                new SqlParameter("@Request",aPIReqResp.Request??string.Empty),
                new SqlParameter("@Response",aPIReqResp.Response??string.Empty),
                new SqlParameter("@RequestIP",aPIReqResp.RequestIP??string.Empty)
            };
            try
            {
                await _dal.ExecuteAsync("insert into Log_DMRAPIUserReqResp(_Method,_Request, _Response, _EntryDate, _RequestIP)values(@Method,@Request, @Response, GETDATE(), @RequestIP)", param);
            }
            catch (Exception ex)
            {
            }
            return true;
        }
    }
}
