namespace RoundpayFinTech.AppCode.ThirdParty.EasyPay
{
    public class EasypaySetting
    {
        public string BBPSBaseURL { get; set; }
        public string BBPSID { get; set; }
        public string AuthCode { get; set; }
        public string CustomerMobile { get; set; }
        public string AgentMobile { get; set; }
        public string Pincode { get; set; }
        public string Geocode { get; set; }
    }
    public class EasypayRequest
    {
        public string MessageCode { get; set; }
        public string AuthCode { get; set; }
        public string RequestID { get; set; }
        public string ProductID { get; set; }
        public string BBPSID { get; set; }
        public string AgentID { get; set; }
        public string CustomerName { get; set; }
        public string PanNumber { get; set; }
        public string AadharNumber { get; set; }
        public string PostalCode { get; set; }
        public string Location { get; set; }
        public string Validator1 { get; set; }
        public string Validator2 { get; set; }
        public string Validator3 { get; set; }
        public string CustomerMobile { get; set; }
        public string Value1 { get; set; }
        public string Value2 { get; set; }
        public string Value3 { get; set; }
        public string Value4 { get; set; }
    }
    public class EasypayResponse
    {
        public string MessageCode { get; set; }
        public string RequestID { get; set; }
        public string ProductID { get; set; }
        public string Valid { get; set; }
        public string ErrorMessage { get; set; }
        public string Validator1 { get; set; }
        public string Validator2 { get; set; }
        public string Validator3 { get; set; }
        public string BillNumber { get; set; }
        public string BillDate { get; set; }
        public string BillDueDate { get; set; }
        public string BillAmount { get; set; }
        public string BillPartial { get; set; }
        public string Value1 { get; set; }
        public string Value2 { get; set; }
        public string Value3 { get; set; }
        public string Value4 { get; set; }
    }
}
