using System.Data;

namespace RoundpayFinTech.AppCode.Model.BBPS
{
    public class BillerInfoUpdateModel
    {
        public int LoginID { get; set; }
        public int OID { get; set; }
        public bool BillerAdhoc { get; set; }
        public int ExactNess { get; set; }
        public string BillerCoverage { get; set; }
        public string BillerName { get; set; }
        public bool IsBillValidation { get; set; }
        public bool IsAmountInValidation { get; set; }
        public string BillerPaymentModes { get; set; }
        public string BillerAmountOptions { get; set; }
        public DataTable tp_OperatorParams { get; set; }
        public DataTable tp_OperatorPaymentChanel { get; set; }

        public DataTable tp_OperatorDictionary { get; set; }
        public int BillFetchRequirement { get; set; }
    }
    public class RPBillerInfoUpdate {
        public int LoginID { get; set; }
        public string Name { get; set; }
        public string OPID { get; set; }
        public int OpTypeID { get; set; }
        public int MinLength { get; set; }
        public int MaxLength { get; set; }
        public int MinAmount { get; set; }
        public int MaxAmount { get; set; }
        public bool IsBBPS { get; set; }
        public bool IsBilling { get; set; }
        public string AccountName { get; set; }
        public string AccountRemark { get; set; }
        public bool IsAccountNumeric { get; set; }
        public string RPBillerID { get; set; }
        public string BillerAmountOptions { get; set; }
        public bool BillerAdhoc { get; set; }
        public int ExactNess { get; set; }
        public string BillerCoverage { get; set; }
        public string BillerName { get; set; }
        public string AccountNoKey { get; set; }
        public bool IsBillValidation { get; set; }
        public bool IsAmountInValidation { get; set; }
        public string BillerPaymentModes { get; set; }
        public string RegExAccount { get; set; }
        public string EarlyPaymentAmountKey { get; set; }
        public string LatePaymentAmountKey { get; set; }
        public string EarlyPaymentDateKey { get; set; }
        public bool IsAmountOptions { get; set; }
        public DataTable tp_OperatorParams { get; set; }
        public DataTable tp_OperatorPaymentChanel { get; set; }
        public DataTable tp_OperatorDictionary { get; set; }
    }
}
