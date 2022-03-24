using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcBlockAccount : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcBlockAccount(IDAL dal) => _dal = dal;
        public async Task<object> Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@UserID",req.UserID),
                new SqlParameter("@IMEI",req.CommonStr??""),
                new SqlParameter("@IP",req.CommonStr2),
                new SqlParameter("@Browser",req.CommonStr3)
            };
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.One,
                Msg = string.Empty
            };

            try
            {
                var dt = await _dal.GetByProcedureAsync(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    res.Statuscode = dt.Rows[0][0] is DBNull ? 1 : Convert.ToInt16(dt.Rows[0][0]);
                    res.Msg = dt.Rows[0]["Msg"] is DBNull ? "" : dt.Rows[0]["Msg"].ToString();
                }
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = req.LoginTypeID,
                    UserId = req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }

        public async Task<object> Call() => throw new NotImplementedException();

        public string GetName() => "proc_BlockAccount";
    }
}
