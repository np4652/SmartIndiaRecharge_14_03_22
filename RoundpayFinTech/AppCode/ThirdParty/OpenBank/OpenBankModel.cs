namespace RoundpayFinTech.AppCode.ThirdParty.OpenBank
{
    public class OpenTransactionType
    {
        public const string NEFT = "2";
        public const string RTGS = "3";
        public const string IMPS = "4";
    }
    public class OpenBankAppSetting
    {
        public int Version { get; set; }
        public string SecretKey { get; set; }
        public string AccessKey { get; set; }
        public string debit_account_number { get; set; }
        public string BaseURL { get; set; }
        public string VerificationURL { get; set; }
        public string VerifyAuth { get; set; }
    }
    public class OpenBankIntiateReq
    {
        public string bene_account_number { get; set; }
        public string ifsc_code { get; set; }
        public string recepient_name { get; set; }
        public string email_id { get; set; }
        public string mobile_number { get; set; }
        public string otp { get; set; }
        public string debit_account_number { get; set; }
        public string transaction_types_id { get; set; }
        public string amount { get; set; }
        public string merchant_ref_id { get; set; }
        public string purpose { get; set; }
    }
    public class OpenBankResp
    {
        public OpenBankData data { get; set; }
        public OpenBankErrors errors { get; set; }
        public int status { get; set; }
        public string message { get; set; }
    }
    public class OpenBankData
    {
        public string amount { get; set; }
        public string open_transaction_ref_id { get; set; }
        public int transaction_status_id { get; set; }
        public int transaction_types_id { get; set; }
        public string purpose { get; set; }
        public string recepient_name { get; set; }
        public string email_id { get; set; }
        public string mobile_number { get; set; }
        public string merchant_ref_id { get; set; }
        public string debit_account_number { get; set; }
        public string bank_error_message { get; set; }
        public string bank_response_message { get; set; }
        public string created_at { get; set; }
        public int status { get; set; }
    }
    public class OpenBankErrors
    {
        public string[] merchant_ref_id { get; set; }
    }

    public class OBVerifiyResponse
    {
        public bool success { get; set; }
        public string message { get; set; }
        public OBVerifyData data { get; set; }
    }
    public class OBVerifyData
    {
        public int id { get; set; }
        public string txn_time { get; set; }
        public string created_at { get; set; }
        public string txn_type { get; set; }
        public string status { get; set; }
        public decimal amount { get; set; }
        public decimal charges_gst { get; set; }
        public decimal settled_amount { get; set; }
        public decimal closing_balance { get; set; }
        public string bank_ref_num { get; set; }
        public string payment_type { get; set; }
        public string reference_number { get; set; }
        public string verify_reason { get; set; }
        public string verify_account_number { get; set; }
        public string verify_account_ifsc { get; set; }
        public string verify_account_holder { get; set; }
    }
}
