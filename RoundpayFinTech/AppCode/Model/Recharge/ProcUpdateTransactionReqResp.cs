using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcUpdateTransactionReqResp : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcUpdateTransactionReqResp(IDAL dal) => _dal = dal;
        public async Task<object> Call(object obj)
        {
            var _req = (TransactionReqResp)obj;
            SqlParameter[] param = {
                new SqlParameter("@TID",_req.TID),
                new SqlParameter("@APIID", _req.APIID),
                new SqlParameter("@Request", _req.Request ?? string.Empty),
                new SqlParameter("@Response", _req.Response ?? string.Empty),
                new SqlParameter("@APIOpCode", _req.APIOpCode ?? string.Empty),
                new SqlParameter("@APIName", _req.APIName ?? string.Empty),
                new SqlParameter("@APICommType", _req.APICommType),
                new SqlParameter("@APIComAmt", _req.APIComAmt)
            };
            try
            {
                var dt = await _dal.GetByProcedureAsync(GetName(), param).ConfigureAwait(false);
                return true;
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call"+_dal.GetDBNameFromIntialCatelog(),
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = _req.APIID
                });
            }
            return false;
        }
        public Task<object> Call() => throw new NotImplementedException();
        public string GetName() => "[dbo].[proc_UpdateTransactionReqResp]";
    }
}
