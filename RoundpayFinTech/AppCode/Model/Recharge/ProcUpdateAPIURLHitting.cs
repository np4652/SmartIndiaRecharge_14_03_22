using Fintech.AppCode.Model;
using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcUpdateAPIURLHitting : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcUpdateAPIURLHitting(IDAL dal)
        {
            _dal = dal;
        }
        public async Task<object> Call(object obj)
        {
            APIURLHitting _req = (APIURLHitting)obj;
            SqlParameter[] param = {
                new SqlParameter("@UserID", _req.UserID),
                new SqlParameter("@TransactionID", _req.TransactionID??string.Empty),
                new SqlParameter("@URL", _req.URL??string.Empty),
                new SqlParameter("@Response",string.IsNullOrEmpty(_req.Response)?string.Empty:( _req.Response.Length > 200 ? _req.Response.Substring(0, 200) : _req.Response))
            };

            try
            {
                DataTable dt = await _dal.GetByProcedureAsync(GetName(), param).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                ErrorLog errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = 1,
                    UserId = _req.UserID
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

        public string GetName()
        {
            return "proc_Update_APIURLHitting";
        }
    }
}
