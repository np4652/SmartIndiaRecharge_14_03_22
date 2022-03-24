namespace Fintech.AppCode.Model
{
    public class TransactionReqResp
    {
        public int TID { get; set; }
        public int APIID { get; set; }
        public string Request { get; set; }
        public string Response { get; set; }
        public string APIName { get; set; }
        public string APIOpCode { get; set; }
        public bool APICommType { get; set; }
        public decimal APIComAmt { get; set; }
    }
}
