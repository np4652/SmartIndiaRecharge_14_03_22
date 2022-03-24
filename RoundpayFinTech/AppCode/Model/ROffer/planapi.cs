using Fintech.AppCode.StaticModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Model.ROffer
{
    public class PlanApi
    {
        public string ERROR { get; set; }
        public string STATUS { get; set; }
        public string MOBILENO { get; set; }
        public List<RDataDetail> RDATA { get; set;}
        public string MESSAGE { get; set; }
    }
    public class RDataDetail
    {
        public string price { get; set; }
        public string commissionUnit { get; set; }
        public string ofrtext { get; set; }
        public string logdesc { get; set; }
        public string commissionAmount { get; set; }
    }

    public class PlanApiViewPlan
    {
        public string ERROR { get; set; }
        public string STATUS { get; set; }
        public string Operator { get; set; }
        public string Circle { get; set; }
        public RDataDet RDATA { get; set; }
        public string MESSAGE { get; set; }
    }

    public class RDataDet
    {
        public List<PlanDetail> DATA { get; set; }
        public List<PlanDetail> STV { get; set; }
        public List<PlanDetail> FULLTT { get; set; }
        public List<PlanDetail> TOPUP { get; set; }

        [JsonProperty("3G/4G")]
        public List<PlanDetail> ThreeG4G { get; set; }
        public List<PlanDetail> Romaing { get; set; }
        public List<PlanDetail> COMBO { get; set; }
        [JsonProperty("RATE CUTTER")]
        public List<PlanDetail> RATECUTTER { get; set; }
        [JsonProperty("2G")]
        public List<PlanDetail> TwoG { get; set; }
        public List<PlanDetail> SMS { get; set; }
        public List<PlanDetail> FRC { get; set; }
    }

    public class PlanDetail
    {
        public string rs { get; set; }
        public string desc { get; set; }
        public string validity { get; set; }
        public string last_update { get; set; }
    }

    public class DTHCustomerInfo
    {
        public string ERROR { get; set; }
        public CustData DATA { get; set; }
    }

    public class CustData
    {
        [JsonProperty("Name")]
        public string Name { get; set; }
        [JsonProperty("Monthly")]
        public string Monthly { get; set; }
        [JsonProperty("Next Recharge Date")]
        public string NextRechDate { get; set; }
        [JsonProperty("Plan")]
        public string Plan { get; set; }
        [JsonProperty("Balance")]
        public string Balance { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string District { get; set; }
        public string State { get; set; }
        [JsonProperty("PIN Code")]
        public string PinCode { get; set; }
    }

    public class DTHPlan
    {
        public string ERROR { get; set; }
        public _SubMplanDTHSimplePlanResp RDATA { get; set; }
    }
    public class MyPlanApiSimplePlan
    {
        public bool status { get; set; }
        public string msg { get; set; }
        public List<MyPlanApiResults> result { get; set; }
    }
    public class MyPlanApiResults
    {
        public int price { get; set; }
        public string description { get; set; }
        public string talktime { get; set; }
        public string validity { get; set; }
        public string sms { get; set; }
        public string data { get; set; }
    }

    public class MyPlanDTHCustomerInfo
    {
        public string status { get; set; }
        public string msg { get; set; }
        public List<RecordMyPlanDthInfo> records { get; set; }
        public resultMyPalanDthInfo result { get; set; }
    }
    public class RecordMyPlanDthInfo
    {
        public string balance { get; set; }
        public string customername { get; set; }
        public string nextrechargedate { get; set; }
        public string status { get; set; }
        public string planname { get; set; }
        public string monthlyrecharge { get; set; }
    }
    public class resultMyPalanDthInfo
    {
        public string tel { get; set; }
        public string operator2 { get; set; }
        public List<RecordMyPlanDthInfo> records { get; set; }
        public string status { get; set; }
        public string time { get; set; }
    }
    public class MyPlanApiDthplan
    {
        public bool status { get; set; }
        public string msg { get; set; }

        public Result result { get; set; }
    }
    public class Result
    {
        public Records records { get; set; }
        public int status { get; set; }
    }
    public class Records
    {
        public List<MyPlanDetail> plan { get; set; }
        [JsonProperty("add-onpack")]
        public List<MyPlanDetail> AddOnpack { get; set; }
    }
    public class MyPlanDetail
    {
        public Rs rs { get; set; }
        public string desc { get; set; }
        public string plan_name { get; set; }
        public string last_update { get; set; }
    }
    public class Rs
    {
        [JsonProperty("1 year")]
        public string _1year { get; set; }
        [JsonProperty("6 months")]
        public string _6months { get; set; }
        [JsonProperty("1 months")]
        public string _1months { get; set; }
        [JsonProperty("3 months")]
        public string _3months { get; set; }
    }
    public class MyPlanApi
    {

        public string status { get; set; }
        public string msg { get; set; }
        public List<RecordMyPlan> records { get; set; }
        public resultMYPLan result { get; set; }
    }
    public class resultMYPLan
    {
        public string tel { get; set; }

        public string operator1 { get; set; }
        public List<RecordMyPlan> records { get; set; }
        public string status { get; set; }
        public string time { get; set; }

    }
    public class RecordMyPlan
    {
        public string rs { get; set; }
        public string desc { get; set; }

    }

    public class MyPlanHavey
    {
        public string desc { get; set; }
        public string customername { get; set; }
        public int status { get; set; }

    }

    public class HaveyResult
    {
        public string tel { get; set; }
        [JsonProperty("operator")]
        public string Operator { get; set; }
        public MyPlanHavey records { get; set; }
        public int status { get; set; }
    }

    public class Root
    {
        public bool status { get; set; }
        public string msg { get; set; }
        public HaveyResult result { get; set; }
    }
}
