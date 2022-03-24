using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.Models;
using System;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetASetting : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetASetting(IDAL dal) => _dal = dal;
        public object Call(object obj) => throw new NotImplementedException();

        public object Call()
        {
            var _resp = new ApplicationSettingModel();
            try
            {
                var dt = _dal.Get(GetName());
                if (dt.Rows.Count > 0)
                {
                    _resp.IsMultipleMobileAllowed = dt.Rows[0]["_IsMultipleMobileAllowed"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsMultipleMobileAllowed"]);
                    _resp.IsDoubleWallet = dt.Rows[0]["_IsDoubleWallet"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsDoubleWallet"]);
                    _resp.IsCCFApplicable = dt.Rows[0]["_IsCCFApplicable"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsCCFApplicable"]);
                    _resp.IsRoleFixed = dt.Rows[0]["_IsRoleFixed"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsRoleFixed"]);
                    _resp.IsTDSnGSTApplicableRoles = dt.Rows[0]["_IsTDSnGSTApplicableRoles"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsTDSnGSTApplicableRoles"]);
                    _resp.IsGSTEnable = dt.Rows[0]["_IsGSTEnable"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsGSTEnable"]);
                    _resp.IsRoleHierarchyFixed = dt.Rows[0]["_IsRoleHierarchyFixed"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsRoleHierarchyFixed"]);
                    _resp.IsAPIUserByAll = dt.Rows[0]["_IsAPIUserByAll"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsAPIUserByAll"]);
                    _resp.IsRoleCommissionDisplay = dt.Rows[0]["_IsRoleCommissionDisplay"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsRoleCommissionDisplay"]);
                    _resp.IsPINRequired = dt.Rows[0]["_IsPINRequired"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsPINRequired"]);
                    _resp.IsDTHInfo = dt.Rows[0]["_IsDTHInfo"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsDTHInfo"]);
                    _resp.IsRoffer = dt.Rows[0]["_IsRoffer"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsRoffer"]);
                    _resp.IsSingleDB = dt.Rows[0]["_IsSingleDB"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsSingleDB"]);
                    _resp.IsReferral = dt.Rows[0]["IsReferral"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["IsReferral"]);
                    _resp.IsPasswordOnly = dt.Rows[0]["_IsPasswordOnly"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsPasswordOnly"]);
                    _resp.IsDefaultOTPDisabled = dt.Rows[0]["_IsDefaultOTPDisabled"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsDefaultOTPDisabled"]);
                    _resp.IsCommissionOnTopUp = dt.Rows[0]["_IsCommissionOnTopUp"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsCommissionOnTopUp"]);
                    _resp.IsPINBlankByDefault = dt.Rows[0]["_IsPINBlankByDefault"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsPINBlankByDefault"]);
                    _resp.IsRealSettlement = dt.Rows[0]["_IsRealSettlement"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsRealSettlement"]);
                    _resp.IsAPISwitchAfterCircleFailed = dt.Rows[0]["_IsAPISwitchAfterCircleFailed"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsAPISwitchAfterCircleFailed"]);
                    _resp.IsFlatCommission = dt.Rows[0]["_IsFlatCommission"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsFlatCommission"]);
                    _resp.IsGSTDOCTypeEnabled = dt.Rows[0]["_IsGSTDOCTypeEnabled"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsGSTDOCTypeEnabled"]);
                    _resp.IsProductWiseWalletSet = dt.Rows[0]["_IsProductWiseWalletSet"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsProductWiseWalletSet"]);
                    _resp.IsGenerateOrderForUPI = dt.Rows[0]["_IsGenerateOrderForUPI"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsGenerateOrderForUPI"]);
                    _resp.IsCommissionOnRegistration = dt.Rows[0]["_IsCommissionOnRegistration"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsCommissionOnRegistration"]);
                    _resp.IsCircleSwitchingFirst = dt.Rows[0]["_IsCircleSwitchingFirst"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsCircleSwitchingFirst"]);
                    _resp.IsRealAPIPerTransaction = dt.Rows[0]["_IsRealAPIPerTransaction"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsRealAPIPerTransaction"]);
                    _resp.IsMoveToUtility = dt.Rows[0]["_IsMoveToUtility"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsMoveToUtility"]);
                    _resp.IsHeavyRefresh = dt.Rows[0]["_IsHeavyRefresh"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsHeavyRefresh"]);
                    _resp.IsWhiteLabel = dt.Rows[0]["_IsWhiteLabel"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsWhiteLabel"]);
                    _resp.IsRechargePlansAPIOnly = dt.Rows[0]["_IsRechargePlansAPIOnly"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsRechargePlansAPIOnly"]);
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
            return _resp;
        }
        public string GetName() => "select * from ApplicationSetting";
    }
}
