using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Model.ProcModel
{
    public class ProcRecentTransactionCounts
    {
        public string EntryDate { get; set; }
        public int Value { get; set; }
        public float ValueFloat { get; set; }
        public float _AmountLMTD { get; set; }
        public int LMTDCount { get; set; }
        public float _AmountTillDate { get; set; }
        public int TillDateCount { get; set; }
        public float _AmountLastDay { get; set; }
        public int _LastDayCount { get; set; }
        public float _AmountCurrentDay { get; set; }
        public int _CurrentDayCount { get; set; }
        public decimal LastDay_Current_Diff { get; set; }
        public decimal LMTD_MTD_Diff { get; set; }
        //public int ServiceRenderIndex { get; set; }

        public string Operator { get; set; }
        public int OID { get; set; }
        public string Account { get; set; }
        public string Status { get; set; }
    }

    public class MostUsedServices
    {
        public int UserID { get; set; }
        public int ServiceID { get; set; }
        public int ServiceRenderIndex { get; set; }
    }

    public class AccountSummaryTable
    {
        public float LastPriTransactionAmount { get; set; }
        public string LastPriTransactionDate { get; set; }
        public float LastSecTransactionAmount { get; set; }
        public string LastSecTransactionDate { get; set; }
        public float LastTerTransactionAmount { get; set; }
        public string LastTerTransactionDate { get; set; }
    }
}
