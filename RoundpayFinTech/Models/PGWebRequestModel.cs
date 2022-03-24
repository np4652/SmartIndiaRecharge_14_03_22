namespace RoundpayFinTech.Models
{
    public class PGWebRequestModel
    {
        public int id { get; set; }
        public decimal a { get; set; }
        public int w { get; set; }
        public string vpa { get; set; }
    }
    public class PGStatusCheckRequestModel {
        public int OrderID { get; set; }
    }
}
