using Newtonsoft.Json;
using System.Collections.Generic;

namespace RoundpayFinTech.AppCode.ThirdParty.Mahagram
{
    /**
     * Note: In this API For AEPS and DMT services Onboarding same but for BBPS onboarding differ 
     * **/

    public class MGOnboardingReq
    {
        public string bc_f_name { get; set; }
        public string bc_m_name { get; set; }
        public string bc_l_name { get; set; }
        public string emailid { get; set; }
        public string phone1 { get; set; }
        public string phone2 { get; set; }
        public string bc_dob { get; set; }//Date of Birth (DD-MM-YYYY) in this Format
        public string bc_state { get; set; }//State ID as per the API Shared
        public string bc_district { get; set; }//District Id as per the API Shared
        public string bc_address { get; set; }
        public string bc_block { get; set; }
        public string bc_city { get; set; }
        public string bc_landmark { get; set; }
        public string bc_loc { get; set; }
        public string bc_mohhalla { get; set; }
        public string bc_pan { get; set; }
        public string bc_pincode { get; set; }
        public string shopname { get; set; }
        public string kyc1 { get; set; }//Identity Proof in datatype (BASE64 format not more than 200 KB)
        public string kyc2 { get; set; }//Address Proof in datatype  (BASE64 format not more than 200 KB)
        public string kyc3 { get; set; }//"Shop Photo in datatype (BASE64 format not more than 200 KB)
        public string kyc4 { get; set; }//Passport Size Photo in datatype (BASE64 format not more than 200 KB)
        public string saltkey { get; set; }
        public string secretkey { get; set; }
        public string shopType { get; set; }
        public string qualification { get; set; }
        public string population { get; set; }
        public string locationType { get; set; }
    }
    public class MGOnboardingResponse
    {
        public string emailid { get; set; }
        public string bc_id { get; set; }
        public string phone1 { get; set; }
        public string Message { get; set; }
        public string Statuscode { get; set; }
        public string status { get; set; }
        public string remarks { get; set; }
        public string Result { get; set; }
        public string bbps_Id { get; set; }
        public string agentid { get; set; }
        public string name { get; set; }
        public string contactperson { get; set; }
        public string mobile { get; set; }
        public string createdate { get; set; }
        public string Response_Message { get; set; }
        public string StatusCode { get; set; }
        public string BcCode { get; set; }
    }
    public class MGBCStatusRequest
    {
        public string bc_id { get; set; }
        public string saltkey { get; set; }
        public string secretkey { get; set; }
        public string cpid { get; set; }
    }
    public class MGBCGetCodeRequest
    {
        public string emailid { get; set; }
        public string phone1 { get; set; }
        public string saltkey { get; set; }
        public string secretkey { get; set; }
        public string cpid { get; set; }
    }
    public class MGInitiateRequest : MGBCStatusRequest
    {
        public string phone1 { get; set; }
        public string ip { get; set; }
        public int userid { get; set; }
    }
    public class MGDMTRequest
    {
        public string saltkey { get; set; }
        public string secretkey { get; set; }
        public string cust_f_name { get; set; }
        public string bc_id { get; set; }
        public string custno { get; set; }
        public string otp { get; set; }
        public string pincode { get; set; }
        public string cust_l_name { get; set; }
        public string Dob { get; set; }
        public string Address { get; set; }
        public string bankname { get; set; }
        public string beneaccno { get; set; }
        public string benemobile { get; set; }
        public string benename { get; set; }
        public string ifsc { get; set; }
        public string clientrefno { get; set; }
        public string amount { get; set; }
        public string StateCode { get; set; }
        public string Mhid { get; set; }
        public string FsessionId { get; set; }
    }
    public class MGDMTResponse
    {
        public string message { get; set; }
        public string statuscode { get; set; }
        public string custfirstname { get; set; }
        public string custlastname { get; set; }
        public string dob { get; set; }
        public string pincode { get; set; }
        public string address { get; set; }
        public string aeps_custname { get; set; }
        public string total_limit { get; set; }
        public string used_limit { get; set; }
        public string custmobile { get; set; }
        public string isairtellive { get; set; }
        public List<MGBeneficiary> Data { get; set; }
    }

    public class MGBeneficiary
    {
        public string id { get; set; }
        public string benename { get; set; }
        public string beneaccno { get; set; }
        public string benemobile { get; set; }
        public string bankname { get; set; }
        public string url { get; set; }
        public string bankid { get; set; }
        public string ifsc { get; set; }
        public string status { get; set; }
    }

