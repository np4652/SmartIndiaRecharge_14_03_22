using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.ThirdParty.AggrePay
{
    public class AggrepayRequest
    {
        public string api_key { get; set; }
        public string order_id { get; set; }
        public string mode { get; set; }
        public decimal amount { get; set; }
        public string currency { get; set; }
        public string description { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
        public string address_line_1 { get; set; }
        public string address_line_2 { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string country { get; set; }
        public string zip_code { get; set; }
        public string timeout_duration { get; set; }
        public string udf1 { get; set; }
        public string udf2 { get; set; }
        public string udf3 { get; set; }
        public string udf4 { get; set; }
        public string udf5 { get; set; }
        public string return_url { get; set; }
        public string return_url_failure { get; set; }
        public string return_url_cancel { get; set; }
        public string percent_tdr_by_user { get; set; }
        public string flatfee_tdr_by_user { get; set; }
        public string show_convenience_fee { get; set; }
        public string split_enforce_strict { get; set; }
        public string split_info { get; set; }
        public string payment_options { get; set; }
        public string payment_page_display_text { get; set; }
        public string allowed_bank_codes { get; set; }
        public string allowed_emi_tenure { get; set; }
        public string allowed_bins { get; set; }
        public string offer_code { get; set; }
        public string product_details { get; set; }
        public string enable_auto_refund { get; set; }
        public string hash { get; set; }
    }
    public class AggrePayResponse
    {
        public string transaction_id { get; set; }
        public string payment_mode { get; set; }
        public string payment_channel { get; set; }
        public string payment_datetime { get; set; }
        public string response_code { get; set; }
        public string response_message { get; set; }
        public string error_desc { get; set; }
        public string order_id { get; set; }
        public decimal amount { get; set; }
        public string currency { get; set; }
        public string description { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
        public string address_line_1 { get; set; }
        public string address_line_2 { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string country { get; set; }
        public string zip_code { get; set; }
        public string udf1 { get; set; }
        public string udf2 { get; set; }
        public string udf3 { get; set; }
        public string udf4 { get; set; }
        public string udf5 { get; set; }
        public decimal tdr_amount { get; set; }
        public decimal tax_on_tdr_amount { get; set; }
        public decimal amount_orig { get; set; }
        public string cardmasked { get; set; }
        public string hash { get; set; }
    }
    public class AggrePayResponseData {
        public List<AggrePayResponse> data { get; set; }
        public string hash { get; set; }
    }
}
