
namespace RoundpayFinTech.AppCode.Model
{
    public class UserSettlement
    {
        public int ID { get; set; }
        public int UserID { get; set; }
        public string TransactionDate { get; set; }
        public string Prefix { get; set; }
        public string UserName { get; set; }
        public string MobileNo { get; set; }
        public decimal Opening { get; set; }
        public decimal FundTransfered { get; set; }
        public decimal FundRecieved { get; set; }
        public decimal Refund { get; set; }
        public decimal Commission { get; set; }
        public decimal CCFComm { get; set; }
        public decimal FundDeducted { get; set; }
        public decimal FundCredited { get; set; }
        public decimal Surcharge { get; set; }
        public decimal SuccessPrepaid { get; set; }
        public decimal SuccessPostpaid { get; set; }
        public decimal SuccessDTH { get; set; }
        public decimal SuccessBill { get; set; }
        public decimal SuccessDMT { get; set; }
        public decimal SuccessDMTCharge { get; set; }
        public decimal OtherCharge { get; set; }
        public decimal CCFCommDebited { get; set; }
        public decimal Closing { get; set; }
        public decimal SuccessOther { get; set; }
        public decimal Expected { get; set; }
        public decimal Difference { get; set; }
        public int WalletID { get; set; }
        public string EntryDate { get; set; }
    }

    public class SettlementFilter
    {
        public int LoginID { get; set; }
        public int LT { get; set; }
        public int UserID { get; set; }
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public int TopRow { get; set; }
        public int WalletTypeID { get; set; }
        public string Mobile { get; set; }
    }
}
