using System.Collections.Generic;

namespace RoundpayFinTech.AppCode.Model.ProcModel
{
    public class SystemSetting
    {
        public int ResultCode { get; set; }
        public string Msg { get; set; }
        public int LoginID { get; set; }
        public int LT { get; set; }
        public int IntervalFundTransfer { get; set; }
        public int IntervalRecharge { get; set; }
        public int MinFundTransfer { get; set; }
        public int MaxFundTransfer { get; set; }
        public string AppVersion { get; set; }
        public int BalanceAlertInterval { get; set; }
        public string WebAppVersion { get; set; }
        public bool IsLookUpFromAPI { get; set; }
        public IEnumerable<TransactionMode> TransactionModes { get; set; }
        public int AdminPasswordExpiry { get; set; }
        public int PasswordExpiry { get; set; }
        public int WrongLoginAttempt { get; set; }
        public bool IsDTHInfo { get; set; }
        public bool IsRoffer { get; set; }
        public bool MoveToWalletInReal { get; set; }
        public bool IsSignupUserActive { get; set; }
        public bool IsPGActiveByAdmin { get; set; }
        public bool IsPGActiveByUpline { get; set; }
        public bool IsAutoBilling { get; set; }
        public bool MTRWRetail { get; set; }
        public decimal FlatSignupCommission { get; set; }
    }
    public class ReferralSetting
    {
        public int Statuscode { get; set; }
        public string Msg { get; set; }
        public bool IsReferralToDownline { get; set; }
        public bool IsUplineUnderAdmin { get; set; }
    }

}
