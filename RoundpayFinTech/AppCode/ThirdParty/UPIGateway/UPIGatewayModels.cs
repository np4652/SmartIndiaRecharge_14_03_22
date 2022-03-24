namespace RoundpayFinTech.AppCode.ThirdParty.UPIGateway
{
    public class UPIGatewayRequest
    {
        public string client_vpa { get; set; }
        public int amount { get; set; }
        public int client_name { get; set; }
        public int client_email { get; set; }
        public int client_mobile { get; set; }
        public int p_info { get; set; }
        public int client_txn_id { get; set; }
        public int udf1 { get; set; }
        public int udf2 { get; set; }
        public int udf3 { get; set; }
        public string key { get; set; }
        public string redirect_url { get; set; }
        public string hash { get; set; }
    }

    public class UPIGatewayResponse {
        public string client_vpa { get; set; }
        public string amount { get; set; }
        public string client_name { get; set; }
        public string client_email { get; set; }
        public string client_mobile { get; set; }
        public string p_info { get; set; }
        public int client_txn_id { get; set; }
        public string udf1 { get; set; }
        public string udf2 { get; set; }
        public string udf3 { get; set; }
        public string socket_id { get; set; }
        public string upi_txn_id { get; set; }
        public string remark { get; set; }
        public string txnAt { get; set; }
        public string status { get; set; }
        public string createdAt { get; set; }
        public string updatedAt { get; set; }
    }
}
