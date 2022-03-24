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
    public class ProcUpdateAEPSTransactionService : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcUpdateAEPSTransactionService(IDAL dal) => _dal = dal;
        public async Task<object> Call(object obj)
        {
            var req = (_CallbackData)obj;
            req.Statuscode = ErrorCodes.Minus1;
            SqlParameter[] param = {
                new SqlParameter("@TID",req.TID),
                new SqlParameter("@VendorID",req.VendorID??string.Empty),
                new SqlParameter("@Type",req.TransactionStatus),
                new SqlParameter("@LiveID",req.LiveID??string.Empty),
                new SqlParameter("@APICode",req.APICode??string.Empty),
                new SqlParameter("@RequestPage",req.RequestPage??string.Empty),
                new SqlParameter("@IP",req.RequestIP??string.Empty),
                new SqlParameter("@Browser",req.Browser??string.Empty),
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@LT",req.LT),
                new SqlParameter("@TransactionID",req.TransactionID??string.Empty),
                new SqlParameter("@Req",req.Request??string.Empty),
                new SqlParameter("@Resp",req.Response??string.Empty)
            };
            try
            {
                var dt = await _dal.GetByProcedureAsync(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    req.Statuscode = Convert.ToInt32(dt.Rows[0][0]);
                    req.Msg = dt.Rows[0]["Msg"].ToString();
                }
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = req.LT,
                    UserId = req.UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return req;
        }

        public Task<object> Call() => throw new NotImplementedException();
        public string GetName() => "proc_UpdateAEPSTransactionService";
    }
}
