
namespace RoundpayFinTech.AppCode.Model.Report
{
    public class NumberSeries
    {
        public int OID { get; set; }
        public short Series { get; set; }
        public short CircleCode { get; set; }
        public int CircleID { get; set; }
        public string Circle { get; set; }
        public string Number { get; set; }
    }
    public class CirlceMaster {
        public int ID { get; set; }
        public string Circle { get; set; }
        public string Code { get; set; }
    }
}
