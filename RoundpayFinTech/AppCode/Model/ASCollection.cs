namespace RoundpayFinTech.AppCode.Model
{
    public class ASCollectionReq
    {
        public int LoginID { get; set; }
        public int UserID { get; set; }
        public string CollectionMode { get; set; }
        public decimal Amount { get; set; }
        public string Remark { get; set; }
        public int BankName { get; set; }
        public string UTR { get; set; }
        public sbyte RequestMode { get; set; }
    }

    public class ASAreaMaster
    {
        public int AreaID { get; set; }
        public int UserID { get; set; }
        public string Area { get; set; }
        public string EntryDate { get; set; }
        public string ModifyDate { get; set; }
    }
}
