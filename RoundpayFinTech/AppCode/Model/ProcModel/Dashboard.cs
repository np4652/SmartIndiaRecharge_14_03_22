using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Model.ProcModel
{
    public class Dashboard
    {
        public List<Dashboard_Chart> Chart { get; set; }
    }
    public class DashboardData
    {
        public decimal Amount { get; set; }
        public int ServiceTypeID { get; set; }
        public string ServiceType { get; set; }
        public decimal OpeningBal { get; set; }
        public int RoleCount { get; set; }
    }
    public class Dashboard_Chart
    {
        public decimal MoveToBank { get; set; }
        public int MTBCount { get; set; }

        public int PCount { get; set; }
        public int KCount { get; set; }
        public int DmrPCount { get; set; }
        public decimal Dmr { get; set; }
        public decimal PAmount { get; set; }
        public int SCount { get; set; }
        public decimal SAmount { get; set; }
        public int FCount { get; set; }
        public decimal FAmount { get; set; }
        public decimal Dispute { get; set; }
        public decimal DMRDispute { get; set; }
        public int OpTypeID { get; set; }
        public string OpType { get; set; }
        public int DisputeCount { get; set; }
        public int DMRDisputeCount { get; set; }
    }
    public class PieChartList
    {
        public string Service { get; set; }
        public decimal Amount { get; set; }
    }
    public class MiddleUser
    {
        public string Role { get; set; }
        public int Status { get; set; }
        public int TranUser { get; set; }
    }
    public class MiddleLayerUser
    {
        public List<MiddleUser> Users { get; set; }
        public List<Dashboard_Chart> Chart { get; set; }
    }
}
