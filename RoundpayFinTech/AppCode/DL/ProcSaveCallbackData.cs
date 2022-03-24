using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcSaveCallbackData : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcSaveCallbackData(IDAL dal)
        {
            _dal = dal;
        }
        public async Task<object> Call(object obj)
        {
            bool IsIPExists = false;
            CallbackData _req = (CallbackData)obj;
            SqlParameter[] param = {
                new SqlParameter("@Content", _req.Content),
                new SqlParameter("@Method", _req.Method??string.Empty),
                new SqlParameter("@Scheme", _req.Scheme??string.Empty),
                new SqlParameter("@Path", _req.Path??string.Empty),
                new SqlParameter("@RequestIP", _req.RequestIP??string.Empty),
                new SqlParameter("@RequestBrowser", _req.RequestBrowser??string.Empty),
                new SqlParameter("@APIID",_req.APIID),
                new SqlParameter("@InActiveMode",_req.InActiveMode??true)
            };
            try
            {
                DataTable dt = await _dal.GetByProcedureAsync(GetName(), param).ConfigureAwait(false);
                if (dt.Rows.Count > 0)
                {
                    IsIPExists = Convert.ToBoolean(dt.Rows[0]["IsIPExists"] is DBNull ? IsIPExists : dt.Rows[0]["IsIPExists"]);
                }
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = 1,
                    UserId = _req.APIID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return IsIPExists;
        }

        public Task<object> Call()
        {
            throw new NotImplementedException();
        }

        public string GetName() => "proc_SaveCallbackData";
        public void SavePaymentGatewayLog(PGCallbackData pGCallbackData)
        {
            SqlParameter[] param = {
                new SqlParameter("@RequestMehtod", pGCallbackData.RequestMehtod),
                new SqlParameter("@CallbackData", pGCallbackData.CallbackData),
                new SqlParameter("@PGID", pGCallbackData.PGID)
            };

            try
            {
                _dal.Execute("insert into Log_PGCallbackData (_RequestMethod,_CallbackData,_PGID,_EntryDate) values(@RequestMehtod,@CallbackData,@PGID,getdate())");
            }
            catch (Exception ex)
            {
            }
        }
    }
}
