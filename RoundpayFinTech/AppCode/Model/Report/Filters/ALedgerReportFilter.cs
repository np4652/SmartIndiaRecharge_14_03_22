namespace RoundpayFinTech.AppCode.Model.Reports.Filter
{
    public class ALedgerReportFilter
    {
        public int DebitCredit_F { get; set; }
        public int Service_F { get; set; }
        public string TransactionId_F { get; set; }
        public string FromDate_F { get; set; }
        public string ToDate_F { get; set; }
        public string Mobile_F { get; set; }
        public int TopRows { get; set; }
        public int Criteria { get; set; }
        public string CriteriaText { get; set; }
        public int WalletTypeID { get; set; }
    }
}
