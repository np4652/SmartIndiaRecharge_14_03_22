using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcSavePGTransactionReqResp : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcSavePGTransactionReqResp(IDAL dal) => _dal = dal;
        public async Task<object> Call(object obj)
        {
            var req = (TransactionPGLog)obj;
            SqlParameter[] param = {
                new SqlParameter("@PGID",req.PGID),
                new SqlParameter("@TID",req.TID),
                new SqlParameter("@Log",req.Log??string.Empty),
                new SqlParameter("@RequestMode",req.RequestMode),
                new SqlParameter("@RequestIP",req.RequestIP??string.Empty),
                new SqlParameter("@Browser",req.Browser??string.Empty),
                new SqlParameter("@TransactionID",req.TransactionID??string.Empty),
                new SqlParameter("@Checksum",req.Checksum??string.Empty),
                new SqlParameter("@IsRequestGenerated",req.IsRequestGenerated),
                new SqlParameter("@Amount",req.Amount),
                new SqlParameter("@VendorID",req.VendorID??string.Empty)
            };
            try
            {
                await _dal.ExecuteProcedureAsync(GetName(), param);
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = req.PGID,
                    UserId = req.TID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
                return false;
            }
            return true;
        }

        public Task<object> Call()
        {
            throw new NotImplementedException();
        }

        public string GetName() => "proc_SavePGTransactionReqResp";
    }
}
