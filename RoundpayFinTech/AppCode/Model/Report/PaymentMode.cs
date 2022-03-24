using System.Collections.Generic;
using RoundpayFinTech.AppCode.Model.ProcModel;

namespace Fintech.AppCode.Model.Reports
{
    public class PaymentRequestModel
    {
        public int BankID { get; set; }
        public int PaymentID { get; set; }
        public List<Bank> BankList { get; set; }
        public IEnumerable<PaymentModeMaster> PaymentModeList { get; set; }
        public UserBalnace userBalnace { get; set; }
        public IEnumerable<BonafideAccount> BonafideAccountList { get; set; }

    }

    public class PaymentModeMaster
    {
        public int ID { get; set; }
        public int LT { get; set; }
        public int LoginID { get; set; }
        public int BankID { get; set; }
        public int ModeID { get; set; }
        public bool IsActive { get; set; }
        public string MODE { get; set; }
        public bool IsTransactionIdAuto { get; set; }
        public bool IsAccountHolderRequired { get; set; }
        public bool IsChequeNoRequired { get; set; }
        public bool IsCardNumberRequired { get; set; }
        public bool IsMobileNoRequired { get; set; }
        public bool IsBranchRequired { get; set; }
        public bool IsUPIID { get; set; }
        public bool Status { get; set; }
        public string CID { get; set; }
    }

    public class DropDown
    {
        public string ID { get; set; }
        public string Value { get; set; }
    }

}
