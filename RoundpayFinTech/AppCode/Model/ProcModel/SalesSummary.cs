using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Model.ProcModel
{
    public class SalesSummary
    {
        public int UserID { get; set; }
        public string OutletMobile { get; set; }
        public string OutletName { get; set; }
        public int OID { get; set; }
        public string Operator { get; set; }
        public int SCount { get; set; }
        public decimal Amount { get; set; }
        public decimal AmountR { get; set; }
        public int FCount { get; set; }
        public decimal FailedAmount { get; set; }
        public decimal FailedAmountR { get; set; }
        public decimal GSTAmount { get; set; }
        public decimal TDSAmount { get; set; }
        public string EntryDate { get; set; }
        public decimal LoginComm { get; set; }
        public int ServiceID { get; set; }
    }
    public class SalesSummaryOpWise {
        public int _OID { get; set; }
        public string _Operator { get; set; }
        public decimal TAmount { get; set; }
        public decimal TAmountR { get; set; }
        public decimal TAmountF { get; set; }
        public decimal TAmountRF { get; set; }
        public decimal TLoginCom { get; set; }
        public List<SalesSummary> OpSalesSummary { get; set; }
    }
    public class SalesSummaryUserDateWise
    {
        public int _UserID { get; set; }
        public string _EntryDate { get; set; }
        public string _OutletName { get; set; }
        public string _OutletMobile { get; set; }
        public int TSuccess { get; set; }
        public decimal TAmount { get; set; }
        public decimal TAmountR { get; set; }
        public int TFail { get; set; }
        public decimal TFAmount { get; set; }
        public decimal TFAmountR { get; set; }
        public decimal TGSTAmount { get; set; }
        public decimal TTDSAmount { get; set; }
        public int _ServiceID { get; set; }
        public List<SalesSummary> OpSalesSummary { get; set; }
    }
  
    public class UserRolewiseTransaction {
        public int OID { get; set; }
        public string Operator { get; set; }
        public int OpTypeID { get; set; }
        public string OpType { get; set; }
        public string TransactionDate { get; set; }
        public int UserID { get; set; }
        public string Prefix { get; set; }
        public string Role { get; set; }
        public string Name { get; set; }
        public string OutletName { get; set; }
        public string MobileNo { get; set; }
        public double Requested { get; set; }
        public double Debited { get; set; }
        public double FailedRequested { get; set; }
        public double FailedDebited { get; set; }
        public double Commission { get; set; }
        public double Surcharge { get; set; }
        public double GST { get; set; }
        public double TDS { get; set; }
        public int SAID { get; set; }
        public string SAName { get; set; }
        public double SACommission { get; set; }
        public double SAGST { get; set; }
        public double SATDS { get; set; }
        public int MDID { get; set; }
        public string MDName { get; set; }
        public double MDCommission { get; set; }
        public double MDGST { get; set; }
        public double MDTDS { get; set; }
        public int DTID { get; set; }
        public string DTName { get; set; }
        public double DTCommission { get; set; }
        public double DTGST { get; set; }
        public double DTTDS { get; set; }
    }
}
