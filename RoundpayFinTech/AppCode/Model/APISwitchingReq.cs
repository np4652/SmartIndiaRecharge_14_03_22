namespace RoundpayFinTech.AppCode.Model
{
    public class OpTypeWiseAPISwitchingReq : APISwitched
    {
        public int LT { get; set; }
        public int LoginID { get; set; }
        public int OpTypeId { get; set; }
        public string IP { get; set; }
        public string Browser { get; set; }
    }
}
