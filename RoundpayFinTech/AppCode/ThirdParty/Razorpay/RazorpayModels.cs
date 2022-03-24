using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.ThirdParty.Razorpay
{
    public class RZRPayAppSetting
    {
        public string key_id { get; set; }
        public string key_secret { get; set; }
        public string CreateCustomerURL { get; set; }
        public string CreateVirtualAccountURL { get; set; }
    }
    public class RZRPayBaseClass
    {
        public string id { get; set; }
        public string entity { get; set; }
        public string name { get; set; }
        public object notes { get; set; }
        public string created_at { get; set; }
    }
    public class RZRPayCreateCustomerResp : RZRPayBaseClass
    {
        public string email { get; set; }
        public string contact { get; set; }
        public string gstin { get; set; }
    }
    public class RZRPayCreateVAccountResp : RZRPayBaseClass
    {
        public string status { get; set; }
        public string description { get; set; }
        public int amount_paid { get; set; }
        public string customer_id { get; set; }
        public List<RZRPayReceivers> receivers { get; set; }
    }
    public class RZRPayReceivers : RZRPayBaseClass
    {
        public string ifsc { get; set; }
        public string bank_name { get; set; }
        public string account_number { get; set; }
        public string username { get; set; }
        public string handle { get; set; }
        public string address { get; set; }
        public string reference { get; set; }
        public string short_url { get; set; }
    }
    public class RazorpayRequest
    {
        public string key_id { get; set; }
        public string order_id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string image { get; set; }
        public string Prefill_name { get; set; }
        public string Prefill_contact { get; set; }
        public string Prefill_email { get; set; }
        public string callback_url { get; set; }
        public string cancel_url { get; set; }
        public decimal amount { get; set; }
        public bool retry { get; set; }
    }
    public class RazorpayOrderRequest
    {
        public decimal amount { get; set; }
        public string currency { get; set; }
        public string receipt { get; set; }
        public short payment_capture { get; set; }
    }
    public class RazorOrder
    {
        public string id { get; set; }
        public string entity { get; set; }
        public int amount { get; set; }
        public int amount_paid { get; set; }
        public int amount_due { get; set; }
        public string currency { get; set; }
        public string receipt { get; set; }
        public object offer_id { get; set; }
        public string status { get; set; }
        public int attempts { get; set; }
        public List<object> notes { get; set; }
        public int created_at { get; set; }
    }
    public class RazorPaySuccessResp
    {
        public string razorpay_payment_id { get; set; }
        public string razorpay_order_id { get; set; }
        public string razorpay_signature { get; set; }
    }
    public class RazorPayCallbackResp
    {
        public string account_id { get; set; }
        public RazorPayload payload { get; set; }
    }
    public class RazorPayload
    {
        public RazorPayloadOP payment { get; set; }        
    }
    public class RazorPayloadOP
    {
        public RazorEntity entity { get; set; }
    }
    public class RazorPaymetnStatusResp
    {
        public string entity { get; set; }
        public int count { get; set; }
        public List<RazorEntity> items { get; set; }
    }
    public class RazorEntity
    {
        public string id { get; set; }
       
        public string status { get; set; }
        public string order_id { get; set; }
        public int amount { get; set; }
        
        public string method { get; set; }
        
        public string vpa { get; set; }
        
        public RazorCard card { get; set; }
       
    }
    public class RazorCard
    {
        public string id { get; set; }
        public string entity { get; set; }
        public string name { get; set; }
        public string last4 { get; set; }
        public string network { get; set; }
        public string type { get; set; }
        public bool international { get; set; }
        public bool emi { get; set; }
    }
    public class RZRPayFetchAllBeneResponse
    {
        public string entity { get; set; }
        public int count { get; set; }
        public List<RZRPayFundAccountEntity> items { get; set; }
    }
    public class RZRPayFundAccountEntity
    {
        public string id { get; set; }
        public string entity { get; set; }
        public string contact_id { get; set; }
        public string account_type { get; set; }
        public bool active { get; set; }
        public string created_at { get; set; }
        public RZRPayBankAccount bank_account { get; set; }
    }
    public class RZRPayBankAccount
    {
        public string ifsc { get; set; }
        public string bank_name { get; set; }
        public string name { get; set; }
        public string account_number { get; set; }
    }
    public class RZRPayFundAccount
    {
        public string id { get; set; }
        public string entity { get; set; }
        public string contact_id { get; set; }
        public RZRPayContact contact { get; set; }

    }
    public class RZRPayContact
    {
        public string id { get; set; }
        public string entity { get; set; }
        public string name { get; set; }
        public string contact { get; set; }
        public string email { get; set; }
        public string type { get; set; }
        public string reference_id { get; set; }
        public bool active { get; set; }
    }
    public class RZRPayoutAppSetting
    {
        public string BaseURL { get; set; }
        public string KeyID { get; set; }
        public string SecretID { get; set; }
        public string AccountNumber { get; set; }
        public string HYPTO_VerificationURL { get; set; }
        public string HYPTO_VerifyAuth { get; set; }
    }
    public class RZRPayoutDirectResponse
    {
        public string id { get; set; }
        public string entity { get; set; }
        public string fund_account_id { get; set; }
        public RZRPayFundAccount fund_account { get; set; }
        public int amount { get; set; }
        public string currency { get; set; }
        public int fees { get; set; }
        public int tax { get; set; }
        public string status { get; set; }
        public string utr { get; set; }
        public string mode { get; set; }
        public string purpose { get; set; }
        public string reference_id { get; set; }
        public string narration { get; set; }
        public string batch_id { get; set; }
        public string failure_reason { get; set; }
        public long created_at { get; set; }
    }
    public class RZRVendorID2Response
    {
        public string entity { get; set; }
        public int count { get; set; }
        public List<RZRItem> items { get; set; }
    }
    public class RZRItem
    {
        public string id { get; set; }
        public string entity { get; set; }
        public string fund_account_id { get; set; }
        public decimal amount { get; set; }
        public string currency { get; set; }
        public decimal fees { get; set; }
        public decimal tax { get; set; }
        public string status { get; set; }
        public string purpose { get; set; }
        public string utr { get; set; }
        public string mode { get; set; }
        public string reference_id { get; set; }
        public string narration { get; set; }
        public string failure_reason { get; set; }
    }
    public class RZRPayError
    {
        public RZRPayErrorData error { get; set; }
    }
    public class RZRPayErrorData
    {
        public string code { get; set; }
        public string description { get; set; }
        public string source { get; set; }
        public string step { get; set; }
        public string reason { get; set; }
        public object metadata { get; set; }
    }

    public class RZRSmartPayCallbackModel
    {
        public string entity { get; set; }
        public string account_id { get; set; }
        public List<string> contains { get; set; }
        public RZRSmartPayPayload payload { get; set; }

        public class RZRSmartPayPayload
        {
            public RZRSmartPayPayment payment { get; set; }
        }
        public class RZRSmartPayPayment
        {
            public RZRPaySmartPayEntity entity { get; set; }
        }
        public class RZRPaySmartPayEntity
        {
            public string id { get; set; }
            public string entity { get; set; }
            public int amount { get; set; }
            public string currency { get; set; }
            public string status { get; set; }
            public string order_id { get; set; }
            public string invoice_id { get; set; }
            public string international { get; set; }
            public string method { get; set; }
            public string amount_refunded { get; set; }
            public string refund_status { get; set; }
            public bool captured { get; set; }
            public string description { get; set; }
            public string card_id { get; set; }
            public string bank { get; set; }
            public string wallet { get; set; }
            public string vpa { get; set; }
            public string email { get; set; }
            public string contact { get; set; }
            public string customer_id { get; set; }
        }
    }
}
