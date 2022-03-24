using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Model.Employee
{
    public class TodayLivePST
    {
        public string Type { get; set; }
        public decimal TotalAmount { get; set; }
        public int TotalUser{ get; set; }
        public int UniqueUser{ get; set; }
    }
}
