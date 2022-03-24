namespace RoundpayFinTech.AppCode.Model.ProcModel
{
    public class AdminSettlement
    {
        public string _TransactionDate { get; set; }
        public double _Opening { get; set; }
        public double _FundTransfered { get; set; }
        public double _Refund { get; set; }
        public double _Commission { get; set; }
        public double _CCFComm { get; set; }
        public double _FundDeducted { get; set; }
        public double _Surcharge { get; set; }
        public double _SuccessPrepaid { get; set; }
        public double _TotalSUccess { get; set; }
        public double _SuccessPostpaid { get; set; }
        public double _SuccessDTH { get; set; }
        public double _SuccessUtility { get; set; }
        public double _SuccessBill { get; set; }
        public double _SuccessDMT { get; set; }
        public double _SuccessDMTCharge { get; set; }
        public double _OtherCharge { get; set; }
        public double _CCFCommDebited { get; set; }
        public string _EntryDate { get; set; }
        public double _Closing { get; set; }
        public double _Expected { get; set; }
    }
}
