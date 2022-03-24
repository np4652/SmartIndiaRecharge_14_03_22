using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.ThirdParty.Zoop
{
    public class ZoopAppSetting
    {
        public string BaseURL { get; set; }
        public string APIKey { get; set; }
        public string AppID { get; set; }
    }
    public class ZoopModelReq
    {
        public string GSTIN { get; set; }
    }
    public class ZoopModelResp
    {
        public string request_id { get; set; }
        public string task_id { get; set; }
        public string group_id { get; set; }
        public bool success { get; set; }
        public string response_code { get; set; }
        public string response_message { get; set; }
        public object metadata { get; set; }
        public string request_timestamp { get; set; }
        public string response_timestamp { get; set; }

    }
    public class ZoopModelResponseGST : ZoopModelResp
    {
        public ZoopResultGST result { get; set; }
    }
    public class ZoopResultGST
    {
        public string aggregate_turn_over { get; set; }
        public List<string> authorized_signatory { get; set; }
        public string business_constitution { get; set; }
        public List<ZoopBusinessDetail> business_details { get; set; }
        public List<string> business_nature { get; set; }
        public string can_flag { get; set; }
        public string central_jurisdiction { get; set; }
        public string compliance_rating { get; set; }
        public ZoopContact contact { get; set; }
        public string current_registration_status { get; set; }
        public string gstin { get; set; }
        public string is_field_visit_conducted { get; set; }
        public string legal_name { get; set; }
        public string mandate_e_invoice { get; set; }
        public object other_business_address { get; set; }
        public ZoopPrimaryBusinessAddress primary_business_address { get; set; }
        public string register_cancellation_date { get; set; }
        public string register_date { get; set; }
        public string state_jurisdiction { get; set; }
        public string tax_payer_type { get; set; }
        public string trade_name { get; set; }
        public string gross_total_income { get; set; }
        public string gross_total_income_financial_year { get; set; }
    }

    public class ZoopModelResponseAadharOTP : ZoopModelResp
    {
        public ZoopResultAadharOTP result { get; set; }
    }
    public class ZoopResultAadharOTP
    {
        public bool is_otp_sent { get; set; }
        public bool is_number_linked { get; set; }
        public bool is_aadhaar_valid { get; set; }
    }

    public class ZoopModelResponseAadhar : ZoopModelResp
    {
        public ZoopResultAadhar result { get; set; }
    }
    public class ZoopResultAadhar
    {
        public string user_full_name { get; set; }
        public string user_aadhaar_number { get; set; }
        public string user_dob { get; set; }
        public string user_gender { get; set; }
        public ZoopAadharAddress user_address { get; set; }
        public string address_zip { get; set; }
        public string user_profile_image { get; set; }
        public bool user_has_image { get; set; }
        public string user_zip_data { get; set; }
        public string aadhaar_xml_raw { get; set; }
        public string user_parent_name { get; set; }
        public string aadhaar_share_code { get; set; }
        public bool user_mobile_verified { get; set; }
        public string reference_id { get; set; }
    }

    public class ZoopModelResponsePAN : ZoopModelResp
    {
        public ZoopResultPAN result { get; set; }
    }
    public class ZoopResultPAN
    {
        public string aadhaar_seeding_status { get; set; }
        public string pan_last_updated { get; set; }
        public string pan_number { get; set; }
        public string pan_status { get; set; }
        public string user_first_name { get; set; }
        //public string user_full_name { get; set; }
        public string name_on_card { get; set; }
        public string user_last_name { get; set; }
        public string user_title { get; set; }
    }

    public class ZoopModelResponseBankAccount : ZoopModelResp
    {
        public ZoopResultBankAccount result { get; set; }
    }
    public class ZoopResultBankAccount
    {
        public string bank_ref_no { get; set; }
        public string beneficiary_name { get; set; }
        public string transaction_remark { get; set; }
        public string verification_status { get; set; }
    }
    public class ZoopAadharAddress
    {
        public string country { get; set; }
        public string dist { get; set; }
        public string state { get; set; }
        public string po { get; set; }
        public string loc { get; set; }
        public string vtc { get; set; }
        public string subdist { get; set; }
        public string street { get; set; }
        public string house { get; set; }
        public string landmark { get; set; }
    }
    public class ZoopPrimaryBusinessAddress
    {
        public string business_nature { get; set; }
        public string detailed_address { get; set; }
        public string email { get; set; }
        public string last_updated_date { get; set; }
        public string mobile_no { get; set; }
        public string registered_address { get; set; }
    }
    public class ZoopBusinessDetail
    {
        public string saccd { get; set; }
        public string sdes { get; set; }
    }
    public class ZoopContact
    {
        public string email { get; set; }
        public string mobile_no { get; set; }
    }

    public class ZoopDgLockerResponse
    {
        public string request_id { get; set; }
        public bool success { get; set; }
        public string response_message { get; set; }
        public string webhook_security_key { get; set; }
        public string request_timestamp { get; set; }
        public string expires_at { get; set; }
        public object metadata { get; set; }
    }
    public class ZoopModelResponseWebhookDigilocker : ZoopModelResp
    {
        public List<ZoopModelWebhookDigilockerResult> result { get; set; }
    }

    public class ZoopModelWebhookDigilockerResult
    {
        public ZoopModelWebhookDigilockerIssued issued { get; set; }
        public string doctype { get; set; }
        public string status { get; set; }
        public string fetched_at { get; set; }
        public string data_xml { get; set; }
        public string data_pdf { get; set; }
        public ZoopModelWebHookDGlockerDataJson data_json { get; set; }
    }
    public class ZoopModelWebHookDGlockerDataJson
    {
        public ZoopModelWebHookDGlockerDataJsonKYCResp KycRes { get; set; }
    }
    public class ZoopModelWebHookDGlockerDataJsonKYCResp
    {
        public string code { get; set; }
        public string ret { get; set; }
        public string ttl { get; set; }
        public string txn { get; set; }
        public string Rar { get; set; }
        public ZoopModelWebHookDGlockerDataJsonUIDData UidData { get; set; }
    }
    public class ZoopModelWebHookDGlockerDataJsonUIDData
    {
        public string tkn { get; set; }
        public string uid { get; set; }
        public ZoopModelWebHookDGlockerDataJsonPOI Poi { get; set; }
        public ZoopModelWebHookDGlockerDataJsonPOA Poa { get; set; }
        public ZoopModelWebHookDGlockerDataJsonLData LData { get; set; }
        public string Pht { get; set; }
        public object Prn { get; set; }
    }
    public class ZoopModelWebHookDGlockerDataJsonPOI
    {
        public string dob { get; set; }
        public string gender { get; set; }
        public string name { get; set; }
    }
    public class ZoopModelWebHookDGlockerDataJsonPOA {
        public string co { get; set; }
        public string country { get; set; }
        public string dist { get; set; }
        public string house { get; set; }
        public string lm { get; set; }
        public string loc { get; set; }
        public string pc { get; set; }
        public string state { get; set; }
        public string street { get; set; }
        public string vtc { get; set; }
    }
    public class ZoopModelWebHookDGlockerDataJsonLData {
        public string co { get; set; }
        public string country { get; set; }
        public string dist { get; set; }
        public string house { get; set; }
        public string lm { get; set; }
        public string loc { get; set; }
        public string pc { get; set; }
        public string state { get; set; }
        public string street { get; set; }
        public string vtc { get; set; }
    }
    public class ZoopModelWebhookDigilockerIssued
    {
        public string name { get; set; }
        public string type { get; set; }
        public string size { get; set; }
        public string date { get; set; }
        public string parent { get; set; }
        public List<string> mime { get; set; }
        public string uri { get; set; }
        public string doctype { get; set; }
        public string description { get; set; }
        public string issuerid { get; set; }
        public string issuer { get; set; }
    }
}
