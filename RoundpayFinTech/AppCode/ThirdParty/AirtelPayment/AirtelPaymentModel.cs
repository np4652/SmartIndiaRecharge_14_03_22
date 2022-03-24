namespace RoundpayFinTech.AppCode.ThirdParty.AirtelPayment
{
    public class AirtelSenderLimitRequest {
        public string caf { get; set; }
        public string channel { get; set; }
        public string customerId { get; set; }
        public string feSessionId { get; set; }
        public string hash { get; set; }
        public string partnerId { get; set; }
        public string ver { get; set; }
        public string appVersion { get; set; }
        public string agentId { get; set; }
        public string custId { get; set; }
        public string stateCode { get; set; }
    }
    public class AirtelBankRequest: AirtelSenderLimitRequest
    {
        public string externalRefNo { get; set; }
        public string apiMode { get; set; }
        public string amount { get; set; }
        public string bankName { get; set; }
        public string ifsc { get; set; }
        public string beneAccNo { get; set; }
        public string beneMobNo { get; set; }
        public string reference1 { get; set; }
        public string reference2 { get; set; }
        public string reference3 { get; set; }
        public string reference4 { get; set; }
        public string reference5 { get; set; }
        public string custFirstName { get; set; }
        public string custLastName { get; set; }
        public string custPincode { get; set; }
        public string custAddress { get; set; }
        public string custDob { get; set; }
    }

    public class AirtelBankResponse
    {
        public AirtelMeta meta { get; set; }
        public AirtelData data { get; set; }
        //mcmv-mcv is limit
        public string feSessionId { get; set; }
        public string dcmv { get; set; }
        public string mcmv { get; set; }
        public string mpt { get; set; }
        public string dcv { get; set; }
        public string mcv { get; set; }
        public string messageText { get; set; }        
        public string charges { get; set; }
        public string amount { get; set; }
        public string beneName { get; set; }
        public string externalRefNo { get; set; }
        public string responseTimestamp { get; set; }
        public string rrn { get; set; }
        public string tranId { get; set; }
        public string code { get; set; }
        public string errorCode { get; set; }
        public string description { get; set; }
        public string status { get; set; }
    }

    public class AirtelMeta 
    {
        public string code { get; set; }
        public string description { get; set; }
        public string status { get; set; }
    }
    public class AirtelData {
        public string tranId { get; set; }
        public string rrn { get; set; }
        public string charges { get; set; }
    }


}