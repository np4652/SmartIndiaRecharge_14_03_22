using Newtonsoft.Json;
using RoundpayFinTech.AppCode.Model.Recharge;
using System.Collections.Generic;

namespace RoundpayFinTech.AppCode.ThirdParty.Eko
{
    public class EKOAppSetting
    {
        public string EKOKey { get; set; }
        public string InitiatorKey { get; set; }
        public string DeveloperKey { get; set; }
        public string BaseURL { get; set; }
        public string OnBoardingURL { get; set; }
    }
    public class EKOClassess
    {
        public int response_status_id { get; set; }
        public int response_type_id { get; set; }
        public string message { get; set; }
        public int? status { get; set; }
        public EKOData data { get; set; }
        public EKOInvalidParams invalid_params { get; set; }
        public APIReqResp aPIReqResp { get; set; }
    }

    public class EKOData
    {
        public string list_specific_id { get; set; }
        public string otp_ref_id { get; set; }
        public string user_code { get; set; }
        public string initiator_id { get; set; }
        public string customer_id_type { get; set; }
        public string customer_id { get; set; }
        public double available_limit { get; set; }
        public double balance { get; set; }
        public string state_desc { get; set; }
        public string name { get; set; }
        public string mobile { get; set; }
        public List<EKOLimit> limit { get; set; }
        public string currency { get; set; }
        public string state { get; set; }
        public double used_limit { get; set; }
        public double total_limit { get; set; }
        public string reason { get; set; }
        public string otp { get; set; }
        public int pan_required { get; set; }
        public List<EKORecipient> recipient_list { get; set; }
        public double remaining_limit_before_pan_required { get; set; }
        public string recipient_id_type { get; set; }
        public string ifsc { get; set; }
        public string is_verified { get; set; }
        public string account { get; set; }
        public int recipient_id { get; set; }
        public EKOPipes pipes { get; set; }

        public string tx_status { get; set; }
        public string debit_user_id { get; set; }
        public string tds { get; set; }
        public string txstatus_desc { get; set; }
        public string fee { get; set; }
        public string total_sent { get; set; }
        public string channel { get; set; }
        public string collectable_amount { get; set; }
        public string txn_wallet { get; set; }
        public string utility_acc_no { get; set; }
        public string sender_name { get; set; }
        public string ekyc_enabled { get; set; }
        public string tid { get; set; }
        public string bank { get; set; }
        public string utrnumber { get; set; }
        public string totalfee { get; set; }
        public string next_allowed_limit { get; set; }
        public string is_otp_required { get; set; }
        public string aadhar { get; set; }
        public string commission { get; set; }
        public string bank_ref_num { get; set; }
        public string timestamp { get; set; }
        public string amount { get; set; }
        public string pinNo { get; set; }
        public string payment_mode_desc { get; set; }
        public string channel_desc { get; set; }
        public string last_used_okekey { get; set; }
        public string client_ref_id { get; set; }
        public string npr { get; set; }
        public string service_tax { get; set; }
        public string paymentid { get; set; }
        public string mdr { get; set; }
        public string recipient_name { get; set; }
        public string kyc_state { get; set; }

        public string branch { get; set; }
        public string tx_desc { get; set; }
        public string allow_retry { get; set; }
        public string refund_tid { get; set; }

        public string commission_reverse { get; set; }
        public string refunded_amount { get; set; }
        public string is_name_editable { get; set; }
        public string is_ifsc_required { get; set; }
        

    }

    public class EKOLimit
    {
        public string name { get; set; }
        public string pipe { get; set; }
        public string used { get; set; }
        public int? priority { get; set; }
        public string remaining { get; set; }
        public string status { get; set; }
    }

    public class EKOInvalidParams
    {
        public string customer_id { get; set; }
    }

    public class EKORecipient
    {
        public int channel_absolute { get; set; }
        public int available_channel { get; set; }
        public string account_type { get; set; }
        public int ifsc_status { get; set; }
        public string is_self_account { get; set; }
        public int channel { get; set; }
        public int is_imps_scheduled { get; set; }
        public string recipient_id_type { get; set; }
        public string imps_inactive_reason { get; set; }
        public int allowed_channel { get; set; }
        public int is_verified { get; set; }
        public string bank { get; set; }
        public string is_otp_required { get; set; }
        public string recipient_mobile { get; set; }
        public string recipient_name { get; set; }
        public string ifsc { get; set; }
        public string account { get; set; }
        public int recipient_id { get; set; }
        public int is_rblbc_recipient { get; set; }
        public string recipient_info { get; set; }
        public EKOPipes pipes { get; set; }
    }

    public class EKOPipes
    {
        [JsonProperty("1")]
        public EKOPipe Pipe1 { get; set; }

        [JsonProperty("2")]
        public EKOPipe Pipe2 { get; set; }

        [JsonProperty("3")]
        public EKOPipe Pipe3 { get; set; }

        [JsonProperty("4")]

        public EKOPipe Pipe4 { get; set; }

        [JsonProperty("5")]
        public EKOPipe Pipe5 { get; set; }

        [JsonProperty("6")]
        public EKOPipe Pipe6 { get; set; }

        [JsonProperty("7")]
        public EKOPipe Pipe7 { get; set; }
    }

    public class EKOPipe
    {
        public int pipe { get; set; }
        public int status { get; set; }
    }

    public class EKORequest
    {
        public string customer_id { get; set; }
        public int service_code { get; set; }
        public string name { get; set; }
        public string sender_name { get; set; }
        public string otp { get; set; }
        public string otp_ref_id { get; set; }
        public string recipient_id_type { get; set; }
        public string id { get; set; }
        public string recipient_type { get; set; }
        public string recipient_name { get; set; }
        public string recipient_mobile { get; set; }
        public string bank_id { get; set; }
        public string recipient_id { get; set; }
        public string bank_code { get; set; }
        public string account { get; set; }
        public string client_ref_id { get; set; }
        public int amount { get; set; }
        public string timestamp { get; set; }
        public string cust_id { get; set; }
        public int hold_timeout { get; set; }
        public int state { get; set; }
        public int channel { get; set; }
        public string ifsc { get; set; }
        public int merchant_document_id_type { get; set; }
        public string merchant_document_id { get; set; }
        public string pincode { get; set; }
        public string latlong { get; set; }
        public string tid { get; set; }
        public int id_proof_type_id { get; set; }
        public string id_proof { get; set; }
        public int ovd_type_id { get; set; }
        public string ovd_number { get; set; }
        public string file1 { get; set; }
        public string file2 { get; set; }
        public string customer_photo { get; set; }
        public string ovd_image { get; set; }
        public string pan_number { get; set; }
        public string mobile { get; set; }
        public string first_name { get; set; }
        public string user_code { get; set; }
        public string residence_address { get; set; }
        public int payment_mode { get; set; }
    }
    public class EKO2OnboardRequest
    {
        public string pan_number { get; set; }
        public string mobile { get; set; }
        public string first_name { get; set; }
        public string middle_name { get; set; }
        public string last_name { get; set; }
        public string email { get; set; }
        public string residence_address { get; set; }
        public string line { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string pincode { get; set; }
        public string district { get; set; }
        public string area { get; set; }
        public string dob { get; set; }
        public string shop_name { get; set; }
    }
}
