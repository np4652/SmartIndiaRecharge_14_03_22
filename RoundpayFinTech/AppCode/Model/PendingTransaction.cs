using RoundpayFinTech.AppCode.Model.ProcModel;
using System.Collections.Generic;

namespace RoundpayFinTech.AppCode.Model
{
    public class PendingTransaction
    {
        public int TID { get; set; }
        public string TransactionID { get; set; }
        public int _Type { get; set; }
        public string Type_ { get; set; }
        public string APICode { get; set; }
        public string AccountNo { get; set; }
        public decimal RequestedAmount { get; set; }
        public string EntryDate { get; set; }
        public int APIID { get; set; }
        public int APIType { get; set; }
        public string APIName { get; set; }
        public string VendorID { get; set; }
        public string Operator { get; set; }
        public int OID { get; set; }
        public string OutletName { get; set; }
        public string OutletMobile { get; set; }
        public string Response { get; set; }
        public string SenderMobile { get; set; }
        public string ApiRequestID { get; set; }
        public string ModifyDate { get; set; }
        public string Display1 { get; set; }
        public string Display2 { get; set; }
        public string Display3 { get; set; }
        public string Display4 { get; set; }
        public string Optional1 { get; set; }
        public string Optional2 { get; set; }
        public string Optional3 { get; set; }
        public string Optional4 { get; set; }
        public string CustomerNo { get; set; }
        public string CCName { get; set; }
        public string CCMobile { get; set; }
        public string ExtraParam { get; set; }
    }

    public class PendingCountAPIWise {
        public int APIID { get; set; }
        public string APIName { get; set; }
        public int Count { get; set; }
        public int APIType { get; set; }
    }

    public class PendingTransactios {
        public List<PendingTransaction> Pendings { get; set; }
        public List<PendingCountAPIWise> PendingAPI { get; set; }
    }
    public class RefundAPITransactios
    {
        public List<RefundTransaction> RefundTransaction { get; set; }
        public List<PendingCountAPIWise> PendingAPI { get; set; }
    }


    public class RefundTransaction
    {
        public int TID { get; set; }
        public string TransactionID { get; set; }
        public int _RefundType { get; set; }
        public string RefundType_ { get; set; }
        public string AccountNo { get; set; }
        public decimal RequestedAmount { get; set; }
        public string EntryDate { get; set; }
        public string RefundRequestDate { get; set; }
        public string RefundActionDate { get; set; }
        public int APIID { get; set; }
        public string APIName { get; set; }
        public string VendorID { get; set; }
        public string Operator { get; set; }
        public int OID { get; set; }
        public int OpType { get; set; }
        public string OutletName { get; set; }
        public string OutletMobile { get; set; }
        public string Response { get; set; }
        public string RStatus { get; set; }
        public string RefundRemark { get; set; }
        public string LiveID { get; set; }
        public string RightAccountNo { get; set; }
        public string RequestMode { get; set; }
    }
}
