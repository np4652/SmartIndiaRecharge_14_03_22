using System.Collections.Generic;
using System.Xml.Serialization;

namespace RoundpayFinTech.AppCode.ThirdParty.Instantpay
{
    public class InstantPayAppSetting
    {
        public string BaseURL { get; set; }
        public string Token { get; set; }
    }
    public class IPayPayoutRequest
    {
        public string token { get; set; }
        public IPayPayoutRequestHelper request { get; set; }
    }
    public class IPayPayoutRequestHelper
    {
        public string sp_key { get; set; }
        public string external_ref { get; set; }
        public string credit_account { get; set; }
        public string credit_rmn { get; set; }
        public string ifs_code { get; set; }
        public string bene_name { get; set; }
        public string credit_amount { get; set; }
        public string upi_mode { get; set; }
        public string vpa { get; set; }
        public string latitude { get; set; }
        public string longitude { get; set; }
        public string endpoint_ip { get; set; }
        public string alert_mobile { get; set; }
        public string alert_email { get; set; }
        public string remarks { get; set; }
        public string otp_auth { get; set; }
        public string otp { get; set; }
    }
    public class IPayCommonResponse
    {
        public string statuscode { get; set; }
        public string status { get; set; }
    }
    public class IPayPayoutResponse : IPayCommonResponse
    {
        public string timestamp { get; set; }
        public string ipay_uuid { get; set; }
        public string orderid { get; set; }
        public string environment { get; set; }
        public IPayPayoutRespData data { get; set; }
    }
    public class IPayPayoutRespData
    {
        public string ipay_id { get; set; }
        public string transfer_value { get; set; }
        public string type_pricing { get; set; }
        public string commercial_value { get; set; }
        public string value_tds { get; set; }
        public string ccf { get; set; }
        public string vendor_ccf { get; set; }
        public string charged_amt { get; set; }
        public IPayPayout payout { get; set; }
    }
    public class IPayPayout
    {
        public string credit_refid { get; set; }
        public string account { get; set; }
        public string ifsc { get; set; }
        public string name { get; set; }
    }
    public class IPayStatusCheckExternalResponse : IPayCommonResponse
    {
        public IPayStatusCheckExternalData data { get; set; }
    }
    public class IPayStatusCheckExternalData
    {
        public string transaction_dt { get; set; }
        public string external_id { get; set; }
        public string order_id { get; set; }
        public string serviceprovider_id { get; set; }
        public string product_key { get; set; }
        public string transaction_account { get; set; }
        public string transaction_amount { get; set; }
        public string transaction_status { get; set; }
        public string transaction_description { get; set; }
        public IPayAdditionalDetail additional_details { get; set; }
    }
    public class IPayAdditionalDetail
    {
        public string beneficiary_name { get; set; }
        public string account { get; set; }
        public string ifsc { get; set; }
    }

    public class IPaySetting
    {
        public string Token { get; set; }
        public string ClientID { get; set; }
        public string EncryptionKey { get; set; }
        public string ClientSecret { get; set; }
        public string ENV { get; set; }
        public string BaseURL { get; set; }
        public string AEPSBaseURL { get; set; }
        public string OnboardURL { get; set; }
        public string IPAddress { get; set; }
    }

    public class IPayDMRReq : IPayCommonResponse
    {
        public string token { get; set; }

        public IPayRequest request { get; set; }
    }

    public class IPayRequest
    {
        public string mobile { get; set; }
        public string name { get; set; }
        public string surname { get; set; }
        public string pincode { get; set; }
        public int outletid { get; set; }
        public string remitterid { get; set; }
        public string otp { get; set; }
        public string ifsc { get; set; }
        public string account { get; set; }
        public string beneficiaryid { get; set; }
        public string remittermobile { get; set; }
        public string agentid { get; set; }
        public string amount { get; set; }
        public string mode { get; set; }
    }


    public class IPayDMRResp : IPayCommonResponse
    {
        public IPayResponse data { get; set; }
    }


    public class IPBeneDelVeri
    {
        public string statuscode { get; set; }
        public string status { get; set; }
        public string data { get; set; }
        public string timestamp { get; set; }
        public string ipay_uuid { get; set; }
        public string orderid { get; set; }
        public string environment { get; set; }
    }

    public class IPayResponse
    {
        public int otp { get; set; }
        public string ipay_id { get; set; }
        public string ref_no { get; set; }
        public string opr_id { get; set; }
        public string name { get; set; }
        public string opening_bal { get; set; }
        public int amount { get; set; }
        public string charged_amt { get; set; }
        public int locked_amt { get; set; }
        public int ccf_bank { get; set; }
        public string bank_alias { get; set; }
        public Remitter remitter { get; set; }
        public List<Beneficiary> beneficiary { get; set; }
        public List<RemitterLimit> remitter_limit { get; set; }
    }

