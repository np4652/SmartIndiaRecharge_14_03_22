using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcAutoBillingUpdate : IProcedure
    {
        private readonly IDAL _dal;
        public ProcAutoBillingUpdate(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (AutoBillingModel)obj;
            var _resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            SqlParameter[] param = {
                new SqlParameter("@LoginID", req.LoginID),
                new SqlParameter("@LT", req.LT),
                new SqlParameter("@Browser", req.Browser),
                new SqlParameter("@IP", req.IP),
                new SqlParameter("@UserId", req.UserId),
                new SqlParameter("@Id", req.Id),
                new SqlParameter("@IsAutoBilling", req.IsAutoBilling),
                new SqlParameter("@FromFOSAB", req.FromFOSAB),
                new SqlParameter("@BalanceForAB", req.BalanceForAB == 0 ? 1000 : req.BalanceForAB),
                new SqlParameter("@AlertBalance", req.AlertBalance == 0 ? 1000 : req.AlertBalance),
                new SqlParameter("@MaxBillingCountAB", req.MaxBillingCountAB == 0 ? 0 : req.MaxBillingCountAB),
                new SqlParameter("@MaxCreditLimitAB", req.MaxCreditLimitAB == 0 ? 5000 : req.MaxCreditLimitAB),
                new SqlParameter("@MaxTransferLimitAB", req.MaxTransferLimitAB == 0 ? 300 : req.MaxTransferLimitAB),
                new SqlParameter("@userIdBulk", req.UserIdBulk == "" ? "" : req.UserIdBulk),
            };
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    _resp.Statuscode = Convert.ToInt32(dt.Rows[0][0]);
                    _resp.Msg = dt.Rows[0]["Msg"].ToString();
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
                    UserId = req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return _resp;
        }
        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_updateAutoBilling";
    }
}
