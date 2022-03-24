namespace RoundpayFinTech.AppCode.Model.ProcModel
{
    public class VIANCallbackRequest
    {
        public int MVBID { get; set; }
        public string MVBToken { get; set; }
        public string AgentID { get; set; }
        public string VIAN { get; set; }
        public decimal Amount { get; set; }
        public string Operation { get; set; }
        public string IPAddress { get; set; }
        public string Browser { get; set; }
    }

    
}
