using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace RoundpayFinTech.AppCode.Model.ROffer
{
    public class RPlanAPIType
    {
        public const string ROFFER = "ROFR";
        public const string SIMPLE = "SIMP";
        public const string DTHCustInfo = "DCUST";
        public const string DTHPlanSIMPLE = "DSIMP";
        public const string DTHPlanCHANNEL = "DCHNL";
        public const string DTHPlanLANGUAGE = "DLANG";
        public const string DTHHeavyRefresh = "DHREF";
        public const string DTHPlanForChRP = "DSIMPR";
    }
    public class ROffer
    {
        [JsonProperty("rs")]
        public string RS { get; set; }
        [JsonProperty("desc")]
        public string Desc { get; set; }
        public string msg { get; set; }
        public string yourip { get; set; }
    }

    public class SimplePlan : ROffer
    {
        [JsonProperty("validity")]
        public string Validity { get; set; }
        [JsonProperty("last_update")]
        public string LastUpdate { get; set; }
    }

    public class SubMplanRofferResp
    {
        [JsonProperty("tel")]
        public string Tel { get; set; }
        [JsonProperty("operator")]
        public string Operator { get; set; }
        [JsonProperty("records")]
        public List<ROffer> Records { get; set; }
    }

    public class SubMplanDTHCustomerInfo
    {
        [JsonProperty("tel")]
        public string Tel { get; set; }
        [JsonProperty("operator")]
        public string Operator { get; set; }
        [JsonProperty("records")]
        public List<MPlanDTHCustomerInfoRecords> Records { get; set; }
        [JsonProperty("status")]
        public int Status { get; set; }
    }
    public class MPlanDTHCustomerInfoRecords
    {
        public string MonthlyRecharge { get; set; }
        public string Balance { get; set; }
        [JsonProperty("customerName")]
        public string CustomerName { get; set; }
        [JsonProperty("status")]
        public string Status { get; set; }
        public string NextRechargeDate { get; set; }
        [JsonProperty("lastrechargedate")]
        public string Lastrechargedate { get; set; }
        [JsonProperty("lastrechargeamount")]
        public string Lastrechargeamount { get; set; }
        [JsonProperty("planname")]
        public string PlanName { get; set; }
        public string Rmn { get; set; }
    }
    public class SubMplanSimplePlanResp
    {
        [JsonProperty("status")]
        public int Status { get; set; }
        [JsonProperty("time")]
        public string Time { get; set; }
        [JsonProperty("records")]
        public _SubMplanSimpleRecords Records { get; set; }
    }
    public class ErrorRecords
    {
        [JsonProperty("rs")]
        public int RS { get; set; }
        [JsonProperty("desc")]
        public string Description { get; set; }
        [JsonProperty("status")]
        public int Status { get; set; }
    }
    public class _SubMplanSimpleRecords : ErrorRecords
    {
        [JsonProperty("FULLTT")]
        public List<SimplePlan> FullTT { get; set; }
        [JsonProperty("3G/4G")]
        public List<SimplePlan> ThreeGFourG { get; set; }
        [JsonProperty("RATE CUTTER")]
        public List<SimplePlan> RateCutter { get; set; }
        [JsonProperty("2G")]
        public List<SimplePlan> TwoG { get; set; }
        [JsonProperty("SMS")]
        public List<SimplePlan> SMS { get; set; }
        [JsonProperty("Romaing")]
        public List<SimplePlan> Roaming { get; set; }
        [JsonProperty("COMBO")]
        public List<SimplePlan> COMBO { get; set; }
        [JsonProperty("TOPUP")]
        public List<SimplePlan> TOPUP { get; set; }
    }
    public class SubMplanDTHSimplePlanResp
    {
        [JsonProperty("records")]
        public _SubMplanDTHSimplePlanResp Records { get; set; }
        [JsonProperty("status")]
        public int Status { get; set; }
    }
    public class _SubMplanDTHSimplePlanResp
    {
        public List<SubMplanDTHSimplePlan> Plan { get; set; }
        [JsonProperty("Add-On Pack")]
        public List<SubMplanDTHSimplePlan> AddOnPack { get; set; }
    }
    public class SubMplanDTHSimplePlan
    {
        [JsonProperty("desc")]
        public string Desc { get; set; }
        [JsonProperty("last_update")]
        public string LastUpdate { get; set; }
        [JsonProperty("plan_name")]
        public string PlanName { get; set; }
        [JsonProperty("rs")]
        public Rupees RS { get; set; }
    }
    public class RoundpayDTHHeavyRefresh
    {
        [JsonProperty("tel")]
        public string Tel { get; set; }
        [JsonProperty("operator")]
        public string Operator { get; set; }
        [JsonProperty("records")]
        public RPHeavyRefreshRecords Records { get; set; }
        [JsonProperty("status")]
        public int Status { get; set; }
    }

    public class RPHeavyRefreshRecords
    {
        [JsonProperty("status")]
        public int Status { get; set; }
        [JsonProperty("desc")]
        public string Desc { get; set; }
    }


    public class Rupees
    {
        [JsonProperty("1 MONTHS")]
        public string OneMonth { get; set; }
        [JsonProperty("3 MONTHS")]
        public string ThreeMonth { get; set; }
        [JsonProperty("6 MONTHS")]
        public string SixMonth { get; set; }
        [JsonProperty("9 MONTHS")]
        public string NineMonth { get; set; }
        [JsonProperty("1 YEAR")]
        public string OneYear { get; set; }
        [JsonProperty("5 YEAR")]
        public string FiveYear { get; set; }
    }

    /// <summary>
    /// For Mplan Heavy Refresh
    /// </summary>
    public class MplanDTHHeavyRefresh
    {
        [JsonProperty("tel")]
        public string Number { get; set; }
        [JsonProperty("operator")]
        public string Operator { get; set; }
        [JsonProperty("records")]
        public records Response { get; set; }
        [JsonProperty("status")]
        public string Status { get; set; }
    }

    public class records
    {
        public string status { get; set; }
        public string desc { get; set; }
    }

    #region RoundpayPlansSection
    public class RoundpaySimplePlanResp
    {
        [JsonProperty("status")]
        public int Status { get; set; }
        [JsonProperty("time")]
        public string Time { get; set; }
        [JsonProperty("records")]
        public PackageName Records { get; set; }
    }
    public class PackageName
    {
        public List<Stv> combo { get; set; }
        [JsonProperty("IUC Topup")]
        public List<Stv> IUCTopup { get; set; }
        [JsonProperty("NEW ALL-IN-ONE")]
        public List<Stv> NewAllinOne { get; set; }
        [JsonProperty("All in One")]
        public List<Stv> AllinOne { get; set; }
        public List<Stv> JioPhone { get; set; }
        public List<Stv> JioPrime { get; set; }
        [JsonProperty("Cricket Pack")]
        public List<Stv> CricketPack { get; set; }
        [JsonProperty("FRCNon-Prime")]
        public List<Stv> FRCNonPrime { get; set; }
        public List<Stv> ALL { get; set; }
        public List<Stv> unlimited { get; set; }
        public List<Stv> frcsrc { get; set; }
        [JsonProperty("smart recharge")]
        public List<Stv> smartrecharge { get; set; }
        public List<Stv> frc { get; set; }
        public List<Stv> isd { get; set; }
        public List<Stv> roaming { get; set; }
        public List<Stv> talktime { get; set; }
        public List<Stv> stv { get; set; }
        public List<Stv> sms { get; set; }
        [JsonProperty("Data Pack")]
        public List<Stv> DATAPACK { get; set; }
        public List<Stv> DATA { get; set; }
        [JsonProperty("international roaming")]
        public List<Stv> internationalroaming { get; set; }
        [JsonProperty("2g3g data")]
        public List<Stv> TwoG3G { get; set; }
        [JsonProperty("validity extension")]
        public List<Stv> validityextension { get; set; }
        public List<Stv> Validity { get; set; }
        [JsonProperty("combo vouchers")]
        public List<Stv> combovouchers { get; set; }
        [JsonProperty("data plans")]
        public List<Stv> dataplans { get; set; }
        [JsonProperty("unlimited plans")]
        public List<Stv> unlimitedplans { get; set; }
        [JsonProperty("data packs")]
        public List<Stv> datapacks { set { dataplans = value; } }
        [JsonProperty("mblaze stv")]
        public List<Stv> mblazestv { set { stv = value; } }
        [JsonProperty("mblaze ultra")]
        public List<Stv> mblazeultra { get; set; }
        [JsonProperty("wifi ultra recharges")]
        public List<Stv> wifiultrarecharges { get; set; }
        [JsonProperty("Work From Home")]
        public List<Stv> workfromhome { get; set; }
        [JsonProperty("All Rounder")]
        public List<Stv> AllRounder { get; set; }
        [JsonProperty("International")]
        public List<Stv> international { get; set; }
        [JsonProperty("2g")]
        public List<Stv> TwoG { get; set; }
        [JsonProperty("3g")]
        public List<Stv> ThreeG { get; set; }
        [JsonProperty("4g")]
        public List<Stv> FourG { get; set; }
        public List<Stv> Local { get; set; }
        public List<Stv> Other { get; set; }
        public List<Stv> VAS { get; set; }
        public List<Stv> HotStar { get; set; }


    }
    public class RoundPayMPlanPlanList
    {
        public List<Stv> frcsrc { get; set; }

        [JsonProperty("international roaming")]
        public List<Stv> internationalroaming { get; set; }
        [JsonProperty("isd pack")]
        public List<Stv> isdpack { get; set; }
        public List<Stv> sms { get; set; }
        public List<Stv> talktime { get; set; }
        public List<Stv> stv { get; set; }
        public List<Stv> data { get; set; }
        public List<Stv> smartrecharge { get; set; }
    }
    public class RoundPayMPlanPlan
    {
        public RoundPayMPlanPlanList records { get; set; }
        public string status { get; set; }
        //public string billing { get; set; }
    }
    public class _RoundpaySubMplanDTHSimplePlanResp
    {
        public string OpName { get; set; }
        public int RechargeAmount { get; set; }
        public string RechargeValidity { get; set; }
        public string RechargeType { get; set; }
        [System.ComponentModel.DataAnnotations.RegularExpression(@"^(?i)[A-Z0-9._%+-]+@[A-Z0-9]+.com")]
        public string Details { get; set; }
    }
    public class RoundpaySubMplanDTHSimplePlanResp
    {
        public List<_RoundpaySubMplanDTHSimplePlanResp> response { get; set; }
    }
    public class Stv
    {
        public string rs { get; set; }
        public string desc { get; set; }
        public string validity { get; set; }
    }

    public class RPDTHPlansSimpleOfPackages
    {
        public string Message { get; set; }
        public string Status { get; set; }
        [JsonProperty("response")]
        public List<RPDTHPlansResponse> Response { get; set; }
        [JsonProperty("package")]
        public List<RPDTHPlansPackage> Package { get; set; }
        [JsonProperty("languages")]
        public List<RPDTHPlansLanguages> Language { get; set; }
    }
    public class RPDTHPlansResponse
    {
        public string RechargeValidity { get; set; }
        public string RechargeType { get; set; }
        public string PackageId { get; set; }
        public string details { get; set; }
        public string rechargeAmount { get; set; }
        public string opName { get; set; }
        public string Channelcount { get; set; }

    }
    public class RPDTHPlansPackage
    {
        public string PackageId { get; set; }
        public string packagelanguage { get; set; }
        public string Channelcount { get; set; }
        public string RechargeValidity { get; set; }
        public string RechargeType { get; set; }
        public string details { get; set; }
        public string rechargeAmount { get; set; }
        public string opName { get; set; }
    }
    public class RPDTHPlansLanguages
    {
        public string Language { get; set; }
        public string Opname { get; set; }
        public string PackageCount { get; set; }
    }

    public class RPGetDTHChannelList
    {
        [JsonProperty("channels")]
        public List<RPChannels> Channels { get; set; }
        [JsonProperty("meta")]
        public RPMeta Meta { get; set; }
    }
    public class RPChannels
    {
        public string name { get; set; }
        public string genre { get; set; }
        public string logo { get; set; }

    }
    public class RPMeta
    {
        [JsonProperty("operator")]
        public string Operator { get; set; }
        public string circle { get; set; }
    }
    #endregion

    #region VastWeb
    public class VastWebRPlan
    {
        public string status { get; set; }
        public List<RofferList> Response { get; set; }

    }
    public class RofferList
    {
        public string price { get; set; }
        public string offer { get; set; }
        public string offerDetails { get; set; }
        public string commAmount { get; set; }
        public string commType { get; set; }
    }

    public class VastWebDTHCustInfo
    {
        public string status { get; set; }
        public DTHCustInfoList Response { get; set; }
    }

    public class DTHCustInfoList
    {
        public string SubsBalance { get; set; }
        public string ActivatedOn { get; set; }
        public string StatusName { get; set; }
        public string SubscriberName { get; set; }
        public string address { get; set; }
        public string State { get; set; }
        public string SchemeName { get; set; }
        public string PaytermName { get; set; }
        public string PackagePrice { get; set; }
    }

    public class ResponseVBHLR
    {
        public string Operator { get; set; }
        public string Circle { get; set; }
        public int product_id { get; set; }
        public bool status { get; set; }
        public bool postpaid { get; set; }
        public string source { get; set; }
    }

    public class VastWebHLRResp
    {
        public string status { get; set; }
        public ResponseVBHLR Response { get; set; }
    }
    #endregion



    public class HLRResponseStatus
    {
        public int Statuscode { get; set; }
        public string Msg { get; set; }
        public int CommonInt { get; set; }
        public int CommonInt2 { get; set; }
        public string CommonStr { get; set; }
        public string CommonStr2 { get; set; }
        public string CommonStr3 { get; set; }
        public string CommonStr4 { get; set; }
        public int CommonInt3 { get; set; }
        public bool CommonBool { get; set; }
        public bool CommonBool2 { get; set; }
        public int Status { get; set; }
        public List<HLRAPIDetails> HLRAPIs { get; set; }
    }

    public class HLRAPIDetails
    {
        public string APIURL { get; set; }
        public string APICode { get; set; }
        public int APIID { get; set; }
    }

    #region PLANSINFO
    #region Rechargeplans
    public class Datum
    {
        public string amount { get; set; }
        public string validity { get; set; }
        public string category { get; set; }
        public string benefit { get; set; }
    }

    public class PlansInfoRechPlanResp
    {
        public List<object> status { get; set; }
        public List<Datum> data { get; set; }
    }
    #endregion

    #region DTHplans

    public class PIPlan
    {
        public string package_name { get; set; }
        public string package_price { get; set; }
        public string package_price_3 { get; set; }
        public string package_price_6 { get; set; }
        public string package_price_12 { get; set; }
        public string package_description { get; set; }
        public string package_type { get; set; }
        public string package_language { get; set; }
        public string package_id { get; set; }
    }
    public class MetaPI
    {
        public string @operator { get; set; }
        public string circle { get; set; }
    }
    public class PlansInfoDTHP
    {
        public List<PIPlan> plans { get; set; }
        public MetaPI meta { get; set; }
    }


    #endregion

    #region ChannelList
    public class PIChannel
    {
        public string name { get; set; }
        public object genre { get; set; }
        public string logo { get; set; }
    }

    public class MetaPICH
    {
        public string @operator { get; set; }
        public string circle { get; set; }
    }

    public class PIChannelList
    {
        public List<PIChannel> channels { get; set; }
        public MetaPICH meta { get; set; }
    }

    #endregion

    #endregion

    #region RPNEWPLAN
    #region RNPRechargePlan
    public class RNPRechargePlans
    {
        public RNPPlanType plans { get; set; }
    }
    public class RNPPlanType
    {
        [JsonProperty("Top up")]
        public List<RNPDetails> TopUp { get; set; }
        public List<RNPDetails> Talktime { get; set; }
        public List<RNPDetails> SMS { get; set; }
        public List<RNPDetails> Local { get; set; }
        public List<RNPDetails> STD { get; set; }
        public List<RNPDetails> ISD { get; set; }
        public List<RNPDetails> Roaming { get; set; }
        public List<RNPDetails> Other { get; set; }
        public List<RNPDetails> Validity { get; set; }
        public List<RNPDetails> Plan { get; set; }
        public List<RNPDetails> FRC { get; set; }
        public List<RNPDetails> STV { get; set; }
        public List<RNPDetails> Unlimited { get; set; }
        public List<RNPDetails> Smart { get; set; }
        public List<RNPDetails> International { get; set; }
        public List<RNPDetails> HotStar { get; set; }
        [JsonProperty("FRC/ non - Prime")]
        public List<RNPDetails> FRCNonPrime { get; set; }
        [JsonProperty("Work From Home")]
        public List<RNPDetails> workfromhome { get; set; }
        public List<RNPDetails> Data { get; set; }
        public List<RNPDetails> JioPhone { get; set; }
        [JsonProperty("NEW ALL - IN - ONE")]
        public List<RNPDetails> NewAllinOne { get; set; }

        public List<RNPDetails> VAS { get; set; }
        [JsonProperty("All Rounder")]

        public List<RNPDetails> AllRounder { get; set; }
        [JsonProperty("Smart Recharge")]
        public List<RNPDetails> smartrecharge { get; set; }
        public List<RNPDetails> RATECUTTER { get; set; }
        public List<RNPDetails> FULLTT { get; set; }
        public List<RNPDetails> Combo { get; set; }
    }
    public class RNPDetails
    {
        public string rs { get; set; }
        public string desc { get; set; }
        public string validity { get; set; }
    }
    public class RNPRechPlansPanel
    {
        public string RechargePlanType { get; set; }
        public string RAmount { get; set; }
        public string Validity { get; set; }
        public string Details { get; set; }
        public string EntryDate { get; set; }
        public int OID { get; set; }
        public int CircleID { get; set; }
    }
    #endregion
    #region RNPDTHPlan
    public class RNPDTHPlans
    {
        public string Message { get; set; }
        public string Status { get; set; }
        [JsonProperty("response")]
        public List<RNPDTHPlansResponse> Response { get; set; }
        [JsonProperty("package")]
        public List<RNPDTHPlansPackage> Package { get; set; }
        [JsonProperty("languages")]
        public List<RNPDTHPlansLanguages> Language { get; set; }
    }
    public class RNPDTHPlansResponse
    {
        public string RechargeValidity { get; set; }
        public string RechargeType { get; set; }
        public int PackageId { get; set; }
        public string details { get; set; }
        public string rechargeAmount { get; set; }
        public string opName { get; set; }
        public int Channelcount { get; set; }
        public DateTime EntryDate { get; set; }

    }
    public class RNPDTHPlansPackage
    {
        public int PackageId { get; set; }
        public string packagelanguage { get; set; }
        public int Channelcount { get; set; }
        public string RechargeValidity { get; set; }
        public string RechargeType { get; set; }
        public string details { get; set; }
        public string rechargeAmount { get; set; }
        public string opName { get; set; }
    }
    public class RNPDTHPlansLanguages
    {
        public string Language { get; set; }
        public string Opname { get; set; }
        public int PackageCount { get; set; }
    }
    public class Price
    {
        public string monthly { get; set; }
        public string quarterly { get; set; }
        public string halfYearly { get; set; }
        public string yearly { get; set; }
    }
    public class PDetial
    {
        public string packageName { get; set; }
        public Price price { get; set; }
        public string pDescription { get; set; }
        public int packageId { get; set; }
        public string pLangauge { get; set; }
        public int pCount { get; set; }
        public int pChannelCount { get; set; }
    }
    public class DTHPlanFDB
    {
        public string pType { get; set; }
        public List<PDetial> pDetials { get; set; }
    }
    public class DTHPlanRespDB
    {
        public string _PackageName { get; set; }
        public string _PackagePrice { get; set; }
        public string _PackagePrice_3 { get; set; }
        public string _PackagePrice_6 { get; set; }
        public string _PackagePrice_12 { get; set; }
        public string _PackageDescription { get; set; }
        public string _PackageType { get; set; }
        public string _PackageLanguage { get; set; }
        public string _PackageId { get; set; }
        public string _pChannelCount { get; set; }
    }
    public class DTHChnlRespDB
    {
        public string name { get; set; }
        public string genre { get; set; }
        public string logo { get; set; }
    }
    #endregion
    #region RNPR-OFFER
    public class RNPRoffer 
    {
        public int StatusCode { get; set; }
        public string Msg { get; set; }
        public List<RNPRofferData> RofferData { get; set; }
    }
    public class RNPRofferData
    {
        public string Amount { get; set; }
        public string Description { get; set; }
    }
    #endregion
    #region DTHCustInfo
    public class RNPDTHCustInfo
    {
        public int StatusCode { get; set; }
        public string Msg { get; set; }
        public List<RNPDTHCustList> Data { get; set; }
    }
    public class RNPDTHCustList
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }
    #endregion
    #region DTHHeavyRefresh
    public class RNPDTHHeavyRefresh
    {
        public int StatusCode { get; set; }
        public string Msg { get; set; }
        public string AccountNo { get; set; }
        public string Operator { get; set; }
        public string Response { get; set; }
    }
    #endregion
    #endregion


    #region INFOAPIHLR
    public class InfoData
    {
        public int @operator { get; set; }
        public int state { get; set; }
    }

    public class InfoAPIHLRResp
    {
        public string status { get; set; }
        public string message { get; set; }
        public string cust { get; set; }
        public InfoData data { get; set; }
    }
    #endregion

    #region AirtelPPHLR
    public class AirtelPPHLRResp
    {
        public bool success { get; set; }
        public string desc { get; set; }
    }
    #endregion

    public class AppResult
    {
        [JsonProperty("records")]
        public _SubMplanDTHSimplePlanResp Records { get; set; }
        public int status { get; set; }
    }
    public class AppMyPlanApiDthplan
    {
        public bool status { get; set; }
        public string msg { get; set; }

        public AppResult result { get; set; }
    }
}
