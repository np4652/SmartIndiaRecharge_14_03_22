using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcResetGoogleAuthenticator : IProcedureAsync
    {
        private readonly IDAL _dal;

        public ProcResetGoogleAuthenticator(IDAL dal) => _dal = dal;
        public async Task<object> Call(object obj)
        {
            IResponseStatus res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            int UserID = (int)obj;
            SqlParameter[] param = {
                new SqlParameter("@UserId", UserID)
            };
            try
            {
                DataTable dt = await _dal.GetAsync(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    res.Statuscode = dt.Rows[0]["Statuscode"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["Statuscode"]);
                    res.Msg = dt.Rows[0]["Msg"] is DBNull ? "" : dt.Rows[0]["Msg"].ToString();

                }
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = 1,
                    UserId = 1
                });
            }
            return res;
        }

        Task<object> IProcedureAsync.Call() => throw new NotImplementedException();

        public string GetName() => "Delete from tbl_TwoFactAuth_Credentials where _UserId=@UserId;select 1 StatusCode,'Success' Msg";
    }
}
