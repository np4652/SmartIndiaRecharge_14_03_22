using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data.SqlClient;
using System.Globalization;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcSystemSetting : IProcedure
    {
        private readonly IDAL _dal;
        public ProcSystemSetting(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var _req = (SystemSetting)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginID", _req.LoginID),
                new SqlParameter("@LT", _req.LT),
                new SqlParameter("@IntervalFundTransfer", _req.IntervalFundTransfer),
                new SqlParameter("@IntervalRecharge", _req.IntervalRecharge),
                new SqlParameter("@MinFundTransfer", _req.MinFundTransfer),
                new SqlParameter("@MaxFundTransfer", _req.MaxFundTransfer),
                new SqlParameter("@AppVersion", _req.AppVersion),
                new SqlParameter("@IsLookUpFromAPI", _req.IsLookUpFromAPI),
                new SqlParameter("@AdminPasswordExpiryDays", _req.AdminPasswordExpiry),
                new SqlParameter("@PasswordExpiryDays", _req.PasswordExpiry),
                new SqlParameter("@WrongLoginAttempt", _req.WrongLoginAttempt),
                new SqlParameter("@IsRoffer", _req.IsRoffer),
                new SqlParameter("@IsDTHInfo", _req.IsDTHInfo),
                new SqlParameter("@BalanceAlertInterval", _req.BalanceAlertInterval),
                new SqlParameter("@MoveToWalletInReal", _req.MoveToWalletInReal),
                new SqlParameter("@IsSignupUserActive", _req.IsSignupUserActive),
                new SqlParameter("@IsPGActiveByAdmin", _req.IsPGActiveByAdmin),
                new SqlParameter("@IsPGActiveByUpline", _req.IsPGActiveByUpline),
                new SqlParameter("@IsAutoBilling", _req.IsAutoBilling),
                new SqlParameter("@MTRWRetail", _req.MTRWRetail),
                new SqlParameter("@FlatSignupCommission", _req.FlatSignupCommission)
            };
            var _resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    _resp.Statuscode = Convert.ToInt32(dt.Rows[0][0],CultureInfo.InvariantCulture);
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
                    LoginTypeID = _req.LT,
                    UserId = _req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return _resp;
        }
        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_SystemSetting";
    }

    public class ProcUpdateSignupSlabID : IProcedure
    {
        private readonly IDAL _dal;
        public ProcUpdateSignupSlabID(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var slabID = (int)obj;
            SqlParameter[] param = {
                new SqlParameter("@SlabID", slabID)
            };
            try
            {
                _dal.Execute(GetName(), param);
                return true;
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = slabID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return false;
        }
        public object Call() => throw new NotImplementedException();
        public string GetName() => "update SYSTEM_SETTING set _DefaultSignupSlabID=@SlabID;update tbl_Users set _SlabID=@SlabID where _RoleID=4;";
    }

    #region Signup Referral
    public class ProcUpdateReferralSetting : IProcedure
    {
        private readonly IDAL _dal;
        public ProcUpdateReferralSetting(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var _req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@IsReferralToDownline", _req.CommonBool),
                new SqlParameter("@IsUplineUnderAdmin", _req.CommonBool2)
            };
            try
            {
                _dal.Execute(GetName(), param);
                return true;
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = 1
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return false;
        }
        public object Call() => throw new NotImplementedException();
        public string GetName() => "update MASTER_SIGNUP_REFERRAL set _IsReferralToDownline=@IsReferralToDownline,_IsUplineUnderAdmin=@IsUplineUnderAdmin";
    }

    public class ProcGetReferralSetting : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetReferralSetting(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            ReferralSetting _res = new ReferralSetting()
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };

            try
            {
                var dt = _dal.GetByProcedure(GetName());
                if (dt.Rows.Count > 0)
                {
                    _res.Statuscode = ErrorCodes.One;
                    _res.Msg = ErrorCodes.SUCCESS;
                    _res.IsReferralToDownline = dt.Rows[0]["_IsReferralToDownline"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsReferralToDownline"]);
                    _res.IsUplineUnderAdmin = dt.Rows[0]["_IsUplineUnderAdmin"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsUplineUnderAdmin"]);
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
                    UserId = 1
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return _res;
        }
        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_GetReferralSetting";
    }

    #endregion
}
