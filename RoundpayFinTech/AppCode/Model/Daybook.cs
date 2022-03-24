using System;
using System.Collections.Generic;

namespace RoundpayFinTech.AppCode.Model
{
    public class CommonDaybook
    {
        public string TDate { get; set; }
        public int TotalHits { get; set; }
        public decimal TotalAmount { get; set; }
        public int SuccessHits { get; set; }
        public decimal SuccessAmount { get; set; }
        public int RefundHits { get; set; }
        public decimal RefundAmount { get; set; }
        public int FailedHits { get; set; }
        public decimal FailedAmount { get; set; }
        public int PendingHits { get; set; }
        public decimal PendingAmount { get; set; }
        public decimal APICommission { get; set; }
        public decimal SelfCommission { get; set; }
        public decimal Commission { get; set; }
        public decimal Incentive { get; set; }
        public decimal CircleComm { get; set; }
        public decimal TeamCommission { get; set; }
        public decimal GSTTaxAmount { get; set; }
        public decimal TDSAmount { get; set; }
        public decimal Profit { get; set; }
    }
    public class Daybook : CommonDaybook
    {
        public string API { get; set; }
        public int OID { get; set; }
        public int Denomination { get; set; }
        public string Operator { get; set; }
        public string Circle { get; set; }
        public DateTime EntryDate { get; set; }
    }
    public class APIDaybookDatewise : CommonDaybook
    {
        public string API { get; set; }
        public DateTime EntryDate { get; set; }
        public List<Daybook> Daybooks { get; set; }
    }
    public class DMRDaybook
    {
        public int TotalHits { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal CCF { get; set; }
        public decimal BaseAmount { get; set; }
        public decimal Surcharge { get; set; }
        public decimal GSTOnSurcharge { get; set; }
        public decimal AmountAfterSurcharge { get; set; }
        public decimal RefundGST { get; set; }
        public decimal AmountWithTDS { get; set; }
        public decimal TDS { get; set; }
        public decimal CreditedAmount { get; set; }
        public decimal APISurcharge { get; set; }
        public decimal APIGSTOnSurcharge { get; set; }
        public decimal APIAmountAfterSurcharge { get; set; }
        public decimal APIRefundGST { get; set; }
        public decimal APIAmountWithTDS { get; set; }
        public decimal APITDS { get; set; }
        public decimal APICreditedAmount { get; set; }
        public decimal SelfCommission { get; set; }
        public decimal Commission { get; set; }
        public decimal TeamCommission { get; set; }
        public decimal Profit { get; set; }
    }
}

