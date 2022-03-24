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
    public class ProcAutoBilling : IProcedure
    {
        private readonly IDAL _dal;
        public ProcAutoBilling(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            int id = (int)obj;
            var _resp = new AutoBillingModel
            {
                StatusCode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            try
            {
                string query = "select 1, 'Success' Msg, _ID,_UserID, isnull(_IsAutoBilling,0)_IsAutoBilling, isnull(_MaxBillingPerDay,0)_MaxBillingPerDay, isnull(_BalanceForAutoBilling,0)_BalanceForAutoBilling,isnull(_AlertBalance,0)_AlertBalance,isnull(_IsAutoBillingFromFOS,0)_IsAutoBillingFromFOS, isnull(_MaxCreditLimitAB,0)_MaxCreditLimitAB, isnull(_MaxTransferLimitAB,0)_MaxTransferLimitAB from tbl_users_balance(nolock) where _UserID =" + id.ToString();
                var dt = _dal.Get(query);
                if (dt.Rows.Count > 0)
                {
                    _resp.StatusCode = Convert.ToInt32(dt.Rows[0][0]);
                    _resp.Msg = dt.Rows[0]["Msg"].ToString();
                    _resp.Id = Convert.ToInt32(dt.Rows[0]["_ID"]);
                    _resp.UserId = Convert.ToInt32(dt.Rows[0]["_UserID"]);
                    _resp.IsAutoBilling = dt.Rows[0]["_IsAutoBilling"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsAutoBilling"]);
                    _resp.FromFOSAB = dt.Rows[0]["_IsAutoBillingFromFOS"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsAutoBillingFromFOS"]);
                    _resp.BalanceForAB = Convert.ToInt32(dt.Rows[0]["_BalanceForAutoBilling"]);
                    _resp.AlertBalance = Convert.ToInt32(dt.Rows[0]["_AlertBalance"]);
                    _resp.MaxBillingCountAB = Convert.ToInt32(dt.Rows[0]["_MaxBillingPerDay"]);
                    _resp.MaxCreditLimitAB = Convert.ToInt32(dt.Rows[0]["_MaxCreditLimitAB"]);
                    _resp.MaxTransferLimitAB = Convert.ToInt32(dt.Rows[0]["_MaxTransferLimitAB"]);
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
            return _resp;
        }
        public object Call() => throw new NotImplementedException();
        public string GetName() => "";
    }
}

   
