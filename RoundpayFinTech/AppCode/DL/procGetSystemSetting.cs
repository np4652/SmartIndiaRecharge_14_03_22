using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetSystemSetting : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetSystemSetting(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var _req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginID", _req.LoginID),
                new SqlParameter("@LT", _req.LoginTypeID)
            };
            var _resp = new SystemSetting
            {
                ResultCode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    _resp.ResultCode = Convert.ToInt32(dt.Rows[0][0]);
                    _resp.Msg = dt.Rows[0]["msg"].ToString();
                    _resp.IntervalFundTransfer= Convert.ToInt32(dt.Rows[0]["_IntervalFundTransfer"]);
                    _resp.MaxFundTransfer = Convert.ToInt32(dt.Rows[0]["_MaxFundTransfer"]);
                    _resp.MinFundTransfer = Convert.ToInt32(dt.Rows[0]["_MinFundTransfer"]);
                    _resp.IntervalRecharge = Convert.ToInt32(dt.Rows[0]["_IntervalRecharge"]);
                    _resp.AppVersion = Convert.ToString(dt.Rows[0]["_AppVersion"]);
                    _resp.WebAppVersion = Convert.ToString(dt.Rows[0]["_WebAppVersion"]);
                    _resp.IsLookUpFromAPI = dt.Rows[0]["_IsLookUpFromAPI"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsLookUpFromAPI"]);
                    _resp.AdminPasswordExpiry = dt.Rows[0]["_AdminPasswordExpiryDays"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_AdminPasswordExpiryDays"]);
                    _resp.PasswordExpiry = dt.Rows[0]["_PasswordExpiryDays"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_PasswordExpiryDays"]);
                    _resp.WrongLoginAttempt = dt.Rows[0]["_WrongLoginAttempt"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_WrongLoginAttempt"]);
                    _resp.IsDTHInfo = dt.Rows[0]["IsDTHInfo"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["IsDTHInfo"]);
                    _resp.IsRoffer = dt.Rows[0]["IsRoffer"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["IsRoffer"]);
                    _resp.BalanceAlertInterval = dt.Rows[0]["_BalanceAlertDuration"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_BalanceAlertDuration"]);
                    _resp.MoveToWalletInReal = dt.Rows[0]["_MoveToWalletInReal"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_MoveToWalletInReal"]);
                    _resp.IsSignupUserActive = dt.Rows[0]["_IsSignupUserActive"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsSignupUserActive"]);
                    _resp.IsPGActiveByAdmin = dt.Rows[0]["_IsPGActiveByAdmin"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsPGActiveByAdmin"]);
                    _resp.IsPGActiveByUpline = dt.Rows[0]["_IsPGActiveByUpline"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsPGActiveByUpline"]);
                    _resp.IsAutoBilling = dt.Rows[0]["_IsAutoBilling"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsAutoBilling"]);
                    _resp.MTRWRetail = dt.Rows[0]["_MTRWRetail"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_MTRWRetail"]);
                    _resp.FlatSignupCommission = dt.Rows[0]["_FlatSignupCommission"] is DBNull?0M: Convert.ToDecimal(dt.Rows[0]["_FlatSignupCommission"]);
                }
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = _req.LoginTypeID,
                    UserId = _req.LoginID
                });
            }
            return _resp;
        }
        public object Call() => throw new NotImplementedException();
        public string GetName() => "Proc_GetSystemSetting";
    }
}
