using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System.Collections.Generic;

namespace RoundpayFinTech.Models
{
    public class AdminTranModel
    {
        [JsonProperty("STCODE")]
        public string Statuscode { get; set; }
        [JsonProperty("STATUS")]
        public string Status { get; set; }
        [JsonProperty("FDATE")]
        public string Fdate { get; set; }
        [JsonProperty("TDATE")]
        public string Tdate { get; set; }
        [JsonProperty("TRNSUMMARY")]
        public IEnumerable<AdminTransactionSummary> TRNSUMMARY { get; set; }
    }
    public class AdminTransactionSummary
    {
        [JsonProperty("TOTALTRN")]
        public int TotalTran { get; set; }
        [JsonProperty("TOTALAMT")]
        public decimal TotalAmt { get; set; }
        [JsonProperty("MOBILENO")]
        public string MobileNo { get; set; }
    }
    public class TranxnSummaryReq
    {
        public string APIKey { get; set; }
        public string Req { get; set; }
        public string IP { get; set; }
        public string Browser { get; set; }
    }
    public class APIDetailModel
    {
        public APIDetail aPIDetail { get; set; }
        public SelectList selectLists { get; set; }
        public bool IsWLAPIAllowed { get; set; }
        public bool IsAdmin { get; set; }
        public int OpTypeID{ get; set; }
        public IEnumerable<APIDetail> APIs { get; set; }
        public IEnumerable<SlabCommission> SlabComs { get; set; }
        public IEnumerable<OpTypeMaster> OpTypes { get; set; }
        public IEnumerable<PriorityApiSwitch> SwitchedPAPIs { get; set; }
        public IEnumerable<APIOpCode> aPIOpCodes { get; set; }
    }
}
