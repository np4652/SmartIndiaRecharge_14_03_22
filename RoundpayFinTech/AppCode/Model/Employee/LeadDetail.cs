using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Model.Employee
{
    public class LeadDetail
    {
        public int LoginID { get; set; }
        public int LoginTypeID { get; set; }
        public int Id { get; set; }
        public string Message { get; set; }
        public string RequestStatus { get; set; }
        public string NextFollowupDate { get; set; }
        public string Remark { get; set; }
    }
}
