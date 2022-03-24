using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Model.Report
{
    public class DealerSummary
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int BalWise1 { get; set; }
        public int BalWise2 { get; set; }
    }
    public class BCAgentSummary
    {
        public int Total { get; set; }
        public int Approve { get; set; }
        public int Reject { get; set; }
        public int Pending { get; set; }
    }
}
