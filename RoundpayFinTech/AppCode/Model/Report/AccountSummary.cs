using RoundpayFinTech.AppCode.Model.ProcModel;
using System.Collections.Generic;

namespace Fintech.AppCode.Model.Reports
{
    public class AccountSummary
    {
        public string OutletName { get; set; }
        public string Mobile { get; set; }
        public string Area { get; set; }
        public decimal Balance { get; set; }
        public decimal UBalance { get; set; }
        public decimal Opening { get; set; }
        public decimal Sales { get; set; }
        public decimal Lsale { get; set; }
        public string LSDate { get; set; }
        public decimal CCollection { get; set; }
        public decimal LCollection { get; set; }
        public string LCDate { get; set; }
        public decimal Closing { get; set; }
        public int StatusCode { get; set; }
        public string Status { get; set; }
        public decimal Purchase { get; set; }
        public decimal FundDeducted { get; set; }
        public decimal Return { get; set; }
        public decimal Requested { get; set; }
        public decimal Debited { get; set; }
        public decimal Debited2202 { get; set; }
        public decimal Refunded { get; set; }
        public decimal Commission { get; set; }
        public decimal CCFCommission { get; set; }
        public decimal Surcharge { get; set; }
        public decimal FundTransfered { get; set; }
        public decimal OtherCharge { get; set; }
        public decimal CCFCommDebited { get; set; }
        public decimal Expected { get; set; }
        public decimal CCF { get; set; }
        public int RoleID { get; set; }
        public int UserID { get; set; }
        public decimal SetTarget { get; set; }
        public decimal TargetTillDate { get; set; }
        public bool IsGift { get; set; }
        public decimal DCommission { get; set; }
        public bool IsPrepaid { get; set; }
        public bool IsUtility { get; set; }
    }
    public class ASCollection
    {
        public List<ASBanks> Banks { get; set; }
        public int UserID { get; set; }
        public string OutletName { get; set; }
        public string Mobile { get; set; }
        public List<UserReport> userList { get; set; }
    }
    public class ASBanks
    {
        public int BankID { get; set; }
        public string BankName { get; set; }
    }

}
