namespace RoundpayFinTech.AppCode.Model.Reports.Filter
{
    public class RechargeReportFilter
    {
        public int TopRows { get; set; }
        public int Status { get; set; }
        public int OID { get; set; }
        public int APIID { get; set; }
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public int Criteria { get; set; }
        public string CriteriaText { get; set; }
        public bool IsExport { get; set; }
        public int OPTypeID { get; set; }
        public int RequestModeID { get; set; }
        public int CircleID { get; set; }
        public int SwitchID { get; set; }
        public int PID { get; set; }
        public int BookingStatus { get; set; }
        public int DMRModelID { get; set; }
        public string BankName { get; set; }
        public string API { get; set; }

    }
    public class _RechargeReportFilter : RechargeReportFilter
    {
        public int LT { get; set; }
        public int LoginID { get; set; }
        public string OutletNo { get; set; }
        public string TransactionID { get; set; }
        public int TID { get; set; }
        public string API { get; set; }
        public string APIRequestID { get; set; }
        public string AccountNo { get; set; }
        public string VendorID { get; set; }
        public string UserOutletMobile { get; set; }
        public string SenderMobile { get; set; }
        public string LiveID { get; set; }
        public int CCID { get; set; }
        public string CCMobileNo { get; set; }
        public string CMobileNo { get; set; }
        public int UserID { get; set; }        
        public bool IsRecent { get; set; }
    }
    public class RefundLogFilter: RechargeReportFilter
    {
        public int LoginTypeID { get; set; }
        public int LoginID { get; set; }
        public int DateType { get; set; }
        public bool IsDMR { get; set; }
        public bool IsReport { get; set; }
    }
    public class _RefundLogFilter : RefundLogFilter {
        public string OutletNo { get; set; }
        public int UserID { get; set; }
        public string TransactionID { get; set; }
        public int TID { get; set; }
        public string AccountNo { get; set; }
        public string RightAccountNo { get; set; }
        
    }
    public class BillFetchReportResponse
    {
        public int ID { get; set; }
        public string BillNumber { get; set; }
        public string Outlet { get; set; }
        public string OutletNo { get; set; }
        public string AccountNumber { get; set; }
        public string Operator { get; set; }
        public string EntryDate { get; set; }
        public string Status { get; set; }
        public string CustomerName { get; set; }
        public decimal Amount { get; set; }
        public string API { get; set; }
    }

    public class _BillFetchReportFilter : RechargeReportFilter
    {
        public int LT { get; set; }
        public int LoginID { get; set; }
        public string OutletNo { get; set; }
        public string BillNumber { get; set; }
        public string Request { get; set; }
        public string Response { get; set; }
        public string CustomerName { get; set; }
        public string TransactionID { get; set; }
        public int TID { get; set; }
        public string API { get; set; }
        public string APIRequestID { get; set; }
        public string AccountNo { get; set; }
        public string VendorID { get; set; }
        public string UserOutletMobile { get; set; }
        public string SenderMobile { get; set; }
        public string LiveID { get; set; }
        public int CCID { get; set; }
        public string CCMobileNo { get; set; }
        public string CMobileNo { get; set; }
        public int UserID { get; set; }
        public bool IsRecent { get; set; }
    }
}
