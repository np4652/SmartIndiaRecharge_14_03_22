using RoundpayFinTech.Models;
using System.Collections.Generic;

namespace RoundpayFinTech.AppCode.Model
{
    public class TransactionPG:ResponseStatus
    {
        public int ID { get; set; }
        public int LT { get; set; }
        public int LoginID { get; set; }
        public int TID { get; set; }
        public int UserID { get; set; }
        public string UserName { get; set; }
        public string MobileNo { get; set; }
        public int RequestedAmount { get; set; }
        public decimal Amount { get; set; }
        public int Type { get; set; }
        public decimal PgCharge { get; set; }
        public int PGID { get; set; }
        public string PGName { get; set; }
        public string FromDate { get; set; }
        public string EntryDate { get; set; }
        public string LastModifyDate { get; set; }
        public string ToDate { get; set; }
        public int OID { get; set; }
        public int OpTypeID { get; set; }
        public int ServiceID { get; set; }
        public string Operator { get; set; }
        public string OpType { get; set; }
        public string OPID { get; set; }
        public string TransactionID { get; set; }
        public string VendorID { get; set; }
        public string LiveID { get; set; }
        public string Remark { get; set; }
        public int RequestedMode { get; set; }
        public string RequestIP { get; set; }        
        public decimal surcharge { get; set; }
        public int ChargeAmtType { get; set; }
        public int WalletID { get; set; }
        public int UPGID { get; set; }
        public int TopRows { get; set; }
        public int Criteria { get; set; }
        public string CriteriaText { get; set; }
        public string Browser { get; set; }
        public bool IsExport { get; set; }
    }

    public class TransactionPGReportModel : ReportModelCommon
    {
        public IEnumerable<MasterPGateway> PGActive { get; set; }
        public IEnumerable<TransactionPG> Report { get; set; }        
    }
    public class MasterPGateway
    {
        public int ID { get; set; }
        public string PGName { get; set; }

    }

    public class TransactionPGLogDetail
    {
        public int TID { get; set; }
        public string TransactionID { get; set; }
        public string VendorID { get; set; }
        public string LastModified { get; set; }
        public List<LogModal> Log { get; set; }
    }

    public class LogModal
    {
        public string LOG { get; set; }
        public string CheckSum { get; set; }
        public string EntryDate { get; set; }
    }
}
