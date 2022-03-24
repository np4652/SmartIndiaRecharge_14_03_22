using Newtonsoft.Json;
using System.Collections.Generic;

namespace RoundpayFinTech.AppCode.Model.ROffer
{
    public class CyrusPlanAPI
    {
        public string tel { get; set; }
        public List<_PlanRecords> records { get; set; }
        public double time { get; set; }
        [JsonProperty("operator")]
        public string Operator { get; set; }
        public string status { get; set; }

    }

    public class _PlanRecords
    {
        public string rs { get; set; }
        public string desc { get; set; }
    }
}
