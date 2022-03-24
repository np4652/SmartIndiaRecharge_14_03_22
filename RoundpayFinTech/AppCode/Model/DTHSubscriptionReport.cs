using RoundpayFinTech.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Model
{
    public class DTHSubscriptionReport
    {
        public int ID { get; set; }
        public int TID { get; set; }
        public string TransactionID { get; set; }
        public string OutletName { get; set; }
        public string MobileNo { get; set; }
        public string Account { get; set; }
        public decimal Opening { get; set; }
        public decimal RequestedAmount { get; set; }
        public decimal Amount { get; set; }
        public decimal Balance { get; set; }
        public decimal Commission { get; set; }
        public int BookingStatus { get; set; }
        public string BookingStatus_ { get; set; }
        public int Status { get; set; }
        public string Status_ { get; set; }
        public string TechnicianName { get; set; }
        public string TechnicianMobile { get; set; }
        public string CustomerID { get; set; }
        public string STBID { get; set; }
        public string VCNO { get; set; }
        public string ApprovalTime { get; set; }
        public string InstallationTime { get; set; }
        public string InstalltionCharges { get; set; }
        public string CustomerNumber { get; set; }
        public string CustomerName { get; set; }
        public string Address { get; set; }
        public string Pincode { get; set; }
        public int PID { get; set; }
        public string PackageName { get; set; }        
        public string Operator { get; set; }
        public int OID { get; set; }
        public string API { get; set; }
        public string APIRequestID { get; set; }
        public string LiveID { get; set; }
        public string Remark { get; set; }
        public string EntryDate { get; set; }
        public string ModifyDate { get; set; }
        public string Gender { get; set; }
        public string Area { get; set; }
        public string City { get; set; }
        public string State { get; set; }

    }

    public class DTHSubscriptionModel : ReportModelCommon
    {
        public IEnumerable<DTHSubscriptionReport> Report { get; set; }
    }
}
