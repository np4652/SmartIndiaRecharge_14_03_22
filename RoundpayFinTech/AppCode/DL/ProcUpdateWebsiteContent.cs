using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcUpdateWebsiteContent : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcUpdateWebsiteContent(IDAL dal) => _dal = dal;
        public async Task<object> Call(object obj)
        {
            CommonReq req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginId", req.LoginID),
                new SqlParameter("@WID", req.CommonInt),
                new SqlParameter("@Section", req.CommonStr),
                new SqlParameter("@Content", req.CommonStr2)
            };
            IResponseStatus _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            try
            {
                DataTable dt = await _dal.GetByProcedureAsync(GetName(), param);
                if (dt != null && dt.Rows.Count > 0)
                {
                    _res.Statuscode = Convert.ToInt16(dt.Rows[0][0].ToString());
                    _res.Msg = dt.Rows[0][1].ToString();
                }
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
            return _res;
        }
        public Task<object> Call() => throw new NotImplementedException();
        public string GetName() => "proc_UpdateWebsiteContent";
    }
}
