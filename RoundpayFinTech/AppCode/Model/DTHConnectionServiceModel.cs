namespace RoundpayFinTech.AppCode.Model
{
    public class DTHConnectionServiceModel
    {
        public int PID { get; set; }
        public string CustomerNumber { get; set; }
        public string Customersurname { get; set; }
        public string Customer { get; set; }
        public string Address { get; set; }
        public string Pincode { get; set; }
        public string SecurityKey { get; set; }
        public string Gender { get; set; }
        public int AreaID { get; set; }
    }
    public class DTHConnectionServiceRequest: DTHConnectionServiceModel
    {
        public int UserID { get; set; }
        public int RequestModeID { get; set; }
        public string APIRequestID { get; set; }
        public string RequestIP { get; set; }
        public string IMEI { get; set; }
    }
    public class DTHConnectionServiceResponse
    {
        public int Statuscode { get; set; }
        public string Msg { get; set; }
        public int TID { get; set; }
        public string TransactionID { get; set; }
        public decimal Balance { get; set; }
    }
}
