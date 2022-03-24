
namespace RoundpayFinTech.AppCode.Model.ProcModel
{
    public class ProcRechargeReportResponse
    {
        public int ResultCode { get; set; }
        public string Msg { get; set; }
        public int _ServiceID { get; set; }
        public int TID { get; set; }
        public string TransactionID { get; set; }
        public string Prefix { get; set; }
        public int UserID { get; set; }
        public string Role { get; set; }
        public string OutletNo { get; set; }
        public string Outlet { get; set; }
        public string Account { get; set; }
        public int OID { get; set; }
        public string Operator { get; set; }
        public decimal LastBalance { get; set; }
        public decimal RequestedAmount { get; set; }
        public decimal Amount { get; set; }
        public decimal Balance { get; set; }
        public string SlabCommType { get; set; }
        public decimal Commission { get; set; }
        public string EntryDate { get; set; }
        public string API { get; set; }
        public string LiveID { get; set; }
        public int _Type { get; set; }
        public string Type_ { get; set; }
        public string VendorID { get; set; }

        public string ApiRequestID { get; set; }
        public string ModifyDate { get; set; }
        public string Optional1 { get; set; }
        public string Optional2 { get; set; }
        public string Optional3 { get; set; }
        public string Optional4 { get; set; }

        public string Display1 { get; set; }
        public string Display2 { get; set; }
        public string Display3 { get; set; }
        public string Display4 { get; set; }
        public string SwitchingName { get; set; }
        public int SwitchingID { get; set; }
        public string CircleName { get; set; }

        public int RefundStatus { get; set; }
        public string RefundStatus_ { get; set; }

        public bool IsWTR { get; set; }
        public bool CommType { get; set; }              
        public bool ShouldSerializeResultCode() => (false);
        public bool ShouldSerializeMsg() => (false);
        public decimal GSTAmount { get; set; }
        public decimal TDSAmount { get; set; }
        public string CustomerNo { get; set; }
        public string CCName { get; set; }
        public string CCMobile { get; set; }
        public string APICode { get; set; }
        public string ExtraParam { get; set; }
        public string RequestMode { get; set; }
        public string O9 { get; set; }
        public string O10 { get; set; }
        public string O11 { get; set; }
        public int RequestModeID { get; set; }
    }
    public class RechargeReportResponse
    {
        public int ResultCode { get; set; }
        public string Msg { get; set; }

        public int TID { get; set; }
        public string TransactionID { get; set; }
        
        public int UserID { get; set; }
        public string Role { get; set; }
        public string OutletNo { get; set; }
        public string Outlet { get; set; }
        public string Account { get; set; }
        public int OID { get; set; }
        public string Operator { get; set; }
        public decimal LastBalance { get; set; }
        public decimal RequestedAmount { get; set; }
        public decimal Amount { get; set; }
        public decimal Balance { get; set; }
        public string SlabCommType { get; set; }
        public decimal Commission { get; set; }
        public string EntryDate { get; set; }
        public string API { get; set; }
        public string LiveID { get; set; }
        public int _Type { get; set; }
        public string Type_ { get; set; }
        public string VendorID { get; set; }
        public decimal ApiComm { get; set; }
        public string ApiRequestID { get; set; }
        public string ModifyDate { get; set; }
        public string Optional1 { get; set; }
        public string Optional2 { get; set; }
        public string Optional3 { get; set; }
        public string Optional4 { get; set; }

        public int RefundStatus { get; set; }
        public string RefundStatus_ { get; set; }

        public bool IsWTR { get; set; }
        public bool CommType { get; set; }
        
        public bool ShouldSerializeResultCode() => (false);
        public bool ShouldSerializeMsg() => (false);
        public string SubAdmin { get; set; }
        public string SAMobile { get; set; }
        public string SACommType { get; set; }
        public string SASlabCommType { get; set; }
        public decimal SAComm { get; set; }        
        public decimal SAGST { get; set; }
        public decimal SATDS { get; set; }
        public string MasterDistributer { get; set; }
        public string MDMobile { get; set; }
        public string MDCommType { get; set; }
        public string MDSlabCommType { get; set; }
        public decimal MDComm { get; set; }
        public decimal MDGST { get; set; }
        public decimal MDTDS { get; set; }
        public string Distributer { get; set; }
        public string DTMobile { get; set; }
        public string DTCommType { get; set; }
        public string DTSlabCommType { get; set; }
        public decimal DTComm { get; set; }
        public decimal DTGST { get; set; }
        public decimal DTTDS { get; set; }
        public string CCName { get; set; }
        public string CCMobile { get; set; }
    }
    public class RechargeReportSummary
    {
        public long TotalSuccessNo { get; set; }
        public decimal TotalSuccessAmount { get; set; }
        public long TotalFailedNo { get; set; }
        public decimal TotalFailedAmount { get; set; }
        public long TotalPendingNo { get; set; }
        public decimal TotalPendingAmount { get; set; }
    }

