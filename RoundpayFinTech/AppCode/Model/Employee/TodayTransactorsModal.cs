using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Model.Employee
{
    public class TodayTransactorsModal
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string OutletName { get; set; }
        public string MobilleNo { get; set; }
        public int TransactionCount { get; set; }
        public decimal Amount { get; set; }
    }
}
