namespace RoundpayFinTech.AppCode.Model.ProcModel
{
    public class ProcAdminLedgerResponse
    {
        public int ResultCode { get; set; }
        public string Msg { get; set; }
        public string Role { get; set; }
        public string TID { get; set; }
        public string Description { get; set; }
        public int ID { get; set; }
        public int ToUserID { get; set; }
        public string OutletName { get; set; }
        public string MobileNo { get; set; }
        public string EntryDate { get; set; }
        public int TopRows { get; set; }
        public string TrDate { get; set; }
        public decimal LastBalance { get; set; }
        public decimal Credit { get; set; }
        public decimal Debit { get; set; }
        public decimal CurentBalance { get; set; }
        
        public int WalletID { get; set; }
        public string UTR { get; set; }
        public string BankName { get; set; }
        public string Remark { get; set; }
        
    }
}
