namespace RoundpayFinTech.AppCode.Model.ProcModel
{
    public class ProcAdminLedgerRequest
    {
        public int LT { get; set; }
        public int LoginID { get; set; }
        public int DebitCredit_F { get; set; }
        public int Service_F { get; set; }
        public string TransactionId_F { get; set; }
        public string FromDate_F { get; set; }
        public string ToDate_F { get; set; }
        public string Mobile_F { get; set; }
        public int TopRows { get; set; }
        public int CCID { get; set; }
        public string CCMobileNo { get; set; }
        public int UserID { get; set; }
        public int WalletTypeID { get; set; }
        public string UTR_F { get; set; }
    }
}
