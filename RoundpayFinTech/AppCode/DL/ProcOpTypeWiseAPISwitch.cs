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
    public class ProcOpTypeWiseAPISwitch : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcOpTypeWiseAPISwitch(IDAL dal) => _dal = dal;
        public async Task<object> Call(object obj)
        {
            OpTypeWiseAPISwitchingReq req = (OpTypeWiseAPISwitchingReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT", req.LT),
                new SqlParameter("@LoginID", req.LoginID),
                new SqlParameter("@OPID", req.OpTypeId),
                new SqlParameter("@APIID", req.APIID),
                new SqlParameter("@MaxCount", req.MaxCount),
                new SqlParameter("@IsActive", req.IsActive),
                new SqlParameter("@IP", req.IP??string.Empty),
                new SqlParameter("@Browser", req.Browser??string.Empty),
                new SqlParameter("@FailoverCount", req.FailoverCount)
            };
            var resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            try
            {
                DataTable dt = await _dal.GetByProcedureAsync(GetName(), param).ConfigureAwait(true);
                if (dt.Rows.Count > 0)
                {
                    resp.Statuscode = Convert.ToInt32(dt.Rows[0][0]);
                    resp.Msg = dt.Rows[0]["Msg"].ToString();
                }
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = req.LT,
                    UserId = req.LoginID
                });
            }
            return resp;
        }

        public Task<object> Call() => throw new NotImplementedException();

        public string GetName() => "Proc_OpType_wise_APISwitch";
    }
}
