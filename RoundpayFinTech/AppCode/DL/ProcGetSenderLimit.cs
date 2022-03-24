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
    public class ProcGetSenderLimit : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcGetSenderLimit(IDAL dal) => _dal = dal;
        public async Task<object> Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
            new SqlParameter("@SenderID",req.CommonInt),
            new SqlParameter("@APIID",req.CommonInt2)
            };
            var res = new SenderLimitModel();
            try
            {
                var dt = await _dal.GetByProcedureAsync(GetName(), param).ConfigureAwait(false);
                if (dt.Rows.Count > 0)
                {
                    res.LimitUsed = dt.Rows[0]["_LimitUsed"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[0]["_LimitUsed"]);
                    res.SenderLimit= dt.Rows[0]["_SenderLimit"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_SenderLimit"]);
                    //res.SenderName= dt.Rows[0]["SenderName"] is DBNull ? "" : dt.Rows[0]["_SenderLimit"].ToString();
                }
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = req.CommonInt
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }

        public Task<object> Call()
        {
            throw new NotImplementedException();
        }

        public string GetName() => "proc_GetSenderLimit";
    }
}
