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
    public class ProcEnable_2FA_Credentials : IProcedureAsync
    {
        private readonly IDAL _dal;

        public ProcEnable_2FA_Credentials(IDAL dal) => _dal = dal;
        public async Task<object> Call(object obj)
        {
            IResponseStatus res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            CommonReq req = (CommonReq)obj;
            if (req == null)
            {
                goto Finish;
            }
            SqlParameter[] param = {
                new SqlParameter("@UserId", req.UserID),
                new SqlParameter("@AccountSecretKey", req.CommonStr),
                new SqlParameter("@IsGoogle2FAEnable", req.CommonBool),
                new SqlParameter("@Action", req.CommonBool1),
            };

            try
            {
                DataTable dt = await _dal.GetByProcedureAsync(GetName(), param);
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
        Finish:
            return res;
        }

        Task<object> IProcedureAsync.Call() => throw new NotImplementedException();

        public string GetName() => "proc_Enable_2FA_Credentials";
    }
}
