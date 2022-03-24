namespace RoundpayFinTech.AppCode.Model.ProcModel
{
    public class ProcUserLedgerResponse
    {
        public int ResultCode { get; set; }
        public string Msg { get; set; }
        public string TID { get; set; }
        public string User { get; set; }
        public string EntryDate { get; set; }
        public string UserID { get; set; }
        public string Description { get; set; }
        public int ID { get; set; }
        public int TopRows { get; set; }
        public decimal LastAmount { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public decimal CurentBalance { get; set; }
        public string MobileNo { get; set; }
        public string Remark { get; set; }
        public int ServiceID { get; set; }
        public bool LType { get; set; }
        public bool ShouldSerializeResultCode() => (false);
        public bool ShouldSerializeMsg() => (false);
    }
}
