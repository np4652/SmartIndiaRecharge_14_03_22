using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Configuration;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetUserBal : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetUserBal(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var _req = (CommonReq)obj;
            SqlParameter[] param = {
                 new SqlParameter("@LoginID", _req.LoginID),
            };
            var _res = new UserBalnace();
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    _res.IsShowLBA = Convert.ToBoolean(dt.Rows[0]["_IsShowLBA"] is DBNull ? false : dt.Rows[0]["_IsShowLBA"]);
                    _res.Balance = Convert.ToDecimal(dt.Rows[0]["_Balance"] is DBNull ? 0 : dt.Rows[0]["_Balance"]);
                    _res.UBalance = Convert.ToDecimal(dt.Rows[0]["_UBalance"] is DBNull ? 0 : dt.Rows[0]["_UBalance"]);
                    _res.BBalance = Convert.ToDecimal(dt.Rows[0]["_BBalance"] is DBNull ? 0 : dt.Rows[0]["_BBalance"]);
                    _res.CBalance = Convert.ToDecimal(dt.Rows[0]["_CBalance"] is DBNull ? 0 : dt.Rows[0]["_CBalance"]);
                    _res.IDBalnace = Convert.ToDecimal(dt.Rows[0]["_IDBalnace"] is DBNull ? 0 : dt.Rows[0]["_IDBalnace"]);
                    _res.PacakgeBalance = Convert.ToDecimal(dt.Rows[0]["_PackageBalance"] is DBNull ? 0 : dt.Rows[0]["_PackageBalance"]);
                    _res.OSBalance = Convert.ToDecimal(dt.Rows[0]["_OSBalance"] is DBNull ? 0 : dt.Rows[0]["_OSBalance"]);
                    _res.IsP = Convert.ToBoolean(dt.Rows[0]["IsPasswordExpired"] is DBNull ? false : dt.Rows[0]["IsPasswordExpired"]);
                    _res.IsPN = ApplicationSetting.IsPasswordOnly ? false :( Convert.ToBoolean(dt.Rows[0]["_IsPINNotSet"] is DBNull ? false : dt.Rows[0]["_IsPINNotSet"]));
                    _res.IsBalance = Convert.ToBoolean(dt.Rows[0]["_IsPrepaid"] is DBNull ? false : dt.Rows[0]["_IsPrepaid"]);
                    _res.IsUBalance = Convert.ToBoolean(dt.Rows[0]["_IsUtility"] is DBNull ? false : dt.Rows[0]["_IsUtility"]);
                    _res.IsBBalance = Convert.ToBoolean(dt.Rows[0]["_IsBank"] is DBNull ? false : dt.Rows[0]["_IsBank"]);
                    _res.IsCBalance = Convert.ToBoolean(dt.Rows[0]["_IsCard"] is DBNull ? false : dt.Rows[0]["_IsCard"]);
                    _res.IsIDBalance = Convert.ToBoolean(dt.Rows[0]["_IsRegID"] is DBNull ? false : dt.Rows[0]["_IsRegID"]);
                    _res.IsPacakgeBalance = Convert.ToBoolean(dt.Rows[0]["_IsPackage"] is DBNull ? false : dt.Rows[0]["_IsPackage"]);
                    _res.IsBalanceFund = Convert.ToBoolean(dt.Rows[0]["_IsPrepaidFund"] is DBNull ? false : dt.Rows[0]["_IsPrepaidFund"]);
                    _res.IsUBalanceFund = Convert.ToBoolean(dt.Rows[0]["_IsUtilityFund"] is DBNull ? false : dt.Rows[0]["_IsUtilityFund"]);
                    _res.IsBBalanceFund = Convert.ToBoolean(dt.Rows[0]["_IsBankFund"] is DBNull ? false : dt.Rows[0]["_IsBankFund"]);
                    _res.IsCBalanceFund = Convert.ToBoolean(dt.Rows[0]["_IsCardFund"] is DBNull ? false : dt.Rows[0]["_IsCardFund"]);
                    _res.IsIDBalanceFund = Convert.ToBoolean(dt.Rows[0]["_IsRegIDFund"] is DBNull ? false : dt.Rows[0]["_IsRegIDFund"]);
                    _res.IsPacakgeBalanceFund = Convert.ToBoolean(dt.Rows[0]["_IsPackageFund"] is DBNull ? false : dt.Rows[0]["_IsPackageFund"]);
                    _res.IsLowBalance = Convert.ToBoolean(dt.Rows[0]["_IsLowBalance"] is DBNull ? false : dt.Rows[0]["_IsLowBalance"]);
                    _res.IsAdminDefined = Convert.ToBoolean(dt.Rows[0]["_IsAdminDefined"] is DBNull ? false : dt.Rows[0]["_IsAdminDefined"]);
                    _res.IsFlatCommissionU = Convert.ToBoolean(dt.Rows[0]["_IsFlatCommissionU"] is DBNull ? false : dt.Rows[0]["_IsFlatCommissionU"]);
                    _res.IsPackageDeducionForRetailor = Convert.ToBoolean(dt.Rows[0]["_IsPackageDeducionForRetailor"] is DBNull ? false : dt.Rows[0]["_IsPackageDeducionForRetailor"]);
                    _res.CommRate = Convert.ToDecimal(dt.Rows[0]["_CommRate"] is DBNull ? 0 : dt.Rows[0]["_CommRate"]);
                    _res.VIAN = dt.Rows[0]["_VIAN"] is DBNull ? string.Empty: dt.Rows[0]["_VIAN"].ToString();
                    _res.IsQRMappedToUser = Convert.ToBoolean(dt.Rows[0]["_IsQRMappedToUser"] is DBNull ? false : dt.Rows[0]["_IsQRMappedToUser"]);
                    _res.IsCandebit = Convert.ToBoolean(dt.Rows[0]["_Candebit"] is DBNull ? false : dt.Rows[0]["_Candebit"]);
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
            return _res;
        }
        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_GetUserBal";
    }
}
