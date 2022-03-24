
namespace RoundpayFinTech.AppCode.Model
{
    public class SlabRangeDetail
    {
        public int ID { get; set; }
        public int OID { get; set; }
        public int SlabID { get; set; }
        public int RangeId { get; set; }
        public int MinRange { get; set; }
        public int MaxRange { get; set; }
        public decimal Comm { get; set; }
        public decimal MaxComm { get; set; }
        public decimal FixedCharge { get; set; }
        public bool CommType { get; set; }
        public bool AmtType { get; set; }
        public string Operator { get; set; }
        public int DMRModelID { get; set; }
    }

    public class SlabSpecialCircleWise {
        public string Circle  { get; set; }
        public int Denomination  { get; set; }
        public int DenomMin  { get; set; }
        public int DenomMax  { get; set; }
        public decimal CommAmount { get; set; }
        public bool CommType { get; set; }
        public bool AmtType { get; set; }
    }
}