    public class MGDMTTransactionResp
    {
        public string message { get; set; }
        public string statuscode { get; set; }
        public string availlimit { get; set; }
        public string total_limit { get; set; }
        public string used_limit { get; set; }
        public List<MGDMTTransData> Data { get; set; }
    }
    public class MGDMTTransData
    {
        public string fesessionid { get; set; }
        public string tranid { get; set; }
        public string rrn { get; set; }
        public string externalrefno { get; set; }
        public string amount { get; set; }
        public string responsetimestamp { get; set; }
        public string benename { get; set; }
        public string messagetext { get; set; }
        public string code { get; set; }
        public string errorcode { get; set; }
        public string mahatxnfee { get; set; }
    }
    public class MGPSARequest
    {
        public string securityKey { get; set; }
        public string createdby { get; set; }
        public string psaname { get; set; }
        public string contactperson { get; set; }
        public string location { get; set; }
        public string pincode { get; set; }
        public string state { get; set; }
        public string phone1 { get; set; }
        public string phone2 { get; set; }
        public string emailid { get; set; }
        public string pan { get; set; }
        public string dob { get; set; }
        public string adhaar { get; set; }
        public string udf1 { get; set; }
        public string udf2 { get; set; }
        public string udf3 { get; set; }
        public string udf4 { get; set; }
        public string udf5 { get; set; }
        public string psaid { get; set; }
        public string requestid { get; set; }
    }
    public class MGPSAResponse
    {
        public string Request { get; set; }
        public string RequestId { get; set; }
        public string psaid { get; set; }
        public string psaname { get; set; }
        public string contactperson { get; set; }
        public string location { get; set; }
        public string pincode { get; set; }
        public string state { get; set; }
        public string phone1 { get; set; }
        public string phone2 { get; set; }
        public string emailid { get; set; }
        public string pan { get; set; }
        public string dob { get; set; }
        public string adhaar { get; set; }
        public string udf1 { get; set; }
        public string udf2 { get; set; }
        public string udf3 { get; set; }
        public string udf4 { get; set; }
        public string udf5 { get; set; }
        public string createdby { get; set; }
        public string Status { get; set; }
        public string status { get; set; }
        public string Message { get; set; }
        public string StatusCode { get; set; }
    }
    public class MGCouponRequest
    {
        public string securityKey { get; set; }
        public string createdby { get; set; }
        public string requestid { get; set; }
        public string totalcoupon_physical { get; set; }
        public string psaid { get; set; }
        public string transactionid { get; set; }
        public string transactiondate { get; set; }
        public string totalcoupon_digital { get; set; }
        public string udf1 { get; set; }
        public string udf2 { get; set; }
        public string udf3 { get; set; }
        public string udf4 { get; set; }
        public string udf5 { get; set; }
    }
    public class MGCouponResponse
    {
        //UATInsCouponRequest
        public string RequestId { get; set; }
        [JsonProperty("Physical Coupon")]
        public string PhysicalCoupon { get; set; }
        [JsonProperty("Digital Coupon")]
        public string DigitalCoupon { get; set; }
        public string coupontopupdate { get; set; }
        [JsonProperty("Merchant Id")]
        public string MerchantId { get; set; }
        [JsonProperty("Reference Id")]
        public string ReferenceId { get; set; }
        public string psaid { get; set; }
        public string status { get; set; }
        public string udf1 { get; set; }
        public string udf2 { get; set; }
        public string udf3 { get; set; }
        public string udf4 { get; set; }
        public string udf5 { get; set; }
        public string Message { get; set; }
        public string StatusCode { get; set; }
        public string totalcoupon { get; set; }
    }
    public class MHMinATMStatuscheckRequest
    {
        public string secretekey { get; set; }
        public string saltkey { get; set; }
        public string referenceid { get; set; }
    }
    public class MGMiniATMStatuscheckResponse
    {
        public string message { get; set; }
        public string statuscode { get; set; }
        public List<MGMiniData> Data { get; set; }
    }
    public class MGMiniData
    {
        public string bcid { get; set; }
        public string bc_id { get; set; }
        public string bankname { get; set; }
        public string mid { get; set; }
        public string pid { get; set; }
        public string stanno { get; set; }
        public string stan_no { get; set; }
        public decimal amount { get; set; }
        public string createdate { get; set; }
        public string status { get; set; }
        public string bankmessage { get; set; }
        public string rrn { get; set; }
        public string cardno { get; set; }
        public string refunddate { get; set; }
        public string clientrefid { get; set; }
        public string udf1 { get; set; }
        public string udf2 { get; set; }
        public string udf3 { get; set; }
        public string remarks { get; set; }
    }

    public class MHAEPSStatuscheckRequest
    {
        public string Secretkey { get; set; }
        public string Saltkey { get; set; }
        public string stanno { get; set; }
    }
}
