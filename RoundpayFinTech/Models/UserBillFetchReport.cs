using Microsoft.AspNetCore.Mvc;
using System;

namespace RoundpayFinTech.Models
{
    public class UserBillFetchReport
    {
        public int UserID { get; set; }
        public string Name { get; set; }
        public string MobileNo { get; set; }
        public int Success { get; set; }
        public int Failed { get; set; }
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public int APIID { get; set; }
        public int OPID { get; set; }
    }
}
