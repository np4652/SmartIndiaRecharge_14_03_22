namespace RoundpayFinTech.AppCode.Model.Reports.Filter
{
    public class ULedgerReportFilter
    {
        public int DebitCredit_F { get; set; }
        public string TransactionId_F { get; set; }
        public string FromDate_F { get; set; }
        public string ToDate_F { get; set; }
        public string Mobile_F { get; set; }
        public int TopRows { get; set; }
        public int WalletTypeID { get; set; }
        public int AreaID { get; set; }
        public int UType { get; set; }
        public sbyte RequestMode { get; set; }
        public int UID { get; set; }
    }
}