    public class Remitter
    {
        public string id { get; set; }
        public string name { get; set; }
        public string mobile { get; set; }
        public string address { get; set; }
        public string pincode { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string kycstatus { get; set; }
        public int consumedlimit { get; set; }
        public int remaininglimit { get; set; }
        public string kycdocs { get; set; }
        public int is_verified { get; set; }
        public int perm_txn_limit { get; set; }
    }

    public class Beneficiary
    {
        public string id { get; set; }
        public string name { get; set; }
        public string mobile { get; set; }
        public string account { get; set; }
        public string bank { get; set; }
        public string status { get; set; }
        public string last_success_date { get; set; }
        public string last_success_name { get; set; }
        public string last_success_imps { get; set; }
        public string ifsc { get; set; }
        public string imps { get; set; }
    }

    public class RemitterLimit
    {
        public string code { get; set; }
        public string status { get; set; }
        public Mode mode { get; set; }
        public Limit limit { get; set; }
    }
    public class Mode
    {
        public string imps { get; set; }
        public string neft { get; set; }
    }

    public class Limit
    {
        public string total { get; set; }
        public string consumed { get; set; }
        public string remaining { get; set; }
    }

    public class IPayCBResp : IPayCommonResponse
    {
        public CBResponse data { get; set; }
    }

    public class CBResponse
    {
        public Remitter remitter { get; set; }
        public Beneficiary beneficiary { get; set; }
    }




    public class InstantPayUO
    {
        public string mobile { get; set; }
        public string outletid { get; set; }
        public string email { get; set; }
        public string company { get; set; }
        public string name { get; set; }
        public string pan { get; set; }
        public string pincode { get; set; }
        public string address { get; set; }
        public string otp { get; set; }
        public string old_mobile { get; set; }
        public string new_mobile { get; set; }
        public string old_mobile_otp { get; set; }
        public string new_mobile_otp { get; set; }

        public string docId { get; set; }
        public string docLink { get; set; }
        public string fileName { get; set; }
    }

    public class InstantPayUOReq
    {
        public string token { get; set; }
        public InstantPayUO request { get; set; }
    }

    public class InstantPayUOResp
    {
        public string statuscode { get; set; }
        public string actcode { get; set; }
        public string status { get; set; }
        public string timestamp { get; set; }
        public string ipay_uuid { get; set; }
        public string orderid { get; set; }
        public string environment { get; set; }
    }
    public class IPSignupResponse : InstantPayUOResp
    {
        public IPResponseData data { get; set; }

    }
    public class IPAEPSResponse : InstantPayUOResp
    {
        public IPAEPSData data { get; set; }
    }
    
    public class IPMiniStatement {
        public string date { get; set; }
        public string txnType { get; set; }
        public string amount { get; set; }
        public string narration { get; set; }
    }
    public class IPAEPSData
    {
        public string opening_bal { get; set; }
        public string ipay_id { get; set; }
        public string amount { get; set; }
        public string amount_txn { get; set; }
        public string account_no { get; set; }
        public string txn_mode { get; set; }
        public string status { get; set; }
        public string opr_id { get; set; }
        public string balance { get; set; }
        public List<IPMiniStatement> mini_statement { get; set; }
    }
    public class IPResponseData
    {
        public string aadhaar { get; set; }
        public string otpReferenceID { get; set; }
        public string hash { get; set; }
    }
    public class IPSignupValidateResponse : InstantPayUOResp
    {
        public IRSignupValidateData data { get; set; }
    }
    public class IRSignupValidateData
    {
        public int outletId { get; set; }
        public string name { get; set; }
        public string dateOfBirth { get; set; }
        public string gender { get; set; }
        public string state { get; set; }
        public string districtName { get; set; }
        public string address { get; set; }
    }
    public class IPStsCheckResponse : InstantPayUOResp
    {
        public IPStatusData data { get; set; }
    }
    public class IPStatusData {
        public string transactionStatusCode { get; set; }
        public string transactionStatus { get; set; }
        public string transactionAmount { get; set; }
        public string transactionReferenceId { get; set; }
        public IPStatusOrder order { get; set; }
    }
    public class IPStatusOrder
    {
        public string refereceId { get; set; }
        public string externalRef { get; set; }
        public string spKey { get; set; }
        public string sspKey { get; set; }
        public string account { get; set; }
        public string optional1 { get; set; }
        public string optional2 { get; set; }
        public string optional3 { get; set; }
        public string optional4 { get; set; }
        public string optional5 { get; set; }
        public string optional6 { get; set; }
        public string optional7 { get; set; }
        public string optional8 { get; set; }
        public string optional9 { get; set; }
    }
}
