using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcLogAEPSCallbackReqRes : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcLogAEPSCallbackReqRes(IDAL dal) => _dal = dal;
        public async Task<object> Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@APICode",req.str),
                new SqlParameter("@Method",req.CommonStr??string.Empty),
                new SqlParameter("@Req",req.CommonStr2??string.Empty),
                new SqlParameter("@Resp",req.CommonStr3??string.Empty)
            };
            try
            {
                await _dal.ExecuteAsync(GetName(),param).ConfigureAwait(false);
                return true;
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
            return false;
        }

        public Task<object> Call()
        {
            throw new NotImplementedException();
        }

        public string GetName() => "insert into Log_AEPSCallbackReqRes(_APICode, _Method, _Req, _Resp, _EntryDate)values(@APICode, @Method, @Req, @Resp, getdate())";
    }
}