    public class ProcDMRTransactionResponse : ProcRechargeReportResponse
    {
        public decimal Opening { get; set; }
        public string OutletUserMobile { get; set; }
        public string OutletUserCompany { get; set; }
        public string SenderMobile { get; set; }
        public string GroupID { get; set; }
        public decimal CCF { get; set; }
        public decimal Surcharge { get; set; }
        public decimal RefundGST { get; set; }
        public decimal AmtWithTDS { get; set; }
        public decimal Credited_Amount { get; set; }
        public string CCName { get; set; }
        public string CCMobileNo { get; set; }
    }
    public class DMRReportResponse {
        public int ResultCode { get; set; }
        public string Msg { get; set; }

        public string GroupID { get; set; }
        public int TID { get; set; }
        public string TransactionID { get; set; }
        
        public int UserID { get; set; }
        public string Role { get; set; }
        public string OutletNo { get; set; }
        public string Outlet { get; set; }
        public string OutletUserCompany { get; set; }
        public string BankName { get; set; }
        public string SenderMobile { get; set; }
        
        public string Account { get; set; }
        public int OID { get; set; }
        public string Operator { get; set; }
        public decimal LastBalance { get; set; }
        public decimal RequestedAmount { get; set; }
        public decimal Amount { get; set; }
        public decimal Balance { get; set; }
        public decimal FixedCharge { get; set; }
        public decimal Charge { get; set; }
        public decimal GSTAfterCharge { get; set; }
        public decimal TDSAfterCharge { get; set; }
        public decimal AmtWithTDS { get; set; }
        public decimal CreditedAmount { get; set; }        
        public string SlabCommType { get; set; }
        public decimal Commission { get; set; }
        public string EntryDate { get; set; }
        public string API { get; set; }
        public string LiveID { get; set; }
        public int _Type { get; set; }
        public string Type_ { get; set; }
        public string VendorID { get; set; }

        public string ApiRequestID { get; set; }
        public string ModifyDate { get; set; }
        public string Optional1 { get; set; }
        public string Optional2 { get; set; }
        public string Optional3 { get; set; }
        public string Optional4 { get; set; }

        public int RefundStatus { get; set; }
        public string RefundStatus_ { get; set; }

        public bool IsWTR { get; set; }
        public bool CommType { get; set; }

        public bool ShouldSerializeResultCode() => (false);
        public bool ShouldSerializeMsg() => (false);
        public string SubAdmin { get; set; }
        public string SAMobile { get; set; }
        public string SACommType { get; set; }
        public string SASlabCommType { get; set; }
        public decimal SAComm { get; set; }
        public decimal SAGST { get; set; }
        public decimal SATDS { get; set; }
        public string MasterDistributer { get; set; }
        public string MDMobile { get; set; }
        public string MDCommType { get; set; }
        public string MDSlabCommType { get; set; }
        public decimal MDComm { get; set; }
        public decimal MDGST { get; set; }
        public decimal MDTDS { get; set; }
        public string Distributer { get; set; }
        public string DTMobile { get; set; }
        public string DTCommType { get; set; }
        public string DTSlabCommType { get; set; }
        public decimal DTComm { get; set; }
        public decimal DTGST { get; set; }
        public decimal DTTDS { get; set; }
        public string CCName { get; set; }
        public string CCMobile { get; set; }
    }
    #region BillFetch

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
    public class ProcBillFetchReportResponse
    {
        public string Reason { get; set; }
        public string PaymentStatus { get; set; }

        public int ID { get; set; }
        public string BillNumber { get; set; }
        public string Outlet { get; set; }
        public string OutletNo { get; set; }
        public string Account { get; set; }
        public string Operator { get; set; }
        public string EntryDate { get; set; }
        public string Status { get; set; }
        public string CustomerName { get; set; }
        public decimal Amount { get; set; }
        public int APIID { get; set; }
        public int ResultCode { get; set; }
        public string Msg { get; set; }
        public string API { get; set; }
        public string Type_ { get; set; }
        public int _Type { get; set; }
        public string RequestMode { get; set; }
        public int RequestModeID { get; set; }
        public decimal Balance { get; set; }
        public decimal LastBalance { get; set; }
        public string OpType { get; set; }

    }
    #endregion
}
