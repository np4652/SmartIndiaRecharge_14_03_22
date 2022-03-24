using System.Collections.Generic;

namespace RoundpayFinTech.AppCode.ThirdParty.Roundpay
{
    public class OutletRegRespRoundpayModel
    {

        public string STATUS {get;set;}
        public string OUTLET_STATUS {get;set;}
        public string KYC_STATUS {get;set;}
        public string MESSAGE  {get;set;}
        public string OutletId {get;set;}

    }

    public class ServicePlus
    {
        public string STATUS { get; set; }
        public string MESSAGE { get; set; }
        public string KEY { get; set; }
        public string OTPSTATUS { get; set; }
        public string PSAID { get; set; }
        public string REQUESTID { get; set; }
        public string PSASTATUS { get; set; }
    }

    public static class RndKYCOutLetStatus
    {
        public const string NotApplied = "Not Applied";
        public const string Approved = "Approved";
        public const string Rejected = "Rejected";
        public const string Pending = "Pending";

        public const int _NotApplied = 0;
        public const int _Approved = 2;
        public const int _Rejected = 3;
        public const int _Pending = 1;
    }

    public class CheckStatusAtRoundpay
    {
        public string MESSAGE { get; set; }
        public string STATUS { get; set; }
        public string AEPS { get; set; }
        public string Outlet { get; set; }
        public string PSA { get; set; }
        public string PsaId { get; set; }
        public string Panrequestid { get; set; }
        public string BBPS { get; set; }
        public string DMT { get; set; }
        public string KYC { get; set; }
        public string PSARemark { get; set; }
        public string OutletRemark { get; set; }
        public string AEPSRemark { get; set; }
        public string BBPSRemark { get; set; }
        public string DocumentRemark { get; set; }
        public string OutletId { get; set; }

    } 

    public class RoundpayApiRequestModel
    {
        public string BaseUrl { get; set; }
        public string Token { get; set; }
        public string Scode { get; set; }
        public string Apiopcode { get; set; }
        public int APIID { get; set; }
        public string APIOutletID { get; set; }
        public string ServicePlus { get; set; }
        public string OTP { get; set; }
        public string  Name { get; set; }
        public string LastName { get; set; }
        public string DOB { get; set; }
        public string Pincode { get; set; }
        public string Address { get; set; }
        public string Landmark { get; set; }
        public string Area { get; set; }
        public string Phone1 { get; set; }
        public string Phone2 { get; set; }
        public string Emailid { get; set; }
        public string Pan { get; set; }
        public string PANLink { get; set; }
        public string Aadhaar { get; set; }
        public string AadharLink { get; set; }
        public string ShopType { get; set; }
        public string ShopLink { get; set; }
        public string Qualification { get; set; }
        public string Poupulation { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public string AreaType { get; set; }
        public int StateId  { get; set; }
        public int DistrictId { get; set; }
        public string totalcoupon { get; set; }
        public string psaid { get; set; }
        public string agentid { get; set; }
    }
    public class PANResponseRoundpay
    {
        public string RESPONSESTATUS { get; set; }
        public string MESSAGE { get; set; }
        public string REQUESTID { get; set; }
    }

    public class BCResponse
    {
        public string RESPONSESTATUS { get; set; }
        public string MESSAGE { get; set; }
        public List<_BCResponse> Table { get; set; }
        public int Statuscode { get; set; }
        public string Msg { get; set; }
        public int ErrorCode { get; set; }
    }

    public class _BCResponse
    {
        public int ErrorCode { get; set; }
        public string Mobileno { get; set; }
        public string SecretKey { get; set; }
        public string SaltKey { get; set; }
        public string CPID { get; set; }
        public string AepsOutletId { get; set; }
        public string EmailId { get; set; }
        public string UserId { get; set; }
        public string BCID { get; set; }
        public string MerchantId { get; set; }
        public string Password { get; set; }
    }
    public class GenerateTokenResponseRP 
    {
        public string UserCode { get; set; }
        public string ServiceCode { get; set; }
        public string tokenId { get; set; }
        public string Status { get; set; }
        public string Description { get; set; }
    }
}
