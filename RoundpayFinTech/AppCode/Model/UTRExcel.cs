using Fintech.AppCode.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Model
{
    public class UTRExcelMaster
    {
        public int FileId { get; set; }
        public string FileName { get; set; }
        public string EntryDate { get; set; }
        public bool IsJobDone { get; set; }
    }
    public class UTRExcel
    {
        public int Id { get; set; }
        public string UserIdentity { get; set; }
        public string UTR { get; set; }
        public string VirtualAccount { get; set; }
        public string Type { get; set; }
        public string ProcName { get; set; }
        public string CustomerCode { get; set; }
        public string CustomerAccountNumber { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; }
        public int FileId { get; set; }
    }

    public class UTRExcelReq : CommonReq
    {
        public List<UTRExcel> Record { get; set; }
    }
}
